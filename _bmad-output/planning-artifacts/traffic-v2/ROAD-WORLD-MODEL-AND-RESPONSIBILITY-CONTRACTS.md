---
name: Traffic V2 Road World Model and Responsibility Contracts
type: architecture-discussion
status: architecture-gate-accepted
created: 2026-09-21
scope: Traffic V2 pre-implementation design
depends_on:
  - ../sprint-change-proposal-2026-09-21.md
  - V1-BEHAVIORAL-ORACLE.md
---

# Traffic V2 Road World Model and Responsibility Contracts

## Decision status

This document fixes responsibility, dependency, behavioral and logical Road World Model data contracts. It deliberately does **not** freeze C# class names, Unity asset nesting, editor UX, planning algorithms, story decomposition or Traffic LOD implementation.

The Road World Model architecture package was **accepted by the owner on 2026-09-22** with two recorded clarifications, both incorporated above and in the spine: re-import identity stability plus version canonicalization (AD-44), and control-binding semantics (AD-46). The V1 oracle baseline is accepted and requires no additional runtime-evidence gate. Implementation stories may now be planned; none are generated or implemented by this architecture run, and acceptance records no importer, compiled model or V2 asset.

## Chosen paradigm

**Snapshot-driven, single-intent traffic planning.**

```text
Authored road sources
        |
        v
Road authoring/import + validation
        |
        v
ROAD WORLD MODEL (immutable semantic topology/geometry)
        |
        +--------------------+
        |                    |
        v                    v
TRAFFIC FRAME          ROUTE PLANNING
(one host tick)              |
        |                    |
        v                    |
AGENT OBSERVATION            |
        |                    |
        +--------+-----------+
                 v
       RULE EVALUATION / JUNCTION COORDINATION
                 |
                 v
          TACTICAL DECISION
                 |
                 v
        PATH + MOTION/SPEED PLAN
                 |
                 v
            SAFETY FILTER
                 |
                 v
        VEHICLE DRIVE INTENT COMPOSER
                 |
                 v
          SHARED VEHICLE PHYSICS

Recovery Supervisor observes progress and requests replanning or a tactical
recovery goal. It never grants priority, edits rules, writes intent or moves a body.
```

Information flows downward. A lower layer may reject invalid input and report a reason; it must not reach upward and mutate another layer's state to make its own plan succeed.

## Road World Model capability contract

The Road World Model is the authoritative Traffic V2 runtime source for **static road meaning**. It must represent, directly or through owned related records:

- stable identity for road sections, directed lanes/corridors, junctions, lane-to-lane movements, signals and portals;
- continuous center/reference geometry plus a drivable corridor or width model;
- legal travel direction, speed limits and permitted vehicle classes/surfaces;
- predecessor/successor connectivity and legal adjacency/lane-change regions;
- authored junction movements with path geometry, entry/exit relation, stop line and conflict relation;
- stop, yield, priority, roundabout and signal applicability as movement semantics;
- source/sink portals and reachability to valid exits;
- surface semantics needed by vehicle physics/planning when a second drivable surface is introduced;
- validation data sufficient to reject missing IDs, broken topology, impossible connectors, invalid movement geometry, signal gaps and unreachable portal paths.

These capabilities do **not** imply that every field belongs to one `Lane` object. The logical ownership split is fixed below; concrete serialization nesting remains an implementation choice.

## Road World Model logical data ownership

The runtime consumes one validated, immutable compiled model. The names below are contract vocabulary, not mandatory C# class names.

| Record | Owns | References | Must not own |
| --- | --- | --- | --- |
| `RoadModel` | model identity, compiler schema version, deterministic content version, coordinate frame and validation profile | every static record through compiler-built indexes | authored reciprocal membership lists, live occupancy, signal phase or vehicle state |
| `RoadSection` | road-level grouping and defaults: road class, surface, speed limit and allowed vehicle classes | nothing; corridors reference the section | any corridor collection, authored or reciprocal; the cross-section frame, which its datum corridor supplies; lane geometry, connectivity or junction permissions |
| `LaneCorridor` | one directed 3D reference curve, arc-length domain, left/right width profiles, local road frame, lane-specific overrides, its own `LateralOrder` within the parent section's cross-section and whether it is that section's ordering datum | parent section and optional source trace | driver-relative lane side, which `LaneAdjacency` owns; successor lists, neighboring-lane permissions or mutable occupancy |
| `LaneConnection` | explicit longitudinal continuation, merge or split between corridor endpoints | from/to corridor IDs | junction right-of-way or lateral lane changes |
| `LaneAdjacency` | left/right relation, overlapping source/target arc-length intervals and lane-change legality/direction | two same-direction corridors | opposite-direction proximity or inferred lane-change permission |
| `Junction` | junction identity, boundary and feature classification | no authoritative child list; child foreign keys are compiled into indexes | approach control, conflict membership, vehicle grants, current phase or route choice |
| `JunctionMovement` | legal from-corridor to-corridor traversal, directed movement curve and route preference | parent junction and approach/departure corridors | control lines, conflict lists, signal groups, current permission or tactical intent |
| `JunctionControl` | `Uncontrolled`/`Priority`/`Yield`/`Stop`/`Signalized`, the movements it binds as their sole control owner, and optional stop/yield line geometry | parent junction and controlled movements | signal-group membership, current grant or phase |
| `ConflictZone` | reviewed 3D conflict volume/polygon and the movements or circulating-corridor occupancies that conflict there | parent junction and member IDs | reciprocal conflict lists on movements or arbitration state |
| `SignalPlan` | movement groups, ordered phases, allowed states and phase timing; each group alone owns its controlled movement/control membership | parent junction | reciprocal group IDs on movements/controls or current phase clock; that is host runtime state |
| `Portal` | entry/exit role, corridor ID, arc-length placement, direction and insertion/removal envelope | one corridor | population demand or spawned agents |
| `ImportManifest` / `SourceTrace` | many-to-many typed lineage among semantic IDs, persistent importer slots and source scene/prefab/object/node/edge keys | semantic records and source artifacts | runtime identity or behavior |

Road-section defaults are resolved during compilation so runtime planning reads a complete effective corridor/movement view. A default never changes ownership: a section speed limit may supply a corridor value, but the corridor remains the geometric and directed travel unit.

Persisted foreign keys on child records are the sole parent/child truth. Inverse collections such as "movements in junction" and all hot indexes are compiler outputs and cannot be authored. `ConflictZone` is the sole conflict-membership owner, `JunctionControl` the sole approach-control/line owner, and `SignalPlan` groups the sole signal-membership owner.

### Lateral cross-section order (AD-48)

Raised by the Story 5.25 review: AD-43 claimed `RoadSection` carried *ordered* `LaneCorridor` IDs while also making child foreign keys the only persisted parent/child truth, so the derived inverse could only be sorted by opaque `RoadId` — an order that carries no lane meaning. Resolved as follows, without reopening anything else in the Road World Model.

