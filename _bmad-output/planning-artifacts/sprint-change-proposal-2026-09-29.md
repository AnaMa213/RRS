# Sprint Change Proposal - 2026-09-29

**Sujet :** L'erreur de suivi du vehicule physique n'est couverte par aucune preuve Gate A signee. Il faut inserer une story de couverture et de re-signature avant la Gate B, et ajuster l'AC normative de la 5.31.
**Statut :** PROPOSE. En attente de l'approbation de Kenan. Aucune edition n'est appliquee, aucun artefact Gate A n'est modifie, aucune implementation n'est engagee.
**Mode :** Batch, pour presenter ensemble les editions liees de la 5.31, de la nouvelle story, du contrat et des regles de build.
**Portee :** Moderee. Ajustement direct dans l'Epic 5 : une story inseree, sans nouvel epic et sans changement de perimetre produit.
**Decisions proprietaire deja prises (planification 5.31) :** A1 = a, A2 = a, A3 = a, A4 = a, A5 = b. Leur texte integral est consigne dans la spec 5.31.

## 1. Probleme et preuves

Constate pendant la planification de la 5.31 (`bmad-build`, Cloud, lecture du code seule) :

- La preuve Gate A signee couvre la **reference compilee**. Son empreinte est gonflee de marge + δ_c, et a_e = 0 est lie a la signature par `ClearanceHash`. Le SHA-256 du bloc « Residus » est egal au `ClearanceHash` signe, verifie le 2026-09-29.
- **a_e n'entre dans aucun calcul.** Il n'apparait que dans le texte du rapport (`AuthoredRoadModel.cs:1313,1397,1596`). Le gonflement reel vaut `LateralClearanceMarginMeters + δ_c` (`ConflictSweep.cs:234-236`, `JunctionClearance.cs:110,115`).
- Tout vehicule physique a ε_t > 0. Avec a_e = 0, aucune trajectoire physique n'est couverte. L'AC normative de la 5.31 (`epics.md:2698-2700`) place dans la 5.31 elle-meme :
  - l'ajout de l'allocation a l'outillage ;
  - la regeneration des preuves physique et Sidewalk ;
  - les decisions de paires du proprietaire ;
  - la re-signature ;
  - la clause « 5.31 is not complete until coverage holds ».
  Aucune de ces capacites n'existe dans le code. La 5.31 devient ainsi une story XL a deux livrables independants.
- *Estimation non verifiee :* residu signe minimal ≈ 0,1127 m (physique et Sidewalk, carrefours classiques). Un ε_t de cet ordre rendrait probablement un residu non positif, ce qui signifie un echec geometrique et pas seulement une re-signature.
- Limites de vitesse : toutes valent 0 dans le modele signe. Le champ est un report d'authoring vers la 5.33 (`AuthoringDecisions.cs:63,487`). La limite de courbe par adherence appartient a la 5.33 (C:190, `epics.md:2224`). L'AC 5.31 (`epics.md:2716`) exige pourtant les deux comme contraintes combinees.
- `Idle` : `VehicleDriveIntent.Idle` = (0,0,0,0) applique le frein moteur `coastTorque`, sans frein de service. Sous `MinimumDirectionSpeed` (0,25 m/s), toute entree `BrakeReverse` devient une **marche arriere** (`VehicleTireModel.cs`, `ResolveWheelDriveTorque` / `ResolveWheelBrakeTorque`).

## 2. Impact (checklist)

| Section | Statut | Constat |
|---|---|---|
| 1. Declencheur | [x] | Story 5.31, limitation technique constatee dans le code, preuves ci-dessus |
| 2. Epics | [x] / [!] | Epic 5 realisable. Gate B deplacee apres une story inseree 5.52 (numero libre, verifie). Gates C a E inchangees |
| 3.1 PRD | [N/A] | Pas de PRD separe (precedents du 2026-09-25 et du 2026-09-28). SPEC canonique inchange |
| 3.2 Architecture / contrat | [!] | Contrat Road World Model : precision de ε_t (A2) et repli du composeur V2 (A5). Spine : aucun changement |
| 3.3 UX | [N/A] | Aucun ecran |
| 3.4 Autres artefacts | [!] | `sprint-status.yaml` (5-52) ; `docs/setup/build-workflow-rules.md` §6 (delegation etendue) |
| 4. Voie | [x] | Ajustement direct. Pas de rollback, puisque la 5.30 reste valide. Pas de revue MVP |
| 5. Transfert | [x] | Proprietaire : approbation et signature. Dev local : 5.31 puis 5.52 |

