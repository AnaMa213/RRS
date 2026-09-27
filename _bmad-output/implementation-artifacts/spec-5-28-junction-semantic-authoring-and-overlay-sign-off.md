---
title: 'Story 5.28 -- Authoring semantique des carrefours et sign-off de l overlay MVP_Run (Gate A)'
type: 'feature'
created: '2026-09-23'
status: 'in-progress'
baseline_commit: '8f25415062972e5c0e39a62e039e21bf35a97cd9'
review_loop_iteration: 0
context:
  - '_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le modele `MVP_Run` migre (5.27) ne compile pas (72 mouvements sans `JunctionControl`), aucune zone de conflit n'existe, largeurs/frontieres/portails ne sont pas revus, et aucun artefact V2 chargeable ni sign-off humain n'existe : la Gate A est fermee.

**Approach:** Aucun `RoadModelSource` 5.27 n'est persiste : la 5.28 relance l'importeur V1 deterministe sur la scene courante et la lignee existante, en lecture seule, et exige zero identite frappee ou retiree. Un fichier de decisions d'authoring humain, seul authoritative, est fusionne a ce resultat ; controles `Uncontrolled` par approche, conflits candidats generes hors ligne depuis les enveloppes compilees puis acceptes, largeurs revues, dispositions explicites des autres taches. Le pipeline produit un `RoadModelSource` complet deterministe persiste, un artefact d'overlay, un rapport Gate A lie, et des fixtures de localisation sur la carte reelle ; le proprietaire revoit l'overlay dans l'Editeur et signe par menu.

## Boundaries & Constraints

**Always:** Decisions (owner, 2026-09-23) : un `JunctionControl` par corridor d'approche, lie a tous les mouvements partant de cette approche, seul genre admis `Uncontrolled` ; conflit candidat = paire de mouvements du meme carrefour, d'approches differentes, dont les enveloppes balayees se recoupent (croisement ou convergence) ; paire de meme approche disposee « suivi, pas conflit » ; vitesse limite, classes de vehicules et surface differees explicitement, chacune avec sa story/condition de reouverture, aucune valeur inventee. Controles et zones sont des records authores : leurs `RoadId` sont frappes une fois et portes par le fichier de decisions, jamais par la lignee V1. Le `RoadModelSource` persiste est un artefact DERIVE (import + lignee + decisions), deterministe octet pour octet, lie par hashes ; son chargement recompile et refuse une version divergente. Toute tache d'authoring 5.27 est disposee exactement une fois ; decision orpheline ou tache non disposee = echec dur. Ecriture fail-closed (tmp puis remplacement), `#if UNITY_EDITOR` hors chargeur. Identite d'approbation = metadonnee du seul sign-off (identite Git configuree), jamais dans `RoadModelVersion` ni dans aucun hash de modele. Tests 5.25/5.26/5.27 inchanges.

**Ask First:** Tout genre de controle autre que `Uncontrolled` ; rejeter une categorie entiere de candidats ; modifier V1, `MVP_Run`, la lignee 5.27 ou l'importeur/validateur/compilateur au-dela de ce que liste la Code Map ; relacher un seuil ; nouvelle assembly ou dependance.

