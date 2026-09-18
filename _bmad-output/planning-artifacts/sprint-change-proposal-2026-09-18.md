# Sprint Change Proposal — 2026-09-18

**Sujet :** Fondation physique vehicule (chassis, roues, suspension, pneumatiques) inseree dans l'Epic 5
avant les travaux de trafic restants, et reaffectation d'`ANO-5.10-03`.

**Statut :** **approuve et applique le 2026-09-18.** Les sections 4.1 a 4.8 sont deja presentes dans les
fichiers cites ; ce document en est la trace (declencheur, preuves, impact, justification, handoff).

**Classification de portee :** **Moderate** — ajout de 5 stories et reorganisation de backlog a l'interieur
d'un epic existant. Aucun replan fondamental, aucun changement de perimetre MVP 1.

**Declencheur :** recette manuelle de la Story 5.10 dans `MVP_Run`, le 2026-09-18.

**Impact sur les Epics 0 a 4 :** **aucun.** Aucune story en `review` ou `done` n'est rouverte ni
renumerotee.

---

## 1. Resume du probleme

### Declencheur

La recette de la Story 5.10 a montre trois comportements non credibles, tous reproductibles dans
`MVP_Run` : des vehicules **projetes en l'air** par une bordure de trottoir, des vehicules **arretes net**
par une levre basse, et un vehicule IA qui **force son itineraire** apres une collision au lieu de reagir
au choc. Le troisieme est enregistre comme anomalie `ANO-5.10-03` (`severity: major`, `status: open`).

### Nature du probleme

**La cause est unique et commune aux trois symptomes : il n'existe aucun modele physique de vehicule.**
Le vehicule est pilote en ecrivant `Rigidbody.linearVelocity` en bloc, seule la composante verticale etant
preservee, sans roue, sans suspension et sans force de pneumatique. Toute face verticale rencontree devient
alors une impulsion de depenetration PhysX. Le lisseur de contact de surface et la regle d'authoring
« aucun collider de module ne s'eleve au-dessus du plan de roulage » ne sont pas des choix de level design :
ce sont des **compensations de l'absence de suspension**, et elles interdisent toute bordure franchissable.

Corollaire : les trois symptomes ne sont pas corrigeables dans la Story 5.10. `ANO-5.10-03` AC7 exige que le
vehicule *puisse* quitter temporairement la chaussee apres une esquive — ce qui suppose une bordure
franchissable — et AC2 exige que le controleur ne combatte plus la resolution PhysX.

### Preuves

1. **Regle de conduite mesuree** (`docs/setup/story-5-10-lane-graph-district-notes.md`) : « le vehicule est
   pilote en ecrivant `linearVelocity`, sans roue ni suspension : toute face verticale devient une impulsion
   de depenetration, donc un envol, un arret net ou un declenchement du hook de degats de la Story 3.5. »
   C'est pourquoi les trottoirs du district sont **affleurants (0 m)**.
2. **Calibration du lisseur** : collider vehicule `2.06 x 1.42 x 4.44`, centre `y = 0.73` → dessous du
   vehicule a `+0.02 m` ; la bande `surfaceContactTolerance = 0.1f` couvre `+0.02 .. +0.12 m`. Le predicat
   `IsSurfaceContact` n'existe que pour compenser l'absence de suspension.
3. **Integration en boucle ouverte** : `NetworkedAIVehicleDriverController` integre sa propre vitesse et
   ecrit `Rigidbody.linearVelocity`. `arrivalRadius = 3f` depasse l'ecart des noeuds de decision de
   carrefour (**2,83 m**), ce qui borne la coupe de l'interieur des virages.
4. **Le modele arcade d'AD-7 n'a jamais ete construit** : sa condition de reouverture (« l'arcade est
   amusante mais il manque un ressenti precis, non reglable simplement ») n'est pas atteinte — il n'y a
   **aucun modele de roue a regler**. AD-35 construit ce qu'AD-7 demandait.
5. **Contrainte de verification decisive** : la suite PlayMode est **rouge 3/33 et non filtrable**
   (mesure du 2026-09-18 consignee dans `docs/setup/devworkflow-rollout.md`), et l'une des fixtures rouges
   est `Story57AiTrafficClientPresentationPlayModeTests` — le domaine meme de la Story 5.14.
