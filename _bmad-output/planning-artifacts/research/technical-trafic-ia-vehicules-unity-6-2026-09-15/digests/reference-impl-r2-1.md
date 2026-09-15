# Digest — R2 Reference implementations, concrete extraction (round 2)

Round 2 of the technical research run on urban AI traffic for a Unity 6 co-op driving game.
Method: downloaded and read the actual source files (curl → local read), not descriptions of them.
Access date for every item below: **2026-09-15**.

---

## Claims

- **highway-env ships IDM and MOBIL in one file, `highway_env/vehicle/behavior.py`, 583 lines, class `IDMVehicle`** — https://raw.githubusercontent.com/Farama-Foundation/HighwayEnv/master/highway_env/vehicle/behavior.py | Farama Foundation | file last touched by commit `5240596` dated 2026-07-03 | accessed 2026-09-15 | high (read the file) | class: pattern
- **`IDMVehicle` longitudinal constants are: `ACC_MAX = 6.0`, `COMFORT_ACC_MAX = 3.0`, `COMFORT_ACC_MIN = -5.0`, `DISTANCE_WANTED = 5.0 + LENGTH`, `TIME_WANTED = 1.5`, `DELTA = 4.0`, `DELTA_RANGE = [3.5, 4.5]`** (all m/s², m, s) — same file, lines 21–40 | accessed 2026-09-15 | high | class: parameter-value
- **`IDMVehicle` lateral (MOBIL) constants are: `POLITENESS = 0.0`, `LANE_CHANGE_MIN_ACC_GAIN = 0.2`, `LANE_CHANGE_MAX_BRAKING_IMPOSED = 2.0`, `LANE_CHANGE_DELAY = 1.0`** — same file, lines 43–46 | accessed 2026-09-15 | high | class: parameter-value
- **`AggressiveVehicle` and `DefensiveVehicle` do NOT subclass IDM's acceleration — they subclass `LinearVehicle`, and differ only in `LANE_CHANGE_MIN_ACC_GAIN`, `MERGE_ACC_GAIN`, and the 3-term `ACCELERATION_PARAMETERS` vector** — same file, lines 562–583 | accessed 2026-09-15 | high | class: parameter-value
- **Both personality classes inherit `LinearVehicle.TIME_WANTED = 2.5`, which *overrides* the IDM base 1.5 — so highway-env's "aggressive" driver actually wants a LONGER time headway than its plain IDM driver** — same file, line 372 vs line 33 | accessed 2026-09-15 | high | class: parameter-value
- **Neither personality class overrides `POLITENESS`; the shipped default is `0.0`, i.e. MOBIL's politeness term is multiplied by zero and the default driver is purely self-interested** — same file, line 43, no override at 562–583 | accessed 2026-09-15 | high | class: parameter-value
- **Vehicle geometry/speed baselines: `LENGTH = 5.0`, `WIDTH = 2.0`, `DEFAULT_INITIAL_SPEEDS = [23, 25]`, `MAX_SPEED = 40.0`, `MIN_SPEED = -40.0`** — https://raw.githubusercontent.com/Farama-Foundation/HighwayEnv/master/highway_env/vehicle/kinematics.py lines 22–30 | accessed 2026-09-15 | high | class: parameter-value
- **Lateral controller gains: `TAU_HEADING = 0.2` → `KP_HEADING = 5.0`; `TAU_LATERAL = 0.6` → `KP_LATERAL ≈ 1.667`; `TAU_PURSUIT = 0.1`; `MAX_STEERING_ANGLE = π/3`; `DELTA_SPEED = 5`** — https://raw.githubusercontent.com/Farama-Foundation/HighwayEnv/master/highway_env/vehicle/controller.py lines 24–33 | accessed 2026-09-15 | high | class: parameter-value
- **highway-env is MIT, dual copyright "2018 Edouard Leurent / 2023 Farama Foundation", LICENSE file present at repo root** — https://raw.githubusercontent.com/Farama-Foundation/HighwayEnv/master/LICENSE | accessed 2026-09-15 | high | class: license
- **Kink3d/SimpleTraffic is MIT with a real `LICENSE` file at root; GitHub API reports `license.spdx_id = "MIT"`; `pushed_at = 2019-05-01`, 275 stars** — https://api.github.com/repos/Kink3d/SimpleTraffic | accessed 2026-09-15 | high | class: license
- **SimpleTraffic constrains vehicles by driving a `NavMeshAgent` toward the *next connection point* of the current road section, not by any lane-following math — `agent.destination = destination.transform.position` where destination is a `NavConnection`** — https://raw.githubusercontent.com/Kink3d/SimpleTraffic/master/Assets/SimpleTraffic/Scripts/Vehicle.cs lines 25–34, 105–115 | accessed 2026-09-15 | high | class: pattern
- **SimpleTraffic's car-following is a single forward `Physics.Raycast` with a hard boolean stop at `blockedDistance = 0.25f`, and stopping is implemented as `agent.velocity = Vector3.zero`** — Vehicle.cs lines 86–100; https://raw.githubusercontent.com/Kink3d/SimpleTraffic/master/Assets/SimpleTraffic/Scripts/Agent.cs lines 54–64 | accessed 2026-09-15 | high | class: pattern
- **SimpleTraffic junctions are phase machines over `WaitZone` trigger volumes, `phaseInterval = 5f`, with the phase "ending" (all positive zones closed) at 50% of the interval as a de-facto amber** — https://raw.githubusercontent.com/Kink3d/SimpleTraffic/master/Assets/SimpleTraffic/Scripts/Junction.cs lines 22, 39–49, 145–149 | accessed 2026-09-15 | high | class: pattern
- **mchrbn/unity-traffic-simulation has NO file named LICENSE anywhere in the repo tree (recursive tree listing, master branch) and the GitHub API returns `"license": null` — BUT the README contains the complete MIT licence text including "Copyright (c) 2019 Matthieu Cherubini"** — https://api.github.com/repos/mchrbn/unity-traffic-simulation/git/trees/master?recursive=1 and https://raw.githubusercontent.com/mchrbn/unity-traffic-simulation/master/README.md lines 60–68 | accessed 2026-09-15 | high | class: license
- **mchrbn's car-following is a fan of `raysNumber = 6` raycasts at `raySpacing = 2°`, `raycastLength = 5`, with two hard thresholds: `emergencyBrakeThresh = 2f` and `slowDownThresh = 4f`, resolved into a 3-state enum `Status {GO, STOP, SLOW_DOWN}`** — https://raw.githubusercontent.com/mchrbn/unity-traffic-simulation/master/Assets/TrafficSimulation/Scripts/VehicleAI.cs lines 22–56, 228–272 | accessed 2026-09-15 | high | class: pattern
- **mchrbn's stop-sign intersections use two lists (`vehiclesQueue`, `vehiclesInIntersection`) plus a `prioritySegments` list; a non-priority arrival is STOPped if either list is non-empty, otherwise it is admitted at SLOW_DOWN; on exit, the head of the queue is released** — https://raw.githubusercontent.com/mchrbn/unity-traffic-simulation/master/Assets/TrafficSimulation/Scripts/Intersection.cs lines 67–98 | accessed 2026-09-15 | high | class: pattern

