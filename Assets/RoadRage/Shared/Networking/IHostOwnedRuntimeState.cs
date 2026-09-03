namespace RoadRage.Shared.Networking
{
    public interface IHostOwnedRuntimeState
    {
        bool IsHostAuthority { get; }
    }
}
