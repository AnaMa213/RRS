---
title: 'Story 1.2 — Coquille de lobby locale et reglages de partie brouillon'
type: 'feature'
created: '2026-09-07'
status: 'done'
review_loop_iteration: 0
baseline_commit: '3641e5a176b8f37a0c8fc3a7b5b5e568c53b5665'
context:
  - '{project-root}/_bmad-output/implementation-artifacts/epic-1-context.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem :** Le panneau de setup de `MainMenuLobby` (Story 1.1) est un placeholder inerte : appuyer sur Play ne montre qu'un label et un bouton Back, sans aucune place reelle pour creer une partie ou regler des parametres.

**Approach :** Ajouter un ecran UGUI de coquille de lobby (Create Lobby, Join By Code placeholder, Start Game, Back, reglage de difficulte) pilote par un nouveau controller App qui construit/modifie un objet `MatchSettings` local dans le feature `Lobby`, consomme un enum `Difficulty` partage, et publie une notice visible pour toute action en ligne indisponible via le canal existant.

## Boundaries & Constraints

**Always :**
- Memes frontieres d'assemblies que Story 1.1 : `RoadRage.Features.UI` ne reference que `RoadRage.Shared` + `UnityEngine.UI`/`Unity.TextMeshPro` ; `RoadRage.Features.Lobby` ne reference que `RoadRage.Shared` ; seule `RoadRage.App` compose UI + Lobby.
- L'ecran UI n'appelle jamais `SceneManager.LoadScene` et ne mute jamais `MatchSettings` directement : il emet des intentions, la couche App traduit vers le feature Lobby.
- `MatchSettings` est un objet C# pur (pas de `MonoBehaviour`, pas de `ScriptableObject`, pas de `NetworkVariable`) — "les valeurs de session runtime ne vivent jamais dans des assets ScriptableObject" (epic-1-context).
- Create Lobby, Join By Code et Start Game passent par `UserNoticeChannel` existant pour tout retour "indisponible" — jamais d'echec silencieux.
- Logs prefixes par couche : `[Lobby]` (feature), `[App]` (controller), `[UI]` (ecran), coherent avec Story 1.1.
- Ajouter `RoadRage.Features.Lobby` aux references de `RoadRage.Tests.EditMode` et `RoadRage.Tests.PlayMode`.

**Ask First :**
- Choix visuel du controle de difficulte (Dropdown TMP vs boutons cycle) si ambigu au moment de peupler la scene.
- Toute modification de scene de build ou de reference asmdef au-dela de celles listees ici.

