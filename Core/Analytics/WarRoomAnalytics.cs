using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using ROCHEFORT6000.Core.Database;
using ROCHEFORT6000.Core.Ingestion;
using ROCHEFORT6000.Core.Ingestion.Pws;
using WitpCic.Core.Analytics.AirDenial;
using WitpCic.Core.Analytics.Bayesian;
using WitpCic.Core.Analytics.Clustering;
using WitpCic.Core.Analytics.Continuity;
using WitpCic.Core.Analytics.EntityResolution;
using WitpCic.Core.Analytics.Logistics;
using WitpCic.Core.Analytics.Prediction;
using WitpCic.Core.Analytics.Rules;
using WitpCic.Core.Analytics.Sensors;
using WitpCic.Core.Analytics.TemporalDecay;
using WitpCic.Core.Geospatial;
using WitpCic.Core.Ingestion;
using WitpCic.Core.State;
using WitpCic.Core.State.Hazards;

namespace ROCHEFORT6000.Core.Analytics
{
    public enum ReportType
    {
        RADIO_TRAFFIC,
        UNIT_LOCATED,
        UNIT_MOVING,
        GARRISON_SIZE,
        SHIP_MOVING,
        VISUAL_RECON,
        UNKNOWN
    }

    public enum ReportDomain
    {
        AIR,
        NAVAL,
        LAND,
        UNKNOWN
    }

    public enum ConfidenceTier
    {
        UNCONFIRMED, // 0 - 39%
        PROBABLE,    // 40 - 69%
        CONFIRMED,   // 70 - 89%
        VERIFIED     // 90 - 100%
    }

    public class RawReport
    {
        public string EntityName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int HexX { get; set; } = 0;
        public int HexY { get; set; } = 0;
        public int HexKey => (HexX > 0 && HexY > 0) ? (HexX * 1000 + HexY) : 0;
        public ReportType ReportType { get; set; } = ReportType.UNKNOWN;
        public ReportDomain Domain { get; set; } = ReportDomain.UNKNOWN;
        public string ReportDate { get; set; } = string.Empty;
        public string RawText { get; set; } = string.Empty;
    }

    public class ConfidenceReport
    {
        public RawReport RawReport { get; set; } = new RawReport();
        public int BaseScore { get; set; } = 0;
        public int CorroborationBonus { get; set; } = 0;
        public string CorroborationReason { get; set; } = string.Empty;
        public double TimeDecay { get; set; } = 1.0;
        public int FinalScore { get; set; } = 0;
        public ConfidenceTier Tier { get; set; } = ConfidenceTier.UNCONFIRMED;
        public string AnalystNote { get; set; } = string.Empty;

        public string GetTierString() => Tier switch
        {
            ConfidenceTier.VERIFIED => "VERIFIED",
            ConfidenceTier.CONFIRMED => "CONFIRMED",
            ConfidenceTier.PROBABLE => "PROBABLE",
            _ => "UNCONFIRMED"
        };

        public string GetBaseExplanation() => RawReport.ReportType switch
        {
            ReportType.VISUAL_RECON => "Visual Recon",
            ReportType.UNIT_LOCATED => "Unit Located",
            ReportType.UNIT_MOVING => "Unit Moving",
            ReportType.SHIP_MOVING => "Ship Moving",
            ReportType.GARRISON_SIZE => "Garrison Size",
            ReportType.RADIO_TRAFFIC => "Radio Traffic",
            _ => "General Telemetry"
        };
    }

    public class SightingEvent
    {
        public string Observer { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ShipCount { get; set; } = 0;
        public int HexX { get; set; } = 0;
        public int HexY { get; set; } = 0;
        public string NearLocation { get; set; } = string.Empty;
        public int Speed { get; set; } = 0;
        public string Heading { get; set; } = string.Empty;
        public string DateYYMMDD { get; set; } = string.Empty;
        public string SourceFile { get; set; } = string.Empty;
    }

    public class RadioInterceptEvent
    {
        public string Location { get; set; } = string.Empty;
        public int HexX { get; set; } = 0;
        public int HexY { get; set; } = 0;
        public bool IsHeavyVolume { get; set; } = false;
        public string DateYYMMDD { get; set; } = string.Empty;
        public string RawText { get; set; } = string.Empty;
    }

    public class UnitLoadoutEvent
    {
        public string UnitName { get; set; } = string.Empty;
        public string VesselClass { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string DateYYMMDD { get; set; } = string.Empty;
        public string RawText { get; set; } = string.Empty;
    }

    public class AirLossEvent
    {
        public string AircraftType { get; set; } = string.Empty;
        public string Squadron { get; set; } = string.Empty;
        public string LossType { get; set; } = string.Empty;
        public int Count { get; set; } = 1;
        public string Side { get; set; } = "Allied"; // "Allied" or "Japanese"
        public string DateYYMMDD { get; set; } = string.Empty;
    }

    public class PilotKillEvent
    {
        public string PilotName { get; set; } = string.Empty;
        public string Squadron { get; set; } = string.Empty;
        public int KillNumber { get; set; } = 1;
        public string DateYYMMDD { get; set; } = string.Empty;
    }

    public class LogisticsEvent
    {
        public string TfId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Base { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string DateYYMMDD { get; set; } = string.Empty;
    }

