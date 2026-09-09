---
title: 'Story 2.6 - Fondation du HUD en jeu'
type: 'feature'
created: '2026-09-09'
status: 'done'
baseline_commit: '1c5364af862c5f7b05dddd339dfe554434048d50'
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Depuis la Story 2.5, les joueurs spawnent en reseau dans `MVP_Run` mais `RunCheckpointHudScreen` reste un placeholder texte sans vie/stamina/nombre de joueurs/statut reseau/argent, alors que l'epic 2 doit se terminer sur un HUD utilisable pour le checkpoint.

**Approach:** Etendre `NetworkedPlayerState` avec des valeurs placeholder host-owned (vie en coeurs, stamina normalisee, argent), puis brancher `RunCheckpointHudScreen` en lecture seule sur ces valeurs et sur l'etat reseau/nombre de joueurs deja exposes par `RunFlowController`/`NetworkManager`, sans creer de nouveau systeme de degats ni d'economie.

## Boundaries & Constraints

**Always:** vie/stamina/argent restent des `NetworkVariable` server-write sur `NetworkedPlayerState` (meme pattern que l'existant) ; le HUD lit l'etat partage sans jamais l'ecrire ; en solo (pas de `NetworkManager` en ecoute), affichage statique sans abonnement reseau.

**Ask First:** introduire un premier `ServerRpc`/`ClientRpc` d'intention joueur (inexistant dans le repo) si une interaction HUD reelle depasse l'affichage.

**Never:** systeme de degats/stamina consommable/economie reelle ; nouveaux sprites de coeur (texte/TMP uniquement) ; modification de `NetworkedPlayerRoot.prefab` ; toucher au flux de spawn/perte d'hote de la Story 2.5.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Session solo locale | `MVP_Run` charge sans `NetworkManager` en ecoute | HUD affiche vie/stamina/argent locaux et "Solo", nombre de joueurs = 1, aucun abonnement reseau | N/A |
| Host avec clients connectes | Session reseau, N clients connectes (1-4) | HUD affiche le statut "Hote", le nombre de joueurs courant, et les valeurs placeholder du joueur local | N/A |
| Client rejoint/quitte en cours de run | `NetworkManager.OnClientConnectedCallback`/`OnClientDisconnectedCallback` declenche | Le compteur de joueurs HUD se met a jour sans reconstruire tout le HUD | Callback ignore si le HUD n'est pas encore lie (spawn pas termine) |
| Valeurs placeholder du joueur local changent | `NetworkedPlayerState` du joueur local notifie `OnValueChanged` sur vie/stamina/argent | Le HUD met a jour uniquement l'affichage concerne | Aucune ecriture HUD vers `NetworkedPlayerState` meme en cas d'erreur d'affichage |

</frozen-after-approval>

## Code Map

- `NetworkedPlayerState.cs` (L17-L47) -- pattern `NetworkVariable` server-write a suivre pour `Hearts`/`MaxHearts`/`StaminaNormalized`/`Money`.
- `RunCheckpointHudScreen.cs` (L21 `FutureHudState`, 3 `TMP_Text`) -- placeholder deja reference par `RunFlowController`, deja documente pour "rage, argent, actions passager".
- `RunFlowController.cs` (L121-124 `ShowRunState`, L177-191 `ResolveLobbyState`) -- point de branchement post-spawn et source du texte statut reseau.
- `NetworkedPlayerSpawnService.cs:73` (`manager.ConnectedClientsIds`) -- source du nombre de joueurs en session.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/Players/NetworkedPlayerState.cs` -- ajouter `Hearts`, `MaxHearts`, `StaminaNormalized`, `Money` en `NetworkVariable` server-write avec valeurs placeholder par defaut au spawn -- donner au HUD une source host-autoritaire sans systeme de gameplay reel.
- [x] `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs` -- ajouter l'affichage coeurs (texte, ex. glyphes repetes), stamina, nombre de joueurs, statut reseau, argent, plus une methode de mise a jour appelable depuis l'exterieur -- fournir le HUD lisible attendu par l'AC, en lecture seule.
- [x] `Assets/RoadRage/App/Run/RunFlowController.cs` -- brancher le HUD sur la `NetworkedPlayerState` locale (OnValueChanged) et sur les callbacks de connexion `NetworkManager` quand un reseau est actif ; gerer explicitement le cas solo sans abonnement -- respecter la contrainte "jamais de mutation directe" et la continuite du flux Story 2.5.
- [x] `Assets/RoadRage/Tests/EditMode/Story26InGameHudTests.cs` -- couvrir conversion vie/coeurs, texte de statut reseau (solo/hote/client), mise a jour du nombre de joueurs sur connexion/deconnexion simulees, et absence de setter public exploitable par le HUD sur `NetworkedPlayerState` -- verrouiller les regressions et l'invariant lecture-seule.

**Acceptance Criteria:**
- Given une session reseau demarree dans `MVP_Run`, when le joueur local spawn avec succes, then le HUD affiche la vie en coeurs, un placeholder de stamina, le nombre de joueurs courant, le statut reseau et une valeur d'argent placeholder.
- Given une partie solo locale sans `NetworkManager` en ecoute, when `MVP_Run` charge, then le HUD affiche un statut solo coherent sans erreur ni abonnement reseau actif.
- Given le nombre de clients connectes change pendant le run, when Netcode notifie une connexion ou deconnexion, then le HUD met a jour uniquement le nombre de joueurs affiche.
- Given le HUD est actif, when on inspecte le code de `RunCheckpointHudScreen.cs` et `RunFlowController.cs`, then aucune ecriture directe vers une `NetworkVariable` de `NetworkedPlayerState` n'est presente (lecture seule stricte).

## Spec Change Log

## Verification

**Commands:**
- Tests EditMode Story 2.6 via Unity MCP -- attendu : compilation OK, tests cibles verts, console sans erreurs Netcode/Facepunch.
- `git diff --check` -- attendu : aucune erreur whitespace.

## Suggested Review Order

**Etat reseau host-owned (source de donnees)**

- Placeholders server-write ajoutes au meme pattern que l'existant (ClientId/WorldPosition).
  [`NetworkedPlayerState.cs:60`](../../Assets/RoadRage/Features/Players/NetworkedPlayerState.cs#L60)

**Affichage HUD en lecture seule**

- Bind initial groupe (vie/stamina/joueurs/argent) sans reconstruire tout le HUD.
  [`RunCheckpointHudScreen.cs:110`](../../Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs#L110)

- Setters individuels : chacun ne touche que son propre label.
  [`RunCheckpointHudScreen.cs:119`](../../Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs#L119)

- Conversion vie -> glyphes coeurs pleins/vides, sans sprite.
  [`RunCheckpointHudScreen.cs:144`](../../Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs#L144)

- Patch post-review : valeurs neutres pendant l'attente/le blocage, pour eviter des lignes vides ou perimees.
  [`RunCheckpointHudScreen.cs:91`](../../Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs#L91)

**Liaison run <-> HUD**

- Point d'entree post-spawn : bind initial puis bascule vers le suivi reseau si actif.
  [`RunFlowController.cs:176`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L176)

- Resolution du joueur local par ClientId + IsSpawned, jusqu'a succes puis abonnement OnValueChanged.
  [`RunFlowController.cs:71`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L71)

- Callbacks connexion/deconnexion : mettent a jour uniquement le nombre de joueurs, ignores si HUD pas encore lie.
  [`RunFlowController.cs:211`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L211)

- Desabonnement systematique pour eviter les callbacks fantomes apres destruction.
  [`RunFlowController.cs:95`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L95)

**Peripheriques**

- Couverture EditMode : glyphes, mises a jour cibleez, invariant lecture-seule, pont de connexion.
  [`Story26InGameHudTests.cs:1`](../../Assets/RoadRage/Tests/EditMode/Story26InGameHudTests.cs#L1)

- Scene `MVP_Run` : 4 labels ajoutes sous `RunCheckpointHud/StatePanel` et cables sur les nouveaux champs serialises ; panneau de fond agrandi (620x142 -> 620x280) pour couvrir les 7 lignes.
  [`MVP_Run.unity`](../../Assets/RoadRage/App/Scenes/MVP_Run.unity)
