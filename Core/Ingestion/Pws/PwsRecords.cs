// ROCHEFORT 6000 // PACIFIC CIC
// Core/Ingestion/Pws/PwsRecords.cs
//
// Immutable record types for all PWS binary block payloads.
// All records are Allied-only (side == 1 enforced at parse time — faction-isolation-rule).
// None of these types are ever populated from Japanese-side data.
//
// Field offsets are per the verified WitP:AE format spec (v1.8.11.28.4).
// "sunk" == status 19999 == UInt16 at offset 36 of ship record.

using System;
using System.Collections.Generic;

namespace ROCHEFORT6000.Core.Ingestion.Pws
{
    // ─── Enumerations ─────────────────────────────────────────────────────────

    public enum ShipStatus
    {
        Active  = 0,    // in play
        Sunk    = 19999 // permanent loss
    }

    public enum ShipSide
    {
        Japanese = 0,
        Allied   = 1
    }

    // ─── Block 520 / 513: Ship Records (stride 464 bytes) ────────────────────

    /// <summary>
    /// Allied ship state parsed from block 520 (or 513).
    /// Only populated for ships where side field == 1 (Allied).
    /// </summary>
    public sealed class PwsShipRecord
    {
        /// <summary>0-based index in the ship table.</summary>
        public int Index { get; init; }

        /// <summary>Ship name from offset 0 (null-terminated Latin1, max 36 bytes).</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Status: 0 = active, 19999 = sunk.</summary>
        public int Status { get; init; }

        /// <summary>Side: 0 = Japanese, 1 = Allied.</summary>
        public int Side { get; init; }

        /// <summary>Nation code (4 = USA, varies).</summary>
        public int Nation { get; init; }

        /// <summary>Ship class index (into class table).</summary>
        public int ClassIndex { get; init; }

        /// <summary>Current hex X coordinate.</summary>
        public int HexX { get; init; }

        /// <summary>Current hex Y coordinate.</summary>
        public int HexY { get; init; }

        /// <summary>Task force index (0 = in port, >0 = at sea).</summary>
        public int TaskForceIndex { get; init; }

        /// <summary>Current fuel level.</summary>
        public int Fuel { get; init; }

        /// <summary>Maximum fuel capacity (from class record).</summary>
        public int MaxFuel { get; init; }

        /// <summary>Days until arrival if reinforcement (0 = already in play).</summary>
        public int ArrivalDays { get; init; }

        /// <summary>Location index of arrival base (if reinforcement).</summary>
        public int ArrivalLocationIndex { get; init; }

        /// <summary>Whether this ship has been sunk.</summary>
        public bool IsSunk => Status == (int)ShipStatus.Sunk;

        /// <summary>Whether this ship is a reinforcement (not yet arrived).</summary>
        public bool IsReinforcement => ArrivalDays > 0;

        /// <summary>Whether this ship is in a task force at sea.</summary>
        public bool IsAtSea => TaskForceIndex > 0;

        /// <summary>Date this snapshot was extracted from (YYMMDD).</summary>
        public string SnapshotDate { get; init; } = string.Empty;
    }

    // ─── Block 582: Task Force Records (stride 556 bytes) ────────────────────

    /// <summary>
    /// Task force state parsed from block 582.
    /// </summary>
    public sealed class PwsTaskForceRecord
    {
        /// <summary>0-based index in the task force table.</summary>
        public int Index { get; init; }

        /// <summary>Flagship ship index (into ship table).</summary>
        public int FlagshipIndex { get; init; }

        /// <summary>Current hex X.</summary>
        public int HexX { get; init; }

        /// <summary>Current hex Y.</summary>
        public int HexY { get; init; }

        /// <summary>Side: 0 = Japanese, 1 = Allied.</summary>
        public int Side { get; init; }

        /// <summary>Home base location index.</summary>
        public int HomeLocationIndex { get; init; }

        /// <summary>Secondary base location index.</summary>
        public int SecondaryLocationIndex { get; init; }

        /// <summary>Destination location index.</summary>
        public int DestinationLocationIndex { get; init; }

        /// <summary>Mission type code.</summary>
        public int MissionType { get; init; }

        /// <summary>Number of ships in this task force.</summary>
        public int ShipCount { get; init; }

        /// <summary>Path waypoints as list of (X, Y) pairs.</summary>
        public List<(int X, int Y)> Path { get; init; } = new();

        /// <summary>Date this snapshot was extracted from (YYMMDD).</summary>
        public string SnapshotDate { get; init; } = string.Empty;
    }

    // ─── Block 533: Leader Records (stride 180 bytes) ─────────────────────────

    /// <summary>
    /// Leader/admiral record from block 533.
    /// </summary>
    public sealed class PwsLeaderRecord
    {
        /// <summary>0-based index in the leader table.</summary>
        public int Index { get; init; }

