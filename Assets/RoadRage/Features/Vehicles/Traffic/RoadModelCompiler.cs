using System;
using System.Collections.Generic;

namespace RoadRage.Features.Vehicles.Traffic
{
    /// <summary>
    /// Point d'entree unique du Road World Model : valide, resout les defauts de section, indexe et
    /// emet la version.
    ///
    /// <b>Sans etat.</b> Classe statique sans aucun champ : le compilateur ne connait que le
    /// <see cref="RoadModelSource"/> qu'on lui passe, ne se souvient d'aucun import precedent,
    /// n'ecrit rien sur disque et rend la meme sortie pour la meme entree. La stabilite d'identite
    /// au re-import (AD-44) est une exigence du pipeline d'import (Story 5.27), pas du compilateur :
    /// lignee, tombstones et remaps sont ici valides <i>tels que fournis</i>, jamais crees ni
    /// reconcilies contre un import precedent.
    ///
    /// <b>Seul emetteur de <see cref="RoadModelVersion"/>.</b> Le constructeur de
    /// <see cref="RoadModelVersion"/> est <c>internal</c> et n'est appele que d'ici.
    /// </summary>
    public static class RoadModelCompiler
    {
        /// <summary>
        /// Version du schema de compilation. Elle couvre le contrat d'encodage canonique dans son
        /// ensemble : ordre de tri, exclusions et regles numeriques (finitude, normalisation de
        /// <c>-0</c>, unites/pas/arrondi). Changer l'un de ces choix rend incomparables toutes les
        /// versions deja emises et impose d'incrementer cette constante.
        ///
        /// Regle (5.26) : changer la VALEUR d'un champ de profil change la version, jamais le schema ;
        /// seul un changement de representation ou de sens incremente le schema. 3 : le profil de
        /// localisation sort du profil de validation et les tolerances geometriques y entrent.
        /// 4 : le seuil d'ancrage au datum (AD-48) entre dans le profil de validation ; il etait une
        /// constante du validateur, donc absent de toute charge canonique.
        /// 5 (Story 5.53) : genre de chaque zone de conflit et abscisses de debut de contact par membre.
        /// </summary>
        public const int CompilerSchemaVersion = 5;

        /// <summary>
        /// Plus ancien schema encore compilable : un document historique signe (schema 4) se relit et redonne sa
        /// version liee. Sa charge n'encode ni genre ni abscisse ; le validateur refuse qu'une source 4 en porte.
        /// </summary>
        public const int MinimumReadableSchemaVersion = 4;

        /// <summary>Schema effectif d'une source : le sien s'il est fixe, sinon le schema courant.</summary>
        public static int SchemaVersionOf(RoadModelSource source)
        {
            return source.CompilerSchemaVersion == 0 ? CompilerSchemaVersion : source.CompilerSchemaVersion;
        }

        /// <summary>Braquage disponible a une vitesse donnee, interpolation lineaire bornee.</summary>
        public static float AvailableLockDegrees(DrivabilityProfile profile, float speedMetersPerSecond)
        {
            float t = Math.Max(0f, Math.Min(1f, Math.Abs(speedMetersPerSecond) / profile.FullReductionSpeedMetersPerSecond));
            return profile.LowSpeedLockDegrees + (profile.HighSpeedLockDegrees - profile.LowSpeedLockDegrees) * t;
        }

        /// <summary>Rayon du point de reference pour un angle de braquage.</summary>
        public static float RadiusMeters(DrivabilityProfile profile, float lockDegrees)
        {
            double radians = lockDegrees * Math.PI / 180d;
            double rearRadius = profile.WheelbaseMeters / Math.Tan(radians);
            return (float)Math.Sqrt(rearRadius * rearRadius
                + profile.ReferencePointAheadRearAxleMeters * profile.ReferencePointAheadRearAxleMeters);
        }

        /// <summary>Rayon minimal admis a la vitesse minimale ou la direction est active.</summary>
        public static float AdmissionRadiusMeters(DrivabilityProfile profile)
        {
            return RadiusMeters(profile, AvailableLockDegrees(profile, profile.SteeringInactiveBelowMetersPerSecond));
        }

        /// <summary>
        /// Plafond cinematique pour une courbure absolue. <see cref="float.PositiveInfinity"/>
        /// signifie qu'aucun plafond n'est impose par le braquage. Une geometrie a l'interieur du
        /// point de reference rend <see cref="float.NaN"/> sans tenter de racine invalide.
        /// </summary>
        public static float SteeringSpeedCeilingMetersPerSecond(DrivabilityProfile profile, float curvaturePerMeter)
        {
            float curvature = Math.Abs(curvaturePerMeter);
            if (curvature <= 0f)
            {
                return float.PositiveInfinity;
            }

            double radius = 1d / curvature;
            double offset = profile.ReferencePointAheadRearAxleMeters;
            if (!(radius > offset))
            {
                return float.NaN;
            }

            double rearRadius = Math.Sqrt(radius * radius - offset * offset);
            double required = Math.Atan(profile.WheelbaseMeters / rearRadius) * 180d / Math.PI;
            if (required <= profile.HighSpeedLockDegrees)
            {
                return float.PositiveInfinity;
            }

            if (required > profile.LowSpeedLockDegrees)
            {
                return 0f;
            }

            return (float)(profile.FullReductionSpeedMetersPerSecond
                * (profile.LowSpeedLockDegrees - required)
                / (profile.LowSpeedLockDegrees - profile.HighSpeedLockDegrees));
        }

