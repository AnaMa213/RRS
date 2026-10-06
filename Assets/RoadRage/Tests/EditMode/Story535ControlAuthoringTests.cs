using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Coordination;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using RoadRage.Features.Vehicles.Traffic.Migration;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.35, phase 1 -- authoring des genres de controle et des lignes d'arret : format 5 des decisions, refus du
    /// validateur (melange, ligne hors Stop/Yield, croisement absent ou multiple), projection s_line, relation de droite
    /// (rotations, miroir, bande ambigue), faits de modele P2, verification d'instance des T et separation positive des lignes.
    /// </summary>
    [Category("Core")]
    [Category("Story535")]
    public sealed class Story535ControlAuthoringTests
    {
        /// <summary>Bord avance de la fenetre d'arret de JunctionEntry : pare-chocs a m_ctrl - 0,02 de la ligne (matrice 5.35).</summary>
        private const float StopWindowToleranceMeters = 0.02f;

        // ============================================================ format 5

        [Test]
        public void Format5RoundTripsAStopLineAndFormat4StillReadsWithoutLines()
        {
            var decisions = AuthoringDecisions.Parse(File.ReadAllText(AuthoredRoadModel.DecisionsPath));
            Assert.That(AuthoringDecisions.FormatVersion, Is.EqualTo(5));
            var lined = decisions.Controls.Where(c => c.HasStopLine).ToList();
            Assert.That(lined.Count, Is.EqualTo(4), "Une ligne par branche de T, aucune ailleurs.");
            Assert.That(lined.All(c => c.Kind == JunctionControlKind.Yield), Is.True);

            var again = AuthoringDecisions.Parse(decisions.Serialize());
            for (int i = 0; i < decisions.Controls.Count; i++)
            {
                Assert.That(again.Controls[i].Kind, Is.EqualTo(decisions.Controls[i].Kind));
                Assert.That(again.Controls[i].HasStopLine, Is.EqualTo(decisions.Controls[i].HasStopLine));
                Assert.That(again.Controls[i].StopLine.Start, Is.EqualTo(decisions.Controls[i].StopLine.Start));
                Assert.That(again.Controls[i].StopLine.End, Is.EqualTo(decisions.Controls[i].StopLine.End));
            }
            Assert.That(decisions.Serialize(), Is.EqualTo(File.ReadAllText(AuthoredRoadModel.DecisionsPath)), "Le fichier committe est la serialisation canonique.");

            // Un fichier format 4 se lit sans ligne, meme si un champ de ligne y figurait.
            string format4 = decisions.Serialize().Replace("\"Format\": 5", "\"Format\": 4");
            Assert.That(AuthoringDecisions.Parse(format4).Controls.Any(c => c.HasStopLine), Is.False);

            int index = decisions.Controls.FindIndex(c => c.HasStopLine);
            var degenerate = decisions.Controls[index];
            degenerate.StopLine.End = degenerate.StopLine.Start;
            decisions.Controls[index] = degenerate;
            Assert.Throws<FormatException>(() => AuthoringDecisions.Parse(decisions.Serialize()), "Une ligne de longueur nulle est refusee.");
        }

        [Test]
        public void LineDispositionFollowsTheJunctionControls()
        {
            var yieldWithLine = new ControlDecision { Kind = JunctionControlKind.Yield, HasStopLine = true };
            var yieldWithout = new ControlDecision { Kind = JunctionControlKind.Yield };
            var priority = new ControlDecision { Kind = JunctionControlKind.Priority };
            var uncontrolled = new ControlDecision { Kind = JunctionControlKind.Uncontrolled };
            Assert.That(AuthoringDecisions.ExpectedLineKind(new[] { uncontrolled, uncontrolled }), Is.EqualTo(TaskDispositionKind.NotRequiredForCurrentControlKind));
            Assert.That(AuthoringDecisions.ExpectedLineKind(new[] { priority, priority, yieldWithLine }), Is.EqualTo(TaskDispositionKind.AuthoredOnControls));
            Assert.That(AuthoringDecisions.ExpectedLineKind(new[] { priority, yieldWithout }), Is.EqualTo(TaskDispositionKind.GenericEntryFallback));
            Assert.That(AuthoringDecisions.ExpectedLineKind(new[] { yieldWithLine, yieldWithout }), Is.EqualTo(TaskDispositionKind.GenericEntryFallback));
        }

        // ============================================================ validateur

        [Test]
        public void ValidatorRefusesMixedUncontrolledAndLinesOutsideStopOrYield()
        {
            var source = AuthoredSource();
            Assert.That(RoadModelValidator.Validate(source), Is.Empty, "Le modele authore (genres et lignes) valide.");

            var mixed = Copy(source);
            int crossing = Array.FindIndex(mixed.Controls, c => c.Kind == JunctionControlKind.Uncontrolled);
            mixed.Controls[crossing].Kind = JunctionControlKind.Yield;
            Assert.That(RoadModelValidator.Validate(mixed).Select(i => i.Code), Has.Member(RoadModelValidationCode.JunctionControlKindsMixed));

            var onPriority = Copy(source);
            int lined = Array.FindIndex(onPriority.Controls, c => c.HasStopLine);
            onPriority.Controls[lined].Kind = JunctionControlKind.Priority;
            Assert.That(RoadModelValidator.Validate(onPriority).Select(i => i.Code), Has.Member(RoadModelValidationCode.StopLineInvalid));

            var away = Copy(source);
            away.Controls[lined].StopLine.Start += new Vector3(0f, 0f, 500f);
            away.Controls[lined].StopLine.End += new Vector3(0f, 0f, 500f);
            Assert.That(RoadModelValidator.Validate(away).Select(i => i.Code), Has.Member(RoadModelValidationCode.StopLineInvalid),
                "Une ligne qui ne coupe pas le mouvement est refusee.");
        }

        [Test]
        public void ProjectionKeepsExactlyOneInteriorCrossing()
        {
            var straight = Straight(10f);
            var line = new RoadLineSegment { Start = new Vector3(-2f, 0f, 4f), End = new Vector3(2f, 0f, 4f) };
            float s;
            Assert.That(StopLineProjection.TryProject(straight, 10f, line, out s), Is.True);
            Assert.That(s, Is.EqualTo(4f).Within(1e-4f));

            var outside = new RoadLineSegment { Start = new Vector3(1f, 0f, 4f), End = new Vector3(3f, 0f, 4f) };
            Assert.That(StopLineProjection.TryProject(straight, 10f, outside, out s), Is.False, "La ligne ne coupe pas l'axe.");

            var atStart = new RoadLineSegment { Start = new Vector3(-2f, 0f, 0f), End = new Vector3(2f, 0f, 0f) };
            Assert.That(StopLineProjection.TryProject(straight, 10f, atStart, out s), Is.False, "Croisement au bord du mouvement.");

            // Un mouvement en epingle coupe deux fois une ligne transversale.
            var hairpin = new List<RoadCurveSample>();
            for (int i = 0; i <= 40; i++)
            {
                float angle = Mathf.PI * i / 40f;
                hairpin.Add(Sample(i * Mathf.PI * 5f / 40f, new Vector3(5f - 5f * Mathf.Cos(angle), 0f, 5f * Mathf.Sin(angle))));
            }
            var across = new RoadLineSegment { Start = new Vector3(-1f, 0f, 2f), End = new Vector3(11f, 0f, 2f) };
            Assert.That(StopLineProjection.Crossings(hairpin, across).Count, Is.EqualTo(2));
            Assert.That(StopLineProjection.TryProject(hairpin, Mathf.PI * 5f, across, out s), Is.False);
        }

        [Test]
        public void TheCompiledModelProjectsEachBranchLineOnBothBranchMovements()
        {
            var compiled = RoadModelCompiler.Compile(AuthoredSource());
            var report = new StringBuilder();
            int lines = 0;
            foreach (var control in compiled.Controls)
            {
                if (!control.HasStopLine) continue;
                lines++;
                foreach (var movementId in control.ControlledMovementIds)
                {
                    CompiledJunctionMovement movement;
                    Assert.That(compiled.TryGetMovement(movementId, out movement), Is.True);
                    float sLine;
                    Assert.That(compiled.TryGetStopLine(movementId, out sLine), Is.True, movement.Label);
                    float expected = movement.Label.Contains("(droite)") ? 3.47f : 3.41f;
                    Assert.That(sLine, Is.EqualTo(expected).Within(0.02f), movement.Label);
                    report.Append(movement.Label).Append(" s_line ").Append(F(sLine)).Append('\n');
                }
            }
            Assert.That(lines, Is.EqualTo(4));
            foreach (var movement in compiled.Movements)
            {
                float sLine;
                CompiledJunctionControl control;
                compiled.TryGetControlForMovement(movement.Id, out control);
                Assert.That(compiled.TryGetStopLine(movement.Id, out sLine), Is.EqualTo(control.HasStopLine), movement.Label);
            }
            TestContext.WriteLine(report.ToString());
        }

        // ============================================================ relation de droite

        [Test]
        public void RightOfWayIsRotationInvariantAndMirrorSwapsLeftAndRight()
        {
            var up = Vector3.up;
            var cases = new[]
            {
                new KeyValuePair<float, RightOfWayRelation>(0f, RightOfWayRelation.Same),
                new KeyValuePair<float, RightOfWayRelation>(-90f, RightOfWayRelation.FromRight),
                new KeyValuePair<float, RightOfWayRelation>(90f, RightOfWayRelation.FromLeft),
                new KeyValuePair<float, RightOfWayRelation>(180f, RightOfWayRelation.Opposite),
                new KeyValuePair<float, RightOfWayRelation>(-70f, RightOfWayRelation.FromRight),
                new KeyValuePair<float, RightOfWayRelation>(30f, RightOfWayRelation.Same)
            };
            for (int rotation = 0; rotation < 360; rotation += 15)
            {
                var frame = Quaternion.AngleAxis(rotation, up);
                foreach (var pair in cases)
                {
                    Vector3 a = frame * Vector3.forward;
                    Vector3 b = frame * (Quaternion.AngleAxis(pair.Key, up) * Vector3.forward);
                    Assert.That(RightOfWay.Classify(a, b, up), Is.EqualTo(pair.Value), "rotation " + rotation + " angle " + pair.Key);

                    // Miroir (x -> -x) : la droite et la gauche s'echangent, le reste est inchange.
                    var mirror = new Vector3(-1f, 1f, 1f);
                    var expected = pair.Value == RightOfWayRelation.FromRight ? RightOfWayRelation.FromLeft
                        : pair.Value == RightOfWayRelation.FromLeft ? RightOfWayRelation.FromRight : pair.Value;
                    Assert.That(RightOfWay.Classify(Vector3.Scale(a, mirror), Vector3.Scale(b, mirror), up), Is.EqualTo(expected),
                        "miroir, rotation " + rotation + " angle " + pair.Key);
                }
            }
            // A droite de A (Unity, main gauche) : right = up x forward = +x ; un B qui roule vers -x arrive par la droite.
            Assert.That(RightOfWay.Classify(Vector3.forward, Vector3.left, up), Is.EqualTo(RightOfWayRelation.FromRight));
        }

        [Test]
        public void RightOfWayIsAmbiguousInsideTheDeclaredBandOnly()
        {
            var up = Vector3.up;
            foreach (float boundary in new[] { 45f, -45f, 135f, -135f })
            {
                foreach (float offset in new[] { -9.9f, 0f, 9.9f })
                {
                    Vector3 b = Quaternion.AngleAxis(boundary + offset, up) * Vector3.forward;
                    Assert.That(RightOfWay.Classify(Vector3.forward, b, up), Is.EqualTo(RightOfWayRelation.Ambiguous), boundary + " + " + offset);
                }
                foreach (float offset in new[] { -10.1f, 10.1f })
                {
                    Vector3 b = Quaternion.AngleAxis(boundary + offset, up) * Vector3.forward;
                    Assert.That(RightOfWay.Classify(Vector3.forward, b, up), Is.Not.EqualTo(RightOfWayRelation.Ambiguous), boundary + " + " + offset);
                }
            }
        }

        [Test]
        public void TheCrossroadsRelationIsUnambiguousAndCoversOnlyUncontrolledJunctions()
        {
            var compiled = RoadModelCompiler.Compile(AuthoredSource());
            var table = RightOfWayTable.For(compiled);
            Assert.That(table.AmbiguousCount, Is.EqualTo(0));
            Assert.That(table.Entries.Count, Is.EqualTo(12), "Croix : 4 approches, 12 paires ordonnees ; aucune autre.");
            foreach (var entry in table.Entries)
            {
                Junction junction;
                Assert.That(compiled.TryGetJunction(entry.JunctionId, out junction), Is.True);
                Assert.That(junction.Feature, Is.EqualTo(JunctionFeature.Crossroads));
                RightOfWayRelation back;
                Assert.That(table.TryGet(entry.ControlB, entry.ControlA, out back), Is.True);
                var expectedBack = entry.Relation == RightOfWayRelation.FromRight ? RightOfWayRelation.FromLeft
                    : entry.Relation == RightOfWayRelation.FromLeft ? RightOfWayRelation.FromRight : entry.Relation;
                Assert.That(back, Is.EqualTo(expectedBack), "Antisymetrie de la relation.");
            }
            Assert.That(table.Entries.Count(e => e.Relation == RightOfWayRelation.Opposite), Is.EqualTo(4));
            Assert.That(table.Entries.Count(e => e.Relation == RightOfWayRelation.FromRight), Is.EqualTo(4));
            Assert.That(RightOfWayTable.For(compiled), Is.SameAs(table), "Calculee une seule fois par modele.");
        }

        // ============================================================ faits de modele P2

        [Test]
        public void AuthoredKindsFollowTheRoadIntent()
        {
            var compiled = RoadModelCompiler.Compile(AuthoredSource());
            int tJunctions = 0;
            foreach (var junction in compiled.Junctions)
            {
                var controls = compiled.GetControlsInJunction(junction.Id).Select(id => { CompiledJunctionControl c; compiled.TryGetControl(id, out c); return c; }).ToList();
                foreach (var control in controls)
                {
                    var labels = control.ControlledMovementIds.Select(id => { CompiledJunctionMovement m; compiled.TryGetMovement(id, out m); return m.Label; }).ToList();
                    string approach = labels[0].Substring(labels[0].IndexOf(": ", StringComparison.Ordinal) + 2);
                    switch (junction.Feature)
                    {
                        case JunctionFeature.Crossroads:
                            Assert.That(control.Kind, Is.EqualTo(JunctionControlKind.Uncontrolled));
                            break;
                        case JunctionFeature.Roundabout:
                            Assert.That(control.Kind, Is.EqualTo(approach.StartsWith("Ring_Split_", StringComparison.Ordinal)
                                ? JunctionControlKind.Priority : JunctionControlKind.Yield), approach);
                            Assert.That(control.HasStopLine, Is.False, "Giratoire : repli entree generique.");
                            break;
                        case JunctionFeature.TJunction:
                            bool branch = approach.StartsWith("Junction_FromSouth", StringComparison.Ordinal);
                            Assert.That(control.Kind, Is.EqualTo(branch ? JunctionControlKind.Yield : JunctionControlKind.Priority), approach);
                            Assert.That(control.HasStopLine, Is.EqualTo(branch));
                            // Axe traversant : FromWest et FromEast portent le « tout droit » ; la branche ne porte que des virages.
                            Assert.That(labels.Any(l => l.Contains("(tout droit)")), Is.EqualTo(!branch), approach);
                            break;
                    }
                }
                if (junction.Feature == JunctionFeature.TJunction) tJunctions++;
            }
            Assert.That(tJunctions, Is.EqualTo(4));
        }

        // ============================================================ instance des T (P2) et position des lignes

        [Test]
        public void EachBranchLineLiesAtTheEndOfItsInstanceCornerSidewalks()
        {
            var decisions = AuthoringDecisions.Parse(File.ReadAllText(AuthoredRoadModel.DecisionsPath));
            var lines = decisions.Controls.Where(c => c.HasStopLine).Select(c => c.StopLine).ToList();
            WithMvpRun(scene =>
            {
                int checkedInstances = 0;
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    {
                        if (!t.name.StartsWith("TJunction_", StringComparison.Ordinal) || t.Find("Visual_Greybox_TJunction") == null) continue;
                        // Trottoirs d'angle de la branche : boites des colliders de l'instance (actifs ou non, decision 5.51), en monde.
                        var se = WorldBox(t.Find("Collision/Col_Sidewalk_Corner_SE").GetComponent<BoxCollider>());
                        var sw = WorldBox(t.Find("Collision/Col_Sidewalk_Corner_SW").GetComponent<BoxCollider>());
                        var corners = se;
                        corners.Encapsulate(sw);
                        // Axe de la branche : direction de l'instance vers ses trottoirs d'angle ; fin des trottoirs = face la plus proche du centre.
                        Vector3 center = t.position;
                        bool alongX = Math.Abs(corners.center.x - center.x) > Math.Abs(corners.center.z - center.z);
                        float end = alongX
                            ? (Math.Abs(corners.min.x - center.x) < Math.Abs(corners.max.x - center.x) ? corners.min.x : corners.max.x)
                            : (Math.Abs(corners.min.z - center.z) < Math.Abs(corners.max.z - center.z) ? corners.min.z : corners.max.z);
                        var line = lines.OrderBy(l => Vector2.Distance(new Vector2(0.5f * (l.Start.x + l.End.x), 0.5f * (l.Start.z + l.End.z)),
                            new Vector2(corners.center.x, corners.center.z))).First();
                        // Ouverture entre les deux angles, transversalement a la branche.
                        float gapLow = alongX ? Math.Min(se.max.z, sw.max.z) : Math.Min(se.max.x, sw.max.x);
                        float gapHigh = alongX ? Math.Max(se.min.z, sw.min.z) : Math.Max(se.min.x, sw.min.x);
                        foreach (var point in new[] { line.Start, line.End })
                        {
                            Assert.That(alongX ? point.x : point.z, Is.EqualTo(end).Within(0.01f), t.name + " : ligne a la fin des trottoirs d'angle.");
                            Assert.That(alongX ? point.z : point.x, Is.InRange(gapLow - 0.01f, gapHigh + 0.01f), t.name + " : ligne dans l'ouverture.");
                        }
                        TestContext.WriteLine(t.name + " : angles " + corners + ", fin des trottoirs " + F(end));
                        checkedInstances++;
                    }
                }
                Assert.That(checkedInstances, Is.EqualTo(4));
            });
        }

        /// <summary>
        /// Separation (P5 amende le 2026-10-06), par la fonction du pipeline d'authoring : cas A sans nouveau contact, cas B sans
        /// degradation au-dela de la tolerance ni nouveau recouvrement nominal. Les deux cas sont presents sur MVP_Run.
        /// </summary>
        [Test]
        public void EachBranchLineIntroducesNoNewContactAgainstTheGenericEntryBaseline()
        {
            var lined = RoadModelCompiler.Compile(AuthoredSource());
            var parameters = GateAEvidenceParameters.Declared();
            var bounds = KinematicOffsetBounds.Compute(lined, SweepGraph.FromModel(lined), parameters);
            Assert.That(bounds.Closed, Is.True);
            var result = StopLineSeparation.Measure(lined, parameters, bounds);
            var text = new StringBuilder();
            StopLineSeparation.Render(text, lined, result);
            TestContext.WriteLine(text.ToString());
            Assert.That(result.Failures, Is.Empty, "HALT avant authoring :\n" + string.Join("\n", result.Failures) + "\n" + text);
            Assert.That(result.Rows.Select(r => r.MovementId).Distinct().Count(), Is.EqualTo(8), "Deux mouvements de branche par T.");
            Assert.That(result.Rows.Any(r => r.Preexisting), Is.True, "Cas B present (virages entrant dans la branche).");
            Assert.That(result.Rows.Where(r => !r.Preexisting).All(r => r.LineSlackMeters > 0f), Is.True, "Cas A : marge positive a la ligne.");
        }

        [Test]
        public void ASeparationViolationIsReportedAsAHaltBeforeAuthoring()
        {
            // La ligne C (bord interieur du passage, z local -4) cree un contact avec le tout-droit de la voie proche (cas A).
            var source = AuthoredSource();
            for (int i = 0; i < source.Controls.Length; i++)
            {
                if (!source.Controls[i].HasStopLine) continue;
                // Ligne B a |27,4| sur l'axe de la branche ; C est 0,6 m plus loin dans le carrefour, a |28|.
                var line = source.Controls[i].StopLine;
                line.Start = ToC(line.Start);
                line.End = ToC(line.End);
                source.Controls[i].StopLine = line;
            }
            var lined = RoadModelCompiler.Compile(source);
            var parameters = GateAEvidenceParameters.Declared();
            var result = StopLineSeparation.Measure(lined, parameters, KinematicOffsetBounds.Compute(lined, SweepGraph.FromModel(lined), parameters));
            Assert.That(result.Passed, Is.False);
            Assert.That(result.Failures.Any(f => f.Contains("cas A, nouveau contact")), Is.True, string.Join("\n", result.Failures));
        }

        [Test]
        public void CaseBRejectsNewNominalContactEvenWithinTheMarginLossTolerance()
        {
            var result = MeasureStraightCrossingSeparation(0.04f, 0.02f);
            var row = result.Rows.Single();
            Assert.That(row.Preexisting, Is.True, "Premisse cas B : contact conservatif au repli b = 0.");
            Assert.That(row.EntryNominalMeters, Is.EqualTo(0.02f).Within(0.001f), "Les gabarits nominaux sont initialement separes.");
            Assert.That(row.LineNominalMeters, Is.EqualTo(0f).Within(0.001f), "La ligne cree un contact nominal.");
            Assert.That(row.EntrySlackMeters - row.LineSlackMeters, Is.LessThanOrEqualTo(0.05f),
                "La perte reste admise : seule la garde de nouveau contact doit refuser cette paire.");
            Assert.That(row.Violation, Is.EqualTo("cas B, nouveau recouvrement nominal"));
            Assert.That(result.Passed, Is.False);
        }

        [Test]
        public void CaseBRejectsMoreThanFiveCentimetersOfMarginLossWithoutNewNominalContact()
        {
            var result = MeasureStraightCrossingSeparation(0.10f, 0.20f);
            var row = result.Rows.Single();
            Assert.That(row.Preexisting, Is.True, "Premisse cas B : contact conservatif au repli b = 0.");
            Assert.That(row.EntryNominalMeters, Is.EqualTo(0.20f).Within(0.001f));
            Assert.That(row.LineNominalMeters, Is.EqualTo(0.10f).Within(0.001f), "Les gabarits nominaux restent separes.");
            Assert.That(row.EntrySlackMeters - row.LineSlackMeters, Is.GreaterThan(0.05f),
                "La degradation depasse la limite proprietaire, sans atteindre le contact nominal.");
            Assert.That(row.Violation, Does.StartWith("cas B, marge degradee au-dela de "));
            Assert.That(result.Passed, Is.False);
        }

        [Test]
        public void CaseBAcceptsMarginLossBelowFiveCentimetersWhileNominalFootprintsStaySeparated()
        {
            var result = MeasureStraightCrossingSeparation(0.04f, 0.20f);
            var row = result.Rows.Single();
            Assert.That(row.Preexisting, Is.True);
            Assert.That(row.EntrySlackMeters - row.LineSlackMeters, Is.InRange(0f, 0.05f));
            Assert.That(row.EntryNominalMeters, Is.GreaterThan(0f));
            Assert.That(row.LineNominalMeters, Is.GreaterThan(0f));
            Assert.That(row.Violation, Is.Null);
            Assert.That(result.Failures, Is.Empty);
        }

        /// <summary>
        /// Deux mouvements droits orthogonaux isolent les gardes du cas B. La voie B passe juste au-dela du nez le plus
        /// avance de A au repli generique : les trois poses arretees utilisent centre - 0,05, + 0,05 et + 0,15 m.
        /// </summary>
        private static StopLineSeparation.Result MeasureStraightCrossingSeparation(float lineMeters, float nominalGapMeters)
        {
            var authored = AuthoredSource();
            var profile = authored.ValidationProfile;
            Assert.That(profile.EnvelopeOverlapToleranceMeters, Is.EqualTo(0.05f), "Limite P5 cas B confirmee par le proprietaire.");
            float halfLength = ConflictSweep.HalfLength(profile);
            float entryNose = StopLineSeparation.Center(0f, halfLength) + halfLength + 0.15f;
            float crossingZ = profile.MaxVehicleHalfWidthMeters + entryNose + nominalGapMeters;
            var junctionId = new RoadId(535UL, 1UL);
            var movementA = new RoadId(535UL, 2UL);
            var movementB = new RoadId(535UL, 3UL);
            var source = new RoadModelSource
            {
                ModelId = new RoadId(535UL, 4UL),
                ValidationProfile = profile,
                LocalizationProfile = authored.LocalizationProfile,
                DrivabilityProfile = authored.DrivabilityProfile,
                Sections = new RoadSection[4],
                Corridors = new LaneCorridor[4],
                Junctions = new[] { new Junction { Id = junctionId, Feature = JunctionFeature.Other,
                    Boundary = new RoadBoundsBox { Center = new Vector3(0f, 0f, 10f), Extents = new Vector3(15f, 2f, 15f) } } }
            };
            var starts = new[] { new Vector3(0f, 0f, -30f), new Vector3(0f, 0f, 20f),
                new Vector3(-40f, 0f, crossingZ), new Vector3(10f, 0f, crossingZ) };
            var tangents = new[] { Vector3.forward, Vector3.forward, Vector3.right, Vector3.right };
            for (int i = 0; i < source.Corridors.Length; i++)
            {
                var sectionId = new RoadId(535UL, (ulong)(10 + i));
                source.Sections[i] = new RoadSection { Id = sectionId, DefaultSpeedLimitMetersPerSecond = 10f,
                    DefaultAllowedVehicleClasses = VehicleClassMask.All };
                source.Corridors[i] = new LaneCorridor { Id = new RoadId(535UL, (ulong)(20 + i)), SectionId = sectionId,
                    IsCrossSectionDatum = true, LengthMeters = 30f, Samples = SeparationStraight(starts[i], tangents[i], 30f, profile) };
            }
            source.Movements = new[]
            {
                new JunctionMovement { Id = movementA, JunctionId = junctionId, FromCorridorId = source.Corridors[0].Id,
                    ToCorridorId = source.Corridors[1].Id, LengthMeters = 20f, Samples = SeparationStraight(Vector3.zero, Vector3.forward, 20f, profile) },
                new JunctionMovement { Id = movementB, JunctionId = junctionId, FromCorridorId = source.Corridors[2].Id,
                    ToCorridorId = source.Corridors[3].Id, LengthMeters = 20f,
                    Samples = SeparationStraight(new Vector3(-10f, 0f, crossingZ), Vector3.right, 20f, profile) }
            };
            source.Controls = new[]
            {
                new JunctionControl { Id = new RoadId(535UL, 30UL), JunctionId = junctionId, Kind = JunctionControlKind.Yield,
                    ControlledMovementIds = new[] { movementA }, HasStopLine = true,
                    StopLine = new RoadLineSegment { Start = new Vector3(-2f, 0f, lineMeters), End = new Vector3(2f, 0f, lineMeters) } },
                new JunctionControl { Id = new RoadId(535UL, 31UL), JunctionId = junctionId, Kind = JunctionControlKind.Priority,
                    ControlledMovementIds = new[] { movementB } }
            };
            source.ConflictZones = new[] { new ConflictZone { Id = new RoadId(535UL, 32UL), JunctionId = junctionId,
                Kind = ConflictKind.Crossing, MemberMovementIds = new[] { movementA, movementB }, Volume = source.Junctions[0].Boundary } };
            source.Portals = new[]
            {
                new Portal { Id = new RoadId(535UL, 40UL), CorridorId = source.Corridors[0].Id, Role = PortalRole.Entry,
                    EnvelopeLengthMeters = profile.MaxVehicleLengthMeters, EnvelopeHalfWidthMeters = profile.MaxVehicleHalfWidthMeters },
                new Portal { Id = new RoadId(535UL, 41UL), CorridorId = source.Corridors[2].Id, Role = PortalRole.Entry,
                    EnvelopeLengthMeters = profile.MaxVehicleLengthMeters, EnvelopeHalfWidthMeters = profile.MaxVehicleHalfWidthMeters }
            };
            var compiled = RoadModelCompiler.Compile(source);
            var parameters = GateAEvidenceParameters.Declared();
            var bounds = KinematicOffsetBounds.Compute(compiled, SweepGraph.FromModel(compiled), parameters);
            Assert.That(bounds.Closed, Is.True, string.Join("\n", bounds.Failures));
            Assert.That(bounds.Infeasible, Is.Empty);
            var result = StopLineSeparation.Measure(compiled, parameters, bounds);
            Assert.That(result.Rows, Has.Count.EqualTo(1), string.Join("\n", result.Failures));
            return result;
        }

        private static RoadCurveSample[] SeparationStraight(Vector3 start, Vector3 tangent, float length, RoadModelValidationProfile profile)
        {
            int intervals = Mathf.RoundToInt(length / 0.10f);
            var samples = new RoadCurveSample[intervals + 1];
            for (int i = 0; i <= intervals; i++)
            {
                float s = length * i / intervals;
                samples[i] = new RoadCurveSample { SMeters = s, Position = start + tangent * s, Tangent = tangent, Up = Vector3.up,
                    HalfWidthLeftMeters = profile.MaxVehicleHalfWidthMeters + profile.LateralClearanceMarginMeters,
                    HalfWidthRightMeters = profile.MaxVehicleHalfWidthMeters + profile.LateralClearanceMarginMeters };
            }
            return samples;
        }

        private static Vector3 ToC(Vector3 point)
        {
            if (Math.Abs(Math.Abs(point.x) - 27.4f) < 1e-3f) point.x = Math.Sign(point.x) * 28f;
            if (Math.Abs(Math.Abs(point.z) - 27.4f) < 1e-3f) point.z = Math.Sign(point.z) * 28f;
            return point;
        }

        private static Bounds WorldBox(BoxCollider box)
        {
            var bounds = new Bounds(box.transform.TransformPoint(box.center), Vector3.zero);
            for (int i = 0; i < 8; i++)
            {
                var corner = box.center + Vector3.Scale(box.size, new Vector3((i & 1) == 0 ? -0.5f : 0.5f, (i & 2) == 0 ? -0.5f : 0.5f, (i & 4) == 0 ? -0.5f : 0.5f));
                bounds.Encapsulate(box.transform.TransformPoint(corner));
            }
            return bounds;
        }

        // ============================================================ outils

        /// <summary>Source du modele signe, controles remplaces par les decisions authorees (genres et lignes).</summary>
        private static RoadModelSource AuthoredSource()
        {
            var source = RoadModelDocument.Load(File.ReadAllText(TrafficV2Settings.ModelPath));
            var decisions = AuthoringDecisions.Parse(File.ReadAllText(AuthoredRoadModel.DecisionsPath));
            var byId = decisions.Controls.ToDictionary(c => c.Id);
            for (int i = 0; i < source.Controls.Length; i++)
            {
                var decision = byId[source.Controls[i].Id];
                source.Controls[i].Kind = decision.Kind;
                source.Controls[i].HasStopLine = decision.HasStopLine;
                source.Controls[i].StopLine = decision.StopLine;
            }
            return source;
        }

        private static RoadModelSource Copy(RoadModelSource source)
        {
            var copy = RoadModelDocument.Load(File.ReadAllText(TrafficV2Settings.ModelPath));
            copy.Controls = (JunctionControl[])source.Controls.Clone();
            return copy;
        }

        private static List<RoadCurveSample> Straight(float length)
        {
            var samples = new List<RoadCurveSample>();
            for (int i = 0; i <= 10; i++) samples.Add(Sample(length * i / 10f, new Vector3(0f, 0f, length * i / 10f)));
            return samples;
        }

        private static RoadCurveSample Sample(float s, Vector3 position)
        {
            return new RoadCurveSample { SMeters = s, Position = position, Tangent = Vector3.forward, Up = Vector3.up };
        }

        private static string F(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static void WithMvpRun(Action<Scene> body)
        {
            var alreadyOpen = SceneManager.GetSceneByPath(MigrationReport.ScenePath);
            bool inHierarchy = alreadyOpen.IsValid();
            bool wasOpen = inHierarchy && alreadyOpen.isLoaded;
            if (wasOpen) Assert.That(alreadyOpen.isDirty, Is.False, "MVP_Run est ouvert avec des modifications non sauvegardees.");
            var scene = wasOpen ? alreadyOpen : EditorSceneManager.OpenScene(MigrationReport.ScenePath, OpenSceneMode.Additive);
            try { body(scene); }
            finally { if (!wasOpen) EditorSceneManager.CloseScene(scene, !inHierarchy); }
        }
    }
}
