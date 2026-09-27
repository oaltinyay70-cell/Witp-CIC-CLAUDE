# ROCHEFORT 6000 // GRAPHICS PRESENTATION BLUEPRINTS (ASCII WIREFRAME SPEC)
# File: docs/graphics-blueprints-template.md
# Instructions:
# 1. Edit the bracketed fields [ ... ] below for each category.
# 2. You can tweak the ASCII art diagram directly to show where you want labels, curves, and bars.
# 3. Fill in the "DATA SOURCES", "MATHEMATICAL LOGIC", and "COMMANDER DEDUCTIONS" sections.

==============================================================================================================
  GRAPHICS SUB-MENU CATEGORY 1: Trend Lines (The "What's Happening" Graphs)
==============================================================================================================

[ ASCII MOCKUP // VISUAL WIREFRAME ]
+------------------------------------------------------------------------------------------------------------+
| ROCHEFORT 6000 // [ TITLE: e.g. MODEL C AIR ATTRITION & SORTIE TELEMETRY (DEC 07 - DEC 20, 1941) ]           |
+------------------------------------------------------------------------------------------------------------+
|                                                                                                            |
| -- SECTION 1: HIGH-LEVEL METRICS / HORIZONTAL COMPARISON BARS -------------------------------------------- |
| [ROW 1]: [A6M2 ZERO ] SORTIES: [ 412 ] LOSSES: [ 14 ] RATE: [  3.4% ] [███░░░░░░░░░░░░░░░░░░░░]           |
| [ROW 2]: [G4M1 BETTY] SORTIES: [ 280 ] LOSSES: [ 38 ] RATE: [ 13.5% ] [███████████░░░░░░░░░░░░]           |
| [ROW 3]: [B5N2 KATE ] SORTIES: [ 195 ] LOSSES: [  9 ] RATE: [  4.6% ] [████░░░░░░░░░░░░░░░░░░░░]           |
| [ROW 4]: [D3A1 VAL  ] SORTIES: [ 210 ] LOSSES: [ 18 ] RATE: [  8.5% ] [███████░░░░░░░░░░░░░░░░]           |
|                                                                                                            |
| -- SECTION 2: TIME-SERIES CURVE / SCATTER PLOT ----------------------------------------------------------- |
| [LABEL: 14-DAY CUMULATIVE OPERATIONAL AIR LOSSES BY DAY (BRAILLE TIME-SERIES CURVE)                      ] |
|                                                                                                            |
|  Y-AXIS                                                                                                    |
|    40 |                                                       ::                                           |
|    35 |                                                   .::'                                             |
|    30 |                                               .::'                                                 |
|    25 |                                           .::'                                                     |
|    20 |                                       .::'                                                         |
|    15 |                                   .::'                                                             |
|    10 |                               .::'                                                                 |
|     5 |                   ....::::::''                                                                     |
|     0 +-----------------------------------------------------------------------                             |
|        D07 D08 D09 D10 D11 D12 D13 D14 D15 D16 D17 D18 D19 D20   <-- X-AXIS                                |
|                                                                                                            |
| -- SECTION 3: LOGISTICAL / FORCE READINESS METERS -------------------------------------------------------- |
| [TF 1]: KOTA BHARU TF : [FUEL: 88% ████████░░] [AMMO: 94% █████████░] [SORTIE FATIGUE: 18% LOW ]          |
| [TF 2]: LEGASPI TF    : [FUEL: 62% ██████░░░░] [AMMO: 51% █████░░░░░] [SORTIE FATIGUE: 42% MED ]          |
| [TF 3]: APARRI TF     : [FUEL: 34% ███░░░░░░░] [AMMO: 28% ██░░░░░░░░] [SORTIE FATIGUE: 68% HIGH]          |
|                                                                                                            |
+------------------------------------------------------------------------------------------------------------+
| [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]                                                             |
| "Loss rates for Betty bombers (13.5%) reflect unescorted daylight strikes against heavy Allied AA.         |
| Cumulative airframe attrition exceeds Japanese frontline replacement capacity by 180%."                    |
+------------------------------------------------------------------------------------------------------------+

[ SPECIFICATION DETAILS TO FILL ]
1. DATA SOURCES:
   - File(s): [ e.g. combatreport files, operations reports ]
   - Extracted Keywords / Regex: [ e.g. "Air attack on", "aircraft lost", "flak" ]

2. MATHEMATICAL FORMULATION:
   - Sortie / Loss Rate: [ Rate = (Losses / Sorties) * 100 ]
   - Curve Accumulation: [ Cumulative_Loss(t) = Sum(Daily_Losses[0..t]) ]

3. X / Y AXIS DEFINITIONS:
   - X-Axis Meaning: [ Campaign Days / Dates ]
   - Y-Axis Meaning: [ Cumulative Aircraft Losses ]

4. TACTICAL DEDUCTION LOGIC:
   - Logic: [ If Loss_Rate > 10% flag as unsustainable attrition ]


==============================================================================================================
  GRAPHICS SUB-MENU CATEGORY 2: Correlation & Logic Graphs (The "Why it's Happening" Graphs)
==============================================================================================================

[ ASCII MOCKUP // VISUAL WIREFRAME ]
+------------------------------------------------------------------------------------------------------------+
| ROCHEFORT 6000 // [ TITLE: e.g. STRIKE ABORT RATE VS DISTANCE & FLAK DENSITY ]                                |
+------------------------------------------------------------------------------------------------------------+
|                                                                                                            |
| -- SECTION 1: CORRELATION FACTORS / RATIO BARS ----------------------------------------------------------- |
| [FACTOR 1]: [DISTANCE 1-3 HEX ] ABORT RATE: [  2.1% ] FLAK: [ LIGHT  ] [██░░░░░░░░░░░░░░░░░░░░]           |
| [FACTOR 2]: [DISTANCE 4-6 HEX ] ABORT RATE: [  6.4% ] FLAK: [ MEDIUM ] [██████░░░░░░░░░░░░░░░░]           |
| [FACTOR 3]: [DISTANCE 7-10 HEX] ABORT RATE: [ 18.2% ] FLAK: [ HEAVY  ] [██████████████████░░]           |
| [FACTOR 4]: [NIGHT OPERATIONS ] ABORT RATE: [ 31.0% ] FLAK: [ RADAR  ] [███████████████████████████████]   |
|                                                                                                            |
| -- SECTION 2: CORRELATION SCATTER / LOGIC CURVE ---------------------------------------------------------- |
| [LABEL: MISSION FAILURE CORRELATION VS TARGET DISTANCE (NM / HEXES)                                      ] |
|                                                                                                            |
|  % ABORT                                                                                                   |
|    40 |                                                                 * *                                |
|    30 |                                                       * * *   *                                    |
|    20 |                                           *   *   *                                                |
|    10 |                             *   *   *                                                              |
|     0 +-----------------------------------------------------------------------                             |
|         40NM  80NM 120NM 160NM 200NM 240NM 280NM 320NM 360NM 400NM  <-- DISTANCE                           |
|                                                                                                            |
| -- SECTION 3: ENVIRONMENTAL / TARGET CORRELATION FACTORS -------------------------------------------------- |
| [PARAM 1]: TARGET WEATHER DEGRADATION : [BAD WX: 42% ████████░░░░] [EFFECT: -35% ACCURACY]                 |
| [PARAM 2]: CREW FATIGUE OVER 40 PTS   : [FATIGUE: 68% ████████████░] [EFFECT: 2.4x ABORT RISK]             |
|                                                                                                            |
+------------------------------------------------------------------------------------------------------------+
| [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]                                                             |
| "Strikes launched past 240 NM exhibit sharp exponential rise in aborts due to navigation drift             |
| and localized interceptor coverage. Sustained operations must establish forward staging fields."           |
+------------------------------------------------------------------------------------------------------------+

[ SPECIFICATION DETAILS TO FILL ]
1. DATA SOURCES:
   - File(s): [ ]
   - Extracted Keywords / Regex: [ ]

2. MATHEMATICAL FORMULATION:
   - Correlation Function: [ ]

3. X / Y AXIS DEFINITIONS:
   - X-Axis Meaning: [ ]
   - Y-Axis Meaning: [ ]

4. TACTICAL DEDUCTION LOGIC:
   - Logic: [ ]


==============================================================================================================
  GRAPHICS SUB-MENU CATEGORY 3: Strategic & Intelligence Overviews
==============================================================================================================

[ ASCII MOCKUP // VISUAL WIREFRAME ]
+------------------------------------------------------------------------------------------------------------+
| ROCHEFORT 6000 // [ TITLE: e.g. THEATER STRATEGIC FORCE RATIOS & REGIONAL BALANCE ]                           |
+------------------------------------------------------------------------------------------------------------+
|                                                                                                            |
| -- SECTION 1: THEATER FORCE BALANCE (ESTIMATED JAPANESE VS ALLIED CONFIRMED) ----------------------------- |
| [CARRIERS   ]: JAPAN: [  6-8 ] ALLIED: [ 3 ] RATIO: [ 2.3 : 1 ] [█████████████████░░░░░]                   |
| [BATTLESHIPS]: JAPAN: [ 10-12] ALLIED: [ 6 ] RATIO: [ 1.8 : 1 ] [████████████░░░░░░░░░░]                   |
| [CRUISERS   ]: JAPAN: [ 28-34] ALLIED: [18 ] RATIO: [ 1.7 : 1 ] [███████████░░░░░░░░░░░]                   |
| [DESTROYERS ]: JAPAN: [ 80-95] ALLIED: [52 ] RATIO: [ 1.6 : 1 ] [██████████░░░░░░░░░░░░]                   |
|                                                                                                            |
| -- SECTION 2: STRATEGIC COMMITMENT OVER TIME ------------------------------------------------------------- |
| [LABEL: TOTAL CAPITAL SHIP TONNAGE ENGAGED IN ACTIVE OFFENSIVE SECTORS                                   ] |
|                                                                                                            |
|  TONS (x1000)                                                                                              |
|   250 |                                                   ::::::::::                                       |
|   200 |                                           ::::::::          :::                                    |
|   150 |                                   ::::::::                     :::                                 |
|   100 |                           ::::::::                                                                 |
|    50 |                 ::::::::::                                                                         |
|     0 +-----------------------------------------------------------------------                             |
|        D01 D03 D05 D07 D09 D11 D13 D15 D17 D19 D21 D23 D25 D27 D29   <-- CAMPAIGN TURN                     |
|                                                                                                            |
| -- SECTION 3: REGIONAL DISPOSITION GAUGES ----------------------------------------------------------------- |
| [MALAYA SECTOR   ]: FORCE ALLOCATION: [ 45% █████████░░░░░░░░░░░] [PRIORITY: MAIN OFFENSIVE AXIS]           |
| [PHILIPPINES SEC ]: FORCE ALLOCATION: [ 35% ███████░░░░░░░░░░░░░] [PRIORITY: INVASION & ISOLATION]         |
| [CENTRAL PACIFIC ]: FORCE ALLOCATION: [ 20% ████░░░░░░░░░░░░░░░░] [PRIORITY: SCREENING & DIVERSION]        |
|                                                                                                            |
+------------------------------------------------------------------------------------------------------------+
| [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]                                                             |
| "Japanese strategic mass is heavily tilted west of Makassar Strait (80% combined tonnage).                 |
| Central Pacific remains vulnerable to Allied carrier hit-and-run raids due to sparse carrier screen."      |
+------------------------------------------------------------------------------------------------------------+

[ SPECIFICATION DETAILS TO FILL ]
1. DATA SOURCES:
   - File(s): [ ]
   - Extracted Keywords / Regex: [ ]

2. MATHEMATICAL FORMULATION:
   - Ratio Formulation: [ ]

3. X / Y AXIS DEFINITIONS:
   - X-Axis Meaning: [ ]
   - Y-Axis Meaning: [ ]

4. TACTICAL DEDUCTION LOGIC:
   - Logic: [ ]


==============================================================================================================
  GRAPHICS SUB-MENU CATEGORY 4: Tactical & Operational Telemetry
==============================================================================================================

[ ASCII MOCKUP // VISUAL WIREFRAME ]
+------------------------------------------------------------------------------------------------------------+
| ROCHEFORT 6000 // [ TITLE: e.g. COMBAT ENGAGEMENT RESOLUTION & SURFACE GUNNERY EFFECTIVENESS ]                |
+------------------------------------------------------------------------------------------------------------+
|                                                                                                            |
| -- SECTION 1: SURFACE GUNNERY HIT RATIOS & ORDNANCE CONSUMPTION ------------------------------------------ |
| [14-16 INCH ]: SHOTS: [ 240 ] HITS: [ 32 ] ACCURACY: [ 13.3% ] [█████████░░░░░░░░░░░░░░░]                 |
| [ 8 INCH    ]: SHOTS: [ 680 ] HITS: [ 58 ] ACCURACY: [  8.5% ] [██████░░░░░░░░░░░░░░░░░░]                 |
| [ 5-6 INCH  ]: SHOTS: [1420 ] HITS: [ 94 ] ACCURACY: [  6.6% ] [████░░░░░░░░░░░░░░░░░░░░]                 |
| [TORPEDOES  ]: SHOTS: [  84 ] HITS: [ 12 ] ACCURACY: [ 14.2% ] [██████████░░░░░░░░░░░░░░]                 |
|                                                                                                            |
| -- SECTION 2: ENGAGEMENT RANGE DISTRIBUTION / HIT HISTOGRAM ---------------------------------------------- |
| [LABEL: SURFACE ENGAGEMENT HIT PROBABILITY BY RANGE BAND (YARDS)                                         ] |
|                                                                                                            |
|  HIT %                                                                                                     |
|    25 |                   |                                                                                |
|    20 |                   |       |                                                                        |
|    15 |                   |       |                                                                        |
|    10 |           |       |       |       |                                                                |
|     5 |   |       |       |       |       |       |                                                        |
|     0 +-----------------------------------------------------------------------                             |
|         <5k    5-8k    8-12k   12-16k  16-20k  20-25k  >25k   <-- RANGE (YARDS)                            |
|                                                                                                            |
| -- SECTION 3: CRITICAL DAMAGE & PENETRATION METERS ------------------------------------------------------- |
| [BELT ARMOR PENETRATIONS ]: [ 28 PENETRATIONS // 42% SUCCESS ] [████████░░░░░░░░░░░░]                       |
| [TORPEDO BULKHEAD BREACH ]: [  8 BREACHES     // 66% FATAL   ] [█████████████░░░░░░░]                       |
|                                                                                                            |
+------------------------------------------------------------------------------------------------------------+
| [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]                                                             |
| "Surface gunfire effectiveness peaks inside 12,000 yards. Night radar-directed fire provides 2.2x          |
| hit advantage over optical rangefinders before dawn illumination."                                         |
+------------------------------------------------------------------------------------------------------------+

[ SPECIFICATION DETAILS TO FILL ]
1. DATA SOURCES:
   - File(s): [ ]
   - Extracted Keywords / Regex: [ ]

2. MATHEMATICAL FORMULATION:
   - Accuracy Rate: [ ]

3. X / Y AXIS DEFINITIONS:
   - X-Axis Meaning: [ ]
   - Y-Axis Meaning: [ ]

4. TACTICAL DEDUCTION LOGIC:
   - Logic: [ ]


==============================================================================================================
  GRAPHICS SUB-MENU CATEGORY 5: [ TBD ]
==============================================================================================================

[ ASCII MOCKUP // VISUAL WIREFRAME ]
+------------------------------------------------------------------------------------------------------------+
| ROCHEFORT 6000 // [ TITLE: TBD ]                                                                              |
+------------------------------------------------------------------------------------------------------------+
|                                                                                                            |
| -- SECTION 1: TOP METRIC BARS ---------------------------------------------------------------------------- |
| [ITEM 1]: [ ... ]                                                                                          |
| [ITEM 2]: [ ... ]                                                                                          |
|                                                                                                            |
| -- SECTION 2: CENTRAL PLOT / CURVE ----------------------------------------------------------------------- |
| [LABEL: TBD                                                                                              ] |
|                                                                                                            |
|  Y-AXIS                                                                                                    |
|       |                                                                                                    |
|       |                                                                                                    |
|     0 +-----------------------------------------------------------------------                             |
|                                                                       <-- X-AXIS                           |
|                                                                                                            |
| -- SECTION 3: STATUS / GAUGES ---------------------------------------------------------------------------- |
| [GAUGE 1]: [ ... ]                                                                                         |
|                                                                                                            |
+------------------------------------------------------------------------------------------------------------+
| [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]                                                             |
| [ ... ]                                                                                                    |
+------------------------------------------------------------------------------------------------------------+

[ SPECIFICATION DETAILS TO FILL ]
1. DATA SOURCES: [ ]
2. MATHEMATICAL FORMULATION: [ ]
3. X / Y AXIS DEFINITIONS: [ ]
4. TACTICAL DEDUCTION LOGIC: [ ]


==============================================================================================================
  GRAPHICS SUB-MENU CATEGORY 6: [ TBD ]
==============================================================================================================

[ ASCII MOCKUP // VISUAL WIREFRAME ]
+------------------------------------------------------------------------------------------------------------+
| ROCHEFORT 6000 // [ TITLE: TBD ]                                                                              |
+------------------------------------------------------------------------------------------------------------+
|                                                                                                            |
| -- SECTION 1: TOP METRIC BARS ---------------------------------------------------------------------------- |
| [ITEM 1]: [ ... ]                                                                                          |
| [ITEM 2]: [ ... ]                                                                                          |
|                                                                                                            |
| -- SECTION 2: CENTRAL PLOT / CURVE ----------------------------------------------------------------------- |
| [LABEL: TBD                                                                                              ] |
|                                                                                                            |
|  Y-AXIS                                                                                                    |
|       |                                                                                                    |
|       |                                                                                                    |
|     0 +-----------------------------------------------------------------------                             |
|                                                                       <-- X-AXIS                           |
|                                                                                                            |
| -- SECTION 3: STATUS / GAUGES ---------------------------------------------------------------------------- |
| [GAUGE 1]: [ ... ]                                                                                         |
|                                                                                                            |
+------------------------------------------------------------------------------------------------------------+
| [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]                                                             |
| [ ... ]                                                                                                    |
+------------------------------------------------------------------------------------------------------------+

[ SPECIFICATION DETAILS TO FILL ]
1. DATA SOURCES: [ ]
2. MATHEMATICAL FORMULATION: [ ]
3. X / Y AXIS DEFINITIONS: [ ]
4. TACTICAL DEDUCTION LOGIC: [ ]
