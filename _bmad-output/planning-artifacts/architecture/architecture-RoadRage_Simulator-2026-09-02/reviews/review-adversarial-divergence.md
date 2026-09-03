# Adversarial Divergence Review

Content reviewed: `ARCHITECTURE-SPINE.md`

Lens: adversarial

Standing context note: workflow persistent fact `file:{project-root}/**/project-context.md` had no matching file in this workspace, so this review uses the architecture spine and its bound SPEC companions.

Scope: ways two future feature builders could obey the current ADs and conventions while still producing incompatible systems across shared state shapes, ownership, networking, vehicle/rage/passenger/on-foot boundaries, asset pipeline, and scenes.

## Findings

### Finding 1

- **location:** `ARCHITECTURE-SPINE.md:43`, `ARCHITECTURE-SPINE.md:78`, `ARCHITECTURE-SPINE.md:186`
- **trigger_condition:** "Shared code stays small" and "cross-feature coordination goes through Run orchestration, shared interfaces/events, or networked state objects" do not define the canonical shared runtime state schema.
- **divergence construction:** Builder A creates `NetworkedRunState` with nested `PlayerState`, `AIVehicleState`, `RageState`, and `UpgradeState` structs because the ER diagram implies a central graph. Builder B creates separate feature-owned `NetworkedPlayerHealth`, `NetworkedVehicleRage`, `NetworkedCrewWallet`, and `NetworkedBossState` components because AD-2 says slices own gameplay and AD-3 only says the host mutates. Both obey the ADs, but UI, save/reset, reward grants, and run outcome code disagree about where the truth lives.
- **guard_snippet:** Add an AD or convention that names the canonical runtime state objects and shape boundaries, for example: `NetworkedRunState` owns `RunPhase`, session seed, restart/victory transitions, crew wallet, and registry references; feature NetworkBehaviours own only per-entity local state; all cross-feature consumers read through shared read-only interfaces.
- **potential_consequence:** Features compile independently but cannot integrate without adapter scripts, duplicated NetworkVariables, and late rewrites of every consumer of run, player, vehicle, economy, and boss state.

### Finding 2

- **location:** `ARCHITECTURE-SPINE.md:84`, `ARCHITECTURE-SPINE.md:96`, `ARCHITECTURE-SPINE.md:164`
- **trigger_condition:** Host authority is specified, but NetworkObject ownership and write-permission conventions are not.
- **divergence construction:** Builder A gives each player's avatar and car-seat object client ownership and uses owner-authoritative RPCs for responsive movement/actions, then host-validates important outcomes. Builder B makes all gameplay NetworkObjects server-owned and treats clients as pure intent emitters. Both can claim "host validates and mutates," but their RPC attributes, NetworkVariable write permissions, spawn paths, and despawn assumptions conflict.
- **guard_snippet:** Tighten AD-3 with a Netcode ownership convention: gameplay-authoritative NetworkObjects are host-owned; client-owned objects are limited to local input/presentation proxies; all mutating RPCs are `ServerRpc` intent calls with explicit validation; NetworkVariable write permission is server-only unless an AD names an exception.
- **potential_consequence:** Passenger actions, vehicle control, on-foot interactions, and respawn logic break when prefabs are wired with incompatible owner permissions and RPC trust assumptions.

### Finding 3

- **location:** `ARCHITECTURE-SPINE.md:96`, `ARCHITECTURE-SPINE.md:276`
- **trigger_condition:** Run outcome is centralized, but death, revive, downed state, vehicle destruction, and "all players are dead" are deferred without a temporary contract.
- **divergence construction:** Builder A implements team wipe as all player characters in `Dead` state, with the car treated as an independent object. Builder B implements team wipe as player car health reaching zero, with passengers still alive but run-failed. Both satisfy CAP-7 wording once all "players" are dead or the boss dies, but they expose incompatible health events to `NetworkedRunState`.
- **guard_snippet:** Add an MVP death-state AD: define the minimum player lifecycle enum, whether car destruction maps to player death, who raises death/revive events, and the exact predicate `NetworkedRunState` uses for team wipe until combat is redesigned.
- **potential_consequence:** The first Rage Road fight, boss endpoint, UI, and restart flow will each implement their own failure logic, undermining AD-5 before the first multiplayer loop is testable.

