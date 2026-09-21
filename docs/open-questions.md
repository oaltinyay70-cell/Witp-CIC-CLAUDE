# WITP-CIC-MUTHUR — Open Spec Questions

**Rule:** When Ozgur answers a question, the agent immediately records the answer in `docs/scope-v1.md` changelog (date-stamped) and in the relevant design doc, then marks the question **[ANSWERED]** here with the same timestamp. No implementation starts before the entry is written.

---

## Q1 — `.pws` password validation — [ANSWERED 2026-09-19 18:33 UTC+3]

**Spec ref:** §4.1, §5, Changelog 2026-09-19

**Question:** §4.1 says the wizard "validates the password" for the `.pws` file. The golden set needs no file password to decompress. Is password validation ever a real code path?

**Answer (Ozgur):** The `.pws` file Dave sends Ozgur can only be opened **in the WITP:AE game** using that password. It is a game-UI PBEM credential (manual §20.5.4), not a file-level encryption key on the golden set. The wizard stores it for the user's own PBEM reference only. WITP-CIC does **not** use it for gzip/zlib decompression. **No password-gate code path in ingest.** "Validate the password" wording in §4.1 is superseded.

---

## Q2 — §8 footer hotkeys per screen — [ANSWERED 2026-09-19 18:33 UTC+3]

**Spec ref:** §8.2 C

**Question:** The spec footer template shows specific hotkeys that don't match the actual screens. Are these War Room-specific or per-screen?

**Answer (Ozgur):** The §8.2 C footer template is a **placeholder**. Footers must be composed **dynamically per screen**, showing only the hotkeys relevant to that screen. No hardcoded footer string anywhere in the codebase.

---

## Q3 — Turn length in the wizard — [ANSWERED 2026-09-19 18:33 UTC+3]

**Spec ref:** §4.1

**Question:** The wizard asks the user to "confirm the game's turn length." What does this mean?

**Answer (Ozgur):** **Not wanted. Remove it.** The turn-length confirmation step means nothing and was not intended. Ingest must automatically cover **all dates and all files** found in the game report folder. No prompt to the user about turn length.

---

## Q4 — Confidence assignment for shared files (Allied player) — [ANSWERED 2026-09-19 18:33 UTC+3]

**Spec ref:** §6

**Question:** When Ozgur (Allied) ingests shared `combatreport_*` and `Combat_Events_*` files, how should confidence be assigned?

**Answer (Ozgur):** Treat shared files as **"shared only to me"** — process them entirely from the Allied player's perspective. Do not attempt to model the opponent's internal view from shared files.

---

## Q5 — Intercept Windows user constraints (TPIE Tab 3) — [ANSWERED 2026-09-19 18:33 UTC+3]

**Spec ref:** §4.5 Tab 3, FAQ

**Question:** How does the user define constraints in the TUI for Intercept Windows?

**Answer (Ozgur):**
- (a) **Yes** — user can type a free-text hex coordinate/range
- (b) **Yes** — user can pick from a list of discovered/known bases and hexes
- (c) The input widget is a **modal popup menu** (not inline in the tab body)

Both modes are required.

---

## Q6 — Dossier Panel search mechanism (Tab 2) — [ANSWERED 2026-09-19 18:33 UTC+3]

**Spec ref:** §4.5 Tab 2

**Question:** How does search work in the Dossier Panel (right side of Tab 2)?

**Answer (Ozgur):**
- (a) **Yes** — user types a free-text partial name
- (b) Results drawn from the **list of discovered and known entities** in the vault
- (c) Covers **all five entity types** (base, vessel, unit, air group, city) in one unified search — no separate tabs per type

---

## Q7 — TPIE minimum data threshold — [ANSWERED 2026-09-19 18:33 UTC+3]

**Spec ref:** §7 intro ("ingest 30+ days of continuous historical logs")

**Question:** Before 30 turns of data exist, what does TPIE Tab 3 show?

**Answer (Ozgur):** Show **degraded / partial predictions from turn 1** with a confidence caveat. No hard data-volume gate before displaying the tab.

---

*All questions answered 2026-09-19 18:33 UTC+3. Recorded in `docs/scope-v1.md` changelog same timestamp. No open questions remain from this set.*
