# WITP:AE CIC Intelligence Helper — Version 1 To-Do List

**Interactive Acceptance Rule (2026-09-20 19:21 UTC+3 — Ozgur):** Every single requirement in this document has a checkbox (`- [ ]`). When Ozgur clicks it (in GitHub or markdown view) or comments "done", it is officially accepted and marked done (`- [x]`). Writing code and passing tests is a prerequisite, but checking off an item requires Ozgur's explicit action or confirmation.

**Scope Addition & Conflict Clarification Rule (2026-09-20 15:05 UTC+3 — Ozgur):** Every new todo item and correction MUST be added to the business scope (`docs/scope-v1.md`). If there is any conflict between a new requirement and existing scope/architecture, the agent MUST ask Ozgur to clarify before taking action.

**Core Docs Rule (2026-09-19 20:57 UTC+3 — Ozgur):** All documentation covering baseline principles (`docs/baseline_witp_cic.md`), to-do lists (`docs/todo.md`), lessons learned (`docs/lessons-learned.md`), design decisions (`docs/architecture-decisions.md`), screen flows (`docs/screen-flow.md`), scope (`docs/scope-v1.md`), project context (`docs/project-context.md`), and UI specifications are Core Docs. They must be maintained and synchronized continuously.

Derived solely from `docs/scope-v1.md` (design spec bible). Cheaper model selected for implementation. Stack decisions: `docs/architecture-decisions.md` (2026-09-15 15:37 UTC).

---

## Active Required Corrections (Ozgur Directives)

