---
title: "Story 4.4 : La seconde action passager cree un incident"
type: 'feature'
created: '2026-09-12'
status: 'done'
review_loop_iteration: 0
context: []
baseline_commit: 'c1ab683c7840d7a69c04b3dc1994865556ddd317'
story_key: '4-4-passenger-action-two-creates-an-incident-or-resource-opportu'
---

<frozen-after-approval reason="human-owned intent -- do not modify unless human renegotiates">

## Intent

**Problem:** Le slot 1 est deja valide par le framework passager, mais ne produit aucun resultat gameplay. Le passager doit pouvoir creer un incident visible et faible valeur sans introduire l'economie ou Rage Road.

**Approach:** Reutiliser l'intention autoritaire existante pour convertir uniquement l'action slot 1 acceptee en marqueur d'incident partage, puis l'afficher en lecture seule dans `MVP_Run` et le harnais rage.

## Boundaries & Constraints

**Always:** Reutiliser la validation, cooldown, sequence et portee de `NetworkedPassengerActionIntent`; creer/modifier l'incident seulement sur le chemin host/solo autoritaire; stocker tout etat partage dans un `NetworkBehaviour` avec ecriture serveur et le rendre lisible par les clients; afficher le resultat sans laisser l'UI muter le gameplay.

**Ask First:** Ajouter une monnaie, une recompense, une definition de ressource, un effet de rage obligatoire, ou modifier le contrat de validation des actions passager.

**Never:** Dupliquer la validation de 4.2, contourner le cooldown, creer du trafic IA, une confrontation Rage Road, un achat ou une logique d'economie Epic 6; transformer l'incident en systeme generique d'effets futur.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|---------------|----------------------------|----------------|
| Slot 1 accepte | Passager assis, cible valide, cooldown libre | L'hote active ou rafraichit un marqueur d'incident partage et le HUD l'affiche | N/A |
| Slot 1 refuse | Cible hors portee, siege/acteur invalide, replay ou cooldown | Aucun incident n'est cree ou rafraichi; le verdict existant reste visible | Aucune mutation partagee |
| Client passager | Intention recue par le serveur | Le meme etat d'incident replique devient visible au client, sans dependance au `RunFlowController` local de l'hote | Ne jamais compter sur un abonnement local client |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/App/Run/NetworkedPassengerActionIntent.cs` -- pipeline unique `RequestSlot` / RPC / `ApplyAuthoritative`; point fiable pour declencher l'effet slot 1 apres verdict accepte, y compris pour un client distant.
- `Assets/RoadRage/App/Run/RunFlowController.cs` -- compose le joueur local, configure l'intent et le HUD; ne conserve que le branchement local de presentation necessaire.
- `Assets/RoadRage/Features/PassengerActions/` -- emplacement d'un etat d'incident minimal, feature-sliced et host-owned; `PassengerActionDef`/catalogue restent inchanges.
- `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs` -- presentation lecture seule du marqueur d'incident, distincte du verdict transitoire et du HUD rage.
- `Assets/RoadRage/DevTools/RageSandboxAutoStart.cs` et `Assets/RoadRage/App/Scenes/MVP_Run.unity` -- harnais et composition a cabler pour l'incident visible sans trafic IA ni economie.
- `Assets/RoadRage/Tests/EditMode/Story43PassengerActionOneChangesRageTests.cs` et `Assets/RoadRage/Tests/PlayMode/Story43PassengerActionOneMvpRunPlayModeTests.cs` -- gabarits de fixture, validation et scene pour les tests 4.4.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/PassengerActions/NetworkedPassengerActionIncidentState.cs` -- ajouter un unique marqueur d'incident host-owned, repliquable et observable, sans monnaie ni catalogue de ressources.
- [x] `Assets/RoadRage/App/Run/NetworkedPassengerActionIntent.cs` -- appliquer/refraichir ce marqueur seulement pour le slot 1 apres validation autoritaire, avec le meme comportement pour les intentions RPC et solo.
- [x] `Assets/RoadRage/App/Run/RunFlowController.cs` et `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs` -- composer puis rendre l'incident partage visible dans le HUD sans en faire une mutation UI.
- [x] `Assets/RoadRage/App/Scenes/MVP_Run.unity` et `Assets/RoadRage/DevTools/RageSandboxAutoStart.cs` -- cabler l'etat et le harnais pour une verification jouable locale.
- [x] `Assets/RoadRage/Tests/EditMode/Story44PassengerActionTwoCreatesIncidentTests.cs` et `Assets/RoadRage/Tests/PlayMode/Story44PassengerActionTwoMvpRunPlayModeTests.cs` -- couvrir acceptation, refus/cooldown sans mutation et presentation de l'incident dans `MVP_Run`.

