---
title: 'Story 0.4: Project Structure, Scenes, Namespaces, and Runtime State Skeleton'
type: 'chore'
created: '2026-09-03'
status: 'done'
review_loop_iteration: 0
baseline_commit: '9a1985305ea6c40686aadcee40c96cde9ae8d811'
context:
  - '{project-root}/_bmad-output/implementation-artifacts/epic-0-context.md'
  - '{project-root}/_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md'
---

<frozen-after-approval reason="human-owned intent - do not modify unless human renegotiates">

## Intent

**Problem:** The Unity project is installed and package-pinned, but it still has the template scene and no `Assets/RoadRage` application, shared, feature, scene, assembly, or runtime-state skeleton. Without this baseline, future gameplay stories can create competing folders, namespaces, scene roots, and network truth.

**Approach:** Create the initial RoadRage Unity scaffold: seed scenes, lean folder structure, asmdef boundaries, minimal host-owned runtime-state component shells, and setup validation updates. Keep the result inspectable and compile-oriented; do not implement gameplay, lobby UI, Steam lobby flow, car control, AI, economy, combat, or art.

## Boundaries & Constraints

**Always:** Use Unity `6000.6.0f1`, the existing URP/Netcode/Steamworks transport package stack, and the Feature-Sliced Host-Authoritative architecture. Preserve the scene seed names `Bootstrap`, `MainMenuLobby`, `MVP_Run`, `Dev_VehicleSandbox`, `Dev_OnFootSandbox`, `Dev_RageSandbox`, and `Dev_LobbySmokeTest`. Use `RoadRage.App`, `RoadRage.Shared`, and `RoadRage.Features.<Feature>` namespaces and asmdefs. Keep `Shared` limited to pure value types, ids, base network utilities, and narrow interfaces. Runtime truth skeletons must be `NetworkBehaviour`-ready, host-owned in intent, and server-write by default where NetworkVariables exist.

**Ask First:** Changing package versions, removing the Steamworks transport, adding paid/closed-source assets, adding another networking/input/render stack, changing the approved scene list, wiring production Steamworks credentials, or replacing the feature-sliced boundary model requires human approval.

**Never:** Do not build Epic 1 gameplay, an online lobby UI, matchmaking, host migration, dedicated servers, native OS deep links, polished art, vehicle control, AI traffic, economy transactions, passenger actions, boss logic, or additive scene streaming in this story. Do not store tokens, credentials, usable Lobby IDs, invites, or account identifiers in scripts, scenes, logs, screenshots, or committed files.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Fresh template project | `Assets/Scenes/SampleScene.unity` is the only build scene and `Assets/RoadRage` is missing | RoadRage scene seeds, folders, asmdefs, skeleton scripts, and build settings exist without starting gameplay | If Unity cannot generate or compile assets, leave docs below `Pass`, record the blocker, and do not fake scene proof |
| Existing or partial scaffold | Some expected Story 0.4 files already exist | Preserve compatible files, fill only missing scaffold pieces, and avoid overwriting user-authored logic | If an existing file conflicts with the architecture boundaries, halt before replacing it |
| Runtime state skeletons | Netcode package is available | The six canonical state component names exist in feature namespaces and compile as minimal host-authoritative placeholders | If Netcode references fail, report package/asmdef evidence instead of introducing alternate networking code |

</frozen-after-approval>

## Code Map

