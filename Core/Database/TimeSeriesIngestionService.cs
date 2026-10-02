// ROCHEFORT 6000 // PACIFIC CIC
// Core/Database/TimeSeriesIngestionService.cs
//
// ETL service: transforms PwsSnapshot → time-series records for the
// WITP:AE Master System Blueprint.
//
// Responsibilities:
//   1. Ship daily state extraction (with class name resolution)
//   2. Task force daily state (with flagship/location name resolution)
//   3. TF composition mapping (which ships belong to which TF)
//   4. Fleet summary aggregation
//   5. Delta-tracked lifecycle events (sinkings, arrivals, damage, fuel critical)
//   6. Operations report line indexing
//
// This service is stateless — it holds a reference to the previous snapshot
// for delta tracking only. The TimeSeriesStore owns all persisted state.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ROCHEFORT6000.Core.Ingestion.Pws;

namespace ROCHEFORT6000.Core.Database
{
    /// <summary>
    /// Transforms parsed PWS snapshots into canonical time-series records.
    /// Call <see cref="IngestSnapshot"/> for each PWS file in chronological order.
    /// </summary>
    public sealed class TimeSeriesIngestionService
    {
        private readonly TimeSeriesStore _store;

        // Previous snapshot for delta tracking (lifecycle events)
        private PwsSnapshot? _previousSnapshot;

        // Low fuel threshold: 25% fuel capacity
        private const int LowFuelThresholdPct = 25;

        // Regex for hex coordinate extraction from report text
        // Matches patterns like (48,128) or (180,107)
        private static readonly Regex HexCoordRegex =
            new(@"\((\d{1,3})\s*,\s*(\d{1,3})\)", RegexOptions.Compiled);

