# Traffic & Driving Rules

Reference du comportement **reellement implemente** de l'IA de trafic de RoadRage Simulator, a
l'issue des Stories 5.17 et 5.18. Ce document decrit le code tel qu'il est, pas l'intention : une
regle absente du code est signalee comme absente.

Public : developpeurs du projet, et diagnostic en recette. Chaque section nomme son **declencheur**,
sa **priorite**, l'**etat resultant**, sa **condition de sortie**, son **interaction** avec les
autres regles et sa **classe porteuse**.

> Tout ce qui suit s'execute **cote hote uniquement** (`if (!IsServer) return;`). L'IA n'ecrit jamais
> le Rigidbody : elle emet un `VehicleDriveIntent` que la couche physique consomme.

**Fichiers porteurs**

| Role | Fichier |
|---|---|
| Boucle de conduite, parcours, visee | `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` |
| Perception, conflits, jonctions, recuperation | `...NetworkedAIVehicleDriverController.Traffic.cs` |
| Predicats purs de perception | `Assets/RoadRage/Features/Vehicles/TrafficPerception.cs` |
| Predicats purs de priorite et d'escalade | `Assets/RoadRage/Features/Vehicles/JunctionRules.cs` |
| Modele longitudinal (IDM) | `Assets/RoadRage/Features/Vehicles/DriverModel.cs` |
| Geometrie et routage purs | `Assets/RoadRage/Features/Vehicles/LaneGraphRouting.cs` |
| Trace routier, jonctions, anneaux | `Assets/RoadRage/Features/Vehicles/LaneGraph.cs` |
| Donnees de monde (feux, degagements, population) | `Assets/RoadRage/Features/Vehicles/TrafficSettingsDef.cs` |
| Personnalites de conduite | `Assets/RoadRage/Features/Vehicles/DriverProfileDef.cs` |
| Apparition et disparition aux portails | `Assets/RoadRage/App/Run/PortalTrafficSpawner.cs` |

---

## Ordre de decision (la regle des regles)

Un pas physique traverse les etages suivants **dans cet ordre**. Le premier etage qui rend `true`
commande la pedale et **rien en dessous ne s'execute**. C'est la reponse a « collision d'urgence vs
permission de jonction vs suivi de file vs feu vs recuperation ».

| # | Etage | Porteur | Peut arreter le pas ? |
|---|---|---|---|
| 0 | Chute sous le monde, retournement soutenu | `FixedUpdate` -> `RecoverAtWaypoint` | oui, et **repositionne** |
| 1 | Encastrement physique soutenu | `FixedUpdate` -> `RecoverAtWaypoint` | oui, et **repositionne** |
| 2 | Vitesse desiree nulle (disposition immobilisante) | `FixedUpdate` | oui |
| 3 | **Securite locale et recuperation** (`TickLocalTraffic`) | `...Traffic.cs` | oui |
| 4 | **Regles d'intersection** (`TickJunctionRules`) | `...Traffic.cs` | oui |
| 5 | Conduite nominale : visee, plafond de courbe, IDM | `NetworkedAIVehicleDriverController.cs` | — |

**Consequence directe : la securite locale prime la permission de jonction.** Un obstacle percu
arrete le vehicule meme s'il a la permission d'entrer dans le carrefour. Inversement, un feu vert
ne fait jamais avancer dans quelqu'un.

A l'interieur de l'etage 3, l'ordre est :

1. manoeuvre de deblocage en cours (elle va jusqu'au bout ou est annulee) ;
2. conflit joueur immediat -> tentative de contournement ;
3. perception saturee, blocage, cession au-dela de sa ligne, face-a-face, joueur en travers ->
   **arret ferme** ;
4. echelle de recuperation (`TryEscalateRecovery`) ;
5. sinon, nommage de l'etat (Cruise / FollowingSameLane / TrafficQueue / YieldTrajectory /
   DeadlockRecovery) et on passe a l'etage 4.

A l'interieur de l'etage 4, l'ordre de l'arbitrage est encode dans `JunctionRules.Resolve` :

**occupation engagee > eligibilite (place de sortie, puis feu, puis stop) > rang de regle
(deblocage 4 > route prioritaire 3 > feu 2 > stop/priorite a droite) > priorite a droite > ordre
total (plus proche, puis index de voie, puis `NetworkObjectId`)**.

---

## Normal Driving

- **Declencheur** : aucun obstacle, aucun conflit, aucune jonction retenue.
- **Etat** : `TrafficDecisionReason.Cruise`.
- **Comportement** : le vehicule vise un point sur sa trajectoire echantillonnee et regle sa vitesse
  par l'IDM vers `DriverProfile.DesiredSpeed`, bruitee par `Consistency`
  (`DriverModel.ResolveNoisyDesiredSpeed`) pour que deux vehicules du meme profil ne roulent pas en
  parfaite synchronie.
- **Sortie** : des qu'un etage superieur s'applique.
- **Porteur** : `NetworkedAIVehicleDriverController.FixedUpdate`, `DriverModel.ComputeAcceleration`.

La personnalite vient de `DriverProfileDef` et est modulee par le comportement courant
(`DriverModel.ResolveEffectiveProfile`, ex. `Calm`). Valeurs authorees de reference :
`DesiredSpeed 8,00 m/s`, `MaxAcceleration 1,50 m/s2`, `ComfortableDeceleration 2,00 m/s2`,
`MinimumGap 0,75 m`, `SafetyMargin 0,30 m`, `ReactionTime 0,30 s`, `PerceptionRadius 20 m`,
`PredictionSeconds 3,0 s`, `HornDelay 2 s`, `JunctionEscalationDelay 12 s`, `RetryCooldown 2 s`,
`ProgressDistance 0,30 m`.

## Lane Following

