# WITP-CIC — screens and milestones

What Ozgur can do in the Windows exe **today** vs what [scope v1](scope-v1.md) still requires.  
Checked = in the running skeleton or validated parser tests. Unchecked = not in the product yet.

**How to read this**

- **Skeleton** means the screen exists so you can navigate; it does **not** mean the spec feature is done.
- Parser ingest is in the `.wic` vault; War Room reads vault-derived rows (Tab 1–3 heuristics; full §7 TPIE engine still open).
- Source of UI facts: scaffold on branch `cursor/witp-cic-scaffold-18a3` (`internal/scaffold-pass-ui-questions.md`). Source of parser facts: [todo](todo.md) §5 checkmarks + golden tests. This file was not re-diffed against `src/` from Origin in this pass (clone from the coordinator VM returned 403).

**Current Active Milestone:** **Alpha 4 (Phase `alpha4` - Build 12)** (2026-09-26 18:45 UTC+3)  
- Release Executable: `dist/witp-cic-muthur-alpha4-12.exe` (Single authoritative versioned binary; zero mirrors per ADR 63 / LL-049)  
- Naming Standard: `witp-cic-muthur-alpha4-[build#].exe` (build counter increments by 1 on each new compilation)  
- Active Workspace: `C:\Users\ozgur\Documents\Witp-CIC-MUTHUR`  
- Active GitHub Remote: `https://github.com/TheZenHippie/MUTHUR6000.git` (branch `main`)  
- Standalone Recovery Archive: `backups/WITP-CIC2-RECOVERY-POINT-alpha4-build12.zip` (86.48 MB)  
- 106-Column Box Expansion, Gap Elimination, and Live Analytics Wiring (ADR 77, LL-060):
  1. 106-Column Monospace Box Expansion: Expanded modal and pop-up box widths in `BuildHeuristicsPopUp`, `BuildNavigablePopUp`, and `BuildReasoningModal` from 78 to 106 columns (`boxWidth = 106`, `innerWidth = 102`). Long intelligence sighting strings up to 102 characters display without truncation or clipping. Header titles clamped at 90 characters.
  2. Gap Elimination & Visual Scaling: Adjusted `RochefortColumn.Width = 0.44*` and elevated `panelFontFactor` to `0.90`, making text boxes occupy ~1.5x more visual width and completely eliminating the empty dead space between the main text body and the advisor panel.
  3. Live Analytics Grounding in Advisor Speech: Dynamic ingestion of live target line, confidence percentage (`[RADAR/RDF CORROBORATED]` or `[IMPERIAL AIR RECON VERIFIED]`), and signal evidence bullets from `_popUpReasonings`.
  4. Faction-Specific Intelligence Advisor: Allied player views Station Hypo Cmdr. Joseph Rochefort (`CMDR. J. ROCHEFORT`, Station Hypo portrait, decrypt telemetry); Japanese player views Combined Fleet Chief of Staff Vice Admiral Matome Ugaki (`V.ADM. M. UGAKI`, authentic dithered Ugaki portrait, *Senkoroku* war diary notes, *Kantoku* decisive battle doctrine). Rochefort window never appears for a Japanese player.
- Box Boundary Invariant Rigidity, Proportional Width Expansion (~1.5x) & Faction-Specific Intelligence Advisor (ADR 76, LL-059):
  1. Box Boundary Invariant: Strict character cell clamping on popup header titles (`shortTitle.Length <= 59`) so padding `rem >= 1` always; inner bullet rows clamped to `innerWidth` with vertical right border `║` strictly at column 78 across all screens.
  2. Proportional Width Expansion & Gap Elimination: Expanded `RochefortColumn` to `0.48*` and increased `panelFontFactor` to `0.88`, enlarging terminal text boxes by ~1.5x in visual width and eliminating the horizontal dead space gap between the terminal and advisor panels without soft-wrapping.
  3. Faction-Specific Intelligence Advisor: Allied player views Station Hypo Cmdr. Joseph Rochefort (`CMDR. J. ROCHEFORT`, Station Hypo portrait, JN-25 decrypts & SIGINT brief); Japanese player views Combined Fleet Chief of Staff Vice Admiral Matome Ugaki (`V.ADM. M. UGAKI`, authentic dithered Ugaki portrait, *Senkoroku* war diary notes, *Kantoku* decisive battle doctrine). Rochefort window never appears for a Japanese player.
