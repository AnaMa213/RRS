---
title: "Handoff Codex : camera vehicule free-look, sieges, klaxon, degats multi-vehicules"
type: 'handoff'
created: '2026-09-12'
author: 'Claude (session interrompue par l''utilisateur, blocage sur le debug camera)'
---

## Contexte

Story BMAD active : `4-3-passenger-action-one-changes-rage` (status `review`, spec dans
`_bmad-output/implementation-artifacts/spec-4-3-passenger-action-one-changes-rage.md`). Codex avait
implemente cette Story puis atteint son quota. Une session Claude a repris le travail, corrige un bug
d'architecture reel introduit par la Story (voir section 1), puis a enchaine sur plusieurs demandes
de l'utilisateur **hors perimetre de la Story 4.3** (voir sections 2 a 5) sans jamais reouvrir de
Story BMAD dediee -- c'est a corriger/formaliser en meme temps que le fix technique.

**L'utilisateur a interrompu la session en cours de debug** avec le message : "ça marche pas du tout"
sur les cameras vehicule (voir section 5), sans jamais confirmer si le probleme est un vrai bug ou un
souci de repro (Play direct sur MVP_Run sans passer par le menu principal -- voir section 5.3). Codex
doit d'abord **reproduire proprement** avant de corriger.

## 1. Fix deja valide et teste (a ne pas revert) : degats vehicule multi-vehicules

**Probleme :** `RunFlowController` ne s'abonnait aux degats de collision (`VehicleCollided`) que sur
UN SEUL vehicule (`FindAnyObjectByType<NetworkedVehicleDriverController>()`, methode
`EnsureVehicleEventBridge`). Les vehicules rage-target/dev ajoutes par la Story 4.3 ne prenaient donc
jamais de degats, meme sans conducteur.

**Fix :** ajout de `EnsureSecondaryVehicleDamageBridges()` + `ApplySecondaryVehicleCollisionDamage()`
dans `Assets/RoadRage/App/Run/RunFlowController.cs` (~ligne 590-650) : abonne chaque vehicule
additionnel (hors celui deja suivi par le HUD/joueur solo) a son propre pont de degats independant.

**Statut :** 289/289 tests EditMode passent avec ce fix. Pas de regression connue.

## 2. Fix deja valide : duplication de vehicules dev en reseau

**Probleme :** `EnsureMinimumMvpRageTargets()` (RunFlowController.cs) tournait sur **chaque pair
reseau** (host ET client) sans verifier l'autorite, donc un client non-host instanciait localement
ses propres vehicules "fantomes" en plus de ceux repliques par le host -- explique en partie
"les voitures ne spawnent pas au meme endroit" entre solo et reseau.

**Fix :** garde ajoutee : `if (!string.Equals(...) || !IsAuthoritativeForDamage()) return;` (meme
garde statique que le pont de degats, ligne ~830).

**Statut :** teste (289/289 EditMode), logique correcte a priori, **jamais verifie en conditions
reelles multi-pairs** (2 instances Editor/Build).

## 3. Fix deja valide : cycle de siege (touche G) cote reseau

**Probleme :** la touche G (cycle des 4 sieges sans sortir du vehicule) n'existait que cote solo
(`RunFlowController.HandleLocalSoloSeatSwitchInteraction`, gate `IsNetworkSessionActive() return`).
Aucun equivalent reseau.

**Fix :** ajout de `NetworkedVehicleSeatService.RequestSwitchSeat(ulong clientId)` (nouvelle methode,
meme pattern que `RequestEnterOrExit`) + `NetworkedVehicleSeatIntent.RequestSwitchSeat()` /
`RequestSwitchSeatRpc()` liee a la touche G, meme pattern RPC que E/Shift+E.

**Limite connue et NON corrigee** : `NetworkedVehicleSeatService` (Story 3.3, deja existant avant
cette session) ne connait qu'**une seule "voiture partagee"** via `FindAnyObjectByType<NetworkedVehicleState>()`.
G fonctionne donc en reseau mais seulement sur ce vehicule unique, pas sur les vehicules
rage-target/dev de la Story 4.3. Etendre ce service a plusieurs vehicules est un chantier plus large,
pas fait dans cette session.

