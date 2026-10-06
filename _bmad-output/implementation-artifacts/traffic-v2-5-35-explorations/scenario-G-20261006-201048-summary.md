# scenario-G 20261006-201048

- Scenario G, population 3, fixedDeltaTime 0.02 s, pas hote 9000, lots 8999 dont refuses 0
- Insertions : 
- Retraits au portail : 0/3
- Sequence :
  - obstacle d'anneau retire au pas hote 0
  - obstacle de l'entree retire au pas hote 0
  - ECHEC :   le vehicule d'anneau et l'entrant ne se sont pas stabilises derriere leurs obstacles
  Expected: greater than 0
  But was:  0

- Coordination : grants incompatibles simultanes 0, EnteredWithoutGrant 0, IncompatibleOccupancy 0
- Regles 5.35 : StopRequired 0, YieldToPriority 0, GrantedMergeGap 0, refus de creneau 0, refus Crossing 0, briseurs 0
- Cout du coordinateur : aucun pas ; pas hote aucun pas
- Contacts entre vehicules 0, obstacles 0, autres 0
- Couverture : aucun TrackingToleranceExceeded
- Invariants : verts
- Trace brute : `scenario-G-20261006-201048-steps.tsv` ; journal de coordination : `scenario-G-20261006-201048-junction.txt`
