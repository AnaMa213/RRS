# Sprint Change Proposal — 2026-09-15

**Sujet :** Remaniement du perimetre de l'Epic 5 (trafic IA, regles de conduite, rage/peur, dechets, reglages de session) et ajustements cibles sur l'Epic 6.

**Statut :** **approuve et applique** par Kenan le 2026-09-15 (revision 3). Artefacts mis a jour : `epics.md`, `ARCHITECTURE-SPINE.md`, `sprint-status.yaml`, `docs/setup/addon-adoption-register.md`.
**Classification de portee :** Moderate — reorganisation de backlog, pas de replan fondamental.
**Cadrage retenu (decision Kenan, 2026-09-15) :** sandbox de fondation etendu. Le quartier urbain livre ici est un terrain de test greybox, **pas** le Level 1 de MVP 2.

**Revision 2 — changements demandes par Kenan :**
1. Les effectifs (vehicules en ville, vehicules lanceurs de dechets) doivent etre **parametrables**, jamais codes en dur.
2. Ajout au perimetre : un **menu d'echappement** avec retour au menu principal, et le reglage de ces effectifs **depuis le lobby**.
3. **Renumerotation** : le checkpoint de l'epic passe en derniere position ; les nouvelles stories sont reincrementees.

**Revision 3 — changement demande par Kenan (meme jour, apres coup) :** la sequence 5.8-5.17 doit **s'incrementer naturellement** le long du graphe de dependances plutot que de suivre l'ordre de redaction. Story 5.16 (menu d'echappement) n'a aucune dependance sur les travaux de trafic ; elle passe en 5.8 pour une livraison precoce et independante — elle rend aussi confortable le test de tout le reste de l'epic, puisqu'une run de trafic longue peut alors etre quittee proprement. Chaque autre story se decale d'un cran. Voir la table de correspondance en tete de la section 4.4 et le diagramme mis a jour en section 5. Toutes les cles etaient en `backlog` : renommage sans perte de statut.

---

## 1. Resume du probleme

### Declencheur

Story **5.8 — Epic 5 AI Traffic Playable Checkpoint** (statut `backlog`), story de checkpoint de l'Epic 5. Le checkpoint tel qu'ecrit valide « trois vehicules IA sur la route, des changements de rage, un evenement Rage Road ». A la revue du perimetre, ce checkpoint a ete juge **non representatif du systeme de trafic et d'IA dont le jeu a besoin**.

### Nature du probleme

**Exigence nouvelle emergeant de la partie prenante**, doublee d'une **limite technique decouverte a l'implementation**.

Les stories 5.1 a 5.7 sont implementees (toutes en `review`) et livrent un trafic fonctionnel, mais dont les fondations ne portent pas l'ambition de gameplay :

| Ce qui existe | Ce qu'il faut |
| --- | --- |
| Liste unique de waypoints en boucle (`RouteWaypoints`) | Graphe de voies route, entrees/sorties type tunnel, parcours varies |
| Aucune notion de voie, d'intersection, de regle | Double sens, intersections, stops, priorite, anti-interblocage |
| Aucune detection d'obstacle | Perception de plusieurs vehicules et joueurs autour de soi |
| Deblocage = teleportation au waypoint courant | Replanification continue, escalade visible, **aucune disparition** |
| Rage = multiplicateur de vitesse de croisiere (`ResolveCruiseSpeedMultiplier`) | Rage/peur modulant les decisions de conduite, de risque et de cible |
| 3 vehicules, effectif fige | Effectif **configurable depuis le lobby**, ~30 en cible de validation |
| Aucun moyen de quitter une run sans tuer le processus | Menu d'echappement et retour au menu principal |

### Preuves

1. **Recherche technique dediee**, menee le 2026-09-15 : `planning-artifacts/research/technical-trafic-ia-vehicules-unity-6-2026-09-15/research.md` (7 digests sources, registre de claims dans `.memlog.md`, verification semantique des citations effectuee).
2. **Constat de code** : `Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs` — la rage pilote exclusivement `cruiseSpeed`. La recherche etablit que la vitesse desiree (`v0`) est **l'axe de personnalite le moins expressif** des six que le modele IDM expose.
3. **Constat d'ecart planning** : le tableau « Corrective roadmap (current) » de `epics.md` ne liste que la story 5.1 avant de renvoyer le reste en « MVP 2 assembly — deferred ». Or 5.2 a 5.7 ont ete implementees. **Le tableau est perime ; l'implementation a deja pris cette direction.** La presente correction formalise une derive constatee, elle ne l'introduit pas.
4. **Incoherence de numerotation** : la story de checkpoint de l'Epic 5 portait le numero 5.8 tout en devant s'executer apres les stories 5.9 et suivantes. Un checkpoint d'epic au milieu de son propre epic n'a pas de sens de lecture.

---

## 2. Analyse d'impact

### 2.1 Impact Epic (checklist section 2)

| Item | Constat | Statut |
| --- | --- | --- |
| 2.1 Epic 5 peut-il etre termine tel que planifie ? | **Non.** Le checkpoint validerait un systeme que le projet ne veut pas garder. | `[!]` Action-needed |
| 2.2 Changements de niveau epic | **Modification du perimetre** de l'Epic 5 : 10 stories ajoutees, checkpoint reecrit et renumerote. Aucun epic cree, supprime ou redefini. | `[x]` Done |
| 2.3 Revue des epics restants | Epic 6 impacte sur 2 points (6.3, et une story nouvelle). Epic 7 : aucun impact structurel. | `[x]` Done |
| 2.4 Epics invalides ou nouveaux epics requis ? | **Aucun.** Le cadrage retenu garde tout dans la structure existante. | `[x]` Done |
| 2.5 Reordonnancement ? | **Oui, interne a l'Epic 5** : le checkpoint passe de 5.8 a **5.18**, en derniere position. Les nouvelles stories occupent 5.8 a 5.17. | `[x]` Done |

### 2.2 Impact Story

**Stories existantes superseded en substance (implementees, en `review`, non annulees) :**

| Story | Ce qui est superseded | Traitement |
| --- | --- | --- |
| 5.2 Basic AI Route Following and Recovery | Le suivi de waypoints en boucle et la recuperation par teleportation. | Conserve comme jalon historique. Les stories 5.9 et 5.11 remplacent son mecanisme. La teleportation de recuperation est **retiree du chemin nominal**. |
| 5.4 Rage-Driven AI Behavior States | Les multiplicateurs de vitesse par palier. | Conserve comme jalon historique. Story 5.13 re-exprime ces etats via la struct de parametres. `ResolveCruiseSpeedMultiplier` est supprime. |

Les stories 5.1, 5.3, 5.5, 5.6 et 5.7 restent valides et sont **reutilisees** : 5.1 fournit les jauges Rage/Peur, 5.5 fournit le ciblage reseau dont 5.13 depend directement, 5.6 fournit le cycle de vie de l'evenement Rage Road, 5.7 fournit la presentation client.

**Renumerotation du checkpoint.** La story de checkpoint est deplacee de `5.8` a `5.18`. Sa cle sprint-status `5-8-epic-5-ai-traffic-playable-checkpoint` est remplacee par `5-18-epic-5-ai-traffic-playable-checkpoint`. **Cette renumerotation est sans risque : la story est en `backlog`, aucun travail n'y est attache.** Une renumerotation d'une cle en `review` ou `done` ne serait pas acceptable et n'est pas proposee ici — les stories 5.1 a 5.7 conservent leurs numeros et leurs cles.

**Stories ajoutees :** 10 (5.8 a 5.17). Detail en section 4.

### 2.3 Conflits d'artefacts (checklist section 3)

| Artefact | Conflit | Statut |
| --- | --- | --- |
| **FR6** (« three AI vehicles on the route ») | Contredit par un effectif configurable valide vers ~30. | `[!]` A modifier |
| **AD-16 — MVP Slice Cardinality [ADOPTED]** | Fige « three spawned enemy vehicles ». La note de supersession assouplit AD-16 sur la cardinalite, mais de facon ambigue. | `[!]` Successeur requis (AD-32) |
| **AD-30 — MVP 1 Foundations Before MVP 2 Assembly [ADOPTED]** | Lie explicitement « AI traffic ». Le quartier urbain doit etre qualifie de « small greybox integration » et non d'assemblage de niveau, sinon la lecture est une violation. | `[!]` Note de clarification requise |
| **AD-21 — Host-Simulated Vehicle Movement** | Parle de « AI spline progress ». Le modele retenu est un graphe de voies. | `[!]` Reformulation mineure |
| **AD-17 — Canonical Runtime State Shape** | Ne prevoit aucun proprietaire d'etat pour les dechets. | `[!]` Ajout requis |
| **AD-31 — Individual Economy and Configurable NPC Response [ADOPTED]** | **Aucun conflit — au contraire.** Exige deja que « vehicle movement and archetype behavior consume the resulting response through narrow feature boundaries ». C'est exactement l'architecture retenue. | `[x]` Conforme |
| **AD-26 — Session Lifecycle And Run Composition** | Impose qu'il n'existe pas de chemin de chargement contournant le lobby et l'hote. Le retour au menu doit reutiliser le chemin de sortie existant, pas en creer un second. | `[x]` Conforme, contrainte inscrite en Story 5.8 |
| **AD-12 / AD-25** — donnees statiques authorees, catalogue de definitions | Les defauts et bornes d'effectif sont des donnees authorees ; la valeur de session ne l'est pas. | `[x]` Conforme, distinction inscrite en Story 5.16 |
| **AD-20 — Vehicle And Rage Attachment Contract** | Exige `NetworkObjectReference` pour les cibles. S'applique tel quel a l'attribution des dechets. | `[x]` Conforme |
| **AD-13 / AD-14 / AD-27** — intake Blender, greybox d'abord, stabilite prefab | S'appliquent a l'asset dechet et au quartier greybox. | `[x]` Conforme, a respecter |
| **`MatchSettings.cs`** | **Aucun conflit.** Le fichier porte deja l'emplacement reserve : « Emplacement reserve aux futurs parametres de partie extensibles ». Il impose que les valeurs de session runtime ne vivent jamais dans un asset ScriptableObject. | `[x]` Conforme, a etendre |
| **`course-correction-2026-09-13.md`** | Classe « litter recovery » et « source-vehicle identification » en MVP 2, **mais autorise explicitement** : « Litter, NPC vehicle identity… should be introduced as small foundations only when their dedicated story requires them ». | `[x]` Compatible — voir 2.5 |
| **`epics.md` — tableau « Corrective roadmap (current) »** | Perime : ne liste que 5.1 alors que 5.2–5.7 sont implementees. | `[!]` A reconcilier |
| **UX Design** (`ux-designs/ux-RoadRage_Simulator-2026-09-13`) | Le klaxon, les insultes, les impacts de dechet, le menu d'echappement et les reglages de lobby demandent un retour joueur. | `[!]` Note d'impact, pas de refonte |
| **PRD** | Aucun fichier PRD distinct dans ce projet ; les FR vivent dans `epics.md`. | `[x]` N/A |

### 2.4 Impact technique

- **Aucun package ajoute.** `com.unity.ai.navigation` 2.0.14 est deja installe et inutilise ; il devient fournisseur de chemin uniquement.
- **Aucun asset tiers adopte.** Le registre d'adoption recoit une ligne de **rejet documente** pour Gley Mobile Traffic System (section 4.5).
- **Aucun nouvel objet de reglages.** Les effectifs etendent `MatchSettings` et empruntent le chemin de synchronisation deja livre par la Story 2.4.
- **Aucun nouveau chemin de teardown.** Le retour au menu reutilise la sortie de session de la Story 2.7.
- **Contrainte dure heritee de la recherche** : jamais `NavMeshAgent` et `NavMeshObstacle` sur le meme vehicule ; pas de carving sur vehicules en mouvement ; `updatePosition`/`updateRotation` a `false`, le Rigidbody existant conduit. Ces trois points sont documentes par Unity comme des modes d'echec, pas des preferences.
- **Risque de performance nouveau et localise** : les dechets sont des objets reseau **avec collider et gravite**, le seul profil de ce projet proche du seuil de degradation rapporte (~200 objets). Plafonne par conception et par configuration. Story 5.17 mesure.
- **Aucun impact** sur CI/CD, deploiement, infrastructure ou observabilite (projet local Windows, AD-28).

### 2.5 Point de cadrage MVP — resolution

La correction du 2026-09-13 et AD-30 ne sont **pas** contredits par cette proposition, sous trois conditions inscrites dans les changements :

1. Le quartier livre est un **terrain de test greybox** (deux routes a double sens, deux intersections, deux portails tunnel). Il ne contient ni commerces, ni boss, ni equipement, ni acces vehicule, ni recuperation de dechets — c'est-a-dire aucun des elements que la correction nomme comme Level 1.
2. Les dechets sont livres en **fondation** (jet, attribution, physique, reseau). La **boucle de recuperation** — ramasser, jeter a la poubelle, renvoyer — reste en Epic 6, exactement comme la correction le prescrit.
3. Les contrats run/level/checkpoint restent **non definis**, conformement au point 6 de l'ordre de livraison correctif.

---

## 3. Approche recommandee

**Option retenue : Option 1 — Direct Adjustment (ajustement direct).**

| Option | Evaluation | Verdict |
| --- | --- | --- |
| **1. Ajustement direct** — ajouter des stories dans l'Epic 5 existant, renumeroter et reecrire le checkpoint, ajuster 2 points en Epic 6 | Effort : **eleve** (10 stories). Risque : **moyen** — le gros du risque est l'outillage d'authoring, que la recherche nomme comme le gouffre de temps reel. Pas de perte de travail. | **Viable — retenu** |
| **2. Rollback** — annuler 5.2/5.4 et repartir | Effort : **moyen**. Risque : **eleve** et injustifie : 5.1, 5.3, 5.5, 5.6, 5.7 dependent du socle en place et restent valides. 5.2 et 5.4 sont superseded **en substance**, pas erronees ; elles ont servi de jalon. | **Non viable** |
| **3. Revue du MVP** — declarer MVP 1 termine, ouvrir MVP 2 | Effort : **tres eleve**. Risque : **eleve** — obligerait a definir maintenant les contrats run/level/checkpoint que la correction du 13/09 a explicitement differes, et a batir des niveaux sur des fondations encore en `review`. | **Non viable maintenant** |

### Justification

L'ajustement direct preserve l'integralite du travail livre, ne cree aucun epic, et respecte le cadre architectural existant — AD-31 exigeait deja la separation que la recherche recommande, et `MatchSettings` avait deja reserve la place des parametres ajoutes ici. Le seul artefact reellement ouvert est la cardinalite (AD-16), qui etait deja partiellement superseded.

Le risque principal n'est pas technique. La recherche etablit que ~30 vehicules est un ordre de grandeur sous toute limite Unity, et que le modele de decision voulu tient en une struct de parametres. **Le risque est l'outillage d'authoring du reseau de voies**, seul poste que les retours de terrain designent comme couteux. La story 5.9 le porte tot, deliberement, pour que le cout se revele pendant qu'il reste de la marge.

### Impact calendrier

L'Epic 5 passe de 8 a 18 stories. Les stories 5.1–5.7 restent en `review` et ne sont pas relivrees. Aucune story d'Epic 6 ou 7 n'est retardee autrement que par l'allongement de l'Epic 5 lui-meme.

---

## 4. Propositions de changement detaillees

### 4.1 Nouvelles decisions d'architecture

> A inserer dans `ARCHITECTURE-SPINE.md`, section « Invariants & Rules », apres AD-31.

---

**AD-32 — Effectifs de trafic configurables et echelle de validation [ADOPTED]**

- **Binds :** Vehicles, Run, Lobby, AI traffic, perimetre Epic 5.
- **Supersedes :** la clause « three spawned enemy vehicles » de AD-16, qui reste historique.
- **Prevents :** que la cardinalite MVP d'origine bloque le trafic urbain ; que des effectifs soient codes en dur dans un controleur ; et symetriquement qu'une ambition d'echelle non mesuree soit inscrite comme contrat.
- **Rule :** Les effectifs de trafic — nombre maximal de vehicules IA en circulation et nombre de vehicules autorises a jeter des dechets — sont **configurables et jamais codes en dur**. Les valeurs par defaut et les bornes minimum/maximum sont authorees dans un `Def` ScriptableObject (AD-12, AD-25) ; la valeur retenue pour une session vit dans `MatchSettings` et emprunte son chemin de synchronisation existant (Stories 1.2 et 2.4), jamais dans un asset ScriptableObject. La cible de validation de l'Epic 5 est d'environ **30 vehicules IA simultanes**. Toute echelle de production superieure est une **decision empirique**, prise apres la mesure de la Story 5.17, et jamais inscrite comme contrat d'architecture avant cette mesure. Les autres cardinalites de AD-16 (une route, une voiture joueur, quatre joueurs) restent inchangees.

---

**AD-33 — Conduite parametree et modulation emotionnelle [ADOPTED]**

- **Binds :** Vehicles, Rage, AI traffic, toute future archetype de conducteur.
- **Prevents :** que la rage et la peur soient implementees comme des etats de conduite paralleles, des multiplicateurs de vitesse, ou des branches ajoutees a une machine a etats monolithique.
- **Rule :** Le style de conduite d'un vehicule IA est une **structure de parametres authoree** (temps inter-vehiculaire, ecart minimal, acceleration, deceleration confortable, vitesse desiree, politesse de changement de voie, seuil de changement, freinage impose acceptable), portee par un `Def` ScriptableObject. L'acceleration longitudinale et la decision de changement de voie sont calculees par des fonctions pures a partir de cette structure. La rage et la peur produisent des parametres **effectifs** par modulation — `effectif = base x f(rage, peur)` — et **ne remplacent jamais la logique de conduite**. `com.unity.ai.navigation` est utilise **uniquement comme fournisseur de chemin** : `updatePosition` et `updateRotation` restent a `false`, le Rigidbody hote conduit (AD-21 inchange). Un vehicule ne porte jamais simultanement `NavMeshAgent` et `NavMeshObstacle`, et le carving n'est jamais actif sur un vehicule en mouvement.

---

**AD-34 — Trafic source/sink et cycle de vie des objets de rue [ADOPTED]**

- **Binds :** Vehicles, Run, AI traffic, dechets, futur contenu de niveau.
- **Prevents :** que des vehicules apparaissent ou disparaissent en pleine route, et que les objets jetes s'accumulent sans borne.
- **Rule :** Les vehicules IA **apparaissent et disparaissent exclusivement aux portails d'entree/sortie** authores. Aucun mecanisme — blocage, embouteillage, distance au joueur, echec de trajet — ne peut retirer un vehicule ailleurs. La variation de parcours vient d'un **tirage de virage par jonction**, avec ratios authores, et non d'un itineraire authore par vehicule. Un vehicule dont le parcours depasse un budget d'arcs authore est reoriente vers la sortie la plus proche. Les objets jetes sur la voie publique sont des NetworkObjects host-owned, referencent leur emetteur par `NetworkObjectReference` (AD-20), et sont bornes par un plafond global configure avec recyclage du plus ancien.

---

**AD-30 — note de clarification** (le texte de la regle reste inchange) :

> *Clarification 2026-09-15 :* le quartier urbain greybox livre par l'Epic 5 est une **small greybox integration** au sens de cette regle — un terrain de test pour le trafic, la conduite et les reactions rage/peur. Il n'est pas le Level 1 de MVP 2 : il ne porte ni commerces, ni equipement, ni boss, ni acces vehicule, ni recuperation de dechets, et aucun contrat run/level/checkpoint n'est introduit avec lui.

**AD-21 — reformulation mineure** : remplacer « AI spline progress » par « AI route progress on the authored lane graph ». Le reste de la regle est inchange.

**AD-17 — ajout** : ajouter a la liste des proprietaires d'etat : « `NetworkedLitterState` owns thrown-litter registry, per-piece source attribution, and the configured live-litter cap. »

### 4.2 Modification des exigences fonctionnelles

**FR6**

```
ANCIEN :
FR6: The MVP includes three AI vehicles on the same route.

NOUVEAU :
FR6: The MVP includes a routed urban traffic population of AI vehicles sharing a
lane graph, with its size configured from the lobby and validated at approximately
thirty simultaneous vehicles. Vehicles enter and leave only through authored portals.
```

**FR8**

```
ANCIEN :
FR8: AI vehicles can remain calm, become irritated, flee, block, ram, or trigger
a confrontation based on their own rage.

NOUVEAU :
FR8: AI vehicles express calm, irritation, flight, blocking, ramming and
confrontation as modulations of one parameterized driving model driven by their
own rage and fear, targeting the player who provoked them.
```

**FR26** — ajout en fin d'enonce (l'exigence porte deja la croissance de l'UI) :

```
AJOUT :
…and the in-run UI includes an escape menu that returns the player to the main menu
through the existing session-exit path.
```

### 4.3 Reconciliation du tableau perime d'`epics.md`

```
ANCIEN (tableau « Corrective roadmap (current) ») :
| 3 | 5.1 Configurable NPC Rage/Fear Foundation | ... | backlog |
| 4 | 6.5 Individual Wallet Authority | ... | backlog |
| later | MVP 2 assembly | Levels, litter loop, encounters, bosses, ... | deferred |

NOUVEAU :
| 3 | 5.1 – 5.7 | Fondation NPC, trafic, ciblage, Rage Road, presentation client. | implemente, en review |
| 4 | 5.8 – 5.17 | Remaniement du trafic : conduite parametree, graphe de voies et
      quartier greybox, regles d'intersection, perception, modulation rage/peur,
      echelle de rage ciblee, fondation dechets, reglages de lobby, menu d'echappement,
      mesure d'echelle. Voir sprint-change-proposal-2026-09-15.md. | backlog |
| 5 | 5.18 | Checkpoint jouable Epic 5, reecrit. Derniere story de l'epic. | backlog |
| 6 | 6.5 Individual Wallet Authority | Inchange. | backlog |
| later | MVP 2 assembly | Niveaux, boucle de recuperation des dechets, rencontres,
      boss, flux run/checkpoint, equilibrage, integration finale. | deferred |
```

### 4.4 Nouvelles stories Epic 5

> Inserees dans `epics.md`, section Epic 5, apres la Story 5.7. L'ancienne Story 5.8 (checkpoint) devient la Story 5.18, reecrite.
>
> **Renumerotation (revision 3, meme jour) :** les numeros ci-dessous sont ceux de la premiere redaction (revision 2). Ils ont ete reincrementes une seconde fois pour que la sequence suive le graphe de dependances de la section 5 sans a-coup. Correspondance vers les numeros finaux (ceux d'`epics.md` et de `sprint-status.yaml`, faisant foi) :
>
> | Redige ici (rev. 2) | Numero final (rev. 3) | Story |
> | --- | --- | --- |
> | 5.8 | **5.9** | Modele de conducteur parametre |
> | 5.9 | **5.10** | Graphe de voies, quartier greybox et trafic source/sink |
> | 5.10 | **5.11** | Regles d'intersection et prevention d'interblocage |
> | 5.11 | **5.12** | Perception elargie et deblocage progressif |
> | 5.12 | **5.13** | Rage et peur comme modulation du modele de conduite |
> | 5.13 | **5.14** | Echelle de rage ciblee sur le joueur et declenchement Rage Road |
> | 5.14 | **5.15** | Fondation des dechets jetes et attribution |
> | 5.15 | **5.16** | Reglages de trafic configurables depuis le lobby |
> | 5.16 | **5.8** | Menu d'echappement et retour au menu principal |
> | 5.17 | **5.17** | Validation d'echelle et budget reseau (inchange) |
> | 5.18 | **5.18** | Checkpoint jouable de l'Epic 5 (inchange) |
>
> Le texte des stories ci-dessous reste sous son numero de redaction d'origine ; ne pas l'utiliser pour retrouver une story dans `sprint-status.yaml`, seule la colonne de droite y correspond.

---

#### Story 5.8 : Modele de conducteur parametre

**Implements :** FR6, FR8, FR24, FR25, FR27, AD-33, NFR4

As a player,
I want AI drivers to have a real driving style rather than a speed setting,
So that rage and fear can later change how they drive and not just how fast.

**Acceptance Criteria :**

**Given** des vehicules IA existent dans la scene
**When** le modele de conduite s'execute cote hote
**Then** le style de conduite de chaque vehicule est porte par une structure de parametres authoree dans un `Def` ScriptableObject — temps inter-vehiculaire, ecart minimal, acceleration maximale, deceleration confortable, vitesse desiree, politesse de changement de voie, seuil de changement, freinage impose acceptable — et aucune de ces valeurs n'est codee en dur dans un controleur
**And** l'acceleration longitudinale est calculee par une fonction pure prenant l'ecart au vehicule de tete et la vitesse de rapprochement projetee
**And** la decision de changement de voie est calculee par une fonction pure superposee a cette meme fonction d'acceleration, et comprend un critere de securite qui veto le changement independamment de tout gain
**And** l'evaluation de changement de voie est cadencee (environ une fois par seconde) avec des minuteurs desynchronises par vehicule, de sorte que son cout ne s'exprime pas a chaque frame

**Given** la Story 5.4 avait exprime les paliers de rage comme des multiplicateurs de vitesse
**When** ce modele est en place
**Then** `ResolveCruiseSpeedMultiplier` est supprime et les comportements qu'il produisait sont re-exprimes via la structure de parametres
**And** aucun chemin de code ne conserve un reglage de conduite exprime uniquement en vitesse

**Given** les fonctions de decision sont pures
**When** les tests s'executent
**Then** elles sont couvertes en EditMode sans scene ni reseau, y compris les cas limites de vehicule de tete a l'arret et d'ecart nul

---

#### Story 5.9 : Graphe de voies, quartier greybox et trafic source/sink route

**Implements :** FR6, FR24, FR25, FR27, AD-32, AD-34, AD-30 (note), NFR2, NFR18

As a player,
I want traffic to come from outside the city, cross it by varying routes and leave,
So that the world reads as a living district rather than a visible loop.

**Acceptance Criteria :**

**Given** `MVP_Run` doit accueillir le trafic
**When** le quartier de test est authore
**Then** il contient au moins deux routes a double sens, au moins deux intersections et au moins deux portails de type tunnel, en greybox conforme a AD-14 et AD-27
**And** les trottoirs sont **exclus du masque d'aire** de l'agent vehicule, et pas seulement affectes d'un cout eleve
**And** le quartier est documente comme terrain de test de fondation, explicitement pas le Level 1 de MVP 2

**Given** le graphe de voies et les portails existent
**When** la simulation de trafic demarre
**Then** les vehicules apparaissent uniquement aux portails et disparaissent uniquement aux portails
**And** chaque vehicule choisit son virage a chaque jonction par tirage aleatoire pondere par des ratios authores sur cette jonction
**And** un vehicule dont le parcours depasse un budget d'arcs authore est reoriente vers la sortie la plus proche plutot que d'errer indefiniment
**And** un portail peut etre configure comme sortie, ou comme sortie reutilisant l'entree d'origine

**Given** le trafic tourne
**When** des vehicules quittent la zone
**Then** de nouveaux vehicules apparaissent aux portails pour tendre vers un **effectif cible resolu a l'execution**, jamais vers une constante codee en dur
**And** l'effectif cible provient des reglages de session (Story 5.15), avec repli sur la valeur par defaut authoree tant que ces reglages n'existent pas
**And** aucun vehicule n'est retire ailleurs qu'a un portail, quelle qu'en soit la raison — blocage, embouteillage, distance au joueur ou echec de trajet
**And** l'insertion impossible parce qu'un portail est encombre met le vehicule en file plutot que de l'abandonner

---

#### Story 5.10 : Regles d'intersection et prevention d'interblocage

**Implements :** FR6, FR24, FR25, FR27, NFR4

As a player,
I want AI vehicles to respect simple traffic rules at intersections,
So that the city reads as ordered before rage makes it chaotic.

**Acceptance Criteria :**

**Given** des intersections existent dans le graphe de voies
**When** elles sont authorees
**Then** la priorite de chaque flux est decidee a l'authoring — voie prioritaire, priorite a droite, stop — et n'est pas recalculee a chaque frame
**And** un panneau stop impose un arret complet puis un creneau minimal accepte avant de franchir

**Given** un vehicule approche d'une intersection
**When** il evalue s'il s'engage
**Then** il ne s'engage pas si sa voie de sortie n'a pas la place de l'accueillir
**And** cette verification precede l'application de la regle de priorite

**Given** un blocage se forme malgre les regles
**When** l'attente d'un vehicule se prolonge
**Then** une echelle d'infraction progressive et authoree s'applique : d'abord s'engager dans une intersection normalement gardee libre, puis contourner un bloqueur
**And** chaque palier est authore et activable, aucun n'est desactive par defaut
**And** **aucun palier de suppression, de teleportation ou de reinsertion ailleurs n'existe**
**And** un interblocage detecte est journalise en build de developpement avec les vehicules impliques

---

#### Story 5.11 : Perception elargie et deblocage progressif

**Implements :** FR6, FR8, FR24, FR25, FR27, NFR4

As a player,
I want AI drivers to notice what is around them and react rather than freeze,
So that they still behave sensibly when players and enraged AI make the road chaotic.

**Acceptance Criteria :**

**Given** un vehicule IA circule
**When** il evalue sa situation
**Then** il percoit plusieurs vehicules et joueurs dans un rayon et un secteur authores, et pas uniquement le vehicule directement devant lui
**And** cette perception alimente a la fois la selection du vehicule de tete pour le modele de conduite et l'arbitrage aux intersections
**And** le cout de perception est borne par un index spatial ou une cadence reduite, jamais par un balayage de tous les vehicules a chaque frame

**Given** le chemin d'un vehicule est bloque
**When** le blocage est detecte
**Then** la replanification demarre immediatement et se poursuit en continu, sans periode d'attente prealable
**And** l'escalade visible est progressive : klaxon apres un delai court authore de l'ordre de deux a quatre secondes, puis tentative de contournement des qu'un creneau est acceptable
**And** un vehicule bloque n'est jamais teleporte, reinsere ailleurs ni supprime pour resoudre le blocage
**And** une manoeuvre de degagement ne fait pas monter le vehicule sur un trottoir

---

#### Story 5.12 : Rage et peur comme modulation du modele de conduite

**Implements :** FR7, FR8, FR24, FR25, FR27, AD-31, AD-33, NFR4, NFR6

As a player,
I want rage and fear to change how an AI drives rather than replace its driving,
So that an enraged driver still behaves like a driver.

**Acceptance Criteria :**

**Given** un vehicule porte ses propres jauges de rage et de peur (Story 5.1)
**When** l'une de ces jauges change
**Then** les parametres de conduite effectifs sont obtenus par modulation des parametres de base en fonction de la rage et de la peur
**And** aucune branche de code ne remplace le modele de conduite par une logique alternative selon l'etat emotionnel
**And** l'etat d'un vehicule n'influence jamais les parametres d'un autre

**Given** les jauges evoluent dans le temps
**When** aucune source ne les alimente
**Then** chaque jauge decroit d'un taux authore, par defaut un pour cent par seconde
**And** l'entree dans un nouveau palier de rage gele la jauge pendant une duree authoree, par defaut dix secondes, avant que la decroissance ne reprenne
**And** a cent pour cent de rage la jauge ne decroit plus tant que l'evenement Rage Road associe n'est pas sorti des etats `Triggered` et `Confrontation`
**And** une montee de jauge reste possible pendant un gel

**Given** rage et peur coexistent
**When** l'arbitrage s'applique
**Then** une peur a cent pour cent prend le dessus sur la rage quel que soit son niveau
**And** une peur superieure ou egale a cinquante pour cent et strictement superieure a la rage produit un comportement de fuite, qui ne cesse qu'une fois la peur redescendue a trente pour cent ou moins
**And** tant que la peur n'atteint pas cent pour cent et ne depasse pas la rage, la rage gouverne le comportement
**And** tous les seuils de cet arbitrage sont authores, pas codes en dur
**And** toutes ces transitions sont host-authoritative

---

#### Story 5.13 : Echelle de rage ciblee sur le joueur et declenchement Rage Road

**Implements :** FR7, FR8, FR11, FR24, FR25, FR27, AD-22, NFR4, NFR5

As a player,
I want an AI I provoked to escalate against me specifically,
So that road rage feels personal and leads somewhere.

**Acceptance Criteria :**

**Given** un vehicule IA a une cible resolue par le mecanisme de ciblage de la Story 5.5
**When** sa rage franchit les paliers
**Then** entre vingt et quarante pour cent il klaxonne en direction de cette cible
**And** entre quarante et quatre-vingts pour cent il suit cette cible, et lorsqu'il arrive a son niveau il l'invective et lui jette un dechet
**And** entre quatre-vingts et quatre-vingt-dix-neuf pour cent il tente de percuter cette cible
**And** a cent pour cent il poursuit cette cible jusqu'a son arret, puis demande le declenchement de l'evenement Rage Road
**And** les bornes de ces paliers sont authorees, pas codees en dur

**Given** un dechet jete par un vehicule enrage atteint un joueur
**When** l'impact est resolu
**Then** la perte de points de vie du joueur est appliquee cote hote a travers une intention validee, jamais par le client
**And** l'effet est authore comme definition, pas code en dur par appelant

**Given** la peur d'un vehicule atteint cent pour cent
**When** ce vehicule est encore en circulation
**Then** il rejoint a vitesse elevee le portail de sortie le plus proche par le chemin le plus court, puis y disparait
**And** il ne disparait a aucun autre endroit

**Given** l'evenement Rage Road existe deja (Story 5.6)
**When** la poursuite aboutit
**Then** la demande de declenchement passe par le cycle de vie existant `Idle -> Triggered -> Confrontation` sans en introduire un second
**And** la resolution de la confrontation reste hors perimetre de cette story

---

#### Story 5.14 : Fondation dechets jetes et attribution

**Implements :** FR17, FR24, FR25, FR27, AD-13, AD-20, AD-32, AD-34, NFR13, NFR18

As a player,
I want some AI drivers to throw litter out of their window,
So that the road accumulates evidence of who behaved badly.

**Acceptance Criteria :**

**Given** le trafic circule
**When** les vehicules lanceurs sont selectionnes
**Then** le nombre de vehicules autorises a jeter des dechets simultanement est **resolu a l'execution depuis les reglages de session** (Story 5.15), jamais depuis une constante
**And** la valeur par defaut et les bornes sont authorees, avec cinq lanceurs par defaut
**And** chaque vehicule lanceur jette au plus un nombre authore de dechets, trois par defaut, entre son apparition et sa disparition

**Given** un vehicule jette un dechet
**When** l'objet est cree
**Then** il s'agit d'un NetworkObject host-owned dote d'une physique d'objet leger
**And** il reference le vehicule emetteur par `NetworkObjectReference`, jamais par un identifiant authore ni un index
**And** l'asset greybox du dechet a franchi la porte d'intake Blender conforme a AD-13

**Given** des dechets s'accumulent au fil d'une session
**When** le plafond global configure de dechets vivants est atteint
**Then** le plus ancien est recycle
**And** un dechet dont le vehicule emetteur a disparu reste valide mais perd sa cible de renvoi

**Given** un dechet peut etre renvoye a son vehicule emetteur
**When** ce renvoi est resolu cote hote
**Then** ce vehicule est marque pour rejoindre le portail de sortie le plus proche et cesse de pouvoir jeter des dechets
**And** ce marquage reutilise le meme mode de navigation que la fuite de peur de la Story 5.13

**Given** la boucle joueur de recuperation appartient a l'Epic 6
**When** cette story est livree
**Then** le ramassage, la mise a la poubelle et le renvoi cote joueur **ne sont pas implementes ici**, seule l'intention cote hote qui les recevra est exposee

---

#### Story 5.15 : Parametres de trafic configurables depuis le lobby

**Implements :** FR3, FR6, FR26, FR27, AD-12, AD-25, AD-32, NFR4

As a host player,
I want to set how many cars populate the city and how many of them throw litter before the run starts,
So that I can tune test conditions without touching code or scenes.

**Acceptance Criteria :**

**Given** `MatchSettings` porte deja un emplacement reserve aux parametres de partie extensibles (Story 1.2)
**When** les parametres de trafic sont ajoutes
**Then** ils etendent `MatchSettings` et **aucun second objet de reglages de partie n'est cree**
**And** les valeurs par defaut et les bornes minimum/maximum sont authorees dans un `Def` ScriptableObject, tandis que la valeur choisie pour la session vit dans `MatchSettings`, conformement a la regle existante selon laquelle les valeurs de session runtime ne vivent jamais dans un asset

**Given** l'hote est dans le lobby
**When** il ouvre les reglages de partie
**Then** il peut regler le nombre maximal de vehicules IA en ville et le nombre de vehicules lanceurs de dechets
**And** chaque valeur est bornee par les limites authorees et une saisie hors bornes est refusee avec un retour visible
**And** le nombre de lanceurs ne peut pas depasser le nombre total de vehicules
**And** seul l'hote peut editer ces valeurs, comme pour le reglage de difficulte existant

**Given** des clients sont connectes au lobby
**When** l'hote demarre la run
**Then** les clients recoivent ces valeurs avant le chargement du monde, par le chemin de synchronisation de reglages deja livre par la Story 2.4
**And** aucun second chemin de synchronisation n'est introduit

**Given** la run demarre
**When** l'hote compose le trafic
**Then** l'effectif cible et le nombre de lanceurs proviennent des reglages resolus
**And** aucun controleur, prefab ou scene ne porte de constante d'effectif
**And** modifier ces valeurs entre deux runs est effectif sans recompilation ni edition de scene

---

#### Story 5.16 : Menu d'echappement et retour au menu principal

**Implements :** FR26, FR27, AD-26, UX-DR3, NFR11, NFR12

As a player,
I want to press Escape and get back to the main menu,
So that I can leave a test run without killing the process.

**Acceptance Criteria :**

**Given** un joueur est dans `MVP_Run`
**When** il presse Echap
**Then** un menu d'echappement s'affiche avec au minimum Reprendre et Quitter vers le menu principal
**And** l'ouverture du menu **ne met pas en pause la simulation host-authoritative** — une session multijoueur ne se fige pas parce qu'un joueur ouvre un menu
**And** la capture et le mode du curseur sont retablis proprement a l'ouverture et a la fermeture
**And** les entrees de conduite et d'action du joueur sont neutralisees tant que le menu est ouvert

**Given** le menu est ouvert
**When** le joueur presse Echap a nouveau ou choisit Reprendre
**Then** le menu se ferme et le controle du jeu lui est rendu dans l'etat ou il l'avait laisse

**Given** un joueur choisit de quitter vers le menu principal
**When** l'action est confirmee
**Then** il retourne a `MainMenuLobby` **par le chemin de sortie de session existant de la Story 2.7**, sans qu'un second chemin de teardown soit introduit
**And** si ce joueur est l'hote, les clients restants sont traites exactement comme le host-quit deja specifie par la Story 2.7
**And** si ce joueur est un client, l'hote et les autres clients poursuivent la run sans interruption
**And** le retour au menu respecte AD-26 : aucun chemin de chargement ne contourne le lobby et l'hote

---

#### Story 5.17 : Validation d'echelle et budget reseau

**Implements :** FR24, FR27, AD-28, AD-32, NFR2, NFR5

As a solo developer,
I want measured numbers rather than estimates before I commit to a traffic scale,
So that the production target is chosen on evidence.

**Acceptance Criteria :**

**Given** les reglages de trafic sont configurables (Story 5.15)
**When** une session hote plus un client est mesuree avec le profileur reseau du package Multiplayer Tools
**Then** la mesure est effectuee pour au moins trois effectifs distincts, dont environ trente vehicules, en pilotant simplement le reglage de lobby
**And** la bande passante par objet et totale est relevee et consignee dans le journal de validation pour chaque effectif
**And** le cout CPU hote des couches conduite, perception et calcul de chemin est releve separement

**Given** les leviers de reduction documentes existent
**When** `NetworkTransform` est configure sur les vehicules IA
**Then** les axes non necessaires sont desactives, la compression de rotation et la demi-precision de position sont evaluees, et les seuils sont regles
**And** l'effet de chaque levier retenu est mesure, pas suppose

**Given** une echelle de production superieure est envisagee
**When** cette story se termine
**Then** un enonce de plafond et de marge est produit pour appuyer la decision cinquante ou cent vehicules
**And** aucune decision d'echelle de production n'est prise sans cette mesure

---

#### Story 5.18 : Checkpoint jouable Epic 5

> **Remplace l'ancienne Story 5.8.** Cle sprint-status renumerotee de `5-8-…` a `5-18-…` ; la story etait en `backlog`, aucun travail n'y etait attache.

**Implements :** FR6, FR7, FR8, FR11, FR17, FR24, FR25, FR26, FR27

As a solo developer,
I want a playable urban traffic and Rage Road checkpoint,
So that I can test the escalation path before building confrontation resolution.

```
ANCIEN :
**Given** AI traffic, rage behavior states, and the Rage Road trigger exist
**When** the game is tested in `MVP_Run` or `Dev_RageSandbox`
**Then** players can drive near three AI vehicles, trigger rage changes through
passenger actions, and create one Rage Road event
**And** the event remains visible and stable until resolved or reset
**And** local and online smoke tests confirm host-authoritative state updates
**And** the checkpoint notes list tuning assumptions for Epic 6 confrontation design

NOUVEAU :
**Given** les stories 5.8 a 5.17 sont livrees
**When** le jeu est teste dans `MVP_Run`
**Then** l'hote regle depuis le lobby le nombre de vehicules et de lanceurs de
dechets, et la run respecte ces valeurs
**And** les vehicules circulent dans le quartier greybox en entrant et sortant par
les portails, sans qu'aucun ne disparaisse en pleine route
**And** les vehicules respectent les priorites et les stops, s'inserent aux
intersections sans interblocage durable, et reagissent aux joueurs et aux obstacles
**And** un joueur peut provoquer un vehicule et observer l'escalade complete
klaxon, poursuite, tentative de percussion, puis declenchement Rage Road
**And** un vehicule dont la peur sature rejoint un portail et y disparait
**And** des dechets jetes sont visibles, physiques, et attribues a leur vehicule emetteur
**And** un joueur peut quitter la run par le menu d'echappement et revenir au menu
principal, en solo comme en ligne
**And** un test fume local et un test fume en ligne a deux joueurs confirment que
tous ces etats sont host-authoritative et coherents cote client
**And** les mesures de la Story 5.17 sont consignees et les hypotheses de reglage
pour la confrontation de l'Epic 6 sont listees
```

### 4.5 Registre d'adoption

Ajouter une ligne au registre `docs/setup/addon-adoption-register.md` :

| Champ | Valeur |
| --- | --- |
| ID | ADDON-008 |
| Statut | `Pass` |
| Candidat | Gley Mobile Traffic System v3 (systeme de trafic Asset Store) |
| Categorie | Fondation trafic IA |
| Source / URL | `https://assetstore.unity.com/packages/tools/behavior-ai/mobile-traffic-system-v3-305800` |
| Version | v3.6.4, publiee 2026-09-04 |
| Licence | EULA Asset Store standard, Extension Asset |
| Cout | EUR 118.69 |
| Decision | **`Reject`** |
| Preuve | `planning-artifacts/research/technical-trafic-ia-vehicules-unity-6-2026-09-15/research.md`, dimensions D1 et D2 |
| Notes | Rejete sur le critere d'extensibilite, pas sur le prix ni la maintenance. L'API de surcharge est reelle et per-vehicule, mais sa sortie `BehaviourResult` est une structure de commande et non de parametres : aucun temps inter-vehiculaire, ecart minimal, deceleration ni politesse n'est expose sur plus de soixante pages d'API. Atteindre ces constantes imposerait de forker les classes livrees et de les refusionner a chaque mise a jour. L'API est de plus entierement `static`, donc singleton global au processus, ce qui s'oppose a la replication host-authoritative. Point non clos : le delegue `Modify Trigger Size` pourrait offrir un levier de distance de suivi s'il peut etre clef sur l'identite du vehicule — non verifie, a trancher avant toute reevaluation. |

### 4.6 Changements Epic 6

**Story 6.3 — On-Foot Transition for Confrontation and Sandbox Stops**

```
AJOUT d'un critere d'acceptation :

**And** l'entree en confrontation declenchee par un vehicule IA provient de l'etat
de poursuite a rage maximale de la Story 5.14, et le verrou de jauge de la Story
5.13 maintient ce vehicule a rage maximale jusqu'a ce que l'evenement Rage Road
quitte les etats `Triggered` et `Confrontation`
```

**Nouvelle Story 6.10 — Boucle de recuperation des dechets**

> *Applique le 2026-09-15 :* l'Epic 6 se terminait par son propre checkpoint en 6.10. La meme regle que pour l'Epic 5 a ete appliquee — la boucle de recuperation prend le numero **6.10** et le checkpoint devient **6.11**, pour qu'un checkpoint d'epic reste la derniere story de son epic. La cle `6-10-epic-6-economy-loop-playable-checkpoint` etait en `backlog`, la renumerotation est donc sans risque.

**Implements :** FR17, FR24, FR25, FR27, AD-34

As a player,
I want to pick up the litter an AI threw and either bin it or throw it back,
So that bad behaviour on the road has a consequence I can deliver myself.

**Acceptance Criteria :**

**Given** des dechets attribues existent dans le monde (Story 5.15, renvoi depuis la Story 6.10)
**When** un joueur a pied s'en approche
**Then** il peut le ramasser, et le dechet ramasse occupe un emplacement de son inventaire (Story 6.1)

**Given** un joueur porte un dechet
**When** il le depose dans une poubelle
**Then** le dechet est retire du monde par l'hote et le joueur recoit un retour visible

**Given** un joueur porte un dechet dont le vehicule emetteur est encore en circulation
**When** il le renvoie dans ce vehicule
**Then** l'intention exposee par la Story 5.15 est appelee cote hote, et ce vehicule rejoint le portail de sortie le plus proche en cessant de jeter des dechets
**And** un renvoi visant un vehicule dont l'emetteur a disparu echoue proprement avec un retour joueur

### 4.7 Note d'impact UX

`ux-designs/ux-RoadRage_Simulator-2026-09-13` recoit une note d'impact, sans refonte :

- Le klaxon, l'invective, l'impact de dechet et l'etat de poursuite ont besoin d'un retour joueur lisible — cadre par la Story 5.14 et le HUD existant de la Story 2.6.
- Le lobby recoit deux champs numeriques bornes, a cote du reglage de difficulte existant — Story 5.16.
- `MVP_Run` recoit une surface d'interface nouvelle : le menu d'echappement — Story 5.8. C'est la seule nouvelle surface introduite par cette proposition.

### 4.8 sprint-status.yaml

> Version finale (revision 3) — reflete la table de correspondance de la section 4.4.

```yaml
  epic-5: in-progress
  5-1-configurable-npc-rage-fear-foundation: review
  5-2-basic-ai-route-following-and-recovery: review
  5-3-unified-solo-and-online-session-start: review
  5-4-rage-driven-ai-behavior-states: review
  5-5-networked-ai-rage-targeting: review
  5-6-rage-road-event-trigger: review
  5-7-ai-traffic-networking-and-client-presentation: review
  5-8-escape-menu-and-return-to-main-menu: backlog
  5-9-parameterized-driver-model: backlog
  5-10-lane-graph-greybox-district-and-routed-traffic: backlog
  5-11-intersection-rules-and-deadlock-prevention: backlog
  5-12-wider-perception-and-progressive-unblocking: backlog
  5-13-rage-and-fear-as-driving-model-modulation: backlog
  5-14-player-targeted-rage-ladder-and-rage-road-trigger: backlog
  5-15-thrown-litter-foundation-and-attribution: backlog
  5-16-lobby-configurable-traffic-settings: backlog
  5-17-scale-validation-and-network-budget: backlog
  5-18-epic-5-ai-traffic-playable-checkpoint: backlog
  epic-5-retrospective: optional
```

La cle `5-8-epic-5-ai-traffic-playable-checkpoint` est **retiree** et remplacee par `5-18-epic-5-ai-traffic-playable-checkpoint`.

Plus, dans Epic 6, la boucle de recuperation est inseree **avant** le checkpoint, qui est renumerote pour rester dernier :

```yaml
  6-9-economy-confrontation-and-stop-ui-feedback: backlog
  6-10-litter-recovery-loop: backlog
  6-11-epic-6-economy-loop-playable-checkpoint: backlog
```

Et un commentaire d'en-tete :

```yaml
# 2026-09-15 course correction: Epic 5 scope expanded (see
# planning-artifacts/sprint-change-proposal-2026-09-15.md). Stories 5.8-5.17 added.
# The epic checkpoint moved from 5.8 to 5.18 so it sits last in its own epic; it was
# in backlog, so no work was attached to the renumbered key. Stories 5.1-5.7 keep
# their numbers and keys. 5.2 and 5.4 remain in review as historical milestones;
# their mechanisms are superseded by 5.9-5.12. Epic 6 gains 6-10-litter-recovery-loop
# and its checkpoint moved 6-10 -> 6-11 for the same last-in-epic reason.
# Traffic headcounts are lobby-configurable (5.16), never hardcoded.
# 2026-09-15 (same day, follow-up): Stories 5.8-5.17 reordered so numbering
# increments along the dependency chain from the sequencing diagram below.
# 5.16 (Escape Menu) had no dependency on the traffic work and moved to 5.8
# for early, independent delivery; every other story shifted down by one.
# All keys were still in backlog, so this was a safe rename.
```

*Note :* la Story 5.18 est la story de checkpoint de l'epic. Conformement au fonctionnement etabli, c'est son passage au vert qui promeut en bloc les stories de l'Epic 5 vers `done` ; les revues individuelles ne le font pas.

---

## 5. Handoff d'implementation

**Classification : Moderate** — reorganisation de backlog avec coordination Product Owner / Developer. Pas de replan fondamental : aucun epic cree ou supprime, la pile technique est inchangee, l'AD le plus structurant (AD-31) etait deja conforme, et les deux mecanismes reutilises (`MatchSettings`, sortie de session) existent deja.

| Destinataire | Responsabilite | Livrables |
| --- | --- | --- |
| **Product Owner / Developer** | Appliquer les editions de la section 4 aux artefacts de planification. | `epics.md` (FR6, FR8, FR26, tableau roadmap, stories 5.8–5.18, 6.3, 6.10, 6.11), `ARCHITECTURE-SPINE.md` (AD-32, AD-33, AD-34, notes AD-30/AD-21/AD-17), `sprint-status.yaml`, `addon-adoption-register.md` (ADDON-008). |
| **Developer (bmad-build)** | Implementer les stories dans l'ordre 5.8 → 5.17, puis 5.18 (numeros finaux, voir table de correspondance en 4.4). | Code, tests EditMode des fonctions pures, integration verifiee dans `MVP_Run` conformement a la regle `AGENTS.md`. |
| **Architect** | Aucune escalade requise. Consultation seulement si la mesure de la Story 5.17 contredit AD-32. | — |

### Sequencement et dependances

> Numeros finaux (revision 3) — ce sont ceux d'`epics.md` et de `sprint-status.yaml`.

```
5.8  (menu echap) ─── independant, livrable a tout moment ─────────────────────────────┐
                                                                                        │
5.9  (modele parametre)  ─┬─> 5.13 (modulation rage/peur) ──> 5.14 (echelle ciblee) ──┐ │
                          │                                          │                │ │
5.10 (graphe + quartier) ─┼─> 5.11 (regles intersection)             │                │ │
                          │                                          │                │ │
                          └─> 5.12 (perception + deblocage) ─────────┘                │ │
                                                                                       │ │
5.15 (dechets) ─── depend de 5.10 (portails) et 5.14 (jet en rage) ──────────────────┤ │
                                                                                       │ │
5.16 (reglages lobby) ─── depend de 5.10 et 5.15 pour avoir quoi configurer ─────────┤ │
                                                                                       │ │
5.17 (mesure) ─── depend de 5.16 pour piloter l'effectif, et de tout le reste ───────┤ │
                                                                                       │ │
5.18 (checkpoint) ─── s'execute en dernier, rassemble tout, y compris 5.8 ───────────┴─┘
```

**5.8 part en premier** : elle n'a aucune dependance sur la chaine trafic et peut etre livree des que souhaite. La monter en tete a une valeur pratique directe — elle rend confortable le test de tout le reste de l'epic, puisqu'une run de trafic longue peut alors etre quittee proprement.

**5.9 et 5.10 peuvent demarrer en parallele juste apres** : l'une est du code de decision testable hors scene, l'autre est de l'authoring. C'est deliberement 5.10 qui porte le risque identifie par la recherche — l'outillage d'authoring du reseau de voies est le seul poste que les retours de terrain designent comme couteux. La lancer tot fait apparaitre ce cout tant qu'il reste du temps pour reagir.

**5.16 debloque 5.17** : c'est le reglage de lobby qui permet de balayer plusieurs effectifs pour la mesure, sans recompiler.

### Criteres de succes

1. L'hote regle depuis le lobby le nombre de vehicules et de lanceurs, et la run respecte ces valeurs — aucun effectif n'est code en dur.
2. Un joueur peut conduire dans un quartier a double sens peuple d'environ trente vehicules IA qui respectent les priorites et les stops.
3. Aucun vehicule n'apparait ni ne disparait ailleurs qu'a un portail, y compris sous blocage.
4. Un vehicule provoque escalade jusqu'a la confrontation en passant par des paliers visibles, et un vehicule terrorise fuit vers un portail.
5. La rage et la peur agissent sur les parametres de conduite, jamais en remplacant la logique de conduite ; `ResolveCruiseSpeedMultiplier` n'existe plus.
6. Un joueur peut quitter une run par Echap et revenir au menu principal, en solo comme en ligne, sans second chemin de teardown.
7. Les mesures d'echelle sont consignees et appuient la decision d'echelle de production.
8. Le tout est verifie dans `MVP_Run`, pas seulement dans une scene `Dev_*`.

### Risques residuels

| Risque | Probabilite | Mitigation |
| --- | --- | --- |
| L'authoring du graphe de voies coute plus que prevu | **Elevee** — c'est le poste que la recherche designe | 5.10 lancee tot ; si le cout explose, l'option d'adopter Gley pour son seul outillage d'authoring reste documentee dans ADDON-008, au prix du plafonnement de la couche emotionnelle |
| Les dechets font franchir un seuil de performance | Faible | Plafonnes par conception et par configuration ; mesures en 5.17 |
| L'interblocage aux intersections se revele au mois trois | Moyenne | La recherche note que ce risque est **non teste, pas refute** ; la regle keep-clear de 5.11 est la couverture a priori, la journalisation en build dev est le detecteur |
| Judder client entre conduite hote en FixedUpdate et interpolation reseau | Moyenne | Non documente publiquement ; a observer explicitement pendant le test fume a deux joueurs de la Story 5.18 |
| Le menu d'echappement derive vers une pause reelle en multijoueur | Faible | Interdit explicitement par un critere d'acceptation de la Story 5.8 : la simulation host-authoritative ne se met jamais en pause pour un joueur |
