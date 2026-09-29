# Sprint Change Proposal - 2026-09-29

**Statut :** APPROUVE par Kenan le 2026-09-29 (revision 2, consolidee) et APPLIQUE : editions 4.1 a 4.9 dans `epics.md`, le contrat Road World Model, `build-workflow-rules.md` et `sprint-status.yaml` ; repercussions 4.10 dans la spec 5.31, passee `ready-for-dev`. Aucun artefact Gate A n'est modifie, aucune signature n'est impliquee, aucune implementation n'est engagee. Ce texte remplace integralement les revisions 0 et 1.
**Sujet :** L'erreur de suivi du vehicule physique n'est couverte par aucune preuve Gate A signee. La proposition insere une story de couverture et de re-signature (5.52) avant la Gate B, et ajuste l'AC normative de la 5.31.
**Mode :** Batch. **Portee :** moderee, ajustement direct dans l'Epic 5, sans nouvel epic et sans changement de perimetre produit.
**Decisions proprietaire prises :** A1 = a, A2 = a, A3 = a, A4 = a, A5 = b (planification 5.31). Revision demandee le 2026-09-29 sur trois points : texte normatif, portee de la borne entre deux pas, freinage multi-pas.

## 1. Probleme et preuves

Tout ce qui suit a ete constate par lecture du code et des donnees dans le Cloud. Rien n'a ete execute dans Unity.

- La preuve Gate A signee couvre la **reference compilee**, gonflee de marge + δ_c.
- a_e = 0 est lie a la signature par `ClearanceHash`, verifie le 2026-09-29. Mais a_e **n'entre dans aucun calcul** : il n'apparait que dans le texte du rapport (`AuthoredRoadModel.cs:1313,1397,1596`). Le gonflement reel vaut marge + δ_c (`ConflictSweep.cs:234-236`, `JunctionClearance.cs:110,115`).
- Tout vehicule physique a ε_t > 0 : aucune trajectoire physique n'est donc couverte. L'AC actuelle place dans la 5.31 la regeneration, les decisions de paires et la re-signature (`epics.md:2698-2700`), alors qu'aucune de ces capacites n'existe.
- **Empreinte de paire** (`PairGeometryFingerprint.cs:102-107`) : schema v1, echantillons des deux mouvements et volume de zone. Le volume depend du gonflement, mais les parametres de gonflement (marge, δ_c, a_e) n'y sont pas inscrits explicitement.
- **Physique** :
  - solveur PGS (`DynamicsManager.asset:38`) ;
  - detection de collision discrete (`m_CollisionDetection: 0`) ;
  - centre de masse explicite (0 ; −0,35 ; 0), donc horizontalement au point de reference (essieux a z = ±1,55) ;
  - collider de 2,06 × 1,42 × 4,44 m, centre a l'origine.
- **Freinage** (`VehicleTireModel.cs`) : `BrakeReverse` freine si v > v_dir = 0,25 m/s et **engage la marche arriere** si v ≤ v_dir, vitesses negatives comprises. Sans entree, c'est `coastTorque` qui s'applique. Le frein a main (`VehiclePhysicsBody.cs:449-466`) applique max(frein de service, 4 500) aux roues non directrices, quel que soit le sens de marche.
- **Limites de vitesse** : elles valent toutes 0, car le champ est reporte a la 5.33 (`AuthoringDecisions.cs:63,487`). La limite de courbe par adherence releve aussi de la 5.33 (C:190).
- **Topologie** : les 72 mouvements sont tous sur un chemin entree → sortie, et aucun poids n'est nul.
- **Borne entre deux pas** : la regle `max(d_k, d_k+1) + δ/2` du lemme 5.50 facture toute la translation le long de la route. A 8 m/s sur l'anneau de 6 m, δ/2 = 0,113 m, soit le residu signe minimal entier (0,1127 m).

## 2. Impact

