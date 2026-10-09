using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Perception;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEngine;
#if UNITY_EDITOR
using RoadRage.Features.Vehicles.Traffic.Migration;
#endif

namespace RoadRage.Features.Vehicles.Traffic.Lifecycle
{
    /// <summary>Composition du trafic d'une session (A4). V1 est le defaut de MVP_Run, comportement inchange.</summary>
    public enum TrafficComposition { V1 = 0, V2Slice = 1 }

    public enum MeasurementKind { Exploratory = 0, Acceptance = 1 }

    /// <summary>Code nomme d'admission ou de refus d'une insertion V2.</summary>
    public enum TrafficV2Code
    {
        Allowed = 0,
        PlayerBuildHasNoEvidence = 1,
        RoadModelMissing = 2,
        UndeclaredDrivabilityProfile = 3,
        GateAEvidenceMissing = 4,
        GateAEvidenceStale = 5,
        TrackingToleranceUndeclared = 6,
        NotCoveredByGateA = 7,
        DriverProfileMissing = 8,
        FirstDecisionNotDrivable = 9,
        UnknownPortal = 10,
        CampaignCompleted = 11,
        RoadModelInvalid = 12,
        VehicleProfileMissing = 13,
        /// <summary>Story 5.33 : scenario de test et run de mesure demandes ensemble, exclusifs ; aucune insertion.</summary>
        ScenarioWithMeasurement = 14,
        /// <summary>Story 5.33 : toutes les insertions du scenario ont eu lieu.</summary>
        ScenarioCompleted = 15
    }

    /// <summary>Constantes declarees de la tranche V2 (une seule source).</summary>
    public static class TrafficV2Settings
    {
        /// <summary>Population de production de la composition V2Slice (D2) ; seul un scenario de test la leve.</summary>
        public const int V2SliceMaxPopulation = 1;

        // Valeurs runtime 5.33 : parametres de conception, confirmes par les totaux et saturations publies par le
        // harness. Toute revision est une decision proprietaire (Ask First).

        /// <summary>Portee arriere de la perception (m).</summary>
        public const float PerceptionRearRangeMeters = 30f;

        /// <summary>Elargissement lateral du couloir balaye de la perception (m).</summary>
        public const float PerceptionLateralRangeMeters = 1f;

        /// <summary>Fenetre longitudinale de l'occupation adjacente (m).</summary>
        public const float PerceptionAdjacentWindowMeters = 10f;

        /// <summary>Capacite des listes de faits de la perception.</summary>
        public const int PerceptionListCapacity = 8;

        /// <summary>Capacite du tampon de requete spatiale de la perception, un par vehicule, reutilise.</summary>
        public const int SpatialQueryCapacity = 32;

        /// <summary>Rayon de la requete du collecteur de dangers, une par vehicule V2 et par pas (m).</summary>
        public const float HazardQueryRadiusMeters = 40f;

        /// <summary>
        /// Capacite du tampon du collecteur : une requete pleine est saturee. Revisee de 64 a 256 par decision
        /// proprietaire du 2026-10-02 (D9) : le decor statique de MVP_Run rend jusqu'a 98 colliders a 40 m le long des
        /// routes de reference (97 statiques) et saturait 64 sur 53 % des requetes.
        /// </summary>
        public const int HazardQueryCapacity = 256;

        /// <summary>Population maximale d'un scenario de test (D2).</summary>
        public const int ScenarioMaxPopulation = 8;

        /// <summary>
        /// Maintien a l'arret D11 (decision proprietaire du 2026-10-02). Vitesses d'entree et de depart de la source :
        /// <see cref="VehicleTireModel.SlipReferenceSpeed"/>, sous laquelle l'adherence laterale du pneu est attenuee
        /// (aucun mouvement entretenu dans cette bande : c'est le rampement qui faisait deriver d au scenario A). Delta_hold
        /// et Delta_release : calibres par Story533LongitudinalTests.TheStopHoldWindowIsCalibratedOnThePointMassBench
        /// (rapport stophold-calibration.md) ; Delta_hold au plus 0,5 m, borne haute du jeu d'une file d'acceptation.
        /// </summary>
        public static StopHoldParameters StopHold
        {
            get
            {
                return new StopHoldParameters(VehicleTireModel.SlipReferenceSpeed, StopHoldGapMarginMeters, StopHoldReleaseGapMarginMeters,
                    VehicleTireModel.SlipReferenceSpeed);
            }
        }

