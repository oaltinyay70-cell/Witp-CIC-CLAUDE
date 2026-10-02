using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WitpCic.Core.Geospatial;

namespace WitpCic.Core.Analytics.AirDenial;

public enum AircraftThreatClass
{
    TorpedoOrDiveBomber, // 1.5x
    FighterOrCap,        // 1.2x
    LevelBomber,         // 1.0x
    ReconOrFloatPlane    // 0.6x
}

public sealed record AirOverflightEvent(
    HexCoordinate Coordinate,
    AircraftThreatClass ThreatClass,
    string Description,
    DateTime OccurredAtUtc
);

public interface IAirDenialService
{
    float GetDenialScore(HexCoordinate coordinate, DateTime asOfUtc);
    bool IsAreaDenied(HexCoordinate coordinate, DateTime asOfUtc, float threshold = 0.70f);
    void RecordOverflight(AirOverflightEvent overflight);
}

public sealed class AirDenialHeatmapService : IAirDenialService
{
    private readonly ConcurrentDictionary<HexCoordinate, List<AirOverflightEvent>> _heatmap = new();
    private static readonly TimeSpan ThreatHalfLife = TimeSpan.FromHours(8);

    public void RecordOverflight(AirOverflightEvent overflight)
    {
        if (!overflight.Coordinate.IsValid) return;

        var list = _heatmap.GetOrAdd(overflight.Coordinate, _ => new List<AirOverflightEvent>());
        lock (list)
        {
            list.Add(overflight);
        }
    }

    public float GetDenialScore(HexCoordinate coordinate, DateTime asOfUtc)
    {
        if (!_heatmap.TryGetValue(coordinate, out var list)) return 0.0f;

        List<AirOverflightEvent> snapshot;
        lock (list)
        {
            snapshot = new List<AirOverflightEvent>(list);
        }

        double totalScore = 0.0;
        foreach (var ev in snapshot)
        {
            TimeSpan age = asOfUtc - ev.OccurredAtUtc;
            if (age < TimeSpan.Zero) age = TimeSpan.Zero;

            // Half-life exponential decay: e^(-ln(2) * age / halfLife)
            double halfLivesPassed = age.TotalSeconds / ThreatHalfLife.TotalSeconds;
            double decay = Math.Pow(0.5, halfLivesPassed);

            double weight = ev.ThreatClass switch
            {
                AircraftThreatClass.TorpedoOrDiveBomber => 1.5,
                AircraftThreatClass.FighterOrCap => 1.2,
                AircraftThreatClass.LevelBomber => 1.0,
                AircraftThreatClass.ReconOrFloatPlane => 0.6,
                _ => 1.0
            };

            totalScore += weight * decay * 0.35; // Scaling factor
        }

        return (float)Math.Clamp(totalScore, 0.0, 1.0);
    }

    public bool IsAreaDenied(HexCoordinate coordinate, DateTime asOfUtc, float threshold = 0.70f)
    {
        return GetDenialScore(coordinate, asOfUtc) >= threshold;
    }
}
