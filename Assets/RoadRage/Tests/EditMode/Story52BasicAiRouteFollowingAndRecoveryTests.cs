using System;
using System.IO;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.2 : verification ciblee des predicats purs de poursuite/blocage du controleur IA
    /// (ComputeSeekIntent, HasArrivedAtWaypoint, IsStuck), du bouclage de route (RouteWaypoints),
    /// de la reutilisation (sans modification) des predicats retournement/hors-zone de la Story 3.4,
    /// et de l'absence d'etat partage entre vehicules IA -- tout testable sans Netcode.
    /// </summary>
    public sealed class Story52BasicAiRouteFollowingAndRecoveryTests
    {
        private const string DriverControllerSourcePath = "Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs";

        [Test]
        public void ComputeSeekIntentAdvancesTowardWaypointOutsideArrivalRadius()
        {
            var intent = NetworkedAIVehicleDriverController.ComputeSeekIntent(
                Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 10f), arrivalRadius: 3f, steerFullLockDegrees: 45f);

            Assert.That(intent.IsIdle, Is.False);
            Assert.That(intent.Throttle, Is.EqualTo(1f), "Poursuite : plein gaz tant que hors du rayon d'arrivee.");
            Assert.That(intent.Steer, Is.EqualTo(0f).Within(0.0001f), "Waypoint droit devant : pas de correction de direction.");
        }

        [Test]
        public void ComputeSeekIntentSteersTowardOffCenterWaypointConsistentlyWithYawConvention()
        {
            // Waypoint a droite de l'avant (Vector3.forward) : Steer positif = droite (meme convention que VehicleDriveIntent
            // et que la rotation appliquee par ApplyMovement, Quaternion.AngleAxis(yaw, Vector3.up) avec yaw = Steer * ...).
            var intent = NetworkedAIVehicleDriverController.ComputeSeekIntent(
                Vector3.zero, Vector3.forward, new Vector3(10f, 0f, 0f), arrivalRadius: 3f, steerFullLockDegrees: 45f);

            Assert.That(intent.Steer, Is.GreaterThan(0f), "Waypoint a droite doit produire un Steer positif (droite).");
            Assert.That(intent.Steer, Is.EqualTo(1f).Within(0.0001f), "90 degres hors axe avec un plein-lock de 45 doit saturer a 1.");
        }

        [Test]
        public void ComputeSeekIntentIsIdleWithinArrivalRadius()
        {
            var intent = NetworkedAIVehicleDriverController.ComputeSeekIntent(
                Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 2f), arrivalRadius: 3f, steerFullLockDegrees: 45f);

            Assert.That(intent.IsIdle, Is.True, "Waypoint dans le rayon d'arrivee : plus de poursuite, l'appelant avance WaypointIndex.");
        }

        [Test]
        public void HasArrivedAtWaypointUsesPlanarDistanceAgainstArrivalRadius()
        {
            Assert.That(NetworkedAIVehicleDriverController.HasArrivedAtWaypoint(Vector3.zero, new Vector3(0f, 0f, 3f), 3f), Is.True);
            Assert.That(NetworkedAIVehicleDriverController.HasArrivedAtWaypoint(Vector3.zero, new Vector3(0f, 0f, 3.01f), 3f), Is.False);
            Assert.That(NetworkedAIVehicleDriverController.HasArrivedAtWaypoint(Vector3.zero, new Vector3(0f, 50f, 0f), 3f), Is.True,
                "La distance ignore l'axe vertical (planaire uniquement).");
        }

        [Test]
        public void IsStuckPredicateFlagsOnlyAtOrBelowSpeedThreshold()
        {
            Assert.That(NetworkedAIVehicleDriverController.IsStuck(0f, 0.5f), Is.True);
            Assert.That(NetworkedAIVehicleDriverController.IsStuck(0.5f, 0.5f), Is.True);
            Assert.That(NetworkedAIVehicleDriverController.IsStuck(0.51f, 0.5f), Is.False);
        }

        [Test]
        public void RouteWaypointsNormalizeIndexLoopsAfterLastPointAndHandlesNegativeIndices()
        {
            Assert.That(RouteWaypoints.NormalizeIndex(0, 3), Is.EqualTo(0));
            Assert.That(RouteWaypoints.NormalizeIndex(2, 3), Is.EqualTo(2));
            Assert.That(RouteWaypoints.NormalizeIndex(3, 3), Is.EqualTo(0), "Boucle apres le dernier waypoint.");
            Assert.That(RouteWaypoints.NormalizeIndex(-1, 3), Is.EqualTo(2), "Index negatif boucle depuis la fin.");
            Assert.That(RouteWaypoints.NormalizeIndex(5, 0), Is.EqualTo(0), "Route vide : jamais d'exception, repli sur 0.");
        }

        [Test]
        public void RouteWaypointsNearestIndexPicksClosestPlanarWaypointForEachVehicleStart()
        {
            var loop = new[]
            {
                new Vector3(45f, 0f, -45f),
                new Vector3(45f, 0f, 0f),
                new Vector3(45f, 0f, 45f),
                new Vector3(0f, 0f, 45f),
                new Vector3(-45f, 0f, 45f),
                new Vector3(-45f, 0f, 0f),
                new Vector3(-45f, 0f, -45f),
                new Vector3(0f, 0f, -45f),
            };

            Assert.That(RouteWaypoints.NearestIndex(new Vector3(45f, 0.05f, -20f), loop), Is.EqualTo(1));
            Assert.That(RouteWaypoints.NearestIndex(new Vector3(20f, 0.05f, 45f), loop), Is.EqualTo(3));
            Assert.That(RouteWaypoints.NearestIndex(new Vector3(-45f, 0.05f, 20f), loop), Is.EqualTo(5));
            Assert.That(RouteWaypoints.NearestIndex(Vector3.zero, System.Array.Empty<Vector3>()), Is.EqualTo(0), "Route vide : repli sur 0.");
        }

        [Test]
        public void DriverControllerReusesExistingRolloverAndVoidHeightPredicatesUnmodified()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);

            Assert.That(source, Does.Contain("NetworkedVehicleDriverController.IsBelowVoidHeightThreshold("),
                "Hors-zone : reutilise le predicat existant plutot qu'une nouvelle geometrie de bornes (spec Design Notes).");
            Assert.That(source, Does.Contain("NetworkedVehicleDriverController.IsRolledOver("),
                "Capote : reutilise le predicat existant de la Story 3.4.");
            Assert.That(source, Does.Contain("+ verticalVelocity"),
                "La conduite IA ne pilote que le plan horizontal : ecraser la composante verticale annulerait "
                + "la gravite, le vehicule leviterait et le seuil de vide ne pourrait plus jamais etre franchi.");
        }

        [Test]
        public void DriverControllerHasNoSharedStaticStateBeyondItsPureFunctions()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);

            // Seules les 3 fonctions de decision (Story 5.2) sont statiques : aucune collection/etat
            // partage entre vehicules IA, chaque instance ne lit/ecrit que ses propres champs.
            Assert.That(Occurrences(source, "static "), Is.EqualTo(3),
                "Seuls ComputeSeekIntent/HasArrivedAtWaypoint/IsStuck doivent etre statiques -- toute autre occurrence signale un etat partage entre vehicules IA.");
            Assert.That(source, Does.Not.Contain("FindObjectsOfType"),
                "La recuperation ne doit jamais inspecter/muter d'autres vehicules IA.");
        }

        [Test]
        public void DriverControllerAppliesRecoveryWithoutRpcOrAdvancingWaypointIndex()
        {
            var source = File.ReadAllText(DriverControllerSourcePath);

            Assert.That(source, Does.Not.Contain("[Rpc"), "Mouvement/recuperation IA restent purement host-automatiques, sans RPC.");
            Assert.That(source, Does.Contain("networkTransform.Teleport(waypointPosition, rotation, transform.localScale);"),
                "Snap NetworkTransform lors de la recuperation, comme la recuperation joueur (Story 3.4).");
        }

        private static int Occurrences(string source, string token)
        {
            var count = 0;
            var index = 0;
            while ((index = source.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }

            return count;
        }
    }
}
