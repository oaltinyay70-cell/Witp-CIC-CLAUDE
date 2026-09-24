# WITP-CIC Lessons Learned Knowledge Base

**Document Status:** Core Project Documentation  
**Authority:** Ozgur (Owner)  
**Established:** 2026-09-19 20:57 UTC+3  

---

## 1. Core Definitions

### 1.1 Definition of "Baseline"
In WITP-CIC, the **Baseline** is the authoritative, verified standard of project requirements, documentation, and operational code.
1. **Approval Gate:** The Baseline is defined by the rule: **"Never mark anything done unless I say so"** (Ozgur).
2. **Technical vs. Baseline Complete:** Code completion, successful compilation, and 100% automated test passes are technical prerequisites, but they **do not** make a feature baseline-complete. An item transitions to completed status (`[x]`) **exclusively** upon explicit confirmation and direction from Ozgur.
3. **Immutability of Scope:** Requirements defined in the Baseline cannot be altered, simplified, or deferred without explicit recorded approval from Ozgur.

### 1.2 Definition of "Core Docs"
The following documentation suite constitutes the governing **Core Docs** of the WITP-CIC project:
1. **`docs/baseline_witp_cic.md`** — The definitive baseline principles and rules bible.
2. **`docs/todo.md`** — Master to-do list and completion tracking.
3. **`docs/lessons-learned.md`** — Knowledge base of corrections, operational learnings, and anti-patterns ("what not to repeat").
4. **`docs/architecture-decisions.md`** — Architectural and design decisions.
5. **`docs/screen-flow.md` & `docs/screen-flow-text.md`** — Application state machine and UI navigation flow diagrams.
6. **`docs/scope-v1.md`** — Living design spec bible and requirement authority.
7. **`docs/project-context.md`** — Project architecture, file locations, constraints, and process rules.
8. **UI Documentation & Specifications** — Layout, palette, typography, and interaction standards.

**Core Docs Rule:** All Core Docs must be continuously maintained, synchronized, and date-stamped. No session boundary or tooling shortcut excuses omitting Core Doc updates.

### 1.3 The Continuous Learning Protocol
**Rule:** After *every* correction, feedback item, or design clarification provided by Ozgur, the agent must immediately:
1. Record the lesson in this knowledge base under **Section 2 (Historical Lessons Learned Ledger)**.
2. Detail:
   - **Date/Time**
   - **Trigger / Context**
   - **What Was Learned**
   - **What NOT to Repeat (Anti-Patterns)**
3. Mirror any scope changes in `docs/scope-v1.md` changelog and `docs/architecture-decisions.md`.

---

## 2. Historical Lessons Learned Ledger

### LL-001: Git Remotes & Branch Authority
- **Date:** 2026-09-19 20:25 UTC+3
- **Context:** Multiple git remotes existed (`origin.cursor.com`, `WITP-CIC-CURSOR`, and `WITP-CIC2`), with active branch on `cursor/witp-cic-scaffold-18a3`. Ozgur directed: *"Make sure we never use the cursor repo ever again. we are using gh repo. confirm"*.
- **What Was Learned:** Having multiple conflicting git remotes or feature branches tied to third-party tools creates deployment ambiguity and potential data leakage. The single authoritative remote is `origin` -> `https://github.com/oaltinyay70-cell/WITP-CIC2.git` on branch `main`.
- **What NOT to Repeat:**
  - DO NOT create branches named `cursor/...`.
  - DO NOT configure or push to any Cursor remotes (`origin.cursor.com` or `WITP-CIC-CURSOR`).
  - DO NOT push to any remote other than GitHub `origin main`.

### LL-002: Task Completion Autonomy vs. User Confirmation
- **Date:** 2026-09-19 20:25 UTC+3
- **Context:** Agent checked off checklist items in `docs/todo.md` upon verifying unit tests passed. Ozgur directed: *"baseline : never mark anything done unless I say so"*.
- **What Was Learned:** Technical verification is an agent responsibility; requirement acceptance is strictly the user's prerogative. Checking off items autonomously violates user trust and oversight.
- **What NOT to Repeat:**
  - DO NOT mark `[x]` on any item in `docs/todo.md` or milestones based on test execution alone.
  - Wait for Ozgur's explicit instruction to mark any task done.

### LL-003: Continuous Looping Ambient Background Sound
- **Date:** 2026-09-19 20:35 UTC+3
- **Context:** User provided audio WAV and requested: *"use the clip I uploaded as background noice"*.
- **What Was Learned:** The ambient Nostromo audio is intended as continuous background immersion throughout the entire terminal session, not merely a random one-shot sound. Calling `PlaySoundA` with `SND_ASYNC | SND_LOOP` keeps it playing infinitely in the background. Meanwhile, acoustic navigation clicks and transition chirps must use `Beep(...)` so they do not interrupt or stop the Win32 `PlaySound` loop channel.
- **What NOT to Repeat:**
  - DO NOT trigger one-shot `PlaySound` calls that stop the active looped background audio.
  - DO NOT block the UI thread during audio playback.

### LL-004: Terminal Prompt Blinking Frequency
- **Date:** 2026-09-19 20:35 UTC+3
- **Context:** User feedback: *"blinking is too fast"*.
- **What Was Learned:** Blinking prompts at 100 ms (~5 Hz) creates an aggressive, unpleasant strobe effect. Authentic vintage computer terminals cycle at approximately 1 Hz (500 ms normal, 500 ms inverted/blank). High-frequency input polling (25 ms) must be decoupled from the visual blink phase (500 ms) so keyboard responsiveness remains instantaneous.
- **What NOT to Repeat:**
  - DO NOT use short 100 ms toggles for visual blinking.
  - DO NOT block keyboard polling while implementing slow visual timing.

### LL-005: Terminal Menu Vertical Spacing
- **Date:** 2026-09-19 20:35 UTC+3
- **Context:** User feedback: *"make menu double spaced"*.
- **What Was Learned:** Single-spaced menus with underline indicators create vertical crampedness and visually jump when cursors move. Consistent double-spacing (`\n` between items) provides the breathing room characteristic of retro monochrome displays and prevents visual clutter.
- **What NOT to Repeat:**
  - DO NOT bunch menu options directly on adjacent lines without blank lines.
  - DO NOT introduce vertical shifts when rendering underline cursors on selected items.

### LL-006: Separation of Wiki and Field Manual Modules
- **Date:** 2026-09-19 20:39 UTC+3
- **Context:** User directed: *"do the WIKI and FIELD MANUAL. FIELD MANUAL WILL BE A SEPERATE ITEM. code both modules"*.
- **What Was Learned:** The WITP-CIC system documentation (architecture, War Room guide, error registry) and the official WITP:AE game manual (6,720 lines, 24 chapters) are fundamentally distinct domains. They must not be conflated into a single menu item. Wiki belongs on `[W]`; Field Manual belongs on `[F]`.
- **What NOT to Repeat:**
  - DO NOT bundle external game manual reference into internal application system help.
  - DO NOT create stub menu items when explicit standalone modules are requested.

### LL-007: Win32 Console Attribute Bleed on Clear
- **Date:** 2026-09-19 19:44 UTC+3
- **Context:** Pressing ENTER on the Splash screen caused the green/inverted attribute to fill the entire right side of the Start screen.
- **What Was Learned:** Calling Win32 `FillConsoleOutputAttribute` with `csbi.wAttributes` while inverted highlight (attribute 160) was active flooded the entire window buffer with light green. `Console::clear()` must explicitly force `defaultAttr = static_cast<WORD>(ConsoleColor::DarkGreen)` before clearing buffer cells.
- **What NOT to Repeat:**
  - DO NOT clear the console without explicitly setting console text attributes to default dark green first.

### LL-008: Win32 Executable Process Locking During Compilation
- **Date:** 2026-09-19 20:36 UTC+3
- **Context:** Recompiling `dist/WitP-CIC.exe` failed with `Permission denied` because the user had the application open in the console.
- **What Was Learned:** Windows locks running binaries exclusively. Before invoking MinGW linker on `dist/WitP-CIC.exe`, check if the process is running (`Get-Process -Name WitP-CIC`) and terminate it (`Stop-Process -Force`) if rebuilding.
- **What NOT to Repeat:**
  - DO NOT attempt to link over an active running binary without checking process status.

### LL-009: MinGW Static Linking Flags
- **Date:** 2026-09-19 20:23 UTC+3
- **Context:** Unit test binaries failed with exit code 1 because MinGW-w64 dynamically linked to `libgcc_s_seh-1.dll` and `libstdc++-6.dll`, which were not in the Windows system path.
- **What Was Learned:** All C++ compilation for Windows target binaries must strictly specify `-static -static-libgcc -static-libstdc++` along with `-lkernel32 -luser32 -lwinmm` to ensure 100% self-contained binaries without runtime DLL dependencies.
- **What NOT to Repeat:**
  - DO NOT omit `-static -static-libgcc -static-libstdc++` from any compilation command.

### LL-010: Physical Container Persistence vs Memory-Only State
- **Date:** 2026-09-19 19:44 UTC+3
- **Context:** Created campaign `D1`, exited, restarted, and `D1` was gone from Continue Campaign list.
- **What Was Learned:** Screen transitions in the wizard were working in RAM, but did not commit the `.wic` container to disk at `C:\WitP-CIC\Campaigns\<Name>.wic`. Any campaign creation must immediately write the physical serialized file to disk so it survives app restarts.
- **What NOT to Repeat:**
  - DO NOT rely on in-memory state for entity creation without immediate disk serialization.