---

## Driver personality constants (the calibration table)

### Table A — what highway-env actually ships (verbatim, `behavior.py` 2026-07-03)

| Parameter | DefensiveVehicle | IDMVehicle (base) | AggressiveVehicle | What it controls | Source |
|---|---|---|---|---|---|
| `LANE_CHANGE_MIN_ACC_GAIN` (a_th) | **1.0** m/s² | **0.2** m/s² | **1.0** m/s² | How much acceleration gain a lane change must buy before it is taken. Higher = fewer lane changes. | behavior.py:44, 563, 575 |
| `LANE_CHANGE_MAX_BRAKING_IMPOSED` (b_safe) | 2.0 (inherited) | **2.0** m/s² | 2.0 (inherited) | Hard veto: max braking the cut-in may force on the new follower. | behavior.py:45 |
| `POLITENESS` (p) | 0.0 (inherited) | **0.0** | 0.0 (inherited) | Weight on *other* vehicles' acceleration change in MOBIL. 0 = purely selfish. **Never varied by personality in this codebase.** | behavior.py:43 |
| `LANE_CHANGE_DELAY` | 1.0 s | **1.0 s** | 1.0 s | Decision re-evaluation period. | behavior.py:46 |
| `TIME_WANTED` (T) | **2.5 s** (via LinearVehicle) | **1.5 s** | **2.5 s** (via LinearVehicle) | Desired time headway. | behavior.py:33, 372 |
| `DISTANCE_WANTED` (s0) | 10.0 m (inherited) | **10.0 m** (= 5.0 + LENGTH 5.0) | 10.0 m (inherited) | Jam distance, centre-to-centre (hence +LENGTH). | behavior.py:30 |
| `COMFORT_ACC_MAX` (a) | 3.0 (inherited) | **3.0** m/s² | 3.0 (inherited) | Desired max acceleration. | behavior.py:24 |
| `COMFORT_ACC_MIN` (b) | −5.0 (inherited) | **−5.0** m/s² | −5.0 (inherited) | Comfortable deceleration. | behavior.py:27 |
| `ACC_MAX` | 6.0 (inherited) | **6.0** m/s² | 6.0 (inherited) | Final clamp on commanded acceleration. | behavior.py:21 |
| `DELTA` (δ) | 4.0 (inherited) | **4.0** (randomised in [3.5, 4.5]) | 4.0 (inherited) | Free-flow speed exponent. | behavior.py:36–39 |
| `MERGE_ACC_GAIN` | **1.2** | — (not defined) | **0.8** | Feeds the linear acceleration gains below. | behavior.py:564, 576 |
| `MERGE_VEL_RATIO` | 0.75 | — | 0.75 | idem | behavior.py:565, 577 |
| `MERGE_TARGET_VEL` | 30 m/s | — | 30 m/s | idem | behavior.py:566, 578 |
| `ACCELERATION_PARAMETERS` [k_speed, k_Δv, k_gap] | **[0.160, 0.0533, 2.0]** | LinearVehicle default **[0.3, 0.3, 2.0]** | **[0.1067, 0.0356, 0.5]** | Linear controller weights; the 3rd term is the gap-violation weight. | behavior.py:353, 567–571, 579–583 |

