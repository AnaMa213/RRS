# Road World Model Adversarial Compatibility Review

> **SUPERSEDED — pre-fix record.** This review describes the draft **before** the reviewer corrections. The current verdict lives in `review-road-world-model-postfix-adversary.md` (rerun 2026-09-22, verdict PASS). Kept as the history of what the first pass required.

**Scope:** Proposed AD-43 through AD-47 and the detailed Road World Model contract.  
**Method:** Construct independently built authoring, compiler, localization, junction and migration units that satisfy the written rules but cannot safely exchange data.  
**Verdict:** **NOT READY FOR OWNER ACCEPTANCE.** The model boundaries are sound, but four high-severity interoperability contracts remain underspecified.

## Critical findings

None.

## High findings

### H1 — Reciprocal references recreate multiple owners for topology, conflicts and signal membership

**Conforming build A:** `Junction` is authoritative for its movement/conflict membership; `JunctionMovement` and `ConflictZone` reverse references are compiler-derived indexes. `SignalPlan` owns group membership and movement bindings are derived.

**Conforming build B:** Each `JunctionMovement` authoritatively stores its junction, conflict zones and signal-group binding; each `ConflictZone` authoritatively stores member movements; `Junction` and `SignalPlan` lists are cached views.

Both builds contain every required field and can claim that each named record owns its stated concept. AD-43 and the ownership table, however, assign the same relationship to both ends:

- `Junction` owns movement/conflict-zone membership while movements and zones also reference that membership;
- `JunctionMovement` references conflict zones while `ConflictZone` owns member movements/corridors;
- `JunctionMovement` owns control-group binding while `SignalPlan` owns movement groups and movement IDs;
- `Junction` owns a declared control kind while each movement/approach owns its applicable control.

No rule says which side is authored truth, which side is derived, or what equality validation is mandatory. The two compilers can accept different graphs after an edit, and a junction coordinator built for one representation will misread the other.

**Required decision:** For every reciprocal relationship, name exactly one authored owner and make the reverse direction a compiled index. Define hard consistency validation. Also define the topology partition: a cross-junction traversal is a `JunctionMovement`; a `LaneConnection` may not encode the same traversal, and the junction-level control kind may only classify/validate movement controls rather than override them.

### H2 — Source identity and canonical model version permit incompatible reimport histories

**Conforming build A:** `SourceTraceKey` is a Unity object identity for each V1 node. When one node contributes to corridor geometry, a movement and a portal relation, the importer creates one trace record and attaches several semantic meanings below it.

**Conforming build B:** `SourceTraceKey` is composite: source object/edge identity plus semantic role and local discriminator. The same V1 node therefore maps to several stable semantic IDs while still avoiding a single key mapping to two live records.

Both preserve source-to-ID mappings, survive rename/reorder and never recycle IDs. They produce different IDs after split/merge/recreated-source edits and cannot consume each other's trace maps. The stated one-key-to-one-live-record failure conflicts with the migration requirement to trace nodes and edges into several possible semantic dispositions unless key granularity is fixed.

`RoadModelVersion` has a second ambiguity: “canonical stable-ID-ordered data” does not define the hash domain or canonical encoding for floating-point samples, optional/defaulted values, labels, source provenance, record ordering inside relations, or validation-profile/tolerance changes. Two valid compilers can hash semantically identical models differently, or fail to change version for a behaviorally relevant compiler/profile change.

**Required decision:** Define semantic source-key granularity and split/merge/remap rules, including whether one source may intentionally have role-qualified keys. Define the canonical version manifest: included records/fields, normalized units/coordinate frame, float quantization/byte encoding, set ordering, compiler/schema/profile version, and explicit exclusions such as diagnostic labels. Bind import maps and tombstones to a source identity/version.

### H3 — Curve and localization values lack enough semantic convention for independent consumers

**Conforming build A:** Positive lateral offset means left when looking along the tangent; signed curvature is positive for a left turn about road-up; `Project` clamps to `[0, Length]`; `WrongWay` takes precedence over `OutsideCorridor` when both apply.

**Conforming build B:** Positive lateral offset means right; curvature is unsigned magnitude; projection may return an extrapolated `s`; `OutsideCorridor` takes precedence and heading error is reported only for on-corridor candidates.

Both expose the required package-independent queries, metre arc length, tangent, road-up, curvature, asymmetric widths, statuses and alternatives. Their output is incompatible for adjacency, stop-line distance, steering, wrong-way handling and recovery localization.

The localization algorithm may remain open, but the value semantics cannot. `OnCorridor`, `OutsideCorridor`, `WrongWay`, `Ambiguous` and `Unlocalized` also lack mutually exclusive definitions/precedence, confidence range/meaning and a required deterministic tie-break for ordered alternatives.

**Required decision:** Fix the directed local frame and signs (`forward`, `up`, `left/right`), curvature convention, heading-error range/sign, `s` domain and endpoint/projection behavior, width-envelope inclusion rule, footprint reference, status predicates/precedence, confidence domain and stable alternative tie-break. Algorithms and thresholds may remain story-level where they do not change those meanings.

### H4 — Migration acceptance can approve selective semantic loss or an unbound report

**Conforming build A:** Reject several awkward V1 edges with documented reasons, preserve enough connectivity for every entry to reach an exit, author the required crossroads/T movements, and receive visual sign-off.

**Conforming build B:** Preserve every viable edge and turn-weight choice, recording explicit approved replacements only where the semantic model intentionally changes behavior.

Both satisfy “explicit disposition,” required movement counts, reachability, zero hard errors and human sign-off. Build A can still remove a historical route/turn choice or misassociate weights without failing the stated gate. Roundabout acceptance also has no exact per-instance entry/exit/movement count despite the inspected three-entry/exit source baseline.

The report is not required to identify the exact source commit/scene/prefab hashes, importer/compiler/schema/validation-profile versions, resulting `RoadModelId`/`RoadModelVersion`, overlay artifact hash or approver decision. A subsequent reimport can therefore drift while retaining an apparently valid earlier report.

**Required decision:** Make the acceptance report an immutable manifest bound to source revision/assets, import map version, compiler/schema/profile, resulting model ID/version, diagnostics, overlays and approval. A rejected source edge requires an explicit owner-approved replacement/obsolescence disposition and its behavioral consequence; preserved weighted choices must have traceable target movements/costs. Add exact roundabout/source-semantic coverage or explicitly approve deviations per instance.

## Safety and Recovery boundary check

**Pass, with one wording cleanup.** AD-43 through AD-47 do not give Safety or Recovery ownership of static road data, grants, plans or body motion. Runtime localization belongs to the Traffic Frame builder; Recovery only consumes its result. Static blocked-exit geometry is separated from dynamic occupancy/coordination, and current signal phase stays outside the Road World Model.

The detailed localization sentence saying `Unlocalized` returns “nearby recovery candidates” could be read as localization selecting recovery maneuvers. The typed contract only returns ordered corridor/location alternatives, so this is not currently a boundary breach. Rename these to “nearby localization alternatives” when applying the high-severity fixes to prevent later scope creep.

## Gate determination

AD-43 through AD-47 should remain `[ASSUMPTION]` until H1-H4 are resolved. The fixes do not require selecting C# classes, Unity asset nesting, a spline package, localization scoring weights or Safety/Recovery algorithms; they bind only the cross-unit meanings needed before independent implementation stories can be generated.