### LL-011: Hierarchical Recursive Topic Navigation & Nested Technical Details
- **Date:** 2026-09-19 21:05 UTC+3
- **Context:** User provided screenshot of flat Wiki topics and instructed: *"pressing a topic on a page will take u to what is under that topic , this maybe text or another set of sub topics in which case, user will traverse those sub topics in the same manner. add a new topic : `technical details`. all of the items here will be sub topic to this new one and will apear under this topic"*.
- **What Was Learned:** Documentation in WITP-CIC must be organized as a recursive tree of nodes rather than a flat single-level list. Selecting a topic node either navigates to another sub-topic menu or opens a leaf text article. The 5 technical/architectural topics (System Architecture, War Room, Data Ingestion, Terminal Controls, and Error Registry) must be grouped under a dedicated `TECHNICAL DETAILS` topic. The navigation UX must remain identical at every tier (double-spaced items, underline cursor, acoustic clicks, ESC returning up one level).
- **What NOT to Repeat:**
  - DO NOT flatten multi-level documentation into a single list.
  - DO NOT hardcode single-tier navigation loops; build a recursive `WikiNode` tree structure.

### LL-012: Granular Error Code Sub-Topics in Documentation
- **Date:** 2026-09-19 21:08 UTC+3
- **Context:** User provided screenshot of `5. SYSTEM ERROR CODE REGISTRY` displaying a flat text article of error codes and instructed: *"each error along with the name is a sub topic."*
- **What Was Learned:** Multi-item diagnostic registries like the System Error Code Registry should not be dumped as a single flat text screen. In accordance with the recursive Wiki model, `SYSTEM ERROR CODE REGISTRY` must be a branch category displaying dedicated selectable sub-topics for each 3-digit error code and name (e.g. `[1] [101] VAULT_NOT_FOUND`, `[2] [102] VAULT_CORRUPT`, `[3] [201] REPORT_DIR_MISSING`, `[4] [202] PWS_READ_ERROR`, `[5] [301] PARSE_FORMAT_MISMATCH`, `[6] [401] MEMORY_ALLOCATION_FAIL`). Selecting any error drills down into that error's dedicated article detailing Subsystem, Severity, Description & Causes, Symptoms, and Actionable Recovery Procedures. Pressing `[ESC]` gracefully unwinds up one menu level.
- **What NOT to Repeat:**
  - DO NOT dump distinct diagnostic codes onto a single flat text article when individual drill-down navigation is needed.
  - DO NOT flatten complex domain topics into monolithic reader views.

### LL-013: Incremental Ingestion & War Room Model Refresh on Campaign Load
- **Date:** 2026-09-20 14:15 UTC+3
- **Context:** User instructed: *"Add to TODO as a required correction: 1. when a current campaign is loaded , make sure that any new turn files as pws or txt are present in the current folders and ingest them and update the war room models"*.
- **What Was Learned:** In WITP:AE PBEM play, new turn output files and `.pws` saves are continually generated while the user plays turns between sessions. When a player resumes an existing campaign (`[ CONTINUE CAMPAIGN ]`), the application must not merely load historical cached turns from the `.wic` vault into memory. The lifecycle must actively perform an incremental diff scan across all stored report and SAVE directories for files with newer date stamps or filenames not yet recorded in the campaign manifest. Any newly detected turn files must be automatically ingested, added to the `.wic` container, serialized to disk, and the War Room intelligence models (High-Value Targets, Strike Concentrations, Port Activity, Routes, Hex Intercepts) must be immediately refreshed and recalculated so the player has live tactical intelligence for the latest turn upon entering the Master Menu / War Room.
- **What NOT to Repeat:**
  - DO NOT treat campaign loading as a static read-only operation.
  - DO NOT require the user to run setup wizards or manual import steps to pick up new turns.
  - DO NOT ingest new turn data without immediately updating and recalculating the War Room analytical models.

### LL-014: Continuous Underline vs. Clean Inverted Highlight & Pop-up Explanations
- **Date:** 2026-09-20 14:18 UTC+3
- **Context:** User provided screenshot highlighting the segmented dashed line (`---`) beneath an active option and inline description, instructing: *"1. When an option is chosen , it is underlined ... this would be a continuous line not _ character.if u cant do this, then lose the underlining totally. 2. the explanation must be a pop up windows and slightly smaller fonts"*.
- **What Was Learned:**
  1. Terminal font rendering can introduce visible gaps between discrete ASCII characters (like `-` or `_`), destroying the authentic aesthetic of continuous Nostromo UI lines. If a truly unbroken continuous horizontal line cannot be guaranteed natively, the underline decoration must be removed entirely, relying on high-contrast inverted highlight (Code 160) for selection clarity.
  2. Menu descriptions and topic explanations must not be dumped inline beneath selected options as this clutters the menu layout, introduces vertical jumping, and disrupts the menu structure. Explanations belong in dedicated pop-up window modals with compact/smaller typography.
- **What NOT to Repeat:**
  - DO NOT use segmented ASCII dashes (`-` or `_`) as pseudo-underlines when continuous rendering is expected; eliminate the underline completely if unbroken rendering isn't possible.
  - DO NOT display topic descriptions inline within the selection list where they push adjacent menu items around; use dedicated pop-up explanation dialogs.

### LL-015: Deletion of Background Ambient Sound
- **Date:** 2026-09-20 14:20 UTC+3
- **Context:** User instructed: *"Delete the background sound"*.
- **What Was Learned:** Continuous looped ambient background static/noise (`PlaySoundA` background channel) is distracting or undesirable to the user during extended terminal use. It has been permanently removed from the application lifecycle. The soundscape now consists solely of discrete user-triggered acoustic feedback (mechanical navigation clicks and transition chirps via `Beep(...)`), leaving the audio background completely silent.
- **What NOT to Repeat:**
  - DO NOT start any background audio loops on application startup.
  - DO NOT play continuous background noise when the user has requested its deletion.

### LL-016: Sombre, Shorter Acoustic Navigation Click Profile
- **Date:** 2026-09-20 14:21 UTC+3
- **Context:** User instructed: *"add to todo: 1. change the key move click to a shorter , sombre click"*.
- **What Was Learned:** High-frequency, piercing beeps (such as 1800 Hz) can become sharp or grating during rapid keyboard navigation through lists and menus. A sombre, restrained retro mechanical click is achieved with a lower, muted frequency band (e.g. 400-600 Hz) and an ultra-brief pulse duration (4-5 ms). This provides tactile acoustic feedback on cursor movement (`UP/DOWN/TAB`) without sounding shrill or intrusive.
- **What NOT to Repeat:**
  - DO NOT use high-pitch, shrill beeps (e.g. >1500 Hz) for frequent UI navigation events.
  - DO NOT use long pulse durations that sound like audio tones rather than physical mechanical clicks.

### LL-017: Mandatory Business Scope Integration & Conflict Clarification Gate
- **Date:** 2026-09-20 15:05 UTC+3
- **Context:** User instructed: *"make baseline : every new todo and correction must be added to the business scope. if there is any conflict ask me to clarify"*.
- **What Was Learned:**
  1. The business scope document (`docs/scope-v1.md`) is the supreme requirements baseline for WITP-CIC. Every new to-do item, user directive, and correction must be immediately codified within it with an explicit date/time stamp. Requirements can never exist solely as informal chat messages or isolated to-do items without being officially incorporated into the scope bible.
  2. If there is ANY ambiguity, tension, or potential conflict between a newly requested correction/to-do item and existing business requirements or architectural constraints, the agent MUST NOT make unilateral assumptions or choose a path silently. The agent is strictly mandated to pause and ask Ozgur to clarify.
- **What NOT to Repeat:**
  - DO NOT add to-do items or implement corrections without immediately documenting them in `docs/scope-v1.md`.
### LL-018: Header Instrumentation, Pop-up Modal Notifications, & Right-Aligned Explanations
- **Date:** 2026-09-20 15:07 UTC+3
- **Context:** User instructed:
  1. *"Active Campaign: D1 (Allied Command) Database Status: 54 operational files loaded across 7 days. part in the Master Menu must come in the top header like Files Processed : XX pws files xx TXT iles  Days Processed : xx all divided by |"*
  2. *"when u press War room , a message comes up - but it overwrites some text . this can come in a pop up text."*
  3. *"in the [ SYSTEM WIKI & ERROR REGISTRY ] menu and in other menus where applicable , trhe TOPIC EXPLANATIONM must be a pop up text with smaller fontsand it must be to right."*
- **What Was Learned:**
  1. Top-line telemetry belonged in the primary instrumentation header bar (`renderHeaderWithStats`), freeing up menu body space and grouping campaign metadata cleanly with `|` delimiters (`Files Processed: X .pws | Y .txt | Days: Z`).
  2. Transition/launch messages should never write to fixed console cursor coordinates (e.g. `(4, 20)`) as this collides with and corrupts bottom-pinned footers or menu items. All feedback messages must be rendered inside dedicated framed pop-up modal boxes.
  3. Menu item explanations should not be placed underneath menus where they take vertical space or push list items down. In an 80-column console, menus occupy the left half (cols 2-40), leaving the right half (cols 42-78) ideal for a compact, framed `TOPIC EXPLANATION` pop-up box that updates dynamically with selection.
- **What NOT to Repeat:**
  - DO NOT write transient status messages directly over screen content using fixed coordinates; always use framed modal pop-ups.
  - DO NOT crowd telemetry text into the menu selection body when it belongs in the top header instrumentation rule.
  - DO NOT stack topic explanations below menus when the right side of the screen provides a clean, non-disruptive panel for pop-up explanations.

