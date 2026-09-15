# Digest — D4 Host-authoritative NGO integration at scale (round 1)

Scope: ~30 networked AI vehicles, NGO 2.13.2, host-authoritative, Rigidbody + NetworkTransform, 4 players.
Budget spent: 8 sources read, 13 tool calls. Accessed 2026-09-15.

**Version caveat stated up front:** the NGO manual pages I could actually read resolve to the **2.5.1** manual build (`com.unity.netcode.gameobjects@2.5/manual/...`). The package landing page confirms **2.13.2** is current, but I did not read 2.13-specific manual text for NetworkTransform or object visibility. Everything below marked NGO 2.x is verified at 2.5.1 and is stated as "unverified at 2.13.2" where it matters. The old `docs-multiplayer.unity3d.com` URLs now 301-redirect to `docs.unity3d.com/Packages/com.unity.netcode.gameobjects@latest/` — any third-party guide still linking docs-multiplayer is pointing at a dead host.

---

## Claims

**NGO's default network tick rate is 30, and NetworkTransform instances are distributed across those tick slots** — https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.5/manual/components/helper/networktransform.html | Unity Technologies | NGO 2.5.1 manual | accessed 2026-09-15 | confidence: high | class: perf-number
> "If you're using the default `NetworkConfig.TickRate` value (30), then there are 30 'tick slots' that each NetworkTransform instance is distributed amongst."

**Half-float precision converts each axis from a 4-byte float to a 2-byte half-float, with a documented hard limit of 64 world units of delta per update** — same URL | Unity | NGO 2.5.1 | accessed 2026-09-15 | confidence: high | class: perf-number
> "The maximum delta per update should not exceed 64 Unity world space units."
At 30 ticks/s a vehicle would need to be moving >1920 u/s to breach this. Irrelevant constraint for city traffic.

**Quaternion synchronization costs 16 bytes per update; quaternion compression reduces that to 4 bytes via a smallest-three algorithm** — same URL | Unity | NGO 2.5.1 | accessed 2026-09-15 | confidence: high | class: perf-number

**NetworkTransform exposes per-axis sync toggles for position, rotation and scale, and per-channel thresholds; lowering a threshold raises sync frequency and bandwidth per instance** — same URL | Unity | NGO 2.5.1 | accessed 2026-09-15 | confidence: high | class: integration-pattern
> "Increasing the threshold resolution (by lowering the position threshold value) increases the potential frequency of when the object's position will be synchronized (and will increase the bandwidth cost per instance)."

**NGO 2.x NetworkTransform is server-authoritative by default; owner-authoritative and distributed-authority are alternative modes** — same URL | Unity | NGO 2.5.1 | accessed 2026-09-15 | confidence: high | class: integration-pattern. The separate `ClientNetworkTransform` sample class of the NGO 1.x era is superseded by the built-in authority mode selector — **do not cite 1.x-era ClientNetworkTransform advice for this project**; it is also irrelevant here since AI vehicles are host-owned by design.

**NGO 2.x ships distance-based relevancy as a documented pattern via `NetworkObject.CheckObjectVisibility`, with a worked distance example and per-tick re-evaluation** — https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.5/manual/basics/object-visibility.html | Unity | NGO 2.5.1 | accessed 2026-09-15 | confidence: high | class: integration-pattern
> "If `NetworkObject.CheckObjectVisibility` isn't assigned, then Netcode for GameObjects assumes it's visible to all clients."
Example uses a `VisibilityDistance` of 5.0f, compares against the client's PlayerObject position, is server-side only, subscribes in `OnNetworkSpawn()`, and is re-checked per network tick. Runtime control: `NetworkShow(clientId)` / `NetworkHide(clientId)`, a static batch `NetworkShow(networkObjects, clientId)`, and `SpawnWithObservers = false` to spawn with no initial observers.

**Unity staff place the "rethink your architecture" line in the hundreds-to-1000+ NetworkObject range, not at tens** — https://discussions.unity.com/t/how-many-network-objects-in-a-scene-is-too-many/870805 | Unity Discussions, NoelStephens_Unity (Unity staff) | posts Nov 2023 and Jun 2025 | accessed 2026-09-15 | confidence: medium-high | class: perf-number
> "If you are in the 100's upwards of 1000+, then you need to think of alternate approaches"
Same thread, community members: a NetworkObject-only prefab scaled to ~700 objects, but a prefab with a Collider and gravity to only ~200; "NetworkUpdate takes about 20% of my CPU time" at several hundred objects; a report of "100-300 projectiles use more than 2000 bytes per second EACH per change in position values"; and a stated practical envelope of "about 1 mb reliable data per second to each client". Class: perf-number, single-thread community reports — **treat the byte figures as order-of-magnitude, not as measurements I can reproduce**.

