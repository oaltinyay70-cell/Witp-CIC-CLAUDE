# Field map — six `wpae021` saves (evidence-gated)

Cross-turn structure, corrected table strides, and candidate field meanings for the six golden `wpae021_*.pws` files catalogued in [`pws-wpae021-catalog.md`](pws-wpae021-catalog.md). Measurements were taken on **2026-09-18** from inflated zlib payloads; scripts and JSON live under `internal/pws-inspection/`.

**Evidence rule (strict):** A proposed meaning appears below only if it is corroborated by that turn’s own cleartext (AAR / combat events / operational report / SIGINT in record `0x51`) or by a cross-file invariant across all six saves (or all seven files including `wpae023` where noted). Everything else is labelled **unknown**. No cipher or compression guesses.

**Section key:** **EVIDENCE** = backed by the rule above; **UNKNOWN** = layout or diff facts only.

---

## 1. How fixed strides were corrected **EVIDENCE** (method)

The first single-file pass ([`pws-wpae023-catalog.md`](pws-wpae023-catalog.md)) assumed the smallest divisor that made name starts line up at slot offset 0. That confuses **record size** with **sub-multiples** of the same layout.

### 1.1 Name-offset folding (`stride_wpae021.py`)

For each large blob, collect start offsets of NUL-terminated ASCII names (length ≥ 4). For every candidate stride `S` that divides the blob length, fold each name offset mod `S` and score the fraction of names landing on the single best residue. Incidental ASCII inside binary fields spreads across residues; true name columns stay on one residue.

The **record stride** is the **largest** `S` where the best residue still holds nearly all real names (smaller divisors also score well but are harmonics, not separate row sizes).

Output: `internal/pws-inspection/wpae021/strides.json` (T6 `381d` as reference blob).

### 1.2 Dead-space vs live sub-slots (`substride_wpae021.py`)

Name folding alone cannot distinguish row size `S` from a smaller `s = S/k` with unused interleaved slots: names would align either way. For each candidate `(S, s)` with `S = k·s`, split every **named** row into `k` sub-slots of length `s`. If sub-slots after the first are all zero in every named row and never change T1→T6, the file is really `s`-byte rows with sparse indexing. If those sub-slots carry nonzero data and change across turns, the row is **`S` bytes**.

Output: `internal/pws-inspection/wpae021/substride.json`.

### 1.3 Corrected stride table vs earlier wrong figures

| Record type | Role (observed) | Inflated bytes | **Wrong** stride (old catalog) | **Correct** stride | Rows | Name offset in row | Dead-space test |
| --- | --- | ---: | ---: | ---: | ---: | ---: | --- |
| `0x08` | Ships | 9 280 000 | 232 (40 000 slots) | **464** | 20 000 | 0 | 2×232 sub-slots both live; 464 confirmed |
| `0x1b` | Ship classes | 1 900 000 | 190 (10 000) | **380** | 5 000 | 0 | 2×190 both live; static T1–T6 |
| `0x15` | Persons / leaders | 9 000 000 | 180 | **180** | 50 000 | 0 | single stride; unchanged |
| `0x1f` | Equipment / devices | 472 000 | 236 | **236** | 2 000 | 0 | single stride; unchanged |
| `0x20` | Aircraft types | 396 000 | 198 (2 000) | **396** | 1 000 | 0 | 2×198 both live |
| `0x35` | Operations / plans | 2 608 000 | 326 (8 000) | **1304** | 2 000 | 0 | four 326-byte regions all live; static T1–T6 |
| `0x13` | (unnamed table) | 21 096 000 | — | **1172** | 18 000 | **+4** | few names; fold on +4 residue |
| `0x14` | (sparse names) | 20 400 680 | — | **20** | 1 020 034 | **+17** | name fold, not a “class” table |

Persons (`180`), equipment (`236`), and the corrected larger strides all **divide the same inflated lengths** as the old figures multiplied by the same factor (e.g. 40 000×232 = 20 000×464). The old numbers were **half or quarter row sizes**, not alternate interpretations of length.

---

## 2. Cross-turn diff T1→T6 **EVIDENCE**

Source: `internal/pws-inspection/diff_wpae021.py` → `wpae021/diff.json`. Comparison is pairwise T1→T2→…→T6 on records aligned by header byte 0 (`a & 0xff`), not file order.

### 2.1 Whole records

