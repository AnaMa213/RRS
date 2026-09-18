# Story 5.9 - Notes de verification du modele de conduite parametre (IDM/MOBIL)

Date: 2026-09-15

## Ce que la story a change

- `ResolveCruiseSpeedMultiplier` et le champ serialise `cruiseSpeed` ont disparu du controleur IA. Le style de conduite vit maintenant dans un profil authore de onze parametres (`DriverProfile`), porte par un asset (`DriverProfileDef_Default.asset`) et consomme par des fonctions pures (`DriverModel`).
- `NetworkedAIVehicleDriverController` est devenu un integrateur : il lit le profil, le module par la disposition publiee, detecte un leader devant lui, integre l'acceleration IDM lissee par le temps de reaction, et cadence l'evaluation de changement de voie sur l'intervalle du profil.

## Valeurs authorees et leur provenance

Asset : `Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset` (id `driver_default`).

| Parametre | Valeur | Provenance |
| --- | --- | --- |
| `desiredSpeed` (v0) | 8 m/s | Reprise a l'identique de l'ancien `cruiseSpeed` du controleur, pour ne pas changer le ressenti de la Story 5.2 en meme temps que le mecanisme. |
| `timeHeadway` (T) | 1.5 s | Valeur de base de l'IDM citee par la recherche (carte des leviers, ligne `T`). |
| `minimumGap` (s0) | 2 m | Ordre de grandeur usuel de l'IDM, pas une mesure. |
| `maxAcceleration` (a) | 1.5 m/s2 | Ordre de grandeur usuel de l'IDM. |
| `comfortableDeceleration` (b) | 2 m/s2 | Ordre de grandeur usuel de l'IDM. |
| `politeness` (p) | 0.25 | Bande "legerement cooperatif" de MOBIL ; le cadran de malveillance (`p < 0`) est atteint par la modulation de rage, pas par l'authoring de base. |
| `laneChangeThreshold` (a_th) | 0.2 m/s2 | Valeur nommee par la recherche (carte des leviers, ligne `a_th`). |
| `safeBrakingLimit` (b_safe) | 4 m/s2 | Valeur nommee par la recherche (~4 m/s2), loin de la limite physique ~9. |
| `reactionTime` | 0.3 s | Jugement. Aucun equivalent direct dans les sources ; le lissage de premier ordre est le mecanisme choisi par la spec (pas de tampon d'historique). |
| `laneChangeEvaluationInterval` | 1 s | Gating a 1 s avec minuteurs desynchronises, ce qui rend MOBIL bon marche (recherche, D3/D4). |
| `consistency` | 0.8 | Jugement. Aucune source ; regle pour un flottement discret par defaut. |

**Ces valeurs sont un point de depart, pas une calibration.** La recommandation R3 de la recherche est explicite : la table de highway-env ne doit pas etre copiee telle quelle (ses archetypes n'exercent jamais la politesse et relevent tous deux `T` a 2.5 s). Aucun reglage par mesure n'a ete fait dans cette story ; le reglage se fera au playtest, sur cet asset unique.

## Ecarts assumes

- **Enveloppe de bruit partagee.** L'amplitude (+/-6 % de v0) et les deux frequences du bruit de vitesse desiree sont des constantes de `DriverModel`, pas des champs du `Def` : seul `consistency` varie par profil pour cette story. Marque `ponytail:` dans le code, avec le chemin de sortie (en faire des champs du `Def` si un playtest reclame un archetype au tremblement plus large).
- **Detection de leader provisoire.** Un seul rayon vers l'avant, portee derivee du profil (`(s0 + v.T) * 2`), et seuls les corps porteurs d'un `Rigidbody` comptent comme leader -- la geometrie statique du decor n'en est pas un. La Story 5.17 (ex-5.12) remplace cette detection par la perception elargie.
- **MOBIL sans candidats.** Le predicat `TryEvaluateLaneChange` et le minuteur cadence par `laneChangeEvaluationInterval` sont livres, mais aucune voie candidate n'existe tant que la Story 5.10 n'a pas livre le graphe de voies. Le minuteur tourne donc sur une liste vide, comme prevu par la spec.
- **Garde-fou de finitude.** L'IDM divise par l'ecart : celui-ci est clampe a 0.05 m avant division, et l'acceleration renvoyee est bornee a -20 m/s2. Ce n'est pas un reglage de conduite mais une protection : sans elle, un ecart nul produit `-Inf` puis un `linearVelocity` NaN qui contamine le `NetworkTransform`.
- **Modulation par disposition, pas par jauge.** `ResolveEffectiveProfile` applique une intensite discrete par disposition (Calm 0, Irritated 0.25, Flee 0.6, Ram 0.9 ; Block et ConfrontationCapable mettent v0 a 0). L'escalade de vitesse de la Story 5.4 est reproduite a l'identique (x1, x1.25, x1.6, x1.9). La Story 5.19 (ex-5.13) remplacera cette entree discrete par les jauges continues rage/peur, entre la modulation et le bruit de personnalite.

## Verification

- Suite EditMode, filtre `Story59ParameterizedDriverModelTests` : couvre toute la matrice d'E/S de la spec (route libre, vitesse atteinte, leader a l'arret, ecart nul/negatif/NaN, veto de securite MOBIL, politesse negative, gating et desynchronisation, dispositions immobilisantes, lissage sans depassement, plancher de `reactionTime`, bruit nul a `consistency = 1`, enveloppe a `consistency = 0`, determinisme), plus les gardes d'authoring et de source.
- Gardes existantes mises a jour : `Story52BasicAiRouteFollowingAndRecoveryTests` (compte de membres statiques 4 -> 3, les fonctions pures ayant migre vers `DriverModel`) et `Story54RageDrivenAiBehaviorStatesTests` (les trois tests de profil sont reexprimes sur le profil effectif ; les proprietes de la Story 5.4 sont conservees et etendues aux autres leviers).
- Suite PlayMode, filtre `Story59ParameterizedDriverModelPlayModeTests` : dans `MVP_Run`, les trois vehicules IA portent le profil authore, avancent le long de leur route sous le nouveau modele, et aucune vitesse NaN/Inf n'apparait pendant 4 s de simulation.
- Aucune commande CLI : le mode batch refuse de s'executer tant que l'Editeur detient le verrou du projet (meme contrainte qu'aux Stories 5.5 a 5.8). Verification par le Test Runner de l'Editeur, checkpoint humain.

## Verification manuelle restante

- Lancer `MVP_Run` en Play Mode et confirmer visuellement que les vehicules demarrent progressivement (integration d'acceleration, plus de vitesse instantanee), ralentissent derriere un obstacle mobile place devant eux, et que la Console ne montre ni `NaN` ni `Inf`.
- Inspecteur du prefab `Greybox_AIVehicle` : le champ `Driver Profile` pointe `DriverProfileDef_Default`, et aucun champ `Cruise Speed` residuel n'est affiche.