- **Ownership of the fact.** Both facts sit on the child. `LaneCorridor` carries a persisted integer `LateralOrder` and a persisted `IsCrossSectionDatum` flag, beside its existing `SectionId` foreign key. `RoadSection` gains nothing and still carries no corridor collection, authored or reciprocal; the child-owned foreign key remains the only persisted parent/child truth.
- **Semantic reference direction.** Exactly one corridor per section is the **datum**, chosen by the author and deliberately independent of which corridor holds order `0`, so re-authoring the order never silently flips the frame. Ascending `LateralOrder` runs toward the datum corridor's AD-45 road-right (`right = cross(up, tangent)`), evaluated **locally at each arc position**. A local rotating frame rather than a fixed section direction vector is what keeps the order meaningful on a bend, at a fork and around a roundabout's circulating corridors, where a single authored direction would go degenerate. One total order spans the section's whole carriageway, both travel directions included, so a two-way road is a single cross-section rather than two unrelated per-direction sequences.
- **Section frame, not driver frame.** For a corridor travelling opposite the datum, ascending `LateralOrder` runs toward that corridor's own road-*left*. A driver-relative consumer — lane change, overtaking, HUD — therefore resolves side through the corridor's own AD-45 road frame and **never** by comparing `LateralOrder` across corridors of opposite travel direction. `LaneAdjacency.LaneSide` is the authority for driver-relative side; `LateralOrder` is the authority for carriageway position. The two are independently authored, so where they disagree the model fails validation; neither silently takes precedence.
- **Uniqueness and validation.** Per section, `LateralOrder` is unique and contiguous from `0` with no gap, and exactly one corridor carries `IsCrossSectionDatum`. These are structural checks requiring no curve mathematics, so they belong to the records/compiler/validator owner. Each failure gets its own validation code — duplicate order, non-contiguous order, missing datum, multiple data — never one bundled code, because the already-catalogued `SignalizedControlWithoutPlan` bundling is a known defect not to be repeated.
- **Participation in `RoadModelVersion`.** Both `LateralOrder` and the datum flag are behaviour-affecting compiled data and enter the canonical payload. AD-44 excludes *record order*; an authored semantic position is not record order, and two models whose lanes sit in a different cross-section order must not hash identically once a consumer branches on that order. Corridors and sections are already written in stable `RoadId` order, so order-independence holds. Including them increments `CompilerSchemaVersion`, free only while no compiled model has shipped — the reason this is decided now rather than deferred. Implementation note: the canonical writer consumes the compiler's effective-corridor projection, so both fields must be carried through that projection as well as the authored record.
- **Interaction with AD-44 lineage.** Both fields are authoring-mutable, so lineage matching never keys on either. Inserting a lane mid-order shifts every sibling's `LateralOrder`: that is a version change, never an identity change, and the stable IDs of the shifted corridors are untouched. A split or merge re-authors the section's order, which the AD-47 migration report disposes explicitly.
- **Derived collection.** The compiler-built corridors-by-section list is ordered by ascending `LateralOrder`, with ascending `RoadId` retained solely as a deterministic tie-break for a model that already failed validation. Every other derived inverse keeps its `RoadId` ordering, which is a cache ordering and claims nothing.
- **Grounding: a datum orders only what it spans.** Every other corridor of the section overlaps the datum's arc domain over a non-empty interval, and every corridor of the section runs roughly parallel or roughly antiparallel to the datum. A corridor with no overlap — a shoulder that ends before the datum begins, a deceleration lane that starts after it ends — would carry an order no geometry could ever ground or refute, so a set that cannot all be grounded on one datum is not one cross-section and is authored as several sections. A section holding one corridor is its own datum and carries no geometric claim. A mixed-rotational-sense pair of circulating corridors is excluded by the same parallel/antiparallel rule, which is the assumption the roundabout case silently rests on.
- **Story 5.26 (geometry).** Owns consistency only, and owns it with a decision procedure fixed enough that two conforming implementations reach the same verdict on the same model. Lateral comparison is the **nearest-point projection onto the datum corridor's curve**, restricted to the arc interval where the two corridors overlap, with the smallest `s` breaking a tie — restriction plus tie-break is what makes the projection single-valued on a bend and around a loop, and what makes a taper, a gore or a lane that starts mid-section comparable at all. Over those intervals, corridor centrelines must be strictly monotone in ascending `LateralOrder` along the datum's local road-right, consecutive corridors' width envelopes must not overlap beyond authored tolerance, and the grounding rule above must hold. `LateralOrder` and `LaneAdjacency.LaneSide` are compared over the adjacency's own arc interval and a disagreement anywhere within it is a failure. Story 5.26 never derives, reorders or repairs the authored order; a disagreement is a validation failure, not a correction.
- **Story 5.27 (migration).** Owns populating `LateralOrder` for every migrated corridor and designating the datum for every emitted section from V1 module geometry, and disposing both explicitly in the migration report under AD-47. A section whose corridors cannot be totally ordered along its datum frame, or cannot all be grounded on one datum, is a hard failure; a defaulted `0`, an arbitrary datum, or a section silently split to make the check pass without appearing in the report is never an acceptable outcome.

### Dependency direction inside the model

```text
  RoadSection ---> LaneCorridor ---> LaneConnection
                       |                 |
                       +--> LaneAdjacency+
                       |
                       +--> JunctionMovement ---> Junction
                                      |              ^
                          JunctionControl -----------+
                          ConflictZone --------------+
                          SignalPlan ----------------+

LaneCorridor <--- Portal
ImportManifest / SourceTrace - - > any authored/imported record
```

All references resolve by stable ID. Runtime indexes are compiled caches, never authored identity.

## Stable identity and model version

- `RoadModelId` and every semantic record ID are opaque serialized 128-bit values generated once.
- IDs are independent of GameObject name, hierarchy order, array index, transform, scene path and runtime `NetworkObjectId`.
- Human-readable labels are diagnostics only and may change without changing identity.
- The importer persists an `ImportManifest` entry per semantic record: stable ID, semantic kind, a persistent importer slot and zero-to-many typed source keys. Source lineage is many-to-many because several V1 nodes may form one corridor and one node/edge may seed several semantic records.
- A source key includes source asset GUID, scene/prefab instance identity, component/object file ID and relation endpoint identity where relevant; array position, display name and transform are not keys. A surviving manifest slot reuses its ID after move, rename or reordering. Grouping/splitting sources or recreating an object requires explicit remap or new IDs.
- Deleted IDs are never silently recycled. Import history may retain tombstones, but the compiled runtime model contains only live records.
- **Re-import stability.** Re-running the import/compile pipeline over an unchanged source set must not mint new identities: where lineage establishes that a V2 entity is the same logical entity as in a previous import, its existing stable ID is preserved; genuinely new entities receive new IDs; and an entity that disappears or is replaced is disposed explicitly by the migration report instead of silently reappearing under another identity. How the mapping is persisted and compared — manifest contents, storage and matching order — remains an implementation decision; the behaviour above does not.
- `RoadModelVersion` is emitted only by the Road Model compiler from its canonical compiled payload plus `CompilerSchemaVersion`. The payload includes every behavior-affecting ID, effective default, curve sample, width, topology, adjacency, movement, control line, conflict zone, signal plan, portal and validation-profile value. **Canonicalization is order-independent:** two semantically identical compiled models produce the same version regardless of source traversal order, collection or iteration order, record insertion order or serialization order, and the payload is serialized deterministically — fixed numeric encoding and quantization, normalized units, stable set ordering — with the concrete format left to implementation. Any behaviorally meaningful change to topology, geometry, usable width, movement or control semantics, conflicts, signals or portals **must** change the version. It excludes labels, source trace, editor metadata, record order and rebuildable indexes/caches. Consumers compare the emitted value and never recompute it independently.
- Compact integer ordinals may be built by sorting stable IDs for hot runtime access. They are valid only for that exact model version and are never persisted or replicated as durable identity.

Duplicate or missing IDs, unresolved references, an incompatible manifest remap, or a behavior-affecting content change without a new version are hard validation failures. One source key may legitimately trace to multiple semantic records; identity uniqueness is enforced on semantic IDs and importer slots, not on lineage edges.

## Geometry and localization contract

### Canonical geometry

Both `LaneCorridor` and `JunctionMovement` expose the same package-independent directed-curve queries:

```text
Length
Sample(s) -> position, tangent, road-up, curvature, left width, right width
Project(world point) -> s, signed lateral offset, signed normal offset, distance
Bounds(segment) -> 3D spatial-index bounds
```

The canonical coordinate is `s`, arc length in metres from the directed start, clamped to `[0, Length]` by point projection. At a sample, `forward = tangent`, `up = road-up`, and `right = normalize(cross(up, forward))`; signed lateral offset is positive to road-right, while normal offset is positive along road-up. Left width covers negative lateral values and right width positive values. Signed heading error is the angle about road-up from tangent to actor forward in `[-180, 180]`, with absolute error above `90 degrees` classified as wrong-way. `curvature` is signed, never an unsigned magnitude: it is positive when `dTangent/ds` points along road-right and zero on a straight, and projection never extrapolates `s` past the clamped domain even for an off-envelope pose. `Bounds(segment)` returns a 3D bound for an `s` interval that contains the full width envelope, not only the centerline samples. A corridor is therefore a swept drivable envelope, not a centerline with a global guessed width. A `JunctionMovement` is an envelope by the same rule: its own left/right width profile is authored with the movement — seeded from its approach corridor but never silently inherited — and the compiler validates lateral continuity against both adjacent corridor envelopes at the seams. The road-up profile keeps slopes, banking and vertically separated roads representable. World-up-only projection is forbidden as a general runtime rule.

