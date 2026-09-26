# Sprint Change Proposal — 2026-09-25

**Subject:** Gate A blocker — the V2 movement geometry of `MVP_Run` is not drivable on the roundabouts, starts left turns at the junction boundary, and sends right turns across square sidewalk corners. Correct the V2 geometry with a model-level drivability validation (Story 5.50), clear the junction corners physically (Story 5.51), and bind Gate A to the physical evidence, before the Story 5.28 sign-off.

**Trigger:** Story 5.28 HALT — owner overlay review of 2026-09-24 in the Gate A review window (captures of `Intersection_Center_Crossroads`, `TJunction_East` and `Roundabout_SouthWest`), followed by the agent diagnostic of 2026-09-24 on the committed model.

**Status:** Edit proposals 1–6 approved individually by Kenan (incremental mode), 2026-09-25, with the owner's corrections and the cross-proposal alignments integrated below. Arbitrations 1–7 decided by the owner the same day. Final review corrections of 2026-09-25 (sound swept-footprint bound in 5.51; Gate A evidence lifecycle in 5.30/5.31 and the contract §8) integrated. **Approved by Kenan on 2026-09-26 and applied the same day to `epics.md`, the 5.28, 5.27 and 5.49 specs, `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`, `ARCHITECTURE-SPINE.md`, `V1-BEHAVIORAL-ORACLE.md`, `sprint-status.yaml` and `deferred-work.md`.** Open, not part of this approval: whether the 5.50 conflict-candidate sweep should reuse the 5.51 interval bound (min end-pose distance − δ/2).

**Scope classification:** **Moderate** — two stories inserted before a gate; frozen invariants of the done Stories 5.27 (design notes only) and 5.49 (ring axis) explicitly renegotiated; one further physical-world exception to the AD-36 freeze; three engineering-target rows added to the Road World Model contract; downstream acceptance criteria added to 5.30, 5.31 and 5.33. No epic added, no AD reopened, no MVP scope change.

**Recommended path:** Direct adjustment — insert Story 5.50 (V2 geometry and model-level drivability) and Story 5.51 (physical corner clearance) between 5.49 and the 5.28 sign-off; 5.28 resumes afterwards with the physical binding of Gate A.

---

## 1. Issue summary

### 1.1 Trigger

During the resumed 5.28 overlay review (after 5.49), the owner flagged three families of movement curves that looked unnatural or wrong: a crossroads left turn leaving its lane early, a T-junction left turn spilling toward the other lane of its branch, and roundabout entry/exit/continuation curves forming hooks and loops near the ring. The owner asked for a consolidated diagnostic tracing the captures back to `RoadWorldModel → JunctionMovement → RoadCurve → overlay → conflict candidates` before any correction.

### 1.2 Problem statement

The V1 → V2 importer (Story 5.27, `V1RoadModelImporter.AddMovement`) builds every junction movement as **one cubic Hermite curve** between the end of the approach corridor and the start of the departure corridor (tangents = corridor end tangents, magnitude = chord), and builds ring corridors by a Catmull-Rom spline through V1 ring nodes spaced about 37° apart. No validator rule bounds curvature or envelope folding. The result:

- **roundabouts** — undrivable geometry (a real defect, common generator, identical on all four instances);
- **left turns** — drivable but the turn starts at the junction boundary, not where the owner intends;
- **right turns** — a physical limitation: square sidewalk corners, not reachable by any curve inside the available 6 m.

The movement curves are the reference paths: the overlay draws the persisted samples, the conflict candidates are computed from them, and the contract (§8) keeps later planning inside them. They must be correct before Gate A.

### 1.3 Evidence (committed model and prefabs, 2026-09-24 / 2026-09-25)

| Finding | Measured | Scope |
| --- | --- | --- |
| Roundabout entries / exits | 2.44 m long; 60° net heading change but 130° cumulative; curvature changes sign; minimum radius 0.25 m | 24 movements, 4 roundabouts, identical |
| Roundabout continuations | 59–61° heading change over a 24° ring arc; minimum radius 1.87 m | 12 movements |
| Ring corridors | radius 5.83–6.19 m between nodes; curvature +0.16 / −0.37 per m (circle: 0.167); end tangents 16–18° off the circle | 12 corridors |
| Loops in the overlay | envelope edges (half-width 4 m) folding back around a 0.25 m radius; 29 and 54 of 129 edge steps run backwards | real envelope property, not a drawing artefact |
| V1 anchor incompatibility | `Ring_Merge_*` lies 7.5° upstream of where the approach-lane axis crosses the ring axis (102° vs 109.5°): no tangential entry can end on it; the importer forces an S-hook | geometric proof |
| Pre-existing | identical centrelines before 5.49 (commit `ffee55e`); 5.49 only widened the envelope, enlarging the loops | importer defect, not a 5.49 regression |
| Left turns | Hermite from boundary to boundary; at the crossing-roadway edge already 29° turned and 1.1 m off the lane axis; body 0.17 m past its own road's centreline; minimum radius 6.33 m (feasible) | 12 movements (crossroads 4, T junctions 8) |
| Right turns | minimum radius 3.80 m; maximum footprint overlaps the square 4 × 4 m corner collider by 0.63–0.67 m; an exact 6 m arc still overlaps by 0.69 m; the best curvature-continuous candidate inside the 6 m budget still overlaps by 0.12 m at zero margin | 12 movements; physical |
| Vehicle steering (`VehicleProfileDef_Default`, referenced by the AI prefab) | wheelbase 3.10 m (wheels ±1.55 m), lock 40° falling linearly to 16° at 26 m/s, same angle on both front wheels, steering inactive below 0.25 m/s → minimum admissible radius R_adm = 4.0344 m (zero-speed radius 4.0064 m has a ceiling of 0.032 m/s) | authoritative project data |
| Validation | no rule bounds curvature or envelope folding; `RoadCurve` channels (positions, tangents, curvature) are independent and only loosely checked | model-wide |

---

## 2. Impact analysis

### 2.1 Epic impact

Epic 5 (Traffic V2) remains completable. Two stories are inserted before Gate A; Gate A now binds physical evidence in addition to the model hashes. No epic is added, invalidated or resequenced beyond the insertion.

### 2.2 Story impact

| Story | Impact |
| --- | --- |
| 5.27 (done) | design notes "movement = dense Hermite", "ring corridors through their nodes", "corridor end = V1 node position" superseded (approved, to be delivered by 5.50); frozen block unchanged; `ImporterVersion` 2 and regenerated report delivered by 5.50 |
| 5.28 (in progress) | sign-off after 5.49, 5.50 and 5.51; new tasks: physical binding of Gate A, roundabout evidence refreshed through `RoundaboutClearance`, tests; re-review of the 9 junctions and changed segments |
| 5.49 (review → **done**, owner-confirmed 2026-09-25) | frozen ring-axis invariant renegotiated (approved, to be delivered by 5.50); physical geometry and applied widths kept |
| **5.50 (new)** | V2 geometry correction and model-level drivability validation |
| **5.51 (new)** | physical corner clearance for right turns (5 conventional junctions) |
| 5.30, 5.31, 5.33 | new acceptance criteria: declared-model admission, single authoritative compiled reference, lateral quantities, steering speed ceiling enforced locally from the first driven slice |

### 2.3 Artifact conflicts

- `epics.md` — two new stories, 5.28 section, program plan (gate table, dependency graph, critical path, story index, test routing, consistency rows, risk R1), 5.30/5.31/5.33 criteria.
- Specs 5.28, 5.27, 5.49 — tasks, code map, design notes, change logs; frozen blocks unchanged.
- `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md` — three gate rows, compiled-reference definition, §8 planning semantics.
- `ARCHITECTURE-SPINE.md` — 2026-09-25 paragraph; wording correction of the 2026-09-23 paragraph (acceptance record unchanged).
- `V1-BEHAVIORAL-ORACLE.md` — 2026-09-25 exception; wording correction of the 2026-09-23 line.
- `sprint-status.yaml`, `deferred-work.md` — tracking.
- PRD `[N/A]` (FR/NFR carried in `epics.md`); UX `[N/A]` (Editor review tool only).

### 2.4 Technical impact

- **5.50:** Editor importer and pipeline (`Features/Vehicles/Traffic/Migration`), runtime model records, compiler/validator and document (`Features/Vehicles/Traffic`), Gate A review window; regenerated 5.27 report and 5.28 artifacts; `RoadModelVersion` changes; V1 source hash and lineage byte-identical. No runtime, physical or V1 change → no V1 regression required.
- **5.51:** `MVP_Run` scene-instance overrides on 5 junction instances, NavMesh rebake, clearance tool; V1 regression before/after (two fresh Editor sessions for PlayMode).
- **5.28 on resumption:** physical fingerprint, `RoundaboutClearance` refresh, sign-off binding and evaluation.

---

## 3. Recommended approach

**Option 1 — Direct adjustment (selected).** Insert 5.50 and 5.51 before the 5.28 sign-off. Effort: medium to high (5.50 is L, 5.51 is M). Risk: medium — mitigated by the owner-decision HALT inside 5.50, the feasibility HALT at the start of 5.51, and the fail-closed Gate A binding.

**Option 2 — Rollback: not viable.** Reverting 5.27 or 5.49 would discard validated work (lineage, dispositions, physical widening); the faulty geometry is replaced, not reverted.

**Option 3 — MVP review: not needed.** The MVP scope is unchanged.

**Why two stories, not an extension of 5.28:** the correction replaces an approved 5.27 design and its committed artifacts, adds contract gate rows, and changes the owner's conflict decisions — all beyond 5.28's code map and behind its Ask First. The physical corner change has a different blast radius (physical world, V1 regression, NavMesh) and depends on the final 5.50 curves, so it is a separate story.

### 3.1 Owner arbitrations (2026-09-25)

| # | Decision |
| --- | --- |
| 1 | Drivability validated **at model level** through a **declared drivability profile**, required at runtime admission (`RoadModelDocument.Load`); undeclared fixtures compile to byte-identical canonical payloads; document format 1 → 2 (fail-closed backward compatibility demonstrated from the reader: format check, canonical re-serialization, integrity hash); `CompilerSchemaVersion` unchanged; single steering admission rule (ceiling ≥ 0.25 m/s ⇔ radius ≥ R_adm = 4.0344 m); channel-consistency rules C1–C5 and fold rules F1–F3 on the real `RoadCurve` representation; 5.25/5.26 fixtures untouched |
| 2 | Vehicle-derived hard floor, no invented margin; steering speed ceiling published per element; any element with a ceiling under 0.25 m/s refused and listed; enforced from 5.31 (first driven slice), carried by 5.30 and 5.33 |
| 3 | Curvature-continuous construction preferred; seam discontinuities published and flagged; numeric seam tolerance deferred to 5.30, never hidden |
| 4 | Stable pair identities; decisions on materially changed pair geometry become historical proposals requiring explicit owner reconfirmation through a targeted review of changed pairs only |
| 5 | Anchor displacement published without a threshold; the 0.10 m node drift measured against the node's own lineage elements |
| 6 | Physical correction through instance overrides on the 5 junction instances; shared prefabs and shared or Synty assets Ask First; clearance evidence and V1 regression before/after mandatory |
| 7 | Full-footprint conflict sweep on all 9 junctions, along valid connected trajectories, handling short elements, excluding ordinary following, respecting junction ownership |

