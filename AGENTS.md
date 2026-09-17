<!-- bmad:context -->
<!-- Verified 2026-09-13 against 8b1883905807e820d490f16fa575c3bf92bbf8eb. Managed by bmad-project-context; edits inside this block are replaced on refresh. Keep anything you want preserved outside the markers. -->

## RoadRage Simulator

Unity 6 cooperative driving prototype. BMAD remains the source of truth for Stories, acceptance criteria, architecture, and project decisions. Planning and implementation artifacts live under `_bmad-output/`; Unity code lives under `Assets/RoadRage/`.

## Where things are

- Architecture spine: `_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md`.
- Epic/story context and deferred work: `_bmad-output/implementation-artifacts/`.
- Graphify graph: `graphify-out/graph.json`; for codebase questions use `graphify query "<question>"`, `graphify explain "<symbol>"`, or `graphify path "<A>" "<B>"` before broad source browsing.

## Running and verifying

- Use Unity MCP when real Editor, scene, prefab, Inspector, Play Mode, or Console state matters.
- After source changes, run `graphify update .` unless the user explicitly asks not to touch Graphify output.

## Conventions that differ from defaults

- BMAD Story requirements, acceptance criteria, architecture decisions, and approved project decisions override tool/plugin recommendations.
- Use Graphify, Unity Skills, Unity MCP, Ponytail, and Blender MCP only when relevant to the current Story; do not force every tool into every workflow.
- Use Ponytail as an over-engineering guard, not as a reason to skip required robustness, networking rules, tests, or edge cases.
- Use Blender MCP only for actual Blender or 3D asset work.
- BMAD review checks acceptance criteria and change impact; do not turn review into automatic whole-repo cleanup.
- Toute fonctionnalite MVP jouable doit etre integree et verifiee dans `Assets/RoadRage/App/Scenes/MVP_Run.unity`; les scenes `Dev_*` restent reservees aux essais de modules isoles et ne remplacent pas un test d'integration.
<!-- /bmad:context -->

<!-- Section locale, volontairement hors du bloc bmad:context : elle survit aux refresh de bmad-project-context. -->

## Packs d'assets tiers

- **Synty POLYGON - City Pack** : `Assets/Synty/PolygonCity/` — 335 prefabs (Buildings 76, Props 174, Environments 65, Characters 9, Vehicles 9, FX 2), 330 FBX, 25 materiaux, 25 textures, scenes demo `Scenes/Demo.unity` et `Scenes/Overview.unity`. Source d'art principale pour le decor urbain.
- **Synty POLYGON - Generic Pack** : `Assets/Synty/PolygonGeneric/` — props et decors generiques, complementaires du pack City.
- Regle d'usage : ces packs sont du **decor uniquement**. Aucun de leurs prefabs ne porte d'etat gameplay, de `NetworkObject` ou de `NetworkVariable`. Ne jamais utiliser leurs prefabs `Characters` ou `Vehicles` comme avatars ou vehicules jouables : les prefabs reseau du projet restent la seule source de verite. Un prop Synty s'instancie en enfant visuel sous un root prefab projet (voir AD-13 / AD-27).
- Leur import exige le package Unity `com.unity.shadergraph` `17.6.0` (`Packages/manifest.json`). Sans lui, les materiaux Synty ne se compilent pas.
- Evaluation d'adoption, licence et inventaire detailles : `docs/setup/addon-adoption-register.md` (lignes `ADDON-009` et `ADDON-010`).
- `Assets/Synty/` est **volontairement ignore par git** (`.gitignore`) : la licence Synty interdit la redistribution et le depot est public. Sur un clone frais, reimporter les packs depuis l'Asset Store ; ne jamais les committer. Poids local ~196 Mo, dont 128,9 Mo de FBX/PNG qui partiraient sinon dans Git LFS.

<!-- Section locale, volontairement hors du bloc bmad:context : elle survit aux refresh de bmad-project-context. -->

## Perimetre et usage de Graphify

