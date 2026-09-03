---
stepsCompleted:
  - step-01-validate-prerequisites
  - step-02-design-epics
  - step-03-create-stories
  - step-04-final-validation
inputDocuments:
  - ../specs/spec-road-rage-simulator/SPEC.md
  - ../specs/spec-road-rage-simulator/gameplay-model.md
  - ../specs/spec-road-rage-simulator/mvp-scope.md
  - ../specs/spec-road-rage-simulator/module-composition.md
  - planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md
  - planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/beginner-architecture-guide.md
  - planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/mcp-tooling-setup.md
---

# RoadRage_Simulator - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for RoadRage_Simulator, decomposing the requirements from the SPEC, architecture spine, beginner guide, and MCP tooling setup into implementable stories.

## Requirements Inventory

### Functional Requirements

FR1: Players can complete an online co-op MVP run loop that includes driving, provoking an AI vehicle, escalating rage, resolving a crisis, earning money, and applying an upgrade.

FR2: The game supports online co-op for up to four players in a private room.

FR3: A host can create a private room with lobby-first flow, a join code, and an invite-link wrapper around that code.

FR4: Joining players can enter a room through the shared code and connect through Steamworks Networking Sockets (Steam Datagram Relay) rather than direct-connect-only networking.

FR5: Players can use one shared player car on one route in the MVP.

FR6: The MVP includes three AI vehicles on the same route.

FR7: Each AI vehicle tracks and expresses its own rage state independently from other AI vehicles.

FR8: AI vehicles can remain calm, become irritated, flee, block, ram, or trigger a confrontation based on their own rage.

FR9: Passengers have three MVP actions that actively create or amplify chaos.

FR10: Each passenger action visibly changes at least one vehicle rage state, incident state, resource opportunity, or crew-help effect.

FR11: Rage escalation can trigger one Rage Road event.

FR12: The Rage Road event can be triggered, moved into confrontation, resolved, and converted into a money reward.

FR13: The MVP includes one simple on-foot transition.

FR14: On-foot play supports compact sandbox stop interaction or Rage Road confrontation and returns value to the driving loop.

FR15: The MVP includes one compact sandbox stop with happenings, money opportunities, purchases, or incidents.

FR16: Players earn meaningful money primarily through road-rage confrontation.

FR17: Absurd side actions can exist as low-value comedy, interaction, and escalation triggers.

FR18: The MVP includes one money reward and one upgrade.

FR19: A road-rage victory grants enough currency to buy one upgrade.

FR20: A purchased upgrade affects the next driving loop.

FR21: The MVP includes one simple boss endpoint sufficient to validate boss-kill victory.

FR22: If every connected player reaches the dead lifecycle state, the run restarts from the beginning.

FR23: If the boss endpoint dies, the game declares victory.

FR24: Gameplay functionality is organized as independently testable modules that can run in small development slices.

FR25: Independently tested modules compose into one integrated `MVP_Run` without duplicating shared runtime truth.

FR26: The player can see lobby, run state, money, rage, actions, failure, and victory through UI that reads shared state and sends player intent.

FR27: Each gameplay epic after the technical readiness gate must leave the game in a launchable and testable state with at least one new playable in-game capability.

FR28: The project can use existing Unity assets, starter controllers, UI libraries, menu frameworks, and gameplay add-ons when they are compatible, maintained, legally usable, and do not violate the architecture contracts.

### Non-Functional Requirements

NFR1: The MVP must remain feasible for one solo developer using vibe coding and AI assistance.

NFR2: The first playable loop must be proven with greybox primitives before polished AI-generated 3D assets are introduced.

NFR3: The project must use one shared Unity engine/render/input/network stack; modules must not create separate engines or incompatible technical foundations.

NFR4: Networked gameplay state must be host-authoritative.

NFR5: Clients submit player intent; the host validates and mutates shared state.

NFR6: Gameplay-authoritative NetworkObjects are host-owned and gameplay NetworkVariables are server-write by default.

NFR7: The MVP must not require user router port forwarding for the host path.

NFR8: The MVP targets Windows PC development builds first.

NFR9: Steamworks services used for the MVP must be configured using the Steamworks test AppID (480/Spacewar) or another non-production Steamworks configuration until public-release readiness is confirmed.

NFR10: Package versions must be pinned and verified in `Packages/manifest.json` and `Packages/packages-lock.json` once the Unity project exists.

NFR11: The project must surface visible Lobby/UI errors for join, relay, disconnect, and service failures.

NFR12: The game must handle host quit or lost session by returning clients to MainMenuLobby with an error; host migration is deferred.

NFR13: Static authored gameplay data must use ScriptableObject definitions with stable globally unique ids.

NFR14: Runtime session values must live in host-owned NetworkBehaviours and NetworkVariables, not inside ScriptableObject assets.

NFR15: Input handlers and UI scripts must not mutate shared gameplay state directly.

NFR16: Each player camera is local-only and must not be synchronized over the network.

NFR17: Every AI-generated or downloaded 3D asset must pass through Blender cleanup before Unity prefab use.

NFR18: Gameplay prefab identity, NetworkObject registration, gameplay components, colliders, and definition ids must remain stable when art is replaced.

NFR19: API keys, tokens, Unity service credentials, and secrets must not be stored in prompts, scripts, scenes, ScriptableObjects, or committed files.

NFR20: Tone and content-rating boundaries remain an open requirement before final passenger action names, UI copy, animations, and generated assets.

NFR21: Exact health values, revive windows, damage sources, and vehicle destruction consequences remain open until the first combat/Rage Road greybox exists.

NFR22: Do not reinvent solved Unity foundations when a compatible and maintained built-in package, official sample, open-source library, or purchased asset can reduce solo-dev risk.

NFR23: Any third-party add-on or asset must pass a validation gate before adoption: Unity version compatibility, license, maintenance status, dependency impact, multiplayer compatibility, source availability or editability, and fit with the architecture spine.

NFR24: A purchased or imported asset must not become an unreviewed black box for core gameplay state, network authority, player lifecycle, economy, or run outcome.

### Additional Requirements

