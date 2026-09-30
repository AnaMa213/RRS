# Story 5.31 - campagne Exploratory exploratory-20260930-133332

Campagne exploratoire : ne vaut pas acceptation.

## Conditions

- pas physique : 0.02 s ; Editeur en hote ; profils par defaut du prefab V2
- commit : 742c412d12f9d5f5f59f9f308af96488f28eb514 (refs/heads/systeme-traffic-ia-v2)
- RoadModelVersion : v4:e8dff9e54bff1712308899158ad16a9b ; budget de graines : 1 ; statut de campagne : Accepted
- epsilon_t : non declare
- modele M : tolerances 0.002 m / 0.05 deg ; reste de Lipschitz vise 0.001 m

- preuve relief 5.51 : empreinte physique 2e1677f8cf104ccba47016a51651a93d7974d500e0b3e3cbaf0c46b4eff530a7 ; chaussées identifiées 29 ; fenetre contact ±50 pas = 1 s

## Resultat

3 vehicule(s), campagne complete True, cale False, pas totaux 8635, d max au pas 0.3276 m, borne entre deux pas max (M verifie) 0.3286 m, intervalles 8627 dont ModelNotVerified 0, pas v > v* 0, couple negatif en repli 0, contacts 2, elements Measured 79 / NotMeasured 85 / NotSelectable 0, NominalPoseInfeasible 0 pas (pose nominale cinematique, contrat §8), dernier code aucun

## Runs

| run | fin | pas | replanifications | replis | d max au pas (m) | temps par etape |
|---|---|---|---|---|---|---|
| 0 | sortie | 2048 | 0 | 1 | 0.196526 | steps 2048 / frame+localisation mean 0.133 ms max 0.917 / route+horizon+mouvement mean 1.107 ms max 9.02 / plan de vitesse mean 1.27 ms max 10.076 / commande+composition mean 0.034 ms max 1.798 |
| 1 | sortie | 2978 | 0 | 1 | 0.327608 | steps 2978 / frame+localisation mean 0.146 ms max 3.17 / route+horizon+mouvement mean 1.541 ms max 16.705 / plan de vitesse mean 1.745 ms max 13.413 / commande+composition mean 0.04 ms max 0.106 |
| 2 | sortie | 3604 | 0 | 1 | 0.327127 | steps 3604 / frame+localisation mean 0.154 ms max 3.439 / route+horizon+mouvement mean 1.789 ms max 15.049 / plan de vitesse mean 2.091 ms max 11.773 / commande+composition mean 0.041 ms max 0.153 |

## Triplets

- run 0 : 4882b42dcf37410f4f629825c30f5f99 -> 436d9a1a58c93bd19cdf0014de309f98 ; via 4aa676c5e3857524d5a9b386be4bdb90 ; graine 0
- run 1 : 4882b42dcf37410f4f629825c30f5f99 -> 48090a5d359625c04fb6050a2407f292 ; via 4dc4e81b4c9cdd45e0484d2e9daeb888 ; graine 0
- run 2 : 4882b42dcf37410f4f629825c30f5f99 -> 4dcf9641f7a714b8bc785696d1709798 ; via 453f130c460dc35e052c30714bec6c8e ; graine 0

## Contacts

run 2: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Est [DrivableRelief] pas 2559-2564, impulsion 0 N.s, v_n 0.369 m/s, v avant/min/apres 6.35/5.913/4.397 m/s, d max 0.0743 m, cap max 2.039 deg, roulis/tangage max 2.039/2.039 deg, roues au sol min 4, sortie True, criteres aucun
run 2: MVP_Run/RunRoot/GreyboxMap/Relief_DosDane_AvenueCenterToEast/Rampe_Ouest [DrivableRelief] pas 2559-2564, impulsion 0 N.s, v_n 0.369 m/s, v avant/min/apres 6.35/5.913/4.397 m/s, d max 0.0743 m, cap max 2.039 deg, roulis/tangage max 2.039/2.039 deg, roues au sol min 4, sortie True, criteres aucun

## Contacts bloquants ou consequences

aucun

## NotSelectable (constructeur)

aucun