        // Regex for TF references in report text
        private static readonly Regex TfRefRegex =
            new(@"TF\s+(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public TimeSeriesIngestionService(TimeSeriesStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <summary>
        /// Ingests a single PWS snapshot into the time-series store.
        /// Must be called in chronological order for correct delta tracking.
        /// </summary>
        public void IngestSnapshot(PwsSnapshot snapshot)
        {
            if (string.IsNullOrEmpty(snapshot.Meta.DateYYMMDD))
                return;

            if (!TryParseDate(snapshot.Meta.DateYYMMDD, out DateOnly date))
                return;

            // Build lookup tables for this snapshot
            var classLookup = BuildClassLookup(snapshot);
            var shipByIndex = BuildShipIndexLookup(snapshot);
            var locationLookup = BuildLocationLookup(snapshot);

            // 1. Ship daily states
            var shipStates = ExtractShipStates(snapshot, date, classLookup);
            _store.AppendShipStates(date, shipStates);

            // 2. Task force daily states
            var tfStates = ExtractTfStates(snapshot, date, shipByIndex, locationLookup);
            _store.AppendTaskForceStates(date, tfStates);

            // 3. TF composition
            var composition = ExtractTfComposition(snapshot, date);
            _store.AppendTfComposition(date, composition);

            // 4. Fleet summary
            var summary = ComputeFleetSummary(snapshot, date, shipStates);
            _store.SetFleetSummary(date, summary);

            // 5. Delta-tracked lifecycle events
            var lifecycleEvents = DetectLifecycleEvents(snapshot, date, _previousSnapshot);
            if (lifecycleEvents.Count > 0)
                _store.AppendLifecycleEvents(lifecycleEvents);

            // 6. Operations report entries
            var reportEntries = ExtractReportEntries(snapshot, date);
            if (reportEntries.Count > 0)
                _store.AppendReportEntries(reportEntries);

            // Advance delta tracking
            _previousSnapshot = snapshot;
        }

        /// <summary>
        /// Resets the delta tracking state (for campaign reloads).
        /// </summary>
        public void Reset()
        {
            _previousSnapshot = null;
        }

        // ─── 1. Ship Daily States ────────────────────────────────────────────

        private static List<ShipDailyState> ExtractShipStates(
            PwsSnapshot snapshot, DateOnly date,
            Dictionary<int, PwsClassRecord> classLookup)
        {
            var result = new List<ShipDailyState>(snapshot.AlliedShips.Count);

            foreach (var ship in snapshot.AlliedShips)
            {
                string className = classLookup.TryGetValue(ship.ClassIndex, out var cls)
                    ? cls.ClassName : string.Empty;
                int hullType = classLookup.TryGetValue(ship.ClassIndex, out var cls2)
                    ? cls2.VesselType : 0;
                int fuelPct = ship.MaxFuel > 0
                    ? (int)Math.Round(100.0 * ship.Fuel / ship.MaxFuel)
                    : 0;

                string status = ship.IsSunk ? "Sunk"
                    : ship.IsReinforcement ? "Reinforcement"
                    : "Active";

                // Location text: resolve from BaseLocationRegistry if in port
                string locationText = ResolveLocationText(ship);

                result.Add(new ShipDailyState
                {
                    SnapshotDate  = date,
                    ShipName      = ship.Name,
                    ShipIndex     = ship.Index,
                    HullType      = hullType,
                    ClassName     = className,
                    Nation        = ship.Nation,
                    Status        = status,
                    LocationHexX  = ship.HexX,
                    LocationHexY  = ship.HexY,
                    LocationText  = locationText,
                    TfId          = ship.TaskForceIndex,
                    FuelPct       = fuelPct,
                    FuelRaw       = ship.Fuel,
                    MaxFuel       = ship.MaxFuel,
                    ArrivalDays   = ship.ArrivalDays,
                    // Damage fields: not yet extracted from binary
                    // (offsets TBD — placeholder zeros until parser extended)
                    SysDmg   = 0,
                    FloatDmg = 0,
                    EngDmg   = 0,
                    Fire     = 0,
                    CrewExp  = 0
                });
            }

            return result;
        }

        // ─── 2. Task Force Daily States ──────────────────────────────────────

        private static List<TaskForceDailyState> ExtractTfStates(
            PwsSnapshot snapshot, DateOnly date,
            Dictionary<int, PwsShipRecord> shipByIndex,
            Dictionary<int, PwsLocationRecord> locationLookup)
        {
            var result = new List<TaskForceDailyState>(snapshot.AlliedTaskForces.Count);

            foreach (var tf in snapshot.AlliedTaskForces)
            {
                // Resolve flagship name
                string flagshipName = shipByIndex.TryGetValue(tf.FlagshipIndex, out var fs)
                    ? fs.Name : string.Empty;

                // Resolve location names from location indices
                string homePort = ResolveLocationName(tf.HomeLocationIndex, locationLookup);
                string destination = ResolveLocationName(tf.DestinationLocationIndex, locationLookup);

                // Compute lowest fuel % among TF ships
                int lowestFuelPct = ComputeLowestFuelPct(tf.Index, snapshot.AlliedShips);

                result.Add(new TaskForceDailyState
                {
                    SnapshotDate  = date,
                    TfId          = tf.Index,
                    MissionType   = tf.MissionType,
                    HexX          = tf.HexX,
                    HexY          = tf.HexY,
                    HomePort      = homePort,
                    Destination   = destination,
                    FlagshipName  = flagshipName,
                    LowestFuelPct = lowestFuelPct,
                    ShipCount     = tf.ShipCount
                });
            }

            return result;
        }

        // ─── 3. TF Composition ───────────────────────────────────────────────

        private static List<TfCompositionRecord> ExtractTfComposition(
            PwsSnapshot snapshot, DateOnly date)
        {
            var result = new List<TfCompositionRecord>();

            foreach (var ship in snapshot.AlliedShips)
            {
                if (ship.TaskForceIndex > 0 && !ship.IsSunk && !ship.IsReinforcement)
                {
                    result.Add(new TfCompositionRecord
                    {
                        SnapshotDate = date,
                        TfId         = ship.TaskForceIndex,
                        ShipName     = ship.Name,
                        ShipIndex    = ship.Index
                    });
                }
            }

            return result;
        }

        // ─── 4. Fleet Summary ────────────────────────────────────────────────

        private static FleetSummary ComputeFleetSummary(
            PwsSnapshot snapshot, DateOnly date,
            List<ShipDailyState> shipStates)
        {
            int damaged = shipStates.Count(s =>
                s.Status == "Active" && (s.SysDmg > 0 || s.FloatDmg > 0 || s.EngDmg > 0));
            int onFire = shipStates.Count(s =>
                s.Status == "Active" && s.Fire > 0);
            int lowFuel = shipStates.Count(s =>
                s.Status == "Active" && s.FuelPct > 0 && s.FuelPct < LowFuelThresholdPct);

            return new FleetSummary
            {
                SnapshotDate         = date,
                TotalActive          = snapshot.AlliedActiveCount,
                AtSea                = snapshot.AlliedAtSeaCount,
                InPort               = snapshot.AlliedInPortCount,
                TotalTfs             = snapshot.AlliedTaskForceCount,
                DamagedCount         = damaged,
                OnFireCount          = onFire,
                LowFuelCount         = lowFuel,
                TotalLost            = snapshot.AlliedSunkCount,
                FutureReinforcements = snapshot.AlliedReinforcementCount
            };
        }

        // ─── 5. Delta-Tracked Lifecycle Events ───────────────────────────────

        private static List<LifecycleEvent> DetectLifecycleEvents(
            PwsSnapshot current, DateOnly date, PwsSnapshot? previous)
        {
            var events = new List<LifecycleEvent>();

            if (previous == null)
            {
                // First snapshot: all sunk ships are historical sinkings
                foreach (var ship in current.AlliedShips.Where(s => s.IsSunk))
                {
                    events.Add(new LifecycleEvent
                    {
                        EventDate    = date,
                        ShipName     = ship.Name,
                        ShipIndex    = ship.Index,
                        EventType    = "Sunk",
                        LocationHexX = ship.HexX,
                        LocationHexY = ship.HexY,
                        Cause        = "Historical sinking (detected in first loaded snapshot)"
                    });
                }
                return events;
            }

            // Build lookup of previous state keyed by ship index
            var prevByIndex = new Dictionary<int, PwsShipRecord>();
            foreach (var s in previous.AlliedShips)
                prevByIndex[s.Index] = s;

            foreach (var ship in current.AlliedShips)
            {
                if (!prevByIndex.TryGetValue(ship.Index, out var prev))
                {
                    // Ship not in previous snapshot
                    if (!ship.IsSunk && !ship.IsReinforcement)
                    {
                        // New arrival (reinforcement countdown reached 0)
                        events.Add(new LifecycleEvent
                        {
                            EventDate    = date,
                            ShipName     = ship.Name,
                            ShipIndex    = ship.Index,
                            EventType    = "Arrived",
                            LocationHexX = ship.HexX,
                            LocationHexY = ship.HexY,
                            Cause        = "Reinforcement arrived (first appearance as active)"
                        });
                    }
                    continue;
                }

                // Delta: newly sunk (was not sunk yesterday, is sunk today)
                if (ship.IsSunk && !prev.IsSunk)
                {
                    events.Add(new LifecycleEvent
                    {
                        EventDate    = date,
                        ShipName     = ship.Name,
                        ShipIndex    = ship.Index,
                        EventType    = "Sunk",
                        LocationHexX = ship.HexX,
                        LocationHexY = ship.HexY,
                        Cause        = "Newly sunk (delta: status changed from active to 19999)"
                    });
                }

                // Delta: reinforcement arrived (was reinforcement, now active)
                if (prev.IsReinforcement && !ship.IsReinforcement && !ship.IsSunk)
                {
                    events.Add(new LifecycleEvent
                    {
                        EventDate    = date,
                        ShipName     = ship.Name,
                        ShipIndex    = ship.Index,
                        EventType    = "Arrived",
                        LocationHexX = ship.HexX,
                        LocationHexY = ship.HexY,
                        Cause        = $"Reinforcement arrived (was {prev.ArrivalDays} days out)"
                    });
                }

                // Delta: newly deployed to sea (was in port, now at sea)
                if (!prev.IsAtSea && ship.IsAtSea && !ship.IsSunk)
                {
                    events.Add(new LifecycleEvent
                    {
                        EventDate    = date,
                        ShipName     = ship.Name,
                        ShipIndex    = ship.Index,
                        EventType    = "Deployed",
                        LocationHexX = ship.HexX,
                        LocationHexY = ship.HexY,
                        Cause        = $"Put to sea in TF {ship.TaskForceIndex}"
                    });
                }

                // Delta: fuel critical (dropped below threshold)
                if (!ship.IsSunk && !ship.IsReinforcement && ship.MaxFuel > 0)
                {
                    int prevPct = prev.MaxFuel > 0 ? (int)Math.Round(100.0 * prev.Fuel / prev.MaxFuel) : 100;
                    int currPct = (int)Math.Round(100.0 * ship.Fuel / ship.MaxFuel);
                    if (prevPct >= LowFuelThresholdPct && currPct < LowFuelThresholdPct)
                    {
                        events.Add(new LifecycleEvent
                        {
                            EventDate    = date,
                            ShipName     = ship.Name,
                            ShipIndex    = ship.Index,
                            EventType    = "FuelCritical",
                            LocationHexX = ship.HexX,
                            LocationHexY = ship.HexY,
                            Cause        = $"Fuel dropped to {currPct}% (was {prevPct}%)"
                        });
                    }
                }
            }

            return events;
        }

        // ─── 6. Operations Report Entries ────────────────────────────────────

        private static List<OperationsReportEntry> ExtractReportEntries(
            PwsSnapshot snapshot, DateOnly date)
        {
            var result = new List<OperationsReportEntry>();
            int eventId = 0;

            foreach (var section in snapshot.Reports)
            {
                string category = section.Kind switch
                {
                    PwsReportSection.SectionKind.CombatReport  => "Combat",
                    PwsReportSection.SectionKind.CombatEvents  => "Combat",
                    PwsReportSection.SectionKind.Operations    => "Movement",
                    PwsReportSection.SectionKind.Sigint        => "Sigint",
                    _ => "Unknown"
                };

                foreach (var line in section.Lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    int hexX = 0, hexY = 0;
                    var hexMatch = HexCoordRegex.Match(line);
                    if (hexMatch.Success)
                    {
                        int.TryParse(hexMatch.Groups[1].ValueSpan, out hexX);
                        int.TryParse(hexMatch.Groups[2].ValueSpan, out hexY);
                    }

                    int tfId = 0;
                    var tfMatch = TfRefRegex.Match(line);
                    if (tfMatch.Success)
                        int.TryParse(tfMatch.Groups[1].ValueSpan, out tfId);

                    result.Add(new OperationsReportEntry
                    {
                        ReportDate      = date,
                        EventId         = eventId++,
                        EventCategory   = category,
                        TextRaw         = line.Trim(),
                        ParsedLocationX = hexX,
                        ParsedLocationY = hexY,
                        InvolvedTfId    = tfId,
                        InvolvedShips   = string.Empty // Parsed by downstream analytics
                    });
                }
            }

            return result;
        }

        // ─── Helper: Location Text Resolution ────────────────────────────────

        private static string ResolveLocationText(PwsShipRecord ship)
        {
            if (ship.IsSunk) return "Sunk";
            if (ship.IsReinforcement) return "En Route (Reinforcement)";
            if (ship.IsAtSea) return $"At Sea, TF {ship.TaskForceIndex} ({ship.HexX},{ship.HexY})";

            // In port — try to resolve via BaseLocationRegistry
            var registry = BaseLocationRegistry.Instance;
            foreach (var b in registry.AllBases)
            {
                if (b.HexX == ship.HexX && b.HexY == ship.HexY)
                    return $"In port: {b.Name} ({ship.HexX},{ship.HexY})";
            }
            return $"In port ({ship.HexX},{ship.HexY})";
        }

        private static string ResolveLocationName(
            int locationIndex, Dictionary<int, PwsLocationRecord> locationLookup)
        {
            if (locationIndex <= 0) return string.Empty;
            if (!locationLookup.TryGetValue(locationIndex, out var loc)) return string.Empty;

            // Try BaseLocationRegistry for name
            var registry = BaseLocationRegistry.Instance;
            foreach (var b in registry.AllBases)
            {
                if (b.HexX == loc.HexX && b.HexY == loc.HexY)
                    return b.Name;
            }
            return $"({loc.HexX},{loc.HexY})";
        }

        private static int ComputeLowestFuelPct(int tfIndex, List<PwsShipRecord> allShips)
        {
            int lowest = 100;
            bool found = false;
            foreach (var ship in allShips)
            {
                if (ship.TaskForceIndex != tfIndex || ship.IsSunk || ship.IsReinforcement)
                    continue;
                if (ship.MaxFuel <= 0) continue;
                found = true;
                int pct = (int)Math.Round(100.0 * ship.Fuel / ship.MaxFuel);
                if (pct < lowest) lowest = pct;
            }
            return found ? lowest : 0;
        }

        // ─── Lookup Builders ─────────────────────────────────────────────────

        private static Dictionary<int, PwsClassRecord> BuildClassLookup(PwsSnapshot snap)
        {
            var dict = new Dictionary<int, PwsClassRecord>(snap.ShipClasses.Count);
            foreach (var c in snap.ShipClasses)
                dict[c.Index] = c;
            return dict;
        }

        private static Dictionary<int, PwsShipRecord> BuildShipIndexLookup(PwsSnapshot snap)
        {
            var dict = new Dictionary<int, PwsShipRecord>(snap.AlliedShips.Count);
            foreach (var s in snap.AlliedShips)
                dict[s.Index] = s;
            return dict;
        }

        private static Dictionary<int, PwsLocationRecord> BuildLocationLookup(PwsSnapshot snap)
        {
            var dict = new Dictionary<int, PwsLocationRecord>(snap.Locations.Count);
            foreach (var loc in snap.Locations)
                dict[loc.Index] = loc;
            return dict;
        }

        // ─── Date Parsing ────────────────────────────────────────────────────

        private static bool TryParseDate(string yymmdd, out DateOnly result)
        {
            result = default;
            if (yymmdd.Length != 6) return false;
            ReadOnlySpan<char> span = yymmdd.AsSpan();
            if (!int.TryParse(span.Slice(0, 2), out int yy)) return false;
            if (!int.TryParse(span.Slice(2, 2), out int mm)) return false;
            if (!int.TryParse(span.Slice(4, 2), out int dd)) return false;
            try
            {
                result = new DateOnly(1900 + yy, mm, dd);
                return true;
            }
            catch { return false; }
        }
    }
}