| Section | Etat | Constat |
|---|---|---|
| Epics | [!] | Gate B deplacee apres la 5.52 ; AC 5.31 et 5.33 ajustees ; Gates C a E inchangees |
| PRD | [N/A] | Pas de PRD separe (precedents du 2026-09-25 et du 2026-09-28) |
| Contrat Road World Model | [!] | ε_t et sa portee, allocation effective, repli du composeur V2 |
| Spine / UX | [N/A] | Aucun changement |
| Regles de build | [!] | §6 : delegation etendue a la 5.52 |
| `sprint-status.yaml` | [!] | Entree 5-52 |
| Spec 5.31 | [!] | Contenu approuve, bloc fige : repercussions listees en 4.10 |

## 3. Voie recommandee

Ajustement direct.
- La **5.31** livre la tranche verticale en run de mesure explicite, la campagne tracable, la declaration d'ε_t par le proprietaire et le refus hors mesure. Elle ne revendique pas la Gate B.
- La **5.52** fait entrer l'allocation dans chaque preuve, reevalue toutes les decisions de conflit dont l'enveloppe change et prepare ta re-signature.
- La **Gate B** se ferme uniquement apres cette re-signature, sur un rejeu hors mode mesure, avec la portee de preuve definie en 4.1.
- Aucun seuil n'est ajuste pour obtenir un verdict.

## 4. Editions normatives (formulations definitives)

### 4.1 `epics.md` — table des gates (l. 1683)

**Ancien :** `| **B — V2 Spine Driven End to End** | 5.31 | One AI vehicle spawns, localizes, routes, plans, drives and exits under V2 in `MVP_Run` | **PlayMode milestone 1** |`

**Nouveau :** `| **B — V2 Spine Driven End to End** | 5.52 (after 5.31) | One AI vehicle spawns, localizes, routes, plans, drives and exits under V2 in `MVP_Run`, outside any measurement run, on trajectories covered by Gate A evidence that includes the declared tracking tolerance. Coverage is established at every simulated physics step and, between steps, only under the per-step-verified integration model of Story 5.31; continuous physical coverage beyond that model is not claimed | **PlayMode milestone 1**, rerun after the 5.52 re-signature |`

### 4.2 `epics.md` — graphe (l. 1701)

`... ─► 5.30 ─► 5.31 ═GATE B═` devient `... ─► 5.30 ─► 5.31 ─► 5.52 ═GATE B═`.

### 4.3 `epics.md` — table des couts (l. 1786)

`| V2 spine cost | 5.30 (Gate B) | ...` devient `| V2 spine cost | 5.31 (before Gate B) | ...`. Cela corrige une incoherence preexistante : la 5.30 n'a pas de chemin runtime.

### 4.4 `epics.md` — Story 5.31

Terme unique : **« V2 fallback command »**, defini en l. 2708-2711 et repris tel quel ailleurs.

| Ligne | Ancien | Nouveau |
|---|---|---|
| 2659 (fin) | `**This closes Gate B.**` | `It proves the driven slice under an explicit measurement run; **Gate B closes after Story 5.52** (sprint-change-proposal-2026-09-29.md).` |
| 2671 | `free-road constraints only: desired speed, road limit, curve limit, with the binding constraint named` | `free-road constraints: desired speed and steering speed ceiling applied with declared longitudinal bounds; road limit and grip curve limit named, reported as deferred to 5.33 and never applied; binding constraint named` |
| 2673 | `or \`Idle\` when no valid plan exists; a non-finite intent or authority scalar is diagnosed and replaced by \`Idle\` before physics` | `or the V2 fallback command when no valid plan exists; a non-finite intent or authority scalar is diagnosed and replaced by the V2 fallback command before physics` |
| 2679 | `**Completion evidence:** **Gate B** — EditMode suite green; PlayMode milestone 1 recorded with its raw output;` | `**Completion evidence:** EditMode suite green; the measurement campaigns recorded with raw output, the owner-declared ε_t and the published coverage verdict;` (la suite de la ligne est inchangee) ; ajouter `Gate B is not claimed by this story.` |
| 2681 | *Unlocks* | prefixer : `Story 5.52 integrates the declared ε_t into the Gate A evidence; after its re-signature, ` |
| 2704 | `or \`Idle\` when no valid plan exists` | `or the V2 fallback command when no valid plan exists` |
| 2716 | `**Then** it combines desired speed, road limit, curve limit and the steering speed ceiling as named constraints and identifies the binding one` | `**Then** it applies desired speed, the steering speed ceiling and the declared longitudinal bounds as named constraints and identifies the binding one; road limit and the grip-based curve limit are named, reported as deferred to Story 5.33 and not applied; an authored non-zero road limit is reported as authored-but-not-applied, never as the absence of a limit` |

