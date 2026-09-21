# Architecture decisions (Version 1)

Recorded **2026-09-15 15:37 UTC** from Ozgur’s answers and two “U decide” items. Authoritative detail is in `docs/scope-v1.md` changelog; contradictory original spec sentences remain there, marked superseded.

## 1. Ship Windows first

**Decision:** Version 1 targets **Microsoft Windows** first. Default installation path: **`C:\WitP-CIC`**.

**Source:** Ozgur resolution of §1 (Mac first) vs §3/§9 (Windows paths and deployment).

## 2. Campaign files in `\Campaigns` only

**Decision:** Campaign vault files live **only** under **`C:\WitP-CIC\Campaigns`**. The §4.1 label “`\Campaign Vaults`” is treated as a **synonym error**; do not create or browse that folder name.

**Source:** Ozgur resolution of §3 vs §4.1 path wording.

## 3. Primary language: C++ Windows console `.exe`

**Decision:** Implement in **C++**, producing a **Windows console executable (`.exe`)**. The TUI uses **native Win32 console APIs** with attributes matching spec §8.1 codes **2**, **10**, and **160**. Python, JavaScript, and .NET are **not** the primary stack for v1.

**Ozgur:** “U decide. I need an exe file.”

**Rationale:** A single native `.exe` matches the Win32 TUI spec, avoids extra runtimes (Python/.NET) and separate processes, and aligns with local-only, zero–data-leakage deployment. C++ is chosen over C for safer string/container handling at scale (multi-year campaigns, large log volumes) while still compiling to one console binary.

## 4. One Campaign File; in-process analytics storage

**Decision:** All campaign data (parsed telemetry, belief models, TPIE state) lives in **one compressed Campaign File** per campaign under `\Campaigns`. **No** Neo4j, InfluxDB, or PostGIS **services or on-disk data directories**. Graph relationships, per-turn time series, and hex-spatial structures required by §7 are **in-process** during analysis and **must serialize** into that Campaign File.

**Ozgur:** “U decide” (storage architecture).

**Rationale:** §3 requires a single custom Campaign File in `\Campaigns` and forbids external or orphaned files. Running separate database processes or sidecar data folders would violate that baseline and add operational overhead inconsistent with a single-player local PBEM helper.

## 5. TUI aesthetic: §8 is implementable; film is not an asset source

**Decision:** The **implementable TUI specification** for Version 1 is **`docs/scope-v1.md` §8** (Win32 console attributes codes **2**, **10**, **160**; header/body/footer topography; §8.3 interaction rules; §8.4 modal overlays). The v1 **aesthetic target** is the **MU/TH/UR / Alien ship-computer green phosphor** look already described in §8 — not a separate undocumented layout.

**Not permitted as sources:** **Film frames** and **pixel-perfect reproduction of copyrighted film interface screens** are **not** asset sources, layout authorities, or licenses to copy proprietary movie UI text or branding. Ozgur’s “perfect copy of MUTHUR” is recorded as a **product aesthetic requirement**; engineering implements §8 toward that look without treating the film as deliverable art or spec.

**Ozgur (2026-09-15 ~15:44 UTC):** “the UI must be a perfect copy of the MUTHUR from the ALien movie.”

**On-screen name (2026-09-15 ~15:47 UTC):** Display **WU/TH/UR** (slashes as shown) in splash, headers, and titles. Closes the former OPEN between spec §8 heading **“W.U.T.H.U.R.”** and user **“MUTHUR”**; the §8 heading text in `docs/scope-v1.md` is retained for traceability.

**Layout authority:** §8.2–§8.4 header/body/footer blocks stay as written in the scope document until the scope document changes them.

**Reference images:** None provided; do not treat film frames as assets (unchanged). Do not invent splash taglines.

## 6. `.pws` on-disk format (golden set, 2026-09-19)

**Decision:** WITP-CIC treats PBEM `.pws` files in the golden set as **gzip wrapping an inner chain of zlib records** (61 types keyed by header byte `a & 0xff`), **not** as PKZIP/ZipCrypto containers. **Do not** fail ingest solely because the user supplied a PBEM password — the password is for the **game UI** per manual §20.5.4; these samples decompress without a file key.

**Partial telemetry:** Only **evidence-gated** ship fields may be read from record `0x08` (464-byte rows: name, sunk sentinel `19999`, loss hex, TF links). Live ship hex and most binary tables remain **unknown** until corroborated (`docs/pws-field-map.md`).

**Metadata:** Save labels (e.g. “Incoming to Dave”) are **opaque strings**; Dave = Japanese PBEM partner, Ozgur = Allied. Do not infer side or file direction from label text alone.

**Source:** Forensic pass on seven golden saves; index `docs/pws-knowledge-summary.md`.

## 7. `.pws` password — game-UI credential only (2026-09-19 18:33 UTC+3)

**Decision:** The `.pws` PBEM password (e.g. `kilroy43`) is a **game-UI credential** used inside the WITP:AE application to load an incoming PBEM file (manual §20.5.4). It is **not** a file-level encryption key on the on-disk golden set. WITP-CIC **stores** the password in the vault for the user's own PBEM reference and **does not** use it for gzip/zlib decompression. There is **no password-gate code path** in ingest now or in v1. (Supersedes "validate the password" in §4.1.)

**Source:** Ozgur answer, `docs/open-questions.md` Q1.

## 8. New Campaign Wizard — turn-length step removed (2026-09-19 18:33 UTC+3)

**Decision:** The wizard step "confirm the game's turn length" is **removed**. It was never wanted. Ingest automatically discovers and covers **all dates and all files** found in the game report folder — no manual confirmation required.

**Wizard steps (authoritative):**
1. Ask which side the player commands
2. Prompt for `.pws` PBEM password (store in vault; do not validate)
3. Find the game report folder (auto-search; `[101]` if not found)
4. Prompt for campaign vault file name
5. Ingest all discovered files; transition to Master Menu

**Source:** Ozgur answer, `docs/open-questions.md` Q3.

## 9. Shared file confidence — Allied perspective only (2026-09-19 18:33 UTC+3)

**Decision:** `combatreport_*` and `Combat_Events_*` are shared files but are processed **entirely from the Allied (Ozgur) player's perspective** ("shared only to me"). WITP-CIC does **not** attempt to reconstruct or model the opponent's internal view from shared files. Confidence assignments for shared file content:

- Allied losses in shared combat files → `FACT` (own-force data)
- Enemy (Japanese) losses/claims in shared combat files → `CLAIM` (BDA; confidence adjusted as turns corroborate/contradict)

**Source:** Ozgur answer, `docs/open-questions.md` Q4.

## 10. Dossier Panel — unified free-text search (2026-09-19 18:33 UTC+3)

**Decision (§4.5 Tab 2):** The Dossier Panel implements **free-text partial-name search** across the vault's list of all discovered and known entities. All five entity types — base, vessel, unit, air group, city — are searchable in **one unified interface**. No separate tabs or categories per entity type.

**Source:** Ozgur answer, `docs/open-questions.md` Q6.

## 11. TPIE Tab 3 — degraded predictions from turn 1 (2026-09-19 18:33 UTC+3)

**Decision (§7):** TPIE Tab 3 is **always shown**, even on turn 1. With insufficient history, it displays **degraded / partial predictions** with a clear confidence caveat (e.g. "LOW CONFIDENCE — N turns of data"). Prediction quality improves as more turns accumulate toward the 30+ day baseline. There is **no hard data-volume gate** before showing the tab.

**Source:** Ozgur answer, `docs/open-questions.md` Q7.

## 12. Footer hotkeys — dynamic per screen (2026-09-19 18:33 UTC+3)

**Decision (§8.2 C):** The footer hotkey legend is **composed dynamically per screen** at render time, showing only the hotkeys active on the current screen. The example string in §8.2 C (`[A] AI | [L] List …`) is a **placeholder** and must not be hardcoded. The `layout.cpp` / `console.cpp` footer renderer accepts a hotkey list parameter from each screen's render function.

**Source:** Ozgur answer, `docs/open-questions.md` Q2.

## 13. Intercept Windows — dual input mode with modal popup (2026-09-19 18:33 UTC+3)

