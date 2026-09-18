# Epic 5 Context: Vehicle Physics, NPC Response Foundation and Routed Traffic

<!-- Compiled from planning artifacts. Edit freely. Regenerate with compile-epic-context if planning docs change. -->

## Goal

Epic 5 gives NPCs a real response system and the road a real traffic system. Stories 5.1–5.7 (delivered, in `review`) established the host-authoritative Rage/Fear foundation, basic route following, unified solo/online session start, networked rage targeting, the Rage Road trigger and client presentation. The 2026-09-15 course correction then expanded the epic to 18 stories because those foundations could not carry the intended gameplay: stories 5.8–5.17 add an escape menu, a parameterized driver model, an authored lane graph with a greybox district and portal-only traffic, intersection rules, wider perception, rage/fear as driving-model modulation, a player-targeted rage ladder, thrown-litter attribution, lobby-configurable headcounts and a measured scale validation, and the checkpoint moved to 5.18 so it stays last in its own epic. The **2026-09-18 course correction** then inserted a vehicle physics foundation ahead of the remaining traffic work, taking the epic to 23 stories: recette showed vehicles launched by curbs, stopped dead by low lips, and AI forcing its route after a collision (`ANO-5.10-03`), all traceable to a drive model that writes `Rigidbody.linearVelocity` directly with no wheel or suspension. New Stories 5.11-5.15 deliver chassis/wheels/suspension, tire forces and steering, arcade assists and uneven ground, AI driving by intent, and credible collisions; the former 5.11-5.18 shifted to 5.16-5.23 (5.16 keeps its number). The epic remains a foundation sandbox: the greybox district is an explicitly scoped proving ground, not the MVP 2 Level 1 city, and no run/level/checkpoint contract is introduced with it.

## Stories

- Story 5.1: Configurable NPC Rage/Fear Foundation
- Story 5.2: Basic AI Route Following and Recovery
- Story 5.3: Unified Solo and Online Session Start
- Story 5.4: Rage-Driven AI Behavior States
- Story 5.5: Networked AI Rage Targeting
- Story 5.6: Rage Road Event Trigger
- Story 5.7: AI Traffic Networking and Client Presentation
- Story 5.8: Escape Menu and Return to Main Menu
- Story 5.9: Parameterized Driver Model
- Story 5.10: Lane Graph, Greybox District, and Routed Source/Sink Traffic
- Story 5.11: Vehicle Chassis, Wheels, and Suspension
- Story 5.12: Tire Forces and Steering
- Story 5.13: Arcade Assists and Uneven Ground
- Story 5.14: AI Drives by Intent (owns `ANO-5.10-03`)
- Story 5.15: Credible Collisions and Damage Integration
- Story 5.16: Lobby-Configurable Traffic Settings
- Story 5.17: Wider Perception and Progressive Unblocking (ex-5.12)
- Story 5.18: Intersection Rules and Deadlock Prevention (ex-5.11)
- Story 5.19: Rage and Fear as Driving Model Modulation (ex-5.13)
- Story 5.20: Thrown Litter Foundation and Attribution (ex-5.15)
- Story 5.21: Player-Targeted Rage Ladder and Rage Road Trigger (ex-5.14)
- Story 5.22: Scale Validation and Network Budget (ex-5.17)
- Story 5.23: Epic 5 AI Traffic Playable Checkpoint (ex-5.18)

## Requirements & Constraints

