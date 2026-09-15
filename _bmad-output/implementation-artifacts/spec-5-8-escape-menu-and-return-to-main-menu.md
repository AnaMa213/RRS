---
title: "Story 5.8 : menu d'echappement et retour au menu principal"
type: "feature"
created: "2026-09-15"
status: "done"
review_loop_iteration: 0
baseline_commit: "288a5a133a609ecd75ab45e746d7aa7693a4c74f"
context:
  ["{project-root}/_bmad-output/implementation-artifacts/epic-5-context.md"]
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** il n'existe aucune sortie volontaire depuis `MVP_Run`. Le seul retour au menu est la detection d'une session perdue par `NetworkedRunSessionMonitor`; un joueur qui veut quitter une run doit tuer le process Unity, et aucun ecran d'intention n'existe cote run (le HUD est purement push, sans bouton).

**Approach:** un ecran d'echappement UGUI (`Resume`, `Quit to Main Menu`) ouvert par Echap, qui ne met jamais la simulation en pause, libere puis restaure le curseur, et bloque les entrees locales tant qu'il est ouvert. Le quit passe par le teardown de session existant (Story 2.7), augmente d'un seul point d'entree volontaire. Un drapeau de sortie volontaire empeche le client de se faire re-embarquer par le lobby apres son retour au menu.

## Boundaries & Constraints

**Always:**

- Une seule implementation du teardown : `NetworkedRunSessionMonitor` reste l'unique proprietaire de `Shutdown()` + `LoadMainMenu()`; le quit volontaire l'appelle, il ne le reecrit pas.
- L'ecran vit dans `RoadRage.Features.UI` et n'emet que des intentions (`ResumeRequested`, `QuitToMainMenuRequested`) : jamais `SceneManager.LoadScene`, `Application.Quit` ni `MatchSettings`, et aucune reference a `RoadRage.App` (gardes `Story11MainMenuLaunchTests.cs:120-134`, `Story12LobbyShellTests.cs:98-111`).
- La touche Echap se lit dans `RoadRage.App` : `Features.UI` ne reference pas `Unity.InputSystem` (`RoadRage.Features.UI.asmdef`).
- `Time.timeScale` n'est jamais touche : une session hebergee continue de simuler quand un joueur ouvre son menu.
- Le menu ne mute aucune `NetworkVariable` et n'emet aucune intention reseau : tout etat partage reste host-authoritative.

**Ask First:**

- Introduire `LocalInputGate` (statique, `RoadRage.Shared`) comme point de blocage unique des lecteurs d'entree disperses dans quatre features. Si ce partage d'etat statique est juge contraire aux conventions d'architecture, s'arreter et demander avant de l'introduire.

**Never:**

- Ne pas fermer la room Steam au depart de l'hote, ni ajouter un leave invite dans `Features/Online` (hors scope Story 2.3), ni de migration d'hote.
- Pas d'ecran de pause general, de menu de parametres ni de dialogue de confirmation : Echap, Resume, Quit to Main Menu suffisent.
- Ne pas modifier `RunCheckpointHudScreen` : le nouveau panneau est un enfant frere du HUD sous le meme Canvas, jamais une extension de l'ecran existant.
- Ne pas casser l'auto-rejoindre legitime de la Story 5.3 pour un joueur qui n'a jamais quitte.

## I/O & Edge-Case Matrix

| Scenario                    | Input / State                | Expected Output / Behavior                                                              | Error Handling             |
| --------------------------- | ---------------------------- | --------------------------------------------------------------------------------------- | -------------------------- |
| Ouvrir                      | Echap, menu ferme            | panneau visible, entrees locales bloquees, curseur libere et visible                    | N/A                        |
| Fermer                      | Echap ou Resume, menu ouvert | panneau masque, entrees rendues, curseur restaure a l'etat capture a l'ouverture        | N/A                        |
| Echap sans clavier          | `Keyboard.current == null`   | aucun changement, aucune exception                                                      | garde de nullite           |
| Quitter en hote             | Quit to Main Menu            | retour `MainMenuLobby` sans notice d'erreur; clients restants traites par Story 2.7     | N/A                        |
| Quitter en client           | Quit to Main Menu            | retour `MainMenuLobby`; hote et autres clients continuent; aucune re-entree automatique | N/A                        |
| Quitter deux fois           | quit deja en cours           | second appel ignore                                                                     | garde `isReturningToLobby` |
| Scene dechargee menu ouvert | teardown                     | entrees rendues et curseur restaure                                                     | `OnDestroy`                |

