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

**Problem:** L'utilisateur a fourni des captures d'ecran comme preuve pour VAL-020 a VAL-024 (Story 0.5), mais les documents de suivi (`tooling-validation-log.md`, `epic-0-readiness-checklist.md`) n'etaient pas a jour ; le premier lot de quatre captures ne couvrait en realite que le cote Blender (VAL-022 a VAL-024), et le second lot (Unity) montrait un bridge non identifie et une inspection lecture seule sans le smoke test de creation/suppression requis.

**Approach:** Verifier chaque capture par rapport a la preuve exacte attendue dans `story-0-5-mcp-tooling-configuration-tutorial.md` ; completer par une verification agent en direct via les ponts Blender MCP puis Unity MCP devenus disponibles dans cette session (inspection de la scene, du materiau et du fichier `.glb` exporte sur disque cote Blender ; identification du package via les logs console et un cycle complet creation/verification/suppression de `MCP_SmokeTest` cote Unity) ; puis mettre a jour `tooling-validation-log.md` et `epic-0-readiness-checklist.md` en consequence -- en ne marquant `Pass` que ce qui est reellement prouve a chaque etape, jamais par anticipation.

</frozen-after-approval>

## Suggested Review Order

**Preuves Blender passees en `Pass`**

- Version Blender `5.2` recoupee par deux captures independantes (chemin extension + type de fichier Explorateur).
  [`tooling-validation-log.md:51`](../../docs/setup/tooling-validation-log.md#L51)

- Selection Blender Lab MCP confirmee (addon actif port `9876`) et config client Codex/Claude Code recoupee directement dans `~/.codex/config.toml` et `.mcp.json`.
  [`tooling-validation-log.md:52`](../../docs/setup/tooling-validation-log.md#L52)

- Smoke test Blender MCP verifie en direct par l'agent (objet `Cube` + materiau `Cube_Material`, export `scene_test.glb` present sur disque) plutot que sur la seule foi de la capture.
  [`tooling-validation-log.md:53`](../../docs/setup/tooling-validation-log.md#L53)

**Preuves Unity passees en `Pass` (identification du bridge + smoke test complete en direct)**

- Bridge identifie sans ambiguite comme `com.unity.ai.assistant` (Unity Official, pas CoplayDev) via la stack trace `Unity_GetConsoleLogs`, chemin de config confirme sur les trois clients sans secret.
  [`tooling-validation-log.md:49`](../../docs/setup/tooling-validation-log.md#L49)

- Smoke test complete par l'agent : creation, verification et suppression de `MCP_SmokeTest`, hierarchie identique a l'etat initial apres nettoyage.
  [`tooling-validation-log.md:50`](../../docs/setup/tooling-validation-log.md#L50)

**Synchronisation du gate Epic 0**

- Lignes action manuelle et validation agent Story 0.5 passees `Pass`, VAL-020 a VAL-024 toutes closes.
  [`epic-0-readiness-checklist.md:50`](../../docs/setup/epic-0-readiness-checklist.md#L50)

- Resume du Gate Epic 1 mis a jour avec le meme etat.
  [`epic-0-readiness-checklist.md:24`](../../docs/setup/epic-0-readiness-checklist.md#L24)