- Epic 0 is mandatory and gates all gameplay development. Epic 1 cannot start until Epic 0 installation, account setup, package setup, MCP setup, and smoke tests are complete.
- Epic 0 must include a to-do list and tutorial-style checklist for every required installation and configuration step.
- Epic 0 must include an add-on/library/asset audit checklist so the project intentionally reuses proven Unity foundations instead of reinventing menus, first-person movement, UI, controller scaffolding, or placeholder assets unnecessarily.
- Epic 0 must define the adoption rule for third-party assets: evaluate first, import second, wrap/adapt third, and only customize after the asset works in a small test scene.
- Install Unity Hub and Unity `6000.6.0f1` on the Unity 6 Update release track.
- Create a Unity Universal 3D/URP project named `RRS` (RoadRageSimulator).
- Install and verify these Unity packages: Netcode for GameObjects `2.13.2`, a Steamworks transport (`com.community.netcode.transport.facepunch` or `.steamnetworkingsockets`, commit/tag pinned at installation), Unity Transport `6.6.0`, Universal Render Pipeline `17.6.0`, Multiplayer Play Mode `3.0.0`, Input System `1.20.0`, and Cinemachine `6.6.0`.
- Configure the project with the Steamworks test AppID (`480`/Spacewar) for development.
- Configure Steamworks for private lobby creation (`ISteamMatchmaking`), Networking Sockets (Steam Datagram Relay), and Steam login.
- Verify that the MVP uses a private Steam lobby with Steamworks Networking Sockets, `MaxPlayers = 4`, host-created private sessions, and a Lobby-ID/invite flow.
- Confirm that invites use the native Steam overlay first, with a Lobby ID as a UI-wrapper fallback, unless platform-native deep links are later verified.
- Configure `Bootstrap`, `MainMenuLobby`, and `MVP_Run` scene seed.
- Configure the first Unity project folder structure under `Assets/RoadRage`.
- Configure `RoadRage.App`, `RoadRage.Shared`, and `RoadRage.Features.<Feature>` namespaces and assembly boundaries.
- Establish the canonical runtime state shape: `NetworkedRunState`, `NetworkedPlayerState`, `NetworkedAIVehicleState`, `NetworkedRageState`, `NetworkedCrewEconomyState`, and `NetworkedBossState`.
- Validate a local Multiplayer Play Mode host/client smoke test.
- Validate a remote two-player Steamworks Networking Sockets smoke test.
- Validate the four-player session cap.
- Validate host-quit handling and visible Lobby/UI error handling.
- Install Blender 5.2 LTS.
- Configure the 3D asset intake workflow: Blender source, exported FBX/GLB, Unity import, prefab creation, and scale testing.
- Install and configure Unity MCP, preferably Unity Official MCP Server if available; otherwise use CoplayDev MCP for Unity pinned to a release tag.
- Install and configure Blender MCP, preferably Blender Lab MCP Server if stable; otherwise use the ahujasid Blender MCP fallback.
- Include both Codex and Claude Code/Claude Desktop configuration paths where the chosen MCP supports them.
- Verify a harmless Unity MCP smoke test, such as creating and inspecting a temporary GameObject.
- Verify a harmless Blender MCP smoke test, such as creating a simple cube prop and exporting a test `.glb`.
- Use MCPs as controlled assistants only; every scene, prefab, package, script, or asset change must be reviewed in Unity/Blender and committed in small steps.
- Unity MCP must not silently add paid services, change package versions, convert the project to dedicated servers, or store secrets.
- Blender MCP and asset-generation tools must not bypass the asset intake checklist.
- The architecture spine governs engine, camera, input, networking, module boundaries, runtime state, package pins, and asset pipeline decisions.
- Module slices to preserve in epic/story design: Vehicle, OnFoot, PassengerActions, Rage, Economy, Lobby/Network, Run, Boss, SandboxStops, and UI.
- Development slices to preserve where useful: `Dev_VehicleSandbox`, `Dev_OnFootSandbox`, `Dev_RageSandbox`, `Dev_LobbySmokeTest`, and `MVP_Run`.
- Starter implementation should begin with Milestone 0: empty project health check, package installation verification, scene seed, and Multiplayer Play Mode local smoke test.
- Epic planning should favor playable checkpoints: after each epic, the user should be able to press Play or launch a development build and test visible progress.

### UX Design Requirements

No dedicated UX design contract exists yet. UX requirements currently come from the SPEC and architecture:

UX-DR1: Lobby UI must let a player create a private room, view a join code, and join a room by code.

UX-DR2: Invite-link UI must be treated as a wrapper around the join code unless platform-native deep links are later verified.

UX-DR3: Lobby/UI must display visible errors for join failure, Networking Sockets failure, disconnect, service failure, and host quit.

UX-DR4: Gameplay UI must show run state, money, rage, available passenger actions, failure, and victory.

UX-DR5: Passenger action UI must make three actions available and show a visible result in rage, incident state, resource opportunity, or crew-help feedback.

UX-DR6: Camera and input must be local-only presentation concerns and must not mutate shared gameplay state directly.

UX-DR7: Menu, lobby, character creation, HUD, and in-game UI may use existing Unity UI foundations or third-party UI assets if they pass the Epic 0 adoption checklist.

## FR Coverage Map

FR1: Epic 7 - complete MVP run loop integration from driving through victory/failure.

FR2: Epic 2 - private online lobby and player session flow.

FR3: Epic 1 and Epic 2 - menu/lobby shell first, real host-created private room second.

FR4: Epic 2 - code-based room join through Steamworks Networking Sockets.

FR5: Epic 3 - shared player car and route driving module.

FR6: Epic 5 - three AI vehicles on the route.

FR7: Epic 4 and Epic 5 - rage module first, traffic integration second.

FR8: Epic 5 - AI vehicle rage-driven behavior states.

FR9: Epic 4 - three MVP passenger actions.

FR10: Epic 4 - visible passenger action effects on rage, incidents, resources, or crew help.

FR11: Epic 5 - rage escalation triggers one Rage Road event.

FR12: Epic 6 - Rage Road confrontation resolves into money reward.

FR13: Epic 1 and Epic 6 - basic movement/empty map first, on-foot transition for confrontation later.

FR14: Epic 6 - on-foot play returns value to the driving loop.

FR15: Epic 6 - one compact sandbox stop.

FR16: Epic 6 - road-rage confrontation pays meaningful money.

FR17: Epic 4 and Epic 6 - absurd actions as low-value triggers and sandbox toys.

FR18: Epic 6 - one reward and one upgrade.

FR19: Epic 6 - road-rage victory grants enough currency for the upgrade.

FR20: Epic 6 and Epic 7 - upgrade affects the next loop and is verified in integrated run.

FR21: Epic 7 - simple boss endpoint.

FR22: Epic 3 and Epic 7 - player lifecycle/team wipe contract first, final restart integration later.

FR23: Epic 7 - boss death declares victory.

FR24: Epic 0 through Epic 7 - each module is independently testable.

FR25: Epic 3 through Epic 7 - modules compose into `MVP_Run` through shared runtime truth.

FR26: Epic 1, Epic 2, Epic 4, Epic 6, and Epic 7 - UI grows from menu/lobby into HUD, rage/action, economy, failure, and victory feedback.

FR27: Epic 1 through Epic 7 - every gameplay epic leaves the game launchable and testable.

FR28: Epic 0 - add-on and asset reuse evaluation before implementation.

## Epic List

### Epic 0: Technical, Tools & Asset/Addon Readiness Gate

The developer can start production safely because Unity, Blender, Unity services, packages, MCPs, Codex/Claude workflow, add-on evaluation, and smoke tests are configured and documented. Epic 1 cannot start until this gate passes.

**FRs covered:** FR24, FR28

### Epic 1: Playable Game Shell, Main Menu & Empty World Entry

The player can launch the game, see a simple main menu, press Play, pass through a first lobby/party setup shell, create or select a rough character, load an empty map, and move around with placeholder assets.

**FRs covered:** FR3, FR13, FR24, FR26, FR27

### Epic 2: Private Online Lobby, Player Spawn & In-Game UI Foundation

Players can create a private online room, share/join via Steam invite or Lobby ID through Steamworks Networking Sockets, enforce the four-player cap, spawn into the empty game world, and see the first usable HUD with health, sprint, network status, and basic player feedback.

**FRs covered:** FR2, FR3, FR4, FR22, FR24, FR25, FR26, FR27

### Epic 3: Vehicle Sandbox & Shared Driving Module

Players can enter or start inside the shared car, drive around one simple route, use the local camera/input setup, collide with the environment, and return to a launchable playable driving state.

**FRs covered:** FR1, FR5, FR13, FR24, FR25, FR27

### Epic 4: Passenger Chaos Actions & Rage Module

Passengers can use three MVP actions that target vehicles or situations, send validated host-side intent, and produce visible effects on independent rage, incidents, low-value resources, or crew-help feedback.

**FRs covered:** FR7, FR9, FR10, FR17, FR24, FR25, FR26, FR27

### Epic 5: AI Traffic & Rage Road Trigger

The route contains three AI vehicles with independent rage states and simple rage-driven behaviors, and escalation can trigger the first Rage Road event.

**FRs covered:** FR6, FR7, FR8, FR11, FR24, FR25, FR27

### Epic 6: On-Foot Confrontation, Sandbox Stop & Economy Loop

Players can leave the car for a compact confrontation or sandbox stop, resolve one Rage Road event, earn a shared money reward, buy one upgrade, and return that value to the next driving loop.

