using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.26 : geometrie dirigee en abscisse curviligne, validation geometrique a echec dur et
    /// localisation de voie. Chaque ligne de la matrice d'E/S de la spec sur un modele synthetique
    /// ecrit a la main. Deterministe, sans scene, sans asset, sans Netcode.
    /// </summary>
    [Category("Core")]
    public sealed class Story526GeometryAndLocalizationTests
    {
        private const string TrafficRootPath = "Assets/RoadRage/Features/Vehicles/Traffic";

        private static RoadId Id(int index)
        {
            return new RoadId(0xC3C3C3C300000000UL | (uint)index, 0x3C3C3C3C00000000UL | (uint)index);
        }

        private static readonly RoadId ModelId = Id(1);
        private static readonly RoadId SectionS1 = Id(2);
        private static readonly RoadId SectionS2 = Id(3);
        private static readonly RoadId SectionS3 = Id(4);
        private static readonly RoadId CorridorSB = Id(5);
        private static readonly RoadId CorridorNB = Id(6);
        private static readonly RoadId CorridorNB2 = Id(7);
        private static readonly RoadId CorridorN2 = Id(8);
        private static readonly RoadId CorridorEB = Id(9);
        private static readonly RoadId JunctionJ = Id(10);
        private static readonly RoadId MovementMS = Id(11);
        private static readonly RoadId MovementMR = Id(12);
        private static readonly RoadId ControlC = Id(13);
        private static readonly RoadId AdjacencyAd = Id(14);
        private static readonly RoadId PortalEntry = Id(15);
        private static readonly RoadId PortalExit = Id(16);

        // Enregistrements ajoutes par certains cas d'echec.
        private static readonly RoadId ConnectionK = Id(50);
        private static readonly RoadId CorridorStray = Id(51);
        private static readonly RoadId AdjacencyOpposite = Id(52);

        // Fixture d'ancrage AD-48 : datum le long de +z, membre a 30 degres.
        private static readonly RoadId SectionGrounding = Id(70);
        private static readonly RoadId DatumGrounding = Id(71);
        private static readonly RoadId MemberGrounding = Id(72);

        private const int SBIndex = 0;
        private const int NBIndex = 1;
        private const int NB2Index = 2;
        private const int MSIndex = 0;

        private static readonly Vector3 North = new Vector3(0f, 0f, 1f);
        private static readonly Vector3 South = new Vector3(0f, 0f, -1f);

        // ================================================================== modele synthetique

        /// <summary>
        /// Route a double sens de 12 m puis carrefour. S1 : SB (x=-2, z 30->0, ordre 0), NB (x=2,
        /// z 0->30, datum, ordre 1), NB2 (x=6, z 0->30, ordre 2), demi-largeurs 2 m, NB -> NB2
        /// adjacence a droite. A z=30, deux mouvements divergents depuis NB2 : MS tout droit vers N2
        /// (S2, x=6, z 40->70), MR a droite en quart de cercle r=10 vers EB (S3, z=40, x 16->46).
        /// Le bord droit de NB2 (x > 8) n'a aucun voisin.
        /// </summary>
        private static RoadModelSource BuildModel()
        {
            var source = new RoadModelSource();
            source.ModelId = ModelId;
            source.Label = "modele synthetique 5.26";
            source.ValidationProfile = ValidationProfile();
            source.LocalizationProfile = LocalizationProfile();

            source.Sections = new[] { Section(SectionS1), Section(SectionS2), Section(SectionS3) };

            source.Corridors = new[]
            {
                Corridor(CorridorSB, SectionS1, Straight(new Vector3(-2f, 0f, 30f), new Vector3(-2f, 0f, 0f)), 0, false),
                Corridor(CorridorNB, SectionS1, Straight(new Vector3(2f, 0f, 0f), new Vector3(2f, 0f, 30f)), 1, true),
                Corridor(CorridorNB2, SectionS1, Straight(new Vector3(6f, 0f, 0f), new Vector3(6f, 0f, 30f)), 2, false),
                Corridor(CorridorN2, SectionS2, Straight(new Vector3(6f, 0f, 40f), new Vector3(6f, 0f, 70f)), 0, true),
                Corridor(CorridorEB, SectionS3, Straight(new Vector3(16f, 0f, 40f), new Vector3(46f, 0f, 40f)), 0, true)
            };

            source.Adjacencies = new[] { Adjacency(AdjacencyAd, CorridorNB, CorridorNB2, LaneSide.Right) };

            var junction = new Junction();
            junction.Id = JunctionJ;
            junction.Label = "J";
            junction.Feature = JunctionFeature.TJunction;
            junction.Boundary = Box(new Vector3(11f, 0f, 35f), new Vector3(8f, 3f, 6f));
            source.Junctions = new[] { junction };

            source.Movements = new[]
            {
                Movement(MovementMS, CorridorNB2, CorridorN2, Straight(new Vector3(6f, 0f, 30f), new Vector3(6f, 0f, 40f))),
                Movement(MovementMR, CorridorNB2, CorridorEB, RightQuarterTurn(new Vector3(6f, 0f, 30f), 10f))
            };

            var control = new JunctionControl();
            control.Id = ControlC;
            control.JunctionId = JunctionJ;
            control.Kind = JunctionControlKind.Uncontrolled;
            control.ControlledMovementIds = new[] { MovementMS, MovementMR };
            source.Controls = new[] { control };

            source.Portals = new[]
            {
                MakePortal(PortalEntry, CorridorNB2, PortalRole.Entry, 0f),
                MakePortal(PortalExit, CorridorN2, PortalRole.Exit, 30f)
            };

            return source;
        }

        private static RoadModelValidationProfile ValidationProfile()
        {
            var profile = new RoadModelValidationProfile();
            profile.MaxVehicleHalfWidthMeters = 1.03f;
            profile.MaxVehicleLengthMeters = 4.5f;
            profile.LateralClearanceMarginMeters = 0.25f;
            profile.SeamGapToleranceMeters = 0.05f;
            profile.SeamTangentToleranceDegrees = 5f;
            profile.LengthToleranceMeters = 0.05f;
            profile.EnvelopeOverlapToleranceMeters = 0.05f;
            profile.GroundingMaxOffAxisDegrees = 45f;
            return profile;
        }

        private static RoadLocalizationProfile LocalizationProfile()
        {
            var profile = new RoadLocalizationProfile();
            profile.ScoreBandMeters = 0.15f;
            profile.HysteresisMeters = 0.1f;
            profile.AcceptanceDistanceMeters = 2.5f;
            profile.WrongWayHeadingDegrees = 90f;
            return profile;
        }

        private static RoadSection Section(RoadId id)
        {
            var section = new RoadSection();
            section.Id = id;
            section.RoadClass = RoadClass.Local;
            section.DefaultSpeedLimitMetersPerSecond = 13.9f;
            section.DefaultAllowedVehicleClasses = VehicleClassMask.Car;
            return section;
        }

        private static LaneCorridor Corridor(RoadId id, RoadId sectionId, RoadCurveSample[] samples, int lateralOrder, bool isDatum)
        {
            var corridor = new LaneCorridor();
            corridor.Id = id;
            corridor.SectionId = sectionId;
            corridor.Samples = samples;
            corridor.LengthMeters = samples[samples.Length - 1].SMeters;
            corridor.LateralOrder = lateralOrder;
            corridor.IsCrossSectionDatum = isDatum;
            return corridor;
        }

        private static JunctionMovement Movement(RoadId id, RoadId from, RoadId to, RoadCurveSample[] samples)
        {
            var movement = new JunctionMovement();
            movement.Id = id;
            movement.JunctionId = JunctionJ;
            movement.FromCorridorId = from;
            movement.ToCorridorId = to;
            movement.Samples = samples;
            movement.LengthMeters = samples[samples.Length - 1].SMeters;
            movement.RoutePreferenceWeight = 1f;
            return movement;
        }

        private static LaneAdjacency Adjacency(RoadId id, RoadId from, RoadId to, LaneSide side)
        {
            var adjacency = new LaneAdjacency();
            adjacency.Id = id;
            adjacency.FromCorridorId = from;
            adjacency.ToCorridorId = to;
            adjacency.Side = side;
            adjacency.FromStartSMeters = 0f;
            adjacency.FromEndSMeters = 30f;
            adjacency.ToStartSMeters = 0f;
            adjacency.ToEndSMeters = 30f;
            adjacency.Permission = LaneChangePermission.Allowed;
            return adjacency;
        }

        private static Portal MakePortal(RoadId id, RoadId corridorId, PortalRole role, float s)
        {
            var portal = new Portal();
            portal.Id = id;
            portal.CorridorId = corridorId;
            portal.Role = role;
            portal.SMeters = s;
            portal.EnvelopeLengthMeters = 6f;
            portal.EnvelopeHalfWidthMeters = 2f;
            return portal;
        }

        private static RoadBoundsBox Box(Vector3 center, Vector3 extents)
        {
            var box = new RoadBoundsBox();
            box.Center = center;
            box.Extents = extents;
            return box;
        }

        private static RoadCurveSample[] Straight(Vector3 start, Vector3 end)
        {
            float length = (end - start).magnitude;
            var tangent = (end - start).normalized;
            return new[]
            {
                Sample(0f, start, tangent, 0f),
                Sample(0.5f * length, Vector3.Lerp(start, end, 0.5f), tangent, 0f),
                Sample(length, end, tangent, 0f)
            };
        }

        /// <summary>Quart de cercle analytique vers la droite depuis une pose orientee +z, un echantillon tous les 5 degres.</summary>
        private static RoadCurveSample[] RightQuarterTurn(Vector3 start, float radius)
        {
            const int steps = 18;
            var center = start + new Vector3(radius, 0f, 0f);
            var samples = new RoadCurveSample[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                float phi = 0.5f * Mathf.PI * i / steps;
                var position = center + new Vector3(-radius * Mathf.Cos(phi), 0f, radius * Mathf.Sin(phi));
                var tangent = new Vector3(Mathf.Sin(phi), 0f, Mathf.Cos(phi));
                samples[i] = Sample(radius * phi, position, tangent, 1f / radius);
            }

            return samples;
        }

        private static RoadCurveSample Sample(float s, Vector3 position, Vector3 tangent, float curvature)
        {
            var sample = new RoadCurveSample();
            sample.SMeters = s;
            sample.Position = position;
            sample.Tangent = tangent;
            sample.Up = Vector3.up;
            sample.CurvaturePerMeter = curvature;
            sample.HalfWidthLeftMeters = 2f;
            sample.HalfWidthRightMeters = 2f;
            return sample;
        }

        private static RoadCurveSample BankedSample(float s, Vector3 position, Vector3 up)
        {
            var sample = Sample(s, position, North, 0f);
            sample.Up = up;
            return sample;
        }

        // ================================================================== helpers de localisation

        private static VehicleFootprint Footprint(float halfWidth)
        {
            var footprint = new VehicleFootprint();
            footprint.ReferenceOriginLocal = Vector3.zero;
            footprint.FrontMeters = 2.25f;
            footprint.RearMeters = 2.25f;
            footprint.LeftMeters = halfWidth;
            footprint.RightMeters = halfWidth;
            return footprint;
        }

        private static VehicleFootprintPose Pose(Vector3 position, Vector3 forward, float halfWidth)
        {
            var pose = new VehicleFootprintPose();
            pose.Position = position;
            pose.Forward = forward;
            pose.Up = Vector3.up;
            pose.Footprint = Footprint(halfWidth);
            return pose;
        }

        private static RoadLocation Localize(CompiledRoadModel model, Vector3 position, Vector3 forward, RoadId previous, RoadId[] route)
        {
            var location = RoadLocalizer.Localize(model, Pose(position, forward, 1.03f), previous, route);
            AssertWellFormed(location);
            return location;
        }

        private static RoadLocation Localize(CompiledRoadModel model, Vector3 position, Vector3 forward)
        {
            return Localize(model, position, forward, RoadId.None, null);
        }

        /// <summary>Invariants valables pour tout resultat, quelle que soit la pose.</summary>
        private static void AssertWellFormed(RoadLocation location)
        {
            Assert.That(location.Confidence, Is.InRange(0f, 1f), "Confiance deterministe dans [0, 1].");
            if (!location.Localized)
            {
                Assert.That(location.ElementId, Is.EqualTo(RoadId.None), "localized=false n'a aucune identite d'element.");
                Assert.That(location.ElementKind, Is.EqualTo(RoadElementKind.None));
                Assert.That(location.Confidence, Is.EqualTo(0f));
            }
            else
            {
                Assert.That(location.ElementKind, Is.Not.EqualTo(RoadElementKind.None),
                    "Un resultat localise nomme son type d'element.");
                Assert.That(location.ElementId.IsEmpty, Is.False, "Un resultat localise nomme son element.");
            }

            for (int i = 1; i < location.Alternatives.Count; i++)
            {
                var a = location.Alternatives[i - 1];
                var b = location.Alternatives[i];
                bool ordered = a.Rank < b.Rank
                    || (a.Rank == b.Rank && (a.Score < b.Score || (a.Score == b.Score && a.ElementId.CompareTo(b.ElementId) < 0)));
                Assert.That(ordered, Is.True, "Alternatives ordonnees par (rang, score, RoadId).");
            }

            for (int i = 0; i < location.Alternatives.Count; i++)
            {
                Assert.That(location.Alternatives[i].ElementId, Is.Not.EqualTo(location.ElementId),
                    "L'element retenu n'est pas sa propre alternative.");
            }
        }

        private static bool HasAlternative(RoadLocation location, RoadId id)
        {
            for (int i = 0; i < location.Alternatives.Count; i++)
            {
                if (location.Alternatives[i].ElementId == id)
                {
                    return true;
                }
            }

            return false;
        }

        private static RoadCurve CorridorCurve(CompiledRoadModel model, RoadId id)
        {
            EffectiveLaneCorridor corridor;
            Assert.That(model.TryGetCorridor(id, out corridor), Is.True);
            return corridor.Curve;
        }

        private static RoadCurve MovementCurve(CompiledRoadModel model, RoadId id)
        {
            CompiledJunctionMovement movement;
            Assert.That(model.TryGetMovement(id, out movement), Is.True);
            return movement.Curve;
        }

        // ================================================================== courbe : AC1, signes, 3D

        [Test]
        public void TheSyntheticModelIsGeometricallyValidAndCompiles()
        {
            Assert.That(RoadModelValidator.Validate(BuildModel()), Is.Empty);
            Assert.That(RoadModelCompiler.Compile(BuildModel()).Version.IsEmpty, Is.False);
        }

        [Test]
        public void ACompiledCorridorOrMovementSamplesItsFullFrameAtS()
        {
            var model = RoadModelCompiler.Compile(BuildModel());

            var nb = CorridorCurve(model, CorridorNB).Sample(12f);
            Assert.That(nb.SMeters, Is.EqualTo(12f).Within(1e-4f));
            Assert.That((nb.Position - new Vector3(2f, 0f, 12f)).magnitude, Is.LessThan(1e-4f));
            Assert.That((nb.Tangent - North).magnitude, Is.LessThan(1e-5f));
            Assert.That((nb.Up - Vector3.up).magnitude, Is.LessThan(1e-5f));
            Assert.That((nb.Right - Vector3.right).magnitude, Is.LessThan(1e-5f), "right = normalize(cross(up, forward)).");
            Assert.That(nb.CurvaturePerMeter, Is.EqualTo(0f), "Nulle en ligne droite.");
            Assert.That(nb.HalfWidthLeftMeters, Is.EqualTo(2f).Within(1e-5f));
            Assert.That(nb.HalfWidthRightMeters, Is.EqualTo(2f).Within(1e-5f));

            var mr = MovementCurve(model, MovementMR);
            var mid = mr.Sample(0.5f * mr.Length);
            Assert.That(mid.CurvaturePerMeter, Is.EqualTo(0.1f).Within(1e-4f), "Virage a droite de rayon 10 m.");
            Assert.That(mid.Tangent.magnitude, Is.EqualTo(1f).Within(1e-5f));
            Assert.That(Vector3.Dot(mid.Tangent, mid.Up), Is.EqualTo(0f).Within(1e-5f));
            Assert.That((mid.Right - Vector3.Cross(mid.Up, mid.Tangent).normalized).magnitude, Is.LessThan(1e-5f));
        }

        [Test]
        public void ProjectionNeverLeavesTheDomainBeforeTheStartOrAfterTheEnd()
        {
            var model = RoadModelCompiler.Compile(BuildModel());
            var nb2 = CorridorCurve(model, CorridorNB2);

            var before = nb2.Project(new Vector3(6f, 0f, -3f));
            Assert.That(before.SMeters, Is.EqualTo(0f), "Borne a 0, jamais extrapole.");
            Assert.That(before.LongitudinalOverrunMeters, Is.EqualTo(3f).Within(1e-4f));

            var after = nb2.Project(new Vector3(7f, 0f, 45f));
            Assert.That(after.SMeters, Is.EqualTo(nb2.Length), "Borne a Length, jamais extrapole.");
            Assert.That(after.LongitudinalOverrunMeters, Is.EqualTo(15f).Within(1e-4f));
            Assert.That(after.LateralOffsetMeters, Is.EqualTo(1f).Within(1e-4f));

            var mr = MovementCurve(model, MovementMR);
            for (int i = 0; i < 20; i++)
            {
                var point = new Vector3(-10f + 3f * i, (i % 3) - 1f, 20f + 2f * i);
                float s = mr.Project(point).SMeters;
                Assert.That(s, Is.InRange(0f, mr.Length), "Toute projection reste dans [0, Length].");
            }
        }

        [Test]
        public void SignConventionsFollowTheRoadFrame()
        {
            var model = RoadModelCompiler.Compile(BuildModel());
            var nb = CorridorCurve(model, CorridorNB);

            Assert.That(nb.Project(new Vector3(3f, 0f, 10f)).LateralOffsetMeters, Is.EqualTo(1f).Within(1e-4f), "Lateral positif a droite.");
            Assert.That(nb.Project(new Vector3(1f, 0f, 10f)).LateralOffsetMeters, Is.EqualTo(-1f).Within(1e-4f));
            Assert.That(nb.Project(new Vector3(2f, 1f, 10f)).NormalOffsetMeters, Is.EqualTo(1f).Within(1e-4f), "Normal positif selon road-up.");

            var frame = nb.Sample(10f);
            float toTheRight = frame.SignedHeadingDegrees(Quaternion.AngleAxis(10f, Vector3.up) * North);
            float toTheLeft = frame.SignedHeadingDegrees(Quaternion.AngleAxis(-10f, Vector3.up) * North);
            Assert.That(toTheRight, Is.EqualTo(10f).Within(1e-3f), "Cap positif vers la droite.");
            Assert.That(toTheLeft, Is.EqualTo(-10f).Within(1e-3f));
            Assert.That(Mathf.Abs(frame.SignedHeadingDegrees(South)), Is.EqualTo(180f).Within(1e-3f), "Cap dans [-180, 180].");

            // Courbure calculee par le constructeur : positive a droite, negative a gauche, nulle droite.
            var right = RoadCurveBuilder.Build(ArcPoints(12f, 10, false), Ups(10), Widths(10), Widths(10), 0.05f);
            var left = RoadCurveBuilder.Build(ArcPoints(12f, 10, true), Ups(10), Widths(10), Widths(10), 0.05f);
            var straight = RoadCurveBuilder.Build(
                new[] { Vector3.zero, new Vector3(0f, 0f, 10f) }, Ups(2), Widths(2), Widths(2), 0.05f);
            // Extremites comprises : c'est la que la condition d'extremite de la spline peut inverser le signe.
            for (int i = 0; i < right.Length; i++)
            {
                Assert.That(right[i].CurvaturePerMeter, Is.GreaterThan(0f), "Virage a droite : courbure positive.");
                Assert.That(left[i].CurvaturePerMeter, Is.LessThan(0f), "Virage a gauche : courbure negative.");
            }

            for (int i = 0; i < straight.Length; i++)
            {
                Assert.That(straight[i].CurvaturePerMeter, Is.EqualTo(0f).Within(1e-6f), "Ligne droite : courbure nulle.");
            }
        }

        [Test]
        public void ProjectionIsThreeDimensionalAlongRoadUpNotWorldUp()
        {
            // Route en devers de 30 degres : road-up n'est pas le haut monde.
            var up = Quaternion.AngleAxis(30f, North) * Vector3.up;
            var samples = new[]
            {
                BankedSample(0f, Vector3.zero, up),
                BankedSample(10f, new Vector3(0f, 0f, 10f), up)
            };
            var curve = new RoadCurve(samples);

            var frame = curve.Sample(5f);
            var alongRoadUp = curve.Project(frame.Position + up * 1.5f);
            Assert.That(alongRoadUp.NormalOffsetMeters, Is.EqualTo(1.5f).Within(1e-4f));
            Assert.That(alongRoadUp.LateralOffsetMeters, Is.EqualTo(0f).Within(1e-4f));

            var alongWorldUp = curve.Project(frame.Position + Vector3.up);
            Assert.That(alongWorldUp.NormalOffsetMeters, Is.EqualTo(Mathf.Cos(30f * Mathf.Deg2Rad)).Within(1e-4f));
            Assert.That(Mathf.Abs(alongWorldUp.LateralOffsetMeters), Is.EqualTo(0.5f).Within(1e-4f),
                "Un haut monde n'est pas un haut route : la projection est 3D.");
            Assert.That(alongWorldUp.DistanceMeters, Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void EqualDistancesAreBrokenByTheSmallestS()
        {
            // U : monte en x=0, traverse en z=10, redescend en x=4. Le point (2, 0, 5) est a 2 m des
            // deux branches.
            var curve = new RoadCurve(new[]
            {
                Sample(0f, new Vector3(0f, 0f, 0f), North, 0f),
                Sample(10f, new Vector3(0f, 0f, 10f), North, 0f),
                Sample(14f, new Vector3(4f, 0f, 10f), Vector3.right, 0f),
                Sample(24f, new Vector3(4f, 0f, 0f), South, 0f)
            });

            var projection = curve.Project(new Vector3(2f, 0f, 5f));
            Assert.That(projection.DistanceMeters, Is.EqualTo(2f).Within(1e-4f));
            Assert.That(projection.SMeters, Is.EqualTo(5f).Within(1e-4f), "Egalite de distance : la plus petite abscisse.");
        }

        [Test]
        public void BoundsContainTheWholeWidthEnvelope()
        {
            var model = RoadModelCompiler.Compile(BuildModel());
            var curves = new[] { CorridorCurve(model, CorridorNB2), MovementCurve(model, MovementMR) };

            foreach (var curve in curves)
            {
                float s0 = 0.2f * curve.Length;
                float s1 = 0.7f * curve.Length;
                var segment = curve.Bounds(s1, s0);
                for (int k = 0; k <= 200; k++)
                {
                    var p = curve.Sample(Mathf.Lerp(s0, s1, k / 200f));
                    Assert.That(segment.Contains(p.Position - p.Right * p.HalfWidthLeftMeters), Is.True, "Bord gauche dans les bornes.");
                    Assert.That(segment.Contains(p.Position + p.Right * p.HalfWidthRightMeters), Is.True, "Bord droit dans les bornes.");
                    Assert.That(curve.FullBounds.Contains(p.Position + p.Right * p.HalfWidthRightMeters), Is.True);
                }
            }
        }

        // ================================================================== constructeur : AC2

        private static Vector3[] ArcPoints(float radius, int count, bool turnLeft)
        {
            // Depart a l'origine vers +z, quart de cercle, un point tous les 90/(count-1) degres.
            float side = turnLeft ? -1f : 1f;
            var center = new Vector3(side * radius, 0f, 0f);
            var points = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                float angle = 0.5f * Mathf.PI * i / (count - 1);
                points[i] = center + new Vector3(-side * radius * Mathf.Cos(angle), 0f, radius * Mathf.Sin(angle));
            }

            return points;
        }

        private static Vector3[] Ups(int count)
        {
            var ups = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                ups[i] = Vector3.up;
            }

            return ups;
        }

        private static float[] Widths(int count)
        {
            var widths = new float[count];
            for (int i = 0; i < count; i++)
            {
                widths[i] = 2f;
            }

            return widths;
        }

        [Test]
        public void APolylineOnATwelveMeterArcCompilesWithinTheChordToleranceAndTheTurnSign()
        {
            const float radius = 12f;
            var points = ArcPoints(radius, 10, false);
            var samples = RoadCurveBuilder.Build(points, Ups(10), Widths(10), Widths(10), 0.05f);

            Assert.That(samples.Length, Is.GreaterThan(points.Length), "La subdivision adaptative a effectivement subdivise.");

            var source = new RoadModelSource();
            source.ModelId = ModelId;
            source.ValidationProfile = ValidationProfile();
            source.LocalizationProfile = LocalizationProfile();
            source.Sections = new[] { Section(SectionS1) };
            source.Corridors = new[] { Corridor(CorridorNB, SectionS1, samples, 0, true) };

            var model = RoadModelCompiler.Compile(source);
            EffectiveLaneCorridor corridor;
            Assert.That(model.TryGetCorridor(CorridorNB, out corridor), Is.True);

            var center = new Vector3(radius, 0f, 0f);
            float worst = 0f;
            for (int i = 1; i < corridor.Samples.Count; i++)
            {
                for (int k = 0; k <= 20; k++)
                {
                    var p = Vector3.Lerp(corridor.Samples[i - 1].Position, corridor.Samples[i].Position, k / 20f);
                    worst = Mathf.Max(worst, Mathf.Abs((p - center).magnitude - radius));
                }

                Assert.That(corridor.Samples[i].CurvaturePerMeter, Is.GreaterThan(0f), "Courbure du signe du virage (droite).");
            }

            Assert.That(worst, Is.LessThanOrEqualTo(0.05f), "Chaque corde s'ecarte de l'arc d'au plus 0,05 m.");
            Assert.That(corridor.Samples[corridor.Samples.Count / 2].CurvaturePerMeter, Is.EqualTo(1f / radius).Within(0.01f));
        }

        [Test]
        public void TheBuilderRejectsDegenerateInput()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                RoadCurveBuilder.Build(new[] { Vector3.zero }, Ups(1), Widths(1), Widths(1), 0.05f);
            });
            Assert.Throws<ArgumentException>(delegate
            {
                RoadCurveBuilder.Build(new[] { Vector3.zero, Vector3.zero }, Ups(2), Widths(2), Widths(2), 0.05f);
            });
            Assert.Throws<ArgumentException>(delegate
            {
                RoadCurveBuilder.Build(new[] { Vector3.zero, North }, Ups(2), Widths(2), Widths(2), 0f);
            });

            // Demi-largeur non positive.
            Assert.Throws<ArgumentException>(delegate
            {
                RoadCurveBuilder.Build(new[] { Vector3.zero, North }, Ups(2), new[] { 2f, 0f }, Widths(2), 0.05f);
            });

            // Road-up parallele a la tangente : repere indefini.
            Assert.Throws<ArgumentException>(delegate
            {
                RoadCurveBuilder.Build(new[] { Vector3.zero, new Vector3(0f, 10f, 0f) }, Ups(2), Widths(2), Widths(2), 0.05f);
            });

            // Tolerance inatteignable : jamais de corde hors tolerance emise en silence.
            Assert.Throws<ArgumentException>(delegate
            {
                RoadCurveBuilder.Build(ArcPoints(12f, 10, false), Ups(10), Widths(10), Widths(10), 1e-12f);
            });
        }

        [Test]
        public void TheBuilderInterpolatesWidthsPerSideAndABankedUp()
        {
            var bank = Quaternion.AngleAxis(20f, North) * Vector3.up;
            var samples = RoadCurveBuilder.Build(
                new[] { Vector3.zero, new Vector3(0f, 0f, 10f) },
                new[] { Vector3.up, bank },
                new[] { 2f, 3f },
                new[] { 4f, 6f },
                0.05f);

            Assert.That(samples[0].HalfWidthLeftMeters, Is.EqualTo(2f).Within(1e-5f));
            Assert.That(samples[0].HalfWidthRightMeters, Is.EqualTo(4f).Within(1e-5f));
            Assert.That(samples[samples.Length - 1].HalfWidthLeftMeters, Is.EqualTo(3f).Within(1e-5f), "Gauche et droite jamais echangees.");
            Assert.That(samples[samples.Length - 1].HalfWidthRightMeters, Is.EqualTo(6f).Within(1e-5f));
            Assert.That((samples[samples.Length - 1].Up - bank).magnitude, Is.LessThan(1e-4f), "Le devers authore est porte.");

            var mid = new RoadCurve(samples).Sample(5f);
            Assert.That(mid.HalfWidthLeftMeters, Is.EqualTo(2.5f).Within(1e-4f));
            Assert.That(mid.HalfWidthRightMeters, Is.EqualTo(5f).Within(1e-4f));
            Assert.That((mid.Up - Quaternion.AngleAxis(10f, North) * Vector3.up).magnitude, Is.LessThan(1e-4f), "Devers interpole a mi-chemin.");
        }

        // ================================================================== geometrie invalide : un cas par code

        private static void AssertApprovedCeilingFailure(Action<RoadModelSource> mutate, string fieldName)
        {
            var source = BuildModel();
            mutate(source);

            bool reported = false;
            foreach (var issue in RoadModelValidator.Validate(source))
            {
                reported |= issue.Code == RoadModelValidationCode.ProfileToleranceAboveApprovedCeiling
                    && issue.SubjectId == ModelId
                    && issue.Message.Contains(fieldName);
            }

            Assert.That(reported, Is.True, "Validate doit nommer " + fieldName + " sous ProfileToleranceAboveApprovedCeiling.");

            var exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(source); },
                "Aucune sortie compilee, aucune version pour un profil qui relache le contrat.");
            Assert.That(exception.HasCode(RoadModelValidationCode.ProfileToleranceAboveApprovedCeiling), Is.True, exception.Message);
            Assert.That(exception.Message.Contains(fieldName), Is.True, "Le motif stable nomme le champ fautif.");
        }

        [Test]
        public void AProfileThatRelaxesAnApprovedToleranceCeilingIsAHardFailure()
        {
            // Le contrat (AD-45) plafonne la couture et la longueur/corde a 0,05 m et la tangente de
            // couture a 5 deg : un profil plus strict passe, un profil plus large est un echec dur.
            var stricter = BuildModel();
            stricter.ValidationProfile.SeamGapToleranceMeters = 0.04f;
            stricter.ValidationProfile.SeamTangentToleranceDegrees = 4f;
            stricter.ValidationProfile.LengthToleranceMeters = 0.04f;
            Assert.That(RoadModelValidator.Validate(stricter), Is.Empty, "Plus strict que le contrat : accepte.");
            Assert.That(RoadModelCompiler.Compile(stricter).Version.IsEmpty, Is.False);

            AssertApprovedCeilingFailure(
                delegate(RoadModelSource source) { source.ValidationProfile.SeamGapToleranceMeters = 0.06f; },
                "SeamGapToleranceMeters");
            AssertApprovedCeilingFailure(
                delegate(RoadModelSource source) { source.ValidationProfile.SeamTangentToleranceDegrees = 6f; },
                "SeamTangentToleranceDegrees");
            AssertApprovedCeilingFailure(
                delegate(RoadModelSource source) { source.ValidationProfile.LengthToleranceMeters = 0.06f; },
                "LengthToleranceMeters");
        }

        /// <summary>Datum le long de +z et membre a 30 degres : propre a tout autre titre que l'ancrage.</summary>
        private static RoadModelSource GroundingAngleSource()
        {
            var source = new RoadModelSource();
            source.ModelId = ModelId;
            source.ValidationProfile = ValidationProfile();
            source.LocalizationProfile = LocalizationProfile();
            source.Sections = new[] { Section(SectionGrounding) };

            var direction = new Vector3(Mathf.Sin(30f * Mathf.Deg2Rad), 0f, Mathf.Cos(30f * Mathf.Deg2Rad));
            var memberStart = new Vector3(4.2f, 0f, 2f);
            source.Corridors = new[]
            {
                Corridor(DatumGrounding, SectionGrounding, Straight(Vector3.zero, new Vector3(0f, 0f, 40f)), 0, true),
                Corridor(MemberGrounding, SectionGrounding, Straight(memberStart, memberStart + direction * 12f), 1, false)
            };

            return source;
        }

        [Test]
        public void TheGroundingAngleIsReadFromTheValidationProfile()
        {
            var source = GroundingAngleSource();
            Assert.That(RoadModelValidator.Validate(source), Is.Empty, "45 deg (fixture) ancre le membre a 30 deg.");
            Assert.That(RoadModelCompiler.Compile(source).Version.IsEmpty, Is.False);

            var narrow = GroundingAngleSource();
            narrow.ValidationProfile.GroundingMaxOffAxisDegrees = 20f;
            bool reported = false;
            foreach (var issue in RoadModelValidator.Validate(narrow))
            {
                reported |= issue.Code == RoadModelValidationCode.CorridorNotGroundedOnDatum && issue.SubjectId == MemberGrounding;
            }

            Assert.That(reported, Is.True, "A 20 deg de seuil, un membre a 30 deg n'est plus ancre.");
            Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(narrow); });
        }

        private static RoadModelCompilationException AssertGeometryFailure(Action<RoadModelSource> mutate, RoadModelValidationCode code, RoadId subject)
        {
            var source = BuildModel();
            mutate(source);

            bool reported = false;
            foreach (var issue in RoadModelValidator.Validate(source))
            {
                reported |= issue.Code == code && issue.SubjectId == subject;
            }

            Assert.That(reported, Is.True, "Validate doit nommer " + code + " sur " + subject + ".");

            var exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(source); },
                "Aucune sortie compilee, aucune version pour une geometrie invalide.");
            Assert.That(exception.HasCode(code), Is.True, "Code attendu " + code + ", obtenu : " + exception.Message);
            return exception;
        }

        [Test]
        public void ANonOrthonormalFrameIsAHardFailure()
        {
            AssertGeometryFailure(
                delegate(RoadModelSource source) { source.Corridors[NBIndex].Samples[1].Up = new Vector3(0f, 0.9f, 0.1f); },
                RoadModelValidationCode.NonOrthonormalFrame, CorridorNB);

            AssertGeometryFailure(
                delegate(RoadModelSource source) { source.Corridors[NBIndex].Samples[1].Up = new Vector3(0f, 0.8f, 0.6f); },
                RoadModelValidationCode.NonOrthonormalFrame, CorridorNB);
        }

        [Test]
        public void AnInconsistentLengthIsAHardFailure()
        {
            AssertGeometryFailure(
                delegate(RoadModelSource source) { source.Corridors[NBIndex].LengthMeters = 31f; },
                RoadModelValidationCode.InconsistentCurveLength, CorridorNB);

            AssertGeometryFailure(
                delegate(RoadModelSource source) { source.Corridors[NBIndex].Samples[1].SMeters = 16f; },
                RoadModelValidationCode.InconsistentCurveLength, CorridorNB);

            AssertGeometryFailure(
                delegate(RoadModelSource source)
                {
                    for (int i = 0; i < source.Corridors[NBIndex].Samples.Length; i++)
                    {
                        source.Corridors[NBIndex].Samples[i].SMeters += 1f;
                    }

                    source.Corridors[NBIndex].LengthMeters += 1f;
                },
                RoadModelValidationCode.InconsistentCurveLength, CorridorNB);
        }

        [Test]
        public void ANonPositiveHalfWidthIsAHardFailure()
        {
            AssertGeometryFailure(
                delegate(RoadModelSource source) { source.Corridors[NBIndex].Samples[1].HalfWidthLeftMeters = 0f; },
                RoadModelValidationCode.NonPositiveHalfWidth, CorridorNB);

            AssertGeometryFailure(
                delegate(RoadModelSource source) { source.Movements[MSIndex].Samples[1].HalfWidthRightMeters = -1f; },
                RoadModelValidationCode.NonPositiveHalfWidth, MovementMS);
        }

        [Test]
        public void AnArcPositionOutsideItsCorridorDomainIsAHardFailure()
        {
            AssertGeometryFailure(
                delegate(RoadModelSource source) { source.Adjacencies[0].FromEndSMeters = 31f; },
                RoadModelValidationCode.ArcPositionOutOfDomain, AdjacencyAd);

            AssertGeometryFailure(
                delegate(RoadModelSource source)
                {
                    source.Adjacencies[0].ToStartSMeters = 12f;
                    source.Adjacencies[0].ToEndSMeters = 12f;
                },
                RoadModelValidationCode.ArcPositionOutOfDomain, AdjacencyAd);

            AssertGeometryFailure(
                delegate(RoadModelSource source) { source.Portals[0].SMeters = -1f; },
                RoadModelValidationCode.ArcPositionOutOfDomain, PortalEntry);
        }

        [Test]
        public void ANonPositiveBoxExtentIsAHardFailure()
        {
            AssertGeometryFailure(
                delegate(RoadModelSource source) { source.Junctions[0].Boundary = Box(new Vector3(11f, 0f, 35f), new Vector3(8f, 0f, 6f)); },
                RoadModelValidationCode.NonPositiveBoxExtents, JunctionJ);
        }

        [Test]
        public void ABrokenConnectionSeamIsAHardFailure()
        {
            AssertGeometryFailure(
                delegate(RoadModelSource source)
                {
                    var connection = new LaneConnection();
                    connection.Id = ConnectionK;
                    connection.FromCorridorId = CorridorNB;
                    connection.ToCorridorId = CorridorN2;
                    connection.Kind = LaneConnectionKind.Continuation;
                    source.Connections = new[] { connection };
                },
                RoadModelValidationCode.ConnectionSeamBroken, ConnectionK);
        }

        [Test]
        public void ABrokenMovementSeamIsAHardFailureOnPositionTangentOrWidth()
        {
            AssertGeometryFailure(
                delegate(RoadModelSource source) { source.Movements[MSIndex].FromCorridorId = CorridorNB; },
                RoadModelValidationCode.MovementSeamBroken, MovementMS);

            AssertGeometryFailure(
                delegate(RoadModelSource source)
                {
                    source.Movements[MSIndex].Samples[0].Tangent = Quaternion.AngleAxis(10f, Vector3.up) * North;
                },
                RoadModelValidationCode.MovementSeamBroken, MovementMS);

            AssertGeometryFailure(
                delegate(RoadModelSource source)
                {
                    for (int i = 0; i < source.Movements[MSIndex].Samples.Length; i++)
                    {
                        source.Movements[MSIndex].Samples[i].HalfWidthLeftMeters = 1.5f;
                    }
                },
                RoadModelValidationCode.MovementSeamBroken, MovementMS);
        }

        [Test]
        public void AnAdjacencyBetweenOppositeDirectionsIsAHardFailure()
        {
            AssertGeometryFailure(
                delegate(RoadModelSource source)
                {
                    source.Adjacencies = new[]
                    {
                        source.Adjacencies[0],
                        Adjacency(AdjacencyOpposite, CorridorNB, CorridorSB, LaneSide.Left)
                    };
                },
                RoadModelValidationCode.OppositeDirectionAdjacency, AdjacencyOpposite);
        }

        [Test]
        public void ALaneSideThatContradictsGeometryAndOrderIsAHardFailure()
        {
            AssertGeometryFailure(
                delegate(RoadModelSource source) { source.Adjacencies[0].Side = LaneSide.Left; },
                RoadModelValidationCode.LaneSideDisagreement, AdjacencyAd);
        }

        [Test]
        public void ANonMonotoneLateralOrderIsAHardFailure()
        {
            // SB et NB2 echangent leurs ordres : l'ordre 0 n'est plus le plus a gauche.
            AssertGeometryFailure(
                delegate(RoadModelSource source)
                {
                    source.Adjacencies = new LaneAdjacency[0];
                    source.Corridors[SBIndex].LateralOrder = 2;
                    source.Corridors[NB2Index].LateralOrder = 0;
                },
                RoadModelValidationCode.NonMonotoneLateralOrder, CorridorNB);
        }

        [Test]
        public void OverlappingLateralEnvelopesAreAHardFailure()
        {
            AssertGeometryFailure(
                delegate(RoadModelSource source)
                {
                    for (int i = 0; i < source.Corridors[NB2Index].Samples.Length; i++)
                    {
                        source.Corridors[NB2Index].Samples[i].HalfWidthLeftMeters = 2.5f;
                    }
                },
                RoadModelValidationCode.OverlappingLateralEnvelopes, CorridorNB2);
        }

        [Test]
        public void ACorridorNotGroundedOnItsDatumIsAHardFailure()
        {
            AssertGeometryFailure(
                delegate(RoadModelSource source)
                {
                    var corridors = new List<LaneCorridor>(source.Corridors);
                    corridors.Add(Corridor(CorridorStray, SectionS1, Straight(new Vector3(10f, 0f, 50f), new Vector3(10f, 0f, 80f)), 3, false));
                    source.Corridors = corridors.ToArray();
                },
                RoadModelValidationCode.CorridorNotGroundedOnDatum, CorridorStray);

            AssertGeometryFailure(
                delegate(RoadModelSource source)
                {
                    // Recouvre le datum mais le traverse a angle droit.
                    var corridors = new List<LaneCorridor>(source.Corridors);
                    corridors.Add(Corridor(CorridorStray, SectionS1, Straight(new Vector3(-10f, 0f, 15f), new Vector3(20f, 0f, 15f)), 3, false));
                    source.Corridors = corridors.ToArray();
                },
                RoadModelValidationCode.CorridorNotGroundedOnDatum, CorridorStray);
        }

        [Test]
        public void NoPublicMemberEmitsAVersionOrACompiledModelOutsideCompile()
        {
            Assert.That(typeof(CompiledRoadModel).GetConstructors(BindingFlags.Public | BindingFlags.Instance), Is.Empty);
            Assert.That(typeof(RoadModelVersion).GetConstructors(BindingFlags.Public | BindingFlags.Instance), Is.Empty);

            foreach (var type in typeof(RoadModelCompiler).Assembly.GetTypes())
            {
                if (type.Namespace != typeof(RoadModelCompiler).Namespace || !type.IsPublic)
                {
                    continue;
                }

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (method.ReturnType == typeof(CompiledRoadModel) || method.ReturnType == typeof(RoadModelVersion))
                    {
                        bool isCompile = type == typeof(RoadModelCompiler) && method.Name == "Compile";
                        bool isModelVersionGetter = type == typeof(CompiledRoadModel) && method.Name == "get_Version";
                        Assert.That(isCompile || isModelVersionGetter, Is.True,
                            "Emetteur de version ou de modele hors Compile : " + type.Name + "." + method.Name);
                    }
                }
            }

            // Et Compile lui-meme n'en emet aucune pour une geometrie invalide.
            var invalid = BuildModel();
            invalid.Corridors[NBIndex].LengthMeters = 31f;
            Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(invalid); });
        }

        // ================================================================== localisation : matrice

        [Test]
        public void NominalPoseAtTheCenterOfACorridorIsLocalizedWithFullConfidence()
        {
            var model = RoadModelCompiler.Compile(BuildModel());
            var location = Localize(model, new Vector3(6f, 0f, 15f), North);

            Assert.That(location.Localized, Is.True);
            Assert.That(location.ElementKind, Is.EqualTo(RoadElementKind.LaneCorridor));
            Assert.That(location.ElementId, Is.EqualTo(CorridorNB2));
            Assert.That(location.SMeters, Is.EqualTo(15f).Within(1e-4f));
            Assert.That(location.LateralOffsetMeters, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(location.HeadingErrorDegrees, Is.EqualTo(0f).Within(1e-3f));
            Assert.That(location.Flags, Is.EqualTo(RoadLocationFlags.None));
            Assert.That(location.Confidence, Is.EqualTo(1f));
            Assert.That(location.ModelId, Is.EqualTo(ModelId));
            Assert.That(location.ModelVersion, Is.EqualTo(model.Version));
        }

        [Test]
        public void OscillatingAcrossTheCorridorToMovementSeamTransitionsAtMostOnceAndNeverBack()
        {
            var model = RoadModelCompiler.Compile(BuildModel());

            // Trois trajectoires : depart cote corridor, depart cote mouvement, et une vraie avancee
            // au-dela de l'hysteresis suivie d'une oscillation.
            var runs = new[]
            {
                new[] { 29.98f, 30.02f, 29.98f, 30.02f, 29.98f, 30.02f, 29.98f, 30.02f, 29.98f, 30.02f },
                new[] { 30.02f, 29.98f, 30.02f, 29.98f, 30.02f, 29.98f, 30.02f, 29.98f, 30.02f, 29.98f },
                new[] { 29.98f, 30.02f, 30.3f, 30.28f, 30.32f, 30.28f, 30.32f, 30.28f }
            };

            foreach (var run in runs)
            {
                var history = new List<RoadId>();
                RoadId previous = RoadId.None;
                for (int i = 0; i < run.Length; i++)
                {
                    var location = Localize(model, new Vector3(6f, 0f, run[i]), North, previous, null);
                    Assert.That(location.Localized, Is.True);
                    if (history.Count == 0 || history[history.Count - 1] != location.ElementId)
                    {
                        history.Add(location.ElementId);
                    }

                    previous = location.ElementId;
                }

                Assert.That(history.Count, Is.LessThanOrEqualTo(2),
                    "Au plus une transition, jamais de retour : " + history.Count + " elements successifs.");
            }
        }

        [Test]
        public void AHysteresisRetainedPreviousElementPastASeamKeepsItsIdentityAndRaisesOutsideEnvelope()
        {
            var model = RoadModelCompiler.Compile(BuildModel());

            // 3 cm au-dela de la couture NB2 -> MS/MR (hysteresis 0,1 m) : le precedent garde son
            // bonus de 0,1 m contre 0,05 m pour ses successeurs explicites, donc il est retenu ; sa
            // projection est bornee a sa fin et le depassement est signale, jamais masque.
            var pastEnd = Localize(model, new Vector3(6f, 0f, 30.03f), North, CorridorNB2, null);
            Assert.That(pastEnd.Localized, Is.True);
            Assert.That(pastEnd.ElementId, Is.EqualTo(CorridorNB2), "Hysteresis : le precedent reste retenu au-dela de la couture.");
            Assert.That(pastEnd.SMeters, Is.EqualTo(30f).Within(1e-4f), "s borne a Length, sans extrapolation.");
            Assert.That(pastEnd.HasFlag(RoadLocationFlags.OutsideEnvelope), Is.True,
                "Le depassement longitudinal de la reference leve OutsideEnvelope.");
            Assert.That(pastEnd.HasFlag(RoadLocationFlags.WrongWay), Is.False);
            Assert.That(HasAlternative(pastEnd, MovementMS), Is.True, "Le successeur reste une alternative.");

            // Symetrique : 3 cm AVANT le debut du mouvement, le mouvement precedent est retenu et sa
            // projection est bornee a 0.
            var beforeStart = Localize(model, new Vector3(6f, 0f, 29.97f), North, MovementMS, null);
            Assert.That(beforeStart.Localized, Is.True);
            Assert.That(beforeStart.ElementId, Is.EqualTo(MovementMS), "Hysteresis : le mouvement precedent reste retenu avant sa couture.");
            Assert.That(beforeStart.SMeters, Is.EqualTo(0f), "s borne a 0, sans extrapolation.");
            Assert.That(beforeStart.HasFlag(RoadLocationFlags.OutsideEnvelope), Is.True);
            Assert.That(beforeStart.HasFlag(RoadLocationFlags.WrongWay), Is.False);
            Assert.That(HasAlternative(beforeStart, CorridorNB2), Is.True);
        }

        [Test]
        public void ADisplacedPoseKeepsItsSignedLateralAndIsOutsideOnlyWhenTheFootprintExceedsTheEnvelope()
        {
            var model = RoadModelCompiler.Compile(BuildModel());

            // +1,5 m vers le bord droit de NB2, qui n'a aucun voisin.
            var narrow = RoadLocalizer.Localize(model, Pose(new Vector3(7.5f, 0f, 15f), North, 0.4f), RoadId.None, null);
            Assert.That(narrow.ElementId, Is.EqualTo(CorridorNB2));
            Assert.That(narrow.LateralOffsetMeters, Is.EqualTo(1.5f).Within(1e-4f));
            Assert.That(narrow.HasFlag(RoadLocationFlags.OutsideEnvelope), Is.False,
                "Empreinte etroite entierement dans l'enveloppe : aucun drapeau.");

            var wide = RoadLocalizer.Localize(model, Pose(new Vector3(7.5f, 0f, 15f), North, 1.03f), RoadId.None, null);
            Assert.That(wide.ElementId, Is.EqualTo(CorridorNB2));
            Assert.That(wide.HasFlag(RoadLocationFlags.OutsideEnvelope), Is.True,
                "Le point de reference est dedans mais l'empreinte deborde : c'est l'empreinte qui compte.");

            // +3 m : la reference elle-meme est hors enveloppe, la pose reste localisee.
            var far = Localize(model, new Vector3(9f, 0f, 15f), North);
            Assert.That(far.Localized, Is.True);
            Assert.That(far.ElementId, Is.EqualTo(CorridorNB2));
            Assert.That(far.LateralOffsetMeters, Is.EqualTo(3f).Within(1e-4f));
            Assert.That(far.HasFlag(RoadLocationFlags.OutsideEnvelope), Is.True);
            Assert.That(far.HasFlag(RoadLocationFlags.WrongWay), Is.False);
        }

        [Test]
        public void AWrongWayPoseOnItsOwnLaneStaysOnThatLane()
        {
            var model = RoadModelCompiler.Compile(BuildModel());
            var location = Localize(model, new Vector3(6f, 0f, 15f), South);

            Assert.That(location.ElementId, Is.EqualTo(CorridorNB2));
            Assert.That(location.HasFlag(RoadLocationFlags.WrongWay), Is.True);
            Assert.That(Mathf.Abs(location.HeadingErrorDegrees), Is.EqualTo(180f).Within(1e-3f));
            Assert.That(location.HasFlag(RoadLocationFlags.OutsideEnvelope), Is.False, "Drapeaux composables, pas un statut unique.");
        }

        [Test]
        public void AWrongWayPoseNextToTheOppositeLaneStaysOnItsPhysicalLane()
        {
            var model = RoadModelCompiler.Compile(BuildModel());

            // Au centre de NB, cap -z : SB, juste a cote, a exactement le cap de la pose.
            var centered = Localize(model, new Vector3(2f, 0f, 15f), South);
            Assert.That(centered.ElementId, Is.EqualTo(CorridorNB), "La voie physique, pas la voie de meme cap.");
            Assert.That(centered.HasFlag(RoadLocationFlags.WrongWay), Is.True);
            Assert.That(HasAlternative(centered, CorridorSB), Is.True, "La voie opposee n'est qu'une alternative.");

            // Decalee vers SB mais encore dans NB : un score naif (lateral + cap) prefererait SB. Le
            // rang d'enveloppe prime, le cap ne classe qu'a l'interieur d'un rang.
            var shifted = Localize(model, new Vector3(0.5f, 0f, 15f), South);
            Assert.That(shifted.ElementId, Is.EqualTo(CorridorNB));
            Assert.That(shifted.HasFlag(RoadLocationFlags.WrongWay), Is.True);
            Assert.That(HasAlternative(shifted, CorridorSB), Is.True);
        }

        [Test]
        public void TheStartOfTwoDivergentMovementsIsAmbiguousUntilARouteElementDecides()
        {
            var model = RoadModelCompiler.Compile(BuildModel());
            var position = new Vector3(6f, 0f, 30.05f);

            var open = Localize(model, position, North);
            Assert.That(open.Localized, Is.True);
            Assert.That(open.ElementKind, Is.EqualTo(RoadElementKind.JunctionMovement));
            Assert.That(open.HasFlag(RoadLocationFlags.Ambiguous), Is.True);
            Assert.That(open.Confidence, Is.LessThan(1f));
            RoadId other = open.ElementId == MovementMS ? MovementMR : MovementMS;
            Assert.That(HasAlternative(open, other), Is.True, "Ambiguous decrit l'ensemble des candidats.");

            var routed = Localize(model, position, North, RoadId.None, new[] { MovementMR });
            Assert.That(routed.ElementId, Is.EqualTo(MovementMR), "Un element de route fourni l'emporte.");
        }

        [Test]
        public void APoseBeyondTheAcceptanceThresholdIsNotLocalizedAndLeavesTheModelUntouched()
        {
            var model = RoadModelCompiler.Compile(BuildModel());
            var versionBefore = model.Version;

            var remote = Localize(model, new Vector3(30f, 0f, 15f), North);
            Assert.That(remote.Localized, Is.False);
            Assert.That(remote.Alternatives, Is.Empty);

            // 3 m hors de l'enveloppe de NB2 : au-dela de l'acceptation (2,5 m), dans le voisinage.
            var near = Localize(model, new Vector3(11f, 0f, 15f), North);
            Assert.That(near.Localized, Is.False);
            Assert.That(HasAlternative(near, CorridorNB2), Is.True, "Alternatives eventuelles sans element courant.");

            Assert.That(model.Version, Is.EqualTo(versionBefore), "Modele inchange.");
            Assert.That(model.Corridors.Count, Is.EqualTo(5));
        }

        [Test]
        public void APoseBeforeTheStartOrAfterTheEndIsBoundedToTheDomain()
        {
            var model = RoadModelCompiler.Compile(BuildModel());

            var before = Localize(model, new Vector3(6f, 0f, -1f), North);
            Assert.That(before.ElementId, Is.EqualTo(CorridorNB2));
            Assert.That(before.SMeters, Is.EqualTo(0f), "s borne a 0.");

            var after = Localize(model, new Vector3(6f, 0f, 71f), North);
            Assert.That(after.ElementId, Is.EqualTo(CorridorN2));
            Assert.That(after.SMeters, Is.EqualTo(30f).Within(1e-4f), "s borne a Length.");
        }

        // ================================================================== patchs de revue

        private static readonly RoadId CorridorA = Id(60);
        private static readonly RoadId CorridorC = Id(61);
        private static readonly RoadId CorridorB = Id(62);
        private static readonly RoadId ConnectionAB = Id(63);

        /// <summary>Une section, un corridor : aucune verification AD-48 ne peut masquer un defaut de forme.</summary>
        private static RoadModelSource SingleCorridorSource(RoadCurveSample[] samples)
        {
            var source = new RoadModelSource();
            source.ModelId = ModelId;
            source.ValidationProfile = ValidationProfile();
            source.LocalizationProfile = LocalizationProfile();
            source.Sections = new[] { Section(SectionS1) };
            source.Corridors = new[] { Corridor(CorridorNB, SectionS1, samples, 0, true) };
            return source;
        }

        private static void AssertSingleCorridorFrameFailure(RoadCurveSample[] samples, RoadModelValidationCode code)
        {
            var source = SingleCorridorSource(samples);
            bool reported = false;
            foreach (var issue in RoadModelValidator.Validate(source))
            {
                reported |= issue.Code == code && issue.SubjectId == CorridorNB;
            }

            Assert.That(reported, Is.True, code + " attendu sur le corridor.");
            var exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(source); });
            Assert.That(exception.HasCode(code), Is.True, "Code attendu " + code + " : " + exception.Message);
        }

        [Test]
        public void ATangentOpposingItsChordIsAHardFailure()
        {
            var samples = Straight(Vector3.zero, new Vector3(0f, 0f, 30f));
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i].Tangent = South;
            }

            // Le repere reste unitaire et orthogonal : c'est le SENS qui ne suit plus la corde, donc
            // un code propre (33), distinct de la forme du repere (19).
            AssertSingleCorridorFrameFailure(samples, RoadModelValidationCode.TangentOpposesChord);
        }

        [Test]
        public void AFlippedRoadUpIsAHardFailure()
        {
            var samples = Straight(Vector3.zero, new Vector3(0f, 0f, 30f));
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i].Up = Vector3.down;
            }

            // Un road-up retourne echange gauche et droite en silence : code 32, pas 19.
            AssertSingleCorridorFrameFailure(samples, RoadModelValidationCode.RoadUpFlipped);
        }

        [Test]
        public void AnUnsetOrOutOfRangeProfileIsAHardFailure()
        {
            var unset = BuildModel();
            unset.LocalizationProfile = default(RoadLocalizationProfile);
            var exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(unset); });
            Assert.That(exception.HasCode(RoadModelValidationCode.NumericValueOutOfRange), Is.True, exception.Message);

            var zeroTolerance = BuildModel();
            zeroTolerance.ValidationProfile.SeamGapToleranceMeters = 0f;
            exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(zeroTolerance); });
            Assert.That(exception.HasCode(RoadModelValidationCode.NumericValueOutOfRange), Is.True, exception.Message);

            var wrongWay = BuildModel();
            wrongWay.LocalizationProfile.WrongWayHeadingDegrees = 181f;
            exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(wrongWay); });
            Assert.That(exception.HasCode(RoadModelValidationCode.NumericValueOutOfRange), Is.True, exception.Message);

            // Le contrat fixe le SENS du drapeau a 90 deg : un seuil au-dela est hors domaine, sinon un
            // contresens frontal cesserait d'etre signale.
            var looseWrongWay = BuildModel();
            looseWrongWay.LocalizationProfile.WrongWayHeadingDegrees = 91f;
            exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(looseWrongWay); });
            Assert.That(exception.HasCode(RoadModelValidationCode.NumericValueOutOfRange), Is.True, exception.Message);

            // Gabarit : un profil non renseigne (demi-largeur ou longueur nulle) ne compile pas en silence.
            var zeroGabarit = BuildModel();
            zeroGabarit.ValidationProfile.MaxVehicleHalfWidthMeters = 0f;
            exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(zeroGabarit); });
            Assert.That(exception.HasCode(RoadModelValidationCode.NumericValueOutOfRange), Is.True, exception.Message);

            var zeroLength = BuildModel();
            zeroLength.ValidationProfile.MaxVehicleLengthMeters = -1f;
            exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(zeroLength); });
            Assert.That(exception.HasCode(RoadModelValidationCode.NumericValueOutOfRange), Is.True, exception.Message);

            // AD-48 : seuil dans ]0, 90[ -- 90 degres accepterait une perpendiculaire.
            var groundingZero = BuildModel();
            groundingZero.ValidationProfile.GroundingMaxOffAxisDegrees = 0f;
            exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(groundingZero); });
            Assert.That(exception.HasCode(RoadModelValidationCode.NumericValueOutOfRange), Is.True, exception.Message);

            var groundingRightAngle = BuildModel();
            groundingRightAngle.ValidationProfile.GroundingMaxOffAxisDegrees = 90f;
            exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(groundingRightAngle); });
            Assert.That(exception.HasCode(RoadModelValidationCode.NumericValueOutOfRange), Is.True, exception.Message);
        }

        [Test]
        public void APortalBeyondItsCorridorLengthIsAHardFailure()
        {
            AssertGeometryFailure(
                delegate(RoadModelSource source) { source.Portals[1].SMeters = 31f; },
                RoadModelValidationCode.ArcPositionOutOfDomain, PortalExit);
        }

        [Test]
        public void AReferencePastTheEndOfACorridorWithoutSuccessorIsOutsideTheEnvelope()
        {
            var model = RoadModelCompiler.Compile(BuildModel());
            var location = Localize(model, new Vector3(6f, 0f, 72f), North);

            Assert.That(location.Localized, Is.True);
            Assert.That(location.ElementId, Is.EqualTo(CorridorN2));
            Assert.That(location.HasFlag(RoadLocationFlags.OutsideEnvelope), Is.True);
        }

        [Test]
        public void AStackedDeckIsNotContainedThroughItsVerticalOffset()
        {
            // Tablier bas plus large (contient la pose en plan) sous un tablier haut a 6 m.
            var source = new RoadModelSource();
            source.ModelId = ModelId;
            source.ValidationProfile = ValidationProfile();
            source.LocalizationProfile = LocalizationProfile();
            source.LocalizationProfile.AcceptanceDistanceMeters = 3.5f;
            source.Sections = new[] { Section(SectionS1), Section(SectionS2) };
            var lower = Straight(Vector3.zero, new Vector3(0f, 0f, 30f));
            for (int i = 0; i < lower.Length; i++)
            {
                lower[i].HalfWidthLeftMeters = 3f;
                lower[i].HalfWidthRightMeters = 3f;
            }

            source.Corridors = new[]
            {
                Corridor(CorridorNB, SectionS1, lower, 0, true),
                Corridor(CorridorN2, SectionS2, Straight(new Vector3(0f, 6f, 0f), new Vector3(0f, 6f, 30f)), 0, true)
            };
            var model = RoadModelCompiler.Compile(source);

            var location = Localize(model, new Vector3(2.1f, 6f, 15f), North);
            Assert.That(location.ElementId, Is.EqualTo(CorridorN2), "Le tablier de la pose, pas celui qui la contient en plan.");

            bool lowerSeen = false;
            for (int i = 0; i < location.Alternatives.Count; i++)
            {
                if (location.Alternatives[i].ElementId == CorridorNB)
                {
                    lowerSeen = true;
                    Assert.That(location.Alternatives[i].Rank, Is.EqualTo(1), "6 m sous la pose : pas une contenance.");
                }
            }

            Assert.That(lowerSeen, Is.True, "Le tablier bas est collecte comme alternative.");
        }

        [Test]
        public void AnInvalidPoseIsRejected()
        {
            var model = RoadModelCompiler.Compile(BuildModel());
            var valid = Pose(new Vector3(6f, 0f, 15f), North, 1f);

            var poses = new List<VehicleFootprintPose>();
            var nanPosition = valid;
            nanPosition.Position = new Vector3(float.NaN, 0f, 15f);
            poses.Add(nanPosition);
            var zeroForward = valid;
            zeroForward.Forward = Vector3.zero;
            poses.Add(zeroForward);
            var zeroUp = valid;
            zeroUp.Up = Vector3.zero;
            poses.Add(zeroUp);
            var parallel = valid;
            parallel.Forward = Vector3.up;
            poses.Add(parallel);
            var nanOrigin = valid;
            nanOrigin.Footprint.ReferenceOriginLocal = new Vector3(0f, float.PositiveInfinity, 0f);
            poses.Add(nanOrigin);
            var negativeExtent = valid;
            negativeExtent.Footprint.LeftMeters = -0.1f;
            poses.Add(negativeExtent);

            foreach (var pose in poses)
            {
                var candidate = pose;
                Assert.Throws<ArgumentException>(delegate { RoadLocalizer.Localize(model, candidate, RoadId.None, null); });
            }
        }

        [Test]
        public void AnExplicitConnectionToThePreviousElementWinsAnOtherwiseEqualTie()
        {
            // B (connecte a A) et C (non connecte) ont la meme geometrie ; C a le plus petit RoadId.
            var source = new RoadModelSource();
            source.ModelId = ModelId;
            source.ValidationProfile = ValidationProfile();
            source.LocalizationProfile = LocalizationProfile();
            source.Sections = new[] { Section(SectionS1), Section(SectionS2), Section(SectionS3) };
            source.Corridors = new[]
            {
                Corridor(CorridorA, SectionS1, Straight(Vector3.zero, new Vector3(0f, 0f, 10f)), 0, true),
                Corridor(CorridorB, SectionS2, Straight(new Vector3(0f, 0f, 10f), new Vector3(0f, 0f, 20f)), 0, true),
                Corridor(CorridorC, SectionS3, Straight(new Vector3(0f, 0f, 10f), new Vector3(0f, 0f, 20f)), 0, true)
            };

            var connection = new LaneConnection();
            connection.Id = ConnectionAB;
            connection.FromCorridorId = CorridorA;
            connection.ToCorridorId = CorridorB;
            connection.Kind = LaneConnectionKind.Continuation;
            source.Connections = new[] { connection };

            var model = RoadModelCompiler.Compile(source);
            var position = new Vector3(0f, 0f, 10.5f);

            Assert.That(Localize(model, position, North).ElementId, Is.EqualTo(CorridorC),
                "Sans precedent, l'egalite exacte se departage par RoadId.");
            Assert.That(Localize(model, position, North, CorridorA, null).ElementId, Is.EqualTo(CorridorB),
                "Le successeur explicite du precedent l'emporte.");
        }

        [Test]
        public void ARouteElementDecidesBetweenTwoSuccessorsOfThePreviousElement()
        {
            var model = RoadModelCompiler.Compile(BuildModel());
            var location = Localize(model, new Vector3(6f, 0f, 30.05f), North, CorridorNB2, new[] { MovementMR });
            Assert.That(location.ElementId, Is.EqualTo(MovementMR));
        }

        [Test]
        public void TheFootprintReferenceOriginIsWhatGetsLocalized()
        {
            var model = RoadModelCompiler.Compile(BuildModel());
            var pose = Pose(new Vector3(6f, 0f, 15f), North, 1f);
            pose.Footprint.ReferenceOriginLocal = new Vector3(0.5f, 0f, 1f);

            var location = RoadLocalizer.Localize(model, pose, RoadId.None, null);
            Assert.That(location.ElementId, Is.EqualTo(CorridorNB2));
            Assert.That(location.SMeters, Is.EqualTo(16f).Within(1e-4f), "Origine avancee de 1 m.");
            Assert.That(location.LateralOffsetMeters, Is.EqualTo(0.5f).Within(1e-4f), "Origine decalee de 0,5 m a droite.");
        }

        // ================================================================== determinisme et gardes

        [Test]
        public void GeometryCompilationAndLocalizationAreDeterministic()
        {
            var first = RoadCurveBuilder.Build(ArcPoints(12f, 10, false), Ups(10), Widths(10), Widths(10), 0.05f);
            var second = RoadCurveBuilder.Build(ArcPoints(12f, 10, false), Ups(10), Widths(10), Widths(10), 0.05f);
            Assert.That(second.Length, Is.EqualTo(first.Length));
            for (int i = 0; i < first.Length; i++)
            {
                Assert.That(second[i].Position, Is.EqualTo(first[i].Position));
                Assert.That(second[i].SMeters, Is.EqualTo(first[i].SMeters));
                Assert.That(second[i].CurvaturePerMeter, Is.EqualTo(first[i].CurvaturePerMeter));
            }

            var modelA = RoadModelCompiler.Compile(BuildModel());
            var shuffled = BuildModel();
            Array.Reverse(shuffled.Corridors);
            Array.Reverse(shuffled.Movements);
            var modelB = RoadModelCompiler.Compile(shuffled);
            Assert.That(modelB.Version, Is.EqualTo(modelA.Version));

            var poses = new[] { new Vector3(6f, 0f, 30.05f), new Vector3(2f, 0f, 15f), new Vector3(9f, 0.4f, 12f) };
            foreach (var pose in poses)
            {
                var a = Localize(modelA, pose, North);
                var b = Localize(modelB, pose, North);
                Assert.That(b.ElementId, Is.EqualTo(a.ElementId));
                Assert.That(b.SMeters, Is.EqualTo(a.SMeters));
                Assert.That(b.Flags, Is.EqualTo(a.Flags));
                Assert.That(b.Confidence, Is.EqualTo(a.Confidence));
                Assert.That(b.Alternatives.Count, Is.EqualTo(a.Alternatives.Count));
                for (int i = 0; i < a.Alternatives.Count; i++)
                {
                    Assert.That(b.Alternatives[i].ElementId, Is.EqualTo(a.Alternatives[i].ElementId));
                }
            }
        }

        [Test]
        public void TheStory526SourcesStayPureAndPackageIndependent()
        {
            string[] files = { "RoadCurve.cs", "RoadCurveBuilder.cs", "RoadGeometryValidator.cs", "RoadLocalization.cs" };
            string[] forbidden = { "Rigidbody", "Transform", "MonoBehaviour", "Splines", "InternalsVisibleTo" };

            foreach (var file in files)
            {
                string text = File.ReadAllText(Path.Combine(TrafficRootPath, file));
                for (int i = 0; i < forbidden.Length; i++)
                {
                    Assert.That(text, Does.Not.Contain(forbidden[i]), file + " contient " + forbidden[i]);
                }
            }

            foreach (var path in Directory.GetFiles("Assets/RoadRage/Features", "*.cs", SearchOption.AllDirectories))
            {
                Assert.That(File.ReadAllText(path), Does.Not.Contain("InternalsVisibleTo"),
                    "Aucun InternalsVisibleTo ne contourne la validation : " + path);
            }

            // Revendication de packaging : aucun assembly nouveau ni reference aux splines, et aucune
            // dependance directe au paquet -- elle n'etait verifiee que par quatre fichiers source.
            Assert.That(Directory.GetFiles(TrafficRootPath, "*.asmdef", SearchOption.AllDirectories), Is.Empty,
                "Le code Traffic vit dans l'assembly existant : aucun .asmdef nouveau.");

            foreach (var path in Directory.GetFiles("Assets/RoadRage", "*.asmdef", SearchOption.AllDirectories))
            {
                Assert.That(File.ReadAllText(path), Does.Not.Contain("Spline"),
                    "Aucun assembly ne reference les splines : " + path);
            }

            string manifest = File.ReadAllText("Packages/manifest.json");
            Assert.That(manifest, Does.Not.Contain("\"com.unity.splines\""),
                "Aucune dependance directe a com.unity.splines.");
        }

        // ================================================================== revue 2026-09-23
        // Preuves demandees par la revue : signe des bords d'enveloppe d'un membre antiparallele,
        // seuils geometriques lus dans le profil, moitie « ordre » du code 27, balayage de tout
        // l'intervalle d'adjacence, surcharge de Project, marge de Bounds, drapeaux composes.

        /// <summary>Datum a x=0 et membre antiparallele a x=lateral, demi-largeurs dissymetriques.</summary>
        private static RoadModelSource AntiparallelMemberSource(float memberLateral, float halfWidthLeft, float halfWidthRight)
        {
            var source = new RoadModelSource();
            source.ModelId = ModelId;
            source.ValidationProfile = ValidationProfile();
            source.LocalizationProfile = LocalizationProfile();
            source.Sections = new[] { Section(SectionS1) };

            var member = Corridor(CorridorSB, SectionS1, Straight(new Vector3(memberLateral, 0f, 30f), new Vector3(memberLateral, 0f, 0f)), 1, false);
            for (int i = 0; i < member.Samples.Length; i++)
            {
                member.Samples[i].HalfWidthLeftMeters = halfWidthLeft;
                member.Samples[i].HalfWidthRightMeters = halfWidthRight;
            }

            source.Corridors = new[]
            {
                Corridor(CorridorNB, SectionS1, Straight(Vector3.zero, new Vector3(0f, 0f, 30f)), 0, true),
                member
            };

            return source;
        }

        private static bool HasGeometryIssue(RoadModelSource source, RoadModelValidationCode code, RoadId subject)
        {
            foreach (var issue in RoadModelValidator.Validate(source))
            {
                if (issue.Code == code && issue.SubjectId == subject)
                {
                    return true;
                }
            }

            return false;
        }

        [Test]
        public void AnAntiparallelMemberKeepsItsOwnRightOnTheDatumSide()
        {
            // Le membre va vers -z : son cote droit fait face au datum. A lateral 4,2 avec gauche 2,4 et
            // droite 2,2, le bord correct tombe a 4,2 - 2,2 = 2,0 (recouvrement nul) ; appliquer la
            // largeur GAUCHE de ce cote donnerait 1,8, soit 0,2 m de recouvrement (code 29).
            var correct = AntiparallelMemberSource(4.2f, 2.4f, 2.2f);
            Assert.That(RoadModelValidator.Validate(correct), Is.Empty,
                "Un membre antiparallele garde sa droite du cote du datum.");
            Assert.That(RoadModelCompiler.Compile(correct).Version.IsEmpty, Is.False);

            var mirrored = AntiparallelMemberSource(4.2f, 2.2f, 2.4f);
            Assert.That(HasGeometryIssue(mirrored, RoadModelValidationCode.OverlappingLateralEnvelopes, CorridorSB), Is.True,
                "La geometrie miroir recouvre de 0,2 m : c'est bien le cote applique qui decide.");
        }

        [Test]
        public void TheSeamLengthAndOverlapTolerancesAreReadFromTheValidationProfile()
        {
            var overlap = AntiparallelMemberSource(4.16f, 2.4f, 2.2f);
            Assert.That(RoadModelValidator.Validate(overlap), Is.Empty, "0,04 m de recouvrement sous 0,05 m.");

            var strictOverlap = AntiparallelMemberSource(4.16f, 2.4f, 2.2f);
            strictOverlap.ValidationProfile.EnvelopeOverlapToleranceMeters = 0.01f;
            Assert.That(HasGeometryIssue(strictOverlap, RoadModelValidationCode.OverlappingLateralEnvelopes, CorridorSB), Is.True,
                "Le meme modele echoue sous une tolerance plus stricte : elle est bien lue dans le profil.");

            var seamGap = BuildModel();
            seamGap.Movements[MSIndex].Samples[0].Position += new Vector3(0f, 0f, 0.03f);
            Assert.That(RoadModelValidator.Validate(seamGap), Is.Empty, "3 cm d'ecart a une couture sous 0,05 m.");

            var strictSeamGap = BuildModel();
            strictSeamGap.Movements[MSIndex].Samples[0].Position += new Vector3(0f, 0f, 0.03f);
            strictSeamGap.ValidationProfile.SeamGapToleranceMeters = 0.02f;
            Assert.That(HasGeometryIssue(strictSeamGap, RoadModelValidationCode.MovementSeamBroken, MovementMS), Is.True,
                "La couture echoue quand le profil resserre l'ecart tolere.");

            var seamTangent = BuildModel();
            seamTangent.Movements[MSIndex].Samples[0].Tangent = Quaternion.AngleAxis(3f, Vector3.up) * North;
            Assert.That(RoadModelValidator.Validate(seamTangent), Is.Empty, "3 deg de tangente sous 5 deg.");

            var strictSeamTangent = BuildModel();
            strictSeamTangent.Movements[MSIndex].Samples[0].Tangent = Quaternion.AngleAxis(3f, Vector3.up) * North;
            strictSeamTangent.ValidationProfile.SeamTangentToleranceDegrees = 2f;
            Assert.That(HasGeometryIssue(strictSeamTangent, RoadModelValidationCode.MovementSeamBroken, MovementMS), Is.True,
                "La tangente echoue quand le profil resserre l'ecart angulaire.");

            var length = BuildModel();
            length.Corridors[NBIndex].Samples[1].SMeters = 15.03f;
            Assert.That(RoadModelValidator.Validate(length), Is.Empty, "3 cm d'ecart abscisse/corde sous 0,05 m.");

            var strictLength = BuildModel();
            strictLength.Corridors[NBIndex].Samples[1].SMeters = 15.03f;
            strictLength.ValidationProfile.LengthToleranceMeters = 0.02f;
            Assert.That(HasGeometryIssue(strictLength, RoadModelValidationCode.InconsistentCurveLength, CorridorNB), Is.True,
                "L'abscisse echoue quand le profil resserre la tolerance de longueur.");
        }

        /// <summary>Datum a x=0, deux membres de sens oppose au datum, arcs disjoints, ordres inverses.</summary>
        private static RoadModelSource DisjointOrderSource()
        {
            var source = new RoadModelSource();
            source.ModelId = ModelId;
            source.ValidationProfile = ValidationProfile();
            source.LocalizationProfile = LocalizationProfile();
            source.Sections = new[] { Section(SectionS1) };
            source.Corridors = new[]
            {
                Corridor(CorridorNB, SectionS1, Straight(Vector3.zero, new Vector3(0f, 0f, 30f)), 0, true),
                Corridor(CorridorSB, SectionS1, Straight(new Vector3(5f, 0f, 10f), new Vector3(5f, 0f, 0f)), 2, false),
                Corridor(CorridorNB2, SectionS1, Straight(new Vector3(10f, 0f, 30f), new Vector3(10f, 0f, 20f)), 1, false)
            };

            var adjacency = Adjacency(AdjacencyAd, CorridorSB, CorridorNB2, LaneSide.Left);
            adjacency.FromStartSMeters = 0f;
            adjacency.FromEndSMeters = 10f;
            adjacency.ToStartSMeters = 0f;
            adjacency.ToEndSMeters = 10f;
            source.Adjacencies = new[] { adjacency };
            return source;
        }

        [Test]
        public void TheOrderHalfOfTheLaneSideCheckIsLoadBearingOnItsOwn()
        {
            // Les deux membres vont vers -z : le cote droit de SB est -x, donc NB2 est a sa gauche et
            // la geometrie s'accorde avec Side=Left. Ce sont les ordres authores qui la contredisent,
            // et ComparePair ne voit pas ce couple (leurs arcs sur le datum ne se recouvrent pas).
            var inverted = DisjointOrderSource();
            Assert.That(HasGeometryIssue(inverted, RoadModelValidationCode.LaneSideDisagreement, AdjacencyAd), Is.True,
                "Ordres inverses sur arcs disjoints : seule la moitie « ordre » du code 27 peut le dire.");

            var consistent = DisjointOrderSource();
            consistent.Corridors[1].LateralOrder = 1;
            consistent.Corridors[2].LateralOrder = 2;
            Assert.That(RoadModelValidator.Validate(consistent), Is.Empty,
                "Avec des ordres conformes, le meme couple ne produit aucun echec : c'est bien l'ordre qui parlait.");
        }

        [Test]
        public void ALaneSideThatFlipsBetweenTwoAuthoredSamplesIsCaughtByTheIntervalCheck()
        {
            // La source n'a que deux echantillons : aucun point de controle authored a l'interieur.
            // La cible est a droite de ses extremites et a gauche en son milieu, les deux sections sont
            // a un seul corridor (donc sans coupe transversale a comparer), et seul un point interieur a
            // l'intervalle contredit les echantillons : AD-48 exige un accord sur tout l'intervalle.
            var source = new RoadModelSource();
            source.ModelId = ModelId;
            source.ValidationProfile = ValidationProfile();
            source.LocalizationProfile = LocalizationProfile();
            source.Sections = new[] { Section(SectionS1), Section(SectionS2) };
            source.Corridors = new[]
            {
                Corridor(CorridorNB, SectionS1, new[]
                {
                    Sample(0f, Vector3.zero, North, 0f),
                    Sample(30f, new Vector3(0f, 0f, 30f), North, 0f)
                }, 0, true),
                Corridor(CorridorSB, SectionS2, VShapedCorridor(), 0, true)
            };
            source.Adjacencies = new[] { Adjacency(AdjacencyAd, CorridorNB, CorridorSB, LaneSide.Right) };

            Assert.That(HasGeometryIssue(source, RoadModelValidationCode.LaneSideDisagreement, AdjacencyAd), Is.True,
                "Un desaccord entre deux echantillons authores est un echec (AD-48, tout l'intervalle).");
        }

        /// <summary>Corridor en V : extremites a droite de la source, sommet a gauche, s cumule sur les cordes.</summary>
        private static RoadCurveSample[] VShapedCorridor()
        {
            float span = new Vector3(10f, 0f, 15f).magnitude;
            return new[]
            {
                Sample(0f, new Vector3(5f, 0f, 0f), new Vector3(-10f, 0f, 15f).normalized, 0f),
                Sample(span, new Vector3(-5f, 0f, 15f), North, 0f),
                Sample(2f * span, new Vector3(5f, 0f, 30f), new Vector3(10f, 0f, 15f).normalized, 0f)
            };
        }

        [Test]
        public void ProjectionOverAWindowIsRestrictedAndMeasuresTheWindowOverrun()
        {
            var curve = new RoadCurve(Straight(Vector3.zero, new Vector3(0f, 0f, 30f)));

            var before = curve.Project(new Vector3(0f, 0f, 6f), 10f, 20f);
            Assert.That(before.SMeters, Is.EqualTo(10f).Within(1e-4f), "Le point le plus proche hors fenetre est rabattu sur sa borne.");
            Assert.That(before.LongitudinalOverrunMeters, Is.EqualTo(4f).Within(1e-4f),
                "Le depassement se mesure contre la fenetre, pas contre le domaine.");

            var inside = curve.Project(new Vector3(0f, 0f, 15f), 10f, 20f);
            Assert.That(inside.SMeters, Is.EqualTo(15f).Within(1e-4f));
            Assert.That(inside.LongitudinalOverrunMeters, Is.EqualTo(0f).Within(1e-4f));

            var collapsed = curve.Project(new Vector3(0f, 0f, 25f), 20f, 10f);
            Assert.That(collapsed.SMeters, Is.EqualTo(20f).Within(1e-4f), "sMax < sMin s'effondre sur la borne basse.");

            var beyondDomain = curve.Project(new Vector3(0f, 0f, 40f), -10f, 400f);
            Assert.That(beyondDomain.SMeters, Is.InRange(0f, 30f), "La fenetre est bornee au domaine de la courbe.");
            Assert.That(beyondDomain.LongitudinalOverrunMeters, Is.EqualTo(10f).Within(1e-4f));
        }

        [Test]
        public void BoundsCoverTheWidthEnvelopeAcrossADiscontinuousWidthOnASharpTurn()
        {
            var samples = RightQuarterTurn(Vector3.zero, 10f);
            for (int i = 0; i < samples.Length; i++)
            {
                bool wide = i * 2 >= samples.Length;
                samples[i].HalfWidthLeftMeters = wide ? 4f : 1f;
                samples[i].HalfWidthRightMeters = wide ? 1f : 4f;
            }

            var curve = new RoadCurve(samples);
            var bounds = curve.Bounds(0f, curve.Length);
            for (float s = 0f; s <= curve.Length; s += 0.05f)
            {
                var frame = curve.Sample(s);
                Assert.That(bounds.Contains(frame.Position - frame.Right * frame.HalfWidthLeftMeters), Is.True,
                    "Bord gauche hors des bornes a s=" + s);
                Assert.That(bounds.Contains(frame.Position + frame.Right * frame.HalfWidthRightMeters), Is.True,
                    "Bord droit hors des bornes a s=" + s);
            }

            var end = curve.Sample(curve.Length);
            Assert.That(bounds.Contains(end.Position + end.Right * end.HalfWidthRightMeters), Is.True,
                "Bord droit hors des bornes a l'extremite.");
        }

        [Test]
        public void ADisplacedWrongWayPoseCarriesBothFlagsAndKeepsItsIdentity()
        {
            var model = RoadModelCompiler.Compile(BuildModel());
            var location = Localize(model, new Vector3(9f, 0f, 15f), South);

            Assert.That(location.Localized, Is.True);
            Assert.That(location.ElementId, Is.EqualTo(CorridorNB2));
            Assert.That(location.HasFlag(RoadLocationFlags.OutsideEnvelope), Is.True);
            Assert.That(location.HasFlag(RoadLocationFlags.WrongWay), Is.True,
                "Les drapeaux sont composables : une pose deplacee a contresens les porte tous les deux.");
        }
    }
}
