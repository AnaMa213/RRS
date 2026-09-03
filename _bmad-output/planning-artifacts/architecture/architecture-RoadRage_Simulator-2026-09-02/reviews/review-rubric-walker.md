# Architecture Spine Rubric Walker Review

## Verdict

Rubric gate verdict: changes required before finalizing the spine.

The deterministic lint pass is clean: `lint_spine.py` reported `ok: true`, `total_findings: 0`. The semantic gate is not clean. The spine names all seven SPEC capabilities in frontmatter and in the capability map, and most architecture decisions have a usable `Binds / Prevents / Rule` shape. The failure is preservation: several SPEC success criteria and constraints land only as broad capability labels, diagram implications, or deferred items, not as enforceable invariants for builders one level down.

## Inputs Read

- Spine: `ARCHITECTURE-SPINE.md`
- SPEC kernel: `SPEC.md`
- SPEC companions: `gameplay-model.md`, `mvp-scope.md`
- Architecture provenance: `.memlog.md`
- Existing project context: none found by `rg --files -g 'project-context.md'`
- Current-tech checks against official Unity and Blender sources:
  - Unity 6.6 manual: https://docs.unity3d.com/Manual/index.html
  - Netcode for GameObjects latest/exact docs: https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@latest/ and https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/install.html
  - Unity Multiplayer Services exact/latest docs: https://docs.unity3d.com/Packages/com.unity.services.multiplayer@2.1/manual/index.html and https://docs.unity3d.com/Packages/com.unity.services.multiplayer@2.3/manual/index.html
  - Unity Multiplayer join-code docs: https://docs.unity.com/en-us/mps-sdk/join-session
  - Unity Relay docs: https://docs.unity.com/en-us/mps-sdk/connect-players
  - URP selection docs: https://docs.unity3d.com/Manual/choose-a-render-pipeline.html
  - Input System latest/exact docs: https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/ and https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/
  - Cinemachine latest/exact docs: https://docs.unity3d.com/Packages/com.unity.cinemachine@latest/ and https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/changelog/CHANGELOG.html
  - Blender LTS/release notes: https://www.blender.org/download/lts/ and https://developer.blender.org/docs/release_notes/

## Top Findings

### 1. MVP cardinalities do not land as enforceable architecture

Location: `ARCHITECTURE-SPINE.md` lines 66-157, 256-269; `SPEC.md` lines 50-52; `mvp-scope.md` lines 11-23

Problem: The SPEC fixes concrete MVP cardinalities: one route, one player car, up to four online players, three AI vehicles with individual rage, three passenger actions, one on-foot transition, one compact sandbox zone, one money reward, one upgrade, and one Rage Road event. The spine maps CAP-1 through CAP-7 by name, but no AD or convention binds those counts as the implementation slice.

Why it matters: Two feature builders could both obey every AD while one builds a two-player/two-AI prototype and another builds scalable arbitrary content. Both would appear compliant, but only one preserves the SPEC contract.

Fix: Add an MVP slice invariant, or tighten AD-11/AD-14, with an enforceable rule such as: "The first playable run contains exactly one route, one player car, session cap four, three spawned enemy vehicles, three passenger action definitions, one on-foot transition, one compact sandbox zone, one money reward, one upgrade, and one Rage Road event until the MVP success signal passes."

### 2. The four-player online cap is only implied, not ruled

Location: `ARCHITECTURE-SPINE.md` lines 86-90, 231-243; `SPEC.md` line 50; `mvp-scope.md` line 15; `gameplay-model.md` line 38

Problem: The diagram shows host plus three clients, but AD-4 does not set session capacity or validation expectations for "up to four players."

Why it matters: Lobby, spawn, seat/role assignment, UI, player state, and death aggregation can diverge if one slice assumes 2 players, another assumes 4, and another treats player count as unbounded.

Fix: Add "MVP session capacity is 1 host plus up to 3 joining clients; lobby, spawn, player-state, UI, and team-wipe logic must enforce that cap."

### 3. Unity Multiplayer Services is pinned to an older version while provenance claims current

Location: `ARCHITECTURE-SPINE.md` lines 72, 178-180; `.memlog.md` line 12

Problem: The spine pins Unity Multiplayer Services to 2.1.1. Official docs checked on 2026-09-02 show Multiplayer Services 2.3.1 exists, while the memlog says a 2026-09-02 web check found 2.1.1 current. Other named stack items checked cleanly enough: Unity 6.6, NGO 2.13.2, Input System 1.20.0, Cinemachine 6.6.0, and Blender 5.2 LTS are supported by official docs. This specific package claim is stale or intentionally pinned without saying so.

