---
title: 'Story 5.43 -- Rage et peur comme modulation continue de la politique de conduite'
type: 'feature'
created: '2026-10-09'
status: 'done'
baseline_commit: '2e3964c4d9919da30c77d79318a0a68a2f1f6211'
review_loop_iteration: 0
context:
  - '{project-root}/_bmad-output/implementation-artifacts/spec-5-41-driving-policy-and-traffic-rule-exception-protocol.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Les jauges rage/peur de `NetworkedRageState` ne decroissent pas, ne gelent pas et ne sont pas arbitrees. Le V2 ne les lit pas, et seul le V1 module son profil, par une disposition discrete.

**Approach:** Un noyau pur de la feature Rage fait evoluer les jauges : decroissance, gel de palier, maintien a 100 %, arbitrage. L'hote publie une lecture etroite dans Shared (`EmotionReading`). `DrivingPolicy.Resolve` en tire les parametres effectifs par une seule formule continue. Decisions proprietaires du 2026-10-09 : canal gouvernant, maintien verrouille, noyau et arete sans prefab.

## Boundaries & Constraints

**Always:**
- **E1, evolution.** Taux, duree, seuils et facteurs sont authores dans `RageTuningDef`, avec ces valeurs par defaut :
  - rage et peur decroissent chacune de 1 % du max par seconde, plancher 0 ;
  - l'entree dans un palier de rage superieur (indice de seuil qui monte) gele la decroissance de rage pendant 10 s ; la jauge peut monter pendant le gel, et un nouveau palier relance le gel.
- **E2, maintien verrouille.** Une montee qui atteint le max arme un maintien : plus de decroissance de rage. Le maintien se libere quand l'evenement Rage Road associe a ete vu actif (`Triggered`/`Confrontation`), puis ne l'est plus. Il se libere aussi si un delta negatif explicite fait passer la rage sous le max. Sans evenement, la rage reste a 100.
- **E3, arbitrage hote.** Seuils en % du max, authores (100, 50, 30) :
  - peur ≥ 100 → `FearSaturated` ;
  - sinon, la fuite s'ouvre si peur ≥ 50 et peur > rage ; ouverte, elle ne se ferme qu'a peur ≤ 30, quelle que soit la rage (`Escape`) ;
  - sinon la rage gouverne (`Rage`).
- **E4, modulation.** Poids (wR, wP) = (rage01, 0) si la rage gouverne, sinon (0, peur01). Pour chaque levier : effectif = base × max(0, 1 + gR·wR + gP·wP). Leviers : v0, T, s0, a, b, b_safe, risque accepte (borne a [0, 1]), creneau accepte et couts de manoeuvre. Les gains sont authores sur `DriverProfileDef`. Surfaces, volontes, `ReactionTime`, `Consistency` et `VehicleProfileDef` sont intacts. Une lecture calme donne une politique identique bit a bit a la 5.41.
  - **Domaines.** Apres modulation, chaque levier reste dans son domaine declare : a et b > 0 ; v0, T, s0, b_safe, creneau et couts ≥ 0 ; risque dans [0, 1]. v0 = 0 reste permis et produit `PolicyImmobilization`. La politique ne lit ni ne modifie aucune capacite physique ; les capacites moteur et frein restent imposees en aval par `SafetyLimits` et le composeur, sans changement.
  - **b_safe.** Les consommateurs de comportement (arbitrage longitudinal, `SpeedPlan`, `MotionCommand`) lisent le b_safe effectif du pas. Le repli V2 du composeur (seuil de maintien, fraction de service, diagnostic de depassement) est un chemin de securite : il lit le b_safe authore de base, jamais une valeur modulee ni un instantane du premier pas.
- **E5, lecture.** Le pilote V2 lit `IEmotionSource` sur son GameObject. Une source absente, ou une lecture dont une composante n'est pas finie, donne la lecture `Calm` entiere. Les composantes finies sont bornees a [0, 1].

**Ask First:**
- Une fixture Story541 ou Story542 rouge : HALT avant d'adapter une assertion. Les fixtures V1 de rage (4.1, 5.1, 5.4, 5.5) ne portent que `Core` : leur invariance est prouvee dans Story543.
- Toute modification d'un prefab, de `MVP_Run`, d'un asset `.asset`, du modele routier ou de Gate A : HALT.

