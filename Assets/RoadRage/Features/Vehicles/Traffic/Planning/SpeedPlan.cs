using System;
using System.Collections.Generic;

namespace RoadRage.Features.Vehicles.Traffic.Planning
{
    /// <summary>Contrainte nommee d'un point du plan de vitesse (5.31, A3).</summary>
    public enum SpeedConstraint
    {
        None = 0,
        DesiredSpeed = 1,
        SteeringCeiling = 2,
        /// <summary>Passe arriere : freinage anticipe vers une contrainte plus loin.</summary>
        AnticipatedDeceleration = 3,
        /// <summary>Passe avant : acceleration bornee depuis l'etat courant.</summary>
        MaxAcceleration = 4,
        /// <summary>L'etat courant ne peut pas ralentir plus vite que la deceleration de planification.</summary>
        CurrentSpeedDeceleration = 5,
        /// <summary>Horizon tronque (LookAheadLimit) : vitesse terminale nulle.</summary>
        HorizonTerminalStop = 6,
        /// <summary>v* inatteignable depuis l'etat courant a la deceleration de confort : freinage a SafeBrakingLimit.</summary>
        SteeringCeilingUnreachable = 7,
        /// <summary>Limite de courbe (2026-09-30, avancee de la 5.33) : v &lt;= racine(a_lat / |kappa|).</summary>
        CurveLimit = 8
    }

    /// <summary>Limite nommee et reportee. La limite de courbe est appliquee (SpeedConstraint.CurveLimit) depuis le 2026-09-30.</summary>
    public enum DeferredLimitKind { RoadLimit = 0 }

    /// <summary>Etat d'une limite nommee mais jamais appliquee (reportee a la 5.33).</summary>
    public enum DeferredLimitState { DeferredUnauthored = 0, DeferredAuthored = 1 }

    public enum SpeedPlanIssue { None = 0, PlanInfeasible = 1, InvalidInput = 2, ProfileRefused = 3 }

    /// <summary>Limite reportee : une valeur authoree n'est jamais presentee comme une absence de limite.</summary>
    public readonly struct DeferredLimit
    {
        public readonly RoadElementKind ElementKind;
        public readonly RoadId ElementId;
        public readonly DeferredLimitKind Kind;
        public readonly DeferredLimitState State;
        /// <summary>Valeur authoree (m/s) quand <see cref="State"/> vaut DeferredAuthored, sinon 0. Jamais appliquee.</summary>
        public readonly float AuthoredMetersPerSecond;

        public DeferredLimit(RoadElementKind elementKind, RoadId elementId, DeferredLimitKind kind, float authored)
        {
            ElementKind = elementKind; ElementId = elementId; Kind = kind;
            bool isAuthored = authored > 0f && !float.IsNaN(authored) && !float.IsInfinity(authored);
            State = isAuthored ? DeferredLimitState.DeferredAuthored : DeferredLimitState.DeferredUnauthored;
            AuthoredMetersPerSecond = isAuthored ? authored : 0f;
        }
    }

    /// <summary>
    /// Un noeud du plan : la vitesse retenue, la contrainte liante et les alternatives rejetees.
    /// Le plafond de braquage non borne est porte par <see cref="CeilingUnbounded"/>, jamais lu comme une vitesse.
    /// </summary>
    public readonly struct SpeedPlanPoint
    {
        public readonly float DistanceMeters;
        public readonly float SpeedMetersPerSecond;
        public readonly SpeedConstraint Binding;
        public readonly float DesiredMetersPerSecond;
        public readonly bool CeilingUnbounded;
        /// <summary>Plafond de braquage retenu sur le voisinage du noeud (min des deux cotes au raccord) ; 0 si non borne.</summary>
        public readonly float CeilingMetersPerSecond;
        public readonly float BackwardMetersPerSecond;
        public readonly float ForwardMetersPerSecond;
        public readonly float CurrentStateMetersPerSecond;
        /// <summary>Limite de courbe retenue sur le voisinage du noeud ; +inf en ligne droite.</summary>
        public readonly float CurveLimitMetersPerSecond;

        internal SpeedPlanPoint(float distance, float speed, SpeedConstraint binding, float desired, bool unbounded,
            float ceiling, float backward, float forward, float current, float curveLimit)
        {
            DistanceMeters = distance; SpeedMetersPerSecond = speed; Binding = binding;
            DesiredMetersPerSecond = desired; CeilingUnbounded = unbounded;
            CeilingMetersPerSecond = unbounded ? 0f : ceiling;
            BackwardMetersPerSecond = backward; ForwardMetersPerSecond = forward;
            CurrentStateMetersPerSecond = current; CurveLimitMetersPerSecond = curveLimit;
        }
    }

