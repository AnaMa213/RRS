# Story 5.31 - campagne Exploratory exploratory-20260930-093040

Campagne exploratoire : ne vaut pas acceptation.

## Conditions

- pas physique : 0.02 s ; Editeur en hote ; profils par defaut du prefab V2
- commit : 742c412d12f9d5f5f59f9f308af96488f28eb514 (refs/heads/systeme-traffic-ia-v2)
- RoadModelVersion : v4:e8dff9e54bff1712308899158ad16a9b ; budget de graines : 1 ; statut de campagne : Accepted
- epsilon_t : non declare
- modele M : tolerances 0.002 m / 0.05 deg ; reste de Lipschitz vise 0.001 m

- preuve relief 5.51 : empreinte physique 2e1677f8cf104ccba47016a51651a93d7974d500e0b3e3cbaf0c46b4eff530a7 ; chaussées identifiées 29 ; fenetre contact ±50 pas = 1 s

## Resultat

9 vehicule(s), campagne complete True, cale False, pas totaux 40718, d max au pas 6.5427 m, borne entre deux pas max (M verifie) 6.6246 m, intervalles 40695 dont ModelNotVerified 0, pas v > v* 72, couple negatif en repli 0, contacts 0, elements Measured 95 / NotMeasured 69 / NotSelectable 0, NominalPoseInfeasible 0 pas (pose nominale cinematique, contrat §8), dernier code aucun

## Runs

| run | fin | pas | replanifications | replis | d max au pas (m) | temps par etape |
|---|---|---|---|---|---|---|
| 0 | sortie | 3564 | 3 | 46 | 0.82251 | steps 3564 / frame+localisation mean 0.125 ms max 0.411 / route+horizon+mouvement mean 0.57 ms max 11.351 / plan de vitesse mean 0.835 ms max 6.157 / commande+composition mean 0.039 ms max 0.28 |
| 1 | sortie | 3759 | 3 | 53 | 6.542675 | steps 3759 / frame+localisation mean 0.128 ms max 9.596 / route+horizon+mouvement mean 0.59 ms max 11.496 / plan de vitesse mean 0.907 ms max 9.486 / commande+composition mean 0.039 ms max 0.096 |
| 2 | sortie | 2736 | 2 | 1 | 0.614781 | steps 2736 / frame+localisation mean 0.121 ms max 3.269 / route+horizon+mouvement mean 0.702 ms max 9.715 / plan de vitesse mean 1.11 ms max 10.696 / commande+composition mean 0.041 ms max 0.094 |
| 3 | sortie | 4545 | 3 | 37 | 0.881052 | steps 4545 / frame+localisation mean 0.12 ms max 0.298 / route+horizon+mouvement mean 1.073 ms max 17.853 / plan de vitesse mean 1.738 ms max 11.598 / commande+composition mean 0.042 ms max 0.118 |
| 4 | sortie | 5670 | 4 | 63 | 0.855406 | steps 5670 / frame+localisation mean 0.131 ms max 8.58 / route+horizon+mouvement mean 1.118 ms max 17.631 / plan de vitesse mean 1.815 ms max 13.442 / commande+composition mean 0.041 ms max 0.117 |
| 5 | sortie | 4545 | 3 | 37 | 0.881052 | steps 4545 / frame+localisation mean 0.12 ms max 0.328 / route+horizon+mouvement mean 1.071 ms max 17.875 / plan de vitesse mean 1.732 ms max 10.49 / commande+composition mean 0.042 ms max 0.119 |
| 6 | sortie | 5670 | 4 | 63 | 0.855406 | steps 5670 / frame+localisation mean 0.129 ms max 3.272 / route+horizon+mouvement mean 1.108 ms max 17.095 / plan de vitesse mean 1.809 ms max 13.175 / commande+composition mean 0.041 ms max 0.124 |
| 7 | sortie | 4545 | 3 | 37 | 0.881052 | steps 4545 / frame+localisation mean 0.121 ms max 3.266 / route+horizon+mouvement mean 1.073 ms max 33.891 / plan de vitesse mean 1.737 ms max 11.244 / commande+composition mean 0.042 ms max 0.121 |
| 8 | sortie | 5670 | 4 | 63 | 0.855406 | steps 5670 / frame+localisation mean 0.13 ms max 3.23 / route+horizon+mouvement mean 1.109 ms max 16.219 / plan de vitesse mean 1.818 ms max 14.275 / commande+composition mean 0.041 ms max 0.119 |

## Contacts

aucun

## Contacts bloquants ou consequences

aucun

## NotSelectable (constructeur)

aucun
