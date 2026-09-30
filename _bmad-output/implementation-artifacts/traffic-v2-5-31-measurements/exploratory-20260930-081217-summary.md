# Story 5.31 - campagne Exploratory exploratory-20260930-081217

Campagne exploratoire : ne vaut pas acceptation.

## Conditions

- pas physique : 0.02 s ; Editeur en hote ; profils par defaut du prefab V2
- commit : 742c412d12f9d5f5f59f9f308af96488f28eb514 (refs/heads/systeme-traffic-ia-v2)
- RoadModelVersion : v4:e8dff9e54bff1712308899158ad16a9b ; budget de graines : 1 ; statut de campagne : Accepted
- epsilon_t : non declare
- modele M : tolerances 0.002 m / 0.05 deg ; reste de Lipschitz vise 0.001 m

- preuve relief 5.51 : empreinte physique 2e1677f8cf104ccba47016a51651a93d7974d500e0b3e3cbaf0c46b4eff530a7 ; chaussées identifiées 29 ; fenetre contact ±50 pas = 1 s

## Resultat

11 vehicule(s), campagne complete True, cale False, pas totaux 53347, d max au pas 0.8806 m, borne entre deux pas max (M verifie) 0.9086 m, intervalles 53319 dont ModelNotVerified 0, pas v > v* 0, couple negatif en repli 0, contacts 10, elements Measured 161 / NotMeasured 3 / NotSelectable 0, NominalPoseInfeasible 0 pas (pose nominale cinematique, contrat §8), dernier code aucun

## Runs

| run | fin | pas | replanifications | replis | d max au pas (m) | temps par etape |
|---|---|---|---|---|---|---|
| 0 | sortie | 6427 | 0 | 31 | 0.435436 | steps 6427 / frame+localisation mean 0.125 ms max 0.412 / route+horizon+mouvement mean 1.532 ms max 13.586 / plan de vitesse mean 2.568 ms max 11.975 / commande+composition mean 0.044 ms max 2.002 |
| 1 | sortie | 5704 | 1 | 1 | 0.616071 | steps 5704 / frame+localisation mean 0.121 ms max 3.392 / route+horizon+mouvement mean 1.404 ms max 12.952 / plan de vitesse mean 2.302 ms max 11.027 / commande+composition mean 0.043 ms max 0.145 |
| 2 | sortie | 4816 | 2 | 21 | 0.821917 | steps 4816 / frame+localisation mean 0.115 ms max 3.218 / route+horizon+mouvement mean 1.128 ms max 24.566 / plan de vitesse mean 1.794 ms max 10.475 / commande+composition mean 0.041 ms max 0.137 |
| 3 | sortie | 4514 | 3 | 35 | 0.821917 | steps 4514 / frame+localisation mean 0.117 ms max 0.304 / route+horizon+mouvement mean 1.06 ms max 22.343 / plan de vitesse mean 1.725 ms max 9.777 / commande+composition mean 0.041 ms max 0.099 |
| 4 | sortie | 3087 | 2 | 20 | 0.620345 | steps 3087 / frame+localisation mean 0.116 ms max 0.357 / route+horizon+mouvement mean 1.056 ms max 13.241 / plan de vitesse mean 1.732 ms max 11.031 / commande+composition mean 0.04 ms max 0.124 |
| 5 | sortie | 3410 | 2 | 21 | 0.822514 | steps 3410 / frame+localisation mean 0.118 ms max 0.358 / route+horizon+mouvement mean 0.767 ms max 18.025 / plan de vitesse mean 1.2 ms max 7.527 / commande+composition mean 0.039 ms max 0.137 |
| 6 | sortie | 5096 | 0 | 18 | 0.392516 | steps 5096 / frame+localisation mean 0.123 ms max 0.346 / route+horizon+mouvement mean 1.22 ms max 12.799 / plan de vitesse mean 2.021 ms max 10.641 / commande+composition mean 0.042 ms max 0.098 |
| 7 | sortie | 4524 | 3 | 36 | 0.822514 | steps 4524 / frame+localisation mean 0.117 ms max 0.323 / route+horizon+mouvement mean 1.075 ms max 22.437 / plan de vitesse mean 1.72 ms max 10.339 / commande+composition mean 0.04 ms max 0.109 |
| 8 | sortie | 5552 | 1 | 16 | 0.60304 | steps 5552 / frame+localisation mean 0.118 ms max 3.225 / route+horizon+mouvement mean 1.308 ms max 12.919 / plan de vitesse mean 2.165 ms max 11.957 / commande+composition mean 0.042 ms max 0.094 |
| 9 | sortie | 4529 | 3 | 37 | 0.880598 | steps 4529 / frame+localisation mean 0.118 ms max 0.517 / route+horizon+mouvement mean 1.068 ms max 17.855 / plan de vitesse mean 1.739 ms max 9.369 / commande+composition mean 0.041 ms max 0.077 |
| 10 | sortie | 5671 | 4 | 63 | 0.852712 | steps 5671 / frame+localisation mean 0.13 ms max 14.365 / route+horizon+mouvement mean 1.1 ms max 13.947 / plan de vitesse mean 1.832 ms max 17.006 / commande+composition mean 0.041 ms max 0.156 |

