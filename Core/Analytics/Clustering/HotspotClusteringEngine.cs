using System;
using System.Collections.Generic;
using System.Linq;
using WitpCic.Core.Geospatial;
using WitpCic.Core.Ingestion;

namespace WitpCic.Core.Analytics.Clustering;

public enum HotspotThreatLevel
{
    Low,
    Moderate,
    High,
    Critical
}

public sealed record OperationalHotspot(
    HexCoordinate PrimaryHex,
    IReadOnlyList<HexCoordinate> ClusteredHexes,
    TargetClassification PrimaryClassification,
    int TotalReports,
    int MaxReportedShips,
    double AverageSpeed,
    HotspotThreatLevel ThreatLevel,
    string Description,
    IReadOnlyList<SightingRecord> Sightings
);

public sealed class HotspotDetectedEventArgs : EventArgs
{
    public OperationalHotspot Hotspot { get; }
    public DateTime DetectedAtUtc { get; }

    public HotspotDetectedEventArgs(OperationalHotspot hotspot)
    {
        Hotspot = hotspot;
        DetectedAtUtc = DateTime.UtcNow;
    }
}

/// <summary>
/// Custom IEqualityComparer for fuzzy spatial clustering within a designated hex radius.
/// </summary>
public sealed class FuzzyHexRadiusComparer : IEqualityComparer<HexCoordinate>
{
    private readonly int _radius;

    public FuzzyHexRadiusComparer(int radius = 1)
    {
        _radius = radius;
    }

    public bool Equals(HexCoordinate x, HexCoordinate y)
    {
        return x.GetDistanceTo(y) <= _radius;
    }

    public int GetHashCode(HexCoordinate obj)
    {
        // For fuzzy clustering, return uniform bucket or coordinate region to force Equals evaluation
        return 0;
    }
}

public sealed class HotspotClusteringEngine
{
    public int NavalBuildupShipThreshold { get; set; } = 6;
    public int NavalBuildupReportThreshold { get; set; } = 4;
    public int PersistentAswReportThreshold { get; set; } = 3;

    public event EventHandler<HotspotDetectedEventArgs>? HotspotDetected;

    /// <summary>
    /// Clusters sighting reports across exact hexes and adjacent spatial-temporal radii.
    /// </summary>
    public IReadOnlyList<OperationalHotspot> AnalyzeHotspots(IEnumerable<SightingRecord> sightings, int spatialRadius = 1)
    {
        var sightingList = sightings.Where(s => s.Coordinate.IsValid).ToList();
        if (sightingList.Count == 0) return Array.Empty<OperationalHotspot>();

        var hotspots = new List<OperationalHotspot>();

        // 1. Exact Hex Grouping via LINQ
        var exactHexGroups = sightingList
            .GroupBy(s => s.Coordinate)
            .Select(g => new
            {
                Coordinate = g.Key,
                Sightings = g.ToList(),
                TotalReports = g.Sum(s => s.CorroborationCount),
                MaxShips = g.Max(s => s.ShipCount ?? 0),
                AvgSpeed = g.Where(s => s.Speed.HasValue).Select(s => (double)s.Speed!.Value).DefaultIfEmpty(0.0).Average(),
                PrimaryType = g.GroupBy(s => s.Classification).OrderByDescending(cg => cg.Count()).First().Key
            })
            .ToList();

        // 2. Spatial Radius Aggregation (DBSCAN-style neighbor linking)
        var visited = new HashSet<HexCoordinate>();

        foreach (var group in exactHexGroups)
        {
            if (visited.Contains(group.Coordinate)) continue;

            // Find all neighboring hexes within radius
            var clusterSightings = sightingList
                .Where(s => s.Coordinate.GetDistanceTo(group.Coordinate) <= spatialRadius)
                .ToList();

            var clusterHexes = clusterSightings
                .Select(s => s.Coordinate)
                .Distinct()
                .ToList();

            foreach (var hex in clusterHexes)
            {
                visited.Add(hex);
            }

            int totalReports = clusterSightings.Sum(s => s.CorroborationCount);
            int maxShips = clusterSightings.Max(s => s.ShipCount ?? 0);
            double avgSpeed = clusterSightings
                .Where(s => s.Speed.HasValue)
                .Select(s => (double)s.Speed!.Value)
                .DefaultIfEmpty(0.0)
                .Average();

            var primaryClassification = clusterSightings
                .GroupBy(s => s.Classification)
                .OrderByDescending(cg => cg.Count())
                .First().Key;

            // Evaluate threat level and anomalies
            var threatLevel = EvaluateThreatLevel(primaryClassification, maxShips, totalReports);
            string description = BuildClusterDescription(primaryClassification, group.Coordinate, maxShips, totalReports, avgSpeed);

            var hotspot = new OperationalHotspot(
                group.Coordinate,
                clusterHexes,
                primaryClassification,
                totalReports,
                maxShips,
                avgSpeed,
                threatLevel,
                description,
                clusterSightings.AsReadOnly()
            );

            hotspots.Add(hotspot);

            // Trigger Event if thresholds met
            if (IsHotspotAlertTriggered(hotspot))
            {
                HotspotDetected?.Invoke(this, new HotspotDetectedEventArgs(hotspot));
            }
        }

        return hotspots.AsReadOnly();
    }

    private HotspotThreatLevel EvaluateThreatLevel(TargetClassification classification, int maxShips, int totalReports)
    {
        if (classification == TargetClassification.SurfaceShip && (maxShips >= 8 || totalReports >= 10))
            return HotspotThreatLevel.Critical;
        if (classification == TargetClassification.Submarine && totalReports >= 3)
            return HotspotThreatLevel.High;
        if (maxShips >= 4 || totalReports >= 4)
            return HotspotThreatLevel.Moderate;

        return HotspotThreatLevel.Low;
    }

    private string BuildClusterDescription(TargetClassification classification, HexCoordinate coordinate, int maxShips, int totalReports, double avgSpeed)
    {
        return classification switch
        {
            TargetClassification.SurfaceShip => $"Major Naval Assembly at {coordinate}: up to {maxShips} ships ({totalReports} sightings, avg speed {avgSpeed:F1} kts)",
            TargetClassification.Submarine => $"Persistent ASW Contact Zone at {coordinate}: {totalReports} correlated underwater detections",
            TargetClassification.Aircraft => $"Intense Air Corridor at {coordinate}: {totalReports} overflights logged",
            _ => $"Concentrated Activity at {coordinate} ({totalReports} reports)"
        };
    }

    private bool IsHotspotAlertTriggered(OperationalHotspot hotspot)
    {
        if (hotspot.PrimaryClassification == TargetClassification.SurfaceShip &&
            (hotspot.MaxReportedShips >= NavalBuildupShipThreshold || hotspot.TotalReports >= NavalBuildupReportThreshold))
        {
            return true;
        }

        if (hotspot.PrimaryClassification == TargetClassification.Submarine &&
            hotspot.TotalReports >= PersistentAswReportThreshold)
        {
            return true;
        }

        return false;
    }
}