- **Declencheur** : permanent.
- **Comportement** : le parcours est un **tirage local** au noeud atteint, pondere par les ratios de
  virage authores sur ce noeud (`ResolveNextNode`) — il n'existe **aucun itineraire pre-calcule par
  vehicule**. La trajectoire suivie est une **polyligne echantillonnee** reconstruite depuis le
  graphe a chaque pas (`BuildPathFromLaneGraph` -> `LaneGraphRouting.SampleLaneEdge`) : aretes
  droites subdivisees tous les 4 m, aretes courbes tous les 1 m, connecteurs de virage en Bezier
  quadratique dont le point de controle est l'intersection des axes de voie
  (`TryResolveLaneAxisCorner`).
- **Visee** : `LaneGraphRouting.ResolvePathLookAheadPoint` sur cette polyligne, a
  `vitesse x lookAheadSeconds` (1,0 s authore sur le prefab), avec un plancher a l'empattement.
  Braquage par poursuite pure (`ComputeSeekIntent`) puis loi de braquage physique
  (`VehicleSteeringModel.ResolveSteerAngleDegrees`).
- **La MEME polyligne sert a conduire, a percevoir et a arbitrer.** C'est la condition pour que deux
  pairs tirent le meme verdict d'un conflit.
- **Changement de voie : NON IMPLEMENTE.** `TickLaneChangeEvaluation` ne fait qu'entretenir un
  compteur ; aucune voie n'est changee. A documenter comme limite, pas comme regle.
- **Porteur** : `NetworkedAIVehicleDriverController.cs`, `LaneGraphRouting.cs`.

## Speed / Curve Handling

- **Declencheur** : permanent.
- **Regle** : la vitesse desiree est **bornee par ce que la courbe a venir permet de tenir**.
  `LaneGraphRouting.ResolvePathMinimumRadius` balaye la trajectoire sur une fenetre en avant et rend
  le rayon minimal, mesure en **rotation par longueur d'arc** (`ResolveRadiusOverArc`, arc de 2 m) et
  non par cercle a trois points — un cercle a trois points sur une polyligne a pas variable fabrique
  des rayons fantomes a chaque transition droite/courbe, donc des freinages brutaux.
  `VehicleSteeringModel.ResolveCurveSpeedLimit` convertit ce rayon en vitesse tenable par le braquage
  reel du vehicule.
- **Etat** : aucun etat dedie ; c'est un plafond sur `DesiredSpeed` dans `ResolveLongitudinalPedal`.
- **Repli** : un rayon intenable a toute vitesse rend 1 m/s et emet **un** avertissement.
- **Mesure** : erreur laterale d'anneau 1,68 m -> **0,22 m**, vitesse **8,00 m/s constante**, a-coup
  **0,0 m/s2**. Virages a 90 deg serres : **1,35 m**, qui est un **plancher physique** (braquage
  requis 35,8 deg pour 35,9 deg disponibles) — il ne se deplace que par authoring.
- **Porteur** : `LaneGraphRouting.cs`, `VehicleSteeringModel.cs`.

## Vehicle Following

- **Declencheur** : un acteur est **devant nous, dans notre couloir** (`IsOnOwnPath`), a portee de
  perception et dans l'arc авant. Le couloir vaut `demi-largeur + SafetyMargin`, et l'ecart se mesure
  **d'emprise a emprise** (`TrafficPerception.TryPathClearance`), jamais de centre a centre.
- **Selection** : le plus proche candidat qualifie (`TrySelectLeader`).
- **Comportement** : IDM (`DriverModel.ComputeAcceleration`) — approche douce, jamais un arret sec.
- **Etat** : `FollowingSameLane`, ou `TrafficQueue` quand le leader attend lui-meme regulierement.
- **Garde-fou** : une file qui n'avance pas depuis `JunctionEscalationDelay` **cesse d'etre
  legitime** (`queueElapsedSeconds`), sinon un anneau ferme A->B->C->A serait une attente reguliere
  pour tout le monde.
- **Un vehicule qui nous FAIT FACE n'est jamais une tete de file** : une file se resout quand sa tete
  avance, or celle-la, en avancant, entre dans nous.
- **Porteur** : `TrafficPerception.TrySelectLeader`, `DriverModel`, `TickLocalTraffic`.

## Intersections

- **Declencheur** : le waypoint courant porte une approche authoree
  (`LaneGraph.TryGetJunctionApproach`) et l'avant du vehicule est a moins de
  `JunctionApproachRadius` de sa ligne d'arret.
- **Etat** : `JunctionYield` (refus), `ExitSaturated` (sortie occupee), `JunctionBreach`
  (deblocage), ou permission « accordee » puis « engage ».
- **Ligne d'arret** : voir *Conflict Points*. Le refus **avant** la ligne pose une contrainte
  longitudinale (`PlannedStopGap`) et laisse l'IDM freiner dessus ; l'arret ferme n'arrive **qu'a**
  la ligne.
- **Engagement** : `CommitJunctionTraversal` n'a lieu qu'a la ligne (`junctionStopGap <= 0`), jamais
  douze metres avant — occuper la jonction de loin la fermerait aux autres pendant tout le trajet
  d'approche — **et jamais pendant qu'une cession de trajectoire court** (`yieldingTrajectory`).
- **Sortie** : `HasClearedCommittedJunction` — l'**arriere** du vehicule doit avoir depasse le noeud
  de sortie le long de l'axe de cette voie.
- **Ensemble des revendiquants** : sphere centree sur le **noeud de jonction** (et non sur nous),
  dedoublonnee par `NetworkObjectId`. Un ensemble incomplet (tampon sature) est **dit** incomplet, et
  la permission est refusee — mais l'attente qui en decoule doit pouvoir se rompre, d'ou le palier de
  deblocage.
