# Reprise Story 4.2 — Codex vers Claude

## Etat

- Spec source : `spec-4-2-passenger-action-framework-and-host-validated-intent.md` (`status: in-progress`, toutes les tâches cochées par l'agent interrompu).
- Baseline : `c089fa4ffadebabd398f0c1a93e9ccf193b63074`.
- Implémentation présente : définitions/catalogue/payload/validateur PassengerActions, `NetworkedPassengerActionIntent`, UI, assets, prefab joueur, `Dev_RageSandbox`, `MVP_Run`, tests EditMode et PlayMode.
- Agent Codex interrompu à la demande de l'utilisateur pour économiser les crédits. Ne pas considérer la Story terminée : l'étape BMAD 3 n'a pas été auditée et l'étape 4 (revue) n'a pas commencé.

## Vérification observée

- `Logs/Editor.log` termine sur des exécutions vertes : 4/4 puis 2/2 ; le dernier run PlayMode charge `Dev_RageSandbox` et `MVP_Run`.
- Des erreurs CS0246 transitoires apparaissent plus tôt pendant l'import de l'asmdef PassengerActions, puis les tests ultérieurs passent.
- `git diff --check` est propre sur `Assets/RoadRage` et les artefacts BMAD.
- `git diff --check` global échoue uniquement sur les sorties Graphify régénérées (`graphify-out/GRAPH_REPORT.md`, `graphify-out/graph.json`) qui contiennent des espaces finaux.

## A faire en premier

1. Recompiler Unity et confirmer zéro erreur Console.
2. Relancer explicitement `Story42PassengerActionFrameworkTests` puis `Story42PassengerActionMvpRunPlayModeTests` et conserver les résultats.
3. Corriger ou compléter la preuve hôte/client : `MvpRunProvidesOfflineAndNetworkPassengerActionWiring` vérifie actuellement le câblage, pas un échange réel entre deux instances. L'AC approuvée exige qu'un client passager envoie et que seul l'hôte valide.
4. Auditer chaque ligne de la matrice I/O contre un test réellement exécuté. Les tâches cochées ne remplacent pas cette preuve.
5. Examiner les gros diffs YAML des deux scènes et du prefab, puis exécuter la revue BMAD étape 4 avant de passer la spec/sprint à `review` ou `done`.

## Attention Git

- Aucun commit Story 4.2 n'a été créé.
- `graphify update .` a régénéré un très gros diff et `graphify-out/2026-09-11/`; décider pendant la revue si ces sorties doivent rester.
- Préserver tous les changements existants : ne pas reset/checkout.
