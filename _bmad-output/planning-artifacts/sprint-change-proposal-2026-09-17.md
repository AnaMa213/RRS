# Sprint Change Proposal — 2026-09-17

**Sujet :** Durcissement du workflow de developpement assiste par IA — perimetre de recon, garde-fous Editeur, chaine de verification deterministe et criteres d'adoption d'outils.

**Statut :** **rejete comme mecanisme de suivi — decision de Kenan, 2026-09-17.** Le travail restant est technique et pilote directement, sans passer par un Epic ou des Stories BMAD. Ce document reste comme trace d'analyse (declencheur, preuves, impact) ; sa section 4.3 (`sprint-status.yaml`) et l'ajout d'`Epic 8` dans `epics.md` ne sont **pas appliques**. Le suivi des taches se fait desormais dans `docs/setup/devworkflow-rollout.md`.
**Classification de portee :** **Moderate** — ajout d'un epic d'outillage et reorganisation de backlog. Aucun replan fondamental, aucun changement de perimetre gameplay MVP.
**Declencheur :** audit d'architecture du workflow IA mene le 2026-09-17, suivi d'un contre-audit qui a invalide trois conclusions du premier passage.

**Impact sur l'Epic 5 :** **aucun.** Les stories 5.1 a 5.9 restent en `review`, 5.10 en `in-progress`, 5.11 a 5.18 en `backlog`. Aucune renumerotation.

---

## 1. Resume du probleme

### Declencheur

Une architecture d'outillage etait envisagee, ajoutant simultanement Serena, Roslynator, jscpd, RTK, Lefthook et GitHub MCP au workflow. L'audit devait determiner si elle etait pertinente pour l'etat reel du repository.

### Nature du probleme

**Constat mesure contredisant l'hypothese de depart.** Le probleme principal n'etait pas l'absence d'outils, mais la **configuration des outils deja presents** — et deux trous de verification qu'aucun des six outils candidats n'aurait combles.

| Ce qui etait suppose | Ce que la mesure a montre |
| --- | --- |
| Il manque des outils d'analyse | Le corpus Graphify etait a **61 % d'outillage** pour 15,6 % de code projet |
| Le graphe est exploitable | **12 011 noeuds** contre une limite utile de 5 000 — vue degradee, 896 communautes parasites |
| La duplication justifie jscpd | **14,7 %** en parametres jscpd reels, mais le signal utile est obtenable en une passe ponctuelle |
| Lefthook est incompatible avec Git LFS | **Faux** — il gere LFS et migre les hooks ; le vrai motif est le ROI |
| Le batchmode Unity bloque l'automatisation | **Faux** — le Unity CLI pilote l'Editeur **connecte** |
| Un filet de tests solide couvre les regressions | 533 tests reellement utilises, mais **couverture des chemins critiques non mesuree** |

### Preuves

1. **Perimetre Graphify** — 997 fichiers indexes dont 391 `.agents/skills` + 217 `.claude/skills` (copies octet-pour-octet) et 112 `_bmad-output`, pour **156 fichiers de code projet**. Log de rebuild : `Graph has 12011 nodes (above 5000 limit). Building aggregated community view...`
2. **Mode d'echec des symboles fantomes** — la requete d'exemple `DriverBehaviorDef` renvoyait 10 526 octets dont **zero ligne de code projet**. Verification : le symbole **n'existe pas** dans le codebase. Apres correction du scope, la meme requete renvoie 34 lignes de code projet, mais **des noeuds sans rapport** : le rescope n'a pas corrige ce mode d'echec, il l'a rendu **moins reconnaissable**.
3. **Unity CLI sur Editeur connecte** — `unity status` renvoie port 7800, etat `ready`, PID 35516. 150 commandes exposees, dont `recompile`, `console`, `run_tests`, `test_status`, `audit`, `list_open_scenes`. Verifie en direct en lecture seule.
4. **Console saturee** — 0 erreur, **500+ avertissements**, integralite de l'echantillon issue de meshes Synty en version 8. Tout gate naif sur les warnings serait inexploitable.
5. **Cluster de duplication reseau** — 5 des 8 premiers clones de production sont **la meme garde d'autorite** repetee dans `NetworkedPlayerLifecycleIntent`, `NetworkedPlayerReviveIntent`, `NetworkedVehicleRecoveryIntent`, `NetworkedVehicleSeatIntent`, `NetworkedPlayerPresentation`.
6. **Hotspot de churn** — `RunFlowController.cs` : 1 522 lignes, 82 methodes, 0 `#region`, **23 commits sur 108 (21 % du churn du repo)**.
7. **Incident de perte d'etat deja survenu** — `VAL-027` documente une scene `isDirty=true` ayant perdu un objet committe ; seule une restauration manuelle `git checkout` a evite la perte.
8. **Gap de gouvernance deja identifie par le projet** — `deferred-work.md` note qu'aucune entree du registre d'adoption ne couvre l'outillage MCP, alors que le projet trace exactement cela (licence, cout, maintenance, risque de dependance) pour tout autre composant tiers.