**Never:** Signer ou generer un sign-off a la place du proprietaire ; controle, conflit ou largeur applique sans entree de decision ; inference de conflit au runtime ; ligne d'arret, `SignalPlan`, adjacence de meme sens ; topologie ou regle tiree d'un nom ; runtime de carrefour (5.34/5.35).

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Nominal | decisions committees, lignee inchangee | 0 erreur, `RoadModelVersion` presente, 0 non dispose, fixtures vertes | N/A |
| Approche sans controle / controle orphelin | decision retiree / cle inconnue | echec dur nommant la cle | rien n'est ecrit |
| Genre non admis | `Stop` dans les decisions | echec dur (5.35) | rien n'est ecrit |
| Candidat non dispose / decision sans candidat | conflit ajoute ou retire | echec dur | rien n'est ecrit |
| Largeur revue != amorce importee | largeur des decisions != largeur importee | largeur revue APPLIQUEE aux echantillons possedes ; rapport : importee / appliquee par sujet (5.49) | N/A (trace dans le rapport) |
| Largeur sous le gabarit | une demi-largeur, gauche ou droite, de tout echantillon possede -- y compris interpole sur un mouvement -- < `MaxVehicleHalfWidthMeters + LateralClearanceMarginMeters` | echec dur nommant le sujet et l'echantillon | rien n'est ecrit |
| Lignee perimee | import frappe ou retire une identite | refus : relancer la migration 5.27 | rien n'est ecrit |
| Modele persiste altere | octet modifie ou version divergente | chargement refuse | jamais repare |
| Sign-off perime | un hash lie differe du pipeline frais | Gate A fermee, motif | jamais repare |

</frozen-after-approval>

## Code Map

- `.../Traffic/Migration/MigrationReport.cs:146,201,425,1088,1146` -- `Run(set, prior)`, `ValidateSource`, `ComputeReachability`, `TryWrite`, garde de scene du menu : a reutiliser, pas a modifier.
- `.../Traffic/Migration/V1RoadModelImporter.cs:69,90,146,164,263,1048,1252` -- `MovementRole`, `AuthoringTask{Category,SubjectKey,Text}`, `ImportedJunction{Key,Module,Movements}`, `V1ImportResult` (`Source`, `Tasks`, `IdOf`, `Lineage.Minted/Retired`), `AddTasks`, `Boundary` ; controles/zones vides a :1215.
- `.../Traffic/Migration/RoadLineage.cs:73,135` -- motif DTO `[Serializable]` + `JsonUtility` + LF + `Parse` strict ; `V1SourceSet.Sha256Hex` (:494).
- `.../Traffic/RoadModelRecords.cs:34,489,508,643,713` -- `RoadId` non serialisable par `JsonUtility` (hex) ; `JunctionControl`, `ConflictZone`, profil (demi-largeur 1,03, marge 0,25), `RoadModelSource`.
- `.../Traffic/RoadModelCompiler.cs:34,44` -- `Compile` leve `RoadModelCompilationException` ; `RoadModelValidator.cs:400-518` : code 9 couverture unique, 13 membres >= 2 meme carrefour, 23 extents > 0.
- `.../Traffic/CompiledRoadModel.cs:12,444-577` -- `RoadModelVersion`, `movement.Curve`, `GetMovementsInJunction` ; `RoadCurve.Project`/`Bounds` (`RoadCurve.cs:123,201`).
- `.../Traffic/RoadLocalization.cs:141` -- `RoadLocalizer.Localize(model, pose, previous, route)` ; motifs de fixtures `Story526GeometryAndLocalizationTests.cs:264-330,974-1130`.
- `Tests/EditMode/Story527MigrationTests.cs:781-800` -- harnais `RunOnMvpRunWith` (garde dirty, ouverture additive).
- `.gitattributes` -- epingler en LF les nouveaux artefacts haches.
- `.../Traffic/Migration/RoundaboutClearance.cs` -- mesure 5.49 des 4 giratoires ; la 5.28 l'etend a sa reprise : balayage conservateur sur les trajectoires finales 5.50 (transitions vers les corridors adjacents incluses), residu d'anneau recalcule, publication des colliders mesures (empreinte physique).
- Outil de degagement des angles de la 5.51 (nom fixe par la 5.51) -- preuve des angles des 5 carrefours classiques ; sa fonction de balayage conservateur est reutilisee par `RoundaboutClearance`.

## Tasks & Acceptance

