---
name: Road Rage Simulator Beginner Architecture Guide
type: companion-guide
scope: Solo beginner implementation path for the Road Rage Simulator MVP
created: 2026-09-02
updated: 2026-09-02
sources:
  - ARCHITECTURE-SPINE.md
  - mcp-tooling-setup.md
---

# Road Rage Simulator Beginner Architecture Guide

## What We Are Building First

Build one small online co-op vertical slice:

1. One host creates a private room.
2. Other players join with a code or invite link.
3. Up to four players spawn around one shared player car.
4. One route contains three AI vehicles.
5. Passengers trigger three chaos actions.
6. Each AI vehicle tracks its own rage.
7. Rage can trigger one Rage Road confrontation.
8. The team earns money, buys one upgrade, and continues.
9. If every player dies, the run restarts.
10. If the boss dies, the run wins.

Do this with cubes, capsules, and temporary materials before investing in polished models.

## Locked Technical Direction

Use Unity, not Godot, for the MVP.

The project should start as a Unity `6000.6.0f1` Universal 3D/URP project on the Unity 6 Update track. Unity is the practical path here because the target has 3D driving, stylized rendering, online lobby flow, Steamworks Networking Sockets connectivity, and a large multiplayer learning surface. Godot remains attractive for open-source simplicity, but the multiplayer/lobby/vehicle tooling risk is higher for this specific game.

Use a player-hosted model with Steamworks Networking Sockets (course-correction, `sprint-change-proposal-2026-09-02.md` - free regardless of player count, unlike Unity Relay/Multiplayer Services):

- The host player creates a private Steam lobby.
- The invite goes out through the native Steam overlay first, with a shared Lobby ID as a UI-wrapper fallback, unless a platform-native deep link is verified later.
- Steamworks Networking Sockets (Steam Datagram Relay) avoids asking the host to open router ports.
- The host still owns the gameplay simulation.

## Install Order

1. Install Unity Hub.
2. Install Unity `6000.6.0f1`.
3. Create a Universal 3D/URP project named `RRS` (RoadRageSimulator).
4. Install these Unity packages from Package Manager:
   - Netcode for GameObjects `2.13.2`
   - A Steamworks transport (`com.community.netcode.transport.facepunch` or `.steamnetworkingsockets`, installed via git URL, commit/tag pinned)
   - Unity Transport `6.6.0`
   - Universal Render Pipeline `17.6.0`
   - Multiplayer Play Mode `3.0.0`
   - Input System `1.20.0`
   - Cinemachine `6.6.0` if it is not already embedded/enabled
5. Configure the project with the Steamworks test AppID (`480`/Spacewar) for development - no paid Steamworks account needed yet.
6. Initialize the Steamworks SDK (`SteamClient.Init`) and verify private lobby creation (`ISteamMatchmaking`) and Networking Sockets work. Do not add public matchmaking, dedicated servers, or a paid tier unless approved.
7. Install Blender 5.2 LTS.
8. Set up the Unity MCP from `mcp-tooling-setup.md`.
9. Set up the Blender MCP from `mcp-tooling-setup.md`.

## First Project Folders

Create only the folders needed for the first loop:

```text
Assets/
  RoadRage/
    App/
      Scenes/
      Services/
    Shared/
      Domain/
      Definitions/
      Networking/
      Presentation/
    Features/
      Lobby/
      Run/
      Players/
      Vehicles/
      Rage/
      PassengerActions/
      OnFoot/
      SandboxStops/
      Economy/
      Boss/
      UI/
    ArtSource/
      Blender/
      GeneratedReferences/
    ArtExports/
    Materials/
    Prefabs/
    ScriptableObjects/
```

Avoid creating many empty folders beyond this. The point is to make the first vertical slice easy to find.

## Build Milestones

### Milestone 0 - Empty Project Health Check

Goal: Unity opens, packages install, play mode runs.

Acceptance:

- Project opens without console errors.
- `Bootstrap`, `MainMenuLobby`, and `MVP_Run` scenes exist.
- Multiplayer Play Mode can launch one extra local player.

### Milestone 1 - Offline Driving Greybox

Goal: one simple car drives around one route.

Acceptance:

- Car uses arcade Rigidbody movement.
- Camera follows with Cinemachine.
- Route has basic barriers and a start point.
- No networking yet beyond package installation.

### Milestone 2 - Local Multiplayer Spawn

Goal: host plus one local client can spawn.

Acceptance:

- NetworkManager exists in the bootstrap flow.
- Host spawns a player object.
- Client spawns a player object.
- Each player has their own local camera and input.

### Milestone 3 - Lobby And Networking Sockets Smoke Test

Goal: one host creates a room and another player joins via Steam invite or Lobby ID.

Acceptance:

- Host button creates a private session.
- Session cap is four players including the host.
- UI displays a Steam invite option and a Lobby ID.
- Client joins via Steam invite or the Lobby ID.
- Steamworks Networking Sockets carries the connection.
- No manual port forwarding is needed.