---

## 4. Detailed change proposals

### 4.1 `epics.md` — new Story 5.50 (Proposal 1, approved; inserted after 5.49, before 5.29)

````markdown
### Story 5.50: V2 Movement Geometry Correction and Model-Level Drivability Validation

**Type:** FOUNDATION · **Boundary:** V2 road geometry, model validation, conflict candidates, review tooling · **Complexity:** L
**Implements:** AD-45 (directed arc-length geometry and envelope), AD-36 (V1 untouched), Road World Model contract gate table, 2026-09-25 course correction

As a solo developer,
I want every V2 corridor and junction movement of `MVP_Run` to describe a trajectory the AI vehicle can physically follow, and the model to refuse any that it cannot,
So that the conflict zones, the Gate A review and every later planning layer are built on drivable reference geometry instead of importer artefacts.

**Capability delivered:** corrected V2 geometry for the 4 roundabouts (ring, entries, exits, continuations) and for the 24 turning movements of the crossroads and T junctions; a declared, vehicle-derived drivability profile validated at model level and required at runtime admission; conflict candidates swept with the full vehicle footprint across seams, with an exhaustive diff and targeted owner reconfirmation of changed pairs; a Gate A isolation view of one movement; regenerated 5.27 and 5.28 artifacts ready for re-review.

**Why here:** Gate A signs the overlays and the conflict decisions. The diagnostic of 2026-09-24 shows that the roundabout curves are not drivable and that the left-turn shape starts turning at the junction boundary; signing either would freeze undrivable reference paths into every later story.

**Prerequisites:** 5.27 (importer, lineage), 5.28 pipeline (implemented, sign-off pending), 5.49 (done: physical ring, applied widths).

**Evidence (diagnostic, 2026-09-24, measured on the committed model):**
- roundabouts, identical on all four instances: entries and exits are 2.44 m long, turn 60° net but 130° cumulative, change curvature sign and reach a 0.25 m radius; continuations turn 59–61° over a 24° ring arc (radius 1.87 m); ring corridors oscillate between radius 5.83 and 6.19 m, curvature +0.16 / −0.37 per metre (a circle is 0.167), end tangents 16–18° off the circle; the loops seen in the overlay are envelope edges folding back (half-width 4 m against a 0.25 m radius). Identical centrelines before 5.49: an importer defect, not a 5.49 regression;
- proof that the V1 anchors are unusable as tangent joins: `Ring_Merge_*` lies 7.5° upstream of where the approach lane axis crosses the ring axis, so no curve can enter tangentially and end on it; the importer forces one anyway;
- left turns (12): a single Hermite curve from junction boundary to junction boundary; at the crossing-roadway edge the curve has already turned 29° and left its lane axis by 1.1 m, and the body is 0.17 m past its own road's centreline; feasible (radius 6.33 m) but not the intended shape;
- right turns (12): minimum radius 3.80 m; physical corner conflict handled by 5.51;
- no validator rule bounds curvature or envelope folding, so these curves passed 5.27, 5.28 and 5.49.

**Decisions recorded by the owner (2026-09-25):**
- V1 data, topology and identities are preserved: no `LaneNode` moves, the V1 source hash and the 5.27 lineage stay byte-identical, no identity is minted or retired. A V2 curve proven incorrect is **not** preserved to protect an earlier invariant;
- one logical ring corridor; the 5.49 physical geometry and applied widths are kept unless new evidence requires a separate owner decision;
- left turns follow the option-B intent (aligned entry, turn started at the right place, natural exit) without a pre-imposed line–arc–line construction or a fixed radius; curvature-continuous construction is preferred;
- drivability is validated **at model level** through a declared profile (below), not by a pipeline-only gate; the frozen 5.25/5.26 fixtures stay unchanged;
- conflict decisions on materially changed geometry are never carried over automatically.

**Renegotiated frozen invariants (owner, 2026-09-25):**
- 5.49 "the imported/compiled ring centreline (chords plus Hermite, ~5.83–6.19 m) is the accepted reference geometry and stays unchanged" is **superseded**: the ring centreline becomes the exact circle fitted from the V1 ring nodes (radius 6.0 m), each lineage-associated node within 0.10 m of it. The nodes themselves do not move;
- 5.49 entry/exit width interpolation (2.0 → 4.0 m) is **kept**, unless the fold rules below fail on an entry or exit; a different width law then needs an owner decision;
- 5.27 design notes "movement = one dense Hermite curve", "ring corridors pass through their V1 nodes" and "a corridor ends at its V1 node position" are **superseded** by the requirements below. The 5.27 frozen block (counts, lineage-key rules, single exception category: decision nodes smoothed by turns) is kept;
- 5.28 design note "lateral sweep only" for conflict candidates is **superseded** by the full-footprint sweep; the frozen wording "swept envelopes of the maximum gauge" is kept.

**Geometry requirements (constructions are chosen in-story and recorded; requirements are binding):**
- **Ring:** ring corridors and continuations lie on one exact circle fitted from the module's V1 ring nodes; every lineage-associated ring node lies within 0.10 m of it.
- **Crossing-roadway boundary (geometric reference):** for a movement, the **departure roadway** is the union of the applied envelopes of the two corridors of its departure arm (the departure corridor and its antiparallel partner at the same junction connector), extended along their axis through the junction. Its **near edge** is the line where the approach axis first enters that band. The **approach roadway** is defined the same way from the approach arm. Both come only from corridor envelopes and arm structure. They never depend on movement curves or conflict envelopes, and never on names. Measured today: crossroads, 4.0 m from the centre; T stem, x = 28 m; T through road into the stem, 4.0 m from the stem axis.
- **Turns at crossroads and T junctions:** a turning movement keeps its footprint in its approach lane (reference point within lane half-width − `MaxVehicleHalfWidthMeters` − `LateralClearanceMarginMeters` = 0.72 m of the lane axis) until its reference point reaches the near edge of the departure roadway. Symmetrically, its footprint is inside its departure lane (0.72 m rule) from the point where it leaves the approach roadway. No fixed radius.
- **Documented exception, measured:** a right turn cannot meet the rule at or above R_adm inside the 6 m available. It may start before the near edge, and the distance by which it does is published per movement. Any other movement that needs this exception is an owner decision.
- A fallback to a construction with curvature jumps needs an owner decision.
- **Roundabout entries and exits:** each starts or ends on its approach or departure axis and joins the ring tangentially. Entries apply the crossing-roadway rule with the ring corridor as the departure roadway: the footprint stays in the approach lane until the reference point enters the ring envelope; exits apply it symmetrically. The V2 anchor points move along the ring, and along the approach or departure corridor where needed. The displacement is published per anchor, with no threshold.
- **V1 node association:** each V1 node's 0.10 m drift is measured against the V2 elements associated with it by lineage (its lineage key or its recorded disposition), never against the nearest curve. The only 5.27 exception category stays "decision node smoothed by a turn".

**Driving authority:** the **compiled reference** — the compiled samples evaluated by the canonical `RoadCurve` evaluation (`Sample`, `Project`: positions interpolated linearly, tangents normalized-interpolated, curvature and widths interpolated linearly) — is the single authoritative trajectory. The validator, conflict generation, overlays, physical clearance and planning use it, and every containment proof is computed on it and inflated by the compiled-curve gate δ_c = 0.05 m, so it also covers every curve the gates admit. For a declared model, the tangent and curvature channels are exact samples of one curvature-continuous witness curve (the reference trajectory below), and positions and chords lie within δ_c of it (C1). The witness exists only in the validator: it is never persisted, exposed or followed. 5.30 builds its path horizon, and 5.31 drives, from the compiled reference only; no runtime layer re-fits, re-smooths or re-samples it into a different curve. Constructed elements (ring, entries, exits, continuations, turns) are sampled directly from their analytic construction at a density that keeps each chord within half of δ_c; they are not re-fitted through a spline.

**Declared drivability profile (model level):**
- `RoadModelSource` gains an optional `DrivabilityProfile` with an explicit `Declared` flag. It carries the vehicle's steering geometry, copied from `VehicleProfileDef_Default` (checked by a test): wheelbase 3.10 m, reference point 1.55 m ahead of the rear axle, lock 40° falling linearly to 16° at 26 m/s, steering inactive below 0.25 m/s.
- The compiler derives everything else: available lock δ(v); radius R(δ) = √((L / tan δ)² + a²); required lock for a curvature κ; steering speed ceiling v*(κ), or "none" when the required lock is 16° or less.
- **Single admission rule:** an element is admitted only if v*(κ) ≥ 0.25 m/s everywhere, which is equivalent to a radius of at least **R_adm = 4.0344 m**. The zero-speed radius, 4.0064 m, is refused (ceiling 0.032 m/s). A radius at or below 1.55 m is refused without producing a non-numeric value.
- The ceiling is a kinematic steering-authority limit and ignores grip and slip. The grip-based curve speed remains 5.33's.
- The canonical block is written only when declared, so undeclared payloads and the 5.25/5.26 fixture versions are byte-identical. `CompilerSchemaVersion` is unchanged.
- The document format goes from 1 to 2. An older reader refuses a new document at the format check, at canonical re-serialization and at the integrity hash.
- `RoadModelDocument.Load` refuses an undeclared model.

