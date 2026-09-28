using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    [Category("Core")]
    public sealed class Story550CompatibilityGolden
    {
        private const string GoldenFileName = "golden-undeclared.txt";

        private static readonly string GoldenDirectory = Path.GetFullPath(Path.Combine(
            Application.dataPath,
            "..",
            "_bmad-output",
            "implementation-artifacts",
            "v1-regression-5-50"));

        private static readonly string GoldenPath = Path.Combine(GoldenDirectory, GoldenFileName);

        [Test, Explicit("Capture locale obligatoire avant toute modification du payload canonique."), Order(0)]
        public void CaptureGolden()
        {
            Directory.CreateDirectory(GoldenDirectory);
            CompiledCase[] cases = CompileCases();
            File.WriteAllText(GoldenPath, CurrentVersions(cases), new UTF8Encoding(false));
            for (int i = 0; i < cases.Length; i++)
            {
                File.WriteAllBytes(Path.Combine(GoldenDirectory, cases[i].FileName), cases[i].CanonicalBytes);
            }

            Assert.That(File.Exists(GoldenPath), Is.True);
        }

        [Test, Order(1)]
        public void UndeclaredModelsMatchTheGolden()
        {
            Assert.That(File.Exists(GoldenPath), Is.True,
                "Executer explicitement CaptureGolden avant toute modification de production.");
            CompiledCase[] cases = CompileCases();
            Assert.That(CurrentVersions(cases), Is.EqualTo(File.ReadAllText(GoldenPath)));
            for (int i = 0; i < cases.Length; i++)
            {
                string path = Path.Combine(GoldenDirectory, cases[i].FileName);
                Assert.That(File.Exists(path), Is.True, path + " absent.");
                Assert.That(cases[i].CanonicalBytes, Is.EqualTo(File.ReadAllBytes(path)), cases[i].Label);
                AssertCanonicalHashMatchesVersion(cases[i]);
            }
        }

        private static CompiledCase[] CompileCases()
        {
            return new[]
            {
                CompileCase("golden-undeclared", "golden-undeclared.bin", GoldenUndeclaredSource()),
                CompileCase("story-5.25", "golden-story-5-25.bin", FixtureSource(typeof(Story525RoadWorldModelTests))),
                CompileCase("story-5.26", "golden-story-5-26.bin", FixtureSource(typeof(Story526GeometryAndLocalizationTests)))
            };
        }

        private static CompiledCase CompileCase(string label, string fileName, RoadModelSource source)
        {
            CompiledRoadModel model = RoadModelCompiler.Compile(source);
            return new CompiledCase(label, fileName, model.Version, RoadModelCanonicalWriter.GetCanonicalBytes(model).ToArray());
        }

        private static string CurrentVersions(CompiledCase[] cases)
        {
            var lines = new string[cases.Length + 1];
            for (int i = 0; i < cases.Length; i++)
            {
                lines[i] = cases[i].Label + "=" + cases[i].Version;
            }

            lines[lines.Length - 1] = string.Empty;
            return string.Join("\n", lines);
        }

        private static void AssertCanonicalHashMatchesVersion(CompiledCase value)
        {
            byte[] digest;
            using (var sha = SHA256.Create())
            {
                digest = sha.ComputeHash(value.CanonicalBytes);
            }

            Assert.That(ReadBigEndianUInt64(digest, 0), Is.EqualTo(value.Version.High), value.Label + " high");
            Assert.That(ReadBigEndianUInt64(digest, 8), Is.EqualTo(value.Version.Low), value.Label + " low");
        }

        private static ulong ReadBigEndianUInt64(byte[] bytes, int offset)
        {
            ulong value = 0UL;
            for (int i = 0; i < 8; i++)
            {
                value = (value << 8) | bytes[offset + i];
            }

            return value;
        }

        private static RoadModelSource FixtureSource(Type fixtureType)
        {
            MethodInfo method = fixtureType.GetMethod("BuildModel", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, fixtureType.FullName + ".BuildModel introuvable.");
            return (RoadModelSource)method.Invoke(null, null);
        }

        private static RoadModelSource GoldenUndeclaredSource()
        {
            var source = new RoadModelSource
            {
                ModelId = new RoadId(0x5500000000000001UL, 0x5500000000000002UL),
                Label = "reference non declaree 5.50",
                ValidationProfile = ValidationProfile(),
                LocalizationProfile = LocalizationProfile(),
                Sections = new[]
                {
                    Section(new RoadId(0x5500000000000010UL, 0x5500000000000011UL), "ligne droite"),
                    Section(new RoadId(0x5500000000000012UL, 0x5500000000000013UL), "arc")
                }
            };

            RoadId straightId = new RoadId(0x5500000000000020UL, 0x5500000000000021UL);
            RoadId arcId = new RoadId(0x5500000000000022UL, 0x5500000000000023UL);
            RoadId junctionId = new RoadId(0x5500000000000030UL, 0x5500000000000031UL);
            RoadId movementId = new RoadId(0x5500000000000040UL, 0x5500000000000041UL);

            source.Corridors = new[]
            {
                Corridor(
                    straightId,
                    source.Sections[0].Id,
                    10f,
                    new[]
                    {
                        Sample(0f, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 1f), 0f),
                        Sample(5f, new Vector3(0f, 0f, 5f), new Vector3(0f, 0f, 1f), 0f),
                        Sample(10f, new Vector3(0f, 0f, 10f), new Vector3(0f, 0f, 1f), 0f)
                    }),
                Corridor(
                    arcId,
                    source.Sections[1].Id,
                    15.307337f,
                    new[]
                    {
                        Sample(0f, new Vector3(10f, 0f, 20f), new Vector3(1f, 0f, 0f), 0.1f),
                        Sample(7.6536684f, new Vector3(17.071068f, 0f, 17.071068f), new Vector3(0.7071068f, 0f, -0.7071068f), 0.1f),
                        Sample(15.307337f, new Vector3(20f, 0f, 10f), new Vector3(0f, 0f, -1f), 0.1f)
                    })
            };

            source.Junctions = new[]
            {
                new Junction
                {
                    Id = junctionId,
                    Label = "quart de tour",
                    Feature = JunctionFeature.Crossroads,
                    Boundary = new RoadBoundsBox
                    {
                        Center = new Vector3(5f, 0f, 15f),
                        Extents = new Vector3(6f, 2f, 6f)
                    }
                }
            };

            source.Movements = new[]
            {
                new JunctionMovement
                {
                    Id = movementId,
                    Label = "mouvement",
                    JunctionId = junctionId,
                    FromCorridorId = straightId,
                    ToCorridorId = arcId,
                    LengthMeters = 15.307337f,
                    RoutePreferenceWeight = 1f,
                    Samples = new[]
                    {
                        Sample(0f, new Vector3(0f, 0f, 10f), new Vector3(0f, 0f, 1f), 0.1f),
                        Sample(7.6536684f, new Vector3(2.9289322f, 0f, 17.071068f), new Vector3(0.7071068f, 0f, 0.7071068f), 0.1f),
                        Sample(15.307337f, new Vector3(10f, 0f, 20f), new Vector3(1f, 0f, 0f), 0.1f)
                    }
                }
            };

            source.Controls = new[]
            {
                new JunctionControl
                {
                    Id = new RoadId(0x5500000000000050UL, 0x5500000000000051UL),
                    JunctionId = junctionId,
                    Kind = JunctionControlKind.Uncontrolled,
                    ControlledMovementIds = new[] { movementId }
                }
            };

            source.Portals = new[]
            {
                Portal(new RoadId(0x5500000000000060UL, 0x5500000000000061UL), straightId, PortalRole.Entry, 0f),
                Portal(new RoadId(0x5500000000000062UL, 0x5500000000000063UL), arcId, PortalRole.Exit, 15.307337f)
            };
            return source;
        }

        private static RoadSection Section(RoadId id, string label)
        {
            return new RoadSection
            {
                Id = id,
                Label = label,
                RoadClass = RoadClass.Local,
                Surface = RoadSurface.Asphalt,
                DefaultSpeedLimitMetersPerSecond = 13.9f,
                DefaultAllowedVehicleClasses = VehicleClassMask.Car
            };
        }

        private static LaneCorridor Corridor(RoadId id, RoadId sectionId, float length, RoadCurveSample[] samples)
        {
            return new LaneCorridor
            {
                Id = id,
                SectionId = sectionId,
                LengthMeters = length,
                Samples = samples,
                LateralOrder = 0,
                IsCrossSectionDatum = true
            };
        }

        private static Portal Portal(RoadId id, RoadId corridorId, PortalRole role, float s)
        {
            return new Portal
            {
                Id = id,
                CorridorId = corridorId,
                Role = role,
                SMeters = s,
                EnvelopeLengthMeters = 6f,
                EnvelopeHalfWidthMeters = 2f
            };
        }

        private static RoadCurveSample Sample(float s, Vector3 position, Vector3 tangent, float curvature)
        {
            return new RoadCurveSample
            {
                SMeters = s,
                Position = position,
                Tangent = tangent,
                Up = Vector3.up,
                CurvaturePerMeter = curvature,
                HalfWidthLeftMeters = 2f,
                HalfWidthRightMeters = 2f
            };
        }

        private static RoadModelValidationProfile ValidationProfile()
        {
            return new RoadModelValidationProfile
            {
                MaxVehicleHalfWidthMeters = 1.03f,
                MaxVehicleLengthMeters = 4.5f,
                LateralClearanceMarginMeters = 0.25f,
                SeamGapToleranceMeters = 0.05f,
                SeamTangentToleranceDegrees = 5f,
                LengthToleranceMeters = 0.05f,
                EnvelopeOverlapToleranceMeters = 0.05f,
                GroundingMaxOffAxisDegrees = 45f
            };
        }

        private static RoadLocalizationProfile LocalizationProfile()
        {
            return new RoadLocalizationProfile
            {
                ScoreBandMeters = 0.15f,
                HysteresisMeters = 0.1f,
                AcceptanceDistanceMeters = 2.5f,
                WrongWayHeadingDegrees = 90f
            };
        }

        private sealed class CompiledCase
        {
            public CompiledCase(string label, string fileName, RoadModelVersion version, byte[] canonicalBytes)
            {
                Label = label;
                FileName = fileName;
                Version = version;
                CanonicalBytes = canonicalBytes;
            }

            public string Label { get; private set; }

            public string FileName { get; private set; }

            public RoadModelVersion Version { get; private set; }

            public byte[] CanonicalBytes { get; private set; }
        }
    }
}
