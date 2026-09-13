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