**Never:**
- Aucun `if`/`switch` sur le gouverneur ou sur `RageDisposition` dans `Features/Vehicles/Traffic`, et aucune reference a `Features.Rage` depuis Vehicles.
- Aucun tick automatique de `NetworkedRageState` : le V1 reste inchange. Le tick de production, le prefab V2, le ciblage et la fuite vers un portail relevent de la 5.44.
- Aucune modification de `DriverModel.ResolveEffectiveProfile(DriverProfile, RageDisposition)` ni du controleur V1.
- `DesiredSpeedAt` reste non applique au runtime (invariant calme bit a bit) : son activation releve de la 5.44, avec l'integration de production et le PlayMode.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Calme | Source absente | Politique = 5.41 bit a bit | N/A |
| Decroissance | rage 40, palier non entre, 5 s | rage 35 | Plancher 0 |
| Gel | 19 → 21 (palier 20), puis 5 s, puis +10 | 21, puis 31 (gel relance si nouveau palier) | N/A |
| Maintien | 100, aucun evenement, 60 s | 100 | N/A |
| Liberation | 100, evenement actif puis `Resolved` | decroit des le pas suivant | N/A |
| Lecture non finie | rage NaN, peur 0,8 | `Calm` entiere | N/A |
| Saturation | peur 100, rage 100 | `FearSaturated`, poids (0, 1) | N/A |
| Fuite | peur 50, rage 40, puis peur 31 et rage 90 | `Escape` tenue | Sortie a peur ≤ 30 |
| Egalite | peur 50, rage 50 | `Rage` | N/A |
| Immobilisation | gain v0 = -1, wR = 1 | v0 = 0 ; blocker `PolicyImmobilization` legitime ; `ProgressExpected` faux | N/A |
| Tuning invalide | seuils non ordonnes, taux < 0, gain < -1 | `TryValidate` faux | N/A |

</frozen-after-approval>

## Code Map

- `Features/Rage/NetworkedRageState.cs` : `ApplyRageDelta`, `ApplyFearDelta` et `ApplyReactionEffect` gardent leurs valeurs ; ils mettent ensuite a jour l'etat hote (gel, maintien, fuite). Ajouts : `Advance(dt, tuning, associatedEventActive)` et `IEmotionSource`.
- `Features/Rage/RageTuningDef.cs` : champs E1 a E3 et `TryValidate`. Les assets gardent les valeurs d'initialiseur, sans reecriture.
- `Shared/Domain/IRageDispositionSource.cs` : patron de lecture etroite pour `IEmotionSource`, `EmotionReading` et `EmotionGovernor`.
- `Features/Vehicles/DriverModel.cs:137` : noyau V1, intact. La surcharge continue s'ecrit a cote.
- `Features/Vehicles/DriverProfileDef.cs:50` et `:172` : patron du champ authore et de la validation (`collisionReaction`, `policy`).
- `Traffic/Policy/DrivingPolicy.cs:153` (`EffectivePolicy`, accesseurs lus par `ManeuverEvaluation`) et `:266` (`Resolve`).
- `Traffic/Lifecycle/TrafficV2VehicleDriver.cs:575` (`Awake`), `:677` (unique `DrivingPolicy.Resolve(`) et `:698` (composeur construit avec le b_safe du premier pas).
- `Traffic/Intent/VehicleDriveIntentComposer.cs:107`, `:124`, `:194` et `:207` : b_safe sur le seul chemin de repli. `Traffic/Safety/SafetyFilter.cs:96` (`SafetyLimits.For`) : capacites physiques, en lecture seule.
- `Traffic/Blockers/BlockerTracker.cs:48` (v0 ≤ 0,001 → `PolicyImmobilization`) et `Traffic/Recovery/RecoverySupervisor.cs:294` (`ProgressExpected`), en lecture seule.
- Scans figes : `Story541DrivingPolicyTests.cs:461` et `:487` (un seul `Resolve`, `var driver = policy.Driver;`), `Story542ManeuverTests.cs:488`.

