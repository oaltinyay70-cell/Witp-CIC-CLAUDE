using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WitpCic.Core.Geospatial;

namespace WitpCic.Core.State.Hazards;

public static class HazardDecayHelper
{
    private static readonly TimeSpan MinefieldHalfLife = TimeSpan.FromHours(72);
    private static readonly TimeSpan OilSlickHalfLife = TimeSpan.FromHours(12);

    public static int CalculateDecayedDensity(HazardState hazard)
    {
        var halfLife = hazard.Type == HazardType.OilSlickAsw ? OilSlickHalfLife : MinefieldHalfLife;
        double halfLives = hazard.Age.TotalSeconds / halfLife.TotalSeconds;
        double decayed = hazard.DensityPercent * Math.Pow(0.5, halfLives);
        return Math.Clamp((int)Math.Round(decayed), 0, 100);
    }
}

/// <summary>
/// Thread-safe spatial repository for persistent environmental hazards like minefields and oil slicks.
/// </summary>
public sealed class HazardStateRepository
{
    private readonly ConcurrentDictionary<HexCoordinate, List<HazardState>> _hazards = new();

    public void AddOrUpdateHazard(HazardState hazard)
    {
        if (!hazard.Coordinate.IsValid) return;

        var list = _hazards.GetOrAdd(hazard.Coordinate, _ => new List<HazardState>());
        lock (list)
        {
            list.RemoveAll(h => h.HazardId == hazard.HazardId);
            list.Add(hazard);
        }
    }

    public void ApplyMinesweepingClearing(HexCoordinate coordinate, int clearingPercent = 80)
    {
        if (_hazards.TryGetValue(coordinate, out var list))
        {
            lock (list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var h = list[i];
                    if (h.Type == HazardType.MinefieldDefensive || h.Type == HazardType.MinefieldOffensive)
                    {
                        int newDensity = Math.Max(0, h.DensityPercent - clearingPercent);
                        list[i] = h with { DensityPercent = newDensity };
                    }
                }
                list.RemoveAll(h => h.DensityPercent <= 5);
            }
        }
    }

    public IReadOnlyList<HazardState> GetHazardsAt(HexCoordinate coordinate)
    {
        if (_hazards.TryGetValue(coordinate, out var list))
        {
            lock (list)
            {
                return list.Select(h => h with { DensityPercent = HazardDecayHelper.CalculateDecayedDensity(h) })
                           .Where(h => h.DensityPercent > 5)
                           .ToList()
                           .AsReadOnly();
            }
        }
        return Array.Empty<HazardState>();
    }

    public bool IsHexHostileHazard(HexCoordinate coordinate, Faction movingUnitFaction)
    {
        var active = GetHazardsAt(coordinate);
        return active.Any(h => h.OwnerFaction != movingUnitFaction && (h.Type == HazardType.MinefieldDefensive || h.Type == HazardType.MinefieldOffensive));
    }
}
