using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Planning;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.35, phase 5 : constructeur deterministe des scenarios PlayMode E, F, G et des campagnes Gate C (2, 4, 8), ecrits
    /// dans scenarios-5-35.json au format du harnais 5.33. E : rencontre ZC23, la branche Yield cede a l'axe Priority. F :
    /// priorite a droite a la croix, le vehicule de gauche portant le plus petit TrafficId. G : entree ouest de Roundabout_SouthWest
    /// face a deux vehicules d'anneau qui passent sa fusion. Gate C : chaque campagne couvre la croix, un T et un giratoire.
    /// </summary>
    [Category("Core")]
    [Category("Story535")]
    public sealed class Story535ScenarioBuilderTests
    {
        private const string DriverProfilePath = "Assets/RoadRage/ScriptableObjects/Vehicles/DriverProfileDef_Default.asset";
        private const string ScenarioFolder = "_bmad-output/implementation-artifacts/traffic-v2-5-35-explorations";
        private const string ScenarioPath = ScenarioFolder + "/scenarios-5-35.json";
        private const float Dt = 0.02f;
        /// <summary>Distance du pare-chocs a la frontiere de controle a l'arret derriere un obstacle d'attente (m), celle de C.</summary>
        private const float WaitingDistanceMeters = 8f;

        private static readonly VehicleFootprint Car = new VehicleFootprint
            { FrontMeters = 2.22f, RearMeters = 2.22f, LeftMeters = 1.03f, RightMeters = 1.03f };
        /// <summary>ConflictZones[23] de MVP_Run (TJunction_West) : FromEast tout droit (axe Priority) et FromSouth a gauche (branche Yield).</summary>
        private static readonly RoadId EastStraight = RoadId.Parse("40ca7f10a97f50a918e8c3a2a1e58493");
        private static readonly RoadId SouthLeft = RoadId.Parse("4e437f94525852d3a072a538537c2093");
        /// <summary>Roundabout_SouthWest : entree ouest (Yield) et continuation d'anneau qui la rejoint a Ring_Merge_West (Merge).</summary>
        private const string WestEntryPrefix = "43605e56";
        private const string WestContinuationPrefix = "4e0c96d3";

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

        // ================================================================== format du harnais 5.33

        [Serializable]
        private sealed class VectorRecord
        {
            public float x, y, z;
            public static VectorRecord Of(Vector3 v) { return new VectorRecord { x = v.x, y = v.y, z = v.z }; }
        }

        [Serializable]
        private sealed class InsertionRecord
        {
            public string Entry;
            public string Exit;
            public ulong Seed;
            public ulong EarliestStep;
            public string[] Elements;
        }

        [Serializable]
        private sealed class ObstacleRecord
        {
            public string Label;
            public string ElementId;
            public float RouteDistanceMeters;
            public VectorRecord Center;
            public VectorRecord Forward;
            public VectorRecord Size;
            public int FromStep;
            public int UntilStep;
        }

        [Serializable]
        private sealed class PushRecord
        {
            public int Insertion;
            public int AtStep;
            public float LateralImpulsePerKilogram;
        }

        [Serializable]
        private sealed class ScenarioRecord
        {
            public string Label;
            public int MaxPopulation;
            public int MaxSteps;
            public InsertionRecord[] Insertions;
            public ObstacleRecord[] Obstacles;
            public PushRecord[] Pushes;
        }

        [Serializable]
        private sealed class ScenarioFile
        {
            public int Format;
            public string RoadModelVersion;
            public float VehicleLengthMeters;
            public float MinimumGapMeters;
            public float MinimumObstacleDistanceMeters;
            public VectorRecord Parking;
            public ScenarioRecord[] Scenarios;
        }

        private const float ObstacleLengthMeters = 1f;
        private const float ObstacleWidthMeters = 2f;
        private const float ObstacleHeightMeters = 1.5f;

        private static RoadId Movement(string prefix)
        {
            var found = Model.Movements.Where(m => m.Id.ToString().StartsWith(prefix, StringComparison.Ordinal)).ToList();
            Assert.That(found.Count, Is.EqualTo(1), "mouvement " + prefix);
            return found[0].Id;
        }

        private static InsertionRecord Record(Portal entry, Portal exit, ulong seed, ulong earliest, TrafficV2Insertion prepared)
        {
            return new InsertionRecord { Entry = entry.Id.ToString(), Exit = exit.Id.ToString(), Seed = seed, EarliestStep = earliest,
                Elements = prepared.Route.Occurrences.Select(o => o.Id.ToString()).ToArray() };
        }

        /// <summary>Premiere insertion (graine, entree, sortie) dont la route, au compteur donne, passe par le mouvement.</summary>
        private static TrafficV2Insertion InsertionVia(RoadId movement, ulong counter, ulong earliest, out InsertionRecord record)
        {
            var prepared = TryInsertionVia(movement, counter, earliest, out record);
            Assert.That(prepared, Is.Not.Null, "aucune insertion ne passe par " + movement);
            return prepared;
        }

        private static TrafficV2Insertion TryInsertionVia(RoadId movement, ulong counter, ulong earliest, out InsertionRecord record)
        {
            var entries = Model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).ToList();
            var exits = Model.Portals.Where(p => p.Role == PortalRole.Exit).OrderBy(p => p.Id).ToList();
            for (ulong seed = 0; seed < 16; seed++)
                foreach (var entry in entries)
                    foreach (var exit in exits)
                    {
                        var prepared = TrafficV2Lifecycle.PrepareInsertion(Admission, entry.Id, exit.Id, seed, counter, Driver, Dt);
                        if (prepared.Code != TrafficV2Code.Allowed || !prepared.Route.Occurrences.Any(o => o.Id == movement)) continue;
                        record = Record(entry, exit, seed, earliest, prepared);
                        return prepared;
                    }
            record = null;
            return null;
        }

        private static float EntryDistance(RoutePlan route, RoadId movement)
        {
            float entry = 0f;
            for (int k = 0; route.Occurrences[k].Id != movement; k++)
                entry += route.Occurrences[k].EndSMeters - route.Occurrences[k].StartSMeters;
            return entry;
        }

        /// <summary>
        /// Obstacle d'attente sur le corridor droit d'approche du mouvement, face proche a <paramref name="fromBoundary"/> de la
        /// frontiere de controle b du mouvement (s_line, sinon son entree) : deux vehicules ainsi retenus sont a la meme distance
        /// de leur frontiere.
        /// </summary>
        private static ObstacleRecord Waiting(RoutePlan route, RoadId movement, float fromBoundary, string label)
        {
            float near = EntryDistance(route, movement) + Index.BoundaryOf(movement) - fromBoundary;
            var corridorId = Index.FromCorridorOf(movement);
            var track = ReferenceTrack.FromRoute(Model, route.Occurrences, 0f);
            var piece = track.Pieces.First(p => p.Id == corridorId);
            EffectiveLaneCorridor corridor;
            Assert.That(Model.TryGetCorridor(corridorId, out corridor), Is.True, "obstacle sur un corridor");
            Assert.That(corridor.Samples.All(s => Math.Abs(s.CurvaturePerMeter) < 1e-4f), Is.True, "corridor droit");
            Assert.That(near, Is.GreaterThanOrEqualTo(piece.StartDistanceMeters + 0.1f), label);
            Assert.That(near + ObstacleLengthMeters, Is.LessThanOrEqualTo(piece.EndDistanceMeters - 0.1f), label);
            var nominal = piece.Nominal(near + ObstacleLengthMeters * 0.5f);
            return new ObstacleRecord { Label = label, ElementId = corridorId.ToString(), RouteDistanceMeters = near,
                Center = VectorRecord.Of(nominal.Position + nominal.Up * (ObstacleHeightMeters * 0.5f)), Forward = VectorRecord.Of(nominal.Forward),
                Size = VectorRecord.Of(new Vector3(ObstacleWidthMeters, ObstacleHeightMeters, ObstacleLengthMeters)), FromStep = 0, UntilStep = -1 };
        }

        /// <summary>
        /// F : premiere paire (X, Y) de mouvements routables de la croix, incompatibles, ou X vient de la droite de Y (preseance de X) ; Y
        /// est insere d'abord et porte le plus petit TrafficId, le departage generique le servirait donc en premier.
        /// </summary>
        private static RoadId[] RightOfWayPair()
        {
            var cross = new HashSet<RoadId>(Model.Junctions.Where(j => j.Feature == JunctionFeature.Crossroads).Select(j => j.Id));
            var movements = Model.Movements.Where(m => cross.Contains(m.JunctionId)).Select(m => m.Id).OrderBy(m => m).ToList();
            foreach (var x in movements)
                foreach (var y in movements)
                {
                    RoadId zone;
                    InsertionRecord ignored;
                    if (x != y && Index.TryGetConflict(x, y, out zone) && Index.HasPrecedence(x, y)
                        && TryInsertionVia(x, 2UL, 0UL, out ignored) != null && TryInsertionVia(y, 1UL, 0UL, out ignored) != null)
                        return new[] { x, y };
                }
            Assert.Fail("aucune paire routable de la croix avec priorite a droite");
            return null;
        }

        private static HashSet<JunctionFeature> Features(IEnumerable<string> elements)
        {
            var junctions = Model.Junctions.ToDictionary(j => j.Id, j => j.Feature);
            var features = new HashSet<JunctionFeature>();
            foreach (var id in elements)
            {
                CompiledJunctionMovement movement;
                if (Model.TryGetMovement(RoadId.Parse(id), out movement)) features.Add(junctions[movement.JunctionId]);
            }
            return features;
        }

        private static string BuildScenarioText()
        {
            var car = Car;
            float length = car.FrontMeters + car.RearMeters;
            var scenarios = new List<ScenarioRecord>();

            // E : la branche (plus petit TrafficId) et l'axe a la meme distance de leur frontiere ; la branche est liberee la
            // premiere et demande alors que l'axe, prioritaire, approche : elle cede (YieldToPriority), s'arrete a sa ligne,
            // puis est servie une fois l'axe passe.
            InsertionRecord south, east;
            var southE = InsertionVia(SouthLeft, 1UL, 0UL, out south);
            var eastE = InsertionVia(EastStraight, 2UL, 0UL, out east);
            scenarios.Add(new ScenarioRecord { Label = "E", MaxPopulation = 2, MaxSteps = 9000, Insertions = new[] { south, east },
                Obstacles = new[] { Waiting(southE.Route, SouthLeft, WaitingDistanceMeters, "attente-branche"),
                    Waiting(eastE.Route, EastStraight, WaitingDistanceMeters, "attente-axe") },
                Pushes = new PushRecord[0] });

            // F : meme mise en scene a la croix ; le vehicule de gauche (Y) est insere et libere le premier.
            var pair = RightOfWayPair();
            InsertionRecord left, right;
            var leftF = InsertionVia(pair[1], 1UL, 0UL, out left);
            var rightF = InsertionVia(pair[0], 2UL, 0UL, out right);
            scenarios.Add(new ScenarioRecord { Label = "F", MaxPopulation = 2, MaxSteps = 9000, Insertions = new[] { left, right },
                Obstacles = new[] { Waiting(leftF.Route, pair[1], WaitingDistanceMeters, "attente-gauche"),
                    Waiting(rightF.Route, pair[0], WaitingDistanceMeters, "attente-droite") },
                Pushes = new PushRecord[0] });

            // G : retenir le premier vehicule d'anneau sur son approche jusqu'a ce que l'entrant soit pret. Sans cette
            // synchronisation, les deux vehicules d'anneau sortent avant que l'entrant atteigne son obstacle.
            var entryG = Movement(WestEntryPrefix);
            var continuation = Movement(WestContinuationPrefix);
            InsertionRecord ringFirst, ringSecond, entering;
            var firstRingG = InsertionVia(continuation, 1UL, 0UL, out ringFirst);
            InsertionVia(continuation, 3UL, 150UL, out ringSecond);
            var enteringG = InsertionVia(entryG, 2UL, 0UL, out entering);
            Assert.That(entering.Elements, Has.No.Member(continuation.ToString()), "l'entrant ne passe pas par la continuation");
            var ringApproach = firstRingG.Route.Occurrences.First(o => Model.Movements.Any(m => m.Id == o.Id)).Id;
            // Le spawner traite les insertions dans l'ordre : l'entrant doit preceder le second anneau, dont le portail
            // reste occupe par le premier anneau retenu. Sinon l'entrant ne peut pas etre cree pour liberer le staging.
            scenarios.Add(new ScenarioRecord { Label = "G", MaxPopulation = 3, MaxSteps = 9000, Insertions = new[] { ringFirst, entering, ringSecond },
                Obstacles = new[] { Waiting(enteringG.Route, entryG, WaitingDistanceMeters, "attente-entree"),
                    Waiting(firstRingG.Route, ringApproach, 2f, "attente-anneau") },
                Pushes = new PushRecord[0] });

            // Gate C : deux insertions par entree vers deux sorties atteignables (rotation par entree, construction d'explore-8) ;
            // la seconde vague passe par la croix. N = 8 : les deux vagues ; N = 4 : la seconde vague d'un coup ; N = 2 : ses deux
            // premieres routes. Sans obstacle ni poussee : campagnes nominales.
            var entries = Model.Portals.Where(p => p.Role == PortalRole.Entry).OrderBy(p => p.Id).ToList();
            var exits = Model.Portals.Where(p => p.Role == PortalRole.Exit).OrderBy(p => p.Id).ToList();
            var waves = new[] { new List<KeyValuePair<Portal, Portal>>(), new List<KeyValuePair<Portal, Portal>>() };
            for (int e = 0; e < entries.Count; e++)
            {
                int wave = 0;
                for (int k = 0; k < exits.Count && wave < 2; k++)
                {
                    var exit = exits[(e + 2 + k) % exits.Count];
                    if (TrafficV2Lifecycle.PrepareInsertion(Admission, entries[e].Id, exit.Id, 0UL, 1UL, Driver, Dt).Code != TrafficV2Code.Allowed)
                        continue;
                    waves[wave++].Add(new KeyValuePair<Portal, Portal>(entries[e], exit));
                }
                Assert.That(wave, Is.EqualTo(2), "deux sorties atteignables depuis l'entree " + entries[e].Id);
            }
            Func<IEnumerable<KeyValuePair<Portal, Portal>>, ulong, InsertionRecord[]> campaign = (pairs, secondWave) =>
            {
                var records = new List<InsertionRecord>();
                ulong slot = 1;
                foreach (var p in pairs)
                {
                    ulong counter = slot++;
                    var prepared = TrafficV2Lifecycle.PrepareInsertion(Admission, p.Key.Id, p.Value.Id, 0UL, counter, Driver, Dt);
                    Assert.That(prepared.Code, Is.EqualTo(TrafficV2Code.Allowed));
                    records.Add(Record(p.Key, p.Value, 0UL, records.Count >= entries.Count ? secondWave : 0UL, prepared));
                }
                return records.ToArray();
            };
            scenarios.Add(new ScenarioRecord { Label = "gatec-2", MaxPopulation = 2, MaxSteps = 7500, Insertions = campaign(waves[1].Take(2), 0UL),
                Obstacles = new ObstacleRecord[0], Pushes = new PushRecord[0] });
            scenarios.Add(new ScenarioRecord { Label = "gatec-4", MaxPopulation = 4, MaxSteps = 7500, Insertions = campaign(waves[1], 0UL),
                Obstacles = new ObstacleRecord[0], Pushes = new PushRecord[0] });
            scenarios.Add(new ScenarioRecord { Label = "gatec-8", MaxPopulation = 8, MaxSteps = 9000, Insertions = campaign(waves[0].Concat(waves[1]), 300UL),
                Obstacles = new ObstacleRecord[0], Pushes = new PushRecord[0] });

            float maxX = float.NegativeInfinity, minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;
            foreach (var corridor in Model.Corridors)
                foreach (var sample in corridor.Samples)
                { maxX = Math.Max(maxX, sample.Position.x); minZ = Math.Min(minZ, sample.Position.z); maxZ = Math.Max(maxZ, sample.Position.z); }
            var file = new ScenarioFile { Format = 1, RoadModelVersion = Model.Version.ToString(), VehicleLengthMeters = length,
                MinimumGapMeters = Driver.MinimumGap, MinimumObstacleDistanceMeters = WaitingDistanceMeters,
                Parking = VectorRecord.Of(new Vector3(maxX + 150f, 0f, 0.5f * (minZ + maxZ))), Scenarios = scenarios.ToArray() };
            return JsonUtility.ToJson(file, true);
        }

        [Test]
        public void TheScenarioBuilderIsDeterministicAndWritesScenariosEFGAndTheGateCCampaigns()
        {
            string text = BuildScenarioText();
            Assert.That(BuildScenarioText(), Is.EqualTo(text), "memes entrees, memes scenarios");
            var file = JsonUtility.FromJson<ScenarioFile>(text);
            Assert.That(file.Scenarios.Select(s => s.Label), Is.EqualTo(new[] { "E", "F", "G", "gatec-2", "gatec-4", "gatec-8" }));

            var e = file.Scenarios[0];
            Assert.That(e.Insertions[0].Elements, Has.Member(SouthLeft.ToString()));
            Assert.That(e.Insertions[1].Elements, Has.Member(EastStraight.ToString()));
            Assert.That(Index.ControlKindOf(SouthLeft), Is.EqualTo(JunctionControlKind.Yield));
            Assert.That(Index.ControlKindOf(EastStraight), Is.EqualTo(JunctionControlKind.Priority));
            Assert.That(Index.BoundaryOf(SouthLeft), Is.GreaterThan(0f), "la branche porte sa StopLine");

            var pair = RightOfWayPair();
            var f = file.Scenarios[1];
            Assert.That(f.Insertions[0].Elements, Has.Member(pair[1].ToString()), "le vehicule de gauche est insere le premier");
            Assert.That(f.Insertions[1].Elements, Has.Member(pair[0].ToString()));
            Assert.That(Index.ControlKindOf(pair[0]), Is.EqualTo(JunctionControlKind.Uncontrolled));
            Assert.That(Index.HasPrecedence(pair[1], pair[0]), Is.False);

            var g = file.Scenarios[2];
            var continuation = Movement(WestContinuationPrefix);
            Assert.That(g.Insertions[0].Elements, Has.Member(continuation.ToString()));
            Assert.That(g.Insertions[2].Elements, Has.Member(continuation.ToString()));
            Assert.That(g.Insertions[1].Elements, Has.Member(Movement(WestEntryPrefix).ToString()),
                "l'entrant est cree avant le second anneau retenu au portail par le premier");
            Assert.That(g.Insertions[1].EarliestStep, Is.LessThanOrEqualTo(g.Insertions[2].EarliestStep));
            RoadId zone;
            ConflictKind kind;
            float startEntry, startRing;
            Assert.That(Index.TryGetConflict(Movement(WestEntryPrefix), continuation, out zone, out kind, out startEntry, out startRing), Is.True);
            Assert.That(kind, Is.EqualTo(ConflictKind.Merge), "fusion ouest typee Merge (5.53a)");
            var ringWait = g.Obstacles.Single(o => o.Label == "attente-anneau");
            Assert.That(g.Insertions[0].Elements, Has.Member(ringWait.ElementId));
            Assert.That(g.Insertions[2].Elements, Has.Member(ringWait.ElementId), "les deux vehicules utilisent la meme approche");
            Assert.That(g.Insertions[1].Elements, Has.No.Member(ringWait.ElementId), "la synchronisation ne bloque pas l'entrant");
            Assert.That(ringWait.ElementId, Is.EqualTo(g.Insertions[0].Elements[0]), "obstacle avant la premiere entree d'anneau");
            Assert.That(ringWait.RouteDistanceMeters, Is.GreaterThan(file.VehicleLengthMeters + Driver.MinimumGap),
                "l'obstacle laisse une distance de retenue depuis la pose initiale ; cree apres insertion pour respecter le rayon du portail");
            Assert.That(g.Obstacles.Single(o => o.Label == "attente-entree").ElementId,
                Is.Not.EqualTo(ringWait.ElementId), "les obstacles d'attente sont independants");

            foreach (var campaign in file.Scenarios.Skip(3))
            {
                Assert.That(campaign.Insertions.Length, Is.EqualTo(campaign.MaxPopulation), campaign.Label);
                var features = Features(campaign.Insertions.SelectMany(i => i.Elements));
                Assert.That(features, Is.SupersetOf(new[] { JunctionFeature.Crossroads, JunctionFeature.TJunction, JunctionFeature.Roundabout }),
                    campaign.Label + " : croix, T et giratoire");
                Assert.That(campaign.Obstacles, Is.Empty, campaign.Label + " : campagne nominale");
            }
            TestContext.WriteLine("F : " + pair[0] + " (droite) / " + pair[1] + " (gauche) ; ZC ouest " + zone + " debuts "
                + startEntry.ToString("0.###", CultureInfo.InvariantCulture) + " / " + startRing.ToString("0.###", CultureInfo.InvariantCulture));
            Directory.CreateDirectory(ScenarioFolder);
            File.WriteAllText(ScenarioPath, text);
        }
    }
}
