---
title: 'Story 1.3 — Creation de personnage rudimentaire et selection du profil joueur'
type: 'feature'
created: '2026-09-08'
status: 'done'
review_loop_iteration: 0
baseline_commit: '035bfc74e7c847c1320b75dd77437350edb4c6d7'
context:
  - '{project-root}/_bmad-output/implementation-artifacts/epic-1-context.md'
  - '{project-root}/_bmad-output/implementation-artifacts/spec-1-2-local-lobby-shell-and-match-settings-draft.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem :** La coquille de lobby (Story 1.2) ne connait aucune identite joueur : il n'existe ni nom, ni personnage selectionnable, ni profil capable de survivre au changement de scene vers `MVP_Run`. L'entree monde de la Story 1.5 n'a donc rien a consommer.

**Approach :** Ajouter un ecran de setup de personnage accessible depuis la coquille de lobby (champ de nom + selection parmi des `CharacterDef` ScriptableObject a id stable + retour de validation visible), un `PlayerProfile` C# pur produit par la couche App, et un depot de profil porte par le bootstrap persistant pour que la Story 1.5 le lise a l'entree du monde.

## Boundaries & Constraints

**Always :**
- Memes frontieres qu'en Story 1.2 : `RoadRage.Features.UI` ne reference que `RoadRage.Shared` + UGUI/TextMeshPro ; `RoadRage.Features.Players` ne reference que `RoadRage.Shared` (+ Netcode deja present) ; seule `RoadRage.App` compose UI + Players. L'ecran ne connait donc jamais `CharacterDef` ni `PlayerProfile` : il recoit des `string`/`DefinitionId` et emet des intentions.
- L'ecran ne mute jamais le profil et ne charge jamais de scene ; la couche App valide, ecrit et rafraichit l'affichage — **un seul ecrivain** par libelle (lecon Patch 3 de la Story 1.2 : aucune ecriture d'etat dans `Awake` de l'ecran).
- Id de personnage : `DefinitionId` (`RoadRage.Shared.Definitions`), valeur minuscule globalement unique, stable et exploitable telle quelle par la synchro reseau de l'Epic 2.
- Les `CharacterDef` sont des donnees auteur statiques sous `Assets/RoadRage/ScriptableObjects/Players/` ; le profil de session runtime n'est jamais stocke dans un ScriptableObject.
- Nom invalide ou vide : retour visible via un libelle de validation dedie **et** aucune ecriture de profil — jamais d'echec silencieux.
- Logs prefixes par couche : `[Players]`, `[App]`, `[UI]`.

**Ask First :**
- Rendu du "modele placeholder" : la story livre une silhouette UI teintee par `CharacterDef` plus un emplacement `previewPrefab` vide, que la Story 1.4 remplira avec le vrai greybox issu de l'intake Blender. Si la revue veut un mesh 3D des maintenant, cela empiete sur la Story 1.4 et le gate d'intake : HALT.
- Toute modification de scene ou de reference asmdef au-dela de celles listees ici.

