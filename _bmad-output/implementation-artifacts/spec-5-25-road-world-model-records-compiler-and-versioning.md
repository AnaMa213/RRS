---
title: 'Story 5.25 -- Road World Model : enregistrements, compilateur et versionnage deterministe'
type: 'feature'
created: '2026-09-22'
status: 'done'
review_loop_iteration: 0
baseline_commit: 'cae393d255a4bc57c7b59bc461de8a2ce981b89c'
context:
  - '_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Traffic V2 n'a aucune donnee routiere. `LaneGraph` derive sa topologie et son identite de l'ordre de hierarchie de la scene a chaque `Rebuild()`, donc deux consommateurs peuvent lire deux verites differentes et un deplacement d'objet change silencieusement toutes les references. Geometrie (5.26), migration (5.27), routage (5.29) et grants (5.34) referencent tous un fait routier qui n'existe pas encore.

**Approach:** Livrer les onze enregistrements logiques d'AD-43 comme donnees C# compilables, une identite opaque 128 bits generee (AD-44), un compilateur qui resout les defauts de section et derive tous les index inverses, un serialiseur canonique ordre-independant, un validateur structurel a echec dur, et un `RoadModelVersion` emis par le seul compilateur. Tout est prouve en EditMode sur des modeles synthetiques ecrits a la main.

## Boundaries & Constraints

**Always:** Les cles etrangeres enfants sont la seule verite parent/enfant persistee ; toute collection inverse et tout index sont des sorties du compilateur. `ConflictZone` seule possede l'appartenance aux conflits et les groupes de `SignalPlan` seuls l'appartenance signal. **Chaque `JunctionMovement` est lie a exactement une liaison de controle faisant autorite** : un `JunctionControl` et un seul le couvre, et c'est cette liaison -- jamais l'approche prise globalement, jamais une classification de carrefour -- qui declare son genre (`Uncontrolled`, `Priority`, `Yield`, `Stop`, `Signalized`) et porte la geometrie de ligne d'arret. Un mouvement n'est ni doublement couvert ni laisse sans couverture ; `Uncontrolled` est un choix explicite, pas un repli. Un `JunctionMovement` porte exactement un `JunctionId` parent persiste. La version vient uniquement du compilateur, depuis `CompilerSchemaVersion` plus la charge canonique comportementale ; aucun consommateur ne la recalcule. **`RoadModelCompiler` est sans etat** : il ne connait que le `RoadModelSource` qu'on lui passe, ne se souvient d'aucun import precedent et ne persiste rien entre deux appels. Les identifiants sont opaques, generes une fois, independants du nom, de l'ordre, de l'index, du transform et du `NetworkObjectId`. C# 9.0 sans `record` ni `init` (non supportes par Unity 6).

**Ask First:** Creer un nouvel assembly (voir la decision en Design Notes) ; ajouter une dependance de package ; modifier un fichier V1 retenu.

**Never:** Aucune mathematique de courbe, aucun `Sample`/`Project`/`Bounds`, aucune localisation -- tout cela appartient a la 5.26 et la charge geometrique reste ici un tableau d'echantillons ecrit a la main. Aucun importeur (5.27), aucune UI d'authoring, aucun runtime de trafic, aucun asset Unity cree. **Le compilateur ne tient aucun historique de migration** : creer, persister ou reconcilier une lignee entre deux imports appartient a la 5.27. La 5.25 ne fait que *definir et valider* les donnees de lignee, de tombstone et de remap **fournies dans le `RoadModelSource`**. Ne jamais reproduire `LaneGraph` : indexation par ordre de hierarchie, jointure par proximite `NearestNodeIndex`, `Rebuild()` runtime comme source de verite topologique, identite derivee d'un nom, d'un transform, d'une position de tableau ou d'un `NetworkObjectId`. Aucun fichier sous `Assets/RoadRage` hors des chemins listes en Tasks n'est touche.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Ordre indifferent | deux modeles semantiquement identiques, ordres d'insertion, de collection et de serialisation differents | meme `RoadModelVersion` | test rouge si divergence |
| Changement comportemental | largeur utile, topologie, mouvement, genre de controle, zone de conflit, plan de signal ou portail modifie | la version change | test rouge si inchangee |
| Changement cosmetique | libelle, `SourceTrace`, metadonnee editeur, ordre des enregistrements, index reconstructible | la version ne change pas | test rouge si changee |
| Integrite referentielle | id duplique, id vide, reference non resolue, remap de manifeste incompatible, id tombstone reutilise -- tous fournis dans le `RoadModelSource` | echec dur nommant l'id fautif et un code de motif stable | aucune sortie compilee emise |
| Propriete unique | mouvement couvert par zero ou deux `JunctionControl`, controle `Signalized` sans `SignalPlan` valide, phase rendant verts deux membres d'une meme `ConflictZone` | echec dur avec le code de motif correspondant | aucune sortie compilee emise |
| Valeur numerique non finie | un `NaN` ou un `Infinity` atteint la charge canonique | echec dur, code de motif dedie | aucune version emise ; jamais quantifie silencieusement |

