# WITP-CIC2 — Open Spec Questions (Batch 2) — [ALL ANSWERED]

**Rule:** Every answer Ozgur provides here is immediately recorded in `docs/scope-v1.md` changelog (date-stamped) and mirrored into `docs/architecture-decisions.md` before any code implementation begins.

---

## A — Screen & Navigation

### A1. `{INDEX}` in the Screen Header — [ANSWERED 2026-09-19 19:24 UTC+3]
- **Spec ref:** §8.2 A (`[ {INDEX}. {SCREEN TITLE} ] Turn: {TURN_DATE}`)
- **Question:** What should `{INDEX}` display on each screen?
- **Answer (Ozgur):** It is likely an artifact from old code. **Do not use `{INDEX}`. Document everything vigorously.** Screen headers will cleanly display `[ {SCREEN TITLE} ]  Turn: {TURN_DATE}` (or omit `Turn:` if before campaign load).

---

### A2. Settings Module Content for v1 — [ANSWERED 2026-09-19 19:24 UTC+3]
- **Spec ref:** §4.2 (`[S] Settings`), §6
- **Question:** What settings must be configurable in the Settings screen for version 1?
- **Answer (Ozgur):** Both **(a) and (b)** with descriptive explanations of what each setting controls:
  1. **Allied SIGINT Confidence Toggle:** Toggle Allied SIGINT interpretation between `HIGH` (historical codebreaking accuracy) and `PROBABLE`.
  2. **Strike Concentration Thresholds:** Configurable warship count (default: `3` warships) and distance threshold (default: `10` hexes) for flagging strike force concentrations.

---

### A3. ESC Key Behavior from Master Menu — [ANSWERED 2026-09-19 19:24 UTC+3]
- **Spec ref:** §4 ("Pressing ESC in any screen immediately moves the view one screen back at a time.")
- **Question:** From the Master Menu screen, what should pressing `ESC` do?
- **Answer (Ozgur):** **(b) Open Exit confirmation prompt** (same behavior as pressing `[E] Exit`).

---

## B — Vault File & Storage

### B1. Campaign Vault File Compression & Protection — [ANSWERED 2026-09-19 19:24 UTC+3]
- **Spec ref:** §3, §5 ("encrypted Campaign Vault File")
- **Question:** What exact encryption or protection scheme should be applied to the Campaign Vault file?
- **Answer (Ozgur):** **"Just compress it"** — Standard Zlib/Zip compression container (`.wic`), no cryptographic password encryption layer required.

---

### B2. Authoritative Campaign Vault File Extension — [ANSWERED 2026-09-19 19:24 UTC+3]
- **Spec ref:** §3, §4.1, `docs/architecture-decisions.md`
- **Question:** Is `.wic` the confirmed, authoritative file extension for saved Campaign Vaults?
- **Answer (Ozgur):** **"All good choose yours"** → **`.wic`** is adopted as the canonical extension (`C:\WitP-CIC\Campaigns\<Name>.wic`).

---

## C — SIGINT Map (Tab 2)

### C1. Visual Representation of the Console "Map" — [ANSWERED 2026-09-19 19:24 UTC+3]
- **Spec ref:** §4.5 Tab 2 ("Displays the strategic planning map overlaid with layered SIGINT data")
- **Question:** In the Win32 green phosphor console, how should the map be rendered visually?
- **Answer (Ozgur):** **(c) Textual / tabular list of localized hex coordinates with distance & sector bearings** (no spatial 2D ASCII grid). *Note: May change in a future version.*

---

## D — Ingest & Game File Discovery

### D1. Default WITP:AE Report Directory Locations — [ANSWERED 2026-09-19 19:24 UTC+3]
- **Spec ref:** §4.1 ("finds the folder where the game writes its reports, searching automatically")
- **Question:** What is the installation path or directory name where WITP:AE writes daily reports and `.pws` saves?
- **Answer (Ozgur):** Two primary standard locations must be checked automatically:
  1. `C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE` (for `.pws` save state files)
  2. `C:\Matrix Games\War in the Pacific Admiral's Edition\SAVE\archive` (for daily text output reports: `asigint_*.txt`, `aoperationsreport_*.txt`, `combatreport_*.txt`, `Combat_Events_*.txt`)
  3. A directory browse/input option must be provided so the user can direct the application to an alternative third folder if their game is installed elsewhere.

---

### D2. Missing or Empty Turn Report Files — [ANSWERED 2026-09-19 19:24 UTC+3]
- **Spec ref:** §5, §11, §13
- **Question:** On quiet turns, can combat report files be completely absent?
- **Resolution:** Ingest is defensive and turn-date driven; processes all files present for discovered dates. If combat or events are empty/absent for a date, ingest logs it and records zero engagements rather than failing with a fatal error.

---

## E — Toolchain & Alien MU/TH/UR UI Architecture

### E1. Toolchain & UI Theme — [ANSWERED 2026-09-19 19:24 UTC+3]
- **Spec ref:** §8, `docs/architecture-decisions.md` §3
- **Question:** Toolchain and aesthetic requirements.
- **Answer (Ozgur):**
  - **Aesthetic:** Alien movie style computer terminal interface (**MU/TH/UR** / Nostromo / Weyland-Yutani computer terminal theme). Green monochrome phosphor palette (Win32 Console attributes 2, 10, 160), crisp scanline/typewriter pacing feel, box framing (`=`, `-`, `+`), blinking status markers, uppercase teletype typography. Reference: [Weyland Uplink MU/TH/UR Terminal](https://weylanduplink.com/news/whats-story-muthur-new-alien-viral-website-launched).
  - **Toolchain:** Developer decision. **C++17 with MinGW-w64 (GCC)** compiling to a standalone, zero-dependency static Windows console executable (`WitP-CIC.exe`) targeting Windows console (`cmd.exe` / Windows Terminal).

---
*All Batch 2 questions documented and answered 2026-09-19 19:24 UTC+3.*
