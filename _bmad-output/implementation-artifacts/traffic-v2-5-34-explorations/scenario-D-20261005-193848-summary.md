# scenario-D 20261005-193848

- Scenario D, population 2, fixedDeltaTime 0.02 s, pas hote 3052, lots 3051 dont refuses 0
- Insertions : #0 00000000000000000000000000000001 au pas 1 ; #1 00000000000000000000000000000002 au pas 2
- Retraits au portail : 2/2
- Sequence :
  - obstacle d'attente retire au pas hote 1386, obstacle de sortie retire au pas hote 2008
  - premier tenu sur 40e937a99618cac3ce12d56586514283 au pas hote 1386, grant 40ca7f10 libere
  - refus du second : 00000000000000000000000000000002 Denied(ExitBlocked) 442bd8af1793e34f2d407ec98f9e6581 traversee 4e437f94525852d3a072a538537c2093 [4e437f94525852d3a072a538537c2093] depuis 1496 source 1496 effectif 1497 expire apres 1497 borne Occupant
  - arret du second a d min 0.348 m, blocker BlockedExit:40e937a99618cac3ce12d56586514283, servi au lot 2135
  - O14 : arret a d = 0.348 m, bande [0.23 ; 0.352] m ; grant effectif au pas hote 2136, entree au pas hote 2249 (2.26 s, 2.617 m/s)
- Coordination : grants incompatibles simultanes 0, EnteredWithoutGrant 0, IncompatibleOccupancy 0
- Cout du coordinateur : moyenne 0.02 / mediane 0.017 / p95 0.025 / max 8.065 ms (3050 pas) ; pas hote moyenne 1.295 / mediane 1.15 / p95 1.964 / max 10.681 ms (3050 pas)
- Contacts entre vehicules 0, obstacles 0, autres 0
- d max au pas : 00000000000000000000000000000001 0.1534, 00000000000000000000000000000002 0.2491
- Couverture : aucun TrackingToleranceExceeded
- Invariants : verts
- Trace brute : `scenario-D-20261005-193848-steps.tsv` ; journal de coordination : `scenario-D-20261005-193848-junction.txt`