**Decision (§4.5 Tab 3):** TPIE Intercept Windows supports two constraint-entry modes, both required: (a) **free-text hex coordinate / range entry** typed by the user, and (b) **pick from a list of discovered/known bases and hexes** in the vault. The input widget is a **modal popup menu** overlaid on Tab 3 (same §8.4 modal mechanism). Projection from known trajectories is the default output; user constraints narrow or extend it.

**Source:** Ozgur answer, `docs/open-questions.md` Q5.

## 14. Screen Header Title Format (2026-09-19 19:24 UTC+3)

**Decision (§8.2 A):** The `{INDEX}` field is removed. It was an artifact from earlier drafts. Headers will be cleanly rendered as `[ {SCREEN TITLE} ]  Turn: {TURN_DATE}` (or `[ {SCREEN TITLE} ]` when no turn date is active, e.g. Start screen / Setup Wizard).

**Source:** Ozgur answer, `docs/open-questions-batch2.md` Q A1.

## 15. Settings Screen Scope & In-Screen Descriptions (2026-09-19 19:24 UTC+3)

**Decision (§4.2 [S], §6):** Version 1 Settings screen exposes:
1. **Allied SIGINT Confidence:** Toggle between `HIGH` and `PROBABLE`.
2. **Strike Concentration Detection:**
   - Minimum Cruiser Count threshold (integer, default `3`).
   - Proximity radius in hexes (integer, default `10`).

Each setting item must display an inline description explaining its function and intelligence modeling impact.

**Source:** Ozgur answer, `docs/open-questions-batch2.md` Q A2.

## 16. Master Menu ESC Key Action (2026-09-19 19:24 UTC+3)

**Decision (§4.2):** Pressing `ESC` from the Master Menu invokes the **Exit Confirmation Prompt** (`[E] Exit`), asking user confirmation before purging memory and shutting down.

**Source:** Ozgur answer, `docs/open-questions-batch2.md` Q A3.

## 17. Campaign Vault Container Compression (2026-09-19 19:24 UTC+3)

**Decision (§3, §5):** "Just compress it." Campaign vaults (`.wic`) are single Zlib/Zip compressed containers. No cryptographic password layer is applied.

**Source:** Ozgur answer, `docs/open-questions-batch2.md` Q B1.

## 18. Tab 2 SIGINT Map View Mode (2026-09-19 19:24 UTC+3)

**Decision (§4.5 Tab 2):** In Version 1, the SIGINT Map view is rendered as a **structured textual/tabular list of localized hex coordinates with distances and sector bearings** relative to active Allied headquarters/bases. A spatial 2D ASCII map is deferred.

**Source:** Ozgur answer, `docs/open-questions-batch2.md` Q C1.

## 19. WITP:AE Default Discovery Paths (2026-09-19 19:24 UTC+3)

**Decision (§4.1):** Automated search checks two default locations:
1. `C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE` (for `.pws` saves)
2. `C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE\archive` (for report text files)
A manual folder browse dialog is provided if the user has an alternative install path.

**Source:** Ozgur answer, `docs/open-questions-batch2.md` Q D1.

## 20. Alien MU/TH/UR Nostromo Terminal Aesthetic & C++ Toolchain (2026-09-19 19:24 UTC+3)

**Decision (§8):**
- **Aesthetic:** Alien movie ship-computer interface (**MU/TH/UR** / Nostromo / Weyland-Yutani). Monochromatic green phosphor palette (Win32 Console attributes 2 dark green, 10 light green, 160 inverted cursor), uppercase teletype typography, crisp box frames (`=`, `-`, `+`), modal popups, blinking cursor status indicators.
- **Toolchain:** C++17 with MinGW-w64 (GCC), building a standalone, static `WitP-CIC.exe` binary with zero dynamic library dependencies.

**Source:** Ozgur answer, `docs/open-questions-batch2.md` Q E1.

## 21. Canonical Remote & Git Repository Rule (2026-09-19 20:25 UTC+3)

**Decision:**
- Permanently decommission and purge all Cursor git remotes (`origin.cursor.com` and `WITP-CIC-CURSOR`) and Cursor local branches.
- The project's sole, authoritative Git repository is **`https://github.com/oaltinyay70-cell/WITP-CIC2.git`** mapped as canonical **`origin`** on branch **`main`**.
- All commits and pushes are strictly and exclusively made to `origin main` (GitHub). Never push to or pull from Cursor repositories.

**Source:** Ozgur directive (2026-09-19 20:25 UTC+3).

## 22. Baseline Task Completion Rule (2026-09-19 20:25 UTC+3)

**Decision:**
- **Baseline:** Never mark any task, requirement, or checkbox as done (`[x]`) in `docs/todo.md` or anywhere in project tracking documents unless Ozgur explicitly says so.
- Writing code, compiling binaries, and passing automated unit tests are technical prerequisites, but the act of marking an item done requires explicit confirmation/instruction from Ozgur.

**Source:** Ozgur directive (2026-09-19 20:25 UTC+3).

## 23. Continuous Ambient Sound, Blink Cadence & Double-Spaced Menus (2026-09-19 20:35 UTC+3)

**Decision:**
1. **Background Audio:** Play user-provided ambient audio (`assets/audio/alien_ambient_transition.wav`) continuously in a background loop (`PlaySoundA` with `SND_FILENAME | SND_ASYNC | SND_LOOP | SND_NODEFAULT`) starting at application launch and terminating cleanly on exit.
2. **Blinking Cadence:** Set prompt blink rate to ~1 Hz (500 ms visible, 500 ms inverted) with high-frequency 25 ms polling to preserve instantaneous keypress responsiveness without visual strobing.
3. **Menu Spacing:** Double-space all interactive menu items (Start Screen, Master Menu, Continue Campaign, Setup Wizard) with a dedicated blank line separating choices for retro terminal readability.

**Source:** Ozgur directive (2026-09-19 20:35 UTC+3).

## 24. Separation of Wiki and Field Manual Modules (2026-09-19 20:39 UTC+3)

**Decision (§4.2, §4.3):**
1. **Master Menu Structure:** `FIELD MANUAL` is decoupled from `WIKI` as a distinct top-level module:
   - `[W] WIKI (WITP-CIC SYSTEM & ERROR REGISTRY)`
   - `[F] FIELD MANUAL (WITP:AE GAME MANUAL BROWSER & SEARCH)`
2. **Wiki Screen Architecture:** Dedicated hierarchical category browser covering WITP-CIC system design, War Room dashboards, PBEM lifecycle, terminal navigation, and the mandatory 3-digit Error Code Registry (§13).
3. **Field Manual Architecture:** Dedicated interactive browser and keyword search/RAG interface indexing all 24 chapters of `docs/witpae-manual.txt` with in-terminal scrolling and query capability.

**Source:** Ozgur directive (2026-09-19 20:39 UTC+3).

## 25. Baseline Definition, Core Docs Classification & Lessons Learned Protocol (2026-09-19 20:57 UTC+3)

**Decision:**
1. **Baseline Definition:** The Baseline is the authoritative set of verified project deliverables. A requirement or task is NEVER marked done (`[x]`) unless Ozgur explicitly says so. Technical verification (clean build, passing tests) is a prerequisite, not completion acceptance.
2. **Core Docs Classification:** Documentation covering Baseline Principles (`docs/baseline_witp_cic.md`), To-Do lists (`docs/todo.md`), Lessons Learned (`docs/lessons-learned.md`), Architecture Decisions (`docs/architecture-decisions.md`), Screen Flow diagrams (`docs/screen-flow.md`, `docs/screen-flow-text.md`), Scope Bible (`docs/scope-v1.md`), Project Context (`docs/project-context.md`), and UI specifications are formally designated as **Core Docs**. They must always be synchronized, date-stamped, and preserved.
3. **Lessons Learned Protocol:** Maintain `docs/lessons-learned.md` as an active knowledge base. Following every correction, feedback item, or design clarification from Ozgur, immediately document what was learned and what NOT to repeat.

**Source:** Ozgur directive (2026-09-19 20:57 UTC+3).

## 26. Recursive Hierarchical Wiki Topic Navigation & Technical Details Grouping (2026-09-19 21:05 UTC+3)

