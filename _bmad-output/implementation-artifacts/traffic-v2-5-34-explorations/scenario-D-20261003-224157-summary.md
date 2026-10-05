# scenario-D 20261003-224157

- Scenario D, population 2, fixedDeltaTime 0.02 s, pas hote 2995, lots 2994 dont refuses 0
- Insertions : #0 00000000000000000000000000000001 au pas 1 ; #1 00000000000000000000000000000002 au pas 2
- Retraits au portail : 2/2
- Sequence :
  - obstacle d'attente retire au pas hote 1386, obstacle de sortie retire au pas hote 1929
  - premier tenu sur 40e937a99618cac3ce12d56586514283 au pas hote 1386, grant 40ca7f10 libere
  - refus du second : 00000000000000000000000000000002 Denied(ExitBlocked) 442bd8af1793e34f2d407ec98f9e6581 traversee 4e437f94525852d3a072a538537c2093 [4e437f94525852d3a072a538537c2093] depuis 1496 source 1496 effectif 1497 expire apres 1497 borne Occupant
  - arret du second a d min 2.448 m, blocker BlockedExit:40e937a99618cac3ce12d56586514283, servi au lot 2056
- Coordination : grants incompatibles simultanes 0, EnteredWithoutGrant 0, IncompatibleOccupancy 0
- Cout du coordinateur : moyenne 0.021 / mediane 0.015 / p95 0.023 / max 7.425 ms (2993 pas) ; pas hote moyenne 1.35 / mediane 1.179 / p95 2.139 / max 55.971 ms (2993 pas)
- Contacts entre vehicules 0, obstacles 0, autres 0
- d max au pas : 00000000000000000000000000000001 0.1534, 00000000000000000000000000000002 0.2491
- Couverture : aucun TrackingToleranceExceeded
- Invariants : verts
- Trace brute : `scenario-D-20261003-224157-steps.tsv` ; journal de coordination : `scenario-D-20261003-224157-junction.txt`
