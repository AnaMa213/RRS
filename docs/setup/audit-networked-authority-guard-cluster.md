# Audit du cluster de garde d'autorite reseau

Date : 2026-09-17. Origine : `docs/setup/devworkflow-rollout.md` (P1), lui-meme trace depuis
`ADDON-023` (pilote jscpd) dans `docs/setup/addon-adoption-register.md` : "5 formant une garde
d'autorite reseau reelle repetee dans 5 fichiers `Networked*`". Portee : lecture seule, aucun
fichier de code modifie, `graphify update` non execute.

Fichiers audites :

- `Assets/RoadRage/App/Run/NetworkedPlayerLifecycleIntent.cs`
- `Assets/RoadRage/App/Run/NetworkedPlayerReviveIntent.cs`
- `Assets/RoadRage/App/Run/NetworkedVehicleRecoveryIntent.cs`
- `Assets/RoadRage/App/Run/NetworkedVehicleSeatIntent.cs`
- `Assets/RoadRage/Features/Players/NetworkedPlayerPresentation.cs`

## 1. Ce que fait chaque fichier

Contexte d'architecture confirme par lecture directe (pas suppose) : `ARCHITECTURE-SPINE.md`
(spine jeu 2026-09-02), **AD-18 "Netcode Ownership And Intent Pipeline"** : "Gameplay-authoritative
NetworkObjects are host-owned ... Client-owned objects are limited to input/presentation proxies
... Player actions become typed ServerRPC intents; host validation checks actor, run phase, player
mode/seat, cooldown, target reference, range, and payload version before mutating state." Les 5
fichiers sont des implementations concretes de ce patron : `NetworkedPlayerRoot` est host-owned, un
`RequireOwnership` Netcode standard ne fonctionnerait donc pas pour le client qui "possede"
logiquement ce root ; chaque classe compense par une verification manuelle emetteur == proprietaire
logique.

| Fichier | Intention | Declencheur | Verification avant action | Ordre | Host | Client | Echec |
|---|---|---|---|---|---|---|---|
| `NetworkedPlayerLifecycleIntent` | Respawn (R, joueur Dead) + Revive (delegue, voir §2) | Touche R en `Update()` si `Lifecycle==Dead` et representation locale | `state.ClientId == manager.LocalClientId` (local), puis `state.ClientId == rpcParams.Receive.SenderClientId` (RPC) | CacheState -> NetworkManager/state null -> ClientId==local -> IsServer ? direct : RPC -> (RPC) CacheState -> ClientId==sender -> Apply | Appel direct `TryRespawn`/`TryReviveNearestDowned` si `IsServer` | Sinon `[Rpc(SendTo.Server, InvokePermission=Everyone)]` | Retour silencieux, aucun log |
| `NetworkedPlayerReviveIntent` | Revive coequipier le plus proche (F) | Touche F si joueur local `Alive` | idem | idem | idem | idem | Retour silencieux, aucun log |
| `NetworkedVehicleRecoveryIntent` | Recuperation manuelle vehicule (R) | Touche R si joueur local en `PlayerMode.Driver` | idem + verification metier supplementaire : `vehicleState.DriverClientId.Value != clientId` | idem, + un 3e palier de verification specifique au domaine apres le palier commun | idem | idem | **Seul cas avec `Debug.LogWarning`** sur le refus metier ("emetteur non conducteur du siege 0") |
| `NetworkedVehicleSeatIntent` | Entrer/sortir (E, +Shift=passager) et changer de siege (G) | Deux flux independants, chacun dupliquant le meme squelette RPC | idem | idem, x2 (un par flux) | idem, x2 | idem, x2 | `Debug.LogWarning` si `NetworkedVehicleSeatService.Instance` absent (garde d'infra, pas d'autorite) |
| `NetworkedPlayerPresentation` | Synchronisation de pose (`SubmitLocalPose`) + rendu visuel greybox des joueurs distants | Appele en continu (chaque frame, via `NetworkedLocalPlayerPoseReporter`), pas une touche | idem | idem | idem | idem | Retour silencieux, aucun log |

## 2. Meme invariant ou coincidence ? Comparaison methode par methode

**Ce qui est reellement identique (invariant semantique, pas coincidence) :**

La ligne de garde d'autorite proprement dite est un texte identique dans les 7 occurrences
(Lifecycle x2 RPC, Revive x1, Recovery x1, Seat x2, Presentation x1) :

```csharp
if (state == null || state.ClientId.Value != rpcParams.Receive.SenderClientId)
{
    return;
}
```

