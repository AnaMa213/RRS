# Sprint Change Proposal - 2026-09-29 - Pose nominale cinematique

**Statut :** APPROUVE par Kenan le 2026-09-29 et APPLIQUE.
- 4.1 et 4.2 ont ete approuvees apres une revision chacune ; 4.3 avec l'option (i).
- 4.4 a 4.8 ont ete approuvees en batch, sous condition d'une passe de consolidation de 4.1 et 4.3 :
  - reste de grille ρ·h_e/2, conforme aux rectangles discrets et a la regle δ/2 des preuves ;
  - amorcage au s effectif des portails ;
  - relaxation a/cos(e*) ;
  - contacts de caisse et appui des pneus distingues ;
  - `fixedDeltaTime` reel consigne ;
  - runs exploratoires et d'acceptation distingues.

  Cette passe est faite dans ce texte.
- Editions appliquees dans le contrat Road World Model (§4, §8), `epics.md` (5.31, 5.52, table des gates), la spine d'architecture et la spec 5.31.
- Aucune preuve Gate A ni aucune signature n'est modifiee.

**Sujet :** Le contrat de reference de la Story 5.31 et toutes les preuves Gate A orientent le gabarit sur la tangente de la reference. Or la cinematique du profil declare impose a la caisse un ecart de cap e. La proposition introduit la pose nominale cinematique, la couverture dirigee des mouvements et une regle de contact sur relief roulable. Elle place la regeneration des preuves en 5.52.

**Mode :** incremental pour 4.1 a 4.3, batch pour la suite.

**Portee :** moderee. Ajustement direct dans l'Epic 5, sans nouvel epic ni nouvelle story, sans changement de perimetre produit.

**Decisions proprietaire :** 1A, 2a, 3 sous conditions (2026-09-29). Aucune preuve n'est modifiee et rien n'est re-signe par cette proposition.

## 1. Probleme et preuves

Tout ce qui suit a ete constate en local : traces publiees, code et modele signe. Aucune commande Unity n'a ete lancee pour cette analyse.

### Ce que montre la campagne exploratoire 5.31

Le run `exploratory-20260929-141746` compte 12 vehicules et 30 029 pas.

- d max = 0,996 m, et 46 % des pas depassent 0,5 m.
- Le suivi de position est bon :
  - ecart lateral moyen 0,105 m, max 0,31 m ;
  - d ≈ 0,005 m en ligne droite.
