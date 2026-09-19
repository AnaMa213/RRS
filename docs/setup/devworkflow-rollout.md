# Rollout workflow de developpement IA

Listing technique, suivi hors du mecanisme Epic/Story BMAD (decision de Kenan, 2026-09-17 : ce travail est de l'outillage pur, pas du gameplay, il n'a pas besoin de cette ceremonie).

Les decisions d'architecture qui gouvernent ces taches restent dans BMAD :

- **Invariants** : `_bmad-output/planning-artifacts/architecture/architecture-RRS-devworkflow-2026-09-17/ARCHITECTURE-SPINE.md` (AD-1 a AD-14)
- **Decisions par outil** : `docs/setup/addon-adoption-register.md` (ADDON-011 a ADDON-025)
- **Trace d'analyse** : `_bmad-output/planning-artifacts/sprint-change-proposal-2026-09-17.md` (rejete comme mecanisme de suivi, garde comme preuve/declencheur/impact)

Coche une case ici au fur et a mesure ; pas de statut `review`/`done` a synchroniser ailleurs.

## P0 — fait (2026-09-17)

- [x] `.graphifyignore` cree, graphe rescope (997 -> 257 fichiers indexes, 12011 -> 4036 noeuds)
- [x] Regle de precheck symbole nomme ecrite dans `AGENTS.md` (AD-3)
- [x] Garde-fou double etat Unity MCP ecrit dans `AGENTS.md` (AD-6)

## P1

- [x] **`scripts/validate.ps1`** — chaine `unity status` -> `recompile` -> `recompile_status` -> `console --level error` -> tests cibles -> `list_open_scenes` + `git status`. Echoue ferme a chaque etape (AD-8) ; gate erreurs, warnings filtres de `Assets/Synty/` (AD-7) ; affiche la version CLI detectee, refuse une version non validee.
  - [x] **Prealable bloquant leve** : `run_tests` / `test_status` exerces en direct sur `RoadRage.Tests.EditMode.RoadRageScaffoldTests` (7/7 passes, `status:"completed"`) avant d'ecrire le script.
  - [x] Routing EditMode/PlayMode selectionnable (AD-9) — parametre `-TestMode EditMode|PlayMode|Both`
  - [x] Verifie en direct 2026-09-17 : succes (7/7 EditMode, exit 0) et echec ferme (`-TestFilter` sans correspondance -> `ECHEC ... absence de resultat n'est jamais un succes`, exit 1)
- [x] **Audit de cohesion `RunFlowController.cs`** — rapport complet : `docs/setup/audit-runflowcontroller-cohesion.md`. Verdict : **pas de refactor maintenant**. 9 groupes fonctionnels cohesifs sous un seul role d'integration runtime (le fichier ne porte deja plus le menu d'echappement ni le lobby, extraits ailleurs). Le 21 % de churn est confirme en frequence de commits (23/108) mais tombe a 5,8 % en volume de lignes ; les co-changements suivent strictement les frontieres de groupes (pas de melange). LobbyFlowController (970 l./28 methodes) et NetworkedAIVehicleDriverController (918 l./26 methodes) sont deja gros dans ce codebase — RunFlowController agrege 7 flux paralleles la ou eux n'en traitent qu'un. Declencheur de reevaluation : un meme fix touchant 2 groupes non lies dans un commit, le fichier depassant ~2000 lignes, ou des collisions merge repetees.
- [x] **Audit du cluster de garde d'autorite reseau** — rapport complet : `docs/setup/audit-networked-authority-guard-cluster.md`. Verdict : **aucune extraction**. Les 5 classes implementent AD-18 du spine jeu (pipeline ownership/intent) ; le check `state.ClientId.Value != rpcParams.Receive.SenderClientId` (7 occurrences) est semantiquement identique et deliberement reproduit, mais c'est deja une expression a une ligne — extraire un helper ne gagnerait ni lignes ni clarte, et forcerait a absorber des divergences de domaine reelles (2e palier Recovery avec log de refus, double flux interne Seat, Presentation non pilotee par une touche). Conforme AD-13 : pas de besoin mesure, pas d'extraction. Declencheur de reevaluation nomme : une 6e classe intent, ou une regle d'autorite devant changer partout simultanement (ex. migration vers `OwnerClientId` natif). Deux constats hors perimetre releves mais non traites : (1) les tests existants verifient le texte source et la presence du composant, aucun ne simule une RPC avec `SenderClientId` usurpe pour verifier le refus a l'execution ; (2) `NetworkedPlayerLifecycleIntent.RequestRevive` duplique presque integralement `NetworkedPlayerReviveIntent` (duplication de logique metier, distincte de la garde d'autorite).

