# Catalog — the six `wpae021.pws` saves

Forensic catalog of six distinct saves Ozgur uploaded, all originally named `wpae021.pws`. Everything below is measured from the bytes. Strings are quoted exactly as stored, including trailing spaces. Integers are little-endian. Structure that is only observed on these samples is labelled **observed**.

Cross-turn diff findings and the candidate field map live in [`pws-field-map.md`](pws-field-map.md). Unlock status is in [`pws-inspection.md`](pws-inspection.md).

> **`wpae023.pws` is the outgoing / wrong-direction file.** Ozgur states it is his own US → Japanese send. It is *not* one of these six. The earlier catalog [`pws-wpae023-catalog.md`](pws-wpae023-catalog.md) describes that file. A direct comparison against the newest of these six is at the end of this document.

Inspection date: 2026-09-18. Source directory: `docs/golden/pws/`.

## Ozgur-stated facts vs what the files say

| Claim | In-file corroboration |
| --- | --- |
| Six distinct saves, all named `wpae021.pws` | Confirmed: six distinct sizes, MD5s, and in-game dates. |
| Six files = Japanese → Allied (Ozgur) | **User-stated provenance.** No side enum in the file. **Dave** is the Japanese PBEM player’s name (Ozgur, 2026-09-19). Five labels read `(PBEM0) Incoming to Dave`; T1 reads `PBEM Ozgur Exchange File`. Labels are free text — they name a person, not a stored direction field. Ship-table fill (Allied rows with task-force links, many IJN rows empty) matches an **Allied-limited** view, which fits Ozgur as recipient. |
| Calendar | T1’s label says **12/07/41**; Ozgur states the real day is **12/08/41** — **exchange only, no one plays** that day. Played in-game days in this set: **12/09–12/13** (five turns with AAR text). There is no separate “missing 12/08” file; T1 is the 12/08 exchange snapshot. |
| Password `kilroy43` | **Not present** as a literal in any encoding, in any of the six files, on disk, in the gzip payload, or in any of the 366 inflated records. See the password section. |

One piece of *indirect* evidence does support the stated direction: the ship table is populated asymmetrically. Japanese ships carry empty group ids and empty movement state while Allied ships carry both, which is what a fog-of-war-limited file handed to the Allied player looks like. That is an inference from fill patterns, not a stored field.

## The six files, in chronological order

Ordered by the in-game date parsed from the save label inside each file. The short id is the hex suffix of the store filename and is used throughout both documents.

| Turn | Short id | In-game date | Save label as stored | Host timestamp as stored |
| ---: | --- | --- | --- | --- |
| T1 (exchange) | `2385` | **12/8/41** (label typo **12/07/41**) | `PBEM Ozgur Exchange File Date: 12/07/41 ` | `Tue, September 01 2026  at 07:30:22 AM  ` |
| T2 | `3c3c` | 12/9/41 | `(PBEM0) Incoming to Dave Date: 12/09/41 ` | `Mon, September 14 2026  at 01:11:15 AM  ` |
| T3 | `251c` | 12/10/41 | `(PBEM0) Incoming to Dave Date: 12/10/41 ` | `Tue, September 15 2026  at 01:04:40 AM  ` |
| T4 | `6b87` | 12/11/41 | `(PBEM0) Incoming to Dave Date: 12/11/41 ` | `Thu, September 17 2026  at 12:01:16 AM  ` |
| T5 | `c446` | 12/12/41 | `(PBEM0) Incoming to Dave Date: 12/12/41 ` | `Thu, September 17 2026  at 12:11:35 PM  ` |
| T6 | `381d` | 12/13/41 | `(PBEM0) Incoming to Dave Date: 12/13/41 ` | `Fri, September 18 2026  at 11:43:22 AM  ` |

The host timestamps run forward with the in-game dates, which independently confirms the ordering: Sep 01 → Sep 14 → Sep 15 → Sep 17 00:01 → Sep 17 12:11 → Sep 18. Two turns were exchanged on Sep 17.

T1 is different in kind from the rest. Its label is `PBEM Ozgur Exchange File` rather than `(PBEM0) Incoming to Dave`, it was saved two weeks before the others, it is the smallest file, and its turn-text record is 32 bytes of empty sections with no after-action report. It reads as a start-of-campaign exchange file rather than a played turn.

## File identity

| Turn | id | On-disk bytes | MD5 | SHA-256 |
| ---: | --- | ---: | --- | --- |
| T1 | `2385` | 2326622 | `2f42745aa2da943dc1754525df8b4d0c` | `73b116562d3bf511d5fa5dd26a6f142aa1e51a187864cf26500a7106d9f7a971` |
| T2 | `3c3c` | 2657552 | `40039800cccb90385401f1e8cc23dc3b` | `2daf0d714dc2789c7fb841dfb0fd106d07790f001dfbb00a62a1cb2573c213ee` |
| T3 | `251c` | 2690375 | `e38b5f8387820dbea3358035b9bbc08d` | `86d0184690f87e2628f22ce9c4f49748f016b239bd89bf51c04cb8d242080f22` |
| T4 | `6b87` | 2707722 | `604ff6160299e4aa2528591ad5abdf63` | `19d49c16a87eacc647c9dbf2ec40e1399e2067c033e61c13c0522e2fda04ed9f` |
| T5 | `c446` | 2716077 | `28868d674ca376a7e215d769254a8823` | `8779c718ba1940883869087bb60d99a7db13f32748311977553a8666bb3f785f` |
| T6 | `381d` | 2730921 | `3bce27452dd5bda802f48f08291a1ab3` | `3a81b5f3f6906d621c55b54fe3b7ff4825ac268716d549065f21689bd1204540` |

| Turn | id | SHA-1 | gzip CRC32 | gzip ISIZE | Inner bytes | Inner MD5 |
| ---: | --- | --- | ---: | ---: | ---: | --- |
| T1 | `2385` | `0d2acb6dd20ae3a4dc14ba79734b5aecacd76bb7` | 3051910082 | 2484539 | 2484539 | `72c9bf46b0359957fe9e0229fe1fecae` |
| T2 | `3c3c` | `fdd9ea9def363f6d5f06b6814ccfae18a07f309c` | 688559916 | 2805991 | 2805991 | `57c6474e4683c73188a9a8db2f919dca` |
| T3 | `251c` | `d7afd8497060685e1168bdb95dbb8b68e6b03a72` | 4083464852 | 2838162 | 2838162 | `bae7352d020a88d0f3bde600d58322ea` |
| T4 | `6b87` | `ef5d4cd3dc5e30cf5f1f79762fcb69c1e1943c91` | 983726841 | 2855543 | 2855543 | `c91fa13deb5ddc38d30d5617629a768e` |
| T5 | `c446` | `959e601dba7262c6eed8c7464f385aa8c38c4be7` | 2233242646 | 2863155 | 2863155 | `8295526744b592bb51e60c38fdea4a15` |
| T6 | `381d` | `e0872315d5669ab2d76ab877b48b770bd57f1fcf` | 1743205426 | 2877780 | 2877780 | `d5087cedfee2b19629e8196917563d5e` |

All six share the same gzip header shape: ID `1f 8b`, method DEFLATE `08`, flags `00`, mtime `0`, xfl `00`, OS `0b` (NTFS). One gzip member each, no trailing bytes. The gzip ISIZE and CRC32 trailers verify against the inflated payload in every file.

Every file's inner payload decomposes with no bytes left over: 61 zlib records plus one 22-byte block that is not zlib. Coverage is 100% for all six.

## Inner framing (**observed**)

Walking the gzip payload from offset 0: a 12-byte header of three little-endian `uint32` values `(a, b, c)`, then a zlib stream at `+12` (CMF/FLG `78 da`), then the next header immediately after that stream.

The six-file set resolves what `a` is. Split it into bytes:

| Byte of `a` | Content | Evidence |
| --- | --- | --- |
| byte 0 | **record type id**. 61 values, unique within a file, and the *same 61 values* in all six files (and in `wpae023`). | Set equality across 7 files. |
| byte 1 | a **per-file constant**. Every one of the 61 headers in a file carries the same value. Observed values 0, 1, 2. | Per-file histograms are single-valued. |
| byte 2 | 0 for 53 record types, 1 for 8 types. When it is 1, `b` and `c` are *not* the inflated and compressed sizes. When it is 0, they are, exactly. | Same split in all six files. |
| byte 3 | always 0. | All 366 headers. |

Per-file byte 1 values: T1 `2385` = 2, T2 `3c3c` = 1, T3 `251c` = 1, T4 `6b87` = 0, T5 `c446` = 1, T6 `381d` = 2. `wpae023` = 2.

### Record order is shuffled per save

This is the reason the earlier single-file pass could describe records by index. With six files the index turns out not to be stable:

- the **first 11 records are always** types 38, 1, 2, 3, 4, 6, 8, 10, 11, 12, 13, in that order, in all six files;
- the **remaining 50 are permuted** differently in every file;
- two files that share the byte-1 constant still have different orders, so the order is not derived from it.

Any parser must key on `a & 0xff`, never on position. Record indices quoted in the `wpae023` catalog are valid for that file only.

### The 22-byte block

Every file has exactly one 22-byte run that is not a 12+zlib record. Its offset differs per file. Its layout is `21 <byte1> 11 00` followed by 18 bytes:

| Turn | id | byte 1 | Offset in gzip payload | Full 22 bytes |
| ---: | --- | ---: | ---: | --- |
| T1 | `2385` | 2 | 1654277 | `21 02 11 00 b9 44 f2 c4 4c 44 0a c2 ca 1f 55 72 1b 2f e1 a6 43 1f` |
| T2 | `3c3c` | 1 | 322063 | `21 01 11 00 f1 c4 0d 7a c7 0c 1f dc 9a 40 62 86 99 66 98 2b 83 d5` |
| T3 | `251c` | 1 | 774044 | `21 01 11 00 f1 c4 0d 7a c7 0c 1f dc 9a 40 62 86 99 66 98 2b 81 a8` |
| T4 | `6b87` | 0 | 1821146 | `21 00 11 00 34 c5 d7 48 5e 0d 5f 03 0a 70 ad d9 c0 55 dd 1b 79 79` |
| T5 | `c446` | 1 | 623155 | `21 01 11 00 f1 c4 0d 7a c7 0c 1f dc 9a 40 62 86 99 66 98 2b 83 d5` |
| T6 | `381d` | 2 | 674896 | `21 02 11 00 b9 44 f2 c4 4c 44 0a c2 ca 1f 55 72 1b 2f e1 a6 b5 cf` |

