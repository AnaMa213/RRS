---
title: "Story 4.5 : Profil Steam persistant et selection de personnage dans le menu principal"
type: "feature"
created: "2026-09-13"
status: "done"
review_loop_iteration: 1
context: []
baseline_commit: "efae8ad36ec18bd14848179e58d79f8e97c04dc9"
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** La selection de personnage passe encore par l'ecran manuel de la Story 1.3 (saisie de pseudo + confirmation), et le profil est purement session : il disparait au redemarrage et n'a aucun lien avec l'identite Steam.

**Approach:** Faire du menu principal la seule surface de selection : au premier lancement, un profil local persistant est cree depuis l'identite Steam avec Rookie par defaut ; le joueur choisit Rookie ou Veteran sur un apercu 3D inspectable et le choix est ecrit sur disque ; le flux manuel 1.3 est retire. Override humain du 2026-09-13 : si Steam est indisponible, le menu reste utilisable avec une identite en memoire non persistee et une erreur Steam visible, et aucun fichier n'est ecrit.

## Boundaries & Constraints

**Always:** Le seul ecrivain du profil reste la couche App via `PlayerProfileStore.Set`; aucun mutateur nouveau. Persistant = id du compte Steam proprietaire + nom affichable normalise + choix cosmetique, rien d'autre : le fichier appartient a un compte, jamais au poste. Le profil est resolu une seule fois a l'ouverture du menu, jamais re-resolu ensuite (les tests et le run injectent encore le store a la main). La tentative d'initialisation Steam devient une etape idempotente du menu, sans regression du flux lobby. `Features/UI` ne recoit que des primitives (`string`, `Color`, `GameObject`) et ne reference jamais `Features.Players`/`Features.Online`. `PlayerNameValidator.TryNormalize` reste l'unique normalisation de nom, nom Steam compris. Toute erreur est visible dans le menu, jamais seulement par la couleur.

**Ask First:** Toute modification de la chaine payload / `CharacterId` / spawn / presentation (perimetre 4.6). Tout repli d'identite autre que « Rookie en memoire, non persiste ». Tout champ persistant au-dela de l'identite Steam et du choix cosmetique. Toute nouvelle reference d'asmdef.

**Never:** Champ de nom libre, confirmation, ecran de gestion de profil, compte maison, backend. Ecrire sur disque quand Steam est indisponible. Persister monnaie, item, vie, siege, etat de lobby ou de session. Renommer `char_rookie`/`char_veteran` ou modifier `CharacterCatalog`, `CharacterDef`, `PreviewPrefab`. Modifier le gate `HasProfile`, `PublishLocalProfile`, `RunFlowController`, `NetworkedPlayerPresentation`. Reecrire les Stories 4.1-4.4 (revues laissees en dette).

## I/O & Edge-Case Matrix

| Scenario                   | Input / State                                               | Expected Output / Behavior                                                                        | Error Handling                                                      |
| -------------------------- | ----------------------------------------------------------- | ------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| Premier lancement          | Steam disponible, aucun fichier profil                      | profil cree depuis l'identite Steam, `char_rookie`, fichier ecrit, libelle Rookie visible         | N/A                                                                 |
| Relance apres choix        | fichier valide `char_veteran` appartenant au compte courant | Veteran restaure, sans confirmation ni ecran intermediaire                                        | N/A                                                                 |
| Autre compte Steam         | fichier valide appartenant a un autre `steamId`             | profil recree depuis l'identite courante, Rookie par defaut, fichier repris par le compte courant | aucune exception ; le choix du compte precedent n'est jamais herite |
| Changement de choix        | joueur selectionne Veteran                                  | store + fichier mis a jour, apercu et libelle suivent                                             | N/A                                                                 |
| Steam indisponible         | client absent / `DllNotFoundException`                      | menu utilisable, Rookie en memoire, erreur Steam visible                                          | aucun fichier ecrit                                                 |
| Fichier corrompu           | JSON invalide ou tronque                                    | fichier ignore, profil recree depuis Steam, ecriture corrective                                   | aucune exception remontee                                           |
| Id inconnu                 | `characterId` absent du catalogue                           | repli sur `char_rookie`, fichier corrige                                                          | aucune exception                                                    |
| Nom Steam non normalisable | vide, >20 caracteres ou caracteres interdits                | repli sur le nom d'affichage du personnage                                                        | jamais de nom brut dans le store                                    |

