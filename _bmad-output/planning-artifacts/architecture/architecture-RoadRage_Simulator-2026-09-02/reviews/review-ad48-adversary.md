# Adversarial Review — AD-48 "Lateral Cross-Section Order Is An Authored Fact On The Child"

Scope: AD-48 only (spine lines 396-400), read against AD-43, AD-44, AD-45, AD-46, AD-47, and the
"Lateral cross-section order (AD-48)" section of
`_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`
(lines 106-117). Goal: construct concrete scenarios where two units, each individually obeying
AD-48, build an incompatible model, or where AD-48 itself produces a wrong/unbuildable model.

## Verdict

AD-48 is structurally sound for the trivial case (one straight, one-way-per-lane, unforked
section) but has one genuine, unaddressed geometric bug (two-way sign inversion) and several
under-specified boundaries (curved/forked sections, roundabouts, arc-position comparability,
LaneAdjacency cross-check, reimport lineage risk) that will produce silently wrong or
irreconcilable models the moment a section is not a straight two-lane road. **Not safe to build
Stories 5.26/5.27 against as written without closing at least Findings 1, 2 and 3.**

---

## Finding 1 (CRITICAL) — CrossSectionAxis cannot be AD-45-right for both travel directions on a two-way section

AD-48 is explicit: "One total order spans the whole carriageway of a section with both travel
directions included, so a two-way road is one cross-section rather than two unrelated sequences."
`CrossSectionAxis` is "the unit direction in which `LateralOrder` increases" — one fixed 3D unit
vector for the whole section.

AD-45 independently fixes each corridor's own local frame: `right = cross(up, tangent)`, lateral
offset positive right, **per corridor**, derived from that corridor's own directed tangent.

