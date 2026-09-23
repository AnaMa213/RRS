# Sprint Change Proposal — 2026-09-23

**Subject:** Gate A blocker — the four `MVP_Run` roundabouts cannot hold two vehicles side by side; widen them physically and apply reviewed widths before the Story 5.28 sign-off.

**Trigger:** `deferred-work.md`, section « Bloquant Gate A -- revue d'overlay de la Story 5.28 (decision du proprietaire, 2026-09-23) ».

**Status:** Edit proposals 1–5 approved individually by Kenan (incremental mode), 2026-09-23. Approved by Kenan on 2026-09-23 and applied the same day to `epics.md`, the 5.28 spec, `ARCHITECTURE-SPINE.md`, `V1-BEHAVIORAL-ORACLE.md`, `sprint-status.yaml` and `deferred-work.md`.

**Scope classification:** **Moderate** — one story inserted before a gate, one frozen spec line renegotiated, one recorded exception to the V1 freeze. No epic added, no AD reopened, no MVP scope change.

**Recommended path:** Direct adjustment — insert Story 5.49 between the delivered 5.28 authoring pipeline and the 5.28 sign-off.

---

## 1. Issue summary

### 1.1 Trigger

The owner's overlay review of Story 5.28 (2026-09-23) accepted 20 of 25 module instances, left the central crossroads pending (conflict filtering added to the review window, to be re-reviewed) and **blocked the four roundabouts**. Gate A cannot be signed on a geometry the owner has already judged insufficient and that is about to change.

### 1.2 Problem statement

Neither the V2-modelled ring nor the physical ring offers room for two vehicles to manoeuvre. Traffic V1 already exhibited head-on deadlocks on roundabouts for lack of physical width. The 5.28 pipeline cannot express a wider ring: the importer seeds the ring width from its approach (2.0 m per side), and the frozen 5.28 rule « Largeur divergente » turns any reviewed width that differs from that seed into a hard failure.

Category: technical limitation discovered during review.

### 1.3 Evidence (Editor measurements, 2026-09-23)

| Item | Value |
| --- | --- |
| Normal two-way road | `Col_Roadway` 8.0 m; V2 2 corridors × (2.0 + 2.0) m = 8.0 m |
| Ring, V2 | one corridor at radius 6.0 m, 2.0 / 2.0 m = 4.0 m (radius 4 → 8) |
| Ring, physical | 16 × 16 m paved square, island cylinder radius 3.0 m → 5.0 m at the narrowest |
| Circulation | counter-clockwise, island on the corridor's left |
| Layout | roundabouts at (±32, ±32) on a 16 m grid; two sides meet `Ring_*` `Greybox_RoadSegment_TwoWay` instances (8 in total) whose `Col_Sidewalk_Left/Right` run their full length; two sides open onto empty ground up to the boundary at ±58; the diagonal arm meets the tunnel portal |
| V1 source hash | `V1SourceSet.ComputeSourceHash` covers module path/rotation and lane nodes only — not colliders or visuals. A physical-only change keeps the source hash and the 5.27 lineage byte-identical |
| Validation profile | `MaxVehicleHalfWidthMeters` 1.03, `MaxVehicleLengthMeters` 4.5, `LateralClearanceMarginMeters` 0.25 (AI box measured 2.06 × 4.44 m) |

**Two-footprint clearance** — two maximum footprints side by side at the tightest position, heading tangent, margin `m` on every side: `R_in = r_inner + m + W/2`, `c_in = √((R_in + W/2)² + (L/2)²)`, `R_out = c_in + 2m + W/2`, `c_out = √((R_out + W/2)² + (L/2)²)`, residual `= (r_outer − m) − c_out`.

| Ring | Required outer radius `c_out` | Available `r_outer − m` | Residual |
| --- | --- | --- | --- |
| V2 today (4.0 m, radius 4 → 8) | 9.53 m | 7.75 m | **−1.78 m** |
| Physical today (5.0 m, radius 3 → 8) | 8.63 m | 7.75 m | **−0.88 m** |
| Target (8.0 m, radius 2 → 10) | 7.76 m | 9.75 m | **+1.99 m** |

