# Digest — D3 Routed source/sink traffic, intersections, blocked handling (round 1)

Round 1. Budget spent: 14 tool calls, 8 sources actually read (4 SUMO doc pages, 1 Unity doc page, 1 JAIR paper record, 2 community/search sweeps of low evidential weight). Accessed 2026-09-15.

## Claims

### Routing (source → network → sink)

**SUMO ships a dedicated router, `jtrrouter`, that routes vehicles by *junction turning ratios* instead of origin-destination pairs — vehicles start on source edges, make a stochastic turn decision at each junction, and terminate when they reach a sink edge.** — https://sumo.dlr.de/docs/jtrrouter.html | DLR / Eclipse SUMO | undated (living doc) | accessed 2026-09-15 | confidence: high | class: routing

**`jtrrouter --turn-defaults` defaults to `30,50,20` — 30% left, 50% straight, 20% right — used wherever measured turning data is absent.** — https://sumo.dlr.de/docs/jtrrouter.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: routing | threshold-value

**Route termination in `jtrrouter` is configured, not pathfound: `--sink-edges` names the exits, `--sources-are-sinks` reuses entry edges as exits, `--accept-all-destinations` lets any edge terminate a route, and `--max-edges-factor` (default 2) prunes routes whose edge count exceeds that ratio of the network's edges, preventing infinite wandering.** — https://sumo.dlr.de/docs/jtrrouter.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: routing

**The documented split of responsibility: `duarouter` (OD matrices, shortest path) is for zone-to-zone demand; `jtrrouter` (turning ratios) is for intersection-level demand where empirical turn data exists but no OD matrix does.** — https://sumo.dlr.de/docs/jtrrouter.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: routing

> Relevance: the RRS scenario (enter at tunnel, vary the route, leave at a tunnel) is *literally* the jtrrouter model — sources + per-junction turn probabilities + sink edges. It needs no A*, no OD matrix, and no authored route list. Route variation falls out of the per-junction dice roll; `--sources-are-sinks` covers "exits by the tunnel it came from".

### Density maintenance, insertion, despawn

**When a vehicle cannot be inserted because the source is congested, SUMO does not drop it: "that vehicle is put into an insertion queue and insertion is repeatedly attempted in subsequent simulation steps", producing a recorded `departDelay`.** — https://sumo.dlr.de/docs/Simulation/VehicleInsertion.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: density

**`--max-depart-delay` default is `-1`, i.e. queued vehicles are never discarded for lateness by default.** — https://sumo.dlr.de/docs/sumo.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: density | threshold-value

**Default insertion strategy stops attempting further insertions on an edge after the first vehicle fails on that edge (a performance optimisation in congested networks); `--eager-insert` restores per-vehicle attempts every step.** — https://sumo.dlr.de/docs/Simulation/VehicleInsertion.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: density

**`departLane` accepts `random`, `free`, `best`, `best_prob`; the docs state multi-lane roads reach highest insertion capacity with `best_prob`. `departPos="last"` or `--extrapolate-departpos` removes spacing artifacts caused by the discrete step length.** — https://sumo.dlr.de/docs/Simulation/VehicleInsertion.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: density

**Cities: Skylines despawns vehicles both on pathfinding failure and on being stationary "for a certain length of time"; players report that when an intersection gridlocks, *all* affected vehicles despawn rather than only the blockers, sometimes clearing 20–50% of the city's vehicles at once.** — https://steamcommunity.com/app/255710/discussions/0/1353742967821571324/ and https://forum.paradoxplaza.com/forum/threads/when-vehicles-gridlock-an-intersection-all-affected-vehicles-despawn-instead-of-just-the-ones-causing-the-gridlock.1604674/ | Steam / Paradox community | undated threads | accessed 2026-09-15 | confidence: low (player-observed, no source access, no stated timer) | class: density

