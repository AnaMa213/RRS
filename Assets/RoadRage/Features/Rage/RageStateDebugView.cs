using TMPro;
using UnityEngine;

namespace RoadRage.Features.Rage
{
    /// <summary>
    /// HUD/dev minimal (Story 4.1) : affiche RageValue/Disposition d'une cible NetworkedRageState
    /// via Update() en lecture seule (la NetworkVariable est deja synchronisee cote client). Le
    /// [ContextMenu] editor-only applique un delta de test cote hote pour verifier visiblement le
    /// changement de palier dans Dev_RageSandbox. Hors scope : Features.Rage ne peut referencer
    /// aucune autre RoadRage.Features.* (cf. RunCheckpointHudScreen, Features.UI).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RageStateDebugView : MonoBehaviour
    {
        [SerializeField]
        private NetworkedRageState target;

        [SerializeField]
        private TMP_Text label;

        [SerializeField]
        [Tooltip("Tuning utilise par 'Apply Test Rage Delta' (editeur uniquement).")]
        private RageTuningDef testTuning;

        [SerializeField]
        [Tooltip("Delta applique par 'Apply Test Rage Delta' (editeur uniquement).")]
        private float testDelta = 25f;

        private void Update()
        {
            Render();
        }

        private void Render()
        {
            if (label == null)
            {
                return;
            }

            if (target == null)
            {
                label.text = "Rage : -";
                return;
            }

            label.text = "Rage : " + Mathf.RoundToInt(target.RageValue.Value) + " (" + target.Disposition.Value + ")";
        }

#if UNITY_EDITOR
        [ContextMenu("Apply Test Rage Delta")]
        private void ApplyTestRageDelta()
        {
            if (target == null || testTuning == null)
            {
                Debug.LogWarning("[Rage] RageStateDebugView : cible ou tuning manquant pour le delta de test.", this);
                return;
            }

            if (!target.IsSpawned || !target.IsServer)
            {
                Debug.LogWarning("[Rage] RageStateDebugView : delta de test disponible seulement cote hote, cible spawn.", this);
                return;
            }

            target.ApplyRageDelta(testDelta, testTuning);
        }
#endif
    }
}
