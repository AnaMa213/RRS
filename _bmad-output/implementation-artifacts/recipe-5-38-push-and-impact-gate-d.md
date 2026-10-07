# Recette 5.38 -- poussee et choc (execution a la Gate D)

Story 5.38, reponse aux collisions. Cette recette est **ecrite** par la story et **executee a la Gate D**. Aucun resultat n'est revendique ici.

Elle couvre le cas de poussee nomme par la recette du 2026-09-18 (`ANO-5.10-03`, complement « POUSSEE »). Ce cas n'est pas couvert par les AC1-AC8 de l'anomalie, ecrits autour d'une collision.

## Preconditions

- Scene `Assets/RoadRage/App/Scenes/MVP_Run.unity`, session hote, trafic V2 actif. Garde d'etat `AGENTS.md` avant et apres.
- Lecture cote hote du driver V2 heurte :
  - `TrafficV2VehicleDriver.Tactical.ToText()` : but, raison, reaction, phase et cause de stabilite ;
  - `LastCollisionFacts` et `LastCollisionRequest`, puis `LastComposed.Intent`.
- La ligne `Tactical` de `LastProjection.ToText()` publie le but tant qu'il est actif.

## Scenario A -- un joueur percute une IA

1. Laisser une IA V2 circuler en ligne droite.
2. Avec le vehicule joueur, la percuter de flanc a 8-12 m/s, puis a l'arriere a la meme vitesse.
3. Observer et consigner, pour chaque choc :
   - le pas du choc, la significativite (`RelativeSpeed`, `DeltaV` ou `SustainedPush`) et la reponse `Accepted` ;
   - la reaction tiree (`Brake`, `Evade` ou `MisReact`) ;
   - `Intent.Throttle` a chaque pas du but : il doit valoir **0** ;
   - la trajectoire : le vehicule est devie, peut se mettre en travers et ne se realigne **pas** en un pas ;
   - la fin du but : `Resumed`, seulement stable et dans epsilon_t, ou `AwaitingRecovery`, frein maintenu.

## Scenario B -- une IA en percute une autre

1. Utiliser le jeton de scenario de test (population <= 8) pour obtenir deux IA V2 en conflit.
2. Provoquer un choc entre elles. Exemple : une IA arretee par un `StopHold` est heurtee par une IA sur la meme file, avec la perception masquee si necessaire.
3. Consigner les memes points que pour A, **pour les deux vehicules**.

## Scenario C -- poussee continue sans choc

1. Avec le vehicule joueur, pousser lentement une IA arretee (< 3 m/s, contact maintenu plus d'une seconde).
2. Attendu :
   - `SustainedPush` des que l'IA tourne (lacet > 0,3 rad/s), perd une roue au sol ou depasse epsilon_t ;
   - aucune propulsion artificielle ;
   - aucun recentrage sur la voie : l'IA reste ou la physique la laisse.

## Criteres de refus

- Gaz non nul pendant un but de collision.
- Pose ou vitesse ecrite par le code : saut de position ou de cap d'un pas a l'autre sans contact.
- Realignement en un pas, ou reprise `Resumed` avec d > epsilon_t.
- Contact leger de file, ou bordure franchie, classe significatif.

## Calibration

Les seuils C2/C6 de la spec 5.38 (decision D1) sont des valeurs declarees. Consigner, pour chaque choc :

- la vitesse normale relative ;
- le delta-v ;
- la duree de contact ;
- le lacet maximal ;
- la duree jusqu'a la stabilite.

Un seuil qui classe mal un cas observe se revise par **decision proprietaire**, sans retouche silencieuse.
