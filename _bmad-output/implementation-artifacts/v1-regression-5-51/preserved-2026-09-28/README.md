# Preservation 5.51 -- 2026-09-28

Sauvegarde recuperable de l'etat de reprise de la Story 5.51 (reprise locale apres interruption de Codex).

## Contenu

- `review-fixes.patch` : diff des corrections de revue de Codex (4 fichiers, +154/-16) tel que trouve a la reprise.
- `corrections-finales.patch` : diff complet des corrections de revue + completions de l'agent (etat `post-review-2`).
- `JunctionClearance.cs`, `Story551JunctionClearanceTests.cs` : copies du dernier etat valide.
- `spec-5-51.md`, `resume-proof.md` : copies des documents modifies non committes.
- `test_status_editmode_914of915.json` : verdict EditMode complet de Codex avant la derniere etape (915 tests, 1 echec Gate A).
- `post-fix-a/` : copies de reference intermediaires (SHA256 `45044550...` / `313F1DE4...`).
- `post-review-2/` : copies de reference FINALES (SHA256 `1903CA78...` / `B0D8A152...`), source de toute restauration.
- `reprise-targeted-run.txt` + `reprise-targeted-results.json` : checkpoint cible 33/33 execute par l'agent.
- `reprise-mutation-run.txt` + `reprise-mutation-results.json` : epreuve par mutation (8/33 echecs exactement attendus).
- `reprise-editmode-full.txt` + `reprise-editmode-results.json` : suite EditMode complete 918 tests, 1 echec Gate A.
- `reprise-playmode-run.txt` + `reprise-playmode-results.json` + `reprise-playmode-compare.txt` : PlayMode session fraiche 39/45 et comparaison test par test.

## Capture

`Tee-Object -FilePath` ne capture pas les runs de `validate.ps1` (sortie `Write-Host`, aucun objet pipeline : le fichier n'est jamais cree). Tous les runs `reprise-*` ont ete captures par processus enfant et redirection au niveau process (`powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "& '<abs>\scripts\validate.ps1' ..." *> $log`), puis `unity cmd test_status --format json`.

## Restauration

Depuis la racine du depot :

```powershell
Copy-Item "_bmad-output\implementation-artifacts\v1-regression-5-51\preserved-2026-09-28\post-review-2\JunctionClearance.cs" "Assets\RoadRage\Features\Vehicles\Traffic\Migration\JunctionClearance.cs" -Force
Copy-Item "_bmad-output\implementation-artifacts\v1-regression-5-51\preserved-2026-09-28\post-review-2\Story551JunctionClearanceTests.cs" "Assets\RoadRage\Tests\EditMode\Story551JunctionClearanceTests.cs" -Force
```

Puis `unity cmd run_script --file AgentScripts\Story516ForceAssetRefresh.cs --entry Story516ForceAssetRefresh.Run`
et attendre `recompile_status: completed`.

## Repères

- HEAD au moment de la sauvegarde : `9d7c4cd` (branche `systeme-traffic-ia-v2`, non poussee).
- Baseline de la story : `58c230a311ef61f81611ca640dd28b4c5bac28a9`.
- Gate A reste fermee ; 5.28 et 5.51 restent `in-progress` tant que le proprietaire n'a pas tranche.

## Reprise du 2026-09-28 (soir)

Les corrections de revue de Codex (patches ci-dessus) ont ete completees par l'agent : filtrage par scene de `RoadTop`, role de trottoir symetrique, UV hors empreinte physique, garde Bootstrap, test d'integration de la porte visuelle, invariant +inf robuste, `AlgorithmVersion` 5. Etat final des sources : `post-review-2/`. Toutes les validations `reprise-*` ont ete executees par l'agent dans cette session ; le detail figure dans le rapport de reprise et le journal de spec.
