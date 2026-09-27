#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace RoadRage.Features.Vehicles.Traffic.Migration
{
    /// <summary>
    /// Story 5.28 -- revue de l'overlay et sign-off Gate A par le proprietaire. La fenetre relance le
    /// pipeline sur MVP_Run, refuse de signer si un artefact committe differe du pipeline frais,
    /// dessine les primitives d'overlay (les memes que le texte canonique), fait cadrer et cocher
    /// chaque instance, puis n'ecrit le sign-off qu'apres confirmation explicite. Aucun autre chemin
    /// n'ecrit de sign-off.
    /// </summary>
    public sealed class GateAReviewWindow : EditorWindow
    {
        private AuthoredRun _run;
        private readonly List<string> _blocking = new List<string>();
        private readonly HashSet<string> _reviewed = new HashSet<string>(StringComparer.Ordinal);
        private string _selected;
        private string _approver;
        private string _approverEmail;
        private Vector2 _scroll;

        /// <summary>
        /// Vue filtree des conflits de l'instance cadree (revue du proprietaire) : une vue sur les
        /// memes primitives, jamais de nouvelles ; le texte d'overlay hache n'en depend pas.
        /// </summary>
        private enum ConflictFilter
        {
            All,
            Approach,
            Movement,
            Zone
        }

        private ConflictFilter _filter;
        private int _pick;

        [MenuItem("RoadRage/Traffic V2/Revue Gate A (overlay MVP_Run)")]
        public static void Open()
        {
            GetWindow<GateAReviewWindow>("Gate A").Show();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += Draw;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= Draw;
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Revue de l'overlay MVP_Run : cadrer et cocher chacune des instances, puis signer. La signature lie votre identite Git aux hashes du pipeline frais.", MessageType.Info);
            if (GUILayout.Button("Charger le pipeline frais"))
            {
                Load();
            }

            if (_run == null)
            {
                return;
            }

            foreach (var reason in _blocking)
            {
                EditorGUILayout.HelpBox(reason, MessageType.Error);
            }

            EditorGUILayout.LabelField("RoadModelVersion", _run.Binding == null ? "-" : _run.Binding.RoadModelVersion);
            EditorGUILayout.LabelField("Overlay", _run.Binding == null ? "-" : _run.Binding.OverlayHash);
            EditorGUILayout.LabelField("Instances revues", _reviewed.Count + " / " + _run.Overlay.Count);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var instance in _run.Overlay)
            {
                EditorGUILayout.BeginHorizontal();
                bool reviewed = EditorGUILayout.ToggleLeft(instance.Label + " (" + instance.ModuleKind + ")", _reviewed.Contains(instance.Key));
                if (reviewed)
                {
                    _reviewed.Add(instance.Key);
                }
                else
                {
                    _reviewed.Remove(instance.Key);
                }

                if (GUILayout.Button("Cadrer", GUILayout.Width(70)) && SceneView.lastActiveSceneView != null)
                {
                    _selected = instance.Key;
                    _filter = ConflictFilter.All;
                    _pick = 0;
                    SceneView.lastActiveSceneView.Frame(instance.Bounds, false);
                    SceneView.RepaintAll();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            ConflictFilterGui();

            bool complete = _blocking.Count == 0 && _reviewed.Count == _run.Overlay.Count && !string.IsNullOrEmpty(_approver);
            EditorGUILayout.LabelField("Approbateur (git config)", string.IsNullOrEmpty(_approver) ? "absent : configurer git user.name" : _approver + " <" + _approverEmail + ">");
            using (new EditorGUI.DisabledScope(!complete))
            {
                if (GUILayout.Button("Signer la Gate A"))
                {
                    Sign();
                }
            }
        }

        private void Load()
        {
            _run = null;
            _blocking.Clear();
            _reviewed.Clear();
            _approver = Git("config user.name");
            _approverEmail = Git("config user.email");

            var scene = SceneManager.GetSceneByPath(MigrationReport.ScenePath);
            string refusal = MigrationReport.RefusalReason(EditorApplication.isPlayingOrWillChangePlaymode, scene.IsValid() && scene.isLoaded, scene.IsValid() && scene.isDirty);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                refusal = "Ouvrir MVP_Run dans l'Editeur : la revue se fait sur le district, dans la Scene view.";
            }

            if (refusal != null)
            {
                _run = new AuthoredRun();
                _blocking.Add(refusal);
                return;
            }

            _run = AuthoredRoadModel.Run(scene, AuthoredRoadModel.ReadIfExists(MigrationReport.LineageFullPath),
                AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(AuthoredRoadModel.DecisionsPath)));
            if (!_run.Succeeded)
            {
                _blocking.AddRange(_run.Failures);
                return;
            }

            // Signer un artefact committe perime lierait le sign-off a autre chose que ce qui est revu.
            _blocking.AddRange(AuthoredRoadModel.VerifyArtifacts(_run,
                AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(AuthoredRoadModel.ModelPath)),
                AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(AuthoredRoadModel.OverlayPath))));
            _blocking.AddRange(AuthoredRoadModel.VerifyReport(AuthoredRoadModel.ReadIfExists(AuthoredRoadModel.FullPath(AuthoredRoadModel.ReportPath)), _run));
            SceneView.RepaintAll();
        }

        private void Sign()
        {
            string message = "Je, " + _approver + " <" + _approverEmail + ">, ai revu les " + _run.Overlay.Count + " instances de l'overlay MVP_Run "
                + "et approuve le modele " + _run.Binding.RoadModelVersion + " (overlay " + _run.Binding.OverlayHash + ").";
            if (!EditorUtility.DisplayDialog("Signer la Gate A", message, "Signer", "Annuler"))
            {
                return;
            }

            var instances = new List<string>(_reviewed);
            string text = AuthoredRoadModel.RenderSignoff(_run, _approver, _approverEmail, instances, DateTime.UtcNow);
            string error;
            if (!AuthoredRoadModel.TryWriteAll(new[] { new KeyValuePair<string, string>(AuthoredRoadModel.FullPath(AuthoredRoadModel.SignoffPath), text) }, out error))
            {
                Debug.LogError("[Traffic V2] Sign-off non ecrit : " + error);
                return;
            }

            AssetDatabase.ImportAsset(AuthoredRoadModel.SignoffPath);
            Debug.Log("[Traffic V2] Sign-off Gate A ecrit : " + AuthoredRoadModel.SignoffPath + ".");
        }

        private void Draw(SceneView view)
        {
            if (_run == null || _run.Overlay.Count == 0)
            {
                return;
            }

            foreach (var instance in _run.Overlay)
            {
                bool selected = instance.Key == _selected;
                HashSet<RoadId> focus = null;
                HashSet<RoadId> partners = null;
                var visible = selected ? VisibleZones(instance, out focus, out partners) : null;
                foreach (var primitive in instance.Primitives)
                {
                    Handles.color = ColorOf(primitive.Kind, selected);
                    if (visible != null && primitive.Kind == "zone")
                    {
                        if (!visible.Contains(primitive.Subject))
                        {
                            continue;
                        }

                        Handles.DrawWireCube(primitive.Points[0], 2f * primitive.Points[1]);
                        Handles.Label(primitive.Points[0], ZoneLabel(primitive.Subject));
                        continue;
                    }

                    if (visible != null && primitive.Kind == "mouvement")
                    {
                        bool isFocus = focus.Contains(primitive.Subject);
                        if (isFocus || partners.Contains(primitive.Subject))
                        {
                            // Membres des zones affichees : couleurs distinctes, trait epais.
                            Handles.color = isFocus ? new Color(1f, 0.55f, 0f) : new Color(0.3f, 0.5f, 1f);
                            Handles.DrawAAPolyLine(primitive.Note == "centre" ? 5f : 2f, primitive.Points);
                            continue;
                        }

                        Handles.color = new Color(1f, 1f, 0f, 0.08f);
                    }

                    if (primitive.IsBox)
                    {
                        Handles.DrawWireCube(primitive.Points[0], 2f * primitive.Points[1]);
                    }
                    else if (primitive.Points.Length == 1)
                    {
                        Handles.DrawWireDisc(primitive.Points[0], Vector3.up, 0.6f);
                        if (selected)
                        {
                            Handles.Label(primitive.Points[0], primitive.Note);
                        }
                    }
                    else
                    {
                        Handles.DrawPolyLine(primitive.Points);
                    }
                }
            }
        }

        // ============================================================ filtre des conflits

        private OverlayInstance SelectedInstance()
        {
            return _run == null || !_run.Succeeded ? null : _run.Overlay.Find(delegate(OverlayInstance i) { return i.Key == _selected; });
        }

        private static List<RoadId> Subjects(OverlayInstance instance, string kind)
        {
            var subjects = new List<RoadId>();
            foreach (var primitive in instance.Primitives)
            {
                if (primitive.Kind == kind && !subjects.Contains(primitive.Subject))
                {
                    subjects.Add(primitive.Subject);
                }
            }

            return subjects;
        }

        private void ConflictFilterGui()
        {
            var instance = SelectedInstance();
            var zones = instance == null ? null : Subjects(instance, "zone");
            if (zones == null || zones.Count == 0)
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Conflits de " + instance.Label + " (" + zones.Count + " zones)", EditorStyles.boldLabel);
            var filter = (ConflictFilter)EditorGUILayout.EnumPopup("Filtre", _filter);
            if (filter != _filter)
            {
                _filter = filter;
                _pick = 0;
            }

            var options = Options(instance);
            if (options.Count > 0)
            {
                var labels = new string[options.Count];
                for (int i = 0; i < options.Count; i++)
                {
                    labels[i] = OptionLabel(options[i]);
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(28)))
                {
                    _pick = (_pick + options.Count - 1) % options.Count;
                }

                _pick = EditorGUILayout.Popup(Mathf.Clamp(_pick, 0, options.Count - 1), labels);
                if (GUILayout.Button(">", GUILayout.Width(28)))
                {
                    _pick = (_pick + 1) % options.Count;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField((_pick + 1) + " / " + options.Count);
            }

            HashSet<RoadId> focus;
            HashSet<RoadId> partners;
            var visible = VisibleZones(instance, out focus, out partners);
            if (visible != null)
            {
                EditorGUILayout.HelpBox("Zones affichees : " + visible.Count + ". Orange : mouvement(s) filtre(s) ; bleu : partenaires de conflit.", MessageType.None);
            }

            SceneView.RepaintAll();
        }

        /// <summary>Choix du filtre courant : controles (approches), mouvements ou zones de l'instance.</summary>
        private List<RoadId> Options(OverlayInstance instance)
        {
            switch (_filter)
            {
                case ConflictFilter.Approach:
                    return Subjects(instance, "controle");
                case ConflictFilter.Movement:
                    return Subjects(instance, "mouvement");
                case ConflictFilter.Zone:
                    return Subjects(instance, "zone");
                default:
                    return new List<RoadId>();
            }
        }

        private string OptionLabel(RoadId id)
        {
            CompiledJunctionControl control;
            if (_filter == ConflictFilter.Approach && _run.Compiled.TryGetControl(id, out control))
            {
                CompiledJunctionMovement first;
                _run.Compiled.TryGetMovement(control.ControlledMovementIds[0], out first);
                return "Approche " + Short(id) + " (" + control.ControlledMovementIds.Count + " mvts, ex. " + first.Label + ")";
            }

            // Un menu deroulant interprete « / » comme un sous-menu : separateurs neutres.
            return (_filter == ConflictFilter.Zone ? ZoneLabel(id).Replace("\n", "  ") : MovementLabel(id)).Replace("/", "-");
        }

        /// <summary>
        /// Zones visibles de l'instance cadree (nul = tout dessiner), mouvements filtres (focus) et
        /// partenaires des zones visibles.
        /// </summary>
        private HashSet<RoadId> VisibleZones(OverlayInstance instance, out HashSet<RoadId> focus, out HashSet<RoadId> partners)
        {
            focus = new HashSet<RoadId>();
            partners = new HashSet<RoadId>();
            var options = Options(instance);
            if (_filter == ConflictFilter.All || options.Count == 0 || _run.Compiled == null)
            {
                return null;
            }

            RoadId picked = options[Mathf.Clamp(_pick, 0, options.Count - 1)];
            CompiledJunctionControl control;
            if (_filter == ConflictFilter.Approach && _run.Compiled.TryGetControl(picked, out control))
            {
                focus.UnionWith(control.ControlledMovementIds);
            }
            else if (_filter == ConflictFilter.Movement)
            {
                focus.Add(picked);
            }

            var instanceZones = Subjects(instance, "zone");
            var visible = new HashSet<RoadId>();
            foreach (var zone in _run.Compiled.ConflictZones)
            {
                var members = zone.MemberMovementIds;
                bool shown = instanceZones.Contains(zone.Id)
                    && (_filter == ConflictFilter.Zone ? zone.Id == picked : focus.Contains(members[0]) || focus.Contains(members[1]));
                if (!shown)
                {
                    continue;
                }

                visible.Add(zone.Id);
                if (_filter == ConflictFilter.Zone)
                {
                    focus.Add(members[0]);
                    partners.Add(members[1]);
                    continue;
                }

                foreach (var member in members)
                {
                    if (!focus.Contains(member))
                    {
                        partners.Add(member);
                    }
                }
            }

            return visible;
        }

        private string ZoneLabel(RoadId zoneId)
        {
            string decision = "sans decision";
            foreach (var conflict in _run.Decisions.Conflicts)
            {
                if (conflict.Id == zoneId)
                {
                    decision = conflict.Decision.ToString();
                }
            }

            foreach (var zone in _run.Compiled.ConflictZones)
            {
                if (zone.Id == zoneId)
                {
                    return "zone " + Short(zoneId) + " (" + decision + ")\n" + MovementLabel(zone.MemberMovementIds[0]) + "\n x " + MovementLabel(zone.MemberMovementIds[1]);
                }
            }

            return "zone " + Short(zoneId);
        }

        private string MovementLabel(RoadId id)
        {
            CompiledJunctionMovement movement;
            return _run.Compiled.TryGetMovement(id, out movement) ? movement.Label : Short(id);
        }

        private static string Short(RoadId id)
        {
            return id.ToString().Substring(0, 8);
        }

        private static Color ColorOf(string kind, bool selected)
        {
            Color color;
            switch (kind)
            {
                case "corridor":
                    color = Color.cyan;
                    break;
                case "mouvement":
                    color = Color.yellow;
                    break;
                case "zone":
                    color = Color.red;
                    break;
                case "frontiere":
                    color = Color.white;
                    break;
                case "portail":
                    color = Color.green;
                    break;
                default:
                    color = Color.magenta;
                    break;
            }

            color.a = selected ? 1f : 0.35f;
            return color;
        }

        internal static string Git(string arguments)
        {
            try
            {
                var info = new ProcessStartInfo("git", arguments);
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.CreateNoWindow = true;
                using (var process = Process.Start(info))
                {
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                    return process.ExitCode == 0 ? output : null;
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return null;
            }
        }
    }
}
#endif
