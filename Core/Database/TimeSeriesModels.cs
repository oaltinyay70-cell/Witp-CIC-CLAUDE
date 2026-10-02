// ROCHEFORT 6000 // PACIFIC CIC
// Core/Database/TimeSeriesModels.cs
//
// Canonical time-series data models for the WITP:AE Master System Blueprint.
// SCD Type 2 daily-state snapshots and event-sourced history records.
// All records are Allied-only (faction-isolation-rule).
// Storage: in-memory SortedDictionary collections, serializable to JSON.

using System;
using System.Collections.Generic;

namespace ROCHEFORT6000.Core.Database
{
    // ─── 1. ship_daily_state ─────────────────────────────────────────────────
    // PK: (SnapshotDate, ShipName)
    
    /// <summary>
    /// Daily state snapshot for a single Allied ship.
    /// Corresponds to the ship_daily_state table in the Master System Blueprint.
    /// One record per ship per game day.
    /// </summary>
    public sealed class ShipDailyState
    {
        /// <summary>Game date of this snapshot (DateOnly from YYMMDD).</summary>
        public DateOnly SnapshotDate { get; init; }

        /// <summary>Ship name (unique identifier within a snapshot date).</summary>
        public string ShipName { get; init; } = string.Empty;

        /// <summary>Hull type code from the ship class table (e.g. 2=CV, 3=CVL, etc.).</summary>
        public int HullType { get; init; }

        /// <summary>Ship class name (e.g. "Yorktown", "Fletcher").</summary>
        public string ClassName { get; init; } = string.Empty;

        /// <summary>Nation code (4 = USA, etc.).</summary>
        public int Nation { get; init; }

        /// <summary>Status string: "Active", "Sunk", "Reinforcement".</summary>
        public string Status { get; init; } = string.Empty;

        /// <summary>Hex X coordinate.</summary>
        public int LocationHexX { get; init; }

        /// <summary>Hex Y coordinate.</summary>
        public int LocationHexY { get; init; }

        /// <summary>Location text description resolved from BaseLocationRegistry.</summary>
        public string LocationText { get; init; } = string.Empty;

        /// <summary>Task Force ID (0 = in port, >0 = assigned to TF).</summary>
        public int TfId { get; init; }

        /// <summary>Fuel percentage (0-100, computed from Fuel/MaxFuel).</summary>
        public int FuelPct { get; init; }

        /// <summary>System damage points.</summary>
        public int SysDmg { get; init; }

        /// <summary>Flotation damage points.</summary>
        public int FloatDmg { get; init; }

        /// <summary>Engine damage points.</summary>
        public int EngDmg { get; init; }

        /// <summary>Fire severity (0 = none).</summary>
        public int Fire { get; init; }

        /// <summary>Crew experience level.</summary>
        public int CrewExp { get; init; }

        /// <summary>0-based index in the ship table (for cross-snapshot tracking).</summary>
        public int ShipIndex { get; init; }

        /// <summary>Days until arrival if reinforcement (0 = in play).</summary>
        public int ArrivalDays { get; init; }

        /// <summary>Maximum fuel capacity.</summary>
        public int MaxFuel { get; init; }

        /// <summary>Current fuel level (raw).</summary>
        public int FuelRaw { get; init; }
    }

    // ─── 2. task_force_daily_state ────────────────────────────────────────────
    // PK: (SnapshotDate, TfId)
    
    /// <summary>
    /// Daily state snapshot for a single Allied task force.
    /// Corresponds to the task_force_daily_state table.
    /// </summary>
    public sealed class TaskForceDailyState
    {
        public DateOnly SnapshotDate { get; init; }
        public int TfId { get; init; }
        public int MissionType { get; init; }
        public int HexX { get; init; }
        public int HexY { get; init; }

        /// <summary>Home port location text (resolved from location index + BaseLocationRegistry).</summary>
        public string HomePort { get; init; } = string.Empty;

        /// <summary>Destination location text.</summary>
        public string Destination { get; init; } = string.Empty;

        /// <summary>Flagship name (resolved from flagship ship index).</summary>
        public string FlagshipName { get; init; } = string.Empty;

        /// <summary>Lowest fuel percentage among all ships in this TF.</summary>
        public int LowestFuelPct { get; init; }

        /// <summary>Number of ships in this task force.</summary>
        public int ShipCount { get; init; }
    }

    // ─── 3. tf_composition_history ────────────────────────────────────────────
    // PK: (SnapshotDate, TfId, ShipName)
    
    /// <summary>
    /// Records which ships belong to which task force on each day.
    /// Enables TF composition change tracking over time.
    /// </summary>
    public sealed class TfCompositionRecord
    {
        public DateOnly SnapshotDate { get; init; }
        public int TfId { get; init; }
        public string ShipName { get; init; } = string.Empty;
        public int ShipIndex { get; init; }
    }

    // ─── 4. fleet_summary ────────────────────────────────────────────────────
    // PK: (SnapshotDate)
    
    /// <summary>
    /// Aggregate fleet statistics for a single game day.
    /// Derived from the ship_daily_state records for that day.
    /// </summary>
    public sealed class FleetSummary
    {
        public DateOnly SnapshotDate { get; init; }
        public int TotalActive { get; init; }
        public int AtSea { get; init; }
        public int InPort { get; init; }
        public int TotalTfs { get; init; }
        public int DamagedCount { get; init; }
        public int OnFireCount { get; init; }
        public int LowFuelCount { get; init; }
        public int TotalLost { get; init; }
        public int FutureReinforcements { get; init; }
    }

    // ─── 5. lifecycle_events ─────────────────────────────────────────────────
    // PK: (EventDate, ShipName, EventType)
    
    /// <summary>
    /// Event-sourced lifecycle transition for a ship.
    /// Delta-tracked: comparing consecutive daily snapshots to detect new events.
    /// </summary>
    public sealed class LifecycleEvent
    {
        public DateOnly EventDate { get; init; }
        public string ShipName { get; init; } = string.Empty;
        public int ShipIndex { get; init; }

        /// <summary>Event type: Commissioned, Arrived, Damaged, Sunk, Lost, Repaired, Deployed, FuelCritical.</summary>
        public string EventType { get; init; } = string.Empty;

        public int LocationHexX { get; init; }
        public int LocationHexY { get; init; }

        /// <summary>Cause or description of the event.</summary>
        public string Cause { get; init; } = string.Empty;
    }

    // ─── 6. operations_report_log ────────────────────────────────────────────
    // PK: (ReportDate, EventId)
    
    /// <summary>
    /// Parsed entry from the embedded operations/combat/sigint reports.
    /// </summary>
    public sealed class OperationsReportEntry
    {
        public DateOnly ReportDate { get; init; }

        /// <summary>Sequential event ID within the report date.</summary>
        public int EventId { get; init; }

        /// <summary>Category: Combat, Movement, Strategic, Logistics, Sigint.</summary>
        public string EventCategory { get; init; } = string.Empty;

        /// <summary>Raw text line from the report.</summary>
        public string TextRaw { get; init; } = string.Empty;

        /// <summary>Parsed hex X coordinate from the text (0 if unparseable).</summary>
        public int ParsedLocationX { get; init; }

        /// <summary>Parsed hex Y coordinate from the text (0 if unparseable).</summary>
        public int ParsedLocationY { get; init; }

        /// <summary>Involved task force ID (0 if none).</summary>
        public int InvolvedTfId { get; init; }

        /// <summary>Comma-separated list of involved ship names.</summary>
        public string InvolvedShips { get; init; } = string.Empty;
    }
}