## P1 — routage persistant du cycle de build (2026-09-18)

Objectif : `bmad-build <story>` doit appliquer le routing sans qu'on le rappelle dans la conversation.

- [x] **`docs/setup/build-workflow-rules.md`** cree — source unique du specifique au cycle de build : recon conditionnelle, routage EditMode/PlayMode, commande de verification a demander, politique de revue (`security-review` conditionnelle), outils a ne pas invoquer. Charge automatiquement a chaque run via `persistent_facts` dans `_bmad/custom/bmad-build.toml` (rendu par `render_skill.py`, donc identique pour Claude Code, Codex et Copilot). Le transversal reste dans `AGENTS.md` : aucune regle dupliquee.
- [x] **Couche de revue `security-review` conditionnelle** ajoutee a `bmad-build.toml` avec `when` (frontiere de confiance reseau : RPC exposee, `SenderClientId`, ownership, permission `NetworkVariable`, donnee client vers chemin autoritaire). Verifie par rendu reel : `step-04-review.md` porte bien `Run only when: ...`.
- [x] **`scripts/validate.ps1` v1.1** — recapitulatif final lisible (et recapitulatif partiel sur echec) exige par l'etape 9 ; avertissements regroupes par message avec nombre d'occurrences et marque `[projet]` quand le message nomme `Assets/RoadRage`, au lieu de 100 lignes identiques qui noyaient le reste. Gate inchange. **Verifie en direct 2026-09-18** : succes (`-TestFilter` scaffold, 0 erreur, 7/7 EditMode, exit 0) et echec ferme (filtre sans correspondance, recapitulatif partiel, `VALIDATION FAILED / INCOMPLETE`, exit 1).
- [x] **`scripts/find-symbol.ps1`** — precheck AD-3 rendu deterministe. Motif mesure : `rg` est **absent de la session PowerShell de l'agent** (verifie 2026-09-18), or la commande `rg` prescrite par AD-3 produisait une sortie vide que le script existant de toute facon ne distinguait pas d'une absence reelle — trois symboles existants (`LaneGraphRouting`, `LaneNode`, `NetworkedAIVehicleDriverController`) etaient lus « ABSENT ». Le script essaie `rg`, retombe sur `Select-String`, et separe trois issues : `0` trouve, `1` absent (STOP), `2` recherche cassee (resultat **inconnu**, jamais « absent »). Les trois codes sont prouves en direct.
- [x] **Second resserrement du perimetre Graphify (AD-2)** — le premier rescope n'avait traite que l'outillage agent et Synty. Mesure du 2026-09-18 : 235 fichiers / 4 098 noeuds, dont ~1 170 (28,5 %) hors code applicatif — `Packages/packages-lock.json` 433, `Packages/manifest.json` 112, exemples TextMesh Pro ~270, `docs/` ~265, transport Facepunch 45. Exclusions ajoutees (`Packages/`, `ProjectSettings/`, `docs/`, `scripts/`, `Assets/TextMesh Pro/`, `Assets/TutorialInfo/`, fichiers de regles agent) puis `graphify update . --force`. Resultat : **160 fichiers / 2 820 noeuds / 6 762 liens / 120 communautes**, **156/156 `.cs` de `Assets/RoadRage` conserves** (2 270 noeuds, 80,5 %). Limite connue : un fichier deja present dans le manifeste n'est pas elague par une nouvelle regle (5 noeuds residuels sous `graphify-out/memory/`).
- [x] **Dry run** sur une Story reelle non commencee (`5-11-intersection-rules-and-deadlock-prevention`, cle renumerotee en `5-18-…` le 2026-09-18 ; la preuve porte sur le run, pas sur la cle) : les 9 points du critere de reussite sont resolus depuis les fichiers persistants seuls.

## P2 — pilotes