</frozen-after-approval>

## Code Map

- `Assets/RoadRage/Features/Players/PlayerProfile.cs`, `PlayerProfileStore.cs:9-45` -- verites existantes : store session, seul mutateur `Set`, aucun `Clear`.
- `Assets/RoadRage/Features/Players/PersistentPlayerProfileRecord.cs` (`displayName`, `characterId`, `steamId`) et `PlayerProfileFileStore.cs` (`TryLoad(steamId, out profile)`, `TrySave(profile, steamId)`) -- le fichier appartient a un compte Steam precis : ni lecture ni ecriture sans proprietaire, et un fichier d'un autre compte n'est jamais adopte. Le supprimer ou le renommer laisse le compte precedent sans recours (un seul fichier par machine) : arbitrage assume, a revoir si le multi-compte devient courant.
- `Assets/RoadRage/Features/Players/CharacterCatalog.cs:36`, `CharacterDef.cs` (`Id`/`DisplayName`/`PreviewTint:48`/`PreviewPrefab`) -- ids et visuels Rookie/Veteran ; catalogue **partage avec le roster du lobby** (guid `090dcb6cd5516654e99a45174314c148`).
- `Assets/RoadRage/Features/Players/PlayerNameValidator.cs:28` -- normalisation reutilisee pour le nom Steam ; ses tests EditMode survivent.
- `Assets/RoadRage/Features/Online/ISteamPlatform.cs`, `FacepunchSteamPlatform.cs:10-39` -- seule frontiere SDK ; `SteamClient.SteamId`/`Name` disponibles (Facepunch.Steamworks 2.3.2). `OnlineServicesBootstrapService` cache son `ISteamPlatform`. **`ISteamPlatform` doit rester inchange** : cinq `FakeSteamPlatform` prives l'implementent (`Story21OnlineServicesBootstrapTests.cs:192`, `Story22...:147`, `Story23...:185`, `Story24...:214`, `Story28...:186`) ; l'identite arrive donc par une interface dediee, exposee par `RoadRageBootstrap` qui detient deja l'instance concrete.
- `Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs:38,76,97,101-102` -- singleton `DontDestroyOnLoad`, porte `Profiles` et `OnlineServices`. Steam n'est initialise qu'a l'ouverture du lobby (`LobbyFlowController.cs:116`).
- `Assets/RoadRage/Features/UI/MainMenuScreen.cs:17-142` -- uGUI/TMP, `menuPanel`/`setupPanel`, events `PlayRequested`/`QuitRequested`/`BackRequested`, `ShowMenu`/`ShowSetupPlaceholder`/`SetPanelActive` ; pont App unique : `App/MainMenu/MainMenuFlowController.cs:11-56`.
- `Assets/RoadRage/App/Players/PlayerProfileFlowController.cs:104-198` et `Features/UI/CharacterSetupScreen.cs:16-160` -- flux manuel a retirer ; l'ecran est le modele a repliquer (primitives seulement, aucun type Players).
- `Assets/RoadRage/Features/UI/LobbyShellScreen.cs:28-62` -- `characterSetupButton:46` et `CharacterSetupRequested:62` a retirer.
- `Assets/RoadRage/App/Scenes/MainMenuLobby.unity` -- `Canvas:136`, `MainMenuScreen:151`, `MenuPanel`, `SetupPanel:8741`, `CharacterPanel:5143`, `PlayerProfileFlow:4555`, `LobbyFlow:518` ; une seule camera (`:1576`), **aucun `RenderTexture` existant** : l'apercu 3D est du terrain neuf.
- Tests : `Tests/EditMode/Story13CharacterSetupTests.cs` (`:207-432` catalogue/store/validator survivent ; `:437-560` flux manuel a adapter/retirer), `Tests/PlayMode/Story13CharacterSetupPlayModeTests.cs` (a supprimer), `Story16Epic1PlayableCheckpointTests.cs:55-159` et sa variante PlayMode (a adapter), `Story11MainMenuLaunchTests.cs:86` (garde de champs serialises).
- Conventions : fixtures `public sealed`, namespace `RoadRage.Tests.<Mode>`, chaque fixture charge sa scene, fichiers `Story45<StoryTitlePascalCase>Tests.cs` / `...PlayModeTests.cs`.

