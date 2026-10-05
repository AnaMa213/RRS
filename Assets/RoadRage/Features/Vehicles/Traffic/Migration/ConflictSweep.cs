#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    /// <summary>Pose plane d'une trajectoire balayee : position et cap horizontal unitaire.</summary>
    public struct SweepPose
    {
        public Vector3 Position;

        /// <summary>Cap horizontal unitaire (x, z) ; nul si la tangente n'a pas de partie horizontale.</summary>
        public Vector2 Heading;

        public bool Degenerate;

        // Provenance de la courbe compilee, ignoree par le balayage de conflits 5.50.
        // La preuve de degagement 5.51 subdivise chaque segment sans franchir ses coutures.
        public RoadId ElementId;
        public float SMeters;

        // Tangente 3D et road-up de la courbe compilee : la pose cinematique (Story 5.52) tourne la tangente
        // de -e autour de road-up. Ignores par les balayages a pose tangente.
        public Vector3 Tangent;
        public Vector3 Up;

        public Vector2 Plan
        {
            get { return new Vector2(Position.x, Position.z); }
        }

        public static SweepPose From(Vector3 position, Vector3 tangent)
        {
            var pose = new SweepPose();
            pose.Position = position;
            pose.Tangent = tangent;
            var horizontal = new Vector2(tangent.x, tangent.z);
            float length = horizontal.magnitude;
            pose.Degenerate = !(length > ConflictSweep.DegenerateHeadingEpsilon);
            pose.Heading = pose.Degenerate ? Vector2.zero : horizontal / length;
            return pose;
        }
    }

    /// <summary>Relation publiee d'une paire de mouvements d'un meme carrefour.</summary>
    public enum PairRelation
    {
        /// <summary>Contact possible prouve par la distance exacte des empreintes : candidat.</summary>
        Candidate,

        /// <summary>Intervalle hors hypotheses (cap degenere, rotation >= 90 deg, portee trop ramifiee) : candidat par prudence.</summary>
        FailClosed,

        /// <summary>Retenue par les seules boites englobantes (AABB), ecartee par la distance exacte : pas un candidat.</summary>
        EnvelopeOnly,

        /// <summary>Aucun contact possible, meme par les boites englobantes.</summary>
        NoContact,

        /// <summary>Meme corridor d'approche : suivi, jamais un conflit.</summary>
        SameApproach,

        /// <summary>L'un suit l'autre sur un chemin a une voie : suivi, pas un conflit.</summary>
        Following
    }

    /// <summary>Resultat du balayage d'une paire : selection englobante, preuve exacte et temoin.</summary>
    public sealed class PairSweep
    {
        public RoadId JunctionId;
        public RoadId MovementA;
        public RoadId MovementB;
        public PairRelation Relation;

        /// <summary>Critere des boites englobantes (AABB des rectangles orientes) satisfait sur au moins un intervalle.</summary>
        public bool EnvelopeSelected;

        /// <summary>Critere de la distance exacte rectangle-rectangle satisfait sur au moins un intervalle.</summary>
        public bool ExactProven;

        public string FailClosedReason;

        /// <summary>Min sur les intervalles de (distance exacte - borne) ; &lt;= 0 : contact possible.</summary>
        public float ExactSlackMeters = float.PositiveInfinity;

        /// <summary>Min sur les intervalles de (distance AABB - borne) ; &lt;= 0 : selection englobante.</summary>
        public float EnvelopeSlackMeters = float.PositiveInfinity;

        public bool HasWitness;
        public SweepPose WitnessA;
        public SweepPose WitnessB;

        public bool HasVolume;
        public RoadBoundsBox Volume;

        public int PathsA;
        public int PathsB;

        /// <summary>Gonflement de chaque empreinte hors restes : marge + delta_c + a_e (Story 5.52).</summary>
        public float BaseInflationMeters;

        /// <summary>Reste de grille des ecarts rho . h_e / 2 ajoute au gonflement (pose cinematique ; 0 sinon).</summary>
        public float OffsetGridRemainderMeters;

        public bool IsCandidate
        {
            get { return Relation == PairRelation.Candidate || Relation == PairRelation.FailClosed; }
        }
    }

    /// <summary>Element de la trajectoire dirigee connectee : corridor ou mouvement compile.</summary>
    public sealed class SweepElement
    {
        public RoadId Id;
        public bool IsMovement;
        public IReadOnlyList<RoadCurveSample> Samples;
        public RoadCurve Curve;
        public readonly List<SweepElement> Next = new List<SweepElement>();
        public readonly List<SweepElement> Previous = new List<SweepElement>();

        public float StartS
        {
            get { return Samples[0].SMeters; }
        }

        public float EndS
        {
            get { return Samples[Samples.Count - 1].SMeters; }
        }

        public float Length
        {
            get { return EndS - StartS; }
        }
    }

    /// <summary>Graphe dirige des corridors et mouvements, sans semantique de carrefour.</summary>
    public sealed class SweepGraph
    {
        public readonly Dictionary<RoadId, SweepElement> Elements = new Dictionary<RoadId, SweepElement>();

        public SweepElement Add(RoadId id, bool isMovement, IReadOnlyList<RoadCurveSample> samples)
        {
            if (samples == null || samples.Count < 2)
            {
                throw new ArgumentException("Element " + id + " : au moins deux echantillons attendus.");
            }

            var element = new SweepElement();
            element.Id = id;
            element.IsMovement = isMovement;
            element.Samples = samples;
            element.Curve = new RoadCurve(samples);
            Elements.Add(id, element);
            return element;
        }

        public void Link(RoadId from, RoadId to)
        {
            SweepElement a;
            SweepElement b;
            if (!Elements.TryGetValue(from, out a) || !Elements.TryGetValue(to, out b))
            {
                return;
            }

            if (!a.Next.Contains(b))
            {
                a.Next.Add(b);
            }

            if (!b.Previous.Contains(a))
            {
                b.Previous.Add(a);
            }
        }

        public static SweepGraph FromModel(CompiledRoadModel model)
        {
            var graph = new SweepGraph();
            foreach (var corridor in model.Corridors)
            {
                graph.Add(corridor.CorridorId, false, corridor.Samples);
            }

            foreach (var movement in model.Movements)
            {
                graph.Add(movement.Id, true, movement.Samples);
            }

            foreach (var movement in model.Movements)
            {
                graph.Link(movement.FromCorridorId, movement.Id);
                graph.Link(movement.Id, movement.ToCorridorId);
            }

            foreach (var connection in model.Connections)
            {
                graph.Link(connection.FromCorridorId, connection.ToCorridorId);
            }

            return graph;
        }
    }

    /// <summary>Element plus court que le gabarit maximal, publie dans le differentiel.</summary>
    public struct ShortElement
    {
        public RoadId Id;
        public bool IsMovement;
        public float LengthMeters;
    }

    /// <summary>
    /// Balayage conservateur des candidats de conflit (Story 5.50, P1). La trajectoire de chaque
    /// mouvement est prolongee d'un demi-gabarit sur les elements adjacents (coutures et elements
    /// courts traverses). Entre deux poses consecutives, la position est lineaire et le cap monotone,
    /// donc toute l'empreinte reste a delta/2 = (|dp| + rho.|dtheta|)/2 d'une pose d'extremite : une
    /// paire est candidate si, pour deux intervalles I et J et deux poses d'extremite a et b,
    /// dist(F_a, F_b) &lt;= delta_I/2 + delta_J/2 + 2(marge + delta_c). La distance qui decide est la
    /// distance exacte entre rectangles orientes ; les boites englobantes ne servent qu'a
    /// preselectionner et a publier les paires qu'elles seules retiendraient.
    /// </summary>
    public static partial class ConflictSweep
    {
        public const int AlgorithmVersion = 1;

        /// <summary>Balayage sur l'ensemble de poses nominales cinematiques (Story 5.52).</summary>
        public const int KinematicAlgorithmVersion = 2;

        public static int AlgorithmVersionFor(GateAEvidenceParameters parameters)
        {
            return parameters != null && parameters.Kinematic ? KinematicAlgorithmVersion : AlgorithmVersion;
        }
        public const float DegenerateHeadingEpsilon = 1e-6f;
        public const float FailClosedHeadingRadians = 0.5f * Mathf.PI;

        /// <summary>Plafond des trajectoires prolongees par mouvement ; au-dela, echec ferme.</summary>
        public const int MaxPathsPerMovement = 64;

        public static float HalfLength(RoadModelValidationProfile profile)
        {
            return 0.5f * profile.MaxVehicleLengthMeters;
        }

        public static float Rho(RoadModelValidationProfile profile)
        {
            float halfLength = HalfLength(profile);
            return Mathf.Sqrt(halfLength * halfLength + profile.MaxVehicleHalfWidthMeters * profile.MaxVehicleHalfWidthMeters);
        }

        /// <summary>Gonflement de chaque empreinte : marge du profil plus porte de courbe compilee delta_c.</summary>
        public static float Inflation(RoadModelValidationProfile profile)
        {
            return profile.LateralClearanceMarginMeters + V1RoadModelImporter.ChordToleranceMeters;
        }

        /// <summary>
        /// Gonflement d'une preuve (Story 5.52) : marge + delta_c + a_e, plus le reste de grille rho . h_e / 2 en
        /// pose cinematique. Parametres historiques : exactement <see cref="Inflation(RoadModelValidationProfile)"/>.
        /// </summary>
        public static float EvidenceInflation(RoadModelValidationProfile profile, GateAEvidenceParameters parameters)
        {
            float inflation = Inflation(profile) + parameters.TrackingAllowanceMeters;
            return parameters.Kinematic ? inflation + OffsetGridRemainder(profile, parameters) : inflation;
        }

        /// <summary>Reste de la grille des ecarts : tout cap intermediaire est a h_e/2 d'un cap de grille, donc a rho h_e / 2.</summary>
        public static float OffsetGridRemainder(RoadModelValidationProfile profile, GateAEvidenceParameters parameters)
        {
            return parameters.Kinematic ? Rho(profile) * parameters.OffsetGridStepRadians * 0.5f : 0f;
        }

        /// <summary>Portee du point de reference au-dela de chaque extremite de mouvement.</summary>
        public static float Reach(RoadModelValidationProfile profile)
        {
            return HalfLength(profile);
        }

        // ============================================================ modele

        /// <summary>
        /// Balayage de toutes les paires de chaque carrefour, dans l'ordre des candidats. Sans parametres :
        /// preuve signee (pose tangente, a_e = 0). Pose cinematique : <paramref name="bounds"/> obligatoire.
        /// </summary>
        public static List<PairSweep> Analyze(CompiledRoadModel model, GateAEvidenceParameters parameters = null,
            KinematicOffsetBounds bounds = null)
        {
            parameters = parameters ?? GateAEvidenceParameters.Legacy;
            if (parameters.Kinematic && (bounds == null || !bounds.Closed))
            {
                throw new ArgumentException("Balayage cinematique : bornes d'ecart fermees requises.", "bounds");
            }

            var graph = SweepGraph.FromModel(model);
            var profile = model.ValidationProfile;
            var results = new List<PairSweep>();
            var paths = new Dictionary<RoadId, List<List<SweepPose>>>();
            var pathFailures = new Dictionary<RoadId, string>();
            foreach (var junction in model.Junctions)
            {
                var ids = model.GetMovementsInJunction(junction.Id);
                for (int i = 0; i < ids.Count; i++)
                {
                    CompiledJunctionMovement a;
                    model.TryGetMovement(ids[i], out a);
                    for (int j = i + 1; j < ids.Count; j++)
                    {
                        CompiledJunctionMovement b;
                        model.TryGetMovement(ids[j], out b);
                        var sweep = new PairSweep();
                        sweep.JunctionId = junction.Id;
                        sweep.MovementA = ids[i];
                        sweep.MovementB = ids[j];
                        if (a.FromCorridorId == b.FromCorridorId)
                        {
                            sweep.Relation = PairRelation.SameApproach;
                            results.Add(sweep);
                            continue;
                        }

                        if (Follows(graph, a.ToCorridorId, b.FromCorridorId, profile.MaxVehicleLengthMeters)
                            || Follows(graph, b.ToCorridorId, a.FromCorridorId, profile.MaxVehicleLengthMeters))
                        {
                            sweep.Relation = PairRelation.Following;
                            results.Add(sweep);
                            continue;
                        }

                        string failureA;
                        string failureB;
                        var pathsA = CachedPaths(graph, ids[i], profile, paths, pathFailures, out failureA);
                        var pathsB = CachedPaths(graph, ids[j], profile, paths, pathFailures, out failureB);
                        if (failureA != null || failureB != null)
                        {
                            sweep.Relation = PairRelation.FailClosed;
                            sweep.FailClosedReason = failureA ?? failureB;
                            sweep.PathsA = pathsA == null ? 0 : pathsA.Count;
                            sweep.PathsB = pathsB == null ? 0 : pathsB.Count;
                            FailClosedVolume(sweep, pathsA, pathsB, profile, EvidenceInflation(profile, parameters));
                            results.Add(sweep);
                            continue;
                        }

                        if (parameters.Kinematic)
                        {
                            EvaluateKinematic(pathsA, pathsB, profile, parameters, bounds, sweep);
                        }
                        else
                        {
                            Evaluate(pathsA, pathsB, profile, sweep, parameters.TrackingAllowanceMeters);
                        }

                        results.Add(sweep);
                    }
                }
            }

            return results;
        }

        public static List<ConflictCandidate> ToCandidates(IList<PairSweep> sweeps)
        {
            var candidates = new List<ConflictCandidate>();
            foreach (var sweep in sweeps)
            {
                if (!sweep.IsCandidate)
                {
                    continue;
                }

                var candidate = new ConflictCandidate();
                candidate.JunctionId = sweep.JunctionId;
                candidate.MovementA = sweep.MovementA;
                candidate.MovementB = sweep.MovementB;
                candidate.Volume = sweep.Volume;
                candidates.Add(candidate);
            }

            return candidates;
        }

        /// <summary>Corridors et mouvements plus courts que le gabarit maximal.</summary>
        public static List<ShortElement> ShortElements(CompiledRoadModel model)
        {
            var list = new List<ShortElement>();
            float limit = model.ValidationProfile.MaxVehicleLengthMeters;
            foreach (var corridor in model.Corridors)
            {
                if (corridor.LengthMeters < limit)
                {
                    list.Add(new ShortElement { Id = corridor.CorridorId, IsMovement = false, LengthMeters = corridor.LengthMeters });
                }
            }

            foreach (var movement in model.Movements)
            {
                if (movement.LengthMeters < limit)
                {
                    list.Add(new ShortElement { Id = movement.Id, IsMovement = true, LengthMeters = movement.LengthMeters });
                }
            }

            list.Sort(delegate (ShortElement x, ShortElement y) { return x.Id.CompareTo(y.Id); });
            return list;
        }

        /// <summary>
        /// Vrai si le corridor <paramref name="toCorridor"/> est atteint depuis
        /// <paramref name="fromCorridor"/> par des corridors seulement (connexions, sans autre
        /// mouvement), la longueur cumulee des corridors traverses avant lui ne depassant pas
        /// <paramref name="maxDistance"/>.
        /// </summary>
        public static bool Follows(SweepGraph graph, RoadId fromCorridor, RoadId toCorridor, float maxDistance)
        {
            SweepElement start;
            if (!graph.Elements.TryGetValue(fromCorridor, out start) || start.IsMovement)
            {
                return false;
            }

            var best = new Dictionary<RoadId, float>();
            var stack = new Stack<KeyValuePair<SweepElement, float>>();
            stack.Push(new KeyValuePair<SweepElement, float>(start, 0f));
            while (stack.Count > 0)
            {
                var entry = stack.Pop();
                SweepElement element = entry.Key;
                float before = entry.Value;
                if (element.Id == toCorridor)
                {
                    return true;
                }

                float seen;
                if (best.TryGetValue(element.Id, out seen) && seen <= before)
                {
                    continue;
                }

                best[element.Id] = before;
                float after = before + element.Length;
                if (after > maxDistance)
                {
                    continue;
                }

                foreach (var next in element.Next)
                {
                    if (!next.IsMovement)
                    {
                        stack.Push(new KeyValuePair<SweepElement, float>(next, after));
                    }
                }
            }

            return false;
        }

        // ============================================================ trajectoires prolongees

        private static List<List<SweepPose>> CachedPaths(
            SweepGraph graph,
            RoadId movement,
            RoadModelValidationProfile profile,
            Dictionary<RoadId, List<List<SweepPose>>> cache,
            Dictionary<RoadId, string> failures,
            out string failure)
        {
            List<List<SweepPose>> paths;
            if (cache.TryGetValue(movement, out paths))
            {
                failures.TryGetValue(movement, out failure);
                return paths;
            }

            paths = Paths(graph, movement, Reach(profile), out failure);
            cache[movement] = paths;
            if (failure != null)
            {
                failures[movement] = failure;
            }

            return paths;
        }

        /// <summary>
        /// Trajectoires du point de reference : queue des predecesseurs sur <paramref name="reach"/>,
        /// le mouvement entier, tete des successeurs sur <paramref name="reach"/>. Un element plus
        /// court que la portee restante est traverse ; chaque embranchement produit une trajectoire.
        /// Deux poses consecutives issues d'elements differents forment l'intervalle de couture.
        /// </summary>
        public static List<List<SweepPose>> Paths(SweepGraph graph, RoadId movement, float reach, out string failure)
        {
            failure = null;
            SweepElement element;
            if (!graph.Elements.TryGetValue(movement, out element))
            {
                failure = "mouvement " + movement + " absent du graphe";
                return null;
            }

            var backward = new List<List<SweepPose>>();
            CollectBackward(element.Previous, reach, new List<SweepPose>(), backward, 0);
            if (backward.Count == 0)
            {
                backward.Add(new List<SweepPose>());
            }

            var forward = new List<List<SweepPose>>();
            CollectForward(element.Next, reach, new List<SweepPose>(), forward, 0);
            if (forward.Count == 0)
            {
                forward.Add(new List<SweepPose>());
            }

            if ((long)backward.Count * forward.Count > MaxPathsPerMovement)
            {
                failure = "portee du mouvement " + movement + " trop ramifiee (" + backward.Count + " x " + forward.Count + " trajectoires)";
                return null;
            }

            var own = Poses(element);
            var paths = new List<List<SweepPose>>();
            foreach (var before in backward)
            {
                foreach (var after in forward)
                {
                    var path = new List<SweepPose>(before.Count + own.Count + after.Count);
                    path.AddRange(before);
                    path.AddRange(own);
                    path.AddRange(after);
                    paths.Add(path);
                }
            }

            return paths;
        }

        private static void CollectBackward(List<SweepElement> previous, float remaining, List<SweepPose> suffix, List<List<SweepPose>> output, int depth)
        {
            if (remaining <= 0f || previous.Count == 0 || depth > MaxPathsPerMovement)
            {
                if (suffix.Count > 0)
                {
                    output.Add(suffix);
                }

                return;
            }

            foreach (var element in previous)
            {
                var poses = Tail(element, remaining);
                poses.AddRange(suffix);
                float left = remaining - element.Length;
                if (left > 0f && element.Previous.Count > 0)
                {
                    CollectBackward(element.Previous, left, poses, output, depth + 1);
                }
                else
                {
                    output.Add(poses);
                }
            }
        }

        private static void CollectForward(List<SweepElement> next, float remaining, List<SweepPose> prefix, List<List<SweepPose>> output, int depth)
        {
            if (remaining <= 0f || next.Count == 0 || depth > MaxPathsPerMovement)
            {
                if (prefix.Count > 0)
                {
                    output.Add(prefix);
                }

                return;
            }

            foreach (var element in next)
            {
                var poses = new List<SweepPose>(prefix);
                poses.AddRange(Head(element, remaining));
                float left = remaining - element.Length;
                if (left > 0f && element.Next.Count > 0)
                {
                    CollectForward(element.Next, left, poses, output, depth + 1);
                }
                else
                {
                    output.Add(poses);
                }
            }
        }

        /// <summary>Les derniers <paramref name="length"/> metres, coupe evaluee par l'interpolation canonique.</summary>
        public static List<SweepPose> Tail(SweepElement element, float length)
        {
            if (length >= element.Length)
            {
                return Poses(element);
            }

            float cut = element.EndS - length;
            var poses = new List<SweepPose>();
            poses.Add(SamplePose(element, cut));
            foreach (var sample in element.Samples)
            {
                if (sample.SMeters > cut)
                {
                    poses.Add(KnotPose(element, sample));
                }
            }

            return poses;
        }

        /// <summary>Les premiers <paramref name="length"/> metres, coupe evaluee par l'interpolation canonique.</summary>
        public static List<SweepPose> Head(SweepElement element, float length)
        {
            if (length >= element.Length)
            {
                return Poses(element);
            }

            float cut = element.StartS + length;
            var poses = new List<SweepPose>();
            foreach (var sample in element.Samples)
            {
                if (sample.SMeters < cut)
                {
                    poses.Add(KnotPose(element, sample));
                }
            }

            poses.Add(SamplePose(element, cut));
            return poses;
        }

        private static SweepPose SamplePose(SweepElement element, float s)
        {
            RoadCurvePoint point = element.Curve.Sample(s);
            var pose = SweepPose.From(point.Position, point.Tangent);
            pose.ElementId = element.Id;
            pose.SMeters = s;
            pose.Up = point.Up;
            return pose;
        }

        // Valeurs compilees telles quelles : les poses 5.50 restent identiques au bit pres.
        private static SweepPose KnotPose(SweepElement element, RoadCurveSample sample)
        {
            var pose = SweepPose.From(sample.Position, sample.Tangent);
            pose.ElementId = element.Id;
            pose.SMeters = sample.SMeters;
            pose.Up = sample.Up;
            return pose;
        }

        private static List<SweepPose> Poses(SweepElement element)
        {
            var poses = new List<SweepPose>(element.Samples.Count);
            foreach (var sample in element.Samples)
            {
                poses.Add(KnotPose(element, sample));
            }

            return poses;
        }

        public static List<SweepPose> Poses(IReadOnlyList<RoadCurveSample> samples)
        {
            var poses = new List<SweepPose>(samples == null ? 0 : samples.Count);
            if (samples == null)
            {
                return poses;
            }

            foreach (var sample in samples)
            {
                poses.Add(SweepPose.From(sample.Position, sample.Tangent));
            }

            return poses;
        }

        // ============================================================ balayage pur

        /// <summary>Balayage de deux ensembles de trajectoires ; renseigne la relation, la preuve et le volume.</summary>
        public static PairSweep Evaluate(
            IList<List<SweepPose>> pathsA,
            IList<List<SweepPose>> pathsB,
            RoadModelValidationProfile profile,
            PairSweep into = null)
        {
            return Evaluate(pathsA, pathsB, profile, into, 0f);
        }

        /// <summary>Meme balayage a pose tangente, chaque empreinte gonflee en plus de l'allocation a_e (Story 5.52).</summary>
        public static PairSweep Evaluate(
            IList<List<SweepPose>> pathsA,
            IList<List<SweepPose>> pathsB,
            RoadModelValidationProfile profile,
            PairSweep into,
            float trackingAllowanceMeters)
        {
            var sweep = into ?? new PairSweep();
            sweep.PathsA = pathsA == null ? 0 : pathsA.Count;
            sweep.PathsB = pathsB == null ? 0 : pathsB.Count;
            if (sweep.PathsA == 0 || sweep.PathsB == 0)
            {
                sweep.Relation = PairRelation.NoContact;
                return sweep;
            }

            float halfLength = HalfLength(profile);
            float halfWidth = profile.MaxVehicleHalfWidthMeters;
            float rho = Rho(profile);
            float inflation = Inflation(profile) + trackingAllowanceMeters;
            sweep.BaseInflationMeters = inflation;
            string failure = HypothesisFailure(pathsA, "A") ?? HypothesisFailure(pathsB, "B");
            if (failure != null)
            {
                sweep.Relation = PairRelation.FailClosed;
                sweep.FailClosedReason = failure;
                FailClosedVolume(sweep, pathsA, pathsB, profile, inflation);
                return sweep;
            }

            var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            var framesA = Frames(pathsA, halfLength, halfWidth);
            var framesB = Frames(pathsB, halfLength, halfWidth);
            for (int pa = 0; pa < pathsA.Count; pa++)
            {
                for (int pb = 0; pb < pathsB.Count; pb++)
                {
                    EvaluatePaths(pathsA[pa], framesA[pa], pathsB[pb], framesB[pb], halfWidth, rho, inflation, sweep, ref min, ref max);
                }
            }

            if (sweep.ExactProven)
            {
                sweep.Relation = PairRelation.Candidate;
                SetVolume(sweep, min, max, halfWidth + inflation);
            }
            else
            {
                sweep.Relation = sweep.EnvelopeSelected ? PairRelation.EnvelopeOnly : PairRelation.NoContact;
            }

            return sweep;
        }

        private static void EvaluatePaths(
            List<SweepPose> a,
            PoseFrame[] framesA,
            List<SweepPose> b,
            PoseFrame[] framesB,
            float halfWidth,
            float rho,
            float inflation,
            PairSweep sweep,
            ref Vector3 min,
            ref Vector3 max)
        {
            int intervalsA = Math.Max(1, a.Count - 1);
            int intervalsB = Math.Max(1, b.Count - 1);
            var deltaB = new float[intervalsB];
            for (int ib = 0; ib < intervalsB; ib++)
            {
                deltaB[ib] = Delta(b[ib], b[Math.Min(ib + 1, b.Count - 1)], rho);
            }

            // Lignes de distances AABB reutilisees : la ligne du combo suivant est celle du combo courant.
            int pointsB = b.Count;
            var lineCur = new float[pointsB];
            var lineNext = new float[pointsB];
            for (int ib = 0; ib < pointsB; ib++)
            {
                lineCur[ib] = AabbDistance(framesA[0], framesB[ib]);
            }

            for (int ia = 0; ia < intervalsA; ia++)
            {
                SweepPose a0 = a[ia];
                int ia1 = Math.Min(ia + 1, a.Count - 1);
                SweepPose a1 = a[ia1];
                PoseFrame frameA0 = framesA[ia];
                PoseFrame frameA1 = framesA[ia1];
                float deltaA = Delta(a0, a1, rho);
                // lineNext est rempli pour TOUTE pose d'extremite, y compris la trajectoire a pose
                // unique (ia1 == ia) : un lineNext laisse a zero faisait passer toute paire par le
                // filtre englobant (envelope = 0) et publiait une marge fausse.
                for (int ib = 0; ib < pointsB; ib++)
                {
                    lineNext[ib] = AabbDistance(framesA[ia1], framesB[ib]);
                }

                for (int ib = 0; ib < intervalsB; ib++)
                {
                    int ib1 = Math.Min(ib + 1, b.Count - 1);
                    SweepPose b0 = b[ib];
                    SweepPose b1 = b[ib1];
                    float bound = 0.5f * deltaA + 0.5f * deltaB[ib] + 2f * inflation;
                    float envelope = Min4(lineCur[ib], lineCur[ib1], lineNext[ib], lineNext[ib1]);
                    sweep.EnvelopeSlackMeters = Math.Min(sweep.EnvelopeSlackMeters, envelope - bound);
                    if (envelope > bound)
                    {
                        // La distance AABB minore la distance exacte : aucun contact possible ici.
                        continue;
                    }

                    sweep.EnvelopeSelected = true;
                    int closest = ClosestIndex(frameA0, frameA1, framesB[ib], framesB[ib1], out float exact);
                    float slack = exact - bound;
                    if (slack < sweep.ExactSlackMeters)
                    {
                        sweep.ExactSlackMeters = slack;
                        sweep.HasWitness = true;
                        switch (closest)
                        {
                            case 0:
                                sweep.WitnessA = a0;
                                sweep.WitnessB = b0;
                                break;
                            case 1:
                                sweep.WitnessA = a0;
                                sweep.WitnessB = b1;
                                break;
                            case 2:
                                sweep.WitnessA = a1;
                                sweep.WitnessB = b0;
                                break;
                            default:
                                sweep.WitnessA = a1;
                                sweep.WitnessB = b1;
                                break;
                        }
                    }

                    if (slack > 0f)
                    {
                        continue;
                    }

                    sweep.ExactProven = true;
                    Encapsulate(ref min, ref max, frameA0, a0.Position.y, halfWidth, 0.5f * deltaA + inflation);
                    Encapsulate(ref min, ref max, frameA1, a1.Position.y, halfWidth, 0.5f * deltaA + inflation);
                    Encapsulate(ref min, ref max, framesB[ib], b0.Position.y, halfWidth, 0.5f * deltaB[ib] + inflation);
                    Encapsulate(ref min, ref max, framesB[ib1], b1.Position.y, halfWidth, 0.5f * deltaB[ib] + inflation);
                }

                var swap = lineCur;
                lineCur = lineNext;
                lineNext = swap;
            }
        }

        /// <summary>Index 0..3 (a0b0, a0b1, a1b0, a1b1) du couple d'extremites le plus proche, dans l'ordre de comparaison historique.</summary>
        private static int ClosestIndex(in PoseFrame a0, in PoseFrame a1, in PoseFrame b0, in PoseFrame b1, out float best)
        {
            int index = 0;
            best = RectangleDistance(a0, b0);
            float d = RectangleDistance(a0, b1);
            if (d < best)
            {
                best = d;
                index = 1;
            }

            d = RectangleDistance(a1, b0);
            if (d < best)
            {
                best = d;
                index = 2;
            }

            d = RectangleDistance(a1, b1);
            if (d < best)
            {
                best = d;
                index = 3;
            }

            return index;
        }

        /// <summary>Premiere hypothese violee (cap degenere ou rotation >= 90 deg), nul sinon.</summary>
        public static string HypothesisFailure(IList<List<SweepPose>> paths, string side)
        {
            foreach (var path in paths)
            {
                if (path.Count == 0)
                {
                    return "trajectoire " + side + " : aucune pose";
                }

                for (int i = 0; i < path.Count; i++)
                {
                    if (path[i].Degenerate)
                    {
                        return "trajectoire " + side + " : tangente sans partie horizontale a la pose " + i;
                    }

                    if (i > 0 && HeadingChange(path[i - 1], path[i]) >= FailClosedHeadingRadians)
                    {
                        return "trajectoire " + side + " : rotation de 90 deg ou plus entre les poses " + (i - 1) + " et " + i;
                    }
                }
            }

            return null;
        }

        /// <summary>Borne de deplacement d'intervalle delta = |dp| + rho.|dtheta| (plan horizontal).</summary>
        public static float Delta(SweepPose a, SweepPose b, float rho)
        {
            return (b.Plan - a.Plan).magnitude + rho * HeadingChange(a, b);
        }

        /// <summary>Angle non signe entre deux caps horizontaux, dans [0, pi].</summary>
        public static float HeadingChange(SweepPose a, SweepPose b)
        {
            float cross = a.Heading.x * b.Heading.y - a.Heading.y * b.Heading.x;
            float dot = Vector2.Dot(a.Heading, b.Heading);
            return Mathf.Abs(Mathf.Atan2(cross, dot));
        }

        /// <summary>Distance exacte entre deux rectangles orientes du plan (0 s'ils se recoupent).</summary>
        public static float RectangleDistance(SweepPose a, SweepPose b, float halfLength, float halfWidth)
        {
            return RectangleDistance(Frame(a, halfLength, halfWidth), Frame(b, halfLength, halfWidth));
        }

        private static float RectangleDistance(in PoseFrame a, in PoseFrame b)
        {
            if (Overlap(a, b))
            {
                return 0f;
            }

            float best = float.PositiveInfinity;
            for (int i = 0; i < 4; i++)
            {
                Vector2 e0 = Corner(b, i);
                Vector2 e1 = Corner(b, (i + 1) % 4);
                Vector2 f0 = Corner(a, i);
                Vector2 f1 = Corner(a, (i + 1) % 4);
                for (int k = 0; k < 4; k++)
                {
                    best = Math.Min(best, PointSegment(Corner(a, k), e0, e1));
                    best = Math.Min(best, PointSegment(Corner(b, k), f0, f1));
                }
            }

            return best;
        }

        /// <summary>Distance entre les boites englobantes axees (AABB) des deux rectangles : minore la distance exacte.</summary>
        public static float AabbDistance(SweepPose a, SweepPose b, float halfLength, float halfWidth)
        {
            return AabbDistance(Frame(a, halfLength, halfWidth), Frame(b, halfLength, halfWidth));
        }

        private static float AabbDistance(in PoseFrame a, in PoseFrame b)
        {
            float dx = Math.Max(0f, Math.Max(a.Min.x - b.Max.x, b.Min.x - a.Max.x));
            float dz = Math.Max(0f, Math.Max(a.Min.y - b.Max.y, b.Min.y - a.Max.y));
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        public static Vector2[] Corners(SweepPose pose, float halfLength, float halfWidth)
        {
            var frame = Frame(pose, halfLength, halfWidth);
            return new[] { frame.C0, frame.C1, frame.C2, frame.C3 };
        }

        /// <summary>Boite et coins du rectangle d'une pose, calcules une seule fois puis reutilises.</summary>
        private struct PoseFrame
        {
            public Vector2 Min;
            public Vector2 Max;
            public Vector2 C0;
            public Vector2 C1;
            public Vector2 C2;
            public Vector2 C3;
        }

        private static PoseFrame Frame(SweepPose pose, float halfLength, float halfWidth)
        {
            Vector2 forward = pose.Heading * halfLength;
            Vector2 right = new Vector2(pose.Heading.y, -pose.Heading.x) * halfWidth;
            Vector2 c = pose.Plan;
            float ex = Mathf.Abs(pose.Heading.x) * halfLength + Mathf.Abs(pose.Heading.y) * halfWidth;
            float ez = Mathf.Abs(pose.Heading.y) * halfLength + Mathf.Abs(pose.Heading.x) * halfWidth;
            var frame = new PoseFrame();
            frame.Min = new Vector2(c.x - ex, c.y - ez);
            frame.Max = new Vector2(c.x + ex, c.y + ez);
            frame.C0 = c + forward + right;
            frame.C1 = c + forward - right;
            frame.C2 = c - forward - right;
            frame.C3 = c - forward + right;
            return frame;
        }

        private static PoseFrame[][] Frames(IList<List<SweepPose>> paths, float halfLength, float halfWidth)
        {
            var frames = new PoseFrame[paths.Count][];
            for (int i = 0; i < paths.Count; i++)
            {
                var path = paths[i];
                var row = new PoseFrame[path.Count];
                for (int k = 0; k < path.Count; k++)
                {
                    row[k] = Frame(path[k], halfLength, halfWidth);
                }

                frames[i] = row;
            }

            return frames;
        }

        private static Vector2 Corner(in PoseFrame frame, int index)
        {
            switch (index)
            {
                case 0:
                    return frame.C0;
                case 1:
                    return frame.C1;
                case 2:
                    return frame.C2;
                default:
                    return frame.C3;
            }
        }

        private static bool Overlap(in PoseFrame a, in PoseFrame b)
        {
            return !Separated(a, b) && !Separated(b, a);
        }

        private static bool Separated(in PoseFrame owner, in PoseFrame other)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 edge = Corner(owner, i + 1) - Corner(owner, i);
                var axis = new Vector2(-edge.y, edge.x);
                float minA;
                float maxA;
                float minB;
                float maxB;
                Project(owner, axis, out minA, out maxA);
                Project(other, axis, out minB, out maxB);
                if (maxA < minB || maxB < minA)
                {
                    return true;
                }
            }

            return false;
        }

        private static void Project(in PoseFrame frame, Vector2 axis, out float min, out float max)
        {
            min = float.PositiveInfinity;
            max = float.NegativeInfinity;
            float value = Vector2.Dot(frame.C0, axis);
            min = Math.Min(min, value);
            max = Math.Max(max, value);
            value = Vector2.Dot(frame.C1, axis);
            min = Math.Min(min, value);
            max = Math.Max(max, value);
            value = Vector2.Dot(frame.C2, axis);
            min = Math.Min(min, value);
            max = Math.Max(max, value);
            value = Vector2.Dot(frame.C3, axis);
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }

        private static float PointSegment(Vector2 p, Vector2 s0, Vector2 s1)
        {
            Vector2 d = s1 - s0;
            float lengthSquared = d.sqrMagnitude;
            float t = lengthSquared > 0f ? Mathf.Clamp01(Vector2.Dot(p - s0, d) / lengthSquared) : 0f;
            return (p - (s0 + t * d)).magnitude;
        }

        private static float Min4(float a, float b, float c, float d)
        {
            return Math.Min(Math.Min(a, b), Math.Min(c, d));
        }

        private static void Encapsulate(ref Vector3 min, ref Vector3 max, in PoseFrame frame, float y, float halfWidth, float grow)
        {
            min = Vector3.Min(min, new Vector3(frame.Min.x - grow, y - halfWidth - grow, frame.Min.y - grow));
            max = Vector3.Max(max, new Vector3(frame.Max.x + grow, y + halfWidth + grow, frame.Max.y + grow));
        }

        private static void SetVolume(PairSweep sweep, Vector3 min, Vector3 max, float minVerticalExtent)
        {
            sweep.HasVolume = true;
            sweep.Volume.Center = 0.5f * (min + max);
            sweep.Volume.Extents = 0.5f * (max - min);
            sweep.Volume.Extents.y = Math.Max(sweep.Volume.Extents.y, minVerticalExtent);
        }

        /// <summary>Echec ferme : volume = boite de toutes les poses des deux trajectoires, gonflee.</summary>
        private static void FailClosedVolume(PairSweep sweep, IList<List<SweepPose>> pathsA, IList<List<SweepPose>> pathsB,
            RoadModelValidationProfile profile, float inflation)
        {
            float halfLength = HalfLength(profile);
            float halfWidth = profile.MaxVehicleHalfWidthMeters;
            float grow = Rho(profile) + inflation;
            var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            bool any = false;
            foreach (var group in new[] { pathsA, pathsB })
            {
                if (group == null)
                {
                    continue;
                }

                foreach (var path in group)
                {
                    foreach (var pose in path)
                    {
                        any = true;
                        min = Vector3.Min(min, new Vector3(pose.Position.x - halfLength - grow, pose.Position.y - halfWidth - grow, pose.Position.z - halfLength - grow));
                        max = Vector3.Max(max, new Vector3(pose.Position.x + halfLength + grow, pose.Position.y + halfWidth + grow, pose.Position.z + halfLength + grow));
                    }
                }
            }

            if (any)
            {
                SetVolume(sweep, min, max, halfWidth + inflation);
            }
        }

        // ============================================================ pose cinematique (Story 5.52)

        /// <summary>
        /// Balayage sur l'ensemble de poses nominales cinematiques (contrat §8) : a chaque pose, une grille de
        /// caps couvre l'enveloppe des ecarts atteignables ; entre deux poses, delta = |dp| + rho . rotation de
        /// caisse max ; chaque empreinte est gonflee de marge + delta_c + a_e + rho . h_e / 2. La distance qui
        /// decide reste la distance exacte entre rectangles orientes, minimale sur les grilles.
        /// </summary>
        public static PairSweep EvaluateKinematic(
            IList<List<SweepPose>> pathsA,
            IList<List<SweepPose>> pathsB,
            RoadModelValidationProfile profile,
            GateAEvidenceParameters parameters,
            KinematicOffsetBounds bounds,
            PairSweep into = null)
        {
            if (parameters == null || !parameters.Kinematic || bounds == null)
            {
                throw new ArgumentException("Balayage cinematique : parametres cinematiques et bornes requis.");
            }

            var sweep = into ?? new PairSweep();
            sweep.PathsA = pathsA == null ? 0 : pathsA.Count;
            sweep.PathsB = pathsB == null ? 0 : pathsB.Count;
            if (sweep.PathsA == 0 || sweep.PathsB == 0)
            {
                sweep.Relation = PairRelation.NoContact;
                return sweep;
            }

            float halfLength = HalfLength(profile);
            float halfWidth = profile.MaxVehicleHalfWidthMeters;
            float rho = Rho(profile);
            sweep.BaseInflationMeters = Inflation(profile) + parameters.TrackingAllowanceMeters;
            sweep.OffsetGridRemainderMeters = OffsetGridRemainder(profile, parameters);
            float inflation = sweep.BaseInflationMeters + sweep.OffsetGridRemainderMeters;
            GridPath[] gridsA = null;
            GridPath[] gridsB = null;
            string failure = HypothesisFailure(pathsA, "A") ?? HypothesisFailure(pathsB, "B");
            if (failure == null)
            {
                failure = GridPaths(pathsA, "A", bounds, parameters.OffsetGridStepRadians, halfLength, halfWidth, out gridsA)
                    ?? GridPaths(pathsB, "B", bounds, parameters.OffsetGridStepRadians, halfLength, halfWidth, out gridsB);
            }

            if (failure != null)
            {
                sweep.Relation = PairRelation.FailClosed;
                sweep.FailClosedReason = failure;
                FailClosedVolume(sweep, pathsA, pathsB, profile, inflation);
                return sweep;
            }

            var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            for (int pa = 0; pa < gridsA.Length; pa++)
            {
                for (int pb = 0; pb < gridsB.Length; pb++)
                {
                    EvaluateGridPaths(gridsA[pa], gridsB[pb], halfWidth, rho, inflation, sweep, ref min, ref max);
                }
            }

            if (sweep.ExactProven)
            {
                sweep.Relation = PairRelation.Candidate;
                SetVolume(sweep, min, max, halfWidth + inflation);
            }
            else
            {
                sweep.Relation = sweep.EnvelopeSelected ? PairRelation.EnvelopeOnly : PairRelation.NoContact;
            }

            return sweep;
        }

        /// <summary>Trajectoire et ses grilles : rectangles par cap, AABB de l'union par pose, rotation de caisse par intervalle.</summary>
        private sealed class GridPath
        {
            public List<SweepPose> Poses;
            public PoseGrid[] Grids;
            public float[] Rotations;
            public PoseFrame[][] Frames;
            public PoseFrame[] Unions;
        }

        private static string GridPaths(IList<List<SweepPose>> paths, string side, KinematicOffsetBounds bounds, float gridStep,
            float halfLength, float halfWidth, out GridPath[] grids)
        {
            grids = new GridPath[paths.Count];
            for (int p = 0; p < paths.Count; p++)
            {
                PoseGrid[] poseGrids;
                float[] rotations;
                string failure = KinematicPoseSet.Build(paths[p], bounds, gridStep, out poseGrids, out rotations);
                if (failure != null)
                {
                    return "trajectoire " + side + " : " + failure;
                }

                var grid = new GridPath { Poses = paths[p], Grids = poseGrids, Rotations = rotations };
                grid.Frames = new PoseFrame[poseGrids.Length][];
                grid.Unions = new PoseFrame[poseGrids.Length];
                for (int i = 0; i < poseGrids.Length; i++)
                {
                    var headings = poseGrids[i].Headings;
                    var frames = new PoseFrame[headings.Length];
                    var union = new PoseFrame();
                    union.Min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                    union.Max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
                    for (int k = 0; k < headings.Length; k++)
                    {
                        frames[k] = Frame(KinematicPoseSet.WithHeading(paths[p][i], headings[k]), halfLength, halfWidth);
                        union.Min = Vector2.Min(union.Min, frames[k].Min);
                        union.Max = Vector2.Max(union.Max, frames[k].Max);
                    }

                    grid.Frames[i] = frames;
                    grid.Unions[i] = union;
                }

                grids[p] = grid;
            }

            return null;
        }

        private static void EvaluateGridPaths(
            GridPath a,
            GridPath b,
            float halfWidth,
            float rho,
            float inflation,
            PairSweep sweep,
            ref Vector3 min,
            ref Vector3 max)
        {
            int countA = a.Poses.Count;
            int countB = b.Poses.Count;
            int intervalsA = Math.Max(1, countA - 1);
            int intervalsB = Math.Max(1, countB - 1);
            var deltaB = new float[intervalsB];
            for (int ib = 0; ib < intervalsB; ib++)
            {
                int ib1 = Math.Min(ib + 1, countB - 1);
                deltaB[ib] = (b.Poses[ib1].Plan - b.Poses[ib].Plan).magnitude + rho * (countB > 1 ? b.Rotations[ib] : 0f);
            }

            var lineCur = new float[countB];
            var lineNext = new float[countB];
            for (int ib = 0; ib < countB; ib++)
            {
                lineCur[ib] = AabbDistance(a.Unions[0], b.Unions[ib]);
            }

            for (int ia = 0; ia < intervalsA; ia++)
            {
                int ia1 = Math.Min(ia + 1, countA - 1);
                float deltaA = (a.Poses[ia1].Plan - a.Poses[ia].Plan).magnitude + rho * (countA > 1 ? a.Rotations[ia] : 0f);
                for (int ib = 0; ib < countB; ib++)
                {
                    lineNext[ib] = AabbDistance(a.Unions[ia1], b.Unions[ib]);
                }

                for (int ib = 0; ib < intervalsB; ib++)
                {
                    int ib1 = Math.Min(ib + 1, countB - 1);
                    float bound = 0.5f * deltaA + 0.5f * deltaB[ib] + 2f * inflation;
                    float envelope = Min4(lineCur[ib], lineCur[ib1], lineNext[ib], lineNext[ib1]);
                    sweep.EnvelopeSlackMeters = Math.Min(sweep.EnvelopeSlackMeters, envelope - bound);
                    if (envelope > bound)
                    {
                        continue;
                    }

                    sweep.EnvelopeSelected = true;
                    float exact = float.PositiveInfinity;
                    int poseA = ia;
                    int poseB = ib;
                    int headingA = 0;
                    int headingB = 0;
                    GridClosest(a, ia, b, ib, ref exact, ref poseA, ref headingA, ref poseB, ref headingB);
                    GridClosest(a, ia, b, ib1, ref exact, ref poseA, ref headingA, ref poseB, ref headingB);
                    GridClosest(a, ia1, b, ib, ref exact, ref poseA, ref headingA, ref poseB, ref headingB);
                    GridClosest(a, ia1, b, ib1, ref exact, ref poseA, ref headingA, ref poseB, ref headingB);
                    float slack = exact - bound;
                    if (slack < sweep.ExactSlackMeters)
                    {
                        sweep.ExactSlackMeters = slack;
                        sweep.HasWitness = true;
                        sweep.WitnessA = KinematicPoseSet.WithHeading(a.Poses[poseA], a.Grids[poseA].Headings[headingA]);
                        sweep.WitnessB = KinematicPoseSet.WithHeading(b.Poses[poseB], b.Grids[poseB].Headings[headingB]);
                    }

                    if (slack > 0f)
                    {
                        continue;
                    }

                    sweep.ExactProven = true;
                    Encapsulate(ref min, ref max, a.Unions[ia], a.Poses[ia].Position.y, halfWidth, 0.5f * deltaA + inflation);
                    Encapsulate(ref min, ref max, a.Unions[ia1], a.Poses[ia1].Position.y, halfWidth, 0.5f * deltaA + inflation);
                    Encapsulate(ref min, ref max, b.Unions[ib], b.Poses[ib].Position.y, halfWidth, 0.5f * deltaB[ib] + inflation);
                    Encapsulate(ref min, ref max, b.Unions[ib1], b.Poses[ib1].Position.y, halfWidth, 0.5f * deltaB[ib] + inflation);
                }

                var swap = lineCur;
                lineCur = lineNext;
                lineNext = swap;
            }
        }

        /// <summary>Distance exacte minimale entre les grilles de deux poses ; un couple ne remplace le meilleur que strictement plus proche.</summary>
        private static void GridClosest(GridPath a, int ia, GridPath b, int ib, ref float best,
            ref int poseA, ref int headingA, ref int poseB, ref int headingB)
        {
            if (AabbDistance(a.Unions[ia], b.Unions[ib]) >= best)
            {
                return;
            }

            var framesA = a.Frames[ia];
            var framesB = b.Frames[ib];
            for (int ka = 0; ka < framesA.Length; ka++)
            {
                if (AabbDistance(framesA[ka], b.Unions[ib]) >= best)
                {
                    continue;
                }

                for (int kb = 0; kb < framesB.Length; kb++)
                {
                    if (AabbDistance(framesA[ka], framesB[kb]) >= best)
                    {
                        continue;
                    }

                    float distance = RectangleDistance(framesA[ka], framesB[kb]);
                    if (distance < best)
                    {
                        best = distance;
                        poseA = ia;
                        poseB = ib;
                        headingA = ka;
                        headingB = kb;
                    }
                }
            }
        }
    }
}
#endif