**Never :**
- Aucune vraie creation de lobby Steam, aucun code de session reel, aucun join reseau (Epic 2).
- Aucun `NetworkManager` demarre, aucun `NetworkedLobbyState` cree — les squelettes `Networked*State` restent hors-perimetre tant que le reseau n'est pas actif.
- Ne pas modifier `MainMenuScreen`/`MainMenuFlowController` au-dela du branchement necessaire pour afficher le nouvel ecran.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Ouverture lobby | Play presse depuis le menu | Coquille affichee : Create Lobby, Join By Code, Start Game, Back, difficulte par defaut visible | N/A |
| Create Lobby | Clic Create Lobby | Notice visible "indisponible hors ligne" ; `MatchSettings` inchange | N/A |
| Join By Code | Clic Join By Code | Notice visible "indisponible hors ligne" | N/A |
| Start Game | Clic Start Game | Notice visible "indisponible" ; aucune scene chargee | N/A |
| Changement difficulte | Interaction sur le controle de difficulte | `MatchSettings.Difficulty` mis a jour, libelle affiche synchronise | N/A |
| Back | Clic Back | Retour au menu ; `MatchSettings` conserve son etat courant | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Lobby/RoadRage.Features.Lobby.asmdef` -- scaffold vide (references `RoadRage.Shared` uniquement), destination de `MatchSettings`. Deja correct, lecture seule.
- `Assets/RoadRage/Shared/Domain/PlayerMode.cs`, `RunPhase.cs` -- modele de style pour le nouvel enum `Difficulty` (namespace `RoadRage.Shared.Domain`, valeurs explicites).
- `Assets/RoadRage/Shared/Presentation/UserNotice.cs`, `UserNoticeChannel.cs` -- canal reutilise tel quel pour les retours "indisponible".
- `Assets/RoadRage/Features/UI/MainMenuScreen.cs:100` (`ShowSetupPlaceholder`) -- proprietaire actuel de `setupPanel` ; le nouvel ecran vient completer le contenu de ce panneau, pas remplacer la classe.
- `Assets/RoadRage/App/MainMenu/MainMenuFlowController.cs` -- patron de couture UI/App a reproduire pour le nouveau controller Lobby.
- `Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs:20` (`EnsureInstance`), propriete `Notices` -- a reutiliser pour publier les notices "indisponible".
- `Assets/RoadRage/App/Scenes/MainMenuLobby.unity:2036` (`SetupPanel`) -- contient aujourd'hui `SetupLabel` + `BackButton` uniquement ; a completer avec Create Lobby, Join By Code, Start Game, controle de difficulte, libelle de reglages.
- `Assets/RoadRage/Tests/EditMode/RoadRage.Tests.EditMode.asmdef`, `Tests/PlayMode/RoadRage.Tests.PlayMode.asmdef` -- ajouter la reference `RoadRage.Features.Lobby`.
- `Assets/RoadRage/Tests/EditMode/Story11MainMenuLaunchTests.cs` -- modele de style de test EditMode (reflexion, refs serialisees, `EditorBuildSettings`).

## Tasks & Acceptance

**Execution :**
- [x] `Assets/RoadRage/Shared/Domain/Difficulty.cs` -- creer l'enum (`Easy = 0`, `Normal = 1`, `Hard = 2`) -- reglage partage, mappable plus tard sur un etat reseau.
- [x] `Assets/RoadRage/Features/Lobby/MatchSettings.cs` -- creer un objet C# pur (`Difficulty` + emplacement documente pour parametres futurs extensibles) -- "objet de donnees local synchronisable plus tard" (AC epics.md).
- [x] `Assets/RoadRage/Features/UI/LobbyShellScreen.cs` -- creer le composant UGUI (refs serialisees boutons Create Lobby/Join By Code/Start Game/Back, controle de difficulte, libelle reglages) exposant des evenements d'intention (`CreateLobbyRequested`, `JoinByCodeRequested`, `StartGameRequested`, `BackRequested`, `DifficultyChanged`) -- meme patron que `MainMenuScreen`.
- [x] `Assets/RoadRage/App/Lobby/LobbyFlowController.cs` -- creer le composant App reliant `LobbyShellScreen` a `MatchSettings` et au canal de notices (Create/Join/Start publient une notice "indisponible hors ligne" ; `DifficultyChanged` met a jour `MatchSettings` et rafraichit le libelle) -- seule couture UI/App/Lobby.
- [x] `Assets/RoadRage/App/Scenes/MainMenuLobby.unity` -- peupler `SetupPanel` (Create Lobby, Join By Code, Start Game, controle difficulte + libelle ; Back deja present) et assigner `LobbyShellScreen` + `LobbyFlowController` avec toutes les refs.
- [x] `Assets/RoadRage/Tests/EditMode/RoadRage.Tests.EditMode.asmdef`, `Tests/PlayMode/RoadRage.Tests.PlayMode.asmdef` -- ajouter la reference `RoadRage.Features.Lobby`.
- [x] `Assets/RoadRage/Tests/EditMode/Story12LobbyShellTests.cs` -- EditMode : `MatchSettings` (valeurs par defaut, mutation `Difficulty`), composants/refs serialisees de `LobbyShellScreen`/`LobbyFlowController` dans la scene, absence de `SceneManager` ou de mutation directe dans le code UI.
- [x] `Assets/RoadRage/Tests/PlayMode/Story12LobbyShellPlayModeTests.cs` -- PlayMode : clic reel Create Lobby/Join By Code/Start Game publie une notice ; changement reel de difficulte met a jour `MatchSettings` et le libelle affiche.

**Acceptance Criteria :**
- Given le menu affiche, when le joueur presse Play, then la coquille de lobby affiche Create Lobby, Join By Code (placeholder), Start Game et Back.
- Given la coquille affichee, when on inspecte `MatchSettings`, then la difficulte est lisible et modifiable via un objet local pur (pas de `ScriptableObject`, pas de `NetworkVariable`).
- Given le projet complet, when la suite EditMode s'execute, then tout passe sans regression Epic 0 / Story 1.1.
- Given le code livre, when on inspecte les assemblies, then aucune reference feature-a-feature directe n'existe (`UI` ne reference pas `Lobby` ; `App` orchestre les deux).

## Spec Change Log

- **Decision "Ask First" tranchee sans blocage (mode Auto, 2026-09-07)** — le spec listait "Dropdown TMP vs boutons cycle" comme choix visuel a valider. Retenu : bouton cycle (`Easy -> Normal -> Hard -> Easy`) plus libelle `SettingsSummaryLabel`, pour rester dans le patron des boutons existants de `MainMenuScreen` et eviter de construire a la main un template `TMP_Dropdown` dans le YAML de scene. Etat evite : une scene au YAML fragile et non verifiable par les tests de cablage existants. A rouvrir si une revue de design visuel veut le Dropdown.

- **Couverture de matrice completee apres audit (step-03, 2026-09-07)** — l'audit de la matrice I/O a trouve deux lignes non couvertes par un test qui tourne et passe : "Back" (retour au menu + conservation de l'etat de `MatchSettings`) et la moitie "difficulte par defaut visible" de "Ouverture lobby". Ajout de deux tests PlayMode (`BackReturnsToMenuAndKeepsMatchSettingsState`, `LobbyShellOpensWithDefaultDifficultyVisible`). Etat evite : un spec declare "verifie" alors que deux comportements de la matrice ne reposaient que sur une inspection manuelle. KEEP : le test Back doit continuer a cliquer le vrai `BackButton` de la scene via `onClick.Invoke()`, pas une methode privee.

- **Deviation assumee sur la tache `LobbyShellScreen` (step-04, 2026-09-07)** — la liste de taches demandait explicitement une reference serialisee `Back` et un evenement `BackRequested` sur `LobbyShellScreen`. Les trois couches de revue ont converge : rien ne s'y abonne, et le `BackButton` de la scene portait deux listeners dont un inerte. Le champ et l'evenement ont ete supprimes ; Back reste possede par `MainMenuScreen`, ce que la Code Map actait deja ("Back deja present"). Traite en `patch` plutot qu'en `bad_spec` : la correction est une suppression contenue, et re-deriver toute la story pour cela aurait coute plus de risque que de valeur. Etat evite : un faux contrat public ou retirer le cablage Story 1.1 tuerait Back sans erreur de compilation ni test rouge.

## Design Notes

Meme frontiere que Story 1.1, un feature de plus dans la composition :

```csharp
// RoadRage.Features.UI.LobbyShellScreen
public event Action CreateLobbyRequested;
public event Action<Difficulty> DifficultyChanged;

