# acceptance-A 20261002-214649

- Scenario : A, population maximale 3, fixedDeltaTime 0.02 s
- Pas hote : 2627, frames construites 2625, refus de frame 0, population maximale observee 3
- Insertions : #0 00000000000000000000000000000001 au plus tot 0, reelle 1 ; #1 00000000000000000000000000000002 au plus tot 0, reelle 284 ; #2 00000000000000000000000000000003 au plus tot 0, reelle 571
- Retraits au portail : 3/3
- Interaction : obstacle 00000000000000000000000000000001 jeu 2.459 ok dominant Obstacle:48415a41524400000000000000000001 ; leader 00000000000000000000000000000002 jeu 2.487 ok dominant Leader:00000000000000000000000000000001 ; leader 00000000000000000000000000000003 jeu 2.457 ok dominant Leader:00000000000000000000000000000002 ;  (obstacle retire au pas hote 1072)
- Collecteur : 77 colliders au plus par requete (capacite 256), 0 pas-vehicule satures
- PerceptionUnavailable : aucun
- Contacts entre vehicules : 0, avec l'obstacle : 0, autres : 0
- d max au pas : 00000000000000000000000000000001 0.1995, 00000000000000000000000000000002 0.1937, 00000000000000000000000000000003 0.1874
- Couverture : aucun TrackingToleranceExceeded
- Maintien a l'arret (D11) :
  - #0 00000000000000000000000000000001 : entree StopHold fr 703 (Obstacle @48415a41524400000000000000000001, jeu 2.49717 m, v 0.45256 m/s), v min pendant le maintien 0 m/s, depart de la cause fr 1072, liberation fr 1073 (SourceGone, jeu nan m), duree du maintien 370 pas (7.4 s), delai depart -> liberation 0.02 s, reprise v >= 0.5 m/s fr 1107 (+0.68 s), entrees au total 1 ; rampement 0 m ; d max 0.19953 m ; TrackingToleranceExceeded non
  - #1 00000000000000000000000000000002 : entree StopHold fr 885 (LeaderFollowing @00000000000000000000000000000001, jeu 2.49738 m, v 0.25515 m/s), v min pendant le maintien 0 m/s, depart de la cause fr 1073, liberation fr 1107 (SourceDeparted, jeu 2.62105 m), duree du maintien 222 pas (4.44 s), delai depart -> liberation 0.68 s, reprise v >= 0.5 m/s fr 1175 (+1.36 s), entrees au total 1 ; rampement 0.09455 m ; d max 0.19373 m ; TrackingToleranceExceeded non
  - #2 00000000000000000000000000000003 : entree StopHold fr 1014 (LeaderFollowing @00000000000000000000000000000002, jeu 2.49915 m, v 0.47897 m/s), v min pendant le maintien 0 m/s, depart de la cause fr 1107, liberation fr 1175 (SourceDeparted, jeu 2.79744 m), duree du maintien 161 pas (3.22 s), delai depart -> liberation 1.36 s, reprise v >= 0.5 m/s fr 1230 (+1.1 s), entrees au total 1 ; rampement 0 m ; d max 0.18739 m ; TrackingToleranceExceeded non
- Cout par vehicule et par pas : frame 1.376 ms / perception 0.15 ms / spine 1 ms / plan+arbitrage 1.122 ms / composition 0.031 ms / total 3.68 ms
- Invariants : verts
- Trace brute : `acceptance-A-20261002-214649-steps.tsv`
