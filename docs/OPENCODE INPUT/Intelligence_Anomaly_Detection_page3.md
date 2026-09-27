# ROCHEFORT 6000 // GRAPHICS PRESENTATION BLUEPRINT
# Page: Intelligence & Anomaly Detection_page3

================================================================================================================
  GRAPHICS SUB-MENU CATEGORY 3: Intelligence & Anomaly Detection (The "Trap" Screen)
================================================================================================================

[ ASCII MOCKUP // VISUAL WIREFRAME ]
+------------------------------------------------------------------------------------------------------------+
| ROCHEFORT 6000 // [ TITLE: SIGNAL ANOMALIES & INTELLIGENCE BLINDSPOTS ]                                       |
+------------------------------------------------------------------------------------------------------------+
|                                                                                                            |
| -- SECTION 1: SIGHTING ANOMALIES --------------------------------------------------------------------------- |
| [CLUSTERING]: SECTOR 44-50 : [ HIGH DENSITY ] [SIGHTINGS: 14 ] [███████████████░░]                          |
| [SIGHTING SPIKE]: SECTOR 12 : [ +210% ] [STATUS: ALERT] [█████████████████████]                             |
| [BLINDSPOT]: SECTOR 88-92   : [ ACTIVE/SILENT ] [RISK: HIGH] [███████████████████]                           |
|                                                                                                            |
| -- SECTION 2: TACTICAL ANOMALY LOG ------------------------------------------------------------------------- |
| [LABEL: RECENT ANOMALY DETECTION SEQUENCE                                                                 ] |
|                                                                                                            |
|  TIME  | TYPE                  | SECTOR | CONFIDENCE | STATUS                                                |
|  ------+-----------------------+--------+------------+------------------------------------------------------ |
|  D17.1 | ESCORTLESS TRANSPORT   | 52,84  | 85%        | [ POTENTIAL TRAP ]                                   |
|  D17.4 | COORDINATE CLUSTER    | 10,95  | 92%        | [ FORCE BUILD-UP ]                                   |
|  D17.9 | RAID-SIGHTING MISMATCH | 79,77  | 40%        | [ UNKNOWN ASSET ]                                    |
|                                                                                                            |
| -- SECTION 3: SECTOR SATURATION GAUGES --------------------------------------------------------------------- |
| [SANGOR SECTOR]: [ DENSITY: 62% ████████████░░░░ ] [PHASE: PRE-PUSH]                                         |
| [BATA VIA SECTOR]: [ DENSITY: 31% ██████░░░░░░░░░ ] [PHASE: SCREENING]                                         |
| [SULU SEA SECTOR]: [ DENSITY: 88% ███████████████ ] [PHASE: SATURATED]                                        |
|                                                                                                            |
+------------------------------------------------------------------------------------------------------------+
| [ TACTICAL ASSESSMENT & WHY THIS MATTERS BOX ]                                                             |
| "High density in Sulu Sea without corresponding naval movement suggests a hidden ambush or a massive      |
| transport screen. Sightings in Sector 12 indicate a sudden breach of our perimeter."                        |
+------------------------------------------------------------------------------------------------------------+

[ SPECIFICATION DETAILS ]
1. DATA SOURCES:
    - File(s): asigint_*.txt, combatreport_*.txt
    - Extracted Keywords: "Sighting", "Radio transmissions", "xAK", "xAP"

2. MATHEMATICAL FORMULATION:
    - Sighting Spike: CurrentCount / Avg(last 7 days).
    - Blindspot: (CombatEvents > 0) AND (SigIntSightings == 0).
    - Clustering: Sum sightings in radius 5 around hex (x, y).

3. X / Y AXIS DEFINITIONS:
    - X-Axis: Time / Sequence.
    - Y-Axis: Confidence Percentage.

4. TACTICAL DEDUCTION LOGIC:
    - If Blindspot == True AND CombatEvents == High -> "Enemy has operational stealth in sector."

================================================================================================================
[ DEVELOPER IMPLEMENTATION GUIDE ]

ALGORITHMS:
- Sighting Spike:
    1. Define sector (e.g., 5x5 hex grid).
    2. Count sightings in that sector for Day T.
    3. Compare to mean of sightings in that same sector for [T-1 ... T-7].
    4. If Ratio > 3.0 -> Spike Alert.
- Intelligence Blindspot:
    1. scan combatreport for "Attack at [X,Y]".
    2. Scan asigint for any sightings in [X+/-2, Y+/-2] for the previous 3 days.
    3. If Combat == True AND Sightings == 0 -> Blindspot found.
- Escort-less Transports:
    1. Filter asigint for transports (xAK, xAP).
    2. For each transport, check all other ships in asigint within radius 3.
    3. If no DD, CL, CA, or BB is present -> Mark as Escort-less.

TIPS FOR THE CODING AI:
- Regex for Coordinates: Use `(\d+),\s*(\d+)` to capture hex coordinates from text.
- Sector Mapping: Create a `Dictionary<SectorID, List<Sighting>>` to make lookup faster than scanning files repeatedly.
- Confidence Score: Weight "Sighting" high (90%) and "Radio Transmission" medium (50%) when calculating anomaly confidence.
