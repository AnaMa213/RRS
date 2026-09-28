using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Vehicles;
using RoadRage.Features.Vehicles.Traffic;
using RoadRage.Features.Vehicles.Traffic.Migration;
using UnityEditor;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    [Category("Core")]
    public sealed class Story550DrivabilityTests
    {
        [Test]
        public void HistoricalTableCoversThe76CommittedDecisionsExactly()
        {
            string tableText = File.ReadAllText(AuthoredRoadModel.FullPath(PairGeometryFingerprint.HistoricalPath));
            HistoricalPairFingerprintTable table = HistoricalPairFingerprintTable.Parse(tableText);
            AuthoringDecisions decisions = AuthoringDecisions.Parse(File.ReadAllText(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath)));

            Assert.That(table.Pairs, Has.Length.EqualTo(76));
            var active = new HashSet<string>(decisions.Conflicts.Select(delegate (ConflictDecision decision)
            {
                return AuthoredRoadModel.PairKey(decision.MovementKeyA, decision.MovementKeyB);
            }), StringComparer.Ordinal);
            var superseding = new HashSet<string>(decisions.Conflicts
                .Where(delegate (ConflictDecision decision) { return !string.IsNullOrEmpty(decision.SupersedesDecisionRevisionId); })
                .Select(delegate (ConflictDecision decision)
                {
                    return AuthoredRoadModel.PairKey(decision.MovementKeyA, decision.MovementKeyB);
                }), StringComparer.Ordinal);
            var actual = new HashSet<string>(table.Pairs.Select(delegate (HistoricalPairFingerprintRecord record)
            {
                return AuthoredRoadModel.PairKey(record.MovementKeyA, record.MovementKeyB);
            }), StringComparer.Ordinal);
            Assert.That(actual.Count, Is.EqualTo(table.Pairs.Length), "Aucune paire historique en double.");
            Assert.That(actual.IsSubsetOf(active), Is.True, "Chaque paire historique garde une decision active qui la remplace.");
            Assert.That(superseding.SetEquals(actual), Is.True,
                "Les seules revisions actives qui remplacent l'historique sont les 76 paires de la table.");
        }

        [Test]
        public void HistoricalTableIsReferencedOnlyByEditorMigrationProductionCode()
        {
            string features = AuthoredRoadModel.FullPath("Assets/RoadRage/Features");
            string marker = nameof(HistoricalPairFingerprintTable);
            string[] offenders = Directory.GetFiles(features, "*.cs", SearchOption.AllDirectories)
                .Where(delegate (string path)
                {
                    string normalized = path.Replace('\\', '/');
                    return !normalized.Contains("/Traffic/Migration/") && File.ReadAllText(path).Contains(marker);
                })
                .ToArray();
            Assert.That(offenders, Is.Empty);
        }

        // ================================================================== validation de conduisibilite (Story 5.50)

        [Test]
        public void TheDerivedAdmissionBoundaryRefusesAndAdmits()
        {
            DrivabilityProfile profile = V1RoadModelImporter.DrivabilityProfile();

            Assert.That(RoadModelCompiler.AdmissionRadiusMeters(profile), Is.EqualTo(4.0344f).Within(0.01f),
                "Le rayon minimal admis est celui du contrat 5.50.");
            Assert.That(RoadModelCompiler.SteeringSpeedCeilingMetersPerSecond(profile, 1f / 4.0064f),
                Is.LessThan(profile.SteeringInactiveBelowMetersPerSecond),
                "Sous le rayon d'admission, la direction ne tient pas la vitesse minimale : refus.");
            Assert.That(RoadModelCompiler.SteeringSpeedCeilingMetersPerSecond(profile, 0f), Is.EqualTo(float.PositiveInfinity),
                "Une droite n'impose aucun plafond.");
            Assert.That(RoadModelCompiler.SteeringSpeedCeilingMetersPerSecond(profile, 1f / 20f), Is.EqualTo(float.PositiveInfinity),
                "Au-dela du rayon pleinement braque, aucun plafond non plus.");
        }

        [Test]
        public void ADeclaredModelRefusesEveryDrivabilityRuleBreak()
        {
            Assert.DoesNotThrow(delegate { RoadModelCompiler.Compile(SyntheticSource()); },
                "Controle : le modele synthetique declare compile.");

            RoadModelSource admission = SyntheticSource();
            MutateSecondSample(admission, delegate (RoadCurveSample[] samples) { samples[1].CurvaturePerMeter = 1f / 3.5f; });
            RoadModelCompilationException refused = CompileRefused(admission, RoadModelValidationCode.DrivabilityAdmissionFailed);
            string message = refused.Issues.First(i => i.Code == RoadModelValidationCode.DrivabilityAdmissionFailed).Message;
            Assert.That(message, Does.Contain("admission"), "Le refus nomme la regle.");
            Assert.That(message, Does.Contain("echantillon"), "Le refus nomme l'echantillon.");

            RoadModelSource up = SyntheticSource();
            MutateSecondSample(up, delegate (RoadCurveSample[] samples) { samples[1].Up = Quaternion.AngleAxis(30f, samples[1].Tangent.normalized) * samples[1].Up; });
            CompileRefused(up, RoadModelValidationCode.DrivabilityRoadUpVaries);

            RoadModelSource tangent = SyntheticSource();
            MutateSecondSample(tangent, delegate (RoadCurveSample[] samples) { samples[1].Tangent = Quaternion.AngleAxis(30f, samples[1].Up.normalized) * samples[1].Tangent; });
            CompileRefused(tangent, RoadModelValidationCode.DrivabilityTangentCurvatureMismatch);

            RoadModelSource chord = SyntheticSource();
            MutateSecondSample(chord, delegate (RoadCurveSample[] samples) { samples[1].Position += Vector3.Cross(samples[1].Up, samples[1].Tangent).normalized * 1f; });
            CompileRefused(chord, RoadModelValidationCode.DrivabilityChordHeadingInconsistent);

            RoadModelSource fold = SyntheticSource();
            MutateSecondSample(fold, delegate (RoadCurveSample[] samples)
            {
                samples[1].CurvaturePerMeter = 1f / 4.5f;
                samples[1].HalfWidthLeftMeters = 4.6f;
                samples[1].HalfWidthRightMeters = 4.6f;
            });
            // Le repli se manifeste ici par F2 (demi-largeur interieure x courbure >= 1), la regle
            // nommee par la matrice 5.50 ; F1 partage le meme seuil sur une geometrie coherente.
            CompileRefused(fold, RoadModelValidationCode.DrivabilityInnerRadiusFold);
        }

        [Test]
        public void TheDocumentRefusesAnUndeclaredProfileAndAFormatOneText()
        {
            var provenance = new RoadModelProvenance
            {
                SourceHash = "a",
                LineageHash = "b",
                DecisionsHash = "c",
                ImporterVersion = V1RoadModelImporter.ImporterVersion,
                PipelineVersion = AuthoredRoadModel.PipelineVersion
            };
            string text = RoadModelDocument.Serialize(SyntheticSource(), provenance);

            RoadModelSource undeclared = SyntheticSource();
            undeclared.DrivabilityProfile = new DrivabilityProfile();
            Assert.Throws<FormatException>(() => RoadModelDocument.Serialize(undeclared, provenance),
                "Un document sans profil declare ne peut pas etre produit.");

            string firstFormat = text.Replace("{\"Format\":2,", "{\"Format\":1,");
            Assert.That(firstFormat, Is.Not.EqualTo(text), "Le document porte bien la marque de format attendue.");
            Assert.Throws<FormatException>(() => RoadModelDocument.Load(firstFormat),
                "Un document format 1 est refuse par le chemin format 2, jamais re-serialise.");
        }

        [Test]
        public void TheDeclaredDrivabilityProfileCopiesTheVehicleDefaults()
        {
            const string path = "Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset";
            VehicleProfileDef def = AssetDatabase.LoadAssetAtPath<VehicleProfileDef>(path);
            Assert.That(def, Is.Not.Null, "Def par defaut introuvable : " + path);
            VehicleProfile vehicle = def.Profile;
            DrivabilityProfile profile = V1RoadModelImporter.DrivabilityProfile();

            Assert.That(profile.Declared, Is.True);
            Assert.That(profile.LowSpeedLockDegrees, Is.EqualTo(vehicle.MaxSteerAngleDegrees),
                "L'autorite de braquage du modele est celle du vehicule.");
            Assert.That(profile.HighSpeedLockDegrees, Is.EqualTo(vehicle.HighSpeedSteerAngleDegrees));
            Assert.That(profile.FullReductionSpeedMetersPerSecond, Is.EqualTo(vehicle.SteerFullReductionSpeed));
            Assert.That(profile.SteeringInactiveBelowMetersPerSecond, Is.EqualTo(vehicle.MinimumDirectionSpeed));
            Assert.That(profile.WheelbaseMeters, Is.EqualTo(3.10f).Within(1e-3f));
            Assert.That(profile.WheelbaseMeters, Is.EqualTo(2f * profile.ReferencePointAheadRearAxleMeters).Within(1e-3f),
                "L'empattement et le point de reference viennent des memes essieux (z +/-1,55).");
        }

        // ================================================================== outils

        /// <summary>
        /// Modele synthetique 5.25 (par reflexion : la fixture 5.25 reste intacte) au profil declare.
        /// Le demi-tour M3 (rayon 2,5 m) n'est pas admissible : la copie 5.28 le retire par reflexion
        /// pour la meme raison ; les mutations de ce fichier portent sur ce meme modele nettoye.
        /// </summary>
        private static RoadModelSource SyntheticSource()
        {
            var build = typeof(Story525RoadWorldModelTests).GetMethod("BuildModel", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(build, Is.Not.Null);
            var source = (RoadModelSource)build.Invoke(null, null);
            source.DrivabilityProfile = V1RoadModelImporter.DrivabilityProfile();

            var m2 = source.Movements.Single(m => m.Label == "M2");
            var m3 = source.Movements.Single(m => m.Label == "M3");
            var m4 = source.Movements.Single(m => m.Label == "M4");
            source.Movements = source.Movements.Where(m => m.Id != m3.Id).ToArray();
            for (int i = 0; i < source.Controls.Length; i++)
            {
                source.Controls[i].ControlledMovementIds = source.Controls[i].ControlledMovementIds.Where(id => id != m3.Id).ToArray();
            }

            source.ConflictZones[0].MemberMovementIds = new[] { m2.Id, m4.Id };
            source.SignalPlans[0].Groups[1].MemberMovementIds = new[] { m4.Id };
            return source;
        }

        private static void MutateSecondSample(RoadModelSource source, Action<RoadCurveSample[]> mutate)
        {
            RoadCurveSample[] samples = source.Corridors[0].Samples;
            Assert.That(samples.Length, Is.GreaterThan(1), "Premisse : la premiere courbe porte au moins deux echantillons.");
            // Le mutateur recoit le tableau : RoadCurveSample est une valeur, muter une copie recue
            // par un parametre de delegate perdrait l'ecriture.
            mutate(samples);
        }

        private static RoadModelCompilationException CompileRefused(RoadModelSource source, RoadModelValidationCode expected, params RoadModelValidationCode[] alsoExpected)
        {
            var exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(source); });
            Assert.That(exception.Issues.Any(i => i.Code == expected), Is.True,
                "Code attendu " + expected + " absent de :\n" + string.Join("\n", exception.Issues.Select(i => i.ToString()).ToArray()));
            foreach (RoadModelValidationCode code in alsoExpected)
            {
                Assert.That(exception.Issues.Any(i => i.Code == code), Is.True,
                    "Code attendu " + code + " absent de :\n" + string.Join("\n", exception.Issues.Select(i => i.ToString()).ToArray()));
            }

            return exception;
        }
    }
}
