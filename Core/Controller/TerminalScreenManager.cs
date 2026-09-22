using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using MUTHUR6000.Core.Analytics;
using MUTHUR6000.Core.Database;
using MUTHUR6000.Core.Ingestion;

namespace MUTHUR6000.Core.Controller
{
    // ──────────────────────────────────────────────────────────────────────────
    //  Screen state machine — full C++ navigation tree (alpha-muthur / alpha3):
    //
    //  Boot  →  StartScreen (3 options: New / Continue / Exit)
    //               [N] NewCampaignWizard  (4 steps)
    //               [C] ContinueCampaign  (vault picker)
    //               [E] ExitConfirmModal
    //               → (after campaign loaded) → VaultIngestion → MasterMenu
    //  MasterMenu (5 items)
    //    [A] War Room  →  WarRoomPrimary (5 items)
    //                        [1] IntelligenceSummary (5 domains) -> PopUp -> Reasoning -> Drill-Down
    //                        [2] SigintMap (contacts log + [D] Dossier Search)
    //                        [3] TbdDomain (Phase 08 reserved domain)
    //                        [4] TpieDashboard (Heatmap + Vectors + [I] Intercepts)
    //                        [5] PredictiveModels (Models A-G -> PopUp -> Reasoning)
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
        IntelligenceSummary,
        IntelDetailPopUp,
        SigintMap,
        DossierSearch,
        TbdDomain,
        TpieDashboard,
        PredictiveModels,
        PredModelPopUp,
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
        ExitConfirmModal
    }

    public class TerminalScreenManager
    {
        // ── State ────────────────────────────────────────────────────────────
        private ScreenState _state = ScreenState.Boot;
        private ScreenState _prevState = ScreenState.WarRoomPrimary;
        private ScreenState _popUpCallerState = ScreenState.IntelligenceSummary;
        private ScreenState _exitReturnState = ScreenState.StartScreen;

        // ── Start Screen ─────────────────────────────────────────────────────
        private int _startScreenSel = 0;           // 0=New 1=Continue 2=Exit

        // ── New Campaign Wizard ──────────────────────────────────────────────
        private int _wizardStep = 1;               // 1-4
        private string _wizardSide = "Allied";     // "Allied" | "Japanese"
        private int _wizardSideSel = 0;            // 0=Allied 1=Japanese
        private string _wizardPassword = "";
        private readonly List<string> _wizardDirs = new()
        {
            @"C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE",
            @"C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE\archive",
            @"sample_data"
        };
        private string _wizardCampaignName = "WITP_CAMPAIGN";
        private string _wizardDirInput = "";
        private string _wizardNameInput = "";
        private bool _wizardAwaitingDirInput = false;
        private string _wizardPwdInput = "";

        // ── Continue Campaign ────────────────────────────────────────────────
        private readonly List<string> _vaultFiles = new();
        private int _continueSel = 0;
        private const string VaultDir = @"C:\WitP-CIC\Campaigns";

        // ── Master Menu Selection ────────────────────────────────────────────
        private int _masterMenuSel = 0;

        // ── War Room Selections ──────────────────────────────────────────────
        private int _warRoomSel = 0;
        private int _intelSumSel = 0;
        private int _intelPopUpSel = 0;
        private int _sigintMapSel = 0;
        private int _tpieSel = 0;
        private int _predModelsSel = 0;
        private int _predPopUpSel = 0;
        private int _reasoningTopicSel = 0;
        private int _heuristicsSel = 0;
        private int _heuristicsPopUpSel = 0;

        // ── Dossier Search ───────────────────────────────────────────────────
        private string _dossierQuery = "";
        private bool _dossierSearching = true;     // true=entering query, false=showing results
        private List<DiscoveredEntity> _dossierResults = new();

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
        private string _aiModelName = "nostromo-tpie-v1";
        private bool _aiConnected = true;

        public IngestionManifest Manifest { get; private set; } = new();
        public WarRoomAnalytics Analytics { get; private set; } = new();

        public ScreenState CurrentState => _state;

        public event Action<string, bool>? ScreenRenderRequested;

        // ── Ctor ─────────────────────────────────────────────────────────────
        public TerminalScreenManager()
        {
            InitializeWikiTree();
            InitializeFieldManual();
        }

        public void InitializeData(List<string>? dirs = null)
        {
            var searchPaths = dirs ?? new List<string>
            {
                @"C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE\archive",
                @"C:\WitP-CIC\Campaigns",
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample_data"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "archive"),
                System.IO.Path.Combine(Directory.GetCurrentDirectory(), "sample_data")
            };
            Manifest = FileScanner.ScanDirectories(searchPaths);
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

                case ScreenState.IntelligenceSummary:
                    HandleIntelligenceSummary(key);
                    break;

                case ScreenState.IntelDetailPopUp:
                    HandleIntelDetailPopUp(key);
                    break;

                case ScreenState.SigintMap:
                    HandleSigintMap(key);
                    break;

                case ScreenState.DossierSearch:
                    HandleDossierSearch(key);
                    break;

                case ScreenState.TbdDomain:
                    HandleTbdDomain(key);
                    break;

                case ScreenState.TpieDashboard:
                    HandleTpieDashboard(key);
                    break;

                case ScreenState.PredictiveModels:
                    HandlePredictiveModels(key);
                    break;

                case ScreenState.PredModelPopUp:
                    HandlePredModelPopUp(key);
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
            _wizardSide = "Allied";
            _wizardSideSel = 0;
            _wizardPassword = "";
            _wizardCampaignName = "WITP_CAMPAIGN";
            _wizardDirInput = "";
            _wizardNameInput = "";
            _wizardPwdInput = "";
            _wizardAwaitingDirInput = false;
            _state = ScreenState.NewCampaignWizard;
            RenderCurrentScreen();
        }

        private void LaunchContinueCampaign()
        {
            _vaultFiles.Clear();
            _continueSel = 0;
            try
            {
                Directory.CreateDirectory(VaultDir);
                foreach (var f in Directory.GetFiles(VaultDir, "*.wic"))
                    _vaultFiles.Add(System.IO.Path.GetFileName(f));
            }
            catch { }
            _state = ScreenState.ContinueCampaign;
            RenderCurrentScreen();
        }

        public bool IsAwaitingTextInput =>
            (_state == ScreenState.NewCampaignWizard && (_wizardStep == 2 || (_wizardStep == 3 && _wizardAwaitingDirInput) || _wizardStep == 4))
            || (_state == ScreenState.DossierSearch && _dossierSearching);

        // ── New Campaign Wizard ──────────────────────────────────────────────
        private void HandleNewCampaignWizard(System.Windows.Input.Key key)
        {
            switch (_wizardStep)
            {
                case 1: HandleWizardStep1(key); break;
                case 2: HandleWizardStep2(key); break;
                case 3: HandleWizardStep3(key); break;
                case 4: HandleWizardStep4(key); break;
            }
        }

        private void HandleWizardStep1(System.Windows.Input.Key key)
        {
            switch (key)
            {
                case System.Windows.Input.Key.A:
                    _wizardSide = "Allied"; _wizardSideSel = 0;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.J:
                    _wizardSide = "Japanese"; _wizardSideSel = 1;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    _wizardSideSel = (_wizardSideSel - 1 + 2) % 2;
                    _wizardSide = _wizardSideSel == 0 ? "Allied" : "Japanese";
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.Tab:
                    _wizardSideSel = (_wizardSideSel + 1) % 2;
                    _wizardSide = _wizardSideSel == 0 ? "Allied" : "Japanese";
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                    _wizardStep = 2;
                    _wizardPwdInput = "";
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.StartScreen;
                    RenderCurrentScreen();
                    break;
            }
        }

        private void HandleWizardStep2(System.Windows.Input.Key key)
        {
            if (key == System.Windows.Input.Key.Escape)
            {
                _wizardStep = 1;
                RenderCurrentScreen();
                return;
            }
            if (key == System.Windows.Input.Key.Enter)
            {
                _wizardPassword = _wizardPwdInput;
                _wizardStep = 3;
                RenderCurrentScreen();
                return;
            }
            if (key == System.Windows.Input.Key.Back)
            {
                if (_wizardPwdInput.Length > 0)
                {
                    _wizardPwdInput = _wizardPwdInput.Substring(0, _wizardPwdInput.Length - 1);
                    RenderCurrentScreen();
                }
                return;
            }
        }

        private void HandleWizardStep3(System.Windows.Input.Key key)
        {
            if (_wizardAwaitingDirInput)
            {
                if (key == System.Windows.Input.Key.Escape)
                {
                    _wizardAwaitingDirInput = false;
                    _wizardDirInput = "";
                    RenderCurrentScreen();
                    return;
                }
                if (key == System.Windows.Input.Key.Enter)
                {
                    if (!string.IsNullOrWhiteSpace(_wizardDirInput)) _wizardDirs.Add(_wizardDirInput.Trim());
                    _wizardDirInput = "";
                    _wizardAwaitingDirInput = false;
                    RenderCurrentScreen();
                    return;
                }
                if (key == System.Windows.Input.Key.Back)
                {
                    if (_wizardDirInput.Length > 0)
                    {
                        _wizardDirInput = _wizardDirInput.Substring(0, _wizardDirInput.Length - 1);
                        RenderCurrentScreen();
                    }
                    return;
                }
                return;
            }

            switch (key)
            {
                case System.Windows.Input.Key.Enter:
                    InitializeData(_wizardDirs);
                    _wizardStep = 4;
                    _wizardNameInput = "";
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.B:
                    _wizardAwaitingDirInput = true;
                    _wizardDirInput = "";
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Escape:
                    _wizardStep = 2;
                    RenderCurrentScreen();
                    break;
            }
        }

        private void HandleWizardStep4(System.Windows.Input.Key key)
        {
            if (key == System.Windows.Input.Key.Escape)
            {
                _wizardStep = 3;
                RenderCurrentScreen();
                return;
            }
            if (key == System.Windows.Input.Key.Enter)
            {
                if (!string.IsNullOrWhiteSpace(_wizardNameInput))
                    _wizardCampaignName = _wizardNameInput.Trim();
                _state = ScreenState.VaultIngestion;
                RenderCurrentScreen();
                return;
            }
            if (key == System.Windows.Input.Key.Back)
            {
                if (_wizardNameInput.Length > 0)
                {
                    _wizardNameInput = _wizardNameInput.Substring(0, _wizardNameInput.Length - 1);
                    RenderCurrentScreen();
                }
                return;
            }
        }

        // ── Text character input (called from MainWindow) ────────────────────
        public void HandleCharInput(char c)
        {
            if (c < 32 || c > 126) return;

            if (_state == ScreenState.NewCampaignWizard)
            {
                if (_wizardStep == 2)
                {
                    if (_wizardPwdInput.Length < 40)
                    {
                        _wizardPwdInput += c;
                        RenderCurrentScreen();
                    }
                }
                else if (_wizardStep == 3 && _wizardAwaitingDirInput)
                {
                    if (_wizardDirInput.Length < 120)
                    {
                        _wizardDirInput += c;
                        RenderCurrentScreen();
                    }
                }
                else if (_wizardStep == 4)
                {
                    if (_wizardNameInput.Length < 40)
                    {
                        _wizardNameInput += c;
                        RenderCurrentScreen();
                    }
                }
            }
            else if (_state == ScreenState.DossierSearch && _dossierSearching)
            {
                if (_dossierQuery.Length < 30)
                {
                    _dossierQuery += c;
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
                    InitializeData();
                    _wizardCampaignName = System.IO.Path.GetFileNameWithoutExtension(_vaultFiles[_continueSel]);
                    _state = ScreenState.VaultIngestion;
                    RenderCurrentScreen();
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
                    _state = ScreenState.IntelligenceSummary;
                    _intelSumSel = 0;
                    break;
                case 1:
                    _state = ScreenState.SigintMap;
                    _sigintMapSel = 0;
                    break;
                case 2:
                    _state = ScreenState.TbdDomain;
                    break;
                case 3:
                    _state = ScreenState.TpieDashboard;
                    _tpieSel = 0;
                    break;
                case 4:
                    _state = ScreenState.PredictiveModels;
                    _predModelsSel = 0;
                    break;
                case 5:
                    _state = ScreenState.Heuristics;
                    _heuristicsSel = 0;
                    break;
            }
            RenderCurrentScreen();
        }

        // ── Intelligence Summary Screen ──────────────────────────────────────
        private void HandleIntelligenceSummary(System.Windows.Input.Key key)
        {
            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.WarRoomPrimary;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    _intelSumSel = (_intelSumSel - 1 + 5) % 5;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.Tab:
                    _intelSumSel = (_intelSumSel + 1) % 5;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                    OpenIntelDetailPopUp(_intelSumSel);
                    break;

                case System.Windows.Input.Key.D1: OpenIntelDetailPopUp(0); break;
                case System.Windows.Input.Key.D2: OpenIntelDetailPopUp(1); break;
                case System.Windows.Input.Key.D3: OpenIntelDetailPopUp(2); break;
                case System.Windows.Input.Key.D4: OpenIntelDetailPopUp(3); break;
                case System.Windows.Input.Key.D5: OpenIntelDetailPopUp(4); break;
            }
        }

        private void OpenIntelDetailPopUp(int category)
        {
            _intelSumSel = category;
            _popUpBullets.Clear();
            _popUpReasonings.Clear();
            _popUpConfidences.Clear();

            switch (category)
            {
                case 0:
                    _popUpTitle = "HIGH-VALUE TARGET (HVT) LOCALIZATION MANIFEST";
                    foreach (var h in Analytics.HighValueTargets)
                    {
                        _popUpBullets.Add($"[{h.Type}] {h.Name} at {h.Location} ({h.HexX},{h.HexY}) [{h.Date}] - {h.Status}");
                        _popUpConfidences.Add(h.ConfidencePct);
                        string datesStr = h.AllDates.Count > 0 ? string.Join(", ", h.AllDates) : h.Date;
                        int freq = Math.Max(1, h.AllDates.Count);
                        string freqStr = freq > 1
                            ? $"OBSERVATION FREQUENCY: {freq} distinct sightings recorded across campaign turns."
                            : "OBSERVATION FREQUENCY: Single verified sighting recorded.";
                        string latestSeen = string.IsNullOrEmpty(h.LastSeenDate) ? h.Date : h.LastSeenDate;
                        var r = new List<string> {
                            "TELEMETRY SOURCE: SIGINT Intercept Report & Radio Direction Finding",
                            $"LOCALIZED POSITION: Hex {h.HexX},{h.HexY} ({h.Location})",
                            freqStr,
                            $"CHRONOLOGY / RECENCY: Tracked until {latestSeen} [Dates seen: {datesStr}]",
                            $"CLASSIFICATION: Vessel designated High-Value Capital Asset ({h.Type}).",
                            "TACTICAL REASONING: Highly prioritized for Allied submarine & naval air interdiction.",
                            "INTERCEPT SIGNIFICANCE: Critical strategic asset monitored for battle group disposition."
                        };
                        r.AddRange(h.MathBreakdown);
                        _popUpReasonings.Add(r);
                    }
                    break;

                case 1:
                    _popUpTitle = "NAVAL STRIKE CONCENTRATION CLUSTERS";
                    foreach (var sc in Analytics.ModelFConcentrations)
                    {
                        _popUpBullets.Add($"{sc.ClusterName} [Hex {sc.CenterHexX},{sc.CenterHexY}] : {sc.WarshipCount} Warships | Threat: {sc.ThreatLevel}");
                        _popUpReasonings.Add(new List<string> {
                            "TELEMETRY SOURCE: Sighting Reports, Combat Events & Hex Grid Spatial Cluster Scan",
                            $"LOCALIZED POSITION: Center Hex {sc.CenterHexX},{sc.CenterHexY} ({sc.ClusterName}) [Radius: 10 hexes]",
                            $"OBSERVATION FREQUENCY: Spatial cluster aggregated from {sc.ConstituentSightings.Count} contact sightings.",
                            "CHRONOLOGY / RECENCY: Active battle group evaluated for current operational turn.",
                            $"FORCE COMPOSITION: {sc.WarshipCount} confirmed surface warships with cruiser/carrier screen.",
                            $"THREAT ASSESSMENT: {sc.ThreatLevel} threat level indicating active offensive surface battle group.",
                            "OPERATIONAL TARGET: Primary screening force for amphibious landings in adjacent theater.",
                            "SPATIAL REASONING: Warship concentration exceeds threshold within 10-hex radius."
                        });
                        _popUpConfidences.Add(0);
                    }
                    break;

                case 2:
                    _popUpTitle = "PORT & ANCHORAGE REPLENISHMENT CONCENTRATIONS";
                    foreach (var p in Analytics.PortConcentrations)
                    {
                        _popUpBullets.Add($"{p.PortName} (Hex {p.HexX},{p.HexY}): {p.WarshipCount} Warships, {p.MerchantCount} Transports");
                        _popUpReasonings.Add(new List<string> {
                            "TELEMETRY SOURCE: Operational Naval Reconnaissance & Harbor Sightings",
                            $"LOCALIZED POSITION: Anchorage facility at Hex {p.HexX},{p.HexY} ({p.PortName})",
                            "OBSERVATION FREQUENCY: Verified harbor reconnaissance scan.",
                            "CHRONOLOGY / RECENCY: Active staging base in current operational theater.",
                            $"LOGISTICAL REASONING: High ratio of merchant/transport hulls ({p.MerchantCount}) indicates forward supply/troop embarkation.",
                            $"WARSHIP PROTECTION: Screened by {p.WarshipCount} combatants.",
                            "ACTIONABLE ADVICE: Target with long-range air strikes or deploy submarine barrier lines."
                        });
                        _popUpConfidences.Add(0);
                    }
                    break;

                case 3:
                    _popUpTitle = "ESTABLISHED ENEMY LOGISTICAL & INVASION CORRIDORS";
                    foreach (var r in Analytics.ModelGRoutes)
                    {
                        _popUpBullets.Add($"{r.CorridorName} : {r.DeparturePoint} -> {r.DestinationPoint} ({r.VesselTypes}) | {r.Confidence}");
                        _popUpReasonings.Add(new List<string> {
                            "TELEMETRY SOURCE: Model G Repeated Waypoint Graph Analysis",
                            $"LOCALIZED POSITION: Shipping vector {r.DeparturePoint} -> {r.DestinationPoint}",
                            $"OBSERVATION FREQUENCY: {r.ObservationCount} repeated waypoint sightings along maritime channel.",
                            "CHRONOLOGY / RECENCY: Persistent logistical highway active across campaign history.",
                            $"TRAFFIC PROFILE: Utilized primarily by {r.VesselTypes} hulls.",
                            $"CONFIDENCE RATING: {r.Confidence} based on multi-turn sighting frequencies.",
                            "INTERDICTION REASONING: Prime ambush route for Allied submarine warfare & carrier strike ops."
                        });
                        _popUpConfidences.Add(0);
                    }
                    break;

                case 4:
                    _popUpTitle = "BATTLE DAMAGE ASSESSMENT & CLAIMS REVISION REGISTER";
                    var claims = Analytics.ModelEClaims.Claims;
                    if (claims.Count == 0)
                    {
                        _popUpBullets.Add("NO HOSTILE COMBAT DAMAGE CLAIMS RECORDED FOR THIS TURN RANGE.");
                        _popUpReasonings.Add(new List<string> {
                            "INTELLIGENCE STATUS: No hostile vessels currently identified taking damage or sunk in verified combat reports.",
                            "ASSESSMENT CRITERIA: Fog-of-war claims register tracks enemy losses only.",
                            "FRIENDLY STATUS: Friendly unit casualties are managed exclusively through operational commands."
                        });
                        _popUpConfidences.Add(0);
                    }
                    else
                    {
                        foreach (var c in claims)
                        {
                            _popUpBullets.Add($"{c.ShipName} : {c.Assessment} ({c.Reason}) [{c.DateYYMMDD}]");
                            _popUpReasonings.Add(new List<string> {
                                "TELEMETRY SOURCE: Combat Action Report Claims vs Operations Sighting Reconciliation",
                                $"LOCALIZED POSITION: Target vessel {c.ShipName} ({(c.IsAllied ? "Allied Target" : "Hostile Japanese Target")})",
                                $"OBSERVATION FREQUENCY: Single action report claim submitted on turn {c.DateYYMMDD}.",
                                $"CHRONOLOGY / RECENCY: Action evaluated for turn {c.DateYYMMDD}.",
                                $"ASSESSMENT REASONING: Initial action claim evaluated as {c.Assessment}.",
                                $"EVIDENCE CRITERIA: {c.Reason}.",
                                "GROUND TRUTH VERIFICATION: Entity reconciled against known vessel tracking register.",
                                "DEDUCTION: Overclaim adjusted to prevent misleading strategic planning."
                            });
                            _popUpConfidences.Add(0);
                        }
                    }
                    break;
            }

            _popUpCallerState = ScreenState.IntelligenceSummary;
            _state = ScreenState.IntelDetailPopUp;
            _intelPopUpSel = 0;
            RenderCurrentScreen();
        }

        private void HandleIntelDetailPopUp(System.Windows.Input.Key key)
        {
            int total = _popUpBullets.Count;
            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.IntelligenceSummary;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    if (total > 0) { _intelPopUpSel = Math.Max(0, _intelPopUpSel - 1); RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Down:
                    if (total > 0) { _intelPopUpSel = Math.Min(total - 1, _intelPopUpSel + 1); RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Tab:
                    if (total > 0) { _intelPopUpSel = (_intelPopUpSel + 1) % total; RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Enter:
                    if (total > 0 && _intelPopUpSel < total)
                        OpenReasoningModal(_popUpBullets[_intelPopUpSel],
                                           _popUpReasonings[_intelPopUpSel],
                                           _intelPopUpSel < _popUpConfidences.Count ? _popUpConfidences[_intelPopUpSel] : 0,
                                           ScreenState.IntelDetailPopUp);
                    break;
            }
        }

        // ── SIGINT Map Screen ────────────────────────────────────────────────
        private void HandleSigintMap(System.Windows.Input.Key key)
        {
            var logs = Analytics.GeospatialLog;
            int total = logs.Count;

            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.WarRoomPrimary;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.D:
                    _dossierQuery = "";
                    _dossierSearching = true;
                    _dossierResults.Clear();
                    _state = ScreenState.DossierSearch;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    if (total > 0) { _sigintMapSel = Math.Max(0, _sigintMapSel - 1); RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Down:
                    if (total > 0) { _sigintMapSel = Math.Min(total - 1, _sigintMapSel + 1); RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Tab:
                    if (total > 0) { _sigintMapSel = (_sigintMapSel + 1) % total; RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Enter:
                    if (total > 0 && _sigintMapSel < total)
                    {
                        var ent = logs[_sigintMapSel];
                        string dStr = string.IsNullOrEmpty(ent.LastSeenDate) ? ent.DateYYMMDD : ent.LastSeenDate;
                        string datesStr = ent.AllDates.Count > 0 ? string.Join(", ", ent.AllDates) : dStr;
                        int freq = Math.Max(1, ent.AllDates.Count);
                        string freqStr = freq > 1
                            ? $"OBSERVATION FREQUENCY: {freq} distinct sightings recorded across campaign turns."
                            : "OBSERVATION FREQUENCY: Single verified sighting recorded.";
                        string bulletTitle = $"{dStr} {ent.EventType} Hex {ent.HexX},{ent.HexY}";
                        var r = new List<string> {
                            "TELEMETRY SOURCE: Operational Sighting & Decrypted Log",
                            $"LOCALIZED POSITION: Hex {ent.HexX},{ent.HexY} ({ent.LocationName}) | Range/Bearing: {ent.BearingFromHQ}",
                            freqStr,
                            $"CHRONOLOGY / RECENCY: Tracked until {dStr} [Dates seen: {datesStr}]",
                            $"OPERATIONAL DETAILS: {ent.Details}",
                            "TACTICAL REASONING: High priority geospatial intercept logged in naval sector.",
                            "INTERCEPT SIGNIFICANCE: Cross-referenced with active enemy task force trajectories."
                        };
                        r.AddRange(ent.MathBreakdown);
                        OpenReasoningModal(bulletTitle, r, ent.ConfidencePct, ScreenState.SigintMap);
                    }
                    break;
            }
        }

        // ── Dossier Search Modal ─────────────────────────────────────────────
        private void HandleDossierSearch(System.Windows.Input.Key key)
        {
            if (_dossierSearching)
            {
                if (key == System.Windows.Input.Key.Escape)
                {
                    _state = ScreenState.SigintMap;
                    RenderCurrentScreen();
                    return;
                }
                if (key == System.Windows.Input.Key.Back)
                {
                    if (_dossierQuery.Length > 0)
                    {
                        _dossierQuery = _dossierQuery.Substring(0, _dossierQuery.Length - 1);
                        RenderCurrentScreen();
                    }
                    return;
                }
                if (key == System.Windows.Input.Key.Enter)
                {
                    _dossierResults = Analytics.SearchDossier(_dossierQuery);
                    _dossierSearching = false;
                    RenderCurrentScreen();
                    return;
                }
            }
            else
            {
                if (key == System.Windows.Input.Key.Escape || key == System.Windows.Input.Key.Enter)
                {
                    _state = ScreenState.SigintMap;
                    RenderCurrentScreen();
                }
            }
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

        // ── TPIE Dashboard ───────────────────────────────────────────────────
        private void HandleTpieDashboard(System.Windows.Input.Key key)
        {
            var targets = Analytics.ModelATargets;
            int maxShow = Math.Min(5, targets.Count);

            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.WarRoomPrimary;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    if (maxShow > 0) { _tpieSel = Math.Max(0, _tpieSel - 1); RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Down:
                    if (maxShow > 0) { _tpieSel = Math.Min(maxShow - 1, _tpieSel + 1); RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Tab:
                    if (maxShow > 0) { _tpieSel = (_tpieSel + 1) % maxShow; RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.I:
                    OpenInterceptWindowsModal();
                    break;

                case System.Windows.Input.Key.Enter:
                    if (_tpieSel < targets.Count)
                    {
                        var t = targets[_tpieSel];
                        string probStr = $"{t.Probability * 100:F1}% Probability";
                        var r = new List<string> {
                            "PREDICTIVE ENGINE: Model A — Weighted Evidence Accumulator (Heuristic Bayes)",
                            $"TARGET OBJECTIVE: {t.Destination} [{probStr} - {t.ThreatLevel}]",
                            "MATHEMATICAL REASONING: Score >= 15 yields >90% probability threshold alert.",
                            $"INTELLIGENCE EVIDENCE: {t.Assessment}",
                            "RECENCY DECAY: Applied temporal decay factor 0.9^delta_t across observed turns."
                        };
                        foreach (var u in t.LoadedUnits) r.Add($"  + EMBARKED UNIT: {u}");
                        foreach (var s in t.RadioTransmissions) r.Add($"  + RADIO SIGNAL: {s}");
                        OpenReasoningModal($"{t.Destination} ({probStr})", r, 0, ScreenState.TpieDashboard);
                    }
                    break;
            }
        }

        private void OpenInterceptWindowsModal()
        {
            var trajs = Analytics.ModelBTrajectories;
            _popUpBullets.Clear();
            _popUpReasonings.Clear();
            _popUpConfidences.Clear();
            _popUpTitle = "TPIE INTERCEPT WINDOWS & PATROL ZONES";

            if (trajs.Count == 0)
            {
                _popUpBullets.Add("NO ACTIVE FLEET TRAJECTORIES LOCALIZED TO COMPUTE INTERCEPT WINDOWS.");
                _popUpReasonings.Add(new List<string> { "INTELLIGENCE STATUS: No trajectory data available." });
                _popUpConfidences.Add(0);
            }
            else
            {
                foreach (var t in trajs.Take(11))
                {
                    string hexStr = $"{t.ProjectedHexX},{t.ProjectedHexY}";
                    _popUpBullets.Add($"{t.TargetDestination,-22} Proj: {hexStr,-14}+/-2.5 Hex  {t.InterceptWindow}");
                    _popUpReasonings.Add(new List<string> {
                        $"MATHEMATICAL FORMULATION: Dead Reckoning T+1 (x_next = x0 + v*cos(theta), y_next = y0 + v*sin(theta))",
                        $"CONTACT: {t.Description}",
                        $"PROJECTED POSITION: Hex {t.ProjectedHexX},{t.ProjectedHexY} (Confidence: +/- 2.5 hexes)",
                        $"ESTIMATED DESTINATION: {t.TargetDestination}",
                        $"INTERCEPT WINDOW: {t.InterceptWindow}"
                    });
                    _popUpConfidences.Add(0);
                }
            }

            _popUpCallerState = ScreenState.TpieDashboard;
            _state = ScreenState.PredModelPopUp;
            _predPopUpSel = 0;
            RenderCurrentScreen();
        }

        // ── Predictive Models Screen ─────────────────────────────────────────
        private void HandlePredictiveModels(System.Windows.Input.Key key)
        {
            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = ScreenState.WarRoomPrimary;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    _predModelsSel = Math.Max(0, _predModelsSel - 1);
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Down:
                    _predModelsSel = Math.Min(6, _predModelsSel + 1);
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Tab:
                    _predModelsSel = (_predModelsSel + 1) % 7;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Enter:
                    OpenPredModelPopUp(_predModelsSel);
                    break;

                case System.Windows.Input.Key.A: OpenPredModelPopUp(0); break;
                case System.Windows.Input.Key.B: OpenPredModelPopUp(1); break;
                case System.Windows.Input.Key.C: OpenPredModelPopUp(2); break;
                case System.Windows.Input.Key.D: OpenPredModelPopUp(3); break;
                case System.Windows.Input.Key.E: OpenPredModelPopUp(4); break;
                case System.Windows.Input.Key.F: OpenPredModelPopUp(5); break;
                case System.Windows.Input.Key.G: OpenPredModelPopUp(6); break;
            }
        }

        private void OpenPredModelPopUp(int modelIdx)
        {
            _predModelsSel = modelIdx;
            _popUpBullets.Clear();
            _popUpReasonings.Clear();
            _popUpConfidences.Clear();

            switch (modelIdx)
            {
                case 0:
                    _popUpTitle = "MODEL A: AMPHIBIOUS ASSAULT PROBABILITY ENGINE";
                    foreach (var t in Analytics.ModelATargets)
                    {
                        _popUpBullets.Add($"{t.Destination} | Prob: {t.Probability * 100:F1}% | Threat: {t.ThreatLevel}");
                        _popUpReasonings.Add(new List<string> {
                            "MATHEMATICAL FORMULATION: Weighted Evidence Accumulator P = min(0.99, Score / 15.0)",
                            $"TARGET: {t.Destination} ({t.ThreatLevel})",
                            $"ASSESSMENT EVIDENCE: {t.Assessment}",
                            "TEMPORAL FACTOR: Recency decay 0.9^delta_t applied across turn delta."
                        });
                        _popUpConfidences.Add(0);
                    }
                    break;

                case 1:
                    _popUpTitle = "MODEL B: FLEET TRAJECTORY & INTERCEPT PROJECTION";
                    foreach (var tr in Analytics.ModelBTrajectories)
                    {
                        string hexStr = $"{tr.StartHexX},{tr.StartHexY} -> {tr.ProjectedHexX},{tr.ProjectedHexY}";
                        _popUpBullets.Add($"{tr.Description} [{hexStr}] {tr.Heading} @ {tr.SpeedKnots} kts");
                        _popUpReasonings.Add(new List<string> {
                            "MATHEMATICAL FORMULATION: Dead Reckoning T+1 (x_next = x0 + v*cos(theta), y_next = y0 + v*sin(theta))",
                            $"CONTACT: {tr.Description}",
                            $"PROJECTED POSITION: Hex {tr.ProjectedHexX},{tr.ProjectedHexY} (Confidence: +/- 2.5 hexes)",
                            $"ESTIMATED DESTINATION: {tr.TargetDestination}",
                            $"INTERCEPT WINDOW: {tr.InterceptWindow}"
                        });
                        _popUpConfidences.Add(0);
                    }
                    break;

                case 2:
                    _popUpTitle = "MODEL C: AIR SUPERIORITY & COMBAT LOSS FORECASTER";
                    var ac = Analytics.ModelCAttrition;
                    _popUpBullets.Add($"Allied Losses: {ac.TotalAlliedLosses} | Japanese Losses: {ac.TotalJapaneseLosses}");
                    _popUpReasonings.Add(new List<string> {
                        "MATHEMATICAL FORMULATION: Exponential Moving Average (EMA) Attrition Decay (alpha = 0.3)",
                        $"COMBAT AIR LOSSES TABULATION: Allied Air Losses: {ac.TotalAlliedLosses} aircraft",
                        $"OPPOSING AIR LOSSES: Japanese Air Losses: {ac.TotalJapaneseLosses} aircraft",
                        $"SUPERIORITY PROJECTION: {ac.Forecast}"
                    });
                    _popUpConfidences.Add(0);
                    foreach (var ace in ac.TopAces)
                    {
                        _popUpBullets.Add($"Allied Ace: {ace.PilotName} ({ace.Squadron}) - {ace.KillNumber} confirmed kills");
                        _popUpReasonings.Add(new List<string> {
                            "TELEMETRY SOURCE: Combat Events Dogfight & Air-to-Air Resolution Log",
                            $"PILOT: {ace.PilotName} | SQUADRON: {ace.Squadron}",
                            $"CONFIRMED AIR KILLS: {ace.KillNumber} hostile aircraft downed",
                            "OPERATIONAL STATUS: Ace pilot status active."
                        });
                        _popUpConfidences.Add(0);
                    }
                    break;

                case 3:
                    _popUpTitle = "MODEL D: LOGISTICAL & OPERATIONAL PAUSE DETECTOR";
                    var p = Analytics.ModelDLogistics;
                    foreach (var a in p.ActivePauses)
                    {
                        _popUpBullets.Add($"{a.EventType} [{a.TfId}] -> {a.Base} ({a.Details})");
                        _popUpReasonings.Add(new List<string> {
                            $"TELEMETRY SOURCE: Operations Report Logistical Status ({a.DateYYMMDD})",
                            $"TASK FORCE: {a.TfId} | BASE: {a.Base}",
                            $"EVENT TYPE: {a.EventType}",
                            $"OPERATIONAL PAUSE REASONING: {a.Details}",
                            $"PAUSE WINDOW ASSESSMENT: {p.PauseWindow}"
                        });
                        _popUpConfidences.Add(0);
                    }
                    if (p.ActivePauses.Count == 0)
                    {
                        _popUpBullets.Add("NO ACTIVE LOGISTICAL PAUSES DETECTED.");
                        _popUpReasonings.Add(new List<string> { "INTELLIGENCE STATUS: All task forces operational." });
                        _popUpConfidences.Add(0);
                    }
                    break;

                case 4:
                    _popUpTitle = "MODEL E: BATTLE CLAIMS & DAMAGE REVISION REGISTER";
                    var oppClaims = Analytics.ModelEClaims.Claims;
                    if (oppClaims.Count == 0)
                    {
                        _popUpBullets.Add("NO HOSTILE COMBAT DAMAGE CLAIMS RECORDED FOR THIS TURN RANGE.");
                        _popUpReasonings.Add(new List<string> {
                            "INTELLIGENCE STATUS: No hostile combat claims currently pending revision.",
                            "METHODOLOGY: Model E tracks enemy BDA and ghost ships only.",
                            "FRIENDLY STATUS: Friendly forces managed via operational fleet screens."
                        });
                        _popUpConfidences.Add(0);
                    }
                    else
                    {
                        foreach (var c in oppClaims)
                        {
                            _popUpBullets.Add($"{c.ShipName} : {c.Assessment} ({c.Reason})");
                            _popUpReasonings.Add(new List<string> {
                                "TELEMETRY SOURCE: Combat Action Report Claims vs Operations Sighting Reconciliation",
                                $"VESSEL: {c.ShipName} | DATE: {c.DateYYMMDD}",
                                $"REVISED ASSESSMENT: {c.Assessment}",
                                $"RECONCILIATION CRITERIA: {c.Reason}",
                                "GROUND TRUTH: Confirmed against entity persistence layer."
                            });
                            _popUpConfidences.Add(0);
                        }
                    }
                    break;

                case 5:
                    _popUpTitle = "MODEL F: STRIKE CONCENTRATION DETECTOR";
                    foreach (var sc in Analytics.ModelFConcentrations)
                    {
                        _popUpBullets.Add($"{sc.ClusterName} [Hex {sc.CenterHexX},{sc.CenterHexY}] : {sc.WarshipCount} Warships");
                        _popUpReasonings.Add(new List<string> {
                            "MATHEMATICAL FORMULATION: Hex Grid Spatial Density Clustering (Radius = 10 hexes)",
                            $"CLUSTER: {sc.ClusterName} at Hex {sc.CenterHexX},{sc.CenterHexY}",
                            $"FORCE DENSITY: {sc.WarshipCount} surface warships detected.",
                            $"THREAT RATING: {sc.ThreatLevel} (Cruiser-class presence exceeds threshold)."
                        });
                        _popUpConfidences.Add(0);
                    }
                    break;

                case 6:
                    _popUpTitle = "MODEL G: ESTABLISHED SHIPPING ROUTES & CORRIDORS";
                    foreach (var r in Analytics.ModelGRoutes)
                    {
                        _popUpBullets.Add($"{r.CorridorName} : {r.DeparturePoint} -> {r.DestinationPoint} | {r.Confidence}");
                        _popUpReasonings.Add(new List<string> {
                            "MATHEMATICAL FORMULATION: Repeated Waypoint Graph Vectorization",
                            $"CORRIDOR: {r.CorridorName}",
                            $"ENDPOINTS: {r.DeparturePoint} -> {r.DestinationPoint}",
                            $"TRAFFIC PROFILE: {r.VesselTypes}",
                            $"CONFIDENCE ASSESSMENT: {r.Confidence}"
                        });
                        _popUpConfidences.Add(0);
                    }
                    break;
            }

            _popUpCallerState = ScreenState.PredictiveModels;
            _state = ScreenState.PredModelPopUp;
            _predPopUpSel = 0;
            RenderCurrentScreen();
        }

        private void HandlePredModelPopUp(System.Windows.Input.Key key)
        {
            int total = _popUpBullets.Count;
            switch (key)
            {
                case System.Windows.Input.Key.Escape:
                    _state = _popUpCallerState;
                    RenderCurrentScreen();
                    break;

                case System.Windows.Input.Key.Up:
                    if (total > 0) { _predPopUpSel = Math.Max(0, _predPopUpSel - 1); RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Down:
                    if (total > 0) { _predPopUpSel = Math.Min(total - 1, _predPopUpSel + 1); RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Tab:
                    if (total > 0) { _predPopUpSel = (_predPopUpSel + 1) % total; RenderCurrentScreen(); }
                    break;

                case System.Windows.Input.Key.Enter:
                    if (total > 0 && _predPopUpSel < total)
                        OpenReasoningModal(_popUpBullets[_predPopUpSel],
                                           _popUpReasonings[_predPopUpSel],
                                           _predPopUpSel < _popUpConfidences.Count ? _popUpConfidences[_predPopUpSel] : 0,
                                           ScreenState.PredModelPopUp);
                    break;
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

                case System.Windows.Input.Key.D1: OpenHeuristicsPopUp(0); break;
                case System.Windows.Input.Key.D2: OpenHeuristicsPopUp(1); break;
                case System.Windows.Input.Key.D3: OpenHeuristicsPopUp(2); break;
                case System.Windows.Input.Key.D4: OpenHeuristicsPopUp(3); break;
                case System.Windows.Input.Key.D5: OpenHeuristicsPopUp(4); break;
                case System.Windows.Input.Key.D6: OpenHeuristicsPopUp(5); break;
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
                        _popUpBullets.Add($"{s.RegionName,-32} Today: {s.CountToday,2} | 7D: {s.Count7DaysAgo,2} | Delta: {sign,3} [{s.Trend}]");
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
                            _popUpBullets.Add($"[{m.DateYYMMDD}] Hex {m.HexX,3},{m.HexY,-3} ({m.LocationName}) : {m.Details}");
                            _popUpReasonings.Add(new List<string> {
                                "TELEMETRY SOURCE: Combat Report Minefield Engagement Log",
                                $"COORDINATES: Hex {m.HexX},{m.HexY} (Near {m.LocationName})",
                                $"DATE: {m.DateYYMMDD}",
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
                            _popUpBullets.Add($"[{c.DateYYMMDD}] Centroid: ({c.CentroidX:F1}, {c.CentroidY:F1}) across {c.ContactCount} SS contacts");
                            var rList = new List<string> {
                                "TELEMETRY SOURCE: asigint & Operational Submarine Sightings Log",
                                $"DATE: {c.DateYYMMDD}",
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
                            _popUpBullets.Add($"VECTOR [{v.FromDate} -> {v.ToDate}]: Drift dX={v.DeltaX:+0.0;-0.0;0}, dY={v.DeltaY:+0.0;-0.0;0} | {v.DistanceHex:F1} Hex Brg {v.BearingDeg:000}° ({v.CompassHeading})");
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
                            _popUpBullets.Add($"[{b.DateYYMMDD}] Hex {b.HexX,3},{b.HexY,-3} ({b.LocationName}) : Unheralded Combat Engagement");
                            _popUpReasonings.Add(new List<string> {
                                "TELEMETRY SOURCE: combatreport Engagement Records",
                                $"LOCATION: Hex {b.HexX},{b.HexY} ({b.LocationName})",
                                $"DATE: {b.DateYYMMDD}",
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
                            _popUpBullets.Add($"[{m.DateYYMMDD}] Mean Fleet Center: ({m.MeanX:F1}, {m.MeanY:F1}) | {m.TotalVesselsCounted} vessels counted");
                            var r = new List<string> {
                                "TELEMETRY SOURCE: asigint & Sighting Telemetry Log",
                                $"DATE: {m.DateYYMMDD}",
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
                            _popUpBullets.Add($"STRATEGIC DRIFT: Vector dX={sd.DeltaX:+0.0;-0.0;0}, dY={sd.DeltaY:+0.0;-0.0;0} | {sd.DistanceHex:F1} Hex/Day Heading {sd.BearingDeg:000}° ({sd.CompassHeading})");
                            _popUpReasonings.Add(new List<string> {
                                "TELEMETRY SOURCE: 3-Day Rolling Window of Mean Fleet Coordinates",
                                $"INTERVAL: {sd.FromDate} to {sd.ToDate}",
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
                            int barLen = Math.Min(20, opt.SeparatorCount / 2);
                            string bar = new string('█', barLen) + new string('░', Math.Max(0, 20 - barLen));
                            _popUpBullets.Add($"[{opt.DateYYMMDD}] Engagements: {opt.SeparatorCount,2} [{bar}] 3D-Avg: {opt.Rolling3DayAvg:F1}");
                            var r = new List<string> {
                                "TELEMETRY SOURCE: combatreport files across daily turns",
                                $"OPERATIONAL DATE: {opt.DateYYMMDD}",
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

            _popUpCallerState = ScreenState.Heuristics;
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
            string earlyWarning = enoughData ? "" : " [!] DATA ACCUMULATING (< 4 DAYS)";

            string[] titles =
            {
                "1. Regional Power Shift (7-Day Delta)",
                "4. Minefield Mapping (Encounters & Path)",
                "5. Submarine Migration (Centroid Vectors)",
                "46. Intelligence Blindspots (Combat vs SIGINT)",
                "49. Axis of Advance (Strategic Drift)",
                "50. Operational Tempo (OPTEMPO 3D-Avg)"
            };

            string[] summaries = new string[6];

            // Summary 1
            if (!enoughData)
                summaries[0] = "NOT ENOUGH DATA // NEED MORE DATA POINTS - WAIT SOME TIME MORE";
            else
            {
                var topBuild = h.RegionalShifts.OrderByDescending(s => s.ShiftDelta).FirstOrDefault();
                summaries[0] = topBuild != null && topBuild.ShiftDelta != 0
                    ? $"Primary Shift: {topBuild.RegionName} ({topBuild.Trend})"
                    : "Regional power distribution stable across all theaters";
            }

            // Summary 4
            summaries[1] = h.MineHitsChronological.Count > 0
                ? $"{h.MineHitCoordinates.Count} minefield hexes mapped across {h.MineHitsChronological.Count} encounter events"
                : "No minefield encounter coordinates logged in combat reports";

            // Summary 5
            summaries[2] = h.SubmarineCentroids.Count > 0
                ? $"{h.SubmarineCentroids.Count} daily SS centroids computed across {h.SubmarineVectors.Count} migration vectors"
                : "No submarine sightings logged for centroid vectorization";

            // Summary 46
            summaries[3] = h.IntelligenceBlindspots.Count > 0
                ? $"{h.IntelligenceBlindspots.Count} combat engagements occurred with ZERO prior 3-day SIGINT/sightings"
                : "All combat hexes had prior intelligence coverage (0 blindspots)";

            // Summary 49
            if (h.StrategicDrift.DistanceHex > 0.001)
                summaries[4] = $"Strategic Drift: Heading {h.StrategicDrift.BearingDeg:000}° ({h.StrategicDrift.CompassHeading}) at {h.StrategicDrift.DistanceHex:F1} hexes/day";
            else if (h.FleetMeans.Count > 0)
                summaries[4] = $"Fleet mean localized at ({h.FleetMeans.Last().MeanX:F1},{h.FleetMeans.Last().MeanY:F1}); drift accumulating";
            else
                summaries[4] = "Insufficient vessel coordinates to calculate advance axis";

            // Summary 50
            if (h.DailyOptempo.Count > 0)
            {
                var latest = h.DailyOptempo.Last();
                summaries[5] = $"Today: {latest.SeparatorCount} engagements | 3-Day Rolling OPTEMPO: {latest.Rolling3DayAvg:F1} events/day";
            }
            else
                summaries[5] = "No combat reports found to compute operational tempo";

            for (int i = 0; i < titles.Length; i++)
            {
                string numStr = $"[{i + 1}] ";
                string titleStr = titles[i];
                string sumStr = summaries[i];

                if (i == _heuristicsSel)
                {
                    sb.AppendLine($"    > {numStr}{titleStr} <");
                    sb.AppendLine($"        STATUS: {sumStr}");
                }
                else
                {
                    sb.AppendLine($"      {numStr}{titleStr}");
                    sb.AppendLine($"        STATUS: {sumStr}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("    >> Press [1-6] or [ENTER] to open findings modal. Press [ESC] to return to War Room.");
            AppendFooter(sb, "[UP/DOWN] Select Metric", "[ENTER] Open Findings", "PRESS ESC TO RETURN");
            return sb.ToString();
        }

        private string BuildHeuristicsPopUp()
        {
            const int boxWidth = 78;
            const int innerWidth = 74;
            const int maxVisible = 11;

            var sb = new StringBuilder();
            string turnInfo = TurnInfo();
            string rightHeader = string.IsNullOrEmpty(turnInfo)
                ? $"ITEMS: {_popUpBullets.Count}"
                : $"{turnInfo} | ITEMS: {_popUpBullets.Count}";

            AppendHeader(sb, _popUpTitle, rightHeader);
            sb.AppendLine();
            AppendSectionHeader(sb, "TRAVERSE FINDINGS WITH ARROW KEYS | PRESS [ENTER] TO VERIFY RAW TELEMETRY");

            int total = _popUpBullets.Count;
            int sel = _heuristicsPopUpSel;
            int startIdx = sel >= maxVisible ? sel - maxVisible + 1 : 0;

            string shortTitle = _popUpTitle.Length > 62 ? _popUpTitle.Substring(0, 59) + "..." : _popUpTitle;
            string topPrefix = $"╔══ [ {shortTitle} ] ";
            int rem = boxWidth - topPrefix.Length - 1;
            sb.AppendLine(topPrefix + (rem > 0 ? new string('═', rem) : "") + "╗");

            for (int i = 0; i < maxVisible; i++)
            {
                int idx = startIdx + i;
                if (idx < total)
                {
                    string prefix = (idx == sel) ? "> " : "  ";
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
            AppendFooter(sb, "[UP/DOWN] Traverse Findings", "[ENTER] View Raw Evidence", "PRESS ESC TO RETURN");
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
                        if (_aiModelProvider == 1) { _aiEndpointUrl = "in-process://heuristics"; _aiModelName = "nostromo-tpie-v1"; }
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
                // Purge memory and reset to boot screen
                _state = ScreenState.Boot;
                RenderCurrentScreen();
            }
            else if (key is System.Windows.Input.Key.N or System.Windows.Input.Key.Escape)
            {
                _state = _exitReturnState;
                RenderCurrentScreen();
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  SCREEN BUILDERS
        // ════════════════════════════════════════════════════════════════════

        public void RenderCurrentScreen()
        {
            string buffer = _state switch
            {
                ScreenState.Boot                   => BuildBootScreen(),
                ScreenState.StartScreen            => BuildStartScreen(),
                ScreenState.NewCampaignWizard      => BuildNewCampaignWizardScreen(),
                ScreenState.ContinueCampaign       => BuildContinueCampaignScreen(),
                ScreenState.VaultIngestion         => BuildVaultIngestionScreen(),
                ScreenState.MasterMenu             => BuildMasterMenuScreen(),
                ScreenState.WarRoomPrimary          => BuildWarRoomPrimaryScreen(),
                ScreenState.IntelligenceSummary     => BuildIntelligenceSummaryScreen(),
                ScreenState.IntelDetailPopUp        => BuildNavigablePopUp(),
                ScreenState.SigintMap               => BuildSigintMapScreen(),
                ScreenState.DossierSearch           => BuildDossierSearchScreen(),
                ScreenState.TbdDomain               => BuildTbdDomainScreen(),
                ScreenState.TpieDashboard           => BuildTpieDashboardScreen(),
                ScreenState.PredictiveModels        => BuildPredictiveModelsScreen(),
                ScreenState.PredModelPopUp          => BuildNavigablePopUp(),
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
                _                                   => BuildStartScreen()
            };
            bool isInstant = _state is ScreenState.ReasoningModal
                                    or ScreenState.VaultIngestion
                                    or ScreenState.ExitConfirmModal;
            ScreenRenderRequested?.Invoke(buffer, isInstant);
        }

        // ── Width / layout helpers ────────────────────────────────────────────
        private const int W = 80;
        private static string Rule(char c = '═') =>
            (c == '-' || c == '─') ? new string('─', W) : new string('═', W);
        private static string PadTo(string s, int width) =>
            s.Length >= width ? s.Substring(0, width) : s + new string(' ', width - s.Length);

        private string TurnInfo()
        {
            if (string.IsNullOrEmpty(Manifest.FormattedDate)) return "";
            string s = Manifest.FormattedDate;
            if (Manifest.DaysProcessed > 0) s += $" ({Manifest.DaysProcessed} Days Loaded)";
            return s;
        }

        private void AppendHeader(StringBuilder sb, string title, string right = "")
        {
            sb.AppendLine(Rule('═'));
            string titleStr = $"[ {title} ]";
            if (!string.IsNullOrEmpty(right))
            {
                string rightStr = right;
                if (!rightStr.Contains("Turn:") && !rightStr.Contains("COUNT:") &&
                    !rightStr.Contains("ITEMS:") && !rightStr.Contains("MATCHES:") &&
                    !rightStr.Contains("ALL ") && !rightStr.Contains("STRATEGIC"))
                    rightStr = "Turn: " + rightStr;
                int spaces = Math.Max(1, W - titleStr.Length - rightStr.Length);
                sb.AppendLine(titleStr + new string(' ', spaces) + rightStr);
            }
            else
            {
                sb.AppendLine(titleStr);
            }
            sb.AppendLine(Rule('═'));
        }

        private void AppendSectionHeader(StringBuilder sb, string title)
        {
            sb.AppendLine($"[ {title} ]");
            sb.AppendLine(Rule('─'));
        }

        private void AppendFooter(StringBuilder sb, params string[] hotkeys)
        {
            sb.AppendLine();
            sb.AppendLine(Rule('═'));
            sb.AppendLine(string.Join("  │  ", hotkeys));
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
                        if (item.IsSelected)
                        {
                            string selectedLabel = $"> {item.Label} <";
                            int remaining = 42 - item.Prefix.Length;
                            if (selectedLabel.Length < remaining)
                                selectedLabel = selectedLabel.PadRight(remaining);
                            leftStr = item.Prefix + selectedLabel;
                        }
                        else
                        {
                            int remaining = 42 - item.Prefix.Length;
                            string lbl = item.Label;
                            if (lbl.Length < remaining)
                                lbl = lbl.PadRight(remaining);
                            leftStr = item.Prefix + lbl;
                        }
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
                AppendSectionHeader(sb, "STEP 1/4: SELECT THEATER COMMAND SIDE");
                sb.AppendLine("  Which forces do you command in this campaign?\n");

                if (_wizardSideSel == 0)
                {
                    sb.AppendLine("    > [A] ALLIED FORCES (Default) <\n");
                    sb.AppendLine("      [J] EMPIRE OF JAPAN\n");
                }
                else
                {
                    sb.AppendLine("      [A] ALLIED FORCES (Default)\n");
                    sb.AppendLine("    > [J] EMPIRE OF JAPAN <\n");
                }

                AppendFooter(sb,
                    "[A] Allied",
                    "[J] Japanese",
                    "[UP/DOWN] Toggle",
                    "[ENTER] Confirm",
                    "[ESC] Return to Start");
            }
            else if (_wizardStep == 2)
            {
                AppendSectionHeader(sb, "STEP 2/4: PBEM GAME-UI PASSWORD");
                sb.AppendLine("  Enter your PBEM password (e.g. for Dave's game file).");
                sb.AppendLine("  NOTE: Stored in vault for reference only; not required for file decryption.\n");
                sb.AppendLine($"  Password > {_wizardPwdInput}_\n");

                AppendFooter(sb,
                    "[ENTER] Confirm Password",
                    "[ESC] Previous Step");
            }
            else if (_wizardStep == 3)
            {
                AppendSectionHeader(sb, "STEP 3/4: GAME REPORTS & SAVE FOLDERS");
                sb.AppendLine("  Active Scanned Directories (Press [B] to add more):\n");

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
                {
                    sb.AppendLine($"\n  Enter Extra Directory Path > {_wizardDirInput}_");
                }
                else
                {
                    sb.AppendLine("\n  Press [ENTER] to process files or [B] to add another folder.");
                }

                AppendFooter(sb,
                    "[ENTER] Process Folders",
                    "[B] Add Extra Directory",
                    "[ESC] Previous Step");
            }
            else if (_wizardStep == 4)
            {
                AppendSectionHeader(sb, "STEP 4/4: CAMPAIGN VAULT NAME");
                sb.AppendLine("  Enter a name for the new campaign vault container.");
                sb.AppendLine($"  File will be saved to: C:\\WitP-CIC\\Campaigns\\{_wizardCampaignName}.wic\n");
                string displayName = string.IsNullOrEmpty(_wizardNameInput) ? _wizardCampaignName : _wizardNameInput;
                sb.AppendLine($"  Campaign Name [Default: {_wizardCampaignName}] > {displayName}_\n");

                AppendFooter(sb,
                    "[ENTER] Finalize & Ingest",
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
                sb.AppendLine($"  NO SAVED CAMPAIGN VAULTS (.wic) FOUND IN {VaultDir}\n");
                sb.AppendLine("  Please create a new campaign using [N] Start New Campaign.");
                sb.AppendLine("  Sample telemetry will be automatically ingested.\n");

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
                        sb.AppendLine($"    > [X] {_vaultFiles[i]} <");
                    }
                    else
                    {
                        sb.AppendLine($"      [ ] {_vaultFiles[i]}");
                    }
                    sb.AppendLine();
                }

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

            sb.AppendLine($"  Total Relevant Game Files Detected: {Manifest.Files.Count}");
            sb.AppendLine($"  Total Operational Days Processed   : {Manifest.DaysProcessed}");
            sb.AppendLine($"  Latest Campaign Date               : {Manifest.FormattedDate}\n");

            AppendSectionHeader(sb, "F I L E   I N V E N T O R Y   ( P W S   &   T X T )");
            int displayCount = 0;
            foreach (var file in Manifest.Files)
            {
                string dateInfo = string.IsNullOrEmpty(file.DateYYMMDD) ? "SAVE" : file.DateYYMMDD;
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
            AppendHeader(sb, "MUTHUR 6000 // SYSTEM BOOT DIAGNOSTIC");
            sb.AppendLine();
            sb.AppendLine(">> SYSTEM INITIALIZATION: OK");
            sb.AppendLine(">> COLD-START DIAGNOSTICS COMPLETE. ARCHITECTURE: 2.028-ALPHA-MUTHUR");
            sb.AppendLine($">> AUTHORITATIVE BASE REGISTRY: {BaseLocationRegistry.Instance.Count:N0} BASES LOADED");
            sb.AppendLine(">> NAVAL DOCTRINE HULL TYPES: 60 CLASSES COMPILED");
            sb.AppendLine();
            sb.AppendLine(Rule('-'));
            sb.AppendLine("  PACIFIC CAMPAIGN TELEMETRY HARVEST");
            sb.AppendLine(Rule('-'));
            sb.AppendLine($"  CURRENT CAMPAIGN DATE : {Manifest.FormattedDate} (TURN CODE: {Manifest.LatestDate})");
            sb.AppendLine($"  INGESTED GAME FILES   : {Manifest.Files.Count} FILES ACROSS {Manifest.DaysProcessed} CAMPAIGN DAYS");
            sb.AppendLine($"  SIGINT INTERCEPTS     : {Analytics.GeospatialLog.Count} CONTACTS TRACKED");
            sb.AppendLine($"  ACTIVE STRIKE FORCES  : {Analytics.ModelFConcentrations.Count} CONCENTRATIONS IDENTIFIED");
            sb.AppendLine($"  UNLOCALIZED CAPITALS  : {Analytics.ModelEClaims.GhostFleetCount} GHOST FLEET THREATS");
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
            string campaignName = Manifest.Files.Count > 0 ? "WITP:AE PBEM CAMPAIGN" : "NO CAMPAIGN LOADED";
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
                "WAR ROOM (INTELLIGENCE & TPIE DASHBOARDS)",
                "WIKI (WITP-CIC SYSTEM & ERROR REGISTRY)",
                "FIELD MANUAL (WITP:AE GAME MANUAL BROWSER)",
                "SYSTEM SETTINGS & DETECTION THRESHOLDS",
                "TERMINATE SESSION & PURGE MEMORY"
            };
            string[] hotkeys = { "[A] ", "[W] ", "[F] ", "[S] ", "[E] " };

            for (int i = 0; i < menuLabels.Length; i++)
            {
                string arrow = (i == _masterMenuSel) ? "> " : "  ";
                string hk = hotkeys[i];
                if (i == _masterMenuSel)
                    sb.AppendLine($"    {hk}> {menuLabels[i]} <");
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
                "Intelligence Summary",
                "SIGINT Map",
                "TBD",
                "TPIE Warning Dashboard",
                "Predictive Models Sub-Menu",
                "Heuristics (Logical Blueprints)"
            };

            for (int i = 0; i < menuLabels.Length; i++)
            {
                string optText = $"[{i + 1}]  {menuLabels[i]}";
                if (i == _warRoomSel)
                    sb.AppendLine($"    > {optText} <");
                else
                    sb.AppendLine($"      {optText}");
                sb.AppendLine();
            }

            sb.AppendLine("    >> Press [1-6] or [ENTER] to open topic sub-menu. All pop-ups support cursor keys.");
            AppendFooter(sb, "[UP/DOWN] Select Topic", "[ENTER] Open Sub-Menu", "PRESS ESC TO RETURN");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  INTELLIGENCE SUMMARY
        // ─────────────────────────────────────────────────────────────────────
        private string BuildIntelligenceSummaryScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "WAR ROOM : THEATER INTELLIGENCE SUMMARY & CONCENTRATIONS", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "SELECT INTELLIGENCE DOMAIN TO DISCLOSE GRANULAR TARGET MANIFEST");

            string[] options =
            {
                "High-Value Target Tracker",
                "Strike Concentrations",
                "Port & Anchorage Concentrations",
                "Established Shipping Routes",
                "Battle Claims & Revisions"
            };

            string[] counts =
            {
                $"{Analytics.HighValueTargets.Count} localized capital units (CV, CA, AO oilers)",
                $"{Analytics.ModelFConcentrations.Count} active warship clusters (>threshold)",
                $"{Analytics.PortConcentrations.Count} major staging ports monitored",
                $"{Analytics.ModelGRoutes.Count} supply & invasion corridors mapped",
                $"{Analytics.ModelEClaims.Claims.Count} hostile action assessments tracked"
            };

            for (int i = 0; i < options.Length; i++)
            {
                string desc = $"{options[i]} : {counts[i]}";
                string lineOut;
                if (i == _intelSumSel)
                    lineOut = $"  [{i + 1}]  > {desc} <";
                else
                    lineOut = $"  [{i + 1}]   {desc}";
                sb.AppendLine(lineOut);
                sb.AppendLine();
            }

            sb.AppendLine("      >> Use [UP/DOWN] to select domain, press [ENTER] to open navigable pop-up.");
            AppendFooter(sb, "[UP/DOWN] Select Domain", "[ENTER] Open Pop-Up", "PRESS ESC TO RETURN");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  NAVIGABLE POP-UP
        // ─────────────────────────────────────────────────────────────────────
        private string BuildNavigablePopUp()
        {
            const int boxWidth = 78;
            const int innerWidth = 74;
            const int maxVisible = 11;

            var sb = new StringBuilder();

            string turnInfo = TurnInfo();
            string rightHeader = string.IsNullOrEmpty(turnInfo)
                ? $"ITEMS: {_popUpBullets.Count}"
                : $"{turnInfo} | ITEMS: {_popUpBullets.Count}";

            AppendHeader(sb, _popUpTitle, rightHeader);
            sb.AppendLine();
            AppendSectionHeader(sb, "TRAVERSE BULLETS WITH ARROW KEYS | PRESS [ENTER] TO DISCLOSE REASONING");

            int total = _popUpBullets.Count;
            int sel = _state == ScreenState.IntelDetailPopUp ? _intelPopUpSel : _predPopUpSel;
            int startIdx = sel >= maxVisible ? sel - maxVisible + 1 : 0;

            string shortTitle = _popUpTitle.Length > 62 ? _popUpTitle.Substring(0, 59) + "..." : _popUpTitle;
            string topPrefix = $"╔══ [ {shortTitle} ] ";
            int rem = boxWidth - topPrefix.Length - 1;
            sb.AppendLine(topPrefix + (rem > 0 ? new string('═', rem) : "") + "╗");

            for (int i = 0; i < maxVisible; i++)
            {
                int idx = startIdx + i;
                if (idx < total)
                {
                    string prefix = (idx == sel) ? "> " : "  ";
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
            sb.AppendLine("    >> Press [ENTER] to view granular telemetry evidence & reasoning for this line.");
            AppendFooter(sb, "[UP/DOWN] Traverse Bullets", "[ENTER] View Reasoning", "PRESS ESC TO RETURN");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  SIGINT MAP
        // ─────────────────────────────────────────────────────────────────────
        private string BuildSigintMapScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "WAR ROOM : GEOSPATIAL SIGINT MAP & SECTOR BEARING LOG", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "DATE     TYPE     LOCALIZED POS   SECTOR BEARING FROM HQ       DETAILS / INTERCEPT");

            var log = Analytics.GeospatialLog;
            if (log.Count == 0)
            {
                sb.AppendLine();
                sb.AppendLine("    NO GEOSPATIAL CONTACTS LOCALIZED FOR THIS TURN RANGE.");
            }
            else
            {
                const int maxVisible = 11;
                int total = log.Count;
                int startIdx = _sigintMapSel >= maxVisible ? _sigintMapSel - maxVisible + 1 : 0;
                int endIdx = Math.Min(total, startIdx + maxVisible);

                for (int i = startIdx; i < endIdx; i++)
                {
                    var entry = log[i];
                    string dStr = string.IsNullOrEmpty(entry.LastSeenDate) ? entry.DateYYMMDD : entry.LastSeenDate;
                    string posStr = $"{entry.HexX},{entry.HexY}";
                    string pointer = (i == _sigintMapSel) ? "> " : "  ";
                    string datePad = PadTo(dStr.Length > 8 ? dStr.Substring(0, 8) : dStr, 9);
                    string typePad = PadTo(entry.EventType.Length > 8 ? entry.EventType.Substring(0, 8) : entry.EventType, 9);
                    string posPad = PadTo(posStr.Length > 15 ? posStr.Substring(0, 15) : posStr, 16);
                    string bearPad = PadTo(entry.BearingFromHQ.Length > 28 ? entry.BearingFromHQ.Substring(0, 28) : entry.BearingFromHQ, 29);
                    string det = entry.Details.Length > 17 ? entry.Details.Substring(0, 17) : entry.Details;
                    sb.AppendLine($"    {pointer}{datePad}{typePad}{posPad}{bearPad}{det}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("    >> Use [UP/DOWN] to traverse contacts. Press [ENTER] for reasoning. Press [D] for Dossier.");
            AppendFooter(sb, "[UP/DOWN] Select Contact", "[ENTER] View Reasoning", "[D] Dossier Search", "PRESS ESC TO RETURN");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  DOSSIER SEARCH
        // ─────────────────────────────────────────────────────────────────────
        private string BuildDossierSearchScreen()
        {
            var sb = new StringBuilder();
            if (_dossierSearching)
            {
                AppendHeader(sb, "UNIFIED INTELLIGENCE DOSSIER SEARCH", "ALL CATEGORIES");
                sb.AppendLine();
                AppendSectionHeader(sb, "TARGET LOOKUP : BASE | VESSEL | UNIT | AIR GROUP | CITY");
                sb.AppendLine();
                sb.AppendLine($"    ENTER SEARCH QUERY (PARTIAL NAME OR CATEGORY): {_dossierQuery}_\n");
                AppendFooter(sb, "[ENTER] Execute Search", "[ESC] Return to SIGINT Map");
            }
            else
            {
                AppendHeader(sb, "DOSSIER SEARCH RESULTS", $"MATCHES: {_dossierResults.Count}");
                sb.AppendLine();
                AppendSectionHeader(sb, $"QUERY: \"{_dossierQuery}\" - ENTERPRISE KNOWLEDGE BASE");

                if (_dossierResults.Count == 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("    NO MATCHING INTELLIGENCE ENTITIES IDENTIFIED IN CURRENT TELEMETRY.");
                }
                else
                {
                    sb.AppendLine("    CATEGORY    NAME / IDENTIFIER           LOCATION / STATUS");
                    sb.AppendLine("    " + new string('─', 76));
                    int maxShow = Math.Min(11, _dossierResults.Count);
                    for (int i = 0; i < maxShow; i++)
                    {
                        var r = _dossierResults[i];
                        string catPad = PadTo(r.Category.Length > 11 ? r.Category.Substring(0, 11) : r.Category, 12);
                        string namePad = PadTo(r.Name.Length > 26 ? r.Name.Substring(0, 26) : r.Name, 28);
                        string det = r.Details.Length > 38 ? r.Details.Substring(0, 38) : r.Details;
                        sb.AppendLine($"    {catPad}{namePad}{det}");
                    }
                }
                AppendFooter(sb, "PRESS ESC TO RETURN");
            }
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
        //  TPIE DASHBOARD
        // ─────────────────────────────────────────────────────────────────────
        private string BuildTpieDashboardScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "WAR ROOM : THEATER PREDICTIVE INTELLIGENCE ENGINE (TPIE)", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "STRATEGIC INVASION PROBABILITY HEATMAP & FLEET TRAJECTORIES");

            sb.AppendLine("[ STRATEGIC INVASION PROBABILITY HEATMAP ]  (Weighted Evidence Accumulator)");
            sb.AppendLine("DESTINATION      PROBABILITY  THREAT LEVEL      TRANSPORTS / UNITS / STATUS");
            sb.AppendLine(Rule('-'));

            var targets = Analytics.ModelATargets;
            if (targets.Count == 0)
            {
                sb.AppendLine("    NO ACTIVE AMPHIBIOUS PREDICTIONS GENERATED YET.");
            }
            else
            {
                int maxShow = Math.Min(5, targets.Count);
                for (int i = 0; i < maxShow; i++)
                {
                    var t = targets[i];
                    string probBuf = $"{t.Probability * 100,3:F0}%";
                    string unitSummary = t.LoadedUnits.Count == 0
                        ? t.Assessment
                        : $"{t.LoadedUnits[0]} on {(t.Transports.Count == 0 ? "xAK" : t.Transports[0])}";

                    string dest = PadTo(t.Destination.Length > 15 ? t.Destination.Substring(0, 15) : t.Destination, 16);
                    string prob = PadTo(probBuf, 12);
                    string threat = PadTo(t.ThreatLevel.Length > 16 ? t.ThreatLevel.Substring(0, 16) : t.ThreatLevel, 17);
                    string units = unitSummary.Length > 32 ? unitSummary.Substring(0, 32) : unitSummary;

                    string marker = (i == _tpieSel) ? "> " : "  ";
                    sb.AppendLine($"{marker}{dest}{prob}{threat}{units}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("[ FLEET TRAJECTORIES & INTERCEPT AMBUSH WINDOWS ]  (Dead reckoning at T+1 turn)");
            var trajs = Analytics.ModelBTrajectories;
            if (trajs.Count > 0)
            {
                var topTraj = trajs[0];
                sb.AppendLine($"  LEAD CONTACT : {topTraj.Description}");
                sb.AppendLine($"  VECTOR       : {topTraj.StartHexX},{topTraj.StartHexY} -> Proj Hex {topTraj.ProjectedHexX},{topTraj.ProjectedHexY} ({topTraj.Heading} @ {topTraj.SpeedKnots} kts) | {topTraj.TargetDestination}");
            }
            else
            {
                sb.AppendLine("  NO ACTIVE FLEET TRAJECTORIES LOCALIZED TO COMPUTE INTERCEPT WINDOWS.");
            }

            sb.AppendLine();
            sb.AppendLine("    >> Press [ENTER] over target for predictive reasoning. Press [I] for Intercepts.");
            AppendFooter(sb, "[UP/DOWN] Select Target", "[ENTER] View Reasoning", "[I] Intercept Windows", "PRESS ESC TO RETURN");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  PREDICTIVE MODELS SUB-MENU
        // ─────────────────────────────────────────────────────────────────────
        private string BuildPredictiveModelsScreen()
        {
            var sb = new StringBuilder();
            AppendHeader(sb, "WAR ROOM : GRANULAR PREDICTIVE INTELLIGENCE MODELS (A-G)", TurnInfo());
            sb.AppendLine();
            AppendSectionHeader(sb, "SELECT PREDICTIVE ANALYTIC ENGINE TO DISCLOSE REASONING DUMP");

            string[] modelTitles =
            {
                "MODEL A : Amphibious Assault Probability Engine (Heuristic Bayes)",
                "MODEL B : Enemy Fleet Trajectory & Intercept Projection (Dead Reckoning)",
                "MODEL C : Air Superiority & Combat Loss Forecaster (EMA Decay)",
                "MODEL D : Logistical & Operational Pause Detector (TF State Machine)",
                "MODEL E : Battle Claims & Damage Revision Register (Entity Match)",
                "MODEL F : Strike Concentration Detector (Spatial Hex Cluster)",
                "MODEL G : Established Shipping Routes & Corridors (Waypoint Graph)"
            };

            for (int i = 0; i < modelTitles.Length; i++)
            {
                string letter = ((char)('A' + i)).ToString();
                string title = modelTitles[i];
                if (i == _predModelsSel)
                    sb.AppendLine($"  [{letter}]  > {title} <");
                else
                    sb.AppendLine($"  [{letter}]   {title}");
                sb.AppendLine();
            }

            sb.AppendLine("    >> Press [ENTER] to view granular telemetry dump and mathematical reasoning.");
            AppendFooter(sb, "[UP/DOWN] Select Model", "[ENTER] View Reasoning", "PRESS ESC TO RETURN");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  REASONING MODAL
        // ─────────────────────────────────────────────────────────────────────
        private string BuildReasoningModal()
        {
            const int width = 78;
            const int innerWidth = 74;
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

            string prompt = "SELECT REASONING TOPIC & PRESS [ENTER] TO DISCLOSE EVIDENCE:";
            sb.AppendLine($"║ {PadTo(prompt, innerWidth)} ║");
            sb.AppendLine($"║ {new string(' ', innerWidth)} ║");

            for (int i = 0; i < _reasoningTopics.Count; i++)
            {
                string prefix = (i == _reasoningTopicSel) ? "> " : "  ";
                string num = $"[{i + 1}] ";
                string title = _reasoningTopics[i].Title;
                string action = " [PRESS ENTER]";
                string full = prefix + num + title;
                if (full.Length + action.Length < innerWidth)
                    full = full + new string(' ', innerWidth - full.Length - action.Length) + action;
                else
                    full = PadTo(full, innerWidth);
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
            string footerText = "[UP/DOWN] Select Topic  │  [ENTER] Drill Down  │  PRESS ESC TO RETURN";
            sb.AppendLine($"║ {PadTo(footerText, innerWidth)} ║");
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
                        if (item.IsSelected)
                        {
                            string sel = $"> {item.Title} <";
                            int remaining = 42 - item.Prefix.Length;
                            if (sel.Length < remaining) sel = sel.PadRight(remaining);
                            leftStr = item.Prefix + sel;
                        }
                        else
                        {
                            int remaining = 42 - item.Prefix.Length;
                            string t = item.Title;
                            if (t.Length < remaining) t = t.PadRight(remaining);
                            leftStr = item.Prefix + t;
                        }
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
                        if (item.IsSelected)
                        {
                            string sel = $"> {item.Title} <";
                            int remaining = 42 - item.Prefix.Length;
                            if (sel.Length < remaining) sel = sel.PadRight(remaining);
                            leftStr = item.Prefix + sel;
                        }
                        else
                        {
                            int remaining = 42 - item.Prefix.Length;
                            string t = item.Title;
                            if (t.Length < remaining) t = t.PadRight(remaining);
                            leftStr = item.Prefix + t;
                        }
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
                    sb.AppendLine($"  [{i + 1}]  > {options[i]} <");
                else
                    sb.AppendLine($"  [{i + 1}]   {options[i]}");
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
                    sb.AppendLine($"  [{i + 1}]  > {options[i]} <");
                else
                    sb.AppendLine($"  [{i + 1}]   {options[i]}");
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
            AppendFooter(sb, "[Y] Yes, Purge and Reset", "[N/ESC] Cancel");
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
                int breakPos = rem.LastIndexOf(' ', curWidth - 1);
                if (breakPos < 0 || breakPos < curWidth / 3) breakPos = curWidth;
                result.Add((first ? "" : new string(' ', indent)) + rem.Substring(0, Math.Min(breakPos, rem.Length)));
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
                    "1.3 TACTICAL INTERCEPT WINDOWS (TPIE)",
                    "The Tactical Patrol & Intercept Engine models enemy heading and speed vectors,",
                    "calculating intercept opportunities for friendly strike aircraft and submarines."
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
                Description = "Intelligence summary, localized hex bearings, and tactical intercept engine",
                ContentLines = new List<string>
                {
                    "2.2 COMBAT INFORMATION CENTER (WAR ROOM) (§4.5)",
                    "----------------------------------------------------------------------",
                    "",
                    "TAB 1: STRATEGIC INTELLIGENCE SUMMARY",
                    "- High-Value Targets (HVT): Monitors spotted enemy aircraft carriers (CV/CVL),",
                    "  battleships (BB), fleet oilers (AO), and amphibious transports (AP/AK).",
                    "- Strike Concentrations: Detects clusters of 3+ capital warships (cruisers+",
                    "  or battleships) operating within 10 hexes of each other.",
                    "- Port Concentrations: Tracks significant enemy tonnage anchored in ports.",
                    "- Battle Damage Claims: Tracks combat sinkings with fog-of-war skepticism.",
                    "- Ghost Fleet: Flags lost contacts whose tracks have gone cold.",
                    "",
                    "TAB 2: GEOSPATIAL CONTACT LIST",
                    "Renders SIGINT radio intercepts and visual sightings as structured tabular",
                    "records with hex coordinates, localized range in hexes, and sector bearings",
                    "(e.g., '14 hexes ENE of Pearl Harbor HQ').",
                    "",
                    "TAB 3: TACTICAL PATROL & INTERCEPT ENGINE (TPIE)",
                    "Simulates enemy task force trajectories based on reported headings and",
                    "cruising speeds. Projects intercept windows and highlights search patrol gaps."
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
                    "The sole authoritative Git remote is origin -> https://github.com/TheZenHippie/MUTHUR6000.git",
                    "for project witp-cic-muthur on branch main. Upstream Witp-CIC2 is preserved read-only.",
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
