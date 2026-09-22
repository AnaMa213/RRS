---
name: Claude handoff — Traffic V2 Road World Model architecture
type: work-handoff
status: accepted-finalized
created: 2026-09-22
owner_review_required: false
---

# Handoff for Claude — Traffic V2 Road World Model

> **State: accepted and finalized on 2026-09-22.** AD-43 through AD-47 are `[ADOPTED]` in the architecture spine and the contract status is `architecture-gate-accepted`. Sections written before the acceptance turn ("Binding user direction", "Remaining work") are the record of what was handed over; the resume log at the end carries the outcome and the two owner clarifications.

## Objective

Continue the `bmad-architecture` Fast-path run for the Traffic V2 Road World Model. Finish the reviewer fixes, re-run the architecture reviewer gate, and present the proposed architecture to Kenan for acceptance or correction.

Do **not** implement Traffic V2 and do **not** generate implementation stories during this continuation.

## Binding user direction

- Traffic V1 is frozen at accepted reference commit `210f48811e3f99fbb93c5d2aa75885e65edcf878`.
- Story 5.14 is complete; no extra V1 replay/harness/branch/tag gates this work.
- Traffic V2 is built beside V1 and reuses sound kernels plus shared physics/network foundations.
- The deliverable selected for this run is **Fast path + spine and detailed contract**.
- The requested design scope is:
  - data split among road sections, lanes/corridors, movements, signals and adjacencies;
  - stable identifiers;
  - geometry and localization;
  - migration from the current LaneGraph;
  - validation on `MVP_Run` roads and intersections.
- Fast-path decisions remain `[ASSUMPTION]` until Kenan accepts or corrects them.

## Main artifacts

- Spine: `../architecture/architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md`
- Spine memlog: `../architecture/architecture-RoadRage_Simulator-2026-09-02/.memlog.md`
- Detailed contract: `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`
- V1 oracle: `V1-BEHAVIORAL-ORACLE.md`
- Course correction: `../sprint-change-proposal-2026-09-21.md`
- Epic plan: `../epics.md`
- Sprint/context mirrors:
  - `../../implementation-artifacts/sprint-status.yaml`
  - `../../implementation-artifacts/epic-5-context.md`
  - `../../implementation-artifacts/deferred-work.md`

## Current architecture proposal

Proposed AD-43 through AD-47 are present in the spine and deliberately tagged `[ASSUMPTION]`:

- **AD-43:** separate logical ownership for section, corridor, longitudinal connection, adjacency, movement, control, conflict zone, signal plan, portal and import lineage.
- **AD-44:** opaque generated 128-bit semantic IDs; many-to-many import manifest; compiler-owned model version.
- **AD-45:** package-independent directed 3D arc-length geometry and localization on corridor **or movement**, with explicit sign conventions and composable state flags.
- **AD-46:** exactly one control per movement; conflict and signal membership have single owners; roundabouts use ordinary corridors/movements.
- **AD-47:** one-way V1 migration and a version-bound `MVP_Run` acceptance report covering every source node/relation.

The detailed contract contains the fuller data matrix, geometry query contract, migration rules, canonical road examples and `MVP_Run` acceptance checklist.

## Repository reality already verified

Actual project evidence was inspected through Graphify plus direct source/prefab/scene reading:

- `MVP_Run` contains 12 two-way segment prefab instances, 1 four-way intersection, 4 T junctions, 4 roundabouts and 4 tunnel portals.
- Their prefab LaneNode counts total 204:
  - segment: 6 each;
  - crossroads: 12;
  - T junction: 9 each;
  - roundabout: 17 each;
  - tunnel: 4 each.
- The current tests verify four entry and four exit portals, no orphan connector, and reachability from every entry to an exit.
- The crossroads carries 12 legal turn choices; every T prefab carries 6.
- V1 lacks stable semantic road IDs, authoritative lane widths, legal lane-change adjacency, stop/yield lines, conflicts, right-of-way controls and signal plans.
- The straight two-way module is 8 m wide, lane references are at +/-2 m, and the AI box is 2.06 m wide.
- `com.unity.splines` is only transitive and unused by RoadRage; the proposal correctly avoids binding Traffic V2 to it.
- No file under `Assets/RoadRage` has been changed by this architecture run.

## Reviewer gate state

Deterministic lint was run before semantic reviewer fixes:

```powershell
uv run .\.agents\skills\bmad-architecture\scripts\lint_spine.py --workspace '_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02'
```

Result: `0` findings.

Three independent reviews were then completed:

1. `../architecture/architecture-RoadRage_Simulator-2026-09-02/reviews/review-road-world-model-rubric.md`
   - verdict: changes required;
   - found missing movement localization, ambiguous control/line ownership, duplicate conflict/parent truth, insufficient import cardinality and imprecise version hash boundary.
2. `../architecture/architecture-RoadRage_Simulator-2026-09-02/reviews/review-road-world-model-adversary.md`
   - verdict: not ready before fixes;
   - found reciprocal ownership, incompatible reimport histories, underspecified sign/projection/status semantics and stale/selective migration-report loopholes.
3. `../architecture/architecture-RoadRage_Simulator-2026-09-02/reviews/review-road-world-model-reality.md`
   - verdict: pass with required revision;
   - confirmed the counts/topology;
   - found that existing tests do not prove the claim that **all** road modules come exclusively from the five inspected prefabs;
   - required the new geometry tolerances to be described as provisional V2 targets rather than measured V1 facts.

## Fixes already applied after the first two reviews

The spine and detailed contract now:

