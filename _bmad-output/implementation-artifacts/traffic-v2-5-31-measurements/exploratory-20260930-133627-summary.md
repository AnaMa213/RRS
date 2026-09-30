# Story 5.31 - campagne Exploratory exploratory-20260930-133627

Campagne exploratoire : ne vaut pas acceptation.

## Conditions

- pas physique : 0.02 s ; Editeur en hote ; profils par defaut du prefab V2
- commit : 742c412d12f9d5f5f59f9f308af96488f28eb514 (refs/heads/systeme-traffic-ia-v2)
- RoadModelVersion : v4:e8dff9e54bff1712308899158ad16a9b ; budget de graines : 1 ; statut de campagne : Accepted
- epsilon_t : non declare
- modele M : tolerances 0.002 m / 0.05 deg ; reste de Lipschitz vise 0.001 m

- preuve relief 5.51 : empreinte physique 2e1677f8cf104ccba47016a51651a93d7974d500e0b3e3cbaf0c46b4eff530a7 ; chaussées identifiées 29 ; fenetre contact ±50 pas = 1 s

## Resultat

11 vehicule(s), campagne complete True, cale False, pas totaux 36604, d max au pas 0.3296 m, borne entre deux pas max (M verifie) 0.3305 m, intervalles 36576 dont ModelNotVerified 0, pas v > v* 0, couple negatif en repli 0, contacts 10, elements Measured 164 / NotMeasured 0 / NotSelectable 0, NominalPoseInfeasible 0 pas (pose nominale cinematique, contrat §8), dernier code aucun

## Runs

| run | fin | pas | replanifications | replis | d max au pas (m) | temps par etape |
|---|---|---|---|---|---|---|
| 0 | sortie | 4451 | 0 | 1 | 0.327637 | steps 4451 / frame+localisation mean 0.149 ms max 0.577 / route+horizon+mouvement mean 2.283 ms max 15.651 / plan de vitesse mean 2.662 ms max 13.769 / commande+composition mean 0.042 ms max 0.1 |
| 1 | sortie | 3683 | 0 | 1 | 0.329551 | steps 3683 / frame+localisation mean 0.144 ms max 3.292 / route+horizon+mouvement mean 2.083 ms max 19.013 / plan de vitesse mean 2.386 ms max 12.158 / commande+composition mean 0.042 ms max 0.097 |
| 2 | sortie | 2633 | 0 | 1 | 0.196796 | steps 2633 / frame+localisation mean 0.129 ms max 0.26 / route+horizon+mouvement mean 1.642 ms max 14.152 / plan de vitesse mean 1.898 ms max 10.744 / commande+composition mean 0.039 ms max 0.084 |
| 3 | sortie | 2862 | 0 | 1 | 0.328179 | steps 2862 / frame+localisation mean 0.14 ms max 3.234 / route+horizon+mouvement mean 1.605 ms max 14.603 / plan de vitesse mean 1.862 ms max 10.953 / commande+composition mean 0.04 ms max 0.095 |
| 4 | sortie | 3948 | 0 | 1 | 0.327112 | steps 3948 / frame+localisation mean 0.142 ms max 0.431 / route+horizon+mouvement mean 2.046 ms max 19.176 / plan de vitesse mean 2.402 ms max 12.92 / commande+composition mean 0.04 ms max 0.146 |
| 5 | sortie | 2177 | 0 | 1 | 0.328425 | steps 2177 / frame+localisation mean 0.138 ms max 0.357 / route+horizon+mouvement mean 1.094 ms max 11.615 / plan de vitesse mean 1.248 ms max 9.441 / commande+composition mean 0.037 ms max 0.131 |
| 6 | sortie | 3732 | 0 | 1 | 0.328483 | steps 3732 / frame+localisation mean 0.147 ms max 3.301 / route+horizon+mouvement mean 1.806 ms max 15.987 / plan de vitesse mean 2.114 ms max 12.761 / commande+composition mean 0.04 ms max 0.085 |
| 7 | sortie | 2977 | 0 | 1 | 0.328212 | steps 2977 / frame+localisation mean 0.143 ms max 0.467 / route+horizon+mouvement mean 1.603 ms max 14.861 / plan de vitesse mean 1.879 ms max 11.473 / commande+composition mean 0.039 ms max 0.097 |
| 8 | sortie | 3542 | 0 | 1 | 0.327569 | steps 3542 / frame+localisation mean 0.145 ms max 6.035 / route+horizon+mouvement mean 1.959 ms max 15.608 / plan de vitesse mean 2.245 ms max 12.877 / commande+composition mean 0.041 ms max 0.134 |
| 9 | sortie | 2978 | 0 | 1 | 0.32761 | steps 2978 / frame+localisation mean 0.144 ms max 0.371 / route+horizon+mouvement mean 1.491 ms max 14.081 / plan de vitesse mean 1.731 ms max 10.647 / commande+composition mean 0.039 ms max 0.131 |
| 10 | sortie | 3604 | 0 | 1 | 0.327127 | steps 3604 / frame+localisation mean 0.153 ms max 0.496 / route+horizon+mouvement mean 1.77 ms max 15.135 / plan de vitesse mean 2.056 ms max 12.418 / commande+composition mean 0.041 ms max 0.127 |

