# Sprint Change Proposal — 2026-09-21

**Subject:** Reassess Road Rage Simulator Traffic AI and decide whether to establish a Traffic V2 architecture.

**Status:** Approved by Kenan and applied to planning/architecture artifacts on 2026-09-21. Traffic V2 implementation is not authorized by this approval.

**2026-09-22 owner clarification:** Story 5.14 is complete. Commit `210f48811e3f99fbb93c5d2aa75885e65edcf878`, the existing tests/artifacts and the behavioral catalog are accepted as the V1 reference baseline. Additional Story 5.14 runtime replay, PlayMode-harness repair and creation of a legacy branch/tag are not prerequisites for Road World Model architecture; `MANUAL`/`GAP` cases remain future V2 regression requirements.

**2026-09-22 Road World Model update:** the Fast-path architecture package was **accepted by the owner on 2026-09-22** — AD-43 through AD-47 are `[ADOPTED]`, with two recorded clarifications (re-import identity stability plus version canonicalization in AD-44, control-binding semantics in AD-46). No replacement story has been generated and Traffic V2 implementation remains unauthorized; stories may be planned from the accepted contracts, implementation starts only with an approved story.

**Scope classification:** **Major** — fundamental replanning with Product Manager and Solution Architect ownership.

**Recommended path:** Preserve Traffic V1 as a frozen behavioral reference, stop adding the remaining traffic responsibilities to its controller, and build a Traffic V2 orchestration architecture beside it. Reuse the sound low-level decision and physics kernels. Retire V1 only after scenario-by-scenario parity and integration gates pass.

---

## 1. Issue summary

### 1.1 Trigger

The immediate trigger is not one isolated anomaly. It is the convergence of four facts:

1. `NetworkedAIVehicleDriverController` has grown from basic waypoint following into a 1,194-line orchestration component owning route progression, route-history workarounds, leader sensing, longitudinal control, steering target continuity, emotion projection, stuck classification, emergency recovery, final intent composition, and network-facing route state.
2. The remaining Epic 5 backlog would add wider perception, overtaking, intersection arbitration, blocked-exit handling, deadlock escalation, rage/fear policy, targeted pursuit, litter-related routing and scale controls to that same flow.
3. Story 5.14 was marked done only after its original collision-response and physical-recovery acceptance criteria were explicitly cut from its approved implementation spec. The omitted behavior remains open and has no valid story owner.
4. Several fixes now compensate for the point-node representation itself: self-avoiding route memory, edge budgets, greedy exit redirection, passed-waypoint orbit detection, bounded aim-point recall, curve-biased sphere casts, and a delayed hidden teleport.

The decision is therefore whether to continue extending the current orchestration, refactor it incrementally, or preserve it as a reference while rebuilding the responsibility boundaries.

### 1.2 Important correction to the external review

The external architectural review is directionally useful but describes several mechanisms that do **not** exist in the current repository. Exact symbol checks under `Assets/RoadRage` found no `TrafficQueue`, `heldByBoundedWait`, `queueElapsedSeconds`, `WaitsFor`, `ResolveWinnerIndex`, or `TryEscalateRecovery`. There is currently no `JunctionManager`, `TrafficLightController`, `RightOfWay`, `WorldSnapshot`, `TrafficSituation`, `MotionPlanner`, `SpeedPlanner`, `SafetySupervisor`, or `RecoverySupervisor` either.

Consequently:

- There is no implemented peer-to-peer right-of-way tournament to replace.
- There is no four-stage recovery ladder in current code. The actual production recovery is a 60-second stuck timer followed by `RecoverAtWaypoint`, plus rollover/void recovery.
- There are not yet several independent subsystems simultaneously commanding throttle, brake and steering. Today there is one monolithic sequential owner that composes one `VehicleDriveIntent`.
- The architectural danger is **prospective but immediate**: implementing Stories 5.17 and 5.18 as written would create the overlapping mechanisms the review warns about.

### 1.3 Evidence inspected

This proposal cross-checks:

- The canonical SPEC package and 2026-09-13 course correction.
- `epics.md`, Epic 5 context, sprint status, prior 2026-09-15 and 2026-09-18 change proposals, and deferred-work records.
- The game architecture spine, especially AD-21 and AD-30 through AD-35.
- Current Traffic/LaneGraph implementation and its Git evolution from Stories 5.2, 5.9, 5.10 and 5.14.
- Traffic EditMode and PlayMode fixtures, including the Story 5.2, 5.9, 5.10, 5.14 and 5.16 suites.
- Open anomalies `ANO-5.10.02` and `ANO-5.10-03`.
- The actual symbols and callers through Graphify and direct source reading.

No Unity Editor was connected during this analysis. No new compilation or test result is claimed. The repository records 645/645 EditMode green after Story 5.16, while PlayMode evidence remains incomplete/order-sensitive and Story 5.14's manual runtime recipe remains unreported.

---

## 2. What architecture actually exists today

```text
TrafficSettings / lobby session values
                 |
                 v
PortalTrafficSpawner ---------> spawn/despawn at authored portals
                 |
                 v
LaneGraph (scene hierarchy of point nodes + successor indices)
                 |
                 v
NetworkedAIVehicleDriverController.FixedUpdate
  - project rage disposition into DriverProfile
  - detect rollover / void / stuck
  - sphere-cast one leader/player ahead
  - advance point-node route
  - apply self-avoidance / edge budget / nearest-exit fallback
  - IDM acceleration + reaction smoothing
  - node look-ahead + aim-point recall
  - compose steer + throttle/brake
                 |
                 v
VehicleDriveIntent
                 |
                 v
VehiclePhysicsBody (shared player/AI wheel physics)
                 |
                 v
Host Rigidbody + NetworkTransform presentation
```

Healthy separations already exist:

