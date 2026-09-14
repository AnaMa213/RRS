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

- source_spec: `_bmad-output/implementation-artifacts/spec-1-3-rough-character-creation-and-player-profile-selection.md`
  summary: Passe de layout sur MainMenuLobby : MenuPanel, SetupPanel et NoticePanel portent un localScale herite de 2.9179332 qui pousse SetupLabel, SettingsSummaryLabel et BackButton hors de l'ecran en Play mode.
  evidence: Mesure des RectTransform en Play mode pendant la Story 1.3, confirmee independamment par la couche de revue verification-gap sur le YAML de scene (SetupPanel &2097947412 et NoticePanel &2062058726 a l'echelle 2.9179332, enfants a y=300, y=-210 et y=-310 contre une reference CanvasScaler 1920x1080). Defaut pre-existant des Stories 1.1 et 1.2, non introduit par la Story 1.3. Invisible pour la suite actuelle : LobbyShellOpensWithDefaultDifficultyVisible assere activeInHierarchy et le texte du libelle, deux conditions vraies pour un element hors ecran.

- source_spec: `_bmad-output/implementation-artifacts/spec-1-3-rough-character-creation-and-player-profile-selection.md`
  summary: Les gardes de frontiere d'assemblies des Stories 1.1 et 1.2 (UiAssemblyDoesNotReferenceLobbyAssembly, AsmdefsAndNamespacesStayInsideApprovedBoundaries) sont des recherches de sous-chaine dans le JSON d'asmdef et deviennent vacantes si Unity ecrit les references en forme GUID.
  evidence: Unity 6000.6.0f1 serialise les references d'asmdef en "GUID:<hash>" quand l'option Use GUIDs de l'Inspector est active ; l'assertion Does.Not.Contain sur le nom d'assembly passe alors inconditionnellement pendant que la reference interdite existe. La Story 1.3 corrige ses propres gardes (resolution des GUID via AssetDatabase.GUIDToAssetPath) mais ne touche pas aux tests des stories precedentes.

- source_spec: `_bmad-output/implementation-artifacts/spec-1-3-rough-character-creation-and-player-profile-selection.md`
  summary: Extraire les helpers de test partages (AssetsPath, FindComponentInScene, AssertSerializedObjectFieldsNonNull, GetPrivateField, PressPlayAndEnterLobbyShell, ClickSerializedButton) dans un type utilitaire commun avant la Story 1.4.
  evidence: Troisieme copie verbatim de ces helpers a travers les suites des Stories 1.1, 1.2 et 1.3, alors que la Code Map demandait de les "reutiliser" ; GetPrivateField est meme duplique entre les suites EditMode et PlayMode de la seule Story 1.3. Chaque story supplementaire multiplie le cout d'une correction unique (le correctif faux-null Unity du Patch 5 de la Story 1.2 devrait aujourd'hui etre applique a trois endroits).

- source_spec: `_bmad-output/implementation-artifacts/spec-3-3-seat-entry-exit-and-passenger-presence.md`
  summary: La porte de proximite d'entree de siege (`NetworkedVehicleSeatService.CanEnterSeat`) se fie a `NetworkedPlayerState.WorldPosition`, qui est ecrite verbatim depuis la RPC client `NetworkedPlayerPresentation.SubmitPoseRpc` sans aucune verification de plausibilite/anti-teleport cote host.
  evidence: Trouve par la revue risk-scaled du diff Story 3.3 : un client modifie peut annoncer une position arbitrairement proche de la voiture et revendiquer n'importe quel siege (y compris conducteur) depuis n'importe ou sur la carte. Racine pre-existante depuis la Story 2.5 (le mouvement a pied est deja entierement client-autoritaire, WorldPosition n'a jamais ete validee), pas introduite par la Story 3.3 qui ne fait que reutiliser cet etat deja en confiance pour une nouvelle porte de validation. Le projet est un jeu coop (pas un contexte competitif adversarial a ce stade), donc severite faible pour l'instant, mais un futur mode competitif ou un cheat public exigerait une validation de mouvement cote host.

- source_spec: `_bmad-output/implementation-artifacts/spec-4-1-rage-state-module-and-definitions.md`
  summary: `RageStateDebugView` (dev-only HUD) vit dans l'assembly de production `RoadRage.Features.Rage` plutot que dans `RoadRage.DevTools`, forcant cet assembly a referencer `Unity.TextMeshPro`.
  evidence: Trouve par la revue risk-scaled du diff Story 4.1 : contrairement a `RageSandboxAutoStart.cs` (correctement isole sous `RoadRage.DevTools` avec garde `#if UNITY_EDITOR`), le HUD de debug est reste dans la feature de production faute d'avoir considere que `RoadRage.DevTools` reference deja plusieurs `RoadRage.Features.*` (meme derogation que `RoadRage.App`) et aurait pu l'heberger sans coupler `Features.Rage` a un package UI. Impact fonctionnel nul aujourd'hui (le composant n'est instancie que dans `Dev_RageSandbox`, scene hors de la liste de build principale, et ce projet est un jeu coop heberge par un pair, sans cible de build serveur dediee/headless a ce stade) ; nettoyage architectural facultatif, pas bloquant.