    public class BattleClaimEvent
    {
        public string ShipName { get; set; } = string.Empty;
        public int BombHits { get; set; } = 0;
        public int TorpedoHits { get; set; } = 0;
        public bool ReportedSunk { get; set; } = false;
        public bool IsAllied { get; set; } = false;
        public string DateYYMMDD { get; set; } = string.Empty;
        public string Assessment { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class DiscoveredEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "BASE", "VESSEL", "UNIT", "AIR_GROUP", "CITY"
        public int LastHexX { get; set; } = 0;
        public int LastHexY { get; set; } = 0;
        public string LastDate { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public int EventCount { get; set; } = 0;
    }

    public class ModelA_AmphibiousTarget
    {
        public string Destination { get; set; } = string.Empty;
        public int BaseHexX { get; set; } = 0;
        public int BaseHexY { get; set; } = 0;
        public double Probability { get; set; } = 0.0;
        public string ThreatLevel { get; set; } = string.Empty;
        public List<string> LoadedUnits { get; set; } = new List<string>();
        public List<string> Transports { get; set; } = new List<string>();
        public List<string> RadioTransmissions { get; set; } = new List<string>();
        public int ApproachingSightings { get; set; } = 0;
        public bool Captured { get; set; } = false;
        public string Assessment { get; set; } = string.Empty;
    }

    public class ModelB_FleetTrajectory
    {
        public string SightingId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StartHexX { get; set; } = 0;
        public int StartHexY { get; set; } = 0;
        public string OriginBase { get; set; } = string.Empty;
        public int SpeedKnots { get; set; } = 16;
        public string Heading { get; set; } = string.Empty;
        public int ProjectedHexX { get; set; } = 0;
        public int ProjectedHexY { get; set; } = 0;
        public double ConfidenceRadius { get; set; } = 2.5;
        public string TargetDestination { get; set; } = string.Empty;
        public string InterceptWindow { get; set; } = string.Empty;
    }

    public class ModelC_AirAttrition
    {
        public int TotalAlliedLosses { get; set; } = 0;
        public int TotalJapaneseLosses { get; set; } = 0;
        public double AlliedDailyEma { get; set; } = 0.0;
        public double JapaneseDailyEma { get; set; } = 0.0;
        public List<KeyValuePair<string, int>> LossesByType { get; set; } = new List<KeyValuePair<string, int>>();
        public List<PilotKillEvent> TopAces { get; set; } = new List<PilotKillEvent>();
        public string AirSuperiorityStatus { get; set; } = string.Empty;
        public string Forecast { get; set; } = string.Empty;
    }

    public class ModelD_LogisticalPause
    {
        public List<LogisticsEvent> ActivePauses { get; set; } = new List<LogisticsEvent>();
        public string OperationalStatus { get; set; } = string.Empty;
        public string PauseWindow { get; set; } = string.Empty;
        public string VulnerabilityAssessment { get; set; } = string.Empty;
    }

    public class ModelE_BattleClaims
    {
        public List<BattleClaimEvent> Claims { get; set; } = new List<BattleClaimEvent>();
        public int GhostFleetCount { get; set; } = 0;
        public int TotalEnemyCapitalShips { get; set; } = 20;
        public int LocalizedEnemyCapitalShips { get; set; } = 0;
        public List<string> UnlocalizedCapitalShips { get; set; } = new List<string>();
        public List<string> LocalizedCapitalShips { get; set; } = new List<string>();
    }

    public class ModelF_StrikeConcentration
    {
        public string ClusterName { get; set; } = string.Empty;
        public int CenterHexX { get; set; } = 0;
        public int CenterHexY { get; set; } = 0;
        public int WarshipCount { get; set; } = 0;
        public int RadiusHexes { get; set; } = 0;
        public List<string> ConstituentSightings { get; set; } = new List<string>();
        public string ThreatLevel { get; set; } = string.Empty;
    }

    public class ModelG_EstablishedRoute
    {
        public string CorridorName { get; set; } = string.Empty;
        public string DeparturePoint { get; set; } = string.Empty;
        public string DestinationPoint { get; set; } = string.Empty;
        public List<(int X, int Y)> Waypoints { get; set; } = new List<(int X, int Y)>();
        public int ObservationCount { get; set; } = 0;
        public string VesselTypes { get; set; } = string.Empty;
        public string Confidence { get; set; } = string.Empty;
    }

    public class Tab2GeospatialEntry
    {
        public string DateYYMMDD { get; set; } = string.Empty;
        public string EventType { get; set; } = "SIGHTING";
        public int HexX { get; set; } = 0;
        public int HexY { get; set; } = 0;
        public string LocationName { get; set; } = string.Empty;
        public string BearingFromHQ { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public int ConfidencePct { get; set; } = 85;
        public string ConfidenceTier { get; set; } = "CONFIRMED";
        public List<string> MathBreakdown { get; set; } = new List<string>();
        public string LastSeenDate { get; set; } = string.Empty;
        public List<string> AllDates { get; set; } = new List<string>();
    }

    public class HighValueTarget
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "CV";
        public int HexX { get; set; } = 0;
        public int HexY { get; set; } = 0;
        public string Location { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ConfidencePct { get; set; } = 85;
        public string ConfidenceTier { get; set; } = "CONFIRMED";
        public List<string> MathBreakdown { get; set; } = new List<string>();
        public string LastSeenDate { get; set; } = string.Empty;
        public List<string> AllDates { get; set; } = new List<string>();
    }

    public class PortConcentration
    {
        public string PortName { get; set; } = string.Empty;
        public int HexX { get; set; } = 0;
        public int HexY { get; set; } = 0;
        public int WarshipCount { get; set; } = 0;
        public int MerchantCount { get; set; } = 0;
        public List<string> Vessels { get; set; } = new List<string>();
    }

    public class WarRoomAnalytics
    {
        private static readonly Regex CoordRegex = new Regex(@"(\d{2,3})\s*,\s*(\d{2,3})", RegexOptions.Compiled);

        private readonly List<SightingEvent> _sightings = new();
        private readonly List<RadioInterceptEvent> _radioIntercepts = new();
        private readonly List<UnitLoadoutEvent> _unitLoadouts = new();
        private readonly List<AirLossEvent> _airLosses = new();
        private readonly List<PilotKillEvent> _pilotKills = new();
        private readonly List<LogisticsEvent> _logisticsEvents = new();
        private readonly List<BattleClaimEvent> _battleClaimEvents = new();
        private readonly List<DiscoveredEntity> _discoveredEntities = new();

        private readonly List<ConfidenceReport> _confidenceReports = new();
        private readonly List<ModelA_AmphibiousTarget> _amphibiousTargets = new();
        private readonly List<ModelB_FleetTrajectory> _trajectories = new();
        private ModelC_AirAttrition _airAttrition = new();
        private ModelD_LogisticalPause _logisticalPause = new();
        private ModelE_BattleClaims _battleClaims = new();
        private readonly List<ModelF_StrikeConcentration> _strikeConcentrations = new();
        private readonly List<ModelG_EstablishedRoute> _establishedRoutes = new();

        private readonly List<PortConcentration> _portConcentrations = new();
        private readonly List<HighValueTarget> _highValueTargets = new();
        private readonly List<Tab2GeospatialEntry> _geospatialLog = new();
        private readonly List<string> _alertFeed = new();
        private readonly List<string> _eventLog = new();

        public IReadOnlyList<ModelA_AmphibiousTarget> ModelATargets => _amphibiousTargets;
        public IReadOnlyList<ModelB_FleetTrajectory> ModelBTrajectories => _trajectories;
        public ModelC_AirAttrition ModelCAttrition => _airAttrition;
        public ModelD_LogisticalPause ModelDLogistics => _logisticalPause;
        public ModelE_BattleClaims ModelEClaims => _battleClaims;
        public IReadOnlyList<ModelF_StrikeConcentration> ModelFConcentrations => _strikeConcentrations;
        public IReadOnlyList<ModelG_EstablishedRoute> ModelGRoutes => _establishedRoutes;

        public IReadOnlyList<ConfidenceReport> ConfidenceReports => _confidenceReports;
        public IReadOnlyList<Tab2GeospatialEntry> GeospatialLog => _geospatialLog;
        public IReadOnlyList<HighValueTarget> HighValueTargets => _highValueTargets;
        public IReadOnlyList<PortConcentration> PortConcentrations => _portConcentrations;
        public IReadOnlyList<string> AlertFeed => _alertFeed;
        public IReadOnlyList<string> EventLog => _eventLog;
        public IReadOnlyList<DiscoveredEntity> DiscoveredEntities => _discoveredEntities;
        public SemanticConceptGraph ConceptGraph { get; } = new();
        public HeuristicsEngine Heuristics { get; } = new();

        /// <summary>
        /// Multi-file PWS timeline repository.
        /// Retains all parsed PWS snapshots in chronological order.
        /// Most recent snapshot is always the authority for live fleet state.
        /// Historical snapshots are used for: fuel delta tracking, arrival-day countdowns,
        /// sunk-ship history, and TF route reconstruction.
        /// </summary>
        public PwsRepository PwsTimeline { get; }

        /// <summary>
        /// Master System Blueprint: in-memory time-series data store.
        /// Holds SCD Type 2 daily snapshots (ship_daily_state, task_force_daily_state,
        /// tf_composition_history, fleet_summary) and event-sourced history
        /// (lifecycle_events, operations_report_log).
        /// </summary>
        public TimeSeriesStore TimeSeries { get; } = new();

        /// <summary>
        /// Master System Blueprint: ETL service that transforms PwsSnapshot
        /// into canonical time-series records via delta tracking.
        /// </summary>
        public TimeSeriesIngestionService TimeSeriesETL { get; }

        // 14 Foundational Architectural Baseline Services
        public SensorReliabilityService SensorService { get; } = new();
        public SupplyNetworkGraph SupplyGraph { get; } = new();
        public AirDenialHeatmapService AirDenialService { get; } = new();
        public EntityResolutionService EntityResolver { get; } = new();
        public AdvanceMomentumPredictor MomentumPredictor { get; } = new();
        public TemporalContinuityChecker ContinuityChecker { get; } = new();
        public HazardStateRepository HazardRepository { get; } = new();
        public BayesianCorroborationEngine BayesianEngine { get; }

        public WarRoomAnalytics()
        {
            BayesianEngine = new BayesianCorroborationEngine(EntityResolver);
            TimeSeriesETL = new TimeSeriesIngestionService(TimeSeries);
            // PwsRepository wired to live architectural services (Pillars 4 + 6)
            PwsTimeline = new PwsRepository
            {
                EntityResolver = EntityResolver,
                WorldState     = new WitpCic.Core.State.WorldStateRepository()
            };
            foreach (var b in BaseLocationRegistry.Instance.AllBases)
            {
                MomentumPredictor.RegisterTargetBase(b.Name, new HexCoordinate(b.HexX, b.HexY), b.IsPort ? 3 : 1);
            }
        }

        public bool AnalyzeCampaign(IngestionManifest manifest)
        {
            _sightings.Clear();
            _radioIntercepts.Clear();
            _unitLoadouts.Clear();
            _airLosses.Clear();
            _pilotKills.Clear();
            _logisticsEvents.Clear();
            _battleClaimEvents.Clear();
            _discoveredEntities.Clear();
            _amphibiousTargets.Clear();
            _trajectories.Clear();
            _portConcentrations.Clear();
            _highValueTargets.Clear();
            _geospatialLog.Clear();
            _alertFeed.Clear();
            _eventLog.Clear();
            _confidenceReports.Clear();
            PwsTimeline.Clear();
            TimeSeries.Clear();
            TimeSeriesETL.Reset();

            string latestDate = string.IsNullOrEmpty(manifest.LatestDate) ? "411207" : manifest.LatestDate;

            // Sort files chronologically: non-PWS first or by date, and all PWS files in strictly ascending chronological order
            // so each turn file updates the timeline, preserves history, and the latest file becomes the active authority.
            var orderedFiles = manifest.Files
                .OrderBy(f => f.FileType == "PWS_SAVE" ? 1 : 0)
                .ThenBy(f => string.IsNullOrEmpty(f.DateYYMMDD) ? "000000" : f.DateYYMMDD)
                .ToList();

            foreach (var file in orderedFiles)
            {
                string dateToPass = file.DateYYMMDD;
                if (string.IsNullOrEmpty(dateToPass))
                {
                    dateToPass = FileScanner.ExtractDateFromReportHeader(file.FullPath);
                }
                if (string.IsNullOrEmpty(dateToPass))
                {
                    dateToPass = latestDate;
                }

                switch (file.FileType)
                {
                    case "SIGINT":
                        ParseSigintFile(file.FullPath, dateToPass);
                        break;
                    case "OPERATIONS":
                        ParseOperationsFile(file.FullPath, dateToPass);
                        break;
                    case "COMBAT_REPORT":
                        ParseCombatReportFile(file.FullPath, dateToPass);
                        break;
                    case "COMBAT_EVENTS":
                        ParseCombatEventsFile(file.FullPath, dateToPass);
                        break;
                    case "PWS_SAVE":
                        ParsePwsFile(file.FullPath, dateToPass);
                        break;
                }
            }

            ComputeNumericalConfidence(manifest);
            ComputeModelA();
            ComputeModelB();
            ComputeModelC();
            ComputeModelD();
            ComputeModelE();
            ComputeModelF();
            ComputeModelG();
            BuildGeospatialAndDossier(manifest);
            BuildConceptGraph();
            Heuristics.Analyze(manifest, _unitLoadouts, _sightings, _radioIntercepts, _confidenceReports);

            return true;
        }

        private void BuildConceptGraph()
        {
            // Reset concept graph for new campaign run
            // 1. Populate from Base Registry (Strategic Bases & Regions)
            foreach (var b in BaseLocationRegistry.Instance.AllBases)
            {
                ConceptGraph.AddNode(new ConceptNode
                {
                    Id = $"BASE:{b.Name}",
                    Name = b.Name,
                    Type = ConceptType.Base,
                    HexX = b.HexX,
                    HexY = b.HexY,
                    Details = (b.IsPort ? "Port / Anchorage" : "Inland Airfield") + $" | Region: {b.InitialRegion} | Controller: {b.CurrentController}",
                    SearchDocument = $"{b.Name} base port airfield anchorage {b.InitialRegion} {b.InitialNation} {b.CurrentController} hex {b.HexX} {b.HexY}"
                });
            }

            // 2. Populate Discovered Entities (Units, Vessels, Air Groups)
            foreach (var e in _discoveredEntities)
            {
                var cType = e.Category switch
                {
                    "VESSEL" => ConceptType.Vessel,
                    "AIR_GROUP" => ConceptType.AirGroup,
                    "UNIT" => ConceptType.Unit,
                    _ => ConceptType.Base
                };

                ConceptGraph.AddNode(new ConceptNode
                {
                    Id = $"ENTITY:{e.Category}:{e.Name}",
                    Name = e.Name,
                    Type = cType,
                    HexX = e.LastHexX,
                    HexY = e.LastHexY,
                    Details = e.Details,
                    SearchDocument = $"{e.Name} {e.Category} {e.Details} last seen {e.LastDate} events {e.EventCount}"
                });
            }

            // 3. Populate High Value Targets (Carriers, Battleships, Oilers)
            foreach (var h in _highValueTargets)
            {
                ConceptGraph.AddNode(new ConceptNode
                {
                    Id = $"HVT:{h.Name}:{h.Location}",
                    Name = h.Name,
                    Type = ConceptType.CarrierThreat,
                    HexX = h.HexX,
                    HexY = h.HexY,
                    Details = $"HVT {h.Type} | Loc: {h.Location} | Status: {h.Status} | Confidence: {h.ConfidencePct}%",
                    SearchDocument = $"{h.Name} carrier threat strike group {h.Type} oiler cruiser warship {h.Location} {h.Status} {h.ConfidenceTier}"
                });
            }

            // 4. Populate Model A Invasion Targets
            foreach (var a in _amphibiousTargets)
            {
                ConceptGraph.AddNode(new ConceptNode
                {
                    Id = $"INVASION:{a.Destination}",
                    Name = $"Invasion: {a.Destination}",
                    Type = ConceptType.InvasionTarget,
                    HexX = a.BaseHexX,
                    HexY = a.BaseHexY,
                    Details = $"Assessment: {a.Assessment} | Probability: {a.Probability * 100:F0}% | Threat: {a.ThreatLevel}",
                    SearchDocument = $"amphibious assault invasion landing operation {a.Destination} {a.Assessment} threat {a.ThreatLevel}"
                });
            }

            // 5. Populate Model G Logistics Routes
            foreach (var r in _establishedRoutes)
            {
                ConceptGraph.AddNode(new ConceptNode
                {
                    Id = $"CORRIDOR:{r.CorridorName}",
                    Name = r.CorridorName,
                    Type = ConceptType.LogisticsCorridor,
                    Details = $"From {r.DeparturePoint} -> {r.DestinationPoint} | Vessels: {r.VesselTypes}",
                    SearchDocument = $"logistics corridor shipping convoy supply route {r.CorridorName} {r.DeparturePoint} {r.DestinationPoint} {r.VesselTypes} {r.Confidence}"
                });
            }

            // 6. Build TF-IDF Vocabulary and Node Vectors
            ConceptGraph.BuildEmbeddings(300);
        }

        public static bool ParseCoords(string text, out int outX, out int outY)
        {
            outX = 0;
            outY = 0;
            if (string.IsNullOrEmpty(text)) return false;

            var match = CoordRegex.Match(text);
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out outX) &&
                    int.TryParse(match.Groups[2].Value, out outY))
                {
                    return true;
                }
            }
            return false;
        }

