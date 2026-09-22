# Road World Model reality-check review

> **SUPERSEDED — pre-fix record.** This review describes the draft **before** the reviewer corrections. The current verdict lives in `review-road-world-model-postfix-reality.md` (rerun 2026-09-22, verdict PASS). Kept as the history of what the first pass required.

**Scope:** proposed AD-43 through AD-47 in `ARCHITECTURE-SPINE.md` and `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`  
**Lens:** verify present-tense facts, counts, capabilities, gaps, dependencies, dimensions and tolerances against the current repository; distinguish inspected V1 reality from proposed V2 contracts.  
**Verdict:** **PASS WITH REQUIRED REVISION.** The material architectural premises and all stated `MVP_Run` source counts are repository-backed. The only false present-tense claim is that existing EditMode guards prove every road-module instance comes from the five inspected prefabs. The new geometry tolerances are legitimate proposed quality gates, but only one is inherited from V1 and one is dimensionally justified; the others have not been measured against the current map and must remain explicitly provisional until the first migration report.

## Evidence inspected

- `Assets/RoadRage/Features/Vehicles/LaneGraph.cs`, `LaneNode.cs`, `LaneGraphRouting.cs` and `TrafficSettingsDef.cs`.
- `Assets/RoadRage/Tests/EditMode/Story510LaneGraphAndRoutedTrafficTests.cs`.
- `Assets/RoadRage/App/Scenes/MVP_Run.unity` and the five traffic-module prefabs used under its `LaneGraph`.
- `Greybox_AIVehicle.prefab` and `TrafficSettingsDef_Default.asset` for physical width and connector tolerance.
- `Packages/manifest.json`, `Packages/packages-lock.json` and RoadRage source references for the spline-package claim.
- The current architecture spine and detailed Traffic V2 Road World Model contract.

This was a static repository reality check. It did not run the future importer/compiler, because neither exists and this phase explicitly does not implement V2.

## Findings

### MEDIUM-1 — the companion attributes a static inventory fact to tests that do not guard it

**Claim under review**

`ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md:285` says existing EditMode guards verify that “all module instances come from the five inspected greybox prefabs.”

**Repository reality**

- `MvpRunDistrictCarriesTwoWayRoadsJunctionsAndTunnelPortals` counts instances of the five known prefab paths, but only requires at least two road segments and exact counts for one crossroads, four T junctions, four roundabouts and four tunnels (`Story510LaneGraphAndRoutedTrafficTests.cs:289-316`).
- `CountModules` only recognizes a prefab path supplied to it (`:1667-1685`). It does not enumerate every `LaneGraph` child and fail on an unrecognized road-module source.
- The tests over `ModulePrefabPaths` validate the five prefab assets themselves (`:847-877`, `:895-920`); they do not prove scene exclusivity.
- A direct static audit of the current scene does establish the intended current inventory: 12 road segments, one crossroads, four T junctions, four roundabouts and four tunnel portals, with no added or removed GameObjects on those instances. That is evidence for the baseline, but it is not the claimed regression guard.

**Risk**

A sixth traffic-module prefab or a loose lane-node hierarchy could be added later without the stated guard detecting that the 25-module/204-node baseline is stale. The migration manifest would then be generated from an incorrect declared source set.

**Required revision**

Change line 285 to separate the two evidence classes: existing tests guard orphan-free connectivity, entry-to-exit reachability, portal counts and crossroads choices; the 25-module/204-node five-prefab inventory is established by the 2026-09-22 static audit and must become an explicit migration/compiler validation. Do not claim a current test already provides that exclusivity check.

### MEDIUM-2 — most numeric geometry gates are new engineering targets, not measured V1 facts

**Repository reality**

- The `0.75 m` migration candidate threshold is exact: `TrafficSettingsDef.connectorJoinDistance` defaults to `0.75f` (`TrafficSettingsDef.cs:67`) and the authored default asset is `0.75`; `JoinConnectors` tests full 3D separation against that threshold (`LaneGraph.cs:317-340`).
- V1's direction gate is only `Vector3.Dot(from.forward, to.forward) > 0` (`LaneGraph.cs:345-349`), which accepts any mismatch below 90 degrees. It does not establish the proposed 5-degree endpoint gate.
- V1 has no curve compiler and therefore no current chord-error, reviewed-curve drift, seam-gap or portal-drift measurement. The proposed `0.05 m`, `5 degrees`, `0.10 m` and `0.05 m` numbers are not derived from an existing test or report.
- The lateral-clearance premise is grounded: `Greybox_AIVehicle.prefab:65` is `2.0600002 m` wide; `Greybox_RoadSegment_TwoWay.prefab:154` is `8 m` wide and its lane references/connectors are at `x = +/-2 m` (`:29,79,219,388,470,554`). A reviewed 4 m lane gives approximately 0.97 m clearance from centerline to each road edge after the default vehicle half-width, comfortably above 0.25 m. This supports a candidate width only; the source contains no authoritative lane-width field.

**Risk**

AD-45 can be read as if every threshold has already been calibrated against `MVP_Run`. It has not. The first compiler may reveal source seam/tangent residuals that conflict with simultaneous node-drift limits, forcing either an undocumented exception or an architecture edit after story generation.

**Required handling**

Keep these values labelled as **initial/provisional V2 acceptance gates**. The first migration report must publish observed maxima/distributions for source connector gap and angle, compiled seam/chord error, source-node drift and portal drift. Any relaxation requires an explicit architecture/owner decision; it must not happen silently in implementation. No current evidence requires changing the values now.

