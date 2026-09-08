# Story 1.6 -- Notes de checkpoint jouable Epic 1

## Resume

Le checkpoint Epic 1 vise un parcours local complet et relisible : `Bootstrap -> MainMenuLobby -> MVP_Run`.
Le joueur peut ouvrir le menu, entrer dans la coquille de lobby, modifier un reglage local, creer un profil, lancer `MVP_Run`, voir son personnage greybox local et se deplacer a pied dans la carte vide.

## Jouable

- Lancement depuis `Bootstrap`, routage automatique vers `MainMenuLobby`, puis entree dans la coquille via Play.
- Coquille de lobby locale avec difficulte modifiable, retour visible pour Create Lobby et Join By Code, et Start Game bloque tant qu'aucun profil n'est confirme.
- Setup personnage avec nom valide, selection de personnage greybox et depot de profil persistant pendant le changement de scene.
- Entree monde dans `MVP_Run` avec spawn local, camera attachee, marche, sprint et arret sur carte greybox.
- Placeholder de checkpoint visible dans le run : `Lobby : local hors ligne`, joueur confirme, et `HUD futur : rage, argent, actions passager`.

## Stubs

- Create Lobby ne cree aucune session Steam et publie seulement une notice d'indisponibilite.
- Join By Code ne tente aucun join reseau et publie seulement une notice d'indisponibilite.
- Les reglages de lobby restent locaux ; ils ne sont pas synchronises et ne pilotent pas encore l'etat de run.
- Le HUD de run ne contient encore ni rage, ni argent, ni actions passager actives.
- Les assets personnage, voiture et batiment restent greybox ; leur remplacement passe par le pipeline d'intake deja documente.

## Remplace par Epic 2

- Story 2.2 remplace Create Lobby par une room Steam privee creee par l'hote.
- Story 2.3 remplace Join By Code par un vrai chemin Lobby ID / invite Steam.
- Story 2.4 synchronise roster, ready state et reglages de partie.
- Story 2.5 remplace le spawn local par le spawn joueur reseau dans le monde vide.
- Story 2.6 transforme le placeholder `HUD futur : rage, argent, actions passager` en fondation HUD en jeu.
- Story 2.7 branche les retours visibles de deconnexion et de quit hote.

## Bloqueurs conserves

- VAL-028 reste a prouver avec un second joueur Steam reel.
- VAL-029 a VAL-032 restent bloques jusqu'au runtime lobby/session de l'Epic 2.
- VAL-013 a VAL-015 restent des preuves Steam runtime incompletes acceptees comme bloqueurs connus depuis le gate Epic 0.

## Verification

- EditMode Story 1.6 verifie l'ordre des scenes, le cablage menu/lobby/profil, la carte greybox, le HUD placeholder, les assets locaux et la note de passation.
- PlayMode Story 1.6 rejoue le parcours local complet depuis `Bootstrap` jusqu'au sprint dans `MVP_Run`, avec collecte des logs bloquants de type Error, Assert et Exception.
