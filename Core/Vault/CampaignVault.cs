// ROCHEFORT 6000 // PACIFIC CIC
// Core/Vault/CampaignVault.cs
//
// The campaign vault: ONE ZIP file per campaign with the .wic extension.
//   manifest.json                     campaign name, side, campaign .pws, source folders, schema version, ingested-file ledger
//   reports/<YYMMDD>/<type>.txt       raw report text exactly as the game wrote it
//   pws/<YYMMDD>.json                 decoded ALLIED-ONLY snapshot of the campaign save for that turn (never the raw .pws)
//   *.prevN.*                         superseded versions - history is never deleted
//
// Rules: offline, read-only toward the game folders, Allied data only, never lose history.
// The vault is written to a temp file and swapped in atomically; the previous vault is kept as <name>.wic.bak.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ROCHEFORT6000.Core.Ingestion;
using ROCHEFORT6000.Core.Ingestion.Pws;

namespace ROCHEFORT6000.Core.Vault
{
    public sealed class LedgerEntry
    {
        public string Name { get; set; } = string.Empty;       // original file name
        public string Type { get; set; } = string.Empty;       // OPERATIONS, SIGINT, COMBAT_REPORT, COMBAT_EVENTS, PWS_SAVE
        public string Date { get; set; } = string.Empty;       // YYMMDD game date
        public long   Size { get; set; }
        public string Hash { get; set; } = string.Empty;       // SHA-256 of the source file
        public string IngestedUtc { get; set; } = string.Empty;
        public string StoredAs { get; set; } = string.Empty;   // entry path inside the vault
        public bool   Superseded { get; set; }                 // replaced by a newer version of the same type+date
    }

    public sealed class CampaignManifest
    {
        public int SchemaVersion { get; set; } = CampaignVault.CurrentSchemaVersion;
        public string CampaignName { get; set; } = string.Empty;
        public string Side { get; set; } = "Allied";
        public string CampaignPws { get; set; } = string.Empty;   // full path of the one .pws that belongs to this campaign
        public List<string> SourceFolders { get; set; } = new();
        public string CreatedUtc { get; set; } = string.Empty;
        public string UpdatedUtc { get; set; } = string.Empty;
        public List<LedgerEntry> Ledger { get; set; } = new();
    }

    public sealed class VaultIngestResult
    {
        public int NewReports { get; set; }
        public int NewSnapshots { get; set; }
        public int Superseded { get; set; }
        public int Unchanged { get; set; }
        public int SkippedUndated { get; set; }
        public Dictionary<string, int> NewByType { get; } = new();
        public List<string> Warnings { get; } = new();
        public int TotalNew => NewReports + NewSnapshots;
    }

    public sealed class CampaignVault
    {
        public const int CurrentSchemaVersion = 1;
        public const string Extension = ".wic";

        private static readonly Regex SnapshotKey = new(@"^pws/(\d{6})\.json$", RegexOptions.Compiled);
        private static readonly Regex ReportKey   = new(@"^reports/(\d{6})/([a-z_]+)\.txt$", RegexOptions.Compiled);

        private static readonly JsonSerializerOptions ManifestJson = new()
        {
            WriteIndented = true
        };

