# BUSINESS ANALYSIS AND REQUIREMENTS DOCUMENT

Project: WITP:AE CIC Intelligence Helper — Version 1

**Status:** Design spec bible. This document grows as requirements change. Additions must be logged in the changelog with date/time. Check off TODO items only upon explicit instruction from Ozgur ("never mark anything done unless I say so").

**Baseline Scope Mandate (2026-09-20 15:05 UTC+3 — Ozgur):** Every new todo item and correction MUST be added to the business scope. If there is any conflict between a new request and existing requirements or architecture, the agent MUST ask Ozgur to clarify before taking action.

**Process rule (2026-09-19 18:22 UTC+3 — Ozgur):** Every answer Ozgur gives to any open question MUST be immediately recorded in this changelog with date/time, and mirrored into the relevant design doc (`docs/project-context.md`, `docs/architecture-decisions.md`, etc.). This applies to all questions, in all sessions, forever. No exception. Agent must write the entry before any implementation begins.

## Changelog
- 2026-10-01 18:45 UTC+3: **Clean To-Do List & Canonical PWS Save Processing & Historical Retention Engine (witp-cic-rochefort-alpha4-27) (Ozgur):**
  1. **Canonical PWS Save Scope Enforcement:**
     - User directive: *"u will only process the pws files under the C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE , \SAVE is always under the install directory. Each turn a new pws file is placed there. u will read this file, extract the knowledge, store, use if applicable. new pws will arrive next turn, same process. never lose old knowledge, Store it."*
     - Enforced in `FileScanner.cs`: `.pws` files are strictly classified as `PWS_SAVE` only when located in `...\SAVE` or test fixture paths (`sample_data`).
     - Default scan paths in `TerminalScreenManager.cs` updated to include `C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE`.
  2. **Multi-Turn Knowledge Store & Historical Retention:**
     - `PwsRepository` accumulates every ingested snapshot into `SortedDictionary<DateOnly, PwsSnapshot>` (Pillar 13).
     - Older snapshots are permanently retained for reinforcement countdown deltas, confirmed sunk ship tracking, fuel consumption deltas, and cross-turn gap detection.
     - The latest save file becomes `PwsRepository.MostRecent` for active fleet authority.
  3. **Split-Pane Architecture & UI Refactor (Builds 20-26):**
     - Full resolution-independent split-pane console architecture implemented across 1024x640, 1366x768, 1920x1080, and 2560x1440.
     - Double-line active border vs single-line inactive border focus switching.
     - Zero text wrapping with ellipsis truncation in Upper Pane; nested reasoning and evidence navigation in Lower Pane.
     - Clean exit modal handling: `[Y]` terminates session immediately with process exit (code 0).
     - Standardized date format `[dd/mm/yy]` and lower-right version badge `v-{Phase}-build{BuildNumber}` (`v-alpha4-build27`).
  4. **Clean To-Do List Reset:**
     - Archived previous completed development tasks and initialized fresh active to-do list in `docs/todo.md`.

- 2026-09-29 23:30 UTC+3: **Interactive Calendar Pop-Up Picker, Menu Descending Chronology, Domain Retirement, & Build 17 (witp-cic-rochefort-alpha4-17) (Ozgur):**
  1. **Interactive Visual Calendar Pop-Up Picker (`[S]` and `[C]`):**
     - Integrated `ScreenState.CalendarPickerModal` into `TerminalScreenManager.cs`.
     - Opened via `[S]` (Single Date) or `[C]` (Custom Range) from the Temporal Filter Modal (`[R]`).
     - Interactive cursor navigation via keyboard: `[Left/Right]` for days, `[Up/Down]` for weeks, `[PgUp/PgDn]` or `[M/N]` for months, `[Enter]` to confirm selection, `[Esc]` to cancel.
     - Strict boundary clamping: Clamped between Pearl Harbor start date (`12/07/1941`) and campaign maximum (`12/31/1946`).
     - Multi-step custom range flow: Step 1 (Select Start Date) and Step 2 (Select End Date) with interactive visual highlights (`(DD)` start, `[DD]` cursor, `=DD=` active range span).
     - Strict 106-column monospace grid with 100% straight vertical borders (`║`).
  2. **Label Clarity & Border Stabilization:**
     - Replaced cryptic text such as `(3hvt)` with clear standard labeling `(3 HIGH-VALUE TARGETS)`.
     - Eliminated border disturbance and jitter when keys are pressed in command strip and modal.
  3. **Universal Descending Date Chronology (Newest to Oldest):**
     - Enforced `OrderByDescending` by date across `SIGINT MAP` (`GeospatialLog`), `IntelligenceSummary` lists (`HighValueTargets`, `ModelEClaims`), and all other manifests.
  4. **TPIE & Predictive Models Retirement:**
     - Deleted sub-pages under TPIE Warning Dashboard; renamed option `[4]` in War Room Primary to `[ TBD ]` routing to `ScreenState.TbdDomain`.
     - Deleted Predictive Models sub-menu and connected pages; renamed option `[5]` in War Room Primary to `[ TBD ]` routing to `ScreenState.TbdDomain`.
  5. **Auto-Increment Release Build System:**
     - Automated build counter auto-increment in `build_release.ps1` to produce `dist/witp-cic-rochefort-alpha4-17.exe`.
     - 147 automated tests passing (100% pass rate).

- 2026-09-29 18:45 UTC+3: **Compact Temporal Filter & Date Range Modal Integration (witp-cic-rochefort-alpha4-16) (Ozgur):**
  1. **Compact 2-Line Command Strip Header:**
     - Replaced the horizontal date slider track in both `BuildIntelligenceSummaryScreen()` and `BuildSigintMapScreen()` with a compact 2-line command strip:
       `PRESETS: [1] LATEST (MM/DD/YY)  [2] LAST 7D  [3] LAST 15D  [4] LAST 30D  [5] ALL  │ [R] RANGE MODAL`
       `ACTIVE:  * MM/DD/YY -> MM/DD/YY (LAST 7 DAYS)    << [ / ] STEP TURN: MM/DD/YY >>     (XX CONTACTS)`
     - Hotkeys `[1-5]` trigger quick preset changes immediately from the primary view.
     - Keys `[` and `]` step reference operational turns backward and forward.
  2. **Dedicated Temporal Filter & Date Range Modal:**
     - Added `ScreenState.TemporalFilterModal` providing View 2 (menu presets [1-6], single date entry [S], custom range [C]) and View 3 (interactive typing prompt with flexible `MM/DD/YY`, `MM/DD/YY - MM/DD/YY`, and `-7d`, `-15d`, `-30d` syntax).
     - Live error validation banner prevents invalid inputs.
  3. **Mandatory Default Filter Rule:**
     - Upon first viewing or switching to `IntelligenceSummary` or `SigintMap`, the filter is reset via `ResetFilterToLast7Days()` to always display the **Last 7 Days** of the active campaign.
  4. **Strict Monospace 106-Column Formatting:**
     - Enforced straight box-drawing borders (`║`) and exact 106 terminal column width with zero horizontal clipping.
  5. **Release Binary & Test Pass:**
     - Standalone release compiled at `dist/witp-cic-rochefort-alpha4-16.exe`.
     - 124 automated tests passing (100% pass rate).

- 2026-09-29 14:15 UTC+3: **Native Semantic Concept Graph & Enhanced Dossier Search (witp-cic-rochefort-alpha4-15) (Ozgur):**
  1. **Zero-Dependency Native C# Implementation:**
     - Created `Core/Analytics/SemanticConceptGraph.cs` implementing a pure C# TF-IDF bag-of-words embedding engine and cosine similarity matcher. Zero new NuGet packages, zero external DLLs, zero runtime footprint increase.
  2. **Comprehensive Domain Concept Indexing:**
     - Ingests over 1,480 entities: 1,330 strategic bases & anchorages from `BaseLocationRegistry`, discovered vessels, units, air groups, High Value Targets (CVs, BBs, fleet oilers), Model A amphibious invasion objectives, and Model G logistics corridors.
     - Builds domain vocabulary and TF-IDF vectors dynamically during `AnalyzeCampaign()`, adhering strictly to in-memory non-destructive architecture.
  3. **Enhanced Dossier Search with Semantic Fallback:**
     - In `WarRoomAnalytics.SearchDossier()`, searches both direct substring matches and semantic concept matches.
     - Natural conceptual queries like `"carrier threat oiler"` or `"amphibious assault landing"` return highly relevant targets with semantic match scoring percentages (`[SEMANTIC: 89%]`).
  4. **Release Binary & Test Pass:**
     - Single-file release published at `dist/witp-cic-rochefort-alpha4-15.exe`.
     - 114 test assertions passing (100% pass rate).

- 2026-09-28 22:50 UTC+3: **Date Slider Track & Total CRT Glitch Defanging (witp-cic-rochefort-alpha4-15) (Ozgur):**
  1. **Date Slider in SIGINT & Theater Intelligence Summary Menus:**
     - Rendered full visual date track in both `BuildIntelligenceSummaryScreen()` and `BuildSigintMapScreen()`:
       `DATE SLIDER:  [◀ 04 - May - 42 ▶]   23 - Dec - 41   ...   ALL`
     - Dates strictly sorted descending (`OrderByDescending(d => d)`), listing newest operational turns first, followed by `ALL`.
     - Prominently highlights active selected date (`[◀ {date} ▶]`), navigable via `[LEFT/RIGHT]`, `[,] [.]`, and `[` / `]`.
     - Added navigation prompt `(USE [ / ] OR LEFT/RIGHT TO CYCLE DATES)`.
  2. **Total Screen Flicker & CRT Glitch Elimination:**
     - Completely defanged `GlitchTimer_Tick` and `TriggerRightToLeftBeamGlitch` in `MainWindow.xaml.cs`.
     - Ensured zero `ContentJitterTransform` displacement (`X = 0`, `Y = 0`) across all states.
     - Enforced `_crtGlitchEnabled = false`, `GlitchChance = 0.0` in code defaults and migrated `%APPDATA%\ROCHEFORT6000\settings.json`.
     - Even if explicitly toggled on, glitch rendering logic produces zero random opacity slices, zero jitter, and zero visual flicker.
  3. **Standalone Release Binary:** Published `dist/witp-cic-rochefort-alpha4-15.exe`. All 111 automated test suites passing.


- 2026-09-27 15:25 UTC+3: **Universal Elimination of MUTHUR 6000 Wording Across Documentation & Programming (Ozgur):**
  1. **User Mandate:** User ordered *"U will remove any and every reference to the MUTHUR 6000 wording in every documentation and programming now."*
  2. **Total Documentation Purge:** Eliminated all occurrences of `MUTHUR 6000` across all specification bibles, architectural decisions, baseline principles, milestone records, ASCII blueprint templates, project context, and open question files.
  3. **Assembly & Packaging Rebranding:** Rebranded project assembly metadata (`MUTHUR6000.csproj`), product titles (`WITP-CIC-ROCHEFORT`), descriptions, company identity, and local AppData persistence folders (`ROCHEFORT6000`).
  4. **Release Script Standardization:** Updated `build_release.ps1` to produce `ROCHEFORT6000` release archives and the authoritative single versioned executable (`dist/witp-cic-rochefort-alpha4-13.exe`) with zero secondary alias binaries (LL-063).

- 2026-09-26 19:30 UTC+3: **Mathematical Heuristic Domains Menu Pruned to Option 1 Only (Ozgur):**
  1. **Menu Streamlining:** User ordered *"delete all other than 1 in the mathematical heuritsitc domains // temporal naval intentions menu NOW"*.
  2. `BuildHeuristicsScreen()` updated to display exclusively `[1] 1. Regional Power Shift (7-Day Delta)`. Items 2, 3, 4, 5, 6 removed from display.
  3. `HandleHeuristics()` restricted to selection `0` with keys `[1]` and `[ENTER]`.
  4. Core underlying algorithms in `HeuristicsEngine.cs` preserved for analytical modeling (ADR 78, LL-062).

- 2026-09-26 19:02 UTC+3: **Absolute Prohibition of secondary alias binaries & Secondary Binaries (Ozgur):**
  1. **Strict Single-Binary Mandate:** User explicitly mandated *"no need to produce secondary alias binaries dont make this mistake again"*.
  2. `dist/secondary alias binaries` deleted immediately.
  3. The sole authorized executable artifact in `dist/` is the active versioned binary (`dist/witp-cic-rochefort-alpha4-12.exe`). No duplicate aliases or mirrors permitted (ADR 63, LL-049, LL-061).

- 2026-09-26 18:45 UTC+3: **106-Column Box Expansion, Gap Elimination & Live Analytics Wiring (Ozgur):**
  1. **106-Column Monospace Box Expansion:** Expanded modal/popup box widths across `BuildHeuristicsPopUp`, `BuildNavigablePopUp`, and `BuildReasoningModal` from 78 to 106 columns (`boxWidth = 106`, `innerWidth = 102`). Long sighting strings (e.g. `> [TF] Major Surface TF (7 ships) at Batan Island (85,70) [411207] - Main Strike Force`) now fit completely with zero text truncation or cutoff against the right border `║`. Header titles clamped at 90 characters. Layout rules and section headers standardized to 106 width.
  2. **Gap Elimination & Proportional Scaling:** Adjusted `RochefortColumn.Width` to `0.44*` and elevated `panelFontFactor` in `UpdateProportionalScaling()` from `0.72` to `0.90`. This expands the text boxes ~1.5x in visual width, closing the dead space gap between the main text body and the advisor panel completely across 1366x768 and 1080p resolutions.
  3. **Live Analytics Grounding in Advisor Speech:** Wired live campaign telemetry from `_popUpReasonings` into `BuildIntelAdvisorComment()` and `BuildPredModelAdvisorComment()`, providing live target lines, confidence percentages (`[RADAR/RDF CORROBORATED]` or `[IMPERIAL AIR RECON VERIFIED]`), signal evidence bullets, and model vectors.
  4. **Strict Faction Isolation:** Station Hypo Cmdr. Rochefort panel appears strictly for Allied players; Japanese players strictly receive Combined Fleet Chief of Staff Vice Admiral Ugaki (`assets/ugaki_lowres.png`). Rochefort panel never appears for Japanese players.

- 2026-09-25 16:30 UTC+3: **Box Boundary Clamping, Proportional Width Expansion (~1.5x) & Faction-Specific Advisor (Ugaki vs Rochefort) (Ozgur):**
  1. **Strict Line Length Invariant for Box Frames:** Clamped box title header strings in `BuildNavigablePopUp` and `BuildHeuristicsPopUp` (`shortTitle.Length <= 59`) so padding `rem >= 1` always, eliminating line-breaks or negative width wraps. Enforced exact string length invariant `Length == 78` with vertical right border `║` across all popup lines.
  2. **Proportional Width Expansion (~1.5x) & Gap Elimination:**
     - Increased `RochefortColumn` from `0.40*` to `0.48*` and tightened margins to eliminate empty dead space between the main text terminal and the advisor window.
     - Elevating `panelFontFactor` in `UpdateProportionalScaling()` from `0.72` to `0.88`, expanding terminal text boxes ~1.5x in visual width across 1366x768, 1080p, and 1440p resolutions without soft-wrapping.
  3. **Faction-Specific Advisor (Rochefort vs Vice Admiral Ugaki):**
     - **Allied Player:** Retains Station Hypo Cmdr. Joseph Rochefort, displaying `[ CMDR. J. ROCHEFORT ]`, `[ STATION HYPO DECRYPT & INTEL BRIEF ]`, Station Hypo portrait, and Allied SIGINT/traffic analysis interpretations.
     - **Japanese Player:** Never shows Rochefort. Instead displays Combined Fleet Chief of Staff Vice Admiral Matome Ugaki (`[ V.ADM. M. UGAKI ]`), `[ COMBINED FLEET INTEL & WAR DIARY BRIEF ]`, authentic dithered Ugaki portrait (`assets/ugaki_lowres.png`), and authentic Imperial Japanese Navy commentary grounded in *Senkoroku* war diary entries, *Kantoku* decisive battle doctrine, and Combined Fleet staff directives.
     - Both portraits embedded as WPF application resources in `ROCHEFORT6000.csproj`.

- 2026-09-25 00:45 UTC+3: **Cmdr. Rochefort Intelligence Panel & Option C Dynamic Scaling (Ozgur):**
  1. **Dual-Box Right Panel Layout:** Partitioned `TerminalGrid` into a primary terminal column and a secondary right-side intelligence console (`0.40*` width):
     - `[BOX 1]`: Cmdr. Joseph Rochefort portrait frame with glowing phosphor border and identity ribbon.
     - `[BOX 2]`: Station Hypo intelligence brief terminal with domain interpretations and amber/phosphor text glow.
  2. **Screen Coverage:** Active across all analysis pop-up and modal views (`ReasoningModal`, `HeuristicsPopUp`, `IntelDetailPopUp`, `PredModelPopUp`).
  3. **Live Analytical Speech Synthesis:** Ingests live telemetry models (`RegionalShifts`, `SubmarineCentroids`, `MineHitCoordinates`, `ReasoningTopics`, confidence ratings) into narrative tactical appraisals via `GetRochefortComment()`.
  4. **Option C Proportional Scaling:** Recalibrates terminal font size by factor `0.72x` when the panel is active, preserving rigid 78-column monospace box borders (`boxWidth = 78`) with 100% straight vertical right margins and zero text clipping.

- 2026-09-24 23:55 UTC+3: **ROCHEFORT 6000 Identity Rebranding & Boot Diagnostic Redesign (Ozgur):**
  1. **System Identity Rebranding:** System identity transitioned to **ROCHEFORT 6000** across UI window titles, system identity badges, baud-rate menu labels, context menus, tooltips, font selector windows, and automated test runners. Codebase namespaces (`namespace ROCHEFORT6000`) and git origin binding are preserved to avoid build or project breakage.
  2. **Diagnostic Boot Screen Redesign (Option 1):** Implemented Option 1 layout in `BuildBootScreen()`:
     - Header: `AppendHeader(sb, "ROCHEFORT 6000 // SYSTEM BOOT DIAGNOSTIC");`
     - Added dynamic heuristic (`>> 6 HEURISTICS READY`) and graphics counts (`>> 10 GRAPHIC SCREENS READY`).
     - Removed legacy `PACIFIC CAMPAIGN TELEMETRY HARVEST` block.
     - Centered 6-line high-impact block ASCII art banner for `ROCHEFORT`.
     - Standardized `[ SYSTEM READY ]` bottom banner and prompt: `PRESS ANY KEY OR [ENTER] TO ACCESS OPERATIONAL THEATER DECK`.

- 2026-09-24 23:20 UTC+3: **Complete TBD Standardization Across Graphics Sub-Menu (Ozgur):**
  1. **All 10 Items Replaced with TBD:** At user direction (*"delete all of these pages and replace menu items with TBD"*), all 10 slots `[1]` through `[10]` in the Graphics Sub-Menu are rendered as `[ TBD ]`.
  2. **Standardized TBD Placeholder Routing:** Selecting any item routes to `BuildGraphicsTbdScreen(_graphicsPageId)`, rendering the clean reserved partition monospace box (`boxWidth = 78`, `innerWidth = 74`).
  3. **Analytical Math Maintained:** Core algorithms in `TelemetryAnalyticsEngine.cs` remain active in the analytics domain.

