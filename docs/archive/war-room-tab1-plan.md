# War Room Tab 1 — implementation plan (next dev phase)

Aligns with [milestones.md](milestones.md) step 2 and [scope v1](scope-v1.md) §4.5. **2026-09-19.**

## Goal

Replace the War Room **stub** with **Tab 1 — Intelligence Summary** that reads **parsed data already in the `.wic` vault** (no `.pws` telemetry required for v1 of this screen).

## Data source (existing)

After New Campaign / Continue ingest (`33f728b` on local `~/Witp-CIC`):

- SIGINT, operational report, combat report, combat events → structured parse output inside the campaign zip vault.

War Room must **not** treat raw text as ground truth (§6 belief model comes later); for this slice, **display ingested lines / summary rows** as “reports in vault” with source tags.

## UI (§8 TUI)

- Entry: Master Menu **`[A] War Room`** → War Room shell with tab label **Intelligence Summary** (Tab 2+ stubs OK).
- Body: scrollable list (arrow keys / PgUp/PgDn if already used elsewhere) of intelligence rows, e.g.:
  - SIGINT lines (from latest ingested `asigint_*` or vault equivalent)
  - Combat event section headers or first N event lines
  - Optional: count of ingested files / last ingest timestamp from vault manifest
- **ESC** → Master Menu.
- Colors: Win32 attributes 2, 10, 160; header **WU/TH/UR**.

## `.pws` corrections (same milestone branch)

Per [pws-knowledge-summary.md](pws-knowledge-summary.md) and [architecture-decisions.md](architecture-decisions.md) §6:

1. Remove or reword any comment/code path that **requires ZipCrypto** or **refuses** to read `.pws` without password.
2. Add `pws_container` (or similar): gzip decompress → walk 12-byte headers + zlib; count record types; extract cleartext from record `0x26` (label) and optionally list sunk ships from `0x08` using **464-byte stride** and offset **36 == 19999** only.
3. Wizard may still **store** PBEM password for user reference; do not use it for zlib.

## Acceptance

- Build `WitP-CIC.exe`; Continue campaign with golden text paths → War Room shows non-empty list from vault.
- Unit tests: existing parser tests still pass; add test for gzip-only `.pws` probe on golden file if copied into `tests/fixtures/`.

## Local repo

Implement on **`~/Witp-CIC`** (WSL). Do not push unless Ozgur asks.