// RoadRage.App.Lobby.LobbyFlowController
screen.CreateLobbyRequested += () => notices.Publish(UnavailableOffline);
screen.DifficultyChanged += d => { settings.Difficulty = d; screen.ShowSettingsSummary(settings.Difficulty); };
```

`MatchSettings` vise a etre mappable telle quelle sur un futur `NetworkedLobbyState` (meme champ `Difficulty`) sans duplication de verite — ne pas anticiper ce type reseau maintenant (Never).

## Verification

L'Unity Editor est ouvert (`Temp/UnityLockfile` present) : verifier via Unity MCP sans fermer la session utilisateur.

**Commands :**
- `Unity_ReadConsole` (Types `Error`) apres refresh -- attendu : zero erreur de compilation.
- `Unity_ManageScene` Load `Bootstrap`, `Unity_ManageEditor` Play, clic Play puis Create Lobby/Join By Code/Start Game/difficulte, `Unity_ReadConsole` -- attendu : notices/mise a jour visibles, aucune erreur.
- `Unity_RunCommand` executant `TestRunnerApi` sur le filtre EditMode -- attendu : suite verte, `RoadRageScaffoldTests` et `Story11MainMenuLaunchTests` inclus sans regression.
- Repli si l'Editor est ferme : `& "D:\Program Files\Unity\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath "D:\Projets\RRS" -runTests -testPlatform EditMode -testResults _bmad-output/implementation-artifacts/story-1-2-editmode-results.xml` (sans `-quit`) -- attendu : XML entierement vert.