**Unity's own AI Navigation docs state that NavMeshAgent and Rigidbody both trying to move the object is undefined behaviour, and that the Rigidbody must be kinematic** — https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/MixingComponents.html | Unity | AI Navigation 2.0 | accessed 2026-09-15 | confidence: high | class: known-failure
> "Both components may try to move the agent at the same time which leads to undefined behavior" · "Turn on kinematic (Is Kinematic) - this is important!"
Also: "You don't need to add physics colliders to NavMesh Agents for them to avoid each other."

**Unity documents the "information flows in one direction" rule and the `updatePosition`/`updateRotation` = false escape hatch** — same URL | Unity | AI Navigation 2.0 | accessed 2026-09-15 | confidence: high | class: integration-pattern
> "Information should always flow in one direction" · "Disable NavMeshAgent.updatePosition and NavMeshAgent.updateRotation to detach the simulation from the game objects locations"
This is the documented pattern for "agent decides where, something else moves the transform".

**NavMeshAgent and NavMeshObstacle on the same object is a documented failure mode, made worse by carving** — same URL | Unity | AI Navigation 2.0 | accessed 2026-09-15 | confidence: high | class: known-failure
> "Do not mix well! Enabling both will make the agent trying to avoid itself" · "If carving is enabled in addition, the agent tries to constantly remap to the edge of the carved hole" · "Make sure only one of them are active at any given time"

**Carve Only Stationary is documented by Unity as the best-performing carving mode; Carve When Moved is scoped to "large, slowly moving obstacles"** — https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/AboutObstacles.html | Unity | AI Navigation 2.0.14 | accessed 2026-09-15 | confidence: high | class: perf-number/known-failure
> "**Carve Only Stationary** is generally the best choice in terms of performance." · Carve When Moved is "useful for large, slowly moving obstacles (for example, a tank that is being avoided by infantry)." · "There is a one-frame delay between changing a NavMesh Obstacle and the effect that change has on the NavMesh."

**Practitioners shipping vehicle AI on NavMesh report NavMesh has no turning-radius concept and steer a physics car from a dummy agent instead** — https://discussions.unity.com/t/navmesh-with-vehicles-a-navmesh-agent-with-a-turning-radius/704650 | Unity Discussions, MD_Reptile (Jun & Sep 2018), Disorganized (Sep 2019) | accessed 2026-09-15 | confidence: medium | class: integration-pattern/known-failure
> "use a dummy navmesh agent that can pathfind through a city, and have the dummy followed by a physics driven car" · "the actual path is the same as it would be for a regular old navmesh agent, and this doesn't consider that a vehicle might be travelling too fast or just can't turn tight enough to make a certain angle of a turn" · "the agent will hit the edges of the navmesh sometimes, and will kind of slide slowly until it rotates enough"
Workarounds they landed on: inflate agent radius so tight turns are never pathed ("prevented the AI from going down paths that are just too tight for it to turn smoothly into"), plus "raycasting for avoidance that overrides whatever the pathfinding decides". A second poster abandoned NavMesh for "rails...just 'pre-set' the turns at each intersection so they stay in the correct lane". Waypoints were judged "not robust enough and the driver can't go offroad or take shortcuts around traffic very well". Age: 2018–2019, outside the ≤2-year pattern freshness bar — flagged as dated, though nothing in NavMesh's kinematic model has changed since.

**A widely-cited "15 NavMeshAgents cost 70% of my framerate" report does not hold up on inspection** — https://discussions.unity.com/t/navmesh-agent-make-big-performance-impact/248175 | Unity Discussions, CoughE / James-YangDan, Sep 2021 | accessed 2026-09-15 | confidence: medium | class: perf-number
Reported 80-90 FPS with 15 agents vs 270-300 with them disabled, but the thread diagnosis attributes it to rendering and animation, not pathfinding: "your rendering thread time-consuming has also doubled", "600k tris" drawn at once, and the conclusion "There seems to be more influencing it here". **This is a good example of why a headline agent-count number must be read past the first post.** Fixes offered were layer culling distances, lower-poly LODs, and reducing `NavMeshAgent.quality` for distant agents.

---

## Replication cost and the knobs that reduce it

Documented knobs, in descending order of payoff for this project:

