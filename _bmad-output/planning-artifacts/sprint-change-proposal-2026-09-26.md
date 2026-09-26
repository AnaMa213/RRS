# Sprint Change Proposal — 2026-09-26

**Subject:** Carry into the normative Story 5.50 of `epics.md` three owner requirements approved on 2026-09-26 during the review of the 5.50 implementation spec. They are already in the approved spec `implementation-artifacts/spec-5-50-v2-movement-geometry-correction-and-model-level-drivability-validation.md`:
- **P1:** a conservative between-sample guarantee for the conflict-candidate sweep;
- **P2:** the bootstrap of the 76 historical conflict decisions;
- **P3:** an executable reference checkpoint.

**Trigger:** owner review of the 5.50 spec (`bmad-build`, CHECKPOINT 1, 2026-09-26): "[E] targeted modification" request, then "[A] approve and carry P1–P3 into the story".

**Status:** drafted in batch mode on 2026-09-26; awaiting owner approval. `epics.md` is not modified until approval.

**Scope classification:** **Minor.** One story is refined: Story 5.50, backlog, spec ready-for-dev. No epic, AD, contract row, SPEC requirement or other story changes.

**Recommended path:** direct adjustment of Story 5.50.

---

## 1. Issue summary

### 1.1 Trigger
The owner reviewed the first 5.50 spec and required three additions before approving it. The normative story is the authority for 5.50 (`bmad-build` rule: "the spec wins" only over tooling, never over `epics.md`). A spec carrying requirements absent from the story would leave the two out of sync.

### 1.2 Problem statement and evidence (verified 2026-09-26)
- **P1.** Story 5.50 sweeps candidates "with the full maximum footprint inflated by the margin and δ_c" but says nothing about the motion **between** sampled poses. Only Story 5.51 carries an interval bound, for vehicle–obstacle clearance (`epics.md` 5.51 AC, "minimum over all pose intervals … δ = |Δp| + ρ·Δθ"). The 2026-09-25 proposal records this as open: "whether the 5.50 conflict-candidate sweep should reuse the 5.51 interval bound". A sample-only sweep can miss two footprints that touch only between poses.
- **P2.** The 76 decisions in `MVP_Run.road-authoring.json` (all `Accepted`) carry no geometry fingerprint. Their geometry lives only in the committed format-1 model, which `RoadModelDocument.Load` will refuse once the document format becomes 2. The story says unchanged pairs keep their decisions, but gives no way to tell an unchanged pair after the switch.
- **P3.** The story requires "a golden canonical hash of an undeclared model captured before the change", but no such golden exists in the tests. The canonical bytes are not public either: `RoadModelCanonicalWriter.Write` is private, and `Story526GeometryAndLocalizationTests.cs:1458-1472` forbids `InternalsVisibleTo`. Only `CompiledRoadModel.Version` (a 128-bit SHA-256 prefix of the canonical bytes) can be captured before any production change.

## 2. Impact analysis

- **Epics:** Epic 5, Story 5.50 only. No resequencing.
- **Stories:**
  - 5.51 is unchanged: P1 reaches the same bound but proves it for two moving footprints; it does not import it.
  - 5.28 is unchanged: its amended text already says no decision on changed pair geometry counts until the owner reconfirms it.
  - 5.30/5.31/5.33 are unchanged.
- **Artifacts:**
  - SPEC, `ARCHITECTURE-SPINE.md`, `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`, `V1-BEHAVIORAL-ORACLE.md`, UX: no conflict. A grep for sweep, fingerprint, golden and historical-decision wording returns nothing.
  - `deferred-work.md` and `sprint-status.yaml`: no change.
  - `epic-5-context.md` becomes stale; the next `bmad-build` recompiles it automatically.
- **Technical:**
  - two more ordered steps before the implementation (0 and 1b), plus a no-behaviour-change step 1a;
  - one Editor-only data file (the historical fingerprint table);
  - one new owner-only action;
  - no runtime, V1 or physical impact.
- **Risk:** low.
  - *Estimate, not measured:* the full-footprint sweep changes every zone volume, so the unchanged-pairs action will probably reconfirm none of the 76 pairs. Each pair would then need an individual owner decision, which is the conservative outcome.

