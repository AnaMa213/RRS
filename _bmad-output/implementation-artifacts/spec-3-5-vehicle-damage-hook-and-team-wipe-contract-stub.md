---
title: 'Story 3.5 : Contrat de degats vehicule et stub de team-wipe'
type: 'feature'
created: '2026-09-09'
status: 'done'
review_loop_iteration: 0
context: []
baseline_commit: 'c5b59a190e50320100240699b33c17b8f1ef53f9'
---

<frozen-after-approval reason="human-owned intent - do not modify unless human renegotiates">

## Intent

**Problem:** Les collisions du vehicule partage (3.4) ne produisent qu'un retour visuel ; aucune regle de vie ou de degats reelle n'existe cote joueur (`NetworkedPlayerState`) ni cote voiture, alors que les hearts restent un placeholder toujours reremplis (2.7) et que rien ne relie collision -> consequence testable.

**Approach:** Ajouter un contrat de degats host-authoritatif : une collision reduit l'HP du joueur assis (nouveau, sur `NetworkedPlayerState`) et, selon la vitesse d'impact, l'HP de la voiture (nouveau, sur `NetworkedVehicleState`) ; a 0 HP joueur -> `Downed` (ejection de siege, vitesse plafonnee, fenetre de resurrection 30s) puis `Alive`/respawn ou `Dead` selon les hearts restants (les hearts deviennent une ressource consommable, remplacant le remplissage automatique de la 2.7) ; a 0 HP voiture -> voiture inconduisible, tous les occupants ejectes via le flux de sortie de siege existant (3.3/3.4). La condition all-dead reste detectable ; le redemarrage reel de run reste stube (Epic 7). Cote presentation, chaque type de degat vehicule actif declenche un effet VFX client cumulable (fumee moteur, etincelles frein, vacillement roue), sans nouveau modele 3D, purement pilote par les flags de degat synchronises existants.

## Boundaries & Constraints

**Always:**
- Tout calcul/application de degats (HP joueur, hearts, transitions de cycle de vie, HP voiture, type de degat, modificateurs de conduite) est calcule et applique cote host uniquement, via `NetworkedPlayerState`/`NetworkedVehicleState` (AD-3, AD-18) -- jamais dans un script client ou UI.
- Le module `RoadRage.Features.Vehicles` ne reference toujours aucune autre feature slice ; le pont degats-vehicule -> HP-joueur passe par un service App/Run (nouveau ou etendu dans `RunFlowController`/aux cotes de `NetworkedVehicleSeatService`) qui s'abonne a l'evenement C# de collision, meme motif que le pont HUD existant.
- La resurrection est une intention client validee (ServerRpc) suivant le meme motif garde que `NetworkedVehicleRecoveryIntent`/`NetworkedPlayerLifecycleIntent` (emetteur = joueur `Alive` proche, cible = joueur `Downed`).
- Les hearts ne sont depensees qu'a l'expiration non-resuscitee de la fenetre de 30s ; le remplissage inconditionnel de `TryRespawn` (`NetworkedPlayerLifecycleService.cs:200`) et la remise a zero sur `Dead` (`:173-176`) sont remplaces par ce flux consommable.
- `IsValidTransition` (`NetworkedPlayerLifecycleService.cs:105-120`) doit autoriser `Downed -> Alive` (resurrection) en plus de `Downed -> Dead/Disconnected` ; rien ne transitionne encore vers `Downed` aujourd'hui, ce chemin doit etre ajoute.
- Le type de degat vehicule (roue/moteur/frein) est tire aleatoirement sans repetition parmi les 3, un seul par seuil de 33 HP cumules franchi, et modifie les parametres de conduite existants (`maxForwardSpeed`, `steerDegreesPerSecond`, `brakeDeceleration`) plutot que d'introduire un second modele physique.
- Ne jamais muter directement l'economie, le boss ou l'etat Rage depuis ce chemin.
- Les effets visuels de degat vehicule (fumee moteur, etincelles frein, vacillement roue) sont purement cosmetiques et cote client, cumulables jusqu'a 3 simultanes, pilotes uniquement par la lecture des flags synchronises `WheelDamaged`/`EngineDamaged`/`BrakeDamaged` de `NetworkedVehicleState` -- aucun calcul de degat ni etat additionnel n'est introduit cote VFX.