Options already rejected by the owner during the review: A (2.75 / 2.0 = 4.75 m, +18.75 %, insufficient proof); C (defer to 5.38–5.40).

---

## 2. Impact analysis

### 2.1 Epic impact

Epic 5 remains completable as planned. Gate A moves after one inserted M story. No future epic is invalidated; Epics 6–7 are untouched.

### 2.2 Story impact

| Story | Impact |
| --- | --- |
| 5.28 | stays `in-progress`; pipeline delivered; HALT resumes after 5.49 (regenerate artifacts, re-review the 4 roundabouts and the central crossroads, sign) |
| **5.49 (new)** | physical widening + applied reviewed widths + regeneration of 5.28 artifacts |
| 5.29, 5.30 | unchanged — "5.28 (a valid model)" remains true, since 5.28 completes only after 5.49 |
| 5.35 | unchanged — still one ring corridor, roundabout entry/yield semantics as planned |
| 5.38 – 5.40, 5.42 | benefit — a wide ring leaves room for collision response, recovery, gridlock escalation and in-corridor bypass |

### 2.3 Artifact conflicts

| Artifact | Conflict | Resolution |
| --- | --- | --- |
| PRD | N/A — FR/NFR live in `epics.md` (precedent 2026-09-21) | — |
| `epics.md` | 5.28 non-goal "no road geometry change"; Gate A position; dependency graph; critical path; tables | Proposals 1 and 2 |
| `spec-5-28-…` | frozen matrix line « Largeur divergente »; Design Notes widths; obsolete test requirement | Proposal 3 |
| `ARCHITECTURE-SPINE.md` | AD-36 freeze vs. a change to a prefab shared with V1 | Proposal 4 (supersession note, no AD reopened) |
| `V1-BEHAVIORAL-ORACLE.md` | "Frozen V1 surface" includes the authored V1 data in `MVP_Run` | Proposal 4 (exception note) |
| UX | N/A | — |
| `sprint-status.yaml`, `deferred-work.md` | new story, blocker status | Proposal 5 |

### 2.4 Technical impact

- Prefab `Greybox_Roundabout` (4 instances) — roadway, island, colliders, visuals. Lane nodes untouched.
- Transition to the 8 adjacent `Ring_*` segment instances — chosen in 5.49; shared segment prefab is Ask First.
- 5.28 pipeline — width rule changes from compare to apply; test `ADivergentWidthIsAHardFailureIncludingAnAsymmetricOne` replaced.
- 5.28 artifacts — decisions, model, overlay, report regenerated; `RoadModelVersion` changes.
- V1 — before/after oracle and regression comparison (EditMode + V1 traffic PlayMode suites); the PlayMode runner is single-use per Editor session, so two fresh sessions are needed.

---

## 3. Recommended approach

| Option | Verdict | Effort | Risk |
| --- | --- | --- | --- |
| 1 — Direct adjustment: insert Story 5.49 | **Selected** | M | Medium — shared prefab with frozen V1; sidewalk overlap on 8 segment instances; change to a frozen 5.28 rule |
| 2 — Rollback | Not viable — nothing in 5.28 is wrong; only the width rule is too strict | — | — |
| 3 — Scope reduction (defer to 5.38–5.40, or accept 4.75 m) | Not viable — rejected by the owner at review | — | — |

**Rationale:** Gate A exists to catch exactly this — signing a ring that cannot hold two vehicles would make the model authoritative on a known-bad geometry that 5.29 would immediately route on. The fix is local (one prefab, one pipeline rule, one set of regenerated artifacts), keeps every architecture decision intact, and turns an owner judgment ("too narrow") into a measured, reusable acceptance proof.

**Timeline impact:** the critical path grows by one M story before Gate A.

---

## 4. Detailed change proposals

### 4.1 `epics.md` — new Story 5.49 (Proposal 1, approved)

Inserted after Story 5.28, before Story 5.29.

