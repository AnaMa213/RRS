using System.Collections;
using System.Linq;
using NUnit.Framework;
using RoadRage.App.Run;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Policy;
using RoadRage.Features.Vehicles.Traffic.Tactical;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>
    /// Story 5.42 (decision proprietaire B du 2026-10-09) : dans MVP_Run, aucun contournement n'est geometriquement faisable
    /// (sections a double sens de 16 m au plus, aucun JunctionMovement admis). Sur le vehicule V2 de production et le pilote
    /// reel, un obstacle de gabarit voiture pose sur le premier corridor a double sens de sa route : le vehicule s'arrete
    /// derriere, chaque candidat est publie refuse, sans demande d'exception, sans but Maneuver, sans verrou epsilon_t, sans
    /// recuperation ni contact ; l'obstacle retire, il repart et sort par son portail.
    /// </summary>
    [Category("Story542")]
    public sealed class Story542BypassRefusalPlayModeTests
    {
        private const int MaxFixedSteps = 12000;
        /// <summary>Arret tenu derriere l'obstacle : au moins deux evaluations D1 (2 s chacune).</summary>
        private const float HeldSeconds = 6f;
        private Story533Harness harness;

        [SetUp]
        public void SetUp()
        {
            TrafficV2Session.Reset();
            harness = new Story533Harness();
        }

        [UnityTearDown]
        public IEnumerator TearDown() { yield return harness.Cleanup(); }

        [UnityTest]
        [Timeout(300000)]
        public IEnumerator BehindAStalledCarInMvpRunEveryCandidateIsRefusedAndTheVehicleWaitsThenLeaves()
        {
            var admission = TrafficV2Lifecycle.AdmitCommittedArtifacts();
            Assert.That(admission.Admitted, Is.True, admission.Code.ToString());
            var model = admission.Model;
            // Scenario 5.33 B (jeton de test, deux insertions NW -> NE) : la route traverse Ring_North_West (13,5 m, double
            // sens). La route de production, un demi-tour par le meme tunnel, n'a aucun corridor a double sens de 12 m.
            Story533Harness.ScenarioFile scenarioFile;
            var record = Story533Harness.Load(admission, "B", out scenarioFile);
            TrafficV2Session.Request(TrafficComposition.V2Slice, null, Story533Harness.ToScenario(record));
            yield return harness.EnterMvpRun();
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MVP_Run"));
            yield return harness.ParkHostPlayer(scenarioFile.Parking.Value, _ => { });
            var spawner = Object.FindAnyObjectByType<PortalTrafficSpawner>();
            Assert.That(spawner, Is.Not.Null);

            // Vehicule lie qui roule ; corridor a double sens d'au moins 12 m plus loin sur sa route.
            TrafficV2VehicleDriver driver = null;
            EffectiveLaneCorridor target = default(EffectiveLaneCorridor);
            bool found = false;
            for (int i = 0; i < MaxFixedSteps && !found; i++)
            {
                yield return new WaitForFixedUpdate();
                driver = Object.FindObjectsByType<TrafficV2VehicleDriver>().FirstOrDefault(d => d.IsBound && d.IntentsApplied > 0);
                if (driver == null || driver.LastProjection == null) continue;
                var occurrences = driver.LastProjection.RouteOccurrences;
                int current = -1;
                for (int k = 0; k < occurrences.Count; k++) if (occurrences[k].Id == driver.LastProjection.ElementId) { current = k; break; }
                if (current < 0) continue;
                for (int k = current + 2; k < occurrences.Count && !found; k++)
                {
                    EffectiveLaneCorridor corridor;
                    if (occurrences[k].Kind != RoadElementKind.LaneCorridor || !model.TryGetCorridor(occurrences[k].Id, out corridor)) continue;
                    if (model.GetCorridorsInSection(corridor.SectionId).Count != 2 || corridor.Curve.Length < 12f) continue;
                    target = corridor;
                    found = true;
                }
            }
            Assert.That(found, Is.True, "corridor a double sens sur la route du vehicule");
            Assert.That(driver.MeasurementLabel, Is.Null, "hors run de mesure");

            // Voiture en panne simulee : boite cinematique de gabarit voiture, centree sur la reference, 4 m avant la fin.
            var at = target.Curve.Sample(target.Curve.Length - 4f);
            var obstacle = harness.CreateObstacle(new Story533Harness.ObstacleRecord
            {
                Label = "Story542",
                Center = Vector(at.Position + at.Up * 0.75f),
                Forward = Vector(at.Tangent),
                Size = Vector(new Vector3(2.06f, 1.5f, 4.44f))
            });

            var body = driver.GetComponent<Rigidbody>();
            float stopped = 0f;
            bool everManeuver = false, everLatched = false, maneuverLine = false;
            int requests = 0, recoveries = 0;
            for (int i = 0; i < MaxFixedSteps && stopped < HeldSeconds; i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.That(spawner.V2Removals, Is.Zero, "le vehicule ne sort pas a travers l'obstacle");
                foreach (var any in Object.FindObjectsByType<TrafficV2VehicleDriver>())
                {
                    if (!any.IsBound) continue;
                    everManeuver |= any.Tactical.ManeuverActive;
                    everLatched |= any.ToleranceResponse.Latched;
                    requests += any.RuleExceptionRequestCount > 0 ? 1 : 0;
                    recoveries += any.Recovery.Attempts.Count;
                }
                maneuverLine |= driver.LastProjection != null && driver.LastProjection.Maneuver != null
                    && driver.LastProjection.Maneuver.StartsWith("Maneuver frame");
                bool onTarget = driver.LastProjection != null && driver.LastProjection.ElementId == target.CorridorId;
                stopped = onTarget && body.linearVelocity.magnitude < 0.2f ? stopped + Time.fixedDeltaTime : 0f;
            }
            Assert.That(stopped, Is.GreaterThanOrEqualTo(HeldSeconds), "arret tenu derriere l'obstacle");

            var evaluation = driver.LastManeuverEvaluation;
            Assert.That(evaluation, Is.Not.Null, "evaluation publiee");
            Assert.That(maneuverLine, Is.True, "ligne Maneuver de la projection publiee");
            string text = evaluation.ToText();
            Assert.That(evaluation.Selected, Is.Null, text);
            Assert.That(evaluation.Candidates.All(c => c.Verdict == ManeuverVerdict.NotOffered || c.Verdict == ManeuverVerdict.GeometryInfeasible),
                Is.True, text);
            Assert.That(evaluation.Candidates.Single(c => c.Kind == ManeuverKind.OpposingCorridor).Verdict,
                Is.EqualTo(ManeuverVerdict.GeometryInfeasible), "corridor oppose offert mais infaisable : " + text);
            Assert.That(driver.RuleExceptionRequestCount + requests, Is.Zero, "aucune demande d'exception, sur aucun vehicule");
            Assert.That(recoveries, Is.Zero, "aucune recuperation, sur aucun vehicule");
            Assert.That(everManeuver, Is.False, "aucun but Maneuver");
            Assert.That(everLatched, Is.False, "aucun verrou epsilon_t");
            Assert.That(driver.Recovery.Attempts, Is.Empty, "attente legitime : aucune recuperation");
            Assert.That(driver.ContactEpisodes.Any(c => c.ColliderPath.Contains("Story533_Obstacle_")), Is.False, "aucun contact");
            Debug.Log("[Story542] mesure MVP_Run, " + target.CorridorId + " L " + target.Curve.Length.ToString("0.0") + " : " + text);

            harness.Destroy(obstacle);
            int expected = record.Insertions.Length;
            for (int i = 0; i < MaxFixedSteps && spawner.V2Removals < expected; i++) yield return new WaitForFixedUpdate();
            Assert.That(spawner.V2Removals, Is.EqualTo(expected), "sorties normales aux portails apres le retrait de l'obstacle");
            Assert.That(driver == null || driver.Recovery.Faulted == false, Is.True);
        }

        private static Story533Harness.VectorRecord Vector(Vector3 value)
        {
            return new Story533Harness.VectorRecord { x = value.x, y = value.y, z = value.z };
        }
    }
}
