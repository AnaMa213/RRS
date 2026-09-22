# Traffic V2 Architecture Spine — Rubric Review

**Reviewer lens:** BMad good-spine checklist  
**Reviewed:** 2026-09-21  
**Scope:** `ARCHITECTURE-SPINE.md`, especially AD-36 through AD-42, reconciled against the approved 2026-09-21 course correction, `V1-BEHAVIORAL-ORACLE.md`, and `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`.

## Verdict

**CONDITIONAL PASS — the Traffic V2 direction is coherent and covers the approved behavioral baseline, but three high-severity spine ambiguities should be corrected before this reviewer gate closes.** The remaining medium finding should be made explicit as an open question before recovery stories are allowed.

The deterministic lint passed with zero findings. AD-36 through AD-42 correctly preserve host authority, shared physics, the V1 oracle/parity gate, semantic road migration, single command ownership, policy-versus-invariant separation, supervisory recovery, one normal/Rage stack, and profile-gated LOD. The companion documents carry the necessary behavioral and responsibility detail without prematurely freezing the serialized schema.

## Evidence checked

- The approved clarification block and BC-1 through BC-12 in `sprint-change-proposal-2026-09-21.md`.
- The complete V1 scenario catalog and evidence states in `V1-BEHAVIORAL-ORACLE.md`.
- The Road World Model capability, ownership, migration, dependency, safety, recovery and architecture-gate contracts in `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`.
- The current spine's AD-7, AD-32 through AD-42, supersession notes, capability map, Open Questions and Deferred table.
- Brownfield evidence: `Story510LaneGraphAndRoutedTrafficTests.MvpRunBakesTheDistrictForTheVehicleAgentWithoutAnyNavMeshAgent` explicitly says the current NavMesh data is authored and “never an active path provider”; repository symbol inspection finds no production `NavMeshAgent` use.
- `lint_spine.py`: zero mechanical findings.

## Findings

### HIGH — The supersession map leaves three older Traffic rules simultaneously binding

**Where:** AD-7, AD-32, AD-33 versus AD-36, AD-38 and AD-42; 2026-09-21 course-correction supersession note.

**Problem:** The new rules are directionally correct, but the old rules are not explicitly narrowed:

- AD-7 still defers “broad traffic simulation,” while AD-36 authorizes Traffic V2 as the next architectural course.
- AD-32 still says production scale is decided after the now-superseded Story 5.22 measurement, while AD-42 replaces that trigger with profiling of a functionally correct V2 or an earlier reproducible breach.
- AD-33 says `com.unity.ai.navigation` “is used only as a path provider,” while AD-38 makes the semantic Road World Model authoritative and the approved proposal explicitly says NavMesh is not a Traffic V2 foundation. Current repository evidence also has no active `NavMeshAgent` path provider.

Two independently built units can therefore choose incompatible interpretations: defer V2 or build it; plan against NavMesh or against the road model; schedule scaling against a deleted story or against the V2 profiling gate.

**Disposition:** **Autofix.** Add explicit, clause-level supersessions without rewriting history:

1. AD-36 lifts only AD-7's broad-traffic deferral for Traffic V2; AD-7's arcade/shared-physics intent remains.
2. AD-42 supersedes AD-32's Story-5.22-specific measurement trigger while retaining the configurable counts and ~30-vehicle benchmark.
3. AD-38 supersedes AD-33's NavMesh path-provider clause for Traffic V2; AD-33's parameterized driving/modulation contract remains. NavMesh may only return later as a measured supplemental provider for unstructured/off-road recovery under a new decision.

### HIGH — `TrafficFrame` is used for both immutable inputs and post-decision outputs

**Where:** AD-37 says one immutable `TrafficFrame` feeds observations, then says “Every frame exposes stable reason codes for route, goal, blockers, grant, binding speed constraint, safety and recovery.”

**Problem:** The companion responsibility contract correctly defines `TrafficFrame` as the immutable input snapshot and separately defines the queryable decision/debug projection. Route, tactical goal, speed binding, safety result and recovery state do not exist when the input frame is built. Saying they are exposed by “the frame” creates a circular ownership choice: either the snapshot builder starts owning planning output, or the nominally immutable input is mutated after planning.

**Disposition:** **Autofix.** Keep `TrafficFrame` input-only. Replace the final AD-37 sentence with a rule that each authoritative evaluation publishes a debug/decision projection keyed to `TrafficFrame` version and containing those reason codes. Do not freeze a C# type name yet.

