# WITP-CIC Baseline Principles

**Document Title:** `baseline_witp_cic`  
**Document Status:** Core Project Documentation (Foundational Authority)  
**Authority:** Ozgur (Owner)  
**Established:** 2026-09-19 20:57 UTC+3  

---

## 1. Governance & Authority Principles

### 1.1 The Baseline Acceptance Rule
> **"Never mark anything done unless I say so." — Ozgur**

- **Absolute Gate:** Technical milestones (writing code, compiling static binaries, achieving 100% automated test passes) are necessary engineering prerequisites, but they **never** mark a requirement or checklist item complete.
- **Authority:** An item in `docs/todo.md` or any milestone checklist transitions to completed status (`[x]`) **exclusively upon explicit confirmation and command from Ozgur**.
- **Immutability:** No requirement in the baseline may be diluted, simplified, or deferred without Ozgur's explicit recorded sign-off.

### 1.2 Git Repository Authority & Exclusivity
- **Sole Remote:** The only authoritative Git repository for this project is:
  `origin` -> `https://github.com/oaltinyay70-cell/WITP-CIC2.git` (branch `main`).
- **Cursor Repositories Purged:** All Cursor remotes (`origin.cursor.com`, `WITP-CIC-CURSOR`) and Cursor feature branches (`cursor/...`) are permanently decommissioned and forbidden.
- **Strict Prohibition:** Never push to, pull from, or reference Cursor repositories.

### 1.3 Core Docs Classification & Sovereignty
The following documentation suite constitutes the immutable **Core Docs** of WITP-CIC:
1. **`docs/baseline_witp_cic.md`** — The definitive baseline principles and rules bible.
2. **`docs/master_implementation_plan.md`** — The cumulative master implementation plan recording all historical, active, and future phases.
3. **`docs/todo.md`** — Master to-do list and completion tracking.
4. **`docs/lessons-learned.md`** — Knowledge base of corrections, learnings, and anti-patterns ("what NOT to repeat").
5. **`docs/architecture-decisions.md`** — Architectural and engineering decisions governing system design.
6. **`docs/screen-flow.md` & `docs/screen-flow-text.md`** — Operational state machine and screen navigation flow diagrams.
7. **`docs/scope-v1.md`** — Living design spec bible, requirements authority, and date-stamped changelog.
8. **`docs/project-context.md`** — High-level project constraints, quality rules, and environment guidelines.
9. **UI Documentation & Specs** — Layout standards, green phosphor palette, typography, and acoustic specifications.

**Maintenance Rule:** All Core Docs must be continuously maintained, synchronized, date-stamped, and preserved across all sessions.

### 1.4 Continuous Lessons Learned Protocol
- **Immediate Documentation:** Following *every* correction, feedback item, or design clarification provided by Ozgur, the agent must immediately log an entry in `docs/lessons-learned.md`.
- **Mandatory Entry Structure:**
  - **Date/Time** (UTC+3)
  - **Trigger / Context**
  - **What Was Learned**
  - **What NOT to Repeat (Anti-Patterns)**

### 1.5 Business Scope Synchronization & Conflict Clarification Protocol
> **"Every new todo and correction must be added to the business scope. If there is any conflict ask me to clarify." — Ozgur (2026-09-20 15:05 UTC+3)**

- **Mandatory Scope Addition:** Every new to-do item, user directive, and correction MUST be immediately added to the authoritative business scope specification bible (`docs/scope-v1.md`) with an explicit date/time stamp. Business scope is the supreme requirements baseline.
### 1.6 Version 0.02 (Stable Alpha 1) Baseline Preservation & Time-Travel Protocol
- **Preserved Milestone:** Version 0.02 is designated as **Stable Alpha 1** as directed by Ozgur (2026-09-20 16:15 UTC+3).
- **Time-Travel Capability:** To ensure future work can always be reverted back to this rock-solid alpha state if subsequent developments are unsatisfactory:
  1. **Git Tags:** Immutable release tags `v0.02` and `v0.02-alpha.1` pushed to GitHub `origin`.
  2. **Dedicated Git Branch:** Permanent branch `stable-alpha-1` tracks this exact commit on `origin`.
  3. **Local Standalone Archive:** `backups/WITP-CIC2-v0.02-alpha1.zip` contains a self-contained snapshot of the source, tests, documentation, and precompiled static binary (`dist/WitP-CIC.exe`).
- **Time-Travel Rollback Instructions:**
  - To check out this exact release in Git:
    ```bash
    git checkout v0.02
    # or:
    git checkout stable-alpha-1
    ```
  - To restore completely to this baseline:
    ```bash
    git reset --hard v0.02
    ```
  - Full rollback documentation: [`docs/how-to-rollback-to-alpha-1.md`](docs/how-to-rollback-to-alpha-1.md).

### 1.7 Mandatory Upgrade, Backup, and Rollback Procedure
> **"from now on any upgrade will be done in this procedure" — Ozgur (2026-09-20 16:31 UTC+3)**

Every future version upgrade, release, or milestone progression must strictly follow this exact 6-step lifecycle procedure:
1. **Automated Verification:** Compile static self-contained binary `dist/WitP-CIC.exe` (`-static -static-libgcc -static-libstdc++ -lkernel32 -luser32 -lwinmm`) and verify 100% pass across all unit and regression test suites.
2. **Core Docs Synchronization:** Update `docs/scope-v1.md` (changelog + requirements), `docs/baseline_witp_cic.md`, `docs/lessons-learned.md`, `docs/architecture-decisions.md`, `docs/project-context.md`, and `docs/milestones.md`. Keep `docs/todo.md` strictly aligned with the Baseline Acceptance Rule (no `[x]` without explicit user sign-off).
3. **Physical Backup Archive:** Generate a standalone zip archive under `backups/` matching `backups/WITP-CIC2-v<Version>-<Milestone>.zip` preserving the complete codebase and runnable binary.
4. **Git Tagging & Stable Branching:** Create annotated Git release tags (e.g. `v0.0X`, `v0.0X-<milestone>`) and update the corresponding stable release branch (`stable-<milestone>`).
5. **Full GitHub Push:** Push `main`, stable branch, and tags to GitHub `origin`.
6. **Rollback Guidance Maintenance:** Maintain and update rollback documentation (`docs/how-to-rollback-to-alpha-1.md` and subsequent guides) with explicit time-travel commands.

