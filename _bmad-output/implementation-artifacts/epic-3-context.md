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
- Vehicle test-damage must be able to update player lifecycle through shared state and support detecting an all-players-dead condition, but the actual full-run restart stays stubbed until later integration; the module must not directly mutate economy, boss, or rage state.
- Replacing placeholder vehicle art must never change vehicle gameplay identity, network registration, or collider behavior.
- Exact damage numbers, revive windows, and vehicle-destruction consequences are intentionally left open until a later combat/Rage Road greybox exists - don't over-specify these in this epic.
- The epic must end in a state a developer can actually launch (editor and Windows dev build) and manually verify: walk, enter car, drive, exit, recover from stuck, with a local host/client check confirming shared car state is visible to clients.

## Technical Decisions

- Vehicle physics is intentionally simple: an arcade Rigidbody-based controller, not WheelColliders or realistic simulation. Broad traffic/physics realism is explicitly deferred.
- Host-simulated movement model: the host simulates the player car's Rigidbody, collisions, and any block/ram-style contacts; clients send driver-input intent only and receive replicated/smoothed transforms back. Client-side prediction, if any, is presentation-only for now.
- Netcode ownership: gameplay-authoritative NetworkObjects (the car) are host-owned; gameplay NetworkVariables are server-write by default. Player actions (drive input, seat requests) become typed ServerRPC intents; host validation checks actor, run phase, player mode/seat, and similar guards before mutating state.
- Player state model: each connected player has one `NetworkedPlayerState` with `PlayerMode` (`Driver | Passenger | OnFootStop | OnFootRageRoad | Spectating`) and `PlayerLifecycle` (`Alive | Downed | Dead`). Seat assignment is host-owned; team-wipe is computed only from every connected player reaching `Dead`.
- Camera/input pattern: each player has a local-only Cinemachine camera rig; Input System action maps feed a local PlayerIntent layer that never mutates shared gameplay state directly.
- Code lands in the `Vehicles` feature slice (`RoadRage.Features.Vehicles`), referencing `Shared` and Run-facing interfaces only; it does not reference other feature slices directly. Networked components use a `Networked` prefix.
- Scene placement follows the three-scene seed: the route/vehicle content lives in `MVP_Run` for the integrated game and in `Dev_VehicleSandbox` for isolated iteration; no additive scene loading yet.
- Greybox-to-art stability: gameplay prefab identity, NetworkObject registration, gameplay components, colliders, and any definition ids must stay stable when placeholder art is later replaced; gameplay colliders are authored separately from decorative meshes.
- Build/test envelope: validation includes a local Multiplayer Play Mode host/client smoke test confirming shared car state replicates to clients, run against a Windows development build target.

## Cross-Story Dependencies

- Epic 3 builds directly on Epic 2's online player-spawn checkpoint (networked players already exist in the empty world) and Epic 1's on-foot movement (used to reach/leave the car).
- Story 3.5's vehicle-damage-to-lifecycle hook is a deliberate stub: it only needs to update `NetworkedPlayerState` and detect the all-dead condition; the real restart flow is completed in Epic 7, not here.
- The driver/passenger seat contract from Story 3.3 is a hard prerequisite for Epic 4 (passenger chaos actions require an occupied passenger seat) and for Epic 6's on-foot transition (players leave car seats to enter confrontations/sandbox stops).
- Epic 5's AI traffic shares the same route/environment established in Story 3.4 but is out of scope here - Epic 3 must remain testable with zero AI vehicles present.
