---
title: 'Story 5.24 -- V2 Regression Oracle Bench and V1 Trace Contract'
type: 'feature'
created: '2026-09-22'
status: 'done'
review_loop_iteration: 1
baseline_commit: '210f48811e3f99fbb93c5d2aa75885e65edcf878'
context:
  - '_bmad-output/planning-artifacts/traffic-v2/V1-BEHAVIORAL-ORACLE.md'
  - '_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-21.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Traffic V2 (Stories 5.25+) has no executable way to prove it preserves V1 behavior -- only a markdown catalog (`V1-BEHAVIORAL-ORACLE.md`) and prose contracts (AD-36, BC-12). Without a machine-readable oracle and a trace-comparison contract, every later story would be measured against memory instead of evidence.

**Approach:** Turn the accepted V1 oracle catalog into an executable EditMode oracle bench: a typed catalog of the 30 `V1-A01`..`V1-G04` rows with their evidence classification, a shape-guard register separating implementation-shape guards from behavioral evidence, a behavior-neutral trace record type, and a replay/compare harness with declared per-field tolerances. Record an EditMode-scoped CPU baseline as observational data.

## Boundaries & Constraints

**Always:** Read V1 at frozen reference commit `210f48811e3f99fbb93c5d2aa75885e65edcf878` (AD-36) -- never modify it. Every catalog row resolves to a named classification (`AUTO-EDIT` / `AUTO-PLAY` / `SOURCE-GUARD` / `OWNER-ACCEPTED` / `MANUAL` / `GAP` / `NEGATIVE`) per the vocabulary in `V1-BEHAVIORAL-ORACLE.md`. All new code lives under `Assets/RoadRage/Tests/EditMode/TrafficOracle/`.

**Ask First:** none -- scope, classification vocabulary and file boundary are already fixed by the accepted oracle catalog and AD-36.

**Never:** No V1 capability work, no PlayMode-harness repair, no new assembly (this asmdef already references every feature assembly needed). No file under `Assets/RoadRage` outside `Tests/` is touched. No test invented for a `GAP` row -- `GAP` rows are recorded as V2 requirements, not faked evidence. No PlayMode test -- this story is deliberately EditMode-only. No trace field is fabricated -- every field recorded from V1 must be backed by actual observable V1 evidence; V1 and a future V2 are never required to share identical internal decision representations. No trace field is byte-identical by requirement unless the implementation shows a specific field/representation genuinely needs it. Traffic-test triage is bounded to the tests and source-shape guards the accepted oracle catalog references -- not a repository-wide historical test audit.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Full catalog | all 30 rows from `V1-BEHAVIORAL-ORACLE.md` | every row has a named classification; `GAP` rows carry no invented test | test fails if any row is unclassified |
| Divergence injection | two traces identical except one field outside tolerance | comparer reports that named field | test fails if comparer instead passes or reports the wrong field |
| Determinism | same seeded scenario replayed twice | trace sequences compare equal under the declared trace contract (exact match on discrete/structural fields, declared tolerance on numeric fields) | test fails on any out-of-contract drift |

</frozen-after-approval>

## Code Map

