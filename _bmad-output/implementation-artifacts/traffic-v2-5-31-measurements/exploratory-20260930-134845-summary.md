# Story 5.31 - campagne Exploratory exploratory-20260930-134845

Campagne exploratoire : ne vaut pas acceptation.

## Conditions

- pas physique : 0.02 s ; Editeur en hote ; profils par defaut du prefab V2
- commit : 742c412d12f9d5f5f59f9f308af96488f28eb514 (refs/heads/systeme-traffic-ia-v2)
- RoadModelVersion : v4:e8dff9e54bff1712308899158ad16a9b ; budget de graines : 1 ; statut de campagne : Accepted
- epsilon_t : non declare
- modele M : tolerances 0.002 m / 0.05 deg ; reste de Lipschitz vise 0.001 m

- preuve relief 5.51 : empreinte physique 2e1677f8cf104ccba47016a51651a93d7974d500e0b3e3cbaf0c46b4eff530a7 ; chaussées identifiées 29 ; fenetre contact ±50 pas = 1 s

## Resultat

9 vehicule(s), campagne complete True, cale False, pas totaux 24181, d max au pas 0.3276 m, borne entre deux pas max (M verifie) 0.3286 m, intervalles 24158 dont ModelNotVerified 0, pas v > v* 0, couple negatif en repli 0, contacts 8, elements Measured 67 / NotMeasured 97 / NotSelectable 0, NominalPoseInfeasible 0 pas (pose nominale cinematique, contrat §8), dernier code aucun

## Runs

| run | fin | pas | replanifications | replis | d max au pas (m) | temps par etape |
|---|---|---|---|---|---|---|
| 0 | sortie | 939 | 0 | 1 | 0.196206 | steps 939 / frame+localisation mean 0.119 ms max 0.283 / route+horizon+mouvement mean 0.481 ms max 7.854 / plan de vitesse mean 0.514 ms max 3.531 / commande+composition mean 0.031 ms max 0.099 |
| 1 | sortie | 1489 | 0 | 1 | 0.196206 | steps 1489 / frame+localisation mean 0.13 ms max 0.353 / route+horizon+mouvement mean 0.66 ms max 8.612 / plan de vitesse mean 0.762 ms max 7.441 / commande+composition mean 0.028 ms max 0.068 |
| 2 | sortie | 1990 | 0 | 1 | 0.163664 | steps 1990 / frame+localisation mean 0.152 ms max 3.364 / route+horizon+mouvement mean 1.043 ms max 10.58 / plan de vitesse mean 1.178 ms max 9.765 / commande+composition mean 0.041 ms max 0.133 |
| 3 | sortie | 2978 | 0 | 1 | 0.327641 | steps 2978 / frame+localisation mean 0.144 ms max 3.243 / route+horizon+mouvement mean 1.423 ms max 12.488 / plan de vitesse mean 1.687 ms max 10.926 / commande+composition mean 0.039 ms max 0.104 |
| 4 | sortie | 3605 | 0 | 1 | 0.32713 | steps 3605 / frame+localisation mean 0.152 ms max 0.486 / route+horizon+mouvement mean 1.677 ms max 14.828 / plan de vitesse mean 2.021 ms max 12.083 / commande+composition mean 0.041 ms max 0.168 |
| 5 | sortie | 2978 | 0 | 1 | 0.327641 | steps 2978 / frame+localisation mean 0.144 ms max 3.292 / route+horizon+mouvement mean 1.428 ms max 12.896 / plan de vitesse mean 1.686 ms max 10.207 / commande+composition mean 0.039 ms max 0.133 |
| 6 | sortie | 3605 | 0 | 1 | 0.32713 | steps 3605 / frame+localisation mean 0.152 ms max 0.409 / route+horizon+mouvement mean 1.717 ms max 15.362 / plan de vitesse mean 2.034 ms max 12.311 / commande+composition mean 0.04 ms max 0.097 |
| 7 | sortie | 2978 | 0 | 1 | 0.327641 | steps 2978 / frame+localisation mean 0.144 ms max 0.368 / route+horizon+mouvement mean 1.464 ms max 15.675 / plan de vitesse mean 1.714 ms max 11.192 / commande+composition mean 0.039 ms max 0.101 |
| 8 | sortie | 3605 | 0 | 1 | 0.32713 | steps 3605 / frame+localisation mean 0.152 ms max 0.418 / route+horizon+mouvement mean 1.717 ms max 15.636 / plan de vitesse mean 2.034 ms max 11.403 / commande+composition mean 0.04 ms max 0.104 |

## Triplets

