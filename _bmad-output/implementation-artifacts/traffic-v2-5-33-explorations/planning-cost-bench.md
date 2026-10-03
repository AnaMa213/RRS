# Banc de cout du planning Traffic V2 (Story 5.33, D13 bis)

Etats : 1335 poses nominales le long des 11 routes de `campaign-5-31.json` (pas 3 m et milieu de chaque morceau), e de la reference, v = 4 m/s pour les temps. Temps : meilleur de 3 executions apres chauffe (ms) ; allocations : GC.GetTotalMemory sans collection (octets). Editeur, EditMode.

## Horizon actuel (portee 100000 m : toute la route restante)

| Mesure | moyenne | p95 | max |
|---|---|---|---|
| longueur de l'horizon (m) | 145.354 | 297.397 | 369.111 |
| points de l'horizon | 1766.045 | 3553 | 4385 |
| noeuds du plan de vitesse | 363.973 | 746 | 927 |
| PathHorizon.Build runtime (ms) | 1.501 | 2.991 | 7.698 |
|   echantillonnage curve.Sample (ms) | 0.425 | 0.85 | 3.136 |
|   construction des points (ms) | 0.211 | 0.376 | 4.097 |
|   transport de e + pose nominale (ms) | 0.449 | 0.883 | 4.894 |
|   pentes et raccords (ms) | 0.129 | 0.261 | 0.369 |
|   allocations (octets) | 628783.557 | 1273856 | 1605632 |

## Plan de vitesse actuel

| Mesure | moyenne | p95 | max |
|---|---|---|---|
| SpeedPlan.Build runtime (ms) | 2.023 | 4.165 | 7.406 |
|   aplatissement des points (ms) | 0.129 | 0.221 | 4.464 |
|   noeuds et voisinages (ms) | 0.06 | 0.121 | 1.149 |
|   passes (atteignabilite, plafonds, arriere, avant) (ms) | 0.025 | 0.033 | 3.153 |
|   points et profil (ms) | 0.004 | 0.003 | 3.311 |
|   verification du profil (ms) | 1.937 | 4.025 | 7.005 |
|   allocations (octets) | 233996.656 | 430080 | 655360 |

## Occupation de la frame (perimetre ~130 points, projection sur tout l'element)

| Mesure | moyenne | p95 | max |
|---|---|---|---|
| occupation actuelle (ms) | 1.233 | 2.67 | 2.874 |
| occupation, projection elaguee exacte (ms) | 0.702 | 1.919 | 2.211 |
|   allocations actuelles (octets) | 27.613 | 0 | 20480 |

Projections comparees : 176220, differences au bit pres : 0.

## Prototypes exacts (resultats compares au bit pres au runtime)

| Mesure | moyenne | p95 | max |
|---|---|---|---|
| horizon exact optimise (ms) | 1.076 | 2.16 | 3.687 |
|   allocations (octets) | 173302.22 | 348160 | 438272 |
| plan de vitesse exact optimise (ms) | 1.651 | 3.303 | 4.867 |
|   allocations (octets) | 105440.551 | 196608 | 229376 |

Differences au bit pres : horizon 0 points, plan de vitesse 0 etats sur 1335.

## Portee bornee H = d1 + v_ref^2 / (2 b_plan) + 1 m

| Mesure | moyenne | p95 | max |
|---|---|---|---|
| longueur bornee (m) | 18.376 | 24.343 | 26.662 |
| points de l'horizon borne | 236.326 | 474 | 529 |
| horizon runtime borne (ms) | 0.208 | 0.42 | 1.683 |
| plan runtime borne (ms) | 0.255 | 0.516 | 1.067 |
| horizon exact optimise borne (ms) | 0.146 | 0.295 | 0.432 |
| plan exact optimise borne (ms) | 0.221 | 0.449 | 0.509 |
| allocations horizon + plan runtime bornes (octets) | 101902.957 | 192512 | 286720 |

Commandes comparees (6 vitesses par etat) : 7838, differences : 0, plans bornes refuses alors que le plan complet est accepte : 0.

## Comparaison horizon + plan de vitesse par vehicule et par pas (ms, moyenne)

| Variante | horizon | plan | total | gain |
|---|---|---|---|---|
| actuel | 1.501 | 2.023 | 3.524 | 1x |
| exact optimise | 1.076 | 1.651 | 2.727 | 1.292x |
| exact optimise + portee bornee | 0.146 | 0.221 | 0.368 | 9.582x |

Occupation : 1.233 -> 0.702 ms par vehicule (1.757x).
