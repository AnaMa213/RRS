# Resultats D15 : optimisation structurelle exacte (Story 5.33)

Proposition : `proposal-20261003-structural-optimization.md`. Banc : `Story533RoundaboutBench`, EditMode, meme methode a
chaque etape. Les rapports de chaque etape sont conserves : `d15-step0-baseline-bench.md` (avant D15), `d15-step1-bench.md`,
`d15-step3-bench.md`, `d15-step4-bench.md`, `d15-step5-bench.md`, `d15-step6-bench.md`. Aucun comportement change : chaque
etape est prouvee au bit pres avant d'etre mesuree.

## Pas Traffic V2 total (ms, banc, moyenne sur les ancres)

| Etape | LD N=1 | RP N=1 | LD N=2 | RP N=2 | LD N=4 | RP N=4 | LD N=8 | RP N=8 |
|---|---|---|---|---|---|---|---|---|
| 0. avant D15 (D14 seul) | 0,753 | 1,421 | 1,397 | 3,050 | 3,900 | 7,113 | 10,333 | 17,750 |
| 1. projection exacte acceleree + amorce chainee de l'occupation | 0,672 | 0,840 | 1,282 | 1,816 | 3,092 | 3,752 | 6,520 | 9,037 |
| 3. obstacles : rejet prouve, projection mutualisee, index des non mesures | 0,637 | 0,836 | 1,182 | 1,642 | 2,798 | 3,427 | 5,066 | 7,038 |
| 4. verification du profil par curseurs | 0,496 | 0,726 | 0,913 | 1,402 | 1,986 | 2,892 | 3,961 | 5,921 |
| 5. projection compacte de l'occupation | 0,471 | 0,686 | 0,896 | 1,353 | 1,907 | 2,782 | 3,773 | 5,706 |
| 2. index spatial de localisation (final) | 0,480 | 0,679 | 0,829 | 1,356 | 2,132 | 2,775 | 3,786 | **5,682** |

LD : ligne droite ; RP : rond-point. Rond-point N = 8 : 17,75 -> 5,68 ms (-68 %). Ligne droite N = 8 : 10,33 -> 3,79 ms
(-63 %).

## Cout par vehicule (ms) : la croissance quadratique a disparu

| Section | RP N=1 avant | RP N=8 avant | RP N=1 apres | RP N=8 apres |
|---|---|---|---|---|
| pas total / N | 1,42 | 2,22 | 0,68 | 0,71 |
| frame (localisation + occupation) / N | 0,73 | 0,65 | 0,27 | 0,24 |
| perception / N | 0,23 | **0,99** | 0,12 | **0,16** |
| SpeedPlan / N | 0,26 | 0,30 | 0,14 | 0,14 |
| spine / N | 0,11 | 0,15 | 0,12 | 0,12 |

Avant, la perception etait multipliee par 4,2 de N = 1 a N = 8 ; apres, par 1,3. La croissance residuelle vient de
l'enumeration de la boite de route (tests de boites, conservee pour le compte de saturation) et des faits eux-memes. Le
pas complet est desormais O(N), avec environ 0,7 ms par vehicule en rond-point et 0,47 ms en ligne droite.

## Travail par vehicule et par pas, rond-point N = 8 (compteurs du runtime)

| Grandeur | avant | apres |
|---|---|---|
| ProjectNearest (occupation) : segments evalues | 4 123 | 586 (132 appels) |
| RoadCurve.Project : segments evalues | 12 164 | ~300 |
| perception : projections d'obstacle | 86,6 | 9 |
| localisation : elements examines | 116 | 11,8 |
| verification : recherches dichotomiques par noeud | 3 | 0 (curseurs) |

## Preuves (EditMode, categorie Story533)

- Projection acceleree contre le parcours lineaire recopie : 116 elements, 18 560 plages (bornes sur des echantillons
  exacts, interieures, effondrees, inversees, hors domaine, minuscules), 18 560 domaines complets, 37 120 amorces dont la
  projection compacte : 0 difference au bit pres.
- Canal obstacles contre la reference d'avant D15 recopiee : 876 observations, 4 402 faits, 465 observations saturees
  (tampon normal et tampon de 3) : 0 difference sur les faits, le total, la saturation et le compte spatial.
- Index de localisation : 2 918 points (coins exacts des boites et nuages autour de chaque courbe), 17 128 elements
  contenants, 0 absent, 0 desordre ; 9,2 elements examines par point au lieu de 116.
- Preuves D14 inchangees et vertes : horizon optimise, verification fusionnee (couvre les curseurs), planification bornee,
  perception sur le chemin complet, projection elaguee.

## Ce qui a change dans le code (runtime)

