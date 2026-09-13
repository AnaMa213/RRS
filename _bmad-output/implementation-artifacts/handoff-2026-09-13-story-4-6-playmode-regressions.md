# Handoff — Story 4.6 : regressions PlayMode non triees

Date : 2026-09-13
Spec : `_bmad-output/implementation-artifacts/spec-4-6-profile-freeze-session-payload-and-selected-character-spawn.md`
Baseline : `03419f428e3ebdb378b412894ccb6539732131e4`

## Etat

| Suite                              | Resultat                                                                                                          |
| ---------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| Recompilation Editor               | Non rapportee                                                                                                     |
| EditMode `RoadRage.Tests.EditMode` | Vert, hors les 2 echecs preexistants deja differes (violation d'asmdef Story 4.4, flake `ThirdPersonCameraTests`) |
| PlayMode `RoadRage.Tests.PlayMode` | **3 fixtures rouges**, cause racine inconnue                                                                      |

Fixtures rouges, telles que remontees par l'humain :

- `Story11MainMenuLaunchPlayModeTests` — 3 tests, 1 en echec (0,635 s)
- `Story16Epic1PlayableCheckpointPlayModeTests` — 1 test, 1 en echec (0,656 s)
- `Story44PassengerActionTwoMvpRunPlayModeTests` — 2 tests, 1 en echec (0,265 s)

Les trois messages recus sont identiques et generiques : `One or more child tests had errors`. Ce n'est pas une erreur enfant : c'est le noeud parent qui agrege. Aucune pile d'appels, aucun type d'echec, aucun nom de test enfant, aucun ordre d'execution.

## Ce qu'il faut aller chercher en premier

1. **L'erreur enfant, pas le parent.** Test Runner > clic sur le test rouge > le message et la pile. Trois informations changent le diagnostic :
   - `NullReferenceException` ou autre exception non geree ;
   - assertion echouee, avec le message et la valeur attendue ;
   - `LogAssert` : « Unhandled log message » ou « Expected log did not appear ».
2. **L'ordre d'execution des fixtures.** Le Test Runner partage processus et play mode entre fixtures PlayMode, et `RoadRageBootstrap` est `DontDestroyOnLoad`. Prener un capture de l'arbre complet de la run, pas seulement des trois noeuds rouges.
3. **Lancer chaque fixture seule**, puis le trio ensemble, puis la suite complete. C'est la manipulation qui discrimine le plus vite « ma story a casse ce test » de « ces fixtures ne supportent pas de tourner ensemble ».

Commandes (a lancer par l'humain ; l'agent ne les execute pas) :

- PlayMode filtre `Story11MainMenuLaunchPlayModeTests`
- PlayMode filtre `Story16Epic1PlayableCheckpointPlayModeTests`
- PlayMode filtre `Story44PassengerActionTwoMvpRunPlayModeTests`
- PlayMode filtre `Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests` — **a faire meme si cette fixture n'est pas rouge** : on ne sait pas si elle a tourne
- Puis la suite PlayMode complete, en conservant le XML de resultats

Si une fixture passe seule et echoue dans la suite, la cause est l'ordre partage, pas le code de la Story 4.6.

## Hypotheses, par ordre de plausibilite

**H1 — Pollution d'etat entre fixtures PlayMode (la plus probable, mais non confirmee).**
Precedent direct : apres le pass 2 de la Story 4.5, le meme trio plus `Story12` etait remonte rouge avec exactement ce message parent. Cause identifiee alors : un test de `Story12` chargeait reellement `MVP_Run` avant d'echouer, ce qui polluait l'etat de scene des fixtures suivantes. Ce test a ete retire et la couverture deplacee vers `Story15` — et cette fois `Story12` est vert. Donc soit une autre fixture charge `MVP_Run` et pollue, soit la cause est differente.
A verifier : la Story 4.6 a ajoute une fixture qui charge `MainMenuLobby`, puis `MVP_Run`, puis recharge `MainMenuLobby` (`FrozenSelectionSpawnsTheSelectedCharacterInMvpRunAndStaysImmutable`). Si l'ordre d'execution place `Story46` avant une des trois fixtures rouges, elle est un candidat serieux.

**H2 — `LogAssert` non satisfait.**
Le gel ajoute deux `Debug.LogWarning` (`PlayerProfileStore.Set` refuse sous gel, et le garde `SetProfileSelectionFrozen` quand le depot est absent). En PlayMode, Unity n'echoue normalement que sur `Error`/`Assert`/`Exception`, donc ces warnings sont a priori inoffensifs — sauf dans une fixture qui emploie `LogAssert.NoUnexpectedReceived()` ou `LogAssert.Expect(...)`. `Story11MainMenuLaunchPlayModeTests` emploie `LogAssert.Expect` (un `LogType.Warning` sur la seconde instance de bootstrap, un `LogType.Log` sur le quit) : c'est le seul des trois qui manipule des attentes de log, et il enchaîne des `RaisePlayRequested` / `RaiseBackRequested` que le gel traverse desormais.

**H3 — Gel herite non leve.**
`PlayerProfileStore.Freeze()` n'est leve que par la reouverture du menu. Une fixture qui clique Play et dont le teardown ne detruit pas `RoadRageBootstrap` laisse un depot gele pour toutes les fixtures suivantes ; ces dernieres partagent le singleton. Les fixtures qui chargent `MVP_Run` ou `Bootstrap` sans passer par le menu ne declenchent aucun degel.
A verifier : pour chaque fixture PlayMode, est-ce que le teardown detruit `RoadRageBootstrap.Instance` ? Un `Profiles.IsFrozen` laisse a `true` en debut de fixture roue confirmerait H3 directement.

**H4 — Regression reelle dans le spawn solo.**
`RunFlowController` lit desormais `SessionSelection` au lieu de `Current`. Ecarte par lecture pour le cas `null` (le garde `HasProfile` renvoie `MissingProfileMessage` avant, donc pas de `NullReferenceException`), mais l'ordre `displayName`/`character` affiche au HUD reste a confirmer sur `Story16`, qui capture `Profiles.Current.DisplayName` avant le clic Play et asserte ensuite le texte du HUD.

## Pistes ecartees par lecture du code

- `bootstrap.Profiles.SessionSelection` ne peut pas lever de `NullReferenceException` dans `RunFlowController.TrySpawnSelectedProfile` : `Freeze()` ne peut capturer `null` que si `Current` etait deja `null`, et dans ce cas `HasProfile` est faux et la methode retourne avant.
- `Story44PassengerActionTwoMvpRunPlayModeTests` ne touche jamais au profil : il charge `MVP_Run` et `Dev_RageSandbox` directement et n'asserte que l'etat d'incident. Un echec ici pointe vers l'environnement partage, pas vers le gel.
- Les clics sur les boutons de personnage sont tous anterieurs au gel dans les fixtures existantes (`Story16` : avant Play ; `Story45` : fixture purement menu), donc aucun test preexistant ne devrait voir sa mutation refusee. `Story46` est le seul a cliquer un personnage apres le gel, et c'est volontaire.
- La garde `Story13CharacterSetupTests.PlayerProfileStoreExposesNoSilentMutator` a ete elargie a `Set`, `Freeze`, `Unfreeze` : elle ne filtre pas sur le type de retour, donc le passage de `Set` a `bool` ne la casse pas.

## Etat a etablir avant de considerer la Story 4.6 terminee

1. Les trois fixtures rouges repassent vertes, ou leur rouge est attribue a une cause etrangere a la Story 4.6 avec la preuve.
2. `Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnPlayModeTests` a bien tourne et est verte : **l'audit de matrice du workflow considere un test qui n'a pas tourne comme manquant**, et deux lignes de la matrice (ecriture disque refusee sous gel, notice de refus visible) ne sont couvertes que par ce fichier.
3. La suite EditMode reste verte hors les 2 echecs preexistants.
4. Le resultat de la recompilation Editor est rapporte (`failed=false`, `errors=[]`).

Tant que 2 n'est pas etabli, la couverture de la matrice de la Story 4.6 par les tests PlayMode est **non prouvee**, meme si les trois autres points sont verts.