- [ ] **[CORRECTION 2026-09-24 23:20 UTC+3]:** Complete Graphics Sub-Menu TBD Standardization: Replace all 10 menu items `[1]` through `[10]` in the Graphics Sub-Menu with `[ TBD ]`. Route selection of any option to the standardized reserved partition placeholder display with rigid monospace straight borders (`boxWidth = 78`, `innerWidth = 74`). Retain underlying analytical models in `TelemetryAnalyticsEngine.cs` (§4.5, §8, ADR 73).
- [ ] **[CORRECTION 2026-09-24 23:10 UTC+3]:** Graphics Sub-Menu Telemetry Views (Pages 1–5) & TBD Reservation (Slots 6–10): Implement Pages 1 through 5 from wireframe blueprints in `docs/OPENCODE INPUT/` ([1] Logistical & Attrition Telemetry, [2] Strategic Vector Analysis, [3] Intelligence & Anomaly Detection, [4] Operational Tempo & Pivots, [5] Predictive Forecasting). Strictly maintain slots 6 through 10 as [ TBD ] placeholders. Ground all viewmodels in campaign telemetry via TelemetryAnalyticsEngine, enforcing 100% straight monospace borders (`boxWidth = 78`, `innerWidth = 74`) (§4.5, §7, §8, ADR 72).
- [ ] **[CORRECTION 2026-09-24 22:35 UTC+3]:** Proportional Dynamic Font & Chrome Scaling System (Option 1): Automatically scale terminal font size (`TerminalOutputBox.FontSize`), line height (`LineHeight = FontSize * 1.35`), header chrome (system badges, quick-switch buttons, header padding), and footer chrome (status labels, active file text, resolution dropdown) across all window sizes and resolution presets (1024x640, 1280x720, 1366x768, 1920x1080, and new 2560x1440 QHD). Ensure 100% straight monospace borders and visual harmony at high resolutions (§8.1, §8.2, ADR 71).
- [ ] **[CORRECTION 2026-09-24 09:14 UTC+3]:** Graphics Option [1] Replaced with TBD Marker: Remove Logistical & Attrition Telemetry page from the active UI display and replace option [1] in the Graphics Sub-Menu with [ TBD ]. When selected, display a standardized reserved telemetry partition TBD screen with straight monospace borders (boxWidth = 78, innerWidth = 74). Retain underlying analytical engine algorithms (§4.5, §8).
- [ ] **[CORRECTION 2026-09-23 22:23 UTC+3]:** War Room Graphics Sub-Menu (10 Items), Analytical Algorithms & Page 1 Telemetry: Rename War Room Option [3] from TBD to Graphics (reached by pressing 3 or selecting [3]). Render 10-item sub-menu with option [1] as Logistical & Attrition Telemetry and options [2]-[10] strictly designated as [ TBD ]. Ingest and implement core analytical algorithms from analytical_algorithms.md (CoG, vector projection, coordinate clustering, OLS regression slope, OPTEMPO 3-day rolling average, strategic pivot latency, logistical shadow, intelligence blindspots, bait patterns, global tempo state machine). Implement Page 1 full-screen visual telemetry display with rigid straight borders (boxWidth = 78, innerWidth = 74) (§4.5, §7, §8).
- [ ] **[CORRECTION 2026-09-22 22:15 UTC+3]:** War Room Graphics Domain & ASCII Wireframe Blueprint: Rename War Room Option [3] from TBD to Graphics with a 6-item navigation sub-menu (`Trend Lines`, `Correlation & Logic`, `Strategic & Intelligence`, `Tacticals`, and two unassigned `[ TBD ]` categories). Render full-screen visual presentations matching `media_1790103739294.jpg` with rigid straight borders. Provide editable ASCII wireframe specification template `docs/graphics-blueprints-template.md` (§4.5, §8).
- [ ] **[CORRECTION 2026-09-22 21:45 UTC+3]:** In-Terminal Unicode Character Graphing & Straight Border Assurance: Implement ADR 64 (Option 1 in-terminal character graphing) inside Heuristics pop-ups with rigid monospace box alignment. Guarantee that the right border `║` is 100% straight and never shifts, bends, or wobbles. Relocate phosphor quick-switch buttons to top window header bar beside CIC matrix badge (§8.1, §8.2).
- [ ] **[CORRECTION 2026-09-22 20:25 UTC+3]:** Evidence Transparency, Resolution Menu & Phosphor Ribbon Controls: Expose exact mathematical formulas and raw data sources in pop-ups; embed resolution dropdown menu in bottom ribbon; add phosphor quick-switch pill buttons (§8.1, §8.2).
- [ ] **[CORRECTION 2026-09-22 19:35 UTC+3]:** War Room Heuristics Domain & The 6 Logical Blueprints: Add dedicated Heuristics screen as option `[6]` in War Room Primary Screen covering metrics 1, 4, 5, 46, 49, and 50 with temporal guard and raw telemetry drill-downs (§4.5, §7).
- [ ] **[CORRECTION 2026-09-21 23:48 UTC+3]:** Core Development, Testing & Scope Rigor Baseline Mandate: Codify 7 baseline rules: (1) Always test before delivery, (2) Don't hallucinate, (3) Do not finish until tests run OK, (4) Test results must strictly cover wanted output, (5) Align new features/corrections with current scope and ask on clashes/ambiguity, (6) Store all added functionality in documentation in foolproof detail, (7) Proactively watch for logic clashes in algorithms and alert Ozgur (§1.13, §11).
- [ ] **[CORRECTION 2026-09-21 23:38 UTC+3]:** Universal Continuous Unicode Box Drawing Characters: Eliminate all ASCII box borders (`+`, `-`, `|`) across the entirety of the project and replace with seamless continuous Unicode box-drawing characters (`╔ ═ ╗ ║ ╚ ═ ╝ ╠ ═ ╣` and `┌ ─ ┐ │ └ ─ ┘`), matching the continuous double-line header style (§8.1, §8.2).
- [ ] **[CORRECTION 2026-09-21 23:32 UTC+3]:** Phase Progression to Alpha 4 & Binary Naming Convention: Advance phase label to **Alpha 4** (`alpha4`). Release binaries follow `witp-cic-muthur-alpha4-[build#].exe`, where `alpha4` is the new phase name and `0` is the 1st build incrementing by 1 (first binary: `dist/witp-cic-muthur-alpha4-0.exe` with mirrors to `dist/WitP-CIC.exe` and `dist/MUTHUR6000.exe`) (§1.13, §3).
- [ ] **[CORRECTION 2026-09-21 22:20 UTC+3]:** Project Switch to Standalone `witp-cic-muthur` Repository: Formally switch active master development to `witp-cic-muthur` (`C:\Users\ozgur\Documents\Witp-CIC-MUTHUR`). Lock `Witp-CIC2` as an immutable reference archive. Integrate MUTHUR 6000 CRT phosphor architecture (.NET 8 WPF single-file binary) with complete mathematical and screen flow parity, zero flicker, instant screen drawing, and full keyboard navigation (§1.13, §3, §8).
- [ ] **[CORRECTION 2026-09-21 20:53 UTC+3]:** Phase Progression to Alpha 3: Officially label project phase as **Alpha 3** (`v0.02-alpha3`). Next compiled binary resets build counter to 0: `dist/witp-cic-v0.02-alpha3-0.exe` (with `dist/WitP-CIC.exe` mirrored) (§1.13, §3).
- [ ] **[CORRECTION 2026-09-21 20:46 UTC+3]:** Scope & To-Do Deletion: Writing / Modifying Game Files: Permanently deleted any prospective writing or modification of game files from active todo list and scope. WITP-CIC operates strictly as a read-only passive intelligence assistant with zero writeback to game files in v1.0 or any future release (§10, baseline principle).
- [ ] **[CORRECTION 2026-09-21 18:54 UTC+3]:** Turn Count Disambiguation & Multi-Day Ingestion Verification: Disambiguate item count from turn count in pop-up headers by replacing 'Turn: COUNT: X' with explicit instrumentation showing both current turn date, total days loaded ('Turn: <Date> (<N> Days Loaded)'), and separate item count ('ITEMS: <X>'). Verify that all ingested telemetry across all loaded days (8 days in active vault) is fully processed into all predictive models and sighting logs (§4.2, §4.5, §8.2).
- [ ] **[CORRECTION 2026-09-21 18:40 UTC+3]:** Hierarchical Reasoning Modal Drill-Down & Program-Wide Text Cutoff Fix: Transform reasoning modal into an interactive topic directory with cursor traversal. Merge Telemetry Source and Localized Position into a unified drill-down revealing detection means, observation frequency, and dates seen. Segregate Confidence Analysis computations into a dedicated drill-down disclosed only on pressing ENTER. Enforce complete multi-line word wrapping program-wide to eliminate border clipping (§4.5, §8.1, §8.2).
- [ ] **[CORRECTION 2026-09-21 18:27 UTC+3]:** Bold Headers & Program-Wide Text-Wrapping: Render all screen and section headers in bold. Eliminate text truncation and cutoff across all modal and pop-up boxes by enforcing full multi-line text wrapping (including titles and reasoning disclosures) and eliminating background screen bleed-through (§4.5, §8.1, §8.2).
- [ ] **[CORRECTION 2026-09-21 18:15 UTC+3]:** Opponent-Only Intelligence Display: War Room menus (specifically [ BATTLE DAMAGE ASSESSMENT & CLAIMS REVISION REGISTER ] and related intelligence tracking menus) must strictly display ONLY the opposing side's assets (Japanese in an Allied campaign; Allied in a Japanese campaign). Friendly casualties and own-force damage must never be shown, as the player already manages friendly status through operational reports (§4.5, §6).
- [ ] **[CORRECTION 2026-09-20 23:02 UTC+3]:** Milestone Alpha 2 & Binary Naming Baseline: Officially advance project milestone to Alpha 2 (`v0.02-alpha2`). When compiling new executables, strictly enforce the naming format `witp-cic-[mainversion id]-[phaseid]-[built no]` with main version in `V.XX` format, phase ID (e.g. `alpha2`), and build number starting from `0` for each new main phase ID (§1.13, §3).
- [ ] **[CORRECTION 2026-09-20 20:28 UTC+3]:** Master Menu System Settings Sub-Item: Add a setting to Master Menu `[S] SYSTEM SETTINGS & DETECTION THRESHOLDS` as a sub-item: `[1] CONNECT TO PREDICTIVE AI MODEL` (§4.2).
- [x] **[CORRECTION 2026-09-20 17:05 UTC+3]:** In-Terminal Hierarchical Manual Ingestion: Under `[ FIELD MANUAL ]`, delete options 2 and 3. Do not launch manual wiki as an external file; ingest it directly into current program as hierarchical JSON (`assets/manual/witp_ae_manual.json`). UI look and feel must match Nostromo green phosphor. No Table of Contents label at top, no Index line. First menu shows `X.0` level topics; selecting and entering drills down into `X.1` level and so on (§4.4, §8.2).
- [x] **[CORRECTION 2026-09-20 16:41 UTC+3]:** Field Manual: Under `[ FIELD MANUAL: WITP:AE GAME MANUAL BROWSER ]`, change section header `[ C H A P T E R   B R O W S E R   &   R A G   S E A R C H ] -> [ WITP:AE MANUAL ]`, delete the line that starts with `Index : ....`, and replace current content with `C:/Users/ozgur/Documents/WITP-CIC ASSETS/docs/rag/witp-ae/witp_ae_manual_wiki.html` (§4.4, §8).
- [x ] **[CORRECTION 2026-09-20 16:31 UTC+3]:** Standardized Upgrade & Rollback Procedure: Codify rollback instructions into `docs/how-to-rollback-to-alpha-1.md` and enforce 6-step lifecycle procedure for all future upgrades (§3, §14).
- [ ] **[CORRECTION 2026-09-20 15:07 UTC+3 - Item 1]:** Master Menu Header Instrumentation: Relocate active campaign and database telemetry stats from the menu body into the top header block: `Files Processed : XX pws files | xx TXT files | Days Processed : xx`, all divided by `|` alongside campaign/theater info (§4.2, §8.2 A).
- [ ] **[CORRECTION 2026-09-20 15:07 UTC+3 - Item 2]:** War Room Launch Pop-up Modal: When user presses War Room (or Settings), display the engagement/retrieval message inside a framed pop-up modal dialog box instead of overwriting screen text (§4.2, §8).
- [ ] **[CORRECTION 2026-09-20 15:07 UTC+3 - Item 3]:** Right-Aligned Topic Explanation Pop-Up Window: In the System Wiki & Error Registry menu (and other screens where applicable), display the `TOPIC EXPLANATION` as a compact, framed pop-up window situated to the RIGHT of the selection menu (columns 43-78), using compact formatting/dimmed phosphor (§4.3, §8).
- [x] **[CORRECTION 2026-09-20 14:21 UTC+3]:** Acoustic Navigation Click Profile: Change the key move click to a shorter, sombre click (lower frequency, very brief duration instead of high-pitch 1800 Hz) (§5.2, §8).
- [x] **[CORRECTION 2026-09-20 14:18 UTC+3 - Item 1]:** Selection Underline Rendering: When an option is chosen, it is underlined with a continuous line (not broken `_` or `-` characters). If continuous line rendering cannot be achieved cleanly in the console, remove the underline totally (§8).
- [ ] **[CORRECTION 2026-09-20 14:18 UTC+3 - Item 2]:** Option/Topic Explanation Pop-Up Window: The explanation text must be presented as a pop-up window with slightly smaller fonts / compact modal presentation rather than inline text (§4.3, §8).
- [x] **[CORRECTION 2026-09-20 14:15 UTC+3]:** When a current campaign is loaded, make sure that any new turn files as `.pws` or `.txt` are present in the current folders and ingest them and update the war room models (§4.1, §4.5).

