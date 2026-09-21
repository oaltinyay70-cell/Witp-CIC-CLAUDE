using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MUTHUR6000.Core.Ingestion
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
        public string FormattedDate { get; set; } = "Dec 07, 1941";
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

            string mStr = MonthNames[mIdx - 1];
            string yStr = "19" + yy;
            return $"{mStr} {dd}, {yStr}";
        }

        public static string FormatShortDate(string yymmdd)
        {
            if (string.IsNullOrEmpty(yymmdd) || yymmdd.Length != 6) return yymmdd ?? string.Empty;
            string mm = yymmdd.Substring(2, 2);
            string dd = yymmdd.Substring(4, 2);

            if (!int.TryParse(mm, out int mIdx) || mIdx < 1 || mIdx > 12)
            {
                return yymmdd;
            }

            string mStr = MonthNames[mIdx - 1];
            return $"{mStr} {dd}";
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

            foreach (var dir in directories)
            {
                try
                {
                    if (!Directory.Exists(dir)) continue;

                    var filePaths = Directory.GetFiles(dir);
                    foreach (var fpath in filePaths)
                    {
                        string fname = Path.GetFileName(fpath);
                        string lowerName = fname.ToLowerInvariant();
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
                        else if (lowerName.EndsWith(".pws"))
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
                manifest.FormattedDate = "Dec 07, 1941";
            }

            return manifest;
        }
    }
}