**Statut :** compile, 289/289 EditMode, **jamais teste en Play Mode reseau reel**.

## 4. Fix deja valide : klaxon (touche H)

Ajout de `NetworkedVehicleDriverController.RequestHonk()` + evenement `VehicleHonked`, relaye via un
seul Rpc unifie `[Rpc(SendTo.Everyone)]` (`HonkRpc`) plutot que le couple Server/NotServer utilise par
Collision/Recuperation, **car un test de regression fige** (`Story34SimpleRouteCollisionAndVehicleRecoveryTests.DriverControllerExposesHostOnlyRecoveryAndCollisionContract`)
compte exactement 2 occurrences de `[Rpc(SendTo.NotServer)]` dans le fichier -- ne pas ajouter de 3e
occurrence (meme dans un commentaire, ca a deja fait echouer un test une fois dans cette session).

Bind cote `RunFlowController.HandleVehicleHornInteraction()` sur la touche H, reserve au conducteur
(solo ou reseau).

**Statut :** compile, tests verts, **jamais entendu/verifie en Play Mode** (pas de clip audio branche,
juste l'evenement C#).

## 5. NON RESOLU : camera libre "chase cam" par siege -- c'est le blocage actuel

### 5.1 Ce qui a ete fait

- Le prefab `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` contient 4 `CinemachineCamera` enfants
  (`LocalVehicleCamera` = conducteur, + 3 dupliquees par l'utilisateur pour les sieges passagers :
  `LocalVehicleCamera_FrontPassenger`, `LocalVehicleCamera_RearLeft`, `LocalVehicleCamera_RearRight`),
  chacune avec `CinemachineOrbitalFollow` (Body) + `CinemachineRotationComposer` (Aim) +
  `CinemachineInputAxisController` (input souris/manette).
- `Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs` a ete reecrit pour gerer un tableau de
  4 cameras (1 conducteur + 3 passagers, champ `passengerSeatCameras`) au lieu d'une seule, active
  celle correspondant au siege occupe (`FindSeatIndex` reseau, ou `localSoloSeatIndex` en solo), et
  expose `SetRageTargetLookOverride(Transform)` pour le verrouillage camera sur cible (touche T).
- Configuration Cinemachine appliquee **directement dans le prefab via un script d'Editor** (Claude a
  utilise `PrefabUtility.LoadPrefabContents` + edition directe des composants + `SaveAsPrefabAsset`) :
  - `HorizontalAxis`/`VerticalAxis`.`Recentering` : `Enabled=true, Wait=5, Time=2` sur les 4 cameras
    (recentrage automatique apres 5s d'inactivite).
  - `HorizontalAxis.Center`/`Value` different par siege : conducteur=0°, passager avant=35°,
    arriere-gauche=-145°, arriere-droit=145° (angle relatif a `RecenteringTarget=TrackingTarget`,
    c'est-a-dire relatif a l'orientation du vehicule).
  - `CinemachineInputAxisController` : les 3 controllers (`Look Orbit X`, `Look Orbit Y`,
    `Orbit Scale`) pointes vers les `InputActionReference` **du package Cinemachine par defaut**
    (`Packages/com.unity.cinemachine/Runtime/Input/CinemachineDefaultInputActions.inputactions`,
    actions `CM Default/Look` et `CM Default/Zoom`) -- **PAS** le fichier
    `Assets/InputSystem_Actions.inputactions` du projet. Ce point merite verification : dans un projet
    qui utilise deja son propre asset d'Input Actions, pointer vers l'asset du package peut ne rien
    lire si le projet a un `PlayerInput`/`InputActionAsset` different actif qui ne l'inclut pas, ou si
    aucun mecanisme n'active ces actions du tout cote gameplay.

### 5.2 Symptome rapporte par l'utilisateur

"ça marche pas du tout" -- **aucune precision obtenue** (l'utilisateur a interrompu avant de repondre
aux questions de clarification). A determiner par Codex : est-ce que la souris ne fait rien bouger du
tout, ou bouge mais ne recentre pas apres 5s, ou T/G ne font rien, ou le jeu plante/log des erreurs ?

### 5.3 Piste forte trouvee mais NON confirmee comme la cause : profil joueur non confirme en Play direct

En testant (`unity command editor_play` sur la scene `MVP_Run` ouverte directement, sans passer par
`MainMenuLobby`), le log suivant apparait immediatement :

```
[Run] Entree monde refusee : aucun profil joueur confirme.
```

Ceci vient de `RunFlowController.TrySpawnSelectedProfile()` (ligne ~196) qui exige
`RoadRageBootstrap.EnsureInstance().Profiles.HasProfile == true`, un etat normalement pose par le
flux `MainMenuLobby` (selection de personnage). **Si aucun joueur ne spawn, `activeLocalPlayer` reste
null et TOUTES les interactions vehicule (E/G/T/H, HandleLocalSoloVehicleInteraction, etc.) sont
no-op silencieuses** -- ce qui matcherait un "ça ne marche vraiment pas du tout".

**Codex doit absolument clarifier avec l'utilisateur (ou verifier lui-meme) si le test se fait**
**via le menu principal (flux normal) ou par un Play direct sur MVP_Run** avant de toucher au code
camera. Si c'est un Play direct, le probleme n'est peut-etre pas la camera du tout mais l'absence de
spawn -- soit changer la methode de test (passer par le menu), soit ajouter un point d'entree dev qui
bypasse le profil pour MVP_Run (a voir avec l'utilisateur, ca ressemble a `RageSandboxAutoStart.cs`
qui fait deja ce genre de bypass pour `Dev_RageSandbox.unity` -- s'en inspirer si un bypass equivalent
est souhaite pour MVP_Run).

### 5.4 Verifications deja faites (pour ne pas les refaire)

- `CinemachineBrain` present et actif sur `Main Camera` dans `MVP_Run`, `OutputCamera` correctement
  assigne.
- Les 4 vcams du prefab sont bien trouvees dans la scene (instance `Greybox_PlayerCar_Parked`),
  toutes `active=False` par defaut (normal, c'est `LocalVehicleCameraRig` qui les active), toutes
  `Priority.Enabled=false` (priorite par defaut Cinemachine, pas de conflit de priorite visible a
  froid).
- Compilation propre, 289/289 tests EditMode verts apres tous les changements ci-dessus.
- **Jamais teste en conditions reelles avec simulation d'input souris** -- la note memoire du projet
  dit explicitement que la simulation de touches/souris synthetique via Unity MCP est peu fiable
  ("Unity MCP Play Mode input limitation: frames only advance per MCP call, reflection sandboxed;
  use public APIs, not synthetic key sim"). Un vrai test manuel par l'utilisateur (ou un test PlayMode
  NUnit qui pousse les InputAction directement via leur API C#, sans passer par le device physique)
  sera probablement necessaire pour valider le look-around a la souris.

## 6. Question d'architecture posee par l'utilisateur, PAS ENCORE traitee

> "pourquoi on créer directement dans le prefab les caméras et non un module à part réutilisable si
> on a plusieurs assets de voiture ?"

**L'utilisateur a raison.** L'implementation actuelle cable les 4 `CinemachineCamera` + leurs
composants Cinemachine directement dans `Greybox_PlayerCar.prefab`. Si d'autres prefabs de vehicule
sont ajoutes plus tard, il faudrait dupliquer manuellement les 4 cameras + toute leur configuration
(recentrage, input, angles par siege) dans chaque nouveau prefab -- aucune reutilisation, risque fort
de divergence de config entre vehicules.

**Piste de refactor suggeree (non implementee) :** extraire un prefab `VehicleCameraRig` autonome
(les 4 CinemachineCamera + composants, sans dependance a un vehicule specifique), instancie et
configure automatiquement au runtime par `LocalVehicleCameraRig.Awake()` (reparente sous le vehicule
courant, `TrackingTarget`/`LookAtTarget` assignes au vehicule au moment de l'instanciation). Ainsi,
tout nouveau prefab de vehicule n'a qu'a ajouter le composant `LocalVehicleCameraRig` + une reference
vers le prefab `VehicleCameraRig` partage, sans reconfigurer Cinemachine a la main. A valider avec
l'utilisateur avant de faire ce refactor (impact sur tous les vehicules existants, notamment
`Greybox_PlayerCar.prefab` qui devra perdre ses 4 cameras enfants au profit du prefab partage).

## 7. Portee BMAD -- a clarifier avant de continuer

Tout ce qui est dans les sections 2 a 6 est **hors du perimetre fige de la Story 4.3**
(cf. `Boundaries & Constraints` du spec : "Never ... rendre la camera focus synchronisee comme
gameplay state", rien sur klaxon/cameras passager/cycle de siege). Ce travail a ete fait sur demande
explicite de l'utilisateur en cours de session, sans jamais ouvrir de nouvelle Story/spec BMAD.

**Avant de corriger le bug camera, il serait sain de :**
1. Confirmer avec l'utilisateur le repro exact (section 5.2/5.3).
2. Decider si ce travail (sieges/camera/klaxon) merite une vraie Story BMAD dediee (ex: `4.4`) avec
   son propre spec/AC, plutot que de continuer a l'empiler sur la review de 4.3.
3. Ne pas toucher `sprint-status.yaml` (Story 4.3 reste `review`) tant que la review humaine n'a pas
   eu lieu.

## 8. Fichiers touches dans cette session (uncommitted, a verifier avec `git diff`)

- `Assets/RoadRage/App/Run/RunFlowController.cs` (dommages multi-vehicules, garde d'autorite spawn,
  cycle de siege solo, klaxon, verrouillage camera T)
- `Assets/RoadRage/App/Run/DevVehicleSpawner.cs` (nouveau, extraction du spawn hors RunFlowController
  pour respecter la garde archi Epic 1 "pas de `.Spawn(` dans RunFlowController.cs")
- `Assets/RoadRage/App/Run/NetworkedVehicleSeatService.cs` (`RequestSwitchSeat`)
- `Assets/RoadRage/App/Run/NetworkedVehicleSeatIntent.cs` (touche G reseau)
- `Assets/RoadRage/Features/Vehicles/NetworkedVehicleDriverController.cs` (klaxon `RequestHonk`/`VehicleHonked`)
- `Assets/RoadRage/Features/Vehicles/LocalVehicleCameraRig.cs` (reecrit : 4 cameras par siege, verrouillage LookAt)
- `Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab` (4 CinemachineCamera enfants + config Cinemachine,
  editee via script d'Editor, PAS via l'inspecteur -- verifier qu'aucun champ ne s'est mal serialise)
- `Assets/RoadRage/Tests/EditMode/Story33SeatEntryExitAndPassengerPresenceTests.cs` (1 assertion de
  texte source mise a jour pour matcher le nouvel appel `SetLocalSoloCameraActive(true, localSoloSeatIndex)`)

**Etat des tests :** 289/289 EditMode verts au dernier `run_tests`. **PlayMode jamais valide dans
cette session** (le CLI `unity run_tests --mode PlayMode` restait bloque a 0 tests decouverts lors
d'une session precedente ; pas re-teste depuis).

## 9. Prochaine etape suggeree pour Codex

1. Reproduire exactement le repro utilisateur (demander precision si besoin -- ne pas deviner).
2. Verifier en particulier le point 5.3 (profil joueur) avant de toucher au code Cinemachine.
3. Si le profil est OK et que la camera ne bouge vraiment pas : verifier que les `InputAction` du
   `CinemachineInputAxisController` sont bien *enabled* au runtime (Cinemachine gere ca via
   `AutoEnableInputs=true`, deja verifie a true, mais a confirmer par un `Debug.Log` ou breakpoint que
   les actions recoivent bien des valeurs non nulles depuis la souris).
4. Traiter la question d'architecture (section 6) avec l'utilisateur avant de refactorer.
