---
title: 'Recherche technique : trafic IA vehicules Unity 6'
type: 'technical'
topic: 'Systemes de trafic et IA vehicules pour Unity 6 (RoadRage Simulator, Epic 5)'
decision: 'Build interne / Adapt un systeme existant / Reference seulement -- pour le trafic urbain IA de l Epic 5 etendu, avec une couche rage/peur qui doit pouvoir modifier reellement les decisions de conduite et de navigation'
source: 'bmad-deep-recon (run)'
status: complete
preset: 'standard (subagents releves a 5)'
validation: 'normal'
shape: 'select'
created: '2026-09-15'
updated: '2026-09-15'
claims_verified: 19
claims_unverified: 6
claims_overturned: 1
claims_note: 'Comptes issus de recon_kit tally sur le memlog (rollup par ref, dernier statut gagnant), jamais comptes a la main.'
---

# Recherche technique : trafic IA vehicules Unity 6

**Decision servie :** Build interne / Adapt un systeme existant / Reference seulement, pour le trafic urbain IA de l'Epic 5 etendu de RoadRage Simulator.

---

## Resume executif

**Verdict : construire en interne, sur les blocs natifs Unity, en portant IDM + MOBIL depuis `highway-env` (MIT) comme reference lue — pas comme dependance.** Score pondere 4.10/5 contre 3.00 pour l'adoption de Gley et 2.35 pour l'adoption d'un systeme open-source. Le runner-up est Gley Mobile Traffic System v3, et il gagne dans un seul scenario, nomme plus bas.

Trois constats portent cette reponse.

**1. La couche de decision dont le projet a besoin est petite — environ neuf flottants et une fonction de modulation.** Ce n'est pas une intuition : le modele IDM calcule l'acceleration a partir de six parametres nommes, et le modele de changement de voie MOBIL en ajoute trois, dont un facteur de politesse dont la documentation de son propre co-auteur decrit la plage negative comme « une personnalite malveillante qui prend plaisir a contrarier les autres conducteurs meme au prix de son propre desavantage » [11]. SUMO expose tout cela comme attributs par type de vehicule, et ajoute `impatience` — « la volonte du conducteur d'entraver des vehicules ayant une priorite superieure […] utilisera n'importe quel creneau *sur* au sens de l'evitement de collision, meme si cela signifie qu'un autre vehicule doit freiner aussi fort qu'il le peut » [13]. C'est litteralement la rage aux intersections, deja modelisee. Le systeme de rage actuel de RRS pilote `v0` (vitesse desiree) — qui est **l'axe de personnalite le moins expressif des six**.

**2. Gley, seul candidat reellement maintenu, expose le mauvais etage.** Son API de surcharge est reelle et per-vehicule (`API.SetVehicleBehaviours`, sous-classement de `VehicleBehaviour` avec `Execute()`) — ce n'est pas du marketing [7]. Mais la sortie de `Execute()` est `BehaviourResult { SteerPercent, BrakePercent, MaxAllowedSpeed, TargetGear, SpeedInPoint }` : **une structure de commande, pas une structure de parametres** [7]. Sur plus de 60 pages d'API, aucun temps inter-vehiculaire, aucun ecart minimal, aucune deceleration confortable, aucun seuil d'acceptation de creneau, aucune politesse. Le seul scalaire de reglage per-vehicule nomme est `SetSpeedVariationPercentage` — exactement le levier « juste la vitesse » que le cadrage a ecarte. **Une seule exception potentielle :** le delegue `Modify Trigger Size` module le volume de detection avant, et le reduire fait coller le vehicule — mais il est documente comme fonction de la *vitesse*, pas de l'identite du vehicule, et sa variabilite per-vehicule n'a pas pu etre verifiee [7]. Les constantes que la rage doit moduler vivent *a l'interieur* des classes `Follow Vehicle`, `Overtake`, `Change Lane` et `Give Way` livrees ; les atteindre signifie forker precisement les quatre classes dont on a le plus besoin, et les refusionner a la main a chaque mise a jour. Symptome corroborant, d'un utilisateur payant (rapport d'ere v2) : « les voitures IA semblent freiner tres brutalement […] ça casse vraiment l'immersion » [9] — c'est la signature d'un freinage en tout-ou-rien sur entree de trigger, pas d'un modele de poursuite continu.

**3. Rien n'est a construire contre le courant technique.** ~30 vehicules reseau n'est pas un probleme : Unity situe la ligne du « repensez votre architecture » a « des centaines jusqu'a 1000+ » NetworkObjects [25]. La bande passante n'est pas la contrainte liante ; le CPU hote est ce qu'il faut profiler. Et le champ open-source ne fournit aucune alternative : **aucun systeme de trafic Unity open-source ne revendique Unity 6, et aucun candidat — gratuit ou payant — ne documente Netcode for GameObjects** [3][7]. Le cablage host-authoritative sera ecrit par le projet quel que soit le choix.

**Le plus gros bemol, honnetement.** Les preuves nomment l'**outillage d'authoring des voies et l'integration** comme gouffre de temps, pas la logique de conduite [31][32][33] — et c'est precisement la part que l'option « build » n'evite pas. Ce biais est reel mais partiellement d'echantillonnage : les avis Asset Store sont ecrits en semaine 1 a 4, quand l'UX d'authoring est ce qu'on rencontre en premier, tandis que le blocage d'intersection est un probleme de mois 3. Traiter « l'authoring est le gouffre » comme hypothese etayee, et « l'interblocage » comme **non teste plutot que refute**. Par ailleurs, **aucune source publique n'indique combien de temps prend un trafic urbain Unity construit de zero** — zero compte-rendu, sur sept sources lues [37]. C'est le chiffre que cette decision voudrait le plus, et il n'existe pas publiquement.

**Condition de bascule vers Gley :** si apres un prototype d'authoring la construction du graphe de voies et des intersections s'avere dominer le calendrier, *et* que la rage/peur reste acceptable en tant que commutation de strategie (« ce conducteur passe en comportement Ignore Traffic Rules ») plutot que modulation continue de parametres, alors les €118.69 achetent le vrai cout. Ce pari doit etre pris en connaissance de cause : il plafonne l'ambition de la couche emotionnelle a l'etage commande.

---

## Cadre de decision (frame)

Etabli depuis le projet et l'utilisateur, avant toute recherche candidat. La recherche web ne fixe pas les exigences.

### Etat actuel du systeme (point de depart, non issu de la recherche)

- `NetworkedAIVehicleDriverController` : poursuite d'une liste unique de waypoints en boucle (`RouteWaypoints`), vitesse imposee directement sur le Rigidbody, host-only, repliquee par NetworkTransform.
- Aucune notion de voie, d'intersection, de detection d'obstacle. Le deblocage est une teleportation au waypoint courant.
- Les etats de rage sont des multiplicateurs de vitesse de croisiere (Calm 1.0, Irritated 1.25, Flee 1.6, Block 0, Ram 1.9, ConfrontationCapable 0).
- `com.unity.ai.navigation` 2.0.14 est installe mais jamais utilise.

### Gates durs (eliminatoires)

| # | Gate | Source projet |
| --- | --- | --- |
| G1 | Gratuit, libre de droits, idealement open-source, ou package Unity/Steamworks officiel. Payant ou closed-source : approbation humaine explicite documentee requise. | Story 0.7 ; `sprint-change-proposal-2026-09-02.md` ; `docs/setup/addon-adoption-register.md` |
| G2 | Compatible Unity `6000.6.0f1` et URP `17.6.0`. | `Packages/manifest.json` ; ADDON-001/002 |
| G3 | Compatible Netcode for GameObjects `2.13.2`, simulation host-authoritative, clients presentation-only. | AD-1/AD-4 ; Story 5.7 |
| G4 | Ne possede pas la verite gameplay centrale (rage, peur, issue de run, autorite reseau). | Registre d'adoption, declencheurs de rejet |
| G5 | Source disponible et pratiquement editable : la couche rage/peur doit pouvoir intervenir dans la logique de decision, pas seulement la contourner. | Exigence utilisateur, 2026-09-15 |

### Criteres ponderes

