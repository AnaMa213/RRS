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