**Channel consistency and fold rules (declared models only):**
- **Reference trajectory (witness):** the curve obtained by integrating the element's stored curvature (linearly interpolated, as `RoadCurve` does) from its first position and tangent. It is curvature-continuous, and its maximum curvature is reached at a sample.
- **C1:** the reference trajectory stays within the compiled-curve gate (0.05 m) of every stored position and of every chord between them.
- **C2:** on each segment, the chord direction lies within the angular range swept by the interpolated tangent. Float noise is absorbed by the existing `UnitTolerance`.
- **C3:** at each seam, the existing gates apply between the reference-trajectory ends (0.05 m, 5°). Heading and curvature jumps are published, and a jump is flagged when jump / adjacent step > 1/R_adm. The numeric seam tolerance is left to 5.30.
- **C4:** the road-up is constant on declared elements; otherwise the element is refused.
- **C5 (tangent ↔ integrated heading):** at every sample, the stored tangent equals the heading of the reference trajectory, meaning the first tangent plus the exact integral of the stored curvature. The only tolerance is float noise, absorbed by the validator's existing `UnitTolerance`, taken as the same unit-vector deviation. C5 ties the tangent and curvature channels together exactly; C1 ties both to the positions within the compiled-curve gate; C2 ties each chord to its end tangents.
- **F1:** on the envelope `RoadCurve` actually builds, for each segment and each side: |c| · min(cos α₀, cos α₁) > w_max · 2 · tan(Δ/2).
- **F2:** on the reference trajectory, inner half-width × |κ| < 1, with the quadratic maximum computed analytically on each segment.
- **F3:** both envelope edges meet the existing 0.05 m seam-gap bound at every seam.

**Conflict candidates:**
- Candidates are swept with the full maximum footprint inflated by the margin and δ_c (`MaxVehicleLengthMeters` / 2 beyond each movement end) along valid directed connected trajectories only, continuing across elements shorter than half a vehicle.
- Overlaps that exist only because one movement directly follows another along a single-lane path are published as **following**, not as candidates; same-approach pairs keep their existing "following, not conflict" disposition.
- Each zone stays owned by its movements' junction, and no cross-junction pair is created.
- Every element shorter than `MaxVehicleLengthMeters` inside or between junctions is listed in the report.
- The rule applies uniformly to the 9 junctions.

**Decision reconfirmation:**
- Pair identities are stable. Each conflict decision records the geometry fingerprint of its pair: the canonical geometry of both member movements and the zone volume.
- A decision whose fingerprint no longer matches becomes a **historical proposal**. It is shown in a targeted review of the changed pairs only, and it blocks Gate A until the owner explicitly reconfirms it or changes it.
- Pairs with identical geometry keep their decisions. A new pair needs an owner decision. A removed pair leaves an orphan decision that the owner disposes of explicitly; it never receives a new active decision. Both stay hard failures until resolved.
- The pipeline never writes, transfers, deletes or approves a decision.

**Gate A isolation view (read-only):**
- one movement alone: start and end markers, direction chevrons, minimum radius and speed ceiling, the curve drawn in red where it breaks the admission rule, optional envelope, and its V1 source nodes;
- no data change, and the canonical overlay text is unchanged.

**Published per element:** minimum radius, speed ceiling (with every value under 0.25 m/s listed as refused, plus the full distribution), net versus cumulative heading change, seam heading and curvature jumps, largest disagreement between stored curvature and the three-point circle, and anchor displacement.

**Non-goals:** no physical change (5.51); no V1 change; no roundabout or junction control kind (5.35); no runtime; no Gate A signature; no decision written by the agent; no second ring corridor or adjacency.

**Must NOT be copied:** moving `LaneNode`s to shape curves; relaxing a gate or reshaping accepted geometry to make a test pass; a pipeline-only drivability gate; drift measured against the nearest curve; carrying a decision onto changed geometry.

**Execution sequence (binding):**
1. Implement the importer, drivability, validation, document, sweep, diff and isolation-view changes; synthetic tests pass.
2. Run the pipeline in **diff mode**. Diff mode is read-only: it writes only a review report. It never writes the decisions, model, overlay or Gate A report. It publishes the exhaustive diff: new, removed, materially changed and following pairs; per-element geometry changes; the drivability tables.
3. **HALT: owner decisions.** Present the new pairs (to decide), the materially changed pairs (historical proposals to reconfirm or change) and the removed pairs (orphan decisions to dispose of explicitly), each viewable in the isolation view. The agent never edits, transfers, deletes or approves a decision. The owner makes every change, by his own edit or explicit tool action.
4. Resume only when no decision is missing, orphaned or unconfirmed. Compile with the owner's decisions, then regenerate the definitive 5.27 report and 5.28 artifacts. Run the full EditMode suite.
5. The story is not marked complete while any owner decision is unresolved.

**EditMode verification:**
- **Unchanged tests:** the 5.25/5.26/5.27 files are unchanged and pass.
- **Canonical compatibility:** a golden canonical hash of an undeclared model is captured before the change and still matches.
- **Document format:** an extra member in a format-1 document is refused as non-canonical; a format-2 document is refused by the format-1 path.
- **Admission boundary:** R_adm = 4.0344 m is admitted with a ceiling equal to 0.25 m/s; 4.0064 m is refused. A left/right radius grid from 3.5 to 20 m shows that admission, "ceiling ≥ 0.25 m/s" and "R ≥ R_adm" agree. The ceiling never decreases as the radius grows, and is "none" from 10.93 m.
- **Channel consistency:**
  - adversarial case: straight positions, stored curvature 0, tangents turned 30° (and alternating ±30°), refused at steps of 0.05 m and 1 m;
  - a circle stored with curvature 0 or 2/R is refused;
  - kinked positions with straight tangents are refused;
  - tangents turned uniformly by 2° on an exact circle — inside C2's chord range and within C1's 0.05 m — are refused by C5;
  - honest straights, circles and clothoid curves are admitted; C5 float-noise error published per element.
- **F1 soundness:** on a deterministic grid of real `RoadCurve` segments (varying, asymmetric widths; steps from 0.05 to 1 m), F1 is checked against a dense edge evaluation.
- **Folds and seams:** a pure fold (R 4.5 m, inner half-width 4.6 m) is refused; a mid-segment peak in F2 is refused; seam jumps of 1° and 0.5° at a 4 m half-width are refused and admitted respectively; a varying road-up is refused.
- **Declared admission:** an undeclared model is refused by `Load`.
- **Real map (`MVP_Run`):**
  - ring nodes lie within 0.10 m of the fitted circle;
  - every V1 node lies within 0.10 m of its lineage-associated elements;
  - each turning movement's approach-lane and departure-lane footprint rule holds against the crossing-roadway boundary; the right-turn exception distances are published; the boundary is shown to be independent of movement curves (identical before and after the geometry correction);
  - all corridors and movements are admitted;
  - the lineage and the source hash are byte-identical;
  - no identity is minted or retired.
- **Candidates:** a synthetic chain with a corridor shorter than half a vehicle between two movements finds the pair; ordinary following is published as such.
- **Diff and reconfirmation:** the exhaustive candidate diff is published; a decision on a changed pair blocks Gate A until reconfirmed.
- **Regenerated artifacts:** the committed artifacts equal a fresh pipeline run.

**PlayMode verification:** none. No runtime, physical or V1 change; V1 traffic code does not reference Traffic V2, which a test asserts. A visual check of the 9 junctions and of the segments whose V2 geometry changed happens in the Editor, under the double state guard.

**Completion evidence:**
- the per-element drivability table;
- the anchor displacement and V1-node association table;
- the exhaustive candidate and decision diff, with the targeted reconfirmation list;
- the owner's resolution of every new, removed and materially changed pair (from the step-3 HALT), with no decision missing, orphaned or unconfirmed;
- the regenerated 5.27 report (`ImporterVersion` 2) and 5.28 artifacts, with the `RoadModelVersion` change recorded.

**Unlocks:** 5.51 (physical corner clearance sized on the final right-turn curves).

**Acceptance Criteria:**

**Given** the corrected importer and the unchanged V1 source
**When** `MVP_Run` is imported
**Then** the V1 source hash and the 5.27 lineage are byte-identical and no identity is minted or retired
**And** every V1 node lies within 0.10 m of the V2 elements associated with it by lineage, with decision-node smoothing by turns as the only exception category

**Given** a declared drivability profile copied from the vehicle profile
**When** any declared model is compiled
**Then** every element satisfies the single admission rule (steering speed ceiling ≥ 0.25 m/s, equivalently radius ≥ 4.0344 m), the channel-consistency rules C1–C5 and the fold rules F1–F3, or compilation fails naming the element, sample and rule
**And** the 5.25/5.26 fixtures, which are undeclared, compile to byte-identical canonical payloads

**Given** an undeclared model or a format-1 document
**When** it is loaded through `RoadModelDocument.Load`
**Then** it is refused

**Given** the corrected roundabouts and turns
**When** the overlay is reviewed
**Then** ring corridors and continuations lie on the exact circle fitted from the V1 ring nodes (each lineage-associated node within 0.10 m of it), entries and exits join the ring tangentially, and every turning footprint stays in its approach lane until its reference point reaches the near edge of the departure roadway (a boundary derived from corridor envelopes only)
**And** right-turn exception distances, anchor displacements, speed ceilings and seam discontinuities are published

**Given** the full-footprint candidate sweep
**When** candidates are generated for the 9 junctions
**Then** pairs are found across elements shorter than half a vehicle, ordinary following is published as following, and every zone stays owned by its junction

**Given** the exhaustive candidate and geometry diff
**When** it contains new, removed or materially changed pairs
**Then** work halts for owner decisions, no decision is written, transferred, deleted or approved by the agent, and the definitive 5.28 artifacts are regenerated only after every pair is resolved

**Given** a decision whose pair geometry changed
**When** Gate A is evaluated
**Then** the decision is shown as a historical proposal in the targeted review, and Gate A stays closed until the owner reconfirms or changes it

---
````

### 4.2 `epics.md` — new Story 5.51 (Proposal 2, approved; inserted after 5.50, before 5.29)

````markdown
### Story 5.51: Junction Corner Clearance for Right Turns

**Type:** FOUNDATION · **Boundary:** Physical district geometry (junction corners) · **Complexity:** M
**Implements:** AD-36 (physical-only exception, V1 data untouched), 2026-09-25 course correction

As a solo developer,
I want the physical corners of the crossroads and T junctions of `MVP_Run` to leave room for a right-turning vehicle,
So that the right-turn movements corrected in 5.50 can be driven without the vehicle body crossing a sidewalk or curb.

**Capability delivered:** a minimal physical cut on the 12 corners used by right turns, made through instance overrides on the 5 junction instances; a per-corner and per-movement physical clearance proof against the final 5.50 curves; a V1 regression before and after, with every delta reviewed by the owner.

**Why here:** measured on 2026-09-25, no right-turn curve inside the 6 m available at a crossroads or T corner keeps the maximum footprint off the square sidewalk corner at the admission radius. The best curvature-continuous candidate still overlaps by 0.12 m with zero margin. Only a physical change can make the right turns drivable, and Gate A must not sign turns the vehicle cannot physically make.

**Prerequisites:** 5.50 complete. The final right-turn curves are fixed, and the owner has resolved every conflict decision. The cut is sized on those curves.

