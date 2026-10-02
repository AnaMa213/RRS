using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Frame;
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
        VehicleProfileMissing = 13
    }

    /// <summary>Constantes declarees de la tranche V2 (une seule source).</summary>
    public static class TrafficV2Settings
    {
        public const int V2SliceMaxPopulation = 1;

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
    /// uniquement par les tests PlayMode de la Story 5.31 ; une garde structurelle refuse toute
    /// construction hors de Tests/. Il ne permet aucune conduite normale.
    /// </summary>
    public sealed class MeasurementRun
    {
        public MeasurementKind Kind { get; }
        public string Label { get; }
        public IReadOnlyList<CampaignTriplet> Triplets { get; }

        public MeasurementRun(MeasurementKind kind, string label, IReadOnlyList<CampaignTriplet> triplets)
        {
            if (triplets == null || triplets.Count == 0) throw new ArgumentException("EmptyCampaign", "triplets");
            Kind = kind;
            Label = string.IsNullOrEmpty(label) ? kind.ToString() : label;
            Triplets = new List<CampaignTriplet>(triplets).AsReadOnly();
        }
    }

    /// <summary>
    /// Choix de composition et de mesure d'une session hote. Lu une fois par le spawner, avant la
    /// premiere insertion, puis fige ; tout changement ulterieur est ignore avec un diagnostic.
    /// </summary>
    public static class TrafficV2Session
    {
        public static TrafficComposition Composition { get; private set; }
        public static MeasurementRun Measurement { get; private set; }

        public static void Request(TrafficComposition composition, MeasurementRun measurement)
        {
            Composition = composition;
            Measurement = measurement;
        }

        public static void Reset()
        {
            Composition = TrafficComposition.V1;
            Measurement = null;
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
