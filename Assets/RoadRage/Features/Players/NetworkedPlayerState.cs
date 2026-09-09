using RoadRage.Shared.Domain;
using RoadRage.Shared.Networking;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Players
{
    /// <summary>
    /// Squelette host-owned du mode, du siege et de la vie joueur. CharacterId (Story 2.5) porte
    /// l'id de personnage resolu par le host au spawn reseau, pour les besoins de presentation
    /// des epics suivants ; jamais mute par un client. Hearts/MaxHearts/StaminaNormalized/Money
    /// (Story 2.6) sont des placeholders host-owned pour le HUD de checkpoint : aucun systeme de
    /// degats/stamina consommable/economie reelle n'existe encore derriere ces valeurs.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedPlayerState : HostOwnedNetworkStateBehaviour
    {
        public const int DefaultMaxHearts = 3;

        public const float DefaultStaminaNormalized = 1f;

        public const int DefaultMoney = 0;

        public NetworkVariable<PlayerMode> Mode = new NetworkVariable<PlayerMode>(
            PlayerMode.Spectating,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<PlayerLifecycle> Lifecycle = new NetworkVariable<PlayerLifecycle>(
            PlayerLifecycle.Alive,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<int> SeatIndex = new NetworkVariable<int>(
            -1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<ulong> ClientId = new NetworkVariable<ulong>(
            0UL,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<FixedString32Bytes> CharacterId = new NetworkVariable<FixedString32Bytes>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<Vector3> WorldPosition = new NetworkVariable<Vector3>(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<float> YawDegrees = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<int> MaxHearts = new NetworkVariable<int>(
            DefaultMaxHearts,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<int> Hearts = new NetworkVariable<int>(
            DefaultMaxHearts,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<float> StaminaNormalized = new NetworkVariable<float>(
            DefaultStaminaNormalized,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<int> Money = new NetworkVariable<int>(
            DefaultMoney,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    }
}
