using System;
using System.Globalization;
using RoadRage.Features.Vehicles.Traffic.Planning;

namespace RoadRage.Features.Vehicles.Traffic.Coordination
{
    /// <summary>
    /// Distances de coordination d'un vehicule (Story 5.34), fonctions pures de sa vitesse, de son profil et du pas
    /// physique ; aucune constante calibree a l'oeil hors <c>m_ctrl</c>, declaree par l'appelant.
    /// <list type="bullet">
    /// <item>latence de decision t_lat = Δt_lot + Δt = 2 Δt : un lot par pas physique ; la frame N decrit l'etat apres la
    /// physique N−1, et l'intent issu d'un refus lu a N+1 agit pendant la physique N+1 ;</item>
    /// <item>vitesse de reference v_ref = v + a t_lat ; deceleration de planification b_plan = 0,99 b (D14) ;</item>
    /// <item>arret garanti D_stop = v_ref t_lat + v_ref² / (2 b_plan) + m_ctrl ;</item>
    /// <item>engagement de la contrainte D_engage = max(D_stop, s*(v_ref)) + v_ref t_lat, s*(v) = s0 + v T + v² / (2 √(a b)) ;</item>
    /// <item>demande D_request = D_engage(v_ref) + v_ref t_lat (decision O10 : D_engage a la vitesse atteignable pendant la
    /// latence, pour que la fenetre de demande ne soit jamais sautee sous acceleration).</item>
    /// </list>
    /// Decision O9 : un vehicule dans la fenetre de maintien a l'arret D11 reste demandeur et contraint ; les seuils effectifs sont
    /// max(D_engage, fenetre) et max(D_request, fenetre). Les formules, D_stop et l'engagement sont inchanges. Decision O14 : la
    /// fenetre d'une entree est D_stop a la vitesse d'entree du maintien D11 (derivee, aucune constante) : le maintien
    /// s'enclenche des que, a cette vitesse, l'arret confortable tombe avant m_ctrl.
    /// </summary>
    public readonly struct JunctionDistances
    {
        public readonly float SpeedMetersPerSecond;
        public readonly float LatencySeconds;
        public readonly float ReferenceSpeedMetersPerSecond;
        public readonly float PlanningDecelerationMetersPerSecondSquared;
        public readonly float ControlMarginMeters;
        /// <summary>D_stop : en deca, le vehicule ne peut plus s'arreter avant l'entree ; il est engage s'il tient un grant.</summary>
        public readonly float StopMeters;
        /// <summary>D_engage (formule) : distance a partir de laquelle la contrainte JunctionEntry s'applique sans grant.</summary>
        public readonly float EngageMeters;
        /// <summary>D_request (formule O10) : distance a partir de laquelle une demande est valide.</summary>
        public readonly float RequestMeters;
        /// <summary>Fenetre de maintien a l'arret devant l'entree : D_stop a la vitesse d'entree du maintien (decisions O9, O14).</summary>
        public readonly float HoldWindowMeters;

        private JunctionDistances(float speed, float latency, float reference, float deceleration, float margin, float stop,
            float engage, float request, float holdWindow)
        {
            SpeedMetersPerSecond = speed; LatencySeconds = latency; ReferenceSpeedMetersPerSecond = reference;
            PlanningDecelerationMetersPerSecondSquared = deceleration; ControlMarginMeters = margin;
            StopMeters = stop; EngageMeters = engage; RequestMeters = request; HoldWindowMeters = holdWindow;
        }

        /// <summary>Seuil effectif de la contrainte JunctionEntry : max(D_engage, fenetre de maintien) (O9, O14).</summary>
        public float EngageThresholdMeters { get { return Math.Max(EngageMeters, HoldWindowMeters); } }

        /// <summary>Seuil effectif d'une demande valide : max(D_request, fenetre de maintien) (O9, O14).</summary>
        public float RequestThresholdMeters { get { return Math.Max(RequestMeters, HoldWindowMeters); } }

        /// <summary>t_lat = 2 Δt : le lot N publie a N+1, plus un pas d'actuation.</summary>
        public static float DecisionLatencySeconds(float deltaTimeSeconds)
        {
            if (!(deltaTimeSeconds > 0f) || float.IsInfinity(deltaTimeSeconds)) throw new ArgumentException("InvalidDeltaTime", "deltaTimeSeconds");
            return 2f * deltaTimeSeconds;
        }

        /// <summary>
        /// Ecart desire de l'IDM devant un obstacle fixe, s*(v) = s0 + v T + v² / (2 √(a b)), avec les memes planchers que
        /// <see cref="DriverModel.ComputeAcceleration"/>.
        /// </summary>
        public static float DesiredGapMeters(DriverProfile driver, float speedMetersPerSecond)
        {
            float v = Finite(speedMetersPerSecond) ? Math.Max(0f, speedMetersPerSecond) : 0f;
            float a = Math.Max(Finite(driver.MaxAcceleration) ? driver.MaxAcceleration : 0f, 0.05f);
            float b = Math.Max(Finite(driver.ComfortableDeceleration) ? driver.ComfortableDeceleration : 0f, 0.05f);
            float headway = Finite(driver.TimeHeadway) ? Math.Max(0f, driver.TimeHeadway) : 0f;
            float s0 = Finite(driver.MinimumGap) ? driver.MinimumGap : 0f;
            return s0 + Math.Max(0f, v * headway + v * v / (2f * (float)Math.Sqrt(a * b)));
        }

        /// <param name="controlMarginMeters">m_ctrl (TrafficV2Settings.JunctionStopControlMarginMeters).</param>
        /// <param name="holdEntrySpeedMetersPerSecond">Vitesse d'entree du maintien D11 (TrafficV2Settings.StopHold) ; 0 : fenetre
        /// D_stop(0), sans effet (D_engage la couvre).</param>
        public static JunctionDistances For(DriverProfile driver, float speedMetersPerSecond, float deltaTimeSeconds,
            float controlMarginMeters, float holdEntrySpeedMetersPerSecond = 0f)
        {
            if (!(controlMarginMeters >= 0f) || float.IsInfinity(controlMarginMeters))
                throw new ArgumentException("InvalidControlMargin", "controlMarginMeters");
            if (!(holdEntrySpeedMetersPerSecond >= 0f) || float.IsInfinity(holdEntrySpeedMetersPerSecond))
                throw new ArgumentException("InvalidHoldEntrySpeed", "holdEntrySpeedMetersPerSecond");
            float latency = DecisionLatencySeconds(deltaTimeSeconds);
            float v = Finite(speedMetersPerSecond) ? Math.Max(0f, speedMetersPerSecond) : 0f;
            float acceleration = Finite(driver.MaxAcceleration) ? Math.Max(0f, driver.MaxAcceleration) : 0f;
            float reference = v + acceleration * latency;
            float deceleration = (Finite(driver.ComfortableDeceleration) ? driver.ComfortableDeceleration : 0f) * SpeedPlan.PlanningBoundMargin;
            if (!(deceleration > 0f)) throw new ArgumentException("InvalidComfortableDeceleration", "driver");
            float travel = reference * latency;
            float stop = Stop(reference, latency, deceleration, controlMarginMeters);
            float engage = Engage(driver, v, latency, acceleration, deceleration, controlMarginMeters);
            // D_request = D_engage(v_ref) + v_ref t_lat : seuil d'engagement a la vitesse atteignable pendant la latence, qui couvre
            // la croissance de D_engage d'un pas a l'acceleration maximale (sinon la fenetre de demande peut etre sautee).
            float request = Engage(driver, reference, latency, acceleration, deceleration, controlMarginMeters) + travel;
            float window = Stop(holdEntrySpeedMetersPerSecond + acceleration * latency, latency, deceleration, controlMarginMeters);
            return new JunctionDistances(v, latency, reference, deceleration, controlMarginMeters, stop, engage, request, window);
        }

        /// <summary>D_stop a la vitesse de reference u_ref : u_ref t_lat + u_ref² / (2 b_plan) + m_ctrl.</summary>
        private static float Stop(float reference, float latency, float deceleration, float margin)
        {
            return reference * latency + reference * reference / (2f * deceleration) + margin;
        }

        /// <summary>D_engage(u) = max(D_stop(u), s*(u_ref)) + u_ref t_lat, u_ref = u + a t_lat.</summary>
        private static float Engage(DriverProfile driver, float speed, float latency, float acceleration, float deceleration, float margin)
        {
            float reference = speed + acceleration * latency;
            return Math.Max(Stop(reference, latency, deceleration, margin), DesiredGapMeters(driver, reference)) + reference * latency;
        }

        /// <summary>
        /// a_kin = −v² / (2 max(d − m_ctrl, ε)) : deceleration qui arrete le vehicule a m_ctrl de l'entree ; 0 a l'arret. Rend
        /// la garantie d'arret structurelle quand un refus survient tard ; toujours finie. Definition unique dans l'arbitrage.
        /// </summary>
        public static float KinematicStopAcceleration(float speedMetersPerSecond, float distanceMeters, float controlMarginMeters)
        {
            return LongitudinalArbitration.KinematicStopAcceleration(speedMetersPerSecond, distanceMeters, controlMarginMeters);
        }

        public string ToText()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "v {0:0.###} t_lat {1:0.###} v_ref {2:0.###} b_plan {3:0.###} m_ctrl {4:0.###} D_stop {5:0.###} D_engage {6:0.###}"
                + " D_request {7:0.###} fenetre {8:0.###}",
                SpeedMetersPerSecond, LatencySeconds, ReferenceSpeedMetersPerSecond, PlanningDecelerationMetersPerSecondSquared,
                ControlMarginMeters, StopMeters, EngageMeters, RequestMeters, HoldWindowMeters);
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