- L'ecart vient du **cap** : 15° en moyenne et 23° au maximum sur ces pas.
  - Exemple, mouvement `469fe814` (continuation d'anneau, R = 6 m) : le cap reste a ~19° pendant 6 m a 3,6 m/s, alors que l'ecart lateral ne varie que de 0,25 m. Une caisse reellement desalignee de 19° aurait derive d'environ 2 m.

### Pourquoi : le contrat decrit une caisse impossible en courbe

- La pose nominale N(s*) de la 5.31 aligne le gabarit sur la tangente au point de reference.
- Les quatre preuves Gate A font de meme : `ConflictSweep.cs:590,599,626`, `JunctionClearance.cs:168`, `RoundaboutClearance.cs:11` (« cap tangent »).
- Le profil declare decrit pourtant une autre cinematique : point de reference a a = 1,55 m devant un essieu arriere non directeur, empattement L = 3,10 m. C'est `RoadModelCompiler.RadiusMeters`, avec R_P = √((L/tan δ)² + a²).
- Avec un point de reference sur la courbe et sans derive a l'essieu arriere, la caisse fait un angle e avec la tangente.

### Verification analytique (demandee par le proprietaire)

| Element | Definition reelle | Coherence |
|---|---|---|
| Courbe | tangente et courbure compilees, echantillons exacts d'un temoin a courbure continue (contrat, « channel consistency ») | l'equation se resout sur le canal de courbure |
| Point de reference | origine vehicule, essieux a z = ±1,55 (`VehicleProfileDef_Default`, profil du prefab V2) | a = 1,55 m |
| Empattement | 3,10 m dans le profil de conduisibilite et dans le profil vehicule | oui |
| Direction | roues avant a angle parallele, sans Ackermann (`VehicleSteeringModel.ResolveWheelSteerAngleDegrees`), roues arriere fixes | modele bicyclette approche ; l'ecart va au residu mesure |

**Loi de la pose.** de/ds = κ(s) − sin(e)/a (tractrice), avec tan δ = (L/a)·tan e.
- Sur un arc constant, e tend vers asin(a·κ). C'est exactement la relation de `RadiusMeters` : 40° a R_adm = 4,03 m.
- Le regime etabli asin(a·κ) n'est qu'une limite. Il saute a chaque discontinuite de κ.

**Confrontation aux 23 311 pas publies** (v ≥ 0,3 m/s, analyse hors Unity, a transformer en fixture) :

| Modele de cap nominal | Ecart RMS | Ecart max |
|---|---|---|
| tangente (contrat actuel) | 10,7° | 22,9° |
| regime etabli asin(a·κ) | 9,4° | 32,2° |
| **tractrice** | **2,7°** | 7,8° |

Face a la tractrice, il reste un exces de +1,8° a +3,4°, qui ne croit pas nettement avec l'acceleration laterale. Deux causes candidates, a separer par la mesure :
- la commande vise le regime etabli (`MotionCommand.Track`, `expectedHeading`) ;
- la direction physique est parallele, sans Ackermann.

### Constats secondaires

**Couverture des mouvements.**
- 20 mouvements sur 72 ne sont jamais selectionnes : les 12 triplets utilisent tous la graine 0.
- Le terme −ln(U)/w, avec w de 20 a 60, pese quelques centimetres face a plusieurs metres d'ecart de distance. La graine n'y change donc rien.
- Aucun poids n'est nul dans le modele signe : le minimum est 1.

**Contacts.**
- `Rampe_Ouest` / `Rampe_Est`, run 1, pas 1365 : c'est la caisse qui touche le dos d'ane `Relief_DosDane_AvenueCenterToEast`.
- La 5.51 classe ce dos d'ane en relief roulable.
- Au moment du contact : d = 0,05 m, 5,4 m/s, aucun repli, sortie atteinte.

## 2. Checklist

| Point | Statut | Note |
|---|---|---|
| 1.1 a 1.3 Declencheur, probleme, preuves | [x] | Story 5.31 ; approche en echec (defaut de contrat) ; traces et code ci-dessus |
| 2.1 Epic 5 realisable | [x] | Oui, avec ajustements en 5.31 et 5.52 |
| 2.2 Changements d'epic | [!] | AC 5.31, capacite et AC 5.52, table des gates (ligne B) |
| 2.3 a 2.5 Autres epics, ordre | [N/A] | Rien apres Gate B n'est touche ; ordre inchange (5.31 → 5.52 → Gate B) |
| 3.1 PRD | [N/A] | Pas de PRD separe (precedents du 2026-09-25, du 2026-09-28 et du 2026-09-29) |
| 3.2 Architecture | [!] | Contrat Road World Model §4 et §8 ; note datee dans la spine |
| 3.3 UX | [N/A] | Aucune interface touchee |
| 3.4 Autres artefacts | [!] | Spec 5.31 (bloc fige renegocie) ; campagne exploratoire devenue historique |
| 4.1 Ajustement direct | Viable | Effort moyen ; risque eleve cote geometrie, porte par la 5.52 |
| 4.2 Retour arriere | Non viable | Les algorithmes 5.49 a 5.51 restent valides, seule la pose d'entree change |
| 4.3 Revue MVP | N/A | Perimetre produit inchange |
| 4.4 Chemin retenu | [x] | Ajustement direct |

**Risque annonce, non chiffre.** La caisse tournee de e balaie une enveloppe plus large en courbe serree.
- Premiers candidats a un residu negatif en 5.52 : les giratoires (anneau de 6 m, e ≈ 15°) et les virages a droite proches de R_adm (e ≈ 22,6°).
- Un tel echec est remonte au proprietaire. Aucune marge, aucun seuil et aucun reste ne sont modifies pour l'absorber.

## 3. Approche recommandee

Ajustement direct dans l'Epic 5, en trois etapes.
1. **5.31, reprise.**
   - Mesure et commande sur la pose nominale propre a chaque route.
   - Couverture dirigee des mouvements.
   - Regle de contact.
   - Nouvelle campagne exploratoire, puis declaration d'ε_t par le proprietaire, puis campagne d'acceptation.
2. **5.52.**
   - Regeneration des quatre preuves sur l'ensemble de poses nominales cinematiques, avec a_e.
   - Reevaluation des paires dont l'enveloppe change.
   - Re-signature explicite par le proprietaire.
3. **Gate B.** Fermee apres la 5.52, dans la portee de preuve de la table.

## 4. Propositions detaillees

### 4.1 Contrat Road World Model, §8 - pose nominale cinematique (APPROUVEE, revision 1)

Dans la puce « Lateral quantities are distinct », remplacer « the upright reference pose » par « the kinematic nominal pose ». Ajouter juste apres :

> - **Kinematic nominal pose** *(2026-09-29, sprint-change-proposal-2026-09-29-kinematic-pose.md)*: the nominal pose of the maximum-gauge box at progress s is the pose of the declared drivability geometry — reference point `a` ahead of a non-steered rear axle, wheelbase `L` — whose reference point lies on the compiled reference with no lateral slip at the rear axle.
>   - **Pose.** Position is the reference position at s and up is road-up. With e = ψ_tangent − ψ_body (the same sign convention as curvature, positive toward road-right), the nominal signed heading error in the localization convention is −e(s). The offset evolves as **de/ds = κ(s) − sin(e)/a**, where κ is the validated curvature channel.
>   - **Steady state.** On a constant-curvature arc with a·|κ| < 1, which the admission radius guarantees, e converges monotonically to e* = asin(a·κ) from any offset in (e* − π, π − e*). Its local relaxation length is a / cos(e*) = a / √(1 − a²κ²). That steady value is not the nominal pose, because it jumps wherever κ jumps. The body rotation rate is dψ_body/ds = sin(e)/a. It is continuous, and it — not the tangent rate — drives every between-sample term.
>   - **Continuity and reset.** e is set to 0 only at the planned insertion, at the entry portal's effective progress s_p on its element, which may lie inside the element. There the vehicle is placed tangent-aligned to the tangent at s_p. Nowhere else is e reset. The body heading is continuous across every seam — `LaneConnection` continuation, merge or split, and corridor-to-movement or movement-to-corridor — so e jumps there by exactly the signed tangent jump of the seam: e⁺ = e⁻ + Δψ_tangent.
>   - **Three quantities stay distinct.**
>     1. The nominal pose model is part of this contract. It is versioned and recorded by every Gate A evidence.
>     2. The physical residual — tyre slip, parallel (non-Ackermann) steering, control lag — is never modelled, only measured, and lies inside ε_t.
>     3. The allocation a_e = max |o(s)| + ε_t inflates every proof around the nominal pose set.
>   - **Route-independent offset bounds.** Each element X carries an entry interval I_X = [e_lo, e_hi] at its start (s = 0). The intervals are computed over the directed transition graph the Route Planner may emit, legal cycles included:
>     1. **Portal seeds.** An entry portal at effective progress s_p on X seeds the solution started from 0 at s_p. That solution is valid on X for s ≥ s_p only, and s_p may lie inside X. An element with neither an entry portal nor an admissible predecessor is reported unreachable and carries no interval.
>     2. **Exit set and transfer.** The exit set O_P of a predecessor P is the hull, at P's end, of:
>        - the images of I_P through P's offset equation;
>        - every portal-seeded solution on P.
>
>        The map Φ_P carries O_P into X through the seam jump. Every image is monotone in its initial value, because one-dimensional solutions of the same equation never cross, and adding a constant jump preserves order. An interval therefore maps to the interval of its endpoint images.
>     3. **Closure (inductiveness).** The family {I_X} is accepted only if, for every element X, ⋃_P Φ_P(O_P) ⊆ I_X. Candidate intervals may come from any iteration or widening. The check alone makes them sound: by induction on route length, every admissible route's offset at every element start lies in I_X, including routes through cycles and routes that began at a portal inside an earlier element.
>     4. **Explicit failure.** If no candidate passes the check within the declared iteration budget, or if any interval reaches |e| ≥ 90°, evidence generation fails explicitly with `HeadingOffsetBoundNotClosed`.
>     5. **Reachable set and measurement.** At progress s on X, the reachable offsets lie in [e_lo(s), e_hi(s)]: the hull of the solutions started from e_lo and e_hi at X's start and of every portal-seeded solution with s_p ≤ s. ε_t is measured against the route's own solution, which the closure places inside that set.
>   - **Physical envelope.** Every proof covers the union, over every s of every compiled segment and every offset e ∈ [e_lo(s), e_hi(s)], of the inflated footprint. The two extreme footprints alone do not suffice.
>     - **Construction kept.** The proofs keep their construction: discrete oriented rectangles of the maximum gauge, the inflation applied as a distance slack (invariant under rotation), and the rule that every point of the footprint between two evaluated poses stays within δ/2 of one of them. ρ is the distance from the reference point to a corner of the non-inflated rectangle.
>     - **Between progress samples,** along one offset solution: δ = |Δp| + ρ·Δθ_max, with Δθ_max = Δs · max|sin(e)/a| over the segment. This bounds the body rotation; it is never the endpoint tangent change. The existing fail-closed rule for a rotation of 90° or more applies to Δθ_max.
>     - **Across offsets at one progress sample,** a grid of step h_e covers the hull of [e_lo(s), e_hi(s)] over both adjacent half-segments, both ends included. Every intermediate heading is then within h_e/2 of a grid heading, so every intermediate footprint point lies within ρ·h_e/2 of the corresponding grid footprint.
>
>     Both remainders, δ/2 and ρ·h_e/2, are declared, published per proof, and added to the inflation. They are never taken from the reserved margin.
>   - **Feasibility.** At every s and for every offset in the interval, the implied steering angle tan δ = (L/a)·tan e must stay within the declared lock at the planned speed, and its rate v·|dδ/ds| within the declared steer rate. A pose that violates either is diagnosed `NominalPoseInfeasible`. The trajectory is then infeasible and never counted as covered by construction.
>   - **Previously signed evidence.** Evidence computed with the tangent-aligned pose — every evidence signed before this change — does not certify physical coverage under this contract. It is kept as superseded history and can never be used to close Gate B. The coverage verdict requires evidence whose recorded pose model is the kinematic nominal pose.

### 4.2 Contrat Road World Model, §4 - objectif de mouvement intermediaire (APPROUVEE, revision 1)

Ajouter apres le paragraphe « Weighted turn preferences… » :

> *2026-09-29 (sprint-change-proposal-2026-09-29-kinematic-pose.md):* **Intermediate movement objective.** A route request may name one `JunctionMovement` that the route must traverse before its exit objective.
>
> - **Semantics.**
>   - A route with an objective is a path in the phased state space (planner state, phase), where phase ∈ {before, after}. The only transition from `before` to `after` is a real traversal of the named movement, over its whole length.
>   - The 5.29 rules — authored weights, seeded preferences, the zero-weight rule, and the rule that a continuation's suffix must reach its objective without revisiting its entry state — apply unchanged inside each phase.
>   - Within a phase, the objective is the named movement for `before` and the exit for `after`. A state may therefore occur once in each phase, which is a legal cycle, but never twice within one phase.
> - **Completeness.**
>   - The planner returns a minimum-cost route in the phased space whenever one exists, and `NoRoute` only when none exists.
>   - Computing the two phases separately is complete under these semantics. The phases share no state and no constraint; they are linked only by the movement's traversal, and the cost is additive. The minimum over the whole path is therefore the minimum of the first phase plus the movement cost plus the minimum of the second phase.
>   - An implementation that couples the phases in any other way must prove the same property. This proof, not the algorithm, is the contract.
> - **Cost and plan shape.**
>   - The named movement's length and seeded preference are counted exactly once.
>   - The concatenated `RoutePlan` keeps ordered identities, occurrences and `s` positions contiguous: the approach occurrence ends at the movement's start, the movement appears once over [0, Length], and the departure occurrence starts at s = 0. There is no duplicate occurrence and no gap.
>   - The plan's reason records the objective and the phase boundary.
> - **Request outcome.** For one request, the planner reports `Planned`, or `NoRoute` with the failing phase named: `NoRouteToObjective` or `NoRouteAfterObjective`. It never returns a hand-assembled or partial plan.
> - **Persistence.** The objective stays in the vehicle's destination state until localization shows the named movement actually traversed to its end. Every replan made before that keeps it. Afterwards, the objective reduces to the exit.
> - **Activation boundary.**
>   - The Route Planner exposes the objective as an ordinary optional request field. It has no knowledge of tests or of any measurement token.
>   - "Measurement only" is enforced where the request is built: only the V2 lifecycle path under the Story 5.31 measurement authorization sets the field. A structural check proves that no other production code sets it.
>   - Normal operation keeps routing with an exit objective alone. The objective never changes authored weights, costs or the signed model.
> - **Campaign diagnostic, distinct from the planner.** `NotSelectable` is a campaign verdict, not a planner outcome. A movement is `NotSelectable` only if **every** (entry, exit) pair of the model yields `NoRoute` for that objective. Feasibility is topological, so seeds do not change it. A movement that any entry, exit or route can reach is selectable, and the campaign must cover it.

### 4.3 Contrat Road World Model, §8 - contacts pendant les campagnes (APPROUVEE, option i)

Sous la puce « 2026-09-28 Gate A sidewalk clarification », apres l'addendum sur le relief roulable :

> - *2026-09-29 (sprint-change-proposal-2026-09-29-kinematic-pose.md):* **Vehicle contacts during driven campaigns.**
>   - **Recognized drivable relief.** A collider is a recognized drivable relief only if it belongs to the drivable-relief result set of the valid Story 5.51 physical clearance evidence. It is identified by hierarchy path and that evidence's input fingerprint, never by a name pattern.
>   - **Body contacts versus tyre support.**
>     - The rule classifies contacts of the vehicle's **colliders**. The V2 prefab has one body `BoxCollider`.
>     - Tyres are suspension raycasts (AD-35), never colliders. Their support on the carriageway, including over a recognized relief, is the normal wheel model: it is neither an observation nor a failure. It is published through the grounded-wheel count.
>     - A vehicle that adds wheel colliders must classify their contact with the carriageway or a recognized relief as support. Their contact with any other collider falls under the last clause below.
>   - **Body contact with a recognized relief.** Such a contact is a **non-blocking physical observation** only while none of the consequence criteria below fires.
>     - *Published per contact:* first and last contact step; maximum contact impulse and relative normal speed; speed before, minimum during and after; maximum d and heading error relative to the nominal pose; maximum roll and pitch; grounded-wheel count; and whether the route continues to its exit.
>     - *Observation window:* the contact steps ± 50 physics steps. The run records the `Time.fixedDeltaTime` actually in force and publishes the window duration as 50 × that value (1 s at the declared 0.02 s). A value that differs from the declared campaign condition invalidates the run. The window is a declared design parameter, used for publication only.
>   - **Consequence criteria.** The contact fails the campaign if, from the first contact to the end of the run, any of the following occurs:
>     - *out of bounds:* d > ε_t, or the localization flag `OutsideEnvelope` or `WrongWay`;
>     - *loss of control:* v > v*(s), `TrackingToleranceExceeded`, or any V2 fallback command;
>     - *abnormal stop:* |v| ≤ 0.05 m/s for 10 consecutive steps while the speed plan's target exceeds `MinimumDirectionSpeed` (the `FallbackHeld` constants of Story 5.31);
>     - *failed continuation:* the vehicle does not reach its planned exit.
>   - **Exploratory versus acceptance runs.**
>     - An exploratory run has no declared ε_t. There, the out-of-bounds criterion uses the localization flags only, and d is published without being judged. Every other criterion is evaluated, and whatever fires is published as a finding. An exploratory run never accepts anything.
>     - In an acceptance run, every criterion, including d > ε_t, fails the campaign.
>   - **Carriageway grounding.** A contact between the body and the carriageway surface itself is neither a relief contact nor a non-road contact. It is published with the same observables and judged under the same criteria and run distinction.
>   - **Any other collider.** A body contact with any other collider keeps its blocking criterion unchanged: it fails an acceptance campaign, and it is published as a finding in an exploratory run. This includes a relief missing from the valid evidence's result set, or one bound to a stale fingerprint.
>   - **Scope.** The rule applies to Story 5.31 campaigns and to the Story 5.52 milestone rerun. It changes neither the Story 5.51 relief criterion nor any clearance evidence.

### 4.4 `epics.md` - Story 5.31

**a. Artifacts** (ligne 2671). Ajouter a la fin de la liste :
> ; `Routing/` extension: the optional intermediate movement objective of the Route Planner contract (§4), set only by the measurement lifecycle path.

**b. AC, ε_t** (ligne 2697)

AVANT : « …relative to the upright reference pose at matched progress; it is evaluated exactly at every simulated physics step, so roll, pitch, heading and translation are all included »

APRES :
> …relative to the **kinematic nominal pose** (Road World Model contract §8) at matched progress. The nominal heading comes from the route's own offset solution, which starts at 0 at insertion at the entry portal's effective progress and is never reset elsewhere. ε_t is evaluated exactly at every simulated physics step, so roll, pitch, the residual heading error and translation are all included.

**c. AC, contacts** (fin de la ligne 2698)

AVANT : « …and any contact of the vehicle with a non-road collider during a campaign fails »

APRES :
> …and vehicle contacts follow the campaign contact rule of contract §8. Tyre support through the suspension raycasts is not a contact. A body contact with a recognized drivable relief of the valid Story 5.51 evidence, or with the carriageway surface itself, is a published non-blocking observation unless a consequence criterion fires. Any other body contact fails an acceptance campaign. An exploratory run, which has no declared ε_t, publishes every fired criterion as a finding and accepts nothing.

**d. AC, campagne** (ligne 2699). Inserer apres « an acceptance campaign verifies it. » :
> The campaign reaches each movement through the measurement-only intermediate movement objective (contract §4). The Route Planner plans and validates every route; no plan is hand-written and no authored weight changes. A movement is NotSelectable only when every (entry, exit) pair yields `NoRoute` for it. Campaigns measured against the tangent-aligned pose are kept as history and count toward nothing.

**e. AC, verdict** (ligne 2700)

AVANT : « (max |o(s)| + ε_t ≤ a_e of the valid evidence) is published; »

APRES :
> (max |o(s)| + ε_t ≤ a_e of valid evidence **whose recorded pose model is the kinematic nominal pose**) is published. Tangent-aligned evidence never yields a covered verdict.

**f. Nouvelle AC**, apres la ligne 2702 :
> **Given** a planned route
> **When** the tracking command is computed
> **Then** its expected heading offset is the route's own nominal offset solution (de/ds = κ − sin(e)/a, with seam jumps), never the steady-state value asin(a·κ)
> **And** along every campaign route, the steering angle implied by the nominal pose stays within the declared lock at the planned speed, and its rate within the declared steer rate. Otherwise the route is diagnosed `NominalPoseInfeasible` and is neither driven in acceptance nor counted as covered.

*Motif :* reporter 4.1 a 4.3 dans la 5.31, pour la seule solution propre a chaque route. Les intervalles par element sont du travail de preuve, qui releve de la 5.52.

### 4.5 `epics.md` - Story 5.52

**a. Titre.**
> Story 5.52: Tracking-Error Coverage and Kinematic Nominal Pose in Gate A Evidence, and Owner Re-Signature

**b. Complexite :** `M` → `L`.

**c. Note d'insertion.** Ajouter :
> *Extended 2026-09-29 (sprint-change-proposal-2026-09-29-kinematic-pose.md): every Gate A proof oriented the gauge along the reference tangent, a body pose the declared drivability geometry cannot hold in a curve; the regeneration also moves every proof to the kinematic nominal pose.*

**d. Capability delivered.** Ajouter a la fin :
> Every Gate A proof moves from the tangent-aligned sweep pose to the kinematic nominal pose set of contract §8. The proofs concerned are: conflict candidates (`ConflictSweep`), junction physical clearance and `Sidewalk` planar clearance (`JunctionClearance`), and roundabout two-gauge residuals (`RoundaboutClearance`).
> - The per-element entry offset intervals are computed over the transition graph — every entry portal, seam tangent jumps, legal cycles — and accepted only by the inductiveness check.
> - Each proof covers the union over every offset in the interval, with the published offset-grid and between-sample remainders added to the inflation.
> - Steering lock and rate feasibility is checked on the whole pose set.
> - The pose-model version enters the pair fingerprint schema beside margin, δ_c and a_e. The previous evidence and sign-off are kept as superseded history.

**e. Rules.** Ajouter :
> - A closure that cannot be demonstrated fails with `HeadingOffsetBoundNotClosed`; a pose outside lock or rate fails with `NominalPoseInfeasible`. Neither is ever counted as covered.
> - The offset-grid and between-sample remainders are declared, published per proof and added to the inflation. They are never taken from the reserved margin.
> - If a proof fails on the kinematic pose with the available clearances, the failure is escalated to the owner through a separate course correction. No margin, threshold, remainder or geometry is changed to obtain a favorable verdict.
> - Tangent-aligned evidence never closes Gate B.

**f. Non-goals.** Remplacer par :
> no geometry change, no driving-behavior change, no change of the declared ε_t (a new value reruns this story), no change of the reserved margin, δ_c or any threshold to absorb the kinematic pose.

**g. EditMode verification.** Ajouter :
> On synthetic straights, arcs, S-curves, seam tangent jumps and cycles:
> - the closed offset intervals contain the offset of every route of a bounded exhaustive route enumeration;
> - a non-closable case fails explicitly;
> - the union coverage is at least the dense-sampling maximum over offsets and progress;
> - the roundabout residual with a rotated gauge matches its closed form;
> - the fingerprint changes when the pose-model version changes;
> - the coverage verdict rejects tangent-aligned evidence.

**h. AC 1** (« every proof's inflation includes a_e… »)
> **Then** every proof is computed on the kinematic nominal pose set and its inflation includes a_e = max |o(s)| + ε_t, the reserved margin, δ_c and the published pose remainders; each residual is published per movement and corner; and the pair fingerprints record the inflation parameters and the pose-model version
> **And** any non-positive residual, unclosed offset bound or infeasible nominal pose fails and is escalated, without allocating margin or residual clearance

### 4.6 `epics.md` - table des gates, ligne B

AVANT : « …on trajectories covered by Gate A evidence that includes the declared tracking tolerance. »

APRES :
> …on trajectories covered by Gate A evidence computed on the kinematic nominal pose set of the Road World Model contract (§8) and including the declared tracking tolerance. Evidence computed with the tangent-aligned pose never closes Gate B.

### 4.7 Spine d'architecture - note datee

A la suite des notes datees des correct-courses, apres celle du 2026-09-25 :

> *2026-09-29 course correction (`planning-artifacts/sprint-change-proposal-2026-09-29-kinematic-pose.md`), accepted by the owner:* the nominal pose of the vehicle gauge is the kinematic pose of the declared drivability geometry, not the tangent-aligned pose. The body heading lags the reference tangent by e, with de/ds = κ − sin(e)/a. Every Gate A proof and every tracking measurement uses it. The Route Planner gains a measurement-only intermediate movement objective. Drivable-relief contacts during driven campaigns are published observations under consequence criteria. The Road World Model contract (§4, §8) carries the rules; Story 5.52 regenerates the evidence; the owner re-signs.

### 4.8 Spec 5.31 - repercussions (renegociation du bloc fige)

**Bloc fige : Boundaries.**
- *Mesure (A2), grandeur mesuree.*
  - N(s*) devient la **pose nominale cinematique** propre a la route (contrat §8) : e = 0 a l'insertion, au s effectif du portail (eventuellement a l'interieur d'un element), sauts de tangente aux raccords, jamais remis a zero ailleurs.
  - d reste le max sur les 8 coins, mesure contre cette pose.
- *Entre deux pas.*
  - Dans L_k, le terme ℓ_k(1 + ρ·ψ'_max) devient ℓ_k(1 + ρ·max|sin e|/a) : c'est le taux de rotation de la caisse nominale.
  - La decoupe aux raccords est conservee pour les positions ; la caisse nominale n'y saute pas.
- *Commande de suivi.* L'anticipation de κ et la correction de cap sont rapportees au cap nominal de la solution de route, et non au regime etabli.
- *Couverture.*
  - Le verdict exige une preuve dont le modele de pose enregistre est la pose cinematique.
  - Avec la preuve actuelle, dont la pose est tangente : `NotCoveredByGateA`, raison `PoseModelMismatch`.
- *Faisabilite.* La pose nominale de chaque route de campagne est verifiee (braquage et taux a la vitesse planifiee). En cas d'echec : `NominalPoseInfeasible`.
- *Campagne deterministe.*
  - Objectif de mouvement intermediaire (contrat §4).
  - `NotSelectable` seulement si toutes les paires (entree, sortie) rendent `NoRoute`.
  - Le budget de graines reste declare, mais il ne decide plus de la selectionnabilite.
- *Controle physique empirique.* Regle de contact du contrat §8 (4.3).
  - Seuls les contacts de collider de caisse sont classes ; l'appui des pneus par raycast n'est pas un contact.
  - Fenetre de ±50 pas, avec le `fixedDeltaTime` reel consigne.
  - En run exploratoire, sans ε_t declare, les criteres declenches sont publies comme constats. En acceptation, ils font echouer la campagne.
- *Sequence.* La campagne exploratoire `20260929-141746` devient historique. Le protocole repart : nouvelle campagne exploratoire, puis declaration d'ε_t, puis campagne d'acceptation.

**Bloc fige : matrice.** Ajouter les lignes suivantes :
- objectif intermediaire : `NoRoute` par phase, a distinguer de `NotSelectable` en campagne ;
- contact avec un relief reconnu, puis contact avec un relief non reconnu ;
- talonnage sur la chaussee ;
- `NominalPoseInfeasible` ;
- preuve a pose tangente, qui rend « non couvert ».

**Taches.** Ajouter :
- `Routing/RoutePlan.cs` et `Routing/RoutePlanner.cs` : objectif intermediaire.
- `Lifecycle/TrackingMeasurement.cs` : pose nominale cinematique, terme L_k.
- `Planning/MotionCommand.cs` : cap nominal de route.
- `Lifecycle/TrafficV2VehicleDriver.cs` : classification des contacts.
- Constructeur de campagne dans `Story531DrivenReplayTests`.
- Nouvelle fixture `Story531ViaObjectiveRoutingTests` [Core], avec les tests adversariaux de 4.2 :
  - piege de couplage contre une enumeration exhaustive ;
  - cout du mouvement impose ;
  - cycles ;
  - `NoRoute` contre `NotSelectable` ;
  - replan.
- Fixture de confrontation des modeles de cap sur trace [Geometry].

**Code Map.** Ajouter :
- `RoutePlanner.cs:156-163` (regle du poids nul, preference) ;
- `RoadModelCompiler.cs:44-50` (`RadiusMeters`) ;
- `VehicleSteeringModel.cs:94-102` (direction parallele) ;
- `JunctionClearance` (`DrivableReliefs`).

**Statut :** `in-progress`, conserve. On reprend par `bmad-build` a l'etape 3. Aucune tache ni aucun critere deja coche n'est coche par cette proposition.

## 5. Transmission

**Portee :** moderee.

| Role | Responsabilite |
|---|---|
| Developer (`bmad-build`, 5.31) | Appliquer 4.8 et implementer 5.31 selon les regles du 2026-09-29 (`-Profile Fast` en boucle, aucun script de test maison) ; nouvelle campagne exploratoire ; s'arreter a la declaration d'ε_t |
| Proprietaire | Declarer ε_t ; approuver la campagne d'acceptation ; plus tard, re-signer Gate A en 5.52 |
| Developer (5.52) | Regenerer les preuves selon 4.5 ; publier les residus ; remonter tout echec geometrique |

**Criteres de succes :**
- les artefacts 4.1 a 4.8 sont edites a l'identique ;
- la 5.31 reprend sans critere coche a tort ;
- aucune preuve ni signature n'est modifiee avant la 5.52.

**Sprint-status :** aucun changement (aucune story ajoutee ni retiree).
