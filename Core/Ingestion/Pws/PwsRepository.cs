// ROCHEFORT 6000 // PACIFIC CIC
// Core/Ingestion/Pws/PwsRepository.cs
//
// Multi-file .pws timeline manager.
//
// Business rule (mandatory):
//   "u must always include data from the most recent pws file.
//    a new pws file drops at the end of each turn which is one day.
//    but also remember that old data must be kept and can be - if required
//    and still valid - used in the computations."
//
// Implementation:
//   - Maintains an ordered SortedDictionary<DateOnly, PwsSnapshot> for temporal continuity.
//   - Most recent snapshot is always the authority for live fleet/leader state.
//   - Older snapshots are retained for: reinforcement tracking, sunk-ship histories,
//     fuel consumption deltas, TF route reconstruction, arrival-day countdown verification.
//   - Pillar 13 (SortedDictionary temporal continuity + TemporalGapWarningEvent) is honoured.
//   - Pillar 6 (WorldStateRepository ConcurrentDictionary<Guid, UnitState>) integration point.
//   - Pillar 4 (EntityResolutionService + DomainEventBus) integration point for unit lifecycle.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ROCHEFORT6000.Core.Ingestion;
using WitpCic.Core.Analytics.EntityResolution;
using WitpCic.Core.Analytics.Events;
using WitpCic.Core.Geospatial;
using WitpCic.Core.State;

namespace ROCHEFORT6000.Core.Ingestion.Pws
{
    /// <summary>
    /// Event arguments for temporal gap warnings between PWS turns.
    /// Emitted when a calendar day is missing between two consecutive snapshots.
    /// </summary>
    public sealed class PwsTemporalGapWarningEvent : EventArgs
    {
        public DateOnly GapStart { get; init; }
        public DateOnly GapEnd   { get; init; }
        public int      DaysMissing { get; init; }
    }

    /// <summary>
    /// Manages the ordered collection of PWS snapshots across multiple turn files.
    /// Implements SortedDictionary temporal continuity (Pillar 13).
    /// Thread-safe for background ingestion via the DecayBackgroundService.
    /// </summary>
    public sealed class PwsRepository
    {
        // SortedDictionary for O(log N) insert + in-order iteration (Pillar 13)
        private readonly SortedDictionary<DateOnly, PwsSnapshot> _snapshots = new();
        private readonly object _lock = new();

        // Fired when a missing turn-day gap is detected between two snapshots (Pillar 13)
        public event EventHandler<PwsTemporalGapWarningEvent>? TemporalGapDetected;

        // ─── Integration points ───────────────────────────────────────────────

        /// <summary>Optional: EntityResolutionService integration (Pillar 4).</summary>
        public EntityResolutionService? EntityResolver { get; init; }

        /// <summary>Optional: DomainEventBus integration (Pillar 4).</summary>
        public DomainEventBus? EventBus { get; init; }

        /// <summary>Optional: WorldStateRepository integration (Pillar 6).</summary>
        public WorldStateRepository? WorldState { get; init; }

        // ─── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Ingests a single pre-parsed PWS snapshot into the repository.
        /// Fires TemporalGapDetected if a day is missing before this snapshot.
        /// Propagates ship/leader state into WorldStateRepository if wired.
        /// </summary>
        public void Ingest(PwsSnapshot snapshot)
        {
            if (string.IsNullOrEmpty(snapshot.Meta.DateYYMMDD))
                return;

            if (!TryParseDate(snapshot.Meta.DateYYMMDD, out DateOnly date))
                return;

            lock (_lock)
            {
                // Gap detection vs. the previous snapshot (Pillar 13)
                if (_snapshots.Count > 0)
                {
                    DateOnly lastDate = _snapshots.Keys.Max();
                    if (date > lastDate.AddDays(1))
                    {
                        var gap = new PwsTemporalGapWarningEvent
                        {
                            GapStart    = lastDate.AddDays(1),
                            GapEnd      = date.AddDays(-1),
                            DaysMissing = date.DayNumber - lastDate.DayNumber - 1
                        };
                        TemporalGapDetected?.Invoke(this, gap);
                    }
                }

                _snapshots[date] = snapshot;
            }

            // Propagate entity state into WorldStateRepository (Pillar 6)
            if (WorldState != null)
                ApplySnapshotToWorldState(snapshot, WorldState);

            // Publish lifecycle events via DomainEventBus (Pillar 4)
            if (EventBus != null)
                PublishLifecycleEvents(snapshot, EventBus);
        }