        /// <summary>
        /// Delta_hold : fenetre d'entree du maintien au-dessus de s0 (m). A 0,25 m, l'arret en roue libre de la bande de
        /// service (vers s0 + 0,4..0,5) laisse 0,15 m de rampement ; a 0,5 m, aucun, jeu maintenu 2,45 m.
        /// </summary>
        public const float StopHoldGapMarginMeters = 0.5f;

        /// <summary>
        /// Marge m de la portee bornee de la planification (D14, 2026-10-02) : H = d1 + v_ref^2 / (2 b_plan) + m. Absorbe
        /// l'arrondi de la chaine arriere du plan de vitesse ; banc Story533PlanningCostBenchTests : 7 838 commandes identiques.
        /// </summary>
        public const float PlanningReachMarginMeters = 1f;

        /// <summary>
        /// Delta_release : ouverture du jeu au-dessus de s0 qui libere un maintien (m). Seule une source lente (&lt; vitesse de
        /// depart) y recourt ; derriere une source a 0,05 m/s pendant 60 s : 4 liberations a 1 m, 2 a 1,5 m, 1 a 2 m.
        /// </summary>
        public const float StopHoldReleaseGapMarginMeters = 2f;

        /// <summary>
        /// m_ctrl, marge de controle longitudinal de l'arret avant une traversee sans grant (Story 5.34) : seule valeur declaree
        /// des distances de coordination. Elle couvre la montee du frein et l'ecart de suivi longitudinal, absents du modele
        /// point-masse ; declaree avant mesure, les scenarios C et D publient la distance minimale a l'entree pendant un refus.
        /// Toute revision est une decision proprietaire (Ask First). Decision O14 (2026-10-03) : 0,5 -> 0,25 m, arret a la limite
        /// de l'entree, a_kin conduisant la fin de l'approche.
        /// </summary>
        public const float JunctionStopControlMarginMeters = 0.25f;

        public static PerceptionLimits PerceptionLimits
        {
            get
            {
                return new PerceptionLimits(PerceptionRearRangeMeters, PerceptionLateralRangeMeters,
                    PerceptionAdjacentWindowMeters, PerceptionListCapacity);
            }
        }

        /// <summary>Fenetre de validite d'une commande, en pas physiques : [p, p].</summary>
        public const int PlanValiditySteps = 1;

        /// <summary>L'horizon couvre toute la route restante ; au-dela, fin LookAheadLimit et vitesse terminale 0.</summary>
        public const float LookAheadMeters = 100000f;

        /// <summary>
        /// Tolerance de suivi epsilon_t : constante unique, declaree par le proprietaire apres la campagne
        /// exploratoire (etape 2 du protocole). Non declaree : aucune conduite hors run de mesure.
        /// Declaree par le proprietaire le 2026-09-30 : 0,34 m, au-dessus du maximum mesure sur le code corrige
        /// (0,3306 m entre deux pas, 0,3296 m au pas ; `exploratory-20260930-133627` et campagnes du meme jour).
        /// Hors mesure, le refus reste NotCoveredByGateA tant que la 5.52 n'a pas integre a_e dans la preuve.
        /// </summary>
        public static TrackingTolerance DeclaredTrackingTolerance { get { return new TrackingTolerance(0.34f); } }

        // Story 5.39 (decision D1, 2026-10-07) : seuils declares de la recuperation, calibres par la recette Gate D. Toute
        // revision est une decision proprietaire (Ask First).
        /// <summary>R2 : progression attendue E qui rend eligible un vehicule qui n'a pas avance (m).</summary>
        public const float RecoveryExpectedProgressMeters = 2f;
        /// <summary>R2 : avance reelle A qui prouve une progression et remet l'episode a zero (m).</summary>
        public const float RecoveryMinimumProgressMeters = 0.5f;
        /// <summary>R4 : tentatives (acceptees ou refusees) par episode avant Faulted.</summary>
        public const int RecoveryMaxAttempts = 4;
        /// <summary>R4 : refus consecutifs qui donnent Faulted.</summary>
        public const int RecoveryMaxConsecutiveRejections = 2;
        /// <summary>R5 : vitesse visee du realignement en marche avant (m/s).</summary>
        public const float RecoveryRealignSpeedMetersPerSecond = 2f;
        /// <summary>R5 : parcours du realignement au-dela duquel il echoue en NoProgress (m).</summary>
        public const float RecoveryRealignMaxTravelMeters = 20f;
        /// <summary>R5 : vitesse visee du recul controle (m/s, en valeur absolue).</summary>
        public const float RecoveryReverseSpeedMetersPerSecond = 1f;
        /// <summary>R5 : parcours du recul controle (m).</summary>
        public const float RecoveryReverseTravelMeters = 3f;

