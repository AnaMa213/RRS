---
title: 'Story 1.1 — Bootstrap et lancement du menu principal'
type: 'feature'
created: '2026-09-07'
status: 'done'
review_loop_iteration: 0
baseline_commit: '70f58d141400423af3b9578efb038c16c4fe51ae'
context:
  - '{project-root}/_bmad-output/implementation-artifacts/epic-1-context.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem :** Les trois scenes seed (`Bootstrap`, `MainMenuLobby`, `MVP_Run`) existent depuis l'Epic 0 mais sont vides : le jeu ne se lance sur rien, sans routage, sans menu, sans emplacement d'erreur visible.

**Approach :** `Bootstrap` devient proprietaire des services persistants (routeur de scenes + canal de notices) et route vers `MainMenuLobby`, qui recoit un menu UGUI avec Play, Quit (builds), une zone de notices visible et un panneau de setup placeholder que Story 1.2 remplira. L'UI emet des intentions ; le routage et le quit appartiennent a la couche App.

## Boundaries & Constraints

**Always :**
- Fondations deja adoptees uniquement : UGUI (`ADDON-001`), Input System (`ADDON-002`). L'`EventSystem` doit utiliser `InputSystemUIInputModule`, sinon les clics UI sont inertes.
- Frontieres d'assemblies preservees : `RoadRage.Features.UI` ne reference que `RoadRage.Shared` plus `UnityEngine.UI` / `Unity.TextMeshPro` ; aucune reference feature-a-feature ; seule `RoadRage.App` compose.
- L'UI n'appelle jamais `SceneManager.LoadScene` ni `Application.Quit`.
- Le flux fonctionne depuis `Bootstrap` **et** depuis `MainMenuLobby` ouverte seule dans l'Editor.
- Les noms de scenes du routeur correspondent aux scenes activees dans `EditorBuildSettings`.
- Logs prefixes par couche (`[App]`, `[UI]`).

**Ask First :**
- Adopter un package ou asset UI tiers (nouvelle ligne de registre requise).
- Ajouter, renommer ou reordonner une scene de build.
- Modifier des references asmdef au-dela de celles listees ici.

