---
title: "Story 4.2 : Framework d'actions passager et intention validee par l'hote"
type: 'feature'
created: '2026-09-11'
status: 'done'
review_loop_iteration: 0
context: []
baseline_commit: 'c089fa4ffadebabd398f0c1a93e9ccf193b63074'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Les passagers n'ont aucun contrat commun pour soumettre une action : ni trois slots authorables, ni intention reseau, ni validation hote, ni retour visible en cas d'acceptation ou de refus.

**Approach:** Ajouter un framework minimal dans `Dev_RageSandbox` et dans le routage normal vers `MVP_Run` : trois definitions versionnees, une intention typee, une validation centralisee et un feedback greybox. Le meme point d'entree traite directement le hors-ligne ou envoie au serveur en hote/client. L'acceptation publie seulement un evenement d'extension ; les effets arrivent en Stories 4.3-4.5.

## Boundaries & Constraints

**Always:** Exactement trois slots `0..2`; ids stables minuscules; payload reseau par id + `NetworkObjectReference`; cooldowns et sequences autoritaires; validation de la version, phase, mode/occupation du siege passager, cooldown, cible valide et portee avant publication, plus sender connecte en reseau. Utiliser `NetworkManager.ServerTime.Time` en reseau et `Time.timeAsDouble` hors ligne. Le framework est disponible apres les chemins existants `AppSceneRouter.LoadMvpRun()` et chargement NGO de `MVP_Run`; chaque verdict est visible. `RunFlowController` ne fait que configurer le contexte local/reseau et l'UI.

**Ask First:** Toute nouvelle `RunPhase`, registre generique multi-catalogues, interface/factory d'effets, ou modification d'un contrat Rage/Vehicle partage.

