using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Diagnostics;
using RoadRage.Features.Vehicles.Traffic.Frame;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.34, phase 1 : coordinateur de carrefour pur sur le modele MVP_Run. Matrice de la spec (cote coordinateur),
    /// paires de traversees de la croix et des T, faits de chaine, cas de l'anneau de 0,87 m, borne de sortie, determinisme a
    /// ordres melanges, anciennete, equite, cycle de vie, frame refusee, invariant par lot et cout structurel.
    /// </summary>
    [Category("Core")]
    [Category("Story534")]
    public sealed class Story534JunctionCoordinatorTests
    {
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const float Dt = 0.02f;

        private static readonly VehicleFootprint Car = new VehicleFootprint
            { FrontMeters = 2.22f, RearMeters = 2.22f, LeftMeters = 1.03f, RightMeters = 1.03f };
        private static readonly Vector3 FarAway = new Vector3(1000f, 0f, 1000f);

        /// <summary>ConflictZones[23] de MVP_Run (TJunction_West) : FromEast tout droit et FromSouth a gauche.</summary>
        private static readonly RoadId EastStraight = RoadId.Parse("40ca7f10a97f50a918e8c3a2a1e58493");
        private static readonly RoadId SouthLeft = RoadId.Parse("4e437f94525852d3a072a538537c2093");
        private static readonly RoadId Zone23 = RoadId.Parse("4258af5419bba1365a3f0ad6ed3d44aa");

        private static TrafficV2Admission admission;

        private static TrafficV2Admission Admission
        {
            get
            {
                if (admission == null)
                    admission = TrafficV2Lifecycle.Admit(File.ReadAllText(TrafficV2Settings.ModelPath),
                        File.ReadAllText(TrafficV2Settings.SignoffPath), File.ReadAllText(TrafficV2Settings.ReportPath));
                Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
                return admission;
            }
        }

        private static CompiledRoadModel Model { get { return Admission.Model; } }
        private static JunctionConflictIndex Index { get { return JunctionConflictIndex.For(Model); } }

        private static DriverProfile Driver
        {
            get { return AssetDatabase.LoadAssetAtPath<DriverProfileDef>(DriverProfilePath).Profile; }
        }

        private static float Reservation { get { return Car.FrontMeters + Car.RearMeters + Driver.MinimumGap; } }

        private static RoadId Id(int n) { return new RoadId(0x534UL, (ulong)n); }

        private static RoadId Movement(string prefix)
        {
            var found = Model.Movements.Where(m => m.Id.ToString().StartsWith(prefix, StringComparison.Ordinal)).ToList();
            Assert.That(found.Count, Is.EqualTo(1), "mouvement " + prefix);
            return found[0].Id;
        }

        private static JunctionTraversal Traversal(params RoadId[] movements)
        {
            return new JunctionTraversal(Index.JunctionOf(movements[0]), movements, Index.ToCorridorOf(movements[movements.Length - 1]));
        }

        private static Vector3[] At(Vector3 point) { return new[] { point, point, point, point }; }

        private static Vector3[] InJunction(RoadId junctionId)
        {
            Junction junction;
            Assert.That(Model.TryGetJunction(junctionId, out junction), Is.True);
            return At(junction.Boundary.Center);
        }

        private static JunctionApproach Approach(JunctionTraversal traversal, float d, float v, bool effective, bool head, float exitFree,
            JunctionExitBound bound = JunctionExitBound.Occupant)
        {
            var distances = JunctionDistances.For(Driver, v, Dt, TrafficV2Settings.JunctionStopControlMarginMeters);
            bool engaged = effective && (d <= 0f || d < distances.StopMeters);
            return new JunctionApproach(traversal, d, distances, head, RoadId.None, effective, engaged,
                engaged ? default(JunctionExitAssessment) : new JunctionExitAssessment(exitFree, bound, RoadId.None, Reservation));
        }

        /// <summary>Vehicule devant la traversee, a d de l'entree ; demande valide si tete de file et d &lt;= D_request.</summary>
        private static JunctionActorReport Requesting(RoadId id, JunctionTraversal traversal, float d = 10f, float v = 4f,
            bool head = true, float exitFree = 50f, bool effective = false, JunctionExitBound bound = JunctionExitBound.Occupant)
        {
            var approach = Approach(traversal, d, v, effective, head, exitFree, bound);
            bool request = !approach.Engaged;
            bool valid = request && head && d <= approach.Distances.RequestMeters;
            var rejection = valid ? JunctionRequestRejection.None : !request ? JunctionRequestRejection.NoTraversal
                : !head ? JunctionRequestRejection.NotHeadOfQueue : JunctionRequestRejection.TooFar;
            var positions = traversal.MovementIds.Select(m => new JunctionMovementPosition(m, JunctionMovementStatus.Ahead)).ToArray();
            return new JunctionActorReport(id, true, Index.FromCorridorOf(traversal.FirstMovementId), At(FarAway), null, null, positions,
                new[] { approach }, request, valid, rejection, Reservation);
        }

        /// <summary>Vehicule dans la traversee : mouvements [from, to] occupes, precedents derriere, suivants devant ; sans demande.</summary>
        private static JunctionActorReport Inside(RoadId id, JunctionTraversal traversal, int from = 0, int to = 0, bool localized = true,
            Vector3[] corners = null)
        {
            var movements = traversal.MovementIds;
            var positions = movements.Select((m, i) => new JunctionMovementPosition(m, i < from ? JunctionMovementStatus.Behind
                : i <= to ? JunctionMovementStatus.Occupied : JunctionMovementStatus.Ahead)).ToArray();
            var occupied = localized ? movements.Skip(from).Take(to - from + 1).ToArray() : new RoadId[0];
            var chain = localized ? new[] { new JunctionTraversal(traversal.JunctionId, movements.Skip(from).ToArray(), traversal.ExitCorridorId) }
                : new JunctionTraversal[0];
            return new JunctionActorReport(id, localized, localized ? movements[from] : RoadId.None,
                corners ?? InJunction(traversal.JunctionId), occupied, chain, localized ? positions : null, null, false, false,
                JunctionRequestRejection.Fallback, Reservation);
        }

        /// <summary>Vehicule sorti : tous les mouvements de la traversee derriere lui, sur le corridor de sortie.</summary>
        private static JunctionActorReport Cleared(RoadId id, JunctionTraversal traversal)
        {
            var positions = traversal.MovementIds.Select(m => new JunctionMovementPosition(m, JunctionMovementStatus.Behind)).ToArray();
            return new JunctionActorReport(id, true, traversal.ExitCorridorId, At(FarAway), null, null, positions, null, false, false,
                JunctionRequestRejection.NoTraversal, Reservation);
        }

        /// <summary>Vehicule present sans fait de carrefour (ni occupation, ni demande).</summary>
        private static JunctionActorReport Idle(RoadId id)
        {
            return new JunctionActorReport(id, true, RoadId.None, At(FarAway), null, null, null, null, false, false,
                JunctionRequestRejection.NoTraversal, Reservation);
        }

        private static JunctionRecord Decision(JunctionSnapshot snapshot, RoadId id, RoadId traversal)
        {
            JunctionRecord record;
            Assert.That(snapshot.TryGetDecision(id, traversal, out record), Is.True, "record de " + id + "\n" + snapshot.ToText());
            return record;
        }

        private static void AssertStatus(JunctionSnapshot snapshot, RoadId id, RoadId traversal, JunctionGrantStatus status,
            JunctionReason reason)
        {
            var record = Decision(snapshot, id, traversal);
            Assert.That(record.Status, Is.EqualTo(status), snapshot.ToText());
            Assert.That(record.Reason, Is.EqualTo(reason), snapshot.ToText());
        }

        private static bool HasEffective(JunctionSnapshot snapshot, RoadId id)
        {
            return snapshot.Records.Any(r => r.TrafficId == id && r.IsEffectiveGrant);
        }

        /// <summary>A aucun instant deux grants effectifs incompatibles d'acteurs distincts.</summary>
        private static void AssertNoIncompatibleEffectiveGrants(JunctionSnapshot snapshot)
        {
            var effective = snapshot.Records.Where(r => r.IsEffectiveGrant).ToList();
            foreach (var a in effective)
                foreach (var b in effective)
                {
                    if (a.TrafficId.CompareTo(b.TrafficId) >= 0) continue;
                    foreach (var ma in a.MovementIds)
                        foreach (var mb in b.MovementIds)
                        {
                            RoadId zone;
                            Assert.That(Index.TryGetConflict(ma, mb, out zone), Is.False,
                                "grants incompatibles effectifs : " + a.ToText() + " / " + b.ToText());
                        }
                }
        }

        /// <summary>Invariant par lot : chaque grant a un acteur present, demandeur, occupant ou engage sur sa traversee (O8).</summary>
        private static void AssertGrantsHaveARealHolder(JunctionSnapshot snapshot, IReadOnlyList<JunctionActorReport> reports)
        {
            foreach (var record in snapshot.Records.Where(r => r.IsEffectiveGrant))
            {
                var holder = reports.FirstOrDefault(r => r.TrafficId == record.TrafficId);
                Assert.That(holder, Is.Not.Null, "grant sans acteur present : " + record.ToText());
                JunctionApproach approach;
                bool requester = holder.RequestValid && holder.Request.Traversal.FirstMovementId == record.TraversalId;
                bool occupant = record.MovementIds.Any(m => holder.OccupiedMovements.Contains(m));
                bool engaged = holder.TryGetApproach(record.TraversalId, out approach) && approach.Engaged;
                bool inside = !holder.Localized && Index.InsideBoundary(record.JunctionId, holder.Corners);
                Assert.That(requester || occupant || engaged || inside, Is.True, "grant sans demande, occupant ni engagement : " + record.ToText());
            }
        }

        private static JunctionSnapshot Batch(JunctionCoordinator coordinator, ulong frame, params JunctionActorReport[] reports)
        {
            var snapshot = coordinator.Resolve(frame, reports);
            Assert.That(snapshot.SourceFrame, Is.EqualTo(frame));
            Assert.That(snapshot.EffectiveFrame, Is.EqualTo(frame + 1UL));
            AssertNoIncompatibleEffectiveGrants(snapshot);
            AssertGrantsHaveARealHolder(snapshot, reports);
            foreach (var record in snapshot.Records)
                Assert.That(record.ExpiresAfterFrame, Is.EqualTo(frame + 1UL), "un grant expire apres le lot suivant");
            return snapshot;
        }

        private static IEnumerable<CompiledJunctionMovement> MovementsOf(JunctionFeature feature)
        {
            var junctions = new HashSet<RoadId>(Model.Junctions.Where(j => j.Feature == feature).Select(j => j.Id));
            return Model.Movements.Where(m => junctions.Contains(m.JunctionId)).OrderBy(m => m.Id);
        }

        private static bool Conflict(RoadId a, RoadId b)
        {
            RoadId zone;
            return Index.TryGetConflict(a, b, out zone);
        }

        // ================================================================== matrice

        [Test]
        public void TwoIncompatibleRequestsOfOneBatchYieldOneGrantEffectiveAtTheNextFrame()
        {
            var coordinator = new JunctionCoordinator(Model, 10);
            var snapshot = Batch(coordinator, 10, Requesting(Id(1), Traversal(EastStraight)), Requesting(Id(2), Traversal(SouthLeft)));
            var granted = Decision(snapshot, Id(1), EastStraight);
            Assert.That(granted.Status, Is.EqualTo(JunctionGrantStatus.Granted));
            Assert.That(granted.SourceFrame, Is.EqualTo(10UL));
            Assert.That(granted.EffectiveFrame, Is.EqualTo(11UL));
            var denied = Decision(snapshot, Id(2), SouthLeft);
            Assert.That(denied.Status, Is.EqualTo(JunctionGrantStatus.Denied));
            Assert.That(denied.Reason, Is.EqualTo(JunctionReason.ConflictGranted));
            Assert.That(denied.CauseActorId, Is.EqualTo(Id(1)), "titulaire en cause");
            Assert.That(denied.ZoneId, Is.EqualTo(Zone23), "zone en cause");
            JunctionRecord record;
            Assert.That(snapshot.TryGetEffectiveGrant(Id(1), EastStraight, 11, out record), Is.True);
            Assert.That(snapshot.TryGetEffectiveGrant(Id(1), EastStraight, 10, out record), Is.False, "jamais lu a la frame qui l'emet");
            Assert.That(HasEffective(snapshot, Id(2)), Is.False);
        }

        [Test]
        public void AtEqualSeniorityTheSmallestTrafficIdIsServed()
        {
            var snapshot = Batch(new JunctionCoordinator(Model, 1), 1, Requesting(Id(8), Traversal(EastStraight)),
                Requesting(Id(1), Traversal(SouthLeft)));
            AssertStatus(snapshot, Id(1), SouthLeft, JunctionGrantStatus.Granted, JunctionReason.Granted);
            AssertStatus(snapshot, Id(8), EastStraight, JunctionGrantStatus.Denied, JunctionReason.ConflictGranted);
            Assert.That(Decision(snapshot, Id(8), EastStraight).CauseActorId, Is.EqualTo(Id(1)));
        }

        [Test]
        public void AnOlderRequestWithAHigherIdIsServedFirst()
        {
            var coordinator = new JunctionCoordinator(Model, 1);
            var occupant = Inside(Id(3), Traversal(SouthLeft));
            var s1 = Batch(coordinator, 1, occupant, Requesting(Id(8), Traversal(EastStraight)));
            AssertStatus(s1, Id(8), EastStraight, JunctionGrantStatus.Denied, JunctionReason.ConflictOccupied);
            var s2 = Batch(coordinator, 2, occupant, Requesting(Id(8), Traversal(EastStraight)), Requesting(Id(1), Traversal(SouthLeft)));
            AssertStatus(s2, Id(1), SouthLeft, JunctionGrantStatus.Denied, JunctionReason.SeniorRequestPending);
            Assert.That(Decision(s2, Id(1), SouthLeft).CauseActorId, Is.EqualTo(Id(8)), "le demandeur plus ancien est cite");
            var s3 = Batch(coordinator, 3, Cleared(Id(3), Traversal(SouthLeft)), Requesting(Id(8), Traversal(EastStraight)),
                Requesting(Id(1), Traversal(SouthLeft)));
            AssertStatus(s3, Id(8), EastStraight, JunctionGrantStatus.Granted, JunctionReason.Granted);
            Assert.That(Decision(s3, Id(8), EastStraight).RequestSinceFrame, Is.EqualTo(1UL));
            AssertStatus(s3, Id(1), SouthLeft, JunctionGrantStatus.Denied, JunctionReason.ConflictGranted);
            Assert.That(Decision(s3, Id(1), SouthLeft).RequestSinceFrame, Is.EqualTo(2UL));
        }

        /// <summary>Triplet (X, Y, Z) de la croix : X incompatible avec Y, Z compatible avec X mais incompatible avec Y.</summary>
        private static RoadId[] SeniorTriple()
        {
            var movements = MovementsOf(JunctionFeature.Crossroads).Select(m => m.Id).ToList();
            foreach (var x in movements)
                foreach (var y in movements)
                    foreach (var z in movements)
                        if (x != y && y != z && x != z && Conflict(x, y) && !Conflict(x, z) && Conflict(y, z)) return new[] { x, y, z };
            Assert.Fail("aucun triplet de la croix");
            return null;
        }

        [Test]
        public void YoungRequestsCompatibleWithTheHolderButNotWithTheOldOneWaitAndTheOldOneIsServedAtRelease()
        {
            var triple = SeniorTriple();
            RoadId x = triple[0], y = triple[1], z = triple[2];
            var coordinator = new JunctionCoordinator(Model, 1);
            var holder = Id(50);
            var old = Id(90);
            var first = Batch(coordinator, 1, Requesting(holder, Traversal(x)), Requesting(old, Traversal(y)));
            AssertStatus(first, old, y, JunctionGrantStatus.Denied, JunctionReason.ConflictGranted);
            ulong frame = 2;
            for (; frame <= 6; frame++)
            {
                var reports = new List<JunctionActorReport> { Inside(holder, Traversal(x)), Requesting(old, Traversal(y)) };
                for (int young = 1; young < (int)frame; young++) reports.Add(Requesting(Id(young), Traversal(z)));
                var snapshot = Batch(coordinator, frame, reports.ToArray());
                Assert.That(Decision(snapshot, old, y).Status, Is.EqualTo(JunctionGrantStatus.Denied), "l'ancien attend son titulaire");
                for (int young = 1; young < (int)frame; young++)
                {
                    AssertStatus(snapshot, Id(young), z, JunctionGrantStatus.Denied, JunctionReason.SeniorRequestPending);
                    Assert.That(Decision(snapshot, Id(young), z).CauseActorId, Is.EqualTo(old), "aucun faible id servi avant l'ancien");
                }
            }
            var release = new List<JunctionActorReport> { Cleared(holder, Traversal(x)), Requesting(old, Traversal(y)) };
            for (int young = 1; young < 7; young++) release.Add(Requesting(Id(young), Traversal(z)));
            var released = Batch(coordinator, frame, release.ToArray());
            AssertStatus(released, holder, x, JunctionGrantStatus.Released, JunctionReason.Cleared);
            AssertStatus(released, old, y, JunctionGrantStatus.Granted, JunctionReason.Granted);
            Assert.That(Decision(released, old, y).RequestSinceFrame, Is.EqualTo(1UL));
            for (int young = 1; young < 7; young++)
                Assert.That(Decision(released, Id(young), z).Reason, Is.EqualTo(JunctionReason.ConflictGranted));
        }

        [Test]
        public void CompatibleMovementsAreGrantedTogether()
        {
            var cross = MovementsOf(JunctionFeature.Crossroads).ToList();
            var sameApproach = cross.SelectMany(a => cross.Where(b => b.FromCorridorId == a.FromCorridorId && a.Id.CompareTo(b.Id) < 0)
                .Select(b => new[] { a.Id, b.Id })).First();
            Assert.That(Conflict(sameApproach[0], sameApproach[1]), Is.False, "meme approche, aucune zone commune");
            var snapshot = Batch(new JunctionCoordinator(Model, 1), 1, Requesting(Id(1), Traversal(sameApproach[0])),
                Requesting(Id(2), Traversal(sameApproach[1])));
            AssertStatus(snapshot, Id(1), sameApproach[0], JunctionGrantStatus.Granted, JunctionReason.Granted);
            AssertStatus(snapshot, Id(2), sameApproach[1], JunctionGrantStatus.Granted, JunctionReason.Granted);

            var same = Batch(new JunctionCoordinator(Model, 1), 1, Requesting(Id(1), Traversal(EastStraight)),
                Requesting(Id(2), Traversal(EastStraight)));
            AssertStatus(same, Id(1), EastStraight, JunctionGrantStatus.Granted, JunctionReason.Granted);
            AssertStatus(same, Id(2), EastStraight, JunctionGrantStatus.Granted, JunctionReason.Granted);
        }

        [Test]
        public void AnInsufficientExitIsDeniedBeforeAnyConflict()
        {
            var coordinator = new JunctionCoordinator(Model, 1);
            Batch(coordinator, 1, Requesting(Id(1), Traversal(EastStraight)));
            var snapshot = Batch(coordinator, 2, Inside(Id(1), Traversal(EastStraight)), Requesting(Id(2), Traversal(SouthLeft), exitFree: 2f));
            var denied = Decision(snapshot, Id(2), SouthLeft);
            Assert.That(denied.Reason, Is.EqualTo(JunctionReason.ExitBlocked), "sortie evaluee avant le conflit");
            Assert.That(denied.ExitBound, Is.EqualTo(JunctionExitBound.Occupant));
            Assert.That(denied.CauseActorId, Is.EqualTo(RoadId.None));
        }

        [Test]
        public void GrantsAlreadyIssuedTowardTheSameExitReserveItsLength()
        {
            float free = Reservation * 1.5f;
            var snapshot = Batch(new JunctionCoordinator(Model, 1), 1, Requesting(Id(1), Traversal(EastStraight), exitFree: free),
                Requesting(Id(2), Traversal(EastStraight), exitFree: free));
            AssertStatus(snapshot, Id(1), EastStraight, JunctionGrantStatus.Granted, JunctionReason.Granted);
            var second = Decision(snapshot, Id(2), EastStraight);
            Assert.That(second.Reason, Is.EqualTo(JunctionReason.ExitBlocked));
            Assert.That(second.ExitBound, Is.EqualTo(JunctionExitBound.Reservations), "la place est deja reservee par le premier grant");

            // Un titulaire deja localise sur le corridor de sortie y est compte par la recherche, pas par une reservation.
            var coordinator = new JunctionCoordinator(Model, 1);
            Batch(coordinator, 1, Requesting(Id(1), Traversal(EastStraight)));
            var onExit = new JunctionActorReport(Id(1), true, Traversal(EastStraight).ExitCorridorId, InJunction(Traversal(EastStraight).JunctionId),
                new[] { EastStraight }, new[] { Traversal(EastStraight) },
                new[] { new JunctionMovementPosition(EastStraight, JunctionMovementStatus.Occupied) }, null, false, false,
                JunctionRequestRejection.NoTraversal, Reservation);
            var after = Batch(coordinator, 2, onExit, Requesting(Id(2), Traversal(EastStraight), exitFree: free));
            AssertStatus(after, Id(2), EastStraight, JunctionGrantStatus.Granted, JunctionReason.Granted);
        }

        /// <summary>Traversee de giratoire (entree, continuation, sortie) et mouvement W en conflit avec la continuation seule.</summary>
        private static void PartialConflict(out RoadId[] chain, out RoadId other)
        {
            foreach (var entry in MovementsOf(JunctionFeature.Roundabout).Where(m => Index.EntersJunction(m.Id)))
                foreach (var continuation in Index.SameJunctionSuccessors(entry.Id).Where(c => Index.SameJunctionSuccessors(c).Count > 0))
                    foreach (var exit in Index.SameJunctionSuccessors(continuation).Where(e => Index.SameJunctionSuccessors(e).Count == 0))
                        foreach (var w in Model.GetMovementsInJunction(entry.JunctionId))
                            if (Conflict(continuation, w) && !Conflict(entry.Id, w) && !Conflict(exit, w)
                                && w != entry.Id && w != continuation && w != exit)
                            {
                                chain = new[] { entry.Id, continuation, exit };
                                other = w;
                                return;
                            }
            Assert.Fail("aucune traversee de giratoire en conflit partiel");
            chain = null;
            other = RoadId.None;
        }

        [Test]
        public void ATraversalIncompatibleOnlyThroughItsContinuationIsRefusedWhole()
        {
            RoadId[] chain;
            RoadId w;
            PartialConflict(out chain, out w);
            var coordinator = new JunctionCoordinator(Model, 1);
            Batch(coordinator, 1, Requesting(Id(1), Traversal(w)));
            var snapshot = Batch(coordinator, 2, Requesting(Id(1), Traversal(w)), Requesting(Id(2), Traversal(chain)));
            var denied = Decision(snapshot, Id(2), chain[0]);
            Assert.That(denied.Reason, Is.EqualTo(JunctionReason.ConflictGranted));
            Assert.That(denied.MovementIds, Is.EqualTo(chain), "la traversee entiere est refusee");
            Assert.That(HasEffective(snapshot, Id(2)), Is.False, "aucun grant partiel");
            Assert.That(snapshot.Records.Where(r => r.TrafficId == Id(2)).All(r => r.MovementIds.Count == chain.Length), Is.True);
        }

        [Test]
        public void AnInterruptedRequestResetsItsSeniorityAndWithdrawsAPendingGrant()
        {
            var coordinator = new JunctionCoordinator(Model, 1);
            var traversal = Traversal(EastStraight);
            var s1 = Batch(coordinator, 1, Requesting(Id(1), traversal, d: 12f));
            AssertStatus(s1, Id(1), EastStraight, JunctionGrantStatus.Granted, JunctionReason.Granted);
            var s2 = Batch(coordinator, 2, Requesting(Id(1), traversal, d: 11.9f, effective: true));
            AssertStatus(s2, Id(1), EastStraight, JunctionGrantStatus.Held, JunctionReason.Pending);
            var s3 = Batch(coordinator, 3, Requesting(Id(1), traversal, d: 11.8f, head: false, effective: true));
            AssertStatus(s3, Id(1), EastStraight, JunctionGrantStatus.Revoked, JunctionReason.RequestWithdrawn);
            var s4 = Batch(coordinator, 4, Requesting(Id(1), traversal, d: 11.7f));
            AssertStatus(s4, Id(1), EastStraight, JunctionGrantStatus.Granted, JunctionReason.Granted);
            Assert.That(Decision(s4, Id(1), EastStraight).RequestSinceFrame, Is.EqualTo(4UL), "anciennete remise a la reprise");
        }

        [Test]
        public void AnEngagedGrantIsHeldDespiteANewConflictOrExit()
        {
            var coordinator = new JunctionCoordinator(Model, 1);
            var traversal = Traversal(EastStraight);
            Batch(coordinator, 1, Requesting(Id(1), traversal, d: 12f));
            // d < D_stop(4 m/s) = 4,8 m : engage ; un occupant incompatible surgit et la sortie se ferme.
            var engaged = Requesting(Id(1), traversal, d: 3f, effective: true, exitFree: 1f);
            Assert.That(engaged.Approaches[0].Engaged, Is.True);
            var s2 = Batch(coordinator, 2, engaged, Inside(Id(7), Traversal(SouthLeft)));
            AssertStatus(s2, Id(1), EastStraight, JunctionGrantStatus.Held, JunctionReason.Committed);
            AssertStatus(s2, Id(7), SouthLeft, JunctionGrantStatus.Denied, JunctionReason.IncompatibleOccupancy);
            Assert.That(s2.Counters.IncompatibleOccupancy, Is.EqualTo(1));
            var s3 = Batch(coordinator, 3, Inside(Id(1), traversal));
            AssertStatus(s3, Id(1), EastStraight, JunctionGrantStatus.Held, JunctionReason.Committed);
        }

        [Test]
        public void AHolderInFallbackKeepsItsGrantUntilItsFootprintLeavesTheBoundary()
        {
            var coordinator = new JunctionCoordinator(Model, 1);
            var traversal = Traversal(EastStraight);
            Batch(coordinator, 1, Requesting(Id(1), traversal));
            var s2 = Batch(coordinator, 2, Inside(Id(1), traversal));
            AssertStatus(s2, Id(1), EastStraight, JunctionGrantStatus.Held, JunctionReason.Committed);
            var s3 = Batch(coordinator, 3, Inside(Id(1), traversal, localized: false));
            AssertStatus(s3, Id(1), EastStraight, JunctionGrantStatus.Held, JunctionReason.Committed);
            var s4 = Batch(coordinator, 4, Requesting(Id(2), Traversal(SouthLeft)), Inside(Id(1), traversal, localized: false, corners: At(FarAway)));
            AssertStatus(s4, Id(1), EastStraight, JunctionGrantStatus.Released, JunctionReason.ClearedUnlocalized);
            AssertStatus(s4, Id(2), SouthLeft, JunctionGrantStatus.Granted, JunctionReason.Granted);
        }

        [Test]
        public void ARefusedFrameRepublishesOnlyEngagedGrantsAndTheNextValidBatchProtectsOccupantsFirst()
        {
            var m1 = Traversal(EastStraight);
            var m2 = Traversal(SouthLeft);
            var far = Traversal(Movement("489d4a3b"));
            var coordinator = new JunctionCoordinator(Model, 1);
            Batch(coordinator, 1, Requesting(Id(1), m1));
            var s2 = Batch(coordinator, 2, Inside(Id(1), m1), Requesting(Id(2), m2), Requesting(Id(3), far, d: 12f));
            AssertStatus(s2, Id(2), SouthLeft, JunctionGrantStatus.Denied, JunctionReason.ConflictOccupied);
            AssertStatus(s2, Id(3), far.FirstMovementId, JunctionGrantStatus.Granted, JunctionReason.Granted);

            var s3 = coordinator.ResolveUnavailableFrame(3);
            Assert.That(s3.EffectiveFrame, Is.EqualTo(4UL));
            Assert.That(s3.Counters.FrameValid, Is.False);
            Assert.That(coordinator.FrameFailures, Is.EqualTo(1));
            AssertStatus(s3, Id(1), EastStraight, JunctionGrantStatus.Held, JunctionReason.CommittedCarried);
            AssertStatus(s3, Id(3), far.FirstMovementId, JunctionGrantStatus.Revoked, JunctionReason.FrameUnavailable);
            Assert.That(s3.Records.Any(r => r.TrafficId == Id(2)), Is.False, "aucune demande examinee sur une frame invalide");
            Assert.That(s3.Records.Count(r => r.Status == JunctionGrantStatus.Granted), Is.Zero, "aucun grant emis");
            AssertNoIncompatibleEffectiveGrants(s3);

            var s4 = Batch(coordinator, 4, Inside(Id(1), m1), Requesting(Id(2), m2));
            AssertStatus(s4, Id(1), EastStraight, JunctionGrantStatus.Held, JunctionReason.Committed);
            AssertStatus(s4, Id(2), SouthLeft, JunctionGrantStatus.Denied, JunctionReason.ConflictOccupied);

            // Meme preuve sans memoire : un coordinateur neuf protege l'occupant par son occupation seule.
            var fresh = new JunctionCoordinator(Model, 4);
            var f4 = Batch(fresh, 4, Inside(Id(1), m1), Requesting(Id(2), m2));
            AssertStatus(f4, Id(1), EastStraight, JunctionGrantStatus.Held, JunctionReason.Restored);
            AssertStatus(f4, Id(2), SouthLeft, JunctionGrantStatus.Denied, JunctionReason.ConflictOccupied);
            Assert.That(f4.Counters.EnteredWithoutGrant, Is.EqualTo(1));
        }

        [Test]
        public void ADespawnedOrVanishedHolderIsRevokedAndAnExitedOneIsReleased()
        {
            var coordinator = new JunctionCoordinator(Model, 1);
            var traversal = Traversal(EastStraight);
            var other = Traversal(Movement("489d4a3b"));
            Batch(coordinator, 1, Requesting(Id(1), traversal), Requesting(Id(2), other));
            Batch(coordinator, 2, Inside(Id(1), traversal), Inside(Id(2), other));
            var s3 = Batch(coordinator, 3, Cleared(Id(2), other));
            AssertStatus(s3, Id(1), EastStraight, JunctionGrantStatus.Revoked, JunctionReason.ActorGone);
            AssertStatus(s3, Id(2), other.FirstMovementId, JunctionGrantStatus.Released, JunctionReason.Cleared);
            Assert.That(s3.Records.Count(r => r.IsEffectiveGrant), Is.Zero);
        }

        [Test]
        public void AVehicleThatEnteredWithoutGrantIsProtectedAndCountedAndIncompatibleOccupantsBlockButAreNeverServed()
        {
            var coordinator = new JunctionCoordinator(Model, 1);
            var elsewhere = Traversal(Movement("489d4a3b"));
            Batch(coordinator, 1, Requesting(Id(9), elsewhere));
            var s2 = Batch(coordinator, 2, Requesting(Id(9), elsewhere, effective: true), Inside(Id(1), Traversal(EastStraight)),
                Requesting(Id(2), Traversal(SouthLeft)));
            Assert.That(s2.Counters.EnteredWithoutGrant, Is.EqualTo(1));
            AssertStatus(s2, Id(1), EastStraight, JunctionGrantStatus.Held, JunctionReason.Restored);
            AssertStatus(s2, Id(2), SouthLeft, JunctionGrantStatus.Denied, JunctionReason.ConflictOccupied);

            var fresh = new JunctionCoordinator(Model, 1);
            var f1 = Batch(fresh, 1, Inside(Id(1), Traversal(EastStraight)), Inside(Id(2), Traversal(SouthLeft)),
                Requesting(Id(3), Traversal(EastStraight)));
            Assert.That(f1.Counters.EnteredWithoutGrant, Is.EqualTo(2));
            Assert.That(f1.Counters.IncompatibleOccupancy, Is.EqualTo(2));
            AssertStatus(f1, Id(1), EastStraight, JunctionGrantStatus.Denied, JunctionReason.IncompatibleOccupancy);
            AssertStatus(f1, Id(2), SouthLeft, JunctionGrantStatus.Denied, JunctionReason.IncompatibleOccupancy);
            var blocked = Decision(f1, Id(3), EastStraight);
            Assert.That(blocked.Reason, Is.EqualTo(JunctionReason.ConflictOccupied), "un occupant incompatible bloque");
            Assert.That(blocked.CauseActorId, Is.EqualTo(Id(2)));
            var f2 = Batch(fresh, 2, Inside(Id(1), Traversal(EastStraight)));
            AssertStatus(f2, Id(1), EastStraight, JunctionGrantStatus.Held, JunctionReason.Restored);
        }

        [Test]
        public void APendingGrantDoesNotSurviveAnIncompatibleVehicleThatEnteredWithoutGrant()
        {
            var coordinator = new JunctionCoordinator(Model, 1);
            var s1 = Batch(coordinator, 1, Requesting(Id(2), Traversal(SouthLeft)));
            AssertStatus(s1, Id(2), SouthLeft, JunctionGrantStatus.Granted, JunctionReason.Granted);

            // Le titulaire peut encore s'arreter (d >= D_stop) ; un vehicule entre sans grant sur un mouvement incompatible.
            var s2 = Batch(coordinator, 2, Requesting(Id(2), Traversal(SouthLeft), effective: true), Inside(Id(1), Traversal(EastStraight)));
            Assert.That(s2.Counters.EnteredWithoutGrant, Is.EqualTo(1));
            AssertStatus(s2, Id(1), EastStraight, JunctionGrantStatus.Denied, JunctionReason.IncompatibleOccupancy);
            Assert.That(HasEffective(s2, Id(2)), Is.False, "aucun grant non engage vers un mouvement occupe incompatible\n" + s2.ToText());
            var revoked = s2.Records.Single(r => r.TrafficId == Id(2) && r.Status == JunctionGrantStatus.Revoked);
            Assert.That(revoked.Reason, Is.EqualTo(JunctionReason.ConflictOccupied), revoked.ToText());
            Assert.That(revoked.CauseActorId, Is.EqualTo(Id(1)), "la revocation cite l'occupant");
            Assert.That(revoked.ZoneId, Is.Not.EqualTo(RoadId.None), "la revocation cite la zone");

            // Engage (d < D_stop), le meme grant est tenu malgre l'occupant.
            var engaged = new JunctionCoordinator(Model, 1);
            Batch(engaged, 1, Requesting(Id(2), Traversal(SouthLeft)));
            var e2 = Batch(engaged, 2, Requesting(Id(2), Traversal(SouthLeft), d: 1f, effective: true), Inside(Id(1), Traversal(EastStraight)));
            Assert.That(e2.Records.Any(r => r.TrafficId == Id(2) && r.Status == JunctionGrantStatus.Held && r.Reason == JunctionReason.Committed),
                Is.True, e2.ToText());
        }

        [Test]
        public void AReportInFallbackIsAnInvalidRequestAndWithdrawsItsPendingGrant()
        {
            var coordinator = new JunctionCoordinator(Model, 1);
            Batch(coordinator, 1, Requesting(Id(2), Traversal(SouthLeft)));
            var report = Requesting(Id(2), Traversal(SouthLeft), effective: true);
            Assert.That(report.WithFallback(false), Is.SameAs(report), "hors repli, rapport inchange");
            var fallback = report.WithFallback(true);
            Assert.That(fallback.RequestValid, Is.False);
            Assert.That(fallback.Rejection, Is.EqualTo(JunctionRequestRejection.Fallback));
            var s2 = Batch(coordinator, 2, fallback);
            AssertStatus(s2, Id(2), SouthLeft, JunctionGrantStatus.Revoked, JunctionReason.RequestWithdrawn);
        }

        [Test]
        public void AnUnknownMovementIsAnInvalidRequest()
        {
            var unknown = new RoadId(0xdeadUL, 1UL);
            var traversal = new JunctionTraversal(Index.JunctionOf(EastStraight), new[] { unknown }, RoadId.None);
            var approach = new JunctionApproach(traversal, 5f, JunctionDistances.For(Driver, 4f, Dt, 0.5f), true, RoadId.None, false, false,
                new JunctionExitAssessment(50f, JunctionExitBound.Occupant, RoadId.None, Reservation));
            var report = new JunctionActorReport(Id(1), true, RoadId.None, At(FarAway), null, null, null, new[] { approach }, true, true,
                JunctionRequestRejection.None, Reservation);
            var snapshot = Batch(new JunctionCoordinator(Model, 1), 1, report);
            AssertStatus(snapshot, Id(1), unknown, JunctionGrantStatus.Denied, JunctionReason.InvalidRequest);
        }

        [Test]
        public void AShiftedSnapshotReadsAsNoGrant()
        {
            var snapshot = Batch(new JunctionCoordinator(Model, 5), 5, Requesting(Id(1), Traversal(EastStraight)));
            JunctionRecord record;
            Assert.That(snapshot.TryGetEffectiveGrant(Id(1), EastStraight, 6, out record), Is.True);
            Assert.That(snapshot.TryGetEffectiveGrant(Id(1), EastStraight, 5, out record), Is.False);
            Assert.That(snapshot.TryGetEffectiveGrant(Id(1), EastStraight, 7, out record), Is.False, "un grant non republie expire");
        }

        // ================================================================== faits du modele

        [Test]
        public void EveryPairOfCrossAndTJunctionTraversalsIsNeverHeldTogetherWhenIncompatibleNorRefusedWhenCompatible()
        {
            int incompatible = 0, compatible = 0;
            foreach (var feature in new[] { JunctionFeature.Crossroads, JunctionFeature.TJunction })
                foreach (var junction in Model.Junctions.Where(j => j.Feature == feature))
                {
                    var movements = Model.GetMovementsInJunction(junction.Id);
                    foreach (var a in movements)
                        foreach (var b in movements)
                        {
                            var coordinator = new JunctionCoordinator(Model, 1);
                            for (ulong frame = 1; frame <= 3; frame++)
                            {
                                var snapshot = Batch(coordinator, frame, Requesting(Id(1), Traversal(a), d: 12f - frame),
                                    Requesting(Id(2), Traversal(b), d: 12f - frame));
                                bool both = HasEffective(snapshot, Id(1)) && HasEffective(snapshot, Id(2));
                                if (Conflict(a, b))
                                {
                                    Assert.That(both, Is.False);
                                    Assert.That(Decision(snapshot, Id(2), b).Reason, Is.EqualTo(JunctionReason.ConflictGranted));
                                }
                                else
                                {
                                    Assert.That(both, Is.True, a + " / " + b + "\n" + snapshot.ToText());
                                    Assert.That(snapshot.Records.Any(r => r.Status == JunctionGrantStatus.Denied), Is.False);
                                }
                            }
                            if (Conflict(a, b)) incompatible++; else compatible++;
                        }
                }
            Assert.That(incompatible, Is.GreaterThan(0));
            Assert.That(compatible, Is.GreaterThan(0));
            TestContext.WriteLine("paires de traversees croix + T : " + incompatible + " incompatibles, " + compatible + " compatibles");
        }

        [Test]
        public void ChainsHaveLengthOneAtCrossAndTJunctionsAndAtLeastTwoFromEveryRoundaboutEntry()
        {
            foreach (var feature in new[] { JunctionFeature.Crossroads, JunctionFeature.TJunction })
                foreach (var movement in MovementsOf(feature))
                {
                    Assert.That(Index.SameJunctionSuccessors(movement.Id), Is.Empty, "aucun enchainement en croix ou en T");
                    Assert.That(Index.EntersJunction(movement.Id), Is.True);
                }
            int entries = 0, linked = 0, exits = 0;
            foreach (var movement in MovementsOf(JunctionFeature.Roundabout))
            {
                int successors = Index.SameJunctionSuccessors(movement.Id).Count;
                if (successors > 0) linked++; else exits++;
                if (!Index.EntersJunction(movement.Id)) continue;
                entries++;
                Assert.That(successors, Is.GreaterThan(0), "une entree de giratoire enchaine au moins un mouvement");
            }
            Assert.That(entries, Is.EqualTo(12));
            Assert.That(linked, Is.EqualTo(24), "entrees et continuations suivies d'un mouvement du meme giratoire");
            Assert.That(exits, Is.EqualTo(12));
            Assert.That(Model.Movements.Count, Is.EqualTo(72));
            Assert.That(Model.ConflictZones.Count, Is.EqualTo(136));
        }

        [Test]
        public void TheIndexIsBuiltOncePerModelAndABatchOnlyChecksTheJunctionConcerned()
        {
            Assert.That(JunctionConflictIndex.For(Model), Is.SameAs(JunctionConflictIndex.For(Model)));
            long before = TrafficV2WorkCounters.Work.JunctionIndexBuilds;
            var copy = RoadModelCompiler.Compile(RoadModelDocument.Load(File.ReadAllText(TrafficV2Settings.ModelPath)));
            JunctionConflictIndex.For(copy);
            JunctionConflictIndex.For(copy);
            Assert.That(TrafficV2WorkCounters.Work.JunctionIndexBuilds - before, Is.EqualTo(1), "un index par modele");

            // Un demandeur seul, puis avec un titulaire d'un autre carrefour : aucune paire testee malgre 136 zones.
            var alone = Batch(new JunctionCoordinator(Model, 1), 1, Requesting(Id(1), Traversal(EastStraight)));
            Assert.That(alone.Counters.PairChecks, Is.Zero);
            var coordinator = new JunctionCoordinator(Model, 1);
            Batch(coordinator, 1, Requesting(Id(2), Traversal(Movement("489d4a3b"))));
            var elsewhere = Batch(coordinator, 2, Inside(Id(2), Traversal(Movement("489d4a3b"))), Requesting(Id(1), Traversal(EastStraight)));
            Assert.That(elsewhere.Counters.PairChecks, Is.Zero, "le travail ne depend que du carrefour concerne");
            var same = new JunctionCoordinator(Model, 1);
            Batch(same, 1, Requesting(Id(2), Traversal(SouthLeft)));
            var concerned = Batch(same, 2, Inside(Id(2), Traversal(SouthLeft)), Requesting(Id(1), Traversal(EastStraight)));
            Assert.That(concerned.Counters.PairChecks, Is.LessThanOrEqualTo(2), "un mouvement demande contre un occupant et un grant");
            Assert.That(concerned.Counters.PairChecks, Is.GreaterThan(0));
        }

        // ================================================================== traversees reelles (constructeur)

        private static RoutePlan RouteVia(RoadId movement)
        {
            var from = Index.FromCorridorOf(movement);
            var start = new RoadLocation { ModelId = Model.ModelId, ModelVersion = Model.Version, Localized = true,
                ElementKind = RoadElementKind.LaneCorridor, ElementId = from, SMeters = 0f };
            var result = RoutePlanner.Plan(new RouteRequest(Model, start, RoadId.None, new RouteSeed(0), Id(1), "route",
                new DecisionCounter(0), null, false, null, movement));
            Assert.That(result.Plan, Is.Not.Null, result.Reason.ToString());
            return result.Plan;
        }

        /// <summary>Distance de route de l'entree du mouvement vise, depuis le debut de la route.</summary>
        private static float EntryDistance(RoutePlan route, RoadId movement)
        {
            float entry = 0f;
            for (int k = 0; route.Occurrences[k].Id != movement; k++)
                entry += route.Occurrences[k].EndSMeters - route.Occurrences[k].StartSMeters;
            return entry;
        }

        /// <summary>Acteur a la pose nominale de la route, son pare-chocs avant a d de l'entree du mouvement vise.</summary>
        private static TrafficActorInput ActorBefore(RoadId id, RoutePlan route, RoadId movement, float d, VehicleFootprint footprint,
            float speed = 4f)
        {
            var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
            float distance = EntryDistance(route, movement) - d - footprint.FrontMeters;
            Assert.That(distance, Is.GreaterThanOrEqualTo(0f), "pose sur la route");
            int piece = track.PieceAt(distance);
            var nominal = track.Nominal(piece, distance);
            return new TrafficActorInput(id, new VehicleFootprintPose { Position = nominal.Position, Forward = nominal.Forward,
                Up = nominal.Up, Footprint = footprint }, speed, track.Pieces[piece].Id, route.Occurrences.Select(o => o.Id).ToArray(),
                track.KinematicAnchors(piece));
        }

        [Test]
        public void ARoundaboutEntryOnTheShortRingIsOneTraversalWhoseExitIsMeasuredAfterItsLastMovement()
        {
            var entry = Movement("43605e56");
            var route = RouteVia(entry);
            float d = Math.Min(12f, EntryDistance(route, entry) - Car.FrontMeters - 1f);
            var frame = new TrafficFrame(1, Model, new[] { ActorBefore(Id(1), route, entry, d, Car) });
            var report = JunctionRequestBuilder.Build(frame, Index, Id(1), route, Driver, Dt,
                TrafficV2Settings.JunctionStopControlMarginMeters, JunctionSnapshot.Initial(1), 1);
            Assert.That(report.HasRequest, Is.True, report.ToText());
            var traversal = report.Request.Traversal;
            Assert.That(traversal.FirstMovementId, Is.EqualTo(entry));
            Assert.That(traversal.MovementIds.Count, Is.GreaterThanOrEqualTo(2), "entree puis mouvement suivant de l'anneau");
            var ring = Index.ToCorridorOf(entry);
            EffectiveLaneCorridor ringCorridor;
            Assert.That(Model.TryGetCorridor(ring, out ringCorridor), Is.True);
            Assert.That(ringCorridor.LengthMeters, Is.LessThan(1f), "corridor d'anneau de 0,87 m");
            Assert.That(traversal.ExitCorridorId, Is.Not.EqualTo(ring), "l'anneau interne n'est jamais la sortie");
            var ids = route.Occurrences.Select(o => o.Id).ToList();
            int last = ids.IndexOf(traversal.LastMovementId);
            Assert.That(ids[last + 1], Is.EqualTo(traversal.ExitCorridorId), "le corridor de sortie suit le dernier mouvement");
            Assert.That(last + 2 >= ids.Count || Index.JunctionOf(ids[last + 2]) != traversal.JunctionId, Is.True,
                "la traversee s'arrete au premier corridor que la route quitte sans mouvement du meme carrefour");
            for (int k = ids.IndexOf(entry); k < last; k++)
                if (route.Occurrences[k].Kind == RoadElementKind.JunctionMovement)
                    Assert.That(traversal.MovementIds.Contains(ids[k]), Is.True, "chaque mouvement de la chaine est dans la traversee");
            EffectiveLaneCorridor exitCorridor;
            Assert.That(Model.TryGetCorridor(traversal.ExitCorridorId, out exitCorridor), Is.True);
            Assert.That(exitCorridor.LengthMeters, Is.GreaterThanOrEqualTo(9.5f));
            Assert.That(report.Request.Exit.FreeLengthMeters, Is.GreaterThanOrEqualTo(Reservation), report.ToText());
            Assert.That(report.Request.DistanceMeters, Is.EqualTo(d).Within(0.2f), "d mesure au pare-chocs avant");
            TestContext.WriteLine(report.ToText());
        }

        [Test]
        public void AnExitShorterThanTheVehicleBeforeTheNextJunctionStopsTheSearchAndIsDenied()
        {
            // Predicat « corridor de sortie < L + s0 avant le mouvement suivant » : la geometrie signee de MVP_Run ne peut pas etre
            // raccourcie sans casser sa validation ; L est donc allonge (empreinte declaree de 16 m) sur le corridor reel.
            var straight = MovementsOf(JunctionFeature.TJunction).First(m => m.Samples.All(s => Math.Abs(s.CurvaturePerMeter) < 1e-4f)
                && Index.OutgoingMovements(m.ToCorridorId).Count > 0);
            var route = RouteVia(straight.Id);
            var longCar = new VehicleFootprint { FrontMeters = 8f, RearMeters = 8f, LeftMeters = 1.03f, RightMeters = 1.03f };
            float d = Math.Min(10f, EntryDistance(route, straight.Id) - longCar.FrontMeters - 1f);
            var frame = new TrafficFrame(1, Model, new[] { ActorBefore(Id(1), route, straight.Id, d, longCar) });
            var report = JunctionRequestBuilder.Build(frame, Index, Id(1), route, Driver, Dt,
                TrafficV2Settings.JunctionStopControlMarginMeters, JunctionSnapshot.Initial(1), 1);
            Assert.That(report.RequestValid, Is.True, report.ToText());
            var exit = report.Request.Exit;
            EffectiveLaneCorridor corridor;
            Assert.That(Model.TryGetCorridor(straight.ToCorridorId, out corridor), Is.True);
            Assert.That(exit.Bound, Is.EqualTo(JunctionExitBound.ExitSearchBound), report.ToText());
            Assert.That(exit.FreeLengthMeters, Is.EqualTo(corridor.LengthMeters).Within(1e-3f), "recherche arretee a l'entree du carrefour suivant");
            Assert.That(exit.RequiredMeters, Is.EqualTo(16f + Driver.MinimumGap).Within(1e-4f));
            var snapshot = Batch(new JunctionCoordinator(Model, 1), 1, report);
            var denied = Decision(snapshot, Id(1), straight.Id);
            Assert.That(denied.Reason, Is.EqualTo(JunctionReason.ExitBlocked));
            Assert.That(denied.ExitBound, Is.EqualTo(JunctionExitBound.ExitSearchBound));
        }

        // ================================================================== determinisme, anciennete, equite

        private static List<JunctionActorReport[]> MixedScenario()
        {
            var triple = SeniorTriple();
            var far = Traversal(Movement("489d4a3b"));
            return new List<JunctionActorReport[]>
            {
                new[] { Requesting(Id(5), Traversal(triple[0])), Requesting(Id(9), Traversal(triple[1])), Requesting(Id(2), Traversal(triple[2])),
                    Requesting(Id(7), far), Inside(Id(4), Traversal(EastStraight)), Requesting(Id(3), Traversal(SouthLeft)) },
                new[] { Inside(Id(5), Traversal(triple[0])), Requesting(Id(9), Traversal(triple[1])), Requesting(Id(2), Traversal(triple[2])),
                    Requesting(Id(7), far, d: 9f, effective: true), Inside(Id(4), Traversal(EastStraight)), Requesting(Id(3), Traversal(SouthLeft)),
                    Requesting(Id(1), Traversal(triple[2])) },
                new[] { Cleared(Id(5), Traversal(triple[0])), Requesting(Id(9), Traversal(triple[1])), Requesting(Id(2), Traversal(triple[2])),
                    Inside(Id(7), far), Cleared(Id(4), Traversal(EastStraight)), Requesting(Id(3), Traversal(SouthLeft)),
                    Requesting(Id(1), Traversal(triple[2])), Idle(Id(8)) }
            };
        }

        private static List<string> Run(List<JunctionActorReport[]> batches, Func<JunctionActorReport[], JunctionActorReport[]> order)
        {
            var coordinator = new JunctionCoordinator(Model, 1);
            var texts = new List<string>();
            for (int i = 0; i < batches.Count; i++)
                texts.Add(Batch(coordinator, (ulong)(i + 1), order(batches[i])).ToText());
            return texts;
        }

        [Test]
        public void TheSnapshotIsIdenticalUnderShuffledRequestAndActorOrders()
        {
            var batches = MixedScenario();
            var reference = Run(batches, r => r);
            var random = new System.Random(534);
            var orders = new List<Func<JunctionActorReport[], JunctionActorReport[]>>
            {
                r => r.Reverse().ToArray(),
                r => r.Skip(1).Concat(r.Take(1)).ToArray(),
                r => r.OrderBy(x => x.TrafficId.ToString()).ToArray()
            };
            for (int seed = 0; seed < 5; seed++) orders.Add(r => r.OrderBy(x => random.Next()).ToArray());
            foreach (var order in orders)
                Assert.That(Run(batches, order), Is.EqualTo(reference));
            Assert.That(reference[1], Does.Contain("Denied(SeniorRequestPending)").Or.Contain("Denied(ConflictGranted)"));
            TestContext.WriteLine(string.Join("\n\n", reference));
        }

        [Test]
        public void SeniorityIsKeptOnContinuousRequestsAndAcrossAnInvalidFrameAndResetByAValidInterruptionOrATraversalChange()
        {
            var coordinator = new JunctionCoordinator(Model, 1);
            var occupant = Inside(Id(2), Traversal(SouthLeft));
            Func<JunctionSnapshot, ulong> since = s => Decision(s, Id(1), EastStraight).RequestSinceFrame;
            Assert.That(since(Batch(coordinator, 1, occupant, Requesting(Id(1), Traversal(EastStraight)))), Is.EqualTo(1UL));
            Assert.That(since(Batch(coordinator, 2, occupant, Requesting(Id(1), Traversal(EastStraight)))), Is.EqualTo(1UL));
            coordinator.ResolveUnavailableFrame(3);
            Assert.That(since(Batch(coordinator, 4, occupant, Requesting(Id(1), Traversal(EastStraight)))), Is.EqualTo(1UL),
                "une frame invalide n'interrompt pas l'anciennete");
            var s5 = Batch(coordinator, 5, occupant, Requesting(Id(1), Traversal(EastStraight), head: false));
            Assert.That(s5.Records.Any(r => r.TrafficId == Id(1)), Is.False);
            Assert.That(since(Batch(coordinator, 6, occupant, Requesting(Id(1), Traversal(EastStraight)))), Is.EqualTo(6UL),
                "remise a zero par une frame valide sans demande");
            var other = MovementsOf(JunctionFeature.TJunction).First(m => m.JunctionId == Index.JunctionOf(SouthLeft)
                && m.Id != EastStraight && Conflict(m.Id, SouthLeft));
            var s7 = Batch(coordinator, 7, occupant, Requesting(Id(1), Traversal(other.Id)));
            Assert.That(Decision(s7, Id(1), other.Id).RequestSinceFrame, Is.EqualTo(7UL), "remise a zero par un changement de traversee");
        }

        [Test]
        public void AContinuousHighIdRequesterIsServedInBoundedBatchesAndNoLowIdIsServedBeforeAnOlderIncompatibleOne()
        {
            var triple = SeniorTriple();
            RoadId x = triple[0], y = triple[1], z = triple[2];
            var coordinator = new JunctionCoordinator(Model, 1);
            var holder = Id(40);
            var senior = Id(99);
            Batch(coordinator, 1, Requesting(holder, Traversal(x)), Requesting(senior, Traversal(y)));
            int servedAt = -1, releasedAt = 30;
            var lowIdsServed = new List<string>();
            for (int frame = 2; frame <= 40 && servedAt < 0; frame++)
            {
                var reports = new List<JunctionActorReport> { frame < releasedAt ? Inside(holder, Traversal(x)) : Cleared(holder, Traversal(x)),
                    Requesting(senior, Traversal(y)) };
                // Flux continu de faibles id, chacun demande z (compatible avec le titulaire, incompatible avec l'ancien).
                for (int low = Math.Max(1, frame - 5); low < frame; low++) reports.Add(Requesting(Id(low), Traversal(z)));
                var snapshot = Batch(coordinator, (ulong)frame, reports.ToArray());
                if (HasEffective(snapshot, senior)) servedAt = frame;
                foreach (var record in snapshot.Records.Where(r => r.Status == JunctionGrantStatus.Granted && r.TrafficId != senior))
                    if (servedAt < 0) lowIdsServed.Add(frame + ":" + record.ToText());
            }
            Assert.That(lowIdsServed, Is.Empty, "aucun faible id servi avant le plus ancien incompatible");
            Assert.That(servedAt, Is.EqualTo(releasedAt), "servi au lot de la liberation de son titulaire");
        }
    }
}
