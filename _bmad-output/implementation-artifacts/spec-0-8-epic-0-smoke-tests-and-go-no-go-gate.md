---
title: 'Story 0.8 : Smoke tests finaux Epic 0 et decision go/no-go'
type: 'chore'
created: '2026-09-07'
status: 'done'
review_loop_iteration: 0
baseline_commit: 'eec20b94602435798845beed5a1ecd939b6361a6'
context:
  - '{project-root}/_bmad-output/implementation-artifacts/epic-0-context.md'
  - '{project-root}/docs/setup/addon-adoption-register.md'
  - '{project-root}/docs/setup/tooling-validation-log.md'
  - '{project-root}/docs/setup/epic-0-readiness-checklist.md'
---

<frozen-after-approval reason="human-owned intent - do not modify unless human renegotiates">

## Intention

**Probleme :** VAL-027 a VAL-033 (`tooling-validation-log.md:56-62`) sont tous `Not Started` et aucune scene ne contient de `NetworkManager` : le gate final Epic 0 (AC `epics.md:423-438`) exige pourtant un smoke test Multiplayer Play Mode local, un smoke test Steam distant a deux joueurs, le rejet du cinquieme joueur, le host quit, le disconnect non-host, les erreurs Lobby/UI visibles, puis une decision finale `Pass`/`Blocked`/`Accepted With Known Blockers`. Story 0.3 reste elle-meme `In Progress` (VAL-013 a VAL-015, VAL-032).

**Approche :** Ajouter un harnais NetworkManager minimal (sans gameplay) a `Dev_LobbySmokeTest.unity` avec deux transports commutables (Unity Transport pour le test local, Facepunch pour Steam), guider l'utilisateur en direct pour executer VAL-027 (local) et VAL-028 (Steam distant, si un second joueur reel est disponible maintenant), puis documenter VAL-029 a VAL-032 comme `Blocked` differes vers les stories Epic 2 nommees (aucune UI/session lobby n'existe encore) et emettre la decision finale VAL-033 en consequence dans les trois documents de suivi plus `sprint-status.yaml`.

## Limites & Contraintes

**Toujours :** Utiliser exclusivement les tokens de statut controles (`Not Started`, `In Progress`, `Pass`, `Blocked`, `Not Applicable`) ; caviarder tout Lobby ID/token/invite Steam avec `[REDACTED_TOKEN]` ; ne jamais passer VAL-027/VAL-028 a `Pass` sans preuve live temoignee par l'agent (capture + verification directe, meme discipline que VAL-020 a VAL-025) ; garder le harnais NetworkManager sans aucun hook gameplay (aucune reference a `NetworkedRunState`/`RunCompositionRoot`, meme precedent que VAL-019) ; pour VAL-029/VAL-030/VAL-031/VAL-032, si le code lobby/session/UI necessaire n'existe pas, les marquer `Blocked` avec reference explicite a la story Epic 2 concernee -- jamais `Pass` ni "preuve fournie" sans preuve reelle ; inclure `Dev_LobbySmokeTest.unity` desactivee dans les Build Settings (comme les autres scenes `Dev_*`).

**Demander d'abord :** Avant de tenter VAL-028, confirmer si un second joueur/machine Steam reel est disponible maintenant -- sinon marquer `Blocked`/differe sans le tenter ; le choix final du statut de gate VAL-033 (`Pass` vs `Accepted With Known Blockers` vs `Blocked`) une fois toutes les preuves reunies.

**Jamais :** Construire la lobby UI, la gestion de session (cap 4 joueurs, host quit, disconnect) ou toute feature gameplay -- perimetre Epic 2 ; modifier les lignes ADDON-00x ou les lignes Story 0.1-0.7 des trois documents au-dela de la synchronisation strictement necessaire a 0.8 ; inventer une preuve utilisateur qui n'existe pas.

## Matrice I/O & Cas Limites

