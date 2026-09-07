# Deferred Work

- source_spec: `_bmad-output/implementation-artifacts/spec-0-1-setup-readiness-checklist-and-local-workspace-baseline.md`
  summary: No local workspace baseline guidance exists yet for version control (git init, Unity-appropriate .gitignore, Git LFS for binary assets).
  evidence: Repo currently has no VCS at all; a story titled "local workspace baseline" never mentions git, and none of Epic 0's stories 0.1-0.8 cover it either. Real gap, but adding it changes frozen scope, so it needs a human decision on where it belongs rather than a silent patch.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-1-setup-readiness-checklist-and-local-workspace-baseline.md`
  summary: No cross-references tie checklist story rows to their supporting `VAL-###` entries in the tooling log or `ADDON-###` entries in the adoption register.
  evidence: Makes it harder to verify a checklist row's completeness by cross-checking the detailed log/register entries that back it; a nice-to-have traceability improvement, not required by the story's acceptance criteria.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-1-setup-readiness-checklist-and-local-workspace-baseline.md`
  summary: VAL-011 (Cinemachine) has an ambiguous pass criterion when the package ships pre-embedded rather than needing explicit install.
  evidence: "Unity resout `com.unity.cinemachine` `6.6.0` si non deja embarque ou active" does not state what status to record if it is already embedded, leaving the validator without a concrete rule for that branch.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-1-setup-readiness-checklist-and-local-workspace-baseline.md`
  summary: `docs/setup/addon-adoption-register.md` has no mechanism to revisit an `Adopt` decision if the underlying package/version changes later.
  evidence: A pinned Unity package version bump could invalidate a prior compatibility assessment; nothing in the register or gate rules triggers re-evaluation, so decisions can go stale silently.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-1-setup-readiness-checklist-and-local-workspace-baseline.md`
  summary: `tooling-validation-log.md` VAL-017/VAL-018 reference the `Assets/RoadRage` feature folder structure only in prose elsewhere, without enumerating the concrete `RoadRage.Features.<Feature>` names to check against.
  evidence: The validator has no concrete checklist of expected feature names (Vehicle, OnFoot, PassengerActions, Rage, Economy, Lobby/Network, Run, Boss, SandboxStops, UI) to verify Story 0.4 output against.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-1-setup-readiness-checklist-and-local-workspace-baseline.md`
  summary: The setup docs prohibit committing secrets/tokens but never state where they should actually be stored locally (env file, OS keychain, dashboard-only, etc.).
  evidence: A solo developer following the checklist has no guidance on the correct place to keep Unity Cloud/Relay credentials, only what not to do with them.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-1-setup-readiness-checklist-and-local-workspace-baseline.md`
  summary: No index/README in `docs/setup/` explains the relationship between the three files (checklist, tooling log, adoption register) or their reading order.
  evidence: A new reader has to already know the structure; discoverability relies on tribal knowledge rather than a pointer file.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-2-unity-editor-project-creation-and-package-pinning.md`
  summary: `docs/setup/story-0-2-unity-install-tutorial.md` never tells the user to add a Unity-appropriate `.gitignore` (`Library/`, `Temp/`, `obj/`, `Logs/`, etc.) once the project exists, before any commit happens.
  evidence: Same underlying gap as the earlier deferred VCS-baseline item, now concrete once a real Unity project folder exists to gitignore; still out of this story's scope since no Epic 0 story (0.1-0.8) owns VCS setup.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-2-unity-editor-project-creation-and-package-pinning.md`
  summary: No defined convention for where to store screenshot evidence; the tutorial and tooling log ask for "une capture" but a Markdown table cell cannot hold an image.
  evidence: `docs/setup/tooling-validation-log.md`'s "Chemin/resume preuve" column has no naming/path convention (e.g. `docs/setup/evidence/VAL-004.png`) for binary proof, only text.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-2-unity-editor-project-creation-and-package-pinning.md`
  summary: No fallback guidance for restricted-network/corporate-proxy scenarios blocking Unity Hub sign-in or package downloads.
  evidence: The tutorial assumes unrestricted home internet access, consistent with the project's solo-beginner target, but offers no troubleshooting note if that assumption fails.

- source_spec: none
  summary: `_bmad/config.toml`'s `document_output_language` flipped from `English` to `French` in the same uncommitted diff where `_bmad-output/implementation-artifacts/epic-0-context.md` was rewritten from French to English -- the two changes move in opposite directions with no stated reconciliation.
  evidence: Both changes were sitting uncommitted before Story 0.4 review started and are unrelated to Story 0.4's scaffold scope; a human needs to decide which language BMAD output should actually be in and re-align the config with the docs (or vice versa).