**Decision (§4.3):**
1. **Recursive Topic Tree:** The Wiki module is implemented as a recursive node tree (`WikiNode`). Selecting any topic node routes either to another sub-topic selection menu (branch) or to a scrollable content reader (leaf).
2. **Technical Details Topic Grouping:** A new top-level category titled `TECHNICAL DETAILS` is established. All 5 technical topics (System Architecture & Memory Sandbox, Combat Information Center, Data Ingestion & PBEM Lifecycle, Nostromo Terminal Controls & Hotkeys, and System Error Code Registry) are nested as sub-topics under `TECHNICAL DETAILS`.
3. **Navigation Consistency:** Sub-topic menus share the identical Nostromo visual language: double-spaced options, underline selection cursor, acoustic relay clicks, chirp transitions, and `[ESC]` navigating back up one level.

**Source:** Ozgur directive (2026-09-19 21:05 UTC+3).

## 27. Granular Error Code Sub-Topic Architecture in System Wiki (2026-09-19 21:08 UTC+3)

**Decision (§4.3, §13):**
1. **Granular Sub-Topic Decomposition:** In accordance with user feedback (*"each error along with the name is a sub topic."*), the `SYSTEM ERROR CODE REGISTRY` is implemented as an interactive branch category containing dedicated selectable sub-topics for every standard 3-digit system error:
   - `[1] [101] VAULT_NOT_FOUND`
   - `[2] [102] VAULT_CORRUPT`
   - `[3] [201] REPORT_DIR_MISSING`
   - `[4] [202] PWS_READ_ERROR`
   - `[5] [301] PARSE_FORMAT_MISMATCH`
   - `[6] [401] MEMORY_ALLOCATION_FAIL`
2. **Dedicated Error Documentation Pages:** Selecting an error code opens its dedicated documentation screen containing:
   - Error code and formal name
   - Associated subsystem and severity level
   - Description and root cause analysis
   - Observable symptoms in the UI/session
   - Concrete, actionable user recovery procedures
3. **Recursive Navigation Flow:** Pressing `[ESC]` from any error page returns to the Error Code list; pressing `[ESC]` again returns to `TECHNICAL DETAILS`; pressing `[ESC]` again returns to the main Wiki category menu; and pressing `[ESC]` from the root Wiki returns to the Master Menu.

**Source:** Ozgur directive (2026-09-19 21:08 UTC+3).

## 28. Incremental Ingestion & War Room Model Refresh on Campaign Load (2026-09-20 14:15 UTC+3)

**Decision (§4.1, §4.5):**
1. **Incremental Directory Scan on Load:** When an existing campaign is selected and loaded (`[ CONTINUE CAMPAIGN ]`), the engine retrieves the configured report directories and SAVE directories stored within the campaign metadata and immediately scans them for any newly created files (`wpae*.pws`, `asigint_*.txt`, `jsigint_*.txt`, `aoperationsreport_*.txt`, `combatreport_*.txt`, `Combat_Events_*.txt`).
2. **Manifest Diff & Ingestion:** The scanner compares discovered files against the campaign's serialized `fileManifest`. Any file not yet in the manifest (or with a newer timestamp/turn date) is scheduled for ingestion. All new files are parsed through the appropriate parser modules.
3. **Vault Update & Container Persistence:** Newly parsed records are merged into the in-memory campaign state, updating the latest turn date and total days processed. The updated vault container is immediately compressed and re-serialized to `C:\WitP-CIC\Campaigns\<Name>.wic`.
4. **War Room Analytical Model Refresh:** The War Room analytical engines (Intelligence Summary, Strike Concentrations, Port Concentrations, Established Routes, and Tactical Intercepts) are triggered to re-run and recalculate metrics across the expanded timeline, ensuring the user is presented with current intelligence immediately upon entering the War Room.

**Source:** Ozgur directive (2026-09-20 14:15 UTC+3).

## 29. Selection Underline Styling / Removal & Pop-up Explanation Modals (2026-09-20 14:18 UTC+3)

**Decision (§4.3, §8):**
1. **Continuous Underline or Total Removal:** When a menu or topic option is selected, if an unbroken continuous horizontal line (via box-drawing characters or console attributes) cannot be rendered without visible font glyph gaps, the underline decoration must be eliminated completely. The interface will rely cleanly and exclusively on inverted phosphor highlight (Code 160) without broken ASCII `-` or `_` lines.
2. **Pop-up Window Explanation Modals:** Topic descriptions and option explanations will no longer be displayed inline under the selected item. Instead, they will be rendered in a dedicated pop-up modal dialog box with framed borders and slightly smaller/compact typography, preserving vertical menu alignment and stability.

**Source:** Ozgur directive (2026-09-20 14:18 UTC+3).

## 30. Deletion of Looped Background Ambient Audio (2026-09-20 14:20 UTC+3)

**Decision (§5.1):**
1. **Looped Audio Deleted:** In accordance with user instruction (*"Delete the background sound"*), continuous background looped radio static playback (`PlaySoundA` with `SND_LOOP`) is completely removed from application startup, runtime, and shutdown.
2. **Acoustic Feedback Preserved:** Discrete interactive feedback—such as mechanical relay clicks (1800 Hz) on selection traversal and retro teletype multi-tone chirps on screen transitions—remains intact via native `Beep(...)`, operating in complete background silence.

**Source:** Ozgur directive (2026-09-20 14:20 UTC+3).

## 31. Shorter, Sombre Navigation Key Click Acoustic Profile (2026-09-20 14:21 UTC+3)

**Decision (§5.2):**
1. **Sombre Click Profile:** In accordance with user instruction (*"change the key move click to a shorter , sombre click"*), the navigation key move click (triggered on `UP/DOWN/TAB` selection change in menus and lists) will be tuned to a lower, muted acoustic profile (e.g. 400-600 Hz for 4-5 ms) replacing the piercing 1800 Hz high tone.
2. **Implementation:** Update `Console::playNavigationClick()` to execute the refined sombre frequency profile via native Win32 `Beep(...)`.

**Source:** Ozgur directive (2026-09-20 14:21 UTC+3).

## 32. Business Scope Synchronization & Conflict Clarification Protocol (2026-09-20 15:05 UTC+3)

**Decision (§3, §14):**
1. **Mandatory Scope Addition:** Every new to-do item, user directive, and correction MUST be immediately added to the business scope document (`docs/scope-v1.md`) with an explicit date/time stamp. Business scope is the supreme requirements baseline.
2. **Conflict Clarification Gate:** If there is ANY ambiguity, tension, or conflict between a newly requested correction/to-do item and existing business scope requirements or architecture, the agent MUST NOT guess or make unilateral assumptions. The agent is strictly mandated to pause and ask Ozgur for clarification before taking action.

**Source:** Ozgur directive (2026-09-20 15:05 UTC+3).

## 33. Master Menu Header Instrumentation & Metadata Layout (2026-09-20 15:07 UTC+3)

**Decision (§4.2, §8.2 A):**
1. **Top Header Relocation:** All active campaign identification (`Active Campaign: <Name> (<Side> Command)`) and database status metrics are relocated from the menu selection body into the top instrumentation header block (`Layout::renderHeaderWithStats`).
2. **Standardized Header Format:** The header bar renders:
   - Line 1: `[ MASTER MENU ]` with Campaign / Command right-aligned.
   - Line 2: `Files Processed : XX pws files | xx TXT files | Days Processed : xx`, partitioned by vertical `|` delimiters.
3. **Clean Menu Space:** This keeps the command module options centered, spacious, and uncluttered.

**Source:** Ozgur directive (2026-09-20 15:07 UTC+3).

## 34. Framed Pop-up Modal Notifications for Module Engagement (2026-09-20 15:07 UTC+3)

**Decision (§4.2, §8):**
1. **Elimination of Fixed Cursor Overwrites:** Removing all instances of `Console::setCursorPosition(4, 20)` which previously overwrote and corrupted footer hotkey text or background layout.
2. **Modal Notification Architecture:** Module transition and retrieval messages (e.g. `[ WAR ROOM MODULE ENGAGED - RETRIEVING THEATER INTELLIGENCE... ]` and `[ SETTINGS MODULE ENGAGED ]`) are rendered within a dedicated centered framed pop-up modal box (`Layout::renderNotificationModal`).