---

## §3 — Development architecture, constraints, and deliverables

- [ ] Purge the in-memory knowledge base completely on application start (§3)
- [ ] Purge the in-memory knowledge base completely when switching between campaigns (§3)
- [ ] Purge the in-memory knowledge base completely on application exit (§3)
- [ ] Enforce absolute sandbox: campaign data belongs to one side only; one campaign cannot influence another (§3)
- [ ] Store all campaign data (parsed data, text, results) compressed/zipped in a single custom Campaign File (§3)
- [ x] Store Campaign Files only in the `\Campaigns` directory under the installation path (§3)
- [ ] Prohibit external or orphaned campaign data files outside the Campaign File model (§3)
- [ ] Document any future business-requirement additions in the spec bible with date/time stamp (§3, Requirements Traceability Notice)
- [ ] Support default installation path `C:\WitP-CIC` (§3)
- [ ] Offer `\Campaigns` under the install path when browsing for vault/campaign file locations (§3)

---

## §4 — Operational application flow and screens

### Global navigation

- [x ] On any screen, pressing ESC moves the view back one screen at a time (§4)

### §4.1 — Initial start screen and campaign lifecycle

- [x] Show a short splash screen when the application opens (§4.1)
- [x] Fix console attribute bleed where pressing ENTER from Splash caused inverted/green background to fill Start screen (Changelog 2026-09-19 19:54 UTC+3)
- [x] Move Splash prompt block further down the page and fix duplicate overlapping text during blink loop (Changelog 2026-09-19 19:44 UTC+3 item 7)
- [x] After splash, prompt: continue a current campaign or start a new one (§4.1)

**New campaign setup wizard (new window) — authoritative step list (arch-decisions §8):**

- [x] Ask which side the player commands (§4.1)
- [x] Prompt for the `.pws` PBEM password — store in vault for user reference only; do NOT use for file decompression (§4.1; arch-decision §7, Q1 2026-09-19)
- [x] Find the folder where the game writes its reports: auto-search `...\SAVE` for `.pws` and `...\SAVE\archive` for text reports, with browse option if missing (§4.1; arch-decision §19, Q D1 2026-09-19)
- [x] Support adding multiple directories with [B] without eliminating previously entered ones (Changelog 2026-09-19 19:44 UTC+3)
- [x] Display pop-up listing all discovered `.pws` and text files taken in and processed (Changelog 2026-09-19 19:44 UTC+3)
- ~~[ ] Confirm the game's turn length~~ **REMOVED** — not wanted; ingest covers all dates/files automatically (Q3 2026-09-19)
- ~~[ ] Validate the `.pws` password~~ **REMOVED** — password is a game-UI credential only; no validation code path (Q1 2026-09-19)
- [x] Prompt the user for a campaign vault file name (§4.1)
- [x] Persist campaign file under `C:\WitP-CIC\Campaigns` only (§4.1; Changelog 2026-09-19 19:44 UTC+3 item 6)
- [x] Physically create and serialize the `.wic` container file to disk on new campaign setup so it is preserved across app restarts
- [x] Store campaign information including location of actual game campaign files (do not re-request unless missing) (§4.1)
- [ ] Ingest all text output files and detected `.pws` files for all discovered dates automatically (§4.1)
- [x] Maintain all collected and generated data within the vault file (§4.1)
- [x] Transition user to the Master Menu Screen when setup completes (§4.1)

**Continue a current campaign:**
 
 - [x] Present a screen to choose from available campaign master files (`.wic` in `C:\WitP-CIC\Campaigns`) (§4.1; Changelog 2026-09-19 19:44 UTC+3)
 - [x] Auto-create `C:\WitP-CIC\Campaigns` if missing and support browsing to alternate folders
 - [x] Load the selected campaign file (§4.1)
 - [x] Verify stored file paths on load (§4.1)
 - [x] **[REQUIRED CORRECTION]:** When a current campaign is loaded, make sure that any new turn files as `.pws` or `.txt` are present in the current folders and ingest them and update the war room models (§4.1, §4.5; Ozgur directive 2026-09-20).


### §4.2 — Master Menu Screen
 
- [x] Present module `[A] War Room` (§4.2)
- [x] Present module `[W] Wiki` (§4.2)
- [x] Present module `[S] Settings`: Allied SIGINT confidence toggle + Strike Concentration thresholds (cruiser count, hex radius), with descriptive explanations (§4.2, §6; arch-decision §15, Q A2 2026-09-19)
- [x] Present module `[E] Exit` (§4.2)
- [x] Support navigation via corresponding hotkey letters (§4.2)
- [x] Support navigation by traversing items with arrow keys (§4.2)
- [x] Pressing ESC triggers Exit confirmation prompt (§4.2; arch-decision §16, Q A3 2026-09-19)
- [x] Display `Days Processed: {COUNT}` and `Turn: {LAST_PROCESSED_DATE}` (proper date format, e.g. Dec 08, 1941) in top header (Changelog 2026-09-19 19:44 UTC+3)

