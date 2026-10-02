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

        public static IngestionManifest ScanDirectories(IEnumerable<string> directories)
        {
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
                        // User rule: Only process .pws save files located under the canonical SAVE directory
                        // (e.g. C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE) or test/sample directories.
                        bool isEligiblePws = false;
                        if (lowerName.EndsWith(".pws"))
                        {
                            string dirOfFile = Path.GetDirectoryName(canonicalPath) ?? string.Empty;
                            isEligiblePws = dirOfFile.EndsWith(@"\SAVE", StringComparison.OrdinalIgnoreCase) ||
                                            dirOfFile.EndsWith(@"/SAVE", StringComparison.OrdinalIgnoreCase) ||
                                            dirOfFile.IndexOf(@"\SAVE\", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                            dirOfFile.IndexOf(@"/SAVE/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                            canonicalPath.IndexOf("sample_data", StringComparison.OrdinalIgnoreCase) >= 0;

                            if (isEligiblePws && string.IsNullOrEmpty(dateCode))
                            {
                                dateCode = ExtractDateFromPws(fpath);
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