- **Porteur** : `TickJunctionRules`, `GatherJunctionClaimants`, `JunctionRules.ResolveWinnerIndex`.

## Traffic Lights

- **Declencheur** : `JunctionApproachRule.TrafficLight` **et** un plan authore pour cet id de
  jonction dans `TrafficSettingsDef.SignalPlans`.
- **Regle** : `JunctionRules.PhaseAllowsGroup(plan, Time.time, approach.SignalGroup)`. La phase est
  **fonction du temps seul** : deux pairs qui partagent l'horloge et la donnee tirent la meme phase,
  donc **aucun etat reseau n'est ajoute**.
- **Sans plan authore, le feu laisse passer** — aucune regle n'est inventee.
- **Priorite** : le feu entre dans `Eligibility` (niveau 1 : place de sortie acquise mais feu rouge),
  donc il est **au-dessus** du rang de regle et **en dessous** de la place de sortie.
- **Interaction cle** : un feu rouge est la **seule attente dont la fin est garantie par la donnee**.
  A ce titre il **gele l'horloge de blocage** (`junctionHeldBySignal` -> `heldByBoundedWait`), et cet
  etat remonte la file d'un cran par pas. Sans cela un vehicule correctement arrete au feu partirait
  en contournement au bout de 2 s.
- **Porteur** : `ResolveSignalAllows`, `JunctionRules.ResolvePhaseIndex`.

## STOP Signs

- **Declencheur** : `JunctionApproachRule.Stop`.
- **Regle** : deux conditions cumulatives.
  1. **Maintien a l'arret** : vitesse sous le seuil d'immobilite et avant a moins de `arrivalRadius`
     de la ligne, pendant `JunctionStopHoldSeconds` (1,2 s authore). Une fois **acquis, le maintien
     le reste** jusqu'a la traversee — sinon le vehicule refreinait, reattendait et redemarrait en
     boucle (mesure : immobile 28 s a 0,74 m de sa ligne).
  2. **Ecart accepte** : `JunctionRules.StopGapAccepted` sur la distance au revendiquant concurrent
     le plus proche. Condition **symetrique** (la distance se lit des deux cotes) ; un ecart non
     mesurable n'est jamais accepte.
- **Etat** : `JunctionYield` tant que l'une manque.
- **Rang** : 1, le plus bas des regles authorees — un stop cede a une route prioritaire et a un feu.
- **Porteur** : `TickJunctionRules`, `JunctionClaim.Eligibility`, `JunctionRules.StopGapAccepted`.

## Junction Priority

Arbitrage **pair a pair**, antisymetrique par construction, dans `JunctionRules.Resolve` :

1. **Conflit ?** `JunctionRules.Conflicts` — deux mouvements qui ne se croisent pas ne s'arbitrent
   pas.
2. **Occupation avant priorite.** Un vehicule deja engage passe. Lui reprendre son mouvement parce
   qu'il vient de gauche l'immobiliserait au milieu de l'aire de conflit.
3. **Eligibilite** (0 pas de place de sortie, 1 feu rouge, 2 stop non satisfait, 3 pret).
4. **Rang de regle** : deblocage 4 > route prioritaire 3 > feu 2 > stop et priorite a droite 1.
5. **Priorite a droite** — et elle tranche toujours pour deux approches perpendiculaires.
6. **Ordre total** : plus proche, puis index de voie, puis `NetworkObjectId`.

Le vainqueur global est celui qui gagne le plus de duels (`ResolveWinnerIndex`). L'egalite n'est
atteignable que pour une revendication face a elle-meme, et rend alors « passe » : **aucun etat ou
les deux attendent l'autre n'est representable**.

## Conflict Points

Conflit **continu** entre deux trajectoires, en dehors de tout carrefour authore.

- **Detection** : `TrafficPerception.FindPathConflict` compare les deux polylignes **avec les
  emprises orientees** et rend le point de rencontre, l'instant du recouvrement et les deux dates
  d'arrivee. Le point de rencontre est mesure separement du premier recouvrement, parce qu'il est le
  **meme des deux cotes** — departager sur le recouvrement donnerait a chacun une avance apparente.
- **Verdict** : `ResolveConflictYield`, dans cet ordre :
  1. **un pair qui n'arrivera jamais ne peut pas etre prioritaire.** Une arrivee infinie ne dit pas
     « il passe en premier », elle dit « il est a l'arret ». Celui qui roule passe, celui qui est
     arrete cede ;
  2. **priorite a l'anneau** (`CompareRingPrecedence`) ;
  3. **ordre d'arrivee**, puis plus petit `NetworkObjectId` en cas d'egalite a 0,25 s pres.
  Le motif retenu est expose dans la trace (`conflictVerdictReason`).
- **LA CESSION A UNE LIGNE.** Ceder, ce n'est pas s'arreter la ou l'on apercoit le conflit : c'est ne
  pas y entrer. `TrafficPerception.ResolveConflictStopGap` recule le point de rencontre de la
  demi-largeur du vehicule et de la marge, et tant que la ligne est devant, le ralentissement passe
  par l'IDM (`PlannedStopGap`) — progressif, et le vehicule garde sa direction. L'arret ferme ne
  revient qu'a la ligne.

  | conflit vu a | decelaration de pointe | arret |
  |---|---|---|
  | 25 m | -1,93 m/s2 (confort 2,00) | 0,82 m avant la ligne |
  | 15 m | -4,74 m/s2 | 0,82 m avant la ligne |
  | 8 m | -12,15 m/s2 | 0,82 m avant la ligne |