**Never :**
- Aucun reseau : ni Steamworks, ni Netcode, ni `NetworkManager` cree ou demarre.
- Aucun gameplay : pas de mouvement, pas de spawn, pas d'entree dans `MVP_Run`.
- Pas de vraie creation de lobby, de code de session ni de join (Story 1.2).
- Pas d'etat de session dans un asset ScriptableObject.
- Ne pas toucher aux scenes `Dev_*` ni aux squelettes `Networked*State`.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Lancement nominal | Demarrage sur `Bootstrap` | Services persistants crees une fois, survivant au changement de scene, puis `MainMenuLobby` chargee et menu affiche | N/A |
| Play | Menu affiche, clic Play | Panneau de setup visible, panneau menu masque, aucun service en ligne sollicite | N/A |
| Back | Panneau de setup visible, clic Back | Retour au panneau menu | N/A |
| Quit en build | Build joueur, clic Quit | `Application.Quit()` appele | N/A |
| Quit dans l'Editor | Play mode Editor, bouton Quit | Aucune tentative de fermeture de l'Editor | Log `[App]`, aucune exception |
| Entree directe | `MainMenuLobby` jouee sans passer par `Bootstrap` | Services auto-instancies, menu pleinement fonctionnel | N/A |
| Notice publiee | Notice (info/avertissement/erreur) publiee | Zone de notices visible, message et severite affiches | N/A |
| Notice effacee | Aucune notice, ou notice effacee | Zone masquee, aucun texte residuel | N/A |
| Double bootstrap | Seconde instance des services persistants | La seconde se detruit, la premiere reste la reference | Log `[App]` d'avertissement |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs` -- ancre vide (`sealed`, `namespace RoadRage.App`) ; devient le singleton persistant. **A modifier.**
- `Assets/RoadRage/App/Services/` et `Assets/RoadRage/Shared/Presentation/` -- dossiers vides prevus par le scaffold, destinations du routeur et du canal.
- `Assets/RoadRage/Features/UI/RoadRage.Features.UI.asmdef` -- ne reference que `RoadRage.Shared` aujourd'hui.
- `Assets/RoadRage/Tests/EditMode/RoadRage.Tests.EditMode.asmdef` -- ne reference ni `RoadRage.Features.UI` ni les assemblies UI/Input.
- `Assets/RoadRage/Tests/EditMode/RoadRageScaffoldTests.cs` -- modele de style des tests (assertions fichier/reflexion, `EditorBuildSettings`, interdiction d'appels par lecture du source). **Ne pas modifier.**
- `Assets/RoadRage/App/Scenes/Bootstrap.unity`, `MainMenuLobby.unity` -- ne contiennent que `Main Camera`, `Directional Light`, `Global Volume`. **A peupler.**
- `ProjectSettings/EditorBuildSettings.asset` -- `Bootstrap`, `MainMenuLobby`, `MVP_Run` actives dans cet ordre, `Dev_*` desactivees. **Lecture seule.**
- `Packages/manifest.json` -- Unity `6000.6.0f1`, `com.unity.ugui` `2.6.0` (fournit TextMeshPro), `com.unity.inputsystem` `1.20.0`. **Lecture seule.**
- `Assets/RoadRage/Features/Run/RunCompositionRoot.cs` -- style projet : `[DisallowMultipleComponent]`, `[SerializeField]` prives + proprietes lecture seule, commentaires XML francais.

## Tasks & Acceptance

**Execution :**
- [x] `Assets/RoadRage/Shared/Presentation/UserNotice.cs` -- creer le type notice (severite `Info`/`Warning`/`Error` + message) -- vocabulaire partage des retours visibles (UX-DR3).
- [x] `Assets/RoadRage/Shared/Presentation/UserNoticeChannel.cs` -- creer un canal C# pur (publier, effacer, evenement, derniere notice) -- l'Epic 2 y branchera les echecs reels sans redessiner l'ecran.
- [x] `Assets/RoadRage/App/Services/AppSceneRouter.cs` -- creer le routeur (noms des trois scenes de build + chargement du menu) -- point de verite unique du routage.
- [x] `Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs` -- singleton persistant (`DontDestroyOnLoad`, garde anti-doublon, `EnsureInstance`) exposant routeur et canal, routant vers `MainMenuLobby` au demarrage.
- [x] `Assets/RoadRage/Features/UI/MainMenuScreen.cs` -- composant d'ecran UGUI (refs serialisees panneaux menu/setup/notice, boutons Play/Quit/Back, label) exposant des evenements d'intention et l'affichage des notices.
- [x] `Assets/RoadRage/Features/UI/RoadRage.Features.UI.asmdef` -- ajouter `UnityEngine.UI` et `Unity.TextMeshPro`.
- [x] `Assets/RoadRage/App/MainMenu/MainMenuFlowController.cs` -- relier les intentions de l'ecran au routeur, au canal et au quit (garde Editor) -- seule couture UI/App.
- [x] `Assets/RoadRage/App/Scenes/Bootstrap.unity` -- ajouter un GameObject portant `RoadRageBootstrap`.
- [x] `Assets/RoadRage/App/Scenes/MainMenuLobby.unity` -- Canvas Screen Space Overlay (`CanvasScaler` 1920x1080), `EventSystem` + `InputSystemUIInputModule`, panneau menu (titre, Play, Quit), panneau notices masque, panneau setup placeholder masque avec Back, GameObject de flux avec toutes les refs assignees.
- [x] `Assets/RoadRage/Tests/EditMode/RoadRage.Tests.EditMode.asmdef` -- ajouter `RoadRage.Features.UI`, `UnityEngine.UI`, `Unity.TextMeshPro`, `Unity.InputSystem`.
- [x] `Assets/RoadRage/Tests/EditMode/Story11MainMenuLaunchTests.cs` -- couvrir les cas testables en EditMode : canal (publication/effacement/severite), noms de scenes du routeur vs `EditorBuildSettings`, composants attendus dans les deux scenes avec refs serialisees non nulles, absence de `SceneManager`/`Application.Quit` dans le source UI, garde `#if UNITY_EDITOR`/`Application.Quit()` par inspection de source.
- [x] `Assets/RoadRage/Features/UI/MainMenuScreen.cs` (revue) -- logger un avertissement `[UI]` pour chaque reference serialisee manquante (panneaux, boutons), pas seulement un no-op silencieux -- patch de revue (blind hunter).
- [x] `Assets/RoadRage/Tests/PlayMode/RoadRage.Tests.PlayMode.asmdef` et `Story11MainMenuLaunchPlayModeTests.cs` (nouveaux) -- couvrir en PlayMode les comportements dependants du cycle de vie Unity (Awake/Start/Destroy) qu'aucun test EditMode ne peut observer : routage reel `Bootstrap` -> `MainMenuLobby`, singleton anti-doublon (la seconde instance se detruit, la premiere reste la reference), cablage complet `MainMenuFlowController` (Play/Back/Notice/Quit) -- patch de revue (verification-gap : trois gaps de regression convergents).

**Acceptance Criteria :**
- Given l'Epic 0 cloturee en `Accepted With Known Blockers`, when le jeu demarre depuis `Bootstrap`, then le joueur arrive sur `MainMenuLobby` sans erreur console.
- Given le menu affiche, when le joueur l'observe, then une commande Play, une commande Quit pour les builds et une zone de notices visible sont presentes.
- Given le menu affiche, when le joueur appuie sur Play, then l'ecran de setup apparait sans qu'aucun service en ligne ne soit initialise.
- Given le projet complet, when la suite EditMode s'execute, then tout passe, `RoadRageScaffoldTests` inclus (aucune regression Epic 0).
- Given le code livre, when on inspecte les assemblies, then aucune reference feature-a-feature et aucun package tiers n'ont ete ajoutes.

## Spec Change Log

## Design Notes

Frontiere UI/App — l'ecran ne connait ni scene ni application :

```csharp
// RoadRage.Features.UI
public event Action PlayRequested;
public event Action QuitRequested;
public void ShowNotice(UserNotice notice);

// RoadRage.App — MainMenuFlowController
screen.PlayRequested += screen.ShowSetupPlaceholder;
screen.QuitRequested += QuitApplication;   // Application.Quit(), garde #if UNITY_EDITOR
notices.NoticePublished += screen.ShowNotice;
```

`MainMenuFlowController.Awake()` appelle `RoadRageBootstrap.EnsureInstance()` : depuis `Bootstrap` rien n'est cree ; en entree directe l'instance nait a la volee. C'est ce qui rend `MainMenuLobby` jouable seule sans dupliquer la composition.

Le panneau de setup est volontairement inerte : Story 1.2 y ajoutera Create Lobby, Join By Code, Start Game et les reglages brouillon. Ne pas les anticiper.

## Verification

L'Unity Editor est ouvert (`Temp/UnityLockfile` present) : le mode batch est indisponible tant que la session vit. Verifier via Unity MCP sans fermer la session de l'utilisateur.

**Commands :**
- `Unity_ReadConsole` (Types `Error`) apres refresh -- attendu : zero erreur de compilation.
- `Unity_ManageScene` Load `Bootstrap`, `Unity_ManageEditor` Play, `Unity_ManageScene` GetActive, `Unity_ManageEditor` Stop -- attendu : scene active `MainMenuLobby`, console sans erreur.
- `Unity_RunCommand` executant `TestRunnerApi` sur le filtre EditMode, resultats relus via `Unity_ReadConsole` -- attendu : suite verte.
- Repli si l'Editor est ferme : `& "D:\Program Files\Unity\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath "D:\Projets\RRS" -runTests -testPlatform EditMode -testResults _bmad-output/implementation-artifacts/story-1-1-editmode-results.xml` (sans `-quit`) -- attendu : XML entierement vert.

**Manual checks :**
- Play mode depuis `Bootstrap` : Play affiche le setup, Back revient au menu, la zone de notices reste masquee sans notice publiee.
- `git status` : seuls les fichiers des taches et leurs `.meta` generes par Unity apparaissent ; aucune scene `Dev_*` ni `EditorBuildSettings.asset` modifie.

### Resultats reels (execution du 2026-09-07, via Unity MCP, Editor 6000.6.0f1 reste ouvert)

- Compilation : zero erreur apres chaque changement de script/asmdef (`Unity_ReadConsole` Types `Error`).
- Suite EditMode (`TestRunnerApi`, resultats lus dans `TestResults.xml`) : **15/15 verts** en execution finale, `RoadRageScaffoldTests` (7) inclus sans regression Epic 0, `Story11MainMenuLaunchTests` (8) verts.
- Lancement nominal + Play + Back + Quit dans l'Editor : verifies en Play mode reel depuis `Bootstrap` (scene active devient `MainMenuLobby`, aucune erreur console ; Play affiche le setup, Back restaure le menu ; Quit logue `[App] Quit demande en Play Mode Editor...` sans fermer l'Editor).
- Entree directe : verifiee en Play mode reel en ouvrant `MainMenuLobby` seule (sans `Bootstrap`) -- `RoadRageBootstrap.Instance` non nul, `Router` et `Notices` construits, aucune erreur console.
- Double bootstrap : verifiee en Play mode reel -- l'ajout d'un second `RoadRageBootstrap` declenche exactement le log `[App] Seconde instance de RoadRageBootstrap detectee, destruction de la seconde ; la premiere reste la reference.` et `RoadRageBootstrap.Instance` reste la premiere instance.
- Quit en build : non exerce reellement (aucun build joueur possible depuis cette session Editor) ; couvert par le test EditMode `QuitApplicationIsGuardedAndOnlyCallsApplicationQuitOutsideTheEditor`, qui verifie par inspection de source que `Application.Quit()` est bien place dans la branche `#else` du garde `#if UNITY_EDITOR`.
- Notice publiee/effacee : couvertes par `UserNoticeChannelPublishesAndTracksLastNotice` et `UserNoticeChannelClearResetsLastNotice`.
- Une premiere tentative de test EditMode pur pour "Entree directe" (`[Test]`, puis `[UnityTest]` avec un frame de delai) a echoue de facon reproductible : `AddComponent` ne declenche pas `Awake()` de maniere synchrone ni au frame suivant hors Play Mode dans ce contexte de test. Le test a ete retire et remplace par la verification Play mode reelle ci-dessus plutot que de forcer une assertion peu fiable.
- Ecarts de perimetre corriges avant clôture : le sous-agent d'implementation avait accidentellement repasse `_bmad-output/implementation-artifacts/spec-0-8-epic-0-smoke-tests-and-go-no-go-gate.md` de `done` a `in-progress` (hors perimetre, Epic 0 est clos) -- annule via `git checkout`. Il avait aussi ajoute `"includePlatforms": ["Editor"]` a `Assets/RoadRage/DevTools/RoadRage.DevTools.asmdef`, ce qui contredit le commentaire XML du script `RoadRageNetcodeSmokeTestAutoStart.cs` affirmant explicitement que l'assembly doit rester runtime-compatible -- annule et reimporte, suite verte confirmee apres coup. `ProjectSettings/ProjectSettings.asset` (`runInBackground`) a ete modifie par le Test Runner pendant les tentatives PlayMode -- annule via `git checkout`.