| Scenario | Entree / Etat | Sortie / Comportement attendu | Gestion d'erreur |
|----------|---------------|-------------------------------|-------------------|
| VAL-027 local | `NetworkManager` + Unity Transport dans `Dev_LobbySmokeTest`, Multiplayer Play Mode avec 1 joueur virtuel + host | Host et client se connectent, capture + verification agent, VAL-027 `Pass` | Si la connexion echoue, VAL-027 `Blocked` avec message d'erreur consigne |
| VAL-028 distant, second joueur disponible | `NetworkManager` + Facepunch transport, host + client Steam reel distant | Deux clients Steam connectes via relay, preuve caviardee, VAL-028 `Pass` | Si aucun second joueur n'est disponible, VAL-028 `Blocked`/differe, jamais `Pass` |
| VAL-029/030/031/032, code lobby absent | Aucune gestion de cap/session/UI d'erreurs en runtime | Marquees `Blocked`, reference story Epic 2 nommee (2.2/2.3/2.4/2.7) | N/A |
| Decision finale | Toutes les lignes VAL-027 a VAL-032 resolues (`Pass` ou `Blocked` documente) | VAL-033 et gate Epic 0 = `Pass`, `Blocked` ou `Accepted With Known Blockers`, coherent entre les 3 documents | Si un bloqueur non accepte subsiste, gate reste `Blocked`, Epic 1 non debloquee |

</frozen-after-approval>

## Code Map

- `docs/setup/tooling-validation-log.md:56-62` -- Lignes VAL-027 a VAL-033 a faire progresser.
- `docs/setup/epic-0-readiness-checklist.md:53` (action manuelle) et `:69` (validation agent) -- Lignes Story 0.8 a synchroniser.
- `docs/setup/epic-0-readiness-checklist.md:71-75` -- Section "Gate go/no-go Epic 1" a completer avec la decision finale et, si besoin, la liste de bloqueurs acceptes.
- `docs/setup/epic-0-readiness-checklist.md:24,48` -- Ligne "Gate Epic 1" (baseline) et ligne Story 0.3 -- refletent l'etat `In Progress` de VAL-013/014/015/032 a accepter comme bloqueur documente.
- `Assets/RoadRage/App/Scenes/Dev_LobbySmokeTest.unity` -- Scene stub vide (11413 octets, identique au template par defaut) : cible pour le `NetworkManager` minimal, doit rester desactivee dans `ProjectSettings/EditorBuildSettings.asset`.
- `Assets/Editor/RoadRageSteamworksSmokeTest.cs` -- Patron existant `SteamClient.Init`/`Shutdown` (AppID `480`) a reprendre pour un nouvel outil editor de smoke test VAL-027/VAL-028.
- `Packages/com.community.netcode.transport.facepunch/Runtime/FacepunchTransport.cs` -- Transport Steamworks deja embarque/patche (`VAL-007`) a assigner au `NetworkManager`.
- `docs/setup/story-0-8-epic-0-smoke-tests-tutorial.md` (nouveau) -- Tutoriel pas-a-pas pour VAL-027/VAL-028, meme format que `story-0-5-mcp-tooling-configuration-tutorial.md`/`story-0-6-blender-asset-intake-tutorial.md`.
- `_bmad-output/planning-artifacts/epics.md:423-438` -- AC source Story 0.8.
- `_bmad-output/implementation-artifacts/sprint-status.yaml:38,45,46` -- `epic-0`, `0-7-...` (deja `done` localement, non commite) et `0-8-...` a faire progresser.

## Tasks & Acceptance

**Execution :**
- [x] `Assets/RoadRage/App/Scenes/Dev_LobbySmokeTest.unity` -- Ajouter un `GameObject` `NetworkManager` (composant `NetworkManager` + `UnityTransport` et `FacepunchTransport` commutables, `NetworkConfig` par defaut) sans aucun hook gameplay -- harnais minimal reutilisable pour VAL-027/VAL-028.
- [x] `docs/setup/story-0-8-epic-0-smoke-tests-tutorial.md` -- Creer le tutoriel pas-a-pas (lancement Multiplayer Play Mode local, puis smoke test Steam distant si un second joueur est confirme disponible) -- meme discipline de preuve que les tutoriels Story 0.5/0.6.
- [x] `docs/setup/tooling-validation-log.md` -- Faire progresser VAL-027 (`Pass` avec preuve live), VAL-028 (`Pass` si second joueur reel confirme et teste, sinon `Blocked`/differe), VAL-029/VAL-030/VAL-031 (`Blocked`, reference Story 2.2/2.3/2.4/2.7 selon le cas), VAL-032 (`Blocked`/differe, reference Story 2.2/2.3/2.4/2.7 -- coherent avec la note deja posee par Story 0.3), VAL-033 (decision finale), plus une entree datee dans "Notes de validation" par ligne modifiee.
- [x] `docs/setup/epic-0-readiness-checklist.md` -- Synchroniser les lignes Story 0.8 (`:53`, `:69`) et la section "Gate go/no-go Epic 1" (`:71-75`) avec le statut reel de VAL-027 a VAL-033, sans "preuve fournie" sur une ligne non-`Pass`.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- Confirmer `0-7-unity-add-on-ui-library-and-asset-adoption-register: done`, faire avancer `0-8-epic-0-smoke-tests-and-go-no-go-gate` (`backlog` -> `in-progress` -> `review`), et mettre a jour `epic-0` si la decision finale le justifie, en conservant le format `last_updated` `YYYY-MM-DD HH:MM`.