- **Ligne d'arret d'une jonction authoree** : `JunctionApproachInfo.ConflictEntryDistance` est la
  premiere abscisse ou les **couloirs** de deux mouvements de la meme jonction se touchent — les deux
  Beziers reellement parcourues, echantillonnees a ~0,25 m, comparees au degagement authore
  `TrafficSettingsDef.JunctionMovementClearance` (2,36 m = 2 x 1,03 + 0,30). La frontiere est bornee
  par le noeud de decision. Mesure du district : intrusion de l'avant dans le couloir de virage
  **2,34 m -> 0,00 m** sur les 16 approches.
- **Un seul partenaire arbitre a la fois.** Un troisieme usager est traite au tour de perception
  suivant, quand il devient le plus proche.
- **Porteur** : `TrafficPerception.cs`, `UpdateTrajectoryConflict`, `LaneGraph.ResolveJunctionBoundaries`.

## Exit Lane Saturation

- **Declencheur** : evaluation de la permission de jonction.
- **Regle** : `JunctionRules.HasExitRoom(exitProbeKnown, blockedByRoadUser)` — faux quand un usager
  occupe la zone de degagement de la sortie, **et faux aussi quand la place est indeterminee**. Une
  place qu'on ne peut pas mesurer n'est jamais lue comme libre : la regle **echoue fermee**.
- **Etat** : `ExitSaturated`, nomme distinctement de `JunctionYield` parce que les deux causes se
  corrigent differemment.
- **Priorite** : c'est le niveau **0** d'eligibilite, donc la condition la plus forte de l'arbitrage
  de jonction.
- **Porteur** : `ResolveApproachExitRoom`, `TrafficPerception.OccupiesExitLane`.

## Roundabouts

- **Detection : aucune donnee authoree.** Les anneaux sont deduits de la **topologie seule**
  (`LaneGraph.BuildRingIndex`) : un cycle ferme d'au moins 5 noeuds parcouru en prenant partout le
  successeur le mieux aligne sur l'axe authore du noeud. Un giratoire plus grand ou a cinq branches
  se detecte de lui-meme. Le district en porte **4**, de 11 noeuds et 6,00 m de rayon.
- **Regle** : `JunctionRules.CompareRingPrecedence` — celui qui **circule** passe, celui qui **entre**
  cede ; deux vehicules du meme anneau, ou de deux anneaux distincts, retombent sur l'arbitrage
  ordinaire.
- **Etre engage suppose deux pieds sur l'anneau** (le noeud quitte **et** le noeud vise). Le seul
  noeud vise ne suffit pas : un vehicule qui atteint le noeud d'entree vise deja l'anneau alors qu'il
  est encore sur la ligne de cession.
- **Mais un vehicule immobilise sur l'anneau ne l'emporte plus** : la finitude des arrivees est
  constatee avant la regle d'anneau.
- **Les giratoires ne portent AUCUNE approche authoree** : leur ligne de cession vient entierement du
  mecanisme *Conflict Points*. C'est pourquoi ils concentraient les symptomes avant ANO-5.18-08.
- **Pourquoi pas la priorite a droite** : en circulation a droite l'anneau tourne dans le sens
  antihoraire, donc celui qui circule arrive **par la gauche** de l'entrant. Le repli du reste du
  reseau est donc **inverse** dans un giratoire, et ne doit jamais s'y appliquer.
- **Porteur** : `LaneGraph.BuildRingIndex`, `JunctionRules.CompareRingPrecedence`.

## Player Interaction

- **Conflit joueur immediat** — l'unique cas ou le joueur entre nommement dans la decision. Trois
  conditions cumulatives, **aucune n'est une prediction** : il est sur la **chaussee**, dans **notre
  voie**, et a portee de contact (`MinimumGap + SafetyMargin`, soit 1,05 m, mesure d'emprise a
  emprise).
- **Reponse** : l'IA cherche d'abord une **echappatoire laterale** (`TryStartDetour`), **sans attendre
  le delai de klaxon** — rester le nez contre un pieton deux secondes n'aurait aucun sens. Si aucune
  sortie n'est validee, le repli est l'arret : on ne roule pas sur un joueur faute de place.
- **Etat** : `PlayerImmediateConflict`, ou `PlayerCollisionRisk` pour un conflit de trajectoire avec
  un usager non arbitrable.
- **Regle d'arbitrage** : un usager **non arbitrable** (joueur, pieton, decor) **passe toujours**.
  Ce n'est pas au joueur de deviner.
- **Un pieton sur le TROTTOIR n'est pas un usager de la chaussee** : la regle n'est pas une distance,
  c'est la **surface qu'il foule** (`IsOnCarriageway`).
- **Le recovery physique ne se declenche jamais pres d'un joueur** (`IsAnyPlayerWithinClearanceRadius`).
- **Porteur** : `RefreshPerception`, `TickLocalTraffic`.

## Static Obstacles

- **Declencheur** : un collider sans Rigidbody, dans notre couloir, devant nous.
- **Particularite** : un objet immobile **n'a pas d'axe** — il obstrue ou il n'obstrue pas ; il n'est
  jamais lu comme « circulant dans notre sens ». Sa trajectoire predite se reduit a un **point**, et
  non a une demi-droite qui traverse la carte.
- **Etat** : `StaticObstacle`, distinct de `FollowingSameLane` : un obstacle immobile **n'est pas une
  attente legitime**, donc il ouvre l'echelle de recuperation.
- **Porteur** : `RefreshPerception`, `IsLegitimateWait`.

## Wrong-Way Detection

- **Mesure** : `ResolveLaneConformance` projette la position sur l'axe routier le plus proche
  (`LaneGraph.TryGetRoadPosition`) et compare notre cap au sens de cet axe
  (`dot > SameLaneAlignmentDot`, soit 45 deg).
