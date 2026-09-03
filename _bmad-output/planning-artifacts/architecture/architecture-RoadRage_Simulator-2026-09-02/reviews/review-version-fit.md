# Version-Fit Review - ARCHITECTURE-SPINE.md

Review date: 2026-09-02

Target: `_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md`

Scope: current technology/version validity for Unity, Unity packages, Blender, Lobby, Relay, and Multiplayer Services. Sources below are official primary sources.

## Verdict

The stack is broadly plausible for a Unity online co-op MVP, but it is not release-lock ready. The Unity 6.6 choice is real and current, yet it is an Update release that should be treated as a moving track, not an LTS anchor. The main stale pin is `com.unity.services.multiplayer` at `2.1.1`; official package docs now resolve the `2.3` line to `2.3.1`. The architecture also leaves URP and Unity Transport under-specified even though both materially affect rendering and Relay-backed Netcode behavior.

## Official Source Checks

| Area | Current official check | Source |
| --- | --- | --- |
| Unity Editor | Unity 6.6 exists; the current concrete release checked is `6000.6.0f1`, released Aug 31, 2026. | https://unity.com/releases/editor/whats-new/6000.6.0f1 |
| Unity release support | Unity 6.3 is the latest LTS and is supported until December 2027; Update releases are supported until the next release is published. | https://unity.com/releases/unity-6/support |
| Unity 6.6 manual | Unity 6.6 documentation is live and marked as a supported documentation version. | https://docs.unity3d.com/6000.6/Documentation/Manual/UnityManual.html |
| C# in Unity 6.6 | Unity 6.6 uses Roslyn and C# 9.0, with documented unsupported/caveated C# 9 features. | https://docs.unity3d.com/6000.6/Documentation/Manual/csharp-compiler.html |
| Netcode for GameObjects | `com.unity.netcode.gameobjects` docs resolve to `2.13.2`; changelog lists `2.13.2` on 2026-08-16. | https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/changelog/CHANGELOG.html |
| Netcode transport | Netcode uses Unity Transport by default; UNet transport is deprecated/unsupported past Unity 2022.2. | https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/advanced-topics/transports.html |
| Multiplayer Services | `com.unity.services.multiplayer@2.3` resolves to `2.3.1`; changelog lists `2.3.1` on 2026-08-25 and earlier `2.2.x` fixes after `2.1.1`. | https://docs.unity3d.com/Packages/com.unity.services.multiplayer@2.3/changelog/CHANGELOG.html |
| Multiplayer Services session create | Current session docs show `SessionOptions.WithRelayNetwork()` and expose `session.Code` for join codes. | https://docs.unity.com/en-us/mps-sdk/create-session |
| Multiplayer Services session join | Current session docs list join code, session browsing, and reconnect flows. | https://docs.unity.com/en-us/mps-sdk/join-session |
| Multiplayer Play Mode | `com.unity.multiplayer.playmode` docs resolve to `3.0.0`; changelog says this version supports Unity 6.3 and beyond. | https://docs.unity3d.com/Packages/com.unity.multiplayer.playmode@3.0/changelog/CHANGELOG.html |
| Input System | `com.unity.inputsystem` docs resolve to `1.20.0`; changelog lists `1.20.0` on 2026-07-21. | https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/changelog/CHANGELOG.html |
| Cinemachine | `com.unity.cinemachine` docs resolve to `6.6.0`; changelog lists `6.6.0` on 2026-05-08. | https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/changelog/CHANGELOG.html |
| Cinemachine compatibility | Cinemachine is compatible with Unity 2022.3 LTS and later; docs warn that Cinemachine 3.x had breaking changes from 2.x. | https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/InstallationAndUpgrade.html |
| URP | `com.unity.render-pipelines.universal` docs resolve to `17.6.0`; changelog page is live for the Unity 6.6-era URP package. | https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.6/changelog/CHANGELOG.html |
| Blender | Blender 5.2 LTS is real, released July 14, 2026, and marketed with two years of LTS updates. | https://www.blender.org/download/releases/5-2/ |