### LL-019: Version 0.02 (Stable Alpha 1) Milestone Release & Time-Travel Preservation
- **Date:** 2026-09-20 16:15 UTC+3
- **Context:** User instructed:
  1. *"push everything to gh and mark this Version 0.02"*
  2. *"preserve this as a stable alpha 1"*
  3. *"docuemtn it as such"*
  4. *"it must be backed up so that we can come to this version as a time travel back if future developments are not satisfactory."*
- **What Was Learned:**
  1. Alpha milestones representing a stable, fully-tested baseline must be preserved with multi-layered redundancy: immutable Git tags (`v0.02`, `v0.02-alpha.1`), a dedicated stable branch (`stable-alpha-1`), and a standalone physical archive (`backups/WITP-CIC2-v0.02-alpha1.zip`).
  2. Clear and explicit time-travel / rollback documentation must be provided directly in the repository and core docs so any developer or user can seamlessly revert the codebase to this exact operational state if future iterations introduce bugs or unsatisfactory changes.
- **What NOT to Repeat:**
  - DO NOT move to new features or major changes without first creating an immutable release tag and verified backup snapshot of known working states.
  - DO NOT rely solely on local unpushed Git history for milestone preservation.

### LL-020: Mandatory Upgrade, Backup, and Rollback Protocol
- **Date:** 2026-09-20 16:32 UTC+3
- **Context:** User instructed:
  1. *"baseline : add all of these options as a doc file to gh , under name how to roll back to Alpha 1"*
  2. *"from now on any upgrade will be done in this procedure"*
- **What Was Learned:**
  1. Rollback documentation must be codified as an independent, primary guide (`docs/how-to-rollback-to-alpha-1.md`) pushed to GitHub so that anyone accessing the repository has instantaneous visibility into how to roll back or time travel.
  2. The release and preservation lifecycle is now a non-negotiable standard baseline procedure. Any future upgrade must systematically execute the full 6-step cycle: automated verification -> core docs synchronization -> offline backup creation -> git tagging/branching -> push to GitHub -> rollback documentation maintenance.
- **What NOT to Repeat:**
  - DO NOT ship upgrades informally without generating the physical backup archive and pushing immutable Git release tags.
  - DO NOT omit explicit rollback instructions when updating milestone versions.

### LL-021: Field Manual WITP:AE MANUAL Header & Interactive Wiki HTML Integration
- **Date:** 2026-09-20 16:45 UTC+3
- **Context:** User instructed:
  1. *"u will replace the current content under that menu"*
  2. *"put the file at C:/Users/ozgur/Documents/WITP-CIC ASSETS/docs/rag/witp-ae/witp_ae_manual_wiki.html under the [ FIELD MANUAL: WITP:AE GAME MANUAL BROWSER ] Menu."*
  3. *"[ C H A P T E R   B R O W S E R   &   R A G   S E A R C H ] -> [ WITP:AE MANUAL ]"*
  4. *"delete the line that start with Index : ...."*
- **What Was Learned:**
  1. The user prefers the rich, 2-column table-of-contents HTML wiki (`witp_ae_manual_wiki.html`) as the primary game manual reference over unformatted raw terminal chapter dumps. The console application acts as a seamless launchpad into the interactive manual via native Win32 `ShellExecuteA`.
  2. Header precision is critical: `C H A P T E R   B R O W S E R   &   R A G   S E A R C H` was renamed to `W I T P : A E   M A N U A L` to accurately represent the module. The redundant `Index: Chapter X of 24...` line was removed to maintain clean terminal minimalism.
- **What NOT to Repeat:**
  - DO NOT display raw, unformatted multi-thousand line dumps when a curated interactive wiki document exists.
  - DO NOT retain outdated or redundant status lines (`Index: ...`) after a screen's data model changes.

### LL-022: In-Terminal Ingestion of WITP:AE Manual via Hierarchical JSON
- **Date:** 2026-09-20 17:20 UTC+3
- **Context:** User instructed:
  1. *"delete options 2 and 3"*
  2. *"dont use the manual wiki as a seperate file, ingest it to the current program as a json file etc ( u decide )"*
  3. *"the wiki manual must have the same UI look as the program"*
  4. *"no need to write Table contents at the top"*
  5. *"First menu shows X.0 level topics , when I select and enter , then I goto the next X.1 level and so on ..."*
- **What Was Learned:**
  1. The user expects full self-contained in-terminal execution matching the Nostromo green phosphor styling. External browser launching must not be used when the user requests an integrated in-app manual browser.
  2. Curated manual text is ingested into a high-performance hierarchical JSON schema (`assets/manual/witp_ae_manual.json`) containing all 15 operational chapters (`1.0` through `15.0`) and 414 nested section nodes.
  3. A clean recursive descent JSON parser built directly into the C++ application enables zero-dependency, ultra-fast manual indexing.
  4. Hierarchical terminal UX flows seamlessly: root menu displays `X.0` level topics, selecting a chapter drills into `X.1` topics, sub-sections drill into `X.1.1` topics, and leaf nodes open the Nostromo article reader with line scrolling and page jumps.
  5. The right-aligned `[ TOPIC EXPLANATION ]` pop-up box dynamically renders section summaries in dimmed phosphor (`ConsoleColor::DarkGreen`) beside the double-spaced inverted menu cursor.
  6. No redundant "Table of Contents" label or `Index : ...` counter is rendered at any menu level.
### LL-023: Cumulative Master Implementation Plan & Zero Hallucination Standard
- **Date:** 2026-09-20 20:31 UTC+3
- **Context:** User instructed:
  1. *"baseline : keep all implementation plans in the Master FIle : Master Implemantation plan ."*
  2. *"Must be a cumulative file showing every phase of implement done or will be done"*
  3. *"If there is a clash , ask me"*
  4. *"never hallucinate"*
  5. *"if in doubt ask me"*
  6. *"this is part of the core documentation"*
- **What Was Learned:**
  1. All implementation plans must be maintained cumulatively in `docs/master_implementation_plan.md` as an authoritative Core Doc. Each phase (past, present, future) remains documented in one permanent location rather than being overwritten or discarded.
  2. Zero Hallucination Standard: Every metric, rule, and calculation must be grounded strictly in decompressed binary facts (`.pws`), official game manual documentation (`assets/manual/witp_ae_manual.json`), or plain-text PBEM turn reports.
  3. Strict Clash & Doubt Protocol: If there is any discrepancy, contradiction, or ambiguity across requirements, the agent must immediately stop and ask Ozgur rather than making assumptions or improvising.
- **What NOT to Repeat:**
  - DO NOT wipe or replace implementation plans between phases; maintain the cumulative master record.
  - DO NOT invent gameplay mechanics, formulas, or binary assumptions; ground every model in verified telemetry.
  - DO NOT guess when facing ambiguous requirements or clashes; stop and ask Ozgur directly.

### LL-024: Terminal Screen Redraw Loops vs Input Polling & Zero Blinking Policy
- **Date:** 2026-09-20 21:05 UTC+3
- **Context:** User stated: *"1. The blinking is faulty, eliminate any blinking. from now on , it will be done only for items I specificly say. 2. Delete Ghost Fleet Counter"*.
- **What Was Learned:**
  1. **Console Redraw Loop Bug:** When an interactive screen loop performs `Console::clear()` and redraws the entire interface inside an outer loop with `if (key == 0) { sleepMs(20); continue; }`, the screen is cleared and redrawn 50 times every second. Even if the console write is fast, this produces severe full-screen flickering and perceived "faulty blinking".
  2. **Standardized Inner Input Polling Pattern:** Screen rendering loops MUST separate the screen render from input polling. The outer loop draws once, and an inner loop polls `Console::readKey()` with `sleepMs(20)` until an actual keystroke is detected (`key != 0`). Only state-changing key events break out to trigger a redraw.
  3. **Universal Zero Blinking Policy:** Absolutely zero elements in the application may blink (no blinking prompts, no blinking cursor, no pulsing highlights, no oscillating headers) unless Ozgur explicitly instructs a specific item to blink.
  4. **Ghost Fleet Counter Deletion:** The Ghost Fleet Counter metric in the War Room was deleted per user directive, streamlining Tab 1 into 5 crisp intelligence summary cards and refocusing Model E strictly on Battle Damage Assessment (BDA) and claims corrections.
- **What NOT to Repeat:**
  - DO NOT place `Console::clear()` inside an unchecked timer or unblocked key polling loop that redraws at 50 FPS.
  - DO NOT add blinking prompts or pulsing text anywhere in the application without Ozgur's explicit request.
  - DO NOT reintroduce Ghost Fleet metrics or ghost ship OOB subtraction in the War Room.

### LL-025: Standard Menus over Tabs, Interactive Pop-Up Traversal & Analytical Reasoning
- **Date:** 2026-09-20 21:18 UTC+3
- **Context:** User directed: *"Delete the Tab structure , we will use a Standart Menu here ... WHEN USER PRESSES A MENU , HE IS TAKEN TO THE SUB MENU OF THAT TOPIC ... Every Item in the pop UP must be traversed by cursor keys ... When I come over a bullet and press enter u must show the reasoning for this line ... Correct the bottom phrase to PRESS ESC TO RETURN"*.
- **What Was Learned:**
  1. **User Preference for Standard Hierarchical Menus:** Horizontal tab structures are replaced with clean, vertical Standard Menus that drill down into dedicated sub-screens. This preserves clean Nostromo visual symmetry and eliminates cluttered horizontal top bars.
  2. **Two-Tier Interactive Navigation in Pop-Ups:** In pop-ups and manifest dialogs, static text blocks must not be used. Every item/bullet must be an interactive list element navigable with `[UP/DOWN]` cursor keys with inverted highlight.
  3. **Line-by-Line Analytical Reasoning:** Pressing `[ENTER]` on any bullet must never just dismiss the dialog; it must open a dedicated Reasoning Modal revealing the underlying telemetry source, mathematical scoring formula, coordinate bearings, and tactical deductions for that specific line.
  4. **Standardized Return Prompt:** The exit prompt across all sub-screens and modals must strictly read `PRESS ESC TO RETURN`.