- **Deux sorties** : `correct` (bon sens) et `distance` (ecart a l'axe).
- **Usage 1 — designation du responsable d'un face-a-face** : `TrafficPerception.MustYield` —
  **d'abord** celui qui est a contresens, **puis** le plus eloigne de l'axe, **puis** le plus grand
  `NetworkObjectId`. C'est la semantique LaneGraph qui designe le vehicule mal place, pas un tirage.
- **Usage 2 — fin d'une manoeuvre** : `HasRecoveredRoute` exige d'etre a moins de
  `demi-largeur + marge` de l'axe **et** dans le bon sens. Un retour hors voie ou a contresens
  **annule** la manoeuvre au lieu de la valider.
- **Limite** : il n'existe **pas** de detection de contresens en conduite nominale — un vehicule a
  contresens n'est reconnu comme tel que lorsqu'il entre dans un face-a-face ou termine une manoeuvre.
- **Porteur** : `ResolveLaneConformance`, `TrafficPerception.MustYield`, `HasRecoveredRoute`.

## Head-On Conflicts

- **Declencheur** : un usager **dans notre couloir** dont le cap est oppose **a la fois** a la
  tangente de notre trajectoire **et** a notre propre cap (deux tests, pas un). Un vehicule
  perpendiculaire arrete sur une branche transversale tombait sinon sur la corde diagonale d'un
  mouvement de jonction et se declarait face-a-face.
- **Le cap d'un pair IA est celui de SA VOIE, jamais celui de son nez.** Un vehicule a l'arret garde
  le nez ou son dernier virage l'a laisse ; dans un giratoire, un nez fige pointe n'importe ou.
- **Comportement** : un face-a-face **ne s'arbitre pas** — laisser l'un des deux « passer en
  premier » autoriserait la collision. Les deux s'arretent. Etat : `EmergencyBrake`.
- **Sortie du blocage** : le vehicule designe **mal place** entre dans l'echelle de recuperation des
  le palier 1. Le vehicule correctement place n'y entre qu'au **palier 3**, quand l'autre a
  manifestement echoue : il n'est pas force hors de sa voie sans raison.
- **Cas particulier** : un face-a-face contre un pair **a l'arret** (`headOnStationary`) n'est plus
  un face-a-face, c'est un obstacle — et un obstacle se contourne.
- **Porteur** : `RefreshPerception`, `MustYieldHeadOn`, `TryEscalateRecovery`.

## Local Avoidance

- **Declencheur** : palier de recuperation >= 1, ou conflit joueur immediat (immediat).
- **Construction** : `TryStartDetour` pose trois cibles — ecart lateral, passage, retour sur l'axe
  routier projete. L'ecart vaut `demi-largeur du bloqueur + notre demi-largeur + 2 x marge +
  2 x pas d'echantillonnage`, et la longueur de deport tient compte du rayon de braquage reel. Les
  deux cotes sont essayes.
- **C'est aussi l'elargissement local d'un virage** : un vehicule engage dans un virage et bloque par
  un vehicule arrete emprunte ce meme mecanisme pour contourner, dans l'espace libre valide — il n'y
  a **pas** de trajectoire de virage elargie par defaut.
- **Validation** : `ValidatePath` simule la pose du vehicule tous les `PathSampleDistance` avec le
  **braquage reel** et le **gabarit reel**, jusqu'a 128 poses. Chaque pose doit etre libre
  (`IsPoseClear`) **et** ses quatre coins doivent reposer sur une surface positivement autorisee.
  Un echec **ferme** le passage.
- **Porteur** : `TryStartDetour`, `ValidatePath`, `IsPoseClear`.

## Unblocking

Voir *Recovery Escalation* : depuis ANO-5.18-10 il n'existe plus de mecanisme de deblocage distinct.
Les etats historiques restent nommes : `UnblockingManeuver` (manoeuvre en cours),
`DeadlockRecovery` (cession relachee par rupture de cycle), `JunctionBreach` (entree dans une
intersection normalement tenue libre).

**Rupture de cycle d'attente.** Une cession ne se relache **jamais** sur un simple delai : un delai
confond la file lente avec l'attente sans issue, et deux vehicules peuvent le declencher au meme
instant. Le seul motif est **semantique** : la chaine `WaitsFor` revient sur nous. Elle se rompt
alors par le meme ordre total que tout le reste — le **plus petit `NetworkObjectId` du cycle** passe
— ce qui libere exactement un vehicule, quel que soit le membre qui fait le calcul
(`TryBreakWaitCycle`, `JunctionRules.ReleasesWaitCycle`, profondeur 12).

## Route Replanning

- **Declencheur** : palier de recuperation >= 1, une seule fois par episode
  (`replanAttemptedForBlock`).
- **Regle** : `TryReplanTowardsExit` n'agit **que si un bloqueur est reellement percu** — la seule
  immobilite d'un voisin ne suffit pas. Le waypoint courant est conserve jusqu'a sa **vraie arrivee**
  (`HasArrivedAtWaypoint`) : un successeur n'est pas un raccourci spatial. La cible vient de
  `LaneGraphRouting.FindNextTowardReachableExit` (parcours vers le portail de sortie atteignable le
  plus proche) et doit passer `ValidatePath`.
- **Impasse** : sans successeur, `FindNextTowardReachableExit` rend le noeud courant — le vehicule ne
  se teleporte pas vers une sortie lointaine.
- **Porteur** : `TryReplanTowardsExit`, `LaneGraphRouting.FindNextTowardReachableExit`.

## Recovery Escalation

**Un seul declencheur, un seul escalier.** Le declencheur est l'**absence de progres le long de la
route** : la distance au waypoint qui diminue est le seul signe qui ne se laisse pas tromper — une
roue qui patine, un vehicule qui pivote sur place ou qui recule en boucle ont tous une vitesse non
nulle sans avancer d'un metre (`TickRouteProgress`, seuil `ProgressDistance`).

`JunctionRules.ResolveRecoveryStage(stalledSeconds, hornDelay, escalationDelay)` — seuils **deja
authores**, aucune constante de temps nouvelle :

| palier | seuil (valeurs authorees) | action | limite |
|---|---|---|---|
| 0 | < 2 s | conduite nominale | — |
| 1 | 2 s | replanification, puis contournement sur la chaussee | `ValidatePath` |
| 2 | 14 s | recul (`ReverseSpeed` x `ReverseDuration`) | `ValidatePath` |
| 3 | 26 s | manoeuvre elargie, espace voisin, **trottoir** | `ValidatePath` + surface autorisee |
| 4 | 38 s | **dernier recours** : un vehicule IA **a l'arret** cesse d'etre infranchissable | pietons, joueurs et decor restent infranchissables |

- **Le dernier recours n'est pas un mode de conduite.** Il ne tolere que le frottement contre un
  vehicule IA immobile (`IsStalledPeerVehicle`), parce qu'un frottement vaut mieux qu'un carrefour
  fige pour toujours. Il rejoue aussi les paliers deja echoues, parce que la **validation** a change :
  ce n'est plus le meme essai.
- **Attente bornee = horloge gelee.** Un feu rouge, et une file derriere un vehicule retenu par un
  feu, ne font **pas** avancer l'horloge (`heldByBoundedWait`, propage d'un cran par pas le long de la
  file).
