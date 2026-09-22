# Story 5.25 — Road World Model : enregistrements, compilateur et versionnage

## Decision d'assembly : aucun nouvel assembly

Le premier code Traffic V2 vit sous `Assets/RoadRage/Features/Vehicles/Traffic/`, namespace
`RoadRage.Features.Vehicles.Traffic`, dans l'assembly existant `RoadRage.Features.Vehicles`.
Aucun `.asmdef` n'a ete cree ni modifie.

Les trois axes autorises a justifier une frontiere d'assembly ont ete evalues contre le depot :

| Axe | Mesure | Verdict |
| --- | --- | --- |
| Isolation de test | `RoadRage.Tests.EditMode.asmdef` reference deja tous les assemblys de features, dont `RoadRage.Features.Vehicles` ; le banc oracle de la 5.24 doit garder V1 **et** V2 dans la meme portee pour comparer leurs traces | **negatif** — un assembly separe n'isole rien et casserait la comparaison |
| Propriete | le dossier `Features/Vehicles/Traffic/` donne deja la couture de suppression attendue par la 5.48 | **faible** — un dossier suffit |
| Direction de dependance | vrai aujourd'hui (le code 5.25 ne reference rien de `RoadRage.Features.Vehicles`, zero dependance sortante), faux des que l'intention de conduite est composee (5.30/5.31) sauf a deplacer `VehicleDriveIntent` vers `Shared` | **non demontre** — la condition ne tient pas au-dela de cette story |

Aucun axe n'etant demontre, la valeur concrete d'un nouvel assembly n'existe pas aujourd'hui : temps
de compilation supplementaire, fichier de plus a maintenir, et une frontiere qu'il faudrait de toute
facon renegocier a la 5.30.

**Declencheur de reexamen : Story 5.30.** C'est la que l'intention de conduite est composee et que la
direction de dependance reelle entre Traffic V2 et `RoadRage.Features.Vehicles` devient observable
plutot que supposee. Si Traffic doit alors referencer `VehicleDriveIntent`, deux sorties restent
ouvertes — deplacer `VehicleDriveIntent` vers `RoadRage.Shared` et extraire l'assembly, ou garder
l'assembly unique — et le choix se tranche sur la dependance mesuree, pas sur cette note.

## Ce que la story livre

| Fichier | Role |
| --- | --- |
| `Assets/RoadRage/Features/Vehicles/Traffic/RoadModelRecords.cs` | `RoadId` (128 bits opaque, hex minuscule), les onze enregistrements authorables, les enums, le profil de validation et le conteneur `RoadModelSource` |
| `.../RoadModelCanonicalWriter.cs` | serialisation canonique ordre-independante + empreinte SHA-256 tronquee a 128 bits |
| `.../RoadModelValidator.cs` | 12 codes de motif stables, echec dur sur le seul contenu du `RoadModelSource` |
| `.../CompiledRoadModel.cs` | vue immuable, copies defensives, defauts resolus, index et collections inverses, `RoadModelVersion` |
| `.../RoadModelCompiler.cs` | `Compile(source)` : seul emetteur de `RoadModelVersion`, classe statique sans aucun champ non-`const` |
| `Assets/RoadRage/Tests/EditMode/Story525RoadWorldModelTests.cs` | 31 tests couvrant la matrice d'E/S de la spec |

## Contrat d'encodage canonique (partie de `CompilerSchemaVersion = 1`)

Les trois regles ci-dessous sont **semantiquement** la version de schema : les changer rend
incomparables toutes les versions deja emises et impose d'incrementer `CompilerSchemaVersion`.

