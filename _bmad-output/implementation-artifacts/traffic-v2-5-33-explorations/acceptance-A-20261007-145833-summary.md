# acceptance-A 20261007-145833

- Scenario : A, population maximale 3, fixedDeltaTime 0.02 s
- Pas hote : 2684, frames construites 2682, refus de frame 0, population maximale observee 3
- Insertions : #0 00000000000000000000000000000001 au plus tot 0, reelle 1 ; #1 00000000000000000000000000000002 au plus tot 0, reelle 284 ; #2 00000000000000000000000000000003 au plus tot 0, reelle 571
- Retraits au portail : 3/3
- Interaction : obstacle 00000000000000000000000000000001 jeu 2.459 ok dominant Obstacle:48415a41524400000000000000000001 ; leader 00000000000000000000000000000002 jeu 2.487 ok dominant Leader:00000000000000000000000000000001 ; entree 00000000000000000000000000000003 distance 0.348 ok (O11) dominant BlockedExit:47de8d1a8e71016a20b301b8a86852a5 ;  (obstacle retire au pas hote 938)
- Collecteur : 77 colliders au plus par requete (capacite 256), 0 pas-vehicule satures
- PerceptionUnavailable : aucun
- Contacts entre vehicules : 0, avec l'obstacle : 0, autres : 0
- d max au pas : 00000000000000000000000000000001 0.1995, 00000000000000000000000000000002 0.1992, 00000000000000000000000000000003 0.2
- Couverture : aucun TrackingToleranceExceeded
- Maintien a l'arret (D11) :
  - #0 00000000000000000000000000000001 : entree StopHold fr 703 (Obstacle @48415a41524400000000000000000001, jeu 2.49717 m, v 0.45256 m/s), v min pendant le maintien 0 m/s, depart de la cause fr 938, liberation fr 939 (SourceGone, jeu nan m), duree du maintien 236 pas (4.72 s), delai depart -> liberation 0.02 s, reprise v >= 0.5 m/s fr 973 (+0.68 s), entrees au total 1 ; rampement apres arret 0 m ; fin d'approche lente poussee 0 m en 0 pas (d max 0 m) ; d max 0.19953 m ; TrackingToleranceExceeded non
  - #1 00000000000000000000000000000002 : entree StopHold fr 885 (LeaderFollowing @00000000000000000000000000000001, jeu 2.49738 m, v 0.25515 m/s), v min pendant le maintien 0 m/s, depart de la cause fr 939, liberation fr 973 (SourceDeparted, jeu 2.62105 m), duree du maintien 88 pas (1.76 s), delai depart -> liberation 0.68 s, reprise v >= 0.5 m/s fr 1041 (+1.36 s), entrees au total 1 ; rampement apres arret 0 m ; fin d'approche lente poussee 0.09455 m en 19 pas (d max 0.10749 m) ; d max 0.19921 m ; TrackingToleranceExceeded non
  - #2 00000000000000000000000000000003 : entree StopHold fr 888 (JunctionEntry @452ee31e83feea5ebc05406c271424a9, jeu 0.34971 m, v 0.12086 m/s), v min pendant le maintien 0 m/s, depart de la cause -, liberation fr 1256 (GrantEffective, jeu 0.34785 m), duree du maintien 368 pas (7.36 s), reprise v >= 0.5 m/s fr 1289 (+0.66 s), entrees au total 1 ; rampement apres arret 0.02873 m ; fin d'approche lente poussee 0 m en 0 pas (d max 0 m) ; d max 0.19999 m ; TrackingToleranceExceeded non
- Cout par vehicule et par pas : frame 0.281 ms / perception 0.1 ms / spine 0.161 ms / plan+arbitrage 0.165 ms / composition 0.014 ms / total 0.721 ms
- Invariants : verts
- Trace brute : `acceptance-A-20261007-145833-steps.tsv`
