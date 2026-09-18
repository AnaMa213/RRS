using RoadRage.Features.Vehicles;
using UnityEngine;

namespace RoadRage.DevTools
{
    /// <summary>
    /// Story 5.11 : vue de telemetrie de la couche physique du vehicule, en build de developpement
    /// uniquement. Elle affiche, par roue, la compression de suspension et le contact au sol, plus
    /// l'etat de caisse (vitesse, vitesse laterale, glissement, angle de derive). C'est l'instrument
    /// de la verification de bordure : sans lui, « le vehicule monte la bordure » et « il ne decolle
    /// pas » ne se liraient qu'a l'oeil.
    ///
    /// Story 5.12 y ajoute la lecture de PNEU par roue : glissement longitudinal et angulaire, force
    /// transmise et adherence disponible (charge portee x coefficient). C'est l'instrument des
    /// controles humains de la story -- « la derive est atteignable et se referme » ne se lit nulle
    /// part ailleurs, et l'angle de roue effectif n'est lisible que par cette vue.
    ///
    /// LECTURE SEULE, AUCUN ETAT DE GAMEPLAY. La vue ne detient rien et ne mute rien : elle interroge
    /// <see cref="VehiclePhysicsBody"/> (qui publie deja son etat en lecture seule) et
    /// <c>Rigidbody.linearVelocity</c> via la fonction pure d'echantillonnage. Motif de
    /// <c>AIVehicleBehaviorDebugView</c> et <c>RageStateDebugView</c> : elle vit dans un assembly de
    /// developpement et ne participe pas au jeu.
    ///
    /// GARDE DE BUILD. L'assembly <c>RoadRage.DevTools</c> est compilee dans les builds joueur (ses
    /// contraintes de plateforme et de definition sont vides) : la garde est donc posee ici, au niveau
    /// source -- <c>Debug.isDebugBuild</c> est vrai dans l'Editeur et dans un build de developpement,
    /// faux dans un build de livraison. La vue s'y desactive d'elle-meme dans <c>OnEnable</c>, ce qui
    /// suffit : un composant desactive ne dessine rien et ne consomme rien.
    ///
    /// Aucune garde d'autorite : la vue ne lit que des valeurs deja calculees, et un client n'a rien a
    /// simuler pour les afficher.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VehiclePhysicsTelemetryView : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Couche physique observee. Laisser vide pour prendre celle de ce GameObject.")]
        private VehiclePhysicsBody target;

        [SerializeField]
        [Tooltip("Decalage local du panneau, au-dessus du vehicule.")]
        private Vector3 worldOffset = new Vector3(0f, 2.4f, 0f);

        [SerializeField]
        [Tooltip("Afficher le detail par roue (compression, contact, glissement, force, adherence).")]
        private bool showWheels = true;

        private bool isEnabledForBuild;

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<VehiclePhysicsBody>();
            }
        }

        private void OnEnable()
        {
            isEnabledForBuild = Debug.isDebugBuild;
            if (!isEnabledForBuild)
            {
                enabled = false;
            }
        }

        private void OnGUI()
        {
            if (!isEnabledForBuild || target == null || !target.HasProfile)
            {
                return;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            var screenPoint = camera.WorldToScreenPoint(transform.TransformPoint(worldOffset));
            if (screenPoint.z <= 0f)
            {
                return;
            }

            var text = ComposeText();
            var style = new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.UpperLeft };
            GUI.contentColor = Color.white;
            GUI.Label(new Rect(screenPoint.x + 8f, Screen.height - screenPoint.y, 320f, 160f), text, style);
        }

        /// <summary>
        /// Texte du panneau. Les absences sont ecrites explicitement -- aucune profil, aucune donnee
        /// de caisse -- plutot que masquees par des zeros qui se liraient comme des mesures.
        /// </summary>
        private string ComposeText()
        {
            var text = name + "  (Story 5.12)";

            if (!target.TrySampleTelemetry(out var sample))
            {
                text += "\ncaisse : Rigidbody absent";
            }
            else
            {
                text += "\nvitesse " + sample.Speed.ToString("F2") + " m/s"
                    + "   laterale " + sample.LateralSpeed.ToString("F2") + " m/s"
                    + "\nglissement " + (sample.Slip * 100f).ToString("F0") + " %"
                    + "   derive " + sample.SlipAngleDegrees.ToString("F1") + " deg"
                    + "   braquage " + target.CurrentSteerAngleDegrees.ToString("F1") + " deg";
            }

            if (!showWheels)
            {
                return text;
            }

            for (var i = 0; i < target.WheelCount; i++)
            {
                if (!target.TryGetWheelState(i, out var wheel))
                {
                    continue;
                }

                text += "\nroue " + i + " : " + (wheel.Grounded ? "au sol" : "en l'air")
                    + "   compression " + wheel.Compression.ToString("F3") + " m";

                if (!target.TryGetTireSample(i, out var tire))
                {
                    continue;
                }

                // Les valeurs qui decident du ressenti sont ecrites telles quelles : glissement et
                // force transmise pour la derive, adherence disponible pour lire a quel point la roue
                // est saturee. Sans ces trois lectures, « la voiture derape » et « elle tient » ne se
                // distinguent qu'a l'oeil.
                text += "\n     glissement " + tire.SlipRatio.ToString("F3")
                    + "   angle " + tire.SlipAngleDegrees.ToString("F1") + " deg"
                    + "\n     charge " + tire.NormalLoad.ToString("F0") + " N"
                    + "   force " + tire.ForceMagnitude.ToString("F0") + " N"
                    + " / adherence " + tire.MaximumForce.ToString("F0") + " N"
                    + " (" + (tire.GripUsage * 100f).ToString("F0") + " %)";
            }

            return text;
        }
    }
}