Authoring may use transforms, polylines or spline tooling, but compilation emits immutable deterministic arc-length samples, curvature and 3D bounds. Traffic V2 is not bound to `com.unity.splines` or any transitive package. That package is present in the project only transitively — resolved `2.9.0` at depth 1 through Cinemachine (`Packages/packages-lock.json`) and consumed by no `Assets/RoadRage` C# source — so "not bound" must never be shortened to "not installed". A later **direct** package dependency requires its own adoption decision; no package is needed to express this contract.

Initial `MVP_Run` validation targets are **proposed V2 acceptance gates, not measured V1 behaviour**:

| Check | Gate |
| --- | --- |
| V1 connector candidate discovery, migration only | at most `0.75 m` separation and same travel direction, preserving the current authored threshold |
| compiled-curve chord error | at most `0.05 m` |
| continuation seam gap | at most `0.05 m` |
| continuation or movement endpoint tangent mismatch | at most `5 degrees` |
| migrated source node to reviewed curve | at most `0.10 m`, except an explicitly recorded smoothing override |
| portal source position to reviewed portal | at most `0.05 m` |
| lateral vehicle clearance | default AI half-width plus at least `0.25 m` on each side |
| declared drivability — steering admission | every sample of every element of a model that declares a drivability profile: steady-state steering speed ceiling v*_ss ≥ minimum active steering speed, equivalently radius ≥ R_adm (4.0344 m for the current AI profile); both derived from the declared vehicle steering geometry, never set independently |
| declared drivability — channel consistency | reference trajectory (integral of the stored, linearly interpolated curvature) within the compiled-curve gate (0.05 m) of every position and chord; each chord direction inside its interpolated tangent range; stored tangent equal to the integrated heading (float noise only); constant road-up |
| declared drivability — envelope fold | runtime envelope edge never reverses (closed-form segment rule); inner half-width × curvature < 1 along the reference trajectory; both envelope edges within the seam gap tolerance at every seam |

The current AI box is `2.06 m` wide and the two-way greybox road is `8 m` wide with lane references at `+/-2 m`; a reviewed `4 m` candidate lane width satisfies the initial clearance gate. The importer may propose that width, but only explicit review makes it authoritative. V1 carries no authoritative lane-width field.

Only the `0.75 m` connector-discovery gate is inherited from V1: it is the authored `TrafficSettingsDef.connectorJoinDistance` default, and V1 closes it with a `Dot(from.forward, to.forward) > 0` direction test that accepts any mismatch below `90 degrees`. V1 has no curve compiler and no seam/drift report, so the chord, seam, tangent, node-drift and portal-drift values are new engineering targets that no current test measures. The first migration run must publish observed maxima/distributions for source connector gap and angle, compiled seam/chord error, source-node drift and portal drift; a relaxation requires an explicit architecture/owner decision and must never happen silently in implementation.

*Added 2026-09-25 (`planning-artifacts/sprint-change-proposal-2026-09-25.md`), owner-approved engineering targets, not measured V1 behaviour.* The three drivability rows apply to models that declare a drivability profile. Runtime admission (`RoadModelDocument.Load`) refuses undeclared models, so every driven model is gated; undeclared test fixtures compile unchanged. The authoritative representation of every element is the **compiled reference**: its compiled samples evaluated by the canonical `RoadCurve` evaluation (`Sample`, `Project` — positions interpolated linearly, tangents normalized-interpolated, curvature and widths interpolated linearly). For a declared model, the rows above prove that its tangent and curvature channels are exact samples of a curvature-continuous witness curve, and that its positions and chords lie within the compiled-curve gate δ_c = 0.05 m of that witness; the witness exists only in the validator. Every containment proof — conflict candidates, overlays, physical clearance, planning feasibility — is computed on the compiled reference and inflated by δ_c, so it covers both the compiled reference and every curve the gates admit. Curvature and the steering speed ceiling come from the validated curvature channel. For a declared model the 5-degree seam tangent row is effectively stricter through the seam-edge rule; seam heading and curvature jumps are published, and their numeric tolerance is set by Story 5.30. The steering speed ceiling is a kinematic steering-authority limit: it ignores grip and tyre slip, which can only lower the real ceiling, and does not replace the grip-based curve limit.

### Localization result

Localization projects the `VehicleFootprint` reference pose supplied by `TrafficFrame` against a 3D spatial index. That footprint contract owns the chassis-local reference origin and front/rear/left/right extents, so localization, stop-line and occupancy systems cannot choose different vehicle anchors. The result covers both ordinary corridors and junction movements:

```text
RoadLocation
  RoadModelId / RoadModelVersion
  ElementKind: LaneCorridor | JunctionMovement
  ElementId
  s
  signed lateral offset
  signed normal offset
  heading error
  flags: OutsideEnvelope | WrongWay | Ambiguous
  localized: true | false
  confidence
  ordered alternative candidates
```

Candidate scoring uses geometric distance, heading, current route, previous accepted element and explicit connectivity. Projection returns every geometrically tied candidate before scoring; deterministic stable-ID ordering breaks only exact score ties. It never chooses by planar nearest point alone. Deterministic hysteresis thresholds live in the model validation profile and prevent element flicker near boundaries.

*2026-09-30 (sprint-change-proposal-2026-09-30.md):* When the caller supplies the vehicle's kinematic offset state, the heading term compares the body with each candidate's kinematic nominal orientation (contract §8): the offset is transported from the vehicle's own anchors along the candidate, seam jumps included. Without an anchor for a candidate, the term is the tangent heading error reduced by a declared dead band that bounds |e| on any admitted element (asin(a / R_adm) plus the seam tangent tolerance). Callers that supply no offset state keep the tangent comparison. `WrongWay` keeps its definition.

Flag meaning is fixed even though its thresholds are profile data. `OutsideEnvelope` means the projected pose or footprint exceeds the accepted element's width envelope at `s`, or the reference point lies beyond one end of the accepted element (hysteresis retention past a seam); the result exposes both causes separately *(2026-09-30, sprint-change-proposal-2026-09-30.md; behaviour unchanged since Story 5.26)*. `WrongWay` means the absolute heading error exceeds `90 degrees`. `Ambiguous` means two or more candidates remain inside the profile's score band after scoring, whether or not one of them is accepted. `localized=false` means no candidate passed the profile's acceptance thresholds; it is mutually exclusive with element identity, and it does not by itself forbid `Ambiguous`. The other flags may coexist, so a displaced wrong-way vehicle is not forced into one lossy status. `confidence` is a deterministic scalar in `[0, 1]` derived from the accepted candidate's score margin, never a random, time-dependent or platform-dependent value. Wrong-way and temporary off-envelope travel remain observable locations rather than triggering a snap, route advance or teleport; an unlocalized result may still carry ordered nearby localization alternatives without claiming one as current. Stop-line and leader clearance use the projected vehicle footprint, not only its reference origin.

### What the Road World Model does not own

- live vehicles, players, pedestrians or obstacles;
- traffic occupancy or current signal phase;
- per-vehicle route, tactical goal, path or motion plan;
- reservations/grants that change during play;
- driver personality, Rage/Fear or rule exceptions;
- recovery timers/progress state;
- Rigidbody or network presentation state.

## Existing geometry migration contract

Replacing `LaneGraph` means replacing it as the V2 runtime authority, not discarding authored work.

The migration path is:

```text
V1 scene modules / LaneNode transforms / portals / turn weights
                         +
              existing road visual geometry
                         |
                         v
             one-way editor/import adapter
                         |
                         v
          candidate semantic Road World Model
                         |
                         v
              validation + visual overlay
                         |
                         v
              reviewed V2 authored asset/data
```

Rules:

- The adapter is editor/import tooling, not a permanent runtime compatibility layer.
- Existing points may seed center geometry, direction, portals and connectivity; they are not assumed to contain lane width, legal adjacency, stop lines, conflicts or signals.
- Missing semantics are surfaced as explicit authoring tasks. The importer must not silently invent right-of-way or junction conflicts.
- Every migrated element receives a stable ID independent of hierarchy order.
- A visual overlay compares imported corridors/movements against the existing district before the V2 data is accepted.
- V1 and V2 never write each other's runtime graph. There is no bidirectional synchronization or dual authority.
- Manual map reconstruction is a fallback for specific failed regions, not the default migration strategy.

