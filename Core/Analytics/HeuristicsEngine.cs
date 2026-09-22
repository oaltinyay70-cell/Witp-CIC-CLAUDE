using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MUTHUR6000.Core.Database;
using MUTHUR6000.Core.Ingestion;

namespace MUTHUR6000.Core.Analytics
{
    public class MinefieldHitRecord
    {
        public string DateYYMMDD { get; set; } = string.Empty;
        public int HexX { get; set; }
        public int HexY { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string RawLine { get; set; } = string.Empty;
    }

    public class SubmarineContactRecord
    {
        public string DateYYMMDD { get; set; } = string.Empty;
        public int HexX { get; set; }
        public int HexY { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string SubmarineName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string RawLine { get; set; } = string.Empty;
    }

    public class DailyCentroid
    {
        public string DateYYMMDD { get; set; } = string.Empty;
        public double CentroidX { get; set; }
        public double CentroidY { get; set; }
        public int ContactCount { get; set; }
        public List<SubmarineContactRecord> Contacts { get; set; } = new();
    }

    public class CombatHexRecord
    {
        public string DateYYMMDD { get; set; } = string.Empty;
        public int HexX { get; set; }
        public int HexY { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool HasPriorSigint { get; set; }
    }

    public class DailyFleetMean
    {
        public string DateYYMMDD { get; set; } = string.Empty;
        public double MeanX { get; set; }
        public double MeanY { get; set; }
        public int TotalVesselsCounted { get; set; }
        public List<string> ContributingUnits { get; set; } = new();
    }

    public class StepVector
    {
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
        public double DistanceHex { get; set; }
        public double BearingDeg { get; set; }
        public string CompassHeading { get; set; } = string.Empty;
    }

    public class DailyOptempoRecord
    {
        public string DateYYMMDD { get; set; } = string.Empty;
        public int SeparatorCount { get; set; }
        public double Rolling3DayAvg { get; set; }
        public List<string> EngagementsSample { get; set; } = new();
    }

    public class RegionalShiftEntry
    {
        public string RegionName { get; set; } = string.Empty;
        public int CountToday { get; set; }
        public int Count7DaysAgo { get; set; }
        public int ShiftDelta => CountToday - Count7DaysAgo;
        public string Trend => ShiftDelta > 0 ? $"BUILD-UP (+{ShiftDelta})" : (ShiftDelta < 0 ? $"WITHDRAWAL ({ShiftDelta})" : "STABLE (0)");
        public List<string> TodayVessels { get; set; } = new();
        public List<string> BaselineVessels { get; set; } = new();
    }

    public class HeuristicsEngine
    {
        private static readonly Regex CoordRegex = new(@"(\d{2,3})\s*,\s*(\d{2,3})", RegexOptions.Compiled);

        public int CampaignDaysProcessed { get; private set; } = 0;
        public bool HasSufficientTemporalData => CampaignDaysProcessed >= 4;

        // Metric 1: Regional Power Shift
        public List<RegionalShiftEntry> RegionalShifts { get; } = new();

        // Metric 4: Minefield Mapping
        public HashSet<(int X, int Y)> MineHitCoordinates { get; } = new();
        public List<MinefieldHitRecord> MineHitsChronological { get; } = new();

        // Metric 5: Submarine Migration
        public List<DailyCentroid> SubmarineCentroids { get; } = new();
        public List<StepVector> SubmarineVectors { get; } = new();

        // Metric 46: Intelligence Blindspots
        public List<CombatHexRecord> IntelligenceBlindspots { get; } = new();

        // Metric 49: Axis of Advance
        public List<DailyFleetMean> FleetMeans { get; } = new();
        public List<StepVector> AxisStepVectors { get; } = new();
        public StepVector StrategicDrift { get; private set; } = new();

        // Metric 50: Operational Tempo (OPTEMPO)
        public List<DailyOptempoRecord> DailyOptempo { get; } = new();