## 3. Recommended approach

Direct adjustment of Story 5.50: three edits (§4), all additions or strengthenings. No approved requirement is relaxed or removed.

## 4. Detailed change proposals

### 4.1 Story 5.50 — Conflict candidates (P1)

**Section:** `**Conflict candidates:**` — append after "The rule applies uniformly to the 9 junctions."

OLD: *(no between-sample clause)*

NEW:
```
- **Conservative between samples (owner, 2026-09-26):** a pair is found whenever the true swept regions of the two moving footprints, each inflated by `LateralClearanceMarginMeters` and δ_c, touch on any interval between consecutive poses. This includes seams and the reach beyond movement ends; a seam counts as one interval with Δp = seam gap and Δθ = heading jump.
  - Definitions: ρ = √((`MaxVehicleLengthMeters`/2)² + `MaxVehicleHalfWidthMeters`²), and δ = |Δp| + ρ·|Δθ| per interval.
  - Rule: a pair is a candidate if, for some interval I of one movement, some interval J of the other, and some end poses a of I and b of J, dist(F_a, F_b) ≤ δ_I/2 + δ_J/2 + 2·(margin + δ_c).
  - Footprint distance is exact or underestimated, never overestimated. An interval with |Δθ| ≥ 90° fails closed.
  - The zone volume is the axis-aligned box of F ⊕ B(δ/2 + margin + δ_c) over the contributing intervals.
- **Proof obligation:** the bound is proven for two moving footprints on the compiled reference (positions linear, normalized-interpolated heading monotone).
  - For any body point, the displacement to pose i plus the displacement to pose i+1 is at most δ, so one end pose is within δ/2.
  - The bound coincides with the 5.51 interval bound but is derived here, not imported. This settles the question left open by the 2026-09-25 proposal.
```

**Rationale:** a sample-only sweep can miss a real conflict, and 5.50 must not inherit 5.51's bound without its own proof.

### 4.2 Story 5.50 — Decision reconfirmation (P2)

**Section:** `**Decision reconfirmation:**` — append after "The pipeline never writes, transfers, deletes or approves a decision."

OLD: *(no bootstrap clause)*

NEW:
```
- **Bootstrap of the 76 existing decisions (owner, 2026-09-26):** they carry no fingerprint, and the format-1 model holding their geometry is refused once the document format becomes 2.
  - Before any importer, format or sweep change, the unchanged pipeline computes a **historical fingerprint table** on the committed `MVP_Run` model. Per decision it records the pair keys, the canonical geometry hash of each member movement, the zone volume, and the fingerprint under a versioned fingerprint schema.
  - The computation refuses if the fresh `ModelVersion` differs from the committed document's.
  - The SHA-256 of the table and of a byte copy of the format-1 model are recorded.
- The table is Editor-only review data. `RoadModelDocument.Load`, the compiler, the validator and runtime never read it, and it never yields a `CompiledRoadModel`. A test asserts that no source outside the migration tooling references it.
- **Owner-only reconfirmation actions:**
  - "Reconfirm unchanged pairs" writes the fresh fingerprint only where the historical fingerprint equals the fresh one, geometry **and** volume. It never touches a pair whose member geometry or volume changed, or a pair absent from the table. If the table's hash or schema version does not match, it reconfirms nothing (fail closed).
  - Changed pairs are reconfirmed one at a time ("reconfirm this pair", with old and new geometry and volume shown), or by the owner's own edit.
  - The pipeline never invokes either action.
```

**Rationale:** without a pre-switch capture, no pair can be proven unchanged. A bulk action must never carry a decision onto changed geometry.

### 4.3 Story 5.50 — Execution sequence, verification and guard rails (P3, and wiring of P1/P2)

**Section:** `**Execution sequence (binding):**`, step 1.