```markdown
### Story 5.49: Roundabout Ring Widening and Applied Reviewed Widths

**Type:** FOUNDATION · **Boundary:** Physical district geometry and width authoring · **Complexity:** M
**Implements:** AD-45 (width envelope), AD-47 (widths require review), 2026-09-23 course correction

As a solo developer,
I want the four `MVP_Run` roundabouts to offer room for two vehicles to pass on the ring, and V2 to model the ring at that width,
So that vehicles meeting on the ring can manoeuvre instead of locking face to face, and Gate A signs the geometry that will actually be driven.

**Capability delivered:** a physically widened ring on all four roundabout instances (one prefab), meeting the approved target geometry and proven by a two-footprint clearance check; a V2 ring corridor whose reviewed width is *applied* by the 5.28 pipeline rather than merely compared with the importer's seed; regenerated 5.28 artifacts ready for re-review.

**Why here:** Gate A signs the roundabout overlays. Signing a ring known to be too narrow would sign stale data, and 5.29 routes on whatever Gate A accepts.

**Prerequisites:** 5.27 (lineage), 5.28 pipeline (implemented, artifacts provisional, sign-off pending).

**Evidence (Editor, 2026-09-23):** normal road — `Col_Roadway` 8.0 m, V2 2 × (2.0 + 2.0) m; ring — one corridor at radius 6.0 m, V2 2.0 / 2.0 = 4.0 m; physical — 16 × 16 m paved square, island radius 3.0 m, 5.0 m at the narrowest; counter-clockwise circulation, island on the corridor's left. V1 exhibited head-on deadlocks on roundabouts. Two-footprint residual (formula below): V2 envelope −1.78 m, physical ring −0.88 m, target +1.99 m.

**Decisions recorded by the owner (2026-09-23):**
- one logical corridor on the ring, centreline unchanged at radius 6.0 m — no second lane, no `LaneAdjacency`, no lane change, no MOBIL, no runtime manoeuvre semantics;
- physical change only: V1 authored traffic data and topology (lane nodes, successors, weights, connectors, portals) are untouched, so the V1 source hash and the 5.27 lineage stay byte-identical and no identity is minted or retired. This is a physical-world exception to the AD-36 freeze. **It does not imply V1 behavioural equivalence**: an unchanged source hash proves unchanged data, not unchanged driving on new colliders;
- the reviewed width is **applied**: the decision width is written onto the owned samples, and the report records imported versus applied width per subject. This replaces the frozen 5.28 rule "Largeur divergente → hard failure".

**Target geometry (binding):** V2 ring corridor at radius 6.0 m with `HalfWidthLeft` = 4.0 m and `HalfWidthRight` = 4.0 m (8.0 m modelled envelope); physical island radius ≤ 1.75 m; physical clear/paved outer radius ≥ 10.25 m.

**Two-footprint clearance criterion (additional functional proof, never a substitute for the target geometry).** All inputs come from the model's versioned validation profile — `MaxVehicleHalfWidthMeters` (W/2), `MaxVehicleLengthMeters` (L), `LateralClearanceMarginMeters` (m) — never from new constants. Two maximum footprints side by side, heading tangent to the ring at the tightest position: the inner footprint's centre radius is `R_in = r_inner + m + W/2`; its outer front corner is at `c_in = √((R_in + W/2)² + (L/2)²)`; the outer footprint's centre radius is `R_out = c_in + 2m + W/2`; its outer front corner is at `c_out = √((R_out + W/2)² + (L/2)²)`. The residual `(r_outer − m) − c_out` must be **strictly positive** and is published in the report per roundabout instance. It is evaluated twice: on the applied V2 envelope (r_inner / r_outer = ring corridor inner and outer edges) and on the physical ring (r_inner = island collider radius, r_outer = nearest non-roadway collider or paved edge, measured). A positive residual never allows the target geometry to shrink; the owner judges the published value at re-review.

**Design points settled in-story:**
- the outer ring meets the first ~2.25 m of the sidewalks of the 8 adjacent `Ring_*` segments. **Current implementation candidate** (not a frozen solution): `MVP_Run` instance overrides on those 8 instances. The story inspects the actual geometry and chooses the minimal clean transition that leaves no sidewalk or curb collider overlapping the ring. Modifying the shared `Greybox_RoadSegment_TwoWay` prefab (also used by the 4 avenues) remains **Ask First**;
- entry/exit movements joining a 2.0 m approach to a 4.0 m ring — default: movement samples interpolate between the applied widths of their endpoint corridors (the importer's existing interpolation rule), and the junction-level width decision states how it applies;
- the diagonal arm next to the tunnel portal is checked for collider overlap.

**Non-goals:** no second lane or adjacency; no roundabout control kind (5.35); no change to crossroads or T junctions; no Gate A sign-off (it stays in 5.28); no automatic acceptance of a V1 behavioural delta.

**Must NOT be copied:** widening by moving lane nodes; filling the width with an invented second corridor; the importer's seed treated as width authority; an unchanged source hash offered as proof of V1 behavioural equivalence.

**EditMode verification:** source hash and lineage unchanged; each of the 4 instances meets the binding target geometry (compiled ring radius 6.0 m with 4.0 / 4.0 m, island ≤ 1.75 m, paved outer radius ≥ 10.25 m, measured from colliders); in addition, the two-footprint residual is strictly positive on both the applied V2 envelope and the physical ring, and is published; no sidewalk or curb collider intersects the ring's drivable envelope; every owned or interpolated sample satisfies the lateral clearance gate on each side; a width decision below the clearance gate is refused; an asymmetric decision is applied exactly as authored; the report lists imported versus applied width per subject; the conflict candidate set is unchanged (curves unchanged).

**V1 regression verification (required, not optional):** the complete V1 oracle/regression suite runs after the prefab/collider change — the full EditMode suite (including `TrafficOracle` and the Story 5.2/5.4/5.7/5.9/5.10/5.14/5.15/5.16 tests) and the V1 traffic PlayMode suites (`Story510RoutedTrafficPlayModeTests`, `Story57…`, `Story59…`, `Story515…`) in a fresh Editor session, compared against a baseline captured on the same suites **before** the change. Any behavioural delta — a changed result, a newly failing or newly passing test, a changed oracle trace — is **reported for owner review**, never accepted automatically because the source hash is unchanged. The PlayMode runner is single-use per Editor session: the before and after runs need two Editor restarts.

**PlayMode verification:** the V1 regression run above; visual check of the 4 roundabouts in the Editor under the double state guard.

**Completion evidence:** before/after V1 regression results with every delta dispositioned by the owner; the per-instance target-geometry measurements and clearance residual table; regenerated 5.28 artifacts (decisions, model, overlay, report) committed, with the `RoadModelVersion` change recorded; roundabout overlays ready for re-review.

**Unlocks:** 5.28 resumes at its HALT — re-review of the roundabouts and the central crossroads, then owner signature (Gate A).

**Acceptance Criteria:**

**Given** the widened roundabout prefab and the applied width decisions
**When** each of the four instances is measured
**Then** the compiled V2 ring corridor is at radius 6.0 m with `HalfWidthLeft` = 4.0 m and `HalfWidthRight` = 4.0 m
**And** the physical island radius is ≤ 1.75 m and the physical clear/paved outer radius is ≥ 10.25 m

**Given** the widened roundabout prefab
**When** the V1 source set is re-extracted
**Then** its source hash and the 5.27 lineage are byte-identical and no identity is minted or retired
**And** no sidewalk or curb collider overlaps the ring's drivable envelope

**Given** two maximum supported footprints from the versioned validation profile
**When** they are placed side by side at the tightest ring position with the lateral clearance margin on every side
**Then**, in addition to the target geometry, the residual clearance is strictly positive on both the applied V2 envelope and the physical ring of every instance, and is published in the report

**Given** a reviewed width decision that differs from the importer's seed
**When** the 5.28 pipeline runs
**Then** the reviewed width is applied to the owned samples and the report records imported and applied values
**And** any owned or interpolated sample below the lateral clearance gate on either side is a hard failure

**Given** the prefab/collider change
**When** the complete V1 oracle/regression suite runs against its pre-change baseline
**Then** every behavioural delta is reported for owner review and none is accepted on the strength of the unchanged source hash

**Given** the regenerated artifacts
**When** Gate A is evaluated
**Then** it remains closed until 5.49 is complete, the 5.28 artifacts are regenerated, and the owner re-reviews the roundabouts and signs in 5.28
```

