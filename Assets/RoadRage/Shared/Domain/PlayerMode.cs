namespace RoadRage.Shared.Domain
{
    public enum PlayerMode
    {
        Driver = 0,
        Passenger = 1,
        OnFootStop = 2,
        OnFootRageRoad = 3,
        Spectating = 4,

        /// <summary>Presence generique a pied dans le monde vide (Story 2.5), avant que les modes on-foot specifiques (stop, Rage Road) n'existent.</summary>
        OnFoot = 5
    }
}
