# scenario-G 20261006-175955

- Scenario G, population 3, fixedDeltaTime 0.02 s, pas hote 9000, lots 8999 dont refuses 0
- Insertions : #0 00000000000000000000000000000001 au pas 1 ; #1 00000000000000000000000000000002 au pas 284 ; #2 00000000000000000000000000000003 au pas 285
- Retraits au portail : 2/3
- Sequence :
  - obstacle de l'entree retire au pas hote 0
- Coordination : grants incompatibles simultanes 0, EnteredWithoutGrant 0, IncompatibleOccupancy 0
- Regles 5.35 : StopRequired 0, YieldToPriority 0, GrantedMergeGap 0, refus de creneau 0, refus Crossing 0, briseurs 0
- Cout du coordinateur : moyenne 0.019 / mediane 0.018 / p95 0.028 / max 3.149 ms (8999 pas) ; pas hote moyenne 0.913 / mediane 0.683 / p95 2.164 / max 10.221 ms (8999 pas)
- Contacts entre vehicules 0, obstacles 0, autres 0
- Couverture : aucun TrackingToleranceExceeded
- Invariants : verts
- Trace brute : `scenario-G-20261006-175955-steps.tsv` ; journal de coordination : `scenario-G-20261006-175955-junction.txt`