**Acceptance Criteria:**
- Given un passager assis et une cible valide, when il declenche le second slot MVP, then l'hote cree ou rafraichit un marqueur d'incident visible de faible valeur.
- Given une validation refusee, when le slot 1 est hors portee, en cooldown ou rejoue, then aucun incident partage ne change et le retour de refus reste visible.
- Given un passager client, when son action slot 1 est acceptee par l'hote, then le marqueur replique est visible sans mutation client directe.
- Given `MVP_Run` ou `Dev_RageSandbox`, when le slot 1 est teste, then le resultat fonctionne sans Rage Road, economie, IA trafic ou recompense significative.

## Spec Change Log

## Design Notes

Un marqueur d'incident unique est le plus petit resultat persistant qui respecte la Story 4.4. Les opportunites monetaires attendront le contrat d'economie d'Epic 6; l'etat n'a pas besoin d'etre un framework d'effets extensible.

## Verification

**Commands:**
- `unity command --project-path D:\Projets\RRS recompile --focus false` -- expected: compilation terminee sans erreur.
- `unity command --project-path D:\Projets\RRS run_tests --mode EditMode --filter RoadRage.Tests.EditMode.Story44PassengerActionTwoCreatesIncidentTests --filter_type testName --async_tests true` -- expected: tous verts.
- `unity command --project-path D:\Projets\RRS run_tests --mode PlayMode --filter RoadRage.Tests.PlayMode.Story44PassengerActionTwoMvpRunPlayModeTests --filter_type testName --async_tests true` -- expected: tous verts.
- `git diff --check` -- expected: aucune erreur dans les fichiers modifies pour cette story.

**Manual checks (if no CLI):**
- Dans `MVP_Run`, entrer comme passager, utiliser le slot 2 sur une rage target valide, verifier le marqueur d'incident; attendre ou reutiliser immediatement l'action et verifier que le cooldown ne le rafraichit pas.

## Suggested Review Order

**Effet autoritaire**

- Le slot 1 cree l'incident seulement apres validation du host.
  [`NetworkedPassengerActionIntent.cs:185`](../../Assets/RoadRage/App/Run/NetworkedPassengerActionIntent.cs#L185)

- L'etat unique replique le compteur avec ecriture serveur.
  [`NetworkedPassengerActionIncidentState.cs:9`](../../Assets/RoadRage/Features/PassengerActions/NetworkedPassengerActionIncidentState.cs#L9)

**Presentation lecture seule**

- Le HUD observe le marqueur partage sans muter le gameplay.
  [`RunCheckpointHudScreen.cs:82`](../../Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs#L82)

- Le harnais rage transmet le meme etat au panneau debug.
  [`RageSandboxAutoStart.cs:128`](../../Assets/RoadRage/DevTools/RageSandboxAutoStart.cs#L128)

- Le panneau sandbox rend le compteur d'incident accessible au test manuel.
  [`PassengerActionDebugView.cs:16`](../../Assets/RoadRage/Features/PassengerActions/PassengerActionDebugView.cs#L16)

**Verification**

- Les refus et cooldown ne modifient jamais le marqueur.
  [`Story44PassengerActionTwoCreatesIncidentTests.cs:42`](../../Assets/RoadRage/Tests/EditMode/Story44PassengerActionTwoCreatesIncidentTests.cs#L42)

- Les deux scenes confirment le rendu visible et l'etat partage.
  [`Story44PassengerActionTwoMvpRunPlayModeTests.cs:17`](../../Assets/RoadRage/Tests/PlayMode/Story44PassengerActionTwoMvpRunPlayModeTests.cs#L17)
