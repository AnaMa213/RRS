# V1 Reference Gate Waiver — Architecture Rubric Review

**Reviewer lens:** BMad good-spine checklist  
**Reviewed:** 2026-09-22  
**Scope:** the narrow decision accepting commit `210f48811e3f99fbb93c5d2aa75885e65edcf878` and Story 5.14 as the Traffic V1 reference, waiving additional V1 runtime/PlayMode/manual-recipe/branch-tag prerequisites, and retaining the Road World Model architecture gate before Traffic V2 implementation stories.

## Verdict

**CONDITIONAL PASS.** The waiver itself is sound and is recorded without falsifying evidence: the accepted commit exists and is current `HEAD`; incomplete PlayMode/manual evidence remains explicitly disclosed; Story 5.14 remains complete only for its approved shared-intent/physics scope; `ANO-5.10-03` remains a V2 requirement; and V1 retirement still requires V2 parity/integration evidence.

Two high-severity wording issues can nevertheless recreate gates outside the approved decision, and one medium proposal inconsistency still assumes a branch/tag that is now optional. These should be corrected before this reviewer lens closes.

The deterministic spine lint passes with zero findings.

## Evidence checked

- `ARCHITECTURE-SPINE.md` AD-36 and its Road World Model open/deferred items.
- `V1-BEHAVIORAL-ORACLE.md` status, evidence vocabulary, scenario evidence, accepted-baseline decision and retirement gate.
- `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md` decision status and architecture gate.
- `sprint-change-proposal-2026-09-21.md` recommendation, migration sequence, work packages, artifact changes and handoff.
- Git object verification: `210f48811e3f99fbb93c5d2aa75885e65edcf878` is a valid commit and equals current `HEAD`.
- `lint_spine.py`: zero findings.

## What is consistent

- AD-36 now names the exact accepted commit, freezes V1 capabilities, removes additional Story 5.14 runtime recipe, PlayMode-harness, branch and tag prerequisites, and leaves only the Road World Model/responsibility architecture gate before Traffic V2 implementation stories.
- The oracle does not pretend the waiver created evidence. `AUTO-PLAY`, `MANUAL`, `GAP` and `OWNER-ACCEPTED` remain distinct, and the 645/645 EditMode record plus unreliable/unreported runtime evidence remain visible.
- Story 5.14's approved reduced scope is consistently separated from collision response and physical lane recovery. Those behaviors remain in V1-F04 through V1-F06 as V2 contract gaps, not claimed V1 deliveries.
- The waiver does not weaken the V1 retirement gate: V2 must still pass, consciously replace, or explicitly obsolete every catalog row and provide `MVP_Run`, host/client and physical-feel evidence before V1 removal.
- The Road World Model schema/ownership/migration questions remain a genuine pre-story architecture gate.

## Findings

### HIGH — The oracle still labels regression-suite extraction as a prerequisite “before V2 implementation”

**Where:** `V1-BEHAVIORAL-ORACLE.md`, heading `Regression suites to extract before V2 implementation` and its six-item list.

**Problem:** The accepted-baseline section correctly says no additional V1 validation gate remains, but this heading still reads as a second pre-implementation gate. Several listed suites require new physical, junction, recovery and network evidence—the exact work the waiver moved into future V2 validation. A planner could therefore block story generation until these suites are extracted, contradicting AD-36 and the contract's “Road World Model gate only” rule.

**Disposition:** **Autofix.** Rename the section to something like `Regression suites carried into V2 delivery and parity` and add one sentence: the catalog is preservation input for future V2 stories/tests, not a prerequisite beyond the Road World Model architecture gate. Keep every scenario; only remove the gating interpretation.

### HIGH — Catastrophic cleanup is incorrectly inside the global Road World Model architecture gate

**Where:** `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`, Architecture gate item 11 plus “Only after this gate should BMAD generate executable stories.”

