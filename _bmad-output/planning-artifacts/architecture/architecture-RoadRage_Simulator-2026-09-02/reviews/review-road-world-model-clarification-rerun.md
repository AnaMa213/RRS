# Road World Model — acceptance-clarification rerun (rubric + reality + adversarial)

**Scope:** the corrected and owner-accepted text of AD-43 through AD-47 in `ARCHITECTURE-SPINE.md` and `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`, plus the two clarifications the owner attached to his acceptance on 2026-09-22.  
**Method:** re-run the three lenses previously used for this gate, focused on what the clarifications changed; the rest of the text is unchanged since the post-fix PASS and was not re-litigated.  
**Verdict:** **no blocking finding.** One gap found by this rerun (deterministic version encoding) was closed during it. Gate may be finalized.

## What the clarifications changed

**AD-44 — re-import stability and version canonicalization.** The contract and spine now state that re-running the import/compile pipeline over an unchanged source set must not mint new identities: a lineage-identical V2 entity keeps its stable ID, genuinely new entities get new IDs, and a removed or replaced entity is disposed explicitly by the migration report instead of silently reappearing under another identity — while how the mapping is persisted and compared stays an implementation choice. The version bullet now requires order-independent canonicalization (traversal, collection/iteration, insertion and serialization order) and an explicit behaviour-change list (topology, geometry, usable width, movement/control semantics, conflicts, signals, portals), with the existing exclusions unchanged.

**AD-46 — control-binding semantics.** "One control per movement" is now stated as one authoritative control binding: a single `JunctionControl` owns each movement and alone declares its kind and stop/yield line, the binding may carry internal states or conditional policy, no two independent systems arbitrate the same movement, and a movement may be neither double-covered nor uncovered. The policy boundary is restated inside the same rule: an ordinary `DrivingPolicy` obeys the binding, and an explicitly authorized gameplay policy such as Road Rage violates only a violable rule through the Traffic Rules/Junction Coordination authority — never by rewriting the authored control model, never by making an incompatible junction grant legal, never outside `SimulationInvariants` or the SafetyFilter.

## Rubric lens

| Criterion | Result |
| --- | --- |
| The new rules are enforceable | Pass — identity preservation, explicit disposal of removals, order-independent hashing and single-binding arbitration are all observable in a migration report or in compiled output. |
| No earlier closure re-opened | Pass — ownership, movement localization, lineage cardinality, version boundary, junction semantics and the migration report contract are untouched or strengthened. |
| Deferrals still cannot cause incompatible builds | Pass — persistence format, comparison mechanism, hash encoding format and internal control states stay implementation choices; none of them changes a cross-unit meaning. |
| Scope discipline | Pass — the clarifications stay inside AD-44 and AD-46; no new record type, no new dependency, no new story. |

**Gap found and closed by this rerun (MEDIUM).** "Order-independent" alone did not make two compilers agree: numeric encoding, quantization and set ordering were still open, so semantically identical models could hash differently across implementations and silently invalidate plans and grants. The contract now requires deterministic numeric encoding and quantization, normalized units and stable set ordering, with the concrete format left to implementation; the spine carries the same clause.

## Reality lens

No new present-tense repository claim is introduced: both clarifications are contract proposals about a model and an importer that do not exist yet. Re-checked against the repository:

- V1 has no control, line, conflict or signal data at all, so "one binding may carry internal states" contradicts no current artefact and cannot retro-describe V1 behaviour.
- V1 has no importer, manifest or model version, so the re-import invariant and the canonicalization rule describe target behaviour, not current capability.
- The already-verified facts this text leans on are unchanged: `TrafficSettingsDef.connectorJoinDistance = 0.75` with its `Dot > 0` direction test, the 8 m / ±2 m two-way module geometry, the 2.06 m AI box, the 25-module / 204-node static-audit baseline, and the transitively resolved `com.unity.splines 2.9.0` with no RoadRage C# reference.
- No Unity source, prefab or scene was mutated; `git diff --name-only -- Assets/RoadRage` is empty.

## Adversarial lens

Two new conforming-pair attempts were built against the clarification text; neither survives.

- **Q1 — "lineage-identical" defined differently.** Build A treats it as equality of source keys, build B as equality through a remap table. Both preserve identity for unchanged sources and differ only when a remap exists — and a remap is already an explicit, reviewable authoring event for grouping, splitting or recreated sources, so the divergence is visible and disposed in the report rather than silent. Accepted as intended behaviour.
- **Q2 — internal control states interpreted as multiple arbiters.** Build A stores a state machine inside one binding; build B treats each state as its own rule owner. The contract now forbids the second explicitly ("two independent systems must never arbitrate the same movement") while permitting the first, so only one of them conforms.
- **Q3 — version drift under a behaviour change.** Checked that each member of the explicit behaviour-change list is inside the version payload as worded: topology, geometry, usable width, movement/control semantics, conflicts, signals and portals all appear in the payload sentence, so a change in any of them must move the version.

## Gate determination

AD-43 through AD-47 are recorded as `[ADOPTED]` on owner acceptance, with the two clarifications incorporated. This review adds design evidence only: it records no importer, no compiled model and no Traffic V2 asset, and it does not generate or authorize implementation stories by itself.