- source_spec: none
  summary: `NetworkedPlayerState.Money` reste un placeholder divergent alors que l'architecture prevoit `NetworkedCrewEconomyState` comme source de verite du crew wallet.
  evidence: Audit Graphify baseline 2026-09-11 : `RunFlowController.HandleMoneyChanged` lit encore `NetworkedPlayerState.Money` pour le HUD, tandis que `NetworkedCrewEconomyState` existe mais n'est pas cable a ce flux. A resoudre avant toute Story introduisant une economie, une recompense, un achat ou une monnaie reelle ; les Stories Epic 4 ne doivent pas promouvoir ce placeholder en source de verite.

- source_spec: none
  summary: La future Story 4.5 doit examiner la duplication entre `NetworkedPlayerLifecycleIntent` et `NetworkedPlayerReviveIntent` avant de reutiliser revive/help comme effet de crew help.
  evidence: Audit Graphify baseline 2026-09-11 : les deux composants `App/Run` exposent `RequestRevive`/`RequestReviveRpc` et appellent `NetworkedPlayerLifecycleService.TryReviveNearestDowned` ; le service host valide correctement l'emetteur, donc ce n'est pas un bug immediat. Si Story 4.5 reutilise ce flux, elle doit choisir ou consolider un contrat public unique au lieu d'ajouter un troisieme chemin.

- source_spec: none
  summary: `RoadRage.Features.UI` reference `RoadRage.Features.PassengerActions`, ce qui viole la garde `RoadRageScaffoldTests.AsmdefsAndNamespacesStayInsideApprovedBoundaries` (aucune assembly de feature ne doit en referencer une autre).
  evidence: Constaté en lancant la suite EditMode complete apres la Story 4.5, ou ce test et le flake camera ci-dessous etaient les deux seuls echecs. Origine anterieure : `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs:2` importe le feature PassengerActions (`:63`, `:86`, `:144`, `:329`), et `RoadRage.Features.UI.asmdef` a ete modifie dans le meme commit `7991d57 feat(story-4-4): add passenger incident marker`. L'asmdef est identique a `HEAD`, donc la suite EditMode etait deja rouge avant la Story 4.5. Non corrige ici : le spec 4.5 interdit de reecrire les livraisons 4.1-4.4 ; la correction (contrat etroit cote Shared, ou remontee du cablage de l'etat incident dans `RunFlowController`/`RoadRage.App`) revient a la revue finale des 4.1-4.4.

- source_spec: none
  summary: `ThirdPersonCameraTests.MouseActionDrivesNativeOrbitWithoutFrameScaling` echoue (delta souris 0 au lieu de 12) dans la suite EditMode complete, mais passe isolement.
  evidence: Test observe rouge en suite complete a deux reprises (dont le run du 2026-09-13 apres la Story 4.5) et vert 2/2 lorsque seule la fixture `RoadRage.Tests.EditMode.ThirdPersonCameraTests` est executee. Le test instancie `Greybox_PlayerCar`, ajoute un device `Mouse` simule puis lit l'InputAction de Cinemachine : un etat d'Input System laisse par une autre fixture explique le delta nul. Aucun code de la Story 4.5 ne touche l'Input System, Cinemachine ou le rig vehicule ; instabilite d'isolation preexistante, a traiter separement (isolation des devices ou reinitialisation en `[SetUp]`).

