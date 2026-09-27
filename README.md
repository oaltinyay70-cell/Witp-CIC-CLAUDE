# WITP:AE Combat Information Center (WITP-CIC / ROCHEFORT 6000)

<div align="center">
  <h3>PACIFIC THEATER COMBAT INFORMATION CENTER & PREDICTIVE INTELLIGENCE ASSISTANT</h3>
  <p><em>An offline, zero-data-leakage intelligence helper and predictive analytics deck for Gary Grigsby's War in the Pacific: Admiral's Edition (PBEM).</em></p>
</div>

---

## 1. Executive Summary & Purpose

**WITP-CIC** (operating under the **ROCHEFORT 6000** terminal interface) is a specialized desktop intelligence companion designed specifically for players of *War in the Pacific: Admiral's Edition* (WITP:AE) engaged in asynchronous Play-By-Email (PBEM) campaigns.

In a standard PBEM campaign, each daily turn produces thousands of lines of dense, semi-structured plain-text reports (Signals Intelligence / SIGINT, Operational Summaries, Combat Reports, and Combat Event Logs) alongside binary save files (`.pws`). Manually reading, cross-referencing, and retaining these observations across months of real-world play leads to severe player fatigue, overlooked enemy task force build-ups, lost contacts, and uncorrected battle claims.

WITP-CIC ingests and synthesizes these daily logs across the entire campaign lifetime, transforming raw data into an interactive, proactive Combat Information Center (CIC). It surfaces critical theater intelligence that human analysis easily misses:
- **Amphibious invasion warnings & target probability ratings** 3–5 days prior to assault initialization.
- **Task force trajectories, advance headings, and nautical dead reckoning**.
- **Regional power shifts and operational tempo fluctuations**.
- **Merchant convoy corridors, chokepoint interdiction opportunities, and submarine migration vectors**.
- **Strict opponent-only tracking** under authentic Fog of War.

---

## 2. Core Architectural Principles & Sandboxing

1. **Zero Data Leakage & Offline Execution:**
   - The application executes strictly on the user's local Windows machine.
   - It maintains an absolute zero-network-traffic footprint: no external telemetry, no background network calls, and no cloud leakage.
   - All in-memory operational caches and analytical graphs are completely purged from RAM on application launch, campaign switching, and process exit.
2. **Absolute Faction Isolation:**
   - Intelligence belongs strictly to one player faction (Allies or Imperial Japanese Navy).
   - Allied campaigns process files from the Allied perspective; Japanese campaigns process from the Japanese perspective.
   - One campaign vault can never see, contaminate, or influence another.
3. **Single Compressed Vault Storage (`.wic`):**
   - All ingested logs, parsed contact tracks, entity histories, and analytical models are stored in a single compressed container file (`.wic`) under `C:\WitP-CIC\Campaigns\<CampaignName>.wic`.
   - No external database services (PostGIS, Neo4j, InfluxDB) or orphaned on-disk files are used; all graph, time-series, and hex-spatial structures serialize directly into the `.wic` vault.
4. **Absolute Read-Only Guarantee:**
   - WITP-CIC is strictly a passive analytical helper. It never writes to, modifies, or intercepts game save files (`.pws`) or game executable memory.

---

## 3. Key Operational Modules

### 🎖️ Combat Information Center (War Room)
- **`[1] Intelligence Summary`**:
  - **High-Value Targets (HVT):** Monitors spotted enemy carriers (CV/CVL/CVE), battleships (BB/BC), fleet oilers (AO), and amphibious assault transports (AP/APA/LST).
  - **Strike Concentrations:** Identifies groups of 3+ capital warships operating within configurable strike radii (default 10 hexes).
  - **Port Concentrations:** Tracks enemy naval and merchant tonnage anchored in forward bases.
  - **Established Routes:** Reconstructs merchant transit corridors and supply lines from repeated sightings.
  - **Opponent-Only Filter:** Strictly filters BDA and claims registers to opposing forces only; friendly losses are excluded.
