---
title: "Story 4.6 : Gel du profil, payload de session et spawn du personnage selectionne"
type: "feature"
created: "2026-09-13"
status: "in-review"
review_loop_iteration: 0
context: []
baseline_commit: "03419f428e3ebdb378b412894ccb6539732131e4"
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Le choix de personnage n'est pas gele : le payload est lu en direct puis encode une fois au Start Game (`LobbyFlowController.cs:412,421`), donc la selection reste mutable entre l'entree du lobby et le depart, et elle est meme re-publiee au lobby (`:612` vers `:628`) — contraire a AD-29 et a « le lobby ne republie pas la selection ». Le spawn solo lit aussi le profil en direct (`RunFlowController.cs:203`). Et un gel du seul depot ne suffit pas : `MainMenuProfileFlowController` ecrit le fichier meme quand `Set` refuserait (`:155`, `:194`), donc une mutation refusee atteint quand meme le disque.

**Approach:** Le depot devient explicitement gelable : a l'entree du lobby (Play en solo, room `Open` ou join `Joined` en multi) il capture un instantane et refuse toute mutation ; `SessionSelection` devient la seule lecture de la selection pour la session (payload, spawn solo), et la persistance disque est gardee par le meme gel. Le gel est leve quand le menu principal se (re)ouvre, sinon le menu resterait en lecture seule.

## Boundaries & Constraints

**Always:** `PlayerProfileStore` reste le seul depot et `Set` sa seule porte de mutation ; sous gel, `Set` ne mute rien et ne leve pas `ProfileChanged`. `SessionSelection` est la seule lecture de la selection en aval du menu. Le gel est un instantane pris a l'entree du lobby, jamais une lecture live, et il est idempotent (Play puis room `Open`). Le menu principal reste la seule surface de selection et le gel est leve a sa (re)ouverture. Toute ecriture disque verifie le gel avant d'ecrire. Tout refus est visible, jamais seulement en log.

**Ask First:** Format du payload ou `NetworkPlayerConnectionPayload`. Reference d'asmdef nouvelle. `ISteamPlatform`, `CharacterCatalog`, `CharacterDef`, `NetworkedPlayerState.CharacterId` ou chaine d'approbation host. Champ persistant au-dela des trois existants. Remplacer le repli silencieux `catalog.GetAt(0)` par un refus.

**Never:** Persister un etat de session ou de runtime (siege, vie, monnaie, lobby, run). Ajouter un second conteneur de selection gelee a cote de `NetworkPlayerRegistry`, deja l'instantane par client cote host. Lire `Current` en aval du menu. Rejouer les revues 4.1-4.4 ou reecrire la chaine reseau.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Behavior | Error Handling |
| -------------------------- | --------------------------------------------------- | -------------------------------------------------------------------------------- | --------------------------------------- |
| Entree lobby solo | clic Play, profil resolu | depot gele, `SessionSelection` = profil affiche, plus aucune mutation acceptee | N/A |
| Entree lobby multi | room `Open` (hote) ou join `Joined` (invite) | gel avant la publication au roster, publication unique | N/A |
| Mutation sous gel | clic Rookie/Veteran, depot gele | aucun `Set`, aucun `ProfileChanged`, aucune ecriture disque | avertissement + notice visible |
| Ecriture disque sous gel | `TryPersist` avec depot gele | refusee, aucun fichier ecrit | notice visible |
| Spawn solo | `SessionSelection` = `char_veteran` | `MVP_Run` spawne `LocalPlayer_char_veteran` et son `PreviewPrefab` | refus visible si personnage manquant |
| Payload multi | gel actif | `ConnectionData` encode `SessionSelection`, jamais `Current` | refus visible si `SessionSelection` absent |
| Retour au menu | joueur quitte le lobby et rouvre le menu | gel leve, selection de nouveau modifiable, profil restaure | notice Steam en cours conservee |
| Depot herite gele | `MainMenuLobby` recharge, bootstrap persistant encore gele | gel leve a la resolution du menu avant tout `Set` | N/A |

</frozen-after-approval>

## Code Map

