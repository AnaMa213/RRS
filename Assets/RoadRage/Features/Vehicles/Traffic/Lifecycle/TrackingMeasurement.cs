using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RoadRage.Features.Vehicles.Traffic.Routing;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Lifecycle
{
    /// <summary>
    /// Boite du gabarit max (5.31) : L = MaxVehicleLengthMeters, demi-largeur W, hauteur du collider,
    /// centree horizontalement sur le point de reference (x droite, y haut, z avant).
    /// </summary>
    public readonly struct GaugeBox
    {
        public readonly float HalfWidthMeters;
        public readonly float LengthMeters;
        public readonly float BottomMeters;
        public readonly float TopMeters;

        public GaugeBox(float halfWidthMeters, float lengthMeters, float bottomMeters, float topMeters)
        { HalfWidthMeters = halfWidthMeters; LengthMeters = lengthMeters; BottomMeters = bottomMeters; TopMeters = topMeters; }

        /// <summary>Coin i (0..7) dans le repere du point de reference.</summary>
        public Vector3 Corner(int i)
        {
            return new Vector3((i & 1) == 0 ? -HalfWidthMeters : HalfWidthMeters,
                (i & 2) == 0 ? BottomMeters : TopMeters,
                (i & 4) == 0 ? -LengthMeters * 0.5f : LengthMeters * 0.5f);
        }

        /// <summary>rho = racine((L/2)^2 + W^2) : rayon de rotation d'un coin autour de road-up.</summary>
        public float Rho { get { return Mathf.Sqrt(LengthMeters * LengthMeters * 0.25f + HalfWidthMeters * HalfWidthMeters); } }

        /// <summary>Vrai si la boite (axes du point de reference) contient la boite donnee.</summary>
        public bool Contains(Vector3 center, Vector3 size, float toleranceMeters)
        {
            return Math.Abs(center.x) + size.x * 0.5f <= HalfWidthMeters + toleranceMeters
                && Math.Abs(center.z) + size.z * 0.5f <= LengthMeters * 0.5f + toleranceMeters
                && center.y - size.y * 0.5f >= BottomMeters - toleranceMeters
                && center.y + size.y * 0.5f <= TopMeters + toleranceMeters;
        }
    }

    /// <summary>Etat d'un corps a un pas physique : pose du point de reference, centre de masse et vitesses de fin de pas.</summary>
    public readonly struct BodyState
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 CenterOfMass;
        public readonly Vector3 LinearVelocity;
        public readonly Vector3 AngularVelocity;

        public BodyState(Vector3 position, Quaternion rotation, Vector3 centerOfMass, Vector3 linearVelocity,
            Vector3 angularVelocity)
        {
            Position = position; Rotation = rotation; CenterOfMass = centerOfMass;
            LinearVelocity = linearVelocity; AngularVelocity = angularVelocity;
        }
    }

    /// <summary>Pose nominale droite : position de reference, tangente et road-up (ni roulis ni tangage).</summary>
    public readonly struct NominalPose
    {
        public readonly Vector3 Position;
        public readonly Vector3 Forward;
        public readonly Vector3 Up;

        public NominalPose(Vector3 position, Vector3 forward, Vector3 up) { Position = position; Forward = forward; Up = up; }
    }

    /// <summary>Un morceau de la reference : une occurrence de route sur sa courbe compilee.</summary>
    public sealed class TrackPiece
    {
        public RoadElementKind Kind { get; }
        public RoadId Id { get; }
        public float StartDistanceMeters { get; }
        public float EndDistanceMeters { get; }
        public float ElementStartSMeters { get; }
        public RoadCurve Curve { get; }
        private readonly float[] sampleS;
        private readonly float[] segmentRate;

        public TrackPiece(RoadElementKind kind, RoadId id, float startDistance, float elementStartS, float elementEndS,
            IReadOnlyList<RoadCurveSample> samples)
        {
            Kind = kind; Id = id; StartDistanceMeters = startDistance;
            EndDistanceMeters = startDistance + (elementEndS - elementStartS);
            ElementStartSMeters = elementStartS;
            Curve = new RoadCurve(samples);
            sampleS = new float[samples.Count];
            segmentRate = new float[Math.Max(0, samples.Count - 1)];
            for (int i = 0; i < samples.Count; i++) sampleS[i] = samples[i].SMeters;
            for (int i = 0; i + 1 < samples.Count; i++)
            {
                // Taux de rotation de la tangente normalisee-interpolee : maximum 2 tan(alpha/2) / ds au milieu.
                float alpha = Vector3.Angle(samples[i].Tangent, samples[i + 1].Tangent) * Mathf.Deg2Rad;
                float ds = samples[i + 1].SMeters - samples[i].SMeters;
                segmentRate[i] = ds > 0f ? 2f * Mathf.Tan(alpha * 0.5f) / ds : 0f;
            }
        }

        public float ElementS(float distance) { return ElementStartSMeters + (distance - StartDistanceMeters); }

        public NominalPose Nominal(float distance)
        {
            var point = Curve.Sample(ElementS(distance));
            Vector3 up = point.Up.normalized;
            Vector3 forward = Vector3.ProjectOnPlane(point.Tangent, up).normalized;
            return new NominalPose(point.Position, forward, up);
        }

        /// <summary>psi'_max sur les segments compiles touches par [d0, d1].</summary>
        public float TangentRateMax(float d0, float d1)
        {
            float s0 = ElementS(Math.Min(d0, d1)), s1 = ElementS(Math.Max(d0, d1));
            float result = 0f;
            for (int i = 0; i < segmentRate.Length; i++)
                if (sampleS[i + 1] >= s0 && sampleS[i] <= s1) result = Math.Max(result, segmentRate[i]);
            return result;
        }
    }

    /// <summary>
    /// La reference compilee d'une route, en distance de route (horizon complet), avec la pose nominale
    /// cinematique propre a la route (contrat Road World Model §8, correct-course du 2026-09-29) : la caisse
    /// fait avec la tangente l'ecart e, solution de de/ds = kappa(s) - sin(e)/a (point de reference a a devant
    /// l'essieu arriere non directeur, sans derive a l'essieu arriere). e part de sa valeur initiale au debut
    /// de la route (0 a l'insertion au portail), saute du saut de tangente signe a chaque raccord (cap de caisse
    /// continu) et n'est jamais remis a zero ailleurs. Sans profil (a = 0), la pose nominale reste tangente.
    /// </summary>
    public sealed class ReferenceTrack
    {
        /// <summary>Pas d'integration RK4 de l'ecart, en metres de route.</summary>
        public const float OffsetStepMeters = 0.02f;

        public IReadOnlyList<TrackPiece> Pieces { get; }
        public float LengthMeters { get { return Pieces.Count == 0 ? 0f : Pieces[Pieces.Count - 1].EndDistanceMeters; } }

        /// <summary>a, distance du point de reference en avant de l'essieu arriere ; 0 : pose tangente.</summary>
        public float ReferenceAheadRearAxleMeters { get; }
        public bool HasKinematicPose { get { return ReferenceAheadRearAxleMeters > 0f; } }

        // Par morceau : distances de route et ecart e (radians) aux noeuds d'integration, bornes comprises.
        private readonly float[][] offsetDistance;
        private readonly float[][] offsetRadians;

        public ReferenceTrack(IReadOnlyList<TrackPiece> pieces) : this(pieces, 0f, 0f) { }

        public ReferenceTrack(IReadOnlyList<TrackPiece> pieces, float referenceAheadRearAxleMeters, float initialOffsetRadians)
        {
            if (pieces == null || pieces.Count == 0) throw new ArgumentException("EmptyTrack", "pieces");
            if (float.IsNaN(referenceAheadRearAxleMeters) || referenceAheadRearAxleMeters < 0f)
                throw new ArgumentOutOfRangeException("referenceAheadRearAxleMeters");
            Pieces = new List<TrackPiece>(pieces).AsReadOnly();
            ReferenceAheadRearAxleMeters = referenceAheadRearAxleMeters;
            offsetDistance = new float[Pieces.Count][];
            offsetRadians = new float[Pieces.Count][];
            if (!HasKinematicPose) return;

            float a = referenceAheadRearAxleMeters;
            double e = initialOffsetRadians;
            for (int p = 0; p < Pieces.Count; p++)
            {
                var piece = Pieces[p];
                if (p > 0) e += SeamTangentJumpRadians(Pieces[p - 1], piece);
                float span = piece.EndDistanceMeters - piece.StartDistanceMeters;
                int steps = Math.Max(1, (int)Math.Ceiling(span / OffsetStepMeters));
                double h = span / (double)steps;
                var ds = new float[steps + 1];
                var es = new float[steps + 1];
                ds[0] = piece.StartDistanceMeters; es[0] = (float)e;
                for (int i = 0; i < steps; i++)
                {
                    double d = piece.StartDistanceMeters + h * i;
                    double k1 = Rate(piece, d, e, a);
                    double k2 = Rate(piece, d + h * 0.5, e + h * 0.5 * k1, a);
                    double k3 = Rate(piece, d + h * 0.5, e + h * 0.5 * k2, a);
                    double k4 = Rate(piece, d + h, e + h * k3, a);
                    e += h / 6.0 * (k1 + 2.0 * k2 + 2.0 * k3 + k4);
                    ds[i + 1] = (float)(d + h); es[i + 1] = (float)e;
                }
                ds[steps] = piece.EndDistanceMeters;
                offsetDistance[p] = ds;
                offsetRadians[p] = es;
            }
        }

        private static double Rate(TrackPiece piece, double distance, double e, float a)
        {
            float kappa = piece.Curve.Sample(piece.ElementS((float)distance)).CurvaturePerMeter;
            return kappa - Math.Sin(e) / a;
        }

        /// <summary>Saut de tangente signe (radians, positif vers road-right) entre la fin de left et le debut de right.</summary>
        public static float SeamTangentJumpRadians(TrackPiece left, TrackPiece right)
        {
            return (float)RoadCurve.SignedTangentJumpRadians(left.Curve.Sample(left.ElementS(left.EndDistanceMeters)),
                right.Curve.Sample(right.ElementS(right.StartDistanceMeters)));
        }

        /// <summary>Ecart nominal e (radians) du morceau a la distance de route ; 0 sans profil cinematique.</summary>
        public float OffsetRadians(int piece, float distance)
        {
            if (!HasKinematicPose) return 0f;
            var ds = offsetDistance[piece];
            var es = offsetRadians[piece];
            if (distance <= ds[0]) return es[0];
            if (distance >= ds[ds.Length - 1]) return es[es.Length - 1];
            int low = 0, high = ds.Length - 1;
            while (high - low > 1)
            {
                int mid = (low + high) / 2;
                if (ds[mid] <= distance) low = mid; else high = mid;
            }
            float t = (distance - ds[low]) / (ds[high] - ds[low]);
            return es[low] + t * (es[high] - es[low]);
        }

        /// <summary>
        /// Ancres cinematiques des morceaux [piece - 1, piece + 2] : e au debut de chaque morceau, d'ou la
        /// localisation transporte l'orientation attendue des candidats autour du vehicule (branches comprises).
        /// Nul sans pose cinematique (la localisation garde alors la pose tangente).
        /// </summary>
        public RoadKinematicAnchor[] KinematicAnchors(int piece)
        {
            if (!HasKinematicPose) return null;
            int from = Math.Max(0, piece - 1), to = Math.Min(Pieces.Count - 1, piece + 2);
            var anchors = new RoadKinematicAnchor[to - from + 1];
            for (int p = from; p <= to; p++)
                anchors[p - from] = new RoadKinematicAnchor(Pieces[p].Id, Pieces[p].ElementStartSMeters,
                    OffsetRadians(p, Pieces[p].StartDistanceMeters));
            return anchors;
        }

        /// <summary>Ecart de cap signe nominal (convention de localisation, degres) : -e.</summary>
        public float NominalHeadingErrorDegrees(int piece, float distance)
        {
            return -OffsetRadians(piece, distance) * Mathf.Rad2Deg;
        }

        /// <summary>Pose nominale cinematique : position de reference, cap = tangente tournee de -e autour de road-up.</summary>
        public NominalPose Nominal(int piece, float distance)
        {
            var pose = Pieces[piece].Nominal(distance);
            if (!HasKinematicPose) return pose;
            Vector3 forward = Quaternion.AngleAxis(-OffsetRadians(piece, distance) * Mathf.Rad2Deg, pose.Up) * pose.Forward;
            return new NominalPose(pose.Position, forward, pose.Up);
        }

        /// <summary>
        /// Taux de rotation de la caisse nominale sur [d0, d1] du morceau : max |sin e| / a (contrat §8) ; sans
        /// profil cinematique, le taux de la tangente normalisee-interpolee.
        /// </summary>
        public float BodyRateMax(int piece, float d0, float d1)
        {
            if (!HasKinematicPose) return Pieces[piece].TangentRateMax(d0, d1);
            float low = Math.Min(d0, d1), high = Math.Max(d0, d1);
            float result = Math.Max(Math.Abs((float)Math.Sin(OffsetRadians(piece, low))),
                Math.Abs((float)Math.Sin(OffsetRadians(piece, high))));
            var ds = offsetDistance[piece];
            var es = offsetRadians[piece];
            for (int i = 0; i < ds.Length; i++)
                if (ds[i] >= low && ds[i] <= high) result = Math.Max(result, Math.Abs((float)Math.Sin(es[i])));
            return result / ReferenceAheadRearAxleMeters;
        }

        public static ReferenceTrack FromRoute(CompiledRoadModel model, IReadOnlyList<RouteOccurrence> occurrences)
        {
            return FromRoute(model, occurrences, 0f);
        }

        /// <summary>
        /// Reference d'une route avec la pose nominale cinematique du profil declare. initialOffsetRadians : 0 a
        /// l'insertion au portail ; lors d'un replan, l'ecart courant de la route precedente (jamais remis a zero).
        /// </summary>
        public static ReferenceTrack FromRoute(CompiledRoadModel model, IReadOnlyList<RouteOccurrence> occurrences,
            float initialOffsetRadians)
        {
            var pieces = new List<TrackPiece>();
            float distance = 0f;
            foreach (var occurrence in occurrences)
            {
                IReadOnlyList<RoadCurveSample> samples;
                EffectiveLaneCorridor corridor;
                CompiledJunctionMovement movement;
                if (occurrence.Kind == RoadElementKind.LaneCorridor && model.TryGetCorridor(occurrence.Id, out corridor))
                    samples = corridor.Samples;
                else if (occurrence.Kind == RoadElementKind.JunctionMovement && model.TryGetMovement(occurrence.Id, out movement))
                    samples = movement.Samples;
                else throw new ArgumentException("MissingElement", "occurrences");
                pieces.Add(new TrackPiece(occurrence.Kind, occurrence.Id, distance, occurrence.StartSMeters,
                    occurrence.EndSMeters, samples));
                distance += occurrence.EndSMeters - occurrence.StartSMeters;
            }
            var drivability = model.DrivabilityProfile;
            float a = drivability.Declared && drivability.ReferencePointAheadRearAxleMeters > 0f
                ? drivability.ReferencePointAheadRearAxleMeters : 0f;
            return new ReferenceTrack(pieces, a, initialOffsetRadians);
        }

        /// <summary>Morceau qui contient la distance ; a un raccord, le morceau de droite.</summary>
        public int PieceAt(float distance)
        {
            for (int i = 0; i < Pieces.Count; i++)
                if (distance < Pieces[i].EndDistanceMeters) return i;
            return Pieces.Count - 1;
        }

        /// <summary>
        /// s* : projection du point de reference sur la reference, en distance de route, sur le morceau
        /// indique et ses deux voisins (le plus proche l'emporte, la plus petite distance a egalite).
        /// </summary>
        public float Project(Vector3 point, int hint, out int piece)
        {
            piece = Mathf.Clamp(hint, 0, Pieces.Count - 1);
            float best = float.PositiveInfinity, result = Pieces[piece].StartDistanceMeters;
            int chosen = piece;
            for (int i = Math.Max(0, piece - 1); i <= Math.Min(Pieces.Count - 1, piece + 1); i++)
            {
                var candidate = Pieces[i];
                float s0 = candidate.ElementStartSMeters;
                float s1 = candidate.ElementS(candidate.EndDistanceMeters);
                var projection = candidate.Curve.Project(point, s0, s1);
                float distance = projection.DistanceMeters + projection.LongitudinalOverrunMeters;
                if (distance < best)
                {
                    best = distance; chosen = i;
                    result = candidate.StartDistanceMeters + (projection.SMeters - s0);
                }
            }
            piece = chosen;
            return result;
        }
    }

    public readonly struct PieceBound
    {
        public readonly int Piece;
        public readonly float GridMaximumMeters;
        public readonly float BoundMeters;
        public readonly float StartDisplacementMeters;
        public readonly float EndDisplacementMeters;

        public PieceBound(int piece, float gridMaximum, float bound, float start, float end)
        { Piece = piece; GridMaximumMeters = gridMaximum; BoundMeters = bound; StartDisplacementMeters = start; EndDisplacementMeters = end; }
    }

    public sealed class InterStepResult
    {
        public bool ModelVerified { get; internal set; }
        public float PositionResidualMeters { get; internal set; }
        public float RotationResidualDegrees { get; internal set; }
        public float BoundMeters { get; internal set; }
        public float LipschitzMeters { get; internal set; }
        public IReadOnlyList<PieceBound> Pieces { get; internal set; }
    }

    /// <summary>
    /// Mesure du deplacement du gabarit (5.31, A2). Au pas : d = max sur les 8 coins de la projection au
    /// sol de (coin reel - coin nominal) ; exacte. Entre deux pas, sous le modele M (corps anime des
    /// vitesses de fin de pas, verifie par pas) : progression appariee, decoupe aux raccords, sous-grille
    /// et reste de Lipschitz L h / 2. Aucune garantie continue au-dela de M.
    /// </summary>
    public static class TrackingMeasurement
    {
        public static float CornerDisplacement(Vector3 position, Quaternion rotation, NominalPose nominal, GaugeBox box)
        {
            Vector3 up = nominal.Up.normalized;
            Vector3 forward = Vector3.ProjectOnPlane(nominal.Forward, up).normalized;
            Vector3 right = Vector3.Cross(up, forward);
            float result = 0f;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = box.Corner(i);
                Vector3 real = position + rotation * corner;
                Vector3 ideal = nominal.Position + right * corner.x + up * corner.y + forward * corner.z;
                Vector3 offset = Vector3.ProjectOnPlane(real - ideal, up);
                result = Math.Max(result, offset.magnitude);
            }
            return result;
        }

        /// <summary>d au pas k : pose reelle contre la pose nominale en s* (max des deux poses a un raccord exact).</summary>
        public static float StepDisplacement(BodyState state, ReferenceTrack track, float distance, GaugeBox box)
        {
            int piece = track.PieceAt(distance);
            float result = CornerDisplacement(state.Position, state.Rotation, track.Nominal(piece, distance), box);
            if (piece > 0 && distance <= track.Pieces[piece].StartDistanceMeters)
                result = Math.Max(result, CornerDisplacement(state.Position, state.Rotation,
                    track.Nominal(piece - 1, distance), box));
            return result;
        }

        /// <summary>Modele M : variation de pose enregistree contre les vitesses de fin de pas.</summary>
        public static bool VerifyModel(BodyState a, BodyState b, float dt, float positionTolerance,
            float rotationToleranceDegrees, out float positionResidual, out float rotationResidualDegrees)
        {
            positionResidual = (a.CenterOfMass + b.LinearVelocity * dt - b.CenterOfMass).magnitude;
            rotationResidualDegrees = Quaternion.Angle(Rotate(a.Rotation, b.AngularVelocity, dt), b.Rotation);
            return !float.IsNaN(positionResidual) && !float.IsNaN(rotationResidualDegrees)
                && positionResidual <= positionTolerance && rotationResidualDegrees <= rotationToleranceDegrees;
        }

        public static InterStepResult InterStepBound(BodyState a, BodyState b, float dt, float sA, float sB,
            ReferenceTrack track, GaugeBox box, float remainderMeters, float positionTolerance, float rotationToleranceDegrees)
        {
            var result = new InterStepResult();
            float positionResidual, rotationResidual;
            result.ModelVerified = VerifyModel(a, b, dt, positionTolerance, rotationToleranceDegrees,
                out positionResidual, out rotationResidual);
            result.PositionResidualMeters = positionResidual;
            result.RotationResidualDegrees = rotationResidual;

            Vector3 comLocal = Quaternion.Inverse(a.Rotation) * (a.CenterOfMass - a.Position);
            float r = 0f;
            for (int i = 0; i < 8; i++) r = Math.Max(r, (box.Corner(i) - comLocal).magnitude);
            float dp = b.LinearVelocity.magnitude * dt;
            float dphi = b.AngularVelocity.magnitude * dt;
            float travel = Math.Abs(sB - sA);

            // Decoupe de [sA, sB] a chaque raccord traverse : aucun reste ne franchit un raccord.
            float low = Math.Min(sA, sB), high = Math.Max(sA, sB);
            var bounds = new List<PieceBound>();
            float overall = 0f, lipschitzMax = 0f;
            for (int p = 0; p < track.Pieces.Count; p++)
            {
                var piece = track.Pieces[p];
                if (piece.EndDistanceMeters < low) continue;
                if (piece.StartDistanceMeters > high) break;
                // Un morceau qui ne touche l'intervalle qu'a un raccord y apporte sa pose unilaterale.
                float from = Math.Max(low, piece.StartDistanceMeters);
                float to = Math.Min(high, piece.EndDistanceMeters);
                float tau0 = high > low ? (from - sA) / (sB - sA) : 0f;
                float tau1 = high > low ? (to - sA) / (sB - sA) : 1f;
                if (tau1 < tau0) { float swap = tau0; tau0 = tau1; tau1 = swap; }
                // Taux de rotation de la caisse nominale (max |sin e| / a), jamais celui de la tangente seule.
                float lipschitz = dp + r * dphi + travel * (1f + box.Rho * track.BodyRateMax(p, from, to));
                lipschitzMax = Math.Max(lipschitzMax, lipschitz);
                float span = tau1 - tau0;
                int steps = lipschitz > 0f && span > 0f
                    ? Math.Max(1, (int)Math.Ceiling(lipschitz * span / (2f * remainderMeters))) : 1;
                float h = span / steps;
                float gridMax = 0f, startValue = 0f, endValue = 0f;
                for (int j = 0; j <= steps; j++)
                {
                    float tau = tau0 + h * j;
                    if (j == steps) tau = tau1;
                    float sigma = high > low ? Mathf.Clamp(sA + (sB - sA) * tau, from, to) : sA;
                    Vector3 com = a.CenterOfMass + b.LinearVelocity * (dt * tau);
                    Quaternion rotation = Rotate(a.Rotation, b.AngularVelocity, dt * tau);
                    Vector3 position = com - rotation * comLocal;
                    float value = CornerDisplacement(position, rotation, track.Nominal(p, sigma), box);
                    gridMax = Math.Max(gridMax, value);
                    if (j == 0) startValue = value;
                    if (j == steps) endValue = value;
                }
                float bound = gridMax + lipschitz * h * 0.5f;
                bounds.Add(new PieceBound(p, gridMax, bound, startValue, endValue));
                overall = Math.Max(overall, bound);
            }
            result.Pieces = bounds.AsReadOnly();
            result.BoundMeters = overall;
            result.LipschitzMeters = lipschitzMax;
            return result;
        }

        private static Quaternion Rotate(Quaternion start, Vector3 angularVelocity, float time)
        {
            float angle = angularVelocity.magnitude * time;
            if (!(angle > 0f)) return start;
            return Quaternion.AngleAxis(angle * Mathf.Rad2Deg, angularVelocity / angularVelocity.magnitude) * start;
        }
    }

    public enum ElementStatus { Measured = 0, NotMeasured = 1, NotSelectable = 2 }

    /// <summary>Tracabilite d'un element de la campagne (mouvement, corridor, cote de raccord d'anneau).</summary>
    public sealed class ElementTrace
    {
        public string Key { get; }
        public string Kind { get; }
        public int Passes { get; internal set; }
        public HashSet<int> Runs { get; } = new HashSet<int>();
        public float MaxStepDisplacementMeters { get; internal set; }
        public float MaxInterStepBoundMeters { get; internal set; }
        public float MaxSpeedRatio { get; internal set; }
        public int VerifiedIntervals { get; internal set; }
        public int UnverifiedIntervals { get; internal set; }
        public bool Selectable { get; internal set; } = true;

        public ElementTrace(string key, string kind) { Key = key; Kind = kind; }

        public ElementStatus Status
        {
            get
            {
                if (!Selectable) return ElementStatus.NotSelectable;
                return VerifiedIntervals > 0 ? ElementStatus.Measured : ElementStatus.NotMeasured;
            }
        }
    }

    /// <summary>Tracabilite par element : 72 mouvements, 24 raccords d'anneau x 2 cotes, 44 corridors.</summary>
    public sealed class CampaignTraceability
    {
        private readonly Dictionary<string, ElementTrace> elements = new Dictionary<string, ElementTrace>(StringComparer.Ordinal);
        private readonly HashSet<string> ringSeams;
        private HashSet<string> activeSeams = new HashSet<string>(StringComparer.Ordinal);
        private int currentRun = -1;
        private string lastKey;

        public IReadOnlyDictionary<string, ElementTrace> Elements { get { return elements; } }

        public CampaignTraceability(CompiledRoadModel model, IEnumerable<string> ringSeamKeys)
        {
            ringSeams = new HashSet<string>(ringSeamKeys, StringComparer.Ordinal);
            foreach (var movement in model.Movements) Add(movement.Id.ToString(), "movement");
            foreach (var corridor in model.Corridors) Add(corridor.CorridorId.ToString(), "corridor");
            foreach (var seam in ringSeams) { Add(seam + ":left", "seam"); Add(seam + ":right", "seam"); }
        }

        private void Add(string key, string kind) { elements[key] = new ElementTrace(key, kind); }

        /// <summary>
        /// Raccords tangents d'anneau (mouvement:entry|exit) : saut de courbure au-dela de la tolerance de
        /// raccord entre un mouvement et son corridor d'approche ou de depart (les 24 epingles par la 5.30).
        /// </summary>
        public static List<string> RingSeamKeys(CompiledRoadModel model)
        {
            var keys = new List<string>();
            foreach (var movement in model.Movements)
            {
                EffectiveLaneCorridor from, to;
                if (!model.TryGetCorridor(movement.FromCorridorId, out from) || !model.TryGetCorridor(movement.ToCorridorId, out to))
                    continue;
                float entry = movement.Curve.Sample(0f).CurvaturePerMeter - from.Curve.Sample(from.LengthMeters).CurvaturePerMeter;
                float exit = to.Curve.Sample(0f).CurvaturePerMeter - movement.Curve.Sample(movement.LengthMeters).CurvaturePerMeter;
                if (Math.Abs(entry) > Planning.PlanningTolerances.SeamCurvatureJumpPerMeter) keys.Add(movement.Id + ":entry");
                if (Math.Abs(exit) > Planning.PlanningTolerances.SeamCurvatureJumpPerMeter) keys.Add(movement.Id + ":exit");
            }
            keys.Sort(StringComparer.Ordinal);
            return keys;
        }

        public void MarkNotSelectable(string key)
        {
            ElementTrace trace;
            if (elements.TryGetValue(key, out trace)) trace.Selectable = false;
        }

        public void BeginRun(int run) { currentRun = run; lastKey = null; activeSeams.Clear(); }

        public void RecordStep(RoadId elementId, float displacement, float speedRatio)
        {
            ElementTrace trace;
            string key = elementId.ToString();
            if (!elements.TryGetValue(key, out trace)) return;
            if (key != lastKey) { trace.Passes++; lastKey = key; }
            trace.Runs.Add(currentRun);
            trace.MaxStepDisplacementMeters = Math.Max(trace.MaxStepDisplacementMeters, displacement);
            if (!float.IsNaN(speedRatio)) trace.MaxSpeedRatio = Math.Max(trace.MaxSpeedRatio, speedRatio);
        }

        public void RecordInterval(ReferenceTrack track, InterStepResult result, float leftSpeedRatio, float rightSpeedRatio)
        {
            var seamsThisInterval = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < result.Pieces.Count; i++)
            {
                var bound = result.Pieces[i];
                var piece = track.Pieces[bound.Piece];
                Record(piece.Id.ToString(), bound.BoundMeters, result.ModelVerified);
                if (i + 1 >= result.Pieces.Count) continue;
                // Raccord traverse : chaque cote porte sa pose nominale unilaterale.
                var next = track.Pieces[result.Pieces[i + 1].Piece];
                string seam = SeamKey(piece, next);
                if (seam == null) continue;
                Record(seam + ":left", bound.BoundMeters, result.ModelVerified);
                Record(seam + ":right", result.Pieces[i + 1].BoundMeters, result.ModelVerified);
                if (seamsThisInterval.Add(seam))
                {
                    bool newPass = !activeSeams.Contains(seam);
                    RecordStep(seam + ":left", bound.EndDisplacementMeters, leftSpeedRatio, newPass);
                    RecordStep(seam + ":right", result.Pieces[i + 1].StartDisplacementMeters, rightSpeedRatio, newPass);
                }
            }
            activeSeams = seamsThisInterval;
        }

        public void RecordInterval(ReferenceTrack track, InterStepResult result)
        {
            RecordInterval(track, result, float.NaN, float.NaN);
        }

        private string SeamKey(TrackPiece left, TrackPiece right)
        {
            if (left.Kind == RoadElementKind.JunctionMovement && ringSeams.Contains(left.Id + ":exit")) return left.Id + ":exit";
            if (right.Kind == RoadElementKind.JunctionMovement && ringSeams.Contains(right.Id + ":entry")) return right.Id + ":entry";
            return null;
        }

        private void Record(string key, float bound, bool verified)
        {
            ElementTrace trace;
            if (!elements.TryGetValue(key, out trace)) return;
            trace.Runs.Add(currentRun);
            if (verified)
            {
                trace.VerifiedIntervals++;
                trace.MaxInterStepBoundMeters = Math.Max(trace.MaxInterStepBoundMeters, bound);
            }
            else trace.UnverifiedIntervals++;
        }

        private void RecordStep(string key, float displacement, float speedRatio, bool newPass)
        {
            ElementTrace trace;
            if (!elements.TryGetValue(key, out trace)) return;
            if (newPass) trace.Passes++;
            trace.Runs.Add(currentRun);
            trace.MaxStepDisplacementMeters = Math.Max(trace.MaxStepDisplacementMeters, displacement);
            if (!float.IsNaN(speedRatio)) trace.MaxSpeedRatio = Math.Max(trace.MaxSpeedRatio, speedRatio);
        }

        public int CountStatus(ElementStatus status)
        {
            int count = 0;
            foreach (var trace in elements.Values) if (trace.Status == status) count++;
            return count;
        }

        public string ToReport(float? declaredTolerance)
        {
            var keys = new List<string>(elements.Keys);
            keys.Sort(StringComparer.Ordinal);
            var text = new StringBuilder();
            text.Append("kind\tkey\tstatus\tpasses\truns\td_step_max_m\td_interstep_max_m\tv_over_vstar_max\tmargin_to_eps_m\tverified\tunverified\n");
            foreach (var key in keys)
            {
                var trace = elements[key];
                float worst = Math.Max(trace.MaxStepDisplacementMeters, trace.MaxInterStepBoundMeters);
                text.Append(trace.Kind).Append('\t').Append(key).Append('\t').Append(trace.Status).Append('\t')
                    .Append(trace.Passes.ToString(CultureInfo.InvariantCulture)).Append('\t')
                    .Append(trace.Runs.Count.ToString(CultureInfo.InvariantCulture)).Append('\t')
                    .Append(F(trace.MaxStepDisplacementMeters)).Append('\t').Append(F(trace.MaxInterStepBoundMeters)).Append('\t')
                    .Append(F(trace.MaxSpeedRatio)).Append('\t')
                    .Append(declaredTolerance.HasValue ? F(declaredTolerance.Value - worst) : "undeclared").Append('\t')
                    .Append(trace.VerifiedIntervals.ToString(CultureInfo.InvariantCulture)).Append('\t')
                    .Append(trace.UnverifiedIntervals.ToString(CultureInfo.InvariantCulture)).Append('\n');
            }
            return text.ToString();
        }

        private static string F(float value) { return value.ToString("0.######", CultureInfo.InvariantCulture); }
    }
}
