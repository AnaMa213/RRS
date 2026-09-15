---
title: "Story 5.7 : reseau du trafic IA et presentation client"
type: "feature"
created: "2026-09-15"
status: "done"
review_loop_iteration: 0
baseline_commit: "bf16f3a4df0dc514e2786cf4fbec6d7af5bf179d"
context:
  ["{project-root}/_bmad-output/implementation-artifacts/epic-5-context.md"]
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Les Stories 5.2 a 5.6 ont pose chaque morceau de l'etat IA en reseau (position par `NetworkTransform`, `Behavior`, `NetworkedRageState`, `RageRoadEvent`), mais rien ne prouve que la presentation client en decoule, et deux ecarts reels subsistent : la permission de **lecture** des NetworkVariables n'est gardee nulle part alors que l'AC « les clients voient » en depend entierement (`RoadRageScaffoldTests.cs:170` ne teste que `WritePerm`), et `NetworkedAIVehicleState.RouteIndex` est repliquee sans jamais etre ecrite ni lue (`NetworkedAIVehicleState.cs:21`, seule occurrence du symbole dans le depot).

**Approach:** Traiter la story comme une story de **preuve et de fermeture d'ecarts**, pas de reconstruction : retirer l'etat replique mort, puis verrouiller le contrat de presentation client par des gardes EditMode (lecture Everyone, aucune ecriture cote vue, rafraichissement present sur tous les pairs, composition des trois vehicules de `MVP_Run`) et une preuve PlayMode hote. Le rattrapage des arrivants tardifs reste structurel et n'est pas re-code.

## Boundaries & Constraints

**Always:** Toute donnee affichee par un pair vient d'une `NetworkVariable` Everyone-read / Server-write ou d'une re-derivation locale de celle-ci ; une vue ne fait que lire. Le rafraichissement de presentation tourne sur **tous** les pairs, sans garde d'autorite (`RageRoadEventFlowController.cs:184` est le patron de reference). Les libelles sont crees a l'execution, jamais cables dans la scene (`RunCheckpointHudScreen.cs:382`, `AIVehicleBehaviorDebugView.cs:118`). Test EditMode : `public sealed class Story57<Titre>Tests`, namespace `RoadRage.Tests.EditMode`, doubles sans Netcode (`new GameObject` + `AddComponent`, jamais `.Spawn()`), `[TearDown]` detruisant les objets crees, gardes textuelles via `CodeOnly(source)` (`Story56RageRoadEventTriggerTests.cs:392`).

**Ask First:** remplacer le retrait de `RouteIndex` par un remplissage (elle devient alors une progression de route affichee) ; tout reglage de `NetworkTransform` (`PositionThreshold`, `RotAngleThreshold`, `UseUnreliableDeltas`, tick rate) ou tout autre arbitrage de budget reseau ; toute nouvelle scène ou modification des Build Settings ; toute modification de `RoadRageScaffoldTests.cs`.

**Never:** Un second chemin de synchronisation. Ajouter des abonnements `OnValueChanged` ou un « rattrapage » dedie pour les arrivants tardifs -- le polling par frame couvre deja l'AC et une seconde voie serait a maintenir. Ajouter une seconde source de verite de la rage ou du comportement. Resolution de confrontation, on-foot, boss, recompense (Epic 6). Controller de trafic, archetypes IA, ville/autoroute, nouvelles valeurs authorées. Retirer les vues de debug du build de production (dette `deferred-work.md`, hors perimetre). Toute ecriture d'etat depuis un client.

## I/O & Edge-Case Matrix

