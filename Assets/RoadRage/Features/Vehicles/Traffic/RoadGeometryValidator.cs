using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic
{
    /// <summary>
    /// Validation geometrique a echec dur (Story 5.26), appelee par
    /// <see cref="RoadModelValidator.Validate"/> sur une source structurellement valide, donc par
    /// <see cref="RoadModelCompiler.Compile"/> avant toute emission de version. Elle ne derive, ne
    /// reordonne et ne repare rien : un desaccord est un echec, jamais une correction.
    ///
    /// Deux phases. La <b>forme</b> (repere, longueur, largeurs, domaines, boites) d'abord ; si
    /// elle echoue, on s'arrete : coutures, adjacences et coupe transversale supposent des reperes
    /// sains et ne feraient qu'empiler du bruit. Puis les <b>relations</b> : coutures longitudinales,
    /// sens et cote des adjacences, procedure de decision d'AD-48.
    /// </summary>
    internal static class RoadGeometryValidator
    {
        /// <summary>Tolerance numerique d'un repere unitaire et orthogonal (sans unite).</summary>
        private const float UnitTolerance = 1e-3f;

        /// <summary>
        /// Plafond du nombre de points ajoutes par un intervalle d'adjacence pour couvrir tout
        /// l'intervalle. Garde de cout, jamais un invariant de contrat.
        /// </summary>
        private const int MaxAdjacencyCheckpoints = 512;

        internal static void Validate(RoadModelSource source, List<RoadModelValidationIssue> issues)
        {
            var profile = source.ValidationProfile;
            var corridors = source.Corridors ?? new LaneCorridor[0];
            var movements = source.Movements ?? new JunctionMovement[0];
            var adjacencies = source.Adjacencies ?? new LaneAdjacency[0];
            var connections = source.Connections ?? new LaneConnection[0];
            var portals = source.Portals ?? new Portal[0];
            var junctions = source.Junctions ?? new Junction[0];
            var zones = source.ConflictZones ?? new ConflictZone[0];

            // ------------------------------------------------------------ forme
            int before = issues.Count;
            var corridorById = new Dictionary<RoadId, LaneCorridor>();
            for (int i = 0; i < corridors.Length; i++)
            {
                corridorById[corridors[i].Id] = corridors[i];
                CheckShape(issues, corridors[i].Samples, corridors[i].LengthMeters, corridors[i].Id, "LaneCorridor", profile);
            }

            for (int i = 0; i < movements.Length; i++)
            {
                CheckShape(issues, movements[i].Samples, movements[i].LengthMeters, movements[i].Id, "JunctionMovement", profile);
            }

            for (int i = 0; i < adjacencies.Length; i++)
            {
                var adjacency = adjacencies[i];
                CheckInterval(issues, adjacency.FromStartSMeters, adjacency.FromEndSMeters, corridorById[adjacency.FromCorridorId], adjacency.Id, "intervalle source", profile);
                CheckInterval(issues, adjacency.ToStartSMeters, adjacency.ToEndSMeters, corridorById[adjacency.ToCorridorId], adjacency.Id, "intervalle cible", profile);
            }

            for (int i = 0; i < portals.Length; i++)
            {
                float length = corridorById[portals[i].CorridorId].LengthMeters;
                if (!(portals[i].SMeters >= -profile.LengthToleranceMeters && portals[i].SMeters <= length + profile.LengthToleranceMeters))
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.ArcPositionOutOfDomain,
                        portals[i].Id,
                        "Portal a s=" + Format(portals[i].SMeters) + " hors du domaine [0, " + Format(length) + "] de son corridor."));
                }
            }

            for (int i = 0; i < junctions.Length; i++)
            {
                CheckExtents(issues, junctions[i].Boundary.Extents, junctions[i].Id, "Junction.Boundary");
            }

            for (int i = 0; i < zones.Length; i++)
            {
                CheckExtents(issues, zones[i].Volume.Extents, zones[i].Id, "ConflictZone.Volume");
            }

            if (issues.Count > before)
            {
                return;
            }

            // ------------------------------------------------------------ relations
            var curves = new Dictionary<RoadId, RoadCurve>();
            for (int i = 0; i < corridors.Length; i++)
            {
                curves[corridors[i].Id] = new RoadCurve(corridors[i].Samples);
            }

            for (int i = 0; i < connections.Length; i++)
            {
                var connection = connections[i];
                var from = curves[connection.FromCorridorId];
                var to = curves[connection.ToCorridorId];
                string defect = SeamDefect(from.Sample(from.Length), to.Sample(to.StartS), false, profile);
                if (defect != null)
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.ConnectionSeamBroken,
                        connection.Id,
                        "Couture " + connection.FromCorridorId + " -> " + connection.ToCorridorId + " rompue : " + defect + "."));
                }
            }

            for (int i = 0; i < movements.Length; i++)
            {
                var movement = movements[i];
                var curve = new RoadCurve(movement.Samples);
                var from = curves[movement.FromCorridorId];
                var to = curves[movement.ToCorridorId];

                string entry = SeamDefect(from.Sample(from.Length), curve.Sample(curve.StartS), true, profile);
                string exit = SeamDefect(curve.Sample(curve.Length), to.Sample(to.StartS), true, profile);
                if (entry != null || exit != null)
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.MovementSeamBroken,
                        movement.Id,
                        "JunctionMovement aux coutures rompues : entree " + (entry ?? "ok") + ", sortie " + (exit ?? "ok") + "."));
                }
            }

            var datumBySection = new Dictionary<RoadId, LaneCorridor>();
            for (int i = 0; i < corridors.Length; i++)
            {
                if (corridors[i].IsCrossSectionDatum)
                {
                    datumBySection[corridors[i].SectionId] = corridors[i];
                }
            }

            for (int i = 0; i < adjacencies.Length; i++)
            {
                CheckAdjacency(issues, adjacencies[i], corridorById, curves, datumBySection, profile);
            }

            var sections = source.Sections ?? new RoadSection[0];
            for (int i = 0; i < sections.Length; i++)
            {
                var members = new List<LaneCorridor>();
                for (int c = 0; c < corridors.Length; c++)
                {
                    if (corridors[c].SectionId == sections[i].Id)
                    {
                        members.Add(corridors[c]);
                    }
                }

                if (members.Count >= 2)
                {
                    members.Sort(delegate(LaneCorridor a, LaneCorridor b) { return a.LateralOrder.CompareTo(b.LateralOrder); });
                    CheckCrossSection(issues, members, datumBySection[sections[i].Id], curves, profile);
                }
            }
        }

        // ------------------------------------------------------------------ forme

        private static void CheckShape(
            List<RoadModelValidationIssue> issues,
            RoadCurveSample[] samples,
            float declaredLength,
            RoadId subject,
            string typeName,
            RoadModelValidationProfile profile)
        {
            float tolerance = profile.LengthToleranceMeters;
            string frameDefect = null;
            string upDefect = null;
            string chordDefect = null;
            string lengthDefect = null;
            string widthDefect = null;

            if (!(Mathf.Abs(samples[0].SMeters) <= tolerance))
            {
                lengthDefect = "premiere abscisse " + Format(samples[0].SMeters) + " au lieu de 0";
            }
            else if (!(Mathf.Abs(declaredLength - samples[samples.Length - 1].SMeters) <= tolerance))
            {
                lengthDefect = "LengthMeters " + Format(declaredLength) + " contre derniere abscisse " + Format(samples[samples.Length - 1].SMeters);
            }

            for (int i = 0; i < samples.Length; i++)
            {
                var sample = samples[i];
                if (frameDefect == null
                    && !(Mathf.Abs(sample.Tangent.magnitude - 1f) <= UnitTolerance
                        && Mathf.Abs(sample.Up.magnitude - 1f) <= UnitTolerance
                        && Mathf.Abs(Vector3.Dot(sample.Tangent, sample.Up)) <= UnitTolerance))
                {
                    frameDefect = "echantillon " + i;
                }

                // Un road-up retourne echange silencieusement gauche et droite : motif propre (code 32),
                // le repere lui-meme restant unitaire et orthogonal.
                if (upDefect == null && !(Vector3.Dot(sample.Up, Vector3.up) > 0f))
                {
                    upDefect = "echantillon " + i;
                }

                // Une tangente opposee a sa corde inverse le sens de marche sans rien casser d'autre :
                // motif propre (code 33).
                if (chordDefect == null && i > 0)
                {
                    Vector3 chord = sample.Position - samples[i - 1].Position;
                    if (!(Vector3.Dot(chord, sample.Tangent) > 0f && Vector3.Dot(chord, samples[i - 1].Tangent) > 0f))
                    {
                        chordDefect = "echantillon " + i;
                    }
                }

                if (widthDefect == null && !(sample.HalfWidthLeftMeters > 0f && sample.HalfWidthRightMeters > 0f))
                {
                    widthDefect = "echantillon " + i;
                }

                if (lengthDefect == null && i > 0)
                {
                    float step = sample.SMeters - samples[i - 1].SMeters;
                    float chord = (sample.Position - samples[i - 1].Position).magnitude;
                    if (!(Mathf.Abs(step - chord) <= tolerance))
                    {
                        lengthDefect = "pas d'abscisse " + Format(step) + " contre corde " + Format(chord) + " a l'echantillon " + i;
                    }
                }
            }

            if (frameDefect != null)
            {
                issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.NonOrthonormalFrame, subject,
                    typeName + " : tangente/road-up non orthonormes a l'" + frameDefect + "."));
            }

            if (upDefect != null)
            {
                issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.RoadUpFlipped, subject,
                    typeName + " : road-up retourne (dote negativement au monde-haut) a l'" + upDefect
                        + ", ce qui echange gauche et droite."));
            }

            if (chordDefect != null)
            {
                issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.TangentOpposesChord, subject,
                    typeName + " : tangente opposee ou perpendiculaire a sa corde a l'" + chordDefect + "."));
            }

            if (lengthDefect != null)
            {
                issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.InconsistentCurveLength, subject,
                    typeName + " : longueur incoherente, " + lengthDefect + "."));
            }

            if (widthDefect != null)
            {
                issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.NonPositiveHalfWidth, subject,
                    typeName + " : demi-largeur non positive a l'" + widthDefect + "."));
            }
        }

        private static void CheckInterval(
            List<RoadModelValidationIssue> issues,
            float start,
            float end,
            LaneCorridor corridor,
            RoadId subject,
            string label,
            RoadModelValidationProfile profile)
        {
            float tolerance = profile.LengthToleranceMeters;
            if (!(start < end && start >= -tolerance && end <= corridor.LengthMeters + tolerance))
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.ArcPositionOutOfDomain,
                    subject,
                    "LaneAdjacency : " + label + " [" + Format(start) + ", " + Format(end) + "] vide ou hors du domaine [0, "
                        + Format(corridor.LengthMeters) + "] de " + corridor.Id + "."));
            }
        }

        private static void CheckExtents(List<RoadModelValidationIssue> issues, Vector3 extents, RoadId subject, string fieldName)
        {
            if (!(extents.x > 0f && extents.y > 0f && extents.z > 0f))
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.NonPositiveBoxExtents,
                    subject,
                    fieldName + ".Extents doit etre strictement positif sur chaque axe."));
            }
        }

        // ------------------------------------------------------------------ coutures

        /// <summary>Null si la couture tient, sinon la description du defaut.</summary>
        private static string SeamDefect(RoadCurvePoint end, RoadCurvePoint start, bool checkWidths, RoadModelValidationProfile profile)
        {
            float gap = (start.Position - end.Position).magnitude;
            if (!(gap <= profile.SeamGapToleranceMeters))
            {
                return "ecart " + Format(gap) + " m";
            }

            float angle = Vector3.Angle(end.Tangent, start.Tangent);
            if (!(angle <= profile.SeamTangentToleranceDegrees))
            {
                return "tangentes a " + Format(angle) + " degres";
            }

            if (checkWidths)
            {
                float left = Mathf.Abs(start.HalfWidthLeftMeters - end.HalfWidthLeftMeters);
                float right = Mathf.Abs(start.HalfWidthRightMeters - end.HalfWidthRightMeters);
                if (!(left <= profile.SeamGapToleranceMeters && right <= profile.SeamGapToleranceMeters))
                {
                    return "largeurs discontinues (gauche " + Format(left) + " m, droite " + Format(right) + " m)";
                }
            }

            return null;
        }

        // ------------------------------------------------------------------ adjacences

        private static void CheckAdjacency(
            List<RoadModelValidationIssue> issues,
            LaneAdjacency adjacency,
            Dictionary<RoadId, LaneCorridor> corridorById,
            Dictionary<RoadId, RoadCurve> curves,
            Dictionary<RoadId, LaneCorridor> datumBySection,
            RoadModelValidationProfile profile)
        {
            var from = corridorById[adjacency.FromCorridorId];
            var to = corridorById[adjacency.ToCorridorId];
            var fromCurve = curves[from.Id];
            var toCurve = curves[to.Id];

            bool sameSection = from.SectionId == to.SectionId;
            RoadCurve datumCurve = sameSection ? curves[datumBySection[from.SectionId].Id] : null;

            // Points de controle : bornes de l'intervalle source et chaque echantillon interieur.
            var checkpoints = new List<float>();
            checkpoints.Add(adjacency.FromStartSMeters);
            for (int i = 0; i < from.Samples.Length; i++)
            {
                if (from.Samples[i].SMeters > adjacency.FromStartSMeters && from.Samples[i].SMeters < adjacency.FromEndSMeters)
                {
                    checkpoints.Add(from.Samples[i].SMeters);
                }
            }

            checkpoints.Add(adjacency.FromEndSMeters);

            // « Sur tout l'intervalle d'adjacence » (AD-48) : les seuls echantillons authored ne
            // suffisent pas, un desaccord peut naitre entre deux d'entre eux. On ajoute une grille au
            // pas de longueur du profil de validation : la resolution du verdict est ainsi versionnee
            // avec le modele, et deux implementations du meme profil rendent le meme verdict. Le
            // plafond de points est un garde de cout, pas un invariant : il ne mord que sur un profil
            // plus fin que la carte ne le justifie.
            float span = adjacency.FromEndSMeters - adjacency.FromStartSMeters;
            float step = Mathf.Max(profile.LengthToleranceMeters, span / MaxAdjacencyCheckpoints);
            if (step > 0f && !float.IsInfinity(step))
            {
                for (float s = adjacency.FromStartSMeters + step; s < adjacency.FromEndSMeters; s += step)
                {
                    checkpoints.Add(s);
                }
            }

            checkpoints.Sort();

            bool wantRight = adjacency.Side == LaneSide.Right;
            string sideDefect = null;
            for (int i = 0; i < checkpoints.Count; i++)
            {
                var here = fromCurve.Sample(checkpoints[i]);
                var there = toCurve.Project(here.Position, adjacency.ToStartSMeters, adjacency.ToEndSMeters).Point;

                if (!(Vector3.Dot(here.Tangent, there.Tangent) > 0f))
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.OppositeDirectionAdjacency,
                        adjacency.Id,
                        "LaneAdjacency entre " + from.Id + " et " + to.Id + " de sens opposes a s=" + Format(checkpoints[i]) + "."));
                    return;
                }

                if (sideDefect != null)
                {
                    continue;
                }

                // Cote geometrique : position de la cible dans le repere de la source.
                float lateral = Vector3.Dot(there.Position - here.Position, here.Right);
                if (!(wantRight ? lateral > 0f : lateral < 0f))
                {
                    sideDefect = "la geometrie place la cible a lateral " + Format(lateral) + " m a s=" + Format(checkpoints[i]);
                    continue;
                }

                // Cote transversal (AD-48) : l'ordre croissant va vers la droite du datum, donc vers la
                // droite d'un corridor de meme sens que lui et vers la gauche d'un corridor oppose.
                if (datumCurve != null)
                {
                    var datumHere = datumCurve.Project(here.Position).Point;
                    bool alongDatum = Vector3.Dot(here.Tangent, datumHere.Tangent) > 0f;
                    bool orderSaysRight = alongDatum ? to.LateralOrder > from.LateralOrder : to.LateralOrder < from.LateralOrder;
                    if (orderSaysRight != wantRight)
                    {
                        sideDefect = "LateralOrder " + from.LateralOrder + " -> " + to.LateralOrder + " contredit Side=" + adjacency.Side + " a s=" + Format(checkpoints[i]);
                    }
                }
            }

            if (sideDefect != null)
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.LaneSideDisagreement,
                    adjacency.Id,
                    "LaneAdjacency " + from.Id + " -> " + to.Id + " : " + sideDefect + "."));
            }
        }

        // ------------------------------------------------------------------ coupe transversale (AD-48)

        /// <summary>Position d'un corridor le long du datum : lateral et bords d'enveloppe vers la droite du datum.</summary>
        private struct DatumTrace
        {
            public float S;
            public float Lateral;
            public float LowEdge;
            public float HighEdge;
        }

        /// <summary>Un corridor projete sur le datum : son intervalle de recouvrement et sa trace.</summary>
        private sealed class GroundedCorridor
        {
            public LaneCorridor Corridor;
            public float Start;
            public float End;
            public List<DatumTrace> Trace = new List<DatumTrace>();
        }

        /// <summary>
        /// Procedure de decision d'AD-48. Chaque corridor est projete au plus proche point sur la
        /// courbe du datum, restreint a l'intervalle ou les deux se recouvrent, egalite departagee par
        /// la plus petite abscisse (<see cref="RoadCurve.Project(Vector3, float, float)"/>). Puis, pour
        /// chaque paire d'ordres croissants dont les intervalles se recouvrent (plus qu'en un point),
        /// les lignes centrales doivent etre strictement croissantes vers la droite locale du datum et
        /// les enveloppes ne pas se recouvrir au-dela de la tolerance.
        /// </summary>
        private static void CheckCrossSection(
            List<RoadModelValidationIssue> issues,
            List<LaneCorridor> members,
            LaneCorridor datum,
            Dictionary<RoadId, RoadCurve> curves,
            RoadModelValidationProfile profile)
        {
            var datumCurve = curves[datum.Id];
            var grounded = new List<GroundedCorridor>();

            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                GroundedCorridor trace = member.Id == datum.Id
                    ? TraceDatum(datum, datumCurve)
                    : TraceOnDatum(member, curves[member.Id], datum, datumCurve, profile);

                if (trace == null)
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.CorridorNotGroundedOnDatum,
                        member.Id,
                        "Corridor sans recouvrement avec le datum " + datum.Id + " de sa section, ou ni parallele ni antiparallele a lui."));
                    continue;
                }

                grounded.Add(trace);
            }

            for (int i = 0; i < grounded.Count; i++)
            {
                for (int j = i + 1; j < grounded.Count; j++)
                {
                    ComparePair(issues, grounded[i], grounded[j], profile);
                }
            }
        }

        private static GroundedCorridor TraceDatum(LaneCorridor datum, RoadCurve curve)
        {
            var grounded = new GroundedCorridor();
            grounded.Corridor = datum;
            grounded.Start = curve.StartS;
            grounded.End = curve.Length;
            for (int i = 0; i < datum.Samples.Length; i++)
            {
                var sample = datum.Samples[i];
                var trace = new DatumTrace();
                trace.S = sample.SMeters;
                trace.Lateral = 0f;
                trace.LowEdge = -sample.HalfWidthLeftMeters;
                trace.HighEdge = sample.HalfWidthRightMeters;
                grounded.Trace.Add(trace);
            }

            return grounded;
        }

        /// <summary>Null si le corridor ne peut pas etre ancre au datum.</summary>
        private static GroundedCorridor TraceOnDatum(
            LaneCorridor member,
            RoadCurve curve,
            LaneCorridor datum,
            RoadCurve datumCurve,
            RoadModelValidationProfile profile)
        {
            float tolerance = profile.LengthToleranceMeters;

            // « A peu pres parallele ou antiparallele » (AD-48) : seuil versionne du profil de
            // validation, jamais une constante de code.
            float minParallelism = Mathf.Cos(profile.GroundingMaxOffAxisDegrees * Mathf.Deg2Rad);

            // Intervalle de recouvrement sur le datum, vu des deux cotes : les points du corridor qui
            // tombent a l'interieur du datum, et les points du datum qui tombent a l'interieur du
            // corridor. Sans le second sens, un corridor qui deborde le datum perdrait la frange
            // recouverte entre deux de ses echantillons.
            float start = float.PositiveInfinity;
            float end = float.NegativeInfinity;
            for (int i = 0; i < member.Samples.Length; i++)
            {
                var projection = datumCurve.Project(member.Samples[i].Position);
                if (projection.LongitudinalOverrunMeters <= tolerance)
                {
                    start = Mathf.Min(start, projection.SMeters);
                    end = Mathf.Max(end, projection.SMeters);
                }
            }

            for (int i = 0; i < datum.Samples.Length; i++)
            {
                if (curve.Project(datum.Samples[i].Position).LongitudinalOverrunMeters <= tolerance)
                {
                    start = Mathf.Min(start, datum.Samples[i].SMeters);
                    end = Mathf.Max(end, datum.Samples[i].SMeters);
                }
            }

            if (!(end - start > tolerance))
            {
                return null;
            }

            // Points du corridor a projeter : ses echantillons, plus ses points les plus proches des
            // echantillons du datum et des bornes de l'intervalle, pour que la trace couvre tout le
            // recouvrement meme quand le corridor a peu d'echantillons.
            var points = new List<RoadCurvePoint>();
            for (int i = 0; i < member.Samples.Length; i++)
            {
                points.Add(curve.Sample(member.Samples[i].SMeters));
            }

            points.Add(curve.Project(datumCurve.Sample(start).Position).Point);
            points.Add(curve.Project(datumCurve.Sample(end).Position).Point);
            for (int i = 0; i < datum.Samples.Length; i++)
            {
                if (datum.Samples[i].SMeters > start && datum.Samples[i].SMeters < end)
                {
                    points.Add(curve.Project(datum.Samples[i].Position).Point);
                }
            }

            var grounded = new GroundedCorridor();
            grounded.Corridor = member;
            grounded.Start = start;
            grounded.End = end;

            int direction = 0;
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                var projection = datumCurve.Project(point.Position, start, end);
                if (projection.LongitudinalOverrunMeters > tolerance)
                {
                    continue;
                }

                float parallelism = Vector3.Dot(point.Tangent, projection.Point.Tangent);
                int sign = parallelism > 0f ? 1 : -1;
                if (!(Mathf.Abs(parallelism) >= minParallelism) || (direction != 0 && sign != direction))
                {
                    return null;
                }

                direction = sign;

                // Bords vus depuis la droite du datum : un corridor de meme sens garde sa droite a
                // droite, un corridor oppose a la sienne a gauche.
                var trace = new DatumTrace();
                trace.S = projection.SMeters;
                trace.Lateral = projection.LateralOffsetMeters;
                trace.LowEdge = trace.Lateral - (sign > 0 ? point.HalfWidthLeftMeters : point.HalfWidthRightMeters);
                trace.HighEdge = trace.Lateral + (sign > 0 ? point.HalfWidthRightMeters : point.HalfWidthLeftMeters);
                grounded.Trace.Add(trace);
            }

            if (grounded.Trace.Count == 0)
            {
                return null;
            }

            grounded.Trace.Sort(delegate(DatumTrace a, DatumTrace b) { return a.S.CompareTo(b.S); });
            return grounded;
        }

        private static void ComparePair(
            List<RoadModelValidationIssue> issues,
            GroundedCorridor lower,
            GroundedCorridor upper,
            RoadModelValidationProfile profile)
        {
            float start = Mathf.Max(lower.Start, upper.Start);
            float end = Mathf.Min(lower.End, upper.End);
            if (!(end - start > profile.LengthToleranceMeters))
            {
                // Recouvrement vide ou reduit a un point : rien a comparer (ex. deux troncons en
                // continuation sur la meme file).
                return;
            }

            var checkpoints = new List<float>();
            checkpoints.Add(start);
            checkpoints.Add(end);
            AddInside(checkpoints, lower.Trace, start, end);
            AddInside(checkpoints, upper.Trace, start, end);
            checkpoints.Sort();

            for (int i = 0; i < checkpoints.Count; i++)
            {
                var a = Evaluate(lower.Trace, checkpoints[i]);
                var b = Evaluate(upper.Trace, checkpoints[i]);

                if (!(b.Lateral > a.Lateral))
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.NonMonotoneLateralOrder,
                        upper.Corridor.Id,
                        "LateralOrder " + upper.Corridor.LateralOrder + " a lateral " + Format(b.Lateral) + " sur le datum, pas a droite de "
                            + lower.Corridor.Id + " (ordre " + lower.Corridor.LateralOrder + ", lateral " + Format(a.Lateral) + ") a s=" + Format(checkpoints[i]) + "."));
                    return;
                }

                float overlap = a.HighEdge - b.LowEdge;
                if (!(overlap <= profile.EnvelopeOverlapToleranceMeters))
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.OverlappingLateralEnvelopes,
                        upper.Corridor.Id,
                        "Enveloppe recouvrant celle de " + lower.Corridor.Id + " de " + Format(overlap) + " m a s=" + Format(checkpoints[i]) + "."));
                    return;
                }
            }
        }

        private static void AddInside(List<float> checkpoints, List<DatumTrace> trace, float start, float end)
        {
            for (int i = 0; i < trace.Count; i++)
            {
                if (trace[i].S > start && trace[i].S < end)
                {
                    checkpoints.Add(trace[i].S);
                }
            }
        }

        /// <summary>Interpolation lineaire de la trace en abscisse du datum, bornee a ses extremites.</summary>
        private static DatumTrace Evaluate(List<DatumTrace> trace, float s)
        {
            if (s <= trace[0].S)
            {
                return trace[0];
            }

            for (int i = 1; i < trace.Count; i++)
            {
                if (s <= trace[i].S)
                {
                    var a = trace[i - 1];
                    var b = trace[i];
                    float span = b.S - a.S;
                    float t = span > 1e-6f ? (s - a.S) / span : 1f;
                    var result = new DatumTrace();
                    result.S = s;
                    result.Lateral = Mathf.LerpUnclamped(a.Lateral, b.Lateral, t);
                    result.LowEdge = Mathf.LerpUnclamped(a.LowEdge, b.LowEdge, t);
                    result.HighEdge = Mathf.LerpUnclamped(a.HighEdge, b.HighEdge, t);
                    return result;
                }
            }

            return trace[trace.Count - 1];
        }

        private static string Format(float value)
        {
            return value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
