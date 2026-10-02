using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WitpCic.Core.Analytics.EntityResolution;
using WitpCic.Core.Geospatial;

namespace WitpCic.Core.State;

public sealed record WorldStateSnapshot(
    DateOnly TurnDate,
    DateTime SnapshotTimestampUtc,
    IReadOnlyList<UnitStateSnapshot> Units,
    IReadOnlyList<string> LogisticalAlerts,
    IReadOnlyDictionary<string, int> BaseFortifications
);

public sealed record UnitStateSnapshot(
    Guid Id,
    string Name,
    EntityType Type,
    UnitLifecycleState LifecycleState,
    HexCoordinate Coordinate,
    int FuelLevel,
    double LossProbability,
    DateTime LastUpdatedUtc
);

/// <summary>
/// Thread-safe reactive repository for the global World State with JSON snapshotting.
/// </summary>
public sealed class WorldStateRepository
{
    private readonly ConcurrentDictionary<Guid, UnitState> _masterUnits = new();
    private readonly ConcurrentDictionary<string, int> _baseFortifications = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _dailyAlerts = new();
    private readonly object _alertLock = new();

    public IReadOnlyCollection<UnitState> Units => _masterUnits.Values.ToList().AsReadOnly();

    public UnitState GetOrAddUnit(Guid id, string name, EntityType type, HexCoordinate coordinate)
    {
        return _masterUnits.GetOrAdd(id, _ => new UnitState
        {
            Id = id,
            Name = name,
            Type = type,
            Coordinate = coordinate,
            LifecycleState = UnitLifecycleState.Active,
            LastUpdatedUtc = DateTime.UtcNow
        });
    }

    public bool TryGetUnit(Guid id, out UnitState? unit)
    {
        return _masterUnits.TryGetValue(id, out unit);
    }

    public void UpdateBaseFortification(string baseName, int fortLevel)
    {
        _baseFortifications[baseName] = fortLevel;
    }

    public int GetBaseFortification(string baseName)
    {
        return _baseFortifications.TryGetValue(baseName, out int level) ? level : 0;
    }

    public void AddAlert(string alert)
    {
        lock (_alertLock)
        {
            _dailyAlerts.Add(alert);
        }
    }

    public IReadOnlyList<string> GetDailyAlerts()
    {
        lock (_alertLock)
        {
            return _dailyAlerts.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Creates an immutable point-in-time snapshot of the current world state.
    /// </summary>
    public WorldStateSnapshot CreateSnapshot(DateOnly turnDate)
    {
        var unitSnapshots = _masterUnits.Values.Select(u => new UnitStateSnapshot(
            u.Id,
            u.Name,
            u.Type,
            u.LifecycleState,
            u.Coordinate,
            u.FuelLevel,
            u.LossProbability,
            u.LastUpdatedUtc
        )).ToList();

        var alerts = GetDailyAlerts();
        var forts = new Dictionary<string, int>(_baseFortifications);

        return new WorldStateSnapshot(
            turnDate,
            DateTime.UtcNow,
            unitSnapshots.AsReadOnly(),
            alerts,
            forts
        );
    }

    /// <summary>
    /// Serializes world state snapshot to JSON stream asynchronously.
    /// </summary>
    public async Task SaveSnapshotAsync(Stream outputStream, WorldStateSnapshot snapshot)
    {
        await JsonSerializer.SerializeAsync(outputStream, snapshot, new JsonSerializerOptions
        {
            WriteIndented = true
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Deserializes and restores world state from a JSON stream.
    /// </summary>
    public async Task RestoreSnapshotAsync(Stream inputStream)
    {
        var snapshot = await JsonSerializer.DeserializeAsync<WorldStateSnapshot>(inputStream).ConfigureAwait(false);
        if (snapshot == null) return;

        _masterUnits.Clear();
        foreach (var u in snapshot.Units)
        {
            _masterUnits[u.Id] = new UnitState
            {
                Id = u.Id,
                Name = u.Name,
                Type = u.Type,
                LifecycleState = u.LifecycleState,
                Coordinate = u.Coordinate,
                FuelLevel = u.FuelLevel,
                LossProbability = u.LossProbability,
                LastUpdatedUtc = u.LastUpdatedUtc
            };
        }

        _baseFortifications.Clear();
        foreach (var kvp in snapshot.BaseFortifications)
        {
            _baseFortifications[kvp.Key] = kvp.Value;
        }

        lock (_alertLock)
        {
            _dailyAlerts.Clear();
            _dailyAlerts.AddRange(snapshot.LogisticalAlerts);
        }
    }
}