- Traffic size is session data, never a constant: the host sets the maximum circulating AI vehicles and the litter-thrower count before the run, within authored bounds, and clients receive those values before the world loads through the match-settings synchronization that already exists. No controller, prefab, or scene carries a headcount constant.
- Vehicles enter and leave the world only at authored portals. Nothing else — blockage, congestion, distance to the player, route failure — may remove one, and a crowded portal queues a vehicle instead of dropping it.
- An AI's driving style is a parameter struct (time headway, minimum gap, maximum acceleration, comfortable deceleration, desired speed, lane-change politeness, change threshold, acceptable imposed braking). Acceleration and the lane-change decision are pure functions of it, with a safety veto independent of any gain, and are covered by EditMode tests without a scene or networking, including a stopped leader and a zero gap.
- Rage and fear modulate effective driving parameters; they never replace the driving logic, and one vehicle's emotional state never changes another vehicle's parameters.
- Rage and fear meters are per-vehicle and host-authoritative: authored decay (one percent per second by default), a freeze on entering a new rage tier (ten seconds by default) during which the meter can still rise, and no decay at one hundred percent rage while the associated Rage Road event is `Triggered` or `Confrontation`. Arbitration is authored, not hard-coded: fear at one hundred percent overrides rage at any level, fear at or above fifty percent and strictly above rage produces escape until fear falls to thirty percent, and rage governs otherwise.
- Each vehicle's state is independent, and transitions are host-authoritative and legible through movement plus a readable label — never color alone.
- Escalation must be visible and personal: a provoked AI moves through authored rage tiers — horn, then pursuit with insults and thrown litter, then a ramming attempt, then pursuit to a stop that requests the Rage Road trigger.
- Intersection priority is decided at authoring time (priority road, priority to the right, stop) and never recomputed per frame; a vehicle does not enter without room on its exit lane, and that check precedes the priority rule. A stop sign requires a full halt plus a minimum accepted gap.
- Deadlock recovery is a progressive authored rule-bending ladder (entering a normally kept-clear intersection, then driving around a blocker); every tier is authored and enabled, no removal, teleport or reinsertion tier exists, and a detected deadlock is logged in development builds with the vehicles involved.
- A blocked vehicle replans immediately with no waiting period, escalates visibly (horn after roughly two to four seconds, then an overtake as soon as a gap is acceptable), and is never teleported, reinserted, or removed to clear the blockage. A **deliberate** clearing manoeuvre does not drive onto a sidewalk; a trajectory *suffered* after a collision may leave the roadway, and Story 5.14 owns that case (`ANO-5.10-03` AC7). Perception covers several vehicles and players within an authored radius and arc, feeding leader selection and intersection arbitration, with cost bounded by a spatial index or a reduced update rate rather than a full per-frame scan.
- The greybox district must contain at least two two-way roads, two intersections and two tunnel-style portals, with sidewalks excluded from the vehicle agent's area mask rather than merely made expensive.
- Thrown litter is a host-owned NetworkObject with light-object physics, referencing its thrower through `NetworkObjectReference` and bounded by a configured global live-litter cap with oldest-first recycling; a piece whose thrower despawned stays valid but loses its return target.
- Targeting stays per-player and host-validated: an explicit lock persists until the player replaces or clears it, an out-of-range lock never falls back to another target, and an unlocked action affects only the nearest eligible AI in its range without creating a lock.
- One Rage Road event at a time, with host-side arbitration of simultaneous triggers and duplicate-trigger prevention; only trigger/pending/active/resolved visibility is in scope — resolution belongs to Epic 6.
- The checkpoint requires local and two-player online smoke tests in `MVP_Run` confirming host-authoritative, client-consistent state, plus the recorded scale measurements and the Epic 6 tuning assumptions.
- Vehicle physics is one layer shared by the player car and every AI vehicle: ground contact, suspension and tire force come from per-wheel raycasts driven by pure functions over a `VehicleProfileDef`, never from `WheelCollider` (AD-35). An AI driver emits a `VehicleDriveIntent` (steer, throttle, brake, handbrake) and never writes the `Rigidbody`'s velocity, position or rotation. The physics layer reads `VehicleProfileDef` only and never reads rage, fear or any driver disposition.
- The sidewalk curb is authored as module geometry inside the 4 m sidewalk band, never inside the 8 m roadway, so **the lane graph is not modified**. Story 5.11 validates this in the Editor on a junction or roundabout module — never a straight, because vehicles cut the inside of turns — against a measured 0.97 m of lateral clearance per side (2 m from lane axis to roadway edge, 2.06 m vehicle width). If AI vehicles touch the curb in nominal driving, the correction is the arrival radius or the Story 5.12 look-ahead point; touching the graph is the last resort.
- Driving surfaces other than road (grass, dirt, gravel) are deferred with a written reopening condition in `deferred-work.md`: Story 5.12 exposes a per-surface grip coefficient, but no second drivable surface is authored in Epics 5 or 6.

## Technical Decisions

