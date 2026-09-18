# Story 5.13 -- Arcade Assists and Uneven Ground : notes de livraison

Trace de livraison, sur le modele des notes 5.11 et 5.12. Mesures citees telles qu'elles sont
sorties des commandes.

## 1. Valeurs authorees retenues

Les aides sont des termes authorés sur `VehicleProfileDef_Default.asset`, chacun **desactivable par
sa propre valeur nulle** (patron deja en place dans la couche : `rate <= 0` rend le terme inerte).

| Parametre | Valeur | Pourquoi |
| --- | --- | --- |
| `yawStabilityRate` | 60 N.m par degre/s | abattement du lacet : s'oppose a la composante de lacet de la vitesse angulaire. C'est le terme qui manquait -- l'amortissement d'assiette projette la vitesse angulaire sur tangage et roulis **seulement**, le lacet n'y est jamais touche |
| `tractionControlStrength` | 0,5 | part du couple moteur d'une roue retiree quand son glissement depasse le pic authore. **Strictement inferieure a 1 par validation** : a 1, une roue en glissement total perdrait tout son couple, ce qui serait un seuil binaire deguise |
| `spinRecoveryRate` | 40 N.m par degre/s | couple de rappel au-dela du seuil de derive, **dirige du cote demande par le conducteur** : entree nulle, couple nul |
| `spinDriftThresholdDegreesPerSecond` | 30 deg/s | sous ce seuil, aucune recuperation ne se declenche : une caisse qui tourne normalement dans un virage ne doit rien sentir |
| `aimPointRecallSpeed` (profil conducteur) | 12 m/s | vitesse de rappel de la cible de visee. Au-dessus de la vitesse de croisiere authoree (8 m/s) donc **invisible en conduite nominale**, en dessous de la pointe (18 m/s) donc une visee rapide reste legerement lissee |

**Bornes derivees du vehicule, jamais des nombres absolus.** Aucun terme de lacet ne depasse le
couple qu'un seul pneu peut produire autour de la verticale :

    budget de lacet = adherence x charge statique d'une roue x demi-voie moyenne
                    = 3,0 x (1200 x 9,81 / 4) x 0,85
                    = 3,0 x 2 943 x 0,85
                    = 7 504 N.m

Et l'anti-roulis, **le seul terme de la couche qui n'avait aucun plafond**, est desormais borne par
la charge portee. C'etait un fait de code, pas une intuition : sa magnitude maximale atteignable
(`antiRollRate x travel` = 20 000 x 0,25 = 5 000 N) **depassait la charge statique d'une roue
(2 943 N)**, donc il pouvait localement annuler la charge d'un coin et la retourner. Or une bordure
franchie par une seule roue produit exactement l'ecart de compression qui l'alimente.

## 2. Autorite par roues au sol

`VehicleArcadeAssist.ResolveGroundedAuthorityFactor(groundedWheels, wheelCount)` rend une
**proportion** -- 4/4 vaut 1, 1/4 vaut 0,25, 0/4 vaut 0 -- et non un reglage : un facteur authore
aurait rendu la proportion fausse des que le nombre de roues change. Elle est publiee par
`VehiclePhysicsBody.GroundedWheelCount` et `GroundedAuthorityFactor`, puis appliquee :

- au **couple moteur** d'un pas (`driveTorque *= groundedAuthority`) ;
- aux **deux taux de direction** (braquage et rappel) ;
- dans `ApplyYawAssists` pour les aides de lacet, elles-memes bornees.

Aucune vitesse et aucune rotation ne sont ecrites pour autant : la couche ne fait que **reduire ce
qu'elle produit**. Un vehicule dont aucune roue ne touche le sol (facteur 0) ne recoit donc plus de
couple moteur et ne bouge plus ses roues, mais rien ne gele sa rotation.

**Correction d'assiette en vol.** `ApplyAttitudeAssist` retournait immediatement des que
`groundedWheels <= 0` : en l'air, plus rien ne stabilisait l'attitude, et un saut se terminait sur le
toit. L'**amortissement** de tangage et roulis n'est plus conditionne au contact ; le **rappel** vers
la verticale, lui, le reste -- c'est une correction de geometrie, il n'y a pas de sol a epouser en
l'air. Un saut ne se redresse donc pas tout seul, mais il ne culbute plus.

## 3. Relief de recette dans `MVP_Run`

Le district ne portait **aucune denivellation longitudinale** : la seule marche authoree etait la
bordure du carrefour, franchie **lateralement**, c'est-a-dire le cas d'une roue seule qui se
comprime. Le franchissement d'une bosse et d'une marche n'etait donc observable nulle part en Play
Mode. Demande explicite de l'utilisateur le 2026-09-18.