**FRs covered:** FR12, FR13, FR14, FR15, FR16, FR17, FR18, FR19, FR20, FR24, FR25, FR26, FR27

### Epic 7: Boss Endpoint, Victory/Failure & MVP Integration Pass

The full MVP run is assembled end to end: lobby, spawn, movement, driving, passenger chaos, rage, Rage Road, money, upgrade, simple boss endpoint, team-wipe restart, and boss-kill victory.

**FRs covered:** FR1, FR20, FR21, FR22, FR23, FR24, FR25, FR26, FR27

## Epic 0: Technical, Tools & Asset/Addon Readiness Gate

The solo developer can start production safely because Unity, Blender, Unity services, packages, MCPs, Codex/Claude workflow, add-on evaluation, and smoke tests are configured and documented. Epic 1 cannot start until this gate passes.

**Requirements covered:** FR24, FR28, NFR1, NFR2, NFR3, NFR7, NFR8, NFR9, NFR10, NFR17, NFR18, NFR19, NFR22, NFR23, NFR24, UX-DR7

Epic 0 implementation rule: BMAD, Codex, and Claude do not pretend to automate external GUI installs, account setup, or service approvals. They guide the manual setup, create the tracking documents, inspect generated project files, validate smoke-test evidence, and issue the Epic 1 go/no-go.

### Story 0.1: Setup Readiness Checklist and Local Workspace Baseline

**Implements:** FR24, FR28, NFR1, NFR22, NFR23, UX-DR7

As a solo developer,
I want a written setup checklist and a clean local workspace baseline,
So that every technical prerequisite is tracked before gameplay development begins.

**Acceptance Criteria:**

**Given** the repository exists locally and Epic 0 has started
**When** the setup tracking artifacts are created
**Then** `docs/setup/epic-0-readiness-checklist.md`, `docs/setup/tooling-validation-log.md`, and `docs/setup/addon-adoption-register.md` exist with status columns for Not Started, In Progress, Pass, Blocked, and Not Applicable
**And** the checklist separates manual user actions from agent validation steps
**And** the checklist states that Epic 1 is blocked until the Epic 0 gate is marked Pass or explicitly accepted with documented blockers

### Story 0.2: Unity Editor, Project Creation, and Package Pinning

**Implements:** FR24, NFR3, NFR8, NFR10, NFR22

As a solo developer,
I want Unity installed with the approved project template and pinned packages,
So that all later gameplay modules use one compatible engine, render, input, camera, and networking foundation.

**Acceptance Criteria:**

**Given** the setup checklist exists
**When** the user manually installs Unity Hub, installs Unity `6000.6.0f1`, and creates the Universal 3D/URP project `RRS`
**Then** the agent validates the presence of `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/`, and `Assets/`
**And** the agent verifies package entries for Netcode for GameObjects `2.13.2`, a Steamworks transport (`com.community.netcode.transport.facepunch` or `.steamnetworkingsockets`), Unity Transport `6.6.0`, Universal Render Pipeline `17.6.0`, Multiplayer Play Mode `3.0.0`, Input System `1.20.0`, and Cinemachine `6.6.0`
**And** any version mismatch is recorded in `docs/setup/tooling-validation-log.md` before Epic 1 begins

### Story 0.3: Steamworks, Lobby, and Networking Sockets Readiness

**Implements:** FR2, FR3, FR4, NFR7, NFR9, NFR11, NFR12, NFR19, UX-DR1, UX-DR2, UX-DR3

As a solo developer,
I want Steamworks configured for private lobby and Networking Sockets networking,
So that the MVP can support invite-code online co-op without requiring host port forwarding, at no cost regardless of player count.

**Acceptance Criteria:**

**Given** the Unity project exists with the approved Steamworks transport
**When** the user manually configures the Steamworks test AppID (`480`/Spacewar), Steam login, private lobby creation (`ISteamMatchmaking`), and Networking Sockets
**Then** the validation log confirms with evidence such as configuration notes, screenshots, logs, or smoke-test output that `MaxPlayers = 4`, host-created private lobbies, Networking-Sockets-backed join flow, and Lobby-ID entry are configured
**And** invites are documented as using the native Steam overlay first, with a Lobby ID as a UI-wrapper fallback, unless native platform deep links are later verified
**And** lobby lifecycle rules are documented for create, join, leave, host close, expiration, abandoned rooms, cleanup, and return-to-menu behavior
**And** a failed join, Networking Sockets failure, service failure, disconnect, and host-quit case each have a visible UI error requirement logged for Epic 2 implementation
**And** a real Steamworks account and its one-time publishing fee are documented as required before public release, not before development

### Story 0.4: Project Structure, Scenes, Namespaces, and Runtime State Skeleton

**Implements:** FR24, FR25, NFR3, NFR13, NFR14

As a solo developer,
I want the first Unity project structure and architecture skeleton configured,
So that future modules can be built independently while sharing the same runtime truth.

**Acceptance Criteria:**

**Given** the Unity project is installed and packages are pinned
**When** the user creates the seed project structure and scenes
**Then** the project contains `Bootstrap`, `MainMenuLobby`, `MVP_Run`, `Dev_VehicleSandbox`, `Dev_OnFootSandbox`, `Dev_RageSandbox`, and `Dev_LobbySmokeTest` scene seeds
**And** the project contains the `Assets/RoadRage` folder structure with assembly boundaries for `RoadRage.App`, `RoadRage.Shared`, and `RoadRage.Features.<Feature>`
**And** the canonical runtime state types are tracked for implementation: `NetworkedRunState`, `NetworkedPlayerState`, `NetworkedAIVehicleState`, `NetworkedRageState`, `NetworkedCrewEconomyState`, and `NetworkedBossState`
**And** the validation log confirms no module creates a separate engine, networking stack, input stack, or duplicated shared runtime truth

### Story 0.5: Codex, Claude, Unity MCP, and Blender MCP Configuration

**Implements:** FR24, FR28, NFR19, NFR22, NFR23, NFR24

As a solo developer,
I want Codex, Claude, Unity MCP, and Blender MCP configured with safe usage rules,
So that AI assistance can accelerate development without silently changing core project decisions.

**Acceptance Criteria:**

**Given** the setup checklist and Unity/Blender tooling decisions exist
**When** the user manually configures Codex and, where desired, Claude Code or Claude Desktop with Unity MCP and Blender MCP
**Then** the setup docs list the selected MCP server for Unity and Blender, the install path or repository, the version or release tag when available, and the exact client configuration path used for Codex and Claude
**And** the setup docs list allowed and forbidden MCP actions separately for Codex, Claude Code, and Claude Desktop when each client is used
**And** Unity MCP is validated with a harmless smoke test such as creating and inspecting a temporary GameObject in a disposable scene
**And** Blender MCP is validated with a harmless smoke test such as creating a simple cube prop and exporting a test `.glb`
**And** the validation log states that MCP tools must not silently add paid services, change package versions, convert to dedicated servers, store secrets, or bypass asset intake

### Story 0.6: Blender and 3D Asset Intake Pipeline

**Implements:** FR24, FR28, NFR17, NFR18, NFR23

As a solo developer using AI-generated 3D assets,
I want a simple Blender-to-Unity intake pipeline,
So that rough characters, cars, buildings, and props can be cleaned before becoming gameplay prefabs.

**Acceptance Criteria:**

**Given** Blender `5.2 LTS` is installed manually by the user
**When** the user creates or imports a test asset through Blender and exports it to Unity as FBX or GLB
**Then** the asset intake checklist documents source file location, export format, scale check, collider plan, prefab destination, and replacement policy
**And** every AI-generated or downloaded 3D asset is required to pass through Blender cleanup before Unity prefab use
**And** prefab identity, NetworkObject registration, gameplay components, colliders, and definition ids are documented as stable when art is replaced