- `RoadCurve` :
  - `Project` et `ProjectNearest` partagent un coeur exact : plage par dichotomie, amorce par la boite la plus proche ou
    par l'indice, elagage par super-blocs, blocs et segments ;
  - `ProjectNearestCompact` (s, distance, depassement) ;
  - `MaximumChordTangentAngleRadians`.
- `TrafficFrame` :
  - amorce chainee et projection compacte de l'occupation ;
  - index par element des acteurs non mesures ;
  - memo des projections d'obstacle par (entree, element, plage).
- `TrafficPerception` : rejet prouve d'une etendue avant toute projection ; projection d'obstacle partagee par la frame ;
  `NoteUnmeasured` sur l'index.
- `MotionPlan.VerifySpeedProfile` : curseurs monotones au lieu de recherches dichotomiques.
- `RoadLocalizer` : index spatial des elements par modele (grille XZ de 8 m, construite une fois) ;
  `ExaminedElements` expose la cellule pour la preuve.
- `TrafficV2WorkCounters` : compteurs de travail diagnostiques (increments seuls).

## PlayMode (2026-10-03, Domain Reload actif, sans redemarrage entre les runs)

### Non-regression

- **Story531** : 13/13. Trace identique ligne a ligne a celle d'avant D15 (937 lignes, 29 colonnes, 0 difference).
- **Story552** : 4/4. Gate B jalon 1 inchangee : 936 pas, d max 0,1993218 m, v/v* 0,3443, 0 contact, reponse 2a au
  pas 95.
- **Story533** : 2/2, deux runs. A et B tiennent leurs criteres : StopHold a 2,497-2,499 m, aucun rampement, d max <= 0,2 m,
  aucun TrackingToleranceExceeded.

Les traces multi-vehicules ne sont pas repetables bit a bit d'un run a l'autre, et ce n'est pas lie a D15 :
- les deux runs d'apres D15 different entre eux a la frame 216 (A) et 222 (B) ;
- le second reproduit les traces d'avant D14 jusqu'a la frame 1 650 (A) et 1 026 (B) ;
- les deux runs A d'avant D15 differaient deja entre eux a la frame 1 073.

C'est un constat a suivre (determinisme PlayMode multi-vehicule), pas une regression.

### Story533Perf contre la cible D13 (`perf-comparison-20261003-114819.md`)

Cout du pas Traffic V2 selon la population vivante (frames d'un seul pas fixe) :

| Run | Vehicules vivants | Pas | Moyenne | p95 | Max |
|---|---|---|---|---|---|
| explore-4 | 4 | 1 970 | 3,06 ms | 3,27 ms | 18,2 ms |
| explore-8 | 8 | 918 | 5,85 ms | 8,77 ms | 30,7 ms |

| Cible D13 | Avant (2026-10-02) | Apres D14 + D15 | Verdict |
|---|---|---|---|
| <= 1 ms par vehicule en population pleine | ~5 ms | 0,77 ms (N = 4), 0,73 ms (N = 8) | tenue |
| N = 8 sous 10 ms par pas hote | non mesurable (cascade) | moyenne 5,85 ms, p95 8,77 ms | tenue en moyenne et au p95 ; pics isoles jusqu'a 30,7 ms |
| <= 2 FixedUpdate par frame, explore-4, N = 4 | jusqu'a 17 | 0,2 en moyenne, 7 frames a plus d'un pas, 1 frame a 3 | tenue sauf une frame a 3 |

Pics de frame rendue > 50 ms : au plus 1 par run (N = 1 a 8). Collections gen0 : 18 a 58 par minute selon N.

**Commandes du 2026-10-03 :**
- `validate.ps1 -TestMode PlayMode -TestFilter Story533Perf -TestFilterType category -IncludeExplicit` : 5/5 ;
- profils Story 5.31, 5.52 et 5.33 PlayMode : verts.

## Pour une flotte de 30 vehicules

Le pas est O(N) avec ~0,5 a 0,7 ms par vehicule, soit ~15 a 21 ms pour 30 vehicules : c'est encore trop pour un pas de
20 ms. Les postes restants sont propres a chaque vehicule :
- SpeedPlan et verification : 0,14 ms ;
- occupation : 0,2 ms en rond-point ;
- perception : 0,12 a 0,16 ms ;
- spine : 0,12 ms.

Pistes de mutualisation pour une story ulterieure (non appliquees) :
- partage du plan de vitesse et de l'horizon entre vehicules de meme route et de meme etat ;
- occupation incrementale d'un pas au suivant (amorce temporelle par vehicule) ;
- enumeration spatiale par element plutot que par boite de route, si le compte de saturation peut etre redefini.
