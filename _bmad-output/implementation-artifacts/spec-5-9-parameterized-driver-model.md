---
title: 'Story 5.9 : Modele de conduite parametre (IDM/MOBIL)'
type: 'feature'
created: '2026-09-15'
status: 'done'
review_loop_iteration: 0
baseline_commit: '8db4a95d5bd0ba686590b502831bf772fa018abe'
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le style de conduite des vehicules IA se resume aujourd'hui a `ResolveCruiseSpeedMultiplier` (`NetworkedAIVehicleDriverController.cs:200`), qui pilote le seul axe `v0` avec des constantes figees dans le controleur. La rage ne peut donc changer que la vitesse, jamais la maniere de conduire, et l'AD-33 interdit exactement cette forme.

**Approach:** Porter le socle IDM (acceleration longitudinale) et MOBIL (decision de changement de voie) tels que specifies par la recherche du 2026-09-15, sous forme de fonctions pures dans une classe statique dediee, alimentees par une struct de parametres authoree dans un `Def` ScriptableObject. Le controleur devient un integrateur : il lit le profil, detecte un leader minimal, integre l'acceleration et evalue le changement de voie a un intervalle authore et desynchronise. Le profil porte 11 parametres, pas 8 : aux huit de la recherche s'ajoutent le temps de reaction, l'intervalle d'evaluation des changements de voie et la regularite, pour que deux archetypes puissent partager la meme vitesse desiree et se sentir neanmoins tres differents a conduire.

## Boundaries & Constraints

**Always:**
- Les huit parametres IDM/MOBIL (temps inter-vehiculaire `T`, ecart minimal `s0`, acceleration max `a`, deceleration confortable `b`, vitesse desiree `v0`, politesse `p`, seuil de changement `a_th`, freinage impose acceptable `b_safe`) plus trois parametres de personnalite (`reactionTime`, `laneChangeEvaluationInterval`, `consistency`) vivent dans le `Def`, jamais en constante de controleur.
- Acceleration longitudinale, decision de changement de voie, lissage de reaction et bruit de vitesse desiree sont des fonctions pures statiques, testables en EditMode sans scene ni Netcode. Elles retournent toujours une valeur finie.
- Le critere de securite MOBIL oppose son veto independamment du gain calcule.
- `laneChangeEvaluationInterval` remplace tout intervalle fixe cable en dur : chaque profil porte le sien, la desynchronisation par instance reste inchangee.
- `reactionTime` lisse l'acceleration appliquee vers l'acceleration IDM calculee (loi de premier ordre) -- jamais un tampon d'historique : c'est le mecanisme le plus simple qui reste un predicat pur testable sans etat suppose entre appels autre que le scalaire deja porte par le controleur.
- Le bruit de `consistency` ne perturbe que la vitesse desiree effective, jamais l'acceleration, la deceleration ou l'ecart -- deterministe (phase par instance, aucun `Random`), borne par une enveloppe fixe, et nul quand `consistency` vaut 1.
- Chaque vehicule ne lit que son propre etat : aucune collection statique partagee entre instances.

**Ask First:**
- Toute modification du predicat de blocage / recuperation de la Story 5.2 (`IsStuck`, `RecoverAtWaypoint`) : la Story 5.10 doit les remplacer, pas cette story.
- Tout changement du mode d'application du mouvement (passage de `linearVelocity` a des forces ou couples physiques). **Leve par la course correction du 2026-09-18 : ce changement est desormais le perimetre de la Story 5.12 (Tire Forces and Steering).**