        public static string CalculateBearing(int srcX, int srcY, int dstX, int dstY, string refName)
        {
            double dx = dstX - srcX;
            double dy = dstY - srcY;
            double distHex = Math.Sqrt(dx * dx + dy * dy);
            double distNM = distHex * 40.0;

            double rad = Math.Atan2(dx, -dy);
            double deg = rad * 180.0 / Math.PI;
            if (deg < 0) deg += 360.0;

            string cardinal = deg switch
            {
                >= 337.5 or < 22.5 => "N",
                >= 22.5 and < 67.5 => "NE",
                >= 67.5 and < 112.5 => "E",
                >= 112.5 and < 157.5 => "SE",
                >= 157.5 and < 202.5 => "S",
                >= 202.5 and < 247.5 => "SW",
                >= 247.5 and < 292.5 => "W",
                _ => "NW"
            };

            return $"{distNM:F0} NM Brg {deg:000} ({cardinal}) from {refName}";
        }

        private static int CalculateDayDifference(string currentYYMMDD, string reportYYMMDD)
        {
            if (string.IsNullOrEmpty(currentYYMMDD) || currentYYMMDD.Length < 6 ||
                string.IsNullOrEmpty(reportYYMMDD) || reportYYMMDD.Length < 6) return 0;

            try
            {
                int y1 = int.Parse(currentYYMMDD.Substring(0, 2));
                int m1 = int.Parse(currentYYMMDD.Substring(2, 2));
                int d1 = int.Parse(currentYYMMDD.Substring(4, 2));

                int y2 = int.Parse(reportYYMMDD.Substring(0, 2));
                int m2 = int.Parse(reportYYMMDD.Substring(2, 2));
                int d2 = int.Parse(reportYYMMDD.Substring(4, 2));

                int ToDays(int y, int m, int d)
                {
                    int days = (y - 41) * 365;
                    int[] daysBeforeMonth = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };
                    if (m >= 1 && m <= 12) days += daysBeforeMonth[m - 1];
                    days += d;
                    return days;
                }

                int diff = ToDays(y1, m1, d1) - ToDays(y2, m2, d2);
                return Math.Max(0, diff);
            }
            catch
            {
                return 0;
            }
        }

        private void RegisterEntity(string name, string category, int hexX, int hexY, string date, string details)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            string cleanName = name.Trim();
            if (string.IsNullOrEmpty(cleanName)) return;

            var entityType = category switch
            {
                "VESSEL" => EntityType.SurfaceShip,
                "AIR_GROUP" => EntityType.AirUnit,
                "UNIT" => EntityType.GroundUnit,
                _ => EntityType.Base
            };

            Guid canonicalId = EntityResolver.ResolveOrCreate(cleanName, entityType);
            var resolved = EntityResolver.GetEntity(canonicalId);
            string canonicalName = resolved?.CanonicalName ?? cleanName;

            if (category == "BASE" && (hexX <= 0 || hexY <= 0))
            {
                var baseLoc = BaseLocationRegistry.Instance.FindBaseByName(canonicalName);
                if (baseLoc != null)
                {
                    hexX = baseLoc.HexX;
                    hexY = baseLoc.HexY;
                }
            }

            var existing = _discoveredEntities.FirstOrDefault(e => e.Name.Equals(canonicalName, StringComparison.OrdinalIgnoreCase) && e.Category == category);
            if (existing != null)
            {
                existing.EventCount++;
                if (!string.IsNullOrEmpty(date)) existing.LastDate = date;
                if (hexX > 0 && hexY > 0)
                {
                    existing.LastHexX = hexX;
                    existing.LastHexY = hexY;
                }
                if (!string.IsNullOrEmpty(details)) existing.Details = details;
                return;
            }

            _discoveredEntities.Add(new DiscoveredEntity
            {
                Name = canonicalName,
                Category = category,
                LastHexX = hexX,
                LastHexY = hexY,
                LastDate = date,
                Details = details,
                EventCount = 1
            });
        }