        /// <summary>
        /// Story 5.40 (G4, decision D1 du 2026-10-08) : paliers d'escalade d'interblocage, authores et tous actifs, appliques dans
        /// cet ordre par le coordinateur. Tout ajout est une decision proprietaire (Ask First) ; 5.41 et 5.42 en sont les extensions.
        /// </summary>
        public static readonly IReadOnlyList<Coordination.GridlockEscalationTier> GridlockEscalationTiers =
            Array.AsReadOnly(new[] { Coordination.GridlockEscalationTier.PrecedenceRelaxation });

        /// <summary>
        /// Story 5.41 (decision D2 du 2026-10-09) : seules regles que l'autorite peut accepter de voir derogees. Tout ajout est une
        /// decision proprietaire (Ask First) ; KeepClear reste exclue (grant cyclique, 5.40 D1).
        /// </summary>
        public static readonly IReadOnlyList<Policy.TrafficRule> ViolableTrafficRules =
            Array.AsReadOnly(new[] { Policy.TrafficRule.OpposingCorridor });

        /// <summary>
        /// Story 5.41 (D3) : duree maximale d'une exception, en pas hote. Porte de 500 a 1000 pas (20 s a 50 Hz) par la decision
        /// proprietaire 4A du 2026-10-09 (Story 5.42) : un contournement par le corridor oppose depuis l'approche ne tenait pas en
        /// 10 s. Revision : Ask First.
        /// </summary>
        public const int MaxRuleExceptionFrames = 1000;

        // Story 5.42 (decisions D1 a D4 approuvees le 2026-10-09) : reglages des manoeuvres non structurees. Revision : Ask First.
        /// <summary>D1 : duree minimale de la meme cause liante (leader ou obstacle) avant une evaluation (s).</summary>
        public const float ManeuverConsiderSeconds = 2f;
        /// <summary>D1 : vitesse de la cause au plus cette fraction de la vitesse desiree.</summary>
        public const float ManeuverSlowSpeedFraction = 0.5f;
        /// <summary>D2 : acceleration laterale admise sur la reference de manoeuvre (m/s2).</summary>
        public const float ManeuverLateralAccelerationMetersPerSecondSquared = 2f;
        /// <summary>D2 : degagement longitudinal avant et apres la cause (m).</summary>
        public const float ManeuverLongitudinalClearanceMeters = 1f;
        /// <summary>D2 : degagement lateral vis-a-vis de la cause (m).</summary>
        public const float ManeuverLateralClearanceMeters = 0.5f;
        /// <summary>D2 : pas de la preuve continue, en abscisse de reference (m).</summary>
        public const float ManeuverProofStepMeters = 0.1f;
        /// <summary>D3 : vitesse de rapprochement minimale v_m - v_cause d'un depassement (m/s).</summary>
        public const float ManeuverMinimumClosingSpeedMetersPerSecond = 2f;
        /// <summary>D4 : marge ajoutee a la duree de manoeuvre pour l'expiration de l'exception (s).</summary>
        public const float ManeuverExceptionMarginSeconds = 2f;
        /// <summary>D4 : delai avant une nouvelle demande apres un refus de l'autorite (s).</summary>
        public const float ManeuverRetrySeconds = 2f;

        /// <summary>Verification du modele M par pas : ecart de position du centre de masse (m).</summary>
        public const float ModelPositionToleranceMeters = 0.002f;

        /// <summary>Verification du modele M par pas : ecart d'orientation (degres).</summary>
        public const float ModelRotationToleranceDegrees = 0.05f;

        /// <summary>Reste de Lipschitz vise entre deux pas (m) : h est choisi pour que L h / 2 &lt;= 1 mm.</summary>
        public const float InterStepRemainderMeters = 0.001f;

        public const string ModelPath = "Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json";
        public const string SignoffPath = "Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-signoff.json";
        public const string ReportPath = "_bmad-output/implementation-artifacts/migration-report-5-28-mvp-run.md";
    }