- Cmdr. Rochefort Intelligence Panel & Option C Dynamic Terminal Scaling (ADR 75, LL-058): Dual-box right side panel displaying Cmdr. Joseph Rochefort portrait ([BOX 1]) and live Station Hypo intelligence brief ([BOX 2]) wired across all analytical pop-up and reasoning modal screens (`ReasoningModal`, `HeuristicsPopUp`, `IntelDetailPopUp`, `PredModelPopUp`).
- System Identity Rebranding & Diagnostic Boot Screen Redesign (ADR 74, LL-057): Rebranded system identity to **ROCHEFORT 6000** across UI window titles, system identity badges, baud-rate menu labels, context menus, tooltips, font selector windows, and automated test runners. Implemented redesigned Diagnostic Boot Screen featuring dynamic heuristic (`>> 6 HEURISTICS READY`) and graphics counts (`>> 10 GRAPHIC SCREENS READY`), removal of legacy telemetry harvest text, and high-impact 6-line block ASCII banner.  
- Complete Graphics Sub-Menu TBD Standardization (ADR 73): All 10 slots `[1]` through `[10]` strictly rendered as `[ TBD ]` routing to standardized full-screen reserved partition screens with rigid straight borders (`boxWidth = 78`).
- Proportional Dynamic Font & Chrome Scaling System (ADR 71): Terminal font size and line height scale dynamically with window resolution. Header bar badges, phosphor quick-switch buttons, padding, and footer telemetry status/resolution dropdown scale proportionally to maintain 100% visual balance across all resolutions (up to 2560x1440 QHD). Included window Maximize/Restore button `🗖`/`🗗` and `F11` fullscreen key handling.
- In-Terminal Unicode Character Graphing (ADR 64): Monospace delta bars `[►████░░░░]`, hit markers `──■──`, contact density meters `[■■■···]`, drift vector velocity arrows `[==►  ]`, gap sensors `[░░░░░░ GAP 0%]`, and OPTEMPO histograms `[████░░░░░░░░░░░░]`.
- Rigid Box Monospace Alignment: 100% straight right border guarantee with zero line wobbling or bending.
- Header Phosphor Relocation: Phosphor quick-switch buttons positioned in top draggable window header adjacent to CIC matrix badge.
- War Room Heuristics & The 6 Logical Blueprints: Dedicated analytical domain option `[6]` in War Room Primary Screen covering Metric 1 (Regional Power Shift across 10 regions), Metric 4 (Minefield Mapping), Metric 5 (Submarine Migration daily centroids), Metric 46 (Intelligence Blindspots), Metric 49 (Axis of Advance & Strategic Drift), and Metric 50 (Operational Tempo 3D-Avg). Includes temporal guard (< 4 days) and drill-down raw telemetry verification modals (2026-09-22 19:35 UTC+3).  
- Turn Count Disambiguation & Multi-Day Ingestion: Pop-up headers show `Turn: <Date> (<N> Days Loaded) | ITEMS: <X>`; verified all 8 operational days in `D1.wic` across 60 files fully processed into models and sighting logs (2026-09-21 18:54 UTC+3).  
- Naval Capabilities Integration: 7 functional classes & 40+ hull codes, Auxiliary / Tender Doctrine wired into Field Manual `1.15`/`1.16` and predictive models (2026-09-21 13:27 UTC+3).  
- Base Location Registry Integration: 1,330 authentic bases across 71 regions with $(X, Y)$ coordinates, maritime housing constraints, dynamic controller mutations, and predictive models grounding (2026-09-21 14:45 UTC+3).  
- Master Hex Map & Geospatial Applications: Official theater hex map (origin 0,0) powering 6 geospatial applications (Model B obstacle avoidance, Model G strait chokepoints, Tab 2 CP437 minimap, Models A & F air umbrellas, Model A approach lanes, hex calibration) (2026-09-21 15:05 UTC+3).  
- Opponent-Only Intelligence Display: War Room BDA and Model E claims register filter strictly for enemy assets; friendly losses excluded (2026-09-21 18:15 UTC+3).  
- Bold Headers & Program-Wide Text-Wrapping: Screen headers and modal titles rendered in BOLD Light Green; zero text cutoff; multi-line wrapping in modal titles and bodies; screen clearing on modal launch; off-by-one line drop bug resolved (2026-09-21 18:27 UTC+3).  
- Hierarchical Reasoning Modal Drill-Down: Interactive 2-tier modal navigation; unified Telemetry Source & Localization History drill-down revealing detection means, observation frequency, and chronology; on-demand confidence computation drill-down; program-wide text cutoff elimination via innerWidth wrapping (2026-09-21 18:40 UTC+3).  