- source_spec: `_bmad-output/implementation-artifacts/spec-0-3-unity-cloud-services-lobby-and-relay-readiness.md`
  summary: Story 0.3 was closed to `done` on explicit user request (to move on to Story 0.5) before `VAL-013` to `VAL-015` (Steam private lobby runtime, `NetworkManager` connection, `MaxPlayers = 4` cap, invite/Lobby ID wrapper) received any real proof; they remain `In Progress` in `docs/setup/tooling-validation-log.md`.
  evidence: These four proofs are exactly the lobby/networking readiness evidence the Epic 0 go/no-go gate (Story 0.8) needs before Epic 1/2 online work starts; must be captured and the log updated with real evidence before Story 0.8 can pass.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-5-codex-claude-unity-mcp-and-blender-mcp-configuration.md`
  summary: No license/provenance/adoption-register entry exists for the third-party MCP tooling being adopted (CoplayDev `unity-mcp`, ahujasid `blender-mcp`), even though the project tracks exactly that (license, cost, maintenance, dependency risk) for other third-party code via the Story 0.7 adoption register.
  evidence: `docs/setup/addon-adoption-register.md`'s criteria (license, cost, Unity compatibility, maintenance, dependency impact, source/editability) apply just as much to MCP server tooling as to UI/controller/asset packages, but Story 0.5's tutorial and VAL-020/VAL-023 never route through that register; a human should decide whether Story 0.5 or Story 0.7 owns recording this.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-5-codex-claude-unity-mcp-and-blender-mcp-configuration.md`
  summary: Blender `5.2 LTS` installation is listed as a Story 0.5 step (`VAL-022`) and also as Story 0.6's own manual-action row in `docs/setup/epic-0-readiness-checklist.md:51`, with no cross-reference clarifying whether one installation satisfies both or the validations are duplicated under two different IDs.
  evidence: This overlap originates in `_bmad-output/planning-artifacts/epics.md` itself (Story 0.5's Blender MCP smoke test implies Blender is already installed, while Story 0.6's AC independently requires proving the Blender `5.2 LTS` install) -- a human should clarify which story owns the install proof before Story 0.6 starts.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-5-mcp-validation-evidence-update.md`
  summary: `Blender/Untitled.blend` and `Blender/Untitled.blend1` (the Story 0.5 Blender MCP smoke-test source file) sit untracked in the repo with no recorded decision on whether to commit them or add them to `.gitignore`; only the generated `Blender/exports_test/` output is currently ignored.
  evidence: Per `[[project_blender_asset_intake_gate]]`, Blender MCP output is draft/scratch material until Story 0.6's intake pipeline exists, which argues for keeping the source `.blend` out of version control too, but that is a repo-hygiene call for the user rather than something to decide unilaterally while just updating validation evidence.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-6-blender-and-3d-asset-intake-pipeline.md`
  summary: The Story 0.6 intake tutorial's worked example covers only a single static primitive (a cube); multi-object hierarchies, rigged/skinned meshes, and assets that already carry colliders have no dedicated guidance.
  evidence: `mcp-tooling-setup.md`'s 9-step checklist and AD-13/AD-27 are written generically; nothing in the architecture spine or epics.md AC requires per-asset-type guidance yet, and no real non-trivial asset has entered the pipeline so far -- adding this now would be speculative rather than grounded in an actual case.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-6-blender-and-3d-asset-intake-pipeline.md`
  summary: No remediation/undo guidance exists for a prefab that was created before completing the scale-test step (only prevention -- "refuse the conversion" -- is documented, not correction after the fact).
  evidence: This touches prefab lifecycle/versioning policy once gameplay may already reference the prefab, which is a bigger decision than this docs-only story's scope and overlaps AD-27's stability guarantees; needs explicit human policy before being documented as a rule.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-6-blender-and-3d-asset-intake-pipeline.md`
  summary: `VAL-025` bundles five distinct proof stages (Blender cleanup, export, Unity import, scale test, prefab conversion) under one status field, so partial progress (e.g. steps 1-3 done, 4-5 pending) cannot be represented.
  evidence: `VAL-025` was already reserved as a single row by a prior story before Story 0.6 touched it; splitting it into sub-IDs (VAL-025a..e) changes the tooling-validation-log.md schema/numbering convention used across all of Epic 0 and needs a human decision, not a silent patch inside one story.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-6-blender-and-3d-asset-intake-pipeline.md`
  summary: No objective threshold defines when an asset transitions from "brouillon" (scratch/draft, like `Blender/exports_test/`) to requiring storage under `Assets/RoadRage/ArtSource/Blender/`.
  evidence: Two operators following the tutorial today could reasonably store the same in-progress asset in different locations; low real risk while this is a solo-dev project with one active example, but worth a clearer rule if the pipeline sees real throughput.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-6-blender-and-3d-asset-intake-pipeline.md`
  summary: Placing a `.blend` file directly under `Assets/` (as the tutorial instructs for `Assets/RoadRage/ArtSource/Blender/`) triggers Unity's native Blender-file auto-import, independent of the explicit FBX/GLB export -> ArtExports -> scale-test -> prefab path the rest of the tutorial documents as the controlled route -- this interaction is never addressed.
  evidence: The project's own `.gitignore` comment notes Unity supports Blender asset imports natively; nothing in the tutorial says whether that native auto-import is acceptable, should be disabled, or is a silent duplicate-import risk. Needs a human decision on the intended behavior before documenting a rule.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-6-blender-and-3d-asset-intake-pipeline.md`
  summary: No guidance on FBX-vs-GLB texture/material portability (FBX commonly needs external texture files kept relative to it; GLB can embed textures) -- a common real-world Blender-to-Unity intake failure mode not mentioned anywhere in the tutorial.
  evidence: Etape 3/4/5 cover material slots, normals, and import warnings but never texture file portability specifically; only one worked example (a simple material, no textures) exists so far, making this speculative rather than grounded in an actual failure.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-6-blender-and-3d-asset-intake-pipeline.md`
  summary: The frozen I/O & Edge-Case Matrix has no scenario for the AD-27 "art replaces an existing gameplay greybox" path, even though AD-27 replacement stability is one of the two rules this story exists to document.
  evidence: Root cause is inside the spec's `<frozen-after-approval>` block (human-owned intent), so it cannot be silently amended during a bad_spec loopback -- needs the human to decide whether a replacement-scenario I/O row belongs in this story or a later one (no real art replacement has happened yet).

- source_spec: `_bmad-output/implementation-artifacts/spec-0-6-blender-and-3d-asset-intake-pipeline.md`
  summary: No file/asset organization or naming convention exists within `ArtSource/Blender/`, `ArtExports/`, or `Prefabs/` once more than one asset exists (flat vs. per-asset subfolder, naming tied to source object name).
  evidence: Defensible as an intentional "Ask First" scope limit for a solo-dev, single-example story; becomes a real gap once the pipeline sees real throughput with multiple assets.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-6-blender-and-3d-asset-intake-pipeline.md`
  summary: The "Plan collider" section's two cases (new asset vs. art replacement) have no third case for authoring a brand-new gameplay object type that needs its first-ever `NetworkObject` registration and gameplay component wiring, not just colliders.
  evidence: This touches networked-gameplay authoring conventions (Netcode ownership, component wiring) that belong with a gameplay feature-slice story, not a Blender-side asset-cleanup tutorial; needs human scoping before being folded into either this story or a later one.