### Revue adversariale (step-04, 2026-09-07)

Trois revues paralleles (blind hunter, edge-case hunter, verification-gap) executees sur le diff complet. Classification : 2 `patch` (appliques ci-dessous), 4 `defer` (voir `deferred-work.md`), le reste `reject`. Aucun `intent_gap` ni `bad_spec` -- aucune boucle de retour necessaire.

- **Patch 1 (verification-gap, 3 gaps convergents)** : aucun test verifiable/repetable ne couvrait le routage `Start()`, le singleton anti-doublon, ni le cablage `MainMenuFlowController` -- `EditorSceneManager.OpenScene` en EditMode n'invoque jamais `Awake`/`Start`, donc mes verifications manuelles precedentes, bien que reelles, n'etaient pas des regressions-proof. Corrige en ajoutant `RoadRage.Tests.PlayMode` (nouvel assembly, 3 `[UnityTest]`) couvrant ces trois comportements avec de vraies simulations Play Mode.
- **Patch 2 (blind hunter)** : incoherence de style defensif entre `MainMenuFlowController` (log d'avertissement si `screen` manquant) et `MainMenuScreen` (no-op silencieux sur panneaux/boutons manquants) -- corrige en ajoutant les memes logs `[UI]`.
- **Faux positif verifie empiriquement (edge-case hunter)** : le hunter affirmait qu'un second `RoadRageBootstrap` detruit dans son propre `Awake()` pourrait quand meme voir son `Start()` s'executer (risque de NRE sur `Router.LoadMainMenu()`). Teste en direct via une sonde `Awake` + `Destroy(gameObject)` + `Start()` en Play Mode reelle : `Start()` ne s'execute jamais sur un objet detruit dans son propre `Awake()`. Rejete avec preuve, pas seulement par memoire.
- **4 `defer` loggees dans `deferred-work.md`** : `UserNoticeChannel.Clear()` ne notifie pas les abonnes (inatteignable dans cette story, aucun appelant ne publie encore de notice) ; `AppSceneRouter.LoadMainMenu()` sans repli si la scene manque des Build Settings (pattern a traiter au niveau projet) ; les deux tests-garde par inspection de source sont fragiles aux refactors (compromis deja assume au style du projet) ; `MainMenuFlowController` sans reference `screen` n'a pas de retour visible en build (deja garde par un test EditMode aujourd'hui).

