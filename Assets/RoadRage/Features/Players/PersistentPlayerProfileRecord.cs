using System;

namespace RoadRage.Features.Players
{
    /// <summary>
    /// Representation serialisable du profil persistant (Story 4.5). Dediee a la persistance :
    /// PlayerProfile reste un objet pur get-only, non serialisable, pour que le depot de session garde
    /// exactement la meme forme qu'avant cette story. Champs publics : JsonUtility n'accepte rien d'autre.
    /// Ne contient que l'identite du compte Steam proprietaire, le nom affichable et le choix
    /// cosmetique, jamais d'etat de session ou de run. L'id Steam est present pour que deux comptes
    /// partageant le meme dossier persistant ne s'heritent pas mutuellement d'un nom et d'un personnage.
    /// </summary>
    [Serializable]
    public sealed class PersistentPlayerProfileRecord
    {
        public string displayName = string.Empty;

        public string characterId = string.Empty;

        public string steamId = string.Empty;
    }
}
