# Epic 4 Context: Passenger Chaos Actions & Rage Module

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

## Goal

Non-driving players (passengers) get three chaos actions they can trigger through in-game UI while someone else drives. Each action sends client intent to the host, which validates and applies the effect, producing a visible change to a target's rage state, an incident, a low-value resource opportunity, or a crew-help effect. This epic gives co-op players a meaningful role beyond driving and establishes the rage/intent contracts that later epics build on: AI traffic will attach real rage-driven behavior to vehicles, and rage escalation here is what eventually triggers a Rage Road confrontation.

## Stories

- Story 4.1: Rage State Module and Definitions
- Story 4.2: Passenger Action Framework and Host-Validated Intent
- Story 4.3: Passenger Action One Changes Rage
- Story 4.4: Passenger Action Two Creates an Incident or Resource Opportunity
- Story 4.5: Passenger Action Three Provides Crew Help
- Story 4.6: Epic 4 Passenger Chaos Playable Checkpoint

## Requirements & Constraints

- Rage must be tracked independently per target (not a shared/global meter), with both a numeric value and a state label.
- Passengers get exactly three action slots. Each is authored as static, host-validated data, not one-off scripts.
- Every action is submitted as client intent and only takes effect after host validation; invalid target, unavailable seat, cooldown, and disconnected-player cases must all produce visible feedback rather than failing silently.
- Every action must visibly change at least one of: target rage, an incident marker, a low-value resource/opportunity, or a crew-help effect. No action may directly grant upgrades or declare run outcomes — significant rewards stay tied to later road-rage confrontation, not to these side actions.
- The rage module and action framework must each be playable and testable on their own (a rage/action sandbox), without depending on real AI traffic or Rage Road confrontation from later epics.
- The epic must end in a launchable, testable state with the new capability verified by at least two players in a local multiplayer test.
- Exact tuning numbers, action names, and tone/content-rating boundaries are explicitly unresolved and expected to change before final content — build for replaceability, not lock-in.

## Technical Decisions

- Rage lives per target on a host-owned networked rage-state component (calm/irritated/flee/block/ram/confrontation-capable), never as a shared/global value; other systems read rage through a narrow interface/event and never mutate it directly.
- Static passenger-action and rage-tuning data are ScriptableObject definitions with stable, globally unique, lowercase ids, registered into one shared definition catalog at bootstrap; network payloads send definition ids, not full data, and host/clients must resolve the same catalog version.
- Runtime session values (current rage, action cooldowns, etc.) live in host-owned networked state, never inside ScriptableObject assets.
- Client input becomes a typed intent sent to the host (never a direct state mutation); host validation checks actor, run phase, player mode/seat, cooldown, target reference, range, and payload version before applying any effect.
- Targets referenced in intents use a Netcode-safe object reference, never authored ids or indexes, so targeting stays correct across spawn/despawn and late join.
- Gameplay-authoritative networked objects and variables are host-owned/server-write by default; camera and input remain local-only presentation and must never touch shared state directly.
- Naming conventions to follow: networked state components prefixed `Networked...`, ScriptableObject definitions suffixed `...Def`, client intent types suffixed `...Intent`; feature code organized under `Rage` and `PassengerActions` feature folders; diagnostic logs prefixed with the owning feature tag (e.g. `[Rage]`).

## UX & Interaction Patterns

- Gameplay UI (HUD or dev UI) must show current rage state and the three available passenger actions, alongside the run/money/failure/victory elements already established.
- Each action needs a visible result after use (rage change, incident, resource, or crew-help feedback), plus visible feedback when an action is unavailable, on cooldown, or targets something invalid.
- A dev UI/HUD element showing raw rage state is acceptable and expected during greybox testing, ahead of any polished presentation.

## Cross-Story Dependencies

- Story order is largely linear: 4.1 (rage module) must exist before 4.2 (action framework); 4.2 must exist before 4.3, 4.4, and 4.5 (the three individual actions), which can otherwise proceed in any order; 4.6 is the epic's checkpoint and depends on all five prior stories working together.
- Depends on the driving/seat setup from Epic 3 (a passenger seat to trigger actions from) but must not depend on AI traffic (Epic 5) or Rage Road confrontation (Epic 6) — those epics consume this epic's rage/action output later, so keep the rage and intent contracts stable for them to build on without rework.
