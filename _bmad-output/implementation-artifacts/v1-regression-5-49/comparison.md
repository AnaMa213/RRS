# Story 5.49 -- regression V1 avant / apres

Baseline capturee le 2026-09-23 avant toute modification de prefab ou de scene (`baseline_commit`
`9ab80d6005f03e24703c1e4f512f36861a4c9c3d`). Tout delta comportemental est soumis au proprietaire ;
aucun n'est accepte parce que le hash source est inchange.

## Empreintes

| Empreinte | Avant | Apres | Verdict |
|---|---|---|---|
| SHA-256 `MVP_Run.road-lineage.json` | `f838ab5926a2cfe66b3074ae9b828cc17f7298df0e83531ffb6d0f8b84a6e6f4` | `f838ab5926a2cfe66b3074ae9b828cc17f7298df0e83531ffb6d0f8b84a6e6f4` | identique |
| `source-hash` (`V1SourceSet.Extract`) | `b3064424c2b3ba22f0893eea36ed25f5cc4f85e4582a2a45d211899fbd8292fc` | `b3064424c2b3ba22f0893eea36ed25f5cc4f85e4582a2a45d211899fbd8292fc` | identique |
| SHA-256 `migration-report-5-27-mvp-run.md` | `61cedbb899f0ea7c24f8deb3a4b91f109732acd316d473b274ad1b70359de40c` | `61cedbb899f0ea7c24f8deb3a4b91f109732acd316d473b274ad1b70359de40c` | identique |
| `RoadModelVersion` (modele authore 5.28) | `v4:bc477eb562d7946c977c13672cab39df` | `v4:33e3cc5fca044e988e9862d7eb77dddb` | change (largeurs appliquees et frontieres de giratoire, attendu) |

Identite : la migration 5.27 relancee re-serialise la lignee octet pour octet, sans frappe ni retrait
(`Story549RoundaboutWideningTests.TheV1SourceAndTheLineageStayByteIdenticalAndNoIdentityIsMintedOrRetired`).

## EditMode (`baseline-editmode.json` -> `after-editmode.json`)

| | Total | Passes | Echecs |
|---|---:|---:|---:|
| Avant | 831 | 830 | 1 |
| Apres | 837 | 836 | 1 |

- Statut change sur un test present avant et apres : **aucun**.
- Seul echec, avant comme apres : `Story528AuthoringAndGateATests.GateAIsOpenedOnlyByTheOwnersBoundSignoff` (sign-off Gate A absent, attendu jusqu'a la signature en 5.28).
- Retire (V2, remplace par la regle appliquee, renegociation 5.28 du 2026-09-23) : `Story528AuthoringAndGateATests.ADivergentWidthIsAHardFailureIncludingAnAsymmetricOne`.
- Ajoutes (V2) : `Story528...AReviewedWidthIsAppliedExactlyAsAuthoredIncludingAnAsymmetricOne`, `Story528...AWidthBelowTheGaugeAForbiddenModeABrokenFloorOrAWidenedPortalIsAHardFailure`, et les 5 tests `Story549RoundaboutWideningTests`.
- Suites V1 (TrafficOracle, Stories 5.2/5.4/5.7/5.9/5.10/5.14/5.15/5.16) : toutes au meme statut qu'avant.

**Delta comportemental V1 EditMode : aucun.**

## PlayMode (`baseline-playmode.json` -> `after-playmode.json`)

Avant : 45 tests, 39 passes, 6 echecs PRE-EXISTANTS (avant tout changement 5.49) :

- `Story512TireForcesAndSteeringPlayModeTests.TheHandbrakeLocksTheRearWheelsAndBreaksTheirGrip`
- `Story513ArcadeAssistsAndUnevenGroundPlayModeTests.LosingAllFourContactsAppliesNoAuthorityAndKeepsTheAttitudeStable`
- `Story513ArcadeAssistsAndUnevenGroundPlayModeTests.TheAuthorityFollowsTheGroundedWheelCountInProportion`
- `Story513ArcadeAssistsAndUnevenGroundPlayModeTests.TheVehicleCrossesTheRecipeBumpAndStepAtDrivingSpeed`
- `Story513ArcadeAssistsAndUnevenGroundPlayModeTests.TheVehicleCrossesTheRecipeStepAtLowSpeed`
- `Story57AiTrafficClientPresentationPlayModeTests.HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels`

Apres : passage manuel du proprietaire dans une session d'Editeur redemarree (2026-09-23), depuis
le Test Runner ; l'Editeur ne retient pas ce resultat (`test_status` : `no_tests`), d'ou l'absence de
`after-playmode.json`. Resultat rapporte : tous verts sauf les 6 memes tests, avec des messages et des
valeurs mesurees identiques a la baseline :

| Test | Avant | Apres |
|---|---|---|
| `TheHandbrakeLocksTheRearWheelsAndBreaksTheirGrip` | glissement arriere 0,004 / avant 0,004 | idem |
| `LosingAllFourContactsAppliesNoAuthorityAndKeepsTheAttitudeStable` | 0 pas en l'air | idem |
| `TheAuthorityFollowsTheGroundedWheelCountInProportion` | comptes observes [4] | idem |
| `TheVehicleCrossesTheRecipeBumpAndStepAtDrivingSpeed` | `Story513_BumpDown` rampe -0,12 | idem |
| `TheVehicleCrossesTheRecipeStepAtLowSpeed` | `Story513_BumpDown` rampe -0,12 | idem |
| `HostSeesPortalSpawnedAiVehiclesAndTheirReplicatedLabels` | scene `Bootstrap` au lieu de `MainMenuLobby` | idem |

Aucun de ces tests ne traverse un giratoire de `MVP_Run` (recettes synthetiques 5.12/5.13, flux de scene 5.7).

**Delta comportemental V1 PlayMode : aucun.**

EditMode, passage manuel du proprietaire dans la meme session : tous verts sauf
`GateAIsOpenedOnlyByTheOwnersBoundSignoff` (sign-off absent), conforme au passage agent ci-dessus.

## Dispositions du proprietaire

Aucun delta detecte, en EditMode comme en PlayMode : aucune disposition de delta n'est requise.
Les 6 echecs PlayMode sont PRE-EXISTANTS a la 5.49 (presents dans la baseline) et hors de son perimetre.