Take a straight 4-lane two-way section: corridors A, B northbound, corridors C, D southbound.
A's tangent is north, so A's `right` (`cross(up, north)`) is east. C and D travel south, so their
own `right` (`cross(up, south)`) is **west** — the physically opposite direction, which is exactly
how a two-way road works (each direction's own right shoulder is on the opposite physical side).

Now author `CrossSectionAxis = east` and assign `LateralOrder` 0,1,2,3 to A,B,C,D west-to-east
(the only sane authoring, since the whole point of AD-48 is one physical order across the
carriageway). For A and B, ascending `LateralOrder` matches ascending along their own AD-45
`right` — consistent. For C and D, ascending `LateralOrder` (C=2 → D=3) moves **east**, which is
C's/D's own AD-45 **left**, not right. Any consumer that assumes "ascending `LateralOrder` =
toward my own right" (a natural reading, since `CrossSectionAxis` is presented as *the* frame
`LateralOrder` is measured in, right next to AD-45's right-positive convention) gets southbound
lane-side identification silently inverted: the inner (passing) southbound lane looks like the
outer (shoulder) lane and vice versa. For a driving simulator whose entire traffic-v2 concern is
lane discipline, overtaking and Road Rage lane violations, an inverted overtaking-lane read is a
correctness bug, not a cosmetic one.

AD-48's text never states which direction's `right` `CrossSectionAxis` is meant to agree with, nor
that it necessarily *disagrees* with one direction's AD-45 frame by construction on any two-way
section. Two consumer stories — one written by someone who reasons "ascending order = right for my
direction," another who reasons "ascending order = a arbitrary section-wide index, direction
must be checked separately" — will produce contradictory lane-side logic from the identical
compiled model, and both are individually AD-48-compliant.

**This is not a corner case; it is the default shape of a two-way road**, which is the primary
subject AD-48 was written to fix ("a two-way section has no adjacency path across it").

---

## Finding 2 (CRITICAL) — Roundabout circulating corridors have no single meaningful CrossSectionAxis

AD-46: "Roundabouts use ordinary circulating corridors plus explicit entry/exit movements and
yield conflicts." A circulating corridor's tangent rotates through 360 degrees along its own arc
length (it is a closed loop). Its AD-45 local `right` therefore also rotates through 360 degrees.

- **Single-lane roundabout:** the section has exactly one corridor. `LateralOrder = 0` is trivially
  unique/contiguous, and `LaneCorridor` has nothing to be laterally ordered against — no consumer
  needs a lateral relation here at all. Yet AD-48's validation still demands "every section
  referenced by at least one corridor carries a finite non-zero `CrossSectionAxis`," forcing the
  author to invent an arbitrary vector with no geometric referent and no consumer. This is exactly
  the "silently defaulted / carries no semantics" failure mode AD-48's own prevents-list was
  written to stop for `LateralOrder` — reintroduced for `CrossSectionAxis` in the degenerate case.
- **Multi-lane roundabout (2 circulating lanes, a real, common case):** a single fixed
  `CrossSectionAxis` vector is, by the same argument as Finding 1, only locally aligned with the
  corridors' actual `right` at one point of the circle, and points the opposite way at the
  diametrically opposite point. There is no arc position at which "the" axis is representative for
  the whole loop. AD-48 and AD-46 never cross-reference each other on this; neither document
  states whether roundabout sections are exempt, split per quadrant, or expected to tolerate a
  meaningless axis. Story 5.26's own check ("`CrossSectionAxis` agrees with the corridors' road
  frames... at comparable arc positions") is silently either vacuous or fails every multi-lane
  roundabout, and AD-48 gives no guidance either way.
- Circulating corridors are also closed curves (start = end), so "comparable arc position" between
  two circulating corridors (needed for the monotonicity check in Story 5.26) has no canonical
  zero — which corridor's start is "the" reference point is unspecified, compounding Finding 3.

---

## Finding 3 (HIGH) — "Comparable arc position" is used but never defined, across three independent failure modes

Both AD-48's spine rule and the linked contract section delegate geometric consistency to Story
5.26 via the phrase "corridor centrelines at **comparable arc positions**." AD-48 never defines
what makes two arc positions on two different corridors comparable, and at least three concrete
situations make the answer non-obvious and implementation-dependent:

1. **Unequal-length corridors in one section.** A section models a lane that starts or ends
   mid-carriageway (an acceleration/deceleration lane, a gore before a portal exit). Nothing in
   AD-43/AD-45/AD-48 requires every corridor referencing one `SectionId` to share the same arc
   length or a common parameterization. Past the point where the shorter corridor's domain ends,
   there is no corresponding `s` on it to compare against the longer corridors' `s`. A consumer
   could resolve this by nearest-endpoint clamp, by normalized fraction-of-length, or by treating
   the section as split at that point — three different, individually reasonable choices, none of
   which AD-48 picks, and which can produce different monotonicity verdicts on the identical model.
2. **Curved sections.** Per Finding 1's mechanism generalized to curvature: even within one travel
   direction, a corridor's local `right` rotates along a bending section. `CrossSectionAxis` is
   one fixed vector, so "agrees with the corridors' road frames" can only hold near one arc
   position. AD-48 does not say which position anchors the axis (section start? midpoint?
   per-corridor nearest projection?), nor what tolerance is acceptable as the road bends away from
   that anchor.
3. **Forked sections.** AD-43 does not require a section to end where a road physically forks. If
   a fork is modeled as one section whose corridors diverge into different headings, "the whole
   carriageway" and "comparable arc position" both lose their ordinary meaning past the fork point,
   and AD-48 gives no rule for whether that is even a legal authoring shape.

Because AD-48 states these are "structural checks requiring no curve mathematics" for the
integer-order half of the rule, but silently hands the geometry half to a separate story using an
undefined term, two teams implementing Story 5.26's check and a downstream lane-change/overtaking
consumer's own reading of `LateralOrder` are free to disagree about what "the model is
geometrically consistent" even means — exactly the two-units-same-AD-different-build failure this
review is asked to find.

---

## Finding 4 (HIGH) — No AD arbitrates LateralOrder vs. LaneAdjacency.LaneSide when they disagree

AD-43 gives `LaneAdjacency` sole ownership of "side, paired arc-length intervals and lane-change
legality." AD-48 gives `LaneCorridor`/`RoadSection` sole ownership of the integer lateral position
and its axis. Both are independently authored (AD-48 explicitly forbids deriving one from the
other: "never recovered from `LaneAdjacency`"), which means nothing stops them from being authored
inconsistently — e.g. `LaneAdjacency` declares corridor B is `Left` of corridor A while
`LateralOrder`+`CrossSectionAxis` (correctly per Finding 1, for whichever direction) says B sits on
A's geometric right.

Consequences:
- No validation code in AD-48's list (duplicate order / non-contiguous order / missing or
  degenerate axis) checks agreement with `LaneAdjacency.LaneSide`. A model where the two disagree
  passes validation cleanly.
- A lane-change/AI-policy story that trusts `LaneAdjacency.LaneSide` (the AD-43-designated owner of
  "side") and a HUD/overtaking-detection story that trusts `LateralOrder`+axis (AD-48's designated
  owner of lateral position) can render opposite left/right pictures of the same two corridors,
  and neither is violating its own governing AD.
