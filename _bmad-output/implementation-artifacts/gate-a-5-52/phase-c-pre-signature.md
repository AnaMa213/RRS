# Story 5.52 — Phase C avant re-signature Gate A (2026-10-01)

Statut : **HALT propriétaire**. La Story reste `in-progress`. Aucun sign-off format 3 n'a été écrit et aucun jalon PlayMode post-signature n'a été lancé.

## Décisions et preuve courante

- Moteur/tests de décisions épinglés au commit préparatoire `66b27388027f8ce6804e9adbfcbdc700594fc024`, approuvé par le propriétaire. Deux plans identiques ont précédé l'application ; le passage suivant ne propose aucun changement (test explicite `Story550AutomatedPairDecisionTests`).
- Manifeste format 2 : 270 paires, dont 42 `ConflictProven`, 94 `ConservativeConflict`, 84 `Following` et 50 `ProvenDisjoint` ; 136 zones actives. Les 270 révisions remplacent les révisions historiques. Ancien manifeste conservé verbatim dans `SupersededManifestText`, SHA-256 `32507cb57f33171bfa2a0e9476b2beff53d70fd3fed0866d886aac9d6e56efb8`.
- Le menu existant `Compiler le modele authore` a régénéré le modèle, l'overlay et le rapport. Le rapport indique `COUVERT`, 0 déficit et 0 échec de mesure. Minima : physique 0,172070518 m, Sidewalk 0,072075896 m, anneau 0,0759442151 m. Bornes fermées en 23 itérations, 0 élément inatteignable, 0 pose infaisable.
- Preuve : `kinematic-v1`, `a_e = 0,34 m`, `max|o| = 0`, `ε_t = 0,34 m`, marge 0,25 m, `δ_c = 0,05 m`, `h_e = 0,008 rad`, `η = 0,002 rad`. Hash des paramètres `2043ab22a4d847d8252ca1a2884a960c169514d1a4d95f1beb1a1712c1d19a84`. Les 24 raccords d'anneau sont dans le bloc `### Residus` haché ; le format 3 et le runtime les liront après la signature.

## Diff des liaisons

| Champ | Historique signé 5.51 | Courant 5.52 |
|---|---|---|
| `source-hash` | `b3064424c2b3ba22f0893eea36ed25f5cc4f85e4582a2a45d211899fbd8292fc` | identique |
| `lineage-hash` | `f838ab5926a2cfe66b3074ae9b828cc17f7298df0e83531ffb6d0f8b84a6e6f4` | identique |
| `decisions-hash` | `3e9c893f48b10a248fa72aa51f7f365c683e7e0a825d657861a65c4356e553fe` | `2e290408351c1b6d8af536d76441974b3bcdb30f814cb692e173c95a41f58e0f` |
| `road-model-version` | `v4:e8dff9e54bff1712308899158ad16a9b` | `v4:46e91057b9f6e8a970fe7952639805a6` |
| `model-hash` | `6525a2366641aeb6c28595259a2762abb4e881ff1c32b2e2f8de16e5e88c1ebb` | `4339c4da0dc0d8c5023871c4263a7d59e2e024fd8ae4f1008161101c84be6051` |
| `overlay-hash` | `20cd1d1f262f812d6fd035119a465a20dcbd4b39c18a58410f481f71d00a0abe` | `b86ef481c3152d24a0a1c9c5dd7d8ae4f2e2e0d21d789f3ca00bd16e28638708` |
| `physical-input-hash` | `5fdefa17a73c7183e701dcd94c5d20ff1b46785e1aa05911c6e3a8416a0c3595` | `c777ce4ebb537362ace165a24d68f844c58a0572eac76e83dcb7bad659afd2d9` |
| `semantic-input-hash` | `9fae1d4e8423b1f7d8485ab8ff2b12dd6b334a1d49a9d40abf2f6fb6caeb6a57` | `43e098fcd6809ecc1e8043156f7cd26f57abb73121b844cf6c8badef8b79aef4` |
| `clearance-hash` | `f397d2ca83e229e3c680d902db8344754656df4ec304d72aafcc94aa7a922d6e` | `7b7fcbf8f2458e4165ad2adb47e7222b52b12e5754baddbb8a76a277c555f14a` |

Le rapport courant complet a le SHA-256 `4abbed380f1a79a6726ca57d3cdecae653faf8b5cc835e45bffd533a7f6295a4`. Le sign-off actif est encore exactement l'ancien (`e2d77b994d6e7e28aefb86ca0fff3d3d35d2eccf7cc15dff2e3097d3f5a2973e`) : l'admission retourne `GateAEvidenceStale`. Aucun fichier `MVP_Run.road-signoff-history.json` n'existe encore.

Les quatre fichiers signés historiques sont conservés sous `historical-signed-5-51/`, avec leurs SHA-256 figés dans `Story552KinematicPoseSetTests`. Le test vérifie leurs octets et la cohérence de la liaison. Le test d'historique de re-signature vérifie en mémoire l'ajout verbatim de l'ancien sign-off, sans écrire sur disque.

## Vérification avant HALT

- Story 5.52 EditMode : **22/22**, 0 erreur Console.
- Fixtures ciblées : 5.51 **33/33** ; 5.30 replay **2/2** et planning **23/23** ; 5.31 cycle de vie **23/23**, vitesse **17/17**, replay **3/3** ; trois contrôles 5.28 de liaison et sign-off en mémoire **1/1** chacun. Chaque passe terminée : 0 erreur Console.
- `MVP_Run` n'a aucune modification en mémoire. La suite EditMode complète n'a pas été exécutée (réservée à la fin d'epic).

## Point d'arrêt

Revue des 25 instances de l'overlay et re-signature Gate A format 3 par le propriétaire depuis la fenêtre dédiée, après approbation distincte. Cette action archivera l'ancien sign-off dans l'historique avant d'écrire le nouveau. Le jalon PlayMode 5.52 hors mesure et la fermeture Gate B suivent seulement la nouvelle signature. Aucun autre commit n'est effectué à ce point d'arrêt.
