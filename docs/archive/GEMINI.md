# WITP-CIC ROCHEFORT 6000 — Agent Working Rules

This file is automatically loaded by Antigravity at the start of every session
on this project. These rules are non-negotiable.

---

## 1. MegaMemory — Active Use Is Mandatory

A MegaMemory knowledge graph is installed at `.megamemory/knowledge.db`.
It contains the full architecture of this project: every module, model,
design decision, invariant, and dependency relationship.

**You must use it. Every session. Without being asked.**

### Before touching any code

Call `understand()` with a query relevant to the task. Do this even if you
think you already know the answer. The graph contains invariants and
constraints that are not obvious from reading the code alone.

```
understand("topic of the current task")
understand("any constraint or policy that might be affected")
```

Minimum: **two `understand` calls** before writing or modifying any code.

### After completing a task

Update the graph to reflect what changed. Use whichever tools apply:

- `update_concept(id, changes)` — if an existing concept changed
- `create_concept(...)` — if something genuinely new was built
- `link(from, to, relation)` — if a new dependency or relationship was created

Do not skip this step. The graph is only useful if it stays current.
A stale graph is worse than no graph.

### When discovering something not yet in the graph

If you read code and find a detail, invariant, or pattern that is not
captured in any existing concept, add it immediately via `update_concept`
or `create_concept`. Do not wait until after the task.

---

## 2. Core Architectural Invariants (Never Violate)

These are recorded in the graph as `decision` concepts. Query them before
any task that touches the analytics pipeline, UI, or build config.

| Invariant | Concept ID |
|---|---|
| Zero network traffic — no external calls ever | `zero-network-traffic-policy` |
| Faction isolation — Allied and Japanese never mix | `faction-isolation-rule` |
| Single-file exe — no new native DLLs or runtimes | `witp-cic-rochefort-6000` |
| Read-only — never write to .pws save files | `witp-cic-rochefort-6000` |
| Zero Flicker Policy — CrtGlitch hard-disabled | `widgetsettings` |
| Window Frame Version Display (WPF Chrome only, strictly excluded from all terminal screens/footers) | `lower-right-frame-version` |

If a proposed change could affect any of these, **stop and flag it explicitly**
before writing code.

---

## 3. Before Adding Any NuGet Package

Query the graph first:

```
understand("single file exe publishing constraints")
understand("zero network traffic policy")
```

Then explicitly answer:
- Does this package add a native DLL that breaks `PublishSingleFile`?
- Does this package make any network calls?
- Is there a pure C# alternative already in the codebase?

If the answer to either of the first two is yes, **do not add the package**
without explicit user approval.

---

## 4. After Each Significant Code Change

Run a final `understand()` pass to confirm the graph still accurately
describes the modified component. If it does not, update it.

The goal: any future agent starting a session on this project should be
able to call `understand("any topic")` and get an accurate, complete answer
without reading a single source file.

---

## 5. Mandatory Architectural Baseline for All New Functionality

From now on, **any newly added functionality, analytical feature, ingestion parser, or UI component MUST utilize and build upon these 14 foundational pillars**:

1. **Streaming & Ingestion:**
   * MUST use lazy line streaming via `File.ReadLines()`. Zero full-file memory allocations.
   * MUST use `ReadOnlySpan<char>` / `Span<T>` tokenizers with `int.TryParse` for zero-allocation hot paths.
   * MUST use `OperationsReportParser` with compiled regexes and corroborating deduplication for all log ingestion.
2. **Temporal Dynamics & Decay:**
   * All intel items MUST implement `IDecayStrategy` (`SightingDecayStrategy`, `SubmarineDecayStrategy`, etc.) with UTC timestamps and age-based confidence degradation.
   * Background decay and purging MUST operate lock-free via `DecayBackgroundService` (`PeriodicTimer`).
3. **Spatial-Temporal Clustering:**
   * Contact aggregations and anomaly detection MUST utilize `HotspotClusteringEngine` with axial distance and DBSCAN-style neighborhood grouping.
   * Alert events MUST trigger through `EventHandler<HotspotDetectedEventArgs>`.
4. **Entity Resolution & Causal State Machine:**
   * All entity naming MUST resolve through `EntityResolutionService` mapping aliases to canonical `Guid` IDs.
   * Lifecycle transitions MUST be validated via `EntityStateMachine` preventing invalid transitions and predicting loss probabilities.
   * Parsers and processors MUST decouple state mutations via `DomainEventBus` in-memory mediator events.
5. **Geospatial & Kinematic Math:**
   * All spatial calculations MUST use `readonly struct HexCoordinate` and `HexGeoMathExtensions` (`GetDistanceTo`, `GetAdjacentHexes`, `ExtrapolatePosition`, `System.Numerics.Vector2`).
