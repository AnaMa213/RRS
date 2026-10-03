# Diagnostic de performance Traffic V2 (Story 5.33, 2026-10-02)

Demande proprietaire du 2026-10-02 apres la video du run explore-8 : diagnostic cible avant toute optimisation, sans
modification du comportement de trafic, des portees, des buffers ni de la population maximale. Le run explore-8 laggy
n'est pas une baseline de performance : il reste la preuve comportementale de la collision qui a motive D12.

## Methode

- Fixture `Story533PerformanceDiagnosticPlayModeTests` (`[Explicit]`, categorie `Story533Perf`), lancee par
  `validate.ps1 -TestMode PlayMode -TestFilter Story533Perf -TestFilterType category -IncludeExplicit`, 4/4 verts.
- Scenario nominal explore-4 tronque a N = 1, 2, 3, 4 : meme route A, obstacle de file jusqu'au pas 1800, aucune poussee.
  Conditions des runs d'acceptation : pilotage au pas physique, observateur du harnais a chaque pas, trace au pas active.
- Instrumentation sans effet sur les decisions : `TrafficV2StepRunner.LastCost` (temps par section du pas hote),
  `V2StageTimings` etendu (preparation, arbitrage, MotionCommand, instrumentation), marqueurs `ProfilerMarker` lus par
  `ProfilerRecorder` a chaque frame rendue (frame, spine, physique, GC).
- Limites : Editeur, pas un build joueur ; `runInBackground` vrai ; `GC.GetAllocatedBytesForCurrentThread` rend 0 dans ce
  runtime, donc pas d'attribution des allocations par section (seul le compteur moteur par frame est disponible).

Rapports bruts : `perf-N1..N4-*-summary.md`, `perf-N1..N4-*-frames.tsv`, `perf-comparison-20261002-223123.md`.

## Resultats

Cout moyen du pas Traffic V2 par pas hote, sur toute la duree du run :

| N | pas Traffic V2 | marginal | pics > 20 / 50 / 100 ms | FixedUpdate max par frame | collections gen0 / min | pause GC moy. / max |
|---|---|---|---|---|---|---|
| 1 | 4,36 ms | -- | 31 / 1 / 0 | 6 | 60 | 3,8 / 7,1 ms |
| 2 | 7,42 ms | +3,06 | 150 / 0 / 0 | 3 | 113 | 4,4 / 8,0 ms |
| 3 | 10,06 ms | +2,64 | 451 / 24 / 0 | 4 | 157 | 4,8 / 8,0 ms |
| 4 | 13,19 ms | +3,13 | 331 / 179 / 110 | 17 | 186 | 7,4 / 18,3 ms |

Phase a population pleine (N = 4, pas 800-2400) : 19,4 a 21,3 ms par pas hote pour le seul Traffic V2, soit plus que le
pas fixe de 20 ms. Unity rattrape jusqu'a 17 FixedUpdate par frame (plafond `maximumDeltaTime` 0,333 s) : frames de 350 a
427 ms, simulation ralentie (pas simule/reel 0,963). C'est le gel visible dans la video.

Repartition a N = 4 en phase pleine (ms par pas hote) : occupation de la frame 6,8 ; SpeedPlan 6,5 ; horizon 5,2 ;
perception 1,3 ; localisation de frame 0,45 ; tout le reste < 0,2 (collecteur 0,07 dont 3 OverlapSphereNonAlloc a 0,009
ms ; arbitrage 0,07 ; MotionCommand 0,02 ; composition 0,07 ; instrumentation des drivers 0,10 ; observateur du harnais
0,01 ; Physics.Simulate 0,08). Aucune ecriture disque pendant le run (trace TSV en fin de run : 191 ms, une fois).