        private static readonly JsonSerializerOptions SnapshotJson = new()
        {
            IncludeFields = true,
            PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly Dictionary<string, byte[]> _entries = new(StringComparer.Ordinal);

        public string FilePath { get; private set; }
        public CampaignManifest Manifest { get; private set; }

        private CampaignVault(string path, CampaignManifest manifest)
        {
            FilePath = path;
            Manifest = manifest;
        }

        // ── Create / open / save ─────────────────────────────────────────────

        public static string PathFor(string vaultDir, string campaignName)
        {
            var bad = Path.GetInvalidFileNameChars();
            var clean = new string((campaignName ?? string.Empty).Trim().Select(c => bad.Contains(c) ? '_' : c).ToArray());
            if (clean.Length == 0) clean = "campaign";
            return Path.Combine(vaultDir, clean + Extension);
        }

        public static CampaignVault Create(string path, string campaignName, IEnumerable<string> sourceFolders, string? campaignPws)
        {
            if (File.Exists(path)) throw new IOException($"Vault already exists: {path}");
            string now = DateTime.UtcNow.ToString("o");
            var m = new CampaignManifest
            {
                CampaignName = campaignName,
                Side = "Allied",
                CampaignPws = campaignPws ?? string.Empty,
                SourceFolders = sourceFolders.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                CreatedUtc = now,
                UpdatedUtc = now
            };
            var v = new CampaignVault(path, m);
            v.Save();
            return v;
        }

        public static CampaignVault Open(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var zip = new ZipArchive(fs, ZipArchiveMode.Read);

            var mEntry = zip.GetEntry("manifest.json") ?? throw new InvalidDataException("Vault has no manifest.json");
            CampaignManifest manifest;
            using (var ms = mEntry.Open())
                manifest = JsonSerializer.Deserialize<CampaignManifest>(ms, ManifestJson) ?? throw new InvalidDataException("Unreadable manifest.json");

            if (manifest.SchemaVersion > CurrentSchemaVersion)
                throw new InvalidDataException($"Vault schema {manifest.SchemaVersion} is newer than this program supports ({CurrentSchemaVersion}).");

            var vault = new CampaignVault(path, manifest);
            foreach (var e in zip.Entries)
            {
                if (e.FullName == "manifest.json" || e.FullName.EndsWith("/")) continue;
                using var s = e.Open();
                using var buf = new MemoryStream();
                s.CopyTo(buf);
                vault._entries[e.FullName] = buf.ToArray();
            }
            return vault;
        }

        public void Save()
        {
            Manifest.UpdatedUtc = DateTime.UtcNow.ToString("o");
            string dir = Path.GetDirectoryName(Path.GetFullPath(FilePath)) ?? ".";
            Directory.CreateDirectory(dir);
            string tmp = FilePath + ".tmp";

            using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                var me = zip.CreateEntry("manifest.json", CompressionLevel.Optimal);
                using (var ms = me.Open())
                    JsonSerializer.Serialize(ms, Manifest, ManifestJson);

                foreach (var kv in _entries.OrderBy(k => k.Key, StringComparer.Ordinal))
                {
                    var ze = zip.CreateEntry(kv.Key, CompressionLevel.Optimal);
                    using var zs = ze.Open();
                    zs.Write(kv.Value, 0, kv.Value.Length);
                }
            }

            if (File.Exists(FilePath))
                File.Replace(tmp, FilePath, FilePath + ".bak");
            else
                File.Move(tmp, FilePath);
        }

        // ── Ingest ───────────────────────────────────────────────────────────

        /// <summary>
        /// Adds every scanned file that is not yet in the ledger (same type + date + content hash).
        /// A changed file for an already-stored type+date supersedes the old copy, which is kept as .prevN.
        /// Does not write the vault file; call Save().
        /// </summary>
        public VaultIngestResult Ingest(IngestionManifest scanned)
        {
            var result = new VaultIngestResult();
            foreach (var f in scanned.Files)
            {
                if (string.IsNullOrEmpty(f.DateYYMMDD)) { result.SkippedUndated++; continue; }

                byte[] bytes;
                try { bytes = File.ReadAllBytes(f.FullPath); }
                catch (Exception ex) { result.Warnings.Add($"{f.FileName}: unreadable ({ex.Message})"); continue; }

                string hash = Convert.ToHexString(SHA256.HashData(bytes));
                var existing = Manifest.Ledger.FirstOrDefault(e => !e.Superseded && e.Type == f.FileType && e.Date == f.DateYYMMDD);
                if (existing != null && existing.Hash == hash) { result.Unchanged++; continue; }

                string key;
                byte[] payload;
                bool isPws = f.FileType == "PWS_SAVE";
                if (isPws)
                {
                    PwsSnapshot snap;
                    try { snap = PwsFileParser.Parse(f.FullPath, f.DateYYMMDD); }
                    catch (Exception ex) { result.Warnings.Add($"{f.FileName}: PWS parse failed ({ex.Message})"); continue; }

                    // Faction isolation: the vault never stores anything but Allied records.
                    snap.AlliedShips.RemoveAll(s => s.Side != 1);
                    snap.AlliedTaskForces.RemoveAll(t => t.Side != 1);
                    snap.AlliedLeaders.RemoveAll(l => l.Side != 1);

                    key = $"pws/{f.DateYYMMDD}.json";
                    payload = JsonSerializer.SerializeToUtf8Bytes(snap, SnapshotJson);
                }
                else
                {
                    key = $"reports/{f.DateYYMMDD}/{f.FileType.ToLowerInvariant()}.txt";
                    payload = bytes;
                }

                if (existing != null)
                {
                    existing.Superseded = true;
                    if (_entries.TryGetValue(key, out var old))
                    {
                        string prev = NextPrevKey(key);
                        _entries[prev] = old;
                        existing.StoredAs = prev;
                    }
                    result.Superseded++;
                }

                _entries[key] = payload;
                Manifest.Ledger.Add(new LedgerEntry
                {
                    Name = f.FileName,
                    Type = f.FileType,
                    Date = f.DateYYMMDD,
                    Size = bytes.LongLength,
                    Hash = hash,
                    IngestedUtc = DateTime.UtcNow.ToString("o"),
                    StoredAs = key
                });
                if (isPws) result.NewSnapshots++; else result.NewReports++;
                result.NewByType[f.FileType] = result.NewByType.GetValueOrDefault(f.FileType) + 1;
            }
            return result;
        }