6. **Reactive World State:**
   * Global state MUST be stored in `WorldStateRepository` with `ConcurrentDictionary<Guid, UnitState>`.
   * Entities MUST implement `INotifyPropertyChanged` for reactive UI data-binding.
   * Turn transitions MUST generate immutable `WorldStateSnapshot` with async `System.Text.Json` serialization.
7. **Strategic Rules Engine:**
   * Tactical recommendations and strategic warnings MUST be implemented as async `IIntelRule` classes evaluated via `StrategicRulesEngine`.
8. **Sensor & Source Reliability Weighting:**
   * Source confidence MUST use the Strategy pattern (`ISensorProfile` / `SensorReliabilityService`) mapping trained visual (1.00), coastwatchers (0.85), SIGINT (0.70), and self-reported sightings (0.55).
   * Confidence scores MUST be held in immutable `AdjustedConfidenceScore` records without mutating raw parsed records.
9. **Logistical Network Flow & Supply Graphs:**
   * Supply routes and base throughput MUST be modeled via directed capacity graphs (`SupplyNetworkGraph` with `Dictionary<HexCoordinate, List<SupplyEdge>>`).
   * Base captures or severed nodes MUST automatically flag downstream bases/units as starved.
10. **Air Superiority & Reconnaissance Denial:**
    * Enemy air presence MUST generate 2D density heatmaps via `IAirDenialService` with category weights (1.5x Torpedo/Dive bombers) and an 8-hour exponential threat half-life.
11. **Bayesian Multi-Source Corroboration:**
    * Multi-observer evidence MUST combine via Bayesian Noisy-OR formula (`1 - Π(1 - Pi)`) capped at 0.99, gated by `EntityResolutionService` to differentiate independent observers from correlated units.
12. **Predictive Threat & Advance Modeling:**
    * Enemy movement momentum MUST be projected via `Vector2` velocity cones using `IPredictionService` snapped to strategic nodes in `BaseLocationRegistry`.
13. **Cross-File Gap Detection & Temporal Continuity:**
    * Turn discontinuities MUST be detected via `SortedDictionary<DateOnly, ...>` emitting `TemporalGapWarningEvent`, with linear hex interpolation and degraded confidence marking for missing days.
14. **Persistent Hazard & Minefield Layer:**
    * Environmental hazards (minefields, oil slicks) MUST be tracked in a dedicated `HazardStateRepository` (`ConcurrentDictionary<HexCoordinate, List<HazardState>>`) with `HazardDecayStrategy` and minesweeper clearance modifiers.

---

## 6. WITP:AE Master System Blueprint (Data Engineering & Time-Series Architecture)

When designing, building, or querying data pipelines, analytical models, or ingestion services for WITP:AE PBEM turns:

1. **Authoritative Ground Truth**:
   * The binary `.pws` save file is the authoritative primary source of truth. All `.xlsx` spreadsheets and `.txt` narrative logs are treated as derived artifacts.
2. **Schema-Compliant Daily State Snapshots (SCD Type 2)**:
   * `ship_daily_state`: PK `(snapshot_date, ship_name)` — includes `hull_type`, `class`, `nation`, `status`, `location_hex_x`, `location_hex_y`, `location_text`, `tf_id`, `fuel_pct`, `sys_dmg`, `float_dmg`, `eng_dmg`, `fire`, `crew_exp`.
   * `task_force_daily_state`: PK `(snapshot_date, tf_id)` — includes `mission_type`, `hex_x`, `hex_y`, `home_port`, `destination`, `flagship_name`, `lowest_fuel_pct`, `ship_count`.
   * `tf_composition_history`: PK `(snapshot_date, tf_id, ship_name)`.
   * `fleet_summary`: PK `(snapshot_date)`.
3. **Event-Sourced History**:
   * `lifecycle_events`: PK `(event_date, ship_name, event_type)` — Commissioned, Arrived, Damaged, Sunk, Lost.
   * `operations_report_log`: PK `(report_date, event_id)` — Combat, Movement, Strategic, Logistics.
4. **WITP:AE Domain Quirks & Handling**:
   * **Hex Parsing**: Always extract integers via regex from string coordinates like `At sea, TF 18 (48,128)` or `In port: Pearl Harbor (180,107)`.
   * **Cumulative Losses**: Delta-tracking is mandatory. Compare current loss state against previous turns to extract *new* sinkings without duplicating historical entries.
   * **Reinforcement Countdown**: Project exact arrival dates via `Arrival Date = snapshot_date + Days to arrival`.
   * **Target Tech Stack**: Target DuckDB + Parquet date partitioning (`/data/ships/date=YYYY-MM-DD/data.parquet`), PostgreSQL + TimescaleDB, or clean SQLite when implementing storage engines.

