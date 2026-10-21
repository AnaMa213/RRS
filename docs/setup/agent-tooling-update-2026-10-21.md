# Mise a jour de l'outillage agents — 21 octobre 2026

Maintenance demandee explicitement par Kenan pour les plugins, skills et outils utilises dans RRS par Claude, ChatGPT/Codex et DeepSeek. Les mises a jour BMAD, Unity et Graphify sont versionnees dans des commits distincts, avec le registre et ce bilan dans un commit de documentation.

| Element | Avant | Apres / resultat |
| --- | --- | --- |
| BMad Method, modules core et bmm | 6.11.0 selon le manifeste installe | **6.12.4**, version stable npm au moment du controle ; 50 skills par client Claude/Codex, compatibilites historiques conservees |
| Graphify | 0.9.58 | **0.9.93**, via `uv tool upgrade graphifyy` |
| Skills Graphify Claude, Codex et Copilot | 0.9.58 | **0.9.93**, references associees actualisees, hooks existants conserves |
| Plugin Unity de Claude | 0.1.2-beta | **0.1.8-beta** |
| Plugin Unity de ChatGPT/Codex | 0.1.8-beta | Deja a jour ; marketplace existant rafraichi |
| Plugin Ponytail de Claude | 4.9.0 | **5.3.2** |
| Plugin Ponytail de ChatGPT/Codex | 5.3.2 | Deja a jour ; marketplace existant rafraichi |
| Skills Unity dans `.agents/skills/` | 27 copies suivies dans `skills-lock.json` | **26 skills actuels** ; 25 actualisations et remplacement des deux anciens skills tilemap par `create-tile-palette-or-rule-tiles` |
| Skills CLI utilise pour la maintenance | Cache 1.5.26 | **1.7.4**, executions npx epinglees, pas d'installation globale |
| Skill global `find-skills` | Depot vercel-labs/skills | Rafraichi ; empreinte upstream identique, donc contenu deja a jour |
| DeepSeek V4 for Copilot Chat | 0.9.4 | **Deja a jour**, confirme par le Marketplace VS Code et le package.json upstream |
| RTK | 0.49.0, installation systeme | **Mise a jour bloquee par les droits Windows** ; v1.0.1 OSS telechargee, SHA-256 verifie, executable demarre avec succes ; le binaire actif reste en 0.49.0 |

Les plugins integres OpenAI sont distribues par l'application et son runtime ; ils ont ete inventories, sans remplacement manuel de leurs caches. Les connexions sans rapport avec RRS (Notion, Sites, etc.) et les plugins d'autres projets ne sont pas modifies. Les applications clientes et extensions Claude/ChatGPT elles-memes ne font pas partie de cette mise a jour des plugins/skills.

## Compatibilite et conservation des decisions RRS

- BMad reste sur la branche stable 6.12. La preview v7 n'est pas installee : sa documentation annonce des changements pouvant casser une installation existante.
- `_bmad/custom/bmad-build.toml` est adapte au contrat de revue 6.12.4 : `{diff_output}` devient une lecture du fichier `{diff_file}`, et la revue edge-case recoit `{claims_file}` pour sa phase de verification des affirmations. Les conditions de declenchement, les quatre couches de revue, les regles de verification et `persistent_facts` restent conserves.
- La resolution des configurations retrouve Kenan, la communication en francais, les documents en francais et `docs/setup/build-workflow-rules.md`. Le rendu du workflow `bmad-build` reussit pour les deux installations Claude/Codex et produit le meme artefact.
- Le CLI Unity reste inchange : sa montee de version demande une revalidation separee selon ADDON-013. Le nouveau skill upstream recommande lui-meme de respecter une version imposee ; les regles RRS restent prioritaires sur ses recettes generiques de verification.
- Blender MCP reste sur la reference `v1.0.0` epinglee par ADDON-016. Aucun package Unity, aucune scene, aucun prefab ni fichier de `Assets/` n'est modifie.
- Les fichiers AGENTS.md, CLAUDE.md, `.graphifyignore` et les hooks projet restent identiques. Les trois fichiers BMAD deja modifies au debut de la maintenance ont conserve exactement leur SHA-256.

## Verification et provenance

