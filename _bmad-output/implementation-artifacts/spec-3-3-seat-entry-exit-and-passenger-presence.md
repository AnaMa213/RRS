---
title: 'Story 3.3 : Entree sortie des sieges et presence passager'
type: 'feature'
created: '2026-09-09'
status: 'in-review'
review_loop_iteration: 0
context: []
baseline_commit: '68a7d82cb10b8df66a386e536818ad345cc3896c'
---

<frozen-after-approval reason="human-owned intent - do not modify unless human renegotiates">

## Intent

**Problem:** La voiture partagee peut deja etre revendiquee et conduite, mais l'etat joueur ne sait pas encore qui est conducteur, passager ou a pied. Sans entree/sortie de siege, Epic 4 ne pourra pas brancher les actions passager et Epic 6 ne pourra pas quitter proprement la voiture vers les phases a pied.

**Approach:** Ajouter une occupation de sieges host-authoritative dans le module Vehicules, puis un pont App/Run qui valide les demandes d'entree/sortie depuis le joueur local, met a jour `NetworkedPlayerState.Mode`/`SeatIndex`, masque le personnage pendant qu'il est dans la voiture, garde les passagers attaches sans controle conducteur, et libere les sieges sur sortie, mort ou deconnexion.

## Boundaries & Constraints

**Always:**
- Le module `RoadRage.Features.Vehicles` reste autonome : il peut stocker les occupants par `ulong clientId`, exposer des helpers de siege, et reutiliser `DriverClientId` comme siege conducteur, mais il ne reference pas `Players`, `OnFoot`, `UI`, `Run`, `PassengerActions`, `Rage`, `Economy` ou `Boss`.
- Toute mutation d'occupation, de `PlayerMode`, de `SeatIndex`, de `WorldPosition` et de `DriverClientId` est faite cote host apres validation du client emetteur, de la proximite, de la vie joueur et de la disponibilite du siege.
- `SeatIndex = 0` represente le conducteur ; les sieges passagers commencent a `1`. `SeatIndex = -1` reste l'etat a pied/non assis.
- L'entree standard choisit le conducteur si libre, sinon le premier passager libre ; une preference passager explicite doit permettre a un second joueur de s'asseoir sans recevoir le controle conducteur.
- En sortie, le joueur revient a une position sure proche de la voiture, repasse en mode `OnFoot`, et le mouvement local a pied est reactive sans laisser le pose reporter ecraser l'etat assis.
- En mode `Driver` ou `Passenger`, le personnage local et sa presentation reseau disparaissent visuellement ; ils reapparaissent uniquement a la sortie a pied.
- Le meme geste `E` fonctionne aussi dans `MVP_Run` solo hors-ligne : le joueur local entre dans la voiture, conduit, ressort a cote de la voiture, et la camera revient au rig a pied.

**Ask First:** Si le flux impose de modifier la representation visuelle des personnages ou de creer une UI durable de selection de siege, HALT et demander avant d'elargir. Un retour HUD/log minimal est acceptable.