- 2026-09-24 23:10 UTC+3: **Graphics Sub-Menu Telemetry Views (Pages 1–5) & TBD Reservation (Slots 6–10) (Ozgur):**
  1. **User Alignment Confirmed:** Slots 6 through 10 in the Graphics Sub-Menu remain strictly designated as `[ TBD ]` placeholders with rigid monospace borders. Pages 1 through 5 are fully implemented from blueprints in `docs/OPENCODE INPUT/`.
  2. **Page Definitions & Screen Routing:**
     - `[1] Logistical & Attrition Telemetry`: Naval & air attrition metrics, K/L ratio with linear regression slope, OPTEMPO 20-turn curve, logistical readiness meters, and commander tactical assessment box.
     - `[2] Strategic Vector Analysis`: Main effort vs secondary effort concentration %, Regional Power Shift % by sector, 10x10 regional Japanese force centroid mapping (CoG), advance vector heading and hex/day magnitude, and logistical shadow tracking.
     - `[3] Intelligence & Anomaly Detection`: Sighting clustering by sector, sighting spike alert, blindspot detection, 3-entry tactical anomaly log table, and sector saturation density gauges.
     - `[4] Operational Tempo & Pivots`: Tempo volatility variance, offensive breath hex count, strategic pivot latency time-elapsed day chart, and theater coordination symmetry pair tracking.
     - `[5] Predictive Forecasting`: Global Tempo State Machine (Build-Up, Strike, Consolidation, Withdrawal) phase with confidence, Japanese advance axis vector projection, submarine migration cluster tracking, mine pathing trend, and sector blindspot risk.
     - `[6]` through `[10] [ TBD ]`: Standardized reserved partition placeholder screens.
  3. **Rigid Monospace 78-Col Border Rule:** All pages maintain `boxWidth = 78`, `innerWidth = 74`, with right border `║` strictly vertical.

- 2026-09-24 22:35 UTC+3: **Proportional Dynamic Font & Chrome Scaling System (Option 1) (Ozgur):**
  1. **Dynamic Font Scaling:** In higher screen resolutions (1280x720, 1366x768, 1920x1080 Full HD, and 2560x1440 QHD / 2K), terminal text (`TerminalOutputBox.FontSize`) scales dynamically based on window dimensions to utilize the full screen area without manual tweaking.
  2. **Harmonized Header and Footer Scaling:** The top retro title bar and bottom status/telemetry footer bar dynamically scale their font sizes, element heights, and padding proportionally with the main terminal text so they never appear shrunken or disconnected at high resolutions.
  3. **Paragraph Line-Height Proportionality:** The terminal's Paragraph `LineHeight` dynamically scales with font size (`FontSize * 1.35`) ensuring 100% vertical padding consistency and preventing text clipping or compression.
  4. **QHD (2560x1440) Preset Integration:** Added native 2560x1440 selection to the Resolution pull-down menu alongside existing presets.
  5. **Monospace & Straight Border Preservation:** Monospace character aspect ratio is strictly preserved across all resolutions, maintaining 100% straight right borders (`boxWidth = 78`, `innerWidth = 74`).

- 2026-09-24 09:14 UTC+3: **Graphics Option [1] Replaced with TBD Placeholder (Ozgur):**
  1. **Option [1] Replacement:** The `Logistical & Attrition Telemetry` screen is removed from the active graphics display, and item `[1]` in the Graphics sub-menu is transitioned to a strict placeholder: `[ TBD ]`. All 10 items `[1]` through `[10]` in the Graphics Sub-Menu now strictly display `[ TBD ]`.
  2. **Page 1 Navigation & Display:** Selecting option `[1]` (or any option `1` through `10`) now opens a standardized `TBD` placeholder screen (`WAR ROOM : GRAPHICS TELEMETRY SCREEN // MODULE STATUS : RESERVED TELEMETRY PARTITION (TBD)`) with rigid monospace straight borders, awaiting future user wireframe blueprints.
  3. **Analytical Engine Preservation:** Core mathematical logic in `TelemetryAnalyticsEngine.cs` is retained in the analytical layer for background processing and subsequent graphic wireframes.

- 2026-09-23 22:23 UTC+3: **War Room Graphics Sub-Menu (10 Items), Analytical Algorithms & Page 1 Telemetry (Ozgur):**
  1. **War Room Option [3] Renamed to Graphics:** Option `[3]` in the War Room Primary Screen is officially transitioned from `TBD` to `Graphics` (reached by pressing `3` or selecting option 3 and pressing Enter).
  2. **10-Option Graphics Sub-Menu:** Selecting `Graphics` opens a dedicated 10-item navigation sub-menu:
     - `[1] Logistical & Attrition Telemetry`
     - `[2] - [10] [ TBD ]` (Strict user directive: No speculative category names; retained strictly as `[ TBD ]` until user supplies explicit blueprints).
  3. **Ingestion & Implementation of Analytical Algorithms (`analytical_algorithms.md`):**
     - **Spatial & Vector:** Center of Gravity (CoG), Axis of Advance (Vector Projection), Coordinate Clustering (Force build-up / sighting spikes).
     - **Temporal & Attrition:** Ordinary Least Squares (OLS) Linear Regression Slope ($m$), Operational Tempo (OPTEMPO) 3-day rolling average, Strategic Pivot Latency.
     - **Logistical & Anomaly:** Logistical Shadow (Range estimation & exhaustion point), Intelligence Blindspot Detection, Bait Pattern Detection.
     - **Global Tempo State Machine:** Weighted scoring for Build-up, Strike, Consolidation, and Withdrawal phases.
  4. **First Graphical Presentation View (Page 1: `Logistical_Attrition_Telemetry_page1.md`):**
     - Full-screen in-terminal ASCII/Unicode presentation screen:
       - Header: `ROCHEFORT 6000 // [ NAVAL & AIR ATTRITION METRICS (CAMPAIGN DURATION) ]`
       - Section 1: Attrition & Drain metrics (K/L ratio with OLS regression slope `[ IMPROVING ]` or `[ DEGRADING ]`, Air losses / Ship losses rate bars).
       - Section 2: Operational Tempo (OPTEMPO) 20-turn time-series curve with Braille/character curve plot (`⢠⣶`, `⢀⣴⡿⠋`, `⢀⣠⡾⠟⠁`, `⢀⣀⣤⣴⣶⡶⠿⠿⠋⠁`) and X/Y axes (`EVENTS` 0..20 vs `TURNS` D01..D20).
       - Section 3: Logistical readiness meters (`[ESCORT DEN]`, `[FUEL LIMIT]`, `[EXHAUSTION]`).
       - Section 4: Tactical assessment & commander deduction box.
       - Rigid straight box border constraint: `boxWidth = 78`, `innerWidth = 74`, right border `║` 100% straight with zero bending.
  5. **Verification & Quality Assurance:** Comprehensive unit tests covering all mathematical algorithms, state machine transitions, menu navigation, and straight box borders. Release binary `dist/witp-cic-rochefort-alpha4-5.exe`.

- 2026-09-22 22:15 UTC+3: **War Room Graphics Domain & ASCII Wireframe Blueprint Template (Ozgur):**
  1. **Rename War Room Option [3] to Graphics:** Option `[3]` in the War Room Primary Screen is officially transitioned from `TBD` to `Graphics`.
  2. **Graphics Sub-Menu Categories:** Selecting Graphics opens a dedicated 6-item navigation sub-menu:
     - `[1] Trend Lines (The "What's Happening" Graphs)`
     - `[2] Correlation & Logic Graphs (The "Why it's Happening" Graphs)`
     - `[3] Strategic & Intelligence Overviews`
     - `[4] Tactical & Operational Telemetry`
     - `[5] [ TBD ]` (retained as unassigned TBD per user directive)
     - `[6] [ TBD ]` (retained as unassigned TBD per user directive)
  3. **Full-Screen Graphical Presentation View:** When a category is chosen, the terminal renders a full-screen native monospace visualization matching the exact architecture of `media_1790103739294.jpg` (top metric rate bars, central time-series Braille/scatter curve, lower readiness gauges, and bottom tactical assessment box) with rigid monospace straight borders.
  4. **ASCII Wireframe Specification Template:** Established `docs/graphics-blueprints-template.md` as an ASCII-art wireframe template allowing Ozgur to directly edit mockups, data sources, math formulas, and axis parameters before implementation begins.

- 2026-09-22 21:45 UTC+3: **Header Phosphor Relocation, In-Terminal Unicode Graphs & Straight Border Assurance (Ozgur):**
  1. **Phosphor Quick-Switch Header Relocation:** Moved the quick-switch phosphor color buttons (`PHOSPHOR: [ Matrix Green ] [ Solar Amber ] [ Nostromo Cyan ] [ Glitch Red ]`) to the top draggable header bar directly beside `[ PACIFIC THEATER CIC MATRIX ]` per screenshot `media_1790098927695.png`.
  2. **In-Terminal Unicode & Character Graphing (ADR 64):** Implemented native monospace graphs inside the Heuristics domain pop-up:
     - Metric 1 (Regional Shift): Horizontal delta bar chart `[►████░░░░]` / `[◄████░░░░]`.
     - Metric 4 (Minefield Mapping): Coordinate timeline with hit markers `──■──`.
     - Metric 5 (Submarine Centroid Vectors): Contact density meter `[■■■···]` and directional vector velocity indicator `[==►  ]`.
     - Metric 46 (Intelligence Blindspots): Surveillance gap sensor meter `[░░░░░░ GAP 0%]`.
     - Metric 49 (Axis of Advance): Fleet concentration meter `[████░░░░]` and strategic advance speed bar.
     - Metric 50 (Operational Tempo): 16-character combat activity histogram `[████░░░░░░░░░░░░]`.
  3. **Rigid Monospace Box Alignment (Straight Right Border):** All rows inside terminal pop-up frames (`║ ... ║`) are strictly constrained to 74 inner monospace character columns, guaranteeing that the right border `║` never shifts, bends, or wobbles. Verified with automated regression assertions in the test suite.
  4. **Release Binary Build 4:** Published and smoke-tested single authoritative executable `dist/witp-cic-rochefort-alpha4-4.exe` (no redundant mirrors per ADR 63).

- 2026-09-22 20:25 UTC+3: **Evidence Transparency, Resolution Pull-Down & Phosphor Ribbon Controls (Ozgur):**
  1. **Full Evidence Transparency & Mathematical Proofs:** Pop-ups explicitly disclose mathematical formulas, input parameters, contributing source files (`asigint`, `combatreport`, `operations`), hex coordinates, dates, and raw excerpts across all 6 Heuristics metrics. Sourcing lines and mathematical derivations route to Topic 1 and Topic 3 for complete user verification.
  2. **Monospace Resolution Dropdown Menu:** Added pull-down menu to bottom status ribbon (`1280 x 720 HD`, `1366 x 768 720p`, `1920 x 1080 Full HD`, `1024 x 640 Standard`) to the right of `STATUS: CONNECTED / READY`. Resizes window, centers on screen, and persists to settings.
  3. **Phosphor Quick-Switch Pill Buttons:** Integrated retro pill buttons into bottom status ribbon matching `media_1790096672384.png`: `PHOSPHOR: [ Matrix Green ] [ Solar Amber ] [ Nostromo Cyan ] [ Glitch Red ]` for instantaneous console re-tinting.
  4. **Elimination of Text Truncation:** Multi-line wrapping (`WrapText`) prevents horizontal truncation in Reasoning modal headers.
  5. **Release Binary Compilation:** Compiled and smoke-tested single authoritative binary `dist/witp-cic-rochefort-alpha4-3.exe` with zero redundant mirrors.

- 2026-09-22 19:35 UTC+3: **War Room Heuristics Domain & The 6 Logical Blueprints (Ozgur):**
  1. **Dedicated Heuristics Domain:** Established a new analytical domain accessible as option `[6]` on the War Room Primary Screen.
  2. **6 Logical Blueprints Implemented:**
     - **Metric 1 (Regional Power Shift):** Granular tracking across 10 strategic regions (Malaya, DEI, Philippines, New Guinea, Solomon Islands, South Pacific, Home Islands, China/Manchuria/Formosa, Mandates, Central Pacific) computing 7-day power shift deltas.
     - **Metric 4 (Minefield Mapping):** HashSet-based spatial deduplication of minefield hits extracted from `combatreport`, tracking chronological passage through mine barriers.
     - **Metric 5 (Submarine Migration):** Unified submarine contact aggregation across `asigint`, `Combat_Events`, and `combatreport` calculating daily spatial centroids and migration vectors.
     - **Metric 46 (Intelligence Blindspots):** Cross-correlation of combat engagements against 3-day prior SIGINT/sightings to isolate unmonitored approach axes.
     - **Metric 49 (Axis of Advance):** Fleet mean center calculations and 3-day rolling Strategic Drift vector computation.
     - **Metric 50 (Operational Tempo):** Rolling 3-day OPTEMPO calculation based on `combatreport` `---` engagement separator frequency.
  3. **Temporal Guard (< 4 Days Old):** Added automatic guard displaying `"NOT ENOUGH DATA // NEED MORE DATA POINTS - WAIT SOME TIME MORE"` when fewer than 4 daily reports exist.
  4. **Drill-Down Evidence Modals:** Interactive pop-up modals disclosing mathematical derivations and raw telemetry excerpts for independent user verification.
  5. **Clean War Room Primary Styling:** Removed the outer text box surrounding choices on the War Room Primary Screen, presenting options cleanly and directly.

- 2026-09-21 23:58 UTC+3: **Elimination of Redundant Binary Mirrors & Aliases (Ozgur):**
  1. **Strict Single-Binary Deliverable:** Permanently eliminated generation of duplicate mirror binaries (`WitP-CIC.exe`, `secondary alias binaries`).
  2. **Authoritative Versioned Output Only:** Every release build must output exclusively the single versioned executable matching `dist/witp-cic-rochefort-alpha4-[build#].exe` (e.g. `dist/witp-cic-rochefort-alpha4-1.exe`, `dist/witp-cic-rochefort-alpha4-2.exe`).

- 2026-09-21 23:51 UTC+3: **Comprehensive Repository Reference & Current Reality Alignment (Ozgur):**
  1. **Documentation Alignment:** Scanned all project documentation, in-app Wiki text, and configuration files, updating all outdated references to the old repository (`oaltinyay70-cell/WITP-CIC2`) and workspace paths (`Witp-CIC2`) to reflect the current active reality.
  2. **Active Git Authority:** Formally codified `origin` -> `https://github.com/oaltinyay70-cell/witp-cic-rochefort.git` on branch `main` as the active repository for `witp-cic-rochefort`, with `C:\Users\ozgur\Documents\witp-cic-rochefort` as the primary workspace and `Witp-CIC2` preserved strictly as an immutable reference archive.

- 2026-09-21 23:48 UTC+3: **Core Development, Testing & Scope Rigor Baseline Mandate (Ozgur):**
  1. **Mandatory Pre-Delivery Testing:** Always run automated tests and smoke test binaries before delivery.
  2. **Zero Hallucination Standard:** Never invent mechanics, game rules, file formats, or assumptions. Ground all logic in verified telemetry.
  3. **Strict Test Pass Gate:** Never finish or conclude a task until all tests pass with 100% success.
  4. **Exact Output Coverage:** Test assertions must strictly cover the exact requested user outputs.
  5. **Scope Alignment & Active Clash Clarification:** Align all additions and corrections with `docs/scope-v1.md`. If clashes or ambiguities exist, stop and ask Ozgur immediately.
  6. **Exhaustive Foolproof Documentation:** Every piece of added functionality must be documented in foolproof detail.
  7. **Algorithm Logic Clash Alerting:** Actively monitor for logic clashes in algorithms and state machines, and immediately alert Ozgur.

- 2026-09-21 23:32 UTC+3: **Phase Progression to Alpha 4 & Binary Naming Convention (Ozgur):**
  1. **Phase Label:** Officially advance the project phase designation to **Alpha 4** (`alpha4`).
  2. **Binary Naming Convention:** All executables follow `witp-cic-rochefort-alpha4-[build#].exe`, where `alpha4` is the new phase name and `0` is the 1st build incrementing by 1 (e.g. `witp-cic-rochefort-alpha4-0.exe`), with canonical mirrors to `dist/WitP-CIC.exe` and `dist/secondary alias binaries`.

- 2026-09-21 22:20 UTC+3: **Project Switch to Standalone `witp-cic-rochefort` Repository (Ozgur):**
  1. **Canonical Master Repository Switch:** Formally established `witp-cic-rochefort` (`C:\Users\ozgur\Documents\witp-cic-rochefort`) as the active, canonical master repository. `Witp-CIC2` is locked, clean, and preserved strictly as an immutable reference archive.
  2. **Platform & Architecture Integration:** Adopted the ROCHEFORT 6000 vintage CRT phosphor hardware-accelerated GUI shell (.NET 8 WPF) with full analytical parity across all 5 War Room domains, Predictive Models A–G, Field Manual, Wiki, and Settings.
  3. **Strict Zero-Shake & Instant Drawing Standard:** CRT flicker, screen shake, and typewriter stream delays are eliminated from interactive navigation to provide instantaneous 60 FPS phosphor UI rendering with keyboard cursor traversal.
  4. **Full Knowledge & Golden Asset Migration:** 100% of governance documentation, lessons learned, golden PBEM telemetry fixtures, base location registries, and test suites are consolidated into `witp-cic-rochefort`.

- 2026-09-21 20:53 UTC+3: **Phase Progression to Alpha 3 (Ozgur):**
  1. **Phase Label:** Officially advance the project phase designation to **Alpha 3** (`v0.02-alpha3`).
  2. **Build Progression:** Next compiled binary resets build counter to 0 for the new phase ID: `dist/witp-cic-v0.02-alpha3-0.exe` (with `dist/WitP-CIC.exe` mirrored).

- 2026-09-21 20:48 UTC+3: **Deletion of Battle Claims Corrections & Model E from Scope & To-Do (Ozgur):**
  1. **Excised Feature:** Formally deleted "Battle Claims Corrections: BDA claim vs later contradiction, displayed as revision under 4.2 Tab 1 — Intelligence Summary (§4.5)" from the active product scope and roadmap per explicit user directive.
  2. **Model E Retirement:** The cross-turn entity reconciliation engine (Model E) is retired from active to-do tracking alongside the Tab 1 Battle Claims display.

- 2026-09-21 20:46 UTC+3: **Permanent Deletion of Game File Writing/Modification from Scope & To-Do (Ozgur):**
  1. **Deletion from Roadmap & Scope:** Formally excised any prospective writing or modification of game files from the active to-do list and scope boundary (§10).
  2. **Absolute Read-Only Guarantee:** Reaffirmed the core architectural principle: WITP-CIC is strictly a passive, read-only PBEM intelligence assistant. The application will never write to, edit, or modify game files in v1.0 or any future release.

- 2026-09-21 20:43 UTC+3: **Scope Reduction & Deprecation of Legacy Predictive Modules (Ozgur):**
  1. **Deletion of Deprecated Modules:** Formally removed Module C (Air Superiority & Attrition Forecasting), Module D (Logistical & Operational Pause Detection), and Module E (Internal Security & Partisan Threat Modeling) from the project scope and active to-do roadmap per explicit user directive ("no longer needed").
  2. **Model Letter Realignment:** The original §7.2 definitions of Module C (Air Superiority Attrition curve), Module D (TF fuel/pause windows), and Module E (garrison/partisan threat vectors) are retired. In the War Room analytics framework, Model E remains dedicated to Battle Damage Assessment & Claims Revision Register (Entity Match / Sunk Set Reconciliation).

- 2026-09-21 20:41 UTC+3: **Alpha 2 Build 5 Recovery Point & Zero-Contamination Protocol (Ozgur):**
  1. **Pre-Major Addition Checkpoint:** Established an absolute, immutable recovery checkpoint prior to introducing large-scale functional additions. Guarantees zero mixing or contamination between past efforts and upcoming additions.
  2. **Multi-Layered Preservation:** Tagged `checkpoint-pre-major-addition-alpha2-build5`, created branch `recovery-point-alpha2-build5`, generated physical archive `backups/WITP-CIC2-RECOVERY-POINT-alpha2-build5.zip` (42 MB), and codified rollback procedures in `docs/recovery-point-alpha2-build5.md`.

