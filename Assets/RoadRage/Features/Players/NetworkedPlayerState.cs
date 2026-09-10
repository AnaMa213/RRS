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
    /// des epics suivants ; jamais mute par un client. StaminaNormalized/Money (Story 2.6) restent
    /// des placeholders host-owned pour le HUD de checkpoint. Hp (Story 3.5) est le contrat de vie
    /// reel : une collision vehicule (via RunFlowController/NetworkedPlayerLifecycleService) le
    /// reduit, et Hearts (ex-placeholder 2.6) devient une ressource reellement consommable --
    /// depensee uniquement quand ReviveDeadlineTime expire sans resurrection (Downed -> Alive via
    /// TryRespawn), jamais remise a zero/remplie automatiquement.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkedPlayerState : HostOwnedNetworkStateBehaviour
    {
        public const int DefaultMaxHearts = 3;

        public const float DefaultStaminaNormalized = 1f;

        public const int DefaultMoney = 0;

        public const int DefaultMaxHp = 100;

        /// <summary>Sentinel "aucune fenetre de resurrection active" pour ReviveDeadlineTime.</summary>
        public const double NoActiveReviveWindow = -1d;

        /// <summary>Duree (Story 3.5) de la fenetre de resurrection apres passage a Downed.</summary>
        public const float ReviveWindowSeconds = 30f;

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

        public NetworkVariable<int> Hp = new NetworkVariable<int>(
            DefaultMaxHp,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        /// <summary>
        /// Horodatage (NetworkManager.ServerTime.Time) auquel la fenetre de resurrection expire ;
        /// NoActiveReviveWindow quand le joueur n'est pas Downed. Lu par le host pour l'expiration
        /// automatique, expose en lecture a tous pour un futur affichage de compte a rebours.
        /// </summary>
        public NetworkVariable<double> ReviveDeadlineTime = new NetworkVariable<double>(
            NoActiveReviveWindow,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    }
}