    public readonly struct CampaignTriplet
    {
        public readonly RoadId EntryPortalId;
        public readonly RoadId ExitPortalId;
        public readonly ulong Seed;
        /// <summary>Mouvement vise (objectif intermediaire, contrat §4) ; vide : route ordinaire vers la sortie.</summary>
        public readonly RoadId ViaMovementId;

        public CampaignTriplet(RoadId entryPortalId, RoadId exitPortalId, ulong seed, RoadId viaMovementId = default(RoadId))
        { EntryPortalId = entryPortalId; ExitPortalId = exitPortalId; Seed = seed; ViaMovementId = viaMovementId; }
    }

    /// <summary>
    /// Jeton de run de mesure : il porte la campagne (entree, sortie imposee, graine). Construit
    /// uniquement par les tests ; une garde structurelle refuse toute
    /// construction hors de Tests/. Il ne permet aucune conduite normale.
    /// </summary>
    public sealed class MeasurementRun
    {
        public MeasurementKind Kind { get; }
        public string Label { get; }
        public IReadOnlyList<CampaignTriplet> Triplets { get; }
        public int MaxPopulation { get; }

        public MeasurementRun(MeasurementKind kind, string label, IReadOnlyList<CampaignTriplet> triplets, int maxPopulation = 1)
        {
            if (triplets == null || triplets.Count == 0) throw new ArgumentException("EmptyCampaign", "triplets");
            if (maxPopulation < 1 || maxPopulation > triplets.Count)
                throw new ArgumentOutOfRangeException(nameof(maxPopulation));
            Kind = kind;
            Label = string.IsNullOrEmpty(label) ? kind.ToString() : label;
            Triplets = new List<CampaignTriplet>(triplets).AsReadOnly();
            MaxPopulation = maxPopulation;
        }
    }

    /// <summary>Insertion d'un scenario de test : entree, sortie imposee, graine et pas hote d'insertion au plus tot.</summary>
    public readonly struct ScenarioInsertion
    {
        public readonly RoadId EntryPortalId;
        public readonly RoadId ExitPortalId;
        public readonly ulong Seed;
        /// <summary>FrameId global a partir duquel l'insertion peut avoir lieu (portail libre exige en plus).</summary>
        public readonly ulong EarliestStep;

        public ScenarioInsertion(RoadId entryPortalId, RoadId exitPortalId, ulong seed, ulong earliestStep)
        {
            EntryPortalId = entryPortalId; ExitPortalId = exitPortalId; Seed = seed; EarliestStep = earliestStep;
        }
    }

    /// <summary>Insertion effective d'un scenario : rang dans le scenario, identite et FrameId global d'insertion.</summary>
    public readonly struct ScenarioInsertionRecord
    {
        public readonly int Index;
        public readonly RoadId TrafficId;
        /// <summary>FrameId global du pas hote ou l'insertion a eu lieu ; le vehicule conduit au pas suivant.</summary>
        public readonly ulong FrameId;

        public ScenarioInsertionRecord(int index, RoadId trafficId, ulong frameId)
        {
            Index = index; TrafficId = trafficId; FrameId = frameId;
        }
    }

    /// <summary>
    /// Jeton de scenario de test (Story 5.33) : etiquette, population maximale (1 a
    /// <see cref="TrafficV2Settings.ScenarioMaxPopulation"/>) et insertions ordonnees. Construit uniquement sous Tests/
    /// (garde structurelle, comme MeasurementRun). Il n'accorde aucune permission : insertion hors mesure, couverture
    /// Covered exigee, repli 2a actif ; aucun objectif intermediaire. Exclusif avec MeasurementRun (refus nomme).
    /// </summary>
    public sealed class TrafficV2Scenario
    {
        public string Label { get; }
        public int MaxPopulation { get; }
        public IReadOnlyList<ScenarioInsertion> Insertions { get; }

        public TrafficV2Scenario(string label, int maxPopulation, IReadOnlyList<ScenarioInsertion> insertions)
        {
            if (insertions == null || insertions.Count == 0) throw new ArgumentException("EmptyScenario", "insertions");
            if (maxPopulation < 1 || maxPopulation > TrafficV2Settings.ScenarioMaxPopulation)
                throw new ArgumentException("InvalidScenarioPopulation", "maxPopulation");
            for (int i = 0; i < insertions.Count; i++)
                if (insertions[i].EntryPortalId.IsEmpty || insertions[i].ExitPortalId.IsEmpty)
                    throw new ArgumentException("InvalidScenarioInsertion", "insertions");
            Label = string.IsNullOrEmpty(label) ? "scenario" : label;
            MaxPopulation = maxPopulation;
            Insertions = new List<ScenarioInsertion>(insertions).AsReadOnly();
        }
    }

