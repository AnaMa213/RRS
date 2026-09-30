using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.31 (correct-course du 2026-09-29, contrat Road World Model §4) : objectif de mouvement
    /// intermediaire. Semantique par phases, completude contre un oracle d'atteignabilite independant,
    /// cout du mouvement compte une fois, occurrences contigues, cycles, codes par phase, replan, et
    /// frontiere d'activation (seul le chemin de mesure pose l'objectif).
    /// </summary>
    [Category("Core")]
    [Category("Story531")]
    public sealed class Story531ViaObjectiveRoutingTests
    {
        private static RoadId Id(int n) { return new RoadId(0x531AUL, (ulong)n); }

        // ------------------------------------------------------------------ modele synthetique
        // Tous les elements sont le meme cercle (rayon 10, depart (10,0,0), tangente +z) : chaque raccord a un
        // ecart et un saut de tangente nuls, seule la topologie compte.

        private const float Radius = 10f;

        private static RoadCurveSample[] Circle()
        {
            var samples = new RoadCurveSample[17];
            for (int i = 0; i < samples.Length; i++)
            {
                float theta = 2f * Mathf.PI * i / 16f;
                samples[i] = new RoadCurveSample {
                    SMeters = Radius * theta,
                    Position = new Vector3(Radius * Mathf.Cos(theta), 0f, Radius * Mathf.Sin(theta)),
                    Tangent = new Vector3(-Mathf.Sin(theta), 0f, Mathf.Cos(theta)),
                    Up = Vector3.up, CurvaturePerMeter = -1f / Radius,
                    HalfWidthLeftMeters = 2f, HalfWidthRightMeters = 2f };
            }
            return samples;
        }

        private sealed class Graph
        {
            public readonly List<int> Corridors = new List<int>();
            public readonly List<(int id, int from, int to)> Connections = new List<(int, int, int)>();
            public readonly List<(int id, int from, int to, float weight)> Movements = new List<(int, int, int, float)>();
            public readonly List<(int id, int corridor)> Exits = new List<(int, int)>();
        }

        private static CompiledRoadModel Compile(Graph graph)
        {
            var circle = Circle();
            float length = circle[circle.Length - 1].SMeters;
            var source = new RoadModelSource();
            source.ModelId = Id(1);
            source.ValidationProfile = new RoadModelValidationProfile {
                MaxVehicleHalfWidthMeters = 1f, MaxVehicleLengthMeters = 4f,
                LateralClearanceMarginMeters = .25f, SeamGapToleranceMeters = .05f,
                SeamTangentToleranceDegrees = 5f, LengthToleranceMeters = .05f,
                EnvelopeOverlapToleranceMeters = .05f, GroundingMaxOffAxisDegrees = 45f };
            source.LocalizationProfile = new RoadLocalizationProfile {
                ScoreBandMeters = .15f, HysteresisMeters = .1f,
                AcceptanceDistanceMeters = 2.5f, WrongWayHeadingDegrees = 90f };
            source.Sections = graph.Corridors.Select(c => new RoadSection {
                Id = Id(1000 + c), RoadClass = RoadClass.Local, Surface = RoadSurface.Asphalt,
                DefaultSpeedLimitMetersPerSecond = 10f, DefaultAllowedVehicleClasses = VehicleClassMask.Car }).ToArray();
            source.Corridors = graph.Corridors.Select(c => new LaneCorridor {
                Id = Id(c), SectionId = Id(1000 + c), LateralOrder = 0, IsCrossSectionDatum = true,
                Samples = Circle(), LengthMeters = length }).ToArray();
            source.Connections = graph.Connections.Select(k => new LaneConnection {
                Id = Id(k.id), FromCorridorId = Id(k.from), ToCorridorId = Id(k.to),
                Kind = LaneConnectionKind.Continuation }).ToArray();
            source.Junctions = graph.Movements.Count == 0 ? new Junction[0] : new[] { new Junction {
                Id = Id(900), Feature = JunctionFeature.Crossroads,
                Boundary = new RoadBoundsBox { Center = Vector3.zero, Extents = new Vector3(15f, 5f, 15f) } } };
            source.Movements = graph.Movements.Select(m => new JunctionMovement {
                Id = Id(m.id), JunctionId = Id(900), FromCorridorId = Id(m.from), ToCorridorId = Id(m.to),
                Samples = Circle(), LengthMeters = length, RoutePreferenceWeight = m.weight }).ToArray();
            source.Controls = graph.Movements.Count == 0 ? new JunctionControl[0] : new[] { new JunctionControl {
                Id = Id(901), JunctionId = Id(900), Kind = JunctionControlKind.Uncontrolled,
                ControlledMovementIds = graph.Movements.Select(m => Id(m.id)).ToArray() } };
            source.Portals = graph.Exits.Select(p => new Portal {
                Id = Id(p.id), CorridorId = Id(p.corridor), Role = PortalRole.Exit, SMeters = length - 0.5f,
                EnvelopeLengthMeters = 1f, EnvelopeHalfWidthMeters = 1f }).ToArray();
            return RoadModelCompiler.Compile(source);
        }

        private static RoadLocation At(CompiledRoadModel model, RoadId element, float s = 0f,
            RoadElementKind kind = RoadElementKind.LaneCorridor)
        {
            return new RoadLocation { ModelId = model.ModelId, ModelVersion = model.Version, Localized = true,
                ElementKind = kind, ElementId = element, SMeters = s };
        }

        private static RouteResult Plan(CompiledRoadModel model, RoadLocation location, RoadId via,
            RoadId exit = default(RoadId), RoutePlan existing = null, bool replan = false, ulong seed = 7)
        {
            return RoutePlanner.Plan(new RouteRequest(model, location, exit, new RouteSeed(seed), Id(800), "route",
                new DecisionCounter(0), existing, replan, null, via));
        }

        private static void AssertWellFormed(CompiledRoadModel model, RoutePlan plan, RoadId via, bool checkReuse = true)
        {
            Assert.That(plan.ViaMovementId, Is.EqualTo(via));
            int viaCount = 0;
            double distance = 0d;
            for (int i = 0; i < plan.Occurrences.Count; i++)
            {
                var o = plan.Occurrences[i];
                distance += o.EndSMeters - o.StartSMeters;
                if (o.Kind == RoadElementKind.JunctionMovement && o.Id == via)
                {
                    viaCount++;
                    CompiledJunctionMovement movement;
                    Assert.That(model.TryGetMovement(via, out movement), Is.True);
                    Assert.That(i, Is.EqualTo(plan.ViaOccurrenceIndex), "frontiere des phases publiee");
                    Assert.That(o.StartSMeters, Is.EqualTo(0f));
                    Assert.That(o.EndSMeters, Is.EqualTo(movement.LengthMeters), "traversee sur toute la longueur");
                }
                if (i + 1 < plan.Occurrences.Count)
                    Assert.That(plan.Occurrences[i + 1].StartSMeters, Is.EqualTo(0f), "occurrences contigues");
            }
            Assert.That(viaCount, Is.EqualTo(1), "le mouvement vise apparait exactement une fois");
            Assert.That(plan.DistanceMeters, Is.EqualTo(distance).Within(1e-3), "longueur comptee une fois");
            if (!checkReuse) return;
            // Le plan re-emis est reutilisable tel quel : la reutilisation 5.29 valide la contiguite des raccords.
            var start = plan.Occurrences[0];
            var reused = Plan(model, At(model, start.Id, start.StartSMeters, start.Kind), via, default(RoadId), plan);
            Assert.That(reused.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(reused.Plan, Is.SameAs(plan), "plan contigu et valide : reutilise sans replanification");
        }

        // ------------------------------------------------------------------ semantique par phases

        [Test]
        public void TheCouplingTrapRevisitsAStateOncePerPhaseWhereAGlobalNoRevisitRuleWouldFail()
        {
            // S -> R -> X -(V)-> R -> E : R est visite une fois avant et une fois apres le mouvement vise.
            var graph = new Graph();
            graph.Corridors.AddRange(new[] { 10, 11, 12, 13 });            // S, R, X, E
            graph.Connections.Add((20, 10, 11));                             // S -> R
            graph.Connections.Add((21, 11, 12));                             // R -> X
            graph.Movements.Add((30, 12, 11, 1f));                          // V : X -> R
            graph.Movements.Add((31, 11, 13, 1f));                          // R -> E
            graph.Exits.Add((40, 13));
            var model = Compile(graph);

            var plain = Plan(model, At(model, Id(10)), RoadId.None);
            Assert.That(plain.Plan, Is.Not.Null);
            Assert.That(plain.Plan.Occurrences.Any(o => o.Id == Id(30)), Is.False, "sans objectif : route directe");

            var result = Plan(model, At(model, Id(10)), Id(30));
            Assert.That(result.Outcome, Is.EqualTo(RouteOutcome.Planned), result.Reason.ToString());
            var ids = result.Plan.Occurrences.Select(o => o.Id).ToArray();
            Assert.That(ids, Is.EqualTo(new[] { Id(10), Id(11), Id(12), Id(30), Id(11), Id(31), Id(13) }));
            Assert.That(result.Plan.ExitPortalId, Is.EqualTo(Id(40)));
            AssertWellFormed(model, result.Plan, Id(30));
        }

        [Test]
        public void ProgressNeverJumpsOverUnvisitedOccurrencesToALaterVisit()
        {
            // S -> R -> X -(V)-> R -> E (decision du 2026-09-30) : la progression n'avance que d'une occurrence contigue.
            // Une localisation sur la seconde visite de R alors que V n'est pas parcouru rend le plan perime ; elle ne
            // fait jamais sauter la progression par-dessus V (campagne 093040, run 1 : d = 6,54 m).
            var graph = new Graph();
            graph.Corridors.AddRange(new[] { 10, 11, 12, 13 });
            graph.Connections.Add((20, 10, 11));
            graph.Connections.Add((21, 11, 12));
            graph.Movements.Add((30, 12, 11, 1f));
            graph.Movements.Add((31, 11, 13, 1f));
            graph.Exits.Add((40, 13));
            var model = Compile(graph);
            var plan = Plan(model, At(model, Id(10)), Id(30)).Plan;
            Assert.That(plan.Occurrences.Select(o => o.Id).ToArray(),
                Is.EqualTo(new[] { Id(10), Id(11), Id(12), Id(30), Id(11), Id(31), Id(13) }));

            var onR = Plan(model, At(model, Id(11), 1f), Id(30), default(RoadId), plan);
            Assert.That(onR.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(onR.Plan.ProgressOccurrenceIndex, Is.EqualTo(1), "progression contigue");
            var onX = Plan(model, At(model, Id(12), 1f), Id(30), default(RoadId), onR.Plan);
            Assert.That(onX.Outcome, Is.EqualTo(RouteOutcome.Planned));
            Assert.That(onX.Plan.ProgressOccurrenceIndex, Is.EqualTo(2));

            var laterVisit = Plan(model, At(model, Id(11), 2f), Id(30), default(RoadId), onX.Plan);
            Assert.That(laterVisit.Outcome, Is.EqualTo(RouteOutcome.Replanned), "V non parcouru : pas de saut");
            Assert.That(laterVisit.Reason, Is.EqualTo(RouteReason.StalePlan));
            var skipped = Plan(model, At(model, Id(12), 1f), Id(30), default(RoadId), plan);
            Assert.That(skipped.Outcome, Is.EqualTo(RouteOutcome.Replanned), "R non parcouru : pas de saut");
        }

        [Test]
        public void AStateIsNeverRepeatedWithinOnePhaseAndACycleIsTakenAtMostOnce()
        {
            // Anneau R <-> Q, depart sur R, mouvement vise V : Q -> E. La route optimale fait au plus un tour.
            var graph = new Graph();
            graph.Corridors.AddRange(new[] { 10, 11, 12 });                  // R, Q, E
            graph.Connections.Add((20, 10, 11));
            graph.Connections.Add((21, 11, 10));
            graph.Movements.Add((30, 11, 12, 1f));
            graph.Exits.Add((40, 12));
            var model = Compile(graph);
            var result = Plan(model, At(model, Id(10), 1f), Id(30));
            Assert.That(result.Outcome, Is.EqualTo(RouteOutcome.Planned), result.Reason.ToString());
            var ids = result.Plan.Occurrences.Select(o => o.Id).ToList();
            Assert.That(ids, Is.EqualTo(new List<RoadId> { Id(10), Id(11), Id(30), Id(12) }));
            AssertWellFormed(model, result.Plan, Id(30));
        }

        [Test]
        public void EachPhaseFailsWithItsOwnNamedCode()
        {
            var graph = new Graph();
            graph.Corridors.AddRange(new[] { 10, 11, 12, 13 });
            graph.Connections.Add((20, 10, 11));
            graph.Movements.Add((30, 11, 12, 1f));   // atteignable, mais 12 ne mene a aucune sortie
            graph.Movements.Add((31, 13, 11, 1f));   // 13 inatteignable depuis 10
            graph.Movements.Add((32, 11, 13, 1f));   // 11 -> 13, la seule voie vers la sortie
            graph.Exits.Add((40, 13));
            var model = Compile(graph);

            var after = Plan(model, At(model, Id(10)), Id(30));
            Assert.That(after.Outcome, Is.EqualTo(RouteOutcome.NoRoute));
            Assert.That(after.Reason, Is.EqualTo(RouteReason.NoRouteAfterObjective));
            Assert.That(after.Plan, Is.Null, "jamais de plan partiel");

            var start13 = Plan(model, At(model, Id(12)), Id(31));
            Assert.That(start13.Outcome, Is.EqualTo(RouteOutcome.NoRoute));
            Assert.That(start13.Reason, Is.EqualTo(RouteReason.NoRouteToObjective));

            var unknown = Plan(model, At(model, Id(10)), Id(999));
            Assert.That(unknown.Outcome, Is.EqualTo(RouteOutcome.InvalidInput));
            Assert.That(unknown.Reason, Is.EqualTo(RouteReason.ObjectiveUnknown));
        }

        [Test]
        public void TheObjectiveIsKeptUntilTraversedAndDroppedOnTheMovementItself()
        {
            var graph = new Graph();
            graph.Corridors.AddRange(new[] { 10, 11, 12, 13 });
            graph.Connections.Add((20, 10, 11));
            graph.Connections.Add((21, 11, 12));
            graph.Movements.Add((30, 12, 11, 1f));
            graph.Movements.Add((31, 11, 13, 1f));
            graph.Exits.Add((40, 13));
            var model = Compile(graph);
            var plan = Plan(model, At(model, Id(10)), Id(30)).Plan;
            Assert.That(plan, Is.Not.Null);

            // Avant le mouvement vise, un replan force garde l'objectif.
            var replanned = Plan(model, At(model, Id(12), 3f), Id(30), default(RoadId), plan, true);
            Assert.That(replanned.Outcome, Is.EqualTo(RouteOutcome.Replanned));
            Assert.That(replanned.Plan.Occurrences.Count(o => o.Id == Id(30)), Is.EqualTo(1));
            Assert.That(replanned.Plan.ViaMovementId, Is.EqualTo(Id(30)));

            // Sur le mouvement vise, la traversee est en cours : route ordinaire depuis ce point.
            var onVia = Plan(model, At(model, Id(30), 2f, RoadElementKind.JunctionMovement), Id(30));
            Assert.That(onVia.Plan, Is.Not.Null);
            Assert.That(onVia.Plan.Occurrences[0].Id, Is.EqualTo(Id(30)));
            Assert.That(onVia.Plan.Occurrences.Count(o => o.Id == Id(30)), Is.EqualTo(1), "jamais une seconde traversee");
        }

        // ------------------------------------------------------------------ MVP_Run : completude contre un oracle

        private static CompiledRoadModel MvpRun()
        {
            var admission = TrafficV2Lifecycle.Admit(File.ReadAllText(TrafficV2Settings.ModelPath),
                File.ReadAllText(TrafficV2Settings.SignoffPath), File.ReadAllText(TrafficV2Settings.ReportPath));
            Assert.That(admission.Model, Is.Not.Null);
            return admission.Model;
        }

        /// <summary>Atteignabilite dirigee corridor -> corridor (liaisons et mouvements), independante du planificateur.</summary>
        private static HashSet<RoadId> Reachable(CompiledRoadModel model, RoadId from)
        {
            var next = new Dictionary<RoadId, List<RoadId>>();
            foreach (var c in model.Corridors) next[c.CorridorId] = new List<RoadId>();
            foreach (var k in model.Connections) next[k.FromCorridorId].Add(k.ToCorridorId);
            foreach (var m in model.Movements) next[m.FromCorridorId].Add(m.ToCorridorId);
            var seen = new HashSet<RoadId> { from };
            var stack = new Stack<RoadId>();
            stack.Push(from);
            while (stack.Count > 0)
                foreach (var to in next[stack.Pop()])
                    if (seen.Add(to)) stack.Push(to);
            return seen;
        }

        [Test]
        public void OnMvpRunTheObjectiveIsPlannedExactlyWhenAnIndependentOracleFindsAPhasedPath()
        {
            var model = MvpRun();
            Assert.That(model.Movements.Any(m => m.RoutePreferenceWeight == 0f), Is.False,
                "aucun poids nul : la regle du poids nul n'exclut aucun mouvement de MVP_Run");
            var entries = model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).ToList();
            var exits = model.Portals.Where(p => p.Role == PortalRole.Exit).OrderBy(p => p.Id).ToList();
            var selectable = new HashSet<RoadId>();
            int planned = 0;
            foreach (var entry in entries)
            {
                var fromEntry = Reachable(model, entry.CorridorId);
                foreach (var movement in model.Movements)
                {
                    bool toObjective = fromEntry.Contains(movement.FromCorridorId);
                    var afterObjective = Reachable(model, movement.ToCorridorId);
                    foreach (var exit in exits)
                    {
                        bool oracle = toObjective && afterObjective.Contains(exit.CorridorId);
                        var result = Plan(model, At(model, entry.CorridorId, entry.SMeters), movement.Id, exit.Id);
                        Assert.That(result.Plan != null, Is.EqualTo(oracle), entry.Id + " -> " + movement.Id + " -> " + exit.Id
                            + " : " + result.Outcome + "/" + result.Reason);
                        if (result.Plan == null)
                        {
                            Assert.That(result.Reason, Is.EqualTo(toObjective ? RouteReason.NoRouteAfterObjective : RouteReason.NoRouteToObjective));
                            continue;
                        }
                        planned++;
                        selectable.Add(movement.Id);
                        Assert.That(result.Plan.ExitPortalId, Is.EqualTo(exit.Id));
                        AssertWellFormed(model, result.Plan, movement.Id, false);
                    }
                }
            }
            TestContext.WriteLine("routes a objectif planifiees : " + planned + " ; mouvements selectionnables : " + selectable.Count + "/" + model.Movements.Count);
            Assert.That(selectable.Count, Is.EqualTo(model.Movements.Count),
                "un mouvement qu'une entree, une sortie ou une route atteint est selectionnable : "
                + string.Join(", ", model.Movements.Where(m => !selectable.Contains(m.Id)).Select(m => m.Label)));
        }

        // ------------------------------------------------------------------ frontiere d'activation

        [Test]
        public void OnlyTheMeasurementPathSetsTheObjective()
        {
            const string root = "Assets/RoadRage";
            var allowed = new HashSet<string>(StringComparer.Ordinal) {
                "Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlan.cs",
                "Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs",
                "Assets/RoadRage/Features/Vehicles/Traffic/PlanningSpine.cs",
                "Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2Composition.cs",
                "Assets/RoadRage/Features/Vehicles/Traffic/Lifecycle/TrafficV2VehicleDriver.cs",
                "Assets/RoadRage/App/Run/PortalTrafficSpawner.cs" };
            foreach (var path in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                string normalized = path.Replace('\\', '/');
                if (normalized.StartsWith(root + "/Tests/", StringComparison.Ordinal)) continue;
                string text = File.ReadAllText(path);
                if (text.IndexOf("ViaMovementId", StringComparison.OrdinalIgnoreCase) >= 0)
                    Assert.That(allowed.Contains(normalized), Is.True, "objectif intermediaire nomme hors du chemin de mesure : " + normalized);
                Assert.That(text.Contains("new CampaignTriplet("), Is.False,
                    "un triplet de campagne (seule source d'objectif) n'est construit que par les tests : " + normalized);
            }
            // Le spawner ne lit l'objectif que depuis le triplet du run de mesure.
            string spawner = File.ReadAllText("Assets/RoadRage/App/Run/PortalTrafficSpawner.cs");
            Assert.That(spawner, Does.Contain("viaMovementId = triplet.ViaMovementId;"));
        }
    }
}
