---
title: 'Story 5.41 -- Frontiere DrivingPolicy et protocole d''exception aux TrafficRules'
type: 'feature'
created: '2026-10-09'
status: 'done'
baseline_commit: '7795bf76a0a225a00a36fd771b699ca9eed6d136'
review_loop_iteration: 0
context:
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-40-gridlock-detection-and-bounded-escalation.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le pilote V2 lit le `DriverProfile` authore en direct. Aucune frontiere ne dit ce qu'un conducteur accepte de tenter (risque, creneau, surfaces, manoeuvres, couts). Aucune autorite ne peut accepter qu'il deroge a une TrafficRule. Or 5.42 (contournement par le corridor oppose) exige les deux.

**Approach:** Un `DrivingPolicy` pur resout la personnalite authoree en parametres effectifs, sans muter la definition. Il peut seulement *proposer* une `RuleExceptionRequest` explicite. Une `TrafficRuleAuthority`, possedee par le `JunctionCoordinator`, l'accepte ou la refuse dans le lot de la frame N. Elle publie l'exception effective a N+1 dans le `JunctionSnapshot`. Le pilote la porte sur sa commande et dans sa projection de debug.

## Boundaries & Constraints

**Always:**
- **P1, resolution.** `DrivingPolicy.Resolve(def, trafficId, seed)` rend une `EffectivePolicy` immuable :
  - `Driver` : le `DriverProfile` authore, soit vitesse desiree, T et s0 ;
  - `AcceptedRisk`, dans [0,1] ;
  - `AcceptedGapSeconds` ;
  - `RoutePreferenceWeight` ;
  - `AllowedSurfaces` ;
  - un cout et une eligibilite par `ManeuverKind`.

  Une manoeuvre est eligible si elle est authoree volontaire et si sa surface requise est permise. La fonction est pure, aucun etat partage.
- **P2, variation.** `EffectivePolicy.DesiredSpeedAt(seconds)` reutilise `DriverModel.ResolveNoisyDesiredSpeed`. La phase vient de `RoutePlanner.UnitDraw(seed, trafficId, "policy", 0, None)`. Le resultat est deterministe et borne par l'enveloppe de `Consistency`.
- **P3, requete.** Une `RuleExceptionRequest` nomme :
  - le demandeur ;
  - la `TrafficRule` ;
  - une portee (`RoadId` de corridor) ou une cible (`TrafficId`), au moins l'une ;
  - une raison de depart non vide ;
  - une terminaison : `ScopeExited` ou `Expiry`, toujours bornee par `ExpiryFrame`.

  `DrivingPolicy.TryPropose` ne la cree que si l'`EffectivePolicy` permet la surface liee a la regle.
- **P4, autorite.** Les requetes du lot N sont traitees dans un ordre stable. Chacune est acceptee ou refusee avec une raison stable :
  - `Malformed` : regle inconnue, ni portee ni cible, raison vide, ou expiration hors de ]N, N + `MaxRuleExceptionFrames`] ;
  - `RequesterAbsent` ;
  - `TargetAbsent` ;
  - `RuleNotViolable` : regle absente de `TrafficV2Settings.ViolableTrafficRules` ;
  - `ScopeInsideJunction` : portee qui n'est pas un corridor du modele ;
  - `AlreadyActive` : une seule exception par (demandeur, regle).

  Une exception effective prend fin, raison publiee, dans ces cas :
  - expiration ;
  - sortie de portee ;
  - demandeur ou cible absent ;
  - frame invalide (fail-closed).
- **P5, publication.** `JunctionSnapshot.RuleExceptions` publie chaque decision du lot : `Accepted`, `Held`, `Denied` ou `Ended`, avec ses frames source et effective. `TryGetEffectiveExceptions(trafficId, frameId)` ne rend des exceptions que si `EffectiveFrame == frameId`.
- **P6, pilote.** `TrafficV2VehicleDriver` resout son `EffectivePolicy` a chaque pas prepare, depuis le profil courant (amendement proprietaire du 2026-10-09, option 1). Il remplace toutes les lectures de `driverProfile.Profile` par `policy.Driver`, valeurs identiques. Il lit ses exceptions effectives dans l'instantane et les porte ensuite sur `MotionCommand` (`WithRuleExceptions`) et sur une ligne `Policy` de la projection de debug.