- **What NOT to Repeat:**
  - DO NOT implement horizontal tab interfaces where hierarchical standard menus are expected.
  - DO NOT render static, unnavigable bullet lists in pop-up dialogs.
  - DO NOT omit analytical reasoning on `[ENTER]` when displaying intelligence manifests.
  - DO NOT use inconsistent return phrases (e.g. "Press Any Key" or "Return to Dashboard"); strictly use `PRESS ESC TO RETURN`.

### LL-026: Geospatial Sighting Last-Seen Tracking, Anti-Bullshit Reasoning & Border Stability
- **Date:** 2026-09-20 21:40 UTC+3
- **Context:** User instructed:
  1. *"Date is missing - this should be the date(s) this TF was seen, In this screen date must show the last time this TF was seen"*
  2. *"When the popup comes - THe text inside the pop is cutoff : COORDINATES: Hex 179,103 | Range/Bearing: 165 NM Brg 346 (N) from Pea... <- Make sure the entire text is on the screen."*
  3. *"Add a confidence Meter Bar , show a %XX confidence level for >=90% make it blink slowly. Confidence level must be retrieved from the reports and pws file(s)!"*
  4. *"TACTICAL REASONING: and INTERCEPT SIGNIFICANCE: must NOT BE Bullshit, dont write anything that doesnt mean anything !. LEave blank for now with *-**-**-**-**-* text"*
  5. *"POP TEXT BOX come scrambled bc of the highlight : when I traverse cursor pop up box border is scrambled. Can u make it a consistent border that wont change."*
- **What Was Learned:**
  1. **Last-Seen Date Ingestion:** PBEM turn export filenames often omit turn dates (e.g. `aoperationsreport.txt`). The file scanner must parse the report header line (`OPERATIONAL REPORT FOR Dec 14, 41`) to extract true turn dates, group recurring sightings by contact/hex, and explicitly display the last time the TF was seen (`lastSeenDate`) so the `[ DATE ]` column is never blank.
  2. **Word-Wrapping Over Truncation:** Modal text must wrap across clean continuation lines (indented 4 spaces) instead of slicing with `...`. Every character of coordinates, bearings, and descriptions must remain fully visible.
  3. **Slow Blinking Exception for High Confidence:** While the baseline policy prohibits arbitrary blinking, an explicit exception was mandated for Confidence Meter Bars with `>=90%` confidence. This is implemented via in-place cursor updates (toggling every 600ms between `Inverted` and `LightGreen`) without clearing the screen, preventing any screen flicker.
  4. **Strict Anti-Bullshit Policy for Reasoning Fields:** Boilerplate placeholder text in analytical deductions is strictly forbidden. Tactical reasoning and intercept significance fields must remain explicitly blank as `*-**-**-**-*-*` until grounded AI/predictive models are connected.
  5. **Rock-Solid Modal Border Stability:** Box drawing in navigable pop-ups must anchor left borders (`| `), content strings (padded to exact inner width), and right borders (` |`) to fixed coordinate columns. Redrawing must never compute variable line lengths that shift or scramble box frames during cursor traversal.
- **What NOT to Repeat:**
  - DO NOT display empty date columns when header timestamps can be extracted from report contents.
  - DO NOT truncate coordinate or bearing text with `...` in reasoning modals.
  - DO NOT write placeholder filler or ungrounded interpretations in tactical reasoning fields; use `*-**-**-**-*-*`.
  - DO NOT allow inverted highlight lines to alter the string width or scramble modal borders during cursor traversal.

### LL-027: Deterministic Numerical Confidence Engine vs Placeholder Hallucinations
- **Date:** 2026-09-20 22:08 UTC+3
- **Context:** User instructed:
  1. *"confidence levels must be a percentage and derived numerically from the pws and txt files . DOnt bullshit , ask me how I do this , say I dont know this !"*
  2. User provided the authoritative 4-phase algorithmic logic:
     - Conceptual structures: `RawReport`, `ConfidenceReport`, `EntityMap`, `HexMap` (integer keyed $X \times 1000 + Y$).
     - Phase 1: Fast non-regex parsing (`std::string::find`), report type classification, base scoring (Radio=30, Garrison=50, Movement=65, Location=75), domain tagging (`AIR`, `NAVAL`, `LAND`).
     - Phase 2: Corroboration bonuses (+15 for repeated entity, +10 for repeated hex).
     - Phase 3: Time staleness decay ($\Delta t=0 \to 1.0$, $\Delta t=1 \to 0.8$, $\Delta t>1 \to 0.5$) and clamping strictly between $0 - 100\%$, mapped to standardized tiers (`UNCONFIRMED`, `PROBABLE`, `CONFIRMED`, `VERIFIED`).
     - Phase 4: Engine memory pooling and integer hex comparison.
  3. *"add all of this to the scope before moving on to modift coding"*
- **What Was Learned:**
  1. **Strict Prohibition of Invented Placeholder Math:** Never output descriptive or hand-waved confidence justifications (such as *"High confidence based on multi-bearing signal triangulation"*). All confidence ratings must be calculated strictly through verifiable arithmetic algorithms grounded in report and binary telemetry.
  2. **Ask Rather Than Guess ("Say I Don't Know This"):** When an operational metric's exact derivation logic is unspecified, explicitly pause, state "I don't know this", and request Ozgur's authoritative formula. Never extrapolate or improvise scoring logic.
  3. **Mandatory Scope Documentation Before Implementation:** When a mathematical or algorithmic specification is supplied by Ozgur, it must be completely incorporated into the business scope (`docs/scope-v1.md`) and Core Docs before modifying any application source code.
  4. **High-Performance C++ Architecture:** Standard `std::regex` must be avoided in favor of `std::string::find` and `substr`. Coordinates must be indexed as integers (`hexX * 1000 + hexY`) to ensure rapid $O(1)$ map lookups with zero heap string allocations.
- **What NOT to Repeat:**
  - DO NOT output subjective text strings for intelligence confidence.
  - DO NOT improvise or invent formulas when the mathematical derivation is unknown; ask Ozgur directly.
  - DO NOT touch production code before codifying the algorithm in `docs/scope-v1.md`.
  - DO NOT use slow string keys or heavy regex in turn log parsing loops.

### LL-028: Corroboration Scope Separation, Visual Recon & Modal Math Breakdown
- **Date:** 2026-09-20 22:27 UTC+3
- **Context:** In response to agent clarification questions, Ozgur specified:
  1. *Corroboration Scope:* "Option B (Per-Turn Only) for the Parser, but handled by the Game State for persistence. The parser should remain 'dumb and fast,' only looking at the current asigint.txt file. Cross-turn persistence is the job of the Game State Manager, not the text parser."
  2. *Visual Recon vs SIGINT:* "Treat them as a distinct VISUAL_RECON report type, and only allow Hex Corroboration, not Entity Corroboration. Set VISUAL_RECON Base Score to 80 ... If SIGINT reports 'Radio traffic at 80,77' and Visual Recon reports '12 ships at 80,77', they corroborate the Hex. Both reports should receive the +10 Hex Corroboration Bonus."
  3. *Pop-Up Presentation:* "Clean Header, Full Math in the 'Reasoning' Body. The Header (Clean): CONFIDENCE: 90% [VERIFIED] ... Below the header, provide the breakdown, but translate the raw numbers into plain English alongside the math."
- **What Was Learned:**
  1. **Stateless Parser vs Stateful Game State:** Keeping the file parser strictly single-turn avoids massive memory bloat and cache invalidation overhead. The parser evaluates entity/hex repetition strictly within the active turn file. Long-term persistence is handled by the Game State Manager, which updates timestamps and resets Time Decay to 1.0 when contacts recur.
  2. **Multi-Source Hex Cross-Corroboration:** Visual recon (location-precise, entity-poor) and SIGINT (entity-precise, location-imprecise) complement each other. Visual recon populates `HexMap` (not `EntityMap`), allowing radio intercepts and recon flights to mutually corroborate hex activity with $+10$ bonuses.
  3. **Tiered UI Transparency:** Headers must remain clean and punchy (`CONFIDENCE: 90% [VERIFIED]`). Granular mathematical transparency belongs in the reasoning body, translated into plain English formulas (`Base Score: 75 (Unit Located) + Corroboration: +15 ...`).
- **What NOT to Repeat:**
  - DO NOT make text parsers stateful across historical turns.
  - DO NOT omit visual recon from spatial hex corroboration.
### LL-029: Standardized Binary Compilation Naming Format & Milestone Progression
- **Date:** 2026-09-20 23:02 UTC+3
- **Context:** User instructed:
  1. *"we are Alpha 2 from  now on"*
  2. *"Also , baseline : - when compiling a new exe the naming format is witp-cic-[mainversion id]-[phaseid]-[built no]"*
  3. *"main version is V.XX"*
  4. *"phase id is alpha , beta etc"*
  5. *"built no , starts from 0 for each new main phase id"*
