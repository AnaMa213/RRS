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
        /// </summary>
        public const int CompilerSchemaVersion = 4;

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
            payload.SchemaVersion = CompilerSchemaVersion;
            payload.ModelId = source.ModelId;
            payload.ValidationProfile = source.ValidationProfile;
            payload.LocalizationProfile = source.LocalizationProfile;
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

            ulong high;
            ulong low;
            RoadModelCanonicalWriter.ComputeFingerprint(payload, out high, out low);

            var version = new RoadModelVersion(CompilerSchemaVersion, high, low);

            return new CompiledRoadModel(
                source.ModelId,
                version,
                source.ValidationProfile,
                source.LocalizationProfile,
                sections,
                corridors,
                connections,
                adjacencies,
                junctions,
                movements,
                controls,
                conflictZones,
                signalPlans,
                portals);
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