| Scenario                   | Input / State                                                          | Expected Output / Behavior                                                                   | Error Handling                                                     |
| -------------------------- | ---------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| Observation client         | Session active, 3 IA dans `AITraffic`                                   | Position, `Behavior`, rage et etat Rage Road identiques a l'hote, tous rendus en texte        | Aucune valeur locale de secours                                     |
| Ecriture client refusee    | Un pair non-hote ecrit `Behavior.Value` ou `WaypointIndex.Value`         | Permission serveur refuse l'ecriture ; l'etat replique reste celui de l'hote                  | Pas d'exception remontee, pas de desynchronisation                   |
| Arrivant tardif            | Client rejoignant `MVP_Run` apres demarrage                              | Il lit les NetworkVariables courantes ; ses vues se remplissent au premier tick               | HUD en placeholder jusque-la, sans erreur                            |
| Aucune IA en scene         | Scene sans trafic (`Dev_RageSandbox`)                                    | Les vues ne trouvent aucune cible et n'affichent rien                                         | Pas de `NullReferenceException` (`AIVehicleBehaviorDebugView.cs:101`) |
| NetworkVariable sans lecteur | Une NV IA existe mais n'est ni ecrite ni lue                            | Elle est consideree comme du cout mort et retiree                                             | N/A                                                                |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs:21` -- `RouteIndex` repliquee, jamais ecrite ni lue ; `:26` `WaypointIndex`, `:31` `Behavior` (les deux seules utiles) ; garde `[DisallowMultipleComponent]` `:18`.
- `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs:104` -- garde `!IsServer` de la simulation ; `:16` -- commentaire citant `RouteIndex` a corriger apres retrait ; `:281`, `:298-306` -- ecritures host-only et `Teleport` de recuperation.
- `Assets/RoadRage/Features/Rage/NetworkedRageState.cs:15,20,25` -- `Disposition` / `RageValue` / `FearValue`, deja Everyone-read / Server-write.
- `Assets/RoadRage/Features/Run/NetworkedRunState.cs:26,37` -- `RageRoadEvent` + `RageRoadEventTarget`.
- `Assets/RoadRage/Features/Vehicles/AIVehicleBehaviorDebugView.cs:97-116` -- `ComposeText` : unique lecture, aucun `.Value =` ; `:118` label monde cree a l'execution ; `:68-75` nettoyage du marqueur.
- `Assets/RoadRage/Features/Rage/RageStateDebugView.cs:41-54` -- polling Everyone-read de la rage IA ; `:68`, `:90` -- outils d'ecriture deja reserves a l'hote.
- `Assets/RoadRage/App/Run/RageRoadEventFlowController.cs:184` -- `RefreshEventPresentation` appelee hors garde d'autorite (patron « presentation sur tous les pairs ») ; `:121` -- `IsAuthoritative` reserve a la decision, pas a l'affichage.
- `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs:245` -- `ShowRageRoadEventStatus` ; `:327`, `:352`, `:382` -- `EnsureRageLabel` / `EnsureIncidentLabel` / `EnsureRageRoadEventLabel`, aucun `using` Netcode (`:1-5`).
- `Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab:104` (`m_InScenePlaced`), `:112` `NetworkedAIVehicleState`, `:151` `NetworkTransform` (`AuthorityMode: 0` = Server, seuils a 0), `:209` `NetworkedRageState`, `:232` vue de comportement.
- `Assets/RoadRage/App/Scenes/MVP_Run.unity:3060` -- racine `AITraffic` sous `RunRoot` ; `:4847`, `:4982`, `:3803` -- les trois instances IA, chacune overridee en in-scene placed avec sa route.
- `Assets/RoadRage/Tests/EditMode/RoadRageScaffoldTests.cs:85-93` -- liste `RuntimeStateTypes` (contient deja `NetworkedAIVehicleState`) ; `:170` -- garde d'ecriture serveur seule.
- `Assets/RoadRage/Tests/EditMode/Story56RageRoadEventTriggerTests.cs:392` -- helper `CodeOnly` ; `:276` -- lecture reflexive d'un libelle prive ; `:324-350` -- ouverture de scene avec `finally`.
- `Assets/RoadRage/Tests/PlayMode/Story44PassengerActionTwoMvpRunPlayModeTests.cs:41-53` -- patron de recherche d'un libelle cree a l'execution dans `MVP_Run` ; `:24-35` -- `Shutdown()` obligatoire en `[UnityTearDown]`.

## Tasks & Acceptance

**Execution:**

