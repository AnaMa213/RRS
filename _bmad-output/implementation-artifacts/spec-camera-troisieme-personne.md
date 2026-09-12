---
title: 'Caméra troisième personne à pied et en voiture'
type: 'bugfix'
created: '2026-09-12'
status: 'in-progress'
baseline_commit: '6c529fccebe6160bf54097fdb326c502b1395c48'
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="Demande utilisateur du 12 septembre : corriger les caméras suivant le handoff et obtenir une troisième personne proche de GTA V dans les deux modes.">

## Intent

**Problème :** Les caméras véhicule enregistrées n'ont aucune action Look et leur orbite sphérique est réglée à 0,4 degré, près du sol. À pied, la caméra enfant tourne le corps avec la souris au lieu de permettre une observation indépendante.

**Approche :** Fournir une orbite troisième personne locale avec Cinemachine : voiture visible derrière et au-dessus, souris pour regarder autour, recentrage doux derrière la voiture ; à pied, vue légèrement décalée et mouvement relatif à la caméra avec personnage orienté vers sa marche. Réutiliser les composants natifs et partager la configuration utile entre les deux modes.

## Boundaries & Constraints

**Always :** Conserver les corrections et modifications non commitées du handoff et de la Story 4.3. Caméra et entrée strictement locales (AD-10), coordination par App/Run, gravité/endurance/mort et autorité réseau inchangées. Réglages de distance, hauteur, sensibilité modifiables. Utiliser Unity MCP pour les changements d'assets et l'inspection réelle. Préserver le verrouillage de cible T.

**Ask First :** Changement des règles de siège, du transport réseau ou ajout d'une dépendance.

**Never :** Répliquer l'orbite, modifier sprint-status, ajouter klaxon/sièges/multivéhicule réseau, refaire la locomotion ou les animations. Ne pas prétendre reproduire toute la caméra propriétaire de GTA V.

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs` : activation solo/réseau des quatre CinemachineCamera, suppression à la sortie, override LookAt T. Préserver les API appelées par RunFlow. Corriger aussi l'initialisation des caméras pour qu'aucune caméra distante ne puisse prendre la vue.
- `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` : OrbitalFollow Sphere, rayon 6, pitch 0.4 degré, TargetOffset zéro, WorldSpace, recentrage désactivé, InputAction null. Les positions Transform manuelles sont écrasées par le pipeline. Quatre slots existants à préserver si suffisants ; pas besoin d'une refonte de prefabs.
- `Assets/RoadRage/Features/OnFoot/LocalOnFootController.cs` : AttachCamera parente actuellement la Main Camera au joueur, ApplyLook tourne le corps, ApplyMovement utilise le corps. MovementEnabled est aussi utilisé pour mort/sièges ; ne pas confondre suspension locomotion et ownership caméra. Step sert aux tests d'intention.
- `Assets/RoadRage/App/Run/RunFlowController.cs` : AttachCamera au spawn ; RefreshLocalSeatMode/TryEnterLocalSoloVehicle/RestoreOnFootCamera et sorties/mort gèrent les transitions. SynchronizeLocalSoloSeatedPose déplace le joueur en voiture. SetFocusedRageTarget peut repositionner la caméra de debug sans joueur.
- `Assets/RoadRage/Shared/Presentation/` : endroit possible d'un petit utilitaire de configuration Cinemachine réellement partagé ; références asmdef Cinemachine/InputSystem à ajouter seulement aux assemblies qui l'utilisent.
- `Library/PackageCache/com.unity.cinemachine@f53aa49b3acc/Runtime/` : source locale canonique des API OrbitalFollow, InputAxisController, Deoccluder. Deoccluder peut protéger la ligne de vue via PullCameraForward et UseFollowTarget pour le lock T. Ne pas ignorer Untagged : sol et voiture partagent cette étiquette.
- `Assets/InputSystem_Actions.inputactions` : réutiliser Look ou une action locale explicitement activée ; prendre en compte la souris delta sans dépendre du framerate.
- `Assets/RoadRage/Tests/EditMode/Story32DriverControlAndLocalCameraTests.cs` : impose caméra véhicule enfant inactive ciblant racine. `Tests/PlayMode/Story15EmptyMapEntryPlayModeTests.cs:78` impose ancien parent Main Camera : adapter à la nouvelle propriété si détachement nécessaire.

## Tasks & Acceptance

**Execution :**
- [ ] `Shared/Presentation/`, `Features/OnFoot/LocalOnFootController.cs` et leurs asmdef : orbite Cinemachine à pied, input local, déplacement relatif à la vue sans faire pivoter le corps à l'arrêt.
- [ ] `Features/Vehicles/LocalVehicleCameraRig.cs`, `Prefabs/Greybox_PlayerCar.prefab` : corriger cadrage et input des quatre sièges, recentrage, horizon stable et protection contre obstacles via composants natifs ; réutiliser réglages communs quand cela simplifie.
- [ ] `App/Run/RunFlowController.cs` : assurer une seule caméra commandante lors des entrées/sorties solo et réseau, sans Main Camera déplacée par son parent joueur pendant les blends.
- [ ] `Tests/EditMode/ThirdPersonCameraTests.cs` et tests existants concernés : vérifier orbite et input, déplacement relatif à la vue, cadrage des sièges, activation/sortie, verrouillage T et obstacle. Préférer tests comportementaux aux assertions de texte source.
- [ ] Vérifier compilation, tests pertinents et inspection en jeu avec profil confirmé ; mettre à jour Graphify après les sources.

**Acceptance Criteria :**
- Étant donné un joueur à pied, quand il regarde sans marcher, alors la vue orbite et le corps garde son orientation ; quand il marche, alors le déplacement suit la vue horizontale et le corps suit le déplacement.
- Étant donné chaque siège local, quand il est occupé, alors la voiture est cadrée derrière et au-dessus, l'orbite souris fonctionne, et une autre voiture inactive ne prend pas la vue.
- Étant donné une orbite décalée en voiture, quand l'entrée cesse, alors elle se recentre progressivement derrière le véhicule sans transmettre de données réseau.
- Étant donné une entrée puis sortie ou changement de siège, quand la transition se termine, alors une seule vue locale contrôle la Main Camera, sans cadrage au sol ni parent qui entraîne la caméra.
- Étant donné un obstacle solide entre cible et caméra, quand il obstrue la vue, alors la caméra se rapproche sans ignorer tous les objets Untagged.

## Design Notes

Point de départ ajustable : distance voiture environ 7,5 m, cible environ 1,2 m, pitch environ 12–17 degrés ; à pied environ 3,5 m, hauteur buste et léger décalage épaule. Ne pas cumuler deux lecteurs de souris. Les quatre sièges partagent le cadrage arrière ; garder l'orbite au changement de siège si possible. Les changements existants sont explicitement repris sur demande utilisateur, sans nettoyage du reste du dépôt.

## Verification

- Compilation Unity et tests EditMode ciblés, puis suite existante si contrats transversaux modifiés.
- Inspection Play Mode via profil confirmé, activation API des sièges et rendu caméra ; ne pas assimiler absence de frames MCP à absence de spawn. Tester une entrée souris par API InputSystem ou test existant, sans simulation native fragile.
- `graphify update .` après les dernières modifications source.

## Spec Change Log

