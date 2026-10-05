#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    /// <summary>Issue du raffinement d'une paire (Story 5.53).</summary>
    public enum RefinementOutcome
    {
        /// <summary>Chaque feuille a une separation strictement superieure a la tolerance.</summary>
        ProvenDisjoint,

        /// <summary>Une feuille porte un temoin : deux poses reelles dont les rectangles gonfles se recouvrent.</summary>
        Witness,

        /// <summary>Feuille non subdivisable (couture, profondeur maximale) sans preuve ni temoin.</summary>
        Unresolved,

        /// <summary>Budget de feuilles epuise avant toute preuve definitive.</summary>
        BudgetExhausted
    }

    /// <summary>Resultat publie du raffinement d'une paire, deterministe pour des entrees donnees.</summary>
    public sealed class PairRefinement
    {
        public RefinementOutcome Outcome;

        /// <summary>Combinaisons de la grille du balayage dont la borne autorisait un contact : racines du raffinement.</summary>
        public int Roots;

        /// <summary>Feuilles evaluees, racines comprises.</summary>
        public int Leaves;

        /// <summary>Feuilles evaluees quand la classification est devenue definitive (premier temoin, ou fin).</summary>
        public int LeavesAtDecision;

        /// <summary>Toutes les racines resolues : budget non epuise.</summary>
        public bool Complete;

        /// <summary>Plus petite separation prouvee (combinaisons et feuilles prouvees), strictement superieure a la tolerance.</summary>
        public float MinimumSeparationMeters = float.PositiveInfinity;

        public bool HasWitness;
        public SweepPose WitnessA;
        public SweepPose WitnessB;

        /// <summary>
        /// Projection sur chaque mouvement des feuilles non prouvees (temoins et feuilles non resolues), en abscisse
        /// du mouvement dans [0, L], fusionnee et triee. Valable seulement si <see cref="Complete"/>.
        /// </summary>
        public readonly List<Vector2> ContactA = new List<Vector2>();
        public readonly List<Vector2> ContactB = new List<Vector2>();

        /// <summary>Feuilles indecises arretees a la resolution du modele (terme d'intervalle &lt;= rho . h_e).</summary>
        public int ResolutionLeaves;

        /// <summary>Resolution du modele appliquee : rho . h_e, derivee du profil et des parametres de preuve.</summary>
        public float ResolutionMeters;

        public string Canonical()
        {
            var text = new StringBuilder();
            text.Append("refinement-v1|order=").Append(ConflictSweep.RefinementOrder).Append("|resolution=")
                .Append(ConflictSweep.FormatMeters(ResolutionMeters)).Append('|').Append(ResolutionLeaves).Append('|').Append(Outcome).Append('|').Append(Roots).Append('|').Append(Leaves).Append('|')
                .Append(LeavesAtDecision).Append('|').Append(Complete ? "complete" : "incomplete").Append('|')
                .Append(ConflictSweep.FormatMeters(MinimumSeparationMeters)).Append('|')
                .Append(HasWitness ? ConflictSweep.FormatPose(WitnessA) + ";" + ConflictSweep.FormatPose(WitnessB) : "none");
            AppendIntervals(text.Append("|A"), ContactA);
            AppendIntervals(text.Append("|B"), ContactB);
            return text.ToString();
        }

        private static void AppendIntervals(StringBuilder text, List<Vector2> intervals)
        {
            foreach (var interval in intervals)
            {
                text.Append('[').Append(ConflictSweep.FormatMeters(interval.x)).Append(',').Append(ConflictSweep.FormatMeters(interval.y)).Append(']');
            }
        }
    }

    /// <summary>
    /// Typage d'une zone acceptee (Story 5.53, decision proprietaire du 2026-10-05), dans l'ordre (A, B) du raffinement.
    /// Merge seulement si la paire est ConflictProven, le raffinement complet, le corridor aval commun
    /// (CommonExitCorridor) et, sur chaque membre, le contact possible forme un seul intervalle terminal qui atteint la
    /// fin du mouvement : aucune sequence contact possible, separation, convergence. Sinon Crossing, avec les debuts de
    /// contact derives de la preuve si elle est complete, 0 (le plus precoce) sinon.
    /// </summary>
    public struct ZoneTyping
    {
        public const float EndToleranceMeters = 1e-4f;

        public ConflictKind Kind;
        public float StartA;
        public float StartB;

        public static ZoneTyping Conservative
        {
            get { return new ZoneTyping { Kind = ConflictKind.Crossing }; }
        }

        public static ZoneTyping Of(PairRefinement refinement, float lengthA, float lengthB, bool commonExitCorridor, bool conflictProven)
        {
            if (refinement == null || !refinement.Complete)
            {
                return Conservative;
            }

            bool merge = conflictProven && commonExitCorridor
                && Terminal(refinement.ContactA, lengthA) && Terminal(refinement.ContactB, lengthB);
            return new ZoneTyping
            {
                Kind = merge ? ConflictKind.Merge : ConflictKind.Crossing,
                StartA = Start(refinement.ContactA, lengthA),
                StartB = Start(refinement.ContactB, lengthB)
            };
        }

        public ZoneTyping Swapped()
        {
            return new ZoneTyping { Kind = Kind, StartA = StartB, StartB = StartA };
        }

        public string Canonical(PairRefinement refinement)
        {
            return "typing-v1|" + Kind + "|" + ConflictSweep.FormatMeters(StartA) + "|" + ConflictSweep.FormatMeters(StartB) + "|"
                + (refinement == null ? "none" : refinement.Canonical());
        }

        private static float Start(List<Vector2> contact, float length)
        {
            return contact.Count == 0 ? 0f : Mathf.Clamp(contact[0].x, 0f, length);
        }

        /// <summary>Un seul intervalle de contact possible, et il atteint la fin du mouvement.</summary>
        private static bool Terminal(List<Vector2> contact, float length)
        {
            return contact.Count == 1 && contact[0].y >= length - EndToleranceMeters;
        }
    }

    public static partial class ConflictSweep
    {
        /// <summary>Plus courte sous-portion subdivisee : en dessous, la moitie n'est plus representable sans ambiguite.</summary>
        public const float MinimumRefinedLengthMeters = 1e-4f;

        /// <summary>Ordre de subdivision v3 : dyadique, cote au plus grand terme d'intervalle d'abord, A a egalite exacte.</summary>
        public const string RefinementOrder = "dyadic-larger-delta-first-tie-A";

        /// <summary>
        /// Resolution du modele (decision proprietaire du 2026-10-05) : rho . h_e. La borne d'une feuille est
        /// delta_A/2 + delta_B/2 + 2 (gonflement de base + rho . h_e / 2). Le second terme, 2 . rho . h_e / 2 = rho . h_e, vient
        /// de la grille des caps : tout cap atteignable est a h_e/2 d'un cap de grille, donc toute empreinte a rho . h_e / 2
        /// d'une empreinte de grille, de chaque cote. Il ne depend pas de l'intervalle et aucune subdivision ne le reduit.
        /// Quand le terme d'intervalle, le seul que la subdivision reduit, ne depasse plus ce terme irreductible, la feuille
        /// est decrite a la resolution meme du modele : la couper encore ne distingue plus rien que la grille de caps
        /// puisse representer. C'est une limite de resolution du modele, pas un seuil de performance. La feuille reste
        /// indecise (contact possible) et ne devient jamais disjointe.
        /// </summary>
        public static float RefinementResolution(RoadModelValidationProfile profile, GateAEvidenceParameters parameters)
        {
            return 2f * OffsetGridRemainder(profile, parameters);
        }

        /// <summary>
        /// Raffinement borne (Story 5.53, politique v3) d'une paire candidate, sur l'ensemble de poses cinematique.
        /// Les combinaisons dont la borne de la grille autorise un contact sont subdivisees de facon dyadique, cote au
        /// plus grand delta d'abord, dans la machinerie du balayage inchangee : meme gonflement, memes grilles de caps
        /// (<see cref="KinematicPoseSet.Build"/> sur la sous-portion), meme borne d'intervalle et meme distance exacte.
        /// Une feuille est prouvee si sa separation depasse strictement <paramref name="tolerance"/> ; elle porte un
        /// temoin si deux de ses poses reelles se recouvrent, gonflees hors restes (tolerance de la politique). Le
        /// raffinement continue apres un temoin pour localiser le contact (typage), dans le meme budget.
        /// </summary>
        public static PairRefinement Refine(
            SweepGraph graph,
            RoadId movementA,
            RoadId movementB,
            IList<List<SweepPose>> pathsA,
            IList<List<SweepPose>> pathsB,
            RoadModelValidationProfile profile,
            GateAEvidenceParameters parameters,
            KinematicOffsetBounds bounds,
            float tolerance,
            int maxDepth,
            int leafBudget)
        {
            if (parameters == null || !parameters.Kinematic || bounds == null || graph == null)
            {
                throw new ArgumentException("Raffinement : parametres cinematiques, bornes et graphe requis.");
            }

            var result = new PairRefinement();
            var context = new RefineContext
            {
                Graph = graph,
                Profile = profile,
                Bounds = bounds,
                GridStep = parameters.OffsetGridStepRadians,
                HalfLength = HalfLength(profile),
                HalfWidth = profile.MaxVehicleHalfWidthMeters,
                Rho = Rho(profile),
                BaseInflation = Inflation(profile) + parameters.TrackingAllowanceMeters,
                Tolerance = tolerance,
                MaxDepth = maxDepth,
                Budget = leafBudget,
                Result = result
            };
            context.Inflation = context.BaseInflation + OffsetGridRemainder(profile, parameters);
            context.Resolution = RefinementResolution(profile, parameters);
            result.ResolutionMeters = context.Resolution;

            var sortedA = Canonical(pathsA);
            var sortedB = Canonical(pathsB);
            GridPath[] gridsA = null;
            GridPath[] gridsB = null;
            string failure = GridPaths(sortedA, "A", bounds, context.GridStep, context.HalfLength, context.HalfWidth, out gridsA)
                ?? GridPaths(sortedB, "B", bounds, context.GridStep, context.HalfLength, context.HalfWidth, out gridsB);
            if (failure != null)
            {
                // Hors hypotheses : le balayage aurait deja echoue ferme ; rien n'est prouve.
                result.Outcome = RefinementOutcome.Unresolved;
                return result;
            }

            var sidesA = new MovementSide[sortedA.Count];
            for (int p = 0; p < sortedA.Count; p++) sidesA[p] = MovementSide.Of(graph, movementA, sortedA[p]);
            var sidesB = new MovementSide[sortedB.Count];
            for (int p = 0; p < sortedB.Count; p++) sidesB[p] = MovementSide.Of(graph, movementB, sortedB[p]);

            var roots = new List<RefineNode>();
            for (int pa = 0; pa < gridsA.Length; pa++)
            {
                for (int pb = 0; pb < gridsB.Length; pb++)
                {
                    Screen(gridsA[pa], gridsB[pb], context, roots, sidesA[pa], sidesB[pb]);
                }
            }

            result.Roots = roots.Count;
            bool unresolved = false;
            var stack = new Stack<RefineNode>();
            for (int r = 0; r < roots.Count && !context.Exhausted; r++)
            {
                stack.Push(roots[r]);
                while (stack.Count > 0)
                {
                    if (result.Leaves >= context.Budget)
                    {
                        context.Exhausted = true;
                        break;
                    }

                    RefineNode node = stack.Pop();
                    result.Leaves++;
                    float deltaA;
                    float deltaB;
                    LeafState state = EvaluateLeaf(node, context, out deltaA, out deltaB);
                    if (state == LeafState.Proven)
                    {
                        continue;
                    }

                    RefineNode first;
                    RefineNode second;
                    if (state != LeafState.Witness && state != LeafState.Unresolved && TrySplit(node, context, deltaA, deltaB, out first, out second))
                    {
                        stack.Push(second);
                        stack.Push(first);
                        continue;
                    }

                    // Feuille terminale non prouvee : contact possible, publie ; sans temoin, elle reste non resolue.
                    unresolved |= state != LeafState.Witness && state != LeafState.WitnessSplit;
                    Project(node, context);
                }
            }

            result.Complete = !context.Exhausted;
            if (result.HasWitness)
            {
                result.Outcome = RefinementOutcome.Witness;
            }
            else
            {
                result.LeavesAtDecision = result.Leaves;
                result.Outcome = context.Exhausted ? RefinementOutcome.BudgetExhausted
                    : unresolved ? RefinementOutcome.Unresolved
                    : RefinementOutcome.ProvenDisjoint;
            }

            if (result.Complete)
            {
                Merge(context.RawA, result.ContactA);
                Merge(context.RawB, result.ContactB);
            }

            return result;
        }

        // ============================================================ etat

        /// <summary>
        /// Proven : separation stricte. Witness : temoin sur une feuille assez fine, terminale. WitnessSplit : temoin sur une
        /// feuille encore large, subdivisee pour localiser le contact (typage). Split : indecise. Unresolved : grilles indisponibles.
        /// </summary>
        private enum LeafState { Proven, Witness, WitnessSplit, Split, Unresolved }

        private sealed class RefineContext
        {
            public SweepGraph Graph;
            public RoadModelValidationProfile Profile;
            public KinematicOffsetBounds Bounds;
            public float GridStep;
            public float HalfLength;
            public float HalfWidth;
            public float Rho;
            public float BaseInflation;
            public float Inflation;
            public float Resolution;
            public float Tolerance;
            public int MaxDepth;
            public int Budget;
            public bool Exhausted;
            public PairRefinement Result;
            public readonly List<Vector2> RawA = new List<Vector2>();
            public readonly List<Vector2> RawB = new List<Vector2>();
        }

        /// <summary>Sous-portion d'une combinaison : deux poses d'extremite sur une trajectoire, et sa place sur le mouvement.</summary>
        private struct RefineSegment
        {
            public SweepPose P0;
            public SweepPose P1;
            public MovementSide Side;

            /// <summary>Intervalle de la trajectoire d'origine : situe une pose hors mouvement (avant ou apres).</summary>
            public int Interval;
        }

        private struct RefineNode
        {
            public RefineSegment A;
            public RefineSegment B;
            public int Depth;
        }

        /// <summary>Position du mouvement dans une trajectoire prolongee : ses poses, son origine et sa longueur.</summary>
        private struct MovementSide
        {
            public RoadId Movement;
            public int FirstIndex;
            public float StartS;
            public float Length;

            public static MovementSide Of(SweepGraph graph, RoadId movement, List<SweepPose> path)
            {
                var side = new MovementSide { Movement = movement, FirstIndex = path.Count };
                for (int i = 0; i < path.Count; i++)
                {
                    if (path[i].ElementId == movement)
                    {
                        side.FirstIndex = i;
                        break;
                    }
                }

                SweepElement element;
                if (graph.Elements.TryGetValue(movement, out element))
                {
                    side.StartS = element.StartS;
                    side.Length = element.Length;
                }

                return side;
            }

            /// <summary>Abscisse de la pose sur le mouvement : 0 avant lui, L apres lui.</summary>
            public float Abscissa(SweepPose pose, int interval)
            {
                if (pose.ElementId == Movement)
                {
                    return Mathf.Clamp(pose.SMeters - StartS, 0f, Length);
                }

                return interval < FirstIndex ? 0f : Length;
            }
        }

        // ============================================================ criblage

        /// <summary>Meme borne que le balayage cinematique ; chaque combinaison non prouvee devient une racine.</summary>
        private static void Screen(GridPath a, GridPath b, RefineContext context, List<RefineNode> roots, MovementSide sideA, MovementSide sideB)
        {
            int countA = a.Poses.Count;
            int countB = b.Poses.Count;
            int intervalsA = Math.Max(1, countA - 1);
            int intervalsB = Math.Max(1, countB - 1);
            for (int ia = 0; ia < intervalsA; ia++)
            {
                int ia1 = Math.Min(ia + 1, countA - 1);
                float deltaA = (a.Poses[ia1].Plan - a.Poses[ia].Plan).magnitude + context.Rho * (countA > 1 ? a.Rotations[ia] : 0f);
                for (int ib = 0; ib < intervalsB; ib++)
                {
                    int ib1 = Math.Min(ib + 1, countB - 1);
                    float deltaB = (b.Poses[ib1].Plan - b.Poses[ib].Plan).magnitude + context.Rho * (countB > 1 ? b.Rotations[ib] : 0f);
                    float bound = 0.5f * deltaA + 0.5f * deltaB + 2f * context.Inflation;
                    float envelope = Min4(AabbDistance(a.Unions[ia], b.Unions[ib]), AabbDistance(a.Unions[ia], b.Unions[ib1]),
                        AabbDistance(a.Unions[ia1], b.Unions[ib]), AabbDistance(a.Unions[ia1], b.Unions[ib1]));
                    float slack = envelope - bound;
                    if (!(slack > context.Tolerance))
                    {
                        float exact = float.PositiveInfinity;
                        int poseA = ia;
                        int poseB = ib;
                        int headingA = 0;
                        int headingB = 0;
                        GridClosest(a, ia, b, ib, ref exact, ref poseA, ref headingA, ref poseB, ref headingB);
                        GridClosest(a, ia, b, ib1, ref exact, ref poseA, ref headingA, ref poseB, ref headingB);
                        GridClosest(a, ia1, b, ib, ref exact, ref poseA, ref headingA, ref poseB, ref headingB);
                        GridClosest(a, ia1, b, ib1, ref exact, ref poseA, ref headingA, ref poseB, ref headingB);
                        slack = exact - bound;
                    }

                    if (slack > context.Tolerance)
                    {
                        context.Result.MinimumSeparationMeters = Math.Min(context.Result.MinimumSeparationMeters, slack);
                        continue;
                    }

                    roots.Add(new RefineNode
                    {
                        A = new RefineSegment { P0 = a.Poses[ia], P1 = a.Poses[ia1], Side = sideA, Interval = ia },
                        B = new RefineSegment { P0 = b.Poses[ib], P1 = b.Poses[ib1], Side = sideB, Interval = ib },
                        Depth = 0
                    });
                }
            }
        }

        // ============================================================ feuilles

        private static LeafState EvaluateLeaf(RefineNode node, RefineContext context, out float deltaA, out float deltaB)
        {
            deltaA = 0f;
            deltaB = 0f;
            GridPath a;
            GridPath b;
            if (!TryGrid(node.A, context, out a) || !TryGrid(node.B, context, out b))
            {
                return LeafState.Unresolved;
            }

            deltaA = SegmentDelta(a, context.Rho);
            deltaB = SegmentDelta(b, context.Rho);
            float bound = 0.5f * deltaA + 0.5f * deltaB + 2f * context.Inflation;
            float envelope = Min4(AabbDistance(a.Unions[0], b.Unions[0]), AabbDistance(a.Unions[0], b.Unions[1]),
                AabbDistance(a.Unions[1], b.Unions[0]), AabbDistance(a.Unions[1], b.Unions[1]));
            if (envelope - bound > context.Tolerance)
            {
                context.Result.MinimumSeparationMeters = Math.Min(context.Result.MinimumSeparationMeters, envelope - bound);
                return LeafState.Proven;
            }

            float exact = float.PositiveInfinity;
            int poseA = 0;
            int poseB = 0;
            int headingA = 0;
            int headingB = 0;
            GridClosest(a, 0, b, 0, ref exact, ref poseA, ref headingA, ref poseB, ref headingB);
            GridClosest(a, 0, b, 1, ref exact, ref poseA, ref headingA, ref poseB, ref headingB);
            GridClosest(a, 1, b, 0, ref exact, ref poseA, ref headingA, ref poseB, ref headingB);
            GridClosest(a, 1, b, 1, ref exact, ref poseA, ref headingA, ref poseB, ref headingB);
            float slack = exact - bound;
            if (slack > context.Tolerance)
            {
                context.Result.MinimumSeparationMeters = Math.Min(context.Result.MinimumSeparationMeters, slack);
                return LeafState.Proven;
            }

            // Temoin : poses reelles gonflees hors restes, meme critere et meme tolerance que le temoin de la politique.
            if (exact <= 2f * context.BaseInflation + AutomatedPairDecisionPolicy.ProofToleranceMeters)
            {
                var result = context.Result;
                if (!result.HasWitness)
                {
                    result.HasWitness = true;
                    result.LeavesAtDecision = result.Leaves;
                    result.WitnessA = KinematicPoseSet.WithHeading(a.Poses[poseA], a.Grids[poseA].Headings[headingA]);
                    result.WitnessB = KinematicPoseSet.WithHeading(b.Poses[poseB], b.Grids[poseB].Headings[headingB]);
                }

                // La classification est acquise ; une feuille dont le terme d'intervalle depasse encore le gonflement
                // localise mal le contact : elle est subdivisee pour le typage, dans le meme budget.
                return 0.5f * deltaA + 0.5f * deltaB > 2f * context.Inflation ? LeafState.WitnessSplit : LeafState.Witness;
            }

            // Resolution du modele atteinte : la feuille reste indecise, terminale (voir RefinementResolution).
            if (0.5f * deltaA + 0.5f * deltaB <= context.Resolution)
            {
                context.Result.ResolutionLeaves++;
                return LeafState.Unresolved;
            }

            return LeafState.Split;
        }

        /// <summary>Grilles de la sous-portion par la machinerie du balayage : deux poses, une rotation de caisse.</summary>
        private static bool TryGrid(RefineSegment segment, RefineContext context, out GridPath grid)
        {
            var path = new List<SweepPose>(2) { segment.P0, segment.P1 };
            GridPath[] grids;
            string failure = GridPaths(new List<List<SweepPose>> { path }, "raffinement", context.Bounds, context.GridStep,
                context.HalfLength, context.HalfWidth, out grids);
            grid = failure == null ? grids[0] : null;
            return grid != null;
        }

        private static float SegmentDelta(GridPath grid, float rho)
        {
            return (grid.Poses[1].Plan - grid.Poses[0].Plan).magnitude + rho * grid.Rotations[0];
        }

        /// <summary>
        /// Subdivision dyadique du cote dont le terme d'intervalle delta est le plus grand (A a egalite), l'autre cote si le
        /// premier ne se coupe pas : le cout ne depend pas de l'ordre des membres.
        /// </summary>
        private static bool TrySplit(RefineNode node, RefineContext context, float deltaA, float deltaB, out RefineNode first, out RefineNode second)
        {
            first = node;
            second = node;
            if (node.Depth >= context.MaxDepth)
            {
                return false;
            }

            bool splitA = Splittable(node.A);
            bool splitB = Splittable(node.B);
            bool useA = splitA && (deltaA >= deltaB || !splitB);
            if (!useA && !splitB)
            {
                return false;
            }

            RefineSegment left;
            RefineSegment right;
            Halve(useA ? node.A : node.B, context.Graph, out left, out right);
            first.Depth = node.Depth + 1;
            second.Depth = node.Depth + 1;
            if (useA)
            {
                first.A = left;
                second.A = right;
            }
            else
            {
                first.B = left;
                second.B = right;
            }

            return true;
        }

        /// <summary>Une couture n'est jamais interpolee ; une portion trop courte n'est plus coupee.</summary>
        private static bool Splittable(RefineSegment segment)
        {
            return segment.P0.ElementId == segment.P1.ElementId && segment.P1.SMeters - segment.P0.SMeters > MinimumRefinedLengthMeters;
        }

        /// <summary>Milieu en abscisse, evalue par l'interpolation canonique de l'element (comme les coupes de portee).</summary>
        private static void Halve(RefineSegment segment, SweepGraph graph, out RefineSegment left, out RefineSegment right)
        {
            SweepElement element = graph.Elements[segment.P0.ElementId];
            float s = 0.5f * (segment.P0.SMeters + segment.P1.SMeters);
            SweepPose middle = SamplePose(element, s);
            left = segment;
            right = segment;
            left.P1 = middle;
            right.P0 = middle;
        }

        // ============================================================ projection

        private static void Project(RefineNode node, RefineContext context)
        {
            context.RawA.Add(Interval(node.A));
            context.RawB.Add(Interval(node.B));
        }

        private static Vector2 Interval(RefineSegment segment)
        {
            float s0 = segment.Side.Abscissa(segment.P0, segment.Interval);
            float s1 = segment.Side.Abscissa(segment.P1, segment.Interval);
            return new Vector2(Math.Min(s0, s1), Math.Max(s0, s1));
        }

        /// <summary>Union triee d'intervalles fermes ; deux intervalles qui se touchent forment un seul intervalle.</summary>
        private static void Merge(List<Vector2> raw, List<Vector2> merged)
        {
            raw.Sort(delegate(Vector2 x, Vector2 y)
            {
                int c = x.x.CompareTo(y.x);
                return c != 0 ? c : x.y.CompareTo(y.y);
            });
            foreach (var interval in raw)
            {
                if (merged.Count > 0 && interval.x <= merged[merged.Count - 1].y)
                {
                    var last = merged[merged.Count - 1];
                    merged[merged.Count - 1] = new Vector2(last.x, Math.Max(last.y, interval.y));
                    continue;
                }

                merged.Add(interval);
            }
        }

        /// <summary>Ordre canonique des trajectoires : le resultat ne depend pas de l'ordre d'enumeration des branches.</summary>
        private static List<List<SweepPose>> Canonical(IList<List<SweepPose>> paths)
        {
            var keyed = new List<KeyValuePair<string, List<SweepPose>>>();
            foreach (var path in paths)
            {
                var key = new StringBuilder();
                foreach (var pose in path)
                {
                    key.Append(pose.ElementId).Append('@').Append(FormatMeters(pose.SMeters)).Append(';');
                }

                keyed.Add(new KeyValuePair<string, List<SweepPose>>(key.ToString(), path));
            }

            keyed.Sort(delegate(KeyValuePair<string, List<SweepPose>> x, KeyValuePair<string, List<SweepPose>> y)
            {
                return string.CompareOrdinal(x.Key, y.Key);
            });
            return keyed.ConvertAll(delegate(KeyValuePair<string, List<SweepPose>> entry) { return entry.Value; });
        }

        internal static string FormatMeters(float value)
        {
            if (float.IsPositiveInfinity(value)) return "+inf";
            if (float.IsNegativeInfinity(value)) return "-inf";
            if (float.IsNaN(value)) return "nan";
            return value.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
        }

        internal static string FormatPose(SweepPose pose)
        {
            return FormatMeters(pose.Position.x) + "," + FormatMeters(pose.Position.y) + "," + FormatMeters(pose.Position.z)
                + "," + FormatMeters(pose.Heading.x) + "," + FormatMeters(pose.Heading.y);
        }
    }
}
#endif