Why it matters: AD-1 claims the adopted MVP stack. A stale "current" assertion can send implementation agents to older APIs and make future package upgrades look accidental instead of governed.

Fix: Either update Multiplayer Services to 2.3.1, or explicitly mark 2.1.1 as an intentional compatibility pin and record why newer 2.2/2.3 APIs are not used.

### 4. Rage Road has no authoritative lifecycle contract

Location: `ARCHITECTURE-SPINE.md` lines 80-84, 98-102, 110-120, 128-138, 260-266; `SPEC.md` lines 32-34; `gameplay-model.md` lines 10-12, 24-26

Problem: CAP-4 requires a Rage Road event that can be triggered, resolved through confrontation, and converted into a money reward. The spine says host owns gameplay event triggers and maps CAP-4 to Run/Rage/OnFoot, but it does not decide the lifecycle owner, states, handoff points, or reward trigger.

Why it matters: Run, Rage, OnFoot, Vehicles, and Economy can each implement a compliant-looking slice with incompatible meanings for "triggered", "resolved", "confrontation", and "reward granted."

Fix: Add a Rage Road lifecycle AD. Example rule: "A host-owned RageRoadEventState in Run owns trigger -> confrontation -> resolved/failed -> reward-granted, with Rage raising eligible triggers, OnFoot/Vehicles reporting resolution intents, and Economy granting money only after the Run-owned resolved state."

### 5. The compact sandbox stop is under-architected

Location: `ARCHITECTURE-SPINE.md` lines 98-102, 128-138, 186-219, 260-268; `SPEC.md` lines 51, 55; `gameplay-model.md` lines 28-30; `mvp-scope.md` lines 18-19, 35-40

Problem: The SPEC says towns/stops are compact sandbox zones with happenings, money opportunities, purchases, and incidents, and the MVP includes one compact zone. The spine mentions stop-zone interaction and keeps the stop zone in `MVP_Run`, but there is no feature owner or data/state contract for sandbox happenings, purchases, opportunities, and incidents.

Why it matters: OnFoot, Economy, Rage, and Run can each add local stop-zone rules and accidentally create either content sprawl or disconnected side gameplay, the exact scope risk named in `mvp-scope.md`.

Fix: Add a `SandboxStops` slice or explicitly bind the first stop-zone contract to `Features/OnFoot` plus `Features/Economy`, with Run owning entry/exit and event state.

### 6. Player life, death, revive, and vehicle destruction are deferred too broadly for CAP-7

Location: `ARCHITECTURE-SPINE.md` lines 80-96, 270-277; `SPEC.md` lines 44-46, 53, 75-76; `gameplay-model.md` lines 36-38

Problem: Deferring exact health, revive, vehicle destruction, and team-wipe timing is reasonable, but the spine also lacks a minimal interim contract for who owns player life/dead state and what "all players are dead" reads from.

Why it matters: CAP-7 is not optional. Vehicles, OnFoot, Boss, Run, and UI could all model death separately and still satisfy AD-3's broad "host owns health/death" rule.

Fix: Keep exact numbers deferred, but add a minimal life-state invariant: one host-owned `NetworkedPlayerState` has `Alive/Down/Dead` or a simpler MVP equivalent, and `NetworkedRunState` computes team wipe only from that authoritative state.

### 7. Boss endpoint scope is ambiguous

Location: `ARCHITECTURE-SPINE.md` lines 92-97, 270-278; `.memlog.md` line 48; `SPEC.md` lines 44-46, 73-76; `mvp-scope.md` line 23

Problem: AD-5 says boss death declares victory, while Deferred says full boss design waits until a simple boss endpoint validates CAP-7. The memlog carries the missing assumption: the MVP can use a simple networked boss test object or vehicle encounter. The spine should not leave that only in provenance.

Why it matters: One builder can ship no boss endpoint because "full boss design" is deferred; another can build a rich boss because victory needs one. Both can cite the spine.

Fix: Add a rule or open item: "MVP victory uses a simple host-owned boss endpoint/test object; rich boss behavior is deferred." If even that is undecided, move the exact question into an Open Questions section.

### 8. Passenger action success criteria are only partially preserved

Location: `ARCHITECTURE-SPINE.md` lines 116-120, 256-264; `SPEC.md` lines 24-27; `gameplay-model.md` lines 16-18; `mvp-scope.md` line 17