1. **Turn off what you don't sync.** Scale is almost certainly constant on AI cars — disable all three scale axes. Rotation on a ground vehicle is meaningfully only yaw; disabling pitch/roll sync (or enabling quaternion compression instead) is the cheap win. Per-axis toggles exist for position, rotation and scale.
2. **Quaternion compression: 16 bytes → 4 bytes per update** if you sync full quaternions. If you instead sync a single Euler axis, cheaper still.
3. **Half-float precision: 4 bytes → 2 bytes per axis.** Position goes 12 → 6 bytes. The 64-unit-per-update delta cap is unreachable at road speeds.
4. **Position threshold.** Raising it suppresses updates entirely for near-stationary vehicles (queued at lights, parked). Documented to trade precision for per-instance bandwidth directly.
5. **Tick rate.** Default 30. NGO explicitly distributes NetworkTransform instances across tick slots, so the per-tick spike of 30 vehicles is already spread rather than all-on-one-tick.

**Derived (my arithmetic, not a sourced measurement):** position half-float (6 B) + compressed quaternion (4 B) = **10 bytes of payload per vehicle per update**, before NGO message/header overhead. At 30 ticks/s × 30 vehicles that is ~9 KB/s of payload per client, ~27 KB/s upstream from the host for 3 clients — before overhead, which community reports suggest dominates (the "2000 bytes/second each" projectile figure implies ~66 B/update total at 30 Hz, i.e. overhead is several times the payload). Even taking the pessimistic community figure at face value: 30 × 2000 B/s = **60 KB/s per client, ~180 KB/s total upstream** — well inside the ~1 MB/s-per-client envelope that thread describes. **Bandwidth is not the binding constraint at 30 vehicles. Host CPU is the thing to profile.**

The CPU signal worth heeding: the same thread reports "Deserialization of NetworkTransform messages takes up most of the frametime and allocates memory" — that is *client*-side cost and *allocation*, which matters more for frame stability than for the byte budget.

**Interest management is available and documented, but is a poor fit here as a bandwidth measure and a good fit as a CPU/spawn measure.** `CheckObjectVisibility` culls per client — but with 4 players in one shared city, the union of what any player can see is likely most of the traffic. The real use is not hiding cars from players, it is **not spawning traffic that no player is near at all**, using `SpawnWithObservers = false` plus a spawn/despawn ring around the players' collective position. That is an authoring decision, not a NetworkTransform tuning decision.

## NavMeshAgent + Rigidbody: the recommended pattern, evidenced

Unity documents exactly one safe configuration and one escape hatch, and they are the two ends of the same rule ("information should always flow in one direction"):

- **Agent drives, Rigidbody is kinematic.** "Turn on kinematic (Is Kinematic) - this is important!" The Rigidbody exists only for collision/trigger events, not for simulation.
- **Rigidbody drives, agent is detached and used as a path source.** Set `updatePosition = false` and `updateRotation = false`, read `agent.desiredVelocity`/`steeringTarget`, and apply your own forces. Unity documents this pattern for root-motion animation; the vehicle practitioners describe the same shape independently — "use a dummy navmesh agent that can pathfind through a city, and have the dummy followed by a physics driven car".

**For this project the second is the one that fits**, because the existing AI vehicles are already Rigidbody-based and driven host-only in FixedUpdate. Adopting the first would mean discarding the physics driving model. The second preserves it and adds NavMesh purely as a path provider. The third option the brief asked about — `CalculatePath`/`NavMeshPath` with no NavMeshAgent component at all — is the leanest version of the same idea and is the natural endpoint if agent avoidance turns out to fight the vehicle steering; I found the pattern described by practitioners but **did not find an authoritative Unity page recommending it specifically for vehicles**.

**Hard constraint, documented:** never put NavMeshAgent and NavMeshObstacle on the same vehicle. If traffic must path around stopped traffic, the stopped car has to switch modes (agent off / obstacle on) rather than run both. With carving on, running both is explicitly described as causing the agent to "constantly remap to the edge of the carved hole".

**Carving with ~30 moving vehicles: do not do it in Carve When Moved mode.** Unity scopes that mode to "large, slowly moving obstacles" and states Carve Only Stationary "is generally the best choice in terms of performance". A city of 30 constantly-moving cars each re-carving the NavMesh is the exact anti-pattern. Vehicle-to-vehicle avoidance should come from agent avoidance priority, raycast override (as the practitioner thread describes), or a lane/queue model — not from carving. Carving is appropriate only for vehicles that have genuinely stopped (crash site, roadblock), via Carve Only Stationary with a `Carving Time To Stationary`.