        /// <summary>
        /// Compile une source en modele immuable versionne.
        /// </summary>
        /// <exception cref="ArgumentNullException">Source absente.</exception>
        /// <exception cref="RoadModelCompilationException">
        /// Echec dur de validation, structurelle ou geometrique (5.26) : aucune sortie compilee
        /// n'est emise et aucune version n'est produite.
        /// </exception>
        public static CompiledRoadModel Compile(RoadModelSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var issues = RoadModelValidator.Validate(source);
            if (issues.Count > 0)
            {
                throw new RoadModelCompilationException(issues);
            }

            var sections = Sorted(source.Sections, delegate(RoadSection a, RoadSection b) { return a.Id.CompareTo(b.Id); });
            var connections = Sorted(source.Connections, delegate(LaneConnection a, LaneConnection b) { return a.Id.CompareTo(b.Id); });
            var adjacencies = Sorted(source.Adjacencies, delegate(LaneAdjacency a, LaneAdjacency b) { return a.Id.CompareTo(b.Id); });
            var junctions = Sorted(source.Junctions, delegate(Junction a, Junction b) { return a.Id.CompareTo(b.Id); });
            var movements = Sorted(source.Movements, delegate(JunctionMovement a, JunctionMovement b) { return a.Id.CompareTo(b.Id); });
            var controls = Sorted(source.Controls, delegate(JunctionControl a, JunctionControl b) { return a.Id.CompareTo(b.Id); });
            var conflictZones = Sorted(source.ConflictZones, delegate(ConflictZone a, ConflictZone b) { return a.Id.CompareTo(b.Id); });
            var signalPlans = Sorted(source.SignalPlans, delegate(SignalPlan a, SignalPlan b) { return a.Id.CompareTo(b.Id); });
            var portals = Sorted(source.Portals, delegate(Portal a, Portal b) { return a.Id.CompareTo(b.Id); });

            var corridors = ResolveEffectiveCorridors(source, sections);

            var payload = new RoadModelCanonicalPayload();
            payload.SchemaVersion = SchemaVersionOf(source);
            payload.ModelId = source.ModelId;
            payload.ValidationProfile = source.ValidationProfile;
            payload.LocalizationProfile = source.LocalizationProfile;
            payload.DrivabilityProfile = source.DrivabilityProfile;
            payload.Sections = sections;
            payload.Corridors = corridors;
            payload.Connections = connections;
            payload.Adjacencies = adjacencies;
            payload.Junctions = junctions;
            payload.Movements = movements;
            payload.Controls = controls;
            payload.ConflictZones = conflictZones;
            payload.SignalPlans = signalPlans;
            payload.Portals = portals;

            byte[] canonicalBytes = RoadModelCanonicalWriter.CreateBytes(payload);
            ulong high;
            ulong low;
            RoadModelCanonicalWriter.ComputeFingerprint(canonicalBytes, out high, out low);

            var version = new RoadModelVersion(payload.SchemaVersion, high, low);

            return new CompiledRoadModel(
                source.ModelId,
                version,
                source.ValidationProfile,
                source.LocalizationProfile,
                source.DrivabilityProfile,
                sections,
                corridors,
                connections,
                adjacencies,
                junctions,
                movements,
                controls,
                conflictZones,
                signalPlans,
                portals,
                canonicalBytes);
        }

        /// <summary>
        /// Applique les defauts de section aux corridors. Un defaut ne transfere aucune propriete :
        /// le corridor reste l'unite geometrique et dirigee, la section ne fournit qu'une valeur.
        /// </summary>
        private static EffectiveLaneCorridor[] ResolveEffectiveCorridors(RoadModelSource source, RoadSection[] sections)
        {
            var sectionById = new Dictionary<RoadId, RoadSection>(sections.Length);
            for (int i = 0; i < sections.Length; i++)
            {
                sectionById[sections[i].Id] = sections[i];
            }

            var authored = Sorted(source.Corridors, delegate(LaneCorridor a, LaneCorridor b) { return a.Id.CompareTo(b.Id); });
            var effective = new EffectiveLaneCorridor[authored.Length];
            for (int i = 0; i < authored.Length; i++)
            {
                var corridor = authored[i];
                var section = sectionById[corridor.SectionId];

                effective[i] = new EffectiveLaneCorridor();
                effective[i].CorridorId = corridor.Id;
                effective[i].SectionId = corridor.SectionId;
                effective[i].LengthMeters = corridor.LengthMeters;
                effective[i].Samples = corridor.Samples ?? new RoadCurveSample[0];
                effective[i].SpeedLimitMetersPerSecond = corridor.HasSpeedLimitOverride
                    ? corridor.SpeedLimitOverrideMetersPerSecond
                    : section.DefaultSpeedLimitMetersPerSecond;
                effective[i].Surface = corridor.HasSurfaceOverride ? corridor.SurfaceOverride : section.Surface;
                effective[i].LateralOrder = corridor.LateralOrder;
                effective[i].IsCrossSectionDatum = corridor.IsCrossSectionDatum;
                effective[i].AllowedVehicleClasses = corridor.HasAllowedVehicleClassesOverride
                    ? corridor.AllowedVehicleClassesOverride
                    : section.DefaultAllowedVehicleClasses;
            }

            return effective;
        }

        private static T[] Sorted<T>(T[] values, Comparison<T> comparison)
        {
            if (values == null || values.Length == 0)
            {
                return new T[0];
            }

            var copy = (T[])values.Clone();
            Array.Sort(copy, comparison);
            return copy;
        }
    }
}