Problem: AD-9 correctly makes passenger actions data-defined host-validated intents, but it does not preserve the MVP count of three actions or the success criterion that they visibly change a vehicle's rage, incident state, or resource opportunity.

Why it matters: A future implementation could add one well-architected action or three actions that only play local comedy animations and still appear compliant with AD-9.

Fix: Bind the first action set to three ScriptableObject definitions, each with target/effect metadata and at least one host-applied effect in rage, incident, resource, or crew-help domains.

### 9. Per-vehicle rage lands, but the three-AI route validation does not

Location: `ARCHITECTURE-SPINE.md` lines 104-114, 256-264; `SPEC.md` lines 28-30; `mvp-scope.md` line 16; `gameplay-model.md` lines 20-22

Problem: AD-8 preserves independent per-enemy rage states and the state names. It does not preserve the success case of three AI vehicles on the same route independently occupying different states.

Why it matters: A single enemy vehicle can satisfy the architecture text but fail the SPEC success signal. The missing test fixture also weakens the proof that rage is not global.

Fix: Add a validation rule under AD-8 or AD-14: the MVP greybox must spawn three enemy vehicles on the same route and demonstrate independent calm/irritated/flee/block/ram/confrontation-capable state paths.

### 10. Economy and upgrade ownership is too implicit

Location: `ARCHITECTURE-SPINE.md` lines 98-102, 152-156, 160-167, 260-266; `SPEC.md` lines 36-38, 54; `gameplay-model.md` lines 32-34; `mvp-scope.md` lines 20-21

Problem: The money convention captures integer currency and "significant rewards come from road-rage confrontation." The spine does not decide the owner of reward calculation, upgrade purchase/apply, or the one-reward/one-upgrade MVP boundary.

Why it matters: Economy, OnFoot, Run, and Rage can diverge on when money exists, where purchases happen, and whether upgrades are runtime state or authored data.

Fix: Add an Economy AD or tighten AD-12/AD-15: authored upgrade definitions are ScriptableObjects, host-owned runtime economy state grants one road-rage reward and applies one upgrade in MVP, and purchases happen only through host-validated intents.

### 11. SPEC content-rating and tone boundary disappears

Location: `ARCHITECTURE-SPINE.md` no landing section; `SPEC.md` line 77; `gameplay-model.md` lines 16-26

Problem: The SPEC leaves an open question about tone and content-rating boundaries for threats, pissing, fights, and absurd provocation actions. The spine does not decide, defer, or record this as open.

Why it matters: PassengerActions, OnFoot, assets, UI copy, and animation can diverge sharply on acceptable content. This is a whole content-boundary dimension left silent.

Fix: Add an Open Questions section or Deferred item for content-rating/tone boundaries, with a revisit trigger before finalizing passenger action names, animations, UI copy, and generated assets.

### 12. Operational/environmental and validation dimensions are only partial

Location: `ARCHITECTURE-SPINE.md` lines 152-169, 171-184, 270-283; `.memlog.md` line 47

Problem: The spine partially addresses operations through transient sessions, secrets, package pinning, network errors, and deferring non-PC platforms. It does not explicitly bind the first environment target from the memlog: Windows PC development builds first. It also lacks a multiplayer validation matrix for local play mode, two remote players, four-player cap, Relay/Lobby failure behavior, host quit behavior, or UGS dev/prod project boundaries.

Why it matters: This is the exact kind of operational/environmental envelope a domain-focused spine can skip. Feature slices can build and test against different assumptions while obeying the gameplay ADs.

Fix: Add an operational envelope AD or convention: MVP targets Windows PC dev builds, uses a non-production Unity services project/environment, validates local multiplayer play mode and at least one remote Relay join path, and surfaces host quit/disconnect/network-service failures through Lobby/UI without inventing host migration.

### 13. AD-4's optional invite link is not as well supported as join code/Relay

Location: `ARCHITECTURE-SPINE.md` lines 86-90; `.memlog.md` lines 24-33

Problem: Official Unity Multiplayer docs clearly support join-code workflows and Relay connection without dedicated servers. The spine also binds an "optional invite link", but the current evidence in the spine sources does not make that link behavior an enforceable Unity capability.

Why it matters: Lobby/UI can spend MVP effort on deep link handling or platform invites before the join-code path is proven.

Fix: Reword AD-4 to "join code first; invite link only if Unity/platform support is confirmed during implementation" or add the verified source and exact expected behavior.

### 14. Terminology is not normalized across stop-zone, sandbox-zone, town, and Rage Road event/crisis

