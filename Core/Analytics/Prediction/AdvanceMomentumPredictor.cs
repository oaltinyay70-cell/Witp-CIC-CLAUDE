using System;
using System.Collections.Generic;
using System.Numerics;
using WitpCic.Core.Geospatial;

namespace WitpCic.Core.Analytics.Prediction;

public sealed record PredictedThreat(
    string TargetBaseName,
    HexCoordinate Coordinate,
    double Probability,
    double AdvanceAlignmentScore,
    int EstimatedHoursToArrival,
    string ThreatLevel,
    string StrategicRationale
);

public interface IPredictionService
{
    IEnumerable<PredictedThreat> PredictNextTargets(
        HexCoordinate currentAdvanceHex, 
        Vector2 momentumVector, 
        int topK = 5);
}

public sealed class AdvanceMomentumPredictor : IPredictionService
{
    private readonly List<(string Name, HexCoordinate Coord, int StrategicTier)> _strategicBases = new();

    public void RegisterTargetBase(string name, HexCoordinate coord, int strategicTier = 1)
    {
        _strategicBases.Add((name, coord, strategicTier));
    }

    public IEnumerable<PredictedThreat> PredictNextTargets(
        HexCoordinate currentAdvanceHex, 
        Vector2 momentumVector, 
        int topK = 5)
    {
        if (!currentAdvanceHex.IsValid || momentumVector == Vector2.Zero)
        {
            return Array.Empty<PredictedThreat>();
        }

        Vector2 normMomentum = Vector2.Normalize(momentumVector);
        var scoredList = new List<PredictedThreat>();

        foreach (var b in _strategicBases)
        {
            int distanceHex = currentAdvanceHex.GetDistanceTo(b.Coord);
            if (distanceHex <= 0 || distanceHex > 20) continue;

            Vector2 toTarget = Vector2.Normalize(b.Coord.ToVector2() - currentAdvanceHex.ToVector2());
            float alignment = Vector2.Dot(normMomentum, toTarget); // [-1.0, 1.0]

            if (alignment < 0.2f) continue; // Behind or orthogonal to advance axis

            // Scoring formula: Alignment (40%) + Distance (30%) + Strategic Tier (30%)
            double distanceFactor = Math.Max(0.1, 1.0 - (distanceHex / 20.0));
            double tierFactor = Math.Min(1.0, b.StrategicTier / 3.0);

            double prob = (alignment * 0.40) + (distanceFactor * 0.30) + (tierFactor * 0.30);
            prob = Math.Clamp(prob, 0.05, 0.98);

            int etaHours = distanceHex * 4; // Approx 4h per hex

            string threatLevel = prob switch
            {
                >= 0.80 => "CRITICAL (>80%) - Imminent Target",
                >= 0.60 => "HIGH (>60%) - Projected Invasion Corridor",
                _ => "MODERATE (>40%) - Secondary Objective"
            };

            string rationale = $"Advance vector alignment ({alignment:F2}) toward {b.Name} ({distanceHex} hexes, ETA T+{etaHours}h).";

            scoredList.Add(new PredictedThreat(
                b.Name,
                b.Coord,
                prob,
                alignment,
                etaHours,
                threatLevel,
                rationale
            ));
        }

        return scoredList.OrderByDescending(p => p.Probability).Take(topK);
    }
}