    /// <summary>
    /// Choix de composition, de mesure et de scenario d'une session hote. Lu une fois par le spawner, avant la
    /// premiere insertion, puis fige ; tout changement ulterieur est ignore avec un diagnostic.
    /// </summary>
    public static class TrafficV2Session
    {
        public static TrafficComposition Composition { get; private set; }
        public static MeasurementRun Measurement { get; private set; }
        /// <summary>Scenario de test (5.33) ; nul en production : population 1.</summary>
        public static TrafficV2Scenario Scenario { get; private set; }

        public static void Request(TrafficComposition composition, MeasurementRun measurement, TrafficV2Scenario scenario = null)
        {
            Composition = composition;
            Measurement = measurement;
            Scenario = scenario;
        }

        public static void Reset()
        {
            Composition = TrafficComposition.V1;
            Measurement = null;
            Scenario = null;
        }
    }

    /// <summary>Modele et preuve admis pour la conduite V2, avec un code nomme.</summary>
    public sealed class TrafficV2Admission
    {
        public TrafficV2Code Code { get; }
        public CompiledRoadModel Model { get; }
        public GateAEvidenceResult Evidence { get; }
        public bool Admitted { get { return Code == TrafficV2Code.Allowed; } }

        internal TrafficV2Admission(TrafficV2Code code, CompiledRoadModel model, GateAEvidenceResult evidence)
        { Code = code; Model = model; Evidence = evidence; }
    }

    /// <summary>Verdict d'insertion : code, couverture vehicule publiee et etiquette de mesure.</summary>
    public readonly struct TrafficV2Verdict
    {
        public readonly TrafficV2Code Code;
        public readonly VehicleCoverage VehicleCoverage;
        public readonly string MeasurementLabel;
        public bool Allowed { get { return Code == TrafficV2Code.Allowed; } }

        public TrafficV2Verdict(TrafficV2Code code, VehicleCoverage coverage, string measurementLabel)
        { Code = code; VehicleCoverage = coverage; MeasurementLabel = measurementLabel; }
    }

    /// <summary>Insertion preparee : pose de portail, identite, graine et premiere decision conduisible.</summary>
    public sealed class TrafficV2Insertion
    {
        public TrafficV2Code Code { get; internal set; }
        public RoadId TrafficId { get; internal set; }
        public RouteSeed Seed { get; internal set; }
        public Portal EntryPortal { get; internal set; }
        public RoadId ExitPortalId { get; internal set; }
        public Vector3 Position { get; internal set; }
        public Quaternion Rotation { get; internal set; }
        public RoutePlan Route { get; internal set; }
        public SpeedPlan FirstSpeedPlan { get; internal set; }
        /// <summary>Objectif intermediaire de la campagne de mesure ; vide hors mesure.</summary>
        public RoadId ViaMovementId { get; internal set; }
    }

    public static class TrafficV2Lifecycle
    {
        private static string cachedKey;
        private static TrafficV2Admission cachedAdmission;

        /// <summary>Identite deterministe d'un vehicule : (graine de session, compteur d'insertion).</summary>
        public static RoadId TrafficIdentity(ulong sessionSeed, ulong insertionCounter)
        {
            return new RoadId(sessionSeed, insertionCounter == 0UL ? 1UL : insertionCounter);
        }

