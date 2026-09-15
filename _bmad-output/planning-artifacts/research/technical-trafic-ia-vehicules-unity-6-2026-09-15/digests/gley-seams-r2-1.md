# Digest — R2 Gley extension seams and authoring value (round 2)

Scope: close the Gley cell. All evidence retrieved 2026-09-15. Primary sources: Gley's official
GitBook docs for v3, the live Unity Asset Store page, and the publisher's Unity Discussions thread.

## Claims

**Gley exposes a documented, named per-vehicle behaviour-override API: `API.SetVehicleBehaviours(int vehicleIndex, IBehaviourList vehicleBehaviours)`** — https://gley.gitbook.io/mobile-traffic-system-v3/complete-api/setvehiclebehaviours.md | Gley (vendor docs) | undated page, product v3.6.4 | accessed 2026-09-15 | high | class: api-surface

**Custom driving logic is authored by subclassing an abstract class: "create a new script that implements the **VehicleBehaviour** abstract class", overriding an abstract `Execute` method "called every update when the behaviour is active"** — https://gley.gitbook.io/mobile-traffic-system-v3/setup-guide/vehicle-behaviours.md | Gley | undated | accessed 2026-09-15 | high | class: api-surface

**The output of a behaviour is a `BehaviourResult` object whose documented fields are `SteerPercent`, `BrakePercent`, `MaxAllowedSpeed`, `TargetGear`, `SpeedInPoint`** — same URL | Gley | accessed 2026-09-15 | high | class: api-surface
> This is the decisive shape. It is a **command/actuator** struct, not a **decision-parameter** struct. There is no documented `timeHeadway`, `minGap`, `comfortableDecel`, `politeness`, or gap-acceptance threshold anywhere in it.

**Multiple behaviours run at once and are merged: the system "allows vehicles to operate in multiple states simultaneously through a merger mechanism that combines active behaviours into a final executable command"** — same URL | Gley | accessed 2026-09-15 | high | class: api-surface

**20 built-in behaviours ship, named: Stop, Temp Stop, Avoid Reverse, Stop in Distance, Stop in Point, Give Way, Overtake, Follow Vehicle, Decelerate, No Waypoints, Forward, Follow Player, Overtake Player, Change Lane, Drive on Side, Slow Down and Stop, Curve Slow Down, Reverse, Clear Path, Ignore Traffic Rules** — same URL | Gley | accessed 2026-09-15 | high | class: api-surface
> `Ignore Traffic Rules`, `Overtake Player`, `Follow Player`, `Clear Path` are directly rage-shaped. They already exist as swappable list entries.

**Behaviour lifecycle has named start/stop control and named events: `StartVehicleBehaviour`, `StopVehicleBehaviour`, `GetVehicleBehaviourOfType`, `InitializeBehaviourImplementation`, `SetAllVehiclesBehaviours`; events `OnBehaviourStarted`, `OnBehaviourStopped`, `OnVehicleCrashed`, `OnWaypointReached`, `OnDestinationReached`, `OnChangeDestination`, `OnObstaclesUpdated`, `OnObstacleRemoved`, `OnVehicleActivated`, `OnVehicleDisabled`** — https://gley.gitbook.io/mobile-traffic-system-v3/llms.txt (complete documentation page index) | Gley | accessed 2026-09-15 | high | class: api-surface

**Four delegates replace default system decisions: "Modify Trigger Size" — "Controls the dimension of the front trigger based on the vehicle's speed"; "Spawn Waypoint Selector" — "Controls the selection of a free waypoint to instantiate a new vehicle on"; "Traffic Lights Behaviour"; "Custom Position Validation" — "Delegate used to validate the position of a vehicle before instantiation"** — https://gley.gitbook.io/mobile-traffic-system-v3/delegates.md | Gley | accessed 2026-09-15 | medium-high (signatures not published on the index page) | class: api-surface
> "Modify Trigger Size" is the closest thing in the product to a following-distance knob: the front trigger is the detection volume that feeds `Follow Vehicle`. Shrink it and the car tailgates. **But it is documented as a function of speed, not of vehicle identity** — whether it can be varied per vehicle is unverified.

**Source is included, not DLL-only: "Complete code included and commented."** — https://gley.gitbook.io/mobile-traffic-system-v3/key-features.md | Gley | accessed 2026-09-15 | high | class: source-availability

