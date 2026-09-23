# Story 5.26 — Geometrie dirigee en abscisse curviligne et localisation de voie

Tout le code vit dans `Assets/RoadRage/Features/Vehicles/Traffic/`, assembly existant
`RoadRage.Features.Vehicles`. Aucun assembly, package ni `.asmdef` ajoute ou modifie ; aucune
reference aux splines Unity.

## Ce que la story livre

| Fichier | Role |
| --- | --- |
| `RoadCurve.cs` | Courbe dirigee immuable (AD-45) : `Length`, `Sample(s)`, `Project(point[, sMin, sMax])`, `Bounds(s0, s1)`, `FullBounds`, cap signe (`RoadCurvePoint.SignedHeadingDegrees`). Exposee sur `EffectiveLaneCorridor.Curve` et `CompiledJunctionMovement.Curve`. |
| `RoadCurveBuilder.cs` | Polyligne authoree -> echantillons : Catmull-Rom centripete, subdivision adaptative, `s` cumule sur les cordes, tangente et courbure signee analytiques. |
| `RoadGeometryValidator.cs` | Validation geometrique a echec dur, codes 19 a 30, appelee par `RoadModelValidator.Validate` quand la validation structurelle est vide. Le code 31 (plafonds de profil) vit dans `RoadModelValidator`, avec le reste du profil. |
| `RoadLocalization.cs` | `VehicleFootprint`, `VehicleFootprintPose`, `RoadLocation`, `RoadLocationCandidate`, `RoadLocationFlags`, `RoadLocalizer`. |

## Conventions (AD-45)

`right = normalize(cross(up, forward))`. Lateral positif a droite, normal positif selon road-up.
Cap signe dans `[-180, 180]`, positif vers la droite. Courbure positive quand `dT/ds` pointe a
droite, nulle en ligne droite. `Project` borne `s` au domaine sans jamais extrapoler, en 3D ; une
egalite de distance est departagee par la plus petite abscisse. `Bounds` contient toute l'enveloppe
de largeur, avec une marge conservatrice pour la rotation du vecteur droite entre echantillons.

Entre deux echantillons : position, `s`, courbure et largeurs lineaires ; tangente et road-up
interpoles puis reorthonormalises.

## Deux profils, une regle de schema

| Profil | Porte | Dans la charge canonique |
| --- | --- | --- |
| `RoadModelValidationProfile` | gabarit, marge, et les tolerances geometriques statiques : couture (position/largeur 0,05 m, tangente 5 deg), longueur/corde (0,05 m), recouvrement d'enveloppes, seuil d'ancrage au datum (AD-48, 45 deg) | oui |
| `RoadLocalizationProfile` (nouveau, sur `RoadModelSource` et `CompiledRoadModel`) | bande de score, hysteresis, seuil d'acceptation, seuil de contresens | oui |

`LocalizationScoreBandMeters` et `WrongWayHeadingDegrees` ont quitte le profil de validation.
Changer une **valeur** de profil change la version ; seul un changement de **representation ou de
sens** incremente `CompilerSchemaVersion`. Ce deplacement en est un : **schema 2 -> 3**. Le
nettoyage final de la 5.26 y ajoute le seuil d'ancrage AD-48 (`GroundingMaxOffAxisDegrees`), qui
etait une constante du validateur et n'entrait donc dans aucune charge : **schema 3 -> 4**.

Domaine : chaque tolerance, la bande, l'hysteresis, l'acceptation et le seuil d'ancrage doivent
etre strictement positifs (l'ancrage dans ]0, 90[ : 90 deg accepterait une perpendiculaire), et le
seuil de contresens dans ]0, 180] ; sinon `NumericValueOutOfRange` (un profil non renseigne ne
compile pas).

**Plafonds approuves (nettoyage 5.26).** Le profil peut etre plus strict que le contrat, jamais
plus laxiste : `SeamGapToleranceMeters` > 0,05 m, `SeamTangentToleranceDegrees` > 5 deg et
`LengthToleranceMeters` > 0,05 m (elle borne aussi l'ecart abscisse/corde et le domaine des
portails, plafonnes a 0,05 m par AD-45) echouent en `ProfileToleranceAboveApprovedCeiling`
(code 31), meme sur un modele geometriquement coherent. Relacher un plafond est une revision
d'architecture, jamais une valeur de profil. `EnvelopeOverlapToleranceMeters` reste authore :
AD-48 ne fixe pas de nombre pour le recouvrement d'enveloppes.

