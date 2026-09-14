using RoadRage.Shared.Domain;
using TMPro;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Label debug lecture seule (Story 5.4), sur le modele de RageStateDebugView : affiche le
    /// comportement synchronise de CE vehicule (<see cref="NetworkedAIVehicleState.Behavior"/>), donc
    /// identiquement sur l'hote et sur un client -- la vue n'ecrit jamais dans une NetworkVariable et
    /// n'envoie aucune RPC. L'etat est rendu par du texte, pas par une couleur (AC accessibilite).
    ///
    /// Si aucun <see cref="TMP_Text"/> n'est cable, la vue cree son propre label monde au-dessus du
    /// vehicule et l'oriente vers la camera : le prefab IA suffit alors a rendre l'etat visible dans
    /// MVP_Run sans cabler un libelle par instance.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AIVehicleBehaviorDebugView : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Etat IA observe. Laisser vide pour prendre celui de ce GameObject.")]
        private NetworkedAIVehicleState target;

        [SerializeField]
        [Tooltip("Libelle cible. Laisser vide pour qu'un label monde soit cree au-dessus du vehicule.")]
        private TMP_Text label;

        [SerializeField]
        [Tooltip("Decalage local du label monde cree automatiquement.")]
        private Vector3 labelOffset = new Vector3(0f, 2.6f, 0f);

        private bool ownsLabel;
        private bool hasRageSource;
        private Camera billboardCamera;

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<NetworkedAIVehicleState>();
            }

            hasRageSource = GetComponent<IRageDispositionSource>() != null;

            if (label == null)
            {
                label = CreateWorldLabel();
                ownsLabel = label != null;
            }
        }

        private void LateUpdate()
        {
            if (label == null)
            {
                return;
            }

            label.text = ComposeText();

            if (ownsLabel)
            {
                FaceCamera();
            }
        }

        /// <summary>
        /// Texte du label : comportement synchrone, et mention explicite des absences (etat IA ou etat
        /// de rage manquant) plutot qu'un "Calm" silencieux qui masquerait un vehicule mal compose.
        /// </summary>
        private string ComposeText()
        {
            if (target == null)
            {
                return "IA : etat absent";
            }

            var text = "IA : " + target.Behavior.Value;
            return hasRageSource ? text : text + " (rage absente)";
        }

        private TMP_Text CreateWorldLabel()
        {
            var holder = new GameObject("AIBehaviorDebugLabel");
            holder.transform.SetParent(transform, false);
            holder.transform.localPosition = labelOffset;

            var text = holder.AddComponent<TextMeshPro>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 3f;
            text.rectTransform.sizeDelta = new Vector2(6f, 1f);
            return text;
        }

        private void FaceCamera()
        {
            if (billboardCamera == null)
            {
                billboardCamera = Camera.main;
            }

            if (billboardCamera == null)
            {
                return;
            }

            var toCamera = label.transform.position - billboardCamera.transform.position;
            if (toCamera.sqrMagnitude > 0.0001f)
            {
                label.transform.rotation = Quaternion.LookRotation(toCamera, Vector3.up);
            }
        }
    }
}
