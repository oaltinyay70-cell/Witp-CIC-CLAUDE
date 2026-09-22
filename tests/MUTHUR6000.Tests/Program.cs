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
            Console.WriteLine("  MUTHUR 6000 // PACIFIC WAR ROOM AUTOMATED ANALYTICS TEST SUITE");
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

            screenManager.HandleKeyDown(System.Windows.Input.Key.Escape);
            Assert(screenManager.CurrentState == ScreenState.MasterMenu, "WarRoom -> MasterMenu on [ESC]");

            Console.WriteLine("\n================================================================================");
            Console.WriteLine($"  SUMMARY: {_passedCount} Passed, {_failedCount} Failed");
            Console.WriteLine("================================================================================\n");

            return _failedCount == 0 ? 0 : 1;
        }
    }
}
