---
title: 'Story 0.1: Checklist de readiness setup et baseline workspace locale'
type: 'chore'
created: '2026-09-02'
status: 'done'
review_loop_iteration: 0
baseline_commit: 'NO_VCS'
context:
  - '{project-root}/_bmad-output/implementation-artifacts/epic-0-context.md'
---

<frozen-after-approval reason="intention controlee par l'humain - ne pas modifier sauf renegociation explicite">

## Intention

**Probleme :** L'Epic 0 a commence, mais le depot ne contient pas encore les artefacts de suivi qui doivent guider l'installation avant tout developpement gameplay Unity. Sans ces documents, les installations manuelles, validations agent, decisions d'adoption de librairies/assets et bloqueurs peuvent diverger.

**Approche :** Creer le guide de setup debutant et deux registres compagnons dans `docs/setup/`. Les documents doivent separer les actions manuelles utilisateur des validations agent, utiliser un vocabulaire de statut partage, et rendre explicite le gate qui bloque l'Epic 1.

## Limites & Contraintes

**Toujours :** Utiliser les statuts `Not Started`, `In Progress`, `Pass`, `Blocked` et `Not Applicable` dans chaque artefact de suivi, avec libelles francais si utile. Garder les installations GUI, comptes, approbations de services et smoke tests comme etapes manuelles ou confirmees par l'utilisateur que les agents guident et valident. Preserver le stack Epic 0 verrouille : Unity Hub, Unity `6000.6.0f1`, projet Universal 3D/URP `RoadRageSimulator`, versions de packages Unity requises, Unity Cloud/Gaming Services, Blender `5.2 LTS`, Unity MCP, Blender MCP, et revue d'adoption add-on/asset.

**Demander d'abord :** Toute modification de version Unity, versions de packages, nom du projet, modele de services, ordre de preference MCP, ou regle de gate Epic 1 necessite une approbation humaine avant implementation.

**Jamais :** Ne pas creer le projet Unity, installer des outils externes, configurer des comptes cloud, ajouter des secrets, pretendre qu'un smoke test a reussi sans preuve, ou importer des assets tiers dans cette story. Ne pas commencer l'Epic 1 tant que l'Epic 0 n'est pas marquee `Pass` ou explicitement acceptee avec bloqueurs documentes.

## Matrice I/O & Cas Limites

| Scenario | Entree / Etat | Sortie / Comportement attendu | Gestion d'erreur |
|----------|---------------|-------------------------------|------------------|
| Docs de setup frais | `docs/setup/` est absent ou vide | Creer les trois artefacts de setup avec le vocabulaire de statut partage et la regle de gate Epic 1 | Si le dossier manque, le creer |
| Docs de setup existants | Un ou plusieurs fichiers cibles existent deja | Preserver le contenu utile existant en ajoutant les exigences manquantes de la Story 0.1 | Ne pas supprimer les notes utilisateur ; fusionner ou ajouter prudemment |
| Projet Unity pas encore cree | Aucun `Assets/`, `Packages/` ou `ProjectSettings/` n'existe | Le checklist note la creation du projet Unity comme action manuelle et la validation comme en attente | Ne pas fabriquer de preuve de package ou de smoke test |

</frozen-after-approval>

## Code Map

- `_bmad-output/planning-artifacts/epics.md` -- Source des criteres d'acceptation Story 0.1, de la regle de gate Epic 0, du vocabulaire de statut attendu et de la sequence des stories Epic 0.
- `_bmad-output/implementation-artifacts/sprint-status.yaml` -- Suit `epic-0` et `0-1-setup-readiness-checklist-and-local-workspace-baseline`; a mettre a jour seulement lors d'un changement d'etat d'implementation.
- `_bmad-output/implementation-artifacts/epic-0-context.md` -- Contexte Epic 0 distille : limite setup manuel, stack verrouille, regles de securite MCP, gate d'intake asset et dependances entre stories.
- `_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md` -- Versions verrouillees, seed structurel, decisions Unity host-authoritative et regles Blender asset intake a resumer dans les docs de setup.
- `_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/beginner-architecture-guide.md` -- Ordre d'installation debutant, milestones, usage sur de l'IA/MCP et checklist asset intake.
- `_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/mcp-tooling-setup.md` -- Ordre de preference Unity MCP et Blender MCP, notes de configuration client, smoke tests et actions MCP interdites.
- `docs/setup/` -- Repertoire cible ; actuellement absent du workspace et a creer par cette story.

## Taches & Acceptation