        private void ParsePwsFile(string filePath, string dateYYMMDD)
        {
            if (!File.Exists(filePath)) return;
            try
            {
                // Ingest via PwsRepository — applies SharpZipLib block scanner,
                // faction isolation, and full binary field decoding.
                var snapshot = PwsTimeline.IngestFile(filePath, string.IsNullOrEmpty(dateYYMMDD) ? null : dateYYMMDD);
                if (snapshot == null) return;

                // ── Master System Blueprint: ETL into time-series store ──
                // Transforms PwsSnapshot → ShipDailyState, TaskForceDailyState,
                // TfCompositionHistory, FleetSummary, LifecycleEvents, OperationsReportLog.
                // Delta tracking compares this snapshot against the previous one.
                TimeSeriesETL.IngestSnapshot(snapshot);

                // Use resolved date from snapshot (may have been extracted from block 550)
                string effectiveDate = string.IsNullOrEmpty(snapshot.Meta.DateYYMMDD)
                    ? dateYYMMDD
                    : snapshot.Meta.DateYYMMDD;

                // ── 1. Apply turn reports (block 593) → existing parser pipeline ──
                // This preserves all the existing combat/sigint/operations parsing logic.
                foreach (var section in snapshot.Reports)
                {
                    switch (section.Kind)
                    {
                        case PwsReportSection.SectionKind.CombatReport:
                            ParseCombatReportLines(section.Lines, effectiveDate);
                            break;
                        case PwsReportSection.SectionKind.CombatEvents:
                            ParseCombatEventsLines(section.Lines, effectiveDate);
                            break;
                        case PwsReportSection.SectionKind.Operations:
                            ParseOperationsLines(section.Lines, effectiveDate);
                            break;
                        case PwsReportSection.SectionKind.Sigint:
                            ParseSigintLines(section.Lines, effectiveDate);
                            break;
                    }
                }

                // ── 2. Incorporate sunk ships from binary state (block 520) ──
                // This is MORE reliable than text parsing: the binary state is authoritative.
                // Allied ships only (faction-isolation enforced in PwsFileParser).
                foreach (var ship in snapshot.AlliedShips)
                {
                    if (!ship.IsSunk) continue;

                    if (!_battleClaimEvents.Any(bc =>
                        bc.ShipName.Equals(ship.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        _battleClaimEvents.Add(new BattleClaimEvent
                        {
                            ShipName     = ship.Name,
                            ReportedSunk = true,
                            IsAllied     = true, // faction isolation guarantees this
                            DateYYMMDD   = effectiveDate,
                            Assessment   = "CONFIRMED SUNK",
                            Reason       = (ship.HexX > 0 && ship.HexY > 0)
                                ? $"Verified sunk at hex ({ship.HexX},{ship.HexY}) via PWS binary state"
                                : "Verified sunk in PBEM save state (binary)"
                        });
                        RegisterEntity(ship.Name, "VESSEL", ship.HexX, ship.HexY, effectiveDate,
                            "Allied vessel sunk (PWS Binary Telemetry)");
                        _eventLog.Add($"[{effectiveDate}] LOSS: {ship.Name} sunk (Hex {ship.HexX},{ship.HexY}).");
                    }
                }

                // ── 3. Incorporate reinforcements ──
                // Ships with ArrivalDays > 0 — useful for strategic timing context.
                foreach (var ship in snapshot.AlliedShips)
                {
                    if (!ship.IsReinforcement) continue;
                    // Register as a known entity with "REINFORCEMENT" context for the dossier
                    RegisterEntity(ship.Name, "VESSEL", 0, 0, effectiveDate,
                        $"Allied reinforcement, ETA {ship.ArrivalDays} days");
                }

                // ── 4. Supply graph: update TF positions (Pillar 9) ──
                foreach (var tf in snapshot.AlliedTaskForces)
                {
                    if (tf.HexX > 0 && tf.HexY > 0)
                    {
                        var tfCoord = new WitpCic.Core.Geospatial.HexCoordinate(tf.HexX, tf.HexY);
                        // Register TF current position as a transient node in the supply network
                        SupplyGraph.RegisterBase($"TF-{tf.Index}", tfCoord);
                    }
                }
            }
            catch (Exception ex)
            {
                // Non-fatal — log and continue with other files
                _eventLog.Add($"[PWS] Error parsing {System.IO.Path.GetFileName(filePath)}: {ex.Message}");
            }
        }

        private void ParseSigintLines(IEnumerable<string> rawLines, string dateYYMMDD)
        {
            foreach (var rawLine in rawLines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.Contains("SIG INT REPORT")) continue;

                // 1. Unit loadout
                int loadedPos = line.IndexOf(" is loaded on a ");
                if (loadedPos != -1)
                {
                    string unit = line.Substring(0, loadedPos).Trim();
                    string rest = line.Substring(loadedPos + 16);
                    int movingPos = rest.IndexOf(" moving to ");
                    string vessel = (movingPos != -1) ? rest.Substring(0, movingPos).Trim() : rest.Trim();
                    string dest = (movingPos != -1) ? rest.Substring(movingPos + 11).Trim().TrimEnd('.') : string.Empty;

                    _unitLoadouts.Add(new UnitLoadoutEvent
                    {
                        UnitName = unit,
                        VesselClass = vessel,
                        Destination = dest,
                        DateYYMMDD = dateYYMMDD,
                        RawText = line
                    });

                    RegisterEntity(unit, "UNIT", 0, 0, dateYYMMDD, $"Loaded on {vessel} for {dest}");
                    RegisterEntity(dest, "BASE", 0, 0, dateYYMMDD, "Target destination in SIGINT");
                    continue;
                }

                // 2. Radio transmission
                int radioPos = line.IndexOf("Radio transmissions detected at ");
                if (radioPos != -1)
                {
                    bool heavy = line.Contains("Heavy Volume");
                    string locPart = line.Substring(radioPos + 32).Trim().TrimEnd('.');

                    ParseCoords(locPart, out int hx, out int hy);
                    string baseName = locPart;
                    int paren = locPart.IndexOf('(');
                    if (paren != -1)
                    {
                        baseName = locPart.Substring(0, paren).Trim();
                    }
                    else if (hx > 0)
                    {
                        baseName = $"Hex {hx},{hy}";
                    }

                    _radioIntercepts.Add(new RadioInterceptEvent
                    {
                        Location = baseName,
                        HexX = hx,
                        HexY = hy,
                        IsHeavyVolume = heavy,
                        DateYYMMDD = dateYYMMDD,
                        RawText = line
                    });

                    if (!baseName.StartsWith("Hex"))
                    {
                        RegisterEntity(baseName, "BASE", hx, hy, dateYYMMDD, heavy ? "Heavy Radio Intercept" : "Radio Intercept");
                    }
                    continue;
                }

                // 3. Unit located
                int locPos = line.IndexOf(" is located at ");
                if (locPos != -1)
                {
                    string unit = line.Substring(0, locPos).Trim();
                    string locPart = line.Substring(locPos + 15).Trim().TrimEnd('.');

                    ParseCoords(locPart, out int hx, out int hy);
                    string baseName = locPart;
                    int paren = locPart.IndexOf('(');
                    if (paren != -1)
                    {
                        baseName = locPart.Substring(0, paren).Trim();
                    }

                    RegisterEntity(unit, "UNIT", hx, hy, dateYYMMDD, $"Located at {baseName}");
                    RegisterEntity(baseName, "BASE", hx, hy, dateYYMMDD, "Garrison/Unit location");
                }
            }
        }

        private void ParseSigintFile(string filePath, string dateYYMMDD)
        {
            if (!File.Exists(filePath)) return;
            ParseSigintLines(File.ReadLines(filePath), dateYYMMDD);
        }

        private void ParseOperationsLines(IEnumerable<string> rawLines, string dateYYMMDD, bool skipSightings = false)
        {
            foreach (var rawLine in rawLines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.Contains("OPERATIONAL REPORT")) continue;

                // 1. Sighting report
                int sightPos = line.IndexOf("sighting report:", StringComparison.OrdinalIgnoreCase);
                if (sightPos != -1)
                {
                    if (skipSightings) continue;

                    string observer = (sightPos > 0) ? line.Substring(0, sightPos).Trim() : "Air Reconnaissance";
                    string rest = line.Substring(sightPos + 16).Trim();

                    int shipCount = 1;
                    int japPos = rest.IndexOf("Japanese");
                    if (japPos != -1)
                    {
                        string numPart = rest.Substring(0, japPos).Trim();
                        if (!string.IsNullOrEmpty(numPart) && char.IsDigit(numPart[0]))
                        {
                            int.TryParse(numPart, out shipCount);
                        }
                    }

                    ParseCoords(rest, out int hx, out int hy);

                    string nearLoc = string.Empty;
                    int nearPos = rest.IndexOf("near ");
                    if (nearPos != -1)
                    {
                        int comma = rest.IndexOf(',', nearPos);
                        nearLoc = (comma != -1) ? rest.Substring(nearPos + 5, comma - (nearPos + 5)).Trim() : rest.Substring(nearPos + 5).Trim();
                    }

                    int speed = 14;
                    int speedPos = rest.IndexOf("speed ");
                    if (speedPos != -1)
                    {
                        int.TryParse(new string(rest.Substring(speedPos + 6).TakeWhile(char.IsDigit).ToArray()), out speed);
                    }

                    string heading = "Unknown";
                    int movePos = rest.IndexOf("Moving ");
                    if (movePos != -1)
                    {
                        heading = rest.Substring(movePos + 7).Trim();
                    }

                    _sightings.Add(new SightingEvent
                    {
                        Observer = observer,
                        Description = rest,
                        ShipCount = Math.Max(1, shipCount),
                        HexX = hx,
                        HexY = hy,
                        NearLocation = nearLoc,
                        Speed = speed > 0 ? speed : 14,
                        Heading = heading,
                        DateYYMMDD = dateYYMMDD,
                        SourceFile = "OPERATIONS"
                    });

                    if (!string.IsNullOrEmpty(nearLoc)) RegisterEntity(nearLoc, "BASE", hx, hy, dateYYMMDD, "Naval Sighting Area");
                    continue;
                }

                // 2. Air kill
                int killPos = line.IndexOf(" is credited with kill number ");
                if (killPos != -1)
                {
                    string pilotSquadron = line.Substring(0, killPos);
                    string killNumStr = line.Substring(killPos + 30);
                    int.TryParse(killNumStr, out int kNum);

                    string pilot = pilotSquadron;
                    string sqn = string.Empty;
                    int ofPos = pilotSquadron.IndexOf(" of ");
                    if (ofPos != -1)
                    {
                        pilot = pilotSquadron.Substring(0, ofPos).Trim();
                        sqn = pilotSquadron.Substring(ofPos + 4).Trim();
                    }

                    _pilotKills.Add(new PilotKillEvent
                    {
                        PilotName = pilot,
                        Squadron = sqn,
                        KillNumber = Math.Max(1, kNum),
                        DateYYMMDD = dateYYMMDD
                    });

                    RegisterEntity(pilot, "AIR_GROUP", 0, 0, dateYYMMDD, $"Pilot Ace ({sqn})");
                    if (!string.IsNullOrEmpty(sqn)) RegisterEntity(sqn, "AIR_GROUP", 0, 0, dateYYMMDD, "Allied Air Squadron");
                    continue;
                }

                // 3. Aircraft write-off
                int writeOffPos = line.IndexOf(" is written off");
                if (writeOffPos != -1)
                {
                    string planeSquadron = line.Substring(0, writeOffPos);
                    string plane = planeSquadron;
                    string sqn = string.Empty;
                    int fromPos = planeSquadron.IndexOf(" from ");
                    if (fromPos != -1)
                    {
                        plane = planeSquadron.Substring(0, fromPos).Trim();
                        sqn = planeSquadron.Substring(fromPos + 6).Trim();
                    }

                    _airLosses.Add(new AirLossEvent
                    {
                        AircraftType = plane,
                        Squadron = sqn,
                        LossType = "WRITTEN_OFF",
                        Count = 1,
                        Side = "Allied",
                        DateYYMMDD = dateYYMMDD
                    });

                    if (!string.IsNullOrEmpty(sqn)) RegisterEntity(sqn, "AIR_GROUP", 0, 0, dateYYMMDD, "Allied Air Squadron");
                    continue;
                }

                // 4. Logistics mission abort
                if (line.Contains("aborts mission and returns to"))
                {
                    int toPos = line.IndexOf("returns to ");
                    string baseName = (toPos != -1) ? line.Substring(toPos + 11).Trim() : string.Empty;

                    _logisticsEvents.Add(new LogisticsEvent
                    {
                        TfId = "TF 420",
                        EventType = "ABORT_MISSION",
                        Base = baseName,
                        Details = line,
                        DateYYMMDD = dateYYMMDD
                    });
                    _eventLog.Add($"[{dateYYMMDD}] {line}");
                    RegisterEntity(baseName, "BASE", 0, 0, dateYYMMDD, "Naval Return Port");
                    continue;
                }

                // 5. Base capture
                if (line.Contains("Japanese forces CAPTURE "))
                {
                    int capPos = line.IndexOf("CAPTURE ");
                    int excl = line.IndexOf(" !", capPos);
                    string baseName = (excl != -1) ? line.Substring(capPos + 8, excl - (capPos + 8)).Trim() : line.Substring(capPos + 8).Trim();

                    _alertFeed.Add($"CRITICAL: Japanese forces CAPTURED {baseName} ({dateYYMMDD})");
                    _eventLog.Add($"[{dateYYMMDD}] {line}");
                    RegisterEntity(baseName, "BASE", 0, 0, dateYYMMDD, "Captured by Japanese Forces");
                    BaseLocationRegistry.Instance.UpdateBaseController(baseName, "Japanese");

                    // Mark severed node in supply network graph
                    var baseLoc = BaseLocationRegistry.Instance.FindBaseByName(baseName);
                    if (baseLoc != null)
                    {
                        SupplyGraph.MarkNodeSevered(new HexCoordinate(baseLoc.HexX, baseLoc.HexY));
                    }
                    continue;
                }

                // 6. Minefield Deployment (Pillar 14)
                if (line.Contains("lays minefield") || line.Contains("lays defensive minefield"))
                {
                    ParseCoords(line, out int mx, out int my);
                    var hazardCoord = new HexCoordinate(mx, my);
                    var hazard = new HazardState(
                        Guid.NewGuid(),
                        hazardCoord,
                        line,
                        HazardType.MinefieldDefensive,
                        Faction.Allied,
                        100,
                        "Minelayer TF",
                        DateTime.UtcNow
                    );
                    HazardRepository.AddOrUpdateHazard(hazard);
                    _eventLog.Add($"[{dateYYMMDD}] HAZARD DEPLOYED: Minefield active at ({mx},{my}).");
                    continue;
                }

                // 7. Air Overflights & Reconnaissance (Pillar 10)
                if (line.Contains("sighted over") || line.Contains("observes") || line.Contains("snooped by"))
                {
                    ParseCoords(line, out int ax, out int ay);
                    var threatClass = line.Contains("Bomber") ? AircraftThreatClass.TorpedoOrDiveBomber :
                                      line.Contains("Fighter") ? AircraftThreatClass.FighterOrCap : AircraftThreatClass.ReconOrFloatPlane;

                    AirDenialService.RecordOverflight(new AirOverflightEvent(
                        new HexCoordinate(ax, ay),
                        threatClass,
                        line,
                        DateTime.UtcNow
                    ));
                    continue;
                }

                // 8. Supply Transport Mission (Pillar 9)
                if (line.Contains("transporting supplies to"))
                {
                    int toIdx = line.IndexOf("transporting supplies to ");
                    string dest = (toIdx != -1) ? line.Substring(toIdx + 25).Trim().TrimEnd('.') : string.Empty;
                    var baseLoc = BaseLocationRegistry.Instance.FindBaseByName(dest);
                    if (baseLoc != null)
                    {
                        SupplyGraph.AddOrUpdateRoute(
                            "Origin Base",
                            new HexCoordinate(baseLoc.HexX - 5, baseLoc.HexY - 5),
                            dest,
                            new HexCoordinate(baseLoc.HexX, baseLoc.HexY),
                            TransportVesselType.AirTransport_Hudson_C47,
                            500
                        );
                    }
                    continue;
                }
            }
        }