- source_spec: `_bmad-output/implementation-artifacts/spec-0-6-blender-and-3d-asset-intake-pipeline.md`
  summary: No versioning/overwrite policy exists for re-saving a `.blend` source or re-exporting an FBX/GLB of the same name after a correction (e.g. after fixing a scale or normals issue found late).
  evidence: Silent overwrite risks losing traceability between asset versions; a real policy (overwrite-in-place vs. suffix/version folders) needs a human decision, not an invented default.

- source_spec: `_bmad-output/implementation-artifacts/spec-1-1-bootstrap-and-main-menu-launch.md`
  summary: `UserNoticeChannel.Clear()` does not raise a "cleared" notification, so a UI subscriber that already rendered a notice has no signal to hide it when the channel is cleared while the notice is still displayed.
  evidence: Currently unreachable in Story 1.1 (no shipped production code calls `Notices.Publish(...)` yet -- the channel only exists for Epic 2 to wire real join/socket/service failures into), so fixing it now risks guessing an API shape (event vs. UI polling) that Epic 2's real usage should drive instead.

- source_spec: `_bmad-output/implementation-artifacts/spec-1-1-bootstrap-and-main-menu-launch.md`
  summary: `AppSceneRouter.LoadMainMenu()` has no defensive handling if the target scene is missing from Build Settings at runtime -- a shipped build would silently hang with no user-facing notice or fallback.
  evidence: Low probability today (the scene is locked by an EditMode test plus an explicit spec `Ask First` boundary on renaming/reordering build scenes), but the same unguarded `SceneManager.LoadScene` pattern will recur every time a future story adds a scene transition -- worth a project-wide convention rather than a one-off fix inside this story.

