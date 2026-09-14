---
title: "Story 5.2 : suivi de route IA basique et recuperation"
type: "feature"
created: "2026-09-14"
status: "done"
review_loop_iteration: 0
baseline_commit: "3abf7e1f2bec946b444d1e4f52e69557d69e13f8"
context:
  ["{project-root}/_bmad-output/implementation-artifacts/epic-5-context.md"]
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** `NetworkedAIVehicleState` n'est qu'un squelette (`RouteIndex` seul) : aucun vehicule IA ne bouge, ne suit de waypoints, ni ne se recupere s'il est bloque, capote ou sort de la zone jouable.

**Approach:** Ajouter `WaypointIndex` a `NetworkedAIVehicleState`, une geometrie de waypoints en scene, et un controleur IA host-only qui calcule un intent de poursuite deterministe, deplace le Rigidbody, et reutilise les predicats capote/hors-zone existants pour se reinitialiser au waypoint courant sans RPC.

## Boundaries & Constraints

**Always:**

- Chaque vehicule IA reste independant, sans etat partage entre vehicules.
- `WaypointIndex` rejoint `RouteIndex` sur `NetworkedAIVehicleState` (server-write, lecture `Everyone`).
- Mouvement/recuperation host-side (`IsServer`) uniquement ; position repliquee via le `NetworkTransform` existant, sans nouvelle RPC.
- Poursuite et blocage sont des predicats statiques purs testables en EditMode sans Netcode (comme 3.4/5.1).
- `RoadRage.Features.Vehicles` ne reference aucune autre feature (garde `RoadRageScaffoldTests`).
- Trafic IA place dans `MVP_Run` (pas de nouvelle scene), avec un moyen de le desactiver/isoler sans affecter le vehicule joueur.

**Ask First:**

- Si "hors zone jouable" ne peut pas etre couvert par le predicat de hauteur de vide existant et exige une nouvelle geometrie de bornes.

**Never:**

- Etats pilotes par la rage, controleur de trafic complet, archetypes finaux, boss, evenement Rage Road -- reserves 5.4/5.6.
- Nouvelle RPC/intention validee pour le mouvement IA, ou modification de `NetworkedVehicleDriverController` au-dela de lire ses predicats statiques.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Behavior | Error Handling |
|----------|---------------|--------------------|-----------------|
| Poursuite | waypoint a distance > rayon d'arrivee | intent avance vers le waypoint, vitesse deterministe | N/A |
| Arrivee/boucle | distance <= rayon d'arrivee | `WaypointIndex` avance, boucle apres le dernier | N/A |
| Capote / hors zone / blocage | `up.y` sous seuil, ou `position.y` sous seuil de vide, ou vitesse quasi nulle, pendant N secondes | reinitialisation au waypoint courant | N/A |
| Independance | recuperation sur un vehicule | les autres restent inchanges | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs:10-17` -- squelette (`RouteIndex` seul) ; ajouter `WaypointIndex`.
- `Assets/RoadRage/Features/Vehicles/RouteWaypoints.cs` -- NOUVEAU : MonoBehaviour de scene, `Transform[]` ordonnes bouclants -- esprit du marqueur `VehicleRecoveryPoint` (3.4).
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- NOUVEAU : `ComputeSeekIntent`/`IsStuck` statiques pures (sortie `VehicleDriveIntent.cs:10-34`) ; `FixedUpdate` sous `IsServer` applique l'intent, reinitialise sur capote/hors-zone/blocage.
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs:245-254` -- lecture seule : `IsRolledOver`/`IsBelowVoidHeightThreshold` reutilises tels quels.
- `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` -- base a dupliquer en prefab IA leger (sans entree/camera joueur), cable avec les nouveaux composants + `NetworkTransform` existant.
- `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- scene d'integration : ajouter 3 instances IA + waypoints, avec un toggle desactivation/isolation.

## Tasks & Acceptance

**Execution:**

- [x] `Assets/RoadRage/Features/Vehicles/RouteWaypoints.cs` -- composant de waypoints -- geometrie reutilisable par le controleur IA.
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs` -- ajouter `WaypointIndex` -- progression separee de `RouteIndex`.
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- controleur host-only : poursuite, mouvement, recuperation -- coeur de la story.
- [x] `Assets/RoadRage/Tests/EditMode/Story52BasicAiRouteFollowingAndRecoveryTests.cs` -- couvrir la matrice I/O et l'independance inter-vehicules.
- [x] Prefab IA (Unity MCP) -- dupliquer `Greybox_PlayerCar.prefab` sans entree/camera joueur.
- [x] `Assets/RoadRage/App/Scenes/MVP_Run.unity` (Unity MCP) -- placer 3 instances IA + waypoints, avec toggle isolation/desactivation.

**Acceptance Criteria:**

- Given trois vehicules IA actifs dans `MVP_Run`, when la simulation demarre, then chacun suit ses waypoints a vitesse deterministe cote host.
- Given le trafic IA desactive, when la scene se charge, then aucun vehicule IA ne bouge.
- Given le diff complet, when on l'inspecte, then aucun etat de rage, controleur de trafic complet, boss ou archetype final n'apparait.

## Spec Change Log