- run 0 : 4882b42dcf37410f4f629825c30f5f99 -> 48090a5d359625c04fb6050a2407f292 ; via 431a11dff650b4115fa5bf99118b9b8a ; graine 0
- run 1 : 4882b42dcf37410f4f629825c30f5f99 -> 48090a5d359625c04fb6050a2407f292 ; via 4aa676c5e3857524d5a9b386be4bdb90 ; graine 0
- run 2 : 48d96a31a823c14dc3c3c55e08da6bbb -> 4dcf9641f7a714b8bc785696d1709798 ; via 4d54e6de5a6bbf4d1eb20f8ed5a40cb2 ; graine 0
- run 3 : 4882b42dcf37410f4f629825c30f5f99 -> 48090a5d359625c04fb6050a2407f292 ; via 4dc4e81b4c9cdd45e0484d2e9daeb888 ; graine 0
- run 4 : 4882b42dcf37410f4f629825c30f5f99 -> 4dcf9641f7a714b8bc785696d1709798 ; via 453f130c460dc35e052c30714bec6c8e ; graine 0
- run 5 : 4882b42dcf37410f4f629825c30f5f99 -> 48090a5d359625c04fb6050a2407f292 ; via 4dc4e81b4c9cdd45e0484d2e9daeb888 ; graine 0
- run 6 : 4882b42dcf37410f4f629825c30f5f99 -> 4dcf9641f7a714b8bc785696d1709798 ; via 453f130c460dc35e052c30714bec6c8e ; graine 0
- run 7 : 4882b42dcf37410f4f629825c30f5f99 -> 48090a5d359625c04fb6050a2407f292 ; via 4dc4e81b4c9cdd45e0484d2e9daeb888 ; graine 0
- run 8 : 4882b42dcf37410f4f629825c30f5f99 -> 4dcf9641f7a714b8bc785696d1709798 ; via 453f130c460dc35e052c30714bec6c8e ; graine 0

## Contacts

run 2: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 945-949, impulsion 0 N.s, v_n 0.345 m/s, v avant/min/apres 6.353/5.917/4.43 m/s, d max 0.0726 m, cap max 2.041 deg, roulis/tangage max 2.041/2.041 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 2: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 945-949, impulsion 0 N.s, v_n 0.345 m/s, v avant/min/apres 6.353/5.917/4.43 m/s, d max 0.0726 m, cap max 2.041 deg, roulis/tangage max 2.041/2.041 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 4: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 2559-2564, impulsion 0 N.s, v_n 0.366 m/s, v avant/min/apres 6.351/5.913/4.396 m/s, d max 0.0744 m, cap max 2.038 deg, roulis/tangage max 2.037/2.037 deg, roues au sol min 4, sortie True, criteres aucun
run 4: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 2559-2564, impulsion 0 N.s, v_n 0.366 m/s, v avant/min/apres 6.351/5.913/4.396 m/s, d max 0.0744 m, cap max 2.038 deg, roulis/tangage max 2.037/2.037 deg, roues au sol min 4, sortie True, criteres aucun
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 2559-2564, impulsion 0 N.s, v_n 0.366 m/s, v avant/min/apres 6.351/5.913/4.396 m/s, d max 0.0744 m, cap max 2.038 deg, roulis/tangage max 2.037/2.037 deg, roues au sol min 4, sortie True, criteres aucun
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 2559-2564, impulsion 0 N.s, v_n 0.366 m/s, v avant/min/apres 6.351/5.913/4.396 m/s, d max 0.0744 m, cap max 2.038 deg, roulis/tangage max 2.037/2.037 deg, roues au sol min 4, sortie True, criteres aucun
run 8: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 2559-2564, impulsion 0 N.s, v_n 0.366 m/s, v avant/min/apres 6.351/5.913/4.396 m/s, d max 0.0744 m, cap max 2.038 deg, roulis/tangage max 2.037/2.037 deg, roues au sol min 4, sortie True, criteres aucun
run 8: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 2559-2564, impulsion 0 N.s, v_n 0.366 m/s, v avant/min/apres 6.351/5.913/4.396 m/s, d max 0.0744 m, cap max 2.038 deg, roulis/tangage max 2.037/2.037 deg, roues au sol min 4, sortie True, criteres aucun

## Contacts bloquants ou consequences

run 2: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 945-949, impulsion 0 N.s, v_n 0.345 m/s, v avant/min/apres 6.353/5.917/4.43 m/s, d max 0.0726 m, cap max 2.041 deg, roulis/tangage max 2.041/2.041 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 2: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 945-949, impulsion 0 N.s, v_n 0.345 m/s, v avant/min/apres 6.353/5.917/4.43 m/s, d max 0.0726 m, cap max 2.041 deg, roulis/tangage max 2.041/2.041 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)

## NotSelectable (constructeur)

aucun
