---
title: 'Story 0.5 : Configuration Codex, Claude, Unity MCP et Blender MCP'
type: 'chore'
created: '2026-09-03'
status: 'done'
review_loop_iteration: 0
baseline_commit: 'f2d7a082bc05cd97ff24618a771c36fabc34cb8c'
context:
  - '{project-root}/_bmad-output/implementation-artifacts/epic-0-context.md'
  - '{project-root}/docs/setup/tooling-validation-log.md'
  - '{project-root}/docs/setup/epic-0-readiness-checklist.md'
  - '{project-root}/_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/mcp-tooling-setup.md'
---

<frozen-after-approval reason="human-owned intent - do not modify unless human renegotiates">

## Intention

**Probleme :** Story 0.4 a cloture le scaffold du projet, mais aucun outil MCP (Unity, Blender) n'est configure pour Codex/Claude, et rien ne documente les actions autorisees/interdites par client. Sans cela, l'assistance IA risque de modifier scenes, packages ou assets sans revue, ou de stocker des secrets.

**Approche :** Creer un tutoriel Story 0.5 (meme forme que `docs/setup/story-0-3-unity-cloud-services-tutorial.md`) qui distille le brouillon `mcp-tooling-setup.md` : ordre de preference Unity MCP (Unity Official puis fallback CoplayDev epingle a un tag), Blender MCP (Blender Lab puis fallback ahujasid), chemins de configuration client Codex/Claude Code/Claude Desktop, smoke tests sans danger (GameObject temporaire Unity, cube + export `.glb` Blender), et actions interdites separees par client. Mettre a jour `tooling-validation-log.md` (VAL-020 a VAL-024) et `epic-0-readiness-checklist.md` sans jamais passer `Pass` sans preuve utilisateur reelle.

## Limites & Contraintes

**Toujours :** Garder la selection et configuration MCP comme actions manuelles utilisateur ; documenter le chemin d'installation/repo, la version ou le tag epingle, et le chemin exact de configuration client pour Codex et Claude (Code et Desktop) ; lister actions autorisees/interdites separement par client ; reutiliser la regle de caviardage `[REDACTED_TOKEN]` de Story 0.3 pour toute cle API MCP.

**Demander d'abord :** Activer un abonnement/beta Unity AI payant, adopter IvanMurzak Unity MCP ou tout autre MCP non liste dans `mcp-tooling-setup.md`, ou faire executer par un MCP un changement de package/version en dehors d'une revue explicite.

**Jamais :** Ne pas laisser un MCP modifier scenes, prefabs, scripts, packages ou assets sans revue humaine et commit en petits pas ; ne pas stocker de token, cle API ou secret dans prompts, config, logs ou fichiers commits ; ne pas demarrer plusieurs bridges MCP sur le meme editeur sauf support explicite ; ne pas marquer `Pass` sans preuve re-verifiable et caviardee ; ne pas commencer le gameplay Epic 1 ou l'intake Blender complet (Story 0.6) ici.

## Matrice I/O & Cas Limites

| Scenario | Entree / Etat | Sortie / Comportement attendu | Gestion d'erreur |
|----------|---------------|-------------------------------|-------------------|
| Configuration nominale | Unity Official MCP accessible (compte AI Unity actif) | Tutoriel guide `Project Settings > AI > Unity MCP`, config client, smoke test GameObject temporaire | Preuve VAL-020/021 caviardee dans le log |
| Unity Official MCP indisponible | Pas d'acces beta/abonnement Unity AI | Tutoriel bascule sur CoplayDev MCP epingle a `v10.0.0`, meme smoke test | Note la raison du fallback dans le log, pas de `Pass` sur l'option non utilisee |
| Blender Lab MCP instable/indisponible | Extension Blender Lab absente ou instable | Tutoriel bascule sur fallback ahujasid (`uvx blender-mcp`) | Note la raison du fallback dans le log |
| Secret dans une preuve | Config ou capture contient une cle API/token MCP | Preuve rejetee jusqu'a caviardage `[REDACTED_TOKEN]` | Ne jamais copier le secret dans le tutoriel ou le log |