> Relevance: this is the exact failure the RRS brief forbids. It is worth recording that a shipped, commercially successful city builder chose mass mid-road despawn as its congestion release valve, and that the community's most-installed correction (No Despawn / TMPE) reveals the gridlock the despawn was hiding. The mechanism is documented nowhere official; treat every number attached to it as unverified.

### Intersections

**SUMO applies a "no-block-heuristic" by default: drivers do not enter a junction when the outbound road is congested. Controlled by `--default.junctions.keep-clear` (default `true`) and per-node/per-connection `keepClear="false"`.** — https://sumo.dlr.de/docs/Simulation/Intersections.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: intersection

**Right-of-way is geometric and rule-based, not reservation-based: prioritised streams cross without slowing, zipper merges adapt speed to neighbours, non-prioritised vehicles wait at the lane end to minimise crossing distance and brake at a configurable visibility distance. Internal junctions let a left-turner wait *inside* the intersection for a gap.** — https://sumo.dlr.de/docs/Simulation/Intersections.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: intersection

**`--no-internal-links` makes vehicles cross junctions instantaneously with no internal-lane dynamics and no blocking constraints — an explicit, documented "cheap intersections" mode.** — https://sumo.dlr.de/docs/Simulation/Intersections.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: intersection

**`--junctions.limit-turn-speed` (default 5.5) derives a turn speed cap from corner radius: `speedLimit = sqrt(radius * factor)`.** — https://sumo.dlr.de/docs/Simulation/Intersections.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: intersection | threshold-value

**Reservation-based intersection management (vehicles request a time-space reservation from an intersection agent; the intersection grants or refuses) is the academic alternative, introduced by Dresner & Stone, *A Multiagent Approach to Autonomous Intersection Management*, JAIR vol. 31 (2008). The authors state the mechanism can emulate a traffic light or a stop sign and therefore subsumes both, and report simulation results significantly outperforming lights and stop signs.** — https://www.jair.org/index.php/jair/article/view/10542 | Journal of AI Research | 2008 | accessed 2026-09-15 | confidence: high for what it claims | class: intersection

> The abstract does not surface an explicit deadlock/starvation proof; that would need the full paper. Flagged as a lead, not a finding.

### Blocked vehicles, jams, and the give-up valve

**SUMO detects gridlock by counting simulation steps in which a vehicle's speed stays below 0.1 m/s; when that accumulated waiting time exceeds `--time-to-teleport`, the vehicle is assumed gridlocked and teleported to the next available edge.** — https://sumo.dlr.de/docs/Simulation/Why_Vehicles_are_teleporting.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: blocked-handling | threshold-value

**`--time-to-teleport` default is 300 seconds; a negative value disables gridlock teleporting entirely.** — https://sumo.dlr.de/docs/Simulation/Why_Vehicles_are_teleporting.html **and independently** https://sumo.dlr.de/docs/sumo.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high (two documentation pages agree) | class: threshold-value

**A teleported vehicle is not deleted: it enters a "teleporting-buffer", stays invisible while virtually traversing the edge at average speed, and reinsertion is attempted at each subsequent edge until it succeeds or the route ends. `--time-to-teleport.remove` instead removes it outright.** — https://sumo.dlr.de/docs/Simulation/Why_Vehicles_are_teleporting.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: blocked-handling

**Other teleport triggers and tiers: `--time-to-teleport.highways` (earlier teleport on fast roads, with `--time-to-teleport.highways.min-speed` default 69 km/h), `--time-to-teleport.disconnected` (route disconnected), `--time-to-teleport.bidi`, `--time-to-teleport.railsignal-deadlock`, `--time-to-teleport.remove-constraint` (resolve rail signal deadlocks), `--time-to-teleport.ride`. `--collision.action` defaults to `teleport`.** — https://sumo.dlr.de/docs/Simulation/Why_Vehicles_are_teleporting.html and https://sumo.dlr.de/docs/sumo.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: blocked-handling

