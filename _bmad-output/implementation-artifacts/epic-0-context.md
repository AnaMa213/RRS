# Epic 0 Context: Technical, Tools & Asset/Addon Readiness Gate

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

## Goal

Epic 0 establishes the production readiness gate for RoadRage_Simulator before gameplay development starts. It makes Epic 1 safe to begin by documenting and validating the Unity project, package stack, Steamworks networking path, Blender asset pipeline, AI/MCP workflow, add-on and asset adoption rules, smoke-test evidence, and final go/no-go status. Agents guide manual setup, create tracking documents, inspect generated files, validate evidence, and report blockers; they do not claim to automate external GUI installs, account setup, service approvals, or unverified tool access.

## Stories

- Story 0.1: Setup Readiness Checklist and Local Workspace Baseline
- Story 0.2: Unity Editor, Project Creation, and Package Pinning
- Story 0.3: Steamworks, Lobby, and Networking Sockets Readiness
- Story 0.4: Project Structure, Scenes, Namespaces, and Runtime State Skeleton
- Story 0.5: Codex, Claude, Unity MCP, and Blender MCP Configuration
- Story 0.6: Blender and 3D Asset Intake Pipeline
- Story 0.7: Unity Add-On, UI Library, and Asset Adoption Register
- Story 0.8: Epic 0 Smoke Tests and Go/No-Go Gate

## Requirements & Constraints

Epic 0 is mandatory and blocks Epic 1 until the final gate is marked `Pass` or `Accepted With Known Blockers`. Setup tracking must include `docs/setup/epic-0-readiness-checklist.md`, `docs/setup/tooling-validation-log.md`, and `docs/setup/addon-adoption-register.md`, with clear statuses for `Not Started`, `In Progress`, `Pass`, `Blocked`, and `Not Applicable`. The checklist must separate manual user actions from agent validation steps.

The MVP foundation must stay feasible for one solo developer. Prove the online co-op loop with greybox primitives before polished generated assets. Do not introduce parallel engines, render pipelines, input stacks, networking stacks, or duplicated runtime truth. Prefer compatible built-in, official, open-source, or otherwise approved Unity foundations when they reduce risk, but evaluate them before import.

Third-party add-ons, UI libraries, controller packages, starter assets, and generated/downloaded assets require an adoption review before use. The register must capture purpose, source, license, cost, Unity compatibility, maintenance status, dependency impact, multiplayer impact, source/editability, and fit with the architecture. Paid or closed-source candidates require explicit human approval and a documented justification before adoption.

The Unity project must be named `RRS` and use the approved Universal 3D/URP stack. The agent must be able to validate `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/`, and `Assets/`; package mismatches must be recorded before Epic 1 begins. The initial target is Windows PC development builds using the Steamworks test AppID `480`/Spacewar until public-release readiness is confirmed.

Online readiness must validate private host-created Steam lobbies, `MaxPlayers = 4`, native Steam invite first, Lobby ID as the fallback join-code wrapper, Steamworks Networking Sockets through Steam Datagram Relay, and no host router port forwarding. Public matchmaking, lobby browsing, dedicated servers, host migration, persistent accounts, cloud saves, analytics, anti-cheat, and secure backend economy are outside the MVP readiness gate.

Secrets must not be stored in prompts, scripts, scenes, ScriptableObjects, screenshots, logs, or committed files. Final readiness evidence must cover local Multiplayer Play Mode host/client smoke testing, a remote two-player Steamworks Networking Sockets smoke test, four-player session cap validation, host-quit handling, and visible Lobby/UI error requirements for join, networking, disconnect, and service failures.

## Technical Decisions

The locked stack is Unity `6000.6.0f1` on the Unity 6 Update track, Unity-supported C# 9.0, Universal 3D/URP, Netcode for GameObjects `2.13.2`, a Steamworks Netcode transport (`com.community.netcode.transport.facepunch` or `.steamnetworkingsockets`) pinned by commit or tag, Unity Transport `6.6.0`, Universal Render Pipeline `17.6.0`, Multiplayer Play Mode `3.0.0`, Input System `1.20.0`, Cinemachine `6.6.0`, Blender `5.2 LTS`, and FBX or GLB for 3D interchange.

