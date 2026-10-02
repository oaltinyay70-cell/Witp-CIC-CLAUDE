using System;
using System.Collections.Generic;
using System.Linq;

namespace ROCHEFORT6000.Core.Controller
{
    /// <summary>
    /// Identifies which pane currently holds keyboard navigation focus.
    /// </summary>
    public enum PaneFocus
    {
        Upper,
        Lower
    }

    /// <summary>
    /// Represents an individual reasoning topic and its associated granular evidence lines.
    /// </summary>
    public sealed class SplitPaneTopic
    {
        public string Title { get; }
        public IReadOnlyList<string> Lines { get; }

        public SplitPaneTopic(string title, IEnumerable<string> lines)
        {
            Title = title ?? string.Empty;
            Lines = lines?.ToList().AsReadOnly() ?? (IReadOnlyList<string>)Array.Empty<string>();
        }
    }

    /// <summary>
    /// Represents a primary actionable intelligence item displayed in the Upper Pane,
    /// complete with its confidence derivation and nested reasoning topics.
    /// </summary>
    public sealed class SplitPaneItem
    {
        public string Title { get; }
        public int ConfidencePct { get; }
        public IReadOnlyList<SplitPaneTopic> Topics { get; }
        public object? Tag { get; set; }

        public SplitPaneItem(string title, int confidencePct, IEnumerable<SplitPaneTopic> topics, object? tag = null)
        {
            Title = title ?? string.Empty;
            ConfidencePct = confidencePct;
            Topics = topics?.ToList().AsReadOnly() ?? (IReadOnlyList<SplitPaneTopic>)Array.Empty<SplitPaneTopic>();
            Tag = tag;
        }

        /// <summary>
        /// Deconstructs raw intelligence logs and telemetry into standardized reasoning topics.
        /// </summary>
        public static SplitPaneItem FromRawTelemetry(string title, int confidencePct, IEnumerable<string>? rawLines, object? tag = null)
        {
            var topic1 = new List<string>();
            var topic2 = new List<string>();
            var topic3 = new List<string>();

            if (rawLines != null)
            {
                foreach (var line in rawLines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.StartsWith("TELEMETRY SOURCE:") || line.StartsWith("LOCALIZED POSITION:") ||
                        line.StartsWith("OBSERVATION FREQUENCY:") || line.StartsWith("CHRONOLOGY / RECENCY:") ||
                        line.StartsWith("COORDINATES:") || line.StartsWith("EVENT IDENTIFIER:") ||
                        line.StartsWith("ANCHORAGE GEOMETRY:") || line.StartsWith("TRANSIT GEOMETRY:") ||
                        line.StartsWith("CLUSTER:") || line.StartsWith("CORRIDOR:") ||
                        line.StartsWith("ENDPOINTS:") || line.StartsWith("PROJECTED POSITION:") ||
                        line.StartsWith("TARGET ASSET:") || line.StartsWith("VESSEL:") ||
                        line.StartsWith("TASK FORCE:") || line.StartsWith("PILOT:") ||
                        line.StartsWith("CONTACT:") || line.StartsWith("TARGET:") ||
                        line.StartsWith("RAW EXCERPT:") || line.StartsWith("EVIDENCE:") ||
                        line.StartsWith("DATA SOURCE:") || line.StartsWith("DATA SOURCES:") ||
                        line.StartsWith("  - ") || line.StartsWith("INTERVAL:") ||
                        line.StartsWith("DATE:") || line.StartsWith("LOCATION:"))
                    {
                        topic1.Add(line);
                    }
                    else if (line.StartsWith("CONFIDENCE ANALYSIS:") || line.StartsWith("  Base Score:") ||
                             line.StartsWith("  Corroboration:") || line.StartsWith("  Time Decay:") ||
                             line.StartsWith("  Final Calculation:") || line.StartsWith("  Analyst Note:") ||
                             line.StartsWith("Base Score:") || line.StartsWith("Corroboration:") ||
                             line.StartsWith("Time Decay:") || line.StartsWith("Final Calculation:") ||
                             line.StartsWith("Analyst Note:") || line.StartsWith("MATHEMATICAL FORMULATION:") ||
                             line.StartsWith("MATHEMATICAL REASONING:") || line.StartsWith("TEMPORAL FACTOR:") ||
                             line.StartsWith("CONFIDENCE RATING:") || line.StartsWith("CONFIDENCE ASSESSMENT:") ||
                             line.StartsWith("CONFIDENCE LEVEL:") || line.StartsWith("ALGORITHMIC FORMULATION:") ||
                             line.StartsWith("MATHEMATICAL PROCESS:") || line.StartsWith("MATHEMATICAL ALGORITHM:") ||
                             line.StartsWith("ALGORITHM:") || line.StartsWith("FORMULATION:") ||
                             line.StartsWith("DISPLACEMENT:") || line.StartsWith("ADVANCE RATE:") ||
                             line.StartsWith("ROLLING") || line.StartsWith("TOTAL ENGAGEMENTS"))
                    {
                        topic3.Add(line);
                    }
                    else
                    {
                        topic2.Add(line);
                    }
                }
            }

            if (topic1.Count == 0)
            {
                topic1.Add("TELEMETRY SOURCE: Campaign Operational Sighting & Decrypted Telemetry");
                topic1.Add("LOCALIZED POSITION: Derived from regional campaign intelligence reports.");
            }
            if (topic3.Count == 0 && confidencePct > 0)
            {
                string tier = confidencePct >= 90 ? "VERIFIED" : confidencePct >= 70 ? "CONFIRMED" : confidencePct >= 40 ? "PROBABLE" : "UNCONFIRMED";
                topic3.Add($"CONFIDENCE LEVEL: {confidencePct}% [{tier}]");
                topic3.Add("ALGORITHMIC FORMULATION: Pure numerical Bayesian sensor correlation with recency decay.");
            }