## Tasks & Acceptance

**Execution:**
- [x] `Shared/Domain/EmotionReading.cs` : `EmotionGovernor`, `EmotionReading` (bornee [0, 1], NaN → 0, `Calm`, `RageWeight`, `FearWeight`) et `IEmotionSource`.
- [x] `Features/Rage/EmotionMeters.cs` : noyau pur E1 a E3 (etat, changement, avance, lecture).
- [x] `Features/Rage/RageTuningDef.cs`, `NetworkedRageState.cs` : champs, validation et integration hote.
- [x] `Features/Vehicles/EmotionModulation.cs` : gains par canal, valeurs par defaut, validation, facteur E4. `DriverModel` recoit la surcharge continue. `DriverProfileDef` recoit le champ et sa validation.
- [x] `Traffic/Policy/DrivingPolicy.cs` : `Resolve(..., EmotionReading)`, `EffectivePolicy` effective et bornee, profil authore et lecture exposes, `ToText`.
- [x] `Traffic/Lifecycle/TrafficV2VehicleDriver.cs` : source mise en cache dans `Awake`, lecture passee a l'unique `Resolve`, composeur construit avec le b_safe authore de base.
- [x] `Tests/EditMode/Story543EmotionModulationTests.cs`, `[Core][Story543]`. Il couvre :
  - la matrice ;
  - la continuite et la monotonie par levier ;
  - l'isolement entre vehicules ;
  - les domaines apres modulation et le repli sur le b_safe de base ;
  - l'integration `NetworkedRageState`, avec des valeurs V1 inchangees ;
  - le scan structurel, sans branche emotionnelle ni `Features.Rage` dans Traffic et sans `VehicleProfile` dans la modulation.

**Acceptance Criteria:**
- Given une jauge qui varie, when les parametres sont resolus, then ils sont une fonction continue du poids gouvernant, sans autre logique de conduite et sans toucher `VehicleProfileDef`.
- Given aucune source d'emotion, when les fixtures Story541 et Story542 tournent, then elles restent vertes sans modification.
- Given deux vehicules, when l'un enrage, then la politique de l'autre est inchangee.

- **2026-10-09 -- Revue.**
  - **Couches actives.** `blind-hunter` (en ligne, aucun constat), `edge-case-hunter` et `verification-gap`. `security-review` inactive : aucune RPC, aucune permission de NetworkVariable ni autorite modifiee. Aucun `intent_gap` ni `bad_spec`.
  - **Correctifs appliques (patch) :**
    - un gain fini mais enorme qui fait deborder un levier ramene la politique authoree, jamais un infini ;
    - la surcharge continue de `DriverModel` ignore une modulation non valide ;
    - le palier hote est reamorce quand le tuning differe du dernier applique (jauge ecrite directement, autre jeu de paliers) : aucun gel parasite ;
    - la lecture d'emotion du pilote V2 est prouvee par execution (sans source, source reelle, source detruite) ;
    - la preuve d'immobilisation construit spine et plan avec le profil effectif v0 = 0, comme le pilote : plan accepte, aucun repli `ProfileRefused`.
  - **Rejetes :** rearmement du maintien sans montee au max (hors E2) ; etat perime au pooling (aucun pooling V2) ; source ajoutee apres `Awake` (cache voulu, prefab en 5.44) ; garde `IsServer` sur `Advance` (convention existante de `NetworkedRageState`).
  - **Validation finale brute.** 0 erreur Console sur chaque fenetre, compilation saine, `MVP_Run` propre.

    | Story | EditMode |
    |---|---|
    | 5.43 | 34/34 |
    | 5.41 | 35/35 |
    | 5.42 | 47/47 |

### Review Findings — 2026-10-09 — Independent requested review

