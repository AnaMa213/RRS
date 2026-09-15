# Digest — D1 Candidate field and screen (round 1)

Run date: 2026-09-15. Budget: 15 tool calls, ~10 sources actually read. All GitHub metrics below come from the GitHub REST search API read live on 2026-09-15 (raw `pushed_at` values), not from README claims.

## Headline

The free/open-source Unity traffic field is **structurally stale**. Every open-source traffic project with meaningful community traction (>40 stars) last received a commit between **2019 and 2023**. The only repositories pushed in the last 6 months are either (a) SUMO co-simulation bridges, (b) DOTS/ECS toolkits, or (c) zero-star personal projects created weeks ago. **No open-source Unity traffic system was found that claims Unity 6 / 6000.x support, and none was found that mentions Netcode for GameObjects.** That absence is the finding.

## Claims

**The AI Navigation package version documented for Unity Editor 6000.6 is 2.0.14; it provides build/use of navmeshes at runtime and edit time, dynamic obstacles, and NavMeshLinks for actions like jumping** — https://docs.unity3d.com/6000.6/Documentation/Manual/com.unity.ai.navigation.html | Unity Technologies | version-stamped for 6000.6 | accessed 2026-09-15 | confidence: high | class: version-compat (two sources: also https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/index.html stating 2.0.14)

**The AI Navigation docs surveyed state no limitation, caveat, or guidance specific to vehicle-like or non-holonomic agents** — https://docs.unity3d.com/6000.6/Documentation/Manual/com.unity.ai.navigation.html | Unity Technologies | accessed 2026-09-15 | confidence: medium (absence-of-evidence on two pages, not an exhaustive doc sweep) | class: landscape

**Splines package 2.8.4 is released for Unity Editor version 6000.6** — https://docs.unity3d.com/Packages/com.unity.splines@2.8/changelog/CHANGELOG.html and https://docs.unity3d.com/6000.6/Documentation/Manual/com.unity.splines.html | Unity Technologies | accessed 2026-09-15 | confidence: high | class: version-compat. Notable in 2.8.x: Burst and Physics module dependencies both became **optional**, and `SplineJobs.EvaluationPosition` is Burst-compiled when Burst is present — i.e. Splines is usable with no extra stack.

**mchrbn/unity-traffic-simulation: 291 stars, last push 2022-08-08T10:25:45Z, GitHub detects NO license file, "Developed / Tested with Unity 2018.3.x and plus"** — https://api.github.com/search/repositories?q=traffic+unity... and https://github.com/mchrbn/unity-traffic-simulation | GitHub / repo author | accessed 2026-09-15 | confidence: high | class: ecosystem + license. The README text asserts MIT (Copyright 2019 Matthieu Cherubini) but the API `license` field is null — **license status is ambiguous and must be verified against the repo's actual LICENSE file before adoption (G1 risk).**

**mchrbn is MonoBehaviour-based: Rigidbody + WheelCollider + WheelDrive/VehicleAI scripts, hand-placed waypoint segments linked at intersections; the author documents intersection linking as "a bit tedious" and requires a top-down orthographic editor view** — https://github.com/mchrbn/unity-traffic-simulation | repo author | accessed 2026-09-15 | confidence: high | class: landscape

**Kink3d/SimpleTraffic: MIT, 275 stars, last push 2019-05-01T06:41:35Z, "Requires Unity 2017.2+", MonoBehaviour + NavMesh Components; constrains NavMeshAgents inside lanes with custom navigation sections, plus junction/crossing logic for cars, pedestrians and trains** — https://github.com/Kink3d/SimpleTraffic + GitHub API | GitHub / repo author | accessed 2026-09-15 | confidence: high | class: ecosystem. Seven years without a commit; predates the com.unity.ai.navigation package split.

**SUMO2Unity (SimuTraffX-Lab): 87 stars, pushed 2026-09-05T18:52:23Z — the only actively maintained traffic-related Unity repo with real traction — but GitHub detects no license** — https://api.github.com/search/repositories?q=traffic+unity... | GitHub | accessed 2026-09-15 | confidence: high | class: ecosystem + license

**tetreum/peque-traffic is explicitly ARCHIVED on GitHub (17 stars, last push 2021-10-15)** — GitHub API | accessed 2026-09-15 | confidence: high | class: ecosystem. A documented dead end.

**Other open-source traffic repos and their last push: DarraghMac97 SUMO bridge 2019-09-10; imaxs/CityBuilder-and-Traffic-System 2021-05-12; BMEAutomatedDrive SUMO-Unity3D 2019-11-12; Blissgig/Easy-Road-3D-ECS-Traffic 2023-07-10; SunnyValleyStudio Simple-Traffic-System 2020-12-10; oddmax/unity-dots-traffic-simulation 2021-09-06; pekaram/Traffic-Toolkit-DOTS 2025-07-23; sneumeier/OpenROUTS3D 2020-07-01** — GitHub API | accessed 2026-09-15 | confidence: high | class: ecosystem