        public static string ResolveRegion(int hexX, int hexY, string baseName)
        {
            if (hexX <= 0 || hexY <= 0)
            {
                var baseLoc = BaseLocationRegistry.Instance.FindBaseByName(baseName);
                if (baseLoc != null)
                {
                    hexX = baseLoc.HexX;
                    hexY = baseLoc.HexY;
                }
            }

            // 1. Malaya (e.g., Singapore, Kota Bharu, Malacca, Penang, Kuala Lumpur, Kuantan)
            if (hexX >= 45 && hexX <= 57 && hexY >= 70 && hexY <= 86)
                return "Malaya";

            // 2. Dutch East Indies (DEI) (e.g., Batavia, Soerabaja, Palembang, Balikpapan, Tarakan, Ambon, Timor)
            if (hexX >= 40 && hexX <= 85 && hexY >= 87 && hexY <= 120)
                return "Dutch East Indies (DEI)";

            // 3. Philippines (e.g., Manila, Legaspi, Aparri, Batan Island, Davao, Clark Field)
            if (hexX >= 70 && hexX <= 92 && hexY >= 60 && hexY <= 95)
                return "Philippines";

            // 4. New Guinea (e.g., Port Moresby, Lae, Gona, Buna, Wewak, Hollandia)
            if (hexX >= 88 && hexX <= 112 && hexY >= 115 && hexY <= 140)
                return "New Guinea";

            // 5. Solomon Islands (e.g., Guadalcanal, Tulagi, Rabaul, Bismarck Archipelago, Shortland, Buka)
            if (hexX >= 105 && hexX <= 125 && hexY >= 118 && hexY <= 145)
                return "Solomon Islands";

            // 6. South Pacific (e.g., Noumea, Espiritu Santo, Fiji, Suva, Pago Pago, New Hebrides)
            if (hexX >= 120 && hexX <= 170 && hexY >= 130 && hexY <= 175)
                return "South Pacific";

            // 7. Home Islands (Japan) (e.g., Tokyo, Kure, Hiroshima, Ominato, Sasebo, Osaka, Nagoya)
            if (hexX >= 100 && hexX <= 125 && hexY >= 45 && hexY <= 65)
                return "Home Islands (Japan)";

            // 8. China / Manchuria / Formosa (e.g., Shanghai, Canton, Takao, Pescadores, Peking, Hong Kong)
            if (hexX >= 65 && hexX <= 100 && hexY >= 35 && hexY <= 68)
                return "China / Manchuria / Formosa";

            // 9. Mandates (Micronesia / Marianas) (e.g., Truk, Saipan, Guam, Palau, Ponape, Kwajalein)
            if (hexX >= 90 && hexX <= 145 && hexY >= 70 && hexY <= 115)
                return "Mandates (Micronesia / Marianas)";

            // 10. Central Pacific (Hawaii / Marshalls) (e.g., Pearl Harbor, Midway, Johnston, Wake)
            if (hexX >= 146 && hexY >= 65)
                return "Central Pacific (Hawaii / Marshalls)";

            // Fallback: Check BaseLocationRegistry initial region
            var fallbackLoc = BaseLocationRegistry.Instance.FindBaseByName(baseName);
            if (fallbackLoc != null && !string.IsNullOrEmpty(fallbackLoc.InitialRegion))
            {
                string r = fallbackLoc.InitialRegion.ToUpperInvariant();
                if (r.Contains("MALAYA")) return "Malaya";
                if (r.Contains("DEI") || r.Contains("DUTCH") || r.Contains("JAVA") || r.Contains("SUMATRA") || r.Contains("BORNEO")) return "Dutch East Indies (DEI)";
                if (r.Contains("PHILIPPINES")) return "Philippines";
                if (r.Contains("NEW GUINEA") || r.Contains("PAPUA")) return "New Guinea";
                if (r.Contains("SOLOMON")) return "Solomon Islands";
                if (r.Contains("JAPAN")) return "Home Islands (Japan)";
                if (r.Contains("CHINA") || r.Contains("MANCHURIA") || r.Contains("FORMOSA")) return "China / Manchuria / Formosa";
                if (r.Contains("MARSHALL") || r.Contains("MARIANA") || r.Contains("CAROLINE")) return "Mandates (Micronesia / Marianas)";
                if (r.Contains("HAWAII") || r.Contains("CENTRAL")) return "Central Pacific (Hawaii / Marshalls)";
            }

            return "Other / Ocean";
        }

