# Gameplay Model

> **Supersession — 2026-09-13:** The following original loop is retained as MVP 2 direction, not MVP 1 scope. See `course-correction-2026-09-13.md` for locked requirements and ownership boundaries.

## Current foundation model

MVP 1 validates independent bricks. Persistent profile cosmetics, Steam identity, lobby/session state, and runtime player representation are distinct. Economy and inventory are individual; host authority owns multiplayer runtime mutation. Rage/Fear is configurable response data, not a Level 1-specific controller.

This companion holds the load-bearing gameplay model that is too detailed for the five-field SPEC kernel.

## MVP 2 Target Core Loop

1. Players drive a shared road route cooperatively.
2. Passengers use active chaos actions to provoke, interfere, collect, help, or amplify incidents.
3. Nearby enemy vehicles update their own rage independently.
4. Escalation creates happenings or a Rage Road crisis.
5. Resolving hostile drivers pays meaningful money.
6. Money buys upgrades or preparation at compact sandbox stops.
7. On-foot actions return value to the same driving, rage, money, and upgrade loop.
8. A run restarts if every player dies and ends in victory when the boss dies.

## Passenger Actions

Passenger play includes provoking, threatening, throwing trash, hindering, collecting, helping, light sabotage, and triggering or amplifying incidents. The MVP fixes the first implementation at three passenger actions; the exact three are not locked by the source.

## Vehicle Rage Model

Rage is tracked per enemy vehicle, not globally. A driver can stay calm, become irritated, flee, block, ram, or trigger a confrontation. At maximum rage, behavior may vary by AI vehicle type, including narrowly fleeing, forming a blockade and fighting, or trying to destroy the player car.

## On-Foot Play

On-foot play supports buying upgrades, collecting trash, fighting during Rage Roads, pissing, and interacting with sandbox stops. Some actions may also exist in-car with different risk, precision, or timing.

## Composable Module Model

Road Rage Simulator should be built from gameplay modules that can work in small development slices and then compose into the full run. Vehicle, OnFoot, PassengerActions, Rage, Economy, Lobby/Network, Run, Boss, SandboxStops, and UI may be tested separately, but the integrated game uses the shared run state and architecture contracts as the single source of truth.

Module independence means a developer can test a piece without building the whole game first. It does not mean separate engines, separate input/camera systems, separate money models, separate player death models, or duplicate network state.

## Sandbox Stops

Towns and stops are compact sandbox zones containing happenings, money opportunities, purchases, and incidents. They are not destructible cities, and their value is measured by how well they feed preparation and escalation for the road loop.

## Economy Role

Meaningful money comes mainly from road rage: provoking, confronting, and beating hostile drivers. Every player earns and spends their own money; rewards and purchases are not shared. Exact reward amounts remain provisional.

## MVP 2 Run Outcome

The game supports online co-op for up to four players. When the party dies, MVP 2 restarts from the latest inter-level checkpoint and resets temporary run resources; exact recovery rules remain open. Boss progression and victory are MVP 2 assembly work.
