# WITP-CIC Master Implementation Plan

**Document Title:** `master_implementation_plan.md`  
**Document Status:** Core Project Documentation (Cumulative Authority)  
**Authority:** Ozgur (Owner)  
**Established:** 2026-09-20 20:31 UTC+3  

---

## 1. Governance & Anti-Hallucination Mandates

> **"baseline : keep all implementation plans in the Master FIle : Master Implemantation plan . Must be a cumulative file showing every phase of implement done or will be done. If there is a clash , ask me. never hallucinate. if in doubt ask me. this is part of the core documentation" — Ozgur (2026-09-20 20:31 UTC+3)**

1. **Cumulative Master Record:** This document is the single, authoritative cumulative implementation plan for WITP-CIC. It permanently maintains every historical phase implemented, the active phase under development, and all scheduled future phases.
2. **Zero Hallucination Standard:** Agents and developers must strictly ground all functionality in verified source telemetry, decompressed binary facts, and explicit user requirements. Never invent file formats, unverified binary offsets, or unsupported gameplay rules.
3. **Clash & Doubt Protocol:** If any conflict, ambiguity, or discrepancy arises between requirements, architecture decisions, or user directives:
   - **STOP IMMEDIATELY.**
   - **DO NOT GUESS OR MAKE ASSUMPTIONS.**
   - **ASK OZGUR TO CLARIFY BEFORE PROCEEDING.**
4. **Core Docs Classification:** This file is officially designated as a **Core Doc**. It must be maintained, date-stamped, synchronized, and preserved across all development sessions and Git releases.

---

## 2. Global Phase Map & Execution Matrix

```
========================================================================================================================
PHASE   STATUS         MILESTONE TARGET                                    CORE DELIVERABLES
========================================================================================================================
01      [COMPLETED]    Foundational Scaffolding & Telemetry Parsers        ETL pipeline, text parsers, PWS probe
02      [COMPLETED]    MU/TH/UR Nostromo TUI & Campaign Vault Lifecycle    Splash, Wizard, .wic container, Continue
03      [COMPLETED]    Master Menu, Instrumentation & Soundscape           Header stats, audio engine, modal popups
04      [COMPLETED]    System Wiki & Granular Error Code Registry          Hierarchical Wiki, error codes [101]-[401]
05      [COMPLETED]    Version 0.02 (Stable Alpha 1) & Rollback SOP        Git release tags, backup zip, rollback doc
06      [COMPLETED]    In-Terminal Hierarchical Manual JSON Browser        assets/manual/witp_ae_manual.json, X.0-X.1
07      [IMPLEMENTED]  War Room Dashboard, TPIE Models & AI Settings       War Room Tabs 1-3, Models A-G, AI Settings
08      [PLANNED]      Full .PWS Decoding & Strategic Map Visualization   Live hex coords, damage matrix, ASCII map
========================================================================================================================
```

---

## 3. Detailed Phase Breakdown

### Phase 01: Foundational Scaffolding & Telemetry Parsers [COMPLETED]
- **Objectives:** Establish local single-player architecture, zero-cloud execution sandbox, and robust ETL parsers for all 5 core turn files.
- **Deliverables:**
  - `src/parse/`: Plain-text report parsing for Signals Intelligence (`asigint_*`, `jsigint_*`), Operational Reports (`aoperationsreport_*`, `joperationsreport_*`), Combat Reports (`combatreport_*`), and Combat Event Logs (`Combat_Events_*`).
  - `.PWS` Probe Engine: Verified `gzip` outer wrapper decompression and 61 sequential `zlib` stream records; extracted ship tombstone verification field (`0x08 @36 == 19999`) without password gate.
  - MinGW Cross-Compilation Pipeline: Configured static Windows compilation via WSL (`x86_64-w64-mingw32-g++`) with `-static -static-libgcc -static-libstdc++`.
  - Golden Fixture Testing: Implemented test suites operating against golden PBEM turn files (`tests/fixtures/golden/`).

### Phase 02: MU/TH/UR Nostromo TUI & Campaign Vault Lifecycle [COMPLETED]
- **Objectives:** Implement the retro Alien ship-computer green phosphor text user interface and local campaign vault encapsulation.
- **Deliverables:**
  - Console Abstraction Layer (`src/console/`): Win32 console attribute controller implementing MU/TH/UR palette: Dark Green (Code 2), Light Green (Code 10), Inverted Highlight (Code 160).
  - Screen Buffer Sanitization: Resolved console attribute bleed by strictly setting attribute to Code 2 prior to buffer fill.
  - Splash Screen (`src/ui/screens/splash_screen.cpp`): Retro teletype splash animation with 1 Hz blinking prompt.
  - Campaign Setup Wizard (`src/ui/screens/campaign_screens.cpp`): 4-step wizard capturing Side (`Allied`/`Japanese`), optional `.pws` reference password, automated game directory search with `[B]` multi-directory accumulation, and physical `.wic` container creation.
  - Campaign Storage Model (`src/campaign/vault_serializer.cpp`): Compressed container file (`.wic`) stored strictly in `C:\WitP-CIC\Campaigns` containing all telemetry records and metadata.
  - Continue Campaign Screen: Directory browser for existing `.wic` containers with automatic fallback browsing.