**l. 2697-2700** (les quatre lignes actuelles sont remplacees par les six suivantes ; leurs exigences encore valables y sont reprises : ε_t declare et mesure, depassement = echec, verdict de couverture publie, refus hors mesure prouve par test) :

- `**And** ε_t bounds the maximum displacement, projected on the road plane, of the eight corners of the maximum-gauge box (maximum footprint extruded to the vehicle's collider height, centred on the reference point), relative to the upright reference pose at matched progress; it is evaluated exactly at every simulated physics step, so roll, pitch, heading and translation are all included`
- `**And** between two physics steps the bound holds only under the integration model M: over one step the body moves with the step's final linear and angular velocities. M is verified per step against the recorded pose change within a declared tolerance; an interval that fails the check is reported model-not-verified and counts as not measured. Under M the bound is the maximum on a sub-grid plus the Lipschitz remainder L·h/2, where L sums the actual corner speed bound and the matched nominal speed bound; intervals are split at every crossed seam, and both one-sided nominal poses are evaluated there. No continuous physical guarantee is claimed beyond M, and any contact of the vehicle with a non-road collider during a campaign fails`
- `**And** an exploratory campaign measures the displacement, the owner then declares ε_t, and an acceptance campaign verifies it. Every one of the 72 movements, the 24 roundabout seams (each side) and the 44 corridors is reported Measured, NotMeasured or NotSelectable, with its maxima; only Measured counts as covered, and any other status fails acceptance. Long campaigns run separately from, and in addition to, the default suite; any displacement beyond the declared ε_t fails, and a runtime monitor evaluates the per-step bound for every V2 vehicle`
- `**And** the coverage verdict (max |o(s)| + ε_t ≤ a_e of the valid evidence) is published; while it does not hold, no V2 vehicle drives outside an explicit measurement run, a test proves the refusal, and the measurement authorization is constructed only by the test protocol and cannot enable normal operation`
- `**And** integrating ε_t into the Gate A evidence and re-signing Gate A belong to Story 5.52; this story never regenerates or signs Gate A evidence, and never converts the reserved clearance margin or a residual clearance into a tracking allowance`
- `**And** no threshold is relaxed to obtain a favorable verdict`

**l. 2708-2711** (remplacees) :

- `**Given** a non-finite value reaches the composer or physics boundary, a plan whose source frame is no longer valid, or no valid plan`
- `**When** the intent is emitted`
- `**Then** it is diagnosed and replaced before physics by the V2 fallback command, recomputed every physics step from the measured longitudinal speed v: above the service band v_dir + 2·b·Δt (v_dir the direction-change speed, b the fallback deceleration, Δt the physics step) it commands service brake only; at or below that band, and while rolling backward, it commands handbrake hold with zero brake-reverse and zero throttle, so that \`VehicleTireModel\` never receives a brake-reverse input at a speed where it would produce reverse drive torque`
- `**And** this holds for both the intent axes and the authority scalars, over successive steps, until a diagnosed held stop or an explicit stop-overrun diagnostic; the vehicle stays present without despawn, teleport or forced realignment; the global semantics of \`VehicleDriveIntent.Idle\` used by V1 are unchanged, and no broader safety behavior is implemented here, because the \`SafetyFilter\` boundary belongs to Story 5.37`
- `**And** a PlayMode physics test proves over several steps, at speeds near zero and negative, the resulting motion, the absence of commanded reverse drive torque, the stop or overrun diagnostic, and that the vehicle remains in the world`

### 4.5 `epics.md` — Story 5.33 (Artifacts, l. 2801)

Ajouter : `This story reopens the deferred authored speed-limit field (Story 5.28 authoring decisions, \`AuthoringDecisions\`) and applies the road limit and the grip-based curve limit that Story 5.31 publishes as deferred.`