### HIGH — The spine diagram omits load-bearing Route and Junction inputs that the companion contract requires

**Where:** AD-37 rule and diagram versus Responsibility Contracts sections 4 and 5.

**Problem:** The diagram supplies only Road World Model to Route Planner and only Perception to Traffic Rules/Junction Coordination. The approved companion requires Route Planning to consume current localization, closures and policy costs, and requires junction coordination to consume movement semantics, signal state and existing grants. A builder working from the spine can reasonably produce a static-only route planner or a junction coordinator that cannot read authoritative rule/signal state. That is exactly the divergence a spine should prevent.

**Disposition:** **Autofix.** Adjust the rule/diagram at the contract level:

- Route Planner consumes Road World Model plus same-frame localization/closures and policy costs.
- Traffic Rules/Junction Coordination consumes Road World Model movement semantics plus same-frame observations/signal state and coordinator-owned prior grants.
- Outputs continue downward into Tactical Decision; neither owner actuates or mutates the input frame.

This does not require new services or concrete schemas.

### MEDIUM — Catastrophic out-of-world cleanup has no owner or revisit gate

**Where:** AD-40 permits Recovery to “declare a separately handled catastrophic policy” while correctly forbidding Recovery from teleporting, reinserting, despawning or moving the Rigidbody. BC-9 and the oracle likewise require catastrophic cleanup, if retained, to be separate and observable.

**Problem:** The separation is sound, but the spine never says who may handle that declaration, what actions are even eligible, or when the decision must be made. A recovery implementer could place the old teleport in Safety, lifecycle or physics and still claim Recovery itself did not perform it.

**Disposition:** **Defer explicitly.** Add an Open Question bound to “before any collision-response/recovery implementation story”: whether catastrophic out-of-world cleanup exists in production, its owner, allowed state transitions, multiplayer observability, and how it differs from normal traffic behavior. Until answered, no runtime teleport/reinsert/despawn path is authorized.

## Checklist result

| Good-spine criterion | Result |
| --- | --- |
| Fixes real divergence points for the next level | Pass after the three high fixes above. |
| Every AD rule is enforceable and prevents its stated divergence | Mostly pass; AD-37 input/output wording currently weakens enforcement. |
| Deferred items cannot silently produce incompatible builds | Pass except catastrophic cleanup ownership, which needs an explicit open item. |
| Named technology is current/fit | Pass for this Traffic V2 update; it adds no new dependency. |
| Ratifies brownfield reality | Pass except the stale AD-33 NavMesh path-provider clause. |
| Covers the approved Traffic V2 contract | Pass: BC-1 through BC-12 and all five approval clarifications landed. |
| Does not weaken inherited decisions | Conditional: the intended supersessions are right but must be clause-explicit. |
| Covers the operational/environmental envelope | Pass: host authority, deterministic ordering, debug/validation, population benchmark and profile-gated LOD are covered. |

## Gate recommendation

Apply the three high-severity text/diagram fixes, add the catastrophic-cleanup open question, rerun deterministic lint, and then close this rubric lens. No implementation story should be generated from the current schema-open companion; that pre-story gate is correctly preserved.

## Closure recheck — 2026-09-21

**FINAL VERDICT: PASS.** All required revisions were applied correctly:

- AD-36, AD-38 and AD-42 now explicitly narrow/supersede the conflicting AD-7, AD-33 and AD-32 clauses while retaining their sound arcade-physics, parameterization, population and benchmark contracts.
- AD-37 now keeps immutable `TrafficFrame` input separate from the keyed decision/debug projection and forbids writing planning outputs into the frame.
- AD-37's rule and diagram now carry the required Road World Model, same-frame localization/closure/signal, policy-cost and prior-grant inputs. The companion further fixes junction resolution as a deterministic frame transaction with next-frame versioned grants.
- AD-40 and the companion place invariant-preserving failure in a diagnosed traffic-lifecycle `Faulted` state. The unresolved production catastrophic-cleanup policy is now an explicit pre-recovery-story Open Question; teleport, reinsertion and mid-road despawn remain unauthorized until it is decided.

No residual finding from this rubric review blocks closure. The separate Road World Model schema/runtime-evidence gates remain intentionally open, so this pass authorizes the architecture baseline—not Traffic V2 implementation stories.