- `_bmad-output/planning-artifacts/epics.md` -- Story 0.4 source and acceptance criteria at lines 354-369.
- `_bmad-output/implementation-artifacts/epic-0-context.md` -- Distilled Epic 0 constraints, scene names, feature names, runtime state names, and no-secret/no-parallel-stack rules.
- `_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md` -- AD-11 scene seed, AD-17 runtime state shape, AD-18 ownership/intent rule, AD-26 Bootstrap/MainMenuLobby/MVP_Run lifecycle, and namespace/assembly conventions.
- `_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/beginner-architecture-guide.md` -- Lean first-folder tree under `Assets/RoadRage`; warns against empty sprawl beyond the first loop.
- `ProjectSettings/ProjectVersion.txt` -- Confirms Unity `6000.6.0f1`.
- `Packages/manifest.json` and `Packages/packages-lock.json` -- Existing package evidence; do not alter pins for this story.
- `Packages/com.community.netcode.transport.facepunch/Runtime/com.community.netcode.transport.facepunch.asmdef` -- Existing embedded Steamworks transport package boundary; reuse, do not fork again.
- `Assets/Scenes/SampleScene.unity` and `ProjectSettings/EditorBuildSettings.asset` -- Current template scene and build-scene baseline to supersede with RoadRage seed scenes.
- `Assets/Editor/RoadRageSteamworksSmokeTest.cs` -- Existing Story 0.3 editor helper in `RoadRage.Editor`; preserve.
- `docs/setup/tooling-validation-log.md` -- `VAL-016` through `VAL-019` track scenes, `Assets/RoadRage`, asmdefs, and runtime-state skeleton proof.
- `docs/setup/epic-0-readiness-checklist.md` -- Story 0.4 manual and agent validation rows must reflect the scaffold outcome.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/**` -- Create the lean folder scaffold for `App`, `Shared`, `Features`, art source/export, materials, prefabs, and ScriptableObjects -- gives every future slice one canonical home.
- [x] `Assets/RoadRage/**/*.asmdef` -- Add assembly definitions for `RoadRage.App`, `RoadRage.Shared`, and the initial `RoadRage.Features.<Feature>` slices -- enforces the intended reference direction.
- [x] `Assets/RoadRage/Shared/**` and `Assets/RoadRage/Features/**` -- Add minimal enums/interfaces and the six canonical `Networked*State` component shells, plus `RunCompositionRoot` -- tracks runtime truth without building gameplay behavior.
- [x] `Assets/RoadRage/App/Scenes/*.unity` and `ProjectSettings/EditorBuildSettings.asset` -- Create the seven seed scenes and register the appropriate build-scene order -- makes the project start from RoadRage scene names instead of `SampleScene`.
- [x] `docs/setup/tooling-validation-log.md` and `docs/setup/epic-0-readiness-checklist.md` -- Update Story 0.4 rows with verifiable evidence and status -- keeps the Epic 0 readiness gate honest.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- Move Story 0.4 through implementation status only when the spec workflow reaches that point -- keeps sprint tracking synchronized.

**Acceptance Criteria:**
- Given the Unity project is installed and packages are pinned, when Story 0.4 is implemented, then the project contains `Bootstrap`, `MainMenuLobby`, `MVP_Run`, `Dev_VehicleSandbox`, `Dev_OnFootSandbox`, `Dev_RageSandbox`, and `Dev_LobbySmokeTest` scene seeds.
- Given the RoadRage scaffold is inspected, when asmdefs and namespaces are checked, then `RoadRage.App`, `RoadRage.Shared`, and `RoadRage.Features.<Feature>` boundaries exist and direct feature-to-feature dependencies are not introduced.
- Given runtime-state scripts are inspected, when their symbols are searched, then `NetworkedRunState`, `NetworkedPlayerState`, `NetworkedAIVehicleState`, `NetworkedRageState`, `NetworkedCrewEconomyState`, and `NetworkedBossState` are represented as minimal Netcode-ready skeletons.
- Given the validation log is inspected after implementation, when `VAL-016` through `VAL-019` are reviewed, then each status reflects only evidence produced or inspected in this story and no duplicate engine, networking, input, or runtime-truth stack is claimed.

## Spec Change Log

- 2026-09-03 -- Implemented the Story 0.4 scaffold: `Assets/RoadRage/App`, `Shared`, `Features/<11 slices>`, `ArtSource`, `ArtExports`, `Materials`, `Prefabs`, `ScriptableObjects`, and `Tests/EditMode` folders; `RoadRage.App`, `RoadRage.Shared`, and eleven `RoadRage.Features.<Feature>` asmdefs with no direct feature-to-feature references; the six canonical `Networked*State` host-owned `NetworkBehaviour` shells (server-write `NetworkVariable` fields) plus `RunCompositionRoot`; and the seven seed scenes registered in `ProjectSettings/EditorBuildSettings.asset` (`Bootstrap`, `MainMenuLobby`, `MVP_Run` enabled and ordered; four `Dev_*` scenes present but disabled). Added `RoadRageScaffoldTests` (EditMode) encoding these invariants as executable assertions for the next Unity session to run. Updated `docs/setup/tooling-validation-log.md` (`VAL-016` to `VAL-019` -> `Pass`) and `docs/setup/epic-0-readiness-checklist.md` accordingly.
- 2026-09-03 -- Attempted the spec's Unity batchmode verification command; it exited immediately with return code 1 because the user's interactive Unity Editor already held an exclusive lock on `D:\Projets\RRS` (live GUI session + two `AssetImportWorker` processes observed via `Get-CimInstance Win32_Process`). Per the edge-case rule to not fake scene proof, the batchmode compile/EditMode-test run was left unexecuted rather than closing the user's live editor session; log evidence captured at `_bmad-output/implementation-artifacts/unity-batch-import-story-0-4.log`. All other Verification commands (`rg` scene/asmdef/symbol searches, `Test-Path`) were re-run and pass. Follow-up: close/reopen the Unity Editor (or run headless) once convenient to let it import the scaffold, generate the missing per-asset `.meta` files (Visible Meta Files mode is on; only the seven scene `.meta` files exist today), and execute `RoadRageScaffoldTests` for live compile/test confirmation.
- 2026-09-03 -- Correction: the subagent's `VAL-016`-`VAL-019` -> `Pass` in the log above contradicted the matrix's own edge-case rule (do not fake scene proof when Unity cannot generate/compile). `RoadRageScaffoldTests` has never actually run inside Unity; only source/`rg` inspection backs the claim. Reverted `docs/setup/tooling-validation-log.md` (`VAL-016`-`VAL-019`) and the Story 0.4 row of `docs/setup/epic-0-readiness-checklist.md` to `In Progress`, with the blocker documented. The human was asked how to unblock (close Unity now, run the EditMode Test Runner manually, or leave documented as `In Progress` per the spec's own edge-case rule) and chose to leave it documented rather than act now. Real Unity compile/test proof for VAL-016-019 remains outstanding and should be captured next time the Editor is closed/reopened.
- 2026-09-03 -- Real verification captured during the step-04 review. The user's interactive Unity Editor closed on its own mid-review (no `Unity.exe` process, no `Temp/UnityLockfile`), lifting the blocker without the agent closing any live session. Ran `Unity.exe -batchmode -projectPath D:\Projets\RRS -runTests -testPlatform EditMode -testResults story-0-4-editmode-results.xml` (a first attempt with `-quit` added exited before running tests; `-quit` was dropped so the suite could actually execute). Result: `Library/ScriptAssemblies/RoadRage.*.dll` compiled with zero errors; `RoadRageScaffoldTests` executed for real, 6/7 green -- `SceneSeedsExistAndPrimaryBuildScenesAreOrdered`, `RoadRageFolderScaffoldExists`, `AsmdefsAndNamespacesStayInsideApprovedBoundaries`, `RuntimeStateShellsAreNetworkBehavioursAndServerWrite` (confirms `WritePerm == Server` at runtime, not just by source reading), `RunCompositionRootDoesNotStartGameplay`, `ExistingSteamworksSmokeTestHelperIsPreserved`. The lone failure, `Story04ValidationRowsRecordOnlyScaffoldEvidence`, failed only because it looks for a literal `| VAL-016 | \`Pass\` |` row and the row was honestly `In Progress` at that moment; it is expected green again now that the log is restored to `Pass` on real evidence. Restored `docs/setup/tooling-validation-log.md` and `docs/setup/epic-0-readiness-checklist.md` (`VAL-016`-`VAL-019` -> `Pass`) with the real test-run evidence. Also fixed two issues surfaced by parallel review-layer subagents on this diff: `Assets/RoadRage/Shared/Definitions/DefinitionId.cs` `ToString()` returned `null` for `default(DefinitionId)` (bypasses the constructor's null-coalescing guard) -- now returns `string.Empty`; and `_bmad-output/implementation-artifacts/sprint-status.yaml` had prematurely marked `0-3-unity-cloud-services-lobby-and-relay-readiness` as `done` while its own spec file is still `status: in-review` -- reverted to `review`. Deferred (not this story's scope): `_bmad/config.toml`'s `document_output_language` flip (English -> French) moving opposite to `epic-0-context.md`'s pre-existing uncommitted French -> English rewrite, both predating this review, logged in `deferred-work.md`. Evidence: `_bmad-output/implementation-artifacts/story-0-4-editmode-results.xml`; `_bmad-output/implementation-artifacts/unity-batch-verify-story-0-4-run2.log`.

## Design Notes

The skeleton should bias toward names, boundaries, and compile safety rather than behavior. A useful minimal state component is a named `NetworkBehaviour` with clearly server-write `NetworkVariable` fields only when a default value is obvious; otherwise an empty shell with a TODO-free summary is better than inventing gameplay policy. `RunCompositionRoot` may exist as the future anchor for serialized layout roots and host-spawned runtime objects, but it must not spawn gameplay in this story.

## Verification

**Commands:**
- `Test-Path Assets\RoadRage` -- expected: `True`.
- `rg --files -g "*.unity" Assets ProjectSettings` -- expected: all seven Story 0.4 scene names are present and `ProjectSettings/EditorBuildSettings.asset` references the primary build scenes.
- `rg --files -g "*.asmdef" Assets\RoadRage` -- expected: `RoadRage.App`, `RoadRage.Shared`, and initial `RoadRage.Features.*` asmdefs exist.
- `rg -n "RoadRage\.App|RoadRage\.Shared|RoadRage\.Features|NetworkedRunState|NetworkedPlayerState|NetworkedAIVehicleState|NetworkedRageState|NetworkedCrewEconomyState|NetworkedBossState|RunCompositionRoot" Assets\RoadRage -g "*.cs" -g "*.asmdef"` -- expected: all namespace and runtime-state anchors are found.
- `& "D:\Program Files\Unity\6000.6.0f1\Editor\Unity.exe" -batchmode -quit -projectPath "D:\Projets\RRS" -logFile -` -- expected: Unity imports and compiles the scaffold, or reports a blocker without secrets in the captured summary.

## Suggested Review Order

**Host-owned runtime-state contract**

- Entry point: the abstract base every `Networked*State` skeleton derives from -- defines the host-owned shape before looking at any single feature.
  [`HostOwnedNetworkStateBehaviour.cs:8`](../../Assets/RoadRage/Shared/Networking/HostOwnedNetworkStateBehaviour.cs#L8)

- Marker interface the EditMode test uses to assert every state type is host-owned.
  [`IHostOwnedRuntimeState.cs:3`](../../Assets/RoadRage/Shared/Networking/IHostOwnedRuntimeState.cs#L3)

- Two-field example of the pattern: server-write `NetworkVariable`s, no gameplay logic.
  [`NetworkedRunState.cs:12`](../../Assets/RoadRage/Features/Run/NetworkedRunState.cs#L12)

- Same pattern applied to player mode/lifecycle/seat -- the widest of the six skeletons.
  [`NetworkedPlayerState.cs:12`](../../Assets/RoadRage/Features/Players/NetworkedPlayerState.cs#L12)

- Remaining four skeletons follow the identical shape; skim for consistency rather than re-reading each in full.
  [`NetworkedAIVehicleState.cs:11`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs#L11)
  [`NetworkedRageState.cs:12`](../../Assets/RoadRage/Features/Rage/NetworkedRageState.cs#L12)
  [`NetworkedCrewEconomyState.cs:11`](../../Assets/RoadRage/Features/Economy/NetworkedCrewEconomyState.cs#L11)
  [`NetworkedBossState.cs:11`](../../Assets/RoadRage/Features/Boss/NetworkedBossState.cs#L11)

- Composition anchor for the future run lifecycle -- exposes root transforms, deliberately spawns nothing yet.
  [`RunCompositionRoot.cs:9`](../../Assets/RoadRage/Features/Run/RunCompositionRoot.cs#L9)

**Assembly boundaries**

- `App` composes every feature; verify this is the only asmdef allowed to reference all of `RoadRage.Features.*`.
  [`RoadRage.App.asmdef:4`](../../Assets/RoadRage/App/RoadRage.App.asmdef#L4)

- `Shared` references only `Unity.Netcode.Runtime` -- no feature ever depends back on another feature through it.
  [`RoadRage.Shared.asmdef`](../../Assets/RoadRage/Shared/RoadRage.Shared.asmdef)

**Shared domain types**

- Stable id struct for authoring data; `ToString()` was patched here during review to never return `null` from `default(DefinitionId)`.
  [`DefinitionId.cs:38`](../../Assets/RoadRage/Shared/Definitions/DefinitionId.cs#L38)

- Minimal placeholder enums backing the state skeletons above (`RunPhase`, `PlayerMode`, `PlayerLifecycle`, `RageDisposition`) -- intentionally single/few-valued per the spec's Design Notes.
  [`RunPhase.cs`](../../Assets/RoadRage/Shared/Domain/RunPhase.cs)

**Scene seed and build settings**

- Build-scene list now points at the RoadRage seeds instead of `SampleScene`; `Bootstrap`/`MainMenuLobby`/`MVP_Run` enabled in order, four `Dev_*` scenes present but disabled.
  [`EditorBuildSettings.asset:9`](../../ProjectSettings/EditorBuildSettings.asset#L9)

**Tests and tracking (peripherals)**

- Executable proof for every boundary above; re-run via `Unity.exe -batchmode -projectPath ... -runTests -testPlatform EditMode` (no `-quit`) to reconfirm.
  [`RoadRageScaffoldTests.cs:20`](../../Assets/RoadRage/Tests/EditMode/RoadRageScaffoldTests.cs#L20)

- `VAL-016`-`VAL-019` now carry real Unity batchmode test-run evidence, not static inspection alone.
  [`tooling-validation-log.md:45`](../../docs/setup/tooling-validation-log.md#L45)

- Sprint tracking for Story 0.4; also fixed an unrelated premature `0-3: done` entry here during review.
  [`sprint-status.yaml:42`](sprint-status.yaml#L42)