- `_bmad-output/planning-artifacts/traffic-v2/V1-BEHAVIORAL-ORACLE.md` -- source of truth for the 30 rows, evidence vocabulary and shape-guard candidates; transcribe, do not re-derive.
- `Assets/RoadRage/Tests/EditMode/RoadRage.Tests.EditMode.asmdef` -- already references every feature assembly (`RoadRage.Features.Vehicles`, `.Run`, etc.) and `nunit.framework.dll`; no asmdef change needed.
- `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs:1389` -- `ReplayRoute(...)`, kinematic replay over the real authored district; reuse as the determinism/trace source, do not copy its body.
- `Assets/RoadRage/Tests/EditMode/Story59ParameterizedDriverModelTests.cs:18` -- driver-kernel suite (`Story59ParameterizedDriverModelTests`); source for the pure-kernel oracle rows (`V1-B*`, `V1-C*`).
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- the V1 decision step; read-only, used to measure the CPU baseline.
- `_bmad-output/implementation-artifacts/anomalies/epic 5/ANO-5.10-03/ANO-5.10-03.md` -- AC1-AC8, backing the `GAP` rows `V1-F04`-`V1-F06`.
- `docs/setup/story-5-15-credible-collisions-and-damage-notes.md` -- prior art for the Stopwatch median/p95 measurement pattern to reuse for the CPU baseline.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Tests/EditMode/TrafficOracle/OracleCatalog.cs` -- static catalog of all 41 `V1-A01`..`V1-G04` rows (id, contracted behavior, classification, bound test name(s) where one exists) transcribed from `V1-BEHAVIORAL-ORACLE.md` -- gives every later story a row to cite. Note: the accepted source document carries 41 distinct row ids, not 30 as this spec's prose estimated; transcribed as-is per Design Notes ("transcribe, do not re-derive").
- [x] `Assets/RoadRage/Tests/EditMode/TrafficOracle/ShapeGuardRegister.cs` -- registry pairing each `SOURCE-GUARD` test with the exact V1 symbol it pins, separate from tests that are behavioral evidence -- the list Story 5.48 consumes to know what retires with V1.
- [x] `Assets/RoadRage/Tests/EditMode/TrafficOracle/TrafficTraceRecord.cs` -- behavior-neutral record type: frame, vehicle id, pose, speed, road element id, route progress, goal, blockers, final intent, each marked either a common field V1 can genuinely provide today or an optional/versioned diagnostic field later V2 stories may populate; no field implies V1 and V2 share internal decision representation.
- [x] `Assets/RoadRage/Tests/EditMode/TrafficOracle/TrafficTraceComparer.cs` -- compares two ordered trace sequences against declared per-field tolerances. Discrete/structural fields (`VehicleId`, `RoadElementId`, `Goal`, `Blockers`) require exact equality. Numeric behavioral fields -- `Position`, `YawDegrees`, `Speed`, `RouteProgress`, **and each of `FinalIntent`'s four components (`Throttle`/`Steer`/`BrakeReverse`/`Handbrake`)** -- use a declared named epsilon (review finding: the user's own clarification explicitly names "continuous intent values" as needing tolerance, not exact match; the prior iteration wrongly bucketed `FinalIntent` as discrete). Every numeric comparison must treat `NaN` as a divergence, not a silent match (review finding: `delta > epsilon` is `false` for `NaN`, so write it as `!(delta <= epsilon)` or an explicit `float.IsNaN` guard).
- [x] `Assets/RoadRage/Tests/EditMode/TrafficOracle/TrafficOracleTests.cs` -- catalog completeness (no unclassified row); shape-guard register non-empty and named; comparer self-check (identical traces pass, one injected out-of-tolerance divergence fails and names the field, including at least one case per newly-tolerant `FinalIntent` component and a NaN-divergence case); determinism (same seeded `ReplayRoute`-based scenario replayed twice produces trace sequences that compare equal under the declared trace contract, and the replay itself is asserted to have reached an exit portal rather than merely hit `maxSteps` -- review finding: a route that never terminates would otherwise silently compare two truncated traces as "equal"). Add one reflection-based test asserting every `BoundTest`/`ShapeGuardEntry` full name resolves to an existing `[Test]`/`[UnityTest]` method in the EditMode assembly (review finding: today only string shape is checked, so a future rename/deletion of a cited V1 test would desync the catalog with zero red test).
- [x] `docs/setup/story-5-24-oracle-bench-notes.md` -- record the EditMode oracle/replay CPU baseline (median/p95, naming exactly what was measured -- e.g. pure decision-kernel or replay-harness cost, not full V1 runtime) as observational data only, plus catalog/self-check results, updated for the comparer and test additions above.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- set `5-24-v2-regression-oracle-bench-and-v1-trace-contract` to `in-progress`.
- [x] `graphify update .` -- regenerate the graph after source changes.

**Acceptance Criteria:**
- Given the accepted V1 oracle catalogue, when the bench is built, then every row `V1-A01`-`V1-G04` resolves to a named classification and, where one exists, a named test, and `GAP` rows carry no invented V1 test.
- Given a V1 trace and a candidate trace, when the comparison harness runs, then it reports divergence per named field against declared tolerances deterministically for the same seed, and an injected out-of-tolerance divergence fails rather than being absorbed.
- Given the tests and source guards referenced by the accepted V1 Behavioral Oracle, when triaged, then each referenced evidence item is classified as behavioral evidence or as a shape guard bound to a named V1 symbol.

## Spec Change Log

- **2026-09-22, review loop 1.** Three-layer review (`blind-hunter`, `edge-case-hunter`, `verification-gap`) on the first implementation. Triggering finding (`bad_spec`): the Tasks section (this file, non-frozen) classified `FinalIntent` as a discrete/exact-match field, contradicting the human's own approval-time clarification, which explicitly names "continuous intent values" as an example of a numeric field needing tolerance. Amended: `FinalIntent`'s four components move to the numeric/epsilon-tolerant bucket in `TrafficTraceComparer.cs`. Known-bad state avoided: a future V2 candidate trace whose intent composer produces the same behavior through slightly different floating-point arithmetic would fail the comparison on FP noise alone, defeating the "behavior not internal architecture" design goal this same clarification established. Folded into the same amendment (cheap, same files, would otherwise ship unfixed): (1) numeric comparisons silently treated `NaN` as a match because `delta > epsilon` is `false` for `NaN` -- fix to `!(delta <= epsilon)`; (2) nothing verified that a `BoundTest`/`ShapeGuardEntry` name actually resolves to a real test method -- add a reflection-based existence check; (3) `ReplayTrace`'s determinism test only asserted `Count > 0`, so a route that never reaches an exit within `maxSteps` would silently compare two identically-truncated traces as passing -- assert the replay actually terminated at an exit. **KEEP** (verified correct, must survive re-derivation): the 41-row catalog transcription and its rationale note about the "30" figure; `ShapeGuardRegister`'s 4 entries; `TrafficTraceFrame`'s common-vs-optional-diagnostic field split and its doc comments; the CPU baseline's exact "what is / is not measured" labeling; the determinism test's approach of reimplementing `ReplayTrace`/`ResolveNextNode` as an independent composition of the same real `LaneGraphRouting`/`NetworkedAIVehicleDriverController` pure functions rather than copying `Story510...ReplayRoute`'s body; discrete/exact-match treatment of `VehicleId`, `RoadElementId`, `Goal`, `Blockers` (unaffected by this finding). Two review findings evaluated and rejected as out of this story's scope: asserting on `JunctionGrant`/`SafetyResult`/`RecoveryState`/`PathPlanId` (these are V1-always-null, V2-future diagnostic fields by design -- nothing populates them yet); guarding `ReplayTrace` against an out-of-range `entryNode` (only ever called with an assert-guarded `graph.EntryPortals[0]`, not a reachable fault).

## Design Notes

The catalog and shape-guard register are plain static C# data (not `ScriptableObject`s) -- this is fixed test infrastructure transcribed from an already-accepted markdown document, not author-editable runtime content, so the project's `ScriptableObject`+stable-id convention does not apply here.

The trace contract compares contracted *behavior*, not internal architecture: V1 and a later V2 are never required to compute their traces the same way internally, only to produce comparable values for the fields each can genuinely observe. Byte-identical serialization is not a goal in itself -- it only falls out of the determinism test because V1's replay is itself deterministic; the actual pass condition is "compares equal under the declared per-field contract."

`FinalIntent`'s four components (`Throttle`/`Steer`/`BrakeReverse`/`Handbrake`) are continuous control outputs, not symbolic labels -- despite living on a "discrete/structural"-sounding field name, they get the same named-epsilon treatment as `Speed`/`Position` (review loop 1). Only fields whose values are genuinely symbolic (`VehicleId`, `RoadElementId`, `Goal`, `Blockers`) stay exact-match.

Every numeric comparison must be written so `NaN` compares as a divergence, never a silent match: `delta > epsilon` is `false` when `delta` is `NaN` (IEEE 754), so the correct form is `!(delta <= epsilon)` (or an explicit `float.IsNaN` check first).

## Verification

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.TrafficOracleTests"` -- targeted fixture green.
- `.\scripts\validate.ps1 -TestMode EditMode` -- full suite green, no V1 test regressed.