**The system is Burst/jobs based: "Works for any platform that supports the Burst compiler" (docs); "powered by a specialized job structure driven by the Unity Burst Compiler" (store listing)** — key-features.md + https://assetstore.unity.com/packages/tools/behavior-ai/mobile-traffic-system-v3-305800 | Gley / Unity | accessed 2026-09-15 | high | class: source-availability

**INFERENCE, not verified: the behaviour layer is managed C#, not Burst.** `VehicleBehaviour` is an *abstract class* with a virtual `Execute` called every update — Burst-compiled jobs cannot dispatch through managed abstract classes. So the driving *decisions* are most likely readable/editable C# and the Burst jobs are the detection/raycast/steering-math layer. | confidence: medium, reasoning from the documented API shape | class: source-availability
> Flagged explicitly: I did not see the source. This is the single most load-bearing unverified item in the cell. It is resolvable in 10 minutes with the asset in hand.

**Routed source/sink traffic IS supported but is off by default: pathfinding uses "A* algorithms in an optimized manner"; "pathfinding is disabled due to performance considerations" since "computing paths in every frame can be CPU-intensive". Enabling it generates "an additional set of waypoints overlaid on the existing ones".** — https://gley.gitbook.io/mobile-traffic-system-v3/setup-guide/path-finding-setup.md | Gley | accessed 2026-09-15 | high | class: authoring

**Without pathfinding enabled, vehicles "pick random turns at intersections"; with it, you "designate an exact destination for a vehicle, and it will determine the optimal route to reach it". Waypoint penalties bias route choice (e.g. prefer highways over dirt roads).** — same URL | Gley | accessed 2026-09-15 | high | class: authoring

**Routing API is named and per-vehicle: `GetPath`, `SetVehiclePath`, `RemoveVehiclePath`, `SetDestination` (Vector3), `InstantiateVehicleWithPath`, `InstantiateVehicleOnTheSpot`, `IsThisWaypointADestination`** — llms.txt index + path-finding-setup.md | Gley | accessed 2026-09-15 | high | class: api-surface

**Despawn is distance-driven from the player, and is individually suppressible: `DistanceToRemove` — "the approximate distance from the player at which a vehicle is flagged to be removed from the scene"; `DontRemoveVehicle(vehicleIndex, true)` — "The marked vehicle will never be automatically removed once instantiated by the system—it will persist until it is manually removed."** — https://gley.gitbook.io/mobile-traffic-system-v3/complete-api/trafficoptions.md and .../dontremovevehicle.md | Gley | accessed 2026-09-15 | high | class: api-surface

**Spawning is occlusion-aware at the near end: `MinDistanceToAdd` — "Beyond the MinDistanceToAdd, vehicles will spawn even without any obstructions or buildings blocking the view."** — trafficoptions.md | Gley | accessed 2026-09-15 | medium-high | class: authoring
> Implies that *inside* MinDistanceToAdd, spawning requires occlusion. The symmetric question — whether *removal* checks visibility — is **not documented**. See gaps.

**Multi-camera / multi-observer support exists in the API: `SetCamera`, `SetCameras` (plural), `SetActiveSquares`, `ActiveSquaresLevel`** — llms.txt index + trafficoptions.md | Gley | accessed 2026-09-15 | medium-high | class: api-surface
> Directly relevant to co-op: the streaming grid can be driven by more than one player position. Not advertised as a networking feature, and I found no doc page explaining it.

**Price €118.69, version 3.6.4, released 4 September 2026, publisher Gley, licence "Extension Asset", "Covered by Standard Unity Asset Store EULA", 65 reviews, 222 favorites, 107.9 MB, original Unity version 2022.3.62, Built-in/URP/HDRP compatible** — https://assetstore.unity.com/packages/tools/behavior-ai/mobile-traffic-system-v3-305800 | Unity Asset Store | live page | accessed 2026-09-15 | high | class: pricing

**Asset Store aggregate is 65 reviews with a 5-star headline rating; I could not read any individual review text** — same URL, and search-result snippet | Unity / search index | accessed 2026-09-15 | low on the star figure, high on "could not read" | class: pricing / user-report

## The API surface, by name (or: the documented absence of one)

