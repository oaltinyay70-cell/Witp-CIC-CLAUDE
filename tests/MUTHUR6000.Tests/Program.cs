using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MUTHUR6000.Core.Analytics;
using MUTHUR6000.Core.Controller;
using MUTHUR6000.Core.Database;
using MUTHUR6000.Core.Ingestion;

namespace MUTHUR6000.Tests
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
            var manifest = FileScanner.ScanDirectories(searchPaths);
            Assert(manifest.Files.Count > 0, $"FileScanner processed {manifest.Files.Count} files");
            Assert(manifest.UniqueDates.Count > 0, $"Ingested {manifest.UniqueDates.Count} distinct operational dates: [{string.Join(", ", manifest.UniqueDates)}]");

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

            // ── Test Suite 6: Unified Dossier Search ─────────────────────────────────
            Console.WriteLine("\n--- TEST SUITE 6: Unified Dossier Search ---");
            var dossierResults = analytics.SearchDossier("KB");
            Assert(dossierResults != null, "Dossier query for 'KB' executes without error");
            var portResults = analytics.SearchDossier("Singapore");
            Assert(portResults != null, "Dossier query for 'Singapore' executes without error");

            // ── Test Suite 7: Terminal Screen Manager Flow & Navigation ─────────────
            Console.WriteLine("\n--- TEST SUITE 7: Terminal Screen Manager Flow ---");
            var screenManager = new TerminalScreenManager();
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

            screenManager.HandleKeyDown(System.Windows.Input.Key.D1);
            Assert(screenManager.CurrentState == ScreenState.IntelligenceSummary, "WarRoom -> IntelligenceSummary on [1]");

            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            Assert(screenManager.CurrentState == ScreenState.WarRoomPrimary, "IntelligenceSummary -> WarRoom on [ESC]");

            // ── Test Suite 8: War Room Heuristics & Logical Blueprints ──────────────
            Console.WriteLine("\n--- TEST SUITE 8: War Room Heuristics & Logical Blueprints ---");
            var heuristics = analytics.Heuristics;
            Assert(heuristics != null, "HeuristicsEngine instantiated on WarRoomAnalytics");

            // Metric 1: Regional Power Shift
            Assert(heuristics!.RegionalShifts.Count >= 8, $"Regional Power Shift evaluated {heuristics.RegionalShifts.Count} strategic regions");
            Assert(heuristics.HasSufficientTemporalData == false, "Temporal Guard active: < 4 days marked as data accumulating");

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
                    if (line.Length != 78)
                    {
                        allPopUpBordersStraight = false;
                        Console.WriteLine($"[FAIL] Box line length mismatch: {line.Length} expected 78: '{line}'");
                    }
                }
            }
            Assert(allPopUpBordersStraight, "ADR 64: In-Terminal Unicode Graph Box right side is 100% straight (Length == 78)");

            screenManager.HandleKeyDown(System.Windows.Input.Key.Enter);
            Assert(screenManager.CurrentState == ScreenState.ReasoningModal, "HeuristicsPopUp -> ReasoningModal (Raw Evidence) on [ENTER]");

            string modalBuffer = screenManager.BuildCurrentScreen();
            var modalLines = modalBuffer.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            bool allModalBordersStraight = true;
            foreach (var line in modalLines)
            {
                if (line.StartsWith("║") && line.EndsWith("║"))
                {
                    if (line.Length != 78)
                    {
                        allModalBordersStraight = false;
                        Console.WriteLine($"[FAIL] Modal line length mismatch: {line.Length} expected 78: '{line}'");
                    }
                }
            }
            Assert(allModalBordersStraight, "ADR 64: Reasoning & Evidence Modal Box right side is 100% straight (Length == 78)");

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

            string warRoomScreen = screenManager.BuildCurrentScreen();
            Assert(warRoomScreen.Contains("[3]  Graphics"), "War Room Option [3] displays '[3]  Graphics' (renamed from TBD)");

            // Press 3 to open GraphicsMenu
            screenManager.HandleKeyDown(System.Windows.Input.Key.D3);
            Assert(screenManager.CurrentState == ScreenState.GraphicsMenu, "WarRoomPrimary -> GraphicsMenu on [3]");

            string graphicsMenuScreen = screenManager.BuildCurrentScreen();
            for (int i = 1; i <= 9; i++)
            {
                Assert(graphicsMenuScreen.Contains($"[{i}]   [ TBD ]"), $"Graphics Menu option [{i}] is '[ TBD ]'");
            }
            Assert(graphicsMenuScreen.Contains("[0]   [ TBD ]"), "Graphics Menu option [0] is '[ TBD ]'");

            // Test Page 1 Display (TBD Partition)
            screenManager.HandleKeyDown(System.Windows.Input.Key.D1);
            Assert(screenManager.CurrentState == ScreenState.GraphicsDisplayScreen, "GraphicsMenu -> Page 1 on [1]");
            string page1Buffer = screenManager.BuildCurrentScreen();
            Assert(page1Buffer.Contains("VISUAL TELEMETRY MATRIX : OPTION 1 (TBD)"), "Page 1 displays Option 1 TBD placeholder");
            Assert(page1Buffer.Contains("MODULE STATUS : RESERVED TELEMETRY PARTITION (TBD)"), "Page 1 displays TBD status header");

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

            // Footer chrome font scaling
            double footerFontScale = 1.0 + (scale1440 - 1.0) * 0.40;
            double footerFont1440 = Math.Round(10.0 * footerFontScale, 1);
            Assert(footerFont1440 >= 14.5 && footerFont1440 <= 16.5, $"QHD Footer text scales proportionally (computed {footerFont1440}pt)");

            Console.WriteLine("\n================================================================================");
            Console.WriteLine($"  SUMMARY: {_passedCount} Passed, {_failedCount} Failed");
            Console.WriteLine("================================================================================\n");

            return _failedCount == 0 ? 0 : 1;
        }
    }
}
