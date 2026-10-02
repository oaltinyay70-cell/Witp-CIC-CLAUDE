using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using WitpCic.Core.Analytics.AirDenial;
using WitpCic.Core.Analytics.Bayesian;
using WitpCic.Core.Analytics.Clustering;
using WitpCic.Core.Analytics.Continuity;
using WitpCic.Core.Analytics.EntityResolution;
using WitpCic.Core.Analytics.Events;
using WitpCic.Core.Analytics.Logistics;
using WitpCic.Core.Analytics.Prediction;
using WitpCic.Core.Analytics.Rules;
using WitpCic.Core.Analytics.Sensors;
using WitpCic.Core.Analytics.TemporalDecay;
using WitpCic.Core.Geospatial;
using WitpCic.Core.Ingestion;
using WitpCic.Core.State;
using WitpCic.Core.State.Hazards;

namespace ROCHEFORT6000.Tests;

public static class IntelEngineTests
{
    public static async Task RunAllTestsAsync()
    {
        Console.WriteLine("[TEST SUITE] Starting ROCHEFORT 6000 Strategic Intel Engine Master Tests (15 Pillars)...");

        // Pillars 1 - 7
        TestGeospatialMath();
        TestStreamingParserAndDeduplication();
        TestTemporalDecayEngine();
        TestClusteringAndHotspotDetection();
        TestEntityResolutionAndStateMachine();
        await TestDomainEventBusAsync();
        await TestWorldStateAndSnapshotAsync();
        await TestStrategicRulesEngineAsync();

        // Pillars 8 - 14
        TestSensorReliability();
        TestLogisticsNetworkGraph();
        TestAirDenialHeatmap();
        TestBayesianCorroboration();
        TestPredictiveMomentumModeling();
        TestTemporalGapContinuity();
        TestHazardStateTracking();

        Console.WriteLine("[TEST SUITE] All 15 Foundational Architectural Pillars PASSED successfully!");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"[TEST FAILURE] {message}");
        }
    }

    public static void TestGeospatialMath()
    {
        Console.Write("Testing Geospatial Math & Kinematics... ");
        var origin = new HexCoordinate(50, 84); // Singapore
        var mersing = new HexCoordinate(51, 82); // Mersing

        int distance = origin.GetDistanceTo(mersing);
        Assert(distance > 0, "Distance between Singapore and Mersing must be positive.");

        var neighbors = mersing.GetAdjacentHexes().ToList();
        Assert(neighbors.Count == 6, "A hex must have exactly 6 neighbors.");

        var startHex = new HexCoordinate(81, 87);
        var predictedHex = startHex.ExtrapolatePosition(10, "West", 4.0f, nauticalMilesPerHex: 40.0f);
        Assert(predictedHex.X == 80 && predictedHex.Y == 87, $"Predicted hex should be (80,87), got {predictedHex}.");
        Console.WriteLine("PASSED");
    }

    public static void TestStreamingParserAndDeduplication()
    {
        Console.Write("Testing Streaming Parser & Corroborating Deduplicator... ");
        string tempLogFile = Path.Combine(Path.GetTempPath(), $"op_test_{Guid.NewGuid():N}.txt");
        try
        {
            var logLines = new List<string>
            {
                "OPERATIONAL REPORT FOR Dec 12, 41",
                "",
                "Sighting report: 10 Japanese ships at 51,82 near Mersing , Speed unknown",
                "Sighting report: 10 Japanese ships at 51,82 near Mersing , Speed unknown",
                "Sighting report: 10 Japanese ships at 51,82 near Mersing , Speed unknown",
                "SBD-2 Dauntless sighting report: 10 Japanese ships at 51,82 near Mersing, speed 14, Moving South",
                "Japanese Aircraft sighted over Clark Field",
                "Japanese Aircraft sighted over Clark Field",
                "5/14th Punjab Battalion destroyed near Georgetown !!!"
            };

            File.WriteAllLines(tempLogFile, logLines);
            var parsedSightings = OperationsReportParser.ParseAndDeduplicate(tempLogFile).ToList();

            var mersingFleet = parsedSightings.FirstOrDefault(s => s.Coordinate == new HexCoordinate(51, 82));
            Assert(mersingFleet != null, "Mersing fleet sighting must be found.");
            Assert(mersingFleet!.CorroborationCount == 4, $"Corroboration count should be 4, got {mersingFleet.CorroborationCount}.");
            Assert(mersingFleet.Speed == 14, $"Speed should be updated to 14, got {mersingFleet.Speed}.");
            Assert(mersingFleet.Heading == "South", $"Heading should be 'South', got {mersingFleet.Heading}.");

            Console.WriteLine("PASSED");
        }
        finally
        {
            if (File.Exists(tempLogFile)) File.Delete(tempLogFile);
        }
    }

    public static void TestTemporalDecayEngine()
    {
        Console.Write("Testing Temporal Decay Engine... ");
        var surfaceStrategy = new SightingDecayStrategy();
        double initialConfidence = 1.0;

        double score4h = surfaceStrategy.CalculateConfidence(initialConfidence, TimeSpan.FromHours(4));
        Assert(Math.Abs(score4h - 0.5) < 0.01, $"Confidence after 4h should be 0.5, got {score4h}.");

        var relocation = new RelocationDecayStrategy();
        double relocatedScore = relocation.CalculateConfidence(initialConfidence, TimeSpan.FromMinutes(1));
        Assert(relocatedScore == 0.0, "Relocation must step-decay immediately to 0.0.");
        Console.WriteLine("PASSED");
    }

    public static void TestClusteringAndHotspotDetection()
    {
        Console.Write("Testing Spatial-Temporal Clustering & Anomaly Hotspots... ");
        var sightings = new List<SightingRecord>
        {
            new(new DateOnly(1941, 12, 12), DateTime.UtcNow, new HexCoordinate(51, 82), "Mersing", TargetClassification.SurfaceShip, "10 Japanese ships", 10, 14, "South", "Patrol", 10, "raw", Array.Empty<string>()),
            new(new DateOnly(1941, 12, 12), DateTime.UtcNow, new HexCoordinate(51, 83), "Mersing South", TargetClassification.SurfaceShip, "4 Japanese ships", 4, 12, "South", "SBD", 2, "raw", Array.Empty<string>())
        };

        var clustering = new HotspotClusteringEngine();
        var hotspots = clustering.AnalyzeHotspots(sightings, spatialRadius: 1);
        Assert(hotspots.Count >= 1, "Should cluster into naval assembly hotspot.");
        Console.WriteLine("PASSED");
    }

    public static void TestEntityResolutionAndStateMachine()
    {
        Console.Write("Testing Entity Resolution & State Machine... ");
        var resolver = new EntityResolutionService();
        var id1 = resolver.ResolveOrCreate("xAKL Legazpi (TF 323)");
        var id2 = resolver.ResolveOrCreate("Legazpi");
        Assert(id1 == id2, "xAKL Legazpi and Legazpi must resolve to identical Guid.");

        var stateMachine = new EntityStateMachine();
        var now = DateTime.UtcNow;
        stateMachine.Fire(UnitLifecycleTrigger.ReportDamaged, now);
        stateMachine.Fire(UnitLifecycleTrigger.PumpFailure, now.AddHours(2));
        Assert(stateMachine.CalculatePredictedLossProbability() >= 0.80, "Loss probability should be >= 80% on pump failure.");
        Console.WriteLine("PASSED");
    }

    public static async Task TestDomainEventBusAsync()
    {
        Console.Write("Testing In-Process Mediator Domain Event Bus... ");
        var bus = new DomainEventBus();
        bool handled = false;

        bus.Subscribe<PumpFailureEvent>(evt =>
        {
            if (evt.EntityName == "xAKL Legazpi") handled = true;
            return Task.CompletedTask;
        });

        await bus.PublishAsync(new PumpFailureEvent(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), "xAKL Legazpi"));
        Assert(handled, "PumpFailureEvent must be processed by subscriber.");
        Console.WriteLine("PASSED");
    }

    public static async Task TestWorldStateAndSnapshotAsync()
    {
        Console.Write("Testing World State Repository & JSON Snapshotting... ");
        var repo = new WorldStateRepository();
        var unitId = Guid.NewGuid();
        var unit = repo.GetOrAddUnit(unitId, "CA New Orleans", EntityType.SurfaceShip, new HexCoordinate(180, 107));
        unit.FuelLevel = 100;

        var snapshot = repo.CreateSnapshot(new DateOnly(1941, 12, 12));
        using var ms = new MemoryStream();
        await repo.SaveSnapshotAsync(ms, snapshot);
        Assert(ms.Length > 0, "Snapshot JSON stream must not be empty.");
        Console.WriteLine("PASSED");
    }

    public static async Task TestStrategicRulesEngineAsync()
    {
        Console.Write("Testing Actionable Strategic Rules Engine... ");
        var repo = new WorldStateRepository();
        repo.UpdateBaseFortification("Singapore", 1);
        for (int i = 0; i < 6; i++)
        {
            var u = repo.GetOrAddUnit(Guid.NewGuid(), $"TF 30{i}", EntityType.TaskForce, new HexCoordinate(50, 80 + i));
            u.FuelLevel = 10;
        }

        var snapshot = repo.CreateSnapshot(new DateOnly(1941, 12, 12));
        var rulesEngine = new StrategicRulesEngine();
        var alerts = await rulesEngine.EvaluateAllAsync(snapshot);
        Assert(alerts.Any(a => a.Severity == AlertSeverity.Warning), "Fuel deficit warning must be triggered.");
        Console.WriteLine("PASSED");
    }

    public static void TestSensorReliability()
    {
        Console.Write("Testing Sensor & Source Reliability Weighting... ");
        var sensorService = new SensorReliabilityService();

        var sbdType = sensorService.DetectSensorType("SBD-2 Dauntless", "Sighting report: 10 ships");
        Assert(sbdType == SensorType.VisualTrainedAir, "SBD must detect as VisualTrainedAir.");

        var selfReportType = sensorService.DetectSensorType("SS K XII", "SS K XII reports having been sighted by the enemy");
        Assert(selfReportType == SensorType.SelfReportedContact, "Self-sighting must detect as SelfReportedContact.");

        var sbdScore = sensorService.CalculateAdjustedScore(Guid.NewGuid(), "SBD-2 Dauntless", "Sighting report: 10 ships", 0.90);
        Assert(sbdScore.FinalAdjustedScore == 0.90, "Visual air should retain 1.0 multiplier.");

        var selfScore = sensorService.CalculateAdjustedScore(Guid.NewGuid(), "SS K XII", "reports having been sighted", 0.90);
        Assert(Math.Abs(selfScore.FinalAdjustedScore - (0.90 * 0.55)) < 0.01, "Self reported contact should receive 0.55 multiplier.");
        Console.WriteLine("PASSED");
    }

    public static void TestLogisticsNetworkGraph()
    {
        Console.Write("Testing Logistical Network Flow & Supply Graph... ");
        var graph = new SupplyNetworkGraph();
        var rabaul = new HexCoordinate(106, 125);
        var portMoresby = new HexCoordinate(98, 130);

        graph.AddOrUpdateRoute("Rabaul", rabaul, "Port Moresby", portMoresby, TransportVesselType.StandardCargo_xAK, 3000);
        var healthBefore = graph.EvaluateNetworkHealth();
        var moresbyBefore = healthBefore.First(n => n.BaseName == "Port Moresby");
        Assert(!moresbyBefore.IsStarved && moresbyBefore.InboundCapacityTons == 3000, "Port Moresby should be supplied.");

        graph.MarkNodeSevered(rabaul);
        var healthAfter = graph.EvaluateNetworkHealth();
        var moresbyAfter = healthAfter.First(n => n.BaseName == "Port Moresby");
        Assert(moresbyAfter.IsStarved && moresbyAfter.InboundCapacityTons == 0, "Port Moresby should be flagged as Starved after origin severed.");
        Console.WriteLine("PASSED");
    }

    public static void TestAirDenialHeatmap()
    {
        Console.Write("Testing Air Superiority & Reconnaissance Denial Mapping... ");
        var airService = new AirDenialHeatmapService();
        var clark = new HexCoordinate(79, 76);
        var now = DateTime.UtcNow;

        for (int i = 0; i < 3; i++)
        {
            airService.RecordOverflight(new AirOverflightEvent(clark, AircraftThreatClass.TorpedoOrDiveBomber, "Kate / Val strike", now));
        }

        float score = airService.GetDenialScore(clark, now);
        Assert(score >= 0.70f, $"Air denial score should be >= 0.70, got {score}.");
        Assert(airService.IsAreaDenied(clark, now), "Clark Field should be marked as AIR_DENIED.");
        Console.WriteLine("PASSED");
    }

    public static void TestBayesianCorroboration()
    {
        Console.Write("Testing Bayesian Multi-Source Corroboration... ");
        var resolver = new EntityResolutionService();
        var bayes = new BayesianCorroborationEngine(resolver);

        var sightings = new List<SightingRecord>
        {
            new(DateOnly.MinValue, DateTime.UtcNow, new HexCoordinate(53, 80), "Kuantan", TargetClassification.Submarine, "Sub sighting", 1, null, null, "SS K XIV", 1, "raw", Array.Empty<string>(), InitialConfidenceScore: 0.70) { CurrentConfidenceScore = 0.70 },
            new(DateOnly.MinValue, DateTime.UtcNow, new HexCoordinate(52, 80), "Kuantan", TargetClassification.Submarine, "Sub sighting", 1, null, null, "SS K XV", 1, "raw", Array.Empty<string>(), InitialConfidenceScore: 0.70) { CurrentConfidenceScore = 0.70 }
        };

        var result = bayes.CombineProbabilities("KUANTAN_ASW", sightings);
        // 1 - (1-0.70)*(1-0.70) = 1 - 0.09 = 0.91
        Assert(Math.Abs(result.CombinedProbability - 0.91) < 0.02, $"Combined probability should be ~0.91, got {result.CombinedProbability}.");
        Assert(result.IndependentSourceCount == 2, "Both submarines must count as independent observers.");
        Console.WriteLine("PASSED");
    }

    public static void TestPredictiveMomentumModeling()
    {
        Console.Write("Testing Predictive Threat & Advance Momentum Modeling... ");
        var predictor = new AdvanceMomentumPredictor();
        predictor.RegisterTargetBase("Cagayan", new HexCoordinate(81, 89), strategicTier: 3);
        predictor.RegisterTargetBase("Davao", new HexCoordinate(80, 93), strategicTier: 2);

        // Advancing South-East from Dinagat (81,87)
        var startHex = new HexCoordinate(81, 87);
        var momentum = new Vector2(0, 1); // South

        var predictions = predictor.PredictNextTargets(startHex, momentum, topK: 3).ToList();
        Assert(predictions.Count > 0, "Should generate predicted targets along advance corridor.");
        Assert(predictions[0].TargetBaseName == "Cagayan", $"Top target should be Cagayan, got {predictions[0].TargetBaseName}.");
        Console.WriteLine("PASSED");
    }

    public static void TestTemporalGapContinuity()
    {
        Console.Write("Testing Cross-File Gap Detection & Temporal Continuity... ");
        var checker = new TemporalContinuityChecker();
        checker.RegisterTurn(new DateOnly(1941, 12, 8), "aoperationsreport_411208.txt");
        checker.RegisterTurn(new DateOnly(1941, 12, 10), "aoperationsreport_411210.txt"); // Dec 9 missing!

        var missing = checker.DetectGaps();
        Assert(missing.Count == 1 && missing[0] == new DateOnly(1941, 12, 9), "Dec 9 must be identified as missing gap.");

        // Interpolate position across missing Dec 9
        var interp = checker.InterpolatePosition(
            Guid.NewGuid(),
            "TF 325",
            new DateOnly(1941, 12, 8),
            new HexCoordinate(50, 70),
            new DateOnly(1941, 12, 10),
            new HexCoordinate(50, 80),
            new DateOnly(1941, 12, 9)
        );

        Assert(interp.InterpolatedHex == new HexCoordinate(50, 75), $"Interpolated hex on Dec 9 should be (50,75), got {interp.InterpolatedHex}.");
        Assert(interp.Confidence == ContinuityConfidence.InterpolatedGap, "Confidence must be InterpolatedGap.");
        Console.WriteLine("PASSED");
    }

    public static void TestHazardStateTracking()
    {
        Console.Write("Testing Persistent Hazard & Minefield Layer... ");
        var repo = new HazardStateRepository();
        var mersing = new HexCoordinate(51, 82);

        var minefield = new HazardState(
            Guid.NewGuid(),
            mersing,
            "Mersing",
            HazardType.MinefieldDefensive,
            Faction.Allied,
            100,
            "TF 448",
            DateTime.UtcNow
        );

        repo.AddOrUpdateHazard(minefield);
        Assert(repo.IsHexHostileHazard(mersing, Faction.Japanese), "Allied minefield should be hostile to Japanese forces.");
        Assert(!repo.IsHexHostileHazard(mersing, Faction.Allied), "Allied minefield should not be hostile to Allied forces.");

        repo.ApplyMinesweepingClearing(mersing, 80);
        var remaining = repo.GetHazardsAt(mersing);
        Assert(remaining.Count == 1 && remaining[0].DensityPercent == 20, "Minefield density should be reduced to 20% after sweeping.");
        Console.WriteLine("PASSED");
    }
}
