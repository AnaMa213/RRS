# Road World Model Architecture — Rubric Review (post-fix rerun)

**Lens:** BMad good-spine checklist  
**Reviewed:** 2026-09-22, rerun against the corrected spine and detailed contract. Supersedes `review-road-world-model-rubric.md`, which described the pre-fix draft.  
**Scope:** proposed AD-43 through AD-47 in `ARCHITECTURE-SPINE.md` plus `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`.  
**Verdict:** **PASS — ready for owner review.** All three high and both medium findings of the first run are closed. This rerun raised four residual precision issues; all four were applied to the contract during the rerun and are recorded below.

Deterministic lint (`lint_spine.py`) was re-run after every edit: **0 findings**.

## First-run findings — closure check

| First-run finding | Severity | State | Evidence in the current text |
| --- | --- | --- | --- |
| `RoadLocation` could not identify a vehicle committed on a `JunctionMovement` | HIGH | Closed | `RoadLocation.ElementKind: LaneCorridor \| JunctionMovement` plus `ElementId`; corridor-to-movement transition and hysteresis thresholds live in the model validation profile; "Unlocalized carries no current element". |
| Approach control and stop-line ownership not resolved | HIGH | Closed | `JunctionControl` is the sole control/line owner; `Junction` owns identity, boundary and classification, and its `Must not own` list begins with approach control. |
| Authored relation ownership had several writable sources of truth | HIGH | Closed | "Persisted foreign keys on child records are the sole parent/child truth"; inverse collections and hot indexes are compiler outputs and cannot be authored; `ConflictZone` sole conflict-membership owner; `SignalPlan` groups sole signal-membership owner. |
| Source-to-ID mapping cardinality too narrow | MEDIUM | Closed | Typed many-to-many `ImportManifest`/`SourceTrace`; one source key may trace to several semantic records; uniqueness is enforced on semantic IDs and importer slots, not on lineage edges. |
| `RoadModelVersion` hash boundary undefined | MEDIUM | Closed | Explicit inclusion list (IDs, effective defaults, curves, widths, topology, adjacency, movements, control lines, conflict zones, signal plans, portals, validation-profile values) and explicit exclusions (labels, source trace, editor metadata, order, rebuildable indexes); consumers never recompute it. |

## Findings raised in this rerun and applied

### R1 — `Bounds(segment)` could legitimately exclude the width envelope (MEDIUM) — Closed

Two conforming compilers could satisfy "`Bounds(segment) -> 3D spatial-index bounds`" while one bounds the centerline samples and the other bounds the swept envelope. Both then pass, yet an occupancy or conflict query near a road edge misses a candidate in one of them.

Closed by stating that `Bounds(segment)` returns a 3D bound for an `s` interval that **contains the full width envelope**, not only the centerline samples.

### R2 — a `JunctionMovement` had no stated width profile (MEDIUM) — Closed

The geometry contract gives corridors and movements the same sample query, including left/right width, while the ownership table assigned "left/right width profiles" to `LaneCorridor` alone. A build could inherit the approach corridor width and another author a movement-local width, producing different movement envelopes and therefore different conflict geometry from identical sources — while AD-46 generates conflicts from swept *movement* envelopes.

Closed by making the `JunctionMovement` an envelope by the same rule: its width profile is authored with the movement, seeded from its approach corridor but never silently inherited, and the compiler validates lateral continuity against both adjacent corridor envelopes at the seams.

### R3 — turn-weight polarity was not stated (MEDIUM) — Closed

"Preserve ratios" left the direction open: an importer can preserve `30/50/20` as preference weights or as costs, satisfy the row literally, and route differently. Closed by requiring the direction to be explicit — a higher value means more preferred — which also keeps the row consistent with the Route Planner's ownership of preference and policy costs.

### R4 — `Ambiguous` versus `localized=false` was written as a prohibition (LOW) — Closed

The previous correction made `localized=false` unable to claim `Ambiguous`. That is a statement about a chosen element, not about the candidate set, and it left the flag's meaning dependent on acceptance. Rewritten: `Ambiguous` is set whenever two or more candidates remain inside the profile's score band, whether or not one is accepted; `localized=false` excludes element identity and does not by itself forbid `Ambiguous`.

## Full checklist (rerun)

| Criterion | Result |
| --- | --- |
| Fixes real divergence points for implementation stories | Pass — location, control, conflict, signal, lineage, version, movement envelope and weight polarity are each single-owner and version-bound. |
| Every AD rule is enforceable and prevents its stated divergence | Pass, with R1–R3 applied. |
| Deferred choices cannot cause incompatible builds | Pass — C# types, Unity nesting, editor UX, authoring-curve implementation, spline dependency, LOD and story numbering stay deferred; no deferred item changes a cross-unit meaning. |
| Ratifies brownfield reality | Pass — and the migration baseline is now attributed to the 2026-09-22 static audit, with source-set exclusivity moved out of the test claim and into a required report validation. |
| Covers requested capability scope | Pass — ownership split, stable identifiers, geometry/localization, V1 migration and `MVP_Run` validation are all present. |
| Preserves inherited Traffic V2 invariants | Pass — junction-movement localization closes the AD-37/AD-39 grant, progress and conflict-occupancy path. |
| Operational/validation envelope is addressed | Pass — the geometry gates are labelled as proposed V2 targets and are consumed by a hash-bound migration report. |

## Gate recommendation

AD-43 through AD-47 may go to owner review as `[ASSUMPTION]`. Nothing in this review adopts them, and the architecture gate stays formally open until Kenan accepts or corrects. No implementation or story generation is required to close these contract issues.

**Status note (added after the acceptance turn):** the owner accepted AD-43 through AD-47 on 2026-09-22 with two clarifications; see `review-road-world-model-clarification-rerun.md` for the post-acceptance rerun and the final state.