Four configured layers completed on `2e3964c..f777f72`, inspected at `3088e6e`. The owner authorized all five patches; all are now implemented with regression coverage. Eight claims were dismissed after checking callers and approved boundaries. Final EditMode validation: Story543 52/52 (18 additional cases), Story541 35/35 and Story542 47/47, with healthy compilation, zero Console errors in each window and clean MVP_Run. The first patch validation failed 2/52; normalization rounding and an incorrect new-test expectation were corrected without changing the existing arbitration assertion. [Detailed findings, resolution and exact validation recaps](code-review-5-43-2026-10-09.md). Spec remains done; sprint tracking remains review under the project checkpoint-promotion rule.

- [x] [Review][Patch][Medium] R1 — Zero b_safe is accepted by LongitudinalBounds and plans with zero behavioral deceleration; negative braking and zero acceleration bounds remain invalid. Regression traverses spine, verifier, speed plan, arbitration and MotionCommand at rest/moving, both governors and v0=0. It also proves physical clamping and unchanged authored fallback braking. `Assets/RoadRage/Features/Vehicles/Traffic/Planning/MotionPlan.cs:40`; `SpeedPlan.cs:325`; `Tests/EditMode/Story543EmotionModulationTests.cs:475`.
- [x] [Review][Patch][Medium] R2 — Either nonfinite raw meter returns whole-reading Calm before normalization. Six NaN/infinity cases cover both kernel and real host producer; finite huge values remain bounded, with exact escape-exit percentage normalization. E5. `Assets/RoadRage/Features/Rage/EmotionMeters.cs:76`; `Tests/EditMode/Story543EmotionModulationTests.cs:226`.
- [x] [Review][Patch][Medium] R3 — Every fear lever now has formula, monotonicity and continuity assertions over governing weights, for Escape and FearSaturated, with distinct nonzero gains and risk saturation. E4. `Assets/RoadRage/Tests/EditMode/Story543EmotionModulationTests.cs:346`.
- [x] [Review][Patch][Medium] R4 — Host publication is checked at fear 31 (escape held), 29 (escape closed) and 28 (subsequent decay starts from the published value), observing both FearValue and CurrentEmotion. E1/E3. `Assets/RoadRage/Tests/EditMode/Story543EmotionModulationTests.cs:528`.
- [x] [Review][Patch][Low] R5 — LeversValid rejects underflowed a/b=0 and restores the entire authored policy. Four cases cover both strict-positive levers and both governing channels, verify valid authoring and preserve calm bits. `Assets/RoadRage/Features/Vehicles/Traffic/Policy/DrivingPolicy.cs:190`; `Tests/EditMode/Story543EmotionModulationTests.cs:622`.

## Design Notes

Gains par defaut (gR / gP) :

| Levier | gR | gP |
|---|---|---|
| v0 | +0,3 | +0,3 |
| T | −0,5 | +0,5 |
| s0 | −0,5 | +0,5 |
| a | +0,5 | +0,3 |
| b | +0,3 | 0 |
| b_safe | +0,5 | 0 |
| risque | +1,0 | −0,5 |
| creneau | −0,5 | +0,5 |
| couts | −0,5 | +0,5 |

Chaque gain est fini et ≥ −1 ; a et b exigent > −1. Les plafonds de route, de courbe et de braquage bornent toujours v0.

Les domaines ne plafonnent pas a et b par la capacite physique. Ce plafond couplerait la politique au `VehicleProfile`, et changerait la politique calme de tout profil authore au-dela de sa capacite. Les capacites restent imposees en aval, comme aujourd'hui.

## Verification

**Commands:**
- `.\scripts\validate.ps1 -Profile Story -Story 5.43 -TestMode EditMode` : expected `VALIDATION STORY`, compte attendu, 0 erreur Console.
- `.\scripts\validate.ps1 -Profile Story -Story 5.41 -TestMode EditMode`, puis `-Story 5.42` : expected verts.
- `git status --short` : expected aucun prefab, scene ou `.asset` modifie.

## Suggested Review Order

**Modulation continue (E4)**

