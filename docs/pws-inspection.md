# Golden PBEM save inspection — `wpae023.pws`

**File:** `docs/golden/wpae023.pws` (store copy)  
**Inspection date:** 2026-09-18  
**Full extract catalog:** [`docs/pws-wpae023-catalog.md`](pws-wpae023-catalog.md)

**Ozgur-stated (not file fields):** **`wpae023.pws` is Ozgur’s outgoing Allied → Japanese send** (not an incoming file). Filename `wpae023.pws`. PBEM partner **Dave** = Japanese player (2026-09-19). Password claimed **`kilroy43`** (not present in file bytes; see unlock section).

**Related set:** six distinct `wpae021_*.pws` saves = Japanese → Allied (Ozgur); catalog [`pws-wpae021-catalog.md`](pws-wpae021-catalog.md). Summary: [`pws-knowledge-summary.md`](pws-knowledge-summary.md).

Upload path `/home/ubuntu/.cursor/projects/workspace/uploads/wpae023_4e88.pws` was **not present** in this environment. This pass hashed the store copy only.

---

## Password unlock (2026-09-18)

Tried **only** methods justified by this file’s magic or by the WITP manual:

| Method | Result |
| --- | --- |
| gzip (magic `1f 8b 08`, flags 0) | Inflates with **no** password. |
| zlib records (CMF/FLG `78 da`) | **61** records inflate with **no** password. |
| PKZIP / ZipCrypto + `kilroy43` | **Fails.** File is not a zip (`BadZipFile` on on-disk, gzip-inner, and the 22-byte gap). |
| Manual §20.4.1 / §20.5.4 | Says saves are compressed and encrypted; password is entered **in the game UI**. No cipher, key schedule, or zip wrapping is documented. |
| Literal `kilroy43` | **Absent** from on-disk, inner, and every inflated record. |

**Unlock not demonstrated.** ZipCrypto is disproved for this file. Gzip/zlib do not use the claimed password. No documented WITP cipher was applied. This does **not** prove or disprove that `kilroy43` is the in-game PBEM password.

---

## This image vs 2026-09-15 pass

The 2026-09-15 write-up described a **different on-disk file**:

| | 2026-09-15 image | This store copy (2026-09-18) |
| --- | --- | --- |
| On-disk bytes | 2 677 122 | **2 726 043** |
| MD5 | `a1d32c3229419cd22713be67e393277d` | **`b85624828c57a1160afccee4a74ca54b`** |
| Inner bytes | 2 824 440 | **2 872 411** |
| PBEM date string | `Date: 12/10/41` | **`Date: 12/13/41`** |
| Host timestamp string | `Mon, September 14 2026  at 08:08:15 AM` | **`Thu, September 17 2026  at 11:34:04 PM  `** |
| Embedded reports | `Dec 09, 41` snippet | Full **Dec 12, 41** AAR / combat events / ops / SIGINT |

Do not mix facts between the two images.

---

## On-disk container

| Property | Value |
| --- | --- |
| Size | 2 726 043 bytes |
| MD5 | `b85624828c57a1160afccee4a74ca54b` |
| SHA-1 | `60635ebfd005a88d87d7dedb518784c4e3828d29` |
| SHA-256 | `aae5d88b92814e3b5fb7becec7faf3619d2d05f12730ca4996e41e697aafbeca` |
| Magic | `1f 8b 08 00 00 00 00 00 00 0b` — gzip DEFLATE, flags 0, mtime 0, OS NTFS (`0b`) |
| gzip members | 1 |
| ISIZE mod 2^32 | 2 872 411 |
| CRC32 trailer | 786172945 (`0x2edc0c11`) |

Manual §20.4.1 uses extension **`.wps`**. WITP-CIC canonical extension is **`.pws`**. Wrapper on disk is **gzip**, not PKZIP.

---

## Inner payload

| Property | Value |
| --- | --- |
| Size | 2 872 411 bytes |
| MD5 | `2eef505d8deb1b69100d3d2632468a9c` |
| SHA-256 | `eec82798c1ef8c022d02c59eaaeac2227cd6338c58e0548e7fcb2263eb709ce5` |
| First 12 bytes | `26 02 00 00 b8 0b 00 00 88 00 00 00` = `(550, 3000, 136)` LE |

**Observed** framing (this sample): 12-byte LE `(a,b,c)` + zlib at `+12`. For `a` in `0x000002xx`, `b` = inflated length and `c` = zlib length. For `a` in `0x000102xx`, `b`/`c` are **not** those sizes; zlib at `+12` still inflates without a password.

**61** zlib records + one **22-byte** opaque gap at inner `1717894` (`21 02 11 00 b9 44 f2 c4 4c 44 0a c2 ca 1f 55 72 1b 2f e1 a6 55 fd`) = **100%** of the inner payload. Gap is not zip/gzip/zlib.

---

## Cleartext that is actually in the file

Record 0 (3000 bytes), complete nonzero content:

| Offset | Stored |
| ---: | --- |
| 0 | `(PBEM1) 12/8 incoming to Ozgur Date: 12/13/41 ` |
| 1000 | `Thu, September 17 2026  at 11:34:04 PM  ` |
| 1800 and 1820 | `1.8.11.28.4` (twice) |
| 1840 and 1844 | `uint32le` **1128** (twice) |
| 2000 | `Email 12/13/41 ` |

Record 1 scenario C-strings: `War in the Pacific: Hakko Ichiu`, `December 1941 Start`, `WITP:AE Team`.

Record 60 is four length-prefixed CRLF texts (complete in the catalog): **AFTER ACTION REPORTS FOR Dec 12, 41**; **COMBAT EVENTS FOR 12/12/41**; **OPERATIONAL REPORT FOR Dec 12, 41**; **SIG INT REPORT FOR Dec 12, 41**.

Name-like ASCII at **corrected** row strides (counts only; full lists in the catalog and [`pws-field-map.md`](pws-field-map.md)): ships **11 282 / 20 000** rows (record type **`0x08`**, **464** bytes/row — not 232); equipment **1 129 / 2 000** (type **`0x1f`**, 236); persons **28 531 / 50 000** (type **`0x15`**, 180); aircraft types (**`0x20`**, 396); ship classes (**`0x1b`**, 380); operations (**`0x35`**, 1304). Align records by **`a & 0xff`**, not file order.

No password-as-stored field. No Japanese-player name in record 0. Side is not stored as the string `Allied` in record 0.

Hex/OOB/unit **status** tables were **not** decoded from the remaining binary records (no format spec in the manual or this file).

---

## Parser implications (facts)

- `.pws` on this sample is gzip-wrapped.
- A working first stage is gzip → 12-byte headers → zlib per record, with `decompressobj` when `c` is not a size. **Observed**, not an official spec.
- Spec §5 “unlocked via user password; complete underlying turn telemetry” is **not** satisfied: ZipCrypto failed; no documented password cipher was run; large blobs stay binary.

---

## Methods

- `gzip` / `zlib.decompressobj`, `ZipFile` ± `kilroy43`, hashes, C-string walk, stride probes, record-60 `uint32le` length prefixes.
- Helpers: `internal/pws-inspection/`.
- No guessed XOR/stream cipher. No network.
