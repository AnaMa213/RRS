namespace RoadRage.Shared.Domain
{
    /// <summary>
    /// Lecture etroite de la disposition de rage courante d'une cible (Story 5.4). Existe parce que
    /// Features/Vehicles doit consommer la rage sans dependre de Features/Rage : les asmdefs de
    /// features ne se referencent jamais entre eux (RoadRageScaffoldTests), et Vehicles ne doit
    /// jamais muter la rage -- l'interface n'expose donc qu'un getter.
    /// Unique implementation : RoadRage.Features.Rage.NetworkedRageState, seul producteur host-owned.
    /// </summary>
    public interface IRageDispositionSource
    {
        /// <summary>Disposition courante synchronisee depuis l'hote ; Calm tant qu'aucune rage n'a ete appliquee.</summary>
        RageDisposition CurrentDisposition { get; }
    }
}
