# scenario-F 20261007-143354

- Scenario F, population 2, fixedDeltaTime 0.02 s, pas hote 2653, lots 2652 dont refuses 0
- Insertions : #0 00000000000000000000000000000001 au pas 1 ; #1 00000000000000000000000000000002 au pas 2
- Retraits au portail : 2/2
- Sequence :
  - obstacle de gauche retire au pas hote 1051, obstacle de droite au pas hote 1052
  - 1. grant de droite au lot 1161, de gauche au lot 1501
  - 2. premier refus de gauche : 00000000000000000000000000000001 Denied(YieldToPriority) 490b6106a4522c5dfec0040c66cd82b9 traversee 4b0942d72a850d61b60bc3e8a6a52384 [4b0942d72a850d61b60bc3e8a6a52384] depuis 1160 source 1160 effectif 1161 expire apres 1161 cause @00000000000000000000000000000002 zone 4f93788f6768a9c8d22bd432fff01490 t_gap 8.787 ETA 2.033
- Coordination : grants incompatibles simultanes 0, EnteredWithoutGrant 0, IncompatibleOccupancy 0
- Regles 5.35 : StopRequired 0, YieldToPriority 2, GrantedMergeGap 0, refus de creneau 0, refus Crossing 113, briseurs 0
- Cout du coordinateur : moyenne 0.022 / mediane 0.022 / p95 0.028 / max 3.163 ms (2651 pas) ; pas hote moyenne 1.427 / mediane 1.311 / p95 1.968 / max 17.028 ms (2651 pas)
- Contacts entre vehicules 0, obstacles 0, autres 0
- Couverture : aucun TrackingToleranceExceeded
- Invariants : verts
- Trace brute : `scenario-F-20261007-143354-steps.tsv` ; journal de coordination : `scenario-F-20261007-143354-junction.txt`
