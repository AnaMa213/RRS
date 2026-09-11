using System;
using RoadRage.Shared.Definitions;
using RoadRage.Shared.Domain;
using UnityEngine;

namespace RoadRage.Features.PassengerActions
{
    [CreateAssetMenu(fileName = "PassengerActionDef", menuName = "RoadRage/Passenger Actions/Action Def")]
    public sealed class PassengerActionDef : ScriptableObject
    {
        [SerializeField]
        private string id = string.Empty;

        [SerializeField]
        private string displayName = string.Empty;

        [SerializeField]
        [Range(0, PassengerActionCatalog.SlotCount - 1)]
        private int slot;

        [SerializeField]
        [Min(1)]
        private int version = 1;

        [SerializeField]
        [Min(0f)]
        private float cooldownSeconds = 1f;

        [SerializeField]
        [Min(0.01f)]
        private float maxRange = 10f;

        [SerializeField]
        private RunPhase[] allowedPhases = { RunPhase.NotStarted };

        public DefinitionId Id => new DefinitionId(id);
        public string RawId => id ?? string.Empty;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? RawId : displayName;
        public int Slot => slot;
        public int Version => version;
        public float CooldownSeconds => cooldownSeconds;
        public float MaxRange => maxRange;

        public bool AllowsPhase(RunPhase phase)
        {
            if (allowedPhases == null)
            {
                return false;
            }

            for (var i = 0; i < allowedPhases.Length; i++)
            {
                if (allowedPhases[i] == phase)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
