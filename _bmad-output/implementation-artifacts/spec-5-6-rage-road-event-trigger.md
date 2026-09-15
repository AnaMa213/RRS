---
title: 'Story 5.6 : declenchement de l''evenement Rage Road'
type: 'feature'
created: '2026-09-15'
status: 'done'
review_loop_iteration: 0
baseline_commit: '1025851ce1580b192fa34b2b23c75ed3272e66db'
context: ["{project-root}/_bmad-output/implementation-artifacts/epic-5-context.md"]
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Aucun evenement Rage Road n'existe en code. La rage d'une IA peut monter jusqu'a `ConfrontationCapable` (palier 95 de `RageTuningDef_Default`) et immobiliser le vehicule, mais cette escalade ne produit rien de materialise, de visible ni de host-owned : l'AD-22 definit un cycle de vie que personne n'ecrit.

**Approach:** Ajouter a `NetworkedRunState` (AD-17) un etat `RageRoadEventState` (AD-22 : `Idle -> Triggered -> Confrontation -> Resolved -> RewardGranted`) plus la reference de cible. Un composant `App/Run` evalue cote hote si une IA eligible a franchi la condition authorée (disposition `ConfrontationCapable`), arbitre en premier-arrive-gagne sur un evenement unique, et publie l'etat ; tous les pairs affichent etat et cible en texte. Epic 5 ne cable que `Idle -> Triggered`.

## Boundaries & Constraints

**Always:** L'etat vit dans `NetworkedRunState` (AD-17), ecriture serveur uniquement, jamais un champ local ni une seconde source de verite. Un seul evenement a la fois : tant que l'etat n'est pas `Idle`, aucune demande n'est acceptee (premier-arrive-gagne, AD-16). La condition n'a pas de seuil propre : c'est la disposition `ConfrontationCapable` deja derivee des paliers de `RageTuningDef` via `IRageDispositionSource` -- pas de seconde machine de rage. La cible est une `NetworkObjectReference` (AD-20), resolue par `AiRageTargetResolution.FindEligibleCandidates` (tri par `NetworkObjectId`, donc identique host/client). La logique pure (table de transitions, predicat de declenchement, premier candidat qualifie) vit dans `Features/Run` et ne depend que de `Shared.Domain` (`RoadRageScaffoldTests.cs:135-138`). Le retour joueur est textuel, jamais la couleur seule.

**Ask First:** toute valeur authorée nouvelle (seuil, magnitude ou priorite propres au Rage Road) ; toute transition au-dela de `Triggered` cablee a du gameplay ; toute modification de `MVP_Run.unity` autre que l'ajout du composant sur la racine du run.

