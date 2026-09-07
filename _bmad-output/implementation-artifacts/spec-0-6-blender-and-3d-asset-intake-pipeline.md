---
title: 'Story 0.6 : Pipeline d''intake Blender et assets 3D'
type: 'chore'
created: '2026-09-07'
status: 'done'
review_loop_iteration: 2
baseline_commit: '9696c15b391ebf577d1607582aeac1ad4fe0b12b'
context:
  - '{project-root}/_bmad-output/implementation-artifacts/epic-0-context.md'
  - '{project-root}/docs/setup/tooling-validation-log.md'
  - '{project-root}/docs/setup/epic-0-readiness-checklist.md'
  - '{project-root}/_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md'
  - '{project-root}/_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/mcp-tooling-setup.md'
---

<frozen-after-approval reason="human-owned intent - do not modify unless human renegotiates">

## Intention

**Probleme :** Story 0.5 a configure Blender MCP et produit un mesh brouillon (`Blender/exports_test/scene_test.glb`) explicitement marque "pas de conversion prefab Unity avant Story 0.6". Aucun document ne definit encore la checklist d'intake Blender vers Unity (transforms, echelle, normals, materiaux, collision, export, import controle, conversion prefab) ni la politique de stabilite prefab quand l'art est remplace.

**Approche :** Creer un tutoriel Story 0.6 (meme forme que Story 0.3/0.5) qui distille la checklist 9 etapes deja actee dans `mcp-tooling-setup.md` et les regles AD-13/AD-27 de l'architecture spine : nettoyage Blender, export FBX ou GLB, import controle sous `Assets/RoadRage/ArtExports`, test d'echelle Unity, conversion prefab sous `Assets/RoadRage/Prefabs` seulement apres validation, et regle de stabilite (identite prefab, registration `NetworkObject`, composants gameplay, colliders et definition ids inchanges quand l'art est remplace). Reprendre le mesh brouillon existant (`scene_test.glb`) comme exemple travaille du tutoriel. Mettre a jour `VAL-025` et les deux lignes Story 0.6 de `epic-0-readiness-checklist.md` sans jamais passer `Pass` sans preuve utilisateur reelle.

## Limites & Contraintes

**Toujours :** Garder le nettoyage Blender et l'import Unity comme actions manuelles utilisateur ; documenter le dossier source (`Assets/RoadRage/ArtSource/Blender`), le dossier export (`Assets/RoadRage/ArtExports`) et le dossier prefab (`Assets/RoadRage/Prefabs`) exacts ; documenter explicitement que l'identite prefab, la registration `NetworkObject`, les composants gameplay, les colliders et les definition ids restent stables quand l'art est remplace (AD-27) ; traiter tout mesh Blender comme brouillon tant que la checklist n'est pas suivie de bout en bout.

**Demander d'abord :** Convertir reellement `scene_test.glb` (ou tout autre mesh) en prefab Unity commite dans le projet ; definir un standard de nommage ou une convention de collider au-dela de ce que AD-13/AD-27 et `mcp-tooling-setup.md` couvrent deja.

**Jamais :** Ne pas importer d'asset 3D genere ou telecharge dans Unity comme prefab gameplay sans etre passe par la checklist Blender ; ne pas commencer le Registre d'adoption add-on (Story 0.7) ni les smoke tests finaux (Story 0.8) ici ; ne pas marquer `VAL-025` ou les lignes 0.6 du readiness checklist `Pass` sans preuve utilisateur reproductible.

## Matrice I/O & Cas Limites

| Scenario | Entree / Etat | Sortie / Comportement attendu | Gestion d'erreur |
|----------|---------------|-------------------------------|-------------------|
| Asset genere IA nominal | Mesh brouillon existe (`scene_test.glb`), utilisateur suit la checklist | Tutoriel documente chaque etape (rename, transforms, echelle, materiaux, normals, geometrie, export, import controle, test echelle, prefab) avec preuve `VAL-025` attendue | Preuve caviardee `[REDACTED_TOKEN]` si un identifiant sensible apparait, jamais de secret dans un fichier Blender |
| Asset sans passage par Blender | Un asset 3D tente d'entrer dans Unity sans etape de nettoyage Blender | Le tutoriel et le readiness checklist bloquent explicitement l'usage gameplay tant que la checklist n'est pas suivie | Note bloqueur documentee, ligne reste `Not Started` ou `Blocked` |

</frozen-after-approval>

## Code Map

- `docs/setup/story-0-5-mcp-tooling-configuration-tutorial.md` -- Patron de structure a reutiliser : Sources consultees, Avant de commencer (regle de caviardage), etapes numerotees avec "Preuve a fournir (VAL-###)" et "Cas limite", arret obligatoire final. Etape 8 (lignes 174-185) documente deja le mesh brouillon `scene_test.glb` a reprendre ici comme exemple travaille.
- `_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/mcp-tooling-setup.md:228-247` -- Checklist source en 9 etapes (renommer -> transforms -> echelle -> materiaux -> normals -> geometrie -> export -> import controle -> prefab) a distiller dans le tutoriel.
- `_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/ARCHITECTURE-SPINE.md:152-156` (AD-13 Blender Intake Gate), `:236-240` (AD-27 Greybox-To-Art Prefab Stability), `:311-317` (arborescence `ArtSource/`, `ArtExports/`, `Materials/`, `Prefabs/`, `ScriptableObjects/`) -- Regles de gate intake et de stabilite prefab a citer fidelement.
- `docs/setup/tooling-validation-log.md:54` -- `VAL-025` deja reserve (`Not Started`, zone "Controles asset intake") ; a faire pointer vers le nouveau tutoriel avec preuve attendue precisee.
- `docs/setup/epic-0-readiness-checklist.md:51` (action manuelle) et `:67` (validation agent) -- Lignes Story 0.6, toutes deux `Not Started`, a synchroniser avec le tutoriel.
- `Blender/exports_test/scene_test.glb` -- Mesh brouillon Story 0.5 (objet `Cube` + `Cube_Material`, plus `Prop_Sphere` residuel sans materiau) a utiliser comme exemple travaille du tutoriel.
- `Assets/RoadRage/ArtSource/Blender/`, `Assets/RoadRage/ArtExports/`, `Assets/RoadRage/Prefabs/` -- Dossiers scaffold Story 0.4 (vides aujourd'hui) ou le tutoriel doit pointer les fichiers source/export/prefab.
- `_bmad-output/implementation-artifacts/sprint-status.yaml` -- Statut Story 0.6 a faire progresser uniquement quand le travail agent-executable correspondant est pret.
- `_bmad-output/planning-artifacts/epics.md:397-403` -- AC source de Story 0.6 : le tutoriel doit documenter explicitement "collider plan" en plus de source, export, echelle, prefab destination, replacement policy -- verifier que ce point precis est couvert, pas seulement AD-27 (preservation lors d'un remplacement).

## Tasks & Acceptance

**Execution :**
- [x] `docs/setup/story-0-6-blender-asset-intake-tutorial.md` -- Creer le tutoriel lineaire (meme forme que 0.3/0.5) couvrant la checklist 9 etapes, les dossiers source/export/prefab exacts, la regle de stabilite prefab AD-27, la regle AD-13 (Blender Intake Gate, section dediee courte comme AD-27), un "Plan collider" explicite (voir ci-dessous), le pivot/origine dans le nettoyage Blender, un `Cas limite` pour chacune des 5 etapes (y compris export et test d'echelle), l'exemple travaille `scene_test.glb` (avec note honnete que son fichier `.blend` source n'a jamais ete sauvegarde -- session Blender jamais sauvegardee depuis -- et que la reference "dossier scratch/test" ne vaut que pour l'export, pas pour un source `.blend` inexistant), et le rappel de caviardage -- donne au solo-dev un guide unique pour tout futur asset 3D.
- [x] `docs/setup/story-0-6-blender-asset-intake-tutorial.md` -- Ajouter un "Plan collider" : pour un asset nouveau (pas de remplacement), definir des colliders simples separes de la geometrie de rendu, dimensionnes a l'objet, sans mesh collider gameplay ; documenter ce choix dans les notes du prefab ; pour un remplacement d'art existant, appliquer uniquement AD-27 (deja couvert). Ne pas inventer de standard de nommage/forme fixe au-dela de ce principe general (reste hors perimetre "Ask First" de ce spec). Ajouter aussi une note dans le nettoyage Blender (etape geometrie/normals) : verifier qu'aucune geometrie retiree ne sert de collider gameplay avant suppression -- couvre l'AC epics.md "collider plan" et evite qu'un collider fonctionnel soit supprime par erreur comme geometrie decorative.
- [x] `docs/setup/tooling-validation-log.md` -- Faire pointer `VAL-025` vers le tutoriel avec preuve attendue precisee, sans passer `Pass` tant qu'aucune preuve utilisateur n'est fournie ; garder le format `last_updated`/notes datees coherent avec les entrees existantes. La ligne principale `VAL-025` doit renseigner "Date preuve" (date de creation du tutoriel) et "Chemin/resume preuve" avec un pointeur court vers le tutoriel -- ne pas laisser "A renseigner" quand une preuve partielle datee existe deja, comme le font les lignes `In Progress` voisines (VAL-013 a VAL-015, VAL-032). La nouvelle entree datee `VAL-025` dans le tableau "Notes de validation" doit etre ajoutee a la fin chronologique du tableau, pas inseree entre les deux entrees `VAL-020, VAL-021` existantes (elle casserait leur recit cause/resolution continu) -- garde le log honnete et lisible.
- [x] `docs/setup/epic-0-readiness-checklist.md` -- Synchroniser les lignes Story 0.6 (action manuelle ligne 51, validation agent ligne 67) avec le tutoriel et son statut reel. La cellule "Preuve requise"/evidence de la ligne 67 ne doit jamais utiliser un temps accompli ("preuve fournie") tant que le statut de la ligne n'est pas `Pass` -- utiliser un temps a venir ("preuve a fournir") pour rester coherent avec les lignes `Not Started` existantes du meme tableau -- garde la porte Epic 0 lisible et honnete.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- Faire avancer Story 0.6 (`in-progress` puis `review`) uniquement quand le tutoriel et la synchro sont prets ; conserver le format horodate existant de `last_updated` (`YYYY-MM-DD HH:MM`, pas seulement la date) -- garde le suivi sprint synchronise et coherent avec les entrees precedentes.
- [x] `docs/setup/story-0-6-blender-asset-intake-tutorial.md` -- Corriger les citations de lignes `mcp-tooling-setup.md` : la checklist 9 etapes ("Asset Generation and Intake") occupe les lignes 218-238 ; la section suivante "Setup Order" (239-247) est un contenu different (ordre d'installation, pas nettoyage d'asset) et ne doit pas etre inclue dans la citation de la checklist.
- [x] `docs/setup/story-0-6-blender-asset-intake-tutorial.md` -- Etape 4 point 2 : clarifier que le cas "geometrie prevue comme base d'un futur collider" ne s'applique qu'a un remplacement d'art existant (regle AD-27, ou le mesh source sert deja de reference a un collider deja en place) -- jamais a un asset entierement nouveau, ou le "Plan collider" interdit deja de deriver un collider gameplay de la geometrie de rendu. Reformuler pour lever la contradiction apparente entre les deux sections.
- [x] `docs/setup/story-0-6-blender-asset-intake-tutorial.md` -- Etape 1 : la "Preuve a fournir" doit aussi demander la confirmation que le fichier `.blend` existe reellement au chemin documente (`Assets/RoadRage/ArtSource/Blender/` ou equivalent), pas seulement le nommage/pivot/transforms -- et le "Cas limite -- fichier .blend jamais sauvegarde" doit dire explicitement de ne pas poursuivre aux etapes suivantes tant que ce fichier n'existe pas.
- [x] `docs/setup/story-0-6-blender-asset-intake-tutorial.md` -- Etape 4 point 3 (export) : ajouter un rappel d'exporter uniquement les objets prevus (par exemple `Selected Objects Only`) pour eviter qu'un objet residuel de scene (comme `Prop_Sphere`) se retrouve dans l'export.
- [x] `docs/setup/story-0-6-blender-asset-intake-tutorial.md` -- Etape 2 point 1 : ajouter une solution de repli quand aucune reference reelle directe n'existe pour un asset stylise -- comparer alors a un asset gameplay deja valide de taille connue.
- [x] `docs/setup/story-0-6-blender-asset-intake-tutorial.md` -- Etape 5, cas limite import : reformuler pour ne plus presenter le probleme d'echelle/axe comme "frequent FBX Blender-vers-Unity" uniquement -- le meme risque existe pour un export GLB (conventions d'axe glTF differentes), donc la correction a la source Blender s'applique quel que soit le format choisi.

**Criteres d'acceptation :**
- Given le tutoriel Story 0.6 est cree, when il est lu, then il liste la checklist 9 etapes (rename/transforms/echelle/materiaux/normals/geometrie/export/import/prefab), les dossiers source/export/prefab exacts, la regle de stabilite prefab AD-27, la regle AD-13, et un plan collider explicite pour les assets nouveaux.
- Given `VAL-025` est inspecte apres creation du tutoriel, when aucune preuve utilisateur reelle n'a encore ete fournie, then il reste `Not Started` ou `In Progress`, jamais `Pass`.
- Given les lignes Story 0.6 de `epic-0-readiness-checklist.md` sont inspectees, then elles referencent le tutoriel, un statut coherent avec `VAL-025`, et n'emploient jamais de formulation au temps accompli ("preuve fournie") sur une ligne non `Pass`.

## Verification

**Commandes :**
- `Test-Path docs/setup/story-0-6-blender-asset-intake-tutorial.md` -- attendu : `True`.
- `$t = Get-Content -LiteralPath 'docs/setup/story-0-6-blender-asset-intake-tutorial.md' -Raw; foreach ($needle in 'transforms','echelle','normals','FBX','GLB','ArtSource','ArtExports','Prefabs','NetworkObject','definition ids','VAL-025','collider','AD-13') { if ($t -notmatch [regex]::Escape($needle)) { throw "tutoriel manque $needle" } }` -- attendu : aucune erreur.
- `$log = Get-Content -LiteralPath 'docs/setup/tooling-validation-log.md' -Raw; if ($log -notmatch "\|\s*VAL-025\s*\|\s*``(Not Started|In Progress|Blocked)``\s*\|\s*Controles asset intake") { throw "la ligne tableau VAL-025 (colonnes ID/Statut/Zone) doit rester Not Started, In Progress ou Blocked sans preuve" }` -- attendu : aucune erreur (regex ancree sur ID + Statut + Zone dans l'ordre reel du tableau `ID | Statut | Zone | ...`, pas sur une occurrence isolee de `VAL-025` ailleurs dans le fichier).
- `$chk = Get-Content -LiteralPath 'docs/setup/epic-0-readiness-checklist.md' -Raw; if ($chk -match "\|\s*0\.6[^|]*\|\s*``(Not Started|In Progress|Blocked)``\s*\|[^\n]*preuve fournie") { throw "la ligne 0.6 (colonne Story puis Statut) ne doit pas dire 'preuve fournie' tant que son statut n'est pas Pass" }` -- attendu : aucune erreur (regex ancree sur les colonnes Story puis Statut du tableau, couvrant les trois statuts non-`Pass` reels -- pas seulement `Not Started` litteralement ; la ligne 0.6 est actuellement `In Progress`).

## Spec Change Log

- **Trigger :** revue step-04 (loopback bad_spec, iteration 1) sur deux findings : (1) l'AC epics.md "collider plan" n'etait couvert par aucune tache du spec original -- le tutoriel livre n'abordait que la preservation de colliders existants (AD-27), jamais la definition d'un plan collider pour un asset entierement nouveau ; (2) la commande de verification `VAL-025` n'etait pas ancree a la ligne du tableau principal, donc un futur flip vers `Pass` sans preuve resterait indetecte tant qu'une autre occurrence `VAL-025` avec statut autorise existe ailleurs dans le fichier (confirme par mutation test du reviewer verification-gap).
  **Amende :** Tasks & Acceptance (ajout d'une tache "Plan collider" + note nettoyage-geometrie/collider, ajout AD-13 en section dediee, ajout Cas limite pour toutes les etapes, correction du format `last_updated`, correction wording "preuve fournie" sur ligne non-Pass) et Verification (commande VAL-025 ancree au tableau principal, ajout d'une commande dediee pour la formulation de la ligne readiness checklist).
  **Etat connu-mauvais evite :** un tutoriel Story 0.6 qui satisfait les checks superficiels mais omet un livrable explicitement requis par l'AC source (collider plan), et un garde-fou de verification qui ne protege pas reellement contre un faux `Pass` futur sur VAL-025 -- risque deja materialise historiquement sur ce projet (voir corrections VAL-016 a VAL-019 et VAL-020/021 dans `tooling-validation-log.md`).
  **KEEP (a preserver telle quelle a la re-derivation) :** structure globale du tutoriel (Sources consultees, Avant de commencer, Dossiers exacts, checklist 9 etapes, etapes 1-5 numerotees avec Preuve VAL-025/partie N, section AD-27 dediee, Exemple travaille `scene_test.glb`, Rappel caviardage, Mettre a jour les documents de suivi, Arret obligatoire avant Story 0.7) ; l'usage de `scene_test.glb` comme exemple travaille documente honnetement comme non-converti ; la synchronisation VAL-025 / readiness checklist / sprint-status en une seule passe coherente ; le refus explicite de convertir reellement `scene_test.glb` en prefab ou d'inventer une convention de collider fixe (reste "Ask First").

- **Trigger :** revue step-04 (loopback bad_spec, iteration 2) sur un finding convergent trouve independamment par deux reviewers : la commande de verification 3 (garde-fou "preuve fournie" sur la ligne readiness checklist 0.6) ne matchait que le texte litteral `Not Started`, alors que le statut reel de la ligne 0.6 apres implementation est `In Progress` -- le garde-fou etait donc mort des le premier passage reel, incapable de proteger l'etat que la ligne occupe effectivement (confirme par mutation test des deux reviewers). Egalement corriges dans la meme iteration (findings mineurs mais reels, groupes pour eviter un 3e cycle de revue complet) : citations de lignes `mcp-tooling-setup.md` incluant a tort la section "Setup Order" non liee ; contradiction apparente entre le "Plan collider" (jamais deriver un collider d'un asset neuf depuis la geometrie de rendu) et le cas limite Etape 4 (geometrie "prevue comme base d'un futur collider") ; preuve Etape 1 ne demandant pas confirmation que le fichier `.blend` existe reellement ; ligne principale `VAL-025` laissant "A renseigner" alors qu'une preuve datee partielle existe deja (incoherent avec les lignes `In Progress` voisines) ; nouvelle entree datee `VAL-025` cassant le recit continu des entrees `VAL-020, VAL-021` existantes ; export sans rappel de limiter aux objets prevus (risque `Prop_Sphere` residuel) ; pas de repli quand aucune reference d'echelle reelle n'existe ; cas limite d'echelle/axe presente comme "FBX uniquement" alors que GLB partage le meme risque.
  **Amende :** Verification (commande 3 re-ancree sur les colonnes Story+Statut du tableau readiness checklist, couvrant les trois statuts non-`Pass`, plus mutation-testee avant redispatch) et Tasks & Acceptance (nouvelles taches ciblees pour chacun des points mineurs listes ci-dessus, en plus des deux taches de creation du tutoriel deja marquees `[x]` qui restent largement valides -- seules des corrections localisees sont necessaires, pas une re-derivation complete).
  **Etat connu-mauvais evite :** un garde-fou de verification qui semble proteger une regle honnete ("jamais 'preuve fournie' sur une ligne non-Pass") mais qui, dans les faits, ne declenche jamais pour l'etat reel que la ligne occupe -- meme classe de risque que le loopback precedent (VAL-025), maintenant traitee de maniere generalisable (ancrage sur structure de colonnes + ensemble complet de statuts, pas un statut litteral unique).
  **KEEP (a preserver telle quelle a la re-derivation) :** tout le contenu du tutoriel valide en iteration 1 (structure globale, Plan collider, sections AD-13/AD-27, exemple travaille honnete `scene_test.glb`, checklist 9 etapes en 5 etapes numerotees) -- cette iteration applique des corrections localisees, pas une reecriture ; la synchronisation VAL-025/readiness-checklist/sprint-status en une seule passe coherente reste la bonne approche.

**Iteration 3 (round-3 review, patch uniquement, pas de loopback) :** revue step-04 sans finding `bad_spec`/`intent_gap` -- les quatre commandes de Verification protegent reellement ce qu'elles pretendent (confirme par mutation test independant sur chacune). Findings `patch` appliques directement sans re-derivation : colonne "Validation attendue" de `VAL-025` mise a jour (mentionnait encore le texte pre-Story-0.6, sans geometrie/collider/prefab) ; enumeration incomplete de la ligne readiness-checklist 0.6 (materiaux/partie 3 manquants) ; preuve Etape 5/partie 5 etendue pour exiger le choix de collider documente (fermait l'ecart entre le "Critere de completion" de la ligne 67 et la preuve demandee) ; accord grammatical ; wording `Selected Objects Only` rendu agnostique du format d'export (FBX vs glTF/GLB) ; mapping Etape 1 corrige pour refleter le pivot/origine et la sauvegarde source, non listes separement dans la checklist 9 etapes source ; ajout du dossier `Assets/RoadRage/Materials/` aux "Dossiers exacts" ; ajout d'une citation directe vers l'AC source `epics.md` dans les Sources consultees. Findings restants (edge cases de profondeur -- re-sauvegarde apres modification, verification non-visuelle des normals, selection vide a l'export, references d'echelle contradictoires, cas d'un tout nouveau type d'objet gameplay necessitant une premiere registration `NetworkObject`) juges hors scope pour cette premiere version du tutoriel (raffinements futurs, pas des defauts bloquants) -- non ajoutes a `deferred-work.md` individuellement car ils relevent tous du meme theme deja couvert par les entrees existantes sur le multi-objet/variantes et le versionnage.

## Suggested Review Order

**Plan collider (livrable central de la revue -- ferme le gap AC "collider plan" trouve en iteration 1)**

- Point de depart : regle "asset neuf vs remplacement d'art", jamais de collider derive de la geometrie de rendu pour un asset neuf.
  [`story-0-6-blender-asset-intake-tutorial.md:61`](../../docs/setup/story-0-6-blender-asset-intake-tutorial.md#L61)

- Application concrete : verification collider avant suppression de geometrie, scopee explicitement au remplacement d'art (fix de la contradiction trouvee en iteration 2).
  [`story-0-6-blender-asset-intake-tutorial.md:101`](../../docs/setup/story-0-6-blender-asset-intake-tutorial.md#L101)

- Preuve exigee : le plan collider applique doit etre documente dans les notes du prefab (fix iteration 3, fermait l'ecart avec le Critere de completion readiness-checklist).
  [`story-0-6-blender-asset-intake-tutorial.md:116`](../../docs/setup/story-0-6-blender-asset-intake-tutorial.md#L116)

**Garde-fous de verification (les deux regressions trouvees et corrigees au fil des iterations)**

- Commande VAL-025 ancree sur les colonnes ID+Statut+Zone du tableau principal (fix iteration 1 : l'ancienne regex matchait n'importe quelle occurrence `VAL-025` dans le fichier).
  [`spec-0-6-blender-and-3d-asset-intake-pipeline.md:78`](spec-0-6-blender-and-3d-asset-intake-pipeline.md#L78)

- Commande readiness-checklist ancree sur les colonnes Story+Statut, couvrant les trois statuts non-`Pass` (fix iteration 2 : l'ancienne regex ne matchait que le texte litteral `Not Started`).
  [`spec-0-6-blender-and-3d-asset-intake-pipeline.md:79`](spec-0-6-blender-and-3d-asset-intake-pipeline.md#L79)

**Checklist 5 etapes et cas limites**

- Etape 1 : preuve exige la confirmation que le fichier `.blend` existe reellement, cas limite bloque la suite tant qu'il n'existe pas.
  [`story-0-6-blender-asset-intake-tutorial.md:76`](../../docs/setup/story-0-6-blender-asset-intake-tutorial.md#L76)

- Etape 5 : conversion prefab seulement apres test d'echelle, jamais avant -- cas limite d'echelle/axe generalise a FBX et GLB.
  [`story-0-6-blender-asset-intake-tutorial.md:114`](../../docs/setup/story-0-6-blender-asset-intake-tutorial.md#L114)

- Exemple travaille honnete : `scene_test.glb` documente comme brouillon qui ne peut pas depasser l'Etape 1 (fichier `.blend` source jamais sauvegarde).
  [`story-0-6-blender-asset-intake-tutorial.md:126`](../../docs/setup/story-0-6-blender-asset-intake-tutorial.md#L126)

**Synchronisation des documents de suivi (jamais de `Pass` sans preuve reelle)**

- Ligne principale `VAL-025`, statut `In Progress`, preuve attendue precisee pour les 5 parties.
  [`tooling-validation-log.md:54`](../../docs/setup/tooling-validation-log.md#L54)

- Lignes Story 0.6 du readiness checklist (action manuelle et validation agent), synchronisees et honnetes (temps a venir, jamais "preuve fournie" hors `Pass`).
  [`epic-0-readiness-checklist.md:51`](../../docs/setup/epic-0-readiness-checklist.md#L51)

- Statut sprint Story 0.6 avance a `review` (travail agent-executable termine, preuve utilisateur reelle reste a fournir).
  [`sprint-status.yaml:44`](sprint-status.yaml#L44)

