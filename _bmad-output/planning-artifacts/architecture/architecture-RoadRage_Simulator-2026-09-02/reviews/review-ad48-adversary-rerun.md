# Adversarial Review (RERUN) — AD-48 "Lateral Cross-Section Order Is An Authored Fact On The Child"

Scope: AD-48 only (spine lines 396-400), plus the "Lateral cross-section order (AD-48)" section of
`_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`
(lines 106-118), read against AD-43 through AD-47. Rerun against the rewritten AD-48, which
replaces the earlier draft's fixed `CrossSectionAxis` direction vector with a per-corridor
`IsCrossSectionDatum` flag (exactly one per section) and a locally-evaluated AD-45 road-right frame.

Prior review: `review-ad48-adversary.md` (five findings against the `CrossSectionAxis` draft).

## Verdict

The rewrite genuinely closes the two CRITICAL findings and the MEDIUM finding from the prior pass
— the fixed-vector sign inversion and the rotating-frame degeneracy are gone by construction, and
lineage matching is now explicitly told to ignore both authored fields. The two HIGH findings are
only partially closed: the new text defines "comparable arc position" for the taper/gore case it
names explicitly, but says nothing about forked sections, and it adds a contradiction check between
`LateralOrder` and `LaneAdjacency.LaneSide` without specifying how to decide that check when the
two facts are grounded differently (one arc-local, one interval-static). The rewrite also opens one
new CRITICAL gap of its own: nothing requires the datum corridor to span the section's full arc
extent, so any corridor with zero arc overlap with the datum is assigned its `LateralOrder` with no
geometric grounding and no validation check ever fires for it — exactly the "order authored with no
consumer-visible meaning" failure AD-48 exists to prevent, reopened for a shape AD-48 itself permits.

---

## Part 1 — Closure of the five prior findings

**Finding 1 (CRITICAL, two-way sign inversion) — CLOSED.**
The rewrite states the inversion explicitly and forecloses the misreading that caused it: "For a
corridor travelling opposite the datum, ascending `LateralOrder` runs toward that corridor's own
road-*left*... a driver-relative consumer... resolves side through the corridor's own AD-45 frame
and never by comparing `LateralOrder` across corridors of opposite travel direction." This is not
just acknowledgment — it is mathematically consistent: `cross(up, -tangent) = -cross(up, tangent)`,
so "opposite corridor's own left" is provably the same physical side as "datum's own right." A
consumer that ignores this and still compares `LateralOrder` across opposite-direction corridors as
if it were a driver-relative side is, by the rule's own text, not AD-48-compliant — the divergence
this finding depended on is no longer available to a compliant implementation.

**Finding 2 (CRITICAL, roundabout rotating frame / degenerate axis) — CLOSED.**
`CrossSectionAxis` (the single fixed vector) is gone. The frame is now "evaluated locally at each
arc position," stated to exist precisely because "a single authored direction would go degenerate"
on a bend, fork or roundabout. The single-lane-roundabout sub-case (forcing an author to invent a
meaningless non-zero axis) no longer applies either: there is no axis to invent, and `LateralOrder`
`0` / datum `true` on the sole corridor is trivially valid. The closed-curve "no canonical zero"
objection is answered by the new comparison rule (projection onto the datum's own curve, not
matching "start" points across corridors) for the general case — see Part 2 for a residual,
narrower version of this problem the rewrite does not cover.

**Finding 3 (HIGH, "comparable arc position" undefined) — PARTIALLY CLOSED.**
Two of the three named sub-cases are now explicitly defined: "Lateral comparison is defined by
projection onto the datum corridor's curve at the datum's arc coordinate, and is made only over arc
intervals where both corridors are defined — which is what makes a taper, a gore or a lane that
starts mid-section comparable at all." This closes the unequal-length-corridor case by name, and the
curved-section case is closed as a side effect of the frame now being local rather than anchored at
one point. The **forked-section** sub-case is not addressed anywhere in the rewrite — AD-43 still
does not require a section to end at a fork, and the rewrite never says whether a fork inside one
section is legal authoring, nor what "projection onto the datum's curve" means once corridors
diverge in space rather than merely differing in arc-length domain. This gap is not cosmetic: it is
the direct ancestor of new Finding A below.