### §4.3 — Wiki module

- [ ] Implement integrated Wiki and Help with hierarchical design by primary section headers (§4.3)
- [ ] Route selecting a main section header to a dedicated sub-page for that topic (§4.3)
- [ ] Continuously update Wiki as new features, functions, and error codes are added (§4.3)
- [ ] Store official WITP:AE game manual as `docs/witpae-manual.txt` (Changelog 2026-09-15, §4.3)
- [ ] Index game manual with local RAG for Wiki ingestion (Changelog 2026-09-15, §4.3)
- [ ] Use game manual as in-app Wiki source (Changelog 2026-09-15, §4.3)
- [ ] Consult game manual when making product decisions (Changelog 2026-09-15, §4.3)

### §4.4 — Exit module

- [ ] Execute clean shutdown of WITP-CIC (§4.4)
- [ ] Purge all in-memory data structures prior to closing (§4.4)

### §4.5 — War Room dashboard

- [ ] Implement dark, high-contrast CIC-style primary dashboard with Standard Menu (`[1] Intelligence Summary`, `[2] SIGINT Map`, `[3] TBD`, `[4] TPIE Warning Dashboard`, `[5] Predictive Models Sub-Menu`) replacing tabs (§4.5; Changelog 2026-09-20 21:18 UTC+3)
- [ ] Transition user into dedicated sub-screen upon selecting any primary menu item (§4.5; Changelog 2026-09-20 21:18 UTC+3)
- [ ] Enable cursor key traversal (`[UP/DOWN]`) across all items and bullets within sub-menus and pop-up manifests (§4.5; Changelog 2026-09-20 21:18 UTC+3)
- [ ] On ENTER over any bullet or report item, launch modal detail overlay showing analytical reasoning, evidence, and calculations for that specific line (§4.5; Changelog 2026-09-20 21:18 UTC+3)
- [ ] Standardize bottom phrase across all sub-screens and modals to `PRESS ESC TO RETURN` (§4.5; Changelog 2026-09-20 21:18 UTC+3)
- [ ] Display sighting `lastSeenDate` (the last time TF was seen, formatted as `Dec 14`) in SIGINT Map so date column is never blank (§4.5; Changelog 2026-09-20 21:40 UTC+3)
- [ ] Word-wrap all text inside reasoning modals across continuation lines with 4-space indent so no text is cut off or truncated with `...` (§4.5; Changelog 2026-09-20 21:40 UTC+3)
- [ ] Display graphical Confidence Meter Bar (`CONFIDENCE LEVEL: [==========] XX%`) in contact modals, with slow 600ms in-place blinking for `>=90%` confidence (§4.5; Changelog 2026-09-20 21:40 UTC+3)
- [ ] Enforce Anti-Bullshit Policy: render `TACTICAL REASONING:` and `INTERCEPT SIGNIFICANCE:` as `*-**-**-**-*-*` until grounded AI/predictive models analyze the contact (§4.5; Changelog 2026-09-20 21:40 UTC+3)
- [ ] Maintain consistent, fixed-column modal and pop-up borders (`| ` and ` |`) that never scramble or shift during cursor traversal (§4.5; Changelog 2026-09-20 21:40 UTC+3)
- [ ] Implement deterministic 4-phase Numerical Confidence Engine (§6.1; Changelog 2026-09-20 22:08 & 22:27 UTC+3)
  - [ ] Implement `RawReport` and `ConfidenceReport` structures with integer-keyed `HexMap` (`X * 1000 + Y`) and per-turn `EntityMap`
  - [ ] Implement Phase 1 fast non-regex parsing and base scoring: Radio=30, Garrison=50, Movement=65, Location=75, Visual Recon=80, and Domain inference (`AIR`, `NAVAL`, `LAND`)
  - [ ] Implement Phase 2 per-turn corroboration calculations (+15 entity repetition bonus, +10 hex repetition bonus; Visual Recon populates HexMap only)
  - [ ] Implement Game State Manager cross-turn persistence (updating contact timestamp and resetting Time Decay to 1.0 on recurrence)
  - [ ] Implement Phase 3 time decay ($\Delta t = 0 \to 1.0$, $\Delta t = 1 \to 0.8$, $\Delta t > 1 \to 0.5$) and score clamping strictly $0\% - 100\%$
  - [ ] Assign standardized confidence tiers: `UNCONFIRMED` (0–39%), `PROBABLE` (40–69%), `CONFIRMED` (70–89%), `VERIFIED` (90–100%)
  - [ ] Render clean modal header (`CONFIDENCE: XX% [TIER]`) paired with full English mathematical breakdowns in reasoning bodies
- [ ] Implement Naval Unit Capability Registry across 7 functional classes & 40+ hull codes (§6.2; Changelog 2026-09-21 13:27 UTC+3)
  - [ ] Class 1: Major Surface Combatants & Carriers (CV, CVL, CVE, BB, BC, CA, CL, CLAA, CS)
  - [ ] Class 2: Surface Escorts & Light Combatants (DD, DE, Frigate/PF, KV, PC, PCE, PG, PB, SC, PT)
  - [ ] Class 3: Mine Warfare (DM, DMS, CM, CMc, AM, AMc, ACM)
  - [ ] Class 4: Amphibious & Troop Transports (AP, APA, APc, APD, LSD, LST, LCI, LCM, LCVP, xAP)
  - [ ] Class 5: Cargo & Logistics Freighters (AK, AKA, AKL, AKV, AO, AOT, TK, xAK, xAKL, xTK)
  - [ ] Class 6: Mobile Tenders & Auxiliaries (AD, AE, AKE, AG, AGC, AGP, AMC, AR, ARD, AS, AV, AVD, AVP)
  - [ ] Class 7: Submarines (SS)