**Execution:**
- [x] `.../Traffic/RoadModelDocument.cs` -- (runtime) serialisation JSON deterministe du `RoadModelSource` complet (DTO, ids hex, enums par nom) + bloc de liaison ; `Load` = parse strict, compile, compare la version liee, sinon refus.
- [x] `.../Traffic/Migration/AuthoringDecisions.cs` -- format, `Parse` strict, `Serialize`, `Propose(import, candidats)` (amorce, n'ecrit jamais sur un fichier existant).
- [x] `.../Traffic/Migration/AuthoredRoadModel.cs` -- pipeline (Design Notes) : relance de l'importeur 5.27 en lecture seule sur scene + lignee, zero frappe ni retrait, application des decisions, compile 1, candidats de conflit, zones, compile 2, fixtures de localisation, document modele, artefact d'overlay, rapport Gate A lie, `Verify` du rapport et du sign-off ; menus `Proposer les decisions` et `Compiler le modele authore`.
- [x] `.../Traffic/Migration/GateAReviewWindow.cs` -- fenetre : dessin Scene view des primitives d'overlay, 25 instances a cadrer et cocher, bouton de signature actif seulement quand tout est coche, confirmation explicite, ecriture du sign-off.
- [x] `Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-authoring.json` -- amorce par le menu, puis relue ; `MVP_Run.road-model.json`, `_bmad-output/implementation-artifacts/overlay-5-28-mvp-run.txt`, `migration-report-5-28-mvp-run.md` -- generes par le menu, committes.
- [x] `Assets/RoadRage/Tests/EditMode/Story528AuthoringAndGateATests.cs` -- chaque ligne de la matrice ; comptes (40 controles, 72 mouvements couverts une fois) ; artefacts committes egaux a un pipeline frais ; aller-retour du document (memes octets, meme version) ; candidats invariants a l'echantillonnage (meme courbe re-echantillonnee) et a l'ordre de la paire ; largeur revue appliquee aux echantillons possedes et largeur sous le gabarit refusee (5.49) ; fixtures ; sign-off lie (echoue tant qu'il manque).
- [x] `deferred-work.md` -- clore les fixtures de localisation ; noter que le self-loop n'est pas rouvert (aucune connexion ni mouvement authore a la main). `sprint-status.yaml` -> `in-progress` ; `graphify update .`.
- [ ] `.../Traffic/Migration/AuthoredRoadModel.cs`, `.../Traffic/Migration/RoundaboutClearance.cs`, `.../Traffic/Migration/GateAReviewWindow.cs` -- liaison physique de la Gate A (correct-course 2026-09-25). Preuve des 9 carrefours : angles des 5 carrefours classiques (croix et T) = resultats de la 5.51 ; 4 giratoires = `RoundaboutClearance`, que la 5.28 etend pour recalculer sur les trajectoires finales 5.50, transitions vers les corridors adjacents incluses, le balayage conservateur de l'empreinte (fonction de la 5.51 reutilisee, gabarit et marge du profil versionne, gonfles de la tolerance de corde compilee) et le residu d'anneau a deux gabarits. Un residu de giratoire non positif = HALT pour decision du proprietaire, sans changement physique. Empreinte des entrees physiques, construite sur les valeurs d'authoring serialisees : `GlobalObjectId`, transforms locaux de toute la chaine, type et proprietes geometriques, contenu du maillage (sommets, indices, options de cuisson) des `MeshCollider`, participation (`Collider.enabled`, `GameObject.activeInHierarchy`, `isTrigger`, couche, `includeLayers`/`excludeLayers`, collision avec la couche du vehicule IA dans la matrice physique, genre de `Rigidbody`), version du code de mesure. Rapport Gate A : `physical-input-hash` et resultats. Sign-off lie aux deux, hors `RoadModelVersion`, hash source et lignee. Evaluation : empreinte identique, residus frais strictement positifs, regle de reproductibilite (comparaison exacte si la reproductibilite bit a bit est demontree par test apres rechargement de scene puis nouvelle session d'Editeur ; sinon HALT, ecart presente au proprietaire qui fixe la borne). Toute decision de conflit non reconfirmee (5.50) compte comme ouverte.
- [ ] `Assets/RoadRage/Tests/EditMode/Story528AuthoringAndGateATests.cs` -- Gate A fermee, cause nommee, si l'empreinte physique differe (collider deplace, `Collider.enabled` ou `isTrigger` bascule, maillage modifie, couche exclue de la matrice), si un residu frais -- angle ou giratoire -- n'est pas strictement positif, si les residus different sous la regle de reproductibilite, ou si une decision n'est pas reconfirmee ; ouverte sinon, apres signature ; un changement de collider hors de portee des 9 carrefours ne ferme pas la Gate A.
- [ ] HALT : revue de l'overlay et signature par le proprietaire dans l'Editeur ; puis `MVP_Run.road-signoff.json` committe.
  - Revue du 2026-09-23 : 20/25 instances acceptees ; carrefour central en attente (filtrage des conflits ajoute a la fenetre, a revoir) ; 4 giratoires BLOQUES : anneau 4,0 m contre 8,0 m pour une route normale, elargissement physique confie a une story de suivi dediee (option B, `deferred-work.md`, section « Bloquant Gate A »). Gate A non signable avant cette story, la regeneration des artefacts et une nouvelle revue des giratoires.
  - Correct-course du 2026-09-23 (sprint-change-proposal-2026-09-23.md) : Story 5.49 inseree avant la signature. Reprise du HALT apres 5.49 : artefacts regeneres, nouvelle revue des 4 giratoires ET du carrefour central, puis signature.
  - Story 5.49 (2026-09-23) : giratoires elargis, largeur revue APPLIQUEE (decisions format 2, `PipelineVersion` 2), artefacts regeneres (decisions, modele, overlay, rapport) ; `RoadModelVersion` `v4:bc477eb562d7946c977c13672cab39df` -> `v4:33e3cc5fca044e988e9862d7eb77dddb` ; lignee et hash source inchanges. Pret pour la nouvelle revue des 4 giratoires et du carrefour central.
  - Revue du 2026-09-24 et correct-course du 2026-09-25 (sprint-change-proposal-2026-09-25.md) : mouvements de giratoire non conduisibles (rayon 0,25 m, crochets), virages a gauche amorces au bord du carrefour, virages a droite en conflit avec les angles de trottoir carres. Stories 5.50 (geometrie V2 et validation de conduisibilite au niveau modele) et 5.51 (degagement physique des angles) inserees avant la signature. Reprise du HALT apres 5.51 : liaison physique de la Gate A (preuve des angles des 5 carrefours classiques par la 5.51, preuve des 4 giratoires recalculee par la 5.28 via `RoundaboutClearance`), artefacts definitifs de la 5.50, nouvelle revue des 9 carrefours et des segments dont la geometrie V2 a change, puis signature.
  - Amendement `5.50-AUTO-DECISIONS-v1` approuve le 2026-09-27 : les decisions de conflit 5.50 peuvent etre produites en lot par la fonction deterministe deleguee si le manifeste, les preuves, les empreintes et toutes les versions verifient. Une decision automatisee incoherente ou perimee ferme Gate A. La revue visuelle et la signature Gate A restent des actes distincts du proprietaire et ne sont jamais automatises.

### Review Findings

Revue du 2026-09-23 (baseline `8f25415`, couches blind-hunter / edge-case-hunter / verification-gap ; security-review inactive : aucune frontiere reseau). Aucun intent_gap ni bad_spec. Hors perimetre sur decision du proprietaire : largeur des giratoires et regle gelee « Largeur divergente » (story corrective, `deferred-work.md`, « Bloquant Gate A »). La story reste `in-progress`, Gate A non signee, artefacts des giratoires provisoires.

- [x] [Review][Patch] `Load` acceptait une provenance editee (hashes source/lignee/decisions, versions) : `IntegrityHash` couvre desormais toute la liaison et le corps. [RoadModelDocument.cs]
- [x] [Review][Patch] Enums numeriques non declares (`"7"`) acceptes a la lecture du modele et des decisions (un `Decision` inconnu valait rejet) : `TryParseDeclaredEnum` partage. [RoadModelDocument.cs, AuthoringDecisions.cs]
- [x] [Review][Patch] Tache typee (Controle/Conflit/Largeur) comptee disposee sans donnee typee couvrant son sujet : echec dur. [AuthoredRoadModel.cs]
- [x] [Review][Patch] Sujet de largeur sans echantillon accepte : echec dur. [AuthoredRoadModel.cs]
- [x] [Review][Patch] Rayon balaye <= 0 bouclait sans fin : refuse avant generation. [AuthoredRoadModel.cs]
- [x] [Review][Patch] Rapport au corps reecrit et body-hash recalcule accepte : le rapport doit egaler le rapport frais octet pour octet. [AuthoredRoadModel.cs]
- [x] [Review][Patch] `EvaluateGateA` ignorait modele et overlay sur disque : `VerifyArtifacts` partage avec la fenetre. [AuthoredRoadModel.cs, GateAReviewWindow.cs]
- [x] [Review][Patch] Le harnais de test retirait une `MVP_Run` presente mais dechargee : `CloseScene(scene, !inHierarchy)`. [Story528AuthoringAndGateATests.cs]
- [x] [Review][Patch] Tests : falsification de chaque champ de liaison ; aller-retour sur un modele synthetique couvrant tous les genres d'enregistrement ; refus de `Parse` (doublons, desordre, enums non declares) ; la proposition n'ecrase jamais une destination existante (`ProposeRefusal`). [Story528AuthoringAndGateATests.cs]
- Non couverts par un test dedie : patchs 3 et 4 (non atteignables par un import reel sans alterer son resultat), gardes en code seulement.

**Verification (2026-09-23)** : `.\scripts\validate.ps1 -TestMode EditMode` -> 830/831, seul echec attendu `GateAIsOpenedOnlyByTheOwnersBoundSignoff` (sign-off absent), 0 erreur Console dans la fenetre, `scriptCompilationFailed=false`. Lignee, decisions et overlay identiques octet pour octet ; modele regenere (liaison), `RoadModelVersion` inchangee `v4:bc477eb562d7946c977c13672cab39df`.

**Acceptance Criteria:**
- Given les decisions committees, when le pipeline tourne, then chaque mouvement a exactement un controle `Uncontrolled` de son approche, chaque carrefour est declare non signalise sans `SignalPlan`, et aucune adjacence n'existe.
- Given le modele compile sans zones, when les candidats sont generes, then ils viennent des enveloppes balayees du gabarit max du profil versionne, et seules les decisions acceptees deviennent des `ConflictZone` ; aucun consommateur n'infere de conflit.
- Given le rapport Gate A, when on le lit, then il publie 0 erreur dure, `RoadModelVersion`, la disposition de chaque tache, candidats et decisions, champs differes avec reouverture, fixtures de localisation (nominal, frontiere, deplace, contresens, carrefour ambigu, hors corridor) et leur verdict.
- Given le sign-off du proprietaire, when la Gate A est evaluee, then il porte approbateur, 25 instances revues, hash d'overlay, et les hashes source, lignee, decisions, compilateur, modele et version d'un pipeline frais.
- Given la suite EditMode complete, when elle tourne apres signature, then elle est verte.

## Spec Change Log

- **2026-09-23 -- renegociation proprietaire, ligne gelee « Largeur divergente ».**
  Declencheur : revue d'overlay, 4 giratoires bloques (anneau V2 4,0 m ; residu a deux gabarits -1,78 m, anneau physique -0,88 m).
  Amende : la largeur revue est appliquee au lieu d'etre seulement comparee ; une demi-largeur de tout echantillon possede ou interpole sous le gabarit reste un echec dur ; implementation, elargissement physique et regeneration des artefacts confies a la Story 5.49 (sprint-change-proposal-2026-09-23.md).
  Etat evite : signer une Gate A sur un anneau qui ne peut pas contenir deux vehicules, ou faire de l'amorce de l'importeur l'autorite de largeur.
  KEEP : decisions seules authoritative, un corridor logique d'anneau, aucune adjacence, Gate A non signee avant 5.49 + regeneration + nouvelle revue.

- **2026-09-25 -- correct-course proprietaire (sprint-change-proposal-2026-09-25.md).**
  Declencheur : revue d'overlay du 2026-09-24 -- mouvements de giratoire non conduisibles (rayon 0,25 m, crochets), virages a gauche amorces au bord du carrefour, virages a droite en conflit avec les angles de trottoir carres.
  Amende : signature apres 5.49, 5.50 et 5.51 ; la Gate A lie aussi l'empreinte des entrees physiques et les resultats de degagement des 9 carrefours -- angles des 5 carrefours classiques par la 5.51, 4 giratoires par `RoundaboutClearance` recalcule par la 5.28 sur les trajectoires finales 5.50 ; une decision de conflit dont la geometrie de paire a change ne compte qu'apres reconfirmation du proprietaire (5.50) ; candidats balayes a empreinte complete (5.50).
  Etat evite : signer des trajectoires de reference non conduisibles, ou une Gate A que la geometrie physique invaliderait sans bruit.
  KEEP : bloc gele inchange -- decisions seules authoritative, candidat = croisement ou convergence d'enveloppes balayees du gabarit max (le suivi ordinaire n'en est pas un), tests 5.25/5.26/5.27 inchanges, signature humaine par menu.