---

## 2. Analyse d'impact

### Impact Epic

| Epic | Impact |
| --- | --- |
| Epic 0 (Tools & Addon Readiness) | **Precedent invoque.** Cet epic couvrait deja l'outillage ; il est `done` et n'est pas rouvert. |
| Epic 5 (trafic IA) | **Aucun.** Aucune story touchee, aucune renumerotation. |
| Epics 6 et 7 | **Aucun.** |
| **Epic 8 (nouveau)** | Porte le travail d'outillage restant. |

### Impact Story

Aucune story existante modifiee. **7 nouvelles stories** sous un Epic 8.

### Conflits d'artefacts

| Artefact | Nature du changement |
| --- | --- |
| `epics.md` | Ajout d'une section `## Epic 8` et de sa liste dans `## Epic List` |
| `sprint-status.yaml` | Ajout de `epic-8` + 7 cles de stories + `epic-8-retrospective` |
| `ARCHITECTURE-SPINE.md` (jeu) | **Aucun** — read-only, scope MVP, hors perimetre |
| `architecture-RRS-devworkflow-2026-09-17/` | **Deja cree** — 14 AD, memlog a 54 entrees, lint a 0 finding |
| `addon-adoption-register.md` | **Deja mis a jour** — ADDON-011 a ADDON-025 |
| `AGENTS.md` | **Deja mis a jour** — sections hors bloc `bmad:context` |
| `.graphifyignore` | **Deja cree** |

### Impact technique

**Contrainte structurante decouverte :** `sprint_plan.py` valide les cles de `development_status` par `^epic-(\d+)$` et `^(\d+)-(\d+)[a-z]?-.+`, et **regenere le bloc depuis `epics.md`**. Une cle libre de type `devworkflow-p1-validate` serait signalee `unrecognized` puis ecrasee a la prochaine planification. **Le suivi exige donc un epic numerote reel dans `epics.md`** — d'ou l'Epic 8 plutot qu'un bloc de suivi ad hoc.

---

## 3. Approche recommandee

**Direct Adjustment** — ajout de stories dans le plan existant, sans rollback ni revision du MVP.

Rationale : le travail est additif et strictement outillage. Il ne touche aucun comportement de jeu, aucun AC produit, aucune capability du SPEC. Le precedent Epic 0 etablit que l'outillage est un epic legitime dans ce projet.

| | |
| --- | --- |
| **Effort** | 7 stories, dont 1 deja terminee et 2 d'audit en lecture seule |
| **Risque** | Faible. Les deux pilotes sont hors du chemin critique et reversibles ; les audits ne modifient rien. |
| **Impact planning** | Nul sur l'Epic 5. Epic 8 est parallelisable. |

### Alternatives ecartees

- **Rouvrir l'Epic 0** — il est `done` et cloture par un gate go/no-go ; le rouvrir invaliderait sa cloture.
- **Bloc de suivi ad hoc dans `sprint-status.yaml`** — techniquement invalide (voir Impact technique).
- **Ne rien tracer dans BMAD** — laisserait 14 decisions d'architecture et 15 decisions d'outil sans support durable, alors que `deferred-work.md` signale deja ce manque.

