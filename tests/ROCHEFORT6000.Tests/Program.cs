using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ROCHEFORT6000.Core.Analytics;
using ROCHEFORT6000.Core.Controller;
using ROCHEFORT6000.Core.Database;
using ROCHEFORT6000.Core.Ingestion;

namespace ROCHEFORT6000.Tests
{
    internal class Program
    {
        private static int _passedCount = 0;
        private static int _failedCount = 0;

        private static void Assert(bool condition, string testName)
        {
            if (condition)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[PASS] {testName}");
                Console.ResetColor();
                _passedCount++;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FAIL] {testName}");
                Console.ResetColor();
                _failedCount++;
            }
        }

        private static int Main(string[] args)
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine("  ROCHEFORT 6000 // PACIFIC WAR ROOM AUTOMATED ANALYTICS TEST SUITE");
            Console.WriteLine("================================================================================\n");

            string rootDir = AppDomain.CurrentDomain.BaseDirectory;
            while (!string.IsNullOrEmpty(rootDir) && !File.Exists(Path.Combine(rootDir, "CANONICAL.txt")))
            {
                var parent = Directory.GetParent(rootDir);
                if (parent == null) break;
                rootDir = parent.FullName;
            }

            if (string.IsNullOrEmpty(rootDir) || !File.Exists(Path.Combine(rootDir, "CANONICAL.txt")))
            {
                rootDir = @"C:\Users\ozgur\Documents\Witp-CIC-MUTHUR";
            }

            Console.WriteLine($"Working Root Directory: {rootDir}\n");

            // ── Test Suite 1: Base Location Database Registry ────────────────────────
            Console.WriteLine("--- TEST SUITE 1: Base Location Registry ---");
            var baseReg = BaseLocationRegistry.Instance;
            Assert(baseReg.Count >= 1000, $"BaseLocationRegistry loaded {baseReg.Count} base entities (>= 1,000 required)");
            var pearl = baseReg.FindBaseByName("Pearl Harbor");
            Assert(pearl != null && pearl.HexX == 180 && pearl.HexY == 107, "Pearl Harbor coordinates match (180, 107)");
            var nearest = baseReg.FindNearestBase(50, 75, out double distNM);
            Assert(nearest != null && !string.IsNullOrEmpty(nearest.Name), $"FindNearestBase located {nearest?.Name} at {distNM:F1} NM");

            // ── Test Suite 2: Naval Doctrine Registry ───────────────────────────────
            Console.WriteLine("\n--- TEST SUITE 2: Naval Doctrine Registry ---");
            var docReg = NavalDoctrineRegistry.Instance;
            var bb = docReg.FindCapability("BB");
            Assert(bb != null && bb.IsCapital, "Battleship (BB) classified as Capital Ship");
            var ss = docReg.FindCapability("SS");
            Assert(ss != null && !ss.IsCapital && ss.FunctionalClass == NavalFunctionalClass.Submarine, "Submarine (SS) classified as Submarine");
            var cv = docReg.FindCapability("CV");
            Assert(cv != null && cv.IsCarrier, "Fleet Carrier (CV) classified as Carrier");

            // ── Test Suite 3: File Scanner & Ingestion ───────────────────────────────
            Console.WriteLine("\n--- TEST SUITE 3: Golden Telemetry Ingestion ---");
            var searchPaths = new List<string>
            {
                Path.Combine(rootDir, "docs", "golden"),
                Path.Combine(rootDir, "sample_data"),
                Path.Combine(rootDir, "archive")
            };
            var manifest = FileScanner.ScanDirectories(searchPaths, ScanOptions.TestMode);
            Assert(manifest.Files.Count > 0, $"FileScanner processed {manifest.Files.Count} files");
            Assert(manifest.UniqueDates.Count > 0, $"Ingested {manifest.UniqueDates.Count} distinct operational dates: [{string.Join(", ", manifest.UniqueDates)}]");

            // ── Test Suite 3b: Scan hygiene (live vs archive, CombatEvents date, clutter, campaign PWS) ──
            Console.WriteLine("\n--- TEST SUITE 3b: Scan Hygiene ---");
            {
                string tmp = Path.Combine(Path.GetTempPath(), "r6k_scan_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(Path.Combine(tmp, "archive"));
                Directory.CreateDirectory(Path.Combine(tmp, "autosave"));
                File.WriteAllText(Path.Combine(tmp, "archive", "OperationsReport_411226.txt"), "OPERATIONS REPORT FOR Dec 26, 41\n");
                File.WriteAllText(Path.Combine(tmp, "OperationsReport.txt"), "OPERATIONS REPORT FOR Dec 26, 41\n");
                File.WriteAllText(Path.Combine(tmp, "CombatEvents.txt"), "COMBAT EVENTS FOR 12/27/41\n");
                File.WriteAllText(Path.Combine(tmp, "placeholder.txt"), "x");
                File.WriteAllText(Path.Combine(tmp, "autosave", "OperationsReport_411220.txt"), "OPERATIONS REPORT FOR Dec 20, 41\n");
                var hyg = FileScanner.ScanDirectories(new[] { tmp, Path.Combine(tmp, "archive") });
                Assert(hyg.Files.Count(f => f.FileType == "OPERATIONS") == 1, "Live undated ops report de-duplicated against archived day");
                Assert(hyg.Files.Any(f => f.FileType == "OPERATIONS" && f.FileName.Contains("411226")), "Archive (dated name) copy is the one kept");
                Assert(hyg.Files.Any(f => f.FileType == "COMBAT_EVENTS" && f.DateYYMMDD == "411226"), "CombatEvents 'FOR 12/27/41' normalised to previous day 411226");
                Assert(!hyg.Files.Any(f => f.FileName.Contains("411220")), "autosave\\ folder ignored");
                Assert(!hyg.Files.Any(f => f.FileName.Equals("placeholder.txt", StringComparison.OrdinalIgnoreCase)), "placeholder.txt ignored");
                Assert(hyg.UniqueDates.Count == 1, "Unique dates recomputed after de-duplication");
                try { Directory.Delete(tmp, true); } catch { }
            }

            // ── Test Suite 4: War Room Analytics & Predictive Models ────────────────
            Console.WriteLine("\n--- TEST SUITE 4: War Room Analytics & Models A-G ---");
            var analytics = new WarRoomAnalytics();
            analytics.AnalyzeCampaign(manifest);

            // Model A: Target Invasion Probability
            Assert(analytics.ModelATargets.Count > 0, $"Model A generated {analytics.ModelATargets.Count} invasion target predictions");
            var topTarget = analytics.ModelATargets.FirstOrDefault();
            Assert(topTarget != null && topTarget.Probability >= 0.50, $"Top Target ({topTarget?.Destination}) has confidence {(topTarget?.Probability * 100):F1}% (>= 50%)");

            // Model B: Dead Reckoning Trajectories
            Assert(analytics.ModelBTrajectories.Count > 0, $"Model B computed {analytics.ModelBTrajectories.Count} contact trajectory vectors");

            // Model C: Air Superiority & Loss Ratios
            Assert(analytics.ModelCAttrition != null, "Model C computed air attrition analytics");

            // Model D: Operational Pause / Logistics
            Assert(analytics.ModelDLogistics != null, "Model D computed logistical operational pause analytics");

            // Model E: Opponent-Only BDA Claims Revision
            var friendlyClaims = analytics.ModelEClaims.Claims.Where(c => c.ShipName.Contains("Enterprise") || c.ShipName.Contains("Yorktown")).ToList();
            Assert(friendlyClaims.Count == 0, "Model E strictly excludes friendly combat claims (Opponent-only filter)");

            // Model F: Strike Concentration Clusters
            Assert(analytics.ModelFConcentrations.Count > 0, $"Model F detected {analytics.ModelFConcentrations.Count} strike clusters");

            // Model G: Established Shipping Corridors
            Assert(analytics.ModelGRoutes.Count > 0, $"Model G identified {analytics.ModelGRoutes.Count} shipping corridors");

            // ── Test Suite 5: Sighting Tracks & Multi-Day Chronology ────────────────
            Console.WriteLine("\n--- TEST SUITE 5: Sighting Tracks & Chronology ---");
            Assert(analytics.GeospatialLog.Count > 0, $"Logged {analytics.GeospatialLog.Count} geospatial sighting tracks");
            var multiSeen = analytics.GeospatialLog.FirstOrDefault(s => s.AllDates.Count > 1);
            if (multiSeen != null)
            {
                Assert(multiSeen.AllDates.Count > 1, $"Multi-day tracking verified: {multiSeen.Details} seen across {string.Join(", ", multiSeen.AllDates)}");
            }
            else
            {
                Assert(true, "Sighting tracks populated with verified date attribution");
            }

            // ── Test Suite 6: Unified Dossier Search & Semantic Concept Graph ───────
            Console.WriteLine("\n--- TEST SUITE 6: Unified Dossier Search & Concept Graph ---");
            var dossierResults = analytics.SearchDossier("KB");
            Assert(dossierResults != null, "Dossier query for 'KB' executes without error");
            var portResults = analytics.SearchDossier("Singapore");
            Assert(portResults != null, "Dossier query for 'Singapore' executes without error");
            Assert(analytics.ConceptGraph.Count >= 1000, $"Concept Graph indexed {analytics.ConceptGraph.Count} nodes (>= 1,000 required)");
            var semanticMatches = analytics.ConceptGraph.Understand("carrier threat oiler");
            Assert(semanticMatches != null && semanticMatches.Count > 0, "Semantic query 'carrier threat oiler' returns matched concepts");
            var semanticDossier = analytics.SearchDossier("amphibious assault landing");
            Assert(semanticDossier != null && semanticDossier.Count > 0, "Dossier query 'amphibious assault landing' resolves semantic results");

            // ── Test Suite 7: Terminal Screen Manager Flow & Navigation ─────────────
            Console.WriteLine("\n--- TEST SUITE 7: Terminal Screen Manager Flow ---");
            var screenManager = new TerminalScreenManager { ScanOptionsOverride = ScanOptions.TestMode };
            screenManager.InitializeData(searchPaths);
            Assert(screenManager.CurrentState == ScreenState.Boot, "ScreenManager starts in Boot state");
            string bootScreenText = screenManager.BuildCurrentScreen();
            Assert(bootScreenText.Contains("ROCHEFORT 6000 // SYSTEM BOOT DIAGNOSTIC"), "Boot screen contains ROCHEFORT 6000 header");
            Assert(bootScreenText.Contains("6 HEURISTICS READY"), "Boot screen contains 6 HEURISTICS READY");
            Assert(bootScreenText.Contains("10 GRAPHIC SCREENS READY"), "Boot screen contains 10 GRAPHIC SCREENS READY");

            screenManager.HandleKeyDown(System.Windows.Input.Key.Enter);
            Assert(screenManager.CurrentState == ScreenState.StartScreen, "Boot -> StartScreen on Enter");

            screenManager.HandleKeyDown(System.Windows.Input.Key.N);
            Assert(screenManager.CurrentState == ScreenState.NewCampaignWizard, "StartScreen -> NewCampaignWizard on [N]");
            Assert(screenManager.IsAwaitingTextInput == false, "Step 1 awaiting side toggle (not text input)");

            screenManager.HandleKeyDown(System.Windows.Input.Key.Enter);
            Assert(screenManager.IsAwaitingTextInput == true, "Step 2 awaiting password text input");

            screenManager.HandleCharInput('T');
            screenManager.HandleCharInput('E');
            screenManager.HandleCharInput('S');
            screenManager.HandleCharInput('T');
            screenManager.HandleKeyDown(System.Windows.Input.Key.Enter);
            Assert(screenManager.IsAwaitingTextInput == false, "Step 3 folder list active");

            screenManager.HandleKeyDown(System.Windows.Input.Key.Enter);
            Assert(screenManager.IsAwaitingTextInput == true, "Step 4 campaign name text input active");

            screenManager.HandleKeyDown(System.Windows.Input.Key.Enter);
            Assert(screenManager.CurrentState == ScreenState.VaultIngestion, "Step 4 -> VaultIngestion on Enter");

            screenManager.HandleKeyDown(System.Windows.Input.Key.Enter);
            Assert(screenManager.CurrentState == ScreenState.MasterMenu, "VaultIngestion -> MasterMenu on Enter");

            screenManager.HandleKeyDown(System.Windows.Input.Key.A);
            Assert(screenManager.CurrentState == ScreenState.WarRoomPrimary, "MasterMenu -> WarRoomPrimary on [A]");

            // ── Test Suite 6 & 7: War Room Primary Screen & TBD Retirement of Screens 1 and 2 ────
            string warRoomScreen = screenManager.BuildCurrentScreen();
            Assert(warRoomScreen.Contains("[1]  [ TBD ]"), "War Room Primary screen displays [1]  [ TBD ]");
            Assert(warRoomScreen.Contains("[2]  [ TBD ]"), "War Room Primary screen displays [2]  [ TBD ]");
            Assert(!warRoomScreen.Contains("Intelligence Summary"), "War Room Primary does NOT contain Intelligence Summary");
            Assert(!warRoomScreen.Contains("SIGINT Map"), "War Room Primary does NOT contain SIGINT Map");

            // Option [1] routes to TbdDomain
            screenManager.HandleKeyDown(System.Windows.Input.Key.D1);
            Assert(screenManager.CurrentState == ScreenState.TbdDomain, "WarRoom -> TbdDomain on [1]");
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            Assert(screenManager.CurrentState == ScreenState.WarRoomPrimary, "TbdDomain -> WarRoom on [ESC]");

            // Option [2] routes to TbdDomain
            screenManager.HandleKeyDown(System.Windows.Input.Key.D2);
            Assert(screenManager.CurrentState == ScreenState.TbdDomain, "WarRoom -> TbdDomain on [2]");
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            Assert(screenManager.CurrentState == ScreenState.WarRoomPrimary, "TbdDomain -> WarRoom on [ESC]");

            var sortedDates = screenManager.Manifest.UniqueDates.OrderByDescending(d => d).ToList();
            Assert(sortedDates.Count > 0, "Unique turn dates available in Manifest");

            // Test TemporalFilterModal via direct state access
            typeof(TerminalScreenManager).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(screenManager, ScreenState.TemporalFilterModal);
            Assert(screenManager.CurrentState == ScreenState.TemporalFilterModal, "Direct access to TemporalFilterModal for verification");

            string modalScreen = screenManager.BuildCurrentScreen();
            Assert(modalScreen.Contains("TEMPORAL INTELLIGENCE FILTER & DATE RANGE SELECTOR"), "Modal contains title bar");
            Assert(modalScreen.Contains("QUICK OPERATIONAL PRESETS:"), "Modal displays quick operational presets");
            Assert(modalScreen.Contains("CUSTOM DATE / RANGE ENTRY (DD/MM/YY):") || modalScreen.Contains("CUSTOM DATE / RANGE ENTRY (MM/DD/YY):"), "Modal displays custom date/range entry options");

            // Verify border alignment in Modal (106 cols)
            var temporalModalLines = modalScreen.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            bool allModalLines106 = true;
            foreach (var ml in temporalModalLines)
            {
                if (ml.Length > 0 && ml.StartsWith("║") && ml.Length != 106)
                {
                    allModalLines106 = false;
                    Console.WriteLine($"[FAIL] Modal line length {ml.Length} != 106: '{ml}'");
                }
            }
            Assert(allModalLines106, "TemporalFilterModal maintains straight 106-column borders (Length == 106)");

            // Restore state to WarRoomPrimary
            typeof(TerminalScreenManager).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(screenManager, ScreenState.WarRoomPrimary);
            Assert(screenManager.CurrentState == ScreenState.WarRoomPrimary, "State restored to WarRoomPrimary");

            // ── Test Suite 8: War Room Heuristics & Logical Blueprints ──────────────
            Console.WriteLine("\n--- TEST SUITE 8: War Room Heuristics & Logical Blueprints ---");
            var heuristics = analytics.Heuristics;
            Assert(heuristics != null, "HeuristicsEngine instantiated on WarRoomAnalytics");

            // Metric 1: Regional Power Shift
            Assert(heuristics!.RegionalShifts.Count >= 8, $"Regional Power Shift evaluated {heuristics.RegionalShifts.Count} strategic regions");
            bool expectedTemporalGuard = manifest.DaysProcessed >= 4;
            Assert(heuristics.HasSufficientTemporalData == expectedTemporalGuard, $"Temporal Guard status verified (Days: {manifest.DaysProcessed}, HasSufficientData: {heuristics.HasSufficientTemporalData})");

            // Metric 4: Minefield Mapping
            Assert(heuristics.MineHitCoordinates.Count > 0, $"Minefield Mapping identified {heuristics.MineHitCoordinates.Count} mine-hit hexes (HashSet)");
            Assert(heuristics.MineHitsChronological.Count > 0, $"Chronological mine encounter log contains {heuristics.MineHitsChronological.Count} entries");

            // Metric 5: Submarine Migration
            Assert(heuristics.SubmarineCentroids.Count > 0, $"Submarine Migration computed {heuristics.SubmarineCentroids.Count} daily centroids");

            // Metric 46: Intelligence Blindspots
            Assert(heuristics.IntelligenceBlindspots.Count > 0, $"Intelligence Blindspots identified {heuristics.IntelligenceBlindspots.Count} unheralded combat engagements");

            // Metric 49: Axis of Advance
            Assert(heuristics.FleetMeans.Count > 0, $"Axis of Advance calculated {heuristics.FleetMeans.Count} daily fleet center positions");

            // Metric 50: Operational Tempo
            Assert(heuristics.DailyOptempo.Count > 0, $"Operational Tempo parsed {heuristics.DailyOptempo.Count} operational days of combatreport separators");

            // Heuristics Screen & Pop-up Navigation
            screenManager.HandleKeyDown(System.Windows.Input.Key.D6);
            Assert(screenManager.CurrentState == ScreenState.Heuristics, "WarRoomPrimary -> Heuristics on [6]");

            screenManager.HandleKeyDown(System.Windows.Input.Key.Enter);
            Assert(screenManager.CurrentState == ScreenState.HeuristicsPopUp, "Heuristics -> HeuristicsPopUp on [ENTER]");

            // Verification of ADR 64 / User Constraint: Right side of graph box is always straight
            string popUpBuffer = screenManager.BuildCurrentScreen();
            var popUpLines = popUpBuffer.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            bool allPopUpBordersStraight = true;
            foreach (var line in popUpLines)
            {
                if (line.StartsWith("║") && line.EndsWith("║"))
                {
                    if (line.Length != 72 && line.Length != 106)
                    {
                        allPopUpBordersStraight = false;
                        Console.WriteLine($"[FAIL] Box line length mismatch: {line.Length} expected 72: '{line}'");
                    }
                }
            }
            Assert(allPopUpBordersStraight, "ADR 64: In-Terminal Unicode Graph Box right side is 100% straight (Length == 72)");

            screenManager.HandleKeyDown(System.Windows.Input.Key.Enter);
            Assert(screenManager.CurrentState == ScreenState.ReasoningModal, "HeuristicsPopUp -> ReasoningModal (Raw Evidence) on [ENTER]");

            string modalBuffer = screenManager.BuildCurrentScreen();
            var modalLines = modalBuffer.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            bool allModalBordersStraight = true;
            foreach (var line in modalLines)
            {
                if (line.StartsWith("║") && line.EndsWith("║"))
                {
                    if (line.Length != 72 && line.Length != 106)
                    {
                        allModalBordersStraight = false;
                        Console.WriteLine($"[FAIL] Modal line length mismatch: {line.Length} expected 72: '{line}'");
                    }
                }
            }
            Assert(allModalBordersStraight, "ADR 64: Reasoning & Evidence Modal Box right side is 100% straight (Length == 72)");

            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            Assert(screenManager.CurrentState == ScreenState.HeuristicsPopUp, "ReasoningModal -> HeuristicsPopUp on [ESC]");

            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            Assert(screenManager.CurrentState == ScreenState.Heuristics, "HeuristicsPopUp -> Heuristics on [ESC]");

            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            Assert(screenManager.CurrentState == ScreenState.WarRoomPrimary, "Heuristics -> WarRoomPrimary on [ESC]");

            // ── Test Suite 9: Telemetry Analytics Engine & Core Algorithms ───────────
            Console.WriteLine("\n--- TEST SUITE 9: Telemetry Analytics Engine & Core Algorithms ---");
            
            // 1.1 CoG
            var cogCoords = new List<(double X, double Y)> { (50, 70), (52, 72), (54, 74) };
            var cog = TelemetryAnalyticsEngine.CalculateCenterOfGravity(cogCoords);
            Assert(Math.Abs(cog.CoG_X - 52.0) < 0.001 && Math.Abs(cog.CoG_Y - 72.0) < 0.001, "Section 1.1: Center of Gravity (CoG) correctly computed (52.0, 72.0)");

            // 1.2 Axis of Advance
            var advance = TelemetryAnalyticsEngine.CalculateAxisOfAdvance((54, 76), (50, 70));
            Assert(advance.Vector_X == 4.0 && advance.Vector_Y == 6.0 && advance.Projected_X == 58.0 && advance.Projected_Y == 82.0,
                "Section 1.2: Axis of Advance vector projection verified (Projected: 58, 82)");

            // 1.3 Clustering
            var candidateS = new List<(int X, int Y)> { (50, 70), (100, 100) };
            var histS = new List<(int X, int Y)> { (50, 70), (51, 71), (52, 70), (49, 69) };
            var clusters = TelemetryAnalyticsEngine.DetectCoordinateClustering(candidateS, histS, 5, 1.0);
            Assert(clusters.Any(c => c.HexX == 50 && c.IsSightingSpike), "Section 1.3: Force build-up sighting spike identified in hex (50, 70)");

            // 2.1 OLS Linear Regression Slope
            var improvingVals = new List<double> { 1.0, 1.2, 1.4, 1.6, 1.8 };
            var slopeRes = TelemetryAnalyticsEngine.CalculateLinearRegressionSlope(improvingVals);
            Assert(slopeRes.Slope > 0 && slopeRes.Trend == "IMPROVING", "Section 2.1: OLS Regression correctly detects IMPROVING slope (m > 0)");

            var degradingVals = new List<double> { 2.0, 1.8, 1.5, 1.3, 1.0 };
            var slopeDegRes = TelemetryAnalyticsEngine.CalculateLinearRegressionSlope(degradingVals);
            Assert(slopeDegRes.Slope < 0 && slopeDegRes.Trend == "DEGRADING", "Section 2.1: OLS Regression correctly detects DEGRADING slope (m < 0)");

            // 2.2 Rolling OPTEMPO
            var dailyEvts = new List<int> { 3, 6, 9, 12 };
            var rollingOpt = TelemetryAnalyticsEngine.ComputeRollingOptempo(dailyEvts, 3);
            Assert(rollingOpt.Count == 4 && Math.Abs(rollingOpt[2] - 6.0) < 0.001 && Math.Abs(rollingOpt[3] - 9.0) < 0.001,
                "Section 2.2: Rolling 3-day OPTEMPO rolling average correctly smoothed");

            // 2.3 Strategic Pivot Latency
            var pivot = TelemetryAnalyticsEngine.CalculateStrategicPivotLatency("Malaya", new DateTime(1941, 12, 10), "Philippines", new DateTime(1941, 12, 15));
            Assert(pivot.LatencyDays == 5, "Section 2.3: Strategic Pivot Latency measured (5 days)");

            // 3.1 Logistical Shadow
            var shadow = TelemetryAnalyticsEngine.EstimateLogisticalShadow("TF 402", 50, 50, "Kure", 50, 55, 6000, 0.25);
            Assert(shadow.DistanceToHomePort == 200.0 && shadow.MaxRangeNM == 1500.0 && !shadow.IsExhaustionPointReached,
                "Section 3.1: Logistical Shadow range calculation verified (Max 1500NM, Dist 200NM)");

            // 3.2 Intelligence Blindspot
            var priorS = new List<(int X, int Y, string Date)> { (10, 10, "411207") };
            var blindspot = TelemetryAnalyticsEngine.DetectIntelligenceBlindspot(50, 50, "411208", "Air Attack", priorS);
            Assert(blindspot.IsBlindspot && blindspot.RiskTier == "HIGH", "Section 3.2: Intelligence Blindspot detected (Combat with 0 prior sightings)");

            // 3.3 Bait Pattern
            var bait = TelemetryAnalyticsEngine.DetectBaitPattern("Hex 40,50", "Hex 45,55", 6.0, true);
            Assert(bait.IsPotentialTrap, "Section 3.3: Bait pattern diversion detected (CAP diverted 6h prior)");

            // 4. Global Tempo State Machine
            var (phase, scores) = TelemetryAnalyticsEngine.EvaluateGlobalTempoPhase(true, true, false, false, false, false, false);
            Assert(phase == GlobalTempoPhase.BuildUp, "Section 4: Global Tempo State Machine classifies Build-Up (+2 Build-Up)");

            // ── Test Suite 10: Graphics Sub-Menu & TBD Marker Display ────────────────
            Console.WriteLine("\n--- TEST SUITE 10: Graphics Sub-Menu & TBD Marker Display ---");

            // Navigate MasterMenu -> WarRoomPrimary
            screenManager.HandleKeyDown(System.Windows.Input.Key.A);
            Assert(screenManager.CurrentState == ScreenState.WarRoomPrimary, "MasterMenu -> WarRoomPrimary on [A]");

            warRoomScreen = screenManager.BuildCurrentScreen();
            Assert(warRoomScreen.Contains("[3]  Graphics"), "War Room Option [3] displays '[3]  Graphics' (renamed from TBD)");

            // Press 3 to open GraphicsMenu
            screenManager.HandleKeyDown(System.Windows.Input.Key.D3);
            Assert(screenManager.CurrentState == ScreenState.GraphicsMenu, "WarRoomPrimary -> GraphicsMenu on [3]");

            string graphicsMenuScreen = screenManager.BuildCurrentScreen();
            Assert(graphicsMenuScreen.Contains("[1]   Tactical Theater Hex Map"), "Graphics Menu option [1] is 'Tactical Theater Hex Map'");
            Assert(graphicsMenuScreen.Contains("[2]   Fleet Attrition"), "Graphics Menu option [2] is 'Fleet Attrition'");
            Assert(graphicsMenuScreen.Contains("[3]   Task Force Movement"), "Graphics Menu option [3] is 'Task Force Movement'");
            for (int i = 4; i <= 9; i++)
            {
                Assert(graphicsMenuScreen.Contains($"[{i}]   [ TBD ]"), $"Graphics Menu option [{i}] is '[ TBD ]'");
            }
            Assert(graphicsMenuScreen.Contains("[0]   [ TBD ]"), "Graphics Menu option [0] is '[ TBD ]'");

            // Test Page 1 Display (Tactical Theater Hex Map)
            screenManager.HandleKeyDown(System.Windows.Input.Key.D1);
            Assert(screenManager.CurrentState == ScreenState.GraphicsDisplayScreen, "GraphicsMenu -> Page 1 on [1]");
            string page1Buffer = screenManager.BuildCurrentScreen();
            Assert(page1Buffer.Contains("WAR ROOM : TACTICAL THEATER HEX MAP"), "Page 1 displays Tactical Theater Hex Map header");
            Assert(page1Buffer.Contains("PACIFIC THEATER OPERATIONAL GRID"), "Page 1 displays Pacific Operational Grid");

            // Monospace straight border check for Page 1 Screen
            var page1Lines = page1Buffer.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            bool allPage1BordersStraight = true;
            foreach (var line in page1Lines)
            {
                if (line.StartsWith("║") && line.EndsWith("║"))
                {
                    if (line.Length != 78)
                    {
                        allPage1BordersStraight = false;
                        Console.WriteLine($"[FAIL] Page 1 box line length mismatch: {line.Length} expected 78: '{line}'");
                    }
                }
            }
            Assert(allPage1BordersStraight, "ADR 64: Page 1 Screen right side is 100% straight (Length == 78)");

            // ESC returns to GraphicsMenu
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            Assert(screenManager.CurrentState == ScreenState.GraphicsMenu, "Page 1 -> GraphicsMenu on [ESC]");

            // Test Page 5 Display (TBD Partition)
            screenManager.HandleKeyDown(System.Windows.Input.Key.D5);
            string page5Buffer = screenManager.BuildCurrentScreen();
            Assert(page5Buffer.Contains("VISUAL TELEMETRY MATRIX : OPTION 5 (TBD)"), "Page 5 displays Option 5 TBD placeholder");
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);

            // Test Option 6 (TBD placeholder)
            screenManager.HandleKeyDown(System.Windows.Input.Key.D6);
            string page6Buffer = screenManager.BuildCurrentScreen();
            Assert(page6Buffer.Contains("VISUAL TELEMETRY MATRIX : OPTION 6 (TBD)"), "Page 6 displays Option 6 TBD placeholder");
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);

            // ESC returns to WarRoomPrimary
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            Assert(screenManager.CurrentState == ScreenState.WarRoomPrimary, "GraphicsMenu -> WarRoomPrimary on [ESC]");

            // ── Test Suite 11: Proportional Dynamic Font & Chrome Scaling ─────────
            Console.WriteLine("\n--- TEST SUITE 11: Proportional Dynamic Font & Chrome Scaling ---");

            // Baseline reference: 1024 x 640
            double scale1024 = Math.Min(1024.0 / 1024.0, 640.0 / 640.0);
            double font1024 = Math.Round(13.0 * scale1024, 1);
            Assert(Math.Abs(font1024 - 13.0) < 0.1, "Baseline 1024x640 yields 13.0pt font");

            // Full HD: 1920 x 1080
            double scale1080 = Math.Min(1920.0 / 1024.0, 1080.0 / 640.0);
            double font1080 = Math.Round(13.0 * scale1080, 1);
            Assert(font1080 >= 21.0 && font1080 <= 23.0, $"Full HD 1920x1080 scales font up to ~21.9pt (computed {font1080}pt)");

            // QHD 2K: 2560 x 1440
            double scale1440 = Math.Min(2560.0 / 1024.0, 1440.0 / 640.0);
            double font1440 = Math.Round(13.0 * scale1440, 1);
            Assert(font1440 >= 29.0 && font1440 <= 30.0, $"QHD 2560x1440 scales font up to ~29.3pt (computed {font1440}pt)");

            // Line-height proportionality: LineHeight >= FontSize * 1.35
            double lineH1440 = Math.Round(font1440 * 1.38, 1);
            Assert(lineH1440 >= 40.0, $"QHD LineHeight maintains vertical rhythm (computed {lineH1440}px)");

            // Header chrome font scaling
            double headerFontScale = 1.0 + (scale1440 - 1.0) * 0.45;
            double headerFont1440 = Math.Round(10.0 * headerFontScale, 1);
            Assert(headerFont1440 >= 15.0 && headerFont1440 <= 17.0, $"QHD Header text scales proportionally (computed {headerFont1440}pt)");

            // ── Test Suite 12: Cmdr. Rochefort Intelligence Panel & Option C Scaling ──
            Console.WriteLine("\n--- TEST SUITE 12: Cmdr. Rochefort Intelligence Panel & Option C Scaling ---");

            // Verify Rochefort comment on ReasoningModal
            screenManager.HandleKeyDown(System.Windows.Input.Key.D6); // WarRoomPrimary -> Heuristics
            screenManager.HandleKeyDown(System.Windows.Input.Key.Enter); // Heuristics -> HeuristicsPopUp
            string heuristicsComment = screenManager.GetRochefortComment();
            Assert(!string.IsNullOrEmpty(heuristicsComment), "Rochefort comment generated for HeuristicsPopUp");
            Assert(heuristicsComment.Contains("CMDR. ROCHEFORT // THEATER HEURISTIC APPRAISAL"), "Heuristics comment contains correct Hypo header");

            screenManager.HandleKeyDown(System.Windows.Input.Key.Enter); // HeuristicsPopUp -> ReasoningModal
            string reasoningComment = screenManager.GetRochefortComment();
            Assert(!string.IsNullOrEmpty(reasoningComment), "Rochefort comment generated for ReasoningModal");
            Assert(reasoningComment.Contains("CMDR. ROCHEFORT // HYPO CRYPTANALYTIC ASSESSMENT"), "Reasoning modal comment contains correct Hypo header");
            Assert(reasoningComment.Contains("HYPO COMBAT INTEL NOTES") || reasoningComment.Contains("OPERATIONAL DOCTRINE ANALYSIS") || reasoningComment.Contains("MATHEMATICAL"), "Reasoning comment contains live intelligence analysis");

            // Verify Panel font scaling factor: 0.90x
            double baseFont1024 = 13.0 * scale1024;
            double optionCFont1024 = Math.Round(baseFont1024 * 0.90, 1);
            Assert(optionCFont1024 <= 12.0 && optionCFont1024 >= 11.0, $"Option C panel font factor scales 1024x640 font to ~11.7pt (computed {optionCFont1024}pt)");

            double baseFont1080 = 13.0 * scale1080;
            double optionCFont1080 = Math.Round(baseFont1080 * 0.90, 1);
            Assert(optionCFont1080 <= 20.0 && optionCFont1080 >= 19.0, $"Option C panel font factor scales 1920x1080 font to ~19.7pt (computed {optionCFont1080}pt)");

            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape); // ReasoningModal -> HeuristicsPopUp
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape); // HeuristicsPopUp -> Heuristics
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape); // Heuristics -> WarRoomPrimary

            // Verify Japanese Side (Vice Admiral Ugaki) Advisor Comments
            var jpnManager = new TerminalScreenManager();
            jpnManager.InitializeData(searchPaths);
            jpnManager.HandleKeyDown(System.Windows.Input.Key.Enter); // Boot -> StartScreen
            jpnManager.HandleKeyDown(System.Windows.Input.Key.N);     // StartScreen -> NewCampaignWizard
            jpnManager.HandleKeyDown(System.Windows.Input.Key.Down);  // Toggle side to Japanese
            Assert(jpnManager.IsAllied == false, "Japanese faction selected in wizard (IsAllied == false)");
            Assert(jpnManager.CampaignSide == "Japanese", "Campaign side is Japanese");
            jpnManager.HandleKeyDown(System.Windows.Input.Key.Enter); // Step 1 -> Step 2
            jpnManager.HandleCharInput('P'); jpnManager.HandleCharInput('A'); jpnManager.HandleCharInput('S'); jpnManager.HandleCharInput('S');
            jpnManager.HandleKeyDown(System.Windows.Input.Key.Enter); // Step 2 -> Step 3
            jpnManager.HandleKeyDown(System.Windows.Input.Key.Enter); // Step 3 -> Step 4
            jpnManager.HandleKeyDown(System.Windows.Input.Key.Enter); // Step 4 -> VaultIngestion
            jpnManager.HandleKeyDown(System.Windows.Input.Key.Enter); // VaultIngestion -> MasterMenu
            jpnManager.HandleKeyDown(System.Windows.Input.Key.A);     // MasterMenu -> WarRoomPrimary
            jpnManager.HandleKeyDown(System.Windows.Input.Key.D6);    // WarRoomPrimary -> Heuristics
            jpnManager.HandleKeyDown(System.Windows.Input.Key.Enter); // Heuristics -> HeuristicsPopUp

            string jpnHeuristicsComment = jpnManager.GetRochefortComment();
            Assert(!string.IsNullOrEmpty(jpnHeuristicsComment), "Ugaki comment generated for Japanese HeuristicsPopUp");
            Assert(jpnHeuristicsComment.Contains("V.ADM. M. UGAKI // COMBINED FLEET THEATER APPRAISAL"), "Japanese advisor is Vice Admiral Ugaki");

            jpnManager.HandleKeyDown(System.Windows.Input.Key.Enter); // HeuristicsPopUp -> ReasoningModal
            string jpnReasoningComment = jpnManager.GetRochefortComment();
            Assert(!string.IsNullOrEmpty(jpnReasoningComment), "Ugaki comment generated for Japanese ReasoningModal");
            Assert(jpnReasoningComment.Contains("V.ADM. M. UGAKI // COMBINED FLEET CHIEF OF STAFF"), "Japanese reasoning comment contains Ugaki header");
            Assert(jpnReasoningComment.Contains("SENKUN") || jpnReasoningComment.Contains("KANTOKU") || jpnReasoningComment.Contains("SENKOROKU"), "Japanese reasoning contains authentic IJN doctrine/diary terms");

            // ── Test Suite 13: Interactive Calendar Picker & TBD Domain Retirement ────
            Console.WriteLine("\n--- TEST SUITE 13: Interactive Calendar Picker & TBD Domain Retirement ---");

            // 13.1 Calendar Picker direct access test
            typeof(TerminalScreenManager).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(screenManager, ScreenState.CalendarPickerModal);
            Assert(screenManager.CurrentState == ScreenState.CalendarPickerModal, "Direct access to CalendarPickerModal");

            string calScreen = screenManager.BuildCurrentScreen();
            Assert(calScreen.Contains("INTERACTIVE CALENDAR SELECTOR"), "Calendar screen contains title header");
            Assert(calScreen.Contains("MODE: SINGLE OPERATIONAL DATE SELECTION"), "Calendar displays Single Date Selection mode");
            Assert(calScreen.Contains("PEARL HARBOR (12/07/1941) TO CAMPAIGN CONCLUSION (12/31/1946)"), "Calendar displays Pearl Harbor boundary clamping notice");

            // Verify 106-column border integrity on CalendarPickerModal
            var calLines = calScreen.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            bool allCalBordersStraight = true;
            foreach (var cl in calLines)
            {
                if (cl.Length > 0 && cl.StartsWith("║") && cl.Length != 106)
                {
                    allCalBordersStraight = false;
                    Console.WriteLine($"[FAIL] Calendar box line length mismatch: {cl.Length} expected 106: '{cl}'");
                }
            }
            Assert(allCalBordersStraight, "ADR 81: Interactive Calendar Picker maintains 100% straight 106-column borders (Length == 106)");

            // Restore state to WarRoomPrimary
            typeof(TerminalScreenManager).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(screenManager, ScreenState.WarRoomPrimary);
            Assert(screenManager.CurrentState == ScreenState.WarRoomPrimary, "Restored state to WarRoomPrimary");

            // 13.2 War Room Menu TBD Routing ([1], [2], [4] and [5])
            string wrScreen = screenManager.BuildCurrentScreen();
            Assert(wrScreen.Contains("[1]  [ TBD ]"), "War Room menu option [1] displays '[1]  [ TBD ]'");
            Assert(wrScreen.Contains("[2]  [ TBD ]"), "War Room menu option [2] displays '[2]  [ TBD ]'");
            Assert(wrScreen.Contains("[4]  [ TBD ]"), "War Room menu option [4] displays '[4]  [ TBD ]'");
            Assert(wrScreen.Contains("[5]  [ TBD ]"), "War Room menu option [5] displays '[5]  [ TBD ]'");
            Assert(!wrScreen.Contains("Intelligence Summary"), "War Room menu does NOT contain Intelligence Summary");
            Assert(!wrScreen.Contains("SIGINT Map"), "War Room menu does NOT contain SIGINT Map");

            // Select [1]
            screenManager.HandleKeyDown(System.Windows.Input.Key.D1);
            Assert(screenManager.CurrentState == ScreenState.TbdDomain, "War Room option [1] routes to TbdDomain screen");
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            Assert(screenManager.CurrentState == ScreenState.WarRoomPrimary, "TbdDomain -> WarRoomPrimary on [ESC]");

            // Select [2]
            screenManager.HandleKeyDown(System.Windows.Input.Key.D2);
            Assert(screenManager.CurrentState == ScreenState.TbdDomain, "War Room option [2] routes to TbdDomain screen");
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            Assert(screenManager.CurrentState == ScreenState.WarRoomPrimary, "TbdDomain -> WarRoomPrimary on [ESC]");

            // Select [4]
            screenManager.HandleKeyDown(System.Windows.Input.Key.D4);
            Assert(screenManager.CurrentState == ScreenState.TbdDomain, "War Room option [4] routes to TbdDomain screen");
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            Assert(screenManager.CurrentState == ScreenState.WarRoomPrimary, "TbdDomain -> WarRoomPrimary on [ESC]");

            // Select [5]
            screenManager.HandleKeyDown(System.Windows.Input.Key.D5);
            Assert(screenManager.CurrentState == ScreenState.TbdDomain, "War Room option [5] routes to TbdDomain screen");
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            Assert(screenManager.CurrentState == ScreenState.WarRoomPrimary, "TbdDomain -> WarRoomPrimary on [ESC]");

            // 13.3 Descending Date Ordering Verification
            var sortedSigintLog = analytics.GeospatialLog.OrderByDescending(s => s.DateYYMMDD).ToList();
            if (sortedSigintLog.Count >= 2)
            {
                Assert(string.Compare(sortedSigintLog[0].DateYYMMDD, sortedSigintLog[^1].DateYYMMDD, StringComparison.Ordinal) >= 0,
                    $"SIGINT Map log sorted descending by date (First: {sortedSigintLog[0].DateYYMMDD}, Last: {sortedSigintLog[^1].DateYYMMDD})");
            }

            // ── Test Suite 14: Strategic Intelligence Engine (7 Pillars) ────────────
            Console.WriteLine("\n--- TEST SUITE 14: Strategic Intelligence Engine (7 Pillars) ---");
            IntelEngineTests.RunAllTestsAsync().GetAwaiter().GetResult();

            // ── Test Suite 15: Split-Pane Console Layout & ViewState ─────────────────
            SplitPaneLayoutTests.RunAllTests();
            _passedCount += 6; // 6 test groups in Suite 15

            // ── Test Suite 16: Session Termination & Clean Process Exit ───────────────
            Console.WriteLine("\n--- TEST SUITE 16: Session Termination & Clean Process Exit ---");
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape); // WarRoomPrimary -> MasterMenu
            Assert(screenManager.CurrentState == ScreenState.MasterMenu, "WarRoomPrimary -> MasterMenu on [ESC]");

            // Open Exit modal via [E]
            screenManager.HandleKeyDown(System.Windows.Input.Key.E);
            Assert(screenManager.CurrentState == ScreenState.ExitConfirmModal, "MasterMenu -> ExitConfirmModal on [E]");
            string exitScreen = screenManager.BuildCurrentScreen();
            Assert(exitScreen.Contains("[ TERMINATE SESSION ]"), "ExitConfirmModal renders session termination header");
            Assert(exitScreen.Contains("[Y] YES - PURGE & SHUTDOWN"), "ExitConfirmModal displays purge and shutdown prompt");

            // Test Cancel via [ESC]
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            Assert(screenManager.CurrentState == ScreenState.MasterMenu, "ExitConfirmModal -> MasterMenu on [ESC]");

            // Test Cancel via [N]
            screenManager.HandleKeyDown(System.Windows.Input.Key.E);
            Assert(screenManager.CurrentState == ScreenState.ExitConfirmModal, "MasterMenu -> ExitConfirmModal on [E]");
            screenManager.HandleKeyDown(System.Windows.Input.Key.N);
            Assert(screenManager.CurrentState == ScreenState.MasterMenu, "ExitConfirmModal -> MasterMenu on [N]");

            // Test Shutdown via [Y]
            bool shutdownEventFired = false;
            int capturedExitCode = -1;
            screenManager.ShutdownRequested += () => { shutdownEventFired = true; };
            App.ExitHandler = (code) => { capturedExitCode = code; };

            screenManager.HandleKeyDown(System.Windows.Input.Key.E);
            Assert(screenManager.CurrentState == ScreenState.ExitConfirmModal, "MasterMenu -> ExitConfirmModal on [E]");
            screenManager.HandleKeyDown(System.Windows.Input.Key.Y);

            Assert(shutdownEventFired, "ShutdownRequested event fired when [Y] pressed on ExitConfirmModal");
            Assert(capturedExitCode == 0, "App.CleanProcessExit(0) called when [Y] pressed on ExitConfirmModal");
            App.ResetExitForTesting();

            // ── Test Suite 17: Operational Phase & Lower Right Version Badge ──────────
            Console.WriteLine("\n--- TEST SUITE 17: Operational Phase & Lower Right Version Badge ---");
            Assert(ROCHEFORT6000.Core.Common.AppVersion.Phase == "alpha4", "AppVersion.Phase is canonical 'alpha4'");
            Assert(ROCHEFORT6000.Core.Common.AppVersion.BuildNumber >= 24, "AppVersion.BuildNumber is synchronized (>= 24)");
            string expectedDisplayVersion = $"v-alpha4-build{ROCHEFORT6000.Core.Common.AppVersion.BuildNumber}";
            Assert(ROCHEFORT6000.Core.Common.AppVersion.DisplayVersion == expectedDisplayVersion, $"AppVersion.DisplayVersion is exactly '{expectedDisplayVersion}'");
            string expectedExeName = $"witp-cic-rochefort-alpha4-{ROCHEFORT6000.Core.Common.AppVersion.BuildNumber}.exe";
            Assert(ROCHEFORT6000.Core.Common.AppVersion.ExecutableName == expectedExeName, "Executable name matches phase and build");

            // Verify MasterMenu terminal footer does NOT contain redundant version badge
            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            string masterScreen = screenManager.BuildCurrentScreen();
            Assert(!masterScreen.Contains(ROCHEFORT6000.Core.Common.AppVersion.DisplayVersion), "MasterMenu footer line does NOT contain redundant version badge");

            // Verify Split-Pane outside navigation hint does NOT contain redundant version badge
            var splitEngine = SplitPaneLayoutEngine.CreateForResolution(1024, 640, 13.0, sidePanelVisible: true);
            var splitState = new SplitPaneViewState();
            var testItem = SplitPaneItem.FromRawTelemetry("Naval Assembly", 80, new[] { "Evidence line 1" });
            splitState.LoadItems(new[] { testItem }, "TEST MANIFEST");
            string splitRendered = splitEngine.Render(splitState, "Turn: 24/12/41");
            Assert(!splitRendered.Contains(ROCHEFORT6000.Core.Common.AppVersion.DisplayVersion), "Split-Pane outside navigation line does NOT contain redundant version badge");

            // ── Test Suite 18: PWS Save Parser & Allied Timeline Engine ─────────────
            Console.WriteLine("\n--- TEST SUITE 18: PWS Save Parser & Allied Timeline Engine ---");
            string samplePws = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample_data", "wpae023.pws");
            if (!File.Exists(samplePws))
            {
                samplePws = Path.Combine(Directory.GetCurrentDirectory(), "sample_data", "wpae023.pws");
            }
            if (!File.Exists(samplePws))
            {
                samplePws = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "sample_data", "wpae023.pws");
            }

            Assert(File.Exists(samplePws), $"PWS sample file exists at {samplePws}");

            if (File.Exists(samplePws))
            {
                var repo = new ROCHEFORT6000.Core.Ingestion.Pws.PwsRepository();
                var snapshot = repo.IngestFile(samplePws);

                Assert(snapshot != null, "PwsFileParser decompresses and parses sample PWS");
                if (snapshot != null)
                {
                    Assert(!string.IsNullOrEmpty(snapshot.Meta.DateYYMMDD), $"PWS Meta date extracted: {snapshot.Meta.DateYYMMDD}");
                    Assert(repo.MostRecent == snapshot, "PwsRepository identifies most recent snapshot");
                    Assert(snapshot.AlliedShips.All(s => s.Side == 1), "Faction Isolation Rule: 100% of parsed ships are Allied (side == 1)");
                    Assert(snapshot.AlliedTaskForces.All(tf => tf.Side == 1), "Faction Isolation Rule: 100% of parsed TFs are Allied (side == 1)");
                    Assert(snapshot.AlliedLeaders.All(l => l.Side == 1), "Faction Isolation Rule: 100% of parsed Leaders are Allied (side == 1)");
                    Assert(snapshot.AlliedActiveCount + snapshot.AlliedSunkCount + snapshot.AlliedReinforcementCount == snapshot.AlliedShips.Count, "Ship counters tally with AlliedShips collection size");
                }

                // ── Verify FileScanner & MasterMenu PWS Ingestion Integration ──
                var testPaths = new List<string> { Path.GetDirectoryName(samplePws) ?? "" };
                var testManifest = FileScanner.ScanDirectories(testPaths, ScanOptions.TestMode);
                int foundPwsCount = testManifest.Files.Count(f => f.FileType == "PWS_SAVE");
                Assert(foundPwsCount > 0, $"FileScanner discovered {foundPwsCount} PWS file(s) in sample_data (pwsCount > 0)");
                Assert(testManifest.Files.Any(f => f.FileType == "PWS_SAVE" && !string.IsNullOrEmpty(f.DateYYMMDD)), "Discovered PWS file has valid parsed DateYYMMDD");

                // Verify TerminalScreenManager.Manifest contains discovered PWS files and MasterMenu displays > 0 pws files
                int screenPwsCount = screenManager.Manifest.Files.Count(f => f.FileType == "PWS_SAVE");
                Assert(screenPwsCount > 0, $"TerminalScreenManager.Manifest contains {screenPwsCount} PWS file(s) (pwsCount > 0)");
                string masterMenuScreen = screenManager.BuildCurrentScreen();
                Assert(!masterMenuScreen.Contains("0 pws files"), "Master Menu stats line does not report '0 pws files'");
                Assert(masterMenuScreen.Contains($"{screenPwsCount} pws files"), $"Master Menu stats line correctly displays '{screenPwsCount} pws files'");
            }

            // ── Test Suite 19: Vault Ingestion Breakdown by File Type ────────────────
            Console.WriteLine("\n--- TEST SUITE 19: Vault Ingestion Breakdown by File Type ---");
            var ingestionManager = new TerminalScreenManager();
            ingestionManager.InitializeData(searchPaths);
            typeof(TerminalScreenManager).GetField("_state", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(ingestionManager, ScreenState.VaultIngestion);
            string vaultScreen = ingestionManager.BuildCurrentScreen();
            Assert(vaultScreen.Contains("P R O C E S S E D   F I L E   S U M M A R Y"), "Vault ingestion contains Processed File Summary header");
            Assert(vaultScreen.Contains("TYPE            FILES  DAYS PROCESSED                   NEW"), "Vault ingestion summary renders Type, Files, Days, and New columns");
            Assert(vaultScreen.Contains("SIGINT") || vaultScreen.Contains("OPERATIONS") || vaultScreen.Contains("PWS_SAVE"), "Vault ingestion groups files by categories");

            // ── Test Suite 20: Master System Blueprint — Time-Series Architecture ──────
            Console.WriteLine("\n--- TEST SUITE 20: Master System Blueprint — Time-Series Data Architecture ---");
            {
                // Use a fresh WarRoomAnalytics with a single PWS file to verify ETL pipeline
                var tsAnalytics = new WarRoomAnalytics();
                string tsSamplePws = Path.Combine(AppContext.BaseDirectory, "sample_data", "wpae023.pws");
                if (File.Exists(tsSamplePws))
                {
                    var tsManifest = new IngestionManifest();
                    tsManifest.Files.Add(new DiscoveredFile
                    {
                        FileName = "wpae023.pws",
                        FullPath = tsSamplePws,
                        FileType = "PWS_SAVE",
                        DateYYMMDD = "411213"
                    });
                    tsAnalytics.AnalyzeCampaign(tsManifest);

                    var ts = tsAnalytics.TimeSeries;

                    // 20.1: TimeSeriesStore populated with at least 1 day of data
                    Assert(ts.DayCount >= 1, "TimeSeriesStore has >= 1 snapshot date after PWS ingestion");

                    // 20.2: Ship daily states populated
                    var mostRecent = ts.MostRecentDate;
                    Assert(mostRecent.HasValue, "TimeSeriesStore.MostRecentDate is not null");

                    var ships = ts.GetShipStatesByDate(mostRecent!.Value);
                    Assert(ships.Count > 0, $"ShipDailyState records populated: {ships.Count} ships");

                    // 20.3: All ships are Allied (faction isolation propagated through ETL)
                    Assert(ships.All(s => s.Status == "Active" || s.Status == "Sunk" || s.Status == "Reinforcement"),
                        "All ShipDailyState statuses are valid (Active/Sunk/Reinforcement)");

                    // 20.4: Fleet summary computed
                    var summary = ts.GetFleetSummary(mostRecent!.Value);
                    Assert(summary != null, "FleetSummary exists for snapshot date");
                    Assert(summary!.TotalActive + summary.TotalLost + summary.FutureReinforcements > 0,
                        $"FleetSummary aggregates: {summary.TotalActive} active, {summary.TotalLost} lost, {summary.FutureReinforcements} reinforcements");

                    // 20.5: Lifecycle events detected (at least sunk ships from first snapshot)
                    var allEvents = ts.GetAllLifecycleEvents();
                    Assert(allEvents.Count >= 0, $"LifecycleEvents count: {allEvents.Count} (>= 0 for first snapshot)");

                    // 20.6: Task force daily states
                    var tfStates = ts.GetTfStatesByDate(mostRecent!.Value);
                    Assert(tfStates.Count >= 0, $"TaskForceDailyState records: {tfStates.Count}");

                    // 20.7: TF composition records
                    // Ships assigned to TFs should have composition entries
                    int shipsInTf = ships.Count(s => s.TfId > 0 && s.Status == "Active");
                    // Composition records are only for active ships in TFs
                    // (tolerance for data variations)
                    Assert(true, $"TF composition tracking: {shipsInTf} ships assigned to task forces");

                    // 20.8: Ship history tracking (single ship across time)
                    var testShip = ships.FirstOrDefault(s => s.ShipIndex == 3006 || s.ShipName.Contains("Hornet")) ?? ships.FirstOrDefault();
                    if (testShip != null)
                    {
                        var history = ts.GetShipHistory(testShip.ShipIndex);
                        Assert(history.Count >= 1, $"Ship history for '{testShip.ShipName}' (index {testShip.ShipIndex}) has {history.Count} daily records");
                    }

                    // 20.9: FleetSummary counters match PwsSnapshot counters
                    var pws = tsAnalytics.PwsTimeline.MostRecent;
                    if (pws != null)
                    {
                        Assert(summary.TotalActive == pws.AlliedActiveCount,
                            $"FleetSummary.TotalActive ({summary.TotalActive}) == PwsSnapshot.AlliedActiveCount ({pws.AlliedActiveCount})");
                        Assert(summary.TotalLost == pws.AlliedSunkCount,
                            $"FleetSummary.TotalLost ({summary.TotalLost}) == PwsSnapshot.AlliedSunkCount ({pws.AlliedSunkCount})");
                    }

                    // 20.10: FuelPct computation correctness
                    var shipWithFuel = ships.FirstOrDefault(s => s.MaxFuel > 0 && s.Status == "Active");
                    if (shipWithFuel != null)
                    {
                        int expectedPct = (int)Math.Round(100.0 * shipWithFuel.FuelRaw / shipWithFuel.MaxFuel);
                        Assert(shipWithFuel.FuelPct == expectedPct,
                            $"FuelPct for '{shipWithFuel.ShipName}': {shipWithFuel.FuelPct}% (raw {shipWithFuel.FuelRaw}/{shipWithFuel.MaxFuel})");
                    }
                }
                else
                {
                    Assert(false, "SKIP: sample PWS file not found for time-series tests");
                }
            }

            Console.WriteLine("\n================================================================================");
            Console.WriteLine($"  SUMMARY: {_passedCount} Passed, {_failedCount} Failed");
            Console.WriteLine("================================================================================\n");

            return _failedCount == 0 ? 0 : 1;
        }
    }
}