6. **Le bump du plan de sol est accepte, pas les degats** : la marche de 5 cm entre une dalle de module et
   le plan de sol de la carte reste ; l'humain a explicitement refuse tout deplacement de collider.
7. **Familie de cause deja rencontree** (`ANO-5.10.02`) : `MeshCollider` non convexes sur les giratoires,
   colliders coplanaires, disque de giratoire sous-dimensionne. Un vehicule pilote via `linearVelocity` ne
   pardonne aucune irregularite de collider ; cette correction-la a traite la geometrie, pas la conduite.

---

## 2. Analyse d'impact

### 2.1 Impact Epic

| Epic | Impact |
| --- | --- |
| **Epic 5** | **Porte les 5 nouvelles stories et la renumerotation.** Titre enrichi : « Vehicle Physics, NPC Response Foundation and Routed Traffic ». 23 stories au lieu de 18. L'epic reste un bac a sable de fondation : le district greybox demeure un terrain d'essai explicitement borne, pas la ville du MVP 2, et aucun contrat run/niveau/checkpoint n'y est introduit. |
| **Epic 6** | **References a corriger seulement** (Story 6.3 → 5.21 / 5.19 ; Story 6.10 → 5.20). Aucune story ajoutee, aucun AC modifie. |
| Epics 0 a 4 | Aucun. Le contrat d'`ANO-5.10-03` renvoie a la Story 5.14, propriete de l'Epic 5. |
| Epic 7 | Aucun. |

### 2.2 Impact Story

**5 nouvelles stories (5.11 a 5.15)**, inserees avant les travaux de trafic restants :

| Story | Titre | Absorbe |
| --- | --- | --- |
| 5.11 | Vehicle Chassis, Wheels, and Suspension | premier prototype de bordure franchissable ; supprime `IsSurfaceContact`, `IsSurfaceOnlyCollision` et leur tolerance + la garde EditMode qui assertait sur leur texte source |
| 5.12 | Tire Forces and Steering | la dette de poursuite point-a-point (`ComputeSeekIntent` → point de visee anticipe) |
| 5.13 | Arcade Assists and Uneven Ground | le franchissement de bordure, de bosse et de marche |
| 5.14 | AI Drives by Intent | **`ANO-5.10-03`** (AC1 a AC8) + suppression du palier de teleportation de recuperation |
| 5.15 | Credible Collisions and Damage Integration | recalibrage du seuil de degats contre le nouveau profil d'impact |

**Renumerotation.** Les huit stories deplacees etaient **toutes en `backlog`**, donc aucune cle ne portait
de travail :

| avant (2026-09-15) | apres (2026-09-18) |
| --- | --- |
| 5.11 Intersection Rules and Deadlock Prevention | **5.18** |
| 5.12 Wider Perception and Progressive Unblocking | **5.17** |
| 5.13 Rage and Fear as Driving Model Modulation | **5.19** |
| 5.14 Player-Targeted Rage Ladder and Rage Road Trigger | **5.21** |
| 5.15 Thrown Litter Foundation and Attribution | **5.20** |
| 5.16 Lobby-Configurable Traffic Settings | **5.16 (inchange)** |
| 5.17 Scale Validation and Network Budget | **5.22** |
| 5.18 Epic 5 AI Traffic Playable Checkpoint | **5.23** |

5.17 et 5.18 sont en outre **inversees** dans la chaine de dependances : la perception (5.17) precede
desormais l'arbitrage d'intersection (5.18) qui la consomme — l'ordre precedent avait cette dependance a
l'envers.

### 2.3 Conflits d'artefacts