## Contacts

run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 1751-1755, impulsion 0 N.s, v_n 0.324 m/s, v avant/min/apres 5.201/6.146/6.984 m/s, d max 0.0547 m, cap max 2.091 deg, roulis/tangage max 2.091/2.091 deg, roues au sol min 4, sortie True, criteres OutOfBounds
run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 1751-1755, impulsion 0 N.s, v_n 0.324 m/s, v avant/min/apres 5.201/6.146/6.984 m/s, d max 0.0547 m, cap max 2.091 deg, roulis/tangage max 2.091/2.091 deg, roues au sol min 4, sortie True, criteres OutOfBounds
run 4: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1883-1885, impulsion 0 N.s, v_n 0.55 m/s, v avant/min/apres 7.477/7.682/7.882 m/s, d max 0.0535 m, cap max 2.07 deg, roulis/tangage max 2.07/2.07 deg, roues au sol min 4, sortie True, criteres aucun
run 4: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1905-1905, impulsion 0 N.s, v_n 0.086 m/s, v avant/min/apres 7.614/7.785/7.916 m/s, d max 0.0535 m, cap max 2.07 deg, roulis/tangage max 2.07/2.07 deg, roues au sol min 4, sortie True, criteres aucun
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1541-1543, impulsion 0 N.s, v_n 0.51 m/s, v avant/min/apres 7.878/7.565/6.167 m/s, d max 0.0525 m, cap max 2.032 deg, roulis/tangage max 2.032/2.032 deg, roues au sol min 4, sortie True, criteres aucun
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1563-1564, impulsion 0 N.s, v_n 0.085 m/s, v avant/min/apres 7.918/7.308/5.354 m/s, d max 0.0525 m, cap max 2.032 deg, roulis/tangage max 2.032/2.032 deg, roues au sol min 4, sortie True, criteres aucun
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 3551-3556, impulsion 0 N.s, v_n 0.345 m/s, v avant/min/apres 7.135/5.144/3.203 m/s, d max 0.0753 m, cap max 1.992 deg, roulis/tangage max 1.991/1.991 deg, roues au sol min 4, sortie True, criteres OutOfBounds
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 3551-3556, impulsion 0 N.s, v_n 0.345 m/s, v avant/min/apres 7.135/5.144/3.203 m/s, d max 0.0753 m, cap max 1.992 deg, roulis/tangage max 1.991/1.991 deg, roues au sol min 4, sortie True, criteres OutOfBounds
run 7: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1926-1928, impulsion 0 N.s, v_n 0.491 m/s, v avant/min/apres 6.173/6.854/7.414 m/s, d max 0.2573 m, cap max 2.489 deg, roulis/tangage max 2.071/2.071 deg, roues au sol min 4, sortie True, criteres aucun
run 7: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1947-1949, impulsion 0 N.s, v_n 0.156 m/s, v avant/min/apres 6.487/7.115/7.557 m/s, d max 0.2346 m, cap max 2.489 deg, roulis/tangage max 2.071/2.071 deg, roues au sol min 4, sortie True, criteres aucun

## Contacts bloquants ou consequences

run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 1751-1755, impulsion 0 N.s, v_n 0.324 m/s, v avant/min/apres 5.201/6.146/6.984 m/s, d max 0.0547 m, cap max 2.091 deg, roulis/tangage max 2.091/2.091 deg, roues au sol min 4, sortie True, criteres OutOfBounds
run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 1751-1755, impulsion 0 N.s, v_n 0.324 m/s, v avant/min/apres 5.201/6.146/6.984 m/s, d max 0.0547 m, cap max 2.091 deg, roulis/tangage max 2.091/2.091 deg, roues au sol min 4, sortie True, criteres OutOfBounds
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 3551-3556, impulsion 0 N.s, v_n 0.345 m/s, v avant/min/apres 7.135/5.144/3.203 m/s, d max 0.0753 m, cap max 1.992 deg, roulis/tangage max 1.991/1.991 deg, roues au sol min 4, sortie True, criteres OutOfBounds
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 3551-3556, impulsion 0 N.s, v_n 0.345 m/s, v avant/min/apres 7.135/5.144/3.203 m/s, d max 0.0753 m, cap max 1.992 deg, roulis/tangage max 1.991/1.991 deg, roues au sol min 4, sortie True, criteres OutOfBounds

## NotSelectable (constructeur)

aucun
