# ROCHEFORT 6000 (WITP-CIC) — Current Specification

Status: DRAFT for owner approval, 2026-10-02. Replaces the C++-era scope bible
(archived in `docs/archive/`, kept for history; where it conflicts with this file, this file wins).

## 1. Purpose
Offline, read-only intelligence helper for *War in the Pacific: Admiral's Edition* PBEM play.
It ingests each turn's text reports and the `.pws` save, keeps all history, and presents a
green-phosphor "Combat Information Center" terminal UI. v1 serves the **Allied** player only.

## 2. Invariants (never violate)
1. **Offline:** no network code at all. No AI/LLM endpoints.
2. **Read-only:** never writes to or modifies game files.
3. **Faction isolation / fog of war:** Allied campaigns only in v1. A `.pws` contains true enemy
   data the player must not see; the parser decodes only Allied records and Japanese rows never
   enter any model, store, or vault.
4. **Never lose history:** every ingested turn is retained; new turns are added, not replaced.
5. **Single exe, no mirrors:** `dist/witp-cic-rochefort-alpha4-<build>.exe`; build number comes from
   `Core/Common/AppVersion.cs`, bumped by `build_release.ps1`.
6. **Zero flicker, 106-column straight-border monospace UI**, dates shown `dd/mm/yy`, newest first.
7. **Tests only use test data:** `sample_data/` and `docs/golden/` are never scanned by real campaigns.

## 3. Stack (actual)
C# / .NET 8 / WPF, single-file publish, SharpZipLib (pure managed) for zlib. Solution
`ROCHEFORT6000.slnx`; tests in `tests/ROCHEFORT6000.Tests` (console runner, assertion-style).
Layout: `Core/Ingestion` (scanner, report + PWS parsers), `Core/Analytics` (WarRoomAnalytics,
heuristics, telemetry, pillar services), `Core/Database` (base registry, time-series store),
`Core/State`, `Core/Controller` (TerminalScreenManager, split-pane engine), `Terminal`, `Settings`.

## 4. Data sources (verified against the real SAVE folder, 2026-10-02)
Game folder: `C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE`
- `archive\` — dated reports `aoperationsreport|asigint|combatreport|Combat_Events|joperationsreport|jsigint_YYMMDD.txt`
  (currently 19 days, 411207–411226; 411209 absent = exchange turn with no play).
- Live current-turn files in SAVE root, **undated names** (`aoperationsreport.txt`, `asigint.txt`,
  `combatreport.txt`, `CombatEvents.txt` — note no underscore). Live ops/combat are byte-identical to the
  newest archive day, so they must be de-duplicated by date+type, not counted twice.
- `CombatEvents.txt` header uses `MM/DD/YY` and is stamped the **next** day (12/27/41) vs "Dec 26, 41"
  in the other files. Date normalisation must handle both.
- `wpae0NN.pws` saves (001–004, 021–023 present): gzip + 61 zlib records (see `docs/pws-knowledge-summary.md`,
  `docs/pws-field-map.md`). Ship rows 464 B; sunk = `19999` @36; hex @352/354. Live hex/fuel/arrival
  offsets are treated as verified (cross-checked via `assets/Allied_Fleet_25Dec41.xlsx`).
- **Campaign save rule (owner, 2026-10-02):** only the one `.pws` that belongs to the campaign is used. For this
  campaign it is `wpae021.pws`; for other campaigns it may be another number. `wpae001–004` are game-special files,
  not saves; `wpae022/023` are not used. The wizard therefore asks which `.pws` is the campaign save, and the vault
  remembers it. The game overwrites that file each turn, so each turn's decoded Allied snapshot is stored in the vault.
- Ignore: `updateReport.txt`, `autosave\`, `SAFE ARCH\`, `archive.rar`, `placeholder.txt`.
- Japanese-prefixed files (`j*`) are fog-of-war stubs; not used.

## 5. Campaign vault (`.wic`) — APPROVED 2026-10-02
Single ZIP container (System.IO.Compression, no new dependency) at `C:\WitP-CIC\Campaigns\<name>.wic`:
- `manifest.json` — campaign name, side (Allied), source folders, schema version, ingested-file ledger
  (name, type, date, size, hash) so Continue only ingests what is new.
- `reports/<YYMMDD>/<type>.txt` — raw turn text, kept verbatim (this is the permanent history).
- `pws/<YYMMDD>.json` — **Allied-only** decoded snapshot per save (never the raw `.pws`, which holds enemy data).
- `derived/` — optional cached analytics; always rebuildable from the above.
On load: purge in-memory state, read vault, scan source folders for files not in the ledger, ingest, rewrite vault.

## 6. UI (current -> target)
Flow: Boot -> Start (New/Continue/Exit) -> Wizard (side, password-for-reference, folders, name) ->
Ingestion -> Master Menu (War Room, Wiki, Field Manual, Settings, Exit).
War Room today: Graphics (3 working pages, rest TBD) and Heuristics (Regional Power Shift).
Decision 2026-10-02: Intelligence Summary, SIGINT Map, TPIE and Predictive Models are **deleted**;
their functions will be re-homed on pages the owner will define.
Settings: AI-endpoint options removed (offline). Thresholds retained.
Wizard: Japanese side option removed for v1; password step kept as reference-only text.

## 7. Decision log
- 2026-10-02 Home = this folder; baseline commit `a60100e` (tag `baseline-takeover-alpha4-33`).
- 2026-10-02 Keep C# WPF; archive C++-era docs. Lean docs: this SPEC + CHANGELOG + TODO. MegaMemory dropped.
- 2026-10-02 Allied only for v1. Real campaigns never scan sample/golden data. Foundations (vault, scan hygiene) first.
- 2026-10-02 Old backups/exes moved to `_archive/` (git-ignored); kept newest backup and build 33.

## 8. Parked analytics ("pillars") — kept as a list, no placement decided
Streaming parser; temporal decay; hotspot clustering; entity resolution + state machine + event bus; hex geo-math;
reactive world state; strategic rules engine; sensor reliability weighting; supply-network graph; air-denial heatmap;
Bayesian corroboration; advance-momentum prediction; temporal-gap detection; hazard/minefield layer; semantic concept graph.
Code stays in `Core/Analytics`; none is being extended until the owner assigns pages for them.
2026-10-02 decision: parked pillars do not run during ingest (code + unit tests kept). Exceptions still running:
sensor reliability weighting and temporal decay (confidence on retained data); hex geo-math and hazard types are core infrastructure.

## 9. Tooling findings (2026-10-02)
- Cloud side: .NET 8 SDK installed; NuGet and the Windows desktop pack are blocked, so the WPF app cannot be built
  there. The non-UI core (`Core/` minus `Controller`) compiles there with a stand-in for SharpZipLib, so new core
  code (e.g. the vault) can be compile-checked and unit-tested before you build.
- Your PC builds the real app (`build_release.ps1`). Exact commands are given with each change; paste back output.
- GitHub: no account is linked to this session, so pushes are done by the owner (see CHANGELOG).

## 10. Language / UI latitude
Owner allows a new language and/or new UI at my discretion. Current position: keep C#/.NET (existing 20k lines,
parsers and tests are reusable); the UI shell (WPF terminal text grid) may be replaced later once the data layer is solid.

## 11. Open questions
1. Which new pages should host the deleted Intelligence Summary / SIGINT Map / TPIE / Predictive functions? (owner, later)