The `ACCELERATION_PARAMETERS` values are computed from the merge constants: `k_speed = GAIN/((1−RATIO)·VEL)`, `k_Δv = GAIN/(RATIO·VEL)`, `k_gap` literal.

### Honest reading of Table A — do not copy it blindly

The single real personality discriminator in highway-env is **`k_gap`: 0.5 aggressive vs 2.0 defensive — a 4× difference in how hard the driver reacts to an under-sized gap.** That is the numeric nugget. Everything else is either identical between the two classes or *backwards* from intuition:

- Both personalities use `TIME_WANTED = 2.5 s`, longer than plain IDM's 1.5 s. The "aggressive" driver is not tailgating.
- Both use `LANE_CHANGE_MIN_ACC_GAIN = 1.0`, five times *higher* than plain IDM's 0.2 — both personalities change lanes *less* than the base driver.
- `POLITENESS` is 0.0 for all three. highway-env never actually exercises the politeness knob.
- Aggressive has *lower* speed-error gain (0.1067 vs 0.160), so it converges to its target speed more slowly than the defensive one.

These classes were written for a highway *merge* scenario (the `MERGE_*` prefix is the giveaway), not as a general aggression model. Treat them as proof that the architecture works, and as one calibrated ratio (`k_gap` 4×), not as a tuning table.

### Table B — suggested rage/fear mapping onto the IDM/MOBIL knobs

