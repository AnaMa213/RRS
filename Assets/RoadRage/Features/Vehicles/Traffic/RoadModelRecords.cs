using System;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic
{
    // =====================================================================================
    // Story 5.25 -- Road World Model : enregistrements authorables.
    //
    // Un seul fichier pour que la matrice de propriete d'AD-43 se relise d'un bloc. Regle
    // structurante, valable pour chacun des types ci-dessous : les cles etrangeres portees par
    // l'enfant sont la SEULE verite parent/enfant persistee. Aucune collection reciproque n'est
    // authorable ; toute collection inverse et tout index sont des sorties de
    // <see cref="RoadModelCompiler"/> lues sur <see cref="CompiledRoadModel"/>.
    //
    // Exceptions volontaires a cette regle, parce que l'appartenance y est la propriete meme du
    // record et non un inverse : JunctionControl possede les mouvements qu'il controle (AD-46,
    // liaison de controle unique et faisant autorite), ConflictZone possede ses membres, et un
    // groupe de SignalPlan possede son appartenance signal. Un mouvement ne stocke jamais en
    // retour son controle, sa zone de conflit ou son groupe.
    //
    // C# 9.0 : ni `record` ni `init`, non supportes par Unity 6. Les enregistrements sont des
    // `[Serializable] struct` a champs publics, lisibles par la serialisation Unity quand la
    // Story 5.27 introduira un conteneur persiste.
    // =====================================================================================

    /// <summary>
    /// Identifiant opaque 128 bits d'un enregistrement du Road World Model (AD-44). Genere une
    /// fois, independant du nom, de l'ordre de hierarchie, de l'index de tableau, du transform et
    /// du <c>NetworkObjectId</c>. Suit le gabarit de <c>RoadRage.Shared.Definitions.DefinitionId</c>
    /// (readonly struct, egalite de valeur, operateurs) avec une valeur hexadecimale minuscule de
    /// 32 caracteres au lieu d'un slug.
    /// </summary>
    /// <remarks>
    /// La 5.27 persistera ces identifiants via <see cref="ToString"/> / <see cref="TryParse"/> :
    /// la serialisation Unity ne serialise pas les champs <c>readonly</c>, donc un conteneur
    /// persiste stockera la forme hexadecimale, pas la structure telle quelle.
    /// </remarks>
    [Serializable]
    public readonly struct RoadId : IEquatable<RoadId>, IComparable<RoadId>
    {
        /// <summary>Identifiant vide : jamais valide sur un enregistrement vivant.</summary>
        public static readonly RoadId None = default(RoadId);

        private readonly ulong _high;
        private readonly ulong _low;

        public RoadId(ulong high, ulong low)
        {
            _high = high;
            _low = low;
        }

        public ulong High
        {
            get { return _high; }
        }

        public ulong Low
        {
            get { return _low; }
        }

        public bool IsEmpty
        {
            get { return _high == 0UL && _low == 0UL; }
        }

        /// <summary>Genere un identifiant opaque. Seule source legitime de nouvelles identites.</summary>
        public static RoadId New()
        {
            byte[] bytes = Guid.NewGuid().ToByteArray();
            return new RoadId(BitConverter.ToUInt64(bytes, 0), BitConverter.ToUInt64(bytes, 8));
        }

        public static bool TryParse(string hex, out RoadId id)
        {
            id = None;
            if (hex == null || hex.Length != 32)
            {
                return false;
            }

            ulong high;
            ulong low;
            if (!TryParseSegment(hex, 0, out high) || !TryParseSegment(hex, 16, out low))
            {
                return false;
            }

            id = new RoadId(high, low);
            return true;
        }

        public static RoadId Parse(string hex)
        {
            RoadId id;
            if (!TryParse(hex, out id))
            {
                throw new FormatException("RoadId attend 32 caracteres hexadecimaux minuscules, recu : " + (hex ?? "<null>"));
            }

            return id;
        }

        private static bool TryParseSegment(string hex, int offset, out ulong value)
        {
            value = 0UL;
            for (int i = 0; i < 16; i++)
            {
                char c = hex[offset + i];
                int digit;
                if (c >= '0' && c <= '9')
                {
                    digit = c - '0';
                }
                else if (c >= 'a' && c <= 'f')
                {
                    digit = 10 + (c - 'a');
                }
                else
                {
                    return false;
                }

                value = (value << 4) | (uint)digit;
            }

            return true;
        }

        public bool Equals(RoadId other)
        {
            return _high == other._high && _low == other._low;
        }

        public override bool Equals(object obj)
        {
            return obj is RoadId other && Equals(other);
        }

        public override int GetHashCode()
        {
            ulong mixed = _high ^ _low;
            return (int)(mixed ^ (mixed >> 32));
        }

        /// <summary>
        /// Ordre total deterministe sur l'identite opaque. Identique a l'ordre ordinal de la forme
        /// hexadecimale : c'est l'ordre de tri canonique de <see cref="RoadModelCanonicalWriter"/>.
        /// </summary>
        public int CompareTo(RoadId other)
        {
            if (_high != other._high)
            {
                return _high < other._high ? -1 : 1;
            }

            if (_low != other._low)
            {
                return _low < other._low ? -1 : 1;
            }

            return 0;
        }

        public override string ToString()
        {
            return _high.ToString("x16", System.Globalization.CultureInfo.InvariantCulture)
                + _low.ToString("x16", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static bool operator ==(RoadId left, RoadId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RoadId left, RoadId right)
        {
            return !left.Equals(right);
        }
    }

    // ------------------------------------------------------------------ enumerations

    /// <summary>Genre semantique d'un enregistrement, porte par la lignee d'import.</summary>
    public enum RoadRecordKind
    {
        Unknown = 0,
        Section = 1,
        Corridor = 2,
        Connection = 3,
        Adjacency = 4,
        Junction = 5,
        Movement = 6,
        Control = 7,
        ConflictZone = 8,
        SignalPlan = 9,
        Portal = 10
    }

    public enum RoadClass
    {
        Unspecified = 0,
        Local = 1,
        Arterial = 2,
        Highway = 3,
        Service = 4
    }

    public enum RoadSurface
    {
        Asphalt = 0,
        Concrete = 1,
        Gravel = 2,
        Dirt = 3
    }

    [Flags]
    public enum VehicleClassMask
    {
        None = 0,
        Car = 1,
        Truck = 2,
        Bus = 4,
        Emergency = 8,
        All = Car | Truck | Bus | Emergency
    }

    public enum LaneConnectionKind
    {
        Continuation = 0,
        Merge = 1,
        Split = 2
    }

    public enum LaneSide
    {
        Left = 0,
        Right = 1
    }

    public enum LaneChangePermission
    {
        Forbidden = 0,
        Allowed = 1
    }

    /// <summary>
    /// Classification authoree d'un carrefour, utilisee par les regles et le debug. Jamais inferee
    /// d'un cycle de graphe ni d'un nombre de noeuds, et ne surclasse jamais un controle authore.
    /// </summary>
    public enum JunctionFeature
    {
        Unspecified = 0,
        Crossroads = 1,
        TJunction = 2,
        Roundabout = 3,
        Other = 4
    }

    /// <summary>
    /// Genre declare par la liaison de controle unique d'un mouvement (AD-46).
    /// <see cref="Uncontrolled"/> est un choix explicite, jamais un repli.
    /// </summary>
    public enum JunctionControlKind
    {
        Uncontrolled = 0,
        Priority = 1,
        Yield = 2,
        Stop = 3,
        Signalized = 4
    }

    public enum SignalState
    {
        Red = 0,
        Yellow = 1,
        Green = 2
    }

    public enum PortalRole
    {
        Entry = 0,
        Exit = 1
    }

    // ------------------------------------------------------------------ geometrie authoree

    /// <summary>
    /// Un echantillon de la charge geometrique. Aucune mathematique de courbe ici : echantillonnage,
    /// projection et bornes vivent dans <see cref="RoadCurve"/>, la production depuis une polyligne
    /// authoree dans <see cref="RoadCurveBuilder"/> (Story 5.26).
    /// </summary>
    [Serializable]
    public struct RoadCurveSample
    {
        /// <summary>Abscisse curviligne en metres depuis le depart dirige.</summary>
        public float SMeters;

        public Vector3 Position;

        /// <summary>Tangente unitaire (avant).</summary>
        public Vector3 Tangent;

        /// <summary>Normale route unitaire (haut route), pas le haut monde.</summary>
        public Vector3 Up;

        /// <summary>Courbure signee en 1/m, positive vers la droite de la route.</summary>
        public float CurvaturePerMeter;

        /// <summary>Demi-largeur utile a gauche, en metres.</summary>
        public float HalfWidthLeftMeters;

        /// <summary>Demi-largeur utile a droite, en metres.</summary>
        public float HalfWidthRightMeters;
    }

    /// <summary>Boite 3D axee monde : frontiere de carrefour ou volume de conflit revu.</summary>
    [Serializable]
    public struct RoadBoundsBox
    {
        public Vector3 Center;

        /// <summary>Demi-dimensions, chacune strictement positive.</summary>
        public Vector3 Extents;
    }

    /// <summary>Segment de ligne d'arret ou de cession, possede par le seul <see cref="JunctionControl"/>.</summary>
    [Serializable]
    public struct RoadLineSegment
    {
        public Vector3 Start;
        public Vector3 End;
    }

    // ------------------------------------------------------------------ les onze enregistrements

    /// <summary>
    /// Regroupement route et valeurs par defaut. Ne possede ni geometrie, ni connectivite, ni
    /// permission de carrefour, et ne porte aucune liste de corridors : le corridor porte sa cle
    /// etrangere de section, et le compilateur en derive la liste inverse.
    /// </summary>
    [Serializable]
    public struct RoadSection
    {
        public RoadId Id;

        /// <summary>Diagnostic seul. Hors charge canonique : le changer ne change pas la version.</summary>
        public string Label;

        public RoadClass RoadClass;
        public RoadSurface Surface;
        public float DefaultSpeedLimitMetersPerSecond;
        public VehicleClassMask DefaultAllowedVehicleClasses;
    }

    /// <summary>
    /// Une courbe de reference 3D dirigee, son domaine curviligne, ses profils de largeur et ses
    /// surcharges locales. Ne possede ni successeurs, ni permissions laterales, ni occupation.
    /// </summary>
    [Serializable]
    public struct LaneCorridor
    {
        public RoadId Id;

        /// <summary>Diagnostic seul.</summary>
        public string Label;

        /// <summary>Cle etrangere de section : seule verite parent/enfant persistee.</summary>
        public RoadId SectionId;

        /// <summary>
        /// Position laterale authoree du corridor dans la coupe transversale de sa section (AD-48).
        /// Unique et contigue depuis 0 par section. Fait de repere <b>section</b>, jamais conducteur :
        /// pour un corridor de sens oppose au datum, l'ordre croissant va vers sa propre gauche, donc
        /// un consommateur relatif au conducteur passe par <see cref="LaneAdjacency"/>, jamais par une
        /// comparaison directe d'ordre entre sens opposes.
        /// </summary>
        public int LateralOrder;

        /// <summary>
        /// Vrai pour l'unique corridor datum de la section (AD-48). Le datum fournit le repere : l'ordre
        /// croissant va vers son road-right au sens d'AD-45, evalue localement a chaque abscisse. La
        /// coherence geometrique de ce repere appartient a la Story 5.26, pas a ce record.
        /// </summary>
        public bool IsCrossSectionDatum;

        /// <summary>Echantillons ordonnes a s strictement croissant, au moins deux.</summary>
        public RoadCurveSample[] Samples;

        public float LengthMeters;

        public bool HasSpeedLimitOverride;
        public float SpeedLimitOverrideMetersPerSecond;

        public bool HasSurfaceOverride;
        public RoadSurface SurfaceOverride;

        public bool HasAllowedVehicleClassesOverride;
        public VehicleClassMask AllowedVehicleClassesOverride;
    }

    /// <summary>
    /// Continuation, fusion ou separation longitudinale entre extremites de corridors, hors
    /// semantique de carrefour. Une traversee de carrefour est toujours un
    /// <see cref="JunctionMovement"/>, jamais une <see cref="LaneConnection"/>.
    /// </summary>
    [Serializable]
    public struct LaneConnection
    {
        public RoadId Id;
        public RoadId FromCorridorId;
        public RoadId ToCorridorId;
        public LaneConnectionKind Kind;
    }

    /// <summary>
    /// Relation laterale entre deux corridors de meme sens, limitee aux intervalles curvilignes qui
    /// se recouvrent, avec la legalite de changement de file.
    /// </summary>
    [Serializable]
    public struct LaneAdjacency
    {
        public RoadId Id;
        public RoadId FromCorridorId;
        public RoadId ToCorridorId;

        /// <summary>Cote du corridor cible vu depuis le corridor source.</summary>
        public LaneSide Side;

        public float FromStartSMeters;
        public float FromEndSMeters;
        public float ToStartSMeters;
        public float ToEndSMeters;

        public LaneChangePermission Permission;
    }

    /// <summary>
    /// Identite, frontiere et classification d'un carrefour. Ne porte aucune liste d'enfants
    /// faisant autorite : mouvements, controles, conflits et plans portent leur cle de carrefour.
    /// </summary>
    [Serializable]
    public struct Junction
    {
        public RoadId Id;

        /// <summary>Diagnostic seul.</summary>
        public string Label;

        public JunctionFeature Feature;
        public RoadBoundsBox Boundary;
    }

    /// <summary>
    /// Traversee legale d'un corridor d'approche vers un corridor de depart, avec sa propre courbe
    /// dirigee et sa preference de route. Ne porte ni ligne de controle, ni liste de conflits, ni
    /// groupe de signal, ni permission courante.
    /// </summary>
    [Serializable]
    public struct JunctionMovement
    {
        public RoadId Id;

        /// <summary>Diagnostic seul.</summary>
        public string Label;

        /// <summary>Unique cle de carrefour parent persistee.</summary>
        public RoadId JunctionId;

        public RoadId FromCorridorId;
        public RoadId ToCorridorId;

        /// <summary>Enveloppe propre au mouvement, authoree avec lui, jamais heritee en silence.</summary>
        public RoadCurveSample[] Samples;

        public float LengthMeters;

        /// <summary>Poids de preference de route, positif ou nul.</summary>
        public float RoutePreferenceWeight;
    }

    /// <summary>
    /// Liaison de controle unique et faisant autorite (AD-46) : elle declare elle-meme son genre et
    /// possede les mouvements qu'elle lie. Un mouvement n'est ni doublement couvert ni laisse sans
    /// couverture. Elle possede seule la geometrie optionnelle de ligne d'arret ou de cession.
    /// </summary>
    [Serializable]
    public struct JunctionControl
    {
        public RoadId Id;
        public RoadId JunctionId;

        public JunctionControlKind Kind;

        /// <summary>Appartenance possedee, pas une collection inverse.</summary>
        public RoadId[] ControlledMovementIds;

        public bool HasStopLine;
        public RoadLineSegment StopLine;
    }

    /// <summary>
    /// Volume de conflit 3D revu et les mouvements qui s'y opposent. Seule proprietaire de
    /// l'appartenance aux conflits : un mouvement ne porte jamais de liste reciproque.
    /// </summary>
    [Serializable]
    public struct ConflictZone
    {
        public RoadId Id;
        public RoadId JunctionId;
        public RoadBoundsBox Volume;

        /// <summary>Au moins deux mouvements du meme carrefour.</summary>
        public RoadId[] MemberMovementIds;
    }

    /// <summary>
    /// Groupe de signal : seul proprietaire de l'appartenance signal de ses mouvements.
    /// </summary>
    [Serializable]
    public struct SignalGroup
    {
        public RoadId GroupId;
        public RoadId[] MemberMovementIds;
    }

    /// <summary>Etat autorise d'un groupe pendant une phase.</summary>
    [Serializable]
    public struct SignalGroupState
    {
        public RoadId GroupId;
        public SignalState State;
    }

    /// <summary>Phase ordonnee : l'ordre des phases est semantique et n'est jamais retrie.</summary>
    [Serializable]
    public struct SignalPhase
    {
        public RoadId PhaseId;
        public float DurationSeconds;
        public SignalGroupState[] GroupStates;
    }

    /// <summary>
    /// Groupes, phases ordonnees et minutage d'un carrefour signalise. Ne porte aucune horloge de
    /// phase courante : cet etat vit dans le runtime hote.
    /// </summary>
    [Serializable]
    public struct SignalPlan
    {
        public RoadId Id;
        public RoadId JunctionId;
        public SignalGroup[] Groups;
        public SignalPhase[] Phases;
    }

    /// <summary>
    /// Source ou puits sur un corridor : role, placement curviligne et enveloppe d'insertion ou de
    /// retrait. Ne possede ni demande de population ni agents apparus.
    /// </summary>
    [Serializable]
    public struct Portal
    {
        public RoadId Id;

        /// <summary>Diagnostic seul.</summary>
        public string Label;

        public RoadId CorridorId;
        public PortalRole Role;
        public float SMeters;
        public float EnvelopeLengthMeters;
        public float EnvelopeHalfWidthMeters;
    }

    // ------------------------------------------------------------------ lignee d'import

    /// <summary>
    /// Cle de source typee. Position de tableau, nom d'affichage et transform ne sont jamais des
    /// cles. Diagnostic de lignee : hors charge canonique.
    /// </summary>
    [Serializable]
    public struct SourceTrace
    {
        public string SourceAssetGuid;
        public long SourceObjectFileId;
        public string RelationKey;
    }

    /// <summary>Une entree de lignee par enregistrement semantique.</summary>
    [Serializable]
    public struct ImportManifestEntry
    {
        public RoadId RecordId;
        public RoadRecordKind Kind;

        /// <summary>
        /// Modele possedant l'enregistrement. Une entree qui nomme un autre modele est une
        /// reference inter-version : echec dur.
        /// </summary>
        public RoadId ModelId;

        /// <summary>Slot d'importeur persistant, unique dans le manifeste.</summary>
        public string ImporterSlot;

        /// <summary>Lignee many-to-many : plusieurs cles peuvent tracer un meme enregistrement.</summary>
        public SourceTrace[] SourceKeys;
    }

    /// <summary>Redirection d'un identifiant retire vers un identifiant vivant.</summary>
    [Serializable]
    public struct RoadIdRemap
    {
        public RoadId FromId;
        public RoadId ToId;
    }

    /// <summary>
    /// Lignee fournie AVEC la source. La 5.25 ne cree ni ne persiste aucun historique de migration :
    /// elle valide seulement ce que le <see cref="RoadModelSource"/> contient (Story 5.27).
    /// </summary>
    [Serializable]
    public struct ImportManifest
    {
        public ImportManifestEntry[] Entries;

        /// <summary>Identifiants supprimes, jamais recycles par un enregistrement vivant.</summary>
        public RoadId[] TombstonedIds;

        public RoadIdRemap[] Remaps;
    }

    // ------------------------------------------------------------------ profil et conteneur

    /// <summary>
    /// Profil de validation versionne : parametres STATIQUES de la validation du modele, geometrie
    /// comprise (Story 5.26). Il participe a la charge canonique, donc le changer change la version
    /// du modele. Admettre une classe de vehicule plus grande impose une recompilation. Les
    /// parametres de requete de localisation vivent a part, dans <see cref="RoadLocalizationProfile"/>.
    /// </summary>
    [Serializable]
    public struct RoadModelValidationProfile
    {
        /// <summary>Demi-largeur maximale de gabarit supportee, en metres.</summary>
        public float MaxVehicleHalfWidthMeters;

        /// <summary>Longueur maximale de gabarit supportee, en metres.</summary>
        public float MaxVehicleLengthMeters;

        /// <summary>Marge de degagement lateral exigee de chaque cote, en metres.</summary>
        public float LateralClearanceMarginMeters;

        /// <summary>
        /// Ecart de position maximal a une couture (connexion ou extremite de mouvement), et ecart
        /// maximal de demi-largeur a une couture de mouvement, en metres. Contrat : 0,05 m au plus.
        /// </summary>
        public float SeamGapToleranceMeters;

        /// <summary>Ecart de tangente maximal a une couture, en degres. Contrat : 5 degres au plus.</summary>
        public float SeamTangentToleranceDegrees;

        /// <summary>
        /// Ecart maximal entre abscisse et longueur mesuree (depart a 0, longueur declaree contre
        /// derniere abscisse, pas d'abscisse contre corde), en metres.
        /// </summary>
        public float LengthToleranceMeters;

        /// <summary>Recouvrement maximal tolere entre enveloppes transversales voisines (AD-48), en metres.</summary>
        public float EnvelopeOverlapToleranceMeters;

        /// <summary>
        /// Ecart angulaire maximal au-dela duquel un corridor n'est ni parallele ni antiparallele au
        /// datum de sa section (AD-48), en degres. AD-48 ne fixe pas de nombre : le seuil est une
        /// valeur de validation versionnee, dans ]0, 90[ (90 degres accepterait une perpendiculaire).
        /// </summary>
        public float GroundingMaxOffAxisDegrees;
    }

    /// <summary>
    /// Parametres de REQUETE de la localisation (Story 5.26). Distincts du profil de validation
    /// parce qu'ils ne decident pas de la validite du modele, mais ils sont dans la charge
    /// canonique : deux modeles qui localisent differemment n'ont pas la meme version.
    /// </summary>
    [Serializable]
    public struct RoadLocalizationProfile
    {
        /// <summary>Bande de score sous laquelle deux candidats du meme rang sont ambigus, en metres.</summary>
        public float ScoreBandMeters;

        /// <summary>
        /// Hysteresis, en metres : depassement d'enveloppe tolere pour l'element precedent et bonus
        /// de score accorde a l'element precedent (moitie pour un voisin explicite ou un element de route).
        /// </summary>
        public float HysteresisMeters;

        /// <summary>Distance maximale entre le point de reference et l'enveloppe acceptee, en metres.</summary>
        public float AcceptanceDistanceMeters;

        /// <summary>Seuil d'erreur de cap absolue au-dela duquel la pose est a contresens, en degres.</summary>
        public float WrongWayHeadingDegrees;
    }

    /// <summary>
    /// Conteneur source complet passe a <see cref="RoadModelCompiler.Compile"/>. Le compilateur ne
    /// connait rien d'autre : il ne se souvient d'aucun import precedent et ne persiste rien.
    /// </summary>
    [Serializable]
    public sealed class RoadModelSource
    {
        public RoadId ModelId;

        /// <summary>Diagnostic seul.</summary>
        public string Label;

        public RoadModelValidationProfile ValidationProfile;

        public RoadLocalizationProfile LocalizationProfile;

        public RoadSection[] Sections;
        public LaneCorridor[] Corridors;
        public LaneConnection[] Connections;
        public LaneAdjacency[] Adjacencies;
        public Junction[] Junctions;
        public JunctionMovement[] Movements;
        public JunctionControl[] Controls;
        public ConflictZone[] ConflictZones;
        public SignalPlan[] SignalPlans;
        public Portal[] Portals;
        public ImportManifest Manifest;
    }
}