**Gley Mobile Traffic System (paid Asset Store) states Unity 2021.3 LTS and above, "Unity 6 compatible", ships complete commented C# source, and runs on any platform supporting the Burst compiler (Burst, not Entities/DOTS)** — https://gley.gitbook.io/mobile-traffic-system | Gley (vendor) | accessed 2026-09-15 | confidence: medium — **SINGLE-SOURCED and vendor-published**; the Unity 6 compatibility claim needs an independent confirmation | class: version-compat

**Gley MTS features waypoint pathfinding, overtaking, lane changing, traffic lights and roundabouts, custom events on waypoints, and documents "override the vehicle behavior from the API" plus delegates for some actions; no multiplayer/networking support and no vehicle-count ceiling is documented** — https://gley.gitbook.io/mobile-traffic-system | Gley | accessed 2026-09-15 | confidence: medium | class: landscape

**Megacity Metro is Unity Technologies' official large-scale multiplayer sample: server-authoritative with prediction/interpolation/lag compensation, built on DOTS packages and Netcode for ENTITIES (not for GameObjects), 128–150 players, URP + Entities Graphics** — https://github.com/Unity-Technologies/megacity-metro and https://unity.com/demos/megacity-competitive-action-sample | Unity Technologies | accessed 2026-09-15 | confidence: medium-high (read via search result summaries of the official README/product page, not a full page fetch) | class: landscape

**EasyRoads3D Pro is priced at $45 on the vendor's own site; the vendor page does not state Unity 6 support or clarify free-vs-Pro feature limits in the fetched content** — https://easyroads3d.com/tools.php | AndaSoft/EasyRoads3D | accessed 2026-09-15 | confidence: low — page fetch returned thin content; price single-sourced; Asset Store listing not read | class: license/landscape

## Candidate table

| Candidate | Type | License | Cost | Unity 6 support (evidenced?) | Last release/commit | Source open? | Screen verdict + why |
|---|---|---|---|---|---|---|---|
| **Gley Mobile Traffic System** (Asset Store) | MonoBehaviour + optional Burst | Unity Asset Store EULA (proprietary, per-seat) | Paid — price NOT verified this run | Vendor says "Unity 6 compatible", Unity 2021.3+ — **single-sourced, vendor** | Actively sold/maintained (v3, 2025 listings) | Yes — full commented C# ships with the asset | **Strongest "adapt" candidate, but needs G1 human approval** (paid + proprietary EULA). Passes G5 (real source), G2 tentatively. G3/G4 unknown: no networking story at all, so host-authoritative wiring is the buyer's problem. Its documented behavior-override API + waypoint events are the hook rage/fear would need. |
| **mchrbn/unity-traffic-simulation** | MonoBehaviour, waypoints, WheelCollider | **Ambiguous** — README says MIT, GitHub detects no license file | Free | No. Claims Unity 2018.3+; 4 years untouched | 2022-08-08 | Yes | **Reference / fork-and-own.** Small enough to read fully and absorb; too old and too thin to "adopt" as a system. License must be verified before any code is copied (G1 risk). |
| **Kink3d/SimpleTraffic** | MonoBehaviour + NavMesh Components | MIT (clean) | Free | No. Unity 2017.2+; 7 years untouched | 2019-05-01 | Yes | **Reference only — architecture reference of high value**: it is the best-documented public example of constraining NavMeshAgents to lanes with junction logic, which is exactly the native-blocks path. Would not run unmodified on Unity 6. |
| **Unity AI Navigation (com.unity.ai.navigation 2.0.14)** | Native package, MonoBehaviour-friendly | Unity package (UCL) | Free with Editor | **Yes — version-stamped for 6000.6** | 2.0.14 current for 6000.6 | Read-only package (needle-mirror source is readable, not editable in place) | **Passes all gates as a building block.** Owns pathfinding only; your code keeps every decision. Risk: agents are humanoid-shaped abstractions — no documented vehicle/non-holonomic guidance found, so lane discipline and car-like steering are yours to write. |
| **Unity Splines (com.unity.splines 2.8.4)** | Native package | Unity package (UCL) | Free with Editor | **Yes — version-stamped for 6000.6** | 2.8.4 | Read-only package | **Passes all gates.** Burst and Physics dependencies now optional — no extra stack. The natural lane-graph geometry carrier if you author lanes yourself. |
| **SUMO2Unity** | External co-simulation bridge (SUMO process) | **None detected** | Free | Not stated | 2026-09-05 (active) | Yes | **CUT on G4 and G5-in-spirit**: SUMO owns vehicle motion and routing in an external process, so rage/fear could never reach the decision layer without fighting the bridge. Also unlicensed = unusable (G1). |
| **pekaram/Traffic-Toolkit-DOTS** | DOTS/Entities | None detected | Free | Not stated | 2025-07-23 | Yes | **CUT**: DOTS stack adoption for ~30 vehicles is disproportionate; no license. |
| **oddmax/unity-dots-traffic-simulation** | DOTS | None detected | Free | No | 2021-09-06 | Yes | **CUT**: DOTS + 5 years stale. |
| **Blissgig/Easy-Road-3D-ECS-Traffic** | DOTS/ECS reading EasyRoads3D data | None detected | Free | No | 2023-07-10 | Yes | **CUT as a system**; mildly interesting as proof that EasyRoads3D road data is machine-readable for lane graphs. |
| **tetreum/peque-traffic** | MonoBehaviour | None | Free | No | 2021-10-15, **ARCHIVED** | Yes | **CUT — explicitly dead**, author-archived. |
| **sneumeier/OpenROUTS3D** | Driving-sim project (OSM-based) | "Other" | Free | No | 2020-07-01 | Yes | **CUT**: 6 years stale, whole-application shaped, not a component. |
| **EasyRoads3D Free v3** (road authoring tool) | Editor tool, MonoBehaviour | Asset Store EULA, free tier | Free tier; Pro $45 (vendor site, single-sourced) | **Not evidenced this run** | Actively sold | Unverified | **Kept as a lead, not screened in.** Free-vs-Pro boundary (intersections, side objects, road data via scripting API) and Unity 6 support were NOT confirmed from a primary source. |
| **Megacity Metro** (official Unity sample) | DOTS + Netcode for **Entities** | Unity sample license | Free | Unity-maintained sample | Actively maintained by Unity | Yes | **Reference only.** Wrong netcode family (Entities, not GameObjects) and wrong stack, but it is Unity's own worked example of server-authoritative networked vehicles. |
| **innocentmiau/MiHordeTraffic** (wildcard) | Flow-field pathfinding, MIT | MIT | Free | Not stated | Created 2026-09-01, pushed 2026-09-12 | Yes | **Wildcard.** Brand new, 0 stars, unproven, "thousands of agents / congestion-aware routing". Flow fields are a genuinely different decision substrate (a cheap per-agent field lookup is trivially perturbable by a rage scalar). Worth 30 minutes of reading; zero grounds to depend on it. |
| **furic/ambient-traffic-jam** (wildcard) | MonoBehaviour, URP, "cosmetic" | None detected | Free | URP named; Unity 6 not evidenced | 2026-08-27 | Yes | **CUT for this decision**: self-described *cosmetic* background traffic — by construction it has no decision layer to hook. |