        /// <summary>Leader name (Latin1, null-terminated, max 40 bytes at offset 0).</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Side: 0 = Japanese, 1 = Allied.</summary>
        public int Side { get; init; }

        /// <summary>Ship index this leader commands (offset +36, u16).</summary>
        public int ShipIndex { get; init; }

        /// <summary>Status (19999 = captured/dead).</summary>
        public int Status { get; init; }

        /// <summary>Date this snapshot was extracted from (YYMMDD).</summary>
        public string SnapshotDate { get; init; } = string.Empty;
    }

    // ─── Block 530/531: Location Records (stride 1172 bytes) ─────────────────

    /// <summary>
    /// Base/location record from blocks 530/531.
    /// Name is NOT decoded (8-byte-block encrypted) — use BaseLocationRegistry for names.
    /// </summary>
    public sealed class PwsLocationRecord
    {
        /// <summary>0-based location index.</summary>
        public int Index { get; init; }

        /// <summary>Hex X coordinate.</summary>
        public int HexX { get; init; }

        /// <summary>Hex Y coordinate.</summary>
        public int HexY { get; init; }

        /// <summary>Controller flag (side, 0=Japanese, 1=Allied, etc.).</summary>
        public int Controller { get; init; }

        /// <summary>Date this snapshot was extracted from (YYMMDD).</summary>
        public string SnapshotDate { get; init; } = string.Empty;
    }

    // ─── Block 539: Ship Class Records (stride 380 bytes) ────────────────────

    /// <summary>
    /// Ship class definition from block 539.
    /// </summary>
    public sealed class PwsClassRecord
    {
        /// <summary>0-based class index.</summary>
        public int Index { get; init; }

        /// <summary>Class name (Latin1, null-terminated, offset 0, max 30 bytes).</summary>
        public string ClassName { get; init; } = string.Empty;

        /// <summary>Vessel type (2 = CV, etc.).</summary>
        public int VesselType { get; init; }

        /// <summary>Maximum fuel capacity.</summary>
        public int MaxFuel { get; init; }

        /// <summary>Date this snapshot was extracted from (YYMMDD).</summary>
        public string SnapshotDate { get; init; } = string.Empty;
    }

    // ─── Block 550: Header / Meta ─────────────────────────────────────────────

    /// <summary>
    /// Game header metadata from block 550.
    /// </summary>
    public sealed class PwsMetaRecord
    {
        /// <summary>Game date string as read from the header (MM/DD/YY format).</summary>
        public string GameDateRaw { get; init; } = string.Empty;

        /// <summary>Normalized date in YYMMDD format.</summary>
        public string DateYYMMDD { get; init; } = string.Empty;

        /// <summary>Scenario name / description from header text.</summary>
        public string ScenarioName { get; init; } = string.Empty;

        /// <summary>Game version string.</summary>
        public string GameVersion { get; init; } = string.Empty;

        /// <summary>Whether this is a PBEM incoming file (wpae021).</summary>
        public bool IsPbemIncoming { get; init; }

        /// <summary>File path from which this meta was parsed.</summary>
        public string FilePath { get; init; } = string.Empty;
    }

    // ─── Block 593: Turn Report Sections ─────────────────────────────────────

    /// <summary>
    /// One named section from the block-593 turn report payload.
    /// </summary>
    public sealed class PwsReportSection
    {
        public enum SectionKind { CombatReport, CombatEvents, Operations, Sigint, Unknown }

        public SectionKind Kind { get; init; }
        public string Text { get; init; } = string.Empty;
        public string[] Lines { get; init; } = Array.Empty<string>();
        public string DateYYMMDD { get; init; } = string.Empty;
    }

    // ─── Aggregate: Full snapshot from one .pws file ─────────────────────────

    /// <summary>
    /// Everything extracted from a single .pws file.
    /// Always Allied-only data. Japanese records are never included.
    /// </summary>
    public sealed class PwsSnapshot
    {
        public PwsMetaRecord Meta { get; set; } = new();
        public List<PwsShipRecord>      AlliedShips      { get; } = new();
        public List<PwsTaskForceRecord> AlliedTaskForces { get; } = new();
        public List<PwsLeaderRecord>    AlliedLeaders    { get; } = new();
        public List<PwsLocationRecord>  Locations        { get; } = new();
        public List<PwsClassRecord>     ShipClasses      { get; } = new();
        public List<PwsReportSection>   Reports          { get; } = new();

        // Aggregate counters (computed post-parse)
        public int AlliedActiveCount        { get; set; }
        public int AlliedAtSeaCount         { get; set; }
        public int AlliedInPortCount        { get; set; }
        public int AlliedSunkCount          { get; set; }
        public int AlliedReinforcementCount { get; set; }
        public int AlliedTaskForceCount     { get; set; }

        /// <summary>All warnings generated during parsing (non-fatal).</summary>
        public List<string> Warnings { get; } = new();
    }

}
