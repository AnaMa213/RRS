# Diagnostic rond-point contre ligne droite (Story 5.33, apres D14)

Banc `Story533RoundaboutCostDiagnosticTests` (EditMode, geometrie et decor reels de MVP_Run). 11 routes de reference 5.31 ; emplacements au metre : 268 en ligne droite (tiers le moins courbe des emplacements hors carrefour a 3 m pres : |kappa| max sur +-6 m <= 0.007 /m, R >= 150 m), 1084 dans un rond-point (dans la frontiere d'un carrefour Roundabout). Chaque configuration place N vehicules dans la meme situation, au plus pres d'une ancre (7 m au moins entre centres), pose nominale cinematique, vitesse min(v desiree, sqrt(b_confort / |kappa|)), plan de route acquis comme en roulant. Pas hote rejoue dans l'ordre du runtime ; temps = meilleur de 3 executions apres chauffe ; octets = GC.GetTotalMemory sans collection ; compteurs = TrafficV2WorkCounters par section. N = 1 : 24 ancres ; N > 1 : 8 ancres.

Carrefours du modele : Roundabout 4 (demi-etendues moyennes 11.669 x 11.669 m), TJunction 4 (demi-etendues moyennes 8.5 x 8.5 m), Crossroads 1 (demi-etendues moyennes 10 x 10 m)

## 1. Cout par pas hote (ms, moyenne sur les ancres)

| N | situation | config. | pas total | par vehicule | preparation | collecte | dont Overlap | frame | dont localisation (a part) | occupation (frame - localisation) | spine | route (a part) | horizon (a part) | dont transport de e | RoutePath (a part) | MotionPlan (a part) | perception | SpeedPlan | dont verification (a part) | arbitrage+commande | octets alloues |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | ligne droite | 24 | 0.672 | 0.672 | 0.007 | 0.01 | 0.005 | 0.11 | 0.016 | 0.094 | 0.123 | 0.002 | 0.078 | 0.027 | 0.014 | 0.009 | 0.124 | 0.283 | 0.243 | 0.009 | 51712 |
| 1 | rond-point | 24 | 0.84 | 0.84 | 0.007 | 0.011 | 0.004 | 0.292 | 0.051 | 0.241 | 0.112 | 0.002 | 0.07 | 0.024 | 0.012 | 0.009 | 0.146 | 0.253 | 0.221 | 0.01 | 52053.333 |
| 2 | ligne droite | 8 | 1.282 | 0.641 | 0.014 | 0.017 | 0.011 | 0.219 | 0.032 | 0.187 | 0.254 | 0.004 | 0.127 | 0.044 | 0.019 | 0.016 | 0.226 | 0.524 | 0.435 | 0.02 | 88576 |
| 2 | rond-point | 8 | 1.816 | 0.908 | 0.015 | 0.018 | 0.009 | 0.61 | 0.103 | 0.507 | 0.289 | 0.005 | 0.139 | 0.048 | 0.023 | 0.017 | 0.31 | 0.526 | 0.433 | 0.025 | 94208 |
| 4 | ligne droite | 8 | 3.092 | 0.773 | 0.029 | 0.049 | 0.026 | 0.454 | 0.063 | 0.391 | 0.59 | 0.008 | 0.359 | 0.064 | 0.05 | 0.035 | 0.669 | 1.196 | 0.974 | 0.045 | 218624 |
| 4 | rond-point | 8 | 3.752 | 0.938 | 0.029 | 0.04 | 0.02 | 1.185 | 0.208 | 0.976 | 0.473 | 0.009 | 0.308 | 0.099 | 0.062 | 0.034 | 0.905 | 1.037 | 0.898 | 0.054 | 207360 |
| 8 | ligne droite | 8 | 6.52 | 0.815 | 0.062 | 0.111 | 0.055 | 0.871 | 0.128 | 0.743 | 1.011 | 0.017 | 0.536 | 0.196 | 0.082 | 0.068 | 1.901 | 2.164 | 1.875 | 0.088 | 326144 |
| 8 | rond-point | 8 | 9.037 | 1.13 | 0.067 | 0.089 | 0.043 | 2.153 | 0.38 | 1.773 | 1.062 | 0.017 | 0.616 | 0.213 | 0.099 | 0.075 | 2.672 | 2.289 | 1.959 | 0.196 | 399872 |

## 2. Travail par vehicule et par pas (compteurs du runtime, moyenne sur les ancres)

| Grandeur | LD N=1 | RP N=1 | RP/LD | LD N=2 | RP N=2 | RP/LD | LD N=4 | RP N=4 | RP/LD | LD N=8 | RP N=8 | RP/LD |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| points construits (PathHorizon) | 285.167 | 266 | 0.933 | 252.188 | 262.188 | 1.04 | 280.344 | 271.844 | 0.97 | 272.203 | 295.188 | 1.084 |
| intervalles de l'horizon | 2.958 | 3.208 | 1.085 | 2.688 | 3.125 | 1.163 | 3.063 | 3.188 | 1.041 | 3.031 | 3.281 | 1.082 |
| etendues du chemin de perception (RoutePath) | 15.167 | 16.292 | 1.074 | 12.625 | 15 | 1.188 | 13.063 | 16.281 | 1.246 | 13.344 | 16.219 | 1.215 |
| constructions d'horizon | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 |
| recherches de route | 0 | 0 | - | 0 | 0 | - | 0 | 0 | - | 0 | 0 | - |
| RoadCurve.Project : appels | 13.417 | 26.208 | 1.953 | 13.438 | 31.563 | 2.349 | 37.219 | 59.031 | 1.586 | 67.281 | 101.031 | 1.502 |
| RoadCurve.Project : segments parcourus | 1559.875 | 3036.125 | 1.946 | 1558.688 | 3860.75 | 2.477 | 4372.469 | 7178.625 | 1.642 | 7924.797 | 12164.047 | 1.535 |
| RoadCurve.Project : segments evalues | 24.292 | 73.25 | 3.015 | 17.375 | 82.188 | 4.73 | 79.719 | 167.531 | 2.102 | 155.688 | 312.219 | 2.005 |
| ProjectNearest (occupation) : appels | 132 | 132 | 1 | 132 | 132 | 1 | 132 | 132 | 1 | 132 | 132 | 1 |
| ProjectNearest : segments evalues | 135.375 | 666.875 | 4.926 | 135.25 | 694.938 | 5.138 | 135 | 673.563 | 4.989 | 134.75 | 585.5 | 4.345 |
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
| 1 | ligne droite | 3 | 0 | 2.042 | 0 | 15.25 | 0 | 4 |
| 1 | rond-point | 2.958 | 0 | 32.167 | 0 | 34.125 | 0 | 4 |
| 2 | ligne droite | 3 | 0 | 2.063 | 0 | 8.313 | 0 | 4 |
| 2 | rond-point | 2.938 | 0 | 30.938 | 0 | 44.375 | 0 | 3.938 |
| 4 | ligne droite | 3 | 0 | 2.031 | 0 | 70.688 | 0 | 4 |
| 4 | rond-point | 2.906 | 0 | 32.5 | 0 | 128.219 | 0 | 3.906 |
| 8 | ligne droite | 3 | 0 | 2.016 | 0 | 146.672 | 0 | 4 |
| 8 | rond-point | 2.906 | 0 | 28.375 | 0 | 277.031 | 0 | 3.906 |

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
| pas total | ligne droite | 0.672 | 0.641 | 0.773 | 0.815 | 1.213 |
| pas total | rond-point | 0.84 | 0.908 | 0.938 | 1.13 | 1.345 |
| collecte | ligne droite | 0.01 | 0.009 | 0.012 | 0.014 | 1.393 |
| collecte | rond-point | 0.011 | 0.009 | 0.01 | 0.011 | 1.041 |
| frame | ligne droite | 0.11 | 0.11 | 0.113 | 0.109 | 0.99 |
| frame | rond-point | 0.292 | 0.305 | 0.296 | 0.269 | 0.923 |
| spine | ligne droite | 0.123 | 0.127 | 0.148 | 0.126 | 1.028 |
| spine | rond-point | 0.112 | 0.144 | 0.118 | 0.133 | 1.185 |
| perception | ligne droite | 0.124 | 0.113 | 0.167 | 0.238 | 1.915 |
| perception | rond-point | 0.146 | 0.155 | 0.226 | 0.334 | 2.289 |
| SpeedPlan | ligne droite | 0.283 | 0.262 | 0.299 | 0.271 | 0.957 |
| SpeedPlan | rond-point | 0.253 | 0.263 | 0.259 | 0.286 | 1.132 |
| arbitrage+commande | ligne droite | 0.009 | 0.01 | 0.011 | 0.011 | 1.266 |
| arbitrage+commande | rond-point | 0.01 | 0.013 | 0.013 | 0.024 | 2.484 |