### What the importer may and may not derive

| V1/source evidence | Import result | Acceptance |
| --- | --- | --- |
| `LaneNode` positions, forward vectors and explicit successor edges | candidate directed curves and topology | may be accepted after geometry/continuity validation |
| outgoing/incoming connectors already matched by V1 | candidate `LaneConnection` seams | preserve the source match, then enforce the stricter compiled seam gates |
| portal roles and `exitReusesEntry` | candidate `Portal` records | preserve entry/exit semantics and verify portal lifecycle scenarios |
| turn weights | route-preference metadata on the corresponding movement/choice | preserve ratios and state the direction explicitly (a higher value means more preferred), so an importer cannot invert a preference into a cost; they are not right-of-way or signal data |
| road collider dimensions and paired lane references | candidate width profile | always requires explicit overlay review |
| visual proximity of parallel or opposing lanes | no adjacency | `LaneAdjacency` is authored explicitly; opposing travel is never lane-change adjacency |
| crossing movement envelopes | candidate conflict zones | must be reviewed and materialized before model validation passes |
| names such as `Roundabout`, `Junction` or `Connector` | source trace/debug label only | never creates rules or topology by naming convention at runtime |

## Junction, right-of-way and signal authoring

- Every legal junction traversal is an explicit `JunctionMovement` with approach/departure corridor IDs and movement geometry. Its only junction ownership link is the persisted parent `JunctionId`. A traversal that crosses a junction is **always** a `JunctionMovement` and never a `LaneConnection`: `LaneConnection` only continues, merges or splits corridor endpoints outside junction semantics, and junction-wide classification may validate movement controls but never override them.
- Every movement has exactly one authoritative control binding: exactly one `JunctionControl` owns it, and that control alone declares its kind — `Uncontrolled`, `Priority`, `Yield`, `Stop` or `Signalized`. One binding may carry internal states or conditional policy, but it stays one binding: two independent systems must never arbitrate the same movement, and a movement may be neither double-covered nor left uncovered. `Uncontrolled` is an explicit choice, not a fallback. The binding is the authoritative rule: an ordinary `DrivingPolicy` obeys it, and an explicitly authorized gameplay policy such as Road Rage violates only a violable rule through the Traffic Rules/Junction Coordination authority — a granted exception never rewrites the authored control model, never makes an incompatible junction grant legal, and never bypasses `SimulationInvariants` or the SafetyFilter.
- `JunctionControl` alone owns optional cross-lane stop/yield geometry. A connector transform may seed one during import but cannot silently become authoritative.
- Conflict candidates are generated offline from swept movement/vehicle envelopes plus the validation margin. The compiler uses the model's declared maximum supported vehicle footprint, which is owned by the `RoadModel` validation profile as a versioned value; admitting a larger vehicle class requires revalidation/recompilation. Accepted conflict zones are materialized in the model, reviewed in the overlay and versioned. Runtime pairwise geometry inference is forbidden.
- A `Signalized` control requires one valid `SignalPlan`: every signalized control is covered by exactly one signal group, phases reference valid groups, incompatible conflict-zone members are never green together, and missing/empty phases fail validation. Signal groups own this membership; controls and movements do not store reciprocal group IDs.
- An explicitly non-signalized junction owns no dummy green plan. Signal current phase and timing live in the host runtime controller and enter `TrafficFrame`; the Road World Model owns only the plan.
- Roundabouts are represented with ordinary directed circulating corridors plus explicit entry/exit movements and yield conflict zones. `Roundabout` is an authored junction/feature classification used by rules and debug, never inferred from a graph cycle or node count.
- Blocked-exit evaluation references the departure corridor occupancy/envelope of the intended movement. It is runtime coordination, not static geometry and not a special route edge.

This split lets the same movement/conflict representation cover the current T junctions, four-way crossroads and roundabouts without three unrelated arbitration systems.

### Canonical contract examples

| Road shape | Representation |
| --- | --- |
| straight two-way road | one `RoadSection`, two opposing `LaneCorridor` records, explicit continuation links, no adjacency between opposing lanes |
| two lanes in one direction | two directed corridors in one section plus one or two directional `LaneAdjacency` records limited to the legal overlapping `s` intervals |
| T junction | three approach/departure corridor pairs, six explicit legal movements, reviewed conflict zones and explicit control per approach |
| four-way junction | four approach/departure corridor pairs, twelve explicit non-U-turn movements, reviewed conflicts and optional signal groups |
| roundabout | directed circulating corridors, explicit entry/exit movements, yield lines/conflicts at entries and ordinary route connectivity through the ring |

`MVP_Run` exercises every row except same-direction multi-lane adjacency. That row therefore requires a small synthetic EditMode fixture before lane-change implementation; the production scene must not be altered merely to satisfy a schema example.

## `MVP_Run` migration baseline and validation target

Repository inspection on 2026-09-22 establishes the source baseline:

| Authored source | Instances in `MVP_Run` | V1 nodes per instance | Preserved candidate semantics |
| --- | ---: | ---: | --- |
| two-way road segment | 12 | 6 | two directed lane references and continuation connectors |
| four-way crossroads | 1 | 12 | four approaches, twelve legal turn choices, `30/50/20` turn weights |
| T junction | 4 | 9 | three approaches, six legal turn choices, authored weights |
| roundabout | 4 | 17 | three entries/exits, directed circulation geometry and `60/40` exit/continue weights |
| tunnel portal | 4 | 4 | one entry lane/portal and one exit lane/portal |
| **Total** | **25 modules** | **204 nodes** | four entry portals and four exit portals |

Two evidence classes must not be merged. Existing EditMode guards verify no orphan connectors, all four entries reach an exit and the central four-way intersection has twelve legal choices; they do **not** enumerate every `LaneGraph` child and do **not** fail on an unrecognised road-module source, so they do not prove that all module instances come from the five inspected greybox prefabs. The 25-module/204-node five-prefab inventory above comes from the 2026-09-22 static scene audit, and the migration/compiler validation must make it explicit instead of assumed: the report declares its source set, every `LaneGraph` module instance must resolve to a recognised imported source, and an unknown module prefab or a loose lane-node hierarchy is a hard failure rather than a silently stale baseline. Those guards remain migration-oracle evidence; they do not prove V2 semantics.

The static audit also proves these fields are absent and must be authored/reviewed for V2: stable semantic IDs, authoritative width profiles, legal same-direction adjacency, stop/yield lines, conflict zones, priority controls, signal plans and signal groups. Current `MVP_Run` contains one lane in each direction on ordinary sections, so its accepted V2 migration must contain **no lane-change adjacency** unless the road geometry is deliberately changed. Opposing lanes remain separate corridors.

The migration proof is a machine-generated artifact keyed to the exact source dependency hash, importer/compiler version, `RoadModelId` and `RoadModelVersion`; validation rejects a stale or hand-detached report. It is accepted only when it contains:

1. typed many-to-many source-to-stable-ID dispositions for all 204 source nodes, every successor edge, parallel turn weight, connector match and portal role — including every rejected or merged source, not only preserved data — where a preserved turn weight resolves to a named target movement or route choice and every rejection or behavioural change carries an owner-approved disposition and its consequence;
2. reviewed corridor/movement overlays for all 25 module instances;
3. an explicit disposition for every source item: corridor/movement sample, connection, control seed, portal relation, merged lineage or rejected-with-reason, with every recorded smoothing override counted and its maximum deviation published;
4. four entry and four exit portals within the portal tolerance;
5. route reachability from every entry to at least one exit without relying on hierarchy order;
6. twelve explicit movements for the central crossroads and six per T-junction instance;
7. roundabout circulation/entry/exit represented with ordinary model primitives, no cycle inference, and an explicit per-instance disposition of each roundabout's three entry and three exit relations;
8. reviewed conflict/control completeness for every junction; current unsignalized junctions are marked explicitly, not granted an implicit green;
9. localization fixtures for nominal, boundary, displaced, wrong-way, ambiguous-junction and temporarily off-corridor poses;
10. zero hard validation errors, zero undisposed source items, and a human overlay sign-off recording the approver identity and the overlay artifact hash, bound to the same source, import-map, compiler and model hashes in `MVP_Run`; a stale or hand-detached report is rejected, not repaired.

