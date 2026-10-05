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

- Le perimetre du graphe est fixe par `.graphifyignore` (racine) : **code applicatif RoadRage uniquement**, soit les fichiers applicatifs de `Assets/RoadRage/`. Les fixtures de `Tests/`, lanceurs et vues de diagnostic de `DevTools/`, artefacts d'auteur de `MVP_Run`, outillage agent, moteur et artefacts BMAD, documentation, manifestes et packages Unity, contenu livre par Unity et assets tiers en sont exclus. Ne pas reintegrer ces chemins exclus : les 110 fichiers de tests indexes gonflaient le graphe sans aider la navigation du code runtime.
- Etat verifie le 2026-10-03 apres resserrement : **173 fichiers / 4 305 noeuds / 9 967 liens / 186 communautes**, dont **172 fichiers `.cs` applicatifs conserves**. Le manifeste contient encore une note Graphify de 5 noeuds dans `graphify-out/memory/`, residu connu hors corpus applicatif. Reconstruction apres modification du perimetre : `graphify update . --force` — le `--force` est requis, un corpus reduit produit moins de noeuds et le rebuild serait sinon refuse. Limite connue : un fichier deja present dans le manifeste n'est pas elague par une nouvelle regle d'exclusion.
- **Precheck obligatoire pour toute requete visant un symbole nomme.** Verifier d'abord son existence exacte, puis n'interroger Graphify que s'il existe :

  ```powershell
  .\scripts\find-symbol.ps1 DriverProfileDef
  ```

  Codes de sortie : `0` trouve, `1` **absent -> STOP**, ne pas interroger Graphify, corriger le nom ; `2` la recherche elle-meme a echoue, le resultat est **inconnu** et jamais « absent ». Cette distinction est tout l'objet du script : `rg` n'est pas garanti present dans la session de l'agent (verifie absent le 2026-09-18), et une commande inconnue produit une sortie vide qu'un agent peut lire comme une absence. Mesure : une requete Graphify sur un symbole inexistant coute ~2 600 tokens et renvoie des noeuds sans rapport, presentes avec le meme aplomb qu'une vraie reponse. Le rescope du graphe n'a pas supprime ce mode d'echec, il l'a seulement rendu moins reconnaissable (le bruit vient desormais de `Assets/RoadRage` au lieu de l'outillage).

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

## Outillage : aucune installation sans decision enregistree (AD-13)

Aucun outil, plugin, package ou serveur MCP (Unity, Claude Code, Codex, VS Code) ne s'installe, ne se
configure ou ne s'invoque sans une ligne `ADDON-###` dans `docs/setup/addon-adoption-register.md`
portant le statut, la decision et le declencheur de reevaluation. Un outil **deja present** doit
avoir sa ligne retroactive : l'existence n'est pas un precedent.

Consequence pratique : decouvrir qu'un outil est disponible ne justifie pas de l'utiliser, et un
outil `Defer` ou `Reject` dans le registre ne se re-evalue pas pendant une story. La liste courante
des outils non integres et son motif vivent dans le registre, pas ici.

## Cycle de build BMAD (`bmad-build`)

Le routage operationnel du cycle de build — recon conditionnelle, selection Graphify, routage
EditMode/PlayMode, commande de verification a demander, politique de revue, outils a ne pas
invoquer — vit dans `docs/setup/build-workflow-rules.md`. Il est charge automatiquement au debut de
chaque run via `persistent_facts` (`_bmad/custom/bmad-build.toml`), donc il ne doit pas etre recopie
ici : `AGENTS.md` porte le transversal valable dans toute session, ce fichier porte le specifique au
cycle de build.

## Verification executee par l'agent (AD-4, abrogee le 2026-09-18)

La verification (compilation, tests, `validate.ps1`, toute commande `unity cmd` qui n'est pas en lecture seule au sens de la garde ci-dessus) est **executee directement par l'agent** — pas seulement dans les routes `bmad-build`, mais dans toute session, y compris en dehors de BMAD. La commande attendue, son mode et son interpretation vivent dans `docs/setup/build-workflow-rules.md`.

Pendant une story, limiter les tests a sa categorie ; les suites completes relevent de la fin d'epic selon `docs/setup/build-workflow-rules.md`.

**Interdit : scripts de test ou de verification ecrits par l'agent** (decision proprietaire du 2026-09-29). Compilation, execution de tests, lecture de resultats de tests et lecture de la Console pour verifier passent **uniquement** par `scripts/validate.ps1`, plus les commandes `unity cmd` en lecture seule listees plus haut. Ne jamais ecrire — dans le scratchpad ou ailleurs — de wrapper `.sh`/`.ps1`/`.py` autour de `unity cmd run_tests`, `test_status`, `recompile` ou `console`, ni de sonde C# executee dans l'Editeur pour obtenir une mesure ou un verdict a la place d'un test. Un controle necessaire devient une fixture sous `Assets/RoadRage/Tests/`, avec sa categorie, lancee par `validate.ps1`. Si `validate.ps1` ne couvre pas un besoin : STOP et le signaler, ne pas le contourner. Motif : run 5.31 (`runtests.sh`, `recompile.sh`, quatre sondes `Probe*.cs`), dont les executions echappaient a la porte AD-7 et au compte des validations.
