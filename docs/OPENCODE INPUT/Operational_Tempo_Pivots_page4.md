# MUTHUR 6000 // GRAPHICS PRESENTATION BLUEPRINT
# Page: Operational Tempo & Pivots_page4

================================================================================================================
  GRAPHICS SUB-MENU CATEGORY 4: Operational Tempo & Pivots (The "Rhythm" Screen)
================================================================================================================

[ ASCII MOCKUP // VISUAL WIREFRAME ]
+------------------------------------------------------------------------------------------------------------+
| MUTHUR 6000 // [ TITLE: THEATER RHYTHM & OFFENSIVE OSCILLATION ]                                            |
+------------------------------------------------------------------------------------------------------------+
|                                                                                                            |
| -- SECTION 1: TEMPO VOLATILITY & BREATH ------------------------------------------------------------------- |
| [VOLATILITY]: [ ERRATIC ] [VAR: 8.4 ] [████████████████░░░░]                                                 |
| [OFF. BREATH]: [ WIDE/SHALLOW ] [HEXES: 14 ] [████████████░░░░░░]                                           |
|                                                                                                            |
| -- SECTION 2: STRATEGIC PIVOT LATENCY ---------------------------------------------------------------------- |
| [LABEL: TIME ELAPSED BETWEEN OFFENSIVE PHASES (DAYS)                                                       ] |
|                                                                                                            |
|   DAYS                                                                                                     |
|    10 |                                                     .::'                                           |
|     8 |                                             .::'                                                    |
|     6 |                                     .::'                                                            |
|     4 |                            .::'                                                                    |
|     2 |                    .::'                                                                            |
|     0 +-----------------------------------------------------------------------                             |
|        PIVOT 1 PIVOT 2 PIVOT 3 PIVOT 4 PIVOT 5 PIVOT 6 PIVOT 7 PIVOT 8   <-- PIVOT INDEX                     |
|                                                                                                            |
| -- SECTION 3: COORDINATION SYMMETRY ------------------------------------------------------------------------ |
| [PHILL / MALAYA]: [ SYMMETRIC ] [WINDOW: 4HRS] [████████████████████]                                       |
| [JAVA / BORNEO  ]: [ ASYMMETRIC ] [WINDOW: 48HRS] [████░░░░░░░░░░░░░░]                                      |
| [SULU / GUAM    ]: [ NO CORRELATION ] [WINDOW: N/A] [░░░░░░░░░░░░░░░░]                                     |
|                                                                                                            |
+------------------------------------------------------------------------------------------------------------+
| [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]                                                             |
| "The high volatility in OPTEMPO suggests the enemy is alternating between heavy strikes and lulls,       |
| potentially to mask logistical replenishment. Symmetry in Malaya/Phils indicates a coordinated theater   |
|-wide offensive."                                                                                            |
+------------------------------------------------------------------------------------------------------------+

[ SPECIFICATION DETAILS ]
1. DATA SOURCES:
    - File(s): combatreport_*.txt, aoperationsreport_*.txt
    - Extracted Keywords: "Air attack on", "Amphibious Assault", "Combat Events"

2. MATHEMATICAL FORMULATION:
    - Volatility: Standard Deviation of Daily Events.
    - Pivot Latency: StartDate(Offensive N) - EndDate(Offensive N-1).
    - Symmetry: Boolean (Raid in Region A AND Raid in Region B on same date).

3. X / Y AXIS DEFINITIONS:
    - X-Axis: Pivot sequence number.
    - Y-Axis: Days elapsed.

4. TACTICAL DEDUCTION LOGIC:
    - If Volatility == Low AND Breath == Narrow -> "Enemy is conducting a methodical, singular push."

================================================================================================================
[ DEVELOPER IMPLEMENTATION GUIDE ]

ALGORITHMS:
- Operational Tempo Volatility:
    1. Create a list of DailyEventCounts for the last 14 days.
    2. Calculate Variance = Sum( (x - mean)^2 ) / n.
    3. StandardDeviation = sqrt(Variance).
- Offensive Breath:
    1. For a single day, extract all hexes where combat occurred.
    2. Breath = DistinctCount(Hexes).
- Strategic Pivot Latency:
    1. Define "Offensive" as 3+ days of combat in a specific region.
    2. Mark EndDate of Offensive A and StartDate of Offensive B.
    3. Latency = DateDiff(StartDate_B, EndDate_A).
- Coordination Symmetry:
    1. Filter raids by region.
    2. If (RegionA_RaidCount > 0) AND (RegionB_RaidCount > 0) on same day -> Symmetric.

TIPS FOR THE CODING AI:
- Event Definition: Be careful to count "Engagement" events and not just "Sighting" events for OPTEMPO.
- Region Mapping: Use a configuration file to define which hexes belong to "Malaya", "Philippines", etc.
- Offset Dates: Since some events happen at "AM" and others "PM", treat a "Day" as a single unit unless the logs provide timestamps.