- 2026-09-21 18:54 UTC+3: **Turn Count Disambiguation & Multi-Day Ingestion Verification (Ozgur):**
  1. **Disambiguation of Item Count vs. Turn Count:** Fixed an acute UI conflation bug where pop-up window headers prepended `"Turn: "` to item counters (e.g. `Layout::renderHeader(title, "COUNT: 3")` rendered as `Turn: COUNT: 3`), causing the user to believe the campaign had only loaded 3 turns.
  2. **Comprehensive Header Instrumentation:** Pop-up and screen headers are instrumented to explicitly display both the current turn date, the total days loaded in the campaign vault, and the separate item count: `Turn: <FormattedDate> (<N> Days Loaded) | ITEMS: <X>`.
  3. **Verification of Multi-Day Telemetry Ingestion:** Confirmed that all days ingested into the active `.wic` vault (8 operational days: Dec 07, 08, 10, 11, 12, 13, 14, 15 across 60 files) are fully and exhaustively processed by `WarRoomAnalytics`. All sightings, SIGINT radio intercepts, combat reports, pilot kills, and losses across all dates are actively incorporated into Models A through G, HVT tracking, Tab 2 geospatial bearing logs, and chronological observation histories.
  4. **Robust Subtitle Handling:** `Layout::renderHeader` updated so that `"Turn: "` is only prepended if the right subtitle is an actual date string, and never prepended if the string already contains `"Turn:"`, `"ITEMS:"`, `"COUNT:"`, `"MATCHES:"`, or custom section markers.

- 2026-09-21 18:40 UTC+3: **Hierarchical Reasoning Modal Drill-Down & Program-Wide Text Cutoff Fix (Ozgur):**
  1. **Hierarchical 2-Tier Modal Navigation:** The reasoning and telemetry disclosure modal transitions from a static single-view dump into an interactive, 2-tier navigable topic directory. The user traverses topic headers with arrow keys (`[UP/DOWN]` or numeric shortcuts `[1]`, `[2]`, `[3]`). Pressing `[ENTER]` on a topic opens a focused drill-down modal revealing the granular evidence and deductions that led to the prediction.
  2. **Merged Sighting & Telemetry Source:** `TELEMETRY SOURCE` and `LOCALIZED POSITION` are unified into a single drill-down topic: `[1] TELEMETRY SOURCE & LOCALIZATION HISTORY`. Drilling down into this topic discloses the exact detection means (SIGINT, visual recon, air patrol, combat reports), observation frequency (single sighting vs. multi-turn repetitions), and observation chronology (tracked until latest turn date and full history of dates seen).
  3. **On-Demand Confidence Derivation:** Mathematical computation breakdowns (`Base Score`, `Corroboration Bonus`, `Time Decay`, `Final Calculation`, `Analyst Note`) must **NEVER** be displayed on the top-level directory view. They are strictly segregated into drill-down topic `[3] CONFIDENCE DERIVATION & MATHEMATICAL PROOF` (or `MATHEMATICAL FORMULATION & REASONING`), disclosed solely when the user explicitly highlights the topic and presses `[ENTER]`.
  4. **Program-Wide Text Cutoff Elimination:** Every single detail line across all topics is processed through strict word wrapping (`wrapText`) to inner box width (`innerWidth = 74`), indenting continuation lines by 4 spaces. Text clipping into or beyond the right border is 100% eliminated. Long titles wrap across multiple rows in inverted phosphor without truncation or ellipses.
  5. **Modal Screen Isolation:** Every modal entry and drill-down transition executes `Console::clear()` and a sombre navigation audio click, completely wiping background screen artifacts and bleed-through.

- 2026-09-21 18:27 UTC+3: **Bold Headers & Program-Wide Text-Wrapping Baseline (Ozgur):**
  1. **Bold Headers Program-Wide:** Screen titles, section titles, master menu instrumentation, and pop-up modal headers must be rendered in **BOLD** across the application using ANSI `\033[1m` bold styling with Win32 virtual terminal processing (`ENABLE_VIRTUAL_TERMINAL_PROCESSING` / `0x0004`) and light green intensity.
  2. **Zero Text Truncation / Cutoff Across Boxes:** Eliminate text cutoff across all modal and pop-up boxes program-wide. Multi-line word wrapping is enforced for item titles, reasoning evidence disclosures, and notification prompts. Substring truncation (`...`) and border overflow are strictly eliminated.
  3. **Modal Isolation & Screen Cleansing:** All pop-up modals (specifically War Room Reasoning & Telemetry Evidence modal) must clear the terminal screen on entry to eliminate background screen bleed-through.
  4. **Elimination of Off-By-One Wrapping Drop Bug:** Calculation of visible modal content lines must account for all header, title, and meter rows, ensuring the final wrapped lines of telemetry disclosures are never truncated or dropped.

- 2026-09-21 18:15 UTC+3: **Opponent-Only Asset Display in Intelligence War Room (Ozgur):**
  1. **Core Intelligence Scope:** In the War Room / Intelligence CIC, all intelligence sighting records, battle claims, damage revisions, and BDA displays must track **ONLY THE OPPONENT'S ASSETS** (Japanese assets in an Allied campaign; Allied assets in a Japanese campaign).
  2. **Operational Rationale:** The player already knows what is happening to friendly forces through native operational reports, ship records, and task force management screens. Friendly casualties, damage, and sinkings must NEVER be shown in enemy intelligence menus.
  3. **Display & TUI Standards:** War Room Tab 1 Intelligence Summary item 4 (`[ BATTLE DAMAGE ASSESSMENT & CLAIMS REVISION REGISTER ]`), Predictive Intelligence Sub-Menu item 4 (`MODEL E`), and associated claim counters must filter strictly for opponent assets. If no hostile claims exist for the active turn range, display clean status: `NO HOSTILE COMBAT DAMAGE CLAIMS RECORDED FOR THIS TURN RANGE.`

- 2026-09-21 15:05 UTC+3: **Master Hex Map Geospatial Applications Codified into Scope (Ozgur):**
  1. **Maritime Obstacle Avoidance & Nautical Pathfinding (Model B):** Binary land/sea hex collision mask enables A*/Dijkstra maritime pathfinding through navigable straits (Luzon, San Bernardino), eliminating unrealistic straight-line projections over land.
  2. **Strategic Chokepoint & Convoy Interdiction (Model G):** Identifies natural bottlenecks (Malacca, Sunda, Makassar, Surigao, San Bernardino, Tsushima) to forecast mandatory crossing times and generate submarine ambush windows.
  3. **In-Terminal Tactical Mini-Radar / Sector Map (Tab 2):** Renders a 40x20 CP437 ASCII/ANSI sector radar centered on selected contacts with coastlines (`#`), friendly ports (`O`), and blinking contact vectors (`*`).
  4. **Land-Based Air Search & Strike Umbrellas (Models A & F):** Concentric range rings around enemy airbases (3-hex torpedo strike, 5-hex medium bomber, 8-hex search) alerting to air danger zones and blind spots.
  5. **Amphibious Beachhead Approach Lanes (Model A):** Combines target base coordinates with coastal approach hexes to project assault transport vectors (AP, APA, LST) from staging bases to landing beaches.
  6. **Hex Grid Calibration & Coordinate Ground-Truth:** Validates doubtful PBEM reports and `.pws` coordinates against visual ground truth.

- 2026-09-21 15:03 UTC+3: **Binary Build Number Auto-Increment Baseline (Ozgur):**
  1. **Strict Build Counter Progression:** When compiling a new executable, the build number component of `witp-cic-[mainversion id]-[phaseid]-[built no]` MUST strictly increment by 1 on each compilation.
  2. **Active Progression:** Transitioning active binary from `dist/witp-cic-v0.02-alpha2-0.exe` to `dist/witp-cic-v0.02-alpha2-1.exe`. Canonical mirror `dist/WitP-CIC.exe` is updated in lockstep.

- 2026-09-21 15:00 UTC+3: **Master Theater Hex Map Image Asset Stored (Ozgur):**
  1. **Authoritative Master Hex Map Ingested:** High-resolution Gary Grigsby's War in the Pacific: Admiral's Edition official theater hex map image stored at `assets/maps/witp_ae_full_hex_map.jpg` and mirrored at `docs/witp_ae_full_hex_map.jpg`.
  2. **Coordinate Origin:** Confirmed top-left is $(0,0)$, mapping directly to the game's native hex grid system.
  3. **Future Strategic Scope:** Groundwork established for land/sea terrain masking, maritime pathfinding (obstacle-avoiding dead reckoning in Model B), chokepoint interdiction modeling (Model G), air search arc overlays, and CP437 ASCII / graphical sector map rendering in Tab 2.

- 2026-09-21 14:45 UTC+3: **Comprehensive Base Location Registry & Hex Grid Database Ingestion (Ozgur):**
  1. **Theater-Wide Base Registry Ingested:** 1,330 authentic base locations with hex coordinates (X, Y) across 71 geographical regions ingested into `BaseLocationRegistry` (`base_locations.json`, `base_registry.hpp`, `base_registry.cpp`).
  2. **Housing Semantics & Maritime Constraints:** Base locations house units, ships, or planes; only coastal/maritime bases house ships (`isPort` flag determined by maritime access and port infrastructure).
  3. **Dynamic Ownership & Changing Controllers:** Initial country/regional designation (e.g., `JAPANESE EMPIRE`, `PHILIPPINES`, `DUTCH EAST INDIES`) serves as turn-1 baseline. Base ownership changes dynamically across turns upon capture events (`updateBaseController()`).
  4. **Predictive Analytics & Geospatial Integration:**
     - **Model A (Amphibious Assault):** Dynamic coordinate resolution for any target base mentioned in unit loadouts/embarkations.
     - **Model B (Dead Reckoning Trajectory):** Dynamic nearest base resolution within dead reckoning confidence radii for projected contact endpoints.
     - **Model D (Logistical Pause & Bases):** Dynamic port and anchorage identification across the Pacific theater.
     - **Tab 2 Geospatial Sightings:** Distance and bearing calculated relative to authoritative base coordinates.
     - **Dossier Entity Search:** Full $O(1)$ indexed and substring search across all 1,330 base locations.
  5. **Strict Standard:** Zero assumptions, zero hallucinations. Authoritative hex grid ground truth.

- 2026-09-21 13:27 UTC+3: **Comprehensive Naval Unit Capabilities & Auxiliary / Tender Doctrine Registry (Ozgur):**
  1. **Core Prerequisite Established:** Complete naval classification schema across 7 functional classes (Major Surface Combatants & Carriers, Surface Escorts, Mine Warfare, Amphibious & Troop Transports, Cargo/Logistics Freighters, Mobile Tenders & Auxiliaries, Submarines) and detailed Auxiliary / Tender Doctrine guide.
  2. **Predictive Intelligence Integration:** All predictive analytical engines (Amphibious Assault Model A, Fleet Trajectory Model B, Logistical Pause & Base Status Model D, Strike Concentration Model F, Established Routes Model G) and intelligence reasoning modals must explicitly ground their deductions on these exact unit-type capabilities.
  3. **Field Manual Ingestion:** Incorporate the complete naval classification and tender guide into Chapter 1 of the in-app Field Manual (`1.15 Naval Vessel Classifications & Operational Roles` and `1.16 Mobile Tenders & Auxiliary Support Guide`) with bracketed citations stripped.
  4. **Strict Standard:** Ignore numbers between `[ ]`. Zero assumptions, zero hallucinations, zero errors.

- 2026-09-20 23:02 UTC+3: **Milestone Alpha 2 Transition & Standardized Binary Naming Baseline (Ozgur):**
  1. **Milestone Progression (Alpha 2):** The project officially advances from Version 0.02 (Stable Alpha 1) to **Alpha 2** (`v0.02-alpha2`).
  2. **Standardized Binary Naming Baseline:**
     - When compiling a new executable, the output filename format is strictly:
       `witp-cic-[mainversion id]-[phaseid]-[built no]`
     - `[mainversion id]`: Main version in `V.XX` format (e.g. `v0.02`).
     - `[phaseid]`: Lifecycle phase ID (e.g. `alpha2`, `beta`, etc.).
     - `[built no]`: Incremental build counter starting from `0` for each new main phase ID.
     - Active Alpha 2 initial executable: `dist/witp-cic-v0.02-alpha2-0.exe` (with `dist/WitP-CIC.exe` retained as canonical copy).

- 2026-09-20 22:27 UTC+3: **Numerical Confidence Engine Refinements: Scopes, Visual Recon & Math Breakdown (Ozgur):**
  1. **Corroboration Scope (Per-Turn Parser vs Game State):**
     - **Parser (Per-Turn Only):** Text parser evaluates corroboration within the current turn file only (`Option B`), preventing memory bloat and maintaining maximum parsing speed in C++. If an entity is mentioned $\ge 2$ times in that specific file, it receives the $+15$ bonus.
     - **Game State Manager (Cross-Turn Persistence):** When new contacts are received by the Game State, permanent memory is updated. If a contact already exists from a prior turn and recurs in the current turn, its timestamp updates to current, Time Decay resets to $1.0$, and confidence remains elevated.
  2. **Visual Reconnaissance (`VISUAL_RECON`) Handling:**
     - Distinct report type `VISUAL_RECON` parsing operational sightings (e.g. `"12 Japanese ships at 80,77 near Legaspi, speed 12, heading South"`).
     - **Base Score = 80** (reflecting high ground truth of direct observation).
     - **Corroboration Rules:** Does NOT populate `EntityMap` (counts/types lack named identities), but DOES populate `HexMap`. If SIGINT reports radio traffic at $(80,77)$ and Visual Recon observes ships at $(80,77)$, both receive the $+10$ Hex Corroboration Bonus.
  3. **Pop-Up Presentation Standard:**
     - **Clean Header:** `CONFIDENCE: 90% [VERIFIED]` (immediate tactical takeaway).
     - **Reasoning Body:** Full mathematical breakdown translated into plain English:
       - `Base Score: 75 (Unit Located)` or `Base Score: 80 (Visual Recon)`
       - `Corroboration: +15 (Entity corroborated) / +10 (Hex corroborated)`
       - `Time Decay: x1.0 (Reported this turn)`
       - `Final Calculation: (Base + Bonus) * Decay = Score%`
       - `Analyst Note: Operational assessment and context`

- 2026-09-20 22:08 UTC+3: **Numerical Intelligence Confidence Engine Specification (Ozgur):**
  1. **Strict Numerical Percentage Standard:** Replaced subjective confidence strings with purely numerical percentage scores (0%–100%) and categorical tiers calculated deterministically from PBEM reports and `.pws` telemetry. Never guess, invent, or hallucinate scoring logic; if unknown, ask Ozgur directly.
  2. **Conceptual Data Architecture:**
     - `RawReport`: Encapsulates parsed string tokens: Entity Name, Location, Hex, Report Type, and Domain.
     - `ConfidenceReport`: Encapsulates evaluated metrics: Base Score, Corroboration Bonus, Time Decay, Final Score, and Tier.
     - Fast Hash Maps / Dictionaries: `EntityMap` (Entity Name $\to$ observation count) and `HexMap` (Integer coordinate $X \times 1000 + Y \to$ observation count).
  3. **Phase 1 — Parsing & Classification (First Pass):**
     - Extracts turn Report Date from line 1.
     - High-performance non-regex pattern matching via `std::string::find` / `substr` / C `strstr`.
     - `RADIO_TRAFFIC`: Matches `"Radio transmissions"` $\to$ extracts Location/Hex $\to$ Base Score = 30.
     - `UNIT_LOCATED`: Matches `"is located at"` $\to$ extracts Entity Name (text before "is located at"), Location, Hex $\to$ Base Score = 75.
     - `UNIT_MOVING`: Matches `"is loaded on"` $\to$ extracts Entity Name, Destination $\to$ Base Score = 65.
     - `GARRISON_SIZE`: Matches `"men are based at"` $\to$ extracts Headcount, Location, Hex $\to$ Base Score = 50.
     - `SHIP_MOVING`: Matches `"is moving to"` (and not matched above) $\to$ extracts Ship Name, Destination, Hex $\to$ Base Score = 65.
     - Domain Inference:
       - `AIR`: Contains `"JAAF"`, `"AF Coy"`, `"Fighter"`, `"Bomber"`.
       - `NAVAL`: Contains `"AMc"`, `"Maru"`, `"Destroyer"`, `"Carrier"`.
       - `LAND`: Contains `"Infantry"`, `"Cavalry"`, `"Army"`, `"Regiment"`.
     - Maps: Pushes to temporary raw buffer, increments `EntityMap[EntityName]`, and increments `HexMap[HexID]`.
  4. **Phase 2 — Corroboration Calculation (Second Pass):**
     - Entity Check: If `EntityMap[EntityName] > 1`, adds $+15$ to Corroboration Bonus.
     - Hex Check: If report has Hex and `HexMap[HexID] > 1`, adds $+10$ to Corroboration Bonus.
  5. **Phase 3 — Time Decay & Final Math:**
     - Time Delta $\Delta t = \text{CurrentGameDate} - \text{ReportDate}$ (difference in turns).
     - Decay Multipliers:
       - $\Delta t = 0 \to \text{Decay} = 1.0$ (Turn current)
       - $\Delta t = 1 \to \text{Decay} = 0.8$ (Previous turn)
       - $\Delta t > 1 \to \text{Decay} = 0.5$ (Stale intelligence)
     - Mathematical Scoring:
       $$\text{Raw Score} = \text{Base Score} + \text{Corroboration Bonus}$$
       $$\text{Final Score} = \text{clamp}(\text{Raw Score} \times \text{Time Decay}, 0, 100)$$
     - Confidence Tier Stratification:
       - $0 - 39\% \to \text{UNCONFIRMED}$
       - $40 - 69\% \to \text{PROBABLE}$
       - $70 - 89\% \to \text{CONFIRMED}$
       - $90 - 100\% \to \text{VERIFIED}$
  6. **Phase 4 — Engine Integration & Performance:**
     - Permanent turn-state storage (`std::vector<ConfidenceReport>`).
     - Fast integer hex keys ($X \times 1000 + Y$) instead of string keys.
     - Elimination of C++ `<regex>` in favor of fast substring search.
     - Clean memory pooling and deallocation of temporary maps per turn parse cycle.

- 2026-09-20 21:40 UTC+3: **Geospatial Sighting Last-Seen Tracking, Anti-Bullshit Reasoning & Border Stability (Ozgur):**
  1. **Last-Seen Date Ingestion & Display:** PBEM report headers (e.g. `OPERATIONAL REPORT FOR Dec 14, 41`) are parsed to resolve turn dates when filenames omit dates. Multi-turn sightings are correlated into tracks, and the `[ DATE ]` column strictly displays the last time the TF was seen (`lastSeenDate`) formatted as `Dec 14`, ensuring the date column is never blank.
  2. **Word-Wrapping Over Truncation in Modals:** Eliminated all text slicing with `...` inside reasoning dialogs. Coordinates, bearings, and descriptions dynamically wrap across continuation lines with 4-space indentation, guaranteeing full visibility of all text.
  3. **Confidence Meter Bar & Slow Blinking Exception:** Integrated an authentic Confidence Meter Bar (`CONFIDENCE LEVEL: [==========] XX%`) grounded in report telemetry (air recon observers, ship counts >= 7, capital ship classifications, multi-turn sightings). Contacts with `>=90%` confidence trigger a slow 600ms in-place blink between `Inverted` and `LightGreen` without clearing the screen, implemented as an explicit user-mandated exception to the zero-blinking policy.
  4. **Strict Anti-Bullshit Policy for Reasoning Fields:** Boilerplate and fluff text in tactical reasoning are strictly prohibited. `TACTICAL REASONING:` and `INTERCEPT SIGNIFICANCE:` remain blank as `*-**-**-**-*-*` until grounded AI/predictive models analyze the contact.
  5. **Rock-Solid Modal Border Stability:** Box drawing in pop-ups and modals anchors left borders (`| `), padded content, and right borders (` |`) to fixed console columns. Highlight traversal no longer shifts line widths or scrambles box frames.

