# Road World Model Adversarial Compatibility Review (post-fix rerun)

**Scope:** proposed AD-43 through AD-47 and the detailed Road World Model contract, after the reviewer corrections. Supersedes `review-road-world-model-adversary.md`, which described the pre-fix draft.  
**Method:** construct independently built authoring, compiler, localization, junction and migration units that each satisfy the written rules but cannot safely exchange data; then attack the corrected text.  
**Verdict:** **PASS for the architecture gate — no critical or high interoperability finding remains.** Every conforming-pair construction attempted in this rerun is either closed by the current text or is an intended, version-visible profile difference.

## H1–H4 closure

### H1 — relationship ownership — Closed

Each relation now has exactly one authored side: persisted foreign keys on child records are the sole parent/child truth, inverse collections such as "movements in junction" and all hot indexes are compiler outputs that cannot be authored. `JunctionControl` alone owns approach control and stop/yield line; `ConflictZone` alone owns conflict membership; `SignalPlan` groups alone own signal membership and phases. `Junction` owns identity, boundary and feature classification, with approach control, conflict membership, grants, phase and route choice on its `Must not own` list. The topology partition is now explicit: a traversal that crosses a junction is **always** a `JunctionMovement` and never a `LaneConnection`, and junction-wide classification may validate movement controls but never override them. The previous pair — build A deriving membership from the junction, build B authoring it on movements — can no longer both conform.

### H2 — source identity and version — Closed

Source keys are qualified by source asset, instance, object/component and relation endpoint, and lineage is typed many-to-many: several nodes may form one corridor, one node may seed several role-qualified records, and one source key may legitimately trace to multiple semantic records. Uniqueness is enforced on semantic IDs and importer slots, not on lineage edges, so split/merge/remap no longer has two incompatible answers. `RoadModelVersion` has an explicit include list, an explicit exclusion list (labels, source trace, editor metadata, order, rebuildable indexes) and a single emitter; consumers compare and never recompute. Validation-profile values are inside the payload, so a tolerance or maximum-footprint change is a new version rather than a silent divergence.

### H3 — value semantics — Closed

The frame is fixed: `forward = tangent`, `up = road-up`, `right = normalize(cross(up, forward))`; lateral offset positive to road-right; normal offset positive up; heading error signed about road-up in `[-180, 180]` with `90 degrees` as the wrong-way boundary; signed curvature positive toward road-right; `s` clamped to `[0, Length]` with no extrapolation; bounds covering the full width envelope. Flag predicates are defined independently of acceptance, `localized=false` excludes element identity, `confidence` is a deterministic `[0, 1]` value from the accepted score margin, ties break on stable-ID order after geometric ties are returned, and hysteresis bands remain profile data.

### H4 — migration acceptance — Closed

The report is an immutable artifact bound to source dependency hash, import map, importer/compiler version, `RoadModelId` and `RoadModelVersion`; a stale or hand-detached report is rejected, not repaired. Every source item is disposed, including rejections and merges, a preserved turn weight must resolve to a named target movement or route choice, every behavioural change carries an owner-approved disposition and its consequence, recorded smoothing overrides are counted with their maximum deviation, roundabout entry/exit relations are disposed per instance, and the overlay sign-off records approver identity and artifact hash. Selective semantic loss can no longer pass as "explicit disposition".

## Pairs constructed in this rerun

| Pair | Incompatible outcome | Disposition |
| --- | --- | --- |
| P1 — bounds of centerline samples versus bounds of the swept envelope | spatial index misses edge-adjacent candidates in one build | Closed: bounds must contain the full width envelope. |
| P2 — movement width inherited from the approach corridor versus authored locally | different movement envelopes, therefore different conflict geometry, from identical sources | Closed: the movement owns its width profile, seeded but never silently inherited, with compiler-validated lateral continuity at both seams. |
| P3 — turn weight preserved as preference versus as cost | identical ratios, opposite routing outcome | Closed: the direction is explicit — higher means more preferred. |
| P4 — `Ambiguous` depending on whether a candidate was accepted | two builds report different flags for the same geometry | Closed: `Ambiguous` describes the candidate set, not the acceptance outcome. |
| P5 — import a `LaneConnection` across a junction instead of a movement | two different runtime graphs for one junction | Closed: the traversal is always a `JunctionMovement`. |

## Residual risks accepted, with visibility

- **Localization scoring weights and hysteresis bands stay profile data.** Two profiles can classify a boundary pose differently. This is intended — the meanings are fixed, the numbers are not — and it is visible because profile values are inside the version payload: a different profile is a different model version, not a silent disagreement.
- **Conflict-candidate generation stays tooling.** Only materialized, reviewed and versioned `ConflictZone` records are consumed at runtime, so a different generator cannot change behaviour without passing review and a version change.
- **Smoothing overrides remain an escape hatch**, now bounded by mandatory counting and maximum-deviation reporting rather than by an absolute cap. The first migration report is the evidence that closes it; recorded as an owner-visible note, not a blocker.
- **Catastrophic out-of-world cleanup ownership is still deferred.** It gates only later collision-response and recovery stories, and it cannot affect a compiled road model, so it does not weaken this gate.

## Safety and Recovery boundary

No change weakens the boundary. Localization belongs to the Traffic Frame builder and only returns locations, flags, confidence and ordered alternatives; the previous wording that could be read as selecting recovery maneuvers ("nearby recovery candidates") is now "nearby localization alternatives". Static blocked-exit geometry stays separated from runtime occupancy and coordination, and current signal phase remains outside the Road World Model.

## Gate determination

AD-43 through AD-47 may go to owner review as `[ASSUMPTION]`. Nothing in this review adopts them, and the architecture gate stays formally open until the owner accepts or corrects.

**Status note (added after the acceptance turn):** the owner accepted AD-43 through AD-47 on 2026-09-22 with two clarifications; see `review-road-world-model-clarification-rerun.md` for the post-acceptance rerun and the final state.
