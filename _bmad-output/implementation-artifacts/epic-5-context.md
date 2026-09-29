# Epic 5 Context: Vehicle Physics, NPC Response Foundation and Routed Traffic

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

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
- Story 5.17: Wider Perception and Progressive Unblocking
- Story 5.18: Intersection Rules and Deadlock Prevention
- Story 5.19: Rage and Fear as Driving Model Modulation
- Story 5.20: Thrown Litter Foundation and Attribution
- Story 5.21: Player-Targeted Rage Ladder and Rage Road Trigger
- Story 5.22: Scale Validation and Network Budget
- Story 5.23: Epic 5 AI Traffic Playable Checkpoint
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
- Stories 5.17–5.23 are superseded requirements evidence, not executable backlog. Traffic V2 begins at 5.24; 5.47 exists only if profiling triggers its performance requirement.
- `MVP_Run` is the acceptance map. Its nine junctions need reviewed road semantics and drivable clearances. Conventional junction turns require two independent, strictly positive conservative proofs: separation from physical obstacles and planar separation from declared `Sidewalk` surfaces, including disabled colliders. Declared surfaces must match visible sidewalks. Drivable carriageway relief is excluded from obstacles only when its complete projection is supported by road, avoids sidewalks, and fits below the vehicle's profile-derived body clearance. Each proof has its own results and input fingerprint; relevant changes invalidate Gate A.

## Technical Decisions

- Player and AI use one host-simulated raycast-wheel physics body. AI issues `VehicleDriveIntent`; it does not move the `Rigidbody` directly. Driver kernels are pure and parameterized; Rage/Fear modulation sits above physics.
- V2 uses a one-way imported, semantically validated Road World Model with stable opaque IDs and deterministic versioning. Directed 3D arc-length corridors, authored lateral order, width envelopes, explicit junction movements, one control binding per movement, conflicts, signals, and portals are road facts. V1's graph and NavMesh are not V2 route authorities.
- One immutable host `TrafficFrame` feeds observation, route, rules/coordination, tactical choice, local plans, a narrow `SafetyFilter`, one intent composer, and shared physics. Route, path, and timed motion are distinct. Versioned decisions expose stable debug reasons; grants become effective in the next frame.
- Traffic rules may admit explicit policy exceptions, but simulation invariants, compatible junction grants, finite controls, and single intent ownership remain mandatory. Collision and recovery supervisors request tactical work; they do not teleport, actuate, or despawn traffic. Fidelity scaling is conditional on measured cost.

## UX & Interaction Patterns

- Start Game and Create Lobby use one host session path. Escape opens a menu without pausing the host simulation, suppresses local driving/action input, and returns through the established session-exit path. Traffic state changes and Rage Road escalation need visible player feedback.

## Cross-Story Dependencies

- The V1 oracle bench precedes V2 migration and replacement. Road records, geometry, importer, roundabout widening, corrected movements, and corner clearance feed the resumed 5.28 overlay review. Gate A binds the model, both clearance proofs, refreshed roundabout evidence, and owner sign-off before routing and the driven V2 slice.
- Story 5.46 decides whether conditional 5.47 exists. Story 5.48 follows the V2 parity and integration gate.