**Ask First:** Si la resurrection exige une UI persistante nouvelle au-dela d'un indicateur HUD minimal (barre HP/compte a rebours), confirmer avant d'ajouter un ecran/overlay durable.

**Never:**
- Ne pas implementer le redemarrage reel de run (Epic 7), l'economie, le boss ou Rage Road.
- Ne pas resynchroniser la camera vehicule ni modifier le contrat de siege (3.3) au-dela de l'ejection forcee a 0 HP voiture.
- Ne pas ajouter de detection "coince" automatique (deja tranche en 3.4, hors scope ici).
- Ne pas creer de nouveau modele 3D (Blender) pour representer un etat d'endommagement visuel ; le retour visuel passe uniquement par VFX (particules) et une perturbation legere du mesh greybox existant.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Collision joueur en siege | Voiture percute qqch, joueur assis (Story 3.4 `VehicleCollided`) | Host reduit l'HP du joueur de 5 via `NetworkedPlayerState` | N/A |
| HP joueur atteint 0 | Reduction HP | Host: `Downed`, ejection du siege, vitesse plafonnee a une fraction, fenetre de resurrection 30s demarree (visible en shared state) | N/A |
| Resurrection a temps | Autre joueur `Alive` proche declenche avant expiration | Host: retour `Alive` a 30 HP, aucune heart depensee | RPC rejetee si emetteur non `Alive`/hors validation |
| Fenetre expiree, hearts restantes | Timer expire, `Hearts > 0` | Host: -1 heart, respawn `Alive` a 100 HP | N/A |
| Fenetre expiree, 0 heart restante | Timer expire, `Hearts == 0` | Host: `Dead` pour le reste du run | N/A |
| Collision vehicule >= vitesse min | Impact vitesse suffisante | HP voiture -5 a -15 selon la vitesse | Sous le seuil min : aucun degat voiture |
| Seuil cumule franchi (67/34/1 HP restants) | Degats cumules | 1 type de degat (roue/moteur/frein) applique, sans repetition | N/A |
| HP voiture atteint 0 | Degats cumules | Voiture inconduisible, tous les occupants ejectes a pied via le flux de sortie existant (3.3/3.4) | N/A |
| Tous les joueurs connectes `Dead` | Verification host | Condition all-dead detectable (extension du log existant) | Redemarrage reel reste stube (Epic 7) |
| Type(s) de degat vehicule actif(s) | 1 a 3 flags vrais sur `NetworkedVehicleState` (`WheelDamaged`/`EngineDamaged`/`BrakeDamaged`) | Effet VFX client correspondant actif (fumee/etincelles/vacillement), cumulable jusqu'a 3 simultanes | Aucun effet si le flag correspondant est faux |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Players/NetworkedPlayerState.cs` -- ajouter `NetworkVariable<int> Hp` (defaut 100) et un etat de fenetre de resurrection (ex. `NetworkVariable<double> ReviveDeadlineTime` ou equivalent), aux cotes de `Hearts`/`MaxHearts`/`Lifecycle` (lignes 60-68, 30-33).
- `Assets/RoadRage/App/Run/NetworkedPlayerLifecycleService.cs` -- etendre `IsValidTransition` (105-120) pour `Downed -> Alive` ; ajouter methode d'application de degats joueur (nouvelle transition vers `Downed` a HP 0) ; modifier `TryRespawn` (187-210, refill hearts a la ligne 200) et la remise a zero sur `Dead` (173-176) pour le flux hearts consommable (depense uniquement a l'expiration non-resuscitee) ; reutiliser `ShouldLogAllDeadRestartCondition`/`CheckAllDeadRestartCondition` (136-149, 227-256) tel quel pour la detection all-dead.
- `Assets/RoadRage/App/Run/NetworkedPlayerLifecycleIntent.cs` -- ajouter une RPC de resurrection miroir de `RequestRespawnRpc` (54-84) : emetteur = joueur `Alive` proche, cible = `clientId` `Downed`.
- `Assets/RoadRage/Features/OnFoot/LocalOnFootController.cs` -- ajouter un multiplicateur de vitesse (aux cotes de `walkSpeed`/`sprintSpeed`, lignes 16-19, appliques dans `ApplyMovement` 181-207) pour le plafond `Downed`, distinct du gel complet existant (`MovementEnabled`, ligne 40).
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleState.cs` -- ajouter `NetworkVariable<int> Hp` (defaut 100) et les flags/modificateurs de degat (roue/moteur/frein), aux cotes des NetworkVariables de siege existantes (35-53).
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` -- `OnCollisionEnter` (285-294) : lire `collision.relativeVelocity.magnitude`, changer `VehicleCollided` (`Action`, ligne 105) en `Action<float>` et propager via `NotifyVehicleCollidedRpc` (310-314) ; appliquer les modificateurs de degat dans la chaine `ApplyPhysics`/`ResolveTargetSpeed`/`ApplySteering` (349-473, ex. `maxForwardSpeed` 35, `steerDegreesPerSecond` 63, `brakeDeceleration` 51) en lisant les flags de `NetworkedVehicleState`.
- `Assets/RoadRage/App/Run/NetworkedVehicleSeatService.cs` -- etendre `ReleaseInvalidOccupants`/`ShouldReleaseOccupant` (163-166, 209-231) pour ejecter tous les occupants quand l'HP voiture atteint 0 (au-dela du cas `Dead`/`Disconnected` existant).
- `Assets/RoadRage/App/Run/RunFlowController.cs` -- etendre `EnsureVehicleEventBridge`/`HandleVehicleCollided` (259-295) pour appliquer les degats joueur+voiture sur collision (pont vers `NetworkedPlayerLifecycleService`/`NetworkedVehicleState`) ; ajouter une branche `Downed` dans `HandleLifecycleChanged` (769-810, actuellement seulement `Dead`/respawn) pour le plafond de vitesse et l'affichage du compte a rebours.
- `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs` -- ajouter affichage HP joueur/indicateur `Downed`+compte a rebours et HP voiture/type de degat, aux cotes de `SetHearts` (152-155) et `ShowDeathOverlay` (180-197).
- `Assets/RoadRage/App/Run/NetworkedPlayerReviveIntent.cs` (nouveau) -- entree locale pres d'un coequipier `Downed` -> RPC serveur validee, miroir de `NetworkedVehicleRecoveryIntent.cs`.
- `Assets/RoadRage/Tests/EditMode/Story35VehicleDamageHookAndTeamWipeContractStubTests.cs` (nouveau) -- tests structurels cibles, meme motif que Story 3.4/2.7 (predicats purs degats/seuils/selection sans repetition/expiration fenetre, assertions source-texte, reaffirmation frontiere asmdef Vehicules).
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDamageVfxController.cs` (nouveau) -- component client (meme module Vehicules, aucune reference croisee) : lit `WheelDamaged`/`EngineDamaged`/`BrakeDamaged` sur `NetworkedVehicleState` (deja implementes, lignes 80-93) via callback `OnValueChanged`, active/desactive les `ParticleSystem` fumee (moteur) et etincelles (frein), et pilote la perturbation de rotation "vacillement" (roue) sur le corps du vehicule ; purement cosmetique, aucune mutation de `NetworkedVehicleState`.
- `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` -- ajouter deux enfants `ParticleSystem` (fumee moteur, etincelles frein) positionnes approximativement aux emplacements moteur/frein sur le mesh unique existant (pas de roues separees, cf. Design Notes) ; aucun nouveau mesh/import.

## Tasks & Acceptance

**Execution:**
- [x] `NetworkedPlayerState.cs` -- ajouter `Hp` + etat fenetre resurrection -- support shared-state du contrat de vie.
- [x] `NetworkedPlayerLifecycleService.cs` -- transition `Downed`, resurrection, hearts consommables, degats joueur -- coeur du contrat de vie host-authoritatif.
- [x] `NetworkedPlayerLifecycleIntent.cs` -- RPC de resurrection validee -- entree manuelle "reviver".
- [x] `LocalOnFootController.cs` -- plafond de vitesse `Downed` -- feedback de mouvement degrade.
- [x] `NetworkedVehicleState.cs` -- `Hp` + flags de degat -- support shared-state du contrat vehicule.
- [x] `NetworkedVehicleDriverController.cs` -- vitesse d'impact + modificateurs de conduite -- degats vehicule et handling degrade.
- [x] `NetworkedVehicleSeatService.cs` -- ejection totale a HP voiture 0 -- reutilise le flux de sortie 3.3/3.4.
- [x] `RunFlowController.cs` -- pont collision -> degats joueur/voiture, branche `Downed` -- parite solo/reseau, HUD.
- [x] `RunCheckpointHudScreen.cs` -- affichage HP/Downed/degat voiture -- visibilite minimale sans UI durable.
- [x] `NetworkedPlayerReviveIntent.cs` (nouveau) -- resurrection validee -- entree manuelle coequipier.
- [x] `Story35VehicleDamageHookAndTeamWipeContractStubTests.cs` (nouveau) -- verification ciblee.
- [x] `NetworkedVehicleDamageVfxController.cs` (nouveau) -- lecture des flags de degat + activation VFX cumulable -- retour visuel client sans nouveau modele.
- [x] `Greybox_PlayerCar.prefab` -- ajouter les `ParticleSystem` fumee/etincelles -- support du controller VFX, aucun nouveau mesh.

**Acceptance Criteria:**
- Given le module Vehicules inspecte apres implementation, when ses references asmdef sont lues, then il ne reference toujours aucune autre feature slice.
- Given une session sans systeme de degats reel avant cette story, when un joueur ou la voiture atteint 0 HP, then le comportement decrit dans la matrice I/O est observable en shared state (`NetworkedPlayerState`/`NetworkedVehicleState`), sans mutation d'economie/boss/rage.
- Given la voiture peut etre testee, when aucun trafic IA ni systeme Rage Road n'est present, then le contrat de degats reste jouable et testable seul.
- Given plusieurs flags de degat vehicule actifs simultanement, when le client observe le vehicule, then les effets VFX correspondants sont tous actifs en meme temps (cumulables), sans mutation d'etat gameplay.

## Design Notes

Vitesse d'impact -> degats voiture : lire `collision.relativeVelocity.magnitude` a l'entree en collision, definir un seuil minimal (aucun degat en dessous) et interpoler lineairement vers une vitesse de reference pour mapper sur [5, 15] HP -- valeurs exactes au choix de l'implementeur, coherent avec le principe "greybox d'abord" deja applique en 3.4.

Selection du type de degat sans repetition : au reset/creation de la voiture, tirer un ordre aleatoire des 3 types (roue/moteur/frein) ; appliquer le prochain de la liste a chaque seuil de 33 HP cumules franchi (67/34/1 HP restants) -- garantit l'absence de repetition sans etat de "types deja vus" a synchroniser separement.

Resurrection : reutiliser le motif deja etabli par `NetworkedVehicleRecoveryIntent` (RPC client -> host validee par identite/etat) plutot qu'un nouveau design d'interaction ; proximite minimale + etat `Downed`/`Alive` valides cote host avant application.

VFX de degat vehicule : le prefab `Greybox_PlayerCar` est un mesh unique sans roues separees -- simuler le "vacillement roue" par une legere oscillation de rotation (bruit/sinusoide, amplitude faible) sur le corps du vehicule plutot que sur une roue dediee, purement cosmetique et cote client. Fumee (moteur) et etincelles (frein) : `ParticleSystem` standard Unity positionnes aux emplacements approximatifs sur le mesh existant. Les trois effets sont independants et s'activent/desactivent un a un selon leur flag respectif -- jusqu'a 3 actifs simultanement sans interaction entre eux.

## Verification

**Commands:**
- `unity command --project-path D:\Projets\RRS recompile --focus false` puis `recompile_status` -- attendu : `completed`, `failed=false`, `errors=[]`.
- `unity command --project-path D:\Projets\RRS run_tests --mode EditMode --filter RoadRage.Tests.EditMode.Story35VehicleDamageHookAndTeamWipeContractStubTests --filter_type testName --async_tests true`, puis `test_status` -- attendu : tous les tests verts, 0 echec, 0 skip.
- `git diff --check` -- attendu : aucune erreur reelle (avertissements CRLF/YAML Unity standard tolerables).

**Manual checks (if no CLI):**
- Lancer `MVP_Run` en solo et en session multijoueur locale (Multiplayer Play Mode) : percuter un batiment pour reduire l'HP joueur et voiture, verifier l'ejection/plafond de vitesse a 0 HP joueur, la resurrection par un coequipier, l'expiration de fenetre avec depense de heart, et l'inconduisibilite + ejection totale a 0 HP voiture.
- Verifier que chaque effet VFX (fumee/etincelles/vacillement) s'active au bon seuil de degat et que les effets restent visibles simultanement lorsque plusieurs flags sont actifs.
