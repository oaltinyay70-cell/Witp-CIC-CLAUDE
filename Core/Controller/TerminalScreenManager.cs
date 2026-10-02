using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ROCHEFORT6000.Core.Analytics;
using ROCHEFORT6000.Core.Common;
using ROCHEFORT6000.Core.Database;
using ROCHEFORT6000.Core.Ingestion;
using ROCHEFORT6000.Core.Vault;

namespace ROCHEFORT6000.Core.Controller
{
    // ──────────────────────────────────────────────────────────────────────────
    //  Screen state machine — full C++ navigation tree (alpha-rochefort / alpha3):
    //
    //  Boot  →  StartScreen (3 options: New / Continue / Exit)
    //               [N] NewCampaignWizard  (4 steps)
    //               [C] ContinueCampaign  (vault picker)
    //               [E] ExitConfirmModal
    //               → (after campaign loaded) → VaultIngestion → MasterMenu
    //  MasterMenu (5 items)
    //    [A] War Room  →  WarRoomPrimary (6 items)
    //                        [1] TbdDomain (Reserved Partition)
    //                        [2] TbdDomain (Reserved Partition)
    //                        [3] GraphicsMenu (Visual Telemetry Displays)
    //                        [4] TbdDomain (Reserved Partition)
    //                        [5] TbdDomain (Reserved Partition)
    //                        [6] Heuristics (Regional Power Shift) -> PopUp -> Reasoning
    //    [W] Wiki       →  WikiTopicMenu (3 root topics -> subtopics -> ArticleView)
    //    [F] FieldManual→  FieldManualMenu (Chapters -> Subsections -> ArticleView)
    //    [S] Settings   →  SettingsMenu (6 parameters + [1] AI Connection sub-menu)
    //    [E/ESC] Exit   →  ExitConfirmModal
    // ──────────────────────────────────────────────────────────────────────────
    public enum ScreenState
    {
        Boot,

        // ── Pre-Master-Menu flow ──────────────────────────────────────────────
        StartScreen,
        NewCampaignWizard,
        ContinueCampaign,
        VaultIngestion,

        // ── Master Menu ───────────────────────────────────────────────────────
        MasterMenu,

        // ── War Room ──────────────────────────────────────────────────────────
        WarRoomPrimary,
        TbdDomain,
        GraphicsMenu,
        GraphicsDisplayScreen,
        ReasoningModal,
        Heuristics,
        HeuristicsPopUp,

        // ── Wiki Module ───────────────────────────────────────────────────────
        WikiTopicMenu,
        WikiArticleView,

        // ── Field Manual Module ───────────────────────────────────────────────
        FieldManualMenu,
        FieldManualArticleView,

        // ── Settings Module ───────────────────────────────────────────────────
        SettingsMenu,
        AiConnectionMenu,

        // ── Exit Confirmation ─────────────────────────────────────────────────
        ExitConfirmModal,

        // ── Temporal Filter & Date Range Modal ───────────────────────────────
        TemporalFilterModal,
        CalendarPickerModal
    }

    public class TerminalScreenManager
    {
        // ── State ────────────────────────────────────────────────────────────
        private ScreenState _state = ScreenState.Boot;
        private ScreenState _prevState = ScreenState.WarRoomPrimary;
        private ScreenState _exitReturnState = ScreenState.StartScreen;

        // ── Start Screen ─────────────────────────────────────────────────────
        private int _startScreenSel = 0;           // 0=New 1=Continue 2=Exit

        // ── New Campaign Wizard (Allied only, v1) ────────────────────────────
        private int _wizardStep = 1;               // 1 = name, 2 = game folders, 3 = campaign .pws
        private const string _wizardSide = "Allied";
        private readonly List<string> _wizardDirs = new()
        {
            @"C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE",
            @"C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE\archive"
        };
        private string _wizardCampaignName = "";
        private string _wizardDirInput = "";
        private string _wizardNameInput = "";
        private bool _wizardAwaitingDirInput = false;
        private string _wizardError = "";
        private readonly List<(string Path, string Date, long Size)> _wizardPwsCandidates = new();
        private int _wizardPwsSel = 0;

        // ── Campaign vault (.wic) ────────────────────────────────────────────
        private CampaignVault? _vault;
        private VaultIngestResult? _lastIngest;

        // ── Continue Campaign ────────────────────────────────────────────────
        private readonly List<string> _vaultFiles = new();
        private int _continueSel = 0;
        private string _continueError = "";

        /// <summary>Folder holding the .wic campaign vaults. Tests point this at a temp folder.</summary>
        public string VaultDirectory { get; set; } = @"C:\WitP-CIC\Campaigns";

        // ── Master Menu Selection ────────────────────────────────────────────
        private int _masterMenuSel = 0;

        // ── War Room Selections ──────────────────────────────────────────────
        private int _warRoomSel = 0;
        private int _reasoningTopicSel = 0;
        private int _heuristicsSel = 0;
        private int _heuristicsPopUpSel = 0;
        private int _graphicsMenuSel = 0;
        private int _graphicsPageId = 1;

        // ── Temporal Intelligence Filter & Range Picker ──────────────────────
        public enum TemporalFilterMode
        {
            LatestTurn,
            PreviousTurn,
            Last7Days,
            Last15Days,
            Last30Days,
            AllCampaign,
            CustomDate,
            CustomRange
        }

        private TemporalFilterMode _filterMode = TemporalFilterMode.Last7Days;
        private DateTime _filterStartDate = DateTime.MinValue;
        private DateTime _filterEndDate = DateTime.MaxValue;
        private ScreenState _filterReturnState = ScreenState.WarRoomPrimary;
        private bool _filterTextEntryActive = false;
        private string _filterTextInput = "";
        private string _filterInputError = "";
        private int _filterTurnStepIdx = 0; // Index into sorted turns list (0 = newest)

        // ── Calendar Picker State ────────────────────────────────────────────
        private DateTime _calendarCursorDate = new DateTime(1941, 12, 7);
        private bool _calendarIsRangeMode = false;
        private int _calendarRangeStep = 1; // 1 = selecting start date, 2 = selecting end date
        private DateTime _calendarRangeStartDate = new DateTime(1941, 12, 7);
        private static readonly DateTime CalendarMinDate = new DateTime(1941, 12, 7); // Pearl Harbor
        private static readonly DateTime CalendarMaxDate = new DateTime(1946, 12, 31); // Max Campaign Date

        private List<string> GetSortedTurnDatesDescending()
        {
            return Manifest.UniqueDates.OrderByDescending(d => d).ToList();
        }

        private List<string> GetAvailableDateFilterList()
        {
            var dates = GetSortedTurnDatesDescending();
            var list = new List<string>();
            list.AddRange(dates);
            list.Add("ALL");
            return list;
        }

        public static bool TryParseYYMMDD(string raw, out DateTime dt)
        {
            dt = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(raw) || raw.Length != 6) return false;
            if (!int.TryParse(raw.Substring(0, 2), out int yy) ||
                !int.TryParse(raw.Substring(2, 2), out int mm) ||
                !int.TryParse(raw.Substring(4, 2), out int dd)) return false;
            int fullYear = yy < 30 ? 2000 + yy : 1900 + yy;
            try
            {
                dt = new DateTime(fullYear, mm, dd);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryParseFlexibleDate(string s, out DateTime dt)
        {
            dt = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim();

            // Try dd/MM/yy, MM/dd/yy or variations
            string[] formats = { "dd/MM/yy", "d/M/yy", "dd/MM/yyyy", "d/M/yyyy", "MM/dd/yy", "M/d/yy", "MM/dd/yyyy", "M/d/yyyy", "dd-MM-yy", "dd-MM-yyyy", "MM-dd-yy", "M-d-yy", "yyyy-MM-dd", "yyMMdd" };
            if (DateTime.TryParseExact(s, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
            {
                return true;
            }

            // Fallback general parse
            return DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt);
        }

        public static string FormatDDMMYY(DateTime dt)
        {
            if (dt == DateTime.MinValue || dt == DateTime.MaxValue) return "--/--/--";
            return dt.ToString("dd/MM/yy");
        }

        public static string FormatMMDDYY(DateTime dt) => FormatDDMMYY(dt);

        public void ResetFilterToLast7Days()
        {
            _filterMode = TemporalFilterMode.Last7Days;
            _filterTextEntryActive = false;
            _filterTextInput = "";
            _filterInputError = "";
            _filterTurnStepIdx = 0;

            var turns = GetSortedTurnDatesDescending();
            if (turns.Count > 0 && TryParseYYMMDD(turns[0], out DateTime latest))
            {
                _filterEndDate = latest;
                _filterStartDate = latest.AddDays(-7);
            }
            else
            {
                _filterStartDate = DateTime.MinValue;
                _filterEndDate = DateTime.MaxValue;
            }
        }

        public void ApplyFilterPreset(TemporalFilterMode mode)
        {
            var turns = GetSortedTurnDatesDescending();
            DateTime latest = DateTime.MinValue;
            if (turns.Count > 0)
                TryParseYYMMDD(turns[0], out latest);

            _filterMode = mode;
            _filterTurnStepIdx = 0;
            _filterTextEntryActive = false;
            _filterInputError = "";

            switch (mode)
            {
                case TemporalFilterMode.LatestTurn:
                    if (latest != DateTime.MinValue)
                    {
                        _filterStartDate = latest;
                        _filterEndDate = latest;
                    }
                    break;

                case TemporalFilterMode.PreviousTurn:
                    if (turns.Count > 1 && TryParseYYMMDD(turns[1], out DateTime prev))
                    {
                        _filterStartDate = prev;
                        _filterEndDate = prev;
                        _filterTurnStepIdx = 1;
                    }
                    else if (latest != DateTime.MinValue)
                    {
                        _filterStartDate = latest;
                        _filterEndDate = latest;
                    }
                    break;

                case TemporalFilterMode.Last7Days:
                    if (latest != DateTime.MinValue)
                    {
                        _filterEndDate = latest;
                        _filterStartDate = latest.AddDays(-7);
                    }
                    break;

                case TemporalFilterMode.Last15Days:
                    if (latest != DateTime.MinValue)
                    {
                        _filterEndDate = latest;
                        _filterStartDate = latest.AddDays(-15);
                    }
                    break;

                case TemporalFilterMode.Last30Days:
                    if (latest != DateTime.MinValue)
                    {
                        _filterEndDate = latest;
                        _filterStartDate = latest.AddDays(-30);
                    }
                    break;

                case TemporalFilterMode.AllCampaign:
                    _filterStartDate = DateTime.MinValue;
                    _filterEndDate = DateTime.MaxValue;
                    break;
            }
        }

        public void StepTurnDate(int direction) // -1 = step backwards (older), +1 = step forwards (newer)
        {
            var turns = GetSortedTurnDatesDescending();
            if (turns.Count == 0) return;

            // _filterTurnStepIdx: 0 is newest
            _filterTurnStepIdx = Math.Clamp(_filterTurnStepIdx - direction, 0, turns.Count - 1);
            if (TryParseYYMMDD(turns[_filterTurnStepIdx], out DateTime stepDt))
            {
                _filterMode = TemporalFilterMode.CustomDate;
                _filterStartDate = stepDt;
                _filterEndDate = stepDt;
            }
        }

        public bool IsDateInActiveFilter(string dateYYMMDD, List<string>? allDates = null)
        {
            if (_filterMode == TemporalFilterMode.AllCampaign) return true;

            // Check primary dateYYMMDD
            if (TryParseYYMMDD(dateYYMMDD, out DateTime dt))
            {
                if (dt >= _filterStartDate && dt <= _filterEndDate) return true;
            }

            // Also check constituent turn dates if present
            if (allDates != null && allDates.Count > 0)
            {
                foreach (var dStr in allDates)
                {
                    if (TryParseFlexibleDate(dStr, out DateTime altDt))
                    {
                        if (altDt >= _filterStartDate && altDt <= _filterEndDate) return true;
                    }
                }
            }

            return false;
        }

        public string GetActiveFilterDescription()
        {
            return _filterMode switch
            {
                TemporalFilterMode.LatestTurn   => $"{FormatDDMMYY(_filterEndDate)} (LATEST TURN)",
                TemporalFilterMode.PreviousTurn => $"{FormatDDMMYY(_filterEndDate)} (PREVIOUS TURN)",
                TemporalFilterMode.Last7Days    => $"{FormatDDMMYY(_filterStartDate)} -> {FormatDDMMYY(_filterEndDate)} (LAST 7 DAYS)",
                TemporalFilterMode.Last15Days   => $"{FormatDDMMYY(_filterStartDate)} -> {FormatDDMMYY(_filterEndDate)} (LAST 15 DAYS)",
                TemporalFilterMode.Last30Days   => $"{FormatDDMMYY(_filterStartDate)} -> {FormatDDMMYY(_filterEndDate)} (LAST 30 DAYS)",
                TemporalFilterMode.AllCampaign  => "ENTIRE CAMPAIGN (ALL TURNS)",
                TemporalFilterMode.CustomDate   => $"{FormatDDMMYY(_filterEndDate)} (SINGLE DATE)",
                TemporalFilterMode.CustomRange  => $"{FormatDDMMYY(_filterStartDate)} -> {FormatDDMMYY(_filterEndDate)} (CUSTOM RANGE)",
                _                               => "ALL DATES"
            };
        }

        private string RenderTemporalCommandStrip()
        {
            const int boxWidth = 63;
            const int innerWidth = boxWidth - 4; // 59
            var sb = new StringBuilder();

            // Top boundary
            sb.AppendLine("┌─ TEMPORAL FILTER CONTROL ───────────────────────────────────┐");

            // Line 1: Presets & Range Hotkey
            string p1 = (_filterMode == TemporalFilterMode.LatestTurn ? "*[1]LATEST" : "[1]LATEST");
            string p2 = (_filterMode == TemporalFilterMode.Last7Days ? "*[2]7D" : "[2]7D");
            string p3 = (_filterMode == TemporalFilterMode.Last15Days ? "*[3]15D" : "[3]15D");
            string p4 = (_filterMode == TemporalFilterMode.Last30Days ? "*[4]30D" : "[4]30D");
            string p5 = (_filterMode == TemporalFilterMode.AllCampaign ? "*[5]ALL" : "[5]ALL");
            string pR = (_filterMode is TemporalFilterMode.CustomRange or TemporalFilterMode.CustomDate ? "*[R]RANGE" : "[R]RANGE");

            string line1Content = $"{p1}  {p2}  {p3}  {p4}  {p5}  |  {pR}";
            sb.AppendLine($"│ {PadTo(line1Content, innerWidth)} │");

            // Line 2: Empty Spacer Line
            sb.AppendLine($"│ {new string(' ', innerWidth)} │");

            // Line 3: Active Range & Step Navigation (strictly NO mention of targets/contacts)
            string rangeDesc;
            switch (_filterMode)
            {
                case TemporalFilterMode.Last7Days:
                    rangeDesc = $"{_filterStartDate:MM/dd}->{_filterEndDate:MM/dd} (7D)";
                    break;
                case TemporalFilterMode.Last15Days:
                    rangeDesc = $"{_filterStartDate:MM/dd}->{_filterEndDate:MM/dd} (15D)";
                    break;
                case TemporalFilterMode.Last30Days:
                    rangeDesc = $"{_filterStartDate:MM/dd}->{_filterEndDate:MM/dd} (30D)";
                    break;
                case TemporalFilterMode.LatestTurn:
                    rangeDesc = $"{_filterEndDate:dd/MM/yy} (LATEST)";
                    break;
                case TemporalFilterMode.PreviousTurn:
                    rangeDesc = $"{_filterEndDate:dd/MM/yy} (PREVIOUS)";
                    break;
                case TemporalFilterMode.AllCampaign:
                    rangeDesc = "ALL CAMPAIGN (ALL TURNS)";
                    break;
                case TemporalFilterMode.CustomDate:
                    rangeDesc = $"{_filterEndDate:dd/MM/yy} (SINGLE DATE)";
                    break;
                case TemporalFilterMode.CustomRange:
                    rangeDesc = $"{_filterStartDate:MM/dd}->{_filterEndDate:MM/dd} (RANGE)";
                    break;
                default:
                    rangeDesc = $"{_filterStartDate:MM/dd}->{_filterEndDate:MM/dd}";
                    break;
            }

            string line3Content = $"ACTIVE: {rangeDesc}  << [ / ] >>";
            sb.AppendLine($"│ {PadTo(line3Content, innerWidth)} │");

            // Bottom boundary
            sb.AppendLine("└─────────────────────────────────────────────────────────────┘");

            return sb.ToString().TrimEnd();
        }

        // ── Pop-up / reasoning data ──────────────────────────────────────────
        private readonly List<string> _popUpBullets = new();
        private readonly List<List<string>> _popUpReasonings = new();
        private readonly List<int> _popUpConfidences = new();
        private string _popUpTitle = "";

        private string _reasoningBulletTitle = "";
        private int _reasoningConfPct = 0;
        private readonly List<ReasoningTopic> _reasoningTopics = new();

        // ── Wiki Module State ────────────────────────────────────────────────
        private WikiNode _wikiRoot = new();
        private readonly Stack<(string Header, List<WikiNode> Topics, bool IsRoot)> _wikiNavStack = new();
        private int _wikiMenuSel = 0;
        private WikiNode _currentWikiArticle = new();
        private int _wikiScrollOffset = 0;

        // ── Field Manual State ───────────────────────────────────────────────
        private List<ManualSectionNode> _manualTree = new();
        private readonly Stack<(string ParentHeader, string SubHeader, List<ManualSectionNode> Nodes, bool IsRoot)> _manualNavStack = new();
        private int _manualMenuSel = 0;
        private int _manualScrollOffset = 0;
        private const int ManualPageSize = 6;
        private ManualSectionNode _currentManualArticle = new();
        private int _manualArticleScrollOffset = 0;
        private List<string> _manualWrappedLines = new();

        // ── Settings Module State ────────────────────────────────────────────
        private int _settingsSel = 0;
        private int _aiConnSel = 0;
        private int _aiModelProvider = 1;          // 1=Local Heuristics, 2=Ollama, 3=Gemini, 4=Custom
        private string _sigintConfidence = "HIGH"; // "HIGH" | "PROBABLE"
        private int _strikeCruiserThreshold = 3;   // 2 - 6
        private int _strikeHexRadius = 10;         // 5, 10, 15, 20, 25
        private int _invasionSensitivity = 80;     // 60, 70, 80, 90
        private string _aiEndpointUrl = "in-process://heuristics";
        private string _aiModelName = "rochefort-heuristics-v1";
        private bool _aiConnected = true;

        public IngestionManifest Manifest { get; private set; } = new();
        public WarRoomAnalytics Analytics { get; private set; } = new();

        public ScreenState CurrentState => _state;
        public string CampaignSide => _wizardSide;
        public bool IsAllied => true;   // v1 is Allied-only

        public event Action<string, bool>? ScreenRenderRequested;
        public event Action? ShutdownRequested;

        // ── Ctor ─────────────────────────────────────────────────────────────
        public TerminalScreenManager()
        {
            InitializeWikiTree();
            InitializeFieldManual();   // data is loaded only when a campaign vault is created or continued
        }

        private static string FindProjectRootDir()
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            while (!string.IsNullOrEmpty(root) && !System.IO.File.Exists(System.IO.Path.Combine(root, "CANONICAL.txt")))
            {
                var parent = System.IO.Directory.GetParent(root);
                if (parent == null) break;
                root = parent.FullName;
            }
            if (string.IsNullOrEmpty(root) || !System.IO.File.Exists(System.IO.Path.Combine(root, "CANONICAL.txt")))
            {
                root = Directory.GetCurrentDirectory();
            }
            return root;
        }

        /// <summary>The one .pws that belongs to the active campaign (e.g. wpae021.pws). Null = none chosen yet.</summary>
        public string? CampaignPwsPath { get; set; }

        /// <summary>Tests only: replaces the default scan options (e.g. ScanOptions.TestMode).</summary>
        public ScanOptions? ScanOptionsOverride { get; set; }

        public void InitializeData(List<string>? dirs = null)
        {
            string root = FindProjectRootDir();
            var searchPaths = new List<string>();

            if (dirs != null)
            {
                foreach (var d in dirs)
                {
                    searchPaths.Add(d);
                    if (!Path.IsPathRooted(d))
                    {
                        searchPaths.Add(Path.Combine(root, d));
                        searchPaths.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, d));
                    }
                }
            }
            else
            {
                // Real campaigns scan only the game's SAVE folder (+ its archive). Never repo sample/golden data.
                searchPaths.AddRange(new[]
                {
                    @"C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE",
                    @"C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE\archive"
                });
            }

            Manifest = FileScanner.ScanDirectories(searchPaths, ScanOptionsOverride ?? new ScanOptions { CampaignPwsPath = CampaignPwsPath });
            Analytics.AnalyzeCampaign(Manifest);
        }

