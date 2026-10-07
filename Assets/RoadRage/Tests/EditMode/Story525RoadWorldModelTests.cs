using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Vehicles.Traffic;
using UnityEngine;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 5.25 : enregistrements du Road World Model, compilateur et versionnage deterministe.
    /// Couvre chaque ligne de la matrice d'E/S de la spec sur des modeles synthetiques ecrits a la
    /// main. Deterministe, sans scene, sans asset, sans Netcode.
    /// </summary>
    [Category("Core")]
    public sealed class Story525RoadWorldModelTests
    {
        private const string FeaturesRootPath = "Assets/RoadRage";

        // ------------------------------------------------------------------ identifiants fixes
        // Deterministes : deux constructions du meme modele partagent les memes identites, ce qui
        // est la condition pour comparer leurs versions.
        private static RoadId Id(int index)
        {
            return new RoadId(0xA5A5A5A500000000UL | (uint)index, 0x5A5A5A5A00000000UL | (uint)index);
        }

        private static readonly RoadId ModelId = Id(1);
        private static readonly RoadId SectionS1 = Id(2);
        private static readonly RoadId CorridorA1 = Id(3);
        private static readonly RoadId CorridorA1B = Id(4);
        private static readonly RoadId CorridorA2 = Id(5);
        private static readonly RoadId CorridorD1 = Id(6);
        private static readonly RoadId CorridorD2 = Id(7);
        private static readonly RoadId CorridorE1 = Id(8);
        private static readonly RoadId JunctionJ = Id(9);
        private static readonly RoadId MovementM1 = Id(10);
        private static readonly RoadId MovementM2 = Id(11);
        private static readonly RoadId MovementM3 = Id(12);
        private static readonly RoadId MovementM4 = Id(13);
        private static readonly RoadId ControlC1 = Id(14);
        private static readonly RoadId ControlC2 = Id(15);
        private static readonly RoadId ConflictZ1 = Id(16);
        private static readonly RoadId PlanP1 = Id(17);
        private static readonly RoadId GroupG1 = Id(18);
        private static readonly RoadId GroupG2 = Id(19);
        private static readonly RoadId PhasePh1 = Id(20);
        private static readonly RoadId PhasePh2 = Id(21);
        private static readonly RoadId ConnectionK1 = Id(22);
        private static readonly RoadId AdjacencyAd1 = Id(23);
        private static readonly RoadId PortalEntry = Id(24);
        private static readonly RoadId PortalExit = Id(25);
        private static readonly RoadId RetiredId = Id(90);
        private static readonly RoadId UnknownId = Id(999);

        // Enregistrements ajoutes a la volee par certains cas d'echec.
        private static readonly RoadId JunctionJ2 = Id(200);
        private static readonly RoadId SectionS2 = Id(201);
        private static readonly RoadId PlanP2 = Id(202);
        private static readonly RoadId PhasePh3 = Id(203);
        private static readonly RoadId SectionS3 = Id(204);

        // Index de tableau du modele synthetique, stables par construction (voir BuildModel).
        private const int CorridorA1Index = 0;
        private const int CorridorA1BIndex = 1;
        private const int CorridorA2Index = 2;
        private const int CorridorD1Index = 3;
        private const int CorridorD2Index = 4;
        private const int CorridorE1Index = 5;
        private const int MovementM1Index = 0;
        private const int ControlC1Index = 0;
        private const int ControlC2Index = 1;
        private const int PortalEntryIndex = 0;

        // ================================================================== modele synthetique

        /// <summary>
        /// Carrefour signalise complet : une section, six corridors diriges, une continuation, une
        /// adjacence legale, quatre mouvements, deux liaisons de controle couvrant chacune deux
        /// mouvements, une zone de conflit revue, un plan a deux phases, un portail d'entree et un
        /// portail de sortie, plus une lignee d'import avec tombstone et remap.
        ///
        /// Geometrie coherente depuis la 5.26 (la validation geometrique fait partie de Compile) :
        /// bandes paralleles espacees de 5 m, demi-largeurs 2 m. A1 (datum, x=0), A1b (x=5), A2
        /// (x=10) vers +z sur z in [0,20] ; D1 (x=15, z 20->10) puis E1 (x=15, z 10->0) en
        /// continuation ; D2 (x=20, z 20->0). Les mouvements sont des demi-tours analytiques a z=20.
        /// </summary>
        private static RoadModelSource BuildModel()
        {
            var source = new RoadModelSource();
            source.ModelId = ModelId;
            source.Label = "modele synthetique 5.25";
            source.ValidationProfile = Profile();
            source.LocalizationProfile = LocalizationProfile();

            source.Sections = new[] { Section(SectionS1, "S1", 13.9f) };

            source.Corridors = new[]
            {
                // Coupe transversale AD-48 : ordres contigus 0..5 croissant vers +x, la droite du
                // datum A1. D1 et E1 ne se recouvrent sur le datum qu'en z=10 : jamais compares.
                Corridor(CorridorA1, "A1", 0f, 0f, 20f, 0, true),
                Corridor(CorridorA1B, "A1b", 5f, 0f, 20f, 1, false),
                Corridor(CorridorA2, "A2", 10f, 0f, 20f, 2, false),
                Corridor(CorridorD1, "D1", 15f, 20f, 10f, 3, false),
                Corridor(CorridorD2, "D2", 20f, 20f, 0f, 5, false),
                Corridor(CorridorE1, "E1", 15f, 10f, 0f, 4, false)
            };

            var connection = new LaneConnection();
            connection.Id = ConnectionK1;
            connection.FromCorridorId = CorridorD1;
            connection.ToCorridorId = CorridorE1;
            connection.Kind = LaneConnectionKind.Continuation;
            source.Connections = new[] { connection };

            var adjacency = new LaneAdjacency();
            adjacency.Id = AdjacencyAd1;
            adjacency.FromCorridorId = CorridorA1;
            adjacency.ToCorridorId = CorridorA1B;
            adjacency.Side = LaneSide.Right;
            adjacency.FromStartSMeters = 2f;
            adjacency.FromEndSMeters = 18f;
            adjacency.ToStartSMeters = 2f;
            adjacency.ToEndSMeters = 18f;
            adjacency.Permission = LaneChangePermission.Allowed;
            source.Adjacencies = new[] { adjacency };

            var junction = new Junction();
            junction.Id = JunctionJ;
            junction.Label = "J";
            junction.Feature = JunctionFeature.Crossroads;
            junction.Boundary = Box(new Vector3(0f, 0f, 25f), new Vector3(6f, 3f, 6f));
            source.Junctions = new[] { junction };

            source.Movements = new[]
            {
                Movement(MovementM1, "M1", CorridorA1, CorridorD1, 0f, 15f),
                Movement(MovementM2, "M2", CorridorA1, CorridorD2, 0f, 20f),
                Movement(MovementM3, "M3", CorridorA2, CorridorD1, 10f, 15f),
                Movement(MovementM4, "M4", CorridorA2, CorridorD2, 10f, 20f)
            };

            source.Controls = new[]
            {
                Control(ControlC1, JunctionControlKind.Signalized, new[] { MovementM1, MovementM2 }),
                Control(ControlC2, JunctionControlKind.Signalized, new[] { MovementM3, MovementM4 })
            };

            var zone = new ConflictZone();
            zone.Id = ConflictZ1;
            zone.JunctionId = JunctionJ;
            zone.Volume = Box(new Vector3(0f, 0f, 25f), new Vector3(3f, 2f, 3f));
            zone.MemberMovementIds = new[] { MovementM2, MovementM3 };
            source.ConflictZones = new[] { zone };

            source.SignalPlans = new[] { Plan() };

            source.Portals = new[]
            {
                MakePortal(PortalEntry, "entree", CorridorA1, PortalRole.Entry, 0f),
                MakePortal(PortalExit, "sortie", CorridorE1, PortalRole.Exit, 10f)
            };

            var manifest = new ImportManifest();
            manifest.Entries = new[]
            {
                ManifestEntry(SectionS1, RoadRecordKind.Section, "slot-section-1"),
                ManifestEntry(CorridorA1, RoadRecordKind.Corridor, "slot-corridor-1"),
                ManifestEntry(JunctionJ, RoadRecordKind.Junction, "slot-junction-1")
            };
            manifest.TombstonedIds = new[] { RetiredId };

            var remap = new RoadIdRemap();
            remap.FromId = Id(91);
            remap.ToId = CorridorA1;
            manifest.Remaps = new[] { remap };

            source.Manifest = manifest;
            return source;
        }

        private static RoadModelValidationProfile Profile()
        {
            var profile = new RoadModelValidationProfile();
            profile.MaxVehicleHalfWidthMeters = 1.03f;
            profile.MaxVehicleLengthMeters = 4.5f;
            profile.LateralClearanceMarginMeters = 0.25f;
            profile.SeamGapToleranceMeters = 0.05f;
            profile.SeamTangentToleranceDegrees = 5f;
            profile.LengthToleranceMeters = 0.05f;
            profile.EnvelopeOverlapToleranceMeters = 0.05f;
            profile.GroundingMaxOffAxisDegrees = 45f;
            return profile;
        }

        private static RoadLocalizationProfile LocalizationProfile()
        {
            var profile = new RoadLocalizationProfile();
            profile.ScoreBandMeters = 0.15f;
            profile.HysteresisMeters = 0.1f;
            profile.AcceptanceDistanceMeters = 2.5f;
            profile.WrongWayHeadingDegrees = 90f;
            return profile;
        }

        private static RoadSection Section(RoadId id, string label, float speedLimit)
        {
            var section = new RoadSection();
            section.Id = id;
            section.Label = label;
            section.RoadClass = RoadClass.Local;
            section.Surface = RoadSurface.Asphalt;
            section.DefaultSpeedLimitMetersPerSecond = speedLimit;
            section.DefaultAllowedVehicleClasses = VehicleClassMask.Car | VehicleClassMask.Truck;
            return section;
        }

        private static LaneCorridor Corridor(RoadId id, string label, float x, float zStart, float zEnd, int lateralOrder, bool isDatum)
        {
            var corridor = new LaneCorridor();
            corridor.Id = id;
            corridor.Label = label;
            corridor.SectionId = SectionS1;
            corridor.Samples = StraightSamples(x, zStart, zEnd);
            corridor.LengthMeters = Mathf.Abs(zEnd - zStart);
            corridor.LateralOrder = lateralOrder;
            corridor.IsCrossSectionDatum = isDatum;
            return corridor;
        }

        /// <summary>Demi-tour analytique a z=20 de la bande xFrom (vers +z) a la bande xTo (vers -z).</summary>
        private static JunctionMovement Movement(RoadId id, string label, RoadId from, RoadId to, float xFrom, float xTo)
        {
            var movement = new JunctionMovement();
            movement.Id = id;
            movement.Label = label;
            movement.JunctionId = JunctionJ;
            movement.FromCorridorId = from;
            movement.ToCorridorId = to;
            movement.Samples = UTurnSamples(xFrom, xTo, 20f);
            movement.LengthMeters = Mathf.PI * 0.5f * (xTo - xFrom);
            movement.RoutePreferenceWeight = 0.5f;
            return movement;
        }

        private static JunctionControl Control(RoadId id, JunctionControlKind kind, RoadId[] movements)
        {
            var control = new JunctionControl();
            control.Id = id;
            control.JunctionId = JunctionJ;
            control.Kind = kind;
            control.ControlledMovementIds = movements;
            control.HasStopLine = true;
            var line = new RoadLineSegment();
            line.Start = new Vector3(-4f, 0f, 19f);
            line.End = new Vector3(4f, 0f, 19f);
            control.StopLine = line;
            return control;
        }

        private static SignalPlan Plan()
        {
            var plan = new SignalPlan();
            plan.Id = PlanP1;
            plan.JunctionId = JunctionJ;
            plan.Groups = new[]
            {
                Group(GroupG1, new[] { MovementM1, MovementM2 }),
                Group(GroupG2, new[] { MovementM3, MovementM4 })
            };
            plan.Phases = new[]
            {
                Phase(PhasePh1, 30f, SignalState.Green, SignalState.Red),
                Phase(PhasePh2, 30f, SignalState.Red, SignalState.Green)
            };
            return plan;
        }

        private static SignalGroup Group(RoadId groupId, RoadId[] members)
        {
            var group = new SignalGroup();
            group.GroupId = groupId;
            group.MemberMovementIds = members;
            return group;
        }

        private static SignalPhase Phase(RoadId phaseId, float duration, SignalState g1, SignalState g2)
        {
            var phase = new SignalPhase();
            phase.PhaseId = phaseId;
            phase.DurationSeconds = duration;
            phase.GroupStates = new[] { GroupState(GroupG1, g1), GroupState(GroupG2, g2) };
            return phase;
        }

        private static SignalGroupState GroupState(RoadId groupId, SignalState state)
        {
            var groupState = new SignalGroupState();
            groupState.GroupId = groupId;
            groupState.State = state;
            return groupState;
        }

        private static Portal MakePortal(RoadId id, string label, RoadId corridorId, PortalRole role, float s)
        {
            var portal = new Portal();
            portal.Id = id;
            portal.Label = label;
            portal.CorridorId = corridorId;
            portal.Role = role;
            portal.SMeters = s;
            portal.EnvelopeLengthMeters = 6f;
            portal.EnvelopeHalfWidthMeters = 2f;
            return portal;
        }

        private static ImportManifestEntry ManifestEntry(RoadId recordId, RoadRecordKind kind, string slot)
        {
            var entry = new ImportManifestEntry();
            entry.RecordId = recordId;
            entry.Kind = kind;
            entry.ModelId = ModelId;
            entry.ImporterSlot = slot;
            var trace = new SourceTrace();
            trace.SourceAssetGuid = "0123456789abcdef0123456789abcdef";
            trace.SourceObjectFileId = 12345L;
            trace.RelationKey = "node";
            entry.SourceKeys = new[] { trace };
            return entry;
        }

        private static RoadCurveSample[] StraightSamples(float x, float zStart, float zEnd)
        {
            float length = Mathf.Abs(zEnd - zStart);
            var tangent = new Vector3(0f, 0f, Mathf.Sign(zEnd - zStart));
            return new[]
            {
                Sample(0f, new Vector3(x, 0f, zStart), tangent, 0f),
                Sample(length * 0.5f, new Vector3(x, 0f, 0.5f * (zStart + zEnd)), tangent, 0f),
                Sample(length, new Vector3(x, 0f, zEnd), tangent, 0f)
            };
        }

        /// <summary>
        /// Demi-cercle echantillonne tous les 10 degres, abscisse = longueur d'arc, tangente exacte,
        /// courbure +1/r (virage a droite, vers +x depuis une bande orientee +z).
        /// </summary>
        private static RoadCurveSample[] UTurnSamples(float xFrom, float xTo, float z)
        {
            const int steps = 18;
            float radius = 0.5f * (xTo - xFrom);
            var center = new Vector3(xFrom + radius, 0f, z);
            var samples = new RoadCurveSample[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                float phi = Mathf.PI * i / steps;
                var position = center + new Vector3(-radius * Mathf.Cos(phi), 0f, radius * Mathf.Sin(phi));
                var tangent = new Vector3(Mathf.Sin(phi), 0f, Mathf.Cos(phi));
                samples[i] = Sample(radius * phi, position, tangent, 1f / radius);
            }

            return samples;
        }

        private static RoadCurveSample Sample(float s, Vector3 position)
        {
            return Sample(s, position, new Vector3(0f, 0f, 1f), 0f);
        }

        private static RoadCurveSample Sample(float s, Vector3 position, Vector3 tangent, float curvature)
        {
            var sample = new RoadCurveSample();
            sample.SMeters = s;
            sample.Position = position;
            sample.Tangent = tangent;
            sample.Up = new Vector3(0f, 1f, 0f);
            sample.CurvaturePerMeter = curvature;
            sample.HalfWidthLeftMeters = 2f;
            sample.HalfWidthRightMeters = 2f;
            return sample;
        }

        private static RoadBoundsBox Box(Vector3 center, Vector3 extents)
        {
            var box = new RoadBoundsBox();
            box.Center = center;
            box.Extents = extents;
            return box;
        }

        // ================================================================== helpers de test

        private static RoadModelVersion VersionOf(Action<RoadModelSource> mutate)
        {
            var source = BuildModel();
            if (mutate != null)
            {
                mutate(source);
            }

            return RoadModelCompiler.Compile(source).Version;
        }

        private static void AssertVersionChanges(Action<RoadModelSource> mutate, string because)
        {
            Assert.That(VersionOf(mutate), Is.Not.EqualTo(VersionOf(null)), because);
        }

        private static void AssertVersionUnchanged(Action<RoadModelSource> mutate, string because)
        {
            Assert.That(VersionOf(mutate), Is.EqualTo(VersionOf(null)), because);
        }

        // ------------------------------------------------------------------ chemin du writer public
        // Depuis la 5.26, certaines mutations isolees d'un champ canonique sont geometriquement
        // invalides (couture rompue, longueur incoherente, repere non unitaire...) : Compile les
        // rejette, a juste titre. Le champ reste prouve par mutation sur le writer public, sur une
        // charge construite ici comme le compilateur la construit, et la meme mutation est prouvee
        // rejetee par Compile. TheTestPayloadMirrorsTheCompilerPayload garde la fidelite de PayloadOf.

        private static RoadModelCanonicalPayload PayloadOf(RoadModelSource source)
        {
            var payload = new RoadModelCanonicalPayload();
            payload.SchemaVersion = RoadModelCompiler.CompilerSchemaVersion;
            payload.ModelId = source.ModelId;
            payload.ValidationProfile = source.ValidationProfile;
            payload.LocalizationProfile = source.LocalizationProfile;
            payload.Sections = source.Sections;
            payload.Corridors = EffectiveCorridorsOf(source);
            payload.Connections = source.Connections;
            payload.Adjacencies = source.Adjacencies;
            payload.Junctions = source.Junctions;
            payload.Movements = source.Movements;
            payload.Controls = source.Controls;
            payload.ConflictZones = source.ConflictZones;
            payload.SignalPlans = source.SignalPlans;
            payload.Portals = source.Portals;
            return payload;
        }

        private static EffectiveLaneCorridor[] EffectiveCorridorsOf(RoadModelSource source)
        {
            var effective = new EffectiveLaneCorridor[source.Corridors.Length];
            for (int i = 0; i < source.Corridors.Length; i++)
            {
                var corridor = source.Corridors[i];
                var section = Array.Find(source.Sections, delegate(RoadSection candidate) { return candidate.Id == corridor.SectionId; });

                effective[i].CorridorId = corridor.Id;
                effective[i].SectionId = corridor.SectionId;
                effective[i].LengthMeters = corridor.LengthMeters;
                effective[i].Samples = corridor.Samples;
                effective[i].SpeedLimitMetersPerSecond = corridor.HasSpeedLimitOverride ? corridor.SpeedLimitOverrideMetersPerSecond : section.DefaultSpeedLimitMetersPerSecond;
                effective[i].Surface = corridor.HasSurfaceOverride ? corridor.SurfaceOverride : section.Surface;
                effective[i].AllowedVehicleClasses = corridor.HasAllowedVehicleClassesOverride ? corridor.AllowedVehicleClassesOverride : section.DefaultAllowedVehicleClasses;
                effective[i].LateralOrder = corridor.LateralOrder;
                effective[i].IsCrossSectionDatum = corridor.IsCrossSectionDatum;
            }

            return effective;
        }

        private static string FingerprintOf(Action<RoadModelSource> mutate)
        {
            var source = BuildModel();
            if (mutate != null)
            {
                mutate(source);
            }

            ulong high;
            ulong low;
            RoadModelCanonicalWriter.ComputeFingerprint(PayloadOf(source), out high, out low);
            return high.ToString("x16") + low.ToString("x16");
        }

        /// <summary>
        /// Champ canonique dont la mutation isolee est geometriquement interdite : il est prouve sur
        /// le writer public, et Compile rejette la meme mutation.
        /// </summary>
        private static void AssertFieldCoveredButGeometricallyForbidden(Action<RoadModelSource> mutate, RoadModelValidationCode code, string because)
        {
            Assert.That(FingerprintOf(mutate), Is.Not.EqualTo(FingerprintOf(null)), because);

            var source = BuildModel();
            mutate(source);
            var exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(source); },
                "La geometrie doit rejeter cette mutation : " + because);
            Assert.That(exception.HasCode(code), Is.True,
                "Le rejet doit porter le code " + code + ", pas un autre motif : " + exception.Message);
        }

        /// <summary>Mutation parametree par un ecart, pour eprouver un pas de quantification.</summary>
        private delegate void DeltaMutation(RoadModelSource source, float delta);

        private static void AssertStepIsLoadBearing(double step, DeltaMutation apply, string unit)
        {
            var atZeroDelta = VersionOf(delegate(RoadModelSource source) { apply(source, 0f); });
            var atQuarterStep = VersionOf(delegate(RoadModelSource source) { apply(source, (float)(step * 0.25d)); });
            var atOneStep = VersionOf(delegate(RoadModelSource source) { apply(source, (float)step); });

            Assert.That(atQuarterStep, Is.EqualTo(atZeroDelta),
                "Un quart de pas doit etre absorbe par la quantification (" + unit + ").");
            Assert.That(atOneStep, Is.Not.EqualTo(atZeroDelta),
                "Un pas complet doit changer la version (" + unit + ").");
        }

        /// <summary>Ajoute une section aux defauts identiques a S1, sans y rattacher de corridor.</summary>
        private static void AddSecondSection(RoadModelSource source)
        {
            var sections = new RoadSection[source.Sections.Length + 1];
            Array.Copy(source.Sections, sections, source.Sections.Length);
            sections[sections.Length - 1] = Section(SectionS2, "S2", 13.9f);
            source.Sections = sections;
        }

        /// <summary>
        /// Deplace E1 sur la section d'accueil demandee, S2 et S3 etant toutes deux ajoutees. E1
        /// porte l'ordre 4 de S1 : D2 repasse de 5 a 4 pour que S1 reste contigue de 0 a 4, et la
        /// coupe reste monotone (D2 est toujours le plus a droite). Sur sa section d'accueil E1
        /// devient l'ordre 0 et le datum, seule forme valide pour une coupe a un corridor (AD-48).
        /// </summary>
        private static void MoveE1Onto(RoadModelSource source, RoadId hostSection)
        {
            AddSecondSection(source);
            AddThirdSection(source);
            source.Corridors[CorridorE1Index].SectionId = hostSection;
            source.Corridors[CorridorE1Index].LateralOrder = 0;
            source.Corridors[CorridorE1Index].IsCrossSectionDatum = true;
            source.Corridors[CorridorD2Index].LateralOrder = 4;
        }

        /// <summary>Ajoute une troisieme section aux defauts identiques a S1, sans corridor.</summary>
        private static void AddThirdSection(RoadModelSource source)
        {
            var sections = new RoadSection[source.Sections.Length + 1];
            Array.Copy(source.Sections, sections, source.Sections.Length);
            sections[sections.Length - 1] = Section(SectionS3, "S3", 13.9f);
            source.Sections = sections;
        }

        /// <summary>Ajoute un second carrefour, sans mouvement ni controle.</summary>
        private static void AddSecondJunction(RoadModelSource source)
        {
            var second = new Junction();
            second.Id = JunctionJ2;
            second.Label = "J2";
            second.Feature = JunctionFeature.TJunction;
            second.Boundary = Box(new Vector3(40f, 0f, 25f), new Vector3(6f, 3f, 6f));

            var junctions = new Junction[source.Junctions.Length + 1];
            Array.Copy(source.Junctions, junctions, source.Junctions.Length);
            junctions[junctions.Length - 1] = second;
            source.Junctions = junctions;
        }

        private static RoadModelCompilationException AssertHardFailure(Action<RoadModelSource> mutate, RoadModelValidationCode expected)
        {
            var source = BuildModel();
            mutate(source);

            var exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(source); });
            Assert.That(exception.HasCode(expected), Is.True,
                "Code de motif attendu " + expected + ", obtenu : " + exception.Message);
            return exception;
        }

        // ================================================================== modele nominal

        [Test]
        public void TheSyntheticModelValidatesCompilesAndCarriesACompilerEmittedVersion()
        {
            var source = BuildModel();

            Assert.That(RoadModelValidator.Validate(source), Is.Empty, "Le modele synthetique doit etre valide.");

            var model = RoadModelCompiler.Compile(source);

            Assert.That(model.ModelId, Is.EqualTo(ModelId));
            Assert.That(model.Version.IsEmpty, Is.False, "Une version est emise.");
            Assert.That(model.Version.SchemaVersion, Is.EqualTo(RoadModelCompiler.CompilerSchemaVersion));
            Assert.That(model.Corridors.Count, Is.EqualTo(6));
            Assert.That(model.Movements.Count, Is.EqualTo(4));
            Assert.That(model.Controls.Count, Is.EqualTo(2));
        }

        [Test]
        public void SectionDefaultsAreResolvedIntoEffectiveCorridorValuesAndOverridesWin()
        {
            var source = BuildModel();
            source.Corridors[1].HasSpeedLimitOverride = true;
            source.Corridors[1].SpeedLimitOverrideMetersPerSecond = 8.5f;
            source.Corridors[1].HasSurfaceOverride = true;
            source.Corridors[1].SurfaceOverride = RoadSurface.Gravel;

            var model = RoadModelCompiler.Compile(source);

            EffectiveLaneCorridor inherited;
            Assert.That(model.TryGetCorridor(CorridorA1, out inherited), Is.True);
            Assert.That(inherited.SpeedLimitMetersPerSecond, Is.EqualTo(13.9f).Within(1e-4f), "Defaut de section resolu.");
            Assert.That(inherited.Surface, Is.EqualTo(RoadSurface.Asphalt));
            Assert.That(inherited.AllowedVehicleClasses, Is.EqualTo(VehicleClassMask.Car | VehicleClassMask.Truck));

            EffectiveLaneCorridor overridden;
            Assert.That(model.TryGetCorridor(CorridorA1B, out overridden), Is.True);
            Assert.That(overridden.SpeedLimitMetersPerSecond, Is.EqualTo(8.5f).Within(1e-4f));
            Assert.That(overridden.Surface, Is.EqualTo(RoadSurface.Gravel));
        }

        [Test]
        public void TheCompiledModelDoesNotAliasTheSourceArrays()
        {
            var source = BuildModel();
            var model = RoadModelCompiler.Compile(source);

            // Toutes les profondeurs de la copie defensive sont mutees apres coup : echantillons,
            // appartenance de controle, et les tableaux imbriques d'un SignalPlan et d'une
            // ConflictZone. Un modele qui aliaserait la source changerait derriere une version
            // deja emise.
            source.Corridors[CorridorA1Index].Samples[0].HalfWidthLeftMeters = 99f;
            source.Controls[ControlC1Index].ControlledMovementIds[0] = UnknownId;
            source.SignalPlans[0].Groups[0].MemberMovementIds[0] = UnknownId;
            source.SignalPlans[0].Phases[0].GroupStates[0].State = SignalState.Red;
            source.ConflictZones[0].MemberMovementIds[0] = UnknownId;

            EffectiveLaneCorridor corridor;
            Assert.That(model.TryGetCorridor(CorridorA1, out corridor), Is.True);
            Assert.That(corridor.Samples[0].HalfWidthLeftMeters, Is.EqualTo(2f).Within(1e-4f),
                "Copie defensive : muter la source ne change pas le modele compile.");

            CompiledJunctionControl control;
            Assert.That(model.TryGetControlForMovement(MovementM1, out control), Is.True);
            Assert.That(control.Id, Is.EqualTo(ControlC1));
            Assert.That(control.ControlledMovementIds[0], Is.EqualTo(MovementM1),
                "Appartenance de controle clonee.");

            Assert.That(model.SignalPlans[0].Groups[0].MemberMovementIds[0], Is.EqualTo(MovementM1),
                "Membres d'un groupe de signal clones.");
            Assert.That(model.SignalPlans[0].Phases[0].GroupStates[0].State, Is.EqualTo(SignalState.Green),
                "Etats de groupe d'une phase clones.");
            Assert.That(model.ConflictZones[0].MemberMovementIds[0], Is.EqualTo(MovementM2),
                "Membres d'une zone de conflit clones.");
        }

        [Test]
        public void TheCompiledModelExposesNoWritableNestedCollection()
        {
            // Type : aucune vue compilee ne porte de tableau, donc `model.X[i].Y[j] = v` ne compile pas.
            var compiledViews = new[]
            {
                typeof(EffectiveLaneCorridor), typeof(CompiledJunctionMovement), typeof(CompiledJunctionControl),
                typeof(CompiledConflictZone), typeof(CompiledSignalPlan), typeof(CompiledSignalGroup),
                typeof(CompiledSignalPhase)
            };
            foreach (var type in compiledViews)
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    Assert.That(field.FieldType.IsArray, Is.False,
                        "Tableau muable expose par le modele compile : " + type.Name + "." + field.Name);
                }
            }

            // Execution : les collections ne se convertissent pas en tableau pour etre ecrites.
            var model = RoadModelCompiler.Compile(BuildModel());
            EffectiveLaneCorridor corridor;
            Assert.That(model.TryGetCorridor(CorridorA1, out corridor), Is.True);
            Assert.That(corridor.Samples, Is.Not.InstanceOf<RoadCurveSample[]>());
            Assert.That(model.Movements[0].Samples, Is.Not.InstanceOf<RoadCurveSample[]>());
            Assert.That(model.Controls[0].ControlledMovementIds, Is.Not.InstanceOf<RoadId[]>());
            Assert.That(model.ConflictZones[0].MemberMovementIds, Is.Not.InstanceOf<RoadId[]>());
            Assert.That(model.SignalPlans[0].Groups[0].MemberMovementIds, Is.Not.InstanceOf<RoadId[]>());
            Assert.That(model.SignalPlans[0].Phases[0].GroupStates, Is.Not.InstanceOf<SignalGroupState[]>());
        }

        // ================================================================== matrice : ordre indifferent

        [Test]
        public void TwoSemanticallyIdenticalModelsWithDifferentOrdersProduceTheSameVersion()
        {
            var straight = BuildModel();
            var shuffled = BuildModel();

            Array.Reverse(shuffled.Corridors);
            Array.Reverse(shuffled.Movements);
            Array.Reverse(shuffled.Controls);
            Array.Reverse(shuffled.Portals);
            Array.Reverse(shuffled.ConflictZones[0].MemberMovementIds);
            Array.Reverse(shuffled.Controls[0].ControlledMovementIds);
            Array.Reverse(shuffled.Controls[1].ControlledMovementIds);
            Array.Reverse(shuffled.SignalPlans[0].Groups);
            Array.Reverse(shuffled.SignalPlans[0].Groups[0].MemberMovementIds);
            Array.Reverse(shuffled.SignalPlans[0].Phases[0].GroupStates);
            Array.Reverse(shuffled.Manifest.Entries);

            var straightVersion = RoadModelCompiler.Compile(straight).Version;
            var shuffledVersion = RoadModelCompiler.Compile(shuffled).Version;

            Assert.That(shuffledVersion, Is.EqualTo(straightVersion),
                "La canonicalisation est ordre-independante : ordre d'insertion, de collection et de serialisation ne comptent pas.");
        }

        [Test]
        public void ThePhaseSequenceIsSemanticAndReorderingItChangesTheVersion()
        {
            AssertVersionChanges(
                delegate(RoadModelSource source) { Array.Reverse(source.SignalPlans[0].Phases); },
                "L'ordre des phases EST la semantique du plan : il n'est pas un ordre de collection.");
        }

        // ================================================================== matrice : changement comportemental

        [Test]
        [Category("Story536")]
        public void EveryBehaviorAffectingChangeMovesTheVersion()
        {
            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].Samples[1].HalfWidthRightMeters = 2.5f; },
                "Largeur utile : comportemental.");

            // Rebrancher une connexion ou un mouvement sans deplacer sa geometrie rompt la couture :
            // prouve sur le writer, rejete par Compile (5.26).
            AssertFieldCoveredButGeometricallyForbidden(
                delegate(RoadModelSource source) { source.Connections[0].FromCorridorId = CorridorD2; },
                RoadModelValidationCode.ConnectionSeamBroken,
                "Topologie : comportemental.");

            AssertFieldCoveredButGeometricallyForbidden(
                delegate(RoadModelSource source) { source.Movements[MovementM1Index].ToCorridorId = CorridorD2; },
                RoadModelValidationCode.MovementSeamBroken,
                "Mouvement : comportemental.");

            // Story 5.35 : une ligne Stop doit couper ses mouvements une fois ; Story 5.36 : un plan ne vit que sur un carrefour
            // signalise. Le genre est donc compare entre Stop et Yield, sans ligne ni plan de part et d'autre : seul le genre differe.
            Action<RoadModelSource> withoutLinesNorPlan = delegate(RoadModelSource source)
            {
                source.Controls[ControlC1Index].HasStopLine = false;
                source.Controls[ControlC2Index].HasStopLine = false;
                source.SignalPlans = new SignalPlan[0];
            };
            Func<JunctionControlKind, Action<RoadModelSource>> allOfKind = delegate(JunctionControlKind kind)
            {
                return delegate(RoadModelSource source)
                {
                    withoutLinesNorPlan(source);
                    source.Controls[ControlC1Index].Kind = kind;
                    source.Controls[ControlC2Index].Kind = kind;
                };
            };
            Assert.That(VersionOf(allOfKind(JunctionControlKind.Stop)), Is.Not.EqualTo(VersionOf(allOfKind(JunctionControlKind.Yield))),
                "Genre de controle : comportemental.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.ConflictZones[0].Volume = Box(new Vector3(0f, 0f, 25f), new Vector3(4f, 2f, 3f)); },
                "Zone de conflit : comportemental.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.SignalPlans[0].Phases[0].DurationSeconds = 25f; },
                "Plan de signal : comportemental.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Portals[PortalEntryIndex].SMeters = 1.5f; },
                "Portail : comportemental.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.ValidationProfile.MaxVehicleHalfWidthMeters = 1.4f; },
                "Profil de validation : versionne.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Sections[0].DefaultSpeedLimitMetersPerSecond = 11f; },
                "Defaut de section resolu en valeur effective de corridor : comportemental.");

            // AD-48. Un echange, pas une reaffectation : l'ensemble reste unique et contigu, donc seul
            // le fait teste bouge. Depuis la 5.26 l'ordre echange contredit la geometrie (A1b n'est
            // pas a gauche de A1) : prouve sur le writer, rejete par Compile.
            AssertFieldCoveredButGeometricallyForbidden(
                delegate(RoadModelSource source)
                {
                    source.Corridors[CorridorA1Index].LateralOrder = 1;
                    source.Corridors[CorridorA1BIndex].LateralOrder = 0;
                },
                RoadModelValidationCode.NonMonotoneLateralOrder,
                "Ordre transversal : position semantique authoree, pas un ordre d'enregistrement.");

            AssertVersionChanges(
                delegate(RoadModelSource source)
                {
                    source.Corridors[CorridorA1Index].IsCrossSectionDatum = false;
                    source.Corridors[CorridorA2Index].IsCrossSectionDatum = true;
                },
                "Datum transversal : change le repere de toute la coupe, donc comportemental.");
        }

        /// <summary>
        /// Une mutation par champ canonique restant : supprimer une seule ligne d'ecriture de
        /// <c>RoadModelCanonicalWriter.Write</c> doit faire rougir cette fixture.
        ///
        /// Exception assumee : les cles de carrefour parent (<c>JunctionMovement.JunctionId</c>,
        /// <c>JunctionControl.JunctionId</c>, <c>ConflictZone.JunctionId</c>,
        /// <c>SignalPlan.JunctionId</c>) ne sont pas isolables ici -- les deplacer exige de
        /// co-deplacer leur proprietaire, donc la mutation ne prouverait plus le champ. Elles sont
        /// gardees par le validateur a la place (voir les echecs durs de parente et d'appartenance).
        /// </summary>
        [Test]
        public void EveryRemainingCanonicalFieldMovesTheVersion()
        {
            // ---------------------------------------------------------- identite et profil
            AssertVersionChanges(
                delegate(RoadModelSource source)
                {
                    source.ModelId = Id(300);
                    for (int i = 0; i < source.Manifest.Entries.Length; i++)
                    {
                        source.Manifest.Entries[i].ModelId = Id(300);
                    }
                },
                "Identite du modele.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.ValidationProfile.MaxVehicleLengthMeters = 5f; },
                "Profil : longueur de gabarit maximale.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.ValidationProfile.LateralClearanceMarginMeters = 0.4f; },
                "Profil : marge de degagement lateral.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.ValidationProfile.SeamGapToleranceMeters = 0.04f; },
                "Profil : tolerance d'ecart de couture.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.ValidationProfile.SeamTangentToleranceDegrees = 4f; },
                "Profil : tolerance de tangente de couture.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.ValidationProfile.LengthToleranceMeters = 0.04f; },
                "Profil : tolerance de longueur.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.ValidationProfile.EnvelopeOverlapToleranceMeters = 0.02f; },
                "Profil : tolerance de recouvrement d'enveloppes.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.ValidationProfile.GroundingMaxOffAxisDegrees = 40f; },
                "Profil : seuil d'ancrage au datum (AD-48).");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.LocalizationProfile.ScoreBandMeters = 0.3f; },
                "Profil de localisation : bande de score.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.LocalizationProfile.HysteresisMeters = 0.2f; },
                "Profil de localisation : hysteresis.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.LocalizationProfile.AcceptanceDistanceMeters = 3f; },
                "Profil de localisation : seuil d'acceptation.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.LocalizationProfile.WrongWayHeadingDegrees = 80f; },
                "Profil de localisation : seuil de contresens.");

            // ---------------------------------------------------------- section
            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Sections[0].RoadClass = RoadClass.Arterial; },
                "Classe de route.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Sections[0].Surface = RoadSurface.Concrete; },
                "Surface de section.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Sections[0].DefaultAllowedVehicleClasses = VehicleClassMask.Car; },
                "Classes de vehicules autorisees par defaut.");

            // Section rattachee. Sous AD-48 on ne peut plus deplacer un corridor en ne changeant que
            // son SectionId : sa position transversale est couplee a son appartenance, et la section
            // d'accueil exigerait alors un datum et une contiguite. L'isolation passe donc par deux
            // sections d'accueil aux defauts identiques, S2 et S3, toutes deux presentes des deux
            // cotes : E1 quitte S1 dans les deux charges, avec la meme position et le meme datum, et
            // seul son SectionId differe. Supprimer l'ecriture de ce champ rend les deux versions
            // egales et fait rougir cette assertion.
            var ontoS2 = VersionOf(delegate(RoadModelSource source) { MoveE1Onto(source, SectionS2); });
            var ontoS3 = VersionOf(delegate(RoadModelSource source) { MoveE1Onto(source, SectionS3); });
            Assert.That(ontoS3, Is.Not.EqualTo(ontoS2),
                "Section parente effective d'un corridor.");

            // ---------------------------------------------------------- corridor effectif
            AssertVersionChanges(
                delegate(RoadModelSource source)
                {
                    source.Corridors[CorridorA1Index].HasSurfaceOverride = true;
                    source.Corridors[CorridorA1Index].SurfaceOverride = RoadSurface.Gravel;
                },
                "Surface effective de corridor.");

            AssertVersionChanges(
                delegate(RoadModelSource source)
                {
                    source.Corridors[CorridorA1Index].HasAllowedVehicleClassesOverride = true;
                    source.Corridors[CorridorA1Index].AllowedVehicleClassesOverride = VehicleClassMask.Emergency;
                },
                "Classes de vehicules effectives de corridor.");

            // Longueur, abscisse, tangente et road-up isoles : longueur incoherente ou repere non
            // unitaire, donc prouves sur le writer et rejetes par Compile (5.26).
            AssertFieldCoveredButGeometricallyForbidden(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].LengthMeters = 21f; },
                RoadModelValidationCode.InconsistentCurveLength,
                "Longueur de corridor.");

            // ---------------------------------------------------------- echantillons de courbe
            AssertFieldCoveredButGeometricallyForbidden(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].Samples[1].SMeters = 11f; },
                RoadModelValidationCode.InconsistentCurveLength,
                "Abscisse curviligne d'un echantillon.");

            AssertFieldCoveredButGeometricallyForbidden(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].Samples[1].Tangent = new Vector3(0.1f, 0f, 0.99f); },
                RoadModelValidationCode.NonOrthonormalFrame,
                "Tangente d'un echantillon.");

            AssertFieldCoveredButGeometricallyForbidden(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].Samples[1].Up = new Vector3(0f, 0.9f, 0.1f); },
                RoadModelValidationCode.NonOrthonormalFrame,
                "Haut route d'un echantillon.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].Samples[1].CurvaturePerMeter = 0.02f; },
                "Courbure signee d'un echantillon.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].Samples[1].HalfWidthLeftMeters = 2.5f; },
                "Demi-largeur gauche d'un echantillon.");

            // ---------------------------------------------------------- connexions et adjacences
            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Connections[0].Kind = LaneConnectionKind.Merge; },
                "Genre de connexion longitudinale.");

            AssertFieldCoveredButGeometricallyForbidden(
                delegate(RoadModelSource source) { source.Connections[0].ToCorridorId = CorridorD2; },
                RoadModelValidationCode.ConnectionSeamBroken,
                "Corridor d'arrivee d'une connexion.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Adjacencies[0].Permission = LaneChangePermission.Forbidden; },
                "Legalite de changement de file.");

            AssertFieldCoveredButGeometricallyForbidden(
                delegate(RoadModelSource source) { source.Adjacencies[0].Side = LaneSide.Left; },
                RoadModelValidationCode.LaneSideDisagreement,
                "Cote d'adjacence.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Adjacencies[0].ToCorridorId = CorridorA2; },
                "Corridor cible d'adjacence.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Adjacencies[0].FromStartSMeters = 3f; },
                "Debut de l'intervalle source d'adjacence.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Adjacencies[0].FromEndSMeters = 17f; },
                "Fin de l'intervalle source d'adjacence.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Adjacencies[0].ToStartSMeters = 3f; },
                "Debut de l'intervalle cible d'adjacence.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Adjacencies[0].ToEndSMeters = 17f; },
                "Fin de l'intervalle cible d'adjacence.");

            // ---------------------------------------------------------- carrefour et mouvements
            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Junctions[0].Feature = JunctionFeature.Roundabout; },
                "Classification authoree du carrefour.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Junctions[0].Boundary = Box(new Vector3(1f, 0f, 25f), new Vector3(6f, 3f, 6f)); },
                "Centre de la frontiere de carrefour.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Junctions[0].Boundary = Box(new Vector3(0f, 0f, 25f), new Vector3(7f, 3f, 6f)); },
                "Demi-dimensions de la frontiere de carrefour.");

            AssertFieldCoveredButGeometricallyForbidden(
                delegate(RoadModelSource source) { source.Movements[MovementM1Index].FromCorridorId = CorridorA2; },
                RoadModelValidationCode.MovementSeamBroken,
                "Corridor d'approche d'un mouvement.");

            AssertFieldCoveredButGeometricallyForbidden(
                delegate(RoadModelSource source) { source.Movements[MovementM1Index].LengthMeters = 10f; },
                RoadModelValidationCode.InconsistentCurveLength,
                "Longueur d'un mouvement.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Movements[MovementM1Index].RoutePreferenceWeight = 0.75f; },
                "Preference de route d'un mouvement.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Movements[MovementM1Index].Samples[1].CurvaturePerMeter = 0.05f; },
                "Enveloppe propre d'un mouvement.");

            // ---------------------------------------------------------- controles
            AssertVersionChanges(
                delegate(RoadModelSource source)
                {
                    // Recomposition : chaque mouvement reste couvert exactement une fois.
                    source.Controls[ControlC1Index].ControlledMovementIds = new[] { MovementM1, MovementM3 };
                    source.Controls[ControlC2Index].ControlledMovementIds = new[] { MovementM2, MovementM4 };
                },
                "Composition d'une liaison de controle.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Controls[ControlC1Index].HasStopLine = false; },
                "Presence d'une ligne d'arret.");

            AssertVersionChanges(
                delegate(RoadModelSource source)
                {
                    var line = source.Controls[ControlC1Index].StopLine;
                    line.Start = new Vector3(-5f, 0f, 19f);
                    source.Controls[ControlC1Index].StopLine = line;
                },
                "Debut de la ligne d'arret.");

            AssertVersionChanges(
                delegate(RoadModelSource source)
                {
                    var line = source.Controls[ControlC1Index].StopLine;
                    line.End = new Vector3(5f, 0f, 19f);
                    source.Controls[ControlC1Index].StopLine = line;
                },
                "Fin de la ligne d'arret.");

            // ---------------------------------------------------------- conflits
            AssertVersionChanges(
                delegate(RoadModelSource source) { source.ConflictZones[0].Volume = Box(new Vector3(1f, 0f, 25f), new Vector3(3f, 2f, 3f)); },
                "Centre du volume de conflit.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.ConflictZones[0].MemberMovementIds = new[] { MovementM1, MovementM4 }; },
                "Appartenance a une zone de conflit.");

            // ---------------------------------------------------------- signaux
            AssertVersionChanges(
                delegate(RoadModelSource source)
                {
                    // Renommage coherent du groupe : le plan et les phases designent le meme groupe.
                    source.SignalPlans[0].Groups[0].GroupId = Id(301);
                    source.SignalPlans[0].Phases[0].GroupStates[0].GroupId = Id(301);
                    source.SignalPlans[0].Phases[1].GroupStates[0].GroupId = Id(301);
                },
                "Identite d'un groupe de signal.");

            AssertVersionChanges(
                delegate(RoadModelSource source)
                {
                    // Echange d'appartenance entre les deux groupes : chaque mouvement signalise
                    // reste couvert par exactement un groupe.
                    source.SignalPlans[0].Groups[0].MemberMovementIds = new[] { MovementM3, MovementM4 };
                    source.SignalPlans[0].Groups[1].MemberMovementIds = new[] { MovementM1, MovementM2 };
                },
                "Appartenance signal d'un groupe.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.SignalPlans[0].Phases[0].PhaseId = PhasePh3; },
                "Identite d'une phase.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.SignalPlans[0].Phases[1].GroupStates[0].State = SignalState.Yellow; },
                "Etat autorise d'un groupe pendant une phase.");

            // ---------------------------------------------------------- portails
            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Portals[PortalEntryIndex].Role = PortalRole.Exit; },
                "Role de portail.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Portals[PortalEntryIndex].CorridorId = CorridorA2; },
                "Corridor d'un portail.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Portals[PortalEntryIndex].EnvelopeLengthMeters = 8f; },
                "Longueur d'enveloppe d'un portail.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Portals[PortalEntryIndex].EnvelopeHalfWidthMeters = 2.5f; },
                "Demi-largeur d'enveloppe d'un portail.");
        }

        /// <summary>
        /// Chaque quantificateur est exerce sur son unite propre : un ecart d'exactement un pas
        /// change la version, un quart de pas ne la change pas. Sans ces lignes, les constantes de
        /// pas pourraient valoir n'importe quoi sans faire rougir la suite.
        /// </summary>
        [Test]
        public void EachQuantizerStepIsLoadBearingOnItsOwnUnit()
        {
            AssertStepIsLoadBearing(
                RoadModelCanonicalWriter.MeterStep,
                delegate(RoadModelSource source, float delta) { source.Corridors[CorridorA1Index].Samples[1].HalfWidthLeftMeters = 2f + delta; },
                "metres");

            AssertStepIsLoadBearing(
                RoadModelCanonicalWriter.SpeedStep,
                delegate(RoadModelSource source, float delta) { source.Sections[0].DefaultSpeedLimitMetersPerSecond = 13.9f + delta; },
                "metres par seconde");

            AssertStepIsLoadBearing(
                RoadModelCanonicalWriter.DegreeStep,
                delegate(RoadModelSource source, float delta) { source.LocalizationProfile.WrongWayHeadingDegrees = 89f + delta; },
                "degres");

            AssertStepIsLoadBearing(
                RoadModelCanonicalWriter.SecondStep,
                delegate(RoadModelSource source, float delta) { source.SignalPlans[0].Phases[0].DurationSeconds = 30f + delta; },
                "secondes");

            AssertStepIsLoadBearing(
                RoadModelCanonicalWriter.CurvatureStep,
                delegate(RoadModelSource source, float delta) { source.Corridors[CorridorA1Index].Samples[1].CurvaturePerMeter = delta; },
                "1/m");

            AssertStepIsLoadBearing(
                RoadModelCanonicalWriter.DirectionStep,
                delegate(RoadModelSource source, float delta) { source.Corridors[CorridorA1Index].Samples[1].Tangent = new Vector3(delta, 0f, 1f); },
                "composante de direction");

            AssertStepIsLoadBearing(
                RoadModelCanonicalWriter.RatioStep,
                delegate(RoadModelSource source, float delta) { source.Movements[MovementM1Index].RoutePreferenceWeight = 0.5f + delta; },
                "sans unite");
        }

        // ================================================================== matrice : changement cosmetique

        [Test]
        public void NoCosmeticChangeMovesTheVersion()
        {
            AssertVersionUnchanged(
                delegate(RoadModelSource source)
                {
                    source.Label = "autre libelle de modele";
                    source.Sections[0].Label = "autre";
                    source.Corridors[CorridorA1Index].Label = "autre";
                    source.Junctions[0].Label = "autre";
                    source.Movements[MovementM1Index].Label = "autre";
                    source.Portals[PortalEntryIndex].Label = "autre";
                },
                "Libelles : diagnostics seuls.");

            AssertVersionUnchanged(
                delegate(RoadModelSource source)
                {
                    source.Manifest.Entries[0].SourceKeys[0].SourceAssetGuid = "ffffffffffffffffffffffffffffffff";
                    source.Manifest.Entries[0].SourceKeys[0].SourceObjectFileId = 777L;
                    source.Manifest.Entries[0].ImporterSlot = "slot-section-renomme";
                },
                "SourceTrace et slot d'importeur : provenance, hors charge canonique.");

            AssertVersionUnchanged(
                delegate(RoadModelSource source)
                {
                    var tombstones = new RoadId[source.Manifest.TombstonedIds.Length + 1];
                    Array.Copy(source.Manifest.TombstonedIds, tombstones, source.Manifest.TombstonedIds.Length);
                    tombstones[tombstones.Length - 1] = Id(92);
                    source.Manifest.TombstonedIds = tombstones;
                },
                "Historique de suppression : hors charge canonique.");

            AssertVersionUnchanged(
                delegate(RoadModelSource source) { Array.Reverse(source.Movements); },
                "Ordre des enregistrements : reconstructible.");
        }

        // ================================================================== matrice : integrite referentielle

        [Test]
        public void ADuplicateIdIsAHardFailureNamingTheOffendingId()
        {
            var exception = AssertHardFailure(
                delegate(RoadModelSource source) { source.Corridors[1].Id = CorridorA1; },
                RoadModelValidationCode.DuplicateId);

            Assert.That(ContainsIssue(exception, RoadModelValidationCode.DuplicateId, CorridorA1), Is.True,
                "L'echec nomme l'identifiant fautif.");
        }

        [Test]
        public void AnEmptyIdIsAHardFailure()
        {
            AssertHardFailure(
                delegate(RoadModelSource source) { source.Junctions[0].Id = RoadId.None; },
                RoadModelValidationCode.EmptyId);
        }

        [Test]
        public void AnUnresolvedReferenceIsAHardFailure()
        {
            var exception = AssertHardFailure(
                delegate(RoadModelSource source) { source.Connections[0].ToCorridorId = UnknownId; },
                RoadModelValidationCode.UnresolvedReference);

            Assert.That(ContainsIssue(exception, RoadModelValidationCode.UnresolvedReference, UnknownId), Is.True);
        }

        [Test]
        public void AManifestEntryFromAnotherModelIsACrossVersionReference()
        {
            AssertHardFailure(
                delegate(RoadModelSource source) { source.Manifest.Entries[0].ModelId = UnknownId; },
                RoadModelValidationCode.CrossVersionReference);
        }

        [Test]
        public void AnIncompatibleManifestRemapIsAHardFailure()
        {
            AssertHardFailure(
                delegate(RoadModelSource source) { source.Manifest.Remaps[0].ToId = UnknownId; },
                RoadModelValidationCode.IncompatibleManifestRemap);

            AssertHardFailure(
                delegate(RoadModelSource source) { source.Manifest.Remaps[0].FromId = CorridorA1B; },
                RoadModelValidationCode.IncompatibleManifestRemap);
        }

        [Test]
        public void AResubmittedTombstonedIdIsAHardFailureAndEmitsNoModel()
        {
            var source = BuildModel();
            source.Manifest.TombstonedIds = new[] { RetiredId, CorridorA1 };

            var exception = Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(source); });

            Assert.That(exception.HasCode(RoadModelValidationCode.TombstonedIdReused), Is.True);
            Assert.That(ContainsIssue(exception, RoadModelValidationCode.TombstonedIdReused, CorridorA1), Is.True,
                "Un identifiant supprime n'est jamais recycle en silence.");
        }

        [Test]
        public void AnInvalidGeometryPayloadIsAHardFailure()
        {
            // Le serialiseur canonique ne trie PAS les echantillons : il compte sur ces deux
            // invariants pour que leur ordre soit semantique et non un ordre de collection.
            AssertHardFailure(
                delegate(RoadModelSource source)
                {
                    source.Corridors[CorridorA1Index].Samples = new[] { Sample(0f, new Vector3(0f, 0f, 0f)) };
                },
                RoadModelValidationCode.InvalidGeometryPayload);

            AssertHardFailure(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].Samples[1].SMeters = 0f; },
                RoadModelValidationCode.InvalidGeometryPayload);
        }

        [Test]
        public void AFiniteButUnquantizableValueIsAHardFailureInsteadOfSaturating()
        {
            // La conversion en long n'est pas verifiee : sans garde, deux modeles distincts mais
            // enormes s'effondreraient sur la meme version. Porte par un poids de route, que la
            // geometrie (5.26) ne contraint pas : une longueur enorme serait rejetee avant le writer.
            var exception = AssertHardFailure(
                delegate(RoadModelSource source) { source.Movements[MovementM1Index].RoutePreferenceWeight = 1e30f; },
                RoadModelValidationCode.NumericValueOutOfRange);

            Assert.That(exception.HasCode(RoadModelValidationCode.NonFiniteNumericValue), Is.False,
                "La valeur est finie : c'est le domaine quantifiable qui la refuse, pas la finitude.");
        }

        // ================================================================== matrice : propriete unique

        [Test]
        public void AMovementCoveredByZeroControlsIsAHardFailure()
        {
            var exception = AssertHardFailure(
                delegate(RoadModelSource source) { source.Controls[ControlC2Index].ControlledMovementIds = new[] { MovementM3 }; },
                RoadModelValidationCode.MovementControlCoverageInvalid);

            Assert.That(ContainsIssue(exception, RoadModelValidationCode.MovementControlCoverageInvalid, MovementM4), Is.True,
                "Uncontrolled est un choix explicite, pas un repli pour un mouvement non couvert.");
        }

        [Test]
        public void AMovementCoveredByTwoControlsIsAHardFailure()
        {
            var exception = AssertHardFailure(
                delegate(RoadModelSource source)
                {
                    source.Controls[ControlC2Index].ControlledMovementIds = new[] { MovementM3, MovementM4, MovementM1 };
                },
                RoadModelValidationCode.MovementControlCoverageInvalid);

            Assert.That(ContainsIssue(exception, RoadModelValidationCode.MovementControlCoverageInvalid, MovementM1), Is.True);
        }

        [Test]
        [Category("Story536")]
        public void EachMovementReadsItsKindFromItsSingleAuthoritativeControlBinding()
        {
            // Story 5.36 : Signalized ne se melange a aucun autre genre ; deux genres distincts sans ligne ni plan gardent l'objet du test.
            var source = BuildModel();
            source.Controls[ControlC1Index].Kind = JunctionControlKind.Priority;
            source.Controls[ControlC2Index].Kind = JunctionControlKind.Yield;
            source.Controls[ControlC1Index].HasStopLine = false;
            source.Controls[ControlC2Index].HasStopLine = false;
            source.SignalPlans = new SignalPlan[0];

            var model = RoadModelCompiler.Compile(source);

            CompiledJunctionControl m1Control;
            CompiledJunctionControl m4Control;
            Assert.That(model.TryGetControlForMovement(MovementM1, out m1Control), Is.True);
            Assert.That(model.TryGetControlForMovement(MovementM4, out m4Control), Is.True);
            Assert.That(m1Control.Kind, Is.EqualTo(JunctionControlKind.Priority));
            Assert.That(m4Control.Kind, Is.EqualTo(JunctionControlKind.Yield),
                "Le genre vient de la liaison, jamais d'une classification de carrefour ni de l'approche.");
        }

        [Test]
        public void AMovementWithoutAResolvedParentJunctionIsAHardFailure()
        {
            AssertHardFailure(
                delegate(RoadModelSource source) { source.Movements[MovementM1Index].JunctionId = RoadId.None; },
                RoadModelValidationCode.MovementParentInvalid);
        }

        [Test]
        public void AMovementDeclaredUnderTwoParentJunctionsIsAHardFailure()
        {
            var exception = AssertHardFailure(
                delegate(RoadModelSource source)
                {
                    AddSecondJunction(source);

                    var duplicate = Movement(MovementM1, "M1-bis", CorridorA1, CorridorD1, 0f, 15f);
                    duplicate.JunctionId = JunctionJ2;

                    var movements = new JunctionMovement[source.Movements.Length + 1];
                    Array.Copy(source.Movements, movements, source.Movements.Length);
                    movements[movements.Length - 1] = duplicate;
                    source.Movements = movements;
                },
                RoadModelValidationCode.MovementParentInvalid);

            Assert.That(ContainsIssue(exception, RoadModelValidationCode.MovementParentInvalid, MovementM1), Is.True,
                "Un JunctionMovement porte exactement un JunctionId parent persiste.");
        }

        [Test]
        public void ASignalizedControlWithoutAValidPlanIsAHardFailure()
        {
            AssertHardFailure(
                delegate(RoadModelSource source) { source.SignalPlans = new SignalPlan[0]; },
                RoadModelValidationCode.SignalizedControlWithoutPlan);

            AssertHardFailure(
                delegate(RoadModelSource source) { source.SignalPlans[0].Phases = new SignalPhase[0]; },
                RoadModelValidationCode.SignalizedControlWithoutPlan);

            AssertHardFailure(
                delegate(RoadModelSource source) { source.SignalPlans[0].Groups[1].MemberMovementIds = new RoadId[0]; },
                RoadModelValidationCode.SignalizedControlWithoutPlan);

            // Deux plans contradictoires sur le meme carrefour : la couverture n'est plus unique.
            AssertHardFailure(
                delegate(RoadModelSource source)
                {
                    var second = Plan();
                    second.Id = PlanP2;

                    var plans = new SignalPlan[source.SignalPlans.Length + 1];
                    Array.Copy(source.SignalPlans, plans, source.SignalPlans.Length);
                    plans[plans.Length - 1] = second;
                    source.SignalPlans = plans;
                },
                RoadModelValidationCode.SignalizedControlWithoutPlan);

            // Un mouvement liste par deux groupes du meme plan : le cote « cover >= 2 ».
            AssertHardFailure(
                delegate(RoadModelSource source)
                {
                    source.SignalPlans[0].Groups[1].MemberMovementIds = new[] { MovementM3, MovementM4, MovementM1 };
                },
                RoadModelValidationCode.SignalizedControlWithoutPlan);

            // Phase de duree nulle.
            AssertHardFailure(
                delegate(RoadModelSource source) { source.SignalPlans[0].Phases[0].DurationSeconds = 0f; },
                RoadModelValidationCode.SignalizedControlWithoutPlan);
        }

        [Test]
        public void APhaseTurningTwoMembersOfOneConflictZoneGreenIsAHardFailure()
        {
            var exception = AssertHardFailure(
                delegate(RoadModelSource source) { source.SignalPlans[0].Phases[0].GroupStates[1].State = SignalState.Green; },
                RoadModelValidationCode.ConflictingMovementsGreenTogether);

            Assert.That(ContainsIssue(exception, RoadModelValidationCode.ConflictingMovementsGreenTogether, ConflictZ1), Is.True);
        }

        [Test]
        public void AConflictZoneWithInvalidMembershipIsAHardFailure()
        {
            AssertHardFailure(
                delegate(RoadModelSource source) { source.ConflictZones[0].MemberMovementIds = new[] { MovementM2 }; },
                RoadModelValidationCode.ConflictZoneMembershipInvalid);

            AssertHardFailure(
                delegate(RoadModelSource source) { source.ConflictZones[0].MemberMovementIds = new[] { MovementM2, MovementM2 }; },
                RoadModelValidationCode.DuplicateId);

            // Le cas dangereux : un JunctionId mal saisi. ValidateConflictingGreens filtre sur
            // zone.JunctionId == plan.JunctionId, donc sans cette garde la zone serait simplement
            // ignoree et une phase rendant ses deux membres verts compilerait.
            var exception = AssertHardFailure(
                delegate(RoadModelSource source)
                {
                    AddSecondJunction(source);
                    source.ConflictZones[0].JunctionId = JunctionJ2;
                },
                RoadModelValidationCode.ConflictZoneMembershipInvalid);

            Assert.That(ContainsIssue(exception, RoadModelValidationCode.ConflictZoneMembershipInvalid, MovementM2), Is.True,
                "L'echec nomme le membre dont le carrefour ne correspond pas.");
        }

        /// <summary>Geometrie (5.26) : un volume de conflit aplati n'est pas un volume.</summary>
        [Test]
        public void ANonPositiveConflictZoneExtentIsAHardFailure()
        {
            var exception = AssertHardFailure(
                delegate(RoadModelSource source) { source.ConflictZones[0].Volume = Box(new Vector3(0f, 0f, 25f), new Vector3(3f, 0f, 3f)); },
                RoadModelValidationCode.NonPositiveBoxExtents);

            Assert.That(ContainsIssue(exception, RoadModelValidationCode.NonPositiveBoxExtents, ConflictZ1), Is.True);
        }

        [Test]
        public void AMistypedConflictZoneJunctionCannotSilentlyDisableTheGreenConflictCheck()
        {
            var source = BuildModel();
            AddSecondJunction(source);
            source.ConflictZones[0].JunctionId = JunctionJ2;
            source.SignalPlans[0].Phases[0].GroupStates[1].State = SignalState.Green;

            Assert.Throws<RoadModelCompilationException>(delegate { RoadModelCompiler.Compile(source); },
                "Une zone detachee de son carrefour plus une phase incompatible ne doit jamais compiler.");
        }

        // ================================================================== matrice : valeurs non finies

        [Test]
        public void ANonFiniteValueIsAHardFailureAndNoVersionIsEmitted()
        {
            AssertHardFailure(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].Samples[1].Position = new Vector3(float.NaN, 0f, 10f); },
                RoadModelValidationCode.NonFiniteNumericValue);

            AssertHardFailure(
                delegate(RoadModelSource source) { source.ValidationProfile.MaxVehicleLengthMeters = float.PositiveInfinity; },
                RoadModelValidationCode.NonFiniteNumericValue);

            AssertHardFailure(
                delegate(RoadModelSource source) { source.Movements[MovementM1Index].RoutePreferenceWeight = float.NegativeInfinity; },
                RoadModelValidationCode.NonFiniteNumericValue);
        }

        /// <summary>
        /// Garde du chemin writer : la charge construite par le test est celle du compilateur. Sans
        /// elle, une mutation prouvee sur le writer pourrait l'etre sur une charge qui n'existe pas.
        /// </summary>
        [Test]
        public void TheTestPayloadMirrorsTheCompilerPayload()
        {
            var version = VersionOf(null);
            Assert.That(FingerprintOf(null), Is.EqualTo(version.High.ToString("x16") + version.Low.ToString("x16")));
        }

        [Test]
        public void TheCanonicalWriterItselfRefusesToQuantizeANonFiniteValue()
        {
            var payload = new RoadModelCanonicalPayload();
            payload.SchemaVersion = RoadModelCompiler.CompilerSchemaVersion;
            payload.ModelId = ModelId;
            payload.ValidationProfile = Profile();
            payload.ValidationProfile.LateralClearanceMarginMeters = float.NaN;

            ulong high;
            ulong low;
            var exception = Assert.Throws<RoadModelCompilationException>(
                delegate { RoadModelCanonicalWriter.ComputeFingerprint(payload, out high, out low); });

            Assert.That(exception.HasCode(RoadModelValidationCode.NonFiniteNumericValue), Is.True,
                "Une valeur non finie n'est jamais quantifiee en silence.");
        }

        // ================================================================== determinisme numerique

        [Test]
        public void NegativeZeroAndPositiveZeroHashIdenticallyButOneQuantizationStepDoesNot()
        {
            float negativeZero = -0.0f;
            Assert.That(1f / negativeZero, Is.EqualTo(float.NegativeInfinity),
                "Garde du test : la constante est bien un zero negatif.");

            var withPositiveZero = VersionOf(delegate(RoadModelSource source)
            {
                source.Corridors[CorridorA1Index].Samples[0].Position = new Vector3(0f, 0f, 0f);
            });

            var withNegativeZero = VersionOf(delegate(RoadModelSource source)
            {
                source.Corridors[CorridorA1Index].Samples[0].Position = new Vector3(negativeZero, 0f, 0f);
            });

            Assert.That(withNegativeZero, Is.EqualTo(withPositiveZero),
                "-0 est normalise en +0 : deux modeles identiques ne divergent pas sur un signe invisible.");

            var oneStepApart = VersionOf(delegate(RoadModelSource source)
            {
                source.Corridors[CorridorA1Index].Samples[0].Position = new Vector3((float)RoadModelCanonicalWriter.MeterStep, 0f, 0f);
            });

            Assert.That(oneStepApart, Is.Not.EqualTo(withPositiveZero),
                "Un ecart d'un pas de quantification est comportemental et change la version.");
        }

        [Test]
        public void ADifferenceBelowHalfAQuantizationStepIsAbsorbed()
        {
            var baseline = VersionOf(null);
            var jittered = VersionOf(delegate(RoadModelSource source)
            {
                source.Corridors[CorridorA1Index].Samples[0].Position = new Vector3((float)(RoadModelCanonicalWriter.MeterStep * 0.25d), 0f, 0f);
            });

            Assert.That(jittered, Is.EqualTo(baseline),
                "Sous le demi-pas, la quantification absorbe le bruit de formatage flottant.");
        }

        // ================================================================== compilateur sans etat

        [Test]
        public void CompilingTheSameSourceTwiceWithAnotherCompileInBetweenNeverMovesEitherVersion()
        {
            var first = BuildModel();
            var other = BuildModel();
            other.Corridors[CorridorA1Index].Samples[1].HalfWidthRightMeters = 3f;

            var firstPass = RoadModelCompiler.Compile(first).Version;
            var otherPass = RoadModelCompiler.Compile(other).Version;
            var firstAgain = RoadModelCompiler.Compile(first).Version;
            var otherAgain = RoadModelCompiler.Compile(other).Version;
            var freshRebuild = RoadModelCompiler.Compile(BuildModel()).Version;

            Assert.That(firstAgain, Is.EqualTo(firstPass), "Compile est une fonction pure de sa source.");
            Assert.That(otherAgain, Is.EqualTo(otherPass));
            Assert.That(freshRebuild, Is.EqualTo(firstPass), "Une reconstruction identique retrouve la meme version.");
            Assert.That(otherPass, Is.Not.EqualTo(firstPass), "Deux sources differentes restent distinguables.");
        }

        [Test]
        public void TheCompilerHoldsNoInstanceStateBetweenCalls()
        {
            var type = typeof(RoadModelCompiler);

            Assert.That(type.IsAbstract && type.IsSealed, Is.True, "Classe statique : aucune instance a porter un etat.");

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                Assert.That(field.IsLiteral, Is.True,
                    "Champ retenant potentiellement un import precedent : " + field.Name);
            }
        }

        // ================================================================== propriete et index

        [Test]
        public void NoSourceRecordOwnsAnAuthorableReciprocalCollection()
        {
            // Seules appartenances possedees autorisees par AD-43/AD-46, plus la lignee d'import.
            var owned = new HashSet<string>(StringComparer.Ordinal)
            {
                "JunctionControl.ControlledMovementIds",
                "ConflictZone.MemberMovementIds",
                "SignalGroup.MemberMovementIds",
                "ImportManifest.TombstonedIds"
            };

            foreach (var type in typeof(RoadModelSource).Assembly.GetTypes())
            {
                if (type.Namespace != typeof(RoadModelSource).Namespace)
                {
                    continue;
                }

                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (field.FieldType != typeof(RoadId[]))
                    {
                        continue;
                    }

                    string qualified = type.Name + "." + field.Name;
                    Assert.That(owned.Contains(qualified), Is.True,
                        "Collection d'identifiants non autorisee sur un type source : " + qualified
                        + ". Les collections inverses sont des sorties du compilateur.");
                }
            }

            Assert.That(typeof(RoadSection).GetField("CorridorIds"), Is.Null,
                "RoadSection ne porte aucune liste de corridors : le corridor porte sa cle de section.");
            Assert.That(typeof(Junction).GetField("MovementIds"), Is.Null,
                "Junction ne porte aucune liste d'enfants faisant autorite.");
        }

        [Test]
        public void EveryInverseCollectionReadOnTheCompiledModelIsDerivedByTheCompiler()
        {
            var model = RoadModelCompiler.Compile(BuildModel());

            Assert.That(model.GetCorridorsInSection(SectionS1).Count, Is.EqualTo(6),
                "Corridors d'une section : derives des cles etrangeres portees par les corridors.");
            Assert.That(model.GetMovementsInJunction(JunctionJ).Count, Is.EqualTo(4));
            Assert.That(model.GetControlsInJunction(JunctionJ).Count, Is.EqualTo(2));
            Assert.That(model.GetConflictZonesInJunction(JunctionJ).Count, Is.EqualTo(1));
            Assert.That(model.GetSignalPlansInJunction(JunctionJ).Count, Is.EqualTo(1));
            Assert.That(model.GetSuccessorCorridors(CorridorD1), Is.EquivalentTo(new[] { CorridorE1 }));
            Assert.That(model.GetPredecessorCorridors(CorridorE1), Is.EquivalentTo(new[] { CorridorD1 }));
            Assert.That(model.GetPortalsOnCorridor(CorridorA1), Is.EquivalentTo(new[] { PortalEntry }));
            Assert.That(model.GetMovementsInJunction(UnknownId), Is.Empty,
                "Un carrefour inconnu n'a pas d'enfants, pas une exception.");
        }

        [Test]
        public void InverseCollectionsAreOrderIndependent()
        {
            var straight = RoadModelCompiler.Compile(BuildModel());

            var shuffledSource = BuildModel();
            Array.Reverse(shuffledSource.Movements);
            Array.Reverse(shuffledSource.Corridors);
            var shuffled = RoadModelCompiler.Compile(shuffledSource);

            Assert.That(shuffled.GetMovementsInJunction(JunctionJ), Is.EqualTo(straight.GetMovementsInJunction(JunctionJ)),
                "Les index compiles sont des caches stables, jamais l'ordre de la source.");
            Assert.That(shuffled.GetCorridorsInSection(SectionS1), Is.EqualTo(straight.GetCorridorsInSection(SectionS1)));
        }

        // ================================================================== garde de source

        [Test]
        public void OnlyTheCompilerConstructsARoadModelVersion()
        {
            // Litteral scinde pour que ce fichier de test ne soit pas lui-meme un faux positif.
            string needle = "new " + "RoadModelVersion(";
            var offenders = new List<string>();

            foreach (var path in Directory.GetFiles(FeaturesRootPath, "*.cs", SearchOption.AllDirectories))
            {
                if (File.ReadAllText(path).Contains(needle))
                {
                    offenders.Add(path.Replace('\\', '/'));
                }
            }

            Assert.That(offenders.Count, Is.EqualTo(1),
                "Un seul emetteur attendu, trouve : " + string.Join(", ", offenders.ToArray()));
            Assert.That(offenders[0], Does.EndWith("Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCompiler.cs"));
        }

        [Test]
        public void TheTrafficV2CodeDoesNotReintroduceTheV1HierarchyOrderIdentityPatterns()
        {
            string[] forbidden = { "NearestNodeIndex", "Rebuild()", "GetSiblingIndex", "NetworkObjectId" };

            foreach (var path in Directory.GetFiles("Assets/RoadRage/Features/Vehicles/Traffic", "*.cs", SearchOption.AllDirectories))
            {
                foreach (var line in File.ReadAllLines(path))
                {
                    // Les commentaires nomment ces patrons pour dire qu'on ne les reproduit pas :
                    // la garde porte sur le code, pas sur sa documentation.
                    string trimmed = line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("*", StringComparison.Ordinal)
                        || trimmed.StartsWith("/*", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    for (int i = 0; i < forbidden.Length; i++)
                    {
                        Assert.That(trimmed, Does.Not.Contain(forbidden[i]),
                            "Patron V1 reintroduit dans " + path + " : " + forbidden[i]);
                    }
                }
            }
        }

        // ================================================================== coupe transversale (AD-48)

        /// <summary>
        /// Le repere transversal d'une section est porte par exactement un corridor datum. Zero et
        /// deux sont deux defauts distincts, donc deux codes distincts : un rapport de migration
        /// (5.27) doit pouvoir nommer lequel s'est produit.
        /// </summary>
        [Test]
        public void ASectionCarriesExactlyOneCrossSectionDatum()
        {
            var noDatum = AssertHardFailure(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].IsCrossSectionDatum = false; },
                RoadModelValidationCode.MissingCrossSectionDatum);
            Assert.That(ContainsIssue(noDatum, RoadModelValidationCode.MissingCrossSectionDatum, SectionS1), Is.True,
                "L'absence de datum se rapporte sur la section, seule porteuse de la coupe.");

            var twoData = AssertHardFailure(
                delegate(RoadModelSource source) { source.Corridors[CorridorA2Index].IsCrossSectionDatum = true; },
                RoadModelValidationCode.MultipleCrossSectionData);
            Assert.That(ContainsIssue(twoData, RoadModelValidationCode.MultipleCrossSectionData, SectionS1), Is.True);
        }

        /// <summary>
        /// Deux corridors d'une meme section ne peuvent pas occuper la meme position transversale :
        /// l'ordre ne serait plus total et deux consommateurs pourraient les classer differemment.
        /// </summary>
        [Test]
        public void LateralOrderIsUniqueWithinASection()
        {
            var exception = AssertHardFailure(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1BIndex].LateralOrder = 0; },
                RoadModelValidationCode.DuplicateLateralOrder);

            Assert.That(ContainsIssue(exception, RoadModelValidationCode.DuplicateLateralOrder, CorridorA1B), Is.True,
                "Le doublon se rapporte sur le corridor fautif, pas sur la section.");

            Assert.That(exception.HasCode(RoadModelValidationCode.NonContiguousLateralOrder), Is.False,
                "Un ordre duplique rend la contiguite indefinie : ne pas empiler un second motif sur le meme defaut.");
        }

        /// <summary>
        /// Une coupe transversale va de 0 a n-1 sans trou. Un trou ou une valeur negative signifie
        /// qu'une voie manque ou qu'un ordre a ete invente.
        /// </summary>
        [Test]
        public void LateralOrderIsContiguousFromZero()
        {
            var gap = AssertHardFailure(
                delegate(RoadModelSource source) { source.Corridors[CorridorA2Index].LateralOrder = 9; },
                RoadModelValidationCode.NonContiguousLateralOrder);
            Assert.That(ContainsIssue(gap, RoadModelValidationCode.NonContiguousLateralOrder, SectionS1), Is.True);

            AssertHardFailure(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].LateralOrder = -1; },
                RoadModelValidationCode.NonContiguousLateralOrder);
        }

        /// <summary>
        /// Seule collection inverse porteuse de sens : elle suit l'ordre transversal authore, pas
        /// l'identifiant opaque. C'est exactement la contradiction d'AD-43 que la 5.25 laissait
        /// ouverte et qu'AD-48 tranche.
        /// </summary>
        [Test]
        public void CorridorsInSectionFollowLateralOrderNotIdentity()
        {
            var model = RoadModelCompiler.Compile(BuildModel());

            Assert.That(model.GetCorridorsInSection(SectionS1),
                Is.EqualTo(new[] { CorridorA1, CorridorA1B, CorridorA2, CorridorD1, CorridorE1, CorridorD2 }));

            // Re-authoring geometriquement coherent (5.26) : datum D2, dont la droite est -x, donc
            // l'ordre croissant court de D2 vers A1 -- a rebours de l'ordre des RoadId.
            var reordered = BuildModel();
            reordered.Corridors[CorridorA1Index].IsCrossSectionDatum = false;
            reordered.Corridors[CorridorD2Index].IsCrossSectionDatum = true;
            reordered.Corridors[CorridorD2Index].LateralOrder = 0;
            reordered.Corridors[CorridorE1Index].LateralOrder = 1;
            reordered.Corridors[CorridorD1Index].LateralOrder = 2;
            reordered.Corridors[CorridorA2Index].LateralOrder = 3;
            reordered.Corridors[CorridorA1BIndex].LateralOrder = 4;
            reordered.Corridors[CorridorA1Index].LateralOrder = 5;

            Assert.That(RoadModelCompiler.Compile(reordered).GetCorridorsInSection(SectionS1),
                Is.EqualTo(new[] { CorridorD2, CorridorE1, CorridorD1, CorridorA2, CorridorA1B, CorridorA1 }),
                "Le RoadId n'est qu'un depart d'egalite : il ne porte aucune semantique de voie.");
        }

        // ================================================================== contrat de persistance RoadId

        /// <summary>
        /// <see cref="RoadId"/> est un <c>readonly struct</c>, donc non serialisable champ par champ
        /// par Unity : la Story 5.27 persistera la forme hexadecimale. Ce contrat d'aller-retour est
        /// ce dont elle depend, et rien ne le verifiait.
        /// </summary>
        [Test]
        public void RoadIdRoundTripsThroughItsHexadecimalForm()
        {
            var ids = new[] { ModelId, SectionS1, CorridorE1, RoadId.None, new RoadId(ulong.MaxValue, 0UL) };

            for (int i = 0; i < ids.Length; i++)
            {
                string text = ids[i].ToString();
                Assert.That(text.Length, Is.EqualTo(32), "Forme persistee : 32 caracteres hexadecimaux.");

                RoadId parsed;
                Assert.That(RoadId.TryParse(text, out parsed), Is.True, "Aller-retour refuse pour " + text + ".");
                Assert.That(parsed, Is.EqualTo(ids[i]));
                Assert.That(RoadId.Parse(text), Is.EqualTo(ids[i]));
            }
        }

        /// <summary>
        /// Les rejets comptent autant que l'aller-retour : une forme invalide acceptee produirait une
        /// identite silencieusement fausse au chargement.
        /// </summary>
        [Test]
        public void RoadIdRejectsEveryMalformedHexadecimalForm()
        {
            string valid = CorridorE1.ToString();

            var rejected = new[]
            {
                null,
                string.Empty,
                valid.Substring(0, 31),
                valid + "0",
                valid.ToUpperInvariant(),
                valid.Substring(0, 31) + "g",
                valid.Substring(0, 31) + " "
            };

            for (int i = 0; i < rejected.Length; i++)
            {
                RoadId parsed;
                Assert.That(RoadId.TryParse(rejected[i], out parsed), Is.False,
                    "Forme invalide acceptee : " + (rejected[i] ?? "<null>"));
                Assert.That(parsed, Is.EqualTo(RoadId.None), "Un echec de TryParse rend RoadId.None.");

                string candidate = rejected[i];
                Assert.Throws<FormatException>(delegate { RoadId.Parse(candidate); });
            }
        }

        private static bool ContainsIssue(RoadModelCompilationException exception, RoadModelValidationCode code, RoadId subject)
        {
            for (int i = 0; i < exception.Issues.Count; i++)
            {
                if (exception.Issues[i].Code == code && exception.Issues[i].SubjectId == subject)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