**Never:**
- Ne pas implementer les actions passager, les degats, la recuperation de voiture, l'AI traffic, les upgrades, l'economie, le boss ou Rage Road.
- Ne pas donner d'input de conduite a un passager.
- Ne pas rendre la camera vehicule synchronisee reseau.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Entree proche | Joueur vivant a pied proche de la voiture, demande entree | Host assigne un siege disponible, met `Mode` a `Driver` ou `Passenger`, renseigne `SeatIndex`, masque le personnage et place la presence joueur au siege | N/A |
| Passager sans controle | Joueur assis sur siege passager | Il suit la voiture via l'etat partage et ne peut pas piloter `NetworkedVehicleDriverController` | Toute intention de conduite est ignoree car `DriverClientId` ne correspond pas |
| Refus entree | Joueur mort, trop loin, deja assis, ou aucun siege disponible | Aucun etat partage ne change | Log/feedback visible minimal |
| Sortie | Joueur assis demande sortie | Host libere le siege, remet `Mode = OnFoot`, `SeatIndex = -1`, teleporte vers une position de sortie sure et reactive le personnage visible | Si le siege ne correspond pas au joueur, ignorer sans mutation |
| Mort/deconnexion | Occupant meurt ou quitte la session | Host libere le siege ; si c'etait le conducteur, `DriverClientId` revient au sentinel et le controle est desactive | Feedback visible minimal cote host/HUD |
| Solo hors-ligne | Joueur local a pied dans `MVP_Run`, aucun `NetworkManager` actif, appuie sur `E` proche de la voiture | `RunFlowController` active le conducteur local sur le meme module voiture, masque le personnage, active la camera voiture, puis restaure camera et controle a pied a la sortie | Si aucune voiture proche existe, feedback HUD minimal |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleState.cs` -- ajouter constantes de siege, occupants passagers en `NetworkVariable<ulong>`, helpers purs/serveur pour lire, trouver, assigner et liberer un siege ; conserver la frontiere asmdef.
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` -- exposer un helper serveur de release/parking et un override conducteur local hors-ligne, sans changer le modele de conduite reseau 3.2.
- `Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs` -- garder l'activation locale reseau par `DriverClientId`, plus un override explicite pour le conducteur solo hors-ligne.
- `Assets/RoadRage/App/Run/NetworkedVehicleSeatService.cs` (nouveau) -- service host-only qui trouve la voiture partagee, valide les demandes, mute `NetworkedPlayerState`, synchronise la presence assise aux offsets de siege, et nettoie mort/deconnexion.
- `Assets/RoadRage/App/Run/NetworkedVehicleSeatIntent.cs` (nouveau) -- composant sur `NetworkedPlayerRoot`, lit l'input local (`E`, preference passager via modificateur) et envoie une RPC serveur validee contre `SenderClientId`.
- `Assets/RoadRage/App/Run/NetworkedPlayerSpawnService.cs` -- attacher l'intent siege aux players spawn et garantir le service siege cote host avec references spawner/HUD.
- `Assets/RoadRage/App/Run/NetworkedLocalPlayerPoseReporter.cs` -- ne plus soumettre la pose a pied quand le `NetworkedPlayerState.Mode` local est `Driver` ou `Passenger`.
- `Assets/RoadRage/App/Run/RunFlowController.cs` -- reagir aux changements `Mode`/`SeatIndex` du joueur local : geler le mouvement a pied pendant qu'il est assis, puis teleport/reactiver a la sortie ; en solo hors-ligne, piloter le meme flux entree/conduite/sortie via `E` dans `MVP_Run`.
- `Assets/RoadRage/Features/Players/NetworkedPlayerPresentation.cs` -- masquer la presentation reseau des joueurs en mode `Driver` ou `Passenger`, pour eviter un personnage debout rendu dans la voiture.
- `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs` -- ajouter un retour texte minimal pour entree/refus/sortie de siege, sans connaitre les types gameplay.
- `Assets/RoadRage/DevTools/RoadRageNetcodeSmokeTestAutoStart.cs` / `RoadRage.DevTools.asmdef` -- ajouter un harness editeur tres leger pour `Dev_VehicleSandbox` : apres auto-host, spawn d'un `NetworkedPlayerRoot` minimal et creation du service siege pour tester `E`/`Shift+E` sans future UI.
- `Assets/RoadRage/App/Scenes/MVP_Run.unity` / `Dev_VehicleSandbox.unity` -- aucune modification directe requise : `MVP_Run` supporte maintenant le chemin solo local via `RunFlowController` et le chemin reseau via `NetworkedPlayerSpawnService`, `Dev_VehicleSandbox` par le harness dev ci-dessus.
- `Assets/RoadRage/Tests/EditMode/Story33SeatEntryExitAndPassengerPresenceTests.cs` (nouveau) -- tests structurels/purs cibles : constantes, helper d'occupation, frontiere asmdef, pose reporter gated, presence des composants/services.

## Tasks & Acceptance

**Execution:**
- [x] `NetworkedVehicleState.cs` -- ajouter le contrat de siege host-owned -- base partagee driver/passagers.
- [x] `NetworkedVehicleSeatService.cs` / `NetworkedVehicleSeatIntent.cs` -- implementer entree/sortie et nettoyage lifecycle/deconnexion -- coeur du flux reseau.
- [x] `NetworkedPlayerSpawnService.cs` / `NetworkedLocalPlayerPoseReporter.cs` / `RunFlowController.cs` -- brancher le flux sur les players existants -- eviter que le controle a pied contredise l'etat assis et couvrir le solo hors-ligne de `MVP_Run`.
- [x] `RunCheckpointHudScreen.cs` / `RoadRageNetcodeSmokeTestAutoStart.cs` et scenes si necessaire -- feedback minimal et testabilite dans `MVP_Run` + `Dev_VehicleSandbox`.
- [x] `Story33SeatEntryExitAndPassengerPresenceTests.cs` -- verification minimale de la story -- verrouiller les invariants a faible cout.

**Acceptance Criteria:**
- Given un joueur vivant proche de la voiture, when il demande l'entree, then le host lui assigne un siege disponible et replique son `Mode`/`SeatIndex`.
- Given le joueur entre dans un siege, when son mode devient `Driver` ou `Passenger`, then son personnage visuel disparait jusqu'a sa prochaine sortie a pied.
- Given un passager assis, when il envoie ou maintient un input de conduite, then la voiture ne l'accepte pas comme conducteur.
- Given un occupant sort, meurt ou se deconnecte, when le host traite l'evenement, then le siege est libere et le controle conducteur est desactive si besoin.
- Given le module Vehicules inspecte apres implementation, when ses references asmdef sont lues, then il ne reference toujours aucune autre feature slice.
- Given `MVP_Run` est lance en solo hors-ligne, when le joueur a pied s'approche de la voiture et appuie sur `E`, then son personnage disparait, la camera passe voiture, et les inputs voiture pilotent le vehicule.
- Given ce joueur solo appuie a nouveau sur `E` dans la voiture, when la sortie est traitee, then il reapparait a cote de la voiture, la voiture ne consomme plus l'input conducteur, et la camera revient au controle a pied.