## Validation geometrique

Deux phases. **Forme** d'abord ; si elle echoue, arret (les relations supposent des reperes sains).
Puis **relations**. Un code par defaut, jamais groupe ; chaque issue nomme l'id fautif.

| Code | Defaut | Sujet |
| --- | --- | --- |
| 19 `NonOrthonormalFrame` | tangente ou road-up non unitaire ou non orthogonaux (1e-3), road-up retourne (`up . monde-haut <= 0`), tangente opposee a sa corde | corridor / mouvement |
| 20 `InconsistentCurveLength` | `s0` != 0, `LengthMeters` != dernier `s`, ou pas de `s` != corde | corridor / mouvement |
| 21 `NonPositiveHalfWidth` | demi-largeur <= 0 | corridor / mouvement |
| 22 `ArcPositionOutOfDomain` | intervalle d'adjacence vide ou hors domaine, `s` de portail hors domaine | adjacence / portail |
| 23 `NonPositiveBoxExtents` | extents de frontiere de carrefour ou de volume de conflit <= 0 | carrefour / zone |
| 24 `ConnectionSeamBroken` | `LaneConnection` : ecart de position ou de tangente | connexion |
| 25 `MovementSeamBroken` | extremite de mouvement : position, tangente ou largeurs | mouvement |
| 26 `OppositeDirectionAdjacency` | adjacence entre sens opposes (AD-47) | adjacence |
| 27 `LaneSideDisagreement` | `Side` contredit la geometrie, ou l'ordre transversal dans une meme section (AD-48) | adjacence |
| 28 `NonMonotoneLateralOrder` | lignes centrales non strictement croissantes vers la droite du datum | corridor d'ordre superieur |
| 29 `OverlappingLateralEnvelopes` | enveloppes voisines recouvrantes au-dela de la tolerance | corridor d'ordre superieur |
| 30 `CorridorNotGroundedOnDatum` | aucun recouvrement avec le datum, ou ni parallele ni antiparallele (seuil du profil, `GroundingMaxOffAxisDegrees`) | corridor |
| 31 `ProfileToleranceAboveApprovedCeiling` | un profil relache un plafond approuve du contrat (couture 0,05 m, tangente 5 deg, longueur/corde 0,05 m) ; un profil plus strict passe | modele |

Procedure AD-48 : chaque corridor est projete au plus proche point sur le datum, restreint a leur
intervalle de recouvrement (trouve dans les deux sens de projection) ; puis chaque paire d'ordres
croissants dont les intervalles se recouvrent plus qu'en un point est comparee sur les abscisses
des deux traces. Rien n'est derive, reordonne ni repare.

`RoadModelVersion` reste emise par le seul `Compile` reussi, geometrie comprise ; `Validate` vide
signifie toujours « compilable ».

## Constructeur de courbe

Entrees rejetees (`ArgumentException`) : demi-largeur <= 0, road-up parallele a la tangente, et
tolerance de corde non atteinte apres 16 subdivisions (jamais de corde hors tolerance en silence).
Tolerance de corde : chaque corde emise s'ecarte de la spline d'au plus la moitie de la tolerance,
l'autre moitie etant reservee a l'ecart spline / courbe voulue. Extremites : point fantome par
extrapolation quadratique (repli sur la reflexion si la seconde difference depasse la moitie de la
corde). La reflexion seule inversait le signe de la courbure aux deux bouts d'un arc — mesure et
corrige pendant la story, garde par le test de signe qui couvre desormais les extremites. Mesure :
arc de 12 m, un point tous les 10 degres, ecart maximal 0,012 m.

## Score de localisation

Collecte : balayage lineaire des `FullBounds` de chaque element elargies du voisinage (2 x
acceptation) ; candidats a distance d'enveloppe <= voisinage. Tri par `(rang, score, RoadId)`.

- **Rang 0** : l'enveloppe (lateral + longitudinal) contient le point de reference ; pour l'element
  precedent, depassement tolere jusqu'a l'hysteresis ; et `|normal|` <= acceptation (un tablier
  superpose ne contient pas la pose). **Rang 1** sinon.
- **Score** (m) : `|lateral| + |normal| + depassement longitudinal + 2 m x |cap|/180`, moins
  l'hysteresis pour le precedent, moins hysteresis/2 pour un voisin explicite du precedent
  (connexion ou extremite de mouvement), moins hysteresis/2 pour un element de route. Bonus
  **cumulables** : un element de route departage deux successeurs du precedent.