**Manual checks :**
- Play mode : Create Lobby / Join By Code / Start Game affichent chacun une notice, sans exception console.
- `git status` : seuls les fichiers des taches et leurs `.meta` apparaissent ; aucune scene `Dev_*` ni `EditorBuildSettings.asset` modifie.

### Resultats reels (execution du 2026-09-07, via Unity MCP, Editor 6000.6.0f1 reste ouvert)

- Compilation : zero erreur apres chaque changement de script/asmdef (`Unity_ReadConsole` Types `Error`).
- Suite EditMode (`TestRunnerApi` via `Unity_RunCommand`) : **21/21 verts** -- `RoadRageScaffoldTests` (7) et `Story11MainMenuLaunchTests` (8) sans regression, `Story12LobbyShellTests` (6) verts.
- Suite PlayMode (`TestRunnerApi` via `Unity_RunCommand`, meme pattern) : **9/9 verts** -- `Story11MainMenuLaunchPlayModeTests` (3) sans regression, `Story12LobbyShellPlayModeTests` (6) verts. A la difference de Story 1.1, `TestMode.PlayMode` s'est execute normalement dans cette session (pas de `testCaseCount=0`).
- Audit de couverture de la matrice I/O (demande par le coordinateur) : deux lignes n'etaient couvertes par aucun test execute. Comblees par deux tests PlayMode supplementaires, verts des la premiere execution :
  - `BackReturnsToMenuAndKeepsMatchSettingsState` (ligne "Back") : entre dans la coquille, change la difficulte pour sortir de l'etat par defaut, clique le vrai `BackButton` de la scene (`Button.onClick.Invoke()`, tous listeners reels), puis verifie que `SetupPanel` est masque, que `MenuPanel` est reaffiche, et que `LobbyFlowController.Settings.Difficulty` a conserve la valeur modifiee.
  - `LobbyShellOpensWithDefaultDifficultyVisible` (ligne "Ouverture lobby", partie "difficulte par defaut visible") : a l'ouverture de la coquille et sans aucune interaction prealable, `Settings.Difficulty` vaut `Normal` et `settingsSummaryLabel` est visible en hierarchie et affiche deja `Normal`.
- Constat de cablage releve pendant cet audit, **laisse tel quel pour la revue** (non "corrige" ici) : le meme `BackButton` de scene est reference a la fois par `MainMenuScreen.backButton` (Story 1.1) et par `LobbyShellScreen.backButton`, et personne ne s'abonne a `LobbyShellScreen.BackRequested` -- le retour reel emprunte `MainMenuScreen` -> `MainMenuFlowController` -> `ShowMenu`. Le test ci-dessus exerce le comportement reel tel qu'il est ; la question de l'evenement mort est laissee a la revue.
- Premiere execution PlayMode : 4/4 tests Story 1.2 ont echoue avec `Expected: not null` -- cause reelle : `LobbyShellScreen` vit sur `SetupPanel`, inactif tant que Play n'a pas ete presse, et `Object.FindAnyObjectByType` exclut les objets inactifs par defaut. Corrige en faisant presser Play (via `MainMenuScreen.RaisePlayRequested`) au debut de chaque test avant de chercher `LobbyShellScreen`, ce qui reproduit le vrai flux utilisateur au lieu de contourner le probleme avec une recherche incluant les objets inactifs.
- Premiere execution EditMode : 2 tests ont echoue (`Story11MainMenuLaunchTests.UiFeatureSourceNeverCallsSceneManagementOrApplicationQuit` et son equivalent Story 1.2) -- cause reelle : le commentaire XML de `LobbyShellScreen.cs` contenait litteralement la chaine `SceneManager.LoadScene` (et `MatchSettings`) que le test de garde par inspection de source detecte n'importe ou dans le fichier, y compris en commentaire. Reformule sans ces chaines litterales, suite verte ensuite.
- Verification manuelle en Play mode reelle depuis `Bootstrap` : `Unity_ManageScene` Load `Bootstrap`, `Unity_ManageEditor` Play -- scene active devient `MainMenuLobby` sans erreur. Clics reels via `Button.onClick.Invoke()` (chemin de production complet, pas de reflexion sur methodes privees) : Play affiche la coquille de lobby, Create Lobby / Join By Code / Start Game logguent chacun `[Lobby] ... demande : ...` et publient une notice, le controle de difficulte cycle `Normal -> Hard` avec libelle synchronise (`Difficulte : Hard`). Zero erreur/avertissement console sur l'ensemble de la sequence.
- `git status` apres implementation : seuls les fichiers des taches et leurs `.meta`, plus `MainMenuLobby.unity` et les deux asmdef de tests (modifications attendues) ; `sprint-status.yaml` mis a jour (`1-2-...: review`) ; aucune scene `Dev_*` ni `EditorBuildSettings.asset` modifie.
### Patchs de revue adversariale appliques (2026-09-07)