**What exists, verbatim from the docs index and pages:**

Per-vehicle behaviour control:
`SetVehicleBehaviours(int vehicleIndex, IBehaviourList vehicleBehaviours)` · `SetAllVehiclesBehaviours` ·
`StartVehicleBehaviour` · `StopVehicleBehaviour` · `GetVehicleBehaviourOfType` ·
`InitializeBehaviourImplementation` · abstract class `VehicleBehaviour` with `Execute()` → `BehaviourResult`

Per-vehicle driving/state control:
`StopVehicleDriving` · `ResumeVehicleDriving` · `SetDestination` · `SetVehiclePath` · `RemoveVehiclePath` ·
`GetPath` · `SetSpeedVariationPercentage` · `SetGiveWayProperty` · `SetStopProperty` · `SetOffset` ·
`IgnoreVehicle` / `RestoreIgnoredVehicle` · `ExcludeVehicleFromSystem` / `RestoreExcludedVehicleToSystem` ·
`DontRemoveVehicle` · `GetSteeringAngle` · `SetHazardLights` · `UpdateVehicleLights`

World/system control:
`SetTrafficDensity` · `SetActiveSquares` · `ClearTrafficOnArea` · `DisableAreaWaypoints` / `EnableAllWaypoints` ·
`SetTrafficLightsCrossingState` · `SetIntersectionRoadToGreen` · `SetPriorityCrossingStopState` ·
`AddWaypointEvent` / `RemoveWaypointEvent`

**What is documented as ABSENT.** Nowhere in the 60+ API pages, the behaviour page, or the delegates page is there a
per-vehicle parameter for:

- desired time headway / following distance (only the indirect, speed-keyed `Modify Trigger Size` delegate)
- minimum gap
- comfortable or maximum deceleration (only `BrakePercent` as an instantaneous command)
- maximum acceleration (only `MaxAllowedSpeed` as a ceiling)
- gap-acceptance threshold for merging or crossing
- lane-change willingness / politeness
- an "aggression" scalar of any kind

`SetSpeedVariationPercentage` is the only named per-vehicle tuning scalar, and it is speed-only — exactly the
"just speed" knob the project said is insufficient.

**Therefore the honest characterisation of the seam:** Gley gives you a per-vehicle **strategy-swap** seam
(replace the behaviour list, add your own `VehicleBehaviour` subclass, intercept via events) sitting at the
**command layer** (steer %, brake %, speed cap, gear). It does not give you the **parameter layer** the project's
round-1 design targets. The IDM/MOBIL-style constants the rage/fear layer wants to modulate live *inside* the
shipped `Follow Vehicle`, `Overtake`, `Change Lane` and `Give Way` behaviour classes, and the docs expose no knob
for them. Reaching them means reading and editing Gley's source — permitted, since complete source ships, but it
is a fork-in-practice of the exact classes you most need, and it is re-merged by hand on every vendor update.

Round 1's phrasing "the vendor documents a behavior-override API" is **confirmed and is not marketing** — the API
names are real and per-vehicle. But it overrides *which behaviour runs*, not *how cautiously it drives*.

## Does it support routed source/sink traffic with no mid-road despawn?

**Routing: yes, verified, with a cost.** A* pathfinding, disabled by default for CPU reasons, enabled from
Settings Window → Path Finding, which bakes a second overlaid waypoint set. `GetPath` returns a waypoint list that
can be cached and handed to any vehicle via `SetVehiclePath`; `SetDestination` retargets a live vehicle;
`InstantiateVehicleWithPath` spawns one already routed; `OnDestinationReached` / `OnChangeDestination` close the
loop. Waypoint penalties bias route variety. Entry points, varied paths and exit points are all expressible. With
pathfinding off you get random turns only — so the project would be running Gley in its non-default, more
expensive configuration.

**Density maintenance: yes.** `InitialDensity`, `SetTrafficDensity(int)`, `MinDistanceToAdd`,
`DistanceToRemove`, `ActiveSquaresLevel` form a streaming pool around the player.