### Milestone 4 - Seats And Passenger Intents

Goal: players can be driver/passengers and passenger actions reach the host.

Acceptance:

- One player can drive.
- Other players can trigger three passenger action buttons.
- Actions are sent as intents to the host.
- Each action visibly changes rage, an incident state, a low-value resource/toy, or a crew-help effect.

### Milestone 5 - AI Vehicles And Rage

Goal: three AI vehicles react independently.

Acceptance:

- Three AI cars move along the route.
- Each has its own rage value/state.
- Passenger actions target one AI vehicle.
- AI can become irritated, flee, block, ram, or start confrontation.
- Rage lives on one `NetworkedRageState` attached to each enemy vehicle.

### Milestone 6 - Rage Road And On-Foot Transition

Goal: rage can produce one confrontation zone.

Acceptance:

- A max-rage condition triggers one Rage Road event.
- Players can leave the car in a compact zone.
- The confrontation can be resolved.
- Players return to the car.
- Run owns the Rage Road event lifecycle from trigger to reward.

### Milestone 7 - Money And Upgrade

Goal: confrontation pays meaningful money and one upgrade matters.

Acceptance:

- Winning the confrontation grants currency.
- Currency buys one upgrade.
- The upgrade affects the next driving loop.
- Money is one shared crew wallet for the MVP.

### Milestone 8 - Run Endings

Goal: the run can fail or win.

Acceptance:

- All players dead triggers restart from the beginning.
- Boss dead triggers victory.
- One NetworkedRunState decides this, not scattered scripts.
- The boss can be a simple host-owned endpoint first; it does not need rich boss behavior yet.

### Milestone 9 - First Art Pass

Goal: replace only the most visible greybox assets.

Acceptance:

- One player vehicle, one AI vehicle, and one small environment kit pass through Blender cleanup.
- Assets have clean names, scale, transforms, materials, and colliders.
- Prefabs replace placeholders without changing gameplay scripts.
- Mesh/material children change, but prefab identity, NetworkObject registration, gameplay components, and colliders stay stable.

## How To Use AI And MCPs Safely

Use Codex or Claude to make small, inspectable changes:

- Create one script at a time.
- Create one prefab or scene setup at a time.
- Ask the agent to explain which GameObjects or files changed.
- Run Play Mode after each network or physics change.
- Commit working checkpoints.

Use Unity MCP for:

- Inspecting scenes and console errors.
- Creating simple GameObjects and prefabs.
- Adding NetworkObjects, NetworkBehaviours, and test UI.
- Checking missing components.
- Running small editor tests if available.

Use Blender MCP for:

- Cleaning AI-generated models.
- Applying transforms.
- Fixing scale and origin.
- Reducing material chaos.
- Exporting FBX or GLB.

Do not let any MCP silently:

- Add paid services.
- Change package versions.
- Convert the project to dedicated servers.
- Commit secrets or tokens.
- Replace working gameplay with polished assets before the loop is proven.

## Asset Intake Checklist

Every model must pass this before Unity prefab use:

1. Source file saved in `Assets/RoadRage/ArtSource/Blender`.
2. Object names are readable.
3. Transforms are applied.
4. Scale is believable next to the player car.
5. Origin is useful.
6. Normals look correct.
7. Material count is low.
8. Invisible/excess geometry is removed.
9. Collision shape is simple.
10. Export is FBX or GLB.
11. Unity prefab is tested in `MVP_Run`.

## Vibe Coding Rules

Ask the AI for vertical slices, not giant systems.

Good prompts:

- "Create the minimal lobby UI that can host and join by code using the existing architecture."
- "Use MaxPlayers=4 and Steamworks Networking Sockets through a private Steam lobby. Treat the Lobby ID as a UI-wrapper fallback around the native Steam invite."
- "Add a placeholder passenger action that sends intent to the host and increases one AI car rage."
- "Make a greybox AI car follow a route spline and expose its rage state."
- "Inspect this Unity scene for missing NetworkObject or ownership mistakes."

Risky prompts:

- "Build the whole multiplayer system."
- "Make realistic GTA-like traffic."
- "Generate all characters and environments."
- "Add progression, economy, bosses, weapons, and towns."

## Do Not Build Yet

Leave these alone until the online greybox loop is fun:

- Public matchmaking.
- Dedicated servers.
- Host migration.
- Realistic car physics.
- Full boss design.
- Large city maps.
- Destructible cities.
- Deep roguelite progression.
- Many weapons.
- Many driver archetypes.
- Large AI-generated asset batches.

## First Implementation Prompt For The Developer Agent

Use this only after the Unity project exists:

```text
Use the architecture spine in _bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md.

Create the initial Unity folder structure under Assets/RoadRage, add Bootstrap, MainMenuLobby, and MVP_Run scenes, install/verify the required packages from Packages/manifest.json, and create a minimal local-only greybox car controller with Cinemachine follow camera in MVP_Run. Do not implement online lobby yet.
```
