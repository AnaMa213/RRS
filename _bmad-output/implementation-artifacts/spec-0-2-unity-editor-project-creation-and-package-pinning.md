---
title: 'Story 0.2 : Installation Unity Editor, creation du projet et verrouillage des packages'
type: 'chore'
created: '2026-09-02'
status: 'done'
review_loop_iteration: 0
baseline_commit: 'NO_VCS'
context:
  - '{project-root}/_bmad-output/implementation-artifacts/epic-0-context.md'
---

<frozen-after-approval reason="intention controlee par l'humain - ne pas modifier sauf renegociation explicite">

## Intention

**Probleme :** Aucun dossier Unity (`Assets/`, `Packages/`, `ProjectSettings/`) n'existe encore dans le depot. Les documents de suivi crees en Story 0.1 renvoient tous vers une installation manuelle Unity encore non guidee pas-a-pas, et aucune preuve reproductible n'existe pour les packages verrouilles requis par l'architecture.

**Approche :** Creer un tutoriel pas-a-pas `docs/setup/story-0-2-unity-install-tutorial.md` couvrant Unity Hub, Unity `6000.6.0f1` sur la track Unity 6 Update, la creation du projet Universal 3D/URP `RRS`, puis l'installation/verification des sept packages verrouilles. Mettre a jour les lignes Story 0.2 de la checklist et du journal de validation pour pointer vers ce tutoriel, sans jamais fabriquer de preuve d'installation.

## Limites & Contraintes

**Toujours :** Garder l'installation Unity Hub, l'installation Unity Editor, la creation du projet et l'installation des packages comme actions manuelles utilisateur ; utiliser le vocabulaire de statut partage `Not Started`, `In Progress`, `Pass`, `Blocked`, `Not Applicable` ; exiger une preuve concrete (capture, chemin, extrait `manifest.json`/`packages-lock.json`) avant de marquer une ligne `Pass` ; preserver exactement les versions verrouillees : Unity `6000.6.0f1` (Unity 6 Update), projet `RRS` (RoadRageSimulator), template Universal 3D/URP, Netcode for GameObjects `2.13.2`, **transport Steamworks communautaire (`com.community.netcode.transport.facepunch` ou `.steamnetworkingsockets`, commit/tag exact epingle a l'installation reelle)**, Unity Transport `6.6.0`, Universal Render Pipeline `17.6.0`, Multiplayer Play Mode `3.0.0`, Input System `1.20.0`, Cinemachine `6.6.0`.

**Demander d'abord :** Tout changement de version Unity, de nom de projet, de template, ou de version/liste de packages verrouilles necessite une approbation humaine avant implementation.

**Jamais :** Ne pas installer Unity Hub, Unity Editor ou un package a la place de l'utilisateur ; ne pas creer les dossiers/fichiers du projet Unity soi-meme ; ne pas marquer une ligne `Pass` sans preuve utilisateur reelle ; ne pas commencer les taches Story 0.3 (Unity Cloud, services, Relay).

## Matrice I/O & Cas Limites

| Scenario | Entree / Etat | Sortie / Comportement attendu | Gestion d'erreur |
|----------|---------------|-------------------------------|------------------|
| Unity `6000.6.0f1` indisponible sur Unity Hub | L'utilisateur ne trouve pas la version exacte | Le tutoriel indique de marquer la ligne `Blocked`, noter la source consultee et la version disponible, puis demander approbation avant tout fallback | Ne jamais auto-accepter une version differente |
| Projet cree avec un nom ou template different | `Assets/`, `Packages/`, `ProjectSettings/` existent mais ne correspondent pas a `RRS` / Universal 3D/URP | La validation agent reste `Blocked` et demande la correction avant de poursuivre | Ne pas valider un projet non conforme |
| Version de package resolue differente de la version verrouillee | `Packages/manifest.json` ou `packages-lock.json` montre un ecart | Le journal de validation note le mismatch explicitement au lieu de le masquer | Ne pas marquer `Pass` sur un ecart non documente |

</frozen-after-approval>

## Code Map

- `docs/setup/epic-0-readiness-checklist.md` -- Lignes Story 0.2 ("Unity editor et projet", "Packages verrouilles") a mettre a jour pour pointer vers le nouveau tutoriel avec des criteres de preuve precis.
- `docs/setup/tooling-validation-log.md` -- Lignes `VAL-002` a `VAL-011` : ajouter la reference au tutoriel comme source de la commande/chemin UI ; les statuts restent `Not Started` tant qu'aucune preuve utilisateur n'existe.
- `_bmad-output/implementation-artifacts/epic-0-context.md` -- Versions verrouillees et dependances Story 0.2 -> Story 0.3 a restituer fidelement dans le tutoriel.
- `_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/beginner-architecture-guide.md` -- Source canonique de l'ordre d'installation (section "Install Order", etapes 1-4) et de la structure de dossiers initiale ; ne pas diverger de cette liste sans approbation.
- `docs/setup/story-0-2-unity-install-tutorial.md` -- Nouveau fichier cree par cette story : le tutoriel pas-a-pas lui-meme.
- `_bmad-output/implementation-artifacts/sprint-status.yaml` -- Suit `epic-0` et `0-2-unity-editor-project-creation-and-package-pinning` ; a mettre a jour seulement lors d'un changement d'etat d'implementation.

