# MUTHUR 6000 // GRAPHICS PRESENTATION BLUEPRINT
# Page: Strategic Vector Analysis_page2

================================================================================================================
  GRAPHICS SUB-MENU CATEGORY 2: Strategic Vector Analysis (The "Movement" Screen)
================================================================================================================

[ ASCII MOCKUP // VISUAL WIREFRAME ]
+------------------------------------------------------------------------------------------------------------+
| MUTHUR 6000 // [ TITLE: ENEMY FORCE VECTORS & STRATEGIC MASS ]                                             |
+------------------------------------------------------------------------------------------------------------+
|                                                                                                            |
| -- SECTION 1: STRATEGIC MASS DISTRIBUTION ---------------------------------------------------------------- |
| [CONCENTRATION]: MAIN EFFORT: [ 72% ] SECONDARY: [ 28% ] [██████████████░░░░░░]                             |
| [POWER SHIFT]: MALAYA SECTOR: [ +12% ] PHILIPPINES: [ -4% ] [████████░░░░░░░░░░]                              |
|                                                                                                            |
| -- SECTION 2: CENTER OF GRAVITY (CoG) & AXIS OF ADVANCE ---------------------------------------------------- |
| [LABEL: JAPANESE FORCE CENTROID MAPPING (10x10 REGIONAL GRID)                                              ] |
|                                                                                                            |
|    Y                                                                                                       |
|   10 | . . . . . . . . . .                                                                                 |
|    8 | . . . . . X . . . .                                                                                 |
|    6 | . . . X X . . . . .     Vector: [NE]                                                                 |
|    4 | . . X X . . . . . .     Magnitude: 1.4 Hex/Day                                                      |
|    2 | . . . . . . . . . .     Current CoG: (5, 5)                                                          |
|    0 +----------------------                                                                               |
|       0 2 4 6 8 10  X                                                                                        |
|                                                                                                            |
| -- SECTION 3: LOGISTICAL RANGE PREDICTIONS ----------------------------------------------------------------- |
| [LOG. SHADOW]: TF 98 (SNG)   : [PRED. RANGE: 450NM ] [STATUS: STABLE] [██████████░░░░]                       |
| [LOG. SHADOW]: TF 112 (BWI)  : [PRED. RANGE: 120NM ] [STATUS: CRITICAL] [██░░░░░░░░░░░]                     |
| [LOG. SHADOW]: TF 45 (GUM)   : [PRED. RANGE: 800NM ] [STATUS: EXCELLENT] [█████████████░]                    |
|                                                                                                            |
+------------------------------------------------------------------------------------------------------------+
| [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]                                                             |
| "The Center of Gravity is shifting North-East toward the Philippines. Heavy concentration in the Main      |
| Effort indicates a decisive blow is being prepared."                                                          |
+------------------------------------------------------------------------------------------------------------+

[ SPECIFICATION DETAILS ]
1. DATA SOURCES:
    - File(s): asigint_*.txt, aoperationsreport_*.txt
    - Extracted Keywords: coordinates (xx, yy), "Sighting", "TFs"

2. MATHEMATICAL FORMULATION:
    - CoG: Average(x), Average(y) of all spotted Japanese units.
    - Shift Vector: CoG(t) - CoG(t-1).
    - Power Shift: (Sum of units in Region R at t) - (Sum at t-1).

3. X / Y AXIS DEFINITIONS:
    - X-Axis: Map X Coordinate.
    - Y-Axis: Map Y Coordinate.

4. TACTICAL DEDUCTION LOGIC:
    - If Vector Magnitude > 2.0 and Concentration > 60% -> "Offensive Push in Progress."

================================================================================================================
[ DEVELOPER IMPLEMENTATION GUIDE ]

ALGORITHMS:
- Center of Gravity (CoG):
    1. Extract all (x, y) pairs for Japanese ships from asigint.
    2. CoG_X = Sum(x) / Count; CoG_Y = Sum(y) / Count.
- Axis of Advance Vector:
    1. Calculate CoG for current day (T) and (T-3).
    2. Vector_X = CoG_X(T) - CoG_X(T-3); Vector_Y = CoG_Y(T) - CoG_Y(T-3).
    3. Direction = atan2(Vector_Y, Vector_X) converted to Compass Heading.
- Regional Power Shift:
    1. Define Region boundaries (minX, maxX, minY, maxY).
    2. Count units inside boundary for Day T and Day T-7.
    3. Delta = (Count_T - Count_T-7) / Count_T-7.
- Logistical Shadow (Range Prediction):
    1. Find closest AO (Tanker) to the TF.
    2. Max_Range = AO_Fuel_Capacity * Efficiency_Constant.

TIPS FOR THE CODING AI:
- Coordinate Normalization: The game map might have offsets. Ensure the CoG calculation uses consistent absolute coordinates.
- Outlier Filtering: If a single unit is spotted very far away (e.g., a lone scout), it can skew the CoG. Use a Median filter or exclude units outside 3 standard deviations.
- Vector Smoothing: Average the vectors over 3 days to avoid "jitter" from units moving back and forth.
