using System;
using System.Collections.Generic;
using System.Text;

namespace ROCHEFORT6000.Core.Controller
{
    /// <summary>
    /// Pure rendering and layout calculation engine for resolution-independent split-pane console UI.
    /// Handles Unicode box-drawing, zero-allocation text truncation, dynamic height budgets, and border integrity.
    /// </summary>
    public sealed class SplitPaneLayoutEngine
    {
        public int BoxWidth { get; set; } = 72;
        public int TotalHeightRows { get; set; } = 26;
        public float UpperRatio { get; set; } = 0.45f;
        public float LowerRatio { get; set; } = 0.55f;

        /// <summary>
        /// Factory creating optimal layout parameters for a given screen resolution and font size.
        /// Targets the 1024x640 minimum resolution floor with zero OS-level scrolling.
        /// </summary>
        public static SplitPaneLayoutEngine CreateForResolution(
            int screenWidth = 1024,
            int screenHeight = 640,
            double fontSize = 13.0,
            bool sidePanelVisible = true)
        {
            // At 820-1024 width with side panel visible (~30.6% star width), left pane has ~545-700px.
            // 72 columns fits safely on 820px, 1024px, and larger displays without any mid-box wrapping.
            int safeWidth = sidePanelVisible ? 72 : 80;

            // At 640 height, window chrome and status bar take ~86px, leaving ~554px.
            // Monospace line height at 13pt is ~17px: 554 / 17 ≈ 32 lines total.
            // 26 rows allocated to split-pane box structure, leaving room for top header and margins.
            int safeHeight = 26;

            return new SplitPaneLayoutEngine
            {
                BoxWidth = safeWidth,
                TotalHeightRows = safeHeight,
                UpperRatio = 0.45f,
                LowerRatio = 0.55f
            };
        }

        /// <summary>
        /// Zero-allocation text truncation utility using ReadOnlySpan and Span.
        /// Guarantees string is clipped cleanly with "..." and never overflows destination.
        /// </summary>
        public static void TruncateWithEllipsis(ReadOnlySpan<char> input, Span<char> destination, out int written)
        {
            if (input.Length <= destination.Length)
            {
                input.CopyTo(destination);
                written = input.Length;
                return;
            }

            if (destination.Length <= 3)
            {
                "...".AsSpan(0, destination.Length).CopyTo(destination);
                written = destination.Length;
                return;
            }

            int prefixLen = destination.Length - 3;
            input.Slice(0, prefixLen).CopyTo(destination);
            destination[prefixLen] = '.';
            destination[prefixLen + 1] = '.';
            destination[prefixLen + 2] = '.';
            written = destination.Length;
        }

        /// <summary>
        /// Wraps text into lines that do not exceed innerWidth.
        /// If the text has a label prefix ending in ": ", subsequent lines are indented to align under the text.
        /// </summary>
        public static List<string> WrapLineWithHangingIndent(string text, int innerWidth)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(text))
            {
                result.Add(string.Empty);
                return result;
            }

            if (text.Length <= innerWidth)
            {
                result.Add(text);
                return result;
            }

            // Determine hanging indent
            int colonIdx = text.IndexOf(": ", StringComparison.Ordinal);
            int indent = 4;
            if (colonIdx > 0 && colonIdx + 2 < innerWidth - 10)
            {
                indent = colonIdx + 2;
            }

            string rem = text;
            bool first = true;
            while (!string.IsNullOrEmpty(rem))
            {
                int curWidth = first ? innerWidth : (innerWidth - indent);
                if (curWidth <= 0) curWidth = innerWidth;

                if (rem.Length <= curWidth)
                {
                    result.Add((first ? "" : new string(' ', indent)) + rem);
                    break;
                }

                int breakPos = rem.LastIndexOf(' ', curWidth);
                if (breakPos <= 0)
                {
                    breakPos = curWidth;
                }

                string chunk = rem.Substring(0, Math.Min(breakPos, rem.Length));
                result.Add((first ? "" : new string(' ', indent)) + chunk);
                rem = breakPos < rem.Length ? rem.Substring(breakPos).TrimStart() : "";
                first = false;
            }