## Tasks & Acceptance

**Execution:**

- [x] `Assets/RoadRage/Features/Online/ISteamIdentitySource.cs`, `FacepunchSteamPlatform.cs` -- interface dediee (nom + id Steam locaux) implementee par la seule classe qui touche le SDK, `ISteamPlatform` inchange -- evite de casser les cinq fakes de test existants.
- [x] `Assets/RoadRage/Features/Players/PersistentPlayerProfileRecord.cs`, `PlayerProfileFileStore.cs` -- DTO `[Serializable]` (`displayName`, `characterId`, `steamId`) + port fichier JSON a chemin injecte, sans exception, ou toute lecture/ecriture exige l'id du compte proprietaire -- `PlayerProfile` reste pur et testable sur un dossier temporaire.
- [x] `Assets/RoadRage/Features/Players/PlayerProfileBootstrapService.cs` -- resoudre le profil (fichier valide, sinon creation Steam + ecriture, repli `char_rookie`, nom normalise) -- politique pure, testable sans Steam.
- [x] `Assets/RoadRage/Features/UI/MainMenuScreen.cs`, `MainMenu/MenuCharacterPreview.cs` -- apercu 3D inspectable (RenderTexture + rotation locale) et controles Rookie/Veteran en primitives -- respecte l'asmdef UI.
- [x] `Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs` -- nouvel unique ecrivain : init Steam idempotente, resolution unique a l'ouverture, `Profiles.Set`, sauvegarde sur selection -- remplace le flux 1.3.
- [x] `Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs` -- chemin de fichier profil configurable (defaut `Application.persistentDataPath`) -- seam requis par les tests PlayMode.
- [x] `Assets/RoadRage/Features/UI/LobbyShellScreen.cs` -- retirer le controle de selection du lobby -- le lobby redevient lecture seule.
- [x] `Assets/RoadRage/App/Players/PlayerProfileFlowController.cs`, `Assets/RoadRage/Features/UI/CharacterSetupScreen.cs` (+ `.meta`) -- supprimer le flux manuel -- plus aucun mecanisme de profil manuel.
- [x] `Assets/RoadRage/App/Scenes/MainMenuLobby.unity` -- retirer les objets du flux manuel, cabler apercu et controles sur `MenuPanel` (couche `CharacterPreview` ajoutee, culling mask de la Main Camera ajuste) -- sinon le menu n'expose rien.
- [x] `Assets/RoadRage/Tests/EditMode/Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests.cs`, `Assets/RoadRage/Tests/PlayMode/Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests.cs` -- couvrir toute la matrice (relance simulee sur le meme fichier temporaire, corrompu, id inconnu, sans Steam) plus le parcours menu et l'absence de controle au lobby -- preuve bout en bout.
- [x] `Tests/EditMode/Story13CharacterSetupTests.cs`, `Tests/PlayMode/Story13CharacterSetupPlayModeTests.cs`, `Tests/EditMode/Story16Epic1PlayableCheckpointTests.cs`, `Tests/PlayMode/Story16Epic1PlayableCheckpointPlayModeTests.cs`, `Tests/EditMode/Story11MainMenuLaunchTests.cs`, `Tests/PlayMode/Story15EmptyMapEntryPlayModeTests.cs`, `Tests/PlayMode/Story12LobbyShellPlayModeTests.cs` -- retirer/adapter les tests du flux manuel, garder catalogue/store/validator, et retirer les cas desormais inatteignables « Start Game sans profil » (le menu publie toujours un profil) -- evitent une suite rouge trompeuse.