## Design Notes

**Decisions (JSON, trie).** `Controls[{Id, ApproachKey, Kind}]` ; `Conflicts[{Id, MovementKeyA<B, Decision: Accepted|Rejected, Reason}]` (Rejected exige un motif) ; `Widths[{SubjectKey, HalfWidthLeftMeters, HalfWidthRightMeters}]` (sections et carrefours ; gauche et droite explicites, AD-45 asymetrique ; la largeur revue est APPLIQUEE aux echantillons possedes et le rapport publie importee / appliquee par sujet ; pour un carrefour, la regle d'application aux mouvements qui joignent deux largeurs differentes est fixee par la 5.49) ; `Dispositions[{Category, SubjectKey, Kind, Note}]` pour Frontiere/Portail (`Reviewed`), Ligne (`NotRequiredForCurrentControlKind` : aucune ligne sous `Uncontrolled` ; reouverture : 5.35, des qu'un controle passe a Stop/Yield/Priority), Signal (`Unsignalized`), Section (`Deferred`) ; `DeferredFields[{Field, Reopening}]` : vitesse -> 5.33, classes -> admission d'une seconde classe (recompilation AD-44), surface -> seconde surface roulable. Controle/Conflit/Largeur sont disposes par leurs donnees typees.

**Candidats.** Rayon r = `MaxVehicleHalfWidthMeters + LateralClearanceMarginMeters` ; chaque courbe est densifiee par `Curve.Sample(s)` a pas fixe `r/4` en abscisse curviligne (extremites incluses), independant de ses echantillons compiles ; test symetrique A->B et B->A (point densifie a moins de 2r de `Project` sur l'autre courbe) ; paire ordonnee par `RoadId`. Des courbes geometriquement equivalentes donnent donc le meme ensemble quel que soit leur echantillonnage ou l'ordre de la paire. Volume = AABB des points densifies en recoupement des deux cotes, elargie de r sur les trois axes. Balayage lateral seul jusqu'a la 5.50, qui le remplace par l'empreinte complete le long des trajectoires dirigees connectees (elements plus courts qu'un demi-vehicule inclus, suivi ordinaire publie comme suivi, zone propre a son carrefour, paire nouvelle, retiree ou modifiee soumise au proprietaire).

**Liaison.** Rapport : en-tete comme la 5.27 (+ `decisions-hash`, `model-hash`, `road-model-version`, `overlay-hash`). Overlay : texte canonique (par instance de module : centres et bords quantifies au cm, zones, frontieres, enveloppes de portail, controles) produit par la meme fonction que le dessin. Fixtures : derivees structurellement (portail d'entree de plus petit `RoadId`, approche a >= 2 mouvements), jamais par nom. Gate A (correct-course 2026-09-25) : le rapport publie aussi `physical-input-hash` et les resultats de degagement des 9 carrefours ; le sign-off les lie ; ni l'un ni l'autre n'entre dans `RoadModelVersion`, le hash source ou la lignee.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story528AuthoringAndGateATests"` -- vert apres signature.
- `.\scripts\validate.ps1 -TestMode EditMode` -- suite complete verte, 0 erreur Console.

**Manual checks:**
- Revue de l'overlay dans l'Editeur sur `MVP_Run`, sous double garde (`git status --short` + `unity cmd list_open_scenes` avant/apres).

## Suggested Review Order

**Pipeline authore (point d'entree)**

- Import 5.27 relance en lecture seule, decisions, deux compilations, fixtures.
  [`AuthoredRoadModel.cs:150`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L150)
- Decisions seules authoritative : controles par approche, largeurs, dispositions.
  [`AuthoredRoadModel.cs:259`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L259)
- Candidats et decisions en bijection ; seules les acceptees deviennent zones.
  [`AuthoredRoadModel.cs:521`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L521)
- Balayage densifie r/4, symetrique, independant de l'echantillonnage.
  [`AuthoredRoadModel.cs:650`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L650)
- Fixtures de localisation derivees structurellement sur la carte reelle.
  [`AuthoredRoadModel.cs:706`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L706)

**Modele persiste**

- Serialisation deterministe du RoadModelSource complet avec liaison.
  [`RoadModelDocument.cs:38`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelDocument.cs#L38)
- Chargement fail-closed : forme canonique, IntegrityHash, recompilation, version.
  [`RoadModelDocument.cs:72`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelDocument.cs#L72)

**Decisions d'authoring**

- Parse strict : listes triees sans doublon, enums declares, motif de rejet.
  [`AuthoringDecisions.cs:142`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoringDecisions.cs#L142)
- Amorce conforme aux decisions du proprietaire, jamais ecrite sur un existant.
  [`AuthoringDecisions.cs:319`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoringDecisions.cs#L319)

**Liaison, Gate A et signature**

- Rapport egal au rapport frais octet pour octet.
  [`AuthoredRoadModel.cs:1319`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L1319)
- Sign-off lie aux hashes d'un pipeline frais et aux 25 instances.
  [`AuthoredRoadModel.cs:1409`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L1409)
- Gate A : rapport, modele et overlay sur disque, puis sign-off.
  [`AuthoredRoadModel.cs:1496`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L1496)
- Ecriture fail-closed de tous les artefacts.
  [`AuthoredRoadModel.cs:1625`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/AuthoredRoadModel.cs#L1625)
- Signature humaine seulement : 25 instances cochees, artefacts a jour, confirmation.
  [`GateAReviewWindow.cs:160`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/GateAReviewWindow.cs#L160)
- Filtrage des conflits par approche, mouvement ou zone (revue du carrefour).
  [`GateAReviewWindow.cs:265`](../../Assets/RoadRage/Features/Vehicles/Traffic/Migration/GateAReviewWindow.cs#L265)

**Peripheriques**

- Comptes, controle unique par mouvement, modele versionne.
  [`Story528AuthoringAndGateATests.cs:27`](../../Assets/RoadRage/Tests/EditMode/Story528AuthoringAndGateATests.cs#L27)
- Falsification du modele persiste refusee, jamais reparee.
  [`Story528AuthoringAndGateATests.cs:268`](../../Assets/RoadRage/Tests/EditMode/Story528AuthoringAndGateATests.cs#L268)
- Artefacts committes egaux a un pipeline frais.
  [`Story528AuthoringAndGateATests.cs:352`](../../Assets/RoadRage/Tests/EditMode/Story528AuthoringAndGateATests.cs#L352)
- Gate A fermee tant que le proprietaire n'a pas signe.
  [`Story528AuthoringAndGateATests.cs:495`](../../Assets/RoadRage/Tests/EditMode/Story528AuthoringAndGateATests.cs#L495)
