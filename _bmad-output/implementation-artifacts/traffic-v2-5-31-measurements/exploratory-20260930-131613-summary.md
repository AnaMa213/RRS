# Story 5.31 - campagne Exploratory exploratory-20260930-131613

Campagne exploratoire : ne vaut pas acceptation.

## Conditions

- pas physique : 0.02 s ; Editeur en hote ; profils par defaut du prefab V2
- commit : 742c412d12f9d5f5f59f9f308af96488f28eb514 (refs/heads/systeme-traffic-ia-v2)
- RoadModelVersion : v4:e8dff9e54bff1712308899158ad16a9b ; budget de graines : 1 ; statut de campagne : Accepted
- epsilon_t : non declare
- modele M : tolerances 0.002 m / 0.05 deg ; reste de Lipschitz vise 0.001 m

- preuve relief 5.51 : empreinte physique 2e1677f8cf104ccba47016a51651a93d7974d500e0b3e3cbaf0c46b4eff530a7 ; chaussées identifiées 29 ; fenetre contact ±50 pas = 1 s

## Resultat

4 vehicule(s), campagne complete True, cale False, pas totaux 11743, d max au pas 0.3284 m, borne entre deux pas max (M verifie) 0.3294 m, intervalles 11732 dont ModelNotVerified 0, pas v > v* 0, couple negatif en repli 0, contacts 4, elements Measured 93 / NotMeasured 71 / NotSelectable 0, NominalPoseInfeasible 0 pas (pose nominale cinematique, contrat §8), dernier code aucun

## Runs

| run | fin | pas | replanifications | replis | d max au pas (m) | temps par etape |
|---|---|---|---|---|---|---|
| 0 | sortie | 2177 | 0 | 1 | 0.328443 | steps 2177 / frame+localisation mean 0.137 ms max 3.448 / route+horizon+mouvement mean 1.061 ms max 8.708 / plan de vitesse mean 1.221 ms max 7.868 / commande+composition mean 0.038 ms max 1.873 |
| 1 | sortie | 2977 | 0 | 1 | 0.328223 | steps 2977 / frame+localisation mean 0.14 ms max 0.346 / route+horizon+mouvement mean 1.522 ms max 9.899 / plan de vitesse mean 1.824 ms max 11.454 / commande+composition mean 0.039 ms max 0.063 |
| 2 | sortie | 2978 | 0 | 1 | 0.327412 | steps 2978 / frame+localisation mean 0.14 ms max 0.354 / route+horizon+mouvement mean 1.426 ms max 15.161 / plan de vitesse mean 1.682 ms max 10.884 / commande+composition mean 0.039 ms max 0.063 |
| 3 | sortie | 3604 | 0 | 1 | 0.327133 | steps 3604 / frame+localisation mean 0.149 ms max 0.434 / route+horizon+mouvement mean 1.671 ms max 14.567 / plan de vitesse mean 2.009 ms max 12.056 / commande+composition mean 0.04 ms max 0.093 |

## Triplets

- run 0 : 4882b42dcf37410f4f629825c30f5f99 -> 49d29d3fd771ae4966aca755b3442cbc ; via 410eb197d612c9963b40e6d1b7fe15bf ; graine 0
- run 1 : 4882b42dcf37410f4f629825c30f5f99 -> 48090a5d359625c04fb6050a2407f292 ; via 414c99412444a4a56b97dd36be51b593 ; graine 0
- run 2 : 4882b42dcf37410f4f629825c30f5f99 -> 48090a5d359625c04fb6050a2407f292 ; via 4dc4e81b4c9cdd45e0484d2e9daeb888 ; graine 0
- run 3 : 4882b42dcf37410f4f629825c30f5f99 -> 4dcf9641f7a714b8bc785696d1709798 ; via 453f130c460dc35e052c30714bec6c8e ; graine 0

## Contacts

run 1: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 1333-1335, impulsion 0 N.s, v_n 0.153 m/s, v avant/min/apres 4.364/5.433/6.25 m/s, d max 0.1376 m, cap max 2.175 deg, roulis/tangage max 2.102/2.102 deg, roues au sol min 4, sortie True, criteres aucun
run 1: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 1333-1335, impulsion 0 N.s, v_n 0.153 m/s, v avant/min/apres 4.364/5.433/6.25 m/s, d max 0.1376 m, cap max 2.175 deg, roulis/tangage max 2.102/2.102 deg, roues au sol min 4, sortie True, criteres aucun
run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 2559-2564, impulsion 0 N.s, v_n 0.369 m/s, v avant/min/apres 6.35/5.913/4.397 m/s, d max 0.0743 m, cap max 2.039 deg, roulis/tangage max 2.039/2.039 deg, roues au sol min 4, sortie True, criteres aucun
run 3: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 2559-2564, impulsion 0 N.s, v_n 0.369 m/s, v avant/min/apres 6.35/5.913/4.397 m/s, d max 0.0743 m, cap max 2.039 deg, roulis/tangage max 2.039/2.039 deg, roues au sol min 4, sortie True, criteres aucun

## Contacts bloquants ou consequences

aucun

## NotSelectable (constructeur)

aucun
