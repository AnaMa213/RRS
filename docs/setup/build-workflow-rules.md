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
- Le filtrage **ne fonctionne pas en PlayMode** avec la chaine actuelle (mesure en section 3). En
  pratique : `-TestMode EditMode -TestFilter …` pour cibler, `-TestMode PlayMode` **sans filtre**
  pour le runtime.

## 3. Checkpoint de verification

L'agent **execute lui-meme** `scripts/validate.ps1` et les commandes `unity cmd` de verification
(AD-4 abrogee le 2026-09-18, voir `AGENTS.md`). Il lit la sortie produite telle quelle et ne la
reformule pas de memoire.

Commande a executer, avec le mode issu du point 2 :

```powershell
.\scripts\validate.ps1 -TestMode EditMode
.\scripts\validate.ps1 -TestMode PlayMode
.\scripts\validate.ps1 -TestMode EditMode -TestFilter "<Namespace>.<Fixture>"
```

**Cout mesure : 72,7 s pour `-TestMode Both`** (523 tests EditMode + 33 tests PlayMode) sur l'Editeur
connecte avec `recompile_status: up_to_date`, le 2026-09-18. Le cout de la suite complete n'est donc
pas un argument pour la fractionner.

**Limite mesuree du filtrage** (CLI `1.0.0-beta.8` + UTF `1.8.0`, 2026-09-18) : `-TestFilter`
**ne matche pas en PlayMode**, avec `-TestFilterType testName` comme avec `assembly` — nom de classe,
nom de methode complet et nom d'assemblage renvoient tous « aucun test execute » (echec ferme,
exit 1), alors que la meme forme matche correctement en EditMode. En PlayMode, le seul mode
utilisable aujourd'hui est donc la suite complete **sans filtre**. Ne pas ajouter de mecanisme de
filtrage separe tant que ce point n'est pas tranche : le contournement couterait plus cher que la
limite.

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