**Ecarts d'implementation (a valider en revue) :**

- Le `RenderTexture` de l'apercu est cree au runtime par `MenuCharacterPreview` (camera + banc dedies en code, couche `CharacterPreview`) au lieu d'un asset authored : moins de YAML a maintenir, mais aucun reglage authorable dans l'Inspector.
- Le seam de test est la propriete statique `PlayerProfileFileStore.DefaultFilePath` (lue par `RoadRageBootstrap.Awake`), et non une propriete de chemin sur le bootstrap comme l'envisageait la tache.
- Le libelle du menu presente le **personnage** selectionne (`character.DisplayName`), pas le nom du joueur : c'est le choix courant que l'UX demande de rendre lisible. Le nom du joueur reste porte par le profil et publie au roster du lobby.

**Acceptance Criteria:**

- Given aucun fichier profil et Steam disponible a l'ouverture du menu, when le menu s'ouvre, then un profil persistant est cree depuis l'identite Steam avec Rookie par defaut, sans champ de nom, confirmation ni ecran de gestion.
- Given un profil persistant existant, when le menu s'ouvre, then le choix enregistre est restaure sur l'apercu 3D et le modifier l'ecrit sur disque sans etape intermediaire.
- Given le joueur choisit Veteran, when il relance l'application, then Veteran est encore selectionne.
- Given Steam indisponible, when le menu s'ouvre, then Rookie est utilisable en memoire, l'erreur Steam reste visible et aucun fichier profil n'est ecrit.
- Given une selection faite dans le menu, when le joueur entre dans le lobby, then aucun controle du lobby ne permet de la modifier.

## Spec Change Log

### Iteration 1 — revue risk-scaled du 2026-09-13

**Declencheur :** la revue du diff a trouve que `MainMenuProfileFlowController` utilisait `resolution.ShouldPersist` comme _autorisation_ d'ecrire, alors que ce drapeau signifie seulement « la resolution courante exige une ecriture corrective ». Apres une relance normale (fichier deja valide), il vaut faux : tout changement de personnage etait publie dans le depot de session et jamais ecrit, donc perdu au redemarrage — exactement ce que la matrice gelee (« Changement de choix : store + fichier mis a jour ») et les criteres 2 et 3 exigent.

**Amende (patch, hors bloc gele) :** `canPersistProfile` derive desormais de la disponibilite de l'identite Steam, et l'ecriture initiale est conditionnee a `canPersistProfile && resolution.ShouldPersist`.

**Etat mauvais evite :** un joueur Steam connecte relancant le jeu voyait ses changements de personnage silencieusement perdus, avec en plus un log annoncant « (session) ».

**KEEP (a ne pas perdre a la re-derivation) :** resolution unique a l'ouverture du menu, `PlayerProfileStore.Set` comme seule porte d'entree du depot, normalisation du nom par `PlayerNameValidator`, aucune ecriture quand Steam est indisponible, service de resolution pur sans dependance a `Features.Online`.

**Test renforce :** le test PlayMode asserte desormais le **contenu relu** du fichier apres chaque changement de selection, et non sa seule existence — l'existence etait deja vraie apres la creation initiale, ce qui rendait la regression invisible.

**Constat escalade (non resolu ici) :** l'identite Steam n'est pas persistee (`PersistentPlayerProfileRecord` ne porte que `displayName` + `characterId`, le `steamId` est jete). Deux comptes Steam sur le meme compte Windows partagent donc `player-profile.json`. Le bloc gele dit « identite Steam + choix cosmetique » sans trancher entre nom d'affichage et identifiant de compte, et la matrice n'a pas de ligne « autre compte » : arbitrage humain requis avant d'ajouter un champ persistant.

**Constat corrige (documentation) :** `epic-4-context.md` affirmait encore « aucun repli hors ligne », en contradiction avec l'override humain approuve ; la phrase a ete alignee (a reporter aussi dans les artefacts de planification, sinon une recompilation du contexte la reintroduira).

### Iteration 1 — resolution de l'arbitrage (2026-09-13)