- **What Was Learned:**
  1. **Standardized Binary Nomenclature:** All compiled release binaries must strictly adhere to the structured naming pattern `witp-cic-[mainversion id]-[phaseid]-[built no]` (e.g. `witp-cic-v0.02-alpha2-0.exe`). This eliminates ambiguous binary names and ensures full build-level traceability.
  2. **Build Counter Lifecycle:** The build number is strictly reset to `0` whenever a new main phase ID (e.g. Alpha 2, Beta 1) is entered, incrementing sequentially for subsequent builds within that phase.
  3. **Milestone Progression (Alpha 2):** Version 0.02 officially transitions into Alpha 2, preserving Stable Alpha 1 as an immutable reference baseline.
- **What NOT to Repeat:**
  - DO NOT generate generic unversioned executable files without the full version-phase-build tuple.
### LL-030: Naval Unit Capability Registry, Auxiliary / Tender Doctrine & Predictive Grounding
- **Date:** 2026-09-21 13:27 UTC+3
- **Context:** User instructed:
  1. *"An important prerequisite of making sense of the information collected is to know what each unit type can do."*
  2. *"I will give u this information in parts. Analyse them , and base your reasoning on these."*
  3. *"if u see any conflict and part u dont understand ask"*
  4. *"all of the info will goto the documentation for the scope and other related places like the Field Manual."*
  5. *"From now on make your predictive work to include these info"*
  6. *"ignore the numbers in brackets [ ]"*
- **What Was Learned:**
  1. **Doctrinal Grounding of AI & Heuristics:** Tactical reasoning and predictive algorithms cannot be generic mathematical abstractions. They must be grounded in the exact operational roles and capabilities of historical naval vessels and auxiliaries in WITP:AE (e.g. distinguishing APD commando insertions from APA heavy assault echelons; recognizing AS forward submarine bases; distinguishing AO underway replenishment from commercial TK point-to-point hauling; recognizing AR/ARD floating repair bases).
  2. **Authoritative Taxonomy Ingestion:** All 7 functional classes (Major Surface Combatants, Surface Escorts, Mine Warfare, Amphibious & Troop Transports, Cargo/Logistics Freighters, Mobile Tenders & Auxiliaries, Submarines) and detailed Auxiliary / Tender Doctrine mechanics must be codified across Core Docs and the in-app Field Manual (`assets/manual/witp_ae_manual.json` sections `1.15` and `1.16`).
  3. **Strict Formatting Standard:** All reference numbers in brackets `[ ]` must be stripped cleanly. Zero assumptions, zero hallucinations, zero unverified mechanics.
- **What NOT to Repeat:**
  - DO NOT generate generic tactical reasoning that ignores vessel class capabilities.
  - DO NOT retain bracketed citations `[...]` in documentation or UI text.
  - DO NOT treat auxiliary tenders (AS, AD, AV, AR, ARD, AE, AO) as generic supply ships; each has distinct mechanical and predictive significance.

### LL-031: Base Location Registry, Dynamic Controller State & Hex Grid Precision
- **Date:** 2026-09-21 14:45 UTC+3
- **Context:** User instructed:
  1. *"I now give u the `locations` on the map with coordinates. These are `base locations`. These locations may house units , ships or planes. Obviously only the ones near a sea can house ships."*
  2. *"THe region name like `japanese empire` is only for the beginning and bases may change hands."*
  3. *"ingest data below and use it in predictive analysis and anyother related area. add to documentation"*
- **What Was Learned:**
  1. **Theater-Wide Hex Ground Truth (Bases DO NOT Move):** The WITP:AE map encompasses 1,330 authentic base locations with explicit, immutable $(X, Y)$ hex coordinates. **Base locations DO NOT move!** They are permanent physical geography. Hardcoded destination coordinates in predictive models (e.g. Model A, Model B) must be completely eliminated in favor of authoritative lookups against `BaseLocationRegistry`.
  2. **Housing Capabilities & Maritime Restriction:** Base locations house units, ships, or aircraft, but only coastal locations adjacent to the ocean house ships (`isPort == true`). Inland bases (e.g. Kofu, Maebashi, Asahikawa, inland China/Manchukuo cities) only house ground and air units.
  3. **Dynamic Controller Architecture (Bases Fixed, Controllers Change):** While base locations and their $(X, Y)$ coordinates are strictly stationary and never move, bases change hands dynamically during gameplay (e.g. captures reported in operational events). The engine mutates live base controller ownership (`updateBaseController()`) while preserving physical coordinate immutability and historical starting baselines.
  4. **Holistic Predictive Grounding:** Integrating the 1,330 base registry elevates Model A (amphibious assault destination hexes), Model B (nearest candidate base within dead reckoning radii), Model D (turnaround and pause bases), Tab 2 geospatial bearing calculations, and Dossier entity lookups to full theater coverage.
- **What NOT to Repeat:**
  - DO NOT ever treat base coordinates as variable or moving: **base locations DO NOT move!** Only units, ships, and planes move.
  - DO NOT hardcode coordinate tables in individual predictive models when an authoritative 1,330-base database exists.
  - DO NOT assume base ownership is immutable; controllers change hands dynamically upon capture events.
  - DO NOT allow inland bases to house naval shipping.

### LL-032: Master Theater Hex Map Storage & Geospatial Calibration
- **Date:** 2026-09-21 15:00 UTC+3
- **Context:** User instructed:
  1. *"this is the hex map where u can see the numbers. top left is 0,0. Store it for future use."*
  2. *"once done, suggest some uses for it using the current scope"*
- **What Was Learned:**
  1. **Ground Truth Visual Anchor:** The official high-resolution Gary Grigsby's War in the Pacific: Admiral's Edition theater hex map provides the definitive geographic baseline with top-left origin $(0, 0)$.
  2. **Asset Organization:** Stored under `assets/maps/witp_ae_full_hex_map.jpg` and mirrored under `docs/witp_ae_full_hex_map.jpg` for offline accessibility and build independence.
  3. **High-Impact Use Cases in Scope:**
     - Hex-level terrain and maritime classification (distinguishing land from ocean hexes).
     - Obstacle-aware maritime pathfinding in Model B to prevent fleet trajectories from clipping over islands/peninsulas.
     - Strategic chokepoint interdiction modeling in Model G.
     - Air search radius overlays and localized CP437 ASCII / sector minimaps for Tab 2.
- **What NOT to Repeat:**
  - DO NOT lose or misplace uploaded map reference assets; store permanently in versioned directories.
  - DO NOT project naval vectors linearly through landmasses when terrain ground truth is available.

### LL-033: Binary Build Counter Auto-Increment Baseline
- **Date:** 2026-09-21 15:03 UTC+3
- **Context:** User instructed:
  1. *"base line when compiling the new exe increae the build number by 1"*
- **What Was Learned:**
  1. **Strict Monotonic Build Counter:** The build number in `witp-cic-[mainversion id]-[phaseid]-[built no]` is strictly incremented by `1` every time a new binary is compiled.
  2. **Sequential Progression:** `witp-cic-v0.02-alpha2-0.exe` (initial Alpha 2 baseline) $\to$ `witp-cic-v0.02-alpha2-1.exe` (first subsequent build) $\to$ `witp-cic-v0.02-alpha2-2.exe`, etc.
  3. **Canonical Mirror Invariance:** `dist/WitP-CIC.exe` is continuously updated as a mirror of the latest build for test harnesses and default execution.
- **What NOT to Repeat:**
  - DO NOT overwrite or reuse the previous build number when compiling a new binary.
  - DO NOT omit incrementing the build counter on new releases.

### LL-034: Six Mandatory Geospatial Predictive Intelligence Applications
- **Date:** 2026-09-21 15:05 UTC+3
- **Context:** User instructed:
  1. *"add the following to the scope: 1. Maritime Obstacle Avoidance & Nautical Pathfinding ... 2. Strategic Chokepoint & Convoy Interdiction ... 3. In-Terminal Tactical Mini-Radar / Sector Map ... 4. Land-Based Air Search & Strike Umbrellas ... 5. Amphibious Beachhead Approach Lanes ... 6. Hex Grid Calibration & Coordinate Ground-Truth"*
- **What Was Learned:**
  1. **Geospatial Intelligence Synergy:** The combination of the 1,330 base location database, PBEM contact reports, and the master hex map raster transforms static coordinate reporting into dynamic geospatial predictive intelligence.
  2. **Pathfinding & Reality Constraint (Model B):** Linear dead reckoning without terrain masking creates immersion-breaking anomalies (ships projected over land). A binary collision grid allows A* maritime pathfinding through authentic straits and coastal passages.
  3. **Operational Chokepoints (Model G):** Strategic narrows create mandatory transit bottlenecks where intelligence can forecast exact ambush opportunities.
  4. **Multi-Domain Air Umbrellas (Models A & F):** Land-based air radius rings establish safe approach corridors and flag high-threat air spaces.
  5. **In-Terminal Minimap Rendering (Tab 2):** Nostromo-styled CP437 ASCII/ANSI sector maps bring immediate visual orientation to tabular SIGINT sighting rows.
- **What NOT to Repeat:**
  - DO NOT omit terrain obstacle constraints when calculating dead reckoning intercepts.
  - DO NOT treat chokepoint transits as unconstrained open-ocean movement.

