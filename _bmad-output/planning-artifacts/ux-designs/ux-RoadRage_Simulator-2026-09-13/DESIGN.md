---
title: RoadRage MVP 1 Menu Design
status: final
updated: 2026-09-13
sources:
  - ../../../../specs/spec-road-rage-simulator/course-correction-2026-09-13.md
---

# Brand & Style

Greybox-first desktop game UI. The selected character model is the visual focus; final art direction is deferred.

# Layout & Spacing

The main menu reserves a large center or side viewport for the 3D character preview and keeps primary actions reachable without opening a secondary profile page.

# Components

| Component | Visual decision |
| --- | --- |
| Character preview | Rotatable greybox 3D Rookie or Veteran model; no gameplay-stat display. |
| Character switch | Two direct labelled controls, Rookie and Veteran, with the current choice visibly selected. |
| Main actions | Start Game, Create Lobby, Join Lobby use the existing menu visual hierarchy. |
| Lobby roster identity | Read-only selected character visual/name; no selector. |

# Do's and Don'ts

- Do preserve visible feedback for Steam/session errors.
- Do use accessible labels in addition to character visuals.
- Do not add a nickname field, profile-management page, progression display, or locked-character affordance.
