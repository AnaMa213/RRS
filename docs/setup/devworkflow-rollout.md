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

## P2 — pilotes

- [x] **Pilote Project Auditor** (`unity cmd audit`, ADDON-018) — baseline complete : `docs/setup/audit-project-auditor-baseline.md`. Package `com.unity.project-auditor-rules` `2.0.0` installe (absent avant, bloquait le scan). Scan complet ~150 s, 4232 diagnostics (1401 hors scope dans `Assets/Synty/`, 1800 dans `Tests/`, 597 en production dont 0 `Major`). Part actionnable confirmee 9/597 (1,5 %) : 8x API obsolete `FindObjectsByType`, 1x prefab reseau en `Resources/`. **Decision : `Adopt` en opt-in manuel**, jamais en gate par defaut — `scripts/validate.ps1 -Audit` ajoute (informatif, hors gate). Reste hors du chemin critique de `validate.ps1` v1, comme prevu.
- [x] **Pilote Microsoft.Unity.Analyzers** (ADDON-019) — baseline complete : `docs/setup/audit-microsoft-unity-analyzers-baseline.md`. DLL recuperee localement (VS Tools for Unity deja installe), copiee dans `Assets/Editor/Analyzers/`, chargee via le label `RoslynAnalyzer` (jamais via `.csproj`, AD-14 confirme : le `.csproj` genere ne reference meme pas la copie projet et pourtant l'analyzer tourne). Catalogue reel lu par reflexion sur le DLL : 43 regles, 3 en `Warning` par defaut, 40 en `Info` (invisibles en Console quel que soit `--level`, comportement Unity/Roslyn standard). Pipeline verifie actif via une sonde jetable (`UNT0033` confirme puis fichier supprime). `.editorconfig` cree : 5 regles en `warning` (3 par defaut + `UNT0007` null-coalescing sur `UnityEngine.Object`, `UNT0004` `Time.fixedDeltaTime` dans `Update`), 38 en `silent`. **Decision : `Adopt`** — 0/10 warnings, 0 faux positif sur le code reel (sous le seuil de rollback) ; valeur preventive plutot que corrective.
- [x] **Cloture** : ADDON-018 (`Adopt`, opt-in via `-Audit`) et ADDON-019 (`Adopt`, `.editorconfig` + analyzer actifs) mis a jour avec leur decision finale dans le registre.

## Reste hors stack (pour memoire, voir ADDON-020 a 025)

Serena, RTK — reportes avec declencheur comportemental, pas de seuil arbitraire.
Roslynator, jscpd, Lefthook, GitHub MCP — hors stack, motif et declencheur de reevaluation dans le registre.