**Problem:** Catastrophic out-of-world cleanup ownership is a valid open decision, but it is not a Road World Model schema/responsibility decision. The spine already scopes it correctly: answer it before a collision-response or recovery story; until then teleport/reinsert/mid-road despawn are unauthorized. Putting it inside the global architecture gate makes an unrelated recovery policy a blocker for every Traffic V2 story and violates the approved “retain only the Road World Model architecture gate” direction.

**Disposition:** **Autofix.** Remove catastrophic policy from the global Road World Model gate and retain it as an explicit open question/deferred decision that gates only collision-response/recovery implementation stories. Preserve the current default prohibition until that narrower decision is approved.

### MEDIUM — The proposal still speaks as though a reference branch/tag exists or is a V2-1 outcome

**Where:** `sprint-change-proposal-2026-09-21.md` lines stating “The reference branch is an archive/oracle,” retirement retains “the tag/branch,” and V2-1 produces a “reference tag/branch plan.”

**Problem:** The proposal now correctly says the accepted commit plus catalog is the reference and branch/tag creation is optional hygiene. The residual wording does not directly recreate a formal gate, but it can generate unnecessary repository work or imply that an archive exists when none is required.

**Disposition:** **Autofix.** Replace those residual claims with the accepted commit/catalog language. State that an optional tag/branch, if later created, is archival hygiene only. Change V2-1's outcome to the already-approved oracle, reconciled ownership and recorded commit; no tag/branch plan is required.

## Good-spine checklist result

| Criterion | Result |
| --- | --- |
| Fixes the real divergence point | Pass: exact V1 reference and sole remaining global gate are explicit. |
| Rules are enforceable | Pass in AD-36; conditional in companion wording until the two high findings are fixed. |
| Deferred/open work cannot silently become a conflicting gate | Conditional: catastrophic cleanup is currently promoted into the wrong global gate. |
| Ratifies brownfield reality | Pass: exact commit and known evidence quality are accurately recorded. |
| Covers approved capabilities without inventing proof | Pass. |
| Preserves migration/parity safety | Pass: evidence gaps remain future V2 scenarios and retirement remains gated. |

## Gate recommendation

Apply the two high wording/scope fixes and the medium proposal cleanup, rerun deterministic lint, then close this rubric lens. No new V1 execution is required by this review.

## Closure recheck — 2026-09-22

**FINAL VERDICT: FAIL — one required consistency correction remains.**

Closed correctly:

- The oracle section is now `Regression suites carried into V2 delivery and parity` and explicitly says extraction/execution against V1 is not an additional prerequisite beyond the Road World Model architecture gate.
- Catastrophic cleanup was removed from the global Road World Model gate and is explicitly deferred as a blocker only for future collision-response/recovery stories; teleport, reinsertion and mid-road despawn remain unauthorized meanwhile.
- The proposal now uses the accepted commit/catalog as the reference, treats any tag/branch as optional archival hygiene, carries the accepted oracle into V2 work instead of rebuilding it, and removes a tag/branch plan from V2-1.

Still open:

- `epics.md` roadmap row 5 still says: “Establish the V1 behavioral oracle, then decide Road World Model schema ownership and responsibility contracts. No implementation stories are generated before this gate.” This presents the accepted oracle and Road World Model work as a combined planning gate, contradicting the clarified Epic 5 note and AD-36, which leave only the Road World Model/responsibility architecture gate open.

Required closure: rewrite roadmap row 5 to state that the V1 oracle is accepted and only Road World Model schema ownership/responsibility contracts remain gated before implementation-story generation. No other prior rubric finding remains open.

## Final closure recheck — 2026-09-22

**FINAL VERDICT: PASS.**

Epic 5 roadmap row 5 now treats the accepted V1 behavioral oracle as input and states unambiguously that only the Road World Model/responsibility architecture gate blocks implementation-story generation. This matches AD-36, the oracle's accepted-baseline decision, the responsibility-contract gate and the clarified Epic 5 course-correction note.

All findings from this gate-waiver rubric are closed. No additional V1 runtime, PlayMode, manual-recipe, branch or tag gate remains.