**Source:** Ozgur directive (2026-09-20 15:07 UTC+3).

## 35. Right-Aligned Pop-up Topic Explanations (2026-09-20 15:07 UTC+3)

**Decision (§4.3, §8):**
1. **Side-by-Side Split Architecture:** In `WikiScreen` (and `StartScreen` where applicable), topic/option selection menus reside on the left (columns 2-38). The `TOPIC EXPLANATION` pop-up box is rendered to the RIGHT (columns 41-78).
2. **Compact Modal Formatting:** The right-hand pop-up window uses compact word-wrapped formatting, border markers, and dimmed phosphor (`DarkGreen`) to give the distinct visual impression of subordinate/compact auxiliary typography.
3. **Zero Vertical Jitter:** The left menu remains perfectly stable at all times while the user navigates choices, with zero vertical shifting and 100% visibility within standard 80x25 terminal screens.

**Source:** Ozgur directive (2026-09-20 15:07 UTC+3).

## 36. Version 0.02 (Stable Alpha 1) Tagging & Rollback Preservation Strategy (2026-09-20 16:15 UTC+3)

**Decision (§3, §14):**
1. **Milestone Baseline Lock:** Establish and lock Version 0.02 as **Stable Alpha 1**, encapsulating the full set of verified features:
   - Retro Alien MU/TH/UR Nostromo console interface in C++17 (Win32 console API).
   - Splash screen with non-blocking blink cycle and zero attribute bleed.
   - Campaign wizard with auto-detection, multi-directory accumulation (`[B]`), and instant `.wic` container serialization under `C:\WitP-CIC\Campaigns`.
   - Incremental diff turn ingestion upon continuing campaigns, refreshing all War Room models.
   - Master Menu header telemetry instrumentation (`Files Processed : XX pws files | xx TXT files | Days Processed : xx`).
   - Framed notification modal pop-ups replacing fixed cursor coordinate overwrites.
   - Recursive hierarchical System Wiki with right-aligned pop-up explanations and decomposed Error Code Registry sub-topics.
   - Standalone 24-chapter Field Manual browser.
   - Sombre, short acoustic navigation clicks and silent background audio.
2. **Immutable Version Tagging:**
   - Git tags: `v0.02` and `v0.02-alpha.1` created and pushed to GitHub `origin`.
   - Dedicated stable branch: `stable-alpha-1` created and pushed to `origin`.
3. **Physical Archive Redundancy:**
   - Standalone offline backup zip created at `backups/WITP-CIC2-v0.02-alpha1.zip`.
4. **Time-Travel & Rollback Standard:**
   - Developers or users can revert to this baseline state at any time using:
     ```bash
     git checkout v0.02
     # or:
     git checkout stable-alpha-1
     ```

**Source:** Ozgur directive (2026-09-20 16:15 UTC+3).

## 37. Standardized Upgrade, Backup, and Rollback Protocol (2026-09-20 16:32 UTC+3)

**Decision (§3, §14):**
1. **Official Rollback Guide:** Establish [`docs/how-to-rollback-to-alpha-1.md`](docs/how-to-rollback-to-alpha-1.md) as the authoritative documentation for restoring, checking out, and testing the Version 0.02 (Stable Alpha 1) baseline.
2. **Standardized Procedure for All Future Upgrades:** Any future software release or milestone progression MUST execute this standardized 6-step lifecycle:
   - **Step 1 (Automated Verification):** Build static binary `dist/WitP-CIC.exe` and verify 100% pass across all unit tests.
   - **Step 2 (Core Docs Synchronization):** Date-stamped additions to `docs/scope-v1.md`, `docs/baseline_witp_cic.md`, `docs/lessons-learned.md`, `docs/architecture-decisions.md`, `docs/project-context.md`, `docs/milestones.md`, while maintaining `docs/todo.md` without unapproved checkmarks.
   - **Step 3 (Physical Backup):** Create standalone compressed archive `backups/WITP-CIC2-v<Version>-<Milestone>.zip`.
   - **Step 4 (Git Tagging & Branching):** Create annotated Git tags (`v<Version>`) and maintain milestone branch (`stable-<milestone>`).
   - **Step 5 (GitHub Remote Synchronization):** Push `main`, milestone branch, and tags to GitHub `origin`.
   - **Step 6 (Rollback Documentation):** Update or publish dedicated rollback instructions.

**Source:** Ozgur directive (2026-09-20 16:32 UTC+3).

## 38. Field Manual: WITP:AE MANUAL Header & Interactive Wiki HTML Integration (2026-09-20 16:45 UTC+3)

**Decision (§4.4, §8):**
1. **Section Header Renamed:** Replaced `[ C H A P T E R   B R O W S E R   &   R A G   S E A R C H ]` with `[ WITP:AE MANUAL ]` (`Layout::renderSectionHeader("W I T P : A E   M A N U A L")`).
2. **Index Counter Deleted:** Deleted the line starting with `Index : Chapter X of 24 | Total Document Lines: ...` from the Field Manual screen layout.
3. **Manual Content Replacement:** Replaced the previous 24 raw text chapters in `FieldManualScreen` with the interactive manual wiki (`witp_ae_manual_wiki.html`), located at `C:\Users\ozgur\Documents\WITP-CIC ASSETS\docs\rag\witp-ae\witp_ae_manual_wiki.html` (and mirrored locally in `docs/witp_ae_manual_wiki.html`).
4. **Browser Launch:** Activating Option `[1] OPEN WITP:AE MANUAL (HTML WIKI)` and pressing `[ENTER]` triggers native Windows `ShellExecuteA(NULL, "open", path.c_str(), NULL, NULL, SW_SHOWNORMAL)` (with command-line fallback) to open the interactive HTML wiki with 2-column TOC in the user's default browser after rendering a Nostromo modal status notification.

## 39. In-Terminal Hierarchical JSON Manual Ingestion & Browser (2026-09-20 17:20 UTC+3)

**Decision (§4.4, §8):**
1. **Full In-Terminal Native Integration:** External web browser launching and separate files are removed. The entire WITP:AE Operations & Mechanics manual wiki is ingested into the terminal application via a high-performance hierarchical JSON schema (`assets/manual/witp_ae_manual.json` and mirrored in `docs/witp_ae_manual.json`).
2. **Options 2 & 3 Deleted:** The previous sub-menu options `[2] SEARCH MANUAL RULES` and `[3] BROWSE CHAPTER TEXT` are deleted. The Field Manual module opens directly into the hierarchical manual browser.
3. **Hierarchical Navigation UX (`X.0` -> `X.1` -> `X.1.1` -> Reader):**
   - **Root Level (`X.0`):** Displays all 15 operational chapters (`1.0 Naval Units` through `15.0 Appendices`).
   - **Sub-Topic Level (`X.1`):** Selecting a chapter opens its sub-topics (e.g., `1.0 Chapter Overview`, `1.1 Task Forces and Ships`, `1.2 Forming a Task Force`).
   - **Deep Hierarchy (`X.1.1`):** Sub-sections with children drill down recursively into sub-topics (e.g., `1.1.1 Task Force Symbols`).
   - **Nostromo Article Reader (Leaf Nodes):** Selecting a leaf section (or overview node) opens the in-terminal article viewer with word-wrapped content lines, line scrolling (`[UP/DOWN]`), page jumps (`[PGUP/PGDN]`), and `[ESC]` return navigation.
4. **Nostromo MU/TH/UR UI Standards:**
   - Monochromatic green phosphor palette (`ConsoleColor::DarkGreen`, `ConsoleColor::LightGreen`, `ConsoleColor::Inverted`).
   - Double-spaced menu options with inverted selection highlight.
   - Right-aligned `[ TOPIC EXPLANATION ]` pop-up window (columns 44–78) rendering wrapped topic summaries in dimmed phosphor (`ConsoleColor::DarkGreen`).
   - Sombre mechanical clicks on cursor movement (`[UP/DOWN/TAB/PGUP/PGDN]`) and teletype chimes on transitions.
   - NO "Table of Contents" label and NO `Index : ...` line.
5. **Zero-Dependency C++ JSON Engine:** A recursive descent JSON parser is integrated into `field_manual_screen.cpp` for instant parsing of all 15 chapters and 414 section nodes with zero third-party dependencies.