**Decision humaine :** [B] — le profil persistant doit etre rattache au compte Steam proprietaire.

**Amende du bloc gele (accord explicite requis et donne) :** la ligne « Persistant = identite Steam + choix cosmetique » precise desormais « id du compte Steam proprietaire + nom affichable normalise + choix cosmetique, le fichier appartient a un compte, jamais au poste », et la matrice gagne la ligne « Autre compte Steam ».

**Re-derivation :** `PersistentPlayerProfileRecord` porte `steamId` ; `PlayerProfileFileStore.TryLoad(steamId, out profile)` / `TrySave(profile, steamId)` refusent toute operation sans proprietaire et ne restaurent jamais le fichier d'un autre compte ; `PlayerProfileBootstrapService.Resolve(steamAvailable, steamDisplayName, steamId)` ; `MainMenuProfileFlowController` capture l'id Steam (`ToInvariantString`) et le transmet a la resolution comme aux ecritures de selection.

**Etat mauvais evite :** sur un poste Windows partage, le second compte Steam heritait du nom affichable et du personnage du premier, et ce nom etait republie dans le roster du lobby et le HUD.

**KEEP (a ne pas perdre) :** resolution unique a l'ouverture du menu ; `PlayerProfileStore.Set` seule porte d'entree du depot ; normalisation du nom par `PlayerNameValidator` ; aucune lecture ni ecriture quand Steam est indisponible ; service de resolution pur, sans dependance a `Features.Online` ; test PlayMode qui asserte le contenu relu du fichier et non sa seule existence.

**Compromis assume :** un seul fichier `player-profile.json` par machine, donc le choix du compte precedent est ecrase quand un autre compte joue. Un fichier par compte (`player-profile-<steamId>.json`) serait la suite naturelle si le multi-compte devient courant ; ce n'est pas le cas du projet aujourd'hui.

### Pass 2 — revue risk-scaled du 2026-09-13

**Declencheur :** second passage de revue apres la re-derivation du pass 1. Trois constats, tous causes par cette story ; les deux corrections du pass 1 ont ete validees par le relecteur.

**Patch 1 (livrable visuel casse) :** `MenuCharacterPreview` posait la camera sur le GameObject du banc ; ses `localPosition` deplacaient donc le banc entier, ancrage compris, et la camera restait confonde avec le modele — l'apercu ne montrait que l'interieur du mesh, et l'isolement annonce a 3000 unites n'existait pas. La camera vit desormais sur un enfant dedie (`PreviewCamera`) : les `localPosition` sont relatifs au banc, le modele reste a l'ancrage et l'isolement redevient effectif.
**KEEP :** apercu monte au runtime (camera + RenderTexture + couche dediee), rotation locale au glisser, teinte par `MaterialPropertyBlock`, RawImage authored transparent puis remis a blanc pour ne pas colorer le modele.

**Patch 2 (test Story 1.5 invalide par la nouvelle garantie) :** le menu publiant desormais toujours un profil, le cas « Start Game sans profil » n'est plus atteignable depuis l'interface. `Story15EmptyMapEntryPlayModeTests.StartGameWithoutProfilePublishesVisibleWarningAndStaysInLobby` ne pouvait plus passer ; il est remplace par `MenuAlwaysPublishesAProfileBeforeLobbyEntry`, qui verrouille la nouvelle garantie, tandis que la garde de source EditMode `LobbyStartGameRequiresAConfirmedProfileBeforeLoadingRun` continue de proteger l'existence du refus dans `LobbyFlowController`.

**Patch 3 (robustesse) :** `HandleCharacterOptionRequested` n'avait pas le garde `profileBootstrap == null` present sur le chemin de resolution.

**Differe :** les fixtures PlayMode qui chargent `MainMenuLobby` sans rediriger `PlayerProfileFileStore.DefaultFilePath` (`Story11`, `Story12`, `Story15`, `Story21`, `Story22`) peuvent lire ou ecraser le profil reel du developpeur. Consigne dans `deferred-work.md` : la correction revient a l'extraction d'un helper de test partage plutot qu'a cinq retouches dans des fixtures d'autres stories.

