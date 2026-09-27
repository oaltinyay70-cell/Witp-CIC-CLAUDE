# witp-cic-rochefort — Project Context

Facts only. If it is not written here, in `docs/scope-v1.md`, or in a later user message, it is unknown. Do not fill gaps.

## What this is

- Product name: **ROCHEFORT 6000 // WITP-CIC Intelligence System**
- Active Milestone: **Alpha 4** (Active 2026-09-22 21:45 UTC+3)
- Executable Naming Standard: `witp-cic-rochefort-alpha4-[build#].exe` (e.g. `witp-cic-rochefort-alpha4-13.exe`; build counter increments by 1 per compilation; zero mirrors/aliases)
- Active Release Binary: `dist/witp-cic-rochefort-alpha4-13.exe` (Single authoritative versioned binary; no mirrors)
- Active Workspace: `C:\Users\ozgur\Documents\witp-cic-rochefort`
- Preserved Baseline: **Version 0.02 (Stable Alpha 1)** (Tagged `v0.02`, `v0.02-alpha.1`, branch `stable-alpha-1`, archive `backups/WITP-CIC2-v0.02-alpha1.zip`)
- Rollback Guide: [`docs/how-to-rollback-to-alpha-1.md`](docs/how-to-rollback-to-alpha-1.md)
- Mandatory Upgrade SOP: All future upgrades must execute the 6-step verification, docs sync, backup archive, tagging, GitHub push, and rollback guide maintenance cycle.
- Working / install naming in spec: **witp-cic-rochefort**, **ROCHEFORT 6000** (default install `C:\WitP-CIC`)
- Kind: Desktop PBEM intelligence helper for *War in the Pacific: Admiral's Edition* (Matrix Games); CRT phosphor hardware-accelerated read-only CIC-style intelligence dashboard
- Git repository: **witp-cic-rochefort** — https://github.com/oaltinyay70-cell/witp-cic-rochefort.git (active `main` branch). Upstream reference archive: `WITP-CIC2` (https://github.com/oaltinyay70-cell/WITP-CIC2)
- Design spec bible: `docs/scope-v1.md` (received 2026-09-15 from Ozgur; FAQ same date)
- Architecture decisions (v1): `docs/architecture-decisions.md` (2026-09-15 15:37 UTC)
- Comprehensive implementation checklist: `docs/todo.md` (derived from spec; check off only after writing and validating code)
- Game manual for Wiki/RAG: `assets/manual/witp_ae_manual.json` / `docs/witpae-manual.txt`

## Quality rules (non-negotiable)

- No hallucination
- No assumptions — ever
- If anything is missing or ambiguous, ask
- Pixel-perfect, nano-specific — treat correctness as production-critical
- Requirements traceability: new business requirements must be added to the spec bible with date/time (§3)
- **Q&A documentation rule (2026-09-19 18:22 UTC+3 — Ozgur):** Every answer Ozgur gives to any open question must be immediately recorded in `docs/scope-v1.md` changelog (date-stamped) and mirrored into the relevant design doc. Agent writes the entry before any implementation begins. No exception, no session boundary excuses.
- **Baseline Definition & Rule (2026-09-19 20:25 & 20:57 UTC+3 — Ozgur):** The Baseline is the authoritative standard of verified completion. **NEVER mark anything done (`[x]`) unless Ozgur explicitly says so.** Passing automated tests and building binaries are technical milestones; only Ozgur's explicit command marks an item complete.
- **Baseline Scope Mandate & Conflict Clarification Gate (2026-09-20 15:05 UTC+3 — Ozgur):** Every new todo item and correction MUST be immediately added to the business scope (`docs/scope-v1.md`). If there is any conflict between a new requirement and existing scope/architecture, the agent MUST ask Ozgur to clarify before taking action.
- **Core Docs Suite (2026-09-19 20:57 UTC+3 — Ozgur):** Any and all documentation regarding Baseline Principles (`docs/baseline_witp_cic.md`), To-Do lists (`docs/todo.md`), Lessons Learned (`docs/lessons-learned.md`), Design & Architecture (`docs/architecture-decisions.md`), Flow docs (`docs/screen-flow.md`, `docs/screen-flow-text.md`), Scope (`docs/scope-v1.md`), Project Context (`docs/project-context.md`), and UI specifications are **Core Docs**. They must always be synchronized, date-stamped, and actively maintained.
- **Lessons Learned Protocol (2026-09-19 20:57 UTC+3 — Ozgur):** After every correction or design clarification, the agent must immediately log what was learned and what NOT to repeat in `docs/lessons-learned.md`.
- **Deterministic Numerical Confidence Standard (2026-09-20 22:08 & 22:27 UTC+3 — Ozgur):** Subjective confidence descriptions are strictly prohibited. Confidence must be calculated as a numerical percentage (0%–100%) and tier (`UNCONFIRMED`, `PROBABLE`, `CONFIRMED`, `VERIFIED`) via a 4-phase deterministic pipeline. The parser evaluates corroboration within the turn file only (`Option B`); the Game State Manager handles multi-turn contact persistence and resets Time Decay to 1.0 on recurrence. Visual recon is scored as `VISUAL_RECON` (Base 80) and corroborates `HexMap` only. Pop-ups display clean headers paired with full plain-English mathematical breakdowns in the reasoning body.
- **Opponent-Only Intelligence Display Rule (2026-09-21 18:15 UTC+3 — Ozgur):** In the Intelligence War Room, BDA, damage claims, and tracking menus must strictly display ONLY the opposing side's assets (Japanese in an Allied campaign; Allied in a Japanese campaign). Friendly casualties, sinkings, and damage must never appear in enemy intelligence tracking screens, as the commander already manages friendly status via operational reports.