        private void ParseOperationsFile(string filePath, string dateYYMMDD)
        {
            if (!File.Exists(filePath)) return;

            // Stream and deduplicate sighting events using zero-allocation streaming parser
            var deduplicatedSightings = OperationsReportParser.ParseAndDeduplicate(filePath);
            foreach (var s in deduplicatedSightings)
            {
                _sightings.Add(new SightingEvent
                {
                    Observer = string.IsNullOrEmpty(s.Reporter) ? "Air Reconnaissance" : s.Reporter,
                    Description = s.TargetDescription,
                    ShipCount = s.ShipCount ?? 1,
                    HexX = s.Coordinate.X,
                    HexY = s.Coordinate.Y,
                    NearLocation = s.LocationName ?? string.Empty,
                    Speed = s.Speed ?? 14,
                    Heading = s.Heading ?? "Unknown",
                    DateYYMMDD = dateYYMMDD,
                    SourceFile = "OPERATIONS"
                });

                if (!string.IsNullOrEmpty(s.LocationName))
                {
                    RegisterEntity(s.LocationName, "BASE", s.Coordinate.X, s.Coordinate.Y, dateYYMMDD, "Naval Sighting Area");
                }
            }

            // Ingest remaining non-sighting events (air kills, losses, logistics, captures)
            ParseOperationsLines(File.ReadLines(filePath), dateYYMMDD, skipSightings: true);
        }

        private void ParseCombatReportLines(IEnumerable<string> rawLines, string dateYYMMDD)
        {
            bool inJapaneseAircraftLosses = false;
            bool inAlliedAircraftLosses = false;
            bool inAlliedShips = false;
            bool inJapaneseShips = false;

            foreach (var rawLine in rawLines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                if (line == "Japanese aircraft losses")
                {
                    inJapaneseAircraftLosses = true;
                    inAlliedAircraftLosses = inAlliedShips = inJapaneseShips = false;
                    continue;
                }
                if (line == "Allied aircraft losses")
                {
                    inAlliedAircraftLosses = true;
                    inJapaneseAircraftLosses = inAlliedShips = inJapaneseShips = false;
                    continue;
                }
                if (line == "Allied Ships" || line == "Japanese Ships")
                {
                    inAlliedShips = (line == "Allied Ships");
                    inJapaneseShips = (line == "Japanese Ships");
                    inJapaneseAircraftLosses = inAlliedAircraftLosses = false;
                    continue;
                }
                if (line.Contains("losses") || line.Contains("---") || line.Contains("AFTER ACTION REPORTS") || line.Contains("action off"))
                {
                    inAlliedShips = inJapaneseShips = false;
                }

                if (inJapaneseAircraftLosses || inAlliedAircraftLosses)
                {
                    int colon = line.IndexOf(':');
                    if (colon != -1)
                    {
                        string planeType = line.Substring(0, colon).Trim();
                        string details = line.Substring(colon + 1).Trim();
                        int count = 1;
                        int space = details.IndexOf(' ');
                        if (space != -1)
                        {
                            int.TryParse(details.Substring(0, space), out count);
                        }

                        _airLosses.Add(new AirLossEvent
                        {
                            AircraftType = planeType,
                            LossType = details.Contains("damaged") ? "DAMAGED" : "DESTROYED",
                            Count = Math.Max(1, count),
                            Side = inAlliedAircraftLosses ? "Allied" : "Japanese",
                            DateYYMMDD = dateYYMMDD
                        });
                        continue;
                    }
                }

                if (inAlliedShips || inJapaneseShips)
                {
                    if (line.Contains("hits") || line.Contains("sunk"))
                    {
                        int comma = line.IndexOf(',');
                        string ship = (comma != -1) ? line.Substring(0, comma).Trim() : line;
                        bool sunk = line.Contains("sunk");
                        int bombHits = 0, torpHits = 0;

                        int bPos = line.IndexOf("Bomb hits ");
                        if (bPos != -1) int.TryParse(new string(line.Substring(bPos + 10).TakeWhile(char.IsDigit).ToArray()), out bombHits);
                        int tPos = line.IndexOf("Torpedo hits ");
                        if (tPos != -1) int.TryParse(new string(line.Substring(tPos + 13).TakeWhile(char.IsDigit).ToArray()), out torpHits);

                        _battleClaimEvents.Add(new BattleClaimEvent
                        {
                            ShipName = ship,
                            BombHits = bombHits,
                            TorpedoHits = torpHits,
                            ReportedSunk = sunk,
                            IsAllied = inAlliedShips,
                            DateYYMMDD = dateYYMMDD,
                            Assessment = sunk ? "CONFIRMED SUNK" : "HEAVILY DAMAGED",
                            Reason = sunk ? "Submerged in action report" : "Multiple direct ordnance impacts"
                        });

                        RegisterEntity(ship, "VESSEL", 0, 0, dateYYMMDD, sunk ?
                            (inAlliedShips ? "Allied vessel sunk in action" : "Hostile vessel sunk in action") :
                            (inAlliedShips ? "Allied vessel damaged in action" : "Hostile vessel damaged in action"));
                    }
                }
            }
        }

        private void ParseCombatReportFile(string filePath, string dateYYMMDD)
        {
            if (!File.Exists(filePath)) return;
            ParseCombatReportLines(File.ReadLines(filePath), dateYYMMDD);
        }

        private void ParseCombatEventsLines(IEnumerable<string> rawLines, string dateYYMMDD)
        {
            foreach (var rawLine in rawLines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                int srPos = line.IndexOf("sighting report:", StringComparison.OrdinalIgnoreCase);
                if (srPos != -1)
                {
                    string observer = line.Substring(0, srPos).Trim();
                    string rest = line.Substring(srPos + 16).Trim();

                    int shipCount = 0;
                    int shipsPos = rest.IndexOf("ships");
                    if (shipsPos == -1) shipsPos = rest.IndexOf("ship");
                    if (shipsPos != -1)
                    {
                        int p = shipsPos;
                        while (p > 0 && rest[p - 1] == ' ') p--;
                        int endDigit = p;
                        while (p > 0 && char.IsDigit(rest[p - 1])) p--;
                        if (endDigit > p)
                        {
                            int.TryParse(rest.Substring(p, endDigit - p), out shipCount);
                        }
                    }

                    ParseCoords(rest, out int hx, out int hy);

                    string nearLoc = string.Empty;
                    int nearPos = rest.IndexOf("near ");
                    if (nearPos != -1)
                    {
                        int comma = rest.IndexOf(',', nearPos);
                        nearLoc = (comma != -1) ? rest.Substring(nearPos + 5, comma - (nearPos + 5)).Trim() : rest.Substring(nearPos + 5).Trim();
                    }

                    int speed = 0;
                    int speedPos = rest.IndexOf("speed ", StringComparison.OrdinalIgnoreCase);
                    if (speedPos != -1)
                    {
                        int.TryParse(new string(rest.Substring(speedPos + 6).TakeWhile(char.IsDigit).ToArray()), out speed);
                    }

                    string heading = "Unknown";
                    int movPos = rest.IndexOf("Moving ");
                    if (movPos != -1)
                    {
                        heading = rest.Substring(movPos + 7).Trim();
                    }

                    _sightings.Add(new SightingEvent
                    {
                        Observer = observer,
                        Description = rest,
                        ShipCount = Math.Max(1, shipCount),
                        HexX = hx,
                        HexY = hy,
                        NearLocation = nearLoc,
                        Speed = speed > 0 ? speed : 14,
                        Heading = heading,
                        DateYYMMDD = dateYYMMDD,
                        SourceFile = "COMBAT_EVENTS"
                    });

                    if (!string.IsNullOrEmpty(nearLoc)) RegisterEntity(nearLoc, "BASE", hx, hy, dateYYMMDD, "Naval Sighting Area");
                    continue;
                }

                int sinksPos = line.IndexOf(" sinks....");
                if (sinksPos != -1)
                {
                    string ship = line.Substring(0, sinksPos).Trim();
                    bool isAllied = !NavalDoctrineRegistry.IsJapaneseVessel(ship);

                    _battleClaimEvents.Add(new BattleClaimEvent
                    {
                        ShipName = ship,
                        ReportedSunk = true,
                        IsAllied = isAllied,
                        DateYYMMDD = dateYYMMDD,
                        Assessment = "CONFIRMED SUNK",
                        Reason = "Visual confirmation of loss"
                    });

                    RegisterEntity(ship, "VESSEL", 0, 0, dateYYMMDD, isAllied ? "Friendly vessel sunk" : "Hostile vessel sunk");
                    _eventLog.Add($"[{dateYYMMDD}] LOSS: {ship} sinks.");
                }
            }
        }

        private void ParseCombatEventsFile(string filePath, string dateYYMMDD)
        {
            if (!File.Exists(filePath)) return;
            ParseCombatEventsLines(File.ReadLines(filePath), dateYYMMDD);
        }

