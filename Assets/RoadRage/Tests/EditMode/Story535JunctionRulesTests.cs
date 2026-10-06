using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.35, phase 3 -- regles pures du coordinateur sur le modele authore de MVP_Run (compile depuis le document committe,
    /// sans admission : la signature Gate A est un acte proprietaire) : preseance routiere et priorite a droite, creneau, Stop
    /// (fixture synthetique : branche de T passee en Stop), briseur, reservation filtree, compatibilite reelle exhaustive, et
    /// priorite locale d'anneau sur Roundabout_SouthWest reel (GrantedMergeGap, P11, rapport perime).
    /// </summary>
    [Category("Core")]
    [Category("Story535")]
    public sealed class Story535JunctionRulesTests
    {
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const float Dt = 0.02f;
        private const float CarLength = 4.44f;
        private static readonly Vector3[] FarAway = { new Vector3(1000f, 0f, 1000f), new Vector3(1000f, 0f, 1000f),
            new Vector3(1000f, 0f, 1000f), new Vector3(1000f, 0f, 1000f) };

        private static CompiledRoadModel model;
        private static CompiledRoadModel stopModel;

        private static CompiledRoadModel Model
        {
            get { return model ?? (model = RoadModelCompiler.Compile(RoadModelDocument.Load(File.ReadAllText(TrafficV2Settings.ModelPath)))); }
        }

        /// <summary>Fixture synthetique deterministe : la branche de TJunction_West passee de Yield a Stop (ligne conservee).</summary>
        private static CompiledRoadModel StopModel
        {
            get
            {
                if (stopModel != null) return stopModel;
                var source = RoadModelDocument.Load(File.ReadAllText(TrafficV2Settings.ModelPath));
                var branch = Movement(Model, "TJunction_West", "Junction_FromSouth -> Connector_West_Out");
                int index = Array.FindIndex(source.Controls, c => c.ControlledMovementIds.Contains(branch));
                Assert.That(source.Controls[index].Kind, Is.EqualTo(JunctionControlKind.Yield));
                source.Controls[index].Kind = JunctionControlKind.Stop;
                return stopModel = RoadModelCompiler.Compile(source);
            }
        }

        private static DriverProfile Driver { get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile; } }
        private static float Reservation { get { return CarLength + Driver.MinimumGap; } }
        private static RoadId Id(int n) { return new RoadId(0x535UL, (ulong)n); }

        private static RoadId Movement(CompiledRoadModel m, string junction, string label)
        {
            var found = m.Movements.Where(x => x.Label.StartsWith(junction + ": ", StringComparison.Ordinal) && x.Label.Contains(label)).ToList();
            Assert.That(found.Count, Is.EqualTo(1), junction + " / " + label);
            return found[0].Id;
        }

        private static JunctionTraversal Traversal(JunctionConflictIndex index, params RoadId[] movements)
        {
            return new JunctionTraversal(index.JunctionOf(movements[0]), movements, index.ToCorridorOf(movements[movements.Length - 1]));
        }

        private static JunctionKinematics Kinematics(float v)
        {
            var driver = Driver;
            return new JunctionKinematics(v, driver.MaxAcceleration, driver.DesiredSpeed, driver.ComfortableDeceleration, CarLength);
        }

        /// <summary>Distances de chaque mouvement de la traversee depuis le pare-chocs : d au premier (moins b), puis longueurs et corridors.</summary>
        private static float[] Starts(CompiledRoadModel m, JunctionConflictIndex index, JunctionTraversal traversal, float firstStart)
        {
            var starts = new float[traversal.MovementIds.Count];
            starts[0] = firstStart;
            for (int i = 1; i < starts.Length; i++)
            {
                RoadId previous = traversal.MovementIds[i - 1];
                float between = 0f;
                EffectiveLaneCorridor corridor;
                if (index.ToCorridorOf(previous) != index.FromCorridorOf(traversal.MovementIds[i])
                    && m.TryGetCorridor(index.ToCorridorOf(previous), out corridor)) between = corridor.LengthMeters;
                else if (m.TryGetCorridor(index.ToCorridorOf(previous), out corridor)) between = corridor.LengthMeters;
                starts[i] = starts[i - 1] + index.LengthOf(previous) + between;
            }
            return starts;
        }

        /// <summary>Demandeur a d de la frontiere b, vitesse v ; demande valide si d &lt;= D_request (sinon TooFar).</summary>
        private static JunctionActorReport Requesting(CompiledRoadModel m, RoadId id, JunctionTraversal traversal, float d, float v,
            float exitFree = 100f, bool head = true)
        {
            var index = JunctionConflictIndex.For(m);
            var distances = JunctionDistances.For(Driver, v, Dt, TrafficV2Settings.JunctionStopControlMarginMeters);
            float b = index.BoundaryOf(traversal.FirstMovementId);
            var approach = new JunctionApproach(traversal, d, distances, head, RoadId.None, false, false,
                new JunctionExitAssessment(exitFree, JunctionExitBound.Occupant, RoadId.None, Reservation), Starts(m, index, traversal, d - b), b);
            bool valid = head && d <= distances.RequestThresholdMeters;
            var positions = traversal.MovementIds.Select(x => new JunctionMovementPosition(x, JunctionMovementStatus.Ahead)).ToArray();
            return new JunctionActorReport(id, true, index.FromCorridorOf(traversal.FirstMovementId), FarAway, null, null, positions,
                new[] { approach }, true, valid, valid ? JunctionRequestRejection.None : head ? JunctionRequestRejection.TooFar
                    : JunctionRequestRejection.NotHeadOfQueue, Reservation, Kinematics(v));
        }

        /// <summary>
        /// Vehicule engage dans la traversee : mouvements avant <paramref name="occupied"/> derriere, celui-ci occupe, suivants devant ;
        /// pare-chocs a <paramref name="sInMovement"/> m dans le mouvement occupe ; approche engagee avec les distances de depart.
        /// </summary>
        private static JunctionActorReport Inside(CompiledRoadModel m, RoadId id, JunctionTraversal traversal, int occupied, float sInMovement,
            float v, bool localized = true, bool fallback = false)
        {
            var index = JunctionConflictIndex.For(m);
            var movements = traversal.MovementIds;
            var positions = movements.Select((x, i) => new JunctionMovementPosition(x, i < occupied ? JunctionMovementStatus.Behind
                : i == occupied ? JunctionMovementStatus.Occupied : JunctionMovementStatus.Ahead)).ToArray();
            var remaining = new JunctionTraversal(traversal.JunctionId, movements.Skip(occupied).ToArray(), traversal.ExitCorridorId);
            var distances = JunctionDistances.For(Driver, v, Dt, TrafficV2Settings.JunctionStopControlMarginMeters);
            var approach = new JunctionApproach(remaining, -sInMovement, distances, true, RoadId.None, true, true, default(JunctionExitAssessment),
                Starts(m, index, remaining, -sInMovement));
            Junction junction;
            m.TryGetJunction(traversal.JunctionId, out junction);
            var corners = Enumerable.Repeat(junction.Boundary.Center, 4).ToArray();
            return new JunctionActorReport(id, localized, localized ? movements[occupied] : RoadId.None, corners,
                localized ? new[] { movements[occupied] } : new RoadId[0], localized ? new[] { remaining } : new JunctionTraversal[0],
                localized ? positions : null, localized ? new[] { approach } : null, false, false, JunctionRequestRejection.NoTraversal, Reservation,
                Kinematics(v), fallback);
        }

        private static JunctionRecord Decision(JunctionSnapshot snapshot, RoadId id, RoadId traversal)
        {
            JunctionRecord record;
            Assert.That(snapshot.TryGetDecision(id, traversal, out record), Is.True, "record de " + id + "\n" + snapshot.ToText());
            return record;
        }

        private static void AssertDecision(JunctionSnapshot snapshot, RoadId id, RoadId traversal, JunctionGrantStatus status, JunctionReason reason,
            RoadId cause = default(RoadId))
        {
            var record = Decision(snapshot, id, traversal);
            Assert.That(record.Status, Is.EqualTo(status), snapshot.ToText());
            Assert.That(record.Reason, Is.EqualTo(reason), snapshot.ToText());
            if (!cause.IsEmpty) Assert.That(record.CauseActorId, Is.EqualTo(cause), snapshot.ToText());
        }

        /// <summary>Invariant 5.34 amende : deux grants effectifs incompatibles seulement sur des zones Merge, l'un admis par creneau.</summary>
        private static void AssertGrantInvariant(CompiledRoadModel m, JunctionSnapshot snapshot)
        {
            var index = JunctionConflictIndex.For(m);
            var effective = snapshot.Records.Where(r => r.IsEffectiveGrant).ToList();
            foreach (var a in effective)
                foreach (var b in effective)
                {
                    if (a.TrafficId.CompareTo(b.TrafficId) >= 0) continue;
                    foreach (var ma in a.MovementIds)
                        foreach (var mb in b.MovementIds)
                        {
                            RoadId zone;
                            ConflictKind kind;
                            float sa, sb;
                            if (!index.TryGetConflict(ma, mb, out zone, out kind, out sa, out sb)) continue;
                            Assert.That(kind == ConflictKind.Merge && (a.MergeGap || b.MergeGap), Is.True,
                                "grants incompatibles effectifs hors creneau de fusion : " + a.ToText() + " / " + b.ToText());
                        }
                }
        }

        private static JunctionSnapshot Batch(CompiledRoadModel m, JunctionCoordinator coordinator, ulong frame, params JunctionActorReport[] reports)
        {
            var snapshot = coordinator.Resolve(frame, reports);
            AssertGrantInvariant(m, snapshot);
            return snapshot;
        }

        // ============================================================ creneau : fonctions pures

        [Test]
        public void TravelAndGapFunctionsAreBoundedAtTheEdges()
        {
            Assert.That(JunctionPriority.TravelSeconds(0f, 0f, 1.5f, 8f), Is.EqualTo(0f));
            Assert.That(JunctionPriority.TravelSeconds(-3f, 4f, 1.5f, 8f), Is.EqualTo(0f));
            Assert.That(JunctionPriority.TravelSeconds(10f, 0f, 0f, 8f), Is.EqualTo(float.PositiveInfinity), "v = 0 sans acceleration.");
            Assert.That(JunctionPriority.TravelSeconds(10f, 2f, 0f, 8f), Is.EqualTo(5f).Within(1e-5f));
            Assert.That(JunctionPriority.TravelSeconds(10f, 0f, 1.5f, 0f), Is.EqualTo(float.PositiveInfinity), "plafond nul.");
            // Depuis l'arret, sans atteindre le plafond : √(2D/a).
            Assert.That(JunctionPriority.TravelSeconds(3f, 0f, 1.5f, 8f), Is.EqualTo(2f).Within(1e-5f));
            // Plafond atteint : acceleration puis croisiere.
            Assert.That(JunctionPriority.TravelSeconds(100f, 0f, 2f, 4f), Is.EqualTo(2f + (100f - 4f) / 4f).Within(1e-4f));
            // Vitesse initiale au-dessus du plafond : ramenee au plafond (plus long, conservatif pour le demandeur).
            Assert.That(JunctionPriority.TravelSeconds(8f, 10f, 1.5f, 4f), Is.EqualTo(2f).Within(1e-5f));
            Assert.That(JunctionPriority.TravelSeconds(float.NaN, 1f, 1f, 1f), Is.EqualTo(float.PositiveInfinity));
            Assert.That(JunctionPriority.MovementSpeedCap(8f, 2f, 0f), Is.EqualTo(8f));
            Assert.That(JunctionPriority.MovementSpeedCap(8f, 2f, 0.5f), Is.EqualTo(2f).Within(1e-5f));
            Assert.That(JunctionPriority.GapSeconds(4.7f, 0.04f), Is.EqualTo(4.7f + 0.04f + JunctionPriority.JunctionGapMarginSeconds).Within(1e-5f));
            Assert.That(JunctionPriority.JunctionGapMarginSeconds, Is.EqualTo(1f));
            Assert.That(JunctionPriority.StopHaltSpeedMetersPerSecond, Is.EqualTo(0.05f));
        }

        // ============================================================ priorite routiere (T)

        [Test]
        public void TheThroughAxisPassesFirstAndTheBranchYieldsWhenTheGapIsShort()
        {
            var index = JunctionConflictIndex.For(Model);
            var axis = Traversal(index, Movement(Model, "TJunction_West", "Junction_FromEast -> Connector_West_Out"));
            var branch = Traversal(index, Movement(Model, "TJunction_West", "Junction_FromSouth -> Connector_West_Out"));
            RoadId zone;
            Assert.That(index.TryGetConflict(axis.FirstMovementId, branch.FirstMovementId, out zone), Is.True, "Premisse : traversees incompatibles.");
            Assert.That(index.HasPrecedence(axis.FirstMovementId, branch.FirstMovementId), Is.True);
            Assert.That(index.HasPrecedence(branch.FirstMovementId, axis.FirstMovementId), Is.False);

            // La branche demande d'abord (plus ancienne) ; l'axe arrive ensuite, proche.
            var coordinator = new JunctionCoordinator(Model);
            Batch(Model, coordinator, 1, Requesting(Model, Id(1), branch, 0.3f, 0f));
            var both = Batch(Model, coordinator, 2, Requesting(Model, Id(1), branch, 0.3f, 0f), Requesting(Model, Id(2), axis, 8f, 6f));
            // Le grant 1 de la branche est deja emis au lot 1 (aucun prioritaire) : il n'est pas revoque pour priorite.
            AssertDecision(both, Id(1), branch.FirstMovementId, JunctionGrantStatus.Held, JunctionReason.Pending);

            // Arrivees simultanees : l'axe est servi, la branche cede, quel que soit l'ordre des rapports.
            foreach (var order in new[] { new[] { 0, 1 }, new[] { 1, 0 } })
            {
                var fresh = new JunctionCoordinator(Model);
                var reports = new[] { Requesting(Model, Id(1), branch, 0.3f, 0f), Requesting(Model, Id(2), axis, 8f, 6f) };
                var snapshot = Batch(Model, fresh, 1, order.Select(i => reports[i]).ToArray());
                AssertDecision(snapshot, Id(2), axis.FirstMovementId, JunctionGrantStatus.Granted, JunctionReason.Granted);
                AssertDecision(snapshot, Id(1), branch.FirstMovementId, JunctionGrantStatus.Denied, JunctionReason.YieldToPriority, Id(2));
                Assert.That(snapshot.Counters.YieldToPriority, Is.EqualTo(1));
            }
        }

        [Test]
        public void ASufficientGapServesTheYieldWithoutStopping()
        {
            var index = JunctionConflictIndex.For(Model);
            var axis = Traversal(index, Movement(Model, "TJunction_West", "Junction_FromEast -> Connector_West_Out"));
            var branch = Traversal(index, Movement(Model, "TJunction_West", "Junction_FromSouth -> Connector_West_Out"));
            // Axe loin (TooFar), lent : ETA superieure au creneau de la branche ; la branche est servie en roulant.
            var snapshot = Batch(Model, new JunctionCoordinator(Model), 1, Requesting(Model, Id(1), branch, 6f, 3f), Requesting(Model, Id(2), axis, 90f, 1f));
            Assert.That(snapshot.Records.Any(r => r.TrafficId == Id(2) && r.Status == JunctionGrantStatus.Denied && r.Reason == JunctionReason.InvalidRequest), Is.False);
            AssertDecision(snapshot, Id(1), branch.FirstMovementId, JunctionGrantStatus.Granted, JunctionReason.Granted);
        }

        [Test]
        public void ARequestBlockedAtItsExitIsEvaluatedBeforePriorityAndAnExitBlockedPriorityDoesNotCount()
        {
            var index = JunctionConflictIndex.For(Model);
            var axis = Traversal(index, Movement(Model, "TJunction_West", "Junction_FromEast -> Connector_West_Out"));
            var branch = Traversal(index, Movement(Model, "TJunction_West", "Junction_FromSouth -> Connector_West_Out"));
            var exitFirst = Batch(Model, new JunctionCoordinator(Model), 1, Requesting(Model, Id(1), branch, 0.3f, 0f, exitFree: 0f),
                Requesting(Model, Id(2), axis, 8f, 6f));
            AssertDecision(exitFirst, Id(1), branch.FirstMovementId, JunctionGrantStatus.Denied, JunctionReason.ExitBlocked);

            var ignored = Batch(Model, new JunctionCoordinator(Model), 1, Requesting(Model, Id(1), branch, 0.3f, 0f),
                Requesting(Model, Id(2), axis, 8f, 6f, exitFree: 0f));
            AssertDecision(ignored, Id(2), axis.FirstMovementId, JunctionGrantStatus.Denied, JunctionReason.ExitBlocked);
            AssertDecision(ignored, Id(1), branch.FirstMovementId, JunctionGrantStatus.Granted, JunctionReason.Granted);
        }

        [Test]
        public void ASeniorReservationNeverBlocksAYoungerRequestWithPrecedence()
        {
            var index = JunctionConflictIndex.For(Model);
            var axis = Traversal(index, Movement(Model, "TJunction_West", "Junction_FromEast -> Connector_West_Out"));
            var branch = Traversal(index, Movement(Model, "TJunction_West", "Junction_FromSouth -> Connector_West_Out"));
            var blocker = Traversal(index, Movement(Model, "TJunction_West", "Junction_FromWest -> Connector_East_Out"));
            var coordinator = new JunctionCoordinator(Model);
            // L'ancien (branche) est refuse a cause d'un occupant incompatible ; le jeune (axe) n'est pas « SeniorRequestPending ».
            RoadId zone;
            Assert.That(index.TryGetConflict(blocker.FirstMovementId, branch.FirstMovementId, out zone), Is.True);
            Assert.That(index.TryGetConflict(blocker.FirstMovementId, axis.FirstMovementId, out zone), Is.False, "Premisse : axes compatibles.");
            Batch(Model, coordinator, 1, Requesting(Model, Id(1), branch, 0.3f, 0f), Inside(Model, Id(3), blocker, 0, 1f, 3f));
            var snapshot = Batch(Model, coordinator, 2, Requesting(Model, Id(1), branch, 0.3f, 0f), Inside(Model, Id(3), blocker, 0, 2f, 3f),
                Requesting(Model, Id(2), axis, 8f, 6f));
            Assert.That(Decision(snapshot, Id(2), axis.FirstMovementId).Reason, Is.Not.EqualTo(JunctionReason.SeniorRequestPending), snapshot.ToText());
            AssertDecision(snapshot, Id(2), axis.FirstMovementId, JunctionGrantStatus.Granted, JunctionReason.Granted);
        }

        // ============================================================ priorite a droite (croix)

        private static void CrossroadsPair(RightOfWayRelation wanted, out RoadId first, out RoadId second)
        {
            var table = RightOfWayTable.For(Model);
            var index = JunctionConflictIndex.For(Model);
            foreach (var entry in table.Entries)
            {
                if (entry.Relation != wanted) continue;
                CompiledJunctionControl a, b;
                Model.TryGetControl(entry.ControlA, out a);
                Model.TryGetControl(entry.ControlB, out b);
                foreach (var ma in a.ControlledMovementIds)
                    foreach (var mb in b.ControlledMovementIds)
                    {
                        RoadId zone;
                        if (!index.TryGetConflict(ma, mb, out zone)) continue;
                        first = ma; second = mb;
                        return;
                    }
            }
            Assert.Fail("Aucune paire " + wanted + " incompatible a la croix.");
            first = second = RoadId.None;
        }

        [Test]
        public void TheApproachFromTheRightPassesFirstUnderAnyArrivalOrder()
        {
            RoadId self, right;
            CrossroadsPair(RightOfWayRelation.FromRight, out self, out right);
            var index = JunctionConflictIndex.For(Model);
            Assert.That(index.HasPrecedence(right, self), Is.True);
            Assert.That(index.HasPrecedence(self, right), Is.False);
            var a = Traversal(index, self);
            var b = Traversal(index, right);
            foreach (var order in new[] { new[] { 0, 1 }, new[] { 1, 0 } })
            {
                var reports = new[] { Requesting(Model, Id(1), a, 0.3f, 0f), Requesting(Model, Id(2), b, 0.3f, 0f) };
                var snapshot = Batch(Model, new JunctionCoordinator(Model), 1, order.Select(i => reports[i]).ToArray());
                AssertDecision(snapshot, Id(2), right, JunctionGrantStatus.Granted, JunctionReason.Granted);
                AssertDecision(snapshot, Id(1), self, JunctionGrantStatus.Denied, JunctionReason.YieldToPriority, Id(2));
            }
        }

        [Test]
        public void OpposedApproachesHaveNoPrecedenceAndKeepTheGenericTieBreak()
        {
            RoadId a, b;
            CrossroadsPair(RightOfWayRelation.Opposite, out a, out b);
            var index = JunctionConflictIndex.For(Model);
            Assert.That(index.HasPrecedence(a, b) || index.HasPrecedence(b, a), Is.False);
            var coordinator = new JunctionCoordinator(Model);
            Batch(Model, coordinator, 1, Requesting(Model, Id(5), Traversal(index, b), 9f, 4f, head: false));
            var snapshot = Batch(Model, coordinator, 2, Requesting(Model, Id(5), Traversal(index, b), 0.3f, 0f), Requesting(Model, Id(4), Traversal(index, a), 0.3f, 0f));
            // Meme anciennete (lot 2) : departage generique 5.34 par TrafficId.
            AssertDecision(snapshot, Id(4), a, JunctionGrantStatus.Granted, JunctionReason.Granted);
            Assert.That(Decision(snapshot, Id(5), b).Reason, Is.Not.EqualTo(JunctionReason.YieldToPriority), snapshot.ToText());
            Assert.That(snapshot.Counters.YieldToPriority, Is.EqualTo(0));
        }

        [Test]
        public void AFourWayRightOfWayCycleIsBrokenOnceForTheOldestAndAnOpenChainIsNot()
        {
            var index = JunctionConflictIndex.For(Model);
            var table = RightOfWayTable.For(Model);
            // Pour chaque approche de la croix, un mouvement en conflit avec un mouvement de l'approche de sa droite.
            var controls = table.Entries.Select(e => e.ControlA).Distinct().ToList();
            Assert.That(controls.Count, Is.EqualTo(4));
            var rightOf = controls.ToDictionary(c => c, c => table.Entries.Single(e => e.ControlA == c && e.Relation == RightOfWayRelation.FromRight).ControlB);
            // Les quatre tout-droits : chacun croise celui de sa droite, le cycle est ferme.
            var chosen = new Dictionary<RoadId, RoadId>();
            foreach (var control in controls)
            {
                CompiledJunctionControl self;
                Model.TryGetControl(control, out self);
                chosen[control] = self.ControlledMovementIds.Single(m => { CompiledJunctionMovement x; Model.TryGetMovement(m, out x); return x.Label.Contains("(tout droit)"); });
            }
            foreach (var control in controls)
            {
                RoadId zone;
                Assert.That(index.TryGetConflict(chosen[control], chosen[rightOf[control]], out zone), Is.True, "Premisse : tout-droits perpendiculaires incompatibles.");
            }
            var reports = controls.Select((c, i) => Requesting(Model, Id(10 + i), Traversal(index, chosen[c]), 0.3f, 0f)).ToArray();
            var snapshot = Batch(Model, new JunctionCoordinator(Model), 1, reports);
            Assert.That(snapshot.Counters.DeadlockBreaks, Is.EqualTo(1), snapshot.ToText());
            Assert.That(snapshot.Records.Count(r => r.Reason == JunctionReason.GrantedDeadlockBreak), Is.EqualTo(1));
            // Meme anciennete : le plus petit TrafficId.
            Assert.That(snapshot.Records.Single(r => r.Reason == JunctionReason.GrantedDeadlockBreak).TrafficId, Is.EqualTo(Id(10)));

            // Chaine ouverte : l'unique cause est un acteur TooFar, hors de W ; aucun briseur.
            var lone = controls[0];
            var open = Batch(Model, new JunctionCoordinator(Model), 1, Requesting(Model, Id(20), Traversal(index, chosen[lone]), 0.3f, 0f),
                Requesting(Model, Id(21), Traversal(index, chosen[rightOf[lone]]), 20f, 8f));
            Assert.That(open.Counters.DeadlockBreaks, Is.EqualTo(0), open.ToText());
            AssertDecision(open, Id(20), chosen[lone], JunctionGrantStatus.Denied, JunctionReason.YieldToPriority, Id(21));
        }

        // ============================================================ Stop (fixture synthetique)

        [Test]
        public void AStopNeedsAMarkedHaltThenAGapAndNoMinimumWait()
        {
            var index = JunctionConflictIndex.For(StopModel);
            var branch = Traversal(index, Movement(StopModel, "TJunction_West", "Junction_FromSouth -> Connector_West_Out"));
            Assert.That(index.ControlKindOf(branch.FirstMovementId), Is.EqualTo(JunctionControlKind.Stop));
            var coordinator = new JunctionCoordinator(StopModel);
            var rolling = Batch(StopModel, coordinator, 1, Requesting(StopModel, Id(1), branch, 0.3f, 0.5f));
            AssertDecision(rolling, Id(1), branch.FirstMovementId, JunctionGrantStatus.Denied, JunctionReason.StopRequired);
            Assert.That(rolling.Counters.StopRequired, Is.EqualTo(1));
            // Arret marque (v <= 0,05 m/s dans la fenetre de b) puis creneau libre : grant au meme lot, sans duree minimale.
            var halted = Batch(StopModel, coordinator, 2, Requesting(StopModel, Id(1), branch, 0.24f, 0.04f));
            AssertDecision(halted, Id(1), branch.FirstMovementId, JunctionGrantStatus.Granted, JunctionReason.Granted);

            // L'arret marque vit avec l'anciennete : une demande interrompue le perd.
            var restart = new JunctionCoordinator(StopModel);
            Batch(StopModel, restart, 1, Requesting(StopModel, Id(1), branch, 0.24f, 0f, exitFree: 0f));
            Batch(StopModel, restart, 2);
            var again = Batch(StopModel, restart, 3, Requesting(StopModel, Id(1), branch, 0.3f, 0.5f));
            AssertDecision(again, Id(1), branch.FirstMovementId, JunctionGrantStatus.Denied, JunctionReason.StopRequired);
        }

        // ============================================================ compatibilite reelle (P9)

        [Test]
        public void EveryCompatibleMovementPairOfMvpRunIsGrantedTogether()
        {
            var index = JunctionConflictIndex.For(Model);
            int pairs = 0;
            foreach (var junction in Model.Junctions)
            {
                var movements = Model.GetMovementsInJunction(junction.Id);
                foreach (var a in movements)
                    foreach (var b in movements)
                    {
                        RoadId zone;
                        if (a.CompareTo(b) >= 0 || index.FromCorridorOf(a) == index.FromCorridorOf(b) || index.TryGetConflict(a, b, out zone)) continue;
                        var snapshot = Batch(Model, new JunctionCoordinator(Model), 1, Requesting(Model, Id(1), Traversal(index, a), 0.3f, 0f),
                            Requesting(Model, Id(2), Traversal(index, b), 0.3f, 0f));
                        AssertDecision(snapshot, Id(1), a, JunctionGrantStatus.Granted, JunctionReason.Granted);
                        AssertDecision(snapshot, Id(2), b, JunctionGrantStatus.Granted, JunctionReason.Granted);
                        Assert.That(snapshot.Counters.YieldToPriority, Is.EqualTo(0));
                        pairs++;
                    }
            }
            Assert.That(pairs, Is.GreaterThan(0));
            TestContext.WriteLine("Paires compatibles d'approches differentes servies ensemble : " + pairs);
        }

        // ============================================================ priorite locale d'anneau (Roundabout_SouthWest reel)

        private const string Ring = "Roundabout_SouthWest";

        private static RoadId R(string label) { return Movement(Model, Ring, label); }

        [Test]
        public void AnEntryYieldsToARingVehicleJustBeforeTheMergeAndEntersWhenItIsFar()
        {
            var index = JunctionConflictIndex.For(Model);
            var entryWest = Traversal(index, R("Connector_West_In ->"));
            RoadId zone;
            ConflictKind kind;
            float sa, sb;
            Assert.That(index.TryGetConflict(entryWest.FirstMovementId, R("Ring_Split_West -> Ring_Merge_West"), out zone, out kind, out sa, out sb), Is.True);
            Assert.That(kind, Is.EqualTo(ConflictKind.Merge), "P12 resolue : fusion ouest typee Merge.");

            // Juste avant la fusion : H entre par la diagonale, continue a l'ouest, sort au sud ; pare-chocs a 4 m dans l'entree.
            var near = Traversal(index, R("Connector_Diagonal_In ->"), R("Ring_Split_West -> Ring_Merge_West"), R("Ring_Split_South -> Connector_South_Out"));
            var refused = Batch(Model, new JunctionCoordinator(Model), 1, Inside(Model, Id(1), near, 0, 4f, 3f), Requesting(Model, Id(2), entryWest, 2f, 2.8f));
            AssertDecision(refused, Id(2), entryWest.FirstMovementId, JunctionGrantStatus.Denied, JunctionReason.ConflictGranted, Id(1));
            Assert.That(refused.Counters.MergeGapRefusals, Is.EqualTo(1));

            // Loin avant la fusion : H a l'arret au debut de l'entree sud, deux mouvements avant la continuation ouest.
            var far = Traversal(index, R("Connector_South_In ->"), R("Ring_Split_Diagonal -> Ring_Merge_Diagonal"), R("Ring_Split_West -> Ring_Merge_West"),
                R("Ring_Split_South -> Connector_South_Out"));
            var admitted = Batch(Model, new JunctionCoordinator(Model), 1, Inside(Model, Id(1), far, 0, 0f, 0f), Requesting(Model, Id(2), entryWest, 2f, 2.8f));
            var grant = Decision(admitted, Id(2), entryWest.FirstMovementId);
            Assert.That(grant.Reason, Is.EqualTo(JunctionReason.GrantedMergeGap), admitted.ToText());
            Assert.That(grant.MergeGap, Is.True);
            Assert.That(admitted.Counters.MergeGapGrants, Is.EqualTo(1));

            // Meme position, entrant a l'arret : creneau plus long que l'ETA de H, refus.
            var stopped = Batch(Model, new JunctionCoordinator(Model), 1, Inside(Model, Id(1), far, 0, 0f, 0f), Requesting(Model, Id(2), entryWest, 2f, 0f));
            AssertDecision(stopped, Id(2), entryWest.FirstMovementId, JunctionGrantStatus.Denied, JunctionReason.ConflictGranted, Id(1));
        }

        [Test]
        public void ARingVehiclePastTheMergeOrLeavingBeforeTheEntryDoesNotBlock()
        {
            var index = JunctionConflictIndex.For(Model);
            var entryWest = Traversal(index, R("Connector_West_In ->"));
            // Apres la fusion : continuation ouest deja liberee (deux lots : grant restaure puis liberation).
            var past = Traversal(index, R("Connector_Diagonal_In ->"), R("Ring_Split_West -> Ring_Merge_West"), R("Ring_Split_South -> Connector_South_Out"));
            var coordinator = new JunctionCoordinator(Model);
            Batch(Model, coordinator, 1, Inside(Model, Id(1), past, 0, 4f, 3f));
            var after = Batch(Model, coordinator, 2, Inside(Model, Id(1), past, 2, 1f, 3f), Requesting(Model, Id(2), entryWest, 2f, 2.8f));
            AssertDecision(after, Id(2), entryWest.FirstMovementId, JunctionGrantStatus.Granted, JunctionReason.Granted);

            // Sortie au bras precedent (diagonale) : trajectoire restante sans zone avec l'entree ouest.
            var leaving = Traversal(index, R("Connector_South_In ->"), R("Ring_Split_Diagonal -> Connector_Diagonal_Out"));
            var other = Batch(Model, new JunctionCoordinator(Model), 1, Inside(Model, Id(1), leaving, 0, 3f, 3f), Requesting(Model, Id(2), entryWest, 2f, 0f));
            AssertDecision(other, Id(2), entryWest.FirstMovementId, JunctionGrantStatus.Granted, JunctionReason.Granted);

            // Plusieurs vehicules d'anneau, aucun ne menace le creneau ni ne tient de Crossing avec l'entree : servie sans arret.
            var several = Batch(Model, new JunctionCoordinator(Model), 1, Inside(Model, Id(1), leaving, 0, 3f, 3f),
                Inside(Model, Id(3), Traversal(index, R("Ring_Split_South -> Connector_South_Out")), 0, 1f, 3f), Requesting(Model, Id(2), entryWest, 2f, 2.8f));
            AssertDecision(several, Id(2), entryWest.FirstMovementId, JunctionGrantStatus.Granted, JunctionReason.Granted);
        }

        [Test]
        public void ARingVehicleLeavingAtTheEntryArmIsAStrictCrossing()
        {
            var index = JunctionConflictIndex.For(Model);
            var entryWest = Traversal(index, R("Connector_West_In ->"));
            // P11 : H tient la sortie du meme bras (Crossing prouve), meme loin : refus strict, jamais par creneau.
            var exiting = Traversal(index, R("Connector_Diagonal_In ->"), R("Ring_Split_West -> Connector_West_Out"));
            var snapshot = Batch(Model, new JunctionCoordinator(Model), 1, Inside(Model, Id(1), exiting, 0, 0f, 0f), Requesting(Model, Id(2), entryWest, 2f, 2.8f));
            AssertDecision(snapshot, Id(2), entryWest.FirstMovementId, JunctionGrantStatus.Denied, JunctionReason.ConflictGranted, Id(1));
            Assert.That(snapshot.Counters.CrossingRefusals, Is.EqualTo(1));
            Assert.That(snapshot.Counters.MergeGapGrants, Is.EqualTo(0));
        }

        [Test]
        public void AStaleOrUnlocalizedRingReportIsRefusedConservatively()
        {
            var index = JunctionConflictIndex.For(Model);
            var entryWest = Traversal(index, R("Connector_West_In ->"));
            var far = Traversal(index, R("Connector_South_In ->"), R("Ring_Split_Diagonal -> Ring_Merge_Diagonal"), R("Ring_Split_West -> Ring_Merge_West"),
                R("Ring_Split_South -> Connector_South_Out"));
            // Repli du titulaire : refus.
            var fallback = Batch(Model, new JunctionCoordinator(Model), 1, Inside(Model, Id(1), far, 0, 0f, 0f, fallback: true),
                Requesting(Model, Id(2), entryWest, 2f, 2.8f));
            AssertDecision(fallback, Id(2), entryWest.FirstMovementId, JunctionGrantStatus.Denied, JunctionReason.ConflictGranted, Id(1));
            Assert.That(fallback.Counters.MergeGapRefusals, Is.EqualTo(1));

            // Titulaire non localise : refus.
            var coordinator = new JunctionCoordinator(Model);
            Batch(Model, coordinator, 1, Inside(Model, Id(1), far, 0, 0f, 0f));
            var lost = Batch(Model, coordinator, 2, Inside(Model, Id(1), far, 0, 0f, 0f, localized: false), Requesting(Model, Id(2), entryWest, 2f, 2.8f));
            AssertDecision(lost, Id(2), entryWest.FirstMovementId, JunctionGrantStatus.Denied, JunctionReason.ConflictGranted, Id(1));

            // Rapport absent a la frame : le grant est revoque (ActorGone) et ne sert pas de titulaire.
            var gone = Batch(Model, coordinator, 3, Requesting(Model, Id(2), entryWest, 2f, 2.8f));
            Assert.That(gone.Records.Any(r => r.TrafficId == Id(1) && r.Reason == JunctionReason.ActorGone), Is.True);
        }

        [Test]
        public void ACrossingWithAFarHolderIsNeverAdmittedByGap()
        {
            var index = JunctionConflictIndex.For(Model);
            var axis = Traversal(index, Movement(Model, "TJunction_West", "Junction_FromEast -> Connector_West_Out"));
            var branch = Traversal(index, Movement(Model, "TJunction_West", "Junction_FromSouth -> Connector_West_Out"));
            RoadId zone;
            ConflictKind kind;
            float sa, sb;
            // Le titulaire tient un grant non engage, loin : la zone Crossing reste stricte.
            var crossing = Movement(Model, "TJunction_West", "Junction_FromWest -> Connector_South_Out");
            Assert.That(index.TryGetConflict(branch.FirstMovementId, crossing, out zone, out kind, out sa, out sb), Is.True);
            Assert.That(kind, Is.EqualTo(ConflictKind.Crossing));
            var coordinator = new JunctionCoordinator(Model);
            Batch(Model, coordinator, 1, Requesting(Model, Id(1), Traversal(index, crossing), 12f, 6f));
            var snapshot = Batch(Model, coordinator, 2, Requesting(Model, Id(1), Traversal(index, crossing), 12f, 6f),
                Requesting(Model, Id(2), branch, 0.3f, 0f));
            AssertDecision(snapshot, Id(2), branch.FirstMovementId, JunctionGrantStatus.Denied, JunctionReason.ConflictGranted, Id(1));
            Assert.That(snapshot.Counters.CrossingRefusals, Is.EqualTo(1));
        }

        // ============================================================ structure

        [Test]
        public void BatchWorkDependsOnTheJunctionActorsNotOnTheModelSize()
        {
            var index = JunctionConflictIndex.For(Model);
            var axis = Traversal(index, Movement(Model, "TJunction_West", "Junction_FromEast -> Connector_West_Out"));
            var branch = Traversal(index, Movement(Model, "TJunction_West", "Junction_FromSouth -> Connector_West_Out"));
            long builds = TrafficV2WorkCounters.Work.JunctionIndexBuilds;
            long before = TrafficV2WorkCounters.Work.JunctionPairChecks;
            Batch(Model, new JunctionCoordinator(Model), 1, Requesting(Model, Id(1), branch, 0.3f, 0f), Requesting(Model, Id(2), axis, 8f, 6f));
            long pairs = TrafficV2WorkCounters.Work.JunctionPairChecks - before;
            Assert.That(TrafficV2WorkCounters.Work.JunctionIndexBuilds, Is.EqualTo(builds), "Index construit une fois par modele.");
            Assert.That(pairs, Is.LessThan(Model.Movements.Count), "Paires testees bornees par les acteurs du carrefour, jamais par le modele.");
        }

        [Test]
        public void TheJunctionRulesCarryNoNetworkStateAndTimeNoStop()
        {
            // Liste 5.34 : aucun etat reseau ni minuterie dans Junction/ ; aucune duree d'attente dans les regles 5.35.
            foreach (var path in Directory.GetFiles("Assets/RoadRage/Features/Vehicles/Traffic/Junction", "*.cs"))
            {
                string text = File.ReadAllText(path);
                foreach (var forbidden in new[] { "NetworkVariable", "ServerRpc", "ClientRpc", "OnValueChanged", "Time.time", "Time.deltaTime",
                    "Time.fixedTime", "Stopwatch", "DateTime.Now" })
                    Assert.That(text.Contains(forbidden), Is.False, Path.GetFileName(path) + " contient " + forbidden);
            }
            Assert.That(Regex.IsMatch(File.ReadAllText("Assets/RoadRage/Features/Vehicles/Traffic/Junction/JunctionCoordinator.cs"), @"Wait(ed)?Seconds|StopDuration"),
                Is.False, "Aucun stop par minuterie.");
        }
    }
}
