using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Coordination
{
    /// <summary>Position d'une approche B par rapport a une approche A, vue du conducteur de A (Story 5.35, P4).</summary>
    public enum RightOfWayRelation
    {
        /// <summary>Meme sens que A.</summary>
        Same = 0,
        /// <summary>B vient de la droite de A : B passe avant A sous priorite a droite.</summary>
        FromRight = 1,
        Opposite = 2,
        /// <summary>B vient de la gauche de A : A passe avant B.</summary>
        FromLeft = 3,
        /// <summary>A moins de <see cref="RightOfWay.AmbiguityDegrees"/> d'une frontiere de secteur : echec de validation.</summary>
        Ambiguous = 4
    }

    /// <summary>
    /// Relation « vient de la droite » derivee de la geometrie (Story 5.35, decision P4), jamais authoree : fonction pure,
    /// deterministe et versionnee du cap en fin de corridor d'approche, dans le plan route (right = up x tangente, AD-45).
    /// Le cap de B, mesure depuis celui de A, tombe dans un des quatre secteurs de 90 deg centres sur A (meme sens), sa droite,
    /// son oppose et sa gauche : un cap de B dirige vers la gauche de A signifie que B arrive par la droite de A.
    /// </summary>
    public static class RightOfWay
    {
        public const int FunctionVersion = 1;

        /// <summary>Demi-ouverture d'un secteur (deg).</summary>
        public const float SectorHalfDegrees = 45f;

        /// <summary>Bande declaree autour de chaque frontiere de secteur ou la relation est ambigue (deg).</summary>
        public const float AmbiguityDegrees = 10f;

        /// <summary>Relation de B vue de A, caps et normale route quelconques (projetes dans le plan route).</summary>
        public static RightOfWayRelation Classify(Vector3 headingA, Vector3 headingB, Vector3 up)
        {
            float theta;
            if (!TrySignedAngle(headingA, headingB, up, out theta)) return RightOfWayRelation.Ambiguous;
            float magnitude = Math.Abs(theta);
            if (Math.Abs(magnitude - SectorHalfDegrees) < AmbiguityDegrees
                || Math.Abs(magnitude - (180f - SectorHalfDegrees)) < AmbiguityDegrees) return RightOfWayRelation.Ambiguous;
            if (magnitude < SectorHalfDegrees) return RightOfWayRelation.Same;
            if (magnitude > 180f - SectorHalfDegrees) return RightOfWayRelation.Opposite;
            // theta > 0 : le cap de B tourne vers la droite de A, donc B traverse de la gauche de A vers sa droite.
            return theta > 0f ? RightOfWayRelation.FromLeft : RightOfWayRelation.FromRight;
        }

        /// <summary>Angle signe (deg) de A vers B autour de up, positif vers la droite de A (right = up x A).</summary>
        public static bool TrySignedAngle(Vector3 headingA, Vector3 headingB, Vector3 up, out float degrees)
        {
            degrees = 0f;
            if (up.sqrMagnitude < 1e-12f) return false;
            Vector3 n = up.normalized;
            Vector3 a = Vector3.ProjectOnPlane(headingA, n), b = Vector3.ProjectOnPlane(headingB, n);
            if (a.sqrMagnitude < 1e-12f || b.sqrMagnitude < 1e-12f) return false;
            a.Normalize(); b.Normalize();
            Vector3 right = Vector3.Cross(n, a);
            degrees = (float)(Math.Atan2(Vector3.Dot(b, right), Vector3.Dot(b, a)) * 180d / Math.PI);
            return true;
        }
    }

    /// <summary>
    /// Relation de droite d'un modele compile, calculee une seule fois par modele pour chaque paire ordonnee de controles d'un
    /// carrefour dont tous les controles sont `Uncontrolled` (seul genre qui la lit). Publiee dans le rapport de migration et
    /// liee a la signature Gate A par son <see cref="Hash"/> : la fonction n'entre pas dans RoadModelVersion.
    /// </summary>
    public sealed class RightOfWayTable
    {
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<CompiledRoadModel, RightOfWayTable> Cache =
            new System.Runtime.CompilerServices.ConditionalWeakTable<CompiledRoadModel, RightOfWayTable>();

        public struct Entry
        {
            public RoadId JunctionId, ControlA, ControlB;
            public float Degrees;
            public RightOfWayRelation Relation;
        }

        private readonly Dictionary<long, RightOfWayRelation> relations = new Dictionary<long, RightOfWayRelation>();
        private readonly Dictionary<RoadId, int> controlIndex = new Dictionary<RoadId, int>();
        private readonly List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries { get { return entries; } }
        /// <summary>Paires ambigues : un modele qui en porte echoue a la validation de l'authoring.</summary>
        public int AmbiguousCount { get; }
        /// <summary>SHA-256 hexadecimal minuscule du texte canonique (<see cref="CanonicalText"/>).</summary>
        public string Hash { get; }
        public string CanonicalText { get; }

        public static RightOfWayTable For(CompiledRoadModel model)
        {
            if (model == null) throw new ArgumentNullException("model");
            return Cache.GetValue(model, m => new RightOfWayTable(m));
        }

        private RightOfWayTable(CompiledRoadModel model)
        {
            for (int i = 0; i < model.Controls.Count; i++) controlIndex[model.Controls[i].Id] = i;
            var junctions = new List<Junction>(model.Junctions);
            junctions.Sort((a, b) => a.Id.CompareTo(b.Id));
            int ambiguous = 0;
            foreach (var junction in junctions)
            {
                var ids = model.GetControlsInJunction(junction.Id);
                var approaches = new List<KeyValuePair<RoadId, RoadCurveSample>>();
                bool uncontrolled = ids.Count > 0;
                foreach (var id in ids)
                {
                    CompiledJunctionControl control;
                    RoadCurveSample end;
                    if (!model.TryGetControl(id, out control) || control.Kind != JunctionControlKind.Uncontrolled) { uncontrolled = false; break; }
                    if (!TryApproachEnd(model, control, out end)) { uncontrolled = false; break; }
                    approaches.Add(new KeyValuePair<RoadId, RoadCurveSample>(id, end));
                }
                if (!uncontrolled) continue;
                foreach (var a in approaches)
                    foreach (var b in approaches)
                    {
                        if (a.Key == b.Key) continue;
                        float degrees;
                        Vector3 up = a.Value.Up;
                        var relation = RightOfWay.TrySignedAngle(a.Value.Tangent, b.Value.Tangent, up, out degrees)
                            ? RightOfWay.Classify(a.Value.Tangent, b.Value.Tangent, up) : RightOfWayRelation.Ambiguous;
                        if (relation == RightOfWayRelation.Ambiguous) ambiguous++;
                        relations[Key(a.Key, b.Key)] = relation;
                        entries.Add(new Entry { JunctionId = junction.Id, ControlA = a.Key, ControlB = b.Key, Degrees = degrees, Relation = relation });
                    }
            }
            AmbiguousCount = ambiguous;
            var text = new StringBuilder();
            text.Append("right-of-way-v").Append(RightOfWay.FunctionVersion.ToString(CultureInfo.InvariantCulture))
                .Append("|sector=").Append(RightOfWay.SectorHalfDegrees.ToString("R", CultureInfo.InvariantCulture))
                .Append("|ambiguity=").Append(RightOfWay.AmbiguityDegrees.ToString("R", CultureInfo.InvariantCulture)).Append('\n');
            foreach (var entry in entries)
                text.Append(entry.JunctionId).Append(' ').Append(entry.ControlA).Append(' ').Append(entry.ControlB).Append(' ')
                    .Append(entry.Relation).Append('\n');
            CanonicalText = text.ToString();
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(CanonicalText));
                var hex = new StringBuilder(bytes.Length * 2);
                foreach (var value in bytes) hex.Append(value.ToString("x2", CultureInfo.InvariantCulture));
                Hash = hex.ToString();
            }
        }

        /// <summary>Relation de l'approche B vue de A ; faux hors table (carrefour non `Uncontrolled`, controle inconnu).</summary>
        public bool TryGet(RoadId controlA, RoadId controlB, out RightOfWayRelation relation)
        {
            return relations.TryGetValue(Key(controlA, controlB), out relation);
        }

        private long Key(RoadId a, RoadId b)
        {
            int ia, ib;
            if (!controlIndex.TryGetValue(a, out ia) || !controlIndex.TryGetValue(b, out ib)) return -1L;
            return ((long)ia << 32) | (uint)ib;
        }

        /// <summary>Cap en fin de corridor d'approche : dernier echantillon du corridor de depart des mouvements du controle.</summary>
        private static bool TryApproachEnd(CompiledRoadModel model, CompiledJunctionControl control, out RoadCurveSample end)
        {
            end = default(RoadCurveSample);
            if (control.ControlledMovementIds.Count == 0) return false;
            CompiledJunctionMovement movement;
            EffectiveLaneCorridor corridor;
            if (!model.TryGetMovement(control.ControlledMovementIds[0], out movement)
                || !model.TryGetCorridor(movement.FromCorridorId, out corridor) || corridor.Samples == null || corridor.Samples.Count == 0)
                return false;
            end = corridor.Samples[corridor.Samples.Count - 1];
            return true;
        }
    }
}