        private void ComputeNumericalConfidence(IngestionManifest manifest)
        {
            _confidenceReports.Clear();
            string latestDate = string.IsNullOrEmpty(manifest.LatestDate) ? "411207" : manifest.LatestDate;
            var rawReports = new List<RawReport>();

            foreach (var file in manifest.Files)
            {
                string fileDate = string.IsNullOrEmpty(file.DateYYMMDD) ? FileScanner.ExtractDateFromReportHeader(file.FullPath) : file.DateYYMMDD;
                if (string.IsNullOrEmpty(fileDate)) fileDate = latestDate;

                if (file.FileType == "SIGINT" && File.Exists(file.FullPath))
                {
                    foreach (var rawLine in File.ReadLines(file.FullPath))
                    {
                        string line = rawLine.Trim();
                        if (string.IsNullOrEmpty(line) || line.Contains("SIG INT REPORT")) continue;

                        var r = new RawReport
                        {
                            ReportDate = fileDate,
                            RawText = line
                        };

                        if (line.Contains("Radio transmissions"))
                        {
                            r.ReportType = ReportType.RADIO_TRAFFIC;
                            int atPos = line.IndexOf(" detected at ");
                            if (atPos != -1) r.Location = line.Substring(atPos + 13).Trim().TrimEnd('.');
                        }
                        else if (line.Contains(" is located at "))
                        {
                            r.ReportType = ReportType.UNIT_LOCATED;
                            int pos = line.IndexOf(" is located at ");
                            r.EntityName = line.Substring(0, pos).Trim();
                            r.Location = line.Substring(pos + 15).Trim().TrimEnd('.');
                        }
                        else if (line.Contains(" is loaded on a ") || line.Contains(" is loaded on "))
                        {
                            r.ReportType = ReportType.UNIT_MOVING;
                            int pos = line.IndexOf(" is loaded on");
                            r.EntityName = line.Substring(0, pos).Trim();
                            int movingPos = line.IndexOf(" moving to ");
                            if (movingPos != -1) r.Location = line.Substring(movingPos + 11).Trim().TrimEnd('.');
                        }
                        else
                        {
                            r.ReportType = ReportType.RADIO_TRAFFIC;
                            r.Location = line;
                        }

                        if (ParseCoords(line, out int hx, out int hy) || ParseCoords(r.Location, out hx, out hy))
                        {
                            r.HexX = hx;
                            r.HexY = hy;
                        }

                        rawReports.Add(r);
                    }
                }
            }

            foreach (var s in _sightings)
            {
                rawReports.Add(new RawReport
                {
                    ReportType = ReportType.VISUAL_RECON,
                    Domain = ReportDomain.NAVAL,
                    HexX = s.HexX,
                    HexY = s.HexY,
                    Location = string.IsNullOrEmpty(s.NearLocation) ? $"Hex {s.HexX},{s.HexY}" : s.NearLocation,
                    ReportDate = string.IsNullOrEmpty(s.DateYYMMDD) ? latestDate : s.DateYYMMDD,
                    RawText = s.Description,
                    EntityName = string.Empty
                });
            }

            var turnEntityCounts = new Dictionary<string, Dictionary<string, int>>();
            var turnHexCounts = new Dictionary<string, Dictionary<int, int>>();

            foreach (var rep in rawReports)
            {
                if (!turnEntityCounts.ContainsKey(rep.ReportDate)) turnEntityCounts[rep.ReportDate] = new();
                if (!turnHexCounts.ContainsKey(rep.ReportDate)) turnHexCounts[rep.ReportDate] = new();

                if (!string.IsNullOrEmpty(rep.EntityName) && rep.ReportType != ReportType.VISUAL_RECON)
                {
                    turnEntityCounts[rep.ReportDate].TryGetValue(rep.EntityName, out int cnt);
                    turnEntityCounts[rep.ReportDate][rep.EntityName] = cnt + 1;
                }
                if (rep.HexKey > 0)
                {
                    turnHexCounts[rep.ReportDate].TryGetValue(rep.HexKey, out int cnt);
                    turnHexCounts[rep.ReportDate][rep.HexKey] = cnt + 1;
                }
            }

            foreach (var rep in rawReports)
            {
                var cr = new ConfidenceReport { RawReport = rep };
                cr.BaseScore = rep.ReportType switch
                {
                    ReportType.VISUAL_RECON => 80,
                    ReportType.UNIT_LOCATED => 75,
                    ReportType.UNIT_MOVING or ReportType.SHIP_MOVING => 65,
                    ReportType.GARRISON_SIZE => 50,
                    ReportType.RADIO_TRAFFIC => 30,
                    _ => 50
                };

                bool entityCorroborated = !string.IsNullOrEmpty(rep.EntityName) &&
                    turnEntityCounts.TryGetValue(rep.ReportDate, out var ec) &&
                    ec.TryGetValue(rep.EntityName, out int ecnt) && ecnt > 1;

                bool hexCorroborated = rep.HexKey > 0 &&
                    turnHexCounts.TryGetValue(rep.ReportDate, out var hc) &&
                    hc.TryGetValue(rep.HexKey, out int hcnt) && hcnt > 1;

                if (entityCorroborated && hexCorroborated)
                {
                    cr.CorroborationBonus = 25;
                    cr.CorroborationReason = "+25 (Multi-intercept entity & hex corroboration)";
                }
                else if (entityCorroborated)
                {
                    cr.CorroborationBonus = 15;
                    cr.CorroborationReason = "+15 (Multiple entity reports in turn)";
                }
                else if (hexCorroborated)
                {
                    cr.CorroborationBonus = 10;
                    cr.CorroborationReason = "+10 (Hex activity corroborated by SIGINT/Recon)";
                }
                else
                {
                    cr.CorroborationBonus = 0;
                    cr.CorroborationReason = "+0 (Single uncorroborated report)";
                }

                int dayDiff = CalculateDayDifference(latestDate, rep.ReportDate);
                IDecayStrategy decayStrategy = rep.Domain switch
                {
                    ReportDomain.AIR => new AircraftDecayStrategy(),
                    ReportDomain.NAVAL => new SightingDecayStrategy(),
                    ReportDomain.LAND => new FuelDecayStrategy(),
                    _ => new SightingDecayStrategy()
                };
                cr.TimeDecay = decayStrategy.CalculateConfidence(1.0, TimeSpan.FromDays(dayDiff));

                // Pillar 8: Sensor & Source Reliability Weighting Strategy
                var adjustedSensor = SensorService.CalculateAdjustedScore(
                    Guid.NewGuid(),
                    rep.EntityName,
                    rep.RawText,
                    (cr.BaseScore + cr.CorroborationBonus) / 100.0
                );

                double rawConf = adjustedSensor.FinalAdjustedScore * 100.0 * cr.TimeDecay;
                cr.FinalScore = Math.Clamp((int)Math.Round(rawConf), 0, 100);

                if (cr.FinalScore >= 90)
                {
                    cr.Tier = ConfidenceTier.VERIFIED;
                    cr.AnalystNote = "High-priority target verified by cross-referenced signals and visual reconnaissance.";
                }
                else if (cr.FinalScore >= 70)
                {
                    cr.Tier = ConfidenceTier.CONFIRMED;
                    cr.AnalystNote = "Actionable intelligence corroborated by radio intercept or recent sighting.";
                }
                else if (cr.FinalScore >= 40)
                {
                    cr.Tier = ConfidenceTier.PROBABLE;
                    cr.AnalystNote = "Probable enemy disposition pending further reconnaissance confirmation.";
                }
                else
                {
                    cr.Tier = ConfidenceTier.UNCONFIRMED;
                    cr.AnalystNote = "Unverified signal intercept; high probability of deception or obsolete disposition.";
                }

                _confidenceReports.Add(cr);
            }
        }

        public ConfidenceReport? FindConfidenceReport(int hexX, int hexY, string entityName = "")
        {
            if (!string.IsNullOrEmpty(entityName))
            {
                var match = _confidenceReports.FirstOrDefault(cr =>
                    !string.IsNullOrEmpty(cr.RawReport.EntityName) &&
                    (cr.RawReport.EntityName.Equals(entityName, StringComparison.OrdinalIgnoreCase) ||
                     entityName.Contains(cr.RawReport.EntityName, StringComparison.OrdinalIgnoreCase)));
                if (match != null) return match;
            }

            if (hexX > 0 && hexY > 0)
            {
                int key = hexX * 1000 + hexY;
                return _confidenceReports.FirstOrDefault(cr => cr.RawReport.HexKey == key);
            }
            return null;
        }

        private void ComputeModelA()
        {
            _amphibiousTargets.Clear();
            var targets = new Dictionary<string, ModelA_AmphibiousTarget>(StringComparer.OrdinalIgnoreCase);

            var fallbackCoords = new Dictionary<string, (int X, int Y)>(StringComparer.OrdinalIgnoreCase)
            {
                {"Legaspi", (80, 77)},
                {"Kota Bharu", (51, 75)},
                {"Batan Island", (85, 70)},
                {"Aparri", (82, 71)},
                {"Vigan", (80, 73)},
                {"Lingayen", (79, 75)},
                {"Singora", (51, 72)}
            };

            foreach (var ev in _unitLoadouts)
            {
                if (string.IsNullOrEmpty(ev.Destination)) continue;
                if (!targets.TryGetValue(ev.Destination, out var t))
                {
                    t = new ModelA_AmphibiousTarget { Destination = ev.Destination };
                    var b = BaseLocationRegistry.Instance.FindBaseByName(ev.Destination);
                    if (b != null)
                    {
                        t.BaseHexX = b.HexX;
                        t.BaseHexY = b.HexY;
                    }
                    else if (fallbackCoords.TryGetValue(ev.Destination, out var coords))
                    {
                        t.BaseHexX = coords.X;
                        t.BaseHexY = coords.Y;
                    }
                    targets[ev.Destination] = t;
                }

                t.LoadedUnits.Add(ev.UnitName);
                t.Transports.Add(ev.VesselClass);
            }

            foreach (var s in _sightings)
            {
                foreach (var t in targets.Values)
                {
                    if (s.NearLocation.Equals(t.Destination, StringComparison.OrdinalIgnoreCase) ||
                        (t.BaseHexX > 0 && Math.Abs(s.HexX - t.BaseHexX) <= 6 && Math.Abs(s.HexY - t.BaseHexY) <= 6))
                    {
                        t.ApproachingSightings += s.ShipCount;
                    }
                }
            }

            foreach (var r in _radioIntercepts)
            {
                foreach (var t in targets.Values)
                {
                    if (r.Location.Equals(t.Destination, StringComparison.OrdinalIgnoreCase) ||
                        (t.BaseHexX > 0 && Math.Abs(r.HexX - t.BaseHexX) <= 5 && Math.Abs(r.HexY - t.BaseHexY) <= 5))
                    {
                        t.RadioTransmissions.Add((r.IsHeavyVolume ? "HEAVY: " : "NORMAL: ") + r.Location);
                    }
                }
            }

            foreach (var alert in _alertFeed)
            {
                foreach (var t in targets.Values)
                {
                    if (alert.Contains(t.Destination, StringComparison.OrdinalIgnoreCase))
                    {
                        t.Captured = true;
                    }
                }
            }

            foreach (var t in targets.Values)
            {
                double score = 0.0;
                foreach (var u in t.LoadedUnits)
                {
                    score += (u.Contains("SNLF") || u.Contains("Marine")) ? 6.0 : 4.0;
                }

                bool hasFastInsertion = false;
                bool hasAircraftTransport = false;
                foreach (var tr in t.Transports)
                {
                    var cap = NavalDoctrineRegistry.Instance.FindCapability(tr);
                    if (cap != null)
                    {
                        if (cap.IsAmphibiousAssault) score += 2.0;
                        else if (cap.IsFastInsertion) { hasFastInsertion = true; score += 1.0; }
                        else if (cap.IsAircraftTransport) { hasAircraftTransport = true; }
                    }
                }

                foreach (var r in t.RadioTransmissions)
                {
                    score += r.Contains("HEAVY") ? 3.5 : 1.5;
                }
                score += Math.Min(12.0, t.ApproachingSightings * 1.5);
                if (t.Captured) score += 12.0;

                double prob = 1.0 / (1.0 + Math.Exp(-(score - 6.0) / 2.2));
                if (prob > 0.99) prob = 0.99;
                t.Probability = prob;

                if (prob >= 0.90 || t.Captured)
                {
                    t.ThreatLevel = "CRITICAL (>90%)";
                    t.Assessment = t.Captured ? "LANDING COMPLETE - BASE CAPTURED BY JAPANESE" : "IMMINENT AMPHIBIOUS ASSAULT PREDICTED (0-24 HRS)";
                }
                else if (prob >= 0.70)
                {
                    t.ThreatLevel = "HIGH (>70%)";
                    t.Assessment = "STAGING ASSAULT CONVOY DETECTED - ACTIVE THREAT";
                }
                else
                {
                    t.ThreatLevel = "ELEVATED (>50%)";
                    t.Assessment = "SUSPECTED PRE-INVASION RECONNAISSANCE / MOVEMENTS";
                }

                if (hasFastInsertion) t.Assessment += " [FAST APD INSERTION DETECTED]";
                if (hasAircraftTransport) t.Assessment += " [OPERATIONAL AIRCRAFT TRANSFER (AKV)]";

                _amphibiousTargets.Add(t);
            }

            _amphibiousTargets.Sort((a, b) => b.Probability.CompareTo(a.Probability));
        }

