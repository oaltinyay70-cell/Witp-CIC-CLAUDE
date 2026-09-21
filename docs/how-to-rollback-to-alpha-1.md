# How to Roll Back to Alpha 1 (Version 0.02)

**Document Status:** Official Rollback Guide & Baseline Procedure  
**Target Release:** Version 0.02 (Stable Alpha 1)  
**Established:** 2026-09-20 16:32 UTC+3  
**Authority:** Ozgur (Owner)  

---

## 1. Overview

Version 0.02 has been locked and preserved as **Stable Alpha 1**. It represents a verified, production-stable baseline containing:
- Retro Alien MU/TH/UR Nostromo console interface in C++17.
- Static, self-contained Windows executable (`dist/WitP-CIC.exe`) requiring zero external DLLs.
- Splash screen with non-blocking 1 Hz prompt blinking and zero attribute bleed.
- New Campaign wizard with report and save auto-detection, multi-directory accumulation (`[B]`), and physical `.wic` container generation under `C:\WitP-CIC\Campaigns`.
- Continue Campaign workflow with automatic incremental diff ingestion of newly arrived `.pws` and `.txt` turn files, refreshing all War Room models.
- Master Menu header instrumentation (`Files Processed : XX pws files | xx TXT files | Days Processed : xx`) partitioned by `|`.
- Framed notification modal pop-ups replacing fixed cursor coordinate overwrites.
- Recursive hierarchical System Wiki with right-aligned `TOPIC EXPLANATION` pop-up windows and decomposed Error Code Registry sub-topics (`[101]`, `[102]`, `[201]`, `[202]`, `[301]`, `[401]`).
- Standalone 24-chapter Field Manual browser.
- Sombre acoustic navigation click profile and silent background audio.
- 100% automated test pass rate across all 4 unit test suites.

If any future upgrade or development is unsatisfactory, use one of the rollback procedures below to instantly time travel back to this state.

---

## 2. Rollback Options

### Option A: Non-Destructive Time Travel (Temporary Inspection / Execution)
Use this option if you want to switch to Version 0.02 without deleting current or uncommitted work on other branches.

1. Stash or discard any untracked / working changes:
   ```bash
   git stash
   ```
2. Check out the release tag or the stable branch:
   ```bash
   git checkout v0.02
   # or:
   git checkout stable-alpha-1
   ```
3. Run the application:
   ```cmd
   dist\WitP-CIC.exe
   ```
4. To return to the latest development branch later:
   ```bash
   git checkout main
   git stash pop
   ```

---

### Option B: Full Hard Rollback (Permanent Repository Reset)
Use this option if future developments on `main` must be completely rolled back and replaced by the Stable Alpha 1 baseline.

1. Open a terminal in the repository root (`%USERPROFILE%\Documents\Witp-CIC2`):
   ```bash
   git fetch origin
   git checkout main
   git reset --hard v0.02
   ```
2. Verify that the working tree is clean:
   ```bash
   git status
   ```
3. Re-verify the build and unit tests:
   ```powershell
   powershell -Command "& 'dist\file_scanner_test.exe'; & 'dist\vault_serializer_test.exe'; & 'dist\wiki_test.exe'; & 'dist\field_manual_test.exe'"
   ```

---

### Option C: Standalone Local Offline Recovery (No Git Required)
Use this option if Git is corrupted, inaccessible, or if an offline restoration is required.

1. Locate the standalone backup archive:
   ```text
   C:\Users\ozgur\Documents\Witp-CIC2\backups\WITP-CIC2-v0.02-alpha1.zip
   ```
2. Extract the archive into a clean directory or overwrite the working directory.
3. The archive contains the complete source tree, all golden sample fixtures, documentation, test suites, and the precompiled static executable (`dist\WitP-CIC.exe`).

---

### Option D: GitHub Release Archive Download
Use this option from any external machine or browser:
1. Open the GitHub repository releases / tags:
   `https://github.com/oaltinyay70-cell/WITP-CIC2/tags`
2. Download the source zip for `v0.02`:
   `https://github.com/oaltinyay70-cell/WITP-CIC2/archive/refs/tags/v0.02.zip`

---

## 3. Mandatory Procedure for All Future Upgrades

Per baseline mandate established on **2026-09-20 16:32 UTC+3**, **every future upgrade must strictly follow this standardized release procedure**:

1. **Verification Gate:**
   - Compile static executable (`dist/WitP-CIC.exe`) with static MinGW flags:
     `-static -static-libgcc -static-libstdc++ -lkernel32 -luser32 -lwinmm`
   - Execute and verify 100% pass on all automated test suites.
2. **Core Docs Synchronization:**
   - Update `docs/scope-v1.md` with date-stamped changelog entry.
   - Update `docs/baseline_witp_cic.md` with release preservation notes.
   - Append new entry to `docs/lessons-learned.md`.
   - Record architectural choices in `docs/architecture-decisions.md`.
   - Update version identifier in `docs/project-context.md`.
   - Update `docs/milestones.md`.
   - Maintain `docs/todo.md` under the Baseline Acceptance Rule (never mark `[x]` without explicit user instruction).
3. **Physical Backup Creation:**
   - Create a clean standalone ZIP archive under `backups/` matching format:
     `backups/WITP-CIC2-v<Version>-<Milestone>.zip`
4. **Git Tagging & Branching:**
   - Create annotated release tag: `git tag -a v<Version> -m "<Description>"`
   - Update/create milestone branch: `git branch -f stable-<milestone>`
5. **Remote Synchronization:**
   - Push commit, stable branch, and tags to GitHub:
     ```bash
     git push origin main stable-<milestone>
     git push origin --tags
     ```
6. **Rollback Documentation:**
   - Maintain and update the rollback documentation (`docs/how-to-rollback-to-alpha-1.md` or next milestone rollback guide) with exact time-travel instructions.