- [ ] Implement Mobile Auxiliary & Tender Doctrine Engine (§6.3; Changelog 2026-09-21 13:27 UTC+3)
  - [ ] Submarine Tenders (AS): Model forward submarine base establishing extended patrol lines
  - [ ] Destroyer Tenders (AD): Model forward escort/destroyer rearming and maintenance bases
  - [ ] Aviation Tenders (AV, AVD, AVP): Model mobile aviation support and maritime search arc establishment
  - [ ] Repair Ships & Docks (AR, ARD): Model forward fleet repair hubs (AR: system/engine/flotation up to 5 pts; ARD: flotation up to 100 pts/turn)
  - [ ] Ammunition Ships (AE, AKE): Model mobile capital warship & carrier sortie replenishment
  - [ ] Fleet Oilers (AO): Model underway replenishment for extended-range deep penetration strikes
  - [ ] Fast Transports (APD): Model high-speed surprise raid/insertion/extraction missions (no assault bonus)
  - [ ] Mine Tenders (ACM): Model 150-mine harbor preservation when disbanded with fuel & <10% damage
  - [ ] Aircraft Transports (AKV): Model operational uncrated airframe transfer
  - [ ] Coastal Sweepers (AMc): Model local 1-hex harbor clearance
- [ ] Ground Predictive Models (A, B, D, F, G) and tactical reasoning disclosures on vessel capabilities (§6.2, §6.3; Changelog 2026-09-21 13:27 UTC+3)
- [ ] Ingest Naval Classification & Auxiliary Doctrine into Field Manual JSON (`assets/manual/witp_ae_manual.json` sections 1.15 and 1.16) (§6.2, §6.3; Changelog 2026-09-21 13:27 UTC+3)

**War Room Predictive Intelligence Sub-Menu (Ozgur directive 2026-09-20 17:07 UTC+3):**

- [ ] **[SUB-MENU]** Implement dedicated Predictive Intelligence sub-menu entry inside the War Room — navigation home for all analytical model results (Changelog 2026-09-20 17:07 UTC+3)

**Model A — Amphibious Assault Probability Engine:**

- [ ] **[MODEL A]** Implement Weighted Evidence Accumulator scoring per target hex (Changelog 2026-09-20 17:07 UTC+3 — §7.2 Module A)
  - [ ] Parse SIGINT radio volume lines: `Heavy Volume` = weight 3, `Radio transmissions detected` = weight 1 (§7.2 Module A)
  - [ ] Parse SIGINT embarkation lines: unit loaded on ship moving to destination → `+5` per SNLF/amphib unit (§7.2 Module A)
  - [ ] Parse transport-class ship (xAK/xAP/AK/AKL) movements to target hex → `+2` per ship (§7.2 Module A)
  - [ ] Parse Combat Events amphibious TF confirmed unloading at target → `+10` (§7.2 Module A)
  - [ ] Apply recency decay: `×0.9^Δt` (turns since last signal) to all accumulated weights (§7.2 Module A)
  - [ ] Flag target as `>90% probability` when total score ≥ 15 (§7.2 Module A)
  - [ ] Display partial/degraded predictions from turn 1 with a `[CONFIDENCE: LOW — N turns of data]` caveat (Q7 2026-09-19; §7 intro)

**Model B — Fleet Trajectory & Intercept Projection:**

- [ ] **[MODEL B]** Implement Dead Reckoning vector projection per tracked TF over multi-sighting sequences (Changelog 2026-09-20 17:07 UTC+3 — §7.2 Module B)
  - [ ] Parse sighting report lines: extract hex (x,y), speed, cardinal/relative heading, ship count (§7.2 Module B)
  - [ ] Accumulate sightings per TF across turns; associate by proximity/heading continuity (§7.2 Module B)
  - [ ] Fit heading vector using least-squares on (Δx, Δy) across sightings (§7.2 Module B)
  - [ ] Project T+1 position: `(xₙ + speed × cos(θ), yₙ + speed × sin(θ))` (§7.2 Module B)
  - [ ] Compute confidence radius: `r = base_radius + k × (T_now − T_last)` expanding with time since last sighting (§7.2 Module B)
  - [ ] Cross-reference projected radius against known IJN bases/ports → list candidate destinations (§7.2 Module B)
  - [ ] Mature into 5-step Markov transition matrix as sightings accumulate (§7.3 Phase 3, FAQ 2026-09-15)

**Model F — Strike Concentration Detector:**

- [ ] **[MODEL F]** Implement hex grid clustering scan per turn (Changelog 2026-09-20 17:07 UTC+3 — §4.5 Strike Concentrations)
  - [ ] Collect all warship sighting coordinates for the current turn from Ops Report and Combat Events (§4.5)
  - [ ] For each candidate center hex C: sum all warship-class ships within radius (default 10 hex, configurable via Settings) (§4.5)
  - [ ] Count cruiser-class or larger ships (CA, BB, CV, BC) within that radius (§4.5)
  - [ ] Flag Strike Concentration when cruiser-class count ≥ threshold (default 3, configurable via Settings) (§4.5; Q A2 2026-09-19)
  - [ ] Display concentration label, center hex, and ship count summary in the sub-menu row (§4.5)

**Model G — Established Shipping Routes:**

- [ ] **[MODEL G]** Implement repeated waypoint graph for shipping route inference (Changelog 2026-09-20 17:07 UTC+3 — §4.5 Established Routes)
  - [ ] Track per-merchant-vessel sighting list across turns: `waypoints[vessel] = [(t, x, y) …]` ordered by turn (§4.5)
  - [ ] On ≥ 2 sightings for a vessel: infer direction vector and identify nearest named port at each endpoint (§4.5)
  - [ ] Assign confidence: `min(1.0, sightings / 5)` displayed as `[LOW/MED/HIGH]` (§4.5)
  - [ ] Display route entry: `"Route: [Origin port] → [Destination port] | [Vessel type] | [N] sightings | Conf: [LEVEL]"` (§4.5)
  - [ ] Flag routes with ≥ 3 sightings as Established for Trade Route Interdiction alerts (§7.1)

**Data readiness and display rules (all models):**

- [ ] All models feed from text report vault (SIGINT, Operations, Combat Events, Combat Report) from turn 1; no binary gate on display (Q7 2026-09-19; Changelog 2026-09-20 17:07 UTC+3)
- [ ] Where binary `.pws` data is not yet decoded, display output with `[ESTIMATED]` or `[CONFIDENCE: LOW]` caveat rather than suppressing the row (Changelog 2026-09-20 17:07 UTC+3 data readiness rule)
- [ ] Recalculate all models automatically on each campaign load / incremental ingest (§4.1; Changelog 2026-09-20 14:15 UTC+3)

**Tab 1 — Intelligence Summary:**

- [ ] High-Value Targets: track detected enemy carriers, capital ships, tankers/oilers, troop transports (§4.5)
- [ ] Strike Concentrations: flag groups of three or more warships (cruiser-class or larger) within ten hexes (configurable threshold) — powered by Model F (§4.5)
- [ ] Port Concentrations: track warships and merchant vessels docked at named ports (§4.5)
- [ ] Established Routes: identify merchant and tanker transit corridors from repeated sightings; infer departure and destination — powered by Model G (§4.5)