1. **Ordre.** Chaque ensemble est trie par `RoadId` ordinal avant ecriture, y compris les
   appartenances possedees (mouvements d'un controle, membres d'une zone de conflit, groupes d'un
   plan, etats de groupe d'une phase). Deux sequences gardent leur ordre parce que l'ordre **est** la
   semantique : les echantillons de courbe (abscisse strictement croissante, garantie par le
   validateur) et les phases d'un `SignalPlan`. Reordonner les phases change donc la version — c'est
   teste.
2. **Exclusions.** Libelles, `SourceTrace`, `ImportManifest` (entrees, slots, tombstones, remaps),
   metadonnees editeur, ordre des enregistrements et index reconstructibles ne sont jamais ecrits.
3. **Numerique.** Valeurs finies uniquement ; `-0` normalise en `+0` ; chaque grandeur declare son
   unite, son pas et son arrondi (`MidpointRounding.AwayFromZero`) puis est ecrite comme entier signe.

| Grandeur | Unite | Pas |
| --- | --- | --- |
| longueurs, positions, largeurs | metre | `0.001` |
| vitesses | m/s | `0.001` |
| angles | degre | `0.01` |
| durees | seconde | `0.001` |
| courbure signee | 1/m | `0.00001` |
| composantes de direction unitaire | sans unite | `0.000001` |
| poids et ratios | sans unite | `0.0001` |

Un `NaN` ou un `Infinity` est un echec dur (`NonFiniteNumericValue`), jamais une valeur quantifiee en
silence : le validateur le refuse, et `RoadModelCanonicalWriter.Quantize` le refuse une seconde fois
si jamais il atteignait la charge.

## Codes de motif de validation

`EmptyId`, `DuplicateId`, `UnresolvedReference`, `CrossVersionReference`,
`IncompatibleManifestRemap`, `TombstonedIdReused`, `NonFiniteNumericValue`,
`MovementParentInvalid`, `MovementControlCoverageInvalid`, `SignalizedControlWithoutPlan`,
`ConflictingMovementsGreenTogether`, `InvalidGeometryPayload`,
`ConflictZoneMembershipInvalid`, `NumericValueOutOfRange`.

Chaque echec nomme son identifiant fautif dans `RoadModelValidationIssue.SubjectId`. `Compile` leve
`RoadModelCompilationException` et n'emet alors **aucune** sortie compilee ni version.

Trois codes ne figurent pas dans la liste de la spec et sont justifies ici :

- `InvalidGeometryPayload` couvre le seul invariant structurel dont depend la regle d'ordre 1
  ci-dessus (au moins deux echantillons, abscisse strictement croissante). Ce n'est pas de la
  mathematique de courbe — echantillonnage, projection et bornes restent a la Story 5.26.
- `ConflictZoneMembershipInvalid` ferme un trou trouve en revue : l'appartenance n'etait resolue que
  comme « est un Movement ». Comme `ValidateConflictingGreens` filtre sur
  `zone.JunctionId == plan.JunctionId`, une zone dont le `JunctionId` est mal saisi etait simplement
  ignoree — une phase rendant ses deux membres verts compilait et emettait une version. La zone exige
  desormais au moins deux membres distincts appartenant au carrefour qu'elle declare.
- `NumericValueOutOfRange` ferme le second trou de revue : la conversion en `long` de
  `Quantize` n'est pas verifiee, donc une valeur finie mais enorme saturait en silence et deux
  modeles distincts partageaient une version. La magnitude quantifiee est desormais bornee a `9e18`.

## Choix de modelisation a connaitre

- **`RoadSection` ne porte aucune liste de corridors.** La matrice d'AD-43 mentionne « ordered
  `LaneCorridor` IDs » en colonne *References*, mais la regle de propriete et le critere
  d'acceptation de la story interdisent toute liste reciproque authorable. La cle etrangere
  `LaneCorridor.SectionId` est la seule verite, et `CompiledRoadModel.GetCorridorsInSection` est une
  sortie du compilateur. Un test par reflexion interdit tout champ `RoadId[]` sur un type source hors
  des trois appartenances possedees (`JunctionControl.ControlledMovementIds`,
  `ConflictZone.MemberMovementIds`, `SignalGroup.MemberMovementIds`) et de `ImportManifest.TombstonedIds`.
- **Un seul type d'identifiant.** `RoadId` plutot que onze enveloppes typees : le validateur verifie
  deja le genre attendu de chaque cle etrangere (`ResolveReference` compare le `RoadRecordKind` reel
  au genre attendu), donc onze structs jumelles n'ajouteraient aucune garde.
- **`RoadId` et la serialisation Unity.** Suivant le gabarit `DefinitionId`, `RoadId` est un
  `readonly struct` ; Unity ne serialise pas les champs `readonly`. La Story 5.27 persistera la forme
  hexadecimale via `RoadId.ToString()` / `RoadId.TryParse`, pas la structure telle quelle.
- **`ConflictZone` porte une boite 3D** (`RoadBoundsBox`) et non un polygone : suffisant pour
  versionner le volume revu, et rien dans la 5.25 ne consomme la forme. Un polygone reste ajoutable
  sans changer la propriete, au prix d'une incrementation de `CompilerSchemaVersion`.
- **Le compilateur ne sait rien du passe.** `RoadModelCompiler` est une classe statique sans aucun
  champ non-`const` (verifie par reflexion dans les tests). Lignee, tombstones et remaps sont valides
  *tels que fournis dans le `RoadModelSource`* ; les creer, les persister et reconcilier un import
  contre le precedent appartient a la Story 5.27.

## Verification

- `.\scripts\validate.ps1 -TestMode EditMode -TestFilter "RoadRage.Tests.EditMode.Story525RoadWorldModelTests"` :
  31/31 verts, 0 erreur Console.
- `.\scripts\validate.ps1 -TestMode EditMode` : suite complete verte, aucune regression V1
  (`Story59ParameterizedDriverModelTests` et `TrafficOracleTests` inchanges).
