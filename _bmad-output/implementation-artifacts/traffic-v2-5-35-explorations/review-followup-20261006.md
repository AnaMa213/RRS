# Story 5.35 — reprise de revue du 2026-10-06

Branche : `systeme-traffic-ia-v2`. HEAD de reprise : `a3c1faac89e12cefe8f873835a4cbc5fa980e15d`.
Baseline BMAD conservee : `183a329d410bd3e6e353c8c16cbfd76c84beffca`.
Etat au checkpoint initial : `in-review`, sans promotion finale, sans commit de cloture. Les sections suivantes sont chronologiques ; la derniere consigne l'etat courant.

Le rapport de revue anterieur demande par le proprietaire n'a pas ete retrouve dans le depot, y compris les fichiers ignores. Son chemin a ete demande pendant la reprise. Ce suivi distingue donc les decisions et findings revalides sur le HEAD ; il ne certifie pas le traitement d'une liste anterieure non disponible.

## Corrections et preuves

| Finding / decision | Pertinence au HEAD | Correction / preuve |
|---|---|---|
| P3(b), reservation d'un ancien `YieldToPriority` | Toujours ajoute a `refused`, donc susceptible de bloquer C dans A -> B -> C | Retire uniquement de la reservation, anciennete conservee. Chaine ouverte testee sur six permutations, cause TooFar hors de W, service normal de C puis service de A avec anciennete initiale. |
| Briseur reserve aux cycles fermes | Algorithme conserve ; preuve d'ordre insuffisante | Cycle de quatre demandes teste sur 24 permutations, un seul grant, departage par id puis anciennete de la demande 13. Aucun briseur dans la chaine ouverte. |
| ETA avec vitesse reelle superieure a v0 | `TravelSeconds` ramenait immediatement v a v0 ; vitesse excessive possible sans perte de localisation/couverture | `EarliestArrivalSeconds` conserve v si elle depasse v0, utilisee pour priorite et fusion. Le calcul de degagement reste conservatif. Fonctions pures et ETA du record/trace testes a vitesse excessive. |
| P5 cas B confirme | Gardes presentes, mais aucun cas negatif direct | Trois geometries synthetiques deterministes : perte > 0,05 m sans contact nominal ; nouveau contact nominal avec perte <= 0,05 m ; perte < 0,05 m conforme. Premisses et raison exacte affirmees. Lignes reelles toujours conformes ; cas A negatif conserve. |
| Hash de relation de droite | Format 4 teste seulement avec hash correct | Admission refusee `GateAEvidenceStale` si hash signe absent ou faux, ou si seule la section de droite du rapport change. Preuve signee intacte sur disque. |
| Trace 5.35 | Champs nouveaux sans assertions directes | Genre, b, t_gap, ETA et arret marque preserves depuis les vrais records ; ETA < gap au refus et ETA >= gap a la fusion ; NaN sans evaluation/decision. |
| Comptage `Junction/` 5 -> 7 fichiers | Toujours present | Remplace par les types publics des responsabilites attendues. Scan des interdictions physique/reseau/pedales conserve. Test categorie `Story535`. |
| Scenario C 5.34, decision proprietaire | FIFO et `ConflictGranted` initial toujours attendus | C conserve, obstacles adaptes en memoire a b, axe `Priority` servi avant branche `Yield`, refus initial `YieldToPriority`, motifs ulterieurs controles. Exclusivite, zero contact et sorties finales toujours requis. Fixture PlayMode et builder C/D inclus dans `Story535` ; builder execute en EditMode. |

## Validation executee

Uniquement `.\scripts\validate.ps1 -Profile Story -Story 5.35 -TestMode EditMode`.

