# PROJECT STATUS: Witp-CIC-MUTHUR
# Date: 230926
# Status: Technical Specifications Finalized

## 1. Project Goal
Creation of a C# helper program (MUTHUR 6000) for "War in the Pacific Admiral's Edition" that parses game output text files to provide advanced statistical telemetry and predictive intelligence for the Allied side.

## 2. Finalized Metrics Set (22 Metrics)
The system is designed to track and visualize the following:

### Logistics & Density
1. Escort Density (Rate of change around bases)
2. Logistical Shadow (AO-based range prediction)
3. The "Bait" Pattern (Small raid -> Large raid sequence)
4. Kill-to-Loss Ratio Slope (Attrition trend)

### Strategic Intent & Vectors
5. Center of Gravity Shift (Mean coordinate movement)
6. Offensive Breath (Unique hex engagement count)
7. OPTEMPO Volatility (Event variance)
8. Resource Drain Rate (Weekly losses)
9. Sector Saturation (Unit density peaks)
10. Strategic Pivot Latency (Offensive transition time)

### Anomalies & Predictions
11. Coordinate "Clustering" (Sighting density)
12. Probable Axis of Advance (Vector projection)
13. Likely Target Cluster (Sighting/Raid overlap)
14. Force Concentration Index (Main vs Secondary effort)
15. Logistic Exhaustion Point (Refuel requirement prediction)
16. Global Tempo Phase (Build-up/Strike/Consolidation/Withdrawal)

### Naval Trends
17. Regional Power Shift (7-day concentration change)
18. Submarine Migration (SS cluster movement)
19. Minefield Mapping (Hit hex pathing trends)
20. Intelligence Blindspots (High activity/Low SIGINT)
21. Axis of Advance (Primary offensive vector)
22. Operational Tempo (Rolling average of events)

## 3. Delivered Documentation
The following specification files have been created in `C:\Users\ozgur\Documents\Witp-CIC-MUTHUR\docs\`:

- **Analytical Algorithms:** `analytical_algorithms.md` (The math and logic for all metrics).
- **UI Blueprints:** 
    - `Logistical_Attrition_Telemetry_page1.md`
    - `Strategic_Vector_Analysis_page2.md`
    - `Intelligence_Anomaly_Detection_page3.md`
    - `Operational_Tempo_Pivots_page4.md`
    - `Predictive_Forecasting_page5.md`

## 4. Technical Stack & Design
- **Language:** C#
- **UI Style:** MUTHUR 6000 (Monospace ASCII/Braille Terminal UI)
- **Data Source:** Dated `.txt` output files (combatreport, asigint, aoperationsreport).
- **Architecture:** Text Parsing $\rightarrow$ DailySnapshot Objects $\rightarrow$ Time-Series Analysis $\rightarrow$ ASCII Rendering.