### Phase 03: Master Menu, Instrumentation & Soundscape [COMPLETED]
- **Objectives:** Deliver central command navigation, top-level campaign telemetry instrumentation, and acoustic feedback.
- **Deliverables:**
  - Header Instrumentation (`Layout::renderHeaderWithStats`): Consolidated campaign and telemetry stats into the top header block: `Files Processed : XX pws files | xx TXT files | Days Processed : xx`, partitioned by `|`.
  - Master Menu Module Navigation: Seamless routing between `[A] WAR ROOM`, `[W] WIKI`, `[F] FIELD MANUAL`, `[S] SETTINGS`, and `[E] EXIT`.
  - Acoustic Soundscape (`src/console/console.cpp`): Integrated shorter, sombre mechanical clicks (1200 Hz, 18 ms) for selection movement, retro teletype chirp chimes for page transitions, and ambient radio static bursts.
  - Notification Modal Engine: Framed pop-up dialog boxes for system status notifications, preventing screen text overwriting.

### Phase 04: System Wiki & Granular Error Code Registry [COMPLETED]
- **Objectives:** Implement a comprehensive in-app system documentation and troubleshooting engine.
- **Deliverables:**
  - Recursive Category Tree (`src/ui/screens/wiki_screen.cpp`): Top-level categories `OPERATIONAL DOCTRINE`, `TECHNICAL DETAILS`, and `BASELINE PRINCIPLES`.
  - Technical Details Grouping: Nested System Architecture, Combat Information Center, Data Ingestion, Terminal Controls, and Error Registry.
  - Granular Error Code Registry: Dedicated selectable sub-topics for every standard 3-digit system error code (`[101]` to `[401]`), detailing Subsystem, Severity, Root Cause, Symptoms, and Recovery Action.
  - Right-Aligned Topic Explanation Box: Side-by-side layout rendering compact topic summaries in dimmed phosphor (columns 44–78).
  - Clean Selection Highlight: Total removal of segmented ASCII underlines in favor of unbroken high-contrast inverted phosphor highlight.

### Phase 05: Version 0.02 (Stable Alpha 1) & Standardized Rollback SOP [COMPLETED]
- **Objectives:** Establish an immutable baseline checkpoint and codify standard release and rollback procedures.
- **Deliverables:**
  - Immutable Release Tagging: Git tags `v0.02` and `v0.02-alpha.1` pushed to GitHub `origin`.
  - Milestone Branch: Permanent branch `stable-alpha-1` created and pushed to `origin`.
  - Standalone Physical Backup: Self-contained archive `backups/WITP-CIC2-v0.02-alpha1.zip` containing source, golden fixtures, prebuilt static binary, and documentation.
  - Authoritative Rollback Guide: Published `docs/how-to-rollback-to-alpha-1.md` documenting non-destructive time travel and hard rollback instructions.
  - Mandatory 6-Step Upgrade SOP: Codified automated test verification, Core Docs sync, physical backup creation, Git release tagging, remote push, and rollback documentation maintenance.

### Phase 06: In-Terminal Hierarchical Manual JSON Browser [COMPLETED]
- **Objectives:** Replace external web browser launching with a native in-terminal hierarchical manual browser.
- **Deliverables:**
  - Structured Manual JSON Ingestion: Extracted all 15 operational chapters (`1.0` through `15.0`) and 414 section nodes into `assets/manual/witp_ae_manual.json` (and mirrored in `docs/witp_ae_manual.json`).
  - Menu Cleanup: Deleted legacy options `[2]` and `[3]`, deleted `Index : ...` counter line, and removed redundant "Table of Contents" label.
  - Sub-Header Standard: Section header renamed to `[ WITP:AE MANUAL ]`.
  - Hierarchical Drill-Down UX: `X.0` level topics drill into `X.1` topics, sub-sections drill into `X.1.1` topics, and leaf nodes open the Nostromo article reader with line scrolling and page jumps.
  - Zero-Dependency C++ JSON Engine: Embedded recursive descent parser in `src/ui/screens/field_manual_screen.cpp`.

