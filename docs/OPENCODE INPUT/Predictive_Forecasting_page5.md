# MUTHUR 6000 // GRAPHICS PRESENTATION BLUEPRINT
# Page: Predictive Forecasting_page5

================================================================================================================
  GRAPHICS SUB-MENU CATEGORY 5: Predictive Forecasting (The "AI" Screen)
================================================================================================================

[ ASCII MOCKUP // VISUAL WIREFRAME ]
+------------------------------------------------------------------------------------------------------------+
| MUTHUR 6000 // [ TITLE: PROBABLE AXIS & GLOBAL TEMPO PROJECTION ]                                           |
+------------------------------------------------------------------------------------------------------------+
|                                                                                                            |
| -- SECTION 1: GLOBAL TEMPO PHASE -------------------------------------------------------------------------- |
| [CURRENT PHASE]: [ STRIKE ] [CONFIDENCE: 88% ] [██████████████████░░]                                       |
| [NEXT PHASE PRED]: [ CONSOLIDATION ] [PROBABILITY: 62%] [████████████░░░░░]                                 |
|                                                                                                            |
| -- SECTION 2: PREDICTIVE TARGETING & AXIS ------------------------------------------------------------------ |
| [LABEL: PROBABLE AXIS OF ADVANCE (VECTOR PROJECTION)                                                       ] |
|                                                                                                            |
|    Y                                                                                                       |
|   10 |                                                                                                     |
|    8 |                                  (Target Cluster)                                                    |
|    6 |                      (Now)  -->  [ X ]                                                              |
|    4 |                        O                                                                            |
|    2 |                                                                                                     |
|    0 +-----------------------------------------------------------------------                             |
|       0 2 4 6 8 10  X                                                                                        |
|                                                                                                            |
| -- SECTION 3: NAVAL TRENDS & MIGRATION -------------------------------------------------------------------- |
| [SUB MIGRATION]: CLUSTER ALPHA: [ MOVING SOUTH-WEST ] [VELOCITY: 1.2 hex/d] [██████░░░░░░]                   |
| [MINE PATHING]: PRIMARY TREND: [ NORTH-EAST ] [HIT DENSITY: HIGH] [██████████████░░]                        |
| [INT. BLINDSPOT]: SECTOR 77-80 : [ RISK: CRITICAL ] [TIME SINCE SIGHTING: 4d] [████████████████████]       |
|                                                                                                            |
+------------------------------------------------------------------------------------------------------------+
| [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]                                                             |
| "Probable axis suggests a shift toward the Southern Philippines. The transition to the STRIKE phase     |
| matches the high force concentration index in the target cluster."                                         |
+------------------------------------------------------------------------------------------------------------+

[ SPECIFICATION DETAILS ]
1. DATA SOURCES:
    - File(s): asigint_*.txt, combatreport_*.txt
    - Extracted Keywords: coordinates, "SS", "mines cleared", "Sighting"

2. MATHEMATICAL FORMULATION:
    - Probable Axis: Linear projection of last 3 CoG vectors.
    - Target Cluster: Intersection of (SigInt Ship Positions) AND (Recent Air Raid Targets).
    - Global Phase: Weighted score of (Volatility, Resource Drain, and Movement Vector).

3. X / Y AXIS DEFINITIONS:
    - X-Axis: Map X Coordinate.
    - Y-Axis: Map Y Coordinate.

4. TACTICAL DEDUCTION LOGIC:
    - If Phase == Strike AND Axis points to Base X -> "Alert Base X for imminent attack."

================================================================================================================
[ DEVELOPER IMPLEMENTATION GUIDE ]

ALGORITHMS:
- Probable Axis of Advance:
    1. Get CoG coordinates for Days T, T-1, T-2.
    2. Calculate Average Vector V = (CoG_T - CoG_T-2) / 2.
    3. Projected_Pos = CoG_T + V.
- Target Cluster Identification:
    1. Find all hexes in asigint with high ship concentrations.
    2. Find all hexes in combatreport that were air-raid targets in the last 3 days.
    3. TargetCluster = Intersection of these two sets.
- Global Tempo Phase State Machine:
    1. Assign points based on daily metrics:
       - Sighting Spike? +1 Build-up
       - Combat Events High? +1 Strike
       - Movement Zero/Low? +1 Consolidation
       - Vector moves away from target? +1 Withdrawal.
    2. Phase = Max(Points).

TIPS FOR THE CODING AI:
- Vector Magnitude: Use Euclidean distance sqrt(dx^2 + dy^2) to determine the speed of advance.
- Submarine Clusters: Use DBSCAN or a simple radius-based clustering algorithm to identify "SS Clusters" rather than individual subs.
- Minefield Trends: Treat "Mines cleared" events as points on a path; use a simple line-fit to find the "Pathing Trend" vector.