### Finding 4

- **location:** `ARCHITECTURE-SPINE.md:108`, `ARCHITECTURE-SPINE.md:114`, `ARCHITECTURE-SPINE.md:262`
- **trigger_condition:** Vehicle and rage ownership boundaries are split across `Vehicles` and `Rage` without an integration contract.
- **divergence construction:** Builder A puts rage state directly on each AI vehicle prefab because AD-8 says each AI vehicle has its own rage state machine. Builder B keeps a separate Rage feature registry keyed by AI vehicle NetworkObject because CAP-3 maps rage to `Features/Rage`. Both obey the text, but vehicle behavior, passenger targeting, state replication, and despawn cleanup disagree on whether rage is a component, service record, or child object.
- **guard_snippet:** Add a convention defining the canonical rage attachment point: one `NetworkedRageState` component per enemy vehicle NetworkObject, owned by the Rage feature but attached to the vehicle prefab through a documented prefab contract; Vehicles consume rage through an interface/event, not direct mutation.
- **potential_consequence:** AI vehicles can move using one rage source while passenger actions and UI update another, producing host-consistent but feature-inconsistent gameplay.

### Finding 5

- **location:** `ARCHITECTURE-SPINE.md:114`, `ARCHITECTURE-SPINE.md:120`, `ARCHITECTURE-SPINE.md:163`
- **trigger_condition:** Passenger actions target explicit vehicles, but target identity semantics are not defined beyond "runtime network object identity comes from Netcode."
- **divergence construction:** Builder A sends `NetworkObjectId` in each `PassengerActionIntent`. Builder B sends a stable authored vehicle id or lane index because the Data ids convention forbids hand-written ids only for runtime identity, not targeting payloads. Both can satisfy AD-9 and AD-12, but target resolution fails across spawn/despawn, late join, pooled AI vehicles, or route resets.
- **guard_snippet:** Add an intent DTO convention: networked target references use `NetworkObjectReference` or the approved Netcode-safe equivalent; authored ids identify definitions only; intents must include action id, actor player id, target reference, host timestamp/tick, and optional payload version.
- **potential_consequence:** Passenger actions will appear valid in local tests but hit the wrong AI vehicle or no vehicle at all when network timing, pooling, or restart enters the slice.

### Finding 6

- **location:** `ARCHITECTURE-SPINE.md:120`, `ARCHITECTURE-SPINE.md:126`, `ARCHITECTURE-SPINE.md:164`
- **trigger_condition:** "Client intent" is repeated, but there is no canonical intent pipeline or validation ownership.
- **divergence construction:** Builder A creates one generic `PlayerIntent` stream in Shared and lets each feature subscribe. Builder B creates feature-specific ServerRPC methods like `UsePassengerActionServerRpc`, `InteractOnFootServerRpc`, and `BuyUpgradeServerRpc`. Both obey AD-10 and the state mutation convention, yet replay prevention, cooldowns, authority checks, and result events are implemented differently.
- **guard_snippet:** Add an AD for intent flow: input adapters produce local intent structs; feature intent clients send typed ServerRPCs through a shared validation base; host validators check actor, phase, cooldown, target, range, and payload version; accepted outcomes emit host-owned events/results.
- **potential_consequence:** Features cannot share cooldowns, permissions, denial feedback, or network diagnostics, and bugs become invisible because every feature invents its own trust boundary.

### Finding 7

- **location:** `ARCHITECTURE-SPINE.md:102`, `ARCHITECTURE-SPINE.md:132`, `ARCHITECTURE-SPINE.md:265`
- **trigger_condition:** On-foot play must feed the same loop, but the architecture does not define whether on-foot is a mode on the player, a separate avatar, a scene substate, or a vehicle-seat state.
- **divergence construction:** Builder A swaps the player into an on-foot prefab with its own NetworkObject, collider, camera target, health, and interaction set. Builder B keeps the player attached to the car and only enables local interaction UI in stop zones or Rage Road arenas. Both obey AD-6, AD-10, AD-11, and AD-14, but their assumptions about spawn points, camera, input maps, collision layers, death, and return-to-driving events are incompatible.
- **guard_snippet:** Add an MVP locomotion/mode convention: define `PlayerMode` values, whether on-foot uses a separate networked avatar or a mode component on the player entity, how seat assignment works, and which host event transitions between driving, stop-zone, Rage Road, and returned states.
- **potential_consequence:** Passenger actions, fights, purchases, and death handling will depend on incompatible representations of "the player," forcing a rewrite at the exact boundary where the core loop must prove itself.