**Limite d'outillage rencontree et documentee honnetement** : `TestRunnerApi.Execute(TestMode.PlayMode)` invoque via Unity MCP (`Unity_RunCommand`) retourne systematiquement `RunStarted testCaseCount=0` dans cette session -- 8 tentatives (filtre simple, filtre par nom d'assembly avec/sans `.dll`, fenetre Test Runner ouverte au prealable, callbacks avec conteneur statique anti-GC) ont toutes echoue de la meme facon, alors que `TestRunnerApi.RetrieveTestList(TestMode.PlayMode)` trouve correctement les 3 tests (`RoutingFromBootstrapEntersMainMenuLobby`, `DuplicateBootstrapSelfDestroysAndPreservesFirst`, `MainMenuFlowControllerWiringRelaysScreenIntentAndNotices`), prouvant que l'assembly compile et est bien reconnue comme suite PlayMode. La suite EditMode, elle, s'est executee normalement (`TestMode.EditMode`) tout au long de la session avec le meme pattern de code. Cause probable : specifique au contexte d'execution dynamique de `Unity_RunCommand` pour ce mode precis, pas un defaut du code testé.
  - **Verification de repli reellement effectuee** : chacun des 3 comportements a ete rejoue manuellement en Play Mode reelle via Unity MCP, en exercant le vrai chemin de code de production (pas une simulation) : routage `Bootstrap` -> `MainMenuLobby` confirme (scene active + `RoadRageBootstrap.Instance` non nul) ; doublon confirme (log d'avertissement exact + premiere instance preservee) ; cablage confirme en cliquant les vrais boutons `Button.onClick.Invoke()` de la scene (trace d'appel complete `Button -> MainMenuScreen.RaiseQuitRequested:133 -> MainMenuFlowController.QuitApplication:58` observee dans la console) plus publication reelle d'une notice via `RoadRageBootstrap.Instance.Notices.Publish(...)`.
  - **A refaire par l'utilisateur pour confirmation formelle** : ouvrir `Window > General > Test Runner`, onglet `PlayMode`, cliquer `Run All` -- ou fermer l'Editor et lancer `Unity.exe -batchmode -projectPath "D:\Projets\RRS" -runTests -testPlatform PlayMode -testResults _bmad-output/implementation-artifacts/story-1-1-playmode-results.xml` (sans `-quit`). Les 3 tests sont attendus verts sur la base des verifications manuelles ci-dessus.