- `DriverModel` contains pure IDM/MOBIL and modulation functions.
- `LaneGraphRouting` contains pure route-selection and geometric helper functions.
- `VehicleDriveIntent` is the one command contract.
- `VehiclePhysicsBody` is shared by player and AI and is the only nominal physics actuator.
- The host is authoritative; clients present replicated state.
- `PortalTrafficSpawner` owns population and source/sink lifecycle rather than the vehicle controller.

The central problem is not a total absence of separation. It is that the **orchestration boundary is missing**: facts, permissions, goals, motion plans, speed constraints, safety vetoes and recovery requests are all represented as local control flow inside one `FixedUpdate`, or are not represented at all.

---

## 3. Direct answers to the requested questions

### 3.1 What architectural problems actually exist?

1. **The controller is both observer, planner, arbitrator and actuator composer.** It reads physics, performs perception, interprets behavior, advances the route, classifies waiting, chooses recovery, computes steering and longitudinal demand, and submits intent.
2. **The road model is a directed point graph, not a lane model.** A node has a transform, successors, weights and a portal/connector role. It has no lane width, longitudinal coordinate, adjacency, speed limit, stop line, junction movement, conflict zone, signal group or lane-change legality.
3. **Route, path and trajectory are conflated.** `WaypointIndex` is simultaneously route progress, geometric target and replicated debug state. A per-junction random successor draw substitutes for a route. A look-ahead point substitutes for a continuous path. No time-indexed trajectory exists.
4. **Recovery legitimacy is heuristic.** `IsDeliberateStop` infers a legitimate wait solely from a detected leader and a gap threshold. Other legitimate causes do not yet exist as data. Recovery therefore cannot reason about red lights, reservations, blocked exits, pedestrians or intentional Rage behavior.
5. **Representation defects produce behavioral patches.** The self-avoiding node walk prevents legitimate cycles; greedy Euclidean exit choice is not graph routing; the unreachable-waypoint predicate skips point targets the vehicle cannot recapture; curve-biased sphere casts compensate for a straight probe; aim-point recall compensates for target discontinuity.
6. **The replicated/debug state is too weak.** Clients receive only `WaypointIndex` and a projected `Behavior`. There is no route, lane coordinate, tactical goal, blocker, junction grant, chosen constraint, planned path or recovery reason to inspect.
7. **Tests strongly protect implementation shape.** Several tests count `static` tokens, inspect source strings, reflect private methods or replay copied kinematics. These are useful historical guards but are not a sufficient behavioral oracle for a new architecture.
8. **The plan and implementation artifacts disagree.** Epic Story 5.14 still claims `ANO-5.10-03` AC1–AC8 and physical recovery, while its approved story spec explicitly excludes them. Sprint status says done; the anomaly and deferred-work registry correctly say the work remains open.
9. **The architecture spine has a long-term conflict.** AD-34 mandates per-junction turn draws and “not a per-vehicle itinerary,” while the requested open-world target requires strategic route planning and replanning. AD-34 is valid for V1 source/sink traffic but cannot bind Traffic V2 unchanged.

### 3.2 Inherent versus accidental complexity

**Inherent complexity that must remain explicit:**

- Directed road topology, lane geometry and legal movements.
- Coherent occupancy/perception under a host simulation tick.
- Vehicle following and finite emergency braking.
- Route choice and replanning.
- Junction permissions, stop/yield/signal rules and blocked exits.
- Continuous local path generation and speed feasibility.
- Collision/off-route recovery without corrupting traffic rules.
- Driver personality and intentional rule-breaking.
- Host authority, stable decision ordering, debugging and scale LOD.

**Accidental complexity caused by the current representation or fix history:**

- Hierarchy-order node indices used as stable network identity.
- Runtime connector joining by distance and direction.
- Per-vehicle `bool[]` self-avoidance to stop roundabout loops.
- `hasDepartedSpawnNode` to distinguish birth from returning to a reused portal.
- Edge-count budget plus Euclidean nearest-exit redirection.
- Densifying roundabouts until node chords stop crossing curbs.
- `HasPassedUnreachableWaypoint` to escape orbit around a missed point.
- Aim-point recall to smooth jumps between point targets.
- `ScanSteerBlend` and a forward sphere cast to approximate curved-lane perception.
- Inferring “legitimate stop” from one leader gap.
- Waiting 60 seconds and hiding a teleport when no player is nearby.
- A lane-change cadence and MOBIL kernel with no adjacent-lane representation or candidates.
- Discrete `RageDisposition` projection inside the driver controller while continuous rage/fear modulation remains backlog.

### 3.3 Which systems overlap or compete?

There is not yet multi-system pedal competition. The overlap is responsibility overlap inside one owner:

| Responsibility | Current owner(s) | Problem |
| --- | --- | --- |
| Road topology | `LaneGraph`, `LaneNode` | Points encode lane, path, junction and portal semantics together. |
| Route selection | `LaneGraphRouting` + controller route memory | Pure weighted draw is mixed with stateful self-avoidance and fallback in the controller. |
| Perception | Controller sphere cast + portal overlap queries | No shared traffic occupancy model; semantic and physical queries are mixed. |
| Longitudinal decision | `DriverModel` + controller smoothing/pedal conversion | Sound kernel, but no general constraint aggregation for junctions/obstacles. |
| Steering/path following | `LaneGraphRouting` look-ahead + `ComputeSeekIntent` | Path geometry and steering control are fused around one point target. |
| Behavior policy | Rage feature + `NetworkedAIVehicleState.Behavior` + controller | Rage is projected into movement inside the controller rather than through an explicit policy contract. |
| Waiting/recovery | Leader-gap heuristic + stuck timer + rollover/void checks + teleport | No shared representation of blockers, permissions or progress expectation. |
| Debug state | `AIVehicleBehaviorDebugView` | Displays only a behavior label, not why the vehicle is doing it. |

Adding Stories 5.17–5.18 directly would create true competition: leader response, obstacle response, intersection permission, deadlock escalation, overtaking and recovery would all need to modify the same `FixedUpdate` output.

### 3.4 Invariants that must survive any redesign