- source_spec: `_bmad-output/implementation-artifacts/spec-1-1-bootstrap-and-main-menu-launch.md`
  summary: The two EditMode "architecture guard" tests (`UiFeatureSourceNeverCallsSceneManagementOrApplicationQuit`, `QuitApplicationIsGuardedAndOnlyCallsApplicationQuitOutsideTheEditor`) verify their contract via raw source-text/substring matching, not executed behavior -- they can false-positive-fail on harmless reformatting and false-negative-pass on an equivalent call routed through an alias, a fully-qualified name, or reflection.
  evidence: This mirrors the project's existing convention (`RoadRageScaffoldTests.RunCompositionRootDoesNotStartGameplay` uses the same source-inspection style for a similarly hard-to-runtime-test guard), so it was a deliberate, accepted trade-off at plan time rather than an oversight -- but it is a real methodology weakness worth reconsidering project-wide if these architecture rules become load-bearing.

- source_spec: `_bmad-output/implementation-artifacts/spec-1-1-bootstrap-and-main-menu-launch.md`
  summary: If `MainMenuFlowController`'s serialized `screen` reference is ever unassigned in the shipped scene, the failure mode is a single Editor-only console warning with no in-game user-visible error, even though the story's own boundary requires visible errors for service/network failures.
  evidence: Today this misconfiguration path is guarded by the checked-in EditMode test `MainMenuLobbySceneContainsWiredComponents` (which fails the build/CI if the reference is ever unassigned in the committed scene), so the gap is only reachable via a manual, uncommitted Editor edit -- low severity, but worth revisiting once Story 1.2+ adds more serialized UI wiring where this pattern could repeat.

- source_spec: `_bmad-output/implementation-artifacts/spec-1-2-local-lobby-shell-and-match-settings-draft.md`
  summary: Une notice publiee reste affichee indefiniment : `MainMenuScreen.ClearNotice()` n'est appele que dans `Awake()`, donc apres un clic Create Lobby l'avertissement persiste a l'ecran pendant toutes les actions suivantes et jusque de retour dans le menu principal.
  evidence: Story 1.2 est la premiere story a reellement publier des notices, ce qui rend atteignable un manque qui existait deja dans le code Story 1.1 (voir l'entree deferee sur `UserNoticeChannel.Clear()`). La matrice I/O du spec exige "Notice visible" et ne dit rien du congediement, donc ajouter un cycle de vie de notice (auto-effacement, bouton de fermeture, effacement a la navigation) depasse le perimetre fige de cette story et demande une decision humaine sur le comportement voulu.

- source_spec: `_bmad-output/implementation-artifacts/spec-1-2-local-lobby-shell-and-match-settings-draft.md`
  summary: Toute la couverture comportementale de Story 1.2 repose sur la suite PlayMode, un chemin d'execution qui a deja rendu un `RunStarted testCaseCount=0` silencieux dans ce projet (8 tentatives documentees en Story 1.1) sans qu'aucun garde-fou ne distingue une suite vide d'une suite verte.
  evidence: Verifie par le reviewer : aucun `.github/workflows` dans le depot, les seules references `-runTests` sont des replis en prose dans les specs, et la suite EditMode n'assure aucun comportement Story 1.2 (uniquement les defauts de `MatchSettings`, le cablage de scene et deux scans de source). Le correctif — faire echouer un run dont le nombre de cas executes est nul, en parsant le XML de resultats — est un changement d'outillage transverse au projet, pas une modification de ce diff.

- source_spec: `_bmad-output/implementation-artifacts/spec-1-2-local-lobby-shell-and-match-settings-draft.md`
  summary: `Story12LobbyShellTests.UiAssemblyDoesNotReferenceLobbyAssembly` duplique en plus faible une assertion existante : `RoadRageScaffoldTests.AsmdefsAndNamespacesStayInsideApprovedBoundaries` parse deja chaque asmdef de feature et echoue sur toute reference commencant par `RoadRage.Features.`, couvrant UI->Lobby et toutes les autres paires.
  evidence: Le nouveau test est un `Does.Not.Contain` sur le texte de l'asmdef, donc il ne verifie qu'une seule direction et devient inoperant si Unity reecrit les references sous forme `GUID:`. Consolider les gardes d'architecture au lieu d'en ajouter une copie par story est un chantier de rangement transverse, pas un correctif de cette story.

- source_spec: `_bmad-output/implementation-artifacts/spec-1-2-local-lobby-shell-and-match-settings-draft.md`
  summary: La duree de vie de `MatchSettings` n'est pas decidee : l'objet est construit dans `LobbyFlowController.Awake()` et vit sur un GameObject de scene, alors que les services qu'il doit alimenter vivent sur le bootstrap `DontDestroyOnLoad`.
  evidence: La garantie "conserve son etat courant apres Back" ne tient aujourd'hui que parce que Back masque un panneau sans rien recharger ; le premier vrai chargement de scene en Epic 2 detruira les reglages brouillon. La classe a explicitement vocation a survivre vers un futur `NetworkedLobbyState`, donc l'emplacement de propriete (bootstrap vs scene) merite une decision explicite au moment ou l'Epic 2 cablera la synchronisation.
