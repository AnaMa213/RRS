# Traffic V2 reality-check review

**Scope:** AD-36 through AD-42 in `ARCHITECTURE-SPINE.md`  
**Lens:** verify that adopted decisions are grounded in the current Road Rage Simulator repository and approved artifacts, and distinguish current capability from target architecture.  
**Verdict:** **PASS WITH REQUIRED REVISIONS.** The course correction is strongly grounded in the repository, and no new decision depends on unverified training-data claims or a newly named external technology. Two wording seams in the spine can still produce incompatible implementations; two retained-foundation assumptions need explicit hardening/qualification.

## Evidence inspected

- Current Traffic V1 implementation: `NetworkedAIVehicleDriverController` (1,194 lines), `LaneGraph`, `LaneNode`, `LaneGraphRouting`, `DriverModel`, `NetworkedAIVehicleState`, `PortalTrafficSpawner`, `VehicleDriveIntent`, `VehiclePhysicsBody`, and Rage state/profile code.
- Existing EditMode and PlayMode traffic/vehicle fixtures, especially Stories 5.9, 5.10, 5.14, 5.15 and 5.16.
- Story 5.14 implementation spec and runtime notes.
- `ANO-5.10-03`, sprint status, Epic 5 context, the approved 2026-09-21 Correct Course proposal, `V1-BEHAVIORAL-ORACLE.md`, and `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`.
- Exact-symbol prechecks for the externally hypothesized queue, junction, snapshot, motion, speed, safety and recovery subsystems. All were absent, matching the negative oracle.

The provisional baseline commit recorded by the oracle, `210f48811e3f99fbb93c5d2aa75885e65edcf878`, exists and is the current `HEAD`. The oracle correctly labels runtime evidence incomplete and does not pretend that a reference branch/tag or a trustworthy PlayMode pass already exists.

## Findings

### HIGH-1 — AD-40 gives collision response ambiguous ownership of the tactical goal

**Evidence**

- AD-37 establishes one Tactical Decision stage and one tactical goal.
- AD-40 currently says: “Collision response temporarily owns the tactical goal while the body is physically unstable.”
- The detailed responsibility contract is clearer: Tactical Decision **produces one tactical goal**, including `collision response`, while collision response “temporarily changes” that goal (`ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md:197,236`).
- Current V1 has no collision-response owner at all; `ANO-5.10-03` is still open, so this wording will directly shape the first implementation.

**Risk**

One story could build collision response as a second goal writer/state machine while another treats collision facts as input to Tactical Decision. That recreates the competing-command architecture AD-37 is intended to prevent.

**Required revision**

State in AD-40 that **Tactical Decision remains the sole tactical-goal owner**. Collision detection/response supplies versioned facts or a requested response mode; Tactical Decision accepts/rejects that request and owns the active collision-response goal through completion/cancellation. Recovery should use the same request handshake.

### HIGH-2 — Portal-only logical lifecycle is not separated from future LOD representation lifecycle

**Evidence**

- Current V1 literally despawns its `NetworkObject` only when `HasReachedExitPortal` is true (`PortalTrafficSpawner.cs:256-280`). That supports AD-34 and the V1 oracle.
- AD-39 lists “portal-only normal lifecycle” as a non-bypassable SimulationInvariant.
- The detailed contract says “Normal traffic spawns/despawns only at portals” (`ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md:268`).
- AD-42 more precisely requires “portal-only **visible entry/exit**” while allowing future near/mid/far representation changes (`ARCHITECTURE-SPINE.md:360`; responsibility contract line 309).

**Risk**

A future LOD story may interpret the invariant as forbidding an off-screen `NetworkObject`/Rigidbody proxy from being unloaded while its logical traffic agent remains alive. Another may unload it mid-road. Both readings are plausible, so AD-39 and AD-42 do not yet converge the implementation.

**Required revision**

Define portal-only lifecycle at the **logical traffic-agent/world-continuity** level. A fidelity representation may be created or released away from a portal only if stable identity, route/policy/progress and deterministic restoration survive and no visible pop is introduced. This preserves the gameplay invariant without precluding the LOD architecture AD-42 explicitly keeps open.

### MEDIUM-1 — The retained `VehicleDriveIntent` boundary does not itself enforce the new finite-output invariant

**Evidence**

- AD-36/AD-37 retain `VehicleDriveIntent` and `VehiclePhysicsBody` as sound foundations; AD-39 requires invalid/non-finite controls to be rejected.
- `VehicleDriveIntent` calls `Mathf.Clamp01/Clamp` but does not reject or replace `NaN`/infinity (`VehicleDriveIntent.cs:18-24`). A NaN is not made finite by comparison-based clamping.
- `VehiclePhysicsBody.ApplyDriveIntent` stores the intent and the three authority scalars without an input-finiteness guard (`VehiclePhysicsBody.cs:240-250`). Some deeper physics helpers are finite-safe, but the boundary is not universally fail-closed.
- V1 normally feeds tested finite kernels, so this is not evidence that current nominal traffic emits NaN. It is evidence that the retained DTO/physics entry point alone does not satisfy the stronger V2 invariant.

**Required revision**

