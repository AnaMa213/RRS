# MVP Scope

> **Superseded scope notice — 2026-09-13:** This legacy slice table is retained for traceability. The current authoritative scope is `course-correction-2026-09-13.md`.

## MVP 1 — Foundation Sandbox

Build and validate isolated reusable systems: Steam-backed persistent cosmetic profile, pre-lobby Rookie/Veteran selection, selected-character spawn, multiplayer boundaries, individual wallet, configurable Rage/Fear, vehicle identity/damage hooks, interactions, items, and NPC foundations as dedicated stories require. Do not assemble complete levels, bosses, checkpoints, or a full economy loop.

## MVP 2 — Advanced Gameplay Assembly

Compose validated foundations into city/highway levels, normal level-to-level carryover, inter-level checkpoint restart, encounters, bosses, progression, balancing, and presentation. Exact boss and reward details remain provisional.

This companion holds MVP limits and deferred scope so downstream planning does not inflate the first build.

## MVP Question

Can cooperative driving, passenger provocation, per-vehicle rage, crisis, and money feel fun with multiple players when built as a small loop?

## Legacy Included Slice (superseded)

| Area | MVP scope |
| --- | --- |
| Route | One route |
| Player vehicle | One player car |
| Players | Online co-op up to four players |
| AI traffic | Three AI vehicles with individual rage |
| Passenger play | Three passenger actions |
| On-foot mode | One simple transition |
| Sandbox stop | One compact zone |
| Economy | One money reward |
| Progression | One upgrade |
| Rage event | One Rage Road event |
| Boss endpoint | One simple endpoint sufficient to validate boss-kill victory |
| Run outcome | Team wipe restarts the run; boss death declares victory |
| Composition | Independently testable modules that compose into one integrated run |

## Deferred From MVP 1

- Rich city content.
- Complex final boss.
- Roguelite progression and full run assembly.
- Many driver archetypes.
- Deep economy.
- Varied weapons.
- Large 3D asset volume.
- Separate mini-games that cannot compose into the shared MVP run.

## Scope Risks

- In-car and on-foot play can become two separate games if on-foot actions do not directly feed rage, money, upgrades, and preparation.
- Online co-op up to four players can dominate implementation unless each milestone proves one small network slice before broad gameplay is added.
- Sandbox stops can become content-heavy if they require dense, replayable happenings before the core loop is proven.
- Passenger actions can become shallow unless they create incidents or change rage instead of existing only as one-off jokes.
- Module independence can accidentally create duplicate player, money, camera, input, rage, or network state unless the architecture spine remains binding.
