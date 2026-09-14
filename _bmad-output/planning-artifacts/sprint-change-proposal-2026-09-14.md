# Proposition de correction de trajectoire — 2026-09-14

> **Approuvée par Kenan le 2026-09-14.** AD-26 clarifiée, `epics.md` et `sprint-status.yaml` mis à jour (Story 5.3 insérée, 5.3-5.6 renumérotées en 5.4-5.7). Implémentation de la Story 5.3 à suivre via `bmad-build`.

## 1. Résumé du problème

La Story 5.2 a révélé que le trafic IA ne roule qu'en lobby en ligne : en lobby local (« Start Game » depuis le menu), aucune session Netcode n'est jamais démarrée, donc tout `NetworkBehaviour` reste inerte. Ce n'est pas un défaut isolé de la 5.2 — c'est une divergence structurelle entre les deux chemins de démarrage qui existe depuis l'Epic 1 et que chaque story suivante a payée en écrivant un second chemin d'exécution à la main.

**Preuve concrète :**

- En ligne : `LobbyFlowController.cs:430` appelle `manager.StartHost()`, puis charge `MVP_Run` via `manager.SceneManager.LoadScene(...)` (:445). Une session Netcode existe, les `NetworkObject` de scène sont *spawned*, `IsServer == true` côté hôte.
- En solo : `AppSceneRouter.LoadMvpRun()` (`AppSceneRouter.cs:24`) fait un `SceneManager.LoadScene("MVP_Run")` nu. `StartHost()` n'apparaît nulle part ailleurs dans le code de production. Aucun `NetworkObject` n'est jamais spawné ; `OnNetworkSpawn` ne se déclenche pas ; `FixedUpdate` d'un `NetworkBehaviour` sort immédiatement sur `!IsServer`.

**Ampleur de la duplication déjà accumulée :** 45 occurrences de `LocalSolo*` dans le code de production, concentrées dans `RunFlowController` (`TryEnterLocalSoloVehicle`, `ExitLocalSoloVehicle`, `SynchronizeLocalSoloSeatedPose`, `RestoreLocalSoloOnFootControl`, `HandleLocalSoloVehicleInteraction`, `HandleLocalSoloVehicleRecoveryInteraction`, `HandleLocalSoloSeatSwitchInteraction`, `RefreshLocalSoloDeathRecovery`…) : un jumeau non réseauté de chaque comportement réseauté, story après story.

**Découvert via :** Story 5.2 (`spec-5-2-basic-ai-route-following-and-recovery.md`), remontée humaine du 2026-09-14, tracée dans `deferred-work.md`.

**Cadrage retenu avec l'humain (2026-09-14) :**