Location: `ARCHITECTURE-SPINE.md` lines 98-102, 128-132, 260-266, 270-283; `SPEC.md` lines 32-34, 51, 55; `gameplay-model.md` lines 28-30

Problem: The spine alternates among stop zone, compact sandbox zone, towns/stops, Rage Road event, and Rage Road crisis without a glossary or owner mapping.

Why it matters: This is a prose/structure issue with real architecture consequences: builders may create multiple concepts where the SPEC intends one compact MVP surface.

Fix: Add a short vocabulary convention: "Sandbox stop" is the MVP compact zone; "Rage Road event" is the triggered crisis/confrontation lifecycle; use those names consistently in ADs, diagrams, feature folders, and maps.

## SPEC Capability Landing Matrix

| Capability | Landing verdict | Notes |
| --- | --- | --- |
| CAP-1 cooperative driving loop | Partial | Lobby, host authority, arcade vehicle, one-loop ADs land. Missing up-to-four cap and full MVP cardinality rule. |
| CAP-2 active passenger chaos | Partial | Data-defined host-validated intents land. Missing three-action boundary and visible rage/incident/resource success criterion. |
| CAP-3 per-vehicle rage | Partial | Per-enemy rage state and named states land. Missing three-AI same-route validation fixture. |
| CAP-4 Rage Road crisis | Partial | Run/Rage/OnFoot mapping lands. Missing authoritative event lifecycle and confrontation-to-reward handoff. |
| CAP-5 money and upgrade reward | Partial | Money significance and integer currency land. Missing reward/upgrade owner and one reward/one upgrade boundary. |
| CAP-6 on-foot preparation/confrontation | Partial | One-loop rule lands. Missing compact sandbox zone owner, items/risk contract, and purchase interaction boundary. |
| CAP-7 failure/victory | Partial | `NetworkedRunState` owns outcome. Missing minimal player life-state contract and explicit simple boss endpoint assumption. |

## SPEC Constraint Landing Matrix

| SPEC constraint | Landing verdict | Notes |
| --- | --- | --- |
| Online co-op up to four players | Partial | Internet path lands; four-player cap does not land as a rule. |
| MVP scope cardinalities | Miss | Names land by capability, but counts do not land as enforceable architecture. |
| In-car and on-foot feed same loop | Pass | AD-6 is clear and enforceable enough. |
| Team wipe restart and boss death victory | Partial | Outcome owner lands; life-state and boss endpoint are underdefined. |
| Significant currency from road-rage confrontation | Pass | Money convention plus AD-6/AD-15 cover the main constraint. |
| Towns/stops are compact sandbox zones, not destructible cities | Partial | Destructible cities are deferred, but compact stop-zone responsibilities lack owner/contract. |
| Prove core loop before complex boss, roguelite systems, deep economy, weapon breadth, large assets | Partial | Boss, economy, asset volume are deferred; weapon breadth and roguelite are not explicitly named, though covered by "deep economy"/progression in spirit. |

## AD Enforceability Audit

All 15 ADs have the required mechanical fields. The lint pass found no missing `Binds`, `Prevents`, or `Rule`.

| AD | Enforceability verdict | Notes |
| --- | --- | --- |
| AD-1 Unity URP MVP Stack | Concern | Rule is enforceable, but Multiplayer Services 2.1.1 is not current per official docs checked on 2026-09-02 unless it is an intentional pin. |
| AD-2 Feature Slices Own Gameplay | Pass | Clear placement and coordination rule. |
| AD-3 Host Owns Shared Runtime State | Pass with tightening option | Strong ownership rule; could name life-state and Rage Road state once added. |
| AD-4 Lobby And Relay Are The Internet Path | Pass with concern | Join-code/Relay path is enforceable; optional invite link and host quit/disconnect behavior need guardrails. |
| AD-5 NetworkedRunState Owns Run Outcome | Concern | Good owner decision, but depends on missing player life-state and boss endpoint contracts. |
| AD-6 One Loop, Not Two Games | Pass with concern | Correct invariant; enforcement improves once sandbox stop and Rage Road handoff contracts exist. |
| AD-7 Arcade Vehicles Before Realistic Vehicle Simulation | Pass | Clear and bounded. |
| AD-8 Rage Is Per Enemy Vehicle | Pass with concern | Per-vehicle state lands; MVP three-AI validation does not. |
| AD-9 Passenger Actions Are Data-Defined Intents | Pass with concern | Strong pattern; missing MVP count and visible-effect success guard. |
| AD-10 Camera And Input Are Local Presentation | Pass | Clear state-mutation boundary. |
| AD-11 Three Scene Seed | Pass with concern | Good seed; could absorb MVP cardinality rule or point to a separate MVP slice AD. |
| AD-12 ScriptableObjects For Authored Static Data | Pass | Clear split between authored static and runtime network state. |
| AD-13 Blender Intake Gate For 3D Assets | Pass | Enforceable asset intake gate. |
| AD-14 Greybox First, Art Second | Pass | Good sequencing invariant. |
| AD-15 Transient Session, No Game Backend | Pass with tightening option | Good backend boundary; should be paired with explicit environment/build target convention. |

