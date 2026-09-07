---
title: 'Story 0.7 : Registre d''adoption add-on, UI et asset Unity'
type: 'chore'
created: '2026-09-07'
status: 'done'
review_loop_iteration: 0
baseline_commit: '28f045f6c7518f168789347188a1eca8da12f4d9'
context:
  - '{project-root}/_bmad-output/implementation-artifacts/epic-0-context.md'
  - '{project-root}/docs/setup/addon-adoption-register.md'
  - '{project-root}/docs/setup/tooling-validation-log.md'
  - '{project-root}/docs/setup/epic-0-readiness-checklist.md'
---

<frozen-after-approval reason="human-owned intent - do not modify unless human renegotiates">

## Intention

**Probleme :** Le registre `docs/setup/addon-adoption-register.md` existe deja (scaffold ADDON-001 a ADDON-006) mais les lignes ADDON-001 (fondation menu/UI) et ADDON-002 (fondation mouvement/controller on-foot) restent `Not Started`/`Pending` avec des champs "A renseigner", alors que l'AC source (`epics.md:405-421`) exige une decision initiale documentee pour ces deux fondations avant le debut de l'Epic 1 -- meme si la decision est de rester sur les packages Unity built-in deja epingles (`com.unity.ugui` `2.6.0`, Input System `1.20.0`, Cinemachine `6.6.0`).