1. Host owns all authoritative traffic decisions and physics simulation.
2. Clients never submit or predict authoritative AI decisions; they present replicated outcomes.
3. Exactly one component emits the final `VehicleDriveIntent` for an AI vehicle per simulation step.
4. Exactly one physics layer consumes the intent for both player and AI vehicles.
5. No planner writes `Rigidbody` position, rotation or velocity during nominal driving.
6. Source/sink population lifecycle remains portal-only; congestion, path failure or distance never silently despawns a vehicle mid-road.
7. Missing/invalid authored data fails visibly and does not invent hidden numeric defaults.
8. IDM remains finite at stopped-leader and zero-gap edge cases.
9. Lane-change safety veto cannot be bought by tactical gain or aggression.
10. A blocked exit is checked before a junction entry permission is used.
11. Rage/fear/personality modulate policy and parameters; they do not create a second physics or perception stack.
12. Static authoring data remains separate from host-owned runtime/session state.
13. Deterministic multiplayer means one authoritative host, stable ordering and seeded decisions—not cross-machine lockstep physics.
14. Every V2 behavior must be observable as facts, selected goal, constraints, permission, plan, safety result and recovery state.

### 3.5 Workarounds that should disappear

- Self-avoiding point-node history as the primary anti-loop rule.
- Greedy Euclidean nearest-exit routing.
- Point densification as the solution to curve/path quality.
- Passed-unreachable-waypoint skipping.
- Aim-point recall as the primary path-continuity mechanism.
- Curve-biased single sphere cast as the primary road perception mechanism.
- `IsDeliberateStop` as the global distinction between legitimate waiting and stuckness.
- `RecoverAtWaypoint`, including the “wait until unseen, then teleport” rule.
- Hierarchy order as network-stable road identity.
- Runtime auto-joining of road modules by proximity as authoritative connectivity.
- Lane-change evaluation without lane candidates.
- Behavior-only debug labels as the traffic diagnostic surface.

They should remain documented as regression lessons, not ported as mechanisms.

### 3.6 Components safe to reuse

**Reuse substantially unchanged:**

- `VehicleDriveIntent`.
- `VehiclePhysicsBody`, `VehicleProfile`, suspension/tire/steering kernels and arcade assists, subject to the already-open runtime verification gaps.
- Host-authoritative NetworkObject/NetworkTransform ownership pattern.
- `DriverProfile` authored data and most `DriverModel.ComputeAcceleration` IDM behavior.
- `DriverModel.TryEvaluateLaneChange` as a pure candidate-evaluation kernel once real adjacent-lane candidates exist.
- Traffic settings/session synchronization and configurable target headcount.
- Portal-only source/sink lifecycle as a gameplay invariant.
- Deterministic seeded-choice concept, but with an explicit session/vehicle seed rather than implicit dependence on spawn-order `NetworkObjectId` alone.
- Existing tests as sources of scenarios and numerical edge cases.

**Reuse behind adapters or after correction:**

- Current point geometry as import material for initial V2 lane centerlines; do not make V2 consume both models at runtime.
- `PortalTrafficSpawner`; retain population ownership but replace direct dependencies on V1 controller/state with a V2 vehicle lifecycle contract.
- Current look-ahead tests as path-continuity assertions; do not preserve the exact point-recall implementation.
- `AIVehicleBehaviorDebugView` presentation shell; replace its content with a structured V2 debug frame.

### 3.7 Components to redesign or replace

- `LaneNode`/`LaneGraph` road representation and validator.
- `NetworkedAIVehicleDriverController` as the orchestration owner.
- `NetworkedAIVehicleState.WaypointIndex` as the principal route/progress contract.
- `LaneGraphRouting` route selection and point-following portions.
- Leader detection and semantic traffic perception.
- Stuck/wait/recovery logic.
- Rage-to-driving integration.
- Junction, signal and right-of-way behavior, which should be designed once rather than appended piecemeal.
- Traffic debug/trace state and behavior-level automated scenarios.

### 3.8 Refactor, redesign in place, or V2?

**Recommendation: V2 beside a frozen V1, with selective reuse.**

- Incremental refactoring is appropriate for extracting the existing pure kernels and scenario tests, but not for evolving the V1 controller into the target architecture.
- A redesign entirely in place would remove the accepted executable behavioral reference while requirements are still being converted into V2 contracts.
- A clean-room rewrite that ignores V1 would rediscover the roundabout, bumper-gap, push, curb, NaN, spawn/exit and test-harness failures.

Use three protections:

1. Treat the approved commit plus behavioral catalog as the V1 reference; a tag/branch is optional repository hygiene, not a gate.
2. Keep V1 executable in development builds behind one composition choice until V2 parity, but freeze it to critical fixes only.
3. Build V2 in the main line beside V1, reusing the same physics and network foundations; never run both controllers on one vehicle.

If an optional reference branch/tag is created later, it is archival hygiene only, not a second product line or planning prerequisite.

### 3.9 Target boundaries and dependency direction

The external review's concepts are mostly sound, but making every box a service/class immediately would be over-engineering. The binding boundaries should be:

```text
RoadWorldModel (static authored lanes, junction movements, signals, portals)
        |
        v
TrafficFrame (host tick, occupancy/spatial index, dynamic actors)
        |
        v
AgentObservation (objective nearby facts; no permissions or commands)
        |
        +-------------> RoutePlanner (strategic lane sequence)
        |
        +-------------> JunctionCoordinator (movement grant / denial)
        |
        v
TacticalDecision (one DrivingGoal + explicit policy exceptions)
        |
        v
MotionAndSpeedPlan (geometric horizon + ordered speed constraints)
        |
        v
SafetyFilter (veto/clamp only, with explicit authorized-contact context)
        |
        v
VehicleDriveIntentComposer (sole command owner)
        |
        v
VehiclePhysicsBody

RecoverySupervisor observes progress and requests replanning or a recovery goal.
It never writes an intent, grants itself right-of-way, or mutates road rules.
```

