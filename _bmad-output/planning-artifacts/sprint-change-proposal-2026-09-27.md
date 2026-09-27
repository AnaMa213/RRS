# Amendement 5.50-AUTO-DECISIONS-v1 — délégation globale des décisions de conflit

**Statut : APPROUVÉ par Kenan le 2026-09-27 — application autorisée.**

**Texte approuvé :** SHA-256 `D12DE07EB0891B47D24083CD41629E53825D63FBDB43035D3578C3E9B334D2BB` avant inscription de l'approbation. Formule reçue : « J’approuve globalement l’amendement 5.50-AUTO-DECISIONS-v1. »

L'approbation autorise une exécution complète sur le différentiel 5.50, mais ne vaut ni revue ni signature de Gate A.

## 1. Amendement du contrat

### 1.1 Portée de la délégation

Le propriétaire délègue à l'agent, une seule fois et pour la Story 5.50 uniquement, la production et l'écriture de toutes les dispositions de paires issues d'un même modèle épinglé. La délégation couvre :

- les paires candidates nouvelles ou dont l'empreinte a changé ;
- les paires de suivi ordinaires ;
- les faux candidats dont l'absence de conflit est démontrée ;
- les candidats indécidables par la preuve géométrique, traités par la politique conservatrice ci-dessous ;
- les éventuelles décisions devenues orphelines, conservées comme tombstones dans le manifeste d'audit avant leur retrait du jeu actif.

Elle ne couvre jamais : une modification V1 ou physique, un changement de règle de voie, de gabarit, de marge ou d'autorité réseau, un nouveau genre de contrôle, une interprétation de priorité, ni la signature de Gate A.

Le nombre de paires n'est jamais codé en dur. Le rapport courant contient 120 paires à décider — 76 modifiées et 44 nouvelles — ainsi que 84 suivis ordinaires, 20 faux candidats des boîtes englobantes déjà écartés par la distance exacte, 0 paire retirée et 0 échec fermé. L'exécution couvre exhaustivement les ensembles recalculés, même si ces comptes changent avant l'application.

### 1.2 Entrées épinglées

Une exécution de décision est identifiée par un `DecisionRunId` et refuse d'écrire tant que les entrées suivantes ne sont pas toutes capturées et cohérentes :

- identifiant d'approbation `5.50-AUTO-DECISIONS-v1` et SHA-256 du présent amendement approuvé ;
- commit Git propre contenant le moteur de décision et ses tests ;
- `RoadModelVersion`, hash source V1, hash de lignée et hash du profil de validation ;
- `ImporterVersion`, `PipelineVersion`, `CompilerSchemaVersion`, `FingerprintSchemaVersion` ;
- nouveaux `ConflictSweepAlgorithmVersion` et `DecisionPolicyVersion` ;
- SHA-256 canonique du modèle, de la table historique, des décisions d'entrée et du différentiel frais ;
- paramètres numériques exacts : dimensions véhicule, marge latérale, delta_c, tolérances de preuve, profondeur maximale et ordre de subdivision.

Toute variation invalide le run. Une nouvelle géométrie produit un nouveau run et de nouvelles empreintes ; aucune décision précédente n'est recopiée implicitement.

### 1.3 Fonction de classification déterministe

La fonction traite les paires par `PairKey` ordinal, les chemins et intervalles dans leur ordre canonique, et applique les règles suivantes dans cet ordre :

1. **Suivi ordinaire.** Classer `Following` uniquement si une preuve topologique montre soit le même corridor d'approche, soit un chemin dirigé corridor-seul, non ambigu, reliant la sortie de l'un à l'entrée de l'autre dans la portée déclarée. La preuve enregistre le chemin de clés. La paire ne matérialise aucune `ConflictZone`.
2. **Conflit démontré.** Classer `ConflictProven` dès qu'un témoin continu donne deux poses dont les rectangles orientés gonflés se recouvrent. Le certificat enregistre chemins, intervalles, paramètres normalisés, axes séparateurs testés, pénétration minimale et volume résultant. La décision active est `Accepted`.
3. **Absence de conflit démontrée.** Classer `ProvenDisjoint` seulement si toutes les combinaisons de chemins et d'intervalles possèdent un certificat de séparation strictement positif. Le solveur subdivise dyadiquement les deux paramètres ; la borne inférieure d'une cellule est la distance exacte au point de référence moins les rayons de mouvement lipschitziens `|dp| + rho*|dtheta|` des deux empreintes et les gonflements. Une cellule n'est écartée que si cette borne est supérieure à la tolérance publiée. La décision est `Rejected`.
4. **Incertitude conservatrice.** Toute cellule non prouvée, limite numérique, cap dégénéré, rotation hors hypothèse, ramification excessive ou budget de subdivision épuisé devient `ConservativeConflict`. Elle produit une décision `Accepted`, avec la cause exacte et les bornes atteintes. L'ambiguïté d'une paire ne provoque jamais de HALT et ne permet jamais un rejet.