- make child foreign keys the persisted parent truth and inverse lists compiler-derived;
- add `JunctionControl` as sole control/stop-line owner;
- make `ConflictZone` sole conflict-membership owner;
- make `SignalPlan` groups sole signal-membership owner;
- localize against either `LaneCorridor` or `JunctionMovement`;
- define right/up/lateral/heading sign conventions and composable localization flags;
- replace one-to-one source mapping with typed many-to-many import lineage;
- define the exact behavior-affecting boundary of compiler-owned `RoadModelVersion`;
- bind migration reports to source hash, compiler/importer version, model ID and model version;
- require disposition of every node, edge, weight, connector match and portal role.

These fixes have **not yet been re-reviewed**.

## Remaining work, in order

1. Apply the reality-review corrections:
   - remove or narrow the detailed-contract sentence claiming existing tests prove all module instances come from the five prefabs;
   - explicitly label `0.05 m`, `5 degrees`, `0.10 m`, `0.25 m`, etc. as proposed initial V2 validation targets, not measured V1 behavior.
2. Append the reviewer-driven corrections and this pause/resume state to the memlog using `_bmad/scripts/memlog.py`; do not manually edit the memlog.
3. Re-run `lint_spine.py`.
4. Re-run all three reviewer lenses against the updated spine/contract. The existing review files describe the pre-fix draft and must be replaced or clearly superseded by post-fix results.
5. Apply any remaining clear fixes. Keep AD-43 through AD-47 as `[ASSUMPTION]` and the gate as review-ready unless Kenan explicitly accepts them.
6. Run consistency searches across spine, contract, proposal, epics, sprint status, Epic 5 context, deferred work and oracle. No artifact may say the gate already passed before owner acceptance.
7. Run `git diff --check` excluding the append-only memlog if its writer preserves existing trailing spaces, and confirm `git diff --name-only -- Assets/RoadRage` is empty.
8. Present the architecture outcome and the few load-bearing decisions to Kenan for acceptance/correction. Do not generate stories in that turn unless separately requested after acceptance.
9. Only after explicit acceptance:
   - change AD-43 through AD-47 from `[ASSUMPTION]` to `[ADOPTED]`;
   - mark the detailed contract architecture gate passed;
   - update roadmap/status mirrors consistently;
   - append acceptance/finalization events to the memlog;
   - run the reviewer/lint consistency checks again.

## Important working-tree caution

The repository was already dirty before this sub-run. Preserve unrelated/user-owned changes, especially `graphify-out/*` and the existing anomaly/artifact edits. Do not reset or clean the worktree. The only intended new edits from this sub-run are BMAD architecture/planning artifacts and reviewer files; no Unity source, prefab or scene mutation is authorized.

## Verification not needed yet

No Unity compilation or tests are required for the current documentation-only draft. The future importer/model story will need deterministic EditMode validation and visual overlay evidence in `MVP_Run`; this handoff does not claim that importer or runtime model exists.

---

## Resume log — 2026-09-22 (continuation session)

Every item of "Remaining work" is done, including the owner decision (item 8) and the post-acceptance finalization (item 9).

1. Reality corrections applied. The companion no longer claims that the guards prove five-prefab scene exclusivity; the two evidence classes are separated and source-set exclusivity becomes a required migration-report validation, with an unknown module prefab or loose lane-node hierarchy as a hard failure. The geometry gates are labelled **proposed V2 acceptance targets, not measured V1 behaviour**, and the single V1-inherited value (`0.75 m`, the authored `TrafficSettingsDef.connectorJoinDistance` closed by a `Dot > 0` direction test) is named with its mechanism.
2. Memlog appended through `_bmad/scripts/memlog.py` in four batches, including a clarification that the earlier "reviewer gates passed" entries refer to the course-correction package and the V1 waiver, not to this gate.
3. Deterministic lint re-run after every edit: `lint_spine.py` → `0` findings.
4. All three reviewer lenses re-run against the corrected text. They found four residual precision issues and one reporting loophole, all applied: `Bounds(segment)` must contain the full width envelope; a `JunctionMovement` owns its width profile (seeded from the approach corridor, never silently inherited, with compiler-validated lateral continuity at both seams); turn-weight polarity must be explicit (higher = more preferred); `Ambiguous` describes the candidate set rather than the acceptance outcome; smoothing overrides must be counted with their maximum deviation in the report. Post-fix reviews, which supersede the pre-fix files (now banner-marked): `reviews/review-road-world-model-postfix-rubric.md`, `...-postfix-reality.md`, `...-postfix-adversary.md` — all PASS.
5. **Owner acceptance recorded on 2026-09-22.** Kenan accepted AD-43, AD-45 and AD-47 as proposed, and AD-44 and AD-46 with two clarifications: re-import identity stability plus `RoadModelVersion` canonicalization (AD-44), and control-binding semantics (AD-46). Both are incorporated in the spine and the contract.
6. **Gate finalized.** AD-43 through AD-47 are `[ADOPTED]`, the contract status is `architecture-gate-accepted`, and `epics.md`, `sprint-status.yaml`, `epic-5-context.md`, `deferred-work.md`, the 2026-09-21 proposal and the V1 behavioral oracle now record the acceptance. No artifact claims an importer, a compiled model or a V2 asset exists.
7. **Post-acceptance rerun.** Deterministic lint is clean, and `reviews/review-road-world-model-clarification-rerun.md` records the rubric, reality and adversarial passes over the clarified text. It found and closed one gap: the version payload had no deterministic numeric encoding, so semantically identical models could still have hashed differently across implementations.
8. `git diff --check` is clean on every tracked BMAD artifact; remaining findings are confined to the append-only `.memlog.md` and to the pre-existing user-owned `graphify-out/graph.json` churn. `git diff --name-only -- Assets/RoadRage` is empty — no Unity source, prefab or scene was touched.
9. **Next step:** a planning run may now generate Traffic V2 stories from AD-36 through AD-47 and the accepted contracts. Implementation stays unauthorized until an approved story exists.