**Execution :**
- [x] `docs/setup/epic-0-readiness-checklist.md` -- Creer une checklist tutorielle couvrant les stories Epic 0.1 a 0.8, actions manuelles, validations agent, preuves requises, notes de blocage, et gate go/no-go Epic 1 -- donne au developpeur un chemin de setup unique.
- [x] `docs/setup/tooling-validation-log.md` -- Creer un modele de journal de validation pour installation Unity, creation projet, packages, Unity services, smoke tests MCP, setup Blender, controles asset intake, smoke tests multiplayer local/distant, cap quatre joueurs, host quit et erreurs UI visibles -- evite les declarations de readiness non verifiees.
- [x] `docs/setup/addon-adoption-register.md` -- Creer un registre d'adoption add-on/librairie/asset avec la regle evaluer d'abord, importer ensuite, wrapper/adapter ensuite, customiser en dernier, et des champs pour licence, cout, compatibilite version, maintenance, dependances, impact multiplayer, source/editabilite, alignement architecture et decision -- rend la reutilisation intentionnelle.

**Criteres d'acceptation :**
- Given le depot existe localement et l'Epic 0 a commence, when les artefacts de suivi setup sont crees, then `docs/setup/epic-0-readiness-checklist.md`, `docs/setup/tooling-validation-log.md` et `docs/setup/addon-adoption-register.md` existent.
- Given l'un des trois artefacts de setup est ouvert, when ses tableaux de suivi sont inspectes, then ils incluent les options de statut `Not Started`, `In Progress`, `Pass`, `Blocked` et `Not Applicable`.
- Given la checklist de readiness est inspectee, when le developpeur la suit, then les actions manuelles utilisateur sont clairement separees des etapes de validation agent.
- Given la checklist de readiness est inspectee, when le gate Epic 1 est relu, then il indique que l'Epic 1 reste bloquee jusqu'a ce que l'Epic 0 soit marquee `Pass` ou explicitement acceptee avec bloqueurs documentes.

## Spec Change Log

- 2026-09-02 : Creation des trois artefacts `docs/setup/` et validation des taches d'implementation Story 0.1.
- 2026-09-02 : Patch review Story 0.1 : localisation francaise des artefacts Epic 0, preuves reproductibles dans le journal tooling, gate adoption renforce, suppression des prompts temporaires de review et ajout de verifications d'IDs/sprint-status/localisation.
- 2026-09-02 : Resumed by Claude (bmad-build) after Codex left the spec at `status: in-review`. Ran the parallel review layers (blind-hunter, edge-case-hunter, verification-gap). Patched two self-consistency gaps: the Story 0.1 manual-confirmation row in `epic-0-readiness-checklist.md` was marked `Pass` without real user evidence (now `In Progress`), and the `Adopt` gate in `addon-adoption-register.md` didn't require row `Statut` = `Pass` and was missing `cout`/`notes de decision` from its required-fields list (both now added). All spec verification commands re-run and pass. Seven lower-value findings (VCS/git baseline guidance, ID cross-referencing, VAL-011 ambiguous pass criterion, no re-evaluation trigger for stale `Adopt` decisions, unenumerated `Assets/RoadRage` feature names, no secrets-storage-location guidance, no `docs/setup/` index) recorded in `deferred-work.md` rather than actioned, since none are required by this story's acceptance criteria.

## Notes De Design

Utiliser des tableaux Markdown concis avec des champs repetables plutot qu'un guide uniquement narratif. La checklist doit rester tutorielle pour un debutant, mais chaque ligne doit aussi pouvoir servir de suivi pendant l'installation.

## Verification