OLD:
```
1. Implement the importer, drivability, validation, document, sweep, diff and isolation-view changes; synthetic tests pass.
```
NEW:
```
0. **Reference checkpoint** (local, Editor connected, before any production C# change):
   - Add only a golden test fixture:
     - a deterministic undeclared model: literal `ModelId`, literal samples covering a section, straight and arc corridors, a junction, a movement and a portal, no randomness, no clock;
     - the 5.25 and 5.26 fixtures, read through reflection from the test assembly. Their files are unchanged.
   - An explicit capture test records each model's `CompiledRoadModel.Version`.
   - `git diff --stat` shows only that fixture. Commit it together with the baseline hashes (lineage, V1 source hash, 5.27 report, committed 5.28 artifacts) and a byte copy of the format-1 model. No `InternalsVisibleTo`.
1a. Add a read-only public accessor for the canonical bytes, with no behaviour change. Capture the golden bytes, and accept them only if their SHA-256 prefix equals the step-0 `Version`; otherwise HALT.
1b. Capture the historical fingerprint table (see Decision reconfirmation).
1c. Implement the importer, drivability, validation, document, sweep, diff and isolation-view changes; synthetic tests pass.
```
Steps 2–5 are unchanged.

**Section:** `**EditMode verification:**`

OLD: `- **Canonical compatibility:** a golden canonical hash of an undeclared model is captured before the change and still matches.`
NEW: `- **Canonical compatibility:** the undeclared models captured at step 0 compile, after the change, to canonical bytes equal to the step-1a golden bytes and to the step-0 `Version`.`

OLD: `- **Candidates:** a synthetic chain with a corridor shorter than half a vehicle between two movements finds the pair; ordinary following is published as such.`
NEW: `- **Candidates:** a synthetic chain with a corridor shorter than half a vehicle between two movements finds the pair; ordinary following is published as such. Three pairs are found even though they touch only between sampled poses: a translation crossing between two poses 10 m apart; a corner that touches only during an interval's rotation; a contact only within a seam interval. A pose-only check provably misses each of the three.`

OLD: `- **Diff and reconfirmation:** the exhaustive candidate diff is published; a decision on a changed pair blocks Gate A until reconfirmed.`
NEW: `- **Diff and reconfirmation:** the exhaustive candidate diff is published; a decision on a changed pair blocks Gate A until reconfirmed. The historical table covers the 76 decisions exactly. The unchanged-pairs action never reconfirms a pair whose geometry or volume changed, or a pair absent from the table, and it fails closed on a tampered table.`

**Section:** `**Must NOT be copied:**` — append: `; reusing another story's bound without its own proof; reconfirming a changed pair in bulk; changing production code before the reference checkpoint`.

**Section:** Acceptance Criteria — full-footprint sweep AC.

OLD: `**Then** pairs are found across elements shorter than half a vehicle, ordinary following is published as following, and every zone stays owned by its junction`
NEW: `**Then** pairs are found across elements shorter than half a vehicle, between sampled poses and across seams, ordinary following is published as following, and every zone stays owned by its junction`

**Rationale:** these edits make the "golden captured before the change" requirement executable, and they order the P2 capture before the format switch that would otherwise destroy the historical geometry.

## 5. Implementation handoff

- **Scope:** Minor.
- **Developer agent (this session, after approval):**
  - apply §4.1–§4.3 to `epics.md`;
  - add to the 2026-09-25 proposal only a pointer line stating that its open question (5.50 sweep bound) is settled here. Its approved content is not otherwise touched.
- **Developer agent (`bmad-build` 5.50, local session with the Unity Editor):** implement from the approved spec, starting at step 0.
- **Success criteria:**
  - Story 5.50 in `epics.md` and the approved spec agree on P1–P3;
  - no other story, contract or architecture text changes;
  - the spec's "Exigences du proprietaire … absentes de la Story 5.50" note becomes historical. The spec's Spec Change Log records the carry-over, and the frozen block stays untouched.

## Checklist record

- **1.1–1.3** Done: trigger, problem and evidence in §1.
- **2.1–2.5** Done: Epic 5 completes as planned; no epic added, removed or resequenced.
- **3.1** N/A: SPEC. **3.2** N/A: architecture and contract. **3.3** N/A: UX. **3.4** Done: `epic-5-context.md` goes stale and is regenerated automatically; no other artifact changes.
- **4.1** Viable: direct adjustment, low effort, low risk. **4.2** Not viable: rollback, nothing to revert. **4.3** N/A: MVP review.
