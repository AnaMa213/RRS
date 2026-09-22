# Road World Model Architecture — Rubric Review

> **SUPERSEDED — pre-fix record.** This review describes the draft **before** the reviewer corrections. The current verdict lives in `review-road-world-model-postfix-rubric.md` (rerun 2026-09-22, verdict PASS). Kept as the history of what the first pass required.

**Reviewer lens:** BMad good-spine checklist  
**Reviewed:** 2026-09-22  
**Scope:** proposed AD-43 through AD-47, reconciled with `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md` and the requested ownership, stable identity, geometry/localization, one-way V1 migration and `MVP_Run` validation scope.

## Verdict

**CHANGES REQUIRED.** The package addresses all requested dimensions and is close to an approvable architecture gate: it separates logical road owners, rejects point-node/runtime inference, defines opaque versioned identity, establishes package-independent 3D arc-length geometry, makes junction semantics explicit, preserves V1 geometry through one-way migration, and defines a concrete `MVP_Run` acceptance report.

However, three high-severity contract gaps still allow incompatible implementations, including one that cannot represent a correctly committed vehicle while it traverses a junction movement. A medium model-version ambiguity should also be corrected before owner acceptance. The deterministic spine lint passes with zero findings.

## Evidence checked

- AD-36 through AD-47 and the Traffic V2 capability/deferred mappings in `ARCHITECTURE-SPINE.md`.
- The complete ownership, identity/version, geometry/localization, importer, junction/signal and `MVP_Run` sections in `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`.
- The companion's Road World Model gate result and explicit implementation deferrals.
- `lint_spine.py`: zero mechanical findings.

## What already passes

- **Scope discipline:** AD-43–AD-47 fix logical contracts while deferring C# names, Unity asset nesting, editor UX, spline dependency and story decomposition.
- **Authoritative model:** V2 has one immutable compiled semantic model; V1 remains import evidence rather than a second runtime authority.
- **Geometry:** directed 3D arc length, road-up, curvature, asymmetric widths and package-independent queries are sufficient foundations for non-planar roads and physically meaningful corridors.
- **Migration:** importer derivation limits are explicit; absent width, adjacency, control, stop-line, conflict and signal semantics cannot be silently invented.
- **Junctions:** movements, reviewed conflicts, signal-plan completeness, blocked-exit separation and roundabout representation avoid the V1 inference traps.
- **Acceptance map:** exact source-module/node/portal counts, dispositions, reachability, movement completeness, localization fixtures, validator errors and visual sign-off form a credible first implementation target without claiming that implementation exists.

## Findings

### HIGH — `RoadLocation` cannot identify a valid vehicle on a `JunctionMovement`

**Where:** AD-45 and the companion's `RoadLocation` shape.

**Problem:** Both corridors and junction movements own directed curves, but localization returns only `LaneCorridorId` plus `s`. While a vehicle is physically between its approach and departure corridors on an explicit junction movement, neither corridor necessarily contains it. The result must therefore become `Ambiguous`, `OutsideCorridor` or `Unlocalized` during entirely valid committed travel, or an implementation must silently project the vehicle back onto an approach/departure lane. Either choice breaks movement progress, conflict-zone occupancy, already-committed junction handling and deterministic recovery.

Two teams could independently solve this incompatibly: one extends localization to movements, another treats movement traversal as an unlocalized special case outside the Road World Model contract.

**Disposition:** **Discuss, then autofix the contract.** Make the localized element explicit and discriminated—e.g. element kind plus stable `LaneCorridor` or `JunctionMovement` ID—with `s`, offsets, heading/status/confidence and alternatives relative to that element. State how transition/hysteresis works at corridor-to-movement and movement-to-corridor seams. Do not require that exact C# type name.

### HIGH — Approach control and stop-line ownership is not actually resolved

**Where:** AD-43 says `JunctionMovement` owns applicable control/line and `Junction` owns declared control kind; AD-46 says each “approach” declares its control; the companion has no logical approach/control-group owner except signal groups.

**Problem:** `Approach` is behaviorally important but not a defined owner. Priority, yield and stop often apply once to an incoming approach and to several movements. Storing them on every movement duplicates authored truth and permits inconsistent control/stop lines among movements from the same approach. Storing one control kind on `Junction` cannot represent a priority-road junction where approaches differ. Signal groups do not solve unsignalized approach ownership.

