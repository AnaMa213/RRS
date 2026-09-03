# Tutoriel Story 0.2 : Unity Hub, Unity Editor, projet et packages verrouilles

Ce tutoriel est le chemin lineaire unique pour executer la Story 0.2. Il couvre uniquement l'installation manuelle de Unity Hub, de Unity `6000.6.0f1`, la creation du projet Universal 3D/URP `RRS`, puis l'installation/verification des sept packages verrouilles. Toutes ces actions restent manuelles : l'agent ne peut ni les executer a ta place ni les marquer `Pass` sans preuve reelle que tu fournis.

Suis les etapes dans l'ordre. Ne saute aucune etape et ne change ni version, ni nom de projet, ni template, ni liste de packages sans approbation humaine explicite au prealable (voir "Limites & Contraintes" de la spec Story 0.2).

## Avant de commencer

- Vocabulaire de statut a utiliser partout : `Not Started`, `In Progress`, `Pass`, `Blocked`, `Not Applicable`.
- Chaque etape ci-dessous liste la preuve exacte a collecter. Sans cette preuve, la ligne correspondante dans `docs/setup/tooling-validation-log.md` (VAL-002 a VAL-011) et dans `docs/setup/epic-0-readiness-checklist.md` (lignes "0.2 Unity editor et projet" et "0.2 Packages verrouilles") reste `Not Started` ou `In Progress`.
- Si une version exacte est indisponible, marque la ligne concernee `Blocked`, note la source consultee et la version disponible dans `tooling-validation-log.md`, puis demande une approbation avant tout fallback. N'accepte jamais automatiquement une version differente.
- Ne cree pas toi-meme les dossiers/fichiers du projet Unity : c'est Unity Hub / Unity Editor qui les genere quand tu crees le projet a l'etape 3.

## Etape 1 -- Installer Unity Hub

1. Va sur la page officielle de telechargement Unity Hub et installe Unity Hub pour Windows.
2. Lance Unity Hub et connecte-toi avec ton compte Unity (Unity ID). Si tu n'as pas de compte, cree-en un gratuitement depuis Unity Hub.
3. Active une licence Unity si aucune n'est deja active (l'edition **Personal** suffit pour ce prototype solo) -- la creation de projet a l'Etape 3 est bloquee tant qu'aucune licence n'est active.
4. Verifie que Unity Hub s'ouvre correctement et affiche l'onglet **Installs**.

**Cas limite -- installation ou connexion impossible :** si l'installation de Unity Hub echoue ou si la connexion au compte Unity ne fonctionne pas (reseau restreint, erreur Unity ID), marque VAL-002 `Blocked`, note l'erreur exacte rencontree, et ne poursuis pas vers l'Etape 2 tant que ce n'est pas resolu.

**Preuve a fournir (VAL-002 -- Installation Unity Hub) :** une capture ou une note montrant la fenetre Unity Hub ouverte avec la version de Unity Hub visible (menu compte ou "About") et une licence active. Sans cette preuve, VAL-002 reste `Not Started`.

## Etape 2 -- Installer Unity `6000.6.0f1` sur la track Unity 6 Update

1. Dans Unity Hub, ouvre l'onglet **Installs** puis clique sur **Install Editor**.
2. Cherche precisement la version `6000.6.0f1` sur la release track **Unity 6 Update**.
   - Si `6000.6.0f1` apparait dans la liste : selectionne-la et lance l'installation.
   - Si `6000.6.0f1` n'apparait pas : ne choisis pas une autre version toi-meme. Marque la ligne VAL-003 `Blocked` dans `tooling-validation-log.md`, note la source consultee (ex. page Unity Hub Installs ou archive Unity) et la version la plus proche disponible, puis demande une approbation humaine avant tout changement de version.
3. Dans les modules d'installation, ajoute au minimum le module de build **Windows Build Support (IL2CPP)** si propose (utile pour la cible build de developpement Windows PC de l'Epic 0), sans installer de modules superflus non requis par le stack verrouille.
4. Laisse l'installation se terminer, puis retourne dans l'onglet **Installs** et confirme que `6000.6.0f1` apparait avec le tag Unity 6 Update.

