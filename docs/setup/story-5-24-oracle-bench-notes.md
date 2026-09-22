# Story 5.24 — banc d'oracle de regression V2 et contrat de trace V1

## Ce qui est mesure

`TrafficOracleTests.ReplayHarnessCpuCostIsMeasuredAsObservationalData` mesure le cout du **harnais de
rejeu EditMode lui-meme** (`ReplayForTrace` : `ComputeSeekIntent` + `ResolveLookAheadPoint` +
resolution de noeud pondere, par pas, jusqu'a portail de sortie) sur le district reellement authore de
`MVP_Run`. Ce n'est **pas** le cout de `NetworkedAIVehicleDriverController.FixedUpdate` en runtime
complet (physique, perception par sphere-cast, Netcode) : c'est le meme sous-ensemble de fonctions
pures que celui deja rejoue par `Story510LaneGraphAndRoutedTrafficTests.ReplayRoute`, mesure en tant
que cout du banc de non-regression et non comme un budget V1.

## Banc reproductible

- Machine : poste de developpement RRS ; Unity 6000.6.0f1, Editeur deja ouvert (CLI 1.0.0-beta.8).
- Scene : `Assets/RoadRage/App/Scenes/MVP_Run.unity`, `LaneGraph` reel sous `RunRoot`, premier portail
  d'entree (`graph.EntryPortals[0]`).
- 40 rejeux complets portail-a-portail, une graine differente par rejeu (`1000 + i`), chronometres
  individuellement avec `System.Diagnostics.Stopwatch` (meme patron que
  `docs/setup/story-5-15-credible-collisions-and-damage-notes.md`) ; mediane et p95 calcules sur les
  echantillons tries.

## Resultat observationnel

Dernier passage journalise (Console, niveau info) :

> `[Story5.24] EditMode replay-harness cost over 40 full portal-to-exit replays on MVP_Run: median
> 0.639 ms, p95 4.880 ms per full replay.`

Un passage anterieur du meme test sur la meme machine a mesure mediane 0,680 ms / p95 2,053 ms : l'ecart
de p95 entre les deux passages (bruit JIT/GC EditMode sur un echantillon de 40) est attendu et n'est pas
un signal de regression -- aucun seuil de performance n'est gate par ce test (Design Notes : donnee
observationnelle uniquement, pas un budget CPU V1). Le test n'affirme que la finitude de la mesure.

## Resultats catalogue et auto-verification

- `OracleCatalog` : 41 lignes `V1-A01`..`V1-G04` transcrites depuis `V1-BEHAVIORAL-ORACLE.md`, toutes
  classifiees (`EveryCatalogRowResolvesToANamedClassification`) ; les 3 lignes `GAP` (`V1-F04`-`V1-F06`)
  ne citent aucun test (`GapRowsCarryNoInventedTest`).
- `ShapeGuardRegister` : 4 entrees, toutes nommees, chacune liee a un `BoundTest` reellement classifie
  `SOURCE-GUARD` sur sa ligne de catalogue (`EveryShapeGuardEntryCitesABoundTestActuallyClassifiedSourceGuardOnItsRow`).
- `TrafficTraceComparer` : auto-verification verte -- traces identiques comparees egales ; une divergence
  injectee hors tolerance est nommee sur chacun des champs discrets (`VehicleId`, `RoadElementId`,
  `Goal`, `Blockers`) et numeriques (`Position`, `YawDegrees`, `Speed`, `RouteProgress`, et les quatre
  composantes de `FinalIntent` -- `Throttle`/`Steer`/`BrakeReverse`/`Handbrake`) ; un cas `NaN` sur un
  champ numerique est rapporte comme divergence, jamais comme egalite silencieuse. Revue 2 : une
  divergence sur DEUX champs de la meme frame (`VehicleId` et `Speed`) est prouvee rapportee en entier,
  pas seulement la premiere trouvee (`MultipleDivergencesOnTheSameFrameAreAllReportedNotJustTheFirst`).
- Determinisme : `DeterministicSeededReplayProducesEqualTraceSequencesAndReachesAnExitPortal` rejoue le
  meme scenario segmente sur deux passages a graine identique ; les deux rejeux atteignent effectivement
  un portail de sortie apres depart (pas seulement `maxSteps`), et les deux sequences de trace comparent
  egales sous le contrat declare.
- Existence reflechie : `EveryCitedTestFullNameResolvesToAnExistingTestMethod` resout chaque
  `BoundTest`/`ShapeGuardEntry` cite vers une methode `[Test]`/`[UnityTest]` reellement presente,
  recherchee sur tous les assemblies charges dans le domaine Editeur (les fixtures `AUTO-PLAY` vivent
  dans `RoadRage.Tests.PlayMode`, un assembly volontairement non reference par
  `RoadRage.Tests.EditMode.asmdef` -- voir Code Map de la spec). Revue 2 : le cas negatif est prouve
  separement (`ResolvesToATestMethodReturnsFalseForAFabricatedNonexistentName`) -- un nom de fixture
  invente n'a jamais resolu, ce qu'aucun test ne verifiait avant puisque le test de completude n'exerce
  la fonction que sur des donnees de catalogue deja valides.

## Revue 2 (robustesse, pas de bug de correction)

Une deuxieme passe de revue sur `TrafficTraceComparer.cs`/`TrafficOracleTests.cs` n'a trouve aucun bug
de correction dans les corrections de la revue 1, mais trois trous de couverture/robustesse, tous
corriges dans cette passe :

1. Aucun test ne prouvait que `Compare` rapporte TOUTES les divergences d'une frame (pas seulement la
   premiere) -- ajoute (`MultipleDivergencesOnTheSameFrameAreAllReportedNotJustTheFirst`).
2. Aucun test ne prouvait que `ResolvesToATestMethod` peut effectivement echouer -- ajoute
   (`ResolvesToATestMethodReturnsFalseForAFabricatedNonexistentName`).
3. `ReplayForTrace` reinitialisait `lastNodeChangeStep` a chaque declenchement de la branche d'arrivee,
   meme quand `ResolveNextTraceNode` rend le MEME noeud (branche `return current;` sans sortie
   atteignable) -- affaiblissement de la garde d'orbite (900 pas), sans faux positif de test (le rejeu
   aurait quand meme fini par echouer via `ReplayMaxSteps`). Corrige : la reinitialisation ne se fait
   que si le noeud resolu differe reellement du noeud precedent.

Deux suggestions de cette meme passe ont ete evaluees et rejetees comme bruit : un null-check sur
`TrafficTrace.Frames` (inatteignable, `readonly List<>` initialisee inline) ; rendre `BlockersEqual`
insensible a l'ordre (rejete -- l'egalite exacte et sensible a l'ordre sur `Blockers` est une decision
de conception deliberee, deja documentee dans la spec/Design Notes, pas un bug).

## Verification

- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.TrafficOracleTests"` :
  24/24 verts (22 + 2 nouveaux tests de la revue 2).
- `.\scripts\validate.ps1 -TestMode EditMode` : 673/673 verts, 0 erreur Console, aucun test V1 regresse.