---

### Phase 07: War Room Dashboard, TPIE Models & AI Settings [IMPLEMENTED / VERIFIED]
- **Objectives:** Implement the full naval Combat Information Center (CIC) War Room dashboard (§4.5), all 7 predictive intelligence models (Models A–G), and the predictive AI model connection setting (§4.2).
- **Core Architecture & Deliverables:**
  1. **Analytical Predictive Engine (`src/campaign/war_room_analytics.hpp` & `.cpp`):**
     - **Model A — Amphibious Assault Probability Engine:** Weighted Evidence Accumulator scoring target hexes using radio volume (`Heavy=3`, `Normal=1`), embarkation lines (`+5`), transport movements (`+2`), confirmed unloading (`+10`), and recency decay (`0.9^Δt`). Score ≥ 15 triggers `>90% probability` alert.
     - **Model B — Fleet Trajectory & Intercept Projection:** Dead Reckoning vector projection across multi-sighting sequences, least-squares heading fit, projected T+1 position `(xₙ + speed × cos(θ), yₙ + speed × sin(θ))`, expanding confidence radius, and candidate destination port mapping.
     - **Model C — Air Attrition Forecasting:** Exponential Moving Average (`α=0.3`) kill count per aircraft type, replacement arrivals offset, and days to local superiority loss estimation.
     - **Model D — Logistical & Operational Pause Detection:** TF state machine (`ACTIVE → REFUELLING → REPAIRING → ACTIVE`), calculating repair and transit vulnerability windows.
     - **Model E — Battle Claims Correction (Opponent-Only):** Cross-turn entity reconciliation checking text sinkings against `.pws` tombstones and subsequent sightings to downgrade overclaims; strictly tracks opponent assets (Japanese in Allied campaigns; Allied in Japanese campaigns). Friendly losses are omitted.
     - **Model F — Strike Concentration Detector:** Hex grid clustering scan summing warships within configurable radius (default 10 hexes); flags if cruiser+ count ≥ threshold (default 3).
      - **Model G — Established Shipping Routes:** Repeated waypoint graph per merchant vessel, inferring transit corridors, port endpoints, and confidence levels.
      - **Deterministic Numerical Confidence Engine (§6.1):** 4-phase C++ algorithmic pipeline calculating intelligence confidence strictly as a percentage (0%–100%) and tier (UNCONFIRMED, PROBABLE, CONFIRMED, VERIFIED). Multi-pass parsing via fast non-regex search (`find`/`substr`), base scoring (Radio=30, Garrison=50, Movement=65, Location=75, Visual Recon=80), corroboration bonuses (+15 entity, +10 hex), stateless per-turn parser scope with Game State Manager cross-turn persistence, time staleness decay (1.0, 0.8, 0.5), and integer-keyed hex hashing (`X * 1000 + Y`). Clean modal header (`CONFIDENCE: 90% [VERIFIED]`) paired with full English mathematical breakdowns in the reasoning body.
      - **Dossier & Geospatial Index:** Localized distance and sector bearings relative to friendly HQs; multi-turn sighting tracking with `lastSeenDate` and `allDates` extracted from operational report headers; unified free-text search across bases, vessels, units, air groups, and cities.
  2. **War Room Standard Menu & Sub-Screen Architecture (`src/ui/screens/war_room_screen.hpp` & `.cpp`):**
      - **Standard Hierarchical Menu (Replaced Previous Tabs):**
        - `[1] Intelligence Summary`: 5 major operational domains (HVT tracker, strike clusters, port concentrations, supply routes, claims register) opening navigable pop-ups.
        - `[2] SIGINT Map`: Geospatial contact log with last-seen dates formatted as `Dec 14`, distance and bearing offsets from HQ, hotkey `[D]` for Unified Dossier Search.
        - `[3] TBD`: Reserved partition for Phase 08 strategic map and raw `.pws` vessel tracking.
        - `[4] TPIE Warning Dashboard`: Strategic invasion probability heatmap, fleet trajectories, and intercept windows (`[I]`).
        - `[5] Predictive Models Sub-Menu`: Granular deep-dive telemetry and formula dumps for Models A through G.
      - **Interactive Navigable Pop-Ups & Bullets:** Every item/bullet in sub-menus and pop-ups can be traversed with cursor keys (`[UP/DOWN]`). Box borders are rock-solid with fixed column coordinates that never shift or scramble during highlight traversal.
      - **Reasoning Disclosures on `[ENTER]`:** Pressing `[ENTER]` over any bullet reveals dedicated telemetry evidence, dynamic word-wrapping (no `...` truncation), and a graphical Confidence Meter Bar (`CONFIDENCE LEVEL: [==========] XX%`). For `>=90%` confidence, a slow 600ms in-place blink is executed as an explicit user-mandated exception to the zero-blinking policy.
      - **Anti-Bullshit Reasoning Protocol:** Tactical reasoning and intercept significance fields strictly display `*-**-**-**-*-*` until grounded AI/predictive models are connected.
      - **Universal Footer Standard:** All sub-screens and modals display the exact footer directive: `PRESS ESC TO RETURN`.
      - **Zero Blinking & Stable Rendering:** Inner input polling loops yielding until keypress, preventing screen redraw flicker across all menus.
  3. **System Settings Screen & Predictive AI Model Connection (`src/ui/screens/settings_screen.hpp` & `.cpp`):**
     - **Sub-Item 1 (`[1] CONNECT TO PREDICTIVE AI MODEL`):**
       - User configuration of predictive AI model backend:
         - `[1] Local Analytical Heuristics Engine (Built-in Default)`
         - `[2] Local LLM / Ollama Bridge (http://localhost:11434)`
         - `[3] Google Gemini API Engine`
         - `[4] Custom Local Inference Endpoint`
       - Endpoint URL and API credential configuration.
       - Connection test and live status monitor (`[CONNECTED] / [STANDBY] / [OFFLINE]`).
     - **Detection Thresholds & Parameters (§4.2, §6):**
       - `[2] ALLIED SIGINT CONFIDENCE`: Toggle between `CERTAIN (Default)` and `PROBABLE`.
       - `[3] STRIKE CONCENTRATION WARSHIP COUNT`: Threshold (default `3`).
       - `[4] STRIKE CONCENTRATION SEARCH RADIUS`: Proximity radius (default `10` hexes).
       - `[5] AMPHIBIOUS ASSAULT PROBABILITY SENSITIVITY`: Score threshold (default `15`).
  4. **Naval Unit Capabilities & Auxiliary Doctrine Integration (§6.2, §6.3; 2026-09-21 13:27 UTC+3):**
      - **7-Class Taxonomy Registry:** Ingest and model all 7 vessel functional classes and 40+ hull codes.
      - **Predictive Grounding:**
        - Ground Model A on distinction between heavy assault (APA/AP/LST/LSD) vs fast commando insertion (APD) vs crated/uncrated aircraft movement (AKV).
        - Ground Model D on forward auxiliary base establishment and repair dynamics (AS sub bases, AD destroyer hubs, AV/AVD/AVP seaplane bases, AR/ARD repair docks, AE/AKE ammunition replenishment, AO fleet oilers).
        - Ground Model F on warship vs escort screen composition.
        - Ground tactical reasoning pop-up modals on actual unit capabilities.
      - **Field Manual Ingestion:** Synchronized into `assets/manual/witp_ae_manual.json` and `docs/witp_ae_manual.json` sections `1.15` and `1.16`.
   5. **Base Location Registry & Dynamic Hex Grid Database (§6.4; 2026-09-21 14:45 UTC+3):**
       - **1,330 Base Ingestion:** Ingest all 1,330 authentic bases with $(X, Y)$ hex coordinates across 71 geographical regions into `BaseLocationRegistry`.
       - **Dynamic Controllers:** Support dynamic base controller mutations (`updateBaseController()`) upon capture events.
       - **Maritime Housing Restriction:** Enforce that only coastal bases house naval vessels (`isPort == true`).
       - **Predictive & Geospatial Grounding:** Ground Model A (amphibious targets), Model B (dead reckoning candidate ports), Model D (turnaround and pause bases), Tab 2 geospatial bearing calculations, and Dossier search in the authoritative base registry.
   6. **Master Menu Wiring (`src/ui/screens/master_menu.cpp`):**
      - Wire `[A] War Room` to `WarRoomScreen::run(config)`.
      - Wire `[S] Settings` to `SettingsScreen::run(config)`.
   7. **Automated Test Suite (`tests/war_room_test.cpp`):**
      - Validate Models A–G, dossier entity search, settings AI connection, naval unit capability derivations, and base location registry lookups/mutations against golden fixture data.
   8. **Master Hex Map Geospatial Applications (§6.5; 2026-09-21 15:05 UTC+3):**
      - **Master Map Calibration:** Ingest and anchor master theater map (`assets/maps/witp_ae_full_hex_map.jpg`, origin 0,0).
      - **Model B Obstacle Avoidance:** A*/Dijkstra nautical pathfinding using binary land/sea hex collision mask.
      - **Model G Chokepoint Interdiction:** Mandatory strait crossing windows for submarine ambush.
      - **Tab 2 Tactical Sector Radar:** In-terminal 40x20 CP437 ASCII/ANSI sector minimap.
      - **Models A & F Air Umbrellas:** Land-based air search and strike coverage range rings.
      - **Model A Beachhead Approaches:** Assault echelon approach vectoring from staging ports to target beaches.
      - **Hex Grid Ground-Truth:** Calibration of doubtful report and `.pws` coordinates.
    9. **Bold Headers & Program-Wide Text-Wrapping Baseline (§8.1, §8.2; 2026-09-21 18:27 UTC+3):**
       - **Bold Typography Program-Wide:** Screen headers, section titles, and pop-up modal titles rendered in BOLD Light Green via Win32 Virtual Terminal Processing (`0x0004`) and ANSI `\033[1m`.
       - **Zero Text Cutoff & Full Multi-Line Wrapping:** Eliminates `...` truncation; wraps item titles, telemetry evidence lines, and notification prompts cleanly.
       - **Screen Isolation on Modal Launch:** Modals call `Console::clear()` on entry to eliminate background text bleed-through.
       - **Off-By-One Wrapping Fix:** Accurately accounts for all title and meter rows to render every line of reasoning without dropping the final wrapped line.
       - **Pop-up Geometry Maximization:** Expands modal width to 78 columns (`startX = 1`, `innerWidth = 74`) for maximum readability and compact presentation.
     10. **Hierarchical Reasoning Modal Drill-Down & Merged Telemetry (§4.5, §8.1, §8.2; 2026-09-21 18:40 UTC+3):**
         - **Interactive 2-Tier Modal Navigation:** Transitions reasoning modal to a 2-tier structure: Tier 1 topic directory with `[UP/DOWN]` cursor navigation, and Tier 2 granular drill-down sub-modals entered via `[ENTER]`.
         - **Merged Sighting & Telemetry History:** Merges Telemetry Source and Localized Position into a unified drill-down disclosing detection means, localized coordinates, observation frequency, and tracking chronology.
         - **On-Demand Confidence Computation:** Segregates mathematical formulas, base scores, bonuses, and decay multipliers into a dedicated drill-down disclosed only on explicit `[ENTER]`.
         - **Program-Wide Text Cutoff Elimination:** Enforces `wrapText(line, innerWidth, 4)` for all detail lines to ensure zero border clipping.