### Story 0.7: Unity Add-On, UI Library, and Asset Adoption Register

**Implements:** FR28, NFR22, NFR23, NFR24, UX-DR7

As a solo developer,
I want a lightweight adoption register for Unity add-ons, UI libraries, controller packages, and starter assets,
So that the project reuses proven foundations without adopting risky black boxes.

**Acceptance Criteria:**

**Given** the project may use existing Unity assets, starter controllers, UI libraries, menu frameworks, and gameplay add-ons
**When** a candidate add-on or asset is considered
**Then** `docs/setup/addon-adoption-register.md` records its purpose, source, license, cost, Unity version compatibility, maintenance status, dependency impact, multiplayer impact, source availability or editability, and fit with the architecture spine
**And** before Epic 1 begins, the register records the initial decision for the menu/UI foundation and on-foot movement/controller foundation, even if the decision is to stay with built-in Unity packages
**And** the adoption workflow is recorded as evaluate first, import second, wrap/adapt third, and customize only after the asset works in a small test scene
**And** core gameplay state, network authority, player lifecycle, economy, and run outcome cannot be delegated to an unreviewed black box
**And** by default, a candidate must be free, royalty-free, and open-source (or a built-in Unity/Steamworks/official package); a paid or closed-source candidate requires explicit human approval with a documented cost/license justification before `Adopt`

### Story 0.8: Epic 0 Smoke Tests and Go/No-Go Gate

**Implements:** FR2, FR3, FR4, FR24, FR27, FR28, NFR7, NFR9, NFR10, NFR11, NFR12

As a solo developer,
I want a final readiness gate before Epic 1 begins,
So that development starts only after the tools, services, packages, and collaboration workflow are proven enough for a beginner-led project.

**Acceptance Criteria:**

**Given** all Epic 0 setup stories are complete or explicitly marked with documented blockers
**When** the user runs local and remote readiness checks
**Then** the validation log includes a local Multiplayer Play Mode host/client smoke test, a remote two-player Steamworks Networking Sockets smoke test, a four-player session cap check, host-quit handling evidence, and visible Lobby/UI error evidence or implementation notes
**And** the readiness checklist records manual evidence provided by the user and agent validations performed against project files
**And** the final Epic 0 status is marked Pass, Blocked, or Accepted With Known Blockers
**And** Epic 1 remains blocked unless the final status is Pass or Accepted With Known Blockers

## Epic 1: Playable Game Shell, Main Menu & Empty World Entry

The player can launch the game, see a simple main menu, press Play, pass through a first lobby/party setup shell, create or select a rough character, load an empty map, and move around with placeholder assets.

**Requirements covered:** FR3, FR13, FR24, FR26, FR27, NFR1, NFR2, NFR3, NFR15, NFR16, NFR22, NFR23, UX-DR1, UX-DR3, UX-DR4, UX-DR6, UX-DR7

### Story 1.1: Bootstrap and Main Menu Launch

**Implements:** FR3, FR26, FR27, UX-DR1, UX-DR3

As a player,
I want the game to open on a simple main menu,
So that I can start the MVP flow from a stable launch point.

**Acceptance Criteria:**

**Given** Epic 0 has passed or has been accepted with documented blockers
**When** the game starts from the `Bootstrap` scene
**Then** the player is routed to `MainMenuLobby`
**And** the main menu shows a Play command, a Quit command for builds, and a visible placeholder for service or network errors
**And** pressing Play moves the player into the next setup screen without requiring online services yet

### Story 1.2: Local Lobby Shell and Match Settings Draft

**Implements:** FR3, FR26, FR27, UX-DR1, UX-DR7

As a host player,
I want a first lobby shell with match settings,
So that the future online lobby already has a clear place for room creation and game options.

**Acceptance Criteria:**

**Given** the main menu is available
**When** the player presses Play
**Then** the game shows a local lobby setup shell with Create Lobby, Join By Code placeholder, Start Game, and Back controls
**And** the lobby shell includes draft match settings for difficulty and future expandable parameters
**And** settings are stored in a local data object that can later be synchronized by the networking epic
**And** unavailable online actions show clear placeholder feedback instead of failing silently

### Story 1.3: Rough Character Creation and Player Profile Selection

**Implements:** FR13, FR24, FR26, FR27, UX-DR7

As a player,
I want to create or select a rough character before entering the world,
So that the session flow already supports player identity and later co-op presentation.

**Acceptance Criteria:**

**Given** the local lobby shell is available
**When** the player starts the setup flow
**Then** the game shows a basic character creation or selection screen with a player name field and at least one placeholder character model
**And** the selected character uses a stable character id suitable for later network synchronization
**And** invalid or empty player names are handled with visible UI feedback
**And** the selected profile is passed into the empty world entry flow

### Story 1.4: First Rough Character, Car, and Building Asset Seeds

**Implements:** FR24, FR28, NFR2, NFR17, NFR18, NFR23

As a solo developer,
I want rough placeholder models for the player, car, and buildings,
So that gameplay can be tested with recognizable forms before polished AI-generated art exists.

**Acceptance Criteria:**

**Given** the Blender and Unity asset intake pipeline from Epic 0 exists
**When** the first rough models are added
**Then** the project contains placeholder prefabs for a character, a shared player car, and at least one simple building or city block prop
**And** each prefab has a documented source, scale check, collider plan, and replacement policy
**And** gameplay scripts do not depend on final art meshes or final animation rigs
**And** assets that come from downloads or AI generation are recorded in the adoption or intake register before use

### Story 1.5: Empty Map Entry and Local On-Foot Movement

**Implements:** FR13, FR24, FR27, NFR15, NFR16, UX-DR6

As a player,
I want to enter an empty map and move my character,
So that the first playable in-game state exists before vehicle, rage, or combat systems are added.

**Acceptance Criteria:**

**Given** the player has selected a profile
**When** the player starts the game from the local lobby shell
**Then** `MVP_Run` loads with a simple empty map, a ground plane or greybox street space, and the selected placeholder character
**And** the player can move, look around, sprint, and stop without mutating shared gameplay state directly
**And** camera and input are local-only presentation concerns
**And** the movement implementation may use a validated Unity starter controller or add-on if it passed the Epic 0 adoption register

### Story 1.6: Epic 1 Playable Checkpoint

**Implements:** FR3, FR13, FR24, FR26, FR27, UX-DR4

As a solo developer,
I want a launchable Epic 1 checkpoint,
So that I can test the first visible game flow end to end before adding real multiplayer.

**Acceptance Criteria:**

**Given** the main menu, lobby shell, character selection, placeholder assets, and empty map entry exist
**When** the game is launched in the editor or a Windows development build
**Then** the player can go from launch to menu to setup to empty world without console-blocking errors
**And** the screen shows basic run-state feedback or placeholders for lobby state, player state, and future HUD values
**And** the checkpoint notes list what is playable, what is stubbed, and what will be replaced by Epic 2

## Epic 2: Private Online Lobby, Player Spawn & In-Game UI Foundation

Players can create a private online room, share and join via Steam invite or Lobby ID through Steamworks Networking Sockets, enforce the four-player cap, spawn into the empty game world, and see the first usable HUD with health, sprint, network status, and basic player feedback.

**Requirements covered:** FR2, FR3, FR4, FR22, FR24, FR25, FR26, FR27, NFR4, NFR5, NFR6, NFR7, NFR9, NFR11, NFR12, NFR15, NFR16, NFR19, UX-DR1, UX-DR2, UX-DR3, UX-DR4, UX-DR6

### Story 2.1: Online Services Bootstrap and Status Feedback

**Implements:** FR3, FR4, FR26, FR27, NFR9, NFR11, NFR19, UX-DR3