## 3. Voie recommandee

Ajustement direct.
- La **5.31** livre la premiere conduite V2 en **run de mesure explicite** : ε_t declare par le proprietaire apres un run exploratoire, mesures, diagnostics, et refus de toute conduite hors mesure tant que la couverture physique n'est pas etablie. Elle ne ferme pas la Gate B.
- La nouvelle **5.52** fait entrer l'allocation dans chaque preuve Gate A, regenere, reevalue les empreintes et les decisions de conflit reellement affectees, puis soumet la re-signature a l'approbation explicite du proprietaire. **La Gate B se ferme apres la 5.52**, par le jalon PlayMode 1 rejoue hors mode mesure.

Effort : 5.31 de taille L (inchangee hors ce retrait) ; 5.52 de taille M a L. Risque principal : un residu non positif avec l'ε_t mesure. Ce serait alors une decision geometrique du proprietaire, par un correct-course distinct, jamais une allocation prise sur la marge.

## 4. Editions proposees

### 4.1 `epics.md` — table des gates (l. 1683)

**Ancien :** `| **B — V2 Spine Driven End to End** | 5.31 | One AI vehicle spawns, localizes, routes, plans, drives and exits under V2 in `MVP_Run` | **PlayMode milestone 1** |`

**Nouveau :** `| **B — V2 Spine Driven End to End** | 5.52 (after 5.31) | One AI vehicle spawns, localizes, routes, plans, drives and exits under V2 in `MVP_Run`, outside any measurement run, on trajectories covered by Gate A evidence that includes the declared tracking tolerance | **PlayMode milestone 1**, rerun after the 5.52 re-signature |`

### 4.2 `epics.md` — graphe de dependances (l. 1701)

**Ancien :** `... ─► 5.29 ─► 5.30 ─► 5.31 ═GATE B═`
**Nouveau :** `... ─► 5.29 ─► 5.30 ─► 5.31 ─► 5.52 ═GATE B═`

### 4.3 `epics.md` — table des mesures de cout (l. 1786)

**Ancien :** `| V2 spine cost | 5.30 (Gate B) | ...`
**Nouveau :** `| V2 spine cost | 5.31 (before Gate B) | ...`. Cela corrige une incoherence preexistante : la 5.30 n'a pas de chemin runtime, et la 5.31 revendique deja cette mesure (l. 2679).

### 4.4 `epics.md` — Story 5.31

- **l. 2659, fin.**
  - Ancien : `**This closes Gate B.**`
  - Nouveau : `It proves the driven slice under an explicit measurement run; **Gate B closes after Story 5.52** (sprint-change-proposal-2026-09-29.md).`
- **l. 2671, `Planning/SpeedPlan`.**
  - Ancien : `free-road constraints only: desired speed, road limit, curve limit, with the binding constraint named`
  - Nouveau : `free-road constraints: desired speed and steering speed ceiling applied, longitudinal bounds declared; road limit and grip curve limit named and reported as deferred to 5.33, never applied, with the binding constraint named`
- **l. 2673, deux passages.**
  - Ancien : `or \`Idle\` when no valid plan exists; a non-finite intent or authority scalar is diagnosed and replaced by \`Idle\` before physics`
  - Nouveau : `or the V2 fallback brake command when no valid plan exists; a non-finite intent or authority scalar is diagnosed and replaced by the fallback before physics`
- **l. 2679.**
  - Ancien : `**Completion evidence:** **Gate B** — EditMode suite green; PlayMode milestone 1 recorded with its raw output; ...`
  - Nouveau : `**Completion evidence:** EditMode suite green; PlayMode milestone 1 recorded under an explicit measurement run with its raw output, the owner-declared ε_t (after an exploratory run) and the published coverage verdict; ...` (le reste de la ligne est inchange). Ajouter a la suite : `Gate B is not claimed by this story.`