| Artefact | Nature du changement |
| --- | --- |
| `planning-artifacts/epics.md` | roadmap corrective, note de correction Epic 5, stories 5.11 a 5.23, notes de renumerotation, references Epic 6 |
| `implementation-artifacts/sprint-status.yaml` | 13 cles ajoutees ou renommees, bloc de commentaire, `5-9` et `5-10` en `review` |
| `architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md` | **AD-35** + note de supersession 2026-09-18 + AD-32 corrigee sur le numero de la story de mesure |
| `implementation-artifacts/epic-5-context.md` | titre, goal, liste des stories, contraintes physiques, dependances inter-stories |
| `implementation-artifacts/anomalies/epic 5/ANO-5.10-03/ANO-5.10-03.md` | reaffectation 5.10 → 5.14 |
| `implementation-artifacts/anomalies/epic 5/ANO-5.10.02/ANO-5.10.02.md` | reference de renumerotation |
| `implementation-artifacts/deferred-work.md` | 2 entrees : surfaces de roulage reportees, dette de poursuite resolue |
| `implementation-artifacts/spec-5-9-…`, `spec-5-10-…` | references de renumerotation (`ex-5.11` / `ex-5.12` / `ex-5.13` / `ex-5.14`) |
| `docs/setup/addon-adoption-register.md` | `ADDON-003` resolu en negatif |
| `docs/setup/devworkflow-rollout.md` | prerequis du harnais PlayMode rattache a la Story 5.14 |
| `docs/setup/story-5-9-…-notes.md`, `story-5-10-…-notes.md` | references de renumerotation et regle du trottoir requalifiee |
| Exigences fonctionnelles | **aucun changement** — la fondation physique est un moyen de satisfaire FR1, FR5 et FR6, pas une exigence nouvelle ; FR6 porteait deja la cardinalite configurable et la validation a ~30 vehicules |

### 2.4 Impact technique

- **Une seule couche physique, partagee** par la voiture joueur et les vehicules IA (AD-35). Les deux
  prefabs portent le meme composant, configure par un `VehicleProfileDef` et non par des champs serialises :
  l'identite de prefab, l'enregistrement `NetworkObject` et les ids de definition restent inchanges (NFR18).
- **Contrainte de testabilite, pas de fidelite** : modele a raycasts et non `WheelCollider`, precisement
  parce que suspension, slip, force et adherence restent des fonctions pures verifiables en EditMode — ou
  la suite est verte et filtrable — alors qu'un modele `WheelCollider` n'est prouvable que pendant un pas
  physique, dans une suite rouge et non filtrable.
- **Prerequis dur enregistre sur la Story 5.14** : la correction du harnais PlayMode (rouge 3/33, non
  filtrable) est du **tooling**, suivie dans `docs/setup/devworkflow-rollout.md`, et non du code de story.