## Triplets

- run 0 : 4882b42dcf37410f4f629825c30f5f99 -> 49d29d3fd771ae4966aca755b3442cbc ; via 489d4a3b8777534a9bdbd8fb0c6cfc8f ; graine 0
- run 1 : 4889b9d5d0f80bc3ce3c3514a5d6f7aa -> 48090a5d359625c04fb6050a2407f292 ; via 4bbce8157214fb0cc7c56ffb4818e38a ; graine 0
- run 2 : 48d96a31a823c14dc3c3c55e08da6bbb -> 436d9a1a58c93bd19cdf0014de309f98 ; via 48af5815b3bc108d4b441d89a7823d88 ; graine 0
- run 3 : 4ce14124d3729e5b3ad1174c8653a99d -> 4dcf9641f7a714b8bc785696d1709798 ; via 4ac23ecbf80f11d7feda468178247ab4 ; graine 0
- run 4 : 4889b9d5d0f80bc3ce3c3514a5d6f7aa -> 436d9a1a58c93bd19cdf0014de309f98 ; via 40ca7f10a97f50a918e8c3a2a1e58493 ; graine 0
- run 5 : 4882b42dcf37410f4f629825c30f5f99 -> 49d29d3fd771ae4966aca755b3442cbc ; via 410eb197d612c9963b40e6d1b7fe15bf ; graine 0
- run 6 : 4882b42dcf37410f4f629825c30f5f99 -> 48090a5d359625c04fb6050a2407f292 ; via 491e02f31f623c7a5d46026ad368f388 ; graine 0
- run 7 : 4882b42dcf37410f4f629825c30f5f99 -> 48090a5d359625c04fb6050a2407f292 ; via 414c99412444a4a56b97dd36be51b593 ; graine 0
- run 8 : 4882b42dcf37410f4f629825c30f5f99 -> 436d9a1a58c93bd19cdf0014de309f98 ; via 4c6997b98ac692f6bd24a50f86009eb3 ; graine 0
- run 9 : 4882b42dcf37410f4f629825c30f5f99 -> 48090a5d359625c04fb6050a2407f292 ; via 4dc4e81b4c9cdd45e0484d2e9daeb888 ; graine 0
- run 10 : 4882b42dcf37410f4f629825c30f5f99 -> 4dcf9641f7a714b8bc785696d1709798 ; via 453f130c460dc35e052c30714bec6c8e ; graine 0

## Contacts