- **Garde anti-boucle** : `failedManeuverMask` interdit de rejouer un palier echoue dans l'episode,
  `retryAfter` (`RetryCooldown`, 2 s) borne les essais, et `ResetUnblockingEpisode` ne remet tout a
  zero qu'apres une **reacquisition reelle** de la voie, du sens et de la route
  (`HasRecoveredRoute`).
- **Fin de manoeuvre** : `RejoinRouteAfterManeuver` n'avance le waypoint que sur les segments
  **reellement franchis** ; un portail ne peut jamais etre signale par cette reconciliation. Si la
  route n'est pas reacquise, la manoeuvre est **annulee** (« retour hors voie ou a contresens »).
- **Aucun palier ne retire le vehicule** : il n'existe ni despawn ni teleportation dans cette echelle.
  Le seul repositionnement du projet vit a l'etage 0/1 (voir *Emergency Behaviour*) et releve de la
  Story 5.14, pas de l'echelle de trafic.
- **Porteur** : `TickRouteProgress`, `TryEscalateRecovery`, `JunctionRules.ResolveRecoveryStage`.

## Tunnel / Despawn Routing

- **Apparition** : `PortalTrafficSpawner` insere au **portail d'entree** quand la zone est libre
  (`PortalClearanceRadius`, 12 m). Sinon le vehicule reste en file et retente au pas suivant.
- **Disparition** : atteindre un **portail de sortie** rend `reachedExitPortal` et declenche
  `NetworkObject.Despawn()`. Un portail « sortie reutilisant l'entree » ne despawne pas a la
  naissance : `hasDepartedSpawnNode` distingue « je viens de naitre ici » de « j'y reviens apres avoir
  circule ».
- **Geometrie mesuree** : les 4 portails du district sont poses exactement sur le bord de l'emprise
  (x et z dans [-47,6 ; 47,6]). Chacun donne sur **12,00 m** de route droite puis 2,26 m jusqu'au
  premier noeud d'anneau — **14,26 m** de course avant la premiere decision. Ce troncon **ne peut pas
  etre allonge vers l'exterieur** sans agrandir le district. Il est **suffisant** : a 1,50 m/s2 le
  vehicule atteint 6,00 m/s sur 12 m, et s'arreter depuis 6,00 m/s au confort demande 9,0 m.
- **Porteur** : `PortalTrafficSpawner.cs`, `ResolveNextNode`.

## Emergency Behaviour

Trois declencheurs, tous **au-dessus** de toute regle de trafic, tous porteurs d'un
**repositionnement** au waypoint (`RecoverAtWaypoint` : vitesses annulees, pose replacee, manoeuvre
annulee, snapshot invalide).

| Declencheur | Condition | Porteur |
|---|---|---|
| Chute sous le monde | `y < voidHeightThreshold` | `NetworkedVehicleDriverController.IsBelowVoidHeightThreshold` |
| Retournement soutenu | `transform.up` sous le seuil pendant `rolloverSustainedSeconds` | `IsRolledOver` |
| Encastrement geometrique soutenu | penetration reelle > `SafetyMargin`, lenteur soutenue, **et pas d'arret volontaire** (`DriverModel.IsDeliberateStop`), **et aucun joueur a proximite** | `RefreshPerception` + `FixedUpdate` |

**La lenteur seule, meme durable et sans leader, ne teleporte jamais.** Il faut un encastrement
geometrique mesure.

**Freinage d'urgence** (`EmergencyBrake`) est un etat de decision, pas un repositionnement : il
couvre le face-a-face et la perception saturee. Une perception saturee (>= 64 colliders) **arrete**
le vehicule et emet un avertissement : un ensemble incomplet n'autorise aucune conclusion.

---

## Master table

