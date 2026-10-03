# Diagnostic rond-point contre ligne droite (Story 5.33, apres D14)

Banc `Story533RoundaboutCostDiagnosticTests` (EditMode, geometrie et decor reels de MVP_Run). 11 routes de reference 5.31 ; emplacements au metre : 268 en ligne droite (tiers le moins courbe des emplacements hors carrefour a 3 m pres : |kappa| max sur +-6 m <= 0.007 /m, R >= 150 m), 1084 dans un rond-point (dans la frontiere d'un carrefour Roundabout). Chaque configuration place N vehicules dans la meme situation, au plus pres d'une ancre (7 m au moins entre centres), pose nominale cinematique, vitesse min(v desiree, sqrt(b_confort / |kappa|)), plan de route acquis comme en roulant. Pas hote rejoue dans l'ordre du runtime ; temps = meilleur de 3 executions apres chauffe ; octets = GC.GetTotalMemory sans collection ; compteurs = TrafficV2WorkCounters par section. N = 1 : 24 ancres ; N > 1 : 8 ancres.

Carrefours du modele : Roundabout 4 (demi-etendues moyennes 11.669 x 11.669 m), TJunction 4 (demi-etendues moyennes 8.5 x 8.5 m), Crossroads 1 (demi-etendues moyennes 10 x 10 m)

## 1. Cout par pas hote (ms, moyenne sur les ancres)

| N | situation | config. | pas total | par vehicule | preparation | collecte | dont Overlap | frame | dont localisation (a part) | occupation (frame - localisation) | spine | route (a part) | horizon (a part) | dont transport de e | RoutePath (a part) | MotionPlan (a part) | perception | SpeedPlan | dont verification (a part) | arbitrage+commande | octets alloues |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | ligne droite | 24 | 0.48 | 0.48 | 0.007 | 0.011 | 0.005 | 0.077 | 0.008 | 0.069 | 0.113 | 0.002 | 0.075 | 0.026 | 0.012 | 0.009 | 0.115 | 0.138 | 0.103 | 0.009 | 51370.667 |
| 1 | rond-point | 24 | 0.679 | 0.679 | 0.007 | 0.01 | 0.004 | 0.265 | 0.044 | 0.221 | 0.118 | 0.002 | 0.073 | 0.026 | 0.013 | 0.008 | 0.123 | 0.138 | 0.099 | 0.011 | 48128 |
| 2 | ligne droite | 8 | 0.829 | 0.414 | 0.013 | 0.015 | 0.009 | 0.15 | 0.015 | 0.134 | 0.196 | 0.004 | 0.126 | 0.044 | 0.02 | 0.016 | 0.192 | 0.243 | 0.184 | 0.017 | 97280 |
| 2 | rond-point | 8 | 1.356 | 0.678 | 0.014 | 0.018 | 0.009 | 0.549 | 0.089 | 0.46 | 0.231 | 0.004 | 0.15 | 0.035 | 0.027 | 0.016 | 0.241 | 0.26 | 0.195 | 0.024 | 119296 |
| 4 | ligne droite | 8 | 2.132 | 0.533 | 0.028 | 0.048 | 0.024 | 0.322 | 0.03 | 0.292 | 0.534 | 0.008 | 0.28 | 0.1 | 0.04 | 0.035 | 0.439 | 0.584 | 0.412 | 0.042 | 206848 |
| 4 | rond-point | 8 | 2.775 | 0.694 | 0.027 | 0.038 | 0.019 | 1.074 | 0.177 | 0.898 | 0.467 | 0.008 | 0.289 | 0.102 | 0.049 | 0.034 | 0.575 | 0.524 | 0.394 | 0.052 | 230400 |
| 8 | ligne droite | 8 | 3.786 | 0.473 | 0.059 | 0.103 | 0.049 | 0.61 | 0.063 | 0.547 | 0.874 | 0.016 | 0.617 | 0.176 | 0.104 | 0.068 | 0.936 | 1.074 | 0.817 | 0.086 | 415232 |
| 8 | rond-point | 8 | 5.682 | 0.71 | 0.057 | 0.074 | 0.036 | 1.912 | 0.318 | 1.594 | 0.984 | 0.017 | 0.65 | 0.211 | 0.102 | 0.073 | 1.263 | 1.132 | 0.86 | 0.18 | 492032 |

## 2. Travail par vehicule et par pas (compteurs du runtime, moyenne sur les ancres)

| Grandeur | LD N=1 | RP N=1 | RP/LD | LD N=2 | RP N=2 | RP/LD | LD N=4 | RP N=4 | RP/LD | LD N=8 | RP N=8 | RP/LD |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| points construits (PathHorizon) | 285.167 | 266 | 0.933 | 252.188 | 262.188 | 1.04 | 280.344 | 271.844 | 0.97 | 272.203 | 295.188 | 1.084 |
| intervalles de l'horizon | 2.958 | 3.208 | 1.085 | 2.688 | 3.125 | 1.163 | 3.063 | 3.188 | 1.041 | 3.031 | 3.281 | 1.082 |
| etendues du chemin de perception (RoutePath) | 15.167 | 16.292 | 1.074 | 12.625 | 15 | 1.188 | 13.063 | 16.281 | 1.246 | 13.344 | 16.219 | 1.215 |
| constructions d'horizon | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 |
| recherches de route | 0 | 0 | - | 0 | 0 | - | 0 | 0 | - | 0 | 0 | - |
| RoadCurve.Project : appels | 9.083 | 14.75 | 1.624 | 9.125 | 16.5 | 1.808 | 9.75 | 18.781 | 1.926 | 10.5 | 21 | 2 |
| RoadCurve.Project : segments parcourus | 1033.875 | 1660.5 | 1.606 | 1049.875 | 2055.688 | 1.958 | 1146.344 | 2320.281 | 2.024 | 1234.641 | 2468.25 | 1.999 |
| RoadCurve.Project : segments evalues | 9.125 | 39.125 | 4.288 | 9.188 | 48.5 | 5.279 | 9.781 | 61.969 | 6.335 | 11.016 | 67.547 | 6.132 |
| ProjectNearest (occupation) : appels | 132 | 132 | 1 | 132 | 132 | 1 | 132 | 132 | 1 | 132 | 132 | 1 |
| ProjectNearest : segments evalues | 135.375 | 666.875 | 4.926 | 135.25 | 694.938 | 5.138 | 135 | 673.563 | 4.989 | 134.75 | 585.5 | 4.345 |
| localisation : elements examines | 8.292 | 12.167 | 1.467 | 7.813 | 11.875 | 1.52 | 8.281 | 12.469 | 1.506 | 8.469 | 11.75 | 1.387 |
| localisation : elements projetes (boite) | 2 | 7.917 | 3.958 | 2 | 7.75 | 3.875 | 2 | 7.594 | 3.797 | 2 | 7.641 | 3.82 |
| localisation : candidats | 2 | 5.667 | 2.833 | 2 | 5.688 | 2.844 | 2 | 5.656 | 2.828 | 2 | 5.625 | 2.813 |
| transport de e : appels | 283.208 | 265.708 | 0.938 | 250.5 | 261.813 | 1.045 | 278.281 | 271.406 | 0.975 | 270.172 | 294.484 | 1.09 |
| transport de e : pas RK4 (x4 sinus) | 459.833 | 675.083 | 1.468 | 414.625 | 680.563 | 1.641 | 441.906 | 683.344 | 1.546 | 436.75 | 663.359 | 1.519 |
| perception : points lus pour la boite | 1807.125 | 1929.542 | 1.068 | 1508.188 | 1745.063 | 1.157 | 1531.344 | 1928.5 | 1.259 | 1554.609 | 1926.922 | 1.239 |
| perception : entrees spatiales examinees | 1.25 | 1.583 | 1.267 | 1.25 | 2.313 | 1.85 | 3.125 | 4.219 | 1.35 | 5.406 | 6.797 | 1.257 |
| perception : projections d'obstacle | 0.083 | 0 | 0 | 0.125 | 1.875 | 15 | 0.906 | 4.938 | 5.448 | 1.828 | 9 | 4.923 |
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
| 1 | ligne droite | 3 | 0 | 2 | 0 | 0.083 | 0 | 4 |
| 1 | rond-point | 2.917 | 0 | 7.917 | 0 | 0 | 0 | 3.917 |
| 2 | ligne droite | 3 | 0 | 2 | 0 | 0.125 | 0 | 4 |
| 2 | rond-point | 2.938 | 0 | 7.75 | 0 | 1.875 | 0 | 3.938 |
| 4 | ligne droite | 3 | 0 | 2 | 0 | 0.75 | 0 | 4 |
| 4 | rond-point | 2.906 | 0 | 7.594 | 0 | 4.375 | 0 | 3.906 |
| 8 | ligne droite | 3 | 0 | 2 | 0 | 1.5 | 0 | 4 |
| 8 | rond-point | 2.906 | 0 | 7.641 | 0 | 6.547 | 0 | 3.906 |

### ProjectSegmentsEvaluated

| N | situation | preparation | collecte | frame | spine | perception | SpeedPlan | arbitrage+commande |
|---|---|---|---|---|---|---|---|---|
| 1 | ligne droite | 3 | 0 | 2.042 | 0 | 0.083 | 0 | 4 |
| 1 | rond-point | 2.958 | 0 | 32.167 | 0 | 0 | 0 | 4 |
| 2 | ligne droite | 3 | 0 | 2.063 | 0 | 0.125 | 0 | 4 |
| 2 | rond-point | 2.938 | 0 | 30.938 | 0 | 10.688 | 0 | 3.938 |
| 4 | ligne droite | 3 | 0 | 2.031 | 0 | 0.75 | 0 | 4 |
| 4 | rond-point | 2.906 | 0 | 32.5 | 0 | 22.656 | 0 | 3.906 |
| 8 | ligne droite | 3 | 0 | 2.016 | 0 | 2 | 0 | 4 |
| 8 | rond-point | 2.906 | 0 | 28.375 | 0 | 32.359 | 0 | 3.906 |

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
| 1 | ligne droite | 0 | 0 | 135.375 | 0 | 0 | 0 | 0 |
| 1 | rond-point | 0 | 0 | 666.875 | 0 | 0 | 0 | 0 |
| 2 | ligne droite | 0 | 0 | 135.25 | 0 | 0 | 0 | 0 |
| 2 | rond-point | 0 | 0 | 694.938 | 0 | 0 | 0 | 0 |
| 4 | ligne droite | 0 | 0 | 135 | 0 | 0 | 0 | 0 |
| 4 | rond-point | 0 | 0 | 673.563 | 0 | 0 | 0 | 0 |
| 8 | ligne droite | 0 | 0 | 134.75 | 0 | 0 | 0 | 0 |
| 8 | rond-point | 0 | 0 | 585.5 | 0 | 0 | 0 | 0 |

## 4. Croissance N = 1 -> 8 (cout par vehicule, ms ; un cout O(N) reste plat, un cout O(N^2) croit avec N)

| Section | situation | N=1 | N=2 | N=4 | N=8 | N=8 / N=1 |
|---|---|---|---|---|---|---|
| pas total | ligne droite | 0.48 | 0.414 | 0.533 | 0.473 | 0.987 |
| pas total | rond-point | 0.679 | 0.678 | 0.694 | 0.71 | 1.045 |
| collecte | ligne droite | 0.011 | 0.008 | 0.012 | 0.013 | 1.226 |
| collecte | rond-point | 0.01 | 0.009 | 0.009 | 0.009 | 0.899 |
| frame | ligne droite | 0.077 | 0.075 | 0.08 | 0.076 | 0.988 |
| frame | rond-point | 0.265 | 0.275 | 0.269 | 0.239 | 0.901 |
| spine | ligne droite | 0.113 | 0.098 | 0.133 | 0.109 | 0.968 |
| spine | rond-point | 0.118 | 0.115 | 0.117 | 0.123 | 1.045 |
| perception | ligne droite | 0.115 | 0.096 | 0.11 | 0.117 | 1.016 |
| perception | rond-point | 0.123 | 0.121 | 0.144 | 0.158 | 1.286 |
| SpeedPlan | ligne droite | 0.138 | 0.121 | 0.146 | 0.134 | 0.971 |
| SpeedPlan | rond-point | 0.138 | 0.13 | 0.131 | 0.142 | 1.027 |
| arbitrage+commande | ligne droite | 0.009 | 0.008 | 0.011 | 0.011 | 1.218 |
| arbitrage+commande | rond-point | 0.011 | 0.012 | 0.013 | 0.022 | 2.106 |
