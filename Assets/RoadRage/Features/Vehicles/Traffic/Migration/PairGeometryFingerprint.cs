#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    /// <summary>Empreinte pure et versionnee d'une paire de mouvements et de son volume de conflit.</summary>
    public static class PairGeometryFingerprint
    {
        public const int FingerprintSchemaVersion = 1;
        public const int HistoricalTableFormat = 1;
        public const string HistoricalPath = "_bmad-output/implementation-artifacts/v1-regression-5-50/historical-pair-fingerprints.json";
        public const string BaselineModelPath = "_bmad-output/implementation-artifacts/v1-regression-5-50/baseline-road-model-format1.json";
        public const string BaselineHashesPath = "_bmad-output/implementation-artifacts/v1-regression-5-50/baseline-hashes.txt";

        private const string PairTag = "RRS-PAIR-GEOMETRY";
        private const string MovementTag = "RRS-MOVEMENT-GEOMETRY";
        public const string HistoricalHashLabel = "historical-pair-fingerprints-sha256";

        public static HistoricalPairFingerprintTable VerifyHistoricalTable(
            string tableText,
            string baselineHashesText,
            string baselineModelText)
        {
            if (string.IsNullOrEmpty(tableText) || string.IsNullOrEmpty(baselineHashesText)
                || string.IsNullOrEmpty(baselineModelText))
            {
                throw new FormatException("Table historique 5.50 : table ou liaison de reference absente.");
            }

            string expectedHash = null;
            string[] lines = baselineHashesText.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].StartsWith(HistoricalHashLabel + " ", StringComparison.Ordinal))
                {
                    continue;
                }

                int separator = lines[i].LastIndexOf(": ", StringComparison.Ordinal);
                if (separator >= 0)
                {
                    expectedHash = lines[i].Substring(separator + 2).Trim();
                }
            }

            string actualHash = V1SourceSet.Sha256Hex(tableText);
            if (!IsSha256(expectedHash) || !string.Equals(expectedHash, actualHash, StringComparison.Ordinal))
            {
                throw new FormatException("Table historique 5.50 alteree : SHA-256 " + actualHash
                    + ", attendu " + (expectedHash ?? "absent") + ".");
            }

            HistoricalPairFingerprintTable table = HistoricalPairFingerprintTable.Parse(tableText);
            string modelHash = V1SourceSet.Sha256Hex(baselineModelText);
            if (!string.Equals(table.SourceModelSha256, modelHash, StringComparison.Ordinal))
            {
                throw new FormatException("Table historique 5.50 : modele source " + table.SourceModelSha256
                    + ", attendu " + modelHash + ".");
            }

            return table;
        }

        internal static bool IsSha256(string value)
        {
            if (value == null || value.Length != 64)
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
                {
                    return false;
                }
            }

            return true;
        }

        public static string Compute(
            string keyA,
            IReadOnlyList<RoadCurveSample> samplesA,
            string keyB,
            IReadOnlyList<RoadCurveSample> samplesB,
            RoadBoundsBox volume)
        {
            Order(ref keyA, ref samplesA, ref keyB, ref samplesB);
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(PairTag);
                writer.Write(FingerprintSchemaVersion);
                writer.Write(keyA);
                writer.Write(keyB);
                WriteSamples(writer, samplesA);
                WriteSamples(writer, samplesB);
                WriteVolume(writer, volume);
                writer.Flush();
                return Sha256Hex(stream.ToArray());
            }
        }

        public static string ComputeMovement(string key, IReadOnlyList<RoadCurveSample> samples)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(MovementTag);
                writer.Write(FingerprintSchemaVersion);
                writer.Write(key);
                WriteSamples(writer, samples);
                writer.Flush();
                return Sha256Hex(stream.ToArray());
            }
        }

        public static HistoricalPairFingerprintTable Capture(
            V1SourceSet set,
            string lineageText,
            string decisionsText,
            string baselineModelText,
            out string error)
        {
            error = null;
            if (set == null || string.IsNullOrEmpty(lineageText) || string.IsNullOrEmpty(decisionsText)
                || string.IsNullOrEmpty(baselineModelText))
            {
                error = "Capture historique 5.50 : entree absente.";
                return null;
            }

            RoadModelSource baselineSource;
            try
            {
                baselineSource = RoadModelDocument.Load(baselineModelText);
            }
            catch (Exception exception)
            {
                error = "Capture historique 5.50 : modele format 1 refuse : " + exception.Message;
                return null;
            }

            var run = AuthoredRoadModel.Run(set, lineageText, decisionsText);
            if (!run.Succeeded)
            {
                error = "Capture historique 5.50 : pipeline courant refuse :\n- " + string.Join("\n- ", run.Failures.ToArray());
                return null;
            }

            RoadModelVersion baselineVersion = RoadModelCompiler.Compile(baselineSource).Version;
            if (run.Compiled.Version != baselineVersion)
            {
                error = "Capture historique 5.50 : version fraiche " + run.Compiled.Version
                    + " differente du ModelVersion format 1 " + baselineVersion + ".";
                return null;
            }

            Dictionary<RoadId, string> keyById = AuthoringDecisions.KeysById(run.Import);
            var candidateByPair = new Dictionary<string, ConflictCandidate>(StringComparer.Ordinal);
            for (int i = 0; i < run.Candidates.Count; i++)
            {
                ConflictCandidate candidate = run.Candidates[i];
                candidateByPair.Add(AuthoredRoadModel.PairKey(keyById[candidate.MovementA], keyById[candidate.MovementB]), candidate);
            }

            var records = new List<HistoricalPairFingerprintRecord>(run.Decisions.Conflicts.Count);
            for (int i = 0; i < run.Decisions.Conflicts.Count; i++)
            {
                ConflictDecision decision = run.Decisions.Conflicts[i];
                string pair = AuthoredRoadModel.PairKey(decision.MovementKeyA, decision.MovementKeyB);
                ConflictCandidate candidate;
                if (!candidateByPair.TryGetValue(pair, out candidate))
                {
                    error = "Capture historique 5.50 : decision sans candidat courant : " + pair.Replace("\n", " x ") + ".";
                    return null;
                }

                CompiledJunctionMovement a;
                CompiledJunctionMovement b;
                if (!run.Compiled.TryGetMovement(candidate.MovementA, out a) || !run.Compiled.TryGetMovement(candidate.MovementB, out b))
                {
                    error = "Capture historique 5.50 : mouvement compile introuvable pour " + pair.Replace("\n", " x ") + ".";
                    return null;
                }

                string keyA = keyById[a.Id];
                string keyB = keyById[b.Id];
                IReadOnlyList<RoadCurveSample> samplesA = a.Samples;
                IReadOnlyList<RoadCurveSample> samplesB = b.Samples;
                Order(ref keyA, ref samplesA, ref keyB, ref samplesB);
                records.Add(HistoricalPairFingerprintRecord.Create(
                    keyA,
                    keyB,
                    decision.Decision.ToString(),
                    ComputeMovement(keyA, samplesA),
                    ComputeMovement(keyB, samplesB),
                    candidate.Volume,
                    Compute(keyA, samplesA, keyB, samplesB, candidate.Volume)));
            }

            records.Sort(delegate(HistoricalPairFingerprintRecord a, HistoricalPairFingerprintRecord b)
            {
                int first = string.CompareOrdinal(a.MovementKeyA, b.MovementKeyA);
                return first != 0 ? first : string.CompareOrdinal(a.MovementKeyB, b.MovementKeyB);
            });

            return new HistoricalPairFingerprintTable
            {
                Format = HistoricalTableFormat,
                FingerprintSchemaVersion = FingerprintSchemaVersion,
                SourceModelSha256 = V1SourceSet.Sha256Hex(baselineModelText),
                SourceModelVersion = baselineVersion.ToString(),
                Pairs = records.ToArray()
            };
        }

        [MenuItem("RoadRage/Traffic V2/Capturer les empreintes historiques 5.50")]
        public static void CaptureHistoricalMenu()
        {
            string historicalFullPath = AuthoredRoadModel.FullPath(HistoricalPath);
            if (File.Exists(historicalFullPath))
            {
                Debug.LogError("[Traffic V2] " + HistoricalPath + " existe deja : aucune capture historique n'est ecrasee.");
                return;
            }

            AuthoredRoadModel.WithMvpRun(delegate(Scene scene)
            {
                string baseline = AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(BaselineModelPath));
                string error;
                HistoricalPairFingerprintTable table = Capture(
                    V1SourceSet.Extract(scene),
                    AuthoredRoadModel.ReadIfExists(MigrationReport.LineageFullPath),
                    AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath)),
                    baseline,
                    out error);
                if (table == null)
                {
                    Debug.LogError("[Traffic V2] " + error);
                    return;
                }

                string tableText = table.Serialize();
                string hashes = AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(BaselineHashesPath));
                if (string.IsNullOrEmpty(hashes) || hashes.Contains(HistoricalHashLabel + " "))
                {
                    Debug.LogError("[Traffic V2] Capture historique 5.50 : baseline-hashes absent ou deja lie ; rien n'est ecrit.");
                    return;
                }

                hashes = hashes.TrimEnd('\r', '\n') + "\n" + HistoricalHashLabel + " (" + HistoricalPath + "): "
                    + V1SourceSet.Sha256Hex(tableText) + "\n";
                if (!AuthoredRoadModel.TryWriteAll(new[]
                    {
                        new KeyValuePair<string, string>(historicalFullPath, tableText),
                        new KeyValuePair<string, string>(AuthoredRoadModel.FullPath(BaselineHashesPath), hashes)
                    }, out error))
                {
                    Debug.LogError("[Traffic V2] Capture historique 5.50 refusee : " + error);
                    return;
                }

                Debug.Log("[Traffic V2] Empreintes historiques 5.50 capturees : " + table.Pairs.Length + " paires.");
            });
        }

        private static void Order(
            ref string keyA,
            ref IReadOnlyList<RoadCurveSample> samplesA,
            ref string keyB,
            ref IReadOnlyList<RoadCurveSample> samplesB)
        {
            if (string.CompareOrdinal(keyA, keyB) <= 0)
            {
                return;
            }

            string key = keyA;
            keyA = keyB;
            keyB = key;
            IReadOnlyList<RoadCurveSample> samples = samplesA;
            samplesA = samplesB;
            samplesB = samples;
        }

        private static void WriteSamples(BinaryWriter writer, IReadOnlyList<RoadCurveSample> samples)
        {
            if (samples == null)
            {
                writer.Write(0);
                return;
            }

            writer.Write(samples.Count);
            for (int i = 0; i < samples.Count; i++)
            {
                RoadCurveSample sample = samples[i];
                writer.Write(Quantize(sample.SMeters, RoadModelCanonicalWriter.MeterStep));
                WriteVector(writer, sample.Position, RoadModelCanonicalWriter.MeterStep);
                WriteVector(writer, sample.Tangent, RoadModelCanonicalWriter.DirectionStep);
                WriteVector(writer, sample.Up, RoadModelCanonicalWriter.DirectionStep);
                writer.Write(Quantize(sample.CurvaturePerMeter, RoadModelCanonicalWriter.CurvatureStep));
                writer.Write(Quantize(sample.HalfWidthLeftMeters, RoadModelCanonicalWriter.MeterStep));
                writer.Write(Quantize(sample.HalfWidthRightMeters, RoadModelCanonicalWriter.MeterStep));
            }
        }

        private static void WriteVolume(BinaryWriter writer, RoadBoundsBox volume)
        {
            WriteVector(writer, volume.Center, RoadModelCanonicalWriter.MeterStep);
            WriteVector(writer, volume.Extents, RoadModelCanonicalWriter.MeterStep);
        }

        private static void WriteVector(BinaryWriter writer, Vector3 value, double step)
        {
            writer.Write(Quantize(value.x, step));
            writer.Write(Quantize(value.y, step));
            writer.Write(Quantize(value.z, step));
        }

        private static long Quantize(float value, double step)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException("value", value, "Valeur finie attendue pour l'empreinte de paire.");
            }

            return (long)Math.Round((value == 0f ? 0d : value) / step, MidpointRounding.AwayFromZero);
        }

        private static string Sha256Hex(byte[] bytes)
        {
            byte[] digest;
            using (var sha = SHA256.Create())
            {
                digest = sha.ComputeHash(bytes);
            }

            var text = new StringBuilder(digest.Length * 2);
            for (int i = 0; i < digest.Length; i++)
            {
                text.Append(digest[i].ToString("x2"));
            }

            return text.ToString();
        }
    }

    [Serializable]
    public sealed class HistoricalPairFingerprintTable
    {
        public int Format;
        public int FingerprintSchemaVersion;
        public string SourceModelSha256;
        public string SourceModelVersion;
        public HistoricalPairFingerprintRecord[] Pairs = new HistoricalPairFingerprintRecord[0];

        public string Serialize()
        {
            return JsonUtility.ToJson(this, true).Replace("\r\n", "\n") + "\n";
        }

        public static HistoricalPairFingerprintTable Parse(string json)
        {
            HistoricalPairFingerprintTable table;
            try
            {
                table = JsonUtility.FromJson<HistoricalPairFingerprintTable>(json);
            }
            catch (Exception exception)
            {
                throw new FormatException("Table historique 5.50 illisible : " + exception.Message);
            }

            if (table == null || table.Format != PairGeometryFingerprint.HistoricalTableFormat
                || table.FingerprintSchemaVersion != PairGeometryFingerprint.FingerprintSchemaVersion
                || table.Pairs == null || string.IsNullOrEmpty(table.SourceModelSha256)
                || string.IsNullOrEmpty(table.SourceModelVersion))
            {
                throw new FormatException("Table historique 5.50 : format, schema ou liaison invalide.");
            }

            if (!PairGeometryFingerprint.IsSha256(table.SourceModelSha256))
            {
                throw new FormatException("Table historique 5.50 : SHA-256 du modele source invalide.");
            }

            string previous = null;
            var pairs = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < table.Pairs.Length; i++)
            {
                HistoricalPairFingerprintRecord record = table.Pairs[i];
                if (record == null || string.IsNullOrEmpty(record.MovementKeyA)
                    || string.IsNullOrEmpty(record.MovementKeyB)
                    || string.CompareOrdinal(record.MovementKeyA, record.MovementKeyB) >= 0
                    || (record.Decision != ConflictDecisionKind.Accepted.ToString()
                        && record.Decision != ConflictDecisionKind.Rejected.ToString())
                    || !PairGeometryFingerprint.IsSha256(record.MovementHashA)
                    || !PairGeometryFingerprint.IsSha256(record.MovementHashB)
                    || !PairGeometryFingerprint.IsSha256(record.Fingerprint)
                    || record.ExtentXMillimeters < 0 || record.ExtentYMillimeters < 0
                    || record.ExtentZMillimeters < 0)
                {
                    throw new FormatException("Table historique 5.50 : enregistrement " + i + " invalide.");
                }

                string pair = AuthoredRoadModel.PairKey(record.MovementKeyA, record.MovementKeyB);
                if (!pairs.Add(pair) || (previous != null && string.CompareOrdinal(previous, pair) >= 0))
                {
                    throw new FormatException("Table historique 5.50 : paire dupliquee ou ordre non canonique.");
                }

                previous = pair;
            }

            return table;
        }
    }

    [Serializable]
    public sealed class HistoricalPairFingerprintRecord
    {
        public string MovementKeyA;
        public string MovementKeyB;
        public string Decision;
        public string MovementHashA;
        public string MovementHashB;
        public long CenterXMillimeters;
        public long CenterYMillimeters;
        public long CenterZMillimeters;
        public long ExtentXMillimeters;
        public long ExtentYMillimeters;
        public long ExtentZMillimeters;
        public string Fingerprint;

        public static HistoricalPairFingerprintRecord Create(
            string keyA,
            string keyB,
            string decision,
            string movementHashA,
            string movementHashB,
            RoadBoundsBox volume,
            string fingerprint)
        {
            return new HistoricalPairFingerprintRecord
            {
                MovementKeyA = keyA,
                MovementKeyB = keyB,
                Decision = decision,
                MovementHashA = movementHashA,
                MovementHashB = movementHashB,
                CenterXMillimeters = Millimeters(volume.Center.x),
                CenterYMillimeters = Millimeters(volume.Center.y),
                CenterZMillimeters = Millimeters(volume.Center.z),
                ExtentXMillimeters = Millimeters(volume.Extents.x),
                ExtentYMillimeters = Millimeters(volume.Extents.y),
                ExtentZMillimeters = Millimeters(volume.Extents.z),
                Fingerprint = fingerprint
            };
        }

        public RoadBoundsBox Volume()
        {
            return new RoadBoundsBox
            {
                Center = new Vector3(Meters(CenterXMillimeters), Meters(CenterYMillimeters), Meters(CenterZMillimeters)),
                Extents = new Vector3(Meters(ExtentXMillimeters), Meters(ExtentYMillimeters), Meters(ExtentZMillimeters))
            };
        }

        private static long Millimeters(float value)
        {
            return (long)Math.Round(value / RoadModelCanonicalWriter.MeterStep, MidpointRounding.AwayFromZero);
        }

        private static float Meters(long value)
        {
            return (float)(value * RoadModelCanonicalWriter.MeterStep);
        }
    }
}
#endif