**Never:**
- Aucun graphe de voies, portail, ou candidat de voie reel (Story 5.10) : MOBIL est livre comme fonction pure plus minuteur cable sur une liste de candidats vide.
- Aucune perception elargie multi-vehicules, index spatial ou arc authore (Story 5.17, ex-5.12) : la detection de leader reste une detection avant minimale, explicitement provisoire.
- Aucune modulation continue par les jauges de rage/peur (Story 5.19, ex-5.13) : la re-expression se limite aux dispositions deja publiees dans `NetworkedAIVehicleState.Behavior`. Le bruit de personnalite est orthogonal a l'emotion et se compose apres elle, jamais a sa place.
- Aucun tampon d'historique de reaction (file de valeurs passees horodatees) : le lissage de premier ordre couvre l'intention sans etat suppose.
- Aucune amplitude ou periode de bruit authoree par profil : l'enveloppe est une constante partagee dans `DriverModel` pour cette story ; seul `consistency` varie par profil.
- Aucun bruit applique a un autre axe que la vitesse desiree (pas de gigue de direction, d'ecart ou de freinage).
- Aucun reglage de valeurs par mesure : les valeurs authorees sont un point de depart (recherche R3), pas une calibration.
- Aucun repli code en dur si le `Def` est absent : le vehicule reste inerte, comme quand la route est absente.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Route libre | aucun leader, vitesse < `v0` | acceleration positive, tendant vers 0 quand la vitesse approche `v0` | N/A |
| Vitesse atteinte | aucun leader, vitesse = `v0` | acceleration ~ 0, jamais positive | N/A |
| Leader a l'arret | ecart > 0, vitesse du leader = 0 | deceleration franche et finie | N/A |
| Ecart nul | ecart = 0 | deceleration bornee et finie, jamais Inf ni NaN | ecart clampe a un epsilon avant division |
| Profil absent | `Def` non assigne | vehicule inerte, avertissement emis une fois | aucun repli numerique en dur |
| MOBIL gain suffisant, securite OK | gain > `a_th`, acceleration du nouveau suiveur >= `-b_safe` | changement accepte | N/A |
| MOBIL gain suffisant, securite KO | gain > `a_th`, acceleration du nouveau suiveur < `-b_safe` | changement refuse | veto prioritaire sur le gain |
| Politesse negative | `p` < 0 | le gain augmente quand le changement penalise autrui | N/A |
| Gating du changement | evaluation appelee a chaque `FixedUpdate` | au plus une evaluation par `laneChangeEvaluationInterval` du profil, phase initiale propre a chaque instance | N/A |
| Disposition immobilisante | `Block` ou `ConfrontationCapable` | `v0` effectif = 0, le vehicule s'arrete | la detection de blocage ne declenche pas de recuperation |
| Lissage de reaction | acceleration IDM cible constante sur plusieurs pas | l'acceleration appliquee converge vers la cible a une vitesse gouvernee par `reactionTime`, jamais un depassement (overshoot) | N/A |
| Reaction quasi instantanee | `reactionTime` proche du plancher autorise | l'acceleration appliquee suit la cible de tres pres des le premier pas | valeur clampee a un plancher positif, jamais 0 |
| Conducteur parfaitement regulier | `consistency` = 1 | bruit de vitesse desiree nul a tout instant | N/A |
| Conducteur erratique | `consistency` = 0 | bruit borne par l'enveloppe maximale authoree, jamais au-dela | N/A |
| Determinisme du bruit | memes `consistency`, phase et instant | meme valeur de bruit a chaque appel | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs:41` -- `cruiseSpeed` serialise a retirer ; `:124` appel du multiplicateur ; `:151-156` branche d'immobilisation ; `:180-181` calcul de l'intent puis `ApplyMovement` ; `:200-217` `ResolveCruiseSpeedMultiplier` a supprimer ; `:265-285` `ApplyMovement` ecrit `linearVelocity` en conservant la composante verticale (`+ verticalVelocity`, epingle par test) ; `:321-344` `CacheComponents`.
- `Assets/RoadRage/Features/Rage/RageTuningDef.cs:16-48,140-210,269-275` -- gabarit exact a copier pour le nouveau `Def` : `CreateAssetMenu`, champ `id` minuscule stable, `DefinitionId Id`, `TryValidate(out string)`, `OnValidate` qui log un avertissement. Le `Def` de conduite n'a pas besoin de catalogue : aucun appelant ne fait de lookup par id avant les Stories 5.19 (ex-5.13) / 5.16.
- `Assets/RoadRage/Features/Vehicles/VehicleDriveIntent.cs` -- unite de commande existante, conservee.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs:30` -- `Behavior`, seule entree de disposition consommee par la modulation.
- `Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab:197-208` -- bloc `NetworkedAIVehicleDriverController` sans aucun champ serialise ecrit : ajouter la reference au `Def` ici, les trois instances de scene en heritent.
- `Assets/RoadRage/App/Scenes/MVP_Run.unity:3437` -- racine `AITraffic` ; trois instances du prefab IA, chacune avec sa route.
- `Assets/RoadRage/Tests/EditMode/Story52BasicAiRouteFollowingAndRecoveryTests.cs:114-125` -- garde `Occurrences(source, "static ") == 4` a mettre a jour, message compris.
- `Assets/RoadRage/Tests/EditMode/Story54RageDrivenAiBehaviorStatesTests.cs:54-90,135-165` -- trois tests de profil appelant `ResolveCruiseSpeedMultiplier` : a reexprimer sur le profil effectif, memes proprietes (escalade lisible, `Block` / `ConfrontationCapable` immobilisent, `Calm` = reference).
- `RoadRage.Features.Vehicles.asmdef` -- ne reference pas `Features.Rage` : la disposition passe par `Shared.Domain.RageDisposition`.
- `_bmad-output/planning-artifacts/research/technical-trafic-ia-vehicules-unity-6-2026-09-15/research.md:110,121-128,280,290,292` -- formules, tableau des leviers, gating 1 s desynchronise, mise en garde R3 sur les valeurs.

## Tasks & Acceptance

**Execution:**
- [x] `Features/Vehicles/DriverProfile.cs` -- creer la struct `[Serializable]` des onze parametres (huit IDM/MOBIL + `reactionTime`, `laneChangeEvaluationInterval`, `consistency`) avec accesseurs en lecture seule -- unite transportee entre le `Def`, la modulation et les fonctions pures.
- [x] `Features/Vehicles/DriverProfileDef.cs` -- creer le `Def` (id minuscule stable, profil, `TryValidate`, `OnValidate`) sur le gabarit `RageTuningDef` -- toutes les valeurs deviennent authorees.
- [x] `ScriptableObjects/Vehicles/DriverProfileDef_Default.asset` -- creer l'asset par defaut avec les valeurs de depart de la recherche -- point de reglage unique.
- [x] `Features/Vehicles/DriverModel.cs` -- creer la classe statique pure : IDM (`ComputeAcceleration`), MOBIL (`TryEvaluateLaneChange` avec veto de securite), `ResolveEffectiveProfile(profil, disposition)`, `SmoothAcceleration(courante, cible, reactionTime, dt)`, `ShouldEvaluateLaneChange(ecoule, intervalle)` et `ResolveNoisyDesiredSpeed(v0, consistency, temps, phase)` -- sort les decisions du controleur, donc du `MonoBehaviour`.
- [x] `Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- retirer `cruiseSpeed` et `ResolveCruiseSpeedMultiplier` ; serialiser le `Def` ; integrer l'acceleration lissee en vitesse ; detection de leader avant minimale ; minuteur de changement de voie sur `laneChangeEvaluationInterval` avec phase initiale par instance -- le controleur n'est plus qu'un integrateur.
- [x] `Prefabs/Greybox_AIVehicle.prefab` -- assigner le `Def` par defaut -- les trois vehicules de `MVP_Run` heritent du profil sans edition de scene.
- [x] `Tests/EditMode/Story59ParameterizedDriverModelTests.cs` -- couvrir la matrice I/O, l'absence de constante de conduite dans le controleur et la disparition de `ResolveCruiseSpeedMultiplier` -- verification sans scene ni Netcode.
- [x] `Tests/EditMode/Story52BasicAiRouteFollowingAndRecoveryTests.cs` -- ajuster la garde de comptage des membres statiques -- la garde d'etat partage reste vraie apres deplacement des fonctions pures.
- [x] `Tests/EditMode/Story54RageDrivenAiBehaviorStatesTests.cs` -- reexprimer les trois tests de profil sur le profil effectif -- les proprietes de la Story 5.4 survivent au changement de mecanisme.
- [x] `Tests/PlayMode/Story59ParameterizedDriverModelPlayModeTests.cs` -- dans `MVP_Run`, les vehicules IA avancent le long de leur route sous le nouveau modele -- preuve d'integration exigee par AGENTS.md.
- [x] `docs/setup/story-5-9-driver-model-notes.md` -- note de verification : valeurs authorees, provenance, ecarts -- les valeurs sont un point de depart, pas une mesure.

**Acceptance Criteria:**
- Given un vehicule IA avec un `Def` assigne, when la simulation tourne cote hote, then chacun des onze parametres provient du `Def` et aucune valeur de conduite n'est litterale dans le controleur.
- Given deux profils partageant la meme `v0` mais des `reactionTime`, `laneChangeEvaluationInterval` et `consistency` differents, when ils sont integres sur les memes stimuli, then leurs trajectoires (vitesse instantanee, cadence de recherche de changement de voie) divergent de maniere mesurable.
- Given la suite EditMode, when elle s'execute, then `ResolveCruiseSpeedMultiplier` n'existe plus dans la base de code et aucun chemin ne conserve un reglage de conduite exprime uniquement en vitesse.
- Given `MVP_Run` en Play Mode, when la scene tourne, then les trois vehicules IA parcourent leur route sans regression visible de la Story 5.2 (recuperation retournement, hors-zone et blocage intactes).
- Given un vehicule dont la disposition passe a `Block` ou `ConfrontationCapable`, when le profil effectif est resolu, then sa vitesse desiree effective est nulle et la detection de blocage ne declenche pas de recuperation.
- Given l'evaluation de changement de voie, when elle est appelee a chaque `FixedUpdate`, then elle ne s'execute qu'une fois par intervalle authore, et deux vehicules crees au meme instant ne l'evaluent pas sur la meme frame.

## Spec Change Log

- **2026-09-15 -- Revue risk-scaled (patch) : tampons NonAlloc a taille fixe sans marge de securite.**
  Declencheur : revue de risque sur le diff complet -- `IsAnyPlayerWithinClearanceRadius`
  (`Collider[16]`, `OverlapSphereNonAlloc` sur 40 m) et `TryDetectLeader` (`RaycastHit[8]`,
  `SphereCastNonAlloc`) peuvent voir leurs resultats tronques sans ordre garanti des que le nombre
  de colliders dans la zone depasse la taille du tampon -- ni crash ni exception, juste un leader ou
  un joueur proche silencieusement absent du resultat.
  Densite verifiee : `MVP_Run` ne porte que 9 colliders au total aujourd'hui, donc aucun des deux
  seuils ne peut deborder dans le perimetre jouable actuel de cette story. Le risque est reel mais
  differe : la Story 5.10 (district greybox : routes, trottoirs, batiments) et les Stories 5.16 / 5.22
  (ex-5.17, ~30 vehicules simultanes) rapprocheront la densite reelle de ces tailles, et un depassement futur
  serait silencieux -- exactement la classe de defaut la plus couteuse a diagnostiquer.
  Amende (patch, aucune decision humaine requise) : tampons marges tres au-dela de la densite
  actuelle (`RaycastHit[8]` -> `[32]`, `Collider[16]` -> `[48]`), et avertissement Console une seule
  fois par vehicule si un tampon sature -- un depassement futur devient bruyant plutot que silencieux.
  Corrige au passage : un commentaire XML pour `TickLaneChangeEvaluation` s'etait retrouve orphelin
  au-dessus de `IsAnyPlayerWithinClearanceRadius` lors de l'edit precedent (deux blocs `<summary>`
  consecutifs) ; chaque methode porte de nouveau sa propre documentation.
  Etat connu-mauvais evite : une detection qui echoue silencieusement a l'echelle visee par l'epique
  (~30 vehicules, district multi-intersections), sans aucun signal permettant de le diagnostiquer.

- **2026-09-15 -- Ecart mesure depuis le centre de masse, pas depuis le pare-chocs.**
  Declencheur : test Play Mode humain -- en embouteillage les vehicules se tassent, se touchent et
  se poussent a vitesse residuelle au lieu de s'espacer ; face a un obstacle ils ralentissent mais
  ne s'arretent jamais et finissent par le penetrer.
  Cause racine : `TryDetectLeader` partait de `body.worldCenterOfMass`, donc `hit.distance` incluait
  la demi-longueur du vehicule (2,22 m pour un collider de 4,44 m). L'IDM cherche son equilibre a
  `s = s0 = 2 m` ; mesures depuis le centre, ces 2 m tombent 0,22 m A L'INTERIEUR du leader. Le
  point d'equilibre du modele etait physiquement dans l'obstacle : le vehicule poussait, la physique
  repoussait, d'ou le surplace colle. La valeur authoree `s0` etait correcte ; la mesure ne l'etait
  pas.
  Amende : le balayage part du pare-chocs (`frontOffset` lu sur le propre `BoxCollider`), et le
  centre de la sphere recule d'un rayon pour que son bord avant coincide avec le pare-chocs --
  `hit.distance` est alors l'ecart pare-chocs a pare-chocs qu'exige la definition de `s` dans l'IDM.
  Etat connu-mauvais evite : un `s0` authore qui ne correspond a aucune distance reelle, donc un
  modele impossible a regler -- toute tentative de correction serait passee par des valeurs
  compensatoires fausses.

- **2026-09-15 -- Rayon d'epaisseur nulle aligne sur le nez.**
  Declencheur : test Play Mode humain -- en virage, l'IA ne voit pas les obstacles en sortie de
  courbe et ne ralentit pas.
  Cause racine : un `Raycast` d'epaisseur nulle strictement aligne sur `transform.forward` ne couvre
  ni la largeur du vehicule ni la trajectoire courbe.
  Amende : `SphereCast` de rayon egal a 80 % de la demi-largeur du vehicule, dirige a mi-chemin
  entre le nez et le cap vise (`ResolveScanDirection`).
  KEEP : cela reste une detection AVANT MINIMALE a une seule cible -- aucun index spatial, aucun arc
  authore, aucune perception multi-vehicules. La couverture geometrique complete des virages reste
  la Story 5.17 (ex-5.12), qui remplacera ce balayage.

- **2026-09-15 -- Detection de leader aveugle aux pietons (decision Ask First, approuvee par l'humain).**
  Declencheur : test Play Mode humain -- un vehicule IA devant lequel le joueur se place a pied
  s'immobilise puis se teleporte devant lui apres 3 s.
  Cause racine : le joueur a pied est un `CharacterController` (`LocalOnFootController`), sans
  `Rigidbody` ; le filtre `hit.rigidbody == null -> continue` de la detection de leader l'excluait.
  L'IA ne freinait donc pas, elle l'encastrait, et la detection de blocage de la Story 5.2 la
  teleportait. Amende : (1) un `CharacterController` compte desormais comme leader ; (2) nouveau
  predicat pur `DriverModel.IsDeliberateStop` -- un arret derriere un leader a un ecart raisonnable
  n'alimente jamais la detection de blocage ; (3) `stuckSustainedSeconds` 3 s -> 60 s ; (4) la
  teleportation de recuperation est refusee tant qu'un joueur est dans
  `recoveryPlayerClearanceRadius` (40 m).
  Etat connu-mauvais evite : la contrainte d'epique « un vehicule bloque n'est jamais teleporte »
  etait violee a chaque freinage correct, soit exactement quand le modele fait son travail.
  KEEP : le palier de recuperation reste present tant que les Stories 5.18 (ex-5.11, echelle
  anti-blocage) et 5.17 (ex-5.12, deblocage progressif : klaxon puis contournement) ne l'ont pas
  remplace -- le retirer maintenant laisserait un vehicule reellement encastre bloque indefiniment. Le contournement propre
  n'est PAS livre par cette story.
  Correction de la spec : la ligne `Ask First` attribuait le remplacement de `IsStuck` /
  `RecoverAtWaypoint` a la Story 5.10 ; les proprietaires reels sont les Stories 5.14, 5.17 et 5.18
  (ex-5.11 / ex-5.12).

## Design Notes

- **Pourquoi une classe statique separee.** `Story52BasicAiRouteFollowingAndRecoveryTests` epingle le nombre de membres `static` du controleur comme garde d'absence d'etat partage entre vehicules. Loger IDM et MOBIL dans `DriverModel` conserve cette garde intacte (elle passe de 4 a 3) au lieu de la diluer, et donne aux fonctions pures un lieu testable sans `MonoBehaviour`.
- **Integrer plutot que reecrire la physique.** `ApplyMovement` continue d'ecrire `linearVelocity` en preservant la composante verticale : contrat epingle par la Story 5.2, car l'ecraser annulerait la gravite et rendrait la recuperation hors-zone inatteignable. Le modele fournit une acceleration ; le controleur maintient une vitesse par instance, `vitesse = clamp(vitesse + a * dt, 0, +inf)`, et la projette sur l'avant. Passer a des forces physiques est un autre sujet, explicitement `Ask First`.
- **Ecart nul.** L'IDM divise par l'ecart : celui-ci est clampe a un epsilon strictement positif avant la division, de sorte qu'un ecart nul produit une deceleration forte mais finie. Sans ce clamp, la matrice I/O produit `-Inf` puis un `linearVelocity` NaN qui contamine le `NetworkTransform`.
- **Re-expression des dispositions.** `ResolveEffectiveProfile` remplace le multiplicateur par une modulation du profil : la rage baisse `T` et `s0`, monte `a`, `b` et `v0`, baisse `p` et `a_th`, monte `b_safe` (tableau des leviers de la recherche). `Block` et `ConfrontationCapable` mettent `v0` effectif a 0. C'est la forme minimale de `effectif = base x f(...)` exigee par l'AD-33 ; la Story 5.19 (ex-5.13) remplacera l'entree discrete par les jauges continues, entre cette modulation et le bruit de personnalite (voir ordre de composition ci-dessous).
- **Phase du minuteur.** La desynchronisation vient de l'instance (`GetInstanceID()` ou equivalent stable), pas d'un tirage aleatoire : deterministe, donc testable. Le meme scalaire de phase sert au minuteur de changement de voie et au bruit de `consistency`.
- **Lissage plutot que tampon d'historique (reactionTime).** Un vrai modele a delai (file de valeurs passees horodatees) ajoute de l'etat et complique le test des cas limites. Une loi de premier ordre (`applique += (cible - applique) * (1 - exp(-dt / reactionTime))`) ne demande que le scalaire d'acceleration deja porte par instance, converge sans depassement et reste un predicat pur : `SmoothAcceleration(applique, cible, reactionTime, dt)`.
- **Ordre de composition : disposition puis bruit.** `effectifV0 = ResolveEffectiveProfile(...).DesiredSpeed`, puis `ResolveNoisyDesiredSpeed(effectifV0, consistency, temps, phase)` juste avant l'appel a `ComputeAcceleration`. La Story 5.19 (ex-5.13) remplacera l'etape de disposition par une modulation continue rage/peur sans toucher au bruit, qui reste une couche de personnalite orthogonale a l'emotion.
- **Enveloppe de bruit partagee.** `ponytail:` l'amplitude maximale et la frequence du bruit de vitesse desiree sont des constantes uniques dans `DriverModel` (par ex. +/-6% de `v0`, deux termes sinusoidaux de frequence fixe combines avec la phase par instance) ; seul `consistency` varie par profil pour cette story. Si le playtest demande un archetype avec un tremblement plus ou moins large que les autres, faire de l'amplitude et de la frequence des champs du `Def`.

## Verification

**Commands:**
- Aucune commande CLI : verification par le Test Runner Unity, checkpoint humain comme aux Stories 5.5 a 5.8.

**Manual checks (if no CLI):**
- Filtre `Story59ParameterizedDriverModelTests` dans `RoadRage.Tests.EditMode`, puis suite EditMode complete verte (Stories 5.2 et 5.4 incluses).
- Filtre `Story59ParameterizedDriverModelPlayModeTests` dans `RoadRage.Tests.PlayMode`.
- `MVP_Run` en Play Mode : les trois vehicules IA avancent, ralentissent derriere un obstacle place devant eux, et aucun `NaN` ni `Inf` n'apparait dans la Console.
- Inspecteur du prefab `Greybox_AIVehicle` : le champ de profil pointe `DriverProfileDef_Default`, aucun champ `cruiseSpeed` residuel.

## Suggested Review Order

**Modele de decision (IDM/MOBIL, fonctions pures)**

- Point d'entree : acceleration IDM, ecart clampe avant division pour rester toujours finie.
  [`DriverModel.cs:61`](../../Assets/RoadRage/Features/Vehicles/DriverModel.cs#L61)

- MOBIL : le veto de securite est evalue independamment du gain, jamais achete par lui.
  [`DriverModel.cs:103`](../../Assets/RoadRage/Features/Vehicles/DriverModel.cs#L103)

- Re-expression de la disposition (Story 5.4) comme modulation du profil, plus l'AD-33 en multiplicateur.
  [`DriverModel.cs:137`](../../Assets/RoadRage/Features/Vehicles/DriverModel.cs#L137)

- Lissage de premier ordre du temps de reaction, sans tampon d'historique.
  [`DriverModel.cs:162`](../../Assets/RoadRage/Features/Vehicles/DriverModel.cs#L162)

- Gating du changement de voie sur l'intervalle authore du profil.
  [`DriverModel.cs:183`](../../Assets/RoadRage/Features/Vehicles/DriverModel.cs#L183)

- Bruit de personnalite deterministe, borne, nul sur uniquement la vitesse desiree.
  [`DriverModel.cs:196`](../../Assets/RoadRage/Features/Vehicles/DriverModel.cs#L196)

- Arret voulu vs blocage (correctif Play Mode) : un ecart raisonnable derriere un leader n'est jamais un blocage.
  [`DriverModel.cs:225`](../../Assets/RoadRage/Features/Vehicles/DriverModel.cs#L225)

**Profil authore (Def)**

- Struct des onze parametres, huit IDM/MOBIL plus trois de personnalite.
  [`DriverProfile.cs:18`](../../Assets/RoadRage/Features/Vehicles/DriverProfile.cs#L18)

- `Def` sur le gabarit RageTuningDef : id stable, validation, avertissement OnValidate.
  [`DriverProfileDef.cs:55`](../../Assets/RoadRage/Features/Vehicles/DriverProfileDef.cs#L55)

**Integration controleur (le coeur du correctif Play Mode)**

- FixedUpdate : la boucle qui orchestre disposition, arret voulu, blocage et integration.
  [`NetworkedAIVehicleDriverController.cs:160`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L160)

- Cause racine du tassement en embouteillage : ecart mesure depuis le pare-chocs, pas le centre de masse.
  [`NetworkedAIVehicleDriverController.cs:399`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L399)

- Cause racine de la teleportation : un pieton (CharacterController) compte desormais comme leader.
  [`NetworkedAIVehicleDriverController.cs:437`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L437)

- Balayage volumique en virage : direction a mi-chemin entre le nez et le cap vise.
  [`NetworkedAIVehicleDriverController.cs:324`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L324)

- L'arret voulu court-circuite la detection de blocage ; la teleportation reste refusee sous les yeux d'un joueur.
  [`NetworkedAIVehicleDriverController.cs:236`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L236)

- Garde-fou visuel : proximite joueur avant toute teleportation de recuperation.
  [`NetworkedAIVehicleDriverController.cs:467`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L467)

- Demi-longueur et demi-largeur lues une fois sur le collider, jamais une valeur inventee.
  [`NetworkedAIVehicleDriverController.cs:356`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L356)

**Robustesse des requetes physiques (findings de revue, patch)**

- Tampons NonAlloc marges tres au-dela de la densite actuelle, saturation avertie plutot que silencieuse.
  [`NetworkedAIVehicleDriverController.cs:105`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L105)

- Avertissement de saturation du tampon de leader, une seule fois par vehicule.
  [`NetworkedAIVehicleDriverController.cs:409`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L409)

**Authoring et cablage**

- Profil par defaut assigne sur le prefab : les trois instances de MVP_Run en heritent sans edition de scene.
  [`Greybox_AIVehicle.prefab:209`](../../Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab#L209)

- Seuil de blocage releve a 60 s, dissocie d'un arret voulu derriere un leader.
  [`NetworkedAIVehicleDriverController.cs:91`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L91)

**Tests et non-regression**

- Suite dediee : matrice I/O complete plus les correctifs Play Mode (ecart, virage, pietons, tampons).
  [`Story59ParameterizedDriverModelTests.cs:45`](../../Assets/RoadRage/Tests/EditMode/Story59ParameterizedDriverModelTests.cs#L45)

- Preuve d'integration exigee par AGENTS.md : les vehicules IA avancent dans MVP_Run sous le nouveau modele.
  [`Story59ParameterizedDriverModelPlayModeTests.cs`](../../Assets/RoadRage/Tests/PlayMode/Story59ParameterizedDriverModelPlayModeTests.cs#L1)

- Garde d'etat partage de la Story 5.2 ajustee a trois membres statiques apres le deplacement vers DriverModel.
  [`Story52BasicAiRouteFollowingAndRecoveryTests.cs:123`](../../Assets/RoadRage/Tests/EditMode/Story52BasicAiRouteFollowingAndRecoveryTests.cs#L123)

- Les trois tests de profil de la Story 5.4 re-exprimes sur le profil effectif, memes proprietes d'escalade.
  [`Story54RageDrivenAiBehaviorStatesTests.cs:74`](../../Assets/RoadRage/Tests/EditMode/Story54RageDrivenAiBehaviorStatesTests.cs#L74)

- Provenance des valeurs authorees et ecarts assumes.
  [`story-5-9-driver-model-notes.md`](../../docs/setup/story-5-9-driver-model-notes.md#L1)