## Is ~30 agents anywhere near a GameObject-approach ceiling?

**No, and the evidence says so from both sides.**

- Networking side: the Unity staff threshold for "think of alternate approaches" is stated as "100's upwards of 1000+" NetworkObjects. 30 is an order of magnitude below the low end of that range. The community-reported degradation points (~200 objects with colliders + gravity, ~700 bare NetworkObjects) are 7x-23x above the target.
- Pathfinding side: the one dramatic low-agent-count horror story I found (15 agents, 70% FPS loss) is **explicitly diagnosed in-thread as a rendering/animation problem, not a NavMesh one**. I could not find a credible account of NavMeshAgent itself being the bottleneck at tens of agents.
- The pathfinding ceiling that does appear in the literature sits near 100 agents, not 30: a 2026 arXiv preprint on scalable game agents reports NavMesh `FindPath` saturation as the hard ceiling above ~96 agents, with n ≤ 64 recommended and n ≥ 128 requiring asynchronous batched pathfinding. **Confidence: low-medium — I saw this only in search-result summary, did not read the paper, and its workload (RL NPCs) is not city traffic.** Treat it as a directional lead, not a number to plan against.

So: **~30 agents is not a DOTS argument.** DOTS would be argued for at thousands of agents, and the brief's own constraint (no Entities packages installed) makes that a non-starter anyway. The MonoBehaviour/GameObject approach is the correct rung here, and the cost discipline that matters is ordinary: throttle `SetDestination`/repath frequency (paths, not steering, are the expensive call), lower `NavMeshAgent.quality` for distant agents (documented practitioner fix), and LOD the *rendering and physics* of far vehicles rather than their logic. The "30+ agents" number is comfortably inside the boring zone in every dimension I could evidence.

## Leads worth chasing

- **NGO 2.13 changelog diff against 2.5** for NetworkTransform and visibility. Eight minor versions of drift is real and I did not close it. Specifically: whether NetworkTransform delta compression, tick-slot distribution, or the client-side deserialization allocation complaint changed.
- **`NetworkTransform` source on GitHub** (`Unity-Technologies/com.unity.netcode.gameobjects`, `develop` branch, `Components/NetworkTransform.cs`) — the authoritative answer on exact per-update byte layout, which no manual page states end-to-end.
- **The Boss Room sample / Unity's Multiplayer Tools package** — bandwidth profiler (Network Profiler / RNSM) would give the project real per-object byte numbers in an hour, replacing every estimate in this digest with measurement. This is the single highest-value next action and it is local, not research.
- **The arXiv "One Policy, Infinite NPCs" preprint (2605.23652)** for the NavMesh FindPath saturation curve, if a real pathfinding ceiling number is wanted.
- **`NavMesh.CalculatePath` off the main thread / `NavMeshQuery` + Jobs** — the Experimental `NavMeshQuery` API allows burst-compiled path queries from jobs without Entities. Not investigated this round; relevant only if profiling shows pathing is hot, which at 30 agents it likely won't.

## What I looked for and could NOT find

- **Any official Unity statement of bytes-per-NetworkTransform-update as a single total figure.** The manual gives component deltas (16→4 for quaternion, 4→2 bytes per axis for half-float) but never a "a moving object costs N bytes/tick" number including message overhead. Every total in circulation, mine included, is arithmetic or a community estimate.
- **Any NGO documentation of a supported ceiling on concurrent networked objects.** No number is published. The only figures are community reports in one thread.
- **Any performance or scaling statement in the object-visibility documentation at all.** Unity documents the `CheckObjectVisibility` mechanism and gives a distance example, but says nothing about its cost when evaluated per-tick across many objects × many clients — which is precisely the thing you'd want to know before applying it to 30 vehicles × 4 clients.
- **Post-2019 practitioner accounts of shipping NavMesh vehicle traffic.** Everything substantive I found on the turning-radius problem is 2018-2019. Either the problem is considered solved-by-workaround and nobody writes it up, or teams doing city traffic have moved to spline/lane-graph systems and stopped discussing NavMesh. I could not distinguish these two explanations.
- **Any evidence about interaction between NGO NetworkTransform interpolation and host-side FixedUpdate physics driving** — i.e. whether clients see judder when the host writes transforms in FixedUpdate at a rate unrelated to the 30 Hz tick. This is a real risk for this project's exact architecture and I found nothing addressing it.
- **Confirmation that NGO 2.13.2 behaves as 2.5.1 documents.** Not checked. Flagged as the top open item.
