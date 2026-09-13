---
title: RoadRage MVP 1 Profile, Menu, and Lobby Experience
status: final
updated: 2026-09-13
sources:
  - ../../../../specs/spec-road-rage-simulator/course-correction-2026-09-13.md
design: DESIGN.md
---

# Foundation

Windows desktop Unity UGUI experience. `DESIGN.md` owns visual presentation; this document owns flow and behavior.

# Information Architecture

| Surface | Purpose |
| --- | --- |
| Main Menu | Current selected-character preview; Rookie/Veteran switch; Start Game, Create Lobby, Join Lobby. |
| Lobby | Roster, ready/session controls, connection feedback, and read-only selected-character identity. |
| Runtime world | Spawned player uses the selection made before lobby/session entry. |

# Voice and Tone

Use concise action labels and explicit visible Steam/session failures. Do not ask the player to create or name a profile.

# Component Patterns

- Switching Rookie/Veteran persists the new cosmetic choice immediately.
- Preview rotation is local presentation only and has no gameplay/network state.
- Lobby displays the already-selected model but never offers a change control.

# State Patterns

| State | Expected behavior |
| --- | --- |
| First launch with Steam available | Automatically create Rookie profile from Steam display identity and show Main Menu. |
| Steam unavailable | Block the supported flow with explicit feedback; no fallback identity flow. |
| Selection changed in Main Menu | Persist choice and refresh preview. |
| Entered lobby | Freeze profile selection for that session. |
| Relaunch | Restore persisted selection before Main Menu is shown. |

# Interaction Primitives

Mouse/controller focus must reach all main actions and character-choice controls. Preview rotation must not prevent using the controls.

# Accessibility Floor

Character identity is always presented with text as well as a model. Selection state and errors are communicated without relying on color alone.

# Key Flows

## First launch — Kenan

1. Kenan starts the game while Steam is available.
2. The game creates a local profile with Steam display identity and Rookie selected.
3. Main Menu opens directly with Rookie previewed.
4. Kenan can start solo play, create a lobby, join a lobby, or choose Veteran before any lobby entry.

## Multiplayer entry — Kenan

1. Kenan selects Veteran in Main Menu.
2. Kenan creates or joins a lobby; the lobby shows Veteran as read-only identity.
3. The lobby starts the session.
4. Kenan spawns as Veteran; no runtime selection menu is presented.
