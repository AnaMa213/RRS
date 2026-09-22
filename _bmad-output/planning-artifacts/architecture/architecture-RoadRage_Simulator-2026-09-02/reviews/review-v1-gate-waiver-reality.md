# V1 baseline gate-waiver reality review

**Date:** 2026-09-22  
**Scope:** the narrow project-owner decision across `ARCHITECTURE-SPINE.md`, `V1-BEHAVIORAL-ORACLE.md`, `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`, and `sprint-change-proposal-2026-09-21.md`.  
**Decision under review:** Story 5.14 is complete; commit `210f48811e3f99fbb93c5d2aa75885e65edcf878` plus existing tests/artifacts/catalog is the accepted V1 baseline; no additional V1 runtime replay, PlayMode-harness repair, branch, or tag gates Road World Model design; collision response and physical recovery remain unresolved V2 requirements.

**Verdict: PASS WITH REQUIRED REVISIONS.** The spine and responsibility contract represent the owner decision accurately, and the oracle preserves the collision/recovery gaps. The proposal still contains one obsolete sequential work package that can recreate the waived oracle/tag gate, and the oracle has one heading that can be read as a new pre-implementation harness gate.

## Reality evidence

- Repository `HEAD` is the accepted baseline commit `210f48811e3f99fbb93c5d2aa75885e65edcf878`; the commit object exists.
- Story 5.14's implemented scope remains the shared `VehicleDriveIntent` / `VehiclePhysicsBody` path. Current V1 still contains `RecoverAtWaypoint`, which resets velocities and repositions the body, and it has no dedicated collision-response or physical lane-reattachment implementation.
- `ANO-5.10-03` remains open in the planning artifacts. The oracle keeps its collision-response, lane-rejoin, and reaction-variability cases as `GAP` rows V1-F04 through V1-F06.
- The owner decision is an acceptance/waiver decision, not new runtime evidence. The oracle correctly retains the historical facts that PlayMode was order-sensitive and the Story 5.14 manual recipe was unreported.

## Required findings

### HIGH-1 — The proposal still schedules the accepted oracle and optional tag as prerequisite work

The proposal's new owner clarification and migration step 1 correctly say not to reopen V1 validation or require a branch/tag. Later text reintroduces the old plan:

- migration step 2 says **“Build a behavioral oracle”** before introducing the V2 road contract (`sprint-change-proposal-2026-09-21.md:349`);
- V2-1 remains a future work package whose outcome includes a **“reference tag/branch plan”** (`:380`);
- retirement assumes a tag/branch will exist and be retained (`:356`).

This is not merely historical wording: a downstream epic/story generator could turn V2-1 into a new executable prerequisite, directly contradicting the 2026-09-22 owner decision and AD-36.

**Required revision:** mark the behavioral oracle/V1 freeze step as already satisfied by the accepted commit and catalog; remove the tag/branch plan from required outcomes; make any tag/branch explicitly optional hygiene if later created. Migration may still translate catalog rows into V2 tests as each V2 boundary is implemented, but that is V2 verification work, not reopening V1.

### MEDIUM-1 — The oracle's regression-suite heading can still imply a new harness gate

The oracle correctly says no new V1 validation-harness work is required and that `MANUAL`/`GAP` rows do not block Road World Model work. However, the heading **“Regression suites to extract before V2 implementation”** (`V1-BEHAVIORAL-ORACLE.md:131`) can be read as requiring the physical and network suites to be implemented before any V2 implementation.

That reading conflicts with the accepted baseline decision and with the listed scenarios themselves: several are new V2 contract tests for capabilities V1 never implemented.

**Required revision:** rename/reframe this section as the V2 regression suites to implement alongside the owning V2 boundaries. Explicitly state that the catalog is complete as the accepted oracle and that the list creates no V1 replay, harness, or pre-Road-World-Model gate.

## Confirmed correct representations

### Architecture spine

- AD-36 names the exact accepted commit, declares Story 5.14 complete, and explicitly removes additional runtime recipe, PlayMode-harness, branch, and tag gates from Road World Model design.
- AD-36 retains only the future V2 retirement parity/integration gate. That is not a V1-baseline prerequisite and is consistent with preserving V1 until V2 replacement is proven.
- AD-36 and the course-correction note explicitly reject any interpretation that Story 5.14 delivered collision response or physical lane recovery; `ANO-5.10-03` remains open.

### Behavioral oracle

- Frontmatter says `accepted-reference-baseline`, not provisional or runtime-evidence-incomplete.
- `OWNER-ACCEPTED` accurately distinguishes owner acceptance from observed runtime proof.
- Historical PlayMode/manual-evidence limitations remain visible without being promoted into gates.
- V1-F04, V1-F05, and V1-F06 preserve collision response, physical lane rejoin, and reaction variability as future V2 `GAP` requirements.
- The accepted-baseline section makes branch/tag optional hygiene and forbids reopening V1 without a new course decision.

### Road World Model and responsibility contract

- The decision-status section explicitly states that no additional V1 runtime-evidence gate remains.
- The only current pre-story gate is the Road World Model schema/responsibility architecture gate.
- Collision response and recovery are described as distinct future tactical/supervisory responsibilities, and the document says their story boundaries remain to be generated after the architecture gate. It does not claim they were delivered by Story 5.14.

### Correct Course proposal

- The 2026-09-22 clarification correctly records Story 5.14 completion, the accepted commit/catalog baseline, and the runtime/harness/tag waiver.
- The artifact-change section correctly says Story 5.14 delivered only shared intent/physics and that significant collision response and physical lane recovery remain open under `ANO-5.10-03`.
- The test section correctly routes parity proof to the future V2 suite rather than requiring repair of the V1 PlayMode harness.

## Gate conclusion

No artifact falsely marks collision response or physical recovery as delivered. The accepted V1 baseline and Story 5.14 completion are accurately represented in the authoritative spine, oracle decision section, and responsibility contract. Final handoff should remove the proposal's obsolete V2-1/tag prerequisite wording and clarify the oracle regression-suite heading so no downstream planner can reconstruct the waived gate.

## Closure recheck — 2026-09-22

**Final verdict: PASS.** Both required corrections were applied and match repository reality:

- The proposal now uses the accepted oracle as an input, forbids reopening V1 for additional proof, removes any required tag/branch plan, and treats an optional tag/branch as repository hygiene only.
- The oracle now carries regression suites into future V2 delivery/parity and explicitly says they require no V1 extraction or execution prerequisite.

Across the spine, oracle, responsibility contract, and proposal, Story 5.14 is consistently complete for its shared-intent/physics scope; commit `210f48811e3f99fbb93c5d2aa75885e65edcf878` is the accepted V1 reference; no V1 replay, PlayMode-harness repair, tag, or branch gates planning; and the Road World Model/responsibility architecture gate is the only blocker before Traffic V2 implementation-story generation. `ANO-5.10-03`, collision response, and physical recovery remain explicitly open future V2 requirements rather than falsely delivered Story 5.14 behavior.