**Source:** Ozgur directive (2026-09-20 17:05 UTC+3).

## 40. Cumulative Master Implementation Plan & Predictive AI Settings Sub-Item (2026-09-20 20:31 UTC+3)

**Decision (§3, §4.2, §4.5, §4.7):**
1. **Cumulative Master Implementation Plan:** `docs/master_implementation_plan.md` is designated as an authoritative Core Doc permanently recording all phases (Phases 1–6 Completed, Phase 7 Active, Phase 8 Planned). It maintains the cumulative historical and forward-looking roadmap.
2. **Zero Hallucination Standard:** In compliance with Ozgur's baseline instruction, all predictive intelligence metrics (Amphibious Assault, Fleet Trajectory, Air Attrition, Logistical Pause, Battle Claims, Strike Concentration, Established Routes) must be strictly computed from authentic PBEM telemetry (`.pws`, `asigint`, `aoperationsreport`, `combatreport`, `Combat_Events`). No heuristic data or rules may be hallucinated.
3. **Clash & Doubt Protocol:** If any discrepancy, contradiction, or ambiguity is detected in requirements, specifications, or data, the developer/agent must pause immediately and ask Ozgur rather than guessing.
4. **Predictive AI Model Connection Sub-Item:** In `Main Menu \ SYSTEM SETTINGS & DETECTION THRESHOLDS`, Sub-Item 1 is dedicated to `[1] CONNECT TO PREDICTIVE AI MODEL`, providing interactive provider toggling (Local Analytics Heuristics Engine, Local LLM / Ollama Bridge, Google Gemini API Engine, Custom Endpoint), live connection status monitoring (`[CONNECTED]`, `[STANDBY]`, `[OFFLINE]`), and endpoint configuration.

**Source:** Ozgur directives (2026-09-20 20:31 UTC+3).

## 41. Zero Blinking Policy, Inner Input Polling, and Ghost Fleet Deletion (2026-09-20 21:05 UTC+3)

**Decision (§4.2, §4.5, §8):**
1. **Universal Zero Blinking Policy:** In response to user directive (*"The blinking is faulty, eliminate any blinking. from now on , it will be done only for items I specificly say"*), all temporal flashing, cursor blinking, and oscillating colors are removed across the entire application. Specifically, the splash screen prompt `[ PRESS ANY KEY TO ENGAGE WU/TH/UR CONSOLE ]` is rendered steady `ConsoleColor::LightGreen`. No feature may implement blinking unless Ozgur specifically designates it.
2. **Inner Input Polling Pattern for Stable Rendering:** All screen loops (`WarRoomScreen`, `SettingsScreen`, `SplashScreen`, etc.) must separate rendering from keyboard event polling. The screen is rendered once, followed by an inner `while (true)` loop that polls `Console::readKey()` with a 20ms sleep until a valid non-zero key is detected. Only key events that modify state break to redraw the screen, eliminating the 50 FPS screen clearing flicker.
3. **Ghost Fleet Counter Deletion:** In response to user directive (*"Delete Ghost Fleet Counter"*), the Ghost Fleet Counter is deleted from Tab 1 of the War Room and excised from Model E detail views. Tab 1 now cleanly presents 5 double-spaced intelligence cards. Model E is designated as `MODEL E : Battle Claims & Damage Revision Register`.

**Source:** Ozgur directives (2026-09-20 21:05 UTC+3).

## 42. War Room Standard Menu & Interactive Reasoning Architecture (2026-09-20 21:18 UTC+3)

**Decision (§4.2, §4.5, §8):**
1. **Standard Hierarchical Menu Paradigm:** The horizontal tab layout in the War Room is permanently replaced with a vertical Standard Menu on the War Room Primary Screen:
   - `[1] Intelligence Summary`
   - `[2] SIGINT Map`
   - `[3] TBD` (Theater Battlespace Domain / Phase 08 placeholder)
   - `[4] TPIE Warning Dashboard`
   - `[5] Predictive Models Sub-Menu`
   Selecting any option takes the user directly into that topic's dedicated sub-menu / sub-screen.
2. **Two-Tier Cursor Traversal in Pop-Ups:** All items and bullets in sub-menus and pop-up manifests can be traversed with cursor keys (`[UP/DOWN]`). The active bullet is highlighted in inverted green phosphor.
3. **Analytical Reasoning on `[ENTER]`:** Pressing `[ENTER]` over any bullet displays a dedicated reasoning dialog detailing the exact telemetry sources, coordinate triangulations, and mathematical deductions supporting that specific line.
4. **Standard Return Footer:** All sub-screens, modals, and pop-up overlays must display the exact exit prompt: `PRESS ESC TO RETURN`.

## 43. Geospatial Sighting Last-Seen Date Tracking, Anti-Bullshit Reasoning & Border Stability (2026-09-20 21:40 UTC+3)

**Decision (§4.2, §4.5, §8):**
1. **Last-Seen Date Ingestion & Display:** To resolve blank `[ DATE ]` fields when turn filenames omit timestamps (e.g. `aoperationsreport.txt`), `FileScanner` extracts turn dates directly from operational report header text (`OPERATIONAL REPORT FOR Dec 14, 41`). Multi-turn contact sightings are correlated to track all sighting dates (`allDates`) and explicitly present the most recent sighting date (`lastSeenDate`) in the first column formatted as `Dec 14`.
2. **Modal Text Word-Wrapping:** Reasoning modal text renders without slicing or ellipsis (`...`). Long lines (such as coordinates with extended bearing descriptions) are dynamically wrapped with clean 4-space continuation line indentation, ensuring 100% of telemetry data is visible.
3. **Slow Blinking Confidence Meter Bar Exception:** A visual Confidence Meter Bar is integrated into contact reasoning modals, showing grounded confidence percentages (75%–98%) calibrated against report observer types, ship counts, capital ship classifications, and multi-turn corroboration. For contacts with `>=90%` confidence, a slow 600ms in-place blink (toggling between `Inverted` and `LightGreen` without clearing the screen) is implemented as an explicit user-mandated exception to the zero-blinking policy.
4. **Anti-Bullshit Reasoning Protocol:** Tactical reasoning and intercept significance fields are strictly prohibited from displaying speculative filler or boilerplate text. Until fully grounded predictive AI models are integrated, these fields are explicitly rendered as `*-**-**-**-*-*`.
5. **Fixed-Width Box Framing:** Navigable pop-up dialogs and reasoning modals anchor left borders (`| `) and right borders (` |`) to fixed console columns with exact content padding, eliminating border scrambling and coordinate shifts during cursor traversal.

## 44. Deterministic Numerical Confidence Level Derivation Engine (2026-09-20 22:08 UTC+3)

**Decision (§4.2, §4.5, §6.1):**
1. **Strict Elimination of Placeholder Descriptions:** Subjective confidence strings (e.g. "High confidence based on multi-bearing signal triangulation") are strictly prohibited. Intelligence confidence must be calculated numerically as an exact percentage ($0\% - 100\%$) and mapped to four standard tiers (`UNCONFIRMED`, `PROBABLE`, `CONFIRMED`, `VERIFIED`).
2. **High-Performance Multi-Pass C++ Pipeline:**
   - **Phase 1 (Parsing & Classification):** Fast substring evaluation (`find` / `substr` / C `strstr` — zero `std::regex`) extracting Entity Name, Location, Hex, Report Type, and Domain:
     - `RADIO_TRAFFIC` ("Radio transmissions"): Base Score = 30
     - `UNIT_LOCATED` ("is located at"): Base Score = 75
     - `UNIT_MOVING` ("is loaded on"): Base Score = 65
     - `GARRISON_SIZE` ("men are based at"): Base Score = 50
     - `SHIP_MOVING` ("is moving to"): Base Score = 65
   - **Phase 2 (Corroboration Bonus):**
     - Entity frequency in `EntityMap` $> 1$: $+15$ bonus
     - Hex frequency in `HexMap` $> 1$: $+10$ bonus
   - **Phase 3 (Time Staleness Decay & Clamping):**
     - Multiplier based on turn delta: current turn ($\Delta t = 0$) $\times 1.0$, previous turn ($\Delta t = 1$) $\times 0.8$, older turns ($\Delta t > 1$) $\times 0.5$.
     - Clamped strictly: $\text{FinalScore} = \text{clamp}\big((\text{BaseScore} + \text{Bonus}) \times \text{TimeDecay},\ 0,\ 100\big)$.
   - **Phase 4 (Engine Memory Allocation):** Permanent turn-state storage in `std::vector<ConfidenceReport>`, integer hex coordinates (`X * 1000 + Y`) for $O(1)$ fast hashing, and immediate deallocation of temporary parsing maps.
