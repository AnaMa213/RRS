# AD-48 Rubric Review — Lateral Cross-Section Order

Scope: AD-48, the AD-43 in-place amendment, and the "Lateral cross-section order (AD-48)"
section of `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`. Whole-spine review out of
scope.

## 1. Enforceability vs. the Prevents line

AD-48's Rule maps cleanly onto each item in its own Prevents line:

- *"Opaque `RoadId` order passing as lane semantics"* — prevented: `LateralOrder` is the only
  order with semantics; `RoadId` is demoted to a tie-break used solely on a model that has
  already failed validation, so it can never substitute for `LateralOrder` in a passing model.
- *"An authorable reciprocal corridor list reappearing on `RoadSection`"* — prevented
  structurally: the Rule and the contract doc both state twice that `RoadSection` carries no
  corridor collection, authored or reciprocal. There is no field for such a list to reappear in.
- *"The order being inferred at runtime or silently defaulted during migration"* — prevented
  indirectly rather than by a dedicated check. There is no explicit "was this value authored vs.
  defaulted" validation code; instead, a silent default (e.g., migration leaving every corridor in
  a section at `LateralOrder = 0`) is caught as a side effect of the uniqueness/contiguity checks,
  and Story 5.26's "never derives, reorders or repairs" clause closes the runtime-inference path.
  This is sound but relies on two other clauses doing the work the Prevents line credits to the
  Rule itself — acceptable, not a defect.
- *"Two models with different lane orders hashing identically"* — prevented by folding
  `LateralOrder`/`CrossSectionAxis` into the AD-44 canonical payload, with the `CompilerSchemaVersion`
  bump making the inclusion legal. Enforcement is real but depends on the compiler actually doing
  this, which is implementation, not architecture — appropriately left there.

Overall: enforceable, and the Rule does what it claims to prevent.

## 2. Cross-story compatibility (5.26 geometry / 5.27 migration / later lane-change)

One real ambiguity survives the letter of AD-48: the contract's Story 5.26 clause requires
corridor centrelines to be "strictly monotone along the axis in ascending `LateralOrder`" at
"comparable arc positions." For a two-way section, `LateralOrder` spans both travel directions in
one total order, so the monotonicity check must relate corridors whose arc-length domains run in
opposite physical directions and need not share a common origin. "Comparable arc position" is
never defined for that opposing-direction case (nearest-point correspondence? matched normalized
`s`? projected global position?). Two conforming Story 5.26 implementations could pick different
correspondences and both pass validation while disagreeing on what "monotone" means across the
median — obeying AD-48 to the letter while building incompatible geometry validators. This is a
gap in the Story 5.26 contract clause that AD-48 introduces the need for, not a contradiction
within AD-48 itself.

No other incompatibility surfaced between 5.26, 5.27, and a hypothetical lane-change story:
lane-change/overtaking adjacency is explicitly kept on `LaneAdjacency` (AD-43), and AD-48 is
explicit that `LateralOrder` must never be used to recover it, closing the obvious temptation to
let a lane-change story infer adjacency from numeric order proximity.

## 3. Consistency with inherited ADs

No weakening or contradiction found against AD-43 (explicitly deferred to by the amended text),
AD-44 (reasoning checked separately below), AD-45 (axis unit-length and geometric agreement
explicitly deferred to AD-45's invariants), AD-46 (untouched), or AD-47 (AD-48 explicitly
reaffirms "opposing lanes never become lane-change adjacency" and builds on it rather than around
it). A grep of the whole spine for `RoadSection`/`LaneCorridor`/`LateralOrder`/`CrossSectionAxis`
confirms no stray leftover text still asserts the old ordered-list claim anywhere else in the
document — the fix is isolated to AD-43 and AD-48, as claimed.

## 4. Is amended AD-43 free of its original contradiction?