C'est la substitution manuelle a `RequireOwnership` requise par AD-18 pour un objet host-owned :
elle lie l'emetteur RPC (authentifie par Netcode) au `ClientId` logique porte par ce
`NetworkedPlayerState`. Meme ordre (verification puis retour immediat), meme absence de feedback
au client, meme position (premiere ligne du handler RPC, avant tout effet de bord). Les
commentaires XML des fichiers le disent explicitement les uns des autres ("meme pattern que
NetworkedVehicleSeatIntent", "comme NetworkedPlayerPresentation.SubmitLocalPose/SubmitPoseRpc") --
ce n'est pas une ressemblance accidentelle, c'est une convention consciente et repetee au fil des
stories (2.5 -> 2.7 -> 3.3 -> 3.4 -> 3.5).

Le squelette autour (CacheState lazy-init, pre-verification cote client
`manager==null || !IsListening || state==null || ClientId!=local`, branchement `IsServer ?
direct : Rpc`) est egalement identique dans les 5 fichiers -- mais ce squelette n'est **pas** la
garde d'autorite : la pre-verification cote client est un raccourci UX (evite d'emettre une RPC
inutile), pas une frontiere de securite, puisqu'un client ne peut de toute facon rien prouver
avant que le host ne revalide via `SenderClientId`. `CacheState()` est du pur boilerplate sans
lien avec l'autorite (c'est vraisemblablement l'un des "3 clones de boilerplate" deja ecartes par
l'inspection jscpd).

**Ce qui diverge reellement (raisons de domaine legitimes) :**