## Findings

### Finding 1

Location: `ARCHITECTURE-SPINE.md:175`

Trigger condition: The Unity Editor row pins `6.6 (6000.6)` but not an installable editor build.

Guard snippet: Pin either `Unity Editor 6000.6.0f1` for the current Update track, or explicitly choose the latest `6000.3.x` LTS patch if stability matters more than new Update-track features.

Potential consequence: Unity Hub, CI, collaborators, and package resolution can land on different 6000.6 patches, making support issues and reproduction harder.

### Finding 2

Location: `ARCHITECTURE-SPINE.md:68-72`, `ARCHITECTURE-SPINE.md:175`

Trigger condition: Unity 6.6 is treated as the MVP base without naming its release-track tradeoff.

Guard snippet: Add an architecture note that Unity 6.6 is an Update release supported until the next release, with a monthly/patch review cadence; otherwise switch the architecture to Unity 6.3 LTS.

Potential consequence: The project can lock itself to a short-support branch while the solo MVP still needs stable tooling and repeatable package behavior.

### Finding 3

Location: `ARCHITECTURE-SPINE.md:175`

Trigger condition: The chosen Unity release was released two days before this review and has active known issues in the official release notes.

Guard snippet: Add an acceptance gate before implementation: install the exact editor, create the template, run a two-player local/Relay smoke test, and record any release-note blockers before declaring the stack adopted.

Potential consequence: A brand-new editor can waste early MVP time on engine/package defects rather than gameplay proof.

### Finding 4

Location: `ARCHITECTURE-SPINE.md:179`

Trigger condition: `com.unity.services.multiplayer` is pinned to `2.1.1`, but official package docs now resolve the current `2.3` line to `2.3.1`.

Guard snippet: Update the planned package pin to `com.unity.services.multiplayer` `2.3.1`, or state why `2.1.1` is deliberately frozen and add a compatibility note for the current docs.

Potential consequence: The project can start from a stale multiplayer SDK and miss fixes added after `2.1.1`, including later build and session handling fixes.

### Finding 5

Location: `ARCHITECTURE-SPINE.md:25-27`, `ARCHITECTURE-SPINE.md:179`

Trigger condition: The source list mixes a versioned package doc for Multiplayer Services `2.1`, current MPS site docs, and a Lobby OpenAPI URL without stating which API layer is canonical.

Guard snippet: Make the canonical path explicit: either "Use Multiplayer Services Sessions API with Relay via `WithRelayNetwork()`" or "Use direct Lobby plus direct Relay SDK APIs"; then align all source URLs and package pins to that path.

Potential consequence: Implementation can duplicate or split lifecycle concerns across Sessions, Lobby, and Relay, creating stale session data, heartbeat mistakes, or confusing join flows.

### Finding 6

Location: `ARCHITECTURE-SPINE.md:90`, `ARCHITECTURE-SPINE.md:224-225`

Trigger condition: The architecture promises an optional invite link, but the current official Multiplayer Services joining docs only establish join code, browsable sessions, and reconnect flows.

Guard snippet: Reword invite links as a custom deep link that carries `session.Code`, or remove the invite-link commitment from MVP scope.

Potential consequence: The team may plan UI and onboarding around a built-in Unity feature that is not evidenced by the current primary docs.

### Finding 7

Location: `ARCHITECTURE-SPINE.md:178-180`, `ARCHITECTURE-SPINE.md:233-242`

Trigger condition: Relay-backed NGO behavior depends on Unity Transport, but `com.unity.transport` is not listed or pinned.

Guard snippet: Add `com.unity.transport` to the stack once Package Manager resolves it, record the exact version in `Packages/manifest.json` and `Packages/packages-lock.json`, and include it in the multiplayer smoke test.

Potential consequence: A transitive transport upgrade can alter Relay connection behavior without any architecture-visible package change.

### Finding 8

Location: `ARCHITECTURE-SPINE.md:176`

Trigger condition: The stack names a Universal 3D / URP template but does not pin `com.unity.render-pipelines.universal`.