---

## 4. Propositions de changement detaillees

### 4.1 `epics.md` — ajout a `## Epic List`

```
OLD:
(fin de la liste, apres Epic 7)

NEW:
- **Epic 8: Dev Workflow Tooling Hardening** — Perimetre de recon, garde-fous
  Editeur, chaine de verification deterministe et pilotes d'analyse statique.
  Outillage uniquement : aucun comportement de jeu, aucune capability SPEC.
  Gouverne par architecture-RRS-devworkflow-2026-09-17 (AD-1 a AD-14).
```

### 4.2 `epics.md` — nouvelle section `## Epic 8`

| Story | Titre | Statut propose |
| --- | --- | --- |
| **8.1** | Perimetre Graphify et garde-fous operationnels | `review` — **deja execute** |
| **8.2** | Script de validation sur Editeur connecte | `backlog` |
| **8.3** | Audit de cohesion de `RunFlowController` | `backlog` |
| **8.4** | Audit du cluster de garde d'autorite reseau | `backlog` |
| **8.5** | Pilote Project Auditor | `backlog` |
| **8.6** | Pilote Microsoft.Unity.Analyzers | `backlog` |
| **8.7** | Checkpoint Epic 8 — decisions KEEP ou rollback consignees | `backlog` |

**Story 8.1 — Perimetre Graphify et garde-fous operationnels** *(execute le 2026-09-17)*
AC : `.graphifyignore` restreint le graphe au code applicatif · le graphe repasse sous la limite utile · 100 % des fichiers `.cs` de `Assets/RoadRage` restent indexes · la regle de precheck de symbole nomme est ecrite dans `AGENTS.md` hors du bloc `bmad:context` · le garde-fou `git status` + `list_open_scenes` est ecrit · Obsidian est retire du seul client ou il est inutile.
Preuve : 997 -> 257 fichiers, 12 011 -> 4 036 noeuds, 896 -> 209 communautes, 156/156 fichiers `.cs` conserves, requete sur symbole reel a 40 lignes utiles et 0 bruit.

**Story 8.2 — Script de validation sur Editeur connecte**
AC : `scripts/validate.ps1` enchaine `unity status` -> `recompile` -> `recompile_status` -> `console --level error` -> tests cibles -> `list_open_scenes` + `git status` · **echoue ferme** a chaque etape (AD-8), un test en echec etant un echec · gate sur `--level error`, warnings filtres de `Assets/Synty/` sans desactivation globale (AD-7) · affiche la version du CLI detectee et refuse une version non validee · routing EditMode/PlayMode selectionnable (AD-9).
**Prerequis bloquant :** exercer reellement `run_tests` / `test_status` sur un petit sous-ensemble EditMode avant de figer le mecanisme. Ces commandes sont verifiees **en existence** seulement ; `unity test` et `unity cmd run_tests` ne sont pas supposes equivalents avec l'Editeur ouvert.

**Story 8.3 — Audit de cohesion de `RunFlowController`**
AC : responsabilites reelles, dependances entrantes et sortantes, groupes de methodes modifies ensemble, causes historiques du churn, tests associes, responsabilites extractibles, risque d'un refactor. Comparaison avec `LobbyFlowController` et `NetworkedAIVehicleDriverController`.
**Ne refactorer que si l'analyse montre un gain net.** La taille seule ne justifie rien.

**Story 8.4 — Audit du cluster de garde d'autorite reseau**
AC : pour les 5 fichiers `Networked*` concernes — meme invariant ? meme responsabilite ? meme ordre de validation ? memes conditions d'ownership ? memes consequences ? memes contraintes host/client ? tests existants ? risque qu'une factorisation cree un couplage artificiel ?
**Extraction proposee seulement si la duplication est semantique et non fortuite.** Sinon, duplication conservee volontairement et documentee comme telle.