Six patchs issus de la revue a 3 couches. Comptes finaux apres application : **EditMode 21/21 verts, PlayMode 9/9 verts** (relances integrales via `TestRunnerApi`, aucun echec, aucune assertion relachee).

- **Patch 1 (gap de verification, confirme par 2 reviewers)** : les 4 tests Create Lobby / Join By Code / Start Game / difficulte passaient par `InvokePrivateMethod` sur les methodes privees `Raise*`/`CycleDifficulty`, contournant `Button.onClick` et les references serialisees -- un bouton non cable serait passe au vert. Remplaces par un vrai clic sur le bouton serialise (helper `ClickSerializedButton`, qui resout le champ serialise puis invoque `onClick`), ce qui exerce a la fois la liaison de scene et le listener d'`Awake`. `PressPlayAndEnterLobbyShell` utilise desormais lui aussi le vrai `playButton`, et `InvokePrivateMethod` a disparu du fichier. Ajout d'une assertion `SceneManager.sceneCount` dans le test Start Game (un chargement additif serait passe inapercu), et `expectedNext` derive de l'ordre reel du cycle (`ExpectedCycleOrder`) au lieu d'une arithmetique sur l'enum.
  - **Preuve empirique du patch** : la mutation exacte citee par le reviewer (retrait de `difficultyButton.onClick.AddListener(CycleDifficulty)`) a ete rejouee. Avant patch elle ne faisait echouer aucun test ; apres patch elle fait echouer **2 tests** (`DifficultyChangeUpdatesMatchSettingsAndDisplayedLabel` et `BackReturnsToMenuAndKeepsMatchSettingsState`, ce dernier dependant du clic de difficulte pour atteindre un etat non-defaut). Mutation annulee ensuite, suites relancees vertes.