        public void Analyze(
            IngestionManifest manifest,
            IReadOnlyList<UnitLoadoutEvent> unitLoadouts,
            IReadOnlyList<SightingEvent> sightings,
            IReadOnlyList<RadioInterceptEvent> radioIntercepts,
            IReadOnlyList<ConfidenceReport> confidenceReports)
        {
            ClearAll();

            CampaignDaysProcessed = manifest.DaysProcessed;
            var orderedDates = manifest.UniqueDates.OrderBy(d => d).ToList();
            string latestDate = manifest.LatestDate;

            // ─────────────────────────────────────────────────────────────────
            // 1. Regional Power Shift
            // ─────────────────────────────────────────────────────────────────
            ComputeRegionalPowerShift(orderedDates, latestDate, unitLoadouts, sightings);

            // ─────────────────────────────────────────────────────────────────
            // 4. Minefield Mapping (combatreport)
            // ─────────────────────────────────────────────────────────────────
            ComputeMinefieldMapping(manifest);

            // ─────────────────────────────────────────────────────────────────
            // 5. Submarine Migration (asigint + Combat_Events + combatreport)
            // ─────────────────────────────────────────────────────────────────
            ComputeSubmarineMigration(manifest, orderedDates, sightings);

            // ─────────────────────────────────────────────────────────────────
            // 46. Intelligence Blindspots (asigint vs combatreport)
            // ─────────────────────────────────────────────────────────────────
            ComputeIntelligenceBlindspots(manifest, orderedDates, radioIntercepts, sightings);

            // ─────────────────────────────────────────────────────────────────
            // 49. Axis of Advance (asigint + sightings Japanese ships)
            // ─────────────────────────────────────────────────────────────────
            ComputeAxisOfAdvance(orderedDates, unitLoadouts, sightings);

            // ─────────────────────────────────────────────────────────────────
            // 50. Operational Tempo (OPTEMPO) (combatreport '---' count)
            // ─────────────────────────────────────────────────────────────────
            ComputeOperationalTempo(manifest, orderedDates);
        }

        private void ClearAll()
        {
            RegionalShifts.Clear();
            MineHitCoordinates.Clear();
            MineHitsChronological.Clear();
            SubmarineCentroids.Clear();
            SubmarineVectors.Clear();
            IntelligenceBlindspots.Clear();
            FleetMeans.Clear();
            AxisStepVectors.Clear();
            StrategicDrift = new StepVector();
            DailyOptempo.Clear();
        }

        private void ComputeRegionalPowerShift(
            List<string> orderedDates,
            string latestDate,
            IReadOnlyList<UnitLoadoutEvent> unitLoadouts,
            IReadOnlyList<SightingEvent> sightings)
        {
            string baselineDate = string.Empty;
            if (orderedDates.Count >= 8)
            {
                int idx = orderedDates.IndexOf(latestDate);
                baselineDate = (idx >= 7) ? orderedDates[idx - 7] : orderedDates[0];
            }
            else if (orderedDates.Count > 1)
            {
                baselineDate = orderedDates[0];
            }

            string[] targetRegions =
            {
                "Malaya",
                "Dutch East Indies (DEI)",
                "Philippines",
                "New Guinea",
                "Solomon Islands",
                "South Pacific",
                "Home Islands (Japan)",
                "China / Manchuria / Formosa",
                "Mandates (Micronesia / Marianas)",
                "Central Pacific (Hawaii / Marshalls)"
            };

            foreach (var rName in targetRegions)
            {
                var entry = new RegionalShiftEntry { RegionName = rName };

                // Today counts (latestDate)
                foreach (var ul in unitLoadouts.Where(u => u.DateYYMMDD == latestDate))
                {
                    string reg = ResolveRegion(0, 0, ul.Destination);
                    if (reg.Equals(rName, StringComparison.OrdinalIgnoreCase))
                    {
                        entry.CountToday++;
                        entry.TodayVessels.Add($"{ul.UnitName} on {ul.VesselClass} -> {ul.Destination}");
                    }
                }
                foreach (var s in sightings.Where(s => s.DateYYMMDD == latestDate))
                {
                    string reg = ResolveRegion(s.HexX, s.HexY, s.NearLocation);
                    if (reg.Equals(rName, StringComparison.OrdinalIgnoreCase))
                    {
                        entry.CountToday += Math.Max(1, s.ShipCount);
                        entry.TodayVessels.Add($"{s.ShipCount} ships at {s.HexX},{s.HexY} near {s.NearLocation} ({s.Description})");
                    }
                }

                // Baseline counts (baselineDate)
                if (!string.IsNullOrEmpty(baselineDate) && baselineDate != latestDate)
                {
                    foreach (var ul in unitLoadouts.Where(u => u.DateYYMMDD == baselineDate))
                    {
                        string reg = ResolveRegion(0, 0, ul.Destination);
                        if (reg.Equals(rName, StringComparison.OrdinalIgnoreCase))
                        {
                            entry.Count7DaysAgo++;
                            entry.BaselineVessels.Add($"{ul.UnitName} on {ul.VesselClass} -> {ul.Destination}");
                        }
                    }
                    foreach (var s in sightings.Where(s => s.DateYYMMDD == baselineDate))
                    {
                        string reg = ResolveRegion(s.HexX, s.HexY, s.NearLocation);
                        if (reg.Equals(rName, StringComparison.OrdinalIgnoreCase))
                        {
                            entry.Count7DaysAgo += Math.Max(1, s.ShipCount);
                            entry.BaselineVessels.Add($"{s.ShipCount} ships at {s.HexX},{s.HexY} near {s.NearLocation}");
                        }
                    }
                }

                RegionalShifts.Add(entry);
            }
        }

