# Story 5.31 - campagne Exploratory exploratory-20260929-141746

Campagne exploratoire : ne vaut pas acceptation.

## Conditions

- pas physique : 0.02 s ; Editeur en hote ; profils par defaut du prefab V2
- commit : e970735a19faa78a720ebde45b781e2fdb5f7cb8 (refs/heads/systeme-traffic-ia-v2)
- RoadModelVersion : v4:e8dff9e54bff1712308899158ad16a9b ; budget de graines : 32 ; statut de campagne : Refused
- epsilon_t : non declare
- modele M : tolerances 0.002 m / 0.05 deg ; reste de Lipschitz vise 0.001 m

## Resultat

12 vehicule(s), campagne complete True, cale False, pas totaux 30029, d max au pas 0.9961 m, borne entre deux pas max (M verifie) 0.9971 m, intervalles 30000 dont ModelNotVerified 0, pas v > v* 0, couple negatif en repli 0, contacts 2, elements Measured 144 / NotMeasured 0 / NotSelectable 20

## Runs

| run | fin | pas | replanifications | replis | d max au pas (m) | temps par etape |
|---|---|---|---|---|---|---|
| 0 | sortie | 2448 | 0 | 1 | 0.922687 | steps 2448 / frame+localisation mean 0.109 ms max 0.396 / route+horizon+mouvement mean 0.637 ms max 13.944 / plan de vitesse mean 0.927 ms max 10.089 / commande+composition mean 0.013 ms max 1.62 |
| 1 | sortie | 2856 | 0 | 1 | 0.99608 | steps 2856 / frame+localisation mean 0.12 ms max 0.394 / route+horizon+mouvement mean 0.763 ms max 13.334 / plan de vitesse mean 1.108 ms max 10.024 / commande+composition mean 0.012 ms max 0.068 |
| 2 | sortie | 2861 | 0 | 1 | 0.99593 | steps 2861 / frame+localisation mean 0.119 ms max 0.321 / route+horizon+mouvement mean 0.752 ms max 15.413 / plan de vitesse mean 1.101 ms max 9.189 / commande+composition mean 0.012 ms max 0.045 |
| 3 | sortie | 2856 | 0 | 1 | 0.995983 | steps 2856 / frame+localisation mean 0.122 ms max 9.268 / route+horizon+mouvement mean 0.747 ms max 13.255 / plan de vitesse mean 1.115 ms max 9.623 / commande+composition mean 0.012 ms max 0.054 |
| 4 | sortie | 2845 | 0 | 1 | 0.996061 | steps 2845 / frame+localisation mean 0.119 ms max 0.33 / route+horizon+mouvement mean 0.728 ms max 12.207 / plan de vitesse mean 1.093 ms max 7.226 / commande+composition mean 0.012 ms max 0.069 |
| 5 | sortie | 2430 | 0 | 1 | 0.92014 | steps 2430 / frame+localisation mean 0.112 ms max 9.021 / route+horizon+mouvement mean 0.61 ms max 11.744 / plan de vitesse mean 0.918 ms max 5.261 / commande+composition mean 0.012 ms max 0.067 |
| 6 | sortie | 2442 | 0 | 1 | 0.919057 | steps 2442 / frame+localisation mean 0.109 ms max 0.292 / route+horizon+mouvement mean 0.597 ms max 12.29 / plan de vitesse mean 0.917 ms max 6.884 / commande+composition mean 0.012 ms max 0.073 |
| 7 | sortie | 2444 | 0 | 1 | 0.92014 | steps 2444 / frame+localisation mean 0.108 ms max 0.326 / route+horizon+mouvement mean 0.602 ms max 11.554 / plan de vitesse mean 0.918 ms max 4.894 / commande+composition mean 0.012 ms max 0.061 |
| 8 | sortie | 2205 | 0 | 1 | 0.917629 | steps 2205 / frame+localisation mean 0.105 ms max 0.278 / route+horizon+mouvement mean 0.518 ms max 9.45 / plan de vitesse mean 0.791 ms max 4.876 / commande+composition mean 0.012 ms max 0.068 |
| 9 | sortie | 2210 | 0 | 1 | 0.919057 | steps 2210 / frame+localisation mean 0.105 ms max 0.247 / route+horizon+mouvement mean 0.524 ms max 9.48 / plan de vitesse mean 0.797 ms max 5.018 / commande+composition mean 0.012 ms max 0.074 |
| 10 | sortie | 2206 | 0 | 1 | 0.919115 | steps 2206 / frame+localisation mean 0.105 ms max 0.215 / route+horizon+mouvement mean 0.527 ms max 11.989 / plan de vitesse mean 0.789 ms max 7.635 / commande+composition mean 0.012 ms max 0.036 |
| 11 | sortie | 2209 | 0 | 1 | 0.919115 | steps 2209 / frame+localisation mean 0.107 ms max 3.349 / route+horizon+mouvement mean 0.533 ms max 14.49 / plan de vitesse mean 0.797 ms max 10.044 / commande+composition mean 0.012 ms max 0.071 |

## Contacts

1:1365:Rampe_Ouest
1:1365:Rampe_Est

## NotSelectable (constructeur)

m:40518565227156c444c481089e3df481
m:410eb197d612c9963b40e6d1b7fe15bf
m:414c99412444a4a56b97dd36be51b593
m:455bb22fbae5a3833e564372b5f1d7a0
m:4562eaa3091e578cd4e9524af6f7908b
m:47084b07a52e936a1f5156ba93413d99
m:47c7c84e9d1de0910268c837613d11b7
m:4a16b65f562a0a5d8a44e286ca088493
m:4a61888b6162623b5f3b239c6967a1b7
m:4a7f00857442da3e6f4487e676ef108d
m:4aa676c5e3857524d5a9b386be4bdb90
m:4ac23ecbf80f11d7feda468178247ab4
m:4c6997b98ac692f6bd24a50f86009eb3
m:4d2aea792f643724e2d9f9ed853eb1a1
m:4d5388e3c39d025ecfc72f8aa302c389
m:4d54e6de5a6bbf4d1eb20f8ed5a40cb2
m:4d83ee0977d55ec7ebd2f1cbcba06bb9
m:4dc4e81b4c9cdd45e0484d2e9daeb888
m:4dfcc1af030746a1a7e376f814e03a81
m:4f7e560ced3c5a4bfbf14942df76f198
