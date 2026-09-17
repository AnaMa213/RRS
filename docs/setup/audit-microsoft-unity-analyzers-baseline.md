# Baseline Microsoft.Unity.Analyzers (ADDON-019)

Mesure du pilote P2, 2026-09-17, apres la baseline Project Auditor (ADDON-018).

## Constat de depart

Le `.csproj` genere contenait deja `<Analyzer Include=".../Visual Studio Tools for Unity/Analyzers/Microsoft.Unity.Analyzers.dll" />`
avant ce pilote. **Cette ligne est un artefact IDE (VS Tools for Unity), pas une entree lue par
la compilation reelle d'Unity** : verifie apres integration, le `.csproj` n'a jamais reference
la copie projet installee ici, et pourtant l'analyzer tournait bien (voir sonde ci-dessous).
Unity 6 charge les analyzers Roslyn via le label d'asset `RoslynAnalyzer`, jamais via les
`.csproj` regeneres (AD-14).

## Integration (sans toucher les `.csproj`)

1. DLL recuperee localement depuis l'installation Visual Studio Tools for Unity deja presente
   (`.../Visual Studio Tools for Unity/Analyzers/Microsoft.Unity.Analyzers.dll`), copiee dans
   `Assets/Editor/Analyzers/Microsoft.Unity.Analyzers.dll`.
2. Import configure via l'API Unity (`unity cmd eval_file`, jamais d'edition manuelle de `.meta`
   a la main) : `PluginImporter.SetCompatibleWithAnyPlatform(false)` (exclue de tout build
   joueur), label `RoslynAnalyzer` pose via `AssetDatabase.SetLabels`.
3. `.editorconfig` cree a la racine (voir plus bas).

## Catalogue reel du DLL (lu par reflexion, pas de memoire/web)

43 regles au total (`UNT0001` a `UNT0043`), enumerees directement depuis l'assembly chargee :

- **3 en `Warning` par defaut** : `UNT0006` (signature de message incorrecte), `UNT0033` (casse de
  message incorrecte), `UNT0015` (methode attendue statique/sans parametre).
- **40 en `Info` par defaut** (dont 2 desactivees : `UNT0021`, `UNT0005`) — **invisibles dans la
  Console Unity a n'importe quel `--level`**, confirme par mesure (`console --level info` vide
  apres un premier scan alors que l'analyzer tournait deja). C'est un comportement Unity/Roslyn
  standard (seuls Warning/Error remontent au build log), pas une panne de l'analyzer.

## Verification que l'analyzer tourne reellement (sonde jetable)

Avant de conclure quoi que ce soit sur un resultat "zero diagnostic", un fichier sonde temporaire
(`Assets/_AnalyzerProbe_DELETE_ME.cs`, supprime immediatement apres verification) avec
`void start()` (casse incorrecte) a ete recompile : **`UNT0033` est bien remonte dans la Console**
avec le message et le lien de documentation attendus. Confirme : le pipeline fonctionne de bout
en bout, un resultat "zero" sur le code reel est un vrai negatif, pas un defaut de plomberie.

## `.editorconfig` : 5 regles en `warning`, 38 en `silent`

Choix motive par regle, pas par defaut mecanique :

| Regle | Raison |
| --- | --- |
| `UNT0006`, `UNT0033`, `UNT0015` | Deja `Warning` par defaut ; bugs reels de signature/nommage de message Unity (methode jamais appelee silencieusement) |
| `UNT0007` | Promue depuis Info. Null-coalescing sur un `UnityEngine.Object` (l'operateur `==` surcharge rend un objet detruit "non null" pour `??`) -- footgun specifique a Unity, pertinent ici vu le churn de cycle de vie `NetworkObject` (spawn/despawn) dans ce projet |
| `UNT0004` | Promue depuis Info. `Time.fixedDeltaTime` utilise dans `Update` -- jeu de conduite/physique, erreur de timestep reelle |

Les 38 autres regles restent explicitement `silent` (pas seulement "laissees au defaut") pour ne
pas faire remonter de bruit cote IDE (VS/Rider affichent les suggestions Info meme quand la
Console Unity les ignore).

## Mesure sur le code reel (104 fichiers de production + tests, recompile complete)

**0 nouveau diagnostic `UNT####`** sur l'ensemble du projet apres promotion des 5 regles.
Le seul `UNT0033` apparu dans les logs venait de la sonde jetable, deja supprimee au moment de
la mesure -- confirme comme une entree perimee du buffer Console (le fichier n'existait plus
sur disque ni dans `git status`), pas un faux positif reel.

**Criteres de rollback (definis avant mesure)** : plus de 10 warnings non actionnables au premier
passage, ou plus d'un faux positif par story. **Aucun des deux n'est atteint** -- il n'y a
simplement aucun diagnostic, ni bruit ni signal, sur le code actuel.

## Decision (ADDON-019)

**`Adopt`.** Justification : cout d'integration nul apres mise en place (aucune maintenance,
aucune edition de `.csproj`, `.editorconfig` fige), zero bruit mesure (0 faux positif, largement
sous le seuil de rollback), mecanisme prouve fonctionnel par une sonde reelle. La valeur n'est
**pas corrective aujourd'hui** (rien trouve sur le code existant) mais **preventive** : les 5
regles actives couvrent une classe de bugs Unity reels (message jamais appele, comparaison
`==` surchargee sur objet detruit) que ni le compilateur C#, ni les tests, ni Project Auditor
(ADDON-018, perimetre different) ne couvrent. Cout de maintien quasi nul, rien a perdre a le
garder actif ; se reevaluera naturellement si un futur pilote constate du bruit reel.

**Cout de rollback** : retirer `Assets/Editor/Analyzers/Microsoft.Unity.Analyzers.dll` (et son
`.meta`) et `.editorconfig` ; aucun autre fichier projet modifie.

**Declencheur de reevaluation** : plus de 10 warnings non actionnables constates sur un futur
passage, ou plus d'un faux positif par story -- comme prevu au depart.
