# Epic 5 Context: Vehicle Physics, NPC Response Foundation and Routed Traffic

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

<!-- Mise a jour 2026-09-28/29, livree avec la 5.29 sans figurer dans ses artefacts : les faits ci-dessous
     sont repris des sources normatives (planning-artifacts/epics.md, AD-42/AD-48 du contrat du modele
     routier, clarification Gate A du 2026-09-28 dans ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md).
     Les faits normatifs retires par cette passe ont ete restaures et les deux formulations de 5.47
     harmonisees sur epics.md (5.46 decide, 5.47 conditionnel non specifie). -->

<!-- Recompile 2026-09-30 (ouverture de la 5.52) : integre sprint-change-proposal-2026-09-29.md
     (insertion 5.52, Gate B deplacee), sprint-change-proposal-2026-09-29-kinematic-pose.md (pose
     nominale cinematique, objectif de mouvement intermediaire, regle de contact) et
     sprint-change-proposal-2026-09-30.md (deux plafonds de braquage, limite de courbe des la 5.31). -->

<!-- Recompile 2026-10-01 (ouverture de la 5.32) : integre sprint-change-proposal-2026-09-30-geometry-5-52.md
     (correction geometrique locale des trottoirs et des anneaux, aucune exigence nouvelle). -->

<!-- Recompile 2026-10-03 (ouverture de la 5.34) : integre l'amendement 5.33 d'epics.md du 2026-10-02
     (decisions proprietaires D1-D15 de la spec 5.33 : frame partagee, collecteur de dangers, limite de route
     appliquee, maintien a l'arret, jeton de scenario, regle D12 pre-5.34). Mise a jour ciblee, pas une
     recompilation complete, pour conserver les faits normatifs restaures a la main. -->

## Goal

Establish host-authoritative vehicle physics and configurable NPC Rage/Fear responses, then build credible routed city traffic on the shared driving stack. The greybox district in `MVP_Run` is the integration and acceptance ground, not the finished Level 1 city or a new run/checkpoint contract. Traffic V1 supplies the behavioral baseline while Traffic V2 is built and validated before V1 retirement.

## Stories

- Story 5.1: Configurable NPC Rage/Fear Foundation
- Story 5.2: Basic AI Route Following and Recovery
- Story 5.3: Unified Solo and Online Session Start
- Story 5.4: Rage-Driven AI Behavior States
- Story 5.5: Networked AI Rage Targeting
- Story 5.6: Rage Road Event Trigger
- Story 5.7: AI Traffic Networking and Client Presentation
- Story 5.8: Escape Menu and Return to Main Menu
- Story 5.9: Parameterized Driver Model
- Story 5.10: Lane Graph, Greybox District, and Routed Source/Sink Traffic
- Story 5.11: Vehicle Chassis, Wheels, and Suspension
- Story 5.12: Tire Forces and Steering
- Story 5.13: Arcade Assists and Uneven Ground
- Story 5.14: AI Drives by Intent
- Story 5.15: Credible Collisions and Damage Integration
- Story 5.16: Lobby-Configurable Traffic Settings
- Story 5.17: Wider Perception and Progressive Unblocking (historical)
- Story 5.18: Intersection Rules and Deadlock Prevention (historical)
- Story 5.19: Rage and Fear as Driving Model Modulation (historical)
- Story 5.20: Thrown Litter Foundation and Attribution (historical)
- Story 5.21: Player-Targeted Rage Ladder and Rage Road Trigger (historical)
- Story 5.22: Scale Validation and Network Budget (historical)
- Story 5.23: Epic 5 AI Traffic Playable Checkpoint (historical)
- Story 5.24: V2 Regression Oracle Bench and V1 Trace Contract
- Story 5.25: Road World Model Records, Compiler and Deterministic Versioning
- Story 5.26: Directed Arc-Length Geometry and Lane Localization
- Story 5.27: V1 Importer, Semantic Validator and Measured Migration Report
- Story 5.28: Junction Semantic Authoring and `MVP_Run` Overlay Sign-Off
- Story 5.49: Roundabout Ring Widening and Applied Reviewed Widths
- Story 5.50: V2 Movement Geometry Correction and Model-Level Drivability Validation
- Story 5.51: Junction Corner Clearance for Right Turns
- Story 5.29: Deterministic Strategic `RoutePlan`
- Story 5.30: Traffic V2 Planning and Runtime Spine Foundation
- Story 5.31: First Driven Traffic V2 Vertical Slice — Portal to Portal
- Story 5.52: Tracking-Error Coverage and Kinematic Nominal Pose in Gate A Evidence, and Owner Re-Signature
- Story 5.32: Perception Boundary — Occupancy Index, Leader and Hazard Observations
- Story 5.33: Vehicle Following Through Named Speed Constraints
- Story 5.34: Junction Coordination Core — Grants, Conflicts, Blocked Exit, Committed Traversal
- Story 5.35: Authored Control Kinds — Stop, Yield, Priority and Roundabout Entry
- Story 5.36: Signal Phase Runtime and Coordinator Integration
- Story 5.37: `SafetyFilter` — Narrow Veto and Clamp Boundary
- Story 5.38: Collision Response — Physics Yields, Tactical Handshake
- Story 5.39: Recovery Supervisor — Expected-vs-Actual Progress and Physical Reattachment
- Story 5.40: Gridlock Detection and Bounded Escalation
- Story 5.41: `DrivingPolicy` Boundary and TrafficRule Exception Protocol
- Story 5.42: Unstructured Maneuvers — Obstacle Bypass, Overtaking, Wrong-Way and Head-On
- Story 5.43: Rage and Fear as Continuous Policy Modulation
- Story 5.44: Targeted Aggression and `AuthorizedContact`
- Story 5.45: Thrown Litter on V2 Contracts
- Story 5.46: Profiling Baseline and ~30-Vehicle Budget Measurement
- Story 5.47: Conditional Minimum Traffic Fidelity-Scaling Response
- Story 5.48: V1/V2 Parity Gate and V1 Runtime Retirement

## Requirements & Constraints

- The host alone simulates AI, car bodies, collisions, target state, and traffic decisions; clients receive replicated presentation. Traffic population and litter-thrower limits are lobby settings bounded by authored definitions. Approximately 30 simultaneous AI vehicles is a measurement target, not an assumed production capacity.
- AI vehicles enter and leave visibly only through authored portals. Normal congestion, blockage, collision, or route failure must not silently remove an agent. Rage and Fear affect authored driver parameters and policy within one driving stack; intentional contact needs target-scoped authorization.
- Traffic V1 remains a frozen oracle. V2 must account for each oracle scenario and pass host/client integration in `MVP_Run` before V1 retirement. Physical-only changes to the district keep V1 authored traffic data and lineage intact, but require a before/after V1 behavioral regression; an unchanged hash alone proves no behavioral equivalence.
- Stories 5.17–5.23 are superseded requirements evidence, not executable backlog. Traffic V2 begins at 5.24; conditional 5.47 exists only if Story 5.46 records AD-42's performance trigger met (if the budget is satisfied, 5.46 leads straight to 5.48).
- `MVP_Run` is the acceptance map. Its nine junctions need reviewed road semantics and drivable clearances. For the five conventional junctions, the inflated compiled-reference footprint must pass two independent, strictly positive conservative gates: separation from obstacle volumes within vehicle height, and planar separation from authored `Sidewalk` surfaces regardless of collider height or activation. The semantic proof includes the disabled north T-junction corners and checks that declarations match visible sidewalks. Drivable carriageway relief is excluded from the obstacle gate only when its complete projection is supported by road, avoids sidewalks, and fits below the vehicle's profile-derived body clearance. Each gate publishes its own per-corner/per-movement results and canonical input fingerprint. Gate A binds both proofs and refreshed roundabout evidence; any relevant input change invalidates the proof.
- Gate A evidence covers a physical vehicle only if the tracking allocation a_e = max |o(s)| + ε_t actually enters every proof's inflation, beside the reserved margin and δ_c; an allowance recorded only as text covers nothing. ε_t bounds the road-plane displacement of the eight corners of the maximum-gauge box against the kinematic nominal pose: exact at every physics step, between steps only under the per-step-verified integration model M, never a continuous physical guarantee. The reserved margin and residual clearances are never converted into allowance, and no threshold, margin, remainder or geometry is changed to obtain a favorable verdict; a failing proof is escalated to the owner through a separate course correction.
- Evidence computed with the tangent-aligned gauge pose (everything signed before 2026-09-29) is superseded history and never closes Gate B. V2 vehicles drive outside an explicit measurement run only on trajectories covered by valid kinematic-pose evidence including ε_t. Gate A is re-signed only by the owner, on a new record; previous records stay as superseded history. Gate B closes after that re-signature and a PlayMode milestone 1 rerun, within the stated per-step/model-M proof scope.
- When an inflated envelope changes, no prior conflict-pair decision is reused without re-evaluation; the approved `5.50-AUTO-DECISIONS-v1` mechanism applies to Story 5.52 under all its conditions but never authorizes review or Gate A signature.
- The kinematic regeneration was made positive by local geometry only (sidewalk corners, curbs, the T-junction north strip and twelve adjoining corridor sidewalks moved outward in `MVP_Run`; twelve authored ring-section half widths widened to 4.30 m), with ε_t, the reserved margin, δ_c, remainders, thresholds and proof logic unchanged. Gate A re-signature and the Gate B milestone are recorded by 5.52; later stories consume that proven road model and never edit it to fit their own needs.

## Technical Decisions

- Player and AI use one host-simulated raycast-wheel physics body. AI issues `VehicleDriveIntent`; it does not move the `Rigidbody` directly. Driver kernels are pure and parameterized; Rage/Fear modulation sits above physics.
- V2 uses a one-way imported, semantically validated Road World Model with stable opaque IDs and deterministic versioning. Directed 3D arc-length corridors, authored lateral order, width envelopes, explicit junction movements, one control binding per movement, conflicts, signals, and portals are road facts. V1's graph is never a second V2 road authority; NavMesh is not its route authority.
- One immutable host `TrafficFrame` feeds observation, route, rules/coordination, tactical choice, local plans, a narrow `SafetyFilter`, one intent composer, and shared physics. Route, path, and timed motion are distinct. Versioned decisions expose stable debug reasons; grants become effective in the next frame.
- Traffic rules may admit explicit policy exceptions, but simulation invariants, compatible junction grants, finite controls, and single intent ownership remain mandatory. Collision and recovery supervisors request tactical work; they do not teleport, actuate, or despawn traffic. Fidelity scaling is conditional on measured cost.
- The nominal pose of the vehicle gauge is the kinematic pose of the declared drivability geometry (reference point a ahead of a non-steered rear axle, wheelbase L), not the tangent-aligned pose: the body lags the tangent by e with de/ds = κ − sin(e)/a, e = 0 only at insertion at the entry portal's effective progress, and e jumps by the seam tangent jump. The pose model is versioned and recorded by every Gate A evidence. Proof-side, per-element entry offset intervals over the transition graph are accepted only by an inductiveness check (else `HeadingOffsetBoundNotClosed`); proofs cover the union over offsets, adding declared offset-grid and between-sample remainders; lock and steer-rate feasibility is checked on the whole pose set (else `NominalPoseInfeasible`).
- Two steering ceilings stay distinct: the model-published steady-state ceiling v*_ss(κ) serves admission and evidence review only; the driving ceiling v*(s) is the route's nominal-pose ceiling. The grip-based curve limit √(a_lat/|κ|) is applied from Story 5.31; since Story 5.33 the road limit is applied as a named cap (`Applied(v)`, or `Unauthored` for 0 — never an applied limit; `MVP_Run` stays unauthored). No Gate A proof uses either ceiling.
- Since Story 5.33, one host `TrafficFrame` per physics step is shared by every V2 vehicle (`TrafficV2StepRunner`: exit removal, then frame and one step per vehicle in `TrafficId` order, then insertion), with declared body footprints and a bounded host hazard collector. Longitudinal arbitration is acceleration-level over named candidates (profile, desired speed, leader following, obstacle, perception unavailable, steering ceiling), tightening immediately and smoothing only recovery; a near-stopped vehicle behind a cause enters a hysteretic `StopHold`. Waiting is a published blocker set with one dominant. Production keeps one V2 vehicle; only a test-only scenario token raises the population, up to 8.
- Invalid or missing plans produce the V2 fallback command (service brake above v_dir + 2·b·Δt, handbrake hold at or below it), not `VehicleDriveIntent.Idle`, whose V1 meaning is unchanged. The Route Planner's optional intermediate movement objective is set only by the measurement lifecycle path. During driven campaigns, body contacts with a recognized drivable relief of the valid 5.51 evidence or with the carriageway are published observations under consequence criteria; any other body contact fails acceptance.

## UX & Interaction Patterns

- Start Game and online play share the session bootstrap. Character selection happens before lobby entry; the lobby shows that identity read-only, and the spawned player reflects it. Escape opens a menu without pausing the host simulation, suppresses local driving/action input, and returns through the established session-exit path. Traffic state changes and Rage Road escalation need visible player feedback.

## Cross-Story Dependencies

- The V1 oracle bench precedes V2 migration and replacement. Road records, geometry, importer, dual clearance proofs, junction semantics, roundabout widening, corrected movements and corner clearance feed the resumed 5.28 overlay review. Gate A binds the model, both clearance proofs, refreshed roundabout evidence, and owner sign-off before routing and the driven V2 slice. Story 5.51 follows final movement geometry from 5.50; its first corner must pass physical shape, semantic region, visible rendering, save/reload, and scene-only diff together before the other cuts. It unlocks the resumed 5.28 overlay review but does not sign Gate A.
- Story 5.31 delivers the driven slice under an explicit measurement run, the campaigns, the owner-declared ε_t and the refusal outside measurement; it never regenerates or signs Gate A evidence. Story 5.52 then regenerates all four Gate A proofs (`ConflictSweep`, `JunctionClearance` physical and `Sidewalk`, `RoundaboutClearance`) on the kinematic pose set with a_e, re-evaluates changed pairs, moves the signed roundabout seam list into signed data, and prepares the owner re-signature; Gate B follows. A new ε_t reruns 5.52. Every later story builds on that proven spine without re-proving it; 5.33 keeps the road limit, following and obstacles.
- Story 5.33's exploratory campaign recorded an unarbitrated crossing in the central crossroads (two movements of one conflict zone, contact, then mutual `StopHold` at negative gap): its pre-5.34 exemption rule D12 is to be withdrawn by the junction coordination story, and the never-released contact hold belongs to recovery (5.39). Gate C is closed by 5.35, not by 5.33 or 5.34.
- Story 5.46 decides whether conditional 5.47 exists. Story 5.48 follows the V2 parity and integration gate.