        private void ComputeMinefieldMapping(IngestionManifest manifest)
        {
            var combatReportFiles = manifest.Files.Where(f => f.FileType == "COMBAT_REPORT" || f.FileType == "COMBAT_EVENTS");

            foreach (var file in combatReportFiles.OrderBy(f => f.DateYYMMDD))
            {
                if (!File.Exists(file.FullPath)) continue;
                string date = file.DateYYMMDD;

                foreach (var rawLine in File.ReadLines(file.FullPath))
                {
                    string line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    // Example 1: TF 442 encounters mine field at Bataan (78,77)
                    if (line.Contains("encounters mine field at", StringComparison.OrdinalIgnoreCase))
                    {
                        int atPos = line.IndexOf("at ", StringComparison.OrdinalIgnoreCase);
                        string locationPart = (atPos != -1) ? line.Substring(atPos + 3).Trim() : string.Empty;

                        int hx = 0, hy = 0;
                        var m = CoordRegex.Match(locationPart);
                        if (m.Success)
                        {
                            int.TryParse(m.Groups[1].Value, out hx);
                            int.TryParse(m.Groups[2].Value, out hy);
                        }

                        string locName = locationPart;
                        int paren = locationPart.IndexOf('(');
                        if (paren != -1) locName = locationPart.Substring(0, paren).Trim();

                        if (hx > 0 && hy > 0)
                        {
                            MineHitCoordinates.Add((hx, hy));
                            MineHitsChronological.Add(new MinefieldHitRecord
                            {
                                DateYYMMDD = date,
                                HexX = hx,
                                HexY = hy,
                                LocationName = locName,
                                Details = line,
                                RawLine = line
                            });
                        }
                    }
                    // Example 2: DD Pope hits Type 88 Mine at Bataan
                    else if (line.Contains("hits Type", StringComparison.OrdinalIgnoreCase) && line.Contains("Mine at", StringComparison.OrdinalIgnoreCase))
                    {
                        int atPos = line.IndexOf("at ", StringComparison.OrdinalIgnoreCase);
                        string locName = (atPos != -1) ? line.Substring(atPos + 3).Trim() : string.Empty;

                        var baseLoc = BaseLocationRegistry.Instance.FindBaseByName(locName);
                        int hx = baseLoc?.HexX ?? 0;
                        int hy = baseLoc?.HexY ?? 0;

                        if (hx > 0 && hy > 0)
                        {
                            MineHitCoordinates.Add((hx, hy));
                            MineHitsChronological.Add(new MinefieldHitRecord
                            {
                                DateYYMMDD = date,
                                HexX = hx,
                                HexY = hy,
                                LocationName = locName,
                                Details = line,
                                RawLine = line
                            });
                        }
                    }
                }
            }
        }

