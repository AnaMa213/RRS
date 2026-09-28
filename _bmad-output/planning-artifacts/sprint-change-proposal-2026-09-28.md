# Sprint Change Proposal - 2026-09-28

**Subject:** Story 5.51 must prove both physical and semantic sidewalk clearance.
**Status:** Approved by Kenan on 2026-09-28 and applied to `epics.md`, `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`, `ARCHITECTURE-SPINE.md` and the draft Story 5.51 spec. No implementation or Gate A signature is implied.
**Mode:** Batch, to present the linked Story 5.51, Gate A, and spec edits together.
**Scope:** Moderate direct adjustment inside Epic 5; no new story, epic, or runtime feature.

## 1. Issue and evidence

The approved 2026-09-25 course correction described right turns crossing visible square sidewalk corners. Story 5.51 in `epics.md` currently measures non-trigger obstacle colliders only. That misses a flat sidewalk outside the vehicle's vertical obstacle range and the disabled corner colliders of `TJunction_North`.

`ProjectSettings/NavMeshAreas.asset` declares `Sidewalk` as area 3. The corner objects `Col_Sidewalk_Corner_*` in `Greybox_Intersection.prefab` and `Greybox_TJunction.prefab` have `NavMeshModifier` with `m_OverrideArea: 1`, `m_Area: 3`, and `m_IgnoreFromBuild: 1`; each has a corresponding visible `Sidewalk_Corner_*` sibling. `MVP_Run.unity` overrides both corner `BoxCollider.m_Enabled` values to 0 on `TJunction_North`, while its visible corner meshes remain. The existing classification therefore expresses the intended sidewalk semantics, but a proof that filters on active colliders alone loses them. The first-corner feasibility check must confirm the declared region and visible surface agree geometrically; a mismatch is a hard stop.

Read-only check in the loaded `MVP_Run` Editor scene on 2026-09-28: all 12 corner declarations report area 3 and an active matching `Sidewalk_Corner_*` renderer; all 12 collider and renderer XZ bounds match (center and extents within 0.01 m). Both north T colliders report `enabled=false`. The scene remained `isDirty=false` and Git gained no Unity-file change. This verifies current classification coverage, not the proposed cut; the feasibility gate must repeat the check after the first override.

## 2. Impact

- **Epic 5 / 5.51:** Existing physical clearance remains mandatory. Add an independent planar non-overlap gate for declared `Sidewalk` surfaces, regardless of collider height or enabled state, on all movements of the five conventional junctions.
- **5.28 Gate A:** Bind its later owner sign-off to both physical and semantic input fingerprints and their clearance results. Story 5.51 produces evidence; it never signs Gate A.
- **Canonical SPEC / PRD role:** `_bmad-output/specs/spec-road-rage-simulator/SPEC.md` and its current-scope companion remain valid; no product scope change. There is no separate PRD file in `planning-artifacts`.
- **Architecture / UX:** No runtime architecture or UI flow changes. Append the dual-evidence clarification to the existing 2026-09-25 architecture note and the Road World Model contract when this proposal is approved. V1 data, 5.50 curves, Synty policy, instance overrides, and before/after regressions stay binding.
- **Testing:** Add flat sidewalk, disabled collider, non-`Sidewalk` roadway, between-sample contact, semantic/visual correspondence, and physical/semantic fingerprint invalidation cases.

## 3. Recommended path

Directly amend the Story 5.51 and downstream 5.28 evidence contracts, then approve the corrected draft spec before implementation. Rollback of 5.50 would not address the missing semantic gate. MVP scope and story order remain unchanged. Effort: medium; main risk: a collider-area declaration may not match its rendered footprint, resolved by the first-corner hard stop and explicit editor evidence.

## 4. Approved edits

### `epics.md`, Story 5.51 - clearance criterion and feasibility

**Old:** Clearance is the strictly positive swept-footprint residual against obstacle collider volumes in the vehicle's vertical range; first-corner feasibility checks collider union, visual, save/reload, and diff.

**New:** Keep that physical gate unchanged and add a second, independent gate: the inflated swept footprint must have strictly positive conservative planar clearance from every surface declared `Sidewalk` in the five junctions, whether the declaring collider is enabled or intersects the vehicle vertically. Resolve `Sidewalk` from the authored `NavMeshModifier` area (area name, not a hard-coded number), including ignored-from-build surfaces. Pair every declared corner surface with its visible sidewalk geometry and reject missing, extra, or materially mismatched coverage. `TJunction_North` disabled corner colliders remain semantic inputs. Apply the same canonical pose/interval/seam bound to planar separation; publish physical and semantic residuals separately by corner and movement.

