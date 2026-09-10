# Epic 3 Context: Vehicle Sandbox & Shared Driving Module

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

## Goal

Players can enter or start inside the one shared player car, drive it around one simple test route, use a local per-player camera/input setup, collide with the route environment, and recover from a stuck or flipped car, ending the epic in a launchable, testable driving state. This is the first module built directly on top of Epic 2's online spawn checkpoint: it proves the core driving primitive - the thing the rest of the MVP loop (passenger chaos, AI traffic, Rage Road, on-foot confrontation) all attaches to - while staying isolated enough to test alone in a dev sandbox scene before any chaos systems exist.

## Stories

- Story 3.1: Shared Player Car Prefab and Vehicle Module Boundary
- Story 3.2: Driver Control and Local Vehicle Camera
- Story 3.3: Seat Entry, Exit, and Passenger Presence
- Story 3.4: Simple Route, Collision, and Vehicle Recovery
- Story 3.5: Vehicle Damage Hook and Team-Wipe Contract Stub
- Story 3.6: Epic 3 Driving Playable Checkpoint

## Requirements & Constraints

- Exactly one shared player car exists for the MVP; it must be loadable both in an isolated dev sandbox scene and in the integrated run scene, and must run without requiring AI traffic or Rage Road systems.
- The vehicle module owns only vehicle concerns - it must not own lobby, economy, boss, or passenger-action rules; cross-module coordination happens through shared state/interfaces, not direct mutation.
- Movement-affecting input is submitted as player intent and validated/simulated on the host; the local camera is a presentation-only concern and is never synchronized over the network.
- Seat occupancy (driver vs. passenger) is host-authoritative; exactly one driver seat and multiple passenger seats exist, and a disconnecting/dying occupant causes the host to release, reassign, or disable that seat with visible feedback.
- Collisions now drive a concrete, host-computed damage contract instead of a stub: both player health/hearts/downed/revive/respawn rules and car health/handling-degradation rules are derived from collision events and live in shared state (`NetworkedPlayerState` for players, vehicle-owned state for the car).
- Hearts are a consumable run resource spent only when a player's revive window expires with nobody reviving them; this supersedes the Story 2.7 placeholder that always refilled hearts on respawn.
- The car accumulates damage and, past internal thresholds, applies discrete handling-degrading effects (steering, top speed, braking) without repeating the same effect twice; a car that runs out of health becomes undrivable and forces every occupant out on foot through the seat-exit flow already established in Stories 3.3/3.4.
- The all-players-dead condition must be detectable from shared player state, but the actual full-run restart stays stubbed until Epic 7 integration; none of this vehicle/player damage logic may directly mutate economy, boss, or rage state.
- Replacing placeholder vehicle art must never change vehicle gameplay identity, network registration, or collider behavior.
- Exact tuning numbers (damage amounts, thresholds, revive-window length) are story-level detail owned by Story 3.5 - treat them as host-computed and shared-state-visible, not as fixed constants other stories should hard-code.
- The epic must end in a state a developer can actually launch (editor and Windows dev build) and manually verify: walk, enter car, drive, exit, recover from stuck, with a local host/client check confirming shared car state is visible to clients.

## Technical Decisions

- Vehicle physics is intentionally simple: an arcade Rigidbody-based controller, not WheelColliders or realistic simulation. Broad traffic/physics realism is explicitly deferred.
- Host-simulated movement model: the host simulates the player car's Rigidbody, collisions, and any block/ram-style contacts; clients send driver-input intent only and receive replicated/smoothed transforms back. Client-side prediction, if any, is presentation-only for now.
- Netcode ownership: gameplay-authoritative NetworkObjects (the car) are host-owned; gameplay NetworkVariables are server-write by default. Player actions (drive input, seat requests) become typed ServerRPC intents; host validation checks actor, run phase, player mode/seat, and similar guards before mutating state.
- Player state model: each connected player has one `NetworkedPlayerState` with `PlayerMode` (`Driver | Passenger | OnFootStop | OnFootRageRoad | Spectating`) and `PlayerLifecycle` (`Alive | Downed | Dead`). Seat assignment is host-owned; team-wipe is computed only from every connected player reaching `Dead`.
- Damage-type effects (wheel/engine/brake-style handling degradation) are applied on top of the arcade Rigidbody controller as modifiers to existing handling parameters (turn rate, top speed, braking force), not as a separate physics model - keeping Story 3.2's driver control and Story 3.4's collision/recovery loop as the single source of vehicle feel.
- Camera/input pattern: each player has a local-only Cinemachine camera rig; Input System action maps feed a local PlayerIntent layer that never mutates shared gameplay state directly.
- Code lands in the `Vehicles` feature slice (`RoadRage.Features.Vehicles`), referencing `Shared` and Run-facing interfaces only; it does not reference other feature slices directly. Networked components use a `Networked` prefix.
- Scene placement follows the three-scene seed: the route/vehicle content lives in `MVP_Run` for the integrated game and in `Dev_VehicleSandbox` for isolated iteration; no additive scene loading yet.
- Greybox-to-art stability: gameplay prefab identity, NetworkObject registration, gameplay components, colliders, and any definition ids must stay stable when placeholder art is later replaced; gameplay colliders are authored separately from decorative meshes.
- Build/test envelope: validation includes a local Multiplayer Play Mode host/client smoke test confirming shared car state replicates to clients, run against a Windows development build target.

## Cross-Story Dependencies

- Epic 3 builds directly on Epic 2's online player-spawn checkpoint (networked players already exist in the empty world) and Epic 1's on-foot movement (used to reach/leave the car).
- Story 3.5 depends on Story 3.4's collision detection as its damage trigger, and on Stories 3.3/3.4's seat-exit flow to force occupants out when the car becomes undrivable. It revises Story 2.7's health/respawn placeholder (hearts no longer auto-refill on respawn) rather than replacing it wholesale. The all-dead detection and restart-condition logging it enables is still a deliberate stub - the real restart flow is completed in Epic 7, not here.
- The driver/passenger seat contract from Story 3.3 is a hard prerequisite for Epic 4 (passenger chaos actions require an occupied passenger seat) and for Epic 6's on-foot transition (players leave car seats to enter confrontations/sandbox stops).
- Epic 5's AI traffic shares the same route/environment established in Story 3.4 but is out of scope here - Epic 3 must remain testable with zero AI vehicles present.