### 4.2 `epics.md` — Story 5.28 and program plan (Proposal 2, approved)

**Story 5.28**

| Location | OLD | NEW |
| --- | --- | --- |
| Prerequisites | `5.27 (the report's enumerated gap list is the worklist).` | `5.27 (the report's enumerated gap list is the worklist). **Sign-off only:** 5.49 — the 2026-09-23 overlay review blocked the four roundabouts; the authoring pipeline is delivered, the HALT resumes after 5.49.` |
| Non-goals | `…; no road geometry change; no traffic runtime.` | `…; no road geometry change **in this story** — the roundabout widening required by the 2026-09-23 overlay review is Story 5.49; no traffic runtime.` |
| Reusable V1 components | `…; the district itself, unchanged.` | `…; the district itself, unchanged by this story (5.49 widens the roundabouts physically, V1 traffic data untouched).` |
| EditMode verification | `…the importer may propose that width but only review makes it authoritative…` | `…the importer may propose that width but only review makes it authoritative, **and the reviewed width is applied to the owned samples, the report recording imported versus applied (2026-09-23 correction, implemented in 5.49)**…` |
| Completion evidence | `**Gate A** — migration report at zero hard errors…` | `**Gate A** (after 5.49) — migration report at zero hard errors…` |

**Program plan**