## Who does the work

- Business analysis, coding, testing, and installing needed tools are the agent’s job
- Cheaper catalog models first (student budget); escalate only if needed, high-stakes, or a model is named
- Cheaper model already selected for implementation (per project coordination)

## Version 1 platform and stack (decided 2026-09-15 15:37 UTC)

- **Ship first:** Windows; default install `C:\WitP-CIC`
- **Campaign files:** only under `C:\WitP-CIC\Campaigns` (one compressed Campaign File per campaign)
- **Primary implementation:** C++ → Windows console `.exe`; Win32 TUI per §8.1 (codes 2, 10, 160)
- **Analytics storage:** graph, time-series, and hex-spatial models in-process; persist into the Campaign File — no Neo4j, InfluxDB, or PostGIS services or data directories

## Version 1 UI aesthetic (2026-09-15 ~15:44 UTC; name decided ~15:47 UTC)

- **Ozgur:** “the UI must be a perfect copy of the ROCHEFORT from the ALien movie” (ship-computer green phosphor look).
- **On-screen display name (decided ~15:47 UTC):** **WU/TH/UR** (slashes as shown).
- **Implementable spec:** `docs/scope-v1.md` §8 — Win32 console codes 2, 10, 160; header/body/footer (§8.2–§8.4) as written until scope changes them; interaction and modal rules unchanged there.
- **Master Menu Header Instrumentation (2026-09-20 15:07 UTC+3):** Consolidate campaign and file/days telemetry into top header bar (`Files Processed : XX pws files | xx TXT files | Days Processed : xx`, partitioned by `|`).
- **Clean Selection Highlighting (2026-09-20 14:18 UTC+3):** Active options use clean `Inverted` highlight without broken ASCII `-` or `_` characters.
- **Right-Aligned Pop-up Explanations (2026-09-20 15:07 UTC+3):** Topic and option descriptions reside in a framed pop-up box to the right (columns 43-78) with compact word-wrapping and dimmed phosphor (`DarkGreen`).
- **Acoustic Profile (2026-09-20 14:20 & 14:21 UTC+3):** Restrained, sombre mechanical relay clicks (450 Hz, 5 ms) on cursor movement; background radio noise deleted (silent audio background).
- **Framed Pop-up Modals (2026-09-20 15:07 UTC+3):** Module transitions (War Room launch, Settings) appear inside framed modal dialogs rather than overwriting screen text.
- **Bold Headers Program-Wide (2026-09-21 18:27 UTC+3):** All screen headers, section headers, and pop-up modal titles render in **BOLD** Light Green typography via Win32 Virtual Terminal Processing (`ENABLE_VIRTUAL_TERMINAL_PROCESSING` / `0x0004`) and ANSI `\033[1m`.
- **Zero Text Cutoff & Full Multi-Line Wrapping (2026-09-21 18:27 UTC+3):** Program-wide multi-line wrapping eliminates ellipses (`...`) and string slicing across all modal titles, telemetry disclosures, and prompts. Modals clear the screen on entry to eliminate background text bleed.
- **Reference images:** none provided; do not invent splash taglines.
- **Spec traceability:** §8 historical heading “W.U.T.H.U.R.” remains in the bible; do not rewrite without a superseded note.
- **Not assets:** film frames / copyrighted film screens are not design or layout asset sources (`docs/architecture-decisions.md` §5).

## Spec-stated constraints (summary)

- Zero data leakage: purge in-memory knowledge base on start, campaign switch, exit (§3)
- Absolute sandbox between campaigns and sides (§3)
- Campaign data in a single compressed Campaign File under `\Campaigns` only (§3; Changelog 2026-09-15 15:37 UTC)
- Product is read-only, local-only, single-player; no cloud or multi-user (§1, §10, FAQ)
- Version 1 in scope: Sections 4–8 plus golden-file test suites (§10)
- Release gate: 100% automated test pass; unparsed lines &lt; 1.0% (§11)

## Golden samples (facts — Ozgur 2026-09-15 15:40 UTC)