- 2026-09-20 21:18 UTC+3: **War Room Standard Menu & Interactive Reasoning Architecture (Ozgur):**
  1. **Tab Structure Replaced by Standard Menu:** Excised the previous horizontal tab bar. The War Room primary screen is now a 5-item Standard Menu:
     - `[1] Intelligence Summary`
     - `[2] SIGINT Map`
     - `[3] TBD` (Theater Battlespace Domain / Phase 08 placeholder)
     - `[4] TPIE Warning Dashboard`
     - `[5] Predictive Models Sub-Menu`
  2. **Hierarchical Sub-Menu Flow:** Selecting any menu item transitions the user into that topic's dedicated sub-screen / sub-menu.
  3. **Cursor Navigation across Pop-Ups:** All items and bullets within sub-menus and pop-up dialogs can be traversed with cursor keys (`[UP/DOWN]`), highlighting the active selection in inverted phosphor.
  4. **Analytical Reasoning on `[ENTER]`:** Pressing `[ENTER]` over any highlighted bullet displays a dedicated reasoning modal detailing the raw telemetry evidence, mathematical deductions, and operational rationale for that specific line.
  5. **Universal Footer Directive:** All sub-screens and modals standardize their return directive to: `PRESS ESC TO RETURN`.


- 2026-09-20 21:05 UTC+3: **Zero Blinking Policy & Ghost Fleet Counter Deletion (Ozgur):**
  1. **Zero Blinking Policy:** Prohibit any blinking, flashing, flickering, or high-frequency redrawing across all screens in the terminal interface. Redrawing must occur only upon user input. Prompt blinking on the splash screen is eliminated (steady light green). Future blinking is strictly prohibited unless specifically and explicitly requested by Ozgur.
  2. **Ghost Fleet Counter Deleted:** The "Ghost Fleet Counter" metric is completely deleted from the War Room Theater Combat Information Center UI and analytical models. Model E is redesignated strictly as `MODEL E : Battle Claims & Damage Revision Register` focusing on action damage assessment and sinkings reconciliation.


- 2026-09-20 20:28 UTC+3: **System Settings Sub-Item — Connect to Predictive AI Model (Ozgur):**
  1. **Master Menu Settings Integration:** Under Master Menu `[S] SYSTEM SETTINGS & DETECTION THRESHOLDS`, establish an interactive configuration screen replacing the previous status modal stub.
  2. **Predictive AI Model Connection Sub-Item:** Implement Sub-Item `[1] CONNECT TO PREDICTIVE AI MODEL` enabling the user to configure, test, and toggle predictive AI model endpoints (e.g. Local Analytical Heuristics Engine, Local LLM / Ollama bridge at `http://localhost:11434`, or Google Gemini API), with connection status monitoring.
  3. **Detection Thresholds & Parameters:** Provide user-configurable controls for Allied SIGINT confidence mode (`CERTAIN` vs `PROBABLE`), Strike Concentration cruiser threshold (default `3`), hex search radius (default `10`), and Amphibious Assault score sensitivity (default `15`).

- 2026-09-20 17:20 UTC+3: **In-Terminal Hierarchical JSON Manual Ingestion & Browser (Ozgur):**
  1. **Full In-Terminal Native Integration:** Removed external web browser dependency and external file launching. Ingested the curated WITP:AE Operations & Mechanics manual wiki directly into `assets/manual/witp_ae_manual.json` (and mirrored in `docs/witp_ae_manual.json`).
  2. **Options 2 and 3 Deleted:** Deleted previous options `[2] SEARCH MANUAL RULES` and `[3] BROWSE CHAPTER TEXT`. The Field Manual screen directly renders the hierarchical manual browser upon selection from the Master Menu.
  3. **Hierarchical Navigation UX:**
     - First menu displays `X.0` level topics (15 operational chapters: `1.0 Naval Units` through `15.0 Appendices`).
     - Selecting `[ENTER]` on a chapter drills down into `X.1` level topics.
     - Selecting sub-sections with children drills down into `X.1.1` level topics.
     - Selecting leaf sections (or overview nodes) opens the Nostromo article reader with line scrolling (`[UP/DOWN]`), page jumps (`[PGUP/PGDN]`), and `[ESC]` return navigation.
     - `[ESC]` returns up one level at any step (and back to Master Menu from root).
  4. **Nostromo MU/TH/UR UI Standards:**
     - Section header: `W I T P : A E   M A N U A L` at root, and `[ X.0 CHAPTER TITLE ]` at sub-levels.
     - NO "Table of Contents" label at the top.
     - NO `Index : ...` line.
     - Double-spaced menu options with inverted phosphor highlight.
     - Right-aligned `[ TOPIC EXPLANATION ]` pop-up box (columns 44–78) rendering section summaries in dimmed phosphor (`ConsoleColor::DarkGreen`).
     - Sombre click acoustic cues on movement (`UP/DOWN/TAB/PGUP/PGDN`) and teletype chimes on transitions.
  5. **Zero-Dependency C++ JSON Engine:** A recursive descent JSON parser is integrated into `field_manual_screen.cpp` for instant parsing of all 15 chapters and 414 section nodes with zero third-party dependencies.

- 2026-09-20 17:07 UTC+3: **War Room Predictive Intelligence Sub-Menu & Models Directive (Ozgur):**
  1. **Sub-Menu Architecture:** All predictive intelligence models listed below are implemented as a dedicated sub-menu item inside the War Room (`[A] War Room` → sub-menu entry). This is the navigation home for all analytical model results.
  2. **Model A — Amphibious Assault Probability Engine:** Weighted Evidence Accumulator (Heuristic Bayes) scoring per target hex using SIGINT radio volume (`Heavy=3`, `Normal=1`), SNLF/amphib unit embarkation lines (`+5` per unit), transport-class ship movements (`+2` per ship), confirmed amphibious TF unloading events (`+10`), and recency decay (`×0.9^Δt`). Score ≥ 15 → `>90% probability` alert. Displayable from turn 1 with a confidence caveat.
  3. **Model B — Fleet Trajectory & Intercept Projection:** Dead Reckoning vector projection over multi-sighting sequences (hex, speed, heading). Least-squares heading fit; T+1 position estimated as `(xₙ + speed × cos(θ), yₙ + speed × sin(θ))`; confidence radius expands with time since last sighting. Cross-references radius with known bases/ports for candidate destinations. Matures into a Markov predictor as sightings accumulate.
  4. **Model C — Air Attrition Forecasting:** Exponential Moving Average (`α=0.3`, tunable) of per-aircraft-type kill counts from Combat Events. Projects `days_to_parity = (pool − threshold) / attrition_rate`. Displays as: `"[Type]: estimated local superiority lost in ~N days at current attrition"`. Pool count flagged `ESTIMATED` until `0x20` binary fields are decoded; uses scenario OOB pool from Field Manual as fallback.
  5. **Model D — Logistical & Operational Pause Detection:** TF state machine (`ACTIVE → REFUELLING → REPAIRING → ACTIVE`) driven by Operational Report pattern triggers. Pause window = repair days + transit days. Alert: `"Identified operational pause at [port]; window ~N days"`.
  6. **Model E — Battle Claims Correction / Ghost Ship Detection:** Cross-turn entity reconciliation between text-declared sinkings and subsequent sightings of the same vessel name. Uses `.pws` tombstone (`0x08 @36 == 19999`) as ground-truth corroboration layer. BDA overclaims downgraded: `"Reported sunk [date]; re-sighted [date] at [hex] — assess DAMAGED"`.
  7. **Model F — Strike Concentration Detector:** Hex grid clustering scan per turn. For each candidate center hex, sums warship-class ships within configurable radius (default 10 hex); flags if cruiser-class count ≥ threshold (default 3). Thresholds driven by Settings screen values (§4.2).
  8. **Model G — Established Shipping Routes:** Repeated waypoint graph per merchant vessel across turns. Two or more sightings at different hexes → infer direction vector, identify nearest named port at each endpoint. Confidence = `min(1.0, sightings / 5)`.
  9. **Data readiness rule:** All models above feed from text report vault (SIGINT, Operations, Combat Events, Combat Report) immediately. `.pws` tombstone enriches Model E. Live ship hex, aircraft pool sizes, and damage fields remain undecoded (`docs/pws-field-map.md` §4); models display with `ESTIMATED` caveat where binary data is missing.

- 2026-09-20 16:45 UTC+3: **Field Manual: WITP:AE MANUAL Header & Interactive Wiki HTML Integration (Ozgur):**
  1. **Section Header Renamed:** Replaced `[ C H A P T E R   B R O W S E R   &   R A G   S E A R C H ]` with `[ WITP:AE MANUAL ]` (`Layout::renderSectionHeader("W I T P : A E   M A N U A L")`).
  2. **Index Counter Deleted:** Deleted the line starting with `Index : Chapter X of 24 | Total Document Lines: ...` from the Field Manual screen layout.
  3. **Content Replaced with Manual Wiki:** Under `[ WITP:AE MANUAL ]`, replaced the previous raw text chapter listing with the interactive manual wiki file `witp_ae_manual_wiki.html` located at `C:/Users/ozgur/Documents/WITP-CIC ASSETS/docs/rag/witp-ae/witp_ae_manual_wiki.html` (and mirrored locally at `docs/witp_ae_manual_wiki.html`).
  4. **Browser Launch Action:** Selecting Option `[1] OPEN WITP:AE MANUAL (HTML WIKI)` and pressing `[ENTER]` presents a modal notification and automatically launches the full interactive HTML wiki (with 2-column TOC, hyperlinks, and rules reference) in the default web browser via native Win32 `ShellExecuteA`.

- 2026-09-20 16:32 UTC+3: **Mandatory Upgrade, Backup, and Rollback Procedure Established as Baseline (Ozgur):**
  1. **Rollback Documentation (`docs/how-to-rollback-to-alpha-1.md`):** Formally created and published `how to roll back to Alpha 1` documentation covering all rollback options: Non-destructive Time Travel (`git checkout v0.02`), Full Hard Rollback (`git reset --hard v0.02`), Offline Backup Archive recovery (`backups/WITP-CIC2-v0.02-alpha1.zip`), and GitHub Release Archive download.
  2. **Mandatory Future Upgrade Procedure:** Established baseline rule that from now on, ANY upgrade must strictly follow the verified 6-step lifecycle procedure:
     - Automated verification of static binary `dist/WitP-CIC.exe` and 100% test pass.
     - Full synchronization across all Core Docs.
     - Standalone physical backup archive generation in `backups/`.
     - Git release tagging and stable branch tracking.
     - Full push to GitHub `origin` (branches and tags).
     - Rollback guide maintenance.

- 2026-09-20 16:15 UTC+3: **Version 0.02 (Stable Alpha 1) Milestone Release & Time-Travel Preservation (Ozgur):**
  1. **Milestone Baseline Lock:** Formal marking and preservation of Version 0.02 as **Stable Alpha 1**.
  2. **Feature Baseline Verified in Release:**
     - Operational Nostromo MU/TH/UR retro terminal interface in C++17 (static MinGW build `dist/WitP-CIC.exe`).
     - Animated splash screen with non-blocking ~1 Hz prompt blinking and zero console attribute bleed.
     - New Campaign wizard with auto-discovery of `.pws` saves and `.txt` reports, multi-directory accumulation (`[B]`), and instant physical `.wic` container generation under `C:\WitP-CIC\Campaigns`.
     - Continue Campaign flow with automatic incremental diff ingestion of newly arrived turn files (`.pws` & `.txt`) and automatic War Room analytical model recalculation.
     - Master Menu header instrumentation (`Files Processed : XX pws files | xx TXT files | Days Processed : xx`) partitioned cleanly by `|`.
     - Framed notification modal pop-up dialogs eliminating all screen-space text overwrites.
     - Recursive hierarchical System Wiki with right-aligned `TOPIC EXPLANATION` pop-up windows and decomposed Error Code Registry sub-topics (`[101]`, `[102]`, `[201]`, `[202]`, `[301]`, `[401]`).
     - Standalone Field Manual module browsing and searching the 6,720-line official manual across 24 chapters.
     - Restrained, sombre navigation click acoustic profile and silent audio background.
  3. **Time-Travel & Rollback Preservation Strategy:**
     - Git tags: `v0.02` and `v0.02-alpha.1` on `origin` (`https://github.com/oaltinyay70-cell/WITP-CIC2.git`).
     - Preserved branch: `stable-alpha-1`.
     - Offline physical backup archive: `backups/WITP-CIC2-v0.02-alpha1.zip`.
     - Time-travel rollback command: `git checkout v0.02` or `git checkout stable-alpha-1`.

- 2026-09-20 15:07 UTC+3: **Master Menu Header Instrumentation, Pop-up War Room Launch, & Right-Aligned Pop-up Explanations (Ozgur):**
  1. **Master Menu Header Instrumentation:** Relocate active campaign and database telemetry stats from the menu body into the top header block. Format: `Files Processed : XX pws files | xx TXT files | Days Processed : xx`, all divided by `|`, alongside active campaign and theater command indicators.
  2. **War Room Launch Pop-up Modal:** When pressing War Room (or Settings), replace hardcoded cursor overwrite (`setCursorPosition(4, 20)`) with a framed pop-up notification modal dialog box, completely eliminating text corruption or overwriting of menu footers.
  3. **Right-Aligned Topic Explanation Pop-Up:** In the System Wiki & Error Registry menu (and other screens where applicable), display the `TOPIC EXPLANATION` as a compact, framed pop-up window situated to the RIGHT of the selection menu (columns 43-78), using compact formatting/dimmed phosphor, avoiding vertical shifting of menu items and keeping the entire view within 80x25 bounds.

- 2026-09-20 15:05 UTC+3: **Baseline Scope Mandate & Conflict Clarification Gate (Ozgur):**
  1. **Mandatory Scope Addition:** Every new todo item, directive, and correction must be immediately incorporated into this business scope document (`docs/scope-v1.md`) with a date/time stamp. Business scope is the supreme requirements baseline.
  2. **Conflict Clarification Gate:** If there is any conflict between a new requirement/correction and existing requirements or architecture, the agent MUST NOT guess or assume; the agent must explicitly ask Ozgur to clarify.

- 2026-09-20 14:21 UTC+3: **Shorter, Sombre Navigation Click Acoustic Profile (Ozgur):**
  1. **Sombre Click Profile:** Change the key movement click (`UP/DOWN/TAB`) from the high-frequency tone (1800 Hz) to a shorter, sombre acoustic click (lower frequency band, brief 4-5 ms duration).
  2. **Tactile Feedback:** Provides restrained, physical retro terminal relay feedback during cursor movement without high-pitched shrillness.

- 2026-09-20 14:20 UTC+3: **Deletion of Background Ambient Audio (Ozgur):**
  1. **Background Sound Removed:** In accordance with user instruction (*"Delete the background sound"*), continuous looped ambient background sound (`PlaySoundA` background channel) is permanently removed from the application lifecycle (`main.cpp` and `Console::startBackgroundNoise()`).
  2. **Discrete Feedback Preserved:** Discrete acoustic feedback (relay navigation clicks and page transition chirps via `Beep(...)`) remains active, operating with a completely silent background.

- 2026-09-20 14:18 UTC+3: **Continuous Selection Underlining / Removal & Pop-up Explanations (Ozgur):**
  1. **Continuous Line Underline or Removal:** Active selected options must use a continuous horizontal line, not segmented ASCII `-` or `_` characters. If continuous line rendering cannot be achieved cleanly in the console, remove the underline completely and rely solely on inverted highlighting (Code 160).
  2. **Pop-up Window Explanations:** Explanations and descriptions for topics/options must be displayed in a dedicated pop-up modal window using slightly smaller/compact typography rather than inline indented text beneath the option.

- 2026-09-20 14:15 UTC+3: **Incremental Turn Ingestion & War Room Model Refresh on Campaign Load (Ozgur):**
  1. **Incremental Discovery on Load:** When continuing a current campaign (`[ CONTINUE CAMPAIGN ]`), the engine actively performs a directory diff scan across all stored report and SAVE paths for any new turn files (`.pws` saves and `.txt` operational reports) generated since the last session.
  2. **Automatic Ingestion & Container Commit:** Any newly detected files are automatically parsed and ingested into the campaign vault, and the updated container is immediately serialized back to disk (`C:\WitP-CIC\Campaigns\<Name>.wic`).
  3. **War Room Model Recalculation:** All internal War Room intelligence models (High-Value Targets, Strike Concentrations, Port Concentrations, Established Routes, and Tactical Intercepts) are updated and recalculated to reflect the latest turn telemetry upon loading.

- 2026-09-19 21:08 UTC+3: **Granular Error Code Sub-Topic Architecture in System Wiki (Ozgur):**
  1. **Decomposed Error Registry:** In accordance with the user instruction (*"each error along with the name is a sub topic."*), `5. SYSTEM ERROR CODE REGISTRY` is no longer a flat text screen. It is now a branch sub-topic under `TECHNICAL DETAILS` containing dedicated selectable sub-topics for every individual error code.
  2. **Error Code Sub-Topics Catalog:**
     - `[1] [101] VAULT_NOT_FOUND`
     - `[2] [102] VAULT_CORRUPT`
     - `[3] [201] REPORT_DIR_MISSING`
     - `[4] [202] PWS_READ_ERROR`
     - `[5] [301] PARSE_FORMAT_MISMATCH`
     - `[6] [401] MEMORY_ALLOCATION_FAIL`
  3. **Comprehensive Diagnostic Articles:** Selecting any error code displays a dedicated article screen detailing Subsystem, Severity, Description & Root Causes, Symptoms, and Actionable Recovery Procedures. Pressing `[ESC]` gracefully returns to the Error Code sub-menu.

- 2026-09-19 21:05 UTC+3: **Recursive Hierarchical Wiki Navigation & Technical Details Grouping (Ozgur):**
  1. **Hierarchical Topic Model:** Pressing a topic in the Wiki navigates to what is under that topic—either a scrollable content text page (leaf) or another set of sub-topics (branch) which the user traverses in the same manner.
  2. **`TECHNICAL DETAILS` Category:** Added a dedicated top-level topic titled `TECHNICAL DETAILS`. All five technical and architectural topics (System Architecture & Memory Sandbox, Combat Information Center, Data Ingestion & PBEM Lifecycle, Nostromo Terminal Controls & Hotkeys, and System Error Code Registry) are nested under this new topic as sub-topics.
  3. **Consistent Navigation UX:** Every level features double-spaced options, solid Nostromo underline cursors, acoustic clicks, and `[ESC]` navigating back up to the parent topic menu (or Master Menu at root).

- 2026-09-19 20:57 UTC+3: **Baseline & Core Docs Definitions, Lessons Learned Knowledge Base (Ozgur):**
  1. **Definition of Baseline:** The verified, authoritative requirements standard governed by the rule: *"Never mark anything done unless I say so"* (Ozgur). Code execution and automated tests are prerequisites; an item is only marked completed (`[x]`) upon explicit command from Ozgur.
  2. **Core Docs Classification:** Any and all documentation regarding Baseline Principles (`docs/baseline_witp_cic.md`), To-Do lists (`docs/todo.md`), Lessons Learned (`docs/lessons-learned.md`), Architecture & Design decisions (`docs/architecture-decisions.md`), Screen flow docs (`docs/screen-flow.md`, `docs/screen-flow-text.md`), Scope bible (`docs/scope-v1.md`), Project Context (`docs/project-context.md`), and UI specifications are classified as **Core Docs**. They must always be maintained, synchronized, date-stamped, and never bypassed.
  3. **Lessons Learned Protocol:** Created `docs/lessons-learned.md`. After every user correction, feedback, or design clarification, the agent must immediately append an entry documenting the context, what was learned, and what NOT to repeat (anti-patterns).

- 2026-09-19 20:39 UTC+3: **Field Manual Module Separation (Ozgur):** Field Manual is a separate, dedicated item from Wiki in the Master Menu (`[W] Wiki` and `[F] Field Manual`). Both modules are coded as standalone interactive screens:
  1. **Wiki Module (`WikiScreen`):** Hierarchical documentation browser covering CIC Architecture, War Room Dashboards, PBEM Ingestion Lifecycle, Nostromo Controls/Hotkeys, and a dedicated 3-digit Error Code Registry (`[101]`, `[102]`, `[201]`, `[202]`, `[301]`, `[401]`) with recovery resolutions.
  2. **Field Manual Module (`FieldManualScreen`):** Dedicated browser and keyword search/RAG reader over the complete official WITP:AE game manual (`docs/witpae-manual.txt`), organizing 24 chapters with in-terminal scrolling and query capability.