As a player,
I want online services to initialize with clear status feedback,
So that lobby errors are visible instead of confusing or silent.

**Acceptance Criteria:**

**Given** the Epic 1 menu shell exists
**When** the game opens the online lobby flow
**Then** Steamworks SDK initialization (`SteamClient.Init`) and Steam login state are attempted through a single bootstrap service
**And** success, initialization failure, sign-in failure, and offline/unavailable states are shown in the lobby UI
**And** secrets, keys, tokens, and service credentials are not stored in scripts, scenes, ScriptableObjects, prompts, or committed files
**And** the online services code is isolated from gameplay state mutation

### Story 2.2: Host-Created Private Room with Join Code

**Implements:** FR2, FR3, FR4, NFR4, NFR7, NFR9, UX-DR1, UX-DR2

As a host player,
I want to create a private online room with a join code,
So that friends can join my session without public matchmaking.

**Acceptance Criteria:**

**Given** online services initialize successfully
**When** the host selects Create Lobby
**Then** a private Steam lobby (`ISteamMatchmaking`) is created with Steamworks Networking Sockets transport and `MaxPlayers = 4`
**And** the lobby UI displays a Steam invite option and/or a Lobby ID that can be copied or shared
**And** direct-connect-only networking and router port forwarding are not required for the MVP host path
**And** the room lifecycle supports host close, player leave, session expiration, abandoned-room cleanup, and return-to-menu behavior with visible feedback
**And** room creation errors are surfaced through visible UI feedback

### Story 2.3: Join By Code and Invite-Link Wrapper

**Implements:** FR2, FR3, FR4, NFR7, NFR11, UX-DR1, UX-DR2, UX-DR3

As a joining player,
I want to join a private room by code or invite-link wrapper,
So that I can enter a friend's co-op session from a simple shared token.

**Acceptance Criteria:**

**Given** a host-created private room exists
**When** a player enters the Lobby ID or accepts a Steam friend invite containing it
**Then** the player attempts to join through Steamworks Networking Sockets session flow
**And** join code input is trimmed, normalized, checked for empty values, and rejected with visible feedback when it contains invalid characters
**And** concurrent joins cannot exceed the four-player cap because slot reservation is validated by the service or host before the Networking Sockets connection proceeds
**And** invalid code, full room, expired session, Networking Sockets failure, and service failure each produce visible UI feedback
**And** the invite link is implemented as a wrapper around the join code unless native deep-link support is verified later
**And** successful join shows the player in the lobby roster

### Story 2.4: Lobby Roster, Ready State, and Settings Sync

**Implements:** FR2, FR3, FR24, FR25, FR26, FR27, UX-DR1

As a host and joining player,
I want the lobby to show players, readiness, and match settings,
So that the group can intentionally start the same session together.

**Acceptance Criteria:**

**Given** players can create and join a private room
**When** players enter the lobby
**Then** the lobby roster shows connected players up to four total players
**And** each player can toggle a ready state or equivalent start readiness marker
**And** the host can edit the MVP difficulty setting and start the run only when services are initialized, the roster is synchronized, settings are valid, and either all connected players are ready or an explicit solo-test exception is enabled
**And** joining clients receive the selected settings before loading the game world

### Story 2.5: Networked Player Spawn in Empty World

**Implements:** FR2, FR4, FR24, FR25, FR27, NFR4, NFR5, NFR6, NFR16, UX-DR6

As a co-op player,
I want all connected players to spawn into the empty map,
So that the online session becomes playable in the world.

**Acceptance Criteria:**

**Given** the host starts a valid private lobby session
**When** the `MVP_Run` scene loads
**Then** each connected player receives one networked player object with a host-authoritative `NetworkedPlayerState`
**And** local input and camera control affect only the local player's presentation and submitted intent
**And** gameplay-authoritative NetworkObjects are host-owned and server-write by default
**And** late, failed, or duplicate spawns are handled with logged and visible error feedback

### Story 2.6: In-Game HUD Foundation

**Implements:** FR22, FR26, FR27, NFR15, UX-DR4, UX-DR6

As a player,
I want a basic in-game HUD,
So that I can see my immediate player status and session state during the empty-world test.

**Acceptance Criteria:**

**Given** networked players can spawn into the empty world
**When** the run starts
**Then** the HUD shows health as hearts, sprint status or stamina placeholder, player count, network status, and a placeholder money value
**And** HUD scripts read shared state and send player intent through approved interfaces
**And** HUD scripts do not directly mutate shared gameplay NetworkVariables
**And** UI scales cleanly for solo and four-player test sessions

### Story 2.7: Player Lifecycle, Disconnect, and Host-Quit Handling

**Implements:** FR22, FR26, FR27, NFR11, NFR12, UX-DR3

As a co-op player,
I want the session to handle death, disconnects, and host quit clearly,
So that early online tests fail in understandable ways.

**Acceptance Criteria:**

**Given** players are spawned into the empty world
**When** a player is marked alive, downed, dead, disconnected, or reconnected in supported test cases
**Then** `NetworkedPlayerState` reflects the lifecycle state from host-owned runtime state
**And** if every connected player reaches dead state, the run restart condition is detected and logged for later full restart integration
**And** if the host quits or the session is lost, clients return to `MainMenuLobby` with a visible error
**And** host migration is explicitly deferred

### Story 2.8: Epic 2 Online Playable Checkpoint

**Implements:** FR2, FR3, FR4, FR22, FR24, FR25, FR26, FR27, NFR7, NFR11, NFR12

As a solo developer,
I want an online lobby and spawn checkpoint,
So that I can verify the hardest networking foundation before adding vehicle gameplay.

**Acceptance Criteria:**

**Given** the online lobby, join flow, roster, player spawn, HUD, and error handling exist
**When** the game is tested with Multiplayer Play Mode and a remote two-player Steamworks Networking Sockets test
**Then** players can create a room, share a code, join, start, spawn, move, and see HUD state
**And** the four-player cap is verified
**And** the checkpoint notes list known limitations, manual test steps, and blockers before Epic 3 starts

## Epic 3: Vehicle Sandbox & Shared Driving Module

Players can enter or start inside the shared car, drive around one simple route, use the local camera/input setup, collide with the environment, and return to a launchable playable driving state.

**Requirements covered:** FR1, FR5, FR13, FR22, FR24, FR25, FR27, NFR2, NFR3, NFR4, NFR5, NFR6, NFR15, NFR16, NFR18

### Story 3.1: Shared Player Car Prefab and Vehicle Module Boundary

**Implements:** FR5, FR24, FR25, FR27, NFR18

As a player,
I want a shared player car to exist in the game world,
So that the driving loop has a stable object to build on.

**Acceptance Criteria:**

**Given** the online empty world checkpoint exists
**When** the vehicle module is added
**Then** the project contains a shared player car prefab with NetworkObject identity, basic colliders, visible placeholder art, and a module-owned component boundary
**And** the car prefab can be loaded in `Dev_VehicleSandbox` and `MVP_Run`
**And** the vehicle module does not own lobby, economy, boss, or passenger action rules
**And** replacing the car art does not change the vehicle gameplay identity

### Story 3.2: Driver Control and Local Vehicle Camera

**Implements:** FR5, FR24, FR25, FR27, NFR4, NFR5, NFR16, UX-DR6

As the driver,
I want to control the shared car with a local camera,
So that the first driving experience is playable without syncing camera state.

**Acceptance Criteria:**

**Given** the shared player car exists
**When** an authorized player enters driver control
**Then** local input submits driving intent and the host validates movement-affecting state
**And** acceleration, braking, steering, and reverse work on a simple route or test plane
**And** the vehicle camera is local-only and is not synchronized over the network
**And** invalid or duplicate driver ownership attempts are rejected with visible feedback or logs

### Story 3.3: Seat Entry, Exit, and Passenger Presence