- 2026-09-14 (revue, patch) -- `ApplyMovement` preserve desormais la composante verticale du Rigidbody
  (`+ verticalVelocity`), comme `NetworkedVehicleDriverController`. Etat connu-mauvais evite : la vitesse etait
  ecrasee en entier a chaque FixedUpdate, ce qui annulait la gravite -- le vehicule IA leviterait a sa hauteur de
  spawn et `IsBelowVoidHeightThreshold` ne pourrait jamais se declencher, rendant la ligne « hors zone » de la
  matrice I/O inatteignable. Garde de non-regression ajoutee dans la fixture EditMode.

- 2026-09-14 -- `RouteWaypoints` : ajout de `NearestIndex` (pur + instance) et repli sur les enfants directs
  quand le tableau `points` est vide. Motif : plusieurs vehicules partagent une meme boucle, il leur faut un
  waypoint de depart propre sans index cable par instance ; et la passerelle Unity MCP ne sait pas ecrire un
  `Transform[]` ni une reference de composant custom, donc le cablage par hierarchie remplace le cablage par
  Inspector. La reference `route` des trois vehicules a ete posee via `SerializedObject` (Unity_RunCommand).
- 2026-09-14 -- Toggle de desactivation du trafic : conteneur de scene `RunRoot/AITraffic` (waypoints +
  3 vehicules). Decocher ce seul GameObject coupe tout le trafic IA sans toucher au vehicule joueur -- pas de
  script de toggle dedie.

## Design Notes

LIMITE CONNUE (hors perimetre, tracee dans `deferred-work.md`) : le trafic IA ne roule qu'en lobby en ligne.
Le chemin solo (`AppSceneRouter.LoadMvpRun`) ne demarre aucune session Netcode, donc `IsServer` est faux et
`FixedUpdate` sort immediatement. Ce n'est pas specifique a cette story -- c'est vrai de tout `NetworkBehaviour`
du projet. Correction decidee le 2026-09-14 : story dediee « solo = host a un joueur », pas de contournement local.

"Hors zone jouable" reutilise le predicat de hauteur de vide existant plutot qu'une nouvelle geometrie de bornes. Integration directe dans `MVP_Run` (demande humaine, coherent avec AGENTS.md) ; isolation/desactivation par toggle in-scene, pas de scene Dev separee.

## Verification

**Commands:**

- Aucune commande CLI : verification par le Test Runner Unity (checkpoint humain).

**Manual checks (if no CLI):**

- `RoadRage.Tests.EditMode` filtre `Story52BasicAiRouteFollowingAndRecoveryTests`, puis suite complete verte.
- `MVP_Run` en Play Mode hote : 3 vehicules IA suivent leurs waypoints ; un capotage/blocage provoque prouve la reinitialisation ; le toggle desactive le trafic sans toucher le vehicule joueur.

## Suggested Review Order

**Boucle de decision host-only**

- Point d'entree : predicats de recuperation puis intent de poursuite, tout sous `IsServer`.
  [`NetworkedAIVehicleDriverController.cs:94`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L94)

- Intent de poursuite deterministe, fonction pure testable sans Netcode.
  [`NetworkedAIVehicleDriverController.cs:157`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L157)

- Mouvement applique au Rigidbody -- vitesse verticale preservee (correction post-revue, gravite sinon annulee).
  [`NetworkedAIVehicleDriverController.cs:197`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L197)

- Recuperation : teleporte au waypoint courant, sans RPC, remet les compteurs a zero.
  [`NetworkedAIVehicleDriverController.cs:224`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L224)

**Depart independant par vehicule**

- Au spawn, chaque IA choisit son waypoint de depart par proximite -- pas d'index cable en scene.
  [`NetworkedAIVehicleDriverController.cs:77`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L77)

- Predicat pur : plus proche repere en distance planaire.
  [`RouteWaypoints.cs:68`](../../Assets/RoadRage/Features/Vehicles/RouteWaypoints.cs#L68)

**Geometrie de route reutilisable**

- Repli sur les enfants directs quand le tableau serialise est vide -- contournement d'une limite de la passerelle Unity MCP.
  [`RouteWaypoints.cs:24`](../../Assets/RoadRage/Features/Vehicles/RouteWaypoints.cs#L24)

- Boucle apres le dernier repere, y compris index negatifs.
  [`RouteWaypoints.cs:89`](../../Assets/RoadRage/Features/Vehicles/RouteWaypoints.cs#L89)

**Etat reseau**

- `WaypointIndex` : ecriture serveur, lecture Everyone, separee de `RouteIndex`.
  [`NetworkedAIVehicleState.cs:21`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs#L21)

**Integration scene et tests**

- Conteneur `RunRoot/AITraffic` : 3 instances + route partagee -- decocher pour desactiver tout le trafic.
  [`MVP_Run.unity:3060`](../../Assets/RoadRage/App/Scenes/MVP_Run.unity#L3060)

- Couverture de la matrice I/O et garde de non-regression sur la vitesse verticale.
  [`Story52BasicAiRouteFollowingAndRecoveryTests.cs:1`](../../Assets/RoadRage/Tests/EditMode/Story52BasicAiRouteFollowingAndRecoveryTests.cs#L1)