        /// <summary>
        /// Parses and ingests a .pws file by path.
        /// </summary>
        public PwsSnapshot? IngestFile(string filePath, string? overrideDate = null)
        {
            try
            {
                var snapshot = PwsFileParser.Parse(filePath, overrideDate);
                Ingest(snapshot);
                return snapshot;
            }
            catch (Exception ex)
            {
                // Non-fatal — log warning, don't crash the analytics pipeline
                System.Diagnostics.Debug.WriteLine($"[PwsRepository] Failed to ingest {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Returns the most recent snapshot (the authority for live state).
        /// This is ALWAYS the most recent .pws file per the business rule.
        /// </summary>
        public PwsSnapshot? MostRecent
        {
            get
            {
                lock (_lock)
                    return _snapshots.Count == 0 ? null : _snapshots[_snapshots.Keys.Max()];
            }
        }

        /// <summary>
        /// Returns all snapshots in chronological order.
        /// Older snapshots are kept for historical computation.
        /// </summary>
        public IReadOnlyList<PwsSnapshot> AllSnapshotsChronological
        {
            get
            {
                lock (_lock)
                    return _snapshots.Values.ToList();
            }
        }

        /// <summary>
        /// Returns the snapshot for a specific game date (YYMMDD), or null if not loaded.
        /// </summary>
        public PwsSnapshot? GetByDate(string dateYYMMDD)
        {
            if (!TryParseDate(dateYYMMDD, out DateOnly date))
                return null;
            lock (_lock)
                return _snapshots.TryGetValue(date, out var s) ? s : null;
        }

        /// <summary>
        /// Returns the arrival-day countdown for a reinforcement ship across all snapshots.
        /// Used for verifying spec validation: ship 3006 "Hornet" arrival-day deltas.
        /// </summary>
        public IReadOnlyList<(string DateYYMMDD, int ArrivalDays)> GetArrivalDayHistory(int shipIndex)
        {
            var result = new List<(string, int)>();
            lock (_lock)
            {
                foreach (var (_, snap) in _snapshots)
                {
                    var ship = snap.AlliedShips.Find(s => s.Index == shipIndex);
                    if (ship != null)
                        result.Add((snap.Meta.DateYYMMDD, ship.ArrivalDays));
                }
            }
            return result;
        }

        /// <summary>
        /// Returns fuel consumption delta for a ship between two dates.
        /// Used by WarRoomAnalytics to infer at-sea movement patterns.
        /// </summary>
        public int? GetFuelDelta(int shipIndex, string fromDateYYMMDD, string toDateYYMMDD)
        {
            var from = GetByDate(fromDateYYMMDD)?.AlliedShips.Find(s => s.Index == shipIndex);
            var to   = GetByDate(toDateYYMMDD)?.AlliedShips.Find(s => s.Index == shipIndex);
            if (from == null || to == null) return null;
            return from.Fuel - to.Fuel; // positive = consumed fuel
        }

        /// <summary>Total number of snapshots currently held.</summary>
        public int Count { get { lock (_lock) return _snapshots.Count; } }

        /// <summary>Clears all snapshots (used between campaign loads).</summary>
        public void Clear() { lock (_lock) _snapshots.Clear(); }

        // ─── WorldStateRepository integration (Pillar 6) ──────────────────────

        private static void ApplySnapshotToWorldState(PwsSnapshot snap, WorldStateRepository ws)
        {
            foreach (var ship in snap.AlliedShips)
            {
                if (ship.IsSunk || ship.IsReinforcement) continue;

                var id = DeriveShipGuid(ship.Index);
                var coord = new HexCoordinate(ship.HexX, ship.HexY);
                var unit = ws.GetOrAddUnit(id, ship.Name, EntityType.SurfaceShip, coord);
                // Update mutable reactive fields (INotifyPropertyChanged triggers UI binding)
                unit.Coordinate     = coord;
                unit.FuelLevel      = ship.Fuel;
                unit.LastUpdatedUtc = DateTime.UtcNow;
            }
        }

        // ─── DomainEventBus integration (Pillar 4) ────────────────────────────

        private static void PublishLifecycleEvents(PwsSnapshot snap, DomainEventBus bus)
        {
            foreach (var ship in snap.AlliedShips)
            {
                if (!ship.IsSunk) continue;

                var entityId = DeriveShipGuid(ship.Index);
                // Use the existing ShipSunkEvent record defined in DomainEventBus.cs
                _ = bus.PublishAsync(new ShipSunkEvent(
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    entityId,
                    ship.Name
                ));
            }
        }

        // ─── Utility ──────────────────────────────────────────────────────────

        private static bool TryParseDate(string yymmdd, out DateOnly result)
        {
            result = default;
            if (yymmdd.Length != 6) return false;
            if (!int.TryParse(yymmdd.AsSpan(0, 2), out int yy)) return false;
            if (!int.TryParse(yymmdd.AsSpan(2, 2), out int mm)) return false;
            if (!int.TryParse(yymmdd.AsSpan(4, 2), out int dd)) return false;
            try
            {
                result = new DateOnly(1900 + yy, mm, dd);
                return true;
            }
            catch { return false; }
        }

        private static Guid DeriveShipGuid(int shipIndex)
            => new Guid(shipIndex, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }
}
