# Recette 5.39 -- rattachement physique apres deplacement (execution a la Gate D)

Story 5.39, superviseur de recuperation. Cette recette est **ecrite** par la story et **executee a la Gate D**, dans la meme session que la recette 5.38 (poussee et choc). Aucun resultat n'est revendique ici.

Elle couvre `ANO-5.10-03` AC5 a AC7 : rattachement sans teleportation, reprise d'une route valide, et possibilite de quitter temporairement la chaussee.

## Preconditions

- Scene `Assets/RoadRage/App/Scenes/MVP_Run.unity`, session hote, trafic V2 actif. Garde d'etat `AGENTS.md` avant et apres.
- Lecture cote hote du driver V2 concerne :
  - `TrafficV2VehicleDriver.Recovery.ToText()` : cause, tentatives de l'episode, progression attendue E et reelle A, derniere tentative ;
  - `Recovery.Attempts` : version, cause, manoeuvre, reponse de la tactique et fin de chaque tentative ;
  - `Tactical.ToText()` : but `Recovery`, manoeuvre, parcours, stabilite ;
  - `Lifecycle` : `Active` ou `Faulted`, et `LastComposed.Reason`.
- Les lignes `Tactical` et `Recovery` de `LastProjection.ToText()` publient l'etat tant qu'il existe.

## Scenario A -- choc qui sort une IA de la chaussee

1. Laisser une IA V2 circuler en ligne droite, pres d'un trottoir.
2. Avec le vehicule joueur, la percuter de flanc a 10-15 m/s, assez fort pour la deporter sur le trottoir ou au-dela.
3. Observer et consigner :
   - la sortie de chaussee est **subie**, sans freinage artificiel du deport ni recentrage force (AC7) ;
   - le but de collision passe en `AwaitingRecovery`, puis la cause `Displaced` donne un `Realign` accepte ;
   - le retour : vitesse au plus 2 m/s, angle de la loi de suivi, franchissement de bordure physique ;
   - la fin `Resumed` : d <= epsilon_t et stabilite C6 tenue 0,5 s ;
   - la suite : replanification eventuelle comptee, puis sortie normale par un portail (AC6, V1-E02).

## Scenario B -- IA encastree ou calee

1. Avec le vehicule joueur, coincer une IA contre une autre IA ou un obstacle statique, nez au contact.
2. Attendu :
   - aucune eligibilite tant qu'un blocker legitime explique l'attente (leader a jeu normal, grant refuse, pieton) ;
   - `ProgressDeficit` des que la progression attendue atteint 2 m sans 0,5 m d'avance reelle, sans blocker legitime ;
   - premiere manoeuvre `Reverse` : roues droites, au plus 1 m/s, 3 m, jamais de gaz, refus `RearBlocked` si un acteur est derriere ;
   - puis alternance avec `Realign`.

## Scenario C -- aucune manoeuvre possible

1. Enfermer une IA de facon qu'elle ne puisse ni reculer ni avancer (acteurs devant et derriere, ou mur).
2. Attendu :
   - les refus et les echecs (`RearBlocked`, `Stalled`, `NoProgress`) sont consignes dans l'historique, sans resoumission identique ;
   - au plus 4 tentatives par episode, ou 2 refus consecutifs, puis `Faulted` ;
   - en `Faulted` : repli tenu, raison `Faulted`, vehicule **present**, ni retire, ni deplace, ni reinsere.

## Criteres de refus

- Saut de position ou de cap d'un pas a l'autre sans cause physique (teleportation, reinsertion).
- Despawn hors d'un portail de sortie, y compris en `Faulted`.
- Recuperation eligible pendant une attente legitime, ou sur la seule vitesse basse.
- Requete identique resoumise a chaque pas apres un refus.
- Gaz pendant un recul, ou vitesse de manoeuvre au-dela de 2 m/s en avant ou 1 m/s en arriere.

## Calibration

Les seuils R2, R4 et R5 de la spec 5.39 (decision D1) sont des valeurs declarees : 2 m et 0,5 m de progression, 4 tentatives, 2 refus, 2 m/s et 20 m pour le realignement, 1 m/s et 3 m pour le recul. Consigner pour chaque episode :

- la cause et la duree jusqu'a l'eligibilite ;
- les manoeuvres et leur parcours ;
- l'ecart d au `Resumed` ;
- les contacts subis pendant la manoeuvre.

Un seuil qui classe mal un cas observe se revise par **decision proprietaire**, sans retouche silencieuse. Limite connue (D5) : le balayage arriere ne voit que les acteurs de la frame, pas un obstacle statique.
