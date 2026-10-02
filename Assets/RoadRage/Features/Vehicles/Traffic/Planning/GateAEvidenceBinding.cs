using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Planning
{
    // GateAEvidenceMissing reste a 0 : un GateAEvidenceResult par defaut ne vaut jamais preuve valide.
    public enum GateAEvidenceStatus { GateAEvidenceMissing, Valid, GateAEvidenceStale }

    /// <summary>
    /// Modele de pose nominale sur lequel une preuve Gate A a ete calculee (contrat §8, 2026-09-29).
    /// Toute preuve signee avant la Story 5.52 est TangentAligned : elle ne certifie aucune couverture
    /// physique sous la pose nominale cinematique.
    /// </summary>
    public enum NominalPoseModel { TangentAligned = 0, Kinematic = 1 }

    public readonly struct GateAEvidenceResult
    {
        public readonly GateAEvidenceStatus Status;
        public readonly float TrackingAllowanceMeters;
        /// <summary>Modele de pose enregistre par la preuve.</summary>
        public readonly NominalPoseModel PoseModel;
        public readonly IReadOnlyList<string> SignedRingSeams;
        public bool Valid { get { return Status == GateAEvidenceStatus.Valid; } }

        internal GateAEvidenceResult(GateAEvidenceStatus status, float allowance,
            NominalPoseModel poseModel = NominalPoseModel.TangentAligned, string[] signedRingSeams = null)
        {
            Status = status; TrackingAllowanceMeters = allowance; PoseModel = poseModel;
            SignedRingSeams = Array.AsReadOnly(signedRingSeams ?? new string[0]);
        }
    }

    /// <summary>Documentary binding only; physical Gate A validation stays with its owner.</summary>
    public static class GateAEvidenceBinding
    {
        [Serializable]
        private sealed class Signoff
        {
            public int Format;
            public string RoadModelVersion;
            public string ModelHash;
            public string ClearanceHash;
            public float TrackingAllowanceMeters;
            public float TrackingToleranceMeters;
            public float MaximumAbsolutePlanningOffsetMeters;
            public string PoseModel;
            public string EvidenceParametersHash;
            public int ConflictSweepAlgorithmVersion;
            public int FingerprintSchemaVersion;
            public string[] SignedRingSeams;
        }

        public static GateAEvidenceResult Bind(CompiledRoadModel model, string modelText,
            string signoffText, string reportText, string currentEvidenceParametersHash = null)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (string.IsNullOrEmpty(modelText) || string.IsNullOrEmpty(signoffText)
                || string.IsNullOrEmpty(reportText))
                return new GateAEvidenceResult(GateAEvidenceStatus.GateAEvidenceMissing, 0f);

            Signoff signoff;
            try { signoff = JsonUtility.FromJson<Signoff>(signoffText); }
            catch (ArgumentException) { return new GateAEvidenceResult(GateAEvidenceStatus.GateAEvidenceMissing, 0f); }
            if (signoff == null || string.IsNullOrEmpty(signoff.RoadModelVersion)
                || string.IsNullOrEmpty(signoff.ModelHash) || string.IsNullOrEmpty(signoff.ClearanceHash))
                return new GateAEvidenceResult(GateAEvidenceStatus.GateAEvidenceMissing, 0f);

            string normalized = reportText.Replace("\r\n", "\n");
            int heading = normalized.IndexOf("### Residus\n", StringComparison.Ordinal);
            int marker = heading < 0 ? -1 : normalized.IndexOf("\na_e = ", heading, StringComparison.Ordinal);
            int start = marker < 0 ? -1 : marker + 1;
            if (start < 0)
                return new GateAEvidenceResult(GateAEvidenceStatus.GateAEvidenceMissing, 0f);
            int table = normalized.IndexOf("\n| Genre |", start, StringComparison.Ordinal);
            int end = table < 0 ? -1 : normalized.IndexOf("\n\n", table, StringComparison.Ordinal);
            if (end < 0) return new GateAEvidenceResult(GateAEvidenceStatus.GateAEvidenceMissing, 0f);
            string block = normalized.Substring(start, end - start + 1);
            int lineEnd = block.IndexOf('\n');
            string first = block.Substring(6, lineEnd - 6);
            int unit = first.IndexOf(' ');
            float allowance;
            if (unit < 0 || !float.TryParse(first.Substring(0, unit), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out allowance)
                || float.IsNaN(allowance) || float.IsInfinity(allowance) || allowance < 0f)
                return new GateAEvidenceResult(GateAEvidenceStatus.GateAEvidenceMissing, 0f);

            if (reportText != normalized || signoff.RoadModelVersion != model.Version.ToString()
                || !string.Equals(signoff.ModelHash, Hash(modelText), StringComparison.OrdinalIgnoreCase)
                || !string.Equals(signoff.ClearanceHash, Hash(block), StringComparison.OrdinalIgnoreCase))
                return new GateAEvidenceResult(GateAEvidenceStatus.GateAEvidenceStale, allowance);
            if (signoff.Format == 3)
            {
                if (signoff.PoseModel != "kinematic-v1" || signoff.TrackingAllowanceMeters != allowance
                    || signoff.TrackingToleranceMeters + signoff.MaximumAbsolutePlanningOffsetMeters != allowance
                    || string.IsNullOrEmpty(signoff.EvidenceParametersHash)
                    || (currentEvidenceParametersHash != null
                        && !string.Equals(signoff.EvidenceParametersHash, currentEvidenceParametersHash, StringComparison.OrdinalIgnoreCase))
                    || !block.Contains("pose-model = " + signoff.PoseModel + " ; ")
                    || !block.Contains("max|o| = " + signoff.MaximumAbsolutePlanningOffsetMeters.ToString("R", CultureInfo.InvariantCulture)
                        + " m ; epsilon_t = " + signoff.TrackingToleranceMeters.ToString("R", CultureInfo.InvariantCulture) + " m\n")
                    || !block.Contains("parametres-hash = " + signoff.EvidenceParametersHash + "\n")
                    || signoff.ConflictSweepAlgorithmVersion != 2 || signoff.FingerprintSchemaVersion != 2
                    || signoff.SignedRingSeams == null || signoff.SignedRingSeams.Length != 24
                    || !block.Contains("raccords-signes = 24\n"))
                    return new GateAEvidenceResult(GateAEvidenceStatus.GateAEvidenceStale, allowance);
                var unique = new HashSet<string>(StringComparer.Ordinal);
                foreach (string seam in signoff.SignedRingSeams)
                    if (string.IsNullOrEmpty(seam) || !unique.Add(seam)
                        || !block.Contains("raccord-signe = " + seam + "\n"))
                        return new GateAEvidenceResult(GateAEvidenceStatus.GateAEvidenceStale, allowance);
                return new GateAEvidenceResult(GateAEvidenceStatus.Valid, allowance,
                    NominalPoseModel.Kinematic, signoff.SignedRingSeams);
            }

            return signoff.Format == 0 || signoff.Format == 2
                ? new GateAEvidenceResult(GateAEvidenceStatus.Valid, allowance)
                : new GateAEvidenceResult(GateAEvidenceStatus.GateAEvidenceStale, allowance);
        }

        private static string Hash(string text)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                var result = new StringBuilder(bytes.Length * 2);
                for (int i = 0; i < bytes.Length; i++) result.Append(bytes[i].ToString("x2", CultureInfo.InvariantCulture));
                return result.ToString();
            }
        }
    }
}
