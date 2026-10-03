using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace RoadRage.Tests.PlayMode
{
    /// <summary>Regressions R2/R4 : calculs, alignement des mesures et porte D13 du rapport reel.</summary>
    [Category("Story533")]
    public sealed class Story533PerformancePublicationTests
    {
        [Test]
        public void ActualFrameAssemblyAttachesPreviousDurationAndRecorderSamplesToTheirPendingEpochs()
        {
            var assembly = new Story533PerformanceDiagnosticPlayModeTests.FrameAssembly();
            assembly.Begin(100UL, 1d);
            assembly.PhysicalStep();
            assembly.Update(101UL, 1.010, 800d, sample => Assert.Fail("premiere frame partielle non lisible"));
            assembly.PhysicalStep(); assembly.PhysicalStep();
            assembly.Update(103UL, 1.022, 900d, sample => Assert.Fail("premiere frame partielle non lisible"));
            Assert.That(assembly.Frames[0].WallMilliseconds, Is.EqualTo(10d).Within(1e-9));
            Assert.That(double.IsNaN(assembly.Frames[0].Values[0]), Is.True);

            assembly.PhysicalStep();
            assembly.Update(104UL, 1.036, 27.5, sample => { sample.Values[0] = 22000000d; sample.Counts[0] = 2; });
            assembly.PhysicalStep();
            assembly.Update(105UL, 1.050, 31.25, sample => { sample.Values[0] = 44000000d; sample.Counts[0] = 1; });
            var first = assembly.Frames[1];
            Assert.That(first.FirstHostStep, Is.EqualTo(102UL));
            Assert.That(first.LastHostStep, Is.EqualTo(103UL));
            Assert.That(first.FixedSteps, Is.EqualTo(2));
            Assert.That(first.WallMilliseconds, Is.EqualTo(27.5));
            Assert.That(first.Values[0], Is.EqualTo(22000000d));
            Assert.That(first.Counts[0], Is.EqualTo(2));
            var second = assembly.Frames[2];
            Assert.That(second.FirstHostStep, Is.EqualTo(104UL));
            Assert.That(second.FixedSteps, Is.EqualTo(1));
            Assert.That(second.WallMilliseconds, Is.EqualTo(31.25));
            Assert.That(second.Values[0], Is.EqualTo(44000000d));
        }

        [Test]
        public void ActualTerminalFlushKeepsThreeFinalPhysicalStepsAndRejectsN4WithoutInventingPendingDuration()
        {
            var assembly = new Story533PerformanceDiagnosticPlayModeTests.FrameAssembly();
            assembly.Begin(10UL, 2d);
            assembly.PhysicalStep();
            assembly.Update(11UL, 2.010, 500d, sample => Assert.Fail("pas de frame precedente"));
            assembly.PhysicalStep();
            assembly.Update(12UL, 2.030, 600d, sample => Assert.Fail("bord partiel"));
            assembly.PhysicalStep(); assembly.PhysicalStep(); assembly.PhysicalStep();
            assembly.End(15UL, 2.080);
            var pending = assembly.Frames[1];
            Assert.That(pending.Partial, Is.True);
            Assert.That(double.IsNaN(pending.WallMilliseconds), Is.True, "pas de duree moteur alignee disponible");
            Assert.That(double.IsNaN(pending.Values[0]), Is.True);
            var final = assembly.Frames[2];
            Assert.That(final.Partial, Is.True);
            Assert.That(final.FirstHostStep, Is.EqualTo(13UL));
            Assert.That(final.LastHostStep, Is.EqualTo(15UL));
            Assert.That(final.FixedSteps, Is.EqualTo(3));
            Assert.That(final.WallMilliseconds, Is.EqualTo(50d).Within(1e-9));
            Assert.That(assembly.Frames.All(Story533PerformanceDiagnosticPlayModeTests.EpochsAligned), Is.True);
            var failures = Story533PerformanceDiagnosticPlayModeTests.D13Failures(4, 10, 2d,
                assembly.Frames.Count, assembly.Frames.Max(f => f.FixedSteps),
                assembly.Frames.All(Story533PerformanceDiagnosticPlayModeTests.EpochsAligned));
            Assert.That(failures, Has.Count.EqualTo(1));
            StringAssert.Contains("deux FixedUpdate", failures[0]);
        }

        [Test]
        public void RecorderMeansUseOnlyTheirOwnMeasuredFramesAndPublishUndefinedCoverageAsMissing()
        {
            var frames = new[]
            {
                new Story533PerformanceDiagnosticPlayModeTests.FrameSample { FixedSteps = 4, Partial = true, Values = new[] { 99000000d, double.NaN } },
                new Story533PerformanceDiagnosticPlayModeTests.FrameSample { FixedSteps = 2, Values = new[] { 6000000d, double.NaN } },
                new Story533PerformanceDiagnosticPlayModeTests.FrameSample { FixedSteps = 3, Values = new[] { double.NaN, 9000000d } },
                new Story533PerformanceDiagnosticPlayModeTests.FrameSample { FixedSteps = 1, Values = new[] { 4000000d, 3000000d } }
            };
            int measuredFrames, measuredSteps;
            Assert.That(Story533PerformanceDiagnosticPlayModeTests.RecorderMeanPerStep(frames, 0, out measuredFrames, out measuredSteps),
                Is.EqualTo(10d / 3d));
            Assert.That(measuredFrames, Is.EqualTo(2));
            Assert.That(measuredSteps, Is.EqualTo(3));
            Assert.That(Story533PerformanceDiagnosticPlayModeTests.RecorderMeanPerStep(frames, 1, out measuredFrames, out measuredSteps),
                Is.EqualTo(3d));
            Assert.That(measuredSteps, Is.EqualTo(4));
            Assert.That(double.IsNaN(Story533PerformanceDiagnosticPlayModeTests.RecorderMeanPerStep(
                new[] { frames[0] }, 0, out measuredFrames, out measuredSteps)), Is.True);
            Assert.That(measuredFrames, Is.Zero);
            var noSteps = new Story533PerformanceDiagnosticPlayModeTests.FrameSample { FixedSteps = 0, Values = new[] { 0d } };
            Assert.That(double.IsNaN(Story533PerformanceDiagnosticPlayModeTests.RecorderMeanPerStep(
                new[] { noSteps }, 0, out measuredFrames, out measuredSteps)), Is.True);
            Assert.That(measuredSteps, Is.Zero);
        }

        [Test]
        public void FrameIntervalsKeepActualHostEpochsAndPhysicalCountsWithoutPredictingSpawnerOrder()
        {
            var frame = Story533PerformanceDiagnosticPlayModeTests.FrameInterval(764UL, 767UL, 3, 81.669);
            Assert.That(frame.FirstHostStep, Is.EqualTo(765UL));
            Assert.That(frame.LastHostStep, Is.EqualTo(767UL));
            Assert.That(frame.FixedSteps, Is.EqualTo(3));
            Assert.That(frame.WallMilliseconds, Is.EqualTo(81.669));
            Assert.That(Story533PerformanceDiagnosticPlayModeTests.EpochsAligned(frame), Is.True);
            var empty = Story533PerformanceDiagnosticPlayModeTests.FrameInterval(767UL, 767UL, 0, 6.0);
            Assert.That(empty.FirstHostStep, Is.Zero);
            Assert.That(empty.LastHostStep, Is.Zero);
            Assert.That(Story533PerformanceDiagnosticPlayModeTests.EpochsAligned(empty), Is.True);
            var mismatch = Story533PerformanceDiagnosticPlayModeTests.FrameInterval(767UL, 768UL, 2, 20.0);
            Assert.That(Story533PerformanceDiagnosticPlayModeTests.EpochsAligned(mismatch), Is.False);
        }

        [Test]
        public void D13RejectsThreePhysicalStepsEvenWithFastFullPopulationCosts()
        {
            var failures = Story533PerformanceDiagnosticPlayModeTests.D13Failures(4, 10, 2.0, 100, 3);
            Assert.That(failures, Has.Count.EqualTo(1));
            StringAssert.Contains("deux FixedUpdate", failures[0]);
            Assert.That(Story533PerformanceDiagnosticPlayModeTests.D13Failures(4, 10, 4.0, 100, 2), Is.Empty);
        }

        [Test]
        public void D13RequiresNonemptyFullPopulationMeasurementsAndAlignedPhysicalEpochs()
        {
            Assert.That(Story533PerformanceDiagnosticPlayModeTests.D13Failures(8, 0, double.NaN, 100, 2), Is.Not.Empty);
            Assert.That(Story533PerformanceDiagnosticPlayModeTests.D13Failures(8, 10, double.PositiveInfinity, 100, 2), Is.Not.Empty);
            Assert.That(Story533PerformanceDiagnosticPlayModeTests.D13Failures(4, 10, 2.0, 0, 0), Is.Not.Empty);
            Assert.That(Story533PerformanceDiagnosticPlayModeTests.D13Failures(4, 10, 2.0, 100, 2, false), Is.Not.Empty);
            Assert.That(Story533PerformanceDiagnosticPlayModeTests.D13Failures(8, 10, 8.0, 100, 2), Is.Empty);
            Assert.That(Story533PerformanceDiagnosticPlayModeTests.D13Failures(8, 10, 8.01, 100, 2), Is.Not.Empty);
            var ten = Story533PerformanceDiagnosticPlayModeTests.D13Failures(8, 10, 10.0, 100, 2);
            Assert.That(ten, Has.Count.EqualTo(2));
            StringAssert.Contains("sous 10 ms", ten[1]);
        }

        [Test]
        public void ComparisonNormalizesEachMeasuredStepByItsActualVehicleCount()
        {
            var result = new Story533PerformanceDiagnosticPlayModeTests.PopulationResult { Population = 8 };
            Story533PerformanceDiagnosticPlayModeTests.RecordStepMetric(result, "pas Traffic V2 total",
                new[] { 2.0, 4.0 }, new[] { 1, 4 });
            // Moyenne par pas = 3 ; moyenne par vehicule = (2/1 + 4/4)/2 = 1,5, jamais 3/8.
            Assert.That(result.PerStep["pas Traffic V2 total"], Is.EqualTo(3.0));
            Assert.That(result.PerVehicle["pas Traffic V2 total"], Is.EqualTo(1.5));
            var text = Story533PerformanceDiagnosticPlayModeTests.ComparisonText(
                new Dictionary<int, Story533PerformanceDiagnosticPlayModeTests.PopulationResult> { { 8, result } }, "test");
            StringAssert.Contains("| pas Traffic V2 total | 3 | 1.5 |", text);
        }

        [Test]
        public void ComparisonPreservesAlreadyNormalizedMetricsAndLeavesGlobalRecordersUnnormalized()
        {
            var result = new Story533PerformanceDiagnosticPlayModeTests.PopulationResult { Population = 8 };
            result.PerStep["pas Traffic V2 par vehicule"] = result.PerVehicle["pas Traffic V2 par vehicule"] = 0.747;
            result.PerStep["perception par vehicule"] = result.PerVehicle["perception par vehicule"] = 0.099;
            result.PerStep["[recorder] Physics.Simulate"] = 0.074;
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                var text = Story533PerformanceDiagnosticPlayModeTests.ComparisonText(
                    new Dictionary<int, Story533PerformanceDiagnosticPlayModeTests.PopulationResult> { { 8, result } }, "test");
                StringAssert.Contains("| pas Traffic V2 par vehicule | 0.747 | 0.747 |", text);
                StringAssert.Contains("| perception par vehicule | 0.099 | 0.099 |", text);
                StringAssert.Contains("| [recorder] Physics.Simulate | 0.074 | - |", text);
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Test]
        public void ComparisonPublishesMissingMeasurementsAsMissing()
        {
            var result = new Story533PerformanceDiagnosticPlayModeTests.PopulationResult { Population = 8 };
            Story533PerformanceDiagnosticPlayModeTests.RecordStepMetric(result, "pas Traffic V2 total",
                Array.Empty<double>(), Array.Empty<int>());
            var text = Story533PerformanceDiagnosticPlayModeTests.ComparisonText(
                new Dictionary<int, Story533PerformanceDiagnosticPlayModeTests.PopulationResult> { { 8, result } }, "test");
            StringAssert.Contains("| pas Traffic V2 total | - | - |", text);
        }
    }
}
