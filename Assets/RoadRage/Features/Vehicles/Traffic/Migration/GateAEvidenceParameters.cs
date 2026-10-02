#if UNITY_EDITOR
using System;
using System.Globalization;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    /// <summary>
    /// Parametres d'une preuve Gate A (Story 5.52) : modele de pose, allocation de suivi a_e, grille des ecarts
    /// h_e, tolerance numerique eta, budget d'iterations et entrees de faisabilite. Une seule source : les
    /// preuves, les empreintes et le rapport les lisent ici. <see cref="Legacy"/> reproduit la preuve signee
    /// le 2026-09-28 (pose tangente, a_e = 0) au bit pres.
    /// </summary>
    public sealed class GateAEvidenceParameters
    {
        public const int KinematicPoseModelVersion = 1;

        /// <summary>Pas de la grille des ecarts (radians), declare avant toute mesure (spec 5.52, Design Notes).</summary>
        public const float DeclaredOffsetGridStepRadians = 0.008f;

        /// <summary>Tolerance numerique eta des intervalles d'ecart (radians) : integrations RK4 et interpolation runtime.</summary>
        public const float DeclaredOffsetToleranceRadians = 0.002f;

        public const int DeclaredClosureIterationBudget = 256;

        /// <summary>max|o(s)| : aucune route V2 ne porte de decalage delibere (PathHorizon.MaximumAbsoluteOffsetMeters = 0).</summary>
        public const float MaximumAbsolutePlanningOffsetMeters = 0f;

        public const string V2PrefabPath = "Assets/RoadRage/Prefabs/Greybox_AIVehicle_V2.prefab";

        public static readonly GateAEvidenceParameters Legacy = new GateAEvidenceParameters(
            NominalPoseModel.TangentAligned, 0f, 0f, 0f, 0, default(NominalPoseFeasibilityInputs));

        public NominalPoseModel PoseModel { get; }
        public float TrackingAllowanceMeters { get; }
        public float OffsetGridStepRadians { get; }
        public float OffsetToleranceRadians { get; }
        public int ClosureIterationBudget { get; }
        public NominalPoseFeasibilityInputs Feasibility { get; }

        private GateAEvidenceParameters(NominalPoseModel poseModel, float allowance, float gridStep, float tolerance,
            int budget, NominalPoseFeasibilityInputs feasibility)
        {
            PoseModel = poseModel;
            TrackingAllowanceMeters = allowance;
            OffsetGridStepRadians = gridStep;
            OffsetToleranceRadians = tolerance;
            ClosureIterationBudget = budget;
            Feasibility = feasibility;
        }

        public bool Kinematic { get { return PoseModel == NominalPoseModel.Kinematic; } }

        /// <summary>Parametres de la preuve signee le 2026-09-28 : pose tangente, a_e = 0.</summary>
        public bool IsLegacy { get { return PoseModel == NominalPoseModel.TangentAligned && TrackingAllowanceMeters == 0f; } }

        public int PoseModelVersion { get { return Kinematic ? KinematicPoseModelVersion : 0; } }

        public string PoseModelLabel { get { return Kinematic ? "kinematic-v" + KinematicPoseModelVersion : "tangent-aligned"; } }

        public static GateAEvidenceParameters Create(NominalPoseModel poseModel, float allowance, float gridStep, float tolerance,
            int budget, NominalPoseFeasibilityInputs feasibility)
        {
            if (!Finite(allowance) || allowance < 0f)
                throw new ArgumentOutOfRangeException("allowance", allowance, "a_e fini et positif ou nul attendu.");
            if (poseModel == NominalPoseModel.Kinematic)
            {
                if (!Finite(gridStep) || !(gridStep > 0f))
                    throw new ArgumentOutOfRangeException("gridStep", gridStep, "Pas de grille h_e fini et strictement positif attendu.");
                if (!Finite(tolerance) || tolerance < 0f)
                    throw new ArgumentOutOfRangeException("tolerance", tolerance, "Tolerance eta finie et positive ou nulle attendue.");
                if (budget <= 0)
                    throw new ArgumentOutOfRangeException("budget", budget, "Budget d'iterations strictement positif attendu.");
                if (!feasibility.Valid)
                    throw new ArgumentException("Entrees de faisabilite invalides (taux de braquage, adherence, vitesse desiree).", "feasibility");
                return new GateAEvidenceParameters(poseModel, allowance, gridStep, tolerance, budget, feasibility);
            }

            return new GateAEvidenceParameters(poseModel, allowance, 0f, 0f, 0, default(NominalPoseFeasibilityInputs));
        }

        /// <summary>Allocation a_e = max|o| + epsilon_t declare (contrat §8) ; jamais la marge ni un residu.</summary>
        public static float DeclaredAllowanceMeters()
        {
            TrackingTolerance declared = TrafficV2Settings.DeclaredTrackingTolerance;
            if (!declared.Declared)
                throw new InvalidOperationException("epsilon_t non declare : aucune allocation de suivi possible.");
            return MaximumAbsolutePlanningOffsetMeters + declared.Meters;
        }

        /// <summary>Parametres declares de la regeneration 5.52 : pose cinematique v1, a_e declare, faisabilite du prefab V2.</summary>
        public static GateAEvidenceParameters Declared(GameObject prefab = null)
        {
            return Create(NominalPoseModel.Kinematic, DeclaredAllowanceMeters(), DeclaredOffsetGridStepRadians,
                DeclaredOffsetToleranceRadians, DeclaredClosureIterationBudget, ReadV2Feasibility(prefab));
        }

        /// <summary>Diagnostic intermediaire (phase A) : a_e declare, pose tangente. Ne vaut jamais preuve.</summary>
        public static GateAEvidenceParameters TangentDiagnostic()
        {
            return Create(NominalPoseModel.TangentAligned, DeclaredAllowanceMeters(), 0f, 0f, 0, default(NominalPoseFeasibilityInputs));
        }

        /// <summary>Taux de braquage et adherence du profil vehicule, vitesse desiree du profil conducteur du prefab V2.</summary>
        public static NominalPoseFeasibilityInputs ReadV2Feasibility(GameObject prefab = null)
        {
            if (prefab == null) prefab = AssetDatabase.LoadAssetAtPath<GameObject>(V2PrefabPath);
            if (prefab == null) throw new InvalidOperationException("Prefab V2 absent : " + V2PrefabPath + ".");
            var body = prefab.GetComponent<VehiclePhysicsBody>();
            var vehicleDef = body == null ? null
                : new SerializedObject(body).FindProperty("vehicleProfile").objectReferenceValue as VehicleProfileDef;
            string error = null;
            if (vehicleDef == null || !vehicleDef.TryValidate(out error))
                throw new InvalidOperationException("Profil vehicule du prefab V2 absent ou invalide : " + error);
            var driver = prefab.GetComponent<TrafficV2VehicleDriver>();
            var driverDef = driver == null ? null : driver.DriverProfileDefinition;
            if (driverDef == null || !driverDef.TryValidate(out error))
                throw new InvalidOperationException("Profil conducteur du prefab V2 absent ou invalide : " + error);
            VehicleProfile vehicle = vehicleDef.Profile;
            return new NominalPoseFeasibilityInputs(vehicle.SteerRateDegreesPerSecond,
                vehicle.LateralFrictionCoefficient * Physics.gravity.magnitude, driverDef.Profile.DesiredSpeed);
        }

        /// <summary>Texte canonique hache dans les empreintes et le rapport ; flottants en aller-retour exact.</summary>
        public string CanonicalText
        {
            get
            {
                return "gate-a-evidence-parameters-v1|pose-model=" + PoseModelLabel + "|a_e=" + R(TrackingAllowanceMeters)
                    + "|h_e=" + R(OffsetGridStepRadians) + "|eta=" + R(OffsetToleranceRadians) + "|budget="
                    + ClosureIterationBudget.ToString(CultureInfo.InvariantCulture) + "|steer-rate=" + R(Feasibility.SteerRateDegreesPerSecond)
                    + "|grip=" + R(Feasibility.LateralGripMetersPerSecondSquared) + "|desired-speed=" + R(Feasibility.DesiredSpeedMetersPerSecond);
            }
        }

        private static bool Finite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static string R(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }
    }
}
#endif
