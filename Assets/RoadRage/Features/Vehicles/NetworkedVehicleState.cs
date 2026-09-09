using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Identite reseau et frontiere du module Vehicules pour la voiture partagee. Story 3.2 y
    /// ajoute la revendication de conducteur (DriverClientId) ; Story 3.3 ajoute l'occupation
    /// formelle des sieges conducteur/passagers.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkedVehicleState : HostOwnedNetworkStateBehaviour
    {
        public const int NoSeatIndex = -1;

        public const int DriverSeatIndex = 0;

        public const int FirstPassengerSeatIndex = 1;

        public const int PassengerSeatCount = 3;

        public const int SeatCount = 1 + PassengerSeatCount;

        /// <summary>
        /// Sentinel signifiant "aucun conducteur" -- meme esprit que SeatIndex = -1 dans
        /// NetworkedPlayerState (Story 2.6), mais propre au module Vehicules : aucun champ
        /// SeatIndex/PlayerMode n'est touche ici (occupation formelle des sieges = Story 3.3).
        /// </summary>
        public const ulong UnclaimedDriverClientId = ulong.MaxValue;

        public const ulong UnoccupiedSeatClientId = UnclaimedDriverClientId;

        public NetworkVariable<ulong> DriverClientId = new NetworkVariable<ulong>(
            UnclaimedDriverClientId,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<ulong> PassengerSeat1ClientId = new NetworkVariable<ulong>(
            UnoccupiedSeatClientId,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<ulong> PassengerSeat2ClientId = new NetworkVariable<ulong>(
            UnoccupiedSeatClientId,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<ulong> PassengerSeat3ClientId = new NetworkVariable<ulong>(
            UnoccupiedSeatClientId,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public bool IsDriver(ulong clientId)
        {
            return DriverClientId.Value == clientId;
        }

        public bool IsSeated(ulong clientId)
        {
            return FindSeatIndex(clientId) != NoSeatIndex;
        }

        public int FindSeatIndex(ulong clientId)
        {
            if (clientId == UnoccupiedSeatClientId)
            {
                return NoSeatIndex;
            }

            if (DriverClientId.Value == clientId)
            {
                return DriverSeatIndex;
            }

            if (PassengerSeat1ClientId.Value == clientId)
            {
                return FirstPassengerSeatIndex;
            }

            if (PassengerSeat2ClientId.Value == clientId)
            {
                return FirstPassengerSeatIndex + 1;
            }

            if (PassengerSeat3ClientId.Value == clientId)
            {
                return FirstPassengerSeatIndex + 2;
            }

            return NoSeatIndex;
        }

        public bool TryGetSeatOccupant(int seatIndex, out ulong clientId)
        {
            clientId = UnoccupiedSeatClientId;

            switch (seatIndex)
            {
                case DriverSeatIndex:
                    clientId = DriverClientId.Value;
                    return true;
                case FirstPassengerSeatIndex:
                    clientId = PassengerSeat1ClientId.Value;
                    return true;
                case FirstPassengerSeatIndex + 1:
                    clientId = PassengerSeat2ClientId.Value;
                    return true;
                case FirstPassengerSeatIndex + 2:
                    clientId = PassengerSeat3ClientId.Value;
                    return true;
                default:
                    return false;
            }
        }

        public bool IsSeatOccupied(int seatIndex)
        {
            return TryGetSeatOccupant(seatIndex, out var clientId) && clientId != UnoccupiedSeatClientId;
        }

        public bool TryFindAvailableSeat(bool preferPassenger, out int seatIndex)
        {
            seatIndex = NoSeatIndex;

            if (preferPassenger)
            {
                return TryFindAvailablePassengerSeat(out seatIndex);
            }

            if (!IsSeatOccupied(DriverSeatIndex))
            {
                seatIndex = DriverSeatIndex;
                return true;
            }

            return TryFindAvailablePassengerSeat(out seatIndex);
        }

        public bool TryFindAvailablePassengerSeat(out int seatIndex)
        {
            for (var index = FirstPassengerSeatIndex; index < SeatCount; index++)
            {
                if (!IsSeatOccupied(index))
                {
                    seatIndex = index;
                    return true;
                }
            }

            seatIndex = NoSeatIndex;
            return false;
        }

        public bool TryAssignSeat(int seatIndex, ulong clientId)
        {
            if (!IsValidSeatIndex(seatIndex) || clientId == UnoccupiedSeatClientId || IsSeated(clientId) || IsSeatOccupied(seatIndex))
            {
                return false;
            }

            SetSeatOccupant(seatIndex, clientId);
            return true;
        }

        public bool ReleaseSeat(int seatIndex, ulong clientId)
        {
            if (!TryGetSeatOccupant(seatIndex, out var occupant) || occupant != clientId)
            {
                return false;
            }

            SetSeatOccupant(seatIndex, UnoccupiedSeatClientId);
            return true;
        }

        public bool ReleaseClient(ulong clientId)
        {
            var seatIndex = FindSeatIndex(clientId);
            return seatIndex != NoSeatIndex && ReleaseSeat(seatIndex, clientId);
        }

        public static bool IsValidSeatIndex(int seatIndex)
        {
            return seatIndex >= DriverSeatIndex && seatIndex < SeatCount;
        }

        public static bool IsPassengerSeatIndex(int seatIndex)
        {
            return seatIndex >= FirstPassengerSeatIndex && seatIndex < SeatCount;
        }

        public static Vector3 ResolveSeatLocalOffset(int seatIndex)
        {
            switch (seatIndex)
            {
                case DriverSeatIndex:
                    return new Vector3(-0.45f, 1.05f, 0.35f);
                case FirstPassengerSeatIndex:
                    return new Vector3(0.45f, 1.05f, 0.35f);
                case FirstPassengerSeatIndex + 1:
                    return new Vector3(-0.45f, 1.05f, -0.65f);
                case FirstPassengerSeatIndex + 2:
                    return new Vector3(0.45f, 1.05f, -0.65f);
                default:
                    return Vector3.zero;
            }
        }

        public static Vector3 ResolveExitLocalOffset(int seatIndex)
        {
            if (seatIndex == DriverSeatIndex || seatIndex == FirstPassengerSeatIndex + 1)
            {
                return new Vector3(-1.8f, 0.1f, -0.35f);
            }

            return new Vector3(1.8f, 0.1f, -0.35f);
        }

        private void SetSeatOccupant(int seatIndex, ulong clientId)
        {
            switch (seatIndex)
            {
                case DriverSeatIndex:
                    DriverClientId.Value = clientId;
                    break;
                case FirstPassengerSeatIndex:
                    PassengerSeat1ClientId.Value = clientId;
                    break;
                case FirstPassengerSeatIndex + 1:
                    PassengerSeat2ClientId.Value = clientId;
                    break;
                case FirstPassengerSeatIndex + 2:
                    PassengerSeat3ClientId.Value = clientId;
                    break;
            }
        }
    }
}
