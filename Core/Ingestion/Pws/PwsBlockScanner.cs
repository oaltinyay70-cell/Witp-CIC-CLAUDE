// ROCHEFORT 6000 // PACIFIC CIC
// Core/Ingestion/Pws/PwsBlockScanner.cs
//
// Robust zlib-magic scanner for .pws PBEM save blocks.
// Uses SharpZipLib Inflater (NOT .NET ZLibStream) because ZLibStream reads ahead
// through an internal buffer, making Position unreliable for block-boundary detection.
//
// Architecture: block id = (id_and_flags & 0xFF) + 512, range 513..593.
// Bit 0x10000 in id_and_flags means len_a/len_b are garbled — don't trust them.
// Strategy: scan for 0x78 zlib magic bytes, inflate trial, accept on clean finish.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace ROCHEFORT6000.Core.Ingestion.Pws
{
    /// <summary>
    /// One decoded block from a .pws file (GZip outer already removed by caller).
    /// </summary>
    public sealed class PwsBlock
    {
        /// <summary>Block identifier in the range 513..593.</summary>
        public int BlockId { get; init; }

        /// <summary>Raw id_and_flags word from the 12-byte header.</summary>
        public uint IdAndFlags { get; init; }

        /// <summary>Whether bit 0x10000 was set (garbled length fields).</summary>
        public bool HasGarbledLengths => (IdAndFlags & 0x10000u) != 0;

        /// <summary>Byte offset of the 12-byte header inside the decompressed outer buffer.</summary>
        public int HeaderOffset { get; init; }

        /// <summary>Decompressed payload bytes.</summary>
        public byte[] Payload { get; init; } = Array.Empty<byte>();

        /// <summary>Compressed bytes consumed (equals the zlib stream size on disk).</summary>
        public int CompressedSize { get; init; }
    }

    /// <summary>
    /// Scans the decompressed body of a .pws file (GZip outer already removed)
    /// and yields all valid zlib blocks in order.
    ///
    /// Algorithm: Walk byte-by-byte looking for 0x78 magic (zlib CMF byte).
    /// At each candidate at position p (p >= 12), attempt Inflater.Inflate().
    /// Accept only streams that reach a proper IsFinished end.
    /// Read the 12-byte header from buf[p-12..p].
    /// </summary>
    public static class PwsBlockScanner
    {
        // zlib CM+FLG second bytes that pair with 0x78
        private static readonly HashSet<byte> ZlibSecondBytes = new HashSet<byte> { 0x01, 0x5E, 0x9C, 0xDA };

        /// <summary>
        /// Decompresses the outer GZip layer and returns the raw inner buffer.
        /// </summary>
        public static byte[] DecompressOuter(string filePath)
        {
            using var fs = File.OpenRead(filePath);
            using var gz = new GZipStream(fs, CompressionMode.Decompress);
            using var ms = new MemoryStream();
            gz.CopyTo(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Scans a raw (GZip-decompressed) buffer and yields all valid PwsBlocks.
        /// Caller provides the buffer; this method is pure and side-effect-free.
        /// </summary>
        public static IEnumerable<PwsBlock> ScanBlocks(byte[] buf)
        {
            // We need at least 12 bytes for a header + 2 bytes for zlib magic
            int limit = buf.Length - 2;

            // p is the candidate position of the 0x78 zlib magic byte
            int p = 12; // minimum: need 12 bytes before p for the header
            while (p < limit)
            {
                if (buf[p] != 0x78 || !ZlibSecondBytes.Contains(buf[p + 1]))
                {
                    p++;
                    continue;
                }

                // Candidate zlib stream starts at p. Try to inflate.
                (byte[]? payload, int compressedSize) = TryInflate(buf, p);
                if (payload == null)
                {
                    p++;
                    continue;
                }

                // Success — read the 12-byte header from buf[p-12..p]
                int headerOffset = p - 12;
                uint idAndFlags = BitConverter.ToUInt32(buf, headerOffset);
                // Block id = (idAndFlags & 0xFF) + 512
                int blockId = (int)(idAndFlags & 0xFFu) + 512;

                yield return new PwsBlock
                {
                    BlockId = blockId,
                    IdAndFlags = idAndFlags,
                    HeaderOffset = headerOffset,
                    Payload = payload,
                    CompressedSize = compressedSize
                };

                // Advance past the consumed compressed bytes
                p += compressedSize;
            }
        }

        /// <summary>
        /// Attempts to inflate starting at <paramref name="startOffset"/> in <paramref name="buf"/>.
        /// Returns (decompressedPayload, compressedBytesConsumed) on success, (null, 0) on failure.
        /// Uses SharpZipLib Inflater so we get the exact TotalIn byte count.
        /// </summary>
        private static (byte[]? payload, int compressedSize) TryInflate(byte[] buf, int startOffset)
        {
            try
            {
                var inf = new Inflater(noHeader: false); // zlib header present
                inf.SetInput(buf, startOffset, buf.Length - startOffset);

                using var ms = new MemoryStream();
                var tmp = new byte[1 << 20]; // 1 MB chunks

                while (!inf.IsFinished)
                {
                    int n = inf.Inflate(tmp);
                    if (n > 0)
                    {
                        ms.Write(tmp, 0, n);
                    }
                    else if (inf.IsNeedingInput || inf.IsNeedingDictionary)
                    {
                        // Not a real zlib block — needs more input but we gave it the whole rest
                        return (null, 0);
                    }
                    else if (n == 0)
                    {
                        // Inflater returned 0 without finishing — stalled, abort
                        break;
                    }
                }

                if (!inf.IsFinished)
                    return (null, 0);

                // TotalIn is the exact number of compressed bytes consumed from SetInput
                int consumed = (int)inf.TotalIn;
                if (consumed <= 0)
                    return (null, 0);

                return (ms.ToArray(), consumed);
            }
            catch
            {
                return (null, 0);
            }
        }
    }
}