The architecture is Feature-Sliced Host-Authoritative Unity. `Assets/RoadRage` is organized around `App`, `Shared`, feature slices, art source/export folders, materials, prefabs, ScriptableObjects, and tests. Namespaces and assembly boundaries are `RoadRage.App`, `RoadRage.Shared`, and `RoadRage.Features.<Feature>`. Seed scenes are `Bootstrap`, `MainMenuLobby`, `MVP_Run`, `Dev_VehicleSandbox`, `Dev_OnFootSandbox`, `Dev_RageSandbox`, and `Dev_LobbySmokeTest`.

The host owns shared gameplay state. Clients send typed player intent; the host validates actor, run phase, mode or seat, cooldown, target, range, and payload version before mutating state. Gameplay-authoritative NetworkObjects are host-owned, and gameplay NetworkVariables are server-write by default. Canonical runtime state is split into `NetworkedRunState`, `NetworkedPlayerState`, `NetworkedAIVehicleState`, `NetworkedRageState`, `NetworkedCrewEconomyState`, and `NetworkedBossState`.

Static authored gameplay data uses ScriptableObject definitions with stable globally unique lowercase ids. Runtime session values live in host-owned NetworkBehaviours and NetworkVariables, not ScriptableObject assets. Future feature slices to preserve are Lobby/Network, Run, Players, Vehicles, Rage, PassengerActions, OnFoot, SandboxStops, Economy, Boss, and UI.

Every AI-generated or downloaded 3D asset must pass through Blender cleanup before Unity prefab use. The intake path saves source files, applies transforms, checks scale and normals, reduces material and geometry issues, exports FBX or GLB, tests scene scale in Unity, then creates prefabs. Prefab identity, NetworkObject registration, gameplay components, colliders, and definition ids remain stable when art is replaced.

Unity MCP should prefer Unity Official MCP if available; otherwise use CoplayDev Unity MCP pinned to a release tag. Blender MCP should prefer Blender Lab MCP if stable; otherwise use the ahujasid Blender MCP fallback. MCP tools are controlled assistants only: all scene, prefab, package, script, and asset changes must be reviewed in Unity or Blender and committed in small steps. MCP tools must not silently add paid services, change package versions, convert to dedicated servers, store secrets, or bypass asset intake.

## UX & Interaction Patterns

Epic 0 UX is primarily setup and validation UX. Checklists, logs, and adoption registers must be easy to scan for status, evidence, owner, blocker, manual action, and agent validation. The adoption register must record initial choices for menu/UI foundation and on-foot movement/controller foundation before Epic 1, even when the decision is to stay with built-in Unity packages.

Lobby requirements captured during this gate must support Epic 2 UI work: create a private room, expose native Steam invite, show a Lobby ID fallback, join by Lobby ID, and display visible errors for join failure, Networking Sockets failure, disconnect, service failure, and host quit. Input and UI scripts remain presentation and intent layers; they must not mutate shared gameplay state directly.

## Cross-Story Dependencies

Story 0.1 creates the setup documents that the remaining Epic 0 stories update. Story 0.2 depends on the checklist and manual Unity project creation before package validation. Story 0.3 depends on the Unity project and Steamworks transport before lobby, AppID, invite, Lobby ID, Networking Sockets, and network error evidence can be validated.

Story 0.4 depends on the project and package baseline, then establishes scenes, folders, namespaces, assembly boundaries, and runtime state skeletons for later gameplay epics. Story 0.5 depends on tooling decisions and records Codex/Claude/MCP configuration plus harmless Unity and Blender MCP smoke tests. Story 0.6 depends on Blender installation and defines the asset intake process used by later art work.

Story 0.7 must record initial menu/UI and movement/controller adoption decisions before Epic 1 starts. Story 0.8 depends on all prior setup stories being complete or explicitly blocked, then records the final `Pass`, `Blocked`, or `Accepted With Known Blockers` readiness decision that controls Epic 1 start.
