using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoadRage.Features.PassengerActions
{
    [DisallowMultipleComponent]
    public sealed class PassengerActionDebugView : MonoBehaviour
    {
        private PassengerActionCatalog catalog;
        private Action<int> requestSlot;
        private bool allowNumericKeys;
        private string verdict = "Aucun verdict.";

        public void Bind(PassengerActionCatalog actionCatalog, Action<int> onRequestSlot, bool numericKeys)
        {
            catalog = actionCatalog;
            requestSlot = onRequestSlot;
            allowNumericKeys = numericKeys;
        }

        public void ShowVerdict(PassengerActionVerdict value)
        {
            verdict = value.Message;
        }

        private void Update()
        {
            if (!allowNumericKeys || Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.digit1Key.wasPressedThisFrame) requestSlot?.Invoke(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) requestSlot?.Invoke(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) requestSlot?.Invoke(2);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(12f, 12f, 320f, 150f), GUI.skin.box);
            GUILayout.Label("Actions passager");
            for (var slot = 0; slot < PassengerActionCatalog.SlotCount; slot++)
            {
                var action = catalog == null ? null : catalog.GetAtSlot(slot);
                GUI.enabled = requestSlot != null && action != null;
                if (GUILayout.Button((slot + 1) + " - " + (action == null ? "indisponible" : action.DisplayName)))
                {
                    requestSlot?.Invoke(slot);
                }
            }

            GUI.enabled = true;
            GUILayout.Label(verdict);
            GUILayout.EndArea();
        }
    }
}
