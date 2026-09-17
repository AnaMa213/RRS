# Baseline Project Auditor (ADDON-018)

Mesure du pilote P2, 2026-09-17. Package installe : `com.unity.project-auditor-rules` `2.0.0`
(absent avant ce pilote — `audit_status` renvoyait `idle` mais l'analyse n'avait aucun module
enregistre : `unavailable`, `"Install the Project Auditor Rules package"`). Scan declenche
par l'utilisateur (AD-4), suivi via `unity cmd audit` -> `audit_status` -> lecture du CSV.

## Duree

~150 secondes (2,5 min) du declenchement a `status:"completed"`, sur le projet complet
(335 prefabs Synty inclus). Confirme le choix deja pris par le backlog de garder Project
Auditor **hors du chemin critique de `validate.ps1`** — trop lent pour un gate a chaque run,
expose en opt-in via `-Audit`.

## Volume et origine (4232 diagnostics)

| Origine | Count | Part |
| --- | --- | --- |
| `Assets/RoadRage/Tests/` | 1800 | 42,5 % |
| `Assets/RoadRage/` (production) | 597 | 14,1 % |
| `Assets/Synty/` (decor tiers, hors scope) | 1401 | 33,1 % |
| Autres Assets, Packages, ProjectSettings | 434 | 10,3 % |

Severite en production RoadRage : 531 `Info`, 66 `Moderate`, **0 `Major`**. Les 3 seuls
diagnostics `Major` du scan complet sont tous dans `Assets/Synty/` (meshes non marquees
Read/Write) — decor tiers, hors perimetre du projet.

## Part actionnable (mesuree, pas estimee)

Sur les 597 diagnostics production :

- **Bruit structurel (~90 %)** : `PAC2002`/`PAC1002`/`PAC2004` (allocations LINQ/string
  generiques, 411 occurrences reparties sur 48+ fichiers, aucune concentration suspecte),
  `PAC0193`/`PAC0192` (`Debug.Log`/`LogWarning` en `OnValidate` ou chemin de chargement
  one-shot, 118 occurrences) : signal reel mais bas risque, non actionnable sans revue
  cas par cas.
- **Faux positif confirme par lecture** : `PAC0231` (`Object.name` usage) sur
  `NetworkedAIVehicleDriverController.cs:257` — l'appel est protege par un flag
  `warnedMissingDriverProfile` qui ne se declenche qu'une fois, jamais par frame. L'analyse
  statique ne voit pas la garde runtime.
- **Actionnable, cout faible, confirme** : **8 occurrences `PAC0194`** (`FindObjectsByType`
  obsolete dans Unity 6) reparties sur 5 fichiers (`NetworkedPassengerActionIntent.cs`,
  `NetworkedVehicleSeatService.cs` x2, `RunFlowController.cs` x3, `AiRageTargetResolution.cs`)
  — migration API mecanique vers la surcharge non obsolete. **1 occurrence `PAA3000`**
  (`NetworkedPlayerRoot.prefab` dans un dossier `Resources/`) — anti-pattern Unity connu
  (cout de build/chargement), a verifier si le chargement par nom est reellement necessaire.

Part actionnable confirmee sans ambiguite : **9 diagnostics sur 597** (~1,5 %). Le reste
exige un jugement humain au cas par cas ; aucun ne bloque quoi que ce soit aujourd'hui (0
`Major` en production).

## Decision (ADDON-018)

**`Adopt`, en opt-in manuel** (`scripts/validate.ps1 -Audit`), jamais comme gate par defaut.
Justification : cout mesure (2,5 min, signal a 90 % de bruit/jugement) trop eleve pour un
gate systematique, mais le pilote a trouve des diagnostics reels qu'aucun autre outil du
stack (compilateur, `Unity.Analyzers.*`, 533 tests) ne couvre — l'obsolescence d'API n'est
visible ni en Console ni en tests tant qu'elle ne casse rien. Valeur marginale reelle mais
etroite : outil de passe occasionnelle, pas de surveillance continue.

**Cout de rollback** : retirer le switch `-Audit` de `scripts/validate.ps1`, arreter d'appeler
`unity cmd audit`, retirer `com.unity.project-auditor-rules` de `Packages/manifest.json`.

**Declencheur de reevaluation** : un `Major` en production apparait dans un futur scan, ou la
part actionnable mesuree dans un futur pilote depasse largement ce ratio (signal que le code
a plus de dette reelle qu'aujourd'hui).
