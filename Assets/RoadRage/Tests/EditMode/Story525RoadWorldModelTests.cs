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

        // Index de tableau du modele synthetique, stables par construction (voir BuildModel).
        private const int CorridorA1Index = 0;
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
        /// </summary>
        private static RoadModelSource BuildModel()
        {
            var source = new RoadModelSource();
            source.ModelId = ModelId;
            source.Label = "modele synthetique 5.25";
            source.ValidationProfile = Profile();

            source.Sections = new[] { Section(SectionS1, "S1", 13.9f) };

            source.Corridors = new[]
            {
                Corridor(CorridorA1, "A1", 20f),
                Corridor(CorridorA1B, "A1b", 20f),
                Corridor(CorridorA2, "A2", 20f),
                Corridor(CorridorD1, "D1", 20f),
                Corridor(CorridorD2, "D2", 20f),
                Corridor(CorridorE1, "E1", 20f)
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
                Movement(MovementM1, "M1", CorridorA1, CorridorD1),
                Movement(MovementM2, "M2", CorridorA1, CorridorD2),
                Movement(MovementM3, "M3", CorridorA2, CorridorD1),
                Movement(MovementM4, "M4", CorridorA2, CorridorD2)
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
                MakePortal(PortalExit, "sortie", CorridorE1, PortalRole.Exit, 20f)
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
            profile.LocalizationScoreBandMeters = 0.15f;
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

        private static LaneCorridor Corridor(RoadId id, string label, float length)
        {
            var corridor = new LaneCorridor();
            corridor.Id = id;
            corridor.Label = label;
            corridor.SectionId = SectionS1;
            corridor.Samples = Samples(length);
            corridor.LengthMeters = length;
            return corridor;
        }

        private static JunctionMovement Movement(RoadId id, string label, RoadId from, RoadId to)
        {
            var movement = new JunctionMovement();
            movement.Id = id;
            movement.Label = label;
            movement.JunctionId = JunctionJ;
            movement.FromCorridorId = from;
            movement.ToCorridorId = to;
            movement.Samples = Samples(9f);
            movement.LengthMeters = 9f;
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

        private static RoadCurveSample[] Samples(float length)
        {
            return new[]
            {
                Sample(0f, new Vector3(0f, 0f, 0f)),
                Sample(length * 0.5f, new Vector3(0f, 0f, length * 0.5f)),
                Sample(length, new Vector3(0f, 0f, length))
            };
        }

        private static RoadCurveSample Sample(float s, Vector3 position)
        {
            var sample = new RoadCurveSample();
            sample.SMeters = s;
            sample.Position = position;
            sample.Tangent = new Vector3(0f, 0f, 1f);
            sample.Up = new Vector3(0f, 1f, 0f);
            sample.CurvaturePerMeter = 0f;
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

            JunctionControl control;
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
        public void EveryBehaviorAffectingChangeMovesTheVersion()
        {
            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].Samples[1].HalfWidthRightMeters = 2.5f; },
                "Largeur utile : comportemental.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Connections[0].FromCorridorId = CorridorD2; },
                "Topologie : comportemental.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Movements[MovementM1Index].ToCorridorId = CorridorD2; },
                "Mouvement : comportemental.");

            AssertVersionChanges(
                delegate(RoadModelSource source)
                {
                    source.Controls[ControlC1Index].Kind = JunctionControlKind.Stop;
                    source.Controls[ControlC2Index].Kind = JunctionControlKind.Stop;
                },
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
                delegate(RoadModelSource source) { source.ValidationProfile.LocalizationScoreBandMeters = 0.3f; },
                "Profil : bande d'hysteresis de localisation.");

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

            // Section rattachee : compare « S2 ajoutee mais vide » a « S2 ajoutee et E1 dessus »,
            // pour que seul le champ SectionId du corridor differe entre les deux charges.
            var withUnusedSection = VersionOf(AddSecondSection);
            var withCorridorMoved = VersionOf(delegate(RoadModelSource source)
            {
                AddSecondSection(source);
                source.Corridors[5].SectionId = SectionS2;
            });
            Assert.That(withCorridorMoved, Is.Not.EqualTo(withUnusedSection),
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

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].LengthMeters = 21f; },
                "Longueur de corridor.");

            // ---------------------------------------------------------- echantillons de courbe
            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].Samples[1].SMeters = 11f; },
                "Abscisse curviligne d'un echantillon.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].Samples[1].Tangent = new Vector3(0.1f, 0f, 0.99f); },
                "Tangente d'un echantillon.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].Samples[1].Up = new Vector3(0f, 0.9f, 0.1f); },
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

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Connections[0].ToCorridorId = CorridorD2; },
                "Corridor d'arrivee d'une connexion.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Adjacencies[0].Permission = LaneChangePermission.Forbidden; },
                "Legalite de changement de file.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Adjacencies[0].Side = LaneSide.Left; },
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

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Movements[MovementM1Index].FromCorridorId = CorridorA2; },
                "Corridor d'approche d'un mouvement.");

            AssertVersionChanges(
                delegate(RoadModelSource source) { source.Movements[MovementM1Index].LengthMeters = 10f; },
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
                delegate(RoadModelSource source, float delta) { source.ValidationProfile.WrongWayHeadingDegrees = 90f + delta; },
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
            // enormes s'effondreraient sur la meme version.
            var exception = AssertHardFailure(
                delegate(RoadModelSource source) { source.Corridors[CorridorA1Index].LengthMeters = 1e30f; },
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
        public void EachMovementReadsItsKindFromItsSingleAuthoritativeControlBinding()
        {
            var source = BuildModel();
            source.Controls[ControlC2Index].Kind = JunctionControlKind.Yield;
            source.SignalPlans[0].Groups = new[] { Group(GroupG1, new[] { MovementM1, MovementM2 }) };
            source.SignalPlans[0].Phases = new[]
            {
                PhaseForSingleGroup(PhasePh1, 30f, SignalState.Green),
                PhaseForSingleGroup(PhasePh2, 30f, SignalState.Red)
            };

            var model = RoadModelCompiler.Compile(source);

            JunctionControl m1Control;
            JunctionControl m4Control;
            Assert.That(model.TryGetControlForMovement(MovementM1, out m1Control), Is.True);
            Assert.That(model.TryGetControlForMovement(MovementM4, out m4Control), Is.True);
            Assert.That(m1Control.Kind, Is.EqualTo(JunctionControlKind.Signalized));
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

                    var duplicate = Movement(MovementM1, "M1-bis", CorridorA1, CorridorD1);
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

        // ================================================================== helpers

        private static SignalPhase PhaseForSingleGroup(RoadId phaseId, float duration, SignalState state)
        {
            var phase = new SignalPhase();
            phase.PhaseId = phaseId;
            phase.DurationSeconds = duration;
            phase.GroupStates = new[] { GroupState(GroupG1, state) };
            return phase;
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