Qualify reuse as “retained data/physics foundation, with a finite-input guard at the V2 composer/physics boundary.” Add the finite/Idle fallback as a V2 contract test before treating the retained path as satisfying AD-39. No V1 implementation change is required during this Correct Course phase.

### MEDIUM-2 — “Frozen baseline” is policy-complete but not repository-freeze-complete

**Evidence**

- AD-36 says V1 “is frozen at its recorded behavioral baseline.”
- The oracle correctly says the commit is provisional, runtime evidence is incomplete, and no reference branch/tag is yet declared.
- The Story 5.14 notes explicitly report unexecuted human runtime evidence and no new PlayMode proof; the repository records the PlayMode harness as order-sensitive/untrustworthy.

**Risk**

“Frozen” can be read as “archival reference cut and verified,” which is not yet true. The existing AD gate mostly mitigates this, but the first sentence is stronger than the actual repository state.

**Required revision**

Call the current state a **capability freeze at a provisional baseline commit**. The archival reference branch/tag is cut only after the oracle runtime-evidence gate. Any oracle-only harness correction must update the baseline commit and evidence record, as the oracle already requires.

## Per-decision reality map

| Decision | Reality verdict | Repository support / limitation |
| --- | --- | --- |
| AD-36 | Supported with qualification | The monolithic V1 controller, unresolved Story 5.14 runtime evidence and open `ANO-5.10-03` justify side-by-side V2. `DriverModel`, `DriverProfileDef`, shared intent/physics and host authority exist. The baseline is provisional, not yet archived. |
| AD-37 | Supported as target architecture | Current V1 has host-only `FixedUpdate`, one final intent submission and shared physics, but no immutable frame or separated planners. The AD accurately presents those as new V2 boundaries rather than current components. |
| AD-38 | Supported | V1 authoring really is transform-based `LaneNode` positions/forward, successor arrays, turn weights and portal roles; `LaneGraph.JoinConnectors` performs runtime distance/direction joining. No lane width, adjacency, movement/conflict, stop-line or signal schema exists. The one-way migration contract correctly avoids claiming those semantics can be recovered automatically. |
| AD-39 | Supported as target; retained-boundary hardening needed | Current host authority and profile modulation are real. Junction grants, policy exceptions, SafetyFilter and explicit SimulationInvariant enforcement do not exist yet. The document correctly treats them as V2; finite intent rejection needs an explicit boundary guard. |
| AD-40 | Supported need; ownership wording must converge | V1 still teleports/reset velocities in `RecoverAtWaypoint` (`NetworkedAIVehicleDriverController.cs:1116-1148`), and collision response/physical lane rejoin remain open. The supervisor direction is evidence-based, but collision-response goal ownership is ambiguous in the spine. |
| AD-41 | Supported | Current Rage state is per vehicle and host-owned, and `DriverModel.ResolveEffectiveProfile` already demonstrates parameter modulation rather than a second physics stack. Fear-driven motion, target pursuit and intentional contact are future capabilities, correctly not claimed as present. |
| AD-42 | Supported as deferred target | V1 has no traffic LOD; the 30-body test is only a collision benchmark. Profiling-gated implementation is appropriately conservative. Logical lifecycle versus representation lifecycle must be made explicit. |

## Confirmed corrections to the external review

- No `TrafficQueue`, bounded-wait flags, `WaitsFor`, peer `ResolveWinnerIndex` tournament, `TryEscalateRecovery`, four-tier recovery ladder, Junction Manager, Traffic Light Controller, World Snapshot, Motion Planner, Speed Planner, Safety Supervisor or Recovery Supervisor exists in current V1.
- Current V1 has one controller producing one final intent; the architectural risk is the already broad ownership plus the responsibilities that superseded Stories 5.17–5.23 would have added, not several already-implemented pedal commanders.
- Current recovery is the 60-second stuck timer plus rollover/void checks leading to `RecoverAtWaypoint`, not the ladder described by the external hypothesis.
- Current lane-change code only schedules/evaluates the pure MOBIL predicate; it has no authored adjacent lane and does not execute a lane-change maneuver.

## Gate conclusion

No web verification is needed for AD-36 through AD-42: they introduce no new named package, service or version. Their factual premises are repository-backed and their future components are consistently labeled as target architecture. Apply HIGH-1 and HIGH-2 in the spine before final handoff; carry MEDIUM-1 into the V2 contract tests and qualify the baseline wording per MEDIUM-2. The Road World Model schema, migration proof and runtime oracle remain correctly gated before implementation-story generation.

## Closure recheck — 2026-09-21

**Final verdict: PASS.** All four required revisions were applied consistently in the spine and companion contract:

- AD-40 now makes Tactical Decision the sole tactical-goal owner and gives collision/recovery supervisors a versioned request handshake.
- AD-39/AD-42 and the companion now distinguish the portal-only logical-agent lifecycle from unloadable fidelity representations while preserving identity and visible continuity.
- AD-39 and the composer contract now require a diagnosed finite-input guard with `Idle` fallback and a mandatory contract test; they no longer imply the retained V1 DTO proves this invariant unchanged.
- AD-36 now describes a capability freeze at a provisional commit and defers the archival branch/tag until the runtime-evidence gate.

No residual reality-check blocker remains. The still-open Road World Model schema and incomplete V1 runtime evidence are correctly represented as pre-story gates, not silently treated as complete.