        /// <summary>Admission pure : Load puis Compile, puis liaison documentaire Gate A sur les trois textes.</summary>
        public static TrafficV2Admission Admit(string modelText, string signoffText, string reportText,
            string currentEvidenceParametersHash = null)
        {
            if (string.IsNullOrEmpty(modelText))
                return new TrafficV2Admission(TrafficV2Code.RoadModelMissing, null, default(GateAEvidenceResult));
            CompiledRoadModel model;
            try { model = RoadModelCompiler.Compile(RoadModelDocument.Load(modelText)); }
            catch (FormatException)
            { return new TrafficV2Admission(TrafficV2Code.UndeclaredDrivabilityProfile, null, default(GateAEvidenceResult)); }
            catch (RoadModelCompilationException)
            { return new TrafficV2Admission(TrafficV2Code.RoadModelInvalid, null, default(GateAEvidenceResult)); }
            if (!model.DrivabilityProfile.Declared)
                return new TrafficV2Admission(TrafficV2Code.UndeclaredDrivabilityProfile, model, default(GateAEvidenceResult));
            var evidence = GateAEvidenceBinding.Bind(model, modelText, signoffText, reportText, currentEvidenceParametersHash);
            var code = evidence.Status == GateAEvidenceStatus.Valid ? TrafficV2Code.Allowed
                : evidence.Status == GateAEvidenceStatus.GateAEvidenceStale ? TrafficV2Code.GateAEvidenceStale
                : TrafficV2Code.GateAEvidenceMissing;
            return new TrafficV2Admission(code, model, evidence);
        }

        /// <summary>
        /// Admission de la session : les trois textes commites sont lus dans l'Editeur seulement, et le
        /// resultat est mis en cache par modele. Un build joueur n'a aucune preuve : aucun vehicule V2.
        /// </summary>
        public static TrafficV2Admission AdmitCommittedArtifacts(GameObject prefab = null)
        {
#if UNITY_EDITOR
            string model = ReadOrNull(TrafficV2Settings.ModelPath);
            string signoff = ReadOrNull(TrafficV2Settings.SignoffPath);
            string report = ReadOrNull(TrafficV2Settings.ReportPath);
            string parametersHash;
            try { parametersHash = V1SourceSet.Sha256Hex(GateAEvidenceParameters.Declared(prefab).CanonicalText); }
            catch (Exception exception) when (exception is InvalidOperationException || exception is ArgumentException)
            { return new TrafficV2Admission(TrafficV2Code.GateAEvidenceStale, null,
                new GateAEvidenceResult(GateAEvidenceStatus.GateAEvidenceStale, 0f)); }
            string key = (model ?? "") + "\u0000" + (signoff ?? "") + "\u0000" + (report ?? "") + "\u0000" + parametersHash;
            if (cachedAdmission != null && string.Equals(key, cachedKey, StringComparison.Ordinal)) return cachedAdmission;
            cachedAdmission = Admit(model, signoff, report, parametersHash);
            cachedKey = key;
            return cachedAdmission;
#else
            return new TrafficV2Admission(TrafficV2Code.PlayerBuildHasNoEvidence, null, default(GateAEvidenceResult));
#endif
        }

#if UNITY_EDITOR
        private static string ReadOrNull(string path)
        {
            try { return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null; }
            catch (System.IO.IOException) { return null; }
        }
#endif

        /// <summary>
        /// Verdict d'insertion. Sous run de mesure, l'insertion est permise (etiquetee) ; sinon il faut une
        /// couverture vehicule etablie, impossible tant qu'epsilon_t n'est pas declare ou que a_e = 0.
        /// </summary>
        public static TrafficV2Verdict EvaluateInsertion(TrafficV2Admission admission, MeasurementRun measurement,
            TrackingTolerance declared)
        {
            if (admission == null)
                return new TrafficV2Verdict(TrafficV2Code.RoadModelMissing, VehicleCoverage.NotEstablished, null);
            if (!admission.Admitted)
                return new TrafficV2Verdict(admission.Code, VehicleCoverage.NotEstablished, null);
            var coverage = MotionPlan.EvaluateVehicleCoverage(admission.Evidence, 0f, declared);
            if (measurement != null)
                return new TrafficV2Verdict(TrafficV2Code.Allowed, coverage, measurement.Kind + ":" + measurement.Label);
            if (coverage == VehicleCoverage.Covered)
                return new TrafficV2Verdict(TrafficV2Code.Allowed, coverage, null);
            return new TrafficV2Verdict(coverage == VehicleCoverage.TrackingToleranceUndeclared
                ? TrafficV2Code.TrackingToleranceUndeclared : TrafficV2Code.NotCoveredByGateA, coverage, null);
        }