### 1.8 Cumulative Master Implementation Plan & Anti-Hallucination Mandate
> **"baseline : keep all implementation plans in the Master FIle : Master Implemantation plan . Must be a cumulative file showing every phase of implement done or will be done . If there is a clash , ask me . never hallucinate . if in doubt ask me . this is part of the core documentation" — Ozgur (2026-09-20 20:31 UTC+3)**

1. **Cumulative Master Record:** All implementation plans across all phases (completed, active, and future) must be permanently codified in `docs/master_implementation_plan.md`. It must never be wiped or restarted per phase; each phase adds cumulatively to the historical record.
2. **Zero Hallucination Standard:** Never invent mechanics, game rules, file offsets, or analytics assumptions. All models must be grounded strictly in decompressed PBEM telemetry (`.pws`), official game manuals (`assets/manual/witp_ae_manual.json`), and verified plain-text turn reports.
3. **Clash & Doubt Protocol:** If any conflict arises between requirements, design decisions, or files—or if any requirement is ambiguous—the agent must **STOP IMMEDIATELY AND ASK OZGUR** rather than guessing or improvising.

### 1.9 Zero Blinking Policy & Ghost Fleet Deletion
> **"1. The blinking is faulty, eliminate any blinking. from now on , it will be done only for items I specificly say. 2. Delete Ghost Fleet Counter" — Ozgur (2026-09-20 21:05 UTC+3)**

1. **Universal Zero Blinking Policy:** Absolutely no blinking, pulsing, flickering, or high-frequency screen refreshing is permitted anywhere in the terminal interface (including splash screen prompts, headers, menu highlights, alerts, or status text) unless specifically and explicitly requested by Ozgur for a designated element.
2. **Inner Input Polling Requirement:** To prevent screen flickering in console rendering loops, every screen must employ an inner non-clearing input wait loop (`while (true) { int key = Console::readKey(); if (key == 0) { Console::sleepMs(20); continue; } ... break; }`) and only invoke `Console::clear()` when an actual user action requires a redraw.
3. **Ghost Fleet Counter Deletion:** The "Ghost Fleet Counter" metric has been completely excised from the Theater Combat Information Center War Room interface and analytical models. Model E is redesignated strictly as `MODEL E : Battle Claims & Damage Revision Register` focusing on entity reconciliation and damage assessment tracking.

### 1.10 War Room Standard Menu & Interactive Reasoning Architecture
> **"Delete the Tab structure , we will use a Standart Menu here ... WHEN USER PRESSES A MENU , HE IS TAKEN TO THE SUB MENU OF THAT TOPIC ... Every Item in the pop UP must be traversed by cursor keys ... When I come over a bullet and press enter u must show the reasoning for this line ... Correct the bottom phrase to PRESS ESC TO RETURN" — Ozgur (2026-09-20 21:18 UTC+3)**

1. **Standard Hierarchical Menu Hierarchy:** The horizontal tab layout is replaced by a 5-item Standard Menu on the War Room Primary Screen (`[1] Intelligence Summary`, `[2] SIGINT Map`, `[3] TBD`, `[4] TPIE Warning Dashboard`, `[5] Predictive Models Sub-Menu`). Selecting any menu item transitions to that topic's dedicated sub-screen.
2. **Universal Cursor Navigation in Pop-Ups:** All items and bullets rendered inside sub-menus and pop-up manifests must support cursor key traversal (`[UP/DOWN]`) with active inverted phosphor highlighting.
3. **Line-by-Line Analytical Reasoning:** Pressing `[ENTER]` over any highlighted bullet must display a dedicated reasoning dialog detailing the exact telemetry sources, coordinate triangulations, and mathematical deductions supporting that specific line.
4. **Standard Return Footer:** All sub-screens, modals, and pop-up overlays must display the exact exit prompt: `PRESS ESC TO RETURN`.

### 1.11 Geospatial Sighting Last-Seen Date Tracking, Anti-Bullshit Reasoning & Border Stability
> **"Date is missing - this should be the date(s) this TF was seen, In this screen date must show the last time this TF was seen ... Make sure the entire text is on the screen ... Add a confidence Meter Bar , show a %XX confidence level for >=90% make it blink slowly ... TACTICAL REASONING: and INTERCEPT SIGNIFICANCE: must NOT BE Bullshit, dont write anything that doesnt mean anything !. LEave blank for now with *-**-**-**-**-* text ... POP TEXT BOX come scrambled bc of the highlight ... Can u make it a consistent border that wont change." — Ozgur (2026-09-20 21:40 UTC+3)**