- 2026-09-19 20:35 UTC+3: **Audio & UI Cadence Refinements (Ozgur):**
  1. **Continuous Looped Ambient Background Noise:** Use the uploaded audio clip (`assets/audio/alien_ambient_transition.wav`) as persistent, looping background noise (`PlaySoundA` with `SND_ASYNC | SND_LOOP`) throughout the terminal session.
  2. **Calibrated Retro Terminal Blinking Cadence:** Slow down the prompt blink rate from rapid ~5 Hz strobing to authentic ~1 Hz teletype cadence (500ms inverted, 500ms light green) with responsive 25ms key polling.
  3. **Double-Spaced Menus:** Apply double-spacing between interactive menu options (Start Screen, Master Menu, Continue Campaign, and Wizard choices) for clean retro terminal legibility.

- 2026-09-19 20:25 UTC+3: **Baseline Completion Rule (Ozgur):** "never mark anything done unless I say so". The agent must NEVER mark any task, requirement, or TODO item as done (`[x]`) unless Ozgur explicitly instructs to do so. Code completion and automated tests alone do not permit checking off items.

- 2026-09-19 20:25 UTC+3: **Repository Restriction (Ozgur):** Permanently decommission and purge all Cursor git remotes and branches. Strictly and exclusively use the GitHub repository `https://github.com/oaltinyay70-cell/WITP-CIC2.git` (canonical `origin`, branch `main`). Never use or reference Cursor repos.

- 2026-09-19 19:44 UTC+3: **Wizard & Header Corrections List (Ozgur):**
  1. **Multi-directory accumulation:** In Setup Wizard Step 3/4, pressing `[B]` adds a 3rd, 4th, etc. extra directory. Adding new directories **does not** eliminate or overwrite the ones entered before; it accumulates them into a list of scanned folders.
  2. **Processed file list pop-up:** When folders are entered, the system inspects all directories, compiles a complete list of discovered `.pws` and text report files taken in and processed for intelligence data, and displays this manifest as a pop-up dialog.
  3. **Master Menu Header display:** The Master Menu header line must display:
     - `Days Processed: {COUNT}`
     - `Turn: {LAST_PROCESSED_DATE}` formatted as a proper calendar date (e.g. `Dec 08, 1941`), representing the latest day processed.
  4. **Continue Campaign Listing:** The `[ CONTINUE CAMPAIGN ]` screen must list all available `.wic` campaign files found in `C:\WitP-CIC\Campaigns` (and allow custom location selection/browsing if needed). Auto-create default directories if missing.
  5. **Console Attribute Bleed on ENTER from Splash:** Fix console buffer clearing where pressing ENTER to skip/advance from the Splash screen caused the inverted or green background attribute to fill/bleed across the entire right side of the Start screen. Always reset console text attributes to default dark green before and during screen clearing.
  6. **Physical `.wic` Vault File Serialization on Campaign Creation:** During the Wizard/Ingestion step, physically write and persist the `.wic` container file to `C:\WitP-CIC\Campaigns\<Name>.wic` containing campaign metadata, scanned directories, and parsed telemetry. Currently the skeleton completed the screen transitions but did not commit the actual `.wic` file to disk, so restarting the application resulted in zero campaigns listed in the Continue screen.
  7. **Splash Screen Prompt Positioning & Overlap Fix:** Position the `[ PRESS ANY KEY TO ENGAGE WU/TH/UR CONSOLE ]` prompt further down the page (lower vertical spacing with clear blank margin) and fix the duplicate overlapping line rendering during the blinking loop.
  8. **Alien MU/TH/UR Audio Cues & Underline Navigation Styling (Nice-to-Have):**
     - When traversing items with cursor keys (UP/DOWN/TAB), current selection is underlined like in the Alien movie terminal.
     - Each item selection change is accompanied by an authentic terminal acoustic click sound (e.g. Win32 `Beep(1200, 15)` or custom short acoustic frequency).
     - Each screen transition/page load comes with an authentic retro teletype terminal chirp/chime sound effect (e.g. short multi-tone sequence), inspired by the Nostromo terminal soundscape.
  9. **Authentic Nostromo MU/TH/UR Visual Typography & Dual-Column Matrix Layout (Nice-to-Have):**
     - Visual design modeled directly on the Nostromo OVERMONITORING ADDRESS MATRIX display.
     - Centered, wide letter-spaced uppercase header typography (`O V E R M O N I T O R I N G   A D D R E S S   M A T R I X`).
     - Dual-column tabular telemetry / menu layout with aligned parameter tags and telemetry values (`CRFX  OM2077AM`, `ATTITUDE  SM2078`, `WASTE HEAT  2080`, etc.).
     - Solid white/bright-green horizontal underline cursor indicator spanning directly beneath the active parameter key and its value.
     - Dark phosphor background with high-contrast monochrome green teletype text.
  10. **Alien Ambient Radio / Static Transition Noise:**
     - User provided authentic ambient radio static / Nostromo audio track (`assets/audio/alien_ambient_transition.wav`).
     - Play short random slices / bursts of this audio during screen transitions and data-load sequences (via native Win32 `PlaySound` API) to provide authentic retro ship atmospheric soundscapes.









- 2026-09-19 19:24 UTC+3: **Open-questions answers Batch 2 (Ozgur — docs/open-questions-batch2.md):**
  1. **A1 — Screen Header `{INDEX}`:** Removed. It was an artifact from older code. Headers cleanly display `[ {SCREEN TITLE} ]  Turn: {TURN_DATE}` (omitting turn date before campaign load). Everything must be documented vigorously.
  2. **A2 — Settings Module:** Version 1 Settings screen includes: (a) Allied SIGINT confidence toggle (`HIGH` vs `PROBABLE`), and (b) Strike Concentration detection thresholds (cruiser count, default 3; hex radius, default 10). Both settings must display clear in-screen descriptions explaining what they mean and how they affect intelligence analysis.
  3. **A3 — ESC key from Master Menu:** Pressing `ESC` from the Master Menu triggers the **Exit confirmation prompt** (identical to pressing `[E] Exit`).
  4. **B1 — Vault Encryption:** "Just compress it" — `.wic` campaign files use standard Zlib/Zip compression container (`miniz`), with no cryptographic password layer.
  5. **B2 — Vault Extension:** Canonical extension is **`.wic`** (`C:\WitP-CIC\Campaigns\<Name>.wic`).
  6. **C1 — SIGINT Map visual representation (Tab 2):** Mode (c) — **Textual / tabular list of localized hex coordinates with distance & sector bearings** (no 2D ASCII spatial map grid for v1; may change later).
  7. **D1 — WITP:AE standard file paths:** Auto-search inspects two default directories:
     - `C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE` (for `.pws` save states)
     - `C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE\archive` (for daily text reports)
     - A directory browse/input prompt allows the user to specify an alternate location if their game is installed elsewhere.
  8. **E1 — Toolchain & Alien MU/TH/UR UI style:** User wants an Alien movie style computer terminal interface (**MU/TH/UR** / Nostromo theme). Implementation is C++17 MinGW-w64 static binary (`WitP-CIC.exe`), green phosphor Win32 console attributes (2, 10, 160), uppercase terminal typography, box framing (`=`, `-`, `+`).

- 2026-09-19 18:33 UTC+3: **Open-questions answers (Ozgur — all 7, docs/open-questions.md):**
  1. **Q1 — `.pws` password:** The `.pws` file Dave sends Ozgur can only be opened **in the WITP:AE game** using that password. It is a game-UI PBEM credential (manual §20.5.4), not a file-level key on the golden set. Wizard stores it for the user's own reference only; WITP-CIC does **not** use it for gzip/zlib decompression. No password-gate code path in ingest. (Supersedes "validate the password" wording in §4.1.)
  2. **Q2 — Footer hotkeys:** §8.2 C footer template is a **placeholder**. Footers are composed **dynamically per screen**, showing only the hotkeys relevant to that screen. No hardcoded footer string anywhere.
  3. **Q3 — Turn length wizard step:** **Not wanted; remove it.** Ingest automatically covers **all dates and all files** found in the game report folder. No turn-length confirmation prompt.
  4. **Q4 — Shared combat file confidence:** `combatreport_*` and `Combat_Events_*` are treated as **"shared only to me"** — processed entirely from the Allied (Ozgur) perspective. Do not model the opponent view from shared files.
  5. **Q5 — Intercept Windows constraints (TPIE Tab 3):** Both modes required: (a) **free-text hex coordinate entry**, and (b) **pick from a list of discovered/known bases and hexes**. Input widget is a **modal popup menu**.
  6. **Q6 — Dossier Panel search (Tab 2):** (a) **Free-text partial name** entry; (b) results from the **vault list of discovered/known entities**; (c) **all five types** (base, vessel, unit, air group, city) in one unified search.
  7. **Q7 — TPIE data threshold:** Show **degraded/partial predictions from turn 1** with a confidence caveat. No hard gate before displaying the tab.

- 2026-09-19 ~10:40 UTC: **PBEM golden set + forensics (Ozgur):** (1) **Dave** = Japanese PBEM player; Ozgur = Allied. (2) Six `wpae021` saves = Japanese → Allied; **`wpae023.pws` = Ozgur outgoing US → Japanese** (supersedes 2026-09-15 provenance “JP → Allied” for `wpae023` only). (3) T1 exchange = **12/08/41** (no play); label typo **12/07**; played days **12/09–12/13**. (4) On-disk `.pws` = gzip + 61 zlib records by type byte; **no file-level password** on golden set; `kilroy43` not in bytes. (5) Proven ship fields (464-byte rows): name @0, sunk `19999` @36, loss hex @352/354, TF links @60/64/66. Product: do not block ingest on zip/password; War Room may use vault text first; partial `.pws` read allowed for container + evidence-gated fields only. Docs: `docs/pws-knowledge-summary.md`, `docs/pws-field-map.md`, `docs/pws-wpae021-catalog.md`.
- 2026-09-15 ~16:19 UTC: **Combat report filing (Ozgur):** (1) Indented names under `Assaulting units:` / `Defending units:` → **ground unit list items**; ship name-only rows (e.g. `BB Haruna`) → **ship names**, separate kinds. (2) Sweeping aircraft lines → **standalone** type/count/altitude only; **no** raid/air-group/unit parent attribution (fog of war). (3) Ground fight → **one parent `ground_combat` block** with ordered child lines. (4) `Japanese forces CAPTURE …` → **same capture fact** as operations reports with **source** (`operations` | `combat_report` | `combat_events`) for dedup. Recorded in `docs/combat-parser-decisions.md` § “Ozgur answers (2026-09-15 ~16:19 UTC)”.
- 2026-09-15 ~16:13 UTC: **Combat report parser (Ozgur):** Approved classifications recorded in `docs/combat-parser-decisions.md` (user answers inline). Implementable without further decision: Categories 4–11, 13–17; Cat.1 same zero-loss kind (Allied/Japanese); Cat.2 ship name-only rows distinct from comma `ship_line`; Cat.7 two amphibious phase header kinds; Cat.10 single fire-line kind; Cat.13 range ellipsis same as non-ellipsis; Cat.12 assault-odds **two** kinds (with/without fort level). **Open in that doc (no answer / not understood):** Cat.2 ground unit list vs ship names; Cat.3 aircraft sweeping block context; Cat.12 flat lines vs ground-combat block; Cat.12 reuse operational `capture` kind.
- 2026-09-15 ~16:05 UTC: **Golden `.pws` forensic pass (`wpae023.pws`):** On-disk file is **gzip** (2 677 122 B → 2 824 440 B inner). Inner layout **observed** (this sample only): repeating **12-byte LE triple** + **zlib** (61 records; 53/61 size fields match). Cleartext without password includes PBEM save label `(PBEM1) 12/8 incoming to Ozgur Date: 12/10/41`, host timestamp `Mon, September 14 2026…`, build **`1.8.11.28.4`**, scenario *War in the Pacific: Hakko Ichiu* / *December 1941 Start*, plus mixed game DB strings and one embedded after-action snippet (`Dec 09, 41`). User password **`kilroy43` not validated** (not in file; manual §20.5.4 in-game load only). Full telemetry / password gate **not** demonstrated. Report: `docs/pws-inspection.md`.
- 2026-09-15 ~15:47 UTC: **TUI on-screen name (Ozgur):** Authoritative **on-screen display string** is **WU/TH/UR** (slashes as shown). **Decided (closes OPEN from ~15:44 UTC):** implement splash, headers, and titles using **WU/TH/UR** — not “ROCHEFORT” and not “W.U.T.H.U.R.” unless those strings appear elsewhere in spec text for traceability. Spec §8 historical heading **“W.U.T.H.U.R.”** remains unchanged here (no rewrite without a superseded note). **§8 layout:** header/body/footer blocks (§8.2–§8.4) stay as written in this document until a scope change edits them. **Reference images:** none provided. Do not invent splash taglines. Do not check off §8 TUI TODOs from this entry alone.
- 2026-09-15 ~15:44 UTC: **v1 UI aesthetic (Ozgur):** “the UI must be a perfect copy of the ROCHEFORT from the ALien movie”. **Aesthetic target:** MU/TH/UR / Alien ship-computer **green phosphor**, as already described in spec §8 (Win32 console codes **2**, **10**, **160**; header/body/footer topography per §8.2–§8.4). **Authoritative layout:** spec §8 remains the implementable standard until Ozgur answers follow-ups. **OPEN:** spec §8 heading **“W.U.T.H.U.R.”** vs user **“ROCHEFORT”** — unresolved; do not pick either without Ozgur. *(Closed — Changelog 2026-09-15 ~15:47 UTC: on-screen **WU/TH/UR**.)* **Implementation constraints:** do not treat film frames as assets; do not use copyrighted film screens as an asset or layout source (see `docs/architecture-decisions.md` §5). Do not check off §8 TUI TODOs from this entry alone.
- 2026-09-15 15:43 UTC: **PBEM save extension (Ozgur):** Product canonical extension for WITP-CIC file matching is **`.pws`** (spec §5; golden `wpae023.pws`). Ozgur (~15:43 UTC): “pws... my bad sorry” (reply to `.pws` vs manual `.wps`). Manual §20.4.1 / §20.5.4 still cite **`.wps`** as the manual’s string — **superseded for WITP-CIC file matching only** (manual text retained for traceability).
- 2026-09-15 15:41 UTC: **Golden `.pws` (Ozgur):** `docs/golden/wpae023.pws` received (2677122 bytes on disk; `file`(1) reports gzip from NTFS; leading bytes `1f 8b 08`). User-stated PBEM provenance: Japanese player → Allied player (Ozgur). User-stated password for this golden file only: `kilroy43` (not programmatically validated in catalog pass). Manual §20.4.1: saved games are compressed and encrypted; PBEM load uses password in-game — inner unlock/telemetry not demonstrated here. Catalog: `docs/golden/INDEX.md`. No second Japanese-side `.pws` golden file in this drop.
- 2026-09-15 15:40 UTC: **Golden samples (Ozgur):** Japanese `jsigint_*` / `joperationsreport_*` at tiny size are **real fog of war** (header-only SIGINT; operational report header plus Batan Island)—the Japanese player gets nothing useful those turns; **not** incomplete or truncated samples. `Combat_Events_*` and `combatreport_*` are **shared by filename** between players; what is written may **change (delta)** because of fog of war—treat as shared files and do **not** assume identical content for both sides. This drop contains **one instance per filename/date**; do **not** invent a second side’s copy. `.pws` files **are coming later**; spec §5 still requires them.
- 2026-09-15 15:37 UTC: **Platform (Ozgur):** Version 1 ships **Windows first**. Default install remains `C:\WitP-CIC`. §1 sentence “Version 1 ships for Mac first…” is **superseded** for v1 (original text retained for traceability).
- 2026-09-15 15:37 UTC: **Campaign storage path (Ozgur):** Campaign vault files live **only** under `\Campaigns` (`C:\WitP-CIC\Campaigns`). §4.1 phrase “\Campaign Vaults” is **superseded**; treat “Campaign Vaults” as a synonym error—canonical folder name is `\Campaigns`.
- 2026-09-15 15:37 UTC: **Primary language (Ozgur: “U decide. I need an exe file”):** **C++**, compiled to a **Windows console `.exe`**. TUI uses native Win32 console attributes matching §8.1 codes 2, 10, 160. Not Python, JavaScript, or .NET as the primary implementation language. FAQ “C or C++ or JS” and “Agreed” to C++/Python/.NET mix are **superseded** for v1 implementation path (original FAQ answers retained).
- 2026-09-15 15:37 UTC: **Storage architecture (Ozgur: “U decide”):** **One compressed Campaign File** per campaign in `\Campaigns`. No Neo4j, InfluxDB, or PostGIS **services or on-disk data directories**. Graph, time-series, and hex-spatial models required by §7 are **in-process** and must **serialize into** that single Campaign File. Rationale: §3 forbids external/orphaned files and extra processes. §7.3 references to PostGIS, Neo4j, and InfluxDB as deployment/storage are **superseded** (original §7.3 text retained).
- 2026-09-15 15:36 UTC: Twelve golden WITP:AE game output text samples received; stored at `docs/golden/`. User mapping: `a*` prefix = Allied, `j*` prefix = Japanese (unprefixed filenames not classified by user).
- 2026-09-15: Version 1 scope received from Ozgur (full BA/requirements text + FAQ). Adopted as the design spec bible.
- 2026-09-15: Game manual `WITPAE Manual Last Official Update` added. It is ingested through a local RAG index, is a source for the in-app Wiki, and must be consulted when making product decisions.

---

## 1. Executive Summary

This document describes a small desktop application for players of the computer wargame *War in the Pacific: Admiral's Edition* (WITP:AE), specifically for those playing by email (PBEM).  The game already produces several plain-text reports at the end of every turn. The application reads those reports, analyses them over time, and presents the player with a "Combat Information Center" (CIC) style dashboard that surfaces intelligence a human reader would likely miss: enemy fleet concentrations, invasion warnings, merchant shipping routes, recurring enemy habits, and corrections to earlier battle claims.

The product is strictly read-only. It never touches the game itself, never makes decisions for the player, and never communicates with anyone. All data stays on the player's own computer. Version 1 ships for Mac first; the same codebase will produce the Windows version. *(Superseded for v1 — Changelog 2026-09-15 15:37 UTC: Version 1 ships **Windows first**; default install `C:\WitP-CIC`.)*

## 2. Background and Problem Statement

WITP:AE is one of the most detailed Second World War Pacific simulations ever made. In PBEM play, two people (Allies and Japan) take turns by exchanging files, often over weeks or months.

Each turn, the game writes thousands of lines of reports per side: signals intelligence (radio intercepts), operational reports, after-action combat reports, and a turn event log. Players must read all of this manually and remember it from turn to turn. In practice this means:

Important signals get buried in noise (a single turn can contain hundreds of report lines).

Tracking an enemy task force across several turns is done by hand, if at all.

Subtle contradictions between reports — for example a ship reported sunk in one place reappearing later — are easily missed.

Each side only sees its own reports, and under the game's "fog of war" some of what a report says may be wrong in reality (a "sunk" ship may only be damaged; a ship may be misidentified).

There is no existing tool that performs this ongoing intelligence analysis for the player. The result is fatigue, mistakes, and a weaker experience than the game deserves.

## 3. Development Architecture & Constraints Baseline

Zero Data Leakage: The in-memory knowledge base MUST be completely purged when starting the app, switching between campaigns, or exiting.

Absolute Sandbox: Campaign data belongs to one side only. Data from one campaign cannot influence another.

Storage Rule: All campaign data (parsed data, text, results) must be compressed/zipped into a single custom Campaign File stored ONLY in the \Campaigns directory. No external or orphaned files permitted.

Requirements Traceability Notice: Any addition to the business requirements will be added to this document. Any request made later by the user to add anything new to the witp_cic WILL be documented here along with the time / date stamp. THIS IS CRITICAL.