**Evidence (2026-09-25):**
- corners today are 4 × 4 m square sidewalk colliders (`Col_Sidewalk_Corner_*`) whose road-side corner sits at (±4, ±4) m from the junction centre, plus curb colliders (`Col_Curb_*`) along them on the crossroads;
- the 12 corners used by right turns: the 4 corners of `Intersection_Center_Crossroads`, and the 2 stem-side corners of each of the 4 `TJunction_*` instances;
- current right turns overlap the corner by 0.63–0.67 m;
- an exact 6 m arc still overlaps by 0.69 m;
- the smallest 45° cut that clears the footprint with the 0.25 m margin has legs of 0.56–1.50 m, depending on the final curve (estimated with the maximum footprint and the versioned margin).

**Decisions recorded by the owner (2026-09-25):**
- the change is made through `MVP_Run` instance overrides on the 5 junction instances. Modifying a prefab remains **Ask First**. `Greybox_Intersection` has only this one instance; `Greybox_TJunction` is shared by the 4 T instances;
- physical change only: V1 traffic data and topology are untouched, so the V1 source hash and the 5.27 lineage stay byte-identical and no identity is minted or retired. This is a physical-world exception to the AD-36 freeze. It **does not imply V1 behavioural equivalence**;
- physical clearance evidence and the V1 regression before and after are mandatory;
- the 4 roundabouts are **not** in this story's scope: their physical evidence is refreshed by 5.28 on resumption through `RoundaboutClearance`.

**Clearance criterion (binding):**
- All inputs come from the versioned validation profile (`MaxVehicleHalfWidthMeters`, `MaxVehicleLengthMeters`, `LateralClearanceMarginMeters`), never from new constants.
- **Trajectories:** the compiled reference of every movement of the 5 junctions (right turns, left turns and straights), each extended by `MaxVehicleLengthMeters` / 2 + margin + δ_c onto its adjacent corridors, following the 5.50 connected-trajectory rule.
- **Footprint:** a rectangle of half-width `MaxVehicleHalfWidthMeters` + margin + δ_c and half-length `MaxVehicleLengthMeters` / 2 + margin + δ_c, centred on the reference point and aligned with the trajectory tangent (δ_c = the 0.05 m compiled-curve gate, covering every curve the 5.50 gates admit).
- **Vertical range (explicit):** from the drivable surface under the trajectory (the top of the roadway colliders, found by a downward query) up to the AI vehicle's collider top (1.44 m above its origin, from the `Greybox_AIVehicle` `BoxCollider`; pinned by a test like the steering parameters).
- **Obstacles (explicit):** every non-trigger collider without a non-kinematic `Rigidbody` whose volume intersects that vertical range within reach of the swept footprint. Colliders the vehicle drives on have no volume above the drivable surface, so they drop out with no name rule.
- **Supported shapes:** `BoxCollider`s turned only about the vertical, and convex `MeshCollider`s. Any other shape or orientation within reach is a hard failure.
- **Plan projection (conservative):** each obstacle is projected onto the plan as a convex polygon: the exact rectangle for a box, the full hull projection for a convex mesh. A convex-mesh projection can only overestimate the obstacle.
- **Poses:** the canonical `RoadCurve` evaluation (`Interpolate`) of each compiled reference, at sub-steps of at most h metres inside each compiled segment. Every compiled sample is itself a pose, so no interval between consecutive poses straddles a sample. The plan heading of a pose is the direction of the horizontal part of its evaluated tangent.
- **Distance:** the exact distance between the footprint rectangle and each obstacle polygon (convex separation) at every pose.
- **Interval bound, derived from the canonical evaluation (no false positive).** Inside one compiled segment, `Interpolate` moves the position linearly and returns the tangent as the normalized linear interpolation of the two sample tangents. Normalizing does not change a direction, so the plan heading is the direction of the linear interpolation of the two horizontal sample tangents. It turns monotonically, with no overshoot, through the angle between them.
  - Between two consecutive poses a and b of one segment, the reference point moves along the straight segment from p_a to p_b, and the heading turns monotonically through Δθ, the angle between their evaluated headings.
  - Every footprint point lies at most ρ from the reference point, where ρ is the distance to the farthest inflated corner. Turning by an angle φ therefore moves it by at most ρ·φ.
  - At any intermediate pose, every footprint point is therefore within D_a of its position at a and within D_b of its position at b. Because the translation and the rotation both split additively along the interval, D_a + D_b ≤ δ = |p_b − p_a| + ρ·Δθ.
  - Since the obstacle distance is 1-Lipschitz, the intermediate footprint is at least d_a − D_a and at least d_b − D_b from the obstacle. The smaller of D_a and D_b is at most δ/2, so the intermediate footprint is at least **min(d_a, d_b) − δ/2** from the obstacle.
  - δ is computed from the evaluated poses, never from the stored curvature channel, so the bound holds whatever the relation between stored curvature and the actual turning rate of the normalized interpolation.