Derived by us from the semantics of each parameter, anchored on the verified base values. **Not measured — this is engineering judgement built on read constants, and needs in-game tuning.**

| Knob | Fearful / timid | Calm baseline (verified IDM default) | Enraged | Effect in play |
|---|---|---|---|---|
| T (time headway) | 2.5–3.0 s | **1.5 s** | 0.4–0.8 s | Tailgating. The most legible aggression cue. |
| s0 (jam distance, centre-to-centre) | 12–14 m | **10 m** (5 + car length) | 6–7 m | How close it creeps in a queue. |
| a (comfort accel) | 1.5–2.0 | **3.0** | 4.5–6.0 | Launch violence off a light. |
| b (comfort decel) | −3.0 (brakes early, gently, from far) | **−5.0** | −8.0 (late, hard braking) | Also widens d* via the sqrt(a·b) term — see formula. |
| δ | 4.0 | **4.0** | 4.0 (leave alone) | Rarely worth exposing. |
| v0 (desired speed) | 0.85 × limit | speed limit | 1.25–1.4 × limit | Speeding. |
| p (politeness) | 0.5–1.0 | **0.0 as shipped**; 0.2–0.5 is a saner "normal" | 0.0 or negative | Negative p = actively enjoys hurting others' acceleration. This is the rage knob MOBIL gives you for free. |
| a_th (min acc gain) | 0.5–1.0 | **0.2** | 0.02–0.05 | Weaving. Low threshold = constant lane changes. |
| b_safe | 1.0–1.5 | **2.0** | 4.0–6.0 | Cutting people off. The dangerous-feeling knob. |

---

## IDM acceleration, as implemented

From `behavior.py:150–217` (`IDMVehicle.acceleration` + `IDMVehicle.desired_gap`). Reduced to C#-portable form:

```
// v      = ego speed (m/s), clamped >= 0 in the free term
// v0     = ego target speed, clamped to [0, lane.speedLimit] when the lane has one
// d      = lane_distance_to(front)  -- along-lane, CENTRE TO CENTRE
// dv     = dot(velocity_ego - velocity_front, direction_ego)   (projected, not scalar speed diff)

float aFree = COMFORT_ACC_MAX * (1f - Mathf.Pow(Mathf.Max(v,0f) / NotZero(v0), DELTA));

float dStar = DISTANCE_WANTED
            + v * TIME_WANTED
            + (v * dv) / (2f * Mathf.Sqrt(COMFORT_ACC_MAX * -COMFORT_ACC_MIN));

float a = aFree;
if (hasFront) a -= COMFORT_ACC_MAX * Mathf.Pow(dStar / NotZero(d), 2f);

a = Mathf.Clamp(a, -ACC_MAX, ACC_MAX);
```

Port notes taken from the source, not from the paper:

1. `DISTANCE_WANTED = 5.0 + LENGTH`. The gap `d` is measured centre-to-centre along the lane, so the vehicle length is folded into s0 rather than subtracted from d. If your Unity code measures bumper-to-bumper, drop the `+ LENGTH`.
2. `COMFORT_ACC_MIN` is stored **negative** (−5.0); the code writes `ab = -COMFORT_ACC_MAX * COMFORT_ACC_MIN` to get a positive 15.0 before the sqrt. Getting this sign wrong produces NaN.
3. `utils.not_zero()` guards both `v0` and `d` against division by zero — you need the same guard, or a stopped leader at distance 0 blows up.
4. `dv` is the **projected** closing speed (`dot` of the velocity difference onto ego heading), not `speedEgo − speedFront`. In a curved city road this matters; `projected=True` is the default in `desired_gap`.
5. The rear vehicle is passed into `acceleration()` but is **unused** in the IDM branch. It exists only so `LinearVehicle` can override with a signature that uses it.
6. During a lane change (`lane_index != target_lane_index`), `act()` computes IDM against *both* the current and the target lane and takes `min(...)` — the conservative one. That is 4 lines and it is what stops mid-change collisions (behavior.py:121–131).
7. Braking is not special-cased. Negative `a` is the brake. A single signed acceleration channel is the whole longitudinal API.

