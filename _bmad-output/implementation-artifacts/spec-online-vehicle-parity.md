---
title: 'Parite multi-vehicules entre solo et session reseau'
type: 'bugfix'
created: '2026-09-13'
status: 'in-review'
review_loop_iteration: 0
baseline_commit: 'b27e3c7c79b952ed5b793a77057c87b9b0364390'
context:
  - '_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md'
---

<frozen-after-approval reason="human-owned intent - do not modify unless human renegotiates">

## Intent

**Problem:** Dans `MVP_Run`, le solo permet d'entrer dans toute voiture proche, tandis que la session reseau ne gere qu'une voiture arbitraire. En solo, les degats ecrivent aussi des `NetworkVariable` sur des objets non spawn, ce qui produit un warning Netcode.

**Approach:** Conserver l'autorite hote et les controles existants, mais faire resoudre au host la voiture proche ou deja occupee pour chaque demande. Fournir un etat de degat local explicite aux voitures non spawn du solo, sans ecriture de `NetworkVariable` avant spawn.

## Boundaries & Constraints

**Always:** Le host reste seul a muter les sieges, poses partagees et degats reseau. Une entree reseau choisit la voiture la plus proche du `WorldPosition` host du joueur; sortie, changement de siege, mort et deconnexion retrouvent ensuite la voiture qui contient ce client. Solo et multi gardent les memes touches (`E`, `Shift+E`, `G`), distances, regles de sieges et controle conducteur. Les voitures runtime restent creees par le host uniquement.

**Ask First:** Arreter si la parite exige de changer la scene au-dela de `MVP_Run`, d'ajouter une UI de selection de vehicule, ou de modifier la configuration des prefabs NGO sans preuve runtime que `DefaultNetworkPrefabs.asset` n'est pas charge.

**Never:** Ne pas introduire de registre global, de nouveau protocole RPC ni de proprietaire client des voitures. Ne pas supprimer les degats solo, contourner la validation de distance host, ni changer la physique host-authoritative.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Entree multi | Joueur vivant a pied, plusieurs voitures, `E` | Le host assigne un siege dans la voiture reseau la plus proche, comme le solo | Refuse si aucune voiture proche/livre ou joueur invalide |
| Continuite multi | Joueur assis dans une voiture, `G`, `E`, mort ou deconnexion | Siege, pose, sortie et liberation concernent cette meme voiture | Aucune mutation si le client ne siege dans aucune voiture |
| Degats solo | Voiture non spawn, collision au-dessus du seuil | HP, flags et HUD changent sans warning NGO | Les valeurs reseau restent reservees aux objets spawn |
| Replication | Hote cree les voitures runtime de `MVP_Run` | Les clients recoivent les memes voitures et positions hote | Diagnostiquer le prefab NGO avant toute modification d'enregistrement |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/App/Run/NetworkedVehicleSeatService.cs` -- le cache unique `vehicleState` (`422-443`) dirige actuellement toutes les operations; reutiliser `FindSeatIndex(clientId)` sur les voitures de scene/runtime pour resoudre la voiture proche a l'entree et celle occupee apres entree.
- `Assets/RoadRage/App/Run/NetworkedVehicleSeatIntent.cs` -- l'intention valide deja le client emetteur et delegue au host (`60-107`); ne changer que si la nouvelle API de service l'exige.
- `Assets/RoadRage/App/Run/RunFlowController.cs` -- le chemin solo de reference cherche deja la voiture la plus proche (`1489-1528`); les ponts de collision appellent `ApplyDamage` (`693-839`) et le HUD lit les degats (`890-919`).
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleState.cs` -- `ResetDamageState` et `ApplyDamage` ecrivent directement les NetworkVariables (`132-184`); centraliser les lectures/ecritures pour bifurquer sur l'etat local lorsque `!IsSpawned`.
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` / `NetworkedVehicleDamageVfxController.cs` -- consommateurs des flags/HP; les basculer vers les accesseurs de lecture de l'etat voiture.
- `Assets/RoadRage/App/Run/DevVehicleSpawner.cs` et `Assets/DefaultNetworkPrefabs.asset` -- creation host-only et prefab voiture existant; lire seulement la configuration avant d'envisager un changement.
- `Assets/RoadRage/Tests/EditMode/Story33SeatEntryExitAndPassengerPresenceTests.cs` et `Story35VehicleDamageHookAndTeamWipeContractStubTests.cs` -- etendre les garanties de plusieurs voitures et de l'etat solo non spawn.

## Tasks & Acceptance

**Execution:**
- [x] `NetworkedVehicleSeatService.cs` -- remplacer la reference vehicule unique par des resolutions bornees: proche pour entrer, occupante pour les actions ulterieures, et boucle sur toutes les voitures pour la maintenance host -- etablit la parite sans etat duplique.
- [x] `NetworkedVehicleState.cs` et ses consommateurs -- encapsuler HP, flags et ordre de degats avec un backing local hors spawn et des accesseurs uniques -- conserve les degats solo sans toucher de NetworkVariable prematurement.
- [x] `Story33SeatEntryExitAndPassengerPresenceTests.cs` / `Story35VehicleDamageHookAndTeamWipeContractStubTests.cs` -- ajouter les controles purs/structurels des deux regressions -- bloque le retour du cache unique et des ecritures solo NGO.
- [x] `MVP_Run` en Play Mode host + client -- verifier la presence et la pose identique des voitures runtime avant d'ajouter toute inscription de prefab -- determine si `DefaultNetworkPrefabs.asset` suffit.

**Acceptance Criteria:**
- Given deux voitures reseau a portee differente, when un joueur a pied appuie sur `E`, then le host l'assoit dans la plus proche et seul ce siege recoit sa pose.
- Given un joueur occupe une voiture, when il appuie sur `G` ou `E`, meurt, ou se deconnecte, then aucune autre voiture ne voit ses sieges ou son conducteur modifies.
- Given une voiture non spawn en solo recoit un impact, when les degats sont appliques, then son HP/flags et le HUD evoluent sans le warning `doesn't know its NetworkBehaviour yet`.
- Given un hote et un client chargent `MVP_Run`, when les voitures runtime sont creees, then leurs positions observent celles choisies par l'hote sur les deux pairs.

## Verification

**Manual checks:**
- En solo sans lobby dans `MVP_Run`, tester les voitures proches, collision et absence du warning NGO.
- En host + client, verifier les memes positions voiture; chacun entre/sort de la voiture proche et un passager ne conduit pas.

**Resultat:** verification manuelle et tests EditMode cibles confirmes verts par l'utilisateur le 2026-09-13.
