# Story 5.31 - campagne Exploratory exploratory-20260930-135653

Campagne exploratoire : ne vaut pas acceptation.

## Conditions

- pas physique : 0.02 s ; Editeur en hote ; profils par defaut du prefab V2
- commit : 742c412d12f9d5f5f59f9f308af96488f28eb514 (refs/heads/systeme-traffic-ia-v2)
- RoadModelVersion : v4:e8dff9e54bff1712308899158ad16a9b ; budget de graines : 1 ; statut de campagne : Accepted
- epsilon_t : non declare
- modele M : tolerances 0.002 m / 0.05 deg ; reste de Lipschitz vise 0.001 m

- preuve relief 5.51 : empreinte physique 2e1677f8cf104ccba47016a51651a93d7974d500e0b3e3cbaf0c46b4eff530a7 ; chaussées identifiées 29 ; fenetre contact ±50 pas = 1 s

## Resultat

7 vehicule(s), campagne complete True, cale False, pas totaux 23496, d max au pas 0.3296 m, borne entre deux pas max (M verifie) 0.3306 m, intervalles 23479 dont ModelNotVerified 0, pas v > v* 0, couple negatif en repli 0, contacts 6, elements Measured 157 / NotMeasured 7 / NotSelectable 0, NominalPoseInfeasible 0 pas (pose nominale cinematique, contrat §8), dernier code aucun

## Runs

| run | fin | pas | replanifications | replis | d max au pas (m) | temps par etape |
|---|---|---|---|---|---|---|
| 0 | sortie | 4451 | 0 | 1 | 0.327644 | steps 4451 / frame+localisation mean 0.148 ms max 0.457 / route+horizon+mouvement mean 2.266 ms max 16.826 / plan de vitesse mean 2.655 ms max 12.981 / commande+composition mean 0.041 ms max 0.146 |
| 1 | sortie | 3683 | 0 | 1 | 0.329588 | steps 3683 / frame+localisation mean 0.143 ms max 3.316 / route+horizon+mouvement mean 2.056 ms max 15.977 / plan de vitesse mean 2.372 ms max 13.274 / commande+composition mean 0.042 ms max 0.162 |
| 2 | sortie | 2633 | 0 | 1 | 0.196801 | steps 2633 / frame+localisation mean 0.129 ms max 3.257 / route+horizon+mouvement mean 1.645 ms max 14.854 / plan de vitesse mean 1.88 ms max 11.177 / commande+composition mean 0.039 ms max 0.111 |
| 3 | sortie | 2862 | 0 | 1 | 0.328179 | steps 2862 / frame+localisation mean 0.138 ms max 0.429 / route+horizon+mouvement mean 1.612 ms max 14.473 / plan de vitesse mean 1.874 ms max 11.301 / commande+composition mean 0.039 ms max 0.094 |
| 4 | sortie | 3948 | 0 | 1 | 0.327428 | steps 3948 / frame+localisation mean 0.144 ms max 6.981 / route+horizon+mouvement mean 2.037 ms max 16.436 / plan de vitesse mean 2.397 ms max 13.437 / commande+composition mean 0.039 ms max 0.095 |
| 5 | sortie | 2177 | 0 | 1 | 0.328425 | steps 2177 / frame+localisation mean 0.138 ms max 0.348 / route+horizon+mouvement mean 1.1 ms max 14.116 / plan de vitesse mean 1.253 ms max 10.215 / commande+composition mean 0.037 ms max 0.114 |
| 6 | sortie | 3732 | 0 | 1 | 0.328483 | steps 3732 / frame+localisation mean 0.146 ms max 0.389 / route+horizon+mouvement mean 1.81 ms max 15.626 / plan de vitesse mean 2.102 ms max 11.333 / commande+composition mean 0.04 ms max 0.128 |

## Triplets