| Class | Count | Record types (`type_hex`) |
| --- | ---: | --- |
| **Byte-identical all six turns** (static reference) | 33 | `0x04`, `0x06`, `0x0a`, `0x0b`, `0x0e`, `0x0f`, `0x19`, `0x1a`, `0x1b`, `0x1d`, `0x22`, `0x23`, `0x24`, `0x25`, `0x28`, `0x29`, `0x2e`, `0x2f`, `0x32`, `0x35`, `0x39`, `0x3c`, `0x3d`, `0x3e`, `0x3f`, `0x40`, `0x41`, `0x42`, `0x43`, `0x47`, `0x49`, `0x4b`, `0x4c` |
| **Changes at least one byte** (turn state) | 28 | `0x01`, `0x02`, `0x03`, `0x08`, `0x0c`, `0x0d`, `0x11`, `0x12`, `0x13`, `0x14`, `0x15`, `0x16`, `0x17`, `0x1e`, `0x1f`, `0x20`, `0x26`, `0x27`, `0x2a`, `0x2d`, `0x37`, `0x3a`, `0x44`, `0x45`, `0x46`, `0x48`, `0x4a`, plus variable `0x51` (turn text) |

Notable static tables among the name-bearing types: **`0x1b` ship classes**, **`0x35` operation titles** — scenario reference, not updated in these six turns.

### 2.2 Within-row offsets that move (fixed stride known)

For strided records, `diff_wpae021.py` folds the T1–T6 change mask modulo the corrected stride (from §1) and lists **offsets inside a slot** where any row changed. Below: offsets with the largest number of affected rows (not an exhaustive field list).

| Type | Stride (corrected) | Slots with any change (T1–T6) | Offsets that move (top by row count) |
| --- | ---: | ---: | --- |
| `0x08` ships | 464 | 11 176 | 36, 54–67, 27–28, 290–334, 348–360, 352–354, … (200 distinct byte offsets) |
| `0x15` persons | 180 | 13 518 | 0–18, 25–32, 34–35, 44–45, 56–61, 66, … |
| `0x16` | 36 | 20 882 | 28–29, 32–36, 45, 48, 60–61, 65, 76, 78, 89, 104, … |
| `0x17` | 304* | 6 977 | 4, 112, 118–119, 138–139, 152, 164–167, 185–187, 192, … |
| `0x13` | 1172 | 16 695 | 48, 112, 536–564, 672–673, 786, 812–813, 984–985, … |
| `0x14` | 20* | 2 471 | 62, 184–185, 204, 220, 236, 256, 304, 357–368, 402, 476, 546–548, … |
| `0x1f` equipment | 236 | 48 | 14–17, 56–57, 78–79, 137, 143–144, 147–148, 152 (`verify_fields.py` S7) |
| `0x20` aircraft | 396 | 123 | 194–224, 233, 376 (`verify_fields.py` S6) |
| `0x1e` | 120* | 1 | sparse counters at a few residues only |
| `0x46` | 1112* | 269 | 4–5, 120, 124, 322, 560–561, 674–680, 878, … |
| `0x45` | 16* | 219 | 0–7 |
| `0x26` PBEM header | 3000 (single block) | n/a | Text slots only: 0–22, 34–35, 1000–1036, 2009–2010 (`verify_fields.py` S9) |

\*Stride for diff folding: auto-picked from a candidate list when residue fold beats random (`diff.json` `stride_pick`). Where that pick disagrees with §1 (e.g. `0x1f` pick 2000 vs true 236), use §1 for row indexing; `verify_fields.py` used the corrected strides for `0x08`, `0x15`, `0x1f`, `0x20`, `0x26`.

### 2.3 Small fixed records

| Type | Size | Static T1–T6? | Moving bytes |
| --- | ---: | --- | --- |
| `0x02`, `0x03`, `0x0c`, `0x44`, `0x4a` | 4 | no | 1–2 bytes (counter-like) |
| `0x37`, `0x48` | 4 | no | subset of turns only |
| `0x2a` | 9 | no | most bytes |
| `0x2d` | 1208 | no | 2 distinct MD5s over six files |
| `0x3a` | 3800 | no | ~780 bytes (host-side metadata candidate) |

---

## 3. Candidate field map **EVIDENCE** (by record)

Confidence: **high** = invariant or exact match to cleartext numbers/names; **medium** = strong structural test with partial cleartext; **low** = diff-only hot spot.

### 3.1 Record `0x26` — PBEM header (3000 bytes, one row) **EVIDENCE**