**Tab 2 — SIGINT Map:**
 
 - [ ] Display SIGINT map as structured textual/tabular list of localized hex coordinates with distance & sector bearings relative to friendly bases/HQs (§4.5; arch-decision §18, Q C1 2026-09-19)
 - [ ] Allow stepping backward and forward through historical turn data (§4.5)
 - [ ] Alert Feed (left): prioritized real-time intelligence warnings (§4.5)
 - [ ] Dossier Panel (right): target search with compiled intel (strength estimates, history) for base, vessel, unit, air group, or city via unified free-text search (§4.5; arch-decision §10, Q6 2026-09-19)
 - [ ] Store and integrate authoritative master theater hex map image asset (origin 0,0) for terrain masking and CP437 sector minimap overlays (§4.5, §6.5, §15)
 - [ ] Maritime Obstacle Avoidance & Nautical Pathfinding: implement A*/Dijkstra binary land/sea hex collision masking for Model B fleet trajectory extrapolation (§6.5, §7.2 Module B)
 - [ ] Strategic Chokepoint & Convoy Interdiction: model mandatory strait crossings (Malacca, Sunda, Makassar, Surigao, San Bernardino, Tsushima) in Model G for submarine ambush windows (§6.5, §7.2 Module G)
 - [ ] In-Terminal Tactical Mini-Radar / Sector Map: render a 40x20 CP437 ASCII/ANSI sector map in Tab 2 centered on selected sighting with coastline, friendly ports, and contact vector (§4.5, §6.5)
 - [ ] Land-Based Air Search & Strike Umbrellas: calculate concentric air range rings (3-hex torpedo, 5-hex bomber, 8-hex search) for Models A & F (§6.5, §7.2)
 - [ ] Amphibious Beachhead Approach Lanes: combine target base coordinates with coastal approach vectors for assault echelons (AP/APA/LST) in Model A (§6.5, §7.2 Module A)
 - [ ] Hex Grid Calibration & Coordinate Ground-Truth: cross-reference doubtful PBEM reports and .pws coordinates against visual ground truth (§6.5, §15)

**Tab 3 — Theater Predictive Intelligence Engine (TPIE):**

- [ ] High-priority warning dashboard: invasion alerts, fleet departures, predicted enemy logistical behaviors over campaign lifetime — powered by Models A, B, D (§4.5)
- [ ] Strategic Foresight: predict amphibious targets and air strike focal points 3–5 days in advance — powered by Models A, B (§4.5)
- [ ] Actionable SIGINT: convert raw intelligence (radio volume, transport manifests) into high-confidence invasion probability display — powered by Model A (§4.5)

---

## §5 — Ingested data sources

- [x] Parse Signals Intelligence (SIGINT): plain-text per side per turn (e.g., `asigint_411207.txt`) (§5)
- [x] Parse Operational Reports: plain-text operational summaries per side per turn (§5)
- [x] Parse Combat Reports: shared plain-text battle output (§5)
- [x] Parse Combat Event Logs: shared naming structure with player-side-specific event entries (§5)
- [ ] Parse Game Save State (`.pws`): unlock via user password; complete underlying turn telemetry (§5)
- [ ] Store all parsed telemetry and analytical models securely within the encrypted Campaign Vault File (§5)

---

## §6 — Intelligence modeling and fog of war

- [ ] Process data through a dynamic Belief Model (not raw text as objective truth) (§6)
- [ ] Allied SIGINT: high-confidence/certain by default (user-configurable) (§6)
- [ ] Japanese SIGINT: probable intelligence requiring secondary verification (§6)
- [ ] Own-Force Operational Reports: absolute fact for friendly units (§6)
- [ ] Shared Combat Reports: healthy skepticism; store BDA as claims with dynamic confidence as turns corroborate or contradict (§6)
- [ ] Parse only the user's native files so opponent intelligence states diverge correctly per side (§6)
- [ ] Implement 4-phase deterministic numerical confidence level derivation engine (Base Score, Corroboration Bonus, Time Decay, Tier categorization) (§6.1)
- [ ] Ingest Naval Unit Capability Registry across 7 functional classes and 40+ hull codes (§6.2)
- [ ] Ingest Mobile Auxiliary & Tender Doctrine (AS, AD, AV/AVD/AVP, AR, ARD, AE/AKE, AO, APD, ACM, AKV, AMc) (§6.3)
- [ ] Ground predictive models (A, B, D, F, G) and tactical reasoning pop-ups on naval unit capabilities and auxiliary roles (§6.2, §6.3)
- [ ] Ingest Authoritative Base Location Registry across 1,330 bases and 71 regions with (X, Y) hex coordinates (§6.4)
- [ ] Enforce maritime housing restrictions (only coastal bases house ships) (§6.4)
- [ ] Track dynamic base ownership and controller mutation upon capture events (§6.4)
- [ ] Ground predictive models (A, B, D), Tab 2 bearings, and Dossier entity search in Authoritative Base Location Registry (§6.4)

---

## §7 — Theater Predictive Intelligence Engine (TPIE)

### §7.1 — Initial turn capabilities and ongoing processing

- [ ] Invasion Warnings: early detection (baseline example Legaspi and Vigan from Allied embarkation intercepts) (§7.1)
- [ ] Landing Focus: confirmed amphibious concentration (baseline example Kota Bharu linking sightings, BDA, named vessels) (§7.1)
- [ ] Unseen Force Detection: localize masked task forces via elevated radio traffic (baseline example Hex 90,96) (§7.1)
- [ ] Logistical Tracking: identify auxiliary vessels (baseline example Midway refueling tanker) for replenishment routes (§7.1)
- [ ] Trade Route Interdiction: correlate repeat merchant sightings (baseline example Ryuyo Maru) into trackable supply lines (§7.1)
- [ ] Subsequent turns: recurring enemy behavioral predictions (e.g., recon over base every N turns; next expected turn) (§7.1)
- [ ] Subsequent turns: course change alerts (§7.1)
- [ ] Subsequent turns: automated claims adjustment (§7.1)
- [ ] Ingest 30+ days of continuous historical logs for forecasting (§7 intro, §7.1 context)

### §7.2 — Core predictive intelligence modules

**Module A — Amphibious Assault & Target Probability Engine:**

- [ ] Use SIGINT (transport loadouts, radio volume) and Operational Reports (TF movements) (§7.2 Module A)
- [ ] Combine unit loadout tracking with high-volume radio intercepts at staging bases for Target Invasion Probability Score (§7.2 Module A)
- [ ] Flag high-probability targets prior to assault (baseline examples Batan Island, Legaspi, Kota Bharu >90%) (§7.2 Module A)

**Module B — Enemy Fleet Trajectory & Intercept Modeling:**

