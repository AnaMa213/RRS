---
name: Traffic V1 Behavioral Oracle
type: regression-catalog
status: accepted-reference-baseline
created: 2026-09-21
code_baseline: 210f48811e3f99fbb93c5d2aa75885e65edcf878
scope: Road Rage Simulator Traffic V1 behavior and shared vehicle foundations
---

# Traffic V1 Behavioral Oracle

## Purpose

This catalog is the preservation boundary for Traffic V2. It records behavior to retain or consciously replace; it does not require V2 to preserve V1 classes, timers, node indices or algorithms.

The accepted code baseline is commit `210f48811e3f99fbb93c5d2aa75885e65edcf878`. By project-owner decision on 2026-09-22, the existing code, tests, artifacts and this catalog are sufficient as the Traffic V1 reference; no additional Story 5.14 recipe, PlayMode-harness repair, branch or tag is required before Road World Model design.

The classification authority is Section 5 of `../sprint-change-proposal-2026-09-21.md`. This document turns that classification into executable or observable scenarios.

## Evidence vocabulary

| State | Meaning |
| --- | --- |
| `AUTO-EDIT` | An existing EditMode test directly exercises the behavior or a pure kernel. |
| `AUTO-PLAY` | An existing PlayMode test exists; its last repository evidence is retained without creating a new V1 gate. |
| `SOURCE-GUARD` | A source-shape/reflection test guards an architecture property; useful, but not behavioral proof by itself. |
| `OWNER-ACCEPTED` | The project owner accepts the delivered V1 behavior without additional replay. |
| `MANUAL` | A historical human runtime recipe or observation exists or may inform later V2 validation. |
| `GAP` | The behavior is required by an anomaly or contract but has no adequate V1 proof; it remains a V2 regression requirement, not a blocker to V2 architecture. |
| `NEGATIVE` | Repository inspection establishes that the supposed mechanism does not exist in V1. |

The last repository record remains 645/645 EditMode green; PlayMode was order-sensitive and Story 5.14's runtime recipe had no reported result. Those facts remain traceability notes, but the project owner accepts Story 5.14 as complete and waives additional V1 runtime proof.

## Frozen V1 surface

The behavioral reference includes:

- `NetworkedAIVehicleDriverController`, `NetworkedAIVehicleState`, `LaneGraph`, `LaneNode`, `LaneGraphRouting` and `DriverModel`;
- `PortalTrafficSpawner`, `TrafficSettingsDef` and the synchronized session traffic settings path;
- `VehicleDriveIntent`, `VehiclePhysicsBody`, `VehicleProfileDef` and the player/AI shared physics path;
- the authored Traffic V1 data in `MVP_Run` and the `Greybox_AIVehicle` prefab;
  *2026-09-23 exception (Story 5.49): the roundabout prefab's physical geometry — roadway, island, sidewalks, colliders — is widened while its V1 traffic data stays unchanged. Oracle verdicts are re-run against a pre-change baseline; a behavioural delta is reported to the owner, not absorbed.*
- Story 5.2, 5.4, 5.7, 5.9, 5.10, 5.14, 5.15 and 5.16 traffic/vehicle tests;
- `ANO-5.10.02`, `ANO-5.10-03` and relevant deferred-work records.

Freeze means no new Traffic V1 capability or validation-harness work is required. Future tests are written against V2 contracts and may reuse V1 fixtures/data without reopening V1.

## Scenario catalog

### A. Authority, intent and physics

| ID | Contracted behavior | Current evidence | V2 parity criterion | Classification |
| --- | --- | --- | --- | --- |
| V1-A01 | Only the host advances authoritative AI decisions and replicated AI state. | `NetworkedAIVehicleDriverController.FixedUpdate`; `Story54...BehaviorIsDerivedHostOnly...`; `Story57...EveryReplicatedState...` | A client cannot advance route, policy or physics state; host/client presentation agrees. | KEEP |
| V1-A02 | One AI decision step submits one `VehicleDriveIntent`; normal driving does not write Rigidbody position, rotation or velocity. | `Story514.TheDrivingStepWritesNoVelocityPositionOrRotation`; `TheIntentIsSubmittedToTheSamePhysicsLayer...` (`SOURCE-GUARD`) | V2 has one composer and no upstream actuator/Rigidbody writes. | KEEP |
| V1-A03 | Player and AI vehicles use the same `VehiclePhysicsBody` and physical profile contract. | `Story514.TheAiPrefabSharesTheSamePhysicsLayerAndProfile...`; Story 5.11–5.13 kernel tests | Both control sources produce intent for the same physics implementation. | KEEP |
| V1-A04 | Missing driver or physics profiles fail inert with a diagnostic, never with hard-coded driving fallback. | `Story59.AMissingProfileLeavesTheVehicleInert...`; controller branches | V2 invalid composition is inert/diagnosed and cannot silently invent tuning. | KEEP |
| V1-A05 | A pushed AI can inherit physical displacement instead of being snapped back by the nominal drive step. | Story 5.14 accepted complete by the project owner (`OWNER-ACCEPTED`); no additional replay required | Push changes pose/velocity naturally; no one-frame realignment; later recovery is physical. | KEEP BUT REDESIGN |
| V1-A06 | Vehicle collisions remain finite, bounded and damage remains host-only with existing occupant filters. | Story 5.15 EditMode guards; three PlayMode physics tests (`AUTO-PLAY`) | Equivalent collision bench passes with V2 intent and shared physics. | KEEP |