Guard snippet: Add the resolved URP package to the stack, currently expected around `com.unity.render-pipelines.universal` `17.6.0` for Unity 6.6-era docs, and lock it in the Unity manifest.

Potential consequence: URP shader/render-graph behavior can drift separately from the editor label, especially if packages are upgraded from Package Manager.

### Finding 9

Location: `ARCHITECTURE-SPINE.md:177`

Trigger condition: `Unity-managed C# for Unity 6.6` is not precise enough for implementation guidance.

Guard snippet: Replace the row with "Roslyn / C# 9.0 as supported by Unity 6.6" and note the Unity-documented caveats around unsupported C# 9 features and records in serialized types.

Potential consequence: Code may be authored with newer C# assumptions or unsupported record/init behavior, causing compile errors or Unity serialization surprises.

### Finding 10

Location: `ARCHITECTURE-SPINE.md:181`, `ARCHITECTURE-SPINE.md:126`

Trigger condition: Cinemachine `6.6.0` is valid, but the architecture does not call out that current Cinemachine docs include major API/architecture changes from Cinemachine 2.x.

Guard snippet: Keep `com.unity.cinemachine` pinned to `6.6.0`, require package-matched samples/docs, and avoid older Cinemachine 2.x tutorials or APIs in implementation stories.

Potential consequence: Developers can copy obsolete camera setup guidance into a new Unity 6.6 project and lose time on renamed components or incompatible scripts.

### Finding 11

Location: `ARCHITECTURE-SPINE.md:168`

Trigger condition: The package-version rule waits until the Unity project exists, but the architecture already makes adopted decisions from exact package versions.

Guard snippet: Add a pre-project "intended stack" table and a post-project "resolved lock" check: after template creation, diff intended pins against `manifest.json` and `packages-lock.json`.

Potential consequence: Architecture approval can silently diverge from the real Package Manager graph on day one.

### Finding 12

Location: `ARCHITECTURE-SPINE.md:183-184`, `ARCHITECTURE-SPINE.md:144`

Trigger condition: Blender 5.2 LTS is valid, but the asset interchange row does not specify whether Unity imports `.blend` source files or only exported FBX/GLB deliverables.

Guard snippet: Keep Blender source files under `ArtSource/Blender`, but define Unity-facing deliverables as exported `.fbx` or `.glb` files plus generated prefabs; avoid relying on Unity to import `.blend` files directly.

Potential consequence: Asset import behavior can depend on local Blender installation details and create inconsistent imports across machines.

## Valid As Written

- `com.unity.netcode.gameobjects` `2.13.2` resolves to current official docs and is fit for a host-authoritative GameObject-based MVP.
- `com.unity.multiplayer.playmode` `3.0.0` resolves to official docs and supports Unity 6.3 and later, so it fits Unity 6.6.
- `com.unity.inputsystem` `1.20.0` resolves to official docs and is pinned enough.
- `com.unity.cinemachine` `6.6.0` resolves to official docs and is compatible with Unity 6.6, with the migration caveat above.
- Blender `5.2 LTS` is a valid current LTS choice and fits the cleanup/export role.

## Suggested Stack Corrections

| Current row | Suggested correction |
| --- | --- |
| Unity Editor: `6.6 (6000.6)` | `6000.6.0f1` plus Update-track cadence, or latest `6000.3.x` LTS patch if release stability wins. |
| Unity project template: `Universal 3D / URP for Unity 6.6` | Add `com.unity.render-pipelines.universal` with the resolved package version, expected `17.6.0` for Unity 6.6-era docs. |
| C#: `Unity-managed C# for Unity 6.6` | `Roslyn / C# 9.0 as supported by Unity 6.6`; document unsupported/caveated C# 9 features. |
| Unity Multiplayer Services: `2.1.1` | Update to `2.3.1` or explicitly freeze to `2.1.1` with a compatibility note. |
| Relay/NGO path | Add `com.unity.transport` as an explicit resolved dependency and smoke-test it with Relay. |
| Optional invite link | Reframe as a custom app/deep link carrying the Unity session join code. |