- Plus de mode hors-ligne du tout. « Start Game » démarre lui aussi un lobby privé Steam avec host à un seul joueur — même mécanisme que « Create Lobby » (`StartHost()` + lobby rejoignable par code, rejoignable en cours de partie).
- « Create Lobby » reste un bouton distinct (montre l'écran de lobby/code avant de lancer), mais les deux boutons convergent vers le même chemin technique en dessous.

## 2. Ce que dit déjà l'architecture

Ce n'est pas une nouvelle décision d'architecture : c'est une non-conformité à une règle déjà adoptée.

- **AD-26 — Session Lifecycle And Run Composition** [ADOPTED] : « Bootstrap owns persistent services, Steamworks SDK initialization and Steam login state, and NetworkManager lifetime. **MainMenuLobby creates or joins the private Steam lobby, then the host starts networking before synchronized load into MVP_Run.** » Cette règle ne prévoit aucun chemin de secours sans lobby/session — le chemin solo actuel la contourne.
- **AD-3 — Host Owns Shared Runtime State**, **AD-18 — Netcode Ownership And Intent Pipeline**, **AD-21 — Host-Simulated Vehicle Movement** : tous supposent qu'un hôte Netcode existe. Aucun AD ne documente un mode d'exécution parallèle sans réseau — les 45 `LocalSolo*` sont une dérive d'implémentation, jamais une exception architecturale approuvée.
- **SPEC.md, Success signal** : « MVP 1 demonstrates persistent pre-lobby cosmetic selection, **consistent solo/multiplayer spawn**, host-authoritative reusable gameplay state… » — l'objectif « spawn cohérent » est déjà écrit et n'est pas atteint.
- **Story 5.6 (checkpoint Epic 5, backlog)** exige déjà : « local and online smoke tests confirm host-authoritative state updates. » Sans cette correction, le smoke test local de la Story 5.6 ne peut pas démontrer d'autorité hôte — il n'y a pas d'hôte.

**Conclusion : aucune nouvelle AD n'est nécessaire.** Une clarification de deux phrases dans AD-26 suffit pour fermer l'ambiguïté ; le reste est un travail d'implémentation qui met le code en conformité avec une règle déjà en vigueur.

## 3. Impact par épopée

- **Epics 0–4 : livraisons historiques, non réécrites.** Conformément au précédent du 2026-09-13 (« Epics 1–3 et Stories 4.1–4.4 restent des livraisons historiques ; elles ne doivent pas être réécrites pour faire semblant d'avoir toujours satisfait les nouvelles exigences »), leurs specs et critères d'acceptation ne changent pas. Mais leur code partagé (`RunFlowController`) change, donc leurs suites PlayMode devront être ré-exécutées (voir section 5) — pas réécrites, seulement revérifiées.
- **Epic 5 (in-progress) : une story insérée, trois renumérotées.** Toutes les stories 5.3 à 5.6 sont encore `backlog` (aucun travail perdu). J'insère la nouvelle story en position 5.3 et décale les suivantes :
  - **5.3 (nouvelle) — Unification de la session de démarrage solo/en ligne**
  - 5.3 → **5.4** Rage-Driven AI Behavior States
  - 5.4 → **5.5** Rage Road Event Trigger
  - 5.5 → **5.6** AI Traffic Networking and Client Presentation
  - 5.6 → **5.7** Epic 5 AI Traffic Playable Checkpoint

  Raison de la position : 5.3 (ex-5.3, rage IA) et la suite ne peuvent être vérifiées en solo tant que le solo n'est pas hébergé ; retarder la correction la ferait payer en dette une seconde fois sur chaque story suivante.
- **Epics 6–7 (backlog, non commencées) : aucun changement de contenu.** Elles héritent simplement d'un chemin de démarrage déjà unifié — net gain, aucune renumérotation nécessaire.

## 4. Chemin retenu

**Option 1 — Ajustement direct (retenue).** Le solo est mis en conformité avec AD-26 : `AppSceneRouter`/le flux « Start Game » démarre un lobby Steam privé à un seul membre et `StartHost()`, exactement comme `LobbyFlowController` le fait déjà pour « Create Lobby », puis charge `MVP_Run` par le SceneManager réseau. Les méthodes `LocalSolo*` de `RunFlowController` sont supprimées une fois que le chemin hôte-authoritaire général les rend redondantes.

- Effort : **Élevé** — pas par complexité de conception (le chemin en ligne existe déjà et sert de référence), mais par surface : ~45 sites d'appel `LocalSolo*` à retirer/fusionner dans `RunFlowController`, plus la revérification de chaque suite PlayMode qui charge `MVP_Run` ou `MainMenuLobby` en solo depuis l'Epic 1.
- Risque : **Moyen** — le chemin en ligne étant déjà éprouvé et host-authoritative, le risque n'est pas dans la logique réseau elle-même mais dans la régression des raccourcis solo (contrôle véhicule immédiat sans latence d'input réseau, respawn, caméra) qui devront redevenir strictement équivalents en passant par le pipeline d'intent réseau.

**Option 2 — Rollback.** Rejetée : rien à annuler, la 5.2 est correcte du point de vue réseau ; le problème est antérieur à elle.

**Option 3 — Révision du MVP.** Rejetée : le SPEC exige déjà un spawn solo/multijoueur cohérent (Success signal) ; réduire cette exigence irait à l'encontre d'un objectif déjà écrit, pas de la refonte de cet objectif.

## 5. Propositions de changement détaillées

### Architecture (AD-26)

**AVANT :**
> MainMenuLobby creates or joins the private Steam lobby, then the host starts networking before synchronized load into MVP_Run.

**APRÈS :**
> MainMenuLobby creates or joins the private Steam lobby, then the host starts networking before synchronized load into MVP_Run. **This applies uniformly to solo and multiplayer starts: "Start Game" and "Create Lobby" are two menu entry points into the same private-lobby-then-host path — a single-member lobby is not a separate offline mode.** There is no scene-load path that bypasses lobby creation and host networking.

**Rationale :** ferme l'ambiguïté qui a permis au chemin solo de contourner AD-26 pendant 5 epics.

### Epics (`epics.md`, section Epic 5)

Insérer avant l'actuelle Story 5.3 :

```markdown
### Story 5.3: Unified Solo and Online Session Start

**Implements:** AD-26, NFR2, NFR4

As a player,
I want Start Game to host a private lobby exactly like Create Lobby,
So that solo and multiplayer runs share one session-bootstrap path and every networked feature works identically in both.

**Acceptance Criteria:**

**Given** the player presses Start Game from the main menu
**When** the run begins
**Then** a private Steam lobby is created with the player as host, exactly as Create Lobby does
**And** networking starts (StartHost) before MVP_Run loads, via the same LobbyFlowController path Create Lobby already uses
**And** a second player can join that lobby by code while the run is in progress
**And** all NetworkBehaviour-driven features (AI traffic, rage, passenger actions, damage) behave identically whether the lobby was opened via Start Game or Create Lobby

**Given** the LocalSolo* duplicate code path in RunFlowController
**When** the unified session start is verified working
**Then** the LocalSolo* methods are removed and replaced by the single host-authoritative path
**And** no new duplicate solo/online branch is introduced elsewhere to compensate

**Given** every PlayMode fixture that loads MainMenuLobby or MVP_Run in solo mode across Epics 1-5
**When** this story is verified
**Then** those fixtures are re-run and pass under the unified path (no fixture rewritten to hide a regression)
```

Renuméroter les stories suivantes (contenu inchangé, seul l'identifiant change) :

| Ancien identifiant | Nouvel identifiant |
| --- | --- |
| Story 5.3: Rage-Driven AI Behavior States | Story 5.4 |
| Story 5.4: Rage Road Event Trigger | Story 5.5 |
| Story 5.5: AI Traffic Networking and Client Presentation | Story 5.6 |
| Story 5.6: Epic 5 AI Traffic Playable Checkpoint | Story 5.7 |

### `sprint-status.yaml`

```yaml
5-2-basic-ai-route-following-and-recovery: review   # inchangé
5-3-unified-solo-and-online-session-start: backlog  # NOUVEAU
5-4-rage-driven-ai-behavior-states: backlog          # etait 5-3
5-5-rage-road-event-trigger: backlog                 # etait 5-4
5-6-ai-traffic-networking-and-client-presentation: backlog  # etait 5-5
5-7-epic-5-ai-traffic-playable-checkpoint: backlog   # etait 5-6
```

### Portée hors-périmètre de cette proposition

- Aucun changement à `SPEC.md`, `mvp-scope.md`, `gameplay-model.md`, `module-composition.md` — aucune capacité (CAP-1..7) ni contrainte n'est modifiée, seule une clarification d'AD.
- Aucun changement UX — les deux boutons `Start Game` / `Create Lobby` restent visibles au même endroit (Story 1.2), seul leur mécanisme partagé change.
- `deferred-work.md` n'est pas modifié rétroactivement (format append-only) ; la nouvelle spec de la Story 5.3 référencera l'entrée existante.

## 6. Impact MVP et plan d'action

Le MVP n'est pas affecté dans sa portée : aucune capacité ajoutée ou retirée. C'est une dette d'implémentation qui menaçait silencieusement de s'aggraver à chaque epic restante (6 et 7 auraient chacune ajouté leur propre paire de chemins).

**Ordre d'exécution :**

1. Amender AD-26 dans `ARCHITECTURE-SPINE.md` (clarification, pas de nouvelle décision).
2. Insérer Story 5.3 et renuméroter 5.3–5.6 → 5.4–5.7 dans `epics.md`.
3. Mettre à jour `sprint-status.yaml` en conséquence.
4. Lancer `bmad-build` sur la nouvelle Story 5.3 (implémentation + revérification des suites PlayMode Epic 1–5).

## 7. Transfert d'implémentation

**Classification de portée : Modérée.** Pas de nouvelle capacité produit, pas de nouvel AD, pas de remaniement stratégique — c'est une réorganisation de backlog (insertion + renumérotation d'une story déjà planifiée dans une epic en cours) suivie d'une implémentation technique dont le chemin de référence (le flux en ligne) existe déjà et fonctionne.

- **Product Owner / Developer (ce workflow + bmad-build)** : appliquer les modifications de `epics.md` et `sprint-status.yaml` ci-dessus, puis lancer `bmad-build` sur la Story 5.3.
- **Critères de succès** : les trois véhicules IA de la Story 5.2 roulent identiquement en lobby local et en ligne ; toutes les suites PlayMode qui chargent `MainMenuLobby`/`MVP_Run` en solo repassent vertes ; `RunFlowController` ne contient plus de méthode `LocalSolo*`.