        // ── Key router ───────────────────────────────────────────────────────
        public void HandleKeyDown(System.Windows.Input.Key key)
        {
            switch (_state)
            {
                case ScreenState.Boot:
                    _state = ScreenState.StartScreen;
                    _startScreenSel = 0;
                    RenderCurrentScreen();
                    break;

                case ScreenState.StartScreen:
                    HandleStartScreen(key);
                    break;

                case ScreenState.NewCampaignWizard:
                    HandleNewCampaignWizard(key);
                    break;

                case ScreenState.ContinueCampaign:
                    HandleContinueCampaign(key);
                    break;

                case ScreenState.VaultIngestion:
                    _state = ScreenState.MasterMenu;
                    _masterMenuSel = 0;
                    RenderCurrentScreen();
                    break;

                case ScreenState.MasterMenu:
                    HandleMasterMenu(key);
                    break;

                case ScreenState.WarRoomPrimary:
                    HandleWarRoomPrimary(key);
                    break;

                case ScreenState.TbdDomain:
                    HandleTbdDomain(key);
                    break;

                case ScreenState.GraphicsMenu:
                    HandleGraphicsMenu(key);
                    break;

                case ScreenState.GraphicsDisplayScreen:
                    HandleGraphicsDisplayScreen(key);
                    break;

                case ScreenState.ReasoningModal:
                    HandleReasoningModal(key);
                    break;

                case ScreenState.Heuristics:
                    HandleHeuristics(key);
                    break;

                case ScreenState.HeuristicsPopUp:
                    HandleHeuristicsPopUp(key);
                    break;

                case ScreenState.WikiTopicMenu:
                    HandleWikiTopicMenu(key);
                    break;

                case ScreenState.WikiArticleView:
                    HandleWikiArticleView(key);
                    break;

                case ScreenState.FieldManualMenu:
                    HandleFieldManualMenu(key);
                    break;

                case ScreenState.FieldManualArticleView:
                    HandleFieldManualArticleView(key);
                    break;

                case ScreenState.SettingsMenu:
                    HandleSettingsMenu(key);
                    break;

                case ScreenState.AiConnectionMenu:
                    HandleAiConnectionMenu(key);
                    break;

                case ScreenState.ExitConfirmModal:
                    HandleExitConfirmModal(key);
                    break;

                case ScreenState.TemporalFilterModal:
                    HandleTemporalFilterModal(key);
                    break;

                case ScreenState.CalendarPickerModal:
                    HandleCalendarPickerModal(key);
                    break;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  KEY HANDLERS
        // ════════════════════════════════════════════════════════════════════

        // ── Start Screen ─────────────────────────────────────────────────────
        private void HandleStartScreen(System.Windows.Input.Key key)
        {
            switch (key)
            {
                case System.Windows.Input.Key.N:
                    LaunchNewCampaignWizard();
                    break;

                case System.Windows.Input.Key.C:
                    LaunchContinueCampaign();
                    break;

                case System.Windows.Input.Key.E:
                case System.Windows.Input.Key.Escape:
                    PromptExit(ScreenState.StartScreen);
                    break;

                case System.Windows.Input.Key.Up:
                    _startScreenSel = (_startScreenSel - 1 + 3) % 3;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.Tab:
                    _startScreenSel = (_startScreenSel + 1) % 3;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                    switch (_startScreenSel)
                    {
                        case 0: LaunchNewCampaignWizard(); break;
                        case 1: LaunchContinueCampaign(); break;
                        case 2: PromptExit(ScreenState.StartScreen); break;
                    }
                    break;
            }
        }

        private void LaunchNewCampaignWizard()
        {
            _wizardStep = 1;
            _wizardCampaignName = "";
            _wizardDirInput = "";
            _wizardNameInput = "";
            _wizardError = "";
            _wizardAwaitingDirInput = false;
            _wizardPwsCandidates.Clear();
            _wizardPwsSel = 0;
            _state = ScreenState.NewCampaignWizard;
            RenderCurrentScreen();
        }

        private void LaunchContinueCampaign()
        {
            _vaultFiles.Clear();
            _continueSel = 0;
            _continueError = "";
            try
            {
                Directory.CreateDirectory(VaultDirectory);
                foreach (var f in Directory.GetFiles(VaultDirectory, "*" + CampaignVault.Extension).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                    _vaultFiles.Add(System.IO.Path.GetFileName(f));
            }
            catch { }
            _state = ScreenState.ContinueCampaign;
            RenderCurrentScreen();
        }

        public bool IsAwaitingTextInput =>
            (_state == ScreenState.NewCampaignWizard && (_wizardStep == 1 || (_wizardStep == 2 && _wizardAwaitingDirInput)))
            || (_state == ScreenState.TemporalFilterModal && _filterTextEntryActive);

        // ── New Campaign Wizard (Allied only) ────────────────────────────────
        private void HandleNewCampaignWizard(System.Windows.Input.Key key)
        {
            switch (_wizardStep)
            {
                case 1: HandleWizardStepName(key); break;
                case 2: HandleWizardStepFolders(key); break;
                case 3: HandleWizardStepSave(key); break;
            }
        }

        private void HandleWizardStepName(System.Windows.Input.Key key)
        {
            if (key == System.Windows.Input.Key.Escape)
            {
                _state = ScreenState.StartScreen;
                RenderCurrentScreen();
                return;
            }
            if (key == System.Windows.Input.Key.Enter)
            {
                string name = _wizardNameInput.Trim();
                if (name.Length == 0)
                {
                    _wizardError = "Enter a campaign name.";
                }
                else if (File.Exists(CampaignVault.PathFor(VaultDirectory, name)))
                {
                    _wizardError = $"A campaign named '{name}' already exists. Pick another name, or use Continue.";
                }
                else
                {
                    _wizardCampaignName = name;
                    _wizardError = "";
                    _wizardStep = 2;
                }
                RenderCurrentScreen();
                return;
            }
            if (key == System.Windows.Input.Key.Back && _wizardNameInput.Length > 0)
            {
                _wizardNameInput = _wizardNameInput.Substring(0, _wizardNameInput.Length - 1);
                _wizardError = "";
                RenderCurrentScreen();
            }
        }

        private void HandleWizardStepFolders(System.Windows.Input.Key key)
        {
            if (_wizardAwaitingDirInput)
            {
                if (key == System.Windows.Input.Key.Escape)
                {
                    _wizardAwaitingDirInput = false;
                    _wizardDirInput = "";
                    RenderCurrentScreen();
                }
                else if (key == System.Windows.Input.Key.Enter)
                {
                    if (!string.IsNullOrWhiteSpace(_wizardDirInput)) _wizardDirs.Add(_wizardDirInput.Trim());
                    _wizardDirInput = "";
                    _wizardAwaitingDirInput = false;
                    RenderCurrentScreen();
                }
                else if (key == System.Windows.Input.Key.Back && _wizardDirInput.Length > 0)
                {
                    _wizardDirInput = _wizardDirInput.Substring(0, _wizardDirInput.Length - 1);
                    RenderCurrentScreen();
                }
                return;
            }

            switch (key)
            {
                case System.Windows.Input.Key.Enter:
                    RefreshPwsCandidates();
                    _wizardStep = 3;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.B:
                    _wizardAwaitingDirInput = true;
                    _wizardDirInput = "";
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Escape:
                    _wizardStep = 1;
                    RenderCurrentScreen();
                    break;
            }
        }

        private void HandleWizardStepSave(System.Windows.Input.Key key)
        {
            int total = _wizardPwsCandidates.Count;
            switch (key)
            {
                case System.Windows.Input.Key.Up when total > 0:
                    _wizardPwsSel = (_wizardPwsSel - 1 + total) % total;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down when total > 0:
                case System.Windows.Input.Key.Tab when total > 0:
                    _wizardPwsSel = (_wizardPwsSel + 1) % total;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                    CreateCampaignFromWizard(total > 0 ? _wizardPwsCandidates[_wizardPwsSel].Path : null);
                    break;

                case System.Windows.Input.Key.S:
                    CreateCampaignFromWizard(null);
                    break;

                case System.Windows.Input.Key.Escape:
                    _wizardStep = 2;
                    RenderCurrentScreen();
                    break;
            }
        }

        // ── Campaign vault orchestration ─────────────────────────────────────

        /// <summary>Tests only: replaces the wizard's game-folder list.</summary>
        public void SetWizardFolders(IEnumerable<string> dirs)
        {
            _wizardDirs.Clear();
            _wizardDirs.AddRange(dirs);
        }

        public string CampaignName => _vault?.Manifest.CampaignName ?? string.Empty;

        private List<string> ExistingWizardDirs()
        {
            string root = FindProjectRootDir();
            var result = new List<string>();
            foreach (var d in _wizardDirs)
            {
                var candidates = System.IO.Path.IsPathRooted(d)
                    ? new[] { d }
                    : new[] { System.IO.Path.Combine(root, d), System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, d) };
                foreach (var c in candidates)
                {
                    if (Directory.Exists(c) && !result.Contains(c, StringComparer.OrdinalIgnoreCase)) { result.Add(c); break; }
                }
            }
            return result;
        }

        private void RefreshPwsCandidates()
        {
            _wizardPwsCandidates.Clear();
            _wizardPwsSel = 0;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dir in ExistingWizardDirs())
            {
                try
                {
                    foreach (var f in Directory.GetFiles(dir, "*.pws").OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                    {
                        string full = System.IO.Path.GetFullPath(f);
                        if (!seen.Add(full)) continue;
                        _wizardPwsCandidates.Add((full, FileScanner.ExtractDateFromPws(full), new FileInfo(full).Length));
                    }
                }
                catch { }
            }
        }

        private ScanOptions MakeScanOptions(string? campaignPws)
            => ScanOptionsOverride ?? new ScanOptions { CampaignPwsPath = string.IsNullOrWhiteSpace(campaignPws) ? null : campaignPws };

        private static string CacheDirFor(string campaignName)
        {
            string stem = System.IO.Path.GetFileNameWithoutExtension(CampaignVault.PathFor("x", campaignName));
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WitP-CIC", "cache", stem);
        }

        private void CreateCampaignFromWizard(string? campaignPws)
        {
            try
            {
                Directory.CreateDirectory(VaultDirectory);
                var dirs = ExistingWizardDirs();
                string path = CampaignVault.PathFor(VaultDirectory, _wizardCampaignName);
                var vault = CampaignVault.Create(path, _wizardCampaignName, dirs, campaignPws);
                CampaignPwsPath = campaignPws;
                var scanned = FileScanner.ScanDirectories(dirs, MakeScanOptions(campaignPws));
                var result = vault.Ingest(scanned);
                vault.Save();
                FinishVaultLoad(vault, result);
            }
            catch (Exception ex)
            {
                _wizardError = $"Could not create campaign vault: {ex.Message}";
                _wizardStep = 1;
                RenderCurrentScreen();
            }
        }

        private void LoadExistingCampaign(string wicPath)
        {
            try
            {
                var vault = CampaignVault.Open(wicPath);
                string? pws = string.IsNullOrWhiteSpace(vault.Manifest.CampaignPws) ? null : vault.Manifest.CampaignPws;
                CampaignPwsPath = pws;
                var dirs = vault.Manifest.SourceFolders.Where(Directory.Exists).ToList();
                var result = vault.Ingest(FileScanner.ScanDirectories(dirs, MakeScanOptions(pws)));
                if (result.TotalNew > 0 || result.Superseded > 0) vault.Save();
                FinishVaultLoad(vault, result);
            }
            catch (Exception ex)
            {
                _continueError = $"Could not load {System.IO.Path.GetFileName(wicPath)}: {ex.Message}";
                RenderCurrentScreen();
            }
        }

        /// <summary>Purges memory and rebuilds all analytics from the vault contents.</summary>
        private void FinishVaultLoad(CampaignVault vault, VaultIngestResult result)
        {
            _vault = vault;
            _lastIngest = result;
            _wizardCampaignName = vault.Manifest.CampaignName;
            Manifest = vault.MaterializeManifest(CacheDirFor(vault.Manifest.CampaignName));
            Analytics.AnalyzeCampaign(Manifest, vault.LoadSnapshots());
            _state = ScreenState.VaultIngestion;
            RenderCurrentScreen();
        }

        // ── Text character input (called from MainWindow) ────────────────────
        public void HandleCharInput(char c)
        {
            if (c < 32 || c > 126) return;

            if (_state == ScreenState.NewCampaignWizard)
            {
                if (_wizardStep == 2 && _wizardAwaitingDirInput)
                {
                    if (_wizardDirInput.Length < 120)
                    {
                        _wizardDirInput += c;
                        RenderCurrentScreen();
                    }
                }
                else if (_wizardStep == 1)
                {
                    if (_wizardNameInput.Length < 40)
                    {
                        _wizardNameInput += c;
                        _wizardError = "";
                        RenderCurrentScreen();
                    }
                }
            }
            else if (_state == ScreenState.TemporalFilterModal && _filterTextEntryActive)
            {
                if (_filterTextInput.Length < 40)
                {
                    _filterTextInput += c;
                    _filterInputError = "";
                    RenderCurrentScreen();
                }
            }
        }

        // ── Continue Campaign ────────────────────────────────────────────────
        private void HandleContinueCampaign(System.Windows.Input.Key key)
        {
            int total = _vaultFiles.Count;

            if (total == 0)
            {
                if (key == System.Windows.Input.Key.Escape)
                { _state = ScreenState.StartScreen; RenderCurrentScreen(); }
                else if (key == System.Windows.Input.Key.N)
                { LaunchNewCampaignWizard(); }
                return;
            }

            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.StartScreen;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    _continueSel = (_continueSel - 1 + total) % total;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.Tab:
                    _continueSel = (_continueSel + 1) % total;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                    LoadExistingCampaign(System.IO.Path.Combine(VaultDirectory, _vaultFiles[_continueSel]));
                    break;
            }
        }

        // ── Master Menu ──────────────────────────────────────────────────────
        private void HandleMasterMenu(System.Windows.Input.Key key)
        {
            switch (key)
            {
                case System.Windows.Input.Key.A:
                    OpenWarRoom();
                    break;

                case System.Windows.Input.Key.W:
                    OpenWiki();
                    break;

                case System.Windows.Input.Key.F:
                    OpenFieldManual();
                    break;

                case System.Windows.Input.Key.S:
                    OpenSettings();
                    break;

                case System.Windows.Input.Key.E:
                case System.Windows.Input.Key.Escape:
                    PromptExit(ScreenState.MasterMenu);
                    break;

                case System.Windows.Input.Key.Up:
                    _masterMenuSel = (_masterMenuSel - 1 + 5) % 5;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.Tab:
                    _masterMenuSel = (_masterMenuSel + 1) % 5;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                    ExecuteMasterMenuSelection();
                    break;

                case System.Windows.Input.Key.D1: _masterMenuSel = 0; ExecuteMasterMenuSelection(); break;
                case System.Windows.Input.Key.D2: _masterMenuSel = 1; ExecuteMasterMenuSelection(); break;
                case System.Windows.Input.Key.D3: _masterMenuSel = 2; ExecuteMasterMenuSelection(); break;
                case System.Windows.Input.Key.D4: _masterMenuSel = 3; ExecuteMasterMenuSelection(); break;
                case System.Windows.Input.Key.D5: _masterMenuSel = 4; ExecuteMasterMenuSelection(); break;
            }
        }

        private void ExecuteMasterMenuSelection()
        {
            switch (_masterMenuSel)
            {
                case 0: OpenWarRoom(); break;
                case 1: OpenWiki(); break;
                case 2: OpenFieldManual(); break;
                case 3: OpenSettings(); break;
                case 4: PromptExit(ScreenState.MasterMenu); break;
            }
        }

        private void OpenWarRoom()
        {
            _state = ScreenState.WarRoomPrimary;
            _warRoomSel = 0;
            RenderCurrentScreen();
        }

        private void OpenWiki()
        {
            _wikiNavStack.Clear();
            _wikiNavStack.Push((_wikiRoot.Title, _wikiRoot.SubTopics, true));
            _wikiMenuSel = 0;
            _state = ScreenState.WikiTopicMenu;
            RenderCurrentScreen();
        }

        private void OpenFieldManual()
        {
            _manualNavStack.Clear();
            _manualNavStack.Push(("FIELD MANUAL: WITP:AE GAME MANUAL BROWSER", "W I T P : A E   M A N U A L", _manualTree, true));
            _manualMenuSel = 0;
            _manualScrollOffset = 0;
            _state = ScreenState.FieldManualMenu;
            RenderCurrentScreen();
        }

        private void OpenSettings()
        {
            _settingsSel = 0;
            _state = ScreenState.SettingsMenu;
            RenderCurrentScreen();
        }

        private void PromptExit(ScreenState returnTo)
        {
            _exitReturnState = returnTo;
            _state = ScreenState.ExitConfirmModal;
            RenderCurrentScreen();
        }

        // ── War Room Primary Screen ──────────────────────────────────────────
        private void HandleWarRoomPrimary(System.Windows.Input.Key key)
        {
            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.MasterMenu;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    _warRoomSel = (_warRoomSel - 1 + 6) % 6;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.Tab:
                    _warRoomSel = (_warRoomSel + 1) % 6;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                    OpenWarRoomSubMenu(_warRoomSel);
                    break;

                case System.Windows.Input.Key.D1: OpenWarRoomSubMenu(0); break;
                case System.Windows.Input.Key.D2: OpenWarRoomSubMenu(1); break;
                case System.Windows.Input.Key.D3: OpenWarRoomSubMenu(2); break;
                case System.Windows.Input.Key.D4: OpenWarRoomSubMenu(3); break;
                case System.Windows.Input.Key.D5: OpenWarRoomSubMenu(4); break;
                case System.Windows.Input.Key.D6: OpenWarRoomSubMenu(5); break;
            }
        }

        private void OpenWarRoomSubMenu(int idx)
        {
            _warRoomSel = idx;
            switch (idx)
            {
                case 0:
                    _state = ScreenState.TbdDomain;
                    break;
                case 1:
                    _state = ScreenState.TbdDomain;
                    break;
                case 2:
                    _state = ScreenState.GraphicsMenu;
                    _graphicsMenuSel = 0;
                    break;
                case 3:
                    _state = ScreenState.TbdDomain;
                    break;
                case 4:
                    _state = ScreenState.TbdDomain;
                    break;
                case 5:
                    _state = ScreenState.Heuristics;
                    _heuristicsSel = 0;
                    break;
            }
            RenderCurrentScreen();
        }

        // ── TBD Domain Screen ────────────────────────────────────────────────
        private void HandleTbdDomain(System.Windows.Input.Key key)
        {
            if (key is System.Windows.Input.Key.Escape or System.Windows.Input.Key.Enter)
            {
                _state = ScreenState.WarRoomPrimary;
                RenderCurrentScreen();
            }
        }

