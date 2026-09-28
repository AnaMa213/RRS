# Regles du cycle de build

Charge automatiquement au debut de chaque run `bmad-build` (via `persistent_facts` dans
`_bmad/custom/bmad-build.toml`). Ce fichier porte **ce qui est specifique au cycle de build**.
Le transversal (garde-fou Graphify sur symbole nomme, garde d'etat Unity MCP, interdiction de
verification par l'agent, perimetre du graphe, assets tiers) vit dans `AGENTS.md` : il n'est pas
recopie ici.

Invariants d'architecture : `_bmad-output/planning-artifacts/architecture/architecture-RRS-devworkflow-2026-09-17/ARCHITECTURE-SPINE.md`.
Decisions par outil : `docs/setup/addon-adoption-register.md`.

Le spec BMAD reste la seule autorite sur le scope, les acceptance criteria et la DoD. Ce fichier
ne les remplace jamais ; en cas de divergence, le spec gagne.

---

"""
Subagent delegation policy:
Prefer direct tool calls over subagent delegation for simple repository exploration,
single-file inspection, searches, and sequential work.

Spawn subagents only when at least one of these applies:
- the work is genuinely independent and can benefit from parallel execution;
- isolated context materially improves the result;
- a specialized subagent provides capabilities or expertise useful to the task.

Do not spawn general-purpose subagents solely for routine reconnaissance that can be
completed with a small number of direct Graphify, search, or file-inspection calls.
Avoid overlapping subagents investigating the same concern.
"""

## 1. Recon conditionnelle

Choisis le niveau de recon avant d'ouvrir quoi que ce soit. Le surcout d'une recon inutile est du
bruit ; le cout d'une recon manquante est une reponse fausse presentee avec assurance.

| Situation                                                                                                                       | Action                                                                                                                                                                                                                                                                                                                             |
| ------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Un symbole precis est nomme (`RunFlowController`, `DriverProfileDef`, `NetworkedAIVehicleDriverController`…)                    | **Precheck obligatoire avant toute requete Graphify** : `.\scripts\find-symbol.ps1 <Symbole>` (voir `AGENTS.md` AD-3). Sortie `1` = absent -> STOP. Sortie `2` = la recherche a echoue -> resultat **inconnu**, ne pas conclure a une absence. Sortie `0` -> Graphify seulement si une analyse d'impact est reellement necessaire. |
| Question macro sans symbole nomme (« quels systemes participent au flow de lobby ? », « qui depend de X ? », impact cross-file) | Graphify directement, sans precheck.                                                                                                                                                                                                                                                                                               |
| Changement XS/S evident (libelle, commentaire, valeur de tuning locale, fichier unique deja connu)                              | **Pas de recon.** Lire le fichier et implementer.                                                                                                                                                                                                                                                                                  |
| Le spec frontmatter liste des `context:`                                                                                        | Les charger : c'est la recon deja faite, ne pas la refaire.                                                                                                                                                                                                                                                                        |

Regle de sortie : une reponse Graphify sans aucune ligne `Assets/RoadRage` est un **echec de
resolution**, jamais une reponse.

## 2. Routage des tests (EditMode / PlayMode)

Ne lance pas les deux par reflexe. Classe selon ce que la story **touche reellement**, pas selon son
libelle.

**EditMode** — logique pure, donnees, calculs, validation, algorithmes, `ScriptableObject` hors
runtime, code independant du cycle de vie Unity.

**PlayMode** — des que la story touche : RPC, host/client, ownership, `NetworkVariable` runtime,
spawn/despawn, scene, prefab runtime, `GameObject`, cycle de vie `MonoBehaviour`, coroutine,
timing/frame, vehicule, physique, collisions, controleurs vehicule, IA dependant du runtime,
interaction entre plusieurs composants.

Le reseau implique PlayMode, mais n'en est **pas** la seule justification. **En cas de doute sur le
classement, PlayMode.**

`Both` seulement si la story contient a la fois de la logique pure testable et du runtime.

**Deux pieges a connaitre :**

- `-TestMode Both` applique le **meme** `-TestFilter` aux deux modes. Un filtre nommant une fixture
  d'un seul mode fait donc echouer l'autre mode sur « aucun test execute » — et c'est voulu (AD-8 :
  une absence de resultat n'est jamais un succes).
- Le filtrage par `testName`/`assembly` **ne fonctionne pas en PlayMode** (le filtrage par
  **categorie**, si — mesures en section 3). En pratique, chemin officiel :
  `-TestMode EditMode -TestFilter …` pour cibler, `-TestMode PlayMode` **sans filtre** pour le
  runtime.
- Les **profils de validation** (section 3, `-Profile`) ne s'appliquent qu'à EditMode. Les combiner
  avec `-TestMode PlayMode`/`Both` est **refuse ferme** par `validate.ps1` : la suite runtime se
  lance complète, sinon ses références de comparaison (section 3, échecs PlayMode connus) ne
  seraient plus valides.

## 3. Checkpoint de verification

L'agent **execute lui-meme** `scripts/validate.ps1` et les commandes `unity cmd` de verification
(AD-4 abrogee le 2026-09-18, voir `AGENTS.md`). Il lit la sortie produite telle quelle et ne la
reformule pas de memoire.

Commande a executer, avec le mode issu du point 2 :

```powershell
.\scripts\validate.ps1 -TestMode EditMode                 # profil Full : suite complète (défaut)
.\scripts\validate.ps1 -TestMode PlayMode                 # suite runtime complète
.\scripts\validate.ps1 -TestMode EditMode -TestFilter "<Namespace>.<Fixture>"   # ciblage manuel
.\scripts\validate.ps1 -Profile Auto                      # selection selon les fichiers modifiés (dev)
.\scripts\validate.ps1 -Profile Geometry                  # preuves 5.49-5.51 + porte A 5.28
.\scripts\validate.ps1 -Profile FullSansGeometry          # officiel d'une story non géométrique
.\scripts\validate.ps1 -Profile Auto -Since <git-ref>     # livraison multi-commit : classe la plage
```

Sans nouveau parametre, la commande se comporte comme avant : profil `Full`, suite EditMode
entiere, aucune exclusion.

### 3.1 Profils de validation EditMode

La suite EditMode est partitionnee en **deux categories NUnit** portees par les fixtures
(`[Category("Core")]` / `[Category("Geometry")]`), pas par des listes tenues a la main :

| Categorie  | Contenu | Cout mesure (2026-09-28) |
| ---------- | ------------------------------------------------------------------------------------ | ------------------------ |
| `Core`     | contrats, invariants rapides, logique pure, validite des artefacts, oracle de trafic | ~10 s (784 tests)        |
| `Geometry` | preuves coûteuses 5.49/5.50/5.51, porte A 5.28, balayages, dégagements, migration   | ~430 s (145 tests)       |

| Profil             | Selection                                                                 | Usage                                                                                                                                   |
| ------------------ | ------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| `Full`             | aucun filtre                                                              | **Seule execution citable comme « validation complète ».** Par defaut, inchange.                                                       |
| `Fast`             | categorie `Core`                                                          | developpement courant. Jamais une acceptation.                                                                                          |
| `FullSansGeometry` | categorie `Core` (meme selection que Fast)                                | validation officielle d'une story **non géométrique**, avec la classification Auto jointe.                                              |
| `Geometry`         | categorie `Geometry`                                                      | itération sur les preuves.                                                                                                             |
| `Auto`             | classifie les fichiers modifiés ; géométrie ou inconnu -> `Full`, sinon `Core` | checkpoint de story. La décision et sa justification sont imprimées dans la sortie.                                                    |

**Regles d'usage — a respecter sans exception :**

- `-Profile Full` reste le seul profil qui vaut « validation complète ». Un profil partiel ne peut
  jamais etre presente comme tel : `validate.ps1` imprime `VALIDATION PARTIELLE` et le nombre exact
  de tests **non executes par selection** (jamais « reussis »). Le recapitulatif distingue
  explicitement : reussis, echoues, non executes par selection (par categorie), et ignores reels
  (`skipped`/`inconclusive` de l'execution).
- **Checkpoints de gate, modifications de contrat, corrections issues d'une code review, livraisons
  importantes : `-Profile Full`.** Ces changements ne prennent pas le raccourci des profils partiels.
- Pour les autres stories : `-Profile Auto` (le checkpoint par defaut du cycle), ou
  `-Profile FullSansGeometry` quand la classification non géométrique est établie et jointe au
  compte rendu. Au moindre doute : `Full`.
- `Fast` est un outil de boucle de developpement ; il n'est jamais cité comme preuve d'acceptation.
- La selection est **conservatrice** : une entree géométrique ou **inconnue** parmi les fichiers
  classés bascule `Auto` sur `Full`. Le mapping des chemins vit dans
  `scripts/validation-profiles.ps1` (source unique, `-SelfTest` pour les simulations de selection) :
  scene `MVP_Run` et ses artefacts, prefabs, pipeline du modèle de route
  (`Features/Vehicles/Traffic/**` **hors `Traffic/Routing/`**, le routage runtime stratégique qui ne
  produit ni courbe ni collider), empreintes véhicule (`VehicleProfileDef*`, `VehicleWheel`),
  `SidewalkDeclarations`, `ProjectSettings/`, manifeste de packages ; une fixture EditMode est
  classée par sa catégorie ; un chemin RoadRage non reconnu ou un fichier de test sans catégorie
  est traité comme géométrique.
- **Portée d'`Auto`** : travail en cours (`git status`, y compris non suivis) et dernier commit
  quand l'arbre est propre. Pour une livraison etalee sur plusieurs commits, passer
  `-Since <git-ref>`. Sans cela, la sortie le dit explicitement.
- **Partition verifiee** : `validate.ps1` compare la selection annoncée au contenu réel
  (`list_tests`) et echoue ferme si un test EditMode n'a ni `Core` ni `Geometry`, ou si l'exécution
  ne correspond pas au compte attendu. La garde `TestSuiteCategoryPartitionTests` le verifie aussi
  dans la suite elle-meme. Une nouvelle fixture lourde doit etre classee `Geometry` explicitement.
- **Gate A** : signée par le propriétaire, jamais regeneree ni signée automatiquement. Le profil
  partiel exécute le controle leger du sign-off (présence + liaison aux artefacts committés, fixture
  5.28, catégorie `Core`) : une signature absente ou détachée des artefacts committés est vue meme
  en `Fast`. La liaison complète (empreintes physiques/sémantiques/clearance, source V1) reste dans
  `Geometry`/`Full`, que `Auto` declenche des qu'une entree géométrique est touchee.
- **PlayMode** : pas de profils, suite complete, et la comparaison aux references applicables reste
  due lors des régressions (les échecs PlayMode connus ne sont jamais masqués).

**Cout mesure (2026-09-28, Editeur 6000.6.0f1, 929 tests EditMode)** : profil `Full` ~430 s de
  temps de test (porte complète ~7-8 min) ; profil `Fast` 784/784 en ~10 s de tests cumulés ;
  profil `Geometry` ~145 tests pour ~430 s. La référence de 72,7 s du 2026-09-18 (523+33 tests)
  est historique : entre-temps la suite a intégré les preuves Traffic V2, qui portent 99 % du coût.
  C'est ce qui justifie la partition — le choix n'est pas un confort, c'est la seule optimisation
  qui ne retire aucune garantie : ce qui n'est pas exécuté est nommé et compte.

**Filtrage en PlayMode (CLI `1.0.0-beta.8` + UTF `1.8.0`)** : `-TestFilter` avec
`-TestFilterType testName` ou `assembly` **ne matche pas en PlayMode** (mesure du 2026-09-18 : nom
de classe, nom de methode complet et nom d'assemblage renvoient « aucun test execute », echec
ferme — alors que la meme forme matche en EditMode). En revanche le filtre par **categorie**
fonctionne en PlayMode (mesure du 2026-09-28 : `--filter Story59 --filter_type category` a execute
le seul test PlayMode de la fixture, 10,4 s, filtre applique `category: Story59`). Le chemin
officiel PlayMode reste neanmoins la suite complete **sans filtre** (comparaison aux references et
aux echecs connus) : le filtrage par categorie sert au ciblage manuel depuis le Test Runner
(bouton « Category ») ou pour une verification directe, jamais a la place de la suite complete.

Le script verifie le CLI Unity, l'Editeur connecte, la recompilation, la Console niveau erreur, les
tests cibles, puis l'etat final scenes/Git. **Il echoue ferme** : Editeur inaccessible, CLI muet,
commande inconnue, timeout, resultat illisible ou absent = echec.

La porte Console (AD-7) est **fenetree sur la validation en cours**. Le script stabilise d'abord
l'Editeur (ni compilation ni rechargement de domaine en cours), capture le curseur Console courant,
puis n'interroge plus que `console --level error --since <curseur>` — une fois apres la
recompilation, une fois apres les tests. Une erreur laissee par un etat intermediaire deja corrige,
ou par une commande CLI rejetee, n'est donc plus comptee : elle appartient a l'historique de la
session d'Editeur, pas a ce checkpoint. Aucune erreur produite pendant la fenetre n'est ignoree, et
rien ne depend de `clear_console` (qui repond `cleared:true` sans vider la memoire tampon du
pipeline : mesure du 2026-09-23).

Un build **encore casse** reste bloquant meme quand son erreur Console est anterieure a la fenetre.
`recompile_status` ne suffit pas : il rapporte la derniere *demande*, et un `recompile` sans
changement l'ecrase (`failed:true` redevient `up_to_date, failed:false`, mesure du 2026-09-23) — un
second passage sur le meme build casse serait passe. Le script lit donc l'etat courant lui-meme,
`EditorUtility.scriptCompilationFailed`, vrai tant que la compilation des scripts est en echec.

Deux consequences a respecter :

- **Une absence de resultat n'est jamais un succes.** Sans sortie du script, la story n'est pas
  verifiee, point final — y compris quand c'est l'agent lui-meme qui l'a lancee.
- **Rapporte la sortie brute**, y compris un echec. Ne l'attenue pas, ne la reformule pas de
  memoire, et ne relance pas silencieusement une commande en echec pour en obtenir une meilleure.

`-Audit` (Project Auditor) reste **hors gate** et optionnel : informatif, jamais bloquant
(ADDON-018). Ne le demande pas par defaut.

## 4. Reviews

Couches **reellement actives**, telles que configurees dans `_bmad/custom/bmad-build.toml`. Une
couche conditionnelle est annoncee au rendu par `Run only when: …` : evalue la condition sur le diff
avant de la lancer. Une `instruction` vide desactiverait une couche — ce n'est plus le cas d'aucune.

| Couche                                              | Activation                                                                                                                                 | Cout                                       |
| --------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------ |
| `blind-hunter` — Risk-Scaled Reviewer               | toujours                                                                                                                                   | faible, proportionnel au blast radius      |
| `edge-case-hunter` — Edge Case Hunter               | conditionnelle : changement **non trivial** (reseau, IA, physique, etat/cycle de vie, contrat partage, refactor cross-file, logique dense) | eleve — d'ou un doute qui la **desactive** |
| `verification-gap` — Verification Gap Reviewer      | conditionnelle : le changement modifie un **comportement observable**                                                                      | faible — d'ou un doute qui l'**active**    |
| `security-review` — Network Trust Boundary Reviewer | conditionnelle : frontiere de confiance reseau (voir ci-dessous)                                                                           | faible                                     |

**Aucune n'est executee pour de la documentation, des commentaires, du formatage, un libelle, une
valeur de tuning isolee ou un changement minuscule et local.**

L'asymetrie des deux regles de doute est deliberee : `edge-case-hunter` produit du volume de
findings, `verification-gap` couvre un risque de preuve. Un doute qui coute une passe courte est
preferable a un doute qui produit du bruit.

La route **one-shot** ne porte que `blind-hunter`. Elle est definie par « zero blast radius », donc
les trois conditions ci-dessus y seraient toujours fausses : les y recopier serait de la
configuration morte et un second exemplaire du meme texte a maintenir. Ne pas les y ajouter sans
changer d'abord la definition de la route.

N'empile pas `code-review` ou `simplify` par-dessus : AD-10 interdit l'empilement de reviewers LLM
sur le meme diff. `ponytail` reste actif comme garde anti-surengineering au moment d'ecrire le code,
pas comme une passe de revue supplementaire.

`intent-alignment` **n'existe pas** dans cette installation BMAD 6.11 — verifie le 2026-09-18 dans
`_bmad/`, les deux copies de skills (`.claude/`, `.agents/`) et tous les snapshots rendus : seules
`blind-hunter`, `edge-case-hunter` et `verification-gap` sont livrees par defaut. Ne pas l'inventer.

**`security-review` est conditionnelle.** Elle se declenche seulement si la story **deplace une
frontiere de confiance reseau** :

- nouvelle RPC, ou RPC existante exposee a un client supplementaire ;
- validation ou usage de `SenderClientId` ;
- autorite host/client, `ownership` ;
- permissions d'ecriture d'une `NetworkVariable` (passage a `Owner`/`Everyone`) ;
- donnees controllables par un client qui atteignent un chemin autoritaire ;
- modification d'une trust boundary reseau.

**Ne la declenche pas** pour une simple modification de valeur reseau, un tuning, un ajout de champ
replique sans changement d'autorite, ou du travail purement local/presentation.

## 5. Outils

N'installe, ne configure et n'invoque **aucun outil, plugin ou serveur MCP** qui n'a pas de ligne
`ADDON-###` dans `docs/setup/addon-adoption-register.md` avec un statut et une decision. Verifier
que l'outil est disponible ne justifie pas de l'utiliser.

En particulier, **ne pas installer** : Serena (`Defer`), RTK (`Defer`), Roslynator, jscpd, Lefthook,
GitHub MCP (`Reject`/`Not Applicable`). Ne pas rouvrir leur benchmark pendant une story : les
declencheurs de reevaluation sont deja ecrits dans le registre.

Usage contextuel, jamais par reflexe :

- **Graphify** — recon, pas passe systematique de debut de story (point 1).
- **Unity MCP** — uniquement quand l'etat reel de l'Editeur est necessaire (scene, prefab,
  Inspector, `GameObject`, Play Mode), et toujours sous la double garde d'etat de `AGENTS.md` AD-6.
- **Blender MCP** — uniquement pour du travail 3D reel.
- **`claude --bare`** — optionnel, hors du cycle de story (gros logs, triage isole). Ne pas
  l'integrer au workflow.

## 6. Exception bornee — decisions de conflit Story 5.50

L'amendement proprietaire `5.50-AUTO-DECISIONS-v1`, approuve le 2026-09-27, autorise le run
`bmad-build` de la Story 5.50 a produire et ecrire toutes les dispositions de paires sans checkpoint
paire par paire. Cette exception est valide seulement si :

- la fonction de classification est deterministe, versionnee et executee sur des entrees epinglees ;
- chaque decision porte sa `PairKey`, sa revision, son empreinte geometrique, son motif, sa preuve,
  le `RoadModelVersion` et les versions exactes du balayage et de la politique ;
- un rejet possede un certificat complet de separation ; toute incertitude devient un conflit
  conservateur accepte ;
- le manifeste d'audit couvre aussi les suivis, faux candidats, revisions remplacees et tombstones ;
- l'ecriture est transactionnelle et une seconde execution ne propose aucun changement ;
- les tests adversariaux et les scenarios de circulation des neuf carrefours satisfont les portes
  de securite, maximalite et progression bornees de l'amendement approuve.

L'agent execute la fonction ; il ne classe aucune paire par jugement LLM. Une geometrie ou une
version differente invalide la revision precedente et impose un nouveau run, jamais un transfert
silencieux. L'ambiguite d'une paire ne provoque pas de HALT. Les HALT sont reserves aux changements
de contrat, aux defauts systemiques non resolus ou a l'absence de politique sure et bornee.

Cette exception ne s'etend a aucune autre story et n'autorise jamais la revue ni la signature de
Gate A, qui restent des actes proprietaire distincts.