Pose en **objets de scene** sous `GreyboxMap`, sur `Avenue_CenterToEast` -- jamais dans un prefab de
module : `Greybox_RoadSegment_TwoWay.prefab` est instancie quatorze fois dans le district, dont sur
les routes IA, et un prefab ferait echouer `NoModuleColliderRisesAboveTheDrivingPlane`.

| Objet | Role | Mesure |
| --- | --- | --- |
| `Relief_DosDane_AvenueCenterToEast` | la **bosse** : deux rampes de 1,206 m a 10 %, crete a 0,120 m, base a -0,0796 m | emprise x [9,018 ; 11,418], largeur 8,000 m centree en z = 0, **2 colliders** |
| `Relief_MarcheBasse_AvenueCenterToEast` | la **levre** : 0,4 m de long, haut a 0,120 m, base a 0 | largeur 8,000 m centree en z = 0 |

Chaussee de reference : x [8 ; 24], z [-4 ; 4]. Aucun des **204 `LaneNode`** du district n'est dans
l'emprise du relief. Geometrie produite par un script de travail hors `Assets/` (voir section 7), qui
a **refuse deux fois de sauvegarder** avant d'obtenir les bonnes cotes : la premiere fois parce que
`Collider.bounds` n'est rafraichi qu'a `Physics.SyncTransforms()`, la seconde parce que le parent
etait pose sur la crete alors que ses enfants etaient deja exprimes depuis le plan de roulage.

## 4. Continuite du point de visee (dette `deferred-work.md` 278-280)

La cible **ideale** est continue tant que le noeud vise ne change pas -- et il change par
construction, a l'entree dans `arrivalRadius`. La cible sautait alors d'une branche de noeud a
l'autre, jusqu'a deux fois la distance de visee (4,8 m a 8 m/s) : un echelon de consigne de direction
d'un pas de physique a l'autre.

Le remede est un **rappel borne** : la cible rendue part de la cible precedente et avance d'au plus
`vitesse de rappel x pas de temps`. La continuite devient une propriete de la fonction, plus une
esperance. La memoire de la cible vit chez **l'appelant** (le controleur IA), pas dans la fonction
pure, qui reste sans etat ; elle est effacee aux deux seuls moments qui remettent le vehicule a un
point de depart connu -- insertion et recuperation sur place.

Une valeur de rappel **nulle** rend le rappel inerte et redonne exactement la loi de la 5.12 : les
deux lois restent donc mesurables cote a cote, ce qui est ce qui rend l'assertion non vide.

## 5. Mesures

- **`unity cmd recompile_status`** : `{"status":"completed","failed":false,"errors":[]}`, puis
  `up_to_date`.
- **`unity cmd run_tests --mode EditMode`** : **601/601 verts**, 0 echec, 0 ignore.
  Baseline 5.12 : 573. Delta : **+28 tests**, dont **25** dans la fixture nouvelle
  `Story513ArcadeAssistsAndUnevenGroundTests` et l'assertion de continuite ajoutee au rejeu de
  district de la Story 5.10. Les tests EditMode de la Story 5.9 sont verts **sans modification**.
- **`.\scripts\validate.ps1 -TestMode EditMode`** : **ECHEC**, `14 erreur(s) en Console (AD-7)`.
  Toutes **historiques** : erreurs de compilation des iterations 5.11 a 5.13 (`ResolveTireForce` /
  `ResolveCombinedForces`, `VehicleProfileDefTestAccess`, appels a l'ancienne signature de
  `ResolveLookAheadPoint`, `PrefabInstanceStatus.PrefabInstance`), **toutes corrigees depuis**. Le
  script appelle `console --level error` **sans `--since`** et rejoue donc tout le journal.
  `unity cmd console --level error --since 4821` rend **0 entree** : aucune erreur nouvelle.
  La mesure de repli prevue par les regles de build a donc ete utilisee, et le fait est rapporte
  tel quel.
- **`-TestMode PlayMode`** : **non productible par l'agent**. Le harnais reste rouge (3/33) et non
  filtrable. Aucun resultat vide n'est interprete comme un succes.

## 6. Procedure de recette humaine -- CONDITION DE CLOTURE

Decision du 2026-09-18 : la story **ne passe pas en revue** sans le resultat brut de cette recette.
Le precedent de 5.11 et 5.12 etait de livrer avec la mesure due ; elle n'a jamais ete faite, et la
revue de 5.12 a demontre qu'une couche physique inerte a l'execution laisse les tests EditMode
verts. Rendre la mesure bloquante est ce qui empeche la 5.13 de reproduire cet ecart.