### Finding 8

- **location:** `ARCHITECTURE-SPINE.md:132`, `ARCHITECTURE-SPINE.md:263`, `ARCHITECTURE-SPINE.md:279`
- **trigger_condition:** Rage Road, stop zone, route, and boss validation all live in `MVP_Run`, but scene-local object discovery and reset conventions are unspecified.
- **divergence construction:** Builder A drops all route, stop, Rage Road, and boss prefabs directly into the `MVP_Run` scene and wires references in the Inspector. Builder B spawns them at runtime from Run services so host restart can rebuild the route. Both obey the three-scene seed, but restart, late join, test setup, and prefab ownership do not agree.
- **guard_snippet:** Add a scene composition convention: `MVP_Run` may hold only bootstrap anchors and authored layout roots; host-spawned gameplay NetworkObjects are registered through `RunCompositionRoot`; restart destroys and respawns from known prefab/layout definitions; scene object references must be resolved through serialized roots or registries, not global finds.
- **potential_consequence:** Team-wipe restart and boss victory may work in one builder's scene but fail once another feature expects runtime-spawned registries or inspector-only references.

### Finding 9

- **location:** `ARCHITECTURE-SPINE.md:138`, `ARCHITECTURE-SPINE.md:162`, `ARCHITECTURE-SPINE.md:214`
- **trigger_condition:** ScriptableObject ids are required, but there is no definition registry, uniqueness rule, versioning rule, or Resources/Addressables/loading convention.
- **divergence construction:** Builder A loads all `*Def` assets from `Resources` by stable id. Builder B uses serialized arrays on feature installers or scene roots. A third builder might prepare Addressables because assets are feature-owned. All obey AD-12, but action ids, upgrade ids, rage thresholds, and boss config cannot be resolved consistently by host and clients.
- **guard_snippet:** Add an authored-data convention: static definitions live under feature-local `ScriptableObjects/<Feature>` folders, are registered by a single Shared definition catalog at bootstrap, ids are globally unique and validated in editor checks, and network payloads send ids only after both host and clients load the same catalog version.
- **potential_consequence:** Multiplayer sessions can connect with mismatched definitions, passenger action payloads can resolve to different effects, and balance data becomes scattered even though every asset is technically a ScriptableObject.

### Finding 10

- **location:** `ARCHITECTURE-SPINE.md:144`, `ARCHITECTURE-SPINE.md:214`, `ARCHITECTURE-SPINE.md:267`
- **trigger_condition:** The Blender intake gate defines cleanup steps, but not import settings, prefab variant rules, collider/LOD conventions, material ownership, or source-to-prefab traceability.
- **divergence construction:** Builder A exports GLB with embedded materials and generates colliders in Unity per prefab. Builder B exports FBX with external textures, shared materials, manually authored colliders, and prefab variants. Both pass through Blender and scale-test in Unity, but their assets behave differently in physics, batching, replacement, and source updates.
- **guard_snippet:** Add an asset pipeline convention: approved units/origin/forward axis, naming suffixes, folder layout for source/export/prefab/materials, collider policy for gameplay-critical assets, material reuse policy, prefab variant policy, and a required source reference component or metadata file.
- **potential_consequence:** Vehicle, stop-zone, boss, and prop assets will look imported correctly while silently breaking physics, replacement workflows, and feature ownership.

### Finding 11