Important refinements to the candidate model:

- **World Snapshot:** use one immutable `TrafficFrame` backed by shared occupancy/spatial indices. Do not copy the entire world for every vehicle.
- **Traffic Situation:** make it an immutable per-agent data result (`AgentObservation`), not a second mutable world model.
- **WaitContext:** a single cause is too lossy because a vehicle may simultaneously face a red signal, blocked exit and pedestrian. Represent a set of `ProgressConstraint`/`Blocker` facts with one selected dominant debug reason. Recovery legitimacy is derived from the set.
- **Motion versus speed planning:** keep separate contracts but allow one implementation initially. The minimum V2 deliverable is a continuous geometric path horizon plus a feasible speed envelope, not a full optimal time-parameterized trajectory framework.
- **Junction management:** coordinate per authored junction/movement, not through one global traffic singleton and not through pairwise vehicle tournaments.
- **NavMesh:** do not make it a Traffic V2 foundation. The lane network is the road path provider; physics queries or NavMesh may supplement off-road/unstructured recovery only if a measured need appears.

Dependency rule: lower layers know no gameplay policy. Physics knows only vehicle profiles and intents. Safety knows hazards and explicit maneuver authorization, not rage meters. Rage policy knows goals/permissions, not Rigidbody or wheel forces.

### 3.10 Normal traffic and Road Rage coexistence

Use one driving stack with a `DrivingPolicy` input. A policy may adjust:

- desired speed/headway/risk parameters;
- which traffic-law constraints are mandatory, bendable or intentionally violated;
- allowed surfaces and maneuver types;
- willingness to overtake or accept gaps;
- an explicit target and `AuthorizedContact` intent for designed ramming behavior.

It may not replace perception, routing, motion planning, safety filtering, intent composition or physics.

The safety filter must distinguish **traffic-law compliance** from **simulation safety**. A Rage policy may intentionally cross a normal priority or target another vehicle, but it still cannot emit NaN/Inf, demand physically impossible curvature, run an unauthorized pedestrian/third-party collision, or bypass the one-intent owner.

### 3.11 Regression scenarios to extract before implementation

**Existing behavior contracts to preserve:**

- Host-only AI decisions; clients read replicated presentation.
- Traffic spawns only at entry portals and leaves only at exit portals.
- Crowded portal insertion is queued, not dropped.
- Population never exceeds the resolved session target and refills after exit.
- Same seed produces the same weighted route decision; different vehicle seeds can diverge.
- Missing settings/profile/route yields an inert vehicle and one useful diagnostic.
- IDM free-road, desired-speed, stopped-leader and zero-gap cases remain finite.
- MOBIL safety veto remains independent of gain.
- Desired following gap is measured bumper-to-bumper, not center-to-center.
- Curved-path perception does not miss a leader immediately after a bend.
- Walking players are perceived as hazards.
- Steering/path targets remain continuous at road-segment boundaries.
- Nominal traffic crosses the authored curbed junction without entering the curb footprint.
- A laterally perturbed vehicle does not orbit a missed roundabout point forever.
- A vehicle can be pushed, displaced and physically affected without one-frame realignment.
- Gravity, wheel authority and arcade assists affect AI through the same physics as the player.
- No mid-road despawn or hidden teleport is used as congestion recovery.

**New V2 contract scenarios:**

- Stable decisions independent of per-MonoBehaviour update order.
- Route/path/trajectory identities are separate and inspectable.
- Legal roundabout circulation may revisit geometric areas without being rejected as a route loop.
- Stop, yield, priority-road, priority-to-right and signal phase permutations.
- Conflicting junction movements never hold simultaneous grants.
- A blocked exit prevents junction entry before priority is evaluated.
- A vehicle already committed to a junction clears it consistently.
- Red-light/stop/blocked-exit/pedestrian waits suppress recovery without relying on one timer heuristic.
- Head-on and wrong-way actors are recognized as hazards with deterministic responses.
- Static obstacle, abandoned car, player car and pedestrian cases select distinct facts but share the same planner.
- Collision response yields to physics, then requests reattachment/replanning without teleport.
- Gridlock detection and recovery do not violate non-negotiable permissions or create cyclic grants.
- Normal and Rage policies share the stack; an authorized ram does not authorize unrelated impacts.
- Near/mid/far LOD transitions preserve route progress, identity and host authority.
- Thirty-vehicle host plus client budget with costs separated for physics, perception, routing and networking.
- V1/V2 trace comparison on the same deterministic seed and scenario corpus.

### 3.12 Migration strategy

1. **Freeze the accepted reference.** Use the approved V1 commit, reconciled Story 5.14/anomaly ownership and current trace fixtures; do not reopen V1 validation or require a tag/branch before Road World Model design.
2. **Use the accepted behavioral oracle.** Carry its scenario catalog, current source-shape guards and cinematic replays into future V2 story/test design. Do not reopen V1 to manufacture additional proof.
3. **Introduce the V2 road contract first.** Author lane corridors and junction movements with validation. Use a one-time/editor conversion from current points for initial geometry; do not support two authoritative runtime graphs.
4. **Introduce the host traffic frame and observation model.** Prove stable ordering and bounded query cost before adding junction behavior.
5. **Deliver a vertical V2 cruise slice.** Route plan, continuous path horizon, IDM speed constraint, single intent owner, shared physics. Run beside V1 in the same `MVP_Run` scenario, selected by composition.
6. **Add junction grants, safety and recovery through their boundaries.** Each capability adds facts/constraints/goals; none gains a direct pedal path.
7. **Integrate Rage policy after the normal stack is stable.** Replace discrete behavior projection with continuous policy modulation and explicit rule exceptions.
8. **Run parity and scale gates.** No removal of V1 until required regression scenarios pass in `MVP_Run`, the two-player host/client smoke is credible, and the measured 30-vehicle budget has headroom.
9. **Retire, do not indefinitely maintain.** Remove the V1 runtime path after V2 parity approval, retaining the accepted commit and behavioral corpus for archaeology; any optional tag/branch is repository hygiene only.