## Taches & Acceptation

**Execution :**
- [x] `docs/setup/story-0-2-unity-install-tutorial.md` -- Creer le tutoriel pas-a-pas (Unity Hub, Unity `6000.6.0f1` sur Unity 6 Update, creation du projet Universal 3D/URP `RRS`, installation/verification des sept packages verrouilles via Package Manager) avec une preuve a fournir apres chaque etape et une instruction explicite d'arret avant Story 0.3 -- donne a l'utilisateur un chemin lineaire unique pour executer Story 0.2.
- [x] `docs/setup/epic-0-readiness-checklist.md` -- Mettre a jour les deux lignes Story 0.2 pour pointer vers le nouveau tutoriel et preciser la preuve exacte attendue -- garde la checklist maitresse synchronisee avec le tutoriel.
- [x] `docs/setup/tooling-validation-log.md` -- Ajouter une reference au tutoriel dans les colonnes pertinentes de `VAL-002` a `VAL-011` sans changer leurs statuts -- evite de re-decrire les memes etapes ailleurs sans fabriquer de completion.

**Criteres d'acceptation :**
- Given le depot ne contient encore aucun dossier Unity, when le tutoriel est cree, then il liste dans l'ordre Unity Hub, Unity `6000.6.0f1` sur Unity 6 Update, la creation du projet Universal 3D/URP `RRS`, puis les sept packages verrouilles avec leurs versions exactes.
- Given le tutoriel est inspecte, when chaque etape d'installation est lue, then une preuve concrete a fournir est indiquee avant que la ligne correspondante puisse passer a `Pass` dans le journal.
- Given `Assets/`, `Packages/` et `ProjectSettings/` restent absents apres cette story, when le journal de validation est inspecte, then aucune ligne `VAL-002` a `VAL-011` n'est marquee `Pass` sans preuve utilisateur reelle.
- Given le tutoriel est termine, when le lecteur atteint la derniere etape, then il est explicitement dirige a s'arreter et a rapporter ses preuves avant que Story 0.3 (Unity Cloud/services) ne commence.

## Spec Change Log