- [x] `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs` -- retirer `RouteIndex` et ajuster le commentaire de classe -- etat replique jamais ecrit ni lu : cout au spawn et seconde source de verite de route alors que la reference de scene `route` la porte deja.
- [x] `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` -- corriger le commentaire qui cite `RouteIndex` -- garder la documentation vraie apres le retrait.
- [x] `Assets/RoadRage/Tests/EditMode/Story57AiTrafficClientPresentationTests.cs` (nouveau) -- gardes du contrat de presentation client -- c'est le seul endroit ou l'AC « les clients voient » est verifiable sans second pair : couvrir la matrice I/O (observation, ecriture refusee, absence d'IA, NV sans lecteur) avec les patrons de `Story56` : (a) sur `NetworkedAIVehicleState`, `NetworkedRageState` et `NetworkedRunState`, chaque `NetworkVariable` publique est `ReadPerm == Everyone` **et** `WritePerm == Server` ; (b) `CodeOnly` prouve que `AIVehicleBehaviorDebugView.cs`, `RageStateDebugView.cs` et `RunCheckpointHudScreen.cs` ne contiennent ni `.Value =` ni `Rpc`/`ClientRpc`/`ServerRpc` ; (c) `RefreshEventPresentation` est appelee hors du bloc `IsAuthoritative` ; (d) `MVP_Run` ouvert puis ferme en `finally` : les trois vehicules de `AITraffic` portent `NetworkedAIVehicleState`, `NetworkedRageState` et `NetworkTransform`, et chaque bloc de prefab-instance porte `m_InScenePlaced: 1` ; (e) aucune `NetworkVariable` d'`NetworkedAIVehicleState` n'est declaree sans consommateur (liste blanche explicite des 2 restantes).
- [x] `Assets/RoadRage/Tests/PlayMode/Story57AiTrafficClientPresentationPlayModeTests.cs` (nouveau) -- preuve hote dans `MVP_Run` -- constater sur un pair reel le chemin que les gardes EditMode decrivent : les trois vehicules de trafic sont spawes, exposent un `Behavior` lisible depuis `NetworkedRageState`, et les libelles crees a l'execution (`AIBehaviorDebugLabel`, `RageRoadEventLabel`) portent le texte derive de l'etat replique ; `NetworkManager.Singleton.Shutdown()` en `[UnityTearDown]`.
- [x] `.meta` pour chaque fichier `.cs` ajoute (`fileFormatVersion: 2` + `guid:`).

**Acceptance Criteria:**

- Given les trois vehicules IA de `MVP_Run` et une session active, when un pair observe le trafic, then position, comportement, rage et etat Rage Road proviennent tous de `NetworkVariable` lisibles par tous et ecrites par le serveur, et aucune copie locale de verite n'existe cote pair.
- Given `NetworkedAIVehicleState`, `NetworkedRageState` et `NetworkedRunState`, when on inspecte leurs `NetworkVariable` publiques, then chacune est en lecture `Everyone` et en ecriture `Server` -- verifie par test, car `RoadRageScaffoldTests` ne couvre que l'ecriture.
- Given un client non-hote, when il tente d'ecrire une `NetworkVariable` de comportement IA, then l'ecriture est refusee et l'etat replique reste identique a celui de l'hote -- aucune vue du depot ne contient d'ecriture ni de RPC.
- Given un client qui rejoint `MVP_Run` apres le demarrage du run, when il charge la scene, then ses vues IA et HUD se remplissent depuis les `NetworkVariable` courantes au premier tick, sans aucun chemin de rattrapage dedie.
- Given `NetworkedAIVehicleState`, when on inventorie ses `NetworkVariable`, then aucune n'est repliquee sans etre ecrite ni lue, et le cout de replication est documente pour 4 joueurs et 3 vehicules IA.

## Spec Change Log

## Design Notes

- Story de preuve, pas de construction : `NetworkTransform` (`Greybox_AIVehicle.prefab:151`), `Behavior`, `NetworkedRageState` et `RageRoadEvent` sont deja repliques. Reecrire la synchronisation serait la regression la plus probable de cette story ; le travail utile est de fermer les deux ecarts reels et de rendre le contrat executable par des tests.
- Permission de lecture : c'est le seul trou de l'AC1. `RoadRageScaffoldTests.cs:170` verifie `WritePerm == Server` pour 6 types d'etat, mais **aucune** garde n'impose `ReadPerm == Everyone` : une `NetworkVariable` privee passerait tous les tests actuels tout en rendant le client aveugle. La garde va dans la nouvelle fixture, pas dans le scaffold (fichier de l'Epic 0, hors perimetre).
- `RouteIndex` : deux indices concordants (`NetworkedAIVehicleState.cs:21` est la seule occurrence du symbole hors commentaire ; la classe la documente deja comme « non mutee par cette story ») en font du cout mort. `WaypointIndex` porte la progression et `route` porte l'identite ; la garde (e) empeche la reapparition silencieuse de ce type de champ.
- Arrivant tardif : `EnableSceneManagement = true` (`RoadRageBootstrap.cs:173`) et les NV `Everyone` suffisent au transport ; cote affichage, `RageRoadEventFlowController.Update` (`:49`) et `RunFlowController.Update` (`:130`) relisent l'etat a chaque frame. L'AC est donc deja satisfaite et la story en apporte la **preuve**, sans ajouter d'abonnement.
- Presentation textuelle uniquement : le préfixe `[CIBLE]` / `[RAGE ROAD]` (`AIVehicleBehaviorDebugView.cs:112-115`) et les libelles HUD rendent l'etat sans couleur, conformement a l'AC d'accessibilite de l'epic.
- **Budget de replication (AC5).** Par vehicule IA, l'etat replique se limite a : `NetworkTransform` (position + rotation, a la cadence d'envoi reseau, seuils a 0 -- `Greybox_AIVehicle.prefab:151-195`), `WaypointIndex` (ecrit au franchissement de waypoint, quelques unites par seconde au plus), `Behavior` (ecrit au changement de disposition), et `NetworkedRageState` (`Disposition` / `RageValue` / `FearValue`, ecrits a la variation de rage ou de peur, donc pilotee par les actions joueur). Les `NetworkVariable` ne coutent rien tant que leur valeur ne change pas ; le poste dominant est donc le `NetworkTransform`, seul contributeur par tick.
- **Budget de replication (AC5), formule.** Cout sortant hote ~= 3 vehicules x (1 mise a jour `NetworkTransform` par tick de deplacement), plus les NV a la demande ; cout entrant client = le meme volume, diffuse aux observateurs. Aucune RPC de mouvement n'y figure (le controleur ecrit le Rigidbody host-only), ni aucun etat local (verrou de cible, marqueurs texte), ni aucune collection : c'est ce qui garde le volume proportionnel au trafic et non aux joueurs. Le retrait de `RouteIndex` allege en plus la charge de spawn.
- **Budget de replication (AC5), mesure restante.** Le chiffre constate a 4 joueurs et 3 IA n'a pas ete releve : le Test Runner ne sait ouvrir qu'un `NetworkManager` par processus, donc seul un run Steam a deux pairs le donne. A reporter dans `docs/setup/story-5-7-notes.md` lors du check manuel (voir Verification), avant que la Story 5.8 ne serve de Go.
- **Budget de replication (AC5), leviers non appliques.** `PositionThreshold` et `RotAngleThreshold` sont a 0 et `UseUnreliableDeltas` a 0 (`Greybox_AIVehicle.prefab:166-195`) : chaque delta est envoye. Ces trois reglages sont le premier levier si la mesure ci-dessus montre un cout excessif -- volontairement non touches ici (Ask First), car ils echangent du reseau contre la nettete du mouvement observe.

## Verification

**Commands:**

- Aucune commande CLI : verification par le Test Runner Unity (checkpoint humain, conformement aux Stories 5.5 et 5.6).
- Repli batch si l'Editor est ferme : `Unity.exe -batchmode -projectPath "D:\Projets\RRS" -runTests -testPlatform EditMode -testResults _bmad-output/implementation-artifacts/story-5-7-editmode-results.xml` (sans `-quit`), puis `-testPlatform PlayMode` -- le batch refuse de s'executer si l'Editor detient le verrou projet.

**Manual checks (if no CLI):**

- `RoadRage.Tests.EditMode` filtre `Story57AiTrafficClientPresentationTests`, puis suite complete verte (dont `RoadRageScaffoldTests`, qui revalide les permissions de `NetworkedAIVehicleState` apres retrait de `RouteIndex`).
- `RoadRage.Tests.PlayMode` filtre `Story57AiTrafficClientPresentationPlayModeTests` : trois vehicules IA spawes dans `MVP_Run`, libelles presents et remplis.
- Session Steam a deux pairs sur `MVP_Run` : l'hote lance une partie, verifier que les trois IA ont la meme position, le meme comportement, la meme rage et le meme etat Rage Road des deux cotes ; faire monter une IA a `ConfrontationCapable` et verifier le libelle `[RAGE ROAD]` cote client ; **faire rejoindre le client apres le demarrage du run** et verifier qu'il voit l'etat courant, puis qu'il subit le meme resultat que l'hote en pressant `T` (aucune modification de comportement forcee cote client).
- Relever le budget de replication constate (position + NV) pour 4 joueurs et 3 IA et le reporter dans `Design Notes` ou dans un `docs/setup/story-5-7-notes.md` -- c'est l'element d'AC que seul un run reel peut trancher.

## Suggested Review Order

**Retrait de l'etat replique mort**

- Point de depart : les deux seules NV restantes sont toutes deux reellement consommees.
  [`NetworkedAIVehicleState.cs:20`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs#L20)

- Le commentaire qui justifie le retrait : l'identite de route vit dans la scene, pas dans le reseau.
  [`NetworkedAIVehicleState.cs:14`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleState.cs#L14)

- La doc qui citait `RouteIndex` est reparee, pour ne pas documenter un champ disparu.
  [`NetworkedAIVehicleDriverController.cs:16`](../../Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs#L16)

**Permission de lecture : le seul trou de l'AC « les clients voient »**

- Garde nouvelle : lecture `Everyone` **et** ecriture `Server` sur les trois etats repliques.
  [`Story57AiTrafficClientPresentationTests.cs:90`](../../Assets/RoadRage/Tests/EditMode/Story57AiTrafficClientPresentationTests.cs#L90)

- Le perimetre inspecte, volontairement restreint aux etats du trafic et du run.
  [`Story57AiTrafficClientPresentationTests.cs:64`](../../Assets/RoadRage/Tests/EditMode/Story57AiTrafficClientPresentationTests.cs#L64)

**Les vues ne font que lire**

- Aucune ecriture de NV ni aucune RPC dans la vue IA, la vue de rage et le HUD.
  [`Story57AiTrafficClientPresentationTests.cs:121`](../../Assets/RoadRage/Tests/EditMode/Story57AiTrafficClientPresentationTests.cs#L121)

- Scene sans trafic : l'absence est dite en texte, sans exception ni « Calm » trompeur.
  [`Story57AiTrafficClientPresentationTests.cs:141`](../../Assets/RoadRage/Tests/EditMode/Story57AiTrafficClientPresentationTests.cs#L141)

- `CodeOnly` : les gardes portent sur le code, pas sur les commentaires qui nomment la regle.
  [`Story57AiTrafficClientPresentationTests.cs:289`](../../Assets/RoadRage/Tests/EditMode/Story57AiTrafficClientPresentationTests.cs#L289)

**Arrivant tardif : structurel, volontairement pas re-code**

- Aucun `OnValueChanged` sur le chemin de presentation : le polling par frame suffit.
  [`Story57AiTrafficClientPresentationTests.cs:157`](../../Assets/RoadRage/Tests/EditMode/Story57AiTrafficClientPresentationTests.cs#L157)

- La decision reste host-only ; la presentation tourne sur tous les pairs, prouve par profondeur d'accolades.
  [`Story57AiTrafficClientPresentationTests.cs:176`](../../Assets/RoadRage/Tests/EditMode/Story57AiTrafficClientPresentationTests.cs#L176)

- Le helper de profondeur, insensible a l'indentation du fichier inspecte.
  [`Story57AiTrafficClientPresentationTests.cs:310`](../../Assets/RoadRage/Tests/EditMode/Story57AiTrafficClientPresentationTests.cs#L310)

**Cout de replication**

- Aucune NV IA sans consommateur : la garde qui empeche `RouteIndex` de revenir.
  [`Story57AiTrafficClientPresentationTests.cs:195`](../../Assets/RoadRage/Tests/EditMode/Story57AiTrafficClientPresentationTests.cs#L195)

- Trois vehicules in-scene placed, chacun avec `NetworkTransform`, rage et rendu textuel.
  [`Story57AiTrafficClientPresentationTests.cs:224`](../../Assets/RoadRage/Tests/EditMode/Story57AiTrafficClientPresentationTests.cs#L224)

**Preuve sur un pair reel (peripherique)**

- L'hote voit les trois IA spawnees et les libelles rendant la valeur repliquee.
  [`Story57AiTrafficClientPresentationPlayModeTests.cs:93`](../../Assets/RoadRage/Tests/PlayMode/Story57AiTrafficClientPresentationPlayModeTests.cs#L93)

- Arret obligatoire du `NetworkManager` et du bootstrap, sinon pollution des fixtures suivantes.
  [`Story57AiTrafficClientPresentationPlayModeTests.cs:57`](../../Assets/RoadRage/Tests/PlayMode/Story57AiTrafficClientPresentationPlayModeTests.cs#L57)