## MOBIL lane-change criterion, as implemented

From `behavior.py:265–324` (`IDMVehicle.mobil`), plus the gating in `change_lane_policy` (219–263).

Gating, before MOBIL is even called:
- If a lane change is already in progress, **do not** evaluate a new one; instead check whether another vehicle is also merging into the same target lane, and if its gap is below `desired_gap`, **abort** by resetting `target_lane_index = lane_index` (lines 229–244). Cheap, effective, and exactly the failure case a naive implementation hits.
- Otherwise evaluate only every `LANE_CHANGE_DELAY = 1.0 s` (`utils.do_every` on a per-vehicle timer seeded from `(sum(position) * π) % delay`, so vehicles are de-phased and do not all decide on the same frame — a nice trick for frame-cost spreading).
- Skip if `|speed| < 1` m/s.
- Only lanes from `road.network.side_lanes(...)` that pass `is_reachable_from(position)`.

The criterion itself, in evaluation order:

```
// 1. SAFETY (hard veto)
aNewFollowerBefore = IDM(newFollowing, front: newPreceding)
aNewFollowerAfter  = IDM(newFollowing, front: self)
if (aNewFollowerAfter < -LANE_CHANGE_MAX_BRAKING_IMPOSED) return false;   // b_safe = 2.0

// 2. ROUTE OVERRIDE: if the planned route demands a specific lane
aSelfAfter = IDM(self, front: newPreceding)
if (route requires a lane) {
    if (sign(candidate - target) != sign(routeLane - target)) return false;  // wrong way
    if (aSelfAfter < -LANE_CHANGE_MAX_BRAKING_IMPOSED)      return false;
    // NOTE: when on route, the incentive test below is SKIPPED entirely
}
// 3. INCENTIVE (only when not route-forced)
else {
    aSelfBefore        = IDM(self, front: oldPreceding)
    aOldFollowerBefore = IDM(oldFollowing, front: self)
    aOldFollowerAfter  = IDM(oldFollowing, front: oldPreceding)

    gain = (aSelfAfter - aSelfBefore)
         + POLITENESS * ( (aNewFollowerAfter - aNewFollowerBefore)
                        + (aOldFollowerAfter - aOldFollowerBefore) );

    if (gain < LANE_CHANGE_MIN_ACC_GAIN) return false;   // a_th = 0.2
}
return true;
```

Port notes:

1. MOBIL costs **six IDM evaluations per candidate lane**. With `LANE_CHANGE_DELAY` gating and de-phased timers, that is amortised to roughly 6 evaluations/vehicle/second, not per frame. This is the single most important performance fact for a Unity port: the decision layer is cheap *because it is gated*, not because it is simple.
2. The variable is literally named `jerk` in the source but is an acceleration gain (m/s²), not a jerk. Do not copy the name.
3. `self.acceleration(ego_vehicle=other, ...)` is called with *other* vehicles as ego — the ego vehicle models its neighbours using **its own** parameters, not theirs. So an enraged driver evaluates whether a cut-in is safe using *its own* aggressive b_safe assumptions about the victim. That is emergent-behaviour gold for a rage system and costs nothing to reproduce.
4. There is no explicit asymmetric/keep-right rule (no European keep-right bias) in this implementation.

## Unity reference patterns worth copying (and what is dated)

### Kink3d/SimpleTraffic (MIT, last push 2019-05-01, Unity 2018-era, NavMeshComponents vendored into Assets/)