**Ask First:**
- Une fixture Story531 a Story540 rouge : HALT avant d'adapter une assertion.
- Toute regle ajoutee a `ViolableTrafficRules`, tout seuil, toute application runtime de la variation de vitesse : HALT.
- Toute modification de `MVP_Run`, du prefab V2 ou de Gate A : HALT.

**Never:**
- `DrivingPolicy` ne detecte aucune manoeuvre ni opportunite. Il ne lit ni frame, ni perception, ni trafic oppose, ni chemin, ni carrefour. Il ne choisit aucun creneau et ne construit aucun chemin : tout cela releve de 5.42.
- Il ne construit jamais d'exception effective et ne s'accorde rien.
- Aucune exception ne modifie un grant, un refus ou un occupant du coordinateur. Le lot de grants est identique avec ou sans requetes.
- Aucune exception n'est lue par le `SafetyFilter`. Aucune `SimulationInvariant` n'est representable comme `TrafficRule`.
- Aucun consommateur d'exception n'est livre ici : la manoeuvre releve de 5.42, la modulation Rage/Fear de 5.43. `RageDisposition` n'est jamais une entree.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Acceptation | Requete `OpposingCorridor`, portee : corridor ; lot N | `Accepted`, effective a N+1, visible dans la commande et la projection | N/A |
| Lecture decalee | Instantane de N lu a N+2 | Aucune exception effective | N/A |
| Regle non violable | `KeepClear` ou `JunctionControl` | `Denied RuleNotViolable` | Aucun effet |
| Portee carrefour | Portee : mouvement de carrefour | `Denied ScopeInsideJunction` | Grants inchanges |
| Requete implicite | Sans portee ni cible, ou raison vide | `Denied Malformed` | N/A |
| Fin | Expiration atteinte, ou portee quittee apres y etre entre | `Ended`, raison publiee | N/A |
| Frame invalide | `ResolveUnavailableFrame` | Toutes les exceptions `Ended FrameUnavailable` | Fail-closed |
| Doublon | Seconde requete meme (demandeur, regle) active | `Denied AlreadyActive` | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/DriverProfile.cs`, `DriverModel.cs:201` (`ResolveNoisyDesiredSpeed`, reutilise tel quel) et `DriverProfileDef.cs:45` (patron `CollisionReactionWeights` : struct authoree hors de `DriverProfile`, validee dans `TryValidate` `:156`). On y ajoute `DrivingPolicyProfile policy`.
- `Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset` : valeurs authorees explicites du bloc `policy`. Il est partage par le prefab V1, qui ne le lit pas.
- `Assets/RoadRage/Features/Vehicles/Traffic/Policy/DrivingPolicy.cs` (nouveau) : `ManeuverKind`, `DrivingSurface` [Flags], `TrafficRule`, `RuleExceptionRequest`, `RuleExceptionTermination`, `EffectivePolicy`, `DrivingPolicy` (P1-P3).
- `Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs:522` : `UnitDraw`, seule source d'alea.
- `Assets/RoadRage/Features/Vehicles/Traffic/Junction/TrafficRuleAuthority.cs` (nouveau) : decision P4 et etat des exceptions actives. Il porte aussi `EffectiveRuleException` (constructeur `internal`, cree ici seulement) et `RuleExceptionRecord`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs` :
  - `Resolve` `:107` : parametre optionnel `ruleExceptions`, nul = comportement identique ;
  - `ResolveBatch` `:116` : appel de l'autorite apres `EscalateGridlocks` `:368`, avec la frame du lot pour la portee et la cible ;
  - `ResolveUnavailableFrame` `:387` : fin fail-closed ;
  - `Publish` `:921`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs:509` : `JunctionSnapshot` (P5), avec le meme patron que `Gridlocks` (copie, tri, `ToText`).
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs:169` : a cote de `GridlockEscalationTiers`, ajouter `ViolableTrafficRules = { OpposingCorridor }` et `MaxRuleExceptionFrames`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs` :
  - `PrepareStep` `:632` : resolution de la politique a chaque pas prepare ;
  - `:673` (composeur) et `:735` (`var driver`) : remplacer `driverProfile.Profile` ;
  - apres `SafetyFilter.Evaluate` `:960` : `WithRuleExceptions` (le filtre reconstruit une commande bornee, `SafetyFilter.cs:234`, et ne lit aucune exception) ;
  - projection `:1018-1024` : ajouter `WithPolicy`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionCommand.cs:11` : champ `RuleExceptions` (jamais nul) et `WithRuleExceptions`.