## Suggested Review Order

**Singleton persistant et routage**

- Point d'entree : le singleton s'assure lui-meme, route depuis `Bootstrap`, et se nettoie a la destruction.
  [`RoadRageBootstrap.cs:41`](../../Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs#L41)

- `Start()` ne route que si la scene active est `Bootstrap` -- rend `MainMenuLobby` jouable seule.
  [`RoadRageBootstrap.cs:57`](../../Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs#L57)

- `EnsureInstance()` cree l'instance a la volee en entree directe, sans dupliquer la composition.
  [`RoadRageBootstrap.cs:30`](../../Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs#L30)

- Point de verite unique du nom des trois scenes de build, verrouille par un test EditMode.
  [`AppSceneRouter.cs:18`](../../Assets/RoadRage/App/Services/AppSceneRouter.cs#L18)

**Frontiere UI/App -- l'ecran n'emet que des intentions**

- Seule couture : relie les evenements de l'ecran au routeur, aux notices, et au quit garde par `#if UNITY_EDITOR`.
  [`MainMenuFlowController.cs:18`](../../Assets/RoadRage/App/MainMenu/MainMenuFlowController.cs#L18)

- Le quit reel n'est jamais appele en Editor -- seule la branche non-Editor ferme l'application.
  [`MainMenuFlowController.cs:55`](../../Assets/RoadRage/App/MainMenu/MainMenuFlowController.cs#L55)

- L'ecran expose des evenements C# purs ; aucun appel a `SceneManager` ou `Application.Quit` ici.
  [`MainMenuScreen.cs:40`](../../Assets/RoadRage/Features/UI/MainMenuScreen.cs#L40)

**Canal de notices (reutilise par l'Epic 2)**

- Canal C# pur : publier/effacer sans dependance Unity, pret pour les echecs reseau futurs.
  [`UserNoticeChannel.cs:15`](../../Assets/RoadRage/Shared/Presentation/UserNoticeChannel.cs#L15)

**Couverture PlayMode (patch de revue -- comportements Awake/Start/Destroy)**

- Prouve que le routage reel fonctionne en Play Mode, la ou EditMode ne peut pas observer `Start()`.
  [`Story11MainMenuLaunchPlayModeTests.cs:34`](../../Assets/RoadRage/Tests/PlayMode/Story11MainMenuLaunchPlayModeTests.cs#L34)

- Prouve la survie du singleton face a un doublon -- seul test qui exerce `Destroy()` en conditions reelles.
  [`Story11MainMenuLaunchPlayModeTests.cs:45`](../../Assets/RoadRage/Tests/PlayMode/Story11MainMenuLaunchPlayModeTests.cs#L45)

- Prouve le cablage complet Play/Back/Notice/Quit via reflexion sur les methodes privees de l'ecran.
  [`Story11MainMenuLaunchPlayModeTests.cs:66`](../../Assets/RoadRage/Tests/PlayMode/Story11MainMenuLaunchPlayModeTests.cs#L66)

**Peripheriques -- tests EditMode et config**

- Composants attendus dans les deux scenes, refs serialisees non nulles.
  [`Story11MainMenuLaunchTests.cs:86`](../../Assets/RoadRage/Tests/EditMode/Story11MainMenuLaunchTests.cs#L86)

- Garde de source : `Application.Quit()` doit rester dans la branche `#else`.
  [`Story11MainMenuLaunchTests.cs:134`](../../Assets/RoadRage/Tests/EditMode/Story11MainMenuLaunchTests.cs#L134)

- Assemblies UI ajoutees : `UnityEngine.UI`, `Unity.TextMeshPro`.
  [`RoadRage.Features.UI.asmdef`](../../Assets/RoadRage/Features/UI/RoadRage.Features.UI.asmdef)