Worth copying:
- **Graph-of-sections, not a pathfinder.** `Vehicle.Initialize(NavSection, NavConnection)` sets `agent.destination` to the *next connection transform* only. On `OnTriggerEnter` with tag `"RoadConnection"`, `SwitchRoad()` deregisters from the old section, re-registers on the new one, re-reads `speedLimit`, and retargets. The NavMeshAgent never plans a long route — it plans one hop. That is what keeps agents on lanes without lane maths.
- **Per-section speed limits applied as `Mathf.Min(navSection.speedLimit, maxSpeed)`** on every section switch (Vehicle.cs:31, 108). Simple, and the natural hook for a rage multiplier.
- **Sections keep a vehicle registry** (`section.RegisterVehicle(this, isAdd)`), which is the cheap spatial index you need for "who is my leader" without a global O(n²) scan.
- **Junctions as phase machines over trigger volumes.** `Junction.Phase` holds `positiveZones/negativeZones` (WaitZone) and matching `TrafficLight[]`. `Enable()` opens the positive zones and closes the negative; `End()` closes the positive ones at 50% of `phaseInterval` — that is the amber, implemented in 3 lines with no extra state. `PhaseType.OnDemand` + `JunctionTrigger` gives sensor-actuated lights.
- **Waiting is a pull, not a push**: the agent stores `m_CurrentWaitZone` on trigger enter and polls `waitZone.canPass` each Update. The junction never has to find and notify its vehicles.

What is dated / do not import:
- `Assets/NavMeshComponents/` is a vendored copy of the pre-package NavMeshComponents (`NavMeshSurface.cs`, `NavMeshLink.cs`, `NavMeshModifier*.cs`) with their own Editor scripts. In Unity 6 these are the `com.unity.ai.navigation` package and the vendored copies will conflict. Delete that folder on any import attempt.
- Car-following is `Physics.Raycast` forward with `blockedDistance = 0.25f` and a tag check for `"Gib"`/`"Unit"` — a boolean stop, not a following model. Stopping is `agent.velocity = Vector3.zero` every frame, which snaps to a halt with no deceleration profile. **This is exactly the layer the IDM work replaces.**
- String tag comparisons (`col.tag == "WaitZone"`) in `OnTriggerEnter` and a `TrafficSystem.Instance` singleton reached from `OnDrawGizmos`. 2018 idiom; use `CompareTag` at minimum.
- Blocked-check runs a raycast **every Update for every vehicle** with no gating and no layer mask (`Physics.Raycast(front.position, forward, out hit)` — all layers). Does not scale.

### mchrbn/unity-traffic-simulation (MIT via README, last push 2022-08-08, 291 stars)

Worth copying:
- **The three-state `Status {GO, STOP, SLOW_DOWN}` enum as the interface between the junction layer and the driving layer** (VehicleAI.cs:22–26). Intersections only ever write `vehicleAI.vehicleStatus`; the driving code alone decides what that means in throttle/brake. Clean seam, and it is exactly where a rage modifier would sit.
- **Stop-sign arbitration without a scheduler** (Intersection.cs:67–98): two lists (`vehiclesQueue`, `vehiclesInIntersection`) plus a `prioritySegments` list. Priority-segment arrivals are always admitted at SLOW_DOWN. Non-priority arrivals are admitted only if *both* lists are empty, else queued at STOP. `ExitStop` removes the vehicle and releases `vehiclesQueue[0]`. ~30 lines for working unsignalised-intersection priority.
- **Turn anticipation via the *future* waypoint**: `futureSteering` is computed from the segment between the current and next-next waypoint; if `|futureSteering| > 0.3`, `maxSpeed` is clamped to `steeringSpeedMax` *before* the corner (VehicleAI.cs:119–142). Cheap corner slowdown without curvature maths.
- **`waypointThresh = 6`** as a deliberate early-arrival radius so the vehicle commits to the next waypoint before reaching the current one — prevents the zig-zag that exact-arrival waypoint following produces.
- **Unstick behaviour**: when a detected vehicle is close and *facing the wrong way* (`dotFront <= 0.8`), it reverses at `acc = -0.3` and steers away based on `dot(forward, other.right)` (lines 174–187). Crude but it resolves the deadlocks a pure stop-when-blocked model creates.