- `Features/Players/PlayerProfileStore.cs:26` -- `Set` seul mutateur, `ProfileChanged:20`, commentaire interdisant un `Clear` silencieux. Point unique du gel. `Features/Players/PlayerProfile.cs` -- instantane a capturer (nom + `CharacterId`).
- `App/MainMenu/MainMenuFlowController.cs:28` -- entree de la coquille lobby solo (Play) ; `ReturnToMenu:70` gere deja retour + rejeu de notice.
- `App/MainMenu/MainMenuProfileFlowController.cs:113` `ResolveProfileAtMenuOpen` -- point de degel (le bootstrap persistant survit aux scenes) ; ecritures a garder : `:151`/`:155` (resolution initiale), `:193`/`:194` (clic option).
- `App/Lobby/LobbyFlowController.cs:728` (`case Open`), `:796` (`case Joined`) -- entrees du lobby multi, a geler avant `PublishLocalProfile()` (`:752`/`:820`). `:412` lecture live + `:421` encodage payload (a basculer sur `SessionSelection`) ; `:475`/`:488` approbation host + `NetworkPlayers.Register` ; `:625` `HandleProfileChanged`, republication rendue impossible par le gel.
- `App/Run/RunFlowController.cs:203` lecture live du spawn solo (a basculer) ; `:220` nom `LocalPlayer_<RawId>` ; `:232` instanciation du `PreviewPrefab`.
- `Features/Players/NetworkPlayerRegistry.cs:28` -- instantane gele par client cote host, a reutiliser tel quel. `App/Run/NetworkedPlayerSpawnService.cs:120,173` resolution host vers `CharacterId`, `:129` repli silencieux `GetAt(0)` (differe). `Features/Players/NetworkedPlayerPresentation.cs:175,219` presentation depuis la `NetworkVariable`.
- `Features/Players/PersistentPlayerProfileRecord.cs:17,19,21` + `PlayerProfileFileStore.cs:112,134` -- whitelist persistee a trois champs, `:134` seule ecriture fichier.
- Idiome de test a copier : `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests.cs:274` (whitelist), `:386`/`:409` (gardes sur le texte source), `Story25NetworkedPlayerSpawnTests.cs:34,67,97` (payload + registre + `EnsureNetworkManager`), `Story28Epic2OnlinePlayableCheckpointTests.cs:66,133` (approbation par reflexion, fakes Steam).
- PlayMode : redirection du chemin profil a poser avant tout chargement de scene, motif deja en place dans `Story45...PlayModeTests.cs:36-62,170-176` et `Story16Epic1PlayableCheckpointPlayModeTests.cs:33-56,62-65`, sur le seam `PlayerProfileFileStore.cs:35` (lu une fois par `RoadRageBootstrap.cs:114`). L'extraction d'une aide partagee et la correction des cinq fixtures non redirigees sont differees (`deferred-work.md`).
- Contraintes : `RoadRage.App.asmdef` reference deja `Features.Players`/`Features.Online` ; `Features.UI` n'y a pas droit ; EditMode et PlayMode ne se referencent pas. `Unity.Netcode.TestHelpers.Runtime` absent, d'ou la verification host/client via le seam existant.

## Tasks & Acceptance

**Execution:**