- **`[2] SIGINT Map & Geospatial Logs`**:
  - Structured, tabular contact registers detailing localized hex coordinates $(X, Y)$, range, and sector bearings relative to friendly headquarters.
  - **Unified Dossier Search:** $O(1)$ indexed partial-name search across all 1,330 historical bases, naval vessels, land units, and air groups.
- **`[3] Graphics Telemetry Deck`**:
  - Full-screen visual presentation modules providing historical trend lines, attrition regressions, and campaign telemetry views.
- **`[4] TPIE Warning Dashboard`**:
  - Theater Predictive Intelligence Engine surfacing high-priority invasion alerts, staging base activity, and tactical intercept windows.
- **`[5] Predictive Models (Models A–G)`**:
  - **Model A (Amphibious Assault Engine):** Weighted Bayesian scoring correlating radio volume spikes, transport movements, and embarkation manifests to forecast target landing beaches.
  - **Model B (Fleet Trajectory & Intercept Projection):** Dead reckoning vector projection modeling course, speed, and candidate destination bases.
  - **Model C (Air Superiority & Attrition):** Exponential Moving Average (EMA) pilot and airframe attrition modeling.
  - **Model D (Logistical & Operational Pause Detector):** Task force turnaround and replenishment cycle estimation.
  - **Model F (Strike Concentration Detector):** Hex grid spatial density clustering alerting to enemy carrier battlegroups.
  - **Model G (Shipping Corridors & Chokepoints):** Transit corridor inference and submarine ambush positioning.
- **`[6] Heuristics & Logical Blueprints`**:
  - **Regional Power Shift (7-Day Delta):** Analyzes multi-theater naval and air mass migration across 10 strategic sectors (DEI, Malaya, Philippines, Solomons, New Guinea, Central Pacific, etc.).

---

## 4. Faction-Specific Intelligence Personas

The intelligence deck dynamically adapts its presentation and narrative analysis based on the player's commanded side:
- **Allied Player Command (`CMDR. J. ROCHEFORT`)**:
  - Features Station Hypo Commander Joseph Rochefort.
  - Grounded in US Navy cryptanalysis, JN-25 radio traffic decryption, high-confidence SIGINT intercepts, and coastwatcher reporting.
- **Japanese Player Command (`V.ADM. M. UGAKI`)**:
  - Features Combined Fleet Chief of Staff Vice Admiral Matome Ugaki.
  - Grounded in authentic Imperial Japanese Navy doctrine, *Senkoroku* war diary commentary, *Kantoku* decisive battle doctrine, and Type 97 flying boat / submarine scouting reconnaissance.
  - *The Rochefort panel is strictly isolated and never appears for an Imperial Japanese player.*

---

## 5. Algorithmic Rigor & Numerical Confidence Engine

- **Numerical Confidence Scoring (0%–100%):**
  - Subjective speculation and placeholder strings are strictly forbidden.
  - Evaluates explicit **Base Scores** (Radio Traffic = 30, Garrison Headcount = 50, Unit Moving = 65, Unit Located = 75, Direct Visual Recon = 80).
  - Rewards **Corroboration Bonuses** (+15 for repeated entity citations, +10 for multi-source hex corroboration).
  - Applies **Time Decay Multipliers** ($1.0\times$ for current turn, $0.8\times$ for $T-1$, $0.5\times$ for historical).
  - Categorizes findings into verified tiers: `UNCONFIRMED`, `PROBABLE`, `CONFIRMED`, and `VERIFIED`.
- **Full Evidence Transparency:**
  - Every finding provides an interactive drill-down reasoning modal (`[ENTER]`) exposing raw telemetry sources, mathematical formulas, coordinate history, and analytical proofs.
- **1,330-Base Geographic Hex Registry:**
  - Grounded in an authoritative database of 1,330 historical bases across 71 theater regions with permanent $(X, Y)$ coordinate stability and dynamic faction ownership updates.

---

## 6. Deliverables & Deployment

- **Single Self-Contained Binary:**
  - Distributed exclusively as an authoritative, versioned single-file Windows executable:  
    `dist/witp-cic-rochefort-alpha4-[build#].exe` (e.g. `witp-cic-rochefort-alpha4-13.exe`).
  - No auxiliary runtimes, DLLs, database servers, or secondary binary aliases are generated.