**Disposition:** **Discuss.** Choose one canonical logical owner before acceptance:

- add an approach/control-group contract that owns incoming corridor, control type, stop/yield line and member movements; or
- explicitly make each movement the only owner and add a hard compilation invariant that movements sharing an approach resolve to one identical approach control/line.

Whichever is chosen, `Junction` should own only junction-wide classification if approach control varies.

### HIGH — Authored relation ownership still has multiple writable sources of truth

**Where:** AD-43 and the ownership table: `Junction` owns movement/conflict membership, `JunctionMovement` references conflict zones, and `ConflictZone` owns movement/corridor membership; similarly parent records and children both carry membership references.

**Problem:** AD-43 promises to prevent duplicate owners, but the current reference directions allow independently serialized lists to disagree. The most safety-sensitive case is a movement listing a conflict zone that does not list the movement, or vice versa. The model says both records own/reference the same relationship without declaring which side is canonical and which side is a compiled reverse index.

**Disposition:** **Autofix.** For every relation, name one authored canonical owner and declare all reverse membership/indexes derived during compilation. At minimum, `ConflictZone` should canonically own its conflicting member set (or the movement should—but not both), with the other direction compiled and validated. Apply the same rule to junction-child and section-corridor membership.

### MEDIUM — Source-to-ID mapping cardinality is too narrow for the migration it describes

**Where:** AD-44 says the importer persists `source-key-to-ID` mappings; the companion says one `SourceTrace` references one semantic record; AD-47 requires every node and edge to receive a disposition.

**Problem:** Migration is not consistently one source object to one semantic record. Several V1 nodes/edges may contribute to one corridor or movement, while one node/connector/portal source can contribute to geometry plus a portal, connection or movement relation. A singular `SourceTraceKey -> stable ID` rule cannot preserve IDs across these many-to-one and one-to-many transformations without unstable ad-hoc choices.

**Disposition:** **Autofix.** Define stable semantic source keys with role/sub-key or explicitly support a relation between source keys and semantic IDs. Preserve the rule that a surviving semantic source identity reuses its ID, but allow a semantic record to retain several source references and one source object to yield several role-qualified semantic records.

### MEDIUM — `RoadModelVersion` does not define its semantic hash boundary

**Where:** AD-44: deterministic content hash over canonical stable-ID-ordered data.

**Problem:** It is unclear whether labels, source provenance, overlay/review metadata and tombstones affect the runtime model version, and how floating compiled geometry is canonicalized. Including diagnostic/import metadata would invalidate every active route/grant for a harmless label or trace edit; excluding an unstated safety-relevant field could leave stale plans apparently compatible.

**Disposition:** **Autofix.** State that the version hashes the canonical compiled runtime-semantic payload—all topology, geometry, legality, controls, conflicts, signals and portals that affect planning—while excluding diagnostics, authoring provenance, review state and tombstones. Require deterministic numeric serialization/quantization, leaving the concrete format to implementation.

## Good-spine checklist result

| Criterion | Result |
| --- | --- |
| Fixes real divergence points for implementation stories | Conditional: broad boundaries are fixed, but location and relation ownership still diverge. |
| Every AD rule is enforceable and prevents its stated divergence | Conditional: AD-43 currently permits duplicate relationship truth; AD-45 omits movement location. |
| Deferred choices cannot cause incompatible builds | Pass for C#/Unity/spline/editor deferrals; the findings above are contract issues and should not be deferred. |
| Ratifies brownfield reality | Pass: V1 points, connectors, weights and portals are migration inputs, not falsely upgraded semantics. |
| Covers requested capability scope | Pass: ownership, stable IDs, geometry/localization, V1 migration and `MVP_Run` validation are all present. |
| Preserves inherited Traffic V2 invariants | Pass except junction-movement localization must be closed to support AD-37/AD-39 grants and progress. |
| Operational/validation envelope is addressed | Pass for the Road World Model slice. |

## Gate recommendation

Resolve the three high findings, apply the two medium clarifications, rerun deterministic lint and this reviewer lens, then present AD-43–AD-47 for owner acceptance. No implementation or story generation is required to close these architecture-contract issues.