1. **Last-Seen Date Ingestion & Display:** Operational report headers are parsed (`OPERATIONAL REPORT FOR Dec 14, 41`) to extract true turn dates even when filenames omit dates. Sightings are correlated to track contact history, and the `[ DATE ]` column strictly renders the most recent sighting date (`lastSeenDate`) formatted as `Dec 14`, ensuring dates are never blank.
2. **Word-Wrapping Over Truncation:** Text inside modals and pop-ups must never be sliced with `...`. Long telemetry lines (such as coordinates and bearing text) must wrap cleanly across subsequent lines with a 4-space indent to ensure all information is 100% visible.
3. **Slow Blinking Confidence Meter Bar Exception:** A graphical Confidence Meter Bar (`CONFIDENCE LEVEL: [==========] XX%`) is displayed in contact modals, grounded in authentic telemetry evidence (air recon observers, ship count >= 7, capital ship classification, multi-turn corroboration). For contacts with `>=90%` confidence, a slow 600ms in-place blink (between `Inverted` and `LightGreen` without screen clearing) is implemented as an explicit user-mandated exception to the zero-blinking policy.
4. **Strict Anti-Bullshit Reasoning Policy:** Fluff and placeholder text in tactical reasoning are strictly prohibited. `TACTICAL REASONING:` and `INTERCEPT SIGNIFICANCE:` fields must remain explicitly blank as `*-**-**-**-*-*` until grounded AI/predictive models analyze the contact.
5. **Rock-Solid Modal Border Stability:** Box drawing in navigable pop-ups must anchor left borders (`| `), padded content, and right borders (` |`) to fixed console columns. Inverted highlight traversal must never shift line lengths or scramble box borders.

### 1.12 Numerical Confidence Level Derivation Engine (Algorithmic Standard)
> **"confidence levels must be a percentage and derived numerically from the pws and txt files . DOnt bullshit , ask me how I do this , say I dont know this !" — Ozgur (2026-09-20 22:08 UTC+3)**

1. **Deterministic Multi-Pass Calculation:** Subjective confidence strings are completely banned. Intelligence confidence must be calculated deterministically as a percentage ($0\% - 100\%$) and standardized tier via a multi-pass C++ engine.
2. **Phase 1 Parsing & Base Scoring (No Regex):**
   - High-speed substring matching via `find` / `substr` / C `strstr` (never `std::regex`).
   - `"Radio transmissions"` $\to$ `RADIO_TRAFFIC` (Base Score = 30).
   - `"is located at"` $\to$ `UNIT_LOCATED` (Base Score = 75).
   - `"is loaded on"` $\to$ `UNIT_MOVING` (Base Score = 65).
   - `"men are based at"` $\to$ `GARRISON_SIZE` (Base Score = 50).
   - `"is moving to"` $\to$ `SHIP_MOVING` (Base Score = 65).
   - Operational recon sightings (e.g. `"Japanese ships at"`, `"Sighting of"`) $\to$ `VISUAL_RECON` (Base Score = 80).
   - Domain inference: `AIR` (JAAF, AF Coy, Fighter, Bomber), `NAVAL` (AMc, Maru, Destroyer, Carrier), `LAND` (Infantry, Cavalry, Army, Regiment).
3. **Phase 2 Corroboration Bonuses & Scopes:**
   - **Parser (Per-Turn Only):** Text parser only evaluates corroboration within the current turn file (`Option B`). If an entity appears $\ge 2$ times in this file $\to +15$ bonus.
   - **Game State Manager (Cross-Turn Persistence):** When contacts recur across turns, permanent game state updates timestamp to current turn, resets Time Decay to $1.0$, and preserves high confidence.
   - **Visual Recon Spatial Corroboration:** Visual recon does not populate `EntityMap`, but populates `HexMap`. If SIGINT radio traffic and visual recon both spot activity at the same hex, both receive the $+10$ Hex Corroboration Bonus.
4. **Phase 3 Time Staleness Decay & Clamping:**
   - Current turn ($\Delta t = 0$): $\times 1.0$.
   - Previous turn ($\Delta t = 1$): $\times 0.8$.
   - Stale turns ($\Delta t > 1$): $\times 0.5$.
   - Final Score = $\text{clamp}\big((\text{Base Score} + \text{Corroboration Bonus}) \times \text{Time Decay},\ 0,\ 100\big)$.
   - Categorical Tiers: $0-39\%$ $\to$ `UNCONFIRMED`, $40-69\%$ $\to$ `PROBABLE`, $70-89\%$ $\to$ `CONFIRMED`, $90-100\%$ $\to$ `VERIFIED`.
5. **Pop-Up Presentation Standard:**
   - Clean header: `CONFIDENCE: 90% [VERIFIED]` (in top bar / meter).
   - Reasoning body: Plain English mathematical breakdown (`Base Score: 75 (Unit Located)`, `Corroboration: +15 (Confirmed by 2 SIGINT intercepts)`, `Time Decay: x1.0 (Reported this turn)`, `Final Calculation: (75 + 15) * 1.0 = 90%`, `Analyst Note: ...`).
6. **Memory Pooling & Integer Hex Hashing:** Coordinates are mapped as integers ($X \times 1000 + Y$) to avoid string allocations; temporary parsing maps are cleared and deallocated every turn cycle.

### 1.13 Versioning and Binary Naming Baseline
> **"it is witp-cic-muthur-alpha4-0 where alpha is the new phase name 0 is the 1st build and will increment by 1" — Ozgur (2026-09-21 23:31 UTC+3)**  
> **"u will follow this rule as a baseline ! from now on" — Ozgur (2026-09-21 23:32 UTC+3)**

1. **Milestone Progression to Alpha 4:** The project is officially designated as **Alpha 4** (`alpha4`) under the standalone **`witp-cic-muthur`** repository.
2. **Standardized Binary Compilation Naming Format:**
   - Every newly compiled Windows binary MUST strictly adhere to the standardized naming structure:
     `witp-cic-muthur-[phase]-[build#].exe`
   - `[phase]`: Current development lifecycle phase (e.g. `alpha4`).
   - `[build#]`: Incremental build counter starting strictly from `0` for each new phase, and **strictly incrementing by 1 on each new compiled executable** (e.g. `witp-cic-muthur-alpha4-0.exe` $\to$ `witp-cic-muthur-alpha4-1.exe` $\to$ `witp-cic-muthur-alpha4-2.exe`).
3. **Active Executable Artifacts:**
   - Active versioned release binary: `dist/witp-cic-muthur-alpha4-0.exe`.
   - Mirrored canonical entrypoints:
     - `dist/WitP-CIC.exe` (mirrored from the latest versioned release binary).
     - `dist/MUTHUR6000.exe` (mirrored for retro-terminal shell launchers).