### LL-035: Opponent-Only Asset Display in Intelligence War Room (Exclusion of Friendly Losses)
- **Date:** 2026-09-21 18:15 UTC+3
- **Context:** User provided screenshot of `[ BATTLE DAMAGE ASSESSMENT & CLAIMS REVISION REGISTER ]` showing friendly Allied vessels (e.g. *AO Platte, CA New Orleans, BB Oklahoma, BB California, BB Maryland, BB Nevada, PC Reliance, BB Arizona, HDML 1062, HDML 1063*) and instructed:
  > *"THis menu must be only for the other side ( Japanese in this case ) assets. I already know what is happening to me."*
- **What Was Learned:**
  1. **Intelligence Rationale:** The War Room / Intelligence CIC exists to give the commander visibility into **enemy** posture, movements, battle damage, and capabilities. The player already has complete ground truth for their own friendly fleet through operational reports, damage reports, and ship listings.
  2. **Zero Pollution of Enemy Intelligence:** Displaying friendly Allied casualties in the BDA/claims register pollutes the intelligence picture and destroys the CIC fantasy. Friendly losses must never appear in enemy intelligence tracking screens.
  3. **Multi-Source Side Disambiguation:**
     - In `combatreport_*.txt`, both `Allied Ships` and `Japanese Ships` sections exist. `ev.isAllied` must be set dynamically according to the section header, rather than hardcoding `true`.
     - In `Combat_Events_*.txt`, sinkings (`... sinks....`) must be classified as Japanese vs Allied via known ship naming markers, prefix patterns, and cached side tracking.
  4. **Strict Presentation Filtering:**
     - When commanding the Allied side, all BDA registers and Model E revision menus must strictly filter for `!c.isAllied` (Japanese assets only).
     - When commanding the Japanese side, filter for `c.isAllied` (Allied assets only).
     - If no enemy claims are present for the turn range, render `NO HOSTILE COMBAT DAMAGE CLAIMS RECORDED FOR THIS TURN RANGE.`
### LL-036: Bold Headers, Modal Screen Isolation & Off-By-One Wrapping Fix
- **Date:** 2026-09-21 18:27 UTC+3
- **Context:** User provided screenshot `media_1789996062653.png` of `[ REASONING & TELEMETRY EVIDENCE ]` modal in War Room showing:
  1. Underlying background screen text (`[`, `==`, `--`, `3`, `+`) bleeding through modal borders.
  2. Item title truncated with ellipses: `ITEM: Malaya Invasion Strike Force (Gulf of Siam / Kota Bharu) [Hex 5...`.
  3. Last line cut off abruptly at the right edge: `OPERATIONAL TARGET: Primary screening force for amphibious landings in ` with remainder dropped, annotated with a large red arrow.
  4. Instructed:
     > *"Make the Headers Bold. The text is cutoff across the box. Apply text wrapping. U can use smaller fonts in pop up boxes. This is a pgram wide problem."*
- **What Was Learned:**
  1. **Win32 & ANSI VT Bold Headers:** Terminal headers need visual punch and hierarchy. By enabling `ENABLE_VIRTUAL_TERMINAL_PROCESSING` (`0x0004`) in `Console::init()`, the console cleanly processes ANSI `\033[1m` bold styling. All screen headers, section headers, and modal box titles must use bold typography.
  2. **Screen Isolation on Modal Launch:** Modals that occupy most of the viewport (like `renderReasoningModal`) must call `Console::clear()` upon entering. Without an explicit clear, characters from the previous screen outside the modal's margins remain visible, causing messy edge bleed.
  3. **Zero Text Truncation — Title & Body Wrapping:** Slicing titles or descriptions with `...` or fixed-width substrings is forbidden. Modal titles must wrap cleanly across multiple lines (in inverted phosphor) if they exceed the inner box width.
  4. **The Off-By-One Wrapping Drop Bug:** In `renderReasoningModal`, `contentHeight` was calculated as `wrappedLines.size() + 4` while `curRow` advanced by 5 (when confidence bar was present). Computing `maxLines = (startY + contentHeight) - curRow` resulted in `maxLines = wrappedLines.size() - 1`, consistently omitting and dropping the final wrapped line. Row layouts must directly loop over all lines in `wrappedLines` without dropping content.
  5. **Maximized Pop-up Geometry:** Using wide modal frames (width up to 78 columns, startX at 1) yields 74 interior columns, minimizing unnecessary line wrapping and keeping all content within standard terminal heights.
- **What NOT to Repeat:**
  - DO NOT omit `Console::clear()` when opening full-screen modals over populated screens.
  - DO NOT truncate item titles or descriptions with `...` when they can wrap.
  - DO NOT use off-by-one formula arithmetic for modal line bounds that drops the last line.
  - DO NOT render headers in plain non-bold font when bold styling is mandated.

### LL-037: Hierarchical Reasoning Modal Drill-Down, Merged Telemetry & On-Demand Confidence Computation
- **Date:** 2026-09-21 18:40 UTC+3
- **Context:** User provided screenshots `media_1789996080168.png` and `media_1789996091816.png` showing:
  1. Monolithic unscrollable text dump of all telemetry lines.
  2. Right border text cutoff (Label 1: `INTERDICTION REASONING: Prime ambush route for Allied submarine warfare` cutting off ` & carrier strike ops.`).
  3. Raw mathematical formulation lines cluttering the modal.
  4. Instructed:
     > *"Make header bold in the pop box. I should be able to traverse ovr the topics using cursor and when i press I must be able to see what led to this result. ie Localized position and Telemetry Source can be merged : pack all into one header and pressing on it reveals by which means and at what frequency it was seen until when. Confidence analysis computations must also be drill down, dont show it unless someone presses enter on it. Text is cut off across the pop up border. this is a common error, solve it program wide. User must be able to traverse over the items and if he presses enter over one, u must tell how you made this prediction in a pop up."*
- **What Was Learned:**
  1. **Two-Tier Interactive Modal Architecture:** Monolithic data dumps overwhelm the user and cause visual clutter. Transforming the reasoning modal into an interactive topic directory (Tier 1) traversed with `[UP/DOWN]` cursor keys allows clean modular organization. Pressing `[ENTER]` on a topic opens a focused drill-down modal (Tier 2) showing the granular evidence for that topic.
  2. **Merged Sighting & Telemetry Source:** Users think in terms of detection history. Merging `TELEMETRY SOURCE` and `LOCALIZED POSITION` into one unified topic reveals detection means, localized coordinates, sighting frequency, and chronology (dates seen).
  3. **On-Demand Confidence Computation:** Mathematical formulas (`Base Score`, `Corroboration`, `Time Decay`, `Final Calculation`, `Analyst Note`) must never be thrust upon the user on the primary view. They must be isolated into a dedicated topic that is only disclosed when the user explicitly requests it via `[ENTER]`.
  4. **Strict Word-Wrapping to Inner Width:** Text cutoff across box borders happens when strings exceed `innerWidth` without wrapping. By applying `wrapText(line, innerWidth, 4)` to every title and detail line, lines wrap safely with indentation, preventing border clipping.
- **What NOT to Repeat:**
  - DO NOT dump all reasoning lines into an unscrollable static modal.
  - DO NOT show raw mathematical calculations on top-level screens without user drill-down.
  - DO NOT print lines longer than `innerWidth` without word wrapping.
### LL-038: Turn Count vs. Item Count Header Conflation & Multi-Day Ingestion Assurance
- **Date:** 2026-09-21 18:54 UTC+3
- **Context:** User provided screenshot `media_1789996024650.png` showing:
  `[ NAVAL STRIKE CONCENTRATION CLUSTERS ]                               Turn: COUNT: 3`
  followed by 3 bullets, and instructed:
  > *"add todo list: Turn count on this turn is 3 , however at least 7 days must have been loaded it to the campaign vault file. Analyse then solve this. ALl the ingested data from all days must be processed. Make sure this is so"*
- **What Was Learned:**
  1. **Header Label Conflation:** In `src/ui/layout.cpp`, `Layout::renderHeader(screenTitle, turnDate)` blindly prepended `"Turn: "` to the second argument whenever it was non-empty. When `runNavigablePopUp` passed `"COUNT: " + std::to_string(bullets.size())`, the header rendered as `Turn: COUNT: 3`. The user reasonably inferred the system believed only 3 turns had been played or loaded, when in fact 3 was merely the number of strike clusters in the list!
  2. **Multi-Day Ingestion Telemetry Display:** The user needs transparent visibility into real campaign ingestion stats at all times. Headers must explicitly instrument both the campaign turn date, total days loaded, and the separate item count: `Turn: Dec 15, 1941 (8 Days Loaded) | ITEMS: 3`.
  3. **Robust Subtitle Formatting:** `Layout::renderHeader` must only prepend `"Turn: "` if the subtitle string does not already contain `"Turn:"`, `"ITEMS:"`, `"COUNT:"`, `"MATCHES:"`, or custom section markers.
  4. **Multi-Day Ingestion Verification:** Verified through campaign manifest inspection and data tracking that all 8 days in the campaign vault (`C:\WitP-CIC\Campaigns\D1.wic`) across 60 files (Dec 07, 08, 10, 11, 12, 13, 14, 15) are fully parsed and ingested by `WarRoomAnalytics`. Sighting tracks, High Value Targets, combat damage claims, and pilot kills span the complete chronology without dropping older data.
- **What NOT to Repeat:**
  - DO NOT hardcode `"Turn: "` prefixes in general-purpose layout renderers without verifying the string content.
### LL-039: Zero-Contamination Recovery Point Protocol Prior to Major Additions
- **Date:** 2026-09-21 20:41 UTC+3
- **Context:** User instructed:
  > *"I am about to make a very serious addition here. So backup everything and put a recovery point here. If next step is not satisfactory , we will return here. can u do this ? it is imperative that future addition and changes do not MIX with the previous effort at any possibility"*