        // ── Graphics Sub-Menu Handlers ───────────────────────────────────────
        private void HandleGraphicsMenu(System.Windows.Input.Key key)
        {
            const int total = 10;
            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.WarRoomPrimary;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    _graphicsMenuSel = (_graphicsMenuSel - 1 + total) % total;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.Tab:
                    _graphicsMenuSel = (_graphicsMenuSel + 1) % total;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                    ExecuteGraphicsMenuSelection(_graphicsMenuSel);
                    break;

                case System.Windows.Input.Key.D1: ExecuteGraphicsMenuSelection(0); break;
                case System.Windows.Input.Key.D2: ExecuteGraphicsMenuSelection(1); break;
                case System.Windows.Input.Key.D3: ExecuteGraphicsMenuSelection(2); break;
                case System.Windows.Input.Key.D4: ExecuteGraphicsMenuSelection(3); break;
                case System.Windows.Input.Key.D5: ExecuteGraphicsMenuSelection(4); break;
                case System.Windows.Input.Key.D6: ExecuteGraphicsMenuSelection(5); break;
                case System.Windows.Input.Key.D7: ExecuteGraphicsMenuSelection(6); break;
                case System.Windows.Input.Key.D8: ExecuteGraphicsMenuSelection(7); break;
                case System.Windows.Input.Key.D9: ExecuteGraphicsMenuSelection(8); break;
                case System.Windows.Input.Key.D0: ExecuteGraphicsMenuSelection(9); break;
            }
        }

        private void ExecuteGraphicsMenuSelection(int idx)
        {
            _graphicsMenuSel = idx;
            _graphicsPageId = idx + 1;
            _state = ScreenState.GraphicsDisplayScreen;
            RenderCurrentScreen();
        }

        private void HandleGraphicsDisplayScreen(System.Windows.Input.Key key)
        {
            if (key is System.Windows.Input.Key.Escape or System.Windows.Input.Key.Enter)
            {
                _state = ScreenState.GraphicsMenu;
                RenderCurrentScreen();
            }
        }

        // ── Heuristics Screen ────────────────────────────────────────────────
        private void HandleHeuristics(System.Windows.Input.Key key)
        {
            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.WarRoomPrimary;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    _heuristicsSel = (_heuristicsSel - 1 + 6) % 6;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.Tab:
                    _heuristicsSel = (_heuristicsSel + 1) % 6;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                    OpenHeuristicsPopUp(_heuristicsSel);
                    break;

                case System.Windows.Input.Key.D1:
                    _heuristicsSel = 0;
                    OpenHeuristicsPopUp(0);
                    break;

                case System.Windows.Input.Key.D2:
                    _heuristicsSel = 1;
                    OpenHeuristicsPopUp(1);
                    break;

                case System.Windows.Input.Key.D3:
                    _heuristicsSel = 2;
                    OpenHeuristicsPopUp(2);
                    break;

                case System.Windows.Input.Key.D4:
                    _heuristicsSel = 3;
                    OpenHeuristicsPopUp(3);
                    break;

                case System.Windows.Input.Key.D5:
                    _heuristicsSel = 4;
                    OpenHeuristicsPopUp(4);
                    break;

                case System.Windows.Input.Key.D6:
                    _heuristicsSel = 5;
                    OpenHeuristicsPopUp(5);
                    break;
            }
        }

        private void OpenHeuristicsPopUp(int metricIdx)
        {
            _heuristicsSel = metricIdx;
            _heuristicsPopUpSel = 0;
            _popUpBullets.Clear();
            _popUpReasonings.Clear();
            _popUpConfidences.Clear();

            var h = Analytics.Heuristics;
            bool enoughData = h.HasSufficientTemporalData;

            switch (metricIdx)
            {
                case 0: // 1. Regional Power Shift
                    _popUpTitle = "HEURISTIC METRIC 1 : REGIONAL POWER SHIFT (7-DAY DELTA)";
                    if (!enoughData)
                    {
                        _popUpBullets.Add("NOT ENOUGH DATA // NEED MORE DATA POINTS - WAIT SOME TIME MORE");
                        _popUpReasonings.Add(new List<string> {
                            "MATHEMATICAL PROCESS: Shift = Count_today - Count_7days_ago",
                            "TEMPORAL LOGIC: Calculate a 7-day Delta. Positive = Build-up; Negative = Withdrawal.",
                            $"CURRENT DATASET: {h.CampaignDaysProcessed} daily report snapshots processed.",
                            "REQUIREMENT: Minimum 4 daily telemetry snapshots required before calculating temporal power shifts."
                        });
                        _popUpConfidences.Add(0);
                    }
                    foreach (var s in h.RegionalShifts)
                    {
                        string sign = s.ShiftDelta > 0 ? $"+{s.ShiftDelta}" : s.ShiftDelta.ToString();
                        int magnitude = Math.Min(8, Math.Abs(s.ShiftDelta) / 2);
                        string graphBar;
                        if (s.ShiftDelta > 0)
                        {
                            graphBar = "►" + new string('█', magnitude) + new string('░', 8 - magnitude);
                        }
                        else if (s.ShiftDelta < 0)
                        {
                            graphBar = "◄" + new string('█', magnitude) + new string('░', 8 - magnitude);
                        }
                        else
                        {
                            graphBar = "───░░░░░░";
                        }
                        _popUpBullets.Add($"{s.RegionName,-22} [{graphBar}] T:{s.CountToday,2} 7D:{s.Count7DaysAgo,2} D:{sign,3} [{s.Trend}]");
                        var reason = new List<string> {
                            $"TELEMETRY SOURCE: asigint & Operations Reports ({s.RegionName} Sector)",
                            $"REGION: {s.RegionName}",
                            $"MATHEMATICAL FORMULATION: Shift = Count_today ({s.CountToday}) - Count_7days_ago ({s.Count7DaysAgo}) = {sign}",
                            "TEMPORAL LOGIC: 7-day lookback delta. Positive = Build-up (+); Negative = Withdrawal (-); 0 = Stable.",
                            $"TACTICAL ASSESSMENT: Regional force posture evaluated as {s.Trend}.",
                            "EVIDENCE - ACTIVE SIGHTINGS / TRANSPORT LOADOUTS (TODAY):"
                        };
                        foreach (var v in s.TodayVessels.Take(10)) reason.Add($"  - [TODAY] {v}");
                        if (s.TodayVessels.Count == 0) reason.Add("  - No naval vessels localized in region today.");
                        if (s.BaselineVessels.Count > 0)
                        {
                            reason.Add("EVIDENCE - BASELINE DISPOSITIONS (7 DAYS AGO):");
                            foreach (var bv in s.BaselineVessels.Take(6)) reason.Add($"  - [BASELINE] {bv}");
                        }
                        _popUpReasonings.Add(reason);
                        _popUpConfidences.Add(0);
                    }
                    break;

                case 1: // 4. Minefield Mapping
                    _popUpTitle = "HEURISTIC METRIC 4 : MINEFIELD MAPPING & HIT CORRIDORS";
                    if (h.MineHitsChronological.Count == 0)
                    {
                        _popUpBullets.Add("NO CONFIRMED MINEFIELD ENCOUNTERS RECORDED IN COMBAT REPORTS.");
                        _popUpReasonings.Add(new List<string> {
                            "TELEMETRY SOURCE: combatreport files (Filter: 'encounters mine field at X,Y')",
                            "MATHEMATICAL FORMULATION: HashSet<(int X, int Y)> spatial deduplication with chronological ordering",
                            "TACTICAL ASSESSMENT: Zero minefield detonating actions detected in ingested reports."
                        });
                        _popUpConfidences.Add(0);
                    }
                    else
                    {
                        foreach (var m in h.MineHitsChronological)
                        {
                            string dFormatted = FileScanner.FormatYYMMDD(m.DateYYMMDD);
                            _popUpBullets.Add($"[{dFormatted}] Hex {m.HexX,3},{m.HexY,-3} ──■── ({m.LocationName}) : {m.Details}");
                            _popUpReasonings.Add(new List<string> {
                                "TELEMETRY SOURCE: Combat Report Minefield Engagement Log",
                                $"COORDINATES: Hex {m.HexX},{m.HexY} (Near {m.LocationName})",
                                $"DATE: {dFormatted}",
                                $"RAW EXCERPT: {m.RawLine}",
                                "MATHEMATICAL FORMULATION: Coordinate (X,Y) registered in HashSet unique spatial barrier set",
                                "TACTICAL ASSESSMENT: Hostile naval passage through Allied defensive mine barrier."
                            });
                            _popUpConfidences.Add(0);
                        }
                    }
                    break;

                case 2: // 5. Submarine Migration
                    _popUpTitle = "HEURISTIC METRIC 5 : SUBMARINE MIGRATION (DAILY CENTROIDS)";
                    if (h.SubmarineCentroids.Count == 0)
                    {
                        _popUpBullets.Add("NO SUBMARINE CONTACTS LOCALIZED FOR CENTROID CALCULATION.");
                        _popUpReasonings.Add(new List<string> {
                            "TELEMETRY SOURCE: asigint (SS) + Combat Events Submarine sightings",
                            "MATHEMATICAL FORMULATION: X_bar = (1/N) * sum(X_i), Y_bar = (1/N) * sum(Y_i)",
                            "TACTICAL ASSESSMENT: Zero SS sightings logged in telemetry."
                        });
                        _popUpConfidences.Add(0);
                    }
                    else
                    {
                        foreach (var c in h.SubmarineCentroids)
                        {
                            string dFormatted = FileScanner.FormatYYMMDD(c.DateYYMMDD);
                            int barCount = Math.Min(6, c.ContactCount);
                            string densityBar = new string('■', barCount) + new string('·', 6 - barCount);
                            _popUpBullets.Add($"[{dFormatted}] Centroid: ({c.CentroidX:F1},{c.CentroidY:F1}) [{densityBar}] {c.ContactCount,2} SS contacts");
                            var rList = new List<string> {
                                "TELEMETRY SOURCE: asigint & Operational Submarine Sightings Log",
                                $"DATE: {dFormatted}",
                                $"MATHEMATICAL FORMULATION: Centroid (X_bar, Y_bar) = ({c.CentroidX:F2}, {c.CentroidY:F2}) computed across N={c.ContactCount} contacts",
                                $"TACTICAL ASSESSMENT: Daily spatial center of mass for hostile submarine operations.",
                                "EVIDENCE SIGHTINGS & INTERCEPTS:"
                            };
                            foreach (var sc in c.Contacts.Take(8)) rList.Add($"  - [{sc.Source}] Hex {sc.HexX},{sc.HexY} near {sc.LocationName} : {sc.SubmarineName}");
                            _popUpReasonings.Add(rList);
                            _popUpConfidences.Add(0);
                        }
                        foreach (var v in h.SubmarineVectors)
                        {
                            int driftBarLen = Math.Min(5, (int)(v.DistanceHex / 5.0));
                            string driftBar = new string('=', driftBarLen) + "►";
                            _popUpBullets.Add($"VECTOR [{v.FromDate}->{v.ToDate}]: [{driftBar,-6}] {v.DistanceHex:F1}Hx Brg {v.BearingDeg:000}° ({v.CompassHeading}) dX:{v.DeltaX:+0.0;-0.0;0} dY:{v.DeltaY:+0.0;-0.0;0}");
                            _popUpReasonings.Add(new List<string> {
                                "TELEMETRY SOURCE: Derived from Consecutive Daily Submarine Centroid Coordinates",
                                $"INTERVAL: {v.FromDate} to {v.ToDate}",
                                $"MATHEMATICAL FORMULATION: Delta_X = X_today - X_prev ({v.DeltaX:+0.00;-0.00;0.00}), Delta_Y = Y_today - Y_prev ({v.DeltaY:+0.00;-0.00;0.00})",
                                $"DISPLACEMENT: Distance = sqrt(dX^2 + dY^2) = {v.DistanceHex:F1} hexes ({v.DistanceHex * 40.0:F0} NM)",
                                $"ADVANCE RATE: Strategic Bearing = atan2(dX, -dY) = {v.BearingDeg:000}° ({v.CompassHeading})",
                                $"TACTICAL ASSESSMENT: Submarine flotilla migrating towards strategic bearing {v.CompassHeading}."
                            });
                            _popUpConfidences.Add(0);
                        }
                    }
                    break;

                case 3: // 46. Intelligence Blindspots
                    _popUpTitle = "HEURISTIC METRIC 46 : INTELLIGENCE BLINDSPOTS (COMBAT VS SIGINT)";
                    if (h.IntelligenceBlindspots.Count == 0)
                    {
                        _popUpBullets.Add("NO COMBAT BLINDSPOTS DETECTED (ALL ACTION HEXES HAD PRIOR SURVEILLANCE).");
                        _popUpReasonings.Add(new List<string> {
                            "TELEMETRY SOURCE: combatreport vs asigint / sightings (3-day lookback)",
                            "MATHEMATICAL FORMULATION: Blindspot = 1 if Combat_Hex > 0 AND Sigint_Hex(t-3..t) == 0",
                            "TACTICAL ASSESSMENT: All tactical engagements were preceded by SIGINT or visual reconnaissance."
                        });
                        _popUpConfidences.Add(0);
                    }
                    else
                    {
                        foreach (var b in h.IntelligenceBlindspots)
                        {
                            string dFormatted = FileScanner.FormatYYMMDD(b.DateYYMMDD);
                            _popUpBullets.Add($"[{dFormatted}] Hex {b.HexX,3},{b.HexY,-3} [░░░░░░ GAP 0%] {b.LocationName} : Combat Blindspot");
                            _popUpReasonings.Add(new List<string> {
                                "TELEMETRY SOURCE: combatreport Engagement Records",
                                $"LOCATION: Hex {b.HexX},{b.HexY} ({b.LocationName})",
                                $"DATE: {dFormatted}",
                                $"RAW EXCERPT: {b.Description}",
                                "MATHEMATICAL FORMULATION: Engagement_Events = 1, Prior_SIGINT_Count(3 Days) = 0 -> Blindspot Confirmed",
                                "TACTICAL ASSESSMENT: Hostile force achieved tactical surprise on unmonitored approach vector."
                            });
                            _popUpConfidences.Add(0);
                        }
                    }
                    break;

                case 4: // 49. Axis of Advance
                    _popUpTitle = "HEURISTIC METRIC 49 : AXIS OF ADVANCE & STRATEGIC DRIFT";
                    if (h.FleetMeans.Count == 0)
                    {
                        _popUpBullets.Add("NO JAPANESE VESSEL CONCENTRATIONS DETECTED TO COMPUTE AXIS OF ADVANCE.");
                        _popUpReasonings.Add(new List<string> {
                            "TELEMETRY SOURCE: asigint (All vessels) + Operations reports",
                            "MATHEMATICAL FORMULATION: Mean_X = sum(X_i)/N, Mean_Y = sum(Y_i)/N; Drift = avg(last 3 step vectors)",
                            "TACTICAL ASSESSMENT: No Japanese vessels localized to compute mean movement."
                        });
                        _popUpConfidences.Add(0);
                    }
                    else
                    {
                        foreach (var m in h.FleetMeans)
                        {
                            string dFormatted = FileScanner.FormatYYMMDD(m.DateYYMMDD);
                            int densityBars = Math.Min(8, m.TotalVesselsCounted / 4);
                            string fleetBar = new string('█', densityBars) + new string('░', 8 - densityBars);
                            _popUpBullets.Add($"[{dFormatted}] Fleet Center: ({m.MeanX:F1},{m.MeanY:F1}) [{fleetBar}] {m.TotalVesselsCounted,2} vessels counted");
                            var r = new List<string> {
                                "TELEMETRY SOURCE: asigint & Sighting Telemetry Log",
                                $"DATE: {dFormatted}",
                                $"MATHEMATICAL FORMULATION: Mean Coordinates = ({m.MeanX:F2}, {m.MeanY:F2}) across N={m.TotalVesselsCounted} vessels",
                                "TACTICAL ASSESSMENT: Daily theater-wide center of enemy naval mass.",
                                "EVIDENCE CONTRIBUTING CONTACTS:"
                            };
                            foreach (var cu in m.ContributingUnits.Take(8)) r.Add($"  - {cu}");
                            _popUpReasonings.Add(r);
                            _popUpConfidences.Add(0);
                        }
                        if (h.StrategicDrift.DistanceHex > 0.001)
                        {
                            var sd = h.StrategicDrift;
                            int speedBars = Math.Min(8, (int)sd.DistanceHex);
                            string speedGraph = new string('█', speedBars) + new string('░', 8 - speedBars);
                            _popUpBullets.Add($"STRATEGIC DRIFT: [{speedGraph}] {sd.DistanceHex:F1} Hx/D Brg {sd.BearingDeg:000}° ({sd.CompassHeading}) dX:{sd.DeltaX:+0.0;-0.0;0} dY:{sd.DeltaY:+0.0;-0.0;0}");
                            _popUpReasonings.Add(new List<string> {
                                "TELEMETRY SOURCE: 3-Day Rolling Window of Mean Fleet Coordinates",
                                $"INTERVAL: {FileScanner.FormatYYMMDD(sd.FromDate)} to {FileScanner.FormatYYMMDD(sd.ToDate)}",
                                $"MATHEMATICAL FORMULATION: Drift_Vector = (1/3) * sum(V_step_k); dX={sd.DeltaX:+0.00;-0.00;0.00}, dY={sd.DeltaY:+0.00;-0.00;0.00}",
                                $"ADVANCE RATE: Advance Rate = {sd.DistanceHex:F1} hexes/day ({sd.DistanceHex * 40.0:F0} NM/day)",
                                $"DISPLACEMENT: Axis Bearing = atan2(avg_dX, -avg_dY) = {sd.BearingDeg:000}° ({sd.CompassHeading})",
                                "TACTICAL ASSESSMENT: Sustained offensive thrust along this strategic axis."
                            });
                            _popUpConfidences.Add(0);
                        }
                    }
                    break;

                case 5: // 50. Operational Tempo (OPTEMPO)
                    _popUpTitle = "HEURISTIC METRIC 50 : OPERATIONAL TEMPO (OPTEMPO)";
                    if (h.DailyOptempo.Count == 0)
                    {
                        _popUpBullets.Add("NO COMBAT REPORT ENGAGEMENT SEPARATORS DETECTED.");
                        _popUpReasonings.Add(new List<string> {
                            "TELEMETRY SOURCE: combatreport files (Regex: '^---.*')",
                            "MATHEMATICAL FORMULATION: OPTEMPO = sum(separators_3days) / 3.0",
                            "TACTICAL ASSESSMENT: Zero combat reports available."
                        });
                        _popUpConfidences.Add(0);
                    }
                    else
                    {
                        foreach (var opt in h.DailyOptempo)
                        {
                            string dFormatted = FileScanner.FormatYYMMDD(opt.DateYYMMDD);
                            int barLen = Math.Min(16, opt.SeparatorCount / 2);
                            string bar = new string('█', barLen) + new string('░', Math.Max(0, 16 - barLen));
                            _popUpBullets.Add($"[{dFormatted}] Engagements: {opt.SeparatorCount,2} [{bar}] 3D-Avg: {opt.Rolling3DayAvg:F1}");
                            var r = new List<string> {
                                "TELEMETRY SOURCE: combatreport files across daily turns",
                                $"OPERATIONAL DATE: {dFormatted}",
                                $"TOTAL ENGAGEMENTS: {opt.SeparatorCount} individual combat resolution engagements ('---')",
                                $"MATHEMATICAL FORMULATION: Rolling 3-Day Avg = sum(Separators_last_3_days) / 3.0 = {opt.Rolling3DayAvg:F2} engagements/day",
                                $"TACTICAL ASSESSMENT: Operational tempo evaluated at {opt.Rolling3DayAvg:F1} engagements per day.",
                                "EVIDENCE SAMPLES (ENGAGEMENT HEADERS):"
                            };
                            foreach (var s in opt.EngagementsSample) r.Add($"  - {s}");
                            if (opt.EngagementsSample.Count == 0) r.Add("  - Multiple combat operations resolved.");
                            _popUpReasonings.Add(r);
                            _popUpConfidences.Add(0);
                        }
                    }
                    break;
            }

            _state = ScreenState.HeuristicsPopUp;
            RenderCurrentScreen();
        }

        private void HandleHeuristicsPopUp(System.Windows.Input.Key key)
        {
            int total = _popUpBullets.Count;
            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.Heuristics;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    if (total > 0) { _heuristicsPopUpSel = Math.Max(0, _heuristicsPopUpSel - 1); RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Down:
                    if (total > 0) { _heuristicsPopUpSel = Math.Min(total - 1, _heuristicsPopUpSel + 1); RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Tab:
                    if (total > 0) { _heuristicsPopUpSel = (_heuristicsPopUpSel + 1) % total; RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Enter:
                    if (total > 0 && _heuristicsPopUpSel < total)
                        OpenReasoningModal(_popUpBullets[_heuristicsPopUpSel],
                                           _popUpReasonings[_heuristicsPopUpSel],
                                           0,
                                           ScreenState.HeuristicsPopUp);
                    break;
            }
        }

        private string BuildHeuristicsScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "WAR ROOM : HEURISTICS & LOGICAL BLUEPRINTS", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "MATHEMATICAL HEURISTIC DOMAINS // TEMPORAL NAVAL INTENTIONS");
            sb.AppendLine();

            var h = Analytics.Heuristics;
            bool enoughData = h.HasSufficientTemporalData;

            string[] titles =
            {
                "Regional Power Shift (7-Day Force Concentration Delta)",
                "Defensive Barrier & Minefield Hazard Mapping",
                "Submarine Migration & Daily ASW Threat Centroids",
                "Intelligence Blindspots & Combat Surveillance Gaps",
                "Axis of Advance, Task Force Kinematics & Strategic Drift",
                "Operational Tempo (OPTEMPO) & Combat Intensity Index"
            };

            string[] statuses = new string[6];

            // 1. Regional Power Shift
            if (!enoughData)
                statuses[0] = "NOT ENOUGH DATA // NEED MORE DATA POINTS - WAIT SOME TIME MORE";
            else
            {
                var topBuild = h.RegionalShifts.OrderByDescending(s => s.ShiftDelta).FirstOrDefault();
                statuses[0] = topBuild != null && topBuild.ShiftDelta != 0
                    ? $"Primary Shift: {topBuild.RegionName} ({topBuild.Trend})"
                    : "Regional power distribution stable across all theaters";
            }

            // 2. Minefield Mapping
            statuses[1] = h.MineHitsChronological.Count > 0
                ? $"{h.MineHitsChronological.Count} confirmed barrier encounter(s) tracked in combat telemetry"
                : "Defensive mine barriers intact; 0 hostile detonations logged";

            // 3. Submarine Migration
            statuses[2] = h.SubmarineCentroids.Count > 0
                ? $"{h.SubmarineCentroids.Count} daily SS center-of-mass centroid(s) mapped across theater"
                : "0 hostile submarine contact clusters localized for migration vectors";

            // 4. Intelligence Blindspots
            statuses[3] = h.IntelligenceBlindspots.Count > 0
                ? $"{h.IntelligenceBlindspots.Count} engagement hex(es) occurred with zero prior SIGINT surveillance"
                : "All tactical combat hexes preceded by corroborated reconnaissance";

            // 5. Axis of Advance
            statuses[4] = h.StrategicDrift.DistanceHex > 0.001
                ? $"Drift: {h.StrategicDrift.DistanceHex:F1} Hx/D Brg {h.StrategicDrift.BearingDeg:000}° ({h.StrategicDrift.CompassHeading})"
                : (h.FleetMeans.Count > 0 ? $"{h.FleetMeans.Count} fleet center-of-mass positions plotted" : "Enemy fleet center-of-mass vector stationary");

            // 6. OPTEMPO
            statuses[5] = h.DailyOptempo.Count > 0
                ? $"Current OPTEMPO: {h.DailyOptempo[^1].SeparatorCount} engagements (3D-Avg: {h.DailyOptempo[^1].Rolling3DayAvg:F1}/day)"
                : "Combat resolution rhythm baseline stable; awaiting daily battle logs";

            for (int i = 0; i < 6; i++)
            {
                string optText = $"[{i + 1}]  {titles[i]}";
                if (i == _heuristicsSel)
                    sb.AppendLine($"    * {optText}");
                else
                    sb.AppendLine($"      {optText}");
                sb.AppendLine($"        STATUS: {statuses[i]}");
                sb.AppendLine();
            }

            sb.AppendLine("    >> Press [1-6] or [ENTER] to open findings modal. Press [ESC] to return to War Room.");
            AppendFooter(sb, "[UP/DOWN] Select Metric", "[1-6/ENTER] Open Findings", "PRESS ESC TO RETURN");
            return sb.ToString();
        }

        private string BuildHeuristicsPopUp()
        {
            const int boxWidth = 72;
            const int innerWidth = 68;
            const int maxVisible = 11;

            var sb = new StringBuilder();
            string turnInfo = TurnInfo();
            string rightHeader = string.IsNullOrEmpty(turnInfo)
                ? $"ITEMS: {_popUpBullets.Count}"
                : $"{turnInfo} | ITEMS: {_popUpBullets.Count}";

            AppendHeader(sb, _popUpTitle, rightHeader, boxWidth);
            sb.AppendLine();
            AppendSectionHeader(sb, "TRAVERSE FINDINGS WITH ARROW KEYS | PRESS [ENTER] TO VERIFY RAW TELEMETRY", boxWidth);

            int total = _popUpBullets.Count;
            int sel = _heuristicsPopUpSel;
            int startIdx = sel >= maxVisible ? sel - maxVisible + 1 : 0;

            string shortTitle = _popUpTitle.Length > 90 ? _popUpTitle.Substring(0, 87) + "..." : _popUpTitle;
            string topPrefix = $"╔══ [ {shortTitle} ] ";
            int rem = boxWidth - topPrefix.Length - 1;
            sb.AppendLine(topPrefix + (rem > 0 ? new string('═', rem) : "") + "╗");

            for (int i = 0; i < maxVisible; i++)
            {
                int idx = startIdx + i;
                if (idx < total)
                {
                    string prefix = (idx == sel) ? "* " : "  ";
                    string content = prefix + _popUpBullets[idx];
                    content = PadTo(content.Length > innerWidth ? content.Substring(0, innerWidth) : content, innerWidth);
                    sb.AppendLine($"║ {content} ║");
                }
                else
                {
                    sb.AppendLine($"║ {new string(' ', innerWidth)} ║");
                }
            }

            sb.AppendLine("╚" + new string('═', boxWidth - 2) + "╝");
            sb.AppendLine();
            sb.AppendLine("    >> Press [ENTER] to view granular telemetry evidence & reasoning for this finding.");
            AppendFooterCustom(sb, boxWidth, "[UP/DOWN] Traverse Findings", "[ENTER] View Raw Evidence", "PRESS ESC TO RETURN");
            return sb.ToString();
        }

        // ── Reasoning Modal ──────────────────────────────────────────────────
        private void OpenReasoningModal(string bulletTitle, List<string> rawLines, int confPct, ScreenState returnTo)
        {
            _prevState = returnTo;
            _reasoningBulletTitle = bulletTitle;
            _reasoningConfPct = confPct;
            _reasoningTopicSel = 0;
            _reasoningTopics.Clear();

            var topic1 = new ReasoningTopic("TELEMETRY SOURCE & LOCALIZATION HISTORY");
            var topic2 = new ReasoningTopic("TACTICAL ASSESSMENT & DOCTRINE GUIDANCE");
            var topic3 = new ReasoningTopic("CONFIDENCE DERIVATION & MATHEMATICAL PROOF");

            foreach (var line in rawLines)
            {
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
                    topic1.Lines.Add(line);
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
                    topic3.Lines.Add(line);
                }
                else
                {
                    topic2.Lines.Add(line);
                }
            }

            if (topic1.Lines.Count == 0)
            {
                topic1.Lines.Add("TELEMETRY SOURCE: Campaign Operational Sighting & Decrypted Telemetry");
                topic1.Lines.Add("LOCALIZED POSITION: Derived from regional campaign intelligence reports.");
            }
            if (topic3.Lines.Count == 0 && confPct > 0)
            {
                string tier = confPct >= 90 ? "VERIFIED" : confPct >= 70 ? "CONFIRMED" : confPct >= 40 ? "PROBABLE" : "UNCONFIRMED";
                topic3.Lines.Add($"CONFIDENCE LEVEL: {confPct}% [{tier}]");
                topic3.Lines.Add("ALGORITHMIC FORMULATION: Pure numerical Bayesian sensor correlation with recency decay.");
            }

            if (topic1.Lines.Count > 0) _reasoningTopics.Add(topic1);
            if (topic2.Lines.Count > 0) _reasoningTopics.Add(topic2);
            if (topic3.Lines.Count > 0) _reasoningTopics.Add(topic3);

            _state = ScreenState.ReasoningModal;
            RenderCurrentScreen();
        }

        private void HandleReasoningModal(System.Windows.Input.Key key)
        {
            int total = _reasoningTopics.Count;
            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = _prevState;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    _reasoningTopicSel = Math.Max(0, _reasoningTopicSel - 1);
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.Tab:
                    _reasoningTopicSel = Math.Min(total - 1, _reasoningTopicSel + 1);
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                    RenderCurrentScreen();
                    break;
            }
        }

        // ── Wiki Module Handlers ─────────────────────────────────────────────
        private void HandleWikiTopicMenu(System.Windows.Input.Key key)
        {
            if (_wikiNavStack.Count == 0)
            {
                _state = ScreenState.MasterMenu;
                RenderCurrentScreen();
                return;
            }

            var current = _wikiNavStack.Peek();
            int total = current.Topics.Count;

            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _wikiNavStack.Pop();
                    if (_wikiNavStack.Count == 0)
                    {
                        _state = ScreenState.MasterMenu;
                    }
                    else
                    {
                        _wikiMenuSel = 0;
                    }
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    if (total > 0) { _wikiMenuSel = (_wikiMenuSel - 1 + total) % total; RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.Tab:
                    if (total > 0) { _wikiMenuSel = (_wikiMenuSel + 1) % total; RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Enter:
                    if (total > 0 && _wikiMenuSel < total)
                    {
                        var target = current.Topics[_wikiMenuSel];
                        if (target.IsLeaf)
                        {
                            _currentWikiArticle = target;
                            _wikiScrollOffset = 0;
                            _state = ScreenState.WikiArticleView;
                        }
                        else
                        {
                            _wikiNavStack.Push(("WIKI: " + target.Title, target.SubTopics, false));
                            _wikiMenuSel = 0;
                        }
                        RenderCurrentScreen();
                    }
                    break;
            }

            // Direct digit jump
            if (key >= System.Windows.Input.Key.D1 && key <= System.Windows.Input.Key.D9)
            {
                int targetIdx = (int)key - (int)System.Windows.Input.Key.D1;
                if (targetIdx < total)
                {
                    _wikiMenuSel = targetIdx;
                    var target = current.Topics[_wikiMenuSel];
                    if (target.IsLeaf)
                    {
                        _currentWikiArticle = target;
                        _wikiScrollOffset = 0;
                        _state = ScreenState.WikiArticleView;
                    }
                    else
                    {
                        _wikiNavStack.Push(("WIKI: " + target.Title, target.SubTopics, false));
                        _wikiMenuSel = 0;
                    }
                    RenderCurrentScreen();
                }
            }
        }

        private void HandleWikiArticleView(System.Windows.Input.Key key)
        {
            const int linesPerPage = 14;
            int totalLines = _currentWikiArticle.ContentLines.Count;

            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.WikiTopicMenu;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    if (_wikiScrollOffset > 0)
                    {
                        _wikiScrollOffset--;
                        RenderCurrentScreen();
                    }
                    break;

                case System.Windows.Input.Key.Down:
                    if (_wikiScrollOffset + linesPerPage < totalLines)
                    {
                        _wikiScrollOffset++;
                        RenderCurrentScreen();
                    }
                    break;

                case System.Windows.Input.Key.PageUp:
                    if (_wikiScrollOffset > 0)
                    {
                        _wikiScrollOffset = Math.Max(0, _wikiScrollOffset - linesPerPage);
                        RenderCurrentScreen();
                    }
                    break;

                case System.Windows.Input.Key.PageDown:
                    if (_wikiScrollOffset + linesPerPage < totalLines)
                    {
                        _wikiScrollOffset = Math.Min(totalLines - linesPerPage, _wikiScrollOffset + linesPerPage);
                        RenderCurrentScreen();
                    }
                    break;
            }
        }

        // ── Field Manual Handlers ────────────────────────────────────────────
        private void HandleFieldManualMenu(System.Windows.Input.Key key)
        {
            if (_manualNavStack.Count == 0)
            {
                _state = ScreenState.MasterMenu;
                RenderCurrentScreen();
                return;
            }

            var current = _manualNavStack.Peek();
            int total = current.Nodes.Count;

            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _manualNavStack.Pop();
                    if (_manualNavStack.Count == 0)
                    {
                        _state = ScreenState.MasterMenu;
                    }
                    else
                    {
                        _manualMenuSel = 0;
                        _manualScrollOffset = 0;
                    }
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    if (total > 0)
                    {
                        if (_manualMenuSel > 0)
                        {
                            _manualMenuSel--;
                            if (_manualMenuSel < _manualScrollOffset) _manualScrollOffset = _manualMenuSel;
                        }
                        else
                        {
                            _manualMenuSel = total - 1;
                            _manualScrollOffset = Math.Max(0, total - ManualPageSize);
                        }
                        RenderCurrentScreen();
                    }
                    break;

                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.Tab:
                    if (total > 0)
                    {
                        if (_manualMenuSel < total - 1)
                        {
                            _manualMenuSel++;
                            if (_manualMenuSel >= _manualScrollOffset + ManualPageSize) _manualScrollOffset++;
                        }
                        else
                        {
                            _manualMenuSel = 0;
                            _manualScrollOffset = 0;
                        }
                        RenderCurrentScreen();
                    }
                    break;

                case System.Windows.Input.Key.PageUp:
                    if (total > 0)
                    {
                        _manualMenuSel = Math.Max(0, _manualMenuSel - ManualPageSize);
                        _manualScrollOffset = Math.Max(0, _manualScrollOffset - ManualPageSize);
                        RenderCurrentScreen();
                    }
                    break;

                case System.Windows.Input.Key.PageDown:
                    if (total > 0)
                    {
                        _manualMenuSel = Math.Min(total - 1, _manualMenuSel + ManualPageSize);
                        _manualScrollOffset = Math.Min(Math.Max(0, total - ManualPageSize), _manualScrollOffset + ManualPageSize);
                        RenderCurrentScreen();
                    }
                    break;

                case System.Windows.Input.Key.Enter:
                    if (total > 0 && _manualMenuSel < total)
                    {
                        var targetNode = current.Nodes[_manualMenuSel];
                        if (targetNode.Subsections.Count > 0)
                        {
                            _manualNavStack.Push(("FIELD MANUAL: WITP:AE GAME MANUAL BROWSER", "[" + targetNode.Code + "] " + targetNode.Title, targetNode.Subsections, false));
                            _manualMenuSel = 0;
                            _manualScrollOffset = 0;
                        }
                        else
                        {
                            _currentManualArticle = targetNode;
                            _manualArticleScrollOffset = 0;
                            PrepareManualArticleLines();
                            _state = ScreenState.FieldManualArticleView;
                        }
                        RenderCurrentScreen();
                    }
                    break;
            }
        }

        private void PrepareManualArticleLines()
        {
            _manualWrappedLines.Clear();
            const int maxLineWidth = 74;

            foreach (var rawLine in _currentManualArticle.ContentLines)
            {
                if (string.IsNullOrEmpty(rawLine))
                {
                    _manualWrappedLines.Add("");
                    continue;
                }
                var wrapped = WrapText(rawLine, maxLineWidth);
                _manualWrappedLines.AddRange(wrapped);
            }

            if (_manualWrappedLines.Count == 0)
            {
                _manualWrappedLines.Add("(No detailed rule text recorded for this section.)");
            }
        }

        private void HandleFieldManualArticleView(System.Windows.Input.Key key)
        {
            const int linesPerPage = 14;
            int totalLines = _manualWrappedLines.Count;

            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.FieldManualMenu;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    if (_manualArticleScrollOffset > 0)
                    {
                        _manualArticleScrollOffset--;
                        RenderCurrentScreen();
                    }
                    break;

                case System.Windows.Input.Key.Down:
                    if (_manualArticleScrollOffset + linesPerPage < totalLines)
                    {
                        _manualArticleScrollOffset++;
                        RenderCurrentScreen();
                    }
                    break;

                case System.Windows.Input.Key.PageUp:
                    if (_manualArticleScrollOffset > 0)
                    {
                        _manualArticleScrollOffset = Math.Max(0, _manualArticleScrollOffset - linesPerPage);
                        RenderCurrentScreen();
                    }
                    break;

                case System.Windows.Input.Key.PageDown:
                    if (_manualArticleScrollOffset + linesPerPage < totalLines)
                    {
                        _manualArticleScrollOffset = Math.Min(totalLines - linesPerPage, _manualArticleScrollOffset + linesPerPage);
                        RenderCurrentScreen();
                    }
                    break;
            }
        }

        // ── Settings Handlers ────────────────────────────────────────────────
        private void HandleSettingsMenu(System.Windows.Input.Key key)
        {
            const int totalOptions = 6;
            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.MasterMenu;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.D1:
                    _aiConnSel = 0;
                    _state = ScreenState.AiConnectionMenu;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.S:
                    // Persist notification feedback
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    _settingsSel = (_settingsSel - 1 + totalOptions) % totalOptions;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.Tab:
                    _settingsSel = (_settingsSel + 1) % totalOptions;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                case System.Windows.Input.Key.Space:
                    if (_settingsSel == 0)
                    {
                        _aiConnSel = 0;
                        _state = ScreenState.AiConnectionMenu;
                    }
                    else if (_settingsSel == 1)
                    {
                        _sigintConfidence = _sigintConfidence == "HIGH" ? "PROBABLE" : "HIGH";
                    }
                    else if (_settingsSel == 2)
                    {
                        _strikeCruiserThreshold = _strikeCruiserThreshold >= 6 ? 2 : _strikeCruiserThreshold + 1;
                    }
                    else if (_settingsSel == 3)
                    {
                        _strikeHexRadius = _strikeHexRadius >= 25 ? 5 : _strikeHexRadius + 5;
                    }
                    else if (_settingsSel == 4)
                    {
                        _invasionSensitivity = _invasionSensitivity >= 90 ? 60 : _invasionSensitivity + 10;
                    }
                    else if (_settingsSel == 5)
                    {
                        // Save to vault
                    }
                    RenderCurrentScreen();
                    break;
            }
        }

        private void HandleAiConnectionMenu(System.Windows.Input.Key key)
        {
            const int totalOptions = 4;
            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.SettingsMenu;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    _aiConnSel = (_aiConnSel - 1 + totalOptions) % totalOptions;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.Tab:
                    _aiConnSel = (_aiConnSel + 1) % totalOptions;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.T:
                    _aiConnected = true;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                case System.Windows.Input.Key.Space:
                    if (_aiConnSel == 0)
                    {
                        _aiModelProvider = (_aiModelProvider % 4) + 1;
                        if (_aiModelProvider == 1) { _aiEndpointUrl = "in-process://heuristics"; _aiModelName = "rochefort-heuristics-v1"; }
                        else if (_aiModelProvider == 2) { _aiEndpointUrl = "http://localhost:11434"; _aiModelName = "mistral:latest"; }
                        else if (_aiModelProvider == 3) { _aiEndpointUrl = "https://generativelanguage.googleapis.com"; _aiModelName = "gemini-1.5-pro"; }
                        else { _aiEndpointUrl = "http://127.0.0.1:8080/v1"; _aiModelName = "custom-llm"; }
                    }
                    else if (_aiConnSel == 1)
                    {
                        _aiEndpointUrl = _aiEndpointUrl.Contains("11434") ? "http://127.0.0.1:8000/v1" : "http://localhost:11434";
                    }
                    else if (_aiConnSel == 2)
                    {
                        if (_aiModelName == "mistral:latest") _aiModelName = "llama3:latest";
                        else if (_aiModelName == "llama3:latest") _aiModelName = "gemini-1.5-pro";
                        else _aiModelName = "mistral:latest";
                    }
                    else if (_aiConnSel == 3)
                    {
                        _aiConnected = true;
                    }
                    RenderCurrentScreen();
                    break;
            }
        }

        // ── Exit Confirmation Modal ──────────────────────────────────────────
        private void HandleExitConfirmModal(System.Windows.Input.Key key)
        {
            if (key is System.Windows.Input.Key.Y)
            {
                // Purge memory and cleanly exit application
                ShutdownRequested?.Invoke();
                App.CleanProcessExit(0);
            }
            else if (key is System.Windows.Input.Key.N or System.Windows.Input.Key.Escape)
            {
                _state = _exitReturnState;
                RenderCurrentScreen();
            }
        }

        // ── Temporal Intelligence Filter & Range Modal ─────────────────────────
        private void HandleTemporalFilterModal(System.Windows.Input.Key key)
        {
            if (_filterTextEntryActive)
            {
                if (key == System.Windows.Input.Key.Escape)
                {
                    _filterTextEntryActive = false;
                    _filterTextInput = "";
                    _filterInputError = "";
                    RenderCurrentScreen();
                    return;
                }

                if (key == System.Windows.Input.Key.Back)
                {
                    if (_filterTextInput.Length > 0)
                    {
                        _filterTextInput = _filterTextInput.Substring(0, _filterTextInput.Length - 1);
                        _filterInputError = "";
                        RenderCurrentScreen();
                    }
                    return;
                }

                if (key == System.Windows.Input.Key.Enter)
                {
                    if (string.IsNullOrWhiteSpace(_filterTextInput))
                    {
                        _filterTextEntryActive = false;
                        _filterInputError = "";
                        RenderCurrentScreen();
                        return;
                    }

                    string raw = _filterTextInput.Trim().ToLowerInvariant();

                    // Check quick span shortcuts: -7d, -15d, -30d
                    if (raw == "-7d" || raw == "7d" || raw == "-7")
                    {
                        ApplyFilterPreset(TemporalFilterMode.Last7Days);
                        _state = _filterReturnState;
                        RenderCurrentScreen();
                        return;
                    }
                    if (raw == "-15d" || raw == "15d" || raw == "-15")
                    {
                        ApplyFilterPreset(TemporalFilterMode.Last15Days);
                        _state = _filterReturnState;
                        RenderCurrentScreen();
                        return;
                    }
                    if (raw == "-30d" || raw == "30d" || raw == "-30" || raw == "-1m" || raw == "1m")
                    {
                        ApplyFilterPreset(TemporalFilterMode.Last30Days);
                        _state = _filterReturnState;
                        RenderCurrentScreen();
                        return;
                    }

                    // Check range: split on " - " or " to " or "-"
                    string[] parts;
                    if (_filterTextInput.Contains(" - "))
                        parts = _filterTextInput.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                    else if (_filterTextInput.ToLowerInvariant().Contains(" to "))
                        parts = System.Text.RegularExpressions.Regex.Split(_filterTextInput, @"\s+to\s+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    else if (_filterTextInput.Contains('-') && _filterTextInput.Length > 8 && !_filterTextInput.StartsWith("-"))
                    {
                        int dashIdx = _filterTextInput.IndexOf('-', 4); // avoid potential leading dash
                        if (dashIdx > 0 && dashIdx < _filterTextInput.Length - 1)
                            parts = new[] { _filterTextInput.Substring(0, dashIdx), _filterTextInput.Substring(dashIdx + 1) };
                        else
                            parts = new[] { _filterTextInput };
                    }
                    else
                    {
                        parts = new[] { _filterTextInput };
                    }

                    if (parts.Length == 2)
                    {
                        if (TryParseFlexibleDate(parts[0], out DateTime dStart) &&
                            TryParseFlexibleDate(parts[1], out DateTime dEnd))
                        {
                            if (dStart > dEnd)
                            {
                                var temp = dStart;
                                dStart = dEnd;
                                dEnd = temp;
                            }
                            _filterMode = TemporalFilterMode.CustomRange;
                            _filterStartDate = dStart;
                            _filterEndDate = dEnd;
                            _filterTextEntryActive = false;
                            _filterTextInput = "";
                            _filterInputError = "";
                            _state = _filterReturnState;
                            RenderCurrentScreen();
                            return;
                        }
                        else
                        {
                            _filterInputError = "INVALID RANGE FORMAT. USE DD/MM/YY - DD/MM/YY.";
                            RenderCurrentScreen();
                            return;
                        }
                    }
                    else if (parts.Length == 1)
                    {
                        if (TryParseFlexibleDate(parts[0], out DateTime dSingle))
                        {
                            _filterMode = TemporalFilterMode.CustomDate;
                            _filterStartDate = dSingle;
                            _filterEndDate = dSingle;
                            _filterTextEntryActive = false;
                            _filterTextInput = "";
                            _filterInputError = "";
                            _state = _filterReturnState;
                            RenderCurrentScreen();
                            return;
                        }
                        else
                        {
                            _filterInputError = "UNRECOGNIZED DATE FORMAT. USE DD/MM/YY (E.G. 04/05/42).";
                            RenderCurrentScreen();
                            return;
                        }
                    }

                    _filterInputError = "INVALID INPUT FORMAT.";
                    RenderCurrentScreen();
                    return;
                }

                return;
            }

            // Navigation mode (not typing custom date)
            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = _filterReturnState;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                    _state = _filterReturnState;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.D1:
                    ApplyFilterPreset(TemporalFilterMode.LatestTurn);
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.D2:
                    ApplyFilterPreset(TemporalFilterMode.PreviousTurn);
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.D3:
                    ApplyFilterPreset(TemporalFilterMode.Last7Days);
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.D4:
                    ApplyFilterPreset(TemporalFilterMode.Last15Days);
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.D5:
                    ApplyFilterPreset(TemporalFilterMode.Last30Days);
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.D6:
                    ApplyFilterPreset(TemporalFilterMode.AllCampaign);
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.S:
                    OpenCalendarPickerModal(isRangeMode: false);
                    break;

                case System.Windows.Input.Key.C:
                    OpenCalendarPickerModal(isRangeMode: true);
                    break;

                case System.Windows.Input.Key.Up:
                case System.Windows.Input.Key.Left:
                case System.Windows.Input.Key.OemOpenBrackets:
                    StepTurnDate(-1);
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.Right:
                case System.Windows.Input.Key.OemCloseBrackets:
                    StepTurnDate(1);
                    RenderCurrentScreen();
                    break;
            }
        }

        private void OpenCalendarPickerModal(bool isRangeMode)
        {
            _calendarIsRangeMode = isRangeMode;
            _calendarRangeStep = 1;

            var turns = GetSortedTurnDatesDescending();
            DateTime candidate = CalendarMinDate;
            if (turns.Count > 0 && TryParseYYMMDD(turns[0], out DateTime latestTurn))
            {
                candidate = latestTurn;
            }
            if (_filterMode == TemporalFilterMode.CustomDate && _filterEndDate >= CalendarMinDate && _filterEndDate <= CalendarMaxDate)
            {
                candidate = _filterEndDate;
            }
            else if (_filterMode == TemporalFilterMode.CustomRange && _filterStartDate >= CalendarMinDate && _filterStartDate <= CalendarMaxDate)
            {
                candidate = _filterStartDate;
            }

            if (candidate < CalendarMinDate) candidate = CalendarMinDate;
            if (candidate > CalendarMaxDate) candidate = CalendarMaxDate;

            _calendarCursorDate = candidate;
            _calendarRangeStartDate = candidate;
            _state = ScreenState.CalendarPickerModal;
            RenderCurrentScreen();
        }

        private void HandleCalendarPickerModal(System.Windows.Input.Key key)
        {
            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.TemporalFilterModal;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Left:
                    _calendarCursorDate = _calendarCursorDate.AddDays(-1);
                    ClampCalendarDate();
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Right:
                    _calendarCursorDate = _calendarCursorDate.AddDays(1);
                    ClampCalendarDate();
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    _calendarCursorDate = _calendarCursorDate.AddDays(-7);
                    ClampCalendarDate();
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down:
                    _calendarCursorDate = _calendarCursorDate.AddDays(7);
                    ClampCalendarDate();
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.PageUp:
                case System.Windows.Input.Key.M:
                case System.Windows.Input.Key.OemOpenBrackets:
                    _calendarCursorDate = _calendarCursorDate.AddMonths(-1);
                    ClampCalendarDate();
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.PageDown:
                case System.Windows.Input.Key.N:
                case System.Windows.Input.Key.OemCloseBrackets:
                    _calendarCursorDate = _calendarCursorDate.AddMonths(1);
                    ClampCalendarDate();
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                    if (!_calendarIsRangeMode)
                    {
                        _filterMode = TemporalFilterMode.CustomDate;
                        _filterStartDate = _calendarCursorDate.Date;
                        _filterEndDate = _calendarCursorDate.Date;
                        _state = _filterReturnState;
                        RenderCurrentScreen();
                    }
                    else
                    {
                        if (_calendarRangeStep == 1)
                        {
                            _calendarRangeStartDate = _calendarCursorDate.Date;
                            _calendarRangeStep = 2;
                            RenderCurrentScreen();
                        }
                        else
                        {
                            DateTime dStart = _calendarRangeStartDate.Date;
                            DateTime dEnd = _calendarCursorDate.Date;
                            if (dStart > dEnd)
                            {
                                var tmp = dStart;
                                dStart = dEnd;
                                dEnd = tmp;
                            }
                            _filterMode = TemporalFilterMode.CustomRange;
                            _filterStartDate = dStart;
                            _filterEndDate = dEnd;
                            _state = _filterReturnState;
                            RenderCurrentScreen();
                        }
                    }
                    break;
            }
        }

        private void ClampCalendarDate()
        {
            if (_calendarCursorDate < CalendarMinDate) _calendarCursorDate = CalendarMinDate;
            if (_calendarCursorDate > CalendarMaxDate) _calendarCursorDate = CalendarMaxDate;
        }

        // ════════════════════════════════════════════════════════════════════
        //  SCREEN BUILDERS
        // ════════════════════════════════════════════════════════════════════

        public string BuildCurrentScreen()
        {
            return _state switch
            {
                ScreenState.Boot                   => BuildBootScreen(),
                ScreenState.StartScreen            => BuildStartScreen(),
                ScreenState.NewCampaignWizard      => BuildNewCampaignWizardScreen(),
                ScreenState.ContinueCampaign       => BuildContinueCampaignScreen(),
                ScreenState.VaultIngestion         => BuildVaultIngestionScreen(),
                ScreenState.MasterMenu             => BuildMasterMenuScreen(),
                ScreenState.WarRoomPrimary          => BuildWarRoomPrimaryScreen(),
                ScreenState.TbdDomain               => BuildTbdDomainScreen(),
                ScreenState.GraphicsMenu            => BuildGraphicsMenuScreen(),
                ScreenState.GraphicsDisplayScreen   => BuildGraphicsDisplayScreen(),
                ScreenState.ReasoningModal          => BuildReasoningModal(),
                ScreenState.Heuristics              => BuildHeuristicsScreen(),
                ScreenState.HeuristicsPopUp         => BuildHeuristicsPopUp(),
                ScreenState.WikiTopicMenu           => BuildWikiTopicMenuScreen(),
                ScreenState.WikiArticleView         => BuildWikiArticleViewScreen(),
                ScreenState.FieldManualMenu         => BuildFieldManualMenuScreen(),
                ScreenState.FieldManualArticleView  => BuildFieldManualArticleViewScreen(),
                ScreenState.SettingsMenu            => BuildSettingsMenuScreen(),
                ScreenState.AiConnectionMenu        => BuildAiConnectionMenuScreen(),
                ScreenState.ExitConfirmModal        => BuildExitConfirmModal(),
                ScreenState.TemporalFilterModal     => BuildTemporalFilterModalScreen(),
                ScreenState.CalendarPickerModal     => BuildCalendarPickerModalScreen(),
                _                                   => BuildStartScreen()
            };
        }

        public void RenderCurrentScreen()
        {
            string buffer = BuildCurrentScreen();
            bool isInstant = _state is ScreenState.ReasoningModal
                                    or ScreenState.VaultIngestion
                                    or ScreenState.ExitConfirmModal
                                    or ScreenState.TemporalFilterModal
                                    or ScreenState.CalendarPickerModal;
            ScreenRenderRequested?.Invoke(buffer, isInstant);
        }

        public string GetRochefortComment()
        {
            return _state switch
            {
                ScreenState.ReasoningModal   => BuildReasoningAdvisorComment(),
                ScreenState.HeuristicsPopUp  => BuildHeuristicsAdvisorComment(),
                _                            => string.Empty
            };
        }

        private string BuildReasoningAdvisorComment()
        {
            var sb = new StringBuilder();
            if (IsAllied)
            {
                sb.AppendLine("CMDR. ROCHEFORT // HYPO CRYPTANALYTIC ASSESSMENT");
                sb.AppendLine("────────────────────────────────────────────────");
                sb.AppendLine($"SUBJECT: {_reasoningBulletTitle}");
                if (_reasoningConfPct > 0)
                {
                    string tier = _reasoningConfPct >= 90 ? "HIGH CONFIDENCE / VERIFIED" :
                                  _reasoningConfPct >= 70 ? "PROBABLE / CORROBORATED" :
                                  _reasoningConfPct >= 40 ? "PLAUSIBLE / UNCONFIRMED" : "LOW / NOISE";
                    sb.AppendLine($"CONFIDENCE: {_reasoningConfPct}% [{tier}]");
                }
                sb.AppendLine();

                if (_reasoningTopics.Count > 0 && _reasoningTopicSel < _reasoningTopics.Count)
                {
                    var topic = _reasoningTopics[_reasoningTopicSel];
                    sb.AppendLine($"FOCUS: {topic.Title}");
                    sb.AppendLine();

                    if (topic.Title.Contains("TELEMETRY"))
                    {
                        sb.AppendLine("HYPO COMBAT INTEL NOTES:");
                        sb.AppendLine("Radio traffic intercepts and coastal watcher reports confirm coordinated Imperial Japanese movement.");
                        var sampleLines = topic.Lines.Take(5).ToList();
                        foreach (var line in sampleLines)
                        {
                            sb.AppendLine($" • {line.Trim()}");
                        }
                        sb.AppendLine();
                        sb.AppendLine("TACTICAL INFERENCE:");
                        sb.AppendLine("Do not treat this contact in isolation; look for tender vessels and tanker escorts trailing within 120 NM.");
                    }
                    else if (topic.Title.Contains("TACTICAL") || topic.Title.Contains("DOCTRINE"))
                    {
                        sb.AppendLine("OPERATIONAL DOCTRINE ANALYSIS:");
                        sb.AppendLine("The enemy adheres strictly to doctrine schedules once underway. Fleet assets will prioritize land-based air cover before committing surface combatants.");
                        sb.AppendLine();
                        sb.AppendLine("RECOMMENDED REACTION:");
                        sb.AppendLine("Position fleet submarines along the projected chokepoint axes. Avoid surface engagement inside their land-based Betty/Nell search radius.");
                    }
                    else
                    {
                        sb.AppendLine("MATHEMATICAL & SENSOR EVALUATION:");
                        sb.AppendLine("Bayesian triangulation correlates multi-source signals. Sighting recency decay ensures phantom contacts fall below threshold.");
                        sb.AppendLine();
                        sb.AppendLine("CRYPTANALYTIC REMARK:");
                        sb.AppendLine("Even partial JN-25 decrypts corroborate this vector. Maintain continuous visual reconnaissance over adjacent hexes.");
                    }
                }
                else
                {
                    sb.AppendLine("Awaiting topic selection for detailed signal decryption breakdown.");
                }
            }
            else
            {
                // Vice Admiral Matome Ugaki (Combined Fleet Chief of Staff)
                sb.AppendLine("V.ADM. M. UGAKI // COMBINED FLEET CHIEF OF STAFF");
                sb.AppendLine("────────────────────────────────────────────────");
                sb.AppendLine($"SUBJECT: {_reasoningBulletTitle}");
                if (_reasoningConfPct > 0)
                {
                    string tier = _reasoningConfPct >= 90 ? "CERTAIN / IMPERIAL NAVY CONFIRMED" :
                                  _reasoningConfPct >= 70 ? "HIGHLY PROBABLE / AIR RECON VERIFIED" :
                                  _reasoningConfPct >= 40 ? "PLAUSIBLE SIGHTING" : "UNCONFIRMED CONTACT";
                    sb.AppendLine($"ASSESSMENT CONFIDENCE: {_reasoningConfPct}% [{tier}]");
                }
                sb.AppendLine();

                if (_reasoningTopics.Count > 0 && _reasoningTopicSel < _reasoningTopics.Count)
                {
                    var topic = _reasoningTopics[_reasoningTopicSel];
                    sb.AppendLine($"SECTOR EMPHASIS: {topic.Title}");
                    sb.AppendLine();

                    if (topic.Title.Contains("TELEMETRY"))
                    {
                        sb.AppendLine("COMBINED FLEET SIGHTINGS LOG (SENKUN):");
                        sb.AppendLine("Reports from Type 97 flying boats and scouting submarines confirm enemy presence.");
                        var sampleLines = topic.Lines.Take(5).ToList();
                        foreach (var line in sampleLines)
                        {
                            sb.AppendLine($" • {line.Trim()}");
                        }
                        sb.AppendLine();
                        sb.AppendLine("STAFF ASSESSMENT:");
                        sb.AppendLine("Ensure our carrier division maintains sufficient CAP before closing range. Do not compromise our logistical timetable.");
                    }
                    else if (topic.Title.Contains("TACTICAL") || topic.Title.Contains("DOCTRINE"))
                    {
                        sb.AppendLine("IMPERIAL NAVY DOCTRINE DIRECTIVE:");
                        sb.AppendLine("Kantoku / Decisive Battle Doctrine dictates rapid night closing maneuvers supported by Long Lance torpedo spreads.");
                        sb.AppendLine();
                        sb.AppendLine("TACTICAL INSTRUCTION:");
                        sb.AppendLine("Coordinate air strikes at dawn from maximum distance. Surface forces shall sweep the perimeter under darkness.");
                    }
                    else
                    {
                        sb.AppendLine("WAR DIARY (SENKOROKU) ESTIMATE:");
                        sb.AppendLine("Operational success hinges on preserving fuel and pilot elite complements. Bayesian models track Allied counter-movements.");
                        sb.AppendLine();
                        sb.AppendLine("CHIEF OF STAFF DIRECTIVE:");
                        sb.AppendLine("Maintain strict radio silence (EMCON). Strike decisively when Allied carriers commit their aircraft.");
                    }
                }
                else
                {
                    sb.AppendLine("Awaiting tactical item inspection for Combined Fleet staff evaluation.");
                }
            }

            return sb.ToString();
        }

        private string BuildHeuristicsAdvisorComment()
        {
            var sb = new StringBuilder();
            if (IsAllied)
            {
                sb.AppendLine("CMDR. ROCHEFORT // THEATER HEURISTIC APPRAISAL");
                sb.AppendLine("────────────────────────────────────────────────");
                sb.AppendLine($"METRIC: {_popUpTitle}");
                sb.AppendLine();

                var h = Analytics.Heuristics;
                if (_heuristicsSel == 0) // Regional Power Shift
                {
                    if (_heuristicsPopUpSel < h.RegionalShifts.Count)
                    {
                        var shift = h.RegionalShifts[_heuristicsPopUpSel];
                        sb.AppendLine($"SECTOR: {shift.RegionName}");
                        sb.AppendLine($"7-DAY DISPOSITION: Today {shift.CountToday} vs Baseline {shift.Count7DaysAgo} (Delta: {shift.ShiftDelta:+0;-0;0})");
                        sb.AppendLine($"STATUS: {shift.Trend}");
                        sb.AppendLine();
                        if (shift.ShiftDelta > 0)
                        {
                            sb.AppendLine("ASSESSMENT:");
                            sb.AppendLine($"Concentrated force buildup detected in {shift.RegionName}. The presence of {shift.CountToday} assets indicates offensive preparation or convoy reinforcement. Prepare defensive air umbrella.");
                        }
                        else if (shift.ShiftDelta < 0)
                        {
                            sb.AppendLine("ASSESSMENT:");
                            sb.AppendLine($"Force dispersal observed in {shift.RegionName}. Enemy units have either completed their push, or diverted tonnage to adjacent theaters.");
                        }
                        else
                        {
                            sb.AppendLine("ASSESSMENT:");
                            sb.AppendLine($"Force posture in {shift.RegionName} is stable. Maintain vigilance along peripheral shipping channels.");
                        }
                    }
                }
                else if (_heuristicsSel == 1) // Minefields
                {
                    sb.AppendLine("MINE WARFARE REPORT:");
                    sb.AppendLine($"Identified {h.MineHitCoordinates.Count} hazardous mine clusters across theater waters.");
                    sb.AppendLine();
                    sb.AppendLine("CAUTION:");
                    sb.AppendLine("Ensure sweepers clear passage before sending capital assets into shallow port approaches.");
                }
                else if (_heuristicsSel == 2) // Submarine Migration
                {
                    sb.AppendLine("SUBMARINE PATROL CENTROIDS:");
                    sb.AppendLine($"Tracked {h.SubmarineCentroids.Count} daily centroid points.");
                    sb.AppendLine();
                    sb.AppendLine("TACTICAL GUIDANCE:");
                    sb.AppendLine("Enemy I-boats patrol along anticipated carrier transit lines. Reroute TF waypoints 60 NM south of direct routes.");
                }
                else if (_heuristicsSel == 3) // Blindspots
                {
                    sb.AppendLine("INTELLIGENCE BLINDSPOTS:");
                    sb.AppendLine($"Flagged {h.IntelligenceBlindspots.Count} unheralded engagements.");
                    sb.AppendLine();
                    sb.AppendLine("APPRAISAL:");
                    sb.AppendLine("Zero prior radio detection in these sectors signifies radar silence or bad weather corridors. Intensify Catalina recon sweeps.");
                }
                else if (_heuristicsSel == 4) // Axis of Advance
                {
                    sb.AppendLine("STRATEGIC AXIS OF ADVANCE:");
                    if (h.StrategicDrift != null && !string.IsNullOrEmpty(h.StrategicDrift.CompassHeading))
                    {
                        sb.AppendLine($"Overall Vector: Heading {h.StrategicDrift.CompassHeading} ({h.StrategicDrift.BearingDeg:F0}°) at {h.StrategicDrift.DistanceHex:F1} hexes/day.");
                    }
                    sb.AppendLine();
                    sb.AppendLine("HYPO ESTIMATE:");
                    sb.AppendLine("The enemy operational center of gravity shifts decisively along this vector. Reinforce destination outposts immediately.");
                }
                else // OPTEMPO
                {
                    sb.AppendLine("OPERATIONAL TEMPO (OPTEMPO):");
                    sb.AppendLine("Combat report activity rates parsed across campaign operational days.");
                    sb.AppendLine();
                    sb.AppendLine("ASSESSMENT:");
                    sb.AppendLine("High OPTEMPO peaks precede logistical replenishment pauses. Plan counterstrikes during their turnaround window.");
                }
            }
            else
            {
                sb.AppendLine("V.ADM. M. UGAKI // COMBINED FLEET THEATER APPRAISAL");
                sb.AppendLine("────────────────────────────────────────────────");
                sb.AppendLine($"METRIC: {_popUpTitle}");
                sb.AppendLine();

                var h = Analytics.Heuristics;
                if (_heuristicsSel == 0) // Regional Power Shift
                {
                    if (_heuristicsPopUpSel < h.RegionalShifts.Count)
                    {
                        var shift = h.RegionalShifts[_heuristicsPopUpSel];
                        sb.AppendLine($"STRATEGIC AREA: {shift.RegionName}");
                        sb.AppendLine($"7-DAY DISPOSITION: Present {shift.CountToday} vs Prior {shift.Count7DaysAgo} (Delta: {shift.ShiftDelta:+0;-0;0})");
                        sb.AppendLine($"TACTICAL STATE: {shift.Trend}");
                        sb.AppendLine();
                        if (shift.ShiftDelta > 0)
                        {
                            sb.AppendLine("UGAKI DIARY NOTE:");
                            sb.AppendLine($"Our offensive echelon in {shift.RegionName} concentrates {shift.CountToday} elements. Ensure landing transports are swiftly unloaded and withdrawn before Allied counter-sorties.");
                        }
                        else if (shift.ShiftDelta < 0)
                        {
                            sb.AppendLine("UGAKI DIARY NOTE:");
                            sb.AppendLine($"Tonnage reduction observed in {shift.RegionName}. Verify merchant shipping is returning along secured coastal corridors.");
                        }
                        else
                        {
                            sb.AppendLine("UGAKI DIARY NOTE:");
                            sb.AppendLine($"Operations in {shift.RegionName} proceed according to timetable. Maintain perimeter patrols.");
                        }
                    }
                }
                else if (_heuristicsSel == 1) // Minefields
                {
                    sb.AppendLine("MINE WARFARE REPORT:");
                    sb.AppendLine($"Known hazardous defensive fields in {h.MineHitCoordinates.Count} maritime choke points.");
                    sb.AppendLine();
                    sb.AppendLine("INSTRUCTION:");
                    sb.AppendLine("Maintain strict designated swept channels. Escorting destroyers must lead transport divisions.");
                }
                else if (_heuristicsSel == 2) // Submarine Migration
                {
                    sb.AppendLine("IMPERIAL SUBMARINE DISPOSITIONS:");
                    sb.AppendLine($"Submarine fleet centroids tracked across {h.SubmarineCentroids.Count} positions.");
                    sb.AppendLine();
                    sb.AppendLine("DIRECTIVE:");
                    sb.AppendLine("Deploy Sixth Fleet I-boats across enemy reinforcement bottlenecks to ambush Allied heavy units.");
                }
                else if (_heuristicsSel == 3) // Blindspots
                {
                    sb.AppendLine("RECONNAISSANCE GAPS:");
                    sb.AppendLine($"Identified {h.IntelligenceBlindspots.Count} sectors with sparse air coverage.");
                    sb.AppendLine();
                    sb.AppendLine("STAFF APPRAISAL:");
                    sb.AppendLine("Extend sea plane tender scouting lines. Do not let enemy carriers approach unspotted.");
                }
                else if (_heuristicsSel == 4) // Axis of Advance
                {
                    sb.AppendLine("IMPERIAL MAIN LINE OF ADVANCE:");
                    if (h.StrategicDrift != null && !string.IsNullOrEmpty(h.StrategicDrift.CompassHeading))
                    {
                        sb.AppendLine($"Advance Vector: Heading {h.StrategicDrift.CompassHeading} ({h.StrategicDrift.BearingDeg:F0}°) advancing {h.StrategicDrift.DistanceHex:F1} hexes/day.");
                    }
                    sb.AppendLine();
                    sb.AppendLine("STRATEGIC INTENT:");
                    sb.AppendLine("Maintain momentum along the southern resources zone. Anchor bases must be garrisoned promptly.");
                }
                else // OPTEMPO
                {
                    sb.AppendLine("OPERATIONAL TEMPO LOG:");
                    sb.AppendLine("Fleet combat tempo logged across campaign operation days.");
                    sb.AppendLine();
                    sb.AppendLine("OBSERVATION:");
                    sb.AppendLine("High combat intensity depletes aviation fuel and munitions. Schedule underway replenishment with tanker groups.");
                }
            }

            return sb.ToString();
        }

        // ── Width / layout helpers ────────────────────────────────────────────
        private const int W = 80;
        private static string Rule(char c = '═', int width = W) =>
            (c == '-' || c == '─') ? new string('─', width) : new string('═', width);
        private static string PadTo(string s, int width) =>
            s.Length >= width ? s.Substring(0, width) : s + new string(' ', width - s.Length);

        private string TurnInfo()
        {
            if (string.IsNullOrEmpty(Manifest.FormattedDate)) return "";
            string s = Manifest.FormattedDate;
            if (Manifest.DaysProcessed > 0) s += $" ({Manifest.DaysProcessed} Days Loaded)";
            return s;
        }

        private void AppendHeader(StringBuilder sb, string title, string right = "", int width = W)
        {
            sb.AppendLine(Rule('═', width));
            string titleStr = $"[ {title} ]";
            if (!string.IsNullOrEmpty(right))
            {
                string rightStr = right;
                if (!rightStr.Contains("Turn:") && !rightStr.Contains("COUNT:") &&
                    !rightStr.Contains("ITEMS:") && !rightStr.Contains("MATCHES:") &&
                    !rightStr.Contains("ALL ") && !rightStr.Contains("STRATEGIC"))
                    rightStr = "Turn: " + rightStr;
                int spaces = Math.Max(1, width - titleStr.Length - rightStr.Length);
                sb.AppendLine(titleStr + new string(' ', spaces) + rightStr);
            }
            else
            {
                sb.AppendLine(titleStr);
            }
            sb.AppendLine(Rule('═', width));
        }

        private void AppendSectionHeader(StringBuilder sb, string title, int width = W)
        {
            sb.AppendLine($"[ {title} ]");
            sb.AppendLine(Rule('─', width));
        }

        private void AppendFooter(StringBuilder sb, params string[] hotkeys)
        {
            sb.AppendLine();
            sb.AppendLine(Rule('═'));
            string left = string.Join("  │  ", hotkeys);
            sb.AppendLine(left);
        }

        private void AppendFooterCustom(StringBuilder sb, int width, params string[] hotkeys)
        {
            sb.AppendLine();
            sb.AppendLine(Rule('═', width));
            string left = string.Join("  │  ", hotkeys);
            sb.AppendLine(left);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  START SCREEN
        // ─────────────────────────────────────────────────────────────────────
        private string BuildStartScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "START");
            sb.AppendLine();
            AppendSectionHeader(sb, "C A M P A I G N   S E L E C T I O N   M A T R I X");

            string[] options =
            {
                "START NEW CAMPAIGN",
                "CONTINUE EXISTING CAMPAIGN",
                "TERMINATE INTERFACE SESSION"
            };
            string[] hotkeys = { "[N] ", "[C] ", "[E] " };
            string[] descriptions =
            {
                "Configure a new operational campaign from WITP:AE PBEM reports and initialize vault container.",
                "Resume intelligence monitoring and analytics from a saved .wic campaign vault.",
                "Securely purge campaign memory from RAM and shutdown interface session."
            };

            var leftRows = new List<(string Prefix, string Label, bool IsSelected)>();
            for (int i = 0; i < options.Length; i++)
            {
                leftRows.Add(("  " + hotkeys[i], options[i], _startScreenSel == i));
                leftRows.Add(("", "", false));
            }

            var rightBox = new List<string>
            {
                "┌── [ OPTION EXPLANATION ] ──────┐"
            };
            string desc = descriptions[_startScreenSel];
            const int contentWidth = 30;
            int startPos = 0;
            while (startPos < desc.Length)
            {
                int chunkLen = Math.Min(contentWidth, desc.Length - startPos);
                if (startPos + chunkLen < desc.Length)
                {
                    int spacePos = desc.LastIndexOf(' ', startPos + chunkLen - 1, chunkLen);
                    if (spacePos > startPos)
                    {
                        chunkLen = spacePos - startPos;
                    }
                }
                string line = desc.Substring(startPos, chunkLen).Trim();
                if (line.Length < contentWidth) line = line.PadRight(contentWidth);
                rightBox.Add("│ " + line + " │");
                startPos += chunkLen;
                while (startPos < desc.Length && desc[startPos] == ' ') startPos++;
            }
            rightBox.Add("└────────────────────────────────┘");

            int totalRows = Math.Max(leftRows.Count, rightBox.Count);
            for (int r = 0; r < totalRows; r++)
            {
                string leftStr;
                if (r < leftRows.Count)
                {
                    var item = leftRows[r];
                    if (string.IsNullOrEmpty(item.Prefix) && string.IsNullOrEmpty(item.Label))
                    {
                        leftStr = new string(' ', 42);
                    }
                    else
                    {
                        string marker = item.IsSelected ? "* " : "  ";
                        int remaining = 42 - item.Prefix.Length - marker.Length;
                        string lbl = item.Label;
                        if (lbl.Length < remaining)
                            lbl = lbl.PadRight(remaining);
                        leftStr = item.Prefix + marker + lbl;
                    }
                }
                else
                {
                    leftStr = new string(' ', 42);
                }

                string rightStr = r < rightBox.Count ? rightBox[r] : "";
                sb.AppendLine(leftStr + "  " + rightStr);
            }

            AppendFooter(sb,
                "[UP/DOWN] Navigate",
                "[ENTER] Select",
                "[N] New",
                "[C] Continue",
                "[E] Exit");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  NEW CAMPAIGN WIZARD
        // ─────────────────────────────────────────────────────────────────────
        private string BuildNewCampaignWizardScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "NEW CAMPAIGN WIZARD");
            sb.AppendLine();

            if (_wizardStep == 1)
            {
                AppendSectionHeader(sb, "STEP 1/3: CAMPAIGN NAME");
                sb.AppendLine("  Allied command. The campaign is stored as one vault file:");
                sb.AppendLine($"  {VaultDirectory}\\<name>{CampaignVault.Extension}\n");
                sb.AppendLine($"  Campaign Name > {_wizardNameInput}_\n");
                if (!string.IsNullOrEmpty(_wizardError)) sb.AppendLine($"  ! {_wizardError}\n");

                AppendFooter(sb,
                    "[ENTER] Next",
                    "[ESC] Return to Start");
            }
            else if (_wizardStep == 2)
            {
                AppendSectionHeader(sb, "STEP 2/3: GAME REPORTS & SAVE FOLDERS");
                sb.AppendLine("  Folders scanned for reports and the campaign save (Press [B] to add more):\n");

                for (int i = 0; i < _wizardDirs.Count; i++)
                {
                    string dir = _wizardDirs[i];
                    bool exists = Directory.Exists(dir) ||
                                  Directory.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir)) ||
                                  Directory.Exists(System.IO.Path.Combine(Directory.GetCurrentDirectory(), dir));
                    string status = exists ? "[EXISTS]" : "[NOT FOUND]";
                    sb.AppendLine($"  [{i + 1}] {dir,-60} {status}");
                }

                if (_wizardAwaitingDirInput)
                    sb.AppendLine($"\n  Enter Extra Directory Path > {_wizardDirInput}_");
                else
                    sb.AppendLine("\n  Press [ENTER] to continue or [B] to add another folder.");

                AppendFooter(sb,
                    "[ENTER] Next",
                    "[B] Add Extra Directory",
                    "[ESC] Previous Step");
            }
            else
            {
                AppendSectionHeader(sb, "STEP 3/3: CAMPAIGN SAVE FILE (.pws)");
                sb.AppendLine("  Pick the ONE .pws that belongs to this campaign - the game overwrites it each turn.");
                sb.AppendLine("  Every other .pws is ignored.\n");

                if (_wizardPwsCandidates.Count == 0)
                {
                    sb.AppendLine("  No .pws files found in the folders above.");
                    sb.AppendLine("  [ENTER] creates the campaign from the text reports only.\n");
                }
                else
                {
                    for (int i = 0; i < _wizardPwsCandidates.Count; i++)
                    {
                        var c = _wizardPwsCandidates[i];
                        string date = string.IsNullOrEmpty(c.Date) ? "??/??/??" : FileScanner.FormatYYMMDD(c.Date);
                        string mark = i == _wizardPwsSel ? "* [X]" : "  [ ]";
                        sb.AppendLine($"    {mark} {System.IO.Path.GetFileName(c.Path),-16} game date {date}   {c.Size / 1024.0 / 1024.0,5:F1} MB");
                    }
                    sb.AppendLine();
                }

                AppendFooter(sb,
                    "[UP/DOWN] Select",
                    "[ENTER] Create Campaign",
                    "[S] Reports Only (no save)",
                    "[ESC] Previous Step");
            }

            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CONTINUE CAMPAIGN SCREEN
        // ─────────────────────────────────────────────────────────────────────
        private string BuildContinueCampaignScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "CONTINUE CAMPAIGN");
            sb.AppendLine();
            AppendSectionHeader(sb, "A V A I L A B L E   C A M P A I G N   V A U L T S");

            if (_vaultFiles.Count == 0)
            {
                sb.AppendLine($"  NO SAVED CAMPAIGN VAULTS (.wic) FOUND IN {VaultDirectory}\n");
                sb.AppendLine("  Please create a new campaign using [N] Start New Campaign.\n");

                AppendFooter(sb,
                    "[N] Start New Campaign",
                    "[ESC] Return to Start Screen");
            }
            else
            {
                for (int i = 0; i < _vaultFiles.Count; i++)
                {
                    if (i == _continueSel)
                    {
                        sb.AppendLine($"    * [X] {_vaultFiles[i]}");
                    }
                    else
                    {
                        sb.AppendLine($"      [ ] {_vaultFiles[i]}");
                    }
                    sb.AppendLine();
                }
                if (!string.IsNullOrEmpty(_continueError)) sb.AppendLine($"  ! {_continueError}\n");

                AppendFooter(sb,
                    "[UP/DOWN] Select",
                    "[ENTER] Load Campaign",
                    "[ESC] Return to Start");
            }

            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  VAULT INGESTION / MANIFEST MODAL
        // ─────────────────────────────────────────────────────────────────────
        private string BuildVaultIngestionScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "DISCOVERED TELEMETRY MANIFEST");
            sb.AppendLine();
            AppendSectionHeader(sb, "P R O C E S S E D   F I L E   S U M M A R Y");

            var typeGroups = Manifest.Files
                .GroupBy(f => f.FileType)
                .OrderBy(g => g.Key switch
                {
                    "SIGINT" => 1,
                    "OPERATIONS" => 2,
                    "PWS_SAVE" => 3,
                    "COMBAT_EVENTS" => 4,
                    "COMBAT_REPORT" => 5,
                    _ => 6
                })
                .ToList();

            if (typeGroups.Count > 0)
            {
                sb.AppendLine("  TYPE            FILES  DAYS PROCESSED                   NEW");
                sb.AppendLine("  " + new string('─', 74));

                foreach (var group in typeGroups)
                {
                    string fType = group.Key;
                    int fileCount = group.Count();
                    int newCount = _lastIngest == null ? fileCount : (_lastIngest.NewByType.TryGetValue(fType, out var nn) ? nn : 0);

                    var sortedDates = group
                        .Where(f => !string.IsNullOrEmpty(f.DateYYMMDD) && TryParseYYMMDD(f.DateYYMMDD, out _))
                        .Select(f => { TryParseYYMMDD(f.DateYYMMDD, out var d); return d; })
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList();

                    string daysInfo;
                    if (sortedDates.Count == 0)
                    {
                        daysInfo = "[Turn Data / Meta]";
                    }
                    else if (sortedDates.Count <= 3)
                    {
                        daysInfo = string.Join(", ", sortedDates.Select(d => FormatDDMMYY(d)));
                        if (sortedDates.Count > 1)
                        {
                            daysInfo += $" ({sortedDates.Count} days)";
                        }
                    }
                    else
                    {
                        DateTime minDate = sortedDates.First();
                        DateTime maxDate = sortedDates.Last();
                        daysInfo = $"{FormatDDMMYY(minDate)} - {FormatDDMMYY(maxDate)} ({sortedDates.Count} days)";
                    }

                    sb.AppendLine($"  {fType,-14}  {fileCount,5}  {daysInfo,-31}  {"+" + newCount,4}");
                }
                sb.AppendLine("  " + new string('─', 74));
                sb.AppendLine($"  Total Relevant Game Files Detected: {Manifest.Files.Count}");
                sb.AppendLine($"  Total Operational Days Processed   : {Manifest.DaysProcessed}");
                sb.AppendLine($"  Latest Campaign Date               : {Manifest.FormattedDate}\n");
            }
            else
            {
                sb.AppendLine($"  Total Relevant Game Files Detected: 0");
                sb.AppendLine($"  Total Operational Days Processed   : 0");
                sb.AppendLine($"  Latest Campaign Date               : --/--/--\n");
            }

            if (_vault != null)
            {
                sb.AppendLine($"  Vault : {_vault.FilePath}");
                if (_lastIngest != null)
                {
                    sb.AppendLine($"  This load : +{_lastIngest.NewReports} reports, +{_lastIngest.NewSnapshots} save snapshot(s), {_lastIngest.Unchanged} unchanged, {_lastIngest.Superseded} updated");
                    foreach (var w in _lastIngest.Warnings.Take(3)) sb.AppendLine($"  ! {w}");
                }
                sb.AppendLine();
            }

            AppendSectionHeader(sb, "F I L E   I N V E N T O R Y   ( P W S   &   T X T )");
            int displayCount = 0;
            foreach (var file in Manifest.Files.OrderByDescending(f => f.DateYYMMDD, StringComparer.Ordinal))
            {
                string dateInfo = string.IsNullOrEmpty(file.DateYYMMDD) ? "SAVE" : FileScanner.FormatYYMMDD(file.DateYYMMDD);
                sb.AppendLine($"    [{file.FileType,-14}] {file.FileName,-35} (Date: {dateInfo})");
                displayCount++;
                if (displayCount >= 10)
                {
                    sb.AppendLine($"    ... and {Manifest.Files.Count - 10} additional files.");
                    break;
                }
            }

            if (Manifest.Files.Count == 0)
            {
                sb.AppendLine("    No files found in specified directories. Sample fixtures active.");
            }

            AppendFooter(sb,
                "[ENTER] Continue to Command Matrix",
                "[ESC] Return to Start");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  BOOT SCREEN
        // ─────────────────────────────────────────────────────────────────────
        private string BuildBootScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "ROCHEFORT 6000 // SYSTEM BOOT DIAGNOSTIC");
            sb.AppendLine();
            sb.AppendLine(">> SYSTEM INITIALIZATION: OK");
            sb.AppendLine(">> COLD-START DIAGNOSTICS COMPLETE. ARCHITECTURE: 2.028-ALPHA-ROCHEFORT");
            sb.AppendLine($">> AUTHORITATIVE BASE REGISTRY: {BaseLocationRegistry.Instance.Count:N0} BASES LOADED");
            sb.AppendLine(">> NAVAL DOCTRINE HULL TYPES: 60 CLASSES COMPILED");
            sb.AppendLine(">> 6 HEURISTICS READY");
            sb.AppendLine(">> 10 GRAPHIC SCREENS READY");
            sb.AppendLine();
            sb.AppendLine(@"  ██████╗  ██████╗  ██████╗██╗  ██╗███████╗███████╗ ██████╗ ██████╗ ████████╗");
            sb.AppendLine(@"  ██╔══██╗██╔═══██╗██╔════╝██║  ██║██╔════╝██╔════╝██╔═══██╗██╔══██╗╚══██╔══╝");
            sb.AppendLine(@"  ██████╔╝██║   ██║██║     ███████║█████╗  █████╗  ██║   ██║██████╔╝   ██║   ");
            sb.AppendLine(@"  ██╔══██╗██║   ██║██║     ██╔══██║██╔══╝  ██╔══╝  ██║   ██║██╔══██╗   ██║   ");
            sb.AppendLine(@"  ██║  ██║╚██████╔╝╚██████╗██║  ██║███████╗██║     ╚██████╔╝██║  ██║   ██║   ");
            sb.AppendLine(@"  ╚═╝  ╚═╝ ╚═════╝  ╚═════╝╚═╝  ╚═╝╚══════╝╚═╝      ╚═════╝ ╚═╝  ╚═╝   ╚═╝   ");
            sb.AppendLine();
            sb.AppendLine(Rule());
            sb.AppendLine("  [ SYSTEM READY ]");
            sb.AppendLine("  PRESS ANY KEY OR [ENTER] TO ACCESS OPERATIONAL THEATER DECK");
            sb.AppendLine(Rule());
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  MASTER MENU
        // ─────────────────────────────────────────────────────────────────────
        private string BuildMasterMenuScreen()
        {
            var sb = new StringBuilder();

            sb.AppendLine(Rule());
            string titleStr = "[ MASTER MENU ]";
            string campaignName = _vault != null ? _vault.Manifest.CampaignName : (Manifest.Files.Count > 0 ? "WITP:AE PBEM CAMPAIGN" : "NO CAMPAIGN LOADED");
            string rightStr = $"Active Campaign: {campaignName} (Allied Command)";
            int sp = Math.Max(1, W - titleStr.Length - rightStr.Length);
            sb.AppendLine(titleStr + new string(' ', sp) + rightStr);
            sb.AppendLine(Rule('-'));

            int pwsCount = Manifest.Files.Count(f => f.FileType == "PWS_SAVE");
            int txtCount = Manifest.Files.Count - pwsCount;
            string statsLine = $"Files Processed : {pwsCount} pws files | {txtCount} TXT files | Days Processed : {Manifest.DaysProcessed}";
            int statsPad = Math.Max(0, (W - statsLine.Length) / 2);
            sb.AppendLine(new string(' ', statsPad) + statsLine);
            sb.AppendLine(Rule());

            sb.AppendLine();
            AppendSectionHeader(sb, "COMMAND MODULES");

            string[] menuLabels =
            {
                "WAR ROOM (THEATER INTELLIGENCE & HEURISTICS)",
                "WIKI (WITP-CIC SYSTEM & ERROR REGISTRY)",
                "FIELD MANUAL (WITP:AE GAME MANUAL BROWSER)",
                "SYSTEM SETTINGS & DETECTION THRESHOLDS",
                "TERMINATE SESSION & PURGE MEMORY"
            };
            string[] hotkeys = { "[A] ", "[W] ", "[F] ", "[S] ", "[E] " };

            for (int i = 0; i < menuLabels.Length; i++)
            {
                string hk = hotkeys[i];
                if (i == _masterMenuSel)
                    sb.AppendLine($"    {hk}* {menuLabels[i]}");
                else
                    sb.AppendLine($"    {hk}  {menuLabels[i]}");
                sb.AppendLine();
            }

            AppendFooter(sb,
                "[UP/DOWN] Select", "[ENTER] Open",
                "[A] War Room", "[W] Wiki", "[F] Field Manual",
                "[S] Settings", "[ESC/E] Exit");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  WAR ROOM PRIMARY SCREEN
        // ─────────────────────────────────────────────────────────────────────
        private string BuildWarRoomPrimaryScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "WAR ROOM : THEATER COMBAT INFORMATION CENTER", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "W A R   R O O M   P R I M A R Y   S C R E E N");
            sb.AppendLine();

            string[] menuLabels =
            {
                "[ TBD ]",
                "[ TBD ]",
                "Graphics",
                "[ TBD ]",
                "[ TBD ]",
                "Heuristics (Logical Blueprints)"
            };

            for (int i = 0; i < menuLabels.Length; i++)
            {
                string optText = $"[{i + 1}]  {menuLabels[i]}";
                if (i == _warRoomSel)
                    sb.AppendLine($"    * {optText}");
                else
                    sb.AppendLine($"      {optText}");
                sb.AppendLine();
            }

            sb.AppendLine("    >> Press [1-6] or [ENTER] to open topic sub-menu. All pop-ups support cursor keys.");
            AppendFooter(sb, "[UP/DOWN] Select Topic", "[ENTER] Open Sub-Menu", "PRESS ESC TO RETURN");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  TBD DOMAIN
        // ─────────────────────────────────────────────────────────────────────
        private string BuildTbdDomainScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "WAR ROOM : TBD (THEATER BATTLESPACE DOMAIN)", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "MODULE STATUS : PHASE 08 RESERVED DOMAIN");

            const int bw = 78;
            const int innerWidth = 74;
            string topTitle = "╔══ [ THEATER BATTLESPACE DOMAIN (TBD) ] ";
            int rem = bw - topTitle.Length - 1;
            sb.AppendLine(topTitle + (rem > 0 ? new string('═', rem) : "") + "╗");

            string[] lines =
            {
                "",
                "  This operational partition is reserved for upcoming Phase 08 binary",
                "  integration deliverables:",
                "",
                "    - 2D Strategic Map ASCII/CP437 Battlespace Canvas",
                "    - Live .PWS vessel hex coordinate decoding & real-time tracking",
                "    - Aircraft replacement reserve pool live monitoring (0x20 records)",
                "    - Carrier strike range & naval air interdiction radius mapping",
                "    - Ship internal damage matrices (System, Engine, Flood, Fire)",
                "",
                "  Authentic PBEM telemetry is actively computed across Models A through G.",
                "  All models remain grounded in verified turn reports.",
                ""
            };

            foreach (var line in lines)
            {
                sb.AppendLine($"║ {PadTo(line, innerWidth)} ║");
            }

            sb.AppendLine("╚" + new string('═', bw - 2) + "╝");
            AppendFooter(sb, "PRESS ESC TO RETURN");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GRAPHICS SUB-MENU (10 OPTIONS)
        // ─────────────────────────────────────────────────────────────────────
        private string BuildGraphicsMenuScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "WAR ROOM : GRAPHICS (VISUAL TELEMETRY MATRIX)", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "T E L E M E T R Y   &   A N A L Y T I C S   G R A P H I C S   S U B - M E N U");
            sb.AppendLine();

            string[] options =
            {
                "Tactical Theater Hex Map & Fleet Dispositions",
                "Fleet Attrition & Cumulative Loss Curves",
                "Task Force Movement & Kinematics Plot",
                "[ TBD ]",
                "[ TBD ]",
                "[ TBD ]",
                "[ TBD ]",
                "[ TBD ]",
                "[ TBD ]",
                "[ TBD ]"
            };

            for (int i = 0; i < options.Length; i++)
            {
                string numStr = (i == 9) ? "[0]" : $"[{i + 1}]";
                string optText = $"{numStr,-5} {options[i]}";
                if (i == _graphicsMenuSel)
                {
                    sb.AppendLine($"    * {optText}");
                }
                else
                {
                    sb.AppendLine($"      {optText}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("    >> Press [1-9, 0] or [ENTER] to access visual telemetry screen.");
            AppendFooter(sb, "[UP/DOWN] Select Category", "[ENTER] Open Visual Screen", "PRESS ESC TO RETURN");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GRAPHICS DISPLAY SCREEN (STANDARDIZED PARTITIONS)
        // ─────────────────────────────────────────────────────────────────────
        private string BuildGraphicsDisplayScreen()
        {
            return _graphicsPageId switch
            {
                1 => BuildTheaterHexMapScreen(),
                2 => BuildLogisticalAttritionScreen(),
                3 => BuildStrategicVectorScreen(),
                _ => BuildGraphicsTbdScreen(_graphicsPageId)
            };
        }

        private string BuildTheaterHexMapScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "WAR ROOM : TACTICAL THEATER HEX MAP", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "PACIFIC THEATER OPERATIONAL GRID & TASK FORCE POSITIONS");

            const int bw = 78;
            const int innerWidth = 74;

            string topTitle = "╔══ [ PACIFIC THEATER OPERATIONAL HEX GRID & FLEET POSITIONS ] ";
            int rem = bw - topTitle.Length - 1;
            sb.AppendLine(topTitle + (rem > 0 ? new string('═', rem) : "") + "╗");

            int activeTfCount = Analytics.PwsTimeline.MostRecent?.AlliedTaskForceCount ?? 0;
            int atSeaCount = Analytics.PwsTimeline.MostRecent?.AlliedAtSeaCount ?? 0;
            int inPortCount = Analytics.PwsTimeline.MostRecent?.AlliedInPortCount ?? 0;

            string tfSummaryStr = GetActiveTfSummary();

            string[] mapLines =
            {
                "",
                " -- PACIFIC BASIN STRATEGIC HEX GRID (APPROX SCALE 1:40 NM) --------------",
                "   HEX  40   60   80  100  120  140  160  180  200  220  X",
                "    40  ·    ·   [TOKYO] ·   ·    ·    ·    ·    ·   [DUTCH HBR]     40",
                "    60  ·  [MANILA]  ·   ·   ·    ·    ·  [MIDWAY] ·    ·           60",
                "    80 [SINGAPORE]   · [TRUK]·    ·    · [PEARL HBR]· [SAN FRAN]    80",
                "   100  ·   [JAVA]   ·   ·   ·  [TARAWA]·   ·    ·    ·            100",
                "   120  ·    ·    ·  [RABAUL]·    ·    ·    ·    ·    ·            120",
                "   140  ·  [DARWIN]  ·  [GUADALCANAL]  ·    ·    ·    ·            140",
                "   160  ·    ·   [SYDNEY]·   ·  [NOUMEA]    ·    ·    ·            160",
                "    Y   40   60   80  100  120  140  160  180  200  220",
                "",
                " -- ALLIED TASK FORCE DISPOSITIONS ---------------------------------------",
                $" [FORCE STATUS]   : {activeTfCount} TASK FORCE(S) ACTIVE | {atSeaCount} AT SEA | {inPortCount} IN PORT",
                $" [FLAGSHIP TRACKS]: {tfSummaryStr}",
                "",
                " -- STRATEGIC CHOKEPOINTS & APPROACH CORRIDORS ---------------------------",
                " [MALACCA STR.] : Hex (50,84)  │ [LOMBOK STR.] : Hex (67,105)",
                " [SURIGAO STR.] : Hex (83,86)  │ [TORRES STR.] : Hex (93,131)",
                " [SAN BERNARDINO]: Hex (84,79) │ [IRONBOTTOM]  : Hex (113,138)",
                "",
                " ------------------------------------------------------------------------",
                " [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]",
                $" \"Strategic sea lines of communication monitored across {activeTfCount} Allied TF groups.\"",
                ""
            };

            foreach (var line in mapLines)
            {
                if (line.Length > innerWidth)
                    sb.AppendLine($"║ {PadTo(line.Substring(0, innerWidth), innerWidth)} ║");
                else
                    sb.AppendLine($"║ {PadTo(line, innerWidth)} ║");
            }

            sb.AppendLine("╚" + new string('═', bw - 2) + "╝");
            AppendFooter(sb, "PRESS ESC OR ENTER TO RETURN");
            return sb.ToString();
        }

        private string GetActiveTfSummary()
        {
            var snap = Analytics.PwsTimeline.MostRecent;
            if (snap == null || snap.AlliedTaskForces.Count == 0)
                return "Zero active Task Forces deployed.";

            var items = new List<string>();
            foreach (var tf in snap.AlliedTaskForces.Take(2))
            {
                var flagship = snap.AlliedShips.FirstOrDefault(s => s.Index == tf.FlagshipIndex);
                string flagName = flagship != null ? flagship.Name : $"Flagship #{tf.FlagshipIndex}";
                items.Add($"TF {tf.Index} ({flagName}) @ ({tf.HexX},{tf.HexY})");
            }
            return string.Join(" | ", items);
        }

        private string BuildLogisticalAttritionScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "WAR ROOM : LOGISTICAL & ATTRITION TELEMETRY", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "NAVAL & AIR ATTRITION METRICS (CAMPAIGN DURATION)");

            const int bw = 78;
            const int innerWidth = 74;
            var vm = TelemetryAnalyticsEngine.ComputePage1ViewModel(Analytics, Analytics.Heuristics, Manifest);

            string topTitle = "╔══ [ NAVAL & AIR ATTRITION METRICS (CAMPAIGN DURATION) ] ";
            int rem = bw - topTitle.Length - 1;
            sb.AppendLine(topTitle + (rem > 0 ? new string('═', rem) : "") + "╗");

            string[] lines =
            {
                "",
                " -- SECTION 1: ATTRITION & DRAIN METRICS ---------------------------------",
                $" [K/L RATIO] : CURRENT: [ {vm.CurrentKLRatio:F1}:1 ]  SLOPE: [ {vm.KLSlopeTrend} ] [██████████████░░░░░░░░░░]",
                $" [RES. DRAIN]: AIR LOSSES: [ {vm.AirLossesPerWeek}/WK ]  SHIP LOSSES: [ {vm.ShipLossesPerWeek}/WK ] [████████░░░░░░░░░░░░░]",
                "",
                " -- SECTION 2: OPERATIONAL TEMPO (OPTEMPO) TIME-SERIES -------------------",
                " [LABEL: ROLLING AVERAGE OF EVENTS PER TURN (BRAILLE TIME-SERIES CURVE)  ]",
                "",
                "  EVENTS",
                "    20 |                                            ⢠⣶",
                "    15 |                                    ⢀⣴⡿⠋",
                "    10 |                        ⢀⣠⡾⠟⠁",
                "     5 |            ⢀⣀⣤⣴⣶⡶⠿⠿⠋⠁",
                "     0 +------------------------------------------------------------------",
                "        D01 D02 D03 D04 D05 D06 D07 D08 D09 D10 D11 D12 D13 D14 D15 D16",
                "",
                " -- SECTION 3: LOGISTICAL READINESS METERS -------------------------------",
                $" [ESCORT DEN]: BASE SECTOR A: [ {vm.EscortDensity:F1} units/hex ] [RATE: -12% ↓] [██████░░░░░░░░░░]",
                $" [FUEL LIMIT]: {vm.FuelLimitTfName} : [ MAX RANGE: {vm.FuelMaxRangeNM}NM ] [LIMIT: {vm.FuelLimitPercent}% ] [█████████░░░░░]",
                $" [EXHAUSTION]: {vm.ExhaustionTfName}  : [ RETREAT DUE: {vm.RetreatDueDate} ] [CRITICAL: {vm.ExhaustionCriticalPercent}% ] [██░░░░░░░░░░░░]",
                "",
                " ------------------------------------------------------------------------",
                " [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]",
                $" \"{vm.TacticalAssessment}\"",
                ""
            };

            foreach (var line in lines)
            {
                if (line.Length > innerWidth)
                {
                    sb.AppendLine($"║ {PadTo(line.Substring(0, innerWidth), innerWidth)} ║");
                }
                else
                {
                    sb.AppendLine($"║ {PadTo(line, innerWidth)} ║");
                }
            }

            sb.AppendLine("╚" + new string('═', bw - 2) + "╝");
            AppendFooter(sb, "PRESS ESC OR ENTER TO RETURN");
            return sb.ToString();
        }

        private string BuildStrategicVectorScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "WAR ROOM : STRATEGIC VECTOR ANALYSIS", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "ENEMY FORCE VECTORS & STRATEGIC MASS DISTRIBUTION");

            const int bw = 78;
            const int innerWidth = 74;
            var vm = TelemetryAnalyticsEngine.BuildStrategicVectorPage2ViewModel(Analytics, Analytics.Heuristics);

            string topTitle = "╔══ [ ENEMY FORCE VECTORS & STRATEGIC MASS ] ";
            int rem = bw - topTitle.Length - 1;
            sb.AppendLine(topTitle + (rem > 0 ? new string('═', rem) : "") + "╗");

            string[] lines =
            {
                "",
                " -- SECTION 1: STRATEGIC MASS DISTRIBUTION ---------------------------------",
                $" [CONCENTRATION]: MAIN EFFORT: [ {vm.MainEffortPercent}% ] SECONDARY: [ {vm.SecondaryEffortPercent}% ] [██████████████░░░░░░]",
                $" [POWER SHIFT]  : {vm.PrimarySectorName}: [ +{vm.PrimarySectorShiftPct:F0}% ]  {vm.SecondarySectorName}: [ {vm.SecondarySectorShiftPct:F0}% ]",
                "",
                " -- SECTION 2: CENTER OF GRAVITY (CoG) & AXIS OF ADVANCE --------------------",
                " [LABEL: JAPANESE FORCE CENTROID MAPPING (10x10 REGIONAL GRID)            ]",
                "",
                "    Y",
                "   10 | . . . . . . . . . .",
                "    8 | . . . . . X . . . .",
                $"    6 | . . . X X . . . . .     Vector: [{vm.VectorHeading}]",
                $"    4 | . . X X . . . . . .     Magnitude: {vm.VectorMagnitude:F1} Hex/Day",
                $"    2 | . . . . . . . . . .     Current CoG: ({vm.CurrentCoG.X}, {vm.CurrentCoG.Y})",
                "    0 +----------------------",
                "       0 2 4 6 8 10  X",
                "",
                " -- SECTION 3: LOGISTICAL RANGE PREDICTIONS ---------------------------------",
                $" [LOG. SHADOW]: {vm.LogShadowTf1}",
                $" [LOG. SHADOW]: {vm.LogShadowTf2}",
                $" [LOG. SHADOW]: {vm.LogShadowTf3}",
                "",
                " ------------------------------------------------------------------------",
                " [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]",
                $" \"{vm.TacticalAssessment}\"",
                ""
            };

            foreach (var line in lines)
            {
                if (line.Length > innerWidth)
                    sb.AppendLine($"║ {PadTo(line.Substring(0, innerWidth), innerWidth)} ║");
                else
                    sb.AppendLine($"║ {PadTo(line, innerWidth)} ║");
            }

            sb.AppendLine("╚" + new string('═', bw - 2) + "╝");
            AppendFooter(sb, "PRESS ESC OR ENTER TO RETURN");
            return sb.ToString();
        }

        private string BuildIntelligenceAnomalyScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "WAR ROOM : INTELLIGENCE & ANOMALY DETECTION", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "SIGNAL ANOMALIES & INTELLIGENCE BLINDSPOTS");

            const int bw = 78;
            const int innerWidth = 74;
            var vm = TelemetryAnalyticsEngine.BuildIntelligenceAnomalyPage3ViewModel(Analytics, Analytics.Heuristics);

            string topTitle = "╔══ [ SIGNAL ANOMALIES & INTELLIGENCE BLINDSPOTS ] ";
            int rem = bw - topTitle.Length - 1;
            sb.AppendLine(topTitle + (rem > 0 ? new string('═', rem) : "") + "╗");

            string[] lines =
            {
                "",
                " -- SECTION 1: SIGHTING ANOMALIES ------------------------------------------",
                $" [CLUSTERING]    : SECTOR {vm.ClusterSector} : [ HIGH DENSITY ] [SIGHTINGS: {vm.ClusterSightingsCount} ]",
                $" [SIGHTING SPIKE]: SECTOR {vm.SpikeSector}    : [ +{vm.SightingSpikePercent:F0}% ] [STATUS: ALERT] [█████████████████████]",
                $" [BLINDSPOT]     : SECTOR {vm.BlindspotSector}: [ ACTIVE/SILENT ] [RISK: {vm.BlindspotRiskTier}] [███████████████████]",
                "",
                " -- SECTION 2: TACTICAL ANOMALY LOG ----------------------------------------",
                "  TIME  │ TYPE                  │ SECTOR │ CONFIDENCE │ STATUS",
                "  ──────┼───────────────────────┼────────┼────────────┼───────────────────"
            };

            var allLines = new List<string>(lines);
            foreach (var a in vm.AnomalyLog.Take(3))
            {
                string t = PadTo(a.TimeCode, 6);
                string tp = PadTo(a.AnomalyType.Length > 21 ? a.AnomalyType.Substring(0, 21) : a.AnomalyType, 21);
                string sec = PadTo(a.Sector, 6);
                string conf = PadTo($"{a.Confidence}%", 10);
                string st = a.Status;
                allLines.Add($"  {t}│ {tp} │ {sec} │ {conf} │ {st}");
            }

            allLines.AddRange(new[]
            {
                "",
                " -- SECTION 3: SECTOR SATURATION GAUGES ------------------------------------",
                $" [{vm.SectorName1}]: [ DENSITY: {vm.SectorDensity1}% ████████████░░░░ ] [PHASE: {vm.SectorPhase1}]",
                $" [{vm.SectorName2}]: [ DENSITY: {vm.SectorDensity2}% ██████░░░░░░░░░ ] [PHASE: {vm.SectorPhase2}]",
                $" [{vm.SectorName3}]: [ DENSITY: {vm.SectorDensity3}% ███████████████ ] [PHASE: {vm.SectorPhase3}]",
                "",
                " ------------------------------------------------------------------------",
                " [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]",
                $" \"{vm.TacticalAssessment}\"",
                ""
            });

            foreach (var line in allLines)
            {
                if (line.Length > innerWidth)
                    sb.AppendLine($"║ {PadTo(line.Substring(0, innerWidth), innerWidth)} ║");
                else
                    sb.AppendLine($"║ {PadTo(line, innerWidth)} ║");
            }

            sb.AppendLine("╚" + new string('═', bw - 2) + "╝");
            AppendFooter(sb, "PRESS ESC OR ENTER TO RETURN");
            return sb.ToString();
        }

        private string BuildOperationalTempoScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "WAR ROOM : OPERATIONAL TEMPO & PIVOTS", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "THEATER RHYTHM & OFFENSIVE OSCILLATION");

            const int bw = 78;
            const int innerWidth = 74;
            var vm = TelemetryAnalyticsEngine.BuildOperationalTempoPage4ViewModel(Analytics, Analytics.Heuristics);

            string topTitle = "╔══ [ THEATER RHYTHM & OFFENSIVE OSCILLATION ] ";
            int rem = bw - topTitle.Length - 1;
            sb.AppendLine(topTitle + (rem > 0 ? new string('═', rem) : "") + "╗");

            string[] lines =
            {
                "",
                " -- SECTION 1: TEMPO VOLATILITY & BREATH -----------------------------------",
                $" [VOLATILITY] : [ {vm.VolatilityStatus} ] [VAR: {vm.VolatilityVariance:F1} ] [████████████████░░░░]",
                $" [OFF. BREATH]: [ {vm.OffensiveBreath} ] [HEXES: {vm.BreathHexCount} ] [████████████░░░░░░]",
                "",
                " -- SECTION 2: STRATEGIC PIVOT LATENCY -------------------------------------",
                " [LABEL: TIME ELAPSED BETWEEN OFFENSIVE PHASES (DAYS)                      ]",
                "",
                "   DAYS",
                "    10 |                                                     .::'",
                "     8 |                                             .::'",
                "     6 |                                     .::'",
                "     4 |                            .::'",
                "     2 |                    .::'",
                "     0 +------------------------------------------------------------------",
                "        PIVOT 1 PIVOT 2 PIVOT 3 PIVOT 4 PIVOT 5 PIVOT 6 PIVOT 7 PIVOT 8",
                "",
                " -- SECTION 3: COORDINATION SYMMETRY ---------------------------------------",
                $" {vm.SymmetryPair1}",
                $" {vm.SymmetryPair2}",
                $" {vm.SymmetryPair3}",
                "",
                " ------------------------------------------------------------------------",
                " [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]",
                $" \"{vm.TacticalAssessment}\"",
                ""
            };

            foreach (var line in lines)
            {
                if (line.Length > innerWidth)
                    sb.AppendLine($"║ {PadTo(line.Substring(0, innerWidth), innerWidth)} ║");
                else
                    sb.AppendLine($"║ {PadTo(line, innerWidth)} ║");
            }

            sb.AppendLine("╚" + new string('═', bw - 2) + "╝");
            AppendFooter(sb, "PRESS ESC OR ENTER TO RETURN");
            return sb.ToString();
        }

        private string BuildGraphicsTbdScreen(int slot)
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "WAR ROOM : GRAPHICS TELEMETRY SCREEN", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "MODULE STATUS : RESERVED TELEMETRY PARTITION (TBD)");

            const int bw = 78;
            const int innerWidth = 74;

            string topTitle = $"╔══ [ VISUAL TELEMETRY MATRIX : OPTION {slot} (TBD) ] ";
            int rem = bw - topTitle.Length - 1;
            sb.AppendLine(topTitle + (rem > 0 ? new string('═', rem) : "") + "╗");

            string[] lines =
            {
                "",
                "  This operational partition is designated as [ TBD ] (To Be Defined).",
                "  Awaiting analytical wireframe specification and data mapping.",
                "",
                "    - Real-time time-series telemetry curve integration pending",
                "    - Multi-axis correlation & vector projections pending",
                "    - Combat attrition & operational drain algorithms active in background",
                "",
                "  Authentic PBEM telemetry is actively computed across Models A through G",
                "  and the Telemetry Analytics Engine. Visual plotting template active.",
                ""
            };

            foreach (var line in lines)
            {
                sb.AppendLine($"║ {PadTo(line, innerWidth)} ║");
            }

            sb.AppendLine("╚" + new string('═', bw - 2) + "╝");
            AppendFooter(sb, "PRESS ESC OR ENTER TO RETURN");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  REASONING MODAL
        // ─────────────────────────────────────────────────────────────────────
        private string BuildReasoningModal()
        {
            const int width = 72;
            const int innerWidth = 68;
            var sb = new StringBuilder();

            string titlePrefix = "╔══ [ REASONING & TELEMETRY EVIDENCE ] ";
            int rem = width - titlePrefix.Length - 1;
            sb.AppendLine(titlePrefix + (rem > 0 ? new string('═', rem) : "") + "╗");

            var itemLines = WrapText($"ITEM: {_reasoningBulletTitle}", innerWidth, 6);
            foreach (var l in itemLines)
            {
                sb.AppendLine($"║ {PadTo(l, innerWidth)} ║");
            }
            sb.AppendLine("╠" + new string('═', width - 2) + "╣");

            if (_reasoningConfPct > 0)
            {
                int filled = Math.Min(10, Math.Max(0, _reasoningConfPct / 10));
                string bar = "[" + new string('=', filled) + new string(' ', 10 - filled) + "]";
                string tier = _reasoningConfPct >= 90 ? "VERIFIED" : _reasoningConfPct >= 70 ? "CONFIRMED" : _reasoningConfPct >= 40 ? "PROBABLE" : "UNCONFIRMED";
                string confText = $"CONFIDENCE LEVEL: {bar} {_reasoningConfPct}% [{tier}]";
                sb.AppendLine($"║ {PadTo(confText, innerWidth)} ║");
                sb.AppendLine("╠" + new string('═', width - 2) + "╣");
            }

            string prompt = "REASONING TOPICS & EVIDENCE (USE ARROW KEYS TO NAVIGATE):";
            sb.AppendLine($"║ {PadTo(prompt, innerWidth)} ║");
            sb.AppendLine($"║ {new string(' ', innerWidth)} ║");

            for (int i = 0; i < _reasoningTopics.Count; i++)
            {
                string prefix = (i == _reasoningTopicSel) ? "* " : "  ";
                string num = $"[{i + 1}] ";
                string title = _reasoningTopics[i].Title;
                string full = PadTo(prefix + num + title, innerWidth);
                sb.AppendLine($"║ {full} ║");
                sb.AppendLine($"║ {new string(' ', innerWidth)} ║");
            }

            if (_reasoningTopics.Count > 0 && _reasoningTopicSel < _reasoningTopics.Count)
            {
                sb.AppendLine("╠" + new string('═', width - 2) + "╣");
                sb.AppendLine($"║ {PadTo($"--- EVIDENCE: {_reasoningTopics[_reasoningTopicSel].Title} ---", innerWidth)} ║");
                sb.AppendLine($"║ {new string(' ', innerWidth)} ║");
                foreach (var line in _reasoningTopics[_reasoningTopicSel].Lines)
                {
                    var wrapped = WrapText(line, innerWidth, 4);
                    foreach (var wLine in wrapped)
                    {
                        sb.AppendLine($"║ {PadTo(wLine, innerWidth)} ║");
                    }
                }
                sb.AppendLine($"║ {new string(' ', innerWidth)} ║");
            }

            sb.AppendLine("╠" + new string('═', width - 2) + "╣");
            string footerText = "[UP/DOWN] Select Topic  │  PRESS ESC TO RETURN";
            sb.AppendLine($"║ {PadTo(footerText, innerWidth)} ║");
            sb.AppendLine("╚" + new string('═', width - 2) + "╝");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  TEMPORAL FILTER MODAL
        // ─────────────────────────────────────────────────────────────────────
        private string BuildTemporalFilterModalScreen()
        {
            const int width = 106;
            const int innerWidth = width - 4; // 102
            var sb = new StringBuilder();

            string topTitle = "╔══ [ TEMPORAL INTELLIGENCE FILTER & DATE RANGE SELECTOR ] ";
            int rem = width - topTitle.Length - 1;
            sb.AppendLine(topTitle + (rem > 0 ? new string('═', rem) : "") + "╗");
            sb.AppendLine($"║ {new string(' ', innerWidth)} ║");

            if (_filterTextEntryActive)
            {
                // VIEW 3: INLINE CUSTOM RANGE TYPING PROMPT
                sb.AppendLine($"║ {PadTo("  CUSTOM RANGE ENTRY MODE:", innerWidth)} ║");
                sb.AppendLine($"║ {new string(' ', innerWidth)} ║");
                sb.AppendLine($"║ {PadTo("    ENTER START DATE (DD/MM/YY) OR RANGE (DD/MM/YY - DD/MM/YY):", innerWidth)} ║");
                string promptLine = $"    >> {_filterTextInput}_";
                sb.AppendLine($"║ {PadTo(promptLine, innerWidth)} ║");
                sb.AppendLine($"║ {new string(' ', innerWidth)} ║");

                if (!string.IsNullOrEmpty(_filterInputError))
                {
                    sb.AppendLine($"║ {PadTo("    [!] " + _filterInputError, innerWidth)} ║");
                    sb.AppendLine($"║ {new string(' ', innerWidth)} ║");
                }

                sb.AppendLine($"║ {PadTo("    VALID FORMATS:", innerWidth)} ║");
                sb.AppendLine($"║ {PadTo("      - Single Date: 04/05/42", innerWidth)} ║");
                sb.AppendLine($"║ {PadTo("      - Date Range:  15/04/42 - 02/05/42  or  15/04/42 to 02/05/42", innerWidth)} ║");
                sb.AppendLine($"║ {PadTo("      - Quick Span:  -7d (Last 7 Days), -15d (Last 15 Days), -30d (Last Month)", innerWidth)} ║");
                sb.AppendLine($"║ {new string(' ', innerWidth)} ║");
                sb.AppendLine($"║ {new string(' ', innerWidth)} ║");
                sb.AppendLine($"║ {new string(' ', innerWidth)} ║");
                sb.AppendLine($"║ {new string(' ', innerWidth)} ║");
                sb.AppendLine($"║ {PadTo("  " + new string('─', innerWidth - 4), innerWidth)} ║");
                string customFooter = "  [ENTER] Confirm & Filter Telemetry          [BACKSPACE] Edit          [ESC] Cancel";
                sb.AppendLine($"║ {PadTo(customFooter, innerWidth)} ║");
                sb.AppendLine("╚" + new string('═', width - 2) + "╝");
                return sb.ToString();
            }

            // VIEW 2: MODAL OVERLAY MENU
            var turns = GetSortedTurnDatesDescending();
            DateTime latest = DateTime.MinValue;
            if (turns.Count > 0) TryParseYYMMDD(turns[0], out latest);
            string latestTurnStr = latest != DateTime.MinValue ? FormatMMDDYY(latest) : "--/--/--";

            DateTime prev = DateTime.MinValue;
            if (turns.Count > 1) TryParseYYMMDD(turns[1], out prev);
            else prev = latest;
            string prevTurnStr = prev != DateTime.MinValue ? FormatMMDDYY(prev) : latestTurnStr;

            string d7Start = latest != DateTime.MinValue ? FormatMMDDYY(latest.AddDays(-7)) : "--/--/--";
            string d15Start = latest != DateTime.MinValue ? FormatMMDDYY(latest.AddDays(-15)) : "--/--/--";
            string d30Start = latest != DateTime.MinValue ? FormatMMDDYY(latest.AddDays(-30)) : "--/--/--";
            string earliestStr = turns.Count > 0 && TryParseYYMMDD(turns[^1], out DateTime eDt) ? FormatMMDDYY(eDt) : "07/12/41";

            string p1Mark = _filterMode == TemporalFilterMode.LatestTurn ? "* " : "  ";
            string p2Mark = _filterMode == TemporalFilterMode.PreviousTurn ? "* " : "  ";
            string p3Mark = _filterMode == TemporalFilterMode.Last7Days ? "* " : "  ";
            string p4Mark = _filterMode == TemporalFilterMode.Last15Days ? "* " : "  ";
            string p5Mark = _filterMode == TemporalFilterMode.Last30Days ? "* " : "  ";
            string p6Mark = _filterMode == TemporalFilterMode.AllCampaign ? "* " : "  ";

            sb.AppendLine($"║ {PadTo("  QUICK OPERATIONAL PRESETS:", innerWidth)} ║");
            sb.AppendLine($"║ {PadTo($"    {p1Mark}[1] Latest Turn Only            {latestTurnStr} (Turn {turns.Count})", innerWidth)} ║");
            sb.AppendLine($"║ {PadTo($"    {p2Mark}[2] Previous Turn               {prevTurnStr} (Turn {Math.Max(1, turns.Count - 1)})", innerWidth)} ║");
            sb.AppendLine($"║ {PadTo($"    {p3Mark}[3] Last 7 Days (Tactical)      {d7Start} -> {latestTurnStr}", innerWidth)} ║");
            sb.AppendLine($"║ {PadTo($"    {p4Mark}[4] Last 15 Days (Operational)  {d15Start} -> {latestTurnStr}", innerWidth)} ║");
            sb.AppendLine($"║ {PadTo($"    {p5Mark}[5] Last 30 Days (Strategic)    {d30Start} -> {latestTurnStr}", innerWidth)} ║");
            sb.AppendLine($"║ {PadTo($"    {p6Mark}[6] Entire Campaign (All Turns) {earliestStr} -> {latestTurnStr} ({turns.Count} Turns Available)", innerWidth)} ║");
            sb.AppendLine($"║ {new string(' ', innerWidth)} ║");

            sb.AppendLine($"║ {PadTo("  " + new string('─', innerWidth - 4), innerWidth)} ║");
            sb.AppendLine($"║ {new string(' ', innerWidth)} ║");

            string sVal = _filterMode == TemporalFilterMode.CustomDate ? FormatMMDDYY(_filterEndDate) : "";
            string cVal = _filterMode == TemporalFilterMode.CustomRange ? $"{FormatMMDDYY(_filterStartDate)} - {FormatMMDDYY(_filterEndDate)}" : "";

            string sDisplay = PadTo(sVal, 16);
            string cDisplay = PadTo(cVal, 21);

            sb.AppendLine($"║ {PadTo("  CUSTOM DATE / RANGE ENTRY (DD/MM/YY):", innerWidth)} ║");
            sb.AppendLine($"║ {PadTo($"    [S] Enter Specific Date:  [ {sDisplay} ]", innerWidth)} ║");
            sb.AppendLine($"║ {PadTo($"    [C] Enter Custom Range:   [ {cDisplay} ]", innerWidth)} ║");
            sb.AppendLine($"║ {new string(' ', innerWidth)} ║");

            sb.AppendLine($"║ {PadTo("  " + new string('─', innerWidth - 4), innerWidth)} ║");

            // Calculate matching entries count
            int matchCount = Analytics.GeospatialLog.Count(e => IsDateInActiveFilter(e.DateYYMMDD, e.AllDates));
            string activeLine = $"  ACTIVE FILTER: {GetActiveFilterDescription()}  │  MATCHING SIGHTINGS: {matchCount} ENTRIES";
            sb.AppendLine($"║ {PadTo(activeLine, innerWidth)} ║");
            sb.AppendLine($"║ {new string(' ', innerWidth)} ║");

            string footerNav = "  [1-6] Quick Preset   [S/C] Calendar Pop-up   [UP/DOWN] Traverse   [ENTER] Apply Filter  [ESC] Cancel";
            sb.AppendLine($"║ {PadTo(footerNav, innerWidth)} ║");
            sb.AppendLine("╚" + new string('═', width - 2) + "╝");

            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CALENDAR PICKER MODAL
        // ─────────────────────────────────────────────────────────────────────
        private string BuildCalendarPickerModalScreen()
        {
            const int width = 106;
            const int innerWidth = width - 4; // 102
            var sb = new StringBuilder();

            string topTitle = "╔══ [ TEMPORAL INTELLIGENCE FILTER // INTERACTIVE CALENDAR SELECTOR ] ";
            int rem = width - topTitle.Length - 1;
            sb.AppendLine(topTitle + (rem > 0 ? new string('═', rem) : "") + "╗");
            sb.AppendLine($"║ {new string(' ', innerWidth)} ║");

            string modeDesc;
            if (!_calendarIsRangeMode)
            {
                modeDesc = "  MODE: SINGLE OPERATIONAL DATE SELECTION";
            }
            else if (_calendarRangeStep == 1)
            {
                modeDesc = "  MODE: CUSTOM DATE RANGE — [STEP 1 OF 2] SELECT START DATE";
            }
            else
            {
                modeDesc = $"  MODE: CUSTOM DATE RANGE — [STEP 2 OF 2] SELECT END DATE (START: {FormatMMDDYY(_calendarRangeStartDate)})";
            }
            sb.AppendLine($"║ {PadTo(modeDesc, innerWidth)} ║");
            sb.AppendLine($"║ {PadTo("  " + new string('─', innerWidth - 4), innerWidth)} ║");
            sb.AppendLine($"║ {new string(' ', innerWidth)} ║");

            // Month navigation banner
            string monthYearStr = _calendarCursorDate.ToString("MMMM yyyy").ToUpperInvariant();
            string navBanner = $"« [PgUp/M] PREV MONTH        [  {monthYearStr}  ]        NEXT MONTH [PgDn/N] »";
            int navPad = Math.Max(0, (innerWidth - navBanner.Length) / 2);
            string centeredNav = new string(' ', navPad) + navBanner;
            sb.AppendLine($"║ {PadTo(centeredNav, innerWidth)} ║");
            sb.AppendLine($"║ {new string(' ', innerWidth)} ║");

            // Day-of-week header: 7 columns of 8 chars = 56 chars
            string dowHeader = "  SUN      MON      TUE      WED      THU      FRI      SAT";
            int gridPad = Math.Max(0, (innerWidth - 56) / 2);
            string padStr = new string(' ', gridPad);
            sb.AppendLine($"║ {PadTo(padStr + dowHeader, innerWidth)} ║");
            sb.AppendLine($"║ {PadTo(padStr + "  ───      ───      ───      ───      ───      ───      ───", innerWidth)} ║");

            // Calendar grid
            DateTime monthStart = new DateTime(_calendarCursorDate.Year, _calendarCursorDate.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(_calendarCursorDate.Year, _calendarCursorDate.Month);
            int startCol = (int)monthStart.DayOfWeek; // 0 = Sunday

            var knownDates = new HashSet<string>(Manifest.UniqueDates);

            int currentDay = 1;
            int totalWeeks = (startCol + daysInMonth + 6) / 7;

            for (int w = 0; w < totalWeeks; w++)
            {
                var rowSb = new StringBuilder();
                rowSb.Append(padStr);
                for (int col = 0; col < 7; col++)
                {
                    int dayIndex = w * 7 + col;
                    if (dayIndex < startCol || currentDay > daysInMonth)
                    {
                        rowSb.Append("        "); // 8 spaces
                    }
                    else
                    {
                        DateTime thisDate = new DateTime(_calendarCursorDate.Year, _calendarCursorDate.Month, currentDay);
                        bool isOutOfBounds = thisDate < CalendarMinDate || thisDate > CalendarMaxDate;
                        bool isCursor = thisDate.Date == _calendarCursorDate.Date;
                        bool isRangeStart = _calendarIsRangeMode && _calendarRangeStep == 2 && thisDate.Date == _calendarRangeStartDate.Date;
                        bool inSelectedRange = false;
                        if (_calendarIsRangeMode && _calendarRangeStep == 2)
                        {
                            DateTime minD = _calendarRangeStartDate < _calendarCursorDate ? _calendarRangeStartDate : _calendarCursorDate;
                            DateTime maxD = _calendarRangeStartDate > _calendarCursorDate ? _calendarRangeStartDate : _calendarCursorDate;
                            if (thisDate.Date >= minD.Date && thisDate.Date <= maxD.Date)
                            {
                                inSelectedRange = true;
                            }
                        }

                        string yymmdd = thisDate.ToString("yyMMdd");
                        bool hasTurn = knownDates.Contains(yymmdd);

                        string cell;
                        if (isOutOfBounds)
                        {
                            cell = $"  ..    ";
                        }
                        else if (isCursor && isRangeStart)
                        {
                            cell = $"*[{currentDay:D2}]*  ";
                        }
                        else if (isCursor)
                        {
                            cell = $" [{currentDay:D2}]   ";
                        }
                        else if (isRangeStart)
                        {
                            cell = $" ({currentDay:D2})   ";
                        }
                        else if (inSelectedRange)
                        {
                            cell = $" ={currentDay:D2}=   ";
                        }
                        else if (hasTurn)
                        {
                            cell = $"  {currentDay:D2}*   ";
                        }
                        else
                        {
                            cell = $"  {currentDay:D2}    ";
                        }

                        rowSb.Append(cell);
                        currentDay++;
                    }
                }
                sb.AppendLine($"║ {PadTo(rowSb.ToString(), innerWidth)} ║");
            }

            // Pad remaining lines to keep height consistent (ensure at least 6 weeks rendered)
            for (int w = totalWeeks; w < 6; w++)
            {
                sb.AppendLine($"║ {new string(' ', innerWidth)} ║");
            }

            sb.AppendLine($"║ {PadTo("  " + new string('─', innerWidth - 4), innerWidth)} ║");

            // Info section
            string curDateFormatted = _calendarCursorDate.ToString("dddd, MMMM dd, yyyy").ToUpperInvariant();
            string curCode = FormatMMDDYY(_calendarCursorDate);
            string curTurnCheck = knownDates.Contains(_calendarCursorDate.ToString("yyMMdd")) ? "[TURN TELEMETRY PRESENT]" : "[NO TURN FILE]";
            sb.AppendLine($"║ {PadTo($"  CURRENT CURSOR : {curDateFormatted} ({curCode})  {curTurnCheck}", innerWidth)} ║");

            if (_calendarIsRangeMode)
            {
                string rStartStr = FormatMMDDYY(_calendarRangeStartDate);
                string rEndStr = _calendarRangeStep == 1 ? "PENDING..." : FormatMMDDYY(_calendarCursorDate);
                int spanDays = _calendarRangeStep == 1 ? 1 : Math.Abs((_calendarCursorDate.Date - _calendarRangeStartDate.Date).Days) + 1;
                sb.AppendLine($"║ {PadTo($"  SELECTED RANGE : {rStartStr}  ──>  {rEndStr}  ({spanDays} DAYS SPAN)", innerWidth)} ║");
            }
            else
            {
                sb.AppendLine($"║ {PadTo($"  TARGET FILTER  : SINGLE OPERATIONAL TURN / REPORT DATE [{curCode}]", innerWidth)} ║");
            }

            sb.AppendLine($"║ {PadTo("  BOUNDS         : PEARL HARBOR (12/07/1941) TO CAMPAIGN CONCLUSION (12/31/1946)", innerWidth)} ║");
            sb.AppendLine($"║ {PadTo("  " + new string('─', innerWidth - 4), innerWidth)} ║");

            string calFooter = "  [ARROWS] Day/Week   [PgUp/PgDn/M/N] Month   [ENTER] Select Date   [ESC] Cancel";
            sb.AppendLine($"║ {PadTo(calFooter, innerWidth)} ║");
            sb.AppendLine("╚" + new string('═', width - 2) + "╝");

            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  WIKI TOPIC MENU
        // ─────────────────────────────────────────────────────────────────────
        private string BuildWikiTopicMenuScreen()
        {
            if (_wikiNavStack.Count == 0) return BuildMasterMenuScreen();

            var current = _wikiNavStack.Peek();
            var topics = current.Topics;
            int total = topics.Count;

            var sb = new StringBuilder();
            AppendHeader(sb, current.Header);
            sb.AppendLine();
            AppendSectionHeader(sb, "D O C U M E N T A T I O N   T O P I C S");

            var leftRows = new List<(string Prefix, string Title, bool IsSelected)>();
            for (int i = 0; i < total; i++)
            {
                string prefix = $"  [{topics[i].Id}] ";
                string title = topics[i].Title;
                if (title.Length > 30) title = title.Substring(0, 27) + "...";
                leftRows.Add((prefix, title, i == _wikiMenuSel));
                leftRows.Add(("", "", false));
            }

            var rightBox = new List<string>();
            if (_wikiMenuSel >= 0 && _wikiMenuSel < total && !string.IsNullOrEmpty(topics[_wikiMenuSel].Description))
            {
                rightBox.Add("┌── [ TOPIC EXPLANATION ] ───────┐");
                string desc = topics[_wikiMenuSel].Description;
                const int contentWidth = 30;
                int startPos = 0;
                while (startPos < desc.Length)
                {
                    int chunkLen = Math.Min(contentWidth, desc.Length - startPos);
                    if (startPos + chunkLen < desc.Length)
                    {
                        int spacePos = desc.LastIndexOf(' ', startPos + chunkLen - 1, chunkLen);
                        if (spacePos > startPos) chunkLen = spacePos - startPos;
                    }
                    string line = desc.Substring(startPos, chunkLen).Trim();
                    if (line.Length < contentWidth) line = line.PadRight(contentWidth);
                    rightBox.Add("│ " + line + " │");
                    startPos += chunkLen;
                    while (startPos < desc.Length && desc[startPos] == ' ') startPos++;
                }
                rightBox.Add("└────────────────────────────────┘");
            }

            int totalRows = Math.Max(leftRows.Count, rightBox.Count);
            for (int r = 0; r < totalRows; r++)
            {
                string leftStr;
                if (r < leftRows.Count)
                {
                    var item = leftRows[r];
                    if (string.IsNullOrEmpty(item.Prefix) && string.IsNullOrEmpty(item.Title))
                    {
                        leftStr = new string(' ', 42);
                    }
                    else
                    {
                        string marker = item.IsSelected ? "* " : "  ";
                        int remaining = 42 - item.Prefix.Length - marker.Length;
                        string t = item.Title;
                        if (t.Length < remaining) t = t.PadRight(remaining);
                        leftStr = item.Prefix + marker + t;
                    }
                }
                else
                {
                    leftStr = new string(' ', 42);
                }

                string rightStr = r < rightBox.Count ? rightBox[r] : "";
                sb.AppendLine(leftStr + "  " + rightStr);
            }

            string exitPrompt = current.IsRoot ? "[ESC] Return to Master Menu" : "[ESC] Return to Previous Menu";
            AppendFooter(sb,
                "[UP/DOWN] Select Topic",
                "[ENTER] Open Topic",
                $"[1-{total}] Direct Jump",
                exitPrompt);
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  WIKI ARTICLE VIEW
        // ─────────────────────────────────────────────────────────────────────
        private string BuildWikiArticleViewScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, $"WIKI: {_currentWikiArticle.Id}. {_currentWikiArticle.Title}");
            sb.AppendLine();

            const int linesPerPage = 14;
            int totalLines = _currentWikiArticle.ContentLines.Count;
            int endLine = Math.Min(_wikiScrollOffset + linesPerPage, totalLines);

            for (int i = _wikiScrollOffset; i < endLine; i++)
            {
                sb.AppendLine("  " + _currentWikiArticle.ContentLines[i]);
            }
            for (int i = endLine - _wikiScrollOffset; i < linesPerPage; i++)
            {
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine($"  [Line {_wikiScrollOffset + 1}-{endLine} of {totalLines}]");

            AppendFooter(sb,
                "[UP/DOWN] Scroll Line",
                "[PGUP/PGDN] Scroll Page",
                "[ESC] Return to Topic Index");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  FIELD MANUAL MENU
        // ─────────────────────────────────────────────────────────────────────
        private string BuildFieldManualMenuScreen()
        {
            if (_manualNavStack.Count == 0) return BuildMasterMenuScreen();

            var current = _manualNavStack.Peek();
            var nodes = current.Nodes;
            int total = nodes.Count;

            var sb = new StringBuilder();
            AppendHeader(sb, current.ParentHeader);
            sb.AppendLine();
            AppendSectionHeader(sb, current.SubHeader);

            var leftRows = new List<(string Prefix, string Title, bool IsSelected)>();
            int endIdx = Math.Min(_manualScrollOffset + ManualPageSize, total);

            for (int i = _manualScrollOffset; i < endIdx; i++)
            {
                string prefix = $"  [{nodes[i].Code}] ";
                string title = nodes[i].Title;
                if (title.Length > 28) title = title.Substring(0, 25) + "...";
                leftRows.Add((prefix, title, i == _manualMenuSel));
                leftRows.Add(("", "", false));
            }
            for (int i = endIdx - _manualScrollOffset; i < ManualPageSize; i++)
            {
                leftRows.Add(("", "", false));
                leftRows.Add(("", "", false));
            }

            var rightBox = new List<string>();
            string explanation = "";
            if (_manualMenuSel >= 0 && _manualMenuSel < total)
            {
                explanation = string.IsNullOrEmpty(nodes[_manualMenuSel].Summary)
                    ? nodes[_manualMenuSel].Title
                    : nodes[_manualMenuSel].Summary;
            }

            if (!string.IsNullOrEmpty(explanation))
            {
                rightBox.Add("┌── [ TOPIC EXPLANATION ] ───────┐");
                const int contentWidth = 30;
                int startPos = 0;
                while (startPos < explanation.Length)
                {
                    int chunkLen = Math.Min(contentWidth, explanation.Length - startPos);
                    if (startPos + chunkLen < explanation.Length)
                    {
                        int spacePos = explanation.LastIndexOf(' ', startPos + chunkLen - 1, chunkLen);
                        if (spacePos > startPos) chunkLen = spacePos - startPos;
                    }
                    string line = explanation.Substring(startPos, chunkLen).Trim();
                    if (line.Length < contentWidth) line = line.PadRight(contentWidth);
                    rightBox.Add("│ " + line + " │");
                    startPos += chunkLen;
                    while (startPos < explanation.Length && explanation[startPos] == ' ') startPos++;
                }
                rightBox.Add("└────────────────────────────────┘");
            }

            int totalRows = Math.Max(leftRows.Count, rightBox.Count);
            for (int r = 0; r < totalRows; r++)
            {
                string leftStr;
                if (r < leftRows.Count)
                {
                    var item = leftRows[r];
                    if (string.IsNullOrEmpty(item.Prefix) && string.IsNullOrEmpty(item.Title))
                    {
                        leftStr = new string(' ', 42);
                    }
                    else
                    {
                        string marker = item.IsSelected ? "* " : "  ";
                        int remaining = 42 - item.Prefix.Length - marker.Length;
                        string t = item.Title;
                        if (t.Length < remaining) t = t.PadRight(remaining);
                        leftStr = item.Prefix + marker + t;
                    }
                }
                else
                {
                    leftStr = new string(' ', 42);
                }

                string rightStr = r < rightBox.Count ? rightBox[r] : "";
                sb.AppendLine(leftStr + "  " + rightStr);
            }

            int totalPages = (total + ManualPageSize - 1) / ManualPageSize;
            int curPage = (_manualScrollOffset / ManualPageSize) + 1;
            string scrollInfo = totalPages > 1 ? $" (PAGE {curPage}/{totalPages})" : "";

            if (current.IsRoot)
            {
                AppendFooter(sb,
                    "[UP/DOWN] Select Chapter" + scrollInfo,
                    "[ENTER] View Chapter Topics",
                    "[PGUP/PGDN] Scroll Chapters",
                    "[ESC] Return to Master Menu");
            }
            else
            {
                bool hasSubs = nodes.Count > 0 && _manualMenuSel < nodes.Count && nodes[_manualMenuSel].Subsections.Count > 0;
                AppendFooter(sb,
                    "[UP/DOWN] Select Topic" + scrollInfo,
                    hasSubs ? "[ENTER] View Sub-Topics" : "[ENTER] Read Section",
                    "[PGUP/PGDN] Scroll Topics",
                    "[ESC] Return Up");
            }

            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  FIELD MANUAL ARTICLE VIEW
        // ─────────────────────────────────────────────────────────────────────
        private string BuildFieldManualArticleViewScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, $"MANUAL: [{_currentManualArticle.Code}] {_currentManualArticle.Title}");
            sb.AppendLine();

            const int linesPerPage = 14;
            int totalLines = _manualWrappedLines.Count;
            int endLine = Math.Min(_manualArticleScrollOffset + linesPerPage, totalLines);

            for (int i = _manualArticleScrollOffset; i < endLine; i++)
            {
                sb.AppendLine("  " + _manualWrappedLines[i]);
            }
            for (int i = endLine - _manualArticleScrollOffset; i < linesPerPage; i++)
            {
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine($"  [Line {_manualArticleScrollOffset + 1}-{endLine} of {totalLines}]");

            AppendFooter(sb,
                $"Line {_manualArticleScrollOffset + 1}-{endLine} of {totalLines}",
                "[UP/DOWN] Scroll Line",
                "[PGUP/PGDN] Scroll Page",
                "[ESC] Return");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  SETTINGS MENU
        // ─────────────────────────────────────────────────────────────────────
        private string BuildSettingsMenuScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "SYSTEM SETTINGS & DETECTION THRESHOLDS", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "OPERATIONAL INTELLIGENCE PARAMETERS");

            string aiSummary = _aiModelProvider switch
            {
                1 => "Built-in Heuristics",
                2 => "Ollama Bridge",
                3 => "Gemini API",
                _ => "Custom Endpoint"
            };

            string[] options =
            {
                $"CONNECT TO PREDICTIVE AI MODEL       : [{aiSummary}]",
                $"ALLIED SIGINT CONFIDENCE THRESHOLD   : [{_sigintConfidence}]",
                $"STRIKE CONCENTRATION: WARSHIP COUNT  : [{_strikeCruiserThreshold} CRUISERS+]",
                $"STRIKE CONCENTRATION: SEARCH RADIUS  : [{_strikeHexRadius} HEXES ({_strikeHexRadius * 40} NM)]",
                $"AMPHIBIOUS ASSAULT SENSITIVITY       : [{_invasionSensitivity}% THRESHOLD]",
                $"SAVE CONFIGURATION TO CAMPAIGN VAULT : [{_wizardCampaignName}.wic]"
            };

            for (int i = 0; i < options.Length; i++)
            {
                if (i == _settingsSel)
                    sb.AppendLine($"  [{i + 1}]  * {options[i]}");
                else
                    sb.AppendLine($"  [{i + 1}]    {options[i]}");
                sb.AppendLine();
            }

            AppendFooter(sb,
                "[UP/DOWN] Select",
                "[ENTER] Open / Toggle",
                "[1] AI Setup",
                "[S] Save",
                "[ESC] Return");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  AI CONNECTION MENU
        // ─────────────────────────────────────────────────────────────────────
        private string BuildAiConnectionMenuScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "SYSTEM SETTINGS : PREDICTIVE AI MODEL CONNECTION", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "AI MODEL INFERENCE BACKEND CONFIGURATION");

            string providerStr = _aiModelProvider switch
            {
                1 => "Local Analytical Heuristics Engine [BUILT-IN]",
                2 => "Local LLM / Ollama Bridge [http://localhost:11434]",
                3 => "Google Gemini API Strategic Synthesis Engine",
                _ => "Custom Local Inference Endpoint"
            };
            string statusStr = _aiConnected ? "[ CONNECTED : PIPELINE READY ]" : "[ STANDBY / OFFLINE ]";

            string[] options =
            {
                "ACTIVE PROVIDER : " + providerStr,
                "ENDPOINT URL    : " + _aiEndpointUrl,
                "MODEL NAME      : " + _aiModelName,
                "STATUS MONITOR  : " + statusStr + " (Press [T] to Test)"
            };

            for (int i = 0; i < options.Length; i++)
            {
                if (i == _aiConnSel)
                    sb.AppendLine($"  [{i + 1}]  * {options[i]}");
                else
                    sb.AppendLine($"  [{i + 1}]    {options[i]}");
                sb.AppendLine();
            }

            AppendFooter(sb,
                "[UP/DOWN] Select",
                "[ENTER] Change / Toggle",
                "[T] Test Handshake",
                "[ESC] Return");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  EXIT CONFIRM MODAL
        // ─────────────────────────────────────────────────────────────────────
        private string BuildExitConfirmModal()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "TERMINATE SESSION CONFIRMATION");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("    ╔═══════════════════════════════════════════════════════════╗");
            sb.AppendLine("    ║  [ TERMINATE SESSION ]                                    ║");
            sb.AppendLine("    ╠═══════════════════════════════════════════════════════════╣");
            sb.AppendLine("    ║  CONFIRM: PURGE ALL IN-MEMORY INTELLIGENCE AND EXIT?      ║");
            sb.AppendLine("    ║                                                           ║");
            sb.AppendLine("    ║  [Y] YES - PURGE & SHUTDOWN       [N/ESC] CANCEL          ║");
            sb.AppendLine("    ╚═══════════════════════════════════════════════════════════╝");
            sb.AppendLine();
            AppendFooter(sb, "[Y] Yes, Purge & Shutdown", "[N/ESC] Cancel");
            return sb.ToString();
        }

        // ── Text wrap helper ─────────────────────────────────────────────────
        private static List<string> WrapText(string text, int width, int indent = 0)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(text)) { result.Add(""); return result; }
            string rem = text;
            bool first = true;
            while (!string.IsNullOrEmpty(rem))
            {
                int curWidth = first ? width : (width - indent);
                if (curWidth <= 0) curWidth = width;
                if (rem.Length <= curWidth)
                {
                    result.Add((first ? "" : new string(' ', indent)) + rem);
                    break;
                }
                int breakPos = rem.LastIndexOf(' ', curWidth);
                if (breakPos <= 0)
                {
                    // Single unbroken word exceeds curWidth: break at curWidth
                    breakPos = curWidth;
                }
                string chunk = rem.Substring(0, Math.Min(breakPos, rem.Length));
                result.Add((first ? "" : new string(' ', indent)) + chunk);
                rem = breakPos < rem.Length ? rem.Substring(breakPos).TrimStart() : "";
                first = false;
            }
            return result;
        }

        // ── Wiki Data Initialization ─────────────────────────────────────────
        private void InitializeWikiTree()
        {
            _wikiRoot = new WikiNode
            {
                Id = "ROOT",
                Title = "SYSTEM WIKI & ERROR REGISTRY",
                Description = "Root wiki documentation index"
            };

            var opDoctrine = new WikiNode
            {
                Id = "1",
                Title = "OPERATIONAL DOCTRINE & THEATER INTELLIGENCE",
                Description = "Combat Information Center workflow, PBEM intelligence lifecycle, tactical intercepts",
                ContentLines = new List<string>
                {
                    "1. OPERATIONAL DOCTRINE & THEATER INTELLIGENCE",
                    "----------------------------------------------------------------------",
                    "",
                    "1.1 THE COMBAT INFORMATION CENTER MISSION",
                    "WITP-CIC acts as the central intelligence clearinghouse for your theater",
                    "command in War in the Pacific: Admiral's Edition. At the end of each daily",
                    "PBEM turn exchange, raw telemetry logs and binary save states are synthesized",
                    "into actionable tactical intelligence.",
                    "",
                    "1.2 STRATEGIC PRIORITIES & CONTACT TRACKING",
                    "- High-Value Targets (HVT): Flattops, battleships, fleet oilers, and invasion",
                    "  transports receive top tracking priority.",
                    "- Strike Concentrations: Clusters of 3+ capital warships within 10 hexes are",
                    "  flagged as imminent carrier strike groups or surface bombardment forces.",
                    "- Port Concentrations: Buildups of naval tonnage in enemy anchorages signal",
                    "  amphibious preparations or fleet replenishment.",
                    "- Established Routes: Reconstructed convoy trajectories expose supply lanes.",
                    "",
                    "1.3 THEATER HEURISTICS & DRIFT ANALYSIS",
                    "Mathematical heuristics track theater power distribution shifts, enemy fleet centers",
                    "of gravity, and operational drift vectors across continuous campaigns."
                }
            };

            var techDetails = new WikiNode
            {
                Id = "2",
                Title = "TECHNICAL DETAILS",
                Description = "System architecture, War Room specs, ingestion lifecycle, controls, error registry"
            };

            var arch = new WikiNode
            {
                Id = "1",
                Title = "SYSTEM ARCHITECTURE & MEMORY SANDBOX",
                Description = "Core design principles, local-only execution, zero cloud leakage, memory purge",
                ContentLines = new List<string>
                {
                    "2.1 SYSTEM ARCHITECTURE & MEMORY SANDBOX",
                    "----------------------------------------------------------------------",
                    "",
                    "LOCAL EXECUTION & DATA SOVEREIGNTY (§1, §3)",
                    "WITP-CIC operates strictly as an offline, single-player Play-By-Email",
                    "(PBEM) intelligence assistant. The software executes locally on your",
                    "machine and never communicates over network sockets, external APIs, or",
                    "cloud storage. All operational telemetry remains strictly local.",
                    "",
                    "CAMPAIGN VAULT ISOLATION (§3)",
                    "Each campaign is encapsulated in a single compressed container file (.wic)",
                    "stored under 'C:\\WitP-CIC\\Campaigns'. Campaign data belongs solely to the",
                    "side commanded (Allied or Japanese). Cross-campaign data leakage is strictly",
                    "prohibited by the architecture.",
                    "",
                    "ABSOLUTE MEMORY PURGE (§3, §4.4)",
                    "When switching campaigns or terminating an interface session, WITP-CIC",
                    "executes an unconditional zeroing and deallocation of all internal graphs,",
                    "time-series buffers, and report caches. No residual state is retained in RAM."
                }
            };

            var cic = new WikiNode
            {
                Id = "2",
                Title = "COMBAT INFORMATION CENTER (WAR ROOM)",
                Description = "Intelligence summary, localized hex bearings, and theater heuristics",
                ContentLines = new List<string>
                {
                    "2.2 COMBAT INFORMATION CENTER (WAR ROOM) (§4.5)",
                    "----------------------------------------------------------------------",
                    "",
                    "OPTION 1: STRATEGIC INTELLIGENCE SUMMARY",
                    "- High-Value Targets (HVT): Monitors spotted enemy aircraft carriers (CV/CVL),",
                    "  battleships (BB), fleet oilers (AO), and amphibious transports (AP/AK).",
                    "- Strike Concentrations: Detects clusters of 3+ capital warships (cruisers+",
                    "  or battleships) operating within 10 hexes of each other.",
                    "- Port Concentrations: Tracks significant enemy tonnage anchored in ports.",
                    "- Battle Damage Claims: Tracks combat sinkings with fog-of-war skepticism.",
                    "- Ghost Fleet: Flags lost contacts whose tracks have gone cold.",
                    "",
                    "OPTION 2: GEOSPATIAL CONTACT LIST (SIGINT MAP)",
                    "Renders SIGINT radio intercepts and visual sightings as structured tabular",
                    "records with hex coordinates, localized range in hexes, and sector bearings",
                    "(e.g., '14 hexes ENE of Pearl Harbor HQ').",
                    "",
                    "OPTION 3: VISUAL GRAPHICS MATRIX (TBD)",
                    "Option 4 & 5: Reserved TBD domains.",
                    "Option 6: Mathematical Heuristics and Strategic Drift."
                }
            };

            var ingest = new WikiNode
            {
                Id = "3",
                Title = "DATA INGESTION & PBEM LIFECYCLE",
                Description = "Multi-directory report scanning, .pws save game ingestion, and .wic storage",
                ContentLines = new List<string>
                {
                    "2.3 DATA INGESTION & PBEM LIFECYCLE (§4.1, §5)",
                    "----------------------------------------------------------------------",
                    "",
                    "DETECTED GAME REPORT FILES",
                    "At each turn transition, WITP:AE generates operational text files:",
                    "- asigint_*.txt / jsigint_*.txt: Signals intelligence intercepts.",
                    "- aoperationsreport_*.txt: Daily theater operational summaries.",
                    "- combatreport_*.txt: Air, naval, and ground combat action reports.",
                    "- Combat_Events_*.txt: Summary log of combat engagements.",
                    "- wpae*.pws: Binary PBEM game save state files.",
                    "",
                    "MULTI-DIRECTORY SCANNING",
                    "In Setup Wizard Step 3, pressing [B] enables accumulating additional report",
                    "directories without overwriting previously configured paths.",
                    "",
                    "VAULT CONTAINER (.WIC)",
                    "All ingested reports, discovered file manifests, and turn indices are stored",
                    "in a single compressed container under 'C:\\WitP-CIC\\Campaigns\\<Name>.wic'."
                }
            };

            var controls = new WikiNode
            {
                Id = "4",
                Title = "NOSTROMO TERMINAL CONTROLS & HOTKEYS",
                Description = "Monochromatic green phosphor styling, audio cues, and keyboard navigation",
                ContentLines = new List<string>
                {
                    "2.4 NOSTROMO TERMINAL CONTROLS & HOTKEYS (§8)",
                    "----------------------------------------------------------------------",
                    "",
                    "VISUAL PALETTE (ALIEN MU/TH/UR)",
                    "- Dark Green (Code 2): Structural frames, borders (=, -), inactive text.",
                    "- Light Green (Code 10): Active titles, headers, values, prompt alerts.",
                    "- Inverted Highlight (Code 160): Active selection cursor indicator.",
                    "- Underline Navigation: Solid underline cursor beneath active selection.",
                    "",
                    "ACOUSTIC SOUNDSCAPE",
                    "- Relay Click: Crisp mechanical click on UP/DOWN/TAB navigation.",
                    "- Teletype Chime: Multi-tone electronic chirp on screen transitions.",
                    "",
                    "GLOBAL NAVIGATION HOTKEYS",
                    "- [UP / DOWN / TAB]: Move selection cursor.",
                    "- [ENTER]: Activate selected option or drill into topic.",
                    "- [ESC]: Return to previous menu or trigger Exit Confirmation modal.",
                    "- [B]: Browse directory or input alternate path.",
                    "- [A]: Open War Room module.",
                    "- [W]: Open Wiki module.",
                    "- [F]: Open Field Manual module.",
                    "- [S]: Open Settings module.",
                    "- [E]: Exit application."
                }
            };

            var errors = new WikiNode
            {
                Id = "5",
                Title = "SYSTEM ERROR CODE REGISTRY",
                Description = "Standardized 3-digit system error codes, causes, and user resolution actions"
            };

            var err101 = new WikiNode
            {
                Id = "1",
                Title = "[101] VAULT_NOT_FOUND",
                Description = "Campaign .wic container file missing or path unresolvable",
                ContentLines = new List<string>
                {
                    "ERROR CODE [101] - VAULT_NOT_FOUND (§13.1)",
                    "----------------------------------------------------------------------",
                    "",
                    "SUBSYSTEM: VAULT SERIALIZER / CAMPAIGN STORAGE",
                    "SEVERITY : CRITICAL",
                    "",
                    "DESCRIPTION & CAUSE",
                    "The application attempted to load a campaign vault container file (.wic)",
                    "from the configured repository path, but the file could not be found on disk.",
                    "1. The campaign file was moved, renamed, or deleted outside WITP-CIC.",
                    "2. The drive containing 'C:\\WitP-CIC\\Campaigns' is disconnected.",
                    "",
                    "ACTIONABLE RECOVERY PROCEDURES",
                    "1. Check repository directory 'C:\\WitP-CIC\\Campaigns' in File Explorer.",
                    "2. Press [N] to initialize a fresh vault container."
                }
            };

            var err102 = new WikiNode
            {
                Id = "2",
                Title = "[102] VAULT_CORRUPT",
                Description = "Magic bytes mismatch or decompression failure during vault read",
                ContentLines = new List<string>
                {
                    "ERROR CODE [102] - VAULT_CORRUPT (§13.2)",
                    "----------------------------------------------------------------------",
                    "",
                    "SUBSYSTEM: VAULT SERIALIZER / DECOMPRESSION ENGINE",
                    "SEVERITY : CRITICAL",
                    "",
                    "DESCRIPTION & CAUSE",
                    "The .wic vault container exists, but internal validation checks failed.",
                    "Magic header bytes do not match expected signature 'WIC\\x01'."
                }
            };

            var err201 = new WikiNode
            {
                Id = "3",
                Title = "[201] REPORT_DIR_MISSING",
                Description = "Configured WITP:AE game or archive report folder not found",
                ContentLines = new List<string>
                {
                    "ERROR CODE [201] - REPORT_DIR_MISSING (§13.3)",
                    "----------------------------------------------------------------------",
                    "",
                    "SUBSYSTEM: FILE SCANNER / INGESTION SUBSYSTEM",
                    "SEVERITY : HIGH",
                    "",
                    "DESCRIPTION & CAUSE",
                    "The report scanner attempted to read game turn text files from a directory",
                    "configured in Setup Wizard Step 3 or Settings, but the path does not exist."
                }
            };

            var err202 = new WikiNode
            {
                Id = "4",
                Title = "[202] PWS_READ_ERROR",
                Description = "Binary PBEM save file locked by game process or transmission truncated",
                ContentLines = new List<string>
                {
                    "ERROR CODE [202] - PWS_READ_ERROR (§13.4)",
                    "----------------------------------------------------------------------",
                    "",
                    "SUBSYSTEM: PWS INGESTION / WIN32 FILE I/O",
                    "SEVERITY : MEDIUM",
                    "",
                    "DESCRIPTION & CAUSE",
                    "An exclusive Win32 file lock or incomplete file copy prevented reading the",
                    "binary PBEM save file (wpae*.pws)."
                }
            };

            var err301 = new WikiNode
            {
                Id = "5",
                Title = "[301] PARSE_FORMAT_MISMATCH",
                Description = "Report syntax or column structure deviates from expected layout",
                ContentLines = new List<string>
                {
                    "ERROR CODE [301] - PARSE_FORMAT_MISMATCH (§13.5)",
                    "----------------------------------------------------------------------",
                    "",
                    "SUBSYSTEM: REPORT PARSERS (SIGINT / COMBAT / OPS)",
                    "SEVERITY : LOW / WARNING",
                    "",
                    "DESCRIPTION & CAUSE",
                    "A line in a game report text file could not be parsed according to standard",
                    "WITP:AE format schemas."
                }
            };

            var err401 = new WikiNode
            {
                Id = "6",
                Title = "[401] MEMORY_ALLOCATION_FAIL",
                Description = "Heap limit exceeded during large multi-year timeline graph rendering",
                ContentLines = new List<string>
                {
                    "ERROR CODE [401] - MEMORY_ALLOCATION_FAIL (§13.6)",
                    "----------------------------------------------------------------------",
                    "",
                    "SUBSYSTEM: MEMORY SANDBOX / GRAPH RENDERER",
                    "SEVERITY : HIGH",
                    "",
                    "DESCRIPTION & CAUSE",
                    "Memory allocation request exceeded available continuous address space while",
                    "generating extensive multi-year theater trend graphs."
                }
            };

            errors.SubTopics.AddRange(new[] { err101, err102, err201, err202, err301, err401 });
            techDetails.SubTopics.AddRange(new[] { arch, cic, ingest, controls, errors });

            var baseline = new WikiNode
            {
                Id = "3",
                Title = "BASELINE PRINCIPLES & GOVERNANCE PROTOCOLS",
                Description = "Authoritative acceptance rule, Git authority, Core Docs suite, continuous lessons learned",
                ContentLines = new List<string>
                {
                    "3. BASELINE PRINCIPLES & GOVERNANCE PROTOCOLS",
                    "----------------------------------------------------------------------",
                    "",
                    "3.1 THE BASELINE ACCEPTANCE RULE",
                    "\"Never mark anything done unless I say so.\" (Ozgur)",
                    "Passing unit tests, compiling static binaries, and executing commands are",
                    "engineering prerequisites, not completion sign-offs. Items transition to",
                    "completed status exclusively upon explicit direction from Ozgur.",
                    "",
                    "3.2 GIT REPOSITORY EXCLUSIVITY",
                    "The sole authoritative Git remote is origin -> https://github.com/oaltinyay70-cell/WITP-CIC-MUTHUR.git",
                    "for project witp-cic-rochefort on branch main. Upstream Witp-CIC2 is preserved read-only.",
                    "",
                    "3.3 CORE DOCS SOVEREIGNTY",
                    "Documentation covering Baseline Principles (docs/baseline_witp_cic.md), To-Do lists",
                    "(docs/todo.md), Lessons Learned (docs/lessons-learned.md), Architecture Decisions",
                    "(docs/architecture-decisions.md), Screen Flows, and Scope are Core Docs. They must",
                    "always be synchronized, date-stamped, and actively maintained.",
                    "",
                    "3.4 CONTINUOUS LESSONS LEARNED PROTOCOL",
                    "Following every user correction or design clarification, an entry is immediately",
                    "logged in docs/lessons-learned.md detailing the trigger, what was learned, and",
                    "anti-patterns (what NOT to repeat)."
                }
            };

            _wikiRoot.SubTopics.AddRange(new[] { opDoctrine, techDetails, baseline });
        }

        // ── Field Manual Data Initialization ─────────────────────────────────
        private void InitializeFieldManual()
        {
            var candidates = new[]
            {
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "manual", "witp_ae_manual.json"),
                System.IO.Path.Combine(Directory.GetCurrentDirectory(), "assets", "manual", "witp_ae_manual.json"),
                @"C:\WitP-CIC\assets\manual\witp_ae_manual.json",
                @"assets\manual\witp_ae_manual.json"
            };

            string? foundPath = candidates.FirstOrDefault(File.Exists);
            if (foundPath != null)
            {
                try
                {
                    string json = File.ReadAllText(foundPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var parsed = JsonSerializer.Deserialize<List<ManualSectionNode>>(json, options);
                    if (parsed != null && parsed.Count > 0)
                    {
                        _manualTree = parsed;
                        return;
                    }
                }
                catch { }
            }

            // Fallback manual structure
            _manualTree = new List<ManualSectionNode>
            {
                new ManualSectionNode
                {
                    Code = "1.0",
                    Title = "GAME OVERVIEW & BASIC CONCEPTS",
                    Summary = "Introduction to War in the Pacific Admiral's Edition rules and scale.",
                    ContentLines = new List<string>
                    {
                        "1.0 GAME OVERVIEW & BASIC CONCEPTS",
                        "War in the Pacific: Admiral's Edition is a strategic-operational simulation",
                        "of the Pacific Theater in World War II from December 1941 through 1945.",
                        "Scale: 40 nautical miles per hex, 1-day turn cycles."
                    }
                },
                new ManualSectionNode
                {
                    Code = "2.0",
                    Title = "NAVAL OPERATIONS & TASK FORCES",
                    Summary = "Naval task force creation, routing, patrol zones, and fuel consumption.",
                    ContentLines = new List<string>
                    {
                        "2.0 NAVAL OPERATIONS & TASK FORCES",
                        "Task forces consist of surface combatants, carriers, submarines, and transports.",
                        "Speed and endurance are computed per vessel displacement and bunker fuel."
                    }
                },
                new ManualSectionNode
                {
                    Code = "3.0",
                    Title = "AIR OPERATIONS & DOCTRINE",
                    Summary = "Air group ranges, CAP percentages, strike coordination, and pilot fatigue.",
                    ContentLines = new List<string>
                    {
                        "3.0 AIR OPERATIONS & DOCTRINE",
                        "Naval and land-based air groups execute CAP, strike, naval search, and ASW patrols."
                    }
                }
            };
        }
    }

    // ── Data types ───────────────────────────────────────────────────────────
    public class ReasoningTopic
    {
        public string Title { get; }
        public List<string> Lines { get; } = new();
        public ReasoningTopic(string title) { Title = title; }
    }

    public class WikiNode
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> ContentLines { get; set; } = new();
        public List<WikiNode> SubTopics { get; set; } = new();
        public bool IsLeaf => SubTopics.Count == 0;
    }

    public class ManualSectionNode
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<string> ContentLines { get; set; } = new();
        public List<ManualSectionNode> Subsections { get; set; } = new();
    }
}
