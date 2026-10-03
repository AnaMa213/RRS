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

La story porte `[Category("Story<epic><story>")]` sur chaque fixture creee ou modifiee :
5.31 -> `Story531`, 5.9 -> `Story59`. En EditMode, conserver aussi `Core` ou
`Geometry` ; la garde `TestSuiteCategoryPartitionTests` reste obligatoire. Une campagne
`[Explicit]` conserve sa categorie dediee (par exemple `Story531Campaign`) et ne porte
pas la categorie de story.

Choisir les modes selon les tests de la story. **EditMode** couvre logique pure, donnees,
calculs et validation hors runtime. **PlayMode** couvre notamment RPC, ownership, scene,
prefab runtime, `GameObject`, cycle de vie, coroutine, vehicule, physique et IA runtime.
En cas de doute sur le classement, PlayMode. `Both` convient si la story a des fixtures
dans les deux modes ; un mode demande sans test correspondant est un echec (AD-8).

Le filtre par categorie fonctionne en PlayMode. Les filtres `testName` et `assembly`
n'y fonctionnent pas (mesure du 2026-09-18). Le CLI compare la categorie par egalite :
`Story531` n'inclut pas `Story531Campaign`. Avec `include_explicit=false` (defaut),
son pipeline exclut les tests et fixtures `[Explicit]` avant le filtrage.
`list_tests` rapporte pourtant `Explicit=false` pour les campagnes PlayMode
marquees au niveau fixture : le profil Story controle aussi leur source et
echoue ferme si une fixture explicite porte la categorie ciblee.

## 3. Checkpoint de verification

L'agent execute lui-meme `scripts/validate.ps1` (AD-4 abrogee le 2026-09-18).
Pendant la boucle de developpement **et** au checkpoint de la story, lancer seulement
les tests de ses fixtures creees ou modifiees :

```powershell
.\scripts\validate.ps1 -Profile Story -Story 5.31 -TestMode EditMode
.\scripts\validate.ps1 -Profile Story -Story 5.31 -TestMode PlayMode
.\scripts\validate.ps1 -Profile Story -Story 5.31 -TestMode Both
```

Choisir uniquement les modes ou la story a des tests. `-Story` exige `X.Y` et
`-Profile Story` ; `-TestFilter` est incompatible. Le recapitulatif porte
`VALIDATION STORY`, les nombres executes et l'absence des suites completes.
Les campagnes `[Explicit]` suivent leur procedure dediee si le spec les exige ;
elles ne font pas partie du profil Story.

Sans nouveau parametre, la commande reste `Full` EditMode, sans exclusion.
Les suites completes EditMode et PlayMode sont reservees a la **fin d'epic**, y compris
pour les gates, contrats, corrections de revue et livraisons importantes.
**Risque accepte (decision proprietaire du 2026-09-29) : une regression dans les tests
d'une autre story n'est detectee qu'en fin d'epic.**

### 3.1 Profils et partition EditMode

La suite EditMode reste partitionnee par `[Category("Core")]` et
`[Category("Geometry")]`. `Story` ajoute une categorie, sans remplacer cette
partition. `list_tests` est interroge apres recompilation et stabilisation ;
un compte execute different du compte attendu ou une partition incomplete echoue
ferme. `TestSuiteCategoryPartitionTests` verifie aussi la partition.

| Profil | Selection | Usage |
| --- | --- | --- |
| `Story` | categorie exacte `Story<epic><story>` | boucle et checkpoint de story, EditMode, PlayMode ou Both ; jamais validation complete |
| `Full` | aucun filtre | suite complete EditMode en fin d'epic ; defaut inchange |
| `Fast` | `Core` | diagnostic EditMode, jamais acceptation complete |
| `FullSansGeometry` | `Core` | diagnostic EditMode avec classification Auto jointe |
| `Geometry` | `Geometry` | diagnostic des preuves geometriques |
| `Auto` | fichiers modifies : geometrie ou inconnu -> `Full`, sinon `Core` | diagnostic de selection |

`Auto` reste conservateur. Le mapping des chemins vit dans
`scripts/validation-profiles.ps1` et son `-SelfTest`. Il classe la scene
`MVP_Run`, les prefabs, le pipeline Traffic V2 hors `Traffic/Routing/`, les
empreintes vehicule, `SidewalkDeclarations`, `ProjectSettings/` et les packages
comme sensibles ; une fixture EditMode est classee par son contenu. `Auto`
considere le travail en cours et le dernier commit si l'arbre est propre ;
`-Since <git-ref>` couvre une livraison multi-commit.

Un profil partiel indique les tests non executes par selection, jamais comme
reussis. La Gate A signee n'est jamais regeneree ni signee automatiquement.

### 3.2 Procedure de fin d'epic

La story checkpoint `X.N` execute cette procedure et est la seule a promouvoir
le `sprint-status` en `done`. Sans story checkpoint, l'executer avant
`bmad-retrospective`.

1. Verifier que l'Editeur repond (`unity status` = `ready`). Aucun redemarrage systematique : voir 3.3.
2. Lancer `.\scripts\validate.ps1 -Profile Full`, puis
   `.\scripts\validate.ps1 -TestMode PlayMode`, une seule fois chacun.
3. Comparer PlayMode a
   `_bmad-output/implementation-artifacts/v1-regression-5-51/after-playmode-results.json`
   (6 echecs connus) ; aucun nouvel echec n'est admis.
4. Consigner les sorties brutes ; la retrospective les cite.

### 3.3 Domain Reload et runs PlayMode successifs (decision proprietaire du 2026-10-03)

- Le projet garde le **Domain Reload actif** a l'entree en Play Mode
  (`ProjectSettings/EditorSettings.asset` : `m_EnterPlayModeOptionsEnabled: 1`, `m_EnterPlayModeOptions: 2`, soit le
  rechargement de scene desactive et le rechargement de domaine actif). C'est un choix intentionnel : les runs PlayMode
  se relancent l'un apres l'autre sans redemarrer l'Editeur.
- **Ne pas demander de redemarrage de l'Editeur entre deux runs PlayMode.** Un redemarrage ne se demande que si
  l'Editeur ou le Test Runner est reellement bloque ou corrompu : `unity status` qui ne revient pas a `ready`, un run
  qui rend `total: 0` sans raison, ou un etat `running` qui ne se termine jamais.
- Pendant un rechargement de domaine, le CLI ne repond pas un court instant (sortie vide ou code de sortie 6).
  `validate.ps1` le tolere pour `test_status` comme pour les requetes Console :
  - lecture retentee 5 fois au plus, a 2 s d'intervalle, puis surveillance normale ;
  - tout autre code de sortie, une sortie illisible, un refus de l'Editeur ou un statut de test `failed` ou `error`
    restent bloquants immediatement ;
  - essais epuises : echec ferme avec diagnostic.
- La politique vit dans `scripts/validation-cli.ps1`, couverte par son `-SelfTest`.
- Un resultat lu ailleurs (journal de l'Editeur, Console) ne vaut jamais preuve si `validate.ps1` n'a pas recupere le
  statut officiel.

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

Cette exception ne s'etend a aucune autre story que 5.52 et n'autorise jamais la revue ni la signature de
Gate A, qui restent des actes proprietaire distincts.

2026-09-29 (`sprint-change-proposal-2026-09-29.md`) : la meme exception, sous toutes ses conditions,
s'applique a la Story 5.52 pour la reevaluation de toutes les paires dont l'enveloppe gonflee change ;
elle n'autorise ni la revue ni la signature de Gate A.