**Preuve a fournir (VAL-003 -- Unity Editor) :** une capture ou une note montrant la ligne d'installation `6000.6.0f1` (avec le libelle de track Unity 6 Update) dans Unity Hub > Installs, plus le chemin local de l'editeur installe. Sans cette preuve, VAL-003 reste `Not Started`.

## Etape 3 -- Creer le projet Universal 3D/URP `RRS`

1. Dans Unity Hub, ouvre l'onglet **Projects** puis clique sur **New project**.
2. Selectionne l'editeur `6000.6.0f1` installe a l'etape 2.
3. Choisis le template **Universal 3D** (URP). Ne choisis pas 3D (Built-in), HDRP, ou tout autre template.
4. Nomme le projet exactement `RRS` (RoadRageSimulator). Choisis un emplacement local de ton choix, idealement un chemin court sans espaces/accents et **hors d'un dossier synchronise par OneDrive** (OneDrive casse frequemment Unity, Package Manager et Netcode en verrouillant ou en deplacant des fichiers pendant la compilation).
5. Clique sur **Create project** et attends que Unity Editor termine l'ouverture initiale du projet.
6. Une fois le projet ouvert, ferme Unity Editor proprement pour que tous les fichiers soient ecrits sur disque, puis verifie dans l'explorateur de fichiers que le dossier du projet contient bien `Assets/`, `Packages/`, `ProjectSettings/`, `Packages/manifest.json` et `Packages/packages-lock.json`.

