---
id: SPEC-road-rage-simulator
companions:
  - gameplay-model.md
  - mvp-scope.md
  - module-composition.md
  - course-correction-2026-09-13.md
  - ../../planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md
sources:
  - ../../forge/road-rage-simulator/forged-idea.md
---

> **Canonical contract.** This SPEC and the files in `companions:` are the complete, preservation-validated contract for what to build, test, and validate. Source documents listed in frontmatter are for traceability; consult them only if you need narrative rationale or prose color this contract intentionally omits.

# Road Rage Simulator

> **2026-09-13 course correction.** `course-correction-2026-09-13.md` is the authoritative current-direction companion. Where this earlier kernel or its legacy companions conflict with it, the course correction wins; completed stories remain historical records.

## Why

Road Rage Simulator exists to develop reusable multiplayer driving and on-foot gameplay foundations for a future chaotic cooperative roguelite. MVP 1 proves isolated greybox systems without prematurely assembling the final city, highway, boss, or progression loop.

## Capabilities

- **CAP-1**
  - **intent:** Players can validate reusable cooperative gameplay foundations in isolated greybox sandboxes.
  - **success:** Profile, spawn, vehicle, interaction, and NPC-response foundations can be exercised independently without duplicating runtime truth.

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
  - **intent:** Future player economy can remain individual and host-authoritative.
  - **success:** Any economy foundation assigns wallet, purchases, and owned items to one player rather than a crew wallet.

- **CAP-6**
  - **intent:** On-foot play can support preparation, rewards, and confrontation without becoming a separate game loop.
  - **success:** A simple on-foot transition lets players use a compact sandbox zone or Rage Road fight, then return to driving with changed money, items, upgrade state, or risk.

- **CAP-7**
  - **intent:** Future roguelite assembly can compose validated foundations into levels and runs.
  - **success:** MVP 2 can add inter-level checkpoints, team restart rules, boss progression, and victory without coupling persistent profile data to runtime state.

## Constraints

- The multiplayer target is online co-op for up to four players.
- Online co-op must use lobby-first room creation with a join code and invite-link wrapper; direct-connect-only multiplayer is not sufficient for the target.
- MVP 1 is a foundation sandbox; specific route, traffic, reward, upgrade, Rage Road, and boss cardinalities are deferred to MVP 2 assembly.
- MVP functionality must be organized as independently testable gameplay modules that can work in small development slices and compose into one integrated run.
- Module independence must not create separate engines, duplicate runtime truth, or incompatible versions of player life, money, input, camera, rage, or network state.
- The adopted architecture spine governs engine, camera, input, networking, module boundaries, runtime state, and asset pipeline decisions.
- In-car and on-foot modes must feed the same rage, money, upgrades, and preparation loop.
- Persistent profile stores Steam identity and cosmetic character selection only; gameplay state is runtime/session state.
- Money, items, and temporary run resources are per-player; a shared crew wallet is out of scope.
- NPC reaction data supports configurable Rage and Fear; final archetypes and tuning are deferred.

## Non-goals

- MVP 1 does not build city/highway levels, complete runs, bosses, final roguelite progression, checkpoints, deep economy, or a Steam fallback identity path.
- Destructible cities are outside the concept as currently locked.
- The MVP is not a full long-distance road-trip simulation; it is a focused proof of the rage-to-money cooperative loop.
- Separate mini-games or gameplay modules that cannot compose into the shared MVP run are outside scope.

## Success signal

MVP 1 demonstrates persistent pre-lobby cosmetic selection, consistent solo/multiplayer spawn, host-authoritative reusable gameplay state, and independently testable greybox systems. MVP 2 later assembles those systems into the level/run loop.

## Assumptions

- The first downstream target is a solo-beginner-feasible 3D prototype, because the Forge goal framed feasibility that way.
- MVP success can be judged by completing the core loop in an online co-op prototype run before validating long-term progression depth.

## Open Questions

- What exact health, damage, revive, and vehicle destruction rules determine player death and team wipe?
- What tone and content-rating boundaries apply to threats, pissing, fights, and absurd provocation actions?