- source_spec: `_bmad-output/implementation-artifacts/spec-4-5-persistent-steam-profile-and-main-menu-character-selection.md`
  summary: Les fixtures PlayMode qui chargent `MainMenuLobby` sans rediriger `PlayerProfileFileStore.DefaultFilePath` lisent et peuvent ecraser le profil reel du developpeur.
  evidence: Depuis la Story 4.5, le menu resout et persiste un profil des son ouverture. Seuls `Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests` et `Story16Epic1PlayableCheckpointPlayModeTests` redirigent le chemin ; `Story11MainMenuLaunchPlayModeTests`, `Story12LobbyShellPlayModeTests`, `Story15EmptyMapEntryPlayModeTests`, `Story21OnlineServicesPlayModeTests` et `Story22HostCreatedPrivateRoomPlayModeTests` ne le font pas. Impact reel limite (le fichier n'est ecrit que s'il manque, est illisible ou appartient a un autre compte Steam) mais non nul : la premiere execution ecrit dans `Application.persistentDataPath`, et le contenu relu depend de la machine, ce qui rend ces tests dependants de l'environnement. Correction volontairement differee : elle appartient a l'extraction d'un helper de test partage (deja demandee par une entree anterieure) plutot qu'a cinq retouches dispersees dans des fixtures appartenant a d'autres stories.

- source_spec: none
  summary: Entree obsolete : l'exigence « la future Story 4.5 doit examiner la duplication entre `NetworkedPlayerLifecycleIntent` et `NetworkedPlayerReviveIntent` avant de reutiliser revive/help comme effet de crew help » ne vise plus aucune story.
  evidence: La correction de trajectoire du 2026-09-13 (`_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-13.md`, CC-1/CC-2) remplace la Story 4.5 « Passenger Action Three Provides Crew Help » et le checkpoint 4.6 par « Persistent Steam Profile and Main Menu Character Selection » puis « Profile Freeze, Session Payload, and Selected-Character Spawn ». Aucune action passager de crew help n'est planifiee dans le MVP 1 courant : la consolidation du contrat revive/help reste souhaitable le jour ou ce flux revient, mais elle n'appartient plus a une story identifiee. L'entree d'origine est laissee intacte (format append-only) et cette entree la neutralise explicitement.

- source_spec: `_bmad-output/implementation-artifacts/spec-4-6-profile-freeze-session-payload-and-selected-character-spawn.md`
  summary: Extraire une aide PlayMode partagee pour la portee du chemin profil, puis la brancher sur les cinq fixtures qui chargent `MainMenuLobby` sans rediriger `PlayerProfileFileStore.DefaultFilePath`.
  evidence: Ecarte du perimetre de la Story 4.6 au moment du controle de taille (le spec depassait le plafond du SCOPE STANDARD). C'est le seul livrable de la story qui soit reellement independant : il ne touche ni le gel du profil, ni le payload de session, ni le spawn, et peut etre revu et fusionne seul. Le gel, la garde de persistance et le spawn du personnage selectionne restent, eux, un objectif unique : le payload et le spawn solo consomment l'API `SessionSelection` creee par le gel, donc les separer produirait une moitie non livrable. La fixture PlayMode de la Story 4.6 redirige son propre chemin en ligne, comme le font deja `Story16` et `Story45` ; la dette de fond (les cinq fixtures des Stories 11, 12, 15, 21 et 22) reste donc entiere et s'alourdit d'une troisieme copie du motif tant que l'extraction n'est pas faite.

- source_spec: `_bmad-output/implementation-artifacts/spec-4-6-profile-freeze-session-payload-and-selected-character-spawn.md`
  summary: Trois fixtures PlayMode (`Story11MainMenuLaunchPlayModeTests`, `Story16Epic1PlayableCheckpointPlayModeTests`, `Story44PassengerActionTwoMvpRunPlayModeTests`) sont remontees rouges apres la Story 4.6, avec le seul message parent generique « One or more child tests had errors » et sans cause racine identifiee.
  evidence: Remonte par l'humain sur la suite PlayMode complete du 2026-09-13, apres implementation du gel de selection. Les trois messages transmis sont des enveloppes parent (« One or more child tests had errors »), pas des erreurs enfant : la cause reelle, le type d'echec (assertion, exception, log inattendu via `LogAssert`) et l'ordre d'execution des fixtures n'ont pas ete captures, et la demande d'aller les chercher dans le Test Runner a ete refusee. Le meme trio (`Story11`, `Story16`, `Story44`, plus `Story12`) etait deja remonte rouge apres le pass 2 de la Story 4.5, et la cause alors identifiee etait une pollution d'etat de scene entre fixtures PlayMode partageant le processus et le `RoadRageBootstrap` persistant — mais `Story12` est vert cette fois et le correctif de 4.5 (retrait du test qui chargeait `MVP_Run`) n'a pas suffi, donc l'hypothese d'ordre n'est pas confirmee. Deux elements ecartes par lecture : `RunFlowController.SessionSelection` ne peut pas lever de `NullReferenceException` (le garde `HasProfile` renvoie `MissingProfileMessage` avant), et `Story44` ne touche jamais au profil. NON RESOLU : on ne sait pas non plus si les deux fixtures `Story46` ont ete executees, la remontee ne listant que trois fixtures rouges ; tant que ce n'est pas etabli, la couverture de la matrice de la Story 4.6 par les tests PlayMode reste non prouvee. Plan de triage detaille dans `handoff-2026-09-13-story-4-6-playmode-regressions.md`.

