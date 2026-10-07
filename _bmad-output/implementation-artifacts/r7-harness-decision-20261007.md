# R7 — décision sur la preuve de fusion

Statut : approuvé par le propriétaire le 2026-10-07 : « R7 est approuvé, tu peux faire la correction ». Appliqué et validé, R7 clos.

La revue exige un `GrantedMergeGap` calculé à partir de poses, routes complètes et rapports du builder, alors que le titulaire conserve son grant, suivi d'un dégagement et d'une poursuite sans contact dans MVP_Run.

Le brouillon [pending-r7-fixture-20261007.patch](pending-r7-fixture-20261007.patch) conserve le montage tenté, **non validé et retiré des sources actives**. Il n'est pas un correctif prêt à appliquer :

- Les routes normales trouvées par l'insertion ne fournissaient pas les deux chaînes nécessaires. Les objectifs intermédiaires du chemin de mesure 5.31 permettent de produire ces routes légalement avec RoutePlanner.
- `MeasurementRun` est exclusif avec `TrafficV2Scenario`, et `PortalTrafficSpawner.TickV2Slice` borne sa population à `V2SliceMaxPopulation = 1`. Le titulaire retenu empêcherait l'insertion de l'entrant.
- La fixture EditMode sur poses réelles a effectivement refusé le créneau avec le profil par défaut : `t_gap 5.604 ETA 4.44`. Cette sortie est un échec conservé, pas une preuve de succès. Aucun PlayMode du brouillon n'a été lancé.

## Proposition concrète

1. Ajouter au jeton `MeasurementRun` un paramètre optionnel de population, conservant le défaut 1 ; vérifier une valeur strictement positive bornée par le nombre de triplets. Seuls les tests construisent ce jeton.
2. Lire cette borne dans `TickV2Slice` pour une campagne de mesure. Conserver l'exclusion scénario/mesure et la population normale V2 de 1. Le test R7 demande explicitement 2.
3. Construire en mémoire, uniquement pour le titulaire H, un profil ayant l'accélération du profil par défaut divisée par quatre. Vitesse désirée, freinage, gabarit, modèle signé et tous les seuils/formules de coordination restent identiques. Le rapport du builder et la conduite réelle de H utilisent le même profil. Le scénario prouve ainsi le cas P10 avec une ETA suffisante, sans prétendre à une admission sur les paramètres par défaut ci-dessus.
4. Relâcher R alors que H conserve son grant ; vérifier le `GrantedMergeGap`, `ETA >= t_gap`, le dégagement de R avant la reprise de H, la poursuite effective et l'absence de contact. Conserver le scénario G de refus de créneau insuffisant.
5. Exécuter la fixture EditMode, la fixture PlayMode dans MVP_Run et les non-régressions du harnais par `scripts/validate.ps1`.

La section Ask First de la spec 5.35 avait imposé un arrêt pour cette décision architecturale indépendante. L'approbation ci-dessus autorise désormais la proposition ; la seule présence du harnais ne constituait pas une autorisation. Le brouillon reste une trace de la tentative précédente, à compléter par les paramètres approuvés et les preuves finales.

## Résultat de l'application

La population explicite est désormais portée par `MeasurementRun`, bornée entre 1 et le nombre de triplets, avec défaut 1 ; le spawner l'utilise après sa garde d'exclusion scénario/mesure. Le trafic normal garde sa borne existante. Le profil H est une copie en mémoire créée/détruite par le test, accélération 0,375 m/s² et vitesse désirée 8 m/s ; le builder et la conduite réelle utilisent ces paramètres.

- [Story535 EditMode](fixes-r7-editmode-20261007.txt) : **51/51**.
- [Story535 PlayMode](fixes-r7-playmode-20261007.txt) : **7/7**, G de refus conservé et G-gap positif inclus.
- [Non-régression du harnais Story531](fixes-r7-nonreg-5-31-20261007.txt) : **50/50 EditMode + 13/13 PlayMode**. Une lecture de statut CLI retentée automatiquement pendant Domain Reload, puis verdict officiel vert.
- [Preuve G-gap dans MVP_Run](traffic-v2-5-35-explorations/scenario-G-gap-20261007-122808-summary.md) : grant au lot 2359, `t_gap=7,785 s`, `ETA=9,191 s`, titulaire retenu conservant son grant, reprise au lot 2628 après dégagement. **1 477 pas** de poursuite avec l'entrant comme leader, dont **17** contraints par `LeaderFollowing`, jeu positif ; **2/2** retraits, zéro contact entre véhicules/obstacles, aucun dépassement de tolérance. Les trois contacts de relief d'impulsion nulle restent explicitement publiés.

Toutes les validations passent par `scripts/validate.ps1`, zéro erreur Console, compilation saine, MVP_Run propre. Graphify actualisé. Aucun changement de scène, géométrie, authoring, asset de profil ou modèle/signoff ; aucune suite complète exécutée.