The first-corner feasibility gate passes only if physical union, declared semantic region, visible surface, saved/reloaded overrides, and scene-only diff are coherent together. A partial pass halts before the other eleven corners. Every evidence record carries separate canonical physical and semantic input fingerprints. The semantic fingerprint covers area declaration, disabled collider geometry and transforms, participation flags, and the paired visible surface mesh/transform/state; relevant changes invalidate the proof.

### `epics.md`, Story 5.51 - verification and acceptance

**Old:** One clearance residual and physical input fingerprint, with physical synthetic tests.

**New:** Require both residual tables and both fingerprints; test a flat `Sidewalk` surface, `TJunction_North` disabled corners, ordinary roadway without `Sidewalk`, a contact only between poses, and independent changes to either input set. A zero or negative residual on either gate fails.

### `epics.md`, Story 5.28; Road World Model contract; architecture note

**Old:** Gate A binds a physical input fingerprint and refreshed nine-junction clearance.

**New:** Gate A binds the 5.51 physical and semantic fingerprints and their results alongside refreshed roundabout evidence and existing model hashes. Any stale input closes the gate; the owner reviews and signs only in 5.28.

## 5. Handoff and approval

Owner approval received on 2026-09-28. The normative edits above are applied; refresh Epic 5 context and mark the approved Story 5.51 spec ready for development through `bmad-build`. Developer: implement the dual gates, first-corner feasibility stop, tests, evidence, and V1 regressions. Success means both positive clearance proofs for all applicable movements, corresponding visible sidewalks confirmed, stable V1/V2 data identities, and every V1 behavioral delta dispositioned by the owner.

**Change checklist:** Trigger/evidence 1.1-1.3 done; epic/dependency impact 2.1-2.5 done (no new epic or resequencing); artifact impact 3.1-3.4 done (canonical SPEC checked, no UX change); path comparison 4.1-4.4 done (direct adjustment selected); proposal/handoff 5.1-5.5 done. Final approval 6.3 and normative edits complete; sprint-status structure needs no change.

## 6. Addendum 2026-09-28 - drivable road relief (owner decision)

**Trigger:** once the first-corner measurer supported tilted boxes, the speed bump `Relief_DosDane_AvenueCenterToEast` (two boxes tilted 5.7 degrees, 0.12 m high, spanning the carriageway of `Avenue_CenterToEast`) made the physical gate fail at -0.025 m for every movement of `Intersection_Center_Crossroads` crossing it. Under the story text, the tilted boxes had been a hard failure.

**Owner decision:** a drivable relief that is part of the carriageway is drivable surface, not an obstacle. Sidewalks, curbs, walls, buildings and non-road lateral obstacles stay obstacles. The rule must be explicit, traceable and conservative, with no ad hoc exclusion.

**Normative edits applied to `epics.md`, Story 5.51:**
- Supported shapes: a `BoxCollider` in any orientation, projected by the hull of its eight corners, which is its exact plan projection. The hull covers the whole volume at every height, and the vertical-range test uses the world bounding box, so an overhanging upper part cannot hide an interaction. Other shapes within reach remain a hard failure.
- Obstacles: drivable road relief is the only exception. A volume in the vertical range is drivable road relief if and only if all three conditions hold:
  - (a) it overlaps no declared `Sidewalk` surface in plan; contact within 0.01 m is not an overlap;
  - (b) its whole projection rests on the carriageway, by exact geometric inclusion (never sampling): the projection minus the union of flat-topped, road-height, non-`Sidewalk` box supports (each dilated by 0.001 m so abutting slabs join) is empty, and it overlaps no declared `Sidewalk` collider; ground below road height, tilted tops and mesh colliders are not proven supports;
  - (c) its height above the drivable surface is at most the AI vehicle's static body clearance, derived from the versioned `Greybox_AIVehicle` profile (`ResolveStaticRideHeight` plus the underside of the body collider: 0.158 m), which exceeds the 0.12 m authored curb height driven by the 5.11/5.13 benches.
- Exempted reliefs are published with their height and bound, and the bound's inputs enter the physical fingerprint.

**Measured effect in `MVP_Run` (measurement code version 2):**
- Drivable reliefs: `Rampe_Ouest`, `Rampe_Est` and `Relief_MarcheBasse_AvenueCenterToEast`, each 0.120 m against a 0.158 m bound.
- The crossroads curbs `Col_Curb_*` remain obstacles, because they overlap the corner sidewalks; they are the physical witnesses of the uncut crossroads right turns.
- No change to `RoadModelVersion`, the V1 source hash, the lineage or any V2 datum.
- No collider is added. Leaving the carriageway stays physically possible for players and AI.