### 1.14 Naval Unit Capability & Operational Role Baseline
> **"An important prerequisite of making sense of the information collected is to know what each unit type can do. I will give u this information in parts. Analyse them , and base your reasoning on these. if u see any conflict and part u dont understand ask. all of the info will goto the documentation for the scope and other related places like the Field Manual. From now on make your predictive work to include these info" — Ozgur (2026-09-21 13:27 UTC+3)**

1. **Mandatory Predictive Grounding:** All predictive analytical engines (Amphibious Assault Model A, Fleet Trajectory Model B, Logistical Pause & Base Status Model D, Strike Concentration Model F, Established Routes Model G) and intelligence reasoning pop-up modals must explicitly ground their deductions on verified unit-type capabilities.
2. **Authoritative Functional Taxonomy (7 Classes, 40+ Hull Classifications):**
   - **Major Surface Combatants & Carriers:** CV, CVL, CVE, BB, BC, CA, CL, CLAA, CS.
   - **Surface Escorts & Light Combatants:** DD, DE, Frigate/PF, KV, PC, PCE, PG, PB, SC, PT.
   - **Mine Warfare:** DM, DMS, CM, CMc, AM, AMc, ACM.
   - **Amphibious & Troop Transports:** AP, APA, APc, APD, LSD, LST, LCI, LCM, LCVP, xAP.
   - **Cargo & Logistics Freighters:** AK, AKA, AKL, AKV, AO, AOT, TK, xAK, xAKL, xTK.
   - **Mobile Tenders & Auxiliaries:** AD, AE, AKE, AG, AGC, AGP, AMC, AR, ARD, AS, AV, AVD, AVP.
   - **Submarines:** SS.
3. **Mobile Auxiliary & Tender Operational Doctrine:**
   - **AS (Submarine Tender):** Mobile forward submarine base; rearms torpedoes and services engines at any port/anchorage (even size 0). Signals forward deployment of extended submarine patrol lines.
   - **AD (Destroyer Tender):** Rearms and repairs DD/DE/escorts in forward anchorages. Signals preparation for sustained surface combat or intense ASW sweeps.
   - **AV / AVD / AVP (Aviation Tenders):** Supplies aviation support, fuel, and ordnance for floatplanes and flying boats at coastal bases (even size 0). Underway air operations require Air Capacity > 0. Precedes wide-area reconnaissance and search arcs over contested sea lanes.
   - **AR (Repair Ship):** Repairs system, engine, and flotation damage across all ship classes at forward anchorages (up to 5 pts Major Engine / Flotation repair).
   - **ARD (Auxiliary Repair Dock):** Floating drydock that ONLY repairs flotation damage (including Major Flotation) up to 100 repair pts/turn (1.0 flotation pt/turn) for a single ship within lift capacity. Identifies forward fleet repair hubs.
   - **AE / AKE (Ammunition Ships):** Rearms heavy capital guns (8-inch, 14-inch, 16-inch) and carrier sorties in forward anchorages. Concentration alongside BB/CA indicates imminent shore bombardment or sustained fleet actions.
   - **AO (Fleet Oiler):** Equipped for underway replenishment of combat task forces at sea. Indicates deep-penetration carrier or cruiser strikes with extended endurance.
   - **APD (Fast Transport):** Converted destroyers for high-speed (25-30+ knot) insertion of company-sized combat units or emergency evacuation; indicates tactical surprise raids rather than full invasion echelons.
   - **ACM (Auxiliary Mine Tender):** Disbanded at base with fuel and <10% damage, protects up to 150 mines from decay; indicates permanent harbor fortification.
   - **AKV (Aircraft Transport):** Transports fully assembled, uncrated airframes ready for immediate service upon arrival.
   - **AMc (Coastal Minesweeper):** Restricted to local harbor sweeps (1 hex radius); defensive harbor clearing only.
4. **Strict Standard:** Remove all bracketed citations `[...]`. Zero assumptions, zero hallucinations, zero errors.

### 1.15 Base Location Registry & Dynamic Hex Grid Database Baseline
> **"I now give u the `locations` on the map with coordinates. These are `base locations`. These locations may house units , ships or planes. Obviously only the ones near a sea can house ships. THe region name like `japanese empire` is only for the beginning and bases may change hands. ingest data below and use it in predictive analysis and anyother related area. add to documentation" — Ozgur (2026-09-21 14:45 UTC+3)**

1. **Authoritative Base Count & Hex Coordinates:** Exactly 1,330 bases across 71 geographical regions ingested into `BaseLocationRegistry` (`base_locations.json`, `src/campaign/base_registry.hpp`, `src/campaign/base_registry.cpp`). Every base is anchored to authentic $(X, Y)$ map coordinates.
2. **Housing Capabilities & Maritime Restriction:** Base locations house ground units, naval vessels, or aircraft. Only coastal bases adjacent to navigable ocean hexes house ships (`isPort == true`).
3. **Base Locations DO NOT Move (Dynamic Controller Architecture):** Base locations and their $(X, Y)$ hex coordinates are permanently fixed physical geography — **base locations DO NOT move!** The initial region/country represents starting scenario control. What changes hands during the war is only the controlling nation/faction as bases are captured (`updateBaseController()`), while the physical coordinates remain strictly immutable.
4. **Predictive Analytics Grounding:**
   - **Model A (Amphibious Assault):** Dynamic coordinate resolution for any target base mentioned in SIGINT unit loadouts or troop movements.
   - **Model B (Dead Reckoning Trajectory):** Dynamic nearest-base proximity searches within projected confidence radii to identify approaching assault sectors and intercept windows.
   - **Model D (Logistical Pause):** Port status and turnaround monitoring across all theater ports and forward anchorages.
   - **Tab 2 Geospatial Sightings:** Distance and bearing calculated relative to authoritative base coordinates.
   - **Dossier Entity Search:** Complete indexed lookup and substring search across all 1,330 bases.