        /// <summary>
        /// Elements attendus pour le bonus de route de la localisation (AD-45) : de l'occurrence courante au
        /// prochain mouvement inclus, plus le corridor qui le suit. Une route a cycle legal (objectif intermediaire,
        /// tour d'anneau) repasse par un meme raccord : un bonus sur toute la route favoriserait a ce raccord le
        /// mouvement d'un passage ulterieur et ferait sauter la progression.
        /// </summary>
        public static RoadId[] ExpectedElements(RoutePlan plan)
        {
            if (plan == null || plan.Occurrences.Count == 0) return null;
            var ids = new List<RoadId>();
            int movements = 0;
            for (int i = Math.Max(0, plan.ProgressOccurrenceIndex); i < plan.Occurrences.Count; i++)
            {
                var occurrence = plan.Occurrences[i];
                if (i > plan.ProgressOccurrenceIndex && occurrence.Kind == RoadElementKind.JunctionMovement && ++movements > 1) break;
                ids.Add(occurrence.Id);
                if (movements == 1 && occurrence.Kind == RoadElementKind.LaneCorridor) break;
            }
            return ids.ToArray();
        }

        /// <summary>Retrait : point de reference localise sur le corridor du portail de sortie, a s &gt;= Portal.SMeters.</summary>
        public static bool HasReachedExit(RoadLocation location, Portal exit)
        {
            return location.Localized && location.ElementKind == RoadElementKind.LaneCorridor
                && location.ElementId == exit.CorridorId && !exit.CorridorId.IsEmpty && location.SMeters >= exit.SMeters;
        }

        /// <summary>Pose de portail : point de reference au s du portail, cap tangent.</summary>
        public static bool TryPortalPose(CompiledRoadModel model, RoadId portalId, out Portal portal,
            out Vector3 position, out Quaternion rotation)
        {
            portal = default(Portal); position = Vector3.zero; rotation = Quaternion.identity;
            for (int i = 0; i < model.Portals.Count; i++)
            {
                if (model.Portals[i].Id != portalId) continue;
                EffectiveLaneCorridor corridor;
                if (!model.TryGetCorridor(model.Portals[i].CorridorId, out corridor)) return false;
                portal = model.Portals[i];
                var point = corridor.Curve.Sample(portal.SMeters);
                position = point.Position;
                rotation = Quaternion.LookRotation(point.Tangent, point.Up);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Prepare une insertion : pose au portail d'entree, vitesse nulle, et premiere decision
        /// (frame 0) evaluee. L'insertion n'a lieu que si cette decision est conduisible.
        /// </summary>
        /// <param name="viaMovementId">Objectif intermediaire : seul un triplet de campagne de mesure en porte un.</param>
        public static TrafficV2Insertion PrepareInsertion(TrafficV2Admission admission, RoadId entryPortalId,
            RoadId exitPortalId, ulong seed, ulong insertionCounter, DriverProfile driver, float fixedDeltaTime,
            RoadId viaMovementId = default(RoadId))
        {
            var insertion = new TrafficV2Insertion { Seed = new RouteSeed(seed), ExitPortalId = exitPortalId,
                TrafficId = TrafficIdentity(seed, insertionCounter), ViaMovementId = viaMovementId };
            Portal entry; Vector3 position; Quaternion rotation;
            if (admission == null || admission.Model == null
                || !TryPortalPose(admission.Model, entryPortalId, out entry, out position, out rotation)
                || entry.Role != PortalRole.Entry)
            { insertion.Code = TrafficV2Code.UnknownPortal; return insertion; }
            insertion.EntryPortal = entry; insertion.Position = position; insertion.Rotation = rotation;
            var pose = new VehicleFootprintPose { Position = position, Forward = rotation * Vector3.forward,
                Up = rotation * Vector3.up };
            var frame = new TrafficFrame(0UL, admission.Model, new[] {
                new TrafficActorInput(insertion.TrafficId, pose, 0f, entry.CorridorId) });
            var decision = PlanningSpine.Evaluate(new PlanningRequest(frame, insertion.TrafficId, null, exitPortalId,
                insertion.Seed, TrafficV2Settings.LookAheadMeters, null, null, null, driver,
                TrackingTolerance.Undeclared, null, null, admission.Evidence, viaMovementId, 0f));
            insertion.Route = decision.Route.Plan;
            if (decision.Motion == null) { insertion.Code = TrafficV2Code.FirstDecisionNotDrivable; return insertion; }
            var plan = SpeedPlan.Build(decision.Motion, admission.Model, driver, 0f);
            insertion.FirstSpeedPlan = plan;
            insertion.Code = plan.Accepted ? TrafficV2Code.Allowed : TrafficV2Code.FirstDecisionNotDrivable;
            return insertion;
        }
    }
}
