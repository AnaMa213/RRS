---
title: "Story 4.1 : Module d'etat de rage et definitions"
type: 'feature'
created: '2026-09-10'
status: 'done'
review_loop_iteration: 0
context: []
baseline_commit: '5769903904b75706a02337ac618e55b229df9c03'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** La rage n'existe pas encore comme etat de gameplay : `NetworkedRageState` n'est qu'un squelette a un seul champ (`Disposition`), sans valeur numerique, sans donnee auteur de seuils, et sans affichage visible pour la tester.

**Approche:** Etendre `NetworkedRageState` avec une valeur de rage numerique host-owned qui pilote `Disposition` via des seuils authorees en ScriptableObject (`RageTuningDef` + `RageTuningCatalog`, sur le modele de `CharacterDef`/`CharacterCatalog`), puis rendre l'etat visible dans `Dev_RageSandbox` via un element HUD/dev minimal.

## Boundaries & Constraints

**Always:**
- `NetworkedRageState` reste `sealed`, herite de `HostOwnedNetworkStateBehaviour`, et toute nouvelle `NetworkVariable` utilise `NetworkVariableWritePermission.Server` sans garde `IsServer` explicite dans la methode de mutation (meme convention que `NetworkedVehicleState.ApplyDamage`).
- `RageTuningDef`/`RageTuningCatalog` copient exactement le contrat de `CharacterDef`/`CharacterCatalog` : `DefinitionId` stable minuscule, `TryValidate(out string error)` appele depuis `OnValidate`, log `Debug.LogWarning("[Rage] ...")`.
- Un `NetworkedRageState` = une instance de cible ; la multiplicite ("au moins une cible independamment") vient d'un composant par GameObject cible, pas d'une collection interne.
- Respecter la frontiere asmdef : `RoadRage.Features.Rage` ne reference aucun autre `RoadRage.Features.*` ; ajouter uniquement `Unity.TextMeshPro` pour l'affichage dev.
- Le sandbox `Dev_RageSandbox` reprend le cablage de `Dev_VehicleSandbox` (GameObject `NetworkManager` + script `DevTools` editor-only qui demarre l'hote si aucun `NetworkManager` n'ecoute).

**Ask First:** Si une interface de cible partagee (`IRageTarget` ou equivalent) doit etre creee maintenant pour anticiper l'Epic 5, ou si l'attache par composant + `NetworkObjectReference` (AD-20) suffit pour cette story.

