# Tutoriel Story 0.5 : Configuration Codex, Claude, Unity MCP et Blender MCP

Ce tutoriel est le chemin manuel pour executer la Story 0.5. Il couvre la selection et la configuration des serveurs MCP Unity et Blender pour les clients Codex, Claude Code et Claude Desktop, ainsi que les smoke tests sans danger et les actions interdites par client. L'agent relit les preuves apres coup ; il ne configure pas les clients a ta place, n'active pas de service payant, ne stocke pas de secret et ne marque aucune ligne `Pass` sans preuve reelle fournie par toi.

**Principe (rappel `mcp-tooling-setup.md`) :** les MCP sont des assistants controles, pas un autopilote. Toute action MCP qui modifie une scene, un prefab, un asset, un package ou un script doit etre relue dans Unity/Blender et commitee en petits pas.

## Sources consultees

- Unity AI MCP -- comment demarrer : https://unity.com/blog/unity-ai-mcp-how-to-get-started
- CoplayDev MCP for Unity : https://github.com/CoplayDev/unity-mcp
- IvanMurzak Unity MCP (optionnel, hors scope premiere configuration) : https://openupm.com/packages/com.ivanmurzak.unity.mcp/
- Blender Lab MCP Server : https://www.blender.org/lab/mcp-server/
- ahujasid Blender MCP (fallback) : https://github.com/ahujasid/blender-mcp
- Brouillon interne deja verifie : `_bmad-output/planning-artifacts/architecture/architecture-RoadRage_Simulator-2026-09-02/mcp-tooling-setup.md`

Les chemins de configuration Codex cites plus bas sont les chemins standards connus au moment de la redaction (`~/.codex/config.toml`, section `[mcp_servers.<nom>]`). Le brouillon source note explicitement que le support Codex doit etre traite comme MCP-compatible mais confirme dans l'environnement Codex actif avant d'etre considere fiable -- ne marque pas la configuration Codex `Pass` sans avoir verifie ce chemin toi-meme dans ton client Codex reel.

## Avant de commencer

- Utilise uniquement les statuts `Not Started`, `In Progress`, `Pass`, `Blocked`, `Not Applicable`.
- Caviarde tout token, API key, credential, secret de licence, Lobby ID ou identifiant de compte avec `[REDACTED_TOKEN]`. Ne colle jamais de secret original dans ce tutoriel, dans une config committee, dans un log ou dans `tooling-validation-log.md`.
- Si un secret atteint le repo, une capture committee ou l'historique Git, arrete la validation, purge la preuve, fais tourner/revoque le secret concerne, puis documente la remediation sans recopier le secret.
- Ne demarre pas plusieurs bridges MCP sur le meme editeur (Unity ou Blender) sauf support multi-instance explicitement documente par le MCP.
- **Demander d'abord** avant : activer un abonnement/beta Unity AI payant, adopter IvanMurzak Unity MCP ou tout autre MCP non liste dans `mcp-tooling-setup.md`, ou faire executer par un MCP un changement de package/version en dehors d'une revue explicite.
- **Jamais :** laisser un MCP modifier scenes, prefabs, scripts, packages ou assets sans revue humaine et commit en petits pas ; stocker un token/cle API/secret dans prompts, config, logs ou fichiers commits ; demarrer plusieurs bridges MCP sur le meme editeur sans support explicite ; marquer `Pass` sans preuve re-verifiable et caviardee ; commencer le gameplay Epic 1 ou l'intake Blender complet (Story 0.6) depuis cette story.

## Etape 1 -- Choisir et installer Unity MCP (Unity Official prefere, fallback CoplayDev)

### Option preferee : Unity Official MCP Server

Utilise cette option en premier si ton compte a acces au beta/trial/abonnement Unity AI tools.

1. Assure-toi que le projet `RRS` utilise Unity `6000.6.0f1` (deja confirme VAL-003).
2. Connecte le projet Unity a Unity Cloud si demande par le flux officiel.
3. Installe le package Unity AI Assistant in-editor si propose par Unity.
4. Dans Unity, ouvre `Edit > Project Settings > AI > Unity MCP`.
5. Confirme que `Unity Bridge` tourne ; clique `Start` si arrete.
6. Dans la section `Integrations`, configure le client AI qui va controler Unity (Codex, Claude Code et/ou Claude Desktop selon disponibilite documentee par Unity).
7. Si l'acces beta/abonnement ou le support client bloque ce chemin, passe a l'option de secours ci-dessous et note la raison du fallback dans le log -- ne marque jamais `Pass` sur l'option Unity Official non utilisee.