3. **Ask Rather Than Guess Mandate:** Whenever an operational formula or derivation logic is unknown, the developer/agent must pause, explicitly state "I don't know this", and ask Ozgur rather than inventing placeholder numbers or heuristics.

## 45. Corroboration Scope Separation, Visual Recon & Modal Math Presentation (2026-09-20 22:27 UTC+3)

**Decision (§4.2, §4.5, §6.1):**
1. **Stateless Per-Turn Parser vs Stateful Game State:** The text parsing engine remains strictly per-turn (`Option B`). Corroboration bonuses for entity/hex repetition (+15 / +10) are computed exclusively within the boundaries of the active turn's report file to prevent memory bloat and maintain fast linear scan times. The Game State Manager manages cross-turn persistence: when a contact from a prior turn recurs in the current turn, its timestamp updates to current, Time Decay resets to $1.0$, and its confidence score remains elevated.
2. **Visual Reconnaissance Integration (`VISUAL_RECON`):** Operational sightings (e.g. `"12 Japanese ships at 80,77 near Legaspi"`) are classified as a dedicated `VISUAL_RECON` type with Base Score = 80. Visual recon does not populate `EntityMap` (counts/types lack distinct named identities), but does populate `HexMap`. This enables spatial multi-source corroboration: if SIGINT reports radio traffic at a hex and Visual Recon spots vessels at that same hex, both receive the $+10$ Hex Corroboration Bonus.
3. **Tiered Modal UI Architecture:**
   - **Header:** Clean tactical summary: `CONFIDENCE: 90% [VERIFIED]` (or `CONFIDENCE LEVEL: [==========] 90% [VERIFIED]`).
   - **Reasoning Body:** Granular mathematical transparency translated into plain English:
     - `Base Score: 75 (Unit Located)` or `Base Score: 80 (Visual Recon)`
     - `Corroboration: +15 (Confirmed by 2 SIGINT intercepts) / +10 (Hex corroborated)`
     - `Time Decay: x1.0 (Reported this turn)`
     - `Final Calculation: (Base + Bonus) * Decay = Score%`
     - `Analyst Note: Operational assessment and context`

**Source:** Ozgur directives (2026-09-20 22:27 UTC+3).

## 46. Standardized Binary Compilation Naming Format & Milestone Progression to Alpha 2 (2026-09-20 23:02 UTC+3)

**Decision (§1.13, §3):**
1. **Milestone Transition to Alpha 2:** The project officially transitions from Version 0.02 (Stable Alpha 1) to **Alpha 2** (`v0.02-alpha2`).
2. **Standardized Binary Compilation Naming Format:**
   - Every compiled release executable must strictly follow the naming pattern:
     `witp-cic-[mainversion id]-[phaseid]-[built no]`
   - `[mainversion id]`: Main version in `V.XX` format (e.g. `v0.02`).
   - `[phaseid]`: Current lifecycle phase (e.g. `alpha2`, `beta`, etc.).
   - `[built no]`: Incremental build counter, strictly starting from `0` for each new main phase ID.
3. **Active Artifact Mapping:**
   - Active executable: `dist/witp-cic-v0.02-alpha2-0.exe`.
   - Canonical mirrored executable: `dist/WitP-CIC.exe` (maintained for test runners, build scripts, and backwards compatibility).
   - **Source:** Ozgur directives (2026-09-20 23:02 UTC+3).

## 47. Naval Unit Capability Registry, Auxiliary Doctrine & Predictive Grounding (2026-09-21 13:27 UTC+3)

**Decision (§1.14, §6.2, §6.3, §7.2):**
1. **Predictive Analytics Grounding in Vessel Capabilities:**
   - All predictive models in the War Room Analytics engine must interpret contact and task force compositions through verified naval capabilities and operational roles rather than treating ships as generic counters:
     - **Model A (Amphibious Assault Probability):** Distinguish assault transports (APA, AP, LST, LSD) that carry heavy assault echelons from fast transports (APD) performing raid/insertion/extraction missions, and aircraft transports (AKV) moving uncrated operational airframes.
     - **Model D (Logistical Pause & Base Status):** Identify forward operational base creation and repair capabilities when mobile tenders are present:
       - **Submarine Tenders (AS):** Enable forward submarine rearming/repairing at low-level ports, extending offensive patrol arcs.
       - **Destroyer Tenders (AD):** Establish forward escort/destroyer rearming and maintenance bases.
       - **Aviation Tenders (AV, AVD, AVP):** Supply mobile aviation support points and fuel for flying boats and floatplanes, establishing forward reconnaissance coverage.
       - **Repair Ships & Docks (AR, ARD):** Enable forward mobile repair hubs for battle-damaged combatants (AR: system/engine/flotation up to 5 pts; ARD: flotation repair up to 100 pts/turn).
       - **Ammunition Ships (AE, AKE):** Provide mobile heavy caliber and sortie replenishment for battleships, cruisers, and carriers at undeveloped anchorages.
       - **Fleet Oilers (AO):** Provide underway replenishment, extending task force operational strike radii deep into contested waters.
     - **Model F (Strike Concentration):** Distinguish capital combatants (CV, CVL, BB, BC, CA) from escort screens (DD, DE, PF, PC) and auxiliary echelons.
2. **Field Manual & Scope Integration:**
   - Codify the complete 7-class naval taxonomy and auxiliary doctrine into the authoritative scope bible (`docs/scope-v1.md` §6.2, §6.3) and in-app Field Manual (`assets/manual/witp_ae_manual.json` sections `1.15` and `1.16`).
   - Strictly strip all bracketed citation numbers `[...]`.

**Source:** Ozgur directives (2026-09-21 13:27 UTC+3).

## 48. Base Location Registry & Dynamic Hex Grid Database (2026-09-21 14:45 UTC+3)

**Decision (§1.15, §6.4):**
1. **Theater-Wide Ground Truth Registry:**
   - Ingest 1,330 authentic base locations with $(X, Y)$ hex coordinates across 71 geographical regions into `BaseLocationRegistry` (`assets/data/base_locations.json`, `docs/base_locations.json`, `src/campaign/base_registry.hpp`, `src/campaign/base_registry.cpp`).
   - Lookups are optimized via hash indexing: $O(1)$ by lowercase name and $O(1)$ by coordinate key `hexX * 1000 + hexY`.
2. **Maritime Housing Constraints:**
   - Base locations house ground units, naval vessels, and aircraft.
   - Only coastal/ocean-adjacent base locations can house ships (`isPort == true`). Inland bases (e.g. Kofu, Maebashi, Asahikawa, inland China/Manchukuo cities) only house ground and air units.
3. **Base Locations DO NOT Move (Dynamic Controller Architecture):**
   - **Base locations and their $(X, Y)$ hex coordinates are permanently fixed physical geography — base locations DO NOT move!**
   - Initial region/nation designations (e.g. `JAPANESE EMPIRE`, `PHILIPPINES`, `DUTCH EAST INDIES`) represent the turn 1 scenario setup.
   - Base ownership mutates as bases are captured during the campaign (`updateBaseController()`), preserving initial national origins while tracking live operational controllers, with coordinates remaining strictly immutable.
4. **Predictive Analytics Integration:**
   - **Model A (Amphibious Assault):** Dynamic coordinate resolution for any target base mentioned in SIGINT unit loadouts or troop movements.
   - **Model B (Dead Reckoning Trajectory):** Dynamic nearest-base proximity searches within projected confidence radii to identify approaching assault sectors and intercept windows.
   - **Model D (Logistical Pause):** Port status and turnaround monitoring across all theater ports and forward anchorages.
   - **Tab 2 Geospatial Sightings:** Distance and bearing calculated relative to authoritative base coordinates.
   - **Dossier Entity Search:** Complete indexed lookup and substring search across all 1,330 bases.