- Separately, `LaneAdjacency` is arc-scoped ("paired arc-length intervals") while `LateralOrder` is
  section-global and constant regardless of `s`. Near a taper/gore, `LaneAdjacency` may correctly
  report "no adjacent lane" for the current `s` while `LateralOrder` still nominally has an
  adjacent-index corridor elsewhere in the section. AD-48 never states that `LateralOrder` must not
  be used, by itself, to infer live lane-change legality — that boundary is implied by AD-43's
  ownership split but not restated or enforced here, and a careless consumer reading only
  `LateralOrder` could treat a gore as a legal neighbor lane.

---

## Finding 5 (MEDIUM) — AD-44 reimport identity stability is only safe if lineage matching never keys on LateralOrder, and AD-48 never says so

Scenario: a 4-lane section has stable corridors A,B,C,D with `LateralOrder` 0,1,2,3. An author
inserts a new lane between B and C. The new corridor E gets `LateralOrder=2`; C and D — which are
otherwise untouched, lineage-identical lanes — must shift to 3 and 4. AD-48 is correct that this is
a legitimate `CompilerSchemaVersion`/`RoadModelVersion`-affecting change and explicitly allows it.

The risk is at the AD-44 boundary: AD-44 promises "a lineage-identical V2 entity keeps its stable
ID" on reimport, but explicitly leaves "how the mapping is persisted and compared" as free
implementation choice. `LateralOrder` is exactly the kind of field a naive or source-array-driven
lineage matcher would be tempted to use as (part of) its identity key, since V1/authoring sources
are often literal ordered lists. If an implementation does that, inserting one lane in the middle
mints new IDs for C and D (and everything downstream of the insertion point) even though they are
the same physical lanes — silently breaking AD-44's stability promise on what should be a routine
edit. Neither AD-44 nor AD-48 calls out that `LateralOrder` must be excluded from lineage-matching
keys precisely because AD-48 guarantees it will move under exactly this kind of edit. This is a
foreseeable trap that a future implementer could fall into while individually satisfying both ADs
as literally written.

(The V1→V2 migration report's disposal obligation under AD-47/Story 5.27 is not implicated here —
that is a one-time, one-way run, not the general reimport path AD-44 governs — but the identity
risk above sits squarely inside AD-44's "not V1-specific" reimport contract.)

---

## Finding 6 (LOW) — "Tie-break for a model that already failed validation" describes an output that may never exist

AD-48's last sentence: the compiler-derived corridors-by-section list uses ascending `RoadId` "as a
deterministic tie-break for a model that already failed validation." But duplicate/non-contiguous
`LateralOrder` is stated as a hard validation failure with its own code, and AD-44 states
"unresolved/duplicate IDs... are hard validation failures" in the same breath as version emission
being gated on validity. It is not specified anywhere whether an invalid model (duplicate
`LateralOrder`) ever reaches the point of having its derived corridors-by-section list built and
consumed by anything (debug tooling? the validator's own diagnostic dump?), or whether this clause
describes dead code — internal compiler state that, per the rest of the document's own rules,
never escapes to a consumer. Harmless as written, but worth a one-line clarification of who reads
a "failed" model's derived list, since "must not own a corridor collection" plus "hard failure"
elsewhere reads as "invalid models produce no consumable compiled output at all."

---

## Summary table

| # | Severity | One-line |
| - | -------- | -------- |
| 1 | CRITICAL | Single `CrossSectionAxis` cannot equal AD-45 `right` for both travel directions of a two-way section; ascending `LateralOrder` is rightward for one direction and leftward for the other, with no AD flagging the inversion. |
| 2 | CRITICAL | Roundabout circulating corridors have a rotating local frame with no valid single `CrossSectionAxis`, yet AD-48 still forces even a single-lane roundabout to author a meaningless non-zero axis; AD-46 and AD-48 never cross-reference this case. |
| 3 | HIGH | "Comparable arc position," relied on by Story 5.26's geometry check, is undefined across unequal-length corridors (tapers/gores), curved sections and forks — leaving independent implementations free to disagree. |
| 4 | HIGH | Nothing arbitrates or cross-validates `LateralOrder`+axis against `LaneAdjacency.LaneSide` when the two independently-authored facts disagree about which side a lane is on. |
| 5 | MEDIUM | AD-44's reimport identity-stability promise is only safe if lineage matching never keys on `LateralOrder`, a value AD-48 guarantees will shift on ordinary mid-order edits, and neither AD says so. |
| 6 | LOW | Unclear whether a model that already failed validation ever produces a consumed corridors-by-section list, given hard-failure gating stated elsewhere. |