- Twelve plain-text turn outputs in `docs/golden/`; catalog `docs/golden/INDEX.md` (byte/line counts unchanged).
- Prefix rule unchanged: `a*` → Allied per-side files; `j*` → Japanese per-side files.
- Japanese `jsigint_*` and `joperationsreport_*` at very small size are **intentional fog of war** (header-only SIGINT; operational report header plus Batan Island)—**not** incomplete or missing data in the golden set.
- `Combat_Events_*` and `combatreport_*` are **shared by filename** between players; written content may **differ (delta)** because of fog of war. This drop contains **one file per filename/date**; do not assume identical Allied/Japanese content or fabricate a second copy per side.
- **Golden `.pws` (forensics 2026-09-18; context 2026-09-19):** **Seven** saves in project store catalogs — six `wpae021_*.pws` (Japanese → Allied / Ozgur) plus **`wpae023.pws` (Ozgur outgoing Allied → Japanese)**. Partner **Dave** = Japanese player. T1 = **12/08/41** exchange (label 12/07); played **12/09–12/13**. Format: gzip → 61 zlib records keyed by `a&0xff`; golden set needs **no file password** to decompress. Proven ship tombstone fields in [`docs/pws-field-map.md`](pws-field-map.md). Index: [`docs/pws-knowledge-summary.md`](pws-knowledge-summary.md), [`docs/pws-wpae021-catalog.md`](pws-wpae021-catalog.md), [`docs/pws-wpae023-catalog.md`](pws-wpae023-catalog.md), [`docs/pws-inspection.md`](pws-inspection.md).
- Spec §5 “unlock via password” for **full telemetry** remains **not demonstrated**; product should probe plain gzip/zlib and use evidence-gated fields only.
- **PBEM save extension (Ozgur 2026-09-15 15:43 UTC):** WITP-CIC canonical extension **`.pws`** (spec §5; `wpae023.pws`; Ozgur: “pws... my bad sorry”). Manual §20.4.1 / §20.5.4 cite **`.wps`** — retain as manual text; superseded for WITP-CIC file matching only.

## Still waiting (per spec)

- Incoming-side golden `.pws` track: six `wpae021` catalogs; outgoing reference: `wpae023` (not a second “Japanese-only” mirror file)
- GitHub private repo WITP-CIC-C (GitHub authentication and owner login)

## Operating loop (from baseline + spec)

1. Scope v1 received (`docs/scope-v1.md`, 2026-09-15)
2. Comprehensive todo derived (`docs/todo.md`)
3. Receive sample/golden files — plain-text set + **seven** `.pws` catalogs (six `wpae021` JP→Allied + `wpae023` outgoing); see [`docs/pws-knowledge-summary.md`](pws-knowledge-summary.md)
4. ~~Resolve blocked contradictions with Ozgur~~ **Done (2026-09-15 15:37 UTC)** — see `docs/architecture-decisions.md`
5. Implement, test against golden files and §11 gates only
## Working tree (Ozgur 2026-09-21)

- **Sole workspace:** `C:\Users\ozgur\Documents\witp-cic-rochefort`. Edit, build, test, and git commit **only** here. See **`CANONICAL.txt`** at repo root.
- **Upstream Baseline:** `Witp-CIC2` (`C:\Users\ozgur\Documents\Witp-CIC2`) is locked as an immutable reference archive.

## Active Milestone: Alpha 4 (2026-09-26 19:30 UTC+3)

- **Active Milestone:** **Alpha 4** (`alpha4` - Build 13)
- **Binary Naming Format:** `witp-cic-rochefort-alpha4-[build#].exe` (e.g. `dist/witp-cic-rochefort-alpha4-13.exe`; single versioned binary; zero mirrors).
- **Hierarchical Reasoning Modal Navigation:** 2-tier navigable modal; cursor traversal (`[UP/DOWN]`); unified Telemetry Source & Localization History drill-down; on-demand confidence computation drill-down; program-wide word-wrapping with zero text cutoff.
- **Opponent-Only Intelligence Display:** War Room BDA and Model E claims register filter strictly for enemy assets; friendly losses excluded.
- **Naval Unit Capabilities Baseline:** 7 functional classes & 40+ hull codes, plus Auxiliary / Tender Doctrine (AS, AD, AV/AVD/AVP, AR, ARD, AE/AKE, AO, APD, ACM, AKV, AMc). All predictive models (A, B, D, F, G) and tactical reasoning pop-ups must be grounded in these capabilities.
- **Base Location Registry Baseline (§6.4):** 1,330 authentic theater base locations across 71 regions with $(X, Y)$ hex coordinates. Coastal bases can house ships (`isPort == true`); inland bases house ground/air units only. Dynamic controller tracking upon capture events. Predictive models (A, B, D), Tab 2 bearings, and Dossier searches grounded in this database.
- **Sole Repository:** `https://github.com/oaltinyay70-cell/witp-cic-rochefort.git` (canonical `origin main`). Upstream archive: `https://github.com/oaltinyay70-cell/WITP-CIC2.git`. All Cursor remotes and branches permanently purged.
- **Workspace:** `C:\Users\ozgur\Documents\witp-cic-rochefort`.



