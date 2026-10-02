using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ROCHEFORT6000.Core.Ingestion.Pws;

namespace ROCHEFORT6000.Core.Ingestion
{
    public class DiscoveredFile
    {
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty; // "SIGINT", "OPERATIONS", "COMBAT_REPORT", "COMBAT_EVENTS", "PWS_SAVE"
        public string DateYYMMDD { get; set; } = string.Empty;
    }

    /// <summary>
    /// Scan options. Real campaigns ingest exactly one .pws (the campaign save, e.g. wpae021.pws);
    /// tests may allow any .pws found under the scanned directories.
    /// </summary>
    public sealed class ScanOptions
    {
        /// <summary>Full path of the one .pws belonging to the campaign. Null = no .pws is ingested.</summary>
        public string? CampaignPwsPath { get; set; }
        /// <summary>Tests only: ingest every .pws under the scanned directories.</summary>
        public bool AllowAnyPws { get; set; }

        public static ScanOptions TestMode => new ScanOptions { AllowAnyPws = true };
    }

    public class IngestionManifest
    {
        public List<DiscoveredFile> Files { get; set; } = new List<DiscoveredFile>();
        public SortedSet<string> UniqueDates { get; set; } = new SortedSet<string>();
        public string LatestDate { get; set; } = "411207";
        public string FormattedDate { get; set; } = "07/12/41";
        public int DaysProcessed { get; set; } = 0;
    }

    public static class FileScanner
    {
        private static readonly string[] MonthNames = {
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        };