| Poids | Critere | Ce qui est evalue |
| --- | --- | --- |
| **35 %** | **Extensibilite de la couche decision/navigation** | Ou peut-on reellement intervenir ? La rage/peur doit pouvoir influencer les decisions de conduite, la prise de risque, les reactions aux autres vehicules, le choix de trajectoire ou de cible — durablement, sans contourner ni remplacer le systeme adopte. |
| 15 % | Support du trafic source/sink route | Entrees/sorties de type tunnel, parcours varies, pas de despawn artificiel en pleine route, maintien d'une densite constante. |
| 15 % | Comportement urbain credible baseline | Double sens, intersections, stops, reaction aux obstacles, attente puis contournement raisonnable, pas de trottoirs. |
| 10 % | Tenue a l'echelle cible | ~30 vehicules actifs simultanement, davantage si les performances le permettent. Les ~10 vehicules ne sont qu'un scenario de test de l'Epic courant. |
| 15 % | Cout d'adoption et cout de sortie | Courbe d'apprentissage solo, effort d'adaptation reel, cout de retrait. |
| 10 % | Sante de l'ecosysteme | Vitalite des releases, compatibilite Unity 6 verifiee, risque de regret a moyen terme. |

### Scenario cible (pour evaluation, pas une architecture imposee)

Les vehicules entrent dans la zone de jeu depuis des points representant l'exterieur de la ville (tunnels), suivent differents parcours dans la ville, avec des routes variables entre entree et sortie, puis rejoignent une sortie logique. Aucun despawn en pleine route : uniquement a la sortie naturelle. De nouveaux vehicules apparaissent depuis les entrees pour maintenir un trafic relativement constant. L'impression visee est celle de vehicules venus de l'exterieur qui traversent la ville puis la quittent, pas une boucle de waypoints visible.

---

## D1 — Champ des candidats et screen

**Le champ open-source est structurellement mort.** Tous les projets de trafic Unity open-source ayant une traction communautaire reelle (plus de 40 etoiles) ont recu leur dernier commit entre 2019 et 2023 [3]. Les seuls depots pousses dans les six derniers mois sont soit des ponts de co-simulation SUMO, soit des boites a outils DOTS/ECS, soit des projets personnels a zero etoile crees il y a quelques semaines. **Aucun systeme de trafic Unity open-source ne revendique le support Unity 6 / 6000.x** [3].

Plus decisif encore : **aucun systeme de trafic, gratuit ou payant, ne documente le support de Netcode for GameObjects** [3][7]. La documentation de Gley ne comporte aucune page reseau. G3 n'est le probleme resolu de personne — l'integration host-authoritative sera ecrite par le projet quelle que soit l'option retenue. Ce constat retire a l'option « adopt » une part importante de ce qu'elle est censee acheter.

Cote natif, les deux blocs passent tous les gates et sont version-stampes pour l'editeur 6000.6 : **AI Navigation 2.0.14** (construction de navmesh a l'execution et en edition, obstacles dynamiques, NavMeshLinks) [1] et **Splines 2.8.4**, dont les dependances Burst et Physics sont devenues optionnelles en 2.8.x — donc utilisable sans ajouter de stack [2]. Tous deux laissent 100 % de la logique de decision dans le code projet. Risque du chemin natif : **la documentation d'AI Navigation ne dit rien des agents de type vehicule sur les deux pages lues** [1] — un silence releve sans balayage documentaire exhaustif, et qui reste le vrai risque du chemin natif ; D4 le caracterise.

Unity ne livre **aucun exemple de trafic sur la pile GameObject/MonoBehaviour** ; ses seuls echantillons adjacents (Megacity, Megacity Metro) sont DOTS + Netcode for *Entities*, donc la mauvaise famille reseau et la mauvaise pile [6].

| Candidat | Type | Licence | Cout | Unity 6 ? | Dernier commit | Source ouverte | Verdict de screen |
|---|---|---|---|---|---|---|---|
| **AI Navigation 2.0.14** | Package natif | Unity (UCL) | Gratuit | **Oui, version-stampe 6000.6** [1] | courant | Lisible | **Retenu comme bloc.** Possede le pathfinding seul ; toute decision reste au projet. |
| **Splines 2.8.4** | Package natif | Unity (UCL) | Gratuit | **Oui, version-stampe 6000.6** [2] | courant | Lisible | **Retenu comme bloc.** Porteur de geometrie de voies, Burst/Physics optionnels. |
| **Gley MTS v3** | MonoBehaviour + Burst | EULA Asset Store | **€118.69** [8] | Revendique, editeur d'origine 2022.3.62 [8] | v3.6.4, 2026-09-04 [8] | **Oui, complete et commentee** [7] | **Finaliste (adapt).** Seul candidat reellement maintenu. Necessite approbation G1. Voir D2 pour la limite decisive. |
| **Kink3d/SimpleTraffic** | MonoBehaviour + NavMesh | **MIT (fichier LICENSE reel)** [28] | Gratuit | Non (Unity 2017.2+) | **2019-05-01** [3] | Oui | **Reference seulement.** Meilleur exemple public d'agents NavMesh contraints en voies avec logique de jonction. |
| **mchrbn/unity-traffic-simulation** | MonoBehaviour, waypoints | **MIT valide — texte complet dans le README** [28] | Gratuit | Non (Unity 2018.3+) | 2022-08-08 [3] | Oui | **Reference seulement.** Arbitrage de stop en ~30 lignes, digne d'etre copie. |
| **SUMO2Unity** | Pont de co-simulation | **Aucune detectee** | Gratuit | Non indique | 2026-09-05 (actif) [3] | Oui | **Ecarte (G4 + G1).** SUMO possede le mouvement et le routage dans un processus externe : la rage n'atteindrait jamais la couche de decision. |
| **Megacity Metro** | DOTS + Netcode for Entities | Sample Unity | Gratuit | Maintenu par Unity [6] | actif | Oui | **Reference seulement.** Mauvaise famille reseau et mauvaise pile. |
| Traffic-Toolkit-DOTS, oddmax-DOTS, Easy-Road-3D-ECS | DOTS/ECS | Aucune detectee | Gratuit | Non | 2021–2025 [3] | Oui | **Ecartes.** Adopter la pile DOTS pour ~30 vehicules est disproportionne ; aucune licence. |
| tetreum/peque-traffic | MonoBehaviour | Aucune | Gratuit | Non | **ARCHIVE** 2021-10-15 [3] | Oui | **Ecarte — mort declare par l'auteur.** |
| furic/ambient-traffic-jam | MonoBehaviour, URP | Aucune detectee | Gratuit | Non evidence | 2026-08-27 [3] | Oui | **Ecarte.** Auto-decrit comme *cosmetique* : par construction, aucune couche de decision a accrocher. |

---

## D2 — Extensibilite de la couche decision/navigation

C'est la dimension qui tranche, et l'hypothese s'y verifie avec des sources primaires : **les modeles microscopiques de trafic exposent la personnalite du conducteur comme une petite structure de flottants.**

**IDM** calcule l'acceleration a partir de six parametres nommes — vitesse desiree `v0`, temps inter-vehiculaire `T`, acceleration maximale `a`, deceleration confortable `b`, espacement a l'arret `s0`, exposant `δ` — via `v̇ = a(1 − (v/v0)^δ − (s*/s)²)` [10]. **MOBIL** se superpose a *n'importe quel* modele de poursuite et ajoute trois leviers, dont la politesse `p` dont Treiber lui-meme documente les bandes : `p = 0` « comportement purement egoiste » ; `p > 1` « tres altruiste » ; **`p < 0` « une personnalite malveillante qui prend plaisir a contrarier les autres conducteurs meme au prix de son propre desavantage »** [11]. Le modele livre un cadran de malveillance.

**SUMO expose tout cela comme attributs par type de vehicule**, et fournit les deux trouvailles les plus utiles pour RRS [13] :