        private void ComputeSubmarineMigration(
            IngestionManifest manifest,
            List<string> orderedDates,
            IReadOnlyList<SightingEvent> sightings)
        {
            var contactsByDate = new Dictionary<string, List<SubmarineContactRecord>>();

            foreach (var d in orderedDates) contactsByDate[d] = new List<SubmarineContactRecord>();

            // 1. From sightings (Combat_Events or Operations) where Description contains SS
            foreach (var s in sightings)
            {
                if (s.Description.Contains(" SS ") || s.Description.Contains("Submarine") || s.Description.StartsWith("SS ") || s.Description.EndsWith(" SS"))
                {
                    if (s.HexX > 0 && s.HexY > 0)
                    {
                        if (!contactsByDate.ContainsKey(s.DateYYMMDD)) contactsByDate[s.DateYYMMDD] = new List<SubmarineContactRecord>();
                        contactsByDate[s.DateYYMMDD].Add(new SubmarineContactRecord
                        {
                            DateYYMMDD = s.DateYYMMDD,
                            HexX = s.HexX,
                            HexY = s.HexY,
                            LocationName = s.NearLocation,
                            SubmarineName = s.Description,
                            Source = s.SourceFile,
                            RawLine = $"{s.Observer}: {s.Description} at {s.HexX},{s.HexY}"
                        });
                    }
                }
            }

            // 2. From Combat Report & Events directly
            foreach (var file in manifest.Files.Where(f => f.FileType == "COMBAT_REPORT" || f.FileType == "SIGINT"))
            {
                if (!File.Exists(file.FullPath)) continue;
                string date = file.DateYYMMDD;

                foreach (var rawLine in File.ReadLines(file.FullPath))
                {
                    string line = rawLine.Trim();
                    if (line.Contains("SS I-") || line.Contains("Japanese SS at ") || line.Contains("SS Ro-"))
                    {
                        var m = CoordRegex.Match(line);
                        int hx = 0, hy = 0;
                        if (m.Success)
                        {
                            int.TryParse(m.Groups[1].Value, out hx);
                            int.TryParse(m.Groups[2].Value, out hy);
                        }
                        if (hx > 0 && hy > 0)
                        {
                            if (!contactsByDate.ContainsKey(date)) contactsByDate[date] = new List<SubmarineContactRecord>();
                            contactsByDate[date].Add(new SubmarineContactRecord
                            {
                                DateYYMMDD = date,
                                HexX = hx,
                                HexY = hy,
                                LocationName = $"Hex {hx},{hy}",
                                SubmarineName = line,
                                Source = file.FileType,
                                RawLine = line
                            });
                        }
                    }
                }
            }

            // Calculate Centroid per day
            foreach (var d in orderedDates)
            {
                var contacts = contactsByDate[d];
                if (contacts.Count > 0)
                {
                    double avgX = contacts.Average(c => c.HexX);
                    double avgY = contacts.Average(c => c.HexY);

                    SubmarineCentroids.Add(new DailyCentroid
                    {
                        DateYYMMDD = d,
                        CentroidX = avgX,
                        CentroidY = avgY,
                        ContactCount = contacts.Count,
                        Contacts = contacts
                    });
                }
            }

            // Connect Centroids daily to create migration vectors
            for (int i = 0; i < SubmarineCentroids.Count - 1; i++)
            {
                var c1 = SubmarineCentroids[i];
                var c2 = SubmarineCentroids[i + 1];

                double dx = c2.CentroidX - c1.CentroidX;
                double dy = c2.CentroidY - c1.CentroidY;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                double rad = Math.Atan2(dx, -dy);
                double deg = rad * 180.0 / Math.PI;
                if (deg < 0) deg += 360.0;

                string cardinal = deg switch
                {
                    >= 337.5 or < 22.5 => "N",
                    >= 22.5 and < 67.5 => "NE",
                    >= 67.5 and < 112.5 => "E",
                    >= 112.5 and < 157.5 => "SE",
                    >= 157.5 and < 202.5 => "S",
                    >= 202.5 and < 247.5 => "SW",
                    >= 247.5 and < 292.5 => "W",
                    _ => "NW"
                };

                SubmarineVectors.Add(new StepVector
                {
                    FromDate = c1.DateYYMMDD,
                    ToDate = c2.DateYYMMDD,
                    DeltaX = dx,
                    DeltaY = dy,
                    DistanceHex = dist,
                    BearingDeg = deg,
                    CompassHeading = cardinal
                });
            }
        }

