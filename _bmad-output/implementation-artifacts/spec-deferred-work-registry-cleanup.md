---
title: 'Nettoyage du registre de travail differe'
type: 'chore'
created: '2026-09-14'
status: 'done'
route: 'one-shot'
context: []
---

# Nettoyage du registre de travail differe

## Intent

**Problem:** Le registre append-only conservait des constats devenus faux, ce qui masquait la dette encore actionnable.

**Approach:** Ajouter des entrees de cloture factuelles et tracer un audit cible des warnings Unity et API depreciees.

## Suggested Review Order

- Les clotures preservent l'historique sans presenter des faits obsoletes comme de la dette active.
  [`deferred-work.md:208`](deferred-work.md#L208)

- L'audit futur impose un inventaire verifiable avant toute migration technique large.
  [`deferred-work.md:216`](deferred-work.md#L216)