- Gate table, row A, column "After": `5.28` → `5.28 (sign-off after 5.49)`.
- Dependency graph: `5.25 ─► 5.26 ─► 5.27 ─► 5.28 ═GATE A═ ─► 5.29 …` → `5.25 ─► 5.26 ─► 5.27 ─► 5.28 ─► 5.49 ─► 5.28✓ ═GATE A═ ─► 5.29 …` (5.24 connector realigned), with the footnote: *`5.28✓` — the 5.28 authoring pipeline is delivered before 5.49; its overlay re-review and owner signature resume after 5.49 and close Gate A. 5.49 is numbered after 5.48 because 5.17–5.23 are burned and 5.24–5.48 were taken when the need arose (2026-09-23 course correction); its position in the chain, not its number, sets its order.*
- Critical path: `… 5.27 → 5.28 → 5.29 …` → `… 5.27 → 5.28 → 5.49 → 5.28 sign-off → 5.29 …`.
- Story index, new row under 5.28: `| 5.49 | Roundabout ring widening and applied reviewed widths *(inserted 2026-09-23, precedes the Gate A sign-off)* | FOUNDATION | Physical geometry / width authoring | M |`.
- Test-routing table, new row under 5.28: `| 5.49 | EditMode + V1 PlayMode regression before/after (two fresh Editor sessions) + Editor check under the double state guard | edge-case-hunter, verification-gap — geometry measurement claims and V1 behavioural-delta disposition |`.
- AD consistency table: AD-36 stories `5.24, 5.31, 5.48` → `5.24, 5.31, 5.49, 5.48`, appending `; 5.49 changes the shared roundabout prefab physically only — V1 traffic data unchanged; behavioural deltas are checked against the complete V1 oracle/regression coverage and every detected delta is dispositioned by the owner; equivalence is never inferred from an unchanged source hash`; AD-45 stories → `5.26, 5.27, 5.49`, appending `; reviewed asymmetric widths are applied, not only compared`; AD-47 stories → `5.27, 5.49, 5.28`.
- Risk R1, appended sentence: *The 2026-09-23 overlay review added 5.49 to this runway — the Gate A checkpoint did exactly its job by blocking a roundabout geometry that could not hold two vehicles.*

### 4.3 `spec-5-28-junction-semantic-authoring-and-overlay-sign-off.md` (Proposal 3, approved)

Status stays `in-progress`. Renegotiated by the owner on 2026-09-23; implementation belongs to 5.49. Edits are in French, matching the file.