- **l. 2681 (Unlocks).** Prefixer : `Story 5.52 integrates the declared ε_t into the Gate A evidence; after its re-signature, `.
- **l. 2697-2700.** Remplacer les quatre lignes par :
  - `**And** ε_t bounds the maximum displacement of every point of the maximum-gauge footprint (translation and heading), relative to the reference pose at the corresponding progress, including between physics steps under a declared conservative interval rule; an exploratory measurement run measures it, the owner then declares the bound, and acceptance runs verify it; any observed displacement beyond the declared ε_t fails`
  - `**And** the coverage verdict (max |o(s)| + ε_t ≤ a_e of the valid evidence) is published; while it does not hold, no V2 vehicle drives outside an explicit measurement run, a test proves the refusal, and the measurement authorization is reserved to the test protocol and cannot enable normal operation`
  - `**And** integrating ε_t into the Gate A evidence and re-signing Gate A belong to Story 5.52; this story never regenerates or signs Gate A evidence, and never converts the reserved clearance margin or a residual clearance into a tracking allowance`
- **l. 2704.**
  - Ancien : `or \`Idle\` when no valid plan exists`
  - Nouveau : `or the V2 fallback brake command when no valid plan exists`
- **l. 2710.**
  - Ancien : `**Then** it is diagnosed and replaced by \`Idle\` before physics`
  - Nouveau : `**Then** it is diagnosed and replaced before physics by the V2 fallback brake command: finite, bounded, valid for one physics step, service brake only above the direction-change speed and no brake input below it; the vehicle stays present with a diagnostic, without despawn, teleport or forced realignment; the global semantics of \`VehicleDriveIntent.Idle\` used by V1 are unchanged`
- **l. 2716.**
  - Ancien : `**Then** it combines desired speed, road limit, curve limit and the steering speed ceiling as named constraints and identifies the binding one`
  - Nouveau : `**Then** it applies desired speed, the steering speed ceiling and the declared longitudinal bounds as named constraints and identifies the binding one; road limit and the grip-based curve limit are named, reported as deferred to Story 5.33 and not applied, and an authored non-zero road limit is reported as authored-but-not-applied, never as the absence of a limit`

### 4.5 `epics.md` — Story 5.33 (Artifacts, l. 2801)

Ajouter a la fin : `This story reopens the deferred authored speed-limit field (Story 5.28 authoring decisions, `AuthoringDecisions`) and applies the road limit and the grip-based curve limit that Story 5.31 publishes as deferred.`

### 4.6 `epics.md` — nouvelle Story 5.52, inseree apres la 5.31

