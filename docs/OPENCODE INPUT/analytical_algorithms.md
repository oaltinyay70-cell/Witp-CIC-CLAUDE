# ROCHEFORT 6000 // CORE ANALYTICAL ALGORITHMS SPECIFICATION
# File: docs/analytical_algorithms.md
# Target: C# Implementation for Witp-CIC-ROCHEFORT

This document provides the centralized mathematical and logical specifications for the ROCHEFORT 6000 telemetry engine.

================================================================================================================
1. SPATIAL & VECTOR ALGORITHMS
================================================================================================================

1.1 Center of Gravity (CoG) Calculation
- Purpose: Find the average "mass" of enemy forces.
- Logic:
    1. Collect all coordinates (x, y) of Japanese units from current day's SIGINT.
    2. CoG_X = Σ(x) / n; CoG_Y = Σ(y) / n.
- Usage: Use as the anchor point for "Axis of Advance" and "Regional Power Shift."

1.2 Axis of Advance (Vector Projection)
- Purpose: Predict where the enemy is moving.
- Logic:
    1. Let P(t) be the CoG at time t.
    2. Vector V = P(t) - P(t-3).
    3. Projected_Position = P(t) + V.
- Usage: Draw the arrow on the Predictive Forecasting screen.

1.3 Coordinate Clustering
- Purpose: Identify force build-ups.
- Logic:
    1. For each hex (x, y), count sightings in radius R (e.g., 5 hexes).
    2. If count > (7-day average * 3), mark as "Sighting Spike."
- Usage: Trigger alerts for "Saturated" sectors.

================================================================================================================
2. TEMPORAL & ATTRITION ALGORITHMS
================================================================================================================

2.1 Linear Regression Slope (The "Trend" Metric)
- Purpose: Determine if a stat is improving or worsening.
- Logic:
    1. Dataset: X = [1, 2, 3, 4, 5], Y = [Value_t-4, ..., Value_t].
    2. Slope m = [ n*Σ(xy) - Σx*Σy ] / [ n*Σ(x^2) - (Σx)^2 ].
- Usage: Apply to "Kill-to-Loss Ratio" and "Raid Size Escalation."

2.2 Operational Tempo (OPTEMPO) Rolling Average
- Purpose: Smooth out daily spikes to see the general phase.
- Logic:
    1. Count total "Combat Events" in combatreport_*.txt for the last 3 days.
    2. Avg = (Day1 + Day2 + Day3) / 3.
- Usage: Feed into the Global Tempo Phase state machine.

2.3 Strategic Pivot Latency
- Purpose: Measure the gap between offensive shifts.
- Logic:
    1. Identify "Offensive" as 3+ days of high combat activity in one region.
    2. Latency = Date(Start_Offensive_B) - Date(End_Offensive_A).
- Usage: Plotted on the "Rhythm" screen to see if the enemy is speeding up.

================================================================================================================
3. LOGISTICAL & ANOMALY ALGORITHMS
================================================================================================================

3.1 Logistical Shadow (Range Estimation)
- Purpose: Predict when a Task Force (TF) must retreat.
- Logic:
    1. Find the nearest AO (Tanker) in asigint.
    2. MaxRange = (AO_Fuel_Capacity * Efficiency_Constant).
    3. Exhaustion_Point = When Distance(TF, HomePort) >= MaxRange.
- Usage: Fill the "Logistic Exhaustion" meters.

3.2 Intelligence Blindspot Detection
- Purpose: Find "Stealth" enemy movements.
- Logic:
    1. Scan combatreport for "Attack at (x, y)".
    2. Scan asigint for any sightings in (x±2, y±2) for the previous 3 days.
    3. If (Combat == True) AND (Sightings == 0) -> Blindspot = True.
- Usage: Display as "Risk: High" in the Intelligence screen.

3.3 The "Bait" Pattern Detection
- Purpose: Spot diversions.
- Logic:
    1. Sequence: [Raid_A (Low Value)] followed by [Raid_B (High Value)] within 12 hours.
    2. Verification: Check if Allied CAP was moved to Raid_A's hex before Raid_B hit.
- Usage: Mark as "Potential Trap" in the Anomaly log.

================================================================================================================
4. GLOBAL TEMPO STATE MACHINE
================================================================================================================

- Purpose: Classify the overall war state.
- Logic: Weighted Scoring System.
    - IF (SightingSpike == True) AND (Combat == Low) -> +2 Build-up.
    - IF (Combat == High) AND (ResourceDrain == High) -> +2 Strike.
    - IF (Movement == Low) AND (Combat == Medium) -> +2 Consolidation.
    - IF (Vector == AwayFromTarget) -> +2 Withdrawal.
- Final State = Max(Scores).

================================================================================================================
IMPLEMENTATION NOTES FOR THE CODER
================================================================================================================
- Time Complexity: All algorithms are O(n) where n is the number of days/reports.
- Space Complexity: O(d) where d is the number of daily snapshots kept in memory.
- Precision: Use `double` for all vector and slope calculations to avoid integer truncation.
- Sorting: ALWAYS sort files by date before running any time-series algorithm.