- **location:** `ARCHITECTURE-SPINE.md:43`, `ARCHITECTURE-SPINE.md:78`, `ARCHITECTURE-SPINE.md:196`
- **trigger_condition:** The Shared layer is allowed to contain primitives, networking wrappers, data ids, and presentation helpers, but there is no anti-corruption rule for feature APIs.
- **divergence construction:** Builder A places shared interfaces like `IRageTarget`, `IRewardSink`, and `IPlayerMode` in `Shared/Domain`, keeping features decoupled. Builder B places concrete helper services in Shared because multiple features need them. Both can say Shared stays small, but Shared slowly becomes the real gameplay layer with hidden ownership.
- **guard_snippet:** Add a Shared eligibility convention: Shared may contain pure value types, ids, base network utilities, and narrow interfaces with no feature policy; concrete gameplay services must live in a feature or App/Run composition; any Shared addition must name at least two current consumers and no feature-specific rule.
- **potential_consequence:** Future builders will accidentally bypass feature ownership while technically avoiding direct feature-to-feature mutation, making the feature-sliced architecture cosmetic.

### Finding 12

- **location:** `ARCHITECTURE-SPINE.md:84`, `ARCHITECTURE-SPINE.md:108`, `ARCHITECTURE-SPINE.md:165`
- **trigger_condition:** Host-owned AI decisions and server-time timers are required, but movement simulation boundaries for player car and AI vehicles are not.
- **divergence construction:** Builder A simulates car Rigidbody motion on the host and replicates transforms to clients. Builder B lets the driver client predict car movement and sends throttle/steer intent to the host for reconciliation. Builder C simulates AI spline state as path progress variables rather than physics transforms. Each can claim host authority over decisions and arcade Rigidbody control, but the networking cost, collision truth, rage trigger timing, and camera feel diverge.
- **guard_snippet:** Add a vehicle networking AD: define whether player car and AI vehicles are host-simulated, client-predicted, or hybrid; define replicated state fields, collision authority, rage trigger tick source, and acceptable local smoothing/prediction boundaries.
- **potential_consequence:** Passenger targeting, road incidents, rams, blocks, and Rage Road triggers can disagree by client even when the host is nominally authoritative.

### Finding 13

- **location:** `ARCHITECTURE-SPINE.md:166`, `ARCHITECTURE-SPINE.md:264`
- **trigger_condition:** Economy rules say integer-only currency and significant rewards come from confrontation, but the owner of wallet state and reward transactions is not named.
- **divergence construction:** Builder A implements one crew wallet in `NetworkedRunState` because the loop is cooperative. Builder B implements per-player wallets in `Economy` because players perform actions and buy upgrades. Both obey "money is integer-only" and host grants rewards, but upgrade purchasing, UI totals, reward splitting, and restart persistence conflict.
- **guard_snippet:** Add an MVP economy AD: currency is a host-owned crew wallet or per-player wallets, not both; rewards are granted through one `EconomyService` transaction API; upgrades consume from the same wallet contract and emit a single networked result event.
- **potential_consequence:** Road-rage rewards and upgrades will appear to work in isolation but fail once multiplayer players buy, earn, or view currency at the same time.

### Finding 14

- **location:** `ARCHITECTURE-SPINE.md:90`, `ARCHITECTURE-SPINE.md:224`, `ARCHITECTURE-SPINE.md:237`
- **trigger_condition:** Lobby/Relay is selected, but session lifecycle handoff from Lobby to gameplay NetworkManager is underspecified.
- **divergence construction:** Builder A starts host/client networking in `MainMenuLobby`, loads `MVP_Run`, then spawns gameplay. Builder B loads `MVP_Run` first and lets a run bootstrap connect/spawn after the scene loads. Both obey AD-4 and AD-11, but NetworkManager lifetime, player prefab spawning, disconnect handling, and leave-session cleanup diverge.
- **guard_snippet:** Add a networking lifecycle convention: `Bootstrap` owns persistent services and NetworkManager; `MainMenuLobby` creates/joins sessions; host starts networking before synchronized load into `MVP_Run`; `RunCompositionRoot` handles player spawn/registry; leave-session tears down network state before returning to lobby.
- **potential_consequence:** Builders will create duplicate NetworkManagers, broken scene synchronization, or players that exist in lobby flow but not in the run scene.

### Finding 15