This minimizes rediscovery by migrating requirements and tests first, then mechanisms.

### 3.13 Epic/story plan impact

- **Stories 5.1–5.16:** preserve as historical foundation work. Do not rewrite their history. Correct the Story 5.14 summary so it no longer claims collision response/recovery was delivered.
- **Stories 5.17–5.23:** do not implement as currently written. Their responsibilities must be replaced by Traffic V2 boundary stories.
- **`ANO-5.10-03`:** reassign from completed Story 5.14 to the V2 collision/recovery boundary story; keep status open.
- **Story 5.19:** its requirement survives, but implementation moves to the V2 policy boundary.
- **Story 5.20:** litter attribution can be retained later, but its “head to exit” behavior must consume V2 routing rather than add a V1 mode.
- **Story 5.22:** rewrite from a final one-off measurement into a baseline-plus-V2 budget comparison including LOD.
- **Story 5.23:** replace with a V2 parity/migration checkpoint.
- **Epic 6:** inventory/on-foot work independent of traffic may continue. Rage Road pursuit, litter return, hostile-driver confrontation and traffic-coupled economy work wait for the V2 behavior gates.
- **Epic 7:** integrated run work remains blocked on the V2 checkpoint.
- **Architecture spine:** add Traffic V2 invariants and supersede the “no per-vehicle itinerary” part of AD-34 for V2 while preserving portal-only lifecycle. Do not silently edit AD-34's history.
- **UX:** no broad redesign. Add only debug/developer presentation requirements and preserve accessible text reasons for traffic state.

### 3.14 Coherent decomposition by architectural boundary

This change is too large for one story. The recommended plan is to close current Epic 5 as the **Traffic V1/vehicle foundation baseline**, create a dedicated **Traffic V2 epic** before traffic-dependent confrontation/economy work, and renumber the currently untouched future epics if the Product Manager chooses integer-only epic IDs.

| Work package | Architectural boundary | Verifiable outcome |
| --- | --- | --- |
| V2-1 Accepted oracle input and V1 freeze | Contract/testing | Accepted scenario catalog and commit, V1 trace knowledge, reconciled anomaly/story ownership; no additional V1 validation or tag/branch plan. |
| V2-2 Road World Model and validator | Static world data | Lanes/corridors, widths, direction, speed, adjacency, portals, junction movements, stop lines, conflicts and signal groups validate without traffic runtime. |
| V2-3 Traffic frame and occupancy/perception | Dynamic facts | One stable host tick view, bounded indices, per-agent observations for vehicles/players/pedestrians/obstacles. |
| V2-4 Strategic route planning | Route | Deterministic lane-sequence route to a portal/destination; weighted variation is a policy cost, not a replacement for connectivity. |
| V2-5 Decision and intent spine | Ownership | One `DrivingGoal`, ordered constraints and one final intent owner; V2 cruise/follow works through existing physics. |
| V2-6 Motion path and speed envelope | Local planning | Continuous curvature-feasible path horizon plus IDM/limit/curve/stop constraints; no point-target patching. |
| V2-7 Junction coordination and signals | Permissions | Authored movement grants cover stop/yield/priority/lights/roundabouts/blocked exits with deterministic arbitration. |
| V2-8 Safety, collision and recovery | Supervision | Emergency veto, collision response, off-route reattachment and gridlock recovery without direct control or teleport. |
| V2-9 Lane change and unstructured avoidance | Tactical maneuvers | Adjacent-lane candidates, MOBIL safety, static/dynamic obstacle and wrong-way/head-on scenarios. |
| V2-10 Driver policy, Rage/Fear and targeted aggression | Gameplay policy | Normal and Rage behavior share the stack; continuous modulation and explicit rule exceptions are inspectable. |
| V2-11 Litter integration | Traffic consumer | Attribution/lifecycle from old Story 5.20 uses V2 routing/policy without creating another navigation mode. |
| V2-12 LOD, scale, networking and migration | Operational envelope | Near/mid/far simulation contracts, ~30-vehicle measurement, host/client smoke, V1/V2 parity and retirement decision. |

The package boundaries are deliberately architectural. If an individual package later needs several implementation stories, split by independently executable vertical behavior, not by document or token size.

---

## 4. Traffic V2 behavioral contract — approved architectural baseline

Kenan approved BC-1 through BC-12 on 2026-09-21 with these binding clarifications:

1. V2 replaces the point-node `LaneGraph` as the authoritative Traffic V2 runtime representation, while existing authored road geometry is reused or migrated through editor/import tooling where practical. The map is not rebuilt manually without evidence that migration is impossible or unsafe.
2. The Road World Model capabilities below are binding; its exact schema is not. Architecture must decide ownership across lane, road section, junction movement, signal and adjacency concepts before implementation stories are generated.
3. The V2 work packages are responsibility boundaries, not a mandatory one-package/one-story mapping. Safety, collision response and recovery may become separate executable stories.
4. Traffic LOD/scalability remains in the target architecture. Its implementation follows profiling of a functionally correct V2 unless an earlier measured constraint forces it forward.
5. `TrafficRules` are policy-visible rules that a `DrivingPolicy` may explicitly bend or violate. `SimulationInvariants` cannot be bypassed by any policy. The `SafetyFilter` enforces only small, objective simulation vetoes and must not become another tactical decision system.

### BC-1 Authority and tick coherence

- The host is the only authoritative Traffic V2 simulator.
- Each traffic decision cycle reads one immutable `TrafficFrame` version.
- Decision ordering and random draws are stable for the same session seed and inputs.
- Clients receive presentation/debug state; they do not run authoritative traffic planning.

### BC-2 Single command ownership

- Exactly one intent composer emits one `VehicleDriveIntent` per AI vehicle and physics step.
- Perception, routing, junctions, tactical policy, motion, speed, safety and recovery never write pedals, steering or Rigidbody state directly.
- `VehiclePhysicsBody` remains the sole nominal movement executor.