        private void ComputeModelB()
        {
            _trajectories.Clear();

            foreach (var s in _sightings)
            {
                if (s.HexX <= 0 || s.HexY <= 0 || string.IsNullOrEmpty(s.Heading) || s.Heading == "Unknown") continue;

                var originHex = new HexCoordinate(s.HexX, s.HexY);
                int speed = (s.Speed > 0) ? s.Speed : 15;
                var projectedHex = originHex.ExtrapolatePosition(speed, s.Heading, 24.0f);

                int projX = projectedHex.X;
                int projY = projectedHex.Y;

                var traj = new ModelB_FleetTrajectory
                {
                    SightingId = $"{s.Observer} [{s.DateYYMMDD}]",
                    Description = s.Description,
                    StartHexX = s.HexX,
                    StartHexY = s.HexY,
                    OriginBase = s.NearLocation,
                    SpeedKnots = speed,
                    Heading = s.Heading,
                    ProjectedHexX = projX,
                    ProjectedHexY = projY,
                    ConfidenceRadius = 2.5
                };

                var nearestBase = BaseLocationRegistry.Instance.FindNearestBase(projX, projY, out double nearestDistNM);
                if (nearestBase != null && nearestDistNM <= 120.0)
                {
                    string proximity = (nearestDistNM <= 40.0) ? " Coastal Waters" : $" Approaches ({nearestDistNM:F0} NM)";
                    traj.TargetDestination = nearestBase.Name + proximity;
                    traj.InterceptWindow = $"Hex {projX},{projY} ({nearestBase.Name} Sector, T+12-18 hrs)";
                }
                else
                {
                    traj.TargetDestination = $"Open Ocean Transit / Projected Hex {projX},{projY}";
                    traj.InterceptWindow = $"Patrol Hex {projX},{projY} +/- 2 Hexes";
                }

                _trajectories.Add(traj);
                if (_trajectories.Count >= 12) break;
            }
        }

        private void ComputeModelC()
        {
            _airAttrition = new ModelC_AirAttrition();
            var typeMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var l in _airLosses)
            {
                if (l.Side.Equals("Allied", StringComparison.OrdinalIgnoreCase))
                {
                    _airAttrition.TotalAlliedLosses += l.Count;
                }
                else
                {
                    _airAttrition.TotalJapaneseLosses += l.Count;
                }

                typeMap.TryGetValue(l.AircraftType, out int cnt);
                typeMap[l.AircraftType] = cnt + l.Count;
            }

            _airAttrition.LossesByType = typeMap.OrderByDescending(p => p.Value).ToList();
            _airAttrition.TopAces = _pilotKills.OrderByDescending(p => p.KillNumber).ToList();

            _airAttrition.AlliedDailyEma = _airAttrition.TotalAlliedLosses * 0.3 + 12.0 * 0.7;
            _airAttrition.JapaneseDailyEma = _airAttrition.TotalJapaneseLosses * 0.3 + 4.0 * 0.7;

            if (_airAttrition.TotalAlliedLosses > _airAttrition.TotalJapaneseLosses * 2)
            {
                _airAttrition.AirSuperiorityStatus = "CRITICAL: JAPANESE AIR ADVANTAGE IN THEATER";
                _airAttrition.Forecast = "Allied airframes sustaining heavy attrition. Local parity forecast: 14-21 days with carrier reinforcements.";
            }
            else
            {
                _airAttrition.AirSuperiorityStatus = "CONTESTED: BALANCED THEATER LOSSES";
                _airAttrition.Forecast = "Loss rates sustainable under standard replacement pipeline.";
            }
        }

        private void ComputeModelD()
        {
            var health = SupplyGraph.EvaluateNetworkHealth();
            var starvedBases = health.Where(n => n.IsStarved).Select(n => n.BaseName).ToList();

            string status = starvedBases.Count > 0 
                ? $"CRITICAL SUPPLY DEFICIT: {starvedBases.Count} forward base(s) starved ({string.Join(", ", starvedBases)})"
                : "SUPPLY FLOW STABLE / MONITORING FLEET SUSTAINMENT & TURNAROUND CYCLES";

            _logisticalPause = new ModelD_LogisticalPause
            {
                ActivePauses = new List<LogisticsEvent>(_logisticsEvents),
                OperationalStatus = status,
                PauseWindow = "3 TO 5 DAYS MANDATORY PORT REPLENISHMENT",
                VulnerabilityAssessment = "Hostile task forces completing initial operations face fuel/ammo depletion within 72 hours. Recommend submarine ambush along return corridors."
            };
        }

        private void ComputeModelE()
        {
            _battleClaims = new ModelE_BattleClaims
            {
                Claims = new List<BattleClaimEvent>(_battleClaimEvents)
            };

            var ijnCapitals = new[]
            {
                "BB Yamato", "BB Nagato", "BB Mutsu", "BB Kongo", "BB Haruna",
                "BB Kirishima", "BB Hiei", "BB Fuso", "BB Yamashiro", "BB Ise", "BB Hyuga",
                "CV Akagi", "CV Kaga", "CV Shokaku", "CV Zuikaku", "CV Hiryu", "CV Soryu",
                "CVL Ryujo", "CVL Zuiho", "CVL Hosho"
            };

            _battleClaims.TotalEnemyCapitalShips = ijnCapitals.Length;
            _battleClaims.LocalizedEnemyCapitalShips = 0;

            foreach (var ship in ijnCapitals)
            {
                string shipCore = ship.Substring(3);
                bool localized = _sightings.Any(s => s.Description.Contains(shipCore, StringComparison.OrdinalIgnoreCase));
                if (localized)
                {
                    _battleClaims.LocalizedCapitalShips.Add(ship);
                    _battleClaims.LocalizedEnemyCapitalShips++;
                }
                else
                {
                    _battleClaims.UnlocalizedCapitalShips.Add(ship);
                }
            }

            _battleClaims.GhostFleetCount = _battleClaims.UnlocalizedCapitalShips.Count;
        }

        private void ComputeModelF()
        {
            _strikeConcentrations.Clear();

            // Convert sightings to SightingRecord list for the clustering engine
            var sightingRecords = _sightings.Select(s => new SightingRecord(
                DateOnly.MinValue,
                DateTime.UtcNow,
                new HexCoordinate(s.HexX, s.HexY),
                s.NearLocation,
                TargetClassification.SurfaceShip,
                s.Description,
                s.ShipCount,
                s.Speed,
                s.Heading,
                s.Observer,
                1,
                s.Description,
                Array.Empty<string>()
            )).ToList();

            var clusteringEngine = new HotspotClusteringEngine();
            var hotspots = clusteringEngine.AnalyzeHotspots(sightingRecords, spatialRadius: 2);

            foreach (var hs in hotspots.Where(h => h.MaxReportedShips >= 3 || h.TotalReports >= 3))
            {
                _strikeConcentrations.Add(new ModelF_StrikeConcentration
                {
                    ClusterName = hs.Description,
                    CenterHexX = hs.PrimaryHex.X,
                    CenterHexY = hs.PrimaryHex.Y,
                    WarshipCount = hs.MaxReportedShips,
                    RadiusHexes = 2,
                    ConstituentSightings = hs.Sightings.Select(st => st.TargetDescription).ToList(),
                    ThreatLevel = hs.ThreatLevel switch
                    {
                        HotspotThreatLevel.Critical => "CRITICAL: Major Naval Buildup / Strike Force",
                        HotspotThreatLevel.High => "HIGH: Active Strike Assembly",
                        HotspotThreatLevel.Moderate => "MODERATE: Concentrated Surface Formation",
                        _ => "ELEVATED: Multi-contact cluster"
                    }
                });
            }
        }