- **`impatience`** — « la volonte du conducteur d'entraver des vehicules ayant une priorite superieure. A une valeur de 1 ou plus, le conducteur utilisera n'importe quel creneau *sur* au sens de l'evitement de collision, meme si cela signifie qu'un autre vehicule doit freiner aussi fort qu'il le peut. » C'est le parametre le plus directement rage-forme trouve : il vit aux intersections, la ou un jeu de conduite cooperatif produit l'essentiel de son drame.
- **`lcImpatience` (−1..1)** — « facteur dynamique modifiant `lcAssertive` et `lcPushy`. L'impatience agit comme un multiplicateur. A −1 le multiplicateur vaut 0.5 et a 1 il vaut 1.5. » **C'est exactement le motif « l'emotion module la baseline sans la remplacer »**, deja implemente dans un simulateur ouvert et mature : un scalaire emotionnel transitoire qui multiplie les constantes de personnalite statiques. Le precedent nomme que le cadrage cherchait existe — en simulation de trafic, pas en IA de jeu.

### La carte des leviers

| Modele | Levier | Effet comportemental | Rage / Peur | Source |
|---|---|---|---|---|
| IDM | `T` (temps inter-vehiculaire, ~1.5 s) | Bas = collage au pare-chocs ; haut = timide | **Rage : baisser. Peur : monter.** Le levier le plus lisible. | [10] |
| IDM | `v0` (vitesse desiree) | Vitesse de flux libre visee | Rage : monter. *C'est le seul levier utilise par RRS aujourd'hui — soit l'axe le plus faible.* | [10] |
| IDM | `a` (acceleration max) | Demarrages brusques vs souples | Rage : monter. Peur : baisser. | [10][28] |
| IDM | `b` (deceleration confortable) | Freine tard et fort vs tot | Rage : monter. Peur : baisser. | [10][28] |
| IDM | `s0` (ecart a l'arret) | Collage en file | Rage : reduire. Peur : agrandir. | [10][28] |
| MOBIL | **`p` (politesse)** | 0 = egoiste ; **< 0 = malveillance deliberee** | **Le levier de rage par excellence.** | [11] |
| MOBIL | `a_th` (seuil de changement, 0.2) | Haut = voies stables ; bas = zigzag incessant | Rage : baisser. Peur : monter. | [11] |
| MOBIL | `b_safe` (freinage impose, ~4 m/s²) | Combien de freinage on impose a celui qu'on rabat | Rage : monter vers la limite physique ~9 = faire des queues de poisson. | [11] |
| SUMO | **`impatience`** | Volonte d'entraver un prioritaire | **La rage aux intersections.** | [13] |
| SUMO | `lcAssertive` | « l'ecart requis est divise par cette valeur » | Rage : monter. | [13] |
| SUMO | `lcCooperative` (0..1) | Cooperation au changement de voie | Rage → 0. Peur → 1. | [13] |
| SUMO | **`lcImpatience` (−1..1)** | Multiplicateur dynamique 0.5×–1.5× sur les constantes | **Le motif lui-meme.** | [13] |
| SUMO | `sigma` (0..1) | « imperfection du conducteur » | Rage et peur degradent toutes deux la precision. | [13] |
| SUMO | `actionStepLength` | Decouple la frequence de decision du tick, « modelise les effets de temps de reaction » | **Peur : allonger = reactions lentes.** | [13] |

### Ou Gley se situe

L'API de surcharge est reelle, nommee et per-vehicule : `API.SetVehicleBehaviours(int vehicleIndex, IBehaviourList)`, logique personnalisee ecrite en sous-classant la classe abstraite `VehicleBehaviour` et en surchargeant `Execute()`, appele a chaque update [7]. Vingt comportements sont livres, dont plusieurs deja rage-formes — `Ignore Traffic Rules`, `Overtake Player`, `Follow Player`, `Clear Path` — fusionnes chaque frame en une commande unique [7]. Dix evenements et quatre delegues completent la surface.

**Le probleme est l'etage de la sortie.** `Execute()` retourne `BehaviourResult { SteerPercent, BrakePercent, MaxAllowedSpeed, TargetGear, SpeedInPoint }` — **une structure de commande, pas une structure de parametres** [7]. Sur plus de 60 pages d'API documentees, il n'existe aucun temps inter-vehiculaire, aucun ecart minimal, aucune deceleration confortable ou maximale, aucun seuil d'acceptation de creneau, aucune politesse de changement de voie, aucun scalaire d'agressivite. Le seul scalaire de reglage per-vehicule nomme est `SetSpeedVariationPercentage`.

**La seule nuance a cette absence**, et elle merite d'etre suivie : le delegue `Modify Trigger Size` « controle la dimension du trigger avant en fonction de la vitesse du vehicule » [7]. Ce trigger avant est le volume de detection qui alimente `Follow Vehicle` — le reduire fait donc coller le vehicule au pare-chocs, ce qui en fait l'objet le plus proche d'un levier de distance de suivi dans le produit. Mais il est documente comme fonction de la vitesse, **pas de l'identite du vehicule**, et les signatures exactes des delegues ne sont pas publiees. **S'il peut etre clef sur le vehicule, c'est un levier de collage utilisable par la couche rage ; sinon, la distance de suivi ne s'atteint que par edition de la source.** Non resolu par ce run.

La caracterisation honnete : Gley offre un point d'accroche de **commutation de strategie** per-vehicule, situe a **l'etage commande**. Il ne donne pas l'etage parametres que la conception issue de D2 vise. Les constantes IDM/MOBIL que la rage doit moduler vivent *dans* les classes `Follow Vehicle`, `Overtake`, `Change Lane` et `Give Way` livrees, sans levier expose. Les atteindre implique de lire et editer la source de Gley — permis, la source complete est livree [7] — mais c'est un fork de fait des quatre classes dont on a le plus besoin, refusionne a la main a chaque mise a jour du vendeur.

Deux elements aggravants decouverts au round 2 : **l'API entiere est `static`** — un singleton global au processus, obstacle structurel supplementaire a la replication host-authoritative [7] ; et un utilisateur payant rapporte « les voitures IA semblent freiner tres brutalement […] ça casse vraiment l'immersion » [9], symptome exact d'un freinage en tout-ou-rien sur entree de trigger plutot que d'un modele de poursuite continu.

**Aucun compte-rendu n'a ete trouve de quiconque ayant construit une couche emotionnelle personnalisee par-dessus Gley.** Absence de preuve, pas preuve d'absence — mais pour le critere numero un du projet, personne n'a publiquement emprunte ce chemin.

### Classement architectural

1. **Modele microscopique parametre (IDM + MOBIL), personnalite = struct de ~9 flottants.** Preuves primaires et ecrasantes. L'injection de la couche modulatrice est `effectif = base × f(rage, peur)` — aucun changement architectural. Limite : ne donne pas a lui seul la *selection de cible* (poursuivre le joueur qui a provoque) ; cela reste une couche au-dessus.
2. **Utility AI sur un jeu de considerations.** Le bon foyer structurel pour les decisions discretes (foncer, bloquer, fuir, conduire normalement), la rage devenant un modificateur de poids. **Preuves nettement plus faibles** : atteintes par resumes de recherche et un HTTP 403 ; le cadrage « les arbres de comportement sont depasses » rencontre est de l'advocacy, pas un post-mortem — **il ne franchit pas la barre des deux sources et n'est pas affirme ici** [36].
3. **FSM monolithique dans un MonoBehaviour** — la forme de la plupart des echantillons Unity gratuits. Chaque variante emotionnelle devient un etat ou une branche de plus : croissance combinatoire. C'est l'architecture que le cadrage a raison de craindre.

---

## D3 — Trafic source/sink route, intersections, blocage

### Le routage : les ratios de virage battent le pathfinding

SUMO livre un routeur dedie pour exactement le scenario RRS. **`jtrrouter` route les vehicules par *ratios de virage aux jonctions* plutot que par paires origine-destination** : les vehicules demarrent sur des arcs sources, tirent une decision de virage stochastique a chaque jonction, et terminent en atteignant un arc puits [14]. Les defauts `--turn-defaults` valent `30,50,20` — 30 % a gauche, 50 % tout droit, 20 % a droite [14]. La terminaison est configuree, pas calculee : `--sink-edges` nomme les sorties, **`--sources-are-sinks` reutilise les arcs d'entree comme sorties** — exactement « ressort par le tunnel d'ou il est venu » — et `--max-edges-factor` (defaut 2) elague les routes trop longues, empechant l'errance infinie [14]. La documentation partage explicitement les roles : `duarouter` (matrices OD, plus court chemin) pour la demande zone-a-zone, `jtrrouter` pour la demande au niveau intersection [14].

**Consequence pour RRS : le scenario cible est litteralement le modele `jtrrouter`.** Il ne demande ni A*, ni matrice OD, ni liste de routes authorees. La variation de parcours tombe du de lance a chaque jonction.

Sur l'insertion et la densite : quand un vehicule ne peut etre insere parce que la source est congestionnee, SUMO ne le jette pas — « ce vehicule est place dans une file d'insertion et l'insertion est retentee aux pas de simulation suivants » [15], et `--max-depart-delay` vaut `-1` par defaut, c'est-a-dire que les vehicules en file ne sont jamais ecartes pour retard [18].

### Les intersections : garder la jonction libre suffit

**SUMO applique par defaut une heuristique de non-blocage : les conducteurs n'entrent pas dans une jonction quand la route de sortie est congestionnee** (`--default.junctions.keep-clear`, defaut `true`) [16]. C'est le levier le plus rentable de tout ce dossier, parce qu'il empeche la moitie « detention » du cycle detention-et-attente de jamais se former. Cout : une verification d'occupation de la voie de sortie avant de s'engager.

La priorite est geometrique et decidee a la construction du reseau — flux prioritaires, fusion en fermeture eclair, non-prioritaires attendant en bout de voie — donc **cout d'arbitrage a l'execution nul** [16]. La gestion par reservation (Dresner & Stone, JAIR 31, 2008), ou chaque vehicule demande un creneau espace-temps a un agent d'intersection, subsume feux et stops selon ses auteurs [19] — et *eliminerait* l'interblocage par construction, puisqu'aucun vehicule n'entre sans creneau accorde, mais **cette propriete est deduite du protocole : la preuve explicite d'absence d'interblocage ne figure pas dans le resume lu.** Elle exige de toute facon un arbitre par jonction. **Pour ~30 vehicules et des regles de niveau stop, c'est plus lourd que necessaire.**

### La question des 20 secondes : la forme est fausse, pas seulement le nombre

**Aucun systeme documente n'utilise un seuil unique d'attente-puis-replanification.** Les conceptions etayees ont **deux paliers plus une rampe continue** :

| Systeme | Mecanisme | Valeur | Defaut documente ? | Source |
|---|---|---|---|---|
| SUMO | `jmTimegapMinor` — creneau minimal accepte sur voie secondaire | **1 s** | Oui | [13] |
| SUMO | `--time-to-impatience` — duree pour atteindre l'impatience maximale | **180 s**, rampe continue depuis t=0 | Oui | [13] |
| SUMO | `--waiting-time-memory` — fenetre d'accumulation de l'attente | **100 s** | Oui | [18] |
| SUMO | `--time-to-teleport` — declarer bloque et teleporter | **300 s** (vitesse < 0.1 m/s cumulee) | Oui, confirme sur deux pages | [17][18] |
| SUMO | `--ignore-junction-blocker` — contourner un bloqueur apres T | **-1 = desactive** | Oui | [16][18] |
| SUMO | `jmIgnoreKeepClearTime` — entrer dans une jonction bloquee | **-1 = jamais** | Oui | [13] |
| Cities: Skylines | minuteur de despawn pour vehicule bloque | **introuvable** | Non | [35] |

L'impatience monte continument : `impatience = MAX(0, MIN(1, base + waitingTime / 180))` [13]. **20 s se situe dans un no-man's-land probatoire** : un ordre de grandeur trop *tard* pour etre une reponse comportementale (a 20 s l'impatience SUMO vaut ~0.11, encore legere, mais la degradation a commence des la premiere seconde), et un ordre de grandeur trop *tot* pour etre une soupape d'abandon (SUMO attend 15× plus longtemps avant quoi que ce soit d'aussi violent).

**Detail decisif : l'action d'abandon de SUMO — la teleportation — est precisement la disparition en pleine route que RRS interdit**, et SUMO n'y recourt qu'apres cinq minutes [17].

**Forme defendable pour RRS, derivee plutot que devinee :** replanifier **immediatement et continument** des que le chemin est bloque — c'est une requete de graphe, elle ne coute rien et n'exige aucune periode d'attente — **escalader le comportement visible en continu** (avancer par a-coups, klaxonner, changer de voie pour depasser des qu'un creneau est acceptable), et **ne prevoir aucun palier de suppression** a proximite du joueur. Le remplacement du « attendre 20 s puis chercher une alternative » est : *replanifier en continu ; c'est l'**animation** de l'hesitation qui vend la scene, pas un minuteur reel.*

**L'echelle d'escalade documentee, par violence croissante** [13][16][17][18] : (1) `jmIgnoreKeepClearTime` — entrer dans une jonction qu'on garderait normalement libre ; (2) `--ignore-junction-blocker` — passer *a travers* un bloqueur, explicitement decrit par SUMO comme modelisant le contournement que font les vrais conducteurs ; (3) `jmIgnoreFoeProb` — ignorer probabilistement un prioritaire arrete ; (4) teleportation a 300 s ; (5) suppression pure.

**Que les etapes 1 a 3 soient toutes desactivees par defaut alors que l'etape 4 est active a 300 s est en soi le constat** : les mainteneurs de SUMO ont choisi « laisser bouchonner cinq minutes, puis tricher invisiblement » plutot que « laisser les conducteurs enfreindre les regles tot ». **Pour un jeu, cet ordre doit s'inverser** — la triche finale est exactement l'artefact que RRS interdit, donc les paliers d'infraction doivent porter la charge et doivent etre actives, pas desactives par defaut.

### Les trottoirs

Unity documente lui-meme la faiblesse de l'instrument : « L'effet des couts sur le chemin resultant peut etre difficile a regler, en particulier pour les longs chemins », les couts sont des indications [20]. **Un trottoir a cout eleve ne garantit donc pas qu'une voiture n'y monte jamais** — il ne biaise que la selection globale de chemin, et l'evitement local est un systeme separe. **Si les trottoirs doivent etre inviolables, ils doivent sortir du masque d'aire de l'agent voiture, pas seulement couter cher.**

### Non etaye

**Le recul pour se degager n'est documente par aucun systeme de trafic trouve.** La reponse de SUMO a un blocage impraticable est la teleportation, pas la marche arriere. A traiter comme une invention non etayee si RRS l'adopte — ce qui reste legitime pour un jeu, mais sans precedent a invoquer. Seule exception reperee : mchrbn declenche une marche arriere quand un vehicule detecte est proche et **oriente a contresens**, avec `acc = -0.3` et braquage derive de l'orientation relative [28] — brut, mais cela resout les interblocages qu'un modele « s'arreter quand bloque » cree.

---

## D4 — Integration host-authoritative NGO a l'echelle

**~30 vehicules reseau est confortable, d'environ un ordre de grandeur.** Le personnel Unity situe la ligne du « repensez votre architecture » a « des centaines jusqu'a 1000+ » NetworkObjects [25]. Les points de degradation rapportes par la communaute dans le meme fil — ~200 objets avec collider et gravite, ~700 objets nus — restent 7 a 23 fois au-dessus de la cible. En prenant meme le chiffre communautaire pessimiste (2000 o/s par objet mobile) : 30 × 2000 = **60 ko/s par client, ~180 ko/s en emission hote pour 3 clients**, largement dans l'enveloppe du ~1 Mo/s par client decrite [25]. **La bande passante n'est pas la contrainte liante ; le CPU hote est ce qu'il faut profiler.**

Les leviers documentes, par rentabilite decroissante [23] : desactiver les axes non synchronises (l'echelle est constante sur une voiture IA ; la rotation n'a de sens qu'en lacet) ; compression de quaternion **16 → 4 octets** ; demi-precision **4 → 2 octets par axe** (le plafond de 64 unites de delta par update est inatteignable a vitesse routiere) ; seuil de position, qui supprime entierement les updates des vehicules quasi immobiles (en file, a l'arret) ; et le tick par defaut de 30, NGO repartissant deja les instances NetworkTransform sur les creneaux de tick.

La **relevance par distance** existe et est documentee (`CheckObjectVisibility`, `NetworkShow`/`NetworkHide`, `SpawnWithObservers = false`) [24], mais son interet ici n'est pas de cacher des voitures a des joueurs — avec 4 joueurs dans une meme ville, l'union de ce que quelqu'un voit couvre l'essentiel du trafic. **Son vrai usage est de ne pas faire apparaitre le trafic dont aucun joueur n'est proche** — une decision d'authoring, pas un reglage de NetworkTransform.

### NavMeshAgent et Rigidbody : le motif recommande

Unity affirme que les deux composants tentant de deplacer l'objet produit un « comportement indefini » et n'autorise que deux configurations [21]. **Pour RRS, c'est la seconde qui convient : `updatePosition = false`, `updateRotation = false`, l'agent servant uniquement de fournisseur de chemin et de cible de braquage, le Rigidbody faisant le deplacement.** Elle preserve le modele de conduite physique existant ; la premiere (Rigidbody cinematique, agent conducteur) obligerait a le jeter. Les praticiens decrivent independamment la meme forme : « utiliser un agent navmesh factice capable de faire du pathfinding a travers une ville, et faire suivre ce factice par une voiture pilotee par la physique » [26].

**Contraintes dures, documentees :**

- **Jamais NavMeshAgent et NavMeshObstacle sur le meme vehicule** — « l'agent essaie de s'eviter lui-meme », et avec le carving actif il « tente de se remapper constamment sur le bord du trou creuse » [21].
- **Le carving avec ~30 voitures en mouvement est exclu.** Unity limite le mode *Carve When Moved* aux « obstacles grands et lents » et indique que *Carve Only Stationary* « est generalement le meilleur choix en termes de performance » [22]. L'evitement vehicule-vehicule doit venir de la priorite d'agent, d'une surcharge par raycast ou d'un modele de voies — pas du carving. Le carving ne convient qu'aux vehicules reellement arretes (accident, barrage).
- **NavMesh n'a aucune notion de rayon de braquage** [26]. Toute approche reposant sur le braquage brut de NavMeshAgent pour des voitures herite du coupage de virage et du « trop vite pour tourner ».

**~30 agents n'est nulle part pres d'un plafond de l'approche GameObject, et ce n'est donc pas un argument DOTS.** Le seul recit catastrophe a faible nombre d'agents trouve (15 agents, 70 % de FPS perdus) est **explicitement diagnostique dans le fil meme comme un probleme de rendu et d'animation, pas de NavMesh** [27] — bon rappel qu'un chiffre d'agents en titre doit se lire au-dela du premier message.

---

## D5 — Cout, lock-in et estimation du build

**Cette base de preuves est mince, et la minceur est elle-meme le constat.** Sept sources lues, avec deux blocages structurels : **reddit.com est inaccessible a cet agent** — r/Unity3D et r/gamedev, gisements primaires designes par le cadrage, n'ont pas pu etre fouilles du tout — et le seul banc d'essai comparatif de praticien trouve a renvoye un HTTP 403 [37].

**Un asset de trafic Unity bien note et toujours reference peut etre vieux de 3,5 ans et vendu plein tarif.** iTS – Intelligent Traffic System est en v2.1.1, derniere mise a jour **3 janvier 2023**, Unity minimum 2019.4.6, vendu **€69**, sans aucune mention de depreciation [31]. Plus parlant : un pack a **€321.10** a recu vers mars 2026, contre la version alors courante, l'avis « le code ne fonctionne toujours pas sous Unity 2022 », avec 4 votes utiles [32] — un asset a prix premium, un avis recent, une version d'Unity deja depassee par Unity 6.

**Tout reproche utilisateur nommant une douleur *specifique* designe la couche authoring ou l'integration**, jamais la conduite : absence d'annulation et suppression accidentelle de voies entieres, reglages de layers non documentes, scenes de demo cassees, impossibilite d'atteindre les outils du manager en ajoutant l'asset a un projet existant, integration de ses propres modeles de vehicules [31][32]. **Zero avis ou message lu ne se plaint d'interblocage aux intersections ou de recuperation de vehicule bloque.** C'est un nul frappant.

Cote construction, le meme doigt pointe la meme direction. Un developpeur ayant annonce un systeme de trafic open-source en 2016 l'a abandonne : le systeme « depend encore de certaines parties de mes autres scripts exclusifs », et « separer le systeme de trafic et ecrire la documentation demanderait un nombre d'heures enorme » [33]. **Le cout n'etait pas le modele de conduite, c'etait l'extraction et la documentation** — la surface d'authoring et d'integration, de nouveau. Corollaire pour l'adoption : un asset construit dans la forme du projet de quelqu'un d'autre resistera a la votre.

**Qualificatif honnete, et il compte.** C'est un biais reel de l'*echantillon*, pas necessairement de la realite. Les avis Asset Store sont ecrits en semaine 1 a 4, quand l'UX d'authoring est ce qu'on rencontre en premier ; l'interblocage d'intersection et la recuperation de vehicule bloque sont des problemes de mois 3, qui apparaissent dans un build livre et se rapportent sur Discord ou nulle part. **« L'authoring est le gouffre » est l'hypothese etayee ; « l'interblocage » est non teste, pas refute** [37].

**Le trou le plus grand :** aucun developpeur nomme n'indique combien de temps lui a pris un systeme de trafic urbain Unity construit de zero. Zero, sur sept sources. Le test des deux sources donne : « adopter un asset de trafic coute un vrai travail d'integration » le franchit (avis Urban Traffic System + recit d'extraction, directions differentes, meme conclusion) ; **« construire de zero prend N mois » echoue — il n'y a pas de premiere source, encore moins deux** [37].

**Et sur le chemin « reference » : la preuve n'est pas mince, elle est vide.** Aucun compte-rendu de praticien, aucune comparaison, rien sur le fait de lire un systeme open-source et d'en reimplementer les idees plutot que d'en dependre [37]. Le cadrage predisait que cette option serait sous-documentee ; c'est confirme. **Quiconque choisit ce chemin pour ce projet le choisit sur raisonnement, pas sur precedent.** Ce rapport recommande ce chemin en le sachant.

**Lock-in, signal structurel :** Gley vend *Mobile Traffic System v2* et *v3* comme **deux fiches distinctes** ; la politique de mise a niveau v2→v3 n'a pas pu etre confirmee [37]. C'est exactement la forme que prend un fork majeur payant. A verifier avant tout achat.

---

## Matrice de decision

Notes de 1 a 5. Une matrice que vous pouvez repondérer vaut mieux qu'un verdict a croire : les poids sont ceux du frame, changez-les et recalculez.

| Critere | Poids | **A. Build interne (blocs natifs + port IDM/MOBIL)** | **B. Adapter Gley MTS v3** | **C. Adopter un OSS (SimpleTraffic / mchrbn)** |
|---|---|---|---|---|
| Extensibilite decision/navigation | **35 %** | **5** — vous possedez chaque parametre ; rage = multiplicateur sur votre propre struct | **2** — accroche a l'etage commande seulement ; l'etage parametres est absent ; l'atteindre = forker les 4 classes cles, API `static` globale hostile au host-authoritative [7] | **3** — source ouverte et petite, mais forme FSM monolithique ; **aucun des deux n'implemente le moindre changement de voie** [28] |
| Trafic source/sink route | 15 % | **4** — le routage par ratios de virage est peu couteux a ecrire [14] | **3** — A* supporte mais **desactive par defaut pour raisons CPU** ; despawn pilote par la distance au joueur, **visibilite non documentee pour la suppression** (elle l'est pour le spawn) [7] | **1** — mchrbn tire le segment suivant au hasard ; SimpleTraffic ne fait qu'un saut ; aucun puits [28] |
| Baseline urbaine credible | 15 % | **3** — intersections et poursuite a ecrire, mais IDM/MOBIL sont specifies et portables | **4** — feux, ronds-points, priorites, depassement livres ; mais freinage tout-ou-rien rapporte [9] | **2** — arret booleen par raycast, aucune notion de headway, seuils independants de la vitesse [28] |
| Tenue a ~30 vehicules | 10 % | **5** — largement dans la zone confortable ; LOD sous votre controle [25] | **4** — Burst/jobs, concu mobile ; mais le mode pathfinding est le mode couteux [7] | **3** — SimpleTraffic raycaste chaque Update par vehicule sans gating ni masque : « ne passe pas a l'echelle » [28] |
| Cout d'adoption et de sortie | 15 % | **2** — cout de construction le plus eleve ; mais cout de sortie nul, c'est votre code | **3** — €118.69 est bon marche, l'authoring est le vrai achat ; sortie couteuse, et G1 exige une approbation documentee [8] | **3** — gratuit, mais ere Unity 2017/2018, NavMeshComponents vendues en conflit sous Unity 6 : le portage est deja du build [28] |
| Sante de l'ecosysteme | 10 % | **5** — packages Unity natifs stampes 6000.6, risque d'abandon nul [1][2] | **4** — reellement maintenu (v3.6.4, 2026-09-04) ; mais precedent de fork majeur payant, zero histoire reseau [8] | **1** — derniers commits 2019 et 2022, maintenance nulle [3] |
| **Total pondere** | | **4.10** | **3.00** | **2.35** |

**Le choix : A.** **Runner-up : B**, et la condition sous laquelle il gagne est nommee dans le resume executif.

**La couverture de reversibilite la moins chere :** faire passer toute conduite IA par une interface etroite — une struct de parametres de conducteur et une fonction `effectif = base × f(rage, peur)` — des le premier jour. Si le build interne s'enlise sur l'authoring, cette couture permet de glisser Gley *sous* elle en reimplementant ses quatre classes cles contre la meme struct, sans toucher a la couche rage/peur. C'est aussi ce qui rend la decision revisitable au lieu d'etre definitive.

---

## Insights inter-dimensions

Ce que seule la combinaison montre.

**1. La seule option maintenue echoue precisement sur le seul critere qui compte le plus.** D1 etablit que Gley est le seul candidat vivant du champ — tout le reste est mort ou natif. D2 etablit que sa couture est a l'etage commande alors que le projet a besoin de l'etage parametres. Prises separement, ces deux dimensions suggerent des conclusions opposees ; ensemble, elles disent que **le marche n'offre pas ce que ce projet demande**, et que l'absence de choix est le choix.

**2. L'argument « acheter pour gagner du temps » est vide de son contenu principal par D1.** D5 nomme l'authoring et l'integration comme gouffre — donc acheter devrait payer. Mais D1 etablit qu'**aucun candidat ne documente Netcode for GameObjects** : l'integration host-authoritative, qui est une grosse part de l'integration pour ce projet precis, est a ecrire dans tous les cas. L'asset achete la moitie authoring du gouffre, pas la moitie integration. Et D2 ajoute que son API `static` globale rend cette moitie *plus* difficile, pas moins.

**3. La contrainte « jamais de disparition en pleine route » est plus structurante qu'elle n'en a l'air.** D3 montre que la soupape de secours de SUMO est la teleportation a 300 s, et que ses paliers d'infraction sont tous desactives par defaut. D1/D2 montrent que le despawn de Gley est purement lie a la distance au joueur, sans condition de visibilite documentee. **Tous les systemes etudies comptent sur une forme de disparition pour se sortir des situations impossibles.** Interdire cette sortie n'est pas un detail de reglage : cela oblige a construire l'echelle d'infraction que personne n'active par defaut. C'est du travail que ni l'adoption ni la reference ne fournissent.

**4. Le budget technique est entierement du cote de la credibilite, pas de la performance.** D4 etablit que ~30 vehicules est un ordre de grandeur sous toute limite, que la bande passante n'est pas liante, et que DOTS n'est pas un argument. D2 etablit que la couche de decision voulue coute environ neuf flottants et une fonction. D3 etablit que MOBIL coute six evaluations IDM par voie candidate, **rendu bon marche uniquement par son gating a 1 s avec minuteurs desynchronises** [28]. **Rien dans ce dossier n'est bloque par la performance.** Tout le risque est dans le fait de rendre le comportement credible — c'est-a-dire dans le reglage et l'authoring, exactement la ou D5 pointe.

---

## Recommandations

Chacune indique la base de confiance sur laquelle elle repose.

**R1 — Construire en interne, avec `com.unity.ai.navigation` 2.0.14 en fournisseur de chemin uniquement.** `updatePosition = false`, `updateRotation = false`, le Rigidbody existant continue de conduire. *Confiance : haute — documentation Unity primaire sur les deux configurations permises [21], corroboree independamment par des praticiens vehicules [26].* Alimente : contrainte d'architecture pour le spine.

**R2 — Faire de la personnalite du conducteur une struct de parametres explicite des le premier jour, et faire de la rage/peur un multiplicateur dessus.** `effectif = base × f(rage, peur)`, sur le modele de `lcImpatience` de SUMO. Remplacer `ResolveCruiseSpeedMultiplier` — qui pilote `v0`, l'axe le plus faible — par T, s0, a, b, v0, p, a_th, b_safe. *Confiance : haute sur la structure (sources primaires IDM/MOBIL/SUMO [10][11][13]) ; **les valeurs cibles sont du jugement, pas de la mesure** — voir R3.* Alimente : le contrat de la couche rage/peur, decision d'architecture.

**R3 — Ne pas copier la table de calibration de highway-env telle quelle.** Ses classes `AggressiveVehicle` / `DefensiveVehicle` sous-classent `LinearVehicle`, pas IDM ; **elles n'exercent jamais la politesse, et toutes deux *relevent* le temps inter-vehiculaire a 2.5 s au-dessus des 1.5 s de l'IDM de base** — l'« agressif » ne colle donc pas au pare-chocs. Elles ont ete reglees pour un scenario d'insertion autoroutiere. Le seul discriminant reel est **`k_gap` : 0.5 agressif contre 2.0 defensif, un facteur 4 dans la reaction a un creneau insuffisant** [28]. *Confiance : haute — le fichier a ete lu ligne a ligne. C'est un renversement d'une attente du round 1, enregistre comme tel.* Utiliser le code comme portage de formule, les valeurs comme point de depart a regler en jeu.

**R4 — Router par ratios de virage aux jonctions, pas par A*.** Arcs sources, tirage stochastique a chaque jonction, arcs puits, avec un equivalent de `--sources-are-sinks` pour « ressort par son tunnel » et un garde-fou facteur-d'arcs contre l'errance. *Confiance : haute — `jtrrouter` est exactement ce modele, documente en source primaire [14].* Cela supprime le besoin d'un graphe de voies pathfinde et reduit d'autant le cout d'authoring que D5 designe comme gouffre.

**R5 — Implementer « garder la jonction libre » avant toute autre regle d'intersection.** Ne jamais s'engager si la voie de sortie n'a pas de place. Une verification d'occupation qui supprime la moitie « detention » de l'interblocage, active par defaut dans SUMO [16]. Priorites decidees a l'authoring, arbitrage a l'execution nul. Ne pas partir sur de la reservation : trop lourd a cette echelle [19]. *Confiance : haute.*

**R6 — Remplacer le minuteur de 20 s par une replanification continue plus une escalade visible.** Replanifier des que le chemin est bloque ; escalader le comportement en continu ; **aucun palier de suppression pres du joueur**. Inverser l'ordre de SUMO : activer les paliers d'infraction (entrer dans une jonction normalement gardee libre, contourner un bloqueur) que SUMO desactive, puisque la triche finale qu'ils evitent est justement interdite ici. *Confiance : haute sur la forme (defauts documentes, confirmes sur deux pages [17][18]) ; **le nombre de game-feel n'existe pas publiquement et doit etre regle par playtest, pas par citation** [37].*

**R7 — Retirer les trottoirs du masque d'aire de l'agent voiture, ne pas se contenter de les rendre couteux.** Unity documente lui-meme que les couts d'aire sont des indications difficiles a regler qui ne biaisent que le chemin global, l'evitement local etant un systeme separe [20]. *Confiance : haute.*

**R8 — Lire SimpleTraffic et mchrbn, ne pas les importer.** Les deux sont MIT valides — **le risque de licence signale au round 1 sur mchrbn est largement refute** : le texte MIT complet avec titulaire nomme figure aux lignes 60-68 du README, et MIT n'exige pas un fichier nomme LICENSE ; le detecteur de GitHub ne scanne simplement pas les README [28]. Mitigation ceinture-et-bretelles si souhaitee : conserver une copie de ces lignes du README a cote de tout fichier derive. A copier : l'enum `Status {GO, STOP, SLOW_DOWN}` comme couture entre couche jonction et couche conduite ; l'arbitrage de stop en ~30 lignes (deux listes plus une liste de segments prioritaires) ; l'anticipation de virage via le waypoint *futur* ; le registre de vehicules par section comme index spatial bon marche evitant un balayage O(n²). A ne pas importer : `Assets/NavMeshComponents/` de SimpleTraffic entrera en conflit avec le package sous Unity 6, et `GetDetectedObstacles` de mchrbn a un bug reel (il retourne la distance du *dernier* rayon, pas de l'obstacle retenu) [28]. *Confiance : haute — les fichiers ont ete lus.*

**R9 — Ne pas activer le carving NavMesh sur des vehicules en mouvement, et ne jamais mettre NavMeshAgent et NavMeshObstacle sur le meme vehicule.** L'evitement vehicule-vehicule vient de la priorite d'agent, d'une surcharge raycast ou du modele de voies. Le carving n'est legitime que pour un vehicule reellement arrete, en *Carve Only Stationary*. *Confiance : haute — deux pages de documentation Unity explicites [21][22].*

**R10 — Avant de figer un budget reseau, mesurer plutot qu'estimer.** Une heure avec le profileur reseau du package Multiplayer Tools remplacerait chaque estimation d'octets de ce rapport par une mesure [23][25]. C'est l'action a plus forte valeur restante, et elle est locale, pas documentaire. *Confiance : haute sur la recommandation ; les chiffres de D4 sont explicitement de l'arithmetique et des rapports communautaires, pas des mesures.*

---

## Questions ouvertes

| Question | Ce qu'il faudrait pour y repondre |
|---|---|
| **Combien de temps prend reellement un trafic urbain construit de zero ?** Aucune source publique ne le dit. | Aucune citation ne reglera ceci. Le seul substitut honnete est un spike time-boxe sur la partie la plus incertaine — l'authoring du reseau de voies — et une extrapolation depuis ce qu'il aura coute. |
| Le minuteur de game-feel : combien de temps une voiture IA immobile parait-elle cassee a un joueur ? | N'existe pas dans la litterature publique. A regler par playtest. |
| NGO 2.13.2 se comporte-t-il comme 2.5.1 le documente ? Les pages de manuel lues resolvent en 2.5.1 ; huit versions mineures de derive non fermees. | Lire le changelog NGO entre 2.5 et 2.13 pour NetworkTransform et la visibilite d'objet, ou lire `Components/NetworkTransform.cs` sur le depot. |
| Les classes de decision de Gley sont-elles du C# manage ou jobifiees Burst ? Infere manage depuis la forme classe abstraite + `Execute()` par update, **non verifie**. | Dix minutes avec le package en main. Ne compte que si l'option B redevient d'actualite. |
| La suppression de Gley verifie-t-elle la visibilite ? Documente pour le spawn, silencieux pour le despawn. | Idem — package en main, ou question au vendeur. |
| **Le delegue `Modify Trigger Size` de Gley peut-il etre clef sur l'identite du vehicule, et pas seulement sur sa vitesse ?** C'est la seule exception potentielle a l'absence de levier de distance de suivi — donc le seul endroit ou l'option B pourrait recuperer une partie du critere numero un. | Les signatures de delegues ne sont pas publiees. Package en main, ou question au vendeur. A trancher **avant** toute reevaluation de l'option B. |
| Politique de mise a niveau v2→v3 de Gley. | Un e-mail au vendeur. A resoudre **avant** tout achat : c'est la forme exacte que prend un lock-in de version majeure. |
| Le judder client entre une conduite hote en FixedUpdate et l'interpolation NetworkTransform a 30 Hz. | Rien trouve sur ce point, et c'est un risque reel pour l'architecture exacte du projet. A observer en test a deux clients. |
| L'interblocage aux intersections est-il vraiment un non-probleme, ou juste absent des avis de semaine 1 ? | Non teste, pas refute. Se revelera au mois 3 ; la reponse R5 est la couverture a priori. |
| r/Unity3D et r/gamedev n'ont pas pu etre fouilles (domaine bloque pour cet agent). | Une recherche manuelle par l'utilisateur, ou un harnais capable d'atteindre reddit. Gisement a plus forte valeur non explore. |

---

## Annexe des sources

| [n] | Ce qu'elle etaye | Editeur | Publication | Consultee | Confiance |
|---|---|---|---|---|---|
| [1] | AI Navigation 2.0.14 stampe pour 6000.6 ; aucune guidance vehicule documentee | [Unity Technologies](https://docs.unity3d.com/6000.6/Documentation/Manual/com.unity.ai.navigation.html) | doc vivante | 2026-09-15 | haute (version) / moyenne (absence) |
| [2] | Splines 2.8.4 pour 6000.6 ; Burst et Physics devenus optionnels | [Unity Technologies](https://docs.unity3d.com/Packages/com.unity.splines@2.8/changelog/CHANGELOG.html) | doc vivante | 2026-09-15 | haute |
| [3] | Metriques `pushed_at` des depots de trafic OSS ; peque-traffic archive | [GitHub REST API](https://github.com) | lecture live | 2026-09-15 | haute |
| [6] | Megacity Metro : DOTS + Netcode for Entities, server-authoritative | [Unity Technologies](https://github.com/Unity-Technologies/megacity-metro) | maintenu | 2026-09-15 | moyenne-haute |
| [7] | API Gley : `SetVehicleBehaviours`, `VehicleBehaviour.Execute()`, `BehaviourResult` ; absence de parametres ; A* desactive par defaut ; `DistanceToRemove` ; source complete livree ; API `static` | [Gley (docs editeur)](https://gley.gitbook.io/mobile-traffic-system-v3) | produit v3.6.4 | 2026-09-15 | haute (API) / moyenne (inference Burst) |
| [8] | Prix €118.69, v3.6.4 du 2026-09-04, EULA standard, Built-in/URP/HDRP | [Unity Asset Store](https://assetstore.unity.com/packages/tools/behavior-ai/mobile-traffic-system-v3-305800) | page live | 2026-09-15 | haute |
| [9] | Rapports utilisateurs pre-v3 : freinage brutal, spawn qui « casse l'illusion » | [Unity Discussions](https://discussions.unity.com/t/mobile-traffic-system-city-traffic-for-games/843638?page=7) | 2024-03 a 2025-04 | 2026-09-15 | moyenne (ere v2) |
| [10] | Formule IDM et ses six parametres ; le parametrage decrit le style de conduite | [Wikipedia, citant Treiber, Hennecke & Helbing, *Phys. Rev. E* 62(2):1805](https://en.wikipedia.org/wiki/Intelligent_driver_model) | 2000-08 | 2026-09-15 | haute |
| [11] | Bandes de politesse MOBIL dont `p < 0` « personnalite malveillante » ; `b_safe`, `a_th` | [traffic-simulation.de (Martin Treiber)](https://traffic-simulation.de/info/info_MOBIL.html) | modele 2007 | 2026-09-15 | haute |
| [13] | `tau`, `sigma`, `minGap`, `impatience`, `lcImpatience`, `lcAssertive`, `jmTimegapMinor`, rampe `waitingTime/180` | [Eclipse SUMO (DLR)](https://sumo.dlr.de/docs/Definition_of_Vehicles,_Vehicle_Types,_and_Routes.html) | doc vivante | 2026-09-15 | haute |
| [14] | `jtrrouter`, `--turn-defaults 30,50,20`, `--sink-edges`, `--sources-are-sinks`, `--max-edges-factor` | [Eclipse SUMO](https://sumo.dlr.de/docs/jtrrouter.html) | doc vivante | 2026-09-15 | haute |
| [15] | File d'insertion, `departDelay`, strategies `departLane` | [Eclipse SUMO](https://sumo.dlr.de/docs/Simulation/VehicleInsertion.html) | doc vivante | 2026-09-15 | haute |
| [16] | Heuristique keep-clear par defaut ; priorite geometrique ; `--ignore-junction-blocker` | [Eclipse SUMO](https://sumo.dlr.de/docs/Simulation/Intersections.html) | doc vivante | 2026-09-15 | haute |
| [17] | `--time-to-teleport` 300 s, detection a < 0.1 m/s, tampon de teleportation | [Eclipse SUMO](https://sumo.dlr.de/docs/Simulation/Why_Vehicles_are_teleporting.html) | doc vivante | 2026-09-15 | haute (deux pages concordantes) |
| [18] | `--max-depart-delay -1`, `--waiting-time-memory 100`, defauts d'options | [Eclipse SUMO](https://sumo.dlr.de/docs/sumo.html) | doc vivante | 2026-09-15 | haute |
| [19] | Gestion d'intersection par reservation ; subsume feux et stops | [Dresner & Stone, *JAIR* 31](https://www.jair.org/index.php/jair/article/view/10542) | 2008 | 2026-09-15 | haute (pour ce qu'elle affirme) |
| [20] | Couts d'aire = indications difficiles a regler ; masque d'aire par agent | [Unity Technologies](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/AreasAndCosts.html) | AI Nav 2.0 | 2026-09-15 | haute |
| [21] | Agent + Rigidbody = comportement indefini ; `updatePosition/updateRotation = false` ; Agent + Obstacle ne se melangent pas | [Unity Technologies](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/MixingComponents.html) | AI Nav 2.0 | 2026-09-15 | haute |
| [22] | *Carve Only Stationary* meilleur en performance ; *Carve When Moved* pour obstacles grands et lents | [Unity Technologies](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/AboutObstacles.html) | AI Nav 2.0.14 | 2026-09-15 | haute |
| [23] | Tick 30 et repartition en creneaux ; demi-precision 4→2 o ; quaternion 16→4 o ; seuils et axes | [Unity Technologies](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.5/manual/components/helper/networktransform.html) | manuel NGO 2.5.1 | 2026-09-15 | haute **a 2.5.1 ; non verifie a 2.13.2** |
| [24] | `CheckObjectVisibility`, `NetworkShow/Hide`, `SpawnWithObservers = false` | [Unity Technologies](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.5/manual/basics/object-visibility.html) | manuel NGO 2.5.1 | 2026-09-15 | haute **a 2.5.1** |
| [25] | Seuil « centaines a 1000+ » (staff Unity) ; points de degradation communautaires ; enveloppe ~1 Mo/s/client | [Unity Discussions, NoelStephens_Unity](https://discussions.unity.com/t/how-many-network-objects-in-a-scene-is-too-many/870805) | 2023-11 / 2025-06 | 2026-09-15 | moyenne-haute (staff) / faible (octets communautaires) |
| [26] | NavMesh sans rayon de braquage ; motif « agent factice suivi d'une voiture physique » | [Unity Discussions](https://discussions.unity.com/t/navmesh-with-vehicles-a-navmesh-agent-with-a-turning-radius/704650) | 2018–2019 | 2026-09-15 | moyenne (**hors barre de fraicheur 2 ans**) |
| [27] | Le recit « 15 agents = 70 % de FPS » diagnostique en fil comme un probleme de rendu | [Unity Discussions](https://discussions.unity.com/t/navmesh-agent-make-big-performance-impact/248175) | 2021-09 | 2026-09-15 | moyenne |
| [28] | Constantes highway-env lues ligne a ligne ; formules IDM et MOBIL implementees ; motifs SimpleTraffic et mchrbn ; statut MIT des trois depots | [Farama HighwayEnv `behavior.py`](https://raw.githubusercontent.com/Farama-Foundation/HighwayEnv/master/highway_env/vehicle/behavior.py) · [Kink3d/SimpleTraffic](https://github.com/Kink3d/SimpleTraffic) · [mchrbn](https://github.com/mchrbn/unity-traffic-simulation) | commit 5240596, 2026-07-03 | 2026-09-15 | haute (sources lues) |
| [31] | iTS : v2.1.1, 2023-01-03, Unity 2019.4.6 min, €69, aucune depreciation ; avis sur l'authoring | [Unity Asset Store](https://assetstore.unity.com/packages/templates/systems/its-intelligent-traffic-system-23564) | maj 2023-01 | 2026-09-15 | haute (faits) / moyenne (avis vieux de 6-7 ans) |
| [32] | €321.10 ; « le code ne fonctionne toujours pas sous Unity 2022 » (≈2026-03, 4 votes) ; frictions d'integration | [Unity Asset Store](https://assetstore.unity.com/packages/templates/systems/urban-traffic-system-full-pack-166688/reviews) | ≈2026-03 | 2026-09-15 | haute |
| [33] | Abandon d'un trafic OSS : « separer le systeme et ecrire la documentation demanderait un nombre d'heures enorme » | [Unity Discussions](https://discussions.unity.com/t/open-source-ai-traffic-system-that-really-works-out-of-the-box/618378) | 2016–2018 | 2026-09-15 | moyenne (**hors barre de fraicheur**) |
| [35] | Despawn de Cities: Skylines sur blocage ; aucun minuteur publie | [Steam / Paradox (fils communautaires)](https://forum.paradoxplaza.com/forum/threads/when-vehicles-gridlock-an-intersection-all-affected-vehicles-despawn-instead-of-just-the-ones-causing-the-gridlock.1604674/) | non dates | 2026-09-15 | faible (observation joueur) |
| [36] | Utility AI / IAUS : considerations, courbes de reponse, facteur de compensation | [Dave Mark, Intrinsic Algorithm](https://www.gameai.com/iaus.php) | non date | 2026-09-15 | **faible — resumes de recherche, une page en HTTP 403 ; non affirme comme constat** |
| [37] | Constats d'absence : aucune duree de build de zero ; chemin « reference » sans precedent public ; reddit inaccessible ; politique v2→v3 non confirmee | Synthese des digests D5 et R2 de ce run | 2026-09-15 | 2026-09-15 | haute (pour « non trouve dans ce budget ») |

---

## Carte de peremption

Fenetres issues du pack technique (versions et compatibilite ≤ 1 mois · signaux d'ecosysteme ≤ 6 mois · paysage ≤ 12 mois · motifs ≤ 24 mois) et de la forme select (tarifs ≤ 3 mois).

| Classe de claim | Sources | Fenetre | A revoir avant |
|---|---|---|---|
| **Version et compatibilite** (AI Nav 2.0.14, Splines 2.8.4, NGO 2.13.2 vs 2.5.1) | [1][2][23][24] | 1 mois | **2026-10-15** |
| **Tarifs et licence** (Gley €118.69, iTS €69, UTS €321.10) | [8][31][32] | 3 mois | **2026-12-15** |
| **Signaux d'ecosysteme** (commits OSS, maintien de Gley, avis) | [3][9][31][32] | 6 mois | 2027-03-15 |
| **Paysage** (etat du champ des candidats) | [3][6] | 12 mois | 2027-09-15 |
| **Motifs** (IDM, MOBIL, jtrrouter, keep-clear, motif Agent/Rigidbody) | [10][11][13][14][16][21] | 24 mois — *science etablie, en pratique stable* | 2028-09-15 |
| **Hors fenetre des maintenant** | [26] (2018–2019), [33] (2016–2018) | motifs ≤ 24 mois | **deja perime — signale en ligne dans l'annexe** |

**Echeance la plus proche : 2026-10-15**, sur les claims de version — et le plus utile a fermer d'ici la est la derive NGO 2.5.1 → 2.13.2, seul endroit ou une lecture de documentation a ete faite sur une version differente de celle du projet.

**Peremption globale de la selection :** un rapport de selection de plus de deux trimestres devrait etre rafraichi avant qu'on agisse dessus. Pour celui-ci : **a rafraichir avant 2027-03-15** si la decision n'a pas ete engagee d'ici la.