- **La geometrie bouge, le graphe non** : la bordure franchissable est authoree **dans la bande de 4 m du
  trottoir, jamais dans les 8 m de chaussee**. Marge mesuree a la validation : 0,97 m de chaque cote
  (2 m de l'axe de voie au bord de chaussee contre 2,06 m de largeur vehicule). Si un vehicule IA touche la
  bordure en conduite nominale, la correction est le rayon d'arrivee ou le point de visee de la Story 5.12 ;
  **toucher le graphe est le dernier recours**.
- **Verification EditMode d'abord** : seuls les AC2, AC6 et AC7 d'`ANO-5.10-03` exigent PlayMode, en au plus
  deux tests.

---

## 3. Approche recommandee

**Direct Adjustment** — ajout de stories dans le plan existant, sans rollback ni revision du MVP.

### Justification

Le travail est **additif** : il insere une fondation physique en amont d'un authoring de comportement qui
n'etait pas encore commence. Aucun livrable en `review` n'est rouvert, aucune cle en `review` ou `done`
n'est renumerotee, et le contrat de prefab est preserve plutot que rompu.

### Alternatives ecartees

- **Corriger `ANO-5.10-03` dans la Story 5.10** (en `review`) : impossible. La reaction demandee exige une
  bordure franchissable et un controleur qui cesse de combattre PhysX ; rouvrir une story livree pour y
  loger un modele physique serait un contournement de perimetre.
- **Adopter un controleur vehicule tiers** (`ADDON-003`, « asset vehicle controller or arcade driving
  helper », `Not Started`) : ecarte. Le modele est ecrit dans le projet ; la ligne du registre est mise a
  jour plutot que laissee en attente.
- **Utiliser `WheelCollider`** : ecarte pour une raison de **testabilite** (AD-35), pas de fidelite.
- **Relever la bordure ou deplacer des colliders** : refuse par l'humain le 2026-09-16.
- **Masquer les symptomes** (masse augmentee, rotations gelee, vitesse IA reduite, collisions desactivees,
  teleportation apres choc, snap sur la lane la plus proche) : **explicitement interdit par la section
  Contraintes d'`ANO-5.10-03`**. Ces solutions auraient masque le probleme au lieu de construire un
  comportement de collision exploitable par le gameplay futur.

### Effort, risque, calendrier

| | |
| --- | --- |
| **Effort** | 5 stories nouvelles : 3 fondations verifiables en EditMode (5.11 a 5.13), 1 porteuse d'une anomalie a 8 AC (5.14), 1 de recalibrage (5.15) |
| **Risque** | **Moyen.** La couche physique remplace le mode d'application du mouvement sur les prefabs utilises par des stories deja livrees (Epic 3) : la non-regression PlayMode est le risque principal, et elle est aggravee par le harnais rouge |
| **Impact calendrier** | Les travaux de trafic restants (5.16 a 5.23) sont decales derriere la fondation. Aucun impact sur les Epics 0 a 4 |
| **Prerequis bloquant** | Correction du harnais PlayMode avant la Story 5.14 |

---

## 4. Propositions de changement detaillees

### 4.1 Nouvelle decision d'architecture — AD-35

`ARCHITECTURE-SPINE.md`, apres AD-34 : **« AD-35 - Raycast Wheel Model, One Physics Layer For Player And
AI [ADOPTED] »**. Contact au sol, suspension et force de pneumatique sont calcules par **raycasts par roue**
pilotes par des fonctions pures sur un `VehicleProfileDef` (AD-12, AD-25). **`WheelCollider` n'est pas
utilise.** La voiture joueur et tout vehicule IA portent **le meme** composant physique ; un conducteur IA
emet un `VehicleDriveIntent` (steer, throttle, brake, handbrake) et **n'ecrit jamais** la velocite, la
position ou la rotation du `Rigidbody`. La couche physique lit `VehicleProfileDef` uniquement et ne lit
jamais rage, peur ou disposition de conducteur : la modulation emotionnelle agit au-dessus d'elle (AD-33).
L'hote reste le seul simulateur (AD-21).

- **Relation a AD-7** : AD-7 est **inchange et non supersede**. Sa condition de reouverture n'est pas
  atteinte — il n'y a pas de modele de roue a regler — et AD-35 construit le modele arcade qu'il demandait.
  Simulation realiste de pneumatique et reglage `WheelCollider` restent reportes.
- **Relation a AD-21** : inchange. L'hote reste le seul a simuler le mouvement des vehicules.
- **Supersession enregistree** : partout ou un texte anterieur ou du code implique qu'un vehicule est pilote
  en ecrivant `Rigidbody.linearVelocity`, ou que l'IA integre sa vitesse en boucle ouverte, AD-35 gagne.

### 4.2 Nouvelles stories Epic 5 (5.11 a 5.15)

Inserees dans `epics.md`, section Epic 5, avant l'ancienne Story 5.11. Le texte complet des criteres
d'acceptation vit dans `epics.md` ; les points structurants sont :

- **5.11** — masse, centre de masse et tenseur d'inertie **explicitement authores** ; un prototype de
  bordure **sur un carrefour ou un giratoire, jamais sur une ligne droite** (les vehicules coupent
  l'interieur des virages) ; trois controles : aucun `LaneNode` sur ou dans la bordure, aucun contact en
  conduite nominale, franchissement a basse vitesse et maintien au sol a vitesse de conduite ; la hauteur
  retenue devient **valeur contractuelle pour le kit artistique**. Supprime `IsSurfaceContact`,
  `IsSurfaceOnlyCollision` et la tolerance associee, ainsi que le test EditMode qui assertait sur leur texte
  source ; reecrit `NoModuleColliderRisesAboveTheDrivingPlane` en garde sur la hauteur de bordure authoree ;
  **laisse intacte** la garde `vehicleDamage > 0` de `RunFlowController` (seuil independant et toujours
  valide, pas un contournement).
- **5.12** — le mode longitudinal passe **aux forces appliquees aux roues**, l'ecriture en bloc de
  `linearVelocity` disparait ; la perte d'adherence est **progressive**, pilotee par une valeur de slip,
  jamais par un seuil binaire ; le point de visee anticipe remplace la poursuite point-a-point, ce qui
  cloture la dette enregistree dans `deferred-work.md` le 2026-09-18 ; `VehicleDriveIntent` gagne une voie
  de frein a main, sur le chemin d'intention **existant** (NFR5).
- **5.13** — aides arcade authorees et desactivables individuellement, lisant **uniquement**
  `VehicleProfileDef` et jamais rage, peur ou disposition ; l'autorite de conduite et de direction decroit
  avec le nombre de roues au sol ; aucune aide ne remet silencieusement l'entree conducteur a zero.
- **5.14** — **absorbe `ANO-5.10-03`**, dont les AC1 a AC8 restent **autoritatifs et inchanges** et ne sont
  pas recopies ; l'integration de la vitesse en boucle ouverte et l'ecriture de `linearVelocity`
  disparaissent ; la recuperation par teleportation est **remplacee** par un retour physique sur une lane
  valide (AC6, AD-34) ; les tests EditMode de la Story 5.9 — DriverModel, IDM, MOBIL — doivent passer
  **verts sans modification** : un echec signifie que la physique a fui dans la couche de decision et doit
  etre corrige avant de continuer.
- **5.15** — mouvement resultant proportionnel a l'impact, aucun vehicule projete, pivote ou coince contre
  la geometrie ; recalibrage du seuil de degats contre le nouveau profil d'impact, **avec la mesure
  enregistree** ; mode de detection de collision decide **par mesure** (risque de tunnellisation contre cout
  CPU a ~30 vehicules), jamais suppose.

### 4.3 Renumerotation 5.11 a 5.18 → 5.16 a 5.23

Voir le tableau en 2.2. Chaque story renumerotee recoit une note
`> Renumbered from Story X on 2026-09-18. It was in `backlog`, so no work was attached to the renumbered
key.` — la meme trace que la correction du 2026-09-15 avait etablie pour la sienne.

### 4.4 Reaffectation d'`ANO-5.10-03`

`anomalies/epic 5/ANO-5.10-03/ANO-5.10-03.md` : `story: 5.14`, `original_story: 5.10`,
`reassigned: 2026-09-18`. Statut **inchange** (`open`). Motif inscrit : la correction est impossible sans
le modele a roues des Stories 5.11 a 5.13, la Reaction B (« esquive ») exigeant une bordure franchissable.
La Story 5.10 passe en `review` : elle est livree sur le fond.

### 4.5 Reconciliation des references et requalification d'une regle

- **Contrainte trottoir requalifiee** : « n'est jamais teleporte, reinsere, **ou conduit sur un trottoir** »
  devient « n'est jamais teleporte, reinsere ou retire ; une manoeuvre de degagement **deliberee** ne va pas
  sur un trottoir, une trajectoire **subie** apres une collision peut quitter la chaussee, cas porte par la
  Story 5.14 (`ANO-5.10-03` AC7) ». Sans cette requalification, l'exigence et l'anomalie se contredisaient.
- **Nouvelles contraintes physiques** : couche physique unique et partagee (AD-35) ; bordure authoree dans
  la bande trottoir de 4 m, **jamais dans les 8 m de chaussee**, donc **le graphe de voies n'est pas
  modifie** ; validation sur un carrefour ou un giratoire, jamais sur une ligne droite.
- **Report ecrit** : les surfaces de roulage autres que la route (herbe, terre, gravier) sont reportees avec
  condition de reouverture. La Story 5.12 expose un coefficient d'adherence par surface, mais aucune seconde
  surface roulable n'est authoree dans les Epics 5 et 6.
- **References de renumerotation** : `epics.md` (Epic 6, Stories 6.3 et 6.10), `ARCHITECTURE-SPINE.md`
  (AD-32), `epic-5-context.md` (dependances inter-stories), `spec-5-9-…`, `spec-5-10-…` et
  `ANO-5.10.02.md` sont alignes sur la nouvelle numerotation, chaque renvoi portant son `(ex-NN)`.

### 4.6 `sprint-status.yaml`

13 cles ajoutees ou renommees (`5-11-…` a `5-23-…`) ; `5-9-parameterized-driver-model` et
`5-10-lane-graph-greybox-district-and-routed-traffic` en `review` ; `epic-5: in-progress`. Bloc de
commentaire en tete du fichier, mentionnant AD-35, la reaffectation d'`ANO-5.10-03` et le prerequis du
harnais PlayMode.

### 4.7 Registre d'adoption

`ADDON-003` (« asset vehicle controller or arcade driving helper », `Not Started`) est resolu **en negatif** :
le modele est ecrit dans le projet, aucun controleur tiers n'est adopte. Conforme a la regle AD-13 du depot
(aucune installation sans ligne de registre) : la ligne existait, elle est fermee plutot que laissee ouverte.

### 4.8 `deferred-work.md`

Deux entrees :

1. **Report des surfaces de roulage differenciees**, avec condition de reouverture explicite : des qu'un
   livrable authore une seconde surface roulable (terrain hors chaussee du Level 1 MVP 2, bas-cote, zone de
   sandbox stop) ou qu'une story demande une perte d'adherence dependant du sol. Le point d'ancrage existe
   deja — le coefficient par surface de la Story 5.12 devient une table indexee par un type de surface porte
   par le materiau, sans retoucher le modele de pneumatique.
2. **Cloture de la dette de poursuite point-a-point** : elle n'est plus une entree ouverte mais un critere
   d'acceptation de la Story 5.12. Un point de visee anticipe **est** le modele de direction, pas un
   livrable separable : une story autonome de suivi de trajectoire n'aurait pas pu etre verifiee
   independamment du modele de pneumatique qui la porte.

---

## 5. Handoff d'implementation

**Portee : Moderate** — reorganisation de backlog et ajout de stories dans un epic existant.

| Destinataire | Responsabilite |
| --- | --- |
| **Agent Developer** | Executer 5.11 → 5.15 dans l'ordre, puis reprendre la chaine 5.16 → 5.23 |
| **Humain (Kenan)** | Correction du harnais PlayMode (tooling, `docs/setup/devworkflow-rollout.md`) **avant** la Story 5.14 |
| **PO / Architecte** | Aucune escalade requise. Consultation seulement si la mesure de la Story 5.22 contredit AD-32 |

### Sequencement et dependances

```
5.11 chassis / roues / suspension
  └─> 5.12 pneumatiques et direction        ── cloture la dette de poursuite
        └─> 5.13 aides arcade et sol irregulier
              └─> 5.14 IA par intention     ── ANO-5.10-03 AC1-AC8
                    └─> 5.15 collisions credibles et degats
                          └─> 5.16 -> 5.17 -> 5.18 -> 5.19 -> 5.20 -> 5.21 -> 5.22 -> 5.23
```

La chaine 5.11 → 5.14 est stricte : 5.12 consomme les raycasts de roue de 5.11, 5.13 se regle contre la
hauteur de bordure que 5.11 authore, et 5.14 remplace le conducteur en boucle ouverte par une intention
emise au-dessus de la direction de 5.12.

### Criteres de succes

1. La bordure franchissable est validee sur un module de carrefour ou de giratoire, **sans qu'aucun
   `LaneNode` ni le graphe de voies soit touche**.
2. `IsSurfaceContact`, `IsSurfaceOnlyCollision` et la tolerance de contact ont disparu, et la garde
   d'authoring est reecrite sur la hauteur de bordure authoree plutot que sur le plan de roulage.
3. Les tests EditMode de la Story 5.9 passent verts **sans modification** a chaque etape de 5.11 a 5.14.
4. La recuperation par teleportation a disparu et `ANO-5.10-03` AC1 a AC8 sont satisfaits.
5. Le seuil de degats recalibre et le mode de detection de collision sont **enregistres avec leur mesure**.
6. Toutes les references de stories portent la numerotation du 2026-09-18 ; aucune ne renvoie a l'ancienne.

### Risques residuels

| Risque | Gravite | Reponse |
| --- | --- | --- |
| Suite PlayMode rouge 3/33 non filtrable | **Haute** | Prerequis enregistre sur 5.14 ; verification EditMode par defaut, PlayMode reduit a 2 tests |
| L'Epic 5 rouvre indirectement le mode d'application du mouvement de l'Epic 3 (prefabs partages) | Moyenne | Non-regression PlayMode ciblee ; collider et dimensions de prefab ne bougent pas, identite et ids inchanges (NFR18) |
| La hauteur de bordure devient une contrainte du kit artistique | Moyenne | Valeur consignee comme contrat d'art dans la Story 5.11 |
| Une seconde surface roulable apparait plus tard | Faible | Report ecrit avec condition de reouverture ; point d'ancrage deja en place (coefficient par surface de 5.12) |
| La marge geometrique des giratoires reste faible (anneau 6,00 m contre R 5,09 m) | Faible | Mesure deja consignee dans `ANO-5.10.02` ; si `desiredSpeed` depasse ~9,4 m/s, ralentir en courbure ou elargir les anneaux |