### 1.16 Master Hex Map & Geospatial Intelligence Applications Baseline
> **"this is the hex map where u can see the numbers. top left is 0,0. Store it for future use. once done, suggest some uses for it using the current scope" — Ozgur (2026-09-21 14:55 UTC+3)**  
> **"add the following to the scope: 1. Maritime Obstacle Avoidance & Nautical Pathfinding ... 2. Strategic Chokepoint & Convoy Interdiction ... 3. In-Terminal Tactical Mini-Radar / Sector Map ... 4. Land-Based Air Search & Strike Umbrellas ... 5. Amphibious Beachhead Approach Lanes ... 6. Hex Grid Calibration & Coordinate Ground-Truth" — Ozgur (2026-09-21 15:05 UTC+3)**

1. **Master Theater Map Asset:** Official high-resolution Gary Grigsby's War in the Pacific: Admiral's Edition theater hex map stored at `assets/maps/witp_ae_full_hex_map.jpg` (mirrored at `docs/witp_ae_full_hex_map.jpg`) with top-left coordinate origin $(0, 0)$.
2. **Six Mandatory Geospatial Applications:**
   - **1. Maritime Obstacle Avoidance & Nautical Pathfinding (Model B):** Binary land/sea hex collision mask enables A*/Dijkstra nautical pathfinding around land barriers (e.g. Luzon Strait, San Bernardino Strait), eliminating "ships sailing over mountains."
   - **2. Strategic Chokepoint & Convoy Interdiction (Model G):** Identifies natural narrows (Malacca, Sunda, Makassar, Surigao, San Bernardino, Tsushima) to forecast mandatory crossing windows for submarine ambush.
   - **3. In-Terminal Tactical Mini-Radar / Sector Map (Tab 2):** Renders a 40x20 CP437 ASCII/ANSI tactical sector radar centered on selected contacts with coastlines (`#`), friendly ports (`O`), and blinking contact vectors (`*`).
   - **4. Land-Based Air Search & Strike Umbrellas (Models A & F):** Concentric range rings around enemy airbases (3-hex torpedo strike, 5-hex medium bomber, 8-hex search) alerting to air danger zones and blind spots.
   - **5. Amphibious Beachhead Approach Lanes (Model A):** Projects the approach vector of assault echelons (AP, APA, LST) from staging bases to landing beaches.
   - **6. Hex Grid Calibration & Coordinate Ground-Truth:** Cross-references doubtful PBEM reports and `.pws` coordinates against visual ground truth.

### 1.17 Opponent-Only Intelligence & BDA Claims Display Baseline
> **"THis menu must be only for the other side ( Japanese in this case ) assets. I already know what is happening to me." — Ozgur (2026-09-21 18:15 UTC+3)**

1. **Intelligence CIC Mandate:** In the War Room, all intelligence tracking screens, battle claims, damage revisions, and Battle Damage Assessments (BDA) must display **ONLY THE OPPOSING SIDE'S ASSETS** (Japanese in an Allied campaign; Allied in a Japanese campaign).
2. **Operational Rationale:** The player already knows what is happening to their own friendly forces through native operational reports, task force screens, and ship management logs. Presenting friendly casualties and own-force damage in an intelligence CIC claims menu pollutes actionable intelligence with redundant data.
3. **Filtering & TUI Presentation Rules:**
   - In Tab 1 Intelligence Summary, item `[4] [ BATTLE DAMAGE ASSESSMENT & CLAIMS REVISION REGISTER ]`, only hostile vessels and damage assessments are displayed. Friendly casualties are strictly excluded.
   - In the Predictive Intelligence Sub-Menu, item `[4] [ MODEL E: BATTLE CLAIMS & DAMAGE REVISION REGISTER ]`, only opponent claims and reconciliation entries are displayed.
   - Menu option summary counters (e.g. `X action assessments tracked`) count only opponent claims.
   - If no hostile claims or damage assessments are recorded for the current turn range, the interface renders: `NO HOSTILE COMBAT DAMAGE CLAIMS RECORDED FOR THIS TURN RANGE.`

### 1.18 Bold Headers & Program-Wide Text-Wrapping Baseline
> **"Make the Headers Bold. The text is cutoff across the box. Apply text wrapping. U can use smaller fonts in pop up boxes. This is a pgram wide problem." — Ozgur (2026-09-21 18:27 UTC+3)**

1. **Bold Headers Program-Wide:**
   - All screen headers (`Layout::renderHeader`, `Layout::renderHeaderWithStats`), section headers (`Layout::renderSectionHeader`), and pop-up modal titles (`Layout::renderNotificationModal`, `WarRoomScreen::renderReasoningModal`, `runNavigablePopUp`, `SettingsScreen`, `WikiScreen`, `FieldManualScreen`) must be displayed in **BOLD** typography.
   - Implemented via native Win32 console Virtual Terminal Processing (`ENABLE_VIRTUAL_TERMINAL_PROCESSING` / `0x0004`) combined with ANSI `\033[1m` bold styling and `ConsoleColor::LightGreen` high intensity.
2. **Zero Text Truncation & Program-Wide Multi-Line Wrapping:**
   - Text must NEVER be cut off across the box boundaries or truncated with `...` when the full text can be wrapped.
   - Long item titles in modals (e.g. `ITEM: Malaya Invasion Strike Force (Gulf of Siam / Kota Bharu) [Hex 53,42]`) must wrap cleanly onto multiple inverted lines rather than being sliced with `substr(..., innerWidth - 3) + "..."`.
   - All reasoning and telemetry evidence lines must wrap cleanly without dropping sentences or words.