### BC-3 Road semantics

- A lane has stable identity, center geometry, width/corridor, travel direction, speed limit, predecessor/successor links and optional adjacent lanes.
- A junction exposes allowed lane-to-lane movements, stop lines, conflicts and signal/right-of-way metadata.
- Portals remain the only normal source/sink points.
- Invalid authoring fails validation before Play Mode where practical.

### BC-4 Separate route, path and trajectory

- `RoutePlan` is a strategic ordered lane/movement sequence.
- `PathHorizon` is continuous local geometry derived from the route and current maneuver.
- `MotionPlan` adds a feasible speed/acceleration envelope over that path.
- None is represented solely by the current target node index.

### BC-5 Facts, permissions, goals and constraints

- Perception produces objective observations only.
- Junction coordination produces explicit movement permission/denial and reasons.
- Tactical decision selects one goal and any explicit gameplay policy exception.
- Speed-related rules contribute named constraints; they do not independently brake.
- The final selected constraint and rejected alternatives are debug-visible.

### BC-6 Waiting and progress

- Waiting is represented by one or more named blockers/constraints, each with source, legitimacy, boundedness and start tick/time.
- Legitimate waits suppress recovery.
- Recovery is based on “permission/plan expected progress but progress did not occur,” not speed alone.
- A single dominant reason may be shown in UI, but the underlying blocker set is retained.

### BC-7 Junction safety

- Exit availability is evaluated before an entry grant.
- Conflicting movements cannot simultaneously own incompatible grants.
- Stable deterministic arbitration resolves ties; update order does not.
- Stop, yield, priority, signal and roundabout rules are authored movement rules, not inferred from arbitrary node cycles.

### BC-8 Motion and safety

- Planned paths respect the lane corridor, curvature/vehicle feasibility and current obstacles.
- Speed plans combine desired speed, road limit, curve limit, leader following, stop line, junction grant and obstacle constraints.
- Safety may veto or clamp a plan for imminent unintended collision or invalid/non-finite output.
- Intentional Rage contact requires explicit authorization identifying its target/scope; it is not a global safety bypass.

### BC-9 Recovery

- Recovery may request route replanning, a local alternative path, a controlled reverse/realignment goal or an escalation decision.
- Recovery cannot grant right-of-way, ignore a red signal, mutate a traffic rule or write drive intent.
- Normal runtime recovery does not teleport, reinsert or despawn a vehicle mid-road.
- Catastrophic out-of-world cleanup, if retained, is a separately declared development/failure policy and never masquerades as traffic behavior.

### BC-10 Personality and Road Rage

- One base driving stack serves normal, fearful, enraged and targeted drivers.
- Personality/Rage/Fear change parameters, costs, allowed maneuvers and explicit legal-rule exceptions.
- They never select another physics implementation or bypass perception and planning.
- One vehicle's emotional state cannot mutate another vehicle's driving parameters.

### BC-11 Lifecycle and scalability

- Traffic identity, route progress and behavior survive near/mid/far LOD transitions.
- Near traffic may use full physics/perception; lower LODs may use reduced cadence or abstract lane progress, but the host remains authoritative.
- No LOD transition creates a visible mid-road spawn/despawn or changes the selected route without an explicit replan reason.

### BC-12 Observability and validation

- A debug frame exposes route, lane coordinate, tactical goal, blockers, junction grant, active speed constraint, path/motion plan id, safety result and recovery state.
- Road validation covers topology, direction, width, curvature, unreachable portals, impossible connectors, missing stop/signal data and movement conflicts.
- Every historical traffic anomaly is represented by a named regression scenario or a documented reason it is obsolete.

---

## 5. Current mechanism classification

