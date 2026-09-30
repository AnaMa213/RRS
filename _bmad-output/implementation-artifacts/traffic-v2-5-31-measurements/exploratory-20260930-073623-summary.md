# Story 5.31 - campagne Exploratory exploratory-20260930-073623

Campagne exploratoire : ne vaut pas acceptation.

## Conditions

- pas physique : 0.02 s ; Editeur en hote ; profils par defaut du prefab V2
- commit : 742c412d12f9d5f5f59f9f308af96488f28eb514 (refs/heads/systeme-traffic-ia-v2)
- RoadModelVersion : v4:e8dff9e54bff1712308899158ad16a9b ; budget de graines : 1 ; statut de campagne : Accepted
- epsilon_t : non declare
- modele M : tolerances 0.002 m / 0.05 deg ; reste de Lipschitz vise 0.001 m

- preuve relief 5.51 : empreinte physique 2e1677f8cf104ccba47016a51651a93d7974d500e0b3e3cbaf0c46b4eff530a7 ; chaussées identifiées 29 ; fenetre contact ±50 pas = 1 s

## Resultat

11 vehicule(s), campagne complete True, cale False, pas totaux 53408, d max au pas 1.0099 m, borne entre deux pas max (M verifie) 76.6286 m, intervalles 53380 dont ModelNotVerified 20, pas v > v* 0, couple negatif en repli 0, contacts 9, elements Measured 164 / NotMeasured 0 / NotSelectable 0, NominalPoseInfeasible 0 pas (pose nominale cinematique, contrat §8), dernier code aucun

## Runs

| run | fin | pas | replanifications | replis | d max au pas (m) | temps par etape |
|---|---|---|---|---|---|---|
| 0 | sortie | 6429 | 0 | 31 | 0.435437 | steps 6429 / frame+localisation mean 0.128 ms max 3.215 / route+horizon+mouvement mean 1.587 ms max 16.793 / plan de vitesse mean 2.629 ms max 14.643 / commande+composition mean 0.044 ms max 0.298 |
| 1 | sortie | 5701 | 1 | 1 | 0.617538 | steps 5701 / frame+localisation mean 0.125 ms max 7.371 / route+horizon+mouvement mean 1.471 ms max 16.72 / plan de vitesse mean 2.353 ms max 12.545 / commande+composition mean 0.044 ms max 0.174 |
| 2 | sortie | 4787 | 2 | 21 | 0.821917 | steps 4787 / frame+localisation mean 0.118 ms max 0.271 / route+horizon+mouvement mean 1.162 ms max 19.599 / plan de vitesse mean 1.843 ms max 13.417 / commande+composition mean 0.042 ms max 0.151 |
| 3 | sortie | 4500 | 3 | 35 | 0.821917 | steps 4500 / frame+localisation mean 0.122 ms max 3.379 / route+horizon+mouvement mean 1.13 ms max 22.565 / plan de vitesse mean 1.763 ms max 11.649 / commande+composition mean 0.042 ms max 0.151 |
| 4 | sortie | 3109 | 2 | 20 | 0.987837 | steps 3109 / frame+localisation mean 0.12 ms max 0.346 / route+horizon+mouvement mean 1.039 ms max 12.73 / plan de vitesse mean 1.758 ms max 12.366 / commande+composition mean 0.041 ms max 0.139 |
| 5 | sortie | 3410 | 2 | 21 | 0.822514 | steps 3410 / frame+localisation mean 0.123 ms max 7.692 / route+horizon+mouvement mean 0.791 ms max 18.488 / plan de vitesse mean 1.226 ms max 5.465 / commande+composition mean 0.04 ms max 0.3 |
| 6 | sortie | 5081 | 0 | 18 | 0.397564 | steps 5081 / frame+localisation mean 0.126 ms max 0.343 / route+horizon+mouvement mean 1.227 ms max 14.985 / plan de vitesse mean 2.052 ms max 7.192 / commande+composition mean 0.043 ms max 0.194 |
| 7 | sortie | 4609 | 2 | 37 | 0.822514 | steps 4609 / frame+localisation mean 0.12 ms max 0.358 / route+horizon+mouvement mean 1.125 ms max 22.644 / plan de vitesse mean 1.764 ms max 12.539 / commande+composition mean 0.042 ms max 0.181 |
| 8 | sortie | 5552 | 1 | 16 | 0.602965 | steps 5552 / frame+localisation mean 0.122 ms max 3.222 / route+horizon+mouvement mean 1.359 ms max 20.289 / plan de vitesse mean 2.22 ms max 13.643 / commande+composition mean 0.043 ms max 0.17 |
| 9 | sortie | 4540 | 3 | 37 | 0.880586 | steps 4540 / frame+localisation mean 0.122 ms max 3.218 / route+horizon+mouvement mean 1.129 ms max 24.459 / plan de vitesse mean 1.773 ms max 11.207 / commande+composition mean 0.042 ms max 0.189 |
| 10 | sortie | 5673 | 4 | 68 | 1.009879 | steps 5673 / frame+localisation mean 0.13 ms max 3.233 / route+horizon+mouvement mean 1.144 ms max 15.414 / plan de vitesse mean 1.856 ms max 12.681 / commande+composition mean 0.041 ms max 0.109 |

## Contacts

