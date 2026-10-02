using System;
using System.Collections.Generic;
using System.Linq;
using ROCHEFORT6000.Core.Controller;

namespace ROCHEFORT6000.Tests
{
    public static class SplitPaneLayoutTests
    {
        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"[TEST FAILURE] {message}");
            }
        }

        public static void RunAllTests()
        {
            Console.WriteLine("\n--- TEST SUITE 15: Split-Pane Console Layout & ViewState Tests ---");

            TestEllipsisTruncationAndSpanUtility();
            TestDynamicHeightFloorAndResolutionIndependence();
            TestBoxBorderIntegrityAndZeroWrap();
            TestActiveBorderFocusSwitching();
            TestLiveSelectionUpdatesWithoutEnter();
            TestNestedNavigationInLowerPane();
            TestEvidenceWordWrappingWithHangingIndent();

            Console.WriteLine("[PASS] All Split-Pane Layout & ViewState Tests PASSED successfully!");
        }

        private static void TestEllipsisTruncationAndSpanUtility()
        {
            string shortText = "Battleship Yamato";
            string truncatedShort = SplitPaneLayoutEngine.TruncateWithEllipsis(shortText, 30);
            Assert(truncatedShort == "Battleship Yamato", "Short text is not truncated");

            string longText = "Major Naval Assembly at 52,82: up to 9 ships including 2 Battleships, 4 Heavy Cruisers, and 3 Destroyers detected";
            string truncatedLong = SplitPaneLayoutEngine.TruncateWithEllipsis(longText, 40);
            Assert(truncatedLong.Length == 40, $"Truncated length must exactly equal 40 (Actual: {truncatedLong.Length})");
            Assert(truncatedLong.EndsWith("..."), $"Truncated text must terminate with ellipsis (Actual: '{truncatedLong}')");

            // Span-based zero-allocation overload
            Span<char> dest = stackalloc char[35];
            SplitPaneLayoutEngine.TruncateWithEllipsis(longText.AsSpan(), dest, out int written);
            Assert(written == 35, $"Span truncate must write exactly destination length (Actual: {written})");
            Assert(dest.EndsWith("..."), "Span truncate destination must end with ellipsis");

            // Formatting row guarantees exact box width
            int innerWidth = 76;
            string boxRow = SplitPaneLayoutEngine.FormatBoxRow(longText, innerWidth, isActive: false);
            Assert(boxRow.Length == innerWidth + 4, $"Formatted box row must equal innerWidth + 4 = 80 (Actual: {boxRow.Length})");
            Assert(boxRow.StartsWith("│ ") && boxRow.EndsWith(" │"), "Inactive box row must be bounded by │ on left and right");

            string activeBoxRow = SplitPaneLayoutEngine.FormatBoxRow(longText, innerWidth, isActive: true);
            Assert(activeBoxRow.Length == innerWidth + 4, $"Active box row must equal innerWidth + 4 = 80 (Actual: {activeBoxRow.Length})");
            Assert(activeBoxRow.StartsWith("║ ") && activeBoxRow.EndsWith(" ║"), "Active box row must be bounded by ║ on left and right");
            Console.WriteLine("  [PASS] Ellipsis truncation and span zero-allocation utilities verified");
        }

        private static void TestDynamicHeightFloorAndResolutionIndependence()
        {
            // Minimum resolution floor: 1024x640 (72-col safe floor with side panel)
            var engine = SplitPaneLayoutEngine.CreateForResolution(1024, 640, 13.0, sidePanelVisible: true);
            Assert(engine.BoxWidth == 72, $"Box width for 1024x640 with side panel must be 72 (Actual: {engine.BoxWidth})");
            Assert(engine.TotalHeightRows == 26, $"Total height budget must be 26 (Actual: {engine.TotalHeightRows})");
            Assert(Math.Abs(engine.UpperRatio - 0.45f) < 0.001f, "Upper pane ratio must be 45%");
            Assert(Math.Abs(engine.LowerRatio - 0.55f) < 0.001f, "Lower pane ratio must be 55%");
            Console.WriteLine("  [PASS] Dynamic height floor and resolution calculation verified");
        }

        private static void TestBoxBorderIntegrityAndZeroWrap()
        {
            var engine = new SplitPaneLayoutEngine
            {
                BoxWidth = 80,
                TotalHeightRows = 24,
                UpperRatio = 0.45f,
                LowerRatio = 0.55f
            };

            var state = new SplitPaneViewState();
            var testItems = new List<SplitPaneItem>();
            for (int i = 0; i < 15; i++)
            {
                string rawLongTitle = $"Major Task Force {i} with extremely long reconnaissance description that definitely exceeds terminal line width at coordinates 180,107 with 12 vessels";
                var rawLines = new List<string>
                {
                    "TELEMETRY SOURCE: SIGINT Intercept Report & Radio Direction Finding Sector Analysis",
                    $"LOCALIZED POSITION: Hex {150 + i},{80 + i} (Coral Sea Theater Approach)",
                    "OBSERVATION FREQUENCY: 4 distinct sightings recorded across campaign turns.",
                    "CHRONOLOGY / RECENCY: Evaluated for operational turn 24/12/41.",
                    "TACTICAL REASONING: High priority enemy carrier battlegroup interdiction target.",
                    "CONFIDENCE ANALYSIS: Pure numerical Bayesian sensor correlation with recency decay."
                };
                testItems.Add(SplitPaneItem.FromRawTelemetry(rawLongTitle, 85, rawLines));
            }

            state.LoadItems(testItems, "TEST DOMAIN MANIFEST");

            string rendered = engine.Render(state, "Turn: 24/12/41");
            var lines = rendered.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            // Filter out empty lines at the very end
            var contentLines = lines.Where(l => !string.IsNullOrEmpty(l)).ToList();

            // Every line containing box borders must have an exact length equal to BoxWidth (80)
            int boxLinesChecked = 0;
            foreach (var line in contentLines)
            {
                if (line.StartsWith("┌") || line.StartsWith("│") || line.StartsWith("└") || line.StartsWith("═") ||
                    line.StartsWith("╔") || line.StartsWith("║") || line.StartsWith("╚"))
                {
                    Assert(line.Length == 80, $"Box line length must be exactly 80 chars without wrapping. Got {line.Length}: '{line}'");
                    boxLinesChecked++;
                }
            }

            Assert(boxLinesChecked >= 15, $"At least 15 structured box lines verified (Actual: {boxLinesChecked})");

            // Verify navigation hint is outside and below the Lower Pane bottom border
            int lowerBottomIdx = -1;
            int hintIdx = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if ((lines[i].StartsWith("└") || lines[i].StartsWith("╚")) && (lines[i].EndsWith("┘") || lines[i].EndsWith("╝")))
                {
                    lowerBottomIdx = i; // Last one is lower pane bottom border
                }
                if (lines[i].Contains(">> [UP/DOWN]"))
                {
                    hintIdx = i;
                }
            }

            Assert(lowerBottomIdx >= 0, "Lower pane bottom border found");
            Assert(hintIdx > lowerBottomIdx, $"Navigation hint must be positioned outside and below lower pane bottom border (Hint line: {hintIdx}, Border line: {lowerBottomIdx})");
            Console.WriteLine("  [PASS] Box border integrity, zero horizontal wrapping, and outside navigation hint verified");
        }

        private static void TestLiveSelectionUpdatesWithoutEnter()
        {
            var engine = new SplitPaneLayoutEngine { BoxWidth = 80, TotalHeightRows = 24 };
            var state = new SplitPaneViewState();

            var item1 = SplitPaneItem.FromRawTelemetry("Item 1: Assembly at 52,82", 90, new[] { "TELEMETRY SOURCE: Hydrophone contact at 52,82" });
            var item2 = SplitPaneItem.FromRawTelemetry("Item 2: Assembly at 180,107", 75, new[] { "TELEMETRY SOURCE: Air recon at 180,107" });
            var item3 = SplitPaneItem.FromRawTelemetry("Item 3: Assembly at 51,99", 50, new[] { "TELEMETRY SOURCE: Coastwatcher report at 51,99" });

            state.LoadItems(new[] { item1, item2, item3 }, "LIVE TEST MANIFEST");

            Assert(state.SelectedItemIndex == 0, "Initial selection is item 0");
            Assert(state.SelectedItem?.Title == "Item 1: Assembly at 52,82", "Initial selected item matches item 1");

            // Live Update: user presses Down arrow
            state.MoveDown();
            Assert(state.SelectedItemIndex == 1, "Selection moved down to item 1");
            Assert(state.SelectedItem?.Title == "Item 2: Assembly at 180,107", "Lower pane immediately reflects item 2 without requiring ENTER");

            // Verify rendered buffer reflects item 2 immediately in Lower Pane
            string rendered = engine.Render(state);
            Assert(rendered.Contains("ITEM: Item 2: Assembly at 180,107"), "Rendered lower pane shows Item 2 instantly");
            Assert(!rendered.Contains("ITEM: Item 1: Assembly at 52,82"), "Rendered lower pane does not show Item 1 anymore");

            Console.WriteLine("  [PASS] Live selection updates on Upper Pane arrow keys verified");
        }

        private static void TestNestedNavigationInLowerPane()
        {
            var engine = new SplitPaneLayoutEngine { BoxWidth = 80, TotalHeightRows = 24 };
            var state = new SplitPaneViewState();

            var rawLines = new List<string>
            {
                "TELEMETRY SOURCE: Dauntless visual reconnaissance at 180,107",
                "LOCALIZED POSITION: Hex 180,107",
                "TACTICAL REASONING: Carrier strike concentration screening enemy convoy",
                "CONFIDENCE ANALYSIS: Corroborated with multi-turn radar tracks"
            };

            var item = SplitPaneItem.FromRawTelemetry("Carrier Battlegroup Kido Butai", 95, rawLines);
            state.LoadItems(new[] { item }, "CARRIER INTEL");

            Assert(state.ActivePane == PaneFocus.Upper, "Initial focus is Upper Pane");
            Assert(state.SelectedTopicIndex == 0, "Initial topic index is 0");
            Assert(state.SelectedTopic?.Title == "TELEMETRY SOURCE & LOCALIZATION HISTORY", "Topic 0 is Telemetry");

            // Switch to Lower Pane via ToggleFocus / Enter / Tab
            state.ToggleFocus();
            Assert(state.ActivePane == PaneFocus.Lower, "Focus switched to Lower Pane");

            // Rendered hint in Lower Pane
            string renderedLower = engine.Render(state);
            Assert(renderedLower.Contains(">> [UP/DOWN] Select Topic | [ENTER] Zoom Reasoning | [TAB/ESC] Return to Items"),
                "Navigation hint in Lower Pane informs user of topic selection and returning");

            // Navigate between topics inside Lower Pane
            state.MoveDown();
            Assert(state.SelectedTopicIndex == 1, "Selected topic moved down to 1");
            Assert(state.SelectedTopic?.Title == "TACTICAL ASSESSMENT & DOCTRINE GUIDANCE", "Topic 1 is Tactical Assessment");

            string renderedTopic1 = engine.Render(state);
            Assert(renderedTopic1.Contains("--- EVIDENCE: TACTICAL ASSESSMENT & DOCTRINE GUIDANCE ---"),
                "Evidence section updates live to reflect selected topic 1");
            Assert(renderedTopic1.Contains("Carrier strike concentration"),
                "Tactical reasoning evidence line rendered");

            // Navigate to Topic 2
            state.MoveDown();
            Assert(state.SelectedTopicIndex == 2, "Selected topic moved down to 2");
            Assert(state.SelectedTopic?.Title == "CONFIDENCE DERIVATION & MATHEMATICAL PROOF", "Topic 2 is Mathematical Proof");

            // Return to Upper Pane
            state.FocusUpper();
            Assert(state.ActivePane == PaneFocus.Upper, "Focus successfully returned to Upper Pane");

            Console.WriteLine("  [PASS] Nested navigation inside Lower Pane topics and evidence verified");
        }

        private static void TestActiveBorderFocusSwitching()
        {
            var engine = new SplitPaneLayoutEngine { BoxWidth = 72, TotalHeightRows = 24 };
            var state = new SplitPaneViewState();

            var item = SplitPaneItem.FromRawTelemetry(
                "Battleship Nagato at 52,82", 
                90, 
                new[] { "TELEMETRY SOURCE: SIGINT Intercept Report & Radio Direction Finding" });
            state.LoadItems(new[] { item }, "TACTICAL SURFACE SCAN");

            // 1. When Upper Pane is active:
            Assert(state.ActivePane == PaneFocus.Upper, "Upper pane starts active");
            string upperActiveRender = engine.Render(state);

            Assert(upperActiveRender.Contains("╔═ UPPER PANE: MAIN ITEMS LIST [ACTIVE]"), 
                "Upper pane has double-line top border and [ACTIVE] tag when focused");
            Assert(upperActiveRender.Contains("║ * Battleship Nagato at 52,82"), 
                "Upper pane rows have double-line borders ║ when focused");
            Assert(upperActiveRender.Contains("┌─ LOWER PANE: SELECTED ITEM DETAILS"), 
                "Lower pane has single-line top border when unfocused");
            Assert(upperActiveRender.Contains("│ ITEM: Battleship Nagato at 52,82"), 
                "Lower pane rows have single-line borders │ when unfocused");

            // 2. Switch focus to Lower Pane:
            state.SwitchPane();
            Assert(state.ActivePane == PaneFocus.Lower, "Active pane switched to Lower");
            string lowerActiveRender = engine.Render(state);

            Assert(lowerActiveRender.Contains("┌─ UPPER PANE: MAIN ITEMS LIST"), 
                "Upper pane has single-line top border when unfocused");
            Assert(lowerActiveRender.Contains("│ > Battleship Nagato at 52,82"), 
                "Upper pane rows have single-line borders │ when unfocused");
            Assert(lowerActiveRender.Contains("╔═ LOWER PANE: SELECTED ITEM DETAILS [ACTIVE]"), 
                "Lower pane has double-line top border and [ACTIVE] tag when focused");
            Assert(lowerActiveRender.Contains("║ ITEM: Battleship Nagato at 52,82"), 
                "Lower pane rows have double-line borders ║ when focused");

            Console.WriteLine("  [PASS] Double-line vs Single-line active window border switching verified");
        }

        private static void TestEvidenceWordWrappingWithHangingIndent()
        {
            int innerWidth = 68;
            string longEvidence = "TELEMETRY SOURCE: SIGINT Intercept Report & Radio Direction Finding Sector Analysis for Southwest Pacific Area";
            var wrapped = SplitPaneLayoutEngine.WrapLineWithHangingIndent(longEvidence, innerWidth);

            Assert(wrapped.Count >= 2, $"Long evidence line must wrap into at least 2 lines (Actual: {wrapped.Count})");
            Assert(wrapped[0].StartsWith("TELEMETRY SOURCE:"), "First line starts with prefix");
            Assert(wrapped[1].StartsWith("                  "), "Second line is indented to align under prefix text (18 spaces)");
            Assert(!wrapped[0].Contains("..."), "Wrapped line should not contain ellipsis");

            // Test box row rendering
            var engine = new SplitPaneLayoutEngine { BoxWidth = 72, TotalHeightRows = 30 };
            var state = new SplitPaneViewState();
            var item = SplitPaneItem.FromRawTelemetry(
                "Naval Strike Concentration",
                85,
                new[] {
                    "TELEMETRY SOURCE: SIGINT Intercept Report & Radio Direction Finding Sector Analysis for Southwest Pacific Area",
                    "LOCALIZED POSITION: Hex 152,84 (Solomon Islands / Coral Sea Tactical Vector Corridor)",
                    "OBSERVATION FREQUENCY: 4 distinct sightings recorded across campaign turns by naval task groups."
                });
            state.LoadItems(new[] { item }, "INTEL MANIFEST");
            string rendered = engine.Render(state);

            // Assert that the full long names are present across wrapped lines and not truncated with ellipsis
            Assert(rendered.Contains("Southwest Pacific Area"), "Wrapped continuation of TELEMETRY SOURCE must be rendered");
            Assert(rendered.Contains("Corridor)"), "Wrapped continuation of LOCALIZED POSITION must be rendered");
            Assert(rendered.Contains("naval task groups."), "Wrapped continuation of OBSERVATION FREQUENCY must be rendered");
            Assert(!rendered.Contains("v-alpha4-build"), "Split-Pane rendering must NOT contain redundant version string");

            Console.WriteLine("  [PASS] Evidence word-wrapping with hanging indent and no-ellipsis rendering verified");
        }
    }
}