- Premier run : 32/34, 0 erreur Console. Deux nouvelles fixtures supposaient a tort `TooFar` a 20 m et 8 m/s ; elles utilisaient un acteur deja valide et servi. Correction : `D_request(8) + 0,1 m`, avec assertion explicite du rejet. [Extrait brut](review-followup-first-failure-20261006.txt).
- Run final : **40/40**, 0 skipped, 0 inconclusive, recompilation `completed`, `scriptCompilationFailed=false`, **0 erreur Console** dans la fenetre 1419. 1 220 tests exclus par selection, suites completes non executees. [Sortie brute complete](review-followup-editmode-20261006.txt).
- `MVP_Run` ouvert, actif et `isDirty=false` avant et apres. Aucun fichier scene, prefab, modele ou preuve Gate A change dans cette reprise.
- `git diff --check` conforme. `graphify update .` execute : 4 531 noeuds, 10 549 liens, 182 communautes. Le CLI a renomme des communautes apres changement du graphe ; aucun relabel LLM demande.

## Dispositions de revue

Revue BMAD du diff : risque proportionne examine directement ; couches edge-case et verification-gap en contextes separes. Aucun changement de frontiere reseau : couche security inactive. Corrections ciblees, garde Ponytail sans refactor transversal.

- Hypothese d'execution runtime sous signature legacy 0/2 **retiree** : l'admission documentaire historique peut rester ouverte, mais `PoseModelMismatch` empeche couverture et insertion depuis la 5.52. Aucun changement de compatibilite historique dans cette reprise.
- Arret synthetique `Stop` trop avance : initialement laisse comme intent gap P6 ; **clos par decision proprietaire lors de la suite de revue** : borne basse `m_ctrl - 0,02 m` appliquee aussi a l'arret marque. Voir le complement ci-dessous.

## Checkpoint initial : restant et HALT (historique)

Aucun PlayMode execute. Les criteres runtime C/D, E/F/G, progression/contacts et Gate C N=2/4/8, dont p95 N8 < 10 ms, restent non verifies dans cette reprise. Aucune acceptation finale ni promotion de la 5.35.

Commande proprietaire suivante (inclut C/D adaptes, E/F/G et la fixture 5.34 modifiee) :

```powershell
.\scripts\validate.ps1 -Profile Story -Story 5.35 -TestMode PlayMode
```

Puis campagne Gate C explicite, apres lecture des resultats precedents :

```powershell
.\scripts\validate.ps1 -TestMode PlayMode -TestFilter Story535GateC -TestFilterType category -IncludeExplicit
```

## Retour proprietaire et rapports disponibles

Le proprietaire signale « on est bon c'est vert », puis precise **« Gate C uniquement verte »**. La Gate C est donc confirmee verte par le proprietaire ; cette confirmation ne couvre pas le profil PlayMode Story535. Aucun test relance par l'agent.

- Gate C : resumes [N2](gatec-2-20261006-180715-summary.md), [N4](gatec-4-20261006-180815-summary.md) et [N8](gatec-8-20261006-180921-summary.md). Respectivement 2/2, 4/4 et 8/8 sorties, aucun grant incompatible hors creneau, aucune entree sans grant, aucune immobilisation interne, aucun contact V2-V2 ni TrackingToleranceExceeded. N8 : p95 pas hote 8,165 ms sur tous les pas et **7,383 ms en population pleine**, sous 10 ms ; un briseur publie. N4/N8 rapportent aussi deux contacts avec d'autres colliders, distingues des contacts V2-V2 par le harnais.
- E et F : resumes `scenario-E-20261006-175600-summary.md` et `scenario-F-20261006-175654-summary.md`, 2/2 sorties, preseance attendue, contacts et violations nuls. **Cela ne vaut pas succes officiel de fixture** : E s'interrompt avant la ligne 3 de sa sequence (assertion de bande, diagnostiquee dans le complement ci-dessous).
- **G reste ouvert** : [scenario-G-20261006-175955-summary.md](scenario-G-20261006-175955-summary.md), 2/3 sorties a 9 000 pas ; obstacle d'entree retire au pas 0 (sentinelle non retiree), aucun grant/creneau de fusion publie. La confirmation proprietaire porte uniquement sur la Gate C ; ce rapport ne peut pas fermer G.
- C : journaux disponibles `scenario-C-20261006-175406-junction.txt` et `scenario-C-20261006-181212-junction.txt`, aucun nouveau resume C correspondant. Pas de conclusion de succes deduite de cette absence.

