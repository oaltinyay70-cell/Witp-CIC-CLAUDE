// ROCHEFORT 6000 // PACIFIC CIC
// Core/Ingestion/Pws/PwsFileParser.cs
//
// Orchestrates PwsBlockScanner → per-block payload decoders → PwsSnapshot.
// All decoding is Allied-only (faction-isolation-rule). Japanese records are
// skipped at the decode stage and are never placed into any output structure.
//
// Block ID mapping (WitP:AE 1.8.11.28.4):
//   513        : alternate ship table (same format as 520 — use whichever is found first)
//   520        : ship records, 464 bytes each × up to 20000
//   530/531    : location records, 1172 bytes each × up to 18000
//   533        : leader records, 180 bytes each × up to 50000
//   539        : ship class records, 380 bytes each × up to 5000
//   550        : game header / metadata (text)
//   565        : AI scenario scripts — SKIP (not used in 2-player PBEM)
//   582        : task force records, 556 bytes each × up to 4000
//   593        : turn reports (4 Latin1 sections with int32 length prefixes)

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ROCHEFORT6000.Core.Ingestion.Pws
{
    public static class PwsFileParser
    {
        // ─── Ship block (520/513): stride 464 bytes ────────────────────────────
        private const int ShipStride          = 464;
        private const int ShipNameLength      = 25;
        private const int ShipNationOffset    = 25;  // u8 — 4 = USA, 2 = Japan, etc.
        private const int ShipSideOffset      = 26;  // u8 — 0 = Japanese, 1 = Allied
        private const int ShipStatusOffset    = 36;  // u16 — 19999 = sunk
        private const int ShipClassOffset     = 44;  // u16
        private const int ShipTFOffset        = 62;  // u16 — task force index
        private const int ShipFuelOffset      = 120; // u16
        private const int ShipArrivalDaysOff  = 86;  // u16
        private const int ShipArrivalLocOff   = 88;  // u16
        private const int ShipHexXOffset      = 352; // u16
        private const int ShipHexYOffset      = 354; // u16

        // ─── Task Force block (582): stride 556 bytes ─────────────────────────
        private const int TFStride             = 556;
        private const int TFFlagshipOffset     = 0;   // u16
        private const int TFSideOffset         = 4;   // u8
        private const int TFMissionOffset      = 6;   // u8
        private const int TFHexXOffset         = 8;   // u16
        private const int TFHexYOffset         = 10;  // u16
        private const int TFShipCountOffset    = 14;  // u16
        private const int TFHomeLocOffset      = 62;  // u16
        private const int TFSecLocOffset       = 64;  // u16
        private const int TFDestLocOffset      = 66;  // u16
        private const int TFPathStartOffset    = 68;  // array of u16 pairs (X,Y)
        private const int TFPathMaxWaypoints   = 10;

        // ─── Leader block (533): stride 180 bytes ─────────────────────────────
        private const int LeaderStride         = 180;
        private const int LeaderNameLength     = 40;
        private const int LeaderSideOffset     = 40;  // u8
        private const int LeaderShipOffset     = 42;  // u16 — index into ship table
        private const int LeaderStatusOffset   = 44;  // u16 — 19999 = lost

        // ─── Location block (530/531): stride 1172 bytes ──────────────────────
        private const int LocStride            = 1172;
        private const int LocHexXOffset        = 4;   // u16
        private const int LocHexYOffset        = 6;   // u16
        private const int LocControllerOffset  = 8;   // u8

        // ─── Ship Class block (539): stride 380 bytes ─────────────────────────
        private const int ClassStride          = 380;
        private const int ClassNameLength      = 30;
        private const int ClassTypeOffset      = 30;  // u8
        private const int ClassMaxFuelOffset   = 48;  // u16

        private static readonly Regex DateRegex =
            new Regex(@"Date:\s*(\d{1,2})/(\d{1,2})/(\d{2})", RegexOptions.Compiled);
        private static readonly Regex VersionRegex =
            new Regex(@"(\d+\.\d+\.\d+[\.\d]*)", RegexOptions.Compiled);
        private static readonly char[] Newlines = { '\r', '\n' };

        // ─── Public entry point ───────────────────────────────────────────────

        /// <summary>
        /// Parses a single .pws file and returns an Allied-only snapshot.
        /// Throws on unreadable GZip outer; inner block errors are silently skipped.
        /// </summary>
        public static PwsSnapshot Parse(string filePath, string? overrideDate = null)
        {
            byte[] outer = PwsBlockScanner.DecompressOuter(filePath);
            var snapshot = new PwsSnapshot();
            var meta = new PwsMetaRecord { FilePath = filePath };

            bool shipBlockSeen    = false;
            bool classBlockSeen   = false;
            bool locBlockSeen     = false;
            bool leaderBlockSeen  = false;
            bool tfBlockSeen      = false;

            // Build a lookup: class index → max fuel (populated once class block is parsed)
            var classMaxFuelMap = new Dictionary<int, int>();

            foreach (var block in PwsBlockScanner.ScanBlocks(outer))
            {
                try
                {
                    switch (block.BlockId)
                    {
                        case 550:
                            meta = ParseMeta(block.Payload, filePath, overrideDate);
                            break;

                        case 539 when !classBlockSeen:
                            classBlockSeen = true;
                            ParseClassBlock(block.Payload, snapshot, meta.DateYYMMDD, classMaxFuelMap);
                            break;

                        case 520 when !shipBlockSeen:
                            shipBlockSeen = true;
                            ParseShipBlock(block.Payload, snapshot, meta.DateYYMMDD, classMaxFuelMap);
                            break;

                        case 533 when !leaderBlockSeen:
                            leaderBlockSeen = true;
                            ParseLeaderBlock(block.Payload, snapshot, meta.DateYYMMDD);
                            break;

                        case 530 or 531 when !locBlockSeen:
                            locBlockSeen = true;
                            ParseLocationBlock(block.Payload, snapshot, meta.DateYYMMDD);
                            break;

                        case 582 when !tfBlockSeen:
                            tfBlockSeen = true;
                            ParseTaskForceBlock(block.Payload, snapshot, meta.DateYYMMDD);
                            break;

                        case 593:
                            ParseReportBlock(block.Payload, snapshot, meta.DateYYMMDD);
                            break;

                        case 565:
                            // AI scenario scripts — skip per spec. Not used in 2-player PBEM.
                            break;
                    }
                }
                catch (Exception ex)
                {
                    snapshot.Warnings.Add($"Block {block.BlockId} decode error: {ex.Message}");
                }
            }

            // If ship block was seen before class block (can happen), re-apply MaxFuel
            if (classMaxFuelMap.Count > 0 && snapshot.AlliedShips.Count > 0)
            {
                // Rebuild ships list in-place applying max fuel from class map
                for (int i = 0; i < snapshot.AlliedShips.Count; i++)
                {
                    var s = snapshot.AlliedShips[i];
                    if (s.MaxFuel == 0 && classMaxFuelMap.TryGetValue(s.ClassIndex, out int mf))
                    {
                        snapshot.AlliedShips[i] = new PwsShipRecord
                        {
                            Index = s.Index,
                            Name = s.Name,
                            Status = s.Status,
                            Side = s.Side,
                            Nation = s.Nation,
                            ClassIndex = s.ClassIndex,
                            HexX = s.HexX,
                            HexY = s.HexY,
                            TaskForceIndex = s.TaskForceIndex,
                            Fuel = s.Fuel,
                            MaxFuel = mf,
                            ArrivalDays = s.ArrivalDays,
                            ArrivalLocationIndex = s.ArrivalLocationIndex,
                            SnapshotDate = s.SnapshotDate
                        };
                    }
                }
            }

            // ── Compute aggregate counters ──────────────────────────────────
            foreach (var ship in snapshot.AlliedShips)
            {
                if (ship.IsSunk)
                {
                    snapshot.AlliedSunkCount++;
                }
                else if (ship.IsReinforcement)
                {
                    snapshot.AlliedReinforcementCount++;
                }
                else
                {
                    snapshot.AlliedActiveCount++;
                    if (ship.IsAtSea)
                        snapshot.AlliedAtSeaCount++;
                    else
                        snapshot.AlliedInPortCount++;
                }
            }

            snapshot.AlliedTaskForceCount = snapshot.AlliedTaskForces.Count;

            // Assign the resolved meta (date may have been resolved after ships/TFs were parsed)
            snapshot.Meta = meta;
            return snapshot;
        }

        // ─── Block 550: Game Header / Meta ────────────────────────────────────

        private static PwsMetaRecord ParseMeta(byte[] payload, string filePath, string? overrideDate)
        {
            string text = Encoding.Latin1.GetString(payload);
            string dateYYMMDD = overrideDate ?? string.Empty;
            string dateRaw = string.Empty;
            string scenario = string.Empty;
            string version = string.Empty;

            if (string.IsNullOrEmpty(dateYYMMDD))
            {
                var m = DateRegex.Match(text);
                if (m.Success)
                {
                    int mm = int.Parse(m.Groups[1].Value);
                    int dd = int.Parse(m.Groups[2].Value);
                    int yy = int.Parse(m.Groups[3].Value);
                    dateYYMMDD = $"{yy:D2}{mm:D2}{dd:D2}";
                    dateRaw = m.Value;
                }
            }

            var vMatch = VersionRegex.Match(text);
            if (vMatch.Success)
                version = vMatch.Value;

            // Scenario name: typically first non-empty line of header text
            var lines = text.Split(Newlines, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.Length > 5 && !trimmed.StartsWith("Date:") && !char.IsDigit(trimmed[0]))
                {
                    scenario = trimmed;
                    break;
                }
            }

            bool isPbemIncoming = System.IO.Path.GetFileName(filePath).IndexOf("021",
                StringComparison.OrdinalIgnoreCase) >= 0;

            return new PwsMetaRecord
            {
                GameDateRaw    = dateRaw,
                DateYYMMDD     = dateYYMMDD,
                ScenarioName   = scenario,
                GameVersion    = version,
                IsPbemIncoming = isPbemIncoming,
                FilePath       = filePath
            };
        }

        // ─── Block 539: Ship Class Records ────────────────────────────────────

        private static void ParseClassBlock(byte[] payload, PwsSnapshot snapshot,
            string dateYYMMDD, Dictionary<int, int> maxFuelMap)
        {
            int numRows = payload.Length / ClassStride;
            for (int i = 0; i < numRows; i++)
            {
                int off = i * ClassStride;
                string name = ReadNullTerminatedLatin1(payload, off, ClassNameLength);
                int vesselType = payload[off + ClassTypeOffset];
                int maxFuel    = BitConverter.ToUInt16(payload, off + ClassMaxFuelOffset);

                maxFuelMap[i] = maxFuel;

                snapshot.ShipClasses.Add(new PwsClassRecord
                {
                    Index       = i,
                    ClassName   = name,
                    VesselType  = vesselType,
                    MaxFuel     = maxFuel,
                    SnapshotDate = dateYYMMDD
                });
            }
        }

        // ─── Block 513/520: Ship Records ──────────────────────────────────────

        private static void ParseShipBlock(byte[] payload, PwsSnapshot snapshot,
            string dateYYMMDD, Dictionary<int, int> classMaxFuelMap)
        {
            int numRows = payload.Length / ShipStride;
            for (int i = 0; i < numRows; i++)
            {
                int off    = i * ShipStride;
                int status = BitConverter.ToUInt16(payload, off + ShipStatusOffset);
                int side   = payload[off + ShipSideOffset];

                // ── Faction isolation: Allied only ──
                // Japanese records (side == 0) are NEVER added to output.
                if (side != (int)ShipSide.Allied)
                    continue;

                string name = ReadNullTerminatedLatin1(payload, off, ShipNameLength);
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                int nation       = payload[off + ShipNationOffset];
                int classIdx     = BitConverter.ToUInt16(payload, off + ShipClassOffset);
                int tfIdx        = BitConverter.ToUInt16(payload, off + ShipTFOffset);
                int fuel         = BitConverter.ToUInt16(payload, off + ShipFuelOffset);
                int arrivalDays  = BitConverter.ToUInt16(payload, off + ShipArrivalDaysOff);
                int arrivalLoc   = BitConverter.ToUInt16(payload, off + ShipArrivalLocOff);
                int hexX         = BitConverter.ToUInt16(payload, off + ShipHexXOffset);
                int hexY         = BitConverter.ToUInt16(payload, off + ShipHexYOffset);
                int maxFuel      = classMaxFuelMap.TryGetValue(classIdx, out int mf) ? mf : 0;

                snapshot.AlliedShips.Add(new PwsShipRecord
                {
                    Index                = i,
                    Name                 = name,
                    Status               = status,
                    Side                 = side,
                    Nation               = nation,
                    ClassIndex           = classIdx,
                    HexX                 = hexX,
                    HexY                 = hexY,
                    TaskForceIndex       = tfIdx,
                    Fuel                 = fuel,
                    MaxFuel              = maxFuel,
                    ArrivalDays          = arrivalDays,
                    ArrivalLocationIndex = arrivalLoc,
                    SnapshotDate         = dateYYMMDD
                });
            }
        }

        // ─── Block 533: Leader Records ────────────────────────────────────────

        private static void ParseLeaderBlock(byte[] payload, PwsSnapshot snapshot, string dateYYMMDD)
        {
            int numRows = payload.Length / LeaderStride;
            for (int i = 0; i < numRows; i++)
            {
                int off  = i * LeaderStride;
                int side = payload[off + LeaderSideOffset];

                // Faction isolation: Allied only
                if (side != (int)ShipSide.Allied)
                    continue;

                string name = ReadNullTerminatedLatin1(payload, off, LeaderNameLength);
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                int shipIdx = BitConverter.ToUInt16(payload, off + LeaderShipOffset);
                int status  = BitConverter.ToUInt16(payload, off + LeaderStatusOffset);

                snapshot.AlliedLeaders.Add(new PwsLeaderRecord
                {
                    Index        = i,
                    Name         = name,
                    Side         = side,
                    ShipIndex    = shipIdx,
                    Status       = status,
                    SnapshotDate = dateYYMMDD
                });
            }
        }

        // ─── Block 530/531: Location Records ──────────────────────────────────

        private static void ParseLocationBlock(byte[] payload, PwsSnapshot snapshot, string dateYYMMDD)
        {
            int numRows = payload.Length / LocStride;
            for (int i = 0; i < numRows; i++)
            {
                int off  = i * LocStride;
                int hexX = BitConverter.ToUInt16(payload, off + LocHexXOffset);
                int hexY = BitConverter.ToUInt16(payload, off + LocHexYOffset);
                int ctrl = payload[off + LocControllerOffset];

                // Include all locations regardless of controller — used for TF destination lookup
                snapshot.Locations.Add(new PwsLocationRecord
                {
                    Index        = i,
                    HexX         = hexX,
                    HexY         = hexY,
                    Controller   = ctrl,
                    SnapshotDate = dateYYMMDD
                });
            }
        }

        // ─── Block 582: Task Force Records ────────────────────────────────────

        private static void ParseTaskForceBlock(byte[] payload, PwsSnapshot snapshot, string dateYYMMDD)
        {
            int numRows = payload.Length / TFStride;
            for (int i = 0; i < numRows; i++)
            {
                int off  = i * TFStride;
                int side = payload[off + TFSideOffset];

                // Faction isolation: Allied only
                if (side != (int)ShipSide.Allied)
                    continue;

                int flagship    = BitConverter.ToUInt16(payload, off + TFFlagshipOffset);
                int mission     = payload[off + TFMissionOffset];
                int hexX        = BitConverter.ToUInt16(payload, off + TFHexXOffset);
                int hexY        = BitConverter.ToUInt16(payload, off + TFHexYOffset);
                int shipCount   = BitConverter.ToUInt16(payload, off + TFShipCountOffset);
                int homeLoc     = BitConverter.ToUInt16(payload, off + TFHomeLocOffset);
                int secLoc      = BitConverter.ToUInt16(payload, off + TFSecLocOffset);
                int destLoc     = BitConverter.ToUInt16(payload, off + TFDestLocOffset);

                // Empty TF check: flagship == 0 and shipCount == 0
                if (flagship == 0 && shipCount == 0)
                    continue;

                // Path waypoints: up to TFPathMaxWaypoints pairs of u16 (X, Y)
                var path = new List<(int, int)>();
                for (int wp = 0; wp < TFPathMaxWaypoints; wp++)
                {
                    int pOff = off + TFPathStartOffset + wp * 4;
                    if (pOff + 4 > payload.Length) break;
                    int wx = BitConverter.ToUInt16(payload, pOff);
                    int wy = BitConverter.ToUInt16(payload, pOff + 2);
                    if (wx == 0 && wy == 0) break;
                    path.Add((wx, wy));
                }

                snapshot.AlliedTaskForces.Add(new PwsTaskForceRecord
                {
                    Index                    = i,
                    FlagshipIndex            = flagship,
                    Side                     = side,
                    MissionType              = mission,
                    HexX                     = hexX,
                    HexY                     = hexY,
                    ShipCount                = shipCount,
                    HomeLocationIndex        = homeLoc,
                    SecondaryLocationIndex   = secLoc,
                    DestinationLocationIndex = destLoc,
                    Path                     = path,
                    SnapshotDate             = dateYYMMDD
                });
            }
        }

        // ─── Block 593: Turn Reports ──────────────────────────────────────────

        private static void ParseReportBlock(byte[] payload, PwsSnapshot snapshot, string dateYYMMDD)
        {
            // 4 sections: [CombatReport, CombatEvents, Operations, Sigint]
            // Each section: int32 length prefix (bytes), then Latin1 text
            var kinds = new[]
            {
                PwsReportSection.SectionKind.CombatReport,
                PwsReportSection.SectionKind.CombatEvents,
                PwsReportSection.SectionKind.Operations,
                PwsReportSection.SectionKind.Sigint
            };

            int p = 0;
            for (int idx = 0; idx < kinds.Length && p + 4 <= payload.Length; idx++)
            {
                int secLen = BitConverter.ToInt32(payload, p);
                p += 4;
                if (secLen <= 0 || p + secLen > payload.Length)
                {
                    p += Math.Max(0, secLen);
                    continue;
                }

                string text = Encoding.Latin1.GetString(payload, p, secLen);
                string[] lines = text.Split(Newlines, StringSplitOptions.None);
                p += secLen;

                snapshot.Reports.Add(new PwsReportSection
                {
                    Kind         = kinds[idx],
                    Text         = text,
                    Lines        = lines,
                    DateYYMMDD   = dateYYMMDD
                });
            }
        }

        // ─── Utilities ────────────────────────────────────────────────────────

        private static string ReadNullTerminatedLatin1(byte[] buf, int offset, int maxLen)
        {
            // Use ReadOnlySpan<byte> for zero-allocation scan (Pillar 1 compliance)
            var span = new ReadOnlySpan<byte>(buf, offset, Math.Min(maxLen, buf.Length - offset));
            int len = span.IndexOf((byte)0);
            if (len < 0) len = span.Length;
            if (len == 0) return string.Empty;
            return Encoding.Latin1.GetString(buf, offset, len).Trim();
        }
    }
}
