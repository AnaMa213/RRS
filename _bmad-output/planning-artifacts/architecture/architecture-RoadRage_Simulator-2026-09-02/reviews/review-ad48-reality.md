# Reality-lens review — AD-48 "Lateral Cross-Section Order Is An Authored Fact On The Child"

Scope: verify AD-48 (ARCHITECTURE-SPINE.md) and its companion section in
ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md against the code Story 5.25 actually shipped in
`Assets/RoadRage/Features/Vehicles/Traffic/`. Nothing else in the spine was reviewed.

## Per-claim verification

### 1. "Derived corridors-by-section list is ordered by opaque RoadId today" — TRUE

`CompiledRoadModel.Freeze()` sorts every derived inverse collection, including
`_corridorsBySection`, with `Array.Sort(values)` on a `RoadId[]` — `Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs:250` (build call) and
`CompiledRoadModel.cs:459` (the actual sort). `RoadId.CompareTo` (`RoadModelRecords.cs:151-164`) orders
by the raw `_high`/`_low` halves of a `Guid.NewGuid()`-derived value with no semantic content — genuinely
opaque. `Story525RoadWorldModelTests.cs:1345-1358` (`InverseCollectionsAreOrderIndependent`) empirically
confirms `GetCorridorsInSection` is stable under source shuffling, i.e. driven by this RoadId sort, not
authoring order.

### 2. "LaneAdjacency genuinely cannot express a cross-section order spanning both travel directions" — TRUE IN INTENT, UNENFORCED IN CODE

The `LaneAdjacency` struct (`RoadModelRecords.cs:404-420`) has `FromCorridorId`, `ToCorridorId`, `Side`,
paired s-intervals and a permission — nothing in the shape itself encodes travel direction, so nothing
stops two opposite-direction corridors from being wired into one `LaneAdjacency` record. `RoadModelValidator.cs`
validates `LaneAdjacency` only for reference resolution and finite s-values
(`RoadModelValidator.cs:214-223`) — there is no check anywhere in the validator that rejects an
adjacency between opposing-direction corridors. AD-47's "opposing lanes never become lane-change
adjacency" is stated as an importer/authoring policy (AD-47's rule text), not as a compiled invariant
Story 5.25 enforces. So the claim holds only as long as every producer of `LaneAdjacency` (today: none
exist yet — there is no importer) honors that unenforced policy; the code does not itself make crossing
"impossible," only unauthored.

### 3. "CompilerSchemaVersion is 1, no compiled asset/importer exists, so the bump is free" — TRUE

`RoadModelCompiler.CompilerSchemaVersion = 1` at `RoadModelCompiler.cs:28`. Repo-wide search for a
compiled-model ScriptableObject/importer (`RoadWorldModelAsset`, `RoadModelAsset`, `RoadModelImporter`,
or any `ScriptableObject` wrapping `RoadModel`) returns nothing under `Assets/RoadRage`. No
`LateralOrder`/`CrossSectionAxis` symbol exists anywhere in the codebase yet either — AD-48 is a pure
forward decision, not yet implemented, which is consistent with it saying the bump "is free only while
no compiled model has shipped."

### 4. "Corridors and sections are already written in stable RoadId order, so order-independence holds" — TRUE

`RoadModelCanonicalWriter.Write` sorts sections by `Id` (`RoadModelCanonicalWriter.cs:121`) and
corridors by `CorridorId` (`RoadModelCanonicalWriter.cs:133`) via `SortedCopy` before writing any field,
regardless of source array order. Adding `LateralOrder` as one more per-corridor scalar value (not a
sort key) would not disturb this: the write order is still keyed on opaque `RoadId`, so two models with
the same corridors in different authoring order still hash identically. Claim is well-founded.

### 5. "EffectiveLaneCorridor mediates the canonical payload — does AD-48 account for that?" — GAP, NOT CALLED OUT

`RoadModelCanonicalPayload.Corridors` is typed `EffectiveLaneCorridor[]`
(`RoadModelCanonicalWriter.cs:19`), not `LaneCorridor[]`. `EffectiveLaneCorridor`
(`CompiledRoadModel.cs:83-92`) is a hand-maintained projection built field-by-field in
`RoadModelCompiler.ResolveEffectiveCorridors` (`RoadModelCompiler.cs:104-134`), which currently copies
`CorridorId`, `SectionId`, `LengthMeters`, `Samples`, and the three section-default-resolved fields —
nothing else. For `LateralOrder` to actually "enter the AD-44 canonical payload" as AD-48 asserts, three
additional call sites need a matching new field, none of which AD-48's text names:
`EffectiveLaneCorridor` itself, the copy in `ResolveEffectiveCorridors`, and the write in
`RoadModelCanonicalWriter.Write` (`RoadModelCanonicalWriter.cs:133-144`). This is a real but minor
precision gap: AD-48 frames the change as "the child owns its own position" (a single new field on
`LaneCorridor`) when the actual compiled/hashed surface touches three distinct types across two files.
Not a false claim, but understated implementation surface.

### 6. "5.25's validator does structural/finiteness checks only and owns no curve mathematics" — TRUE

`RoadModelValidator.cs` end to end performs only: id emptiness/duplication, reference resolution,
`float.IsNaN`/`IsInfinity` finiteness (`CheckFinite`, `RoadModelValidator.cs:502-518`), strictly-increasing
`s` on sample arrays (a structural ordering check on an authored scalar, not curve math —
`RoadModelValidator.cs:544-550`), signal/control/conflict membership bookkeeping, and manifest
consistency. There is no distance, intersection, width, or projection computation anywhere in the file —
confirms the claim and confirms AD-48's structural-only validation plan (uniqueness/contiguity of
`LateralOrder`, finite non-zero `CrossSectionAxis`) is the same category of check already used
elsewhere, not a new capability the validator would need to grow.

### Bonus corroboration: the "known defect" self-reference is accurate

AD-48 cites the "already-catalogued `SignalizedControlWithoutPlan` bundling" as the anti-pattern it
refuses to repeat. Checked: `RoadModelValidationCode.SignalizedControlWithoutPlan` is in fact reused for
five distinct failure conditions — missing phases (`RoadModelValidator.cs:586`), non-positive phase
duration (`:609`), empty phase (`:618`), missing/non-unique plan for a signalized control (`:653`), and
miscovered signalized movement (`:681`). The self-reference is factually accurate, which supports (rather
than undermines) AD-48's stated intent to give `LateralOrder`/`CrossSectionAxis` failures their own
distinct codes.

## Summary

AD-48's factual claims about the current Story 5.25 code are almost entirely accurate: the opaque
`RoadId` ordering of the derived corridors-by-section list, freedom to bump `CompilerSchemaVersion`,
order-independence of the canonical hash, and the structural-only scope of the validator all check out
against real file:line evidence. Two findings temper it: the "LaneAdjacency genuinely cannot" claim
currently rests on an unenforced authoring policy rather than a code-level guarantee, and AD-48
understates the implementation surface by not naming the `EffectiveLaneCorridor` indirection that
`LateralOrder` will have to be threaded through to actually reach the canonical payload.
