using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic.Planning
{
    public enum GateAEvidenceStatus { Valid, GateAEvidenceMissing, GateAEvidenceStale }

    public readonly struct GateAEvidenceResult
    {
        public readonly GateAEvidenceStatus Status;
        public readonly float TrackingAllowanceMeters;
        public bool Valid { get { return Status == GateAEvidenceStatus.Valid; } }

        internal GateAEvidenceResult(GateAEvidenceStatus status, float allowance)
        { Status = status; TrackingAllowanceMeters = allowance; }
    }

    /// <summary>Documentary binding only; physical Gate A validation stays with its owner.</summary>
    public static class GateAEvidenceBinding
    {
        [Serializable]
        private sealed class Signoff
        {
            public string RoadModelVersion;
            public string ModelHash;
            public string ClearanceHash;
        }

        public static GateAEvidenceResult Bind(CompiledRoadModel model, string modelText,
            string signoffText, string reportText)
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
            return new GateAEvidenceResult(GateAEvidenceStatus.Valid, allowance);
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
