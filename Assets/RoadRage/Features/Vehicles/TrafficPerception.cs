using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Paliers de degagement, essayes dans cet ordre par le conducteur. L'ordre de l'ECHELLE est celui
    /// des paliers dans <c>TickLocalTraffic</c>, pas celui des valeurs de l'enum : les valeurs sont
    /// figees pour ne pas renumeroter celles deja livrees par la Story 5.17.
    /// </summary>
    public enum TrafficUnblockingAction
    {
        FollowRoute = 0,
        Replan = 1,
        Reverse = 2,
        RoadDetour = 3,
        SidewalkDetour = 4,

        /// <summary>
        /// Story 5.18 : palier de TETE de l'echelle, le moins couteux et le plus lisible. Il ne fait
        /// qu'assouplir une regle que le vehicule respectait -- entrer dans l'intersection normalement
        /// tenue libre -- la ou <see cref="Reverse"/>, <see cref="RoadDetour"/> et
        /// <see cref="SidewalkDetour"/> engagent une manoeuvre. Aucun palier de retrait, de
        /// teleportation ou de despawn n'existe dans cette echelle.
        /// </summary>
        IntersectionBreach = 5,

        /// <summary>
        /// Story 5.19 : remise dans le SENS DE CIRCULATION. Aucun autre palier ne reoriente le
        /// vehicule -- un contournement se construit sur son cap courant, et le recul suit ce meme
        /// cap a l'envers. Un vehicule a contresens rejouait donc indefiniment des manoeuvres qui
        /// preservaient toutes l'erreur qu'il fallait corriger (mesure soak : 53,1 s a contresens,
        /// 68,8 s immobile, palier 4 atteint).
        /// </summary>
        Reorient = 6
    }

    /// <summary>
    /// Story 5.18 (correctif du retour terrain) : POURQUOI le conducteur roule ou s'arrete. Le retour
    /// terrain le demandait explicitement -- "IA : Calm" ne distingue pas une file d'un refus de
    /// priorite ni d'un arret sur rien, donc une validation manuelle ne pouvait pas trancher.
    ///
    /// C'est un motif de DECISION, pas un etat de machine : il se relit a chaque pas et ne pilote
    /// rien. L'echelle de deblocage reste <see cref="TrafficUnblockingAction"/>, qui est orthogonale.
    /// </summary>
    public enum TrafficDecisionReason
    {
        /// <summary>Route libre : aucune raison de ne pas continuer.</summary>
        Cruise = 0,

        /// <summary>Un leader occupe NOTRE couloir devant nous : l'IDM adapte la vitesse.</summary>
        FollowingSameLane = 1,

        /// <summary>Nos deux trajectoires se disputent un point commun et nous sommes le cedant.</summary>
        YieldTrajectory = 2,

        /// <summary>La trajectoire d'un joueur (ou d'un pieton) coupe reellement la notre.</summary>
        PlayerCollisionRisk = 3,

        /// <summary>Un obstacle immobile occupe physiquement notre voie.</summary>
        StaticObstacle = 4,

        /// <summary>
        /// File LEGITIME : le leader est lui-meme en attente reguliere (feu, priorite, file). Ce
        /// n'est PAS un blocage, et l'echelle de deblocage ne doit donc pas s'y declencher -- sans
        /// cette distinction, toute queue finissait en manoeuvre d'evitement au bout du delai de
        /// klaxon, puis recommencait indefiniment.
        /// </summary>
        TrafficQueue = 11,

        /// <summary>
        /// Voie de sortie de jonction REELLEMENT occupee : entrer bloquerait l'intersection. Distinct
        /// de <see cref="JunctionYield"/>, qui est un refus de priorite.
        /// </summary>
        ExitSaturated = 12,

        /// <summary>
        /// Joueur IMMEDIATEMENT en travers de la voie, a portee de contact. Seul cas ou le joueur
        /// entre dans la decision : ni la proximite, ni l'approche, ni une prediction lointaine ne
        /// suffisent. La reponse attendue est un evitement, pas un arret.
        /// </summary>
        PlayerImmediateConflict = 13,

        /// <summary>Tampon de perception sature : l'environnement est INCONNU, donc jamais lu comme libre.</summary>
        PerceptionSaturated = 5,

        /// <summary>Arbitrage d'approche de jonction authoree : l'autre approche passe d'abord.</summary>
        JunctionYield = 6,

        /// <summary>Palier de deblocage d'intersection franchi (Story 5.18).</summary>
        JunctionBreach = 7,

        /// <summary>Manoeuvre de degagement en cours (echelle de la Story 5.17).</summary>
        UnblockingManeuver = 8,

        /// <summary>Cession de priorite relachee en dernier recours : le prioritaire n'avance pas.</summary>
        DeadlockRecovery = 9,

        /// <summary>
        /// Face-a-face DANS notre voie. Seul motif qui ne s'arbitre jamais : autoriser l'un des deux
        /// a "passer en premier" reviendrait a autoriser la collision, donc les deux s'arretent.
        /// </summary>
        EmergencyBrake = 10
    }

    /// <summary>Vue locale cadencee, reutilisable pour les decisions d'intersection.</summary>
    public readonly struct TrafficPerceptionCandidate
    {
        public TrafficPerceptionCandidate(float distance, float angleDegrees, float speed,
            bool isRoadUser, bool isPedestrian, bool isObstacle, bool isBehind,
            float lateralOffset = 0f, Vector3 position = default, Vector3 velocity = default,
            Vector3 extents = default, float halfWidth = 0f, Quaternion rotation = default,
            bool isPlayer = false, bool isCollisionThreat = true, bool isOnOwnPath = true,
            float conflictArrival = float.PositiveInfinity, float otherConflictArrival = float.PositiveInfinity)
        {
            Distance = distance; AngleDegrees = angleDegrees; Speed = speed; IsRoadUser = isRoadUser;
            IsPedestrian = isPedestrian; IsObstacle = isObstacle; IsBehind = isBehind;
            LateralOffset = lateralOffset; Position = position; Velocity = velocity;
            Extents = extents; HalfWidth = halfWidth;
            Rotation = rotation == default ? Quaternion.identity : rotation;
            IsPlayer = isPlayer;
            IsCollisionThreat = isCollisionThreat;
            IsOnOwnPath = isOnOwnPath;
            ConflictArrival = conflictArrival;
            OtherConflictArrival = otherConflictArrival;
        }
        public float Distance { get; }
        public float AngleDegrees { get; }
        public float Speed { get; }
        public bool IsRoadUser { get; }
        public bool IsPedestrian { get; }
        public bool IsObstacle { get; }
        public bool IsBehind { get; }
        public float LateralOffset { get; }
        public Vector3 Position { get; }
        public Vector3 Velocity { get; }
        public Vector3 Extents { get; }
        public float HalfWidth { get; }
        /// <summary>Orientation des demi-dimensions Extents, qui ne sont pas une AABB monde.</summary>
        public Quaternion Rotation { get; }
        /// <summary>Un joueur proche ne devient un leader que si sa trajectoire coupe reellement la notre.</summary>
        public bool IsPlayer { get; }

        /// <summary>
        /// Story 5.18 (correctif) : vrai quand les OCCUPATIONS futures se recouvrent reellement, pas
        /// quand l'acteur est seulement proche ou de face. <see cref="Distance"/> et
        /// <see cref="LateralOffset"/> repondent a "ou est-il par rapport a ma VOIE", ce drapeau
        /// repond a "nos trajectoires se disputent-elles le meme espace au meme moment".
        /// </summary>
        public bool IsCollisionThreat { get; }

        /// <summary>
        /// Vrai quand l'acteur se projette DANS notre couloir de voie. C'est ce qui distingue un
        /// leader a suivre (meme couloir) d'un croisement a arbitrer (couloirs distincts) : sans lui,
        /// un vehicule de la voie opposee et un vehicule devant nous rendaient la meme decision.
        /// </summary>
        public bool IsOnOwnPath { get; }

        /// <summary>Delai (s) avant que NOUS atteignions le point de conflit. Infini sans conflit.</summary>
        public float ConflictArrival { get; }

        /// <summary>Delai (s) avant que l'AUTRE atteigne le meme point. Infini sans conflit.</summary>
        public float OtherConflictArrival { get; }
    }

    /// <summary>
    /// Story 5.18 (correctif) : resultat de la comparaison de deux trajectoires PREVUES. Il ne dit pas
    /// "quelque chose est proche" mais "nos deux emprises se disputent le meme point, et voici quand".
    /// </summary>
    public readonly struct PathConflict
    {
        public static readonly PathConflict None = new PathConflict(false, float.PositiveInfinity,
            Vector3.zero, float.PositiveInfinity, float.PositiveInfinity);

        public PathConflict(bool exists, float time, Vector3 point, float ownArrival, float otherArrival)
        {
            Exists = exists; Time = time; Point = point; OwnArrival = ownArrival; OtherArrival = otherArrival;
        }

        /// <summary>Les deux occupations se recouvrent dans l'horizon considere.</summary>
        public bool Exists { get; }

        /// <summary>Debut du recouvrement (s), donc le delai avant lequel une decision doit exister.</summary>
        public float Time { get; }

        /// <summary>Point de conflit en coordonnees monde : la ou les deux couloirs se rencontrent.</summary>
        public Vector3 Point { get; }

        /// <summary>Delai avant que NOUS atteignions <see cref="Point"/>, en suivant notre trajectoire.</summary>
        public float OwnArrival { get; }

        /// <summary>Delai avant que l'AUTRE l'atteigne, en suivant la sienne.</summary>
        public float OtherArrival { get; }
    }

    public static class TrafficPerception
    {
        public static bool IsInArc(float distance, float angleDegrees, float radius, float arcDegrees)
        {
            return float.IsFinite(distance) && distance >= 0f && distance <= radius
                && float.IsFinite(angleDegrees) && Mathf.Abs(angleDegrees) <= arcDegrees * 0.5f;
        }

        /// <summary>
        /// Leader a suivre : l'acteur le plus proche DEVANT NOUS DANS NOTRE COULOIR DE VOIE.
        ///
        /// Story 5.18 (correctif du retour terrain) : la selection ne demande plus "est-il dans mon
        /// cone avant", question qui confondait la voie opposee, un croisement et une file. Elle
        /// demande "se projette-t-il dans mon couloir" -- <see cref="TrafficPerceptionCandidate.IsOnOwnPath"/>,
        /// calcule contre la POLYLIGNE de voie reellement suivie et non contre la tangente du nez. Un
        /// vehicule de la voie opposee n'est donc jamais un leader, quel que soit son cap ; un
        /// vehicule ARRETE dans notre voie en reste un, joueur compris -- ce que la variante
        /// precedente, qui ecartait tout joueur sans collision predite, laissait traverser.
        ///
        /// Le rayon reste un filtre de PORTEE de perception, jamais un critere de danger.
        /// </summary>
        public static bool TrySelectLeader(IReadOnlyList<TrafficPerceptionCandidate> candidates, int count,
            float radius, float arcDegrees, out float gap, out float speed, float ownHalfWidth = float.PositiveInfinity)
        {
            return TrySelectLeader(candidates, count, radius, arcDegrees, out gap, out speed, out _, ownHalfWidth);
        }

        /// <summary>Meme selection, en rendant l'INDEX retenu : le diagnostic doit pouvoir nommer le leader.</summary>
        public static bool TrySelectLeader(IReadOnlyList<TrafficPerceptionCandidate> candidates, int count,
            float radius, float arcDegrees, out float gap, out float speed, out int index,
            float ownHalfWidth = float.PositiveInfinity)
        {
            gap = DriverModel.NoLeaderGap; speed = 0f; index = -1;
            for (var i = 0; candidates != null && i < count && i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if ((!candidate.IsObstacle && !candidate.IsRoadUser && !candidate.IsPedestrian)
                    || candidate.IsBehind || !candidate.IsOnOwnPath
                    || !IsInArc(candidate.Distance, candidate.AngleDegrees, radius, arcDegrees)
                    || candidate.Distance >= gap) continue;
                index = i; gap = candidate.Distance; speed = candidate.Speed;
            }
            return index >= 0;
        }

        /// <summary>
        /// Distance PLANAIRE d'un point a une boite orientee : distance au point le plus proche de la
        /// boite, et non a son centre.
        ///
        /// La difference n'est pas cosmetique, c'est la cause d'un faux positif entier. Mesurer
        /// "centre de l'autre, moins son extension projetee sur ma normale" donne, pour un vehicule
        /// PERPENDICULAIRE, sa demi-LONGUEUR (2,22 m sur le greybox) au lieu de sa demi-largeur
        /// (1,03 m) : le couloir de voie gonflait a 3,55 m et avalait la branche transversale d'un
        /// carrefour, distante de 2,83 m. Un vehicule arrete sur une AUTRE branche devenait alors le
        /// "leader" de celui-ci.
        ///
        /// ponytail: la composante verticale n'est pas clampee -- les emprises du projet sont droites
        /// (vehicules, murs, bordures). Une boite fortement inclinee demanderait un clamp 3D complet.
        /// </summary>
        public static float PlanarDistanceToBox(Vector3 point, Vector3 boxCenter, Vector3 boxExtents,
            Quaternion boxRotation)
        {
            if (!ValidRotation(boxRotation)) boxRotation = Quaternion.identity;
            var local = Quaternion.Inverse(boxRotation) * (point - boxCenter);
            var clamped = new Vector3(
                Mathf.Clamp(local.x, -Mathf.Abs(boxExtents.x), Mathf.Abs(boxExtents.x)),
                local.y,
                Mathf.Clamp(local.z, -Mathf.Abs(boxExtents.z), Mathf.Abs(boxExtents.z)));
            return Vector3.ProjectOnPlane(point - (boxCenter + (boxRotation * clamped)), Vector3.up).magnitude;
        }

        /// <summary>
        /// Degagement reel entre une trajectoire et une emprise orientee : la plus courte distance
        /// entre la polyligne et la boite, avec l'abscisse curviligne ou elle se produit.
        ///
        /// C'est le predicat d'appartenance au couloir de voie. Il repond "l'objet empiete-t-il sur
        /// ma voie", jamais "son centre est-il proche de mon axe".
        /// </summary>
        public static bool TryPathClearance(IReadOnlyList<Vector3> path, int count, Vector3 boxCenter,
            Vector3 boxExtents, Quaternion boxRotation, out float clearance, out float alongDistance,
            float corridor = 0f, float sampleStep = 0.5f)
        {
            clearance = float.PositiveInfinity; alongDistance = 0f;
            if (path == null || count < 2) return false;
            var length = PathLength(path, count);
            var step = Mathf.Max(0.25f, sampleStep);
            var samples = Mathf.Clamp(Mathf.CeilToInt(length / step) + 1, 2, 64);
            var resolved = false;
            var entry = float.PositiveInfinity;
            for (var i = 0; i < samples; i++)
            {
                var arc = Mathf.Min(length, i * step);
                var distance = PlanarDistanceToBox(SamplePath(path, count, arc), boxCenter, boxExtents, boxRotation);
                if (distance < clearance) { clearance = distance; alongDistance = arc; resolved = true; }

                // Abscisse d'ENTREE dans le couloir, et non abscisse du point le plus proche. Pour un
                // leader long, le minimum de distance tombe le long de son flanc, donc plusieurs metres
                // APRES son pare-chocs arriere : s'en servir comme ecart de suivi le surestimerait et
                // ferait coller le suiveur. C'est l'entree dans le couloir qui mesure l'ecart.
                if (corridor > 0f && distance <= corridor && arc < entry) entry = arc;
            }

            if (resolved && float.IsFinite(entry)) alongDistance = entry;
            return resolved;
        }

        /// <summary>
        /// Un usager occupe-t-il la VOIE DE SORTIE d'une jonction ? Trois conditions cumulatives, et
        /// c'est leur absence qui a produit ANO-5.18-01 :
        ///
        /// 1. EN AVAL -- son emprise croise le troncon [sortie, sortie + longueur utile]. Ce qui est en
        ///    amont est dans l'intersection, et releve de l'arbitrage de priorite ;
        /// 2. SUR LA VOIE -- son emprise empiete sur le couloir. Mesure sur l'EMPRISE, pas sur le
        ///    centre : dans le district, la voie de sens oppose passe a 4,00 m du noeud de sortie,
        ///    donc a l'interieur de tout rayon de proximite de 6 m. Une sphere declarait ainsi la
        ///    sortie saturee des qu'un vehicule arrivait EN FACE, des deux cotes a la fois, et les
        ///    deux s'arretaient definitivement ;
        /// 3. QUI NE DEGAGE PAS -- un usager qui s'eloigne dans le sens de la sortie libere la place.
        ///
        /// La fonction est pure et symetrique : deux pairs qui lisent le meme monde en tirent le meme
        /// verdict, ce dont depend l'arbitrage de jonction.
        /// </summary>
        public static bool OccupiesExitLane(Vector3 exitPoint, Vector3 laneDirection, float usableLength,
            float corridor, Vector3 occupantCenter, Vector3 occupantExtents, Quaternion occupantRotation,
            Vector3 occupantVelocity, float clearingSpeed)
        {
            var direction = Vector3.ProjectOnPlane(laneDirection, Vector3.up);
            if (direction.sqrMagnitude < 0.0001f || !float.IsFinite(usableLength) || usableLength <= 0f
                || !Finite(occupantCenter) || !ValidExtents(occupantExtents)) return false;
            direction.Normalize();
            if (!ValidRotation(occupantRotation)) occupantRotation = Quaternion.identity;

            var side = Vector3.Cross(Vector3.up, direction);
            var toOccupant = Vector3.ProjectOnPlane(occupantCenter - exitPoint, Vector3.up);

            var reachAlong = ProjectExtent(occupantExtents, occupantRotation, direction);
            var along = Vector3.Dot(toOccupant, direction);
            if (along + reachAlong < 0f || along - reachAlong > usableLength) return false;

            var lateral = Mathf.Abs(Vector3.Dot(toOccupant, side))
                - ProjectExtent(occupantExtents, occupantRotation, side);
            if (lateral > Mathf.Max(0f, corridor)) return false;

            return Vector3.Dot(occupantVelocity, direction) <= clearingSpeed;
        }

        /// <summary>Longueur cumulee d'une polyligne planaire. 0 en dessous de deux points.</summary>
        public static float PathLength(IReadOnlyList<Vector3> path, int count)
        {
            var total = 0f;
            for (var i = 1; path != null && i < count && i < path.Count; i++)
            {
                total += Vector3.ProjectOnPlane(path[i] - path[i - 1], Vector3.up).magnitude;
            }

            return total;
        }

        /// <summary>
        /// Projette un point sur une polyligne : abscisse curviligne, ecart lateral signe, tangente
        /// locale. C'est la primitive qui remplace "devant moi / a cote de moi" mesures sur la
        /// tangente du nez ; en virage les deux reponses different de plusieurs metres.
        /// </summary>
        public static bool TryProjectOnPath(IReadOnlyList<Vector3> path, int count, Vector3 point,
            out float alongDistance, out float lateralOffset, out Vector3 tangent)
        {
            alongDistance = 0f; lateralOffset = 0f; tangent = Vector3.forward;
            if (path == null || count < 2) return false;
            var travelled = 0f;
            var best = float.PositiveInfinity;
            var resolved = false;
            for (var i = 1; i < count && i < path.Count; i++)
            {
                var start = path[i - 1];
                var end = path[i];
                var leg = Vector3.ProjectOnPlane(end - start, Vector3.up);
                var length = leg.magnitude;
                if (length <= 0.0001f) continue;
                var direction = leg / length;
                var projection = ClosestPointOnSegment(point, start, end);
                var offset = Vector3.ProjectOnPlane(point - projection, Vector3.up);
                var distance = offset.sqrMagnitude;
                if (distance < best)
                {
                    best = distance;
                    alongDistance = travelled
                        + Vector3.Dot(Vector3.ProjectOnPlane(projection - start, Vector3.up), direction);
                    lateralOffset = Vector3.Dot(offset, Vector3.Cross(Vector3.up, direction));
                    tangent = direction;
                    resolved = true;
                }

                travelled += length;
            }

            return resolved;
        }

        /// <summary>
        /// Point d'une polyligne a l'abscisse curviligne donnee. Au-dela de la fin, rend le DERNIER
        /// point : une trajectoire qui s'epuise decrit un acteur qui s'arrete la, jamais un acteur qui
        /// continue tout droit dans le decor.
        /// </summary>
        public static Vector3 SamplePath(IReadOnlyList<Vector3> path, int count, float distance)
        {
            return SamplePath(path, distance, count, out _);
        }

        /// <summary>Meme echantillonnage, en rendant aussi la tangente locale (cap de l'emprise).</summary>
        public static Vector3 SamplePath(IReadOnlyList<Vector3> path, float distance, int count, out Vector3 tangent)
        {
            tangent = Vector3.forward;
            if (path == null || count <= 0 || path.Count == 0) return Vector3.zero;
            var first = path[0];
            if (count == 1) return first;
            var remaining = Mathf.Max(0f, distance);
            var last = first;
            for (var i = 1; i < count && i < path.Count; i++)
            {
                var leg = Vector3.ProjectOnPlane(path[i] - path[i - 1], Vector3.up);
                var length = leg.magnitude;
                if (length <= 0.0001f) continue;
                tangent = leg / length;
                last = path[i];
                if (remaining <= length) return path[i - 1] + tangent * remaining;
                remaining -= length;
            }

            return last;
        }

        /// <summary>
        /// Coeur du modele de decision (Story 5.18, correctif du retour terrain). Il repond a UNE
        /// question : nos deux trajectoires PREVUES se disputent-elles le meme espace au meme moment ?
        ///
        /// Deux etapes, et leur separation EST la correction :
        ///
        /// 1. GEOMETRIE -- premier point de notre trajectoire ou l'autre trajectoire passe a moins d'un
        ///    degagement (somme des demi-largeurs + marge). Deux voies paralleles separees de plus que
        ///    ce degagement n'ont AUCUN point de conflit, quels que soient les caps et les vitesses :
        ///    c'est ce qui rend un croisement en sens inverse sans objet.
        /// 2. CINEMATIQUE -- chacun occupe ce point pendant un intervalle borne par sa demi-longueur et
        ///    sa vitesse. Il n'y a conflit que si les deux intervalles se RECOUVRENT dans l'horizon :
        ///    deux vehicules qui passent au meme endroit a dix secondes d'ecart ne se genent pas.
        ///
        /// Un acteur immobile n'occupe que le point ou il se trouve deja : il bloque si notre
        /// trajectoire le traverse, et il est sans objet sinon. C'est exactement la difference entre un
        /// joueur arrete SUR la voie et un joueur arrete a cote.
        ///
        /// Chaque trajectoire est une POLYLIGNE, jamais une demi-droite : un vehicule qui tourne, qui
        /// circule sur un anneau de giratoire ou qui suit une courbe n'extrapole plus sa tangente dans
        /// le decor. Cette extrapolation rectiligne etait la cause des arrets sur rien.
        /// </summary>
        public static PathConflict FindPathConflict(
            IReadOnlyList<Vector3> ownPath, int ownCount, float ownSpeed, Vector3 ownExtents,
            IReadOnlyList<Vector3> otherPath, int otherCount, float otherSpeed,
            Vector3 otherExtents, Quaternion otherRotation,
            float margin, float horizon, float sampleStep = 1f)
        {
            if (ownPath == null || otherPath == null || ownCount < 2 || otherCount < 1
                || !Finite(ownExtents) || !Finite(otherExtents)
                || !float.IsFinite(horizon) || horizon <= 0f
                || !float.IsFinite(margin) || margin < 0f
                || !float.IsFinite(ownSpeed) || !float.IsFinite(otherSpeed)) return PathConflict.None;

            var step = Mathf.Max(0.25f, sampleStep);
            var ownRadius = new Vector2(ownExtents.x, ownExtents.z).magnitude;
            var otherRadius = new Vector2(otherExtents.x, otherExtents.z).magnitude
                + new Vector2(otherExtents.y, 0f).magnitude;
            var reach = Mathf.Max(0f, ownSpeed) * horizon + ownExtents.z + margin;
            var span = Mathf.Min(PathLength(ownPath, ownCount), reach);
            // ponytail: echantillonnage lineaire borne a 64 pas. Un index spatial ne se justifierait
            // qu'a une portee de perception tres superieure aux 20 m authores.
            var samples = Mathf.Clamp(Mathf.CeilToInt(span / step) + 1, 2, 64);

            var found = false;
            var conflictTime = float.PositiveInfinity;
            var conflictPoint = Vector3.zero;
            // Le point de RENCONTRE des deux voies -- la ou elles passent au plus pres -- est mesure
            // separement du premier recouvrement. Les deux ne coincident pas : le recouvrement
            // commence une demi-longueur avant. Departager sur le premier recouvrement donnerait a
            // chaque vehicule une avance apparente sur l'autre, donc "aucun des deux ne cede" et une
            // collision. Le point de rencontre, lui, est le MEME des deux cotes.
            var closest = float.PositiveInfinity;
            var meetOwnArc = 0f;
            var meetOtherArc = 0f;

            for (var i = 0; i < samples; i++)
            {
                var ownArc = Mathf.Min(span, i * step);
                var point = SamplePath(ownPath, ownArc, ownCount, out var ownTangent);
                if (!TryNearestOnPath(otherPath, otherCount, point, out var otherArc, out var separation,
                        out var otherPoint, out var otherTangent)) continue;

                if (separation < closest)
                {
                    closest = separation;
                    meetOwnArc = ownArc;
                    meetOtherArc = otherArc;
                }

                if (found) continue;

                // Filtre de rayon avant la passe exacte : il elimine la quasi-totalite des candidats
                // sans jamais decider seul, donc aucune approximation n'entre dans le verdict.
                if (separation > ownRadius + otherRadius + margin) continue;

                // Pose ORIENTEE des deux emprises. La conserver est ce qui empeche un mur oblique de
                // remplir un tunnel avec son enveloppe axe-aligne -- correctif acquis en Story 5.17,
                // qu'un test de disque aurait perdu.
                var ownPose = Quaternion.LookRotation(ownTangent, Vector3.up);
                var otherPose = otherCount >= 2 ? Quaternion.LookRotation(otherTangent, Vector3.up) : otherRotation;
                if (!ValidRotation(otherPose)) otherPose = Quaternion.identity;
                if (!PlanarBoxesOverlap(otherPoint - point, ownExtents, ownPose, otherExtents, otherPose, margin))
                    continue;

                // La demi-emprise a franchir au point de conflit couvre LES DEUX gabarits : en
                // croisement, on n'a quitte la place de l'autre qu'apres avoir depasse sa propre
                // longueur. Ne compter que la sienne faisait rater un croisement a 0,3 s d'ecart --
                // soit exactement le cas "joueur qui converge vite" que l'IA doit anticiper.
                var clearAlong = ProjectExtent(ownExtents, ownPose, ownTangent)
                    + ProjectExtent(otherExtents, otherPose, ownTangent) + margin;
                if (!TryOccupancy(ownArc, ownSpeed, clearAlong, out var ownEnter, out var ownExit)
                    || !TryOccupancy(otherArc, otherSpeed, clearAlong, out var otherEnter, out var otherExit))
                    continue;

                var begin = Mathf.Max(ownEnter, otherEnter);
                if (begin > Mathf.Min(ownExit, otherExit) || begin > horizon) continue;

                found = true;
                conflictTime = Mathf.Max(0f, begin);
                conflictPoint = point;
            }

            return found
                ? new PathConflict(true, conflictTime, conflictPoint,
                    Arrival(meetOwnArc, ownSpeed), Arrival(meetOtherArc, otherSpeed))
                : PathConflict.None;
        }

        /// <summary>
        /// Recouvrement de deux rectangles ORIENTES dans le plan : quatre axes separants suffisent en
        /// deux dimensions. La marge s'ajoute une fois, sur l'axe teste.
        /// </summary>
        public static bool PlanarBoxesOverlap(Vector3 offset, Vector3 ownExtents, Quaternion ownRotation,
            Vector3 otherExtents, Quaternion otherRotation, float margin)
        {
            for (var i = 0; i < 4; i++)
            {
                var axis = i == 0 ? ownRotation * Vector3.right
                    : i == 1 ? ownRotation * Vector3.forward
                    : i == 2 ? otherRotation * Vector3.right
                    : otherRotation * Vector3.forward;
                axis.y = 0f;
                if (axis.sqrMagnitude < 0.000001f) continue;
                axis.Normalize();
                var extent = ProjectExtent(ownExtents, ownRotation, axis)
                    + ProjectExtent(otherExtents, otherRotation, axis) + margin;
                if (Mathf.Abs(Vector3.Dot(offset, axis)) > extent) return false;
            }

            return true;
        }

        /// <summary>
        /// Qui passe en premier sur un point de conflit partage : celui qui l'atteint le plus tot. La
        /// fonction est ANTISYMETRIQUE par construction (comparaison stricte, puis identifiant), donc
        /// deux vehicules ne peuvent jamais conclure tous les deux "j'attends l'autre".
        ///
        /// Aucune regle de giratoire n'est necessaire : un vehicule deja engage sur l'anneau est plus
        /// proche du point de conflit qu'un vehicule encore a l'entree, donc il passe. C'est la
        /// priorite a l'anneau, obtenue sans la nommer -- et donc sans authoring a poser.
        /// </summary>
        public static bool YieldsAtConflict(float ownArrival, ulong ownId, float otherArrival, ulong otherId,
            float tieSeconds = 0.25f)
        {
            var own = float.IsFinite(ownArrival) ? ownArrival : float.MaxValue;
            var other = float.IsFinite(otherArrival) ? otherArrival : float.MaxValue;
            if (Mathf.Abs(own - other) > Mathf.Max(0f, tieSeconds)) return own > other;
            return ownId > otherId;
        }

        /// <summary>Intervalle d'occupation du point situe a <paramref name="arc"/> sur la trajectoire.</summary>
        private static bool TryOccupancy(float arc, float speed, float half, out float enter, out float exit)
        {
            enter = 0f; exit = 0f;
            if (!float.IsFinite(arc) || arc < 0f) return false;
            var travel = Mathf.Abs(speed);
            if (travel < 0.01f)
            {
                // Immobile : il n'occupe que la ou il se trouve deja, mais il y reste.
                if (arc > half) return false;
                exit = float.PositiveInfinity;
                return true;
            }

            enter = Mathf.Max(0f, (arc - half) / travel);
            exit = (arc + half) / travel;
            return true;
        }

        private static float Arrival(float arc, float speed)
        {
            var travel = Mathf.Abs(speed);
            return travel < 0.01f ? (arc <= 0.01f ? 0f : float.PositiveInfinity) : arc / travel;
        }

        private static bool TryNearestOnPath(IReadOnlyList<Vector3> path, int count, Vector3 point,
            out float alongDistance, out float separation, out Vector3 nearest, out Vector3 tangent)
        {
            alongDistance = 0f; separation = float.PositiveInfinity;
            nearest = Vector3.zero; tangent = Vector3.forward;
            if (path == null || count <= 0 || path.Count == 0) return false;
            if (count == 1)
            {
                nearest = path[0];
                separation = Vector3.ProjectOnPlane(point - nearest, Vector3.up).magnitude;
                return true;
            }

            var travelled = 0f;
            var resolved = false;
            for (var i = 1; i < count && i < path.Count; i++)
            {
                var start = path[i - 1];
                var end = path[i];
                var leg = Vector3.ProjectOnPlane(end - start, Vector3.up);
                var length = leg.magnitude;
                if (length <= 0.0001f) continue;
                var direction = leg / length;
                var projection = ClosestPointOnSegment(point, start, end);
                var distance = Vector3.ProjectOnPlane(point - projection, Vector3.up).magnitude;
                if (distance < separation)
                {
                    separation = distance;
                    nearest = projection;
                    tangent = direction;
                    alongDistance = travelled
                        + Vector3.Dot(Vector3.ProjectOnPlane(projection - start, Vector3.up), direction);
                    resolved = true;
                }

                travelled += length;
            }

            return resolved;
        }

        /// <summary>Compatibilite pour les rectangles plans deja exprimes dans le meme repere.</summary>
        public static bool PredictCollision(Vector3 relativePosition, Vector3 relativeVelocity,
            Vector3 combinedExtents, float horizon, out float time, out Vector3 position)
        {
            time = 0f; position = relativePosition;
            if (!Finite(relativePosition) || !Finite(relativeVelocity) || !Finite(combinedExtents)
                || combinedExtents.x < 0f || combinedExtents.y < 0f || combinedExtents.z < 0f
                || !float.IsFinite(horizon) || horizon < 0f) return false;
            var end = horizon;
            if (!IntersectAxis(relativePosition.x, relativeVelocity.x, combinedExtents.x, ref time, ref end)
                || !IntersectAxis(relativePosition.z, relativeVelocity.z, combinedExtents.z, ref time, ref end)) return false;
            position = relativePosition + relativeVelocity * time;
            return true;
        }

        /// <summary>
        /// Intersection continue de deux boites orientees en translation. Les quinze axes separants
        /// partagent le MEME intervalle de temps : un mur diagonal ne remplit plus artificiellement
        /// le tunnel avec son AABB. Les rotations sont celles du snapshot, rafraichi a cadence bornee.
        /// </summary>
        public static bool PredictOrientedCollision(Vector3 relativePosition, Vector3 relativeVelocity,
            Vector3 ownExtents, Quaternion ownRotation, Vector3 otherExtents, Quaternion otherRotation,
            float margin, float horizon, out float time, out Vector3 position)
        {
            time = 0f; position = relativePosition;
            if (!Finite(relativePosition) || !Finite(relativeVelocity) || !ValidExtents(ownExtents)
                || !ValidExtents(otherExtents) || !ValidRotation(ownRotation) || !ValidRotation(otherRotation)
                || !float.IsFinite(margin) || margin < 0f || !float.IsFinite(horizon) || horizon < 0f) return false;
            var end = horizon;
            for (var i = 0; i < 3; i++)
            {
                var ownAxis = ownRotation * Axis(i);
                var otherAxis = otherRotation * Axis(i);
                if (!IntersectOrientedAxis(ownAxis, relativePosition, relativeVelocity,
                        ownExtents, ownRotation, otherExtents, otherRotation, margin, ref time, ref end)
                    || !IntersectOrientedAxis(otherAxis, relativePosition, relativeVelocity,
                        ownExtents, ownRotation, otherExtents, otherRotation, margin, ref time, ref end)) return false;
                for (var j = 0; j < 3; j++)
                    if (!IntersectOrientedAxis(Vector3.Cross(ownAxis, otherRotation * Axis(j)), relativePosition,
                        relativeVelocity, ownExtents, ownRotation, otherExtents, otherRotation,
                        margin, ref time, ref end)) return false;
            }
            position = relativePosition + relativeVelocity * time;
            return true;
        }

        public static float ProjectExtent(Vector3 extents, Quaternion rotation, Vector3 axis)
        {
            var localAxis = Quaternion.Inverse(rotation) * axis;
            return Mathf.Abs(localAxis.x) * extents.x + Mathf.Abs(localAxis.y) * extents.y
                + Mathf.Abs(localAxis.z) * extents.z;
        }

        private static bool IntersectOrientedAxis(Vector3 axis, Vector3 offset, Vector3 velocity,
            Vector3 ownExtents, Quaternion ownRotation, Vector3 otherExtents, Quaternion otherRotation,
            float margin, ref float start, ref float end)
        {
            if (axis.sqrMagnitude < 0.000001f) return true;
            axis.Normalize();
            var extent = ProjectExtent(ownExtents, ownRotation, axis)
                + ProjectExtent(otherExtents, otherRotation, axis) + margin;
            return IntersectAxis(Vector3.Dot(offset, axis), Vector3.Dot(velocity, axis), extent, ref start, ref end);
        }

        private static Vector3 Axis(int index) => index == 0 ? Vector3.right : index == 1 ? Vector3.up : Vector3.forward;
        private static bool ValidExtents(Vector3 extents) => Finite(extents) && extents.x >= 0f && extents.y >= 0f && extents.z >= 0f;
        private static bool ValidRotation(Quaternion rotation) => float.IsFinite(rotation.x) && float.IsFinite(rotation.y)
            && float.IsFinite(rotation.z) && float.IsFinite(rotation.w)
            && Mathf.Abs(Quaternion.Dot(rotation, rotation) - 1f) < 0.001f;

        private static bool IntersectAxis(float position, float speed, float extent, ref float start, ref float end)
        {
            if (Mathf.Abs(speed) < 0.0001f) return Mathf.Abs(position) <= extent;
            var a = (-extent - position) / speed;
            var b = (extent - position) / speed;
            start = Mathf.Max(start, Mathf.Min(a, b));
            end = Mathf.Min(end, Mathf.Max(a, b));
            return start <= end;
        }

        /// <summary>Both peers compare the same ordering; the controller latches until separated.</summary>
        public static bool MustYield(bool selfCorrect, float selfLaneDistance, ulong selfId,
            bool otherCorrect, float otherLaneDistance, ulong otherId)
        {
            if (selfCorrect != otherCorrect) return !selfCorrect;
            if (selfCorrect && !Mathf.Approximately(selfLaneDistance, otherLaneDistance))
                return selfLaneDistance > otherLaneDistance;
            return selfId > otherId;
        }

        /// <summary>
        /// LIGNE DE CESSION d'un conflit de trajectoire : ecart (m) entre l'AVANT du vehicule et le
        /// point ou il devrait etre arrete, lorsque ce point est encore devant lui.
        ///
        /// Ceder n'est pas s'arreter la ou l'on apercoit le conflit : c'est ne pas y entrer. Sans
        /// cette grandeur, une cession detectee a la portee de prediction produit un freinage a la
        /// position courante -- au milieu d'un troncon, parfois a la bouche d'un tunnel -- au lieu
        /// d'un ralentissement qui se termine devant l'aire de conflit.
        ///
        /// La ligne est le point de rencontre recule de la demi-largeur du vehicule et de la marge
        /// authoree : la meme construction que la ligne d'arret d'une approche authoree, appliquee a
        /// une geometrie que le graphe ne decrit pas (giratoires du district : aucune approche).
        ///
        /// La projection se fait sur le CAP, donc sur la corde. En courbe elle sous-estime la
        /// distance restante et arrete donc un peu en deca : le seul sens acceptable pour une
        /// cession. Rend une valeur negative quand le point est deja atteint ou depasse.
        /// </summary>
        public static float ResolveConflictStopGap(Vector3 position, Vector3 forward, Vector3 conflictPoint,
            float frontOffset, float halfWidth, float margin)
        {
            var heading = Vector3.ProjectOnPlane(forward, Vector3.up);
            if (heading.sqrMagnitude <= 0.000001f) return float.NegativeInfinity;

            var along = Vector3.Dot(Vector3.ProjectOnPlane(conflictPoint - position, Vector3.up), heading.normalized);
            return along - Mathf.Max(0f, frontOffset) - Mathf.Max(0f, halfWidth) - Mathf.Max(0f, margin);
        }

        public static Vector3 ClosestPointOnSegment(Vector3 position, Vector3 start, Vector3 end)
        {
            var delta = end - start; delta.y = 0f;
            return start + delta * (delta.sqrMagnitude > 0.0001f
                ? Mathf.Clamp01(Vector3.Dot(position - start, delta) / delta.sqrMagnitude) : 0f);
        }

        private static bool Finite(Vector3 value) => float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
    }
}
