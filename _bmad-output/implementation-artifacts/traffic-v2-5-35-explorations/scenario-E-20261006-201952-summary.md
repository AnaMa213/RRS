# scenario-E 20261006-201952

- Scenario E, population 2, fixedDeltaTime 0.02 s, pas hote 2639, lots 2638 dont refuses 0
- Insertions : #0 00000000000000000000000000000001 au pas 1 ; #1 00000000000000000000000000000002 au pas 2
- Retraits au portail : 2/2
- Sequence :
  - obstacle de la branche retire au pas hote 1421, obstacle de l'axe au pas hote 1422
  - 1. 00000000000000000000000000000001 Denied(YieldToPriority) 442bd8af1793e34f2d407ec98f9e6581 traversee 4e437f94525852d3a072a538537c2093 [4e437f94525852d3a072a538537c2093] depuis 1531 source 1531 effectif 1532 expire apres 1532 cause @00000000000000000000000000000002 zone 4258af5419bba1365a3f0ad6ed3d44aa genre Yield t_gap 8 ETA 2.03
  - 2. grant de l'axe au lot 1532, de la branche au lot 1840
  - 3. arret a d = 0.347 m de la frontiere (b = 3.411 m), bande [0.23 ; 0.352] m ; grant effectif au pas hote 1841, frontiere franchie au pas hote 1889 (0.96 s)
- Coordination : grants incompatibles simultanes 0, EnteredWithoutGrant 0, IncompatibleOccupancy 0
- Regles 5.35 : StopRequired 0, YieldToPriority 2, GrantedMergeGap 0, refus de creneau 103, refus Crossing 0, briseurs 0
- Cout du coordinateur : moyenne 0.022 / mediane 0.021 / p95 0.029 / max 3.298 ms (2637 pas) ; pas hote moyenne 1.318 / mediane 1.21 / p95 2.071 / max 9.983 ms (2637 pas)
- Contacts entre vehicules 0, obstacles 0, autres 0
- Couverture : aucun TrackingToleranceExceeded
- Invariants : verts
- Trace brute : `scenario-E-20261006-201952-steps.tsv` ; journal de coordination : `scenario-E-20261006-201952-junction.txt`