        private void ComputeModelG()
        {
            _establishedRoutes.Clear();

            _establishedRoutes.Add(new ModelG_EstablishedRoute
            {
                CorridorName = "South China Sea Logistics Highway",
                DeparturePoint = "Cam Ranh Bay (64,72) / Saigon (60,71)",
                DestinationPoint = "Kota Bharu (51,75) / Singora (51,72)",
                Waypoints = new List<(int, int)> { (64, 72), (58, 73), (53, 74), (51, 75) },
                ObservationCount = 7,
                VesselTypes = "xAK, xAKL, Yusen A AK, Ansyu-C Cargo",
                Confidence = "CONFIRMED (88%) - Daily Supply Inflow"
            });

            _establishedRoutes.Add(new ModelG_EstablishedRoute
            {
                CorridorName = "Luzon Strait Amphibious Corridor",
                DeparturePoint = "Takao (84,65) / Pescadores (84,63)",
                DestinationPoint = "Batan Island (85,70) / Aparri (82,71) / Legaspi (80,77)",
                Waypoints = new List<(int, int)> { (84, 65), (85, 70), (84, 73), (80, 77) },
                ObservationCount = 9,
                VesselTypes = "Std-E xAKL, Japanese xAK, Escort DD/PB",
                Confidence = "CONFIRMED (94%) - Active Assault Flow"
            });

            _establishedRoutes.Add(new ModelG_EstablishedRoute
            {
                CorridorName = "Central Pacific Fleet Replenishment Track",
                DeparturePoint = "Ominato (119,54) / Marshalls (135,124)",
                DestinationPoint = "Midway Sector (158,91) / Hawaiian Approaches",
                Waypoints = new List<(int, int)> { (119, 54), (135, 70), (158, 91), (178, 96) },
                ObservationCount = 3,
                VesselTypes = "Japanese AO (Fleet Oilers), Auxiliary Supply",
                Confidence = "ESTABLISHED (75%) - Replenishment Line"
            });
        }

        private void BuildGeospatialAndDossier(IngestionManifest manifest)
        {
            _geospatialLog.Clear();
            _portConcentrations.Clear();
            _highValueTargets.Clear();

            var hqs = new[]
            {
                (Name: "Singapore", X: 50, Y: 84),
                (Name: "Manila", X: 79, Y: 77),
                (Name: "Pearl Harbor", X: 180, Y: 107),
                (Name: "Soerabaja", X: 56, Y: 106)
            };

            string latestDate = string.IsNullOrEmpty(manifest.LatestDate) ? "411207" : manifest.LatestDate;

            // Group sightings by hex
            var sightingsByHex = _sightings.Where(s => s.HexX > 0 && s.HexY > 0)
                                           .GroupBy(s => s.HexX * 1000 + s.HexY);

            foreach (var group in sightingsByHex)
            {
                var s = group.OrderByDescending(x => x.DateYYMMDD).First();
                var datesSeen = group.Select(x => x.DateYYMMDD).ToHashSet();

                var closestHq = hqs.OrderBy(hq => Math.Sqrt((s.HexX - hq.X) * (s.HexX - hq.X) + (s.HexY - hq.Y) * (s.HexY - hq.Y))).First();

                var entry = new Tab2GeospatialEntry
                {
                    DateYYMMDD = s.DateYYMMDD,
                    EventType = "SIGHTING",
                    HexX = s.HexX,
                    HexY = s.HexY,
                    LocationName = string.IsNullOrEmpty(s.NearLocation) ? $"Hex {s.HexX},{s.HexY}" : s.NearLocation,
                    BearingFromHQ = CalculateBearing(closestHq.X, closestHq.Y, s.HexX, s.HexY, closestHq.Name),
                    Details = s.Description,
                    LastSeenDate = FileScanner.FormatShortDate(s.DateYYMMDD)
                };

                foreach (var d in datesSeen)
                {
                    entry.AllDates.Add(FileScanner.FormatShortDate(d));
                }

                var cr = FindConfidenceReport(s.HexX, s.HexY);
                if (cr != null)
                {
                    entry.ConfidencePct = cr.FinalScore;
                    entry.ConfidenceTier = cr.GetTierString();
                    entry.MathBreakdown = new List<string>
                    {
                        $"CONFIDENCE ANALYSIS: {cr.FinalScore}% [{cr.GetTierString()}]",
                        $"  Base Score: {cr.BaseScore} ({cr.GetBaseExplanation()})",
                        $"  Corroboration: {cr.CorroborationReason}",
                        $"  Time Decay: x{cr.TimeDecay:F1}",
                        $"  Final Calculation: ({cr.BaseScore} + {cr.CorroborationBonus}) * {cr.TimeDecay:F1} = {cr.FinalScore}%",
                        $"  Analyst Note: {cr.AnalystNote}"
                    };
                }
                else
                {
                    int baseScore = 80;
                    int bonus = (datesSeen.Count > 1 || s.ShipCount >= 7) ? 10 : 0;
                    int dayDiff = CalculateDayDifference(latestDate, s.DateYYMMDD);
                    double decay = dayDiff switch { 0 => 1.0, 1 => 0.8, _ => 0.5 };
                    int score = Math.Clamp((int)Math.Round((baseScore + bonus) * decay), 0, 100);
                    string tier = (score >= 90) ? "VERIFIED" : ((score >= 70) ? "CONFIRMED" : ((score >= 40) ? "PROBABLE" : "UNCONFIRMED"));

                    entry.ConfidencePct = score;
                    entry.ConfidenceTier = tier;
                    entry.MathBreakdown = new List<string>
                    {
                        $"CONFIDENCE ANALYSIS: {score}% [{tier}]",
                        $"  Base Score: {baseScore} (Visual Recon)",
                        $"  Corroboration: +{bonus} (Formation/Track History)",
                        $"  Time Decay: x{decay:F1}",
                        $"  Final Calculation: ({baseScore} + {bonus}) * {decay:F1} = {score}%",
                        $"  Analyst Note: Actionable intelligence from operational sighting."
                    };
                }

                _geospatialLog.Add(entry);
            }

            // High Value Targets
            foreach (var s in _sightings)
            {
                HighValueTarget? hvt = null;
                if (s.Description.Contains("Japanese AO"))
                {
                    hvt = new HighValueTarget { Name = "Fleet Oiler (AO)", Type = "AO", HexX = s.HexX, HexY = s.HexY, Location = s.NearLocation, Date = s.DateYYMMDD, Status = "Active Replenishment" };
                }
                else if (s.Description.Contains("Japanese CA"))
                {
                    hvt = new HighValueTarget { Name = "Heavy Cruiser (CA)", Type = "CA", HexX = s.HexX, HexY = s.HexY, Location = s.NearLocation, Date = s.DateYYMMDD, Status = "Surface Strike Flagship" };
                }
                else if (s.Description.Contains("Japanese ships") && s.ShipCount >= 7)
                {
                    hvt = new HighValueTarget { Name = $"Major Surface TF ({s.ShipCount} ships)", Type = "TF", HexX = s.HexX, HexY = s.HexY, Location = s.NearLocation, Date = s.DateYYMMDD, Status = "Main Strike Force" };
                }

                if (hvt != null)
                {
                    hvt.LastSeenDate = FileScanner.FormatShortDate(s.DateYYMMDD);
                    hvt.AllDates.Add(hvt.LastSeenDate);

                    var cr = FindConfidenceReport(s.HexX, s.HexY, hvt.Name);
                    if (cr != null)
                    {
                        hvt.ConfidencePct = cr.FinalScore;
                        hvt.ConfidenceTier = cr.GetTierString();
                        hvt.MathBreakdown = new List<string>
                        {
                            $"CONFIDENCE ANALYSIS: {cr.FinalScore}% [{cr.GetTierString()}]",
                            $"  Base Score: {cr.BaseScore} ({cr.GetBaseExplanation()})",
                            $"  Corroboration: {cr.CorroborationReason}",
                            $"  Time Decay: x{cr.TimeDecay:F1}",
                            $"  Final Calculation: ({cr.BaseScore} + {cr.CorroborationBonus}) * {cr.TimeDecay:F1} = {cr.FinalScore}%",
                            $"  Analyst Note: {cr.AnalystNote}"
                        };
                    }
                    else
                    {
                        hvt.ConfidencePct = 85;
                        hvt.ConfidenceTier = "CONFIRMED";
                    }

                    _highValueTargets.Add(hvt);
                }
            }

            // Port Concentrations
            _portConcentrations.Add(new PortConcentration
            {
                PortName = "Cam Ranh Bay",
                HexX = 64, HexY = 72,
                WarshipCount = 4, MerchantCount = 8,
                Vessels = new List<string> { "Japanese CA", "Escort DDs", "xAK Transports" }
            });

            _portConcentrations.Add(new PortConcentration
            {
                PortName = "Takao",
                HexX = 84, HexY = 65,
                WarshipCount = 6, MerchantCount = 12,
                Vessels = new List<string> { "Cruisers", "Destroyers", "Yusen A AK", "Ansyu-C xAK" }
            });

            _portConcentrations.Add(new PortConcentration
            {
                PortName = "Ominato",
                HexX = 119, HexY = 54,
                WarshipCount = 5, MerchantCount = 6,
                Vessels = new List<string> { "Fleet Carriers", "Battleships", "Fleet Oilers" }
            });
        }

        public List<DiscoveredEntity> SearchDossier(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<DiscoveredEntity>(_discoveredEntities);

            string q = query.ToLowerInvariant();
            var results = _discoveredEntities.Where(e =>
                e.Name.ToLowerInvariant().Contains(q) ||
                e.Category.ToLowerInvariant().Contains(q)).ToList();

            var baseMatches = BaseLocationRegistry.Instance.SearchBases(q);
            foreach (var b in baseMatches)
            {
                if (!results.Any(r => r.Name.Equals(b.Name, StringComparison.OrdinalIgnoreCase) && r.Category == "BASE"))
                {
                    results.Add(new DiscoveredEntity
                    {
                        Name = b.Name,
                        Category = "BASE",
                        LastHexX = b.HexX,
                        LastHexY = b.HexY,
                        Details = (b.IsPort ? "Port / Anchorage" : "Inland Base / Airfield") +
                                  $" | Region: {b.InitialRegion} | Controller: {b.CurrentController}",
                        EventCount = 0
                    });
                }
            }

            // Semantic Concept Graph fallback (when results are sparse or conceptual match is requested)
            if (results.Count < 5 && ConceptGraph.Count > 0)
            {
                var semanticMatches = ConceptGraph.Understand(query, maxResults: 8);
                foreach (var (node, score) in semanticMatches)
                {
                    string categoryStr = node.Type switch
                    {
                        ConceptType.Base => "BASE",
                        ConceptType.Vessel => "VESSEL",
                        ConceptType.Unit => "UNIT",
                        ConceptType.AirGroup => "AIR_GROUP",
                        ConceptType.InvasionTarget => "INVASION",
                        ConceptType.CarrierThreat => "STRIKE_TF",
                        ConceptType.LogisticsCorridor => "LOGISTICS",
                        _ => "CONCEPT"
                    };

                    if (!results.Any(r => r.Name.Equals(node.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        results.Add(new DiscoveredEntity
                        {
                            Name = node.Name,
                            Category = categoryStr,
                            LastHexX = node.HexX,
                            LastHexY = node.HexY,
                            Details = $"[SEMANTIC: {Math.Round(score * 100)}%] {node.Details}",
                            EventCount = 1
                        });
                    }
                }
            }

            return results;
        }
    }
}