Les boîtes englobantes ne peuvent que présélectionner. Un libellé, un nom d'objet, une image ou une appréciation LLM ne peut jamais déterminer une décision.

### 1.4 Identité, révision et preuve de chaque décision

Chaque résultat possède une identité stable de paire et une identité de révision :

- `PairKey` ordonné : identité fonctionnelle de la décision ;
- `DecisionRevisionId` : SHA-256 du record canonique comprenant `PairKey`, empreinte géométrique, résultat, version de politique et hash de preuve ;
- `GeometryFingerprint`, hashes canoniques des deux mouvements et volume quantifié ;
- `ModelVersion`, `DecisionRunId` et toutes les versions listées en 1.2 ;
- `Classification`, `Decision`, `ReasonCode`, motif lisible, données ou bornes du témoin, et `EvidenceHash` ;
- `SupersedesDecisionRevisionId` lorsque la paire existait auparavant.

Une zone déjà acceptée conserve son `RoadId` si la même `PairKey` reste acceptée, mais reçoit obligatoirement une nouvelle révision explicitant l'ancienne et la nouvelle empreinte. Un passage `Accepted -> Rejected` retire la zone active et écrit un tombstone. Un passage `Rejected -> Accepted` ou une nouvelle paire frappe un `RoadId` une seule fois avec le mécanisme existant, puis le fige dans le plan d'application. La seconde passe et toutes les reprises réutilisent ce plan : elles ne refrappent jamais une identité.

Le manifeste Editor-only `_bmad-output/implementation-artifacts/v1-regression-5-50/automated-pair-decisions.json` conserve tous les records, y compris suivis, faux candidats, révisions remplacées et tombstones. Le fichier d'authoring ne reçoit que les décisions actives requises par le compilateur ; chacune référence son `DecisionRevisionId` et son `EvidenceHash`. Le chargeur runtime ne lit jamais le manifeste.

### 1.5 Écriture transactionnelle

Après approbation, l'agent exécute sans checkpoint paire par paire :

1. implémenter et committer le moteur, le schéma et les tests sur un arbre propre ;
2. produire deux fois en lecture seule le même plan canonique ; hors identités nouvelles préallouées dans la première passe puis réutilisées, les octets doivent être identiques ;
3. valider la couverture exhaustive, les preuves, les identités et les scénarios de circulation ;
4. écrire en fichiers temporaires le manifeste et les décisions, les relire, puis remplacer atomiquement les destinations ;
5. relancer le moteur : zéro changement proposé et mêmes hashes ;
6. régénérer les artefacts 5.27/5.28 autorisés et lancer les validations officielles ;
7. laisser Gate A fermée et non signée.

Aucune décision partielle n'est écrite. Une erreur avant le remplacement conserve les décisions d'entrée intactes.

### 1.6 Politique de HALT amendée

Le HALT « décisions du propriétaire » de la Story 5.50 est supprimé après l'approbation globale. Ne déclenchent jamais un HALT : le nombre de paires, une paire nouvelle ou modifiée, une ambiguïté géométrique isolée, ni une classification conservatrice.

Un HALT reste obligatoire uniquement pour :

- une modification de contrat non couverte par `5.50-AUTO-DECISIONS-v1` ;
- une entrée épinglée incohérente ou altérée que la reconstruction ne résout pas ;
- un défaut systémique — couverture incomplète, identités dupliquées, paire inter-carrefours, preuve non reproductible ou résultats non déterministes — persistant après correction ;
- l'échec des invariants de sécurité ou de progression après application de la politique conservatrice, lorsqu'aucune politique sûre et bornée ne peut être appliquée ;
- une validation officielle révélant qu'une exigence approuvée doit être changée.

