---
title: 'Mise a jour des preuves de validation MCP Story 0.5 (VAL-020 a VAL-024)'
type: 'chore'
created: '2026-09-07'
status: 'done'
review_loop_iteration: 0
route: 'one-shot'
baseline_commit: '74b239918b0b69db43b80fdd2a1ed07f565b89d2'
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** L'utilisateur a fourni quatre captures d'ecran comme preuve pour VAL-020 a VAL-024 (Story 0.5), mais les documents de suivi (`tooling-validation-log.md`, `epic-0-readiness-checklist.md`) n'etaient pas a jour, et les captures fournies ne couvraient en realite que le cote Blender (VAL-022 a VAL-024), pas Unity (VAL-020, VAL-021).

**Approach:** Verifier chaque capture par rapport a la preuve exacte attendue dans `story-0-5-mcp-tooling-configuration-tutorial.md`, completer par une verification agent en direct via le pont Blender MCP maintenant fonctionnel dans cette session (inspection de la scene, du materiau et du fichier `.glb` exporte sur disque), puis mettre a jour `tooling-validation-log.md` et `epic-0-readiness-checklist.md` en consequence -- en gardant VAL-020/VAL-021 `Not Started` puisqu'aucune preuve Unity n'a ete fournie, plutot que de les marquer `Pass` a tort.

</frozen-after-approval>

## Suggested Review Order

**Preuves Blender passees en `Pass`**

- Version Blender `5.2` recoupee par deux captures independantes (chemin extension + type de fichier Explorateur).
  [`tooling-validation-log.md:51`](../../docs/setup/tooling-validation-log.md#L51)

- Selection Blender Lab MCP confirmee (addon actif port `9876`) et config client Codex/Claude Code recoupee directement dans `~/.codex/config.toml` et `.mcp.json`.
  [`tooling-validation-log.md:52`](../../docs/setup/tooling-validation-log.md#L52)

- Smoke test Blender MCP verifie en direct par l'agent (objet `Cube` + materiau `Cube_Material`, export `scene_test.glb` present sur disque) plutot que sur la seule foi de la capture.
  [`tooling-validation-log.md:53`](../../docs/setup/tooling-validation-log.md#L53)

**VAL-020/VAL-021 laissees `Not Started` malgre la demande initiale**

- Aucune des quatre captures ne montre l'Unity Editor ; note aussi une entree `unity_mcp`/`relay_win.exe` deja presente en config, non documentee par le tutoriel, a clarifier avant preuve.
  [`tooling-validation-log.md:49`](../../docs/setup/tooling-validation-log.md#L49)

**Synchronisation du gate Epic 0**

- Ligne action manuelle Story 0.5 passee `Not Started` -> `In Progress` avec le detail Blender fait / Unity restant.
  [`epic-0-readiness-checklist.md:50`](../../docs/setup/epic-0-readiness-checklist.md#L50)

- Ligne validation agent Story 0.5 et resume du Gate Epic 1 mis a jour avec le meme etat.
  [`epic-0-readiness-checklist.md:66`](../../docs/setup/epic-0-readiness-checklist.md#L66)