```markdown
### Story 5.52: Tracking-Error Coverage in Gate A Evidence and Owner Re-Signature

**Type:** FOUNDATION (evidence) · **Boundary:** Road model evidence · **Complexity:** M
**Implements:** AD-37, Road World Model contract §8 (Gate A evidence lifecycle), SimulationInvariant 7
*Inserted 2026-09-29 (sprint-change-proposal-2026-09-29.md): Story 5.31 planning found that the signed allowance a_e is recorded text, not an input of any proof, so the first-signed evidence (a_e = 0) covers no physical vehicle.*

As the owner,
I want the Gate A evidence regenerated with the tracking tolerance declared and measured in Story 5.31,
So that V2 vehicles drive outside measurement runs only on trajectories the signed evidence covers.

**Prerequisites:** 5.31 (owner-declared ε_t and its measurement record).

**Capability delivered:** the allowance a_e = max |o(s)| + ε_t enters, beside the reserved margin and δ_c, the inflation of every Gate A proof: conflict candidates, junction physical clearance, `Sidewalk` planar clearance, roundabout residuals. Candidates and clearance are regenerated, and the candidate diff and fingerprints are published. The signed roundabout seam list moves into signed data. A new sign-off record is bound to the regenerated evidence, and the previous record is kept as superseded history.

**Rules:** the reserved clearance margin and any residual clearance are never converted into allowance. Pairs whose conflict geometry fingerprint changed are materially changed: a decision bound to a previous conflict geometry is never reused silently, and becomes a historical proposal to reconfirm. New pairs and orphaned decisions are handled explicitly. The approved delegated decision mechanism `5.50-AUTO-DECISIONS-v1` applies under all its conditions where applicable. Any non-positive residual fails; a geometry change is an owner decision through a separate course correction. Gate A is re-signed only by the owner, explicitly, never automatically.

**Non-goals:** no geometry change, no driving-behavior change, no change of the declared ε_t (a new value reruns this story).

**EditMode verification:** a synthetic allowance enlarges every proof's inflation exactly by a_e; changed, new and orphaned pairs are classified; a second run proposes no change; a stale fingerprint closes Gate A; the evidence binding reads a_e from the new record and equals the declared allowance.

**PlayMode verification — MILESTONE 1 rerun:** in `MVP_Run`, outside any measurement run, one V2 vehicle spawns at an entry portal, drives covered trajectories within ε_t and v ≤ v*(s), and despawns at an exit portal.

**Completion evidence:** **Gate B** — EditMode green, regenerated evidence and diff published, owner re-signature recorded, PlayMode milestone 1 rerun with raw output.

**Acceptance Criteria:**

**Given** the ε_t declared in Story 5.31
**When** the Gate A evidence is regenerated
**Then** every proof's inflation includes a_e = max |o(s)| + ε_t in addition to the reserved margin and δ_c, and each residual is published per movement and corner
**And** any non-positive residual fails without allocating margin or residual clearance

**Given** the regenerated conflict candidates
**When** they are compared with the signed set
**Then** new, materially changed and orphaned pairs are identified by fingerprint, no decision bound to a previous conflict geometry is reused silently, and delegated decisions satisfy every condition of `5.50-AUTO-DECISIONS-v1`

**Given** the regenerated evidence
**When** Gate A is re-signed
**Then** only the owner signs, on a new record bound to the regenerated evidence, and the previous record is kept as superseded history

**Given** the new signature
**When** the milestone 1 PlayMode test runs outside any measurement run
**Then** one V2 vehicle drives portal to portal on covered trajectories, and Gate B closes
```

### 4.7 Contrat `ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md`

- **§8, puce « Lateral quantities are distinct », a la fin :** ajouter *2026-09-29 :* `ε_t bounds the maximum displacement of every point of the maximum-gauge footprint, translation and heading included, relative to the reference pose at the corresponding progress, with a declared conservative rule between physics steps; the reserved margin and any residual clearance are never converted into an allowance.`
- **§8, puce « Gate A evidence lifecycle », a la fin :** ajouter `The allowance enters the inflation of every proof; a value recorded only as text covers nothing.`
- **§10 (composeur), l. 445-447 :** ajouter *2026-09-29 :* `For Traffic V2, the replacement output is a finite, bounded fallback brake command valid for one physics step (service brake only above the direction-change speed); the global meaning of \`VehicleDriveIntent.Idle\` is unchanged. This is not the Safety Filter.`

### 4.8 `docs/setup/build-workflow-rules.md` §6

Ajouter : `2026-09-29 (sprint-change-proposal-2026-09-29.md) : la meme exception, sous toutes ses conditions, s'applique a la Story 5.52 pour les paires dont l'empreinte de conflit a change ; elle n'autorise ni la revue ni la signature de Gate A.` La phrase « Cette exception ne s'etend a aucune autre story » devient « … a aucune autre story que 5.52 ».

### 4.9 `sprint-status.yaml`

Inserer apres `5-31-...: backlog` :
```yaml
  # Inserted 2026-09-29 (sprint-change-proposal-2026-09-29.md): tracking-error
  # coverage in Gate A evidence and owner re-signature. Gate B closes after it.
  5-52-tracking-error-coverage-in-gate-a-evidence-and-owner-re-signature: backlog
```

## 5. Transfert et approbation

- **Proprietaire (Kenan) :** approuver cette proposition. Ensuite, et seulement ensuite, les editions 4.1 a 4.9 sont appliquees en un commit documentaire. Aucune signature Gate A n'est impliquee.
- **Developpement local :** 5.31 (spec `ready-for-dev` apres approbation), puis 5.52 (spec a produire via `bmad-build`).
- **Criteres de succes :**
  - la 5.31 ne revendique pas la Gate B ;
  - aucune decision de conflit n'est reutilisee sur une geometrie changee ;
  - aucune allocation n'est prise sur la marge ;
  - la Gate B est constatee apres la re-signature du proprietaire.