3. **Screen Isolation on Modal Entry:**
   - All pop-up modals (specifically `WarRoomScreen::renderReasoningModal`) must call `Console::clear()` upon opening to wipe underlying background text, preventing edge bleed-through and fragmented background characters (`[`, `==`, `--`, `+`).
4. **Elimination of Off-By-One Wrapping Drop Bug:**
   - Sizing calculations for modal content height and row bounds must accommodate all header, wrapped title, divider, meter, and disclosure lines.
   - `renderReasoningModal` must render every single line of `wrappedLines` without dropping the final wrapped line.
5. **Compact Pop-up Box Layout:**
   - Pop-up boxes must optimize interior layout (width up to 78 columns, startX at 1) to maximize content width, prevent cramped line wraps, and maintain symmetrical border alignment.

---

## 2. Platform & Engineering Architecture Principles

### 2.1 Platform Deployment Priority
- Version 1 ships for **Microsoft Windows first**.
- Canonical default installation path: **`C:\WitP-CIC`**.
- Campaign vaults directory: **`C:\WitP-CIC\Campaigns`** (the label `\Campaign Vaults` is a synonym error and must never be used).

### 2.2 Native Binary & Static Toolchain
- **Language:** Primary implementation is strictly **C++17**.
- **Binary Format:** Windows console executable (`WitP-CIC.exe`).
- **Linker Mandate:** All production and test compilation must specify:
  `-static -static-libgcc -static-libstdc++ -lkernel32 -luser32 -lwinmm`
- **Zero Runtime Dependencies:** The resulting executable must be 100% self-contained and run on a clean Windows machine with zero external DLL or framework prerequisites.

### 2.3 Process Management During Builds
- Windows exclusively locks active `.exe` files. Before invoking the linker on `dist/WitP-CIC.exe`, check if the application is currently running (`Get-Process -Name WitP-CIC`) and terminate it (`Stop-Process -Force`) to prevent permission errors.

---

## 3. Data Sovereignty & Storage Principles

### 3.1 Local-Only Execution & Zero Data Leakage
- WITP-CIC is an offline, single-player Play-By-Email (PBEM) helper.
- **Zero Cloud Leakage:** The application never makes outbound network connections, opens remote sockets, or uses cloud storage.
- **Read-Only Guarantee:** The application strictly inspects game files; it **never** writes to, modifies, or intercepts actual game files or runtime game memory.

### 3.2 Single Campaign File Container Model
- Each campaign is encapsulated within a **single compressed container file** (`.wic`) stored under `C:\WitP-CIC\Campaigns\<CampaignName>.wic`.
- **In-Process Analytics:** Graph relationships, per-turn time series, and hex-spatial structures required by analytics operate in-memory and serialize directly into the single `.wic` container.
- **Forbidden Services:** Running separate database daemons (PostGIS, Neo4j, InfluxDB) or creating sidecar data directories outside `C:\WitP-CIC\Campaigns` is strictly prohibited.

### 3.3 Absolute Memory Sandbox & Purge
- Campaign data belongs strictly to the side commanded (Allied or Japanese); one campaign cannot influence another.
- An unconditional zeroing and deallocation of all internal graphs, time-series models, and report caches must execute:
  1. On application startup.
  2. When switching between campaigns.
  3. On application termination.

### 3.4 Immediate Physical Container Persistence
- When a new campaign is created in the setup wizard, the system must immediately write and serialize the physical `.wic` container to disk at `C:\WitP-CIC\Campaigns\<Name>.wic`. In-memory state alone is never sufficient; campaigns must survive application restarts immediately.

---

## 4. User Interface & Nostromo Terminal Principles

### 4.1 MU/TH/UR Nostromo Green Phosphor Palette
All terminal UI rendering follows the Alien ship-computer monochrome green phosphor aesthetic using native Win32 console attributes:
- **Code 2 (Dark Green):** Structural frames, borders (`=`, `-`), inactive labels, background text.
- **Code 10 (Light Green):** Screen titles, section headers, active prompts, numerical values.
- **Code 160 (Inverted Highlight):** Selected menu items (black text on light green background).
- **Code 12 (Red Alert):** Serious errors and critical warning banners.

### 4.2 Console Attribute Bleed Prevention
- When clearing the console screen buffer (`Console::clear()`), the text attribute must always be explicitly set to default dark green (`ConsoleColor::DarkGreen`) before calling `FillConsoleOutputAttribute`. Never clear using the currently active attribute when inverted highlighting is enabled.

### 4.3 Typography & Nostromo Formatting
- **Header Topography:** Wide letter-spaced uppercase titles (`O V E R M O N I T O R I N G   A D D R E S S   M A T R I X`).
- **Framing:** Double horizontal borders (`=`), dashed sub-dividers (`-`), and clean corner markers (`+`).
- **Continuous Selection Underline or Total Removal:** When an option is chosen, it must be underlined with a continuous line, not segmented ASCII `-` or `_` characters. If continuous line rendering cannot be achieved cleanly in the console font, the underline must be eliminated entirely, relying solely on high-contrast inverted highlight (Code 160).
- **Vertical Double-Spacing:** All interactive menu options (Start Screen, Master Menu, Campaign Selection, Wizard steps) must be double-spaced with a blank line separating choices to ensure comfortable retro terminal readability and prevent cursor jitter.
- **Blinking Cadence:** Prompt blinking cycles at approximately 1 Hz (500 ms visible, 500 ms inverted). Keyboard input polling operates at high frequency (25 ms) so key response is instantaneous.

### 4.4 Right-Aligned Topic Explanation Pop-Up Window
- Topic descriptions and option explanations must be presented in dedicated framed pop-up modal dialogs situated to the RIGHT of the selection menu (columns 41-78) with compact word-wrapping and dimmed phosphor typography (`DarkGreen`), rather than inline text pushed directly beneath selected items or bottom-stacked boxes. This ensures zero vertical menu jitter and keeps all content within standard 80x25 terminal screens.