</frozen-after-approval>

## Code Map

- `_bmad-output/planning-artifacts/traffic-v2/ROAD-WORLD-MODEL-AND-RESPONSIBILITY-CONTRACTS.md` -- sections « Road World Model logical data ownership », « Stable identity and model version » et « Junction, right-of-way and signal authoring » : la matrice de propriete a transcrire, pas a re-deriver.
- `Assets/RoadRage/Shared/Definitions/DefinitionId.cs:9` -- gabarit exact de l'identifiant de valeur : `readonly struct` serialisable, egalite ordinale, operateurs. `RoadId` suit ce patron avec une valeur hexadecimale 128 bits au lieu d'un slug.
- `Assets/RoadRage/Features/Vehicles/TrafficSettingsDef.cs:18` -- gabarit `Def` du projet (id minuscule stable, `[CreateAssetMenu]`, jamais mute a l'execution). Reference conventionnelle seulement : la 5.25 ne cree aucun asset.
- `Assets/RoadRage/Features/Vehicles/LaneGraph.cs:22` -- contre-exemple V1, lecture seule : `Rebuild()` plus index d'ordre de collecte. Lire pour savoir ce qu'on ne reproduit pas.
- `Assets/RoadRage/Features/Vehicles/RoadRage.Features.Vehicles.asmdef` -- assembly d'accueil retenu (8 443 lignes, references `RoadRage.Shared` plus Netcode/Input/Cinemachine/TMP). Non modifie.
- `Assets/RoadRage/Tests/EditMode/RoadRage.Tests.EditMode.asmdef` -- reference deja `RoadRage.Features.Vehicles` ; aucune modification d'asmdef necessaire.
- `Assets/RoadRage/Tests/EditMode/TrafficOracle/OracleCatalog.cs` -- ligne `V1-D08` remplacee par cette story (representation par noeuds ponctuels et identite par ordre de hierarchie ne sont pas des contrats V2). Lecture seule ici.
- `Assets/RoadRage/Tests/EditMode/Story59ParameterizedDriverModelTests.cs` -- doit rester vert sans modification : un echec signalerait une fuite de la 5.25 dans la couche de decision.

## Tasks & Acceptance

**Execution:**
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs` -- `RoadId` (128 bits opaque, hex minuscule serialisable), les onze enregistrements authorables (`RoadSection`, `LaneCorridor`, `LaneConnection`, `LaneAdjacency`, `Junction`, `JunctionMovement`, `JunctionControl`, `ConflictZone`, `SignalPlan`, `Portal`, `ImportManifest`/`SourceTrace`), les enums de genre de controle et de role de portail, le profil de validation et le conteneur source `RoadModelSource` -- un seul fichier pour que la matrice de propriete se relise d'un bloc.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCanonicalWriter.cs` -- serialiseur canonique : tri des ensembles par `RoadId` ordinal ; exclusion explicite des libelles, de la provenance, des metadonnees editeur et des index derives ; empreinte SHA-256 tronquee a 128 bits. **Numerique : valeurs finies uniquement** -- `NaN` et `Infinity` sont un echec dur, jamais une valeur encodee ; `-0` est normalise en `+0` avant encodage ; chaque grandeur declare son unite (metres, degres, secondes), son pas de quantification et sa regle d'arrondi, puis est ecrite comme entier signe. **Ces trois choix font partie de `CompilerSchemaVersion`** : les changer impose d'incrementer le schema, puisque toutes les versions deja emises deviennent incomparables.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelValidator.cs` -- echecs durs a codes de motif stables, sur le seul contenu du `RoadModelSource` recu : id duplique ou vide, reference non resolue, reference inter-version, remap de manifeste incompatible, id tombstone reutilise, valeur numerique non finie, mouvement a parent absent ou multiple, mouvement couvert par zero ou deux `JunctionControl`, `Signalized` sans plan valide, membres d'une meme `ConflictZone` verts simultanement.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs` -- vue compilee immuable : copies defensives des enregistrements, defauts de section resolus en valeurs effectives de corridor, collections inverses et index chauds derives, `RoadModelVersion` porte en lecture seule.
- [x] `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCompiler.cs` -- point d'entree unique `Compile(source)` : valide, resout, indexe, emet la version. Seul emetteur de `RoadModelVersion`, et **sans etat** : aucun champ d'instance retenant un import precedent, aucune ecriture sur disque, meme sortie pour la meme entree.
- [x] `Assets/RoadRage/Tests/EditMode/Story525RoadWorldModelTests.cs` -- couvre chaque ligne de la matrice I/O, plus : un modele synthetique complet compile, valide et versionne ; une preuve que chaque collection inverse lue sur le modele compile provient du compilateur et non d'une liste authoree ; une preuve qu'un id tombstone re-soumis echoue ; une preuve que `-0` et `+0` produisent la meme version alors qu'un ecart d'un pas de quantification la change ; une preuve que deux `Compile` successifs d'une meme source donnent la meme version et qu'un `Compile` intercale d'une autre source ne deplace ni l'une ni l'autre.
- [x] `docs/setup/story-5-25-road-world-model-notes.md` -- enregistrer la decision d'assembly avec son raisonnement et l'axe qui l'a justifiee ou le motif d'absence, ainsi que le declencheur de reexamen a la Story 5.30.
- [x] `_bmad-output/implementation-artifacts/sprint-status.yaml` -- passer `5-25-road-world-model-records-compiler-and-versioning` a `in-progress`.
- [x] `graphify update .` -- regenerer le graphe apres les changements de source.

**Acceptance Criteria:**
- Given le premier code Traffic V2 a placer, when la frontiere d'assembly est tranchee, then la decision, son raisonnement et l'axe justificatif -- direction de dependance, propriete ou isolation de test -- sont enregistres dans les notes de la story, et aucun assembly nouveau n'est cree faute de valeur concrete demontree.
- Given un modele compile, when un consommateur lit la parente ou une collection inverse, then aucune liste reciproque authorable n'existe dans les types sources et toute collection inverse est produite par le compilateur.
- Given un modele compile, when sa version est lue, then elle a ete emise par le seul compilateur depuis `CompilerSchemaVersion` plus la charge canonique, et aucun autre type du projet ne la calcule.
- Given une meme source compilee plusieurs fois, dans n'importe quel ordre et entrecoupee d'autres sources, when les versions sont comparees, then elles sont identiques : le compilateur ne porte aucun etat d'un appel a l'autre et ne reconcilie rien contre un import precedent.
- Given un carrefour compile, when le controle d'un mouvement est lu, then il provient d'une liaison `JunctionControl` unique qui couvre ce mouvement et declare elle-meme son genre, et un mouvement non couvert ou couvert deux fois est un echec dur.
- Given la suite EditMode existante, when elle tourne apres cette story, then elle reste verte sans modification, en particulier `Story59ParameterizedDriverModelTests` et `TrafficOracleTests`.

## Spec Change Log

## Design Notes

**Decision d'assembly -- mesuree, pas supposee.** Les trois axes autorises ont ete evalues contre le depot : *isolation de test* est **negative** -- `RoadRage.Tests.EditMode` reference deja tous les assemblys, et le banc oracle de la 5.24 doit garder V1 **et** V2 dans la meme portee pour comparer leurs traces ; *propriete* est faible -- un dossier separe donne deja la couture de suppression de la 5.48 ; *direction de dependance* ne tient que si Traffic ne reference pas `RoadRage.Features.Vehicles`, ce qui reste vrai pour cette story (zero dependance) mais devient faux des que l'intention de conduite est composee (5.30/5.31), sauf a deplacer `VehicleDriveIntent` vers `Shared`. Aucun axe n'est donc **demontre** aujourd'hui : **pas de nouvel assembly**, code sous `Features/Vehicles/Traffic/`, namespace `RoadRage.Features.Vehicles.Traffic`. La 5.30 reexamine contre la direction de dependance reelle.

**Deux formes, une seule par role.** Les enregistrements sources sont des `[Serializable] struct` a champs publics, lisibles par la serialisation Unity quand la 5.27 introduira un conteneur persiste ; `CompiledRoadModel` en prend des copies defensives (tableaux clones compris) et ne les expose qu'en `IReadOnlyList`. Aucun `record` ni `init` : Unity 6 ne les supporte pas.

**Un seul type d'identifiant.** `RoadId` unique plutot que onze enveloppes typees : le validateur verifie deja le genre attendu de chaque cle etrangere, et onze structs jumelles seraient du boilerplate sans garde supplementaire au-dela de ce que le validateur couvre.

**Determinisme numerique.** L'encodage canonique quantifie chaque flottant en entier a pas fixe avant ecriture -- c'est le point qu'a ferme la relecture post-acceptation : sans lui, deux modeles semantiquement identiques pouvaient encore differer par le formatage flottant. Trois precisions rendent la regle executable plutot que declarative : seules des valeurs **finies** entrent dans la charge (`NaN` et `Infinity` sont un echec dur du validateur, jamais une valeur silencieusement quantifiee) ; `-0` est normalise en `+0`, sans quoi deux modeles identiques hasheraient differemment sur un signe invisible ; et unite, pas de quantification et regle d'arrondi sont declares par grandeur. Ces trois choix sont **semantiquement partie de `CompilerSchemaVersion`** : les modifier rend incomparables toutes les versions deja emises, donc impose d'incrementer le schema.

**Le compilateur ne sait rien du passe.** La stabilite d'identite au re-import (AD-44) est une exigence du *pipeline*, pas du compilateur. La 5.25 definit les donnees qui la portent -- lignee `ImportManifest`/`SourceTrace`, tombstones, remaps -- et les valide **telles que fournies dans le `RoadModelSource`**. Les creer, les persister entre deux runs et reconcilier un import contre le precedent appartient a la 5.27. Consequence pratique : `Compile` est une fonction pure de sa source, ce qui rend le determinisme testable sans fixture de persistance.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story525RoadWorldModelTests"` -- fixture ciblee verte.
- `.\scripts\validate.ps1 -TestMode EditMode` -- suite complete verte, aucune regression V1.

## Suggested Review Order

**Le contrat central : une seule source de version**

- Point d'entree unique : valide, resout, indexe, emet. Tout le reste en decoule.
  [`RoadModelCompiler.cs:38`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCompiler.cs#L38)

- Le seul endroit du depot qui construit une `RoadModelVersion`.
  [`RoadModelCompiler.cs:82`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCompiler.cs#L82)

- Ce qui entre dans l'empreinte, champ par champ : c'est la definition executable du mot « comportemental ».
  [`RoadModelCanonicalWriter.cs:108`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCanonicalWriter.cs#L108)

**Determinisme numerique (les gardes ajoutees en revue vivent ici)**

- Quantification : unite, pas, arrondi. Refuse le non-fini, et desormais le fini hors domaine plutot que de saturer.
  [`RoadModelCanonicalWriter.cs:357`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCanonicalWriter.cs#L357)

- Tri par `RoadId` : les collections derivees ne dependent jamais de l'ordre d'insertion.
  [`CompiledRoadModel.cs:450`](../../Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs#L450)

**Propriete des donnees (AD-43) — ou une erreur d'authoring devient un echec dur**

- Appartenance d'une zone de conflit : compte, carrefour, doublons. Correctif de revue P1, le plus consequent.
  [`RoadModelValidator.cs:364`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelValidator.cs#L364)

- Les onze enregistrements et leurs cles etrangeres : la matrice de propriete se relit d'un bloc.
  [`RoadModelRecords.cs:472`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs#L472)

- `RoadId` : 128 bits opaques sur le gabarit `DefinitionId`. Non serialisable par champ — la 5.27 passera par l'hexadecimal.
  [`RoadModelRecords.cs:39`](../../Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs#L39)

- Vue compilee immuable : copies defensives profondes, index derives, version en lecture seule.
  [`CompiledRoadModel.cs:263`](../../Assets/RoadRage/Features/Vehicles/Traffic/CompiledRoadModel.cs#L263)

**Tests — c'est la que la revue a le plus change**

- Une mutation par champ canonique restant : supprimer une ligne du payload fait desormais rougir la suite (P3).
  [`Story525RoadWorldModelTests.cs:576`](../../Assets/RoadRage/Tests/EditMode/Story525RoadWorldModelTests.cs#L576)

- Chaque pas de quantification est porteur : un quart de pas est absorbe, un pas complet change la version (P3).
  [`Story525RoadWorldModelTests.cs:828`](../../Assets/RoadRage/Tests/EditMode/Story525RoadWorldModelTests.cs#L828)

- Le scenario exact du finding P1 : zone detachee de son carrefour + phase verte sur ses deux membres.
  [`Story525RoadWorldModelTests.cs:1160`](../../Assets/RoadRage/Tests/EditMode/Story525RoadWorldModelTests.cs#L1160)

- Ordre indifferent : la propriete que toute la story existe pour garantir.
  [`Story525RoadWorldModelTests.cs:487`](../../Assets/RoadRage/Tests/EditMode/Story525RoadWorldModelTests.cs#L487)

- Non-aliasing, y compris les copies imbriquees de `SignalPlan` et de `ConflictZone` (P5).
  [`Story525RoadWorldModelTests.cs:450`](../../Assets/RoadRage/Tests/EditMode/Story525RoadWorldModelTests.cs#L450)

**Peripheriques**

- Decision d'assembly, contrat d'encodage, table des unites et codes de motif.
  [`story-5-25-road-world-model-notes.md:1`](../../docs/setup/story-5-25-road-world-model-notes.md#L1)