- **Patch 2 (evenement mort)** : `backButton`, l'evenement `BackRequested` et `RaiseBackRequested` supprimes de `LobbyShellScreen` -- personne n'y etait abonne et le bouton portait deux listeners `onClick` dont un seul faisait reellement le retour. Back reste possede par `MainMenuScreen`, ce que la Code Map actait deja ("Back deja present"). La liaison `backButton` du composant a ete purgee de la scene par re-serialisation (verifie : une seule occurrence `backButton` subsiste dans le YAML, celle de `MainMenuScreen`) ; le GameObject `BackButton` et son cablage Story 1.1 sont intacts. `BackReturnsToMenuAndKeepsMatchSettingsState` continue de passer et clique toujours le vrai bouton, desormais resolu depuis `MainMenuScreen.backButton`.
- **Patch 3 (double ecriture d'ordre indefini)** : `LobbyShellScreen.Awake` n'ecrit plus le libelle (`ShowSettingsSummary` retire, avec commentaire expliquant pourquoi ne pas le reintroduire). La couche App est le seul ecrivain, ce qui elimine la dependance a l'ordre d'`Awake` -- d'autant que cet ecran vit sur un panneau inactif et s'eveille apres le controller. `LobbyShellOpensWithDefaultDifficultyVisible` passe toujours.
- **Patch 4 (retour joueur indifferenciable + cause erronee)** : un message distinct par action, avec sa cause reelle. Start Game n'annonce plus un echec "hors ligne" mais sa vraie cause ("aucune scene de gameplay n'existe encore a ce stade") ; Create Lobby et Join By Code nomment chacun l'action refusee et pointent l'Epic 2. `PublishUnavailableOffline()` devient `PublishUnavailable(string)`.
- **Patch 5 (faux-null Unity)** : `AssertSerializedObjectFieldsNonNull` compare desormais via l'operateur `==` surcharge d'`UnityEngine.Object` (`Assert.That(value != null, Is.True, ...)`) au lieu de `Is.Not.Null`, qui compare des references et laissait passer une reference vers un objet detruit ou manquant se comportant pourtant comme null a l'execution.
- **Patch 6 (chemins relatifs au cwd)** : chemins construits depuis `Application.dataPath` via `Path.Combine` (helper `AssetsPath`), au lieu de chemins relatifs supposant que le cwd est la racine du projet -- ce qui levait une `DirectoryNotFoundException` sous la commande de repli `-batchmode -runTests` lancee depuis un autre repertoire, au lieu d'un echec de test lisible. Le chemin de scene passe a `EditorSceneManager.OpenScene` reste volontairement relatif au projet : c'est la convention attendue par cette API, resolue par Unity independamment du cwd.

- Choix "Ask First" tranche sans blocage (mode Auto) : controle de difficulte implemente en bouton cycle (Easy -> Normal -> Hard -> Easy) plutot qu'en Dropdown TMP, pour rester dans le meme patron que les boutons existants (`MainMenuScreen`) et eviter la complexite d'un template `TMP_Dropdown` construit a la main dans le YAML de scene. Le libelle `SettingsSummaryLabel` affiche la valeur courante independamment du bouton.

## Suggested Review Order

**Couture UI/App/Lobby -- le coeur du changement**

- Point d'entree : seule couture entre l'ecran, les reglages de partie et le canal de notices.
  [`LobbyFlowController.cs:24`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L24)

- La couche App est le seul ecrivain des reglages et du libelle affiche.
  [`LobbyFlowController.cs:74`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L74)

- Un message distinct par action indisponible, avec sa vraie cause (patch de revue).
  [`LobbyFlowController.cs:84`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L84)

**Frontiere UI -- l'ecran n'emet que des intentions**

- Quatre evenements d'intention ; aucun `Back` ici, il reste possede par `MainMenuScreen`.
  [`LobbyShellScreen.cs:38`](../../Assets/RoadRage/Features/UI/LobbyShellScreen.cs#L38)

- `Awake` n'ecrit volontairement pas le libelle : evite une double ecriture d'ordre indefini.
  [`LobbyShellScreen.cs:89`](../../Assets/RoadRage/Features/UI/LobbyShellScreen.cs#L89)

- Le cycle de difficulte emet la valeur suivante sans jamais muter les reglages.
  [`LobbyShellScreen.cs:109`](../../Assets/RoadRage/Features/UI/LobbyShellScreen.cs#L109)

**Donnees de session locales**

- Objet C# pur, mappable vers un futur `NetworkedLobbyState` sans duplication de verite.
  [`MatchSettings.cs:12`](../../Assets/RoadRage/Features/Lobby/MatchSettings.cs#L12)

- Enum partage dans `Shared.Domain`, comme `PlayerMode` et `RunPhase`.
  [`Difficulty.cs`](../../Assets/RoadRage/Shared/Domain/Difficulty.cs)

**Tests -- clics reels plutot que reflexion**

- Helper de clic reel : exerce la liaison serialisee ET le listener d'`Awake`.
  [`Story12LobbyShellPlayModeTests.cs:206`](../../Assets/RoadRage/Tests/PlayMode/Story12LobbyShellPlayModeTests.cs#L206)

- Ligne de matrice "Back" : etat des reglages conserve apres retour au menu.
  [`Story12LobbyShellPlayModeTests.cs:146`](../../Assets/RoadRage/Tests/PlayMode/Story12LobbyShellPlayModeTests.cs#L146)

- Ligne de matrice "Ouverture lobby" : difficulte par defaut affichee sans interaction.
  [`Story12LobbyShellPlayModeTests.cs:122`](../../Assets/RoadRage/Tests/PlayMode/Story12LobbyShellPlayModeTests.cs#L122)

- Assertion de cablage passant par l'operateur `==` d'Unity, pas le `Is.Not.Null` de NUnit.
  [`Story12LobbyShellTests.cs:139`](../../Assets/RoadRage/Tests/EditMode/Story12LobbyShellTests.cs#L139)