- `Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs:292` : `WithPolicy` et la ligne `Policy ... / exceptions ...` dans `ToText` `:308`.
- Lecture seule :
  - `Safety/SafetyFilter.cs:160`, qui ne lit aucune exception ;
  - `Lifecycle/TrafficV2StepRunner.cs`, appel `Resolve` inchange (aucun producteur avant 5.42).
- Patrons de test :
  - `Tests/EditMode/Story540GridlockTests.cs` : modele en memoire, lots, scans structurels ;
  - `Story535JunctionRulesTests.cs:189` (`CrossingSource`) et `:158` (`AssertGrantInvariant`) ;
  - `Story59ParameterizedDriverModelTests.cs:296-332` : bornes du bruit.

## Tasks & Acceptance

**Execution:**
- [x] `Traffic/Policy/DrivingPolicy.cs` : types et fonctions P1-P3.
- [x] `DriverProfileDef.cs` et `DriverProfileDef_Default.asset` : bloc `policy` authore et sa validation.
- [x] `Junction/TrafficRuleAuthority.cs` : P4.
- [x] `JunctionCoordinator.cs` et `JunctionRecords.cs` : branchement et P5.
- [x] `TrafficV2Composition.cs` : `ViolableTrafficRules`, `MaxRuleExceptionFrames`.
- [x] `MotionCommand.cs`, `TrafficDecisionProjection.cs`, `TrafficV2VehicleDriver.cs` : P6.
- [x] `Assets/RoadRage/Tests/EditMode/Story541DrivingPolicyTests.cs`, categories `[Core][Story541]`. Il couvre :
  - la matrice ;
  - P1, sans mutation du Def ni d'un autre vehicule ;
  - P2, determinisme et bornes, `Consistency` 1 exact ;
  - les grants identiques avec ou sans requetes, toutes decisions confondues ;
  - un `SafetyResult` identique avec ou sans exceptions sur la commande ;
  - les scans structurels : `DrivingPolicy.cs` ne reference ni `TrafficFrame`, ni perception, ni planification, ni carrefour, ni `EffectiveRuleException`, ni autorite ; `SafetyFilter.cs` ne reference aucune exception ;
  - `Policy` present dans `ToText`.

**Acceptance Criteria:**
- Given une personnalite authoree, when ses parametres effectifs sont resolus, then le resultat contient la vitesse desiree, la distance de suivi, le risque accepte, les couts de route et de tactique, les surfaces permises et l'eligibilite par manoeuvre. La definition authoree n'est pas mutee. Le resultat est deterministe et borne pour une meme graine, et ne depend d'aucun autre vehicule.
- Given une politique qui veut deroger a une regle, when elle agit, then elle soumet une requete explicite, et seule l'autorite du coordinateur la decide et la publie versionnee par frame. Aucune exception ne leve un grant, une `SimulationInvariant` ni le `SafetyFilter`.
- Given l'implementation de la politique, when elle est revue, then elle ne contient ni detection de manoeuvre, ni selection de creneau, ni construction de chemin, ni evaluation du trafic oppose.
- Given aucune requete, when les fixtures Story531 a Story540 tournent, then le comportement est identique et `MVP_Run` est inchange.

- **2026-10-09 -- Amendement proprietaire (option 1) pendant l'implementation.**
  - **Declencheur.** `Story535JunctionPlayModeTests.ScenarioGGap...` est passe au rouge (6/7 PlayMode, « aucun GrantedMergeGap »). La fixture remplace par reflexion le `driverProfile` du titulaire apres son spawn. Avec une resolution unique a `Bind`, ce profil lent etait ignore.
  - **Amendement.** P6 passe de « une fois a `Bind` » a « a chaque pas prepare ». C'est le comportement exact de l'ancienne lecture de `driverProfile.Profile`. Aucune fixture existante n'est modifiee.
  - **KEEP.** Une garde structurelle de Story541 impose que la resolution reste dans `PrepareStep`.
  - **Ecart corrige en ecriture.** Les exceptions sont portees sur la commande apres le `SafetyFilter`. Le filtre reconstruit une commande bornee et ne lit aucune exception.