**Implements:** FR5, FR13, FR22, FR24, FR25, FR27

As a co-op player,
I want to enter and exit the shared car as driver or passenger,
So that on-foot and vehicle modules can connect without merging into one system.

**Acceptance Criteria:**

**Given** networked players and the shared car exist
**When** a player interacts with the car
**Then** the player can occupy an available seat or exit to a nearby safe position
**And** seat occupancy is represented in host-authoritative shared state
**And** passengers can ride without receiving driver control
**And** if the driver or a seated player dies, disconnects, or leaves the session, the host releases that seat and parks, reassigns, or disables driver control with visible feedback
**And** the entry and exit flow works in `Dev_VehicleSandbox` without requiring future passenger actions

### Story 3.4: Simple Route, Collision, and Vehicle Recovery

**Implements:** FR5, FR24, FR27, NFR2

As a player,
I want a simple route with basic collision and recovery,
So that driving can be tested as a repeatable gameplay slice.

**Acceptance Criteria:**

**Given** the shared car is controllable
**When** the route sandbox loads
**Then** the scene contains a simple road loop or route segment, boundaries, basic buildings or props, and a car reset or recovery point
**And** collisions with the route environment produce visible feedback without breaking the session
**And** the car can be recovered if it flips, exits the playable area, or becomes stuck
**And** the route can run without AI traffic or Rage Road systems

### Story 3.5: Vehicle Damage Hook and Team-Wipe Contract Stub

**Implements:** FR22, FR24, FR25, NFR4, NFR5, NFR21

As a co-op player,
I want vehicle danger to connect to player lifecycle in a minimal way,
So that failure rules can be integrated later without rewriting the driving module.

**Acceptance Criteria:**

**Given** the shared car can collide and recover
**When** test damage is applied through a debug trigger or collision threshold
**Then** affected player lifecycle state can be updated by the host through `NetworkedPlayerState`
**And** the all-dead condition can be detected from shared player state
**And** the actual full-run restart remains stubbed until Epic 7
**And** vehicle damage hooks do not mutate economy, boss, or rage state directly

### Story 3.6: Epic 3 Driving Playable Checkpoint

**Implements:** FR5, FR13, FR22, FR24, FR25, FR27

As a solo developer,
I want a playable driving checkpoint,
So that I can test walking, entering the car, driving, exiting, and recovering before adding chaos systems.

**Acceptance Criteria:**

**Given** the vehicle module, seats, route, controls, collision, and recovery exist
**When** the game is launched in editor and in a Windows development build
**Then** at least one player can move on foot, enter the car, drive the route, exit the car, and recover from a stuck state
**And** a local Multiplayer Play Mode host/client check confirms shared car state is visible to clients
**And** the checkpoint notes list which parts are greybox, which assets are placeholders, and what remains for Epic 4

## Epic 4: Passenger Chaos Actions & Rage Module

Passengers can use three MVP actions that target vehicles or situations, send validated host-side intent, and produce visible effects on independent rage, incidents, low-value resources, or crew-help feedback.

**Requirements covered:** FR7, FR9, FR10, FR17, FR24, FR25, FR26, FR27, NFR4, NFR5, NFR6, NFR13, NFR14, NFR15, NFR20, UX-DR4, UX-DR5

### Story 4.1: Rage State Module and Definitions

**Implements:** FR7, FR24, FR25, FR26, NFR13, NFR14, UX-DR4

As a player,
I want rage to exist as a visible gameplay state,
So that passenger actions and AI behavior can affect something understandable.

**Acceptance Criteria:**

**Given** the driving checkpoint exists
**When** the rage module is added
**Then** `NetworkedRageState` can track rage values and state labels for at least one target independently
**And** static rage thresholds and tuning data use ScriptableObject definitions with stable globally unique ids
**And** runtime rage values live in host-owned NetworkBehaviours and NetworkVariables, not ScriptableObject assets
**And** a dev UI or HUD element shows current rage state during testing

### Story 4.2: Passenger Action Framework and Host-Validated Intent

**Implements:** FR9, FR10, FR24, FR25, FR26, FR27, NFR4, NFR5, NFR15, UX-DR5

As a passenger,
I want to trigger chaos actions through a clear UI,
So that co-op players can affect the driving loop without becoming the driver.

**Acceptance Criteria:**

**Given** the rage state module exists
**When** the passenger action framework is added
**Then** passengers have three action slots available through in-game UI
**And** each action sends player intent to the host for validation before mutating shared state
**And** cooldown, invalid target, unavailable seat, and disconnected player cases are handled with visible feedback
**And** the framework can run in `Dev_RageSandbox` without requiring AI traffic from Epic 5

### Story 4.3: Passenger Action One Changes Rage

**Implements:** FR9, FR10, FR17, FR24, FR26, NFR20, UX-DR5

As a passenger,
I want a first chaos action that raises rage,
So that I can immediately see my action affect the world.

**Acceptance Criteria:**

**Given** a passenger is seated and a rage target exists
**When** the passenger triggers the first MVP action
**Then** the host validates the action and increases or changes the target's rage state
**And** the HUD or dev UI shows the rage change clearly
**And** the action has placeholder animation, sound, or visual feedback suitable for greybox testing
**And** the final action name and tone remain replaceable until content-rating boundaries are finalized

### Story 4.4: Passenger Action Two Creates an Incident or Resource Opportunity

**Implements:** FR9, FR10, FR17, FR24, FR26, NFR20, UX-DR5

As a passenger,
I want a second chaos action that can create an incident or low-value opportunity,
So that side actions feel playful without becoming the main money source.

**Acceptance Criteria:**

**Given** a passenger is seated and the action framework is available
**When** the passenger triggers the second MVP action
**Then** the host validates the action and creates a visible incident marker, minor resource opportunity, or equivalent low-value gameplay result
**And** the result can optionally affect rage without guaranteeing a major reward
**And** repeated use is limited by cooldown, availability, or test tuning
**And** the action can be tested without Rage Road confrontation from Epic 6

### Story 4.5: Passenger Action Three Provides Crew Help

**Implements:** FR9, FR10, FR24, FR26, UX-DR5

As a passenger,
I want a third chaos action that helps the crew,
So that passenger play includes more than pure escalation.

**Acceptance Criteria:**

**Given** the passenger action framework is available
**When** the passenger triggers the third MVP action
**Then** the host validates the action and applies a visible crew-help effect such as temporary support, risk reduction, distraction, or preparation for later rewards
**And** the effect is represented in shared runtime state or clear local feedback as appropriate
**And** the action does not directly purchase upgrades or declare run outcomes
**And** cooldown and unavailable-state feedback are visible to the passenger

### Story 4.6: Epic 4 Passenger Chaos Playable Checkpoint

**Implements:** FR7, FR9, FR10, FR17, FR24, FR25, FR26, FR27

As a solo developer,
I want a playable passenger chaos checkpoint,
So that I can test co-op roles and rage feedback before adding real AI traffic behavior.

**Acceptance Criteria:**

**Given** the three passenger actions and rage module exist
**When** the game is launched with at least two players in a local Multiplayer Play Mode test
**Then** one player can drive while another passenger triggers three actions and sees visible results
**And** rage, incident, resource, or crew-help feedback is visible in UI
**And** host validation prevents clients from directly mutating shared rage or action state
**And** the checkpoint notes list tuning placeholders and content-tone items that remain open

## Epic 5: AI Traffic & Rage Road Trigger

The route contains three AI vehicles with independent rage states and simple rage-driven behaviors, and escalation can trigger the first Rage Road event.

**Requirements covered:** FR6, FR7, FR8, FR11, FR24, FR25, FR27, NFR2, NFR4, NFR5, NFR6, NFR13, NFR14, NFR18

### Story 5.1: Three AI Vehicles on the Route