**Mid-road despawn: partially controllable, and this is a real friction point.** Removal is documented as
distance-from-player only — `DistanceToRemove` "flags" a vehicle for removal at approximately that radius. The
docs do **not** state that removal checks visibility, whereas spawning explicitly does check occlusion inside
`MinDistanceToAdd`. The escape hatch is `DontRemoveVehicle(vehicleIndex, true)`, which pins an individual vehicle
permanently — usable, but it is a per-vehicle manual pin, not a "never despawn while visible" policy, and pinning
many vehicles defeats the pool that makes the asset fast. A user complaint corroborates the concern rather than
resolving it.

## Critical user reports on v3

**Weakest part of this cell — and the weakness is itself a finding.** I could not retrieve individual Asset Store
review text (the reviews render client-side; the fetched page returned only the aggregate). The publisher's Unity
Discussions thread yielded real but **pre-v3** complaints. v3's store listing shows v3.6.4 dated 4 Sept 2026;
the posts I could read are March 2024 – April 2025, i.e. predominantly v2 era. Per the brief, they do not transfer
cleanly — but two of them are architectural, not bug-shaped, and the architecture did not change between v2 and v3
in the relevant respects:

- **bobadi, 2 May 2024:** "The spawning system breaks the illusion of real traffic" — asked for pre-placed AI cars
  instead of runtime spawning. Directly the project's no-mid-road-despawn concern, from a paying user, and Gley's
  answer (per the thread) was a density/pool explanation rather than a policy fix.
  https://discussions.unity.com/t/mobile-traffic-system-city-traffic-for-games/843638?page=7
- **hansdrum, 11 May 2024:** "AI cars seem to break very suddenly...it would be more realistic if its more smooth",
  "This really breaks immersion" — braking is not graduated; questioned whether the asset can "simulate real traffic
  behavior". This is exactly an IDM-absence symptom: bang-bang braking on trigger entry rather than a continuous
  car-following model. Same URL.
- **mojtaba64, 29 Apr 2025:** asked how to increase distance between spawned traffic cars — again, spacing is not a
  first-class parameter.
- **shafei1, 14 Mar 2024:** external (non-Gley) vehicles fall through the road once the system activates — a layer
  /integration friction report, relevant because this project must integrate player-driven and networked vehicles.
- **Lundrim12, 2 Feb 2025:** asked about runtime road generation — unanswered in what I read.

No account was found of anyone successfully building a custom emotional/aggression AI layer on top of Gley. That
is absence of evidence, not evidence of absence — but for the project's #1 criterion, nobody has publicly walked
this path.

## What I looked for and could NOT find

1. **Individual Asset Store review text, critical or otherwise, for v3.** The page is JS-rendered; only the
   aggregate (65 reviews) came back. The star rating "5 stars / 63 reviews" appears only in a search-engine
   snippet, not read off the live page — treat it as low confidence. **Unclosed. This is the biggest hole.**
2. **Whether the driving-decision classes are plain C# or Burst-jobified.** Inferred managed-C# from the abstract
   class + per-update `Execute` shape; not verified. Resolvable only with the package in hand.
3. **Exact delegate signatures**, and specifically whether `Modify Trigger Size` can key on vehicle identity rather
   than only speed. If it can, it is a usable per-vehicle tailgating knob for the rage layer. If it cannot, the
   nearest headway control is source-editing.
4. **Whether `DistanceToRemove` removal checks visibility.** Documented for spawning, silent for removal.
5. **The FAQ page content** — the fetch returned an empty/refused response on two framings; not retried within
   budget. It may contain the source/DLL and despawn answers.
6. **Any mention of Netcode for GameObjects, multiplayer, or networking** anywhere in the 130-page doc index.
   Round 1's finding stands, now with a stronger basis: the doc index has no networking page at all, and the entire
   API is `static` (`API.SetVehicleBehaviours(...)`), i.e. a process-global singleton — an additional structural
   obstacle to host-authoritative replication that round 1 did not have evidence for.
7. **Gley's v2→v3 upgrade/refund policy.** Upgrade *guide* pages exist (`upgrade-from-v2-to-v3`); commercial
   upgrade *pricing* was not found. Still unconfirmed, as in round 1.
8. **The Standard Unity Asset Store EULA text itself.** I verified the licence *label* on the live page
   ("Extension Asset", "Covered by Standard Unity Asset Store EULA") but did not read the EULA this run, so I make
   no claim about what modifications it permits beyond noting that the asset ships complete source and the vendor
   documents an extension workflow.