| Offset | Width | Type | Proposed meaning | Evidence | Conf. |
| ---: | ---: | --- | --- | --- | --- |
| 0 | 1000 | char[] | Save label + in-game date | Cleartext in all six files; only these bytes change per turn | high |
| 1000 | 800 | char[] | Host timestamp string | Cleartext; changes per turn | high |
| 1800 | 20 | char[] | Build string `1.8.11.28.4` | Identical all six turns | high |
| 1820 | 20 | char[] | Duplicate build string | Identical all six turns | high |
| 1840 | 8 | 2×uint32le | Values `1128`, `1128` | Identical all six turns; not ASCII | medium (values only) |
| 2000 | 1000 | char[] | Email line `Email MM/DD/YY` | Cleartext date suffix changes | high |

### 3.2 Record `0x51` — turn text **EVIDENCE**

| Layout | Evidence | Conf. |
| --- | --- | --- |
| Repeated **`uint32le` length** then that many bytes of CRLF text, four sections per turn (AAR, combat, operational, SIGINT) | `turntext_wpae021.py` tiles every byte; section headers quoted in catalog | high |

Meaning of non-text tail padding: **unknown**.

### 3.3 Record `0x08` — ships (464-byte rows, 20 000 slots) **EVIDENCE**

| Offset | Width | Type | Proposed meaning | Evidence | Conf. |
| ---: | ---: | --- | --- | --- | --- |
| 0 | ≤40 | char[] | Ship name | ~99.3% of named rows start ASCII name at 0 (`strides.json`) | high |
| 36 | 2 | uint16le | Out-of-play marker; **`19999`** = removed/sunk | Count rises with turn; newly set rows match AAR sunk names (`ship_hypotheses.json` H1); monotonic | high |
| 60 | 2 | uint16le | Task-force / group id | 73 groups; each group’s live slots form one chain (`verify_fields.py` S4) | medium |
| 64 | 2 | uint16le | Next slot index in group | Doubly linked with @66: 2383/2383 back-links (`ship_hypotheses.json` H3) | medium |
| 66 | 2 | uint16le | Previous slot index in group | Same as @64 | medium |
| 352 | 2 | uint16le | Map **x** at loss | For 8/8 AAR lines with unambiguous ship and “sunk”, equals printed hex (`verify_fields.py` S2) | high |
| 354 | 2 | uint16le | Map **y** at loss | Same | high |
| 333 | 1 | uint8 | Flag on out-of-play rows | Value **100** on almost all `@36==19999` rows; **0** on live (`verify_fields.py` S3) | medium |
| 54–67, 290–334, … | various | — | Movement / damage / cargo state | Move when AAR reports hits but not sunk (`verify_fields.py` S5); dominate diff mask | **unknown** (hot) |
| (live position) | — | — | Current map hex for active ships | `probe_hex.py` / `probe_hex2.py`: no offset pair matches live AAR anchors across ships; @352/@354 mostly zero on live T6 ships | **unknown** |

Tombstone behaviour: after `@36` becomes 19999, row mostly freezes (**90** slot-turns unchanged vs **8** changed post-mark; `ship_hypotheses.json` H5).

Japanese vs Allied fill asymmetry (empty @60 chain on many IJN names, populated on Allied) is noted in the catalog as **observed fog-of-war pattern**, not a decoded side field.

### 3.4 Record `0x15` — persons (180-byte rows) **EVIDENCE**

| Offset | Width | Type | Proposed meaning | Evidence | Conf. |
| ---: | ---: | --- | --- | --- | --- |
| 0 | ≤40 | char[] | `Last, First` name | Name fold at 0 (`strides.json`) | high |
| 0–66, … | — | — | Per-person game state | 13 518 / 50 000 rows change; 46 byte offsets (`verify_fields.py` S8) | **unknown** |

### 3.5 Record `0x1f` — equipment (236-byte rows) **EVIDENCE**

| Offset | Width | Type | Proposed meaning | Evidence | Conf. |
| ---: | ---: | --- | --- | --- | --- |
| 0 | ≤40 | char[] | Device / unit name | 1152 named rows at 0; static names | high |
| 14–17, 56–57, 78–79, 137, 143–148, 152 | mixed | — | Turn-varying counters or stats | 48 rows change T1–T6; engine/squad/gun rows listed in S7 | **unknown** |

No join from AAR numbers to these offsets was established (`probe_aircraft*.py` targets `0x20`, not `0x1f`).

### 3.6 Record `0x20` — aircraft types (396-byte rows) **EVIDENCE**

| Offset | Width | Type | Proposed meaning | Evidence | Conf. |
| ---: | ---: | --- | --- | --- | --- |
| 0 | ≤40 | char[] | Aircraft type name | 555 names at 0 (`strides.json`) | high |
| 194–224, 233, 376 | mixed | — | Per-type pool / loss counters | 123 rows move; offsets cluster in second 198-byte half of row (S6) | **unknown** |