**Implements:** FR6, FR7, FR24, FR25, FR27, NFR4, NFR6, NFR18

As a player,
I want AI vehicles to exist on the route,
So that the road starts to feel like a reactive driving space.

**Acceptance Criteria:**

**Given** the driving route and rage module exist
**When** the AI traffic module is added
**Then** the route contains three AI vehicle prefabs with stable ids and visible placeholder art
**And** each AI vehicle has its own `NetworkedAIVehicleState` and independent rage state
**And** AI vehicle runtime state is host-owned and server-write by default
**And** the scene can be tested without starting a Rage Road confrontation

### Story 5.2: Basic AI Route Following and Recovery

**Implements:** FR6, FR24, FR25, FR27, NFR2, NFR4

As a player,
I want AI vehicles to move along the route,
So that driving around traffic is testable before complex behaviors are added.

**Acceptance Criteria:**

**Given** three AI vehicles exist on the route
**When** the route simulation starts
**Then** AI vehicles follow simple waypoints or lane markers at testable speeds
**And** AI vehicles can recover or reset if stuck, flipped, or outside the playable route
**And** movement is deterministic enough for host-authoritative networking tests
**And** AI traffic can be disabled or isolated in a development sandbox

### Story 5.3: Rage-Driven AI Behavior States

**Implements:** FR7, FR8, FR24, FR25, FR27, NFR4

As a player,
I want AI vehicles to react differently as rage changes,
So that passenger chaos produces visible road behavior.

**Acceptance Criteria:**

**Given** AI vehicles can move and each has independent rage state
**When** rage thresholds are reached
**Then** an AI vehicle can enter calm, irritated, flee, block, ram, or confrontation-triggering behavior states
**And** state changes are visible through movement, UI/debug labels, or feedback markers
**And** one AI vehicle changing state does not force all other AI vehicles into the same state
**And** behavior transitions are host-authoritative

### Story 5.4: Rage Road Event Trigger

**Implements:** FR11, FR24, FR25, FR27, NFR4

As a player,
I want rage escalation to trigger one Rage Road event,
So that the road chaos can move toward a confrontation loop.

**Acceptance Criteria:**

**Given** at least one AI vehicle can reach a confrontation-triggering rage state
**When** rage crosses the configured trigger condition
**Then** one Rage Road event is created with a clear event state, target AI vehicle, and visible player feedback
**And** simultaneous trigger attempts are resolved by a documented host-side arbitration rule such as first-trigger-wins, configured priority, or a single active-event queue
**And** the event can be marked pending, active, or resolved later without requiring Epic 6 resolution logic yet
**And** duplicate triggers are prevented for the same active event
**And** the event state is stored in host-owned runtime state

### Story 5.5: AI Traffic Networking and Client Presentation

**Implements:** FR6, FR7, FR8, FR11, FR24, FR25, FR27, NFR4, NFR5, NFR6

As a co-op player,
I want every player to see consistent AI traffic and rage reactions,
So that online play remains understandable.

**Acceptance Criteria:**

**Given** AI traffic and Rage Road trigger state exist
**When** clients join and observe the route
**Then** AI vehicle positions, behavior states, rage state labels, and active Rage Road event state are synchronized from host-owned state
**And** clients cannot directly force AI vehicle behavior changes
**And** late-joining clients receive the current relevant traffic and event state
**And** network traffic remains suitable for MVP tests with up to four players and three AI vehicles

### Story 5.6: Epic 5 AI Traffic Playable Checkpoint

**Implements:** FR6, FR7, FR8, FR11, FR24, FR25, FR27

As a solo developer,
I want a playable AI traffic and Rage Road trigger checkpoint,
So that I can test the escalation path before building confrontation resolution.

**Acceptance Criteria:**

**Given** AI traffic, rage behavior states, and the Rage Road trigger exist
**When** the game is tested in `MVP_Run` or `Dev_RageSandbox`
**Then** players can drive near three AI vehicles, trigger rage changes through passenger actions, and create one Rage Road event
**And** the event remains visible and stable until resolved or reset
**And** local and online smoke tests confirm host-authoritative state updates
**And** the checkpoint notes list tuning assumptions for Epic 6 confrontation design

## Epic 6: On-Foot Confrontation, Sandbox Stop & Economy Loop

Players can leave the car for a compact confrontation or sandbox stop, resolve one Rage Road event, earn a shared money reward, buy one upgrade, and return that value to the next driving loop.

**Requirements covered:** FR12, FR13, FR14, FR15, FR16, FR17, FR18, FR19, FR20, FR24, FR25, FR26, FR27, NFR1, NFR2, NFR4, NFR5, NFR6, NFR13, NFR14, NFR15, NFR20, NFR21, UX-DR4

### Story 6.1: On-Foot Transition for Confrontation and Sandbox Stops

**Implements:** FR13, FR14, FR24, FR25, FR27

As a player,
I want to leave the car for a compact on-foot interaction,
So that road events can briefly become a different playable module.

**Acceptance Criteria:**

**Given** the shared car and Rage Road event trigger exist
**When** a confrontation or sandbox stop begins
**Then** players can transition from car seats to on-foot spawn points in a compact interaction area
**And** players can return to the car after the interaction ends
**And** the on-foot module can run in `Dev_OnFootSandbox` without requiring the full MVP run
**And** the transition preserves player lifecycle and network ownership rules

### Story 6.2: Compact Rage Road Confrontation Resolution

**Implements:** FR12, FR14, FR24, FR25, FR27, NFR21

As a co-op player,
I want to resolve one Rage Road confrontation,
So that escalation has a meaningful playable payoff.

**Acceptance Criteria:**

**Given** a Rage Road event is active and players can transition on foot
**When** players complete the compact confrontation objective
**Then** the event is marked resolved by host-owned runtime state
**And** resolution uses a greybox objective suitable for a beginner MVP, such as survive, interact, push back, or de-escalate within a small area
**And** failure, timeout, or player death outcomes are visible in UI or logs
**And** the confrontation can be tested without boss endpoint logic

### Story 6.3: Shared Money Reward for Road-Rage Victory

**Implements:** FR12, FR16, FR18, FR19, FR24, FR25, FR26

As a co-op player,
I want a resolved road-rage confrontation to grant money,
So that the rage loop feeds the upgrade loop.

**Acceptance Criteria:**

**Given** a Rage Road confrontation can be resolved
**When** the players win the confrontation
**Then** `NetworkedCrewEconomyState` grants a shared money reward large enough to buy the first upgrade
**And** money is shown in the HUD to all connected players
**And** absurd side actions may grant low-value feedback but do not become the primary money source
**And** the host validates reward grants to prevent duplicate payouts

### Story 6.4: Compact Sandbox Stop with Happenings

**Implements:** FR15, FR17, FR24, FR25, FR26, NFR20

As a player,
I want one compact sandbox stop with small happenings,
So that the world has a playful non-driving interaction space.

**Acceptance Criteria:**

**Given** players can transition on foot
**When** a sandbox stop is entered
**Then** the area contains at least one happening, small money opportunity, purchase option, or incident interaction
**And** the interaction is compact enough to test in a single dev scene
**And** outcomes can affect money, minor resources, rage, or crew-help feedback without requiring boss logic
**And** low-value comedy interactions remain replaceable until tone boundaries are finalized

### Story 6.5: One Upgrade Purchase

**Implements:** FR18, FR19, FR20, FR24, FR25, FR26, NFR13, NFR14

As a co-op player,
I want to spend shared money on one upgrade,
So that winning a confrontation changes the next driving loop.

**Acceptance Criteria:**