**Cas limite -- creation echouee :** si aucun de ces dossiers n'existe apres l'etape 6 (Unity a affiche une erreur ou n'a jamais termine la creation), ne passe pas a l'Etape 4. Marque VAL-004 `Blocked`, note l'erreur Unity rencontree, et relance la creation depuis Unity Hub avant de continuer.

**Cas limite -- projet non conforme :** si `Assets/`, `Packages/` et `ProjectSettings/` existent mais que le nom du projet n'est pas `RRS` ou que le template n'est pas Universal 3D/URP, ne continue pas : corrige le projet (recree-le si besoin) avant de poursuivre. La validation agent restera `Blocked` tant que ce n'est pas corrige.

**Preuve a fournir (VAL-004 -- Creation projet Unity) :** une capture ou une note montrant le nom du projet, le template Universal 3D/URP, la version editeur `6000.6.0f1`, et le chemin local du projet, plus la confirmation que `Assets/`, `Packages/`, `ProjectSettings/`, `Packages/manifest.json` et `Packages/packages-lock.json` existent. Sans cette preuve, VAL-004 reste `Not Started`.

## Etape 4 -- Installer et verifier les sept packages verrouilles

Ouvre le projet `RRS` dans Unity Editor, puis va dans **Window > Package Manager**. Si un package verrouille n'apparait pas dans la recherche, ouvre les parametres de Package Manager (icone roue dentee) et active **Enable Pre-release Packages** / **Show preview packages** avant de reessayer -- plusieurs packages multiplayer sont distribues en preview. Installe ou confirme chacun des packages suivants, dans cet ordre, avec exactement ces versions verrouillees :

1. **Universal Render Pipeline** `17.6.0` (com.unity.render-pipelines.universal) -- generalement deja present via le template Universal 3D ; verifie la version exacte resolue.
2. **Netcode for GameObjects** `2.13.2` (com.unity.netcode.gameobjects).
3. **Transport Steamworks** (`com.community.netcode.transport.facepunch` ou `com.community.netcode.transport.steamnetworkingsockets`) -- package communautaire installe via **Add package from git URL** dans Package Manager, URL `https://github.com/Unity-Technologies/multiplayer-community-contributions.git?path=/Transports/<nom-du-transport>`. Remplace `com.unity.services.multiplayer` (retire par course-correction, voir `_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-02.md`). **Si `com.unity.services.multiplayer` `2.3.1` est deja installe dans ce projet, retire-le d'abord via Package Manager avant d'ajouter ce package.** Note le commit/tag Git exact utilise -- c'est ce commit/tag qui devient le verrou reproductible, pas un numero de version registry classique.
4. **Unity Transport** `6.6.0` (com.unity.transport) -- package builtin attendu avec Unity `6000.6.0f1`; rien a configurer a cette story : aucun `NetworkManager` n'existe encore dans le projet (il sera cree en Story 0.4/Epic 1). Verifie seulement que le package est installe avec la bonne version resolue.
5. **Multiplayer Play Mode** `3.0.0` (com.unity.multiplayer.playmode).
6. **Input System** `1.20.0` (com.unity.inputsystem).
7. **Cinemachine** `6.6.0` (com.unity.cinemachine) -- si deja embarque/active, confirme simplement la version resolue.

Pour chaque package :

1. Dans Package Manager, recherche le package par nom (ou via **Add package by name** avec l'id exact ci-dessus).
2. Installe la version exacte verrouillee si Unity la propose. Si Unity resout une version differente (plus recente ou plus ancienne imposee par la resolution de dependances), n'essaie pas de la masquer : note l'ecart explicitement.
3. Si un package n'apparait pas du tout ou que son installation echoue, ne le laisse pas de cote silencieusement : marque la ligne VAL correspondante `Blocked`, note l'erreur exacte, et n'installe pas de substitut non approuve.
4. Une fois tous les packages installes, ouvre `Packages/manifest.json` et `Packages/packages-lock.json` et verifie que chaque id de package et sa version resolue y apparaissent. Si les deux fichiers se contredisent, `packages-lock.json` fait foi car il reflete la resolution reelle effectuee par Unity.

**Cas limite -- ecart de version :** si `Packages/manifest.json` ou `Packages/packages-lock.json` montre une version resolue differente de la version verrouillee listee ci-dessus, ne marque pas la ligne correspondante `Pass`. Ajoute une note explicite du mismatch dans `docs/setup/tooling-validation-log.md` (colonne "Resultat / bloqueur" de la ligne VAL concernee) au lieu de le masquer, et demande une approbation avant de continuer si le mismatch doit devenir la nouvelle version verrouillee.

**Preuve a fournir (VAL-005 a VAL-011 -- un par package) :** pour chaque package, un extrait de `Packages/manifest.json` et/ou `Packages/packages-lock.json` montrant l'id du package et la version resolue. Sans cette preuve pour un package donne, la ligne VAL correspondante reste `Not Started`. Correspondance ligne VAL <-> package :
   - VAL-005 : Universal Render Pipeline `17.6.0`
   - VAL-006 : Netcode for GameObjects `2.13.2`
   - VAL-007 : Transport Steamworks (commit/tag Git exact installe)
   - VAL-008 : Unity Transport `6.6.0`
   - VAL-009 : Multiplayer Play Mode `3.0.0`
   - VAL-010 : Input System `1.20.0`
   - VAL-011 : Cinemachine `6.6.0`

## Apres l'etape 4 -- Mettre a jour les documents de suivi

Une fois que tu as reellement execute les etapes 1 a 4 et collecte les preuves listees ci-dessus :

1. Rapporte chaque preuve (capture, chemin, extrait `manifest.json`/`packages-lock.json`) a l'agent, ou colle-la directement (avec les secrets caviardes s'il y en a, meme si cette story n'en implique normalement pas) dans `docs/setup/tooling-validation-log.md` pour les lignes VAL-002 a VAL-011.
2. L'agent (ou toi-meme) met a jour uniquement les lignes VAL-002 a VAL-011 avec les champs reproductibles requis (date, responsable, commande/chemin UI, chemin/resume de preuve, validateur, resultat) et fait passer chaque ligne a `Pass` seulement si la preuve reelle correspond exactement a la version verrouillee, ou a `Blocked` avec le mismatch/blocage documente.
3. Met a jour les deux lignes Story 0.2 de `docs/setup/epic-0-readiness-checklist.md` ("0.2 Unity editor et projet" et "0.2 Packages verrouilles") en consequence, en actions manuelles utilisateur ET en validations agent.

## Arret obligatoire avant Story 0.3

**Arrete-toi ici.** Ne commence aucune tache de Story 0.3 (Steamworks, AppID de test, lobby, Networking Sockets, Lobby ID) avant d'avoir :

- termine les quatre etapes ci-dessus,
- collecte et rapporte toutes les preuves demandees pour VAL-002 a VAL-011,
- confirme que la checklist et le journal de validation refletent l'etat reel (des `Pass` uniquement la ou la preuve existe, sinon `Blocked` ou `Not Started`).

Story 0.3 depend explicitement du projet Unity et des packages multiplayer valides ici. Rapporte tes preuves et attends la confirmation de l'etat Story 0.2 avant de demarrer Story 0.3.