## Deferred Safety

Safe as written:

- Public matchmaking, lobby browser, and friend systems: safe because AD-4 fixes private join-code sessions first.
- Realistic vehicle physics, WheelCollider tuning, and broad traffic simulation: safe because AD-7 fixes arcade vehicle/spline AI first.
- Additive scene loading and larger world streaming: safe because AD-11 fixes three scenes and MVP_Run first.
- Persistent accounts, cloud saves, analytics, anti-cheat, and secure economy: safe because AD-15 fixes transient sessions for MVP.
- Console, mobile, WebGL, and store compliance: safe if Windows PC development builds are explicitly bound in the spine.
- Large 3D asset volume and polished pipeline automation: safe because AD-13/AD-14 gate assets and greybox first.
- Rich city content and destructible cities: safe and aligned with SPEC non-goals.

Unsafe or conditionally unsafe:

- Exact health, revive, vehicle destruction, and team-wipe timing rules: unsafe if fully deferred, because CAP-7 requires team-wipe failure. Keep exact tuning deferred, but add a minimal authoritative player life-state contract now.
- Dedicated servers and host migration: mostly safe, but only if AD-4 or conventions specify MVP host-quit/disconnect behavior so Lobby/UI and Run do not invent incompatible behavior.
- Full boss design: safe only if the spine explicitly says MVP victory uses a simple host-owned boss endpoint/test object. Otherwise it leaves CAP-7 implementation ambiguous.

## Silent Or Near-Silent Dimensions

- Content-rating/tone boundary: silent. The SPEC asks what boundaries apply to threats, pissing, fights, and absurd provocation actions. The spine should decide, defer, or keep it open.
- Multiplayer validation/acceptance harness: silent. The stack includes Multiplayer Play Mode, but the spine does not say what validates the online co-op success signal, the four-player cap, remote Relay path, or failure modes.
- Operational/environmental envelope: partial, not silent. The spine covers no backend, secrets, package pinning, and deferred platforms, but it should explicitly bind Windows PC development builds first, UGS dev/non-production environment expectations, and minimum network failure handling.
- Sandbox/content ownership: near-silent. The compact sandbox stop is central to CAP-6 but lacks a feature owner and lifecycle contract.

## Editorial Structure And Prose Notes

Purpose/audience read: this document exists to help future implementation agents and the solo builder keep the first Unity online co-op vertical slice coherent.

Chosen structure model: Reference/Database with a short conceptual preface. The AD schema is consistent and easy to scan. No major prose-only blockers were found.

Highest-value structural fixes:

| Pass | Original Text | Revised Text | Changes |
| --- | --- | --- | --- |
| structure | Capability map only lists CAP IDs and feature locations. | Add a compact "MVP Slice Invariant" AD or add a fourth "MVP proof fixture" column to the map. | Preserves SPEC cardinalities and success fixtures without bloating every AD. |
| structure | Deferred includes health/team-wipe timing and full boss design. | Split "minimal MVP owner/state contract" from "exact tuning/full design deferred." | Prevents unsafe deferral while keeping unknown design detail out of the spine. |
| prose | Stop zone, sandbox zone, town, Rage Road event, Rage Road crisis. | Normalize to "sandbox stop" and "Rage Road event" unless a distinct concept is intended. | Reduces architecture ambiguity from vocabulary drift. |

## Recommended Fix Order

1. Add an MVP slice/cardinality AD.
2. Add minimal authoritative contracts for player life-state, Rage Road event lifecycle, and simple boss endpoint.
3. Add sandbox stop ownership and economy/upgrade owner rules.
4. Update or justify the Unity Multiplayer Services package pin.
5. Add Open Questions/Deferred entries for content rating and any remaining boss/death tuning.
6. Add a short operational/validation convention for Windows PC dev builds, UGS environment, remote Relay validation, and host/network failure behavior.