run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 1751-1755, impulsion 0 N.s, v_n 0.324 m/s, v avant/min/apres 5.201/6.146/6.984 m/s, d max 0.0547 m, cap max 2.091 deg, roulis/tangage max 2.091/2.091 deg, roues au sol min 4, sortie True, criteres OutOfBounds,LossOfControl
run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 1751-1755, impulsion 0 N.s, v_n 0.324 m/s, v avant/min/apres 5.201/6.146/6.984 m/s, d max 0.0547 m, cap max 2.091 deg, roulis/tangage max 2.091/2.091 deg, roues au sol min 4, sortie True, criteres OutOfBounds,LossOfControl
run 4: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1902-1905, impulsion 0 N.s, v_n 0.493 m/s, v avant/min/apres 7.475/7.677/7.888 m/s, d max 0.0528 m, cap max 2.043 deg, roulis/tangage max 2.043/2.043 deg, roues au sol min 4, sortie True, criteres OutOfBounds,LossOfControl
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1541-1543, impulsion 0 N.s, v_n 0.51 m/s, v avant/min/apres 7.878/7.565/6.167 m/s, d max 0.0525 m, cap max 2.032 deg, roulis/tangage max 2.032/2.032 deg, roues au sol min 4, sortie True, criteres OutOfBounds,LossOfControl
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1563-1564, impulsion 0 N.s, v_n 0.085 m/s, v avant/min/apres 7.918/7.308/5.354 m/s, d max 0.0525 m, cap max 2.032 deg, roulis/tangage max 2.032/2.032 deg, roues au sol min 4, sortie True, criteres OutOfBounds,LossOfControl
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 3551-3556, impulsion 0 N.s, v_n 0.349 m/s, v avant/min/apres 7.136/5.178/3.21 m/s, d max 0.0748 m, cap max 1.995 deg, roulis/tangage max 1.994/1.994 deg, roues au sol min 4, sortie True, criteres OutOfBounds,LossOfControl
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 3551-3556, impulsion 0 N.s, v_n 0.349 m/s, v avant/min/apres 7.136/5.178/3.21 m/s, d max 0.0748 m, cap max 1.995 deg, roulis/tangage max 1.994/1.994 deg, roues au sol min 4, sortie True, criteres OutOfBounds,LossOfControl
run 7: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1925-1928, impulsion 5105.446 N.s, v_n 6.857 m/s, v avant/min/apres 6.164/2.595/4.188 m/s, d max 0.2573 m, cap max 4.617 deg, roulis/tangage max 1.954/1.954 deg, roues au sol min 3, sortie True, criteres OutOfBounds,LossOfControl
run 7: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1960-1960, impulsion 0 N.s, v_n 0.022 m/s, v avant/min/apres 6.633/3.717/4.897 m/s, d max 0.2549 m, cap max 4.617 deg, roulis/tangage max 1.954/1.954 deg, roues au sol min 2, sortie True, criteres OutOfBounds,LossOfControl

## Contacts bloquants ou consequences

run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 1751-1755, impulsion 0 N.s, v_n 0.324 m/s, v avant/min/apres 5.201/6.146/6.984 m/s, d max 0.0547 m, cap max 2.091 deg, roulis/tangage max 2.091/2.091 deg, roues au sol min 4, sortie True, criteres OutOfBounds,LossOfControl
run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 1751-1755, impulsion 0 N.s, v_n 0.324 m/s, v avant/min/apres 5.201/6.146/6.984 m/s, d max 0.0547 m, cap max 2.091 deg, roulis/tangage max 2.091/2.091 deg, roues au sol min 4, sortie True, criteres OutOfBounds,LossOfControl
run 4: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1902-1905, impulsion 0 N.s, v_n 0.493 m/s, v avant/min/apres 7.475/7.677/7.888 m/s, d max 0.0528 m, cap max 2.043 deg, roulis/tangage max 2.043/2.043 deg, roues au sol min 4, sortie True, criteres OutOfBounds,LossOfControl
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1541-1543, impulsion 0 N.s, v_n 0.51 m/s, v avant/min/apres 7.878/7.565/6.167 m/s, d max 0.0525 m, cap max 2.032 deg, roulis/tangage max 2.032/2.032 deg, roues au sol min 4, sortie True, criteres OutOfBounds,LossOfControl
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1563-1564, impulsion 0 N.s, v_n 0.085 m/s, v avant/min/apres 7.918/7.308/5.354 m/s, d max 0.0525 m, cap max 2.032 deg, roulis/tangage max 2.032/2.032 deg, roues au sol min 4, sortie True, criteres OutOfBounds,LossOfControl
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 3551-3556, impulsion 0 N.s, v_n 0.349 m/s, v avant/min/apres 7.136/5.178/3.21 m/s, d max 0.0748 m, cap max 1.995 deg, roulis/tangage max 1.994/1.994 deg, roues au sol min 4, sortie True, criteres OutOfBounds,LossOfControl
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 3551-3556, impulsion 0 N.s, v_n 0.349 m/s, v avant/min/apres 7.136/5.178/3.21 m/s, d max 0.0748 m, cap max 1.995 deg, roulis/tangage max 1.994/1.994 deg, roues au sol min 4, sortie True, criteres OutOfBounds,LossOfControl
run 7: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1925-1928, impulsion 5105.446 N.s, v_n 6.857 m/s, v avant/min/apres 6.164/2.595/4.188 m/s, d max 0.2573 m, cap max 4.617 deg, roulis/tangage max 1.954/1.954 deg, roues au sol min 3, sortie True, criteres OutOfBounds,LossOfControl
run 7: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1960-1960, impulsion 0 N.s, v_n 0.022 m/s, v avant/min/apres 6.633/3.717/4.897 m/s, d max 0.2549 m, cap max 4.617 deg, roulis/tangage max 1.954/1.954 deg, roues au sol min 2, sortie True, criteres OutOfBounds,LossOfControl

## NotSelectable (constructeur)

aucun