| Current rule/mechanism | Classification | Original problem | Still needed? | V2 owner | Dependencies/interactions | Regression if removed without replacement |
| --- | --- | --- | --- | --- | --- | --- |
| Host-only AI simulation | KEEP | Prevent client divergence | Yes | Traffic runtime / physics | Netcode, NetworkTransform | Clients disagree or mutate traffic. |
| One `VehicleDriveIntent` into shared physics | KEEP | Separate driving decision from Rigidbody | Yes | Intent composer → physics | Player/AI parity, AD-35 | Competing actuators and physics instability. |
| `DriverModel.ComputeAcceleration` IDM | KEEP | Realistic finite following | Yes | Speed planning kernel | Leader observation, profile | Tailgating, infinite zero-gap values, abrupt speed rules. |
| MOBIL pure safety/gain kernel | KEEP BUT REDESIGN | Future lane-change decision | Yes | Lane-change tactical planner | Adjacent lanes, followers, occupancy | Unsafe changes or duplicated heuristic. |
| `DriverProfileDef` authored parameters | KEEP | Remove controller literals; personalities | Yes | Driving policy/speed planner | Rage modulation | Hard-coded archetypes and untestable tuning. |
| Discrete `RageDisposition` profile modulation | KEEP BUT REDESIGN | Replace speed-only rage behavior | Yes, as continuous Rage/Fear policy | Driving policy | Rage meters, target, tactical permissions | Rage becomes a second driver or only a speed multiplier. |
| Portal-only spawn/despawn | KEEP | Living source/sink traffic without visible popping | Yes | Traffic lifecycle | Road portals, population | Mid-road disappearance and broken world continuity. |
| Configurable session population | KEEP | Avoid hard-coded cardinality | Yes | Run/traffic settings | Lobby sync, scale budget | Controller constants and unrepeatable scale tests. |
| Crowded portal queues insertion | KEEP | Do not drop traffic demand | Yes | Traffic lifecycle | Occupancy/perception | Population silently undershoots under congestion. |
| Weighted turn ratios | KEEP BUT REDESIGN | Route variety | Yes, as route cost/policy | Route planner | Session seed, destinations | Uniform/boring routes or non-deterministic branching. |
| Seed from `NetworkObjectId` | KEEP BUT REDESIGN | Deterministic per-instance variation | Yes | Host decision seed service | Session seed, stable traffic id | Spawn-order changes alter every later route unexpectedly. |
| Point-node `LaneGraph` | REMOVE / REPLACE | Minimal routed traffic authoring | The problem remains, representation does not | Road World Model | Scene authoring, portals | Cannot express width, lanes, movements, signals or legal lane changes. |
| Hierarchy-order node index as stable id | REMOVE / REPLACE | Cheap identical client index | Stable identity remains needed | Authored stable lane/movement ids | Serialization, networking | Reordering hierarchy changes runtime meaning. |
| Runtime proximity connector joining | REMOVE / REPLACE | Easy modular road placement | Connectivity validation remains needed | Road authoring/importer | Module seams | Wrong or orphan connections become runtime surprises. |
| Per-junction draw with no strategic route | KEEP BUT REDESIGN | Source/sink variation | Not sufficient long term | Route planner | Portal/destination, congestion | No destination, no reliable replan, wrong-way escape hacks. |
| Self-avoiding `traversedNodes` | REMOVE / REPLACE | Prevent roundabout/orbit loops | Eventual progress is needed | Route planner/recovery | Route horizon, junction topology | Legitimate cycles become impossible; workaround leaks into routing. |
| Edge budget | KEEP BUT REDESIGN | Bound wandering | Yes | Route planner/recovery policy | Route validity | Infinite wandering when authoring/path fails. |
| Greedy nearest-exit redirect | REMOVE / REPLACE | Escape budget/dead ends cheaply | Exit routing remains needed | Route planner shortest/cost path | Topology, closures | Euclidean choice can be unreachable or directionally wrong. |
| Nearest point-node insertion/reattachment | KEEP BUT REDESIGN | Start/recover from arbitrary position | Yes | Lane localization | Heading, corridor, occupancy | Reattach to wrong side/direction/lane. |
| Node look-ahead point | KEEP BUT REDESIGN | Reduce point-to-point corner cutting | Continuous path needed | Motion/path planner | Lane spline/corridor | Vehicles cut corners or target discontinuously. |
| Aim-point recall memory | REMOVE / REPLACE | Smooth target jump on node change | Continuity remains required | Motion/path planner by construction | Path horizon | Steering discontinuity returns if removed alone. |
| Passed-unreachable-waypoint predicate | REMOVE / REPLACE | Escape orbit around missed points | Robust off-path progress needed | Path planner/recovery | Vehicle curvature | Orbit regression if point following remains. |
| Forward curve-biased sphere cast | KEEP BUT REDESIGN | See leader after a bend | Yes | Hybrid perception | Lane occupancy + physics hazards | Rear-end collisions on curves. |
| Per-instance non-alloc buffers | KEEP principle | Bound GC/query cost | Yes | Traffic frame/perception | Density/LOD | Allocations or truncated hazards under load. |
| Bumper-to-bumper gap measurement | KEEP | Correct IDM spacing | Yes | Perception/speed planner | Vehicle extents | Vehicles overlap before reaching `s0`. |
| Walking-player detection | KEEP | Avoid pedestrian blindness | Yes | Hybrid perception/safety | Character representation | AI ignores on-foot players. |
| `IsDeliberateStop` leader-gap heuristic | REMOVE / REPLACE | Avoid teleporting a correctly stopped follower | The distinction remains vital | Blocker/progress constraints | Signals, grants, exit blockage | Legitimate waits trigger recovery, or real wedges never recover. |
| 60-second stuck timer | REMOVE / REPLACE | Last-resort wedge detection | Progress supervision remains needed | Recovery supervisor | Expected progress, blockers | Arbitrary slow response and false positives. |
| Player-clearance check before teleport | REMOVE / REPLACE | Hide visible recovery teleport | No; teleport must leave normal runtime | Recovery policy | Player visibility | Hidden discontinuities and interpenetration. |
| `RecoverAtWaypoint` teleport/reset | REMOVE / REPLACE | Recover rollover/void/stuck cheaply | Recovery remains needed | Recovery supervisor + planner | Physics stability, lane localization | Lost vehicles if removed before physical recovery exists. |
| Rollover/void predicates | KEEP BUT REDESIGN | Detect catastrophic invalid state | Yes | Safety/recovery diagnostics | Physics | Vehicles remain permanently invalid; must not imply automatic teleport. |
| Lane-change timer with no candidates | REMOVE / REPLACE | Prepare MOBIL cadence | Cadence yes, placeholder no | Tactical planner scheduler | Adjacent lanes | Dead code obscures actual capability. |
| `WaypointIndex` replicated state | REMOVE / REPLACE | Client route progress/debug | Progress remains needed | V2 debug/presentation state | Lane coordinate, route id | Clients/debug tools cannot explain traffic after V2. |
| Behavior text label | KEEP BUT REDESIGN | Accessible synchronized AI state | Yes | Debug frame presenter | Tactical/policy state | Traffic remains opaque; color-only debugging risk. |
| Source-text architecture tests | KEEP BUT REDESIGN | Guard hard boundaries cheaply | Some invariants yes | Test suite | Refactors, scenario tests | Brittle failures or false confidence. |
| Cinematic route replay | KEEP BUT REDESIGN | Reproduce perturbation/orbit cheaply | Yes as scenario seed/oracle | Behavior harness | Real physics parity | Regressions escape if replay is treated as physical proof. |

---

## 6. Artifact change proposals

### 6.1 `epics.md` — Epic 5 status and Story 5.14

**OLD:**

> Story 5.14 absorbs `ANO-5.10-03`; collision-response AC1–AC8 and physical return to a lane are delivered by it.

**NEW:**

> Story 5.14 delivered the shared-intent/physics path only. Significant collision response and physical lane recovery were explicitly excluded by the approved Story 5.14 spec and remain open. `ANO-5.10-03` is reassigned to the Traffic V2 recovery boundary.