**Finding 4 (HIGH, no arbitration between `LateralOrder` and `LaneAdjacency.LaneSide`) — PARTIALLY
CLOSED.** The rewrite adds an explicit arbitration rule that did not exist before: "where the two
disagree the model fails validation rather than one silently winning," restated in the contract as
"`LateralOrder` must not contradict `LaneAdjacency.LaneSide`... a disagreement is a validation
failure, not a correction." The prior complaint — that nothing arbitrates — is answered: something
now does. What remains unspecified is *how* the check decides agreement/disagreement, given
`LaneAdjacency.LaneSide` is a single label over an arc-length interval (AD-43) while `LateralOrder`'s
geometric meaning is local-per-arc-position (projection onto the datum, "evaluated locally at each
arc position"). See new Finding C.

**Finding 5 (MEDIUM, `LateralOrder` as an accidental lineage key) — CLOSED.**
The rewrite adds the sentence the prior review said was missing, verbatim in spirit: "Both fields
are authoring-mutable, so lineage matching never keys on either. Inserting a lane mid-order shifts
every sibling's `LateralOrder`: that is a version change, never an identity change, and the stable
IDs of the shifted corridors are untouched." This directly forecloses the naive source-array-order
lineage matcher this finding warned about.

**Finding 6 (LOW, dead-code tie-break for an already-failed model) — STILL OPEN.**
The clause survives almost unchanged: "ascending `RoadId` retained solely as a deterministic
tie-break for a model that already failed validation." Nothing in the rewrite clarifies whether a
model that has already hard-failed validation (duplicate/non-contiguous order, missing/multiple
datum) ever has its corridors-by-section list built and read by anything. Untouched by this pass;
still low severity.

---

## Part 2 — Fresh attack on the rewritten shape

### New Finding A (CRITICAL) — A datum that does not span the section's full arc extent leaves other corridors with no geometry to be ordered against

Nothing in AD-48 or the contract requires the datum corridor's arc-length domain to cover the whole
section. The datum is chosen "deliberately independent of which corridor holds order `0`" — but nothing
constrains it to be the longest, or a through, corridor either.

**Scenario.** Section spans `s = [0, 500]`. The author picks a short merge/gore corridor `M`
(`s = [200, 260]`) as the datum (perhaps because it is the corridor Story 5.27's importer happened
to designate). Two through corridors exist: `A` on `s = [0, 500]` and a shoulder/parking lane `G` on
`s = [0, 50]` only, and a deceleration lane `H` on `s = [450, 500]` only. `G` and `H` never overlap
`M`'s domain `[200, 260]` at all.

Per the contract's own comparability rule, "made only over arc intervals where both corridors are
defined" — for the pair `(G, M)` and `(H, M)` that interval is **empty**. Story 5.26's monotonicity
check ("corridor centrelines must be strictly monotone in ascending `LateralOrder`... over those
intervals") has nothing to iterate over for `G` or `H` against the datum, so it is vacuously
satisfied regardless of what integers `G` and `H` are assigned. The structural check ("unique and
contiguous from `0`") only constrains the *set* of integers used section-wide, not which physical
corridor gets which one. Two independently-compliant authoring passes over the identical physical
geometry can assign `G = 0, H = 4` or `G = 4, H = 0` and both pass every validation code AD-48
defines — yet a lane-change/overtaking consumer trusting `LateralOrder`'s section-wide ordering will
read `G` and `H` on physically swapped sides in one of the two builds. This is precisely the "order
inferred... or silently defaulted" failure mode AD-48's own prevents-list targets, reopened for any
corridor whose arc domain happens not to intersect the datum's — a shape the rewrite explicitly
permits and does not flag as illegal, and Story 5.26 ("owns consistency only... never derives,
reorders or repairs") has no mandate to catch it because there is no comparable interval for it to
inspect.

### New Finding B (HIGH) — Nearest-point projection onto the datum curve is not stated to be canonical, and is not guaranteed monotonic on non-trivial shapes

The comparison method is "projection onto the datum corridor's curve at the datum's arc coordinate."
Left unstated: projection *by what rule* — presumably nearest Euclidean point, but that is not said,
and nearest-point projection onto a curved (let alone closed/circulating) curve is not globally
single-valued. On an S-curve, near the inflection a point can sit equidistant from two different arc
positions on the datum; on a circulating datum (roundabout), a point near the loop's geometric center
or where the loop nearly overlaps itself has the same ambiguity. Two Story 5.26 implementations that
resolve the tie differently (first candidate vs. arc-continuity-preferring candidate) can reach
different monotonicity verdicts — pass vs. fail, or a different ordering being "the" violation — on
the identical compiled model. AD-48 states the projection exists but not that it is well-defined
everywhere corridors and datum in fact occur; roundabouts are the case load-bearing enough that AD-46
was written for them, and the rewrite's own justification for going local ("around a roundabout's
circulating corridors") is the same geometry where this ambiguity is most likely to occur.

### New Finding C (HIGH) — The `LateralOrder`-vs-`LaneAdjacency.LaneSide` contradiction check mixes an interval-static fact with an arc-local one, and AD-48 does not say how to reduce one to the other

`LaneAdjacency` owns "overlapping source/target arc-length **intervals**" and a `LaneSide` presumably
constant over that interval (AD-43). `LateralOrder`'s geometric meaning is local: "evaluated locally
at each arc position." On a section that bends enough within one `LaneAdjacency` interval, the
physical left/right relationship the datum frame implies between two corridors can in principle rotate
across that interval even though the pair's width envelopes never cross (Story 5.26 forbids envelope
overlap, not frame rotation). Neither AD-48 nor the contract states which arc position within the
shared interval the contradiction check samples — interval start, midpoint, every sample, worst-case?
— so two compliant validators can reach opposite AGREE/CONTRADICT verdicts for the same model. This is
a narrower, more precise version of prior Finding 4's residual gap: the rewrite added the check but
not its decision procedure.

### New Finding D (MEDIUM) — Datum choice between concentric same-sense circulating corridors is safe by construction, but AD-48 never states the constraint that makes it safe, and nothing validates it

Worked the geometry: for two circulating corridors that share the same rotational sense (both flow
clockwise, say), `cross(up, tangent)` at a comparable angular position points the same physical
direction regardless of which corridor's radius is used, so choosing the inner or the outer lane as
datum does **not**, by itself, flip the resolved order — the concern the review brief raised does not
materialize for the ordinary case. It would materialize if a section ever contained circulating
corridors of **opposite** rotational sense (a mis-imported or legacy bidirectional service lane sharing
a roundabout section, or a data error from Story 5.27's importer). AD-48 has no validation code that
checks rotational-sense agreement among corridors sharing a section before trusting the datum frame,
and nothing elsewhere in the Road World Model rules this configuration out. Low-probability, but silent
if it occurs, and no line of AD-48 acknowledges the constraint its own correctness is quietly relying on.

### New Finding E (LOW) — "Exactly one datum per section" for a single-corridor section is well-defined but unstated as a degenerate case

For a section with exactly one corridor, that corridor is trivially the only candidate datum, and
`LateralOrder = 0` / `IsCrossSectionDatum = true` satisfies every validation rule with no geometric
content behind it (there is nothing to project onto or order against). This is not a bug — the rule
composes correctly — but AD-48 never says so explicitly, unlike the taper/gore and roundabout cases it
does call out by name. An author or import-tool implementer could reasonably wonder whether a
single-corridor section needs a datum at all, or could special-case it out of habit and trip the
"exactly one" structural check for a reason unrelated to any real authoring mistake. Worth one
clarifying clause; no rule change required.

---

## Summary table

| # | Status/Severity | One-line |
| - | --- | --- |
| Prior 1 | CLOSED | Two-way sign inversion: rewrite states the inversion and proves it consistent; `LateralOrder` can no longer be compared across opposite-direction corridors under a compliant reading. |
| Prior 2 | CLOSED | Rotating-frame degeneracy: fixed `CrossSectionAxis` is gone; frame is evaluated locally per arc position, closing both the single-lane and multi-lane roundabout objections. |
| Prior 3 | PARTIALLY CLOSED | Taper/gore and curved-section comparability now explicitly defined via datum-curve projection; forked sections still never addressed. |
| Prior 4 | PARTIALLY CLOSED | Arbitration rule added ("disagreement is a validation failure"); the decision procedure for detecting disagreement between an interval-static and an arc-local fact is still unspecified. |
| Prior 5 | CLOSED | Rewrite explicitly excludes both `LateralOrder` and the datum flag from AD-44 lineage-matching keys. |
| Prior 6 | STILL OPEN | Tie-break clause for an already-failed model's derived list is unchanged; still unclear if that output is ever consumed. |
| New A | CRITICAL | Datum need not span the section's full arc extent; corridors with zero arc overlap with the datum get `LateralOrder` with no geometric grounding and no validation ever checks them. |
| New B | HIGH | "Projection onto the datum curve" is not stated to use a canonical nearest-point rule, and nearest-point projection is not globally single-valued on curved/circulating datums. |
| New C | HIGH | `LateralOrder` vs `LaneAdjacency.LaneSide` contradiction check has no stated sampling rule for reducing an arc-local fact to an interval-static one; two validators can disagree. |
| New D | MEDIUM | Datum choice between concentric same-sense circulating corridors is safe by construction, but AD-48 never states or validates the same-rotational-sense assumption it relies on. |
| New E | LOW | Single-corridor sections trivially satisfy "exactly one datum" with no geometric content; unstated as a degenerate case, unlike the other named special cases. |
