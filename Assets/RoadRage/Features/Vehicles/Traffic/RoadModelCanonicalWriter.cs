using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic
{
    /// <summary>
    /// Charge canonique comportementale soumise au serialiseur. Remplie par
    /// <see cref="RoadModelCompiler"/> avec les enregistrements deja resolus : corridors en valeurs
    /// effectives, defauts de section appliques.
    /// </summary>
    public struct RoadModelCanonicalPayload
    {
        public int SchemaVersion;
        public RoadId ModelId;
        public RoadModelValidationProfile ValidationProfile;
        public RoadSection[] Sections;
        public EffectiveLaneCorridor[] Corridors;
        public LaneConnection[] Connections;
        public LaneAdjacency[] Adjacencies;
        public Junction[] Junctions;
        public JunctionMovement[] Movements;
        public JunctionControl[] Controls;
        public ConflictZone[] ConflictZones;
        public SignalPlan[] SignalPlans;
        public Portal[] Portals;
    }

    /// <summary>
    /// Serialiseur canonique ordre-independant du Road World Model.
    ///
    /// Trois regles fixent le determinisme, et les trois font semantiquement partie de
    /// <see cref="RoadModelCompiler.CompilerSchemaVersion"/> : les changer rend incomparables
    /// toutes les versions deja emises et impose donc d'incrementer le schema.
    ///
    /// 1. <b>Ordre.</b> Chaque ensemble est trie par <see cref="RoadId"/> ordinal avant ecriture,
    ///    y compris les appartenances possedees (mouvements d'un controle, membres d'une zone de
    ///    conflit, groupes d'un plan, etats de groupe d'une phase). Seules restent dans leur ordre
    ///    d'ecriture les sequences dont l'ordre EST la semantique : les echantillons de courbe
    ///    (abscisse strictement croissante, garantie par le validateur) et les phases ordonnees
    ///    d'un <see cref="SignalPlan"/>.
    /// 2. <b>Exclusions.</b> Libelles, provenance (<see cref="SourceTrace"/>,
    ///    <see cref="ImportManifest"/>, tombstones, remaps), metadonnees editeur, ordre des
    ///    enregistrements et index reconstructibles ne sont jamais ecrits.
    /// 3. <b>Numerique.</b> Valeurs finies uniquement -- un <c>NaN</c> ou un <c>Infinity</c> est un
    ///    echec dur, jamais une valeur encodee ; <c>-0</c> est normalise en <c>+0</c> ; chaque
    ///    grandeur declare son unite, son pas de quantification et sa regle d'arrondi
    ///    (<see cref="MidpointRounding.AwayFromZero"/>) puis est ecrite comme entier signe.
    ///
    /// L'empreinte est un SHA-256 de la charge, tronque a ses 128 premiers bits.
    /// </summary>
    public static class RoadModelCanonicalWriter
    {
        /// <summary>Marqueur de format : toute evolution passe par la version de schema.</summary>
        private const string FormatTag = "RRS-ROADMODEL-CANONICAL";

        /// <summary>Longueurs et positions, en metres. Pas : 1 mm.</summary>
        public const double MeterStep = 0.001d;

        /// <summary>Vitesses, en metres par seconde. Pas : 1 mm/s.</summary>
        public const double SpeedStep = 0.001d;

        /// <summary>Angles, en degres. Pas : 0,01 degre.</summary>
        public const double DegreeStep = 0.01d;

        /// <summary>Durees, en secondes. Pas : 1 ms.</summary>
        public const double SecondStep = 0.001d;

        /// <summary>Courbure signee, en 1/m. Pas : 1e-5 /m.</summary>
        public const double CurvatureStep = 0.00001d;

        /// <summary>Composantes de direction unitaire, sans unite. Pas : 1e-6.</summary>
        public const double DirectionStep = 0.000001d;

        /// <summary>Grandeurs sans unite (poids, ratios). Pas : 1e-4.</summary>
        public const double RatioStep = 0.0001d;

        /// <summary>
        /// Calcule l'empreinte 128 bits de la charge canonique. Ne construit aucune
        /// <see cref="RoadModelVersion"/> : seul <see cref="RoadModelCompiler"/> en emet une.
        /// </summary>
        /// <exception cref="RoadModelCompilationException">
        /// Une valeur non finie a atteint la charge canonique.
        /// </exception>
        public static void ComputeFingerprint(RoadModelCanonicalPayload payload, out ulong high, out ulong low)
        {
            byte[] digest;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
                {
                    Write(writer, payload);
                }

                using (var sha = SHA256.Create())
                {
                    digest = sha.ComputeHash(stream.ToArray());
                }
            }

            high = ReadBigEndianUInt64(digest, 0);
            low = ReadBigEndianUInt64(digest, 8);
        }

        // ------------------------------------------------------------------ charge

        private static void Write(BinaryWriter writer, RoadModelCanonicalPayload payload)
        {
            writer.Write(FormatTag);
            writer.Write(payload.SchemaVersion);
            WriteId(writer, payload.ModelId);

            // Profil de validation : versionne, donc dans la charge.
            WriteMeters(writer, payload.ValidationProfile.MaxVehicleHalfWidthMeters);
            WriteMeters(writer, payload.ValidationProfile.MaxVehicleLengthMeters);
            WriteMeters(writer, payload.ValidationProfile.LateralClearanceMarginMeters);
            WriteMeters(writer, payload.ValidationProfile.LocalizationScoreBandMeters);
            WriteDegrees(writer, payload.ValidationProfile.WrongWayHeadingDegrees);

            var sections = SortedCopy(payload.Sections, delegate(RoadSection a, RoadSection b) { return a.Id.CompareTo(b.Id); });
            writer.Write(sections.Length);
            for (int i = 0; i < sections.Length; i++)
            {
                // Le libelle est exclu : diagnostic, pas comportement.
                WriteId(writer, sections[i].Id);
                writer.Write((int)sections[i].RoadClass);
                writer.Write((int)sections[i].Surface);
                WriteSpeed(writer, sections[i].DefaultSpeedLimitMetersPerSecond);
                writer.Write((int)sections[i].DefaultAllowedVehicleClasses);
            }

            var corridors = SortedCopy(payload.Corridors, delegate(EffectiveLaneCorridor a, EffectiveLaneCorridor b) { return a.CorridorId.CompareTo(b.CorridorId); });
            writer.Write(corridors.Length);
            for (int i = 0; i < corridors.Length; i++)
            {
                WriteId(writer, corridors[i].CorridorId);
                WriteId(writer, corridors[i].SectionId);
                WriteSpeed(writer, corridors[i].SpeedLimitMetersPerSecond);
                writer.Write((int)corridors[i].Surface);
                writer.Write((int)corridors[i].AllowedVehicleClasses);
                WriteMeters(writer, corridors[i].LengthMeters);
                WriteSamples(writer, corridors[i].Samples);
            }

            var connections = SortedCopy(payload.Connections, delegate(LaneConnection a, LaneConnection b) { return a.Id.CompareTo(b.Id); });
            writer.Write(connections.Length);
            for (int i = 0; i < connections.Length; i++)
            {
                WriteId(writer, connections[i].Id);
                WriteId(writer, connections[i].FromCorridorId);
                WriteId(writer, connections[i].ToCorridorId);
                writer.Write((int)connections[i].Kind);
            }

            var adjacencies = SortedCopy(payload.Adjacencies, delegate(LaneAdjacency a, LaneAdjacency b) { return a.Id.CompareTo(b.Id); });
            writer.Write(adjacencies.Length);
            for (int i = 0; i < adjacencies.Length; i++)
            {
                WriteId(writer, adjacencies[i].Id);
                WriteId(writer, adjacencies[i].FromCorridorId);
                WriteId(writer, adjacencies[i].ToCorridorId);
                writer.Write((int)adjacencies[i].Side);
                WriteMeters(writer, adjacencies[i].FromStartSMeters);
                WriteMeters(writer, adjacencies[i].FromEndSMeters);
                WriteMeters(writer, adjacencies[i].ToStartSMeters);
                WriteMeters(writer, adjacencies[i].ToEndSMeters);
                writer.Write((int)adjacencies[i].Permission);
            }

            var junctions = SortedCopy(payload.Junctions, delegate(Junction a, Junction b) { return a.Id.CompareTo(b.Id); });
            writer.Write(junctions.Length);
            for (int i = 0; i < junctions.Length; i++)
            {
                WriteId(writer, junctions[i].Id);
                writer.Write((int)junctions[i].Feature);
                WritePosition(writer, junctions[i].Boundary.Center);
                WritePosition(writer, junctions[i].Boundary.Extents);
            }

            var movements = SortedCopy(payload.Movements, delegate(JunctionMovement a, JunctionMovement b) { return a.Id.CompareTo(b.Id); });
            writer.Write(movements.Length);
            for (int i = 0; i < movements.Length; i++)
            {
                WriteId(writer, movements[i].Id);
                WriteId(writer, movements[i].JunctionId);
                WriteId(writer, movements[i].FromCorridorId);
                WriteId(writer, movements[i].ToCorridorId);
                WriteMeters(writer, movements[i].LengthMeters);
                WriteRatio(writer, movements[i].RoutePreferenceWeight);
                WriteSamples(writer, movements[i].Samples);
            }

            var controls = SortedCopy(payload.Controls, delegate(JunctionControl a, JunctionControl b) { return a.Id.CompareTo(b.Id); });
            writer.Write(controls.Length);
            for (int i = 0; i < controls.Length; i++)
            {
                WriteId(writer, controls[i].Id);
                WriteId(writer, controls[i].JunctionId);
                writer.Write((int)controls[i].Kind);
                WriteIdSet(writer, controls[i].ControlledMovementIds);
                writer.Write(controls[i].HasStopLine);
                if (controls[i].HasStopLine)
                {
                    WritePosition(writer, controls[i].StopLine.Start);
                    WritePosition(writer, controls[i].StopLine.End);
                }
            }

            var zones = SortedCopy(payload.ConflictZones, delegate(ConflictZone a, ConflictZone b) { return a.Id.CompareTo(b.Id); });
            writer.Write(zones.Length);
            for (int i = 0; i < zones.Length; i++)
            {
                WriteId(writer, zones[i].Id);
                WriteId(writer, zones[i].JunctionId);
                WritePosition(writer, zones[i].Volume.Center);
                WritePosition(writer, zones[i].Volume.Extents);
                WriteIdSet(writer, zones[i].MemberMovementIds);
            }

            var plans = SortedCopy(payload.SignalPlans, delegate(SignalPlan a, SignalPlan b) { return a.Id.CompareTo(b.Id); });
            writer.Write(plans.Length);
            for (int i = 0; i < plans.Length; i++)
            {
                WriteId(writer, plans[i].Id);
                WriteId(writer, plans[i].JunctionId);

                var groups = SortedCopy(plans[i].Groups, delegate(SignalGroup a, SignalGroup b) { return a.GroupId.CompareTo(b.GroupId); });
                writer.Write(groups.Length);
                for (int g = 0; g < groups.Length; g++)
                {
                    WriteId(writer, groups[g].GroupId);
                    WriteIdSet(writer, groups[g].MemberMovementIds);
                }

                // Les phases gardent leur ordre authore : la sequence EST la semantique.
                var phases = plans[i].Phases ?? new SignalPhase[0];
                writer.Write(phases.Length);
                for (int p = 0; p < phases.Length; p++)
                {
                    WriteId(writer, phases[p].PhaseId);
                    WriteSeconds(writer, phases[p].DurationSeconds);

                    var states = SortedCopy(phases[p].GroupStates, delegate(SignalGroupState a, SignalGroupState b) { return a.GroupId.CompareTo(b.GroupId); });
                    writer.Write(states.Length);
                    for (int s = 0; s < states.Length; s++)
                    {
                        WriteId(writer, states[s].GroupId);
                        writer.Write((int)states[s].State);
                    }
                }
            }

            var portals = SortedCopy(payload.Portals, delegate(Portal a, Portal b) { return a.Id.CompareTo(b.Id); });
            writer.Write(portals.Length);
            for (int i = 0; i < portals.Length; i++)
            {
                WriteId(writer, portals[i].Id);
                WriteId(writer, portals[i].CorridorId);
                writer.Write((int)portals[i].Role);
                WriteMeters(writer, portals[i].SMeters);
                WriteMeters(writer, portals[i].EnvelopeLengthMeters);
                WriteMeters(writer, portals[i].EnvelopeHalfWidthMeters);
            }
        }

        private static void WriteSamples(BinaryWriter writer, RoadCurveSample[] samples)
        {
            var values = samples ?? new RoadCurveSample[0];
            writer.Write(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                WriteMeters(writer, values[i].SMeters);
                WritePosition(writer, values[i].Position);
                WriteDirection(writer, values[i].Tangent);
                WriteDirection(writer, values[i].Up);
                WriteCurvature(writer, values[i].CurvaturePerMeter);
                WriteMeters(writer, values[i].HalfWidthLeftMeters);
                WriteMeters(writer, values[i].HalfWidthRightMeters);
            }
        }

        private static void WriteIdSet(BinaryWriter writer, RoadId[] ids)
        {
            var values = SortedCopy(ids, delegate(RoadId a, RoadId b) { return a.CompareTo(b); });
            writer.Write(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                WriteId(writer, values[i]);
            }
        }

        private static void WriteId(BinaryWriter writer, RoadId id)
        {
            writer.Write(id.High);
            writer.Write(id.Low);
        }

        // ------------------------------------------------------------------ numerique

        private static void WriteMeters(BinaryWriter writer, float value)
        {
            writer.Write(Quantize(value, MeterStep, "metres"));
        }

        private static void WriteSpeed(BinaryWriter writer, float value)
        {
            writer.Write(Quantize(value, SpeedStep, "metres par seconde"));
        }

        private static void WriteDegrees(BinaryWriter writer, float value)
        {
            writer.Write(Quantize(value, DegreeStep, "degres"));
        }

        private static void WriteSeconds(BinaryWriter writer, float value)
        {
            writer.Write(Quantize(value, SecondStep, "secondes"));
        }

        private static void WriteCurvature(BinaryWriter writer, float value)
        {
            writer.Write(Quantize(value, CurvatureStep, "1/m"));
        }

        private static void WriteRatio(BinaryWriter writer, float value)
        {
            writer.Write(Quantize(value, RatioStep, "sans unite"));
        }

        private static void WritePosition(BinaryWriter writer, Vector3 value)
        {
            WriteMeters(writer, value.x);
            WriteMeters(writer, value.y);
            WriteMeters(writer, value.z);
        }

        private static void WriteDirection(BinaryWriter writer, Vector3 value)
        {
            writer.Write(Quantize(value.x, DirectionStep, "sans unite"));
            writer.Write(Quantize(value.y, DirectionStep, "sans unite"));
            writer.Write(Quantize(value.z, DirectionStep, "sans unite"));
        }

        /// <summary>
        /// Magnitude quantifiee maximale encodable. Sous <c>long.MaxValue</c> avec une marge qui
        /// couvre l'arrondi : au-dela, la conversion en entier saturerait et deux modeles distincts
        /// s'effondreraient sur la meme version.
        /// </summary>
        private const double MaxQuantizedMagnitude = 9.0e18d;

        /// <summary>
        /// Normalise puis quantifie une grandeur finie en entier signe. Jamais de quantification
        /// silencieuse : une valeur non finie, ou finie mais hors du domaine encodable, est un
        /// echec dur.
        /// </summary>
        private static long Quantize(float value, double step, string unit)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new RoadModelCompilationException(new[]
                {
                    new RoadModelValidationIssue(
                        RoadModelValidationCode.NonFiniteNumericValue,
                        RoadId.None,
                        "Valeur non finie (" + unit + ") atteignant la charge canonique.")
                });
            }

            // -0 et +0 sont la meme grandeur : sans cette normalisation, deux modeles identiques
            // hasheraient differemment sur un signe invisible.
            double normalized = value == 0f ? 0d : (double)value;
            double scaled = normalized / step;

            // La conversion en long est non verifiee : sans cette garde, une valeur finie mais
            // enorme saturerait en silence et deux modeles differents partageraient une version.
            if (scaled > MaxQuantizedMagnitude || scaled < -MaxQuantizedMagnitude)
            {
                throw new RoadModelCompilationException(new[]
                {
                    new RoadModelValidationIssue(
                        RoadModelValidationCode.NumericValueOutOfRange,
                        RoadId.None,
                        "Valeur (" + unit + ") hors du domaine quantifiable : "
                        + value.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".")
                });
            }

            return (long)Math.Round(scaled, MidpointRounding.AwayFromZero);
        }

        // ------------------------------------------------------------------ helpers

        private static T[] SortedCopy<T>(T[] values, Comparison<T> comparison)
        {
            if (values == null || values.Length == 0)
            {
                return new T[0];
            }

            var copy = (T[])values.Clone();
            Array.Sort(copy, comparison);
            return copy;
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
    }
}
