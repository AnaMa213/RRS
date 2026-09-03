# Module Composition

This companion captures how the MVP should be split into independently testable slices without losing one integrated game.

## Principle

Each gameplay module should be useful in isolation during development and composable in the real MVP run. The shared Unity project, rendering stack, input layer, network authority model, player lifecycle, economy state, rage state, and run outcome model remain common.

## Modules

| Module | Works alone by proving | Composes into the full game by | Must not own |
| --- | --- | --- | --- |
| Vehicle | A car drives a simple route with placeholder traffic. | Run uses it as the shared road vehicle and AI traffic surface. | Player lifecycle, wallet, lobby state, or final run outcome. |
| OnFoot | A player can move and interact in a compact zone. | Run switches player mode into sandbox stop or Rage Road play. | Separate progression, separate death model, or separate camera truth. |
| PassengerActions | Three actions can be triggered and visualized. | Host applies actions to rage, incidents, resources, or crew help. | Direct money grants, direct run phase changes, or local-only gameplay truth. |
| Rage | Three enemy vehicles can hold independent rage states. | Rage requests incidents or Rage Road triggers through Run. | Vehicle movement authority, rewards, or player life state. |
| Economy | A shared crew wallet grants one reward and buys one upgrade. | Run and Rage Road resolution call host-owned economy transactions. | Per-player wallet systems or persistent backend economy. |
| Lobby/Network | Host creates a room and clients join by code. | MainMenuLobby hands the connected session into MVP_Run. | Gameplay rules, rewards, boss death, or content spawning policy. |
| Run | A full run can start, reset, and end. | Run composes every other module into the MVP flow. | Final art production or external account/backend persistence. |
| Boss | A simple endpoint can die and emit victory. | NetworkedRunState declares victory from boss death. | Rich boss design, roguelite progression, or standalone boss campaign. |
| SandboxStops | One compact stop exposes interactions, purchases, and incidents. | Sandbox stop interactions prepare or escalate the road loop. | Deep town simulation or destructible city systems. |
| UI | The player can see lobby, run state, money, rage, actions, failure, and victory. | UI reads shared state and sends player intent through feature interfaces. | Authoritative gameplay mutation. |

## Development Slices

Use small test scenes or test setups to prove modules before full integration:

| Slice | Purpose |
| --- | --- |
| `Dev_VehicleSandbox` | Tune arcade driving and simple route movement. |
| `Dev_OnFootSandbox` | Test on-foot movement and compact interactions. |
| `Dev_RageSandbox` | Test three AI vehicles with independent rage. |
| `Dev_LobbySmokeTest` | Test host, join code, Relay, and player cap. |
| `MVP_Run` | Compose the real online run. |

## Composition Rules

- A module may use mock data or placeholder visuals in its dev slice.
- A module may not define its own replacement for shared player lifecycle, crew wallet, run phase, rage truth, input truth, camera truth, or network authority.
- The full MVP run uses the architecture spine as the binding integration contract.
- Integration beats isolation: if a module works alone but cannot compose into `MVP_Run`, it is not done.