- `NetworkedVehicleRecoveryIntent` ajoute un **second palier** de verification que les 4 autres
  n'ont pas : apres le controle d'emetteur, il verifie une regle metier distincte
  (`vehicleState.DriverClientId.Value != clientId`, "seul le conducteur du siege 0 peut
  recuperer") et journalise ce refus. Les 4 autres ne journalisent jamais un refus d'autorite.
- `NetworkedVehicleSeatIntent` porte **deux flux independants** (entree/sortie et changement de
  siege), chacun dupliquant tout le squelette une deuxieme fois **a l'interieur du meme fichier**
  -- une duplication plus etroite et plus actionnable que le cluster inter-fichiers audite ici,
  mais hors perimetre de cette tache.
- `NetworkedPlayerPresentation` n'est pas une "intention" au sens des 4 autres : elle est
  declenchee en continu (chaque frame) et non par une touche, et le fragment RPC n'occupe que
  ~20 lignes d'un fichier de 269 lignes dont la responsabilite dominante est le rendu visuel
  (catalogue de personnages, instanciation de prefab, desactivation de colliders, abonnements
  `OnValueChanged`). Le rapprochement avec les 4 autres classes tient a la forme du fragment RPC,
  pas a la responsabilite du fichier.
- **Duplication non demandee mais constatee, plus forte que celle auditee** :
  `NetworkedPlayerLifecycleIntent.RequestRevive()` / `RequestReviveRpc()` /
  `ApplyServerReviveRequest()` sont, methode pour methode, quasi identiques a
  `NetworkedPlayerReviveIntent` (meme delegation a `TryReviveNearestDowned(reviverClientId)`). Les
  deux classes offrent le meme point d'entree "revive" en parallele. Ce n'est pas dans le
  perimetre de cette tache (qui porte sur la garde d'autorite, pas sur la logique de revive) et
  n'appelle pas d'action ici, mais merite d'etre note comme signal si la duplication redevient un
  sujet de revue.

**Conclusion §2 :** la garde d'autorite (1 ligne) est un invariant semantique reel et volontaire.
Le reste du squelette qui l'entoure est du boilerplate coincidental (meme forme aujourd'hui, pas
necessairement lie a jamais). Les points de divergence (Recovery, Seat, Presentation) sont motives
par le domaine, pas accidentels.

## 3. Tests existants

Recherche `rg` de chaque nom de classe dans `Assets/RoadRage/Tests/`. Les 5 classes sont couvertes,
mais **exclusivement par assertions sur le texte source** (`File.ReadAllText(...)` +
`Assert.That(source, Does.Contain("..."))`) et par des controles de presence de composant sur le
prefab `NetworkedPlayerRoot` -- aucun test ne simule reellement deux `NetworkManager`/clients et
n'envoie une RPC avec un `SenderClientId` usurpe pour verifier que le refus se produit a
l'execution :

- `Story27PlayerLifecycleTests.LifecycleIntentRpcValidatesSenderAndDelegatesRespawnToLifecycleService`
  -- verifie la presence textuelle de `state.ClientId.Value != rpcParams.Receive.SenderClientId`.
- `Story35VehicleDamageHookAndTeamWipeContractStubTests` (ligne ~138) -- meme verification
  textuelle pour `NetworkedPlayerReviveIntent`.
- `Story34SimpleRouteCollisionAndVehicleRecoveryTests.RecoveryIntentMirrorsSeatIntentClientToHostRpcPattern`
  -- idem pour `NetworkedVehicleRecoveryIntent`, plus verification textuelle du controle
  `DriverClientId.Value != clientId`.
- `Story33SeatEntryExitAndPassengerPresenceTests` -- verifie la presence du composant sur le prefab
  et le fait qu'il herite de `NetworkBehaviour` ; ne verifie pas le texte de la garde RPC
  elle-meme pour ce fichier precis (verification la plus faible des 5).
- `Story25NetworkedPlayerSpawnTests.NetworkedPresentationAcceptsPoseOnlyFromRepresentedClient`
  -- meme verification textuelle pour `NetworkedPlayerPresentation`.

**Constat :** le "happy path" (composant present, delegation au bon service) et la **forme
textuelle** de la garde sont sous test pour 4/5 fichiers. Le **comportement** de la garde
(refus effectif d'une RPC dont le `SenderClientId` ne correspond pas au `ClientId` du
`NetworkedPlayerState` cible) n'est verifie a l'execution pour aucun des 5 -- ecart reel, mais
distinct de la question de duplication posee par cette tache ; il concerne la robustesse des
tests, pas la structure du code.

## 4. Risque de couplage si extraction

Le seul fragment veritablement identique et lie a l'autorite est une expression booleenne d'une
ligne : `state.ClientId.Value != rpcParams.Receive.SenderClientId`. Un helper plausible serait :

```csharp
private static bool IsAuthorizedSender(NetworkedPlayerState state, RpcParams rpcParams)
    => state != null && state.ClientId.Value == rpcParams.Receive.SenderClientId;
```

Cout d'appel a chaque site : passer `state` + `rpcParams` (2 parametres), pour remplacer une
expression d'une ligne deja lisible in situ par un appel de methode d'une ligne. Aucun gain net de
lignes, aucune reduction de complexite cyclomatique, et une perte de localite (le lecteur doit
sauter vers une classe partagee pour verifier ce qu'une ligne inline disait deja). C'est l'exemple
type de l'echelon 6 de l'echelle ponytail ("peut tenir sur une ligne ? -> une ligne.") deja
satisfait par le code actuel : il n'y a rien a raccourcir.

Une extraction plus ambitieuse (classe de base `NetworkAuthorityIntent`, ou methode partagee
couvrant tout le squelette CacheState + pre-check client + branchement IsServer/RPC) devrait
absorber les divergences reelles du §2 : le palier metier supplementaire de Recovery (avec son
log), les deux flux independants de Seat (avec leur propre log d'absence de service), et le fait
que Presentation n'est pas pilotee par une touche et vit dans un fichier a responsabilite de rendu.
Une base commune devrait donc exposer des hooks/virtuals pour ces variations -- exactement le
"risque de couplage artificiel" nomme par le backlog : la surface de l'abstraction grandirait
jusqu'a couvrir l'union des besoins des 5 classes, ce qui la rend plus complexe que les 5 copies
qu'elle remplacerait.

## 5. Verdict

**Aucune action. Cas ferme, a reevaluer seulement sur declencheur nomme.**

- La duplication qui a declenche l'audit (la ligne de garde d'autorite) est **semantique** :
  meme regle, meme raisonnement (AD-18, substitution d'ownership Netcode pour un objet host-owned),
  volontairement reproduite a l'identique au fil de 5 stories. Ce n'est pas fortuit.
- Mais elle est **trop petite pour justifier une extraction** : une ligne, deja lisible en place,
  sans gain de lignes ni de clarte a en faire un appel de methode partagee (§4). Le squelette
  environnant qui semble dupliquer beaucoup plus de surface (CacheState, pre-check client,
  branchement IsServer) est du boilerplate non lie a l'autorite, et diverge deja de facon motivee
  par le domaine des qu'on regarde au-dela de la forme (Recovery, Seat, Presentation, §2).
- Conforme a AD-13 (spine devworkflow) : pas de besoin mesure ici -- extraire un helper d'une
  ligne n'a pas de ROI positif, et une extraction plus large forcerait un couplage artificiel entre
  5 responsabilites de domaine differentes (lifecycle, revive, recuperation vehicule, siege,
  presentation visuelle).
- **Declencheur de reevaluation nomme** : si une 6e classe "intent" apparait et que la regle
  d'autorite doit changer **simultanement** dans les fichiers existants (ex. migration vers
  `OwnerClientId`/`RequireOwnership` natif Netcode si le modele d'ownership change, ou ajout d'une
  verification anti-spoofing supplementaire commune aux 5+), alors la duplication sera devenue "doit
  changer ensemble pour toujours" plutot que "se ressemble aujourd'hui" -- a ce moment seulement,
  extraire le helper `IsAuthorizedSender` (ou equivalent) devient justifie et bon marche.
- Observation annexe hors perimetre (§2) : `NetworkedPlayerLifecycleIntent.RequestRevive` duplique
  presque integralement `NetworkedPlayerReviveIntent` -- duplication de logique de revive, pas de
  garde d'autorite. A surveiller si une revue future s'en saisit, mais ne fait pas partie du
  verdict de cette tache.
- Ecart de test note en §3 (garde jamais exercee a l'execution, seulement en texte source) : reel,
  mais distinct de la question de duplication posee ; ne motive pas non plus une extraction, il
  motiverait plutot un test d'integration reseau dedie si le risque de regression sur cette garde
  devient preoccupant.