- Traffic headcounts are configurable and never hard-coded: defaults and minimum/maximum bounds are authored in a `Def` ScriptableObject, the session value lives in `MatchSettings` and travels its existing synchronization path, and approximately thirty simultaneous AI vehicles is Epic 5's validation target — a larger production scale is an empirical decision taken only after the scale measurement, never an architecture contract beforehand. This supersedes the earlier "three spawned enemy vehicles" cardinality; one route, one player car and four players are unchanged.
- Driving is parameterized and emotion modulates it, so rage and fear must not appear as parallel driving states, speed multipliers, or extra branches bolted onto a state machine. `com.unity.ai.navigation` is a path provider only: `updatePosition` and `updateRotation` stay `false` and the host Rigidbody drives. A vehicle never carries `NavMeshAgent` and `NavMeshObstacle` together, and carving is never active on a moving vehicle.
- Traffic is source/sink: route variation comes from a per-junction weighted turn draw with authored ratios rather than per-vehicle itineraries, and a vehicle exceeding an authored edge budget is redirected to the nearest exit.
- Run's runtime state gains `NetworkedLitterState` (thrown-litter registry, per-piece source attribution, configured live-litter cap). Rage/Fear extends the existing host-owned `NetworkedRageState`; `Features/Vehicles` reads it through a narrow interface or event and never mutates it. NPC response is consumed by vehicle movement and archetype behavior through narrow feature boundaries, never hard-coded in a controller.
- Rage Road events stay one host-owned `Idle -> Triggered -> Confrontation -> Resolved -> RewardGranted` lifecycle; Epic 5 only drives it to a triggered/pending-visible state and must not introduce a second lifecycle. Epic 6 owns Confrontation onward.
- No scene-load path bypasses lobby creation and host networking: Start Game and Create Lobby are two entry points into one private-lobby-then-host path, and the escape menu must reuse the existing session-exit path rather than adding a second teardown; host quit is handled exactly as the existing host-quit case specifies.
- Authored static data uses `Def` ScriptableObjects with stable lowercase ids registered in the shared catalog at bootstrap, and runtime session values live only in host-owned NetworkVariables. Rage and litter targets travel as `NetworkObjectReference`, never authored ids or indexes. Host authority is unchanged: gameplay NetworkObjects are host-owned, gameplay NetworkVariables are server-write, and clients send typed ServerRPC intents the host validates for actor, target, spawned state, range, cooldown and payload version before any mutation. Camera and input stay local presentation.
- The greybox district is a _small greybox integration_ — no shops, equipment, boss, vehicle access or litter recovery — and introduces no run/level/checkpoint contract. Its assets follow the Blender intake gate, greybox-first and prefab-stability conventions. Conventions: `Networked`-prefixed state components, `Def`-suffixed definitions, `Intent`-suffixed intents, feature-prefixed logs; Unity 6 / C# 9.0 / URP / Netcode for GameObjects over Steam transport, with `MVP_Run` as the integration scene and `Dev_*` scenes for isolated module work only.

## UX & Interaction Patterns

- New in-run surface: pressing Escape in `MVP_Run` opens a menu with at least Resume and Quit to Main Menu. It must not pause the host-authoritative simulation, cursor capture and cursor mode must be restored cleanly on open and on close, and driving and action input must be suppressed while it is open.
- Quitting to the main menu returns to `MainMenuLobby` through the existing session-exit path: a host leaving is handled like the existing host-quit case, while a client leaving leaves the host and the other clients running uninterrupted.
- The lobby gains two bounded numeric fields next to the existing difficulty setting — maximum AI vehicles and litter-throwing vehicles — editable by the host only; out-of-bounds entries are refused with visible feedback, and litterers cannot exceed the total vehicle count.
- Horn, insults, litter impacts and pursuit state need readable player feedback, framed by the existing HUD; behavior and event state must never be communicated by color alone.
- Target lock stays per-player: `T` locks the nearest eligible AI and points the local camera at that vehicle (not a spawn point, invisible object or decor target), and `Y` cycles deterministically without disturbing the lock when no other eligible AI exists. Camera and input handlers are local presentation only and never mutate shared gameplay state.

## Cross-Story Dependencies

- Story 5.1 extends the Epic 4 rage foundation and must stay compatible with existing callers. Story 5.3 unified solo and online onto one host-authoritative path and is a prerequisite for every later story's verification; the duplicate solo path is removed rather than compensated for elsewhere.
- Story 5.5 supplies the persistence and eligibility rules that Story 5.21's rage ladder targets; Story 5.6 supplies the Rage Road lifecycle that Story 5.21 only requests a trigger on; Story 5.7 supplies the client presentation the checkpoint verifies.
- Stories 5.9 and 5.18 supersede the mechanisms of Stories 5.2 (waypoint loop with teleport recovery) and 5.4 (rage as a cruise-speed multiplier), which remain valid historical milestones. Story 5.9 removes `ResolveCruiseSpeedMultiplier` and re-expresses its behaviors through the parameter struct; the teleport recovery itself is removed by Story 5.14, which replaces it with a physical return to a valid lane (`ANO-5.10-03` AC6).
- Story 5.10 consumes the headcount from Story 5.16's session settings and falls back to the authored default until those settings exist; Story 5.16 extends `MatchSettings` — never a second settings object — and reuses the existing settings-sync path; Story 5.22 measures the configuration Story 5.16 delivers and gates any production scale decision. Story 5.19 needs Story 5.1's meters and Story 5.5's targeting; Story 5.20 exposes only the host-side litter return intent, since player pickup, binning and throwing back belong to Epic 6.
- Stories 5.11 to 5.14 are a strict chain: 5.12 consumes 5.11's wheel raycasts, 5.13 tunes against the curb height 5.11 authors, and 5.14 replaces the open-loop driver with an intent emitted on top of 5.12's steering. Story 5.15 recalibrates collision damage against the impact profile they produce, and Story 5.22 records the per-wheel cost of that layer as its own measurement line.
- Upstream: driving, seats and damage from Epic 3; passenger actions and the rage module from Epic 4. Downstream: Epic 6 consumes the Rage Road event, the litter return intent, the saturated-fear navigation mode and this epic's tuning assumptions; Epic 7 inherits the unified session path. Epic 5 ships event state and trigger arbitration only — not resolution, the MVP 2 city, boss, or checkpoints.