**Résultats de la première passe (2026-09-18, recette utilisateur).**

| Point | Résultat |
| --- | --- |
| 1. Bordure du carrefour de biais | **OK** — montée sans décollage, aucune immobilisation contre la lèvre |
| 2. Relief de l'avenue, les deux sens | **OK** — dos-d'âne puis marche basse franchis dans les deux sens, basse vitesse et vitesse de conduite |
| 3. Roue délestée | **NON EXÉCUTÉ** — point mal formulé, voir ci-dessous |
| 4. Perte des quatre contacts | **NON EXÉCUTÉ** — aucun moyen connu de faire décoller le véhicule |
| 5. Désactivation individuelle des trois aides | **OK** — chaque aide perceptible puis disparue à sa valeur zéro |
| 6. Trafic IA | **OK** — aucune régression, hormis le décollage sur le relief, rattaché à la 5.14 |
| 7. Télémétrie | **NON EXÉCUTÉ** — la manipulation n'était pas donnée, voir ci-dessous |

**Consequence a l'issue de cette PREMIERE passe** (bloc historique, referme par la deuxieme passe
ci-dessous) : l'absolu de l'**AC2** etait mesure et satisfait (points 1 et 2) ; celui de l'**AC3** ne
l'etait pas, faute d'avoir exerce le cas des roues decollees. AC5 n'etait donc pas satisfait a ce
stade -- et une absence de resultat n'est jamais un succes.

**Résultats de la deuxième passe (2026-09-18, recette utilisateur).** Points **3, 4 et 7 : OK** —
tenue de caisse au franchissement en biais, cas des roues décollées exercé et observé, télémétrie lue.

| Point | Résultat |
| --- | --- |
| 1. Bordure du carrefour de biais | **OK** — montée sans décollage, aucune immobilisation contre la lèvre |
| 2. Relief de l'avenue, les deux sens | **OK** — dos-d'âne puis marche basse franchis dans les deux sens, basse vitesse et vitesse de conduite |
| 3. Roue délestée | **OK** (deuxième passe) — la caisse se redresse, elle ne se couche pas |
| 4. Perte des quatre contacts | **OK** (deuxième passe) — cas exercé et observé via la télémétrie |
| 5. Désactivation individuelle des trois aides | **OK** — chaque aide perceptible puis disparue à sa valeur zéro |
| 6. Trafic IA | **OK** — aucune régression, hormis le décollage sur le relief, rattaché à la 5.14 |
| 7. Télémétrie | **OK** (deuxième passe) — roues au sol et facteur d'autorité lus |

**Clôture.** L'absolu de l'**AC2** est mesuré et **satisfait** (points 1 et 2). Celui de l'**AC3**
l'est également (points 3 et 4, deuxième passe) : l'attitude tient et l'autorité suit le nombre de
roues au sol, observés en conditions réelles. **AC5 est donc satisfait**, et la story peut passer en
revue.

**Nature exacte de cette mesure, pour ne pas la surinterpréter.** La recette a été rapportée sous forme
de **verdicts** (`OK` par point), pas de valeurs numériques : l'excursion verticale et la vitesse
verticale maximales que la procédure demandait de consigner, à comparer aux bornes de non-régression du
banc 5.11 (**0,4 m** et **4 m/s**), n'ont pas été relevées. AC5 est donc tenu par le verdict humain sur
chaque cas **exercé** — et les points 1 à 4 et 7 l'ont été — pas par un chiffre. À reprendre avec les
valeurs dès qu'un seuil devient discutable.