### B. Longitudinal driving and perception

| ID | Contracted behavior | Current evidence | V2 parity criterion | Classification |
| --- | --- | --- | --- | --- |
| V1-B01 | Free-road acceleration is finite and fades to zero at desired speed. | `Story59.FreeRoadAccelerates...`, `AtDesiredSpeed...` (`AUTO-EDIT`) | Speed planner preserves the tested IDM kernel envelope. | KEEP |
| V1-B02 | A stopped leader causes finite firm braking, including a zero/collapsed gap. | `StoppedLeaderProducesFirmFiniteBraking`, `ZeroGapProducesBoundedFiniteDeceleration` | No NaN/inf; collision avoidance constraint dominates desired speed. | KEEP |
| V1-B03 | Following settles near the authored minimum gap measured bumper-to-bumper. | `TheIdmSettlesAtTheAuthoredMinimumGap...`, `LeaderDetectionMeasuresTheGapFromTheBumper...` | Leader observation reports physical clearance and the speed plan respects the authored gap. | KEEP |
| V1-B04 | Leader sensing covers the steered heading/curve and sees walking players, not only rigidbodies. | `LeaderDetectionSweepsAVolume...`, `LeaderDetectionSeesWalkingPlayers...` | Hybrid perception finds structured lane leaders and unstructured players/obstacles on the path horizon. | KEEP BUT REDESIGN |
| V1-B05 | Perception queries are allocation-bounded and report buffer saturation. | `PhysicsQueryBuffersAreMargined...` | V2 indexes/queries have explicit capacity/overflow diagnostics and measured cost. | KEEP principle |
| V1-B06 | Acceleration response smoothing is finite, bounded and deterministic for the same inputs. | `SmoothedAcceleration...`, `ANearInstantReactionTime...`, `AZeroOrNegativeReactionTime...` | Speed plan/controller response passes the kernel cases or an explicitly approved successor contract. | KEEP |

### C. Driver personality and emotional state

| ID | Contracted behavior | Current evidence | V2 parity criterion | Classification |
| --- | --- | --- | --- | --- |
| V1-C01 | Authored driver profiles own driving parameters; the controller owns no fallback drive constants. | `Story59.TheDefaultDriverProfileAsset...`, `TheControllerHoldsNoDriveConstants...` | DrivingPolicy resolves authored parameters/costs without controller literals. | KEEP |
| V1-C02 | Desired-speed variation is deterministic and bounded by the consistency envelope. | `APerfectlyConsistentDriver...`, `AnErraticDriver...`, `TheNoiseIsDeterministic...` | Same seed/input yields same policy variation; bounds remain authored. | KEEP |
| V1-C03 | Rage disposition changes effective driver parameters without mutating the base personality. | `ImmobilizingDispositions...`, `DispositionModulationLeavesThePersonalityParametersUntouched` | Rage/Fear policy produces effective parameters, costs and allowed exceptions without altering definitions. | KEEP BUT REDESIGN |
| V1-C04 | Each vehicle derives behavior only from its own rage state; missing rage falls back to calm. | Story 5.4 tests (`AUTO-EDIT`) | Emotional state remains per vehicle and host-owned; absence has one explicit safe policy. | KEEP |
| V1-C05 | Immobilizing dispositions stop route pursuit and do not trigger stuck recovery. | controller desired-speed branch; Story 5.4/5.9 tests | An intentional tactical stop creates a legitimate blocker/goal, not a recovery trigger. | KEEP BUT REDESIGN |

### D. Road topology, routing and lifecycle