run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 1093-1097, impulsion 0 N.s, v_n 0.378 m/s, v avant/min/apres 5.337/6.253/7.053 m/s, d max 0.0549 m, cap max 2.097 deg, roulis/tangage max 2.097/2.097 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 1093-1097, impulsion 0 N.s, v_n 0.378 m/s, v avant/min/apres 5.337/6.253/7.053 m/s, d max 0.0549 m, cap max 2.097 deg, roulis/tangage max 2.097/2.097 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1087-1087, impulsion 8312.421 N.s, v_n 7.505 m/s, v avant/min/apres 7.722/7.522/2.371 m/s, d max 0.0574 m, cap max 2.211 deg, roulis/tangage max 2.211/2.211 deg, roues au sol min 2, sortie True, criteres aucun
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_MarcheBasse_AvenueCenterToEast [DrivableRelief] pas 1135-1142, impulsion 0 N.s, v_n 0.246 m/s, v avant/min/apres 7.535/2.313/3.503 m/s, d max 0.0574 m, cap max 2.211 deg, roulis/tangage max 2.211/2.211 deg, roues au sol min 2, sortie True, criteres aucun
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 2676-2682, impulsion 0 N.s, v_n 0.354 m/s, v avant/min/apres 6.383/4.954/3.197 m/s, d max 0.0695 m, cap max 2.024 deg, roulis/tangage max 2.023/2.023 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 2676-2682, impulsion 0 N.s, v_n 0.354 m/s, v avant/min/apres 6.383/4.954/3.197 m/s, d max 0.0695 m, cap max 2.024 deg, roulis/tangage max 2.023/2.023 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 7: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 1333-1335, impulsion 0 N.s, v_n 0.153 m/s, v avant/min/apres 4.364/5.433/6.25 m/s, d max 0.1376 m, cap max 2.175 deg, roulis/tangage max 2.102/2.102 deg, roues au sol min 4, sortie True, criteres aucun
run 7: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 1333-1335, impulsion 0 N.s, v_n 0.153 m/s, v avant/min/apres 4.364/5.433/6.25 m/s, d max 0.1376 m, cap max 2.175 deg, roulis/tangage max 2.102/2.102 deg, roues au sol min 4, sortie True, criteres aucun
run 10: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 2559-2564, impulsion 0 N.s, v_n 0.369 m/s, v avant/min/apres 6.35/5.913/4.397 m/s, d max 0.0743 m, cap max 2.039 deg, roulis/tangage max 2.039/2.039 deg, roues au sol min 4, sortie True, criteres aucun
run 10: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 2559-2564, impulsion 0 N.s, v_n 0.369 m/s, v avant/min/apres 6.35/5.913/4.397 m/s, d max 0.0743 m, cap max 2.039 deg, roulis/tangage max 2.039/2.039 deg, roues au sol min 4, sortie True, criteres aucun

## Contacts bloquants ou consequences

run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 1093-1097, impulsion 0 N.s, v_n 0.378 m/s, v avant/min/apres 5.337/6.253/7.053 m/s, d max 0.0549 m, cap max 2.097 deg, roulis/tangage max 2.097/2.097 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 1093-1097, impulsion 0 N.s, v_n 0.378 m/s, v avant/min/apres 5.337/6.253/7.053 m/s, d max 0.0549 m, cap max 2.097 deg, roulis/tangage max 2.097/2.097 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 2676-2682, impulsion 0 N.s, v_n 0.354 m/s, v avant/min/apres 6.383/4.954/3.197 m/s, d max 0.0695 m, cap max 2.024 deg, roulis/tangage max 2.023/2.023 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)
run 6: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 2676-2682, impulsion 0 N.s, v_n 0.354 m/s, v avant/min/apres 6.383/4.954/3.197 m/s, d max 0.0695 m, cap max 2.024 deg, roulis/tangage max 2.023/2.023 deg, roues au sol min 4, sortie True, criteres OutOfBounds(OutsideEnvelope)

## NotSelectable (constructeur)

aucun