    /// <summary>
    /// Plan de vitesse a contraintes nommees (Story 5.31, A3). Contraintes appliquees : vitesse desiree,
    /// plafond de braquage v*(s), limite de courbe et bornes longitudinales ; la limite de route est nommee et
    /// reportee (5.33), jamais appliquee. Passe arriere a la deceleration de confort, passe avant a
    /// l'acceleration maximale, verification par le verificateur 5.30 avec les bornes du profil
    /// (MaxAcceleration, SafeBrakingLimit).
    ///
    /// Limite de courbe (decision proprietaire du 2026-09-30, avancee de la 5.33) : v &lt;= racine(a_lat / |kappa|),
    /// a_lat = min(ComfortableDeceleration, adherence laterale du vehicule). Le conducteur applique a
    /// l'acceleration laterale le meme rayon de confort qu'a son freinage planifie, sous l'adherence ; c'est une
    /// contrainte de conduite comme la vitesse desiree, que le verificateur ne juge pas.
    /// </summary>
    public sealed class SpeedPlan
    {
        /// <summary>
        /// Ecart minimal entre deux noeuds du profil. Sous ~1 mm, l'arrondi flottant de v^2 / (2 ds)
        /// depasserait la tolerance du verificateur (1e-3 m/s2) ; 0,2 m borne aussi le cout du verificateur
        /// (chaque point de l'horizon y interpole le profil). Les points de l'horizon entre deux noeuds
        /// restent juges par le verificateur, et leur plafond est replie sur les noeuds voisins.
        /// </summary>
        public const float MinimumKnotSpacingMeters = 0.2f;

        /// <summary>Marge relative prise sur les bornes de planification, sous les bornes verifiees.</summary>
        public const float PlanningBoundMargin = 0.99f;

        public IReadOnlyList<SpeedPlanPoint> Points { get; }
        public IReadOnlyList<DeferredLimit> DeferredLimits { get; }
        public SpeedConstraint Binding { get; }
        public float PlanningDecelerationMetersPerSecondSquared { get; }
        public SpeedPlanIssue Issue { get; }
        public SpeedProfileResult Verification { get; }
        public MotionIssue PlanIssue { get; }
        public bool Accepted { get { return Issue == SpeedPlanIssue.None; } }
        /// <summary>
        /// Contrainte qui borne le plan au vehicule : celle du plafond d'ou part la passe arriere au premier noeud
        /// (SteeringCeiling, CurveLimit, DesiredSpeed, HorizonTerminalStop), la ou <see cref="Binding"/> ne nomme
        /// que la regle du premier noeud (le plus souvent MaxAcceleration).
        /// </summary>
        public SpeedConstraint LimitingConstraint { get; }
        /// <summary>a_lat de la limite de courbe (m/s2).</summary>
        public float LateralAccelerationMetersPerSecondSquared { get; }

        private SpeedPlan(List<SpeedPlanPoint> points, List<DeferredLimit> deferred, SpeedConstraint binding,
            float deceleration, SpeedPlanIssue issue, SpeedProfileResult verification, MotionIssue planIssue,
            SpeedConstraint limiting = SpeedConstraint.None, float lateral = 0f)
        {
            Points = points.AsReadOnly(); DeferredLimits = deferred.AsReadOnly(); Binding = binding;
            PlanningDecelerationMetersPerSecondSquared = deceleration; Issue = issue;
            Verification = verification; PlanIssue = planIssue;
            LimitingConstraint = limiting; LateralAccelerationMetersPerSecondSquared = lateral;
        }

        /// <summary>Vitesse planifiee a une distance d'horizon (v^2 lineaire entre deux noeuds).</summary>
        public float SpeedAt(float distanceMeters)
        {
            if (Points.Count == 0) return 0f;
            if (distanceMeters <= Points[0].DistanceMeters) return Points[0].SpeedMetersPerSecond;
            for (int i = 1; i < Points.Count; i++)
                if (distanceMeters <= Points[i].DistanceMeters)
                {
                    var a = Points[i - 1]; var b = Points[i];
                    double t = (distanceMeters - a.DistanceMeters) / (b.DistanceMeters - a.DistanceMeters);
                    double v2 = (double)a.SpeedMetersPerSecond * a.SpeedMetersPerSecond * (1d - t)
                        + (double)b.SpeedMetersPerSecond * b.SpeedMetersPerSecond * t;
                    return (float)Math.Sqrt(Math.Max(0d, v2));
                }
            return Points[Points.Count - 1].SpeedMetersPerSecond;
        }