**Frozen I/O matrix** — replace the row:
```
| Largeur divergente | largeur importee != revue | echec dur | rien n'est ecrit |
```
with:
```
| Largeur revue != amorce importee | largeur des decisions != largeur importee | largeur revue APPLIQUEE aux echantillons possedes ; rapport : importee / appliquee par sujet (5.49) | N/A (trace dans le rapport) |
| Largeur sous le gabarit | une demi-largeur, gauche ou droite, de tout echantillon possede -- y compris interpole sur un mouvement -- < `MaxVehicleHalfWidthMeters + LateralClearanceMarginMeters` | echec dur nommant le sujet et l'echantillon | rien n'est ecrit |
```

**Design Notes, `Widths`** — replace `(sections et carrefours ; gauche et droite explicites, AD-45 asymetrique, egales aujourd'hui dans MVP_Run : tous les echantillons possedes doivent valoir ces demi-largeurs a la quantification pres)` with `(sections et carrefours ; gauche et droite explicites, AD-45 asymetrique ; la largeur revue est APPLIQUEE aux echantillons possedes et le rapport publie importee / appliquee par sujet ; pour un carrefour, la regle d'application aux mouvements qui joignent deux largeurs differentes est fixee par la 5.49)`.

**Tasks** — in the tests line, replace `largeur asymetrique divergente refusee` with `largeur revue appliquee aux echantillons possedes et largeur sous le gabarit refusee (5.49)`; add under the HALT: `- Correct-course du 2026-09-23 (sprint-change-proposal-2026-09-23.md) : Story 5.49 inseree avant la signature. Reprise du HALT apres 5.49 : artefacts regeneres, nouvelle revue des 4 giratoires ET du carrefour central, puis signature.`

**Spec Change Log** — new entry:
```markdown
- **2026-09-23 -- renegociation proprietaire, ligne gelee « Largeur divergente ».**
  Declencheur : revue d'overlay, 4 giratoires bloques (anneau V2 4,0 m ; residu a deux gabarits -1,78 m, anneau physique -0,88 m).
  Amende : la largeur revue est appliquee au lieu d'etre seulement comparee ; une demi-largeur de tout echantillon possede ou interpole sous le gabarit reste un echec dur ; implementation, elargissement physique et regeneration des artefacts confies a la Story 5.49 (sprint-change-proposal-2026-09-23.md).
  Etat evite : signer une Gate A sur un anneau qui ne peut pas contenir deux vehicules, ou faire de l'amorce de l'importeur l'autorite de largeur.
  KEEP : decisions seules authoritative, un corridor logique d'anneau, aucune adjacence, Gate A non signee avant 5.49 + regeneration + nouvelle revue.
```

### 4.4 Architecture and V1 oracle (Proposal 4, approved)

**`ARCHITECTURE-SPINE.md`, "Course-correction supersessions"** — new closing paragraph:

> *2026-09-23 course correction (`planning-artifacts/sprint-change-proposal-2026-09-23.md`), accepted by the owner:* the `MVP_Run` overlay review blocked the four roundabouts (V2 ring 4.0 m, physical ring 5.0 m at the narrowest; neither holds two maximum footprints side by side). Story 5.49 widens the shared roundabout prefab. This is a **physical-world exception to the AD-36 freeze, not a V1 capability change**: V1 authored traffic data and topology — lane nodes, successors, weights, connectors, portals — stay unchanged, so the V1 source hash and the 5.27 lineage stay byte-identical. An unchanged source hash proves unchanged data, **not unchanged behaviour**: the complete V1 oracle/regression suite is re-run against a pre-change baseline to detect behavioural deltas; every detected delta is dispositioned by the owner and never accepted automatically. Under AD-45/AD-47 a reviewed width is now **applied** to the owned samples rather than only compared with the importer's seed; the report publishes imported versus applied. No AD is reopened.

**`traffic-v2/V1-BEHAVIORAL-ORACLE.md`, "Frozen V1 surface"** — under the bullet `the authored Traffic V1 data in MVP_Run and the Greybox_AIVehicle prefab;` add:

> *2026-09-23 exception (Story 5.49): the roundabout prefab's physical geometry — roadway, island, sidewalks, colliders — is widened while its V1 traffic data stays unchanged. Oracle verdicts are re-run against a pre-change baseline; a behavioural delta is reported to the owner, not absorbed.*

### 4.5 Tracking (Proposal 5, approved)

