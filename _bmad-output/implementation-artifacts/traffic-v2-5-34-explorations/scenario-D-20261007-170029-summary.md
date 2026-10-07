# scenario-D 20261007-170029

- Scenario D, population 2, fixedDeltaTime 0.02 s, pas hote 2981, lots 2980 dont refuses 0
- Insertions : #0 00000000000000000000000000000001 au pas 1 ; #1 00000000000000000000000000000002 au pas 2
- Retraits au portail : 2/2
- Sequence :
  - obstacle d'attente retire au pas hote 1386, obstacle de sortie retire au pas hote 2053
  - premier tenu sur 40e937a99618cac3ce12d56586514283 au pas hote 1386, grant 40ca7f10 libere
  - refus du second : 00000000000000000000000000000002 Denied(ExitBlocked) 442bd8af1793e34f2d407ec98f9e6581 traversee 4e437f94525852d3a072a538537c2093 [4e437f94525852d3a072a538537c2093] depuis 1522 source 1522 effectif 1523 expire apres 1523 borne Occupant genre Yield
  - arret du second a d min 0.348 m, blocker BlockedExit:40e937a99618cac3ce12d56586514283, servi au lot 2180
  - O14 : arret a d = 0.348 m, bande [0.23 ; 0.352] m ; grant effectif au pas hote 2181, frontiere b = 3.411 m franchie au pas hote 2229 (0.96 s, 0.884 m/s)
- Coordination : grants incompatibles simultanes 0, EnteredWithoutGrant 0, IncompatibleOccupancy 0
- Cout du coordinateur : moyenne 0.02 / mediane 0.019 / p95 0.029 / max 0.247 ms (2979 pas) ; pas hote moyenne 1.353 / mediane 1.244 / p95 2.086 / max 10.566 ms (2979 pas)
- Contacts entre vehicules 0, obstacles 0, autres 0
- d max au pas : 00000000000000000000000000000001 0.1534, 00000000000000000000000000000002 0.2491
- Couverture : aucun TrackingToleranceExceeded
- Invariants : verts
- Trace brute : `scenario-D-20261007-170029-steps.tsv` ; journal de coordination : `scenario-D-20261007-170029-junction.txt`