**Deux défauts constatés pendant cette recette, hors périmètre de la 5.13.** Un véhicule IA qui
franchit le relief décolle (rattaché à la 5.14, entrée dédiée au registre). Et les deux
`MVP_RageTargetVehicle_*` ne sont pas conduisibles directement. Sur ce second point, deux suspects
ont été **écartés par la mesure** : `Dev_IndestructibleCar` porte bien `vehicleProfile` (même guid que
la voiture joueur) et la scène ne le surcharge pas ; et les deux instances portent les **quatre
marqueurs** d'objet posé en scène (`m_InScenePlaced`, `GlobalObjectIdHash`,
`SceneMigrationSynchronization`, `InScenePlacedSourceGlobalObjectIdHash`) avec des valeurs non nulles
et distinctes. Le suspect restant est la **poignée de main conducteur/siège** (`DriverClientId` /
`IsInoperable()` : `NetworkedVehicleDriverController.ApplyServerDriveIntent` rejette l'intention si
`state.DriverClientId.Value` ne correspond pas à l'expéditeur), qui demande une mesure en Play Mode
pour être tranchée.

**Comment lire la télémétrie — c'est aussi l'instrument du point 4.**

`VehiclePhysicsTelemetryView` n'est montée que dans `Dev_VehicleSandbox`, pas dans `MVP_Run`. Pour la
recette, dans l'Éditeur : sélectionner le véhicule joueur dans `MVP_Run`, **Add Component** →
`Vehicle Physics Telemetry View`. La vue affiche alors, à l'écran, sur deux lignes ajoutées par cette
story :

- `roues au sol N / 4   autorite XX %` — le compte publié par la couche physique et le facteur
d'autorité réellement appliqué au pas précédent ;
- puis, par roue, `roue i : au sol` ou `roue i : en l'air`, avec compression, glissement, charge et
force transmise.

**Retirer le composant avant de sauvegarder la scène** (garde de double état d'`AGENTS.md`) :
`MVP_Run.unity` et `git status --short` doivent revenir à leur état initial.

**Ce qui reste à mesurer, et comment.** Aucune géométrie nouvelle n'est nécessaire pour le point 4 :
si une roue quitte le sol au franchissement, **la vue de télémétrie le dit** (`roue i : en l'air`),
et le facteur d'autorité baisse dans la même frame. Le banc PlayMode de la 5.11 a mesuré une excursion
verticale de **jusqu'à 0,4 m** au franchissement de la bordure à vitesse de conduite : franchie assez
vite, la bordure peut donc déjà délester des roues. **Si la vue ne montre jamais de roue en l'air**, le
dire franchement : cela signifie que la géométrie actuelle n'exerce pas ce cas, et poser une seconde
rampe de recette devient une décision d'authoring à prendre — pas un correctif à improviser.

**Point 3, reformulé.** Il ne s'agit pas de la rotation des roues (voir le constat sur les roues
figées, section 7) mais de la **tenue de caisse** : aborder la bordure du carrefour **en biais**, de
sorte qu'un seul côté monte (deux roues chargées, deux délestées), et vérifier que la caisse **se
redresse** au lieu de se coucher. C'est la démonstration qui manquait à `deferred-work.md` 258-260,
où le sens appliqué de l'anti-roulis n'était gardé que par une assertion sur le texte source.


Editeur, scene `MVP_Run`, en hote.

1. **Bordure du carrefour central** (`Col_Curb_*`, 0,12 m), a vitesse de conduite, roue par roue et
   de biais. Attendu : le vehicule monte, aucun decollage, aucune immobilisation contre la levre.
   Consigner l'excursion verticale et la vitesse verticale maximales, a comparer aux bornes de
   non-regression du banc 5.11 (**0,4 m** et **4 m/s**).
2. **Relief de recette de `Avenue_CenterToEast`**, dans les deux sens : **dos-d'ane** puis **marche
   basse**, a basse vitesse puis a vitesse de conduite. Attendu : la caisse passe, aucun decollage,
   aucune immobilisation contre la face. C'est le seul endroit du district ou le franchissement
   longitudinal est observable.
   - Si la marche basse **ne reproduit pas** d'immobilisation, le dire : cela **infirme** l'hypothese
     geometrique ci-dessous, et aucun mecanisme ne doit etre ajoute pour un symptome non reproduit.
3. **Roue delestee** (bordure franchie par un seul cote) : la caisse doit se redresser au lieu de se
   coucher. C'est la demonstration qui manquait a `deferred-work.md` 258-260.
4. **Perte des quatre contacts** : attitude stable, **sans rotation gelee** -- le vehicule tourne
   encore sur lui-meme s'il etait en rotation ; un saut ne se redresse pas tout seul mais ne culbute
   plus.
5. **Desactivation individuelle** : en virage et en appui, chaque aide doit etre perceptible, puis
   disparaitre quand sa valeur authoree est remise a zero **une par une**.
6. **Trafic IA** : observer au **carrefour central**, en conduite nominale -- aucune regression
   visible par rapport a l'etat de la Story 5.12. **NE PAS juger cette ligne sur l'avenue** : voir le
   premier constat ci-dessous, un vehicule IA qui franchit le relief decolle, et c'est un defaut de
   la Story 5.14, pas des aides.

Lire le nombre de roues au sol et le facteur d'autorite dans la vue de telemetrie et consigner les
valeurs. Garde de double etat d'`AGENTS.md` : `MVP_Run.unity` et `git status --short` doivent revenir
a leur etat initial apres la recette.

## 7. Constats et points ouverts

- **DEFECT CONFIRME EN RECETTE (2026-09-18) : un vehicule IA qui franchit le relief de recette DECOLLE et poursuit son trajet en l'air, comme s'il n'y avait pas de gravite.** Rapporte par l'utilisateur avec une video. **Ce n'est ni le relief ni les aides qui sont en cause** : c'est le chemin de mouvement en boucle ouverte de l'IA, qui reimpose le vecteur vitesse entier a chaque pas de physique.
  - Mecanisme, dans `NetworkedAIVehicleDriverController.ApplyMovement` : la composante **horizontale** est forcee a `forward * longitudinalSpeed`, donc le vehicule **ne peut pas ralentir** sur le relief -- sa vitesse est une **entree**, pas une consequence ; la composante **verticale** est relue puis **reecrite telle quelle**, donc toute vitesse verticale donnee par un contact est **reinjectee au lieu d'etre consommee** ; et le lacet est impose par `body.MoveRotation`, donc la caisse reste a plat et garde son cap -- d'ou l'impression qu'« il continue son trace ».
  - **Verifications faites** : `m_UseGravity: 1` sur `Greybox_AIVehicle.prefab` **et** `Greybox_PlayerCar.prefab` -- la gravite n'est donc pas desactivee, c'est l'ecriture qui la neutralise. Le joueur est indemne parce que son chemin complet ne passe que par `VehiclePhysicsBody.ApplyDriveIntent` et n'ecrit aucune vitesse.
  - **Les aides de la 5.13 ne sont pas la cause** : aucune ne produit de force verticale, et l'anti-roulis borne par cette story est passe de 5 000 N a 2 943 N maximum, donc a **moins** de force verticale qu'avant.
  - **Pourquoi le defaut etait invisible** : le district etait **plat**, sans aucune pente ni relief longitudinal authore. Le dos-d'ane est le premier objet du district a porter un vehicule IA vers le haut.
  - **Rattache a la Story 5.14**, qui possede deja cette cause racine (meme famille que l'entree « les vehicules IA ne peuvent pas etre pousses » et que `ANO-5.10-03`, avec un symptome **vertical** au lieu de longitudinal). **Aucun garde-fou vertical de compensation n'a ete ajoute** : ce serait exactement le travail de la 5.14, et cela masquerait la cause.
- **Hypothese non verifiee, ecrite comme telle.** Le « coince contre la levre » est attribue a la
  **geometrie de detection** : un rayon vertical unique par roue, dont l'origine est l'ancrage. Si
  l'ancrage passe au-dela de la levre horizontalement, le rayon manque la marche, la roue est
  declaree en l'air, la suspension n'existe plus, alors que le collider du chassis bute sur la face
  verticale. C'est une **inference de geometrie**, pas une mesure : l'etape 2 de la recette doit la
  confirmer ou l'infirmer **avant** qu'un mecanisme soit ajoute.
- **L'IA est inerte face aux aides jusqu'a la 5.14.** Les deux prefabs vehicule partagent le meme
  profil author e, donc aucune aide ne peut etre differenciee par la donnee ; et
  `NetworkedAIVehicleDriverController.ApplyMovement` remplace encore la velocite et impose le lacet a
  chaque pas, donc tout couple d'aide y est ecrase. Aucun verrou par vehicule n'a ete ajoute, ce qui
  aurait ete un second chemin de verite (AD-35).
- **Le relief de recette est un instrument, pas un element du district** : condition de retrait
  enregistree dans `deferred-work.md`.
- **NEUF bancs PlayMode non executes** au total : **5** pour cette story (`Story513ArcadeAssistsAndUnevenGroundPlayModeTests.cs`), 2 pour la 5.11, 2 pour la 5.12. Le decompte precedent de « six » etait faux -- corrige apres revue, en comptant les `[UnityTest]` de chaque fichier.
- **Dette technique du registre reparee.** Le fichier `deferred-work.md` avait ete abime par un
  formateur markdown : la cle `source_spec` etait devenue `source*spec`, `Col_Curb_*` etait devenu
  `Col_Curb*\*`, et deux lignes `evidence:` avaient perdu leur indentation de continuation. Les six
  lignes sont reparees. **Ne pas relancer de formateur markdown sur les artefacts BMAD** : ils
  portent des accolades, des underscores et des listes a continuation que l'outil reinterprete.
- **Script de travail** : `AgentScripts/Story513RecipeRelief.cs` a produit le relief. Il vit hors
  `Assets/` (donc sans reimport ni rechargement de domaine) et n'est pas suivi par Git. A supprimer
  si la provenance du relief n'a plus besoin d'etre rejouable.
