using System;
using Unity.Collections;
using Unity.Netcode;

namespace RoadRage.Features.PassengerActions
{
    [Serializable]
    public struct PassengerActionIntent : INetworkSerializable, IEquatable<PassengerActionIntent>
    {
        public byte Slot;
        public FixedString32Bytes ActionId;
        public int CatalogVersion;
        public int ActionVersion;
        public ulong Sequence;
        public NetworkObjectReference Target;

        public PassengerActionIntent(int slot, string actionId, int catalogVersion, int actionVersion, ulong sequence, NetworkObjectReference target)
        {
            Slot = (byte)slot;
            ActionId = actionId;
            CatalogVersion = catalogVersion;
            ActionVersion = actionVersion;
            Sequence = sequence;
            Target = target;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Slot);
            serializer.SerializeValue(ref ActionId);
            serializer.SerializeValue(ref CatalogVersion);
            serializer.SerializeValue(ref ActionVersion);
            serializer.SerializeValue(ref Sequence);
            serializer.SerializeValue(ref Target);
        }

        public bool Equals(PassengerActionIntent other)
        {
            return Slot == other.Slot && ActionId.Equals(other.ActionId)
                && CatalogVersion == other.CatalogVersion && ActionVersion == other.ActionVersion
                && Sequence == other.Sequence && Target.Equals(other.Target);
        }
    }
}