### LOW-1 — the spline-package statement is accurate only as a no-binding rule, not as package absence

`com.unity.splines` is not a direct dependency in `Packages/manifest.json`, and no RoadRage source file references its namespace or types. It is nevertheless already resolved transitively: Cinemachine requests it in `Packages/packages-lock.json:59-65`, and the lock resolves `com.unity.splines` at `2.9.0` (`:254-263`).

The contract wording at line 153 is acceptable because it says Traffic V2 is **not bound** to that package and a future **direct** dependency needs adoption review. “No new dependency is currently justified” is also accurate. Future documentation must not shorten this to “Splines is not installed”; that would be false and stale. No web check is needed because no V2 decision introduces or pins a new technology/version.

## Verified facts and capability gaps

### Source inventory and topology

The stated baseline arithmetic is exact in the current files:

| Source prefab | Scene instances | `LaneNode` components per prefab | Total |
| --- | ---: | ---: | ---: |
| `Greybox_RoadSegment_TwoWay` | 12 | 6 | 72 |
| `Greybox_Intersection` | 1 | 12 | 12 |
| `Greybox_TJunction` | 4 | 9 | 36 |
| `Greybox_Roundabout` | 4 | 17 | 68 |
| `Greybox_TunnelPortal` | 4 | 4 | 16 |
| **Total** | **25** | — | **204** |

- Each tunnel prefab has one `PortalEntry`, one `PortalExit`, one incoming connector and one outgoing connector, so four instances yield four entry and four exit portals.
- The crossroads has four three-successor decision nodes with `30/50/20` weights, matching twelve non-U-turn choices.
- Each T junction has three two-successor decisions, matching six choices.
- Each roundabout has three exit/continue splits with `60/40` weights and three merges; V1 carries geometry and routing weights, not roundabout right-of-way semantics.
- The current connectivity test rebuilds the graph, requires zero orphan connectors and proves each current entry reaches an exit (`Story510LaneGraphAndRoutedTrafficTests.cs:269-286`). The crossroads test requires four branch nodes with three successors (`:404-446`). These are valid oracle facts, not V2 semantic proof.

### Current V1 representation

- `LaneNode` owns only a role, successor references, parallel turn weights and the entry-reuse flag (`LaneNode.cs:44-58`). Its transform position/forward are the lane reference.
- `LaneGraph` derives durable-looking runtime indices from `GetComponentsInChildren` hierarchy order (`LaneGraph.cs:254-262`), projects nearest-node and nearest-exit selection onto the XZ plane (`:132-175`), and joins connector modules at runtime by distance/direction (`:301-364`).
- There are no V1 fields or runtime owners for stable semantic road IDs, road sections, lane widths, same-direction adjacency, explicit junction movements, stop/yield lines, conflict zones, priority controls, signal plans or signal groups. AD-43 through AD-47 consistently present those as new V2 contracts rather than delivered capabilities.
- Ordinary two-way sections contain one directed reference in each direction. The “no lane-change adjacency in the accepted migration unless geometry changes” rule is therefore grounded and correctly prevents an importer from mistaking opposing lanes for adjacency.

### Dependency and migration claims

- AD-47's one-way importer premise matches V1: authored successor edges and weights can be preserved; connector matches are derived from the recorded 0.75 m/direction rule; portal roles are explicit. Width, adjacency and junction-control semantics cannot be recovered as authoritative data from V1 and correctly require review.
- AD-44's generated 128-bit semantic IDs, import manifest and content-hash model version are entirely new architecture. Current hierarchy indices and string-backed `DefinitionId` do not already satisfy that contract; the documents do not claim they do.
- AD-45's directed 3D corridor corrects a real current limitation: nearest-node localization explicitly zeroes vertical offset, and there is no V1 corridor envelope or heading-aware localization result.
- AD-46's explicit junction/control/conflict/signal model corrects genuine missing capabilities. Current roundabout checks even assert that driver/spawner source contains no `GiveWay`, `Yield`, `Priority` or `Roundabout` rule, so no hidden V1 right-of-way system is being overlooked.

## Per-decision reality map

| Decision | Reality verdict | Repository grounding |
| --- | --- | --- |
| AD-43 | Supported as new logical contract | V1 collapses geometry, topology, roles and turn weights into transforms/arrays; none of the proposed owners exists today. The companion's canonical-owner corrections are target decisions, not claimed current classes. |
| AD-44 | Supported as new identity contract | V1 hierarchy indices are explicitly order-derived. No current road semantic ID/version system is being incorrectly advertised as reusable. |
| AD-45 | Supported, tolerances provisional | The planar-nearest and point-transform limitations are real. The 0.75 m source threshold and vehicle/road dimensions are verified; the curve/seam/tangent/drift thresholds await migration measurements. |
| AD-46 | Supported as missing semantics | No V1 controls, stop lines, conflicts, signal plans or roundabout priority rules exist. Keeping blocked-exit occupancy dynamic is consistent with the target responsibility split. |
| AD-47 | Counts and migration direction supported; evidence wording needs correction | 25 modules, 204 nodes, 4/4 portals, 12 crossroads choices and 6/T are correct. Current tests do not prove five-prefab scene exclusivity. |

## Gate conclusion

Apply MEDIUM-1 before owner acceptance. Preserve the provisional/evidence status required by MEDIUM-2 in AD-45 and in the first migration work package; do not present the tolerances as already measured parity. With those conditions, repository reality supports AD-43 through AD-47 as architectural proposals, and there is no unsupported new package or technology dependency blocking the Road World Model gate.
