# MUTHUR 6000 // GRAPHICS PRESENTATION BLUEPRINT
# Page: Logistical & Attrition Telemetry_page1

================================================================================================================
  GRAPHICS SUB-MENU CATEGORY 1: Logistical & Attrition Telemetry (The "Health" Screen)
================================================================================================================

[ ASCII MOCKUP // VISUAL WIREFRAME ]
+------------------------------------------------------------------------------------------------------------+
| MUTHUR 6000 // [ TITLE: NAVAL & AIR ATTRITION METRICS (CAMPAIGN DURATION) ]                                  |
+------------------------------------------------------------------------------------------------------------+
|                                                                                                            |
| -- SECTION 1: ATTRITION & DRAIN METRICS -------------------------------------------------------------------- |
| [K/L RATIO]: CURRENT: [ 1.2:1 ] SLOPE: [ IMPROVING ] [██████████████░░░░░░░░░░]                               |
| [RES. DRAIN]: AIR LOSSES: [ 42/WK ] SHIP LOSSES: [ 2/WK ] [████████░░░░░░░░░░░░░]                               |
|                                                                                                            |
| -- SECTION 2: OPERATIONAL TEMPO (OPTEMPO) TIME-SERIES ------------------------------------------------------ |
| [LABEL: ROLLING AVERAGE OF EVENTS PER TURN (BRAILLE TIME-SERIES CURVE)                                      ] |
|                                                                                                            |
|  EVENTS                                                                                                    |
|    20 |                                            ⢠⣶                                                     |
|    15 |                                    ⢀⣴⡿⠋                                                          |
|    10 |                        ⢀⣠⡾⠟⠁                                                                   |
|     5 |            ⢀⣀⣤⣴⣶⡶⠿⠿⠋⠁                                                                     |
|     0 +-----------------------------------------------------------------------                             |
|        D01 D02 D03 D04 D05 D06 D07 D08 D09 D10 D11 D12 D13 D14 D15 D16 D17 D18 D19 D20   <-- TURN            |
|                                                                                                            |
| -- SECTION 3: LOGISTICAL READINESS METERS ------------------------------------------------------------------ |
| [ESCORT DEN]: BASE SECTOR A: [ 0.4 units/hex ] [RATE: -12% ↓] [██████░░░░░░░░░░]                             |
| [FUEL LIMIT]: TF 402 (MAIN)  : [ MAX RANGE: 1200NM ] [LIMIT: 85% ] [█████████░░░░░]                          |
| [EXHAUSTION]: TF 105 (SNG)   : [ RETREAT DUE: D22 ] [CRITICAL: 15% ] [██░░░░░░░░░░░░]                         |
|                                                                                                            |
+------------------------------------------------------------------------------------------------------------+
| [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]                                                             |
| "The positive slope in K/L ratio suggests Japanese combat effectiveness is degrading. However, the OPTEMPO    |
| spike indicates a transition from build-up to active strike phase."                                           |
+------------------------------------------------------------------------------------------------------------+

[ SPECIFICATION DETAILS ]
1. DATA SOURCES:
    - File(s): combatreport_*.txt, aoperationsreport_*.txt
    - Extracted Keywords: "casualties reported", "sunk", "destroyed", "TFs", "fuel"

2. MATHEMATICAL FORMULATION:
    - K/L Ratio Slope: Linear regression slope of (AlliedLosses/JapaneseLosses) over last 7 days.
    - OPTEMPO: 3-day moving average of total combat events per turn.
    - Escort Density: (Sum of DD/CL/CA/BB in radius 5) / Area.

3. X / Y AXIS DEFINITIONS:
    - X-Axis: Campaign Turn (D01, D02...)
    - Y-Axis: Event count (Integer)

4. TACTICAL DEDUCTION LOGIC:
    - If K/L Slope < 0 AND OPTEMPO > Threshold -> "Enemy is pushing despite losses (Desperation)."

================================================================================================================
[ DEVELOPER IMPLEMENTATION GUIDE ]

ALGORITHMS:
- Kill-to-Loss Slope:
    1. Create a list of ratios [ (AlliedLosses_t / JapLosses_t) ] for t = current - 6 to current.
    2. Use Ordinary Least Squares (OLS) to find the slope 'm'.
    3. If m > 0: "Improving" (Enemy loss ratio is increasing).
- OPTEMPO Rolling Avg:
    1. Count total entries in combatreport for Day T.
    2. Average = (Count(T) + Count(T-1) + Count(T-2)) / 3.
- Escort Density Rate of Change:
    1. Find all escort-class ships in a fixed radius around base coordinates.
    2. Density_T = count / area.
    3. Change% = ((Density_T - Density_T-1) / Density_T-1) * 100.

TIPS FOR THE CODING AI:
- Handle Division by Zero: When calculating K/L ratios, if JapLosses == 0, use a small epsilon (0.001) or null.
- Date Parsing: Filenames use YYMMDD; ensure they are sorted chronologically before calculating slopes.
- Memory: Store parsed DailySnapshots in a List<DailySnapshot> to avoid re-reading files for every metric.
