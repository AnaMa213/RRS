using System.Collections.Generic;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// ANO-5.18-04. Deux invariants, tous deux mesures sur le district avant correction :
    ///
    /// 1. LA VISEE RESTE SUR LA VOIE. Elle venait d'une extrapolation en ligne droite au-dela du
    ///    noeud vise, donc elle quittait toute courbe par l'exterieur -- 1,68 m hors de l'anneau de
    ///    6,00 m du giratoire nord-est, pour une demi-voie de 2,00 m.
    /// 2. LA COURBE EST TENABLE. Le connecteur de virage serre demande 4,25 m ; a la vitesse desiree
    ///    authoree (8,0 m/s) le braquage disponible ne permet que 4,85 m. Le vehicule ne pouvait pas
    ///    suivre sa propre trajectoire et sortait 1,84 m a cote.
    ///
    /// Les valeurs du vehicule greybox sont reprises telles qu'authorees dans
    /// <c>VehicleProfileDef_Default</c> : empattement 3,10 m, braquage 40 deg reduit a 16 deg a
    /// 26 m/s. Aucune n'est inventee ici.
    /// </summary>
    public sealed class Story518CurveFollowingTests
    {
        private const float RingRadius = 6f;
        private const float Wheelbase = 3.10f;
        private const float MaxSteer = 40f;
        private const float HighSteer = 16f;
        private const float FullReduction = 26f;
        private const float DesiredSpeed = 8f;

        /// <summary>Anneau du giratoire, echantillonne comme la conduite le fait : courbe par arete.</summary>
        private static Vector3[] BuildRing(out int count, float degreesPerNode = 37f, float sampleStep = 0.9f)
        {
            var points = new List<Vector3>();
            var nodes = new List<(Vector3 position, Vector3 forward)>();
            for (var angle = 0f; angle <= 360f + degreesPerNode; angle += degreesPerNode)
            {
                var radians = angle * Mathf.Deg2Rad;
                var position = new Vector3(RingRadius * Mathf.Cos(radians), 0f, RingRadius * Mathf.Sin(radians));
                nodes.Add((position, new Vector3(-Mathf.Sin(radians), 0f, Mathf.Cos(radians))));
            }

            points.Add(nodes[0].position);
            for (var i = 1; i < nodes.Count; i++)
            {
                var length = LaneGraphRouting.ResolveJunctionTurnLength(
                    nodes[i - 1].position, nodes[i - 1].forward, nodes[i].position, nodes[i].forward);
                var samples = Mathf.Max(1, Mathf.CeilToInt(length / sampleStep));
                for (var s = 1; s <= samples; s++)
                {
                    points.Add(LaneGraphRouting.ResolveJunctionTurnPoint(
                        nodes[i - 1].position, nodes[i - 1].forward, nodes[i].position, nodes[i].forward, s / (float)samples));
                }
            }

            count = points.Count;
            return points.ToArray();
        }

        [Test]
        public void TheAimPointStaysOnTheRingInsteadOfLeavingItTangentially()
        {
            var path = BuildRing(out var count);
            var lookAhead = DesiredSpeed * 0.6f; // vitesse desiree x fenetre de visee authoree

            var worst = 0f;
            for (var i = 0; i < count - 8; i++)
            {
                var aim = LaneGraphRouting.ResolvePathLookAheadPoint(path, count, path[i], lookAhead);
                worst = Mathf.Max(worst, Mathf.Abs(new Vector2(aim.x, aim.z).magnitude - RingRadius));
            }

            // La fleche d'un troncon de 37 deg vaut 0,31 m : la visee ne peut pas faire mieux que la
            // polyligne qui la porte, et elle ne doit pas faire pire.
            Assert.That(worst, Is.LessThan(0.35f),
                "La visee quitte l'anneau de " + worst.ToString("0.00") + " m.");
        }

        [Test]
        public void TheOldTangentAimPointLeftTheRingByMoreThanHalfALane()
        {
            // Ce test documente la CAUSE. Il echouerait si quelqu'un revenait a l'extrapolation
            // tangentielle en croyant simplifier : elle vise hors de la chaussee.
            var node = new Vector3(RingRadius, 0f, 0f);
            var nodeForward = new Vector3(0f, 0f, 1f);
            var aim = LaneGraphRouting.ResolveLookAheadPoint(node, node, nodeForward,
                DesiredSpeed * 0.6f, Vector3.zero, 0f);

            var excess = new Vector2(aim.x, aim.z).magnitude - RingRadius;
            Assert.That(excess, Is.GreaterThan(1.5f),
                "L'extrapolation tangentielle visait " + excess.ToString("0.00") + " m hors de l'anneau.");
        }

        [Test]
        public void TheAimPointAdvancesAlongThePathAndNeverGoesBackwards()
        {
            var path = BuildRing(out var count);
            var previousAngle = float.NaN;
            var wrapped = 0;
            for (var i = 0; i < count - 8; i++)
            {
                var aim = LaneGraphRouting.ResolvePathLookAheadPoint(path, count, path[i], 4.8f);
                var angle = Mathf.Atan2(aim.z, aim.x) * Mathf.Rad2Deg;
                if (!float.IsNaN(previousAngle))
                {
                    var delta = Mathf.DeltaAngle(previousAngle, angle);
                    if (delta < -1f) wrapped++;
                    Assert.That(delta, Is.GreaterThan(-1f).Or.LessThan(-300f),
                        "La visee est revenue en arriere de " + delta.ToString("0.0") + " deg.");
                }
                previousAngle = angle;
            }

            Assert.That(wrapped, Is.LessThanOrEqualTo(1), "Un seul tour d'anneau est attendu.");
        }

        [Test]
        public void ThePathRadiusOfASampledCircleIsTheCircleRadius()
        {
            var path = BuildRing(out var count);
            for (var i = 2; i < count - 2; i++)
            {
                var radius = LaneGraphRouting.ResolvePathRadius(path, count, i);
                Assert.That(radius, Is.EqualTo(RingRadius).Within(0.6f),
                    "Rayon lu " + radius.ToString("0.00") + " m sur un anneau de " + RingRadius + " m.");
            }
        }

        [Test]
        public void AStraightPathImposesNoSpeedLimit()
        {
            var path = new[] { Vector3.zero, new Vector3(0f, 0f, 5f), new Vector3(0f, 0f, 10f), new Vector3(0f, 0f, 15f) };
            for (var i = 1; i < 3; i++)
            {
                var radius = LaneGraphRouting.ResolvePathRadius(path, path.Length, i);
                Assert.That(VehicleSteeringModel.ResolveCurveSpeedLimit(radius, Wheelbase, MaxSteer, HighSteer, FullReduction),
                    Is.EqualTo(float.PositiveInfinity));
            }
        }

        [Test]
        public void TheCurveSpeedLimitIsExactlyTheReciprocalOfTheAuthoredSteeringLaw()
        {
            foreach (var radius in new[] { 3.5f, 4.25f, 5f, 6f, 7.07f, 9f, 12f })
            {
                var limit = VehicleSteeringModel.ResolveCurveSpeedLimit(radius, Wheelbase, MaxSteer, HighSteer, FullReduction);
                if (!float.IsFinite(limit) || limit <= 0f) continue;

                var required = Mathf.Atan(Wheelbase / radius) * Mathf.Rad2Deg;
                var availableAtLimit = VehicleSteeringModel.ResolveSteerAngleDegrees(
                    1f, limit, 0f, MaxSteer, HighSteer, FullReduction);
                Assert.That(availableAtLimit, Is.EqualTo(required).Within(0.05f),
                    "A la vitesse plafond le braquage disponible doit valoir EXACTEMENT le braquage requis.");

                var availableFaster = VehicleSteeringModel.ResolveSteerAngleDegrees(
                    1f, limit + 1f, 0f, MaxSteer, HighSteer, FullReduction);
                Assert.That(availableFaster, Is.LessThan(required),
                    "Un metre par seconde plus vite, le rayon " + radius + " m n'est deja plus tenable.");
            }
        }

        [Test]
        public void TheTightDistrictTurnIsRefusedAtCruiseSpeedAndAllowedBelowIt()
        {
            // 4,25 m : le rayon mesure du connecteur de virage serre des quatre carrefours du district.
            var limit = VehicleSteeringModel.ResolveCurveSpeedLimit(4.25f, Wheelbase, MaxSteer, HighSteer, FullReduction);
            Assert.That(limit, Is.LessThan(DesiredSpeed),
                "Le virage serre doit imposer un plafond sous la vitesse de croisiere.");
            Assert.That(limit, Is.GreaterThan(3f), "Un virage ordinaire ne se prend pas au pas.");

            // 7,07 m : le virage large du meme carrefour. Il est tenable a la vitesse desiree.
            Assert.That(VehicleSteeringModel.ResolveCurveSpeedLimit(7.07f, Wheelbase, MaxSteer, HighSteer, FullReduction),
                Is.GreaterThan(DesiredSpeed), "Le virage large ne doit imposer aucun ralentissement.");

            // 6,00 m : l'anneau du giratoire. Il se parcourt a la vitesse desiree, comme un giratoire.
            Assert.That(VehicleSteeringModel.ResolveCurveSpeedLimit(RingRadius, Wheelbase, MaxSteer, HighSteer, FullReduction),
                Is.GreaterThan(DesiredSpeed), "L'anneau ne doit pas etre traite comme un virage serre.");
        }

        [Test]
        public void ABiggerIntersectionRaisesItsOwnSpeedLimitWithoutAnySceneConstant()
        {
            var previous = 0f;
            foreach (var radius in new[] { 4f, 6f, 8f, 10f })
            {
                var limit = VehicleSteeringModel.ResolveCurveSpeedLimit(radius, Wheelbase, MaxSteer, HighSteer, FullReduction);
                Assert.That(limit, Is.GreaterThan(previous), "Un rayon plus grand doit permettre plus vite.");
                previous = float.IsFinite(limit) ? limit : previous + 1f;
            }
        }

        [Test]
        public void AGeometricallyImpossibleRadiusIsReportedAsSuchInsteadOfBeingRounded()
        {
            // Sous l'empattement / tan(braquage max), aucune vitesse ne tient le rayon.
            var impossible = Wheelbase / Mathf.Tan(MaxSteer * Mathf.Deg2Rad) * 0.5f;
            Assert.That(VehicleSteeringModel.ResolveCurveSpeedLimit(impossible, Wheelbase, MaxSteer, HighSteer, FullReduction),
                Is.EqualTo(0f));
        }
    }
}