## Static and dynamic world separation

| Concern | Owner | Mutability |
| --- | --- | --- |
| Road geometry, topology and legal semantics | Road World Model | Immutable during one run unless a later explicit dynamic-road contract is added |
| Signal phase, closures and authored rule activation | Traffic Frame source services | Host-mutated, snapshotted per decision frame |
| Actor pose, bounds, velocity and intent horizon | Traffic Frame | Immutable view for one decision frame |
| Lane localization and occupancy index | Traffic Frame builder | Rebuilt/updated by host from the same frame inputs |
| Per-agent observations | Perception | Derived, ephemeral, no world mutation |
| Route/goal/path/motion/recovery state | Owning vehicle planning components | Host-owned, versioned against frame/model IDs |

One host decision cycle reads one `TrafficFrame` version. A planner cannot mix actor positions from one frame with grants or signal state from another without explicitly invalidating/recomputing the result.

## Responsibility contracts

### 1. Road authoring, import and validation

**Consumes:** existing road geometry, V1 LaneGraph data where useful, human-authored semantics.  
**Produces:** validated immutable Road World Model plus diagnostics.  
**Owns:** stable IDs and source-to-generated traceability.  
**Must not:** infer safety-critical junction rules silently or become runtime navigation logic.

### 2. Traffic Frame builder

**Consumes:** Road World Model, host physics/network state, signal controllers and dynamic obstacles.  
**Produces:** one immutable indexed frame containing actors, bounds, kinematics, localization, occupancy, signals/closures and previous published intent horizons.  
**Owns:** frame identity and deterministic ordering.  
**Must not:** decide priority, route or maneuvers.

The frame is a shared indexed view, not a deep object copy per vehicle. Perception queries it and returns compact agent-relative facts.

### 3. Perception

**Consumes:** one Traffic Frame, the agent footprint and current planning horizon.  
**Produces:** objective observations such as leader/follower, adjacent occupancy, obstacles, players/pedestrians, predicted overlap and exit occupancy.  
**Owns:** uncertainty/range/timestamp metadata for its observations.  
**Must not:** decide who has right-of-way, whether a rule may be violated, or which pedal to press.

Structured lane occupancy is primary for ordinary traffic. Physics/spatial queries cover unstructured hazards and validate near-field reality. Neither source alone is sufficient.

### 4. Route Planner

**Consumes:** Road World Model, destination/exit objective, closures, policy costs and current localization.  
**Produces:** a `RoutePlan` of stable lane/movement IDs with reason, cost and model version.  
**Owns:** strategic progress and explicit replan outcome.  
**Must not:** produce steering, negotiate right-of-way or hide a no-route result behind greedy Euclidean motion.

Weighted turn preferences survive as route costs/policy, not as the only routing model. A legal cycle is allowed; endless wandering is not.

*2026-09-29 (sprint-change-proposal-2026-09-29-kinematic-pose.md):* **Intermediate movement objective.** A route request may name one `JunctionMovement` that the route must traverse before its exit objective.

- **Semantics.**
  - A route with an objective is a path in the phased state space (planner state, phase), where phase ∈ {before, after}. The only transition from `before` to `after` is a real traversal of the named movement, over its whole length.
  - The 5.29 rules — authored weights, seeded preferences, the zero-weight rule, and the rule that a continuation's suffix must reach its objective without revisiting its entry state — apply unchanged inside each phase.
  - Within a phase, the objective is the named movement for `before` and the exit for `after`. A state may therefore occur once in each phase, which is a legal cycle, but never twice within one phase.
- **Completeness.**
  - The planner returns a minimum-cost route in the phased space whenever one exists, and `NoRoute` only when none exists.
  - Computing the two phases separately is complete under these semantics. The phases share no state and no constraint; they are linked only by the movement's traversal, and the cost is additive. The minimum over the whole path is therefore the minimum of the first phase plus the movement cost plus the minimum of the second phase.
  - An implementation that couples the phases in any other way must prove the same property. This proof, not the algorithm, is the contract.
- **Cost and plan shape.**
  - The named movement's length and seeded preference are counted exactly once.
  - The concatenated `RoutePlan` keeps ordered identities, occurrences and `s` positions contiguous: the approach occurrence ends at the movement's start, the movement appears once over [0, Length], and the departure occurrence starts at s = 0. There is no duplicate occurrence and no gap.
  - The plan's reason records the objective and the phase boundary.
- **Request outcome.** For one request, the planner reports `Planned`, or `NoRoute` with the failing phase named: `NoRouteToObjective` or `NoRouteAfterObjective`. It never returns a hand-assembled or partial plan.
- **Persistence.** The objective stays in the vehicle's destination state until localization shows the named movement actually traversed to its end. Every replan made before that keeps it. Afterwards, the objective reduces to the exit.
- **Activation boundary.**
  - The Route Planner exposes the objective as an ordinary optional request field. It has no knowledge of tests or of any measurement token.
  - "Measurement only" is enforced where the request is built: only the V2 lifecycle path under the Story 5.31 measurement authorization sets the field. A structural check proves that no other production code sets it.
  - Normal operation keeps routing with an exit objective alone. The objective never changes authored weights, costs or the signed model.
- **Campaign diagnostic, distinct from the planner.** `NotSelectable` is a campaign verdict, not a planner outcome. A movement is `NotSelectable` only if **every** (entry, exit) pair of the model yields `NoRoute` for that objective. Feasibility is topological, so seeds do not change it. A movement that any entry, exit or route can reach is selectable, and the campaign must cover it.

### 5. Traffic Rules and Junction Coordination

**Consumes:** intended junction movement, movement semantics, signal state, observations and existing grants.  
**Produces:** permission/denial, blocking reasons, grant lifetime and deterministic arbitration result.  
**Owns:** mutable coordination state scoped to a junction.  
**Must not:** select a route, generate a path, command a vehicle or invent recovery escalation.

Exit capacity is checked before an entry grant. Conflict ownership and tie-breaking are deterministic and independent of vehicle update order.

Junction coordination is a frame transaction. All requests derived from `TrafficFrame N` are collected, resolved as one stable-ID-ordered batch against coordinator-owned prior grants, and published as versioned grants effective in the next decision frame. A grant carries request/source frame, effective frame, movement, expiry and revocation/denial reason. Tactical planning never acts on an unpublished same-frame grant.

### 6. Driving Policy

**Consumes:** authored personality, Rage/Fear, target context and applicable TrafficRules.  
**Produces:** desired parameters, maneuver costs and explicit rule-exception requests.  
**Owns:** what this driver is willing to attempt.  
**Must not:** bypass SimulationInvariants, perception, path feasibility or shared physics.

Normal and Road Rage drivers use the same planning and physics stack. Road Rage is policy data/logic that may request aggressive gaps, targeted pursuit, permitted intentional contact or a named rule violation; it is not a second traffic controller.

Policy only **proposes** a TrafficRule exception. Traffic Rules/Junction Coordination is the single authority that accepts or denies it and publishes an effective frame-versioned exception. Legal exceptions cannot waive conflict-compatible junction grants. A plan carrying authorized intentional contact includes target identity, scope and expiry so Safety can classify contact without consulting mutable live Rage/target state.

### 7. Tactical Decision

**Consumes:** route progress, observations, grants/denials, policy output, blockers and recovery requests.  
**Produces:** one tactical goal (cruise, follow, stop, yield, change lane, avoid, pursue, collision response, recovery maneuver) plus named constraints and reason.  
**Owns:** maneuver selection and its lifecycle.  
**Must not:** actuate physics or resolve geometric feasibility itself.

Tactical Decision is the sole owner of the active tactical goal. Collision analysis and Recovery may submit versioned requests; Tactical accepts/rejects them with a reason and owns any accepted collision-response or recovery goal through completion/cancellation.

### 8. Path / Motion / Speed Planning

The contracts stay separate even if the first implementation keeps them in one module.