- source_spec: `_bmad-output/implementation-artifacts/spec-5-2-basic-ai-route-following-and-recovery.md`
  summary: Le demarrage solo (menu principal -> Start Game) ne demarre aucune session Netcode, donc toute fonctionnalite ecrite en `NetworkBehaviour` est inerte en local -- constat generalise, a corriger par une story dediee « solo = host a un joueur ».
  evidence: Remonte par l'humain apres la Story 5.2 : le trafic IA roule en lobby en ligne et pas en lobby local. Cause racine etablie par lecture : `StartHost()` n'apparait dans le code de production qu'en un seul endroit, `Assets/RoadRage/App/Lobby/LobbyFlowController.cs:430`, suivi de `manager.SceneManager.LoadScene(MvpRunSceneName)` a la ligne 445. Le chemin solo passe par `AppSceneRouter.LoadMvpRun()` (`Assets/RoadRage/App/Services/AppSceneRouter.cs:24`) qui fait un `SceneManager.LoadScene` nu, sans NetworkManager. Sans session, les `NetworkObject` places en scene ne sont jamais spawned : `OnNetworkSpawn` ne se declenche pas et `NetworkedAIVehicleDriverController.FixedUpdate` sort immediatement sur `if (!IsServer)`. Ce n'est pas propre a la Story 5.2 : c'est la raison pour laquelle chaque story precedente a du ecrire un jumeau solo a la main (45 occurrences de `LocalSolo*` dans le code de production, concentrees dans `RunFlowController`). Correction retenue avec l'humain le 2026-09-14 : story dediee ouverte par bmad-correct-course, faisant demarrer un host Netcode a un joueur sur le chemin solo pour unifier les deux lobbies, plutot qu'une 46e divergence locale dans le controleur IA. Impact hors perimetre 5.2 : touche les Epics 1 a 4 et impose une reverification complete.