**Enseignement (pour les prochaines stories) :** l'apercu 3D releve de la verification manuelle et n'etait couvert par aucun test, et la suite PlayMode complete n'a pas ete executee par l'agent. Les deux passes de revue ont trouve ce que la verification automatique seule ne voyait pas : ne pas considerer un lot comme pret sans la suite PlayMode complete ni le controle visuel.

## Design Notes

Fichier JSON unique (`player-profile.json`) ecrit via `JsonUtility` sur un DTO dedie : `PlayerProfile` reste get-only et `PlayerProfileStore.Set` reste l'unique porte d'entree, ce qui preserve le test `PlayerProfileStoreExposesNoSilentMutator`. Resolution en aller simple : identite Steam -> fichier valide -> sinon creation (nom normalise, `char_rookie`) + ecriture. Le service de resolution ne connait que des valeurs simples (Steam disponible ou non, nom) : `Features.Players` n'a donc aucune dependance sur `Features.Online`, et `ISteamPlatform` reste inchange pour garder les cinq `FakeSteamPlatform` compilables. `MenuCharacterPreview` instancie le `PreviewPrefab` du catalogue devant une camera dediee rendant dans un `RenderTexture`, teinte via `MaterialPropertyBlock` ; presentation locale pure, sans etat gameplay. Les suppressions (flux manuel) et la mise a jour de la scene doivent etre appliquees dans la meme passe, sinon la scene reference un composant absent.

## Verification

