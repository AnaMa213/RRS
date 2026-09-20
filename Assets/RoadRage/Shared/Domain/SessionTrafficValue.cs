namespace RoadRage.Shared.Domain
{
    /// <summary>
    /// Story 5.16 : sentinel unique des valeurs de trafic resolues par une session (effectif de
    /// vehicules IA, nombre de jeteurs de detritus).
    ///
    /// Il vit dans Shared parce que ses trois porteurs vivent dans trois assemblies qui ne se
    /// referencent pas entre elles -- <c>LobbyRosterSnapshot</c> (Features.Online), <c>MatchSettings</c>
    /// (Features.Lobby) et <c>NetworkedRunState</c> (Features.Run). Une constante recopiee trois fois
    /// serait trois fois fausse le jour ou elle change.
    ///
    /// <c>0</c> est une valeur legale du <c>TrafficSettingsDef</c> et ne peut donc pas porter ce sens :
    /// <see cref="Unresolved"/> veut dire "aucune session ne l'a resolue", et le repli est alors le
    /// defaut authore du Def, jamais une valeur inventee en code.
    /// </summary>
    public static class SessionTrafficValue
    {
        /// <summary>Aucune session n'a resolu la valeur : le defaut authore du Def s'applique.</summary>
        public const int Unresolved = -1;
    }
}