        public IReadOnlyList<SpeedProfilePoint> ToProfile()
        {
            var profile = new SpeedProfilePoint[Points.Count];
            for (int i = 0; i < profile.Length; i++)
                profile[i] = new SpeedProfilePoint(Points[i].DistanceMeters, Points[i].SpeedMetersPerSecond);
            return profile;
        }

        /// <param name="lateralGripMetersPerSecondSquared">
        /// Adherence laterale du vehicule (coefficient x g), borne physique de la limite de courbe ; +inf : confort seul.
        /// </param>
        public static SpeedPlan Build(MotionPlan motion, CompiledRoadModel model, DriverProfile driver,
            float currentSpeedMetersPerSecond, float lateralGripMetersPerSecondSquared = float.PositiveInfinity)
        {
            if (motion == null) throw new ArgumentNullException("motion");
            if (model == null) throw new ArgumentNullException("model");
            var deferred = DeferredLimitsOf(motion.Path, model);
            var empty = new List<SpeedPlanPoint>();
            var bounds = new LongitudinalBounds(driver.MaxAcceleration, driver.SafeBrakingLimit);
            if (motion.Issue != MotionIssue.None)
                return new SpeedPlan(empty, deferred, SpeedConstraint.None, 0f, SpeedPlanIssue.PlanInfeasible,
                    new SpeedProfileResult(SpeedProfileIssue.PlanInfeasible, motion.IssueDistanceMeters,
                        motion.Diagnostics, motion.Issue), motion.Issue);
            float desired = driver.DesiredSpeed;
            float comfortable = driver.ComfortableDeceleration;
            if (!bounds.Valid || !IsFinite(desired) || desired < 0f || !(comfortable > 0f) || !IsFinite(comfortable)
                || !IsFinite(currentSpeedMetersPerSecond) || !(lateralGripMetersPerSecondSquared > 0f))
                return new SpeedPlan(empty, deferred, SpeedConstraint.None, 0f, SpeedPlanIssue.InvalidInput,
                    new SpeedProfileResult(SpeedProfileIssue.InvalidSpeedProfile, 0f, motion.Diagnostics), MotionIssue.None);
            double lateral = Math.Min(comfortable, lateralGripMetersPerSecondSquared);

            // Points de l'horizon a plafond regroupe : un raccord porte le min des deux cotes (et la plus forte courbure).
            var distances = new List<float>();
            var ceilings = new List<float>();
            var unbounded = new List<bool>();
            var curvatures = new List<float>();
            foreach (var interval in motion.Path.Intervals)
                foreach (var point in interval.Points)
                {
                    int last = distances.Count - 1;
                    float curvature = Math.Abs(point.Reference.CurvaturePerMeter);
                    if (last >= 0 && point.DistanceMeters <= distances[last])
                    {
                        if (!point.Unbounded && (unbounded[last] || point.SteeringCeilingMetersPerSecond < ceilings[last]))
                        { ceilings[last] = point.SteeringCeilingMetersPerSecond; unbounded[last] = false; }
                        curvatures[last] = Math.Max(curvatures[last], curvature);
                        continue;
                    }
                    distances.Add(point.DistanceMeters);
                    ceilings.Add(point.Unbounded ? 0f : point.SteeringCeilingMetersPerSecond);
                    unbounded.Add(point.Unbounded);
                    curvatures.Add(curvature);
                }
            if (distances.Count == 0)
                return new SpeedPlan(empty, deferred, SpeedConstraint.None, 0f, SpeedPlanIssue.InvalidInput,
                    new SpeedProfileResult(SpeedProfileIssue.InvalidSpeedProfile, 0f, motion.Diagnostics), MotionIssue.None);

            // Noeuds du profil : au plus un tous les MinimumKnotSpacingMeters, premier et dernier conserves.
            var knots = new List<int> { 0 };
            for (int i = 1; i < distances.Count; i++)
            {
                bool lastPoint = i == distances.Count - 1;
                if (distances[i] - distances[knots[knots.Count - 1]] >= MinimumKnotSpacingMeters) knots.Add(i);
                else if (lastPoint)
                {
                    if (knots.Count == 1) knots.Add(i);
                    else knots[knots.Count - 1] = i;
                }
            }
            if (knots.Count == 1) knots.Add(0);

            int n = knots.Count;
            var d = new double[n];
            var ceiling = new double[n];
            var ceilingUnbounded = new bool[n];
            var curve = new double[n];
            for (int k = 0; k < n; k++)
            {
                d[k] = distances[knots[k]];
                // Le verificateur juge chaque sous-intervalle par max(v) aux bornes contre min(v*) aux bornes,
                // et son v* en un point est le min des deux points d'horizon qui l'encadrent : un noeud porte
                // donc le plus petit plafond de tous les points entre ses voisins, elargi d'un point de chaque cote.
                // La limite de courbe prend le meme voisinage (courbure maximale).
                int from = Math.Max(0, (k == 0 ? knots[0] : knots[k - 1]) - 2);
                int to = Math.Min(distances.Count - 1, (k == n - 1 ? knots[n - 1] : knots[k + 1]) + 1);
                ceilingUnbounded[k] = true;
                ceiling[k] = 0d;
                double curvature = 0d;
                for (int i = Math.Min(from, to); i <= Math.Max(from, to); i++)
                {
                    if (!unbounded[i] && (ceilingUnbounded[k] || ceilings[i] < ceiling[k]))
                    { ceiling[k] = ceilings[i]; ceilingUnbounded[k] = false; }
                    curvature = Math.Max(curvature, curvatures[i]);
                }
                curve[k] = curvature > 0d ? Math.Sqrt(lateral / curvature) : double.PositiveInfinity;
            }
            bool terminalStop = motion.Path.End == HorizonEnd.LookAheadLimit;
            double v0 = Math.Max(0d, currentSpeedMetersPerSecond);
            double acceleration = driver.MaxAcceleration * (double)PlanningBoundMargin;

            // Faisabilite du plafond seul depuis l'etat courant, a la deceleration de confort.
            double planningDeceleration = comfortable * (double)PlanningBoundMargin;
            bool unreachable = !CeilingReachable(d, ceiling, ceilingUnbounded, terminalStop, v0, planningDeceleration);
            if (unreachable) planningDeceleration = driver.SafeBrakingLimit * (double)PlanningBoundMargin;

            var cap = new double[n];
            var capBinding = new SpeedConstraint[n];
            for (int k = 0; k < n; k++)
            {
                cap[k] = desired; capBinding[k] = SpeedConstraint.DesiredSpeed;
                if (!ceilingUnbounded[k] && ceiling[k] < cap[k])
                { cap[k] = ceiling[k]; capBinding[k] = SpeedConstraint.SteeringCeiling; }
                if (curve[k] < cap[k]) { cap[k] = curve[k]; capBinding[k] = SpeedConstraint.CurveLimit; }
            }
            if (terminalStop) { cap[n - 1] = 0d; capBinding[n - 1] = SpeedConstraint.HorizonTerminalStop; }

            var backward = new double[n];
            var backwardBinding = new SpeedConstraint[n];
            // Noeud dont le plafond fixe la passe arriere en k : la contrainte limitante du plan au vehicule.
            var origin = new int[n];
            backward[n - 1] = cap[n - 1]; backwardBinding[n - 1] = capBinding[n - 1]; origin[n - 1] = n - 1;
            for (int k = n - 2; k >= 0; k--)
            {
                double reach = Math.Sqrt(backward[k + 1] * backward[k + 1] + 2d * planningDeceleration * (d[k + 1] - d[k]));
                if (reach < cap[k])
                {
                    backward[k] = reach; backwardBinding[k] = SpeedConstraint.AnticipatedDeceleration;
                    origin[k] = origin[k + 1];
                }
                else { backward[k] = cap[k]; backwardBinding[k] = capBinding[k]; origin[k] = k; }
            }
            var forward = new double[n];
            var lower = new double[n];
            var speed = new double[n];
            var binding = new SpeedConstraint[n];
            forward[0] = Math.Min(backward[0], v0);
            lower[0] = v0;
            for (int k = 1; k < n; k++)
            {
                double step = d[k] - d[k - 1];
                forward[k] = Math.Min(backward[k], Math.Sqrt(forward[k - 1] * forward[k - 1] + 2d * acceleration * step));
                lower[k] = Math.Sqrt(Math.Max(0d, lower[k - 1] * lower[k - 1] - 2d * planningDeceleration * step));
            }
            for (int k = 0; k < n; k++)
            {
                if (lower[k] > forward[k])
                {
                    speed[k] = lower[k];
                    binding[k] = unreachable ? SpeedConstraint.SteeringCeilingUnreachable
                        : SpeedConstraint.CurrentSpeedDeceleration;
                }
                else
                {
                    speed[k] = forward[k];
                    binding[k] = forward[k] < backward[k] ? SpeedConstraint.MaxAcceleration : backwardBinding[k];
                }
            }

            // Horizon plus court qu'un pas de noeud (arrivee au portail) : aucune acceleration sur quelques
            // millimetres, sinon l'arrondi de v^2 / (2 ds) serait juge comme un depassement de borne.
            if (n == 2 && d[1] - d[0] < MinimumKnotSpacingMeters && speed[1] > speed[0]) speed[1] = speed[0];

            var points = new List<SpeedPlanPoint>(n);
            for (int k = 0; k < n; k++)
                points.Add(new SpeedPlanPoint((float)d[k], (float)speed[k], binding[k], desired, ceilingUnbounded[k],
                    (float)ceiling[k], (float)backward[k], (float)forward[k], (float)lower[k], (float)curve[k]));
            // Un seul noeud (horizon nul) : le profil doit tout de meme couvrir [0, L].
            if (points.Count == 2 && points[0].DistanceMeters == points[1].DistanceMeters)
                points.RemoveAt(1);

            var profile = new SpeedProfilePoint[points.Count];
            for (int i = 0; i < profile.Length; i++)
                profile[i] = new SpeedProfilePoint(points[i].DistanceMeters, points[i].SpeedMetersPerSecond);
            var verification = motion.VerifySpeedProfile(profile, bounds);
            var planBinding = unreachable ? SpeedConstraint.SteeringCeilingUnreachable : points[0].Binding;
            return new SpeedPlan(points, deferred, planBinding, (float)planningDeceleration,
                verification.Issue == SpeedProfileIssue.None ? SpeedPlanIssue.None : SpeedPlanIssue.ProfileRefused,
                verification, verification.PlanIssue, capBinding[origin[0]], (float)lateral);
        }

