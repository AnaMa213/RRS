# Epic 5 Context: NPC Response Foundation and Future Traffic

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

## Goal

Epic 5 establishes configurable, data-driven NPC Rage/Fear response, unifies solo and online session start onto one host-authoritative path, and delivers the first testable slice of AI traffic: three vehicles that follow a route, can be individually targeted for rage/fear actions, change behavior as their rage evolves, and can trigger a Rage Road event. The session-start unification (Story 5.3) was inserted by the 2026-09-14 course correction to fix a structural gap dating back to Epic 1 (solo never started a Netcode session), which blocked solo verification of every later story in this epic. The epic stays a sandbox of independently testable foundations — no complete traffic controller, boss, final archetype list, or level-specific behavior — preparing MVP 2 assembly (city/highway, checkpoints, confrontation) without locking it in or depending on Epic 6's economy.

## Stories

- Story 5.1: Configurable NPC Rage/Fear Foundation
- Story 5.2: Basic AI Route Following and Recovery
- Story 5.3: Unified Solo and Online Session Start
- Story 5.4: Rage-Driven AI Behavior States
- Story 5.5: Networked AI Rage Targeting
- Story 5.6: Rage Road Event Trigger
- Story 5.7: AI Traffic Networking and Client Presentation
- Story 5.8: Epic 5 AI Traffic Playable Checkpoint

## Requirements & Constraints

- Rage/Fear is per-target, host-authoritative, and data-driven (static tendencies; an effect can move rage, fear, or both). No complete traffic controller, boss, final archetype list, or level-specific behavior belongs in this epic, and the foundation must stay testable alone in a sandbox.
- AI traffic starts at three vehicles following waypoints/lanes at testable speeds, with self-recovery when stuck, flipped, or off the playable route; movement must be deterministic enough for host-authoritative network tests, and traffic must be disable-able/isolatable in a dev sandbox.
- Solo and online share one session-bootstrap path: Start Game must behave exactly like Create Lobby (private Steam lobby, host as owner, StartHost before MVP_Run loads), a second player must be able to join mid-run by code, and every NetworkBehaviour-driven feature (AI traffic, rage, passenger actions, damage) must behave identically regardless of entry point. Legacy solo-only code is removed, not duplicated elsewhere, and existing PlayMode fixtures across Epics 1-5 must keep passing under the unified path.
- Each AI vehicle's behavior state is independent — one vehicle changing state must never force others — and transitions are host-authoritative and visible through movement plus a non-color-only UI/debug signal.
- Rage/Fear targeting is per-player and host-validated: only spawned AI carrying the required networked AI and rage/fear state are eligible; an explicit lock persists until the player replaces or clears it (no silent reassignment, no fallback target when the lock is out of range); without a lock, an action only affects the nearest eligible target within its own range and never creates a persistent lock; the mechanism must generalize across current and future rage/fear actions (horn, provocations, etc.).
- A Rage Road trigger produces exactly one event at a time with an identified target and visible feedback; simultaneous triggers are arbitrated by a documented host-side rule (e.g. first-trigger-wins, priority, queue); duplicate triggers on an active event are prevented. Only trigger/pending/active/resolved visibility is in scope — resolution belongs to Epic 6.
- Clients see AI positions, behavior states, rage labels, and event state synchronized from host-owned state only; clients cannot force behavior changes, and late joiners receive current state. The design must remain workable for four players and three AI vehicles.
- The epic's checkpoint must be exercised in both `MVP_Run` and `Dev_RageSandbox` with local and online smoke tests confirming host-authoritative updates, and must record tuning assumptions for Epic 6.

## Technical Decisions

- AD-26 (session lifecycle, updated 2026-09-14): Bootstrap owns persistent services, Steamworks init, and NetworkManager lifetime; MainMenuLobby creates or joins the private Steam lobby, then the host starts networking before the synchronized MVP_Run load. "Start Game" and "Create Lobby" are two entry points into that same path — a single-member lobby is not a separate offline mode — and no scene load may bypass lobby creation/host start. `RunCompositionRoot` spawns host-owned gameplay NetworkObjects and rebuilds the run on team wipe; a lost host sends clients back to MainMenuLobby with an error, never host migration.
- Rage/Fear extends the existing host-owned `NetworkedRageState` (`Features/Rage`, from Epic 4) rather than introducing a second source of truth; `Features/Vehicles` consumes that state through a narrow interface/event and never mutates it directly (one-way, feature-sliced).
- `NetworkedAIVehicleState` (`Features/Vehicles`) currently holds only a route index; this epic adds route/behavior/recovery there while keeping tuning and rage/fear reaction logic out of it.
- Rage Road events are a host-owned state machine on Run: `Idle -> Triggered -> Confrontation -> Resolved -> RewardGranted`. Epic 5 only drives it to a triggered/pending-visible state; Epic 6 owns Confrontation onward.
- Authored tuning, archetype tendencies, and effects are lowercase-id ScriptableObjects registered in the shared definition catalog at bootstrap; session values live only in `NetworkVariable`s, never in the ScriptableObjects.
- Host-authoritative throughout: gameplay `NetworkVariable`s are server-write; clients send typed Intents validated by the host (actor, target via `NetworkObjectReference`, range, cooldown, payload version) before any mutation. Input handlers and camera stay local presentation and never mutate shared state.
- Conventions: `Networked`-prefixed state components, `Def`-suffixed definitions, `Intent`-suffixed intents, feature-prefixed logs (`[Rage]`, `[Vehicles]`). Stack: Unity 6 / C# 9.0 / URP / Netcode for GameObjects over Steam transport. `Dev_RageSandbox` isolates traffic/rage work; `MVP_Run` remains the integrated greybox validation scene.

## UX & Interaction Patterns

- AI behavior-state changes must be legible through a readable label (rage HUD, existing debug views) in addition to movement — never color alone.
- `T` locks the nearest eligible AI and points the local camera at it; `Y` explicitly cycles the lock; both are per-player and independent of any fixed point or color.
- A Rage Road trigger needs visible, textual feedback, not color alone.
- Session-start unification changes no screen: Start Game and Create Lobby keep their existing placement; only the shared underlying mechanism changes. This epic introduces no new menu screens and stays greybox/replaceable.

## Cross-Story Dependencies

- 5.1 extends the Epic 4 rage foundation (Stories 4.1-4.4, reviewed and done) and must stay compatible with existing calls.
- 5.3 was inserted after 5.2 revealed AI traffic only ran online, because solo never started a Netcode session; 5.4-5.8 cannot be solo-verified until 5.3 lands.
- Internal order: 5.2 and 5.3 each depend on 5.1; 5.4 depends on 5.2 and 5.3; 5.5 depends on the rage/fear foundation, passenger actions, and the unified session path; 5.6 depends on 5.4 (a vehicle able to reach a trigger state); 5.7 depends on 5.3-5.6 and existing networking; 5.8 validates all of it locally and online.
- Upstream: driving, seats, and damage from Epic 3; host-validated passenger actions from Epic 4.
- Downstream: Epic 6 consumes the Rage Road event (resolution, confrontation, economic reward) and this epic's tuning; Epic 6 and Epic 7 inherit the unified session-start path with no renumbering needed on their side. Epic 5 ships event state and trigger arbitration only — not resolution, city/highway, boss, or checkpoints.