- **Path planning** produces continuous local geometry inside allowed corridors/movements and evaluates candidate maneuvers.
- **Motion planning** associates feasible curvature/acceleration evolution and vehicle footprint with that path.
- **Speed planning** combines named longitudinal constraints: desired speed, road limit, curve limit, steering speed ceiling, leader following, stop line, grant, exit blockage and obstacle limits.
- **Reference trajectory (2026-09-25):** the compiled reference (see the gate table) is the single authoritative trajectory. Validator, conflict generation, overlays, physical clearance and planning all use it, with any containment proof inflated by δ_c. No consumer produces an alternative reference by re-fitting, re-smoothing or re-sampling it into a different curve.
- **Lateral quantities are distinct:** δ_c (representation allowance, already inside every proof); `LateralClearanceMarginMeters` (**reserved** clearance, never consumed by tracking or planning); the tracking tolerance ε_t (a bound on the controller's lateral deviation, declared by the driving story and verified by measurement, never assumed; an observed deviation beyond it is a failure); a deliberate planning offset o(s). *2026-09-29 (sprint-change-proposal-2026-09-29.md):* ε_t bounds the road-plane displacement of the eight corners of the maximum-gauge box relative to the kinematic nominal pose at matched progress. It is exact at every simulated physics step; between steps it holds only under the per-step-verified integration model of Story 5.31, with a Lipschitz remainder and seam splitting, and it is never presented as a continuous physical guarantee beyond that model. The reserved margin and any residual clearance are never converted into an allowance.
- **Kinematic nominal pose** *(2026-09-29, sprint-change-proposal-2026-09-29-kinematic-pose.md)*: the nominal pose of the maximum-gauge box at progress s is the pose of the declared drivability geometry — reference point `a` ahead of a non-steered rear axle, wheelbase `L` — whose reference point lies on the compiled reference with no lateral slip at the rear axle.
  - **Pose.** Position is the reference position at s and up is road-up. With e = ψ_tangent − ψ_body (the same sign convention as curvature, positive toward road-right), the nominal signed heading error in the localization convention is −e(s). The offset evolves as **de/ds = κ(s) − sin(e)/a**, where κ is the validated curvature channel.
  - **Steady state.** On a constant-curvature arc with a·|κ| < 1, which the admission radius guarantees, e converges monotonically to e* = asin(a·κ) from any offset in (e* − π, π − e*). Its local relaxation length is a / cos(e*) = a / √(1 − a²κ²). That steady value is not the nominal pose, because it jumps wherever κ jumps. The body rotation rate is dψ_body/ds = sin(e)/a. It is continuous, and it — not the tangent rate — drives every between-sample term.
  - **Continuity and reset.** e is set to 0 only at the planned insertion, at the entry portal's effective progress s_p on its element, which may lie inside the element. There the vehicle is placed tangent-aligned to the tangent at s_p. Nowhere else is e reset. The body heading is continuous across every seam — `LaneConnection` continuation, merge or split, and corridor-to-movement or movement-to-corridor — so e jumps there by exactly the signed tangent jump of the seam: e⁺ = e⁻ + Δψ_tangent.
  - **Three quantities stay distinct.**
    1. The nominal pose model is part of this contract. It is versioned and recorded by every Gate A evidence.
    2. The physical residual — tyre slip, parallel (non-Ackermann) steering, control lag — is never modelled, only measured, and lies inside ε_t.
    3. The allocation a_e = max |o(s)| + ε_t inflates every proof around the nominal pose set.
  - **Route-independent offset bounds.** Each element X carries an entry interval I_X = [e_lo, e_hi] at its start (s = 0). The intervals are computed over the directed transition graph the Route Planner may emit, legal cycles included:
    1. **Portal seeds.** An entry portal at effective progress s_p on X seeds the solution started from 0 at s_p. That solution is valid on X for s ≥ s_p only, and s_p may lie inside X. An element with neither an entry portal nor an admissible predecessor is reported unreachable and carries no interval.
    2. **Exit set and transfer.** The exit set O_P of a predecessor P is the hull, at P's end, of:
       - the images of I_P through P's offset equation;
       - every portal-seeded solution on P.

       The map Φ_P carries O_P into X through the seam jump. Every image is monotone in its initial value, because one-dimensional solutions of the same equation never cross, and adding a constant jump preserves order. An interval therefore maps to the interval of its endpoint images.
    3. **Closure (inductiveness).** The family {I_X} is accepted only if, for every element X, ⋃_P Φ_P(O_P) ⊆ I_X. Candidate intervals may come from any iteration or widening. The check alone makes them sound: by induction on route length, every admissible route's offset at every element start lies in I_X, including routes through cycles and routes that began at a portal inside an earlier element.
    4. **Explicit failure.** If no candidate passes the check within the declared iteration budget, or if any interval reaches |e| ≥ 90°, evidence generation fails explicitly with `HeadingOffsetBoundNotClosed`.
    5. **Reachable set and measurement.** At progress s on X, the reachable offsets lie in [e_lo(s), e_hi(s)]: the hull of the solutions started from e_lo and e_hi at X's start and of every portal-seeded solution with s_p ≤ s. ε_t is measured against the route's own solution, which the closure places inside that set.
  - **Physical envelope.** Every proof covers the union, over every s of every compiled segment and every offset e ∈ [e_lo(s), e_hi(s)], of the inflated footprint. The two extreme footprints alone do not suffice.
    - **Construction kept.** The proofs keep their construction: discrete oriented rectangles of the maximum gauge, the inflation applied as a distance slack (invariant under rotation), and the rule that every point of the footprint between two evaluated poses stays within δ/2 of one of them. ρ is the distance from the reference point to a corner of the non-inflated rectangle.
    - **Between progress samples,** along one offset solution: δ = |Δp| + ρ·Δθ_max, with Δθ_max = Δs · max|sin(e)/a| over the segment. This bounds the body rotation; it is never the endpoint tangent change. The existing fail-closed rule for a rotation of 90° or more applies to Δθ_max.
    - **Across offsets at one progress sample,** a grid of step h_e covers the hull of [e_lo(s), e_hi(s)] over both adjacent half-segments, both ends included. Every intermediate heading is then within h_e/2 of a grid heading, so every intermediate footprint point lies within ρ·h_e/2 of the corresponding grid footprint.

    Both remainders, δ/2 and ρ·h_e/2, are declared, published per proof, and added to the inflation. They are never taken from the reserved margin.
  - **Feasibility.** At every s and for every offset in the interval, the implied steering angle tan δ = (L/a)·tan e must stay within the declared lock at the planned speed, and its rate v·|dδ/ds| within the declared steer rate. A pose that violates either is diagnosed `NominalPoseInfeasible`. The trajectory is then infeasible and never counted as covered by construction.
  - **Previously signed evidence.** Evidence computed with the tangent-aligned pose — every evidence signed before this change — does not certify physical coverage under this contract. It is kept as superseded history and can never be used to close Gate B. The coverage verdict requires evidence whose recorded pose model is the kinematic nominal pose.
- **Gate A evidence lifecycle:** each piece of Gate A evidence (conflict candidates, physical clearance) records the lateral tracking allowance a_e it was computed for, which is 0 for the evidence first signed at Gate A.
  - A trajectory is **covered** when max |o(s)| + ε_t ≤ a_e along it. Its signature then stays valid and nothing is regenerated.
  - Otherwise the candidates and the clearance are regenerated with the allowance max |o(s)| + ε_t, and the candidate diff against the signed set is published. The owner decides new pairs, reconfirms or changes materially changed pairs and disposes of orphaned decisions. A non-positive residual is a failure. Gate A is re-reviewed on what changed and re-signed on a new record bound to the regenerated evidence, and the previous record is kept as superseded history.
  - Outside an explicitly authorized measurement-only run, no vehicle drives a trajectory that the valid evidence does not cover. Story 5.31 measurement runs may drive an uncovered trajectory only to collect diagnostic evidence; they do not certify coverage or permit normal insertion, which remains refused until valid evidence covers the trajectory.
  - *2026-09-29:* The allowance enters the inflation of every proof and is recorded in the pair fingerprints; a value recorded only as text covers nothing. When an inflated envelope changes, no prior pair decision is reused without re-evaluation.
- **2026-09-28 Gate A sidewalk clarification:** for the five conventional junctions, clearance evidence has two independent gates on the same inflated compiled reference: physical obstacle-volume separation in the vehicle's vertical range, and planar separation from authored `Sidewalk` surfaces regardless of collider height or activation. The second gate includes the disabled corner colliders of `TJunction_North` and verifies that declared regions cover the visible sidewalks. Each gate has its own result set and input fingerprint; a relevant change, stale fingerprint, or non-positive residual invalidates Gate A. Story 5.51 prepares these proofs; only resumed Story 5.28 binds them with the refreshed roundabout evidence and owner sign-off. None enters `RoadModelVersion`, V1 source hash, or lineage. *2026-09-28 addendum:* drivable road relief (resting on the carriageway, overlapping no `Sidewalk`, no taller than the AI vehicle's static body clearance) is drivable surface, not a physical obstacle; curbs, sidewalks and taller volumes stay obstacles (Story 5.51 criterion).
  - *2026-09-29 (sprint-change-proposal-2026-09-29-kinematic-pose.md):* **Vehicle contacts during driven campaigns.**
    - **Recognized drivable relief.** A collider is a recognized drivable relief only if it belongs to the drivable-relief result set of the valid Story 5.51 physical clearance evidence. It is identified by hierarchy path and that evidence's input fingerprint, never by a name pattern.
    - **Body contacts versus tyre support.**
      - The rule classifies contacts of the vehicle's **colliders**. The V2 prefab has one body `BoxCollider`.
      - Tyres are suspension raycasts (AD-35), never colliders. Their support on the carriageway, including over a recognized relief, is the normal wheel model: it is neither an observation nor a failure. It is published through the grounded-wheel count.
      - A vehicle that adds wheel colliders must classify their contact with the carriageway or a recognized relief as support. Their contact with any other collider falls under the last clause below.
    - **Body contact with a recognized relief.** Such a contact is a **non-blocking physical observation** only while none of the consequence criteria below fires.
      - *Published per contact:* first and last contact step; maximum contact impulse and relative normal speed; speed before, minimum during and after; maximum d and heading error relative to the nominal pose; maximum roll and pitch; grounded-wheel count; and whether the route continues to its exit.
      - *Observation window:* the contact steps ± 50 physics steps. The run records the `Time.fixedDeltaTime` actually in force and publishes the window duration as 50 × that value (1 s at the declared 0.02 s). A value that differs from the declared campaign condition invalidates the run. The window is a declared design parameter, used for publication only.
    - **Consequence criteria.** The contact fails the campaign if, from the first contact to the end of the run, any of the following occurs:
      - *out of bounds:* d > ε_t; the vehicle's reference point or footprint crossing the width envelope of the accepted element; or the localization flag `WrongWay`. *(2026-09-30, sprint-change-proposal-2026-09-30.md)* A longitudinal overrun of the accepted element alone — the hysteresis retention past a seam, or the end of the exit portal's element — is not out of bounds, although it raises `OutsideEnvelope`;
      - *loss of control:* v > v*(s), the route's nominal-pose ceiling, `TrackingToleranceExceeded`, or any V2 fallback command other than the terminal hold at a reached exit portal (`ExitPortalReached`);
      - *abnormal stop:* |v| ≤ 0.05 m/s for 10 consecutive steps while the speed plan's target exceeds `MinimumDirectionSpeed` (the `FallbackHeld` constants of Story 5.31);
      - *failed continuation:* the vehicle does not reach its planned exit.
    - **Exploratory versus acceptance runs.**
      - An exploratory run has no declared ε_t. There, the out-of-bounds criterion uses the localization flags only, and d is published without being judged. Every other criterion is evaluated, and whatever fires is published as a finding. An exploratory run never accepts anything.
      - In an acceptance run, every criterion, including d > ε_t, fails the campaign.
    - **Carriageway grounding.** A contact between the body and the carriageway surface itself is neither a relief contact nor a non-road contact. It is published with the same observables and judged under the same criteria and run distinction.
    - **Any other collider.** A body contact with any other collider keeps its blocking criterion unchanged: it fails an acceptance campaign, and it is published as a finding in an exploratory run. This includes a relief missing from the valid evidence's result set, or one bound to a stale fingerprint.
    - **Scope.** The rule applies to Story 5.31 campaigns and to the Story 5.52 milestone rerun. It changes neither the Story 5.51 relief criterion nor any clearance evidence.
