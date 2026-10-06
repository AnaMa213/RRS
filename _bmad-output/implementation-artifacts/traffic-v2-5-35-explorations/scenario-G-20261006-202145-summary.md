# scenario-G 20261006-202145

- Scenario G, population 3, fixedDeltaTime 0.02 s, pas hote 2910, lots 2909 dont refuses 0
- Insertions : #0 00000000000000000000000000000001 au pas 1 ; #1 00000000000000000000000000000002 au pas 2 ; #2 00000000000000000000000000000003 au pas 1373
- Retraits au portail : 3/3
- Sequence :
  - obstacle d'anneau retire au pas hote 1113
  - obstacle de l'entree retire au pas hote 1400
  - 1. premier refus de l'entrant : 00000000000000000000000000000002 Denied(ConflictGranted) 4c8d26eb6c05c178fc8444e842e07d8c traversee 43605e569eb08d6ffe62fa7470d59fa0 [43605e569eb08d6ffe62fa7470d59fa0,42e993cf17d9aad93755cb544a0d02a9,45607ec286d32b63e09ca677f22031ba] depuis 1510 source 1726 effectif 1727 expire apres 1727 cause @00000000000000000000000000000003 zone 4df53fca19b7ee5211d4e512b3a4da86 genre Yield t_gap 5.787 ETA 0.501
  - 2. raisons des refus : ConflictGranted
  - 3. grant de l'entrant au lot 1932
- Coordination : grants incompatibles simultanes 0, EnteredWithoutGrant 0, IncompatibleOccupancy 0
- Regles 5.35 : StopRequired 0, YieldToPriority 0, GrantedMergeGap 0, refus de creneau 7, refus Crossing 0, briseurs 0
- Cout du coordinateur : moyenne 0.024 / mediane 0.022 / p95 0.034 / max 3.193 ms (2908 pas) ; pas hote moyenne 1.716 / mediane 1.622 / p95 2.314 / max 11.635 ms (2908 pas)
- Contacts entre vehicules 0, obstacles 0, autres 0
- Couverture : aucun TrackingToleranceExceeded
- Invariants : verts
- Trace brute : `scenario-G-20261006-202145-steps.tsv` ; journal de coordination : `scenario-G-20261006-202145-junction.txt`