**Given** the crew has enough money from a resolved confrontation
**When** a player opens the upgrade purchase UI and buys the MVP upgrade
**Then** the purchase is validated by the host and deducted from shared money
**And** simultaneous purchase attempts are handled as one atomic host transaction so money and upgrade state cannot duplicate or diverge
**And** the upgrade has a stable definition id stored in ScriptableObject data
**And** the purchased state is stored in host-owned runtime state
**And** insufficient money, duplicate purchase, and disconnected player cases produce visible feedback

### Story 6.6: Upgrade Effect on the Next Driving Loop

**Implements:** FR20, FR24, FR25, FR27

As a player,
I want the purchased upgrade to affect driving after the stop,
So that the economy loop has visible gameplay value.

**Acceptance Criteria:**

**Given** the MVP upgrade has been purchased
**When** players return to the car or restart the driving segment
**Then** the upgrade visibly affects the next driving loop through a small testable benefit such as durability, recovery, speed, handling, action cooldown, or crew support
**And** the effect is read from shared upgrade state rather than hardcoded into the vehicle module
**And** clients see consistent upgraded behavior or UI feedback
**And** the effect can be disabled for comparison in a development test

### Story 6.7: Economy, Confrontation, and Stop UI Feedback

**Implements:** FR12, FR15, FR16, FR18, FR19, FR20, FR26, FR27, UX-DR4

As a player,
I want clear UI during confrontations, stops, rewards, and upgrades,
So that the expanded loop remains understandable.

**Acceptance Criteria:**

**Given** confrontation, sandbox stop, money, and upgrade systems exist
**When** the player enters these states
**Then** the UI shows current run state, money, active confrontation or stop prompt, reward result, and upgrade purchase result
**And** UI reads shared state and sends intent through approved interfaces
**And** failed purchases, failed interactions, and unavailable actions are visibly explained
**And** UI remains usable for one to four connected players

### Story 6.8: Epic 6 Economy Loop Playable Checkpoint

**Implements:** FR12, FR13, FR14, FR15, FR16, FR17, FR18, FR19, FR20, FR24, FR25, FR26, FR27

As a solo developer,
I want a playable confrontation and economy checkpoint,
So that I can test the MVP reward loop before boss and final integration.

**Acceptance Criteria:**

**Given** on-foot transition, confrontation resolution, sandbox stop, money reward, upgrade purchase, and upgrade effect exist
**When** the game is tested from driving into a Rage Road event
**Then** players can trigger an event, leave the car, resolve a confrontation, receive money, buy one upgrade, return to driving, and see the upgrade affect gameplay
**And** all key state changes are host-authoritative and visible in UI
**And** the checkpoint notes identify what remains for final failure, victory, and full-run polish in Epic 7

## Epic 7: Boss Endpoint, Victory/Failure & MVP Integration Pass

The full MVP run is assembled end to end: lobby, spawn, movement, driving, passenger chaos, rage, Rage Road, money, upgrade, simple boss endpoint, team-wipe restart, and boss-kill victory.

**Requirements covered:** FR1, FR20, FR21, FR22, FR23, FR24, FR25, FR26, FR27, NFR1, NFR2, NFR4, NFR5, NFR6, NFR11, NFR12, NFR20, NFR21, UX-DR3, UX-DR4

### Story 7.1: Integrated MVP Run State Machine

**Implements:** FR1, FR20, FR21, FR22, FR23, FR24, FR25, FR26, FR27

As a player,
I want the full MVP run to flow through its major states,
So that the separate modules combine into one coherent playable game.

**Acceptance Criteria:**

**Given** the lobby, on-foot, vehicle, passenger action, rage, AI traffic, confrontation, economy, and upgrade modules exist
**When** a host starts the MVP run
**Then** `NetworkedRunState` can move through lobby start, spawn, driving, rage escalation, confrontation, reward, upgrade, boss endpoint, failure, restart, and victory states
**And** each module reads or updates shared runtime truth through approved boundaries
**And** no module duplicates ownership of run outcome, player lifecycle, economy, or boss state
**And** development scenes remain usable for independent module tests

### Story 7.2: Simple Boss Endpoint

**Implements:** FR21, FR23, FR24, FR25, FR27, NFR21

As a co-op player,
I want a simple boss endpoint at the end of the MVP route,
So that the run has a clear final objective.

**Acceptance Criteria:**

**Given** the integrated run can reach an endpoint state
**When** the boss endpoint spawns or activates
**Then** `NetworkedBossState` tracks boss alive/dead state, health or equivalent progress, and endpoint availability
**And** players can damage, complete, or otherwise defeat the boss through a simple greybox mechanic
**And** boss state is host-owned and synchronized to clients
**And** boss implementation is intentionally minimal and does not require final art, complex combat, or final encounter design

### Story 7.3: Boss-Kill Victory Flow

**Implements:** FR21, FR23, FR26, FR27, UX-DR4

As a co-op player,
I want the game to declare victory when the boss dies,
So that the MVP has a complete win condition.

**Acceptance Criteria:**

**Given** the boss endpoint is active
**When** the boss reaches dead state
**Then** the host sets the run outcome to victory
**And** all players see a victory screen or overlay
**And** if boss death and team wipe are detected in the same simulation tick, the committed boss-dead state takes precedence and the run resolves as victory
**And** victory state prevents duplicate reward, death, or restart outcomes from firing afterward
**And** the victory flow can return players to main menu or restart a new test run

### Story 7.4: Team-Wipe Restart From Beginning

**Implements:** FR22, FR24, FR25, FR26, FR27, NFR11, NFR12, UX-DR3

As a co-op player,
I want the run to restart if everyone dies,
So that failure is clear and the group can try again.

**Acceptance Criteria:**

**Given** player lifecycle state is tracked for all connected players
**When** every connected player reaches the dead lifecycle state before victory
**Then** the host sets the run outcome to failure and restarts the run from the beginning
**And** restart resets run-scoped state including player lifecycle, spawn positions, car position, rage state, event state, boss state, shared money, purchased MVP upgrade, temporary rewards, and temporary action effects
**And** restart preserves only out-of-run state required to continue the same session, such as room membership, join code validity, roster identity, and selected match settings
**And** clients see failure feedback before or during restart
**And** disconnects are handled separately from death so accidental host/session loss does not masquerade as valid team wipe

### Story 7.5: Complete MVP Loop Integration

**Implements:** FR1, FR20, FR21, FR22, FR23, FR24, FR25, FR26, FR27

As a player,
I want to play the MVP loop from lobby to boss victory,
So that RoadRage_Simulator has its first complete testable version.

**Acceptance Criteria:**

**Given** victory and failure flows exist
**When** players run through the full MVP flow
**Then** the game supports creating a room, joining by code, spawning, moving on foot, entering the shared car, driving, triggering passenger chaos, escalating AI rage, starting Rage Road, resolving confrontation, earning money, buying one upgrade, reaching the boss, and winning by killing the boss
**And** if all connected players die before victory, the run restarts from the beginning
**And** all major state transitions are visible through UI
**And** the loop remains feasible with placeholder assets and greybox mechanics

### Story 7.6: MVP Stabilization, Build, and Handoff Notes

**Implements:** FR1, FR2, FR22, FR23, FR24, FR25, FR26, FR27, NFR1, NFR11, NFR12

As a solo developer,
I want the final MVP to be stabilized and documented,
So that the next development cycle can improve content instead of rediscovering the foundation.

**Acceptance Criteria:**

**Given** the complete MVP loop can be played
**When** the MVP integration pass is performed
**Then** a Windows development build can be produced and smoke-tested
**And** local Multiplayer Play Mode and remote Steamworks Networking Sockets test notes cover one-player, two-player, and up-to-four-player scenarios where feasible, with not-run reasons recorded
**And** known bugs, tuning assumptions, asset placeholders, networking risks, and next-iteration candidates are documented
**And** the MVP is marked ready for retrospective or next sprint planning only after the documented smoke tests pass or accepted blockers are recorded