        private string NextPrevKey(string key)
        {
            int dot = key.LastIndexOf('.');
            string stem = key.Substring(0, dot), ext = key.Substring(dot);
            for (int n = 1; ; n++)
            {
                string k = $"{stem}.prev{n}{ext}";
                if (!_entries.ContainsKey(k)) return k;
            }
        }

        // ── Read back ────────────────────────────────────────────────────────

        public IReadOnlyList<string> SnapshotDates =>
            _entries.Keys.Select(k => SnapshotKey.Match(k)).Where(m => m.Success).Select(m => m.Groups[1].Value).OrderBy(d => d, StringComparer.Ordinal).ToList();

        public IReadOnlyList<string> ReportDates =>
            _entries.Keys.Select(k => ReportKey.Match(k)).Where(m => m.Success).Select(m => m.Groups[1].Value).Distinct().OrderBy(d => d, StringComparer.Ordinal).ToList();

        public string? GetReportText(string dateYYMMDD, string fileType)
        {
            return _entries.TryGetValue($"reports/{dateYYMMDD}/{fileType.ToLowerInvariant()}.txt", out var b)
                ? Encoding.UTF8.GetString(b) : null;
        }

        public PwsSnapshot? LoadSnapshot(string dateYYMMDD)
        {
            return _entries.TryGetValue($"pws/{dateYYMMDD}.json", out var b)
                ? JsonSerializer.Deserialize<PwsSnapshot>(b, SnapshotJson) : null;
        }

        /// <summary>All stored snapshots, oldest first.</summary>
        public IEnumerable<PwsSnapshot> LoadSnapshots()
        {
            foreach (var d in SnapshotDates)
            {
                var s = LoadSnapshot(d);
                if (s != null) yield return s;
            }
        }

        /// <summary>
        /// Extracts the stored reports into cacheDir and returns a manifest pointing at them,
        /// so the existing parsers can read vault content exactly like game files.
        /// Snapshots are not materialised; use LoadSnapshots().
        /// </summary>
        public IngestionManifest MaterializeManifest(string cacheDir)
        {
            Directory.CreateDirectory(cacheDir);
            var manifest = new IngestionManifest();
            foreach (var key in _entries.Keys.OrderBy(k => k, StringComparer.Ordinal))
            {
                var m = ReportKey.Match(key);
                if (!m.Success) continue;
                string date = m.Groups[1].Value;
                string type = m.Groups[2].Value.ToUpperInvariant();
                string name = $"{type.ToLowerInvariant()}_{date}.txt";
                string full = Path.Combine(cacheDir, name);
                var bytes = _entries[key];
                if (!File.Exists(full) || new FileInfo(full).Length != bytes.Length)
                    File.WriteAllBytes(full, bytes);
                manifest.Files.Add(new DiscoveredFile { FileName = name, FullPath = full, FileType = type, DateYYMMDD = date });
                manifest.UniqueDates.Add(date);
            }
            foreach (var d in SnapshotDates)
            {
                // Snapshots are restored from the vault, not from a file: FullPath stays empty.
                manifest.Files.Add(new DiscoveredFile { FileName = $"{d}.json", FullPath = string.Empty, FileType = "PWS_SAVE", DateYYMMDD = d });
                manifest.UniqueDates.Add(d);
            }

            manifest.DaysProcessed = manifest.UniqueDates.Count;
            if (manifest.UniqueDates.Count > 0)
            {
                manifest.LatestDate = manifest.UniqueDates.Max()!;
                manifest.FormattedDate = FileScanner.FormatYYMMDD(manifest.LatestDate);
            }
            return manifest;
        }
    }
}
