using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Migration;
using RoadRage.Features.Vehicles.Traffic.Planning;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.52 -- allocation de suivi a_e dans le gonflement de chaque preuve, empreinte de paire v2, liaison de
    /// la preuve signee et reponse de fonctionnement normal a TrackingToleranceExceeded (decision 2a). Cas purs.
    /// </summary>
    [Category("Core")]
    [Category("Story552")]
    public sealed class Story552EvidenceTests
    {
        private const float Allowance = 0.34f;

        private static RoadModelValidationProfile Profile()
        {
            var profile = new RoadModelValidationProfile();
            profile.MaxVehicleHalfWidthMeters = 1.03f;
            profile.MaxVehicleLengthMeters = 4.5f;
            profile.LateralClearanceMarginMeters = 0.25f;
            return profile;
        }

        private static DrivabilityProfile Drivability()
        {
            return new DrivabilityProfile
            {
                Declared = true,
                WheelbaseMeters = 3.1f,
                ReferencePointAheadRearAxleMeters = 1.55f,
                LowSpeedLockDegrees = 40f,
                HighSpeedLockDegrees = 16f,
                FullReductionSpeedMetersPerSecond = 26f,
                SteeringInactiveBelowMetersPerSecond = 0.25f
            };
        }

        private static NominalPoseFeasibilityInputs Feasibility()
        {
            return new NominalPoseFeasibilityInputs(300f, 9.81f, 8f);
        }

        private static GateAEvidenceParameters Kinematic(float allowance = Allowance, float step = 0.008f, float eta = 0.002f)
        {
            return GateAEvidenceParameters.Create(NominalPoseModel.Kinematic, allowance, step, eta, 256, Feasibility());
        }

        private static List<SweepPose> Line(float x0, float z0, float x1, float z1, float step)
        {
            var poses = new List<SweepPose>();
            float length = new Vector2(x1 - x0, z1 - z0).magnitude;
            int count = Mathf.Max(1, Mathf.CeilToInt(length / step));
            var tangent = new Vector3(x1 - x0, 0f, z1 - z0).normalized;
            for (int i = 0; i <= count; i++)
            {
                float t = (float)i / count;
                poses.Add(SweepPose.From(new Vector3(Mathf.Lerp(x0, x1, t), 0f, Mathf.Lerp(z0, z1, t)), tangent));
            }

            return poses;
        }

        private static List<List<SweepPose>> One(List<SweepPose> path)
        {
            return new List<List<SweepPose>> { path };
        }

        private static Vector2[] Rect(float xMin, float xMax, float zMin, float zMax)
        {
            return new[] { new Vector2(xMin, zMin), new Vector2(xMax, zMin), new Vector2(xMax, zMax), new Vector2(xMin, zMax) };
        }

        private static List<RoadCurveSample> Straight(float x0, float z0, float x1, float z1, int count)
        {
            var samples = new List<RoadCurveSample>();
            var tangent = new Vector3(x1 - x0, 0f, z1 - z0).normalized;
            float length = new Vector2(x1 - x0, z1 - z0).magnitude;
            for (int i = 0; i <= count; i++)
            {
                float t = (float)i / count;
                samples.Add(new RoadCurveSample
                {
                    SMeters = length * t,
                    Position = new Vector3(Mathf.Lerp(x0, x1, t), 0f, Mathf.Lerp(z0, z1, t)),
                    Tangent = tangent,
                    Up = Vector3.up,
                    HalfWidthLeftMeters = 1.75f,
                    HalfWidthRightMeters = 1.75f
                });
            }

            return samples;
        }

        [Test]
        public void TheDeclaredAllowanceIsTheDeclaredToleranceAboveAZeroPlanningOffset()
        {
            Assert.That(GateAEvidenceParameters.DeclaredAllowanceMeters(), Is.EqualTo(TrafficV2Settings.DeclaredTrackingTolerance.Meters));
            Assert.That(GateAEvidenceParameters.DeclaredAllowanceMeters(), Is.EqualTo(Allowance), "a_e = max|o| (0) + epsilon_t (0,34 m).");
            Assert.That(GateAEvidenceParameters.Legacy.IsLegacy, Is.True);
            Assert.That(GateAEvidenceParameters.Legacy.TrackingAllowanceMeters, Is.EqualTo(0f));

            var diagnostic = GateAEvidenceParameters.TangentDiagnostic();
            Assert.That(diagnostic.IsLegacy, Is.False, "Le diagnostic porte a_e : jamais la preuve signee.");
            Assert.That(diagnostic.Kinematic, Is.False);

            var declared = GateAEvidenceParameters.Declared();
            Assert.That(declared.Kinematic, Is.True);
            Assert.That(declared.TrackingAllowanceMeters, Is.EqualTo(Allowance));
            Assert.That(declared.OffsetGridStepRadians, Is.EqualTo(GateAEvidenceParameters.DeclaredOffsetGridStepRadians));
            Assert.That(declared.OffsetToleranceRadians, Is.EqualTo(GateAEvidenceParameters.DeclaredOffsetToleranceRadians));
            Assert.That(declared.Feasibility.Valid, Is.True, "Faisabilite lue sur le prefab V2.");
        }

        [Test]
        public void AnAllowanceEnlargesTheConflictSweepInflationExactly()
        {
            var profile = Profile();
            var a = Line(0f, 0f, 20f, 0f, 0.5f);
            var b = Line(0f, 2.56f, 20f, 2.56f, 0.5f);
            var legacy = ConflictSweep.Evaluate(One(a), One(b), profile);
            var allowed = ConflictSweep.Evaluate(One(a), One(b), profile, null, 0.3f);
            Assert.That(legacy.BaseInflationMeters, Is.EqualTo(ConflictSweep.Inflation(profile)), "Historique inchange.");
            Assert.That(allowed.BaseInflationMeters, Is.EqualTo(ConflictSweep.Inflation(profile) + 0.3f));
            Assert.That(allowed.ExactSlackMeters - legacy.ExactSlackMeters, Is.EqualTo(-0.6f).Within(1e-5f), "Chaque empreinte gagne exactement a_e.");
            Assert.That(allowed.EnvelopeSlackMeters - legacy.EnvelopeSlackMeters, Is.EqualTo(-0.6f).Within(1e-5f));

            Assert.That(ConflictSweep.EvidenceInflation(profile, GateAEvidenceParameters.Legacy), Is.EqualTo(ConflictSweep.Inflation(profile)));
            Assert.That(ConflictSweep.EvidenceInflation(profile, Kinematic(0.3f)),
                Is.EqualTo(ConflictSweep.Inflation(profile) + 0.3f + ConflictSweep.Rho(profile) * 0.008f * 0.5f).Within(1e-6f),
                "Pose cinematique : le reste de grille rho.h_e/2 s'ajoute, jamais pris sur la marge.");
        }

        [Test]
        public void AnAllowanceEnlargesTheJunctionClearanceInflationExactly()
        {
            var profile = Profile();
            var poses = Line(0f, 0f, 10f, 0f, 0.05f);
            var obstacle = Rect(2f, 8f, 3f, 4f);
            Assert.That(JunctionClearance.HalfLength(profile, 0f), Is.EqualTo(JunctionClearance.HalfLength(profile)));
            Assert.That(JunctionClearance.HalfWidth(profile, 0.3f), Is.EqualTo(JunctionClearance.HalfWidth(profile) + 0.3f));
            var legacy = JunctionClearance.MeasurePath(poses, new[] { obstacle }, profile, "box");
            var allowed = JunctionClearance.MeasurePath(poses, new[] { obstacle }, JunctionClearance.HalfLength(profile, 0.3f),
                JunctionClearance.HalfWidth(profile, 0.3f), "box");
            Assert.That(legacy.Residual, Is.GreaterThan(0f));
            Assert.That(allowed.Residual - legacy.Residual, Is.EqualTo(-0.3f).Within(1e-4f), "Le residu recule exactement de a_e.");
        }

        [Test]
        public void AnOverlapIsMeasuredByItsPenetrationDepthNotClampedToZero()
        {
            var a = Rect(0f, 4f, 0f, 2f);
            Assert.That(JunctionClearance.SignedDistance(a, Rect(5f, 6f, 0f, 2f)), Is.EqualTo(1f).Within(1e-5f), "Separes : distance exacte.");
            Assert.That(JunctionClearance.SignedDistance(a, Rect(3.7f, 6f, 0f, 2f)), Is.EqualTo(-0.3f).Within(1e-5f), "Recouvrement de 0,3 m.");
            Assert.That(JunctionClearance.SignedDistance(a, Rect(1f, 3f, 1.4f, 5f)), Is.EqualTo(-0.6f).Within(1e-5f), "Plus petit recouvrement des axes.");
            Assert.That(JunctionClearance.Distance(a, Rect(3.7f, 6f, 0f, 2f)), Is.EqualTo(0f), "La distance historique reste nulle en contact.");

            var poses = Line(0f, 0f, 10f, 0f, 0.05f);
            var profile = Profile();
            var overlapping = Rect(2f, 8f, 1.0f, 3f);
            float hl = JunctionClearance.HalfLength(profile);
            float hw = JunctionClearance.HalfWidth(profile);
            var clamped = JunctionClearance.MeasurePath(poses, new[] { overlapping }, hl, hw, "box");
            var signed = JunctionClearance.MeasurePath(poses, new[] { overlapping }, hl, hw, "box", true);
            Assert.That(clamped.Residual, Is.EqualTo(-0.025f).Within(1e-5f), "Historique : plafonne a -delta/2.");
            Assert.That(signed.Residual, Is.EqualTo(-(hw - 1f) - 0.025f).Within(1e-4f), "Signe : penetration chiffree.");
        }

        [Test]
        public void AnAllowanceWidensEachRingMarginExactly()
        {
            var profile = Profile();
            float legacy = RoundaboutClearance.Residual(4f, 12f, profile);
            Assert.That(RoundaboutClearance.Residual(4f, 12f, profile, 0f, 0f), Is.EqualTo(legacy), "Historique au bit pres.");
            var widened = profile;
            widened.LateralClearanceMarginMeters += 0.3f;
            Assert.That(RoundaboutClearance.Residual(4f, 12f, profile, 0.3f, 0f),
                Is.EqualTo(RoundaboutClearance.Residual(4f, 12f, widened)).Within(1e-4f), "Marge de chaque cote m + a_e.");
        }

        [Test]
        public void ThePairFingerprintChangesWithEveryInflationParameterAndThePoseModelVersion()
        {
            var profile = Profile();
            var samplesA = Straight(0f, 0f, 10f, 0f, 20);
            var samplesB = Straight(5f, -5f, 5f, 5f, 20);
            var volume = new RoadBoundsBox { Center = new Vector3(5f, 0f, 0f), Extents = new Vector3(3f, 1f, 3f) };
            var idA = new RoadId(1UL, 1UL);
            var idB = new RoadId(1UL, 2UL);
            string v1 = PairGeometryFingerprint.Compute("a", samplesA, "b", samplesB, volume);
            Assert.That(PairGeometryFingerprint.Compute("a", idA, samplesA, "b", idB, samplesB, volume, GateAEvidenceParameters.Legacy, profile, null),
                Is.EqualTo(v1), "Parametres historiques : schema v1 inchange.");

            var widened = profile;
            widened.LateralClearanceMarginMeters += 0.01f;
            var tangent = GateAEvidenceParameters.Create(NominalPoseModel.TangentAligned, Allowance, 0f, 0f, 0, default(NominalPoseFeasibilityInputs));
            var fingerprints = new List<string>
            {
                v1,
                Fingerprint(samplesA, samplesB, volume, Kinematic(), profile, Entry(idA, 0d, 0.01d)),
                Fingerprint(samplesA, samplesB, volume, Kinematic(0.35f), profile, Entry(idA, 0d, 0.01d)),
                Fingerprint(samplesA, samplesB, volume, Kinematic(step: 0.009f), profile, Entry(idA, 0d, 0.01d)),
                Fingerprint(samplesA, samplesB, volume, Kinematic(eta: 0.003f), profile, Entry(idA, 0d, 0.01d)),
                Fingerprint(samplesA, samplesB, volume, Kinematic(), widened, Entry(idA, 0d, 0.01d)),
                Fingerprint(samplesA, samplesB, volume, Kinematic(), profile, Entry(idA, 0d, 0.02d)),
                Fingerprint(samplesA, samplesB, volume, tangent, profile, null)
            };
            Assert.That(new HashSet<string>(fingerprints).Count, Is.EqualTo(fingerprints.Count),
                "Marge, delta_c, a_e, h_e, eta, version de pose et intervalle d'entree changent chacun l'empreinte.");
            Assert.That(PairGeometryFingerprint.Compute("b", idB, samplesB, "a", idA, samplesA, volume, Kinematic(), profile, Entry(idA, 0d, 0.01d)),
                Is.EqualTo(fingerprints[1]), "Empreinte canonique quel que soit l'ordre de la paire.");
            Assert.That(PairGeometryFingerprint.SchemaVersionFor(Kinematic()), Is.EqualTo(PairGeometryFingerprint.EvidenceFingerprintSchemaVersion));
            Assert.That(PairGeometryFingerprint.SchemaVersionFor(GateAEvidenceParameters.Legacy), Is.EqualTo(PairGeometryFingerprint.FingerprintSchemaVersion));
        }

        private static string Fingerprint(List<RoadCurveSample> samplesA, List<RoadCurveSample> samplesB, RoadBoundsBox volume,
            GateAEvidenceParameters parameters, RoadModelValidationProfile profile, KinematicOffsetBounds bounds)
        {
            return PairGeometryFingerprint.Compute("a", new RoadId(1UL, 1UL), samplesA, "b", new RoadId(1UL, 2UL), samplesB, volume,
                parameters, profile, bounds);
        }

        private static KinematicOffsetBounds Entry(RoadId id, double lo, double hi)
        {
            var bounds = new KinematicOffsetBounds();
            bounds.Elements.Add(id, new ElementOffsets { Id = id, HasEntry = true, EntryLo = lo, EntryHi = hi });
            return bounds;
        }

        [Test]
        public void TheToleranceResponseLatchesOnlyOutsideAMeasurementRun()
        {
            var declared = new TrackingTolerance(Allowance);
            var response = new TrackingToleranceResponse();
            Assert.That(response.Observe(5UL, true, declared, 0.5f), Is.False, "Run de mesure : la campagne juge, aucun verrou.");
            Assert.That(response.Observe(6UL, false, TrackingTolerance.Undeclared, 0.5f), Is.False, "Sans epsilon_t, aucune borne.");
            Assert.That(response.Observe(7UL, false, declared, Allowance), Is.False, "La borne elle-meme n'est pas un depassement.");
            Assert.That(response.Latched, Is.False);
            Assert.That(response.Observe(8UL, false, declared, Allowance + 0.001f), Is.True);
            Assert.That(response.Latched, Is.True);
            Assert.That(response.LatchedAtStep, Is.EqualTo(8UL));
            Assert.That(response.Observe(9UL, false, declared, 1f), Is.False, "Verrou unique, jamais relache.");
            Assert.That(response.LatchedAtStep, Is.EqualTo(8UL));
        }

        [Test]
        public void TheV2DriverLatchesTheFallbackAndNeverConstrainsTheBody()
        {
            string source = File.ReadAllText("Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs");
            StringAssert.Contains("toleranceResponse.Observe(", source);
            StringAssert.Contains("command = null;", source);
            StringAssert.Contains("refusal = V2FallbackReason.TrackingToleranceExceeded;", source);
            foreach (var forbidden in new[] { "constraints", "MovePosition", "MoveRotation", "Teleport", "linearVelocity =", "angularVelocity =" })
            {
                StringAssert.DoesNotContain(forbidden, source, "Reponse 2a : aucun snap, teleportation ni contrainte du corps.");
            }

            Assert.That(Regex.Matches(source, @"isKinematic\s*=").Count, Is.EqualTo(1), "Seule l'autorite hote pose isKinematic.");
        }

        [Test]
        public void TheSignedTangentEvidenceNeverYieldsCoverage()
        {
            var admission = TrafficV2Lifecycle.Admit(File.ReadAllText(TrafficV2Settings.ModelPath), File.ReadAllText(TrafficV2Settings.SignoffPath),
                File.ReadAllText(TrafficV2Settings.ReportPath));
            Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
            Assert.That(admission.Evidence.PoseModel, Is.EqualTo(NominalPoseModel.TangentAligned));
            Assert.That(admission.Evidence.TrackingAllowanceMeters, Is.EqualTo(0f));
            Assert.That(MotionPlan.EvaluateVehicleCoverage(admission.Evidence, 0f, TrafficV2Settings.DeclaredTrackingTolerance),
                Is.EqualTo(VehicleCoverage.PoseModelMismatch), "Une preuve a pose tangente ne couvre jamais un vehicule.");
        }

        [Test]
        public void AStaleResidualBlockClosesTheBinding()
        {
            string modelText = File.ReadAllText(TrafficV2Settings.ModelPath);
            string signoff = File.ReadAllText(TrafficV2Settings.SignoffPath);
            string report = File.ReadAllText(TrafficV2Settings.ReportPath);
            var model = RoadModelCompiler.Compile(RoadModelDocument.Load(modelText));
            Assert.That(GateAEvidenceBinding.Bind(model, modelText, signoff, report).Status, Is.EqualTo(GateAEvidenceStatus.Valid));

            int block = report.IndexOf("### Residus", StringComparison.Ordinal);
            int row = report.IndexOf("| degagement | ", block, StringComparison.Ordinal);
            Assert.That(block >= 0 && row > block, Is.True, "Bloc des residus introuvable.");
            string stale = report.Substring(0, row) + "| degagement |  " + report.Substring(row + "| degagement | ".Length);
            Assert.That(GateAEvidenceBinding.Bind(model, modelText, signoff, stale).Status, Is.EqualTo(GateAEvidenceStatus.GateAEvidenceStale),
                "Un residu different de celui signe ferme la preuve.");
        }

        [Test]
        public void TheSteerRateBoundCoversEveryOffsetAndCurvatureOfItsCells()
        {
            var drivability = Drivability();
            var inputs = Feasibility();
            double a = drivability.ReferencePointAheadRearAxleMeters;
            double ratio = drivability.WheelbaseMeters / a;
            var random = new System.Random(552);
            for (int trial = 0; trial < 120; trial++)
            {
                double lo = -0.4d + 0.8d * random.NextDouble();
                double hi = lo + 0.3d * random.NextDouble();
                double kappaMin = -0.25d + 0.5d * random.NextDouble();
                double kappaMax = kappaMin + 0.1d * random.NextDouble();
                double bound = KinematicOffsetBounds.MaximumSteerRateDegreesPerSecond(drivability, inputs, ratio, a, lo, hi, kappaMin, kappaMax, 0.008f);
                double dense = 0d;
                for (int i = 0; i <= 300; i++)
                {
                    double e = lo + (hi - lo) * i / 300d;
                    for (int j = 0; j <= 10; j++)
                    {
                        double kappa = kappaMin + (kappaMax - kappaMin) * j / 10d;
                        double cap = inputs.DesiredSpeedMetersPerSecond;
                        if (Math.Abs(kappa) > 0d) cap = Math.Min(cap, Math.Sqrt(inputs.LateralGripMetersPerSecondSquared / Math.Abs(kappa)));
                        double speed = Math.Min(cap, PathHorizon.NominalSteeringCeilingMetersPerSecond(drivability, (float)e));
                        dense = Math.Max(dense, speed * Math.Abs(NominalPoseFeasibility.WheelAngleRatePerMeter(ratio, a, e, kappa)) * 180d / Math.PI);
                    }
                }

                Assert.That(bound, Is.GreaterThanOrEqualTo(dense - 1e-6d), "Cellule [" + lo + ", " + hi + "] x [" + kappaMin + ", " + kappaMax + "].");
            }
        }
    }
}
