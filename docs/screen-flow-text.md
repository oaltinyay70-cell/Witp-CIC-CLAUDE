# WITP-CIC-MUTHUR — SCREEN NAVIGATION FLOW (TEXT & ASCII)

Below is the complete screen flow displayed in clean ASCII tree and table formats so you can read it directly without needing an image/diagram renderer.

---

## 1. Top-Level Screen Flow Tree

```
[SplashScreen]  (WU/TH/UR Nostromo intro, auto-advances or keypress)
      │
      ▼
[StartScreen]
      │
      ├── [N] ──► [NewCampaignWizardScreen]
      │                 │ (Steps 1 to 4 completed)
      │                 ▼
      │           [IngestionProcessing] ──► [MasterMenuScreen]
      │
      ├── [C] ──► [ContinueCampaignScreen]
      │                 │ (.wic selected & verified)
      │                 ▼
      │           [IngestionProcessing] ──► [MasterMenuScreen]
      │
      └── [E] ──► [ExitModal] ──► (Confirm [Y]) ──► [Process Shutdown]
```

---

## 2. In-Campaign Navigation Tree (From Master Menu)

```
                       [MasterMenuScreen]
                               │
       ┌───────────────────────┼───────────────────────┬──────────────────────┐
       │ [A]                   │ [W]                   │ [S]                  │ [E] or [ESC]
       ▼                       ▼                       ▼                      ▼
 [WarRoomScreen]          [WikiScreen]         [SettingsScreen]          [ExitModal]
       │                       │                       │                      │
       ├── Tab 1: Intel        ├── Categories          ├── SIGINT Confidence  ├── [Y]: Purge & Exit
       ├── Tab 2: SIGINT Map   └── Topic Details       └── Strike Thresholds  └── [N]: Cancel
       └── Tab 3: TPIE
```

---

## 3. Detailed Screen-by-Screen Transitions & Hotkeys

| # | Current Screen | Title Displayed in Header | Actions & Keys | Target Screen / Modal |
|---|----------------|---------------------------|----------------|-----------------------|
| **1** | **SplashScreen** | `[ WU/TH/UR ]` | Any key or timeout | **StartScreen** |
| **2** | **StartScreen** | `[ START ]` | `[N]` (New Campaign) | **NewCampaignWizardScreen** |
| | | | `[C]` (Continue Campaign) | **ContinueCampaignScreen** |
| | | | `[E]` (Exit) | **ExitModal** |
| **3** | **NewCampaignWizardScreen** | `[ NEW CAMPAIGN WIZARD ]` | Complete Steps 1–4 | **IngestionProcessing** |
| | | | `[ESC]` (on Step 1) | Return to **StartScreen** |
| | | | `[ESC]` (on Steps 2–4) | Previous Wizard Step |
| **4** | **ContinueCampaignScreen** | `[ CONTINUE CAMPAIGN ]` | Select `.wic` + `[ENTER]` | **IngestionProcessing** |
| | | | `[ESC]` | Return to **StartScreen** |
| **5** | **IngestionProcessing** | `[ VAULT INGESTION ]` | (Automatic parsing) | **MasterMenuScreen** |
| **6** | **MasterMenuScreen** | `[ MASTER MENU ]` | `[A]` | **WarRoomScreen** (Tab 1) |
| | | | `[W]` | **WikiScreen** |
| | | | `[S]` | **SettingsScreen** |
| | | | `[E]` or `[ESC]` | **ExitModal** |
| **7** | **WarRoomScreen** | `[ WAR ROOM - TAB X ]` | `[1]` / `[2]` / `[3]` / `[TAB]` | Switch between Tabs 1, 2, and 3 |
| | | | `[ENTER]` on any row | Open **DetailModal** |
| | | | `[F]` (in Tab 3) | Open **InterceptConstraintsModal** |
| | | | `[/]` (in Tab 2) | Open **DossierSearchModal** |
| | | | `[ESC]` | Return to **MasterMenuScreen** |
| **8** | **DetailModal** | Pop-up overlay | `[ENTER]` or `[ESC]` | Dismiss and return to **WarRoomScreen** |
| **9** | **InterceptConstraintsModal** | Pop-up overlay | Submit or `[ESC]` | Dismiss and return to **WarRoomScreen** |
| **10** | **DossierSearchModal** | Pop-up overlay | Submit or `[ESC]` | Dismiss and return to **WarRoomScreen** |
| **11** | **WikiScreen** | `[ WIKI & FIELD MANUAL ]` | `[ENTER]` on Topic | Open Topic Sub-page |
| | | | `[ESC]` (inside Topic) | Return to Category List |
| | | | `[ESC]` (at Category) | Return to **MasterMenuScreen** |
| **12** | **SettingsScreen** | `[ SETTINGS ]` | `UP`/`DOWN` + `[ENTER]` toggle | Edit values |
| | | | `[ESC]` | Save changes & return to **MasterMenuScreen** |
| **13** | **ExitModal** | `[ TERMINATE SESSION ]` | `[Y]` | Purge memory & shut down app |
| | | | `[N]` or `[ESC]` | Cancel and return to previous screen |

---

## 4. Wizard Step Sequence (Inside NewCampaignWizardScreen)

```
[Step 1: Side]       ──► Prompt: Command Allied or Japanese forces?
        │ [ENTER]
        ▼
[Step 2: Password]   ──► Prompt: Enter Dave's .pws PBEM password (stored for reference)
        │ [ENTER]
        ▼
[Step 3: Discovery]  ──► Auto-search: ...\SAVE and ...\SAVE\archive (or press [B] to browse)
        │ [ENTER]
        ▼
[Step 4: Vault Name] ──► Prompt: Enter campaign vault name (creates C:\WitP-CIC\Campaigns\<Name>.wic)
        │ [ENTER]
        ▼
   (Ingestion)       ──► Auto-loads all discovered reports & saves into .wic
```