- **location:** `ARCHITECTURE-SPINE.md:120`, `ARCHITECTURE-SPINE.md:138`, `ARCHITECTURE-SPINE.md:150`
- **trigger_condition:** Greybox-first and data-defined actions do not define how placeholder gameplay objects graduate into final prefabs without changing ids and network contracts.
- **divergence construction:** Builder A treats greybox prefabs as disposable scene objects and replaces them with art prefabs later. Builder B creates production prefab names and components from day one with primitive meshes. Both obey AD-14, but network prefab registration, ScriptableObject references, collider identities, and action effect bindings can change during art replacement.
- **guard_snippet:** Add a greybox-to-art convention: gameplay prefabs keep stable prefab identity, network prefab registration, components, colliders, and definition ids while only mesh/material child assets change; final art must be introduced as child/variant replacements after multiplayer validation.
- **potential_consequence:** Art pass can invalidate multiplayer prefab hashes, action bindings, and saved scene references, causing a late integration break after the core loop has been proven.

### Finding 16

- **location:** `ARCHITECTURE-SPINE.md:114`, `ARCHITECTURE-SPINE.md:120`, `ARCHITECTURE-SPINE.md:263`
- **trigger_condition:** Incidents and Rage Road triggers are mentioned as effects, but there is no canonical event taxonomy or phase model connecting rage escalation to confrontation.
- **divergence construction:** Builder A models incidents as spawned NetworkObjects with lifecycle states. Builder B models incidents as events on `NetworkedRunState` that immediately alter rage/money/phase. Both obey AD-8 and AD-9 because effects can spawn incidents or trigger confrontation, but OnFoot, Economy, UI, and Boss integrations cannot depend on a stable incident contract.
- **guard_snippet:** Add a Run/Rage event convention: define MVP event types such as `RageChanged`, `IncidentSpawned`, `ConfrontationStarted`, `ConfrontationResolved`, `RewardGranted`, and `RunPhaseChanged`; state whether each is persistent state, transient RPC/result, or derived UI signal.
- **potential_consequence:** Two features may both react to "Rage Road started" but subscribe to different concepts, producing duplicate fights, missing rewards, or UI that announces a phase the host never entered.

### Finding 17

- **location:** `ARCHITECTURE-SPINE.md:162`, `ARCHITECTURE-SPINE.md:186`, `ARCHITECTURE-SPINE.md:256`
- **trigger_condition:** Folder names are specified, but assembly boundaries, namespace conventions, and dependency direction are not.
- **divergence construction:** Builder A creates one Unity assembly per feature and enforces `Features/*` cannot reference each other. Builder B uses one project assembly and relies on folder discipline. Both obey the structural seed, but compile-time dependencies, test fixtures, and circular references become inconsistent.
- **guard_snippet:** Add a convention for namespaces and `.asmdef` boundaries: `RoadRage.App`, `RoadRage.Shared`, and `RoadRage.Features.<Feature>`; features may reference Shared and approved Run interfaces only; App/Run composition references features; direct feature-to-feature references fail assembly validation.
- **potential_consequence:** The project can look feature-sliced in folders while code dependencies make later extraction, testing, and refactoring expensive or impossible.

## Tightening Candidates

- Add `AD-16 - Canonical Runtime State Shape`: name the host-owned NetworkBehaviours, their fields, and which feature owns each state segment.
- Add `AD-17 - Netcode Ownership And Intent Pipeline`: lock NetworkObject ownership, NetworkVariable write permissions, ServerRPC payload shape, validation checks, and result events.
- Add `AD-18 - Player Mode And Entity Boundary`: define driving, passenger, on-foot, downed/dead, and return-to-car state as one lifecycle contract.
- Add `AD-19 - Vehicle/Rage Attachment Contract`: define how `NetworkedRageState` attaches to AI vehicles, how Vehicles reads it, and how passenger targeting references it.
- Add `AD-20 - Run Scene Composition And Reset`: define `MVP_Run` roots, host spawn registry, restart teardown/respawn, and late-join synchronization.
- Add `AD-21 - Authored Data Catalog`: define ScriptableObject registry, id uniqueness validation, payload id usage, and catalog version matching.
- Add `AD-22 - Asset Import And Prefab Stability`: define Blender export/import settings, colliders, materials, variants, source traceability, and greybox-to-art replacement rules.
- Add conventions for `.asmdef` and namespaces so the feature-sliced architecture is enforceable rather than only descriptive.

