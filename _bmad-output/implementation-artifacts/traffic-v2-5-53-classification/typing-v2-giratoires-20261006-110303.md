# Typing-v2 5.53a -- fusions de giratoire sur MVP_Run (verification ciblee, rien n'est ecrit)

Chemin 5.52 (regeneration, parametres declares), puis `ConflictSweep.Refine` en typing-v2 (contenance et dedoublonnage exact des racines) sur les seules 12 fusions de giratoire ; aucun plan global. Budget 65536 feuilles par paire, tolerances, profondeur, `WitnessSplit` et ordre inchanges. Debuts de contact dans l'ordre de la cle de paire (A / B).

- Fusions Ouest non typees `Merge` sous le budget (HALT, fallback Crossing) : 0.
- Fusions Sud / Diagonale dont genre ou debuts different d'un v1 recalcule (HALT) : 0.
- Preuves v2 differentes du typage committe : 0.

| Bras | Giratoire | Mouvements | v1 recalcule | v1 feuilles | v2 | v2 feuilles | Racines | Doublons ecartes | Prouvees | Temoins | Resolution | Contenance | Duree (s) |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Ouest | Roundabout_SouthWest | Connector_West_In -> Ring_Merge_West (entree d'anneau) | Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Crossing 0 / 0 | 65536 (incomplet) | Merge 0 / 1.46071267 | 46339 | 120530 | 77651 | 1698 | 42077 | 446 | 388 | 24.559 |
| Ouest | Roundabout_NorthWest | Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Connector_West_In -> Ring_Merge_West (entree d'anneau) | Crossing 0 / 0 | 65536 (incomplet) | Merge 0 / 1.46071374 | 46235 | 120530 | 77651 | 1620 | 42077 | 446 | 414 | 24.336 |
| Ouest | Roundabout_NorthEast | Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Connector_West_In -> Ring_Merge_West (entree d'anneau) | Crossing 0 / 0 | 65536 (incomplet) | Merge 0 / 1.46071267 | 46235 | 120530 | 77651 | 1620 | 42077 | 446 | 414 | 24.277 |
| Ouest | Roundabout_SouthEast | Ring_Split_West -> Ring_Merge_West (continuation d'anneau) | Connector_West_In -> Ring_Merge_West (entree d'anneau) | Crossing 0 / 0 | 65536 (incomplet) | Merge 0 / 1.46071267 | 46235 | 120530 | 77651 | 1620 | 42077 | 446 | 414 | 24.429 |
| Sud | Roundabout_SouthWest | Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) | Merge 1.434629 / 0 | 57194 | Merge 1.434629 / 0 | 26909 | 46862 | 23431 | 1659 | 22843 | 459 | 209 | 13.015 |
| Sud | Roundabout_NorthWest | Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) | Merge 1.43462956 / 0 | 57188 | Merge 1.43462956 / 0 | 26906 | 46860 | 23430 | 1657 | 22843 | 459 | 209 | 12.961 |
| Sud | Roundabout_NorthEast | Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) | Merge 1.43462873 / 0 | 57194 | Merge 1.43462873 / 0 | 26909 | 46862 | 23431 | 1659 | 22843 | 459 | 209 | 13.179 |
| Sud | Roundabout_SouthEast | Ring_Split_South -> Ring_Merge_South (continuation d'anneau) | Connector_South_In -> Ring_Merge_South (entree d'anneau) | Merge 1.434629 / 0 | 57194 | Merge 1.434629 / 0 | 26909 | 46862 | 23431 | 1659 | 22843 | 459 | 209 | 13.001 |
| Diagonale | Roundabout_SouthWest | Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Merge 0 / 1.46071386 | 28236 | Merge 0 / 1.46071386 | 26812 | 23376 | 0 | 1669 | 22796 | 444 | 185 | 4.511 |
| Diagonale | Roundabout_NorthWest | Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Merge 0 / 1.46071267 | 28236 | Merge 0 / 1.46071267 | 26812 | 23376 | 0 | 1669 | 22796 | 444 | 185 | 4.487 |
| Diagonale | Roundabout_NorthEast | Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Merge 0 / 1.46071386 | 28236 | Merge 0 / 1.46071386 | 26812 | 23376 | 0 | 1669 | 22796 | 444 | 185 | 4.492 |
| Diagonale | Roundabout_SouthEast | Connector_Diagonal_In -> Ring_Merge_Diagonal (entree d'anneau) | Ring_Split_Diagonal -> Ring_Merge_Diagonal (continuation d'anneau) | Merge 0 / 1.46071327 | 28236 | Merge 0 / 1.46071327 | 26812 | 23376 | 0 | 1669 | 22796 | 444 | 185 | 4.477 |