- Installation BMad executee avec `npx --yes bmad-method@6.12.4 install --directory D:/Projets/RRS --action quick-update --yes` ; manifeste final et rendu des customisations controles.
- Plugins Claude actualises par leur gestionnaire officiel, qui confirme les versions finales et demande un redemarrage. Les marketplaces Git de Codex ne signalent aucune erreur ni nouvelle version.
- Skills Unity copies avec `skills@1.7.4`, en ciblant uniquement Codex et le repertoire universel `.agents/skills/`, sans ajouter de copies Unity sous `.claude/skills/` (Claude utilise son plugin).
- Source Unity skills : commit `9d282100540601cfddc4bbd69f36a239d7a6f55c`. Source Unity plugin : `421acb1d365cbab6506b7d77ce09a71602edbce1`. Ponytail : `09c434ab73ca43761c5d8b0c5d341926c2fba710`.
- Le retrait des anciens skills tilemap par Skills CLI a annonce un succes mais termine avec une assertion Node/libuv Windows et laisse les dossiers/entrees presents. La migration a donc ete finalisee par deplacement des deux dossiers en sauvegarde et retrait de leurs seules entrees du lock ; etat final controle : 26 entrees et le nouveau skill present.
- `graphify update .` execute sans LLM : **4 617 noeuds, 12 589 liens, 186 communautes**. Le manifeste conserve 190 fichiers applicatifs et l'unique note memoire historique deja connue ; aucun nouveau chemin hors corpus applicatif. `.graphifyignore` reste inchange.
- Aucun test Unity requis : seuls l'outillage agent et son graphe changent. Aucun Editeur ni serveur MCP Unity/Blender n'a ete pilote.

Sources consultees : [installation BMad et branche stable](https://docs.bmad-method.org/start/install-bmad/), [package npm BMad](https://www.npmjs.com/package/bmad-method), [Graphify sur PyPI](https://pypi.org/project/graphifyy/), [plugin Unity](https://github.com/Unity-Technologies/unity-agent-plugin), [skills Unity](https://github.com/Unity-Technologies/skills), [Ponytail](https://github.com/DietrichGebert/ponytail), [Skills CLI](https://github.com/vercel-labs/skills), [DeepSeek sur le Marketplace VS Code](https://marketplace.visualstudio.com/items?itemName=Vizards.deepseek-v4-for-copilot), [release RTK 1.0.1](https://github.com/rtk-ai/rtk/releases/tag/v1.0.1), [gestion des paquets plugins ChatGPT/Codex](https://developers.openai.com/plugins/build/plugins).

## Derniere etape RTK — droits administrateur necessaires

Windows refuse l'ecriture dans `D:\Program Files\rtk\rtk.exe` : le dossier systeme donne uniquement lecture/execution aux utilisateurs ordinaires. Aucun changement de PATH ou de hook n'a ete fait pour contourner cette protection.

La release 1.0.1 propose une edition OSS Apache-2.0, retenue ici, et une edition incluant le moteur de compression BUSL-1.1. Archive Windows utilisee : `rtk-oss-x86_64-pc-windows-msvc.zip`. SHA-256 confirme avec le digest GitHub : `0e53e2eca471e15fbd3636e64e6bc0b182cbc278ed0339c80450a2dd03f4cd6b`. Le nouveau binaire annonce `rtk 1.0.1` et expose toujours `hook claude`.

Pour terminer, executer cette commande dans **PowerShell ouvert en administrateur** :

```powershell
Copy-Item -LiteralPath 'C:\Users\Kenan\AppData\Local\RRS\tooling-updates\2026-10-21\rtk-oss-1.0.1\rtk.exe' -Destination 'D:\Program Files\rtk\rtk.exe' -Force
rtk --version
```

La sortie attendue est `rtk 1.0.1`. Ensuite relancer Claude et ouvrir de nouvelles conversations Codex/Copilot pour recharger les catalogues de skills et plugins.

## Sauvegarde et retour arriere

Sauvegarde locale hors depot : `C:\Users\Kenan\AppData\Local\RRS\tooling-backups\2026-10-21\`.

Elle contient les anciens dossiers agents, `_bmad/`, les configurations et metadonnees utilisateur modifiees, le registre initial, les empreintes des changements preexistants, les anciens skills tilemap et `rtk-before.exe`. Graphify a aussi sauvegarde le graphe precedent dans `graphify-out/2026-10-21/` (ignore par Git).

Pour revenir en arriere, restaurer uniquement les chemins concernes depuis cette sauvegarde ; Graphify peut etre reepingle avec `uv tool install --force graphifyy==0.9.58`. Ne pas restaurer ou ecraser les artefacts de story lors d'un rollback d'outillage.