</frozen-after-approval>

## Code Map

**Creer**

- `Shared/Input/LocalInputGate.cs` -- portail statique (bloque/rend, remise a zero), seul point de blocage des entrees locales.
- `Features/UI/RunEscapeMenuScreen.cs` -- ecran d'intention porte par son propre panneau. Modele : `MainMenuScreen.cs` (evenements `:52-59`, raiseurs `:209-232`, `SetPanelActive` `:234-239`) et `RunCheckpointHudScreen.cs:259-275` (toggle de panneau).
- `App/Run/RunEscapeMenuFlowController.cs` -- Echap, curseur, portail, routage du quit. Objet de scene sur `RunRoot`, comme `RunFlowController` (`MVP_Run.unity:4001`).

**Modifier**

- `App/Run/NetworkedRunSessionMonitor.cs` -- `ReturnClientToLobby` (`:84-111`) prend une notice optionnelle; les declencheurs perdus (`:45`, `:56`) gardent la notice d'erreur; ajouter l'entree publique volontaire idempotente.
- `App/Bootstrap/RoadRageBootstrap.cs` -- drapeau `SessionExitRequested` + marquage/levee, a cote des services (`:60-95`).
- `App/Lobby/LobbyFlowController.cs` -- `Update()` (`:230-248`) consulte le drapeau; creation, join et Start Game le levent.
- Lecteurs d'entree, un garde chacun : `Features/Vehicles/NetworkedVehicleDriverController.cs:168-195`, `Features/OnFoot/LocalOnFootController.cs:116-224`, `Features/Vehicles/NetworkedVehicleSeatIntent.cs:49-54`, `Features/OnFoot/NetworkedPlayerLifecycleIntent.cs:48-52`, `Features/OnFoot/NetworkedPlayerReviveIntent.cs:43-47`, `Features/Vehicles/NetworkedVehicleRecoveryIntent.cs:44-48`, `Features/Vehicles/LocalVehicleCameraRig.cs:60-64` (orbite), `App/Run/RunFlowController.cs:425-460,1172-1196` (touches dev/action).
- `App/Scenes/MVP_Run.unity` -- panneau `EscapeMenuPanel` dernier enfant de `RunCheckpointHud` (`:4659`), portant `RunEscapeMenuScreen` et ses boutons, plus un `EventSystem` et un `InputSystemUIInputModule` (absents aujourd'hui, requis pour cliquer).

**Verification existante a imiter**

- Gardes de source : `Story57AiTrafficClientPresentationTests.cs:289` (`CodeOnly`), ouverture de scene `:224-233`.
- PlayMode : `Story43PassengerActionOneMvpRunPlayModeTests.cs:19-20` (chargement direct), teardown obligatoire `Story57AiTrafficClientPresentationPlayModeTests.cs:52-55`.

## Tasks & Acceptance

**Execution:**

- [x] `Shared/Input/LocalInputGate.cs` -- creer le portail statique -- bloque en un point des lecteurs repartis dans quatre assemblies de feature.
- [x] `App/Bootstrap/RoadRageBootstrap.cs` -- drapeau de sortie volontaire -- survit au changement de scene et reste lisible par le lobby.
- [x] `App/Run/NetworkedRunSessionMonitor.cs` -- notice parametree + entree volontaire -- un seul teardown, deux declencheurs.
- [x] `App/Lobby/LobbyFlowController.cs` -- consulter et lever le drapeau -- empeche la re-entree automatique sans casser le join de la Story 5.3.
- [x] `Features/UI/RunEscapeMenuScreen.cs` -- ecran d'intention + afficher/masquer -- respecte les gardes UI (aucun routage, aucune donnee de lobby).
- [x] `App/Run/RunEscapeMenuFlowController.cs` -- Echap, curseur, portail, quit -- proprietaire du menu cote App; expose ouvrir/fermer/basculer pour les tests PlayMode.
- [x] Lecteurs d'entree locaux -- garde de blocage -- entrees de conduite et d'action supprimees pendant l'ouverture.
- [x] `App/Scenes/MVP_Run.unity` -- panneau et `EventSystem` -- le menu doit etre cliquable et au-dessus de l'overlay de mort.
- [x] `Tests/EditMode/Story58EscapeMenuTests.cs` -- gardes de source, portail, drapeau, scene -- verifie sans PlayMode.
- [x] `Tests/PlayMode/Story58EscapeMenuPlayModeTests.cs` -- ouverture/fermeture, blocage, retour au menu -- verifie le chemin reel.
- [x] `docs/setup/story-5-8-escape-menu-notes.md` -- note de verification -- preuve et ecarts.

**Acceptance Criteria:**

- Given un joueur dans `MVP_Run`, when il presse Echap, then le menu apparait avec Resume et Quit to Main Menu, les entrees de conduite et d'action (y compris le deplacement a pied) sont bloquees, et le curseur est libere et visible.
- Given le menu ouvert, when le joueur presse Echap a nouveau ou choisit Resume, then le menu se ferme, les entrees sont rendues et le curseur retrouve l'etat capture a l'ouverture.
- Given une session hebergee avec plusieurs joueurs, when un joueur ouvre le menu, then la simulation host-authoritative continue (`Time.timeScale` inchange) et aucun `NetworkVariable` n'est modifie.
- Given le menu ouvert, when le joueur choisit Quit to Main Menu, then il revient a `MainMenuLobby` par le chemin de sortie de session existant, sans second teardown.
- Given un hote qui quitte, then les clients restants sont traites exactement comme le cas host-quit de la Story 2.7; given un client qui quitte, then l'hote et les autres clients continuent la run et le client n'y est pas re-embarque automatiquement.
- Given un retour au menu effectue, when le joueur relance ou rejoint explicitement une partie, then l'auto-rejoindre legitime de la Story 5.3 fonctionne de nouveau.

## Spec Change Log

## Design Notes

- **Pourquoi un portail statique.** Les entrees locales sont lues par huit lecteurs dans quatre assemblies de feature, aucun n'ayant le droit de referencer `RoadRage.App`; chaque feature reference `RoadRage.Shared`. Un portail partage evite huit plomberies d'activation distinctes et reste testable en EditMode sans scene. Il doit etre remis a zero par `RunEscapeMenuFlowController.OnDestroy` : une scene dechargee menu ouvert ne doit pas laisser le jeu muet.
- **Notice parametree plutot que chemin parallele.** Le quit volontaire n'est pas une erreur : il ne publie donc aucune notice, alors que la perte de session conserve `HostDisconnectedMessage`. Parametrer la coroutine existante conserve un seul `Shutdown()` + `LoadMainMenu()`.
- **Drapeau plutot qu'un leave Steam.** Un client qui quitte garde `LobbyJoinStatus.Joined`, donc `LobbyFlowController.Update()` (`:230-248`) le relancerait immediatement dans la run. Le drapeau tranche ce cas sans ouvrir un second chemin de sortie de lobby (leave invite explicitement hors scope Story 2.3).
- **Resolution paresseuse du moniteur.** `NetworkedRunSessionMonitor` est ajoute par `RunFlowController.Start()` (`EnsureNetworkSessionMonitor` `:1399-1411`) : le resoudre au moment du quit, jamais en `Awake`, pour ne pas dependre de l'ordre des composants.
- **Garde de la Story 2.7 preservee.** `Story27PlayerLifecycleTests.LocalOnFootControllerUpdateGuardsOnMovementEnabledBeforeReadingInputOrGravity` epingle la forme litterale `if (!MovementEnabled)` de `LocalOnFootController.Update()`. Le blocage du menu est donc une branche distincte placee AVANT ce garde-fou, plutot qu'une condition ajoutee a sa suite : le contrat de la Story 2.7 reste vrai au caractere pres, et la duplication d'un appel `UpdateStamina` est le prix de cette preservation.
- **Deux corrections de tests pendant la verification.** (1) L'assertion d'idempotence du test de curseur EditMode exigeait qu'un second `Open()` ramene le curseur a `None` : c'est faux et hors matrice -- un `Open()` deja ouvert ne retouche pas le curseur vivant. L'assertion porte desormais sur l'etat CAPTURE, ce qui est la vraie propriete, et la restauration a l'etat de la premiere ouverture reste verifiee par le `Close()` qui suit. (2) Le test de teardown ne peut pas exister en EditMode : Unity n'appelle ni `Awake` ni `OnDestroy` pour un composant ajoute a la volee hors Play Mode. Il est donc devenu une garde de source en EditMode, et son execution reelle est verifiee en PlayMode par `TearingDownTheOwnerWithTheMenuOpenReleasesInputAndRestoresTheCursor`.

## Verification

**Commands:**

- Aucune commande CLI : verification par le Test Runner Unity, checkpoint humain comme aux Stories 5.5 a 5.7.

**Manual checks (if no CLI):**

- Filtre `Story58EscapeMenuTests` dans `RoadRage.Tests.EditMode`, puis suite complete verte.
- Filtre `Story58EscapeMenuPlayModeTests` dans `RoadRage.Tests.PlayMode`.
- `MVP_Run` en Play Mode : Echap ouvre et ferme le menu, le curseur est libere puis restaure, la voiture ne repond plus pendant l'ouverture, la simulation continue.
- Session a deux pairs : un client quitte au menu, l'hote et l'autre client continuent, le client revenu ne repart pas tout seul dans la run; puis reboot de l'hote et controle du cas host-quit de la Story 2.7.

## Suggested Review Order

**Un seul teardown, deux declencheurs**

- L'entree volontaire idempotente : pose le drapeau, ne publie aucune notice, ne reecrit rien.
  [`NetworkedRunSessionMonitor.cs:80`](../../Assets/RoadRage/App/Run/NetworkedRunSessionMonitor.cs#L80)

- La coroutine unique prend la notice en parametre : nulle pour un quit, erreur pour une perte.
  [`NetworkedRunSessionMonitor.cs:129`](../../Assets/RoadRage/App/Run/NetworkedRunSessionMonitor.cs#L129)

- Le quit du menu emprunte ce chemin et refuse tout repli de routage, meme sans session.
  [`RunEscapeMenuFlowController.cs:150`](../../Assets/RoadRage/App/Run/RunEscapeMenuFlowController.cs#L150)

**Le portail partage et ses lecteurs**

- Pourquoi un etat statique : huit assemblies de feature ne peuvent pas referencer App.
  [`LocalInputGate.cs:15`](../../Assets/RoadRage/Shared/Input/LocalInputGate.cs#L15)

- Soumet Idle, sinon le host conserve la derniere intention et la voiture accelere encore.
  [`NetworkedVehicleDriverController.cs:195`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L195)

- Le lecteur oublie en premiere passe, rattrape en revue : respawn solo sur R.
  [`LocalVoidRespawnController.cs:80`](../../Assets/RoadRage/App/Run/LocalVoidRespawnController.cs#L80)

- Branche distincte pour ne pas detruire le litteral epingle par la Story 2.7.
  [`LocalOnFootController.cs:118`](../../Assets/RoadRage/Features/OnFoot/LocalOnFootController.cs#L118)

- L'orbite s'arrete net sans changer de camera : la vue du joueur est conservee.
  [`LocalVehicleCameraRig.cs:71`](../../Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs#L71)

**Le drapeau de sortie volontaire**

- Porte par l'objet persistant, donc il survit au changement de scene.
  [`RoadRageBootstrap.cs:93`](../../Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs#L93)

- Suspend la re-entree automatique de la Story 5.3, qui relancerait le client a la premiere frame.
  [`LobbyFlowController.cs:240`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L240)

- Leve seulement par une entree explicite : un clic refuse ne rend pas l'auto-rejoindre.
  [`LobbyFlowController.cs:258`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L258)

**L'ecran d'intention et son cablage de scene**

- L'ecran n'emet que des intentions : ni routage, ni quit, ni donnees de lobby.
  [`RunEscapeMenuScreen.cs:27`](../../Assets/RoadRage/Features/UI/RunEscapeMenuScreen.cs#L27)

- Echap, curseur capture puis restaure, portail bloque tant que le panneau est ouvert.
  [`RunEscapeMenuFlowController.cs:92`](../../Assets/RoadRage/App/Run/RunEscapeMenuFlowController.cs#L92)

- Panneau plein ecran, dernier enfant du Canvas donc au-dessus de l'overlay de mort.
  [`MVP_Run.unity:2247`](../../Assets/RoadRage/App/Scenes/MVP_Run.unity#L2247)

- EventSystem absent de la scene avant la story : sans lui, aucun bouton n'est cliquable.
  [`MVP_Run.unity:1264`](../../Assets/RoadRage/App/Scenes/MVP_Run.unity#L1264)

**Tests et preuve**

- Gardes de source, portail, drapeau, et composition authored de la scene.
  [`Story58EscapeMenuTests.cs:106`](../../Assets/RoadRage/Tests/EditMode/Story58EscapeMenuTests.cs#L106)

- Chemin reel : chargement direct, clic reel, quit, teardown menu ouvert.
  [`Story58EscapeMenuPlayModeTests.cs:99`](../../Assets/RoadRage/Tests/PlayMode/Story58EscapeMenuPlayModeTests.cs#L99)

- Preuves, couverture ligne a ligne de la matrice d'E/S, ecarts et residus acceptes.
  [`story-5-8-escape-menu-notes.md`](../../docs/setup/story-5-8-escape-menu-notes.md)