**Source:** Ozgur directives (2026-09-21 14:45 UTC+3).

## 49. Master Theater Hex Map Storage & Geospatial Calibration (2026-09-21 15:00 UTC+3)

**Decision (§4.5, §7.2, §15):**
1. **Asset Storage:** Store the authoritative Gary Grigsby's War in the Pacific: Admiral's Edition theater hex map raster image at `assets/maps/witp_ae_full_hex_map.jpg` (with mirror at `docs/witp_ae_full_hex_map.jpg`).
2. **Coordinate Standard:** Origin $(0, 0)$ is fixed at the top-left corner, matching the $(X, Y)$ coordinate system of PBEM reports, `.pws` data, and the 1,330 base location database.
3. **Strategic Applications Identified:**
   - **Land/Sea Hex Grid Masking:** Extract terrain boundaries to prevent naval contacts from dead-reckoning across landmasses.
   - **Maritime A* Pathfinding (Model B):** Enable nautical route pathfinding through navigable straits for realistic intercept windows.
   - **Chokepoint Interdiction (Model G):** Correlate merchant sightings against strategic narrows (Malacca, Sunda, San Bernardino, Makassar, Tsushima).
   - **Air Search Radius Overlays:** Calculate radar and visual search arcs around bases and task forces.
   - **In-Terminal CP437 Sector Radar Display:** Project localized sub-regions on Tab 2 SIGINT Map.

**Source:** Ozgur directives (2026-09-21 15:00 UTC+3).

## 50. Binary Build Counter Auto-Increment Baseline (2026-09-21 15:03 UTC+3)

**Decision (§1.13, §14):**
1. **Incremental Build Numbering:** Whenever compiling a new Windows release executable, the build counter in `witp-cic-[mainversion id]-[phaseid]-[built no]` must strictly increase by `1`.
2. **Current Active Binary:** `dist/witp-cic-v0.02-alpha2-1.exe`.
3. **Canonical Mirror:** `dist/WitP-CIC.exe` is updated concurrently.

**Source:** Ozgur directives (2026-09-21 15:03 UTC+3).

## 51. Six Mandatory Geospatial Predictive Intelligence Applications (2026-09-21 15:05 UTC+3)

**Decision (§1.16, §6.5):**
The Gary Grigsby's War in the Pacific: Admiral's Edition master theater hex map raster (`assets/maps/witp_ae_full_hex_map.jpg`, origin $(0, 0)$) powers six mandatory geospatial intelligence applications:
1. **Maritime Obstacle Avoidance & Nautical Pathfinding (Model B):** Binary land/sea hex collision mask enables A*/Dijkstra nautical pathfinding around land barriers (e.g. Luzon Strait, San Bernardino Strait), eliminating "ships sailing over mountains."
2. **Strategic Chokepoint & Convoy Interdiction (Model G):** Identifies natural narrows (Malacca, Sunda, Makassar, Surigao, San Bernardino, Tsushima) to forecast mandatory crossing windows for submarine ambush.
3. **In-Terminal Tactical Mini-Radar / Sector Map (Tab 2):** Renders a 40x20 CP437 ASCII/ANSI tactical sector radar centered on selected contacts with coastlines (`#`), friendly ports (`O`), and blinking contact vectors (`*`).
4. **Land-Based Air Search & Strike Umbrellas (Models A & F):** Concentric range rings around enemy airbases (3-hex torpedo strike, 5-hex medium bomber, 8-hex search) alerting to air danger zones and blind spots.
5. **Amphibious Beachhead Approach Lanes (Model A):** Combines target base coordinates with coastal maritime approach hexes to project assault transport vectors (AP, APA, LST) from staging bases to landing beaches.
6. **Hex Grid Calibration & Coordinate Ground-Truth:** Cross-references doubtful PBEM reports and `.pws` coordinates against visual ground truth.

## 52. Opponent-Only Intelligence & BDA Claims Filtering (2026-09-21 18:15 UTC+3)

**Decision (§4.5, §6):**
1. **Hostile-Only Asset Rule:** In the War Room / Intelligence CIC, all intelligence tracking screens, battle claims, damage revisions, and Battle Damage Assessments (BDA) must strictly track and display **ONLY THE OPPONENT'S ASSETS** (Japanese in an Allied campaign; Allied in a Japanese campaign).
2. **Exclusion of Friendly Forces:** Friendly casualties, own-force damage, and friendly sinkings are already managed by the player via operational reports, damage summaries, and ship manifests. Displaying them in enemy intelligence BDA screens creates confusion and polutes the intelligence picture.
3. **Filtering Architecture:**
   - In `WarRoomAnalytics`, `parseCombatReportFile()` parses both `Allied Ships` and `Japanese Ships`, recording `ev.isAllied = inAlliedShips;`.
   - In `parseCombatEventsFile()`, sinkings are disambiguated as Allied vs Japanese using ship naming markers, prefix patterns, and cached side tracking.
   - In `WarRoomScreen`, Tab 1 Intelligence Summary item `[4] [ BATTLE DAMAGE ASSESSMENT & CLAIMS REVISION REGISTER ]` and Predictive Intelligence Sub-Menu item `[4] [ MODEL E: BATTLE CLAIMS & DAMAGE REVISION REGISTER ]` filter `m_battleClaims.claims` by campaign player side (`isAlliedCampaign ? !c.isAllied : c.isAllied`).
   - If no hostile claims exist for the turn range, render `NO HOSTILE COMBAT DAMAGE CLAIMS RECORDED FOR THIS TURN RANGE.`

## 53. Bold Header Typography & Program-Wide Modal Wrapping Architecture (2026-09-21 18:27 UTC+3)

**Decision (§4.2, §4.5, §8.1, §8.2):**
1. **Bold Headers Program-Wide:**
   - All screen headers (`Layout::renderHeader`, `Layout::renderHeaderWithStats`), section titles (`Layout::renderSectionHeader`), and pop-up modal titles (`Layout::renderNotificationModal`, `WarRoomScreen::renderReasoningModal`, `runNavigablePopUp`, `SettingsScreen`, `WikiScreen`, `FieldManualScreen`) must render in **BOLD** typography.
   - Implemented by enabling Win32 `ENABLE_VIRTUAL_TERMINAL_PROCESSING` (`0x0004`) in `Console::init()` and emitting ANSI `\033[1m` before text and `\033[22m` after text via `Console::writeBold()` and `Console::writeLineBold()`.
2. **Zero Text Truncation & Program-Wide Wrapping:**
   - Ellipses truncation (`...`) and hard string slicing are eliminated across all boxes and modals.
   - Modal item titles (`ITEM: ...`) must wrap across multiple lines using inverted phosphor highlight (`ConsoleColor::Inverted`) when exceeding inner box width.
   - Telemetry evidence lines and notification messages must wrap cleanly without text cutoff or border overflow.
3. **Screen Isolation on Modal Entry:**
   - Pop-up modals (specifically `WarRoomScreen::renderReasoningModal`) must call `Console::clear()` upon opening to eliminate background screen bleed-through and edge artifacting (`[`, `==`, `--`, `3`, `+`).
4. **Elimination of Off-By-One Wrapping Drop Bug:**
   - Sizing calculations for modal content height and row loops must account for all header, title, divider, meter, and disclosure lines.
   - `renderReasoningModal` must render every single line of `wrappedLines` without dropping the final wrapped line.
5. **Pop-up Geometry Maximization:**
   - Pop-up modal width is expanded to 78 columns (`startX = 1`, `innerWidth = 74`), maximizing horizontal space and minimizing unnecessary line wrapping.

**Source:** Ozgur directives (2026-09-21 18:27 UTC+3).

## 54. Hierarchical Reasoning Modal Drill-Down & Merged Telemetry Architecture (2026-09-21 18:40 UTC+3)

