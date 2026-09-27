#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    /// <summary>
    /// Story 5.50 -- revue des paires de conflit par le proprietaire. La fenetre relance le pipeline
    /// sur MVP_Run, construit le differentiel structure (table historique verifiee, modele format 1
    /// lie), isole une paire dans la Scene view (anciennes et nouvelles trajectoires, volumes,
    /// empreintes temoins, noeuds V1) et n'ecrit les decisions qu'apres un geste explicite du
    /// proprietaire confirme par dialogue. Aucun autre chemin n'appelle <see cref="PairReviewActions"/>.
    /// </summary>
    public sealed class PairReviewWindow : EditorWindow
    {
        public const string MenuPath = "RoadRage/Traffic V2/Revue des paires 5.50";

        public enum StatusFilter
        {
            Tous,
            Modifiees,
            Nouvelles,
            Retirees,
            Inchangees,
            SansDecisionConfirmee
        }

        private PairReviewModel _review;
        private string _error;
        private string _selected;
        private StatusFilter _status;
        private string _junction = string.Empty;
        private string _search = string.Empty;
        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private bool _showOld = true;
        private bool _showNew = true;
        private bool _showVolumes = true;
        private bool _showWitness = true;
        private bool _showEnvelope;
        private bool _showNodes = true;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<PairReviewWindow>("Paires 5.50").Show();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += Draw;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= Draw;
        }

        // ============================================================ fonctions pures (testees)

        public static List<PairReviewEntry> Filter(IList<PairReviewEntry> entries, StatusFilter status, string junction, string search)
        {
            var list = new List<PairReviewEntry>();
            foreach (var entry in entries)
            {
                if (!Matches(entry, status))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(junction) && entry.JunctionLabel != junction)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(search)
                    && (entry.LabelA + " " + entry.LabelB + " " + entry.PairKey).IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                list.Add(entry);
            }

            return list;
        }

        private static bool Matches(PairReviewEntry entry, StatusFilter status)
        {
            switch (status)
            {
                case StatusFilter.Modifiees:
                    return entry.Status == PairReviewStatus.Modified;
                case StatusFilter.Nouvelles:
                    return entry.Status == PairReviewStatus.New;
                case StatusFilter.Retirees:
                    return entry.Status == PairReviewStatus.Removed;
                case StatusFilter.Inchangees:
                    return entry.Status == PairReviewStatus.Unchanged;
                case StatusFilter.SansDecisionConfirmee:
                    return entry.DecisionState != PairDecisionState.Confirmed;
                default:
                    return true;
            }
        }

        public static string Title(PairReviewEntry entry)
        {
            return "[" + StatusLabel(entry.Status) + "] " + entry.JunctionLabel + " : " + entry.LabelA + " x " + entry.LabelB
                + " (" + DecisionLabel(entry) + ")";
        }

        public static string StatusLabel(PairReviewStatus status)
        {
            switch (status)
            {
                case PairReviewStatus.Modified:
                    return "modifiee";
                case PairReviewStatus.New:
                    return "nouvelle";
                case PairReviewStatus.Removed:
                    return "retiree";
                default:
                    return "inchangee";
            }
        }

        public static string DecisionLabel(PairReviewEntry entry)
        {
            string kind = entry.HasDecision ? entry.Decision.Decision.ToString() : "sans decision";
            switch (entry.DecisionState)
            {
                case PairDecisionState.Confirmed:
                    return kind + ", reconfirmee";
                case PairDecisionState.Stale:
                    return kind + ", empreinte perimee";
                case PairDecisionState.Unconfirmed:
                    return kind + ", a reconfirmer";
                case PairDecisionState.Orphan:
                    return kind + ", orpheline";
                default:
                    return "a decider";
            }
        }

        /// <summary>Cles des noeuds V1 source d'une cle de mouvement (« movement:&lt;depart&gt;&gt;&lt;arrivee&gt; »).</summary>
        public static string[] SourceNodeKeys(string movementKey)
        {
            const string prefix = "movement:";
            if (string.IsNullOrEmpty(movementKey) || !movementKey.StartsWith(prefix, StringComparison.Ordinal))
            {
                return new string[0];
            }

            return movementKey.Substring(prefix.Length).Split('>');
        }

        /// <summary>
        /// Actions offertes au proprietaire pour cette paire, dans l'ordre d'affichage. Accepter,
        /// rejeter ou ajouter une decision n'y figure plus : le format 4 exige une revision et une
        /// preuve emises par le plan deterministe approuve.
        /// </summary>
        public static List<string> AvailableActions(PairReviewEntry entry)
        {
            var actions = new List<string>();
            if (entry.Status == PairReviewStatus.Removed)
            {
                if (entry.HasDecision)
                {
                    actions.Add("Disposer la decision orpheline");
                }

                return actions;
            }

            if (entry.HasDecision && entry.DecisionState != PairDecisionState.Confirmed)
            {
                actions.Add("Reconfirmer cette paire");
            }

            return actions;
        }

        // ============================================================ interface

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Revue 5.50 des paires de conflit. Lecture seule tant que vous ne cliquez pas une action : chaque action "
                + "reecrit uniquement " + AuthoredRoadModel.DecisionsPath + " apres confirmation. Le differentiel reste "
                + "provisoire tant qu'il n'a pas ete regenere et verifie localement.",
                MessageType.Info);
            if (GUILayout.Button("Charger le differentiel frais"))
            {
                Load();
            }

            if (_error != null)
            {
                EditorGUILayout.HelpBox(_error, MessageType.Error);
            }

            if (_review == null)
            {
                return;
            }

            EditorGUILayout.LabelField("Historiques / fraiches", _review.HistoricalPairs + " / " + _review.FreshPairs);
            EditorGUILayout.LabelField("Modifiees / nouvelles / retirees / inchangees",
                _review.Count(PairReviewStatus.Modified) + " / " + _review.Count(PairReviewStatus.New) + " / "
                + _review.Count(PairReviewStatus.Removed) + " / " + _review.Count(PairReviewStatus.Unchanged));
            EditorGUILayout.LabelField("Ecartees par la distance exacte / suivi / echecs fermes",
                _review.EnvelopeOnly.Count + " / " + _review.Following.Count + " / " + _review.FailClosed.Count);

            _status = (StatusFilter)EditorGUILayout.EnumPopup("Statut", _status);
            var junctions = new List<string> { string.Empty };
            foreach (var entry in _review.Entries)
            {
                if (!junctions.Contains(entry.JunctionLabel))
                {
                    junctions.Add(entry.JunctionLabel);
                }
            }

            int picked = Mathf.Max(0, junctions.IndexOf(_junction));
            var junctionLabels = junctions.ConvertAll(delegate(string j) { return j.Length == 0 ? "(tous les carrefours)" : j.Replace("/", "-"); });
            _junction = junctions[EditorGUILayout.Popup("Carrefour", picked, junctionLabels.ToArray())];
            _search = EditorGUILayout.TextField("Recherche", _search);

            var visible = Filter(_review.Entries, _status, _junction, _search);
            EditorGUILayout.LabelField("Paires affichees", visible.Count.ToString());
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll, GUILayout.Height(180));
            foreach (var entry in visible)
            {
                bool isSelected = entry.PairKey == _selected;
                if (GUILayout.Toggle(isSelected, Title(entry), "Button") && !isSelected)
                {
                    _selected = entry.PairKey;
                    Frame(entry);
                }
            }

            EditorGUILayout.EndScrollView();
            PairReviewEntry current = _selected == null ? null : _review.Find(_selected);
            if (current != null)
            {
                Detail(current);
            }

            OwnerBatchGui();
            SceneView.RepaintAll();
        }

        private void Detail(PairReviewEntry entry)
        {
            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            EditorGUILayout.LabelField(entry.JunctionLabel + " -- " + StatusLabel(entry.Status), EditorStyles.boldLabel);
            EditorGUILayout.LabelField("A (orange)", entry.LabelA);
            EditorGUILayout.LabelField("B (bleu)", entry.LabelB);
            EditorGUILayout.SelectableLabel(entry.KeyA + "\n" + entry.KeyB, EditorStyles.textArea, GUILayout.Height(36));
            EditorGUILayout.HelpBox(entry.Reason, MessageType.None);
            EditorGUILayout.LabelField("Decision", DecisionLabel(entry));
            if (entry.HasDecision)
            {
                EditorGUILayout.LabelField("Motif actuel", entry.Decision.Reason);
            }

            EditorGUILayout.LabelField("Decision historique", entry.HistoricalDecision ?? "-");
            EditorGUILayout.LabelField("Empreinte ancienne", entry.OldFingerprint ?? "-");
            EditorGUILayout.LabelField("Empreinte fraiche", entry.NewFingerprint ?? "-");
            EditorGUILayout.LabelField("Volume ancien (gris)", entry.HasOldVolume ? PairReview.FormatVolume(entry.OldVolume) : "-");
            EditorGUILayout.LabelField("Volume nouveau (rouge)", entry.HasNewVolume ? PairReview.FormatVolume(entry.NewVolume) : "-");
            EditorGUILayout.LabelField("Changements", "A " + (entry.GeometryAChanged ? "modifiee" : "inchangee")
                + " ; B " + (entry.GeometryBChanged ? "modifiee" : "inchangee")
                + " ; volume " + (entry.VolumeChanged ? "modifie" : "inchange"));
            if (entry.Sweep != null)
            {
                EditorGUILayout.LabelField("Relation fraiche", entry.Sweep.Relation.ToString());
                EditorGUILayout.LabelField("Marge exacte / englobante",
                    PairReview.Meters(entry.Sweep.ExactSlackMeters) + " / " + PairReview.Meters(entry.Sweep.EnvelopeSlackMeters));
                if (entry.Sweep.FailClosedReason != null)
                {
                    EditorGUILayout.HelpBox("Echec ferme : " + entry.Sweep.FailClosedReason, MessageType.Warning);
                }
            }

            DrivabilityGui("A", entry.NewSamplesA, entry.OldSamplesA);
            DrivabilityGui("B", entry.NewSamplesB, entry.OldSamplesB);

            EditorGUILayout.BeginHorizontal();
            _showOld = GUILayout.Toggle(_showOld, "Anciennes", "Button");
            _showNew = GUILayout.Toggle(_showNew, "Nouvelles", "Button");
            _showVolumes = GUILayout.Toggle(_showVolumes, "Volumes", "Button");
            _showWitness = GUILayout.Toggle(_showWitness, "Temoins", "Button");
            _showEnvelope = GUILayout.Toggle(_showEnvelope, "Enveloppes", "Button");
            _showNodes = GUILayout.Toggle(_showNodes, "Noeuds V1", "Button");
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Cadrer la paire"))
            {
                Frame(entry);
            }

            OwnerPairGui(entry);
            EditorGUILayout.EndScrollView();
        }

        private void DrivabilityGui(string side, IReadOnlyList<RoadCurveSample> fresh, IReadOnlyList<RoadCurveSample> old)
        {
            DrivabilityProfile profile = _review.Model.DrivabilityProfile;
            float ceiling = PairReview.MinCeilingMetersPerSecond(fresh, profile);
            EditorGUILayout.LabelField("Conduisibilite " + side,
                "rayon min " + PairReview.Meters(PairReview.MinRadiusMeters(fresh))
                + " (ancien " + PairReview.Meters(PairReview.MinRadiusMeters(old)) + "), plafond "
                + (float.IsNaN(ceiling) ? "refuse" : float.IsPositiveInfinity(ceiling) ? "aucun" : ceiling.ToString("F2") + " m/s"));
        }

        private void OwnerPairGui(PairReviewEntry entry)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Action du proprietaire", EditorStyles.boldLabel);
            var actions = AvailableActions(entry);
            if (actions.Count == 0)
            {
                EditorGUILayout.HelpBox("Aucune action : aucune decision a reconfirmer pour cette paire.", MessageType.None);
                return;
            }

            foreach (var action in actions)
            {
                if (GUILayout.Button(action))
                {
                    Apply(entry, action);
                }
            }
        }

        private void OwnerBatchGui()
        {
            int unchanged = 0;
            foreach (var entry in _review.Entries)
            {
                if (entry.Status == PairReviewStatus.Unchanged && entry.HasDecision && entry.DecisionState != PairDecisionState.Confirmed)
                {
                    unchanged++;
                }
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(unchanged == 0))
            {
                if (GUILayout.Button("Reconfirmer les paires inchangees (" + unchanged + ")"))
                {
                    string message = "Reconfirmer " + unchanged + " paire(s) dont la geometrie des deux mouvements et le volume de "
                        + "conflit sont identiques a la table historique ? Aucune paire modifiee, nouvelle ou retiree n'est touchee.";
                    if (EditorUtility.DisplayDialog("Reconfirmer les paires inchangees", message, "Reconfirmer", "Annuler"))
                    {
                        Write(delegate(string text)
                        {
                            int count;
                            return PairReviewActions.ReconfirmUnchanged(text, _review, out count);
                        });
                    }
                }
            }
        }

        private void Apply(PairReviewEntry entry, string action)
        {
            string identity = GateAReviewWindow.Git("config user.name") ?? "proprietaire";
            string message = identity + ", " + action.ToLowerInvariant() + " pour " + entry.JunctionLabel + " :\n"
                + entry.LabelA + "\n x " + entry.LabelB + "\n\n" + entry.Reason;
            if (!EditorUtility.DisplayDialog(action, message, "Confirmer", "Annuler"))
            {
                return;
            }

            Write(delegate(string text)
            {
                switch (action)
                {
                    case "Reconfirmer cette paire":
                        return PairReviewActions.ReconfirmPair(text, entry);
                    case "Disposer la decision orpheline":
                        return PairReviewActions.DisposeRemoved(text, entry);
                    default:
                        throw new InvalidOperationException("Action inconnue : " + action);
                }
            });
        }

        private void Write(Func<string, string> transform)
        {
            string path = AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath);
            string updated;
            try
            {
                updated = transform(AuthoredRoadModel.ReadIfExists(path));
            }
            catch (Exception exception)
            {
                Debug.LogError("[Traffic V2] Decision refusee, rien n'est ecrit : " + exception.Message);
                return;
            }

            string error;
            if (!AuthoredRoadModel.TryWriteAll(new[] { new KeyValuePair<string, string>(path, updated) }, out error))
            {
                Debug.LogError("[Traffic V2] Decisions non ecrites : " + error);
                return;
            }

            AssetDatabase.ImportAsset(AuthoredRoadModel.DecisionsPath);
            Debug.Log("[Traffic V2] Decision du proprietaire ecrite dans " + AuthoredRoadModel.DecisionsPath + ".");
            string keep = _selected;
            Load();
            _selected = keep;
        }

        private void Load()
        {
            _review = null;
            _error = null;
            var scene = SceneManager.GetSceneByPath(MigrationReport.ScenePath);
            string refusal = MigrationReport.RefusalReason(EditorApplication.isPlayingOrWillChangePlaymode, scene.IsValid() && scene.isLoaded, scene.IsValid() && scene.isDirty);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                refusal = "Ouvrir MVP_Run dans l'Editeur : la revue se fait sur le district, dans la Scene view.";
            }

            if (refusal != null)
            {
                _error = refusal;
                return;
            }

            try
            {
                string baseline = AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(PairGeometryFingerprint.BaselineModelPath));
                var historical = PairGeometryFingerprint.VerifyHistoricalTable(
                    AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(PairGeometryFingerprint.HistoricalPath)),
                    AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(PairGeometryFingerprint.BaselineHashesPath)),
                    baseline);
                var run = AuthoredRoadModel.Run(scene, AuthoredRoadModel.ReadIfExists(MigrationReport.LineageFullPath),
                    AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath)));
                _review = PairReview.Build(run, historical, baseline);
            }
            catch (FormatException exception)
            {
                _error = exception.Message;
            }

            SceneView.RepaintAll();
        }

        private void Frame(PairReviewEntry entry)
        {
            var bounds = new Bounds();
            bool any = false;
            foreach (var samples in new[] { entry.NewSamplesA, entry.NewSamplesB, entry.OldSamplesA, entry.OldSamplesB })
            {
                if (samples == null)
                {
                    continue;
                }

                foreach (var sample in samples)
                {
                    if (!any)
                    {
                        bounds = new Bounds(sample.Position, Vector3.one);
                        any = true;
                    }

                    bounds.Encapsulate(sample.Position);
                }
            }

            if (any && SceneView.lastActiveSceneView != null)
            {
                bounds.Expand(6f);
                SceneView.lastActiveSceneView.Frame(bounds, false);
            }
        }

        // ============================================================ vue d'isolement

        private void Draw(SceneView view)
        {
            PairReviewEntry entry = _review == null || _selected == null ? null : _review.Find(_selected);
            if (entry == null)
            {
                return;
            }

            float admission = _review.Model.DrivabilityProfile.Declared
                ? RoadModelCompiler.AdmissionRadiusMeters(_review.Model.DrivabilityProfile)
                : 0f;
            if (_showOld)
            {
                Dotted(entry.OldSamplesA, new Color(1f, 0.75f, 0.4f, 0.9f));
                Dotted(entry.OldSamplesB, new Color(0.55f, 0.7f, 1f, 0.9f));
            }

            if (_showNew)
            {
                Trajectory(entry.NewSamplesA, new Color(1f, 0.55f, 0f), admission, "A : " + entry.LabelA);
                Trajectory(entry.NewSamplesB, new Color(0.3f, 0.5f, 1f), admission, "B : " + entry.LabelB);
            }

            if (_showEnvelope)
            {
                Envelope(entry.NewSamplesA, new Color(1f, 0.55f, 0f, 0.5f));
                Envelope(entry.NewSamplesB, new Color(0.3f, 0.5f, 1f, 0.5f));
            }

            if (_showVolumes)
            {
                if (entry.HasOldVolume)
                {
                    Handles.color = new Color(0.7f, 0.7f, 0.7f, 0.9f);
                    Handles.DrawWireCube(entry.OldVolume.Center, 2f * entry.OldVolume.Extents);
                }

                if (entry.HasNewVolume)
                {
                    Handles.color = Color.red;
                    Handles.DrawWireCube(entry.NewVolume.Center, 2f * entry.NewVolume.Extents);
                }
            }

            if (_showWitness && entry.Sweep != null && entry.Sweep.HasWitness)
            {
                var profile = _review.Model.ValidationProfile;
                Footprint(entry.Sweep.WitnessA, profile, new Color(1f, 0.55f, 0f));
                Footprint(entry.Sweep.WitnessB, profile, new Color(0.3f, 0.5f, 1f));
            }

            if (_showNodes)
            {
                Nodes(entry.KeyA);
                Nodes(entry.KeyB);
            }
        }

        private static void Dotted(IReadOnlyList<RoadCurveSample> samples, Color color)
        {
            if (samples == null)
            {
                return;
            }

            Handles.color = color;
            for (int i = 1; i < samples.Count; i++)
            {
                Handles.DrawDottedLine(samples[i - 1].Position, samples[i].Position, 3f);
            }
        }

        private static void Trajectory(IReadOnlyList<RoadCurveSample> samples, Color color, float admission, string label)
        {
            if (samples == null || samples.Count < 2)
            {
                return;
            }

            for (int i = 1; i < samples.Count; i++)
            {
                float curvature = Mathf.Max(Mathf.Abs(samples[i - 1].CurvaturePerMeter), Mathf.Abs(samples[i].CurvaturePerMeter));
                bool refused = admission > 0f && curvature > 0f && 1f / curvature < admission;
                Handles.color = refused ? Color.red : color;
                Handles.DrawAAPolyLine(refused ? 7f : 4f, samples[i - 1].Position, samples[i].Position);
            }

            Handles.color = Color.green;
            Handles.DrawWireDisc(samples[0].Position, Vector3.up, 0.5f);
            Handles.color = Color.red;
            Handles.DrawWireDisc(samples[samples.Count - 1].Position, Vector3.up, 0.5f);
            Handles.color = color;
            float next = samples[0].SMeters;
            foreach (var sample in samples)
            {
                if (sample.SMeters < next)
                {
                    continue;
                }

                next = sample.SMeters + 2f;
                Vector3 right = Vector3.Cross(Vector3.up, sample.Tangent).normalized * 0.35f;
                Vector3 back = -sample.Tangent.normalized * 0.5f;
                Handles.DrawLine(sample.Position, sample.Position + back + right);
                Handles.DrawLine(sample.Position, sample.Position + back - right);
            }

            Handles.Label(samples[samples.Count / 2].Position + Vector3.up * 0.5f,
                label + "\nrayon min " + PairReview.Meters(PairReview.MinRadiusMeters(samples)));
        }

        private static void Envelope(IReadOnlyList<RoadCurveSample> samples, Color color)
        {
            if (samples == null || samples.Count < 2)
            {
                return;
            }

            var left = new Vector3[samples.Count];
            var right = new Vector3[samples.Count];
            for (int i = 0; i < samples.Count; i++)
            {
                Vector3 side = Vector3.Cross(samples[i].Up, samples[i].Tangent).normalized;
                left[i] = samples[i].Position - side * samples[i].HalfWidthLeftMeters;
                right[i] = samples[i].Position + side * samples[i].HalfWidthRightMeters;
            }

            Handles.color = color;
            Handles.DrawPolyLine(left);
            Handles.DrawPolyLine(right);
        }

        private static void Footprint(SweepPose pose, RoadModelValidationProfile profile, Color color)
        {
            var corners = ConflictSweep.Corners(pose, ConflictSweep.HalfLength(profile), profile.MaxVehicleHalfWidthMeters);
            var points = new Vector3[5];
            for (int i = 0; i < 5; i++)
            {
                Vector2 c = corners[i % 4];
                points[i] = new Vector3(c.x, pose.Position.y + 0.05f, c.y);
            }

            Handles.color = color;
            Handles.DrawAAPolyLine(3f, points);
        }

        private void Nodes(string movementKey)
        {
            if (_review.Import == null || _review.Import.SourceSet == null)
            {
                return;
            }

            foreach (var key in SourceNodeKeys(movementKey))
            {
                V1Node node = _review.Import.SourceSet.Nodes.Find(delegate(V1Node n) { return n.Key == key; });
                if (node == null)
                {
                    continue;
                }

                Handles.color = Color.magenta;
                Handles.DrawWireDisc(node.Position, Vector3.up, 0.3f);
                Handles.Label(node.Position + Vector3.up * 0.3f, node.Label);
            }
        }
    }
}
#endif