**Never:** Construire un vehicule IA, une prefab d'ennemi, ou cabler `NetworkedRageState` sur `NetworkedAIVehicleState`/`Features.Vehicles` (Epic 5). Implementer un declencheur d'action passager ou un slot d'UI joueur (Story 4.2/4.3). Construire le catalogue generique multi-types enregistre au bootstrap decrit par l'AD-25 ; suivre le precedent plus etroit `CharacterCatalog` (catalogue scoped a la feature) a la place.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Seuil franchi a la hausse | `RageValue` passe de 15 a 25, seuil Irritated a 20 | `Disposition` = Irritated | N/A |
| Sous le premier seuil | `RageValue` = 5, premier seuil (Irritated) a 20 | `Disposition` = Calm | N/A |
| Delta positif au plafond | `ApplyRageDelta(+999, tuning)`, `MaxRageValue` = 100 | `RageValue` clampee a 100, `Disposition` = dernier palier | N/A |
| Delta negatif sous zero | `ApplyRageDelta(-999, tuning)` | `RageValue` clampee a 0, `Disposition` = Calm | N/A |
| Tuning absent | `ApplyRageDelta(delta, tuning: null)` | Aucun changement d'etat | Pas d'exception, no-op |
| Id de catalogue duplique/non minuscule/vide | `RageTuningCatalog.TryValidate` sur donnee invalide | Retourne `false` | Message d'erreur descriptif via `out error` |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Rage/NetworkedRageState.cs` -- squelette existant (`Disposition` seul) a etendre avec `RageValue` + `ApplyRageDelta`.
- `Assets/RoadRage/Shared/Domain/RageDisposition.cs` -- enum existant (Calm/Irritated/Flee/Block/Ram/ConfrontationCapable), deja aligne sur l'epic.
- `Assets/RoadRage/Shared/Networking/HostOwnedNetworkStateBehaviour.cs` -- base class ; `Assets/RoadRage/Shared/Definitions/DefinitionId.cs` -- type d'id partage.
- `Assets/RoadRage/Features/Players/CharacterDef.cs` + `CharacterCatalog.cs` -- gabarit exact a suivre pour `RageTuningDef`/`RageTuningCatalog` (id, `TryValidate`, `OnValidate`).
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleState.cs:167-195` -- gabarit du predicat pur seuil (`ApplyDamage`/`ComputeThresholdsCrossed`) a refleter pour la resolution de `Disposition` par seuils.
- `Assets/RoadRage/Features/UI/RunCheckpointHudScreen.cs` -- pattern HUD lecture seule existant (hors scope direct : `Features.Rage` ne peut pas le referencer, cf. isolation asmdef).
- `Assets/RoadRage/Features/Rage/RoadRage.Features.Rage.asmdef` -- reference actuelle `["RoadRage.Shared", "Unity.Netcode.Runtime"]`, a completer avec `Unity.TextMeshPro`.
- `Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity` -- scene vide (Camera/Light/Global Volume seulement) a completer.
- `Assets/RoadRage/DevTools/RoadRageNetcodeSmokeTestAutoStart.cs` + son cablage scene dans `Dev_VehicleSandbox.unity` (GameObject `NetworkManager` + composant DevTools) -- gabarit exact pour le nouvel auto-start du rage sandbox.
- `Assets/RoadRage/Tests/EditMode/RoadRageScaffoldTests.cs:85-93` -- `NetworkedRageState` deja enregistre dans `RuntimeStateTypes` ; le nouveau champ `RageValue` est automatiquement couvert par `RuntimeStateShellsAreNetworkBehavioursAndServerWrite` (aucune modif de ce fichier requise).
- `Assets/RoadRage/Tests/EditMode/Story32DriverControlAndLocalCameraTests.cs` (ou tout `Story3X...Tests.cs`) -- convention de nommage/emplacement a suivre pour `Story41RageStateModuleAndDefinitionsTests.cs`.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/Rage/RageTuningDef.cs` -- nouveau ScriptableObject (`id`, `maxRageValue`, paliers ordonnes `RageDisposition`+seuil, `ResolveDisposition(float)` pur) -- donnee auteur statique demandee par l'AC.
- [x] `Assets/RoadRage/Features/Rage/RageTuningCatalog.cs` -- nouveau ScriptableObject miroir de `CharacterCatalog` (`TryGetById`, `TryValidate`) -- id stable globalement unique.
- [x] `Assets/RoadRage/Features/Rage/NetworkedRageState.cs` -- ajouter `NetworkVariable<float> RageValue` (defaut 0) et `ApplyRageDelta(float delta, RageTuningDef tuning)` (clamp + recalcul de `Disposition` via `tuning.ResolveDisposition`) -- porte la valeur numerique manquante de l'AC.
- [x] `Assets/RoadRage/Features/Rage/RoadRage.Features.Rage.asmdef` -- ajouter `Unity.TextMeshPro`.
- [x] `Assets/RoadRage/Features/Rage/RageStateDebugView.cs` -- nouveau `MonoBehaviour` (champ `NetworkedRageState`, `TMP_Text`, lecture par `Update()`, plus `[ContextMenu]` editor-only pour appliquer un delta de test cote hote) -- satisfait l'AC "dev UI/HUD affiche l'etat de rage".
- [x] `Assets/RoadRage/DevTools/RageSandboxAutoStart.cs` -- nouveau script editor-only, gate sur la scene `Dev_RageSandbox`, demarre l'hote si `NetworkManager` n'ecoute pas (sur le modele simplifie de `RoadRageNetcodeSmokeTestAutoStart`, sans harness de siege).
- [x] `Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity` -- ajouter GameObject `NetworkManager` (+ `RageSandboxAutoStart`), une cible in-scene (`NetworkObject` + `NetworkedRageState`), un Canvas/`TMP_Text` cable a `RageStateDebugView`.
- [x] `Assets/RoadRage/ScriptableObjects/Rage/RageTuningDef_Default.asset` + `RageTuningCatalog.asset` -- instances authorees (seuils placeholder explicitement remplacables).
- [x] `Assets/RoadRage/Tests/EditMode/Story41RageStateModuleAndDefinitionsTests.cs` -- tests EditMode : `ResolveDisposition` aux bornes de seuil, clamp haut/bas de `ApplyRageDelta`, `ApplyRageDelta` avec tuning nul, `TryValidate` sur id duplique/vide/non minuscule.

**Acceptance Criteria:**
- Given un `NetworkedRageState` attache a une cible et un second attache a une autre cible, when chacun recoit un `ApplyRageDelta` different, then leurs `RageValue`/`Disposition` evoluent independamment.
- Given un `RageTuningDef` avec des paliers ascendants et un id stable minuscule, when `RageValue` franchit un seuil, then `Disposition` reflete le palier correspondant et `RageValue` reste dans `[0, MaxRageValue]`.
- Given `Dev_RageSandbox` joue dans l'Editeur en hote, when la scene demarre, then `RageStateDebugView` affiche la valeur de rage et le libelle d'etat courants de la cible sandbox.
- Given le test scaffold existant `RuntimeStateShellsAreNetworkBehavioursAndServerWrite`, when il s'execute apres l'ajout de `RageValue`, then il reste vert sans modification du fichier de test.

## Spec Change Log

