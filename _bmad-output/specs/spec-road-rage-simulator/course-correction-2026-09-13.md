# Course Correction — 2026-09-13

This companion supersedes conflicting MVP scope, game-flow, economy, profile, and run-assembly statements in the earlier SPEC companions. Historical implementation records remain unchanged.

## Requirement classification

### Locked current requirements

- First launch creates and persists one local player profile from Steam identity. Steam display name is used; there is no nickname field, confirmation screen, offline fallback, account system, progression, or backend.
- Rookie is the default character. Rookie and Veteran differ only in visual model and can be replaced as greybox assets later.
- Main Menu is the only character-selection surface. It shows an inspectable rotating 3D preview and lets the player switch Rookie/Veteran before solo start, lobby creation, or lobby join.
- Lobby is a separate screen. Character/profile selection is immutable after entry to a lobby.
- The selected character must drive the existing payload-to-spawn-to-presentation path for both solo and multiplayer. Persistent profile, Steam identity, lobby/session, and spawned runtime player are separate concerns.
- Current MVP work is a sandbox for independently testable, reusable gameplay foundations. It must not assemble Level 1 or Level 2 prematurely.
- Money, owned items, and temporary run resources are per-player. Network authority remains host-authoritative for multiplayer runtime state.
- NPC reaction foundations support configurable Rage and Fear tendencies. Rage/Fear state and static tuning are separate from vehicle movement.

### MVP 2 target direction

- A roguelite run contains levels; profile cosmetics persist between runs; temporary run resources can carry between normal level transitions but reset when a party restart returns to the latest inter-level checkpoint.
- Level 1 city: on-foot play, litter recovery, source-vehicle identification, preparation, equipment, a boss, and vehicle access are later assembly work.
- Level 2 highway: vehicle play, rest areas, hostile drivers, boss progression, vehicle condition and later component replacement are later assembly work.
- Litter, NPC vehicle identity, provocation/intimidation, Rage/Fear, damage, parts, encounters, boss triggers, and level/run/checkpoint state should be introduced as small foundations only when their dedicated story requires them.

### Provisional greybox ideas

Exact reward amounts, boss vehicles, truck/old-woman presentation, boss phases, Rage Essence, vehicle-part list, final character models, and final Rage/Fear values are not architecture contracts.

## Non-goals for MVP 1

- Complete city or highway scenes, full roguelite assembly, bosses, final combat loop, economy balance, checkpoint implementation, persistent progression, shared wallet, final vehicle-parts simulation, or fallback identity system.

## Ownership boundaries

| Concern | Owner / lifetime |
| --- | --- |
| Steam display identity | platform boundary; read into profile |
| Persistent profile | local persistent storage; identity and selected cosmetic only |
| Lobby membership / ready state | Steam lobby and session services |
| Character selection in a networked match | immutable connection payload and host-resolved runtime state |
| Player life, seats, items, wallet, NPC meters, vehicles, run resources | host-authoritative runtime state |
| Menu preview, camera rotation, HUD presentation | local visual presentation |

## Corrective delivery order

1. Review 4.1–4.4; resolve Story 4.4's documentation-status discrepancy.
2. Implement persistent Steam profile and Main Menu visual selection.
3. Freeze the profile at lobby entry and verify payload/spawn/presentation in solo and multiplayer.
4. Establish individual-wallet authority before any real reward or purchase.
5. Add configurable Rage/Fear before AI behavior work.
6. Define run/level/checkpoint contracts only when MVP 2 assembly begins.
