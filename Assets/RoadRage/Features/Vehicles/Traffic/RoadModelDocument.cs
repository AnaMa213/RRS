using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic
{
    /// <summary>
    /// Provenance d'un <see cref="RoadModelSource"/> persiste (Story 5.28) : le modele est un
    /// artefact DERIVE (import V1 + lignee + decisions d'authoring), lie a ses entrees par hashes.
    /// Donnee de liaison seulement : aucune de ces valeurs n'entre dans <see cref="RoadModelVersion"/>.
    /// </summary>
    public struct RoadModelProvenance
    {
        public string SourceHash;
        public string LineageHash;
        public string DecisionsHash;
        public int ImporterVersion;
        public int PipelineVersion;
    }

    /// <summary>
    /// Serialisation JSON deterministe du <see cref="RoadModelSource"/> complet (Story 5.28) :
    /// identifiants en hexadecimal, enums par nom, un objet de tableau par ligne, LF. Le chargeur est
    /// fail-closed : forme non canonique, corps modifie, echec de compilation ou version divergente
    /// = refus, jamais reparation. Runtime (hors <c>UNITY_EDITOR</c>) : c'est le chargeur du modele.
    /// </summary>
    public static class RoadModelDocument
    {
        public const int Format = 2;

        /// <summary>
        /// Compile la source (seul un modele compilable est persiste) et rend le document. Deux
        /// sources egales et une meme provenance donnent les memes octets.
        /// </summary>
        /// <exception cref="RoadModelCompilationException">Source non compilable : rien n'est produit.</exception>
        public static string Serialize(RoadModelSource source, RoadModelProvenance provenance)
        {
            if (source == null || !source.DrivabilityProfile.Declared)
            {
                throw new FormatException("Document de modele format 2 : profil de conduisibilite non declare.");
            }

            var compiled = RoadModelCompiler.Compile(source);
            var model = ToDto(source);

            var document = new DocumentDto();
            document.Format = Format;
            document.Binding = new BindingDto();
            document.Binding.ModelVersion = compiled.Version.ToString();
            document.Binding.CompilerSchemaVersion = RoadModelCompiler.CompilerSchemaVersion;
            document.Binding.SourceHash = provenance.SourceHash ?? string.Empty;
            document.Binding.LineageHash = provenance.LineageHash ?? string.Empty;
            document.Binding.DecisionsHash = provenance.DecisionsHash ?? string.Empty;
            document.Binding.ImporterVersion = provenance.ImporterVersion;
            document.Binding.PipelineVersion = provenance.PipelineVersion;
            document.Model = model;
            document.Binding.IntegrityHash = IntegrityHash(document);
            return Write(document);
        }

        /// <summary>
        /// Charge un document : parse strict, controle d'integrite, compilation, version liee. Rend la
        /// source VERIFIEE, pas un modele compile : <see cref="RoadModelCompiler.Compile"/> reste le seul
        /// emetteur de <see cref="CompiledRoadModel"/> et de <see cref="RoadModelVersion"/> (5.26), et
        /// recompiler cette source redonne exactement la version liee.
        /// </summary>
        /// <exception cref="FormatException">Document illisible, non canonique, altere ou de version divergente.</exception>
        /// <exception cref="RoadModelCompilationException">Le modele persiste ne compile plus.</exception>
        public static RoadModelSource Load(string text)
        {
            RoadModelProvenance provenance;
            return Load(text, out provenance);
        }

        public static RoadModelSource Load(string text, out RoadModelProvenance provenance)
        {
            if (text == null || text.Trim().Length == 0)
            {
                throw new FormatException("Document de modele vide.");
            }

            DocumentDto document;
            try
            {
                document = JsonUtility.FromJson<DocumentDto>(text);
            }
            catch (ArgumentException exception)
            {
                throw new FormatException("Document de modele illisible : " + exception.Message);
            }

            if (document == null || document.Format != Format || document.Binding == null || document.Model == null)
            {
                throw new FormatException("Document de modele : format absent ou inconnu.");
            }

            // Forme canonique : tout octet hors de la forme ecrite par Serialize (espace, champ
            // absent ou en trop, ordre) est un refus. Le hash d'integrite couvre ensuite toutes les
            // valeurs, liaison et labels compris, que la version ne couvre pas.
            if (Write(document) != text)
            {
                throw new FormatException("Document de modele non canonique : modifie depuis sa generation.");
            }

            if (document.Binding.IntegrityHash != IntegrityHash(document))
            {
                throw new FormatException("Document de modele altere : hash d'integrite different (liaison ou corps modifie).");
            }

            if (document.Binding.CompilerSchemaVersion != RoadModelCompiler.CompilerSchemaVersion)
            {
                throw new FormatException("Document de modele d'un autre schema de compilation ("
                    + document.Binding.CompilerSchemaVersion + ", attendu " + RoadModelCompiler.CompilerSchemaVersion + ").");
            }

            var source = FromDto(document.Model);
            if (!source.DrivabilityProfile.Declared)
            {
                throw new FormatException("Document de modele format 2 : profil de conduisibilite non declare.");
            }
            var compiled = RoadModelCompiler.Compile(source);
            if (compiled.Version.ToString() != document.Binding.ModelVersion)
            {
                throw new FormatException("Version divergente : le document lie " + document.Binding.ModelVersion
                    + ", la recompilation donne " + compiled.Version + ". Chargement refuse, jamais repare.");
            }

            provenance = new RoadModelProvenance();
            provenance.SourceHash = document.Binding.SourceHash;
            provenance.LineageHash = document.Binding.LineageHash;
            provenance.DecisionsHash = document.Binding.DecisionsHash;
            provenance.ImporterVersion = document.Binding.ImporterVersion;
            provenance.PipelineVersion = document.Binding.PipelineVersion;
            return source;
        }

        /// <summary>
        /// Hash d'integrite : chaque champ de liaison (sauf lui-meme) puis le corps. Toute valeur
        /// modifiee, provenance comprise, change ce hash.
        /// </summary>
        private static string IntegrityHash(DocumentDto document)
        {
            var binding = document.Binding;
            string stored = binding.IntegrityHash;
            binding.IntegrityHash = string.Empty;
            string covered = Write(binding) + Write(document.Model);
            binding.IntegrityHash = stored;
            return Sha256Hex(covered);
        }

        public static string Sha256Hex(string text)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(new UTF8Encoding(false).GetBytes(text));
                var hex = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                {
                    hex.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
                }

                return hex.ToString();
            }
        }

        // ================================================================== ecriture

        /// <summary>
        /// JSON compact de <c>JsonUtility</c> (flottants exacts, aller-retour sans perte), puis un saut
        /// de ligne avant chaque objet d'un tableau, hors chaine : une ligne par echantillon ou record,
        /// donc un diff lisible sans le triple volume de l'indentation.
        /// </summary>
        private static string Write(object value)
        {
            string json = JsonUtility.ToJson(value, false);
            var text = new StringBuilder(json.Length + json.Length / 64);
            bool inString = false;
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if (inString)
                {
                    text.Append(c);
                    if (c == '\\' && i + 1 < json.Length)
                    {
                        text.Append(json[++i]);
                    }
                    else if (c == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (c == '"')
                {
                    inString = true;
                }
                else if (c == '{' && i > 0 && (json[i - 1] == '[' || json[i - 1] == ','))
                {
                    text.Append('\n');
                }

                text.Append(c);
            }

            return text.Append('\n').ToString();
        }

        // ================================================================== conversion

        private static ModelDto ToDto(RoadModelSource source)
        {
            var model = new ModelDto();
            model.ModelId = Hex(source.ModelId);
            model.Label = source.Label ?? string.Empty;
            model.ValidationProfile = source.ValidationProfile;
            model.LocalizationProfile = source.LocalizationProfile;
            model.DrivabilityProfile = source.DrivabilityProfile;

            model.Sections = Map(source.Sections, delegate(RoadSection r)
            {
                var d = new SectionDto();
                d.Id = Hex(r.Id);
                d.Label = r.Label ?? string.Empty;
                d.RoadClass = r.RoadClass.ToString();
                d.Surface = r.Surface.ToString();
                d.DefaultSpeedLimitMetersPerSecond = r.DefaultSpeedLimitMetersPerSecond;
                d.DefaultAllowedVehicleClasses = r.DefaultAllowedVehicleClasses.ToString();
                return d;
            });

            model.Corridors = Map(source.Corridors, delegate(LaneCorridor r)
            {
                var d = new CorridorDto();
                d.Id = Hex(r.Id);
                d.Label = r.Label ?? string.Empty;
                d.SectionId = Hex(r.SectionId);
                d.LateralOrder = r.LateralOrder;
                d.IsCrossSectionDatum = r.IsCrossSectionDatum;
                d.Samples = r.Samples ?? new RoadCurveSample[0];
                d.LengthMeters = r.LengthMeters;
                d.HasSpeedLimitOverride = r.HasSpeedLimitOverride;
                d.SpeedLimitOverrideMetersPerSecond = r.SpeedLimitOverrideMetersPerSecond;
                d.HasSurfaceOverride = r.HasSurfaceOverride;
                d.SurfaceOverride = r.SurfaceOverride.ToString();
                d.HasAllowedVehicleClassesOverride = r.HasAllowedVehicleClassesOverride;
                d.AllowedVehicleClassesOverride = r.AllowedVehicleClassesOverride.ToString();
                return d;
            });

            model.Connections = Map(source.Connections, delegate(LaneConnection r)
            {
                var d = new ConnectionDto();
                d.Id = Hex(r.Id);
                d.FromCorridorId = Hex(r.FromCorridorId);
                d.ToCorridorId = Hex(r.ToCorridorId);
                d.Kind = r.Kind.ToString();
                return d;
            });

            model.Adjacencies = Map(source.Adjacencies, delegate(LaneAdjacency r)
            {
                var d = new AdjacencyDto();
                d.Id = Hex(r.Id);
                d.FromCorridorId = Hex(r.FromCorridorId);
                d.ToCorridorId = Hex(r.ToCorridorId);
                d.Side = r.Side.ToString();
                d.FromStartSMeters = r.FromStartSMeters;
                d.FromEndSMeters = r.FromEndSMeters;
                d.ToStartSMeters = r.ToStartSMeters;
                d.ToEndSMeters = r.ToEndSMeters;
                d.Permission = r.Permission.ToString();
                return d;
            });

            model.Junctions = Map(source.Junctions, delegate(Junction r)
            {
                var d = new JunctionDto();
                d.Id = Hex(r.Id);
                d.Label = r.Label ?? string.Empty;
                d.Feature = r.Feature.ToString();
                d.Boundary = r.Boundary;
                return d;
            });

            model.Movements = Map(source.Movements, delegate(JunctionMovement r)
            {
                var d = new MovementDto();
                d.Id = Hex(r.Id);
                d.Label = r.Label ?? string.Empty;
                d.JunctionId = Hex(r.JunctionId);
                d.FromCorridorId = Hex(r.FromCorridorId);
                d.ToCorridorId = Hex(r.ToCorridorId);
                d.Samples = r.Samples ?? new RoadCurveSample[0];
                d.LengthMeters = r.LengthMeters;
                d.RoutePreferenceWeight = r.RoutePreferenceWeight;
                return d;
            });

            model.Controls = Map(source.Controls, delegate(JunctionControl r)
            {
                var d = new ControlDto();
                d.Id = Hex(r.Id);
                d.JunctionId = Hex(r.JunctionId);
                d.Kind = r.Kind.ToString();
                d.ControlledMovementIds = Hexes(r.ControlledMovementIds);
                d.HasStopLine = r.HasStopLine;
                d.StopLine = r.StopLine;
                return d;
            });

            model.ConflictZones = Map(source.ConflictZones, delegate(ConflictZone r)
            {
                var d = new ZoneDto();
                d.Id = Hex(r.Id);
                d.JunctionId = Hex(r.JunctionId);
                d.Volume = r.Volume;
                d.MemberMovementIds = Hexes(r.MemberMovementIds);
                return d;
            });

            model.SignalPlans = Map(source.SignalPlans, delegate(SignalPlan r)
            {
                var d = new PlanDto();
                d.Id = Hex(r.Id);
                d.JunctionId = Hex(r.JunctionId);
                d.Groups = Map(r.Groups, delegate(SignalGroup g)
                {
                    var gd = new GroupDto();
                    gd.GroupId = Hex(g.GroupId);
                    gd.MemberMovementIds = Hexes(g.MemberMovementIds);
                    return gd;
                });
                d.Phases = Map(r.Phases, delegate(SignalPhase p)
                {
                    var pd = new PhaseDto();
                    pd.PhaseId = Hex(p.PhaseId);
                    pd.DurationSeconds = p.DurationSeconds;
                    pd.GroupStates = Map(p.GroupStates, delegate(SignalGroupState s)
                    {
                        var sd = new GroupStateDto();
                        sd.GroupId = Hex(s.GroupId);
                        sd.State = s.State.ToString();
                        return sd;
                    });
                    return pd;
                });
                return d;
            });

            model.Portals = Map(source.Portals, delegate(Portal r)
            {
                var d = new PortalDto();
                d.Id = Hex(r.Id);
                d.Label = r.Label ?? string.Empty;
                d.CorridorId = Hex(r.CorridorId);
                d.Role = r.Role.ToString();
                d.SMeters = r.SMeters;
                d.EnvelopeLengthMeters = r.EnvelopeLengthMeters;
                d.EnvelopeHalfWidthMeters = r.EnvelopeHalfWidthMeters;
                return d;
            });

            model.Manifest = new ManifestDto();
            model.Manifest.Entries = Map(source.Manifest.Entries, delegate(ImportManifestEntry e)
            {
                var d = new EntryDto();
                d.RecordId = Hex(e.RecordId);
                d.Kind = e.Kind.ToString();
                d.ModelId = Hex(e.ModelId);
                d.ImporterSlot = e.ImporterSlot ?? string.Empty;
                d.SourceKeys = e.SourceKeys ?? new SourceTrace[0];
                return d;
            });
            model.Manifest.TombstonedIds = Hexes(source.Manifest.TombstonedIds);
            model.Manifest.Remaps = Map(source.Manifest.Remaps, delegate(RoadIdRemap r)
            {
                var d = new RemapDto();
                d.FromId = Hex(r.FromId);
                d.ToId = Hex(r.ToId);
                return d;
            });
            return model;
        }

        private static RoadModelSource FromDto(ModelDto model)
        {
            var source = new RoadModelSource();
            source.ModelId = Id(model.ModelId, "ModelId");
            source.Label = model.Label;
            source.ValidationProfile = model.ValidationProfile;
            source.LocalizationProfile = model.LocalizationProfile;
            source.DrivabilityProfile = model.DrivabilityProfile;

            source.Sections = Map(model.Sections, delegate(SectionDto d)
            {
                var r = new RoadSection();
                r.Id = Id(d.Id, "RoadSection.Id");
                r.Label = d.Label;
                r.RoadClass = ParseEnum<RoadClass>(d.RoadClass, "RoadSection.RoadClass");
                r.Surface = ParseEnum<RoadSurface>(d.Surface, "RoadSection.Surface");
                r.DefaultSpeedLimitMetersPerSecond = d.DefaultSpeedLimitMetersPerSecond;
                r.DefaultAllowedVehicleClasses = ParseEnum<VehicleClassMask>(d.DefaultAllowedVehicleClasses, "RoadSection.DefaultAllowedVehicleClasses");
                return r;
            });

            source.Corridors = Map(model.Corridors, delegate(CorridorDto d)
            {
                var r = new LaneCorridor();
                r.Id = Id(d.Id, "LaneCorridor.Id");
                r.Label = d.Label;
                r.SectionId = Id(d.SectionId, "LaneCorridor.SectionId");
                r.LateralOrder = d.LateralOrder;
                r.IsCrossSectionDatum = d.IsCrossSectionDatum;
                r.Samples = d.Samples ?? new RoadCurveSample[0];
                r.LengthMeters = d.LengthMeters;
                r.HasSpeedLimitOverride = d.HasSpeedLimitOverride;
                r.SpeedLimitOverrideMetersPerSecond = d.SpeedLimitOverrideMetersPerSecond;
                r.HasSurfaceOverride = d.HasSurfaceOverride;
                r.SurfaceOverride = ParseEnum<RoadSurface>(d.SurfaceOverride, "LaneCorridor.SurfaceOverride");
                r.HasAllowedVehicleClassesOverride = d.HasAllowedVehicleClassesOverride;
                r.AllowedVehicleClassesOverride = ParseEnum<VehicleClassMask>(d.AllowedVehicleClassesOverride, "LaneCorridor.AllowedVehicleClassesOverride");
                return r;
            });

            source.Connections = Map(model.Connections, delegate(ConnectionDto d)
            {
                var r = new LaneConnection();
                r.Id = Id(d.Id, "LaneConnection.Id");
                r.FromCorridorId = Id(d.FromCorridorId, "LaneConnection.FromCorridorId");
                r.ToCorridorId = Id(d.ToCorridorId, "LaneConnection.ToCorridorId");
                r.Kind = ParseEnum<LaneConnectionKind>(d.Kind, "LaneConnection.Kind");
                return r;
            });

            source.Adjacencies = Map(model.Adjacencies, delegate(AdjacencyDto d)
            {
                var r = new LaneAdjacency();
                r.Id = Id(d.Id, "LaneAdjacency.Id");
                r.FromCorridorId = Id(d.FromCorridorId, "LaneAdjacency.FromCorridorId");
                r.ToCorridorId = Id(d.ToCorridorId, "LaneAdjacency.ToCorridorId");
                r.Side = ParseEnum<LaneSide>(d.Side, "LaneAdjacency.Side");
                r.FromStartSMeters = d.FromStartSMeters;
                r.FromEndSMeters = d.FromEndSMeters;
                r.ToStartSMeters = d.ToStartSMeters;
                r.ToEndSMeters = d.ToEndSMeters;
                r.Permission = ParseEnum<LaneChangePermission>(d.Permission, "LaneAdjacency.Permission");
                return r;
            });

            source.Junctions = Map(model.Junctions, delegate(JunctionDto d)
            {
                var r = new Junction();
                r.Id = Id(d.Id, "Junction.Id");
                r.Label = d.Label;
                r.Feature = ParseEnum<JunctionFeature>(d.Feature, "Junction.Feature");
                r.Boundary = d.Boundary;
                return r;
            });

            source.Movements = Map(model.Movements, delegate(MovementDto d)
            {
                var r = new JunctionMovement();
                r.Id = Id(d.Id, "JunctionMovement.Id");
                r.Label = d.Label;
                r.JunctionId = Id(d.JunctionId, "JunctionMovement.JunctionId");
                r.FromCorridorId = Id(d.FromCorridorId, "JunctionMovement.FromCorridorId");
                r.ToCorridorId = Id(d.ToCorridorId, "JunctionMovement.ToCorridorId");
                r.Samples = d.Samples ?? new RoadCurveSample[0];
                r.LengthMeters = d.LengthMeters;
                r.RoutePreferenceWeight = d.RoutePreferenceWeight;
                return r;
            });

            source.Controls = Map(model.Controls, delegate(ControlDto d)
            {
                var r = new JunctionControl();
                r.Id = Id(d.Id, "JunctionControl.Id");
                r.JunctionId = Id(d.JunctionId, "JunctionControl.JunctionId");
                r.Kind = ParseEnum<JunctionControlKind>(d.Kind, "JunctionControl.Kind");
                r.ControlledMovementIds = Ids(d.ControlledMovementIds, "JunctionControl.ControlledMovementIds");
                r.HasStopLine = d.HasStopLine;
                r.StopLine = d.StopLine;
                return r;
            });

            source.ConflictZones = Map(model.ConflictZones, delegate(ZoneDto d)
            {
                var r = new ConflictZone();
                r.Id = Id(d.Id, "ConflictZone.Id");
                r.JunctionId = Id(d.JunctionId, "ConflictZone.JunctionId");
                r.Volume = d.Volume;
                r.MemberMovementIds = Ids(d.MemberMovementIds, "ConflictZone.MemberMovementIds");
                return r;
            });

            source.SignalPlans = Map(model.SignalPlans, delegate(PlanDto d)
            {
                var r = new SignalPlan();
                r.Id = Id(d.Id, "SignalPlan.Id");
                r.JunctionId = Id(d.JunctionId, "SignalPlan.JunctionId");
                r.Groups = Map(d.Groups, delegate(GroupDto g)
                {
                    var group = new SignalGroup();
                    group.GroupId = Id(g.GroupId, "SignalGroup.GroupId");
                    group.MemberMovementIds = Ids(g.MemberMovementIds, "SignalGroup.MemberMovementIds");
                    return group;
                });
                r.Phases = Map(d.Phases, delegate(PhaseDto p)
                {
                    var phase = new SignalPhase();
                    phase.PhaseId = Id(p.PhaseId, "SignalPhase.PhaseId");
                    phase.DurationSeconds = p.DurationSeconds;
                    phase.GroupStates = Map(p.GroupStates, delegate(GroupStateDto s)
                    {
                        var state = new SignalGroupState();
                        state.GroupId = Id(s.GroupId, "SignalGroupState.GroupId");
                        state.State = ParseEnum<SignalState>(s.State, "SignalGroupState.State");
                        return state;
                    });
                    return phase;
                });
                return r;
            });

            source.Portals = Map(model.Portals, delegate(PortalDto d)
            {
                var r = new Portal();
                r.Id = Id(d.Id, "Portal.Id");
                r.Label = d.Label;
                r.CorridorId = Id(d.CorridorId, "Portal.CorridorId");
                r.Role = ParseEnum<PortalRole>(d.Role, "Portal.Role");
                r.SMeters = d.SMeters;
                r.EnvelopeLengthMeters = d.EnvelopeLengthMeters;
                r.EnvelopeHalfWidthMeters = d.EnvelopeHalfWidthMeters;
                return r;
            });

            var manifest = model.Manifest ?? new ManifestDto();
            source.Manifest = new ImportManifest();
            source.Manifest.Entries = Map(manifest.Entries, delegate(EntryDto d)
            {
                var e = new ImportManifestEntry();
                e.RecordId = Id(d.RecordId, "ImportManifestEntry.RecordId");
                e.Kind = ParseEnum<RoadRecordKind>(d.Kind, "ImportManifestEntry.Kind");
                e.ModelId = Id(d.ModelId, "ImportManifestEntry.ModelId");
                e.ImporterSlot = d.ImporterSlot;
                e.SourceKeys = d.SourceKeys ?? new SourceTrace[0];
                return e;
            });
            source.Manifest.TombstonedIds = Ids(manifest.TombstonedIds, "ImportManifest.TombstonedIds");
            source.Manifest.Remaps = Map(manifest.Remaps, delegate(RemapDto d)
            {
                var r = new RoadIdRemap();
                r.FromId = Id(d.FromId, "RoadIdRemap.FromId");
                r.ToId = Id(d.ToId, "RoadIdRemap.ToId");
                return r;
            });
            return source;
        }

        private static TOut[] Map<TIn, TOut>(TIn[] values, Func<TIn, TOut> map)
        {
            var result = new TOut[values == null ? 0 : values.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = map(values[i]);
            }

            return result;
        }

        private static string Hex(RoadId id)
        {
            return id.ToString();
        }

        private static string[] Hexes(RoadId[] ids)
        {
            return Map(ids, Hex);
        }

        private static RoadId Id(string hex, string what)
        {
            RoadId id;
            if (!RoadId.TryParse(hex, out id))
            {
                throw new FormatException("Document de modele : identifiant illisible pour " + what + ".");
            }

            return id;
        }

        private static RoadId[] Ids(string[] hexes, string what)
        {
            return Map(hexes, delegate(string hex) { return Id(hex, what); });
        }

        /// <summary>Enum par nom exact : un nombre, une casse ou une valeur inconnue est un refus.</summary>
        private static T ParseEnum<T>(string name, string what) where T : struct
        {
            T value;
            if (!TryParseDeclaredEnum(name, out value))
            {
                throw new FormatException("Document de modele : valeur inconnue '" + name + "' pour " + what + ".");
            }

            return value;
        }

        /// <summary>
        /// Enum par nom exact et declare : un nombre (meme defini), une casse differente, une valeur
        /// non declaree ou, pour un enum [Flags], un bit hors des drapeaux declares est refuse.
        /// </summary>
        internal static bool TryParseDeclaredEnum<T>(string name, out T value) where T : struct
        {
            if (name == null || !System.Enum.TryParse(name, false, out value) || value.ToString() != name)
            {
                value = default(T);
                return false;
            }

            var type = typeof(T);
            if (!type.IsDefined(typeof(FlagsAttribute), false))
            {
                return System.Enum.IsDefined(type, value);
            }

            ulong declared = 0UL;
            foreach (var flag in System.Enum.GetValues(type))
            {
                declared |= Convert.ToUInt64(flag, CultureInfo.InvariantCulture);
            }

            return (Convert.ToUInt64(value, CultureInfo.InvariantCulture) & ~declared) == 0UL;
        }

        // ================================================================== DTO
        // Les records RoadId ne sont pas serialisables par JsonUtility (champs readonly) et un enum y
        // serait un entier : chaque record passe par un DTO a identifiants hex et enums par nom.

        [Serializable]
        private sealed class DocumentDto
        {
            public int Format;
            public BindingDto Binding;
            public ModelDto Model;
        }

        [Serializable]
        private sealed class BindingDto
        {
            public string ModelVersion;
            public int CompilerSchemaVersion;
            public string SourceHash;
            public string LineageHash;
            public string DecisionsHash;
            public int ImporterVersion;
            public int PipelineVersion;
            public string IntegrityHash;
        }

        [Serializable]
        private sealed class ModelDto
        {
            public string ModelId;
            public string Label;
            public RoadModelValidationProfile ValidationProfile;
            public RoadLocalizationProfile LocalizationProfile;
            public DrivabilityProfile DrivabilityProfile;
            public SectionDto[] Sections;
            public CorridorDto[] Corridors;
            public ConnectionDto[] Connections;
            public AdjacencyDto[] Adjacencies;
            public JunctionDto[] Junctions;
            public MovementDto[] Movements;
            public ControlDto[] Controls;
            public ZoneDto[] ConflictZones;
            public PlanDto[] SignalPlans;
            public PortalDto[] Portals;
            public ManifestDto Manifest;
        }

        [Serializable]
        private sealed class SectionDto
        {
            public string Id;
            public string Label;
            public string RoadClass;
            public string Surface;
            public float DefaultSpeedLimitMetersPerSecond;
            public string DefaultAllowedVehicleClasses;
        }

        [Serializable]
        private sealed class CorridorDto
        {
            public string Id;
            public string Label;
            public string SectionId;
            public int LateralOrder;
            public bool IsCrossSectionDatum;
            public float LengthMeters;
            public bool HasSpeedLimitOverride;
            public float SpeedLimitOverrideMetersPerSecond;
            public bool HasSurfaceOverride;
            public string SurfaceOverride;
            public bool HasAllowedVehicleClassesOverride;
            public string AllowedVehicleClassesOverride;
            public RoadCurveSample[] Samples;
        }

        [Serializable]
        private sealed class ConnectionDto
        {
            public string Id;
            public string FromCorridorId;
            public string ToCorridorId;
            public string Kind;
        }

        [Serializable]
        private sealed class AdjacencyDto
        {
            public string Id;
            public string FromCorridorId;
            public string ToCorridorId;
            public string Side;
            public float FromStartSMeters;
            public float FromEndSMeters;
            public float ToStartSMeters;
            public float ToEndSMeters;
            public string Permission;
        }

        [Serializable]
        private sealed class JunctionDto
        {
            public string Id;
            public string Label;
            public string Feature;
            public RoadBoundsBox Boundary;
        }

        [Serializable]
        private sealed class MovementDto
        {
            public string Id;
            public string Label;
            public string JunctionId;
            public string FromCorridorId;
            public string ToCorridorId;
            public float LengthMeters;
            public float RoutePreferenceWeight;
            public RoadCurveSample[] Samples;
        }

        [Serializable]
        private sealed class ControlDto
        {
            public string Id;
            public string JunctionId;
            public string Kind;
            public string[] ControlledMovementIds;
            public bool HasStopLine;
            public RoadLineSegment StopLine;
        }

        [Serializable]
        private sealed class ZoneDto
        {
            public string Id;
            public string JunctionId;
            public RoadBoundsBox Volume;
            public string[] MemberMovementIds;
        }

        [Serializable]
        private sealed class PlanDto
        {
            public string Id;
            public string JunctionId;
            public GroupDto[] Groups;
            public PhaseDto[] Phases;
        }

        [Serializable]
        private sealed class GroupDto
        {
            public string GroupId;
            public string[] MemberMovementIds;
        }

        [Serializable]
        private sealed class PhaseDto
        {
            public string PhaseId;
            public float DurationSeconds;
            public GroupStateDto[] GroupStates;
        }

        [Serializable]
        private sealed class GroupStateDto
        {
            public string GroupId;
            public string State;
        }

        [Serializable]
        private sealed class PortalDto
        {
            public string Id;
            public string Label;
            public string CorridorId;
            public string Role;
            public float SMeters;
            public float EnvelopeLengthMeters;
            public float EnvelopeHalfWidthMeters;
        }

        [Serializable]
        private sealed class ManifestDto
        {
            public EntryDto[] Entries;
            public string[] TombstonedIds;
            public RemapDto[] Remaps;
        }

        [Serializable]
        private sealed class EntryDto
        {
            public string RecordId;
            public string Kind;
            public string ModelId;
            public string ImporterSlot;
            public SourceTrace[] SourceKeys;
        }

        [Serializable]
        private sealed class RemapDto
        {
            public string FromId;
            public string ToId;
        }
    }
}