- [x] **Pilote Project Auditor** (`unity cmd audit`, ADDON-018) — baseline complete : `docs/setup/audit-project-auditor-baseline.md`. Package `com.unity.project-auditor-rules` `2.0.0` installe (absent avant, bloquait le scan). Scan complet ~150 s, 4232 diagnostics (1401 hors scope dans `Assets/Synty/`, 1800 dans `Tests/`, 597 en production dont 0 `Major`). Part actionnable confirmee 9/597 (1,5 %) : 8x API obsolete `FindObjectsByType`, 1x prefab reseau en `Resources/`. **Decision : `Adopt` en opt-in manuel**, jamais en gate par defaut — `scripts/validate.ps1 -Audit` ajoute (informatif, hors gate). Reste hors du chemin critique de `validate.ps1` v1, comme prevu.
- [x] **Pilote Microsoft.Unity.Analyzers** (ADDON-019) — baseline complete : `docs/setup/audit-microsoft-unity-analyzers-baseline.md`. DLL recuperee localement (VS Tools for Unity deja installe), copiee dans `Assets/Editor/Analyzers/`, chargee via le label `RoslynAnalyzer` (jamais via `.csproj`, AD-14 confirme : le `.csproj` genere ne reference meme pas la copie projet et pourtant l'analyzer tourne). Catalogue reel lu par reflexion sur le DLL : 43 regles, 3 en `Warning` par defaut, 40 en `Info` (invisibles en Console quel que soit `--level`, comportement Unity/Roslyn standard). Pipeline verifie actif via une sonde jetable (`UNT0033` confirme puis fichier supprime). `.editorconfig` cree : 5 regles en `warning` (3 par defaut + `UNT0007` null-coalescing sur `UnityEngine.Object`, `UNT0004` `Time.fixedDeltaTime` dans `Update`), 38 en `silent`. **Decision : `Adopt`** — 0/10 warnings, 0 faux positif sur le code reel (sous le seuil de rollback) ; valeur preventive plutot que corrective.
- [x] **Cloture** : ADDON-018 (`Adopt`, opt-in via `-Audit`) et ADDON-019 (`Adopt`, `.editorconfig` + analyzer actifs) mis a jour avec leur decision finale dans le registre.

## P2 — harnais PlayMode (prerequis DUR, non traite)

Declare par l'epic 5 (« tracked as tooling in `docs/setup/devworkflow-rollout.md` ») et cite par
`sprint-status.yaml`, mais **absent de ce fichier jusqu'au 2026-09-19** : le suivi etait declare et
n'existait pas. Ligne creee par la Story 5.14, qui en depend directement -- le domaine de la story
touche un des fixtures rouges (`Story57AiTrafficClientPresentationPlayModeTests`).

- [ ] **Rendre la suite PlayMode executable par l'agent, et filtrable.** Mesure du 2026-09-18 : `RoadRage.Tests.PlayMode` est rouge (**3/33**) et `-TestFilter` n'y matche ni par classe, ni par nom complet de methode, ni par assemblage -- quatre tentatives renvoient « aucun test execute » alors que la meme forme matche en EditMode. `unity cmd run_tests --mode PlayMode` en synchrone **ne peut pas** fonctionner (le passage en Play Mode declenche un domain reload qui abandonne la requete) et `--async_tests true` + `test_status` a rendu une fois **0 test**. Les trois echecs partagent la signature de la pollution entre fixtures, coherente avec la sensibilite a l'ordre deja documentee (fixtures partageant le process et le `RoadRageBootstrap` en `DontDestroyOnLoad`).
  - Consequence : **aucune story classee PlayMode ne peut etre prouvee par la porte automatisee**, et 11 bancs PlayMode sont deja en attente (2 pour la 5.11, 2 pour la 5.12, 5 pour la 5.13, plus les deux `[UnityTest]` ecartes de la 5.14 avec le perimetre reaction-au-choc). Depuis la Story 5.14, la recette humaine est la **seule** mesure de plusieurs criteres d'acceptation.
  - Surfaces a regarder en premier, dans cet ordre : (1) isolation des fixtures -- le `RoadRageBootstrap` en `DontDestroyOnLoad` est le suspect nomme, et une fixture qui charge `MVP_Run` pollue les suivantes ; (2) support du filtrage PlayMode, cote `unity cmd run_tests` ou cote NUnit ; (3) chemin asynchrone (`--async_tests` + `test_status`), qui rend aujourd'hui 0 test.
  - Aucun contournement accepte : `scripts/validate.ps1` echoue ferme sur « aucun test execute » (AD-8), et une absence de resultat ne doit **jamais** etre lue comme un succes.

## Reste hors stack (pour memoire, voir ADDON-020 a 025)

Serena, RTK — reportes avec declencheur comportemental, pas de seuil arbitraire.
Roslynator, jscpd, Lefthook, GitHub MCP — hors stack, motif et declencheur de reevaluation dans le registre.