- **What Was Learned:**
  1. **Proactive Checkpoint Isolation:** Before embarking on large architectural additions or sweeping new requirements, a bulletproof recovery checkpoint must be established across all dimensions: Git tag, dedicated Git branch, physical standalone zip archive, and an explicit rollback guide (`docs/recovery-point-alpha2-build5.md`).
  2. **Elimination of Cross-Contamination:** Future code, tests, and documentation must be cleanly isolated so that if the new addition is not satisfactory, a single command (`git reset --hard checkpoint-pre-major-addition-alpha2-build5` or `git checkout recovery-point-alpha2-build5`) completely wipes any newly created artifacts and restores the pristine baseline.
- **What NOT to Repeat:**
  - DO NOT begin major architectural additions without a dedicated recovery tag, branch, and offline physical archive.
  - DO NOT rely on memory or informal diffs to roll back complex features.

### LL-040: Scope Pruning & User-Driven Module Retirement
- **Date:** 2026-09-21 20:43 UTC+3
- **Context:** User instructed:
  > *"delete the following from todo and scope
  > Module C — Air Superiority & Attrition Forecasting
  > Module D — Logistical & Operational Pause Detection
  > Module E — Internal Security & Partisan Threat
  > no longer needed"*
- **What Was Learned:**
  1. **Prompt Scope Pruning:** As game intelligence requirements evolve, obsolete or undesired theoretical models (such as partisan threat analysis or air attrition curves that don't fit the player's tactical workflow) must be cleanly pruned from both active roadmap (`docs/todo.md`) and requirements specifications (`docs/scope-v1.md`).
  2. **Lettering and Entity Disambiguation:** When retiring legacy modules, ensure lettering clashes are disambiguated across the codebase and docs (e.g., in the War Room, Model E designates Battle Damage Assessment & Claims Revision / Sunk Set entity match, whereas the retired spec item was garrison/partisan threat).
- **What NOT to Repeat:**
  - DO NOT retain zombie requirements or orphaned to-do items when the user explicitly deprecates them.
  - DO NOT touch or delete unrelated modules (such as Model E Claims Revision or Model F Strike Concentration) when specific legacy modules are retired.

### LL-041: Complete Exclusion of Game File Writing & Modification
- **Date:** 2026-09-21 20:46 UTC+3
- **Context:** User instructed:
  > *"add this to TODO for next dev effort
  > delete these from todo and scope
  > Writing / modifying game files"*
- **What Was Learned:**
  1. **Reinforcement of Passive Read-Only Paradigm:** WITP-CIC must never be conceived as a save game editor, turn modifier, or injector. The user desires an analytical companion tool that operates strictly passively upon exports and save records.
  2. **Purging Deferred Modifiers:** Even deferred speculative items in future roadmaps (v1.1+) must not contemplate writing or altering game files. Removing them permanently aligns with the baseline integrity principle of zero file pollution and zero save corruption risk.
- **What NOT to Repeat:**
  - DO NOT list game file writing or modifying even as an "out of scope / deferred" future feature; it is permanently excluded by design.

### LL-042: Deletion of Battle Claims Corrections & Associated Model E
- **Date:** 2026-09-21 20:48 UTC+3
- **Context:** User instructed:
  > *"delete the following from todo and scope
  > Battle Claims Corrections: BDA claim vs later contradiction, displayed as revision under 4.2 Tab 1 — Intelligence Summary (§4.5)"*
- **What Was Learned:**
  1. **Direct Focus on Real-Time Intelligence:** Reconciling past damage claims vs later sightings (BDA contradiction tracking) is no longer part of the target product experience. The user prefers the Intelligence Summary to concentrate on direct tactical observations (HVTs, Strike Concentrations, Port Concentrations, Established Routes).
  2. **Coupled Engine Retirement:** When an interface intelligence feature is retired, its underlying predictive analytical engine (Model E entity reconciliation) must also be cleanly retired from active roadmap items to avoid lingering dangling tasks.
- **What NOT to Repeat:**
  - DO NOT retain background analytical engines when their presenting user interface feature has been explicitly deleted.

### LL-043: Phase Milestone Advancement Protocol
- **Date:** 2026-09-21 20:53 UTC+3
- **Context:** User instructed:
  > *"from now label phase as alpha3"*
- **What Was Learned:**
  1. **Phase Lifecycle Transition:** Advancing phase IDs (Alpha 2 -> Alpha 3) signals a major structural milestone shift.
  2. **Build Number Reset:** In accordance with §1.13, when advancing phase ID, the build counter resets strictly to `0` (`witp-cic-v0.02-alpha3-0.exe`).
- **What NOT to Repeat:**
### LL-044: WPF Key Event Tunneling & PreviewTextInput Routing
- **Date:** 2026-09-21 22:15 UTC+3
- **Context:** In `MainWindow.xaml.cs`, marking `e.Handled = true` unconditionally in `Window_PreviewKeyDown` completely blocked `PreviewTextInput` / `TextInput` from receiving alphanumeric characters, breaking password entry and search fields.
- **What Was Learned:**
  - In WPF, `PreviewKeyDown` runs before `PreviewTextInput`. If marked handled, character input events are suppressed entirely.
  - When text input is active (`_screenManager.IsAwaitingTextInput`), alphanumeric keys must NOT be marked handled in `PreviewKeyDown`, allowing `PreviewTextInput` to capture characters cleanly. Special action keys (`[ENTER]`, `[ESC]`, `[BACKSPACE]`, `[SPACE]`) are handled explicitly.
- **What NOT to Repeat:**
  - DO NOT unconditionally swallow all key events in `PreviewKeyDown` when user text input fields are active.

### LL-045: Single-Instance Process Management During .NET Publish
- **Date:** 2026-09-21 22:12 UTC+3
- **Context:** `dotnet publish` failed with `System.IO.IOException: The process cannot access the file ... because it is being used by another process` because a running instance of `MUTHUR6000.exe` was active.
- **What Was Learned:**
  - Always terminate any active instances of the target executable before running `dotnet publish`.
### LL-046: Alpha 4 Binary Naming Convention & Phase Transition
- **Date:** 2026-09-21 23:32 UTC+3
- **Context:** User instructed:
  > *"it is witp-cic-muthur-alpha4-0 where alpha is the new phase name 0 is the 1st build and will increment by 1"*
- **What Was Learned:**
  - With the transition to standalone repository `witp-cic-muthur`, the phase designation is **Alpha 4** (`alpha4`).
  - Executables follow `witp-cic-muthur-alpha4-[build#].exe`, starting at `0` for the first build (`witp-cic-muthur-alpha4-0.exe`) and incrementing by 1 for each subsequent build in this phase, with canonical mirrors to `dist/WitP-CIC.exe` and `dist/MUTHUR6000.exe`.
### LL-047: WPF Single-File Native Library Self-Extraction Requirement
- **Date:** 2026-09-21 23:35 UTC+3
- **Context:** `witp-cic-muthur-alpha4-0.exe` threw `System.DllNotFoundException` at `MS.Internal.WindowsBase.NativeMethodsSetLastError.SetWindowLongPtrWndProc` when launched as a single-file executable.
- **What Was Learned:**
  - In .NET 8 WPF single-file deployments, WPF depends on native C++ runtime binaries (`PresentationNative_cor3.dll`, `wpfgfx_cor3.dll`, `PenImc_cor3.dll`).
  - Without `<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>` and `<IncludeAllContentForSelfExtract>true</IncludeAllContentForSelfExtract>` in `.csproj`, WPF cannot resolve native Win32 window subclassing functions, causing `DllNotFoundException` on window creation.
- **What NOT to Repeat:**
  - DO NOT omit `<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>` in WPF single-file publish configurations.

### LL-048: Core Development, Testing & Scope Rigor Baseline Mandate
- **Date:** 2026-09-21 23:48 UTC+3
- **Context:** User codified the 7 fundamental baseline engineering rules:
  > *"baseline :
  > 1. always test before delivery
  > 2. dont hallucinate
  > 3. do not finish until tests run ok
  > 4. test results must be strictly covering the wanted output
  > 5. when new features are added or corrections are made , align it with the current scope , if there are clashes ask, if there is a part u dont understand ask , 
  > 6 everything that is added to functionality must be stored in the documentation in detail - fool proof 
  > 7, always watch for logic clashes in the algorithm and alert me"*
- **What Was Learned:**
  1. **Strict Pre-Delivery Verification:** Never deliver changes or binaries without running the full automated test suite and performing binary smoke tests.
  2. **Zero Hallucination Standard:** Never speculate on data formats or requirements. Ground all deductions in verified telemetry and official rules.
  3. **Uncompromising Test Pass Gate:** Never declare a task complete until 100% of tests pass cleanly.
  4. **Strict Output Coverage:** Test suites must strictly validate the intended visual and functional outputs.
  5. **Scope Alignment & Active Clash Clarification:** Align every change with `docs/scope-v1.md`. If anything is ambiguous or conflicting, ask Ozgur immediately.
  6. **Exhaustive Foolproof Documentation:** Document all functionality, architecture, and behavior in full, foolproof detail.
  7. **Proactive Logic Clash Alerting:** Actively audit algorithms and state machines for logic contradictions and alert Ozgur immediately.
- **What NOT to Repeat:**
  - DO NOT declare a task done with failing or unrun tests.
  - DO NOT guess or hallucinate when requirements are unclear.
  - DO NOT proceed when logic or scope clashes exist without asking Ozgur.