## Leads worth chasing

1. **Gley Mobile Traffic System is the decision.** It is the only candidate that is simultaneously maintained, source-included, MonoBehaviour-based, and feature-complete (overtaking, lane change, lights, roundabouts). Round 2 should get: exact price, exact EULA terms on modification, an **independent** confirmation of Unity 6 compatibility, and — most important for this project — how far the "override the vehicle behavior from the API" hook actually reaches into decisions vs. just speed. Also check the Gley "Urban Traffic & Pedestrian System" / "Traffic System" variants — there appear to be several SKUs and it is unclear which is current.
2. **Verify the mchrbn license.** 291 stars and an MIT claim in the README with no LICENSE file detected by GitHub is a real G1 hazard if any code is lifted.
3. **EasyRoads3D free tier boundary** — unresolved and it matters, because lane-graph authoring is the cost centre. Read the actual Asset Store listing (last-update date + supported Unity versions) rather than the vendor's marketing page.
4. **The native-blocks path deserves a costed estimate**, not just a verdict: AI Navigation 2.0.14 + Splines 2.8.4 both stamped for 6000.6, both free, both gate-clean, and both leave 100% of decision logic in project code. The open question is authoring cost for lanes/intersections, not runtime feasibility at ~30 vehicles.
5. **NavMeshAgent for car-like agents** is unresearched here and is the real technical risk of the native path (non-holonomic motion, lane discipline, local avoidance behaving like crowd-avoidance rather than traffic). Needs its own round.
6. **OpenDRIVE/ASAM importers** were not reached within budget — an explicit gap.

## What I looked for and could NOT find

- **Any open-source Unity traffic system claiming Unity 6 / 6000.x support.** Searched GitHub by stars and by recency; none of the top-20 traffic repos names Unity 6.
- **Any traffic system — free or paid — that documents Netcode for GameObjects support.** Gley's docs mention no networking at all. This means G3 is *nobody's* solved problem: host-authoritative wiring will be project-written regardless of which candidate wins.
- **An official Unity traffic or vehicle-AI sample on the GameObject/MonoBehaviour stack.** The only official traffic-adjacent samples found (Megacity, Megacity Metro) are DOTS + Netcode for Entities.
- **Explicit vehicle-agent limitations in the AI Navigation docs.** Two pages read, nothing stated. Treat as "not documented", not as "no limitations".
- **Gley's price and exact EULA modification terms** — not verified within budget.
- **EasyRoads3D free-tier feature list and Unity 6 statement from a primary source** — vendor page fetch returned thin content.
- **OpenDRIVE/ASAM importers and Unity spline-based road tools other than EasyRoads3D** — out of budget, unexplored.
- **Open-issue trends over time** for any candidate — the API search gives push dates, not issue trajectories.
