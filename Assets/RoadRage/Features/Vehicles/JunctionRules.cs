using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.18 : regle de priorite authoree sur une approche de jonction. C'est de la DONNEE, lue
    /// une fois a la construction du graphe et jamais recalculee par frame.
    ///
    /// <see cref="PriorityToRight"/> est la valeur par defaut : c'est la regle qui s'applique quand
    /// aucune route prioritaire n'est authoree, exactement comme dans le code de la route.
    /// </summary>
    public enum JunctionApproachRule
    {
        /// <summary>Priorite a droite : l'approche cede a celle qui vient sur sa droite. Repli quand rien n'est authore.</summary>
        PriorityToRight = 0,

        /// <summary>Route prioritaire : l'approche a la priorite sur celles qui ne la portent pas.</summary>
        PriorityRoad = 1,

        /// <summary>Stop : arret complet, maintien authore, puis franchissement seulement avec un ecart accepte.</summary>
        Stop = 2,

        /// <summary>Feu : l'approche franchit sans arret quand sa phase l'autorise, et attend sinon.</summary>
        TrafficLight = 3
    }

    /// <summary>Verdict rendu par l'arbitrage d'une approche. Deux verdicts, jamais trois : la symetrie en depend.</summary>
    public enum JunctionVerdict
    {
        Wait = 0,
        Proceed = 1
    }

    /// <summary>
    /// Une phase d'un plan de feux : sa duree et les groupes d'approches qu'elle autorise. Un groupe
    /// non liste dans la phase courante est au rouge -- il n'existe pas de troisieme etat, donc pas
    /// d'orange a arbitrer.
    /// </summary>
    [Serializable]
    public struct TrafficSignalPhase
    {
        [SerializeField]
        [Min(0.1f)]
        [Tooltip("Duree de la phase, en secondes. Doit atteindre la garde de vert minimal du plan.")]
        private float durationSeconds;

        [SerializeField]
        [Tooltip("Groupes d'approches autorises pendant cette phase. Les autres sont au rouge.")]
        private int[] greenGroups;

        public TrafficSignalPhase(float durationSeconds, int[] greenGroups)
        {
            this.durationSeconds = durationSeconds;
            this.greenGroups = greenGroups ?? Array.Empty<int>();
        }

        public float DurationSeconds
        {
            get { return durationSeconds; }
        }

        public IReadOnlyList<int> GreenGroups
        {
            get { return greenGroups ?? (IReadOnlyList<int>)Array.Empty<int>(); }
        }

        /// <summary>Vrai si cette phase autorise le groupe donne. Un groupe negatif n'existe pas.</summary>
        public bool AllowsGroup(int group)
        {
            var groups = greenGroups;
            if (groups == null || group < 0)
            {
                return false;
            }

            for (var i = 0; i < groups.Length; i++)
            {
                if (groups[i] == group)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Plan de feux d'une jonction. Il ne porte AUCUN etat replique : c'est une fonction du temps
    /// partage, donc les deux pairs tirent la meme phase de la meme horloge et de la meme donnee.
    /// </summary>
    [Serializable]
    public struct TrafficSignalPlan
    {
        [SerializeField]
        [Tooltip("Id de jonction porte par les LaneNode concernes. Un plan s'applique a toutes les jonctions qui portent cet id.")]
        private string junctionId;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Garde de vert minimal (s) : une phase plus courte que cette valeur est refusee a l'authoring, car elle ferait clignoter le feu.")]
        private float minimumGreenSeconds;

        [SerializeField]
        [Tooltip("Cycle des phases, dans l'ordre. Au moins deux phases : un plan a une seule phase n'arbitre rien.")]
        private TrafficSignalPhase[] phases;

        public TrafficSignalPlan(string junctionId, float minimumGreenSeconds, TrafficSignalPhase[] phases)
        {
            this.junctionId = junctionId;
            this.minimumGreenSeconds = minimumGreenSeconds;
            this.phases = phases ?? Array.Empty<TrafficSignalPhase>();
        }

        public string JunctionId
        {
            get { return junctionId ?? string.Empty; }
        }

        public float MinimumGreenSeconds
        {
            get { return minimumGreenSeconds; }
        }

        public IReadOnlyList<TrafficSignalPhase> Phases
        {
            get { return phases ?? (IReadOnlyList<TrafficSignalPhase>)Array.Empty<TrafficSignalPhase>(); }
        }

        /// <summary>Duree totale du cycle, ou 0 si le plan n'est pas exploitable.</summary>
        public float CycleSeconds
        {
            get
            {
                var phases = this.phases;
                if (phases == null)
                {
                    return 0f;
                }

                var total = 0f;
                for (var i = 0; i < phases.Length; i++)
                {
                    if (float.IsFinite(phases[i].DurationSeconds) && phases[i].DurationSeconds > 0f)
                    {
                        total += phases[i].DurationSeconds;
                        if (!float.IsFinite(total))
                        {
                            return 0f;
                        }
                    }
                }

                return total;
            }
        }
    }

    /// <summary>
    /// Story 5.18 : revendication d'une approche de jonction, telle que l'evalue un pair. C'est une
    /// structure de VALEUR, sans reference a un autre vehicule : tout ce dont l'arbitrage a besoin y
    /// est deja resolu, donc deux appels symetriques ne peuvent pas lire deux verites differentes.
    /// </summary>
    public readonly struct JunctionClaim
    {
        public JunctionClaim(
            int junctionKey,
            JunctionApproachRule rule,
            Vector3 approachForward,
            float distanceToJunction,
            int laneId,
            ulong networkObjectId,
            bool hasExitRoom,
            bool stopSatisfied,
            bool signalAllows,
            bool breach,
            Vector3 entryPoint = default,
            Vector3 exitPoint = default,
            bool committed = false)
        {
            JunctionKey = junctionKey;
            Rule = JunctionRules.NormalizeRule(rule);
            ApproachForward = Sanitize(approachForward);
            DistanceToJunction = distanceToJunction;
            LaneId = laneId;
            NetworkObjectId = networkObjectId;
            HasExitRoom = hasExitRoom;
            StopSatisfied = stopSatisfied;
            SignalAllows = signalAllows;
            Breach = breach;
            EntryPoint = entryPoint;
            ExitPoint = exitPoint;
            Committed = committed;
        }

        /// <summary>Jonction revendiquee. Toute valeur &lt;= 0 signifie "ce noeud n'est pas une approche" : aucun arbitrage.</summary>
        public int JunctionKey { get; }

        /// <summary>Regle authoree sur l'approche, ramenee dans son domaine.</summary>
        public JunctionApproachRule Rule { get; }

        /// <summary>Cap de l'approche, projete sur le plan et normalise. Sert au test "sens oppose" et a la priorite a droite.</summary>
        public Vector3 ApproachForward { get; }

        /// <summary>Distance planaire au noeud de decision de l'approche (m).</summary>
        public float DistanceToJunction { get; }

        /// <summary>Identite de la voie d'approche : l'index du noeud dans le graphe, identique sur tous les pairs.</summary>
        public int LaneId { get; }

        public ulong NetworkObjectId { get; }

        /// <summary>Place disponible sur la voie de sortie. Faux couvre le cas indetermine : on traite comme sature, jamais comme libre.</summary>
        public bool HasExitRoom { get; }

        /// <summary>
        /// Ecart accepte atteint envers les autres revendiquants. Sans objet (donc vrai) hors regle Stop,
        /// et vrai aussi quand personne d'autre ne se dispute la jonction.
        ///
        /// C'est une condition SYMETRIQUE, et c'est deliberé : la distance entre deux vehicules se lit des
        /// deux cotes, donc les deux en tirent le meme verdict. Le MAINTIEN a l'arret authore, lui, n'est
        /// observable que par le vehicule lui-meme : il reste une garde LOCALE appliquee par le controleur,
        /// hors de l'arbitrage. L'y faire entrer rendrait les deux verdicts dissymetriques pendant tout le
        /// temps du maintien et ferait attendre les deux vehicules a la fois.
        /// </summary>
        public bool StopSatisfied { get; }

        /// <summary>Phase courante du feu autorisant cette approche. Sans objet (donc vrai) hors regle feu, et quand aucun plan n'existe.</summary>
        public bool SignalAllows { get; }

        /// <summary>Vrai quand le vehicule a attendu plus que le delai authore et assouplit sa propre regle pour debloquer la jonction.</summary>
        public bool Breach { get; }

        /// <summary>Trajectoire effectivement choisie a l'approche. Absente dans les fixtures historiques.</summary>
        public Vector3 EntryPoint { get; }
        public Vector3 ExitPoint { get; }

        /// <summary>
        /// Vrai quand le vehicule OCCUPE deja la jonction : son mouvement lui a ete accorde, il est
        /// engage, et il n'a pas encore degage.
        ///
        /// Sans cette notion, un vehicule disparaissait de l'arbitrage a l'instant meme ou il entrait
        /// -- sa revendication se lisait sur son noeud d'approche, qu'il venait de quitter. Le suivant
        /// ne voyait donc aucun concurrent et entrait a son tour : les deux se rencontraient au milieu
        /// (ANO-5.18-03, carrefour central). Une occupation reelle est ce que l'arbitrage n'avait pas.
        /// </summary>
        public bool Committed { get; }
        public bool HasRouteGeometry => Vector3.ProjectOnPlane(ExitPoint - EntryPoint, Vector3.up).sqrMagnitude > 0.0001f;

        /// <summary>
        /// Eligibilite, en un SEUL scalaire monotone -- c'est ce qui rend l'arbitrage symetrique par
        /// construction : deux revendications se comparent sans jamais inverser le sens du test.
        ///
        /// L'ordre des paliers est celui de la spec : la place sur la voie de sortie d'abord, puis
        /// l'autorisation du feu, puis le stop satisfait.
        /// </summary>
        public int Eligibility
        {
            get
            {
                if (!HasExitRoom)
                {
                    return 0;
                }

                if (Rule == JunctionApproachRule.TrafficLight && !SignalAllows)
                {
                    return 1;
                }

                if (Rule == JunctionApproachRule.Stop && !StopSatisfied)
                {
                    return 2;
                }

                return 3;
            }
        }

        /// <summary>
        /// Rang de priorite effective, plus grand = passe d'abord. Le palier de deblocage prime tout :
        /// c'est ce qui garantit qu'un vehicule en interblocage finit par entrer plutot que d'attendre
        /// indefiniment, sans qu'aucun palier de retrait n'existe.
        /// </summary>
        public int Rank
        {
            get
            {
                if (Breach)
                {
                    return 4;
                }

                switch (Rule)
                {
                    case JunctionApproachRule.PriorityRoad:
                        return 3;
                    case JunctionApproachRule.TrafficLight:
                        return 2;
                    case JunctionApproachRule.PriorityToRight:
                        return 1;
                    default:
                        return 0;
                }
            }
        }

        /// <summary>Copie de la revendication avec le palier de deblocage pose ou retire.</summary>
        public JunctionClaim WithBreach(bool breach)
        {
            return new JunctionClaim(JunctionKey, Rule, ApproachForward, DistanceToJunction, LaneId,
                NetworkObjectId, HasExitRoom, StopSatisfied, SignalAllows, breach, EntryPoint, ExitPoint, Committed);
        }

        /// <summary>Copie de la revendication avec une autre regle d'approche.</summary>
        public JunctionClaim WithRule(JunctionApproachRule rule)
        {
            return new JunctionClaim(JunctionKey, rule, ApproachForward, DistanceToJunction, LaneId,
                NetworkObjectId, HasExitRoom, StopSatisfied, SignalAllows, Breach, EntryPoint, ExitPoint, Committed);
        }

        /// <summary>
        /// Copie de la revendication avec un autre verdict d'ecart accepte. C'est la seule valeur qui
        /// depend d'un COUPLE de revendications : elle se calcule donc apres que l'ensemble des
        /// revendiquants a ete rassemble, et jamais pendant qu'on en lit un.
        /// </summary>
        public JunctionClaim WithStopSatisfied(bool stopSatisfied)
        {
            return new JunctionClaim(JunctionKey, Rule, ApproachForward, DistanceToJunction, LaneId,
                NetworkObjectId, HasExitRoom, stopSatisfied, SignalAllows, Breach, EntryPoint, ExitPoint, Committed);
        }

        private static Vector3 Sanitize(Vector3 forward)
        {
            var planar = new Vector3(forward.x, 0f, forward.z);
            if (!float.IsFinite(planar.x) || !float.IsFinite(planar.z) || planar.sqrMagnitude <= 0.0001f)
            {
                return Vector3.forward;
            }

            return planar.normalized;
        }
    }

    /// <summary>
    /// Story 5.18 : arbitrage d'intersection, PUR et SYMETRIQUE. Aucune scene, aucun Netcode, aucun
    /// etat : la garantie anti-interblocage se prouve donc en EditMode, sans rien executer.
    ///
    /// L'invariance par echange est le coeur du mecanisme. Chaque vehicule n'observe pas un "tour de
    /// passage" mais une fonction symetrique de l'etat partage : les deux parties enoncent le meme
    /// gagnant sans se parler. C'est le motif deja en place pour les conflits frontaux
    /// (<see cref="TrafficPerception.MustYield"/>), etendu ici aux jonctions en ajoutant la regle
    /// authoree comme premier critere et l'ordre total (distance, voie, identifiant) comme dernier.
    /// </summary>
    public static class JunctionRules
    {
        /// <summary>
        /// Angle a partir duquel deux approches de la meme jonction sont lues comme OPPOSEES, donc
        /// non concurrentes : les deux sens d'une meme avenue ne s'arbitrent pas l'un contre l'autre.
        /// Sous ce seuil (et jusqu'a la perpendiculaire) elles se croisent ou fusionnent.
        /// </summary>
        public const float OppositeApproachDegrees = 150f;

        /// <summary>
        /// Tolerance (m) en dessous de laquelle deux distances a la jonction sont lues comme EGALES.
        ///
        /// Elle absorbe un pas de physique de mouvement : chaque vehicule lit la position de l'autre au
        /// pas precedent, donc les deux distances ne coincident pas exactement. Sans cette tolerance,
        /// deux revendications quasi egales pouvaient chacune se croire la plus proche du MEME couple de
        /// valeurs -- et les deux franchir. Les egalites ainsi reconnues passent a l'ordre total, qui est
        /// stable des deux cotes (voie, puis identifiant).
        /// </summary>
        public const float DistanceTieToleranceMeters = 1f;

        private static readonly float OppositeApproachDot = Mathf.Cos(OppositeApproachDegrees * Mathf.Deg2Rad);

        /// <summary>Ramene toute valeur serialisee hors bornes sur la regle de repli, sans lever.</summary>
        /// <summary>
        /// PRIORITE A L'ANNEAU. Qui passe, entre un vehicule qui CIRCULE dans un giratoire et un
        /// vehicule qui cherche a y ENTRER ? Rend +1 si nous passons, -1 si nous cedons, 0 si la
        /// regle ne dit rien.
        ///
        /// Un giratoire n'est pas une course : celui qui y est engage passe, toujours, quel que soit
        /// celui qui atteindrait le point de conflit en premier. C'est precisement ce que l'ordre
        /// d'arrivee ne sait pas dire -- un vehicule lance vers l'entree peut atteindre le point de
        /// conflit AVANT celui qui fait le tour, et passer devant lui. C'est le defaut rapporte en
        /// recette, et il ne se corrige pas en ajustant des temps : il manquait une regle.
        ///
        /// La priorite a droite, qui est le repli du reste du reseau, y est de surcroit INVERSEE :
        /// en circulation a droite l'anneau tourne dans le sens antihoraire, donc le vehicule qui
        /// circule arrive par la GAUCHE de celui qui entre. Appliquer le repli a un giratoire
        /// donnerait donc la priorite a l'entrant -- exactement l'inverse de la regle.
        ///
        /// La fonction est ANTISYMETRIQUE par construction : echanger les deux arguments change le
        /// signe du resultat. Les deux pairs lisent la meme donnee d'anneau, indexee une fois sur le
        /// graphe, et en tirent donc des verdicts opposes sans se parler.
        ///
        /// Deux vehicules du MEME anneau ne sont pas departages ici : entre eux il n'y a pas
        /// d'entrant, seulement une file, et c'est l'ordre d'arrivee qui a raison.
        /// </summary>
        /// <summary>
        /// PALIER DE RECUPERATION correspondant a une duree SANS PROGRES DE ROUTE.
        ///
        /// Un seul escalier, un seul declencheur. Les quatre causes d'arret durable observees en
        /// recette -- file qui n'avance plus, cession a un pair qui n'arrivera jamais, face-a-face,
        /// enchevetrement physique -- ne different que par leur NOM ; leur consequence est la meme,
        /// et la reponse doit l'etre aussi. Avant, chacune avait son horloge et son echappatoire,
        /// et le face-a-face n'en avait aucune.
        ///
        /// Les seuils sont ceux DEJA AUTHORES : le delai de klaxon ouvre l'evitement local, puis
        /// chaque palier suivant coute un delai d'escalade de jonction supplementaire. Aucune
        /// constante de temps nouvelle n'entre dans le modele.
        ///
        /// 0 : conduite nominale -- les regles ordinaires s'appliquent ;
        /// 1 : evitement local (contournement sur la chaussee) ;
        /// 2 : recul / repositionnement ;
        /// 3 : manoeuvre elargie, espace voisin et trottoir ;
        /// 4 : dernier recours -- contact limite tolere plutot qu'un blocage definitif.
        /// </summary>
        public static int ResolveRecoveryStage(float stalledSeconds, float hornDelay, float escalationDelay)
        {
            if (!float.IsFinite(stalledSeconds) || stalledSeconds <= 0f) return 0;
            var first = Mathf.Max(0.1f, hornDelay);
            if (stalledSeconds < first) return 0;

            var step = Mathf.Max(0.1f, escalationDelay);
            return Mathf.Clamp(1 + Mathf.FloorToInt((stalledSeconds - first) / step), 1, 4);
        }

        public static int CompareRingPrecedence(int ownRing, int peerRing)
        {
            if (ownRing == peerRing) return 0;
            if (ownRing != 0 && peerRing == 0) return 1;
            if (ownRing == 0 && peerRing != 0) return -1;
            return 0;
        }

        /// <summary>
        /// Ce vehicule rompt-il le cycle d'attente auquel il appartient ?
        ///
        /// Un CYCLE est la preuve qu'aucun de ses membres ne peut avancer par le jeu normal des
        /// regles : chacun attend un membre du cycle, donc l'attente n'a aucun debouche. C'est la
        /// seule situation qui justifie de passer outre une cession -- et c'est ce qui la distingue
        /// d'une file lente ou d'une saturation reelle, ou l'attente a un debouche et doit courir.
        ///
        /// La rupture emprunte le MEME ordre total que tout le reste de l'arbitrage : le plus petit
        /// identifiant reseau passe. La fonction est donc COHERENTE PAR CONSTRUCTION -- tous les
        /// membres parcourent le meme cycle, en tirent le meme minimum, et exactement un obtient vrai.
        /// Aucun minuteur n'intervient : un delai confondrait la file lente avec l'interblocage, et
        /// deux vehicules pourraient l'atteindre au meme instant.
        ///
        /// <paramref name="others"/> porte les autres membres du cycle, dans n'importe quel ordre.
        /// Une liste vide signifie qu'aucun cycle n'a ete observe : personne ne rompt quoi que ce soit.
        /// </summary>
        public static bool ReleasesWaitCycle(ulong self, IReadOnlyList<ulong> others, int count)
        {
            if (others == null || count <= 0) return false;

            for (var i = 0; i < count && i < others.Count; i++)
            {
                if (others[i] == self) continue;
                if (others[i] < self) return false;
            }

            return true;
        }

        public static JunctionApproachRule NormalizeRule(JunctionApproachRule rule)
        {
            return rule == JunctionApproachRule.PriorityRoad || rule == JunctionApproachRule.Stop
                || rule == JunctionApproachRule.TrafficLight
                ? rule
                : JunctionApproachRule.PriorityToRight;
        }

        /// <summary>
        /// Deux revendications se disputent-elles la meme jonction ? Vrai si elles portent la meme
        /// jonction et qu'elles viennent de DEUX voies differentes. Quand les deux sorties sont deja
        /// choisies, le test porte sur leurs trajectoires de voie, pas sur le seul cap entrant : un
        /// tourne-a-gauche face a un tout-droit oppose est donc bien concurrent, sans bloquer deux
        /// tout-droits sur des voies distinctes.
        /// Deux vehicules de la meme voie ne s'arbitrent pas : c'est une poursuite, elle appartient a
        /// la perception de la Story 5.17.
        /// </summary>
        public static bool Conflicts(in JunctionClaim own, in JunctionClaim other)
        {
            if (own.JunctionKey <= 0 || own.JunctionKey != other.JunctionKey)
            {
                return false;
            }

            if (own.LaneId == other.LaneId)
            {
                return false;
            }

            if (own.HasRouteGeometry && other.HasRouteGeometry)
            {
                return PathsIntersect(own.EntryPoint, own.ExitPoint, other.EntryPoint, other.ExitPoint);
            }

            return Vector3.Dot(own.ApproachForward, other.ApproachForward) > OppositeApproachDot;
        }

        private static bool PathsIntersect(Vector3 firstStart, Vector3 firstEnd, Vector3 secondStart, Vector3 secondEnd)
        {
            var first = Vector3.ProjectOnPlane(firstEnd - firstStart, Vector3.up);
            var second = Vector3.ProjectOnPlane(secondEnd - secondStart, Vector3.up);
            var offset = Vector3.ProjectOnPlane(secondStart - firstStart, Vector3.up);
            var denominator = Cross(first, second);
            if (Mathf.Abs(denominator) < 0.0001f)
            {
                // Des voies distinctes peuvent se rejoindre sur le meme axe de sortie. Elles ne
                // se coupent pas au sens strict, mais leur troncon commun reste bien une occupation
                // concurrente de la jonction.
                if (Mathf.Abs(Cross(offset, first)) >= 0.0001f)
                {
                    return false;
                }

                var firstLengthSquared = Vector3.Dot(first, first);
                if (firstLengthSquared < 0.0001f)
                {
                    return false;
                }

                var secondProjectionStart = Vector3.Dot(offset, first) / firstLengthSquared;
                var secondProjectionEnd = secondProjectionStart + Vector3.Dot(second, first) / firstLengthSquared;
                return Mathf.Max(0f, Mathf.Min(secondProjectionStart, secondProjectionEnd))
                    <= Mathf.Min(1f, Mathf.Max(secondProjectionStart, secondProjectionEnd));
            }

            var firstT = Cross(offset, second) / denominator;
            var secondT = Cross(offset, first) / denominator;
            return firstT >= 0f && firstT <= 1f && secondT >= 0f && secondT <= 1f;
        }

        private static float Cross(Vector3 first, Vector3 second) => first.x * second.z - first.z * second.x;

        /// <summary>
        /// Arbitrage d'une paire. Le verdict de <paramref name="own"/> est l'exact inverse de celui
        /// rendu pour <paramref name="other"/> : c'est la propriete qui rend "les deux attendent
        /// l'autre" impossible, et elle est testee comme telle.
        /// </summary>
        public static JunctionVerdict Resolve(in JunctionClaim own, in JunctionClaim other)
        {
            if (!Conflicts(own, other))
            {
                // Aucune concurrence : la place de l'autre ne regarde pas cette approche.
                return JunctionVerdict.Proceed;
            }

            // OCCUPATION AVANT PRIORITE. Un vehicule deja engage dans la jonction a recu son
            // mouvement ; le lui reprendre parce qu'il vient de gauche l'immobiliserait au milieu de
            // l'aire de conflit, ce que la story interdit explicitement. Le test reste antisymetrique :
            // deux occupants concurrents -- qui ne devraient pas exister, puisque le second n'aurait
            // pas ete admis -- retombent sur l'arbitrage ordinaire, et le freinage d'urgence couvre le
            // cas degenere.
            if (own.Committed != other.Committed)
            {
                return own.Committed ? JunctionVerdict.Proceed : JunctionVerdict.Wait;
            }

            // Un scalaire par revendication, jamais un signe a inverser : l'eligibilite encode
            // l'ordre de la spec (place de sortie, puis feu, puis stop).
            if (own.Eligibility != other.Eligibility)
            {
                return own.Eligibility > other.Eligibility ? JunctionVerdict.Proceed : JunctionVerdict.Wait;
            }

            if (own.Rank != other.Rank)
            {
                return own.Rank > other.Rank ? JunctionVerdict.Proceed : JunctionVerdict.Wait;
            }

            // A regle et eligibilite egales, la priorite a droite tranche -- et elle tranche TOUJOURS
            // pour deux approches qui se croisent, car le produit scalaire vaut alors plus ou moins 1.
            if (ComesFromTheRight(own, other))
            {
                return JunctionVerdict.Wait;
            }

            if (ComesFromTheRight(other, own))
            {
                return JunctionVerdict.Proceed;
            }

            // Dernier recours : ordre total (plus proche, puis voie, puis NetworkObjectId). L'egalite
            // n'est atteignable que par une revendication face a ELLE-MEME (meme distance, meme voie,
            // meme identifiant : c'est le meme vehicule), et elle rend alors `Proceed` -- jamais
            // `Wait`. "Aucun etat ou les deux attendent l'autre" prime aussi sur ce cas degenere.
            return CompareTotalOrder(own, other) >= 0 ? JunctionVerdict.Proceed : JunctionVerdict.Wait;
        }

        /// <summary>
        /// Vrai si <paramref name="other"/> arrive sur la droite de <paramref name="own"/> : il vient
        /// de la direction de sa droite. Antisymetrique des que les deux caps sont perpendiculaires,
        /// ce qui est exactement le cas des approches qui se croisent.
        /// </summary>
        public static bool ComesFromTheRight(in JunctionClaim own, in JunctionClaim other)
        {
            var ownRight = Vector3.Cross(Vector3.up, own.ApproachForward);
            return Vector3.Dot(ownRight, -other.ApproachForward) > 0f;
        }

        /// <summary>
        /// Ordre total de departage : plus proche de la jonction gagne, puis index de voie croissant,
        /// puis <c>NetworkObjectId</c> croissant. Rend &gt; 0 si <paramref name="own"/> gagne, &lt; 0
        /// s'il perd, et 0 seulement pour une revendication identique a elle-meme.
        /// </summary>
        public static int CompareTotalOrder(in JunctionClaim own, in JunctionClaim other)
        {
            var ownDistance = float.IsFinite(own.DistanceToJunction) ? own.DistanceToJunction : float.MaxValue;
            var otherDistance = float.IsFinite(other.DistanceToJunction) ? other.DistanceToJunction : float.MaxValue;
            if (Mathf.Abs(ownDistance - otherDistance) > DistanceTieToleranceMeters)
            {
                return ownDistance < otherDistance ? 1 : -1;
            }

            if (own.LaneId != other.LaneId)
            {
                return own.LaneId < other.LaneId ? 1 : -1;
            }

            if (own.NetworkObjectId != other.NetworkObjectId)
            {
                return own.NetworkObjectId < other.NetworkObjectId ? 1 : -1;
            }

            return 0;
        }

        /// <summary>
        /// Index de la phase active a l'instant donne, ou -1 si le plan n'est pas exploitable (aucune
        /// phase, duree nulle, valeur non finie). Fonction du TEMPS seul : deux pairs qui partagent
        /// l'horloge et la donnee tirent la meme phase, donc aucun etat reseau n'est ajoute.
        /// </summary>
        public static int ResolvePhaseIndex(in TrafficSignalPlan plan, float elapsedSeconds)
        {
            var phases = plan.Phases;
            if (phases.Count == 0)
            {
                return -1;
            }

            var cycle = plan.CycleSeconds;
            if (!float.IsFinite(cycle) || cycle <= 0f)
            {
                return -1;
            }

            var seconds = float.IsFinite(elapsedSeconds) ? elapsedSeconds : 0f;
            var position = seconds - (Mathf.Floor(seconds / cycle) * cycle);

            for (var i = 0; i < phases.Count; i++)
            {
                var duration = phases[i].DurationSeconds;
                if (!float.IsFinite(duration) || duration <= 0f)
                {
                    continue;
                }

                if (position < duration)
                {
                    return i;
                }

                position -= duration;
            }

            // Fin de cycle sur une queue de phases invalides : la derniere phase exploitable gouverne.
            for (var i = phases.Count - 1; i >= 0; i--)
            {
                if (float.IsFinite(phases[i].DurationSeconds) && phases[i].DurationSeconds > 0f)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Vrai si la phase courante autorise le groupe d'approche donne. Un plan absent ou sans phase
        /// exploitable laisse le comportement authore precedent : l'appelant ne construit alors pas de
        /// regle de feu, et cette fonction n'est pas consultee.
        /// </summary>
        public static bool PhaseAllowsGroup(in TrafficSignalPlan plan, float elapsedSeconds, int group)
        {
            var index = ResolvePhaseIndex(plan, elapsedSeconds);
            return index >= 0 && plan.Phases[index].AllowsGroup(group);
        }

        /// <summary>
        /// Vrai quand l'attente a une jonction a depasse le delai authore ET qu'au moins un autre
        /// revendiquant attend aussi : c'est la definition d'un interblocage (deux parties qui
        /// s'attendent), pas d'un vehicule simplement retenu.
        /// </summary>
        public static bool IsJunctionDeadlock(float ownWaitSeconds, bool anotherClaimantWaiting, float escalationDelaySeconds)
        {
            if (!anotherClaimantWaiting)
            {
                return false;
            }

            var delay = float.IsFinite(escalationDelaySeconds) && escalationDelaySeconds > 0f ? escalationDelaySeconds : 0f;
            return float.IsFinite(ownWaitSeconds) && ownWaitSeconds >= delay;
        }

        /// <summary>
        /// Place sur la voie de sortie. Faux quand un usager de la route occupe la zone de degagement,
        /// et faux AUSSI quand la place est indeterminee (approche sans voie de sortie sondee) : une
        /// place qu'on ne peut pas mesurer n'est jamais lue comme libre.
        /// </summary>
        public static bool HasExitRoom(bool exitProbeKnown, bool blockedByRoadUser)
        {
            return exitProbeKnown && !blockedByRoadUser;
        }

        /// <summary>
        /// Ecart accepte d'une approche Stop envers le revendiquant concurrent le plus proche. C'est
        /// une condition symetrique -- la distance se lit des deux cotes, donc les deux en tirent le
        /// meme verdict -- et un ecart non mesurable n'est jamais accepte.
        /// </summary>
        public static bool StopGapAccepted(float nearestClaimantDistance, float acceptedGap, bool hasClaimants)
        {
            if (!hasClaimants)
            {
                return true;
            }

            if (!float.IsFinite(nearestClaimantDistance))
            {
                return false;
            }

            var gap = float.IsFinite(acceptedGap) && acceptedGap > 0f ? acceptedGap : 0f;
            return nearestClaimantDistance >= gap;
        }

        /// <summary>Borne du parcours d'admission : le masque des admis tient dans un entier 64 bits.</summary>
        private const int MaxAdmissionClaims = 64;

        /// <summary>
        /// ENSEMBLE ADMIS d'un lot de revendications : rend vrai si la revendication d'index
        /// <paramref name="self"/> peut entrer dans la jonction.
        ///
        /// Remplace l'usage direct de <see cref="ResolveWinnerIndex"/> par le controleur, pour une
        /// raison de correction et non de confort. Le decompte de victoires est un TOURNOI : son
        /// resultat depend de l'ensemble ENTIER. Or chaque vehicule n'arbitrait que les
        /// revendications en conflit avec LUI, donc deux observateurs d'une meme jonction notaient
        /// deux tournois differents et pouvaient se croire vainqueurs tous les deux.
        ///
        /// Mesure (sonde Story519ArbitrationConsistencyProbe, trois revendications reelles A/B/C ou
        /// A et C ne se croisent pas) :
        ///
        ///   ensemble filtre sur soi : A -> PASSE, B -> PASSE, et A-B sont EN CONFLIT ;
        ///   ensemble complet        : B seul passe.
        ///
        /// La correction tient en deux points, tous deux necessaires : le controleur rassemble
        /// TOUTES les revendications de la jonction (pas seulement celles qui le concernent), et
        /// l'admission se calcule par un parcours glouton sur un ORDRE TOTAL, donc identique chez
        /// tous les observateurs du meme ensemble.
        ///
        /// Le glouton admet au passage les mouvements qui ne se croisent PAS : deux virages a droite
        /// opposes passent ensemble, la ou un vainqueur unique les serialisait sans raison.
        /// </summary>
        public static bool IsAdmitted(IReadOnlyList<JunctionClaim> claims, int count, int self,
            int[] winScratch, int[] orderScratch)
        {
            if (claims == null || winScratch == null || orderScratch == null) return true;
            if (self < 0 || claims.Count == 0 || count <= 0) return true;

            count = Mathf.Min(count, Mathf.Min(claims.Count, Mathf.Min(winScratch.Length, orderScratch.Length)));
            count = Mathf.Min(count, MaxAdmissionClaims);
            if (self >= count) return true;

            // 1. Decompte des duels, sur l'ensemble ENTIER.
            for (var i = 0; i < count; i++)
            {
                winScratch[i] = 0;
                orderScratch[i] = i;
            }

            for (var i = 0; i < count; i++)
            {
                for (var j = 0; j < count; j++)
                {
                    if (i != j && Conflicts(claims[i], claims[j])
                        && Resolve(claims[i], claims[j]) == JunctionVerdict.Proceed)
                    {
                        winScratch[i]++;
                    }
                }
            }

            // 2. Ordre total : victoires decroissantes, puis l'ordre total de departage deja utilise
            //    partout ailleurs. Tri par insertion, sans allocation : l'ensemble tient dans un
            //    tampon de jonction et la fonction est appelee dans la boucle physique.
            for (var i = 1; i < count; i++)
            {
                var value = orderScratch[i];
                var j = i - 1;
                while (j >= 0 && PrecedesInAdmission(claims, winScratch, value, orderScratch[j]))
                {
                    orderScratch[j + 1] = orderScratch[j];
                    j--;
                }

                orderScratch[j + 1] = value;
            }

            // 3. Glouton : une revendication entre si elle ne croise AUCUNE des deja admises.
            //    Deterministe, donc tous les pairs calculent le meme ensemble admis.
            var admitted = 0UL;
            for (var k = 0; k < count; k++)
            {
                var candidate = orderScratch[k];
                var blocked = false;
                for (var a = 0; a < k; a++)
                {
                    var other = orderScratch[a];
                    if ((admitted & (1UL << other)) == 0UL) continue;
                    if (!Conflicts(claims[candidate], claims[other])) continue;
                    blocked = true;
                    break;
                }

                if (blocked) continue;
                admitted |= 1UL << candidate;
                if (candidate == self) return true;
            }

            return false;
        }

        /// <summary>Ordre d'examen de l'admission : plus de victoires d'abord, puis l'ordre total.</summary>
        private static bool PrecedesInAdmission(IReadOnlyList<JunctionClaim> claims, int[] wins, int left, int right)
        {
            if (wins[left] != wins[right]) return wins[left] > wins[right];
            return CompareTotalOrder(claims[left], claims[right]) > 0;
        }

        /// <summary>
        /// Gagnant d'un ensemble de revendications, et seule porte d'entree de l'arbitrage a l'echelle
        /// d'une jonction. Rend l'index du gagnant, ou -1 quand aucune paire ne se dispute la meme
        /// jonction (personne n'a alors a ceder : un vehicule seul n'est jamais retenu par l'arbitrage).
        ///
        /// Une somme de duels ne suffit pas, et c'est le point delicat de la story : la priorite a droite
        /// forme un CYCLE des que quatre approches se presentent ensemble (chacune cede a sa droite), et un
        /// cycle ferait attendre tout le monde -- exactement l'etat que la story interdit. Le gagnant est
        /// donc celui qui l'emporte sur le plus d'adversaires, et un ordre total strict tranche les
        /// egalites. Il en sort TOUJOURS exactement un gagnant, quel que soit le nombre de revendicants.
        ///
        /// <paramref name="count"/> borne la lecture : les tampons du conducteur sont dimensionnes au
        /// pire cas et une revendication non ecrite ne doit pas peser dans l'arbitrage.
        /// </summary>
        public static int ResolveWinnerIndex(IReadOnlyList<JunctionClaim> claims, int count, int[] winCounts)
        {
            if (claims == null || winCounts == null || claims.Count == 0 || count <= 0)
            {
                return -1;
            }

            count = Mathf.Min(count, Mathf.Min(claims.Count, winCounts.Length));

            var contested = false;
            for (var i = 0; i < count; i++)
            {
                winCounts[i] = 0;
                for (var j = i + 1; j < count; j++)
                {
                    if (Conflicts(claims[i], claims[j]))
                    {
                        contested = true;
                    }
                }
            }

            if (!contested)
            {
                return -1;
            }

            for (var i = 0; i < count; i++)
            {
                for (var j = 0; j < count; j++)
                {
                    if (i != j && Conflicts(claims[i], claims[j]) && Resolve(claims[i], claims[j]) == JunctionVerdict.Proceed)
                    {
                        winCounts[i]++;
                    }
                }
            }

            var best = 0;
            for (var i = 1; i < count; i++)
            {
                if (winCounts[i] > winCounts[best])
                {
                    best = i;
                }
                else if (winCounts[i] == winCounts[best] && CompareTotalOrder(claims[i], claims[best]) > 0)
                {
                    best = i;
                }
            }

            return best;
        }

    }
}