| ID | Contracted behavior | Current evidence | V2 parity criterion | Classification |
| --- | --- | --- | --- | --- |
| V1-D01 | Normal traffic enters and leaves only at authored portals. | `TrafficOnlyEntersAtEntryPortalsAndOnlyLeavesAtExitPortals` (`AUTO-PLAY`); `NothingRemoves...ButAnExitPortal` (`SOURCE-GUARD`) | No normal mid-road spawn, despawn or reinsertion; V2 portals map to migrated geometry. | KEEP |
| V1-D02 | A crowded entry defers insertion rather than silently dropping demand. | `ACrowdedPortalQueuesTheInsertion...` | Population demand remains pending until a safe portal insertion exists. | KEEP |
| V1-D03 | A reusable entry/exit portal does not despawn a vehicle at birth but may accept it after circulation. | `AnEntryPortalMarkedReusable...`, `AVehicleSpawnedOnAReusableExitPortal...` | Portal lifecycle distinguishes insertion from later exit crossing. | KEEP |
| V1-D04 | Authored turn weights produce deterministic varied routes and have a diagnosed fallback for invalid weights. | weighted-draw and two-vehicle tests (`AUTO-EDIT`) | Route policy preserves authored ratios and stable seeded decisions; invalid data fails validation or a declared safe fallback. | KEEP BUT REDESIGN |
| V1-D05 | Vehicles do not wander forever when a destination/exit cannot be reached normally. | edge-budget/dead-end tests; greedy redirect implementation | Route planner detects no-route/budget failure and produces a deterministic replan/failure result. | KEEP BUT REDESIGN |
| V1-D06 | A graph with no exit reports failure and does not delete traffic. | `AGraphWithoutAnyExitPortalReportsNoReachableExitInsteadOfRemovingAnything` | Invalid world data is rejected before play; runtime failure never becomes arbitrary removal. | KEEP |
| V1-D07 | Modular connectors join only within distance and travel-direction constraints; orphans are reported. | connector tests (`AUTO-EDIT`) | Import/validation preserves only compatible seams and reports unmapped geometry. | KEEP BUT REDESIGN |
| V1-D08 | The current point-node representation, hierarchy-order IDs and runtime proximity joins are not V2 contracts. | `LaneGraph` implementation and proposal classification | V2 runtime uses stable semantic IDs and validated road semantics; importer may consume V1 geometry. | REMOVE / REPLACE |
| V1-D09 | Traffic population comes from synchronized session values within authored bounds. | Story 5.16 tests (`AUTO-EDIT`) | Host-selected count/litterer count follows the existing `MatchSettings` path and is deterministic. | KEEP |

### E. Path following and perturbation

| ID | Contracted behavior | Current evidence | V2 parity criterion | Classification |
| --- | --- | --- | --- | --- |
| V1-E01 | Nominal traffic progresses through the authored district without entering the curb footprint. | `NominalAiTrafficCrossesTheCurbedCrossroads...` cinematic replay (`AUTO-EDIT`, not physical proof) | A physical V2 scenario crosses equivalent migrated geometry within lane/corridor tolerance. | KEEP BUT REDESIGN |
| V1-E02 | A perturbed vehicle does not orbit a missed waypoint forever and eventually reaches an exit. | `PassedWaypoint...` tests and `PerturbedVehiclesAlwaysReachAnExitPortal...` | Off-path localization and replanning restore progress without point-specific orbit hacks. | KEEP BUT REDESIGN |
| V1-E03 | Steering target remains continuous across node changes. | `TheAimPointStaysContinuousAcrossNodeChanges...` | Path/trajectory samples are continuous within declared curvature/jerk tolerances. | KEEP BUT REDESIGN |
| V1-E04 | Self-avoidance prevents the authored ring replay from cycling forever. | ring-walk tests | Strategic routing handles legal cycles and still guarantees progress or an explicit no-route outcome. | REMOVE / REPLACE mechanism; keep outcome |
| V1-E05 | AI drives along its route under the authored driver profile. | Story 5.9 PlayMode test (`AUTO-PLAY`) | V2 nominal driving moves under authored policy and shared physics in `MVP_Run`. | KEEP |

### F. Waiting, faults and recovery

| ID | Contracted behavior | Current evidence | V2 parity criterion | Classification |
| --- | --- | --- | --- | --- |
| V1-F01 | Correctly stopping behind a leader is not classified as a blockage. | `StoppingBehindALeaderIsADeliberateStop...`, `NoLeaderMeans...`, `ACollapsedGapIsAWedge...` | Recovery uses expected progress plus blocker legitimacy, not low speed alone. | KEEP BUT REDESIGN |
| V1-F02 | V1 rollover/void/stuck recovery resets velocity and teleports to the current waypoint without advancing route state. | controller `RecoverAtWaypoint`; Story 5.2/5.14 source guards | V2 normal recovery must not copy this mechanism; catastrophic cleanup is separately declared and observable. | REMOVE / REPLACE |
| V1-F03 | V1 avoids a stuck teleport while a player is nearby or a leader wait is deliberate. | `TheControllerNeverTeleportsAVehicleThatIsDeliberatelyStoppedOrWatchedByAPlayer` | Superseded by no normal-runtime teleport plus legitimate-wait suppression. | REMOVE / REPLACE |
| V1-F04 | A significant collision temporarily interrupts nominal driving until physical instability resolves. | `ANO-5.10-03` AC1/AC4 (`GAP`) | Dedicated collision-response contract passes impact, sustained-contact, spin and displaced-path scenarios. | KEEP BUT REDESIGN |
| V1-F05 | After a collision, the vehicle can physically rejoin a valid lane and resume a valid route without teleport/despawn. | `ANO-5.10-03` AC5–AC8 (`GAP`) | Recovery scenario passes in `MVP_Run`, including temporary off-road displacement. | KEEP BUT REDESIGN |
| V1-F06 | Collision reactions may vary by authored driver policy while simulation invariants remain enforced. | `ANO-5.10-03` AC3 (`GAP`) | Deterministic seeded reaction selection supports brake/evasion/loss-of-control policies without bypassing invariant vetoes. | KEEP BUT REDESIGN |