---

### Phase 08: Full .PWS Decoding & Strategic Map Visualization [PLANNED]
- **Objectives:** Decouple from text-only limitations by decoding underlying `.pws` binary structures and rendering advanced geospatial map views.
- **Deliverables:**
  - **Live Ship Hex Coordinates:** Decode vessel records in `.pws` to extract exact ship locations, current speed, and destination without waiting for visual sighting reports.
  - **Ship Damage Matrices:** Decode internal hull, system, engine, and flood damage values per ship.
  - **Air Pool & Replacement Reserves:** Decode `0x20` aircraft pool records to replace fallback scenario pools with exact live game reserves.
  - **2D Strategic Map Interface:** In-terminal ASCII / CP437 strategic map canvas rendering island landmasses, sea hexes, task force markers, and patrol sectors.
  - **Linux Distribution & Proton Packaging:** Cross-compile and validate Wine/Proton execution on Linux platforms.

---

## 4. Verification & Quality Gates

Every implementation phase must strictly pass all quality gates prior to release:
1. **Automated Test Gate:** 100% pass across all unit test suites (`field_manual_test`, `file_scanner_test`, `vault_serializer_test`, `wiki_test`, `war_room_test`, `json_parser_test`).
2. **Static Linkage & Binary Naming Gate (§1.13):** Production binary compiles cleanly with zero dynamic external dependencies following the naming baseline `witp-cic-[mainversion id]-[phaseid]-[built no]` (e.g. `dist/witp-cic-v0.02-alpha2-0.exe`, with `dist/WitP-CIC.exe` mirrored as canonical entrypoint).
3. **Core Docs Synchronization Gate:** Synchronize `scope-v1.md`, `baseline_witp_cic.md`, `lessons-learned.md`, `architecture-decisions.md`, `todo.md`, and `master_implementation_plan.md`.
4. **Physical Backup Gate:** Update standalone compressed archive in `backups/WITP-CIC2-v0.02-alpha2.zip`.
5. **Authoritative Git Gate:** Push commits, milestone branches, and release tags exclusively to GitHub `origin main`.
6. **User Acceptance Gate:** All items remain `[ ]` until Ozgur explicitly confirms completion.
