using RoadRage.Shared.Domain;

namespace RoadRage.Features.Lobby
{
    /// <summary>
    /// Objet de donnees local pur pour les reglages de partie brouillon (Story 1.2).
    /// Pas de MonoBehaviour, pas de ScriptableObject, pas de NetworkVariable : les valeurs de session
    /// runtime ne vivent jamais dans des assets ScriptableObject (epic-1-context).
    /// Concu pour etre mappable sans duplication de verite vers un futur NetworkedLobbyState (Epic 2) :
    /// meme champ Difficulty, meme forme de donnees.
    /// </summary>
    public sealed class MatchSettings
    {
        public Difficulty Difficulty { get; set; } = Difficulty.Normal;

        /// <summary>
        /// Story 5.16 : effectif de vehicules IA choisi pour la session. Vaut
        /// <see cref="SessionTrafficValue.Unresolved"/> tant qu'aucune session ne l'a resolu ; les
        /// bornes autorisees viennent du <c>TrafficSettingsDef</c>, jamais d'ici.
        /// </summary>
        public int AiVehicleTargetCount { get; set; } = SessionTrafficValue.Unresolved;

        /// <summary>
        /// Story 5.16 : nombre de vehicules qui jettent des detritus, toujours inferieur ou egal a
        /// <see cref="AiVehicleTargetCount"/>. Aucun systeme de detritus ne vit ici : la Story 5.20
        /// consommera la valeur resolue.
        /// </summary>
        public int LitterThrowerCount { get; set; } = SessionTrafficValue.Unresolved;

        // Emplacement reserve aux futurs parametres de partie extensibles (ex. carte, nombre de joueurs
        // max, regles de session). Ajouter ici au fur et a mesure des besoins, en gardant la mappabilite
        // vers un futur NetworkedLobbyState.
    }
}
