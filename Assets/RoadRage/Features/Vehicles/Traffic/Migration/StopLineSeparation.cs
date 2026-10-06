#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RoadRage.Features.Vehicles.Traffic.Lifecycle;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    /// <summary>
    /// Preuve de separation des lignes d'arret (Story 5.35, P5 amende par le proprietaire le 2026-10-06). Pour chaque mouvement
    /// controle par une ligne et chaque mouvement en conflit avec lui, l'empreinte maximale de Gate A (a_e et restes inclus),
    /// arretee pare-chocs avant au bord avance de la fenetre d'arret (m_ctrl - 0,02) de la ligne, est comparee a la meme
    /// empreinte arretee au repli generique b = 0 :
    /// <list type="bullet">
    /// <item>cas A, separee a b = 0 : la ligne doit rester separee (aucun nouveau contact) ;</item>
    /// <item>cas B, contact conservatif deja present a b = 0 : marge non degradee au-dela de la tolerance declaree du profil et
    /// aucun nouveau recouvrement des empreintes nominales (gabarit, pose tangente, sans gonflement).</item>
    /// </list>
    /// Meme traitement des deux positions : poses interpolees sur la trajectoire prolongee.
    /// </summary>
    public static class StopLineSeparation
    {
        /// <summary>Bord avance de la fenetre d'arret de JunctionEntry : pare-chocs a m_ctrl - 0,02 de la frontiere.</summary>
        public const float StopWindowToleranceMeters = 0.02f;

        /// <summary>Allongement amont de la trajectoire : le repli b = 0 place le centre de l'empreinte avant le mouvement.</summary>
        public const float UpstreamReachMeters = 2f;

        public sealed class Row
        {
            public RoadId MovementId, OtherId;
            public float SLineMeters;
            public bool Preexisting;
            public float EntrySlackMeters, LineSlackMeters, EntryNominalMeters, LineNominalMeters;
            public string Violation;
        }

        public sealed class Result
        {
            public readonly List<Row> Rows = new List<Row>();
            public readonly List<string> Failures = new List<string>();
            public bool Passed { get { return Failures.Count == 0; } }
        }

        public static Result Measure(CompiledRoadModel model, GateAEvidenceParameters parameters, KinematicOffsetBounds bounds)
        {
            var result = new Result();
            bool any = false;
            foreach (var control in model.Controls) any |= control.HasStopLine;
            if (!any) return result;
            if (parameters == null || !parameters.Kinematic || bounds == null || !bounds.Closed)
            {
                result.Failures.Add("Lignes d'arret : la preuve de separation exige la pose cinematique et des bornes d'ecart fermees.");
                return result;
            }

            var graph = SweepGraph.FromModel(model);
            var profile = model.ValidationProfile;
            float halfLength = ConflictSweep.HalfLength(profile);
            float reach = ConflictSweep.Reach(profile);
            var otherPaths = new Dictionary<RoadId, List<List<SweepPose>>>();
            foreach (var control in model.Controls)
            {
                if (!control.HasStopLine) continue;
                foreach (var movementId in control.ControlledMovementIds)
                {
                    float sLine;
                    string failure;
                    if (!model.TryGetStopLine(movementId, out sLine))
                    {
                        result.Failures.Add("Ligne d'arret non projetee sur " + movementId + ".");
                        continue;
                    }

                    var paths = ConflictSweep.Paths(graph, movementId, reach + UpstreamReachMeters, out failure);
                    List<SweepPose> atLine, atEntry;
                    if (failure != null || paths.Count == 0
                        || !TryStopped(paths[0], movementId, Center(sLine, halfLength), out atLine, out failure)
                        || !TryStopped(paths[0], movementId, Center(0f, halfLength), out atEntry, out failure))
                    {
                        result.Failures.Add("Ligne d'arret de " + movementId + " : " + (failure ?? "trajectoire absente") + ".");
                        continue;
                    }

                    var others = new SortedSet<RoadId>();
                    foreach (var zone in model.ConflictZones)
                    {
                        if (!Contains(zone.MemberMovementIds, movementId)) continue;
                        foreach (var member in zone.MemberMovementIds) if (member != movementId) others.Add(member);
                    }

                    foreach (var other in others)
                    {
                        List<List<SweepPose>> pathsB;
                        if (!otherPaths.TryGetValue(other, out pathsB))
                        {
                            pathsB = ConflictSweep.Paths(graph, other, reach, out failure);
                            otherPaths[other] = pathsB;
                            if (failure != null)
                            {
                                result.Failures.Add("Mouvement en conflit " + other + " : " + failure + ".");
                                continue;
                            }
                        }

                        var line = Sweep(atLine, pathsB, profile, parameters, bounds);
                        var entry = Sweep(atEntry, pathsB, profile, parameters, bounds);
                        var row = new Row
                        {
                            MovementId = movementId, OtherId = other, SLineMeters = sLine, Preexisting = entry.ExactProven,
                            EntrySlackMeters = Slack(entry), LineSlackMeters = Slack(line),
                            EntryNominalMeters = Nominal(atEntry, pathsB, halfLength, profile.MaxVehicleHalfWidthMeters),
                            LineNominalMeters = Nominal(atLine, pathsB, halfLength, profile.MaxVehicleHalfWidthMeters)
                        };
                        if (line.Relation == PairRelation.FailClosed || entry.Relation == PairRelation.FailClosed)
                            row.Violation = "echec ferme : " + (line.FailClosedReason ?? entry.FailClosedReason);
                        else if (!row.Preexisting && line.ExactProven)
                            row.Violation = "cas A, nouveau contact";
                        else if (row.Preexisting && row.LineSlackMeters < row.EntrySlackMeters - profile.EnvelopeOverlapToleranceMeters)
                            row.Violation = "cas B, marge degradee au-dela de " + F(profile.EnvelopeOverlapToleranceMeters) + " m";
                        else if (row.Preexisting && row.EntryNominalMeters > 0f && !(row.LineNominalMeters > 0f))
                            row.Violation = "cas B, nouveau recouvrement nominal";
                        if (row.Violation != null) result.Failures.Add("Ligne d'arret " + movementId + " x " + other + " : " + row.Violation + ".");
                        result.Rows.Add(row);
                    }
                }
            }

            return result;
        }

        /// <summary>Section du rapport de migration : une ligne par paire, cas, marges et delta.</summary>
        public static void Render(StringBuilder text, CompiledRoadModel model, Result result)
        {
            text.Append("## Lignes d'arret : separation (P5)\n\n");
            text.Append("Empreinte maximale de Gate A arretee pare-chocs avant a `m_ctrl` - ").Append(F(StopWindowToleranceMeters))
                .Append(" m de la ligne, comparee au repli generique b = 0. Cas A : separee a b = 0, la ligne doit le rester. Cas B : contact conservatif ")
                .Append("deja present a b = 0, marge non degradee au-dela de `EnvelopeOverlapToleranceMeters` (").Append(F(model.ValidationProfile.EnvelopeOverlapToleranceMeters))
                .Append(" m) et aucun nouveau recouvrement nominal. Marges en metres (negatif : contact Gate A) ; distance nominale sans gonflement.\n\n");
            if (result.Rows.Count == 0)
            {
                text.Append("Aucune ligne authoree.\n\n");
                return;
            }

            text.Append("| Mouvement | s_line | Mouvement en conflit | Cas | Marge b = 0 | Marge ligne | Delta | Nominal b = 0 | Nominal ligne | Verdict |\n");
            text.Append("|---|---:|---|---|---:|---:|---:|---:|---:|---|\n");
            foreach (var row in result.Rows)
            {
                text.Append("| ").Append(Label(model, row.MovementId)).Append(" | ").Append(F(row.SLineMeters)).Append(" | ").Append(Label(model, row.OtherId))
                    .Append(" | ").Append(row.Preexisting ? "B" : "A").Append(" | ").Append(F(row.EntrySlackMeters)).Append(" | ").Append(F(row.LineSlackMeters))
                    .Append(" | ").Append(F(row.LineSlackMeters - row.EntrySlackMeters)).Append(" | ").Append(F(row.EntryNominalMeters))
                    .Append(" | ").Append(F(row.LineNominalMeters)).Append(" | ").Append(row.Violation ?? "conforme").Append(" |\n");
            }

            text.Append('\n');
        }

        /// <summary>Centre de l'empreinte arretee : pare-chocs avant au bord avance de la fenetre d'arret de la frontiere b.</summary>
        public static float Center(float boundaryMeters, float halfLength)
        {
            return boundaryMeters - (TrafficV2Settings.JunctionStopControlMarginMeters - StopWindowToleranceMeters) - halfLength;
        }

        /// <summary>
        /// Poses interpolees sur la trajectoire prolongee aux abscisses centre - 0,05, + 0,05 et + 0,15 (cumulee depuis l'entree du
        /// mouvement, negative dans le corridor d'approche).
        /// </summary>
        public static bool TryStopped(List<SweepPose> path, RoadId movementId, float center, out List<SweepPose> poses, out string failure)
        {
            poses = new List<SweepPose>();
            failure = null;
            int first = path.FindIndex(p => p.ElementId == movementId);
            if (first <= 0)
            {
                failure = "trajectoire sans amont";
                return false;
            }

            float origin = path[first].SMeters;
            int last = first;
            while (last + 1 < path.Count && path[last + 1].ElementId == movementId) last++;
            var along = new float[last + 1];
            for (int i = first; i <= last; i++) along[i] = path[i].SMeters - origin;
            for (int i = first - 1; i >= 0; i--) along[i] = along[i + 1] - (path[i + 1].Plan - path[i].Plan).magnitude;
            foreach (float target in new[] { center - 0.05f, center + 0.05f, center + 0.15f })
            {
                if (target < along[0] || target > along[last])
                {
                    failure = "position d'arret hors de la trajectoire (" + F(target) + " m)";
                    return false;
                }

                int i = 0;
                while (i + 1 < last && along[i + 1] < target) i++;
                float t = along[i + 1] > along[i] ? Mathf.Clamp01((target - along[i]) / (along[i + 1] - along[i])) : 0f;
                var a = path[i];
                var b = path[i + 1];
                var pose = SweepPose.From(Vector3.Lerp(a.Position, b.Position, t), Vector3.Lerp(a.Tangent, b.Tangent, t).normalized);
                pose.Up = Vector3.Lerp(a.Up, b.Up, t).normalized;
                var near = t < 0.5f ? a : b;
                pose.ElementId = near.ElementId;
                pose.SMeters = a.ElementId == b.ElementId ? Mathf.Lerp(a.SMeters, b.SMeters, t) : near.SMeters;
                poses.Add(pose);
            }

            return true;
        }

        private static PairSweep Sweep(List<SweepPose> stopped, List<List<SweepPose>> otherPaths, RoadModelValidationProfile profile,
            GateAEvidenceParameters parameters, KinematicOffsetBounds bounds)
        {
            return ConflictSweep.EvaluateKinematic(new List<List<SweepPose>> { stopped }, otherPaths, profile, parameters, bounds);
        }

        private static float Slack(PairSweep sweep)
        {
            return sweep.EnvelopeSelected ? sweep.ExactSlackMeters : sweep.EnvelopeSlackMeters;
        }

        /// <summary>Distance minimale entre empreintes nominales (gabarit maximal, pose tangente, sans gonflement).</summary>
        private static float Nominal(List<SweepPose> stopped, List<List<SweepPose>> otherPaths, float halfLength, float halfWidth)
        {
            float best = float.PositiveInfinity;
            foreach (var a in stopped)
                foreach (var path in otherPaths)
                    foreach (var b in path)
                        best = Math.Min(best, ConflictSweep.RectangleDistance(a, b, halfLength, halfWidth));
            return best;
        }

        private static bool Contains(IReadOnlyList<RoadId> ids, RoadId id)
        {
            for (int i = 0; i < ids.Count; i++) if (ids[i] == id) return true;
            return false;
        }

        private static string Label(CompiledRoadModel model, RoadId movementId)
        {
            CompiledJunctionMovement movement;
            return model.TryGetMovement(movementId, out movement) && !string.IsNullOrEmpty(movement.Label)
                ? movement.Label.Replace("|", "/") : movementId.ToString();
        }

        private static string F(float value)
        {
            return float.IsPositiveInfinity(value) ? "inf" : value.ToString("0.000", CultureInfo.InvariantCulture);
        }
    }
}
#endif