- Point d'entree : une formule, modulation non valide ou debordante ramenee a l'authore.
  [`DrivingPolicy.cs:305`](../../Assets/RoadRage/Features/Vehicles/Traffic/Policy/DrivingPolicy.cs#L305)
- Leviers de politique : risque borne, creneau, facteur de cout.
  [`DrivingPolicy.cs:317`](../../Assets/RoadRage/Features/Vehicles/Traffic/Policy/DrivingPolicy.cs#L317)
- Profil effectif sans branche ; personnalite hors emotion intacte.
  [`DriverModel.cs:168`](../../Assets/RoadRage/Features/Vehicles/DriverModel.cs#L168)
- Facteur max(0, 1 + gR.wR + gP.wP), exactement 1 au calme.
  [`EmotionModulation.cs:96`](../../Assets/RoadRage/Features/Vehicles/EmotionModulation.cs#L96)
- Gains par defaut et validation (>= -1, a et b > -1).
  [`EmotionModulation.cs:75`](../../Assets/RoadRage/Features/Vehicles/EmotionModulation.cs#L75)

**Jauges et arbitrage (E1-E3)**

- Palier superieur -> gel, montee au max -> maintien, puis arbitrage.
  [`EmotionMeters.cs:36`](../../Assets/RoadRage/Features/Rage/EmotionMeters.cs#L36)
- Avance : liberation du maintien apres evenement, decroissance hors gel.
  [`EmotionMeters.cs:56`](../../Assets/RoadRage/Features/Rage/EmotionMeters.cs#L56)
- Lecture : peur saturee, sinon fuite tenue, sinon la rage gouverne.
  [`EmotionMeters.cs:74`](../../Assets/RoadRage/Features/Rage/EmotionMeters.cs#L74)
- Poids gouvernants : la seule selection de canal, cote Shared.
  [`EmotionReading.cs:40`](../../Assets/RoadRage/Shared/Domain/EmotionReading.cs#L40)
- Seuils authores et validation ordonnee de l'arbitrage.
  [`RageTuningDef.cs:265`](../../Assets/RoadRage/Features/Rage/RageTuningDef.cs#L265)

**Etat hote et arete V2 (E5)**

- Avance hote sans tick automatique : le V1 reste inchange.
  [`NetworkedRageState.cs:58`](../../Assets/RoadRage/Features/Rage/NetworkedRageState.cs#L58)
- Palier reamorce quand le tuning change : aucun gel parasite.
  [`NetworkedRageState.cs:162`](../../Assets/RoadRage/Features/Rage/NetworkedRageState.cs#L162)
- Lecture etroite, calme si source absente ou detruite.
  [`TrafficV2VehicleDriver.cs:587`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L587)
- Unique resolution, lecture d'emotion passee a chaque pas prepare.
  [`TrafficV2VehicleDriver.cs:687`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L687)
- Repli V2 sur le b_safe authore, jamais module.
  [`TrafficV2VehicleDriver.cs:709`](../../Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs#L709)
- Champ authore sur la personnalite, hors struct V1.
  [`DriverProfileDef.cs:54`](../../Assets/RoadRage/Features/Vehicles/DriverProfileDef.cs#L54)

**Preuves**

- Calme : politique 5.41 identique bit a bit.
  [`Story543EmotionModulationTests.cs:219`](../../Assets/RoadRage/Tests/EditMode/Story543EmotionModulationTests.cs#L219)
- Matrice d'arbitrage.
  [`Story543EmotionModulationTests.cs:140`](../../Assets/RoadRage/Tests/EditMode/Story543EmotionModulationTests.cs#L140)
- Maintien verrouille et liberation.
  [`Story543EmotionModulationTests.cs:111`](../../Assets/RoadRage/Tests/EditMode/Story543EmotionModulationTests.cs#L111)
- Immobilisation : blocker legitime, aucune progression attendue, plan effectif accepte.
  [`Story543EmotionModulationTests.cs:371`](../../Assets/RoadRage/Tests/EditMode/Story543EmotionModulationTests.cs#L371)
- Pilote V2 : source lue, calme sans source ou apres destruction.
  [`Story543EmotionModulationTests.cs:422`](../../Assets/RoadRage/Tests/EditMode/Story543EmotionModulationTests.cs#L422)
- Scan : aucune branche emotionnelle ni physique dans la pile de conduite.
  [`Story543EmotionModulationTests.cs:478`](../../Assets/RoadRage/Tests/EditMode/Story543EmotionModulationTests.cs#L478)