La spec reste `in-review`, sans promotion finale. Gate C confirmee verte ; C et G ne sont pas clos, et aucun succes global du profil PlayMode Story535 n'est consigne. La reprise s'arrete a ce checkpoint, sans nouvelle execution agent.

## Complement : reste de la revue demande par le proprietaire

Reprise sur le meme HEAD, sans rollback, sans promotion, sans PlayMode agent. Les lignes precedentes decrivent le checkpoint anterieur ; ce complement porte le nouvel etat du travail.

| Finding confirme | Preuve disponible | Correction |
|---|---|---|
| C n'arbitre pas deux demandes fraiches ensemble | C 18:12 : axe `Granted` source 1531, branche premiere demande source 1532, axe alors `Held`. Assertion de grant anterieur au lot commun bloquante, aucun summary. | Retrait simultane des deux obstacles, distances a b conservees. Aucun grant anterieur, grant frais `Priority`, branche `YieldToPriority` restent requis. Invariants/contacts/sorties conserves, summary garanti dans `finally` avec motif d'echec. Montage a confirmer par le proprietaire. |
| G rencontre manquee | Anneau #0 sur element avant fusion aux frames 311-392, entrant premier maintien sur obstacle a 1337, puis 50 pas stabilises requis ; deux anneaux deja passes. Obstacle non retire, 2/3 sorties. | Deuxieme obstacle independant sur l'approche initiale commune des anneaux, libere seulement une fois premier anneau et entrant stabilises. Entrant libere a l'approche de la fusion. Builder deterministe affirme route, independance des obstacles et espace d'insertion. G exige un refus sur la vraie zone de fusion avec ETA < t_gap ; un simple `ExitBlocked` ne satisfait pas sa precondition. |
| E selectionne l'arret de preparation | E premier maintien sans grant frame 1372, cause `Obstacle`, d = 10,50482 m ; retrait obstacle a 1421. Sequence du summary s'interrompt avant assertion bande. | Selection de l'arret cause `JunctionEntry`, contrainte active, faible vitesse. Aucun elargissement de bande. E/F/G consignent l'assertion d'echec dans leur summary, y compris les invariants finaux. |
| Ligne decalee apres replanification dans le mouvement | E 1794 -> 1795 : d = 0,34693 -> 1,19077 m quasi sans mouvement. Route replanifiee commence vers s = 0,84384 m, soustrait au pare-chocs mais pas a la frontiere. | Origine physique corrigee pour d, debuts des mouvements (ETA/degagement), occupation et tete de file : soustraction de `StartSMeters`. b reste l'abscisse du modele. Regression meme frame et pose, route complete puis replanifiee, avant/apres b sur les deux branches. |
| Arret marque `Stop` trop avance (intent gap P6) | Borne basse 0 acceptee par le code ; matrice affiche m_ctrl - 0,02. | **Decision proprietaire explicite : appliquer m_ctrl - 0,02 aussi a P6.** Constante `StopHaltIntegrationToleranceMeters` = 0,02 m. Tests juste sous la borne basse, exactement aux deux bornes et juste au-dessus de la borne haute. Aucun temps minimal ajoute. |
| Faux franchissement par disparition de demande | Une approche sous grant peut devenir engagee et quitter `HasRequest` avant de franchir b. | Helpers C/D/E mesurent le franchissement sur la trace de route, avec origine de la piece et porte-a-faux reel capture sur le collider au staging. Maintien/contrainte absents jusqu'au franchissement, plafond 3 s conserve. |

Revue BMAD ciblee complete sur ces corrections : edge-case des bornes P6 et routes partielles ; verification-gap des traces et montages C/E/G. Aucun autre finding confirme sur ce nouveau diff. Le rapport original introuvable reste une limite documentaire, pas une preuve de cloture de sa liste.