**Commandes :**
- `Test-Path docs/setup/epic-0-readiness-checklist.md; Test-Path docs/setup/tooling-validation-log.md; Test-Path docs/setup/addon-adoption-register.md` -- attendu : les trois commandes retournent `True`.
- `rg "Not Started|In Progress|Pass|Blocked|Not Applicable" docs/setup` -- attendu : chaque artefact cible contient le vocabulaire complet de statut partage.
- `rg "Epic 1" docs/setup/epic-0-readiness-checklist.md` -- attendu : la regle de gate est presente et explicite.
- `if (Test-Path '_bmad-output/implementation-artifacts/review-prompt-0-1-*.md') { throw 'prompts temporaires de review encore presents' }` -- attendu : aucune erreur ; les prompts temporaires sont absents.
- `$files = @('docs/setup/epic-0-readiness-checklist.md','docs/setup/tooling-validation-log.md','docs/setup/addon-adoption-register.md'); $statuses = @('Not Started','In Progress','Pass','Blocked','Not Applicable'); foreach ($file in $files) { $text = Get-Content -LiteralPath $file -Raw; foreach ($status in $statuses) { if ($text -notmatch [regex]::Escape($status)) { throw "$file missing $status" } } }` -- attendu : aucune erreur ; chaque artefact contient tous les statuts contractuels.
- `$changed = @('_bmad-output/implementation-artifacts/epic-0-context.md','docs/setup/epic-0-readiness-checklist.md','docs/setup/tooling-validation-log.md','docs/setup/addon-adoption-register.md'); $forbidden = @('Setup Readiness Checklist and Local Workspace Baseline','Unity Editor, Project Creation, and Package Pinning','Unity Cloud, Services, Lobby, and Relay Readiness','Project Structure, Scenes, Namespaces, and Runtime State Skeleton','Blender and 3D Asset Intake Pipeline','Unity Add-On, UI Library, and Asset Adoption Register','Epic 0 Readiness Checklist','Tooling Validation Log','Add-on Adoption Register'); foreach ($file in $changed) { $text = Get-Content -LiteralPath $file -Raw; foreach ($phrase in $forbidden) { if ($text.Contains($phrase)) { throw "$file contains untranslated phrase: $phrase" } } }` -- attendu : aucune erreur ; les anciens libelles anglais non contractuels ne restent pas dans les artefacts modifies.
- `$context = Get-Content -LiteralPath '_bmad-output/implementation-artifacts/epic-0-context.md' -Raw; if (-not $context.StartsWith('# Epic 0 Context:')) { throw 'prefixe heading BMAD perdu' }; if ($context -notmatch 'prefixe `# Epic 0 Context:` est conserve volontairement') { throw 'note BMAD manquante' }` -- attendu : aucune erreur ; le prefixe BMAD reste present et explique.
- `$yaml = Get-Content -LiteralPath '_bmad-output/implementation-artifacts/sprint-status.yaml' -Raw; if (([regex]::Matches($yaml, '(?m)^  epic-0: in-progress$')).Count -ne 1) { throw 'epic-0 status invalide' }; if (([regex]::Matches($yaml, '(?m)^  0-1-setup-readiness-checklist-and-local-workspace-baseline: in-progress$')).Count -ne 1) { throw 'story 0.1 sprint-status invalide' }; if ($yaml -match '(?m)^  0-1-setup-readiness-checklist-and-local-workspace-baseline: (review|done)$') { throw 'sprint-status avance trop tot' }` -- attendu : aucune erreur ; le sprint-status reste parseable par assertions de statut et la story reste `in-progress`.
- `$log = Get-Content -LiteralPath 'docs/setup/tooling-validation-log.md' -Raw; $ids = [regex]::Matches($log, '(?m)^\|\s*VAL-(\d{3})\s*\|') | ForEach-Object { [int]$_.Groups[1].Value }; if (($ids | Select-Object -Unique).Count -ne $ids.Count) { throw 'IDs VAL dupliques' }; for ($i = 1; $i -le $ids.Count; $i++) { if ($ids[$i-1] -ne $i) { throw "sequence VAL invalide a $i" } }` -- attendu : aucune erreur ; les IDs `VAL-###` sont uniques et sequentiels.
- `$register = Get-Content -LiteralPath 'docs/setup/addon-adoption-register.md' -Raw; $ids = [regex]::Matches($register, '(?m)^\|\s*ADDON-(\d{3})\s*\|') | ForEach-Object { [int]$_.Groups[1].Value }; if (($ids | Select-Object -Unique).Count -ne $ids.Count) { throw 'IDs ADDON dupliques' }; for ($i = 1; $i -le $ids.Count; $i++) { if ($ids[$i-1] -ne $i) { throw "sequence ADDON invalide a $i" } }` -- attendu : aucune erreur ; les IDs `ADDON-###` numeriques sont uniques et sequentiels.

## Suggested Review Order

**Readiness checklist (entry point)**

- Single setup path for Epic 0: shared status vocabulary, then per-story manual/agent rows and the Epic 1 gate.
  [`epic-0-readiness-checklist.md:1`](../../docs/setup/epic-0-readiness-checklist.md#L1)

- Story 0.1's own manual-confirmation row now reads `In Progress`, not a self-certified `Pass`, since the user hasn't confirmed yet.
  [`epic-0-readiness-checklist.md:45`](../../docs/setup/epic-0-readiness-checklist.md#L45)

- Epic 1 stays blocked until Epic 0 is `Pass` or explicitly accepted with documented blockers.
  [`epic-0-readiness-checklist.md:73`](../../docs/setup/epic-0-readiness-checklist.md#L73)

**Tooling validation log**

- VAL-001 through VAL-033 give every Epic 0 story a reproducible evidence row; sequential numeric IDs are enforced.
  [`tooling-validation-log.md:27`](../../docs/setup/tooling-validation-log.md#L27)

**Add-on adoption register**

- `Adopt` gate now also requires row `Statut` = `Pass` and includes `cout`/`notes de decision`, closing a loophole where a candidate could be adopted without those fields.
  [`addon-adoption-register.md:50`](../../docs/setup/addon-adoption-register.md#L50)

- Initial UI, controller, vehicle, lobby/network, and 3D-asset candidates are seeded `Pending` — evaluated first, nothing imported yet.
  [`addon-adoption-register.md:34`](../../docs/setup/addon-adoption-register.md#L34)

**Epic context localization (peripheral)**

- `epic-0-context.md` translated to French for consistency with the new setup docs; the BMAD-required `# Epic 0 Context:` heading prefix is kept and explained rather than translated.
  [`epic-0-context.md:3`](epic-0-context.md#L3)