Les défauts ordinaires de code ou de test sont corrigés et la procédure est relancée ; ils ne sont pas transformés en arbitrages manuels de paires.

### 1.7 Modifications normatives à appliquer après approbation

**`docs/setup/build-workflow-rules.md` — exception bornée 5.50 :** ajouter une clause indiquant qu'une approbation globale identifiée peut déléguer les décisions à une fonction déterministe versionnée ; l'agent ne décide pas par jugement, il exécute la fonction et enregistre ses preuves. Cette exception ne s'étend à aucune autre story et ne permet jamais une signature Gate A.

**`epics.md`, Story 5.50 :**

- remplacer « targeted owner reconfirmation » par « deterministic delegated disposition under one owner-approved policy » dans la capacité ;
- remplacer la section `Decision reconfirmation` par les règles 1.1 à 1.5 ;
- remplacer les étapes 2 à 5 par : génération du plan, double exécution identique, validation, application transactionnelle, régénération et validations officielles sans HALT de paire ;
- remplacer l'AC final par : toutes les paires reçoivent une disposition prouvée ou conservatrice, chaque révision est liée à l'empreinte et aux versions exactes, et aucune décision n'est transférée implicitement ;
- conserver « no Gate A signature » et ajouter « no LLM/manual classification » aux non-objectifs.

**Spec 5.50 :** après approbation explicite, renégocier le bloc gelé uniquement sur les clauses `Decisions`, `Never`, matrice des paires, tâches de décision et HALT ; ajouter le manifeste, les versions, les preuves, les tests et scénarios ci-dessous. Les captures 0/1a/1b, les invariants V1, la géométrie, le profil et les gates restent inchangés.

**Spec 5.28 et suivi Gate A :** reconnaître les décisions 5.50 automatisées comme valides seulement si le manifeste et les hashes vérifient. La revue visuelle et la signature Gate A restent des actes séparés du propriétaire ; aucun bouton ni pipeline ne les déclenche automatiquement.

Les propositions historiques des 25 et 26 septembre ne sont pas réécrites ; le présent amendement les supersède uniquement sur l'autorité et le HALT des décisions de paires. Aucun PRD n'existe dans le projet ; aucun changement UX, runtime, réseau ou d'ordre des stories n'est requis.

## 2. Mécanisme de validation

### 2.1 Preuves géométriques et tests adversariaux

Les tests EditMode couvrent au minimum :

- contact franc, contact tangent exact, séparation juste au-dessus de la tolérance ;
- croisement uniquement entre poses distantes, rotation seule et couture seule ;
- chaîne plus courte qu'un demi-véhicule et portée au-delà des extrémités ;
- même approche et suivi sur chemin à une voie ; branchement ambigu refusant la classification `Following` ;
- faux positif AABB prouvé disjoint par rectangles orientés ;
- cap dégénéré, rotation >= 90 degrés et budget de subdivision épuisé, tous classés `ConservativeConflict` ;
- permutation de l'ordre des mouvements, des chemins et des paires : résultat canonique identique ;
- même `PairKey` avec géométrie ou volume modifié : nouvelle révision obligatoire, ancienne empreinte jamais réutilisée ;
- modèle, balayage, politique, preuve ou manifeste altéré : aucune écriture ;
- suppression ou duplication d'un record, paire inter-carrefours ou collision d'identité : échec fermé systémique.

Pour un rejet, le test exige le certificat de séparation complet et le revérifie indépendamment. L'absence de témoin de contact n'est jamais une preuve de séparation.

### 2.2 Couverture du différentiel réel

Sur `MVP_Run`, le run doit démontrer :

- exactement 9 carrefours, tous présents dans le rapport ;
- chaque paire possible classée exactement une fois ;
- partition sans recouvrement entre `ConflictProven`, `ProvenDisjoint`, `Following` et `ConservativeConflict` ;
- couverture de toutes les 120 paires du différentiel courant si les entrées restent identiques, plus publication des 84 suivis et 20 faux candidats actuels ;
- zéro décision manquante, obsolète, orpheline non tracée ou liée à une autre empreinte ;
- tous les `Accepted` matérialisent une zone du bon carrefour ; aucun `Rejected` ne matérialise de zone.