- [ ] Use Operational Reports (sightings, TF speeds/headings) and Combat Events (shadowing alerts) (§7.2 Module B)
- [ ] Apply time-series analysis to fragmented sightings to project vector, speed, target destination (§7.2 Module B)
- [ ] Triangulate multi-day patrol sightings for strike force and invasion fleet trajectories (§7.2 Module B)

### §7.3 — Technical implementation pipeline

**Phase 1 — ETL & data ingestion:**

- [ ] Log parsing (regex & NLP): custom regex extracting entities across raw text logs (§7.3 Phase 1)
- [ ] Entity extraction: Task Force IDs, hex (X,Y), unit names, vessel classes, speed, heading, weather, event types (§7.3 Phase 1)
- [ ] Contextual NLP: classify entries (e.g., "snooped by" vs "shadowed by" vs "detects") to weight confidence (§7.3 Phase 1)
- [ ] Data normalization: normalize map grid coordinates into in-process hex-spatial structures for geospatial querying; serialize into Campaign File (§7.3 Phase 1; Changelog 2026-09-15 15:37 UTC — no PostGIS)

**Phase 2 — Data modeling & storage:**

- [ ] In-process graph model: map entity relationships (Units, Task Forces, Transports, Bases) for operational lineage; serialize into Campaign File (§7.3 Phase 2; Changelog 2026-09-15 15:37 UTC — no Neo4j)
- [ ] In-process time-series model: log daily operational trends per turn (fuel stocks, supply decay, fort levels, air pools); serialize into Campaign File (§7.3 Phase 2; Changelog 2026-09-15 15:37 UTC — no InfluxDB)

**Phase 3 — Predictive analytics & machine learning:**

- [ ] Markov Chain Models: 5-step destination probability matrix from TF position, vector, historical habits (§7.3 Phase 3)
- [ ] Survival Analysis (Kaplan-Meier): model base time-to-capture from bombardment density, fort degradation, garrison strength (§7.3 Phase 3)
- [ ] Heuristic Rule Engine: real-time alerts from SIGINT conditions (e.g., heavy radio volume + SNLF manifest → invasion alert) (§7.3 Phase 3)

### §7.4 — Project deliverables and milestones

**Milestone 1 — Parser & Core Dashboard (Weeks 1–3):**

- [ ] Deliver ETL pipeline ingesting the five core file types (§7.4 Milestone 1)
- [ ] Provide base UI dashboard showing parsed unit locations and movement histories (§7.4 Milestone 1)

**Milestone 2 — SIGINT & Target Predictor (Weeks 4–6):**

- [ ] Deploy Amphibious Target Probability Engine (§7.4 Milestone 2)
- [ ] Output daily Threat Matrix: top likely targets from transport manifests and radio traffic (§7.4 Milestone 2)

**Milestone 3 — Fleet Trajectory & Attrition Models (Weeks 7–10):**

- [ ] Integrate spatial movement algorithms and naval/air attrition models (§7.4 Milestone 3)
- [ ] Output Intercept Windows projecting optimal patrol zones to interdict enemy convoys (§7.4 Milestone 3)

**Milestone 4 — Full System Integration & Companion API (Weeks 11–12):**

- [ ] Package engine into local companion app reading save states in real-time (§7.4 Milestone 4)
- [ ] Render predictive overlays on the strategic map (§7.4 Milestone 4)

---

## §8 — Text User Interface (TUI) standards

**Traceability (Changelog 2026-09-15 ~15:44 UTC / ~15:47 UTC — Ozgur):** v1 UI aesthetic target is Alien ship-computer green phosphor per spec §8 (codes 2, 10, 160; header/body/footer per §8.2–§8.4 until scope changes them). **On-screen display name:** **WU/TH/UR** (decided ~15:47 UTC; closes OPEN “W.U.T.H.U.R.” vs “MUTHUR”). No reference images; do not invent splash taglines. Do not treat film frames as assets. **Do not check off §8 items below from changelog entries alone.**

### §8.1 — Color palette (Win32 console codes)

- [ ] Dark Green (Code 2): borders, panel framing, inactive text, secondary details (§8.1)
- [ ] Light Green (Code 10): headers, titles, active prompts, status, numerical data (§8.1)
- [ ] Inverted Highlight (Code 160): black on light green strictly for selected navigation elements (§8.1)
- [ ] Red / platform default: access-denied or error flashes with audio alert beep (§8.1)

### §8.2 — Standard screen topography

- [ ] Header block: full width, double-line `=`, `[ {SCREEN TITLE} ] Turn: {TURN_DATE}`, no `{INDEX}` (§8.2 A; arch-decision §14, Q A1 2026-09-19)
- [ ] Body: sections separated by `-`; bracketed section titles; data indent 2–4 spaces per spec template (§8.2 B)
- [ ] Nostromo typography: wide letter-spaced section headers and dual-column parameter/value matrix styling (Changelog 2026-09-19 19:44 UTC+3 item 9)
- [ ] Footer: hotkey legend composed **dynamically per screen** — show only hotkeys active on that screen; no hardcoded footer string (§8.2 C; arch-decision §12, Q2 2026-09-19)

### §8.3 — Interaction rules

- [ ] Selection navigation: UP ARROW, DOWN ARROW, TAB only (§8.3)
- [ ] Instant hotkeys: bracketed letters execute immediately without list scrolling (§8.3)
- [ ] Cursor rendering: move console cursor and re-print with inverted styling (§8.3)
- [ ] Cursor navigation styling: solid underline bar directly spanning active parameter and value like in the Nostromo screen (Changelog 2026-09-19 19:44 UTC+3 items 8 & 9)
- [ ] Audio feedback — navigation click: each selection movement is accompanied by a terminal mechanical click sound (Changelog 2026-09-19 19:44 UTC+3 item 8)
- [ ] Audio feedback — page transition: each screen transition/page load triggers a retro teletype terminal chirp/chime sound (Changelog 2026-09-19 19:44 UTC+3 item 8)
- [ ] Audio feedback — ambient transition static: play random short bursts of user-provided Nostromo radio static (`assets/audio/alien_ambient_transition.wav`) during screen transitions (Changelog 2026-09-19 19:44 UTC+3 item 10)
- [ ] Blinking feedback: modal prompts and waiting screens use 1.0 Hz sleep loop toggling colors (§8.3)

### §8.4 — Modal overlays

- [ ] Intelligence inspect: centered pop-up over active buffer per spec layout (title, rationale word-wrapped ~65 chars, blinking ENTER to return) (§8.4)

---

## §9 — Platform architecture and deployment