- **Steering speed ceilings** *(2026-09-30, sprint-change-proposal-2026-09-30.md)*. Two ceilings share the declared drivability profile's available lock δ_avail(v) and stay distinct.
  - **Steady-state ceiling v*_ss(κ).** The model publishes it along every element: the highest speed at which δ_avail(v) covers the steady-state lock atan(L / √(R² − a²)) of the curvature κ, or none when that lock is within the high-speed lock. It serves model admission (v*_ss ≥ the minimum active steering speed, equivalently R ≥ R_adm) and evidence review. It is not a driving bound, because the steady state is not the nominal pose.
  - **Nominal-pose ceiling v*(s).** Along a planned route, v*(s) is the highest speed at which δ_avail(v) covers |δ_N(s)|, with tan δ_N = (L/a)·tan e(s) and e the route's own offset solution (kinematic nominal pose). It is none when |δ_N| is within the high-speed lock, and 0 when |δ_N| exceeds the zero-speed lock or |e| ≥ 90°. At a seam, where e jumps, both one-sided values are evaluated and the lower one applies. Every driving-side use of v*(s) means this ceiling: speed planning, the motion-plan verifier, the runtime monitor, the published observed-speed comparison and the loss-of-control criterion.
  - **Enforcement.** Speed planning enforces v*(s) locally along the planned trajectory, decelerating within its declared bound early enough before each tighter curve, never as a single cap equal to the lowest ceiling in the look-ahead. When it cannot be met from the current state, the plan declares that infeasibility as its binding constraint.
  - Only a caller without a declared drivability geometry, such as an undeclared test fixture, has no offset solution. It keeps the steady-state ceiling. Runtime admission refuses undeclared models, so every driven V2 route has an offset solution.
- **Curve limit** *(2026-09-30, sprint-change-proposal-2026-09-30.md; applied from Story 5.31)*. The speed plan also applies v ≤ √(a_lat / |κ(s)|), with a_lat = min(the driver's declared comfortable acceleration bound, the vehicle's lateral grip μ·g). It is a named constraint and can bind. It bounds lateral acceleration, which neither steering ceiling does. A dedicated lateral comfort parameter may replace the driver's comfortable deceleration later; that is not a change of rule.

The result identifies the binding constraint and rejected candidates. No rule writes brake directly.

### 9. Safety Filter

**Consumes:** proposed motion/speed plan, near-field frame facts and SimulationInvariants.  
**Produces:** pass, clamp, emergency stop or invalid-plan rejection with one objective reason.  
**Owns only:** last-moment simulation vetoes for non-finite output, physically invalid intent, imminent unintended collision, stale plan/frame mismatch or invalid actor state.  
**Must not:** choose routes, negotiate junctions, rank maneuvers, decide traffic-law compliance, select Rage targets or escalate recovery.

If the Safety Filter accumulates policy preferences or routine obstacle handling, that logic is moved back to Tactical Decision or planning. “Safety” is not a license for a second Traffic AI.

Safety normally evaluates the plan against its source frame. It may also consume a separately labelled near-field sample from the current physics step solely for SimulationInvariant vetoes. Source-frame and physics-step epochs remain visible; the newer sample never becomes an excuse to recompute tactics inside Safety.

### 10. VehicleDriveIntent composer and physics

**Composer consumes:** the accepted immediate motion/speed command.  
**Composer produces:** exactly one finite `VehicleDriveIntent` per AI per physics step.  
**Physics owns:** applying that intent through `VehiclePhysicsBody` and the Rigidbody.  
**Must not:** read route, Rage/Fear, traffic rules or recovery policy.

`VehicleDriveIntent` and `VehiclePhysicsBody` are retained foundations unless later measured evidence invalidates them.

The retained V1 types do not currently prove fail-closed handling for every NaN/infinity input. Traffic V2 therefore adds an explicit finite-input guard at the composer/physics boundary: any non-finite intent or authority scalar is diagnosed and replaced by `Idle` before physics. This is a mandatory contract test, not a V1 implementation change during planning.