**Never :**
- Aucun `NetworkManager`, aucun `NetworkedPlayerState` instancie, aucune synchro de profil (Epic 2).
- Aucun chargement de `MVP_Run` ni mouvement on-foot (Story 1.5) ; aucun prefab de personnage greybox ni import Blender (Story 1.4).
- Ne pas persister le profil sur disque (`PlayerPrefs`, fichier) : la duree de vie est celle de la session applicative.
- Ne pas toucher au cablage Back / Play de `MainMenuScreen` (Story 1.1).

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Ouverture setup personnage | Clic sur le bouton Personnage de la coquille de lobby | Ecran affiche : champ de nom, personnage courant (nom + silhouette), Confirmer, Fermer ; premier `CharacterDef` du catalogue selectionne par defaut | N/A |
| Nom valide confirme | Nom `Kenan`, personnage courant | Profil ecrit dans le depot (`DisplayName` normalise + `CharacterId`), notice Info visible, ecran ferme | N/A |
| Nom vide ou espaces | `""` ou `"   "`, clic Confirmer | Libelle de validation visible ; profil du depot inchange ; ecran reste ouvert | Message explicite "nom requis" |
| Nom trop long / caracteres interdits | 40 caracteres, ou `a<b>` | Libelle de validation visible ; profil inchange ; ecran reste ouvert | Message nommant la regle violee |
| Nom avec espaces en bordure | `"  Kenan  "` | Normalise en `Kenan` avant ecriture du profil | N/A |
| Changement de personnage | Clic sur le bouton de cycle | Personnage suivant du catalogue affiche (nom + teinte) ; profil non ecrit tant que Confirmer n'est pas presse | N/A |
| Catalogue vide ou non assigne | Reference catalogue absente ou liste vide | Avertissement console, cycle et Confirmer inertes, libelle de validation visible | Aucune exception |
| Reouverture apres confirmation | Profil deja confirme, reouverture de l'ecran | Nom et personnage confirmes reaffiches | N/A |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Players/RoadRage.Features.Players.asmdef` -- references `RoadRage.Shared` + `Unity.Netcode.Runtime` ; destination des nouveaux types. Aucune modification requise.
- `Assets/RoadRage/Shared/Definitions/DefinitionId.cs:9` -- struct id stable (`Value`, `IsEmpty`, `==`) a reutiliser tel quel pour l'id de personnage. Lecture seule.
- `Assets/RoadRage/Features/Lobby/MatchSettings.cs:12` -- patron exact de l'objet de session C# pur a reproduire pour `PlayerProfile`.
- `Assets/RoadRage/Features/UI/LobbyShellScreen.cs:21-44` -- refs serialisees + evenements d'intention ; y ajouter `characterSetupButton` et `CharacterSetupRequested` en suivant le patron `RaiseCreateLobbyRequested` (`Awake` cable `onClick`, avertit si ref nulle, n'ecrit aucun etat).
- `Assets/RoadRage/App/Lobby/LobbyFlowController.cs:24-54` -- patron de couture App a reproduire : `EnsureInstance`, souscription en `Awake`, desabonnement symetrique en `OnDestroy`, `PublishUnavailable`.
- `Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs:22-24,53-54` -- proprietes `Router`/`Notices` creees en `Awake` ; ajouter `Profiles` sur exactement le meme modele (l'objet persistant `DontDestroyOnLoad` est le seul point de passage vers la Story 1.5).
- `Assets/RoadRage/Shared/Presentation/UserNoticeChannel.cs:15` -- `Publish` reutilise tel quel pour la confirmation ; `MainMenuScreen.ShowNotice` affiche deja le canal.
- `Assets/RoadRage/App/Scenes/MainMenuLobby.unity:3251` (`SetupPanel`, 7 enfants, porte `LobbyShellScreen` en `&2097947415`) -- y ajouter le bouton Personnage ; le nouveau `CharacterPanel` est un frere sous `Canvas` (`&27721528`), serialise apres `SetupPanel` pour se superposer.
- `Assets/RoadRage/Tests/PlayMode/RoadRage.Tests.PlayMode.asmdef` -- ne reference pas `RoadRage.Features.Players` : a ajouter. L'asmdef EditMode le reference deja.
- `Assets/RoadRage/Tests/EditMode/Story12LobbyShellTests.cs:99-125` -- helpers `AssetsPath`, `FindComponentInScene`, `AssertSerializedObjectFieldsNonNull` (comparaison `!= null` Unity, Patch 5) a reutiliser.
- `Assets/RoadRage/Tests/PlayMode/Story12LobbyShellPlayModeTests.cs:175-213` -- `PressPlayAndEnterLobbyShell`, `ClickSerializedButton` : **obligatoires** pour les nouveaux tests (Patch 1 : jamais d'invocation de methode privee, sinon un bouton non cable passe au vert). Les composants d'un panneau inactif sont invisibles a `FindAnyObjectByType` tant que le panneau n'est pas active.

## Tasks & Acceptance

**Execution :**
- [x] `Assets/RoadRage/Features/Players/CharacterDef.cs` -- ScriptableObject `[CreateAssetMenu]` : `id` (string minuscule) expose en `DefinitionId`, `displayName`, `previewTint` (Color), `previewPrefab` (GameObject, vide en Story 1.3) -- donnee auteur statique a id stable.
- [x] `Assets/RoadRage/Features/Players/CharacterCatalog.cs` -- ScriptableObject : liste de `CharacterDef`, `Count`, acces par index, `TryGetById`, garde contre les ids dupliques ou vides -- source unique des personnages selectionnables.
- [x] `Assets/RoadRage/Features/Players/PlayerNameValidator.cs` -- statique C# pur : `TryNormalize(raw, out normalized, out error)` ; trim, refus du vide, longueur 2-20, lettres/chiffres/espace/`-`/`_` uniquement -- regle de validation testable sans Unity.
- [x] `Assets/RoadRage/Features/Players/PlayerProfile.cs` -- classe C# pure : `DisplayName`, `CharacterId` (`DefinitionId`) -- meme forme que `MatchSettings`, mappable plus tard sur `NetworkedPlayerState` sans duplication de verite.
- [x] `Assets/RoadRage/Features/Players/PlayerProfileStore.cs` -- classe C# pure : `Current` (nullable), `HasProfile`, `Set(...)`, evenement `ProfileChanged` -- depot de session lu par l'entree monde de la Story 1.5.
- [x] `Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs` -- ajouter `public PlayerProfileStore Profiles { get; private set; }`, instanciee en `Awake` a cote de `Router`/`Notices` -- seul porteur survivant au changement de scene.
- [x] `Assets/RoadRage/Features/UI/CharacterSetupScreen.cs` -- ecran UGUI : refs serialisees `nameInputField` (TMP_InputField), `characterCycleButton`, `confirmButton`, `closeButton`, `characterNameLabel`, `characterPreviewImage`, `validationLabel` ; evenements `NextCharacterRequested`, `ConfirmRequested(string)`, `CloseRequested` ; methodes d'affichage `ShowCharacter(string, Color)`, `ShowValidation(string)`, `ClearValidation()`, `SetName(string)`, `Show()`, `Hide()` -- emet des intentions, n'ecrit aucun etat gameplay.
- [x] `Assets/RoadRage/App/Players/PlayerProfileFlowController.cs` -- couture App : refs serialisees `LobbyShellScreen`, `CharacterSetupScreen`, `CharacterCatalog` ; ouvre l'ecran sur `CharacterSetupRequested`, cycle l'index courant, valide via `PlayerNameValidator`, ecrit `bootstrap.Profiles` et publie une notice Info a la confirmation, reaffiche le profil courant a la reouverture -- seul ecrivain du profil et des libelles.
- [x] `Assets/RoadRage/Features/UI/LobbyShellScreen.cs` -- ajouter `characterSetupButton` + `CharacterSetupRequested` (patron `Raise*` existant) -- point d'entree du flux de setup depuis la coquille de lobby.
- [x] `Assets/RoadRage/ScriptableObjects/Players/` -- creer `CharacterCatalog.asset` et au moins deux `CharacterDef` (ids `char_rookie`, `char_veteran`) via une commande Editor -- au moins un modele placeholder selectionnable, cycle observable.
- [x] `Assets/RoadRage/App/Scenes/MainMenuLobby.unity` -- ajouter le bouton Personnage dans `SetupPanel`, le `CharacterPanel` plein ecran opaque (frere de `SetupPanel`, inactif au chargement) avec ses controles, et l'objet `PlayerProfileFlow` ; assigner toutes les refs serialisees. Construire le `TMP_InputField` via commande Editor (`TMP_DefaultControls`), pas a la main dans le YAML.
- [x] `Assets/RoadRage/Tests/PlayMode/RoadRage.Tests.PlayMode.asmdef` -- ajouter la reference `RoadRage.Features.Players`.
- [x] `Assets/RoadRage/Tests/EditMode/Story13CharacterSetupTests.cs` -- EditMode : matrice de `PlayerNameValidator` (vide, espaces, trop long, caracteres interdits, bordures normalisees), catalogue (ids minuscules, uniques, non vides), `PlayerProfile`/`PlayerProfileStore` sans type de base Unity, composants et refs serialisees presents dans la scene, garde de source (`CharacterSetupScreen` n'appelle ni `SceneManagement` ni le depot de profil), `RoadRage.Features.UI` ne reference pas `RoadRage.Features.Players`.
- [x] `Assets/RoadRage/Tests/PlayMode/Story13CharacterSetupPlayModeTests.cs` -- PlayMode : chaque ligne de la matrice I/O exercee par un vrai clic sur bouton serialise (`ClickSerializedButton`) apres `PressPlayAndEnterLobbyShell`, y compris nom invalide (validation visible, depot inchange), nom valide (depot ecrit, notice publiee, ecran ferme), cycle de personnage, reouverture, et `SceneManager.sceneCount` inchange.

**Acceptance Criteria :**
- Given la coquille de lobby affichee, when le joueur ouvre le setup de personnage, then l'ecran presente un champ de nom et au moins un personnage placeholder selectionnable.
- Given un profil confirme, when on inspecte `RoadRageBootstrap.Profiles.Current`, then l'id de personnage est un `DefinitionId` minuscule non vide, identique a celui du `CharacterDef` choisi, et survit a un changement de scene.
- Given le code livre, when on inspecte les assemblies, then aucune reference feature-a-feature directe n'existe (`UI` ne reference ni `Players` ni `Lobby` ; `App` orchestre).
- Given le projet complet, when les suites EditMode et PlayMode s'executent, then tout passe sans regression Epic 0 / Story 1.1 / Story 1.2.

## Spec Change Log

- **Decision "Ask First" tranchee sans blocage (mode Auto, 2026-09-08)** — le rendu du "modele placeholder" reste une silhouette UI (`Image` teintee par `CharacterDef.previewTint`) plus l'emplacement `previewPrefab` laisse vide, exactement comme le spec l'autorise. Aucun mesh 3D, aucun import Blender : le greybox reste la charge de la Story 1.4 et de son gate d'intake. Un test EditMode (`CharacterDefsHavePreviewPrefabSlotLeftEmptyUntilStory14`) verrouille l'emplacement vide pour que la frontiere ne se perde pas silencieusement.

- **Ajustement de placement du bouton Personnage (2026-09-08)** — le bouton a d'abord ete pose a cote de `BackButton` (`-360, -310`), puis mesure en Play mode : il sortait de l'ecran. Repositionne et redimensionne (`-244, -46`, `160x60`, libelle 22) pour etre entierement visible et hit-testable a cote de la colonne de boutons du lobby, sans deplacer aucun element de la Story 1.2. Constat releve pendant cette mesure et **laisse tel quel** (hors perimetre, `Ask First` sur toute modification de scene au-dela des taches) : `SetupPanel`, `MenuPanel` et `NoticePanel` portent un `localScale` herite de `2.9179332`, ce qui pousse deja `SetupLabel`, `SettingsSummaryLabel` et `BackButton` hors de l'ecran en Play mode. Les tests PlayMode de la Story 1.2 ne l'avaient pas vu parce qu'ils cliquent les boutons par code. A traiter par une passe de layout dediee.

- **Audit de couverture de matrice complete par le coordinateur (step-03, 2026-09-08)** — la ligne "Catalogue vide ou non assigne" enonce deux etats distincts, et un seul etait exerce : `MissingCatalogLeavesCycleAndConfirmInertWithVisibleValidation` met `catalog` a `null` et emprunte donc la garde de reference nulle, jamais la garde `Count > 0` de `HasUsableCatalog`. Ajout de `EmptyCatalogLeavesCycleAndConfirmInertWithVisibleValidation` : catalogue reellement assigne mais sans entree, cycle et Confirmer inertes, message `EmptyCatalogValidation` affiche, aucune notice publiee, aucun profil ecrit. Vert des la premiere execution. Etat evite : une garde `Count > 0` supprimable sans faire rougir aucun test. KEEP : le test doit continuer a construire un vrai `CharacterCatalog` vide, pas a passer `null` deguise.

## Design Notes

Meme couture qu'en Story 1.2, avec l'ecran maintenu ignorant des types du feature Players :

```csharp
// RoadRage.Features.UI.CharacterSetupScreen — n'expose que des primitives
public event Action<string> ConfirmRequested;   // texte brut du champ, non valide
public void ShowValidation(string message);

// RoadRage.App.Players.PlayerProfileFlowController — seul ecrivain
screen.ConfirmRequested += raw =>
{
    if (!PlayerNameValidator.TryNormalize(raw, out var name, out var error)) { screen.ShowValidation(error); return; }
    bootstrap.Profiles.Set(new PlayerProfile(name, current.Id));
    bootstrap.Notices.Publish(new UserNotice(UserNoticeSeverity.Info, "Profil enregistre : " + name));
    screen.Hide();
};
```

`PlayerProfileStore` vit sur l'objet `DontDestroyOnLoad` du bootstrap : c'est ce qui rend le profil lisible par l'entree monde de la Story 1.5 sans coupler `MainMenuLobby` a `MVP_Run`. Le catalogue reste reference en serialise par le controller App ; sa promotion en registre global charge au bootstrap est differee jusqu'a ce qu'un second feature en ait besoin.

`CharacterPanel` est un panneau plein ecran opaque superpose a `SetupPanel` plutot qu'un troisieme etat de `MainMenuScreen` : cela evite d'etendre l'arbitrage de panneaux de la Story 1.1 et garde un seul proprietaire de visibilite par panneau.

## Verification

L'Unity Editor peut etre ouvert (`Temp/UnityLockfile`) : verifier via Unity MCP sans fermer la session utilisateur.

**Commands :**
- `Unity_ReadConsole` (Types `Error`) apres refresh -- attendu : zero erreur de compilation.
- `Unity_RunCommand` executant `TestRunnerApi` sur EditMode -- attendu : suite verte, `RoadRageScaffoldTests`, `Story11MainMenuLaunchTests` et `Story12LobbyShellTests` inclus sans regression.
- `Unity_RunCommand` executant `TestRunnerApi` sur PlayMode -- attendu : suite verte, `Story11*` et `Story12*` sans regression.
- Repli Editor ferme : `& "D:\Program Files\Unity\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath "D:\Projets\RRS" -runTests -testPlatform EditMode -testResults _bmad-output/implementation-artifacts/story-1-3-editmode-results.xml` (sans `-quit`) -- attendu : XML entierement vert. Ce repli n'a pas ete emprunte : l'Editor est reste ouvert et les suites ont tourne via `TestRunnerApi`, qui produit les traces texte `story-1-3-editmode-results.txt` et `story-1-3-playmode-results.txt` citees plus bas. Le `.xml` n'existe donc que si ce repli est un jour utilise.

**Manual checks :**
- Play mode depuis `Bootstrap` : Play -> Personnage -> nom vide refuse avec message visible, nom valide accepte avec notice, cycle de personnage visible, aucune exception console.
- `git status` : seuls les fichiers des taches, leurs `.meta`, `MainMenuLobby.unity` et l'asmdef PlayMode ; aucune scene `Dev_*` ni `EditorBuildSettings.asset` modifie.

### Resultats reels (execution du 2026-09-08, via Unity MCP, Editor 6000.6.0f1 reste ouvert)

- Compilation : zero erreur apres chaque changement de script, d'asmdef et de scene (`Unity_ReadConsole` Types `Error`).
- Suite EditMode (`TestRunnerApi` via `Unity_RunCommand`, filtre assembly `RoadRage.Tests.EditMode`) : **65/65 verts** apres la revue adversariale -- `RoadRageScaffoldTests` (7), `Story11MainMenuLaunchTests` (8) et `Story12LobbyShellTests` (6) inchanges (21 comme en Story 1.2, aucune regression), `Story13CharacterSetupTests` (44) verts. Trace : `story-1-3-editmode-results.txt`.
- Suite PlayMode (`TestRunnerApi`, filtre assembly `RoadRage.Tests.PlayMode`) : **29/29 verts** apres la revue adversariale -- `Story11MainMenuLaunchPlayModeTests` (3) et `Story12LobbyShellPlayModeTests` (6) sans regression, `Story13CharacterSetupPlayModeTests` (20) verts. Trace : `story-1-3-playmode-results.txt`.
- Comptage des traces : le collecteur `ICallbacks` compte un cas de test quand `ITestResultAdaptor.Test.IsSuite` est faux. Compter les feuilles via `HasChildren` faisait passer la racine d'un run vide pour un cas reussi, ce qui a produit une trace mensongere (`PASSED=0 ... [ok] RRS`) lors d'un run PlayMode revenu a `testCaseCount=0`. Les traces portent desormais `CASES=`, le decompte recalcule et le decompte rapporte par l'API, cote a cote.
- **Contre-verification du coordinateur (step-03)** : la premiere trace EditMode enregistree etait vide (`PASSED=0 ... [ok] RRS`) -- le collecteur ne comptait que le noeud racine, donc le "49/49" n'etait pas prouve par l'artefact. Suite relancee avec un collecteur corrige (comptage sur `!r.Test.IsSuite`, un `assemblyNames` explicite) : **49/49 verts, 49 cas nommes**, trace remplacee. Le resultat annonce etait bon, la preuve ne l'etait pas.
- Couverture de la matrice I/O : chaque ligne est exercee par un vrai clic sur un bouton serialise (`ClickSerializedButton`) apres `PressPlayAndEnterLobbyShell` puis un clic reel sur `characterSetupButton` -- jamais d'invocation de methode privee (Patch 1 de la Story 1.2). Lignes couvertes : ouverture (champ + premier personnage + teinte), nom valide (depot ecrit, notice Info, ecran ferme, `SceneManager.sceneCount` inchange), nom vide, nom d'espaces, nom trop long (40 caracteres), caracteres interdits (`a<b>`), bordures normalisees (`"  Kenan  "` -> `Kenan`), cycle de personnage (+ wrap-around), catalogue absent **et** catalogue vide (cycle et Confirmer inertes, validation visible, aucune notice, aucune exception), reouverture apres confirmation, fermeture sans confirmation. Un test supplementaire (`ConfirmedProfileSurvivesSceneChange`) recharge la scene et verifie que le profil survit -- c'est le contrat consomme par l'entree monde de la Story 1.5.
- Verification manuelle reelle en Play mode depuis `Bootstrap` (`Unity_ManageScene` Load `Bootstrap`, `Unity_ManageEditor` Play) : la scene active devient `MainMenuLobby`, puis clics reels via `Button.onClick.Invoke()` sur les boutons de scene --
  - ouverture : `panelActive=True character='Rookie' tint=RGBA(0.300, 0.620, 0.950, 1.000) validationVisible=False`
  - nom vide : `validationVisible=True msg='Nom requis : saisis un nom de joueur.' hasProfile=False stillOpen=True`
  - caracteres interdits : `msg='Caracteres interdits : lettres, chiffres, espace, - et _ uniquement.' hasProfile=False`
  - cycle : `character='Veteran' tint=RGBA(0.900, 0.450, 0.200, 1.000) index=1`
  - confirmation de `"  Kenan  "` : `hasProfile=True name='Kenan' characterId='char_veteran' screenClosed=True`, notice `Info : Profil enregistre : Kenan (Veteran)`
  - reouverture : `nameField='Kenan' character='Veteran' index=1 validationVisible=False`
  - fermeture : `screenClosed=True profileKept=True`
  - console : **zero erreur et zero avertissement** sur toute la sequence.
- Verification de visibilite reelle (mesure des `RectTransform` en Play mode, Game view 813x714) : `CharacterButton x[6..204] y[263..337] fullyOnScreen=True`, sans chevauchement avec la colonne de boutons du lobby (`x[209..604]`). Raycast `GraphicRaycaster` au centre du bouton : `top=CharacterButtonLabel` -- le bouton est donc cliquable a la souris, pas seulement par code. Panneau ouvert, le raycast au centre de `ConfirmButton` remonte `ConfirmButtonLabel` (depth 26) puis `ConfirmButton` puis `CharacterPanel` (depth 15) avant les elements de `SetupPanel` (depth 11 et moins) : la superposition opaque du panneau plein ecran fonctionne comme prevu.
- `git status` apres implementation : seuls les fichiers des taches et leurs `.meta`, plus `MainMenuLobby.unity`, l'asmdef PlayMode et les deux fichiers de resultats de tests ; aucune scene `Dev_*` ni `EditorBuildSettings.asset` modifie. `Prop_Barrel.blend` et `sprint-status.yaml` etaient deja modifies avant cette story.
### Patchs de revue adversariale appliques (2026-09-08)

Seize patchs issus de la revue, sans boucle de retour sur l'intention gelee. Comptes finaux apres application : **EditMode 65/65 verts, PlayMode 29/29 verts** (relances integrales via `TestRunnerApi`, aucun echec, aucune assertion relachee).

**Code de production**

- **Patch 1 (id de personnage vide atteignant le depot)** : `TryValidate` ne faisait que loguer un avertissement en `Awake`, et `HandleConfirmRequested` ne testait que `character == null`. Un `CharacterDef` a l'id blanc ecrivait donc un profil dont `CharacterId.IsEmpty` est vrai, en contradiction directe avec le critere d'acceptation "id minuscule non vide". Une garde unique `TryGetUsableCharacter` refuse desormais l'entree nulle **et** l'id vide, avec un libelle de validation dedie. Couvert par `CatalogWithEmptyCharacterIdRefusesConfirmationWithVisibleValidation`.
- **Patch 2 (id borde d'espaces)** : `" char_rookie"` passait `IsNullOrWhiteSpace` et la comparaison de casse, puis ces espaces voyageaient dans le `DefinitionId`. `TryValidate` refuse maintenant tout `rawId != rawId.Trim()`.
- **Patch 3 (echec silencieux)** : quand le depot est indisponible, la confirmation loguait et sortait sans rien afficher. Elle publie desormais `ProfileStoreUnavailableValidation`, comme tous les autres refus.
- **Patch 4 (champ de nom non reinitialise)** : `SetName` n'etait appele que s'il existait un profil. Un nom refuse puis abandonne reapparaissait a la reouverture sans son message d'erreur. La couche App reecrit maintenant le champ a chaque ouverture (nom confirme, sinon chaine vide). Couvert par `ReopeningAfterAbandonedRejectionClearsTheNameField`.
- **Patch 5 (bornes recopiees)** : `TooShortError`/`TooLongError` codaient "2" et "20" en dur pendant que `MinLength`/`MaxLength` les definissaient, et les tests comparaient aux constantes -- la derive aurait ete invisible. Les messages sont derives des bornes (`static readonly`), et `PlayerNameValidatorBoundMessagesQuoteTheActualBounds` verrouille la derivation.
- **Patch 6 (mutateur mort)** : `PlayerProfileStore.Clear()` supprime -- aucun appelant, aucun test, et il ne levait pas `ProfileChanged`, donc le brancher plus tard aurait desynchronise ses abonnes en silence. Meme decision que le Patch 2 de la Story 1.2 sur `BackRequested`. `ProfileChanged` est conserve : le spec le demande comme point d'extension pour la Story 1.5. `PlayerProfileStoreExposesNoSilentMutator` interdit desormais tout nouveau mutateur public muet.
- **Patch 7 (surface publique inutile)** : `CharacterSetupScreen.GetRawName()` repasse en prive ; la couche App recoit le texte brut dans la charge utile de `ConfirmRequested`.

**Tests**

- **Patch 8 (bornes a sens unique)** : `MinLength` exact et `MaxLength + 1` n'etaient pas couverts, et le cas trop long utilisait 40 caracteres, loin de la borne. **Preuve empirique** : la mutation citee par le reviewer (`trimmed.Length < MinLength` -> `<= MinLength`) a ete rejouee. Avant patch elle ne faisait echouer aucun test ; apres patch elle fait echouer `PlayerNameValidatorAcceptsNameAtExactMinLength` (`CASES=65 PASSED=64 FAILED=1`). Mutation annulee ensuite, suites relancees vertes.
- **Patch 9 (branches de refus jamais observees)** : aucune des branches de rejet de `TryValidate` n'etait vue retourner `false`. Onze cas EditMode construisent desormais des `CharacterCatalog`/`CharacterDef` en memoire pour la liste vide, l'entree nulle, l'id vide, l'id borde d'espaces, l'id non minuscule et l'id duplique, en verifiant `false` et le message attendu.
- **Patch 10 (emplacement nul dans un catalogue non vide)** : `Count` compte les emplacements, donc la garde `Count > 0` passait et seule la garde d'entree nulle empechait une `NullReferenceException`. `CatalogWithNullEntryLeavesCycleAndConfirmInertWithVisibleValidation` injecte une liste d'un seul element nul.
- **Patch 11 (depot jamais ecrit)** : les tests de refus prouvaient "inchange" sur un depot vide. `RejectedNameLeavesAnAlreadyConfirmedProfileStrictlyUnchanged` confirme d'abord un profil puis verifie que le refus laisse l'instance et les deux champs identiques ; `ConfirmingAgainOverwritesBothProfileFields` prouve le symetrique, faute de quoi "inchange" serait vrai pour la mauvaise raison.
- **Patch 12** : assertion `published Is.Null` ajoutee a `MissingCatalogLeavesCycleAndConfirmInertWithVisibleValidation`, alignee sur son jumeau catalogue vide.
- **Patch 13** : `CyclingWrapsAroundToFirstCatalogEntry` verifie `catalog.Count >= 2` avant la boucle, sinon le test serait vide de sens.
- **Patch 14 (gardes de frontiere contournables)** : les tests cherchaient une sous-chaine dans le JSON brut des asmdef. Unity ecrit `"GUID:<hash>"` quand l'option "Use GUIDs" de l'Inspector est active : le test serait alors passe inconditionnellement alors meme que la reference interdite existe. Les references sont desormais parsees puis resolues via `AssetDatabase.GUIDToAssetPath` vers le `name` de l'asmdef cible, sur les deux tests de frontiere.
- **Patch 15 (superposition non verifiee)** : l'ordre de fratrie ne prouve ni l'opacite ni le blocage des clics -- supprimer le fond `Image` gardait la suite verte. `CharacterPanelHasOpaqueFullScreenRaycastBlockingBackground` verifie `raycastTarget`, `color.a == 1` et les ancres plein ecran.
- **Patch 16 (controles atteignables)** : tous les clics passent par `onClick.Invoke()`, ce qui court-circuite le layout. `Story13ControlsStayInsideTheCanvasSoAPlayerCanReachThem` verifie que les quatre coins monde du bouton Personnage et des controles du panneau tiennent dans le rectangle du canvas. **Preuve empirique** : replacer `CharacterButton` a son placement initial hors ecran `(-360, -310)` fait maintenant echouer ce test (`CASES=29 PASSED=28 FAILED=1`, message `characterSetupButton : coin 0 hors du canvas en x`), la ou toute la suite restait verte avant. Mutation annulee, suites relancees vertes. Portee volontairement limitee aux controles de la Story 1.3 : les elements des Stories 1.1 et 1.2 debordent deja pour une cause anterieure, laissee a une passe de layout dediee.
- **Relayout du `CharacterPanel` induit par le Patch 16** : les controles s'etendaient de +425 a -460 unites canvas, ce qui exigeait une demi-hauteur de canvas de 460 et aurait rendu l'assertion dependante de la taille de la Game view. Le panneau tient desormais dans [-380..385] avec `max|x| = 450`, soit tout format jusqu'a environ 2,5:1. Aucun element des Stories 1.1 / 1.2 n'a bouge.

**Spec**

- **Patch 17** : la section Verification citait `story-1-3-editmode-results.xml` comme sortie du repli batch-mode alors que les resultats reels citent les traces `.txt`. La ligne de repli precise maintenant qu'elle n'a pas ete empruntee et que le `.xml` n'existe que si ce repli est un jour utilise.

- Note d'outillage (pour les stories suivantes) : `Unity_RunCommand` refuse tout script contenant `File.Delete` / `Directory.CreateDirectory` ("User interactions are not supported for MCP tool calls") et interdit `System.Reflection`. Pour recolter des resultats de tests, enregistrer le collecteur `ICallbacks` dans un premier appel puis lancer `TestRunnerApi.Execute` dans un second : les deux dans le meme appel declenchent le meme refus. `TestMode.PlayMode` a rendu `testCaseCount=0` a deux reprises (meme flakiness qu'en Story 1.1) ; passer `assemblyNames = { "RoadRage.Tests.PlayMode" }` dans le `Filter` a rendu le lancement fiable.

### Contre-verification finale du coordinateur (step-04, 2026-09-08)

Les comptes annonces apres patchs ont ete revalidas independamment, sans se fier au rapport de l'agent :

- **EditMode : confirme en direct.** Relance complete depuis la session coordinatrice : `CASES=65 PASSED=65 FAILED=0 OTHER=0 | API PassCount=65 FailCount=0 STATE=Passed` -- les deux comptages (recalcule sur `!r.Test.IsSuite` et celui de l'API) concordent.
- **Patchs de production : verifies dans les sources**, pas seulement declares -- `PlayerProfileStore.Clear()` absent, `CharacterSetupScreen.GetRawName()` prive, garde `rawId != rawId.Trim()` presente, garde `candidate.Id.IsEmpty` centralisee dans `TryGetUsableCharacter`, `ShowValidation(ProfileStoreUnavailableValidation)` sur le chemin depot indisponible, messages d'erreur derives de `MinLength`/`MaxLength`.
- **PlayMode : confirme indirectement.** Quatre `TestRunnerApi.Execute` lances depuis la session coordinatrice ont rendu `CASES=0` sans aucune erreur console -- les callbacks d'un assembly compile dynamiquement se perdent au rechargement de domaine declenche par l'entree en Play mode. Ce n'est pas un echec de test. La preuve tient par deux canaux independants : `RetrieveTestList(TestMode.PlayMode)` enumere **exactement 29 cas** depuis cette meme session, et la trace `story-1-3-playmode-results.txt` contient **29 lignes de cas nommes** -- un run a zero cas ne peut produire ni l'un ni l'autre. S'y ajoutent les deux runs de mutation rapportes a 28/29 avec un test nomme en echec et son message d'assertion, impossibles sur un run vide.
- **Consequence pour les stories suivantes** : ne jamais accepter un compte de tests sur la seule foi d'un en-tete `STATE=Passed`. Exiger des cas nommes, et recouper avec `RetrieveTestList` quand `Execute` est suspect.

## Suggested Review Order

**Couture App -- le coeur du changement**

- Point d'entree : la seule classe qui valide, ecrit le profil et pilote les libelles.
  [`PlayerProfileFlowController.cs:46`](../../Assets/RoadRage/App/Players/PlayerProfileFlowController.cs#L46)

- Garde unique partagee par ouverture, cycle et confirmation : refuse catalogue et id inutilisables.
  [`PlayerProfileFlowController.cs:229`](../../Assets/RoadRage/App/Players/PlayerProfileFlowController.cs#L229)

- Chemin d'ecriture du profil : validation, ecriture, notice, fermeture -- aucun echec silencieux.
  [`PlayerProfileFlowController.cs:169`](../../Assets/RoadRage/App/Players/PlayerProfileFlowController.cs#L169)

- Ouverture : l'App reecrit toujours le champ de nom, jamais un reste de saisie refusee.
  [`PlayerProfileFlowController.cs:104`](../../Assets/RoadRage/App/Players/PlayerProfileFlowController.cs#L104)

**Contrat de persistance vers la Story 1.5**

- Le depot nait sur l'objet DontDestroyOnLoad : seul passage vers l'entree monde.
  [`RoadRageBootstrap.cs:62`](../../Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs#L62)

- Depot de session C# pur, sans type Unity, avec evenement pour la Story 1.5.
  [`PlayerProfileStore.cs:26`](../../Assets/RoadRage/Features/Players/PlayerProfileStore.cs#L26)

**Donnees auteur et identite stable**

- Id expose en DefinitionId : la forme que l'Epic 2 consommera telle quelle.
  [`CharacterDef.cs:32`](../../Assets/RoadRage/Features/Players/CharacterDef.cs#L32)

- Garde de coherence des donnees auteur : nul, vide, non minuscule, padde, duplique.
  [`CharacterCatalog.cs:85`](../../Assets/RoadRage/Features/Players/CharacterCatalog.cs#L85)

- Regle de nom testable sans Unity, messages derives des constantes de bornes.
  [`PlayerNameValidator.cs:28`](../../Assets/RoadRage/Features/Players/PlayerNameValidator.cs#L28)

**Frontiere UI -- l'ecran ignore tout du feature Players**

- L'ecran n'echange que des primitives et n'ecrit aucun etat de jeu.
  [`CharacterSetupScreen.cs:101`](../../Assets/RoadRage/Features/UI/CharacterSetupScreen.cs#L101)

- Nouveau point d'entree depuis la coquille de lobby, patron Raise* de la Story 1.2.
  [`LobbyShellScreen.cs:90`](../../Assets/RoadRage/Features/UI/LobbyShellScreen.cs#L90)

**Tests -- les gardes ajoutees par la revue**

- Un controle que les tests cliquent doit etre atteignable par un joueur.
  [`Story13CharacterSetupPlayModeTests.cs:589`](../../Assets/RoadRage/Tests/PlayMode/Story13CharacterSetupPlayModeTests.cs#L589)

- La superposition opaque est verrouillee par un test, plus seulement par un raycast manuel.
  [`Story13CharacterSetupTests.cs:490`](../../Assets/RoadRage/Tests/EditMode/Story13CharacterSetupTests.cs#L490)