**Criteres d'acceptation :**
- Given le `NetworkManager` de `Dev_LobbySmokeTest.unity` est inspecte, when la scene est ouverte, then aucun composant gameplay (`NetworkedRunState`, `RunCompositionRoot`, etc.) n'y est reference et la scene reste desactivee dans les Build Settings.
- Given VAL-027 a VAL-033 sont inspectees apres implementation, when leur statut est lu, then chaque ligne est `Pass` avec preuve live ou `Blocked` avec bloqueur et reference Epic 2 explicites -- jamais `Pass` sans preuve reelle.
- Given le gate go/no-go Epic 1 est inspecte, when sa decision finale est lue, then elle est `Pass`, `Blocked` ou `Accepted With Known Blockers`, coherente avec VAL-033 et avec les trois documents de suivi.

## Spec Change Log

- 2026-09-07 : Preuve utilisateur recue pour VAL-027 (`RoadRageNetcodeSmokeTest` avec `ConnectedClientsCount=2`) et confirmation humaine de cloturer Epic 0 avec VAL-028 plus tard et les bloqueurs connus acceptes. Synchronisation effectuee : VAL-027 `Pass`, VAL-033 `Pass` avec decision `Accepted With Known Blockers`, `epic-0` et Story 0.8 en `done` dans `sprint-status.yaml`.
- 2026-09-07 : Revue BMAD step-04 explicitement non requise par l'utilisateur pour cette cloture documentaire ; spec cloturee directement en `done` apres verification des assertions.

## Design Notes

Le harnais `NetworkManager` porte deux composants transport (Unity Transport et Facepunch) mais un seul est actif a la fois via le champ `NetworkConfig.NetworkTransport` -- bascule manuelle avant chaque smoke test, jamais les deux simultanement. Cela reutilise les deux transports deja verrouilles (VAL-008, VAL-007) sans introduire de nouveau package ni de code de lobby/session : c'est un point de preuve, pas une fondation Epic 2.

## Verification

**Commandes :**
- `$log = Get-Content -LiteralPath 'docs/setup/tooling-validation-log.md' -Raw; foreach ($id in 'VAL-027','VAL-028','VAL-029','VAL-030','VAL-031','VAL-032','VAL-033') { $row = ($log -split "`r?`n") | Where-Object { $_ -like "| $id |*" }; if (-not $row -or $row -match 'A renseigner') { throw "$id encore non renseigne" } }` -- attendu : aucune erreur.
- `$chk = Get-Content -LiteralPath 'docs/setup/epic-0-readiness-checklist.md' -Raw; if ($chk -match "\|\s*0\.8[^|]*\|\s*`(Not Started|In Progress|Blocked)`\s*\|[^\n]*preuve fournie") { throw "ligne 0.8 ne doit pas dire preuve fournie hors Pass" }` -- attendu : aucune erreur.
- `$sprint = Get-Content -LiteralPath '_bmad-output/implementation-artifacts/sprint-status.yaml' -Raw; if ($sprint -notmatch '0-8-epic-0-smoke-tests-and-go-no-go-gate:\s*(review|done)') { throw "Story 0.8 non avancee dans sprint-status.yaml" }` -- attendu : aucune erreur.

**Manual checks (if no CLI) :**
- Ouvrir `Dev_LobbySmokeTest.unity` dans Unity et confirmer visuellement l'absence de tout composant gameplay sur le `NetworkManager`.