Les comptes recalculés, le nombre d'incertitudes et les marges minimales sont publiés par carrefour et comparés entre les deux exécutions.

### 2.3 Scénarios de circulation sur les neuf carrefours

Un harnais Editor-only, alimenté par le graphe de conflits final et non par un runtime futur, exécute sur chacun des neuf carrefours :

1. chaque mouvement seul ;
2. chaque paire acceptée demandée simultanément ;
3. tous les mouvements demandés simultanément ;
4. arrivées cycliques saturées dans l'ordre canonique, l'ordre inverse et des permutations à graines fixes ;
5. suivis ordinaires avec espacement valide, qui ne doivent jamais être bloqués comme conflits.

Le harnais utilise un ordonnanceur de preuve à ordre total tournant et accorde à chaque tour un ensemble maximal de mouvements compatibles. Il ne définit aucun `JunctionControlKind` et n'écrit aucune priorité gameplay.

Les portes sont :

- jamais deux mouvements reliés par une décision `Accepted` accordés ensemble ;
- file non vide => au moins un mouvement accordé ;
- ensemble accordé maximal : aucun mouvement compatible avec tous les accordés n'est bloqué évitablement ;
- tout mouvement continuellement en attente progresse en au plus `M_j` tours d'octroi, `M_j` étant le nombre de mouvements du carrefour ;
- aucune paire `Following` ou `ProvenDisjoint` ne crée un blocage ;
- résultats identiques entre répétitions et permutations d'entrée ;
- rapport par carrefour : degrés du graphe, taille de clique, taille des ensembles accordés, attente maximale, débit minimal et nombre de conflits conservateurs.

Cette preuve vérifie l'absence de deadlock, de famine, de blocage évitable et de priorité contradictoire dans le graphe produit. La sémantique de priorité routière réelle reste la responsabilité des Stories 5.34/5.35.

### 2.4 Validation officielle et reproductibilité

Avant régénération définitive :

- tests ciblés Story 5.50, y compris preuve indépendante des certificats et byte-identité du plan ;
- tests Story 5.28 : une empreinte, preuve ou version incohérente ferme Gate A ;
- tests 5.25/5.26/5.27 inchangés et verts ;
- suite EditMode complète via `scripts/validate.ps1 -TestMode EditMode`, zéro nouvelle erreur Console ;
- vérification source V1, lignée et identités ;
- génération des artefacts autorisés, puis comparaison avec un pipeline frais ;
- `graphify update .` seulement après stabilisation finale.

Gate A reste fermée tant que ses preuves physiques, sa revue visuelle et sa signature propriétaire distincte ne sont pas complètes.

## 3. Risques résiduels

- **Conservatisme.** Une paire incertaine devient un conflit actif. Cela peut réduire le parallélisme sans compromettre la sécurité. Le harnais borne l'attente et interdit les blocages évitables selon le graphe connu, mais ne prouve pas les performances du futur runtime.
- **Preuve géométrique numérique.** Le solveur peut ne pas conclure près d'une tangence ; il accepte alors conservativement. Un rejet reste impossible sans certificat de séparation strictement positif.
- **Identités nouvelles.** Les nouveaux `RoadId` restent frappés une fois par le mécanisme existant, puis figés dans le plan. La classification est déterministe ; l'allocation opaque initiale n'est pas recalculée et son résultat est couvert par hash.
- **Priorités futures.** La 5.50 produit un graphe symétrique de conflit, pas une règle de priorité. Le harnais prouve une politique neutre sûre et vivante ; les interactions avec stop/yield/roundabout/signaux devront être revalidées en 5.34/5.35.
- **Écart modèle-réalité.** Les décisions portent sur la référence compilée et son gabarit gonflé, pas sur la physique finale. Les preuves physiques de la 5.51 et la Gate A restent obligatoires.
- **Portée de l'approbation.** Toute modification hors Story 5.50 ou toute nouvelle autorité de décision exige un nouvel amendement. L'approbation présente n'est ni un précédent général ni une délégation de signature.

**Formule d'approbation attendue :** `J'approuve globalement l'amendement 5.50-AUTO-DECISIONS-v1.`