Validation : premier passage **42/42 EditMode Story535**, 0 skipped/inconclusive, 0 erreur Console, compilation saine, MVP_Run propre. [Sortie brute](review-rest-editmode-20261006.txt). Ce passage inclut le constructeur G regenere. Dernier passage apres correction du franchissement des helpers : **42/42**, 0 skipped/inconclusive, recompilation `completed`, `scriptCompilationFailed=false`, **0 erreur Console** depuis le curseur 1844, MVP_Run propre. [Sortie brute finale](review-rest-final-editmode-20261006.txt). 1 220 tests exclus par selection et 3 Explicit hors suite ; aucune suite complete executee. `git diff --check` conforme. La revue ciblee confirme que TrackIndex/RouteDistance proviennent de la meme reference apres replanification et que les captures d'empreinte correspondent aux vehicules verifies.

`graphify update .` execute apres les changements : 4 531 noeuds, 10 549 liens, 182 communautes ; dernier passage sans changement de topologie. Aucun modele, scene, prefab, preuve ou signature Gate A modifie.

**HALT requis pour les verdicts runtime.** Aucun PlayMode agent execute. La Gate C confirmee verte est celle des runs proprietaire precedant ce nouveau correctif runtime ; elle n'est pas transferee silencieusement au nouvel etat. Spec `in-review`, sprint-status inchange, aucune promotion ni commit final.

Commandes proprietaire, dans cet ordre (seconde apres lecture de la premiere sortie) :

```powershell
.\scripts\validate.ps1 -Profile Story -Story 5.35 -TestMode PlayMode
.\scripts\validate.ps1 -TestMode PlayMode -TestFilter Story535GateC -TestFilterType category -IncludeExplicit
```

## Execution agent autorisee apres ce checkpoint

Le proprietaire autorise explicitement les executions : « Tu as la main pour faire les executions ». Cette instruction leve l'interdiction PlayMode anterieure pour la validation Story535 et sa campagne Gate C ; l'arret avant promotion finale reste en vigueur.

