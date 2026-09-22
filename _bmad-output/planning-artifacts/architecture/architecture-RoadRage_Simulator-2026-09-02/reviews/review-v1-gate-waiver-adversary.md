# V1 Runtime-Evidence Gate Waiver — Adversarial Review

**Lens:** Determine whether independently acting downstream builders can interpret the 2026-09-22 waiver differently about (a) the accepted V1 reference, (b) future Traffic V2 regression obligations, or (c) the remaining pre-story architecture gate.

**Verdict: PASS.** The narrow waiver is coherent across the architecture spine, behavioral oracle, responsibility contract and approved proposal. It removes additional work on the V1 harness/reference; it does not waive any Traffic V2 regression, integration or retirement evidence.

## Adversarial constructions

### 1. Builder treats “accepted baseline” as permission to skip all weakly evidenced behaviors in V2

This interpretation is not conforming.

- The oracle vocabulary makes `OWNER-ACCEPTED`, `AUTO-PLAY`, `MANUAL` and `GAP` distinct evidence states.
- `GAP` explicitly means a required V2 regression scenario that does not block Road World Model architecture.
- `MANUAL` and `GAP` rows explicitly remain future V2 regression requirements.
- Every catalog row remains subject to the retirement gate: pass in V2, consciously replace with an approved passing successor, or declare obsolete with recorded risk.
- `MVP_Run` integration, host/client evidence and manual physical-feel review remain mandatory for retirement.

Therefore “accepted” means the reference catalog and pinned commit are sufficient inputs; it does not mean every listed behavior has already been proven or may be omitted from V2.

### 2. Builder treats `GAP` rows as claims about behavior implemented by V1

This interpretation is not conforming.

- The oracle identifies `GAP` as having no adequate V1 proof.
- V1-F04 through V1-F06 point to the open anomaly contract and describe V2 parity criteria, not delivered V1 mechanisms.
- The explicit negative oracle separately states that intersection arbitration, signals, blocked-exit grants, lane changes and multi-stage recovery do not exist in V1.
- The proposal and AD-36 both state that Story 5.14 delivered the shared-intent transition, not collision response or physical lane recovery.

The catalog intentionally contains both historical behavior and future regression requirements, but its evidence and classification columns disambiguate them row by row.

### 3. Builder repairs/replays V1 before beginning Road World Model design

This is allowed only as optional investigation, not as a planning prerequisite.

- AD-36 says no additional V1 runtime recipe, PlayMode-harness repair, branch or tag gates Road World Model design.
- The oracle repeats that the pinned commit, existing artifacts and catalog are the accepted reference.
- The responsibility contract states that no additional runtime-evidence gate remains.

No downstream builder can legitimately block Road World Model architecture on new V1 evidence without a new explicit course decision.

### 4. Builder starts implementation stories because the V1 gate was waived

This interpretation is not conforming.

- AD-36 permits story generation only after the Road World Model/responsibility architecture gate passes.
- The responsibility contract lists eleven concrete gate decisions, including schema ownership, identity, geometry, junction/signal ownership, localization, migration proof, representative topology examples, validation and catastrophic policy.
- The proposal retains owner approval of that later architecture gate before story generation.

The waiver removes one former prerequisite; it does not imply that the remaining gate has passed.

## Findings

### Critical / high

None.

### Low — Proposal retains stale future-tense/plural gate wording

The proposal still says replacement stories wait until “the V1 behavioral oracle and the V2 Road World Model/responsibility contracts have passed architecture review” and elsewhere refers to “architecture gates” in the plural. Read alone, those phrases could suggest a separate V1 review gate remains. The proposal’s dated owner clarification, AD-36, the oracle’s accepted status and the responsibility contract all resolve the meaning, so this is not an architectural divergence point.

**Disposition:** Editorial autofix if the proposal is touched again: say that the V1 oracle is accepted and only the Road World Model/responsibility architecture gate remains before story generation.

## Final determination

Two conforming builders cannot now disagree about the governing boundary:

1. **Accepted input:** commit `210f48811e3f99fbb93c5d2aa75885e65edcf878`, existing artifacts/tests and the accepted oracle catalog; no new V1 replay, harness repair, branch or tag is required.
2. **Future obligations:** every oracle row still requires V2 pass, approved successor or recorded obsolescence before V1 retirement; `GAP` and `MANUAL` cases remain V2 regression work.
3. **Current pre-story blocker:** only the enumerated Road World Model/responsibility architecture gate remains open.

The narrow V1 runtime-evidence waiver is safe to retain.

## Closure recheck — latest artifact correction

**Final verdict: PASS.**

- The proposal now says the V1 oracle is accepted and names only the V2 Road World Model/responsibility architecture gate before replacement story generation. The prior plural/future-tense wording is closed.
- AD-36, the oracle, contract, sprint status, Epic 5 context and the dated deferred-work closure consistently reject any new V1 recipe, PlayMode-harness repair, branch or tag as a prerequisite. Older deferred-work text describing the former harness blockage remains chronological history and is explicitly superseded by the 2026-09-22 decision.
- The Road World Model gate now contains ten road/schema/validation decisions. Catastrophic cleanup is not one of them.
- Catastrophic cleanup is explicitly deferred and gates only future collision-response/recovery stories. Until that later decision, teleport, reinsertion and mid-road despawn remain unauthorized; this does not block Road World Model story planning after its own gate passes.
- References to future V2 parity, regression and retirement gates remain correct: they constrain V1 retirement and V2 acceptance, not Road World Model architecture entry.

The earlier review sentence claiming eleven Road World Model gate items, including catastrophic policy, is superseded by this closure. No current authoritative wording recreates a V1 validation/oracle gate or couples catastrophic cleanup to the Road World Model gate.