- **2026-10-09 -- Revue.**
  - **Couches actives.** `blind-hunter` (en ligne), `edge-case-hunter` et `verification-gap`. `security-review` est inactive : aucune frontiere reseau n'est deplacee. Aucun `intent_gap` ni `bad_spec`, donc aucune boucle.
  - **Correctifs appliques (patch) :**
    - une cible egale au demandeur est refusee `Malformed` ;
    - neuf cas prouvent que `DrivingPolicyProfile.TryValidate`, et `DriverProfileDef.TryValidate` a travers lui, refusent une politique invalide ;
    - le scan du pilote verifie aussi `TryGetEffectiveExceptions(insertion.TrafficId, frameId, ...)`, `WithRuleExceptions(ruleExceptions)` et `WithPolicy(policy, ruleExceptions)`.
  - **Differe (`deferred-work.md`).** La preuve runtime du cablage P6 attend le premier producteur de demandes (5.42) ou la Gate D.
  - **Rejetes :**
    - records par defaut et elements nuls : entrees non produites par le code ;
    - localisation perdue apres l'entree : choix teste, l'expiration borne ;
    - demandes sur frame invalide : ce chemin n'en recoit pas ;
    - phase arrondie a 1,0f : sans effet sur la variation ;
    - absence de validation dans `Resolve` : l'authoring est valide et aucun consommateur n'existe avant 5.42.
  - **Couverture du critere 4.** Les fixtures 5.31, 5.33, 5.34, 5.36, 5.37 et 5.38 n'ont pas ete rejouees. Elles relevent des suites de fin d'epic (risque accepte du 2026-09-29).
  - **Validation finale brute.** 0 erreur Console sur chaque fenetre, compilation saine, `MVP_Run` propre.

    | Story | EditMode | PlayMode |
    |---|---|---|
    | 5.41 | 34/34 | -- |
    | 5.40 | 21/21 | -- |
    | 5.35 | 51/51 | 7/7 |
    | 5.39 | -- | 4/4 |

    5.35 et 5.39 ont ete rejouees apres l'option 1.

## Design Notes

Decisions proposees a l'approbation :
- **D1, pas de variation au runtime.** `DesiredSpeedAt` est livre et prouve, mais le pilote garde `policy.Driver.DesiredSpeed`. Avec `Consistency` = 0,8 dans l'asset, l'appliquer changerait la conduite V2 sans preuve PlayMode, que la story exclut. Il sera branche avec la modulation de 5.43.
- **D2, regles violables.** Une seule est authoree : `OpposingCorridor`, utile a 5.42. `KeepClear` reste interdite (grant cyclique, 5.40 D1). Les regles de controle et de feu attendent un consommateur (5.43 et 5.44).
- **D3, valeurs authorees par defaut** (conducteur normal) :
  - `AcceptedRisk` 0,3 ;
  - `AcceptedGapSeconds` 4 ;
  - `RoutePreferenceWeight` 1 ;
  - surfaces : `Carriageway` et `OpposingCorridor` ;
  - manoeuvres volontaires, cout 1 : `CorridorOffset`, `AdjacentCorridor`, `OpposingCorridor` ;
  - `AuthorizedSurface` : non volontaire.

  `MaxRuleExceptionFrames` = 500 pas, soit 10 s a 50 Hz.
- **D4, placement.** `DrivingPolicy` est dans `Policy/`, l'autorite dans `Junction/` (Coordination, AD-39). Les deux partagent l'assembly `RoadRage.Features.Vehicles` : la separation tient au constructeur `internal` d'`EffectiveRuleException` et au scan structurel.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -Profile Story -Story 5.41 -TestMode EditMode` : expected `VALIDATION STORY`, compte execute egal au compte attendu, 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Story -Story 5.40 -TestMode EditMode`, puis `-Story 5.35 -TestMode Both` et `-Story 5.39 -TestMode PlayMode` : expected verts (coordinateur et pilote sans changement de comportement).
- `git status --short` : expected aucun fichier `MVP_Run` ni prefab modifie.

## Suggested Review Order

**Autorite des regles (seule a decider)**