**Never:** Appliquer rage, incident, ressource, aide, recompense ou resultat de run; ajouter trafic/IA, selection de cible finale, `NetworkVariable` par cooldown, ou logique PassengerActions dans `RunFlowController`.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Intention valide | acteur connecte, phase autorisee, siege passager coherent, cible Rage spawnée/a portee, versions et sequence valides | evenement `ActionValidated` emis une fois, cooldown demarre, feedback accepte | N/A |
| Intention hors ligne | `MVP_Run` sans NetworkManager en ecoute, joueur local assis passager | meme validation/evenement/cooldown local, sans RPC | verdict visible localement |
| Acteur invalide | sender usurpe, deconnecte ou lifecycle non jouable | aucun evenement ni cooldown | verdict visible explicite |
| Contexte invalide | mauvaise phase, mode/siege/occupant incoherent | aucun evenement ni cooldown | verdict visible explicite |
| Action invalide | id/slot/version catalogue ou payload inconnu, sequence rejouee, cooldown actif | aucun evenement | verdict visible explicite |
| Cible invalide | reference irresolue/despawnee, sans `NetworkedRageState` ou hors portee | aucun evenement ni cooldown | verdict visible explicite |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/PassengerActions/` -- feature isolee; suivre `RageTuningDef.cs:14-75` et `RageTuningCatalog.cs:28-157`, sans registre generique.
- `Assets/RoadRage/App/Run/NetworkedPassengerActionIntent.cs` -- nouveau composant RPC; reprendre l'authentification sender de `NetworkedVehicleSeatIntent.cs:49-79` et la revalidation de `NetworkedVehicleRecoveryIntent.cs:53-106`.
- `Assets/RoadRage/Features/Players/NetworkedPlayerState.cs:36-54` et `Assets/RoadRage/Features/Vehicles/NetworkedVehicleState.cs:251-317,380-388` -- verites acteur, mode, siege et occupant, en lecture seule.
- `Assets/RoadRage/Shared/Domain/RunPhase.cs:3-6` -- seule phase actuelle `NotStarted`; les defs sandbox l'autorisent, sans inventer `Driving`.
- `Assets/RoadRage/Resources/NetworkedPlayerRoot.prefab:190-245` et `NetworkedPlayerSpawnService.cs:149-152` -- porter et verifier le composant d'intention.
- `Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity:448-756` -- reutiliser NetworkManager, Canvas et cible Rage; completer le harness passager/run/siege.
- `Assets/RoadRage/App/Services/AppSceneRouter.cs:21-28` et `Assets/RoadRage/App/Lobby/LobbyFlowController.cs:378-466` -- routes existantes vers `MVP_Run`, locale directe ou scene NGO synchronisee; ne pas les dupliquer.
- `Assets/RoadRage/App/Run/RunFlowController.cs:96-123,720-852` et `Assets/RoadRage/App/Scenes/MVP_Run.unity:3605-3656,4184` -- brancher l'action UI sur le joueur local et son siege, en mode hors ligne comme en reseau.
- `Assets/RoadRage/Tests/EditMode/Story41RageStateModuleAndDefinitionsTests.cs` et `Story33SeatEntryExitAndPassengerPresenceTests.cs` -- precedents validation d'assets, gardes pures et prefab.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/PassengerActions/PassengerActionDef.cs`, `PassengerActionCatalog.cs`, `PassengerActionIntent.cs`, `PassengerActionValidation.cs` -- definir trois slots, versions, payload NGO et verdict pur; valider ids/slots/catalogue.
- [x] `Assets/RoadRage/App/Run/NetworkedPassengerActionIntent.cs` -- exposer un meme point d'entree : traitement autoritaire direct sans session reseau, ou RPC puis resolution/validation hote; proteger sequence/cooldown, publier `ActionValidated` et diffuser le feedback sans appliquer d'effet.
- [x] `Assets/RoadRage/Features/PassengerActions/PassengerActionDebugView.cs` -- afficher les trois slots et le dernier verdict; touches `1/2/3` uniquement dans le sandbox.
- [x] `Assets/RoadRage/Resources/NetworkedPlayerRoot.prefab`, `NetworkedPlayerSpawnService.cs` et asmdefs concernes -- cabler/configurer le composant pour les joueurs hote et clients.
- [x] `Assets/RoadRage/ScriptableObjects/PassengerActions/` et `Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity` -- authorer trois defs placeholder + catalogue versionne; fournir acteur Passenger vivant, siege occupe, `NetworkedRunState`, cible Rage et UI.
- [x] `Assets/RoadRage/App/Run/RunFlowController.cs`, `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs` et `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- configurer le contexte et afficher les trois slots/verdicts apres entree par le routage existant; supporter le siege passager hors ligne (`Shift+E`) et le joueur reseau local.
- [x] `Assets/RoadRage/Tests/EditMode/Story42PassengerActionFrameworkTests.cs` et `Assets/RoadRage/Tests/PlayMode/Story42PassengerActionMvpRunPlayModeTests.cs` -- couvrir la matrice/cablage puis prouver l'entree `MVP_Run` hors ligne et le flux hote/client.

**Acceptance Criteria:**
- Given un passager dans `Dev_RageSandbox`, when il utilise les slots 1, 2 et 3, then chaque intention atteint l'hote et produit un feedback visible sans effet gameplay.
- Given le menu route une partie hors ligne vers `MVP_Run`, when le joueur prend un siege passager avec `Shift+E` puis utilise un slot, then l'intention est validee localement et son verdict apparait dans le HUD.
- Given l'hote charge `MVP_Run` par le `NetworkSceneManager` avec un client connecte, when le client passager utilise un slot, then seul l'hote valide et les deux instances restent synchronisees sans mutation directe cote client.
- Given une intention invalide pour acteur, phase, siege, cooldown, cible, portee, sequence ou version, when l'hote la traite, then elle est rejetee avant `ActionValidated` et la raison est visible.
- Given une intention valide, when l'hote l'accepte, then `ActionValidated` est emis exactement une fois avec la definition, l'acteur et la cible resolus.

## Spec Change Log

## Design Notes

Un validateur pur retourne un verdict. `NetworkedPassengerActionIntent` adapte soit le contexte local, soit NGO, sans dupliquer les regles. Un unique evenement accepte est le point d'extension des Stories 4.3-4.5.

## Verification

**Commands:**
- `unity command --project-path D:\Projets\RRS recompile --focus false` puis `recompile_status` -- attendu : compilation terminee sans erreur.
- `unity command --project-path D:\Projets\RRS run_tests --mode EditMode --filter RoadRage.Tests.EditMode.Story42PassengerActionFrameworkTests --filter_type testName --async_tests true`, puis `test_status` -- attendu : tous verts.
- `unity command --project-path D:\Projets\RRS run_tests --mode PlayMode --filter RoadRage.Tests.PlayMode.Story42PassengerActionMvpRunPlayModeTests --filter_type testName --async_tests true`, puis `test_status` -- attendu : parcours hors ligne et hote/client verts.
- `git diff --check` -- attendu : aucune erreur.

**Manual checks (if no CLI):**
- Depuis `MainMenuLobby`, entrer dans `MVP_Run` hors ligne puis en hote/client : prendre un siege passager, utiliser les trois slots et verifier les verdicts, sans changement de rage. Garder `Dev_RageSandbox` comme diagnostic isole.

## Suggested Review Order

**Validation pure et modele de donnees**

- Point d'entree : le validateur pur retourne un verdict a partir du contexte, sans effet de bord.
  [`PassengerActionValidation.cs:75`](../../Assets/RoadRage/Features/PassengerActions/PassengerActionValidation.cs#L75)

- Trois slots fixes `0..2`, ids stables, version de catalogue verifiee avant tout verdict.
  [`PassengerActionCatalog.cs:9`](../../Assets/RoadRage/Features/PassengerActions/PassengerActionCatalog.cs#L9)

- Definition versionnee d'une action passager (id, slot, cooldown).
  [`PassengerActionDef.cs:9`](../../Assets/RoadRage/Features/PassengerActions/PassengerActionDef.cs#L9)

**Autorite reseau et routage du feedback (correction de revue incluse)**

- Meme point d'entree pour le hors-ligne et le reseau ; protege sequence/cooldown avant de publier `ActionValidated`.
  [`NetworkedPassengerActionIntent.cs:121`](../../Assets/RoadRage/App/Run/NetworkedPassengerActionIntent.cs#L121)

- Correction de revue : le verdict n'est plus diffuse a tous les clients, il cible desormais uniquement l'acteur reel via `RpcTarget.Single`.
  [`NetworkedPassengerActionIntent.cs:183`](../../Assets/RoadRage/App/Run/NetworkedPassengerActionIntent.cs#L183)

- Chemin direct hors session reseau ou RPC serveur selon le contexte de l'appelant.
  [`NetworkedPassengerActionIntent.cs:72`](../../Assets/RoadRage/App/Run/NetworkedPassengerActionIntent.cs#L72)

**Cablage scene et prefab**

- Configure le catalogue, la cible Rage et le composant d'intention pour le joueur local, hors ligne comme en reseau.
  [`RunFlowController.cs:247`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L247)

- Harnais de diagnostic isole (editeur uniquement) pour `Dev_RageSandbox`.
  [`RageSandboxAutoStart.cs:23`](../../Assets/RoadRage/DevTools/RageSandboxAutoStart.cs#L23)

- Affichage HUD du dernier verdict passager.
  [`RunCheckpointHudScreen.cs:121`](../../Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs#L121)

- Vue de debug sandbox : trois slots et dernier verdict, touches `1/2/3`.
  [`PassengerActionDebugView.cs:8`](../../Assets/RoadRage/Features/PassengerActions/PassengerActionDebugView.cs#L8)

**Peripheriques**

- Tests EditMode couvrant la matrice complete de validite/invalidite.
  [`Story42PassengerActionFrameworkTests.cs`](../../Assets/RoadRage/Tests/EditMode/Story42PassengerActionFrameworkTests.cs#L1)

- Tests PlayMode prouvant l'entree `MVP_Run` hors ligne et le cablage hote/client.
  [`Story42PassengerActionMvpRunPlayModeTests.cs`](../../Assets/RoadRage/Tests/PlayMode/Story42PassengerActionMvpRunPlayModeTests.cs#L1)