**Rationale:** Align planning truth with the implemented code and frozen story spec.

### 6.2 `epics.md` — remaining Epic 5 backlog

**OLD:**

> Stories 5.17–5.23 add perception/unblocking, intersection rules/deadlock prevention, emotion modulation, litter, targeted rage, scale measurement and checkpoint directly after V1.

**NEW:**

> Freeze the current Epic 5 implementation surface as the accepted Traffic V1 and shared vehicle-physics reference. Remove Stories 5.17–5.23 from executable sprint status and preserve their text only as superseded requirements evidence. The V1 oracle is accepted; the V2 Road World Model/responsibility architecture package was accepted by the owner on 2026-09-22, so replacement stories may be planned from the accepted contracts. Move litter and targeted Rage behaviors behind the future V2 route/policy boundaries.

**Rationale:** Prevent the backlog from extending the monolithic controller and make each story converge on the approved responsibility model.

### 6.3 Architecture spine

Add adopted Traffic V2 decisions only after approval:

- Traffic V2 responsibility/dependency chain and one-intent ownership.
- Road World Model semantics and stable authored identity.
- Host traffic-frame coherence and deterministic decision ordering.
- Separate route/path/motion-plan contracts.
- Junction movement grants and blocked-exit precedence.
- Recovery supervision constraints.
- One normal/Rage driving stack with explicit rule exceptions.
- V1 freeze, parity gate and retirement rule.

Supersede, do not erase, the AD-34 clause that prohibits per-vehicle itineraries. Preserve its portal lifecycle rule.

### 6.4 Deferred work and anomalies

- Reassign `ANO-5.10-03` to V2 recovery; keep open.
- Replace the open “point pursuit” record with a V2 motion-planning requirement rather than claiming the present look-ahead fully closes the architectural debt.
- Treat Story 5.14 as complete; preserve its historical runtime notes only as context for future V2 scenarios.
- Record the external review's absent-symbol discrepancy so future analyses do not import another version's mechanisms as facts.

### 6.5 Tests

- Preserve current fixtures until their behavior is represented in the V2 scenario catalog.
- Do not delete V1 tests just because V2 code does not use the same classes.
- Mark source-shape guards as either enduring architecture guards or migration-only tests.
- Validate V2 parity through the future V2 scenario/integration suite; repairing the V1 PlayMode harness is not a prerequisite.

### 6.6 UX

No player-facing redesign is required by this course correction. Add a development-only, text-accessible traffic debug surface showing route/goal/blocker/grant/constraint/safety/recovery information.

---

## 7. Options evaluated

| Option | Effort | Risk | Verdict |
| --- | --- | --- | --- |
| Continue current stories and patch anomalies | Medium initially, unbounded later | High | Not viable. It places new responsibilities in the known convergence point. |
| Incrementally refactor V1 into the target | High | High | Not preferred. The accepted V1 behavioral reference would be obscured during structural surgery. |
| Redesign in place in one cut | High | Very high | Not viable. No executable oracle during migration. |
| Freeze V1, build V2 beside it, reuse kernels, migrate by parity | High | Medium–high but controlled | **Recommended.** Preserves behavior knowledge and creates clean boundaries. |
| Roll back vehicle physics/intent work | Medium | High, no benefit | Rejected. AD-35 and the shared physics path are sound reusable foundations. |
| Reduce MVP traffic to avoid V2 | Low | Product risk high | Possible only if the open-world/Rage target is explicitly deferred. Not the stated direction. |

Timeline impact is substantial: the remaining Epic 5 traffic work becomes a dedicated multi-story architecture program. Traffic-dependent portions of future epics move behind its parity gate. Independent on-foot/inventory work can continue in parallel if desired.

---

## 8. Implementation handoff

**Classification:** Major.

| Recipient | Responsibility |
| --- | --- |
| Product Manager / Product Owner | Keep Stories 5.17–5.23 permanently superseded and create no replacement stories until the Road World Model package gate is closed (owner acceptance was recorded on 2026-09-22); later decide epic insertion/renumbering and MVP capability gates. |
| Solution Architect | Present proposed AD-43 through AD-47 and the detailed Road World Model contract for owner acceptance/correction. Do not generate implementation stories in this run. |
| Developer | Preserve V1 code and tests as evidence. Do not implement Traffic V2 or continue Stories 5.17–5.23 during this phase. |
| Kenan | Accept or correct the Road World Model Fast-path assumptions before story generation, and later provide judgment for runtime feel scenarios that automation cannot validate. |

### Success criteria for the course correction

1. The Traffic V2 behavioral contract is explicitly approved before implementation stories are changed or started.
2. Story 5.14, `ANO-5.10-03`, sprint status and deferred work no longer contradict each other.
3. V1 is preserved as a reference, not maintained as a competing evolving product.
4. No V2 subsystem other than the intent composer can command the vehicle.
5. Historical anomaly knowledge is represented in the scenario catalog before V1 retirement.
6. The V2 checkpoint passes in `MVP_Run`, including credible host/client evidence and the measured traffic budget.

---

## 9. Correct Course checklist status

| Section | Status | Result |
| --- | --- | --- |
| 1. Trigger and context | `[x]` | Technical limitation and plan/implementation divergence established with code, history, tests and artifacts. |
| 2. Epic impact | `[x]` | Epic 5 remainder requires replacement; traffic-dependent future work is resequenced. |
| 3. Artifact impact | `[x]` | Epics, architecture, anomaly ownership, deferred work and tests affected; PRD `[N/A]` because FR/NFR live in `epics.md`; UX minimal. |
| 4. Path forward | `[x]` | Hybrid V1 freeze + V2 selected. Rollback and continued patching rejected. |
| 5. Proposal components | `[x]` | Issue, impacts, contract, classification, decomposition and handoff included. |
| 6. Final review | `[x]` | Approved by Kenan on 2026-09-21 with the five clarifications recorded above. Planning-state and architecture artifacts are authorized; Traffic V2 implementation is not. |