**Previous Preserved Baselines:**  
- **Alpha 2 Build 5 Recovery Point:** Tag: `checkpoint-pre-major-addition-alpha2-build5` | Branch: `recovery-point-alpha2-build5` | Archive: `backups/WITP-CIC2-RECOVERY-POINT-alpha2-build5.zip` (See [`docs/recovery-point-alpha2-build5.md`](recovery-point-alpha2-build5.md))  
- **Version 0.02 (Stable Alpha 1):** Tags: `v0.02`, `v0.02-alpha.1` | Branch: `stable-alpha-1` | Archive: `backups/WITP-CIC2-v0.02-alpha1.zip` (See [`docs/how-to-rollback-to-alpha-1.md`](how-to-rollback-to-alpha-1.md))  

**Runtime note (2026-09-22):** Standalone executable `dist/witp-cic-muthur-alpha4-4.exe`. Continuous Unicode box frames program-wide. Mathematical evidence pop-up transparency, bottom-ribbon resolution dropdown (1280x720 HD, 720p, Full HD, Standard), header Phosphor quick-switch pill buttons, and in-terminal Unicode graphs. Vault files: `C:\WitP-CIC\Campaigns\*.wic`.

---

## Page: Splash

- [x] Show splash on open (§4.1) — **skeleton**; on-screen name **WU/TH/UR**
- [x] Blink wait (~1 Hz) with **no** wait-label text (provisional; spec §8.3)
- [ ] Full §8 header/body/footer splash layout (index, turn date, spec templates)
- [ ] Invented taglines — **forbidden**; not a milestone

---

## Page: New or Continue

- [x] Prompt: New Campaign vs Continue a Current Campaign (§4.1)
- [x] Arrow / selection navigation to those two items

### New Campaign wizard

- [x] Ask which side the player commands
- [x] Prompt for `.pws` password (stored; **not** validated unlock)
- [x] Find the game reports folder (auto-search; `[101]` if missing)
- [x] Confirm turn length
- [ ] Store PBEM password for in-game use only — on-disk golden `.pws` needs no file unlock (2026-09-19)
- [x] Ask for campaign vault file name
- [x] Create vault only under `C:\WitP-CIC\Campaigns` (`.wic`)
- [x] Remember game campaign file locations
- [x] Ingest SIGINT, ops, combat report, combat events into the vault
- [ ] Ingest `.pws` telemetry (probe only today)
- [x] Then go to Master Menu after vault exists

### Continue campaign

- [x] List available campaign master files (`.wic` in `\Campaigns`)
- [x] Load selected vault
- [x] Verify stored paths
- [x] Ingest new game text outputs using saved paths; `.pws` still not unlocked

---

## Page: Master Menu

- [x] Show `[A] War Room` (§4.2) — **three-tab dashboard**
- [x] Show `[W] Wiki` — **label only / stub destination**
- [x] Show `[S] Settings` — **label only / stub destination**
- [x] Show `[E] Exit`
- [x] Hotkeys A / W / S / E — **skeleton**
- [x] Arrow-key traverse — **skeleton**
- [x] ESC goes back one screen — **skeleton**
- [ ] §8 header with `{INDEX}` and `{TURN_DATE}` (omitted until specified)
- [x] War Room operational (three tabs; Wiki/Settings still stubs)

---

## Page: War Room (2026-09-19)

### Tab 1 — Intelligence Summary

- [x] Scrollable vault-derived rows in sections (WU/TH/UR TUI; Master Menu **A**; ESC back)
- [x] High-value mentions (heuristic on ingested SIGINT/combat text: CV/BB+, tanker, AK, troop loads)
- [x] Strike concentrations (aggregate `N Japanese ships at hex` + capital markers within 10 hexes from combat events/ops text)
- [x] Port concentrations (2+ vessel `arrives at` lines grouped by port name in events/ops)
- [x] Established routes (2+ SIGINT `moving to` same destination)
- [x] Battle claims corrections (`returned to service` / repair lines in operational reports)
- [x] Ghost fleet counter (heuristic name count vs hex-localized markers; not full order of battle)
- [x] Sinkings / battle claims from combat report + combat events in vault
- [x] SIGINT locator rows (coordinates from ingested SIGINT)
- [x] ENTER on a row → intelligence modal overlay (§4.5 / §8.4)

### Tab 2 — SIGINT Map