### 4.5 Dynamic Footers
- The bottom of each screen displays hotkeys relevant strictly to that screen. Footers are dynamically composed per screen and never hardcoded globally.

### 4.6 Header Instrumentation & Top Telemetry
- All campaign metadata (`Active Campaign: <Name> (<Side> Command)`) and file telemetry (`Files Processed : XX pws files | xx TXT files | Days Processed : xx`) must be consolidated into the top instrumentation header block (`Layout::renderHeaderWithStats`), partitioned by vertical `|` lines, keeping the central command area spacious and clean.

### 4.7 Framed Pop-up Modals for System Notifications
- System status and transition notices (e.g. War Room engagement, loading settings) must be rendered in centered framed modal pop-up boxes. Never overwrite screen content using hardcoded cursor positions.

---

## 5. Acoustic Soundscape Principles

### 5.1 Deletion of Continuous Background Ambient Sound
- In accordance with user directive (2026-09-20 14:20 UTC+3), continuous looped background radio static audio has been deleted from application launch, runtime, and shutdown. The audio background remains completely silent.

### 5.2 Discrete Acoustic Feedback Cues
- **Navigation Click:** Moving selection cursors (`UP/DOWN/TAB`) triggers a short, sombre acoustic click (muted frequency, brief 4-5 ms pulse).
- **Page Transitions:** Navigating screens triggers retro multi-tone teletype chirps (950 Hz, 1400 Hz, 1900 Hz).
- **Acoustic Engine:** All clicks and chirps use native Win32 `Beep(...)`, operating reliably without external file dependencies.

---

## 6. Telemetry Ingestion & PBEM Principles

### 6.1 PBEM Save State (.pws) Forensics
- On-disk `.pws` files are gzip-compressed containers holding zlib records.
- **Password Credential Handling:** The PBEM password (`kilroy43`) is a game-UI PBEM credential only (WITP:AE manual §20.5.4). It is stored in the campaign vault for user reference. WITP-CIC does **not** use it as a cryptographic file decryption gate.
- No password-gate code path exists in ingest.

### 6.2 Discovered Game Files & Multi-Directory Accumulation
- Ingest automatically discovers and parses all 5 relevant game files:
  1. `asigint_*.txt` / `jsigint_*.txt` (Signals Intelligence).
  2. `aoperationsreport_*.txt` (Operational Reports).
  3. `combatreport_*.txt` (Combat Reports).
  4. `Combat_Events_*.txt` (Combat Event Summaries).
  5. `wpae*.pws` (PBEM Turn Save States).
- **Multi-Directory Accumulation:** Pressing `[B]` in setup wizard steps accumulates additional directories without eliminating or overwriting previously entered paths.
- **All Dates Ingested:** Ingestion covers all discovered dates automatically; turn length confirmation prompts are removed.
- **Perspectives:** Shared combat reports are processed strictly from the Allied player's perspective ("shared only to me").

### 6.3 Incremental Turn Ingestion on Campaign Load
- Resuming an existing campaign (`[ CONTINUE CAMPAIGN ]`) must actively diff-scan all configured game directories for newly generated turn files (`.pws` and text reports).
- Any newly detected files must be ingested automatically, serialized to the `.wic` container, and trigger an immediate refresh and recalculation of all War Room analytical models.

---

## 7. Error Handling & Documentation Principles

### 7.1 Standardized 3-Digit Error Codes
All application errors must follow the standardized format **`[XXX]`**:
- **`[101] VAULT_NOT_FOUND`**: Specified `.wic` container could not be located.
- **`[102] VAULT_CORRUPT`**: Magic bytes missing or decompression failed.
- **`[201] REPORT_DIR_MISSING`**: Configured game reports directory missing.
- **`[202] PWS_READ_ERROR`**: Save file locked by another process or incomplete.
- **`[301] PARSE_FORMAT_MISMATCH`**: Text report line format deviates from expected layout (captured in unparsed buffer).
- **`[401] MEMORY_ALLOCATION_FAIL`**: System memory constrained during graph/time-series build.

### 7.2 Error Registry Architecture & Granular Sub-Topics
- Every error code must be documented with its root cause, symptom, and recovery action in external developer documentation (`docs/error-codes.md`).
- Inside the in-app Wiki (`TECHNICAL DETAILS` -> `SYSTEM ERROR CODE REGISTRY`), each error code is implemented as its own dedicated selectable sub-topic (`[101]`, `[102]`, `[201]`, `[202]`, `[301]`, `[401]`). Selecting an error code drills into a dedicated diagnostic page detailing Subsystem, Severity, Root Causes, Symptoms, and Actionable Recovery Procedures.
- Pressing `[ESC]` gracefully navigates back up one level in the hierarchy.

---

## 8. Modular Independence Principles

### 8.1 Distinct Wiki and Field Manual Modules
- **`[W] WIKI`**: Dedicated exclusively to internal WITP-CIC system architecture, War Room dashboard capabilities, PBEM ingestion workflows, Nostromo terminal controls, and the 3-digit Error Code Registry.
- **`[F] FIELD MANUAL`**: Dedicated exclusively to browsing and searching the official game manual, organizing rules reference with keyword search / RAG retrieval and web-browser interactive wiki.
- **Prohibition:** External game reference material and internal application system documentation must never be bundled into a single item or relegated to stubs.

### 8.2 In-Terminal Hierarchical JSON Manual Ingestion & Browser
> **"delete options 2 and 3. dont use the manual wiki as a seperate file, ingest it to the current program as a json file etc ( u decide ). the wiki manual must have the same UI look as the program. no need to write Table contents at the top. First menu shows X.0 level topics , when I select and enter , then I goto the next X.1 level and so on ..." — Ozgur (2026-09-20 17:05 UTC+3)**