## Verification

**Commands:**
- `git diff --check` -- OK ; seulement avertissements CRLF Windows, aucune erreur whitespace.
- `unity command --project-path D:\Projets\RRS recompile --focus false` puis `recompile_status` -- OK, `completed`, `failed=false`, `errors=[]`.
- `unity command --project-path D:\Projets\RRS run_tests --mode EditMode --filter RoadRage.Tests.EditMode.Story33SeatEntryExitAndPassengerPresenceTests --filter_type testName --async_tests true`, puis `test_status` -- OK final apres correctif solo/camera, 9/9 tests verts, 0 echec, 0 skip, duree 0.38 s.
- `unity command --project-path D:\Projets\RRS run_tests --mode EditMode --filter RoadRage.Tests.EditMode.Story32DriverControlAndLocalCameraTests --filter_type testName --async_tests true`, puis `test_status` -- OK, 9/9 tests verts, 0 echec, 0 skip, duree 0.33 s.
- `git status --short --branch` / `git diff --stat` -- OK ; changements limites aux fichiers de Story 3.3/correctif avant commit final.

## Suggested Review Order

**Contrat de siege**

- Verite partagee minimale : conducteur reuse `DriverClientId`, passagers restent dans Vehicles.
  [`NetworkedVehicleState.cs:16`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleState.cs#L16)

- Assignation/liberation empechent doublons et sieges invalides.
  [`NetworkedVehicleState.cs:156`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleState.cs#L156)

**Arbitrage host**

- Entree/sortie centralisee cote host, avec validations joueur/vehicule.
  [`NetworkedVehicleSeatService.cs:114`](../../Assets/RoadRage/App/Run/NetworkedVehicleSeatService.cs#L114)

- Nettoyage mort/deconnexion libere les sieges et coupe le conducteur.
  [`NetworkedVehicleSeatService.cs:207`](../../Assets/RoadRage/App/Run/NetworkedVehicleSeatService.cs#L207)

- Poses assises publiees depuis le host vers les proxies joueur.
  [`NetworkedVehicleSeatService.cs:231`](../../Assets/RoadRage/App/Run/NetworkedVehicleSeatService.cs#L231)

**Intentions locales**

- `E`/`Shift+E` envoie une demande RPC, jamais une mutation locale.
  [`NetworkedVehicleSeatIntent.cs:49`](../../Assets/RoadRage/App/Run/NetworkedVehicleSeatIntent.cs#L49)

- La conduite ne consomme que le client vraiment conducteur.
  [`NetworkedVehicleDriverController.cs:104`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L104)

- En solo hors-ligne, le meme controleur accepte un conducteur local explicite sans session Netcode.
  [`NetworkedVehicleDriverController.cs:223`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L223)

**Pont joueur local**

- Le spawner garantit le service de sieges dans le run reseau.
  [`NetworkedPlayerSpawnService.cs:189`](../../Assets/RoadRage/App/Run/NetworkedPlayerSpawnService.cs#L189)

- La pose a pied ne reporte plus pendant les modes assis.
  [`NetworkedLocalPlayerPoseReporter.cs:39`](../../Assets/RoadRage/App/Run/NetworkedLocalPlayerPoseReporter.cs#L39)

- Les presentations reseau assises sont masquees, pas rendues debout dans la voiture.
  [`NetworkedPlayerPresentation.cs:79`](../../Assets/RoadRage/Features/Players/NetworkedPlayerPresentation.cs#L79)

- Le joueur local gele/suit le siege, puis redevient mobile a la sortie.
  [`RunFlowController.cs:330`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L330)

- Le chemin solo `MVP_Run` entre/sort avec `E`, active la camera voiture, puis rattache la camera au rig a pied.
  [`RunFlowController.cs:382`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L382)

**Test direct**

- Le sandbox vehicule cree un player reseau minimal pour tester `E` sans UI future.
  [`RoadRageNetcodeSmokeTestAutoStart.cs:73`](../../Assets/RoadRage/DevTools/RoadRageNetcodeSmokeTestAutoStart.cs#L73)

- Le prefab joueur porte le NetworkBehaviour d'intention.
  [`NetworkedPlayerRoot.prefab:205`](../../Assets/RoadRage/Resources/NetworkedPlayerRoot.prefab#L205)

**Verification**

- Tests cibles couvrent contrat, gates, prefab et harness dev.
  [`Story33SeatEntryExitAndPassengerPresenceTests.cs:18`](../../Assets/RoadRage/Tests/EditMode/Story33SeatEntryExitAndPassengerPresenceTests.cs#L18)