        private void ComputeIntelligenceBlindspots(
            IngestionManifest manifest,
            List<string> orderedDates,
            IReadOnlyList<RadioInterceptEvent> radioIntercepts,
            IReadOnlyList<SightingEvent> sightings)
        {
            var combatReports = manifest.Files.Where(f => f.FileType == "COMBAT_REPORT");

            foreach (var file in combatReports)
            {
                if (!File.Exists(file.FullPath)) continue;
                string date = file.DateYYMMDD;

                foreach (var rawLine in File.ReadLines(file.FullPath))
                {
                    string line = rawLine.Trim();
                    // Detect attack header: e.g. "Morning Air attack on Pearl Harbor , at 180,107"
                    if (line.Contains("attack on", StringComparison.OrdinalIgnoreCase) && line.Contains("at "))
                    {
                        var m = CoordRegex.Match(line);
                        if (m.Success && int.TryParse(m.Groups[1].Value, out int hx) && int.TryParse(m.Groups[2].Value, out int hy))
                        {
                            // Extract location name
                            int onPos = line.IndexOf("on ", StringComparison.OrdinalIgnoreCase);
                            int commaPos = line.IndexOf(',', onPos != -1 ? onPos : 0);
                            string locName = (onPos != -1 && commaPos != -1) ? line.Substring(onPos + 3, commaPos - (onPos + 3)).Trim() : $"Hex {hx},{hy}";

                            // Check prior 3 days in SIGINT and sightings
                            bool hasPriorSigint = radioIntercepts.Any(r => r.HexX == hx && r.HexY == hy && r.DateYYMMDD != date) ||
                                                  sightings.Any(s => s.HexX == hx && s.HexY == hy && s.DateYYMMDD != date);

                            if (!hasPriorSigint && !IntelligenceBlindspots.Any(b => b.HexX == hx && b.HexY == hy && b.DateYYMMDD == date))
                            {
                                IntelligenceBlindspots.Add(new CombatHexRecord
                                {
                                    DateYYMMDD = date,
                                    HexX = hx,
                                    HexY = hy,
                                    LocationName = locName,
                                    Description = line,
                                    HasPriorSigint = false
                                });
                            }
                        }
                    }
                }
            }
        }

