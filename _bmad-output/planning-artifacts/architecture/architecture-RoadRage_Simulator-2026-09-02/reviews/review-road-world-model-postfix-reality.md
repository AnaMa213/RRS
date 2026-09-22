# Road World Model reality-check review (post-fix rerun)

**Scope:** proposed AD-43 through AD-47 in `ARCHITECTURE-SPINE.md` and `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`, after the reviewer corrections. Supersedes `review-road-world-model-reality.md`, which described the pre-fix draft.  
**Lens:** separate inspected V1 reality from proposed V2 contracts; verify present-tense facts, counts, dimensions and tolerances against the current repository.  
**Verdict:** **PASS.** Both required revisions from the first run are applied, and every present-tense claim introduced or changed since then is repository-backed. Static repository check; the future importer/compiler does not exist and this phase does not implement it.

## Required revisions — closure check

### MEDIUM-1 — evidence classes are now separated — Closed

The corrected text states that existing EditMode guards verify no orphan connectors, that all four entries reach an exit, and that the central four-way intersection has twelve legal choices; it states explicitly that those guards do **not** enumerate every `LaneGraph` child and do **not** fail on an unrecognised road-module source, so they do not prove that all module instances come from the five inspected greybox prefabs. The 25-module/204-node inventory is attributed to the 2026-09-22 static scene audit, and source-set exclusivity becomes a declared, validated property of the migration report — an unknown module prefab or a loose lane-node hierarchy is a hard failure.

Independently re-verified in this rerun:

- `Story510LaneGraphAndRoutedTrafficTests.MvpRunDistrictCarriesTwoWayRoadsJunctionsAndTunnelPortals` asserts `avenues >= 2` (not twelve), `crossroads == 1`, `tees == 4`, `roundabouts == 4`, `tunnels == 4`, and ties the entry/exit portal counts to the tunnel count. It therefore fixes today's topology without guarding source exclusivity.
- `CountModules(root, prefabPath)` counts only instances whose source asset path equals the path handed to it, so any sixth module prefab or loose lane-node hierarchy is invisible to it.
- The 25-module/204-node arithmetic is corroborated independently by the 2026-09-18 Story 5.11 recon note in the repository memory: 12×6 + 12 + 4×9 + 4×17 + 4×4 = 204.

### MEDIUM-2 — the geometry gates are labelled provisional — Closed

The table now reads "Initial `MVP_Run` validation targets are **proposed V2 acceptance gates, not measured V1 behaviour**", and a following paragraph names the single inherited value and its mechanism: `TrafficSettingsDef.connectorJoinDistance` defaults to `0.75f` (`Assets/RoadRage/Features/Vehicles/TrafficSettingsDef.cs:67`, authored asset `0.75`) and is closed by `LaneGraph.JoinConnectors` rejecting `Vector3.Dot(from.forward, to.forward) <= 0f` (`LaneGraph.cs:347`), which accepts any mismatch below `90 degrees`. V1 has no curve compiler, so the chord, seam, tangent, node-drift and portal-drift values are stated as new engineering targets, with an obligation to publish observed maxima/distributions in the first migration run and an explicit owner decision for any relaxation.

## LOW-1 — spline wording — Closed and re-verified

`com.unity.splines` is resolved at `2.9.0`, `depth: 1`, `source: registry` in `Packages/packages-lock.json`, requested by the Cinemachine entry; it is absent from `Packages/manifest.json` as a direct dependency. A search across `Assets/RoadRage` for the Splines API returns no C# source reference; the only matches are Cinemachine serialized fields whose names contain the word (`SplineCurvature`, `m_Curvature`) in prefabs and scenes. The contract now states the transitive presence and that "not bound" must not be shortened to "not installed".

## Facts re-checked in this rerun

| Claim | Verified state |
| --- | --- |
| Two-way module dimensions | `Greybox_RoadSegment_TwoWay.prefab` roadway collider `8 × 0.2 × 16` (`:154`) and two `4 × 0.2 × 16` sidewalk colliders (`:293`, `:630`). |
| Vehicle scale premise | AI box `2.06 m` wide, lane references at ±2 m, so a 4 m candidate lane satisfies a 0.25 m per-side clearance gate. |
| Turn weights and choices | Crossroads `30/50/20` with twelve non-U-turn choices; T junction six; roundabout `60/40` exit/continue. |
| Portals | Four tunnel portals per direction, portal counts tied to the tunnel count by the EditMode guard. |
| Absent V1 semantics | No stable semantic road IDs, authoritative lane widths, legal adjacency, stop/yield lines, conflicts, right-of-way controls or signal plans in `Assets/RoadRage`. |
| RoadRage source mutation | None. This architecture run changed planning/review artifacts only. |

## Claims introduced since the first run, and their status

`Bounds` envelope inclusion, movement-owned width profile, turn-weight polarity, flag-interaction wording, smoothing-override reporting and the maximum-supported-vehicle-footprint owner are **contract proposals**, not repository claims. The only new present-tense repository claim is the transitively resolved spline package consumed by Cinemachine, verified above. No new technology, version or package dependency is introduced, so no web check is required.

## Residual caveat for downstream artifacts

No artifact may present the geometry gates as measured parity, and none may state that the architecture gate has passed before owner acceptance. Until the first migration report publishes observed maxima, the numbers remain proposed acceptance targets.

**Status note (added after the acceptance turn):** the owner accepted AD-43 through AD-47 on 2026-09-22 with two clarifications; see `review-road-world-model-clarification-rerun.md` for the post-acceptance rerun and the final state. The gates remain proposed acceptance targets — acceptance does not measure them.
