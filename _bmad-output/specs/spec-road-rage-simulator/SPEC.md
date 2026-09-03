---
id: SPEC-road-rage-simulator
companions:
  - gameplay-model.md
  - mvp-scope.md
  - module-composition.md
  - ../../planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md
sources:
  - ../../forge/road-rage-simulator/forged-idea.md
---

> **Canonical contract.** This SPEC and the files in `companions:` are the complete, preservation-validated contract for what to build, test, and validate. Source documents listed in frontmatter are for traceability; consult them only if you need narrative rationale or prose color this contract intentionally omits.

# Road Rage Simulator

## Why

Road Rage Simulator exists to test the vision of a chaotic online cooperative 3D road-trip game where up to four players act as road vigilantes and troublemakers, turning bad drivers into player-made incidents, money, and progression. The MVP must prove that the irreducible loop of cooperative driving, passenger provocation, per-vehicle rage, crisis, and reward is fun before broader content is built.

## Capabilities

- **CAP-1**
  - **intent:** Players can drive as a chaotic cooperative road crew that turns hostile or reckless traffic into incidents, money, and progression.
  - **success:** An online co-op MVP run completes the loop of driving, provoking an AI vehicle, escalating rage, resolving a crisis, earning money, and applying an upgrade.

- **CAP-2**
  - **intent:** Passengers can actively create or amplify chaos instead of waiting passively during driving.
  - **success:** The MVP exposes three passenger actions that visibly change at least one vehicle's rage, incident state, or resource opportunity.

- **CAP-3**
  - **intent:** Each enemy vehicle can track and express its own rage state independently from other vehicles.
  - **success:** Three AI vehicles on the same route can independently remain calm, become irritated, flee, block, ram, or trigger a confrontation based on their own rage.

- **CAP-4**
  - **intent:** Rage escalation can create Rage Road crises that force players to respond in or out of the car.
  - **success:** At least one Rage Road event can be triggered, resolved through confrontation, and converted into a money reward.

- **CAP-5**
  - **intent:** Players can earn meaningful money primarily by provoking, confronting, and beating hostile drivers.
  - **success:** Road-rage victory grants enough currency to buy one upgrade, while absurd side actions remain low-value triggers or toys.

- **CAP-6**
  - **intent:** On-foot play can support preparation, rewards, and confrontation without becoming a separate game loop.
  - **success:** A simple on-foot transition lets players use a compact sandbox zone or Rage Road fight, then return to driving with changed money, items, upgrade state, or risk.

- **CAP-7**
  - **intent:** A run can end in either team-wipe failure or boss-kill victory.
  - **success:** When every player dies the run restarts from the beginning, and when the boss dies the game declares victory.

## Constraints

- The multiplayer target is online co-op for up to four players.
- Online co-op must use lobby-first room creation with a join code and invite-link wrapper; direct-connect-only multiplayer is not sufficient for the target.
- The MVP scope is one route, one player car, online co-op, three AI vehicles with individual rage, three passenger actions, one simple on-foot transition, one compact sandbox zone, one money reward, one upgrade, one Rage Road event, and one simple boss endpoint.
- MVP functionality must be organized as independently testable gameplay modules that can work in small development slices and compose into one integrated run.
- Module independence must not create separate engines, duplicate runtime truth, or incompatible versions of player life, money, input, camera, rage, or network state.
- The adopted architecture spine governs engine, camera, input, networking, module boundaries, runtime state, and asset pipeline decisions.
- In-car and on-foot modes must feed the same rage, money, upgrades, and preparation loop.
- If every player dies, the run restarts from the beginning; if the boss dies, the game declares victory.
- Significant currency must come from road-rage confrontation; absurd actions pay little and primarily serve comedy, interaction, and escalation.
- Towns and stops are compact sandbox stops with happenings, money opportunities, purchases, and incidents, not destructible city simulations.
- The prototype must prove the core cooperative loop before adding complex boss behavior, roguelite systems, deep economy, weapon breadth, or large 3D asset volume.

## Non-goals

- Rich city content, complex final boss design, roguelite progression, many driver archetypes, deep economy, varied weapons, and large 3D asset volume are outside the MVP.
- Destructible cities are outside the concept as currently locked.
- The MVP is not a full long-distance road-trip simulation; it is a focused proof of the rage-to-money cooperative loop.
- Separate mini-games or gameplay modules that cannot compose into the shared MVP run are outside scope.

## Success signal

An online co-op prototype run demonstrates one route where passenger chaos provokes independently tracked AI rage, triggers a Rage Road crisis, pays meaningful money, buys an upgrade, and returns value to the same driving loop; a full run fails when every player dies and wins when the boss dies. The run is assembled from independently testable vehicle, passenger, rage, on-foot, economy, lobby/network, sandbox stop, boss, and UI slices without duplicating shared runtime truth.

## Assumptions

- The first downstream target is a solo-beginner-feasible 3D prototype, because the Forge goal framed feasibility that way.
- MVP success can be judged by completing the core loop in an online co-op prototype run before validating long-term progression depth.

## Open Questions

- What exact health, damage, revive, and vehicle destruction rules determine player death and team wipe?
- What tone and content-rating boundaries apply to threats, pissing, fights, and absurd provocation actions?