            var topics = new List<SplitPaneTopic>();
            if (topic1.Count > 0) topics.Add(new SplitPaneTopic("TELEMETRY SOURCE & LOCALIZATION HISTORY", topic1));
            if (topic2.Count > 0) topics.Add(new SplitPaneTopic("TACTICAL ASSESSMENT & DOCTRINE GUIDANCE", topic2));
            if (topic3.Count > 0) topics.Add(new SplitPaneTopic("CONFIDENCE DERIVATION & MATHEMATICAL PROOF", topic3));

            return new SplitPaneItem(title, confidencePct, topics, tag);
        }
    }

    /// <summary>
    /// ViewState controller managing active pane focus, selected items, nested topic selection,
    /// and dynamic scrolling windows for the split-pane console UI.
    /// </summary>
    public sealed class SplitPaneViewState
    {
        private readonly List<SplitPaneItem> _items = new();
        private int _selectedItemIndex = 0;
        private int _selectedTopicIndex = 0;
        private PaneFocus _activePane = PaneFocus.Upper;

        public event Action? SelectionChanged;

        public string DomainTitle { get; set; } = "OPERATIONAL INTELLIGENCE MANIFEST";

        public PaneFocus ActivePane
        {
            get => _activePane;
            set
            {
                if (_activePane != value)
                {
                    _activePane = value;
                    SelectionChanged?.Invoke();
                }
            }
        }

        public IReadOnlyList<SplitPaneItem> Items => _items;

        public int SelectedItemIndex
        {
            get => _selectedItemIndex;
            set
            {
                int clamped = _items.Count == 0 ? 0 : Math.Clamp(value, 0, _items.Count - 1);
                if (_selectedItemIndex != clamped)
                {
                    _selectedItemIndex = clamped;
                    _selectedTopicIndex = 0; // Live update: reset nested topic index on primary item shift
                    SelectionChanged?.Invoke();
                }
            }
        }

        public int SelectedTopicIndex
        {
            get => _selectedTopicIndex;
            set
            {
                var curItem = SelectedItem;
                int maxTopics = curItem?.Topics.Count ?? 0;
                int clamped = maxTopics == 0 ? 0 : Math.Clamp(value, 0, maxTopics - 1);
                if (_selectedTopicIndex != clamped)
                {
                    _selectedTopicIndex = clamped;
                    SelectionChanged?.Invoke();
                }
            }
        }

        public SplitPaneItem? SelectedItem =>
            _items.Count > 0 && _selectedItemIndex >= 0 && _selectedItemIndex < _items.Count
                ? _items[_selectedItemIndex]
                : null;

        public SplitPaneTopic? SelectedTopic
        {
            get
            {
                var item = SelectedItem;
                if (item != null && item.Topics.Count > 0 && _selectedTopicIndex >= 0 && _selectedTopicIndex < item.Topics.Count)
                {
                    return item.Topics[_selectedTopicIndex];
                }
                return null;
            }
        }

        public bool HasItems => _items.Count > 0;

        public void LoadItems(IEnumerable<SplitPaneItem> items, string domainTitle)
        {
            _items.Clear();
            if (items != null)
            {
                _items.AddRange(items);
            }
            DomainTitle = domainTitle ?? "OPERATIONAL INTELLIGENCE MANIFEST";
            _selectedItemIndex = 0;
            _selectedTopicIndex = 0;
            _activePane = PaneFocus.Upper;
            SelectionChanged?.Invoke();
        }

        public void MoveUp()
        {
            if (_activePane == PaneFocus.Upper)
            {
                if (_selectedItemIndex > 0)
                {
                    _selectedItemIndex--;
                    _selectedTopicIndex = 0;
                    SelectionChanged?.Invoke();
                }
            }
            else // Lower pane
            {
                if (_selectedTopicIndex > 0)
                {
                    _selectedTopicIndex--;
                    SelectionChanged?.Invoke();
                }
            }
        }

        public void MoveDown()
        {
            if (_activePane == PaneFocus.Upper)
            {
                if (_selectedItemIndex < _items.Count - 1)
                {
                    _selectedItemIndex++;
                    _selectedTopicIndex = 0;
                    SelectionChanged?.Invoke();
                }
            }
            else // Lower pane
            {
                var curItem = SelectedItem;
                int maxTopics = curItem?.Topics.Count ?? 0;
                if (_selectedTopicIndex < maxTopics - 1)
                {
                    _selectedTopicIndex++;
                    SelectionChanged?.Invoke();
                }
            }
        }

        public void ToggleFocus()
        {
            _activePane = _activePane == PaneFocus.Upper ? PaneFocus.Lower : PaneFocus.Upper;
            SelectionChanged?.Invoke();
        }

        public void SwitchPane() => ToggleFocus();

        public void FocusUpper()
        {
            if (_activePane != PaneFocus.Upper)
            {
                _activePane = PaneFocus.Upper;
                SelectionChanged?.Invoke();
            }
        }

        public void FocusLower()
        {
            if (_activePane != PaneFocus.Lower)
            {
                _activePane = PaneFocus.Lower;
                SelectionChanged?.Invoke();
            }
        }
    }
}