## Design Notes

`RageTuningDef.ResolveDisposition(float rageValue)` reste un predicat pur sur des paliers tries par seuil ascendant (meme esprit que `NetworkedVehicleState.ComputeThresholdsCrossed`), pour rester testable en EditMode sans Netcode :

```csharp
public RageDisposition ResolveDisposition(float rageValue)
{
    var resolved = RageDisposition.Calm;
    for (var i = 0; i < thresholds.Length; i++)
        if (rageValue >= thresholds[i].MinValue) resolved = thresholds[i].Disposition;
    return resolved;
}
```

L'ordre ascendant des paliers est une invariante d'authoring validee par `RageTuningCatalog.TryValidate`, pas recalculee a l'execution.

## Verification

**Commands:**
- `unity command --project-path D:\Projets\RRS recompile --focus false` puis `recompile_status` -- attendu : `completed`, `failed=false`, `errors=[]`.
- `unity command --project-path D:\Projets\RRS run_tests --mode EditMode --filter RoadRage.Tests.EditMode.Story41RageStateModuleAndDefinitionsTests --filter_type testName --async_tests true`, puis `test_status` -- attendu : tous verts.
- `unity command --project-path D:\Projets\RRS run_tests --mode EditMode --filter RoadRage.Tests.EditMode.RoadRageScaffoldTests --filter_type testName --async_tests true`, puis `test_status` -- attendu : toujours vert (non-regression scaffold).
- `git diff --check` -- attendu : aucune erreur reelle.

**Manual checks (if no CLI):**
- Lancer `Dev_RageSandbox` en Play Mode (hote) : verifier l'affichage initial "Rage : 0 (Calm)", declencher le `[ContextMenu]` de test pour verifier que le libelle change de palier visiblement.

## Suggested Review Order

**Etat de rage host-owned**

- Point d'entree : clamp + recalcul de Disposition, meme convention sans garde IsServer que ApplyDamage.
  [`NetworkedRageState.cs:32`](../../Assets/RoadRage/Features/Rage/NetworkedRageState.cs#L32)

**Seuils authores et validation**

- Predicat pur : dernier palier ascendant franchi resout Disposition, testable sans Netcode.
  [`RageTuningDef.cs:82`](../../Assets/RoadRage/Features/Rage/RageTuningDef.cs#L82)

- Garde de coherence des donnees auteur, miroir exact de CharacterCatalog (id, paliers).
  [`RageTuningCatalog.cs:84`](../../Assets/RoadRage/Features/Rage/RageTuningCatalog.cs#L84)

- Invariante ajoutee par la revue : refuse un palier authore au-dessus de MaxRageValue.
  [`RageTuningDef.cs:123`](../../Assets/RoadRage/Features/Rage/RageTuningDef.cs#L123)

- Branchement du nouveau garde-fou dans TryValidate.
  [`RageTuningCatalog.cs:139`](../../Assets/RoadRage/Features/Rage/RageTuningCatalog.cs#L139)

**Visualisation dev et sandbox**

- HUD lecture seule : affiche RageValue/Disposition, n'ecrit jamais dans une NetworkVariable.
  [`RageStateDebugView.cs:35`](../../Assets/RoadRage/Features/Rage/RageStateDebugView.cs#L35)

- Delta de test editor-only, garde par IsSpawned/IsServer avant d'appliquer un changement.
  [`RageStateDebugView.cs:53`](../../Assets/RoadRage/Features/Rage/RageStateDebugView.cs#L53)

- Auto-start editeur simplifie : demarre l'hote si Dev_RageSandbox est la scene active.
  [`RageSandboxAutoStart.cs:21`](../../Assets/RoadRage/DevTools/RageSandboxAutoStart.cs#L21)

- Cablage scene : NetworkManager du sandbox (transport + auto-start), sur le modele Dev_VehicleSandbox.
  [`Dev_RageSandbox.unity:448`](../../Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity#L448)

- Cible in-scene : NetworkObject + NetworkedRageState, la "au moins une cible" de l'AC.
  [`Dev_RageSandbox.unity:686`](../../Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity#L686)

- Canvas/label relies au RageStateDebugView pour l'affichage visible en Play Mode.
  [`Dev_RageSandbox.unity:568`](../../Assets/RoadRage/App/Scenes/Dev_RageSandbox.unity#L568)

**Peripheriques**

- Couverture de la matrice I/O (bornes de seuil, clamp, no-op, validations de catalogue).
  [`Story41RageStateModuleAndDefinitionsTests.cs:1`](../../Assets/RoadRage/Tests/EditMode/Story41RageStateModuleAndDefinitionsTests.cs#L1)

- Donnee authored placeholder livree avec le module (seuils explicitement remplacables).
  [`RageTuningDef_Default.asset:1`](../../Assets/RoadRage/ScriptableObjects/Rage/RageTuningDef_Default.asset#L1)