1. **Full In-Terminal Look & Feel:** External browser launching and separate files are removed. The entire manual wiki is ingested into the terminal application as hierarchical JSON (`assets/manual/witp_ae_manual.json` and mirrored in `docs/witp_ae_manual.json`).
2. **Options 2 & 3 Deleted:** The previous sub-options `[2] SEARCH MANUAL RULES` and `[3] BROWSE CHAPTER TEXT` are permanently removed. The Field Manual module renders the hierarchical browser directly.
3. **Hierarchical Navigation UX (`X.0` -> `X.1` -> `X.1.1` -> Article Reader):**
   - **Root Level (`X.0`):** Displays all 15 operational chapters (`1.0 Naval Units` through `15.0 Appendices`).
   - **Sub-Topic Level (`X.1`):** Selecting a chapter opens its sub-topics (e.g. `1.0 Chapter Overview`, `1.1 Task Forces and Ships`, `1.2 Forming a Task Force`).
   - **Deep Hierarchy (`X.1.1`):** Sub-sections drill down recursively into sub-topics (e.g. `1.1.1 Task Force Symbols`).
   - **Nostromo Article Reader (Leaf Nodes):** Selecting a leaf section opens the in-terminal article viewer with word-wrapped content lines, line scrolling (`[UP/DOWN]`), page jumps (`[PGUP/PGDN]`), and `[ESC]` return navigation.
4. **Retro Terminal Standards:**
   - Section header: `W I T P : A E   M A N U A L` at root, and `[ X.0 CHAPTER TITLE ]` at sub-levels.
   - NO "Table of Contents" label at the top.
   - NO `Index : ...` line.
   - Double-spaced menu options with inverted phosphor highlight.
   - Right-aligned `[ TOPIC EXPLANATION ]` pop-up box (columns 44–78) rendering section summaries in dimmed phosphor (`ConsoleColor::DarkGreen`).
   - Sombre click acoustic cues on movement (`UP/DOWN/TAB/PGUP/PGDN`) and teletype chimes on transitions.
5. **Zero-Dependency C++ JSON Engine:** A recursive descent JSON parser is integrated into `field_manual_screen.cpp` for instant parsing of all 15 chapters and 414 section nodes with zero third-party dependencies.

---

## 9. Intelligence War Room & Modal Navigation Principles

### 9.1 Hierarchical Reasoning Modal Drill-Down Baseline
> **"Make header bold in the pop box. I should be able to traverse ovr the topics using cursor and when i press I must be able to see what led to this result. ie Localized position and Telemetry Source can be merged : pack all into one header and pressing on it reveals by which means and at what frequency it was seen until when. Confidence analysis computations must also be drill down, dont show it unless someone presses enter on it. Text is cut off across the pop up border. this is a common error, solve it program wide." — Ozgur (2026-09-21 18:40 UTC+3)**

1. **Interactive 2-Tier Modal Navigation:** Modals must not be static monolithic text dumps. When an intelligence item is selected, the reasoning dialog opens into Tier 1 (Topic Directory Screen), presenting selectable evidence topics traversed with `[UP/DOWN]` and entered via `[ENTER]`.
2. **Merged Telemetry & Localization History:** `TELEMETRY SOURCE` and `LOCALIZED POSITION` are merged into Topic 1: `[1] TELEMETRY SOURCE & LOCALIZATION HISTORY`. Entering this topic reveals detection means, localized coordinates, observation frequency, and tracking chronology (dates seen).
3. **On-Demand Confidence Computation:** Mathematical formulas, base scores, corroboration bonuses, time decay multipliers, and analyst notes are strictly hidden from the top-level modal view and isolated into Topic 3 (`[3] CONFIDENCE DERIVATION & MATHEMATICAL PROOF`). They are disclosed solely when the user drills down via `[ENTER]`.
4. **Program-Wide Multi-Line Word Wrapping:** Every title and detail line across all screens and modals must wrap within `innerWidth` (indenting continuation lines by 4 spaces). Zero text cutoff, zero border clipping, and zero truncation ellipses (`...`) on titles or reasoning evidence.
5. **Screen Isolation:** All modal dialogs must call `Console::clear()` on entry to wipe background screen text completely, eliminating bleed-through.

### 9.2 Header Turn & Item Count Disambiguation Baseline
> **"Turn count on this turn is 3 , however at least 7 days must have been loaded it to the campaign vault file. Analyse then solve this. ALl the ingested data from all days must be processed. Make sure this is so" — Ozgur (2026-09-21 18:54 UTC+3)**

1. **Zero Header Counter Conflation:** The top-level layout header function (`Layout::renderHeader`) must never conflate item counts, query match counts, or category labels with campaign turn dates. The `"Turn: "` prefix must only be prepended when the provided string is a bona fide date or campaign turn descriptor. If the subtitle contains `"Turn:"`, `"ITEMS:"`, `"COUNT:"`, `"MATCHES:"`, or section indicators, the text must be rendered exactly as supplied.
2. **Explicit Multi-Day Campaign Telemetry in Pop-Up Headers:** When displaying navigable pop-up lists of intelligence items (strike clusters, assault targets, trajectories, base pauses, battle claims, routes), the header must explicitly display both the current turn date, the total number of campaign days loaded, and the separate item count:
   `Turn: <Date> (<N> Days Loaded) | ITEMS: <Count>`
3. **Exhaustive Multi-Day Data Ingestion Processing:** Every file across all operational days ingested into the campaign vault (e.g. all 8 days: Dec 07 through Dec 15 across 60 files) must be fully parsed and fed into the analytical pipeline (`WarRoomAnalytics::analyzeCampaign`). Models A through G, High Value Target tracking, Tab 2 geospatial bearing logs, and sighting chronology must reflect the complete historical telemetry of all loaded turns without dropping or ignoring earlier turns.