**`--ignore-junction-blocker <TIME>` lets vehicles ignore a blocker sitting on a crossing lane after the given waiting time, explicitly documented as modelling the real-world workaround drivers perform. Default `-1` (disabled).** — https://sumo.dlr.de/docs/Simulation/Intersections.html and https://sumo.dlr.de/docs/sumo.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: blocked-handling | threshold-value

**Driver aggression escalates continuously rather than at a cliff: `impatience` = `MAX(0, MIN(1.0, baseImpatience + waitingTime / timeToMaxImpatience))`, where `timeToMaxImpatience` comes from `--time-to-impatience` and defaults to 180 seconds. Base `impatience` attribute defaults to 0.0 and can be set to `"off"`.** — https://sumo.dlr.de/docs/Definition_of_Vehicles,_Vehicle_Types,_and_Routes.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: threshold-value

**Junction-model knobs with defaults: `jmIgnoreKeepClearTime` = -1 (always keep the junction clear), `jmIgnoreFoeProb` = 0 (never ignore a priority vehicle), `jmIgnoreFoeSpeed` = 0 m/s, `jmTimegapMinor` = 1 s (minimum accepted gap when passing ahead of or behind a prioritised vehicle), `jmDriveAfterRedTime` = -1 (never run a red). `--waiting-time-memory` default 100 s bounds the accumulated-waiting-time window these rules read.** — https://sumo.dlr.de/docs/Definition_of_Vehicles,_Vehicle_Types,_and_Routes.html and https://sumo.dlr.de/docs/sumo.html | DLR / Eclipse SUMO | undated | accessed 2026-09-15 | confidence: high | class: threshold-value

### Keeping vehicles in plausible space

**Unity NavMesh supplies 3 built-in area types (Walkable, Not Walkable, Jump) plus up to 29 custom ones; path cost is `distance × areaCost`, all costs must be greater than 1.0, and each agent carries an Area Mask selecting which areas it may use.** — https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/AreasAndCosts.html | Unity Technologies | AI Navigation package 2.0 docs | accessed 2026-09-15 | confidence: high | class: blocked-handling

**Unity's own documentation warns that area costs are a weak instrument: "The effect of the costs on the resulting path can be hard to tune, especially for longer paths", costs should be treated as hints, and paths may visibly detour because nodes sit on polygons of varying size.** — https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/AreasAndCosts.html | Unity Technologies | accessed 2026-09-15 | confidence: high | class: blocked-handling

> Read against the scenario: a high-cost "sidewalk" area does **not** guarantee a car never drives on the pavement — it only biases global path selection, and local avoidance is a separate system. If sidewalks must be inviolable, they must be *off the mask* (Not Walkable for the car agent type), not merely expensive. This is the failure mode the brief asked about, and Unity documents it themselves.

## Timeout / threshold evidence table (answers the 20-second question)

| System | Mechanism | Value | Is it a documented default? | Source |
|---|---|---|---|---|
| SUMO | `--time-to-teleport` — declare gridlocked, teleport out | **300 s** (speed < 0.1 m/s accumulated) | **Yes**, confirmed on two separate doc pages | Why_Vehicles_are_teleporting.html; sumo.html |
| SUMO | `--time-to-impatience` — waiting time to reach max impatience | **180 s** (continuous ramp from t=0, not a cliff) | **Yes** | Definition_of_Vehicles… |
| SUMO | `--waiting-time-memory` — window over which waiting is accumulated | **100 s** | **Yes** | sumo.html |
| SUMO | `jmTimegapMinor` — min accepted gap at a minor-road junction | **1 s** | **Yes** | Definition_of_Vehicles… |
| SUMO | `--ignore-junction-blocker` — drive around a junction blocker after T | **-1 = disabled** by default | **Yes** | Intersections.html; sumo.html |
| SUMO | `jmIgnoreKeepClearTime` — enter a blocked junction after accumulated wait | **-1 = never** | **Yes** | Definition_of_Vehicles… |
| SUMO | `--max-depart-delay` — discard a vehicle that never got inserted | **-1 = never discard** | **Yes** | sumo.html |
| SUMO | `--time-to-teleport.highways.min-speed` — road speed above which early teleport applies | **69 km/h** | **Yes** | Why_Vehicles_are_teleporting.html |
| SUMO | `--junctions.limit-turn-speed` factor | **5.5** (`speed = sqrt(radius × 5.5)`) | **Yes** | Intersections.html |
| Cities: Skylines | stuck/gridlock despawn timer | **not stated anywhere I could reach** | No | Steam/Paradox threads |
| Assorted game "unstuck" cvars surfaced in search (e.g. `ai_attack_unstuck` ≈ 2.75 s, a 3 s player-stuck wait) | recovery nudge | 2.75–3 s | **No** — single-source, unverified, different problem class (an agent stuck on geometry, not a car waiting in traffic) | search snippets only, not fetched |

