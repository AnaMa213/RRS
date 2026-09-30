#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RoadRage.Features.Vehicles.Traffic.Planning;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    /// <summary>Solution de l'ecart e (radians) aux noeuds d'integration RK4 d'un element, abscisses croissantes.</summary>
    public sealed class OffsetTrace
    {
        public readonly List<double> S = new List<double>();
        public readonly List<double> E = new List<double>();

        public double StartS { get { return S[0]; } }
        public double EndS { get { return S[S.Count - 1]; } }

        /// <summary>Meme integration que le runtime (<see cref="RoadCurve.AdvanceKinematicOffset(float, float, double, float, Action{double, double})"/>).</summary>
        public static OffsetTrace Integrate(SweepElement element, float s0, double e0, float a)
        {
            var trace = new OffsetTrace();
            trace.S.Add(s0);
            trace.E.Add(e0);
            element.Curve.AdvanceKinematicOffset(s0, element.EndS, e0, a, delegate(double s, double e)
            {
                trace.S.Add(s);
                trace.E.Add(e);
            });
            return trace;
        }

        /// <summary>Valeur interpolee lineairement entre noeuds, bornee aux extremites.</summary>
        public double At(double s)
        {
            if (s <= S[0]) return E[0];
            int last = S.Count - 1;
            if (s >= S[last]) return E[last];
            int low = 0;
            int high = last;
            while (high - low > 1)
            {
                int mid = (low + high) / 2;
                if (S[mid] <= s) low = mid; else high = mid;
            }

            double span = S[high] - S[low];
            double t = span > 0d ? (s - S[low]) / span : 0d;
            return E[low] + t * (E[high] - E[low]);
        }

        /// <summary>Etend [lo, hi] aux valeurs sur [s0, s1] (noeuds internes et extremites) ; faux si la solution n'existe pas encore.</summary>
        public bool Range(double s0, double s1, ref double lo, ref double hi)
        {
            if (s1 < StartS) return false;
            s0 = Math.Max(s0, StartS);
            s1 = Math.Max(s0, Math.Min(s1, EndS));
            Include(At(s0), ref lo, ref hi);
            Include(At(s1), ref lo, ref hi);
            for (int k = 0; k < S.Count; k++)
            {
                if (S[k] > s0 && S[k] < s1) Include(E[k], ref lo, ref hi);
            }

            return true;
        }

        private static void Include(double value, ref double lo, ref double hi)
        {
            lo = Math.Min(lo, value);
            hi = Math.Max(hi, value);
        }
    }

    /// <summary>Intervalle d'entree I_X, solutions de ses bornes et solutions amorcees aux portails d'un element.</summary>
    public sealed class ElementOffsets
    {
        public RoadId Id;
        public bool IsMovement;
        public SweepElement Element;
        public bool HasEntry;
        public double EntryLo;
        public double EntryHi;
        public OffsetTrace Lower;
        public OffsetTrace Upper;
        public readonly List<OffsetTrace> Seeds = new List<OffsetTrace>();

        /// <summary>Reste entre noeuds B = h^2/8 (max|kappa'| + (max|kappa| + 1/a)/a) : ecart de la solution a son interpolee.</summary>
        public double NodeRemainderRadians;

        public bool Reachable { get { return HasEntry || Seeds.Count > 0; } }
    }

    /// <summary>
    /// Bornes d'ecart independantes de la route (contrat Road World Model §8, Story 5.52). Chaque element X
    /// porte un intervalle d'entree I_X calcule sur le graphe de transitions (connexions et mouvements, cycles
    /// compris), amorce a 0 au s effectif de chaque portail d'entree, transporte par l'equation
    /// de/ds = kappa - sin(e)/a et saute du saut de tangente signe a chaque raccord. Les candidats viennent d'une
    /// iteration de Kleene elargie de eta ; seul le controle d'inductivite les accepte.
    /// </summary>
    public sealed class KinematicOffsetBounds
    {
        public const string NotClosedCode = "HeadingOffsetBoundNotClosed";
        public const string InfeasibleCode = "NominalPoseInfeasible";
        private const double HalfPi = 0.5d * Math.PI;
        private const float SeedToleranceMeters = 0.001f;

        public readonly Dictionary<RoadId, ElementOffsets> Elements = new Dictionary<RoadId, ElementOffsets>();

        /// <summary>Echecs de fermeture : aucune preuve ne peut etre calculee.</summary>
        public readonly List<string> Failures = new List<string>();

        /// <summary>Poses hors braquage ou hors taux : jamais couvertes, HALT proprietaire.</summary>
        public readonly List<string> Infeasible = new List<string>();

        public readonly List<RoadId> Unreachable = new List<RoadId>();
        public int Iterations;
        public bool Converged;
        public float ReferenceAheadRearAxleMeters;
        public double ToleranceRadians;
        public int FeasibilityChecks;
        public double MinimumSteerRateMarginDegreesPerSecond = double.PositiveInfinity;
        public double MinimumLockSpeedMarginMetersPerSecond = double.PositiveInfinity;

        public bool Closed { get { return Failures.Count == 0; } }

        public static KinematicOffsetBounds Compute(CompiledRoadModel model, SweepGraph graph, GateAEvidenceParameters parameters)
        {
            if (model == null) throw new ArgumentNullException("model");
            var seeds = new List<KeyValuePair<RoadId, float>>();
            foreach (var portal in model.Portals)
            {
                if (portal.Role == PortalRole.Entry) seeds.Add(new KeyValuePair<RoadId, float>(portal.CorridorId, portal.SMeters));
            }

            var bounds = Compute(graph, seeds, model.DrivabilityProfile, parameters);
            if (bounds.Closed) bounds.CheckFeasibility(model.DrivabilityProfile, parameters);
            return bounds;
        }

        public static KinematicOffsetBounds Compute(SweepGraph graph, IList<KeyValuePair<RoadId, float>> portalSeeds,
            DrivabilityProfile drivability, GateAEvidenceParameters parameters)
        {
            if (graph == null) throw new ArgumentNullException("graph");
            if (parameters == null || !parameters.Kinematic) throw new ArgumentException("Parametres de pose cinematique attendus.", "parameters");
            var bounds = new KinematicOffsetBounds();
            float a = drivability.ReferencePointAheadRearAxleMeters;
            bounds.ReferenceAheadRearAxleMeters = a;
            bounds.ToleranceRadians = parameters.OffsetToleranceRadians;
            if (!drivability.Declared || !(a > 0f) || float.IsInfinity(a))
            {
                bounds.Failures.Add(NotClosedCode + " : profil de conduisibilite non declare (a = " + R(a) + " m).");
                return bounds;
            }

            var order = new List<RoadId>(graph.Elements.Keys);
            order.Sort();
            foreach (var id in order)
            {
                var element = graph.Elements[id];
                bounds.Elements.Add(id, new ElementOffsets
                {
                    Id = id,
                    IsMovement = element.IsMovement,
                    Element = element,
                    NodeRemainderRadians = NodeRemainder(element, a)
                });
            }

            var seedExits = new Dictionary<RoadId, List<double>>();
            foreach (var seed in portalSeeds)
            {
                ElementOffsets offsets;
                if (!bounds.Elements.TryGetValue(seed.Key, out offsets))
                {
                    bounds.Failures.Add(NotClosedCode + " : portail d'entree sur un element absent du graphe (" + seed.Key + ").");
                    continue;
                }

                var element = offsets.Element;
                if (!(seed.Value >= element.StartS - SeedToleranceMeters) || !(seed.Value <= element.EndS + SeedToleranceMeters))
                {
                    bounds.Failures.Add(NotClosedCode + " : portail d'entree hors de son element " + seed.Key + " (s = " + R(seed.Value) + " m).");
                    continue;
                }

                var trace = OffsetTrace.Integrate(element, Mathf.Clamp(seed.Value, element.StartS, element.EndS), 0d, a);
                offsets.Seeds.Add(trace);
                List<double> exits;
                if (!seedExits.TryGetValue(seed.Key, out exits)) seedExits.Add(seed.Key, exits = new List<double>());
                exits.Add(trace.E[trace.E.Count - 1]);
            }

            if (bounds.Failures.Count > 0) return bounds;

            var jumps = new Dictionary<RoadId, Dictionary<RoadId, double>>();
            foreach (var id in order)
            {
                var element = graph.Elements[id];
                var row = new Dictionary<RoadId, double>();
                foreach (var next in element.Next)
                {
                    row[next.Id] = RoadCurve.SignedTangentJumpRadians(element.Curve.Sample(element.EndS), next.Curve.Sample(next.StartS));
                }

                jumps.Add(id, row);
            }

            // Iteration de Kleene croissante depuis l'ensemble vide : chaque passe elargit I_X a l'enveloppe des images.
            var lo = new Dictionary<RoadId, double>();
            var hi = new Dictionary<RoadId, double>();
            bool changed = true;
            for (int iteration = 1; iteration <= parameters.ClosureIterationBudget && changed; iteration++)
            {
                bounds.Iterations = iteration;
                changed = false;
                var images = Images(bounds, order, lo, hi, seedExits, jumps, a);
                foreach (var id in order)
                {
                    KeyValuePair<double, double> image;
                    if (!images.TryGetValue(id, out image)) continue;
                    double oldLo;
                    double oldHi;
                    bool had = lo.TryGetValue(id, out oldLo) & hi.TryGetValue(id, out oldHi);
                    double newLo = had ? Math.Min(oldLo, image.Key) : image.Key;
                    double newHi = had ? Math.Max(oldHi, image.Value) : image.Value;
                    if (!had || newLo < oldLo || newHi > oldHi)
                    {
                        changed = true;
                        lo[id] = newLo;
                        hi[id] = newHi;
                    }

                    if (!(Math.Abs(newLo) < HalfPi) || !(Math.Abs(newHi) < HalfPi))
                    {
                        bounds.Failures.Add(NotClosedCode + " : |e| >= 90 deg a l'entree de " + Describe(bounds.Elements[id]) + " apres "
                            + iteration + " iteration(s) (I = [" + Deg(newLo) + ", " + Deg(newHi) + "] deg).");
                        return bounds;
                    }
                }
            }

            bounds.Converged = !changed;

            // Candidat elargi de eta ; seul le controle d'inductivite l'accepte.
            double eta = parameters.OffsetToleranceRadians;
            var wideLo = new Dictionary<RoadId, double>();
            var wideHi = new Dictionary<RoadId, double>();
            foreach (var id in order)
            {
                double l;
                double h;
                if (!lo.TryGetValue(id, out l) || !hi.TryGetValue(id, out h)) continue;
                wideLo[id] = l - eta;
                wideHi[id] = h + eta;
                if (!(Math.Abs(l - eta) < HalfPi) || !(Math.Abs(h + eta) < HalfPi))
                {
                    bounds.Failures.Add(NotClosedCode + " : |e| >= 90 deg apres elargissement a l'entree de " + Describe(bounds.Elements[id]) + ".");
                    return bounds;
                }
            }

            var check = Images(bounds, order, wideLo, wideHi, seedExits, jumps, a);
            foreach (var id in order)
            {
                KeyValuePair<double, double> image;
                if (!check.TryGetValue(id, out image)) continue;
                double l;
                double h;
                bool present = wideLo.TryGetValue(id, out l) & wideHi.TryGetValue(id, out h);
                if (!present || image.Key < l || image.Value > h)
                {
                    bounds.Failures.Add(NotClosedCode + " : image [" + Deg(image.Key) + ", " + Deg(image.Value) + "] deg hors de I = ["
                        + (present ? Deg(l) + ", " + Deg(h) : "vide") + "] deg a l'entree de " + Describe(bounds.Elements[id])
                        + " apres " + bounds.Iterations + " iteration(s)" + (bounds.Converged ? "." : " (budget epuise)."));
                }
            }

            if (bounds.Failures.Count > 0) return bounds;

            foreach (var id in order)
            {
                var offsets = bounds.Elements[id];
                double l;
                double h;
                if (wideLo.TryGetValue(id, out l) && wideHi.TryGetValue(id, out h))
                {
                    offsets.HasEntry = true;
                    offsets.EntryLo = l;
                    offsets.EntryHi = h;
                    offsets.Lower = OffsetTrace.Integrate(offsets.Element, offsets.Element.StartS, l, a);
                    offsets.Upper = OffsetTrace.Integrate(offsets.Element, offsets.Element.StartS, h, a);
                    for (int k = 0; k < offsets.Lower.E.Count; k++)
                    {
                        if (offsets.Lower.E[k] > offsets.Upper.E[k])
                        {
                            bounds.Failures.Add(NotClosedCode + " : transport d'ecart non monotone dans " + Describe(offsets) + " (s = "
                                + R((float)offsets.Lower.S[k]) + " m).");
                            break;
                        }
                    }
                }

                foreach (var trace in new[] { offsets.Lower, offsets.Upper })
                {
                    if (trace != null) CheckBelowRightAngle(bounds, offsets, trace);
                }

                foreach (var trace in offsets.Seeds) CheckBelowRightAngle(bounds, offsets, trace);
                if (!offsets.Reachable) bounds.Unreachable.Add(id);
            }

            return bounds;
        }

        /// <summary>
        /// Enveloppe des ecarts atteignables sur [s0, s1] de l'element, elargie du reste entre noeuds et de eta.
        /// Faux si aucune solution n'y existe (element ou portion inatteignable).
        /// </summary>
        public bool TryHull(RoadId id, double s0, double s1, out double lo, out double hi)
        {
            lo = 0d;
            hi = 0d;
            ElementOffsets offsets;
            if (!Elements.TryGetValue(id, out offsets)) return false;
            if (s1 < s0)
            {
                double swap = s0;
                s0 = s1;
                s1 = swap;
            }

            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;
            bool any = false;
            if (offsets.HasEntry)
            {
                any |= offsets.Lower.Range(s0, s1, ref min, ref max);
                any |= offsets.Upper.Range(s0, s1, ref min, ref max);
            }

            foreach (var seed in offsets.Seeds) any |= seed.Range(s0, s1, ref min, ref max);
            if (!any) return false;
            double pad = offsets.NodeRemainderRadians + ToleranceRadians;
            lo = min - pad;
            hi = max + pad;
            return true;
        }

        /// <summary>
        /// Faisabilite sur tout l'ensemble de poses (contrat §8) a chaque noeud de pas &lt;= 0,1 m : braquage
        /// (v*_N(e) &gt;= vitesse de direction active) et taux v |d delta/ds| &lt;= taux declare, v = min(vitesse
        /// desiree, v*_N(e), racine(mu g / |kappa|)), evalue sur la grille des ecarts plus un reste de Lipschitz.
        /// </summary>
        public void CheckFeasibility(DrivabilityProfile drivability, GateAEvidenceParameters parameters)
        {
            double a = drivability.ReferencePointAheadRearAxleMeters;
            double ratio = drivability.WheelbaseMeters / a;
            var inputs = parameters.Feasibility;
            var order = new List<RoadId>(Elements.Keys);
            order.Sort();
            foreach (var id in order)
            {
                var offsets = Elements[id];
                if (!offsets.Reachable) continue;
                var element = offsets.Element;
                double length = element.EndS - element.StartS;
                int nodes = Math.Max(1, (int)Math.Ceiling(length / RoadCurve.KinematicOffsetStepMeters));
                double ds = length / nodes;
                int failed = 0;
                double firstS = double.NaN;
                double lastS = double.NaN;
                double worstOffset = 0d;
                double worstCeiling = double.PositiveInfinity;
                double worstRate = 0d;
                for (int j = 0; j <= nodes; j++)
                {
                    double s = element.StartS + j * ds;
                    double s0 = Math.Max(element.StartS, s - 0.5d * ds);
                    double s1 = Math.Min(element.EndS, s + 0.5d * ds);
                    double lo;
                    double hi;
                    if (!TryHull(id, s0, s1, out lo, out hi)) continue;
                    double kappaMin;
                    double kappaMax;
                    CurvatureRange(element, s0, s1, out kappaMin, out kappaMax);
                    FeasibilityChecks++;

                    double extreme = Math.Max(Math.Abs(lo), Math.Abs(hi));
                    double ceiling = PathHorizon.NominalSteeringCeilingMetersPerSecond(drivability, (float)extreme);
                    double lockMargin = ceiling - drivability.SteeringInactiveBelowMetersPerSecond;
                    MinimumLockSpeedMarginMetersPerSecond = Math.Min(MinimumLockSpeedMarginMetersPerSecond, lockMargin);

                    double rate = MaximumSteerRateDegreesPerSecond(drivability, inputs, ratio, a, lo, hi, kappaMin, kappaMax,
                        parameters.OffsetGridStepRadians);
                    MinimumSteerRateMarginDegreesPerSecond = Math.Min(MinimumSteerRateMarginDegreesPerSecond,
                        inputs.SteerRateDegreesPerSecond - rate);
                    if (lockMargin >= 0d && rate <= inputs.SteerRateDegreesPerSecond) continue;
                    if (failed == 0) firstS = s;
                    lastS = s;
                    failed++;
                    worstOffset = Math.Max(worstOffset, extreme);
                    worstCeiling = Math.Min(worstCeiling, ceiling);
                    worstRate = Math.Max(worstRate, rate);
                }

                if (failed > 0)
                {
                    Infeasible.Add(InfeasibleCode + " : " + Describe(offsets) + " s [" + R((float)firstS) + ", " + R((float)lastS) + "] m, "
                        + failed + " noeud(s), |e| max " + Deg(worstOffset) + " deg, v*_N min " + R((float)worstCeiling) + " m/s (seuil "
                        + R(drivability.SteeringInactiveBelowMetersPerSecond) + "), taux max " + R((float)worstRate) + " deg/s (limite "
                        + R(inputs.SteerRateDegreesPerSecond) + ").");
                }
            }
        }

        /// <summary>
        /// Majorant du taux de braquage (deg/s) sur e dans [lo, hi] et kappa dans [kMin, kMax] : par cellule de la
        /// grille (0 compris comme point de rupture), v au |e| minimal de la cellule (v*_N decroissant en |e|) et
        /// |d delta/ds| max aux coins plus la borne de Lipschitz sur la demi-cellule.
        /// </summary>
        public static double MaximumSteerRateDegreesPerSecond(DrivabilityProfile drivability, NominalPoseFeasibilityInputs inputs,
            double ratio, double a, double lo, double hi, double kappaMin, double kappaMax, float gridStep)
        {
            double absoluteMax = Math.Max(Math.Abs(kappaMin), Math.Abs(kappaMax));
            double absoluteMin = kappaMin <= 0d && kappaMax >= 0d ? 0d : Math.Min(Math.Abs(kappaMin), Math.Abs(kappaMax));
            double cap = inputs.DesiredSpeedMetersPerSecond;
            if (absoluteMin > 0d) cap = Math.Min(cap, Math.Sqrt(inputs.LateralGripMetersPerSecondSquared / absoluteMin));
            double lipschitz = NominalPoseFeasibility.WheelAngleRateLipschitz(ratio, a, absoluteMax);
            var points = GridPoints(lo, hi, gridStep, true);
            double worst = 0d;
            for (int k = 0; k < points.Count; k++)
            {
                double e0 = points[k];
                double e1 = k + 1 < points.Count ? points[k + 1] : e0;
                if (k + 1 >= points.Count && points.Count > 1) break;
                double nearest = e0 <= 0d && e1 >= 0d ? 0d : Math.Min(Math.Abs(e0), Math.Abs(e1));
                double ceiling = PathHorizon.NominalSteeringCeilingMetersPerSecond(drivability, (float)nearest);
                double speed = Math.Min(cap, ceiling);
                double gain = Math.Max(
                    Math.Max(Math.Abs(NominalPoseFeasibility.WheelAngleRatePerMeter(ratio, a, e0, kappaMin)),
                        Math.Abs(NominalPoseFeasibility.WheelAngleRatePerMeter(ratio, a, e0, kappaMax))),
                    Math.Max(Math.Abs(NominalPoseFeasibility.WheelAngleRatePerMeter(ratio, a, e1, kappaMin)),
                        Math.Abs(NominalPoseFeasibility.WheelAngleRatePerMeter(ratio, a, e1, kappaMax))))
                    + lipschitz * 0.5d * (e1 - e0);
                worst = Math.Max(worst, speed * gain * (180d / Math.PI));
            }

            return worst;
        }

        /// <summary>Points de grille de pas &lt;= step sur [lo, hi], bornes comprises (et 0 si demande et interieur).</summary>
        public static List<double> GridPoints(double lo, double hi, float step, bool includeZero)
        {
            var points = new List<double>();
            int count = hi > lo ? (int)Math.Ceiling((hi - lo) / step) : 0;
            for (int k = 0; k <= count; k++) points.Add(count == 0 ? lo : lo + (hi - lo) * k / count);
            if (includeZero && lo < 0d && hi > 0d)
            {
                points.Add(0d);
                points.Sort();
            }

            return points;
        }

        public static double MaximumAbsoluteSine(double lo, double hi)
        {
            return Math.Sin(Math.Min(HalfPi, Math.Max(Math.Abs(lo), Math.Abs(hi))));
        }

        /// <summary>Tableau canonique des intervalles d'entree, par element (degres).</summary>
        public string Render()
        {
            var text = new StringBuilder();
            text.Append("| Element | Genre | Entree I_X (deg) | Amorces | Etat |\n|---|---|---|---:|---|\n");
            var order = new List<RoadId>(Elements.Keys);
            order.Sort();
            foreach (var id in order)
            {
                var offsets = Elements[id];
                text.Append("| ").Append(id).Append(" | ").Append(offsets.IsMovement ? "mouvement" : "corridor").Append(" | ")
                    .Append(offsets.HasEntry ? "[" + Deg(offsets.EntryLo) + ", " + Deg(offsets.EntryHi) + "]" : "-").Append(" | ")
                    .Append(offsets.Seeds.Count).Append(" | ").Append(offsets.Reachable ? "atteignable" : "inatteignable").Append(" |\n");
            }

            return text.ToString();
        }

        private static Dictionary<RoadId, KeyValuePair<double, double>> Images(KinematicOffsetBounds bounds, List<RoadId> order,
            Dictionary<RoadId, double> lo, Dictionary<RoadId, double> hi, Dictionary<RoadId, List<double>> seedExits,
            Dictionary<RoadId, Dictionary<RoadId, double>> jumps, float a)
        {
            var images = new Dictionary<RoadId, KeyValuePair<double, double>>();
            foreach (var id in order)
            {
                var element = bounds.Elements[id].Element;
                double exitLo = double.PositiveInfinity;
                double exitHi = double.NegativeInfinity;
                bool any = false;
                double l;
                double h;
                if (lo.TryGetValue(id, out l) && hi.TryGetValue(id, out h))
                {
                    double lower = element.Curve.AdvanceKinematicOffset(element.StartS, element.EndS, l, a);
                    double upper = element.Curve.AdvanceKinematicOffset(element.StartS, element.EndS, h, a);
                    exitLo = Math.Min(lower, upper);
                    exitHi = Math.Max(lower, upper);
                    any = true;
                }

                List<double> seeds;
                if (seedExits.TryGetValue(id, out seeds))
                {
                    foreach (double value in seeds)
                    {
                        exitLo = Math.Min(exitLo, value);
                        exitHi = Math.Max(exitHi, value);
                    }

                    any |= seeds.Count > 0;
                }

                if (!any) continue;
                foreach (var jump in jumps[id])
                {
                    double imageLo = exitLo + jump.Value;
                    double imageHi = exitHi + jump.Value;
                    KeyValuePair<double, double> current;
                    images[jump.Key] = images.TryGetValue(jump.Key, out current)
                        ? new KeyValuePair<double, double>(Math.Min(current.Key, imageLo), Math.Max(current.Value, imageHi))
                        : new KeyValuePair<double, double>(imageLo, imageHi);
                }
            }

            return images;
        }

        private static double NodeRemainder(SweepElement element, float a)
        {
            double curvature = 0d;
            double slope = 0d;
            var samples = element.Samples;
            for (int i = 0; i < samples.Count; i++)
            {
                curvature = Math.Max(curvature, Math.Abs(samples[i].CurvaturePerMeter));
                if (i == 0) continue;
                double span = samples[i].SMeters - samples[i - 1].SMeters;
                if (span > 0d) slope = Math.Max(slope, Math.Abs(samples[i].CurvaturePerMeter - samples[i - 1].CurvaturePerMeter) / span);
            }

            double h = RoadCurve.KinematicOffsetStepMeters;
            return h * h / 8d * (slope + (curvature + 1d / a) / a);
        }

        private static void CurvatureRange(SweepElement element, double s0, double s1, out double min, out double max)
        {
            double k0 = element.Curve.Sample((float)s0).CurvaturePerMeter;
            double k1 = element.Curve.Sample((float)s1).CurvaturePerMeter;
            min = Math.Min(k0, k1);
            max = Math.Max(k0, k1);
            foreach (var sample in element.Samples)
            {
                if (sample.SMeters > s0 && sample.SMeters < s1)
                {
                    min = Math.Min(min, sample.CurvaturePerMeter);
                    max = Math.Max(max, sample.CurvaturePerMeter);
                }
            }
        }

        private static void CheckBelowRightAngle(KinematicOffsetBounds bounds, ElementOffsets offsets, OffsetTrace trace)
        {
            for (int k = 0; k < trace.E.Count; k++)
            {
                if (Math.Abs(trace.E[k]) + offsets.NodeRemainderRadians + bounds.ToleranceRadians < HalfPi) continue;
                bounds.Failures.Add(NotClosedCode + " : |e| >= 90 deg dans " + Describe(offsets) + " (s = " + R((float)trace.S[k]) + " m).");
                return;
            }
        }

        internal static string Describe(ElementOffsets offsets)
        {
            return (offsets.IsMovement ? "mouvement " : "corridor ") + offsets.Id;
        }

        internal static string Deg(double radians)
        {
            return (radians * 180d / Math.PI).ToString("0.####", CultureInfo.InvariantCulture);
        }

        private static string R(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>Grille de caps de caisse a une pose balayee : enveloppe des ecarts atteignables et caps de la grille.</summary>
    public sealed class PoseGrid
    {
        public SweepPose Reference;
        public bool Unreachable;
        public double OffsetLo;
        public double OffsetHi;
        public Vector2[] Headings;
    }

    /// <summary>Ensemble de poses nominales cinematiques d'une trajectoire balayee (contrat §8).</summary>
    public static class KinematicPoseSet
    {
        /// <summary>Cap de caisse : tangente tournee de -e autour de road-up (convention de <c>ReferenceTrack.Nominal</c>).</summary>
        public static Vector2 BodyHeading(SweepPose pose, double offsetRadians)
        {
            Vector3 up = pose.Up.sqrMagnitude > 0f ? pose.Up : Vector3.up;
            Vector3 tangent = pose.Tangent.sqrMagnitude > 0f ? pose.Tangent : new Vector3(pose.Heading.x, 0f, pose.Heading.y);
            Vector3 forward = Quaternion.AngleAxis(-(float)offsetRadians * Mathf.Rad2Deg, up) * tangent;
            var heading = new Vector2(forward.x, forward.z);
            float length = heading.magnitude;
            return length > ConflictSweep.DegenerateHeadingEpsilon ? heading / length : Vector2.zero;
        }

        /// <summary>Copie de la pose avec le cap de caisse donne.</summary>
        public static SweepPose WithHeading(SweepPose pose, Vector2 heading)
        {
            var copy = pose;
            copy.Heading = heading;
            return copy;
        }

        /// <summary>
        /// Grilles de caps (pas &lt;= <paramref name="gridStep"/>, bornes comprises) sur l'enveloppe des ecarts
        /// atteignables sur les deux demi-segments adjacents, et rotation de caisse max de chaque intervalle :
        /// ds . max|sin e| / a sur un segment, 0 a un raccord (cap de caisse continu). Une portion inatteignable
        /// est balayee a e = 0 avec la rotation de la tangente (sur-ensemble). Nul si tout est valide, sinon le motif.
        /// </summary>
        public static string Build(IReadOnlyList<SweepPose> path, KinematicOffsetBounds bounds, float gridStep,
            out PoseGrid[] grids, out float[] rotations)
        {
            int count = path.Count;
            grids = new PoseGrid[count];
            rotations = new float[Math.Max(0, count - 1)];
            float a = bounds.ReferenceAheadRearAxleMeters;
            for (int i = 0; i < count; i++)
            {
                SweepPose pose = path[i];
                double left = i > 0 && path[i - 1].ElementId == pose.ElementId ? 0.5d * (pose.SMeters - path[i - 1].SMeters) : 0d;
                double right = i + 1 < count && path[i + 1].ElementId == pose.ElementId ? 0.5d * (path[i + 1].SMeters - pose.SMeters) : 0d;
                double lo;
                double hi;
                bool reachable = bounds.TryHull(pose.ElementId, pose.SMeters - Math.Abs(left), pose.SMeters + Math.Abs(right), out lo, out hi);
                if (!reachable)
                {
                    lo = 0d;
                    hi = 0d;
                }

                var points = KinematicOffsetBounds.GridPoints(lo, hi, gridStep, false);
                var headings = new Vector2[points.Count];
                for (int k = 0; k < points.Count; k++)
                {
                    headings[k] = BodyHeading(pose, points[k]);
                    if (headings[k] == Vector2.zero) return "cap de caisse degenere a la pose " + i + " (" + pose.ElementId + ")";
                }

                grids[i] = new PoseGrid { Reference = pose, Unreachable = !reachable, OffsetLo = lo, OffsetHi = hi, Headings = headings };
            }

            for (int i = 0; i + 1 < count; i++)
            {
                SweepPose p0 = path[i];
                SweepPose p1 = path[i + 1];
                float rotation;
                if (grids[i].Unreachable || grids[i + 1].Unreachable)
                {
                    rotation = ConflictSweep.HeadingChange(p0, p1);
                }
                else if (p0.ElementId != p1.ElementId)
                {
                    rotation = 0f;
                }
                else
                {
                    double lo;
                    double hi;
                    if (!bounds.TryHull(p0.ElementId, p0.SMeters, p1.SMeters, out lo, out hi))
                        return "enveloppe d'ecart introuvable entre les poses " + i + " et " + (i + 1);
                    rotation = (float)(Math.Abs(p1.SMeters - p0.SMeters) * KinematicOffsetBounds.MaximumAbsoluteSine(lo, hi) / a);
                }

                if (rotation >= ConflictSweep.FailClosedHeadingRadians)
                    return "rotation de caisse de 90 deg ou plus entre les poses " + i + " et " + (i + 1);
                rotations[i] = rotation;
            }

            return null;
        }
    }
}
#endif
