# Diagnostic rond-point contre ligne droite (Story 5.33, apres D14)

Banc `Story533RoundaboutCostDiagnosticTests` (EditMode, geometrie et decor reels de MVP_Run). 11 routes de reference 5.31 ; emplacements au metre : 268 en ligne droite (tiers le moins courbe des emplacements hors carrefour a 3 m pres : |kappa| max sur +-6 m <= 0.007 /m, R >= 150 m), 1084 dans un rond-point (dans la frontiere d'un carrefour Roundabout). Chaque configuration place N vehicules dans la meme situation, au plus pres d'une ancre (7 m au moins entre centres), pose nominale cinematique, vitesse min(v desiree, sqrt(b_confort / |kappa|)), plan de route acquis comme en roulant. Pas hote rejoue dans l'ordre du runtime ; temps = meilleur de 3 executions apres chauffe ; octets = GC.GetTotalMemory sans collection ; compteurs = TrafficV2WorkCounters par section. N = 1 : 24 ancres ; N > 1 : 8 ancres.

Carrefours du modele : Roundabout 4 (demi-etendues moyennes 11.669 x 11.669 m), TJunction 4 (demi-etendues moyennes 8.5 x 8.5 m), Crossroads 1 (demi-etendues moyennes 10 x 10 m)

## 1. Cout par pas hote (ms, moyenne sur les ancres)

| N | situation | config. | pas total | par vehicule | preparation | collecte | dont Overlap | frame | dont localisation (a part) | occupation (frame - localisation) | spine | route (a part) | horizon (a part) | dont transport de e | RoutePath (a part) | MotionPlan (a part) | perception | SpeedPlan | dont verification (a part) | arbitrage+commande | octets alloues |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | ligne droite | 24 | 0.753 | 0.753 | 0.041 | 0.01 | 0.005 | 0.108 | 0.015 | 0.093 | 0.111 | 0.002 | 0.073 | 0.025 | 0.012 | 0.009 | 0.156 | 0.278 | 0.245 | 0.043 | 60416 |
| 1 | rond-point | 24 | 1.421 | 1.421 | 0.026 | 0.011 | 0.005 | 0.73 | 0.111 | 0.619 | 0.114 | 0.002 | 0.071 | 0.025 | 0.012 | 0.008 | 0.233 | 0.257 | 0.228 | 0.033 | 56320 |
| 2 | ligne droite | 8 | 1.397 | 0.698 | 0.082 | 0.016 | 0.01 | 0.215 | 0.03 | 0.185 | 0.203 | 0.004 | 0.149 | 0.043 | 0.032 | 0.016 | 0.282 | 0.497 | 0.445 | 0.086 | 111104 |
| 2 | rond-point | 8 | 3.05 | 1.525 | 0.064 | 0.02 | 0.01 | 1.547 | 0.229 | 1.318 | 0.229 | 0.004 | 0.15 | 0.059 | 0.023 | 0.016 | 0.552 | 0.505 | 0.437 | 0.081 | 95744 |
| 4 | ligne droite | 8 | 3.9 | 0.975 | 0.165 | 0.047 | 0.024 | 0.426 | 0.06 | 0.367 | 0.45 | 0.008 | 0.278 | 0.097 | 0.04 | 0.035 | 1.487 | 1.108 | 0.968 | 0.178 | 192512 |
| 4 | rond-point | 8 | 7.113 | 1.778 | 0.124 | 0.041 | 0.021 | 2.94 | 0.462 | 2.478 | 0.493 | 0.009 | 0.289 | 0.102 | 0.049 | 0.034 | 2.242 | 1.056 | 0.906 | 0.166 | 219648 |
| 8 | ligne droite | 8 | 10.333 | 1.292 | 0.341 | 0.123 | 0.06 | 0.865 | 0.12 | 0.745 | 0.914 | 0.017 | 0.571 | 0.191 | 0.085 | 0.068 | 5.346 | 2.203 | 1.909 | 0.364 | 430592 |
| 8 | rond-point | 8 | 17.75 | 2.219 | 0.249 | 0.103 | 0.049 | 5.199 | 0.847 | 4.352 | 1.208 | 0.017 | 0.617 | 0.213 | 0.097 | 0.074 | 7.902 | 2.382 | 1.983 | 0.407 | 462336 |

## 2. Travail par vehicule et par pas (compteurs du runtime, moyenne sur les ancres)

| Grandeur | LD N=1 | RP N=1 | RP/LD | LD N=2 | RP N=2 | RP/LD | LD N=4 | RP N=4 | RP/LD | LD N=8 | RP N=8 | RP/LD |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| points construits (PathHorizon) | 285.167 | 266 | 0.933 | 252.188 | 262.188 | 1.04 | 280.344 | 271.844 | 0.97 | 272.203 | 295.188 | 1.084 |
| intervalles de l'horizon | 2.958 | 3.208 | 1.085 | 2.688 | 3.125 | 1.163 | 3.063 | 3.188 | 1.041 | 3.031 | 3.281 | 1.082 |
| etendues du chemin de perception (RoutePath) | 15.167 | 16.292 | 1.074 | 12.625 | 15 | 1.188 | 13.063 | 16.281 | 1.246 | 13.344 | 16.219 | 1.215 |
| constructions d'horizon | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 |
| recherches de route | 0 | 0 | - | 0 | 0 | - | 0 | 0 | - | 0 | 0 | - |
| RoadCurve.Project : appels | 13.417 | 26.208 | 1.953 | 13.438 | 31.563 | 2.349 | 37.219 | 59.031 | 1.586 | 67.281 | 101.031 | 1.502 |
| RoadCurve.Project : segments parcourus | 1560.292 | 3116.958 | 1.998 | 1559.063 | 3982.125 | 2.554 | 4373.656 | 7376.5 | 1.687 | 7926.984 | 12399.75 | 1.564 |
| RoadCurve.Project : segments evalues | 1559.875 | 3036.125 | 1.946 | 1558.688 | 3860.75 | 2.477 | 4372.469 | 7178.625 | 1.642 | 7924.797 | 12164.047 | 1.535 |
| ProjectNearest (occupation) : appels | 132 | 132 | 1 | 132 | 132 | 1 | 132 | 132 | 1 | 132 | 132 | 1 |
| ProjectNearest : segments evalues | 177.458 | 4861.417 | 27.395 | 167.063 | 5292.188 | 31.678 | 161.438 | 4778.938 | 29.602 | 161.719 | 4123.344 | 25.497 |
| localisation : elements examines | 116 | 116 | 1 | 116 | 116 | 1 | 116 | 116 | 1 | 116 | 116 | 1 |
| localisation : elements projetes (boite) | 2 | 7.917 | 3.958 | 2 | 7.75 | 3.875 | 2 | 7.594 | 3.797 | 2 | 7.641 | 3.82 |
| localisation : candidats | 2 | 5.667 | 2.833 | 2 | 5.688 | 2.844 | 2 | 5.656 | 2.828 | 2 | 5.625 | 2.813 |
| transport de e : appels | 283.208 | 265.708 | 0.938 | 250.5 | 261.813 | 1.045 | 278.281 | 271.406 | 0.975 | 270.172 | 294.484 | 1.09 |
| transport de e : pas RK4 (x4 sinus) | 459.833 | 675.083 | 1.468 | 414.625 | 680.563 | 1.641 | 441.906 | 683.344 | 1.546 | 436.75 | 663.359 | 1.519 |
| perception : points lus pour la boite | 1807.125 | 1929.542 | 1.068 | 1508.188 | 1745.063 | 1.157 | 1531.344 | 1928.5 | 1.259 | 1554.609 | 1926.922 | 1.239 |
| perception : entrees spatiales examinees | 1.25 | 1.583 | 1.267 | 1.25 | 2.313 | 1.85 | 3.125 | 4.219 | 1.35 | 5.406 | 6.797 | 1.257 |
| perception : projections d'obstacle | 4.417 | 11.458 | 2.594 | 4.438 | 16.938 | 3.817 | 28.219 | 44.625 | 1.581 | 58.281 | 86.578 | 1.486 |
| perception : paires d'intention | 0 | 0 | - | 0 | 0 | - | 0 | 0 | - | 0 | 0 | - |
| perception : ConflictZone examinees | 0 | 0 | - | 0 | 0 | - | 0 | 0 | - | 0 | 0 | - |
| perception : plages de zone | 0 | 0 | - | 0 | 0 | - | 0 | 0 | - | 0 | 0 | - |
| verification : noeuds | 339.208 | 320.75 | 0.946 | 304.063 | 312.688 | 1.028 | 334.938 | 321.75 | 0.961 | 327.109 | 350.578 | 1.072 |
| colliders rendus par requete | 70.5 | 47.667 | 0.676 | 70.313 | 45.063 | 0.641 | 69.875 | 45.25 | 0.648 | 74.328 | 48.766 | 0.656 |
| dont decor statique ecarte | 69.958 | 46.75 | 0.668 | 70.313 | 44.5 | 0.633 | 69.375 | 44.688 | 0.644 | 73.484 | 48.141 | 0.655 |
| dangers emis (par pas) | 0.542 | 0.917 | 1.692 | 0 | 0.625 | - | 2 | 0.875 | 0.438 | 2.625 | 1.375 | 0.524 |
| echantillons par metre de l'element occupe | 0.206 | 17.551 | 85.073 | 0.205 | 20.937 | 102.201 | 0.21 | 18.133 | 86.232 | 0.207 | 15.225 | 73.537 |

## 3. Attribution par section (par vehicule et par pas)

### ProjectCalls

| N | situation | preparation | collecte | frame | spine | perception | SpeedPlan | arbitrage+commande |
|---|---|---|---|---|---|---|---|---|
| 1 | ligne droite | 3 | 0 | 2 | 0 | 4.417 | 0 | 4 |
| 1 | rond-point | 2.917 | 0 | 7.917 | 0 | 11.458 | 0 | 3.917 |
| 2 | ligne droite | 3 | 0 | 2 | 0 | 4.438 | 0 | 4 |
| 2 | rond-point | 2.938 | 0 | 7.75 | 0 | 16.938 | 0 | 3.938 |
| 4 | ligne droite | 3 | 0 | 2 | 0 | 28.219 | 0 | 4 |
| 4 | rond-point | 2.906 | 0 | 7.594 | 0 | 44.625 | 0 | 3.906 |
| 8 | ligne droite | 3 | 0 | 2 | 0 | 58.281 | 0 | 4 |
| 8 | rond-point | 2.906 | 0 | 7.641 | 0 | 86.578 | 0 | 3.906 |

### ProjectSegmentsEvaluated

| N | situation | preparation | collecte | frame | spine | perception | SpeedPlan | arbitrage+commande |
|---|---|---|---|---|---|---|---|---|
| 1 | ligne droite | 514 | 0 | 4 | 0 | 526.083 | 0 | 515.792 |
| 1 | rond-point | 300.917 | 0 | 990.583 | 0 | 1375.625 | 0 | 369 |
| 2 | ligne droite | 514 | 0 | 4 | 0 | 524.938 | 0 | 515.75 |
| 2 | rond-point | 388.813 | 0 | 1023.125 | 0 | 1990.813 | 0 | 458 |
| 4 | ligne droite | 514 | 0 | 4 | 0 | 3338.75 | 0 | 515.719 |
| 4 | rond-point | 374.344 | 0 | 1051.313 | 0 | 5317.375 | 0 | 435.594 |
| 8 | ligne droite | 514 | 0 | 4 | 0 | 6891.156 | 0 | 515.641 |
| 8 | rond-point | 365.891 | 0 | 966.813 | 0 | 10408.813 | 0 | 422.531 |

### KinematicSteps

| N | situation | preparation | collecte | frame | spine | perception | SpeedPlan | arbitrage+commande |
|---|---|---|---|---|---|---|---|---|
| 1 | ligne droite | 0 | 0 | 75.292 | 384.542 | 0 | 0 | 0 |
| 1 | rond-point | 0 | 0 | 301.833 | 373.25 | 0 | 0 | 0 |
| 2 | ligne droite | 0 | 0 | 72.625 | 342 | 0 | 0 | 0 |
| 2 | rond-point | 0 | 0 | 310.313 | 370.25 | 0 | 0 | 0 |
| 4 | ligne droite | 0 | 0 | 72.438 | 369.469 | 0 | 0 | 0 |
| 4 | rond-point | 0 | 0 | 307.281 | 376.063 | 0 | 0 | 0 |
| 8 | ligne droite | 0 | 0 | 76.313 | 360.438 | 0 | 0 | 0 |
| 8 | rond-point | 0 | 0 | 265.594 | 397.766 | 0 | 0 | 0 |

### NearestSegmentsEvaluated

| N | situation | preparation | collecte | frame | spine | perception | SpeedPlan | arbitrage+commande |
|---|---|---|---|---|---|---|---|---|
| 1 | ligne droite | 0 | 0 | 177.458 | 0 | 0 | 0 | 0 |
| 1 | rond-point | 0 | 0 | 4861.417 | 0 | 0 | 0 | 0 |
| 2 | ligne droite | 0 | 0 | 167.063 | 0 | 0 | 0 | 0 |
| 2 | rond-point | 0 | 0 | 5292.188 | 0 | 0 | 0 | 0 |
| 4 | ligne droite | 0 | 0 | 161.438 | 0 | 0 | 0 | 0 |
| 4 | rond-point | 0 | 0 | 4778.938 | 0 | 0 | 0 | 0 |
| 8 | ligne droite | 0 | 0 | 161.719 | 0 | 0 | 0 | 0 |
| 8 | rond-point | 0 | 0 | 4123.344 | 0 | 0 | 0 | 0 |

## 4. Croissance N = 1 -> 8 (cout par vehicule, ms ; un cout O(N) reste plat, un cout O(N^2) croit avec N)

| Section | situation | N=1 | N=2 | N=4 | N=8 | N=8 / N=1 |
|---|---|---|---|---|---|---|
| pas total | ligne droite | 0.753 | 0.698 | 0.975 | 1.292 | 1.716 |
| pas total | rond-point | 1.421 | 1.525 | 1.778 | 2.219 | 1.562 |
| collecte | ligne droite | 0.01 | 0.008 | 0.012 | 0.015 | 1.513 |
| collecte | rond-point | 0.011 | 0.01 | 0.01 | 0.013 | 1.128 |
| frame | ligne droite | 0.108 | 0.107 | 0.107 | 0.108 | 1.005 |
| frame | rond-point | 0.73 | 0.774 | 0.735 | 0.65 | 0.89 |
| spine | ligne droite | 0.111 | 0.102 | 0.113 | 0.114 | 1.028 |
| spine | rond-point | 0.114 | 0.114 | 0.123 | 0.151 | 1.321 |
| perception | ligne droite | 0.156 | 0.141 | 0.372 | 0.668 | 4.291 |
| perception | rond-point | 0.233 | 0.276 | 0.561 | 0.988 | 4.236 |
| SpeedPlan | ligne droite | 0.278 | 0.249 | 0.277 | 0.275 | 0.992 |
| SpeedPlan | rond-point | 0.257 | 0.252 | 0.264 | 0.298 | 1.16 |
| arbitrage+commande | ligne droite | 0.043 | 0.043 | 0.045 | 0.045 | 1.062 |
| arbitrage+commande | rond-point | 0.033 | 0.041 | 0.042 | 0.051 | 1.556 |