**Deja verifie le 2026-09-13 (agent, via le CLI Unity sur l'Editor ouvert) :**

- `recompile` -- `completed`, `failed=false`, `errors=[]`.
- Suite EditMode `Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests` -- **15/15 vertes**.
- Suite PlayMode `Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests` -- **2/2 vertes**. Steam etait en ligne : le chemin « identite Steam lue, nom normalise, profil ecrit » a donc reellement ete exerce (et non le repli).
- Une premiere execution affichait le nom du joueur au lieu du personnage dans le libelle du menu : le code a ete corrige, pas le test.
- `ThirdPersonCameraTests.MouseActionDrivesNativeOrbitWithoutFrameScaling` a echoue une fois puis est repasse (2/2) au relancement : flaky d'entree souris, sans lien avec cette story.
- `git diff --check` signale des espaces de fin dans `MainMenuLobby.unity` : c'est le style d'ecriture d'Unity (103 lignes du meme type deja presentes dans `HEAD`), non corrigeable sans reformater un fichier reserialise par l'editeur. Le reste du diff est propre.

**Suites completes executees par le developpeur le 2026-09-13 (checkpoint) :**

- EditMode `RoadRage.Tests.EditMode` -- tout vert, hors les deux echecs preexistants ci-dessus (la violation d'asmdef 4.4 et le flake camera), tous deux consignes dans `_bmad-output/implementation-artifacts/deferred-work.md`.
- PlayMode `RoadRage.Tests.PlayMode` -- **tout vert** apres les adaptations `Story12`/`Story15` et le correctif d'apercu. C'est la suite la plus exposee au changement de scene : `Story11`, `Story12`, `Story15`, `Story16`, `Story21` et `Story22` chargent toutes `MainMenuLobby`.
- Regression PlayMode du 2026-09-13 : `Story11MainMenuLaunchPlayModeTests`, `Story12LobbyShellPlayModeTests`, `Story16Epic1PlayableCheckpointPlayModeTests` et `Story44PassengerActionTwoMvpRunPlayModeTests` sont remontes rouges apres le pass 2, avec un message parent generique (« One or more child tests had errors »). Cause identifiee pour `Story12` : le menu publiant desormais toujours un profil, son test « Start Game refuse et ne charge aucune scene » chargeait reellement `MVP_Run` avant d'echouer, polluant l'etat de scene des fixtures suivantes. Le test est retire et la couverture deplacee vers `Story15` ; la suite est repassee verte. `Story44` ne passe jamais par le menu et reste hors perimetre de cette story.

**Verification manuelle a faire en Play Mode sur `MainMenuLobby` (non couverte par les tests) :**

- Apercu 3D inspectable a la souris, aucun champ de nom, choix conserve apres redemarrage, erreur Steam visible client ferme, aucun controle de selection dans le lobby.

## Suggested Review Order

**Profil persistant : la politique de resolution**

- Point d'entree : un aller simple, et rien n'est ecrit sans Steam.
  [`PlayerProfileBootstrapService.cs:52`](../../Assets/RoadRage/Features/Players/PlayerProfileBootstrapService.cs#L52)

- Le fichier appartient a un compte : celui d'un autre n'est jamais adopte.
  [`PlayerProfileFileStore.cs:58`](../../Assets/RoadRage/Features/Players/PlayerProfileFileStore.cs#L58)

- Ecriture refusee sans proprietaire : jamais de profil orphelin sur disque.
  [`PlayerProfileFileStore.cs:112`](../../Assets/RoadRage/Features/Players/PlayerProfileFileStore.cs#L112)

- Seule donnee persistee au-dela du nom et du cosmetique : l'id Steam.
  [`PersistentPlayerProfileRecord.cs:20`](../../Assets/RoadRage/Features/Players/PersistentPlayerProfileRecord.cs#L20)

**Couture App : le bug de persistance corrige en revue**

- Autorisation d'ecrire derivee de Steam, distincte de l'ecriture corrective.
  [`MainMenuProfileFlowController.cs:46`](../../Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs#L46)

- Resolution unique a l'ouverture, apres tous les Awake de la scene.
  [`MainMenuProfileFlowController.cs:111`](../../Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs#L111)

- Un changement de selection reecrit le fichier, meme apres une relance normale.
  [`MainMenuProfileFlowController.cs:169`](../../Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs#L169)

**Identite Steam et composition**

- Frontiere dediee : `ISteamPlatform` reste inchange pour les fakes existants.
  [`FacepunchSteamPlatform.cs:52`](../../Assets/RoadRage/Features/Online/FacepunchSteamPlatform.cs#L52)

- Composition root : une seule instance Steam pour le statut et l'identite.
  [`RoadRageBootstrap.cs:44`](../../Assets/RoadRage/App/Bootstrap/RoadRageBootstrap.cs#L44)

**Apercu 3D et surface de menu**

- Banc isole : camera sur son propre enfant, sinon elle voit l'interieur du modele.
  [`MenuCharacterPreview.cs:117`](../../Assets/RoadRage/Features/UI/MenuCharacterPreview.cs#L117)

- Instanciation, teinte et cadrage automatique du personnage selectionne.
  [`MenuCharacterPreview.cs:79`](../../Assets/RoadRage/Features/UI/MenuCharacterPreview.cs#L79)

- Rotation libre : presentation locale, aucun etat de jeu touche.
  [`MenuCharacterPreview.cs:98`](../../Assets/RoadRage/Features/UI/MenuCharacterPreview.cs#L98)

- L'ecran ne connait que deux emplacements opaques, jamais le catalogue.
  [`MainMenuScreen.cs:66`](../../Assets/RoadRage/Features/UI/MainMenuScreen.cs#L66)

- Seul ecrivain des libelles et du modele d'apercu : l'UI ne decide rien.
  [`MainMenuScreen.cs:166`](../../Assets/RoadRage/Features/UI/MainMenuScreen.cs#L166)

- Le lobby cesse d'etre une surface de selection et redevient lecture seule.
  [`LobbyShellScreen.cs:23`](../../Assets/RoadRage/Features/UI/LobbyShellScreen.cs#L23)

**Scene (YAML reserialise par Unity : diff volumineux, a lire en cible)**

- Nouvelles surfaces creees sur le panneau du menu principal.
  [`MainMenuLobby.unity:387`](../../Assets/RoadRage/App/Scenes/MainMenuLobby.unity#L387)

- Noeud du flux de profil, equivalent du `PlayerProfileFlow` retire.
  [`MainMenuLobby.unity:6615`](../../Assets/RoadRage/App/Scenes/MainMenuLobby.unity#L6615)

- Main Camera exclut la couche d'apercu, sans quoi elle verrait le banc.
  [`MainMenuLobby.unity:1338`](../../Assets/RoadRage/App/Scenes/MainMenuLobby.unity#L1338)

- Couche dediee nommee, avec repli code sur l'index 31 si elle disparait.
  [`TagManager.asset:16`](../../ProjectSettings/TagManager.asset#L16)

**Tests**

- Premier lancement : creation depuis Steam, Rookie par defaut, fichier relu.
  [`Story45…EditModeTests.cs:74`](../../Assets/RoadRage/Tests/EditMode/Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests.cs#L74)

- Relance : un fichier valide prime sur le nom Steam courant, sans reecriture.
  [`Story45…EditModeTests.cs:100`](../../Assets/RoadRage/Tests/EditMode/Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests.cs#L100)

- Autre compte Steam : heritage refuse, ni le nom ni le personnage.
  [`Story45…EditModeTests.cs:124`](../../Assets/RoadRage/Tests/EditMode/Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests.cs#L124)

- Forme du record verrouillee : identite et cosmetique, rien de plus.
  [`Story45…EditModeTests.cs:248`](../../Assets/RoadRage/Tests/EditMode/Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests.cs#L248)

- Ni lecture ni ecriture sans proprietaire, et aucun fichier orphelin.
  [`Story45…EditModeTests.cs:287`](../../Assets/RoadRage/Tests/EditMode/Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests.cs#L287)

- Cablage de scene du nouvel apercu et du flux de profil du menu.
  [`Story45…EditModeTests.cs:311`](../../Assets/RoadRage/Tests/EditMode/Story45PersistentSteamProfileAndMainMenuCharacterSelectionTests.cs#L311)

- Parcours reel du menu, persistance verifiee sur le contenu relu du fichier.
  [`Story45…PlayModeTests.cs:60`](../../Assets/RoadRage/Tests/PlayMode/Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests.cs#L60)

- Le lobby ne peut plus modifier la selection une fois le menu quitte.
  [`Story45…PlayModeTests.cs:130`](../../Assets/RoadRage/Tests/PlayMode/Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests.cs#L130)

- Helper de lecture qui rattache le fichier au compte local.
  [`Story45…PlayModeTests.cs:181`](../../Assets/RoadRage/Tests/PlayMode/Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests.cs#L181)

- Cas « Start Game sans profil » devenu inatteignable : la garantie le remplace.
  [`Story15…PlayModeTests.cs:42`](../../Assets/RoadRage/Tests/PlayMode/Story15EmptyMapEntryPlayModeTests.cs#L42)

- Meme retrait dans la coquille de lobby, avec sa justification ecrite.
  [`Story12…PlayModeTests.cs:57`](../../Assets/RoadRage/Tests/PlayMode/Story12LobbyShellPlayModeTests.cs#L57)

### Review Findings

- [x] [Review][Patch] Surface visible on profile-write failure [Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs:153] — `TryPersist` returns `false` for recoverable I/O failures, but both initial repair and later selection saves discard it; the session changes while the player receives no visible warning and loses the choice on restart.
- [x] [Review][Patch] Preserve the Steam-unavailable warning when returning to the menu [Assets/RoadRage/Features/UI/MainMenuScreen.cs:151] — `ShowMenu` clears the sole offline notice and the Back handler does not replay `Notices.LastNotice`, violating the requirement that the Steam error remains visible.
- [x] [Review][Patch] Repair persisted names after normalization [Assets/RoadRage/Features/Players/PlayerProfileBootstrapService.cs:78] — a valid-but-untrimmed stored display name is normalized in memory but `ShouldPersist` stays false, leaving the persistent record outside the specified normalized form indefinitely.
- [x] [Review][Patch] Verify rendered and inspectable character preview in PlayMode [Assets/RoadRage/Tests/PlayMode/Story45PersistentSteamProfileAndMainMenuCharacterSelectionPlayModeTests.cs:88] — current coverage asserts only a rig and RenderTexture exist; a blank/culling-broken or non-rotating preview remains green despite the acceptance criterion.