Le cout depend de la phase, pas du mouvement : l'horizon couvre toute la route restante (`TrafficV2Settings`), donc
horizon et SpeedPlan decroissent avec la route restante ; l'occupation depend de la longueur de l'element occupe (0,8 a
2,6 ms pour un seul vehicule selon l'element).

## Operations periodiques

Aucun traitement toutes les N frames dans le chemin Traffic V2 ni dans le spawner : admission en cache, aucun flush,
aucun rapport, aucune reconstruction periodique, aucune ecriture disque pendant le run. Le seul phenomene periodique est
le GC gen0 : environ une collection par seconde et par vehicule (60 a 186 par minute), pauses de 4 a 7 ms en moyenne,
18 ms au plus a N = 4. Le compteur moteur donne ~0,45 Mo alloues par vehicule et par pas hote (0,95 Mo par frame rendue
a N = 4). Aux faibles N, ce sont ces pauses, ajoutees a un pas de 4 a 7 ms, qui produisent les pics reguliers ; a
N = 4, la spirale de FixedUpdate domine (127 des 179 pics > 50 ms contiennent aussi une collection).

## Classement

1. **Cout lineaire eleve par vehicule : oui, cause principale.** Marginal constant d'environ 3 ms par vehicule en moyenne,
   ~5 ms par vehicule en phase pleine (occupation ~1,7, SpeedPlan ~1,6, horizon ~1,3, perception ~0,33).
2. **Travail commun repete : oui, sous deux formes.** Dans le temps : chaque pas reconstruit horizon et plan de vitesse sur
   toute la route restante, alors qu'ils changent peu d'un pas au suivant (dette 508 de la 5.31). Dans le modele :
   `TrafficFrame.TryOccupy` projette ~130 points du perimetre de l'empreinte sur toute la courbe de l'element et recalcule
   la courbure maximale de l'element, donnee statique. Le collecteur, lui, est negligeable (0,02 ms par vehicule).
3. **Quadratique : non.** Seule la perception par vehicule croit avec N (0,157 -> 0,189 ms), sans effet notable.
4. **Instrumentation ou harnais : non.** Instrumentation des drivers ~0,025 ms par vehicule, observateur 0,01 ms par pas.
5. **Arriere de FixedUpdate : oui, en consequence.** Des que le pas depasse le pas fixe (N = 4 en phase pleine, a la
   limite a N = 3, p95 16 ms), la boucle physique decroche en spirale jusqu'au plafond de `maximumDeltaTime`.

## Verdict sur D7

Le seuil D7 (relatif : 2 fois la pire moyenne 5.31) passe, mais il ne dit rien du budget absolu : a N = 4, le seul pas
Traffic V2 consomme le pas fixe complet. La dette de cout 5.31 (entree 508 de `deferred-work.md`) est devenue bloquante :
a traiter avant la 5.34, qui ajoute des requetes de carrefour par vehicule.

## Optimisations proposees (non realisees, HALT)

1. **Occupation de la frame** : projection fenetree autour de l'abscisse localisee (s +/- demi-diagonale de l'empreinte +
   marge) au lieu de toute la courbe, et courbure maximale par element mise en cache dans le modele compile. Meme resultat
   sous la condition de continuite locale deja exigee par la borne 5.32 (entree 516 de `deferred-work.md`). Gain attendu :
   ~1,7 -> ~0,05 ms par vehicule.
2. **Horizon et plan de vitesse** : ne plus tout reconstruire sur la route restante a chaque pas. Option a, sans
   changement de comportement : horizon incremental (reprendre la geometrie du pas precedent, ne transporter e que sur le
   troncon nouveau, piste 508), plan de vitesse reconstruit sur la meme fenetre ; preuve par rejeu EditMode au bit pres et
   PlayMode 5.31/5.52 inchanges. Option b, plus simple mais avec changement de comportement : borner le look-ahead (distance
   de freinage depuis la vitesse maximale, plus marge) avec une condition terminale non bloquante ; decision proprietaire
   et revalidation Gate B. Gain attendu : ~3 -> ~0,3 a 0,6 ms par vehicule.
3. **Allocations** : listes et tableaux reutilises pour l'horizon, le plan et l'occupation ; projection de diagnostic
   construite a la lecture et non a chaque pas (`DriveOutcome`). Attribution prealable par section avec un compteur
   disponible dans ce runtime (`Profiler.GetMonoUsedSizeLong` par section, en ecartant les intervalles qui contiennent une
   collection).
4. **Hors sujet** : collecteur, arbitrage, composition, harnais, physique : chacun < 0,1 ms par vehicule.

Cible proposee pour accepter la 5.33 et ouvrir la 5.34 : pas Traffic V2 <= 1 ms par vehicule en phase pleine, N = 8 sous
10 ms par pas hote, aucune frame a plus de 2 FixedUpdate sur le scenario explore-4 a N = 4, mesure par la meme fixture.