`aircraft_probe.json`: rows that move overlap types mentioned in turn text more than random control, but **no** uint32 offset was found whose delta equals AAR destroyed/damaged counts (`probe_aircraft2.py`).

### 3.7 Static name tables (reference) **EVIDENCE**

| Type | Stride | Meaning | Evidence |
| --- | ---: | --- | --- |
| `0x1b` | 380 | Ship class name at 0 | Identical all six turns; 1705 names |
| `0x35` | 1304 | Operation / plan title at 0 | Identical all six turns; 1308 names |

Other bytes in these rows: nonzero but **never change** in this campaign slice → treat as scenario data, fields **unknown**.

### 3.8 Records with cleartext strings but mostly unknown binary **EVIDENCE / UNKNOWN**

| Type | Cleartext | Binary |
| --- | --- | --- |
| `0x01` | Scenario title, start, author strings | ~98% NUL; small per-turn noise |
| `0x0d`, `0x3c` | Some ASCII fragments | Same length T1–T6; `0x0d` changes, `0x3c` static |

Large mostly binary blobs (`0x17`, `0x16`, `0x13`, `0x14`, `0x46`, `0x45`, `0x27`, `0x43`, …): diff profiles in §2.2 only — **unknown** without external anchors.

### 3.9 Inner container (not a zlib record) **EVIDENCE**

| Item | Evidence | Conf. |
| --- | --- | --- |
| 12-byte header `(a,b,c)` per zlib record | 61 types; byte0 = type, byte2=1 ⇒ `b`,`c` not sizes | high |
| 22-byte `0x21` block | Not zlib; 16-byte payload determined by header byte1 | high |
| Payload of `0x21` | **unknown** (not turn state; identical across files sharing byte1) | — |

---

## 4. What remains undecoded **UNKNOWN**

| Gap | Why it is still open | Data that would help |
| --- | --- | --- |
| Live ship / TF map coordinates | No stable `(x,y)` pair in the 464-byte row for live Allied anchors; only loss hex at @352/@354 is verified | Saves after known moves with many “located at (x,y)” SIGINT lines; side-by-side diff on one ship |
| Damage, fuel, cargo, speed | Hot offsets (@54–67, @290+) correlate with combat but no numeric join | AAR lines with exact damage % or fuel states tied to one ship name |
| Aircraft / equipment pools | Moving fields identified; no counter matches AAR destroyed/damaged | Same type on consecutive turns with simple loss totals in text |
| `0x16`, `0x17`, `0x13`, `0x14`, `0x46` | Large; partial stride guesses from diff only | External ID lists (bases, units, LCUs) or in-game editor dumps |
| `0x3a`, counters `0x02`/`0x03`/`0x0c` | Small changes | Compare two saves that differ only in turn number / host clock |
| `0x21` 22-byte block | Constant 16-byte core per byte1 | Matrix spec or loader disassembly (out of scope here) |
| Encryption / password | All 366 records inflate without a key | N/A for these files — format is plain gzip+zlib |

---

## 5. Scripts and artifacts

| Script | Output |
| --- | --- |
| `stride_wpae021.py` | `wpae021/strides.json` |
| `substride_wpae021.py` | `wpae021/substride.json` |
| `diff_wpae021.py` | `wpae021/diff.json` |
| `verify_fields.py` | `wpae021/field_verification.json` (+ stdout tests S1–S9) |
| `probe_ships2.py`, `probe_hex*.py`, `parse_aar.py` | `ship_hypotheses.json`, `hex_probe.json`, `aar_engagements.json` |
| `turntext_wpae021.py` | `wpae021/turntext/*.txt` |

Re-run from `internal/pws-inspection/` after `extract_wpae021.py` populates `/tmp/pws021/`.

---

## 6. Summary: evidence vs unknown

| Area | Status |
| --- | --- |
| Container (gzip, zlib, 61 record types, alignment by `a&0xff`) | **EVIDENCE** |
| Corrected strides (§1.3) | **EVIDENCE** |
| Static vs changing record sets (§2.1) | **EVIDENCE** |
| PBEM header strings (`0x26`), turn text framing (`0x51`) | **EVIDENCE** |
| Ship name, out-of-play sentinel, loss hex, group linked list | **EVIDENCE** (meanings as stated) |
| Ship movement / live hex / most damage bytes | **UNKNOWN** |
| Person, LCU, air wing, equipment field semantics | **UNKNOWN** (names + diff only) |
| `0x21` block, binary bulk records | **UNKNOWN** |