**Direct answer on the developer's ~20 s guess.** No system I could evidence uses anything close to 20 s as a wait-then-replan threshold, and the reason is structural: the evidenced designs do not have *one* threshold, they have **two tiers plus a continuous ramp**.

1. **Immediate/continuous tier (0 s onward, ~1 s granularity).** Gap acceptance (`jmTimegapMinor` = 1 s) and impatience (`waitingTime / 180`) start acting the instant a vehicle stops. Behaviour degrades smoothly; there is no waiting period during which the driver does nothing.
2. **Give-up tier (180–300 s).** Impatience saturates at 180 s; teleport fires at 300 s. These are *simulation-throughput* valves — "this vehicle is destroying my statistics, remove it" — not believability mechanisms.

So 20 s sits in an evidential no-man's-land: an order of magnitude too *late* to be a behavioural response (SUMO's driver is already fully impatient at 180 s and meaningfully impatient after ~20 s of the ramp, i.e. impatience ≈ 0.11, still mild), and an order of magnitude too *early* to be a give-up valve (SUMO waits 15× longer before doing anything as violent as removing the vehicle). Notably, SUMO's give-up action — teleport — is precisely the "vehicle vanishes mid-road" that the RRS brief forbids, and SUMO only resorts to it after 5 minutes.

The defensible shape for RRS, derived from the above rather than guessed: **replan attempt within a few seconds** of the path being blocked (cheap, invisible, no waiting period needed — it is a graph query), **escalating visible behaviour continuously** (creep, honk, lane-change-to-overtake when a gap is acceptable), and **no removal tier at all** near the player, since removal is the one thing the design has ruled out. A ~20 s "wait, then look for an alternative" gate should be replaced by "replan immediately and continuously; the *animation* of hesitation is what sells it, not an actual timer".

## Deadlock: how it happens and how it is broken

**How it happens.** Two-way roads with unsignalised junctions produce hold-and-wait cycles: vehicle A occupies the junction waiting for its outbound lane to clear, while the vehicle that would clear that lane is itself waiting on A's lane. SUMO treats this as the normal, expected outcome of dense networks — the entire teleport subsystem exists because of it.

**Prevention (cheapest, evidenced).** SUMO's default **no-block heuristic**: never enter a junction unless the outbound road has room. This is enabled by default (`--default.junctions.keep-clear` = true) and is the single highest-leverage rule, because it prevents the *hold* half of hold-and-wait from ever happening. It costs one occupancy check on the exit lane before committing to the crossing.

**Prevention (structural).** Explicit right-of-way per stream (major/minor, zipper merge) plus internal junctions where a left-turner can wait mid-junction without blocking the crossing stream. Priority is decided at network-build time, not at runtime — zero runtime arbitration cost.

