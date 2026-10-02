# WITP:AE CIC Intelligence Helper — ROCHEFORT 6000 Active To-Do List

**Interactive Acceptance Rule (2026-09-20 19:21 UTC+3 — Ozgur):** Every single requirement in this document has a checkbox (`- [ ]`). When Ozgur clicks it (in GitHub or markdown view) or comments "done", it is officially accepted and marked done (`- [x]`). Writing code and passing tests is a prerequisite, but checking off an item requires Ozgur's explicit action or confirmation.

**Scope Addition & Conflict Clarification Rule (2026-09-20 15:05 UTC+3 — Ozgur):** Every new todo item and correction MUST be added to the business scope (`docs/scope-v1.md`). If there is any conflict between a new requirement and existing scope/architecture, the agent MUST ask Ozgur to clarify before taking action.

**Core Docs Rule (2026-09-19 20:57 UTC+3 — Ozgur):** All documentation covering baseline principles (`docs/baseline_witp_cic.md`), to-do lists (`docs/todo.md`), lessons learned (`docs/lessons-learned.md`), design decisions (`docs/architecture-decisions.md`), screen flows (`docs/screen-flow.md`), scope (`docs/scope-v1.md`), project context (`docs/project-context.md`), and UI specifications are Core Docs. They must be maintained and synchronized continuously.

**Current Active Phase:** `alpha4` | **Build:** `27` | **Version Badge:** `v-alpha4-build27`

---

## Active & Upcoming Directives (New Turn / Ready For Input)

- [ ] **[TODO-NEW-01]:** Define and prioritize the next feature/enhancement for Phase `alpha4` (Awaiting Ozgur's directives).
- [ ] **[TODO-NEW-02]:** Monitor and test incoming `.pws` turn file drop in `C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE` for subsequent PBEM turns.
- [ ] **[TODO-NEW-03]:** Continuous verification of Faction Isolation (Allied side = 1 only; 0 Japanese data exposed).
- [ ] **[TODO-NEW-04]:** Ensure single-file standalone executable packaging (`dist/witp-cic-rochefort-alpha4-[build#].exe`) remains green and test suite maintains 100% pass rate.

---

## Recently Completed & Accepted Work (Archived Baseline)

- [x] **[PWS INGESTION & RETENTION 2026-09-30]:** Restrict `.pws` save file processing exclusively to canonical `SAVE` directory (`C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE`), store all historical turn snapshots chronologically in `PwsRepository`, calculate deltas (fuel, arrival days, sunk ships), and designate most recent save as active authority.
- [x] **[SPLIT-PANE UI REFACTOR 2026-09-30]:** Resolution-independent split-pane console architecture implemented across 1024x640, 1366x768, 1920x1080, and 2560x1440. Upper pane with ellipsis clipping, lower pane with reasoning topics and evidence, dynamic double-line active window borders, outside navigation prompt, and lower-right version badge `v-{Phase}-build{BuildNumber}` (`v-alpha4-build27`).
- [x] **[DATE FORMAT & FILTER STANDARDIZATION 2026-09-30]:** Standardized all dates to `[dd/mm/yy]` across all screens. Updated temporal filter header to `[1]LATEST *[2]7D [3]15D [4]30D [5]ALL | [R]RANGE` with zero mention of targets in the command strip.
- [x] **[CLEAN EXIT MODAL 2026-09-30]:** Clean process shutdown on pressing `[Y]` in Exit Confirmation Modal (terminates process with code 0 instead of returning to boot screen).
- [x] **[14 FOUNDATIONAL ARCHITECTURAL PILLARS 2026-09-30]:** Streaming parser, temporal decay, spatial clustering, entity resolution state machine, hex geospatial math, reactive world state, rules engine, sensor reliability, supply network flow, air denial heatmap, Bayesian corroboration, advance momentum prediction, cross-file temporal continuity (Pillar 13), and persistent hazard layer.
- [x] **[REBRANDING & CRT FLICKER DEFANGING 2026-09-28]:** Universal removal of MUTHUR wording, rebrand to ROCHEFORT 6000, and hard-reset of CrtGlitchEnabled to false (Zero Flicker Policy).
- [x] **[INTERACTIVE CALENDAR PICKER 2026-09-29]:** Interactive keyboard-navigable calendar pop-up window (`[S]` Single Date, `[C]` Custom Range) clamped between `12/07/1941` and `12/31/1946`.