| Situation | Detection | Qui cede / agit | Comportement attendu | Recuperation / escalade | Implementation |
|---|---|---|---|---|---|
| Croisiere | aucun etage superieur | — | IDM vers `DesiredSpeed` bruitee, visee sur polyligne | — | `FixedUpdate`, `DriverModel` |
| Courbe / giratoire | rayon minimal sur la fenetre avant | soi-meme | plafond de vitesse tenable par le braquage | — | `ResolvePathMinimumRadius`, `ResolveCurveSpeedLimit` |
| File | acteur devant, dans le couloir, meme axe | le suiveur | IDM, approche douce | file immobile 12 s -> plus legitime | `TrySelectLeader`, `DriverModel` |
| Obstacle immobile | collider sans Rigidbody dans le couloir | soi-meme | arret, puis contournement | echelle des palier 1 | `RefreshPerception`, `TryStartDetour` |
| Conflit de trajectoire | recouvrement des deux emprises predites | arrivee finie d'abord, puis anneau, puis ordre d'arrivee, puis plus petit id | ralentir **jusqu'a la ligne de cession**, puis arret | rupture de cycle ; echelle si sans issue | `FindPathConflict`, `ResolveConflictYield`, `ResolveConflictStopGap` |
| Entree de giratoire | anneau deduit de la topologie | l'entrant cede a celui qui **circule** | arret a la ligne de cession | un pair immobile perd sa priorite | `BuildRingIndex`, `CompareRingPrecedence` |
| Intersection authoree | approche a portee de sa ligne | vainqueur des duels par `Resolve` | arret **a la ligne**, puis engagement complet | deblocage apres 12 s si un autre attend | `TickJunctionRules`, `ResolveWinnerIndex` |
| Feu rouge | plan authore + `Time.time` | groupe non autorise | arret a la ligne, horloge de blocage **gelee** | aucune : l'attente est bornee | `PhaseAllowsGroup` |
| Stop | regle authoree | soi-meme | 1,2 s a l'arret **et** ecart accepte | maintien acquis reste acquis | `JunctionClaim.Eligibility`, `StopGapAccepted` |
| Sortie saturee | zone de degagement occupee **ou indeterminee** | soi-meme | ne pas entrer | echoue **fermee** | `HasExitRoom`, `OccupiesExitLane` |
| Face-a-face | cap oppose a la tangente **et** a notre cap | **personne** (pas d'arbitrage) | les deux s'arretent | mal place au palier 1, l'autre au palier 3 | `MustYieldHeadOn`, `TryEscalateRecovery` |
| Face-a-face contre un pair arrete | idem + pair sous le seuil d'immobilite | soi-meme | traite comme un obstacle | contournement des le palier 1 | `headOnStationary` |
| Joueur / pieton en travers | sur la chaussee, dans la voie, a 1,05 m | **l'IA, toujours** | contournement immediat, sinon arret | pas de delai de klaxon | `playerImmediate`, `TryStartDetour` |
| Pieton sur trottoir | surface non chaussee | personne | ignore de l'arbitrage | — | `IsOnCarriageway` |
| Cycle d'attente | chaine `WaitsFor` refermee | plus petit `NetworkObjectId` du cycle | un seul libere | `DeadlockRecovery` | `ReleasesWaitCycle` |
| Enchevetrement / gridlock | aucun progres vers le waypoint | soi-meme | escalier 1 -> 4 | dernier recours : contact limite tolere | `ResolveRecoveryStage`, `IsStalledPeerVehicle` |
| Retournement / chute / encastrement | geometrie physique | soi-meme | repositionnement au waypoint | hors echelle de trafic (Story 5.14) | `RecoverAtWaypoint` |
| Perception saturee | >= 64 colliders | soi-meme | arret prudent + avertissement | aucune manoeuvre validee | `RefreshPerception`, `ValidatePath` |
| Sortie de carte | portail de sortie atteint apres avoir circule | — | despawn reseau | — | `PortalTrafficSpawner` |

---

## Debug states / reasons

`TrafficDecisionReason` (valeurs telles qu'elles apparaissent dans le libelle de debug) :

| Etat | Sens | Preuve affichee (`decisionDetail`) |
|---|---|---|
| `Cruise` (0) | rien ne contraint | — |
| `FollowingSameLane` (1) | suivi, ou leader arrete | leader, voies, ecart, vitesse du leader, attente |
| `YieldTrajectory` (2) | cession d'un point de conflit | pair, deux arrivees, **motif du verdict**, distance a la ligne |
| `PlayerCollisionRisk` (3) | conflit avec un usager non arbitrable | idem |
| `StaticObstacle` (4) | obstacle immobile dans la voie | objet, ecart, voie |
| `PerceptionSaturated` (5) | tampon plein, aucune conclusion possible | — |
| `JunctionYield` (6) | permission de jonction refusee | jonction, mouvement, permission, revendications, distance a la ligne |
| `JunctionBreach` (7) | entree forcee (deblocage) | idem |
| `UnblockingManeuver` (8) | manoeuvre de degagement en cours | — |
| `DeadlockRecovery` (9) | cession relachee par rupture de cycle | — |
| `TrafficQueue` (11) | file **legitime** | leader, voies, ecart, attente |
| `ExitSaturated` (12) | pas de place sur la voie de sortie | libre / requis / utile / occupants / premier occupant |
| `PlayerImmediateConflict` (13) | joueur a portee de contact dans la voie | joueur, ecart, relation |

`DescribeDecision()` (affiche par `OnDrawGizmosSelected` sur le vehicule selectionne) ajoute : voie
courante et sortie prevue, jonction et permission, distance a la ligne et maintien, leader, conflit
avec son **verdict et son motif** et le cote mal place en face-a-face, `Depend de` (chaine
d'attente), **palier de blocage et duree sans progres**, place de sortie, motif de refus.

Sondes hors `Assets/` (via `unity cmd run_script`) : `Story518LiveTrafficProbe` (etat complet du
trafic en cours, chaines d'attente, immobiles, contresens), `Story518StopLineProbe` (intrusion des
lignes d'arret), `Story518YieldBrakingProbe` (profil de freinage), `Story518PortalRunwayProbe` et
`Story518PortalChainProbe` (geometrie portail -> giratoire), `Story518RingPriorityProbe`.

---

## LaneGraph assumptions required by the traffic AI

1. **L'index de noeud est identique sur tous les pairs.** Il vient de l'ordre de collecte de la
   hierarchie de scene, et sert directement de `NetworkedAIVehicleState.WaypointIndex`.
2. **Chaque noeud porte une rotation exploitable** : `GetNodeRotation(i) * Vector3.forward` est le
   sens de circulation authore. Tout le reste en depend — echantillonnage des courbes, cap d'un pair
   arrete, detection d'anneau, conformite de voie.
3. **Les ratios de virage sont authores par noeud** et strictement positifs ; un poids invalide est
   signale une fois par noeud.
4. **Les jonctions sont regroupees par module porteur** (`ResolveJunctionKey`), et les approches
   d'une meme jonction partagent sa cle.
5. **Une approche a un predecesseur aligne** a moins de 4 sauts, sinon le noeud de decision se sert
   de frontiere a lui-meme — et la ligne d'arret tombe alors sur le noeud.
6. **Les giratoires sont des cycles fermes d'au moins 5 noeuds** dont chaque successeur est aligne
   sur l'axe authore. Un anneau plus court, ou dont un noeud pointe ailleurs, n'est pas detecte.
7. **Les portails d'entree et de sortie sont marques sur les noeuds.** Sans portail de sortie
   atteignable, la replanification rend le noeud courant.
8. **La chaussee et le trottoir sont reconnus par NOM de collider** (`Col_Roadway*`,
   `Col_Sidewalk_*`, `Col_Curb_*`) — convention greybox transitoire, a remplacer par des metadonnees
   de surface.
9. **`TrafficSettingsDef` est assigne.** Sans lui : aucun vehicule insere, connecteurs non joints,
   aucun feu, et le degagement de mouvement tombe a 0 — la ligne d'arret retombe alors sur
   l'intersection stricte des axes sans epaisseur.
10. **Les axes opposes sont separes d'environ 4,00 m** (mediane mesuree sur 164 paires du district),
    soit 2,00 m de demi-chaussee pour un demi-gabarit de 1,03 m : **0,97 m de budget d'erreur
    laterale**.

---

## Known limitations

1. **Aucun changement de voie.** `TickLaneChangeEvaluation` n'entretient qu'un compteur. Un vehicule
   ne depasse jamais autrement que par une manoeuvre de deblocage.
2. **Pas de detection de contresens en conduite nominale.** Le contresens n'est constate que dans un
   face-a-face ou en fin de manoeuvre.
3. **Le LaneGraph n'a aucune largeur de voie authoree.** Le couloir est derive a l'execution
   (`demi-largeur + SafetyMargin`), et le degagement de jonction est une valeur unique de monde, pas
   une donnee par voie. *(Review Finding #3, ouverte.)*
4. **`JunctionApproachRadius` est lu sur le profil de l'OBSERVATEUR.** Deux conducteurs aux profils
   differents peuvent donc construire des ensembles de revendiquants differents. Le rayon devrait
   etre une donnee de monde. *(Review Finding #6, moitie ouverte.)*
5. **L'occupation de la sortie arbitree n'est pas projetee sur l'axe authore.** *(Review Finding #7,
   moitie ouverte.)*
6. **Un seul partenaire de conflit a la fois.** Un troisieme usager attend le tour de perception
   suivant.
7. **Tampons fixes** : 8 revendiquants par jonction, 64 colliders de perception, 64 de pose, 32
   points de trajectoire, profondeur 12 pour les cycles d'attente. Une saturation est **dite** et
   provoque un arret prudent, jamais une conclusion.
8. **Plancher physique du virage serre : 1,35 m d'erreur laterale** pour 0,97 m de budget. Les
   virages a 90 deg les plus serres du district demandent 35,8 deg de braquage pour 35,9 disponibles.
   Seul l'authoring (intersection plus large, ou vehicule plus court) le deplace.
9. **La ligne de cession est projetee sur la corde**, pas sur l'arc : en courbe elle sous-estime la
   distance restante et arrete un peu tot. Sens conservateur, mais ce n'est pas exact.
10. **Le dernier recours tolere le contact** entre vehicules IA immobiles. C'est un choix assume :
    preserver la circulation prime la conformite parfaite en situation de gridlock severe.
11. **Les paliers 3 et 4 s'atteignent a 26 s et 38 s** d'immobilite complete. Un gridlock severe reste
    donc visible longtemps avant d'etre traite.

## Unresolved edge cases

1. **Une file longue arretee a un feu rouge depuis plus de 12 s** perd sa legitimite de file si la
   propagation `heldByBoundedWait` est rompue (leader hors perception, ou leader non IA). L'horloge
   repart alors et un contournement peut etre tente.
2. **Deux vehicules tous deux a l'arret et en conflit** retombent sur l'ordre d'arrivee puis sur le
   plus petit identifiant : le verdict est defini, mais aucun des deux ne bouge tant que l'echelle
   ne les prend pas.
3. **Saturation du tampon de revendiquants (> 8)** rend la permission de jonction indeterminee
   jusqu'au palier de deblocage. Non observe en recette mais representable.
4. **Les trois suites Play Mode de la Story 5.18** (`Story518NormalTrafficFlowPlayModeTests`,
   `Story518IntersectionRulesAndDeadlockPreventionPlayModeTests`,
   `Story518RuntimeEvidencePlayModeTests`) n'ont pas ete rejouees depuis ces correctifs : le lanceur
   Play Mode ne supporte **qu'une execution par session d'editeur**, toute invocation ulterieure
   rendant `total: 0` jusqu'a un redemarrage manuel.
5. **Aucun run Play Mode complet sans vehicule bloque n'a encore ete valide.** Tout ce qui precede est
   prouve au niveau du modele (EditMode, geometrie reelle de `MVP_Run`) et par la mesure ; la
   confirmation en conditions reelles reste due.
6. **Le contact tolere au palier 4 peut pousser un tiers** : `ValidatePath` verifie les poses de
   **notre** gabarit, pas la reaction physique du vehicule franchi.