- run 0 : 4882b42dcf37410f4f629825c30f5f99 -> 49d29d3fd771ae4966aca755b3442cbc ; via 489d4a3b8777534a9bdbd8fb0c6cfc8f ; graine 0
- run 1 : 4889b9d5d0f80bc3ce3c3514a5d6f7aa -> 48090a5d359625c04fb6050a2407f292 ; via 4bbce8157214fb0cc7c56ffb4818e38a ; graine 0
- run 2 : 48d96a31a823c14dc3c3c55e08da6bbb -> 436d9a1a58c93bd19cdf0014de309f98 ; via 48af5815b3bc108d4b441d89a7823d88 ; graine 0
- run 3 : 4ce14124d3729e5b3ad1174c8653a99d -> 4dcf9641f7a714b8bc785696d1709798 ; via 4ac23ecbf80f11d7feda468178247ab4 ; graine 0
- run 4 : 4889b9d5d0f80bc3ce3c3514a5d6f7aa -> 436d9a1a58c93bd19cdf0014de309f98 ; via 40ca7f10a97f50a918e8c3a2a1e58493 ; graine 0
- run 5 : 4882b42dcf37410f4f629825c30f5f99 -> 49d29d3fd771ae4966aca755b3442cbc ; via 410eb197d612c9963b40e6d1b7fe15bf ; graine 0
- run 6 : 4882b42dcf37410f4f629825c30f5f99 -> 48090a5d359625c04fb6050a2407f292 ; via 491e02f31f623c7a5d46026ad368f388 ; graine 0

## Contacts

run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 1093-1097, impulsion 0 N.s, v_n 0.378 m/s, v avant/min/apres 5.337/6.253/7.053 m/s, d max 0.0549 m, cap max 2.097 deg, roulis/tangage max 2.097/2.097 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 1093-1097, impulsion 0 N.s, v_n 0.378 m/s, v avant/min/apres 5.337/6.253/7.053 m/s, d max 0.0549 m, cap max 2.097 deg, roulis/tangage max 2.097/2.097 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1087-1087, impulsion 8312.421 N.s, v_n 7.505 m/s, v avant/min/apres 7.722/7.522/2.371 m/s, d max 0.0574 m, cap max 2.211 deg, roulis/tangage max 2.211/2.211 deg, roues au sol min 2, sortie True, criteres aucun
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1135-1142, impulsion 0 N.s, v_n 0.246 m/s, v avant/min/apres 7.535/2.313/3.503 m/s, d max 0.0574 m, cap max 2.211 deg, roulis/tangage max 2.211/2.211 deg, roues au sol min 2, sortie True, criteres aucun
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 2676-2682, impulsion 0 N.s, v_n 0.358 m/s, v avant/min/apres 6.383/4.955/3.198 m/s, d max 0.0693 m, cap max 2.025 deg, roulis/tangage max 2.025/2.025 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 2676-2682, impulsion 0 N.s, v_n 0.358 m/s, v avant/min/apres 6.383/4.955/3.198 m/s, d max 0.0693 m, cap max 2.025 deg, roulis/tangage max 2.025/2.025 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)

## Contacts bloquants ou consequences

run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 1093-1097, impulsion 0 N.s, v_n 0.378 m/s, v avant/min/apres 5.337/6.253/7.053 m/s, d max 0.0549 m, cap max 2.097 deg, roulis/tangage max 2.097/2.097 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 1093-1097, impulsion 0 N.s, v_n 0.378 m/s, v avant/min/apres 5.337/6.253/7.053 m/s, d max 0.0549 m, cap max 2.097 deg, roulis/tangage max 2.097/2.097 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 2676-2682, impulsion 0 N.s, v_n 0.358 m/s, v avant/min/apres 6.383/4.955/3.198 m/s, d max 0.0693 m, cap max 2.025 deg, roulis/tangage max 2.025/2.025 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 2676-2682, impulsion 0 N.s, v_n 0.358 m/s, v avant/min/apres 6.383/4.955/3.198 m/s, d max 0.0693 m, cap max 2.025 deg, roulis/tangage max 2.025/2.025 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)

## NotSelectable (constructeur)

aucun