**`sprint-status.yaml`** — header comment `Gates: A after 5.28, …` → `Gates: A after 5.28 (sign-off after 5.49), …`; new key immediately after 5.28, before 5.29:

```yaml
  5-28-junction-semantic-authoring-and-overlay-sign-off: in-progress
  # Inserted 2026-09-23 (sprint-change-proposal-2026-09-23.md): the overlay
  # review blocked the 4 roundabouts. 5.49 runs BEFORE the 5.28 sign-off; its
  # number follows 5.48 only because 5.24-5.48 were already taken.
  5-49-roundabout-ring-widening-and-applied-reviewed-widths: backlog
  5-29-deterministic-strategic-route-plan: backlog
```

**`deferred-work.md`, « Bloquant Gate A »** — the blocker stays **open**; `summary:` becomes:

```
  summary: PLANIFIEE (2026-09-23) -- prise en charge par la Story 5.49 (`epics.md`, `sprint-change-proposal-2026-09-23.md`). RESTE OUVERTE jusqu'a ce que : la 5.49 soit implementee, les artefacts 5.28 regeneres, les 4 giratoires revus a nouveau et la Gate A signee. Decisions du proprietaire : un seul corridor logique d'anneau ; axe au rayon 6,0 m inchange ; enveloppe V2 cible 4,0 / 4,0 m ; preuve de degagement a deux gabarits ; largeur revue appliquee ; exception physique seule au gel AD-36, donnees et topologie de trafic V1 authorees inchangees -- les deltas comportementaux V1 sont recherches par comparaison de la suite oracle/regression avant/apres ; tout delta detecte est soumis au proprietaire et n'est jamais accepte automatiquement ; transition vers les segments `Ring_*` decidee en 5.49 ; modification du prefab partage `Greybox_RoadSegment_TwoWay` en Ask First. ENTREE HISTORIQUE (conservee telle quelle) : <original summary unchanged>
```

The original `evidence:` field is kept unchanged. `graphify update .` is not needed: the Graphify scope is `Assets/RoadRage/` only and none of these edits touch it.

---

## 5. Implementation handoff

**Scope:** Moderate.

| Role | Responsibility |
| --- | --- |
| Developer agent (this correct-course run) | apply sections 4.2–4.5 and insert 4.1 into `epics.md` after final approval |
| Developer agent (`bmad-build`, Story 5.49) | implement 5.49: physical widening, `Ring_*` transition, applied-width rule, clearance proof, V1 before/after regression, regeneration of 5.28 artifacts |
| Owner (Kenan) | disposition every V1 behavioural delta; approve any change to `Greybox_RoadSegment_TwoWay` (Ask First); re-review the 4 roundabouts and the central crossroads; sign Gate A in 5.28 |

**Sequence:** 5.49 (backlog → build) → 5.28 HALT resumes (regeneration, re-review, signature) → Gate A → 5.29.

**Success criteria:**
- binding target geometry met on all four instances, with a strictly positive two-footprint residual published per instance;
- V1 source hash and 5.27 lineage byte-identical; every V1 behavioural delta dispositioned by the owner;
- regenerated 5.28 artifacts committed and equal to a fresh pipeline;
- the « Bloquant Gate A » entry closes only when Gate A is signed.

---

## Checklist record

| Section | Status | Notes |
| --- | --- | --- |
| 1. Trigger and context | `[x]` | 5.28 overlay review; technical limitation; Editor measurements and two-footprint residuals |
| 2. Epic impact | `[x]` | Epic 5 completable; one inserted story before Gate A; no epic added or invalidated |
| 3. Artifact impact | `[x]` | Epics, 5.28 spec, spine note, oracle note, tracking; PRD `[N/A]` (FR/NFR in `epics.md`); UX `[N/A]` |
| 4. Path forward | `[x]` | Option 1 direct adjustment; rollback and scope reduction not viable |
| 5. Proposal components | `[x]` | Proposals 1–5 approved individually, with owner corrections applied |
| 6. Final review and handoff | `[x]` | approved by Kenan 2026-09-23; sections 4.1–4.5 applied, including the `sprint-status.yaml` update; handoff to `bmad-build` for Story 5.49 |
