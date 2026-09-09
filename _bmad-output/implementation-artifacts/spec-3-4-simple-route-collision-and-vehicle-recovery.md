---
title: 'Story 3.4 : Route simple, collisions et recuperation du vehicule'
type: 'feature'
created: '2026-09-09'
status: 'done'
review_loop_iteration: 0
context: []
baseline_commit: '64a02009681df47546724d3dd29b909bc86e9779'
---

<frozen-after-approval reason="human-owned intent - do not modify unless human renegotiates">

## Intent

**Problem:** La voiture partagee peut deja etre conduite et occupee, mais `MVP_Run` n'offre qu'un terrain minuscule (22x28) sans route, limites ni decor, et rien ne permet de recuperer la voiture si elle se retourne, sort de la zone jouable ou se coince -- Epic 5 (trafic IA) et Epic 6 (transition a pied) ont besoin d'un vrai terrain roulable pour s'y greffer.

**Approach:** Agrandir nettement `MVP_Run` (et le sandbox dev) avec une boucle routiere simple, des limites de zone jouable, quelques batiments/props en decor, un point de recuperation, une detection hote de retournement/sortie de zone avec recuperation automatique, une recuperation manuelle a la demande du conducteur, et un retour visuel minimal sur collision.

## Boundaries & Constraints

**Always:**
- Toute detection (retournement, hors-limites) et toute recuperation (repositionnement, remise a zero vitesse/rotation) sont calculees et appliquees cote host uniquement, coherent avec la simulation physique host-authoritative existante.
- La recuperation manuelle est une intention client validee cote host (emetteur = conducteur actuel du siege 0) avant application, suivant le meme schema RPC que les intentions de siege de la 3.3.
- Le module `RoadRage.Features.Vehicles` reste autonome (aucune reference a `Players`, `UI`, `Run`, etc.) ; toute liaison HUD/Run passe par un evenement C# expose et consomme cote `App/Run`.
- Les collisions avec l'environnement de route ne doivent jamais interrompre la session (pas d'exception, pas de freeze physique) ; seul un retour visuel/texte minimal est requis.
- La boucle routiere, les limites et les props utilisent uniquement des primitives greybox / assets deja adoptes (`Greybox_CityBlock_A`, `Prop_Barrel`, boites greybox) -- pas de nouvelle creation d'art.
- Le point de recuperation est un repere de scene fixe dans `MVP_Run`, pas une liste de waypoints ; dans une scene sans repere explicite (ex. `Dev_VehicleSandbox`), la position initiale du vehicule au demarrage sert de repli.
- Seul `MVP_Run` est agrandi et recoit la boucle/limites/props de cette story ; `Dev_VehicleSandbox` reste inchange (aucun agrandissement, aucune construction de boucle).

**Ask First:** Si la taille finale de la carte ou le trace de boucle propose semble exiger un nouvel asset de route/virage courbe plutot que des primitives greybox existantes, HALT et confirmer avant de construire.