Deliverables:

Comprehensive step-by-step To-Do List.

Check off items ONLY after writing and validating code.

App Name / Installation Paths:

Default Install: C:\WitP-CIC

Campaign Vault Files are placed in the directory: C:\WitP-CIC\Campaigns

These are offered to the user when browsing target locations for vault files.

## 4. Operational Application Flow

Note: Pressing ESC in any screen immediately moves the view one screen back at a time.

### 4.1 Initial Start Screen

When opened, the application shows a short splash screen, then ask if user wants to continue a current campaign or start a new one.

If the user selects New Campaign, a setup wizard opens in a new window:

The wizard asks which side the player commands and prompts for the password for the .pws file. It finds the folder where the game writes its reports (searching automatically and guiding the user with clear messages if anything is missing), confirms the game's turn length, and validates the password. *(Superseded — Changelog 2026-09-19 18:33 & 19:24 UTC+3: (a) "validates the password" — password is a game-UI PBEM credential only; WITP-CIC stores it for user reference and does NOT use it for file decompression; no password-gate code path in ingest. (b) "confirms the game's turn length" step — REMOVED; not wanted; ingest covers all dates and all files automatically. (c) Automatic report search checks default paths: `C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE` for `.pws` saves and `...\SAVE\archive` for report text files; user may also browse/specify an alternate custom folder.)*

If validated, the system prompts the user for a campaign vault file name. This file stores all information from the current campaign, including the location of the actual game campaign files (which will not be requested again unless missing). This file is stored in the installation directory under \Campaign Vaults. *(Superseded — Changelog 2026-09-15 15:37 UTC: store under `\Campaigns` only, e.g. `C:\WitP-CIC\Campaigns`; “\Campaign Vaults” is a synonym error.)*

All text output files and detected .pws files are inspected; every piece of gleaned information and newly produced intelligence is placed into this vault file going forward.

All collected and generated data—without exception—is maintained within this file.

Once complete, the user is transitioned to the Main Menu Screen.

If the user selects Continue a Current Campaign:

The user is directed to a screen to choose from available campaign master files.

The user selects and loads a campaign file.

WITP-CIC loads the file, verifies the stored file paths, and updates the campaign vault with newly added game output files and .pws files using the saved credentials. *(Updated — Changelog 2026-09-20 14:15 UTC+3: Upon loading an existing campaign, WITP-CIC performs an active diff-scan across all configured directories for new `.pws` and `.txt` operational reports. Newly discovered files are automatically ingested into the manifest, turn counts and calendar dates updated, the `.wic` container persisted to disk, and all War Room models recalculated.)*

### 4.2 Master Menu Screen

The Master Menu presents the following operational modules:

[A] War Room

[W] Wiki (WITP-CIC System Documentation & Error Code Registry)

[F] Field Manual (WITP:AE Official Game Manual Browser & RAG Query) *(Added — Changelog 2026-09-19 20:39 UTC+3: separated as a distinct standalone module).*

[S] Settings *(Added — Changelog 2026-09-19 19:24 UTC+3: v1 includes Allied SIGINT confidence toggle [HIGH/PROBABLE] and Strike Concentration thresholds [cruiser count, hex radius], each with clear in-screen explanatory descriptions).*

[E] Exit

Navigation can be completed by pressing the corresponding hotkey letter or by traversing items with the arrow keys. *(Added — Changelog 2026-09-19 19:24 UTC+3: pressing ESC from the Master Menu triggers the Exit confirmation prompt [E]).*

*(Updated — Changelog 2026-09-20 15:07 UTC+3: Top header block consolidates campaign metadata and telemetry instrumentation: `[ MASTER MENU ]` with `Active Campaign: <Name> (<Side> Command)` right-aligned, and line 2 displaying `Files Processed : XX pws files | xx TXT files | Days Processed : xx`, partitioned by `|` delimiters. All telemetry is transferred out of the menu selection body to keep command modules spacious. Furthermore, engaging `[A] War Room` or `[S] Settings` presents status notices inside a centered framed modal pop-up box rather than overwriting screen text.)*

### 4.3 Wiki Module

An integrated Wiki and Help system providing comprehensive user documentation:

Structure: Hierarchical design organized by primary section headers. *(Updated — Changelog 2026-09-19 21:05 & 21:08 UTC+3: Hierarchical topic navigation featuring top-level categories `OPERATIONAL DOCTRINE`, `TECHNICAL DETAILS`, and `BASELINE PRINCIPLES`. `TECHNICAL DETAILS` nests 5 sub-topics including `SYSTEM ERROR CODE REGISTRY`, which in turn decomposes into dedicated selectable sub-topics for every standard 3-digit error `[101]`, `[102]`, `[201]`, `[202]`, `[301]`, `[401]` with full diagnostic articles and recovery procedures.)*

Interaction: Selecting a main section header routes the user to a dedicated sub-page for that topic. Selecting a leaf article opens the full document view; pressing ESC navigates back up one level in the hierarchy.

Right-Aligned Pop-up Topic Explanations: *(Added — Changelog 2026-09-20 14:18 & 15:07 UTC+3: Topic and option descriptions are rendered in a compact, framed pop-up box situated to the RIGHT of the selection menu [columns 43–78] with compact word-wrapping and dimmed phosphor typography, ensuring zero vertical menu jitter and keeping all content within standard 80x25 bounds.)*

Maintenance: Must be continuously updated as new features, functions, and error codes are added.

Game manual source (added 2026-09-15): The official WITP:AE game manual is a Wiki source. It is stored as `docs/witpae-manual.txt`, indexed with local RAG, and consulted when making product decisions.

### 4.4 Exit Module

Executes a clean shutdown of WITP-CIC, purging all in-memory data structures prior to closing.

### 4.5 War Room Dashboard

The primary interface is a dark, high-contrast visual dashboard modeled after a classic naval Combat Information Center. Pressing ENTER over any report item launches a modal detail overlay displaying detailed intelligence context.

Tab 1 — Intelligence Summary

High-Value Targets: Tracks detected enemy carriers, capital ships, tankers/oilers, and troop transports.

Strike Concentrations: Flags groups of three or more warships (cruiser-class or larger) detected within ten hexes of one another (configurable threshold).

Port Concentrations: Tracks warships and merchant vessels docked at named ports.

Established Routes: Identifies merchant and tanker transit corridors built from repeated sightings, inferring departure and destination points.

Tab 2 — SIGINT Map

Displays the strategic planning map overlaid with layered SIGINT data (radio intercepts, visual sightings, concentration radii, and shipping routes). Players can step backward and forward through historical turn data. *(Clarified — Changelog 2026-09-19 19:24 UTC+3: in v1, rendered as a structured textual / tabular list of localized hex coordinates with distance & sector bearings relative to friendly bases/HQs; 2D ASCII spatial map deferred to later version).*

The interface includes permanent auxiliary panels:

Alert Feed (Left): Prioritized real-time intelligence warnings.

