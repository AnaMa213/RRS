# Story 5.12 -- Tire Forces and Steering : note de livraison

Date : 2026-09-18. Spec : `_bmad-output/implementation-artifacts/spec-5-12-tire-forces-and-steering.md`.
Baseline : `68976ac7a315fe24529bfa8b4ba073d4bd6ec521`.

Ce document est la trace de livraison : ce qui a ete livre, ce qui a ete **mesure**, et ce qui reste
ouvert. Les mesures sont citees telles qu'elles sont sorties des commandes.

## 1. Ce qui est livre

- `Assets/RoadRage/Features/Vehicles/VehicleTireModel.cs` -- **nouveau**. Le modele de pneu en
  fonctions pures : glissement longitudinal et angulaire, part d'adherence transmise (montee lineaire
  jusqu'au pic, puis decroissance monotone vers une asymptote authoree strictement positive), force de
  pneu a budget unique (les deux glissements normalises par leur pic, une magnitude, une direction),
  attenuation basse vitesse, et le train roulant (couple moteur, couple de frein, integration de la
  rotation de roue bornee, remise a zero par le frein).
- `Assets/RoadRage/Features/Vehicles/VehicleSteeringModel.cs` -- **nouveau**. Angle de roue qui diminue
  avec la vitesse, taux de braquage et taux de rappel distincts, avancee a taux borne (jamais de saut),
  et application aux seules roues authorees directrices.
- `Assets/RoadRage/Features/Vehicles/VehiclePhysicsBody.cs` -- la couche physique possede desormais le
  **plan horizontal** : elle publie la charge portee par chaque roue, applique les efforts de pneu au
  point de contact, integre la rotation de chaque roue, et expose le point d'entree
  `ApplyDriveIntent(intent, maxForwardSpeed, steerRateDegreesPerSecond, brakeTorque)`. Le frottement de
  contact provisoire de la Story 5.11 disparait. `TryGetTireSample(int, out TireSample)` publie
  glissement, charge, force transmise et adherence disponible par roue.
- `Assets/RoadRage/Features/Vehicles/VehicleProfile.cs` + `VehicleProfileDef.cs` +
  `Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset` -- parametres de pneu, de
  direction, de frein a main et de train roulant, tous authores et valides par `TryValidate` qui nomme
  le champ refuse. `rollingResistanceCoefficient` disparait (le frein moteur `coastTorque` prend sa
  place fonctionnelle).