- Le perimetre du graphe est fixe par `.graphifyignore` (racine) : **code applicatif RoadRage uniquement**. Outillage agents (`.agents/`, `.claude/`, `.codex/`), moteur et artefacts BMAD (`_bmad/`, `_bmad-output/`), sortie Graphify et assets tiers Synty en sont exclus. Ne pas reintegrer ces chemins dans le graphe : ils avaient fait passer le corpus a 997 fichiers dont 15,6 % seulement de code projet, pour 12 011 noeuds contre une limite utile de 5 000.
- **Precheck obligatoire pour toute requete visant un symbole nomme.** Verifier d'abord son existence exacte, puis n'interroger Graphify que s'il existe :

  ```powershell
  rg "\bDriverProfileDef\b" Assets/RoadRage -g "*.cs"
  ```

  Symbole absent -> **STOP**, ne pas interroger Graphify, corriger le nom. Mesure : une requete sur un symbole inexistant coute ~2 600 tokens et renvoie des noeuds sans rapport, presentes avec le meme aplomb qu'une vraie reponse. Le rescope du graphe n'a pas supprime ce mode d'echec, il l'a seulement rendu moins reconnaissable (le bruit vient desormais de `Assets/RoadRage` au lieu de l'outillage).
- **Pas de precheck** pour les questions macro sans symbole nomme ("quels systemes participent au flow de lobby ?", "comment fonctionne la recuperation vehicule ?") : interroger Graphify directement.
- Toute sortie de requete sans aucune ligne `Assets/RoadRage` doit etre traitee comme un echec de resolution, jamais comme une reponse.

## Garde-fou avant et apres une operation Unity MCP

L'etat Git/disque et l'etat charge en memoire dans l'Editeur peuvent diverger : une scene ouverte `isDirty=true` a deja perdu un objet committe, qu'une sauvegarde aurait efface du disque.

**Avant** toute operation MCP significative (scene, prefab, asset, Inspector), verifier les deux etats :

```powershell
git status --short
unity cmd list_open_scenes        # -> isLoaded / isDirty / isActive par scene
```

Etat inattendu (scene `isDirty=true` non voulue, fichier modifie non explique) -> **STOP**, rendre la main a l'utilisateur. Ne jamais sauvegarder une scene dont l'etat dirty n'a pas ete explique.

**Apres** l'operation, reverifier `unity cmd list_open_scenes` puis `git status --short`, et `git diff` si un fichier inattendu apparait.

## Unity CLI et Editeur connecte

- Le Unity CLI (`unity`, version **1.0.0-beta.8** validee avec RRS) parle a l'**Editeur deja ouvert** : `unity status` doit renvoyer l'etat `ready` pour le projet RRS. Le batchmode (`unity run`, `unity build`) lance un second Editeur et ne peut pas ouvrir un projet deja ouvert.
- Commandes utiles en lecture : `unity cmd recompile_status`, `unity cmd console --level error`, `unity cmd list_open_scenes`, `unity cmd list_tests`, `unity cmd audit_status`.
- Version beta : ne pas mettre a jour le CLI automatiquement. Une montee de version exige une revalidation separee avant de devenir la nouvelle reference.
- La Console contient en permanence 500+ avertissements tiers (meshes Synty en version 8). Tout controle doit donc gater sur `--level error` et filtrer `Assets/Synty/` avant de regarder les warnings, sans masquer globalement les warnings issus de `Assets/RoadRage`.

<!-- Section locale, volontairement hors du bloc bmad:context : elle survit aux refresh de bmad-project-context. -->

## Verification declenchee par l'utilisateur (AD-4)

La verification (compilation, tests, `validate.ps1`, toute commande `unity cmd` qui n'est pas en lecture seule au sens de la garde ci-dessus) est **demandee a l'utilisateur, jamais executee directement par l'agent** — pas seulement dans les routes `bmad-build` (deja couvert par `_bmad/custom/bmad-build.toml`), mais dans toute session, y compris en dehors de BMAD. L'agent demande l'execution et consomme la sortie fournie.

Exception explicite : construire ou deboguer un outil de verification lui-meme (ex. ecrire `scripts/validate.ps1`) exige de l'exercer en direct pour le prouver ; dans ce cas, le dire clairement au moment de le faire plutot que de laisser la regle glisser silencieusement.
