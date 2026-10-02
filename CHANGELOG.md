# Changelog
Newest first. History before 2026-10-02 lives in `docs/archive/scope-v1.md` (changelog section).

## 2026-10-02 — Cleanup: pillar services parked (awaiting owner confirmation)
- Owner decision: stop running unused pillar services, keep code and tests. None of their output reached a screen.
- No longer run during ingest: concept graph, hotspot clustering (Model F now empty), supply-network graph (Model D status is a fixed line), entity resolution, air-denial heatmap, hazard repository, Bayesian corroboration, advance-momentum predictor, PWS world-state wiring, strategic rules engine.
- Still running: sensor weighting + time decay (they set confidence on retained HVT/location data); hex geometry and hazard types (used by parsers and the vault).
- Dossier search keeps name/base matching; semantic fallback removed. Concept graph now tested directly.

## 2026-10-02 — Cleanup: AI-endpoint settings removed (awaiting owner confirmation)
- Settings no longer offers "Connect to predictive AI model"; the AI connection screen (provider/endpoint/model/test) is gone. It never made network calls. Settings is now 5 options: SIGINT confidence, strike warship count, strike radius, amphibious sensitivity, save to vault.
- Tests: Settings has no AI options and `[1]` no longer opens an AI screen.

## 2026-10-02 — Cleanup: War Room dead screens (awaiting owner confirmation)
- Removed unreachable screens Intelligence Summary (+ detail pop-up and its advisor panel), SIGINT Map, Dossier Search and Predictive Forecasting, plus the Page 5 view-model; ~890 lines. TPIE had no code left.
- Kept the analytics behind them (HVTs, geospatial log, `SearchDossier`, etc.) for re-homing, and the split-pane components (own tests).
- The date-range filter modal is now unreachable; kept pending owner decision.

## 2026-10-02 — Foundations (awaiting owner confirmation)
- Scan hygiene: only the campaign's chosen .pws; live-vs-archive de-dup; CombatEvents `MM/DD/YY` = next-day stamp; ignores autosave\, SAFE ARCH\, rar/placeholder/updateReport, j* (Japanese-side) reports; repo sample/golden data scanned by tests only.
- `.wic` vault (`Core/Vault/CampaignVault.cs`): zip with manifest + ledger, raw reports, Allied-only decoded PWS snapshots, superseded versions kept (.prevN), atomic save + .bak.
- Wizard is now Allied-only, 3 steps: name -> game folders -> pick campaign .pws; creates the vault. Continue opens the vault and ingests only new files. Password step removed (vault unencrypted). No scan at startup.

## 2026-10-02 — Takeover
- Baseline commit of builds alpha4-16..33 (`a60100e`, tag `baseline-takeover-alpha4-33`).
- Archived old backups/builds to `_archive/` (git-ignored). Consolidated docs into SPEC/CHANGELOG/TODO;
  old core docs moved to `docs/archive/`.
- Remote renamed to `Witp-CIC-CLAUDE` (old MUTHUR repo retired). Vault layout approved; campaign save = `wpae021.pws` only.