- [x] `Assets/RoadRage/Features/Players/PlayerProfileStore.cs` -- gel (`Freeze`/`Unfreeze`/`IsFrozen`), instantane `SessionSelection` capture a l'entree, `Set` refuse sous gel -- point unique qui rend le choix immuable sans second conteneur.
- [x] `Assets/RoadRage/App/MainMenu/MainMenuFlowController.cs` -- geler a l'entree de la coquille lobby, degeler dans `ReturnToMenu` -- ouvre et ferme la fenetre de selection en solo.
- [x] `Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs` -- degeler en tete de `ResolveProfileAtMenuOpen`, refuser explicitement les deux ecritures sous gel -- sans quoi une mutation refusee atteindrait le disque.
- [x] `Assets/RoadRage/App/Lobby/LobbyFlowController.cs` -- geler sur `Open` et `Joined` avant toute publication au roster, lire `SessionSelection` pour le payload -- supprime la re-publication et la lecture live.
- [x] `Assets/RoadRage/App/Run/RunFlowController.cs` -- lire `SessionSelection` au spawn solo -- le solo consomme la meme selection gelee que le multi.
- [x] `Assets/RoadRage/Tests/EditMode/Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnTests.cs` -- couvrir la matrice (gel, refus de mutation, idempotence, depot herite, refus d'ecriture, payload, whitelist persistee) plus une garde de source « l'etat de session n'ecrit jamais le profil » sur `RunFlowController`, `NetworkedPlayerSpawnService`, `NetworkedPlayerPresentation`, `NetworkedPlayerState`.
- [x] `Assets/RoadRage/Tests/PlayMode/Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests.cs` -- rediriger le chemin profil avant chargement, puis menu reel : choisir un personnage non defaut, Play, Start Game, verifier que `MVP_Run` spawne ce personnage et qu'aucune mutation apres le gel ne le change -- seule preuve bout en bout cote solo qui manque aujourd'hui.

**Acceptance Criteria:**

- Given un profil persistant et une selection faite dans le menu, when le joueur entre dans le lobby (Play, room `Open` ou join `Joined`), then le depot est gele et `SessionSelection` porte la selection affichee, sans controle permettant de la modifier ensuite.
- Given le depot gele, when un changement de personnage est demande, then depot, `ProfileChanged` et fichier restent inchanges, et le refus est visible.
- Given le depot gele, when une ecriture de profil est tentee, then elle est refusee et aucun fichier n'est ecrit.
- Given `char_veteran` gele a l'entree du lobby, when le joueur lance une partie solo, then `MVP_Run` spawne ce personnage avec sa presentation, sans lire le profil courant.
- Given une selection gelee, when la session reseau demarre, then `ConnectionData` transporte cette selection et la resolution host enregistre le meme personnage pour le client.
- Given un depot gele par une session precedente, when le menu se rouvre, then le gel est leve, le profil enregistre est restaure et la selection redevient modifiable.
- Given n'importe quel chemin de session, when il est inspecte, then aucun etat de session n'atteint le profil persistant, dont les trois champs restent la seule surface persistee.

## Spec Change Log

## Design Notes

Le gel vit dans `PlayerProfileStore` : c'est deja le seul depot, `Set` y est deja la seule porte, et un second conteneur a cote de `NetworkPlayerRegistry` dupliquerait une verite. `SessionSelection` retourne l'instantane une fois gele, sinon le profil courant : un seul point de lecture, plus de lecture live au Start Game ni au spawn. Le refus de mutation ne protege pas le disque, puisque `MainMenuProfileFlowController` enchaine `Set` puis `TryPersist` ; les deux appels sont donc gardes explicitement. Le degel appartient au menu parce que le bootstrap est `DontDestroyOnLoad`.

## Verification

**Commands (lancables par l'humain, conformement au checkpoint de verification du workflow) :**

- Recompilation de l'Editor -- attendu : `failed=false`, `errors=[]`.
- Suite EditMode `RoadRage.Tests.EditMode` -- attendu : tout vert, hors echecs preexistants connus (violation d'asmdef Story 4.4, flake camera).
- Suite PlayMode `RoadRage.Tests.PlayMode` -- attendu : tout vert, nouvelle fixture comprise.

**Resultats rapportes le 2026-09-13 (run humain, suite PlayMode complete) :**

- Recompilation Editor : non rapportee.
- EditMode : vert, hors les 2 echecs preexistants deja differes. Aucun rouge attribuable a cette story.
- PlayMode : **3 fixtures rouges, cause racine non identifiee** -- `Story11MainMenuLaunchPlayModeTests` (1 test sur 3), `Story16Epic1PlayableCheckpointPlayModeTests` (1 sur 1), `Story44PassengerActionTwoMvpRunPlayModeTests` (1 sur 2). Les trois messages recus sont l'enveloppe parent generique `One or more child tests had errors` ; l'erreur enfant, le type d'echec et l'ordre d'execution des fixtures n'ont pas ete captures, et la demande d'aller les chercher dans le Test Runner a ete refusee par l'humain.

**Etat de verification : INCOMPLET.** Les criteres d'acceptation metier sont implantes et couverts par les fixtures, mais leur satisfaction n'est pas prouvee de bout en bout : on ne sait pas non plus si `Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests` a effectivement tourne, la remontee ne listant que trois fixtures rouges. L'audit de matrice du workflow traite un test qui n'a pas tourne comme manquant, et deux lignes de la matrice (ecriture disque refusee sous gel, notice de refus visible) ne sont couvertes que par ce fichier. Trace complete et plan de triage : `handoff-2026-09-13-story-4-6-playmode-regressions.md` ; dette consignee dans `deferred-work.md`.

**Manual checks (non couverts par les tests) :** Play Mode sur `MainMenuLobby` : Veteran, Play, Back -- la selection redevient modifiable et le profil affiche est le bon. Puis solo avec Veteran : verifier visuellement le modele spawne dans `MVP_Run`.

## Suggested Review Order

**Le gel, point unique**

- Le coeur de la story : capturer l'instantane a l'entree du lobby, en un seul endroit.
  [`PlayerProfileStore.cs:48`](../../Assets/RoadRage/Features/Players/PlayerProfileStore.cs#L48)

- La seule porte de mutation, qui refuse desormais sous gel sans lever `ProfileChanged`.
  [`PlayerProfileStore.cs:75`](../../Assets/RoadRage/Features/Players/PlayerProfileStore.cs#L75)

- Lecture unique de la selection : instantane sous gel, profil courant sinon.
  [`PlayerProfileStore.cs:35`](../../Assets/RoadRage/Features/Players/PlayerProfileStore.cs#L35)

- Le degel libère l'instantane ; sans lui le menu resterait en lecture seule.
  [`PlayerProfileStore.cs:63`](../../Assets/RoadRage/Features/Players/PlayerProfileStore.cs#L63)

**Entree du lobby**

- Entree de la coquille lobby cote solo : le clic Play devient le point d'ancrage du gel.
  [`MainMenuFlowController.cs:85`](../../Assets/RoadRage/App/MainMenu/MainMenuFlowController.cs#L85)

- Entree hote : gel avant toute publication au roster, donc plus de re-publication possible.
  [`LobbyFlowController.cs:747`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L747)

- Entree invite : meme gel sur join reussi, idempotent avec le gel de Play.
  [`LobbyFlowController.cs:819`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L819)

**Degel et garde de persistance**

- Le degel appartient au menu, seule surface de selection et seul point de retour.
  [`MainMenuProfileFlowController.cs:126`](../../Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs#L126)

- Ecriture initiale gardee : un `Set` refuse ne doit pas laisser passer le `TryPersist` qui suit.
  [`MainMenuProfileFlowController.cs:161`](../../Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs#L161)

- Meme garde au clic d'emplacement : c'est le trou que la story existe pour fermer.
  [`MainMenuProfileFlowController.cs:209`](../../Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs#L209)

- Le refus est visible au joueur, jamais seulement journalise.
  [`MainMenuProfileFlowController.cs:229`](../../Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs#L229)

- Retour au menu : degel puis rejeu de la derniere notice.
  [`MainMenuFlowController.cs:96`](../../Assets/RoadRage/App/MainMenu/MainMenuFlowController.cs#L96)

**Lecture unique de la selection en aval du menu**

- Le payload de session transporte la selection gelee, plus une lecture live.
  [`LobbyFlowController.cs:412`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L412)

- La publication au roster lit la meme source, sans conteneur parallele.
  [`LobbyFlowController.cs:641`](../../Assets/RoadRage/App/Lobby/LobbyFlowController.cs#L641)

- Le spawn solo consomme exactement la meme selection que le multi.
  [`RunFlowController.cs:205`](../../Assets/RoadRage/App/Run/RunFlowController.cs#L205)

**Tests**

- Contrat du gel : refus sans mutation ni evenement.
  [`Story46…Tests.cs:48`](../../Assets/RoadRage/Tests/EditMode/Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnTests.cs#L48)

- Depot herite gele : le cas qui rendrait le menu definitivement en lecture seule.
  [`Story46…Tests.cs:137`](../../Assets/RoadRage/Tests/EditMode/Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnTests.cs#L137)

- Les deux ecritures gardees et ordonnees : `Set` refuse avant tout `TryPersist`.
  [`Story46…Tests.cs:178`](../../Assets/RoadRage/Tests/EditMode/Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnTests.cs#L178)

- Garde anti-fuite : aucun chemin de session n'ecrit le profil ni ne contourne le depot.
  [`Story46…Tests.cs:267`](../../Assets/RoadRage/Tests/EditMode/Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnTests.cs#L267)

- Seule preuve bout en bout : menu reel, choix non defaut, spawn, refus, retour menu.
  [`Story46…PlayModeTests.cs:65`](../../Assets/RoadRage/Tests/PlayMode/Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests.cs#L65)

- Gel par room ouverte isole du gel de Play ; `Assert.Inconclusive` sans Steam, comme Story22.
  [`Story46…PlayModeTests.cs:174`](../../Assets/RoadRage/Tests/PlayMode/Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests.cs#L174)

- Garde Story 1.3 elargie : `Freeze`/`Unfreeze` sont des portes de session, pas des mutations.
  [`Story13CharacterSetupTests.cs:411`](../../Assets/RoadRage/Tests/EditMode/Story13CharacterSetupTests.cs#L411)

- Story 1.5 adaptee : le profil est desormais pose avant le clic Play, jamais apres le gel.
  [`Story15…PlayModeTests.cs:117`](../../Assets/RoadRage/Tests/PlayMode/Story15EmptyMapEntryPlayModeTests.cs#L117)