### LL-049: Elimination of Redundant Binary Mirrors & Aliases
- **Date:** 2026-09-21 23:58 UTC+3
- **Context:** Agent was generating two duplicate mirror binaries (`dist/WitP-CIC.exe` and `dist/MUTHUR6000.exe`) alongside the versioned executable `dist/witp-cic-muthur-alpha4-[build#].exe`. Ozgur asked:
  > *"why is 2 ? why do we need mirros ... yes and dont make the same mistake again"*
- **What Was Learned:**
  1. **Strict Single-Binary Output:** Never produce duplicate mirror files or legacy alias copies (e.g. `WitP-CIC.exe` or `MUTHUR6000.exe`).
  2. **Disk & Cleanliness Hygiene:** Each build must output ONLY the single, authoritative, versioned binary matching the established naming convention: `dist/witp-cic-muthur-alpha4-[build#].exe`.
  3. **No Redundant Aliases:** The test suite and runtime do not need mirrors; generating duplicate 70 MB binaries creates clutter and wastes storage.
- **What NOT to Repeat:**
  - NEVER copy or mirror the output binary to `WitP-CIC.exe` or `MUTHUR6000.exe`.
  - NEVER generate redundant binary copies in `dist/`. Only generate the single versioned `witp-cic-muthur-alpha4-[build#].exe`.

### LL-050: In-Terminal Unicode Graphing & Monospace Box Straight Border Mandate
- **Date:** 2026-09-22 21:45 UTC+3
- **Context:** User requested:
  > *"implement Option 1 (In-Terminal Unicode & Character Graphing) the ADR 64. Make sure that right side of the graph box is always straight and when done add everything to documentation & backup"*
- **What Was Learned:**
  1. **Strict Monospace Column Integrity:** In console/TUI windowing systems with box-drawing frames (`╔═╗`, `║ ║`, `╚═╝`), all rows enclosed within vertical side borders `║` MUST have identical display column widths. Any variation in character count or presence of wide characters causes the right border `║` to bend, indent, or wobble.
  2. **Single-Width Monospace Glyphs:** When generating in-terminal graphs, use only standard single-width characters (`█`, `░`, `▓`, `■`, `▲`, `▼`, `►`, `◄`, `+`, `-`, `=`, `·`) whose string `.Length` matches their visual cell width in monospace fonts like Cascadia Code and Consolas.
- **What NOT to Repeat:**
  - NEVER use wide full-width glyphs or unpadded strings inside box frames.
  - NEVER permit lines inside a modal box to vary in length.
  - NEVER deliver graph or modal changes without automated test validation confirming that the right border is 100% straight.

### LL-051: ASCII-Art Wireframe Protocol for User Plotting Specifications
- **Date:** 2026-09-22 22:15 UTC+3
- **Context:** User requested:
  > *"make the template easier to edit like a ascii file to fill ? it maybe like an ascii- zied version of the pic u sent me where I fill and comment ... NO [do not invent names for items 5 and 6] ... document everything"*
- **What Was Learned:**
  1. **Visual ASCII Mockups Over Abstract Lists:** Providing user-facing design templates in formatted ASCII art wireframes (mirroring the actual CRT screen layout) allows the user to directly visualize and edit data fields, curves, axes, and gauges with maximum intuitive clarity.
  2. **Strict User Guidance on Placeholders:** When a user requests TBD items (such as items 5 and 6), NEVER invent speculative names or categories. Strictly preserve `[ TBD ]` until the user supplies explicit labels.
  3. **Exhaustive Documentation Synchronization:** Every directive must immediately update all Core Docs (`scope-v1.md`, `architecture-decisions.md`, `milestones.md`, `lessons-learned.md`, `todo.md`) before taking code actions.
- **What NOT to Repeat:**
  - DO NOT provide plain bullet lists when the user requests an editable visual ASCII layout.
  - DO NOT invent category names for unassigned slots when the user commands NO.

### LL-052: Ingestion of Analytical Algorithms & Dedicated Graphics Sub-Menu
- **Date:** 2026-09-23 22:23 UTC+3
- **Context:** User requested:
  > *"Graph page wll be reached by pressing 3 TBD from the war room menu. when user presses graph page then he will reach a sub menu page where are 10 options. No 1 is Logistical Attrition Telemetry rest is TBD for now document and implement and test this once this is done under C:\Users\ozgur\Documents\Witp-CIC-MUTHUR\docs\OPENCODE INPUT find and ingest analytical_algorithms.md file where algorithms that will be used in the pages are store. implement them , add them to the scope and other related dox test and verify as usual then implement first graph page Logistical_Attrition_Telemetry_page1.md under the same directory"*
- **What Was Learned:**
  1. **Strict 10-Item Graphics Menu Layout:** The Graphics sub-menu must have 10 numbered slots, with option `[1]` dedicated to `Logistical & Attrition Telemetry`, and slots `[2]` through `[10]` strictly designated as `[ TBD ]`.
  2. **Decoupled Analytical Engine Architecture:** Encapsulate mathematical models and algorithms (CoG, vector projection, OLS regression slope, OPTEMPO rolling average, logistical shadow, anomaly detection, global tempo state machine) in a dedicated analytics engine class (`TelemetryAnalyticsEngine`) to keep UI controllers clean, modular, and easily testable.
  3. **Preservation of Rigid Monospace Alignment in Graphing:** For in-terminal ASCII/Unicode presentation screens, all rows must adhere to `boxWidth = 78` and `innerWidth = 74` so that right borders `║` remain perfectly straight across ASCII plots, rate bars, and Braille curves.
### LL-053: Immediate Feature Reversal & TBD Marker Replacement
- **Date:** 2026-09-24 09:14 UTC+3
- **Context:** User commanded:
  > *"delete Logistical Attrition Telemetry page , put a TBD Market in its place"*
- **What Was Learned:**
  1. **Immediate Execution of Deletion / Reversion Directives:** When the user explicitly requests deleting an interface screen and replacing it with a TBD marker, execute the change cleanly across the controller, UI text, test suite, and Core Docs without hesitation or unnecessary pushback.
  2. **Uniformity of Reserved Sub-Menu Slots:** Replacing option `[1]` with `[ TBD ]` creates a clean, unified 10-item placeholder menu where every item consistently communicates its pending status to the user.
  3. **Preservation of Underpinning Logic:** Removing an interface screen does not mean destroying the underlying analytical algorithms if they are needed for future blueprints; keep background math decoupled in the analytics engine while presenting a clean TBD placeholder in the UI.
- **What NOT to Repeat:**
  - DO NOT leave traces of removed titles in active menus when asked to replace with TBD.
  - DO NOT delete underlying domain algorithms unless explicitly instructed to destroy them.

### LL-054: Structured Pros/Cons Analysis & Proportional Chrome Scaling
- **Date:** 2026-09-24 22:35 UTC+3
- **Context:** User requested:
  > *"base line : i need honest answers. say what u think is best and what can be really done. dont ever bullshit"*
  > *"suggest what can be done : 1. screen res chane doesnt change the size of the fonts. in its current state the writing doesnt use all the screen especially in the higher resolutions. Tell me what can be done without losing the MUTHUR Look and feel"*
  > *"when I ask sth like thi always state pros/cons ...."*
  > *"I want to go option 1 but as is known - the top title bar and bottom status bar are currently fixed at ~28px. When the terminal text scales up to 24pt, the main content looks great, but the title and footer text will look relatively smaller unless we scale their font sizes slightly as well. this must be fixed ... can u solve it ?"*
- **What Was Learned:**
  1. **Mandatory Structured Pros/Cons for Architectural Decisions:** Whenever presenting architectural options or design alternatives, always explicitly list structured Pros, Cons, risks, and trade-offs for each choice without sugarcoating or hand-waving.
  2. **Holistic Window Scaling (Chrome + Body):** In retro terminal desktop emulators, scaling the central monospace character font alone creates an unnatural, miniature border around large text. The surrounding chrome (title badges, quick-switch buttons, header padding, footer telemetry labels, resolution combo boxes) must scale proportionally to preserve the visual weight, ergonomics, and aesthetic authenticity of the terminal.
  3. **Aspect-Ratio-Safe Font Scaling Factor:** Using $S = \min(\text{Width} / W_{\text{base}}, \text{Height} / H_{\text{base}})$ prevents vertical clipping on wide monitors and horizontal clipping on tall displays.
- **What NOT to Repeat:**
  - DO NOT present technical choices without explicit structured Pros and Cons.
  - DO NOT scale body fonts while leaving header/footer chrome tiny and fixed.


---


## 3. Checklist for Future Corrections
Whenever Ozgur provides feedback or a correction:
- [ ] 1. Stop and read the exact feedback without making assumptions.
- [ ] 2. Check for any conflict with existing business scope or architecture; IF ANY CONFLICT, ASK OZGUR TO CLARIFY.
- [ ] 3. Add the requirement/correction immediately to the business scope (`docs/scope-v1.md`) with a date/time stamp.
- [ ] 4. Identify the root cause and log a new entry in `docs/lessons-learned.md` (LL-XXX) with Date/Time, Trigger, Learned, and What NOT to Repeat.
- [ ] 5. Mirror architectural decisions in `docs/architecture-decisions.md`.
- [ ] 6. Ensure all Core Docs are synchronized.
- [ ] 7. Implement, verify with automated tests, and compile clean static binary.
- [ ] 8. Verify the Baseline Rule: do NOT mark `[x]` in `docs/todo.md` without explicit instruction.
- [ ] 9. Commit and push exclusively to GitHub (`origin main`).
