# Story 5.52 — Gate A re-signée et Gate B, jalon 1 (2026-10-01)

Statut : Gate A **liée** et Gate B **fermée pour le jalon 1 hors mesure**. Story `in-progress` jusqu'à la revue et au commit approuvé. La couverture entre deux pas est celle du modèle M vérifié, sans revendication continue au-delà de ce modèle.

## Signature et intégrité

- Le propriétaire Kenan a signé le format 3 le `2026-10-01T18:10:44Z`. La liaison réelle renvoie `Valid` / `Allowed`, pose `Kinematic`, `a_e = 0,34 m` et 24 raccords signés.
- Sign-off courant SHA-256 : `c6ab2964d0a78982326936622b5d721e49d4af3025fb85c3ffc218c7e426d1b7`.
- Historique SHA-256 : `3d946722372005b8d100738dd8f99257e1b0a482647910fb7ecd73a68de6b2e6`. Son unique entrée est exactement le texte de l'ancien sign-off, SHA-256 `e2d77b994d6e7e28aefb86ca0fff3d3d35d2eccf7cc15dff2e3097d3f5a2973e` ; aucun ancien artefact signé n'a été réécrit.
- Modèle, overlay et rapport courants : SHA-256 respectifs `4339c4da0dc0d8c5023871c4263a7d59e2e024fd8ae4f1008161101c84be6051`, `b86ef481c3152d24a0a1c9c5dd7d8ae4f2e2e0d21d789f3ca00bd16e28638708`, `4abbed380f1a79a6726ca57d3cdecae653faf8b5cc835e45bffd533a7f6295a4`. Le diff complet avant signature figure dans `phase-c-pre-signature.md`.

## Gate B — jalon 1

`Story552MilestonePlayModeTests.TheFirstV2TripCrossesMvpRunWithinTheSignedLimitsOutsideMeasurement` a traversé `MVP_Run` de portail à portail en composition V2, sans run de mesure. Le test vérifie sur la trace de la caisse : `d ≤ ε_t = 0,34 m` à chaque pas, borne entre deux pas `≤ ε_t` avec vérification du modèle M à chaque intervalle, `v ≤ v*`, aucune sortie de l'enveloppe de largeur, aucun contact de caisse hors chaussée, et aucune écriture des trois preuves liées. La trace est activée par le test pour observer la session hors mesure ; le verdict d'insertion reste hors mesure.

- Profil EditMode Story 5.52 : **24/24**, 0 skipped, 0 inconclusive, 0 erreur Console. La revue a ajouté un test de restauration octet pour octet si le second fichier de re-signature ne peut pas être écrit ; `TryWriteAll` restaure les fichiers déjà remplacés lors d'une erreur d'E/S gérée.
- Profil PlayMode Story 5.52 : **2/2**, 0 skipped, 0 inconclusive, 0 erreur Console. Il comprend aussi le banc physique de réponse 2a accepté.
- Profil PlayMode Story 5.31 ciblé après adaptation du refus historique : **13/13**, 0 skipped, 0 inconclusive, 0 erreur Console.
- `MVP_Run` est restée propre en mémoire après les validations. Les suites complètes relèvent de la fin d'epic.

Le test historique EditMode continue de vérifier les SHA-256 figés des quatre artefacts 5.51, leur liaison interne, puis le refus explicite de reconfirmer les anciennes décisions sur la géométrie actuelle. Le sign-off format 3 est une nouvelle signature du propriétaire ; l'ancien reste uniquement dans l'archive et l'historique.
