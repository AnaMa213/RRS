# Story 5.31 - campagne Exploratory exploratory-20260930-095445

Campagne exploratoire : ne vaut pas acceptation.

## Conditions

- pas physique : 0.02 s ; Editeur en hote ; profils par defaut du prefab V2
- commit : 742c412d12f9d5f5f59f9f308af96488f28eb514 (refs/heads/systeme-traffic-ia-v2)
- RoadModelVersion : v4:e8dff9e54bff1712308899158ad16a9b ; budget de graines : 1 ; statut de campagne : Accepted
- epsilon_t : non declare
- modele M : tolerances 0.002 m / 0.05 deg ; reste de Lipschitz vise 0.001 m

- preuve relief 5.51 : empreinte physique 2e1677f8cf104ccba47016a51651a93d7974d500e0b3e3cbaf0c46b4eff530a7 ; chaussées identifiées 29 ; fenetre contact ±50 pas = 1 s

## Resultat

7 vehicule(s), campagne complete True, cale False, pas totaux 32998, d max au pas 0.8225 m, borne entre deux pas max (M verifie) 0.8235 m, intervalles 32981 dont ModelNotVerified 0, pas v > v* 0, couple negatif en repli 0, contacts 8, elements Measured 156 / NotMeasured 8 / NotSelectable 0, NominalPoseInfeasible 0 pas (pose nominale cinematique, contrat §8), dernier code aucun

## Runs

| run | fin | pas | replanifications | replis | d max au pas (m) | temps par etape |
|---|---|---|---|---|---|---|
| 0 | sortie | 6416 | 0 | 31 | 0.435437 | steps 6416 / frame+localisation mean 0.131 ms max 9.972 / route+horizon+mouvement mean 1.656 ms max 19.444 / plan de vitesse mean 2.597 ms max 16.563 / commande+composition mean 0.045 ms max 0.329 |
| 1 | sortie | 5682 | 1 | 1 | 0.614437 | steps 5682 / frame+localisation mean 0.128 ms max 9.28 / route+horizon+mouvement mean 1.462 ms max 19.496 / plan de vitesse mean 2.312 ms max 15.532 / commande+composition mean 0.044 ms max 0.168 |
| 2 | sortie | 4811 | 2 | 21 | 0.821917 | steps 4811 / frame+localisation mean 0.119 ms max 9.212 / route+horizon+mouvement mean 1.179 ms max 18.292 / plan de vitesse mean 1.802 ms max 13.355 / commande+composition mean 0.042 ms max 0.127 |
| 3 | sortie | 4500 | 3 | 35 | 0.821917 | steps 4500 / frame+localisation mean 0.12 ms max 0.365 / route+horizon+mouvement mean 1.116 ms max 22.482 / plan de vitesse mean 1.733 ms max 11.726 / commande+composition mean 0.042 ms max 0.133 |
| 4 | sortie | 3074 | 2 | 20 | 0.620357 | steps 3074 / frame+localisation mean 0.122 ms max 8.98 / route+horizon+mouvement mean 1.133 ms max 19.313 / plan de vitesse mean 1.773 ms max 15.096 / commande+composition mean 0.041 ms max 0.089 |
| 5 | sortie | 3410 | 2 | 21 | 0.822514 | steps 3410 / frame+localisation mean 0.12 ms max 0.333 / route+horizon+mouvement mean 0.808 ms max 18.051 / plan de vitesse mean 1.206 ms max 11.091 / commande+composition mean 0.04 ms max 0.095 |
| 6 | sortie | 5095 | 0 | 18 | 0.392516 | steps 5095 / frame+localisation mean 0.125 ms max 0.368 / route+horizon+mouvement mean 1.265 ms max 18.766 / plan de vitesse mean 2.028 ms max 14.489 / commande+composition mean 0.043 ms max 0.102 |

## Contacts

run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 1751-1755, impulsion 0 N.s, v_n 0.324 m/s, v avant/min/apres 5.201/6.146/6.984 m/s, d max 0.0547 m, cap max 2.091 deg, roulis/tangage max 2.091/2.091 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 1751-1755, impulsion 0 N.s, v_n 0.324 m/s, v avant/min/apres 5.201/6.146/6.984 m/s, d max 0.0547 m, cap max 2.091 deg, roulis/tangage max 2.091/2.091 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 4: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1883-1885, impulsion 0 N.s, v_n 0.55 m/s, v avant/min/apres 7.477/7.682/7.882 m/s, d max 0.0535 m, cap max 2.07 deg, roulis/tangage max 2.07/2.07 deg, roues au sol min 4, sortie True, criteres aucun
run 4: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1905-1905, impulsion 0 N.s, v_n 0.086 m/s, v avant/min/apres 7.614/7.785/7.916 m/s, d max 0.0535 m, cap max 2.07 deg, roulis/tangage max 2.07/2.07 deg, roues au sol min 4, sortie True, criteres aucun
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1541-1543, impulsion 0 N.s, v_n 0.51 m/s, v avant/min/apres 7.878/7.565/6.167 m/s, d max 0.0525 m, cap max 2.032 deg, roulis/tangage max 2.032/2.032 deg, roues au sol min 4, sortie True, criteres aucun
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1563-1564, impulsion 0 N.s, v_n 0.085 m/s, v avant/min/apres 7.918/7.308/5.354 m/s, d max 0.0525 m, cap max 2.032 deg, roulis/tangage max 2.032/2.032 deg, roues au sol min 4, sortie True, criteres aucun
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 3551-3556, impulsion 0 N.s, v_n 0.346 m/s, v avant/min/apres 7.135/5.145/3.203 m/s, d max 0.0753 m, cap max 1.992 deg, roulis/tangage max 1.992/1.992 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 3551-3556, impulsion 0 N.s, v_n 0.346 m/s, v avant/min/apres 7.135/5.145/3.203 m/s, d max 0.0753 m, cap max 1.992 deg, roulis/tangage max 1.992/1.992 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)

## Contacts bloquants ou consequences

run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 1751-1755, impulsion 0 N.s, v_n 0.324 m/s, v avant/min/apres 5.201/6.146/6.984 m/s, d max 0.0547 m, cap max 2.091 deg, roulis/tangage max 2.091/2.091 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 1751-1755, impulsion 0 N.s, v_n 0.324 m/s, v avant/min/apres 5.201/6.146/6.984 m/s, d max 0.0547 m, cap max 2.091 deg, roulis/tangage max 2.091/2.091 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 3551-3556, impulsion 0 N.s, v_n 0.346 m/s, v avant/min/apres 7.135/5.145/3.203 m/s, d max 0.0753 m, cap max 1.992 deg, roulis/tangage max 1.992/1.992 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 3551-3556, impulsion 0 N.s, v_n 0.346 m/s, v avant/min/apres 7.135/5.145/3.203 m/s, d max 0.0753 m, cap max 1.992 deg, roulis/tangage max 1.992/1.992 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)

## NotSelectable (constructeur)

aucun