### G. Networking, presentation and scale

| ID | Contracted behavior | Current evidence | V2 parity criterion | Classification |
| --- | --- | --- | --- | --- |
| V1-G01 | Client presentation consumes replicated state without a second synchronization path. | Story 5.7 EditMode tests; one PlayMode host visibility test | V2 debug/presentation state is host-produced and read-only on clients. | KEEP |
| V1-G02 | AI behavior has a text-accessible debug representation. | `BehaviorDebugViewRendersTextOnly...` | V2 exposes text route/goal/blocker/grant/constraint/safety/recovery reasons. | KEEP BUT REDESIGN |
| V1-G03 | Roughly 30 vehicle bodies retain the chosen collision mode without tunnelling in the existing collision bench. | Story 5.15 PlayMode scale test (`AUTO-PLAY`) | Preserve as a shared-physics benchmark; do not infer full Traffic V2 scalability from it. | KEEP benchmark |
| V1-G04 | Near/mid/far traffic LOD behavior is not implemented in V1. | Repository inspection (`NEGATIVE`) | Architecture preserves identity/route invariants; implementation waits for functional V2 profiling unless measured earlier. | NEW V2 capability |

## Explicit negative oracle

The following names/mechanisms are absent from the V1 codebase and must not be treated as historical behavior: `TrafficQueue`, `heldByBoundedWait`, `queueElapsedSeconds`, `WaitsFor`, `ResolveWinnerIndex`, `TryEscalateRecovery`, `JunctionManager`, `TrafficLightController`, `WorldSnapshot`, `MotionPlanner`, `SpeedPlanner`, `SafetySupervisor` and `RecoverySupervisor`.

V1 also has no implemented intersection right-of-way arbitration, signal control, blocked-exit grant, lane-change maneuver, overtaking, strategic multi-lane itinerary, or four-stage recovery ladder. Those are requirements to design, not behaviors to preserve.

## Regression suites carried into V2 delivery and parity

These suites are preservation input for future V2 stories and tests. Extracting or executing them against V1 is not an additional prerequisite; the Road World Model architecture package was accepted by the owner on 2026-09-22 (AD-43 through AD-47 adopted).

1. **Pure kernel suite:** IDM, response smoothing, deterministic noise, MOBIL safety/gain, weighted routing and topology validation.
2. **Recorded road scenarios:** nominal crossroad/roundabout travel, perturbed missed-node recovery, reusable portals, crowded insertion and no-exit failure.
3. **Physical interaction suite:** leader following, walking player ahead, player push, offset collision, wall impact, curb/bump, rollover, wrong-way/head-on and static obstacle.
4. **Junction requirement suite:** stop, yield, priority road/right, signal phases, conflicting movements, blocked exit, roundabout entry and deterministic simultaneous arrival. These begin as contract tests because V1 has no implementation.
5. **Recovery suite:** legitimate waits, wedge without blocker, post-collision stabilization, off-road lane rejoin and no normal teleport/despawn.
6. **Network suite:** host-only decisions, client read-only presentation, stable seeded outcomes and two-player `MVP_Run` smoke.

## Accepted baseline decision

- Commit `210f48811e3f99fbb93c5d2aa75885e65edcf878` plus this catalog is the accepted V1 reference.
- Story 5.14 is complete; its manual recipe and additional PlayMode execution are not prerequisites for Traffic V2 architecture.
- A legacy branch or tag may still be created as repository hygiene, but it is optional and carries no planning gate.
- `MANUAL` and `GAP` rows preserve knowledge for future V2 regression design; they do not reopen V1 or block Road World Model work.
- V1 receives no further capability work. Any later correction requires a new explicit course decision.

## Retirement gate

V1 can be retired only when every catalog row is one of:

- passed by V2 with retained evidence;
- consciously replaced with an approved successor contract and passing scenario;
- explicitly declared obsolete with the regression risk recorded.

Passing unit kernels alone is insufficient. The parity decision requires `MVP_Run` integration, host/client evidence, and manual feel review for physical behaviors that cannot yet be asserted reliably.