- [x] Tab body: scaled console map from SIGINT hex markers for selected turn
- [x] Step backward / forward by turn (`[` / `]` when vault has multiple date tokens)
- [x] Alert feed (left) — heavy/radio SIGINT lines for active turn
- [x] Dossier panel (right) — scrollable hex-linked SIGINT rows; ENTER modal
- [x] Event log (bottom) — combat events lines for active turn (or vault fallback)
- [ ] Strategic planning map artwork / concentration radii (console dots only today)

### Tab 3 — TPIE

- [x] Tab body: invasion/amphib heuristic rows from SIGINT troop loads; fleet/logistics lines from ops/events
- [x] Labeled stub for §7 heatmaps / 3–5 day foresight (not computed)
- [ ] Full TPIE engine (§7 modules A–E)

---

## Page: Wiki — not implemented (menu stub)

- [ ] Hierarchical help by section headers
- [ ] Sub-page per topic
- [ ] Error-code section `[XXX]` (§13)
- [ ] In-app use of `docs/witpae-manual.txt` as Wiki source
- [x] Manual **file** exists in Context / git `docs/witpae-manual.txt` (not wired into the exe)
- [x] Local RAG index exists in the agent store (`internal/manual-rag/`) — **not in the Windows app**

---

## Page: Field Manual (WITP:AE Manual)

- [x] Field Manual module in Master Menu `[F]`
- [x] Header `FIELD MANUAL: WITP:AE GAME MANUAL BROWSER`
- [x] Section header `[ WITP:AE MANUAL ]` (supersedes `C H A P T E R   B R O W S E R   &   R A G   S E A R C H`)
- [x] Removed redundant `Index : Chapter X...` line
- [x] Launch `witp_ae_manual_wiki.html` in default web browser via native Win32 `ShellExecuteA`
- [x] In-app keyword/RAG search fallback across rules and tables

---

## Page: Settings — not implemented (menu stub)

Spec §4.2 lists the module; it does not enumerate settings rows. Until the spec lists them, do not invent toggles.

- [ ] Settings screen with real options (empty until specified)
- [ ] User-configurable Allied SIGINT confidence (§6) — no UI yet

---

## Page: Exit

- [x] Exit module / clean process exit — **skeleton**
- [x] Purge in-memory data on exit
- [x] Purge on start and on campaign switch (§3)

---

## Not on a page yet (engine)

These are milestones with **no screen**. They do not make War Room “done.”

### Text ingest (§5) — tests on golden files

- [x] Parse SIGINT (`asigint_*` / `jsigint_*`)
- [x] Parse operational reports
- [x] Parse combat reports
- [x] Parse combat event logs
- [x] Ingest parse results into the Campaign File (not shown in War Room yet)
- [x] Wire parse results into War Room dashboard (Tab 1–3 vault heuristics; belief model + full TPIE still open)
- [x] Campaign vault zip (single `.wic` in `\Campaigns`)
- [x] `.pws` container probe + optional sunk-ship read (`pws_container`; gzip/zlib; 464-byte rows) — not full telemetry
- [ ] Parse `.pws` remaining telemetry (live hex, damage, bulk records — see field map)

### Belief / FOW (§6)

- [ ] Belief model (not raw text as truth)
- [ ] Allied vs Japanese SIGINT confidence rules in product
- [ ] BDA as claims with corroboration over turns

### TPIE engine (§7)

- [ ] Invasion warnings, landing focus, unseen TF, logistics, trade routes
- [ ] Modules A–E (amphibious probability, trajectory, air attrition, pause detection, partisan)
- [ ] In-process graph / time-series / hex models serialized into Campaign File
- [ ] Markov / Kaplan-Meier / heuristic alert engine

### Install / sandbox (§3, §9)

- [x] Vault path `C:\WitP-CIC\Campaigns` (exe may still live under Documents)
- [x] Single-exe ship without extra MinGW DLLs (static link)
- [x] Campaign sandbox: one side per vault file

### Spec v1.1+ (do not build now)

- Surface combat log detail, global weather, ship tactical map icons, cloud/multi-user (writing/modifying game files permanently deleted from scope)

---

## Suggested build order (product)

1. ~~Static exe + `\Campaigns` vault + wizard/Continue ingest~~ (local `33f728b`)
2. **War Room Tab 1** from vault data (then map, then TPIE)
3. **Wiki** from the game manual
4. **Settings** after spec lists the fields
5. **`.pws` unlock** when format/password is proven