        /// <summary>
        /// Vrai si l'etat courant peut respecter le plafond de braquage (et l'arret terminal) en
        /// freinant a <paramref name="deceleration"/>. La vitesse desiree n'entre pas ici : la depasser
        /// n'est pas une infaisabilite.
        /// </summary>
        private static bool CeilingReachable(double[] d, double[] ceiling, bool[] unbounded, bool terminalStop,
            double v0, double deceleration)
        {
            int n = d.Length;
            double next = double.PositiveInfinity;
            var chain = new double[n];
            for (int k = n - 1; k >= 0; k--)
            {
                double cap = unbounded[k] ? double.PositiveInfinity : ceiling[k];
                if (terminalStop && k == n - 1) cap = 0d;
                double reach = k == n - 1 ? double.PositiveInfinity
                    : Math.Sqrt(next * next + 2d * deceleration * (d[k + 1] - d[k]));
                chain[k] = Math.Min(cap, reach);
                next = chain[k];
            }
            double lower = v0;
            for (int k = 0; k < n; k++)
            {
                if (k > 0) lower = Math.Sqrt(Math.Max(0d, lower * lower - 2d * deceleration * (d[k] - d[k - 1])));
                if (lower > chain[k]) return false;
            }
            return true;
        }

        private static List<DeferredLimit> DeferredLimitsOf(PathHorizon path, CompiledRoadModel model)
        {
            var result = new List<DeferredLimit>();
            foreach (var interval in path.Intervals)
            {
                float authored = 0f;
                EffectiveLaneCorridor corridor;
                if (interval.Kind == RoadElementKind.LaneCorridor && model.TryGetCorridor(interval.Id, out corridor))
                    authored = corridor.SpeedLimitMetersPerSecond;
                // La limite de courbe est appliquee depuis le 2026-09-30 : seule la limite de route reste reportee.
                result.Add(new DeferredLimit(interval.Kind, interval.Id, DeferredLimitKind.RoadLimit, authored));
            }
            return result;
        }

        private static bool IsFinite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