- [ ] Ship Version 1 for Microsoft Windows first; default install `C:\WitP-CIC` (§9, §3; Changelog 2026-09-15 15:37 UTC — supersedes §1 Mac-first for v1)
- [ ] Deliver primary artifact as Windows console `.exe` built from C++ (Changelog 2026-09-15 15:37 UTC)
- [ ] Plan Linux distribution testing for Wine/Proton compatibility layers (§9)

---

## §10 — Version 1 scope boundary

**In scope (implement for v1.0):**

- [ ] Deliver all core capabilities in Sections 4 through 8 (§10)
- [ ] Build manual/automated test suites against golden game reference files (§10)

**Out of scope (do not implement in v1.0):**

- [ ] Defer detailed surface combat log parsing to v1.1+ (§10)
- [ ] Defer dynamic global weather extraction to v1.1+ (§10)
- [ ] Defer rendering individual ship tactical map icons to v1.1+ (§10)
- [ ] Defer cloud synchronization or multi-user features to v1.1+ (§10)

---

## §11 — Quality assurance and acceptance standards

- [ ] Parse all report and `.pws` file types into structured data models (§11)
- [ ] Maintain permanent baseline golden reference files (§11)
- [ ] Every build release: run automated validation suites reproducing target counts (report totals, sinkings, warning indicators) across synthetic multi-turn scenarios (§11)
- [ ] Strictly increment build number by 1 on each new release executable compilation adhering to witp-cic-[mainversion id]-[phaseid]-[built no] (§1.13, §14)
- [ ] Execute rigorous manual QA checklist: cross-platform installer paths, setup wizard flows, dashboard interaction states (§11)
- [ ] Release gate: approve deployment only if all automated tests pass 100% and unparsed line rates remain strictly below 1.0% (§11)

---

## §13 — Error handling and documentation standards

- [ ] Format all application errors as strict 3-digit codes `[XXX]` (e.g., `[101]`, `[404]`) (§13)
- [ ] Log every generated error code in external developer documentation with full definition (§13)
- [ ] Wiki: dedicated section listing all error codes and user resolutions (§13)

---

## §1–§2 — Product principles (read-only, local, PBEM)

- [ ] Keep product strictly read-only: never touch the game, never decide for the player, never communicate externally (§1)
- [ ] Keep all data on the player's own computer (§1)
- [ ] Support PBEM workflow: ingest turn-end outputs from multiplayer campaigns (§2, FAQ 2026-09-15)

---

## FAQ 2026-09-15 — Additional stated requirements

- [ ] Normalize time-series values per turn (FAQ 2026-09-15)
- [ ] Implement time-series storage and retrospective analysis in witp-cic (FAQ 2026-09-15)
- [ ] Use actual game output files as golden format reference; format consistent across campaigns (FAQ 2026-09-15)
- [ ] Create test cases for Ozgur approval before locking automated tests (FAQ 2026-09-15)
- [ ] Do not support real-time game state updates (FAQ 2026-09-15)
- [ ] Update UI/log stream discretely as each PBEM turn is played (FAQ 2026-09-15)
- [ ] Intercept Windows: implement projection from known trajectories and patrol zones first; support user-defined constraints as spec asks both (FAQ 2026-09-15)
- [ ] Remain strictly single-player/local-only PBEM helper (FAQ 2026-09-15)
- [ ] Markov chain inputs include heading and speed; use 5-step transition approach per §7.3 (FAQ 2026-09-15)
- [ ] For Kaplan-Meier and Markov model choices: perform comparative analysis and select approach (FAQ 2026-09-15)
- [ ] Bombardment density for survival analysis: incorporate strikes, average tonnage, weighted probabilities from attack records and other derivable signals (FAQ 2026-09-15)
- [ ] Alerts: rule-based engine; SIGINT may feed rule conditions (FAQ 2026-09-15)

---

## Changelog 2026-09-15 — Scope adoption

- [ ] Treat `docs/scope-v1.md` as living design spec bible; log additions with date/time in changelog (Changelog 2026-09-15)

---

## Decided — Changelog 2026-09-15 15:37 UTC (Ozgur + agent)

Former spec contradictions; implement only these paths. See `docs/architecture-decisions.md` and spec changelog.

- [x] **Platform v1:** Windows first; default install `C:\WitP-CIC` (supersedes §1 Mac-first for v1).
- [x] **Vault folder:** `\Campaigns` only (`C:\WitP-CIC\Campaigns`); `\Campaign Vaults` in §4.1 is a synonym error.
- [x] **Primary language:** C++ → Windows console `.exe`; Win32 console TUI per §8.1 codes 2, 10, 160 (Ozgur: “U decide. I need an exe file”).
- [x] **Storage:** One compressed Campaign File in `\Campaigns`; graph / time-series / hex-spatial models in-process, serialized into that file; no Neo4j, InfluxDB, PostGIS services or data directories (Ozgur: “U decide”; rationale §3).
- [x] **PBEM save extension:** **`.pws`** for WITP-CIC detection and file matching (Changelog 2026-09-15 15:43 UTC; spec §5; golden `wpae023.pws`). Manual §20.4.1 / §20.5.4 **`.wps`** string is manual wording only — superseded for product file matching.
- [x] **TUI on-screen name:** **WU/TH/UR** (Changelog 2026-09-15 ~15:47 UTC; closes OPEN “W.U.T.H.U.R.” vs “MUTHUR”). §8 header/body/footer layout as in spec until scope changes; no reference images.

---

## WAITING — Blocked on Ozgur or external access

From spec § “Still waiting (not in this document)”:

- [x] **Plain-text golden samples:** received (2026-09-15); 12 files in `docs/golden/` (`docs/golden/INDEX.md`).
- [x] **`.pws` golden set (2026-09-19):** six `wpae021` (JP → Allied) + `wpae023` (Allied → JP outgoing). Knowledge index: `docs/pws-knowledge-summary.md`. Container parsed (gzip + 61 zlib records); ship tombstone fields evidence-gated in `docs/pws-field-map.md`. **Implement:** container probe without zip/password gate; defer live-ship hex until proven.
- [x] **Spec §5 “password unlock”:** superseded for on-disk format on golden set — no file password; in-game PBEM password may still apply in WITP UI only.
- [ ] GitHub private repo **WITP-CIC-C** (blocked on GitHub authentication and owner login).

**Closed questions (Ozgur 2026-09-15 ~15:40 UTC — no code action):**

- ~~Are tiny Japanese `jsigint_*` / `joperationsreport_*` incomplete samples?~~ **Closed:** real fog of war (header-only SIGINT; ops header + Batan Island); not truncated files.
- ~~Are `Combat_Events_*` / `combatreport_*` duplicated per side?~~ **Closed:** shared by filename; content may delta via fog of war; golden drop has one instance per filename/date—do not invent a second side’s copy.

---