Dossier Panel (Right): Target search tool providing compiled intel reports (strength estimates, history) for any base, vessel, unit, air group, or city. *(Clarified — Changelog 2026-09-19 18:33 UTC+3: user types free-text partial name; results drawn from vault's list of discovered/known entities; all five entity types (base, vessel, unit, air group, city) searchable in one unified interface — no separate tabs per type.)*

Event Log (Bottom): Chronological stream of daily operational events.

Tab 3 — Theater Predictive Intelligence Engine (TPIE)

A high-priority warning dashboard surfacing invasion alerts, fleet departures, and predicted enemy logistical behaviors over the campaign lifetime.

Strategic Foresight: Shifts user decision-making from reactive response to proactive theater management by predicting amphibious targets and air strike focal points 3–5 days in advance.

Actionable SIGINT: Converts raw, noisy intelligence reports (radio volume metrics, transport manifests) into high-confidence invasion probability heatmaps.

## 5. Ingested Data Sources

WITP-CIC parses five distinct report and file structures produced during game operation:

Signals Intelligence (SIGINT): Plain-text files generated per side per turn (e.g., asigint_411207.txt).

Operational Reports: Plain-text operational summaries generated per side per turn.

Combat Reports: Shared plain-text battle output accessible to both players.

Combat Event Logs: Shared file naming structure containing player-side specific event entries.

Game Save State (.pws): Unlocked via user password; contains complete underlying turn telemetry. *(Superseded — Changelog 2026-09-19 18:33 UTC+3: "Unlocked via user password" refers to the WITP:AE game UI PBEM load dialog only (manual §20.5.4). WITP-CIC decompresses .pws with gzip/zlib only — no password gate. Password is stored in vault for user reference.)*

All parsed telemetry and analytical models are stored securely within the encrypted Campaign Vault File.

## 6. Intelligence Modeling & Fog of War Logic

To maintain historical authenticity and game integrity, WITP-CIC processes data through a dynamic "Belief Model" rather than treating raw text reports as objective truth:

Allied SIGINT: Treated as high-confidence/certain by default (reflecting historical code-breaking capabilities; user-configurable).

Japanese SIGINT: Treated as probable intelligence requiring secondary verification.

Own-Force Operational Reports: Treated as absolute fact for friendly units.

Shared Combat Reports: Treated with healthy skepticism. Battle Damage Assessments (BDA) are stored as claims, dynamically adjusting confidence scores as subsequent turns corroborate or contradict the report. *(Clarified — Changelog 2026-09-19 18:33 UTC+3 & 2026-09-21 18:15 UTC+3: shared files `combatreport_*` / `Combat_Events_*` are treated as "shared only to me" — processed entirely from the player's perspective. Crucially, the Intelligence War Room BDA and Claims registers must track **ONLY THE OPPONENT'S ASSETS** [Japanese in Allied campaigns; Allied in Japanese campaigns]. Friendly losses and damage must never appear in enemy intelligence tracking menus, as the commander already knows friendly status from operational reports).*

Because each client instance only parses its user's native files, opponent intelligence states correctly diverge. For example, while both sides may see a claim that destroyer Maratha was sunk, only the Allied player receives internal confirmation, while only the Japanese player logs the unflagged sinking of two damaged transports post-engagement.

### 6.1 Numerical Confidence Level Derivation Engine (Algorithmic Specification)

> **"confidence levels must be a percentage and derived numerically from the pws and txt files . DOnt bullshit , ask me how I do this , say I dont know this !" — Ozgur (2026-09-20 22:08 UTC+3)**

To eliminate subjective placeholder strings and ungrounded estimates, intelligence confidence levels are computed strictly as numerical percentages ($0\% - 100\%$) and standardized tiers through a deterministic, multi-pass algorithmic engine tailored for C++ high-performance execution.

#### 1. Conceptual Data Structures
Before parsing, define the core memory structures:
- **`RawReport` Struct:** Holds parsed string tokens extracted from report lines:
  - `entityName` (std::string)
  - `location` (std::string)
  - `hexX`, `hexY` (int)
  - `hexKey` (int, computed as `hexX * 1000 + hexY` for integer hashing)
  - `reportType` (enum: `RADIO_TRAFFIC`, `UNIT_LOCATED`, `UNIT_MOVING`, `GARRISON_SIZE`, `SHIP_MOVING`, `VISUAL_RECON`, `UNKNOWN`)
  - `domain` (enum: `AIR`, `NAVAL`, `LAND`, `UNKNOWN`)
  - `reportDate` (std::string, YYMMDD)
- **`ConfidenceReport` Struct:** Holds evaluated numerical metrics and final tier:
  - `rawReport` (`RawReport`)
  - `baseScore` (int: 30, 50, 65, 75, 80)
  - `corroborationBonus` (int: 0, 10, 15, 25)
  - `timeDecay` (double: 1.0, 0.8, 0.5)
  - `finalScore` (int: clamped strictly between 0 and 100)
  - `tier` (enum: `UNCONFIRMED`, `PROBABLE`, `CONFIRMED`, `VERIFIED`)
- **Fast Lookup Dictionaries / Hash Maps:**
  - `EntityMap`: Maps `entityName` (std::string) to observation count (`int`) within the current turn file.
  - `HexMap`: Maps integer `hexKey` (`int = X * 1000 + Y`) to observation count (`int`) within the current turn file. Using integer keys avoids heap string allocation and maximizes hashing throughput.

#### 2. Phase 1: Parsing and Classification (First Pass)
- **Objective:** Ingest the report file line-by-line, extract tokens, classify report types and domains, assign base scores, and build lookup frequency maps.
- **Initialization:** Open report file. Read first line to extract `reportDate`. Clear `EntityMap` and `HexMap`.
- **Fast Substring Pattern Matching (No Regex):** Standard `std::regex` is strictly avoided due to overhead. Pattern evaluation utilizes high-speed `std::string::find` and `std::string::substr` (or C `strstr`):
  1. `"Radio transmissions"` $\to$ `Type = RADIO_TRAFFIC`. Extract Location/Hex. **Base Score = 30**.
  2. `"is located at"` $\to$ `Type = UNIT_LOCATED`. Extract Entity Name (substring before `"is located at"`), Location, Hex. **Base Score = 75**.
  3. `"is loaded on"` $\to$ `Type = UNIT_MOVING`. Extract Entity Name and Destination. **Base Score = 65**.
  4. `"men are based at"` $\to$ `Type = GARRISON_SIZE`. Extract Personnel Headcount, Location, Hex. **Base Score = 50**.
  5. `"is moving to"` (and not matched above) $\to$ `Type = SHIP_MOVING`. Extract Ship Name, Destination, Hex. **Base Score = 65**.
  6. Direct operational sightings (e.g. `"Japanese ships at"`, `"Sighting of"`) $\to$ `Type = VISUAL_RECON`. Extract Ship Count / Type, Location, Hex. **Base Score = 80**.
- **Domain Inference:**
  - Contains `"JAAF"`, `"AF Coy"`, `"Fighter"`, `"Bomber"` $\to$ `Domain = AIR`.
  - Contains `"AMc"`, `"Maru"`, `"Destroyer"`, `"Carrier"` $\to$ `Domain = NAVAL`.
  - Contains `"Infantry"`, `"Cavalry"`, `"Army"`, `"Regiment"` $\to$ `Domain = LAND`.
- **Store & Map Aggregation:**
  - Push parsed record to temporary `std::vector<RawReport>` buffer.
  - If `entityName` is non-empty and `reportType != VISUAL_RECON`, insert into `EntityMap` or increment count: `EntityMap[entityName]++`. (Visual recon lacks specific named identities and does not populate `EntityMap`).
  - If `hexKey > 0`, insert into `HexMap` or increment count: `HexMap[hexKey]++`. Both SIGINT and Visual Recon populate `HexMap` for spatial cross-corroboration.

#### 3. Phase 2: Corroboration Calculation (Second Pass)
- **Objective:** Cross-reference extracted reports against per-turn frequency maps to reward corroborated intelligence.
- **Corroboration Scope (Per-Turn Parser vs Game State Manager):**
  - **Parser (Per-Turn Only — Option B):** The text parser stays "dumb and fast" and only evaluates corroboration within the current turn file. If an entity is mentioned $\ge 2$ times in this turn's file, it receives $+15$.
  - **Game State Manager (Cross-Turn Persistence):** Cross-turn tracking is managed in permanent game state memory. When the parser hands new contacts to the game state, if an entity from Turn 1 is observed again in Turn 2, the game state updates its timestamp, resets Time Decay to $1.0$, and keeps confidence elevated.
- **Iteration & Bonuses:**
  - **Entity Check:** Query `EntityMap[report.entityName]`. If count $> 1$, add **+15** to `corroborationBonus`.
  - **Hex Check:** If report has coordinates and `HexMap[report.hexKey] > 1`, add **+10** to `corroborationBonus`. (e.g., if SIGINT reports radio traffic at (80,77) and Visual Recon spots ships at (80,77), both receive the $+10$ Hex Corroboration Bonus).

#### 4. Phase 3: Time Decay and Final Math
- **Objective:** Penalize stale intelligence over elapsed PBEM turns and compute final clamped 0–100 percentage.
- **Time Delta Calculation:** Compare `reportDate` against current campaign turn date:
  $$\Delta t = \text{CurrentTurnDate} - \text{ReportDate} \quad \text{(in turns)}$$
- **Time Decay Multiplier Assignment:**
  - $\Delta t = 0 \implies \text{Time Decay} = 1.0$ (Fresh / Current Turn)
  - $\Delta t = 1 \implies \text{Time Decay} = 0.8$ (Previous Turn)
  - $\Delta t > 1 \implies \text{Time Decay} = 0.5$ (Historical / Stale Intelligence)
- **Final Numerical Score Calculation:**
  $$\text{Raw Score} = \text{Base Score} + \text{Corroboration Bonus}$$
  $$\text{Final Score} = \text{clamp}\Big(\text{round}(\text{Raw Score} \times \text{Time Decay}), 0, 100\Big)$$
- **Confidence Tier Stratification:**
  - $0\% - 39\% \implies \textbf{UNCONFIRMED}$
  - $40\% - 69\% \implies \textbf{PROBABLE}$
  - $70\% - 89\% \implies \textbf{CONFIRMED}$
  - $90\% - 100\% \implies \textbf{VERIFIED}$ (triggers slow 600ms in-place blink on Confidence Meter Bar)

#### 5. Phase 4: Engine Integration, Performance & UI Presentation
- **Permanent State Allocation:** Pre-allocate or copy final `ConfidenceReport` records into the campaign's permanent turn-state array.
- **Memory Cleanup & Sandbox Integrity:** Clear and shrink temporary parsing buffers, `EntityMap`, and `HexMap` immediately upon phase completion to eliminate memory leakage.
- **Integer Coordinate Hashing:** Coordinates are hashed and stored as single 32-bit integers (`hexX * 1000 + hexY`), avoiding string allocation and providing $O(1)$ fast lookups. Formatted coordinates (e.g. `"106,44"`) are generated exclusively at render time.
- **Pop-Up & Modal UI Presentation Standards:**
  - **Clean Header:** `CONFIDENCE: 90% [VERIFIED]` (immediate tactical takeaway in the top bar / meter).
  - **Reasoning Body (Full Mathematical Transparency):** Below the header, display the calculation components translated into plain English:
    ```
    CONFIDENCE LEVEL: [==========] 90% [VERIFIED]
    Base Score: 75 (Unit Located)
    Corroboration: +15 (Confirmed by 2 SIGINT intercepts this turn)
    Time Decay: x1.0 (Reported this turn)
    Final Calculation: (75 + 15) * 1.0 = 90%
    Analyst Note: Intercepted traffic confirms active unit presence.
    ```

### 6.2 Naval Vessel Classifications and Operational Roles

An essential prerequisite for interpreting theater intelligence is understanding what each naval unit type is equipped and doctrinally designated to do. All predictive intelligence models, tactical reasoning disclosures, and threat evaluations are grounded in the following seven functional categories and standard hull classifications:

#### 1. Major Surface Combatants & Carriers
These form the backbone of Surface Combat, Air Combat, and Bombardment Task Forces:
- **CV (Fleet Aircraft Carrier):** High-capacity flight deck; offensive center of Carrier Strike TFs; primary target for enemy air-sea strikes.
- **CVL (Light Aircraft Carrier):** Converted cruiser or purpose-built light carrier; moderate air capacity for fleet air defense and strike augmentation.
- **CVE (Escort Aircraft Carrier):** Converted merchant or auxiliary hull; slower speed; specialized in ASW hunter-killer operations, convoy escort, and ferrying replacement aircraft.
- **BB (Battleship):** Heavy armor and massive main battery guns (14-inch to 18.1-inch); supreme surface combat and shore bombardment firepower.
- **BC (Battlecruiser):** High speed paired with capital-caliber armament at the cost of reduced armor protection; excels in fast interception and bombardment.
- **CA (Heavy Cruiser):** Armed with 8-inch (203mm) guns; surface combat vanguard and carrier strike escort.
- **CL (Light Cruiser):** Armed with 6-inch (152mm/155mm) guns; high rate of fire; versatile lead scout and destroyer screen leader.
- **CLAA (Anti-Aircraft Light Cruiser):** Dedicated dual-purpose anti-aircraft gun platforms; crucial for fleet air defense umbrellas.
- **CS (Scout Cruiser / Seaplane Carrier):** Hybrid cruiser/seaplane platform; provides extended aerial scouting and reconnaissance.

#### 2. Surface Escorts & Light Combatants
Primarily utilized in ASW (Anti-Submarine Warfare), convoy escort, and local defense roles:
- **DD (Destroyer):** High-speed multi-mission escort equipped with torpedoes, depth charges, and dual-purpose guns; vital for screening capital ships, hunting submarines, and surface night fighting.
- **DE (Destroyer Escort):** Purpose-built convoy escort; lower speed and cost than fleet destroyers, optimized for ASW and anti-aircraft escort.
- **Frigate / PF (Patrol Frigate):** Oceanic escort and patrol craft bridging corvette and destroyer capabilities.
- **KV (Corvette):** Specialized ASW escort vessels designed for coastal and convoy protection.
- **PC (Submarine Chaser - Steel Hull):** Larger coastal escort craft equipped with sonar and depth charges for harbor and convoy defense.
- **PCE (Patrol Craft Escort):** Coastal and ocean patrol craft escort.
- **PG (Gunboat / Patrol Gunboat):** Shallow-draft or coastal armed patrol vessel for riverine, archipelago, and local harbor defense.
- **PB (Patrol Boat):** Light patrol craft for local surveillance and picket duties.
- **SC (Submarine Chaser - Wood Hull):** Smaller, wooden-hulled craft utilized for shallow coastal ASW sweeps.
- **PT (Motor Torpedo Boat):** Fast, shallow-water coastal attack craft armed with torpedoes; lethal in restricted straits and night littoral ambushes.

#### 3. Mine Warfare Vessels
Specialized for laying or clearing defensive and offensive minefields:
- **DM (Destroyer Minelayer):** High-speed converted destroyers capable of laying offensive minefields in forward enemy waters.
- **DMS (High-Speed Minesweeper):** Converted destroyers with minesweeping gear; capable of keeping pace with fleet units to clear assault channels.
- **CM (Minelayer):** Dedicated large minelayers carrying extensive mine payloads for major barrier construction.
- **CMc (Coastal Minelayer):** Smaller vessels for harbor defense and coastal minelaying.
- **AM (Minesweeper):** Fleet/ocean-going minesweepers for clearing magnetic, acoustic, and moored contact mines.
- **AMc (Coastal Minesweeper):** Local harbor sweepers restricted to short-range (1-hex radius) defensive clearance; cannot join standard offensive Minesweeping TFs.
- **ACM (Auxiliary Mine Tender / Harbor Defense Vessel):** Disbanded at a base with fuel and <10% damage, an ACM preserves up to 150 mines from decay.

#### 4. Amphibious & Troop Transports
Dedicated to carrying troops, combat equipment, and landing assault forces:
- **AP (Troop Transport):** Military-specification personnel vessel carrying garrison and infantry formations.
- **APA (Attack Transport):** Purpose-built amphibious assault ship carrying integral landing craft (LCA/LCVP) allowing combat-loaded troops to assault beaches battle-ready with accelerated unloading rates.
- **APc (Coastal Transport):** Small coastal transport for inter-island personnel transfer.
- **APD (High-Speed Transport):** Converted destroyer or escort hull used for rapid troop insertion, commando raids, and extraction; fast transit (25-30+ knots) with ASW protection, but no amphibious unloading bonus.
- **LSD (Landing Ship, Dock):** Carries pre-loaded landing craft and vehicles within an integral well deck.
- **LST (Landing Ship, Tank):** Ocean-going beachable vessel capable of discharging tanks, heavy vehicles, and artillery directly onto shorelines.
- **LCI (Landing Craft, Infantry):** Shallow-draft vessel designed to deliver infantry platoons directly onto beaches.
- **LCM (Landing Craft, Mechanized):** Carries vehicles and heavy cargo from offshore transports to the shore.
- **LCVP (Landing Craft, Vehicle, Personnel):** Standard landing craft (Higgins boat) for infantry and light vehicles.
- **xAP (Converted Commercial Passenger / Troop Transport):** Requisitioned commercial liner or passenger ship; large troop capacity but low survivability and vulnerable to air/sub attack.

#### 5. Cargo & Logistics Freighters
The lifeline of overseas logistical projection, hauling bulk supplies, vehicles, fuel, and resources:
- **AK (Cargo Ship):** Military-specification cargo freighter with cargo-handling derricks and armament.
- **AKA (Attack Cargo Ship):** Combat-loaded amphibious freighter carrying assault equipment and vehicles, designed for combat beach unloading alongside APAs.
- **AKL (Small Cargo Ship / Coastal Freighter):** Light cargo vessel for shallow-port and inter-island supply distribution.
- **AKV (Auxiliary Aircraft Cargo Ship / Transport):** Transports fully assembled, uncrated airframes without disabling them upon arrival. Requires docking to load.
- **AO (Fleet Oiler):** Equipped with high-speed pumping gear, booms, and flexible hoses for underway replenishment of combat task forces at sea.
- **AOT (Commercial / Auxiliary Oil Tanker):** Bulk fuel/oil hauling between production centers and rear refineries; cannot conduct underway replenishment.
- **TK (Tanker):** General naval fuel and oil tanker.
- **xAK (Converted Commercial Cargo Ship):** Requisitioned civilian merchant freighter; the logistical backbone for general cargo and supply movement.
- **xAKL (Converted Small / Coastal Commercial Cargo Ship):** Requisitioned small merchant freighter for distributed local island logistics.
- **xTK (Converted Commercial Tanker):** Requisitioned civilian tanker for bulk resource/fuel shipment.

#### 6. Mobile Tenders & Auxiliaries
Vessels designed to maintain, repair, and supply combat ships and aircraft away from major industrial bases:
- **AD (Destroyer Tender):** Provides mobile repair, torpedo reloading, and maintenance for DDs, DEs, and small escorts in forward anchorages.
- **AE (Ammunition Ship):** Mobile resupply of heavy artillery shells, aerial bombs, and ammunition; reloads carrier air sorties and capital warships.
- **AKE (Auxiliary Cargo / Ammunition Issue Ship):** Rearms combat ships while anchored in port; AE operates as AKE until underway rearming develops in 1945.
- **AG (Miscellaneous Auxiliary):** General support vessel.
- **AGC (Amphibious Force Flagship):** Dedicated command and control ship with extensive communications arrays for joint landing coordination.
- **AGP (Motor Torpedo Boat Tender):** Mobile mother ship providing fuel, torpedo replenishment, and maintenance for PT boat squadrons.
- **AMC (Armed Merchant Cruiser):** Converted merchant hull fitted with cruiser guns for ocean convoy escort or commerce raiding.
- **AR (Repair Ship):** Mobile workshop vessel reducing repair delays for system, engine, and flotation damage at forward anchorages (up to 5 points Major Engine / Flotation repair).
- **ARD (Auxiliary Repair Dock):** Mobile floating drydock capable of lifting damaged hulls to repair heavy flotation and structural underwater damage (up to 100 repair pts/turn for a single ship within lift capacity).
- **AS (Submarine Tender):** Mobile forward submarine base providing torpedo reloading, engine servicing, and fuel.
- **AV (Seaplane Tender):** Fleet seaplane tender supporting long-range patrol flying boats and reconnaissance floatplanes.
- **AVD (Seaplane Tender - Converted Destroyer):** Fast seaplane tender for rapid establishment of temporary aerial surveillance bases.
- **AVP (Small Seaplane Tender):** Compact seaplane tender for island surveillance outposts.

#### 7. Submarines
Undersea warfare vessels utilized for scouting, commerce raiding, minelaying, and stealth operations:
- **SS (Fleet Submarine):** Ocean-going submarine for merchant interdiction, naval reconnaissance, and covert insertions.

---

### 6.3 Auxiliary & Tender Doctrine Guide

Mobile tenders and auxiliaries are force multipliers in WITP:AE, creating forward operational nodes without requiring high-level port infrastructure:

#### 1. Submarine Tenders (AS)
- **Mechanics:** Submarines normally require a Level 3+ port to reload torpedoes and service engines. An AS functions as a mobile forward submarine base at any anchorage or port (even Level 0), rearming torpedoes and clearing minor system damage.
- **Predictive Grounding:** Detection of an enemy AS advancing to or anchored at a forward base (e.g. Kwajalein, Truk, Shortlands) indicates that extended submarine patrol lines are being established in that maritime sector.

#### 2. Destroyer Tenders (AD)
- **Mechanics:** Destroyers and escorts rapidly expend depth charges and ammunition and incur system wear during high-speed screening. An AD re-arms and repairs DD, DE, PC, and SC hulls at forward anchorages.
- **Predictive Grounding:** Forward staging of an AD signals preparations for sustained surface combat sweeps or intense ASW screening operations from that harbor.

#### 3. Aviation Tenders (AV, AVD, AVP)
- **Mechanics:** Provide Aviation Support points, aviation fuel, and ordnance for floatplanes (F1M Pete, E13A Jake, Kingfisher) and flying boats (H6K Mavis, H8K Emily, PBY Catalina) at coastal bases (even Level 0). Tenders operate aircraft underway only if Air Capacity > 0 (e.g. Wright class = 4, Curtiss = 0, Langley = 0; AVD = 0; Dutch Arend = 1, Poolster = 0). At anchorages, they establish mobile aviation bases.
- **Predictive Grounding:** Forward movement of AV/AVD/AVP assets precedes wide-area maritime search arcs, long-range patrol coverage, and air-sea search over contested sea lanes.

#### 4. Repair Ships & Floating Drydocks (AR, ARD)
- **Mechanics:**
  - **AR (Repair Ship):** Provides mobile repair points that accelerate the reduction of system, engine, and light flotation damage for all ship classes at the anchorage, repairing up to 5 points of Major Engine / Major Flotation damage.
  - **ARD (Auxiliary Repair Dock):** A floating drydock that ONLY repairs flotation damage (including Major Flotation), repairing up to 100 repair points/turn (1.0 flotation point/turn) for a single ship within its Ship Lift Capacity.
- **Predictive Grounding:** Detection of an AR or ARD at an anchorage (e.g. Noumea, Espiritu Santo, Shortlands, Truk) flags that port as an active forward fleet repair hub sustaining offensive naval operations.

#### 5. Ammunition Ships (AE, AKE)
- **Mechanics:** Combat ships rapidly expend heavy-caliber main gun ammunition (8-inch, 14-inch, 16-inch) and anti-aircraft munitions. Heavy shells cannot be reloaded at undeveloped forward bases without specialized lift gear. AKE rearms combat ships when anchored in port; AE rearms underway in 1945 (prior to 1945 it functions as an AKE). Neither can rearm mines or 46cm (18.1-inch) guns. Cargo capacity limits maximum shell size. AE/AKE vessels also replenish carrier air strike sorties.
- **Predictive Grounding:** Concentration of AE/AKE vessels alongside capital combatants (BB/CA) strongly indicates imminent shore bombardment missions or sustained surface combat actions.

#### 6. Fleet Oilers (AO)
- **Mechanics:** Unlike commercial tankers (TK, AOT, xTK) that haul bulk fuel between refineries and depots, Fleet Oilers (AO) possess high-speed pumps and specialized rigging to conduct underway replenishment (replenishing combat task forces at sea).
- **Predictive Grounding:** Task Forces containing AO vessels are identified as long-range striking forces or deep-penetration raiding groups with extended operational endurance far beyond base support radii.

#### 7. Fast Transports (APD)
- **Mechanics:** Converted destroyers carrying a small combat contingent (typically a company) and light equipment at destroyer speeds (25-30+ knots) with ASW escorts. They lack heavy assault capabilities (no amphibious unload bonus), but unload rapidly.
- **Predictive Grounding:** High-speed APD approaches indicate tactical surprise insertions, commando raids, or emergency garrison evacuations rather than full-scale invasion echelons.

#### 8. Auxiliary Mine Tender / Harbor Defense (ACM)
- **Mechanics:** Disbanded at a base with fuel and <10% damage, an ACM preserves up to 150 mines from decay.
- **Predictive Grounding:** Deployment of an ACM indicates long-term harbor fortification and defensive minefield maintenance.

#### 9. Aircraft Transport (AKV)
- **Mechanics:** Transports fully assembled, uncrated airframes without disabling them upon arrival. Must dock to load.
- **Predictive Grounding:** Direct, battle-ready air reinforcement of forward airfields.

#### 10. Coastal Minesweeper (AMc)
- **Mechanics:** Restricted to local sweep operations (1-hex radius); cannot join standard Minesweeping TFs.
- **Predictive Grounding:** Local harbor defensive clearance rather than offensive beach clearing.

### 6.4 Authoritative Base Location & Hex Grid Registry

1. **Comprehensive Geographic Scope:**
   - The theater grid incorporates exactly 1,330 distinct historical base locations across 71 geographical regions (Japan, Japanese Islands, Ryukyus, Kuriles, Korea, Formosa, Manchukuo, Occupied China, Hainan, Indochina, Thailand, Marianas, Carolines, Marshalls, USA/Alaska/Hawaii, Philippines, British Commonwealth/India/Australia/New Zealand, Netherlands East Indies, Free French, Soviet Union, etc.).
   - Each base is mapped to integer hex grid coordinates $(X, Y)$ ranging across the entire Pacific theater map.

2. **Housing Capabilities & Maritime Constraints:**
   - Base locations house ground units, naval vessels, and aircraft.
   - **Maritime Constraint:** Only bases situated on coastal or sea-adjacent hexes house ships (`isPort == true`). Inland bases (e.g. Kofu, Maebashi, Asahikawa, inland China/Manchukuo cities) only house ground and air units.

3. **Base Locations DO NOT Move (Controllers Change Hands):**
   - **Base locations and their $(X, Y)$ hex coordinates are permanently fixed physical geography. Base locations DO NOT move!**
   - The initial nation/region assigned to a base represents the scenario starting disposition.
   - What changes hands during the war is only the controlling nation/faction as bases are captured in operations (e.g., Batan Island, Legaspi, Kota Bharu, Wake Island, Rabaul). Ingestion of combat reports and operations files updates base control dynamically (`updateBaseController()`) while $(X, Y)$ coordinates remain strictly immutable.

4. **Predictive Analytics & Geospatial Integration:**
   - **Model A (Amphibious Assault):** Dynamic coordinate resolution for any target base mentioned in SIGINT unit loadouts or troop movements, eliminating hardcoded coordinates.
   - **Model B (Dead Reckoning Trajectory):** Dynamic nearest-base proximity searches within projected confidence radii to identify approaching assault sectors and intercept windows.
   - **Model D (Logistical Pause):** Port status and turnaround monitoring across all theater ports and forward anchorages.
   - **Tab 2 Geospatial Sightings:** Authoritative bearing and distance calculations relative to closest friendly HQs or bases.
   - **Dossier Entity Search:** Complete indexed lookup and case-insensitive substring search across all 1,330 bases.

### 6.5 Master Hex Map & Geospatial Predictive Intelligence Applications

The authoritative theater hex map (`assets/maps/witp_ae_full_hex_map.jpg`, mirrored in `docs/witp_ae_full_hex_map.jpg`, with top-left origin $(0, 0)$) directly powers six core geospatial intelligence applications:

#### 1. Maritime Obstacle Avoidance & Nautical Pathfinding (Model B: Fleet Trajectory)
- **Current Limitation:** Pure linear dead-reckoning extrapolation (e.g., heading South from Formosa) can mathematically project a fleet moving straight across the island of Luzon.
- **Map Use:** By extracting a binary land/sea hex collision mask from the map grid, Model B can execute nautical pathfinding (A* or Dijkstra along navigable sea hexes through Luzon Strait or San Bernardino Strait).
- **Tactical Value:** Fleet vectors and intercept windows will follow realistic maritime channels, eliminating "ships sailing over mountains."

#### 2. Strategic Chokepoint & Convoy Interdiction (Model G: Established Corridors)
- **Map Use:** The map clearly defines critical natural bottlenecks (Malacca Strait, Sunda Strait, Makassar Strait, Surigao Strait, San Bernardino Strait, Tsushima Strait).
- **Tactical Value:** When SIGINT or recon detects merchant/cargo convoys on either side of a strait, Model G can automatically project their mandatory bottleneck crossing time, generating precise ambush windows for Allied submarines.

#### 3. In-Terminal Tactical Mini-Radar / Sector Map (Tab 2: SIGINT Map)
- **Map Use:** Because top-left is $(0,0)$, we can render a 40×20 CP437 ASCII / ANSI tactical sector radar inside the console War Room alongside the sighting table.
- **Tactical Value:** When you traverse contacts with the cursor keys, the mini-radar can center on that specific theater sector, rendering coastline dots (`#`), friendly ports (`O`), and a blinking indicator (`*`) showing the hostile TF's position and projected heading.

#### 4. Land-Based Air Search & Strike Umbrellas (Models A & F)
- **Map Use:** We can map concentric hex rings around known enemy air bases (e.g., 3-hex Betty/Nell torpedo strike radius, 5-hex medium bomber range, 8-hex flying boat search arc).
- **Tactical Value:** Instantly flags when friendly fleets are about to enter enemy land-based air umbrellas, or highlights blind spots in enemy search coverage where friendly carrier strikes can penetrate undetected.

#### 5. Amphibious Beachhead Approach Lanes (Model A: Amphibious Assault Engine)
- **Map Use:** Combine target base coordinates (e.g., Kota Bharu [51,75], Legaspi [80,77], Vigan [80,73]) with coastal maritime approach hexes.
- **Tactical Value:** Projects the exact approach vector of incoming assault echelons (AP, APA, LST) from staging bases (e.g., Cam Ranh Bay, Takao) directly to the target beach.

#### 6. Hex Grid Calibration & Coordinate Ground-Truth
- **Map Use:** Since the hex grid numbers are visible, we can cross-reference any doubtful coordinate in PBEM reports or future `.pws` decoded ship coordinates against this visual ground truth.

---

## 7. Theater Predictive Intelligence Engine (TPIE) Specifications

The "War in the Pacific: Admiral's Edition" (WiTP:AE) generates massive amounts of semi-structured telemetry data across daily operational, combat, and signals intelligence (SIGINT) logs. Currently, players react to enemy movements post-facto. The Theater Predictive Intelligence Engine (TPIE) is a proposed data-mining and machine learning initiative designed to ingest 30+ days of historical game logs to forecast enemy intent, fleet trajectories, logistical bottlenecks, and attrition rates.

By treating WiTP:AE not just as a game, but as a complex, data-rich simulation environment, the Theater Predictive Intelligence Engine will provide an unprecedented layer of strategic depth. This data-mining initiative will transform raw daily logs into a cohesive, forward-looking intelligence apparatus, fulfilling the core business requirement of turning historical game data into actionable, predictive theater dominance.

WiTP:AE generates extensive operational logs daily. TPIE ingests 30+ days of continuous historical logs to forecast enemy intent, fleet trajectories, logistical bottlenecks, and attrition rates—transforming reactive gameplay into proactive theater control. *(Clarified — Changelog 2026-09-19 18:33 UTC+3: TPIE Tab 3 displays degraded/partial predictions with a confidence caveat from turn 1. No hard data-volume gate before showing the tab. Predictions improve as more turns accumulate.)*

### 7.1 Initial Turn Capabilities (Baseline Example: Dec 7, 1941)

Invasion Warnings: Early detection for Legaspi and Vigan derived from Allied intercepts of embarkation orders.

Landing Focus: Confirmed amphibious concentration at Kota Bharu linking visual sightings, BDA, and named vessels.

Unseen Force Detection: Localization of masked task forces (e.g., Hex 90,96) flagged via elevated radio traffic volume.

Logistical Tracking: Identification of auxiliary vessels (e.g., Midway refueling tanker) to plot replenishment routes.

Trade Route Interdiction: Correlating repeat sightings of merchant vessels (e.g., Ryuyo Maru) into trackable supply lines.

Subsequent turn processing adds recurring enemy behavioral predictions (e.g., "Recon over base every 2 turns; next expected turn 17"), course change alerts, and automated claims adjustment.

### 7.2 Core Predictive Intelligence Modules

Module A: Amphibious Assault & Target Probability Engine

Data Sources: SIGINT Reports (transport loadouts, radio volume), Operational Reports (TF movements).

Capability: Combines unit loadout tracking (e.g., 2/Kure 1st SNLF loaded on xAK moving to Legaspi) with high-volume radio intercepts at staging bases to calculate a Target Invasion Probability Score.

Use Case: Flags Batan Island, Legaspi, and Kota Bharu as >90% probability targets prior to assault initialization.

Module B: Enemy Fleet Trajectory & Intercept Modeling

Data Sources: Operational Reports (sightings, TF speeds/headings), Combat Events (shadowing alerts).

Capability: Applies time-series analysis to fragmented sighting reports (e.g., PBY-4 Catalina sighting 9 ships at hex 85,70 moving East) to project vector, speed, and target destination.

Use Case: Triangulates multi-day patrol sightings to extrapolate strike force and invasion fleet trajectories.

### 7.3 Technical Implementation Pipeline

Phase 1: ETL & Data Ingestion Pipeline

Log Parsing (Regex & NLP): Custom regex engine extracting entities across raw text logs.

Entity Extraction: Task Force IDs, Hex Coordinates (X,Y), Unit Names, Vessel Classes, Speed, Heading, Weather, Event Types.

Contextual NLP: Classifies unstructured log entries (e.g., differentiating "snooped by" vs "shadowed by" vs "detects" to weight confidence).

Data Normalization: Normalizes map grid coordinates into standardized spatial structures (e.g., PostGIS) for geospatial querying. *(Superseded for v1 storage — Changelog 2026-09-15 15:37 UTC: hex-spatial structures are in-process and serialize into the Campaign File; no PostGIS service or data directory.)*

Phase 2: Data Modeling & Storage

Graph Database (Neo4j): Maps entity relationships (Units, Task Forces, Transports, Bases) to trace unit operational lineage. *(Superseded for v1 storage — Changelog 2026-09-15 15:37 UTC: graph model in-process; serializes into Campaign File; no Neo4j service or data directory.)*

Time-Series Database (InfluxDB): Logs daily operational trends (fuel stocks, supply decay rates, fort levels, air pools). *(Superseded for v1 storage — Changelog 2026-09-15 15:37 UTC: time-series model in-process; serializes into Campaign File; no InfluxDB service or data directory.)*

Phase 3: Predictive Analytics & Machine Learning

Markov Chain Models: Computes a 5-step destination probability matrix based on TF position, vector, and historical habits.

Survival Analysis (Kaplan-Meier): Models base "time-to-capture" by tracking bombardment density, fort degradation, and garrison strength.

Heuristic Rule Engine: Fires real-time alerts based on specific SIGINT conditions (e.g., IF Radio_Volume = Heavy AND Manifest = SNLF THEN Alert Invasion).

### 7.4 Project Deliverables & Milestones

Milestone 1: Parser & Core Dashboard (Weeks 1–3)

Deliver ETL pipeline capable of ingesting the 5 core file types.

Provide base UI dashboard showing parsed unit locations and movement histories.

Milestone 2: SIGINT & Target Predictor (Weeks 4–6)

Deploy Amphibious Target Probability Engine.

Output daily "Threat Matrix" identifying top likely targets based on transport manifests and radio traffic.

Milestone 3: Fleet Trajectory & Attrition Models (Weeks 7–10)

Integrate spatial movement algorithms and naval/air attrition models.

Output "Intercept Windows" projecting optimal patrol zones to interdict enemy convoys.

Milestone 4: Full System Integration & Companion API (Weeks 11–12)

Package engine into local companion app reading save states in real-time, rendering predictive overlays on the strategic map.

## 8. Text User Interface (TUI) Standards & Design Specifications

*(v1 UI requirement — Changelog 2026-09-15 ~15:44 UTC / ~15:47 UTC: Ozgur asked for a perfect copy of **ROCHEFORT** (Alien movie ship computer). **On-screen name (decided ~15:47 UTC):** display **WU/TH/UR**. **Implement against this section:** green phosphor aesthetic (§8.1 codes 2, 10, 160) and header/body/footer layout (§8.2–§8.4) as written until scope changes them. No reference images. Film frames are not assets; §8 body text below is unchanged.)*

W.U.T.H.U.R. TUI Architecture & Formatting Standard

### 8.1 Color Palette & Typography (Win32 Console Codes & ANSI VT)

The user interface uses a strict monochromatic green phosphor aesthetic, emulating retro 1980s terminal displays (inspired by Alien):

Dark Green (Code 2): Used for structural borders (=, -), panel framing, inactive text, and secondary details.

Light Green (Code 10): Used for headers, screen titles, active prompts, system status indicators, and numerical data.

Light Green Bold (Code 10 + ANSI \033[1m): *(Added — Changelog 2026-09-21 18:27 UTC+3)*: All screen headers, section titles, and modal headers are rendered in **BOLD** typography via Win32 Virtual Terminal Processing (`ENABLE_VIRTUAL_TERMINAL_PROCESSING` / `0x0004`).

Inverted Highlight (Code 160): Black text on Light Green background; reserved strictly for selected navigation elements.

Red / Error (Platform Default): Used strictly for access-denied or error feedback flashes (accompanied by audio alert beep).

### 8.2 Standard Screen Topography

Every standard screen follows a strict 3-part vertical layout consisting of a Header, Body, and Footer:

A. Header Block

Spans full width of terminal; enclosed with double-line equal signs (=). *(Superseded — Changelog 2026-09-19 19:24 UTC+3: `{INDEX}` removed; screen headers cleanly display `[ {SCREEN TITLE} ]  Turn: {TURN_DATE}` or `[ {SCREEN TITLE} ]`).* All title text is rendered in **BOLD** Light Green.

Standard Header:
```
(Color 2)  ================================================================================
(Color 10 BOLD) [ {SCREEN TITLE} ] Turn: {TURN_DATE}
(Color 2)  ================================================================================
```

Master Menu Header Instrumentation *(Added — Changelog 2026-09-20 15:07 & 2026-09-21 18:27 UTC+3)*:
```
(Color 2)  ================================================================================
(Color 10 BOLD) [ MASTER MENU ]             Active Campaign: {NAME} ({SIDE} Command)
(Color 2)  --------------------------------------------------------------------------------
(Color 10 BOLD)   Files Processed : {PWS_COUNT} pws files | {TXT_COUNT} TXT files | Days Processed : {DAYS}
(Color 2)  ================================================================================
```

B. Body / Content Block

Logical sections are separated by single dashes (-). Section titles are bracketed and rendered in **BOLD** Light Green; data items indent by 2 to 4 spaces:

```
(Color 10 BOLD) [ SECTION HEADER ]
(Color 2)  --------------------------------------------------------------------------------
(Color 2)    > Data Point 1       : (Color 10) {VALUE}
(Color 2)    > Data Point 2       : (Color 10) {VALUE}

(Color 10 BOLD) [ INTERACTIVE LIST HEADER ]
(Color 2)  --------------------------------------------------------------------------------
(Color 160)  [X] SELECTED ITEM (Inverted colors)
(Color 10)   [ ] Inactive Item 1
(Color 10)   [ ] Inactive Item 2
```

Menu Formatting, Modal Text-Wrapping & Selection Rules *(Added — Changelog 2026-09-19 20:35, 2026-09-20 14:18, 2026-09-21 18:27 UTC+3)*:
- **Bold Headers Program-Wide:** All screen headers, section headers, and pop-up modal titles must be displayed in bold font.
- **Zero Text Cutoff & Program-Wide Text-Wrapping:** Text must NEVER be cut off across the box boundary or truncated with ellipses (`...`) when full text can be wrapped. Item titles in modals, telemetry disclosure lines, and notification prompts must wrap across multiple lines cleanly.
- **Screen Cleansing on Modal Entry:** Pop-up modals (specifically War Room Reasoning & Telemetry Evidence modal) must clear the console upon opening to prevent background screen text bleed-through.
- **Vertical Double-Spacing:** All interactive menu items (Start Screen, Master Menu, Campaign Selection, Wiki, Field Manual) are double-spaced with blank lines separating choices to ensure comfortable retro terminal legibility.
- **Continuous Selection Underline or Total Elimination:** Active selections must use an unbroken continuous line. If continuous horizontal line rendering cannot be achieved cleanly in the console font, the underline is eliminated entirely, relying cleanly and exclusively on `ConsoleColor::Inverted` (Code 160). Broken ASCII `-` or `_` characters are forbidden.
- **Right-Aligned Topic Explanation Pop-Up Window:** *(Added — Changelog 2026-09-20 15:07 UTC+3)*: In menus where applicable (System Wiki, Error Registry, Start Screen), topic and option explanations are displayed as a framed pop-up window situated to the RIGHT of the selection menu (columns 43–78), word-wrapped to 30 characters in dimmed phosphor (`ConsoleColor::DarkGreen`). This prevents vertical line jumping and keeps all screens within 80x25 bounds.

C. Footer (Hotkey Legend)

Pinned to the absolute bottom line of the console display, defining keybind actions. *(Superseded — Changelog 2026-09-19 18:33 UTC+3: the example below is a PLACEHOLDER only. Footer content is composed dynamically per screen, showing only the hotkeys relevant to the active screen. No hardcoded footer string anywhere in the codebase.)*

```
(Color 2)  [A] AI  |  [L] List  |  [W] Wiki  |  [C/H/D] Jump  |  [X] Logout  |  [ENTER] Inspect
```
*(Example placeholder — actual hotkeys vary by screen.)*

### 8.3 Interaction Rules

Selection Navigation: Users navigate exclusively using UP ARROW, DOWN ARROW, or TAB keys.

Instant Hotkeys: Bracketed letters (e.g., [N] New Vault) execute keybind actions immediately without requiring list scrolling.

Cursor Rendering: The active cursor position updates by moving the console cursor to target coordinates and re-printing with inverted styling.

Blinking Feedback Loops: Modal prompts and waiting screens utilize a 1.0 Hz sleep loop to toggle text colors (simulating hardware terminal cursors).

Acoustic Feedback Profile: *(Updated — Changelog 2026-09-20 14:20 & 14:21 UTC+3)*:
- **Navigation Click:** Moving selection cursors (`UP/DOWN/TAB`) triggers a short, sombre mechanical relay click (muted 450 Hz frequency, brief 5 ms pulse) via Win32 `Beep(...)`.
- **Page Transitions:** Navigating screens triggers multi-tone teletype chirps (950 Hz, 1400 Hz, 1900 Hz).
- **Silent Background:** Continuous looped ambient radio noise has been permanently removed; all audio operations run against a silent background.

### 8.4 Modal Overlays

Inspecting detailed intelligence reports or triggering module launches renders a centered framed pop-up window over the active screen buffer:

Detailed Intelligence Report Overlay:
```
(Color 2)    +-----------------------------------------------------------------------+
(Color 2)    | (Color 10) [ DETAILED INTELLIGENCE REPORT ]                           |
(Color 2)    |-----------------------------------------------------------------------|
(Color 2)    | (Color 10) TITLE: {Report Title}                                      |
(Color 2)    |                                                                       |
(Color 2)    | (Color 10) RATIONALE:                                                 |
(Color 2)    | {Multiline reasoning block, properly word-wrapped to 65 chars}        |
(Color 2)    |                                                                       |
(Color 2)    |                   [ BLINKING: PRESS ENTER TO RETURN ]                 |
(Color 2)    +-----------------------------------------------------------------------+
```

Framed Pop-up Notification Modal *(Added — Changelog 2026-09-20 15:07 UTC+3)*:
Engaging modules (such as `[A] War Room` or `[S] Settings`) renders a centered framed modal box (`Layout::renderNotificationModal`) instead of overwriting screen lines:
```
(Color 10)   +-- [ WAR ROOM ENGAGED ] ------------------------------------------+
(Color 2)    |                                                                  |
(Color 10)   |  RETRIEVING THEATER INTELLIGENCE & TPIE TELEMETRY...             |
(Color 2)    |  Stand by for tactical intercepts and strike concentration.      |
(Color 2)    |                                                                  |
(Color 10)   +------------------------------------------------------------------+
```

## 9. Platform Architecture & Deployment Scope

Initial production deployments target Microsoft Windows environments, followed by Linux distribution testing to support Wine/Proton compatibility layers.

## 10. Version 1 Scope Boundary

In Scope (Version 1.0): All core capabilities in Sections 4 through 8, alongside manual/automated test suites built against golden game reference files.

Out of Scope (Deferred to v1.1+): Detailed surface combat log parsing; dynamic global weather extraction; rendering individual ship tactical map icons; cloud synchronization or multi-user features.

Permanently Out of Scope (Deleted per Ozgur Directive 2026-09-21 20:46 UTC+3): Direct writing or modification of game files. WITP-CIC enforces an absolute read-only guarantee across all versions; the application strictly reads and analyses exported reports and save saves without writing to, modifying, or intercepting game files or memory.

## 11. Quality Assurance & Acceptance Standards

All report and .pws file types are parsed into structured data models and maintained as permanent baseline ("golden") reference files. Every build release must execute automated validation suites reproducing target counts (report totals, sinkings, warning indicators) across synthetic multi-turn scenarios.

A rigorous manual QA checklist verifies cross-platform installer paths, setup wizard flows, and dashboard interaction states.

Release Gate: Builds will not be approved for deployment unless all automated tests pass with 100% success and unparsed line rates remain strictly below 1.0%.

## 12. Risk Analysis & Mitigation Strategies

Game Patch Formatting Alterations: Mitigated via defensive parsing algorithms, unread-line logging, and golden reference regression suites.

Scope Creep: Mitigated by strictly freezing Version 1.0 boundaries.

Niche Audience Risk: Mitigated by zero infrastructure overhead (serverless, local deployment) and open community distribution.

## 13. Error Handling & Documentation Standards

Error Format: All application errors must use a strict 3-digit code system formatted as [XXX] (e.g., [101], [404]).

External Documentation: Every generated error code MUST be logged and fully defined in external developer documentation.

Error Registry: The internal Wiki module must contain a dedicated section listing all error codes and user resolutions.

## 14. Glossary

PBEM: Play-By-Email; asynchronous multiplayer where players exchange turn data files.

Hex: Hexagonal map coordinate cell representing physical geography in the game.

SIGINT: Signals Intelligence; operational intelligence derived from intercepted communications.

Task Force (TF): A fleet or naval grouping organized by the player for a specific mission.

Fog of War: Strategic ambiguity and incomplete information simulated by the game engine.

BDA: Battle Damage Assessment; evaluations of damage inflicted or target sinkings.

CIC: Combat Information Center; tactical intelligence display hub serving as the model for this application.

---

## FAQ (answers from Ozgur, received 2026-09-15)

What is the source of all these logs?

These are generated by the game itself at the end of the each game turn in a pbem multiplayer campaign game. Each side receives his own version of the files. There may or may not be different information for the same turn for 2 players.

Are they exported from an in-game save file, or read during live gameplay via memory hooks?

They are produced by the game at the end of a turn. Player A sends his moves to the Player B by email, game takes both move files and run the simulation and produces the results which u can see in hose files.

Is this tool intended to be used in a PBEM (Play-by-Email) environment?

Yes

Which game or engine are we modeling here exactly?

War in the Pacific : admirals edition by matrix games.

Since you mention "PBY-4 Catalina", "Task Force", and "amphibious invasion", it seems like there's a naval simulation aspect involved.

yes.

Is this tied to an existing game, or a fictionalized version of one?

Game exists.

What language(s) will be used for the application?

C or C++ or JS

From a system standpoint, this could be a mix of C++, Python, .NET (for Win32), etc., especially given the Win32 console TUI and .NET Core backend.

Agreed

*(Superseded for v1 — Changelog 2026-09-15 15:37 UTC: primary language **C++**, Windows console **`.exe`**; Win32 TUI per §8.1; not Python/JS/.NET as primary. Ozgur: “U decide. I need an exe file.”)*

How do you plan to handle multi-turn scenario replay?

U will receive files from the outputs of these multiple turns.

Do you store the full history in time-series databases and perform retrospective analysis on parsed events?

I want you to write the code for that into the witp-cic. Yes I want to see these things.

Is simulation replay part of a larger feature set or separate process?

Simulation replay happens after both player files are merged and run through the game model.

What specific types of raw text logs do you parse? Example line: "PBY-4 Catalina sighted 9 ships at hex 85,70 moving East" — where would these come from? Are they from .pws files?

These come from the text files that are produced at the end of each game turn and also from the .pws files. Each turn , a combat events, operations report and sigint file is produced for each player. One turn is 1 day. There are about 4 years worth of game time.

Are the time-series values normalized per-turn or aggregated over timeframes?

Normalized per turn.

You talk about "daily operational trends" → how is that data derived?

You will find them

Which Markov Chain model specifically?

Ur decision. Make a comparative analysis and find the right one.

Do you use state transition matrices of fixed-size steps (e.g., 5-step)? What are the inputs (heading, speed, current location)?

I dont get this question. But yes, inputs are heading , speed

For the “Survival Analysis” (Kaplan-Meier), how is base destruction modeled?

Ur decision. Make a comparative analysis and find the right one.

Is bombardment density based on number of strikes, average tonnage, or weighted probabilities from attack records?

Yes all of them and u can find others

Does SIGINT data feed directly into rule engines for alert triggering, or are alerts only generated off parsed tactical movements?

No the game itself is not AI dependent but rule based. There maybe something like u mention though.

What tools/processes are used to generate the “Golden” reference files for QA?

They are actual game outputfiles, use them for format reference. Each campaign has different files but format stays the same.

Are test cases created manually, or automatically derived from saved game scenarios?

U create them and I OK them and then u test them

On the TUI side – are you building a custom console rendering engine, leveraging something like ncurses, or just using native Windows Console APIs with color code injection (like SetConsoleTextAttribute) ?

No idea…

Do you expect to support real-time updates of game state?

No

For example, will the UI update dynamically as the log stream changes?

Log stream changes discretely over time as each turn is played by pbem

How do the “Intercept Windows” work?

That is a windows on the screen.

Is this purely a projection from known trajectories and patrol zones, or is it interactive where the user can define constraints?

Both is asked. U can start with projection.

For Partisan Threat Modeling – how are those historical vectors (e.g., Colombo, Howrah) encoded into models?

U will model them into the witp-cic.

Are they geospatial lookup tables tied to garrison strength ?

Check the pws file , there maybe info there

Can the system scale into multi-user, or will it remain strictly single-player/local-only?

Single player, pbem helper

---

## Open contradictions (historical — resolved 2026-09-15 15:37 UTC)

These sentences remain in the bible for traceability. **Implementers follow Changelog 2026-09-15 15:37 UTC** (and `docs/architecture-decisions.md`).

1. Section 1 Mac-first vs §3 Windows paths vs §9 Windows-first deployment → **Decided:** Windows first for v1; `C:\WitP-CIC`.
2. §3 `\Campaigns` only vs §4.1 `\Campaign Vaults` → **Decided:** `\Campaigns` only; `\Campaign Vaults` is a synonym error.
3. Language C/C++/JS vs C++/Python/.NET mix → **Decided:** C++ primary, Windows console `.exe`.
4. §3 single Campaign File vs §7.3 PostGIS, Neo4j, InfluxDB → **Decided:** in-process models serializing into one Campaign File; no external DB services or data dirs.

---

## Still waiting (not in this document)

- **Golden `.pws` set (2026-09-19):** seven saves catalogued in project store — six `wpae021` (Japanese → Allied) + `wpae023` (Ozgur outgoing Allied → Japanese); see Changelog 2026-09-19 and [`docs/pws-knowledge-summary.md`](pws-knowledge-summary.md). Separate Japanese-side-only golden `.pws` still not delivered as its own reference file.
- GitHub private repo WITP-CIC-C (blocked on GitHub authentication and owner login).