**Never:** resolution de confrontation, zone de confrontation, on-foot, boss, recompense/argent (Epic 6). Controller de trafic, archetypes IA, ville/autoroute. Progression automatique de l'evenement. Retargeting silencieux ou reset automatique quand la cible est perdue. Mutation cote client. Nouvelle scene ou modification des Build Settings (`RoadRageScaffoldTests.cs:96-109`).

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Declenchement | Une IA eligible atteint `ConfrontationCapable`, etat `Idle` | Etat passe a `Triggered`, cible = cette IA, HUD texte + libelle `[RAGE ROAD]` | Aucune IA qualifiee : etat inchange, aucun message d'erreur |
| Deux IA au meme tick | Deux IA eligibles franchissent la condition la meme frame | La premiere par `NetworkObjectId` gagne ; un seul evenement | La seconde est ignoree, la cible d'origine reste |
| Declenchement duplique | La meme IA reste `ConfrontationCapable` apres `Triggered` | Aucun nouvel evenement, cible et etat inchanges | N/A (empeche par l'etat, pas par un drapeau par vehicule) |
| Cible perdue | Cible detruite ou despawnee pendant `Triggered` | Etat reste `Triggered`, le retour signale la cible perdue | Aucun reset automatique, aucun retargeting |
| Pair client | L'hote declenche l'evenement | Le client affiche le meme etat et la meme cible depuis les NetworkVariables | Aucune ecriture cote client |
| Aucune cible eligible | Aucune IA portant a la fois `NetworkedAIVehicleState` et une source rage/peur spawnee | Etat reste `Idle` | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Run/NetworkedRunState.cs:12-25` -- porte l'etat de run ; accueille `RageRoadEvent` + la reference de cible. Tout `NetworkVariable` public d'instance doit etre serveur-ecriture (`RoadRageScaffoldTests.cs:147-176`).
- `Assets/RoadRage/Shared/Domain/RunPhase.cs` -- gabarit du nouvel enum partage (namespace, taille, absence de logique).
- `Assets/RoadRage/Shared/Domain/RageDisposition.cs:5-10` -- `ConfrontationCapable = 5`, la condition de declenchement retenue.
- `Assets/RoadRage/Features/Run/RunCompositionRoot.cs` -- seul autre occupant de `Features/Run` ; aucune dependance vers une autre feature n'y est admise.
- `Assets/RoadRage/Features/Vehicles/AiRageTargetResolution.cs:22-44` -- `IsStructurallyEligible` / `FindEligibleCandidates` : eligibilite et tri deterministe deja ecrits, a reutiliser tels quels.
- `Assets/RoadRage/Features/Rage/NetworkedRageState.cs:29-33` -- `CurrentDisposition` via `IRageDispositionSource`, seule lecture de rage autorisee depuis une autre couche.
- `Assets/RoadRage/App/Run/RunFlowController.cs:948-953` -- garde d'autorite (`manager == null || !manager.IsListening || manager.IsServer`) a reproduire ; `:353` -- patron "resoudre `NetworkedRunState` par recherche de scene", sans cablage serialise.
- `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs:296-344` -- `EnsureRageLabel` / `EnsureIncidentLabel` : patron du label cree a l'execution (aucune edition de scene requise ; `rageLabel` est deja `{fileID: 0}` dans `MVP_Run.unity:4671`) ; `:210-225` -- `ShowRageStatus`, patron d'ecriture d'un label d'etat.
- `Assets/RoadRage/Features/Vehicles/AIVehicleBehaviorDebugView.cs:43-79` -- `SetLocalRageTargetLock` et `ComposeText` : patron du prefixe texte local a etendre.
- `Assets/RoadRage/Tests/EditMode/Story54RageDrivenAiBehaviorStatesTests.cs:213-252`, `Story55NetworkedAiRageTargetingTests.cs:456-508` -- doubles de test sans Netcode (`new GameObject` + `AddComponent`, jamais `.Spawn()`, aucun `NetworkManager`) et `SetPrivateField` par reflexion, recopie par fixture.
- `Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab:123,220` -- composition IA (`NetworkedAIVehicleState` + `NetworkedRageState` sur le meme objet) ; `NetworkedVehicleState` et `RouteWaypoints` en sont absents (references de scene) ; `MVP_Run.unity:3060-3082` -- racine `AITraffic` (`AIRoute` + 3 vehicules in-scene).

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Shared/Domain/RageRoadEventState.cs` (nouveau) -- enum `Idle / Triggered / Confrontation / Resolved / RewardGranted` (AD-22) -- vocabulaire unique partage par Run, UI et tests.
- [x] `Assets/RoadRage/Features/Run/RageRoadEventLifecycle.cs` (nouveau) -- `IsEventActive`, `CanAdvance` (avance d'un seul cran, table AD-22), `IsTriggerConditionMet(RageDisposition)` et `ResolveFirstTriggerIndex(etatCourant, dispositionsOrdonnees)` -- logique pure sans Netcode, arbitrage et anti-doublon en un point unique.
- [x] `Assets/RoadRage/Features/Run/NetworkedRunState.cs` -- ajouter `RageRoadEvent` (defaut `Idle`) et la `NetworkObjectReference` de cible, toutes deux serveur-ecriture -- etat host-owned exige par l'AC.
- [x] `Assets/RoadRage/App/Run/RageRoadEventFlowController.cs` (nouveau) -- cote hote : evaluer `FindEligibleCandidates()` + `CurrentDisposition`, ecrire `Triggered` + la cible, journaliser la demande refusee quand un evenement est actif ; sur tous les pairs : rafraichir le HUD, le marqueur de cible et le signalement de cible perdue -- un proprietaire unique, `RunFlowController` reste le hub HUD existant.
- [x] `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs` -- `ShowRageRoadEventStatus(...)` + `EnsureRageRoadEventLabel()` (label cree a l'execution sous le label incident, patron exact d'`EnsureIncidentLabel`), remis a zero dans `ShowPlaceholderHudValues` -- retour textuel sans edition de scene.
- [x] `Assets/RoadRage/Features/Vehicles/AIVehicleBehaviorDebugView.cs` -- `SetRageRoadEventTarget(bool)` et prefixe `[RAGE ROAD]` dans `ComposeText` -- identifier la cible de l'evenement meme hors du cone de visee, sans couleur.
- [x] `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- ajouter le composant sur la racine du run (celle qui porte `RunFlowController` et dont `RunCheckpointHud` est enfant), sans toucher aux Build Settings -- point d'entree inspectable et unique.
- [x] `Assets/RoadRage/Tests/EditMode/Story56RageRoadEventTriggerTests.cs` (nouveau) -- couvrir la matrice I/O : declenchement, arbitrage du premier par `NetworkObjectId`, anti-doublon, table `CanAdvance`, cible perdue sans reset, `Idle` quand rien ne qualifie -- `public sealed class`, namespace `RoadRage.Tests.EditMode`, doubles sans Netcode, `SetPrivateField` local, `[TearDown]` detruisant les objets crees. Les gardes textuelles passent par `CodeOnly(source)` : elles portent sur le code, pas sur les commentaires qui nomment la regle verifiee.
- [x] `.meta` pour chaque fichier `.cs` ajoute (`fileFormatVersion: 2` + `guid:`).

**Acceptance Criteria:**
- Given une IA eligible dont la rage atteint `ConfrontationCapable` et un evenement `Idle`, when l'hote evalue, then `NetworkedRunState` passe a `Triggered` avec cette IA comme cible, et l'etat comme le nom de la cible sont visibles en texte (HUD et prefixe `[RAGE ROAD]` du vehicule).
- Given un evenement actif (`Triggered` ou `Confrontation`), when une autre IA franchit la meme condition, then aucun second evenement n'est cree, la cible d'origine est conservee, et la demande refusee est journalisee cote hote (regle documentee : premier-arrive-gagne, un seul evenement actif).
- Given un evenement `Triggered`, when la meme IA reste au-dessus de la condition, then aucun nouvel evenement n'est produit.
- Given l'etat publie par l'hote, when un client l'observe, then il affiche le meme etat et la meme cible depuis les NetworkVariables et n'ecrit jamais dedans.
- Given la table AD-22, when on demande une transition, then seules les avancees d'un cran `Idle -> Triggered -> Confrontation -> Resolved -> RewardGranted` sont acceptees ; l'implementation ne cable que `Idle -> Triggered` et rend les suivantes testables sans logique Epic 6.
- Given un evenement `Triggered`, when la cible est detruite ou despawnee, then l'etat reste `Triggered`, le retour signale la cible perdue, et ni retargeting ni reset automatique n'ont lieu.

## Design Notes

- Condition derivee de `ConfrontationCapable` : les paliers de `RageTuningDef` sont la seule source de seuil existante (95 dans `RageTuningDef_Default.asset:16-31`) et l'Epic 5 interdit une seconde machine de rage ; un seuil Rage Road dedie dupliquerait cette authorite.
- Logique pure dans `Features/Run` et orchestration dans `App/Run` : `RoadRageScaffoldTests.cs:135-138` interdit toute reference `RoadRage.Features.*` entre features, donc `Features/Run` ne peut lire ni la rage ni l'eligibilite IA. `App/Run` est le seul assemblage qui voit Run, Rage et Vehicles.
- L'etat suffit comme anti-doublon : AD-16/AD-22 n'autorisent qu'un evenement, et un drapeau "deja declenche" par vehicule deviendrait une seconde source de verite a resynchroniser en fin de run.
- Cible perdue : l'etat reste `Triggered` plutot que d'inventer une resolution ; le reset appartient a la Story 5.8 et `Confrontation` a l'Epic 6.
- `Dev_RageSandbox` ne contient aucun vehicule IA de trafic ni `RouteWaypoints` : l'evenement n'y est pas exercable en l'etat. Ecart a traiter en Story 5.8, hors perimetre ici.

## Verification

**Commands:**
- Aucune commande CLI : verification par le Test Runner Unity (checkpoint humain).

**Manual checks (if no CLI):**
- `RoadRage.Tests.EditMode` filtre `Story56RageRoadEventTriggerTests`, puis suite complete verte (dont `RoadRageScaffoldTests`, qui valide les permissions des nouvelles NetworkVariables).
- `MVP_Run` en Play Mode, host + client : monter une IA a `ConfrontationCapable` (4 provocations passager a +25, ou klaxons a +15) et verifier l'etat `Triggered`, le nom de la cible sur le HUD, le prefixe `[RAGE ROAD]`, et l'etat identique cote client ; faire franchir la condition a une seconde IA et verifier qu'aucun second evenement n'apparait ; detruire ou despawner la cible et verifier que l'etat reste `Triggered` avec signalement de cible perdue.

## Suggested Review Order

**Declenchement et arbitrage (coeur de la story)**

- Point unique de decision host-side : evaluer, sinon refuser sans jamais creer de second evenement.
  [`RageRoadEventFlowController.cs:127`](../../Assets/RoadRage/App/Run/RageRoadEventFlowController.cs#L127)

- Arbitrage premier-arrive-gagne et anti-doublon portes par l'etat publie, sans drapeau par vehicule.
  [`RageRoadEventLifecycle.cs:63`](../../Assets/RoadRage/Features/Run/RageRoadEventLifecycle.cs#L63)

- Ecriture unique d'etat et de cible, dans cet ordre : un pair qui voit `Triggered` a deja la cible.
  [`RageRoadEventFlowController.cs:162`](../../Assets/RoadRage/App/Run/RageRoadEventFlowController.cs#L162)

- Condition derivee des paliers de `RageTuningDef` : aucune seconde machine de rage, aucun seuil dedie.
  [`RageRoadEventLifecycle.cs:49`](../../Assets/RoadRage/Features/Run/RageRoadEventLifecycle.cs#L49)

- Meme garde d'autorite que les autres chemins host-only d'`App/Run`.
  [`RageRoadEventFlowController.cs:121`](../../Assets/RoadRage/App/Run/RageRoadEventFlowController.cs#L121)

- Correctif de revue : un balayage de scene par frame aurait ete un cout permanent sur l'hote.
  [`RageRoadEventFlowController.cs:45`](../../Assets/RoadRage/App/Run/RageRoadEventFlowController.cs#L45)

**Etat host-owned (AD-17 / AD-22)**

- Les deux NetworkVariables serveur-ecriture : etat unique du run, et cible en `NetworkObjectReference`.
  [`NetworkedRunState.cs:29`](../../Assets/RoadRage/Features/Run/NetworkedRunState.cs#L29)

- Vocabulaire AD-22 partage par Run, UI et tests, volontairement sans logique.
  [`RageRoadEventState.cs:12`](../../Assets/RoadRage/Shared/Domain/RageRoadEventState.cs#L12)

- Table de transitions : avancees d'un seul cran, Epic 5 ne cable que `Idle -> Triggered`.
  [`RageRoadEventLifecycle.cs:33`](../../Assets/RoadRage/Features/Run/RageRoadEventLifecycle.cs#L33)

**Retour joueur textuel (jamais la couleur seule)**

- Ligne HUD dediee, creee a l'execution : aucune edition de scene n'etait necessaire.
  [`RunCheckpointHudScreen.cs:245`](../../Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs#L245)

- Presque identique a `EnsureIncidentLabel`, empilee sous la ligne incident.
  [`RunCheckpointHudScreen.cs:382`](../../Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs#L382)

- Prefixe `[RAGE ROAD]` local, re-derive de l'etat synchronise et jamais replique.
  [`AIVehicleBehaviorDebugView.cs:112`](../../Assets/RoadRage/Features/Vehicles/AIVehicleBehaviorDebugView.cs#L112)

- Correctif de revue : `Shutdown()` annule `SpawnManager` en laissant `Singleton` non nul.
  [`RageRoadEventFlowController.cs:238`](../../Assets/RoadRage/App/Run/RageRoadEventFlowController.cs#L238)

**Cablage de scene**

- Un composant, un seul, sur la racine du run qui porte deja `RunFlowController`.
  [`MVP_Run.unity:4093`](../../Assets/RoadRage/App/Scenes/MVP_Run.unity#L4093)

**Peripheriques (tests, script guides)**

- Couverture de la matrice I/O : declenchement, arbitrage, anti-doublon, cible perdue, cablage.
  [`Story56RageRoadEventTriggerTests.cs:32`](../../Assets/RoadRage/Tests/EditMode/Story56RageRoadEventTriggerTests.cs#L32)

- Garde d'unicite des ecritures : une seule ligne d'etat, une seule de cible dans le controleur.
  [`Story56RageRoadEventTriggerTests.cs:221`](../../Assets/RoadRage/Tests/EditMode/Story56RageRoadEventTriggerTests.cs#L221)

- Les gardes textuelles portent sur le code : trois faux rouges venaient des commentaires.
  [`Story56RageRoadEventTriggerTests.cs:392`](../../Assets/RoadRage/Tests/EditMode/Story56RageRoadEventTriggerTests.cs#L392)

- Verifie l'existence du composant sur `RunRoot` plutot que de faire confiance au diff de scene.
  [`Story56RageRoadEventTriggerTests.cs:324`](../../Assets/RoadRage/Tests/EditMode/Story56RageRoadEventTriggerTests.cs#L324)
