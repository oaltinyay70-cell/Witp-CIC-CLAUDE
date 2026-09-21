# PBEM save (`.pws`) — knowledge summary

One-page index for WITP-CIC. Forensic detail lives in linked catalogs; this file states **facts** and **product implications** only.

**Last updated:** 2026-09-19 (Ozgur clarifications + six-file pass).

---

## Campaign context (user-stated)

| Who | Role |
| --- | --- |
| **Ozgur** | Allied / US PBEM player |
| **Dave** | Japanese PBEM player |

| Files | Direction (provenance) | Notes |
| --- | --- | --- |
| Six golden `wpae021_*.pws` | Japanese → Allied (Ozgur) | Five labels: `(PBEM0) Incoming to Dave`; T1: `PBEM Ozgur Exchange File` |
| `wpae023.pws` | **Allied → Japanese** (Ozgur outgoing) | Label `(PBEM1) … incoming to Ozgur` is **free text**, not a reliable direction field |

**Calendar (six-pack):**

- **T1 (`2385`):** **12/08/41** — exchange only, **no gameplay** (file label typo **12/07/41**).
- **T2–T6:** in-game dates **12/09 – 12/13/41** with full turn text (AAR, combat events, ops, SIGINT).
- **`wpae023`:** same in-game date as T6 (**12/13/41**); host timestamp between T5 and T6 — fits Ozgur playing and sending outgoing, then receiving T6.

Scenario in all samples: *War in the Pacific: Hakko Ichiu*, December 1941 Start, build **`1.8.11.28.4`**.

---

## On-disk format (proven on seven files)

1. Single **gzip** member (`1f 8b 08`, no passphrase in header).
2. Inner payload = **61 zlib records** + one **22-byte** gap (not zlib), **100%** byte coverage.
3. Each record: **12-byte LE header** `(a, b, c)` + zlib at `+12` (`78 da`).
4. **Record type** = `a & 0xff` (same 61 types in every file). **Do not parse by file order** (types after the first 11 are permuted per save).
5. Password **`kilroy43`:** not stored in the file; every record inflates **without** a key. In-game PBEM password may still exist in the **game UI** (manual §20.5.4); on-disk blob is plain gzip+zlib for these samples.

---

## Turn text (record `0x51`)

Four **uint32le length** + CRLF text sections per played turn: AAR, combat events, operational report, SIGINT. Save label date often **one day ahead** of the AAR header date. Full text: [`pws-wpae021-catalog.md`](pws-wpae021-catalog.md).

---

## Tables (corrected row strides)

| Type | Content | Row bytes | Rows |
| ---: | --- | ---: | ---: |
| `0x08` | Ships | **464** | 20 000 |
| `0x1b` | Ship classes | **380** | 5 000 (static T1–T6) |
| `0x15` | Persons | 180 | 50 000 |
| `0x1f` | Equipment | 236 | 2 000 |
| `0x20` | Aircraft types | **396** | 1 000 |
| `0x35` | Operations | **1304** | 2 000 (static T1–T6) |

Old single-file catalog used **half** strides for ships/classes/aircraft/ops — **superseded** by [`pws-field-map.md`](pws-field-map.md).

---

## Ship row — safe to implement in WITP-CIC

Evidence-gated only (464-byte rows, record `0x08`):

| Offset | Meaning |
| ---: | --- |
| 0 | Ship name (NUL-terminated ASCII) |
| 36 | **`19999`** = out of play / sunk |
| 352, 354 | Map **x, y at loss** (matches AAR for verified sinkings) |
| 60, 64, 66 | Task-force group id, next/prev slot (linked lists) |
| 333 | Flag **100** on most sunk rows |

**Not proven:** live hex for active ships, damage/fuel/cargo, side enum in row. Allied-limited **fog** shows as empty TF fields on many IJN rows vs populated Allied rows.

---

## WITP-CIC product rules (from forensics)

1. **Do not** require a password to **decompress** `.pws` on disk for these samples; probe gzip → zlib records instead.
2. **Do not** treat save labels as direction or side; store them as metadata strings only.
3. **`.pws` full telemetry** in spec §5 remains **partial**: implement container read + proven ship tombstones before guessing live positions.
4. **Text reports** in the vault remain the primary War Room source until `.pws` fields are evidence-gated.

---

## Deep dives

| Document | Contents |
| --- | --- |
| [`pws-wpae021-catalog.md`](pws-wpae021-catalog.md) | Six incoming saves, record index, turn text, vs `wpae023` |
| [`pws-wpae023-catalog.md`](pws-wpae023-catalog.md) | Outgoing save byte catalog |
| [`pws-field-map.md`](pws-field-map.md) | Cross-turn diff, field map, unknowns |
| [`pws-inspection.md`](pws-inspection.md) | Unlock tests, `wpae023` summary |
| [`golden/INDEX.md`](golden/INDEX.md) | Golden file inventory |

Scripts and JSON: `internal/pws-inspection/` (project store, not in git repo).
