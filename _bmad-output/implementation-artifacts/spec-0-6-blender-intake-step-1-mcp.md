---
title: 'Story 0.6 : execution MCP de l''etape 1 Blender'
type: 'chore'
created: '2026-09-07'
status: 'done'
route: 'one-shot'
---

# Story 0.6 : execution MCP de l'etape 1 Blender

## Intent

**Problem:** La Story 0.6 avait un tutoriel d'intake Blender, mais aucune preuve reelle de l'etape 1 n'etait encore capturee pour un asset source. Le mesh de travail etait encore issu du smoke test Blender et devait etre sauvegarde dans le chemin controle avant toute suite de pipeline.

**Approach:** Executer uniquement l'etape 1 via Blender MCP sur le mesh selectionne, sauvegarder `Prop_Barrel.blend` sous `Assets/RoadRage/ArtSource/Blender/`, generer la metadata Unity source pour stabiliser le GUID, puis consigner la preuve sans passer `VAL-025` a `Pass`.

## Suggested Review Order

**Preuve Blender**

- La preuve detaillee donne le statut et le perimetre exact.
  [`val-025-prop-barrel-step-1-evidence.md:1`](../../docs/setup/val-025-prop-barrel-step-1-evidence.md#L1)

- La provenance rappelle que l'asset est encore un placeholder cube.
  [`val-025-prop-barrel-step-1-evidence.md:11`](../../docs/setup/val-025-prop-barrel-step-1-evidence.md#L11)

- Les resultats MCP structurent les transforms, bounds et unites.
  [`val-025-prop-barrel-step-1-evidence.md:56`](../../docs/setup/val-025-prop-barrel-step-1-evidence.md#L56)

**Suivi VAL-025**

- La ligne principale garde `In Progress` et separe execute de futur.
  [`tooling-validation-log.md:54`](../../docs/setup/tooling-validation-log.md#L54)

- La note datee rattache la partie 1 au meme identifiant `VAL-025`.
  [`tooling-validation-log.md:101`](../../docs/setup/tooling-validation-log.md#L101)

- La checklist montre les parties 2 a 5 toujours ouvertes.
  [`epic-0-readiness-checklist.md:51`](../../docs/setup/epic-0-readiness-checklist.md#L51)

**Metadata Unity**

- Le GUID source Unity est stabilise par la meta committee.
  [`Prop_Barrel.blend.meta:2`](../../Assets/RoadRage/ArtSource/Blender/Prop_Barrel.blend.meta#L2)