The first four bytes are a header word in the same shape as the record headers (type `0x21`, the file's byte-1 constant, then `0x11` where records carry 0 or 1). The **16 bytes that follow are a pure function of that byte-1 constant**: the three files with byte 1 = 1 have byte-identical 16-byte blocks, as do the two with byte 1 = 2, and `wpae023` (also byte 1 = 2) matches those two exactly. The final 2 bytes vary and are not unique per file. The block is not gzip, not zlib, and too short to be a zip local header. It is left opaque; no cipher was guessed at it.

## Password / unlock, retested on all six files

Only falsifiable tests were run. Each has a definite answer and each answer is reported whether or not it helps.

| Test | Result |
| --- | --- |
| gzip decompress, no password | **Succeeds** on all six. gzip flags are 0; there is no passphrase in this container. |
| zlib inflate of every record, no password | **Succeeds** for all 61 records in all six files, 366 records total. Adler-32 trailers verify. |
| PKZIP / ZipCrypto with `kilroy43` | **Not applicable.** No `PK` local-file header anywhere: not on disk, not in the gzip payload, not in the 22-byte block. `ZipFile` raises `BadZipFile` with and without the password. |
| Literal search for the password | **0 hits.** 34 encodings and paddings (UTF-8, UTF-16LE, Latin-1, upper/capitalised, NUL- and space-padded to 9/10/12/16/20/21/32 bytes), searched case-sensitively and case-insensitively across on-disk bytes, gzip payload, and all 366 inflated records. |
| 16-byte block == a standard digest of the password | **No match.** 348 candidate digests (MD5, MD4, SHA-1[:16], SHA-256[:16], each salted before/after with the byte-1 constant, plus HMAC-MD5 keyed on it) against the 3 distinct blocks. |
| Trailing 2 bytes == a checksum of the password | **No match.** 116 candidates (CRC-32 low half, Adler-32 low half, CRC-CCITT, byte sum) against the observed tails, big- and little-endian. |

**The decisive point is that no password is needed.** All 61 records in all six files inflate cleanly with no key. There is no ciphertext left over except the 22-byte block, whose 16 payload bytes are identical between files with different content, so it cannot be an encryption of the turn state.

This neither proves nor disproves that `kilroy43` is the in-game PBEM password. The WITP manual (§20.4.1, §20.5.4) says saves are "compressed and encrypted" and that PBEM load prompts for a password in the game UI, but documents no cipher, key schedule, or container. What these files show is that the on-disk format is plain gzip over plain zlib, so a password gate — if there is one — is enforced by the game, not by the file.

## Record index, aligned across the six turns

One row per record type, keyed by `a & 0xff`. `b?`/`c?` say whether the header's `b` equals the inflated length and `c` equals the zlib bytes consumed. Inflated lengths are identical across all six turns for 60 of the 61 types; only the turn-text record varies.

| Type | hex | byte 2 | Inflated bytes | b? | c? | Distinct MD5s over 6 turns | Changes between turns? |
| ---: | --- | ---: | ---: | --- | --- | ---: | --- |
| 1 | `0x01` | 1 | 99696 | N | N | 6 | yes |
| 2 | `0x02` | 0 | 4 | Y | Y | 5 | yes |
| 3 | `0x03` | 0 | 4 | Y | Y | 6 | yes |
| 4 | `0x04` | 0 | 4 | Y | Y | 1 | no |
| 6 | `0x06` | 0 | 4 | Y | Y | 1 | no |
| 8 | `0x08` | 1 | 9280000 | N | N | 6 | yes |
| 10 | `0x0a` | 0 | 4 | Y | Y | 1 | no |
| 11 | `0x0b` | 0 | 4 | Y | Y | 1 | no |
| 12 | `0x0c` | 0 | 4 | Y | Y | 6 | yes |
| 13 | `0x0d` | 0 | 95120 | Y | Y | 6 | yes |
| 14 | `0x0e` | 0 | 16 | Y | Y | 1 | no |
| 15 | `0x0f` | 0 | 4 | Y | Y | 1 | no |
| 17 | `0x11` | 0 | 180000 | Y | Y | 6 | yes |
| 18 | `0x12` | 0 | 360000 | Y | Y | 6 | yes |
| 19 | `0x13` | 1 | 21096000 | N | N | 6 | yes |
| 20 | `0x14` | 1 | 20400680 | N | N | 6 | yes |
| 21 | `0x15` | 1 | 9000000 | N | N | 6 | yes |
| 22 | `0x16` | 0 | 12600000 | Y | Y | 6 | yes |
| 23 | `0x17` | 0 | 21280000 | Y | Y | 6 | yes |
| 25 | `0x19` | 0 | 2000 | Y | Y | 1 | no |
| 26 | `0x1a` | 0 | 2000 | Y | Y | 1 | no |
| 27 | `0x1b` | 0 | 1900000 | Y | Y | 1 | no |
| 29 | `0x1d` | 0 | 2480 | Y | Y | 1 | no |
| 30 | `0x1e` | 0 | 120000 | Y | Y | 6 | yes |
| 31 | `0x1f` | 1 | 472000 | N | N | 6 | yes |
| 32 | `0x20` | 1 | 396000 | N | N | 6 | yes |
| 34 | `0x22` | 0 | 4 | Y | Y | 1 | no |
| 35 | `0x23` | 0 | 4 | Y | Y | 1 | no |
| 36 | `0x24` | 0 | 4 | Y | Y | 1 | no |
| 37 | `0x25` | 0 | 4 | Y | Y | 1 | no |
| 38 | `0x26` | 0 | 3000 | Y | Y | 6 | yes |
| 39 | `0x27` | 0 | 190240 | Y | Y | 6 | yes |
| 40 | `0x28` | 0 | 4 | Y | Y | 1 | no |
| 41 | `0x29` | 0 | 2 | Y | Y | 1 | no |
| 42 | `0x2a` | 0 | 9 | Y | Y | 6 | yes |
| 45 | `0x2d` | 0 | 1208 | Y | Y | 2 | yes |
| 46 | `0x2e` | 0 | 36 | Y | Y | 1 | no |
| 47 | `0x2f` | 0 | 28 | Y | Y | 1 | no |
| 50 | `0x32` | 0 | 1 | Y | Y | 1 | no |
| 53 | `0x35` | 0 | 2608000 | Y | Y | 1 | no |
| 55 | `0x37` | 0 | 4 | Y | Y | 2 | yes |
| 57 | `0x39` | 0 | 4 | Y | Y | 1 | no |
| 58 | `0x3a` | 0 | 3800 | Y | Y | 6 | yes |
| 60 | `0x3c` | 0 | 95120 | Y | Y | 1 | no |
| 61 | `0x3d` | 1 | 2 | N | N | 1 | no |
| 62 | `0x3e` | 0 | 450 | Y | Y | 1 | no |
| 63 | `0x3f` | 0 | 76 | Y | Y | 1 | no |
| 64 | `0x40` | 0 | 4 | Y | Y | 1 | no |
| 65 | `0x41` | 0 | 4 | Y | Y | 1 | no |
| 66 | `0x42` | 0 | 4 | Y | Y | 1 | no |
| 67 | `0x43` | 0 | 127872 | Y | Y | 1 | no |
| 68 | `0x44` | 0 | 4 | Y | Y | 6 | yes |
| 69 | `0x45` | 0 | 760960 | Y | Y | 6 | yes |
| 70 | `0x46` | 0 | 2224000 | Y | Y | 6 | yes |
| 71 | `0x47` | 0 | 4 | Y | Y | 1 | no |
| 72 | `0x48` | 0 | 4 | Y | Y | 3 | yes |
| 73 | `0x49` | 0 | 4 | Y | Y | 1 | no |
| 74 | `0x4a` | 0 | 4 | Y | Y | 6 | yes |
| 75 | `0x4b` | 0 | 4 | Y | Y | 1 | no |
| 76 | `0x4c` | 0 | 4 | Y | Y | 1 | no |
| 81 | `0x51` | 0 | 32–58673 (varies) | Y | Y | 6 | yes |

**33 of 61 record types are byte-identical in all six saves.** Those are scenario reference data that the turn cycle never touches. The remaining 28 carry the turn-to-turn state.

## Cleartext, per file

### Record type `0x26` — the PBEM header block (3000 bytes)

Same slot layout in all six files: 1000 / 800 / 20 / 20 / 160 / 1000 bytes, NUL-padded. Every non-NUL byte in the record is accounted for below.

| Turn | id | @0 save label | @1000 host timestamp | @1800 build | @1820 build | @2000 email line |
| ---: | --- | --- | --- | --- | --- | --- |
| T1 | `2385` | `PBEM Ozgur Exchange File Date: 12/07/41 ` | `Tue, September 01 2026  at 07:30:22 AM  ` | `1.8.11.28.4` | `1.8.11.28.4` | `Email 12/07/41 ` |
| T2 | `3c3c` | `(PBEM0) Incoming to Dave Date: 12/09/41 ` | `Mon, September 14 2026  at 01:11:15 AM  ` | `1.8.11.28.4` | `1.8.11.28.4` | `Email 12/09/41 ` |
| T3 | `251c` | `(PBEM0) Incoming to Dave Date: 12/10/41 ` | `Tue, September 15 2026  at 01:04:40 AM  ` | `1.8.11.28.4` | `1.8.11.28.4` | `Email 12/10/41 ` |
| T4 | `6b87` | `(PBEM0) Incoming to Dave Date: 12/11/41 ` | `Thu, September 17 2026  at 12:01:16 AM  ` | `1.8.11.28.4` | `1.8.11.28.4` | `Email 12/11/41 ` |
| T5 | `c446` | `(PBEM0) Incoming to Dave Date: 12/12/41 ` | `Thu, September 17 2026  at 12:11:35 PM  ` | `1.8.11.28.4` | `1.8.11.28.4` | `Email 12/12/41 ` |
| T6 | `381d` | `(PBEM0) Incoming to Dave Date: 12/13/41 ` | `Fri, September 18 2026  at 11:43:22 AM  ` | `1.8.11.28.4` | `1.8.11.28.4` | `Email 12/13/41 ` |

The 160-byte slot at 1840 is not a string. Its only non-zero content, in every file, is two identical `uint32le` values:

| Turn | id | @1840 | @1844 |
| ---: | --- | ---: | ---: |
| T1 | `2385` | 1128 | 1128 |
| T2 | `3c3c` | 1128 | 1128 |
| T3 | `251c` | 1128 | 1128 |
| T4 | `6b87` | 1128 | 1128 |
| T5 | `c446` | 1128 | 1128 |
| T6 | `381d` | 1128 | 1128 |

Build string `1.8.11.28.4` is stored twice in every file, in the two 20-byte slots. It never changes across the six turns, so all six were written by the same game build. The `1128` pair also never changes.

Diffing this record across turns, the only bytes that ever move are inside the three text slots — the label at 0–35, the timestamp at 1000–1036, and the email date at 2009–2010. Nothing else in the 3000 bytes is turn-dependent.

### Record type `0x01` — scenario strings

| Turn | id | Stored C-strings |
| ---: | --- | --- |
| T1 | `2385` | `War in the Pacific: Hakko Ichiu`, `December 1941 Start`, `WITP:AE Team`, ` "((`, `!"'+` |
| T2 | `3c3c` | `War in the Pacific: Hakko Ichiu`, `December 1941 Start`, `WITP:AE Team`, `m~=`, ` "((`, `!"'+` |
| T3 | `251c` | `War in the Pacific: Hakko Ichiu`, `December 1941 Start`, `WITP:AE Team`, `BXt`, ` "((`, `!"'+` |
| T4 | `6b87` | `War in the Pacific: Hakko Ichiu`, `December 1941 Start`, `WITP:AE Team`, ` "((`, `!"'+` |
| T5 | `c446` | `War in the Pacific: Hakko Ichiu`, `December 1941 Start`, `WITP:AE Team`, `M7Z`, ` "((`, `!"'+` |
| T6 | `381d` | `War in the Pacific: Hakko Ichiu`, `December 1941 Start`, `WITP:AE Team`, `6\|'\|`, ` "((`, `!"'+` |

The scenario is `War in the Pacific: Hakko Ichiu`, start `December 1941 Start`, author `WITP:AE Team`, in all six. The short runs such as `` ` "((` `` and `` `!"'+` `` are incidental ASCII inside binary fields, not names; they are listed only because the C-string walk finds them. The rest of the record is binary (NUL ratio ≈ 0.98).

### Record type `0x51` — the length-prefixed turn text

**Observed** layout: repeating `uint32le length` followed by that many bytes of CRLF text, then NUL padding. This is the record that carries the after-action report, combat events, operational report and SIG INT report.

| Turn | id | Save date | Record bytes | Sections (first line as stored) |
| ---: | --- | --- | ---: | --- |
| T1 | `2385` | 12/7/41 | 32 | *(four zero-length sections; no report text)* |
| T2 | `3c3c` | 12/9/41 | 32787 | `AFTER ACTION REPORTS FOR Dec 08, 41`, `COMBAT EVENTS FOR 12/09/41`, `OPERATIONAL REPORT FOR Dec 08, 41`, `SIG INT REPORT FOR Dec 08, 41` |
| T3 | `251c` | 12/10/41 | 47477 | `AFTER ACTION REPORTS FOR Dec 09, 41`, `COMBAT EVENTS FOR 12/08/41`, `OPERATIONAL REPORT FOR Dec 08, 41`, `SIG INT REPORT FOR Dec 08, 41` |
| T4 | `6b87` | 12/11/41 | 58673 | `AFTER ACTION REPORTS FOR Dec 10, 41`, `COMBAT EVENTS FOR 12/10/41`, `OPERATIONAL REPORT FOR Dec 10, 41`, `SIG INT REPORT FOR Dec 10, 41` |
| T5 | `c446` | 12/12/41 | 48832 | `AFTER ACTION REPORTS FOR Dec 11, 41`, `COMBAT EVENTS FOR 12/11/41`, `OPERATIONAL REPORT FOR Dec 11, 41`, `SIG INT REPORT FOR Dec 11, 41` |
| T6 | `381d` | 12/13/41 | 57827 | `AFTER ACTION REPORTS FOR Dec 12, 41`, `COMBAT EVENTS FOR 12/12/41`, `OPERATIONAL REPORT FOR Dec 12, 41`, `SIG INT REPORT FOR Dec 12, 41` |

Two things are worth noting. First, **the reports lag the save date by one day** — the save dated 12/13/41 carries the after-action report for Dec 12, 41. Second, **T3's combat-events, operational and SIG INT sections are stale**: the save dated 12/10/41 carries an after-action report for Dec 09 but combat events for 12/08 and an operational report for Dec 08. T2 is the mirror case, with combat events for 12/09 next to a Dec 08 report. Both sections are reproduced verbatim below, so the mismatch can be checked rather than taken on trust.

#### T1 `2385` — save date 12/7/41

The record is 32 bytes: four length-prefixed sections of 2 bytes each, all zero, then NUL padding. **No report text exists in this save.**

#### T2 `3c3c` — save date 12/9/41

Complete text of the record, all 4 sections, exactly as stored (section banners added by the extractor):

```text
===== section @0 len=16020 =====
AFTER ACTION REPORTS FOR Dec 08, 41
--------------------------------------------------------------------------------
Invasion Support action off Kota Bharu (51,75)
 
18 Coastal gun shots fired in defense.
 
Japanese Ships
      CL Sendai
      DD Ayanami
      DD Uranami
      PB Tatsumiya Maru
      SC Ch 9
      SC Ch 7
      AK Sasako Maru
      DD Shikinami
 
Allied ground losses:
      174 casualties reported
         Squads: 1 destroyed, 16 disabled
         Non Combat: 0 destroyed, 3 disabled
         Engineers: 0 destroyed, 4 disabled
 
DD Uranami firing at 8th Indian Brigade
PB Tatsumiya Maru fired at enemy troops
SC Ch 9 fired at enemy troops
SC Ch 7 fired at enemy troops
DD Ayanami fired at enemy troops
Defensive Guns fire at approaching troops in landing craft at 1,000 yards
 
 
--------------------------------------------------------------------------------
TF 442 encounters mine field at Bataan (78,77)
 
Allied Ships
      DD Pope, Mine hits 1,  on fire,  heavy damage
      DD Peary
      DD John D. Ford, Mine hits 1,  on fire,  heavy damage
      DD Pillsbury, Mine hits 1,  heavy damage
 
4 mines cleared
 
 
--------------------------------------------------------------------------------
ASW attack near Lubang  at 78,78
 
Japanese Ships
      SS I-124
 
Allied Ships
      PT-35
      AM Finch
      AVD Childs
      xAP Neptuna
      xAK Si Kiang
      PG Isabel
      AM Whippoorwill
      AM Lark
      AM Quail
 
SS I-124 is sighted by escort
PG Isabel fails to find sub, continues to search...
AM Whippoorwill fails to find sub, continues to search...
AM Lark fails to find sub and abandons search
AM Quail fails to find sub and abandons search
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
Submarine attack near Lubang  at 78,78
 
Japanese Ships
      SS I-124
 
Allied Ships
      xAKL Don Jose
 
SS I-124 is sighted by xAKL Don Jose
Captain of SS I-124 elects not to launch torpedoes at this target
 
 
--------------------------------------------------------------------------------
Night Air attack on Clark Field , at 79,76 
 
Weather in hex: Light rain
 
Raid detected at 51 NM, estimated altitude 11,000 feet.
Estimated time to target is 16 minutes
 
Japanese aircraft
      Ki-48-Ib Lily x 15
 
Japanese aircraft losses
      Ki-48-Ib Lily: 1 damaged
 
Aircraft Attacking:
      15 x Ki-48-Ib Lily bombing from 9000 feet * 
               Airfield Attack:  2 x 100 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kota Bharu (51,75)
Defensive Guns engage approaching landing force
 
8 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Tatsumiya Maru
      SC Ch 9
      SC Ch 7
      AK Sasako Maru
 
PB Tatsumiya Maru fired at enemy troops
SC Ch 9 fired at enemy troops
SC Ch 7 fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kota Bharu (51,75)
Defensive Guns engage approaching landing force
 
4 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Tatsumiya Maru
      SC Ch 9
      SC Ch 7
      AK Sasako Maru
 
PB Tatsumiya Maru fired at enemy troops
SC Ch 9 fired at enemy troops
SC Ch 7 fired at enemy troops
 
 
--------------------------------------------------------------------------------
Sub attack near Djemadja  at 52,83
 
Japanese Ships
      SS I-162
 
Allied Ships
      HDML 1062
 
Captain of SS I-162 elects not to launch torpedoes at this target
I-162 bottoming out ....
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
Day Time Surface Combat, near Hong Kong at 77,66, Range 16,000 Yards
 
Japanese Ships
      PC Kaki
      PC Sumire
 
Allied Ships
      xAK Bennevis, Shell hits 4
      MTB 9
 
Maximum visibility in Partly Cloudy Conditions: 28,000 yards
Range closes to 16,000 yards...
CONTACT: Japanese lookouts spot Allied task force at 16,000 yards
CONTACT: Allied lookouts spot Japanese task force at 16,000 yards
MTB 9 engages PC Sumire at 16,000 yards
Range closes to 15,000 yards
PC Sumire engages xAK Bennevis at 15,000 yards
PC Sumire engages xAK Bennevis at 15,000 yards
Range closes to 12,000 yards
PC Kaki engages xAK Bennevis at 12,000 yards
Range closes to 10,000 yards
Range closes to 7,000 yards
PC Sumire engages xAK Bennevis at 7,000 yards
Range increases to 10,000 yards
MTB 9 engages PC Sumire at 10,000 yards
PC Kaki engages xAK Bennevis at 10,000 yards
Range increases to 12,000 yards
PC Sumire engages MTB 9 at 12,000 yards
PC Sumire engages xAK Bennevis at 12,000 yards
Range increases to 13,000 yards
MTB 9 engages PC Sumire at 13,000 yards
PC Kaki engages xAK Bennevis at 13,000 yards
Task forces break off...
 
 
--------------------------------------------------------------------------------
Day Time Surface Combat, near Hong Kong at 77,66, Range 14,000 Yards
 
Japanese Ships
      PC Kaki
      PC Sumire
 
Allied Ships
      xAK Hanyang, Shell hits 3
      xAKL Joan Moller, Shell hits 3,  on fire
 
Maximum visibility in Partly Cloudy Conditions: 26,000 yards
Range closes to 16,000 yards...
CONTACT: Japanese lookouts spot Allied task force at 16,000 yards
Range closes to 14,000 yards...
Japanese open fire on surprised Allied ships at 14,000 yards
PC Sumire fires at xAK Hanyang at 14,000 yards
Range closes to 11,000 yards
PC Sumire engages xAK Hanyang at 11,000 yards
Range closes to 7,000 yards
PC Sumire engages xAKL Joan Moller at 7,000 yards
Range increases to 9,000 yards
PC Sumire engages xAK Hanyang at 9,000 yards
PC Kaki engages xAKL Joan Moller at 9,000 yards
Range increases to 13,000 yards
PC Sumire engages xAK Hanyang at 13,000 yards
Range increases to 16,000 yards
PC Kaki engages xAK Hanyang at 16,000 yards
PC Kaki engages xAKL Joan Moller at 16,000 yards
Task forces break off...
 
 
--------------------------------------------------------------------------------
Morning Air attack on TF, near Georgetown at 49,74
 
Weather in hex: Heavy cloud
 
Raid spotted at 4 NM, estimated altitude 16,000 feet.
Estimated time to target is 1 minutes
 
Japanese aircraft
      A6M2 Zero x 17
      G3M2 Nell x 18
 
No Japanese losses
 
Allied Ships
      xAK Ellenga, Torpedo hits 3, and is sunk
      xAK Demosthenes, Torpedo hits 3, and is sunk
      xAP Hong Siang, Torpedo hits 2, and is sunk
 
Allied ground losses:
      1570 casualties reported
         Squads: 32 destroyed, 34 disabled
         Non Combat: 55 destroyed, 55 disabled
         Engineers: 5 destroyed, 0 disabled
      Guns lost 11 (6 destroyed, 5 disabled)
      Vehicles lost 14 (9 destroyed, 5 disabled)
 
Aircraft Attacking:
      18 x G3M2 Nell launching torpedoes at 200 feet
               Naval Attack:  1 x 18in Type 91 Torpedo
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on Clark Field , at 79,76 
 
Weather in hex: Partial cloud
 
Raid spotted at 40 NM, estimated altitude 22,000 feet.
Estimated time to target is 13 minutes
 
Japanese aircraft
      A6M2 Zero x 4
 
Allied aircraft
      P-40E Warhawk x 8
 
No Japanese losses
 
Allied aircraft losses
      P-40E Warhawk: 2 destroyed
 
CAP engaged:
24th PG/3rd PS with P-40E Warhawk (0 airborne, 0 on standby, 0 scrambling)
      (1 plane(s) diverted to support CAP in hex.)
      0 plane(s) not yet engaged, 1 being recalled, 0 out of immediate contact.
      Group patrol altitude is 20000 , scrambling fighters between 0 and 20000.
      Time for all group planes to reach interception is 3 minutes
24th PG/17th PS with P-40E Warhawk (2 airborne, 0 on standby, 0 scrambling)
      (7 plane(s) diverted to support CAP in hex.)
      2 plane(s) intercepting now.
      0 plane(s) not yet engaged, 5 being recalled, 0 out of immediate contact.
      Group patrol altitude is 10000 , scrambling fighters between 0 and 10000.
      Time for all group planes to reach interception is 1 minutes
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on 20th Chinese Corps, at 83,51 , near Wuchang
 
Weather in hex: Thunderstorms
 
Raid spotted at 10 NM, estimated altitude 11,000 feet.
Estimated time to target is 2 minutes
 
Japanese aircraft
      Ki-27b Nate x 12
      Ki-30 Ann x 11
 
No Japanese losses
 
Aircraft Attacking:
      11 x Ki-30 Ann bombing from 9000 feet
               Ground Attack:  1 x 250 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on TF, near Georgetown at 49,74
 
Weather in hex: Heavy cloud
 
Raid spotted at 9 NM, estimated altitude 9,000 feet.
Estimated time to target is 2 minutes
 
Japanese aircraft
      Ki-27b Nate x 2
      Ki-30 Ann x 13
 
No Japanese losses
 
Allied Ships
      xAKL Nam Yong
      xAP Kelantan, Bomb hits 1, and is sunk
 
Allied ground losses:
      15 casualties reported
         Squads: 0 destroyed, 0 disabled
         Non Combat: 1 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
Aircraft Attacking:
      13 x Ki-30 Ann bombing from 6000 feet
               Naval Attack:  1 x 250 kg SAP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on Hong Kong , at 77,61 
 
Weather in hex: Severe storms
 
Raid spotted at 17 NM, estimated altitude 21,000 feet.
Estimated time to target is 4 minutes
 
Japanese aircraft
      Ki-44 Tojo x 8
 
No Japanese losses
 
Aircraft Attacking:
       8 x Ki-44 Tojo sweeping at 20000 feet
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on Georgetown , at 49,74 
 
Weather in hex: Heavy cloud
 
Raid spotted at 6 NM, estimated altitude 32,000 feet.
Estimated time to target is 1 minutes
 
Japanese aircraft
      Ki-43-Ia Oscar x 23
 
No Japanese losses
 
Aircraft Attacking:
      23 x Ki-43-Ia Oscar sweeping at 30000 feet
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on Clark Field , at 79,76 
 
Weather in hex: Partial cloud
 
Raid detected at 27 NM, estimated altitude 22,000 feet.
Estimated time to target is 8 minutes
 
Japanese aircraft
      A6M2 Zero x 45
 
Allied aircraft
      P-40B Warhawk x 1
      P-40E Warhawk x 3
 
No Japanese losses
 
Allied aircraft losses
      P-40B Warhawk: 1 destroyed
      P-40E Warhawk: 2 destroyed
 
Aircraft Attacking:
      45 x A6M2 Zero sweeping at 20000 feet
 
CAP engaged:
24th PG/20th PS with P-40B Warhawk (0 airborne, 0 on standby, 0 scrambling)
      (1 plane(s) diverted to support CAP in hex.)
      0 plane(s) not yet engaged, 0 being recalled, 1 out of immediate contact.
      Group patrol altitude is 10000 , scrambling fighters between 0 and 10000.
      Time for all group planes to reach interception is 36 minutes
24th PG/17th PS with P-40E Warhawk (0 airborne, 0 on standby, 2 scrambling)
      0 plane(s) not yet engaged, 0 being recalled, 1 out of immediate contact.
      Group patrol altitude is 10000 
      Time for all group planes to reach interception is 34 minutes
 
 
 
--------------------------------------------------------------------------------
Afternoon Air attack on TF, near Kota Bharu at 51,75
 
Weather in hex: Partial cloud
 
Raid spotted at 16 NM, estimated altitude 5,000 feet.
Estimated time to target is 5 minutes
 
Japanese aircraft
      Ki-27b Nate x 25
 
Allied aircraft
      Hudson I x 3
 
No Japanese losses
 
Allied aircraft losses
      Hudson I: 1 destroyed
 
CAP engaged:
1st Sentai with Ki-27b Nate (11 airborne, 0 on standby, 0 scrambling)
      (4 plane(s) diverted to support CAP in hex.)
      11 plane(s) intercepting now.
      Group patrol altitude is 15000 , scrambling fighters between 0 and 15000.
      Raid is overhead
11th Sentai with Ki-27b Nate (14 airborne, 0 on standby, 0 scrambling)
      (1 plane(s) diverted to support CAP in hex.)
      14 plane(s) intercepting now.
      Group patrol altitude is 10000 , scrambling fighters between 0 and 10000.
      Raid is overhead
 
 
 
--------------------------------------------------------------------------------
Afternoon Air attack on TF, near Bacolod at 79,85
 
Weather in hex: Heavy rain
 
Raid spotted at 16 NM, estimated altitude 13,000 feet.
Estimated time to target is 6 minutes
 
Japanese aircraft
      A5M4 Claude x 7
      B5N1 Kate x 12
 
No Japanese losses
 
Allied Ships
      xAK Governor Wright, Torpedo hits 3, and is sunk
 
Aircraft Attacking:
      12 x B5N1 Kate launching torpedoes at 200 feet
               Naval Attack:  1 x 18in Type 91 Torpedo
 
 
 
--------------------------------------------------------------------------------
 
Ground combat at Pengpu (90,50)
 
Japanese Deliberate attack
 
Attacking force 5629 troops, 38 guns, 0 vehicles, Assault Value = 184
 
Defending force 1498 troops, 18 guns, 0 vehicles, Assault Value = 46
 
Japanese adjusted assault: 191 
 
Allied adjusted defense: 5 
 
Japanese assault odds: 38 to 1 
 
Combat modifiers
Defender: leaders(+), leaders(-), morale(-), experience(-)
Attacker: leaders(+)
 
Japanese ground losses:
      9 casualties reported
         Squads: 0 destroyed, 1 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
Allied ground losses:
      972 casualties reported
         Squads: 39 destroyed, 9 disabled
         Non Combat: 19 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
      Guns lost 3 (3 destroyed, 0 disabled)
      Units retreated 1
 
Defeated Allied Units Retreating!
 
Assaulting units:
    13th Ind.Mixed Brigade
 
Defending units:
    89th Chinese Corps
 
 
--------------------------------------------------------------------------------
 
Ground combat at Tsingsing (90,40)
 
Japanese Deliberate attack
 
Attacking force 11081 troops, 131 guns, 32 vehicles, Assault Value = 908
 
Defending force 2557 troops, 16 guns, 0 vehicles, Assault Value = 112
 
Japanese adjusted assault: 146 
 
Allied adjusted defense: 37 
 
Japanese assault odds: 3 to 1 (fort level 1) 
 
Japanese forces CAPTURE Tsingsing !!!
 
Combat modifiers
Defender: terrain(+), forts(+), op mode(-), leaders(-), experience(-)
Attacker: 
 
Japanese ground losses:
      348 casualties reported
         Squads: 2 destroyed, 38 disabled
         Non Combat: 0 destroyed, 4 disabled
         Engineers: 0 destroyed, 2 disabled
      Units pursuing 3
 
Allied ground losses:
      1493 casualties reported
         Squads: 55 destroyed, 18 disabled
         Non Combat: 37 destroyed, 12 disabled
         Engineers: 4 destroyed, 0 disabled
      Guns lost 5 (4 destroyed, 1 disabled)
      Units retreated 1
 
Defeated Allied Units Retreating!
 
Assaulting units:
    37th Division
    41st Division
    4th Ind.Mixed Brigade
    6th Medium Field Artillery Regiment
 
Defending units:
    1/19th Chinese Corps
 
 
--------------------------------------------------------------------------------
 
Ground combat at Kota Bharu (51,75)
 
Allied Bombardment attack
 
Attacking force 1158 troops, 6 guns, 0 vehicles, Assault Value = 97
 
Defending force 5298 troops, 41 guns, 0 vehicles, Assault Value = 174
 
Assaulting units:
    FMSV Brigade
    8th Indian Brigade
    3rd ISF Base Force
 
Defending units:
    56th Infantry Regiment
    12th Engineer Regiment
    5th JAAF AF Coy 
 
 

===== section @16024 len=114 =====
COMBAT EVENTS FOR 12/09/41

Vk.A.4 sub-groups COMBINING at Soerabaja
Vk.A.2 sub-groups COMBINING at Soerabaja

===== section @16142 len=2 =====


===== section @16148 len=2 =====


===== section @16154 len=15364 =====
OPERATIONAL REPORT FOR Dec 08, 41

SS K XII reports having been sighted by the enemy at 54 , 78
21st Light AA Regiment packing for load on xAK Michael Embiricos (TF 325), 1 day(s) remaining
B Sqn 3rd Hussars Regiment packing for load on xAK Maldive (TF 325), 1 day(s) remaining
21st Light AA Regiment packing for load on xAK Clan Lamont (TF 325), 1 day(s) remaining
B Sqn 3rd Hussars Regiment packing for load on xAK Bhima (TF 325), 1 day(s) remaining
21st Light AA Regiment packing for load on xAK African Prince (TF 325), 1 day(s) remaining
2nd Marine Defense Battalion packing for load on APD Waters (TF 389), 1 day(s) remaining
110th USA Base Force packing for load on AP William Ward Burrows (TF 389), 1 day(s) remaining
2nd Marine Defense Battalion packing for load on AP Harris (TF 389), 1 day(s) remaining
101st USN Base Force packing for load on AK Bellatrix (TF 389), 1 day(s) remaining
I US Amphib Corps packing for load on xAP Santa Maria (TF 389), 2 day(s) remaining
101st USN Base Force packing for load on xAK Exmoor (TF 389), 1 day(s) remaining
I US Amphib Corps packing for load on xAKL Mexican (TF 389), 2 day(s) remaining
CS Convoy TF 321 unable to load any cargo at Vancouver 
CMc Pro Patria lays minefield near Palembang ( 49 , 90) and sets course for Batavia
TF 442 converted to Escort TF 491 (4 ships) and routed to Manila
DD John D. Ford sinks at sea
DD Pillsbury sinks at sea
DD Pope sinks at sea
Repairs completed on CA New Orleans at Pearl Harbor, ship returned to service
TF 441 leaving patrol zone to refuel at Melbourne
21st Light AA Regiment packing for load on xAK Michael Embiricos (TF 325), 1 day(s) remaining
B Sqn 3rd Hussars Regiment packing for load on xAK Maldive (TF 325), 1 day(s) remaining
21st Light AA Regiment packing for load on xAK Clan Lamont (TF 325), 1 day(s) remaining
B Sqn 3rd Hussars Regiment packing for load on xAK Bhima (TF 325), 1 day(s) remaining
21st Light AA Regiment packing for load on xAK African Prince (TF 325), 1 day(s) remaining
2nd Marine Defense Battalion packing for load on APD Waters (TF 389), 1 day(s) remaining
110th USA Base Force packing for load on AP William Ward Burrows (TF 389), 1 day(s) remaining
2nd Marine Defense Battalion packing for load on AP Harris (TF 389), 1 day(s) remaining
101st USN Base Force packing for load on AK Bellatrix (TF 389), 1 day(s) remaining
I US Amphib Corps packing for load on xAP Santa Maria (TF 389), 2 day(s) remaining
101st USN Base Force packing for load on xAK Exmoor (TF 389), 1 day(s) remaining
I US Amphib Corps packing for load on xAKL Mexican (TF 389), 2 day(s) remaining
xAKL Mauban observes H6K4 Mavis at 78,93 near Dadjangas
TF 349 snooped by Japanese Level Bomber at 78,93 near Dadjangas
TF 348 sights Japanese Aircraft at 79,87 near Tagbilaran
xAP Kelantan followed by Japanese Aircraft at 49,74 near Georgetown
TF 399 snooped by Japanese Aircraft at 49,74 near Georgetown
TF 400 shadowed by Japanese Float Plane at 49,74 near Georgetown
TF 447 observes Japanese Aircraft at 76,68 near Paracel Islands
TF 338 sighted by Japanese Aircraft at 50,84 near Singapore
SBD-3 Dauntless reports diving submarine at 143, 105 near Taongi
SBD-2 Dauntless sighting report: 9 Japanese ships at 166,122 near Johnston Island, speed 13, Moving West
xAK Governor Wright sights Japanese Aircraft at 79,85 near Bacolod
TF 345 followed by Japanese Level Bomber at 79,85 near Bacolod
Japanese Aircraft sighted over Kuantan 
Japanese Aircraft sighted over Clark Field 
Japanese Aircraft sighted over Clark Field 
Japanese Aircraft sighted over Clark Field 
Japanese Torpedo Bomber sighted over 21st Chinese Corps
Ki-51 Sonia sighted over 21st Chinese Corps
C5M2 Babs sighted over Batangas 
Japanese Recon sighted over 21st Chinese Corps
Ki-36 Ida sighted over Tuguegarao 
Japanese Light Bomber sighted over Tuguegarao 
Japanese Dive Bomber sighted over Tuguegarao 
Japanese Torpedo Bomber sighted over Chengchow 
Japanese Fighter Bomber sighted over Tsingsing 
Japanese Fighter sighted over 26th Group Army
Japanese Aircraft sighted over Changsha 
Ki-15-II Babs sighted over Wenchow 
Japanese Fighter Bomber sighted over Iba 
Japanese Aircraft sighted over San Fernando 
5/14th Punjab Battalion destroyed near Georgetown !!!
5/14th Punjab Battalion destroyed aboard xAP Kelantan
Ki-15-II Babs sighted over 20th Chinese Corps
Japanese Recon sighted over 20th Chinese Corps
Japanese Aircraft sighted over Hong Kong 
Japanese Aircraft sighted over Georgetown 
Ki-15-II Babs sighted over Taiping 
Japanese Aircraft sighted over Mersing 
Ki-46-II Dinah sighted over Manila 
Japanese Fighter sighted over Kuantan 
Japanese Fighter Bomber sighted over Clark Field 
C5M2 Babs sighted over Clark Field 
Japanese Fighter sighted over Clark Field 
Japanese Dive Bomber sighted over 21st Chinese Corps
Japanese Aircraft sighted over 21st Chinese Corps
C5M2 Babs sighted over Batangas 
Japanese Light Bomber sighted over 21st Chinese Corps
Japanese Light Bomber sighted over Tuguegarao 
Japanese Aircraft sighted over Chengchow 
Japanese Aircraft sighted over Tsingsing 
Japanese Aircraft sighted over 26th Group Army
Japanese Aircraft sighted over Changsha 
Ki-15-II Babs sighted over Wenchow 
Japanese Fighter Bomber sighted over Iba 
Japanese Aircraft sighted over San Fernando 
Japanese Aircraft sighted over 20th Chinese Corps
Japanese Aircraft sighted over 20th Chinese Corps
Japanese Aircraft sighted over 20th Chinese Corps
Japanese Fighter sighted over Hong Kong 
Japanese Aircraft sighted over Georgetown 
Ki-15-II Babs sighted over Taiping 
Japanese Aircraft sighted over Mersing 
Japanese Aircraft sighted over Manila 
TF 342 detects Japanese Torpedo Bomber at 75,67 near Paracel Islands
xAKL Luzon sights Japanese Float Plane at 75,88 near Zamboanga
TF 338 shadowed by Japanese Float Plane at 50,84 near Singapore
Sighting report: 9 Japanese ships at 51,75 near Kota Bharu , Speed unknown  
Japanese Aircraft sighted over Kota Bharu 
Japanese Aircraft sighted over Kuantan 
Japanese Aircraft sighted over Johore Bahru 
Japanese Dive Bomber sighted over 21st Chinese Corps
Japanese Aircraft sighted over 21st Chinese Corps
Japanese Aircraft sighted over Batangas 
Japanese Light Bomber sighted over 21st Chinese Corps
Japanese Light Bomber sighted over Tuguegarao 
Japanese Torpedo Bomber sighted over Tuguegarao 
Ki-36 Ida sighted over Tuguegarao 
Japanese Aircraft sighted over 55th Chinese Corps
Japanese Aircraft sighted over 26th Group Army
Japanese Aircraft sighted over 88th Chinese Corps
Japanese Aircraft sighted over Wenchow 
Japanese Aircraft sighted over Bataan 
Japanese Aircraft sighted over Iba 
Ki-46-I Dinah sighted over San Fernando 
Japanese Aircraft sighted over Georgetown 
Japanese Aircraft sighted over Alor Star 
Japanese Aircraft sighted over Taiping 
Japanese Recon sighted over Mandalay 
Japanese Aircraft sighted over Vigan 
Japanese Aircraft sighted over Manila 
Japanese Aircraft sighted over Kota Bharu 
Japanese Recon sighted over Kuantan 
Japanese Torpedo Bomber sighted over Johore Bahru 
Ki-51 Sonia sighted over 21st Chinese Corps
Japanese Dive Bomber sighted over 21st Chinese Corps
Japanese Recon sighted over Batangas 
Japanese Dive Bomber sighted over 21st Chinese Corps
Japanese Light Bomber sighted over Tuguegarao 
Japanese Aircraft sighted over 55th Chinese Corps
Japanese Aircraft sighted over 26th Group Army
Japanese Fighter sighted over 88th Chinese Corps
Japanese Fighter sighted over Wenchow 
Japanese Aircraft sighted over Bataan 
Japanese Aircraft sighted over Iba 
Japanese Aircraft sighted over San Fernando 
Japanese Fighter sighted over Georgetown 
Japanese Aircraft sighted over Alor Star 
Ki-15-II Babs sighted over Taiping 
Japanese Aircraft sighted over Mandalay 
Japanese Aircraft sighted over Vigan 
Ki-46-II Dinah sighted over Manila 
SBD-3 Dauntless from VS-2 crashes on landing
SBD-2 Dauntless from VB-2 is damaged on landing
SBD-2 Dauntless from VB-2 is damaged on landing
2 x PBY-4 Catalina transporting SE Borneo KNIL Battalion from Bandjermasin
7 x PBY-4 Catalina transporting SE Borneo KNIL Battalion from Bandjermasin
6 x PBY-4 Catalina transporting SE Borneo KNIL Battalion from Bandjermasin
11 x L-18 Lodestar transporting Singkawang Base Force from Singkawang
3 x Do-24K-1 transporting SSVF Brigade from Singapore
1 x Do-24K-1 transporting SE Borneo KNIL Battalion from Bandjermasin
2 x Do-24K-1 transporting Sorong Det. Base Force from Sorong
2 x Do-24K-1 transporting 224 Group RAF  from Singapore
4 x Catalina I transporting 1st Ind. Coy  from Kavieng
4 x Catalina I transporting Lark Force  from Rabaul
2 x S.23 Empire transporting Rabaul Det. Base Force from Rabaul
2 x S.23 Empire transporting Rabaul Det. Base Force from Rabaul
4 x Catalina I transporting 1st Regt Cavalerie  from Batavia
5/14th Punjab Battalion DISPERSES!!
Repairs completed on SS K VIII at Soerabaja, ship returned to service
TF 320 does not have enough fuel to complete mission
TF 323 does not have enough fuel to complete mission
TF 342 does not have enough fuel to complete mission
TF 344 does not have enough fuel to complete mission
TF 359 does not have enough fuel to complete mission
TF 360 does not have enough fuel to complete mission
TF 361 does not have enough fuel to complete mission
TF 362 does not have enough fuel to complete mission
TF 365 does not have enough fuel to complete mission
TF 375 does not have enough fuel to complete mission
TF 420 does not have enough fuel to complete mission
TF 424 does not have enough fuel to complete mission
TF 443 does not have enough fuel to complete mission
TF 444 does not have enough fuel to complete mission
TF 445 does not have enough fuel to complete mission
TF 447 does not have enough fuel to complete mission
TF 475 does not have enough fuel to complete mission
TF 479 does not have enough fuel to complete mission
Japanese forces CAPTURE Tsingsing !!!
San Francisco expands fortifications to size 2
Hilo expands fortifications to size 1
Lahaina expands fortifications to size 1
Palmyra expands fortifications to size 1
Pago Pago expands fortifications to size 1
Manila expands fortifications to size 1
Christmas Island expands fortifications to size 1
Bangalore expands fortifications to size 1
Hyderabad expands fortifications to size 1
Canberra expands fortifications to size 1
Portland expands fortifications to size 1
Wagga Wagga expands fortifications to size 1
Calgary expands fortifications to size 1
Winnipeg expands fortifications to size 1
Puckapunyal expands fortifications to size 1
69th OG/82nd OS at Fort Ord takes O-47A replacements
70th OG/123rd OS at Ogden takes O-47A replacements
7th BG/9th BS at San Francisco takes B-17D Fortress replacements
24th PG/3rd PS at Manila takes P-40E Warhawk replacements
55th PG/38th PS at Ogden takes P-43 Lancer replacements
20th PG/55th PS at Ogden takes P-40B Warhawk replacements
54th PG/56th PS at Ogden takes P-39D Airacobra replacements
54th PG/57th PS at Ogden takes P-39D Airacobra replacements
Cavite USN Base Force - some devices have been upgraded at Manila
3rd/45th PS Inf Battalion - some devices have been upgraded at Manila
21st PA Infantry Division - some devices have been upgraded at Lingayen
51st PA Infantry Division - some devices have been upgraded at Lucena
A Coy/49th Aus Battalion - some devices have been upgraded at Horn Island
A Coy/NGV Rifles  - some devices have been upgraded at Lae
B Coy/NGV Rifles  - some devices have been upgraded at Rabaul
D Coy/NGV Rifles  - some devices have been upgraded at Salamaua
2nd RAN Base Force - some devices have been upgraded at Darwin
Delayed planes for the aircraft pool:
  0 (2) x Catalina I, 1 (2) x Bolingbroke IV, 0 (1) x P-26A, 0 (2) x P-36A Mohawk, 3 (20) x P-39D Airacobra
  0 (3) x P-40B Warhawk, 3 (12) x P-43 Lancer, 4 (5) x OS2U-3 Kingfisher, 1 (10) x PBY-4 Catalina
  3 (5) x PBY-5 Catalina, 0 (1) x SNJ-3 Texan
Device(s) upgraded in 5th RAAF Base Force supplied from Esperance
Device(s) upgraded in 15th RAAF Base Force supplied from Port Moresby
Device(s) upgraded in 22nd Light Horse Regiment supplied from Hobart
Device(s) upgraded in 6th RAN Base Force supplied from Townsville
Device(s) upgraded in 16th RAAF Base Force supplied from Portland
Device(s) upgraded in 4th RAN Base Force supplied from Perth
Device(s) upgraded in 19th Australian Battalion supplied from Darwin
Device(s) upgraded in 17th RAAF Base Force supplied from Ceduna
Device(s) upgraded in DG RN Detachment  supplied from Diego Garcia
Device(s) upgraded in 31st Infantry Regiment supplied from Manila
Low garrison leads to damage and 1 VP loss from partisan attack at Colombo (29, 48)!
Low garrison leads to damage and 1 VP loss from partisan attack at Mandalay (59, 46)!
Low garrison leads to damage and 1 VP loss from partisan attack at Madurai (30, 42)!
Benjamins, P.A.C. now available for reassignment
Chan E. now available for reassignment
Marshall C. now available for reassignment
Lopatin T. now available for reassignment
AV Wright arrives at Pearl Harbor
xAK Ewa arrives at Balboa
xAKL Island Mail arrives at Balboa
Loss of xAK Governor Wright on Dec 08, 1941 is admitted
xAP Largs Bay arrives at Aden
Loss of xAK Demosthenes on Dec 08, 1941 is admitted
Loss of xAK Ellenga on Dec 08, 1941 is admitted
xAP Felix Roussel arrives at Cape Town
xAP Cap St Jacques arrives at Cape Town
Loss of xAP Hong Siang on Dec 08, 1941 is admitted
Loss of xAP Kelantan on Dec 08, 1941 is admitted
20th PG/55th PS arrives at Ogden
42nd BG/75th BS arrives at San Diego
16th RS arrives at San Diego
42nd BG/77th BS arrives at San Diego
42nd BG/76th BS arrives at San Diego
AVG/1st Sqn arrives at Rangoon
No.62 Sqn RAF arrives at Singapore
24th PG/20th PS arrives at Manila
51st PG/16th PS arrives at San Diego
51st PG/25th PS arrives at San Diego
14th PG/48th PS arrives at Ogden
54th PG/56th PS arrives at Ogden
55th PG/38th PS arrives at Ogden
12th BG/82nd BS arrives at Ogden
No.1 Sqn RAAF arrives at Singapore
No.36 Sqn RAF Det arrives at Singapore
1e-2-Vl.G.I arrives at Batavia
1-Vl.G.IV arrives at Batavia
Vk.A.1 arrives at Batavia
Vk.A.4 arrives at Batavia
69 SAD/76 IAP arrives at Vladivostok
34 SAD/77 IAP arrives at Vladivostok
34 SAD/59 BAP arrives at Vladivostok
34 SAD/36 BAP arrives at Vladivostok
34 SAD/47 IAP arrives at Vladivostok
No.6 Sqn RAAF Det arrives at Townsville
No.6 Sqn RAAF arrives at Townsville
VS-1D11 arrives at San Diego
No.243 Sqn RAF Det arrives at Singapore
34 SAD/53 IAP arrives at Vladivostok
29 IAD/60 IAP arrives at Vladivostok
69th OG/31st OS arrives at Ogden
51st PG/26th PS arrives at Ogden
55th PG/37th PS arrives at Ogden
54th PG/42nd PS arrives at Ogden
14th PG/50th PS arrives at Ogden
55th PG/54th PS arrives at Ogden
14th PG/HqS arrives at Ogden
54th PG/HqS arrives at Ogden
55th PG/HqS arrives at Ogden
201st PA Construction Battalion arrives at Bataan
Task Force 515 assigned to defensive patrol 179, 108 - 177, 108 off Pearl Harbor
Planes and pilots of HK Flight FAA withdrawing
Planes and pilots of HK Flight RAF Det withdrawing
Task Force 345 assigned to patrol 179, 106 - 179, 103 around 183, 105

===== section @31522 len=1261 =====
SIG INT REPORT FOR Dec 08, 41

Heavy Volume of Radio transmissions detected at 66,69.
Radio transmissions detected at 66,69.
Heavy Volume of Radio transmissions detected at 64,73.
1/5th Field AF Construction Battalion is loaded on a Yusen A class AK moving to Kota Bharu.
2/5th Field Artillery Regiment is loaded on a Japanese xAK moving to Kota Bharu.
Radio transmissions detected at Hailun (111,37).
Radio transmissions detected at Kota Bharu (51,75).
2/Kure 1st SNLF  is loaded on a Ansyu-C Cargo class xAK moving to Legaspi.
6th Field AF Construction Battalion is located at Saigon(60,71).
Heavy Volume of Radio transmissions detected at 94,70.
2/2nd Ind.Art.Mortar Battalion is loaded on a Japanese xAK moving to Batan Island.
Heavy Volume of Radio transmissions detected at 111,81.
1/148th Infantry Regiment is loaded on a Japanese xAKL moving to Kota Bharu.
4th JAAF AF Bn  is located at Takao(84,65).
Heavy Volume of Radio transmissions detected at Takao (84,65).
57th Infantry Regiment is located at Sunwu(113,34).
6th Ind.Hvy.Art. Battalion is located at Yenki(110,44).
1st Ind.Inf.Group  is located at Canton(77,59).
Heavy Volume of Radio transmissions detected at Takao (84,65).
37th Const Co  is located at Pescadores(84,63).
```

#### T3 `251c` — save date 12/10/41

Complete text of the record, all 4 sections, exactly as stored (section banners added by the extractor):

```text
===== section @0 len=23139 =====
AFTER ACTION REPORTS FOR Dec 09, 41
--------------------------------------------------------------------------------
Sub attack near Tioman Island  at 52,84
 
Japanese Ships
      SS I-155
 
Allied Ships
      HDML 1062
 
Captain of SS I-155 elects not to launch torpedoes at this target
I-155 bottoming out ....
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
Sub attack near Muntok  at 52,90
 
Japanese Ships
      SS I-162
 
Allied Ships
      CM Willem v d Zaan
 
SS I-162 launches 4 torpedoes
I-162 bottoming out ....
CM Willem v d Zaan fails to find sub, continues to search...
CM Willem v d Zaan fails to find sub, continues to search...
CM Willem v d Zaan fails to find sub, continues to search...
CM Willem v d Zaan fails to find sub, continues to search...
CM Willem v d Zaan fails to find sub, continues to search...
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
ASW attack near Kuantan  at 52,79
 
Japanese Ships
      DD Maikaze
      CA Mikuma
      DD Hagikaze
      DD Arashi
 
Allied Ships
      SS K XIII
 
SS K XIII launches 4 torpedoes at DD Maikaze
K XIII bottoming out ....
DD Hagikaze fails to find sub, continues to search...
DD Arashi fails to find sub, continues to search...
DD Hagikaze fails to find sub, continues to search...
DD Arashi fails to find sub and abandons search
DD Hagikaze fails to find sub, continues to search...
DD Hagikaze fails to find sub, continues to search...
DD Hagikaze fails to find sub, continues to search...
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
ASW attack near Kuantan  at 51,79
 
Japanese Ships
      CA Atago
      DD Hibiki
      DD Akatsuki
      DD Arashio
 
Allied Ships
      SS O 19, hits 8
 
SS O 19 is sighted by escort
DD Hibiki attacking submerged sub ....
DD Akatsuki fails to find sub, continues to search...
DD Arashio fails to find sub and abandons search
DD Hibiki fails to find sub and abandons search
DD Akatsuki fails to find sub, continues to search...
DD Akatsuki fails to find sub, continues to search...
DD Akatsuki attacking submerged sub ....
DD Akatsuki fails to find sub, continues to search...
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
Pre-Invasion action off Kuantan (51,79)
 
45 Coastal gun shots fired in defense.
 
Japanese Ships
      DD Shirakumo
      DD Shinonome
      AK Tosan Maru
 
DD Shirakumo firing at 22nd Indian Brigade
DD Shinonome firing at 3rd HK&S Light AA Regiment
DD Shirakumo fired at enemy troops
Defensive Guns fire at approaching troops in landing craft at 4,000 yards
Defensive Guns fire at approaching troops in landing craft at 1,000 yards
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Kuantan (51,79)
 
TF 92 troops unloading over beach at Kuantan, 51,79
 
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Bukit Besi (52,78)
 
TF 98 troops unloading over beach at Bukit Besi, 52,78
 
Japanese ground losses:
      20 casualties reported
         Squads: 1 destroyed, 0 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
17 troops of a SNLF Squad lost from landing craft during unload of 91st Nav Gd /1
 
 
--------------------------------------------------------------------------------
Pre-Invasion action off Guam (106,95)
Defensive Guns engage approaching landing force
 
2 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Kohuku Maru
      PB Hirotama Maru
      xAK Shofuku Maru
 
PB Kohuku Maru fired at enemy troops
PB Hirotama Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Guam (106,95)
 
TF 128 troops unloading over beach at Guam, 106,95
 
12 Support troops lost from landing craft during unload of 5th Base Force /2
12 Engineers accidentally lost during unload of 5th Base Force /2
12 Naval Support troops lost overboard during unload of 5th Base Force /2
12 Engineers lost from landing craft during unload of 5th Base Force
12 Aviation Support troops lost overboard during unload of 5th Base Force
12 Support troops lost from landing craft during unload of 5th Base Force
12 Aviation Support troops lost overboard during unload of 5th Base Force
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kuantan (51,79)
 
18 Coastal gun shots fired in defense.
 
Japanese Ships
      DD Shirakumo
      AK Tosan Maru
      DD Shinonome
 
DD Shirakumo fired at enemy troops
Defensive Guns fire at approaching troops in landing craft at 2,000 yards
 
 
--------------------------------------------------------------------------------
Invasion Support action off Guam (106,95)
Defensive Guns engage approaching landing force
 
2 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Kohuku Maru
      PB Hirotama Maru
      xAK Shofuku Maru
 
PB Kohuku Maru fired at enemy troops
PB Hirotama Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Day Time Surface Combat, near Iba at 76,69, Range 3,000 Yards
 
Japanese Ships
      PC Kaki
      PC Sumire
 
Allied Ships
      xAKL Joan Moller, Shell hits 5,  heavy fires,  heavy damage
 
Poor visibility due to Thunderstorms
Maximum visibility in Thunderstorms: 3,000 yards
Range closes to 16,000 yards...
Range closes to 14,000 yards...
Range closes to 12,000 yards...
Range closes to 11,000 yards...
Range closes to 10,000 yards...
Range closes to 9,000 yards...
Range closes to 8,000 yards...
Range closes to 7,000 yards...
Range closes to 6,000 yards...
Range closes to 5,000 yards...
Range closes to 4,000 yards...
Range closes to 3,000 yards...
CONTACT: Japanese lookouts spot Allied task force at 3,000 yards
Japanese open fire on surprised Allied ships at 3,000 yards
PC Sumire fires at xAKL Joan Moller at 3,000 yards
PC Kaki launches Torpedoes at xAKL Joan Moller at 3,000 yards
Range closes to 2,000 yards
PC Sumire engages xAKL Joan Moller at 2,000 yards
Range increases to 6,000 yards
Range increases to 9,000 yards
PC Sumire engages xAKL Joan Moller at 9,000 yards
Task forces break off...
 
 
--------------------------------------------------------------------------------
Day Time Surface Combat, near Iba at 76,69, Range 3,000 Yards
 
Japanese Ships
      PC Kaki
      PC Sumire
 
Allied Ships
      xAK Ming Sang, Shell hits 1,  on fire
      xAKL Yat Shing, Shell hits 10,  heavy fires,  heavy damage
 
Low visibility due to Thunderstorms
Maximum visibility in Thunderstorms: 3,000 yards
Range closes to 16,000 yards...
Range closes to 14,000 yards...
Range closes to 12,000 yards...
Range closes to 11,000 yards...
Range closes to 10,000 yards...
Range closes to 9,000 yards...
Range closes to 8,000 yards...
Range closes to 7,000 yards...
Range closes to 6,000 yards...
Range closes to 5,000 yards...
Range closes to 4,000 yards...
Range closes to 3,000 yards...
CONTACT: Japanese lookouts spot Allied task force at 3,000 yards
Japanese open fire on surprised Allied ships at 3,000 yards
PC Sumire fires at xAKL Yat Shing at 3,000 yards
Range closes to 2,000 yards
PC Sumire engages xAKL Yat Shing at 2,000 yards
PC Sumire engages xAKL Yat Shing at 2,000 yards
Range increases to 3,000 yards
PC Kaki engages xAKL Yat Shing at 3,000 yards
Range increases to 6,000 yards
PC Sumire engages xAK Ming Sang at 6,000 yards
PC Sumire engages xAKL Yat Shing at 6,000 yards
Range increases to 7,000 yards
PC Kaki engages xAKL Yat Shing at 7,000 yards
PC Sumire engages xAKL Yat Shing at 7,000 yards
PC Kaki engages xAK Ming Sang at 7,000 yards
Range closes to 6,000 yards
PC Kaki engages xAK Ming Sang at 6,000 yards
PC Kaki engages xAKL Yat Shing at 6,000 yards
Range increases to 9,000 yards
PC Kaki engages xAKL Yat Shing at 9,000 yards
Task forces break off...
 
 
--------------------------------------------------------------------------------
Submarine attack near Donggala  at 69,96
 
Japanese Ships
      SS I-123
 
Allied Ships
      xAKL Lepus, Shell hits 5,  heavy fires,  heavy damage
 
xAKL Lepus is sighted by SS I-123
SS I-123 attacking xAKL Lepus on the surface
 
 
--------------------------------------------------------------------------------
Submarine attack near Toboali  at 50,95
 
Japanese Ships
      SS I-164
 
Allied Ships
      CMc Pro Patria, Torpedo hits 1,  on fire,  heavy damage
 
CMc Pro Patria is sighted by SS I-164
SS I-164 launches 2 torpedoes
 
 
--------------------------------------------------------------------------------
Morning Air attack on Manila , at 79,77 
 
Weather in hex: Heavy cloud
 
Raid detected at 17 NM, estimated altitude 26,000 feet.
Estimated time to target is 5 minutes
 
Japanese aircraft
      A6M2 Zero x 40
 
Allied aircraft
      P-40B Warhawk x 6
      P-40E Warhawk x 36
 
Japanese aircraft losses
      A6M2 Zero: 2 destroyed
 
Allied aircraft losses
      P-40B Warhawk: 1 destroyed
      P-40E Warhawk: 4 destroyed
 
Aircraft Attacking:
      16 x A6M2 Zero sweeping at 20000 feet * 
 
CAP engaged:
24th PG/3rd PS with P-40E Warhawk (3 airborne, 9 on standby, 0 scrambling)
      12 plane(s) intercepting now.
      0 plane(s) not yet engaged, 0 being recalled, 1 out of immediate contact.
      Group patrol altitude is 20000 , scrambling fighters between 20000 and 25000.
      Time for all group planes to reach interception is 41 minutes
24th PG/17th PS with P-40E Warhawk (0 airborne, 4 on standby, 0 scrambling)
      4 plane(s) intercepting now.
      0 plane(s) not yet engaged, 5 being recalled, 0 out of immediate contact.
      Group patrol altitude is 10000 , scrambling fighters between 20000 and 21000.
      Time for all group planes to reach interception is 1 minutes
24th PG/20th PS with P-40B Warhawk (2 airborne, 3 on standby, 0 scrambling)
      5 plane(s) intercepting now.
      0 plane(s) not yet engaged, 1 being recalled, 0 out of immediate contact.
      Group patrol altitude is 10000 
      Time for all group planes to reach interception is 1 minutes
35th PG/21st PS with P-40E Warhawk (0 airborne, 6 on standby, 0 scrambling)
      6 plane(s) intercepting now.
      0 plane(s) not yet engaged, 8 being recalled, 0 out of immediate contact.
      Group patrol altitude is 10000 , scrambling fighters between 25000 and 27000.
      Time for all group planes to reach interception is 1 minutes
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on Manila , at 79,77 
 
Weather in hex: Heavy cloud
 
Raid detected at 110 NM, estimated altitude 21,000 feet.
Estimated time to target is 36 minutes
 
Japanese aircraft
      A6M2 Zero x 4
 
Allied aircraft
      P-40E Warhawk x 4
 
No Japanese losses
 
Allied aircraft losses
      P-40E Warhawk: 1 destroyed
 
Aircraft Attacking:
       2 x A6M2 Zero sweeping at 20000 feet * 
 
CAP engaged:
35th PG/21st PS with P-40E Warhawk (0 airborne, 0 on standby, 0 scrambling)
      2 plane(s) not yet engaged, 0 being recalled, 0 out of immediate contact.
      Group patrol altitude is 10000 , scrambling fighters to 11000.
      Time for all group planes to reach interception is 28 minutes
24th PG/17th PS with P-40E Warhawk (0 airborne, 0 on standby, 0 scrambling)
      0 plane(s) not yet engaged, 1 being recalled, 0 out of immediate contact.
      Group patrol altitude is 10000 , scrambling fighters to 13000.
      Time for all group planes to reach interception is 4 minutes
24th PG/3rd PS with P-40E Warhawk (0 airborne, 0 on standby, 0 scrambling)
      1 plane(s) not yet engaged, 0 being recalled, 0 out of immediate contact.
      Group patrol altitude is 20000 , scrambling fighters to 9000.
      Time for all group planes to reach interception is 29 minutes
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on 1/19th Chinese Corps, at 89,40 , near Tsingsing
 
Weather in hex: Light cloud
 
Raid spotted at 17 NM, estimated altitude 13,000 feet.
Estimated time to target is 5 minutes
 
Japanese aircraft
      Ki-48-Ib Lily x 19
 
No Japanese losses
 
Aircraft Attacking:
      19 x Ki-48-Ib Lily bombing from 9000 feet * 
               Ground Attack:  2 x 100 kg GP Bomb
 
Also attacking 33rd Chinese Corps ...
Also attacking 1/19th Chinese Corps ...
 
 
--------------------------------------------------------------------------------
Morning Air attack on 20th Chinese Corps, at 83,51 , near Wuchang
 
Weather in hex: Overcast
 
Raid spotted at 46 NM, estimated altitude 10,000 feet.
Estimated time to target is 13 minutes
 
Japanese aircraft
      Ki-27b Nate x 11
      Ki-30 Ann x 9
 
No Japanese losses
 
Allied ground losses:
      7 casualties reported
         Squads: 0 destroyed, 0 disabled
         Non Combat: 0 destroyed, 1 disabled
         Engineers: 0 destroyed, 0 disabled
 
Aircraft Attacking:
       9 x Ki-30 Ann bombing from 9000 feet
               Ground Attack:  1 x 250 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on Hong Kong , at 77,61 
 
Weather in hex: Light rain
 
Raid spotted at 10 NM, estimated altitude 24,000 feet.
Estimated time to target is 2 minutes
 
Japanese aircraft
      Ki-44 Tojo x 8
 
No Japanese losses
 
Aircraft Attacking:
       8 x Ki-44 Tojo sweeping at 20000 feet
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on 62nd Chinese Corps, at 77,58 , near Canton
 
Weather in hex: Light rain
 
Raid spotted at 35 NM, estimated altitude 9,000 feet.
Estimated time to target is 11 minutes
 
Japanese aircraft
      A6M2 Zero x 9
      Ki-27b Nate x 5
      Ki-48-Ib Lily x 9
 
No Japanese losses
 
Allied ground losses:
      24 casualties reported
         Squads: 0 destroyed, 2 disabled
         Non Combat: 0 destroyed, 1 disabled
         Engineers: 0 destroyed, 0 disabled
 
Aircraft Attacking:
       9 x Ki-48-Ib Lily bombing from 6000 feet
               Ground Attack:  4 x 100 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Submarine attack near Donggala  at 69,96
 
Japanese Ships
      SS I-123
 
Allied Ships
      xAKL Legazpi, Shell hits 5,  heavy fires
 
xAKL Legazpi is sighted by SS I-123
SS I-123 attacking on the surface
SS I-123 low on gun ammo, Ikura C. breaks off surface engagement and submerges
 
 
--------------------------------------------------------------------------------
Sub attack near French Frigate Shoal  at 164,103
 
Japanese Ships
      CV Kaga
      BB Kirishima
      BB Hiei
      CA Chikuma
      DD Isokaze
      DD Kasumi
      DD Arare
      DD Tanikaze
      DD Hamakaze
 
Allied Ships
      SS Trout
 
SS Trout launches 6 torpedoes at CV Kaga
DD Kasumi fails to find sub, continues to search...
DD Arare fails to find sub and abandons search
DD Tanikaze fails to find sub and abandons search
DD Hamakaze attacking submerged sub ....
DD Kasumi attacking submerged sub ....
DD Hamakaze fails to find sub and abandons search
DD Kasumi fails to find sub, continues to search...
DD Kasumi fails to find sub, continues to search...
DD Kasumi fails to find sub, continues to search...
DD Kasumi fails to find sub, continues to search...
DD Kasumi fails to find sub, continues to search...
DD Kasumi fails to find sub, continues to search...
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
Pre-Invasion action off Nauru Island (127,128)
 
Japanese Ships
      DD Yunagi
      CM Okinoshima
      CM Tokiwa
 
Allied ground losses:
      129 casualties reported
         Squads: 0 destroyed, 0 disabled
         Non Combat: 5 destroyed, 7 disabled
         Engineers: 2 destroyed, 2 disabled
      Guns lost 3 (2 destroyed, 1 disabled)
 
DD Yunagi firing at Nauru Det. 
CM Okinoshima firing at Nauru Det. 
CM Tokiwa firing at Nauru Det. 
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Nauru Island (127,128)
 
TF 15 troops unloading over beach at Nauru Island, 127,128
 
12 Support troops accidentally lost during unload of 51st Nav Gd
 
 
--------------------------------------------------------------------------------
Pre-Invasion action off Kuantan (51,79)
Defensive Guns engage approaching landing force
 
12 Coastal gun shots fired in defense.
 
Japanese Ships
      CL Sendai
      DD Shikinami
      AK Sasako Maru
 
DD Shikinami firing at 22nd Indian Brigade
DD Shikinami fired at enemy troops
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Kuantan (51,79)
 
TF 85 troops unloading over beach at Kuantan, 51,79
 
 
 
--------------------------------------------------------------------------------
Pre-Invasion action off Kuantan (51,79)
 
51 Coastal gun shots fired in defense.
 
Japanese Ships
      DD Murakumo
      PB Chosa Maru
      LSD Shinshu Maru
      DD Yugiri
 
Allied ground losses:
      6 casualties reported
         Squads: 0 destroyed, 1 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
DD Murakumo firing at 22nd Indian Brigade
PB Chosa Maru fired at enemy troops
DD Murakumo fired at enemy troops
Defensive Guns fire at approaching troops in landing craft at 4,000 yards
Defensive Guns fire at approaching troops in landing craft at 2,000 yards
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Kuantan (51,79)
 
TF 91 troops unloading over beach at Kuantan, 51,79
 
Japanese ground losses:
      Guns lost 1 (1 destroyed, 0 disabled)
 
12 Support troops accidentally lost during unload of 41st Infantry Rgt /3
10cm T91 Howitzer lost overboard during unload of 5th FA Rgt /1
12 Support troops lost in surf during unload of 11th Infantry Rgt /1
12 Support troops lost in surf during unload of 11th Infantry Rgt /2
12 Support troops lost in surf during unload of 11th Infantry Rgt /2
12 Support troops accidentally lost during unload of 41st Infantry Rgt /1
12 Support troops accidentally lost during unload of 41st Infantry Rgt /1
12 Support troops lost overboard during unload of 11th Infantry Rgt /3
12 Support troops accidentally lost during unload of 41st Infantry Rgt /2
 
 
--------------------------------------------------------------------------------
Pre-Invasion action off Kuantan (51,79)
 
41 Coastal gun shots fired in defense.
 
Japanese Ships
      SC Ch 8
      xAK Kyokko Maru
      AV Sanyo Maru
      DMS W-6
 
SC Ch 8 fired at enemy troops
Defensive Guns fire at approaching troops in landing craft at 4,000 yards
Defensive Guns fire at approaching troops in landing craft at 2,000 yards
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Kuantan (51,79)
 
TF 97 troops unloading over beach at Kuantan, 51,79
 
Japanese ground losses:
      Vehicles lost 2 (2 destroyed, 0 disabled)
 
12 Support troops lost from landing craft during unload of 5th Engr Rgt /1
Type 97 Medium Tank dropped into water during unload of 1st Tank Rgt
Type 97 Medium Tank dropped into water during unload of 1st Tank Rgt
 
 
--------------------------------------------------------------------------------
Invasion Support action off Guam (106,95)
Defensive Guns engage approaching landing force
 
1 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Kohuku Maru
      PB Hirotama Maru
      xAK Shofuku Maru
 
PB Kohuku Maru fired at enemy troops
PB Hirotama Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
 
Ground combat at Kota Bharu (51,75)
 
Japanese Deliberate attack
 
Attacking force 3797 troops, 38 guns, 0 vehicles, Assault Value = 176
 
Defending force 3905 troops, 40 guns, 2 vehicles, Assault Value = 98
 
Japanese adjusted assault: 29 
 
Allied adjusted defense: 14 
 
Japanese assault odds: 2 to 1 (fort level 2) 
 
Japanese Assault reduces fortifications to 1
 
Combat modifiers
Defender: leaders(-), preparation(-), morale(-), experience(-)
Attacker: fatigue(-)
 
Japanese ground losses:
      87 casualties reported
         Squads: 0 destroyed, 10 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
Allied ground losses:
      132 casualties reported
         Squads: 1 destroyed, 16 disabled
         Non Combat: 0 destroyed, 1 disabled
         Engineers: 0 destroyed, 0 disabled
 
Assaulting units:
    56th Infantry Regiment
    12th Engineer Regiment
    5th JAAF AF Coy 
 
Defending units:
    FMSV Brigade
    8th Indian Brigade
    3rd ISF Base Force
 
 
--------------------------------------------------------------------------------
 
Ground combat at Nauru Island (127,128)
 
Japanese Shock attack
 
Attacking force 1620 troops, 12 guns, 0 vehicles, Assault Value = 61
 
Defending force 0 troops, 0 guns, 0 vehicles, Assault Value = 0
 
Japanese adjusted assault: 28 
 
Allied adjusted defense: 1 
 
Japanese assault odds: 28 to 1 (fort level 0) 
 
Japanese forces CAPTURE Nauru Island !!!
 
Combat modifiers
Attacker: shock(+), leaders(-)
 
Assaulting units:
    51st Naval Guard Unit
 
 
--------------------------------------------------------------------------------
 
Ground combat at Kuantan (51,79)
 
Allied Bombardment attack
 
Attacking force 2374 troops, 13 guns, 1 vehicles, Assault Value = 99
 
Defending force 10291 troops, 117 guns, 230 vehicles, Assault Value = 217
 
Allied ground losses:
      6 casualties reported
         Squads: 0 destroyed, 1 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
Assaulting units:
    22nd Indian Brigade
    3rd HK&S Light AA Regiment
    1st ISF Base Force
 
Defending units:
    42nd Infantry Regiment
    5th Recon Regiment
    1st Tank Regiment
    41st Infantry Rgt /4
    11th Infantry Rgt /4
    5th Engr Rgt /3
    25th Army /4
    5th FA Rgt /3
    84th JAAF AF Bn /2
    20th AA Regiment
    5th Field AF Const Bn /2
 
 

===== section @23143 len=7889 =====
COMBAT EVENTS FOR 12/08/41

ADJUST TASK FORCE MISSIONS 
CALCULATE RANGE TO ENEMY
CALCULATE AIR SUPERIORITY
ASSIGN COMPUTER CONTROLLED UNITS
Kwangchowan is occupied by the Japanese
Combat Events:
NAVAL REACTION PHASE
LOADING/UNLOADING TASK FORCES
Amphibious TF 85 unloading at Kota Bharu
TASK FORCE LOADING/UNLOADING COMPLETE
CALCULATING TASK FORCE PATHS
MINESWEEPING OPERATIONS
NAVAL MOVEMENT PHASE
DD Pope hits Type 88 Mine at Bataan
DD Pillsbury hits Type 88 Mine at Bataan
DD John D. Ford hits Type 88 Mine at Bataan
ADJUST SHIP FUEL
NIGHT AIR OPERATIONS PHASE
AIR NIGHT STRIKES
AIRCRAFT LANDING
DD Bagley sinks....
DD Pillsbury sinks....
DD Blue sinks....
SURFACE COMBAT CHECK
DIVIDE CRIPPLED TFs
Amphibious TF 85 unloading at Kota Bharu
REPAIRING SHIPS
END 12 HOUR PULSE
DIVIDE CRIPPLED TFs
CHECKING TF FUEL
TF'S RETURN TO PORT
Combat Events:
NAVAL REACTION PHASE
LOADING/UNLOADING TASK FORCES
Amphibious TF 85 unloading at Kota Bharu
TASK FORCE LOADING/UNLOADING COMPLETE
CALCULATING TASK FORCE PATHS
MINESWEEPING OPERATIONS
NAVAL MOVEMENT PHASE
Task Force 444 encounters enemy Task Force at 77 , 66
Task Force 446 encounters enemy Task Force at 77 , 66
ADJUST SHIP FUEL
AIR OPERATIONS PHASE : AM
xAKL Mauban observes H6K4 Mavis at 78,93 near Dadjangas
TF 349 snooped by Japanese Level Bomber at 78,93 near Dadjangas
TF 348 sights Japanese Aircraft at 79,87 near Tagbilaran
TF 399 snooped by Japanese Aircraft at 49,74 near Georgetown
TF 400 shadowed by Japanese Float Plane at 49,74 near Georgetown
TF 447 observes Japanese Aircraft at 76,68 near Paracel Islands
TF 338 sighted by Japanese Aircraft at 50,84 near Singapore
E7K2 Alf has spotted an Allied AP at 49,74
SBD-3 Dauntless reports diving submarine at 143, 105 near Taongi
SBD-2 Dauntless sighting report: 9 Japanese ships at 166,122 near Johnston Island, speed 13, Moving West
PBY-5 Catalina sighting report: 9 Japanese ships at 172,103 near French Frigate Shoal , Speed 14 , Moving West
PBY-5 Catalina sighting report: 7 Japanese ships at 171,103 near French Frigate Shoal , Speed 17 , Moving West
PBY-4 Catalina sighting report: 6 Japanese ships at 152,87 near Kure Island , Speed 13 , Moving West
xAK Governor Wright sights Japanese Aircraft at 79,85 near Bacolod
TF 345 followed by Japanese Level Bomber at 79,85 near Bacolod
AIR STRIKES
xAK Demosthenes sinks....
xAK Ellenga sinks....
xAP Hong Siang sinks....
xAP Kelantan sinks....
CPT Clarkson M. has been KILLED
AIRCRAFT LANDING
DIVIDE CRIPPLED TFs
AIR OPERATIONS PHASE : PM
TF 342 detects Japanese Torpedo Bomber at 75,67 near Paracel Islands
xAKL Luzon sights Japanese Float Plane at 75,88 near Zamboanga
TF 338 shadowed by Japanese Float Plane at 50,84 near Singapore
PBY-4 Catalina sighting report: 5 Japanese ships at 152,87 near Kure Island , Speed 12 , Moving West
Do-24K-1 sighting report: 2 Japanese ships at 51,75 near Kota Bharu , Speed unknown 
AIR STRIKES
xAK Governor Wright sinks....
CDR Bordonaro, J.H. has been KILLED
AIRCRAFT LANDING
AIR TRANSPORT PHASE
AIRCRAFT LANDING
SURFACE COMBAT CHECK
DIVIDE CRIPPLED TFs
Amphibious TF 85 unloading at Kota Bharu
REPAIRING SHIPS
END 12 HOUR PULSE
DIVIDE CRIPPLED TFs
CHECKING TF FUEL
TF'S RETURN TO PORT
89th Chinese Corps RETREATING towards Hwaiyin
Japanese forces CAPTURE Tsingsing !!!
1/19th Chinese Corps RETREATING towards Yutze
37th Division IN PURSUIT
4th Ind.Mixed Brigade IN PURSUIT
6th Medium Field Artillery Regiment IN PURSUIT
RECALCULATING ZONES OF CONTROL
SUPPLY REQUIREMENTS PHASE
San Francisco expands fortifications to size 2
Hilo expands fortifications to size 1
Lahaina expands fortifications to size 1
Palmyra expands fortifications to size 1
Pago Pago expands fortifications to size 1
Manila expands fortifications to size 1
Christmas Island expands fortifications to size 1
Bangalore expands fortifications to size 1
Hyderabad expands fortifications to size 1
Canberra expands fortifications to size 1
Portland expands fortifications to size 1
Wagga Wagga expands fortifications to size 1
Calgary expands fortifications to size 1
Winnipeg expands fortifications to size 1
Puckapunyal expands fortifications to size 1
MOVING OIL AND RESOURCES
PRODUCTION PHASE
SUPPLY MOVEMENT PHASE
CALCULATING SPOILAGE
AIRCRAFT REPAIR PHASE
AIRCRAFT SUPPLY PHASE
UPGRADING SHIPS
UPGRADING AIR UNITS
UPGRADING LAND UNITS
SUPPLY PHASE COMPLETE
END OF DAY
TRAINING SHIP CREWS
DISBANDING HUMAN CONTROLLED TASK FORCES
ADJUSTING AVIATION SUPPORT
LAND UNIT REPLACEMENT PHASE
AIR UNIT REPLACEMENT PHASE
MERGING SUB UNITS
SUB UNIT MERGE COMPLETE
DIVIDE CRIPPLED TFs
Low garrison leads to damage and 1 VP loss from partisan attack at Colombo (29, 48)!
Low garrison leads to damage and 1 VP loss from partisan attack at Mandalay (59, 46)!
Low garrison leads to damage and 1 VP loss from partisan attack at Madurai (30, 42)!
LEADER REASSIGNMENT PHASE
AV Wright arrives at Pearl Harbor
xAK Ewa arrives at Balboa
xAKL Island Mail arrives at Balboa
xAP Largs Bay arrives at Aden
xAP Felix Roussel arrives at Cape Town
xAP Cap St Jacques arrives at Cape Town
20th PG/55th PS arrives at Ogden
20th PG/55th PS sub-groups COMBINING at Ogden
42nd BG/75th BS arrives at San Diego
42nd BG/75th BS sub-groups COMBINING at San Diego
16th RS arrives at San Diego
42nd BG/77th BS arrives at San Diego
42nd BG/76th BS arrives at San Diego
42nd BG/76th BS sub-groups COMBINING at San Diego
AVG/1st Sqn arrives at Rangoon
AVG/1st Sqn sub-groups COMBINING at Rangoon
No.62 Sqn RAF arrives at Singapore
No.62 Sqn RAF sub-groups COMBINING at Singapore
24th PG/20th PS arrives at Manila
24th PG/20th PS sub-groups COMBINING at Manila
51st PG/16th PS arrives at San Diego
51st PG/25th PS arrives at San Diego
14th PG/48th PS arrives at Ogden
14th PG/48th PS sub-groups COMBINING at Ogden
54th PG/56th PS arrives at Ogden
54th PG/56th PS sub-groups COMBINING at Ogden
55th PG/38th PS arrives at Ogden
55th PG/38th PS sub-groups COMBINING at Ogden
12th BG/82nd BS arrives at Ogden
12th BG/82nd BS sub-groups COMBINING at Ogden
No.1 Sqn RAAF arrives at Singapore
No.1 Sqn RAAF sub-groups COMBINING at Singapore
No.36 Sqn RAF Det arrives at Singapore
No.36 Sqn RAF Det sub-groups COMBINING at Singapore
1e-2-Vl.G.I arrives at Batavia
1e-2-Vl.G.I sub-groups COMBINING at Batavia
1-Vl.G.IV arrives at Batavia
1-Vl.G.IV sub-groups COMBINING at Batavia
Vk.A.1 arrives at Batavia
Vk.A.1 sub-groups COMBINING at Batavia
Vk.A.4 arrives at Batavia
69 SAD/76 IAP arrives at Vladivostok
69 SAD/76 IAP sub-groups COMBINING at Vladivostok
34 SAD/77 IAP arrives at Vladivostok
34 SAD/77 IAP sub-groups COMBINING at Vladivostok
34 SAD/59 BAP arrives at Vladivostok
34 SAD/59 BAP sub-groups COMBINING at Vladivostok
34 SAD/36 BAP arrives at Vladivostok
34 SAD/36 BAP sub-groups COMBINING at Vladivostok
34 SAD/47 IAP arrives at Vladivostok
34 SAD/47 IAP sub-groups COMBINING at Vladivostok
No.6 Sqn RAAF Det arrives at Townsville
No.6 Sqn RAAF Det sub-groups COMBINING at Townsville
No.6 Sqn RAAF arrives at Townsville
No.6 Sqn RAAF sub-groups COMBINING at Townsville
VS-1D11 arrives at San Diego
No.243 Sqn RAF Det arrives at Singapore
34 SAD/53 IAP arrives at Vladivostok
29 IAD/60 IAP arrives at Vladivostok
69th OG/31st OS arrives at Ogden
51st PG/26th PS arrives at Ogden
55th PG/37th PS arrives at Ogden
54th PG/42nd PS arrives at Ogden
14th PG/50th PS arrives at Ogden
55th PG/54th PS arrives at Ogden
14th PG/HqS arrives at Ogden
54th PG/HqS arrives at Ogden
55th PG/HqS arrives at Ogden
201st PA Construction Battalion arrives at Bataan
SEARCHING FOR ADJACENT ENEMY UNITS
REASSIGNING TRANSPORT GROUPS
No.60 Sqn RAF Det sub-groups COMBINING at Singapore
2-Vl.G.IV sub-groups COMBINING at Batavia
1-Vl.G.I sub-groups COMBINING at Soerabaja

===== section @31036 len=2 =====


===== section @31042 len=2 =====


===== section @31048 len=15160 =====
OPERATIONAL REPORT FOR Dec 08, 41

SS K XII reports having been sighted by the enemy at 54 , 78
21st Light AA Regiment packing for load on xAK Michael Embiricos (TF 325), 1 day(s) remaining
B Sqn 3rd Hussars Regiment packing for load on xAK Maldive (TF 325), 1 day(s) remaining
21st Light AA Regiment packing for load on xAK Clan Lamont (TF 325), 1 day(s) remaining
B Sqn 3rd Hussars Regiment packing for load on xAK Bhima (TF 325), 1 day(s) remaining
21st Light AA Regiment packing for load on xAK African Prince (TF 325), 1 day(s) remaining
2nd Marine Defense Battalion packing for load on APD Waters (TF 389), 1 day(s) remaining
110th USA Base Force packing for load on AP William Ward Burrows (TF 389), 1 day(s) remaining
2nd Marine Defense Battalion packing for load on AP Harris (TF 389), 1 day(s) remaining
101st USN Base Force packing for load on AK Bellatrix (TF 389), 1 day(s) remaining
I US Amphib Corps packing for load on xAP Santa Maria (TF 389), 2 day(s) remaining
101st USN Base Force packing for load on xAK Exmoor (TF 389), 1 day(s) remaining
I US Amphib Corps packing for load on xAKL Mexican (TF 389), 2 day(s) remaining
CS Convoy TF 321 unable to load any cargo at Vancouver 
CMc Pro Patria lays minefield near Palembang ( 49 , 90) and sets course for Batavia
TF 442 converted to Escort TF 491 (4 ships) and routed to Manila
DD John D. Ford sinks at sea
DD Pillsbury sinks at sea
DD Pope sinks at sea
Repairs completed on CA New Orleans at Pearl Harbor, ship returned to service
TF 441 leaving patrol zone to refuel at Melbourne
21st Light AA Regiment packing for load on xAK Michael Embiricos (TF 325), 1 day(s) remaining
B Sqn 3rd Hussars Regiment packing for load on xAK Maldive (TF 325), 1 day(s) remaining
21st Light AA Regiment packing for load on xAK Clan Lamont (TF 325), 1 day(s) remaining
B Sqn 3rd Hussars Regiment packing for load on xAK Bhima (TF 325), 1 day(s) remaining
21st Light AA Regiment packing for load on xAK African Prince (TF 325), 1 day(s) remaining
2nd Marine Defense Battalion packing for load on APD Waters (TF 389), 1 day(s) remaining
110th USA Base Force packing for load on AP William Ward Burrows (TF 389), 1 day(s) remaining
2nd Marine Defense Battalion packing for load on AP Harris (TF 389), 1 day(s) remaining
101st USN Base Force packing for load on AK Bellatrix (TF 389), 1 day(s) remaining
I US Amphib Corps packing for load on xAP Santa Maria (TF 389), 2 day(s) remaining
101st USN Base Force packing for load on xAK Exmoor (TF 389), 1 day(s) remaining
I US Amphib Corps packing for load on xAKL Mexican (TF 389), 2 day(s) remaining
xAKL Mauban observes H6K4 Mavis at 78,93 near Dadjangas
TF 349 snooped by Japanese Level Bomber at 78,93 near Dadjangas
TF 348 sights Japanese Aircraft at 79,87 near Tagbilaran
xAP Kelantan followed by Japanese Aircraft at 49,74 near Georgetown
TF 399 snooped by Japanese Aircraft at 49,74 near Georgetown
TF 400 shadowed by Japanese Float Plane at 49,74 near Georgetown
TF 447 observes Japanese Aircraft at 76,68 near Paracel Islands
TF 338 sighted by Japanese Aircraft at 50,84 near Singapore
SBD-3 Dauntless reports diving submarine at 143, 105 near Taongi
SBD-2 Dauntless sighting report: 9 Japanese ships at 166,122 near Johnston Island, speed 13, Moving West
xAK Governor Wright sights Japanese Aircraft at 79,85 near Bacolod
TF 345 followed by Japanese Level Bomber at 79,85 near Bacolod
Japanese Aircraft sighted over Kuantan 
Japanese Aircraft sighted over Clark Field 
Japanese Aircraft sighted over Clark Field 
Japanese Aircraft sighted over Clark Field 
Japanese Torpedo Bomber sighted over 21st Chinese Corps
Ki-51 Sonia sighted over 21st Chinese Corps
C5M2 Babs sighted over Batangas 
Japanese Recon sighted over 21st Chinese Corps
Ki-36 Ida sighted over Tuguegarao 
Japanese Light Bomber sighted over Tuguegarao 
Japanese Dive Bomber sighted over Tuguegarao 
Japanese Torpedo Bomber sighted over Chengchow 
Japanese Fighter Bomber sighted over Tsingsing 
Japanese Fighter sighted over 26th Group Army
Japanese Aircraft sighted over Changsha 
Ki-15-II Babs sighted over Wenchow 
Japanese Fighter Bomber sighted over Iba 
Japanese Aircraft sighted over San Fernando 
5/14th Punjab Battalion destroyed near Georgetown !!!
5/14th Punjab Battalion destroyed aboard xAP Kelantan
Ki-15-II Babs sighted over 20th Chinese Corps
Japanese Recon sighted over 20th Chinese Corps
Japanese Aircraft sighted over Hong Kong 
Japanese Aircraft sighted over Georgetown 
Ki-15-II Babs sighted over Taiping 
Japanese Aircraft sighted over Mersing 
Ki-46-II Dinah sighted over Manila 
Japanese Fighter sighted over Kuantan 
Japanese Fighter Bomber sighted over Clark Field 
C5M2 Babs sighted over Clark Field 
Japanese Fighter sighted over Clark Field 
Japanese Dive Bomber sighted over 21st Chinese Corps
Japanese Aircraft sighted over 21st Chinese Corps
C5M2 Babs sighted over Batangas 
Japanese Light Bomber sighted over 21st Chinese Corps
Japanese Light Bomber sighted over Tuguegarao 
Japanese Aircraft sighted over Chengchow 
Japanese Aircraft sighted over Tsingsing 
Japanese Aircraft sighted over 26th Group Army
Japanese Aircraft sighted over Changsha 
Ki-15-II Babs sighted over Wenchow 
Japanese Fighter Bomber sighted over Iba 
Japanese Aircraft sighted over San Fernando 
Japanese Aircraft sighted over 20th Chinese Corps
Japanese Aircraft sighted over 20th Chinese Corps
Japanese Aircraft sighted over 20th Chinese Corps
Japanese Fighter sighted over Hong Kong 
Japanese Aircraft sighted over Georgetown 
Ki-15-II Babs sighted over Taiping 
Japanese Aircraft sighted over Mersing 
Japanese Aircraft sighted over Manila 
TF 342 detects Japanese Torpedo Bomber at 75,67 near Paracel Islands
xAKL Luzon sights Japanese Float Plane at 75,88 near Zamboanga
TF 338 shadowed by Japanese Float Plane at 50,84 near Singapore
Sighting report: 9 Japanese ships at 51,75 near Kota Bharu , Speed unknown  
Japanese Aircraft sighted over Kota Bharu 
Japanese Aircraft sighted over Kuantan 
Japanese Aircraft sighted over Johore Bahru 
Japanese Dive Bomber sighted over 21st Chinese Corps
Japanese Aircraft sighted over 21st Chinese Corps
Japanese Aircraft sighted over Batangas 
Japanese Light Bomber sighted over 21st Chinese Corps
Japanese Light Bomber sighted over Tuguegarao 
Japanese Torpedo Bomber sighted over Tuguegarao 
Ki-36 Ida sighted over Tuguegarao 
Japanese Aircraft sighted over 55th Chinese Corps
Japanese Aircraft sighted over 26th Group Army
Japanese Aircraft sighted over 88th Chinese Corps
Japanese Aircraft sighted over Wenchow 
Japanese Aircraft sighted over Bataan 
Japanese Aircraft sighted over Iba 
Ki-46-I Dinah sighted over San Fernando 
Japanese Aircraft sighted over Georgetown 
Japanese Aircraft sighted over Alor Star 
Japanese Aircraft sighted over Taiping 
Japanese Recon sighted over Mandalay 
Japanese Aircraft sighted over Vigan 
Japanese Aircraft sighted over Manila 
Japanese Aircraft sighted over Kota Bharu 
Japanese Recon sighted over Kuantan 
Japanese Torpedo Bomber sighted over Johore Bahru 
Ki-51 Sonia sighted over 21st Chinese Corps
Japanese Dive Bomber sighted over 21st Chinese Corps
Japanese Recon sighted over Batangas 
Japanese Dive Bomber sighted over 21st Chinese Corps
Japanese Light Bomber sighted over Tuguegarao 
Japanese Aircraft sighted over 55th Chinese Corps
Japanese Aircraft sighted over 26th Group Army
Japanese Fighter sighted over 88th Chinese Corps
Japanese Fighter sighted over Wenchow 
Japanese Aircraft sighted over Bataan 
Japanese Aircraft sighted over Iba 
Japanese Aircraft sighted over San Fernando 
Japanese Fighter sighted over Georgetown 
Japanese Aircraft sighted over Alor Star 
Ki-15-II Babs sighted over Taiping 
Japanese Aircraft sighted over Mandalay 
Japanese Aircraft sighted over Vigan 
Ki-46-II Dinah sighted over Manila 
SBD-3 Dauntless from VS-2 crashes on landing
SBD-2 Dauntless from VB-2 is damaged on landing
SBD-2 Dauntless from VB-2 is damaged on landing
2 x PBY-4 Catalina transporting SE Borneo KNIL Battalion from Bandjermasin
7 x PBY-4 Catalina transporting SE Borneo KNIL Battalion from Bandjermasin
6 x PBY-4 Catalina transporting SE Borneo KNIL Battalion from Bandjermasin
11 x L-18 Lodestar transporting Singkawang Base Force from Singkawang
3 x Do-24K-1 transporting SSVF Brigade from Singapore
1 x Do-24K-1 transporting SE Borneo KNIL Battalion from Bandjermasin
2 x Do-24K-1 transporting Sorong Det. Base Force from Sorong
2 x Do-24K-1 transporting 224 Group RAF  from Singapore
4 x Catalina I transporting 1st Ind. Coy  from Kavieng
4 x Catalina I transporting Lark Force  from Rabaul
2 x S.23 Empire transporting Rabaul Det. Base Force from Rabaul
2 x S.23 Empire transporting Rabaul Det. Base Force from Rabaul
4 x Catalina I transporting 1st Regt Cavalerie  from Batavia
5/14th Punjab Battalion DISPERSES!!
Repairs completed on SS K VIII at Soerabaja, ship returned to service
TF 320 does not have enough fuel to complete mission
TF 323 does not have enough fuel to complete mission
TF 342 does not have enough fuel to complete mission
TF 344 does not have enough fuel to complete mission
TF 359 does not have enough fuel to complete mission
TF 360 does not have enough fuel to complete mission
TF 361 does not have enough fuel to complete mission
TF 362 does not have enough fuel to complete mission
TF 365 does not have enough fuel to complete mission
TF 375 does not have enough fuel to complete mission
TF 420 does not have enough fuel to complete mission
TF 424 does not have enough fuel to complete mission
TF 443 does not have enough fuel to complete mission
TF 444 does not have enough fuel to complete mission
TF 445 does not have enough fuel to complete mission
TF 447 does not have enough fuel to complete mission
TF 475 does not have enough fuel to complete mission
TF 479 does not have enough fuel to complete mission
Japanese forces CAPTURE Tsingsing !!!
San Francisco expands fortifications to size 2
Hilo expands fortifications to size 1
Lahaina expands fortifications to size 1
Palmyra expands fortifications to size 1
Pago Pago expands fortifications to size 1
Manila expands fortifications to size 1
Christmas Island expands fortifications to size 1
Bangalore expands fortifications to size 1
Hyderabad expands fortifications to size 1
Canberra expands fortifications to size 1
Portland expands fortifications to size 1
Wagga Wagga expands fortifications to size 1
Calgary expands fortifications to size 1
Winnipeg expands fortifications to size 1
Puckapunyal expands fortifications to size 1
69th OG/82nd OS at Fort Ord takes O-47A replacements
70th OG/123rd OS at Ogden takes O-47A replacements
7th BG/9th BS at San Francisco takes B-17D Fortress replacements
24th PG/3rd PS at Manila takes P-40E Warhawk replacements
55th PG/38th PS at Ogden takes P-43 Lancer replacements
20th PG/55th PS at Ogden takes P-40B Warhawk replacements
54th PG/56th PS at Ogden takes P-39D Airacobra replacements
54th PG/57th PS at Ogden takes P-39D Airacobra replacements
Cavite USN Base Force - some devices have been upgraded at Manila
3rd/45th PS Inf Battalion - some devices have been upgraded at Manila
21st PA Infantry Division - some devices have been upgraded at Lingayen
51st PA Infantry Division - some devices have been upgraded at Lucena
A Coy/49th Aus Battalion - some devices have been upgraded at Horn Island
A Coy/NGV Rifles  - some devices have been upgraded at Lae
B Coy/NGV Rifles  - some devices have been upgraded at Rabaul
D Coy/NGV Rifles  - some devices have been upgraded at Salamaua
2nd RAN Base Force - some devices have been upgraded at Darwin
Delayed planes for the aircraft pool:
  0 (2) x Catalina I, 1 (2) x Bolingbroke IV, 0 (1) x P-26A, 0 (2) x P-36A Mohawk, 3 (20) x P-39D Airacobra
  0 (3) x P-40B Warhawk, 3 (12) x P-43 Lancer, 4 (5) x OS2U-3 Kingfisher, 1 (10) x PBY-4 Catalina
  3 (5) x PBY-5 Catalina, 0 (1) x SNJ-3 Texan
Device(s) upgraded in 5th RAAF Base Force supplied from Esperance
Device(s) upgraded in 15th RAAF Base Force supplied from Port Moresby
Device(s) upgraded in 22nd Light Horse Regiment supplied from Hobart
Device(s) upgraded in 6th RAN Base Force supplied from Townsville
Device(s) upgraded in 16th RAAF Base Force supplied from Portland
Device(s) upgraded in 4th RAN Base Force supplied from Perth
Device(s) upgraded in 19th Australian Battalion supplied from Darwin
Device(s) upgraded in 17th RAAF Base Force supplied from Ceduna
Device(s) upgraded in DG RN Detachment  supplied from Diego Garcia
Device(s) upgraded in 31st Infantry Regiment supplied from Manila
Low garrison leads to damage and 1 VP loss from partisan attack at Colombo (29, 48)!
Low garrison leads to damage and 1 VP loss from partisan attack at Mandalay (59, 46)!
Low garrison leads to damage and 1 VP loss from partisan attack at Madurai (30, 42)!
Benjamins, P.A.C. now available for reassignment
Chan E. now available for reassignment
Marshall C. now available for reassignment
Lopatin T. now available for reassignment
AV Wright arrives at Pearl Harbor
xAK Ewa arrives at Balboa
xAKL Island Mail arrives at Balboa
Loss of xAK Governor Wright on Dec 08, 1941 is admitted
xAP Largs Bay arrives at Aden
Loss of xAK Demosthenes on Dec 08, 1941 is admitted
Loss of xAK Ellenga on Dec 08, 1941 is admitted
xAP Felix Roussel arrives at Cape Town
xAP Cap St Jacques arrives at Cape Town
Loss of xAP Hong Siang on Dec 08, 1941 is admitted
Loss of xAP Kelantan on Dec 08, 1941 is admitted
20th PG/55th PS arrives at Ogden
42nd BG/75th BS arrives at San Diego
16th RS arrives at San Diego
42nd BG/77th BS arrives at San Diego
42nd BG/76th BS arrives at San Diego
AVG/1st Sqn arrives at Rangoon
No.62 Sqn RAF arrives at Singapore
24th PG/20th PS arrives at Manila
51st PG/16th PS arrives at San Diego
51st PG/25th PS arrives at San Diego
14th PG/48th PS arrives at Ogden
54th PG/56th PS arrives at Ogden
55th PG/38th PS arrives at Ogden
12th BG/82nd BS arrives at Ogden
No.1 Sqn RAAF arrives at Singapore
No.36 Sqn RAF Det arrives at Singapore
1e-2-Vl.G.I arrives at Batavia
1-Vl.G.IV arrives at Batavia
Vk.A.1 arrives at Batavia
Vk.A.4 arrives at Batavia
69 SAD/76 IAP arrives at Vladivostok
34 SAD/77 IAP arrives at Vladivostok
34 SAD/59 BAP arrives at Vladivostok
34 SAD/36 BAP arrives at Vladivostok
34 SAD/47 IAP arrives at Vladivostok
No.6 Sqn RAAF Det arrives at Townsville
No.6 Sqn RAAF arrives at Townsville
VS-1D11 arrives at San Diego
No.243 Sqn RAF Det arrives at Singapore
34 SAD/53 IAP arrives at Vladivostok
29 IAD/60 IAP arrives at Vladivostok
69th OG/31st OS arrives at Ogden
51st PG/26th PS arrives at Ogden
55th PG/37th PS arrives at Ogden
54th PG/42nd PS arrives at Ogden
14th PG/50th PS arrives at Ogden
55th PG/54th PS arrives at Ogden
14th PG/HqS arrives at Ogden
54th PG/HqS arrives at Ogden
55th PG/HqS arrives at Ogden
201st PA Construction Battalion arrives at Bataan
VO-4/1 on AV Wright converting to size 2 from 4

===== section @46212 len=1261 =====
SIG INT REPORT FOR Dec 08, 41

Heavy Volume of Radio transmissions detected at 66,69.
Radio transmissions detected at 66,69.
Heavy Volume of Radio transmissions detected at 64,73.
1/5th Field AF Construction Battalion is loaded on a Yusen A class AK moving to Kota Bharu.
2/5th Field Artillery Regiment is loaded on a Japanese xAK moving to Kota Bharu.
Radio transmissions detected at Hailun (111,37).
Radio transmissions detected at Kota Bharu (51,75).
2/Kure 1st SNLF  is loaded on a Ansyu-C Cargo class xAK moving to Legaspi.
6th Field AF Construction Battalion is located at Saigon(60,71).
Heavy Volume of Radio transmissions detected at 94,70.
2/2nd Ind.Art.Mortar Battalion is loaded on a Japanese xAK moving to Batan Island.
Heavy Volume of Radio transmissions detected at 111,81.
1/148th Infantry Regiment is loaded on a Japanese xAKL moving to Kota Bharu.
4th JAAF AF Bn  is located at Takao(84,65).
Heavy Volume of Radio transmissions detected at Takao (84,65).
57th Infantry Regiment is located at Sunwu(113,34).
6th Ind.Hvy.Art. Battalion is located at Yenki(110,44).
1st Ind.Inf.Group  is located at Canton(77,59).
Heavy Volume of Radio transmissions detected at Takao (84,65).
37th Const Co  is located at Pescadores(84,63).
```

#### T4 `6b87` — save date 12/11/41

Complete text of the record, all 4 sections, exactly as stored (section banners added by the extractor):

```text
===== section @0 len=35467 =====
AFTER ACTION REPORTS FOR Dec 10, 41
--------------------------------------------------------------------------------
Night Time Surface Combat, near Iba at 77,70, Range 12,000 Yards
 
Japanese Ships
      DD Ikazuchi
      DD Inazuma
 
Allied Ships
      xAK Ming Sang, Torpedo hits 1, and is sunk
 
 
Maximum visibility in Clear Conditions and 57% moonlight: 12,000 yards
CONTACT: Japanese lookouts spot Allied task force at 12,000 yards
Japanese launch Long Lance torpedoes at 12,000 yards before allies detect Japanese presence
DD Ikazuchi launches Long Lance Torpedoes at xAK Ming Sang at 12,000 yards
DD Ikazuchi engages xAK Ming Sang at 12,000 yards
Combat ends with last Allied ship sunk...
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kuantan (51,79)
Defensive Guns engage approaching landing force
 
10 Coastal gun shots fired in defense.
 
Japanese Ships
      CL Sendai
      DD Ayanami
      DD Uranami
      DD Isonami
      AK Sasako Maru
 
CL Sendai firing at 22nd Indian Brigade
DD Uranami fired at enemy troops
DD Isonami fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kuantan (51,79)
Defensive Guns engage approaching landing force
 
5 Coastal gun shots fired in defense.
 
Japanese Ships
      DD Yugiri
      PB Chosa Maru
      LSD Shinshu Maru
 
PB Chosa Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kuantan (51,79)
Defensive Guns engage approaching landing force
 
7 Coastal gun shots fired in defense.
 
Japanese Ships
      AK Sagami Maru
      AK Kinka Maru
      AK Tosan Maru
 
AK Sagami Maru fired at enemy troops
AK Kinka Maru fired at enemy troops
AK Tosan Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kuantan (51,79)
Defensive Guns engage approaching landing force
 
9 Coastal gun shots fired in defense.
 
Japanese Ships
      SC Ch 8
      xAK Kyokko Maru
 
SC Ch 8 fired at enemy troops
 
 
--------------------------------------------------------------------------------
Submarine attack near Port Swettenham  at 48,80
 
Japanese Ships
      SS I-156
 
Allied Ships
      xAKL Nam Yong
 
xAKL Nam Yong is sighted by SS I-156
SS I-156 launches 2 torpedoes at xAKL Nam Yong
 
 
--------------------------------------------------------------------------------
Submarine attack near Malacca  at 48,81
 
Japanese Ships
      SS I-154
 
Allied Ships
      xAK Wing Sang, Torpedo hits 1,  heavy damage
 
xAK Wing Sang is sighted by SS I-154
SS I-154 launches 2 torpedoes at xAK Wing Sang
 
 
--------------------------------------------------------------------------------
Submarine attack near Donggala  at 68,97
 
Japanese Ships
      SS I-123
 
Allied Ships
      xAKL Legazpi,  on fire
 
xAKL Legazpi is sighted by SS I-123
SS I-123 launches 2 torpedoes
 
 
--------------------------------------------------------------------------------
Night Naval bombardment of Foochow at 86,60
 
Japanese Ships
      DD Tachikaze
      DD Yamagumo
      TB Tomozuru
      TB Hatsukari
      TB Manazuru
      TB Chidori
 
Allied ground losses:
      103 casualties reported
         Squads: 0 destroyed, 5 disabled
         Non Combat: 0 destroyed, 8 disabled
         Engineers: 0 destroyed, 1 disabled
 
Airbase hits 6
Airbase supply hits 6
Runway hits 26
Port hits 10
Port fuel hits 2
Port supply hits 3
 
DD Tachikaze firing at 86th Chinese Corps
DD Yamagumo firing at Foochow 
TB Tomozuru firing at Foochow 
TB Hatsukari firing at Foochow 
TB Manazuru firing at 86th Chinese Corps
TB Chidori firing at 86th Chinese Corps
 
 
--------------------------------------------------------------------------------
Sub attack near Kota Bharu  at 53,75
 
Japanese Ships
      DD Sagiri
 
Allied Ships
      SS O 20, hits 1
 
SS O 20 launches 4 torpedoes at DD Sagiri
DD Sagiri attacking submerged sub ....
DD Sagiri attacking submerged sub ....
DD Sagiri fails to find sub, continues to search...
DD Sagiri fails to find sub, continues to search...
DD Sagiri fails to find sub, continues to search...
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
Pre-Invasion action off Kota Bharu (51,75)
 
28 Coastal gun shots fired in defense.
 
Japanese Ships
      CL Kashii
      PB Fuji Maru #3
      PB Shonon Maru #10
      PB Shuko Maru #2
      PB Sozan Maru
      PB Eifuku Maru
      xAP Manzyu Maru
 
Allied ground losses:
      122 casualties reported
         Squads: 0 destroyed, 5 disabled
         Non Combat: 1 destroyed, 11 disabled
         Engineers: 0 destroyed, 1 disabled
      Guns lost 2 (1 destroyed, 1 disabled)
 
CL Kashii firing at 8th Indian Brigade
PB Fuji Maru #3 fired at enemy troops
PB Shonon Maru #10 fired at enemy troops
PB Shuko Maru #2 fired at enemy troops
PB Sozan Maru fired at enemy troops
PB Eifuku Maru fired at enemy troops
Defensive Guns fire at approaching troops in landing craft at 4,000 yards
Defensive Guns fire at approaching troops in landing craft at 1,000 yards
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Kota Bharu (51,75)
 
TF 44 troops unloading over beach at Kota Bharu, 51,75
 
12 Support troops lost in surf during unload of 36th JAAF AF Bn /4
12 Aviation Support troops lost in surf during unload of 27th JAAF AF Bn /3
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kuantan (51,79)
Defensive Guns engage approaching landing force
 
8 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Chosa Maru
      LSD Shinshu Maru
 
PB Chosa Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kuantan (51,79)
Defensive Guns engage approaching landing force
 
7 Coastal gun shots fired in defense.
 
Japanese Ships
      AK Sagami Maru
      AK Kinka Maru
      AK Tosan Maru
 
AK Sagami Maru fired at enemy troops
AK Kinka Maru fired at enemy troops
AK Tosan Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kuantan (51,79)
Defensive Guns engage approaching landing force
 
9 Coastal gun shots fired in defense.
 
Japanese Ships
      SC Ch 8
      xAK Kyokko Maru
 
SC Ch 8 fired at enemy troops
 
 
--------------------------------------------------------------------------------
Pre-Invasion action off Kuantan (51,79)
Defensive Guns engage approaching landing force
 
14 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Eiko Maru
      xAK Nojima Maru
 
PB Eiko Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Kuantan (51,79)
 
TF 98 troops unloading over beach at Kuantan, 51,79
 
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kota Bharu (51,75)
Defensive Guns engage approaching landing force
 
14 Coastal gun shots fired in defense.
 
Japanese Ships
      CL Kashii
      PB Fuji Maru #3
      PB Shonon Maru #10
      PB Shuko Maru #2
      PB Sozan Maru
      PB Eifuku Maru
      xAP Manzyu Maru
 
Allied ground losses:
      22 casualties reported
         Squads: 0 destroyed, 1 disabled
         Non Combat: 0 destroyed, 1 disabled
         Engineers: 0 destroyed, 1 disabled
 
PB Fuji Maru #3 fired at enemy troops
PB Shonon Maru #10 fired at enemy troops
PB Shuko Maru #2 fired at enemy troops
PB Sozan Maru fired at enemy troops
PB Eifuku Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kuantan (51,79)
Defensive Guns engage approaching landing force
 
6 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Chosa Maru
      LSD Shinshu Maru
 
PB Chosa Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kuantan (51,79)
Defensive Guns engage approaching landing force
 
14 Coastal gun shots fired in defense.
 
Japanese Ships
      SC Ch 8
      xAK Kyokko Maru
 
SC Ch 8 fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kuantan (51,79)
Defensive Guns engage approaching landing force
 
5 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Eiko Maru
      xAK Nojima Maru
 
PB Eiko Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Guam (106,95)
Defensive Guns engage approaching landing force
 
1 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Kohuku Maru
      PB Hirotama Maru
      xAK Shofuku Maru
 
PB Kohuku Maru fired at enemy troops
PB Hirotama Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Submarine attack near Toboali  at 51,93
 
Japanese Ships
      SS I-153
 
Allied Ships
      xAK Toendjoek, Torpedo hits 1,  heavy damage
 
xAK Toendjoek is sighted by SS I-153
SS I-153 launches 2 torpedoes at xAK Toendjoek
 
 
--------------------------------------------------------------------------------
Submarine attack near Toboali  at 51,93
 
Japanese Ships
      SS I-153
 
Allied Ships
      xAK Somelsdijk, Torpedo hits 1,  on fire,  heavy damage
 
xAK Somelsdijk is sighted by SS I-153
SS I-153 launches 2 torpedoes at xAK Somelsdijk
 
 
--------------------------------------------------------------------------------
Submarine attack near Singapore  at 49,85
 
Japanese Ships
      SS I-121
 
Allied Ships
      xAKL Nam Yong, Shell hits 3,  heavy fires,  heavy damage
 
xAKL Nam Yong is sighted by SS I-121
SS I-121 attacking on the surface
 
 
--------------------------------------------------------------------------------
Morning Air attack on Rangoon , at 54,53 
 
Weather in hex: Light rain
 
Raid spotted at 35 NM, estimated altitude 23,000 feet.
Estimated time to target is 11 minutes
 
Japanese aircraft
      A6M2 Zero x 25
 
No Japanese losses
 
Aircraft Attacking:
      25 x A6M2 Zero sweeping at 20000 feet
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on 58th Chinese Corps, at 84,52 , near Wuchang
 
Weather in hex: Light cloud
 
Raid spotted at 20 NM, estimated altitude 12,000 feet.
Estimated time to target is 5 minutes
 
Japanese aircraft
      Ki-27b Nate x 11
      Ki-30 Ann x 9
 
No Japanese losses
 
Allied ground losses:
      9 casualties reported
         Squads: 0 destroyed, 1 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
Aircraft Attacking:
       9 x Ki-30 Ann bombing from 9000 feet
               Ground Attack:  1 x 250 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on 6th Indian Brigade, at 49,73 (Alor Star)
 
Weather in hex: Partial cloud
 
Raid spotted at 19 NM, estimated altitude 6,000 feet.
Estimated time to target is 5 minutes
 
Japanese aircraft
      Ki-27b Nate x 5
      Ki-30 Ann x 18
 
No Japanese losses
 
Allied ground losses:
      8 casualties reported
         Squads: 0 destroyed, 0 disabled
         Non Combat: 0 destroyed, 1 disabled
         Engineers: 0 destroyed, 0 disabled
 
Aircraft Attacking:
      18 x Ki-30 Ann bombing from 6000 feet
               Ground Attack:  1 x 250 kg GP Bomb
 
Also attacking 15th Indian Brigade ...
Also attacking 6th Indian Brigade ...
 
 
--------------------------------------------------------------------------------
Morning Air attack on 52nd Chinese Corps, at 70,56 (Lang Son)
 
Weather in hex: Overcast
 
Raid spotted at 38 NM, estimated altitude 8,000 feet.
Estimated time to target is 13 minutes
 
Japanese aircraft
      Ki-51 Sonia x 6
 
No Japanese losses
 
Aircraft Attacking:
       6 x Ki-51 Sonia bombing from 6000 feet
               Ground Attack:  4 x 50 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on 21st Chinese Corps, at 85,55 , near Nanchang
 
Weather in hex: Severe storms
 
Raid spotted at 45 NM, estimated altitude 8,000 feet.
Estimated time to target is 15 minutes
 
Japanese aircraft
      Ki-51 Sonia x 12
 
No Japanese losses
 
Allied ground losses:
      8 casualties reported
         Squads: 0 destroyed, 0 disabled
         Non Combat: 0 destroyed, 1 disabled
         Engineers: 0 destroyed, 0 disabled
 
Aircraft Attacking:
      12 x Ki-51 Sonia bombing from 6000 feet
               Ground Attack:  4 x 50 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on Rangoon , at 54,53 
 
Weather in hex: Light rain
 
Raid spotted at 10 NM, estimated altitude 14,000 feet.
Estimated time to target is 2 minutes
 
Japanese aircraft
      Ki-21-IIa Sally x 29
      Ki-43-Ib Oscar x 23
 
No Japanese losses
 
Airbase hits 5
Airbase supply hits 2
Runway hits 24
 
Aircraft Attacking:
      29 x Ki-21-IIa Sally bombing from 10000 feet
               Airfield Attack:  4 x 250 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on Rangoon , at 54,53 
 
Weather in hex: Light rain
 
Raid spotted at 20 NM, estimated altitude 22,000 feet.
Estimated time to target is 6 minutes
 
Japanese aircraft
      A6M2 Zero x 3
 
No Japanese losses
 
Aircraft Attacking:
       3 x A6M2 Zero sweeping at 20000 feet
 
 
 
--------------------------------------------------------------------------------
Afternoon Air attack on TF, near Bataan at 78,77
 
Weather in hex: Overcast
 
Raid detected at 117 NM, estimated altitude 17,000 feet.
Estimated time to target is 46 minutes
 
Japanese aircraft
      A6M2 Zero x 18
      G3M2 Nell x 13
 
Allied aircraft
      P-40B Warhawk x 6
      P-40E Warhawk x 22
 
Japanese aircraft losses
      A6M2 Zero: 2 destroyed
      G3M2 Nell: 4 destroyed
 
Allied aircraft losses
      P-40B Warhawk: 1 destroyed
      P-40E Warhawk: 1 destroyed
 
Allied Ships
      xAK Haraldsvang, Torpedo hits 2, and is sunk
      xAK Fatshan, Torpedo hits 2, and is sunk
 
Aircraft Attacking:
       6 x G3M2 Nell launching torpedoes at 200 feet
               Naval Attack:  1 x 18in Type 91 Torpedo
 
CAP engaged:
24th PG/3rd PS with P-40E Warhawk (3 airborne, 0 on standby, 0 scrambling)
      (11 plane(s) diverted to support CAP in hex.)
      3 plane(s) intercepting now.
      0 plane(s) not yet engaged, 8 being recalled, 0 out of immediate contact.
      Group patrol altitude is 20000 , scrambling fighters between 0 and 20000.
      Time for all group planes to reach interception is 18 minutes
      8 planes vectored on to bombers
24th PG/17th PS with P-40E Warhawk (0 airborne, 0 on standby, 0 scrambling)
      (5 plane(s) diverted to support CAP in hex.)
      0 plane(s) not yet engaged, 4 being recalled, 1 out of immediate contact.
      Group patrol altitude is 10000 , scrambling fighters between 0 and 10000.
      Time for all group planes to reach interception is 46 minutes
      3 planes vectored on to bombers
24th PG/20th PS with P-40B Warhawk (0 airborne, 0 on standby, 0 scrambling)
      (6 plane(s) diverted to support CAP in hex.)
      0 plane(s) not yet engaged, 2 being recalled, 4 out of immediate contact.
      Group patrol altitude is 10000 , scrambling fighters between 0 and 10000.
      Time for all group planes to reach interception is 48 minutes
      3 planes vectored on to bombers
35th PG/21st PS with P-40E Warhawk (0 airborne, 0 on standby, 0 scrambling)
      (6 plane(s) diverted to support CAP in hex.)
      0 plane(s) not yet engaged, 6 being recalled, 0 out of immediate contact.
      Group patrol altitude is 10000 , scrambling fighters between 0 and 10000.
      Time for all group planes to reach interception is 17 minutes
 
 
 
--------------------------------------------------------------------------------
Afternoon Air attack on 88th Chinese Corps, at 90,56 , near Hangchow
 
Weather in hex: Severe storms
 
Raid spotted at 10 NM, estimated altitude 9,000 feet.
Estimated time to target is 3 minutes
 
Japanese aircraft
      B5M1 Mabel x 5
 
No Japanese losses
 
Allied ground losses:
      9 casualties reported
         Squads: 0 destroyed, 1 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
Aircraft Attacking:
       5 x B5M1 Mabel bombing from 6000 feet
               Ground Attack:  2 x 250 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Afternoon Air attack on 3rd Group Army, at 87,48 , near Sinyang
 
Weather in hex: Heavy cloud
 
Raid spotted at 20 NM, estimated altitude 12,000 feet.
Estimated time to target is 6 minutes
 
Japanese aircraft
      Ki-27b Nate x 8
      Ki-51 Sonia x 13
 
No Japanese losses
 
Aircraft Attacking:
      13 x Ki-51 Sonia bombing from 8000 feet
               Ground Attack:  4 x 50 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Afternoon Air attack on 48th Chinese Corps, at 88,50 , near Hwainan
 
Weather in hex: Thunderstorms
 
Raid spotted at 28 NM, estimated altitude 13,000 feet.
Estimated time to target is 13 minutes
 
Japanese aircraft
      Ki-36 Ida x 15
 
No Japanese losses
 
Allied ground losses:
      8 casualties reported
         Squads: 0 destroyed, 0 disabled
         Non Combat: 0 destroyed, 1 disabled
         Engineers: 0 destroyed, 0 disabled
 
Aircraft Attacking:
      15 x Ki-36 Ida bombing from 10000 feet
               Ground Attack:  4 x 30 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Sub attack near Toboali  at 51,93
 
Japanese Ships
      SS I-153
 
Allied Ships
      xAK Toendjoek, Torpedo hits 2,  on fire,  heavy damage
 
SS I-153 launches 2 torpedoes
 
 
--------------------------------------------------------------------------------
Submarine attack near Singapore  at 49,85
 
Japanese Ships
      SS I-121
 
Allied Ships
      AMC Manoora, Torpedo hits 1
 
AMC Manoora is sighted by SS I-121
SS I-121 launches 2 torpedoes
 
 
--------------------------------------------------------------------------------
Submarine attack near Singapore  at 49,85
 
Japanese Ships
      SS I-121
 
Allied Ships
      AMC Manoora, Torpedo hits 2,  on fire,  heavy damage
 
AMC Manoora is sighted by SS I-121
SS I-121 launches 2 torpedoes
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kota Bharu (51,75)
Defensive Guns engage approaching landing force
 
6 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Fuji Maru #3
      PB Shonon Maru #10
      PB Shuko Maru #2
      PB Sozan Maru
      PB Eifuku Maru
      xAP Manzyu Maru
 
PB Fuji Maru #3 fired at enemy troops
PB Shonon Maru #10 fired at enemy troops
PB Shuko Maru #2 fired at enemy troops
PB Sozan Maru fired at enemy troops
PB Eifuku Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Aparri (82,73)
 
TF 90 troops unloading over beach at Aparri, 82,73
 
Japanese ground losses:
      68 casualties reported
         Squads: 3 destroyed, 0 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
17 troops of a SNLF Squad lost in surf during unload of Sasebo 1st SNLF /1
17 troops of a SNLF Squad lost from landing craft during unload of Sasebo 1st SNLF /1
17 troops of a SNLF Squad accidentally lost during unload of Sasebo 1st SNLF /5
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kuantan (51,79)
Defensive Guns engage approaching landing force
 
7 Coastal gun shots fired in defense.
 
Japanese Ships
      DMS W-6
      DMS W-5
      DMS W-1
      xAK Kyokko Maru
 
DMS W-6 fired at enemy troops
DMS W-5 fired at enemy troops
DMS W-1 fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Kuantan (51,79)
Defensive Guns engage approaching landing force
 
6 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Eiko Maru
      xAK Nojima Maru
 
PB Eiko Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Pre-Invasion action off Cagayan (79,89)
Defensive Guns engage approaching landing force
 
9 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Myoken Maru
      xAK Yamafuku Maru
 
Allied ground losses:
      32 casualties reported
         Squads: 0 destroyed, 2 disabled
         Non Combat: 0 destroyed, 2 disabled
         Engineers: 0 destroyed, 0 disabled
 
PB Myoken Maru fired at enemy troops
Defensive Guns fire at approaching troops in landing craft at 5,000 yards
Defensive Guns fire at approaching troops in landing craft at 1,000 yards
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Cagayan (79,89)
 
TF 110 troops unloading over beach at Cagayan, 79,89
 
Japanese ground losses:
      48 casualties reported
         Squads: 2 destroyed, 0 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
17 troops of a SNLF Squad accidentally lost during unload of Kure 1st SNLF 
19 troops of a IJA Infantry Sqd (H) lost from landing craft during unload of Kimura Det /2
 
 
--------------------------------------------------------------------------------
Amphibious Assault at San Fernando (80,74)
 
TF 115 troops unloading over beach at San Fernando, 80,74
 
Japanese ground losses:
      91 casualties reported
         Squads: 4 destroyed, 0 disabled
         Non Combat: 4 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
17 troops of a IJA Infantry Squad lost overboard during unload of 65th Bde /2
17 troops of a IJA Infantry Squad lost from landing craft during unload of 65th Bde /2
17 troops of a IJA Infantry Squad accidentally lost during unload of 65th Bde /3
12 Engineers lost overboard during unload of 65th Bde
12 Support troops lost in surf during unload of 65th Bde /5
17 troops of a IJA Infantry Squad lost overboard during unload of 65th Bde /6
12 Support troops lost overboard during unload of 65th Bde /8
 
 
--------------------------------------------------------------------------------
Amphibious Assault at San Fernando (80,74)
 
TF 117 troops unloading over beach at San Fernando, 80,74
 
12 Support troops lost in surf during unload of 23rd Air Flotilla 
12 Support troops lost in surf during unload of 5th Naval Const Bn /1
12 Support troops accidentally lost during unload of 28th JAAF AF Bn 
 
 
--------------------------------------------------------------------------------
Pre-Invasion action off Guam (106,95)
Defensive Guns engage approaching landing force
 
3 Coastal gun shots fired in defense.
 
Japanese Ships
      xAKL Ujigawa Maru
      xAKL Heiwa Maru
      xAK Nittai Maru
      xAK Shinkoku Maru
 
xAKL Ujigawa Maru firing at Guam USN Base Force
xAKL Ujigawa Maru fired at enemy troops
xAKL Heiwa Maru fired at enemy troops
xAK Nittai Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Guam (106,95)
 
TF 129 troops unloading over beach at Guam, 106,95
 
12 Support troops accidentally lost during unload of II/81st Nav Gd /2
 
 
--------------------------------------------------------------------------------
 
Ground combat at Kuantan (51,79)
 
Japanese Deliberate attack
 
Attacking force 8360 troops, 93 guns, 137 vehicles, Assault Value = 224
 
Defending force 3314 troops, 42 guns, 37 vehicles, Assault Value = 99
 
Japanese adjusted assault: 165 
 
Allied adjusted defense: 29 
 
Japanese assault odds: 5 to 1 (fort level 2) 
 
Japanese forces CAPTURE Kuantan !!!
 
Allied aircraft
      no flights
 
Allied aircraft losses
      Blenheim I: 1 destroyed
 
Combat modifiers
Defender: terrain(+), forts(+), leaders(-), preparation(-)
 fatigue(-), morale(-), experience(-)
Attacker: 
 
Japanese ground losses:
      175 casualties reported
         Squads: 5 destroyed, 13 disabled
         Non Combat: 0 destroyed, 1 disabled
         Engineers: 0 destroyed, 0 disabled
      Units destroyed 1
 
Allied ground losses:
      1650 casualties reported
         Squads: 31 destroyed, 8 disabled
         Non Combat: 68 destroyed, 0 disabled
         Engineers: 30 destroyed, 0 disabled
      Guns lost 33 (33 destroyed, 0 disabled)
      Vehicles lost 36 (36 destroyed, 0 disabled)
      Units retreated 2
      Units destroyed 1
 
Assaulting units:
    42nd Infantry Regiment
    5th Recon Regiment
    1st Tank Regiment
    11th Infantry Regiment
    41st Infantry Regiment
    20th AA Regiment
    5th Engr Rgt /3
    5th FA Rgt /3
    25th Army /4
    84th JAAF AF Bn /2
    5th Field AF Const Bn /2
 
Defending units:
    22nd Indian Brigade
    3rd HK&S Light AA Regiment
    1st ISF Base Force
 
 
--------------------------------------------------------------------------------
 
Ground combat at 83,51 (near Wuchang)
 
Japanese Shock attack
 
Attacking force 12572 troops, 122 guns, 0 vehicles, Assault Value = 930
 
Defending force 8278 troops, 38 guns, 0 vehicles, Assault Value = 235
 
Japanese adjusted assault: 406 
 
Allied adjusted defense: 51 
 
Japanese assault odds: 7 to 1 
 
Combat modifiers
Defender: fatigue(-), morale(-), experience(-)
Attacker: shock(+)
 
Japanese ground losses:
      2041 casualties reported
         Squads: 2 destroyed, 164 disabled
         Non Combat: 0 destroyed, 15 disabled
         Engineers: 0 destroyed, 14 disabled
      Units pursuing 2
 
Allied ground losses:
      3947 casualties reported
         Squads: 119 destroyed, 22 disabled
         Non Combat: 118 destroyed, 0 disabled
         Engineers: 4 destroyed, 5 disabled
      Guns lost 7 (5 destroyed, 2 disabled)
      Units retreated 1
 
Defeated Allied Units Retreating!
 
Assaulting units:
    6th Division
    3rd Division
    18th Ind.Mixed Brigade
 
Defending units:
    20th Chinese Corps
 
 
--------------------------------------------------------------------------------
 
Ground combat at Hong Kong (77,61)
 
Japanese Deliberate attack
 
Attacking force 22417 troops, 321 guns, 218 vehicles, Assault Value = 630
 
Defending force 7301 troops, 132 guns, 80 vehicles, Assault Value = 228
 
Japanese engineers reduce fortifications to 2
 
Japanese adjusted assault: 303 
 
Allied adjusted defense: 269 
 
Japanese assault odds: 1 to 1 (fort level 2) 
 
Japanese Assault reduces fortifications to 2
 
Combat modifiers
Defender: terrain(+), experience(-)
Attacker: 
 
Japanese ground losses:
      862 casualties reported
         Squads: 1 destroyed, 65 disabled
         Non Combat: 0 destroyed, 9 disabled
         Engineers: 1 destroyed, 40 disabled
 
Allied ground losses:
      245 casualties reported
         Squads: 7 destroyed, 58 disabled
         Non Combat: 0 destroyed, 13 disabled
         Engineers: 1 destroyed, 1 disabled
      Guns lost 3 (1 destroyed, 2 disabled)
 
Assaulting units:
    19th Ind. Engineer Regiment
    66th Infantry Regiment
    38th Division
    20th Ind. Engineer Regiment
    3rd Ind.Hvy.Art. Battalion
    10th Ind. Mountain Gun Regiment
    2nd Mortar Battalion
    5th RF Gun Battalion
    2nd Ind.Hvy.Art. Battalion
    1st Hvy.Artillery Regiment
    2nd RF Gun Battalion
    20th Ind. Mtn Gun Battalion
 
Defending units:
    Rifles of Canada Battalion
    1st Middlesex Battalion
    Winnipeg Grenadiers Battalion
    Kowloon Brigade
    102nd RN Base Force
    Hong Kong Fortress
 
 
--------------------------------------------------------------------------------
 
Ground combat at 88,43 (near Kaifeng)
 
Japanese Deliberate attack
 
Attacking force 8760 troops, 55 guns, 0 vehicles, Assault Value = 322
 
Defending force 4949 troops, 43 guns, 0 vehicles, Assault Value = 170
 
Japanese adjusted assault: 210 
 
Allied adjusted defense: 53 
 
Japanese assault odds: 3 to 1 
 
Combat modifiers
Defender: terrain(+), op mode(-), leaders(+), fatigue(-), morale(-)
 experience(-)
Attacker: 
 
Japanese ground losses:
      371 casualties reported
         Squads: 4 destroyed, 40 disabled
         Non Combat: 0 destroyed, 2 disabled
         Engineers: 0 destroyed, 0 disabled
      Guns lost 3 (1 destroyed, 2 disabled)
 
Allied ground losses:
      2414 casualties reported
         Squads: 50 destroyed, 15 disabled
         Non Combat: 51 destroyed, 0 disabled
         Engineers: 4 destroyed, 3 disabled
      Guns lost 5 (2 destroyed, 3 disabled)
      Units retreated 1
 
Defeated Allied Units Retreating!
 
Assaulting units:
    17th RGC Temp. Division
    3rd Ind.Mixed Brigade
    16th RGC Temp. Division
    24th NCPC Route Brigade
 
Defending units:
    98th Chinese Corps
 
 
--------------------------------------------------------------------------------
 
Ground combat at 87,48 (near Sinyang)
 
Japanese Deliberate attack
 
Attacking force 6643 troops, 57 guns, 11 vehicles, Assault Value = 224
 
Defending force 5213 troops, 43 guns, 0 vehicles, Assault Value = 117
 
Japanese adjusted assault: 82 
 
Allied adjusted defense: 19 
 
Japanese assault odds: 4 to 1 
 
Combat modifiers
Defender: leaders(-), fatigue(-), experience(-)
Attacker: morale(-)
 
Japanese ground losses:
      373 casualties reported
         Squads: 1 destroyed, 51 disabled
         Non Combat: 0 destroyed, 4 disabled
         Engineers: 0 destroyed, 1 disabled
 
Allied ground losses:
      1705 casualties reported
         Squads: 96 destroyed, 0 disabled
         Non Combat: 85 destroyed, 0 disabled
         Engineers: 2 destroyed, 2 disabled
      Guns lost 7 (7 destroyed, 0 disabled)
      Units retreated 2
 
Defeated Allied Units Retreating!
 
Assaulting units:
    1st Ind.Mixed Brigade
    18th RGC Temp. Division
 
Defending units:
    12th Chinese Corps
    3rd Group Army
 
 
--------------------------------------------------------------------------------
 
Ground combat at Kota Bharu (51,75)
 
Japanese Shock attack
 
Attacking force 3727 troops, 38 guns, 0 vehicles, Assault Value = 197
 
Defending force 3671 troops, 39 guns, 2 vehicles, Assault Value = 69
 
Japanese adjusted assault: 188 
 
Allied adjusted defense: 11 
 
Japanese assault odds: 17 to 1 (fort level 1) 
 
Japanese forces CAPTURE Kota Bharu !!!
 
Combat modifiers
Defender: leaders(-), preparation(-), morale(-), experience(-)
Attacker: shock(+)
 
Japanese ground losses:
      40 casualties reported
         Squads: 0 destroyed, 5 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
      Units pursuing 1
 
Allied ground losses:
      3143 casualties reported
         Squads: 184 destroyed, 0 disabled
         Non Combat: 226 destroyed, 0 disabled
         Engineers: 32 destroyed, 0 disabled
      Guns lost 38 (38 destroyed, 0 disabled)
      Vehicles lost 2 (2 destroyed, 0 disabled)
      Units retreated 2
      Units destroyed 1
 
Defeated Allied Units Retreating!
 
Assaulting units:
    56th Infantry Regiment
    12th Engineer Regiment
    36th JAAF AF Bn 
    35th JAAF AF Bn 
    3rd Air Division
    27th JAAF AF Bn 
    5th JAAF AF Coy 
 
Defending units:
    FMSV Brigade
    8th Indian Brigade
    3rd ISF Base Force
 
 
--------------------------------------------------------------------------------
 
Ground combat at Guam (106,95)
 
Japanese Deliberate attack
 
Attacking force 4398 troops, 18 guns, 0 vehicles, Assault Value = 53
 
Defending force 894 troops, 0 guns, 32 vehicles, Assault Value = 9
 
Japanese adjusted assault: 29 
 
Allied adjusted defense: 21 
 
Japanese assault odds: 1 to 1 (fort level 1) 
 
Japanese Assault reduces fortifications to 0
 
Combat modifiers
Defender: terrain(+), leaders(+), leaders(-), experience(-)
Attacker: 
 
Assaulting units:
    II/81st Naval Guard Unit
    5th Base Force
 
Defending units:
    Guam USN Base Force
 
 
--------------------------------------------------------------------------------
 
Ground combat at Hong Kong (77,61)
 
Allied Bombardment attack
 
Attacking force 954 troops, 74 guns, 0 vehicles, Assault Value = 177
 
Defending force 21762 troops, 321 guns, 218 vehicles, Assault Value = 577
 
Assaulting units:
    1st Middlesex Battalion
    Rifles of Canada Battalion
    Winnipeg Grenadiers Battalion
    Kowloon Brigade
    102nd RN Base Force
    Hong Kong Fortress
 
Defending units:
    66th Infantry Regiment
    38th Division
    19th Ind. Engineer Regiment
    20th Ind. Engineer Regiment
    2nd Ind.Hvy.Art. Battalion
    2nd RF Gun Battalion
    5th RF Gun Battalion
    3rd Ind.Hvy.Art. Battalion
    2nd Mortar Battalion
    1st Hvy.Artillery Regiment
    10th Ind. Mountain Gun Regiment
    20th Ind. Mtn Gun Battalion
 
 
--------------------------------------------------------------------------------
 
Ground combat at Lashio (62,46)
 
Japanese Shock attack
 
Attacking force 579 troops, 8 guns, 0 vehicles, Assault Value = 44
 
Defending force 486 troops, 5 guns, 0 vehicles, Assault Value = 4
 
Japanese adjusted assault: 4 
 
Allied adjusted defense: 4 
 
Japanese assault odds: 1 to 1 (fort level 0) 
 
Japanese Assault reduces fortifications to 0
 
Combat modifiers
Defender: terrain(+), op mode(-), preparation(-), morale(-)
 experience(-)
Attacker: op mode(-), shock(+)
 
Japanese ground losses:
      16 casualties reported
         Squads: 0 destroyed, 2 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
Assaulting units:
    Yokosuka 3rd SNLF /2
    Yokosuka 1st SNLF /1
 
Defending units:
    102nd RAF Base Force
 
 

===== section @35471 len=8061 =====
COMBAT EVENTS FOR 12/10/41

ADJUST TASK FORCE MISSIONS 
CALCULATE RANGE TO ENEMY
CALCULATE AIR SUPERIORITY
ASSIGN COMPUTER CONTROLLED UNITS
Bukit Besi is occupied by the Japanese
Combat Events:
NAVAL REACTION PHASE
Task Force 314 encounters enemy Task Force at 77 , 70
xAK Ming Sang sinks....
LOADING/UNLOADING TASK FORCES
Amphibious TF 85 unloading at Kuantan
Amphibious TF 91 unloading at Kuantan
Amphibious TF 92 unloading at Kuantan
Amphibious TF 97 unloading at Kuantan
TASK FORCE LOADING/UNLOADING COMPLETE
CALCULATING TASK FORCE PATHS
MINESWEEPING OPERATIONS
NAVAL MOVEMENT PHASE
xAK Wing Sang sinks....
Task Force 336 has refueled TF 492 at 70 , 94 and is returning to Balikpapan
ADJUST SHIP FUEL
Japanese Ships Bombarding Foochow
NIGHT AIR OPERATIONS PHASE
AIR NIGHT STRIKES
AIRCRAFT LANDING
SHIP CREWS PERFORM DAMAGE CONTROL
SURFACE COMBAT CHECK
DIVIDE CRIPPLED TFs
SHIP CREWS PERFORM DAMAGE CONTROL
SHIP CREWS PERFORM DAMAGE CONTROL
Amphibious TF 44 offshore of Kota Bharu
Amphibious TF 85 unloading at Kuantan
Amphibious TF 91 unloading at Kuantan
Amphibious TF 92 unloading at Kuantan
Amphibious TF 97 unloading at Kuantan
Amphibious TF 98 offshore of Kuantan
REPAIRING SHIPS
END 12 HOUR PULSE
DIVIDE CRIPPLED TFs
CHECKING TF FUEL
TF'S RETURN TO PORT
Combat Events:
NAVAL REACTION PHASE
LOADING/UNLOADING TASK FORCES
Amphibious TF 44 unloading at Kota Bharu
Amphibious TF 85 unloading at Kuantan
Amphibious TF 91 unloading at Kuantan
Amphibious TF 92 unloading at Kuantan
Amphibious TF 97 unloading at Kuantan
Amphibious TF 98 unloading at Kuantan
Amphibious TF 128 unloading at Guam
TASK FORCE LOADING/UNLOADING COMPLETE
CALCULATING TASK FORCE PATHS
MINESWEEPING OPERATIONS
NAVAL MOVEMENT PHASE
xAK Somelsdijk sinks....
xAKL Nam Yong sinks....
ADJUST SHIP FUEL
AIR OPERATIONS PHASE : AM
TF 344 detected by Japanese Torpedo Bomber at 73,71 near Paracel Islands
TF 447 observes Japanese Recon at 78,77 near Bataan
TF 348 followed by Japanese Recon at 70,94 near Tandjoengselor
TF 492 sights Japanese Aircraft at 68,92 near Tandjoengselor
DD Peary sighted by Japanese Aircraft at 69,90 near Tawau
TF 301 shadowed by Japanese Float Plane at 45,72 near Langsa
TF 341 sights Japanese Patrol at 52,84 near Tioman Island
Catalina I sighting report: 10 Japanese ships at 51,79 near Kuantan , Speed unknown 
TF 342 observes Japanese Float Plane at 77,78 near Lubang
xAP Rochambeau snooped by Japanese Float Fighter at 71,88 near Sandakan
TK Gertrude Kellogg sighted by E13A1 Jake at 71,88 near Sandakan
TF 341 shadowed by Japanese Fighter Bomber at 52,84 near Tioman Island
TF 301 sighted by Japanese Recon at 45,72 near Langsa
SS K XIV shadowed by Japanese Torpedo Bomber at 53,80 near Kuantan
MTB 11 sights Japanese Torpedo Bomber at 73,71 near Paracel Islands
TF 443 observes Japanese Float Plane at 76,77 near Bataan
AMC Manoora snooped by E13A1 Jake at 49,85 near Singapore
TF 444 detects E7K2 Alf at 76,77 near Bataan
TF 514 sighted by Japanese Float Plane at 177,120 near Kona
Do-24K-1 sighting report: 7 Japanese ships at 51,79 near Kuantan , Speed unknown 
Blenheim I sighting report: 3 Japanese ships at 51,79 near Kuantan , Speed 3 , Moving West
Blenheim I sighting report: 9 Japanese ships at 51,79 near Kuantan , Speed unknown 
Blenheim I sighting report: 8 Japanese ships at 51,79 near Kuantan , Speed 7 , Moving West
Blenheim I sighting report: 9 Japanese ships at 51,79 near Kuantan , Speed unknown 
TF 388 followed by Japanese Recon at 69,91 near Tawau
H6K4 Mavis has spotted a Pacific S Cargo class xAKL at 70,94
AIR STRIKES
AIRCRAFT LANDING
SHIP CREWS PERFORM DAMAGE CONTROL
DIVIDE CRIPPLED TFs
AIR OPERATIONS PHASE : PM
TF 435 sights Japanese Dive Bomber at 72,75 near Iba
D3A1 Val attacking a O16 Class class SS at 158,103
PBY-5 Catalina sighting report: 9 Japanese ships at 186,109 near Hilo, speed 20, Moving East
139WH-3 reports radio transmissions at 49, 97 near Batavia
Do-24K-1 sighting report: 9 Japanese ships at 51,79 near Kuantan , Speed unknown 
Do-24K-1 sighting report: 5 Japanese ships at 51,79 near Kuantan , Speed unknown 
Blenheim I sighting report: 9 Japanese ships at 51,79 near Kuantan , Speed unknown 
AIR STRIKES
xAK Fatshan sinks....
xAK Haraldsvang sinks....
AIRCRAFT LANDING
AIR TRANSPORT PHASE
:: Ki-57-I Topsy damaged by flak
AIRCRAFT LANDING
No.205 Sqn RAF sub-groups COMBINING at Palembang
AVG/3rd Sqn sub-groups COMBINING at Diamond Harbour
No.1 Sqn RAAF sub-groups COMBINING at Soerabaja
6 OAZ-VU sub-groups COMBINING at Vladivostok
SHIP CREWS PERFORM DAMAGE CONTROL
xAK Toendjoek sinks....
SURFACE COMBAT CHECK
DIVIDE CRIPPLED TFs
AMC Manoora sinks....
CPT Spurgeon, A.H. has been KILLED
SHIP CREWS PERFORM DAMAGE CONTROL
SHIP CREWS PERFORM DAMAGE CONTROL
Amphibious TF 44 unloading at Kota Bharu
Amphibious TF 85 unloading at Kuantan
Amphibious TF 90 offshore of Aparri
Amphibious TF 91 unloading at Kuantan
Amphibious TF 92 unloading at Kuantan
Amphibious TF 97 unloading at Kuantan
Amphibious TF 98 unloading at Kuantan
Amphibious TF 110 offshore of Cagayan
Amphibious TF 115 offshore of San Fernando
Amphibious TF 117 offshore of San Fernando
Amphibious TF 129 offshore of Guam
REPAIRING SHIPS
END 12 HOUR PULSE
DIVIDE CRIPPLED TFs
CHECKING TF FUEL
TF'S RETURN TO PORT
Japanese forces CAPTURE Kuantan !!!
22nd Indian Brigade RETREATING towards Temuloh
3rd HK&S Light AA Regiment RETREATING towards Temuloh
1st ISF Base Force SURRENDERS !!!
20th Chinese Corps RETREATING towards Changsha
6th Division IN PURSUIT
18th Ind.Mixed Brigade IN PURSUIT
98th Chinese Corps RETREATING towards Chengchow
12th Chinese Corps RETREATING towards Nanyang
3rd Group Army RETREATING towards Nanyang
Japanese forces CAPTURE Kota Bharu !!!
FMSV Brigade RETREATING towards Kuala Krai
8th Indian Brigade SURRENDERS !!!
3rd ISF Base Force RETREATING towards Kuala Krai
12th Engineer Regiment IN PURSUIT
RECALCULATING ZONES OF CONTROL
SUPPLY REQUIREMENTS PHASE
Guam expands fortifications to size 1
Agra expands fortifications to size 1
Bareilly expands fortifications to size 1
Cawnpore expands fortifications to size 1
Port Augusta expands fortifications to size 1
Fort St. John expands fortifications to size 1
Regina expands fortifications to size 1
Choybalsan expands fortifications to size 1
Malaybalay expands fortifications to size 1
MOVING OIL AND RESOURCES
PRODUCTION PHASE
SUPPLY MOVEMENT PHASE
CALCULATING SPOILAGE
AIRCRAFT REPAIR PHASE
AIRCRAFT SUPPLY PHASE
UPGRADING SHIPS
UPGRADING AIR UNITS
UPGRADING LAND UNITS
SUPPLY PHASE COMPLETE
END OF DAY
TRAINING SHIP CREWS
DISBANDING HUMAN CONTROLLED TASK FORCES
ADJUSTING AVIATION SUPPORT
LAND UNIT REPLACEMENT PHASE
AIR UNIT REPLACEMENT PHASE
MERGING SUB UNITS
SUB UNIT MERGE COMPLETE
DIVIDE CRIPPLED TFs
Low garrison leads to damage and 1 VP loss from partisan attack at Colombo (29, 48)!
Low garrison leads to damage and 1 VP loss from partisan attack at Agra (50, 19)!
Low garrison leads to damage and 1 VP loss from partisan attack at Sholapur (36, 29)!
LEADER REASSIGNMENT PHASE
DD Morris arrives at Eastern USA
No.67 Sqn RAF arrives at Myitkyina
No.67 Sqn RAF sub-groups COMBINING at Myitkyina
2-Vl.G.IV arrives at Batavia
2-Vl.G.IV sub-groups COMBINING at Batavia
No.60 Sqn RAF arrives at Myitkyina
2-VI.G.II  arrives at Batavia
2-VI.G.II  sub-groups COMBINING at Batavia
1-VI.G.II  arrives at Batavia
1-VI.G.II  sub-groups COMBINING at Batavia
D.Vl.A arrives at Soerabaja
D.Vl.A sub-groups COMBINING at Soerabaja
No.27 Sqn RAF arrives at Myitkyina
VP-51 arrives at San Diego
No.111 Sqn RCAF arrives at Vancouver
No.5 Sqn RAAF arrives at Townsville
7th BG/9th BS arrives at San Diego
35th PG/70th PS arrives at San Diego
35th PG/HqS arrives at San Diego
48th Light AA Regiment arrives at Cape Town
3rd/101st PA Battalion arrives at Cotabato
SEARCHING FOR ADJACENT ENEMY UNITS
REASSIGNING TRANSPORT GROUPS
AVG/3rd Sqn sub-groups COMBINING at Calcutta

===== section @43536 len=2 =====


===== section @43542 len=2 =====


===== section @43548 len=14127 =====
OPERATIONAL REPORT FOR Dec 10, 41

Submarine TF 424 terminates patrol and returns to Sydney
SS K XII reports having been sighted by the enemy at 52 , 75
SS K XIII reports having been sighted by the enemy at 52 , 79
SS K XV reports having been sighted by the enemy at 52 , 80
SS O 20 reports having been sighted by the enemy at 53 , 75
CS Convoy TF 298 unable to fully load cargo at Abadan 
CS Convoy TF 326 unable to fully load cargo at Abadan 
8th Marine Regiment packing for load on AP Barnett (TF 506), 1 day(s) remaining
8th Marine Regiment packing for load on xAP Mariposa (TF 506), 1 day(s) remaining
8th Marine Regiment packing for load on xAK Agwimonte (TF 506), 1 day(s) remaining
8th Marine Regiment packing for load on xAK K.I. Luckenbach (TF 506), 1 day(s) remaining
Mine Sweeping TF 441 resumes patrol, enroute to 78 , 171  near Warnambool
Surface Combat TF 341 resumes patrol, enroute to 52 , 83  near Djemadja
Task Force 336 has refueled TF 492 at 70 , 94 and is returning to Balikpapan
8th Marine Regiment packing for load on AP Barnett (TF 506), 1 day(s) remaining
8th Marine Regiment packing for load on xAP Mariposa (TF 506), 1 day(s) remaining
8th Marine Regiment packing for load on xAK Agwimonte (TF 506), 1 day(s) remaining
8th Marine Regiment packing for load on xAK K.I. Luckenbach (TF 506), 1 day(s) remaining
Task Force 444 out of fuel at 77, 76
Escort TF 42 (1 ship) formed from TF 532 and routed to Singapore
TF 344 detected by Japanese Torpedo Bomber at 73,71 near Paracel Islands
TF 447 observes Japanese Recon at 78,77 near Bataan
TF 348 followed by Japanese Recon at 70,94 near Tandjoengselor
TF 492 sights Japanese Aircraft at 68,92 near Tandjoengselor
DD Peary sighted by Japanese Aircraft at 69,90 near Tawau
TF 301 shadowed by Japanese Float Plane at 45,72 near Langsa
TF 341 sights Japanese Patrol at 52,84 near Tioman Island
Sighting report: 10 Japanese ships at 51,79 near Kuantan , Speed unknown  
TF 342 observes Japanese Float Plane at 77,78 near Lubang
xAP Rochambeau snooped by Japanese Float Fighter at 71,88 near Sandakan
TK Gertrude Kellogg sighted by E13A1 Jake at 71,88 near Sandakan
TF 341 shadowed by Japanese Fighter Bomber at 52,84 near Tioman Island
TF 301 sighted by Japanese Recon at 45,72 near Langsa
AMC Manoora observes E13A1 Jake at 49,85 near Singapore
SS K XIV shadowed by Japanese Torpedo Bomber at 53,80 near Kuantan
MTB 11 sights Japanese Torpedo Bomber at 73,71 near Paracel Islands
TF 443 observes Japanese Float Plane at 76,77 near Bataan
AMC Manoora snooped by E13A1 Jake at 49,85 near Singapore
TF 444 detects E7K2 Alf at 76,77 near Bataan
TF 514 sighted by Japanese Float Plane at 177,120 near Kona
139WH-3 reports submarine wake at 49, 97 near Batavia
139WH-3 reports conning tower at 49, 97 near Batavia
139WH-3 reports oil slick at 49, 97 near Batavia
139WH-3 reports periscope at 49, 97 near Batavia
139WH-3 reports turbulence in water at 49, 97 near Batavia
Sighting report: 8 Japanese ships at 51,79 near Kuantan , Speed unknown  
Sighting report: 6 Japanese ships at 51,79 near Kuantan , Speed unknown  
Sighting report: 4 Japanese ships at 51,79 near Kuantan , Speed unknown  
Sighting report: 8 Japanese ships at 51,79 near Kuantan , Speed unknown  
Sighting report: 2 Japanese ships at 51,79 near Kuantan , Speed 4 , Moving West 
Sighting report: 6 Japanese ships at 51,79 near Kuantan , Speed unknown  
Sighting report: 4 Japanese ships at 51,79 near Kuantan , Speed unknown  
Sighting report: 8 Japanese ships at 51,79 near Kuantan , Speed unknown  
Sighting report: 4 Japanese ships at 51,79 near Kuantan , Speed 7 , Moving West 
AM Penguin detected by Japanese Dive Bomber at 106,103 near Satawal
TF 388 followed by Japanese Recon at 69,91 near Tawau
Japanese Fighter sighted over Kuantan 
Japanese Aircraft sighted over Johore Bahru 
Japanese Fighter Bomber sighted over Vigan 
Japanese Torpedo Bomber sighted over Lingayen 
Japanese Fighter Bomber sighted over Batangas 
Japanese Dive Bomber sighted over Tuguegarao 
Japanese Aircraft sighted over Tuguegarao 
Japanese Dive Bomber sighted over Tuguegarao 
Japanese Aircraft sighted over Chengchow 
Japanese Fighter Bomber sighted over Iba 
Japanese Fighter Bomber sighted over San Fernando 
Japanese Recon sighted over Singapore 
Japanese Fighter sighted over Rangoon 
Ki-15-II Babs sighted over Georgetown 
Japanese Aircraft sighted over Alor Star 
Japanese Aircraft sighted over Lashio 
Ki-46-II Dinah sighted over Lashio 
Japanese Aircraft sighted over Yutze 
Japanese Fighter Bomber sighted over Sungei Pantani 
Japanese Aircraft sighted over Vigan 
Japanese Torpedo Bomber sighted over Manila 
Japanese Aircraft sighted over Kuantan 
Japanese Torpedo Bomber sighted over Johore Bahru 
Japanese Aircraft sighted over Vigan 
Japanese Aircraft sighted over Lingayen 
Japanese Aircraft sighted over Batangas 
Japanese Aircraft sighted over Tuguegarao 
Japanese Torpedo Bomber sighted over Chengchow 
Japanese Torpedo Bomber sighted over Iba 
Japanese Aircraft sighted over San Fernando 
Japanese Fighter sighted over Singapore 
Japanese Torpedo Bomber sighted over Rangoon 
Japanese Aircraft sighted over Georgetown 
Japanese Fighter Bomber sighted over Alor Star 
Japanese Aircraft sighted over Lashio 
Japanese Fighter sighted over Lashio 
Japanese Recon sighted over Yutze 
Japanese Torpedo Bomber sighted over Sungei Pantani 
Japanese Aircraft sighted over Vigan 
Japanese Torpedo Bomber sighted over Manila 
TF 435 sights Japanese Dive Bomber at 72,75 near Iba
CL Durban sighted by Japanese Torpedo Bomber at 49,86 near Singkep
PBY-5 Catalina sighting report: 9 Japanese ships at 186,109 near Hilo, speed 20, Moving East
139WH-3 reports shadow in water at 49, 97 near Batavia
139WH-3 reports turbulence in water at 49, 97 near Batavia
139WH-3 reports shadow in water at 49, 97 near Batavia
139WH-3 reports radio transmissions at 49, 97 near Batavia
139WH-3 reports object under water at 49, 97 near Batavia
139WH-3 reports submarine wake at 49, 97 near Batavia
139WH-3 reports radio transmissions at 49, 97 near Batavia
Sighting report: 2 Japanese ships at 51,75 near Kota Bharu  , Moving West 
Sighting report: 5 Japanese ships at 51,75 near Kota Bharu , Speed unknown  
Japanese Aircraft sighted over Kota Bharu 
Japanese Aircraft sighted over Kuantan 
Japanese Aircraft sighted over Johore Bahru 
Japanese Aircraft sighted over Clark Field 
Japanese Aircraft sighted over Vigan 
Japanese Aircraft sighted over Lingayen 
Japanese Fighter Bomber sighted over Batangas 
1LT Thorne, H.G. of 24th PG/3rd PS is credited with kill number 2
1LT Thorne, H.G. of 24th PG/3rd PS is credited with kill number 3
Japanese Torpedo Bomber sighted over Tuguegarao 
Japanese Torpedo Bomber sighted over Tuguegarao 
Japanese Aircraft sighted over Tuguegarao 
Japanese Aircraft sighted over Chengchow 
Japanese Aircraft sighted over Loyang 
Japanese Fighter Bomber sighted over 55th Chinese Corps
Japanese Aircraft sighted over Changsha 
Japanese Aircraft sighted over Bataan 
Japanese Aircraft sighted over San Fernando 
Japanese Aircraft sighted over Hong Kong 
Japanese Aircraft sighted over Singapore 
Japanese Recon sighted over 20th Chinese Corps
Japanese Recon sighted over Hong Kong 
Japanese Aircraft sighted over Georgetown 
Japanese Aircraft sighted over Alor Star 
Japanese Fighter sighted over Yutze 
Japanese Aircraft sighted over Sungei Pantani 
Japanese Fighter Bomber sighted over Vigan 
Japanese Aircraft sighted over Manila 
Japanese Torpedo Bomber sighted over Cagayan 
Japanese Aircraft sighted over Kota Bharu 
Japanese Recon sighted over Kuantan 
Japanese Fighter Bomber sighted over Johore Bahru 
Japanese Fighter sighted over Clark Field 
Japanese Torpedo Bomber sighted over Vigan 
Japanese Fighter Bomber sighted over Lingayen 
Japanese Fighter Bomber sighted over Batangas 
Japanese Aircraft sighted over Tuguegarao 
Japanese Aircraft sighted over Chengchow 
Ki-15-II Babs sighted over Loyang 
Japanese Aircraft sighted over 55th Chinese Corps
Japanese Fighter Bomber sighted over Changsha 
Japanese Aircraft sighted over Bataan 
Ki-46-I Dinah sighted over San Fernando 
Ki-15-II Babs sighted over Hong Kong 
Ki-15-II Babs sighted over Singapore 
Japanese Recon sighted over 20th Chinese Corps
Japanese Recon sighted over Hong Kong 
Japanese Aircraft sighted over Georgetown 
Japanese Aircraft sighted over Alor Star 
Japanese Aircraft sighted over Yutze 
Japanese Aircraft sighted over Sungei Pantani 
Japanese Aircraft sighted over Vigan 
Japanese Recon sighted over Manila 
Japanese Fighter Bomber sighted over Cagayan 
P-40E Warhawk from 24th PG/3rd PS is written off
Damaged P-40E Warhawk from 35th PG/21st PS is missing
Ki-57-I Topsy damaged by flak
2 x PBY-4 Catalina transporting SE Borneo KNIL Battalion from Bandjermasin
2 x Do-24K-1 transporting 1 ML-KNIL Aviation  from Batavia
3 x Do-24K-1 transporting 1 ML-KNIL Aviation  from Batavia
2 x Do-24K-1 transporting SE Borneo KNIL Battalion from Bandjermasin
3 x Do-24K-1 transporting 1 ML-KNIL Aviation  from Batavia
1 x Do-24K-1 transporting Sorong Det. Base Force from Sorong
3 x Do-24K-1 transporting 1 ML-KNIL Aviation  from Batavia
1 x T.IVa transporting Riouw KNIL Battalion from Singkep
3 x PBY-5 Catalina transporting 1 ML-KNIL Aviation  from Batavia
4 x Catalina I transporting Lark Force  from Rabaul
4 x Catalina I transporting Lark Force  from Rabaul
2 x S.23 Empire transporting Lark Force  from Rabaul
2 x S.23 Empire transporting Lark Force  from Rabaul
2 x Catalina I transporting 1 ML-KNIL Aviation  from Batavia
16 x DC-2 transporting supplies to Chengtu
2 x DC-2 transporting supplies to Chengtu
TF 323 does not have enough fuel to complete mission
TF 342 does not have enough fuel to complete mission
TF 346 does not have enough fuel to complete mission
TF 351 does not have enough fuel to complete mission
TF 360 does not have enough fuel to complete mission
TF 362 does not have enough fuel to complete mission
TF 375 does not have enough fuel to complete mission
TF 420 does not have enough fuel to complete mission
TF 424 does not have enough fuel to complete mission
TF 433 does not have enough fuel to complete mission
TF 435 does not have enough fuel to complete mission
TF 441 leaving patrol zone to refuel at Melbourne
TF 443 does not have enough fuel to complete mission
TF 444 does not have enough fuel to complete mission
TF 445 does not have enough fuel to complete mission
TF 446 does not have enough fuel to complete mission
TF 447 does not have enough fuel to complete mission
TF 460 does not have enough fuel to complete mission
TF 475 does not have enough fuel to complete mission
TF 504 does not have enough fuel to complete mission
TF 510 does not have enough fuel to complete mission
TF 513 does not have enough fuel to complete mission
Japanese forces CAPTURE Kuantan !!!
Japanese forces CAPTURE Kota Bharu !!!
Guam expands fortifications to size 1
Agra expands fortifications to size 1
Bareilly expands fortifications to size 1
Cawnpore expands fortifications to size 1
Port Augusta expands fortifications to size 1
Fort St. John expands fortifications to size 1
Regina expands fortifications to size 1
Choybalsan expands fortifications to size 1
Malaybalay expands fortifications to size 1
VMF-111 at San Diego takes F4F-3A Wildcat replacements
VMJ-152 at San Diego takes R3D-2 replacements
18th PG/6th PS at Pearl Harbor takes P-40B Warhawk replacements
31st PG/40th PS at San Diego takes P-39D Airacobra replacements
54th PG/42nd PS at Ogden takes P-39D Airacobra replacements
15th PG/46th PS at Pearl Harbor takes P-39D Airacobra replacements
1st PG/71st PS at San Diego takes P-43 Lancer replacements
VS-1D11 on CV Saratoga converting to size 81 from 18
No.814 Sqn FAA on CVL Hermes converting to size 18 from 12
18th Australian Battalion - some devices have been upgraded at Sydney
Darwin RAAF Base Force - some devices have been upgraded at Darwin
3/16th Punjab Battalion - some devices have been upgraded at Singapore
Delayed planes for the aircraft pool:
  8 (0) x H81-A3, 0 (7) x Hudson I, 0 (2) x Bolingbroke IV, 1 (2) x B-17D Fortress, 1 (0) x P-26A
  6 (19) x P-36A Mohawk, 7 (9) x P-39D Airacobra, 1 (1) x P-40B Warhawk, 3 (5) x P-43 Lancer
  1 (2) x OS2U-3 Kingfisher, 2 (6) x PBY-4 Catalina, 2 (3) x PBY-5 Catalina, 2 (11) x SB2U-3 Vindicator
  1 (0) x SNJ-3 Texan
BB Nevada available for service in damaged state at Pearl Harbor
BB Pennsylvania available for service in damaged state at Pearl Harbor
BC Repulse available for service in damaged state at Singapore
Device(s) upgraded in Batavia Base Force supplied from Batavia
Device(s) upgraded in 1st RNZAF Base Force supplied from Suva
Low garrison leads to damage and 1 VP loss from partisan attack at Colombo (29, 48)!
Low garrison leads to damage and 1 VP loss from partisan attack at Agra (50, 19)!
Low garrison leads to damage and 1 VP loss from partisan attack at Sholapur (36, 29)!
DD Morris arrives at Eastern USA
Loss of xAK Fatshan on Dec 10, 1941 is admitted
Loss of xAK Ming Sang on Dec 10, 1941 is admitted
Loss of xAKL Nam Yong on Dec 10, 1941 is admitted
Loss of xAK Wing Sang on Dec 10, 1941 is admitted
Loss of xAK Somelsdijk on Dec 10, 1941 is admitted
Loss of xAK Toendjoek on Dec 10, 1941 is admitted
Loss of xAK Haraldsvang on Dec 10, 1941 is admitted
Loss of AMC Manoora on Dec 10, 1941 is admitted
No.67 Sqn RAF arrives at Myitkyina
2-Vl.G.IV arrives at Batavia
No.60 Sqn RAF arrives at Myitkyina
2-VI.G.II  arrives at Batavia
1-VI.G.II  arrives at Batavia
D.Vl.A arrives at Soerabaja
No.27 Sqn RAF arrives at Myitkyina
VP-51 arrives at San Diego
No.111 Sqn RCAF arrives at Vancouver
No.5 Sqn RAAF arrives at Townsville
7th BG/9th BS arrives at San Diego
35th PG/70th PS arrives at San Diego
35th PG/HqS arrives at San Diego
48th Light AA Regiment arrives at Cape Town
3rd/101st PA Battalion arrives at Cotabato

===== section @57679 len=990 =====
SIG INT REPORT FOR Dec 10, 41

3/Sasebo 3rd SNLF is loaded on a Kongo Maru class xAP moving to Samah.
51st Air Defense AA Battalion is located at Kagi(85,64).
3618 men are based at Phnom Penh (58,69).
Radio transmissions detected at Tungchow (92,53).
6th Mongol Cavalry Division is located at Kalgan(95,37).
Miura Det  is located at Babeldaob(90,97).
17th Medium Field Artillery Regiment is located at Babeldaob(90,97).
334 men are based at Dalat (63,71).
4th RGC Route Brigade is located at Shanghai(92,55).
3rd JAAF AF Coy  is located at Tourane(66,65).
Radio transmissions detected at 98,66.
40th/A Division is located at Wuchang(84,51).
Radio transmissions detected at Cam Ranh Bay (64,72).
2/9th Infantry Regiment is planning for an attack on Lingayen.
7th Field AF Construction Battalion is located at Chumphon(52,66).
3rd Hvy.Artillery Regiment is located at 102,41.
15th AA Regiment is located at Anyang(89,43).
Radio transmissions detected at Tunghua (106,44).
```

#### T5 `c446` — save date 12/12/41

Complete text of the record, all 4 sections, exactly as stored (section banners added by the extractor):

```text
===== section @0 len=26539 =====
AFTER ACTION REPORTS FOR Dec 11, 41
--------------------------------------------------------------------------------
Invasion Support action off Guam (106,95)
Defensive Guns engage approaching landing force
 
2 Coastal gun shots fired in defense.
 
Japanese Ships
      xAKL Ujigawa Maru
      xAKL Heiwa Maru
      xAK Nittai Maru
      xAK Shinkoku Maru
 
xAKL Ujigawa Maru fired at enemy troops
xAKL Heiwa Maru fired at enemy troops
xAK Nittai Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Submarine attack near Donggala  at 69,96
 
Japanese Ships
      SS I-123
 
Allied Ships
      xAKL Latouche, Torpedo hits 1,  on fire,  heavy damage
 
xAKL Latouche is sighted by SS I-123
SS I-123 launches 2 torpedoes at xAKL Latouche
 
 
--------------------------------------------------------------------------------
Submarine attack near Toboali  at 51,93
 
Japanese Ships
      SS I-162
 
Allied Ships
      xAP Glenapp, Torpedo hits 3,  on fire,  heavy damage
 
xAP Glenapp is sighted by SS I-162
SS I-162 launches 4 torpedoes at xAP Glenapp
 
 
--------------------------------------------------------------------------------
ASW attack near Kuantan  at 52,79
 
Japanese Ships
      DD Sagiri
 
Allied Ships
      SS K XIII, hits 1
 
SS K XIII is sighted by escort
DD Sagiri fails to find sub, continues to search...
DD Sagiri fails to find sub, continues to search...
DD Sagiri fails to find sub, continues to search...
DD Sagiri attacking submerged sub ....
DD Sagiri is out of ASW ammo
DD Sagiri fails to find sub, continues to search...
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
Night Air attack on Singapore , at 50,84 
 
Weather in hex: Overcast
 
Raid spotted at 26 NM, estimated altitude 16,000 feet.
Estimated time to target is 8 minutes
 
Japanese aircraft
      G4M1 Betty x 8
 
Japanese aircraft losses
      G4M1 Betty: 2 damaged
      G4M1 Betty: 1 destroyed by flak
 
Port fuel hits 1
 
Aircraft Attacking:
       8 x G4M1 Betty bombing from 12000 feet
               Port Attack:  2 x 250 kg SAP Bomb,  4 x 60 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Night Air attack on Singapore , at 50,84 
 
Weather in hex: Overcast
 
Raid detected at 10 NM, estimated altitude 13,000 feet.
Estimated time to target is 3 minutes
 
Japanese aircraft
      G4M1 Betty x 7
 
No Japanese losses
 
Aircraft Attacking:
       7 x G4M1 Betty bombing from 12000 feet
               Port Attack:  2 x 250 kg SAP Bomb,  4 x 60 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Submarine attack near Toboali  at 51,93
 
Japanese Ships
      SS I-153
 
Allied Ships
      xAP Kota Tjandi, Torpedo hits 1,  heavy damage
 
xAP Kota Tjandi is sighted by SS I-153
SS I-153 launches 2 torpedoes at xAP Kota Tjandi
 
 
--------------------------------------------------------------------------------
Night Naval bombardment of Wenchow at 89,58
 
Japanese Ships
      BB Hyuga
      BB Ise
      DD Sanae
      DD Kuretake
      DD Wakatake
 
Allied ground losses:
      329 casualties reported
         Squads: 1 destroyed, 15 disabled
         Non Combat: 10 destroyed, 34 disabled
         Engineers: 1 destroyed, 2 disabled
 
Manpower hits 6
Heavy Industry hits 1
Resources hits 2
Fires 4558
Airbase hits 16
Airbase supply hits 9
Runway hits 41
Port hits 11
Port fuel hits 10
Port supply hits 1
 
E8N2 Dave acting as spotter for BB Hyuga
BB Hyuga firing at Wenchow 
E8N2 Dave acting as spotter for BB Ise
BB Ise firing at 100th Chinese Corps
DD Sanae firing at 100th Chinese Corps
DD Kuretake firing at Wenchow 
DD Wakatake firing at Wenchow 
 
 
--------------------------------------------------------------------------------
Pre-Invasion action off Cagayan (79,89)
Defensive Guns engage approaching landing force
 
9 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Showa Maru #5
      PB Showa Maru #3
      PB Nichi Maru #1
      PB Gamitsu Maru #1
      PB Saiko Maru
      PB Edo Maru
      xAP Haruna Maru
      CS Chitose
 
Allied ground losses:
      17 casualties reported
         Squads: 0 destroyed, 1 disabled
         Non Combat: 1 destroyed, 2 disabled
         Engineers: 0 destroyed, 0 disabled
      Vehicles lost 1 (1 destroyed, 0 disabled)
 
PB Showa Maru #5 fired at enemy troops
PB Showa Maru #3 fired at enemy troops
PB Nichi Maru #1 fired at enemy troops
PB Gamitsu Maru #1 fired at enemy troops
PB Saiko Maru fired at enemy troops
PB Edo Maru fired at enemy troops
Defensive Guns fire at approaching troops in landing craft at 5,000 yards
Defensive Guns fire at approaching troops in landing craft at 2,000 yards
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Cagayan (79,89)
 
TF 89 troops unloading over beach at Cagayan, 79,89
 
Japanese ground losses:
      17 casualties reported
         Squads: 1 destroyed, 0 disabled
         Non Combat: 3 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
19 troops of a IJA Infantry Sqd (H) lost in surf during unload of Kimura Det 
12 Engineers lost from landing craft during unload of 1st Naval Const Bn
12 Support troops lost overboard during unload of Kure 1st SNLF /3
12 Support troops lost overboard during unload of 1st Naval Const Bn /2
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Hollandia (93,116)
 
TF 112 troops unloading over beach at Hollandia, 93,116
 
Japanese ground losses:
      16 casualties reported
         Squads: 1 destroyed, 0 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
17 troops of a SNLF Squad lost overboard during unload of 1st JNAF Coy 
 
 
--------------------------------------------------------------------------------
Invasion Support action off Guam (106,95)
Defensive Guns engage approaching landing force
 
1 Coastal gun shots fired in defense.
 
Japanese Ships
      xAKL Ujigawa Maru
      xAKL Heiwa Maru
      xAK Nittai Maru
      xAK Shinkoku Maru
 
xAKL Ujigawa Maru fired at enemy troops
xAKL Heiwa Maru fired at enemy troops
xAK Nittai Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Cagayan (79,89)
Defensive Guns engage approaching landing force
 
4 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Showa Maru #5
      PB Showa Maru #3
      PB Nichi Maru #1
      PB Gamitsu Maru #1
      PB Saiko Maru
      PB Edo Maru
      xAP Haruna Maru
      CS Chitose
 
Allied ground losses:
      38 casualties reported
         Squads: 0 destroyed, 1 disabled
         Non Combat: 0 destroyed, 7 disabled
         Engineers: 0 destroyed, 0 disabled
 
PB Showa Maru #5 fired at enemy troops
PB Showa Maru #3 fired at enemy troops
PB Nichi Maru #1 fired at enemy troops
PB Gamitsu Maru #1 fired at enemy troops
PB Saiko Maru fired at enemy troops
PB Edo Maru fired at enemy troops
Defensive Guns fire at approaching troops in landing craft at 1,000 yards
 
 
--------------------------------------------------------------------------------
Invasion Support action off Guam (106,95)
Defensive Guns engage approaching landing force
 
3 Coastal gun shots fired in defense.
 
Japanese Ships
      xAKL Ujigawa Maru
      xAKL Heiwa Maru
      xAK Nittai Maru
      xAK Shinkoku Maru
 
xAKL Ujigawa Maru fired at enemy troops
xAKL Heiwa Maru fired at enemy troops
xAK Nittai Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Sub attack near Pearl & Hermes Reef  at 154,103
 
Japanese Ships
      AO Nippon Maru
      AO Shinkoku Maru
      DD Akebono
 
Allied Ships
      SS Tambor
 
SS Tambor launches 4 torpedoes at AO Nippon Maru
Tambor diving deep ....
DD Akebono fails to find sub, continues to search...
DD Akebono fails to find sub, continues to search...
DD Akebono fails to find sub, continues to search...
DD Akebono fails to find sub, continues to search...
DD Akebono fails to find sub, continues to search...
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
ASW attack near Kuantan  at 52,79
 
Japanese Ships
      DD Yugiri
      PB Chosa Maru
 
Allied Ships
      SS K XIII, hits 5
 
SS K XIII is sighted by escort
DD Yugiri attacking submerged sub ....
PB Chosa Maru fails to find sub and abandons search
DD Yugiri fails to find sub, continues to search...
DD Yugiri fails to find sub, continues to search...
DD Yugiri fails to find sub, continues to search...
DD Yugiri attacking submerged sub ....
DD Yugiri is out of ASW ammo
DD Yugiri is out of ASW ammo
DD Yugiri fails to find sub, continues to search...
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
Sub attack near Pearl & Hermes Reef  at 154,103
 
Japanese Ships
      AO Kokuyo Maru
      AO Kyokuto Maru
      DD Akebono
 
Allied Ships
      SS Trout
 
SS Trout launches 4 torpedoes at AO Kokuyo Maru
DD Akebono fails to find sub, continues to search...
DD Akebono fails to find sub, continues to search...
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
ASW attack near Kota Bharu  at 53,75
 
Japanese Ships
      xAKL Kasui Maru
      PB Fuji Maru #4
 
Allied Ships
      SS O 20
 
SS O 20 is sighted by escort
PB Fuji Maru #4 fails to find sub, continues to search...
PB Fuji Maru #4 fails to find sub, continues to search...
PB Fuji Maru #4 fails to find sub, continues to search...
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
Sub attack near Kota Bharu  at 53,75
 
Japanese Ships
      xAKL Matsutan Maru, Torpedo hits 1,  on fire,  heavy damage
      xAKL Kasui Maru
      PB Fuji Maru #4
 
Allied Ships
      SS O 20
 
Japanese ground losses:
      22 casualties reported
         Squads: 0 destroyed, 0 disabled
         Non Combat: 2 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
SS O 20 launches 2 torpedoes at xAKL Matsutan Maru
O 20 bottoming out ....
PB Fuji Maru #4 fails to find sub, continues to search...
PB Fuji Maru #4 fails to find sub, continues to search...
PB Fuji Maru #4 fails to find sub, continues to search...
PB Fuji Maru #4 fails to find sub, continues to search...
PB Fuji Maru #4 fails to find sub, continues to search...
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
Morning Air attack on 1/19th Chinese Corps, at 89,40 , near Tsingsing
 
Weather in hex: Partial cloud
 
Raid spotted at 17 NM, estimated altitude 15,000 feet.
Estimated time to target is 5 minutes
 
Japanese aircraft
      Ki-48-Ib Lily x 16
 
No Japanese losses
 
Allied ground losses:
      6 casualties reported
         Squads: 0 destroyed, 0 disabled
         Non Combat: 0 destroyed, 1 disabled
         Engineers: 0 destroyed, 0 disabled
 
Aircraft Attacking:
      16 x Ki-48-Ib Lily bombing from 9000 feet * 
               Ground Attack:  2 x 100 kg GP Bomb
 
Also attacking 33rd Chinese Corps ...
Also attacking 1/19th Chinese Corps ...
Also attacking 33rd Chinese Corps ...
Also attacking 1/19th Chinese Corps ...
 
 
--------------------------------------------------------------------------------
Morning Air attack on 15th Indian Brigade, at 49,73 (Alor Star)
 
Weather in hex: Overcast
 
Raid spotted at 32 NM, estimated altitude 12,000 feet.
Estimated time to target is 10 minutes
 
Japanese aircraft
      Ki-27b Nate x 14
      Ki-51 Sonia x 22
 
No Japanese losses
 
Aircraft Attacking:
      22 x Ki-51 Sonia bombing from 10000 feet
               Ground Attack:  4 x 50 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on 58th Chinese Corps, at 84,52 , near Wuchang
 
Weather in hex: Thunderstorms
 
Raid spotted at 13 NM, estimated altitude 12,000 feet.
Estimated time to target is 3 minutes
 
Japanese aircraft
      Ki-27b Nate x 11
      Ki-30 Ann x 9
 
No Japanese losses
 
Aircraft Attacking:
       9 x Ki-30 Ann bombing from 9000 feet
               Ground Attack:  1 x 250 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on 12th Chinese Corps, at 86,47 , near Sinyang
 
Weather in hex: Clear sky
 
Raid spotted at 8 NM, estimated altitude 10,000 feet.
Estimated time to target is 2 minutes
 
Japanese aircraft
      Ki-51 Sonia x 19
 
No Japanese losses
 
Allied ground losses:
      41 casualties reported
         Squads: 0 destroyed, 4 disabled
         Non Combat: 0 destroyed, 3 disabled
         Engineers: 0 destroyed, 0 disabled
 
Aircraft Attacking:
      19 x Ki-51 Sonia bombing from 8000 feet
               Ground Attack:  4 x 50 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on 48th Chinese Corps, at 88,50 , near Hwainan
 
Weather in hex: Light cloud
 
Raid spotted at 12 NM, estimated altitude 15,000 feet.
Estimated time to target is 5 minutes
 
Japanese aircraft
      Ki-36 Ida x 21
 
No Japanese losses
 
Allied ground losses:
      25 casualties reported
         Squads: 1 destroyed, 1 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 1 disabled
 
Aircraft Attacking:
      21 x Ki-36 Ida bombing from 10000 feet
               Ground Attack:  4 x 30 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on 52nd Chinese Corps, at 70,56 (Lang Son)
 
Weather in hex: Light cloud
 
Raid spotted at 8 NM, estimated altitude 6,000 feet.
Estimated time to target is 2 minutes
 
Japanese aircraft
      Ki-51 Sonia x 6
 
No Japanese losses
 
Aircraft Attacking:
       6 x Ki-51 Sonia bombing from 6000 feet
               Ground Attack:  4 x 50 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on 15th Indian Brigade, at 49,73 (Alor Star)
 
Weather in hex: Overcast
 
Raid spotted at 15 NM, estimated altitude 9,000 feet.
Estimated time to target is 4 minutes
 
Japanese aircraft
      Ki-30 Ann x 11
 
No Japanese losses
 
Allied ground losses:
      16 casualties reported
         Squads: 0 destroyed, 1 disabled
         Non Combat: 0 destroyed, 2 disabled
         Engineers: 0 destroyed, 0 disabled
 
Aircraft Attacking:
      11 x Ki-30 Ann bombing from 6000 feet
               Ground Attack:  1 x 250 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Afternoon Air attack on 88th Chinese Corps, at 90,56 , near Hangchow
 
Weather in hex: Clear sky
 
Raid spotted at 17 NM, estimated altitude 11,000 feet.
Estimated time to target is 5 minutes
 
Japanese aircraft
      B5M1 Mabel x 5
 
No Japanese losses
 
Allied ground losses:
      28 casualties reported
         Squads: 0 destroyed, 1 disabled
         Non Combat: 0 destroyed, 2 disabled
         Engineers: 1 destroyed, 0 disabled
 
Aircraft Attacking:
       5 x B5M1 Mabel bombing from 6000 feet
               Ground Attack:  2 x 250 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Afternoon Air attack on Hong Kong , at 77,61 
 
Weather in hex: Overcast
 
Raid detected at 14 NM, estimated altitude 11,000 feet.
Estimated time to target is 4 minutes
 
Japanese aircraft
      A6M2 Zero x 9
      Ki-48-Ib Lily x 6
 
No Japanese losses
 
Airbase hits 3
Runway hits 3
 
Aircraft Attacking:
       6 x Ki-48-Ib Lily bombing from 10000 feet
               Airfield Attack:  4 x 100 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Afternoon Air attack on Rangoon , at 54,53 
 
Weather in hex: Overcast
 
Raid spotted at 33 NM, estimated altitude 14,000 feet.
Estimated time to target is 9 minutes
 
Japanese aircraft
      Ki-21-IIa Sally x 20
      Ki-43-Ib Oscar x 23
 
No Japanese losses
 
Airbase hits 3
Airbase supply hits 1
Runway hits 6
 
Aircraft Attacking:
      20 x Ki-21-IIa Sally bombing from 10000 feet
               Airfield Attack:  4 x 250 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Naga (81,81)
 
TF 87 troops unloading over beach at Naga, 81,81
 
Japanese ground losses:
      17 casualties reported
         Squads: 1 destroyed, 0 disabled
         Non Combat: 2 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
19 troops of a IJA Infantry Sqd (H) accidentally lost during unload of Tanaka Det 
12 Support troops lost overboard during unload of 28th JAAF AF Bn /2
12 Support troops lost in surf during unload of Tanaka Det /2
 
 
--------------------------------------------------------------------------------
Invasion Support action off Cagayan (79,89)
Defensive Guns engage approaching landing force
 
10 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Showa Maru #5
      PB Showa Maru #3
      PB Nichi Maru #1
      PB Gamitsu Maru #1
      PB Saiko Maru
      PB Edo Maru
      xAP Haruna Maru
 
Allied ground losses:
      11 casualties reported
         Squads: 0 destroyed, 2 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
PB Showa Maru #5 fired at enemy troops
PB Showa Maru #3 fired at enemy troops
PB Nichi Maru #1 fired at enemy troops
PB Gamitsu Maru #1 fired at enemy troops
PB Saiko Maru fired at enemy troops
PB Edo Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Guam (106,95)
Defensive Guns engage approaching landing force
 
1 Coastal gun shots fired in defense.
 
Japanese Ships
      xAK Shinkoku Maru
 
xAK Shinkoku Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
 
Ground combat at 89,40 (near Tsingsing)
 
Japanese Deliberate attack
 
Attacking force 17611 troops, 179 guns, 69 vehicles, Assault Value = 568
 
Defending force 7554 troops, 54 guns, 0 vehicles, Assault Value = 244
 
Japanese adjusted assault: 435 
 
Allied adjusted defense: 307 
 
Japanese assault odds: 1 to 1 
 
Combat modifiers
Defender: terrain(+), morale(-), experience(-)
Attacker: 
 
Japanese ground losses:
      603 casualties reported
         Squads: 0 destroyed, 90 disabled
         Non Combat: 0 destroyed, 7 disabled
         Engineers: 0 destroyed, 2 disabled
      Vehicles lost 3 (1 destroyed, 2 disabled)
 
Allied ground losses:
      487 casualties reported
         Squads: 1 destroyed, 90 disabled
         Non Combat: 1 destroyed, 8 disabled
         Engineers: 1 destroyed, 1 disabled
 
Assaulting units:
    37th Division
    4th Ind.Mixed Brigade
    6th Medium Field Artillery Regiment
 
Defending units:
    1/19th Chinese Corps
    33rd Chinese Corps
    7th Group Army
 
 
--------------------------------------------------------------------------------
 
Ground combat at Hong Kong (77,61)
 
Japanese Bombardment attack
 
Attacking force 2541 troops, 192 guns, 133 vehicles, Assault Value = 840
 
Defending force 6916 troops, 131 guns, 80 vehicles, Assault Value = 178
 
Allied ground losses:
      22 casualties reported
         Squads: 0 destroyed, 4 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
Assaulting units:
    68th Ind.Infantry Battalion
    38th Division
    67th Ind.Infantry Battalion
    66th Infantry Regiment
    1st Ind.Inf.Group 
    19th Ind. Engineer Regiment
    20th Ind. Engineer Regiment
    20th RGC Division
    3rd Ind.Hvy.Art. Battalion
    2nd Ind.Hvy.Art. Battalion
    1st Hvy.Artillery Regiment
    20th Ind. Mtn Gun Battalion
    5th RF Gun Battalion
    47th JAAF AF Bn 
    2nd RF Gun Battalion
    10th Ind. Mountain Gun Regiment
    23rd Army
    21st Mortar Battalion
    2nd Mortar Battalion
    1st JAAF AF Coy 
 
Defending units:
    1st Middlesex Battalion
    Rifles of Canada Battalion
    Winnipeg Grenadiers Battalion
    Kowloon Brigade
    102nd RN Base Force
    Hong Kong Fortress
 
 
--------------------------------------------------------------------------------
 
Ground combat at San Fernando (80,74)
 
Japanese Deliberate attack
 
Attacking force 7383 troops, 38 guns, 0 vehicles, Assault Value = 254
 
Defending force 0 troops, 0 guns, 0 vehicles, Assault Value = 0
 
Japanese adjusted assault: 242 
 
Allied adjusted defense: 1 
 
Japanese assault odds: 242 to 1 (fort level 0) 
 
Japanese forces CAPTURE San Fernando !!!
 
Combat modifiers
Attacker: 
 
Assaulting units:
    65th Brigade
    5th Naval Construction Battalion
    23rd Air Flotilla 
    6th Naval Construction Battalion
    28th JAAF AF Bn 
 
 
--------------------------------------------------------------------------------
 
Ground combat at Victoria Point (51,66)
 
Japanese Deliberate attack
 
Attacking force 1218 troops, 12 guns, 0 vehicles, Assault Value = 43
 
Defending force 396 troops, 4 guns, 0 vehicles, Assault Value = 6
 
Japanese adjusted assault: 43 
 
Allied adjusted defense: 1 
 
Japanese assault odds: 43 to 1 (fort level 1) 
 
Japanese forces CAPTURE Victoria Point !!!
 
Combat modifiers
Defender: terrain(+), leaders(+), preparation(-), morale(-)
 experience(-)
Attacker: leaders(+)
 
Allied ground losses:
      247 casualties reported
         Squads: 3 destroyed, 2 disabled
         Non Combat: 12 destroyed, 0 disabled
         Engineers: 4 destroyed, 0 disabled
      Guns lost 2 (2 destroyed, 0 disabled)
      Units retreated 1
 
Defeated Allied Units Retreating!
 
Assaulting units:
    I./143rd Infantry Battalion
 
Defending units:
    108th/2 Base Force
 
 
--------------------------------------------------------------------------------
 
Ground combat at Aparri (82,73)
 
Japanese Deliberate attack
 
Attacking force 1097 troops, 17 guns, 0 vehicles, Assault Value = 42
 
Defending force 0 troops, 0 guns, 0 vehicles, Assault Value = 0
 
Japanese adjusted assault: 42 
 
Allied adjusted defense: 1 
 
Japanese assault odds: 42 to 1 (fort level 0) 
 
Japanese forces CAPTURE Aparri !!!
 
Combat modifiers
Attacker: leaders(+)
 
Assaulting units:
    Sasebo 1st SNLF 
 
 
--------------------------------------------------------------------------------
 
Ground combat at Lashio (62,46)
 
Japanese Shock attack
 
Attacking force 720 troops, 12 guns, 0 vehicles, Assault Value = 48
 
Defending force 486 troops, 5 guns, 0 vehicles, Assault Value = 4
 
Japanese adjusted assault: 82 
 
Allied adjusted defense: 4 
 
Japanese assault odds: 20 to 1 (fort level 0) 
 
Japanese forces CAPTURE Lashio !!!
 
Combat modifiers
Defender: terrain(+), op mode(-), leaders(+), preparation(-)
 morale(-), experience(-)
Attacker: shock(+)
 
Allied ground losses:
      328 casualties reported
         Squads: 2 destroyed, 0 disabled
         Non Combat: 26 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
      Guns lost 1 (1 destroyed, 0 disabled)
      Units retreated 1
 
Defeated Allied Units Retreating!
 
Assaulting units:
    Yokosuka 3rd SNLF
    Yokosuka 1st SNLF /1
 
Defending units:
    102nd RAF Base Force
 
 
--------------------------------------------------------------------------------
 
Ground combat at Guam (106,95)
 
Japanese Deliberate attack
 
Attacking force 5082 troops, 21 guns, 0 vehicles, Assault Value = 53
 
Defending force 894 troops, 0 guns, 32 vehicles, Assault Value = 9
 
Japanese adjusted assault: 26 
 
Allied adjusted defense: 13 
 
Japanese assault odds: 2 to 1 (fort level 1) 
 
Japanese Assault reduces fortifications to 0
 
Combat modifiers
Defender: terrain(+), preparation(-), experience(-)
Attacker: 
 
Japanese ground losses:
      25 casualties reported
         Squads: 0 destroyed, 2 disabled
         Non Combat: 0 destroyed, 1 disabled
         Engineers: 0 destroyed, 0 disabled
 
Allied ground losses:
      18 casualties reported
         Squads: 0 destroyed, 2 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 2 disabled
 
Assaulting units:
    II/81st Naval Guard Unit
    5th Base Force
 
Defending units:
    Guam USN Base Force
 
 
--------------------------------------------------------------------------------
 
Ground combat at Hong Kong (77,61)
 
Allied Bombardment attack
 
Attacking force 954 troops, 74 guns, 0 vehicles, Assault Value = 174
 
Defending force 32095 troops, 408 guns, 278 vehicles, Assault Value = 840
 
Assaulting units:
    Winnipeg Grenadiers Battalion
    Rifles of Canada Battalion
    1st Middlesex Battalion
    Kowloon Brigade
    102nd RN Base Force
    Hong Kong Fortress
 
Defending units:
    38th Division
    1st Ind.Inf.Group 
    20th Ind. Engineer Regiment
    66th Infantry Regiment
    67th Ind.Infantry Battalion
    68th Ind.Infantry Battalion
    19th Ind. Engineer Regiment
    20th RGC Division
    2nd RF Gun Battalion
    1st Hvy.Artillery Regiment
    23rd Army
    47th JAAF AF Bn 
    2nd Mortar Battalion
    3rd Ind.Hvy.Art. Battalion
    5th RF Gun Battalion
    10th Ind. Mountain Gun Regiment
    2nd Ind.Hvy.Art. Battalion
    21st Mortar Battalion
    20th Ind. Mtn Gun Battalion
    1st JAAF AF Coy 
 
 

===== section @26543 len=7393 =====
COMBAT EVENTS FOR 12/11/41

ADJUST TASK FORCE MISSIONS 
CALCULATE RANGE TO ENEMY
CALCULATE AIR SUPERIORITY
ASSIGN COMPUTER CONTROLLED UNITS
Hwaiyin is occupied by the Japanese
Combat Events:
NAVAL REACTION PHASE
LOADING/UNLOADING TASK FORCES
Amphibious TF 115 unloading at San Fernando
Amphibious TF 117 unloading at San Fernando
Amphibious TF 129 unloading at Guam
TASK FORCE LOADING/UNLOADING COMPLETE
CALCULATING TASK FORCE PATHS
MINESWEEPING OPERATIONS
NAVAL MOVEMENT PHASE
xAKL Latouche sinks....
xAP Glenapp sinks....
ADJUST SHIP FUEL
NIGHT AIR OPERATIONS PHASE
AIR NIGHT STRIKES
AIRCRAFT LANDING
SHIP CREWS PERFORM DAMAGE CONTROL
SURFACE COMBAT CHECK
DIVIDE CRIPPLED TFs
SHIP CREWS PERFORM DAMAGE CONTROL
Japanese Ships Bombarding Wenchow
SHIP CREWS PERFORM DAMAGE CONTROL
Amphibious TF 89 offshore of Cagayan
Amphibious TF 112 offshore of Hollandia
Amphibious TF 117 unloading at San Fernando
Amphibious TF 129 unloading at Guam
REPAIRING SHIPS
END 12 HOUR PULSE
DIVIDE CRIPPLED TFs
CHECKING TF FUEL
TF'S RETURN TO PORT
Combat Events:
NAVAL REACTION PHASE
LOADING/UNLOADING TASK FORCES
Amphibious TF 89 unloading at Cagayan
Amphibious TF 112 unloading at Hollandia
Amphibious TF 117 unloading at San Fernando
Amphibious TF 129 unloading at Guam
TASK FORCE LOADING/UNLOADING COMPLETE
CALCULATING TASK FORCE PATHS
MINESWEEPING OPERATIONS
NAVAL MOVEMENT PHASE
ADJUST SHIP FUEL
AIR OPERATIONS PHASE : AM
TF 301 snooped by Japanese Aircraft at 44,70 near Sabang
TF 416 followed by Japanese Aircraft at 50,84 near Singapore
xAK Hanyang snooped by Japanese Medium Bomber at 69,82 near Balabac
TF 341 sights G3M2 Nell at 52,83 near Djemadja
E13A1 Jake has spotted an Allied DD at 50,84
TF 338 detected by Japanese Float Fighter at 49,85 near Singapore
AM Goulburn detected by Japanese Fighter Bomber at 50,84 near Singapore
TF 342 snooped by Japanese Float Plane at 76,82 near Taytay
TF 340 sights Japanese Torpedo Bomber at 50,84 near Singapore
Search E8N2 Dave destroyed by CAP at (52,83)
TF 342 detected by Japanese Recon at 76,82 near Taytay
SOC-1 Seagull reports oil slick at 148, 137 near Baker Island
SOC-1 Seagull sighting report: Japanese SS at 148,131 near Howland Island , Speed 17 , Moving West 
Do-24K-1 sighting report: 5 Japanese ships at 51,79 near Kuantan , Speed unknown 
Blenheim I sighting report: 5 Japanese ships at 51,79 near Kuantan , Speed unknown 
Blenheim I sighting report: 2 Japanese ships at 51,79 near Kuantan  , Moving West
Blenheim I sighting report: 10 Japanese ships at 51,79 near Kuantan , Speed unknown 
AM Penguin snooped by Japanese Aircraft at 100,108 near Woleai
AIR STRIKES
AIRCRAFT LANDING
SHIP CREWS PERFORM DAMAGE CONTROL
xAKL Legazpi sinks....
DIVIDE CRIPPLED TFs
AIR OPERATIONS PHASE : PM
MTB 11 followed by Japanese Recon at 73,71 near Paracel Islands
TF 447 snooped by Japanese Aircraft at 75,81 near Taytay
Search F1M2 Pete destroyed by CAP
TF 444 sights Japanese Float Plane at 76,82 near Taytay
Blenheim I sighting report: 2 Japanese ships at 51,79 near Kuantan , Speed 7 , Moving West
Blenheim I sighting report: 10 Japanese ships at 51,79 near Kuantan , Speed unknown 
Blenheim I sighting report: 10 Japanese ships at 51,79 near Kuantan , Speed 5 , Moving West
Blenheim I sighting report: 10 Japanese ships at 51,79 near Kuantan , Speed 1 , Moving West
G4M1 Betty has spotted a Bathurst class AM at 100,108
AIR STRIKES
AIRCRAFT LANDING
AIR TRANSPORT PHASE
AIRCRAFT LANDING
VP-12 sub-groups COMBINING at Suva
VP-12 sub-groups COMBINING at Suva
VP-11 sub-groups COMBINING at Johnston Island
GVT-11 sub-groups COMBINING at Palembang
SHIP CREWS PERFORM DAMAGE CONTROL
SURFACE COMBAT CHECK
DIVIDE CRIPPLED TFs
SHIP CREWS PERFORM DAMAGE CONTROL
SHIP CREWS PERFORM DAMAGE CONTROL
Amphibious TF 87 offshore of Naga
Amphibious TF 89 unloading at Cagayan
Amphibious TF 112 unloading at Hollandia
Amphibious TF 117 unloading at San Fernando
Amphibious TF 129 unloading at Guam
REPAIRING SHIPS
END 12 HOUR PULSE
DIVIDE CRIPPLED TFs
CHECKING TF FUEL
TF'S RETURN TO PORT
Japanese forces CAPTURE San Fernando !!!
Japanese forces CAPTURE Victoria Point !!!
108th/2 Base Force RETREATING towards Mergui
Japanese forces CAPTURE Aparri !!!
Japanese forces CAPTURE Lashio !!!
102nd RAF Base Force RETREATING towards Gokteik Viaduct
RECALCULATING ZONES OF CONTROL
SUPPLY REQUIREMENTS PHASE
Guam expands fortifications to size 1
Wake Island expands fortifications to size 2
Butuan expands fortifications to size 1
Bacolod expands fortifications to size 1
Toungoo expands fortifications to size 1
Ahmedabad expands fortifications to size 1
Diamond Harbour expands fortifications to size 1
Gorakhpur expands fortifications to size 1
Lucknow expands fortifications to size 1
Moradabad expands fortifications to size 1
Bourke expands fortifications to size 1
Kingscote expands fortifications to size 1
Port Kembla expands fortifications to size 1
Blenheim expands fortifications to size 1
Gisborne expands fortifications to size 1
Invercargill expands fortifications to size 1
Palmerston North expands fortifications to size 1
Waipapakauri expands fortifications to size 1
Ansi expands fortifications to size 1
MOVING OIL AND RESOURCES
PRODUCTION PHASE
SUPPLY MOVEMENT PHASE
CALCULATING SPOILAGE
AIRCRAFT REPAIR PHASE
AIRCRAFT SUPPLY PHASE
UPGRADING SHIPS
UPGRADING AIR UNITS
UPGRADING LAND UNITS
SUPPLY PHASE COMPLETE
END OF DAY
TRAINING SHIP CREWS
DISBANDING HUMAN CONTROLLED TASK FORCES
ADJUSTING AVIATION SUPPORT
LAND UNIT REPLACEMENT PHASE
AIR UNIT REPLACEMENT PHASE
MERGING SUB UNITS
SUB UNIT MERGE COMPLETE
DIVIDE CRIPPLED TFs
Low garrison leads to damage and 1 VP loss from partisan attack at Rangoon (54, 53)!
Low garrison leads to damage and 1 VP loss from partisan attack at Howrah (52, 36)!
Low garrison leads to damage and 1 VP loss from partisan attack at Sholapur (36, 29)!
Low garrison leads to damage and 1 VP loss from partisan attack at Hengyang (80, 53)!
Low garrison leads to damage and 1 VP loss from partisan attack at Loyang (87, 43)!
Low garrison leads to damage and 1 VP loss from partisan attack at Kweiteh (90, 46)!
Low garrison leads to damage and 1 VP loss from partisan attack at Nanning (72, 55)!
Low garrison leads to damage and 1 VP loss from partisan attack at Paoshan (65, 45)!
Low garrison leads to damage and 1 VP loss from partisan attack at Wenchow (89, 58)!
Low garrison leads to damage and 1 VP loss from partisan attack at Yenan (88, 37)!
Low garrison leads to damage and 1 VP loss from partisan attack at Bandoeng (50, 100)!
Low garrison leads to damage and 1 VP loss from partisan attack at Tingchow (83, 58)!
LEADER REASSIGNMENT PHASE
xAP Lurline arrives at San Diego
AK Betelgeuse arrives at San Diego
AK Arcturus arrives at San Diego
xAK Corrales arrives at Balboa
xAK Mahimahi arrives at Cristobal
xAP Esperance Bay arrives at Aden
AVG/1st Sqn arrives at Myitkyina
AVG/3rd Sqn arrives at Calcutta
AVG/3rd Sqn sub-groups COMBINING at Calcutta
VMF-121  arrives at San Diego
No.139 Sqn RAF arrives at Aden
No.146 Sqn RAF arrives at Chittagong
130th FA Rgt  arrives at Fort Ord
1st Indian Coastal Artillery Regiment arrives at Bombay
Provisional GMC Grp  arrives at Bataan
SEARCHING FOR ADJACENT ENEMY UNITS
REASSIGNING TRANSPORT GROUPS

===== section @33940 len=2 =====


===== section @33946 len=2 =====


===== section @33952 len=13673 =====
OPERATIONAL REPORT FOR Dec 11, 41

SS Skipjack reports having been sighted by the enemy at 77 , 93
Submarine TF 203 reports having been sighted by the enemy at 77 , 93
SS Spearfish reports having been sighted by the enemy at 77 , 93
SS S-37 reports having been sighted by the enemy at 78 , 94
Submarine TF 272 reports having been sighted by the enemy at 78 , 94
SS Sargo reports having been sighted by the enemy at 77 , 93
SS Triton reports having been sighted by the enemy at 153 , 103
SS K XI reports having been sighted by the enemy at 52 , 82
SS K XII reports having been sighted by the enemy at 52 , 75
SS K XIII reports having been sighted by the enemy at 52 , 79
SS K XIV reports having been sighted by the enemy at 53 , 80
SS K XV reports having been sighted by the enemy at 52 , 80
SS O 20 reports having been sighted by the enemy at 53 , 75
Mine Sweeping TF 441 resumes patrol, enroute to 78 , 171  near Warnambool
Task Force 448 lays minefield at Mersing ( 51 , 82) and sets course for Singapore
Escort TF 165 (1 ship) formed from TF 532 and routed to Batavia
Pumps fail aboard xAKL Legazpi, flooding increases...
Repairs completed on SS K IX at Soerabaja, ship returned to service
Escort TF 350 diverted to Soerabaja for emergency repairs
Fires causing damage to Light Industry at Wenchow!
CM Rigel lays defensive minefield at Palembang ( 48 , 91) and sets course for Batavia
TF 301 snooped by Japanese Aircraft at 44,70 near Sabang
TF 416 followed by Japanese Aircraft at 50,84 near Singapore
xAK Hanyang snooped by Japanese Medium Bomber at 69,82 near Balabac
TF 341 sights G3M2 Nell at 52,83 near Djemadja
SS S-40 followed by Japanese Aircraft at 82,91 near Davao
SS K XIV shadowed by Japanese Float Plane at 53,78 near Bukit Besi
TF 338 detected by Japanese Float Fighter at 49,85 near Singapore
AM Goulburn detected by Japanese Fighter Bomber at 50,84 near Singapore
TF 342 snooped by Japanese Float Plane at 76,82 near Taytay
TF 340 sights Japanese Torpedo Bomber at 50,84 near Singapore
WO Ennis N. of No.21 Sqn RAAF is credited with kill number 3
TF 342 detected by Japanese Recon at 76,82 near Taytay
SOC-1 Seagull reports oil slick at 148, 137 near Baker Island
139WH-3 reports object under water at 49, 97 near Batavia
139WH-3 reports possible submarine at 49, 97 near Batavia
139WH-3 reports conning tower at 49, 97 near Batavia
139WH-3 reports shape below surface at 49, 97 near Batavia
AM Penguin snooped by Japanese Aircraft at 100,108 near Woleai
Japanese Fighter sighted over Johore Bahru 
Japanese Aircraft sighted over Clark Field 
Japanese Fighter sighted over Lingayen 
H6K4 Mavis sighted over Cagayan 
Japanese Aircraft sighted over Tuguegarao 
Japanese Light Bomber sighted over Tuguegarao 
Japanese Dive Bomber sighted over Tuguegarao 
Japanese Fighter sighted over Chengchow 
Japanese Fighter sighted over Loyang 
Ki-15-II Babs sighted over 55th Chinese Corps
Japanese Fighter sighted over Changsha 
Japanese Aircraft sighted over 88th Chinese Corps
Japanese Aircraft sighted over Wenchow 
Japanese Recon sighted over Pakhoi 
Ki-51 Sonia sighted over Pakhoi 
Japanese Recon sighted over Pakhoi 
Japanese Aircraft sighted over Sungei Pantani 
Japanese Aircraft sighted over Sungei Pantani 
Ki-15-II Babs sighted over Sungei Pantani 
Japanese Aircraft sighted over Bataan 
Japanese Aircraft sighted over Iba 
Japanese Fighter Bomber sighted over San Fernando 
Japanese Aircraft sighted over 20th Chinese Corps
Japanese Recon sighted over Georgetown 
Japanese Fighter sighted over Alor Star 
Japanese Aircraft sighted over Yutze 
Japanese Fighter Bomber sighted over Manila 
Japanese Fighter sighted over Johore Bahru 
Japanese Aircraft sighted over Clark Field 
Japanese Aircraft sighted over Lingayen 
Japanese Aircraft sighted over Cagayan 
Ki-36 Ida sighted over Tuguegarao 
Ki-15-II Babs sighted over Chengchow 
Japanese Fighter Bomber sighted over Loyang 
Japanese Aircraft sighted over 55th Chinese Corps
Japanese Aircraft sighted over Changsha 
Japanese Fighter sighted over 88th Chinese Corps
Japanese Aircraft sighted over Wenchow 
Japanese Aircraft sighted over Sungei Pantani 
Japanese Aircraft sighted over Sungei Pantani 
Japanese Aircraft sighted over Sungei Pantani 
Japanese Fighter sighted over Bataan 
Japanese Aircraft sighted over Iba 
Ki-15-II Babs sighted over 20th Chinese Corps
Japanese Aircraft sighted over Georgetown 
Japanese Torpedo Bomber sighted over Alor Star 
Japanese Aircraft sighted over Yutze 
Japanese Aircraft sighted over Manila 
xAKL Legazpi sinks at sea
MTB 11 followed by Japanese Recon at 73,71 near Paracel Islands
TF 447 snooped by Japanese Aircraft at 75,81 near Taytay
TF 444 sights Japanese Float Plane at 76,82 near Taytay
SOC-1 Seagull reports suspected submarine at 149, 137 near Baker Island
139WH-3 reports conning tower at 49, 97 near Batavia
139WH-3 reports oil slick at 49, 97 near Batavia
139WH-3 reports suspected submarine at 49, 97 near Batavia
CW-22 Falcon reports periscope wake at 49, 97 near Batavia
Japanese Aircraft sighted over Clark Field 
Japanese Aircraft sighted over Vigan 
Japanese Fighter Bomber sighted over Lingayen 
Japanese Aircraft sighted over Batangas 
Ki-36 Ida sighted over Tuguegarao 
Ki-36 Ida sighted over Tuguegarao 
Japanese Recon sighted over Tuguegarao 
Japanese Fighter Bomber sighted over Chengchow 
Japanese Fighter Bomber sighted over Loyang 
Japanese Aircraft sighted over Changsha 
Japanese Fighter sighted over 88th Chinese Corps
Japanese Aircraft sighted over Wenchow 
Japanese Light Bomber sighted over Pakhoi 
Ki-51 Sonia sighted over Pakhoi 
Japanese Light Bomber sighted over Pakhoi 
Japanese Fighter sighted over Sungei Pantani 
Japanese Aircraft sighted over Sungei Pantani 
Japanese Aircraft sighted over Sungei Pantani 
Japanese Aircraft sighted over Bataan 
Japanese Aircraft sighted over Iba 
Japanese Torpedo Bomber sighted over San Fernando 
Japanese Aircraft sighted over Hong Kong 
Japanese Fighter Bomber sighted over Singapore 
Japanese Fighter Bomber sighted over Rangoon 
Japanese Aircraft sighted over 20th Chinese Corps
Japanese Fighter Bomber sighted over Hong Kong 
Japanese Aircraft sighted over Georgetown 
Ki-46-II Dinah sighted over Alor Star 
Japanese Aircraft sighted over Lashio 
Japanese Aircraft sighted over Lashio 
Japanese Aircraft sighted over Yutze 
Japanese Fighter Bomber sighted over Vigan 
C5M2 Babs sighted over Clark Field 
Japanese Aircraft sighted over Vigan 
Japanese Aircraft sighted over Lingayen 
Japanese Recon sighted over Batangas 
Japanese Light Bomber sighted over Tuguegarao 
Ki-15-II Babs sighted over Chengchow 
Japanese Fighter sighted over Loyang 
Ki-15-II Babs sighted over Changsha 
Japanese Torpedo Bomber sighted over 88th Chinese Corps
Japanese Fighter Bomber sighted over Wenchow 
Japanese Recon sighted over Sungei Pantani 
Japanese Aircraft sighted over Sungei Pantani 
Japanese Aircraft sighted over Sungei Pantani 
Japanese Aircraft sighted over Bataan 
Japanese Fighter sighted over Iba 
Ki-15-II Babs sighted over Hong Kong 
Japanese Aircraft sighted over Singapore 
Japanese Aircraft sighted over Rangoon 
Japanese Aircraft sighted over 20th Chinese Corps
Japanese Aircraft sighted over Hong Kong 
Japanese Aircraft sighted over Georgetown 
Japanese Aircraft sighted over Alor Star 
Japanese Aircraft sighted over Lashio 
Japanese Fighter Bomber sighted over Lashio 
Japanese Fighter Bomber sighted over Yutze 
Japanese Torpedo Bomber sighted over Vigan 
SBD-2 Dauntless from VS-6 is damaged on landing
2 x PBY-4 Catalina transporting SE Borneo KNIL Battalion from Bandjermasin
3 x Do-24K-1 transporting 1 ML-KNIL Aviation  from Batavia
3 x Do-24K-1 transporting 1 ML-KNIL Aviation  from Batavia
2 x Do-24K-1 transporting SE Borneo KNIL Battalion from Bandjermasin
2 x Do-24K-1 transporting 1 ML-KNIL Aviation  from Batavia
1 x Do-24K-1 transporting Sorong Det. Base Force from Sorong
3 x Do-24K-1 transporting 1 ML-KNIL Aviation  from Batavia
1 x T.IVa transporting Riouw KNIL Battalion from Singkep
3 x PBY-5 Catalina transporting 1 ML-KNIL Aviation  from Batavia
4 x Catalina I transporting Lark Force  from Rabaul
4 x Catalina I transporting Lark Force  from Rabaul
2 x S.23 Empire transporting Lark Force  from Rabaul
1 x S.23 Empire transporting Lark Force  from Rabaul
3 x Catalina I transporting 1 ML-KNIL Aviation  from Batavia
6 x Hudson I transporting supplies to Port Moresby
6 x Hudson I transporting supplies to Port Moresby
1 x DC-2 transporting supplies to Chengtu
TF 323 does not have enough fuel to complete mission
TF 341 leaving patrol zone to refuel at Singapore
TF 346 does not have enough fuel to complete mission
TF 351 does not have enough fuel to complete mission
TF 360 does not have enough fuel to complete mission
TF 362 does not have enough fuel to complete mission
TF 375 does not have enough fuel to complete mission
TF 388 does not have enough fuel to complete mission
TF 424 does not have enough fuel to complete mission
TF 433 does not have enough fuel to complete mission
TF 441 leaving patrol zone to refuel at Melbourne
TF 443 does not have enough fuel to complete mission
TF 444 does not have enough fuel to complete mission
TF 446 does not have enough fuel to complete mission
TF 447 does not have enough fuel to complete mission
TF 460 does not have enough fuel to complete mission
TF 475 does not have enough fuel to complete mission
TF 504 does not have enough fuel to complete mission
TF 510 does not have enough fuel to complete mission
TF 513 does not have enough fuel to complete mission
Japanese forces CAPTURE San Fernando !!!
Japanese forces CAPTURE Victoria Point !!!
Japanese forces CAPTURE Aparri !!!
Japanese forces CAPTURE Lashio !!!
Guam expands fortifications to size 1
Wake Island expands fortifications to size 2
Butuan expands fortifications to size 1
Bacolod expands fortifications to size 1
Toungoo expands fortifications to size 1
Ahmedabad expands fortifications to size 1
Diamond Harbour expands fortifications to size 1
Gorakhpur expands fortifications to size 1
Lucknow expands fortifications to size 1
Moradabad expands fortifications to size 1
Bourke expands fortifications to size 1
Kingscote expands fortifications to size 1
Port Kembla expands fortifications to size 1
Blenheim expands fortifications to size 1
Gisborne expands fortifications to size 1
Invercargill expands fortifications to size 1
Palmerston North expands fortifications to size 1
Waipapakauri expands fortifications to size 1
Ansi expands fortifications to size 1
VP-14 at Pearl Harbor takes PBY-5 Catalina replacements
VP-24 at Pearl Harbor takes PBY-5 Catalina replacements
VP-72 at San Diego takes PBY-5A Catalina replacements
18th PG/19th PS at Pearl Harbor takes P-40B Warhawk replacements
55th PG/54th PS at Ogden takes P-26A replacements
35th PG/HqS at San Diego takes P-40E Warhawk replacements
41st Australian Battalion - some devices have been upgraded at Sydney
Wyndham RAAF Base Force - some devices have been upgraded at Wyndham
1st RNZAF Base Force - some devices have been upgraded at Suva
Delayed planes for the aircraft pool:
  2 (16) x DC-2, 0 (7) x Hudson I, 2 (0) x Bolingbroke IV, 0 (2) x B-17D Fortress, 4 (15) x P-36A Mohawk
  5 (4) x P-39D Airacobra, 0 (1) x P-40B Warhawk, 2 (3) x P-43 Lancer, 1 (1) x OS2U-3 Kingfisher
  2 (4) x PBY-4 Catalina, 3 (0) x PBY-5 Catalina, 4 (7) x SB2U-3 Vindicator
BB Oklahoma available for service in damaged state at Pearl Harbor
CL St. Louis available for service in damaged state at Pearl Harbor
Device(s) upgraded in 3rd RAAF Base Force supplied from Bundaberg
Device(s) upgraded in 14th RAAF Base Force supplied from Townsville
Device(s) upgraded in 12th Australian Brigade supplied from Launceston
Low garrison leads to damage and 1 VP loss from partisan attack at Rangoon (54, 53)!
Low garrison leads to damage and 1 VP loss from partisan attack at Howrah (52, 36)!
Low garrison leads to damage and 1 VP loss from partisan attack at Sholapur (36, 29)!
Low garrison leads to damage and 1 VP loss from partisan attack at Hengyang (80, 53)!
Low garrison leads to damage and 1 VP loss from partisan attack at Loyang (87, 43)!
Low garrison leads to damage and 1 VP loss from partisan attack at Kweiteh (90, 46)!
Low garrison leads to damage and 1 VP loss from partisan attack at Nanning (72, 55)!
Low garrison leads to damage and 1 VP loss from partisan attack at Paoshan (65, 45)!
Low garrison leads to damage and 1 VP loss from partisan attack at Wenchow (89, 58)!
Low garrison leads to damage and 1 VP loss from partisan attack at Yenan (88, 37)!
Low garrison leads to damage and 1 VP loss from partisan attack at Bandoeng (50, 100)!
Low garrison leads to damage and 1 VP loss from partisan attack at Tingchow (83, 58)!
Parson, P. now available for reassignment
xAP Lurline arrives at San Diego
AK Betelgeuse arrives at San Diego
AK Arcturus arrives at San Diego
xAK Corrales arrives at Balboa
xAK Mahimahi arrives at Cristobal
Loss of xAKL Latouche on Dec 11, 1941 is admitted
xAP Esperance Bay arrives at Aden
Loss of xAP Glenapp on Dec 11, 1941 is admitted
AVG/1st Sqn arrives at Myitkyina
AVG/3rd Sqn arrives at Calcutta
VMF-121  arrives at San Diego
No.139 Sqn RAF arrives at Aden
No.146 Sqn RAF arrives at Chittagong
130th FA Rgt  arrives at Fort Ord
1st Indian Coastal Artillery Regiment arrives at Bombay
Provisional GMC Grp  arrives at Bataan
Planes and pilots of HK Flight RAF  withdrawing

===== section @47629 len=1199 =====
SIG INT REPORT FOR Dec 11, 41

2/16th Recon Regiment is planning for an attack on Mauban.
3/9th Infantry Regiment is planning for an attack on Lingayen.
Radio transmissions detected at 83,81.
Heavy Volume of Radio transmissions detected at Shanghai (92,55).
1st Formosa Inf. Regiment is planning for an attack on Lingayen.
Heavy Volume of Radio transmissions detected at Truk (112,108).
Radio transmissions detected at Kwajalein Island (132,115).
Radio transmissions detected at 85,85.
1/18th JAAF Base Force is loaded on a Japanese xAKL moving to Kota Bharu.
5/I./124th Infantry Battalion is loaded on a Ansyu-C Cargo class xAK moving to Kuantan.
2/11th Infantry Regiment is loaded on xAP Naminoue Maru moving to Kuantan.
23rd JAAF AF Bn  is located at Cam Ranh Bay(64,72).
2/56th Recon Regiment is loaded on a Japanese xAKL moving to Kota Bharu.
51st JAAF AF Bn  is located at Hailar(107,30).
23rd JAAF AF Bn  is located at Cam Ranh Bay(64,72).
Heavy Volume of Radio transmissions detected at Nagasaki/Sasebo (102,58).
17016 men are based at Kiamusze (113,40).
Radio transmissions detected at Amoy (83,61).
21st Infantry Regiment is loaded on xAP Huso Maru moving to Kuantan.
```

#### T6 `381d` — save date 12/13/41

Complete text of the record, all 4 sections, exactly as stored (section banners added by the extractor):

```text
===== section @0 len=33616 =====
AFTER ACTION REPORTS FOR Dec 12, 41
--------------------------------------------------------------------------------
Invasion Support action off Cagayan (79,89)
Defensive Guns engage approaching landing force
 
6 Coastal gun shots fired in defense.
 
Japanese Ships
      PB Showa Maru #5
      PB Showa Maru #3
      PB Nichi Maru #1
      PB Gamitsu Maru #1
      PB Saiko Maru
      PB Edo Maru
      xAP Haruna Maru
 
PB Showa Maru #5 fired at enemy troops
PB Showa Maru #3 fired at enemy troops
PB Nichi Maru #1 fired at enemy troops
PB Gamitsu Maru #1 fired at enemy troops
PB Saiko Maru fired at enemy troops
PB Edo Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Guam (106,95)
Defensive Guns engage approaching landing force
 
2 Coastal gun shots fired in defense.
 
Japanese Ships
      xAK Shinkoku Maru
 
xAK Shinkoku Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
ASW attack near Mersing  at 51,81
 
Japanese Ships
      DD Maikaze
      DD Hatsuyuki
      DD Hagikaze
      DD Arashi
 
Allied Ships
      SS K XI, hits 2
 
SS K XI launches 4 torpedoes at DD Maikaze
DD Hagikaze fails to find sub and abandons search
DD Arashi fails to find sub, continues to search...
DD Arashi fails to find sub, continues to search...
DD Arashi attacking submerged sub ....
DD Arashi fails to find sub, continues to search...
DD Arashi fails to find sub, continues to search...
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
Night Air attack on Singapore , at 50,84 
 
Weather in hex: Heavy cloud
 
Raid spotted at 79 NM, estimated altitude 15,000 feet.
Estimated time to target is 24 minutes
 
Japanese aircraft
      Ki-48-Ib Lily x 20
 
Allied aircraft
      no flights
 
Japanese aircraft losses
      Ki-48-Ib Lily: 4 damaged
      Ki-48-Ib Lily: 1 destroyed by flak
 
Allied aircraft losses
      Buffalo I: 1 damaged
      Blenheim IV: 2 damaged
 
Airbase hits 1
Airbase supply hits 1
Runway hits 1
 
Aircraft Attacking:
      12 x Ki-48-Ib Lily bombing from 11000 feet
               Airfield Attack:  4 x 100 kg GP Bomb
       6 x Ki-48-Ib Lily bombing from 11000 feet
               Port Attack:  4 x 100 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Night Air attack on Singapore , at 50,84 
 
Weather in hex: Heavy cloud
 
Raid detected at 33 NM, estimated altitude 15,000 feet.
Estimated time to target is 11 minutes
 
Japanese aircraft
      G4M1 Betty x 7
 
No Japanese losses
 
Aircraft Attacking:
       7 x G4M1 Betty bombing from 12000 feet
               Port Attack:  2 x 250 kg SAP Bomb,  4 x 60 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Night Air attack on Singapore , at 50,84 
 
Weather in hex: Heavy cloud
 
Raid detected at 12 NM, estimated altitude 14,000 feet.
Estimated time to target is 3 minutes
 
Japanese aircraft
      Ki-48-Ib Lily x 13
 
No Japanese losses
 
Allied Ships
      xAP Kedah, Bomb hits 1,  heavy fires
 
Aircraft Attacking:
      13 x Ki-48-Ib Lily bombing from 11000 feet
               Port Attack:  4 x 100 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Night Air attack on Singapore , at 50,84 
 
Weather in hex: Heavy cloud
 
Raid detected at 40 NM, estimated altitude 15,000 feet.
Estimated time to target is 12 minutes
 
Japanese aircraft
      Ki-48-Ib Lily x 11
 
Allied aircraft
      no flights
 
Japanese aircraft losses
      Ki-48-Ib Lily: 1 damaged
      Ki-48-Ib Lily: 1 destroyed by flak
 
Allied aircraft losses
      Blenheim I: 1 damaged
 
Runway hits 2
 
Aircraft Attacking:
      11 x Ki-48-Ib Lily bombing from 11000 feet
               Airfield Attack:  4 x 100 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Night Air attack on Singapore , at 50,84 
 
Weather in hex: Heavy cloud
 
Raid spotted at 22 NM, estimated altitude 14,000 feet.
Estimated time to target is 7 minutes
 
Japanese aircraft
      G4M1 Betty x 8
 
Japanese aircraft losses
      G4M1 Betty: 5 damaged
 
Aircraft Attacking:
       8 x G4M1 Betty bombing from 12000 feet
               Port Attack:  2 x 250 kg SAP Bomb,  4 x 60 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Sub attack near Bukit Besi  at 53,78
 
Japanese Ships
      xAKL Yoshinogawa Maru, Torpedo hits 3,  on fire,  heavy damage
      xAKL Kasui Maru
      PB Fuji Maru #4
 
Allied Ships
      SS K XIV, hits 3
 
SS K XIV launches 4 torpedoes at xAKL Yoshinogawa Maru
PB Fuji Maru #4 attacking submerged sub ....
PB Fuji Maru #4 fails to find sub, continues to search...
PB Fuji Maru #4 fails to find sub, continues to search...
PB Fuji Maru #4 fails to find sub, continues to search...
PB Fuji Maru #4 fails to find sub, continues to search...
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Mersing (51,82)
 
TF 82 troops unloading over beach at Mersing, 51,82
 
Japanese ground losses:
      Vehicles lost 1 (1 destroyed, 0 disabled)
 
12 Support troops accidentally lost during unload of 11th Shipping Engr Rgt
12 Support troops lost overboard during unload of 11th Shipping Engr Rgt
12 Aviation Support troops lost in surf during unload of 18th JAAF Base Force /3
Motorized Support dropped into water during unload of 18th JAAF Base Force /3
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Mersing (51,82)
 
TF 85 troops unloading over beach at Mersing, 51,82
 
 
 
--------------------------------------------------------------------------------
Invasion Support action off Cagayan (79,89)
Defensive Guns engage approaching landing force
 
5 Coastal gun shots fired in defense.
 
Japanese Ships
      DMS W-8
      DMS W-7
      xAP Haruna Maru
 
Allied ground losses:
      49 casualties reported
         Squads: 1 destroyed, 5 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
DMS W-8 fired at enemy troops
DMS W-7 fired at enemy troops
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Mersing (51,82)
 
TF 91 troops unloading over beach at Mersing, 51,82
 
Japanese ground losses:
      Guns lost 1 (1 destroyed, 0 disabled)
 
75mm T90 Field Gun lost in surf during unload of 5th FA Rgt /2
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Mersing (51,82)
 
TF 97 troops unloading over beach at Mersing, 51,82
 
Japanese ground losses:
      26 casualties reported
         Squads: 1 destroyed, 0 disabled
         Non Combat: 1 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
19 troops of a IJA Infantry Sqd (H) lost from landing craft during unload of 41st Infantry Rgt /3
12 Support troops lost in surf during unload of 42nd Infantry Rgt /6
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Mersing (51,82)
 
TF 100 troops unloading over beach at Mersing, 51,82
 
Japanese ground losses:
      26 casualties reported
         Squads: 1 destroyed, 0 disabled
         Non Combat: 9 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
      Vehicles lost 1 (1 destroyed, 0 disabled)
 
19 troops of a IJA Infantry Sqd (H) accidentally lost during unload of II./124th Infantry Bn
12 Aviation Support troops lost from landing craft during unload of 9th Base Force
12 Engineers accidentally lost during unload of 9th Base Force
12 Aviation Support troops lost overboard during unload of 9th Base Force
12 Support troops lost overboard during unload of 9th Base Force
12 Support troops lost from landing craft during unload of 9th Base Force
12 Support troops lost in surf during unload of 9th Base Force
12 Naval Support troops lost in surf during unload of 9th Base Force
12 Naval Support troops lost from landing craft during unload of 9th Base Force
Type 97 Medium Tank lost in surf during unload of 6th Tank Rgt
12 Support troops lost in surf during unload of 9th Base Force /2
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Naga (81,81)
 
TF 107 troops unloading over beach at Naga, 81,81
 
Japanese ground losses:
      184 casualties reported
         Squads: 10 destroyed, 0 disabled
         Non Combat: 3 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
10 troops of a IJA HMG Section accidentally lost during unload of 9th Infantry Rgt
10 troops of a IJA HMG Section accidentally lost during unload of 9th Infantry Rgt /2
19 troops of a IJA Infantry Sqd (H) accidentally lost during unload of 9th Infantry Rgt /2
19 troops of a IJA Infantry Sqd (H) lost overboard during unload of 20th Infantry Rgt
19 troops of a IJA Infantry Sqd (H) lost overboard during unload of 20th Infantry Rgt
19 troops of a IJA Infantry Sqd (H) lost overboard during unload of 20th Infantry Rgt
12 Support troops lost in surf during unload of 20th Infantry Rgt
12 Support troops lost from landing craft during unload of 20th Infantry Rgt
16 troops of a IJA Engineer Squad accidentally lost during unload of 16th Engr Rgt
12 Support troops lost overboard during unload of 9th Infantry Rgt /3
19 troops of a IJA Infantry Sqd (H) lost overboard during unload of 20th Infantry Rgt /5
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Mersing (51,82)
 
TF 108 troops unloading over beach at Mersing, 51,82
 
Japanese ground losses:
      210 casualties reported
         Squads: 10 destroyed, 0 disabled
         Non Combat: 12 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
      Guns lost 2 (2 destroyed, 0 disabled)
 
19 troops of a IJA Infantry Sqd (H) lost in surf during unload of 21st Infantry Rgt
12 Support troops accidentally lost during unload of 21st Infantry Rgt
19 troops of a IJA Infantry Sqd (H) lost in surf during unload of 21st Infantry Rgt /2
12 Support troops lost from landing craft during unload of 21st Infantry Rgt /2
13.2mm T93 AAMG lost overboard during unload of 34th Fld AA Bn
16 troops of a IJA Engineer Squad lost overboard during unload of 15th Ind. Engr Rgt
12 Engineers lost overboard during unload of 15th Ind. Engr Rgt
12 Engineers lost overboard during unload of 15th Ind. Engr Rgt
12 Engineers lost overboard during unload of 23rd Ind. Engr Rgt
12 Support troops accidentally lost during unload of 23rd Ind. Engr Rgt
37mm Type 94 AT Gun lost overboard during unload of 55th Infantry Rgt
12 Support troops lost overboard during unload of 55th Infantry Rgt
19 troops of a IJA Infantry Sqd (H) lost overboard during unload of 55th Infantry Rgt /2
12 Support troops lost overboard during unload of 55th Infantry Rgt /2
19 troops of a IJA Infantry Sqd (H) lost overboard during unload of 114th Infantry Rgt /1
19 troops of a IJA Infantry Sqd (H) lost overboard during unload of 114th Infantry Rgt /1
19 troops of a IJA Infantry Sqd (H) accidentally lost during unload of 114th Infantry Rgt /1
12 Support troops accidentally lost during unload of 114th Infantry Rgt /1
19 troops of a IJA Infantry Sqd (H) accidentally lost during unload of 114th Infantry Rgt
12 Support troops lost from landing craft during unload of 114th Infantry Rgt
12 Support troops lost overboard during unload of 114th Infantry Rgt
12 Support troops lost overboard during unload of 114th Infantry Rgt
 
 
--------------------------------------------------------------------------------
Invasion Support action off Guam (106,95)
Defensive Guns engage approaching landing force
 
3 Coastal gun shots fired in defense.
 
Japanese Ships
      xAK Shinkoku Maru
 
xAK Shinkoku Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Cagayan (79,89)
Defensive Guns engage approaching landing force
 
11 Coastal gun shots fired in defense.
 
Japanese Ships
      DMS W-8
      DMS W-7
      xAP Haruna Maru
 
Allied ground losses:
      33 casualties reported
         Squads: 0 destroyed, 4 disabled
         Non Combat: 0 destroyed, 1 disabled
         Engineers: 0 destroyed, 0 disabled
 
DMS W-8 fired at enemy troops
DMS W-7 fired at enemy troops
 
 
--------------------------------------------------------------------------------
Invasion Support action off Guam (106,95)
Defensive Guns engage approaching landing force
 
2 Coastal gun shots fired in defense.
 
Japanese Ships
      xAK Shinkoku Maru
 
xAK Shinkoku Maru fired at enemy troops
 
 
--------------------------------------------------------------------------------
ASW attack near Kuantan  at 52,79
 
Japanese Ships
      xAKL Meiko Maru
      PB Fuji Maru #4
 
Allied Ships
      SS K XIII
 
SS K XIII is sighted by escort
K XIII bottoming out ....
PB Fuji Maru #4 fails to find sub, continues to search...
PB Fuji Maru #4 fails to find sub, continues to search...
PB Fuji Maru #4 fails to find sub, continues to search...
PB Fuji Maru #4 fails to find sub, continues to search...
PB Fuji Maru #4 fails to find sub, continues to search...
Escort abandons search for sub
 
 
--------------------------------------------------------------------------------
TF 10 encounters mine field at Soc Trang (58,73)
 
Japanese Ships
      PB Shuko Maru #2
 
1 mine cleared
 
 
--------------------------------------------------------------------------------
Morning Air attack on Manila , at 79,77 
 
Weather in hex: Light cloud
 
Raid detected at 26 NM, estimated altitude 25,000 feet.
Estimated time to target is 8 minutes
 
Japanese aircraft
      A6M2 Zero x 44
 
Allied aircraft
      P-40B Warhawk x 6
      P-40E Warhawk x 27
 
Japanese aircraft losses
      A6M2 Zero: 3 destroyed
 
Allied aircraft losses
      P-40B Warhawk: 2 destroyed
      P-40E Warhawk: 5 destroyed
 
Aircraft Attacking:
      26 x A6M2 Zero sweeping at 20000 feet
 
CAP engaged:
24th PG/3rd PS with P-40E Warhawk (0 airborne, 11 on standby, 0 scrambling)
      11 plane(s) intercepting now.
      0 plane(s) not yet engaged, 3 being recalled, 2 out of immediate contact.
      Group patrol altitude is 20000 
      Time for all group planes to reach interception is 30 minutes
24th PG/17th PS with P-40E Warhawk (1 airborne, 2 on standby, 3 scrambling)
      3 plane(s) intercepting now.
      0 plane(s) not yet engaged, 0 being recalled, 1 out of immediate contact.
      Group patrol altitude is 10000 , scrambling fighters between 10000 and 19000.
      Time for all group planes to reach interception is 31 minutes
24th PG/20th PS with P-40B Warhawk (0 airborne, 3 on standby, 0 scrambling)
      3 plane(s) intercepting now.
      0 plane(s) not yet engaged, 1 being recalled, 2 out of immediate contact.
      Group patrol altitude is 10000 , scrambling fighters to 23000.
      Time for all group planes to reach interception is 44 minutes
35th PG/21st PS with P-40E Warhawk (1 airborne, 2 on standby, 0 scrambling)
      3 plane(s) intercepting now.
      0 plane(s) not yet engaged, 1 being recalled, 0 out of immediate contact.
      Group patrol altitude is 10000 
      Time for all group planes to reach interception is 3 minutes
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on 58th Chinese Corps, at 84,52 , near Wuchang
 
Weather in hex: Clear sky
 
Raid spotted at 19 NM, estimated altitude 14,000 feet.
Estimated time to target is 5 minutes
 
Japanese aircraft
      Ki-27b Nate x 11
      Ki-30 Ann x 7
 
No Japanese losses
 
Aircraft Attacking:
       7 x Ki-30 Ann bombing from 9000 feet
               Ground Attack:  1 x 250 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on 31st Group Army, at 86,46 , near Nanyang
 
Weather in hex: Overcast
 
Raid spotted at 17 NM, estimated altitude 13,000 feet.
Estimated time to target is 5 minutes
 
Japanese aircraft
      Ki-51 Sonia x 19
 
No Japanese losses
 
Allied ground losses:
      62 casualties reported
         Squads: 0 destroyed, 2 disabled
         Non Combat: 0 destroyed, 5 disabled
         Engineers: 0 destroyed, 0 disabled
 
Aircraft Attacking:
      19 x Ki-51 Sonia bombing from 8000 feet
               Ground Attack:  4 x 50 kg GP Bomb
 
Also attacking 29th Chinese Corps ...
Also attacking 31st Group Army ...
Also attacking 29th Chinese Corps ...
Also attacking 31st Group Army ...
 
 
--------------------------------------------------------------------------------
Morning Air attack on 48th Chinese Corps, at 88,50 , near Hwainan
 
Weather in hex: Overcast
 
Raid spotted at 16 NM, estimated altitude 15,000 feet.
Estimated time to target is 7 minutes
 
Japanese aircraft
      Ki-36 Ida x 21
 
No Japanese losses
 
Allied ground losses:
      105 casualties reported
         Squads: 0 destroyed, 12 disabled
         Non Combat: 0 destroyed, 2 disabled
         Engineers: 0 destroyed, 0 disabled
 
Aircraft Attacking:
      21 x Ki-36 Ida bombing from 10000 feet
               Ground Attack:  4 x 30 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on 64th Chinese Corps, at 72,57 , near Pakhoi
 
Weather in hex: Severe storms
 
Raid spotted at 20 NM, estimated altitude 10,000 feet.
Estimated time to target is 6 minutes
 
Japanese aircraft
      Ki-51 Sonia x 6
 
No Japanese losses
 
Allied ground losses:
      19 casualties reported
         Squads: 1 destroyed, 0 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
Aircraft Attacking:
       6 x Ki-51 Sonia bombing from 6000 feet
               Ground Attack:  4 x 50 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Morning Air attack on Rangoon , at 54,53 
 
Weather in hex: Heavy rain
 
Raid spotted at 24 NM, estimated altitude 13,000 feet.
Estimated time to target is 6 minutes
 
Japanese aircraft
      Ki-21-IIa Sally x 29
      Ki-43-Ib Oscar x 23
 
No Japanese losses
 
Airbase hits 5
Airbase supply hits 2
Runway hits 14
 
Aircraft Attacking:
      29 x Ki-21-IIa Sally bombing from 10000 feet
               Airfield Attack:  4 x 250 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Afternoon Air attack on 100th Chinese Corps, at 88,57 , near Wenchow
 
Weather in hex: Moderate rain
 
Raid spotted at 15 NM, estimated altitude 11,000 feet.
Estimated time to target is 4 minutes
 
Japanese aircraft
      B5M1 Mabel x 5
 
No Japanese losses
 
Aircraft Attacking:
       5 x B5M1 Mabel bombing from 6000 feet
               Ground Attack:  2 x 250 kg GP Bomb
 
 
 
--------------------------------------------------------------------------------
Submarine attack near Kota Bharu  at 52,75
 
Japanese Ships
      xAKL Matsutan Maru, Torpedo hits 1,  on fire,  heavy damage
 
Allied Ships
      SS K XII
 
xAKL Matsutan Maru is sighted by SS K XII
SS K XII launches 2 torpedoes
 
 
--------------------------------------------------------------------------------
TF 82 encounters mine field at Mersing (51,82)
 
 
 
--------------------------------------------------------------------------------
Invasion Support action off Cagayan (79,89)
Defensive Guns engage approaching landing force
 
7 Coastal gun shots fired in defense.
 
Japanese Ships
      DMS W-8
      DMS W-7
      xAP Haruna Maru
 
Allied ground losses:
      16 casualties reported
         Squads: 0 destroyed, 1 disabled
         Non Combat: 0 destroyed, 1 disabled
         Engineers: 0 destroyed, 0 disabled
 
DMS W-8 fired at enemy troops
DMS W-7 fired at enemy troops
 
 
--------------------------------------------------------------------------------
TF 100 encounters mine field at Mersing (51,82)
 
 
 
--------------------------------------------------------------------------------
Amphibious Assault at Mersing (51,82)
 
TF 105 troops unloading over beach at Mersing, 51,82
 
 
 
--------------------------------------------------------------------------------
 
Ground combat at 83,52 (near Changsha)
 
Japanese Bombardment attack
 
Attacking force 11465 troops, 102 guns, 0 vehicles, Assault Value = 547
 
Defending force 18420 troops, 115 guns, 0 vehicles, Assault Value = 476
 
Allied ground losses:
      18 casualties reported
         Squads: 0 destroyed, 2 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
Assaulting units:
    6th Division
    18th Ind.Mixed Brigade
 
Defending units:
    37th Chinese Corps
    20th Chinese Corps
    58th Chinese Corps
 
 
--------------------------------------------------------------------------------
 
Ground combat at 84,49 (near Hankow)
 
Japanese Deliberate attack
 
Attacking force 16865 troops, 120 guns, 42 vehicles, Assault Value = 624
 
Defending force 10629 troops, 75 guns, 0 vehicles, Assault Value = 234
 
Japanese adjusted assault: 323 
 
Allied adjusted defense: 32 
 
Japanese assault odds: 10 to 1 
 
Combat modifiers
Defender: op mode(-), leaders(+), experience(-)
Attacker: 
 
Japanese ground losses:
      69 casualties reported
         Squads: 1 destroyed, 10 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
Allied ground losses:
      4260 casualties reported
         Squads: 126 destroyed, 117 disabled
         Non Combat: 163 destroyed, 0 disabled
         Engineers: 6 destroyed, 3 disabled
      Guns lost 23 (14 destroyed, 9 disabled)
      Units retreated 1
 
Defeated Allied Units Retreating!
 
Assaulting units:
    11th RGC Temp./B Division
    34th Division
    13th RGC Temp. Division
    11th RGC Temp./C Division
 
Defending units:
    30th Chinese Corps
 
 
--------------------------------------------------------------------------------
 
Ground combat at 88,50 (near Hwainan)
 
Japanese Deliberate attack
 
Attacking force 11287 troops, 102 guns, 0 vehicles, Assault Value = 363
 
Defending force 4715 troops, 43 guns, 0 vehicles, Assault Value = 150
 
Japanese adjusted assault: 183 
 
Allied adjusted defense: 16 
 
Japanese assault odds: 11 to 1 
 
Combat modifiers
Defender: leaders(+), morale(-), experience(-)
Attacker: leaders(+), leaders(-)
 
Japanese ground losses:
      89 casualties reported
         Squads: 0 destroyed, 9 disabled
         Non Combat: 0 destroyed, 2 disabled
         Engineers: 0 destroyed, 2 disabled
 
Allied ground losses:
      1579 casualties reported
         Squads: 81 destroyed, 42 disabled
         Non Combat: 45 destroyed, 0 disabled
         Engineers: 4 destroyed, 3 disabled
      Guns lost 16 (5 destroyed, 11 disabled)
      Units retreated 1
 
Defeated Allied Units Retreating!
 
Assaulting units:
    116th Division
 
Defending units:
    48th Chinese Corps
 
 
--------------------------------------------------------------------------------
 
Ground combat at Hong Kong (77,61)
 
Japanese Deliberate attack
 
Attacking force 30130 troops, 408 guns, 218 vehicles, Assault Value = 857
 
Defending force 6954 troops, 131 guns, 80 vehicles, Assault Value = 177
 
Japanese engineers reduce fortifications to 1
 
Japanese adjusted assault: 620 
 
Allied adjusted defense: 393 
 
Japanese assault odds: 1 to 1 (fort level 1) 
 
Japanese Assault reduces fortifications to 1
 
Combat modifiers
Defender: terrain(+), leaders(-), experience(-)
Attacker: 
 
Japanese ground losses:
      892 casualties reported
         Squads: 4 destroyed, 90 disabled
         Non Combat: 1 destroyed, 4 disabled
         Engineers: 1 destroyed, 17 disabled
 
Allied ground losses:
      472 casualties reported
         Squads: 19 destroyed, 33 disabled
         Non Combat: 2 destroyed, 13 disabled
         Engineers: 0 destroyed, 2 disabled
      Vehicles lost 3 (1 destroyed, 2 disabled)
 
Assaulting units:
    67th Ind.Infantry Battalion
    1st Ind.Inf.Group 
    68th Ind.Infantry Battalion
    38th Division
    19th Ind. Engineer Regiment
    20th Ind. Engineer Regiment
    66th Infantry Regiment
    20th RGC Division
    1st Hvy.Artillery Regiment
    23rd Army
    5th RF Gun Battalion
    20th Ind. Mtn Gun Battalion
    2nd RF Gun Battalion
    21st Mortar Battalion
    10th Ind. Mountain Gun Regiment
    2nd Ind.Hvy.Art. Battalion
    2nd Mortar Battalion
    47th JAAF AF Bn 
    3rd Ind.Hvy.Art. Battalion
    1st JAAF AF Coy 
 
Defending units:
    Rifles of Canada Battalion
    Winnipeg Grenadiers Battalion
    1st Middlesex Battalion
    Kowloon Brigade
    102nd RN Base Force
    Hong Kong Fortress
 
 
--------------------------------------------------------------------------------
 
Ground combat at 89,40 (near Tsingsing)
 
Japanese Deliberate attack
 
Attacking force 17095 troops, 179 guns, 68 vehicles, Assault Value = 502
 
Defending force 7049 troops, 54 guns, 0 vehicles, Assault Value = 181
 
Japanese adjusted assault: 266 
 
Allied adjusted defense: 184 
 
Japanese assault odds: 1 to 1 
 
Combat modifiers
Defender: terrain(+), morale(-), experience(-)
Attacker: 
 
Japanese ground losses:
      209 casualties reported
         Squads: 1 destroyed, 26 disabled
         Non Combat: 0 destroyed, 2 disabled
         Engineers: 0 destroyed, 1 disabled
 
Allied ground losses:
      397 casualties reported
         Squads: 25 destroyed, 15 disabled
         Non Combat: 1 destroyed, 1 disabled
         Engineers: 0 destroyed, 0 disabled
      Guns lost 8 (2 destroyed, 6 disabled)
 
Assaulting units:
    37th Division
    4th Ind.Mixed Brigade
    6th Medium Field Artillery Regiment
 
Defending units:
    1/19th Chinese Corps
    33rd Chinese Corps
    7th Group Army
 
 
--------------------------------------------------------------------------------
 
Ground combat at Naga (81,81)
 
Japanese Deliberate attack
 
Attacking force 3780 troops, 29 guns, 0 vehicles, Assault Value = 427
 
Defending force 0 troops, 0 guns, 0 vehicles, Assault Value = 0
 
Japanese adjusted assault: 89 
 
Allied adjusted defense: 1 
 
Japanese assault odds: 89 to 1 (fort level 0) 
 
Japanese forces CAPTURE Naga !!!
 
Combat modifiers
Attacker: 
 
Assaulting units:
    Tanaka Det 
    16th Recon Regiment
    20th Infantry Regiment
    9th Infantry Regiment
    16th Engineer Regiment
    40th JAAF AF Bn 
    28th JAAF AF Bn /1
 
 
--------------------------------------------------------------------------------
 
Ground combat at Cagayan (79,89)
 
Japanese Deliberate attack
 
Attacking force 4345 troops, 51 guns, 0 vehicles, Assault Value = 145
 
Defending force 2742 troops, 4 guns, 49 vehicles, Assault Value = 58
 
Japanese adjusted assault: 110 
 
Allied adjusted defense: 25 
 
Japanese assault odds: 4 to 1 (fort level 0) 
 
Japanese forces CAPTURE Cagayan !!!
 
Combat modifiers
Defender: terrain(+), leaders(+), preparation(-), fatigue(-)
 experience(-)
Attacker: 
 
Japanese ground losses:
      10 casualties reported
         Squads: 0 destroyed, 1 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
Allied ground losses:
      616 casualties reported
         Squads: 37 destroyed, 3 disabled
         Non Combat: 70 destroyed, 2 disabled
         Engineers: 0 destroyed, 0 disabled
      Vehicles lost 29 (29 destroyed, 0 disabled)
      Units retreated 3
 
Defeated Allied Units Retreating!
 
Assaulting units:
    Kimura Det 
    Kure 1st SNLF 
    1st Naval Construction Battalion
 
Defending units:
    3rd PA Constabulary Regiment
    Cagayan USAAF Base Force
    Mindanao Force Corps
 
 
--------------------------------------------------------------------------------
 
Ground combat at Hollandia (93,116)
 
Japanese Deliberate attack
 
Attacking force 386 troops, 0 guns, 0 vehicles, Assault Value = 5
 
Defending force 0 troops, 0 guns, 0 vehicles, Assault Value = 0
 
Japanese adjusted assault: 4 
 
Allied adjusted defense: 1 
 
Japanese assault odds: 4 to 1 (fort level 1) 
 
Japanese forces CAPTURE Hollandia !!!
 
Combat modifiers
Attacker: leaders(-)
 
Assaulting units:
    1st JNAF Coy 
 
 
--------------------------------------------------------------------------------
 
Ground combat at Guam (106,95)
 
Japanese Deliberate attack
 
Attacking force 5106 troops, 21 guns, 0 vehicles, Assault Value = 51
 
Defending force 894 troops, 0 guns, 32 vehicles, Assault Value = 9
 
Japanese adjusted assault: 35 
 
Allied adjusted defense: 27 
 
Japanese assault odds: 1 to 1 (fort level 1) 
 
Japanese Assault reduces fortifications to 0
 
Combat modifiers
Defender: terrain(+), leaders(+), preparation(-)
Attacker: 
 
Japanese ground losses:
      48 casualties reported
         Squads: 0 destroyed, 4 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
Allied ground losses:
      24 casualties reported
         Squads: 0 destroyed, 0 disabled
         Non Combat: 0 destroyed, 2 disabled
         Engineers: 0 destroyed, 2 disabled
 
Assaulting units:
    II/81st Naval Guard Unit
    5th Base Force
 
Defending units:
    Guam USN Base Force
 
 
--------------------------------------------------------------------------------
 
Ground combat at Hong Kong (77,61)
 
Allied Bombardment attack
 
Attacking force 954 troops, 74 guns, 0 vehicles, Assault Value = 131
 
Defending force 31753 troops, 408 guns, 278 vehicles, Assault Value = 783
 
Assaulting units:
    1st Middlesex Battalion
    Rifles of Canada Battalion
    Winnipeg Grenadiers Battalion
    Kowloon Brigade
    102nd RN Base Force
    Hong Kong Fortress
 
Defending units:
    67th Ind.Infantry Battalion
    38th Division
    1st Ind.Inf.Group 
    66th Infantry Regiment
    68th Ind.Infantry Battalion
    19th Ind. Engineer Regiment
    20th Ind. Engineer Regiment
    20th RGC Division
    2nd Mortar Battalion
    1st Hvy.Artillery Regiment
    20th Ind. Mtn Gun Battalion
    21st Mortar Battalion
    3rd Ind.Hvy.Art. Battalion
    23rd Army
    2nd Ind.Hvy.Art. Battalion
    5th RF Gun Battalion
    10th Ind. Mountain Gun Regiment
    47th JAAF AF Bn 
    2nd RF Gun Battalion
    1st JAAF AF Coy 
 
 
--------------------------------------------------------------------------------
 
Ground combat at 90,56 (near Hangchow)
 
Japanese Deliberate attack
 
Attacking force 7585 troops, 74 guns, 29 vehicles, Assault Value = 258
 
Defending force 4807 troops, 43 guns, 0 vehicles, Assault Value = 159
 
Japanese adjusted assault: 181 
 
Allied adjusted defense: 109 
 
Japanese assault odds: 1 to 1 
 
Combat modifiers
Defender: leaders(+), experience(-)
Attacker: 
 
Japanese ground losses:
      167 casualties reported
         Squads: 0 destroyed, 12 disabled
         Non Combat: 0 destroyed, 2 disabled
         Engineers: 0 destroyed, 1 disabled
      Guns lost 2 (1 destroyed, 1 disabled)
 
Allied ground losses:
      532 casualties reported
         Squads: 4 destroyed, 75 disabled
         Non Combat: 0 destroyed, 2 disabled
         Engineers: 0 destroyed, 4 disabled
 
Assaulting units:
    22nd/A Division
    22nd/B Division
 
Defending units:
    88th Chinese Corps
 
 
--------------------------------------------------------------------------------
 
Ground combat at Alor Star (49,73)
 
Japanese Deliberate attack
 
Attacking force 475 troops, 2 guns, 33 vehicles, Assault Value = 63
 
Defending force 4650 troops, 20 guns, 2 vehicles, Assault Value = 207
 
Japanese adjusted assault: 28 
 
Allied adjusted defense: 27 
 
Japanese assault odds: 1 to 1 (fort level 1) 
 
Japanese Assault reduces fortifications to 0
 
Combat modifiers
Defender: terrain(+), op mode(-), preparation(-), experience(-)
Attacker: 
 
Japanese ground losses:
      24 casualties reported
         Squads: 0 destroyed, 2 disabled
         Non Combat: 0 destroyed, 0 disabled
         Engineers: 0 destroyed, 0 disabled
 
Allied ground losses:
      61 casualties reported
         Squads: 0 destroyed, 9 disabled
         Non Combat: 0 destroyed, 1 disabled
         Engineers: 0 destroyed, 0 disabled
 
Assaulting units:
    5th Recon Rgt /2
    1st Tank Rgt /1
 
Defending units:
    6th Indian Brigade
    15th Indian Brigade
 
 

===== section @33620 len=8936 =====
COMBAT EVENTS FOR 12/12/41

ADJUST TASK FORCE MISSIONS 
CALCULATE RANGE TO ENEMY
CALCULATE AIR SUPERIORITY
ASSIGN COMPUTER CONTROLLED UNITS
Wuchow is occupied by the Japanese
Combat Events:
NAVAL REACTION PHASE
LOADING/UNLOADING TASK FORCES
Amphibious TF 87 unloading at Naga
Amphibious TF 89 unloading at Cagayan
Amphibious TF 112 unloading at Hollandia
Amphibious TF 129 unloading at Guam
TASK FORCE LOADING/UNLOADING COMPLETE
CALCULATING TASK FORCE PATHS
MINESWEEPING OPERATIONS
NAVAL MOVEMENT PHASE
Task Force 202 has merged with TF 201 at 50 , 89
ADJUST SHIP FUEL
NIGHT AIR OPERATIONS PHASE
AIR NIGHT STRIKES
AIRCRAFT LANDING
SHIP CREWS PERFORM DAMAGE CONTROL
SURFACE COMBAT CHECK
DIVIDE CRIPPLED TFs
SHIP CREWS PERFORM DAMAGE CONTROL
SHIP CREWS PERFORM DAMAGE CONTROL
AVD William B. Preston sinks....
Amphibious TF 82 offshore of Mersing
Amphibious TF 85 offshore of Mersing
Amphibious TF 87 unloading at Naga
Amphibious TF 89 unloading at Cagayan
Amphibious TF 91 offshore of Mersing
Amphibious TF 97 offshore of Mersing
Amphibious TF 100 offshore of Mersing
Amphibious TF 107 offshore of Naga
Amphibious TF 108 offshore of Mersing
Amphibious TF 129 unloading at Guam
REPAIRING SHIPS
END 12 HOUR PULSE
DIVIDE CRIPPLED TFs
CHECKING TF FUEL
TF'S RETURN TO PORT
Combat Events:
NAVAL REACTION PHASE
LOADING/UNLOADING TASK FORCES
Amphibious TF 100 unloading at Mersing
Amphibious TF 107 unloading at Naga
Amphibious TF 108 unloading at Mersing
Amphibious TF 82 unloading at Mersing
Amphibious TF 85 unloading at Mersing
Amphibious TF 87 unloading at Naga
Amphibious TF 89 unloading at Cagayan
Amphibious TF 91 unloading at Mersing
Amphibious TF 97 unloading at Mersing
Amphibious TF 129 unloading at Guam
TASK FORCE LOADING/UNLOADING COMPLETE
CALCULATING TASK FORCE PATHS
MINESWEEPING OPERATIONS
NAVAL MOVEMENT PHASE
ADJUST SHIP FUEL
AIR OPERATIONS PHASE : AM
TF 445 observes H6K4 Mavis at 71,89 near Tawi Tawi
TF 301 shadowed by Japanese Recon at 44,70 near Sabang
TF 341 observes Japanese Level Bomber at 52,84 near Tioman Island
PBY-4 Catalina sighting report: 6 Japanese ships at 79,89 near Cagayan , Speed unknown 
PBY-4 Catalina sighting report: Japanese DD at 83,86 near Guiuan , Speed unknown  
PBY-4 Catalina sighting report: 3 Japanese ships at 81,87 near Dinagat , Speed 10 , Moving West
TF 252 followed by Japanese Float Fighter at 84,87 near Dinagat
SS Tarpon snooped by Japanese Float Plane at 84,88 near Siargao
TF 342 detects Japanese Recon at 73,85 near Puerto Princesa
TF 338 observes Japanese Fighter Bomber at 50,84 near Singapore
Search E13A1 Jake destroyed by CAP
MTB 11 observes Japanese Float Fighter at 73,71 near Paracel Islands
TF 340 followed by Japanese Float Fighter at 49,88 near Singkep
Search E7K2 Alf destroyed by CAP at (50,84)
CM Kung Wo shadowed by Japanese Torpedo Bomber at 50,89 near Muntok
TF 340 sighted by Japanese Torpedo Bomber at 49,88 near Singkep
TF 443 sighted by Japanese Recon at 75,81 near Taytay
SBD-2 Dauntless sighting report: 6 Japanese ships at 140,156 near Hoorn Islands, speed 13, Moving Southwest
SOC-1 Seagull sighting report: Japanese SS at 149,137 near Baker Island , Speed 14 , Moving East 
139WH-3 sighting report: Japanese SS at 49,97 near Batavia , Speed 20 , Moving Southwest 
139WH-3 attacking a Japanese SS at 49,97
Do-24K-1 sighting report: 2 Japanese ships at 51,82 near Mersing , Speed 29 , Moving Southwest
Do-24K-1 sighting report: 5 Japanese ships at 51,82 near Mersing , Speed unknown 
Blenheim I sighting report: 10 Japanese ships at 51,82 near Mersing , Speed unknown 
Blenheim I sighting report: 10 Japanese ships at 51,82 near Mersing , Speed unknown 
Blenheim I sighting report: 4 Japanese ships at 51,82 near Mersing , Speed unknown 
Blenheim I sighting report: 9 Japanese ships at 51,82 near Mersing , Speed 2 , Moving Southwest
Blenheim I sighting report: 10 Japanese ships at 51,82 near Mersing , Speed unknown 
Blenheim I sighting report: 5 Japanese ships at 51,82 near Mersing , Speed unknown 
Blenheim I sighting report: 5 Japanese ships at 51,82 near Mersing , Speed unknown 
AIR STRIKES
AIRCRAFT LANDING
SHIP CREWS PERFORM DAMAGE CONTROL
DIVIDE CRIPPLED TFs
AIR OPERATIONS PHASE : PM
PBY-4 Catalina sighting report: 7 Japanese ships at 83,80 near Catanduanes , Speed unknown 
PBY-4 Catalina sighting report: 5 Japanese ships at 79,89 near Cagayan , Speed unknown 
PBY-4 Catalina sighting report: 2 Japanese ships at 87,94 near Babeldaob , Speed 13 , Moving Southeast
PBY-4 Catalina sighting report: 3 Japanese ships at 79,89 near Cagayan , Speed unknown 
TF 342 sights E13A1 Jake at 73,85 near Puerto Princesa
F1M2 Pete has spotted a Admiralty HDML class HDML at 52,84
TF 448 sighted by Japanese Fighter Bomber at 50,84 near Singapore
Search E13A1 Jake destroyed by CAP at (50,84)
TF 443 followed by E13A1 Jake at 75,81 near Taytay
SBD-2 Dauntless reports periscope at 143, 155 near Wallis Island
Do-24K-1 sighting report: 7 Japanese ships at 51,79 near Kuantan , Speed 4 , Moving West
Blenheim I sighting report: 9 Japanese ships at 51,82 near Mersing , Speed unknown 
Blenheim I sighting report: 7 Japanese ships at 51,79 near Kuantan , Speed 4 , Moving West
Blenheim I sighting report: 7 Japanese ships at 51,82 near Mersing , Speed unknown 
AM Penguin sights Japanese Aircraft at 100,117 near Manus
AIR STRIKES
AIRCRAFT LANDING
AIR TRANSPORT PHASE
AIRCRAFT LANDING
AVG/3rd Sqn sub-groups COMBINING at Calcutta
SHIP CREWS PERFORM DAMAGE CONTROL
SURFACE COMBAT CHECK
DIVIDE CRIPPLED TFs
SHIP CREWS PERFORM DAMAGE CONTROL
SHIP CREWS PERFORM DAMAGE CONTROL
Amphibious TF 82 unloading at Mersing
Amphibious TF 87 unloading at Naga
Amphibious TF 89 unloading at Cagayan
Amphibious TF 91 unloading at Mersing
Amphibious TF 97 unloading at Mersing
Amphibious TF 100 unloading at Mersing
Amphibious TF 105 offshore of Mersing
Amphibious TF 107 unloading at Naga
Amphibious TF 108 unloading at Mersing
Amphibious TF 129 unloading at Guam
REPAIRING SHIPS
END 12 HOUR PULSE
DIVIDE CRIPPLED TFs
CHECKING TF FUEL
TF'S RETURN TO PORT
30th Chinese Corps RETREATING towards Nanyang
48th Chinese Corps RETREATING towards Nanyang
Japanese forces CAPTURE Naga !!!
Japanese forces CAPTURE Cagayan !!!
3rd PA Constabulary Regiment RETREATING towards Malaybalay
Cagayan USAAF Base Force RETREATING towards Malaybalay
Mindanao Force Corps RETREATING towards Malaybalay
Japanese forces CAPTURE Hollandia !!!
RECALCULATING ZONES OF CONTROL
SUPPLY REQUIREMENTS PHASE
Midway Island expands fortifications to size 1
Guam expands fortifications to size 1
Clark Field expands fortifications to size 2
Iba expands fortifications to size 1
Singapore expands fortifications to size 1
Amritsar expands fortifications to size 1
Edmonton expands fortifications to size 1
MOVING OIL AND RESOURCES
PRODUCTION PHASE
SUPPLY MOVEMENT PHASE
CALCULATING SPOILAGE
AIRCRAFT REPAIR PHASE
AIRCRAFT SUPPLY PHASE
UPGRADING SHIPS
UPGRADING AIR UNITS
UPGRADING LAND UNITS
SUPPLY PHASE COMPLETE
END OF DAY
TRAINING SHIP CREWS
DISBANDING HUMAN CONTROLLED TASK FORCES
ADJUSTING AVIATION SUPPORT
LAND UNIT REPLACEMENT PHASE
AIR UNIT REPLACEMENT PHASE
MERGING SUB UNITS
SUB UNIT MERGE COMPLETE
DIVIDE CRIPPLED TFs
Low garrison leads to damage and 1 VP loss from partisan attack at Foochow (86, 60)!
Low garrison leads to damage and 1 VP loss from partisan attack at Rangoon (54, 53)!
Low garrison leads to damage and 1 VP loss from partisan attack at Bombay (36, 24)!
Low garrison leads to damage and 1 VP loss from partisan attack at Chungking (76, 45)!
Low garrison leads to damage and 1 VP loss from partisan attack at Loyang (87, 43)!
Low garrison leads to damage and 1 VP loss from partisan attack at Kweiteh (90, 46)!
Low garrison leads to damage and 1 VP loss from partisan attack at Nanning (72, 55)!
Low garrison leads to damage and 1 VP loss from partisan attack at Pakhoi (72, 58)!
Low garrison leads to damage and 1 VP loss from partisan attack at Paoshan (65, 45)!
Low garrison leads to damage and 1 VP loss from partisan attack at Tsiaotso (88, 42)!
Low garrison leads to damage and 1 VP loss from partisan attack at Wenchow (89, 58)!
Low garrison leads to damage and 1 VP loss from partisan attack at Yenan (88, 37)!
Low garrison leads to damage and 1 VP loss from partisan attack at Bandoeng (50, 100)!
Low garrison leads to damage and 1 VP loss from partisan attack at Tingchow (83, 58)!
LEADER REASSIGNMENT PHASE
No.14 Sqn RAAF arrives at Townsville
71st OG/110th OS arrives at San Francisco
AHQ Far East  arrives at Singapore
69th Coast AA Regiment arrives at San Diego
193rd Tank Battalion arrives at San Francisco
202nd PA Construction Battalion arrives at Bataan
SEARCHING FOR ADJACENT ENEMY UNITS
REASSIGNING TRANSPORT GROUPS
No.67 Sqn RAF sub-groups COMBINING at Chittagong

===== section @42560 len=2 =====


===== section @42566 len=2 =====


===== section @42572 len=14185 =====
OPERATIONAL REPORT FOR Dec 12, 41

SS S-40 reports having been sighted by the enemy at 82 , 91
Submarine TF 272 reports having been sighted by the enemy at 82 , 91
SS Triton reports having been sighted by the enemy at 153 , 103
SS K XIII reports having been sighted by the enemy at 52 , 79
SS K XIV reports having been sighted by the enemy at 53 , 78
SS K XV reports having been sighted by the enemy at 52 , 80
SS O 20 reports having been sighted by the enemy at 53 , 75
SS K XVIII lays minefield at Soc Trang ( 58 , 73) and sets course for Soerabaja
Mine Sweeping TF 441 resumes patrol, enroute to 78 , 171  near Warnambool
Surface Combat TF 341 resumes patrol, enroute to 52 , 83  near Djemadja
Task Force 202 has merged with TF 201 at 50 , 89
AVD William B. Preston sinks in port
Task Force 443 out of fuel at 74, 81
TF 445 observes H6K4 Mavis at 71,89 near Tawi Tawi
TF 301 shadowed by Japanese Recon at 44,70 near Sabang
TF 341 observes Japanese Level Bomber at 52,84 near Tioman Island
Sighting report: 6 Japanese ships at 79,89 near Cagayan , Speed unknown  
Sighting report: 6 Japanese ships at 79,89 near Cagayan , Speed unknown  
Sighting report: 2 Japanese ships at 79,89 near Cagayan , Speed unknown  
Sighting report: 4 Japanese ships at 81,87 near Dinagat , Speed 10 , Moving West 
TF 252 followed by Japanese Float Fighter at 84,87 near Dinagat
SS Tarpon snooped by Japanese Float Plane at 84,88 near Siargao
TF 342 detects Japanese Recon at 73,85 near Puerto Princesa
AMc MMS B detected by E13A1 Jake at 48,91 near Palembang
TF 338 observes Japanese Fighter Bomber at 50,84 near Singapore
SS K XIV shadowed by F1M2 Pete at 53,78 near Bukit Besi
MTB 11 observes Japanese Float Fighter at 73,71 near Paracel Islands
TF 340 followed by Japanese Float Fighter at 49,88 near Singkep
SS K XV snooped by Japanese Torpedo Bomber at 52,80 near Kuantan
CM Kung Wo shadowed by Japanese Recon at 50,89 near Muntok
CM Kung Wo shadowed by Japanese Torpedo Bomber at 50,89 near Muntok
TF 340 sighted by Japanese Torpedo Bomber at 49,88 near Singkep
TF 443 sighted by Japanese Recon at 75,81 near Taytay
SBD-2 Dauntless sighting report: 6 Japanese ships at 140,156 near Hoorn Islands, speed 13, Moving Southwest
139WH-3 reports shape below surface at 49, 97 near Batavia
139WH-3 reports object under water at 49, 97 near Batavia
139WH-3 reports oil slick at 49, 97 near Batavia
139WH-3 from WH-1 Patrouille attacking a Japanese SS at 49,97
Sighting report: 2 Japanese ships at 51,82 near Mersing , Speed 29 , Moving Southwest 
Sighting report: 10 Japanese ships at 51,82 near Mersing , Speed unknown  
Sighting report: 10 Japanese ships at 51,82 near Mersing , Speed unknown  
Sighting report: 5 Japanese ships at 51,82 near Mersing , Speed unknown  
Sighting report: 6 Japanese ships at 51,82 near Mersing , Speed unknown  
Sighting report: 4 Japanese ships at 51,82 near Mersing , Speed unknown  
Sighting report: 9 Japanese ships at 51,82 near Mersing , Speed unknown  
Sighting report: 10 Japanese ships at 51,82 near Mersing , Speed unknown  
Sighting report: 7 Japanese ships at 51,82 near Mersing , Speed unknown  
Sighting report: 5 Japanese ships at 51,82 near Mersing , Speed unknown  
Sighting report: 9 Japanese ships at 51,82 near Mersing , Speed 3 , Moving Southwest 
Sighting report: 10 Japanese ships at 51,82 near Mersing , Speed unknown  
AM Penguin sighted by Japanese Aircraft at 100,117 near Manus
Japanese Fighter Bomber sighted over Johore Bahru 
Japanese Aircraft sighted over Clark Field 
Japanese Aircraft sighted over Iba 
Japanese Recon sighted over Lingayen 
Japanese Aircraft sighted over Batangas 
Ki-36 Ida sighted over Tuguegarao 
Japanese Light Bomber sighted over Tuguegarao 
Japanese Aircraft sighted over Tuguegarao 
Japanese Aircraft sighted over Chengchow 
Japanese Fighter Bomber sighted over Loyang 
Japanese Aircraft sighted over 30th Chinese Corps
Japanese Aircraft sighted over 55th Chinese Corps
Japanese Aircraft sighted over Nanyang 
Japanese Aircraft sighted over Changsha 
Ki-15-II Babs sighted over 88th Chinese Corps
Ki-15-II Babs sighted over Wenchow 
Japanese Aircraft sighted over Pakhoi 
Japanese Aircraft sighted over Pakhoi 
Japanese Fighter sighted over Sabang 
Japanese Aircraft sighted over Medan 
Japanese Aircraft sighted over Manila 
Japanese Fighter Bomber sighted over Singapore 
Ki-46-II Dinah sighted over Rangoon 
Japanese Aircraft sighted over 20th Chinese Corps
Japanese Aircraft sighted over Georgetown 
Japanese Torpedo Bomber sighted over Manila 
Japanese Aircraft sighted over Johore Bahru 
Japanese Aircraft sighted over Clark Field 
Japanese Aircraft sighted over Iba 
Japanese Recon sighted over Lingayen 
Japanese Aircraft sighted over Batangas 
Japanese Light Bomber sighted over Tuguegarao 
Japanese Aircraft sighted over Chengchow 
Japanese Aircraft sighted over Loyang 
Japanese Torpedo Bomber sighted over 30th Chinese Corps
Japanese Fighter sighted over 55th Chinese Corps
Ki-15-II Babs sighted over Nanyang 
Japanese Aircraft sighted over Changsha 
Japanese Aircraft sighted over 88th Chinese Corps
Japanese Aircraft sighted over Wenchow 
Japanese Aircraft sighted over Sabang 
Japanese Aircraft sighted over Medan 
Japanese Fighter sighted over Manila 
Japanese Fighter sighted over Singapore 
Japanese Aircraft sighted over Rangoon 
Ki-15-II Babs sighted over 20th Chinese Corps
Japanese Torpedo Bomber sighted over Georgetown 
Ki-46-II Dinah sighted over Manila 
PBY-5 Catalina from VP-24 crashes on landing
P-40E Warhawk from 24th PG/3rd PS is written off
Damaged P-40E Warhawk from 24th PG/3rd PS crashes
P-40E Warhawk from 35th PG/21st PS is written off
P-40E Warhawk from 35th PG/21st PS is written off
Sighting report: 6 Japanese ships at 81,81 near Naga , Speed unknown  
Sighting report: 2 Japanese ships at 81,81 near Naga , Speed unknown  
TF 342 sights E13A1 Jake at 73,85 near Puerto Princesa
TF 448 sighted by Japanese Fighter Bomber at 50,84 near Singapore
SS S-41 observes Japanese Float Fighter at 84,87 near Dinagat
TF 443 followed by E13A1 Jake at 75,81 near Taytay
SBD-2 Dauntless reports periscope at 143, 155 near Wallis Island
139WH-3 reports periscope at 49, 97 near Batavia
139WH-3 reports shape below surface at 49, 97 near Batavia
139WH-3 reports object under water at 49, 97 near Batavia
139WH-3 reports diving submarine at 49, 97 near Batavia
AM Penguin sights Japanese Aircraft at 100,117 near Manus
C5M2 Babs sighted over Palembang 
Japanese Fighter sighted over Palembang 
Japanese Aircraft sighted over Johore Bahru 
Japanese Aircraft sighted over Clark Field 
Japanese Fighter Bomber sighted over Iba 
Ki-36 Ida sighted over Tuguegarao 
Japanese Light Bomber sighted over Tuguegarao 
Japanese Aircraft sighted over Tuguegarao 
Japanese Aircraft sighted over Chengchow 
Japanese Torpedo Bomber sighted over Loyang 
Japanese Aircraft sighted over 30th Chinese Corps
Japanese Recon sighted over 55th Chinese Corps
Japanese Aircraft sighted over Nanyang 
Japanese Aircraft sighted over Changsha 
Japanese Fighter sighted over 88th Chinese Corps
Japanese Aircraft sighted over Wenchow 
Japanese Aircraft sighted over Sabang 
Japanese Aircraft sighted over Langsa 
Japanese Aircraft sighted over Bataan 
Japanese Aircraft sighted over Manila 
Japanese Aircraft sighted over Rangoon 
Japanese Recon sighted over 20th Chinese Corps
Japanese Recon sighted over Georgetown 
Japanese Aircraft sighted over Alor Star 
Japanese Fighter Bomber sighted over 88th Chinese Corps
Japanese Aircraft sighted over Vigan 
Japanese Aircraft sighted over Manila 
Japanese Aircraft sighted over Palembang 
Japanese Aircraft sighted over Clark Field 
Japanese Fighter Bomber sighted over Iba 
Japanese Light Bomber sighted over Tuguegarao 
Japanese Fighter Bomber sighted over Chengchow 
Japanese Aircraft sighted over Loyang 
Japanese Fighter sighted over 30th Chinese Corps
Japanese Fighter Bomber sighted over 55th Chinese Corps
Japanese Aircraft sighted over Nanyang 
Japanese Fighter Bomber sighted over Changsha 
Ki-15-II Babs sighted over 88th Chinese Corps
Japanese Aircraft sighted over Wenchow 
Ki-15-II Babs sighted over Sabang 
Japanese Fighter sighted over Langsa 
Ki-15-II Babs sighted over Bataan 
Japanese Aircraft sighted over Manila 
Japanese Aircraft sighted over Rangoon 
Japanese Fighter sighted over 20th Chinese Corps
Ki-15-II Babs sighted over Georgetown 
Japanese Aircraft sighted over Alor Star 
Ki-15-II Babs sighted over 88th Chinese Corps
Japanese Fighter sighted over Vigan 
Japanese Aircraft sighted over Manila 
3 x Do-24K-1 transporting 1 ML-KNIL Aviation  from Batavia
3 x Do-24K-1 transporting 1 ML-KNIL Aviation  from Batavia
3 x Do-24K-1 transporting 1 ML-KNIL Aviation  from Batavia
2 x Do-24K-1 transporting Sorong Det. Base Force from Sorong
3 x Do-24K-1 transporting 1 ML-KNIL Aviation  from Batavia
2 x T.IVa transporting Riouw KNIL Battalion from Singkep
3 x PBY-5 Catalina transporting 1 ML-KNIL Aviation  from Batavia
4 x Catalina I transporting Lark Force  from Rabaul
4 x Catalina I transporting Lark Force  from Rabaul
2 x S.23 Empire transporting Lark Force  from Rabaul
1 x S.23 Empire transporting Lark Force  from Rabaul
5 x Catalina I transporting 1 ML-KNIL Aviation  from Batavia
1 x DC-3 transporting supplies to Chengtu
6 x Hudson I transporting supplies to Port Moresby
6 x Hudson I transporting supplies to Port Moresby
1 x DC-2 transporting supplies to Chengtu
Repairs completed on SS K X at Soerabaja, ship returned to service
TF 346 does not have enough fuel to complete mission
TF 351 does not have enough fuel to complete mission
TF 360 does not have enough fuel to complete mission
TF 362 does not have enough fuel to complete mission
TF 375 does not have enough fuel to complete mission
TF 388 does not have enough fuel to complete mission
TF 424 does not have enough fuel to complete mission
TF 433 does not have enough fuel to complete mission
TF 435 does not have enough fuel to complete mission
TF 441 leaving patrol zone to refuel at Melbourne
TF 443 does not have enough fuel to complete mission
TF 446 does not have enough fuel to complete mission
TF 460 does not have enough fuel to complete mission
TF 475 does not have enough fuel to complete mission
TF 504 does not have enough fuel to complete mission
TF 510 does not have enough fuel to complete mission
TF 513 does not have enough fuel to complete mission
Japanese forces CAPTURE Naga !!!
Japanese forces CAPTURE Cagayan !!!
Japanese forces CAPTURE Hollandia !!!
Babeldaob expands airfield to size 4
Midway Island expands fortifications to size 1
Guam expands fortifications to size 1
Clark Field expands fortifications to size 2
Iba expands fortifications to size 1
Singapore expands fortifications to size 1
Amritsar expands fortifications to size 1
Edmonton expands fortifications to size 1
VF-3 at San Diego takes F4F-3 Wildcat replacements
VMO-251  at San Diego takes F4F-3 Wildcat replacements
VP-41 at Kodiak takes PBY-5 Catalina replacements
No.273 Sqn RAF at Colombo takes Vildebeest III replacements
VMSB-131  on CV Saratoga converting to size 81 from 18
Port Hedland RAN Base Force - some devices have been upgraded at Port Hedland
Carnavon RAN Base Force - some devices have been upgraded at Carnarvon
Geraldton RAN Base Force - some devices have been upgraded at Geraldton
Burnie RAAF Det Base Force - some devices have been upgraded at Launceston
Hobart RAAF Det Base Force - some devices have been upgraded at Launceston
Launceston RAAF Base Force - some devices have been upgraded at Launceston
Queenstown RAAF Base Force - some devices have been upgraded at Launceston
1st RAAF Base Force - some devices have been upgraded at Bourke
Delayed planes for the aircraft pool:
  1 (15) x DC-2, 3 (4) x Hudson I, 1 (1) x B-17D Fortress, 10 (5) x P-36A Mohawk, 4 (0) x P-39D Airacobra
  1 (0) x P-40B Warhawk, 2 (5) x P-40E Warhawk, 3 (0) x P-43 Lancer, 1 (3) x F4F-3 Wildcat
  1 (0) x OS2U-3 Kingfisher, 4 (0) x PBY-4 Catalina, 4 (4) x PBY-5 Catalina, 3 (4) x SB2U-3 Vindicator
BB California available for service in damaged state at Pearl Harbor
CL Helena available for service in damaged state at Pearl Harbor
DD Worden available for service in damaged state at Pearl Harbor
254th Armoured Brigade takes replacements from Calcutta
Low garrison leads to damage and 1 VP loss from partisan attack at Foochow (86, 60)!
Low garrison leads to damage and 1 VP loss from partisan attack at Rangoon (54, 53)!
Low garrison leads to damage and 1 VP loss from partisan attack at Bombay (36, 24)!
Low garrison leads to damage and 1 VP loss from partisan attack at Chungking (76, 45)!
Low garrison leads to damage and 1 VP loss from partisan attack at Loyang (87, 43)!
Low garrison leads to damage and 1 VP loss from partisan attack at Kweiteh (90, 46)!
Low garrison leads to damage and 1 VP loss from partisan attack at Nanning (72, 55)!
Low garrison leads to damage and 1 VP loss from partisan attack at Pakhoi (72, 58)!
Low garrison leads to damage and 1 VP loss from partisan attack at Paoshan (65, 45)!
Low garrison leads to damage and 1 VP loss from partisan attack at Tsiaotso (88, 42)!
Low garrison leads to damage and 1 VP loss from partisan attack at Wenchow (89, 58)!
Low garrison leads to damage and 1 VP loss from partisan attack at Yenan (88, 37)!
Low garrison leads to damage and 1 VP loss from partisan attack at Bandoeng (50, 100)!
Low garrison leads to damage and 1 VP loss from partisan attack at Tingchow (83, 58)!
xAKL Yoshinogawa Maru is reported to have been sunk near Bukit Besi on Dec 12, 1941
xAKL Matsutan Maru is reported to have been sunk near Kota Bharu on Dec 12, 1941
No.14 Sqn RAAF arrives at Townsville
71st OG/110th OS arrives at San Francisco
AHQ Far East  arrives at Singapore
69th Coast AA Regiment arrives at San Diego
193rd Tank Battalion arrives at San Francisco
202nd PA Construction Battalion arrives at Bataan
Planes and pilots of No.100 Sqn RAF Det withdrawing

===== section @56761 len=1062 =====
SIG INT REPORT FOR Dec 12, 41

Radio transmissions detected at 100,46.
Radio transmissions detected at Fusan (103,55).
2/113th Infantry Regiment is loaded on a Japanese xAK moving to Kota Bharu.
110th/C Division is located at Paoting(94,40).
5th Manchukuo Cav Brigade is located at Yingkow(101,43).
5th Tank Regiment is located at Jehol(97,39).
91st Naval Guard Unit is located at 50,72.
Korea Army is located at Fusan(103,55).
54th Field AA Battalion is located at Tunghua(106,44).
Radio transmissions detected at 112,107.
Radio transmissions detected at Babeldaob (90,97).
3/97th JAAF AF Bn  is loaded on a Aden Cargo class xAK moving to Mersing.
2/37th Const Co  is loaded on a Japanese xAK moving to Naga.
Heavy Volume of Radio transmissions detected at Tokyo (114,60).
8th Garrison Unit  is located at Hailun(111,37).
Heavy Volume of Radio transmissions detected at 66,69.
7/11th Infantry Regiment is located at Kuantan(51,79).
RTA/A Division is located at Bangkok(56,62).
16th Infantry Regiment is planning for an attack on Singapore.
```

## Fixed-stride tables

Record sizes below were measured two independent ways and agree: folding the offsets of every NUL-terminated name modulo each divisor of the blob, and checking whether the bytes between consecutive names are dead space or live fields. Details and the correction to the earlier figures are in [`pws-field-map.md`](pws-field-map.md).

| Type | Record bytes | Rows | Named rows | Name at | Changes across the six turns? |
| --- | ---: | ---: | ---: | ---: | --- |
| `0x08` ships | 464 | 20000 | 11167 | +0 | yes |
| `0x13`  | 1172 | 18000 | 36 | +4 | yes |
| `0x14`  | 20 | 1020034 | 108 | +17 | yes |
| `0x15` persons / leaders | 180 | 50000 | 28533 | +0 | yes |
| `0x16`  | 36 | 350000 | 22712 | +0 | yes |
| `0x1b` ship classes | 380 | 5000 | 1705 | +0 | no — static reference data |
| `0x1f` equipment / devices | 236 | 2000 | 1152 | +0 | yes |
| `0x20` aircraft types | 396 | 1000 | 555 | +0 | yes |
| `0x35` operations / plans | 1304 | 2000 | 1308 | +0 | no — static reference data |

## Comparison with `wpae023.pws`, the outgoing file

`wpae023.pws` carries the **same in-game date** as T6 (`12/13/41`), which makes it the one file that can be compared like for like.

| | T6 `381d` | `wpae023` |
| --- | --- | --- |
| Save label | `(PBEM0) Incoming to Dave Date: 12/13/41 ` | `(PBEM1) 12/8 incoming to Ozgur Date: 12/13/41 ` |
| Host timestamp | `Fri, September 18 2026  at 11:43:22 AM  ` | `Thu, September 17 2026  at 11:34:04 PM  ` |
| Build | `1.8.11.28.4` | `1.8.11.28.4` |
| Header byte 1 | 2 | 2 |
| On-disk MD5 | *(see table above)* | `b85624828c57a1160afccee4a74ca54b` |
| Ships marked out of play | 35 | 35 |

The two files share the same 61 record types and **44 of 61 types are byte-identical**. The types that differ are:

| Type | Bytes differing |
| --- | --- |
| `0x01` | 72 |
| `0x02` | 2 |
| `0x03` | 1 |
| `0x08` | 2187 |
| `0x0c` | 1 |
| `0x12` | 669 |
| `0x13` | 16069 |
| `0x14` | 1179 |
| `0x15` | 125 |
| `0x16` | 5630 |
| `0x17` | 3783 |
| `0x20` | 2 |
| `0x26` | 44 |
| `0x3a` | 86 |
| `0x44` | 2 |
| `0x46` | 3724 |
| `0x51` | different lengths: 57827 vs 50926 |

The timestamps place `wpae023` **between** T5 and T6: T5 was written Sep 17 at 12:11 PM, `wpae023` at 11:34 PM the same evening, T6 the next morning at 11:43 AM. That is consistent with Ozgur receiving the 12/12 file, playing his turn and sending `wpae023` out that night, then receiving T6 back the next day. Both carry in-game date 12/13/41 because the date advances once both sides have moved.

Note that `wpae023`'s own label says `incoming to Ozgur`, which contradicts a reading of these labels as direction markers and supports treating them as free text. The direction of `wpae023` rests on Ozgur's statement, not on a file field.

## Method

- `gzip.decompress`; per-record `zlib.decompressobj` so the stream trailer defines the compressed length; walk resynchronises on `78 da` when a header does not lead to a stream.
- Records aligned across files by `a & 0xff`, never by position.
- Record size measured by name-offset folding and by dead-space testing, not assumed.
- Turn text parsed as `uint32le` length prefixes that exactly tile the blob.
- Password tried only as zip crypto and as literal/digest/checksum comparisons. No guessed XOR or stream cipher. No network.
- Scripts: `internal/pws-inspection/` (`extract_wpae021.py`, `align_wpae021.py`, `stride_wpae021.py`, `substride_wpae021.py`, `diff_wpae021.py`, `turntext_wpae021.py`, `password_tests_021.py`, `compare_wpae023.py`, `gen_catalog_021.py`). Measurements: `internal/pws-inspection/wpae021/*.json`.