**Approche :** Evaluer ADDON-001 et ADDON-002 contre le stack verrouille (`Packages/manifest.json`) et la politique gratuit/libre/open-source deja actee dans le registre, enregistrer la decision initiale `Adopt` (fondations Unity built-in deja pinnees, aucun package tiers requis pour l'instant), completer tous les champs obligatoires de la section "Gate avant `Adopt`", puis synchroniser les deux lignes Story 0.7 de `epic-0-readiness-checklist.md` et `VAL-026` de `tooling-validation-log.md` avec cette realite.

## Limites & Contraintes

**Toujours :** Utiliser exclusivement les tokens de decision controles (`Pending`, `Adopt`, `Reject`, `Defer`, `Needs Spike`, `Not Applicable`) ; ne jamais passer ADDON-001 ou ADDON-002 a `Adopt` sans que tous les champs obligatoires de "Gate avant `Adopt`" soient remplis et verifiables (licence, source, version/provenance, cout, compatibilite Unity, maintenance, dependances, securite multiplayer, source/editabilite, alignement architecture, cout rollback, responsable, validateur, preuve, notes) ; citer `Packages/manifest.json`/`Packages/packages-lock.json` comme preuve verifiable pour toute decision "rester built-in" ; conserver le format exact des tableaux (colonnes, ordre) dans les trois fichiers touches ; ne jamais employer un temps accompli ("preuve fournie") sur une ligne dont le statut n'est pas `Pass`.

**Demander d'abord :** Adopter reellement un package ou asset tiers (non built-in Unity) pour l'UI/menu ou le controller on-foot -- cette spec documente uniquement la decision "rester built-in", pas l'evaluation d'un candidat tiers specifique.

**Jamais :** Modifier les lignes ADDON-003 a ADDON-006 (vehicule, lobby/network, `Prop_Barrel`, transport Steamworks) -- hors perimetre de l'AC "menu/UI + on-foot controller" ; commencer les smoke tests Story 0.8 ; inventer une preuve utilisateur qui n'existe pas.

## Matrice I/O & Cas Limites

| Scenario | Entree / Etat | Sortie / Comportement attendu | Gestion d'erreur |
|----------|---------------|-------------------------------|-------------------|
| Decision initiale nominale | ADDON-001/ADDON-002 encore `Not Started`/`Pending`, packages built-in deja pinnes dans `Packages/manifest.json` | Lignes passees a `Adopt` avec tous les champs obligatoires remplis et verifiables, decision "rester built-in" documentee | Si un champ reste non verifiable, garder `Needs Spike`/`Blocked` plutot que `Adopt` |
| Futur candidat tiers | Un package/asset tiers est envisage plus tard pour UI ou controller | Cette spec ne le decide pas ; le candidat suit une nouvelle ligne `ADDON-###` via le template existant du registre | N/A |

</frozen-after-approval>

## Code Map

- `docs/setup/addon-adoption-register.md:36` -- Ligne ADDON-001 (fondation menu/UI) a completer avec la decision `Adopt` et les champs obligatoires.
- `docs/setup/addon-adoption-register.md:37` -- Ligne ADDON-002 (fondation mouvement/controller on-foot) a completer de la meme maniere.
- `docs/setup/addon-adoption-register.md:40` -- Ligne ADDON-005 (`Prop_Barrel`, `Pass`/`Adopt`) -- patron d'une ligne completement remplie avec preuve, a reprendre pour le niveau de detail attendu.
- `docs/setup/addon-adoption-register.md:51-66` -- Sections "Gate avant `Adopt`" et "Declencheurs de rejet ou blocage" -- regles exactes a respecter pour ne pas passer une ligne a `Adopt` prematurement.
- `docs/setup/tooling-validation-log.md:55` -- Ligne `VAL-026` (`Not Started`, zone "Controles adoption add-on") a faire avancer.
- `docs/setup/epic-0-readiness-checklist.md:52` (action manuelle) et `:68` (validation agent) -- Lignes Story 0.7 a synchroniser.
- `Packages/manifest.json:7,11,15,18` -- Versions pinnees `com.unity.cinemachine` (`6.6.0`), `com.unity.inputsystem` (`1.20.0`), `com.unity.render-pipelines.universal` (`17.6.0`), `com.unity.ugui` (`2.6.0`) -- preuve factuelle pour les decisions "rester built-in".
- `_bmad-output/planning-artifacts/epics.md:405-421` -- AC source Story 0.7.
- `_bmad-output/implementation-artifacts/sprint-status.yaml:45` -- Statut Story 0.7 a faire progresser.

## Tasks & Acceptance

**Execution :**
- [x] `docs/setup/addon-adoption-register.md` -- Completer la ligne ADDON-001 : Decision `Adopt`, Source/URL = package Unity officiel `com.unity.ugui` (built-in), Version/provenance = `2.6.0` via `Packages/manifest.json`, Licence = licence package Unity, Cout = Gratuit, et tous les autres champs obligatoires renseignes factuellement (compatibilite, maintenance, dependances, securite multiplayer, source/editabilite, alignement architecture, cout rollback), Preuve = pointeur vers `Packages/manifest.json` et les validations Story 0.5 (VAL-020 a VAL-024), Notes precisant que c'est une decision initiale reevaluable si un besoin UI specifique emerge en Epic 1+.
- [x] `docs/setup/addon-adoption-register.md` -- Completer la ligne ADDON-002 de la meme maniere : Decision `Adopt`, packages Input System `1.20.0` + Cinemachine `6.6.0` deja pinnes, aucun starter controller tiers adopte, Notes precisant que le controller on-foot sera ecrit en interne au-dessus d'Input System.
- [x] `docs/setup/tooling-validation-log.md` -- Faire avancer `VAL-026` (ligne principale) d'un statut coherent avec les deux lignes registre completees, renseigner Date preuve / Chemin-resume preuve, et ajouter une entree datee dans "Notes de validation" a la fin chronologique du tableau (meme discipline que `VAL-025`).
- [x] `docs/setup/epic-0-readiness-checklist.md` -- Synchroniser les deux lignes Story 0.7 avec le statut reel de `VAL-026`, sans temps accompli ("preuve fournie") sur une ligne non-`Pass`.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- Faire avancer `0-7-unity-add-on-ui-library-and-asset-adoption-register` (`in-progress` puis `review`) en conservant le format `last_updated` `YYYY-MM-DD HH:MM`.

**Criteres d'acceptation :**
- Given le registre est inspecte apres implementation, when les lignes ADDON-001 et ADDON-002 sont lues, then leur Decision est `Adopt`, tous les champs obligatoires de "Gate avant `Adopt`" sont remplis et verifiables via `Packages/manifest.json`/preuves Story 0.5, et aucune ligne ADDON-003 a ADDON-006 n'a ete modifiee.
- Given `VAL-026` et les lignes 0.7 du readiness checklist sont inspectees, then leur statut est coherent entre les trois fichiers et n'emploie jamais "preuve fournie" sur une ligne non-`Pass`.
- Given un futur candidat tiers pour UI ou controller est envisage, when quelqu'un compare ce candidat a cette spec, then il n'y trouve aucune decision d'adoption d'un package tiers -- seulement la decision de rester built-in.

## Spec Change Log

_Vide -- aucune revue step-04 n'a encore eu lieu._

## Verification

**Commandes :**
- `$reg = Get-Content -LiteralPath 'docs/setup/addon-adoption-register.md' -Raw; $row1 = ($reg -split "`r?`n") | Where-Object { $_ -like '| ADDON-001 |*' }; if (-not $row1 -or $row1 -match 'A renseigner' -or $row1 -notmatch '`Adopt`') { throw "ADDON-001 incomplet ou non Adopt" }` -- attendu : aucune erreur.
- `$reg = Get-Content -LiteralPath 'docs/setup/addon-adoption-register.md' -Raw; $row2 = ($reg -split "`r?`n") | Where-Object { $_ -like '| ADDON-002 |*' }; if (-not $row2 -or $row2 -match 'A renseigner' -or $row2 -notmatch '`Adopt`') { throw "ADDON-002 incomplet ou non Adopt" }` -- attendu : aucune erreur.
- `$log = Get-Content -LiteralPath 'docs/setup/tooling-validation-log.md' -Raw; if ($log -match "\|\s*VAL-026\s*\|\s*`Not Started`\s*\|\s*Controles adoption add-on") { throw "VAL-026 doit avancer au-dela de Not Started" }` -- attendu : aucune erreur.
- `$chk = Get-Content -LiteralPath 'docs/setup/epic-0-readiness-checklist.md' -Raw; if ($chk -match "\|\s*0\.7[^|]*\|\s*`(Not Started|In Progress|Blocked)`\s*\|[^\n]*preuve fournie") { throw "ligne 0.7 ne doit pas dire preuve fournie hors Pass" }` -- attendu : aucune erreur.

## Suggested Review Order

**Decision d'adoption (coeur de la revue)**

- Point de depart : decision `Adopt` pour la fondation UI, packages built-in deja pinnes cites en preuve.
  [`addon-adoption-register.md:36`](../../docs/setup/addon-adoption-register.md#L36)

- Meme decision pour la fondation mouvement/controller, aucun starter tiers adopte.
  [`addon-adoption-register.md:37`](../../docs/setup/addon-adoption-register.md#L37)

**Coherence entre documents de suivi (jamais de `Pass` sans preuve)**

- Ligne principale `VAL-026`, preuve pointee vers les deux lignes registre et `Packages/manifest.json`.
  [`tooling-validation-log.md:55`](../../docs/setup/tooling-validation-log.md#L55)

- Entree changelog datee `VAL-026`, attribution Story 0.2/0.5 corrigee par la revue.
  [`tooling-validation-log.md:104`](../../docs/setup/tooling-validation-log.md#L104)

- Ligne action manuelle Story 0.7 synchronisee avec le registre complete.
  [`epic-0-readiness-checklist.md:52`](../../docs/setup/epic-0-readiness-checklist.md#L52)

- Ligne validation agent Story 0.7 synchronisee avec `VAL-026`.
  [`epic-0-readiness-checklist.md:68`](../../docs/setup/epic-0-readiness-checklist.md#L68)

**Suivi sprint**

- Statut Story 0.7 avance a `review`, en attente de cloture humaine.
  [`sprint-status.yaml:45`](sprint-status.yaml#L45)
