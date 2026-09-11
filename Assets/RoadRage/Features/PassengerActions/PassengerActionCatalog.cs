using System.Collections.Generic;
using System.Text;
using RoadRage.Shared.Definitions;
using UnityEngine;

namespace RoadRage.Features.PassengerActions
{
    [CreateAssetMenu(fileName = "PassengerActionCatalog", menuName = "RoadRage/Passenger Actions/Action Catalog")]
    public sealed class PassengerActionCatalog : ScriptableObject
    {
        public const int SlotCount = 3;

        [SerializeField]
        [Min(1)]
        private int version = 1;

        [SerializeField]
        private List<PassengerActionDef> actions = new List<PassengerActionDef>();

        public int Version => version;
        public int Count => actions == null ? 0 : actions.Count;

        public PassengerActionDef GetAtSlot(int slot)
        {
            if (actions == null || slot < 0 || slot >= SlotCount)
            {
                return null;
            }

            for (var i = 0; i < actions.Count; i++)
            {
                if (actions[i] != null && actions[i].Slot == slot)
                {
                    return actions[i];
                }
            }

            return null;
        }

        public bool TryGetById(DefinitionId id, out PassengerActionDef action)
        {
            action = null;
            if (actions == null || id.IsEmpty)
            {
                return false;
            }

            for (var i = 0; i < actions.Count; i++)
            {
                var candidate = actions[i];
                if (candidate != null && candidate.Id == id)
                {
                    action = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryValidate(out string error)
        {
            if (version < 1)
            {
                error = "Version de catalogue invalide.";
                return false;
            }

            if (actions == null || actions.Count != SlotCount)
            {
                error = "Le catalogue doit contenir exactement trois actions.";
                return false;
            }

            var ids = new HashSet<string>();
            var slots = new bool[SlotCount];
            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (action == null)
                {
                    error = "Action nulle a l'index " + i + ".";
                    return false;
                }

                var id = action.RawId;
                if (string.IsNullOrWhiteSpace(id) || id != id.Trim() || id != id.ToLowerInvariant())
                {
                    error = "Id d'action invalide a l'index " + i + ".";
                    return false;
                }

                if (Encoding.UTF8.GetByteCount(id) > 32 || !ids.Add(id))
                {
                    error = "Id d'action trop long ou duplique : " + id + ".";
                    return false;
                }

                if (action.Slot < 0 || action.Slot >= SlotCount || slots[action.Slot])
                {
                    error = "Slot d'action invalide ou duplique : " + action.Slot + ".";
                    return false;
                }

                if (action.Version < 1 || action.CooldownSeconds < 0f || action.MaxRange <= 0f)
                {
                    error = "Version, cooldown ou portee invalide pour " + id + ".";
                    return false;
                }

                slots[action.Slot] = true;
            }

            error = string.Empty;
            return true;
        }

        private void OnValidate()
        {
            if (!TryValidate(out var error))
            {
                Debug.LogWarning("[PassengerActions] Catalogue invalide : " + error, this);
            }
        }
    }
}
