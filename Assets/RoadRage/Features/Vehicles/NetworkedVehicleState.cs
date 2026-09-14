using RoadRage.Shared.Networking;
using Unity.Netcode;
using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>Type de degat vehicule (Story 3.5) : roue/moteur/frein, un seul selectionne par seuil de 33 HP cumules.</summary>
    public enum VehicleDamageType
    {
        Wheel = 0,
        Engine = 1,
        Brake = 2
    }

    /// <summary>
    /// Identite reseau et frontiere du module Vehicules pour la voiture partagee. Story 3.2 y
    /// ajoute la revendication de conducteur (DriverClientId) ; Story 3.3 ajoute l'occupation
    /// formelle des sieges conducteur/passagers. Story 3.5 ajoute Hp et les flags de degat
    /// roue/moteur/frein : un ordre aleatoire (DamageOrderSlot0/1/2) est tire une seule fois via
    /// EnsureDamageStateInitialized (appele au spawn hote ou a l'activation solo, jamais depuis
    /// Awake -- ecrire un NetworkVariable avant le spawn declenche un avertissement Netcode) pour
    /// garantir qu'aucun type ne se repete sans avoir a synchroniser une liste separee de types
    /// "deja vus" (cf. Design Notes de la story) -- chaque seuil de 33 HP cumules franchi consomme
    /// le prochain type de l'ordre via DamageThresholdsCrossed.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkedVehicleState : HostOwnedNetworkStateBehaviour
    {
        public const int DefaultMaxHp = 100;

        public const int DamageThresholdStep = 33;

        public const int MaxDamageTypeCount = 3;

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

        public NetworkVariable<int> Hp = new NetworkVariable<int>(
            DefaultMaxHp,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> WheelDamaged = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> EngineDamaged = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> BrakeDamaged = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        /// <summary>Ordre (aleatoire, tire via EnsureDamageStateInitialized) des VehicleDamageType consommes un par un aux seuils de 33 HP cumules.</summary>
        public NetworkVariable<int> DamageOrderSlot0 = new NetworkVariable<int>(
            (int)VehicleDamageType.Wheel,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<int> DamageOrderSlot1 = new NetworkVariable<int>(
            (int)VehicleDamageType.Engine,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<int> DamageOrderSlot2 = new NetworkVariable<int>(
            (int)VehicleDamageType.Brake,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<int> DamageThresholdsCrossed = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private bool damageStateInitialized;
        private int localHp = DefaultMaxHp;
        private bool localWheelDamaged;
        private bool localEngineDamaged;
        private bool localBrakeDamaged;
        private int localDamageOrderSlot0 = (int)VehicleDamageType.Wheel;
        private int localDamageOrderSlot1 = (int)VehicleDamageType.Engine;
        private int localDamageOrderSlot2 = (int)VehicleDamageType.Brake;
        private int localDamageThresholdsCrossed;

        public int CurrentHp { get { return IsSpawned ? Hp.Value : localHp; } }
        public bool IsWheelDamaged { get { return IsSpawned ? WheelDamaged.Value : localWheelDamaged; } }
        public bool IsEngineDamaged { get { return IsSpawned ? EngineDamaged.Value : localEngineDamaged; } }
        public bool IsBrakeDamaged { get { return IsSpawned ? BrakeDamaged.Value : localBrakeDamaged; } }

        public event System.Action LocalDamageStateChanged;

        /// <summary>Voiture inconduisible (Story 3.5) : tous les occupants doivent etre ejectes (NetworkedVehicleSeatService).</summary>
        public bool IsInoperable()
        {
            return CurrentHp <= 0;
        }

        /// <summary>
        /// Tire l'ordre aleatoire de degat une seule fois (au premier appel), depuis
        /// NetworkedVehicleDriverController.OnNetworkSpawn (IsServer) --
        /// jamais depuis Awake() : ecrire un NetworkVariable.Value avant que le NetworkObject ne soit
        /// spawn declenche l'avertissement Netcode "doesn't know its NetworkBehaviour yet". Les
        /// valeurs par defaut des NetworkVariable (Hp/flags/compteur) sont deja correctes sans appel ;
        /// seul l'ordre aleatoire a besoin d'etre tire explicitement.
        /// </summary>
        public void EnsureDamageStateInitialized()
        {
            if (damageStateInitialized)
            {
                return;
            }

            damageStateInitialized = true;
            ResetDamageState();
        }

        /// <summary>
        /// Remet Hp/flags/compteur a l'etat initial et tire un nouvel ordre aleatoire de types de
        /// degat (creation/reset greybox, cf. Design Notes de la Story 3.5). Public pour un futur
        /// reset explicite (ex. restart de run, Epic 7) ; l'initialisation normale passe par
        /// EnsureDamageStateInitialized.
        /// </summary>
        public void ResetDamageState()
        {
            SetHp(DefaultMaxHp);
            SetWheelDamaged(false);
            SetEngineDamaged(false);
            SetBrakeDamaged(false);
            SetDamageThresholdsCrossed(0);

            var order = ShuffleDamageTypeOrder(UnityEngine.Random.Range);
            SetDamageOrderSlot0(order[0]);
            SetDamageOrderSlot1(order[1]);
            SetDamageOrderSlot2(order[2]);
        }

        /// <summary>
        /// Applique des degats voiture, clampe Hp a 0, puis applique (sans repetition) un type de
        /// degat par seuil de 33 HP cumules nouvellement franchi.
        /// </summary>
        public void ApplyDamage(int amount)
        {
            if (amount <= 0 || CurrentHp <= 0)
            {
                return;
            }

            var previousHp = CurrentHp;
            var nextHp = Mathf.Max(0, previousHp - amount);
            SetHp(nextHp);

            var previousThresholds = ComputeThresholdsCrossed(previousHp, DefaultMaxHp);
            var nextThresholds = ComputeThresholdsCrossed(nextHp, DefaultMaxHp);
            for (var slot = previousThresholds; slot < nextThresholds; slot++)
            {
                ApplyDamageTypeAtSlot(slot);
            }
        }

        /// <summary>
        /// Predicat pur (Design Notes) : nombre de seuils de 33 HP cumules franchis depuis maxHp --
        /// avec DefaultMaxHp (100), correspond exactement aux seuils 67/34/1 HP restants de la matrice
        /// I/O (33 -> 1, 66 -> 2, 99/100 -> 3, plafonne a MaxDamageTypeCount).
        /// </summary>
        public static int ComputeThresholdsCrossed(int hp, int maxHp)
        {
            var damageTaken = Mathf.Max(0, maxHp - hp);
            return Mathf.Clamp(damageTaken / DamageThresholdStep, 0, MaxDamageTypeCount);
        }

        /// <summary>
        /// Fisher-Yates pur, parametre par une fonction de tirage (min inclus, max exclu) pour rester
        /// testable sans UnityEngine.Random -- utilise en jeu avec UnityEngine.Random.Range.
        /// </summary>
        public static int[] ShuffleDamageTypeOrder(System.Func<int, int, int> randomRange)
        {
            var order = new[] { 0, 1, 2 };
            for (var i = order.Length - 1; i > 0; i--)
            {
                var j = randomRange(0, i + 1);
                var temp = order[i];
                order[i] = order[j];
                order[j] = temp;
            }

            return order;
        }

        private void ApplyDamageTypeAtSlot(int slot)
        {
            if (slot < 0 || slot >= MaxDamageTypeCount || CurrentDamageThresholdsCrossed > slot)
            {
                return;
            }

            switch (ResolveDamageOrderSlot(slot))
            {
                case (int)VehicleDamageType.Wheel:
                    SetWheelDamaged(true);
                    break;
                case (int)VehicleDamageType.Engine:
                    SetEngineDamaged(true);
                    break;
                case (int)VehicleDamageType.Brake:
                    SetBrakeDamaged(true);
                    break;
            }

            SetDamageThresholdsCrossed(slot + 1);
        }

        private int ResolveDamageOrderSlot(int slot)
        {
            switch (slot)
            {
                case 0:
                    return IsSpawned ? DamageOrderSlot0.Value : localDamageOrderSlot0;
                case 1:
                    return IsSpawned ? DamageOrderSlot1.Value : localDamageOrderSlot1;
                default:
                    return IsSpawned ? DamageOrderSlot2.Value : localDamageOrderSlot2;
            }
        }

        private int CurrentDamageThresholdsCrossed { get { return IsSpawned ? DamageThresholdsCrossed.Value : localDamageThresholdsCrossed; } }

        private void SetHp(int value) { if (IsSpawned) { Hp.Value = value; } else { localHp = value; NotifyLocalDamageStateChanged(); } }
        private void SetWheelDamaged(bool value) { if (IsSpawned) { WheelDamaged.Value = value; } else { localWheelDamaged = value; NotifyLocalDamageStateChanged(); } }
        private void SetEngineDamaged(bool value) { if (IsSpawned) { EngineDamaged.Value = value; } else { localEngineDamaged = value; NotifyLocalDamageStateChanged(); } }
        private void SetBrakeDamaged(bool value) { if (IsSpawned) { BrakeDamaged.Value = value; } else { localBrakeDamaged = value; NotifyLocalDamageStateChanged(); } }
        private void SetDamageThresholdsCrossed(int value) { if (IsSpawned) { DamageThresholdsCrossed.Value = value; } else { localDamageThresholdsCrossed = value; } }
        private void SetDamageOrderSlot0(int value) { if (IsSpawned) { DamageOrderSlot0.Value = value; } else { localDamageOrderSlot0 = value; } }
        private void SetDamageOrderSlot1(int value) { if (IsSpawned) { DamageOrderSlot1.Value = value; } else { localDamageOrderSlot1 = value; } }
        private void SetDamageOrderSlot2(int value) { if (IsSpawned) { DamageOrderSlot2.Value = value; } else { localDamageOrderSlot2 = value; } }

        private void NotifyLocalDamageStateChanged()
        {
            LocalDamageStateChanged?.Invoke();
        }

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
