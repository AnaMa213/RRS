using RoadRage.Shared.Domain;
using TMPro;
using UnityEngine;

namespace RoadRage.Features.Rage
{
    /// <summary>
    /// HUD/dev minimal (Story 4.1, etendue Story 5.1) : affiche RageValue/Disposition et FearValue
    /// d'une cible NetworkedRageState via Update() en lecture seule (les NetworkVariables sont deja
    /// synchronisees cote client). Les [ContextMenu] editor-only appliquent un delta de rage ou un
    /// effet de reaction cote hote pour verifier visiblement les deux canaux dans Dev_RageSandbox.
    /// Hors scope : Features.Rage ne peut referencer aucune autre RoadRage.Features.* (cf.
    /// RunCheckpointHudScreen, Features.UI).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RageStateDebugView : MonoBehaviour
    {
        [SerializeField]
        private NetworkedRageState target;

        [SerializeField]
        private TMP_Text label;

        [SerializeField]
        [Tooltip("Tuning utilise par les declencheurs de test (editeur uniquement).")]
        private RageTuningDef testTuning;

        [SerializeField]
        [Tooltip("Delta (rage) ou magnitude (effet de reaction) applique par les declencheurs de test (editeur uniquement).")]
        private float testDelta = 25f;

        [SerializeField]
        [Tooltip("Canal vise par 'Apply Test Reaction Effect' (editeur uniquement). 'Fear' seul sert a verifier que la rage ne bouge pas.")]
        private ReactionChannel testReactionChannel = ReactionChannel.Both;

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
                label.text = "Rage : - | Peur : -";
                return;
            }

            label.text = "Rage : " + Mathf.RoundToInt(target.RageValue.Value) + " (" + target.Disposition.Value + ")"
                + " | Peur : " + Mathf.RoundToInt(target.FearValue.Value);
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

        /// <summary>
        /// Declencheur dev de la Story 5.1 : applique un effet sur le canal choisi
        /// (testReactionChannel, Both par defaut) pour verifier les deux canaux dans le sandbox.
        /// </summary>
        [ContextMenu("Apply Test Reaction Effect")]
        private void ApplyTestReactionEffect()
        {
            if (target == null || testTuning == null)
            {
                Debug.LogWarning("[Rage] RageStateDebugView : cible ou tuning manquant pour l'effet de test.", this);
                return;
            }

            if (!target.IsSpawned || !target.IsServer)
            {
                Debug.LogWarning("[Rage] RageStateDebugView : effet de test disponible seulement cote hote, cible spawn.", this);
                return;
            }

            target.ApplyReactionEffect(new NpcReactionEffect(testReactionChannel, testDelta), testTuning);
        }
#endif
    }
}
