# Changelog
Newest first. History before 2026-10-02 lives in `docs/archive/scope-v1.md` (changelog section).

## 2026-10-02 — Foundations (awaiting owner confirmation)
- Scan hygiene: only the campaign's chosen .pws; live-vs-archive de-dup; CombatEvents `MM/DD/YY` = next-day stamp; ignores autosave\, SAFE ARCH\, rar/placeholder/updateReport, j* (Japanese-side) reports; repo sample/golden data scanned by tests only.
- `.wic` vault (`Core/Vault/CampaignVault.cs`): zip with manifest + ledger, raw reports, Allied-only decoded PWS snapshots, superseded versions kept (.prevN), atomic save + .bak.
- Wizard is now Allied-only, 3 steps: name -> game folders -> pick campaign .pws; creates the vault. Continue opens the vault and ingests only new files. Password step removed (vault unencrypted). No scan at startup.

## 2026-10-02 — Takeover
- Baseline commit of builds alpha4-16..33 (`a60100e`, tag `baseline-takeover-alpha4-33`).
- Archived old backups/builds to `_archive/` (git-ignored). Consolidated docs into SPEC/CHANGELOG/TODO;
  old core docs moved to `docs/archive/`.
- Remote renamed to `Witp-CIC-CLAUDE` (old MUTHUR repo retired). Vault layout approved; campaign save = `wpae021.pws` only.