        private static readonly Regex HeaderDateRegex = new Regex(@"([A-Za-z]{3})\s+(\d{1,2}),\s+(\d{2})", RegexOptions.Compiled);
        // CombatEvents.txt header: "COMBAT EVENTS FOR 12/27/41" (MM/DD/YY, stamped with the NEXT day).
        private static readonly Regex NumericHeaderDateRegex = new Regex(@"FOR\s+(\d{1,2})/(\d{1,2})/(\d{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex DateCodeRegex = new Regex(@"(\d{6})", RegexOptions.Compiled);

        public static string FormatYYMMDD(string yymmdd)
        {
            if (string.IsNullOrEmpty(yymmdd) || yymmdd.Length != 6) return yymmdd ?? string.Empty;
            string yy = yymmdd.Substring(0, 2);
            string mm = yymmdd.Substring(2, 2);
            string dd = yymmdd.Substring(4, 2);

            if (!int.TryParse(mm, out int mIdx) || mIdx < 1 || mIdx > 12)
            {
                return yymmdd;
            }

            return $"{dd}/{mm}/{yy}";
        }

        public static string FormatShortDate(string yymmdd)
        {
            if (string.IsNullOrEmpty(yymmdd) || yymmdd.Length != 6) return yymmdd ?? string.Empty;
            string yy = yymmdd.Substring(0, 2);
            string mm = yymmdd.Substring(2, 2);
            string dd = yymmdd.Substring(4, 2);

            if (!int.TryParse(mm, out int mIdx) || mIdx < 1 || mIdx > 12)
            {
                return yymmdd;
            }

            return $"{dd}/{mm}/{yy}";
        }

        public static string ExtractDateFromPws(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;
            try
            {
                // Method 1: Scan decompressed outer blocks for block 550 (game header / meta)
                byte[] outer = PwsBlockScanner.DecompressOuter(filePath);
                foreach (var block in PwsBlockScanner.ScanBlocks(outer))
                {
                    if (block.BlockId == 550 && block.Payload != null && block.Payload.Length > 0)
                    {
                        string headerText = System.Text.Encoding.Latin1.GetString(block.Payload);
                        var m = Regex.Match(headerText, @"Date:\s*(\d{1,2})/(\d{1,2})/(\d{2})");
                        if (m.Success)
                        {
                            int mm = int.Parse(m.Groups[1].Value);
                            int dd = int.Parse(m.Groups[2].Value);
                            int yy = int.Parse(m.Groups[3].Value);
                            return $"{yy:D2}{mm:D2}{dd:D2}";
                        }
                    }
                }

                // Method 2: Fallback to full PwsFileParser parse
                var snapshot = PwsFileParser.Parse(filePath);
                if (!string.IsNullOrEmpty(snapshot.Meta.DateYYMMDD))
                {
                    return snapshot.Meta.DateYYMMDD;
                }
            }
            catch
            {
                // Fallback on corrupt or non-PWS file
            }
            return string.Empty;
        }

        public static string ExtractDateFromReportHeader(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;

            try
            {
                using var reader = new StreamReader(filePath);
                for (int i = 0; i < 10; i++)
                {
                    string? line = reader.ReadLine();
                    if (line == null) break;

                    var numeric = NumericHeaderDateRegex.Match(line);
                    if (numeric.Success && line.IndexOf("COMBAT EVENTS", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        int nm = int.Parse(numeric.Groups[1].Value);
                        int nd = int.Parse(numeric.Groups[2].Value);
                        int ny = int.Parse(numeric.Groups[3].Value);
                        try
                        {
                            var stamped = new DateTime(1900 + ny, nm, nd).AddDays(-1);
                            return $"{stamped.Year % 100:D2}{stamped.Month:D2}{stamped.Day:D2}";
                        }
                        catch { /* fall through to other formats */ }
                    }

                    int forPos = line.IndexOf("FOR ", StringComparison.OrdinalIgnoreCase);
                    if (forPos != -1)
                    {
                        string datePart = line.Substring(forPos + 4);
                        var match = HeaderDateRegex.Match(datePart);
                        if (match.Success)
                        {
                            string mon = match.Groups[1].Value;
                            string day = match.Groups[2].Value.PadLeft(2, '0');
                            string yr = match.Groups[3].Value;

                            string mm = mon.ToLowerInvariant() switch
                            {
                                "jan" => "01",
                                "feb" => "02",
                                "mar" => "03",
                                "apr" => "04",
                                "may" => "05",
                                "jun" => "06",
                                "jul" => "07",
                                "aug" => "08",
                                "sep" => "09",
                                "oct" => "10",
                                "nov" => "11",
                                "dec" => "12",
                                _ => "12"
                            };

                            return $"{yr}{mm}{day}";
                        }
                    }
                }
            }
            catch
            {
                // Fallback on error
            }

            return string.Empty;
        }

        private static readonly string[] IgnoredDirNames = { "autosave", "safe arch", "_to_delete", "_archive" };
        private static readonly Regex JapaneseSideFile = new Regex(@"^j(sigint|operationsreport|combatreport|combat_events|combatevents)", RegexOptions.Compiled);
        private static readonly string[] IgnoredFileNames = { "updatereport.txt", "archive.rar", "placeholder.txt" };

        /// <summary>True for game-folder clutter that must never be ingested.</summary>
        internal static bool IsIgnoredPath(string scanRoot, string filePath)
        {
            string name = Path.GetFileName(filePath).ToLowerInvariant();
            if (Array.IndexOf(IgnoredFileNames, name) >= 0) return true;
            // j* reports (jsigint, joperationsreport, ...) are the Japanese side's files - Allied-only v1 never ingests them.
            if (JapaneseSideFile.IsMatch(name)) return true;

            string rel;
            try { rel = Path.GetRelativePath(scanRoot, Path.GetDirectoryName(filePath) ?? scanRoot); }
            catch { return false; }
            foreach (var seg in rel.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries))
                if (Array.IndexOf(IgnoredDirNames, seg.ToLowerInvariant()) >= 0) return true;
            return false;
        }

        /// <summary>
        /// The game writes undated live files at the SAVE root that are identical to the newest archived day.
        /// For every (type, date) keep a single file, preferring the one with the date in its file name (archive).
        /// </summary>
        private static void DeduplicateLiveVsArchive(IngestionManifest manifest)
        {
            var keep = new List<DiscoveredFile>();
            foreach (var group in manifest.Files.GroupBy(f => (f.FileType, f.DateYYMMDD)))
            {
                var list = group.ToList();
                if (list.Count == 1 || string.IsNullOrEmpty(group.Key.DateYYMMDD) || group.Key.FileType == "PWS_SAVE")
                {
                    keep.AddRange(list);
                    continue;
                }
                var dated = list.FirstOrDefault(f => DateCodeRegex.IsMatch(f.FileName)) ?? list[0];
                keep.Add(dated);
            }
            manifest.Files = keep
                .OrderBy(f => f.DateYYMMDD, StringComparer.Ordinal)
                .ThenBy(f => f.FileType, StringComparer.Ordinal)
                .ToList();
        }

        public static IngestionManifest ScanDirectories(IEnumerable<string> directories, ScanOptions? options = null)
        {
            options ??= new ScanOptions();
            string? campaignPws = string.IsNullOrWhiteSpace(options.CampaignPwsPath) ? null : Path.GetFullPath(options.CampaignPwsPath);
            var manifest = new IngestionManifest();
            var visitedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenFileSignatures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dir in directories)
            {
                try
                {
                    if (!Directory.Exists(dir)) continue;

                    // Collect files from dir and all subdirectories
                    var filePaths = new List<string>();
                    try
                    {
                        filePaths.AddRange(Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories));
                    }
                    catch
                    {
                        // Fallback to top directory only if subdirectories are inaccessible
                        filePaths.AddRange(Directory.GetFiles(dir));
                    }

                    foreach (var fpath in filePaths)
                    {
                        if (IsIgnoredPath(dir, fpath)) continue;
                        string canonicalPath = Path.GetFullPath(fpath);
                        if (!visitedFiles.Add(canonicalPath)) continue;

                        string fname = Path.GetFileName(fpath);
                        string lowerName = fname.ToLowerInvariant();

                        // Avoid processing duplicate files with identical names and file sizes from overlapping paths
                        try
                        {
                            long fileLength = new FileInfo(fpath).Length;
                            string signature = $"{lowerName}:{fileLength}";
                            if (!seenFileSignatures.Add(signature)) continue;
                        }
                        catch { }

                        string ftype = string.Empty;
                        string dateCode = string.Empty;

                        var dateMatch = DateCodeRegex.Match(fname);
                        if (dateMatch.Success)
                        {
                            dateCode = dateMatch.Groups[1].Value;
                        }

                        if (string.IsNullOrEmpty(dateCode) && lowerName.EndsWith(".txt"))
                        {
                            dateCode = ExtractDateFromReportHeader(fpath);
                        }
                        // Rule: only the ONE .pws that belongs to the campaign is ingested
                        // (the game overwrites it each turn). Other wpae*.pws are game-special or unused.
                        bool isEligiblePws = false;
                        if (lowerName.EndsWith(".pws"))
                        {
                            isEligiblePws = options.AllowAnyPws ||
                                (campaignPws != null && string.Equals(canonicalPath, campaignPws, StringComparison.OrdinalIgnoreCase));

                            if (isEligiblePws)
                            {
                                dateCode = ExtractDateFromPws(fpath);   // never trust digits in a .pws file name
                            }
                        }

                        if (lowerName.Contains("sigint"))
                        {
                            ftype = "SIGINT";
                        }
                        else if (lowerName.Contains("operationsreport") || lowerName.Contains("ops") || lowerName.Contains("operations"))
                        {
                            ftype = "OPERATIONS";
                        }
                        else if (lowerName.Contains("combatreport"))
                        {
                            ftype = "COMBAT_REPORT";
                        }
                        else if (lowerName.Contains("combat_events") || lowerName.Contains("combatevents"))
                        {
                            ftype = "COMBAT_EVENTS";
                        }
                        else if (isEligiblePws)
                        {
                            ftype = "PWS_SAVE";
                        }

                        if (!string.IsNullOrEmpty(ftype))
                        {
                            manifest.Files.Add(new DiscoveredFile
                            {
                                FileName = fname,
                                FullPath = fpath,
                                FileType = ftype,
                                DateYYMMDD = dateCode
                            });

                            if (!string.IsNullOrEmpty(dateCode))
                            {
                                manifest.UniqueDates.Add(dateCode);
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore unreadable paths gracefully
                }
            }

            DeduplicateLiveVsArchive(manifest);

            manifest.UniqueDates.Clear();
            foreach (var f in manifest.Files)
                if (!string.IsNullOrEmpty(f.DateYYMMDD)) manifest.UniqueDates.Add(f.DateYYMMDD);

            manifest.DaysProcessed = manifest.UniqueDates.Count;
            if (manifest.UniqueDates.Count > 0)
            {
                manifest.LatestDate = manifest.UniqueDates.Max()!;
                manifest.FormattedDate = FormatYYMMDD(manifest.LatestDate);
            }
            else
            {
                manifest.LatestDate = "411207";
                manifest.FormattedDate = "07/12/41";
            }

            return manifest;
        }
    }
}
