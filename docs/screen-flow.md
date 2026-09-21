# WITP-CIC-MUTHUR — Screen Navigation State Flow

```mermaid
stateDiagram-v2
    [*] --> SplashScreen: Launch Application

    state SplashScreen {
        [*] --> DisplaySplash: Render Retro Alien MU/TH/UR Header & Boot Sequence
        DisplaySplash --> [*]: Automatic transition or any keypress
    }

    SplashScreen --> StartScreen: Transition after splash

    state StartScreen {
        [*] --> SelectMode
        note right of SelectMode
            Title: [ START ]
            Header: No Turn Date
            Options:
              [N] New Campaign
              [C] Continue Campaign
              [E] Exit
        end note
    }

    StartScreen --> NewCampaignWizardScreen: Select [N] New Campaign
    StartScreen --> ContinueCampaignScreen: Select [C] Continue Campaign
    StartScreen --> ExitModal: Select [E] Exit

    state NewCampaignWizardScreen {
        [*] --> Step1_Side: [1/4] Select Side (Allied / Japanese)
        Step1_Side --> Step2_Password: Confirm Side
        Step2_Password --> Step3_Paths: Enter PBEM Password (stored for ref)
        Step3_Paths --> Step4_VaultName: Auto-search SAVE & archive (or [B] Browse)
        Step4_VaultName --> IngestionProcessing: Enter .wic Vault Name
        
        note right of Step4_VaultName
            Title: [ NEW CAMPAIGN WIZARD ]
            Header: No Turn Date
            ESC from Step 1 -> Return to StartScreen
            ESC from Steps 2-4 -> Previous Wizard Step
        end note
    }

    NewCampaignWizardScreen --> StartScreen: ESC on Step 1

    state ContinueCampaignScreen {
        [*] --> ListVaults: Scan C:\WitP-CIC\Campaigns\*.wic
        ListVaults --> VerifyPaths: Select .wic file [ENTER]
        VerifyPaths --> IngestionProcessing: Verify stored SAVE/archive paths
        
        note right of ListVaults
            Title: [ CONTINUE CAMPAIGN ]
            Header: No Turn Date
            Options:
              UP/DOWN select .wic
              [ENTER] Load Vault
              [ESC] Back to StartScreen
        end note
    }

    ContinueCampaignScreen --> StartScreen: Press [ESC]

    state IngestionProcessing {
        [*] --> ParseAndSync: Ingest new report files & .pws records
        ParseAndSync --> [*]: Commit to .wic container
        
        note right of ParseAndSync
            Title: [ VAULT INGESTION ]
            Blinking 1.0 Hz MU/TH/UR status line
        end note
    }

    IngestionProcessing --> MasterMenuScreen: Ingestion Complete

    state MasterMenuScreen {
        [*] --> MenuOptions
        note right of MenuOptions
            Title: [ MASTER MENU ]
            Header: Active Campaign: {NAME} ({SIDE})
            Stats: Files: {PWS} pws | {TXT} txt | Days: {DAYS}
            Options:
              [A] War Room -> Pop-up Notification
              [W] Wiki (System & Error Registry)
              [F] Field Manual (Game Manual Browser)
              [S] Settings -> Pop-up Notification
              [E] Exit Prompt
              [ESC] Exit Prompt
        end note
    }

    MasterMenuScreen --> WarRoomScreen: Select [A] War Room
    MasterMenuScreen --> WikiScreen: Select [W] Wiki
    MasterMenuScreen --> FieldManualScreen: Select [F] Field Manual
    MasterMenuScreen --> SettingsScreen: Select [S] Settings
    MasterMenuScreen --> ExitModal: Select [E] or Press [ESC]

    state FieldManualScreen {
        [*] --> ChapterBrowser: Index 24 Official Manual Chapters
        ChapterBrowser --> ManualReader: Select Chapter [ENTER]
        ChapterBrowser --> KeywordSearch: Press [/] or [S]
        ManualReader --> ChapterBrowser: Press [ESC]
        KeywordSearch --> ChapterBrowser: Press [ESC]
        
        note right of ChapterBrowser
            Title: [ FIELD MANUAL ]
            Header: Full Game Manual (6,720 lines)
            Search: Keyword RAG Query
            [ESC] Back to Master Menu
        end note
    }

    FieldManualScreen --> MasterMenuScreen: Press [ESC] at root chapter list

    state WarRoomScreen {
        [*] --> Tab1_IntelSummary: Default active tab
        
        state Tab1_IntelSummary {
            [*] --> RenderTab1: HVTs, Strike/Port Concentrations, Routes, Ghost Fleet
        }

        state Tab2_SIGINTMap {
            [*] --> RenderTab2: Tabular Hex Sightings with Range/Bearings, Alert Feed, Dossier
        }

        state Tab3_TPIE {
            [*] --> RenderTab3: Invasion % Projections, Fleet Vectors, Air Attrition
        }

        Tab1_IntelSummary --> Tab2_SIGINTMap: Press [2] or TAB
        Tab2_SIGINTMap --> Tab3_TPIE: Press [3] or TAB
        Tab3_TPIE --> Tab1_IntelSummary: Press [1] or TAB

        note right of WarRoomScreen
            Title: [ WAR ROOM - TAB X ]
            Header: Turn: {TURN_DATE}
            Hotkeys:
              [1] Tab 1  [2] Tab 2  [3] Tab 3
              [ENTER] Inspect Row -> DetailModal
              [F] In Tab 3 -> InterceptConstraintsModal
              [/] In Tab 2 -> DossierSearchModal
              [ESC] Back to MasterMenuScreen
        end note
    }

    WarRoomScreen --> MasterMenuScreen: Press [ESC]
    WarRoomScreen --> DetailModal: Press [ENTER] on any data row
    DetailModal --> WarRoomScreen: Press [ENTER] or [ESC] to return

    WarRoomScreen --> InterceptConstraintsModal: Press [F] in Tab 3
    InterceptConstraintsModal --> WarRoomScreen: Submit / Cancel

    WarRoomScreen --> DossierSearchModal: Press [/] in Tab 2
    DossierSearchModal --> WarRoomScreen: Submit / Cancel

    state WikiScreen {
        [*] --> WikiRootMenu: Top-level categories
        WikiRootMenu --> TechnicalDetailsMenu: Select [2] TECHNICAL DETAILS
        WikiRootMenu --> WikiArticleView: Select Leaf Category
        
        state TechnicalDetailsMenu {
            [*] --> TechSubTopics: 5 Sub-topics
            TechSubTopics --> ErrorRegistryMenu: Select [5] ERROR REGISTRY
        }

        state ErrorRegistryMenu {
            [*] --> ErrorSubTopics: [101], [102], [201], [202], [301], [401]
            ErrorSubTopics --> ErrorArticleView: Select Error Code
            ErrorArticleView --> ErrorSubTopics: Press [ESC]
        }

        note right of WikiScreen
            Title: [ WIKI (SYSTEM & ERROR REGISTRY) ]
            Layout: Left Menu (cols 2-42), Right Pop-up Explanation (cols 44-78)
            Navigation: Hierarchical tree with recursive ESC traversal
        end note
    }

    WikiScreen --> MasterMenuScreen: Press [ESC] at root category level

    state SettingsScreen {
        [*] --> SettingsList
        note right of SettingsList
            Title: [ SETTINGS ]
            Header: Turn: {TURN_DATE}
            Configurable Items:
              - Allied SIGINT Confidence [HIGH/PROBABLE]
              - Strike Concentration Min Cruisers [Default: 3]
              - Strike Concentration Radius Hexes [Default: 10]
            Each row displays inline description
            [ESC] Save & Return to MasterMenuScreen
        end note
    }

    SettingsScreen --> MasterMenuScreen: Press [ESC] or [ENTER] to save

    state ExitModal {
        [*] --> PromptConfirm: "PURGE MEMORY AND TERMINATE SESSION? (Y/N)"
        note right of PromptConfirm
            Overlay Pop-up
            [Y] Purge Memory & Close
            [N] or [ESC] Cancel
        end note
    }

    ExitModal --> MasterMenuScreen: Select [N] or Press [ESC] (if from MasterMenu)
    ExitModal --> StartScreen: Select [N] or Press [ESC] (if from StartScreen)
    ExitModal --> PurgeAndShutdown: Select [Y]

    state PurgeAndShutdown {
        [*] --> ZeroMemory: Zero and deallocate all in-memory campaign buffers
        ZeroMemory --> CloseConsole: Reset console color attributes
    }

    PurgeAndShutdown --> [*]: Program Exit
```