### Option de secours : CoplayDev MCP for Unity (`v10.0.0`)

Utilise cette option si Unity Official MCP est indisponible, bloque par abonnement, ou ne configure pas le client voulu.

1. Installe Python 3.10+ et `uv`.
2. Dans Unity, ouvre `Window > Package Manager`.
3. Choisis `Add package from git URL`.
4. Ajoute le package epingle au tag `v10.0.0` (jamais `#main`, pour eviter un changement d'outillage IA inattendu) :

```text
https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#v10.0.0
```

5. Dans Unity, ouvre `Window > MCP for Unity > Configure All Detected Clients`.
6. Redemarre le client MCP et Unity si les outils ne sont pas detectes immediatement.

**Preuve a fournir (VAL-020 -- Selection Unity MCP) :** capture ou note montrant l'option retenue (`Project Settings > AI > Unity MCP` avec `Unity Bridge` actif, ou `Package Manager` montrant `com.coplaydev...#v10.0.0`), sans secret. Tant que cette preuve n'existe pas, VAL-020 reste `Not Started`.

**Cas limite -- Unity Official MCP indisponible :** si le compte n'a pas acces au beta/abonnement Unity AI, arrete/desactive proprement l'option primaire avant de demarrer le fallback -- par exemple, clique `Stop` sur `Unity Bridge` dans `Project Settings > AI > Unity MCP` si elle a deja ete demarree -- avant d'installer et de demarrer CoplayDev, pour ne jamais avoir deux bridges MCP actifs sur le meme editeur Unity. Bascule ensuite sur CoplayDev epingle `v10.0.0`, note la raison exacte du fallback (message d'erreur, absence d'acces) dans `tooling-validation-log.md`, et ne marque jamais `Pass` sur l'option Unity Official non utilisee.

## Etape 2 -- Configurer les clients Codex, Claude Code et Claude Desktop pour Unity MCP

Si tu as utilise `Configure All Detected Clients` (CoplayDev), le menu ecrit automatiquement la configuration pour les clients qu'il detecte sur ta machine (Claude Code, Claude Desktop, Cursor, Windsurf, VS Code Copilot selon disponibilite). Verifie ensuite chaque chemin ci-dessous plutot que de supposer que Codex a ete configure, car Codex n'est pas garanti detecte automatiquement.

Si ce menu configure aussi automatiquement d'autres clients detectes non couverts par ce tutoriel (Cursor, Windsurf, VS Code Copilot), tu dois soit leur appliquer les memes actions autorisees/interdites que Codex/Claude Code/Claude Desktop (Etape 3), soit desactiver/supprimer leur entree MCP si tu ne les utilises pas -- sinon le bridge Unity MCP reste accessible depuis un client non gouverne par ce tutoriel, sans revue ni regle appliquee.

- **Codex :** chemin standard `~/.codex/config.toml` (Windows : `%USERPROFILE%\.codex\config.toml`), section `[mcp_servers.unity]` avec les cles `command` et `args` pointant vers le serveur MCP Unity choisi. Confirme la syntaxe exacte dans ton client Codex actif avant de t'y fier -- ce chemin n'est pas verifie officiellement par Unity ou CoplayDev pour Codex.
- **Claude Code :** enregistre le serveur avec `claude mcp add unity <command> <args...>`, ou verifie l'entree ecrite par `Configure All Detected Clients`. Le registre vit soit dans `.mcp.json` a la racine du projet (portee projet, recommandee ici pour rester relisible dans le repo), soit dans `~/.claude.json` (portee utilisateur), selon la portee choisie a l'enregistrement. Si un futur serveur MCP necessite une authentification (cle API, token), verifie le contenu de `.mcp.json` avant de le committer -- aucune cle API, aucun token et aucun chemin utilisateur sensible ne doit y apparaitre en clair ; applique la meme regle de caviardage `[REDACTED_TOKEN]` qu'a l'Etape 9 si tu dois en documenter un extrait.
- **Claude Desktop :** fichier `%APPDATA%\Claude\claude_desktop_config.json` sous Windows, cle `mcpServers`, entree pour le serveur Unity MCP choisi.

**Preuve a fournir (VAL-020, suite) :** chemin de config exact confirme pour chaque client utilise (Codex/Claude Code/Claude Desktop), sans coller le contenu complet du fichier si celui-ci contient un chemin utilisateur sensible non pertinent -- resume suffisant.

**Cas limite -- client non detecte :** si `Configure All Detected Clients` ne detecte pas Codex ou un client Claude installe, configure-le manuellement au chemin ci-dessus et note la methode manuelle utilisee.

## Etape 3 -- Actions autorisees et interdites par client (Unity MCP)

| Client | Actions autorisees | Actions interdites |
| --- | --- | --- |
| Codex | Inspecter hierarchie de scene, GameObjects, valeurs de composants, console ; proposer des edits de script pour revue humaine ; creer/supprimer un GameObject de smoke test clairement nomme puis nettoyage manuel. | Modifier scene/prefab/script/package/asset sans revue humaine et commit en petits pas ; changer une version de package sans note ; ajouter un service Unity AI payant sans approbation ; stocker un secret dans prompt/config/log/commit ; demarrer un second bridge MCP sur le meme editeur Unity. |
| Claude Code | Memes actions autorisees que Codex : inspection lecture seule, proposition d'edits de script relus, smoke test GameObject temporaire nettoye manuellement. | Memes interdictions que Codex, plus : convertir le projet vers des dedicated servers ; adopter IvanMurzak ou un MCP non liste sans validation "demander d'abord". |
| Claude Desktop | Inspection lecture seule de la scene/hierarchie/console via le bridge Unity MCP configure ; proposition d'edits relus ; smoke test GameObject temporaire nettoye manuellement. | Memes interdictions que Codex et Claude Code ; ne jamais laisser Claude Desktop appliquer un changement de scene/prefab/package sans que le meme changement soit relu et commite depuis l'environnement de dev habituel. |

Ces regles s'appliquent quel que soit le MCP Unity choisi (Unity Official ou CoplayDev).

## Etape 4 -- Smoke test Unity MCP sans danger

1. Depuis le client configure (Codex, Claude Code ou Claude Desktop), demande d'abord une inspection en lecture seule de la scene courante (hierarchie, composants visibles).
2. Demande ensuite une action de smoke test sans risque :
   - Unity Official MCP : creer un GameObject vide nomme `MCP_SmokeTest`.
   - CoplayDev MCP : creer un cube a l'origine et lui ajouter un `Rigidbody`.
3. Verifie le resultat dans l'Unity Editor avant de garder ou supprimer l'objet ; supprime-le manuellement apres verification si tu ne veux pas le garder.
4. Confirme qu'aucune autre scene, prefab, script, package ou asset n'a ete modifie pendant ce test.

**Preuve a fournir (VAL-021 -- Smoke test Unity MCP) :** capture ou note Unity Editor montrant l'objet de smoke test cree (nom, hierarchie) et sa suppression/conservation manuelle apres revue, sans secret. Tant que cette preuve n'existe pas, VAL-021 reste `Not Started`.

**Cas limite -- le MCP tente d'aller au-dela du smoke test :** si le client MCP propose ou applique un changement de scene/prefab/script/package non demande, rejette le changement, ne le commite pas, note l'incident (quel client, quelle action) dans `tooling-validation-log.md`, et ne marque jamais `Pass` sur ce smoke test.

## Etape 5 -- Installer Blender `5.2 LTS` (VAL-022)

1. Installe Blender `5.2 LTS` depuis la source officielle.
2. Verifie la version via `Help > About Blender` dans l'interface, ou en option `blender --version` en ligne de commande.

**Preuve a fournir (VAL-022 -- Installation Blender) :** capture ou sortie commande montrant la version Blender installee.

**Cas limite -- version exacte indisponible :** si `5.2 LTS` n'est pas disponible, marque VAL-022 `Blocked`, note la source consultee, la version disponible, l'impact, et demande une approbation avant tout fallback de version (regle deja posee dans `tooling-validation-log.md`).

## Etape 6 -- Choisir et configurer Blender MCP (Blender Lab prefere, fallback ahujasid)

### Option preferee : Blender Lab MCP Server

Utilise cette option en premier si l'extension officielle Blender Lab MCP est disponible et stable dans ta version Blender installee.

1. Installe `uv`.
2. Ouvre https://www.blender.org/lab/mcp-server/ et installe l'extension MCP Blender Lab (drag-and-drop dans Blender ou telechargement puis installation depuis disque).
3. Dans Blender, ouvre `Edit > Preferences > Add-ons`, recherche `mcp`, active/configure l'extension.
4. Garde l'hote/port par defaut sauf besoin contraire : `localhost:9876`.
5. Demarre le serveur MCP depuis les parametres de l'add-on.
6. Configure le client AI (Codex, Claude Code ou Claude Desktop) comme serveur MCP local stdio, en suivant la commande documentee par Blender Lab.

### Option de secours : ahujasid Blender MCP

Utilise cette option si Blender Lab est trop instable, indisponible, ou incompatible avec le client AI choisi.

1. Installe Blender 3.0+ et Python 3.10+ (`5.2 LTS` reste la cible projet).
2. Installe `uv` sous Windows :

```powershell
powershell -c "irm https://astral.sh/uv/install.ps1 | iex"
```

3. Assure-toi que `uvx` est sur le PATH. Si un client GUI ne le trouve pas, utilise le chemin complet ou passe par `cmd /c`.
4. Configure le client :
   - **Claude Desktop** -- ajoute au fichier `%APPDATA%\Claude\claude_desktop_config.json` :

```json
{
  "mcpServers": {
    "blender": {
      "command": "uvx",
      "args": ["blender-mcp"]
    }
  }
}
```

   - **Claude Code** -- enregistre via :

```powershell
claude mcp add blender uvx blender-mcp
```

   - **Codex** -- ajoute une section `[mcp_servers.blender]` dans `~/.codex/config.toml` (Windows : `%USERPROFILE%\.codex\config.toml`) avec `command = "uvx"` et `args = ["blender-mcp"]` ; confirme la syntaxe exacte dans ton client Codex actif avant de t'y fier.
5. Installe l'add-on Blender :

```powershell
uvx blender-mcp install-addon
```

6. Dans Blender, ouvre `Edit > Preferences > Add-ons` et active `Interface: MCP for Blender`.
7. Dans le viewport 3D, appuie sur `N`, ouvre l'onglet MCP, et demarre le serveur MCP.

**Preuve a fournir (VAL-023 -- Selection Blender MCP) :** capture ou note montrant l'option retenue (extension Blender Lab active + serveur demarre, ou add-on ahujasid actif + config client), et le chemin de config exact confirme pour chaque client utilise (Codex/Claude Code/Claude Desktop), sans secret. Tant que cette preuve n'existe pas, VAL-023 reste `Not Started`.

**Cas limite -- Blender Lab instable/indisponible :** arrete proprement le serveur Blender Lab MCP (bouton stop dans les parametres de l'add-on) avant d'installer et de demarrer le fallback ahujasid, pour ne jamais avoir deux serveurs Blender MCP actifs sur le meme editeur Blender. Bascule ensuite sur le fallback ahujasid, note la raison exacte du fallback dans `tooling-validation-log.md`, et ne marque jamais `Pass` sur l'option non utilisee. Ne demarre qu'une seule instance de serveur Blender MCP a la fois.

## Etape 7 -- Actions autorisees et interdites par client (Blender MCP)

| Client | Actions autorisees | Actions interdites |
| --- | --- | --- |
| Codex | Inspecter une scene Blender vide ou existante ; creer un cube prop simple ; assigner un materiau ; exporter un `.glb` de test dans un dossier de scratch/test. | Traiter un mesh genere comme final avant passage par l'intake Blender complet (Story 0.6) ; demarrer une deuxieme instance de serveur Blender MCP ; installer un addon/service supplementaire sans revue ; stocker un secret. |
| Claude Code | Memes actions autorisees que Codex. | Memes interdictions que Codex, plus : sauter le nettoyage Blender (renommage, transforms appliquees, echelle, normals) avant tout import Unity. |
| Claude Desktop | Memes actions autorisees que Codex, via le serveur MCP local configure. | Memes interdictions que Codex et Claude Code ; ne jamais laisser Claude Desktop exporter directement un asset de production sans revue dans Blender puis Unity. |

## Etape 8 -- Smoke test Blender MCP sans danger

1. Depuis le client configure, demande d'abord une inspection de la scene Blender vide (lecture seule).
2. Demande la creation d'un cube prop simple.
3. Demande l'assignation d'un materiau simple au cube.
4. Demande l'export d'un fichier `.glb` de test dans un dossier de scratch/test (pas dans un dossier d'assets de production commite).
5. Confirme qu'aucune autre scene, objet, materiau ou fichier n'a ete modifie pendant ce test.
6. Traite ce mesh comme un brouillon (`draft`) tant qu'il n'est pas passe par l'intake Blender complet de la Story 0.6 -- ne le convertis pas en prefab Unity depuis cette story.

**Preuve a fournir (VAL-024 -- Smoke test Blender MCP) :** capture ou note Blender montrant le cube cree, le materiau assigne, et la confirmation d'export `.glb` du fichier de test, sans secret. Tant que cette preuve n'existe pas, VAL-024 reste `Not Started`.

**Cas limite -- l'add-on ou l'export echoue :** si l'extension Blender Lab ou l'add-on ahujasid plante, ou si l'export `.glb` echoue de maniere repetee, marque VAL-024 `Blocked`, note l'erreur exacte et l'impact.

## Etape 9 -- Rappel de la regle de caviardage

Reutilise la regle de caviardage `[REDACTED_TOKEN]` de la Story 0.3 pour toute cle API MCP, token, credential ou identifiant de compte visible dans une capture, un fichier de config ou une note. Ne colle jamais le secret original dans ce tutoriel, dans `tooling-validation-log.md`, ni dans un fichier committe. Si un fichier de config MCP contenant un secret venait a etre committe par erreur, arrete-toi, purge la preuve, fais tourner/revoque le secret concerne, puis documente la remediation sans recopier le secret.

## Etape 10 -- Mettre a jour les documents de suivi

Apres chaque vraie action manuelle ou chaque bloqueur, mets a jour `docs/setup/tooling-validation-log.md` :

| ID | Statut avant preuve reelle | Preuve attendue |
| --- | --- | --- |
| VAL-020 | `Not Started` ou `In Progress` | Unity Official MCP configure si disponible (Project Settings > AI > Unity MCP, Bridge actif) sinon CoplayDev epingle `v10.0.0` (Package Manager), plus chemin de config confirme pour Codex/Claude Code/Claude Desktop, sans secret. |
| VAL-021 | `Not Started` ou `In Progress` | Smoke test Unity MCP sans danger (`MCP_SmokeTest` ou cube+Rigidbody) cree, revu, nettoye manuellement, aucune autre modification scene/prefab/script/package. |
| VAL-022 | `Not Started` ou `In Progress`/`Blocked` | Version Blender `5.2 LTS` confirmee, ou ligne `Blocked` avec source, version disponible et demande d'approbation fallback. |
| VAL-023 | `Not Started` ou `In Progress` | Blender Lab MCP configure si stable, sinon fallback ahujasid, plus chemin de config confirme pour Codex/Claude Code/Claude Desktop, sans secret. |
| VAL-024 | `Not Started` ou `In Progress` | Scene vide inspectee, cube prop cree, materiau assigne, export `.glb` de test realise ; mesh traite comme brouillon avant intake Story 0.6. |

Apres tout changement de `VAL-020` a `VAL-024`, mets aussi a jour `docs/setup/epic-0-readiness-checklist.md` (lignes Story 0.5, action manuelle et validation agent) et `_bmad-output/implementation-artifacts/sprint-status.yaml`.

Une ligne passe en `Pass` seulement si date, acteur, commande/chemin UI, chemin/resume de preuve caviardee, validateur et resultat sont tous presents et re-verifiables.

## Arret obligatoire avant Story 0.6

Arrete-toi ici apres preparation des preuves et notes Story 0.5. Ne commence pas Story 0.6 (Blender et intake asset 3D complet), Story 0.7 (registre adoption add-on), le lobby UI gameplay, ou Epic 1 depuis cette story.