**Never:**
- Ne pas implementer l'IA de trafic, Rage Road, les degats reels, l'economie ou le boss.
- Ne pas synchroniser la camera vehicule sur le reseau.
- Ne pas ajouter de detection "coince" automatique par minuteur (risque de faux positifs) -- la recuperation "coince" reste manuelle via l'action conducteur.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Retournement | Axe haut de la voiture ecarte de la verticale au-dela du seuil, soutenu N secondes | Host repositionne/reoriente la voiture au point de recuperation, vitesse/rotation a zero | N/A |
| Sortie de zone | Voiture sous le seuil de vide ou hors limites de la carte | Meme recuperation automatique cote host | N/A |
| Coince (manuel) | Conducteur assis appuie sur la touche de recuperation | RPC validee (emetteur = conducteur) declenche la meme recuperation host | Requete ignoree si l'emetteur n'est pas le conducteur actuel |
| Collision route/decor | Voiture percute un batiment/prop/limite | Retour visuel/texte minimal (HUD), session non interrompue | N/A |
| Solo hors-ligne | Aucun `NetworkManager` actif, conducteur local appuie sur la touche de recuperation | Le meme controleur local applique la recuperation sans passer par une RPC | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` -- ajouter detection retournement/hors-zone cote host (FixedUpdate existant L122), methode `RecoverVehicle(position, rotation)` reinitialisant `body` (position/rotation/velocite), override recuperation locale solo (miroir de `SetLocalSoloDriverActive` L226), `OnCollisionEnter` -> evenement C# de collision.
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleState.cs` -- aucune nouvelle `NetworkVariable` requise ; le repositionnement du `Transform`/`Rigidbody` deja replique suffit.
- `Assets/RoadRage/App/Run/NetworkedVehicleRecoveryIntent.cs` (nouveau) -- lit la touche de recuperation, envoie une RPC serveur validee contre le conducteur courant (siege 0), miroir du pattern `NetworkedVehicleSeatIntent.cs:49`.
- `Assets/RoadRage/App/Run/RunFlowController.cs` -- brancher la recuperation solo (miroir de `RefreshLocalSoloDeathRecovery` L730) et l'ecoute de l'evenement de collision reseau -> relais HUD.
- `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs` -- ajouter un retour texte minimal pour collision et recuperation.
- `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- agrandir `Greybox_GroundPlane`, construire une boucle routiere (segments greybox), limites de zone, instances `Greybox_CityBlock_A`/`Prop_Barrel`, repere `VehicleRecoveryPoint`.
- `Assets/RoadRage/App/Scenes/Dev_VehicleSandbox.unity` -- aucune modification ; la recuperation y retombe sur la position initiale du vehicule (pas de repere ni de boucle a construire).
- `Assets/RoadRage/Tests/EditMode/Story34SimpleRouteCollisionAndVehicleRecoveryTests.cs` (nouveau) -- tests structurels cibles (constantes de seuil, presence methode/evenement, frontiere asmdef, marqueurs de scene).

## Tasks & Acceptance

**Execution:**
- [x] `NetworkedVehicleDriverController.cs` -- detection retournement/hors-zone + `RecoverVehicle()` + evenement collision -- coeur de la recuperation host-authoritative.
- [x] `NetworkedVehicleRecoveryIntent.cs` (nouveau) -- touche de recuperation conducteur -> RPC validee -- entree manuelle "coince".
- [x] `RunFlowController.cs` -- brancher recuperation solo + relais collision -> HUD -- parite solo/reseau.
- [x] `RunCheckpointHudScreen.cs` -- retour texte minimal collision/recuperation -- visibilite sans nouvelle UI durable.
- [x] `MVP_Run.unity` -- boucle routiere agrandie, limites, batiments/props, repere de recuperation -- contenu jouable de la story (`Dev_VehicleSandbox.unity` non touche).
- [x] `Story34SimpleRouteCollisionAndVehicleRecoveryTests.cs` (nouveau) -- verification minimale ciblee.

**Acceptance Criteria:**
- Given la voiture partagee controlable, when la scene `MVP_Run` charge, then elle contient une boucle/route, des limites, des batiments/props et un point de recuperation.
- Given une collision avec l'environnement de route, when elle se produit, then un retour visuel minimal apparait et la session continue sans interruption.
- Given la voiture retournee ou hors zone jouable au-dela du seuil, when le host evalue l'etat, then elle est automatiquement repositionnee au point de recuperation.
- Given un conducteur coince, when il declenche la recuperation manuelle, then seule une requete emise par le conducteur actuel du siege 0 est acceptee et appliquee.
- Given le mode solo hors-ligne, when le joueur declenche la recuperation, then le meme comportement s'applique sans session reseau.
- Given le module Vehicules inspecte apres implementation, when ses references asmdef sont lues, then il ne reference toujours aucune autre feature slice.
- Given la route peut etre testee, when aucun trafic IA ni systeme Rage Road n'est present, then la boucle reste jouable et recuperable seule.

## Spec Change Log

- 2026-09-09 -- Renegociation humaine en cours d'implementation : `Dev_VehicleSandbox` ne doit pas etre agrandi ni recevoir de boucle/limites/props ; seul `MVP_Run` est concerne par le contenu de carte de cette story. Le point de recuperation par defaut retombe sur la position initiale du vehicule pour les scenes sans repere explicite. KEEP : tout le reste du spec (detection retournement/hors-zone, recuperation manuelle RPC, retour collision) est inchange.
- 2026-09-09 -- Review (patch, non bloquant) : `RecoverVehicle` repositionnait `body`/`transform` sans passer par `NetworkTransform.Teleport(...)`, ce qui aurait fait glisser visuellement la voiture (interpolation NGO) au lieu de la faire apparaitre instantanement chez les clients -- corrige par un appel `networkTransform.Teleport(position, rotation, transform.localScale)` dans `RecoverVehicle`. Deuxieme patch : `VehicleCollided`/`VehicleRecovered` n'etaient leves que localement (host/solo), donc le retour HUD n'apparaissait jamais chez un client non-host -- corrige par `NotifyClientsIfNetworked` + deux `[Rpc(SendTo.NotServer)]` (`NotifyVehicleCollidedRpc`/`NotifyVehicleRecoveredRpc`) qui relevent le meme evenement C# local chez chaque client. KEEP : le reste du contrat (garde d'autorite host/solo, point d'entree unique `RecoverAtRecoveryPoint`, aucune reference UI dans le module Vehicules) est inchange.

## Design Notes

Terrain vise : agrandir le sol de ~22x28 a un ordre de grandeur nettement plus grand (ex. ~120x120) avec une boucle rectangulaire simple (4 segments greybox + limites aux bords), suffisant pour "tester la conduite comme tranche de gameplay repetable" sans necessiter de virages courbes. Seuil de retournement : `Vector3.Dot(transform.up, Vector3.up) < seuil` soutenu quelques secondes (meme logique anti-faux-positif que `ApplyStabilityAssist`). Seuil hors-zone : reutiliser le meme motif de seuil de vide que `LocalVoidRespawnController`/`NetworkedPlayerLifecycleService`, applique a la position du vehicule plutot qu'au joueur.

## Verification

**Commands:**
- `unity command --project-path D:\Projets\RRS recompile --focus false` puis `recompile_status` -- OK, `completed`, `failed=false`, `errors=[]` (apres correctif du `using RoadRage.App.Run;` manquant dans le fichier de test).
- `unity command --project-path D:\Projets\RRS run_tests --mode EditMode --filter RoadRage.Tests.EditMode.Story34SimpleRouteCollisionAndVehicleRecoveryTests --filter_type testName --async_tests true`, puis `test_status` -- OK final apres les deux patchs de review (Teleport + relais RPC clients), 10/10 tests verts, 0 echec, 0 skip, duree 0.05 s.
- `git diff --check` -- OK ; seulement avertissements CRLF Windows et le motif YAML Unity standard (`m_Name: `, `m_Data: `, etc., deja present dans le fichier avant cette story), aucune erreur reelle. Le fichier TMP `LiberationSans SDF - Fallback.asset` modifie par un effet de bord d'atlas dynamique en session Play Mode a ete restaure (`git checkout --`) pour ne pas polluer le diff.
- `git status --short --branch` / `git diff --stat` -- OK ; changements limites aux fichiers de Story 3.4 avant commit final.

**Manual checks (if no CLI):**
- Lancer `MVP_Run` en solo : entrer dans la voiture, la faire percuter un batiment (retour visuel), la retourner volontairement (recuperation auto), declencher la touche de recuperation (recuperation manuelle).

## Suggested Review Order

**Detection et recuperation host-authoritative**

- Point d'entree unique de la recuperation (auto ou manuelle), retombe sur la position initiale si aucun repere de scene.
  [`NetworkedVehicleDriverController.cs:223`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L223)

- Retournement soutenu N secondes + sortie de zone, calcules chaque `FixedUpdate` cote host/solo uniquement.
  [`NetworkedVehicleDriverController.cs:179`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L179)

- Predicats purs (retournement, seuil de vide) reutilisant l'esprit anti-faux-positif deja en place.
  [`NetworkedVehicleDriverController.cs:206`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L206)

- Reset position/rotation/vitesse, toujours derriere la meme garde host/solo que le reste du fichier.
  [`NetworkedVehicleDriverController.cs:256`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L256)

**Snap reseau et relais clients (correctifs de revue)**

- `NetworkTransform.Teleport` evite que la voiture glisse visuellement chez les clients au lieu d'apparaitre instantanement.
  [`NetworkedVehicleDriverController.cs:274`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L274)

- Sans ce relais RPC, seul l'ecran du host aurait vu le retour collision/recuperation.
  [`NetworkedVehicleDriverController.cs:302`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L302)

- Collision route/decor : jamais d'exception, juste l'evenement C# + le relais client.
  [`NetworkedVehicleDriverController.cs:285`](../../Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs#L285)

**Intention manuelle "coince" (reseau et solo)**

- Meme patron RPC client -> host que le siege (Story 3.3) : requete rejetee si l'emetteur n'est pas le conducteur du siege 0.
  [`NetworkedVehicleRecoveryIntent.cs:86`](../../Assets/RoadRage/App/Run/NetworkedVehicleRecoveryIntent.cs#L86)

- Touche `R` reservee au conducteur assis ; en solo, appel direct sans Rpc.
  [`RunFlowController.cs:480`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L480)

**Pont Run/HUD**

- Abonnement paresseux unique aux evenements C# du module Vehicules, jamais l'inverse.
  [`RunFlowController.cs:259`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L259)

- Retour texte minimal, reutilise le label siege existant (pas de nouvelle UI durable).
  [`RunCheckpointHudScreen.cs:127`](../../Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs#L127)

**Contenu de carte**

- `MVP_Run` agrandi (~120x120), boucle 4 segments, limites, decor et repere de recuperation.
  [`MVP_Run.unity`](../../Assets/RoadRage/App/Scenes/MVP_Run.unity)

- `Dev_VehicleSandbox` volontairement non touche par le contenu de carte de cette story.
  [`Dev_VehicleSandbox.unity`](../../Assets/RoadRage/App/Scenes/Dev_VehicleSandbox.unity)

**Verification**

- Tests cibles couvrant detection, RPC, pont HUD, frontiere asmdef, contenu de scene et non-regression sandbox.
  [`Story34SimpleRouteCollisionAndVehicleRecoveryTests.cs:1`](../../Assets/RoadRage/Tests/EditMode/Story34SimpleRouteCollisionAndVehicleRecoveryTests.cs#L1)