- 2026-09-02 : Cree `docs/setup/story-0-2-unity-install-tutorial.md` (tutoriel pas-a-pas fidele a l'ordre et aux versions de la section "Install Order" de `beginner-architecture-guide.md`, avec preuve exacte par etape et arret explicite avant Story 0.3). Mis a jour les quatre lignes Story 0.2 (`Unity editor et projet`, `Packages verrouilles`) dans `docs/setup/epic-0-readiness-checklist.md` (actions manuelles et validations agent) pour pointer vers le tutoriel. Ajoute une reference au tutoriel dans la colonne "Commande ou chemin UI" de `VAL-002` a `VAL-011` dans `docs/setup/tooling-validation-log.md` sans changer leurs statuts (tous restent `Not Started`, aucune preuve utilisateur reelle n'existe encore). Aucun dossier `Assets/`, `Packages/` ou `ProjectSettings/` n'a ete cree par l'agent. Toutes les commandes de verification de la spec ont ete executees et passent. `sprint-status.yaml` mis a `review` pour la story 0.2 (travail agent-executable termine ; les etapes manuelles Unity restent a la charge de l'utilisateur).
- 2026-09-02 : Revue a 3 couches (blind-hunter, edge-case-hunter, verification-gap) executee sur le tutoriel. Patches appliques dans `docs/setup/story-0-2-unity-install-tutorial.md` : ajout de l'activation de licence Unity (Etape 1, bloquante pour la creation de projet), gestion `Blocked` si l'installation/connexion Unity Hub echoue, avertissement chemin OneDrive/long avant la creation du projet (Etape 3), cas limite "creation echouee" si aucun dossier Unity n'apparait, note pour activer les packages preview si un package verrouille n'apparait pas, retrait de la verification `NetworkManager` non realisable a ce stade (aucun `NetworkManager` n'existe avant Story 0.4/Epic 1), gestion `Blocked` si un package echoue totalement a s'installer, et clarification que `packages-lock.json` fait foi en cas de desaccord avec `manifest.json`. Reordonne la liste des sept packages verrouilles (Etape 4 du tutoriel et ligne "Packages verrouilles" de `epic-0-readiness-checklist.md`) pour suivre l'ordre canonique VAL-005 a VAL-011. Toutes les commandes de verification de la spec re-executees et passent ; aucun dossier `Assets/`, `Packages/` ou `ProjectSettings/` toujours present. Quatre pistes mineures (`.gitignore` Unity, convention de stockage des preuves captures, fallback reseau restreint, et deux edge cases deja couverts par le pattern `Accepted With Known Blockers` de l'Epic 0) enregistrees dans `deferred-work.md` sans etre actionnees, hors perimetre des criteres d'acceptation.
- 2026-09-02 : Apres creation reelle du projet Unity `RRS`, preuves utilisateur et inspection locale validees. Unity Transport `6.6.0` est approuve humainement comme nouveau verrou pour Unity `6000.6.0f1` car le package est resolu par Unity comme builtin dans `Packages/packages-lock.json` et le cache package local. `VAL-002` a `VAL-011` sont desormais `Pass`, Story 0.2 est cloturee dans `sprint-status.yaml`, et Story 0.3 est promue `ready-for-dev`.
- 2026-09-02 : Course-correction (`_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-02.md`) -- le package `com.unity.services.multiplayer` `2.3.1` est retire du verrou Story 0.2 et remplace par un transport Steamworks communautaire (`com.community.netcode.transport.facepunch` ou `.steamnetworkingsockets`), suite a la contrainte financiere solo-dev identifiee par l'utilisateur (pas de revenu recurrent, le cout ne doit pas scaler avec le nombre de joueurs). Renegociation explicite de la section frozen-after-approval, demandee et approuvee par l'utilisateur. `VAL-007` repasse de `Pass` a `Not Started`, reciblee sur la presence du transport Steamworks. Le package `com.unity.services.multiplayer` reste installe dans `Packages/manifest.json` tant que l'utilisateur ne l'a pas retire manuellement via Package Manager -- cette entree de log ne pretend pas que l'action manuelle est faite.

## Notes De Design

Le contenu du tutoriel doit rester fidele mot pour mot aux versions et a l'ordre de la section "Install Order" de `beginner-architecture-guide.md` -- ne pas reformuler les versions ou l'ordre des etapes, seulement les detailler pour un debutant (ou cliquer, quoi verifier, quelle preuve capturer).

## Verification

**Commandes :**
- `Test-Path docs/setup/story-0-2-unity-install-tutorial.md` -- attendu : `True`.
- `$t = Get-Content -LiteralPath 'docs/setup/story-0-2-unity-install-tutorial.md' -Raw; $required = @('6000.6.0f1','RRS','2.13.2','Steamworks','17.6.0','3.0.0','1.20.0','6.6.0'); foreach ($r in $required) { if ($t -notmatch [regex]::Escape($r)) { throw "tutoriel manque $r" } }` -- attendu : aucune erreur ; toutes les versions verrouillees, le nom de projet et le transport Steamworks sont presents.
- `$t = Get-Content -LiteralPath 'docs/setup/story-0-2-unity-install-tutorial.md' -Raw; if ($t -notmatch '(?i)Story 0\.3') { throw 'instruction arret avant Story 0.3 manquante' }` -- attendu : aucune erreur ; le tutoriel dirige explicitement vers un arret avant Story 0.3.
- `$log = Get-Content -LiteralPath 'docs/setup/tooling-validation-log.md' -Raw; for ($i=2; $i -le 11; $i++) { $id = 'VAL-{0:D3}' -f $i; if ($log -notmatch "\|\s*$id\s*\|\s*``Pass``") { throw "$id doit etre Pass apres preuves Story 0.2" } }` -- attendu : aucune erreur ; toutes les lignes VAL-002 a VAL-011 sont validees.
- `$yaml = Get-Content -LiteralPath '_bmad-output/implementation-artifacts/sprint-status.yaml' -Raw; if ($yaml -notmatch '(?m)^  0-2-unity-editor-project-creation-and-package-pinning: done$') { throw 'sprint-status story 0.2 invalide' }` -- attendu : aucune erreur.

## Suggested Review Order

**Tutorial entry point and failure paths**

- Single linear path for Story 0.2; states up front that the agent can guide but never perform these manual steps.
  [`story-0-2-unity-install-tutorial.md:1`](../../docs/setup/story-0-2-unity-install-tutorial.md#L1)

- License activation added as a blocking prerequisite for project creation, plus a `Blocked` path if Hub install/sign-in fails.
  [`story-0-2-unity-install-tutorial.md:14`](../../docs/setup/story-0-2-unity-install-tutorial.md#L14)

- OneDrive/long-path warning added before project creation, a common silent Unity breakage cause.
  [`story-0-2-unity-install-tutorial.md:41`](../../docs/setup/story-0-2-unity-install-tutorial.md#L41)

- Preview-packages toggle called out, and package ordering realigned to the canonical VAL-005..VAL-011 sequence; the unreachable `NetworkManager` check was dropped since no such object exists this early.
  [`story-0-2-unity-install-tutorial.md:53`](../../docs/setup/story-0-2-unity-install-tutorial.md#L53)

**Tracking doc sync**

- Story 0.2 rows point readers at the new tutorial instead of re-describing the same steps.
  [`epic-0-readiness-checklist.md:46`](../../docs/setup/epic-0-readiness-checklist.md#L46)

- VAL-002 through VAL-011 now have completed evidence, with Transport `6.6.0` approved as the active lock for Unity `6000.6.0f1`.
  [`tooling-validation-log.md:30`](../../docs/setup/tooling-validation-log.md#L30)