## Suggested Review Order

**Trace contract**

- Entry point: the behavior-neutral field split this whole bench is built to compare -- common V1-capable fields vs. nullable V2-only diagnostics.
  [`TrafficTraceRecord.cs:24`](../../Assets/RoadRage/Tests/EditMode/TrafficOracle/TrafficTraceRecord.cs#L24)

**Comparison logic (review loop 1+2 fixes live here)**

- Divergence rules: discrete/exact-match fields vs. numeric/epsilon-tolerant fields, including `FinalIntent`'s four components (loop 1: they were wrongly exact-match).
  [`TrafficTraceComparer.cs:60`](../../Assets/RoadRage/Tests/EditMode/TrafficOracle/TrafficTraceComparer.cs#L60)

- `!(delta <= epsilon)` instead of `delta > epsilon` -- the NaN-safety fix (loop 1); collects every divergence per frame, not just the first.
  [`TrafficTraceComparer.cs:139`](../../Assets/RoadRage/Tests/EditMode/TrafficOracle/TrafficTraceComparer.cs#L139)

**Oracle data (hand-transcribed, verify against the source document)**

- The 41-row catalog transcribed from `V1-BEHAVIORAL-ORACLE.md`, each row's classification and bound tests.
  [`OracleCatalog.cs:78`](../../Assets/RoadRage/Tests/EditMode/TrafficOracle/OracleCatalog.cs#L78)

- The 4 SOURCE-GUARD entries Story 5.48 will consume at V1 retirement.
  [`ShapeGuardRegister.cs:34`](../../Assets/RoadRage/Tests/EditMode/TrafficOracle/ShapeGuardRegister.cs#L34)

**Replay harness and self-checks**

- Independent re-composition of V1's real pure functions, with the orbit-stall guard (loop 2 fix: reset only on an actual node change).
  [`TrafficOracleTests.cs:463`](../../Assets/RoadRage/Tests/EditMode/TrafficOracle/TrafficOracleTests.cs#L463)

- Determinism now asserts the replay actually reached a departed exit portal, not just that it produced frames (loop 1 fix).
  [`TrafficOracleTests.cs:281`](../../Assets/RoadRage/Tests/EditMode/TrafficOracle/TrafficOracleTests.cs#L281)

- Reflection check: every cited `BoundTest`/`ShapeGuardEntry` name must resolve to a real `[Test]`/`[UnityTest]` method (loop 1 fix), with a negative-case proof it can actually fail (loop 2 fix).
  [`TrafficOracleTests.cs:357`](../../Assets/RoadRage/Tests/EditMode/TrafficOracle/TrafficOracleTests.cs#L357)
  [`TrafficOracleTests.cs:388`](../../Assets/RoadRage/Tests/EditMode/TrafficOracle/TrafficOracleTests.cs#L388)

- Multi-field divergence proof: guards against the comparer silently regressing to "first divergence only" (loop 2 fix).
  [`TrafficOracleTests.cs:208`](../../Assets/RoadRage/Tests/EditMode/TrafficOracle/TrafficOracleTests.cs#L208)

- Observational CPU baseline, explicitly labeled as replay-harness cost, not V1's runtime cost.
  [`TrafficOracleTests.cs:311`](../../Assets/RoadRage/Tests/EditMode/TrafficOracle/TrafficOracleTests.cs#L311)

**Peripherals**

- Measured numbers and what is/isn't measured, kept in sync across both review loops.
  [`story-5-24-oracle-bench-notes.md:1`](../../docs/setup/story-5-24-oracle-bench-notes.md#L1)