**Decision (§4.5, §8.1, §8.2):**
1. **Hierarchical 2-Tier Modal Navigation Architecture:**
   - The reasoning modal is transformed into an interactive 2-tier modal structure.
   - **Tier 1 (Topic Directory View):** Displays the bold modal header, wrapped item title, confidence meter bar (if applicable), and an interactive list of topics traversed via `[UP/DOWN]` cursor keys or numeric hotkeys (`[1]`, `[2]`, `[3]`).
   - **Tier 2 (Granular Evidence Drill-Down View):** Triggered when the user presses `[ENTER]` on a topic. Displays a focused pop-up showing the granular evidence for that topic with wrapped lines and indentations. Pressing `[ESC]` or `[ENTER]` returns directly to Tier 1.
2. **Merged Telemetry & Localization History:**
   - `TELEMETRY SOURCE` and `LOCALIZED POSITION` are unified into `[1] TELEMETRY SOURCE & LOCALIZATION HISTORY`.
   - Drilling down reveals: detection means (SIGINT, visual recon, air patrol), localized coordinates, observation frequency, and tracking chronology (dates seen, until when tracked).
3. **On-Demand Confidence Computation:**
   - Mathematical calculations (`Base Score`, `Corroboration`, `Time Decay`, `Final Calculation`, `Analyst Note`) are completely hidden from the Tier 1 view.
   - They are segregated into Topic 3 (`[3] CONFIDENCE DERIVATION & MATHEMATICAL PROOF`), accessed exclusively via explicit drill-down.
4. **Program-Wide Text Cutoff Elimination:**
   - Word wrapping helper `wrapText(line, innerWidth, 4)` is enforced for all content lines.
   - All detail lines are padded to exact `innerWidth`, guaranteeing symmetric vertical borders and zero clipping.
5. **Screen Isolation:**
   - `Console::clear()` is executed on every modal launch and tier transition, preventing background text bleed.

**Source:** Ozgur directives (2026-09-21 18:40 UTC+3).

## 55. Header Turn & Item Count Disambiguation and Multi-Day Ingestion Telemetry Architecture (2026-09-21 18:54 UTC+3)

**Decision (§4.2, §4.5, §8.2):**
1. **Header Label Conflation Elimination:**
   - In `Layout::renderHeader(screenTitle, turnDate)`, the string is checked before prepending `"Turn: "`. If the subtitle already contains `"Turn:"`, `"ITEMS:"`, `"COUNT:"`, `"MATCHES:"`, `"ALL "`, or `"STRATEGIC"`, the string is displayed verbatim without appending a redundant and confusing `"Turn: "` prefix.
2. **Explicit Multi-Day Campaign Telemetry in Pop-Up Headers:**
   - In `WarRoomScreen::runNavigablePopUp(title, bullets, reasonings, confidences, turnInfo)`, the header right subtitle is instrumented to display the full campaign status alongside the item count:
     `Turn: <FormattedDate> (<N> Days Loaded) | ITEMS: <Count>`
   - This completely eliminates ambiguity between list element counts (`ITEMS: 3`) and campaign turn progress (`Dec 15, 1941 (8 Days Loaded)`).
3. **Multi-Day Telemetry Ingestion Verification:**
   - In `WarRoomAnalytics::analyzeCampaign`, all files present in `config.manifest.files` across all dates are processed. Multi-day campaigns (e.g. `D1.wic` with 8 operational dates: Dec 07 through Dec 15 across 60 files) are parsed into all analytical engines without dropping historical sighting tracks, pilot kills, damage reports, or geospatial records.

**Source:** Ozgur directives (2026-09-21 18:54 UTC+3).

## 56. Alpha 2 Build 5 Recovery Point & Zero-Contamination Checkpoint Protocol (2026-09-21 20:41 UTC+3)

**Decision (§1.13, §3, §14):**
1. **Zero-Contamination Pre-Addition Checkpoint:**
   - Prior to introducing large-scale functional additions, a pristine, fully verified recovery point is created with zero risk of mixing future code with previous stable efforts.
2. **Multi-Layered Preservation Hierarchy:**
   - **Git Tag:** `checkpoint-pre-major-addition-alpha2-build5` stamped immutably at commit `7295684`.
   - **Dedicated Recovery Branch:** `recovery-point-alpha2-build5` branched from this baseline.
   - **Standalone Physical Archive:** `backups/WITP-CIC2-RECOVERY-POINT-alpha2-build5.zip` (42,101,712 bytes) containing source, tests, assets, docs, and the compiled `dist/witp-cic-v0.02-alpha2-5.exe` binary.
   - **Recovery & Rollback Guide:** `docs/recovery-point-alpha2-build5.md` providing step-by-step restoration commands (Git hard reset, branch switch, and physical zip extraction).

**Source:** Ozgur directives (2026-09-21 20:41 UTC+3).

## 57. Deprecation & Deletion of Legacy Predictive Modules C, D, and E (Partisan) from Scope and To-Do (2026-09-21 20:43 UTC+3)

**Decision (§7.2, §7.3):**
1. **Formal Removal of Deprecated Modules:**
   - Per explicit user directive ("delete the following from todo and scope ... no longer needed"), the following legacy predictive modules are formally retired and excised from `docs/scope-v1.md` and `docs/todo.md`:
     - **Module C — Air Superiority & Attrition Forecasting:** Tracking airframe write-offs vs replacement arrivals, forecasting local air superiority loss date per theater, outputting air attrition curves.
     - **Module D — Logistical & Operational Pause Detection:** Monitoring TF fuel reserves and transport delays from ops reports, predicting operational pause windows when hostile fleets must return to port.
     - **Module E — Internal Security & Partisan Threat Modeling:** Correlating garrison troop density with historical partisan vectors (Colombo, Howrah) to issue reinforcement alerts.
2. **War Room Lettering Alignment:**
   - In the active War Room implementation, Model E remains dedicated to Battle Damage Assessment & Claims Revision Register (cross-turn entity match between reported sinkings and live re-sightings / `.pws` tombstone reconciliation).
   - Pruning legacy Modules C, D, and E removes extraneous engineering overhead and sharpens the analytical focus for upcoming major capability additions.

**Source:** Ozgur directives (2026-09-21 20:43 UTC+3).

## 58. Permanent Deletion of Game File Writing/Modification & Read-Only Ingestion Guarantee (2026-09-21 20:46 UTC+3)

**Decision (§10, baseline principle):**
1. **Permanent Scope & To-Do Deletion:**
   - Any prospective capability or future consideration to write to, alter, or modify game files (previously deferred to v1.1+ in §10) is permanently deleted from `docs/todo.md`, `docs/scope-v1.md`, and the product roadmap.
2. **Absolute Read-Only Guarantee:**
   - WITP-CIC is formally codified as an entirely passive, read-only intelligence tool.
   - The application inspects exported text logs and decodes `.pws` PBEM saves solely in memory; it will never write back, alter, or inject data into game files or runtime game processes across any version (v1.0 or beyond).

**Source:** Ozgur directives (2026-09-21 20:46 UTC+3).

## 59. Deletion of Battle Claims Corrections & Model E from Scope and To-Do (2026-09-21 20:48 UTC+3)

**Decision (§4.2, §4.5, §7.2):**
1. **Scope & Roadmap Deletion:**
   - Per explicit user directive ("delete the following from todo and scope: Battle Claims Corrections: BDA claim vs later contradiction, displayed as revision under 4.2 Tab 1 — Intelligence Summary (§4.5)"), Battle Claims Corrections is completely excised from `docs/scope-v1.md` and `docs/todo.md`.
2. **Model E Engine Retirement:**
   - The corresponding cross-turn entity reconciliation engine (Model E) is removed from active to-do tracking.

**Source:** Ozgur directives (2026-09-21 20:48 UTC+3).

## 60. Milestone & Phase Progression to Alpha 3 (2026-09-21 20:53 UTC+3)

**Decision (§1.13, §3, §14):**
1. **Phase Label Designation:**
   - The project development phase is officially labeled **Alpha 3** (`v0.02-alpha3`), advancing from the Alpha 2 baseline.
2. **Build Number Reset & Binary Naming:**
   - Following the structured naming convention `witp-cic-[mainversion id]-[phaseid]-[built no]`, the build number resets strictly to `0` for this new phase: `dist/witp-cic-v0.02-alpha3-0.exe`.
   - The canonical root mirror `dist/WitP-CIC.exe` remains updated in lockstep.

**Source:** Ozgur directives (2026-09-21 20:53 UTC+3).