- **Degenerate segment (hard failure):** a segment whose horizontal sample tangents are antiparallel within the existing `UnitTolerance`, or whose horizontal tangent is null. There the interpolated tangent can vanish and `Interpolate` falls back to the chord direction, which is a heading jump inside the segment.
- **Seams (explicit):** at every seam of the connected trajectory (corridor → movement → corridor, including the extensions), the two sides are separate compiled references. The last pose before the seam and the first pose after it are both evaluated, and the seam is an explicit interval with δ_seam = |p_after − p_before| + ρ·Δθ_seam. These are the published seam gap and heading jump; the jump is taken the shorter way, which C3 limits to well under 180°. This bounds a footprint that crosses the gap while turning through the jump. The vehicle's actual path through a seam heading jump is a tracking deviation, covered by ε_t (5.31), not by this bound.
- **Residual:** the minimum, over every interval (segment sub-steps and seams), of min(d_a, d_b) − δ/2. It is a lower bound on the true minimum distance of the continuously swept footprint for any choice of sub-steps; a finer step is not claimed to tighten it. It must be **strictly positive**.
- **Published** per corner and per movement: the minimum residual, the location, the obstacle and whether the minimum falls on a seam interval; h; the δ of that interval; and the largest δ.
- The residual is evidence alongside the target, never a way to shrink it.
- **Physical evidence fingerprint:** the clearance run also publishes, separately from its results, the canonical fingerprint of the physical inputs within reach of the 5 junctions, as defined for Gate A in 5.28: serialized authoring values (`GlobalObjectId`, the local transform of every ancestor, type and geometric properties, `MeshCollider` mesh content and cooking options), participation properties (`Collider.enabled`, `GameObject.activeInHierarchy`, `isTrigger`, layer, `includeLayers`/`excludeLayers`, collision with the AI vehicle's layer in the physics layer matrix, attached `Rigidbody` kind) and the version of the measurement code. The fingerprint describes the physical world only. It never enters `RoadModelVersion`, the V1 source hash or the lineage. 5.28 binds the Gate A sign-off to it, together with the refreshed roundabout evidence.

**Authoring facts (2026-09-25):**
- Each corner collider is a separate `BoxCollider` (`Col_Sidewalk_Corner_*`, 4 × 0.2 × 4 m). The crossroads also have curb `BoxCollider`s (`Col_Curb_*`); the T junctions have none.
- Each corner visual is the Synty mesh `SM_Env_Sidewalk_Corner_01`. It is licensed, git-ignored and shared by `Greybox_CityBlock_A`, `Greybox_Intersection` and `Greybox_TJunction`. It can be neither modified nor derived, and no derivative can be committed.
- The `MVP_Run` overrides observed so far are property modifications and added components, both used by 5.49. Added or removed GameObjects are supported by the Editor but have never been used in this project.
- Resizing a `BoxCollider` cannot produce a chamfer.

**Candidate construction (binding only once step 1 has verified it):**
- **Collider:** the chamfered corner is reproduced **exactly** by the union of three boxes: the original corner box, resized by a property override; one added box; one added box turned 45° about the vertical on an added child GameObject, with one face on the chamfer line. For a chamfer leg c, the construction is exact as long as c ≤ 8/3 m; the expected range is 0.56–1.50 m. The crossroads curbs are handled the same way.
- **Visual:** it must match the collider. The candidate is to deactivate the Synty corner visual by override and add project-owned greybox primitives (Unity's built-in cube mesh with a project material) matching the collider union, with no visible artefact where their surfaces overlap.
- **Alternatives, if the owner prefers:** an existing Synty piece as a visual child, if one matches; or a new project-owned mesh asset under `Assets/RoadRage`. Modifying a shared prefab, a prefab variant, or any shared or Synty asset stays **Ask First**.

**Step 1 — feasibility, before any sizing (HALT on failure).** In the Editor, under the double state guard, on one corner:
- author the candidate collider union and visual through instance overrides;
- save, reload the scene, and verify that the overrides serialize into `MVP_Run.unity` and survive the reload;
- verify that `git diff` touches only `MVP_Run.unity`, plus any new project-owned asset if the owner chose one, and that every prefab and shared or Synty asset is byte-identical;
- verify that the measured collider union equals the intended shape, and that the visual shows no artefact.

If any check fails, or if the visual cannot match cleanly, the story **stops and presents the alternatives to the owner**.

**Remaining design points settled in-story (after step 1):**
- the cut's shape and size: the minimal shape giving a strictly positive residual for every movement of the junction;
- the curb colliders that run into the cut;
- rebaking the `MVP_Run` NavMesh.

**Non-goals:**
- no V2 geometry change. If no physical cut clears a 5.50 curve, the story stops and returns to the owner; curves are never reshaped silently;
- no roundabout measurement or change (5.28 refreshes the roundabout evidence);
- no `LaneNode` or V1 traffic data change;
- no prefab modification without Ask First;
- no Gate A signature.

**Must NOT be copied:**
- clearing the corner by moving lane nodes or bending V2 curves;
- a residual computed with a margin or footprint other than the versioned profile's;
- a chamfer "made" by resizing a `BoxCollider`; a modified, derived or committed Synty mesh;
- an unchanged source hash offered as proof of V1 behavioural equivalence.

**EditMode verification:**
- the V1 source hash and lineage match the pre-change values captured at the start of this story, and no identity is minted or retired;
- `RoadModelVersion` and the conflict candidate set are unchanged, since no V2 data changed;
- every corner has a strictly positive residual for every movement of its junction, published;
- golden examples of the clearance function: today's square corner against the final 5.50 right turn is negative; the corrected corner is positive;
- **clearance soundness (adversarial):** synthetic sweeps in which the footprint meets an obstacle only between two evaluated poses, never at a pose, must never produce a strictly positive residual. Each case runs with the sub-step h as coarse as the case allows:
  - a rotation-dominated segment: a short chord with a large heading change, where an inflated corner swings through the obstacle;
  - a straight segment with a thin obstacle between two poses;
  - a seam heading jump that swings a corner through an obstacle;
  - a segment whose stored curvature channel is far below its evaluated turning. The former κ_max bound would report this case as clear; the evaluated bound must not.
  In every case and on a grid of placements, the residual is ≤ the minimum distance obtained by dense evaluation of the canonical interpolation. A segment with antiparallel horizontal tangents is a hard failure;
- the vehicle height and obstacle filter are pinned: a sidewalk collider in the vertical range counts, a roadway collider does not, a trigger does not, a tilted box within reach is a hard failure;
- an unsupported collider within reach is a hard failure;
- the physical input fingerprint changes when a relevant collider moves, is disabled (`Collider.enabled`), becomes a trigger, or leaves the vehicle's collision layers, and does not change for a collider out of reach.

**V1 regression verification (required, not optional):**
- The baseline is captured **before** any prefab or scene change, on the complete V1 oracle and regression suite:
  - the full EditMode suite, including `TrafficOracle` and the V1 traffic stories' tests;
  - the V1 traffic PlayMode suites (`Story510RoutedTrafficPlayModeTests`, `Story57…`, `Story59…`, `Story515…`).
- The same suites run again after the change, in a fresh Editor session.
- Every behavioural delta is **reported for owner review** and never accepted on the strength of the unchanged source hash: a changed result, a newly failing or newly passing test, or a changed oracle trace.
- The PlayMode runner works once per Editor session, so the runs before and after need two Editor restarts.

**PlayMode verification:** the V1 regression above; a visual check of the 5 junctions in the Editor, under the double state guard.

**Completion evidence:**
- the step-1 feasibility record (overrides used, reload check, `git diff` scope) and the physical input fingerprint;
- the V1 regression results before and after, with every delta dispositioned by the owner;
- the per-corner and per-movement residual table;
- the list of instance overrides;
- the NavMesh rebake.

**Unlocks:** 5.28 resumes at its HALT: roundabout evidence refreshed, physical binding of Gate A, re-review of the 9 junctions and of the segments whose V2 geometry changed in 5.50, then the owner's signature (Gate A).

**Acceptance Criteria:**

**Given** one corner authored with the candidate collider union and visual through `MVP_Run` instance overrides
**When** the scene is saved and reloaded
**Then** the overrides survive, only `MVP_Run.unity` (plus any owner-approved project-owned asset) changes, every prefab and shared or Synty asset is byte-identical, and the collider union matches the intended shape
**And** otherwise the story stops and presents the alternatives to the owner

**Given** the final 5.50 compiled references and the instance overrides on the 5 junction instances
**When** the maximum footprint, inflated by the versioned margin and δ_c, is swept conservatively along every movement and its reach onto adjacent corridors, against every obstacle collider within the explicit vertical range
**Then** the residual (the minimum over all pose intervals, seams included, of the smaller end-pose distance minus half the interval's displacement bound δ = |Δp| + ρ·Δθ, computed from the canonical evaluation) is strictly positive and published per corner and per movement, with the physical input fingerprint published separately

**Given** synthetic sweeps in which the footprint meets an obstacle only between two evaluated poses (rotation-dominated segment, straight segment, seam heading jump, stored curvature below the evaluated turning)
**When** the clearance residual is computed
**Then** it is never strictly positive, it never exceeds the minimum distance found by dense evaluation, and a segment with antiparallel horizontal tangents is a hard failure

**Given** the corner change
**When** the V1 source set is re-extracted and the V2 pipeline runs
**Then** the V1 source hash and the 5.27 lineage are byte-identical, no identity is minted or retired, and `RoadModelVersion` and the conflict candidates are unchanged

**Given** the physical change
**When** the complete V1 oracle and regression suite runs against its pre-change baseline
**Then** every behavioural delta is reported for owner review and none is accepted on the strength of the unchanged source hash

**Given** that no physical cut clears a 5.50 curve with a positive residual
**When** the story evaluates its options
**Then** it stops and returns to the owner without modifying V2 geometry or any shared prefab

---
````

### 4.3 `epics.md` — Story 5.28, program plan and downstream obligations (Proposal 3, approved)

**Gate A physical binding — design (implemented by 5.28 on resumption):**

- **Scope:** the 9 junctions, measured against the **final 5.50 compiled references**, each extended by `MaxVehicleLengthMeters` / 2 + margin + δ_c onto adjacent corridors, with the conservative swept-footprint method of 5.51 (versioned footprint and margin, inflated by δ_c; the vertical range and obstacle filter of 5.51). The 5.49 measurements, made against the previous V2 geometry, are **not reused**; the 5.49 two-footprint ring residual is recomputed on the final ring. Corner evidence of the 5 conventional junctions comes from 5.51. **5.28 extends `RoundaboutClearance`** to recompute the 4 roundabouts on the final 5.50 trajectories with the same sweep (reusing 5.51's sweep function), transitions to adjacent corridors and the recomputed ring residual included. A non-positive roundabout residual **halts 5.28 for an owner decision**; neither 5.51's scope nor any physical geometry changes without that decision.
- **Two separate records:** (1) the **physical input fingerprint** — deterministic canonical serialization of serialized authoring values within reach of the 9 junctions: per collider its `GlobalObjectId`, the serialized local transform of every ancestor, type and geometric properties (box centre and size; `MeshCollider` convex flag, cooking options and a content hash of vertices and indices), participation properties (`Collider.enabled`, `GameObject.activeInHierarchy`, `isTrigger`, layer, `includeLayers`/`excludeLayers`, collision with the AI vehicle's layer in the physics matrix, attached `Rigidbody` kind), plus the clearance measurement code version, written exactly as serialized; (2) the **clearance results** — recomputed residuals per junction, corner and movement, and the recomputed ring residuals; evidence, not identity. Every piece of Gate A evidence (conflict candidates and clearance) records the **lateral tracking allowance a_e** it was computed for. It is 0 for the evidence first signed at Gate A: the margin is reserved and δ_c is spent on representation.
- **Sign-off** binds `physical-input-hash` and the clearance results; neither enters `RoadModelVersion`, the V1 source hash or the lineage.
- **Evaluation** recomputes both on the open `MVP_Run` under the scene guard. Gate A stays open only if: the fresh fingerprint equals the signed one exactly (any relevant geometry or participation change closes Gate A, naming the collider or property); every fresh residual is strictly positive (a clearance regression closes Gate A); the fresh residuals match the signed ones under the **reproducibility rule** — exact comparison if bit-identical reproducibility across a scene reload and a fresh Editor session is demonstrated by test; otherwise 5.28 halts and presents the observed spread to the owner, who sets the comparison bound.

**Gate table — OLD:**
`| **A — `MVP_Run` Road Model Migration Validated** | 5.28 (sign-off after 5.49) | The **data** gate, not the design gate: the real `MVP_Run` V1 authoring has been imported, compiled, measured, validated and visually reviewed as V2 data — zero hard errors, every source item disposed, human-signed overlay | EditMode + overlay sign-off |`

**Gate table — NEW:**
`| **A — `MVP_Run` Road Model Migration Validated** | 5.28 (sign-off after 5.49, 5.50 and 5.51) | The **data** gate, not the design gate: the real `MVP_Run` V1 authoring has been imported, compiled, measured, validated and visually reviewed as V2 data — zero hard errors, every source item disposed, every declared element drivable, human-signed overlay bound to the model hashes and to the reviewed physical clearance evidence | EditMode + overlay sign-off |`

**Dependency graph:** the main line becomes `5.25 ─► 5.26 ─► 5.27 ─► 5.28 ─► 5.49 ─► 5.50 ─► 5.51 ─► 5.28✓ ═GATE A═ ─► 5.29 ─► 5.30 ─► 5.31 ═GATE B═`; every line below it shifts right by 16 columns and the 5.24 connector grows by 16 `─`, keeping every junction symbol aligned (generated from the current block).

**Footnote — NEW:**
> `5.28✓` — the 5.28 authoring pipeline is delivered before 5.49; its overlay re-review and owner signature resume after 5.49, 5.50 and 5.51 and close Gate A. 5.49 (2026-09-23 course correction) and 5.50–5.51 (2026-09-25 course correction) are numbered after 5.48 because 5.17–5.23 are burned and 5.24–5.48 were taken when the need arose; their position in the chain, not their number, sets their order.

**Critical path:** `… 5.28 → 5.49 → 5.28 sign-off → 5.29 …` → `… 5.28 → 5.49 → 5.50 → 5.51 → 5.28 sign-off → 5.29 …`

**Story index — two rows after 5.49:**
```markdown
| 5.50 | V2 movement geometry correction and model-level drivability validation *(inserted 2026-09-25, precedes the Gate A sign-off)* | FOUNDATION | V2 road geometry / model validation | L |
| 5.51 | Junction corner clearance for right turns *(inserted 2026-09-25, precedes the Gate A sign-off)* | FOUNDATION | Physical geometry (junction corners) | M |
```

**Story 5.28 section:**
- Prerequisites — `**Sign-off only:** 5.49 — the 2026-09-23 overlay review blocked the four roundabouts; the authoring pipeline is delivered, the HALT resumes after 5.49.` → `**Sign-off only:** 5.49, 5.50, 5.51 — the 2026-09-23 overlay review blocked the four roundabouts (5.49); the 2026-09-24 review found undrivable movement geometry (5.50) and right-turn corner conflicts (5.51); the authoring pipeline is delivered, the HALT resumes after 5.51.`
- Non-goals — after "…is Story 5.49", append `; the V2 movement geometry correction is Story 5.50 and the junction corner clearance is Story 5.51`.
- Reusable V1 components — `(5.49 widens the roundabouts physically, V1 traffic data untouched)` → `(5.49 widens the roundabouts and 5.51 cuts the junction corners physically, V1 traffic data untouched)`.
- Artifacts — `…the sign-off record carrying approver identity and overlay artifact hash.` → `…the sign-off record carrying approver identity, overlay artifact hash, the model binding hashes, the physical input fingerprint and the clearance results (9 junctions: roundabouts refreshed in 5.28, corners from 5.51).`
- EditMode verification — appended after "…zero undisposed source items": `; the model declares its drivability profile and every element passes the 5.50 admission, consistency and fold rules; no conflict decision on materially changed pair geometry is counted until the owner reconfirms it; the Gate A evaluation recomputes the physical input fingerprint and the clearance results for the 9 junctions against the final 5.50 trajectories, and refuses a sign-off whose fingerprint differs, whose fresh residuals are not all strictly positive, or whose residuals differ under the reproducibility rule, naming the cause`.
- Completion evidence — `**Gate A** (after 5.49) — migration report at zero hard errors and zero undisposed items, reviewed overlays for all 25 module instances, and a sign-off recording approver identity and overlay artifact hash bound to the same source, import-map, compiler and model hashes.` → `**Gate A** (after 5.49, 5.50 and 5.51) — migration report at zero hard errors and zero undisposed items, every declared element drivable, reviewed overlays for all 25 module instances with a re-review of the 9 junctions and of every segment whose V2 geometry changed, every conflict decision confirmed by the owner, and a sign-off recording approver identity and overlay artifact hash bound to the same source, import-map, compiler and model hashes and to the physical input fingerprint and the recomputed clearance results of the 9 junctions.`
- Gate A acceptance criterion — two lines appended:
```markdown
**And** the sign-off is also bound to the physical input fingerprint and the clearance results of the 9 junctions, recomputed on the final 5.50 trajectories, neither of which is part of `RoadModelVersion`, the V1 source hash or the lineage
**And** any later change to the relevant collision geometry or participation, or any clearance regression, closes Gate A until re-review and re-signature
```

**Story 5.30:**
- Prerequisites — `5.26, 5.28, 5.29.` → `5.26, 5.28, 5.29; 5.50 (declared drivability profile, steering speed ceiling).`
- New acceptance block, before "Given this story's implementation":
```markdown
**Given** a runtime road model
**When** the planning spine is constructed
**Then** the model comes only from `RoadModelDocument.Load`, which refuses a model without a declared drivability profile
**And** the compiled reference — the canonical `RoadCurve` evaluation of the compiled samples — is the single authoritative trajectory; every containment proof is inflated by the compiled-curve gate δ_c; no layer produces an alternative reference by re-fitting, re-smoothing or re-sampling it into a different curve
**And** `LateralClearanceMarginMeters` is reserved clearance, never consumed by tracking or planning; a planned trajectory is covered by the valid Gate A evidence only if max |o(s)| + ε_t ≤ the tracking allowance a_e recorded with that evidence (ε_t counts as 0 until the driving story declares it); a plan that is not covered is infeasible until the evidence is regenerated and Gate A re-signed under the lifecycle defined in the Road World Model contract (§8)
**And** the path and motion contracts carry the steering speed ceiling v*(s) along the reference, and a motion plan whose speed profile exceeds it anywhere is infeasible
```

**Story 5.31:**
- Free-road block — `**Then** it combines desired speed, road limit and curve limit as named constraints and identifies the binding one` → `**Then** it combines desired speed, road limit, curve limit and the steering speed ceiling as named constraints and identifies the binding one`.
- New acceptance block, after the first block:
```markdown
**Given** the first driven V2 slice
**When** a vehicle is spawned on the V2 path
**Then** its road model was admitted through `RoadModelDocument.Load` with a declared drivability profile, and a PlayMode test proves an undeclared model drives no vehicle
**And** the planned speed profile respects the steering speed ceiling locally: v(s) ≤ v*(s) at every point of the planned trajectory, reached with deceleration within the speed planner's declared deceleration bound, starting early enough before each tighter curve — not a single cap equal to the lowest ceiling anywhere in the look-ahead
**And** when the ceiling cannot be met from the current state, the plan declares that infeasibility as its binding constraint and brakes at the declared bound
**And** the observed speed at each traversed position is published against v*(s), and any exceedance fails the test
**And** the tracking tolerance ε_t is declared as a bound and measured on test runs of the first driven slice; any observed lateral deviation beyond the declared ε_t fails the test
**And** if every planned trajectory of the slice is covered by the valid Gate A evidence (max |o(s)| + ε_t ≤ its recorded allowance a_e), the Gate A signature stays valid and the coverage check is published; since the evidence first signed at Gate A has a_e = 0, any ε_t > 0 takes the next branch
**And** otherwise the conflict candidates and the physical clearance are regenerated with the allowance max |o(s)| + ε_t, and the candidate diff against the signed set is published. The owner decides every new pair, reconfirms or changes every materially changed pair and disposes of every orphaned decision; the agent never does. Any non-positive residual fails. Gate A is re-reviewed on what changed (the candidate diff and the regenerated clearance results) and re-signed on a new sign-off record bound to the regenerated evidence, the previous record kept as superseded history
**And** outside the measurement test runs, no vehicle drives a V2 trajectory that the valid evidence does not cover, a test proves the refusal, and 5.31 is not complete until coverage holds
```

**Story 5.33:** `**Then** following is one named constraint among desired speed, road limit and curve limit, and the plan identifies which constraint binds` → `**Then** following is one named constraint among desired speed, road limit, curve limit and the steering speed ceiling, and the plan identifies which constraint binds`.

**Test routing — two rows after 5.49:**
```markdown
| 5.50 | EditMode + Editor overlay/isolation check under the double state guard | `edge-case-hunter`, `verification-gap` — dense geometry and validation logic, measurement and drivability claims |
| 5.51 | EditMode + V1 PlayMode regression before/after (two fresh Editor sessions) + Editor check under the double state guard | `edge-case-hunter`, `verification-gap` — clearance soundness and V1 behavioural-delta disposition |
```

**Consistency rows:**
- AD-36 — stories `5.24, 5.31, 5.49, 5.48` → `5.24, 5.31, 5.49, 5.51, 5.48`; text "5.49 changes the shared roundabout prefab physically only" → "5.49 (the `Greybox_Roundabout` prefab asset, used only by the 4 roundabout instances, plus scene-instance overrides on the 8 `Ring_*` and 4 `TunnelPortal_*` segments) and 5.51 (junction corners, scene-instance overrides on the 5 junction instances) change physical geometry only".
- AD-45 — stories add 5.50; text appended `; 5.50 requires every declared element to be drivable (vehicle-derived admission rule, channel consistency, fold rules) and makes the compiled reference the single driving authority`.
- AD-47 — stories `5.27, 5.49, 5.28` → `5.27, 5.49, 5.50, 5.51, 5.28`; text `Gate A is the ten-point report plus signed overlay` → `Gate A is the ten-point report plus signed overlay, bound to the model hashes and to the physical clearance evidence`.

**Risk R1 — sentence appended:** `The 2026-09-24 review added 5.50 and 5.51 for the same reason: the overlay exposed undrivable importer geometry and square junction corners before any vehicle could learn them.`

### 4.4 Specs (Proposal 4, approved; French, frozen blocks unchanged)

#### 4.4.1 `spec-5-28-junction-semantic-authoring-and-overlay-sign-off.md` (status stays `in-progress`)

**HALT — fourth sub-bullet:**
```markdown
  - Revue du 2026-09-24 et correct-course du 2026-09-25 (sprint-change-proposal-2026-09-25.md) : mouvements de giratoire non conduisibles (rayon 0,25 m, crochets), virages a gauche amorces au bord du carrefour, virages a droite en conflit avec les angles de trottoir carres. Stories 5.50 (geometrie V2 et validation de conduisibilite au niveau modele) et 5.51 (degagement physique des angles) inserees avant la signature. Reprise du HALT apres 5.51 : liaison physique de la Gate A (preuve des angles des 5 carrefours classiques par la 5.51, preuve des 4 giratoires recalculee par la 5.28 via `RoundaboutClearance`), artefacts definitifs de la 5.50, nouvelle revue des 9 carrefours et des segments dont la geometrie V2 a change, puis signature.
```

**Two new unchecked tasks, before the HALT:**
```markdown
- [ ] `.../Traffic/Migration/AuthoredRoadModel.cs`, `.../Traffic/Migration/RoundaboutClearance.cs`, `.../Traffic/Migration/GateAReviewWindow.cs` -- liaison physique de la Gate A (correct-course 2026-09-25). Preuve des 9 carrefours : angles des 5 carrefours classiques (croix et T) = resultats de la 5.51 ; 4 giratoires = `RoundaboutClearance`, que la 5.28 etend pour recalculer sur les trajectoires finales 5.50, transitions vers les corridors adjacents incluses, le balayage conservateur de l'empreinte (fonction de la 5.51 reutilisee, gabarit et marge du profil versionne, gonfles de la tolerance de corde compilee) et le residu d'anneau a deux gabarits. Un residu de giratoire non positif = HALT pour decision du proprietaire, sans changement physique. Empreinte des entrees physiques, construite sur les valeurs d'authoring serialisees : `GlobalObjectId`, transforms locaux de toute la chaine, type et proprietes geometriques, contenu du maillage (sommets, indices, options de cuisson) des `MeshCollider`, participation (`Collider.enabled`, `GameObject.activeInHierarchy`, `isTrigger`, couche, `includeLayers`/`excludeLayers`, collision avec la couche du vehicule IA dans la matrice physique, genre de `Rigidbody`), version du code de mesure. Rapport Gate A : `physical-input-hash` et resultats. Sign-off lie aux deux, hors `RoadModelVersion`, hash source et lignee. Evaluation : empreinte identique, residus frais strictement positifs, regle de reproductibilite (comparaison exacte si la reproductibilite bit a bit est demontree par test apres rechargement de scene puis nouvelle session d'Editeur ; sinon HALT, ecart presente au proprietaire qui fixe la borne). Toute decision de conflit non reconfirmee (5.50) compte comme ouverte.
- [ ] `Assets/RoadRage/Tests/EditMode/Story528AuthoringAndGateATests.cs` -- Gate A fermee, cause nommee, si l'empreinte physique differe (collider deplace, `Collider.enabled` ou `isTrigger` bascule, maillage modifie, couche exclue de la matrice), si un residu frais -- angle ou giratoire -- n'est pas strictement positif, si les residus different sous la regle de reproductibilite, ou si une decision n'est pas reconfirmee ; ouverte sinon, apres signature ; un changement de collider hors de portee des 9 carrefours ne ferme pas la Gate A.
```

**Code Map — two entries:**
```markdown
- `.../Traffic/Migration/RoundaboutClearance.cs` -- mesure 5.49 des 4 giratoires ; la 5.28 l'etend a sa reprise : balayage conservateur sur les trajectoires finales 5.50 (transitions vers les corridors adjacents incluses), residu d'anneau recalcule, publication des colliders mesures (empreinte physique).
- Outil de degagement des angles de la 5.51 (nom fixe par la 5.51) -- preuve des angles des 5 carrefours classiques ; sa fonction de balayage conservateur est reutilisee par `RoundaboutClearance`.
```

**Design Notes — Candidats, last sentence:** `` `ponytail:` balayage lateral seul, la longueur du gabarit au-dela des extremites n'est pas balayee (les coutures relevent du suivi) ; a etendre si la 5.34 mesure un conflit manque. `` → `Balayage lateral seul jusqu'a la 5.50, qui le remplace par l'empreinte complete le long des trajectoires dirigees connectees (elements plus courts qu'un demi-vehicule inclus, suivi ordinaire publie comme suivi, zone propre a son carrefour, paire nouvelle, retiree ou modifiee soumise au proprietaire).`

**Design Notes — Liaison, sentence appended:** `Gate A (correct-course 2026-09-25) : le rapport publie aussi \`physical-input-hash\` et les resultats de degagement des 9 carrefours ; le sign-off les lie ; ni l'un ni l'autre n'entre dans \`RoadModelVersion\`, le hash source ou la lignee.`

**Spec Change Log — new entry:**
```markdown
- **2026-09-25 -- correct-course proprietaire (sprint-change-proposal-2026-09-25.md).**
  Declencheur : revue d'overlay du 2026-09-24 -- mouvements de giratoire non conduisibles (rayon 0,25 m, crochets), virages a gauche amorces au bord du carrefour, virages a droite en conflit avec les angles de trottoir carres.
  Amende : signature apres 5.49, 5.50 et 5.51 ; la Gate A lie aussi l'empreinte des entrees physiques et les resultats de degagement des 9 carrefours -- angles des 5 carrefours classiques par la 5.51, 4 giratoires par `RoundaboutClearance` recalcule par la 5.28 sur les trajectoires finales 5.50 ; une decision de conflit dont la geometrie de paire a change ne compte qu'apres reconfirmation du proprietaire (5.50) ; candidats balayes a empreinte complete (5.50).
  Etat evite : signer des trajectoires de reference non conduisibles, ou une Gate A que la geometrie physique invaliderait sans bruit.
  KEEP : bloc gele inchange -- decisions seules authoritative, candidat = croisement ou convergence d'enveloppes balayees du gabarit max (le suivi ordinaire n'en est pas un), tests 5.25/5.26/5.27 inchanges, signature humaine par menu.
```

#### 4.4.2 `spec-5-27-v1-importer-validator-and-migration-report.md` (stays `done`; frozen block untouched)

**Design Notes, start of `**Geometrie.**`:** `*(Remplacement partiel approuve le 2026-09-25, a realiser par la Story 5.50 -- voir Spec Change Log.)*`

**New section before `## Design Notes`:**
```markdown
## Spec Change Log

- **2026-09-25 -- renegociation approuvee par le proprietaire (sprint-change-proposal-2026-09-25.md), a realiser par la Story 5.50.**
  Declencheur : diagnostic du 2026-09-24 -- anneaux de giratoire construits a travers des noeuds espaces d'environ 37 degres (tangentes d'extremite fausses de 16 a 18 degres) ; entrees et sorties impossibles a raccorder tangentiellement sur `Ring_Merge`/`Ring_Split` (7,5 degres en amont du croisement de l'axe de voie) ; virages amorces au bord du carrefour.
  Sera remplace (Design Notes, **Geometrie**) : « Mouvement = Hermite cubique dense... », « Corridors par `RoadCurveBuilder` a travers leurs noeuds » pour l'anneau, et l'egalite fin de corridor = position du noeud V1. Par : les exigences de la 5.50 -- cercle exact ajuste sur les noeuds V1 d'anneau, chaque noeud associe par lignee a 0,10 m au plus du cercle ; raccords tangents avec ancrages deplaces et publies ; virages a courbure continue echantillonnes analytiquement ; derive de 0,10 m mesuree contre les elements de lignee du noeud.
  Inchange : bloc gele -- effectifs, regles de cle de lignee, seule categorie d'exception (noeuds de decision lisses par les virages), serialisation deterministe, ecriture fail-closed ; tests 5.27 inchanges. `ImporterVersion` passera a 2 et le rapport 5.27 sera regenere par la 5.50.
```

#### 4.4.3 `spec-5-49-roundabout-ring-widening-and-applied-reviewed-widths.md` (stays `done`; frozen block untouched)

**Right after `</frozen-after-approval>`:** `*Renegociation approuvee le 2026-09-25 (invariant d'axe d'anneau), a realiser par la Story 5.50 -- voir Spec Change Log. Le bloc gele ci-dessus reste l'etat livre par la 5.49.*`

**New section before `## Design Notes`:**
```markdown
## Spec Change Log

- **2026-09-25 -- renegociation approuvee par le proprietaire (sprint-change-proposal-2026-09-25.md), a realiser par la Story 5.50 (axe) et par la Story 5.28 a sa reprise (preuve physique des giratoires).**
  Invariant gele renegocie (Always) : « l'axe importe/compile (representation cordes + Hermite, mesure ~5,83-6,19 m du centre) est la geometrie de reference acceptee et reste INCHANGE ». Remplacement approuve : l'axe de l'anneau deviendra le cercle exact ajuste sur les noeuds V1 d'anneau (rayon 6,0 m), chaque noeud associe par lignee a 0,10 m au plus du cercle ; les noeuds ne bougent pas.
  Conditionnel (matrice, `EndpointInterpolation` 2,0 -> 4,0 m sur les entrees et sorties) : conserve, sauf echec des regles de repli de la 5.50 sur une entree ou une sortie ; une autre loi de largeur exigera alors une decision du proprietaire.
  Preuve physique : les mesures 5.49 (residus a deux gabarits, rayons d'ilot et de chaussee) ont ete faites sur l'ancienne geometrie V2. `RoundaboutClearance` sera etendu par la 5.28 a sa reprise pour les recalculer sur les trajectoires finales 5.50 (balayage conservateur de l'empreinte, transitions vers les corridors adjacents incluses, residu d'anneau recalcule), et c'est cette preuve recalculee que liera la Gate A. Un residu non positif arretera le travail pour une decision du proprietaire.
  Inchange : geometrie physique et largeurs appliquees de la 5.49, cible contraignante (4,0 / 4,0 m, ilot <= 1,75 m, chaussee >= 10,25 m), hash source et lignee, un seul corridor d'anneau.
  Design Notes « L'axe ne change pas, donc les candidats de conflit non plus » : vrai pour la 5.49 seule ; avec la 5.50, les candidats changeront et passeront par le differentiel exhaustif et la reconfirmation du proprietaire.
```

### 4.5 Architecture artifacts (Proposal 5, approved)

#### 4.5.1 `traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md` (CRLF)

**Gate table — three rows after `| lateral vehicle clearance | … |`:**
```markdown
| declared drivability — steering admission | every sample of every element of a model that declares a drivability profile: steering speed ceiling ≥ minimum active steering speed, equivalently radius ≥ R_adm (4.0344 m for the current AI profile); both derived from the declared vehicle steering geometry, never set independently |
| declared drivability — channel consistency | reference trajectory (integral of the stored, linearly interpolated curvature) within the compiled-curve gate (0.05 m) of every position and chord; each chord direction inside its interpolated tangent range; stored tangent equal to the integrated heading (float noise only); constant road-up |
| declared drivability — envelope fold | runtime envelope edge never reverses (closed-form segment rule); inner half-width × curvature < 1 along the reference trajectory; both envelope edges within the seam gap tolerance at every seam |
```

**Paragraph after the table's existing explanatory paragraphs:**
```markdown
*Added 2026-09-25 (`planning-artifacts/sprint-change-proposal-2026-09-25.md`), owner-approved engineering targets, not measured V1 behaviour.* The three drivability rows apply to models that declare a drivability profile. Runtime admission (`RoadModelDocument.Load`) refuses undeclared models, so every driven model is gated; undeclared test fixtures compile unchanged. The authoritative representation of every element is the **compiled reference**: its compiled samples evaluated by the canonical `RoadCurve` evaluation (`Sample`, `Project` — positions interpolated linearly, tangents normalized-interpolated, curvature and widths interpolated linearly). For a declared model, the rows above prove that its tangent and curvature channels are exact samples of a curvature-continuous witness curve, and that its positions and chords lie within the compiled-curve gate δ_c = 0.05 m of that witness; the witness exists only in the validator. Every containment proof — conflict candidates, overlays, physical clearance, planning feasibility — is computed on the compiled reference and inflated by δ_c, so it covers both the compiled reference and every curve the gates admit. Curvature and the steering speed ceiling come from the validated curvature channel. For a declared model the 5-degree seam tangent row is effectively stricter through the seam-edge rule; seam heading and curvature jumps are published, and their numeric tolerance is set by Story 5.30. The steering speed ceiling is a kinematic steering-authority limit: it ignores grip and tyre slip, which can only lower the real ceiling, and does not replace the grip-based curve limit.
```

**§8 Speed planning bullet:** `desired speed, road limit, curve limit, leader following, …` → `desired speed, road limit, curve limit, steering speed ceiling, leader following, …`.

**§8 — three bullets added after the three existing ones:**
```markdown
- **Reference trajectory (2026-09-25):** the compiled reference (see the gate table) is the single authoritative trajectory. Validator, conflict generation, overlays, physical clearance and planning all use it, with any containment proof inflated by δ_c. No consumer produces an alternative reference by re-fitting, re-smoothing or re-sampling it into a different curve.
- **Lateral quantities are distinct:** δ_c (representation allowance, already inside every proof); `LateralClearanceMarginMeters` (**reserved** clearance, never consumed by tracking or planning); the tracking tolerance ε_t (a bound on the controller's lateral deviation, declared by the driving story and verified by measurement, never assumed; an observed deviation beyond it is a failure); a deliberate planning offset o(s).
- **Gate A evidence lifecycle:** each piece of Gate A evidence (conflict candidates, physical clearance) records the lateral tracking allowance a_e it was computed for, which is 0 for the evidence first signed at Gate A.
  - A trajectory is **covered** when max |o(s)| + ε_t ≤ a_e along it. Its signature then stays valid and nothing is regenerated.
  - Otherwise the candidates and the clearance are regenerated with the allowance max |o(s)| + ε_t, and the candidate diff against the signed set is published. The owner decides new pairs, reconfirms or changes materially changed pairs and disposes of orphaned decisions. A non-positive residual is a failure. Gate A is re-reviewed on what changed and re-signed on a new record bound to the regenerated evidence, and the previous record is kept as superseded history.
  - No vehicle drives a trajectory that the valid evidence does not cover.
- **Steering speed ceiling:** the model publishes v*(s) along every element. Speed planning enforces it locally along the planned trajectory, decelerating within its declared bound early enough before each tighter curve, never as a single cap equal to the lowest ceiling in the look-ahead. When it cannot be met from the current state, the plan declares that infeasibility as its binding constraint.
```

#### 4.5.2 `ARCHITECTURE-SPINE.md` (CRLF)

**Wording correction in the 2026-09-23 paragraph (acceptance record unchanged):** `Story 5.49 widens the shared roundabout prefab.` → `Story 5.49 widens the roundabouts physically (as delivered: the \`Greybox_Roundabout\` prefab asset — island reduced to radius 1.5 m, paved disc \`Col_Roadway_Ring\` of radius 10.5 m — used only by the 4 roundabout instances, plus \`MVP_Run\` scene-instance overrides trimming or deactivating the sidewalks of the 8 adjacent \`Ring_*\` and 4 \`TunnelPortal_*\` segments; wording corrected 2026-09-25).`

**New paragraph after the 2026-09-23 paragraph, before `## Consistency Conventions`:**
```markdown
*2026-09-25 course correction (`planning-artifacts/sprint-change-proposal-2026-09-25.md`), accepted by the owner, to be delivered by Stories 5.50 and 5.51 and by 5.28 on resumption:* the 2026-09-24 overlay review found V2 movement geometry the AI vehicle cannot drive (roundabout entries and exits at a 0.25 m radius, a ring axis off its circle, turns starting at the junction boundary), and right turns whose footprint crosses square sidewalk corners. Story 5.50 is to correct the V2 geometry and to add a declared drivability profile validated at model level: steering parameters copied from the vehicle profile, one admission rule (steering speed ceiling ≥ minimum active steering speed), plus channel-consistency and envelope-fold rules. `RoadModelDocument.Load` is to refuse undeclared models. The compiled reference (the canonical `RoadCurve` evaluation, with every containment proof inflated by the compiled-curve gate) is to become the single authoritative trajectory. Story 5.51 is to cut the crossroads and T-junction corners through `MVP_Run` scene-instance overrides: **a physical-world exception to the AD-36 freeze on the same terms as 5.49** — V1 authored traffic data and topology unchanged, the V1 source hash and the 5.27 lineage byte-identical, the complete V1 oracle/regression suite re-run against a pre-change baseline, and every detected delta dispositioned by the owner. The Gate A sign-off is to be bound to the model hashes and to a physical input fingerprint with recomputed clearance results for the 9 junctions (corners by 5.51, roundabouts by `RoundaboutClearance` refreshed in 5.28); none of these enters `RoadModelVersion`. The frozen 5.49 ring-axis invariant is renegotiated to the exact circle fitted from the V1 ring nodes. No AD is reopened.
```

#### 4.5.3 `traffic-v2/V1-BEHAVIORAL-ORACLE.md` (LF)

**Wording correction of the 2026-09-23 line:** `the roundabout prefab's physical geometry — roadway, island, sidewalks, colliders — is widened while its V1 traffic data stays unchanged.` → `the roundabout prefab's roadway and island (and their colliders) are widened, and the sidewalks of the 12 adjacent segments are trimmed through \`MVP_Run\` scene-instance overrides, while V1 traffic data stays unchanged (wording corrected 2026-09-25).`

**New line after it, same bullet:**
```markdown
  *2026-09-25 exception (Story 5.51): the corners of the crossroads and T junctions — sidewalk and curb colliders and their visuals — are cut through `MVP_Run` scene-instance overrides while V1 traffic data stays unchanged. Oracle verdicts are re-run against a pre-change baseline; a behavioural delta is reported to the owner, not absorbed.*
```

### 4.6 Tracking (Proposal 6, approved)

#### 4.6.1 `sprint-status.yaml` (LF)

- Gate comment — `Gates: A after 5.28 (sign-off after 5.49), B after 5.31, C after 5.35,` → `Gates: A after 5.28 (sign-off after 5.49, 5.50, 5.51), B after 5.31, C after 5.35,`
- `5-49-roundabout-ring-widening-and-applied-reviewed-widths: review` → `done` (owner-confirmed 2026-09-25).
- After 5.49, before 5.29:
```yaml
  # Inserted 2026-09-25 (sprint-change-proposal-2026-09-25.md): the 2026-09-24
  # overlay review found undrivable V2 movement geometry (5.50) and right-turn
  # corner conflicts (5.51). Both run BEFORE the 5.28 sign-off, in this order.
  5-50-v2-movement-geometry-correction-and-model-level-drivability-validation: backlog
  5-51-junction-corner-clearance-for-right-turns: backlog
```
- 5.28 stays `in-progress`.

#### 4.6.2 `deferred-work.md` (CRLF with BOM)

**Existing entry "Bloquant Gate A -- revue d'overlay de la Story 5.28 (2026-09-23)", `summary` only (historical entry and `evidence` unchanged):**
`RESTE OUVERTE jusqu'a ce que : la 5.49 soit implementee, les artefacts 5.28 regeneres, les 4 giratoires revus a nouveau et la Gate A signee. Decisions du proprietaire : un seul corridor logique d'anneau ; axe au rayon 6,0 m inchange ;` → `5.49 LIVREE (confirmee done le 2026-09-25). RESTE OUVERTE jusqu'a ce que : les artefacts 5.28 soient regeneres apres 5.50 et 5.51, les 4 giratoires revus a nouveau et la Gate A signee. Decisions du proprietaire : un seul corridor logique d'anneau ; noeuds V1 d'anneau au rayon 6,0 m et donnees source V1 inchanges -- l'axe V2 compile retenu par la 5.49 (cordes + Hermite) est remplace par le cercle exact ajuste sur ces noeuds, a livrer par la Story 5.50 (renegociation du 2026-09-25) ;`

**New section after that entry:**
```markdown
## Bloquant Gate A -- revue d'overlay du 2026-09-24 (correct-course du 2026-09-25)

- source_spec: `_bmad-output/implementation-artifacts/spec-5-28-junction-semantic-authoring-and-overlay-sign-off.md`
  summary: PLANIFIEE (2026-09-25) -- prise en charge par les Stories 5.50 (geometrie V2 et validation de conduisibilite au niveau modele) et 5.51 (degagement physique des angles des 5 carrefours classiques), puis par la 5.28 a sa reprise (preuve physique des giratoires, liaison physique de la Gate A) (`epics.md`, `sprint-change-proposal-2026-09-25.md`). RESTE OUVERTE jusqu'a ce que : 5.50 et 5.51 soient implementees ; les paires de conflit nouvelles soient decidees par le proprietaire, les paires materiellement modifiees explicitement reconfirmees ou changees, et les decisions des paires retirees (historiques ou orphelines) explicitement disposees par le proprietaire, sans nouvelle decision active ; les artefacts 5.28 definitifs regeneres ; les 9 carrefours et les segments dont la geometrie V2 a change revus a nouveau ; et la Gate A signee avec sa liaison physique.
  evidence: Diagnostic du 2026-09-24 sur le modele committe. Giratoires (identiques sur les 4, deja presents avant la 5.49) : entrees et sorties de 2,44 m, rayon mini 0,25 m, cap cumule 130 deg pour 60 deg net ; `Ring_Merge` 7,5 deg en amont du croisement de l'axe de voie, aucun raccord tangent possible. Virages a gauche : 29 deg et 1,1 m hors axe au bord de la chaussee croisee. Virages a droite : rayon mini 3,80 m, gabarit empietant de 0,63 a 0,67 m sur les angles de trottoir carres ; aucun virage dans les 6 m disponibles ne les degage a R_adm (4,0344 m). Aucune regle de validateur ne bornait la courbure ni le repli d'enveloppe.
```

---

## 5. Implementation handoff

**Scope:** Moderate.

| Role | Responsibility |
| --- | --- |
| Developer agent (this correct-course run) | after final approval, apply sections 4.1–4.6 to the artifacts, preserving each file's line endings (LF: `epics.md`, oracle, specs, `sprint-status.yaml`; CRLF: spine, contract; CRLF with BOM: `deferred-work.md`), and confirm with `git diff --stat` that only the intended lines changed |
| Developer agent (`bmad-build`, Story 5.50) | implement 5.50 up to its diff-mode HALT; resume after the owner's decisions; regenerate the definitive 5.27 report and 5.28 artifacts |
| Developer agent (`bmad-build`, Story 5.51) | step-1 feasibility (HALT on failure), corner cut via instance overrides, conservative clearance proof, V1 regression before/after |
| Developer agent (`bmad-build`, Story 5.28 resumption) | `RoundaboutClearance` refresh on final trajectories, physical fingerprint, sign-off binding, evaluation, reproducibility test (HALT if not bit-identical) |
| Owner (Kenan) | decide new pairs, reconfirm or change materially changed pairs, dispose of orphan decisions (5.50 HALT); choose among 5.51 alternatives if step 1 fails; approve any shared-prefab or shared-asset change (Ask First); disposition every V1 behavioural delta (5.51); decide the comparison bound if reproducibility is not bit-identical (5.28); re-review the 9 junctions and changed segments; sign Gate A |

**Sequence:** 5.50 (backlog → build → owner-decision HALT → regeneration) → 5.51 (feasibility HALT → corners → V1 regression) → 5.28 HALT resumes (roundabout evidence, physical binding, re-review, signature) → Gate A → 5.29.

**Success criteria:**
- every declared element of `MVP_Run` passes the admission, consistency and fold rules; the 5.25/5.26/5.27 tests are unchanged and green; V1 source hash and 5.27 lineage byte-identical;
- ring on the exact circle fitted from the V1 ring nodes; entries and exits tangent; turns meet the crossing-roadway rule, with right-turn exceptions published;
- every new, removed and materially changed conflict pair resolved by the owner; no decision written by the agent;
- strictly positive physical residuals for every movement of the 9 junctions; every V1 behavioural delta from 5.51 dispositioned by the owner;
- Gate A signed and bound to the model hashes, the physical input fingerprint and the clearance results; both « Bloquant Gate A » entries close only then.

---

## Checklist record

| Section | Status | Notes |
| --- | --- | --- |
| 1. Trigger and context | `[x]` | 5.28 overlay review 2026-09-24; failed approach (5.27 movement generator) plus physical limitation (square corners); diagnostic measurements on the committed model and prefabs |
| 2. Epic impact | `[x]` | Epic 5 completable; two stories inserted before Gate A; 5.30/5.31/5.33 criteria extended; no epic added or invalidated |
| 3. Artifact impact | `[x]` | Epics, specs 5.28/5.27/5.49, Road World Model contract, spine, oracle, tracking; PRD `[N/A]` (FR/NFR in `epics.md`); UX `[N/A]` |
| 4. Path forward | `[x]` | Option 1 direct adjustment; rollback and MVP review not viable or not needed; arbitrations 1–7 decided by the owner |
| 5. Proposal components | `[x]` | Proposals 1–6 approved individually, with owner corrections and cross-proposal alignments integrated |
| 6. Final review and handoff | `[x]` | approved by Kenan 2026-09-26; sections 4.1–4.6 applied with line endings preserved, including `sprint-status.yaml` (5.49 → done, 5.50 and 5.51 in backlog); handoff to `bmad-build` for Story 5.50 |