- `Assets/RoadRage/Features/Vehicles/VehicleDriveIntent.cs` -- voie `Handbrake`, sur le chemin d'intent
  existant : meme RPC, meme validation serveur, nom de RPC inchange.
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` -- **plus aucune ecriture de
  vitesse ni de rotation de caisse**. Le controleur lit une intention, l'inverse en marche arriere si
  besoin, et la soumet. Les trois resolveurs de degats de la Story 3.5 restent, et reduisent desormais
  l'AUTORITE de conduite (pointe, taux de braquage, couple de frein) au lieu d'une vitesse ecrite.
- `Assets/RoadRage/Features/Vehicles/LaneGraphRouting.cs` -- `ResolveLookAheadPoint` : point de visee
  anticipe, continu a la distance de visee, sans mutation de geometrie.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- `ComputeSeekIntent` vise
  le **point de visee anticipe** au lieu de la position du noeud, et ne decide plus lui-meme de
  l'arrivee (elle reste ou la decision se prend, `HasArrivedAtWaypoint`). Lacet impose, integration de
  vitesse et `RecoverAtWaypoint` **inchanges** : c'est la Story 5.14.
- `Assets/RoadRage/DevTools/VehiclePhysicsTelemetryView.cs` -- glissement, angle, charge, force et
  adherence par roue, plus l'angle de roue effectif.
- Tests : `Tests/EditMode/Story512TireForcesAndSteeringTests.cs` (24 gardes, **nouveau**),
  `Tests/PlayMode/Story512TireForcesAndSteeringPlayModeTests.cs` (2 preuves runtime, **nouveau**),
  `Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs` (rejeu raccorde aux fonctions pures
  reelles, plus le controle de bordure de 5.11), `Tests/EditMode/Story511VehicleChassisWheelsAndSuspensionTests.cs`
  (adaptee : frottement provisoire, champs du profil, refus de validation).
- Prefabs : meme composant physique et meme profil sur le joueur et l'IA, `lookAheadSeconds: 0.6` sur
  l'IA, bornes de vitesse retirees du controleur joueur (`maxForwardSpeed`, `acceleration`,
  `brakeDeceleration`, `coastDeceleration`, `lateralGrip`, `steerDegreesPerSecond`,
  `minimumSteerSpeed`).

## 2. Valeurs authorees, et ce qui les a produites

| Parametre | Valeur | Pourquoi |
| --- | --- | --- |
| `engineTorque` / `reverseTorque` | 900 / 700 N.m | effort au sol = couple / rayon de roue (0,33 m), soit ~2 700 N par roue motrice |
| `brakeTorque` / `coastTorque` / `handbrakeTorque` | 700 / 120 / 3 000 N.m | le frein a main doit BLOQUER les roues arriere : 3 000 depasse largement l'adherence disponible de l'essieu, la ou 700 freine sans bloquer |
| `wheelInertia` | 3 kg.m2 | integre la rotation de roue sans la rendre instantanee |
| `maxForwardSpeed` / `maxReverseSpeed` | 18 / 7 m/s | **les memes valeurs qu'avant**, desormais lues sur le profil : la pointe est approchee par une pente d'effort, jamais posee |
| `minimumDirectionSpeed` | 0,25 m/s | le seuil de changement de sens de la Story 3.2, conserve |
| `maxSteerAngleDegrees` / `highSpeedSteerAngleDegrees` / `steerFullReductionSpeed` | 32 / 10 / 18 | la direction reste lisible a vitesse elevee |
| `steerRateDegreesPerSecond` / `steerReturnRateDegreesPerSecond` | 180 / 140 | rappel plus lent que le braquage : relacher la direction n'est pas une remise sur rails |
| `tirePeakSlipRatio` / `tirePeakSlipAngleDegrees` / `tireSlipFalloffFraction` | 0,14 / 8 / 0,7 | pic a 14 % de glissement longitudinal et 8 deg d'angle, puis chute vers 70 % : la perte d'adherence est progressive et ne tombe jamais a zero |
| `lateralFrictionCoefficient` | 1,2 | **reutilise** comme adherence du pneu, pas renomme (aucune table par surface : cf. section 5) |
| `lookAheadSeconds` (IA) | 0,6 s | distance de visee = vitesse x duree, soit ~4,8 m a 8 m/s |
| Train roulant | essieu avant directrice, essieu arriere **motrice** | voir section 4 : c'est un changement de la donnee author e en 5.11 |

## 3. Mesures

- **Suite EditMode complete : 569 / 569 verts, 0 echec, 0 ignore** (`unity cmd run_tests --mode EditMode`).
  Baseline avant la story : 544. Delta = +24 (fixture 5.12) +1 (controle de bordure en 5.10).
- Fixture 5.12 seule : **24 / 24 verts** (`--filter RoadRage.Tests.EditMode.Story512TireForcesAndSteeringTests`).
- Fixture 5.10 seule (rejeu raccorde aux fonctions pures + controle de bordure) : **40 / 40 verts**.
- `graphify update .` : **3 089 nœuds / 7 352 liens / 128 communautes** (Story 5.11 : 2 949 / 7 014 / 129).
  Delta +140 nœuds, coherent avec les deux nouveaux fichiers de modele, les deux nouvelles fixtures et
  les fichiers modifies. Aucun saut de perimetre.
- **PlayMode : non mesurable par l'agent.** Le harnais est casse dans cette session
  (`run_tests --mode PlayMode` repond que le mode play ne peut pas s'executer en synchrone, et la voie
  asynchrone revient « completed » avec 0 test). Les deux preuves runtime livrees attendent donc une
  execution humaine dans le Test Runner de l'Editeur, exactement comme pour la Story 5.11, et doivent
  etre citees comme telles -- jamais presentees comme vertes par l'agent.

### Controle de bordure de la Story 5.11 : la mesure, et ce qu'elle ne peut pas dire

Le controle « les vehicules IA traversent le module sans toucher la bordure en conduite nominale »
n'avait aucune assertion. Il est desormais mesure par rejeu de la cinematique reelle du driver, depuis
chaque nœud qui entoure le carrefour, avec la largeur du vehicule (2,06 m) comme marge.

Balayage complet des couples (rayon d'arrivee, duree de visee) -- pas passes dans l'emprise d'une
bordure, sur 20 trajectoires qui atteignent toutes une sortie :

| Rayon d'arrivee | visee 0 s | 0,3 s | 0,6 s (livree) | 1,0 s |
| --- | --- | --- | --- | --- |
| 3,0 m | 104 | 104 | **104** | 102 |
| 2,5 m | 126 | 126 | 132 | 102 |
| 2,0 m | 154 | 154 | 152 | 142 |
| 1,5 m | 180 | 190 | 176 | 156 |
| 1,0 m | 198 | 208 | 202 | 190 |

Deux lectures, et une conclusion :

1. La visee anticipee est **neutre** sur ce chiffre (104 sans, 104 avec) : elle ne degrade pas le
   controle, ce que le test garde desormais comme non-regression mesuree -- et elle change bien les
   trajectoires dans le district, ce que le meme test verifie pour qu'il ne mesure pas deux fois la
   meme chose.
2. Le chiffre **empire** quand le rayon d'arrivee diminue : la correction « reduire le rayon
   d'arrivee » que l'AC prevoyait comme second recours est donc mesurablement la mauvaise direction.
3. Aucun couple ne ramene le chiffre a zero, y compris la poursuite point-a-point d'avant 5.12. Le
   rejeu est une cinematique pure -- vitesse constante, lacet borne, ni pneu, ni suspension, ni
   contact -- et il ne peut donc pas decider si un vehicule REEL touche une bordure. **Une assertion
   absolue ecrite sur cet instrument serait une garde qui ne garde rien** : elle forcerait un reglage
   mesure contre un modele qui ne represente pas la voiture.

L'absolu reste donc tenu par l'observation humaine en Play Mode, comme la Story 5.11 l'avait
enregistre, et le point est consigne dans `deferred-work.md` avec sa condition de reouverture. Le test
EditMode, lui, garde ce qui est verifiable : trajectoires non vides, sortie atteinte par toutes,
visee anticipee reellement exercee, et pas de degradation du degagement.

## 4. Constats, et ce qui demande une confirmation humaine

- **Train roulant : l'essieu moteur a change de place.** La donnee author e en 5.11 portait
  `isDriven` sur l'essieu **avant** (traction), mais le drapeau etait inerte : rien ne le lisait. La
  5.12 le consomme et l'a porte sur l'essieu **arriere** (propulsion), ce qui est le montage qui donne
  un sens au frein a main : ce sont les roues qui ne braquent pas qui se bloquent, et une propulsion
  dont l'arriere decroche entre en derive. **C'est un changement de comportement souhaite, pas un
  detail** -- a confirmer a la recette, et reversible en une valeur dans le profil si la traction est
  preferee (l'AC ne tranche pas le montage).
- **Etat intermediaire, nomme et date.** L'IA ecrit encore sa vitesse en bloc et impose son lacet par
  `MoveRotation` : c'est la Story 5.14 qui le supprime, et le commentaire d'en-tete d'
  `ApplyMovement` porte la date et la story qui le leve. La couche physique est deja la seule et la
  meme pour les deux vehicules -- c'est elle que la 5.14 consommera. Consequence assumee : les
  vehicules IA ne peuvent toujours pas etre pousses (entree `deferred-work.md` : les deux passages
  d'observation sont faits, l'entree reste ouverte sous la 5.14).
- **La porte Console de `validate.ps1` est rouge pour la session entiere.** `validate.ps1 -TestMode
  EditMode` a echoue sur 5 erreurs Console, toutes **historiques** : une commande Pipeline rejetee
  (`m_LocalRotation`), trois appels a une API de pneu intermediaire corrigee pendant l'implementation
  (`ResolveTireForce` / `ResolveCombinedForces`), et une reference de test corrigee dans la meme
  session. Le script appelle `console --level error` **sans `--since`** et rejoue donc tout le journal.
  La mesure de repli prevue par les regles de build a ete utilisee : `unity cmd run_tests --mode
  EditMode` + lecture du rapport, soit **569/569 verts**. Une session d'Editeur neuve remettrait la
  porte au vert sans rien changer au code.
- **Un fichier de test a ete ecrit puis corrige** : la premiere version de la fixture 5.12 utilisait un
  type d'acces inexistant et une garde de source trop large (`"Rage"` matchait le namespace
  `RoadRage`, `"isKinematic"` matchait le predicat de sol, `"linearVelocity"` matchait une LECTURE
  legitime dans l'etape de conduite). Les quatre gardes sont desormais des jetons precis -- le piege
  est le meme que celui deja documente pour les gardes de texte.

## 5. Ce qui reste ouvert

- **Controle de bordure absolu** : tenu par l'observation humaine en Play Mode (section 3),
  condition de reouverture ecrite dans `deferred-work.md`.
- **Surfaces de roulage differenciees** : le coefficient d'adherence est un **parametre** des fonctions
  de pneu, author e une seule fois, et aucune seconde surface n'est authoree -- le report de la
  course correction reste valide tel quel, et son point d'ancrage est en place.
- **Aides arcade (Story 5.13)** : rien n'a ete ajoute ici. Stabilite en lacet, controle de traction et
  recuperation de tete-a-queue sont le perimetre de la 5.13, et la couche physique ne lit ni rage ni
  peur ni disposition. Les valeurs a regler en 5.13 existent deja dans le profil (pic de glissement,
  chute, adherence, couples).
- **Preuves runtime** : a executer par l'humain (section 3).

## 6. Procedure de recette humaine (Editeur, `MVP_Run`, hote)

1. Accelerer depuis l'arret, freiner jusqu'a l'arret, puis tourner a vitesse de conduite. Attendu : la
   voiture tourne par ses roues, l'angle de braquage diminue avec la vitesse, la roue revient au
   centre au relachement, aucune remise sur rails.
2. Frein a main en virage. Attendu : les roues arriere se bloquent, l'arriere decroche, et la
   recuperation est progressive au relachement. Lire glissement, force et adherence par roue dans la
   vue de telemetrie.
3. Carrefour central : observer les vehicules IA en conduite nominale. Attendu : aucun contact avec
   `Col_Curb_*`. C'est la seule mesure de ce point qui fait foi.
4. Test Runner : lancer `Story512TireForcesAndSteeringPlayModeTests` **et** `Story511VehicleChassisWheelsAndSuspensionPlayModeTests`, et rapporter les resultats bruts. Le banc 5.11 est a re-executer : ses seuils portent desormais sur une couche dont le frottement de contact a ete remplace par le modele de pneu, et son moteur impose la vitesse horizontale -- partage qui n'est plus celui du jeu.
5. Garde de double etat : toute vue de telemetrie ajoutee a la main doit etre retiree avant de sauver
   la scene ; `MVP_Run.unity` et `git status --short` reviennent a leur etat initial.

## 7. Revue du 2026-09-18 (etape 4 du cycle de build)

Couches actives : `blind-hunter` (toujours), `edge-case-hunter` (changement non trivial : physique,
IA de conduite, prefab, contrat partage), `verification-gap` (comportement observable modifie),
`security-review` (voie de frein a main ajoutee a un RPC existant). Resultat : **aucun constat de
frontiere de confiance** (garde serveur, `SenderClientId` et permissions de `NetworkVariable`
verifies inchanges) ; les autres couches ont produit des constats, tries et traites comme suit.

**Corriges dans cette passe** (re-mesure apres correction : **573/573 EditMode verts**, soit +4 tests) :

- Le compte de roues AU SOL etait conditionne a une charge strictement positive : une caisse posee sur
  ses roues mais momentanement delestee aurait perdu la correction d'assiette -- exactement le
  correctif de la Story 5.11. Le compte redevenu independant de la charge.
- Deux entrees non finies encore acceptees par les fonctions pures (fraction de chute d'adherence,
  bornes d'angle de roue, direction de visee degeneree) sont refusees comme les autres.
- Le profil refuse desormais deux reglages casses : un essieu dont les deux roues ne partagent pas
  `isDriven` (couple de lacet permanent) et un profil sans aucune roue non directrice (frein a main
  silencieusement inerte).
- `wheelSpinAngle` etait ecrit a chaque pas et lu nulle part : etat mort supprime.
- Un prefab sans profil physique ignore desormais la conduite en le DISANT une fois, au lieu de rester
  silencieusement immobile.
- Les deux decisions du chemin joueur qui n'etaient gardees que par des assertions de texte --
  l'inversion de direction en marche arriere et la reduction d'autorite par degats (Story 3.5) -- sont
  devenues des **fonctions pures** (`ResolveSignedIntent`, `ResolveDriveAuthority`) et sont mesurees :
  la revue a montre que retirer l'inversion, neutraliser un multiplicateur ou supprimer l'intent neutre
  laissait les 569 tests verts.
- Le montage du train roulant (essieu moteur a l'arriere) et la duree de visee author ee sur le prefab
  IA sont desormais epingles : revenir a la donnee d'avant 5.12, ou mettre la duree a zero, fait
  echouer un test.
- Le commentaire du banc PlayMode 5.11 ne pretend plus que son moteur reproduit ce qu'ecrit le
  controleur, et la procedure de recette humaine l'inclut dans les bancs a re-executer.

**Reportes** (entrees ajoutees a `deferred-work.md`) : le saut de point de visee a l'avancee de noeud
(le vrai sujet est la conduite en carrefour, pas la visee seule -- la mesure montre le meme chiffre
avec et sans visee anticipee) ; la disparition de la resistance au roulement (le frein moteur ne
couvre que l'absence d'entree, et le district est plat) ; l'assertion de bordure qui passe par egalite,
qui ne distingue donc pas la visee livree de celle qu'elle remplace ; et les preuves runtime non
executees, qui dependent du harnais PlayMode.

**Rejetes** : frein a main binaire (l'entree est une touche, un dosage author e serait de la
configuration morte), glissement non publie pour une roue en l'air (aucune force a rapporter),
changement de format du RPC (tous les pairs executent le meme build, aucune exigence de compatibilite
inter-versions dans le perimetre).