**Breaking a deadlock that formed anyway — the documented escalation ladder, in order of increasing violence:**
1. `jmIgnoreKeepClearTime` — after accumulated waiting, allow entering a junction you would normally keep clear (default off).
2. `--ignore-junction-blocker <TIME>` — after T seconds, drive *through* a vehicle blocking a perpendicular lane; SUMO's docs explicitly frame this as modelling a real-world workaround (default off, i.e. -1).
3. `jmIgnoreFoeProb` / `jmIgnoreFoeSpeed` — probabilistically ignore a slow or stopped priority vehicle (both default 0).
4. `--time-to-teleport` = 300 s — remove from the road, virtually traverse, reinsert downstream.
5. `--time-to-teleport.remove` — delete outright.

That every one of steps 1–3 is **disabled by default** while step 4 is **on by default at 300 s** is itself the finding: SUMO's maintainers chose "let it jam for five minutes, then cheat invisibly" over "let drivers bend the rules early". For a *game* the ordering must invert — the cheat at the end is exactly the artefact RRS forbids, so the rule-bending tiers (1–3) have to carry the load, and they must be enabled, not defaulted off.

**Alternative class.** Reservation/ticket-based intersection management (Dresner & Stone 2008) eliminates deadlock by construction — no vehicle enters without a granted time-space slot, so hold-and-wait cannot form. Cost: a per-intersection arbiter agent and a request/grant protocol. For ~30 vehicles and stop-sign-grade rules this is heavier than needed; the SUMO-style keep-clear + priority rules achieve non-deadlock far more cheaply. It becomes interesting only if the game ever wants perfectly smooth signal-free junctions.

## Leads worth chasing

- **Full text of Dresner & Stone 2008** (JAIR is open access) for the explicit deadlock/starvation argument and the FCFS policy details — the abstract alone does not carry it.
- **SUMO `randomTrips.py` and `<flow>` definitions** — the documented mechanism for generating a continuous stream of source→sink trips at a set rate; this is the direct answer to "maintain roughly constant density" and I did not get to read it this round. Expect `--period`, `--fringe-factor` (biases trip start/end to network fringe edges — literally "vehicles come from outside the city").
- **`--fringe-factor` specifically**: strongly suspected to be the exact tunnel/outside-the-city semantics the brief describes. Unverified this round.
- **Epic's MassTraffic / ZoneGraph documentation** — the City Sample doc page body did not render through fetch (only the ToC came back). A search snippet indicates MassTraffic is lane-graph based with per-road-type density and spawner actors, and that City Sample uses separate spawners for crowd, intersections, traffic and parked vehicles. That needs a real read before it can be cited as more than a rumour.
- **Traffic Manager: President Edition (TMPE) source** — an actual open-source implementation of "make a shipped city sim stop despawning stuck vehicles", i.e. the closest thing to a case study in the exact constraint RRS has adopted.
- **SUMO `--device.rerouting.*`** — periodic rerouting on travel-time changes, the mechanism by which a SUMO vehicle avoids a jam rather than waiting in it. Directly relevant to wait-then-replan; unread.

## What I looked for and could NOT find

- **Any documented game-industry timeout for "AI car has been blocked too long".** Searches surfaced only asset-store listings, mod pages and forum anecdotes. The two numbers that appeared (≈2.75 s, 3 s) are single-source, unverifiable, and come from a different problem class (character stuck on geometry). They do **not** meet the two-source bar and must not be used to justify a threshold.
- **Any evidence on how long a stationary AI car feels broken to a player.** No study, no postmortem, no GDC material retrieved. This is a genuine gap: the game-feel number the decision wants does not appear to exist in public literature, and should be settled by playtest, not by citation.
- **Cities: Skylines' actual despawn timer value.** Community consensus that a timer exists; no number from Paradox, no number in the thread I read.
- **Documented use of *reversing out* of a blockage** by any traffic system. Nothing found. SUMO's answer to an impassable blockage is teleport, not reverse. Treat reversing as an unevidenced invention if RRS adopts it.
- **Epic MassTraffic primary documentation body** — fetch returned navigation only.
- **SUMO flow/density-control pages** — not read this round (budget), so every claim about maintaining constant density here rests on insertion-queue behaviour rather than on the flow-rate machinery itself.