            return result;
        }

        /// <summary>
        /// Truncates string with ellipsis if exceeding maxVisibleWidth.
        /// </summary>
        public static string TruncateWithEllipsis(string? text, int maxVisibleWidth)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (text.Length <= maxVisibleWidth) return text;
            if (maxVisibleWidth <= 3) return new string('.', Math.Max(0, maxVisibleWidth));
            return string.Concat(text.AsSpan(0, maxVisibleWidth - 3), "...");
        }

        /// <summary>
        /// Pads or clamps string to exact width.
        /// </summary>
        public static string PadTo(string? text, int width)
        {
            text ??= string.Empty;
            if (text.Length >= width) return text.Substring(0, width);
            return text.PadRight(width);
        }

        public static string BuildTopBorder(string title, int boxWidth, bool isActive = false)
        {
            char cLeft = isActive ? '╔' : '┌';
            char cBar = isActive ? '═' : '─';
            char cRight = isActive ? '╗' : '┐';

            string prefix = $"{cLeft}{cBar} {title} ";
            int rem = boxWidth - prefix.Length - 1;
            if (rem < 0)
            {
                int maxTitle = Math.Max(4, boxWidth - 6);
                prefix = $"{cLeft}{cBar} {TruncateWithEllipsis(title, maxTitle)} ";
                rem = boxWidth - prefix.Length - 1;
            }
            return prefix + (rem > 0 ? new string(cBar, rem) : "") + cRight;
        }

        public static string BuildBottomBorder(int boxWidth, bool isActive = false)
        {
            char cLeft = isActive ? '╚' : '└';
            char cBar = isActive ? '═' : '─';
            char cRight = isActive ? '╝' : '┘';
            return cLeft + new string(cBar, Math.Max(0, boxWidth - 2)) + cRight;
        }

        public static string BuildDivider(int boxWidth, char c = '═') =>
            new string(c, Math.Max(0, boxWidth));

        public static string FormatBoxRow(string content, int innerWidth, bool isActive = false)
        {
            char cPipe = isActive ? '║' : '│';
            return $"{cPipe} {PadTo(TruncateWithEllipsis(content, innerWidth), innerWidth)} {cPipe}";
        }

        /// <summary>
        /// Renders the complete split-pane console UI buffer from current ViewState.
        /// </summary>
        public string Render(SplitPaneViewState state, string turnInfo = "")
        {
            var sb = new StringBuilder();
            int boxWidth = Math.Max(60, BoxWidth);
            int innerWidth = boxWidth - 4; // 2 chars on each side: "│ " and " │"

            // ── 1. Top Screen Header ──
            string rightHeader = string.IsNullOrEmpty(turnInfo)
                ? $"ITEMS: {state.Items.Count}"
                : $"{turnInfo} | ITEMS: {state.Items.Count}";

            string headerRule = new string('═', boxWidth);
            sb.AppendLine(headerRule);

            string titleStr = $"[ {TruncateWithEllipsis(state.DomainTitle, boxWidth - rightHeader.Length - 8)} ]";
            int headerSpaces = Math.Max(1, boxWidth - titleStr.Length - rightHeader.Length);
            sb.AppendLine(titleStr + new string(' ', headerSpaces) + rightHeader);
            sb.AppendLine();

            // ── 2. Height Budget Partitioning (45% Upper / 55% Lower) ──
            // Overhead: Upper top/bottom borders (2) + Divider (1) + Lower top/bottom borders (2) = 5 rows
            int totalBudget = Math.Max(16, TotalHeightRows);
            int availableContentRows = Math.Max(11, totalBudget - 5);

            int upperContentRows = Math.Max(3, (int)Math.Round(availableContentRows * UpperRatio));
            int lowerContentRows = Math.Max(6, availableContentRows - upperContentRows);

            // ── 3. Render Upper Pane: Main Items List ──
            bool isUpperActive = state.ActivePane == PaneFocus.Upper;
            bool isLowerActive = state.ActivePane == PaneFocus.Lower;

            string upperTitle = isUpperActive
                ? "UPPER PANE: MAIN ITEMS LIST [ACTIVE]"
                : "UPPER PANE: MAIN ITEMS LIST";
            sb.AppendLine(BuildTopBorder(upperTitle, boxWidth, isUpperActive));

            int totalItems = state.Items.Count;
            int sel = state.SelectedItemIndex;
            int startIdx = 0;
            if (sel >= upperContentRows)
            {
                startIdx = sel - upperContentRows + 1;
            }

            if (totalItems == 0)
            {
                sb.AppendLine(FormatBoxRow("NO ACTIONABLE INTELLIGENCE ITEMS RECORDED FOR THIS TURN RANGE.", innerWidth, isUpperActive));
                for (int i = 1; i < upperContentRows; i++)
                {
                    sb.AppendLine(FormatBoxRow("", innerWidth, isUpperActive));
                }
            }
            else
            {
                for (int i = 0; i < upperContentRows; i++)
                {
                    int itemIdx = startIdx + i;
                    if (itemIdx < totalItems)
                    {
                        var item = state.Items[itemIdx];
                        string pointer;
                        if (itemIdx == sel)
                        {
                            pointer = isUpperActive ? "* " : "> ";
                        }
                        else
                        {
                            pointer = "  ";
                        }

                        string itemText = pointer + item.Title;
                        sb.AppendLine(FormatBoxRow(itemText, innerWidth, isUpperActive));
                    }
                    else
                    {
                        sb.AppendLine(FormatBoxRow("", innerWidth, isUpperActive));
                    }
                }
            }

            sb.AppendLine(BuildBottomBorder(boxWidth, isUpperActive));

            // ── 4. Split Pane Divider ──
            sb.AppendLine(BuildDivider(boxWidth, '═'));

            // ── 5. Render Lower Pane: Selected Item Details & Reasoning ──
            string lowerTitle = isLowerActive
                ? "LOWER PANE: SELECTED ITEM DETAILS [ACTIVE]"
                : "LOWER PANE: SELECTED ITEM DETAILS";
            sb.AppendLine(BuildTopBorder(lowerTitle, boxWidth, isLowerActive));

            var curItem = state.SelectedItem;
            if (curItem == null)
            {
                sb.AppendLine(FormatBoxRow("NO ITEM SELECTED", innerWidth, isLowerActive));
                for (int i = 1; i < lowerContentRows; i++)
                {
                    sb.AppendLine(FormatBoxRow("", innerWidth, isLowerActive));
                }
            }
            else
            {
                int rowsUsed = 0;

                // Item Header Line
                string itemHeader = $"ITEM: {curItem.Title}";
                sb.AppendLine(FormatBoxRow(itemHeader, innerWidth, isLowerActive));
                rowsUsed++;

                // Confidence / Status Bar
                if (curItem.ConfidencePct > 0)
                {
                    int filled = Math.Min(10, Math.Max(0, curItem.ConfidencePct / 10));
                    string bar = "[" + new string('=', filled) + new string(' ', 10 - filled) + "]";
                    string tier = curItem.ConfidencePct >= 90 ? "VERIFIED"
                                : curItem.ConfidencePct >= 70 ? "CONFIRMED"
                                : curItem.ConfidencePct >= 40 ? "PROBABLE" : "UNCONFIRMED";
                    string confLine = $"CONFIDENCE LEVEL: {bar} {curItem.ConfidencePct}% [{tier}]";
                    sb.AppendLine(FormatBoxRow(confLine, innerWidth, isLowerActive));
                    rowsUsed++;
                }
                else
                {
                    sb.AppendLine(FormatBoxRow("", innerWidth, isLowerActive));
                    rowsUsed++;
                }

                // Reasoning Topics Header
                sb.AppendLine(FormatBoxRow("REASONING TOPICS & EVIDENCE:", innerWidth, isLowerActive));
                rowsUsed++;

                // Topic List Rows (up to 3 topics)
                int topicCount = curItem.Topics.Count;
                int displayedTopics = Math.Min(3, topicCount);
                for (int t = 0; t < displayedTopics; t++)
                {
                    string topicPointer;
                    if (t == state.SelectedTopicIndex)
                    {
                        topicPointer = isLowerActive ? "* " : "> ";
                    }
                    else
                    {
                        topicPointer = "  ";
                    }

                    string topicLine = $"{topicPointer}[{t + 1}] {curItem.Topics[t].Title}";
                    sb.AppendLine(FormatBoxRow(topicLine, innerWidth, isLowerActive));
                    rowsUsed++;
                }

                // Evidence Section
                var curTopic = state.SelectedTopic;
                if (curTopic != null && rowsUsed < lowerContentRows)
                {
                    string evHeader = $"--- EVIDENCE: {curTopic.Title} ---";
                    sb.AppendLine(FormatBoxRow(evHeader, innerWidth, isLowerActive));
                    rowsUsed++;

                    int remainingRows = lowerContentRows - rowsUsed;
                    if (remainingRows > 0)
                    {
                        var wrappedLines = new List<string>();
                        for (int e = 0; e < curTopic.Lines.Count; e++)
                        {
                            var wLines = WrapLineWithHangingIndent(curTopic.Lines[e], innerWidth);
                            wrappedLines.AddRange(wLines);
                        }

                        int totalWrapped = wrappedLines.Count;
                        if (totalWrapped <= remainingRows)
                        {
                            for (int w = 0; w < totalWrapped; w++)
                            {
                                sb.AppendLine(FormatBoxRow(wrappedLines[w], innerWidth, isLowerActive));
                                rowsUsed++;
                            }
                        }
                        else
                        {
                            // Display as many as possible, reserving last row for overflow indicator
                            int rowsToDisplay = Math.Max(0, remainingRows - 1);
                            for (int w = 0; w < rowsToDisplay; w++)
                            {
                                sb.AppendLine(FormatBoxRow(wrappedLines[w], innerWidth, isLowerActive));
                                rowsUsed++;
                            }

                            int more = totalWrapped - rowsToDisplay;
                            string moreMsg = $"  [+{more} MORE EVIDENCE LOG ENTRIES RECORDED IN HYPO BRIEF]";
                            sb.AppendLine(FormatBoxRow(moreMsg, innerWidth, isLowerActive));
                            rowsUsed++;
                        }
                    }
                }

                // Fill any remaining rows to preserve strict rectangular floor
                while (rowsUsed < lowerContentRows)
                {
                    sb.AppendLine(FormatBoxRow("", innerWidth, isLowerActive));
                    rowsUsed++;
                }
            }

            sb.AppendLine(BuildBottomBorder(boxWidth, isLowerActive));

            // ── 6. Navigation Hint (Strictly Outside & Below Lower Pane Border) ──
            sb.AppendLine();
            string hint = state.ActivePane == PaneFocus.Upper
                ? "  >> [UP/DOWN] Select Item | [TAB/ENTER] Reasonings | PRESS ESC TO RETURN"
                : "  >> [UP/DOWN] Select Topic | [ENTER] Zoom Reasoning | [TAB/ESC] Return to Items";
            sb.AppendLine(hint);

            return sb.ToString();
        }
    }
}