**Story 8.5 — Pilote Project Auditor**
AC : baseline etablie via `unity cmd audit` · duree, nombre de diagnostics, part actionnable, faux positifs et problemes reellement nouveaux mesures · decision consignee dans ADDON-018 · expose via un commutateur `-Audit`, **hors** du chemin critique de `validate.ps1` v1.

**Story 8.6 — Pilote Microsoft.Unity.Analyzers**
AC : execute **apres** 8.5, qui sert de baseline · analyzers charges cote Unity sans jamais editer les `.csproj` generes (AD-14) · doublons avec l'IDE et les analyzers Unity verifies · `.editorconfig` minimal, 4 a 6 regles `UNT` en `warning`, le reste en `silent` · mesure sur quelques stories · decision KEEP ou rollback consignee dans ADDON-019.
Critere de rollback : plus de 10 warnings non actionnables au premier passage sur les 104 fichiers de production, ou plus d'un faux positif par story.

**Story 8.7 — Checkpoint Epic 8**
AC : ADDON-018 et ADDON-019 portent une decision finale · `validate.ps1` a ete utilise sur au moins 3 stories reelles · les audits 8.3 et 8.4 ont rendu une conclusion explicite (refactorer ou conserver) · le spine d'outillage est a jour si une decision a change.

### 4.3 `sprint-status.yaml` — ajout sous `development_status`

```yaml
  epic-8: in-progress
  8-1-graphify-scope-and-operational-guardrails: review
  8-2-connected-editor-validation-script: backlog
  8-3-runflowcontroller-cohesion-audit: backlog
  8-4-network-authority-guard-cluster-audit: backlog
  8-5-project-auditor-pilot: backlog
  8-6-unity-analyzers-pilot: backlog
  8-7-epic-8-tooling-checkpoint: backlog
  epic-8-retrospective: optional
```

Conforme a `EPIC_KEY_RE`, `STORY_KEY_RE` et `RETRO_KEY_RE`. Story 8.1 en `review` et non `done` : conformement a la convention du projet, seule la story de checkpoint d'un epic promeut l'ensemble en `done`.

### 4.4 Artefacts deja produits — pour information

| Artefact | Etat |
| --- | --- |
| `architecture-RRS-devworkflow-2026-09-17/ARCHITECTURE-SPINE.md` | 14 AD, `status: final`, lint a 0 finding |
| `architecture-RRS-devworkflow-2026-09-17/.memlog.md` | 54 entrees |
| `addon-adoption-register.md` | ADDON-011 a 025, 21 colonnes conformes |
| `AGENTS.md` | 3 sections hors bloc `bmad:context` |
| `.graphifyignore` | Cree, applique, mesure |

---

## 5. Handoff d'implementation

**Portee : Moderate** — reorganisation de backlog, coordination PO/DEV.

| Destinataire | Responsabilite |
| --- | --- |
| Kenan (PO) | Approuver l'ajout de l'Epic 8 et la formulation des AC |
| Agent (DEV) | Executer 8.2 a 8.6 via `bmad-build`, en respectant AD-4 : l'agent ne lance aucune verification, il la demande |
| Agent (DEV) | Consigner chaque resultat de pilote dans ADDON-018 et ADDON-019 |

### Criteres de succes

1. `validate.ps1` utilise sur au moins 3 stories reelles sans faux succes.
2. Les deux audits (8.3, 8.4) rendent une conclusion explicite, **y compris « ne rien changer »**.
3. Chaque pilote se termine par une decision ecrite dans le registre, pas par un abandon silencieux.
4. Aucune story de l'Epic 5 n'est retardee par l'Epic 8.

### Points non verifies restants

- Signal reel de Project Auditor sur RRS — `audit_status` repond `idle`, jamais execute.
- Benefice reel des diagnostics `UNT####` sur ce code.
- Comportement de Lefthook vis-a-vis des hooks LFS et Graphify — documente, non teste.
- Voie `run_tests` / `test_status` sur Editeur connecte — existence verifiee, **execution non exercee**.
- Couverture effective des chemins critiques par les 533 tests.
- Part reelle des tokens issue du shell — aucune telemetrie.