### 4.6 `epics.md` — nouvelle Story 5.52, inseree apres la 5.31

```markdown
### Story 5.52: Tracking-Error Coverage in Gate A Evidence and Owner Re-Signature

**Type:** FOUNDATION (evidence) · **Boundary:** Road model evidence · **Complexity:** M
**Implements:** AD-37, Road World Model contract §8 (Gate A evidence lifecycle), SimulationInvariant 7
*Inserted 2026-09-29 (sprint-change-proposal-2026-09-29.md): Story 5.31 planning found that the signed allowance a_e is recorded text, not an input of any proof, so the first-signed evidence (a_e = 0) covers no physical vehicle.*

As the owner,
I want the Gate A evidence regenerated with the tracking tolerance declared and measured in Story 5.31,
So that V2 vehicles drive outside measurement runs only on trajectories the signed evidence covers.

**Prerequisites:** 5.31 (owner-declared ε_t and its accepted, fully Measured campaign).

**Capability delivered:** a_e = max |o(s)| + ε_t enters, beside the reserved margin and δ_c, the inflation of every Gate A proof: conflict candidates, junction physical clearance, `Sidewalk` planar clearance, roundabout residuals. The pair fingerprint schema is versioned to record the inflation parameters (margin, δ_c, a_e) explicitly. Candidates and clearance are regenerated, with candidate diff and fingerprints published. The signed roundabout seam list moves into signed data. A new sign-off record is bound to the regenerated evidence; every previous record is kept as superseded history.

**Rules:**
- ε_t keeps the Story 5.31 definition and scope (per-step exact, model M between steps, no continuous physical claim).
- The reserved clearance margin and any residual clearance are never converted into allowance.
- Because a_e enlarges every inflated envelope, every pair's envelope changes: every prior decision becomes a historical proposal, and none is carried over without re-evaluation on the new envelope. New and orphaned pairs are handled explicitly.
- The approved mechanism `5.50-AUTO-DECISIONS-v1` applies under all its conditions where applicable.
- Any non-positive residual fails; a geometry change is an owner decision through a separate course correction.
- Gate A is re-signed only by the owner, explicitly, never automatically.
- This story decides the normal-operation response to a runtime `TrackingToleranceExceeded` diagnostic.

**Non-goals:** no geometry change, no driving-behavior change, no change of the declared ε_t (a new value reruns this story).

**EditMode verification:** a synthetic allowance enlarges every proof's inflation exactly by a_e; the fingerprint changes when any inflation parameter changes; no decision is reused without re-evaluation; a second run proposes no change; a stale fingerprint closes Gate A; the evidence binding reads a_e from the new record and equals the declared allowance.

**PlayMode verification — MILESTONE 1 rerun:** in `MVP_Run`, outside any measurement run, V2 vehicles spawn at entry portals, drive covered trajectories within ε_t (per-step bound and model-M inter-step bound) and v ≤ v*(s), and despawn at exit portals, without contact with non-road colliders.

**Completion evidence:** **Gate B** — EditMode green, regenerated evidence and diff published, owner re-signature recorded, PlayMode milestone 1 rerun with raw output, within the proof scope stated in the gate table.

**Acceptance Criteria:**

**Given** the ε_t declared in Story 5.31
**When** the Gate A evidence is regenerated
**Then** every proof's inflation includes a_e = max |o(s)| + ε_t in addition to the reserved margin and δ_c, each residual is published per movement and corner, and the pair fingerprints record the inflation parameters
**And** any non-positive residual fails without allocating margin or residual clearance

**Given** the regenerated conflict candidates
**When** they are compared with the signed set
**Then** every pair whose inflated envelope changed has its prior decision downgraded to a historical proposal and re-evaluated; new and orphaned pairs are identified; no decision bound to a previous envelope is reused silently; delegated decisions satisfy every condition of `5.50-AUTO-DECISIONS-v1`

**Given** the regenerated evidence
**When** Gate A is re-signed
**Then** only the owner signs, on a new record bound to the regenerated evidence, and every previous record is kept as superseded history

**Given** the new signature
**When** the milestone 1 PlayMode test runs outside any measurement run
**Then** V2 vehicles drive portal to portal on covered trajectories within the stated proof scope, and Gate B closes
```