Yes. The old claim that `RoadSection` carries *ordered* `LaneCorridor` IDs is gone. Current text:
"Child foreign keys are persisted truth; inverse parent collections and hot indexes are
compiler-derived only, and the ordering of a derived collection carries no semantics unless
another `AD` supplies the authored fact it is ordered by — lateral cross-section order is supplied
by AD-48." This states the FK-only truth and the "no derived order has meaning by default" rule
in the same sentence, with AD-48 as the sole named exception — the self-contradiction (ordered
parent list vs. FK-only truth) cannot recur because there is no ordered parent list left to
contradict the FK-only claim.

## 5. Silent dimensions

- **Single-corridor section**: resolves trivially (`LateralOrder = 0` alone is unique and
  contiguous from 0). Not a real gap.
- **Corridor whose `SectionId` resolves to nothing**: not addressed by AD-48 directly, but already
  covered by AD-44's general clause ("unresolved/duplicate IDs or cross-version references fail
  validation"). Not a gap specific to AD-48.
- **`LateralOrder` under corridor split/merge at import (AD-44 lineage rules)** — genuinely silent.
  AD-44 requires "explicit remap" when sources are grouped/split, and Story 5.27 requires
  `LateralOrder` to be populated and disposed in the migration report, with "a section whose
  corridors cannot be totally ordered... is a hard failure." But nothing says what happens to the
  *other, already-numbered* corridors in the same section when a split or merge changes the
  corridor count: contiguity from 0 with no gaps means every sibling's `LateralOrder` may need to
  shift, and neither AD-48 nor the contract's Story 5.27 clause says who renumbers, when, or
  whether a partial re-import that touches one corridor in a section is allowed to leave the rest
  un-renumbered mid-pass. This is exactly the kind of underspecified interaction two Story 5.27
  implementations could resolve differently while both satisfying the letter of AD-48.

## 6. Soundness of the AD-44 "not record order" reasoning

Sound. AD-44 excludes *record order* (iteration/serialization order with no meaning) from the
canonical payload but requires anything behaviorally meaningful to be included. The contract's own
test — "two models whose lanes sit in a different cross-section order must not hash identically
once a consumer branches on that order" — is the correct discriminator, and it is consistent with
AD-48's own validation semantics (a lane-change/overtaking consumer does branch on `LateralOrder`
adjacency-by-rank even though it must not treat it as adjacency itself). The order-independence of
the canonicalization algorithm as a whole is preserved because `LateralOrder`/`CrossSectionAxis`
are serialized as *values* on/near each record, not encoded via traversal order — so there is no
circularity between "the hash is order-independent" and "lane order is a hashed value."

## Verdict

AD-48 is enforceable, resolves the AD-43 contradiction cleanly, and is consistent with AD-44
through AD-47. Two real gaps remain below the AD, both in the Story-level contract text AD-48
delegates to, not in AD-48's own Rule.

## Findings

1. **[high]** Neither AD-48 nor the Story 5.27 contract clause specifies how sibling corridors'
   `LateralOrder` get renumbered (or whether they must be) when a corridor is split or merged
   during re-import under AD-44's lineage/remap rules, leaving contiguity-preservation undefined
   across that interaction.
2. **[medium]** The Story 5.26 contract clause requires centrelines to be monotone along
   `CrossSectionAxis` at "comparable arc positions," but never defines correspondence between
   opposite-direction corridors sharing one two-way total order, letting two conforming
   implementations disagree on what the check even means at the median.
3. **[low]** The Prevents-line item "order being inferred at runtime or silently defaulted during
   migration" is enforced only indirectly, by the uniqueness/contiguity checks and Story 5.26's
   "never repairs" clause, rather than by a dedicated validation code of its own — worth noting for
   traceability even though the net effect is sound.
4. **[low]** `CrossSectionAxis`'s relationship to AD-45's per-corridor `right = cross(up, tangent)`
   convention is left to Story 5.26's "agrees with the corridors' road frames" check without
   defining what "agrees" means (parallel to one canonical direction's right vector? something
   else?) — minor, deferred cleanly, but worth tightening before Story 5.26 starts.

Full review: `_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/reviews/review-ad48-rubric.md`