- Point d'entree : decision stable par demande, raisons P4 dans l'ordre.
  [`TrafficRuleAuthority.cs:175`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/TrafficRuleAuthority.cs#L175)

- Fins d'abord (absence, expiration, sortie de portee), puis demandes triees.
  [`TrafficRuleAuthority.cs:112`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/TrafficRuleAuthority.cs#L112)

- Fail-closed : une frame invalide met fin a toute exception.
  [`TrafficRuleAuthority.cs:165`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/TrafficRuleAuthority.cs#L165)

- Constructeur internal : seule l'autorite cree une exception effective.
  [`TrafficRuleAuthority.cs:47`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/TrafficRuleAuthority.cs#L47)

- Branchement apres toute decision de grant ; rien sans demande ni exception active.
  [`JunctionCoordinator.cs:378`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs#L378)

- Lecture versionnee : seulement a EffectiveFrame, sans allocation sinon.
  [`JunctionRecords.cs:574`](../../Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionRecords.cs#L574)

**Politique de conduite (propose, n'accorde rien)**

- Resolution pure : profil authore copie, phase tiree de (graine, vehicule).
  [`DrivingPolicy.cs:273`](../../Assets/RoadRage/Features/Vehicles/Traffic/Policy/DrivingPolicy.cs#L273)

- Proposition seulement si la surface liee a la regle est permise.
  [`DrivingPolicy.cs:307`](../../Assets/RoadRage/Features/Vehicles/Traffic/Policy/DrivingPolicy.cs#L307)

- Eligibilite = volonte authoree et surface requise permise.
  [`DrivingPolicy.cs:179`](../../Assets/RoadRage/Features/Vehicles/Traffic/Policy/DrivingPolicy.cs#L179)

- Variation livree mais non appliquee au runtime (D1).
  [`DrivingPolicy.cs:186`](../../Assets/RoadRage/Features/Vehicles/Traffic/Policy/DrivingPolicy.cs#L186)

**Pilote V2 (P6)**

- Option 1 : politique resolue a chaque pas prepare, comme l'ancienne lecture du profil.
  [`TrafficV2VehicleDriver.cs:659`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L659)

- Exceptions portees apres le SafetyFilter, qui n'en lit aucune.
  [`TrafficV2VehicleDriver.cs:975`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L975)

- Ligne Policy de la projection de debug.
  [`TrafficV2VehicleDriver.cs:1036`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L1036)

- Copie de commande portant les exceptions, jamais nulles.
  [`MotionCommand.cs:64`](../../Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionCommand.cs#L64)

- Copie immuable de projection avec politique et exceptions.
  [`TrafficDecisionProjection.cs:305`](../../Assets/RoadRage/Features/Vehicles/Traffic/Debug/TrafficDecisionProjection.cs#L305)

**Donnees authorees**

- Regles derogeables authorees : OpposingCorridor seule (D2).
  [`TrafficV2Composition.cs:176`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs#L176)

- Bloc policy hors de DriverProfile, valide avec la definition.
  [`DriverProfileDef.cs:50`](../../Assets/RoadRage/Features/Vehicles/DriverProfileDef.cs#L50)

- Valeurs D3 explicites dans l'asset par defaut.
  [`DriverProfileDef_Default.asset:35`](../../Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset#L35)

**Preuves**

- Grants et refus identiques avec ou sans demandes d'exception.
  [`Story541DrivingPolicyTests.cs:355`](../../Assets/RoadRage/Tests/EditMode/Story541DrivingPolicyTests.cs#L355)

- Effective a N+1 seulement, jamais decalee ni pour un autre vehicule.
  [`Story541DrivingPolicyTests.cs:193`](../../Assets/RoadRage/Tests/EditMode/Story541DrivingPolicyTests.cs#L193)

- Verdict du SafetyFilter identique avec exceptions portees.
  [`Story541DrivingPolicyTests.cs:383`](../../Assets/RoadRage/Tests/EditMode/Story541DrivingPolicyTests.cs#L383)

- Scan : la politique ne planifie rien et n'accorde rien.
  [`Story541DrivingPolicyTests.cs:436`](../../Assets/RoadRage/Tests/EditMode/Story541DrivingPolicyTests.cs#L436)

- Politique authoree invalide refusee par la definition.
  [`Story541DrivingPolicyTests.cs:103`](../../Assets/RoadRage/Tests/EditMode/Story541DrivingPolicyTests.cs#L103)
