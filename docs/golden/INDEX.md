# Golden WITP:AE game output samples — catalog

Canonical directory: `docs/golden/`

User-stated side mapping (prefix only): filenames starting with `a` → Allied; filenames starting with `j` → Japanese. No side assignment for other filenames.

**Ozgur 2026-09-15 15:40 UTC:** Japanese `jsigint_*` / `joperationsreport_*` at tiny size are **real fog of war**, not incomplete samples. `Combat_Events_*` and `combatreport_*` are **shared by filename** (content may delta per fog of war); this drop has **one file per name/date**—do not invent a second side’s copy.

Filename tokens `411207` and `411208` appear in every plain-text sample name (spec §5 example filename: `asigint_411207.txt`). YYMMDD interpretation not stated.

## Plain-text turn outputs

| Canonical filename | Bytes | Lines | Side (prefix) | Classification (Ozgur 2026-09-15 15:40 UTC) |
| --- | ---: | ---: | --- | --- |
| `aoperationsreport_411207.txt` | 8710 | 166 | Allied (`a` prefix) | Per-side operational report |
| `aoperationsreport_411208.txt` | 15111 | 288 | Allied (`a` prefix) | Per-side operational report |
| `asigint_411207.txt` | 1140 | 22 | Allied (`a` prefix) | Per-side SIGINT |
| `asigint_411208.txt` | 1261 | 22 | Allied (`a` prefix) | Per-side SIGINT |
| `Combat_Events_411207.txt` | 5833 | 131 | Shared (no a/j prefix) | Shared by filename; fog-of-war deltas possible; one instance in this drop |
| `Combat_Events_411208.txt` | 7749 | 205 | Shared (no a/j prefix) | Shared by filename; fog-of-war deltas possible; one instance in this drop |
| `combatreport_411207.txt` | 30002 | 1022 | Shared (no a/j prefix) | Shared by filename; fog-of-war deltas possible; one instance in this drop |
| `combatreport_411208.txt` | 16020 | 575 | Shared (no a/j prefix) | Shared by filename; fog-of-war deltas possible; one instance in this drop |
| `joperationsreport_411207.txt` | 79 | 3 | Japanese (`j` prefix) | Fog of war: header + Batan Island only; not incomplete |
| `joperationsreport_411208.txt` | 76 | 3 | Japanese (`j` prefix) | Fog of war: header + Batan Island only; not incomplete |
| `jsigint_411207.txt` | 33 | 2 | Japanese (`j` prefix) | Fog of war: header-only; Japanese gets nothing useful this turn; not incomplete |
| `jsigint_411208.txt` | 33 | 2 | Japanese (`j` prefix) | Fog of war: header-only; Japanese gets nothing useful this turn; not incomplete |

**Plain-text count:** 12 files, all non-empty at catalog time.

## Game save (`.pws`)

See **[`docs/pws-knowledge-summary.md`](../pws-knowledge-summary.md)** for campaign roles (Dave = Japanese, Ozgur = Allied), calendar, and product rules.

| File | Role (Ozgur 2026-09-19) | Catalog |
| --- | --- | --- |
| Six `wpae021_*.pws` (ids `2385`, `3c3c`, `251c`, `6b87`, `c446`, `381d`) | Japanese → Allied (incoming to Ozgur) | [`pws-wpae021-catalog.md`](../pws-wpae021-catalog.md) |
| `wpae023.pws` | Allied → Japanese (**Ozgur outgoing**) | [`pws-wpae023-catalog.md`](../pws-wpae023-catalog.md) |

**Store copy (2026-09-18 image):** `wpae023.pws` — 2 726 043 B on disk, MD5 `b85624828c57a1160afccee4a74ca54b`; gzip → 2 872 411 B inner; 61 zlib records; build `1.8.11.28.4`. **No on-disk password** required to decompress golden samples. In-game PBEM password **not validated** from bytes. Details: [`pws-inspection.md`](../pws-inspection.md).

**`.pws` count:** seven files catalogued (six incoming track + one outgoing); binary copies may live under `docs/golden/pws/` when present in store.

**Extension (facts):** WITP-CIC canonical PBEM save extension is **`.pws`** (spec §5; golden `wpae023.pws`; Ozgur correction Changelog 2026-09-15 15:43 UTC). Official manual §20.4.1 / §20.5.4 cite **`.wps`** as the manual’s string — superseded for WITP-CIC file matching only.
