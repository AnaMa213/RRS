using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    public sealed class Story517WiderPerceptionAndProgressiveUnblockingTests
    {
        /// <summary>
        /// ANO-5.18-02 (2026-09-21) : l'appartenance a la voie ne se decide plus DANS la selection du
        /// leader. Elle est resolue en amont, contre la trajectoire reelle et l'identite de voie, puis
        /// portee par <see cref="TrafficPerceptionCandidate.IsOnOwnPath"/>.
        ///
        /// La garde laterale interne qui vivait ici faisait doublon, et c'est ce doublon qui a produit
        /// le faux leader : elle elargissait la bande de l'extension de l'autre vehicule projetee sur
        /// notre normale, soit sa demi-LONGUEUR pour un vehicule perpendiculaire. Un vehicule arrete
        /// sur la branche transversale d'un carrefour, a 2,83 m, tombait donc dans une bande de
        /// 3,55 m. Une seule source de verite, et elle est en amont.
        /// </summary>
        [Test]
        public void LeaderIncludesStaticObstacleButExcludesAdjacentLaneAndBehind()
        {
            var candidates = new[]
            {
                new TrafficPerceptionCandidate(4f, 0f, 0f, false, false, true, false, 0f),
                new TrafficPerceptionCandidate(1f, 15f, 0f, true, false, true, false, 4f, halfWidth: 1f,
                    isOnOwnPath: false),
                new TrafficPerceptionCandidate(0f, 180f, 0f, true, false, true, true)
            };
            Assert.That(TrafficPerception.TrySelectLeader(candidates, 3, 20f, 100f, out var gap, out _, 1.3f), Is.True);
            Assert.That(gap, Is.EqualTo(4f), "La voie adjacente n'appartient pas a notre file.");
            Assert.That(TrafficPerception.IsInArc(21f, 0f, 20f, 100f), Is.False);
            Assert.That(TrafficPerception.IsInArc(2f, 51f, 20f, 100f), Is.False);
        }

        /// <summary>
        /// Story 5.18 (correctif du 2026-09-21) : ce qui ecarte un joueur proche n'est plus l'absence
        /// de collision predite mais son APPARTENANCE A NOTRE VOIE. Le critere precedent avait un
        /// effet de bord grave dans l'autre sens -- un joueur ARRETE en travers de la voie n'a aucune
        /// collision predite (personne ne bouge vers personne), donc il cessait d'etre un leader et
        /// l'IA lui roulait dessus. La projection sur la voie repond correctement aux deux cas.
        /// </summary>
        [Test]
        public void NearbyPlayerIsALeaderOnlyWhenItSitsInOurLane()
        {
            var besideTheLane = new TrafficPerceptionCandidate(5f, 0f, 0f, true, true, true, false,
                isPlayer: true, isCollisionThreat: false, isOnOwnPath: false);
            Assert.That(TrafficPerception.TrySelectLeader(new[] { besideTheLane }, 1, 20f, 100f, out _, out _, 1.3f), Is.False);

            var stoppedInOurLane = new TrafficPerceptionCandidate(5f, 0f, 0f, true, true, true, false,
                isPlayer: true, isCollisionThreat: false, isOnOwnPath: true);
            Assert.That(TrafficPerception.TrySelectLeader(new[] { stoppedInOurLane }, 1, 20f, 100f, out _, out _, 1.3f), Is.True,
                "Un joueur immobile dans notre voie doit rester un leader, sans quoi l'IA le traverse.");
        }

        [Test]
        public void DefaultStoppedQueueGapIsOneMeterOrLessAndHeadwayStillGrowsWithSpeed()
        {
            var profile = AssetDatabase.LoadAssetAtPath<DriverProfileDef>(
                "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset").Profile;
            Assert.That(profile.MinimumGap, Is.LessThanOrEqualTo(1f));
            Assert.That(profile.MinimumGap + 8f * profile.TimeHeadway, Is.GreaterThan(profile.MinimumGap));
        }

        [TestCase(0f, 20f, 0f, -20f, true)] // joueur frontal rapide
        [TestCase(10f, 10f, -10f, -10f, true)] // croisement
        [TestCase(0f, 12f, 0f, -8f, true)] // rattrapage
        [TestCase(5f, 0f, 0f, -10f, false)] // voie voisine
        [TestCase(0f, 12f, 0f, 8f, false)] // eloignement
        [TestCase(10f, 20f, -10f, -5f, false)] // meme position, heures differentes
        public void PredictionRequiresOverlappingSpaceAndTime(float x, float z, float vx, float vz, bool expected)
        {
            Assert.That(TrafficPerception.PredictCollision(new Vector3(x, 0f, z), new Vector3(vx, 0f, vz),
                new Vector3(2f, 1f, 4f), 3f, out var time, out var position), Is.EqualTo(expected));
            if (expected)
            {
                Assert.That(time, Is.InRange(0f, 3f));
                Assert.That(Mathf.Abs(position.x), Is.LessThanOrEqualTo(2.001f));
                Assert.That(Mathf.Abs(position.z), Is.LessThanOrEqualTo(4.001f));
            }
        }

        [Test]
        public void PredictionRejectsInvalidGeometry()
        {
            Assert.That(TrafficPerception.PredictCollision(Vector3.zero, Vector3.zero,
                new Vector3(-1f, 1f, 1f), 3f, out _, out _), Is.False);
            Assert.That(TrafficPerception.PredictCollision(Vector3.zero, Vector3.one * float.NaN,
                Vector3.one, 3f, out _, out _), Is.False);
        }

        [TestCase(45f)]
        [TestCase(135f)]
        [TestCase(225f)]
        [TestCase(315f)]
        public void AuthoredTunnelWallsDoNotBlockAnAiAtItsEntry(float yaw)
        {
            WithVehicle(controller =>
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_TunnelPortal.prefab");
                var tunnel = UnityEngine.Object.Instantiate(prefab, controller.transform.position, Quaternion.Euler(0f, yaw, 0f));
                GameObject obstacle = null;
                try
                {
                    LaneNode entry = null;
                    foreach (var node in tunnel.GetComponentsInChildren<LaneNode>())
                        if (node.IsEntryPortal) { entry = node; break; }
                    Assert.That(entry, Is.Not.Null, "Le prefab reel doit porter un portail d'entree.");
                    controller.transform.SetPositionAndRotation(entry.transform.position, entry.transform.rotation);
                    Physics.SyncTransforms();
                    var profile = ((DriverProfileDef)Field(controller, "driverProfile").GetValue(controller)).Profile;
                    Invoke<object>(controller, "RefreshPerception", profile);
                    Assert.That(controller.PerceptionSaturated, Is.False);
                    Assert.That((bool)Field(controller, "hasPerceivedLeader").GetValue(controller), Is.False,
                        "Un mur lateral ne constitue pas un leader devant le pare-chocs.");
                    Assert.That(float.IsPositiveInfinity(controller.PredictedCollisionTime), Is.True,
                        "Les murs orientes ne doivent pas provoquer un freinage au spawn.");
                    Assert.That(Invoke<bool>(controller, "TickLocalTraffic", profile, .02f, 0f, 0f), Is.False,
                        "La securite doit laisser la conduite nominale accelerer hors du tunnel.");

                    obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obstacle.transform.SetPositionAndRotation(controller.transform.position + controller.transform.forward * 7f
                        + Vector3.up, controller.transform.rotation);
                    obstacle.transform.localScale = new Vector3(2f, 2f, 2f);
                    Physics.SyncTransforms();
                    Invoke<object>(controller, "RefreshPerception", profile);
                    Assert.That((bool)Field(controller, "hasPerceivedLeader").GetValue(controller), Is.True,
                        "Le correctif doit conserver la detection d'un vrai obstacle dans le tunnel.");
                }
                finally
                {
                    if (obstacle != null) UnityEngine.Object.DestroyImmediate(obstacle);
                    UnityEngine.Object.DestroyImmediate(tunnel);
                }
            });
        }

        [TestCase(0f)]
        [TestCase(45f)]
        [TestCase(135f)]
        public void OrientedPredictionIsInvariantUnderGlobalRotation(float yaw)
        {
            var rotation = Quaternion.Euler(0f, yaw, 0f);
            var size = new Vector3(1f, .7f, 2f);
            Assert.That(TrafficPerception.PredictOrientedCollision(rotation * new Vector3(0f, 0f, 20f),
                rotation * new Vector3(0f, 0f, -10f), size, rotation, size, rotation, .3f, 3f,
                out var time, out _), Is.True);
            Assert.That(time, Is.EqualTo(1.57f).Within(.001f));
            Assert.That(TrafficPerception.PredictOrientedCollision(rotation * new Vector3(4f, 0f, 20f),
                rotation * new Vector3(0f, 0f, -10f), size, rotation, size, rotation, .3f, 3f,
                out _, out _), Is.False, "La voie voisine reste libre meme en diagonale.");
        }

        [TestCase(0f)]
        [TestCase(45f)]
        [TestCase(135f)]
        [TestCase(225f)]
        public void OpposingTrafficInSeparateLanesIsNeitherLeaderNorCollisionRisk(float yaw)
        {
            WithVehicle(controller =>
            {
                var rotation = Quaternion.Euler(0f, yaw, 0f);
                controller.transform.rotation = rotation;
                var body = controller.GetComponent<Rigidbody>();
                body.linearVelocity = rotation * Vector3.forward * 8f;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab");
                var opposite = UnityEngine.Object.Instantiate(prefab,
                    controller.transform.position + rotation * new Vector3(4f, 0f, 12f),
                    rotation * Quaternion.Euler(0f, 180f, 0f));
                try
                {
                    opposite.GetComponent<Rigidbody>().linearVelocity = rotation * Vector3.back * 8f;
                    Physics.SyncTransforms();
                    var profile = ((DriverProfileDef)Field(controller, "driverProfile").GetValue(controller)).Profile;
                    Invoke<object>(controller, "RefreshPerception", profile);
                    Assert.That((bool)Field(controller, "hasPerceivedLeader").GetValue(controller), Is.False,
                        "Les centres des deux voies authorisees sont separes de 4 m.");
                    Assert.That(float.IsPositiveInfinity(controller.PredictedCollisionTime), Is.True,
                        "Se rapprocher en sens opposes ne constitue pas un risque si les gabarits ne se croisent pas.");
                    Assert.That(Invoke<bool>(controller, "TickLocalTraffic", profile, .02f, 8f, 8f), Is.False);

                    opposite.transform.position = controller.transform.position + rotation * new Vector3(.5f, 0f, 12f);
                    Physics.SyncTransforms();
                    Invoke<object>(controller, "RefreshPerception", profile);
                    Assert.That(controller.PredictedCollisionTime, Is.InRange(0f, profile.PredictionSeconds),
                        "Le meme vehicule qui empiete dans notre voie doit redevenir un danger.");
                    Assert.That(Invoke<bool>(controller, "TickLocalTraffic", profile, .02f, 8f, 8f), Is.True);
                    var intent = (VehicleDriveIntent)Field(controller.GetComponent<VehiclePhysicsBody>(), "driveIntent")
                        .GetValue(controller.GetComponent<VehiclePhysicsBody>());
                    Assert.That(intent.BrakeReverse, Is.GreaterThan(0f), "Le danger reel commande un freinage.");
                }
                finally { UnityEngine.Object.DestroyImmediate(opposite); }
            });
        }

        [Test]
        public void RightOfWayIsReciprocalAndRoadDirectionWinsBeforeIdentifier()
        {
            Assert.That(TrafficPerception.MustYield(true, 0f, 99, false, 0f, 1), Is.False);
            Assert.That(TrafficPerception.MustYield(false, 0f, 1, true, 0f, 99), Is.True);
            Assert.That(TrafficPerception.MustYield(true, .1f, 99, true, 1f, 1), Is.False);
            Assert.That(TrafficPerception.MustYield(true, 0f, 9, true, 0f, 3), Is.True);
            Assert.That(TrafficPerception.MustYield(true, 0f, 3, true, 0f, 9), Is.False);
        }

        [Test]
        public void RedirectUsesADirectedReachableExitAndNeverJumpsAcrossADeadEnd()
        {
            IReadOnlyList<int>[] graph =
            {
                new[] { 1, 2 }, // 1 parait proche mais finit en cul-de-sac; 2 rejoint la sortie.
                Array.Empty<int>(),
                new[] { 3 },
                new[] { 4 },
                Array.Empty<int>()
            };

            Assert.That(LaneGraphRouting.FindNextTowardReachableExit(
                0, graph.Length, node => graph[node], node => node == 4), Is.EqualTo(2));
            Assert.That(LaneGraphRouting.FindNextTowardReachableExit(
                1, graph.Length, node => graph[node], node => node == 4), Is.EqualTo(-1),
                "Un cul-de-sac reste sur place : aucune reinsertion directe au portail n'est permise.");
        }

        [Test]
        public void SegmentProjectionDoesNotConfuseNearestEndpointWithNearestRoad()
        {
            Assert.That(TrafficPerception.ClosestPointOnSegment(new Vector3(2f, 0f, 50f),
                Vector3.zero, new Vector3(0f, 0f, 100f)), Is.EqualTo(new Vector3(0f, 0f, 50f)));
            Assert.That(typeof(LaneGraph).GetField("built", BindingFlags.Instance | BindingFlags.NonPublic).IsNotSerialized,
                Is.True, "Le drapeau ne peut survivre seul a la recharge qui vide ses listes readonly.");
        }

        [Test]
        public void EveryDispositionPreservesAuthoredSafetyParameters()
        {
            var def = AssetDatabase.LoadAssetAtPath<DriverProfileDef>("Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset");
            Assert.That(def.TryValidate(out var error), Is.True, error);
            foreach (RageDisposition disposition in Enum.GetValues(typeof(RageDisposition)))
            {
                var effective = DriverModel.ResolveEffectiveProfile(def.Profile, disposition);
                Assert.That(effective.PredictionSeconds, Is.EqualTo(def.Profile.PredictionSeconds));
                Assert.That(effective.SafetyMargin, Is.EqualTo(def.Profile.SafetyMargin));
                Assert.That(effective.ReverseSpeed, Is.EqualTo(def.Profile.ReverseSpeed));
                Assert.That(effective.ManeuverTimeout, Is.EqualTo(def.Profile.ManeuverTimeout));
                Assert.That(effective.PathSampleDistance, Is.EqualTo(def.Profile.PathSampleDistance));
            }
        }

        [Test]
        public void EmergencyStopNeverEngagesReverseTorqueAtRestOrWhileReversing()
        {
            WithVehicle(controller =>
            {
                var physics = controller.GetComponent<VehiclePhysicsBody>();
                foreach (var speed in new[] { 0f, -.5f, physics.Profile.MinimumDirectionSpeed, 5f })
                {
                    var intent = Invoke<VehicleDriveIntent>(controller, "ResolveStopIntent", speed);
                    Assert.That(VehicleTireModel.ResolveWheelDriveTorque(intent.Throttle, intent.BrakeReverse, speed,
                        physics.Profile.MinimumDirectionSpeed, physics.Profile.EngineTorque, physics.Profile.ReverseTorque,
                        physics.Profile.MaxForwardSpeed, physics.Profile.MaxReverseSpeed), Is.EqualTo(0f));
                    Assert.That(speed > physics.Profile.MinimumDirectionSpeed ? intent.BrakeReverse : intent.Handbrake, Is.EqualTo(1f));
                }
            });
        }

        [Test]
        public void SnapshotIsCadencedAndSaturationClosesThePath()
        {
            WithVehicle(controller =>
            {
                var def = (DriverProfileDef)Field(controller, "driverProfile").GetValue(controller);
                Field(controller, "hasSnapshot").SetValue(controller, true);
                Field(controller, "perceptionElapsedSeconds").SetValue(controller, 0f);
                var args = new object[] { def.Profile, def.Profile.PerceptionInterval * .5f, 0f, 0f };
                typeof(NetworkedAIVehicleDriverController).GetMethod("TryDetectLeader", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(controller, args);
                Assert.That(Field(controller, "refreshedThisStep").GetValue(controller), Is.False);
                Field(controller, "perceptionSaturated").SetValue(controller, true);
                Assert.That(Invoke<bool>(controller, "ValidatePath", def.Profile,
                    new[] { controller.transform.position + Vector3.forward * 5f }, 0, 1, false, false, false), Is.False);
            });
        }

        [Test]
        public void ManeuverTargetsStayFixedAndCancelStartsCooldown()
        {
            WithVehicle(controller =>
            {
                var target = controller.transform.position + Vector3.forward * 5f;
                var targets = (Vector3[])Field(controller, "maneuverTargets").GetValue(controller);
                targets[0] = target;
                Invoke<object>(controller, "BeginManeuver", TrafficUnblockingAction.Reverse, 1);
                controller.transform.position += Vector3.back;
                Assert.That(controller.ManeuverTarget, Is.EqualTo(target));
                var profile = ((DriverProfileDef)Field(controller, "driverProfile").GetValue(controller)).Profile;
                Invoke<object>(controller, "CancelManeuver", "test veto", profile);
                Assert.That((float)Field(controller, "retryAfter").GetValue(controller), Is.GreaterThanOrEqualTo(Time.time));
                Assert.That(controller.TrafficRefusal, Is.EqualTo("test veto"));
                Assert.That(Invoke<bool>(controller, "HasManeuverFailed", TrafficUnblockingAction.Reverse), Is.True,
                    "Une manoeuvre engagee puis abandonnee ne doit pas repartir en boucle dans le meme episode.");
            });
        }

        private static void WithVehicle(Action<NetworkedAIVehicleDriverController> action)
        {
            WithVehicleInstance(action);
        }

        [Test]
        public void ChassisSweepRejectsCentralObstacleAndUnknownGroundButPermitsSeparatingReverse()
        {
            WithVehicle(controller =>
            {
                var origin = controller.transform.position;
                var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                try
                {
                    road.name = "Col_Roadway";
                    road.transform.position = origin - Vector3.up * .1f;
                    road.transform.localScale = new Vector3(20f, .2f, 40f);
                    obstacle.transform.position = origin + new Vector3(0f, 1f, 5f);
                    obstacle.transform.localScale = Vector3.one * 2f;
                    Physics.SyncTransforms();
                    var profile = ((DriverProfileDef)Field(controller, "driverProfile").GetValue(controller)).Profile;
                    Assert.That(Invoke<bool>(controller, "ValidatePath", profile, new[] { origin + Vector3.forward * 10f }, 0, 1, false, false, false), Is.False);
                    obstacle.transform.position = origin + new Vector3(0f, 1f, 3.25f);
                    Physics.SyncTransforms();
                    Assert.That(Invoke<bool>(controller, "ValidatePath", profile, new[] { origin + Vector3.back * 3f }, 0, 1, true, false, false), Is.True);
                    obstacle.transform.position = origin + new Vector3(0f, 1f, -3f);
                    Physics.SyncTransforms();
                    Assert.That(Invoke<bool>(controller, "ValidatePath", profile, new[] { origin + Vector3.back * 3f }, 0, 1, true, false, false), Is.False);
                    obstacle.SetActive(false);
                    road.name = "UnknownGround";
                    Physics.SyncTransforms();
                    Assert.That(Invoke<bool>(controller, "ValidatePath", profile, new[] { origin + Vector3.forward * 8f }, 0, 1, false, true, false), Is.False);
                    road.name = "Col_Sidewalk_Test";
                    Assert.That(Invoke<bool>(controller, "ValidatePath", profile, new[] { origin + Vector3.forward * 8f }, 0, 1, false, false, false), Is.False);
                    Assert.That(Invoke<bool>(controller, "ValidatePath", profile, new[] { origin + Vector3.forward * 8f }, 0, 1, false, true, false), Is.True);
                }
                finally { UnityEngine.Object.DestroyImmediate(obstacle); UnityEngine.Object.DestroyImmediate(road); }
            });
        }

        private static void WithVehicleInstance(Action<NetworkedAIVehicleDriverController> action)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/RoadRage/Prefabs/Greybox_AIVehicle.prefab");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                instance.transform.position = new Vector3(2000f, 10f, 2000f);
                var controller = instance.GetComponent<NetworkedAIVehicleDriverController>();
                instance.GetComponent<VehiclePhysicsBody>().BindProfile(AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(
                    "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset"));
                Invoke<object>(controller, "CacheComponents");
                Invoke<object>(controller, "CacheVehicleExtents");
                action(controller);
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }

        private static FieldInfo Field(object target, string name) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        private static T Invoke<T>(object target, string name, params object[] args)
            => (T)target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
    }
}