- **Acceptation** : distance 3D a l'enveloppe <= seuil. `localized=false` si aucun candidat accepte ;
  aucune identite d'element, alternatives possibles.
- **Marge** : ecart de score avec le candidat suivant du meme rang (parmi les acceptes si localise).
  `Ambiguous` si marge < bande ; `confidence = clamp01(marge / bande)`, 1 sans rival du meme rang,
  0 si non localise.
- **Drapeaux** : `WrongWay` si |cap| > seuil ; `OutsideEnvelope` si la reference depasse une
  extremite de l'element, ou si la reference ou un coin de l'empreinte sort lateralement de
  l'enveloppe (repere de l'element a l'abscisse de la reference ; les coins ne sont pas testes
  longitudinalement, sinon chaque couture leverait le drapeau).
- **Entree** : pose non finie, avant ou haut nul, avant parallele au haut, ou extent negatif ->
  `ArgumentException`.

Le cap ne classe qu'a l'interieur d'un rang : deux voies opposees ont des enveloppes disjointes
(AD-48 valide), donc une pose a contresens reste sur sa voie physique.

Le score additif (bonus du precedent, d'un voisin explicite et d'un element de route), leur cumul
et le rayon de collecte de 2 x acceptation sont des **choix d'implementation**, pas des invariants
d'architecture : le contrat exige seulement un classement combinant geometrie, cap, route, element
precedent et connectivite explicite, et un balayage qui ne manque aucun candidat pertinent. Un
index spatial (AD-42 / 5.46) ou une autre ponderation des bonus pourront les remplacer sans changer
le contrat, tant que le cap ne classe jamais entre les rangs.

Regression dedicacee (nettoyage 5.26) :
`AHysteresisRetainedPreviousElementPastASeamKeepsItsIdentityAndRaisesOutsideEnvelope` -- 3 cm
au-dela de la couture NB2 -> mouvement, le precedent garde son identite (`s` borne a sa fin) et
`OutsideEnvelope` est leve ; cas symetrique 3 cm avant le debut d'un mouvement (`s` borne a 0).

## Choix de fixture

- **5.25** : la fixture est devenue geometriquement coherente (bandes a 5 m, demi-largeurs 2 m,
  demi-tours analytiques ; voir la spec). Les mutations de champ canonique que la geometrie interdit
  (couture, longueur, repere, ordre ou cote incoherents) sont prouvees sur
  `RoadModelCanonicalWriter.ComputeFingerprint` avec une charge construite par le test, en regard de
  l'assertion que `Compile` rejette la meme mutation. `TheTestPayloadMirrorsTheCompilerPayload`
  garde la fidelite de cette charge. Le cas « valeur finie non quantifiable » passe par un poids de
  route, la longueur enorme etant desormais rejetee par la geometrie avant le writer.
- **5.26** : route a double sens + deuxieme voie de meme sens (SB, NB datum, NB2) puis un carrefour
  en T ou NB2 diverge en tout-droit et virage a droite r=10 m. Le bord droit de NB2 n'a aucun voisin
  (cas deplace), NB est a cote de sa voie opposee (contresens), et le debut des deux mouvements est
  le cas ambigu.

## Nettoyage post-livraison (2026-09-23)

- Plafonds approuves : code 31 `ProfileToleranceAboveApprovedCeiling`, prouve par
  `AProfileThatRelaxesAnApprovedToleranceCeilingIsAHardFailure` (un profil plus strict passe, les
  trois champs relaches echouent, le motif nomme le champ fautif).
- Ancrage AD-48 : `GroundingMaxOffAxisDegrees` dans le profil, la charge et le schema 4 ; prouve
  par `TheGroundingAngleIsReadFromTheValidationProfile` (45 deg passe, 20 deg rejette le membre a
  30 deg) et par les cas 0 / 90 de `AnUnsetOrOutOfRangeProfileIsAHardFailure`.
- `EveryRemainingCanonicalFieldMovesTheVersion` (5.25) couvre le nouveau champ : une valeur
  differente change la version.
- Les deux entrees de `deferred-work.md` (invariants geometriques de la 5.25, adjacence de sens
  oppose) sont CLOSES le 2026-09-23 : couvertes par les codes 19-23, 26 et 27 et par leurs tests.

## Hors perimetre, laisse ouvert

- Aucun index spatial optimise : balayage lineaire (AD-42 / 5.46).
- Auto-boucle `FromCorridorId == ToCorridorId` : toujours non rejetee (reportee a la 5.27).
