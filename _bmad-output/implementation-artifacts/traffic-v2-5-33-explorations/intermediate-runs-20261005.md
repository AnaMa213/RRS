# Runs intermediaires du 2026-10-05

Ces quatre sorties locales, initialement exclues du commit de cloture 5.34,
sont archivees a la demande du proprietaire lors de la consolidation Git.
Elles restent des observations intermediaires : aucun journal `validate.ps1`
associe a ces executions n'est archive ici. Elles ne remplacent pas les portes
finales referencees dans `../code-review-5-34-2026-10-05.md`.

- `perf-N8-20261005-113317-summary.md` et `perf-comparison-20261005-113205.md` :
  diagnostic N8 avant la correction de l'initialisation des recorders, campagne
  `20261005-113205`. Le p95 hote en population pleine est **10.133 ms** : la
  cible stricte `< 10 ms` n'est pas satisfaite. Ce resultat est conserve comme
  echec intermediaire, sans modifier le verdict final de la 5.34.
- `explore-8-20261005-113533-summary.md` et son journal `-junction.txt` :
  exploration nominale N8, **8/8 sorties**, zero contact V2-V2 et invariants
  de coordination verts, p95 hote en population pleine **8.951 ms**.

Les TSV bruts de ces runs restent locaux selon les exclusions existantes.
Les portes officielles finales publiees restent `Story533Perf` **5/5**
(N8 p95 **9.479 ms**) et `Story533Exploration` **5/5** (N8 p95 **9.568 ms**).

La consolidation archive aussi la mesure regeneree par la regression officielle
Story531 dans `../traffic-v2-5-31-measurements/playmode-short-run-steps.tsv`.
Seuls les temps de la premiere ligne de synthese changent ; les donnees par pas
et les resultats fonctionnels sont identiques. Le precedent relevé reste
accessible dans l'historique Git.
