// ROCHEFORT 6000 // PACIFIC CIC
// Core/Database/TimeSeriesStore.cs
//
// In-memory time-series data store for the WITP:AE Master System Blueprint.
// Holds SCD Type 2 daily snapshots and event-sourced lifecycle history.
// Thread-safe via locking. Serializable to JSON for session persistence.
//
// Pillar 13: SortedDictionary<DateOnly, ...> for temporal continuity.
// Pillar 6:  Immutable query results; mutable state guarded by lock.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ROCHEFORT6000.Core.Database
{
    /// <summary>
    /// Central time-series data store holding all 6 blueprint table types.
    /// All mutation methods are thread-safe. Query methods return snapshots.
    /// </summary>
    public sealed class TimeSeriesStore
    {
        private readonly object _lock = new();

        // ─── SCD Type 2 daily-state tables ───────────────────────────────────
        private readonly SortedDictionary<DateOnly, List<ShipDailyState>> _shipStates = new();
        private readonly SortedDictionary<DateOnly, List<TaskForceDailyState>> _tfStates = new();
        private readonly SortedDictionary<DateOnly, List<TfCompositionRecord>> _tfComposition = new();
        private readonly SortedDictionary<DateOnly, FleetSummary> _fleetSummaries = new();

        // ─── Event-sourced tables ────────────────────────────────────────────
        private readonly List<LifecycleEvent> _lifecycleEvents = new();
        private readonly List<OperationsReportEntry> _reportEntries = new();

        // ─── Ship state: Append ──────────────────────────────────────────────

        /// <summary>
        /// Appends a batch of ship daily states for a single snapshot date.
        /// Replaces any existing records for that date (idempotent re-ingestion).
        /// </summary>
        public void AppendShipStates(DateOnly date, IEnumerable<ShipDailyState> states)
        {
            var list = states.ToList();
            lock (_lock)
                _shipStates[date] = list;
        }

        /// <summary>
        /// Appends a batch of task force daily states for a single snapshot date.
        /// </summary>
        public void AppendTaskForceStates(DateOnly date, IEnumerable<TaskForceDailyState> states)
        {
            var list = states.ToList();
            lock (_lock)
                _tfStates[date] = list;
        }

        /// <summary>
        /// Appends TF composition records for a single snapshot date.
        /// </summary>
        public void AppendTfComposition(DateOnly date, IEnumerable<TfCompositionRecord> records)
        {
            var list = records.ToList();
            lock (_lock)
                _tfComposition[date] = list;
        }

        /// <summary>
        /// Sets the fleet summary for a single snapshot date.
        /// </summary>
        public void SetFleetSummary(DateOnly date, FleetSummary summary)
        {
            lock (_lock)
                _fleetSummaries[date] = summary;
        }

        /// <summary>
        /// Appends lifecycle events (delta-tracked, append-only).
        /// Deduplicates by (EventDate, ShipIndex, EventType) composite key.
        /// </summary>
        public void AppendLifecycleEvents(IEnumerable<LifecycleEvent> events)
        {
            lock (_lock)
            {
                foreach (var evt in events)
                {
                    bool exists = _lifecycleEvents.Exists(e =>
                        e.EventDate == evt.EventDate &&
                        e.ShipIndex == evt.ShipIndex &&
                        e.EventType == evt.EventType);
                    if (!exists)
                        _lifecycleEvents.Add(evt);
                }
            }
        }

        /// <summary>
        /// Appends operations report entries.
        /// </summary>
        public void AppendReportEntries(IEnumerable<OperationsReportEntry> entries)
        {
            lock (_lock)
                _reportEntries.AddRange(entries);
        }

        // ─── Query: Ship States ──────────────────────────────────────────────

        /// <summary>Returns all ship states for a specific date, or empty.</summary>
        public IReadOnlyList<ShipDailyState> GetShipStatesByDate(DateOnly date)
        {
            lock (_lock)
                return _shipStates.TryGetValue(date, out var list) ? list.ToList() : Array.Empty<ShipDailyState>();
        }

        /// <summary>
        /// Returns the full history for a single ship across all snapshot dates.
        /// Keyed by ShipIndex for stable cross-day tracking.
        /// </summary>
        public IReadOnlyList<ShipDailyState> GetShipHistory(int shipIndex)
        {
            lock (_lock)
            {
                var result = new List<ShipDailyState>();
                foreach (var (_, states) in _shipStates)
                {
                    var ship = states.Find(s => s.ShipIndex == shipIndex);
                    if (ship != null)
                        result.Add(ship);
                }
                return result;
            }
        }

        // ─── Query: Task Force States ────────────────────────────────────────

        /// <summary>Returns all TF states for a specific date.</summary>
        public IReadOnlyList<TaskForceDailyState> GetTfStatesByDate(DateOnly date)
        {
            lock (_lock)
                return _tfStates.TryGetValue(date, out var list) ? list.ToList() : Array.Empty<TaskForceDailyState>();
        }

        /// <summary>
        /// Returns TF composition records for a specific TF across all dates.
        /// Enables tracking which ships joined/left over time.
        /// </summary>
        public IReadOnlyList<(DateOnly Date, List<TfCompositionRecord> Members)> GetTfCompositionHistory(int tfId)
        {
            lock (_lock)
            {
                var result = new List<(DateOnly, List<TfCompositionRecord>)>();
                foreach (var (date, records) in _tfComposition)
                {
                    var members = records.Where(r => r.TfId == tfId).ToList();
                    if (members.Count > 0)
                        result.Add((date, members));
                }
                return result;
            }
        }

        // ─── Query: Fleet Summaries ──────────────────────────────────────────

        /// <summary>Returns the fleet summary for a specific date, or null.</summary>
        public FleetSummary? GetFleetSummary(DateOnly date)
        {
            lock (_lock)
                return _fleetSummaries.TryGetValue(date, out var s) ? s : null;
        }

        /// <summary>Returns all fleet summaries in chronological order.</summary>
        public IReadOnlyList<FleetSummary> GetAllFleetSummaries()
        {
            lock (_lock)
                return _fleetSummaries.Values.ToList();
        }

        // ─── Query: Lifecycle Events ─────────────────────────────────────────

        /// <summary>Returns new sinkings detected on a specific date.</summary>
        public IReadOnlyList<LifecycleEvent> GetNewSinkings(DateOnly date)
        {
            lock (_lock)
                return _lifecycleEvents
                    .Where(e => e.EventDate == date && e.EventType == "Sunk")
                    .ToList();
        }

        /// <summary>Returns reinforcement arrivals on a specific date.</summary>
        public IReadOnlyList<LifecycleEvent> GetReinforcementArrivals(DateOnly date)
        {
            lock (_lock)
                return _lifecycleEvents
                    .Where(e => e.EventDate == date && e.EventType == "Arrived")
                    .ToList();
        }

        /// <summary>Returns all lifecycle events for a specific ship.</summary>
        public IReadOnlyList<LifecycleEvent> GetShipLifecycle(int shipIndex)
        {
            lock (_lock)
                return _lifecycleEvents
                    .Where(e => e.ShipIndex == shipIndex)
                    .OrderBy(e => e.EventDate)
                    .ToList();
        }

        /// <summary>Returns all lifecycle events in chronological order.</summary>
        public IReadOnlyList<LifecycleEvent> GetAllLifecycleEvents()
        {
            lock (_lock)
                return _lifecycleEvents.OrderBy(e => e.EventDate).ToList();
        }

        // ─── Query: Operations Report ────────────────────────────────────────

        /// <summary>Returns all report entries for a specific date.</summary>
        public IReadOnlyList<OperationsReportEntry> GetReportEntriesByDate(DateOnly date)
        {
            lock (_lock)
                return _reportEntries.Where(e => e.ReportDate == date).ToList();
        }

        // ─── Query: Timeline Metadata ────────────────────────────────────────

        /// <summary>Returns all dates that have ship state data, in order.</summary>
        public IReadOnlyList<DateOnly> AllDates
        {
            get { lock (_lock) return _shipStates.Keys.ToList(); }
        }

        /// <summary>Returns the most recent date in the store, or null.</summary>
        public DateOnly? MostRecentDate
        {
            get { lock (_lock) return _shipStates.Count > 0 ? _shipStates.Keys.Max() : null; }
        }

        /// <summary>Total number of snapshot dates stored.</summary>
        public int DayCount { get { lock (_lock) return _shipStates.Count; } }

        /// <summary>Total number of lifecycle events stored.</summary>
        public int LifecycleEventCount { get { lock (_lock) return _lifecycleEvents.Count; } }

        /// <summary>Total number of report entries stored.</summary>
        public int ReportEntryCount { get { lock (_lock) return _reportEntries.Count; } }

        // ─── Mutation: Clear ──────────────────────────────────────────────────

        /// <summary>Clears all data (used between campaign reloads).</summary>
        public void Clear()
        {
            lock (_lock)
            {
                _shipStates.Clear();
                _tfStates.Clear();
                _tfComposition.Clear();
                _fleetSummaries.Clear();
                _lifecycleEvents.Clear();
                _reportEntries.Clear();
            }
        }

        // ─── Persistence: JSON Serialization ─────────────────────────────────

        /// <summary>
        /// Serializable snapshot of the entire store for JSON persistence.
        /// </summary>
        public sealed class StoreSnapshot
        {
            public List<ShipDailyState> ShipStates { get; set; } = new();
            public List<TaskForceDailyState> TfStates { get; set; } = new();
            public List<TfCompositionRecord> TfComposition { get; set; } = new();
            public List<FleetSummary> FleetSummaries { get; set; } = new();
            public List<LifecycleEvent> LifecycleEvents { get; set; } = new();
            public List<OperationsReportEntry> ReportEntries { get; set; } = new();
        }

        /// <summary>
        /// Creates an immutable snapshot of all data for serialization.
        /// </summary>
        public StoreSnapshot CreateSnapshot()
        {
            lock (_lock)
            {
                return new StoreSnapshot
                {
                    ShipStates = _shipStates.Values.SelectMany(x => x).ToList(),
                    TfStates = _tfStates.Values.SelectMany(x => x).ToList(),
                    TfComposition = _tfComposition.Values.SelectMany(x => x).ToList(),
                    FleetSummaries = _fleetSummaries.Values.ToList(),
                    LifecycleEvents = _lifecycleEvents.ToList(),
                    ReportEntries = _reportEntries.ToList()
                };
            }
        }

        /// <summary>
        /// Serializes the store to a JSON stream asynchronously.
        /// </summary>
        public async Task SaveAsync(Stream outputStream)
        {
            var snapshot = CreateSnapshot();
            await JsonSerializer.SerializeAsync(outputStream, snapshot, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Restores the store from a JSON stream.
        /// </summary>
        public async Task RestoreAsync(Stream inputStream)
        {
            var snapshot = await JsonSerializer.DeserializeAsync<StoreSnapshot>(inputStream, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            if (snapshot == null) return;

            lock (_lock)
            {
                _shipStates.Clear();
                _tfStates.Clear();
                _tfComposition.Clear();
                _fleetSummaries.Clear();
                _lifecycleEvents.Clear();
                _reportEntries.Clear();

                // Re-group by date
                foreach (var group in snapshot.ShipStates.GroupBy(s => s.SnapshotDate))
                    _shipStates[group.Key] = group.ToList();
                foreach (var group in snapshot.TfStates.GroupBy(s => s.SnapshotDate))
                    _tfStates[group.Key] = group.ToList();
                foreach (var group in snapshot.TfComposition.GroupBy(s => s.SnapshotDate))
                    _tfComposition[group.Key] = group.ToList();
                foreach (var summary in snapshot.FleetSummaries)
                    _fleetSummaries[summary.SnapshotDate] = summary;
                _lifecycleEvents.AddRange(snapshot.LifecycleEvents);
                _reportEntries.AddRange(snapshot.ReportEntries);
            }
        }
    }
}