- source_spec: `_bmad-output/implementation-artifacts/spec-5-3-unified-solo-and-online-session-start.md`
  summary: Le code de lobby affiche n'est ni copiable dans le presse-papier ni lisible : il faudrait un code court (5 caracteres alphanumeriques en majuscules) et un bouton « copier ».
  evidence: Demande de l'humain pendant la verification manuelle a deux clients Steam de la Story 5.3. Aujourd'hui `LobbyRosterScreen.ShowRoomCode` affiche l'identifiant de lobby Steam brut (`lobbyRoom.JoinCode.ToString()` / `lobbyJoin.JoinedLobbyId.ToString()`), un entier 64 bits que l'humain doit recopier a la main pour le transmettre. Hors perimetre gele de la Story 5.3 (dont les AC ne portent que sur l'unification du chemin de demarrage de session) : introduire un code court exige une table de correspondance code-court -> identifiant Steam ou un encodage reversible, plus la mise a jour du parsing de `HandleJoinByCodeRequested` et des tests de la Story 2.3 -- c'est un objectif livrable independamment.

- source_spec: `_bmad-output/implementation-artifacts/spec-5-2-basic-ai-route-following-and-recovery.md`
  summary: La recuperation « blocage » teleporte le vehicule sur le waypoint courant, qui peut etre precisement le lieu du blocage lorsque plusieurs IA partagent la meme route -- boucle possible de teleportations toutes les N secondes avec interpenetration.
  evidence: Constat de la revue de la Story 5.2. Les trois instances de `MVP_Run` referencent la meme `RouteWaypoints` ; si une IA se retrouve bloquee a proximite du waypoint qu'elle vise (poussee par le joueur contre une autre IA arretee), `RecoverAtWaypoint` la repositionne exactement sur ce waypoint, donc potentiellement dans le collider de l'autre vehicule, et remet `stuckElapsedSeconds` a zero. Non corrige dans cette story : « reinitialisation au waypoint courant » est le comportement inscrit dans la matrice I/O figee de la spec, et tout degagement plus malin (decalage lateral, saut au waypoint suivant, evitement mutuel) releve du controleur de trafic de la Story 5.3 / de la densite reseau de la 5.5. Risque reel faible en l'etat : les trois IA partent equidistantes, a vitesse identique et dans le meme sens, donc elles ne se rattrapent pas sans intervention du joueur.

- source_spec: `_bmad-output/implementation-artifacts/spec-5-4-rage-driven-ai-behavior-states.md`
  summary: Corriger le ciblage des vehicules IA pour les actions rage/peur : locks individuels T/Y, camera, selection temporaire dans la portee et validation Host–Client.
  evidence: Scinde a la demande humaine apres que la specification combinee comportements IA + ciblage ait atteint 3177 tokens. Le ciblage est un livrable autonome : il modifie l'intention joueur, la camera et la validation reseau, alors que les comportements 5.4 ne touchent que l'etat et la conduite IA. Les lier dans une seule story rendrait les deux changements difficiles a verifier et revoir separement.

- source_spec: `_bmad-output/implementation-artifacts/spec-5-4-rage-driven-ai-behavior-states.md`
  summary: Résolu par planification : le ciblage IA Rage/Fear est désormais la Story 5.5 « Networked AI Rage Targeting ».
  evidence: Décision explicite de Kenan le 2026-09-14. La story est inscrite dans `epics.md`, suivie dans `sprint-status.yaml`, et les stories précédemment 5.5–5.7 sont décalées en 5.6–5.8. Cette entrée append-only préserve la trace du report initial sans le laisser comme dette non planifiée.

- source_spec: `_bmad-output/implementation-artifacts/spec-5-4-rage-driven-ai-behavior-states.md`
  summary: `AIVehicleBehaviorDebugView` (label dev) vit dans l'assembly de production `RoadRage.Features.Vehicles` et lui fait referencer `Unity.TextMeshPro` -- exactement la meme dette que celle deja consignee pour `RageStateDebugView` dans `RoadRage.Features.Rage` (Story 4.1).
  evidence: Le chemin `Assets/RoadRage/Features/Vehicles/AIVehicleBehaviorDebugView.cs` est impose par la liste de taches de la spec 5.4, donc suivi tel quel. `RoadRage.DevTools` reference deja `RoadRage.Features.Vehicles` et aurait pu heberger la vue sans coupler la feature a un package UI. Impact fonctionnel nul (composant de presentation locale, lecture seule) ; a traiter en une seule fois avec la dette 4.1 si le nettoyage architectural est decide.

- source_spec: none
  summary: Cloture append-only des entrees VCS/.blend de Stories 0.1, 0.2 et 0.5 : le depot est initialise, une `.gitignore` Unity est presente, et les brouillons `Blender/*.blend` / `*.blend1` sont ignores.
  evidence: Cette entree neutralise explicitement les constats initiaux d'absence de VCS, de `.gitignore` Unity et de decision pour `Blender/Untitled.blend`. La documentation d'onboarding peut encore etre amelioree si un nouveau contributeur arrive, mais ce n'est plus une dette de configuration du workspace courant.

- source_spec: none
  summary: Cloture append-only de l'entree de frontiere Story 4.4 : `RoadRage.Features.UI` ne reference plus `RoadRage.Features.PassengerActions`.
  evidence: `Assets/RoadRage/Features/UI/RoadRage.Features.UI.asmdef` ne liste que Shared, Netcode, UI et TextMeshPro, et `RunCheckpointHudScreen.cs` ne comporte plus d'import PassengerActions. Le constat historique est conserve append-only, mais la garde d'assemblies ne porte plus cette violation.

- source_spec: none
  summary: Effectuer un audit dedie des warnings Unity et des API/methodes depreciees ou obsoletes avant le prochain checkpoint Epic.
  evidence: Aucune liste de reference ni regle de tri ne distingue aujourd'hui les warnings introduits par le travail courant, les warnings historiques benins et les usages d'API a migrer. L'audit doit partir de la Console Unity et des sorties de test/build, inventorier chaque occurrence avec son origine, puis corriger ou documenter uniquement les cas reels ; ne pas lancer une migration globale speculative.