### 4.7 Contrat `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`

- **§8, « Lateral quantities are distinct », fin** — ajouter *2026-09-29 :* `ε_t bounds the road-plane displacement of the eight corners of the maximum-gauge box relative to the upright reference pose at matched progress. It is exact at every simulated physics step; between steps it holds only under the per-step-verified integration model of Story 5.31, with a Lipschitz remainder and seam splitting, and it is never presented as a continuous physical guarantee beyond that model. The reserved margin and any residual clearance are never converted into an allowance.`
- **§8, « Gate A evidence lifecycle », fin** — ajouter `The allowance enters the inflation of every proof and is recorded in the pair fingerprints; a value recorded only as text covers nothing. When an inflated envelope changes, no prior pair decision is reused without re-evaluation.`
- **§10, l. 445-447** — ajouter *2026-09-29 :* `For Traffic V2, the replacement output is the V2 fallback command, recomputed every physics step from measured speed: service brake only above the band v_dir + 2·b·Δt, handbrake hold with zero brake-reverse at or below it and while rolling backward, until a diagnosed held stop or stop-overrun. The global meaning of \`VehicleDriveIntent.Idle\` is unchanged. This is not the Safety Filter.`

### 4.8 `docs/setup/build-workflow-rules.md` §6

Ajouter : `2026-09-29 (sprint-change-proposal-2026-09-29.md) : la meme exception, sous toutes ses conditions, s'applique a la Story 5.52 pour la reevaluation de toutes les paires dont l'enveloppe gonflee change ; elle n'autorise ni la revue ni la signature de Gate A.` La phrase « ne s'etend a aucune autre story » devient « … a aucune autre story que 5.52 ».

### 4.9 `sprint-status.yaml`

Inserer apres `5-31-...: backlog` :
```yaml
  # Inserted 2026-09-29 (sprint-change-proposal-2026-09-29.md): tracking-error
  # coverage in Gate A evidence and owner re-signature. Gate B closes after it.
  5-52-tracking-error-coverage-in-gate-a-evidence-and-owner-re-signature: backlog
```

### 4.10 Repercussions sur la spec 5.31

Son contenu a ete approuve le 2026-09-29 ; le bloc fige est renegocie par cette proposition. Changements a porter en meme temps que les editions 4.1 a 4.9 :

- **Mesure.**
  - Les 8 coins de la boite (gabarit extrude a la hauteur du collider) remplacent les 4 coins du plan.
  - Le modele M remplace H1 : vitesses de fin de pas, verifie a chaque pas, echec → `ModelNotVerified` = non mesure.
  - La constante de Lipschitz est calculee avec la distance 3D max des coins au centre de masse.
  - Decoupe aux raccords, avec evaluation des deux poses nominales, au lieu du terme ζ.
  - Echec sur tout contact avec un collider hors chaussee.
- **Repli.**
  - Bande de service v_dir + 2·b·Δt (0,41 m/s avec b = 4 m/s² et Δt = 0,02 s), au lieu de v_dir seul, pour couvrir un pas de latence entre la mesure et l'application.
  - Table de conversion `VehicleTireModel` (§5 ci-dessous).
  - Banc physique multi-pas : depart dans la bande (0,35 m/s) ajoute ; couple moteur de chaque roue recalcule a chaque pas par `VehicleTireModel.ResolveWheelDriveTorque` avec les entrees et la vitesse reelles, et il doit etre ≥ 0 ; vehicule present, pose finie, au-dessus du sol.
- **Campagne.** Statuts `Measured` / `NotMeasured` / `NotSelectable` par element (72 + 24×2 + 44), dont `ModelNotVerified`, qui compte comme non mesure.

## 5. Conversion effective de la commande de repli

Donnees : v la vitesse longitudinale mesuree, v_dir = 0,25 m/s, b = `SafeBrakingLimit` (4 m/s²), Δt = 0,02 s, bande de service v_s = v_dir + 2·b·Δt = 0,41 m/s.