        private void ComputeAxisOfAdvance(
            List<string> orderedDates,
            IReadOnlyList<UnitLoadoutEvent> unitLoadouts,
            IReadOnlyList<SightingEvent> sightings)
        {
            foreach (var d in orderedDates)
            {
                var hexes = new List<(double X, double Y)>();
                var contributing = new List<string>();

                // 1. Sightings for date d
                foreach (var s in sightings.Where(s => s.DateYYMMDD == d))
                {
                    if (s.HexX > 0 && s.HexY > 0)
                    {
                        int count = Math.Max(1, s.ShipCount);
                        for (int k = 0; k < count; k++) hexes.Add((s.HexX, s.HexY));
                        contributing.Add($"{s.ShipCount} ships at {s.HexX},{s.HexY} ({s.NearLocation})");
                    }
                }

                // 2. Unit loadouts moving to destination
                foreach (var ul in unitLoadouts.Where(u => u.DateYYMMDD == d))
                {
                    var baseLoc = BaseLocationRegistry.Instance.FindBaseByName(ul.Destination);
                    if (baseLoc != null && baseLoc.HexX > 0)
                    {
                        hexes.Add((baseLoc.HexX, baseLoc.HexY));
                        contributing.Add($"Transport on {ul.VesselClass} -> {ul.Destination} ({baseLoc.HexX},{baseLoc.HexY})");
                    }
                }

                if (hexes.Count > 0)
                {
                    FleetMeans.Add(new DailyFleetMean
                    {
                        DateYYMMDD = d,
                        MeanX = hexes.Average(h => h.X),
                        MeanY = hexes.Average(h => h.Y),
                        TotalVesselsCounted = hexes.Count,
                        ContributingUnits = contributing
                    });
                }
            }

            // Calculate step vectors: Pos_today - Pos_yesterday
            for (int i = 0; i < FleetMeans.Count - 1; i++)
            {
                var m1 = FleetMeans[i];
                var m2 = FleetMeans[i + 1];

                double dx = m2.MeanX - m1.MeanX;
                double dy = m2.MeanY - m1.MeanY;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                double rad = Math.Atan2(dx, -dy);
                double deg = rad * 180.0 / Math.PI;
                if (deg < 0) deg += 360.0;

                string cardinal = deg switch
                {
                    >= 337.5 or < 22.5 => "N",
                    >= 22.5 and < 67.5 => "NE",
                    >= 67.5 and < 112.5 => "E",
                    >= 112.5 and < 157.5 => "SE",
                    >= 157.5 and < 202.5 => "S",
                    >= 202.5 and < 247.5 => "SW",
                    >= 247.5 and < 292.5 => "W",
                    _ => "NW"
                };

                AxisStepVectors.Add(new StepVector
                {
                    FromDate = m1.DateYYMMDD,
                    ToDate = m2.DateYYMMDD,
                    DeltaX = dx,
                    DeltaY = dy,
                    DistanceHex = dist,
                    BearingDeg = deg,
                    CompassHeading = cardinal
                });
            }

            // Calculate Strategic Drift: Average the last 3 vectors (or available)
            if (AxisStepVectors.Count > 0)
            {
                var vectorsToAverage = AxisStepVectors.TakeLast(3).ToList();
                double avgDx = vectorsToAverage.Average(v => v.DeltaX);
                double avgDy = vectorsToAverage.Average(v => v.DeltaY);
                double avgDist = Math.Sqrt(avgDx * avgDx + avgDy * avgDy);

                double rad = Math.Atan2(avgDx, -avgDy);
                double deg = rad * 180.0 / Math.PI;
                if (deg < 0) deg += 360.0;

                string cardinal = deg switch
                {
                    >= 337.5 or < 22.5 => "N",
                    >= 22.5 and < 67.5 => "NE",
                    >= 67.5 and < 112.5 => "E",
                    >= 112.5 and < 157.5 => "SE",
                    >= 157.5 and < 202.5 => "S",
                    >= 202.5 and < 247.5 => "SW",
                    >= 247.5 and < 292.5 => "W",
                    _ => "NW"
                };

                StrategicDrift = new StepVector
                {
                    FromDate = vectorsToAverage.First().FromDate,
                    ToDate = vectorsToAverage.Last().ToDate,
                    DeltaX = avgDx,
                    DeltaY = avgDy,
                    DistanceHex = avgDist,
                    BearingDeg = deg,
                    CompassHeading = cardinal
                };
            }
        }

        private void ComputeOperationalTempo(IngestionManifest manifest, List<string> orderedDates)
        {
            var combatReportsByDate = manifest.Files
                .Where(f => f.FileType == "COMBAT_REPORT")
                .GroupBy(f => f.DateYYMMDD)
                .ToDictionary(g => g.Key, g => g.Select(f => f.FullPath).ToList());

            var dailyCounts = new List<(string Date, int Count, List<string> Samples)>();

            foreach (var d in orderedDates)
            {
                int sepCount = 0;
                var samples = new List<string>();

                if (combatReportsByDate.TryGetValue(d, out var fpaths))
                {
                    foreach (var fpath in fpaths)
                    {
                        if (!File.Exists(fpath)) continue;
                        foreach (var rawLine in File.ReadLines(fpath))
                        {
                            string line = rawLine.Trim();
                            if (line.StartsWith("---"))
                            {
                                sepCount++;
                            }
                            else if (line.Contains("attack on", StringComparison.OrdinalIgnoreCase) && samples.Count < 5)
                            {
                                samples.Add(line);
                            }
                        }
                    }
                }

                dailyCounts.Add((d, sepCount, samples));
            }

            for (int i = 0; i < dailyCounts.Count; i++)
            {
                var cur = dailyCounts[i];
                // Rolling 3-day average
                int windowStart = Math.Max(0, i - 2);
                int windowLen = i - windowStart + 1;
                double avg = dailyCounts.Skip(windowStart).Take(windowLen).Average(x => x.Count);

                DailyOptempo.Add(new DailyOptempoRecord
                {
                    DateYYMMDD = cur.Date,
                    SeparatorCount = cur.Count,
                    Rolling3DayAvg = avg,
                    EngagementsSample = cur.Samples
                });
            }
        }
    }
}
