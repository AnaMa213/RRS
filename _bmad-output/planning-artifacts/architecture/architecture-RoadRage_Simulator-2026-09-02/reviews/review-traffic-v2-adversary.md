# Traffic V2 Adversarial Compatibility Review

**Lens:** Construct independently built units that obey AD-36 through AD-42 yet fail to interoperate.  
**Initial verdict:** **NOT READY FOR IMPLEMENTATION STORIES.** The decomposition was sound, but four cross-unit protocols were underspecified. Each could produce two locally compliant implementations with different timing, authority or lifecycle semantics.

## Critical findings

None.

## High findings

### H1 — Junction grants have no frame transaction or publication contract

**Conforming build A:** During TrafficFrame `N`, vehicles ask the junction coordinator in stable vehicle-ID order. Each accepted grant is committed immediately, so later requests in the same pass observe earlier grants.

**Conforming build B:** All movement requests derived from TrafficFrame `N` are collected first, resolved as one batch, then published for TrafficFrame `N+1`.

Both builds use one immutable frame, deterministic ordering, junction-scoped state and compatible grants. They nevertheless disagree on grant latency, fairness, what “existing grants” means, and whether a tactical planner may act on a grant in frame `N` or must wait for `N+1`. A coordinator, planner and frame builder built to different interpretations will reject valid grants as stale or enter without a grant visible in the planner's expected epoch.

**Required architecture decision:** Define the junction transaction boundary: request epoch, batch versus ordered commit semantics, grant owner/store, effective frame, expiry/revocation rules, and the frame/version relation carried by permission results. This belongs in AD-37 or the responsibility contract, not in an implementation story.

### H2 — TrafficRule exception requests have no authorization owner or route back into junction coordination

**Conforming build A:** `DrivingPolicy` emits an exception that Tactical Decision treats as authorized; Tactical ignores a red signal or priority denial while still exposing the exception in debug state.

**Conforming build B:** `DrivingPolicy` only proposes an exception; Traffic Rules/Junction Coordination validates it and returns an effective permission before Tactical may proceed.

Both can claim an explicit, inspectable, scoped exception with a termination condition. The diagram, however, sends Policy only to Tactical, while the prose says Junction Coordination participates and the SimulationInvariant forbids incompatible grants. The two builds are behaviorally incompatible, and build A can bypass the only component able to evaluate cross-movement conflicts.

The same gap reaches Safety: intentional contact is target-scoped, but the accepted plan has no required immutable authorization projection. One Safety implementation can query live Rage/target state (breaking frame coherence and pulling policy into Safety); another can treat every predicted contact as unintended; a third can trust an unversioned boolean.

**Required architecture decision:** Assign one authority that turns an exception request into an effective, versioned exception. Define its flow into rule evaluation, Tactical Decision, planning and Safety. Junction-law exceptions may alter legal compliance, but must not bypass conflict-compatible movement authorization. The accepted plan must carry enough frame/version, target and termination scope for Safety to classify authorized contact without reading mutable policy state.

### H3 — Decision frames, physics steps and intent hold semantics are not related

**Conforming build A:** Build a TrafficFrame and run the whole pipeline once per `FixedUpdate`; the composer emits the newly accepted intent for that physics step.

**Conforming build B:** Run traffic decisions at a lower cadence, retain the accepted plan, and have the composer re-emit an intent on every physics step until its validity window expires.

Both emit exactly one finite intent per vehicle per physics step and can describe their source as one coherent frame. They differ on plan lifetime, stale-frame rejection, Safety sampling and reaction latency. The current statement that Safety consumes “near-field frame facts” also permits either old decision-frame facts or a newer physics-step hazard sample, which conflicts with the ban on mixed-time reads unless the exception is explicitly modeled.

**Required architecture decision:** Define the authoritative decision clock and its mapping to physics steps: whether plans may be held, their validity/version window, which layer resamples immediate physical facts, and how a newer safety sample is represented without silently mixing epochs. AD-42 may defer performance-driven cadence choices, but the temporal interoperability contract cannot be deferred.

## Medium findings

### M1 — Recovery requests and catastrophic failure have no acknowledgement or terminal-owner contract

**Conforming build A:** A valid Recovery Supervisor request preempts the ordinary tactical goal until a progress criterion completes it.

**Conforming build B:** Recovery is advisory; Tactical may repeatedly reject it while route/rule goals remain active. A catastrophic failure parks the vehicle indefinitely because recovery may not teleport, remove, reinsert or directly actuate it.

Both preserve the “request, never actuator” boundary and do not self-grant right-of-way. They disagree on request priority, ownership of attempt completion, retry accounting, cancellation, and what system owns a vehicle that has no invariant-preserving recovery plan. This can make a Recovery implementation and Tactical implementation mutually ineffective even though each follows AD-40.

**Required architecture decision:** Define a small request handshake: eligibility/preconditions, Tactical acceptance/rejection reason, active-attempt ownership, completion/cancellation signal and retry history update. Name the owner and allowed run-level outcomes for declared catastrophic failure. The outcome may remain product-tunable, but “nobody owns it” cannot be a conforming state.

## Adversarial conclusion

AD-36, AD-38, AD-41 and AD-42 sufficiently constrain migration, reuse, shared-stack policy and profiling-gated LOD at the current altitude. The implementation gate should remain closed until H1-H3 are resolved in the architecture contracts. M1 should be resolved before collision-response or recovery stories are emitted.

## Closure recheck

**Final verdict: PASS for these four findings.**

- **H1 closed:** AD-37 and the companion now require stable-ID batch resolution from frame N, coordinator-owned prior grants, versioned publication for the next decision frame, and grant source/effective/expiry/revocation metadata.
- **H2 closed:** AD-39 makes Junction Coordination the sole exception authority, preserves conflict-compatible grants, and requires accepted plans to carry target, scope and expiry into Safety without live policy reads.
- **H3 closed:** the contracts now relate decision and physics epochs, require source-frame/validity windows, define plan hold versus `Idle`, and label current-step near-field Safety samples as invariant-only inputs.
- **M1 closed:** AD-40 defines request ownership, accept/reject reasons, accepted-maneuver ownership, completion/cancellation and history updates. The traffic lifecycle owns diagnosed `Faulted` safe stop; destructive cleanup remains explicitly unauthorized until the architecture gate approves a Run-level catastrophic policy.

No residual incompatibility from H1-H3 or M1 blocks the current architecture-planning handoff. This verdict does not approve implementation stories while the separate Road World Model/schema and runtime-evidence gates remain open.
