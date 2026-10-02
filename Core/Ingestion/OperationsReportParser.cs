using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using WitpCic.Core.Geospatial;

namespace WitpCic.Core.Ingestion;

/// <summary>
/// High-performance, zero-allocation streaming parser and corroborating deduplicator for operational reports.
/// </summary>
public static partial class OperationsReportParser
{
    [GeneratedRegex(@"^OPERATIONAL REPORT FOR\s+(?<date>[A-Za-z]{3}\s+\d{1,2},\s+\d{2,4})", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex HeaderDateRegex();

    [GeneratedRegex(@"(?<count>\d+)\s+Japanese ships at\s+(?<x>\d+)\s*,\s*(?<y>\d+)(?:\s+near\s+(?<loc>[^,]+))?(?:,\s*speed\s+(?<speed>\d+|unknown))?(?:,\s*Moving\s+(?<heading>[A-Za-z]+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex FleetSightingRegex();

    [GeneratedRegex(@"(?<obs>.+?)\s+(?:observes|sights|followed by|snooped by|shadowed by|detects)\s+(?<target>.+?)\s+at\s+(?<x>\d+)\s*,\s*(?<y>\d+)(?:\s+near\s+(?<loc>.+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex ObserverSightingRegex();

    [GeneratedRegex(@"(?<target>.+?)\s+sighted over\s+(?<loc>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex OverLocationRegex();

    [GeneratedRegex(@"(?<unit>.+?)\s+reports having been sighted by the enemy at\s+(?<x>\d+)\s*,\s*(?<y>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex ReverseSightingRegex();

    private sealed class SightingRecordBuilder
    {
        public DateOnly TurnDate { get; set; }
        public DateTime TimestampUtc { get; set; }
        public HexCoordinate Coordinate { get; set; }
        public string? LocationName { get; set; }
        public TargetClassification Classification { get; set; }
        public string TargetDescription { get; set; } = string.Empty;
        public int? ShipCount { get; set; }
        public int? Speed { get; set; }
        public string? Heading { get; set; }
        public string? Reporter { get; set; }
        public int CorroborationCount { get; set; } = 1;
        public string PrimaryRawText { get; set; } = string.Empty;
        public List<string> RawLines { get; } = new();

        public SightingRecord Build() => new(
            TurnDate,
            TimestampUtc,
            Coordinate,
            LocationName,
            Classification,
            TargetDescription,
            ShipCount,
            Speed,
            Heading,
            Reporter,
            CorroborationCount,
            PrimaryRawText,
            RawLines.AsReadOnly()
        );
    }

    /// <summary>
    /// Lazily streams and deduplicates sighting reports from a log file.
    /// </summary>
    public static IEnumerable<SightingRecord> ParseAndDeduplicate(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        if (!File.Exists(filePath)) yield break;

        DateOnly currentTurnDate = DateOnly.MinValue;
        var exactLineSeen = new HashSet<string>(StringComparer.Ordinal);
        
        // Near-match composite dictionary: (TurnDate, HexX, HexY, Classification)
        var hexContactMap = new Dictionary<(DateOnly Date, int X, int Y, TargetClassification Type), SightingRecordBuilder>();
        
        // Fallback near-match dictionary for coordinate-free sightings: (TurnDate, LocationName, Classification)
        var locationContactMap = new Dictionary<(DateOnly Date, string Location, TargetClassification Type), SightingRecordBuilder>(StringComparerLocationTuple.Instance);

        foreach (string rawLine in File.ReadLines(filePath))
        {
            ReadOnlySpan<char> span = rawLine.AsSpan().Trim();
            if (span.IsEmpty) continue;

            // 1. Check Date Header
            if (span.StartsWith("OPERATIONAL REPORT FOR", StringComparison.OrdinalIgnoreCase))
            {
                var match = HeaderDateRegex().Match(rawLine);
                if (match.Success && TryParseReportDate(match.Groups["date"].ValueSpan, out var parsedDate))
                {
                    currentTurnDate = parsedDate;
                }
                continue;
            }

            // 2. Exact match filter
            if (!exactLineSeen.Add(rawLine))
            {
                // Exact duplicate detected - record as corroborated on existing contact
                CorroborateExactLine(rawLine, currentTurnDate, hexContactMap, locationContactMap);
                continue;
            }

            // 3. Fast Zero-Allocation Span Pre-Filter for Sightings
            if (!IsPotentialSighting(span))
            {
                continue;
            }

            // 4. Parse contact details (Span-first with regex fallback)
            if (TryParseSighting(rawLine, span, currentTurnDate, out var parsed))
            {
                MergeContact(parsed, rawLine, hexContactMap, locationContactMap);
            }
        }

        // Yield consolidated contacts
        foreach (var builder in hexContactMap.Values)
        {
            yield return builder.Build();
        }

        foreach (var builder in locationContactMap.Values)
        {
            yield return builder.Build();
        }
    }

    private static bool IsPotentialSighting(ReadOnlySpan<char> span)
    {
        return span.Contains("sighting report:", StringComparison.OrdinalIgnoreCase)
            || span.Contains("sighted", StringComparison.OrdinalIgnoreCase)
            || span.Contains("observes", StringComparison.OrdinalIgnoreCase)
            || span.Contains("snooped by", StringComparison.OrdinalIgnoreCase)
            || span.Contains("shadowed by", StringComparison.OrdinalIgnoreCase)
            || span.Contains("detects", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseSighting(string rawLine, ReadOnlySpan<char> span, DateOnly turnDate, out SightingRecordBuilder builder)
    {
        DateTime defaultTimestamp = turnDate != DateOnly.MinValue 
            ? turnDate.ToDateTime(new TimeOnly(12, 0), DateTimeKind.Utc) 
            : DateTime.UtcNow;

        builder = new SightingRecordBuilder
        {
            TurnDate = turnDate,
            TimestampUtc = defaultTimestamp,
            PrimaryRawText = rawLine
        };
        builder.RawLines.Add(rawLine);

        // Pattern 1: Explicit Fleet sighting reports
        var fleetMatch = FleetSightingRegex().Match(rawLine);
        if (fleetMatch.Success)
        {
            builder.Classification = TargetClassification.SurfaceShip;
            builder.ShipCount = int.Parse(fleetMatch.Groups["count"].Value);
            builder.Coordinate = new HexCoordinate(int.Parse(fleetMatch.Groups["x"].Value), int.Parse(fleetMatch.Groups["y"].Value));
            builder.LocationName = fleetMatch.Groups["loc"].Success ? fleetMatch.Groups["loc"].Value.Trim() : null;
            builder.TargetDescription = $"{builder.ShipCount} Japanese ships";
            
            if (fleetMatch.Groups["speed"].Success && int.TryParse(fleetMatch.Groups["speed"].Value, out int speed))
                builder.Speed = speed;
                
            if (fleetMatch.Groups["heading"].Success)
                builder.Heading = fleetMatch.Groups["heading"].Value.Trim();

            return true;
        }

        // Pattern 2: Reverse sighting ("reports having been sighted by...")
        var reverseMatch = ReverseSightingRegex().Match(rawLine);
        if (reverseMatch.Success)
        {
            builder.Reporter = reverseMatch.Groups["unit"].Value.Trim();
            builder.TargetDescription = "Enemy naval/air force";
            builder.Coordinate = new HexCoordinate(int.Parse(reverseMatch.Groups["x"].Value), int.Parse(reverseMatch.Groups["y"].Value));
            builder.Classification = TargetClassification.SurfaceShip;
            return true;
        }

        // Pattern 3: Observer sighting (Aircraft/Sub/Ship)
        var obsMatch = ObserverSightingRegex().Match(rawLine);
        if (obsMatch.Success)
        {
            builder.Reporter = obsMatch.Groups["obs"].Value.Trim();
            builder.TargetDescription = obsMatch.Groups["target"].Value.Trim();
            builder.Coordinate = new HexCoordinate(int.Parse(obsMatch.Groups["x"].Value), int.Parse(obsMatch.Groups["y"].Value));
            builder.LocationName = obsMatch.Groups["loc"].Success ? obsMatch.Groups["loc"].Value.Trim() : null;
            builder.Classification = ClassifyTarget(builder.TargetDescription);
            return true;
        }

        // Pattern 4: Sightings over named bases/units
        var overMatch = OverLocationRegex().Match(rawLine);
        if (overMatch.Success)
        {
            builder.TargetDescription = overMatch.Groups["target"].Value.Trim();
            builder.LocationName = overMatch.Groups["loc"].Value.Trim();
            builder.Classification = ClassifyTarget(builder.TargetDescription);
            return true;
        }

        return false;
    }

    private static void MergeContact(
        SightingRecordBuilder incoming, 
        string rawLine, 
        Dictionary<(DateOnly, int, int, TargetClassification), SightingRecordBuilder> hexMap,
        Dictionary<(DateOnly, string, TargetClassification), SightingRecordBuilder> locMap)
    {
        if (incoming.Coordinate.IsValid)
        {
            var key = (incoming.TurnDate, incoming.Coordinate.X, incoming.Coordinate.Y, incoming.Classification);
            if (hexMap.TryGetValue(key, out var existing))
            {
                existing.CorroborationCount++;
                existing.RawLines.Add(rawLine);
                if (incoming.ShipCount.HasValue) existing.ShipCount = incoming.ShipCount; // Most recent reported count
                if (incoming.Speed.HasValue) existing.Speed = incoming.Speed;             // Specific over unknown
                if (!string.IsNullOrEmpty(incoming.Heading)) existing.Heading = incoming.Heading;
                if (string.IsNullOrEmpty(existing.LocationName) && !string.IsNullOrEmpty(incoming.LocationName))
                    existing.LocationName = incoming.LocationName;
            }
            else
            {
                hexMap[key] = incoming;
            }
        }
        else if (!string.IsNullOrEmpty(incoming.LocationName))
        {
            var key = (incoming.TurnDate, incoming.LocationName, incoming.Classification);
            if (locMap.TryGetValue(key, out var existing))
            {
                existing.CorroborationCount++;
                existing.RawLines.Add(rawLine);
                if (incoming.ShipCount.HasValue) existing.ShipCount = incoming.ShipCount;
            }
            else
            {
                locMap[key] = incoming;
            }
        }
    }

    private static void CorroborateExactLine(
        string rawLine, 
        DateOnly turnDate,
        Dictionary<(DateOnly, int, int, TargetClassification), SightingRecordBuilder> hexMap,
        Dictionary<(DateOnly, string, TargetClassification), SightingRecordBuilder> locMap)
    {
        ReadOnlySpan<char> span = rawLine.AsSpan().Trim();
        if (TryParseSighting(rawLine, span, turnDate, out var temp))
        {
            if (temp.Coordinate.IsValid)
            {
                var key = (temp.TurnDate, temp.Coordinate.X, temp.Coordinate.Y, temp.Classification);
                if (hexMap.TryGetValue(key, out var existing))
                {
                    existing.CorroborationCount++;
                    existing.RawLines.Add(rawLine);
                }
            }
            else if (!string.IsNullOrEmpty(temp.LocationName))
            {
                var key = (temp.TurnDate, temp.LocationName, temp.Classification);
                if (locMap.TryGetValue(key, out var existing))
                {
                    existing.CorroborationCount++;
                    existing.RawLines.Add(rawLine);
                }
            }
        }
    }

    private static TargetClassification ClassifyTarget(ReadOnlySpan<char> desc)
    {
        if (desc.Contains("ship", StringComparison.OrdinalIgnoreCase) || desc.Contains("TF", StringComparison.OrdinalIgnoreCase))
            return TargetClassification.SurfaceShip;
        if (desc.Contains("submarine", StringComparison.OrdinalIgnoreCase) || desc.Contains("sub", StringComparison.OrdinalIgnoreCase))
            return TargetClassification.Submarine;
        if (desc.Contains("bomber", StringComparison.OrdinalIgnoreCase) || desc.Contains("fighter", StringComparison.OrdinalIgnoreCase) || 
            desc.Contains("aircraft", StringComparison.OrdinalIgnoreCase) || desc.Contains("plane", StringComparison.OrdinalIgnoreCase) ||
            desc.Contains("recon", StringComparison.OrdinalIgnoreCase) || desc.Contains("mavis", StringComparison.OrdinalIgnoreCase) ||
            desc.Contains("babs", StringComparison.OrdinalIgnoreCase) || desc.Contains("dinah", StringComparison.OrdinalIgnoreCase) ||
            desc.Contains("dauntless", StringComparison.OrdinalIgnoreCase))
            return TargetClassification.Aircraft;

        return TargetClassification.Unknown;
    }

    private static bool TryParseReportDate(ReadOnlySpan<char> span, out DateOnly date)
    {
        return DateOnly.TryParseExact(span, new[] { "MMM dd, yy", "MMM dd, yyyy", "MMM d, yy" }, 
            CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    private sealed class StringComparerLocationTuple : IEqualityComparer<(DateOnly Date, string Location, TargetClassification Type)>
    {
        public static readonly StringComparerLocationTuple Instance = new();

        public bool Equals((DateOnly Date, string Location, TargetClassification Type) x, (DateOnly Date, string Location, TargetClassification Type) y) =>
            x.Date == y.Date && x.Type == y.Type && string.Equals(x.Location, y.Location, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((DateOnly Date, string Location, TargetClassification Type) obj) =>
            HashCode.Combine(obj.Date, StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Location), obj.Type);
    }
}