| Regime (v mesure au pas k) | Commande emise | Couple moteur (`ResolveWheelDriveTorque`) | Couple de frein (`ResolveWheelBrakeTorque` et frein a main) |
|---|---|---|---|
| v > v_s | gaz 0, `BrakeReverse` = b_f ∈ ]0, 1], frein a main 0 | 0 tant que v > v_dir au moment de l'application | b_f × `BrakeTorque` sur chaque roue |
| −∞ < v ≤ v_s (y compris en recul) | gaz 0, `BrakeReverse` 0, frein a main 1 | 0 (aucune entree de gaz ni de marche arriere) | roues non directrices : max(service, 4 500) ; roues directrices : `coastTorque` |

- **Risque residuel.** Si la vitesse chute de plus de 2·b·Δt en un seul pas (par exemple lors d'un choc), une commande emise au-dessus de v_s peut etre appliquee avec v ≤ v_dir et produire un couple de marche arriere pendant un pas.
- Ce cas n'est pas exclu par construction. Il est **mesure** : le couple recalcule a chaque pas doit etre ≥ 0 dans le banc et dans les campagnes ; tout couple negatif pendant un repli est un echec publie.
- L'ordre d'execution entre le driver V2 et `VehiclePhysicsBody` dans `FixedUpdate` n'est pas connu : la bande de deux pas couvre les deux ordres.

## 6. Limites de preuve qui restent ouvertes

1. **Mouvement continu reel.**
   - La simulation ne definit l'etat qu'aux pas physiques (PGS, detection discrete).
   - Entre deux pas, la borne vaut sous le modele M, verifie a chaque pas contre la variation de pose enregistree. Que PGS integre les positions avec les vitesses de fin de pas est une connaissance generale de PhysX, **non verifiee dans ce depot** ; la verification par pas l'etablit, ou marque l'intervalle non mesure.
   - Aucune majoration du mouvement physique continu hors de M n'est demontrable en 5.31.
   - **Effet sur la Gate B :** son critere est formule dans cette portee (4.1). Elle ne pourra pas etre presentee comme une preuve continue inconditionnelle.
2. **Caractere empirique d'ε_t.**
   - La valeur declaree est verifiee sur la campagne, pas prouvee hors des conditions parcourues (graines, profils, commit).
   - Le moniteur runtime la surveille a chaque pas. La reponse en fonctionnement normal est decidee en 5.52.
3. **Selectionnabilite.** Que `RoutePlanner` choisisse chaque mouvement dans le budget de graines est inconnu. Un `NotSelectable` bloque la campagne et renvoie la decision au proprietaire.
4. **Contenance du gabarit (H3).**
   - Verifiee statiquement sur le prefab V1 : aucune marge en largeur (2,06 = 2,06).
   - Le prefab V2 doit la reverifier.
   - Le roulis est desormais mesure par les 8 coins ; son ampleur reelle est inconnue.
5. **Budget geometrique.**
   - *Estimation non verifiee :* residu minimal signe ≈ 0,1127 m.
   - Un ε_t mesure proche ou superieur fera echouer la 5.52 sur la geometrie. Ce serait une decision proprietaire par un correct-course distinct, jamais un ajustement de seuil.
6. **Repli.** La marche arriere n'est pas exclue en cas de chute de vitesse superieure a 2·b·Δt en un pas ; elle est seulement detectee (§5). Le maintien au frein a main est a mesurer.

## 7. Transfert et approbation

- **Proprietaire (Kenan) :** approuver cette revision 2. Ensuite seulement, les editions 4.1 a 4.9 et les repercussions 4.10 sont appliquees en un commit documentaire. Aucune signature Gate A n'est impliquee.
- **Developpement local :** 5.31 (`ready-for-dev` apres application), puis 5.52 (spec via `bmad-build`).
- **Criteres de succes :**
  - la 5.31 ne revendique pas la Gate B ;
  - chaque element est rapporte avec un statut explicite ;
  - aucune decision de conflit n'est reutilisee sur une enveloppe changee ;
  - aucune allocation n'est prise sur la marge ou un residu ;
  - la Gate B est constatee apres ta re-signature, dans la portee enoncee.