What is dated / weak:
- Car-following is a 6-ray fan with two magic distances (`emergencyBrakeThresh = 2f`, `slowDownThresh = 4f`) and discrete throttle values (`acc = 1 / .5 / .3 / 0`). No headway concept, so behaviour is speed-independent — the same 2 m threshold at 10 km/h and 90 km/h.
- `GetDetectedObstacles` has a real bug worth not copying: it tracks `minDist` but `break`s on the first closer hit, and returns `hitDist` from the *last* `CastRay` call rather than from the chosen obstacle (lines 228–248).
- Traffic lights use `InvokeRepeating("SwitchLights", ...)` with a string method name and a nested `Invoke` for the amber delay — works, but unserialisable and awkward to pause.
- Next segment is chosen by `Random.Range` over `nextSegments` (line 274–279) — no routing, no destination.
- Unity 2019/2020-era project; `Packages/` present but no evidence of Unity 6 compatibility. Same conclusion as round 1: **read it, do not import it.**

## Licensing status of each reference

| Repo | Licence evidence | Verdict |
|---|---|---|
| Farama-Foundation/HighwayEnv | `LICENSE` at root, MIT, "Copyright (c) 2018 Edouard Leurent / Copyright (c) 2023 Farama Foundation" — read directly | **Clean MIT.** Free to read, port to C#, and ship, provided the copyright notice + permission text ride along. Porting Python→C# produces a derivative work; keep the attribution. |
| Kink3d/SimpleTraffic | `LICENSE` present in root tree listing; GitHub API `license.spdx_id = "MIT"` | **Clean MIT.** Readable and portable. Note the vendored `Assets/NavMeshComponents/` is Unity's own code under its own terms — do not carry it over. |
| mchrbn/unity-traffic-simulation | GitHub API `"license": null`; recursive tree listing shows **no file matching `licen*` anywhere**; README lines 60–68 contain the **full MIT text** with "Copyright (c) 2019 Matthieu Cherubini" | **Round 1's hazard flag is largely refuted.** MIT does not require a file named LICENSE — it requires the notice and permission text, and the complete text is in the README with a named copyright holder. GitHub's detector simply does not scan README bodies. This is a valid grant; reading and reimplementing is safe, and even importing is permitted under MIT terms. Lowest-effort mitigation if the solo dev wants belt-and-braces: keep a copy of those README lines alongside any derived file. |

## What I looked for and could NOT find

- **A calibrated aggressive-vs-timid parameter set in highway-env.** Searched the file that ships the personality classes and read all 583 lines. The personality classes exist but are merge-scenario tuning, not a general aggression model: they never touch `POLITENESS`, they both *raise* the lane-change threshold above base IDM, and they both *raise* time headway above base IDM. The only genuine aggression signal is `k_gap` 0.5 vs 2.0. **Absence of evidence is the finding here: nobody has published the table the requesting project wanted; Table B above is judgement, not measurement.**
- **Any `AggressiveVehicle` override of `TIME_WANTED`, `DISTANCE_WANTED`, `COMFORT_ACC_MAX/MIN`, or `POLITENESS`.** Confirmed absent — lines 562–583 are the complete class bodies, 6 and 6 lines respectively.
- **Lane-change / overtaking logic in either Unity reference.** Neither SimpleTraffic nor mchrbn implements any lane-change decision at all. SimpleTraffic vehicles are pinned to a NavMesh section; mchrbn vehicles follow a fixed waypoint chain and pick the next *segment* at random. **The MOBIL layer has no Unity precedent in these references — it has to come from the highway-env port.**
- **Any junction *priority* model richer than "queue + a priority-segment list".** mchrbn's is the more capable of the two and it is still first-come-first-served with a manually authored priority list. No gap-acceptance, no unprotected-left-turn logic, in either repo.
- **A Unity or Godot open-source project implementing IDM directly (target 4).** **Not investigated** — the tool budget was consumed by the three primary targets. This is an unexamined gap, not a negative result. Recommend one focused round-3 query if a game-engine IDM worked example is still wanted.
