---
title: 'Story 0.6 : execution MCP des etapes 2 a 4 Blender'
type: 'chore'
created: '2026-09-07'
status: 'done'
route: 'one-shot'
---

# Story 0.6 : execution MCP des etapes 2 a 4 Blender

## Intention

Poursuivre l'execution guidee de `VAL-025` sur le placeholder `Prop_Barrel` apres la partie 1 : verifier une echelle reelle plausible, nettoyer les materiaux, recalculer/valider normals et geometrie, produire un export controle sous `Assets/RoadRage/ArtExports`, puis documenter les limites restantes sans marquer `VAL-025` en `Pass`.

## Probleme

Les etapes 2 a 4 du tutoriel Story 0.6 demandent une preuve concrete, pas seulement un asset visuellement present dans Blender. Sans traces sur l'echelle de reference, les transforms, les materiaux, les normals, le contenu exporte, l'import metadata Unity et les hashes, l'asset pourrait entrer en prefab avec une echelle, un importeur ou une provenance ambigue.

## Approche

- Redimensionner la geometrie source dans Blender en conservant des transforms propres (`location 0,0,0`, `rotation 0,0,0`, `scale 1,1,1`).
- Utiliser une reference de baril acier 200/210 L pour arrondir le placeholder a `0.6 m x 0.6 m x 0.9 m`.
- Nommer et limiter le materiau source a `Prop_Barrel_Placeholder_Mat`.
- Recalculer les normals vers l'exterieur, verifier les dots numeriquement et nettoyer uniquement les loose vertices/edges si presents.
- Exporter un FBX selection-only vers `Assets/RoadRage/ArtExports/Prop_Barrel.fbx`, stabiliser la `.meta` Unity comme asset statique et documenter les limites de partie 5.

## Suggested Review Order

1. Preuve principale des parties 2 a 4 : [val-025-prop-barrel-step-2-4-evidence.md](../../docs/setup/val-025-prop-barrel-step-2-4-evidence.md#L15)
2. Normals/geometrie et limites Unity restantes : [val-025-prop-barrel-step-2-4-evidence.md](../../docs/setup/val-025-prop-barrel-step-2-4-evidence.md#L49), [limites](../../docs/setup/val-025-prop-barrel-step-2-4-evidence.md#L111)
3. Export controle et import metadata Unity : [val-025-prop-barrel-step-2-4-evidence.md](../../docs/setup/val-025-prop-barrel-step-2-4-evidence.md#L67), [fbx.meta](../../Assets/RoadRage/ArtExports/Prop_Barrel.fbx.meta#L44)
4. Ligne de validation `VAL-025` : [tooling-validation-log.md](../../docs/setup/tooling-validation-log.md#L54)
5. Checklist Epic 0 / Story 0.6 maintenue en `In Progress` : [epic-0-readiness-checklist.md](../../docs/setup/epic-0-readiness-checklist.md#L51), [ligne detaillee](../../docs/setup/epic-0-readiness-checklist.md#L67)
6. Exception explicite autorisant l'execution MCP/Codex des etapes 1 a 4 : [story-0-6-blender-asset-intake-tutorial.md](../../docs/setup/story-0-6-blender-asset-intake-tutorial.md#L22)
7. Travail differe sur la granularite du sprint status : [deferred-work.md](deferred-work.md#L103)

## Resultat

Les parties 2 a 4 de `VAL-025` sont completees pour `Prop_Barrel`. Le source `.blend` et l'export FBX ont des hashes documentes ; Unity lit le FBX comme `UnityEngine.GameObject`; le GLB temporaire a ete retire ; le FBX ne contient plus de chemin local machine. `VAL-025` reste `In Progress` tant que la partie 5 n'a pas cree/valide la scene de test Unity, le prefab et le plan collider.

## Verification

- `git diff --check`
- `Get-FileHash` sur `.blend`, `.blend.meta`, `.fbx`, `.fbx.meta`
- scan FBX ASCII pour prefixes locaux projet/utilisateur Windows et objet residuel `Prop_Sphere`
- controle `.fbx.meta` : cameras/lights/blend shapes/animations/avatar mapping desactives
- preuve GLB temporaire absent

Astuce review : Ctrl+click les liens ci-dessus dans VS Code pour parcourir le lot dans l'ordre.