An accepted plan carries `SourceFrameId` plus a validity window in host decision/physics epochs. The composer emits exactly one intent on every physics step from the current valid plan, or `Idle` when no valid plan exists. Traffic decision cadence may later be reduced for profiling, but plan hold/expiry and reaction latency cannot be implicit.

*2026-09-29 (sprint-change-proposal-2026-09-29.md):* For Traffic V2, the replacement output is the V2 fallback command, recomputed every physics step from measured speed: service brake only above the band v_dir + 2·b·Δt, handbrake hold with zero brake-reverse at or below it and while rolling backward, until a diagnosed held stop or stop-overrun. The global meaning of `VehicleDriveIntent.Idle` is unchanged. This is not the Safety Filter.

### 11. Recovery Supervisor

**Consumes:** expected progress, actual progress, blockers, tactical state, collision/invalid-state facts and prior recovery attempts.  
**Produces:** a request to replan route, generate an alternative path, choose controlled reverse/realignment, or declare a separately handled catastrophic failure.  
**Owns:** recovery attempt history and escalation observability.  
**Must not:** grant right-of-way, rewrite TrafficRules, suppress SimulationInvariants, compose intent, move the Rigidbody, teleport, despawn or reinsert normal traffic.

Collision response and recovery are distinct: collision response temporarily changes the tactical goal while physical instability resolves; recovery begins only when a stable vehicle is unable to resume meaningful progress.

Recovery uses a request handshake: the supervisor owns eligibility detection, request identity and attempt history; Tactical Decision accepts or rejects with a reason and owns an accepted recovery maneuver until it reports completion or cancellation. Rejection updates attempt history rather than silently retrying every tick. If no invariant-preserving plan exists, the host traffic lifecycle moves the actor to a diagnosed safe-stop `Faulted` state. Any later cleanup/removal is owned by an explicit run-level catastrophic policy, not Recovery, Safety or Physics.

## TrafficRules versus SimulationInvariants

### TrafficRules

TrafficRules describe legal/cooperative road behavior. A DrivingPolicy may request a named, scoped exception, and Tactical Decision/Junction Coordination must expose that exception in debug state.

Examples:

- speed limit and preferred following gap;
- stop/yield/priority behavior;
- signal compliance;
- lane-change permissions and etiquette;
- keep-clear/blocked-exit convention;
- ordinary wrong-way and sidewalk prohibitions;
- cooperative gap creation.

A rule exception is never implicit. It identifies the rule, scope/target, start/reason and termination condition.

### SimulationInvariants

SimulationInvariants preserve determinism, finite state, ownership and physical/world integrity. No personality, Rage state, recovery tier or authoring flag may bypass them.

Initial invariant set:

1. The host is the sole authoritative traffic simulator and shared-state mutator.
2. One immutable frame version feeds one decision cycle.
3. Exactly one composer emits one finite intent per vehicle/physics step.
4. Only shared vehicle physics writes nominal Rigidbody motion.
5. Stable IDs, deterministic ordering and seeded choices produce repeatable decisions for the same inputs.
6. A plan references existing, version-compatible road and actor identities.
7. A logical traffic agent enters/exits the world only at portals; normal recovery never teleports, reinserts or removes it mid-road. A fidelity representation may unload/reload elsewhere only while identity, route/policy/progress and deterministic restoration survive with no visible pop.
8. No granted movement can knowingly create an incompatible simultaneous junction grant.
9. Intentional contact requires explicit policy authorization scoped to a valid target; all other imminent collisions remain safety hazards.
10. Invalid/non-finite plans and controls are rejected before physics.

The exact collision envelope is not frozen here. It must permit the game's explicit Road Rage contact while preventing undefined or unintended physical commands.

## Blockers and waiting

The external proposal's single `WaitContext` is too lossy for simultaneous causes. V2 retains a set of named blockers/constraints:

```text
Blocker
  Id / Source
  Kind
  BlockingActorOrRule
  Legitimate
  ExpectedToClear
  Recoverable
  SinceFrame
```

One blocker may be selected as the dominant debug reason, but arbitration and recovery read the full set. A red signal, leader and blocked exit can coexist without being collapsed into whichever `if` ran last.

## Determinism and networking

- Host authority remains the replication model; V2 is not required to be deterministic lockstep across clients.
- Within the host, iteration order, tie-breaking, decision seeds and frame versions are stable and inspectable.
- Randomness is derived from session seed + stable traffic identity + decision domain/counter, not `NetworkObjectId` alone.
- Clients receive presentation state and a compact debug projection; they do not reproduce authoritative planning.
- Network bandwidth and decision cadence are measured after functional correctness. They do not justify premature Traffic LOD implementation.

## Traffic LOD target contract

LOD remains architectural because incompatible representations would otherwise be built, but implementation is profiling-gated.

All future levels must preserve across LOD transitions:

- stable vehicle identity and authored driver policy;
- route/destination and lane progress;
- relevant Rage/Fear/target state;
- logical-agent lifecycle ownership and portal-only visible entry/exit, while allowing non-visible fidelity representations to unload/reload without ending the logical agent;
- deterministic re-entry to higher fidelity without unexplained route changes.

Near/mid/far representation, cadence and physics choices remain deferred until a correct full-fidelity V2 is profiled in the target scene. An earlier implementation requires a measured budget breach with a reproducible benchmark.

## Debug contract

Each authoritative decision frame must be queryable as text and visual overlays:

```text
Frame / RoadModelVersion
Vehicle / LaneId / longitudinal coordinate
RoutePlan / next movement
Observations / blockers
Junction permission and reason
Policy and active rule exceptions
Tactical goal
Path/Motion plan id
Named speed constraints + binding constraint
Safety result
Recovery state / expected versus actual progress
Final VehicleDriveIntent
```

Every denial, clamp, replan and recovery escalation has a stable reason code suitable for automated assertions.

## Architecture gate result

**ACCEPTED BY THE OWNER — 2026-09-22.** Kenan accepted AD-43, AD-45 and AD-47 as proposed, and accepted AD-44 and AD-46 subject to two clarifications now incorporated: re-import must preserve the identity of a lineage-identical logical entity while the model version stays canonicalization-order-independent (AD-44), and "one control per movement" means one authoritative control binding that may carry internal states or conditional policy, provided no two independent systems arbitrate the same movement and an authorized gameplay exception never rewrites the authored control model (AD-46). The pre-story Road World Model gate is therefore closed for planning; a later planning run may generate Traffic V2 stories, and none is generated here. Acceptance records no importer, no compiled model and no V2 asset: the first executable work must still build and prove the migration/validation substrate against the stated `MVP_Run` baseline before route, junction or driving behavior depends on it. A later targeted clarification on the same day added AD-48, which resolves the lateral cross-section ordering AD-43 left contradictory (see “Lateral cross-section order” above); it narrows one AD-43 claim and reopens nothing else, and generates no implementation story. Deterministic spine lint is clean, and the reviewer lenses were re-run against the corrected and accepted text — `reviews/review-road-world-model-postfix-rubric.md`, `...-postfix-reality.md`, `...-postfix-adversary.md` and `...-clarification-rerun.md` — none of which adopts the decisions on the owner's behalf.

Owner acceptance will not claim that an importer or V2 asset already exists. The first future executable work must build and prove the migration/validation substrate against the stated `MVP_Run` baseline before route, junction or driving behavior depends on it. Story boundaries must follow independently testable architectural behavior; Safety, collision response and recovery remain separate candidates and are not pulled into Road World Model implementation.

## Explicitly deferred

- concrete C# types, namespaces, Unity asset nesting and editor UX;
- authoring-curve implementation behind the package-independent compiled geometry contract; no new dependency is currently justified;
- a generalized city editor beyond the migration/validation need;
- Traffic LOD implementation until profiling justifies it;
- production-scale traffic count above the existing approximate 30-vehicle validation target;
- dynamic road destruction/closures unless a product requirement activates them;
- catastrophic out-of-world cleanup ownership and allowed transitions; this gates only future collision-response/recovery stories, and teleport, reinsertion and mid-road despawn remain unauthorized until it is decided;
- implementation story numbering and estimates; this run authorizes later planning but does not generate them.