</frozen-after-approval>

## Code Map

- `docs/setup/story-0-3-unity-cloud-services-tutorial.md` -- Patron de structure a reutiliser : Contexte, Sources consultees, Avant de commencer (regle de caviardage), etapes numerotees avec "Preuve a fournir (VAL-###)" et "Cas limite", tableau de sync, arret obligatoire.
- `_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/mcp-tooling-setup.md` -- Contenu source deja verifie (options Unity MCP/Blender MCP, commandes d'installation, smoke tests, liste "MCP ne doit jamais faire") a distiller dans le nouveau tutoriel.
- `docs/setup/tooling-validation-log.md:49-53` -- Lignes `VAL-020` a `VAL-024` deja reservees (Selection/Smoke test Unity MCP, Installation Blender, Selection/Smoke test Blender MCP), toutes `Not Started` ; a faire pointer vers le nouveau tutoriel.
- `docs/setup/epic-0-readiness-checklist.md:50,66` -- Lignes Story 0.5 (action manuelle et validation agent), `Not Started`, a synchroniser avec le tutoriel.
- `Assets/Editor/RoadRageSteamworksSmokeTest.cs` -- Reference de convention editor-only + no-secret-logging si un helper Unity s'avere utile ; les smoke tests MCP eux-memes passent par le client MCP, pas par un script.
- `_bmad-output/implementation-artifacts/epic-0-context.md` -- Decisions techniques MCP deja actees (preference Unity Official/CoplayDev, Blender Lab/ahujasid) a garder coherentes.
- `_bmad-output/implementation-artifacts/sprint-status.yaml` -- Statut Story 0.5 a faire progresser uniquement quand le workflow spec l'atteint.

## Taches & Acceptation

**Execution :**
- [ ] `docs/setup/story-0-5-mcp-tooling-configuration-tutorial.md` -- Creer le tutoriel lineaire (meme forme que Story 0.3) couvrant Unity MCP (Official puis fallback CoplayDev `v10.0.0`), Blender MCP (Lab puis fallback ahujasid), chemins de config Codex/Claude Code/Claude Desktop, actions autorisees/interdites par client, smoke tests sans danger, et regle de caviardage -- donne au solo-dev un guide unique et complet.
- [ ] `docs/setup/tooling-validation-log.md` -- Faire pointer `VAL-020` a `VAL-024` vers le tutoriel avec preuves attendues precisees, sans passer `Pass` tant qu'aucune preuve utilisateur n'est fournie -- garde le log honnete.
- [ ] `docs/setup/epic-0-readiness-checklist.md` -- Synchroniser les lignes Story 0.5 (action manuelle / validation agent) avec le tutoriel et son statut reel -- garde la porte Epic 0 lisible.
- [ ] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- Faire avancer Story 0.5 (`in-progress` puis `review`) uniquement quand le travail agent-executable correspondant est pret -- garde le suivi sprint synchronise.

**Criteres d'acceptation :**
- Given le tutoriel Story 0.5 est cree, when il est lu, then il liste le serveur MCP Unity et Blender selectionnes, le chemin d'installation/repo, la version ou le tag epingle, et le chemin exact de configuration client pour Codex et Claude.
- Given le tutoriel est inspecte, when les actions MCP sont recherchees, then les actions autorisees et interdites sont listees separement pour Codex, Claude Code et Claude Desktop.
- Given `VAL-020` a `VAL-024` sont inspectes apres creation du tutoriel, when aucune preuve utilisateur reelle n'a encore ete fournie, then elles restent `Not Started` ou `In Progress`, jamais `Pass`.
- Given le log de validation est inspecte, then il affirme que les MCP ne doivent jamais ajouter silencieusement de service payant, changer une version de package, convertir vers des dedicated servers, stocker un secret, ou contourner l'intake d'assets.

## Verification

**Commandes :**
- `Test-Path docs/setup/story-0-5-mcp-tooling-configuration-tutorial.md` -- attendu : `True`.
- `$t = Get-Content -LiteralPath 'docs/setup/story-0-5-mcp-tooling-configuration-tutorial.md' -Raw; foreach ($needle in 'Unity Official MCP','CoplayDev','v10.0.0','Blender Lab','ahujasid','Codex','Claude Code','Claude Desktop','[REDACTED_TOKEN]','VAL-020','VAL-024') { if ($t -notmatch [regex]::Escape($needle)) { throw "tutoriel manque $needle" } }` -- attendu : aucune erreur.
- `$log = Get-Content -LiteralPath 'docs/setup/tooling-validation-log.md' -Raw; foreach ($id in 'VAL-020','VAL-021','VAL-022','VAL-023','VAL-024') { if ($log -notmatch "\|\s*$id\s*\|\s*``(Not Started|In Progress|Blocked)``") { throw "$id doit rester Not Started, In Progress ou Blocked sans preuve" } }` -- attendu : aucune erreur.

## Suggested Review Order

**Selection et fallback MCP**

- Entry point : ordre de preference Unity MCP et instructions d'arret propre du bridge primaire avant le fallback CoplayDev.
  [`story-0-5-mcp-tooling-configuration-tutorial.md:27`](../../docs/setup/story-0-5-mcp-tooling-configuration-tutorial.md#L27)

- Meme logique cote Blender : Blender Lab prefere, arret du serveur avant de basculer sur ahujasid.
  [`story-0-5-mcp-tooling-configuration-tutorial.md:107`](../../docs/setup/story-0-5-mcp-tooling-configuration-tutorial.md#L107)

**Chemins de configuration client et gouvernance**

- Chemins exacts Codex/Claude Code/Claude Desktop, plus la regle pour les clients auto-detectes non couverts (Cursor/Windsurf/VS Code Copilot) et le rappel de verifier `.mcp.json` avant commit.
  [`story-0-5-mcp-tooling-configuration-tutorial.md:61`](../../docs/setup/story-0-5-mcp-tooling-configuration-tutorial.md#L61)

- Actions autorisees/interdites par client pour Unity MCP.
  [`story-0-5-mcp-tooling-configuration-tutorial.md:75`](../../docs/setup/story-0-5-mcp-tooling-configuration-tutorial.md#L75)

- Meme tableau pour Blender MCP.
  [`story-0-5-mcp-tooling-configuration-tutorial.md:166`](../../docs/setup/story-0-5-mcp-tooling-configuration-tutorial.md#L166)

**Smoke tests et preuves honnetes**

- Smoke test Unity MCP sans danger et son cas limite (MCP qui deborde du perimetre).
  [`story-0-5-mcp-tooling-configuration-tutorial.md:85`](../../docs/setup/story-0-5-mcp-tooling-configuration-tutorial.md#L85)

- Smoke test Blender MCP sans danger, mesh traite comme brouillon avant Story 0.6.
  [`story-0-5-mcp-tooling-configuration-tutorial.md:174`](../../docs/setup/story-0-5-mcp-tooling-configuration-tutorial.md#L174)

- `VAL-020` a `VAL-024` : verifier qu'aucune ligne n'est faussement `Pass` et que `Blocked` reste une issue valide (ex. VAL-022 si Blender indisponible).
  [`tooling-validation-log.md:49`](../../docs/setup/tooling-validation-log.md#L49)

**Peripheriques : suivi et statut**

- Ligne Story 0.5 action manuelle, synchronisee avec le tutoriel.
  [`epic-0-readiness-checklist.md:50`](../../docs/setup/epic-0-readiness-checklist.md#L50)

- Ligne Story 0.5 validation agent et Gate Epic 1 mis a jour.
  [`epic-0-readiness-checklist.md:66`](../../docs/setup/epic-0-readiness-checklist.md#L66)

- Statut sprint Story 0.5 avance a `review` (travail agent-executable termine, preuves utilisateur restent a fournir).
  [`sprint-status.yaml:43`](sprint-status.yaml#L43)