- Garde avant execution : branche `systeme-traffic-ia-v2`, Editeur `ready`, MVP_Run propre ; seules les modifications expliquees et traces precedentes sur disque.
- Premier run `.\scripts\validate.ps1 -Profile Story -Story 5.35 -TestMode PlayMode` : **4/6**, exit 1. C, E, F et saturation passent. [Sortie brute conservee](review-rest-playmode-20261006.txt). Compilation saine ; aucune campagne Gate C lancee apres cet echec.
- D echoue sur l'ancien critere `ElementId == SouthLeft` avant liberation de la sortie : sous 5.35, le mouvement commence avant sa ligne b. Correction : comparer le pare-chocs reel a b sur la trace de route. Refus `ExitBlocked` prioritaire, aucun grant avant sortie suffisante, aucune occupation au-dela de b, zero contact et sorties finales restent requis.
- G : [rapport 20:10](scenario-G-20261006-201048-summary.md), **0/3 insertions**, deux obstacles non retires. L'obstacle d'anneau a 7,5 m dans la route initiale (jeu pare-chocs 5,28 m) est dans le rayon de degagement 12 m du portail et empeche la premiere insertion. Correction : le creer seulement apres la presence du premier vehicule, avant qu'il atteigne sa face proche. Autre precaution necessaire : le spawner traite les insertions dans l'ordre ; l'entrant devient index 1 / compteur 2, avant le second anneau index 2 / compteur 3 retenu au portail par le premier. Les anneaux sont indexes 0 et 2 dans tous les controles de G. Le builder affirme ce nouvel ordre et ecrit son calendrier avant PlayMode.
- Revue verification-gap du seul nouveau diff D/G : calendrier, compteurs/indices, garde de portail et timing de creation de l'obstacle revalides ; aucun autre finding confirme. `graphify update .` sans changement de topologie. MVP_Run propre apres le premier run en echec.
- Reprise `.\scripts\validate.ps1 -Profile Story -Story 5.35 -TestMode Both` : **42/42 EditMode et 6/6 PlayMode**, exit 0, 0 skipped/inconclusive, compilation saine, 0 erreur Console depuis le curseur 2070, MVP_Run propre. Nouveau calendrier G regenere par EditMode avant PlayMode. [Sortie brute](review-rest-retry-both-20261006.txt).
- C/D/E/F/G desormais clos pour ces criteres et cet etat du code : [C](../traffic-v2-5-34-explorations/scenario-C-20261006-201758-summary.md), [D](../traffic-v2-5-34-explorations/scenario-D-20261006-201858-summary.md), [E](scenario-E-20261006-201952-summary.md), [F](scenario-F-20261006-202046-summary.md), [G](scenario-G-20261006-202145-summary.md). C/D/E : arret a 0,347 / 0,348 / 0,347 m de b et franchissement 0,96 s apres grant. G : trois insertions (anneau #0 frame1, entrant #1 frame2, anneau #2 frame1373), retenues retirees 1113 / 1400, refus sur vraie fusion avec ETA 0,501 s < t_gap 5,787 s, service a 1932, **3/3 sorties**. Contacts, entrees sans grant, occupation incompatible et TrackingToleranceExceeded nuls dans ces scenarios.

Profil Story535 vert, puis campagne Gate C explicite **3/3**, exit 0, 0 skipped/inconclusive, **0 erreur Console** depuis le curseur 2224, compilation saine et MVP_Run propre. [Sortie brute](review-rest-gatec-20261006.txt). Le libelle technique `Full` du script ne change pas la portee : filtre exact `Story535GateC`, aucune suite complete executee.

| Campagne apres corrections | Sorties | Contacts V2-V2 / obstacles | Grants incompatibles hors creneau / entree sans grant / occupation incompatible | Briseurs | p95 pas hote en population pleine |
|---|---|---|---|---|---|
| [N2](gatec-2-20261006-202314-summary.md) | 2/2 | 0 / 0 | 0 / 0 / 0 | 0 | 2,189 ms |
| [N4](gatec-4-20261006-202414-summary.md) | 4/4 | 0 / 0 | 0 / 0 / 0 | 1 | 8,387 ms |
| [N8](gatec-8-20261006-202520-summary.md) | 8/8 | 0 / 0 | 0 / 0 / 0 | 1 | **8,624 ms < 10 ms** |

Aucun TrackingToleranceExceeded, aucune immobilisation interne, aucun acteur restant. N8 : p95 tous les pas 8,368 ms, 925 pas en population pleine. N4/N8 rapportent aussi deux contacts avec d'autres colliders, categorie distincte des contacts V2-V2, comme dans la campagne proprietaire precedente ; ils ne sont pas masques.

Les findings confirmes traites dans ce suivi sont corriges et leurs validations ciblees sont vertes : P3(b), ETA a vitesse excessive, preuves P5/hash/trace/responsabilites, borne basse P6, origine de route partielle, adaptation C/D/E et rencontre G. Aucune autre anomalie confirmee restante sur ce diff. Limites maintenues : rapport original introuvable non certifie ligne par ligne ; rejeux historiques hors categorie Story535 et suites completes non executes dans cette reprise. Les sorties brutes conservent le premier echec 4/6 et la correction subsequente.

**Arret avant promotion finale**, comme demande : spec `in-review`, sprint-status non promu, aucun commit de cloture. Aucun fichier modele, preuve/signature Gate A, scene ou prefab modifie pendant ces executions. `graphify update .` execute apres les dernieres modifications source ; `git diff --check` conforme.

Les autres rejeux historiques restent hors de l'execution autorisee pour cette reprise. La liste originale du rapport de revue reste a confronter quand son chemin sera fourni.

## Cloture demandee par le proprietaire

Instruction suivante : « Fais tous ça sans HALT », en reponse a la liste restante (rejeux historiques, promotion BMAD, commit et recherche du rapport). Elle autorise ces executions et leve l'arret avant promotion finale. Les checkpoints precedents restent l'historique des autorisations et resultats, pas le verdict courant.

Rapport original : recherches dans les artefacts et docs (fichiers caches/ignores compris), noms de fichiers suivis, historique Git toutes branches et emplacements BMAD des copies voisines `RRS-preservation` / `RoadRage_Simulator`. Aucun rapport 5.35 anterieur retrouve ; aucune reconstruction fictive de sa liste. Le present suivi constitue le rapport de la revue effectivement executee sur le diff actuel, avec dispositions et preuves. La limite documentaire de confrontation a la liste originale reste explicitement ouverte ; aucun finding confirme de cette revue n'est laisse sans correction.

Premier rejeu 5.31 : validation interrompue avant tests, lecture `scriptCompilationFailed` muette/code CLI 6. [Sortie d'echec conservee](closure-regression-531-20261006.txt). Etat recontrole par commandes autorisees : Editeur `ready`, recompilation `up_to_date`, MVP_Run propre ; reprise annoncee puis validation officielle relancee, sans changement du validateur ni contournement de la porte.

### Verdict courant de cloture

| Selection officielle | EditMode | PlayMode | Sortie brute |
|---|---|---|---|
| Story535 | 42/42 | 6/6 (C/D, saturation, E/F/G) | [Both final](review-rest-retry-both-20261006.txt) |
| Story535GateC, Explicit | hors selection | 3/3 (N2/N4/N8) | [Gate C](review-rest-gatec-20261006.txt) |
| Story531 | 50/50 | 13/13 | [Rejeu final](closure-regression-531-retry-20261006.txt) |
| Story552 | 26/26 | 4/4 | [Rejeu](closure-regression-552-20261006.txt) |
| Story533 | 69/69 | 12/12, A/B inclus | [Rejeu](closure-regression-533-20261006.txt) |
| Story534 | 48/48 | C/D et saturation couverts par Story535 ci-dessus | [EditMode](closure-regression-534-editmode-20261006.txt) |

Chaque validation finale : exit 0, aucun skipped/inconclusive, compilation saine, zero erreur Console dans sa fenetre, MVP_Run propre. Les fixtures ont seulement regenere leurs artefacts attendus : trace courte 5.31 et resumes [A](../traffic-v2-5-33-explorations/acceptance-A-20261006-234607-summary.md) / [B](../traffic-v2-5-33-explorations/acceptance-B-20261006-234644-summary.md). Les suites completes et campagnes historiques Explicit hors scope ne sont pas executees ; elles ne sont pas declarees reussies.

Findings confirmes clos : P3(b), ETA a vitesse excessive, negatifs P5/hash/trace, responsabilites Junction explicites, borne basse Stop P6, origine physique apres replanification, adaptation C/D/E et synchronisation G. Aucun finding confirme de cette revue reste ouvert. Limite restante : rapport original non retrouve, donc absence de certification de sa liste anterieure. La dette P11 de reservation d'une sortie et la surveillance de marge D13 restent consignees dans `deferred-work.md`, hors correctifs de cette revue ; aucun gain de debit ni garantie au-dela de N8 n'est revendique.

Promotion autorisee et appliquee : spec `done`, sprint `review`, conforme au step-05-present rendu et a `sync-sprint-status` ; `done` du sprint reste reserve au checkpoint de fin d'epic (build rules §3.2). Decisions proprietaires consignees, `Suggested Review Order` actualise depuis la baseline BMAD. Dettes StopLine et priorite d'anneau soldees avec les preuves runtime ; D13 reevalue a 8,624 ms / marge 1,376 ms. `graphify update .` execute apres les correctifs source. Controle des espaces conforme sur sources/documents : `git diff --cached --check` avec exclusion explicite de deux captures historiques d'echec qui gardent leurs octets d'origine (`review-followup-first-failure-20261006.txt`, ligne vide finale ; `scenario-G-20261006-201048-summary.md`, espaces produits dans le message NUnit). Aucun nettoyage de ces preuves brutes. Aucun modele, scene, prefab, preuve ou signature Gate A change. Commit local de cloture autorise ; aucun push automatique.
