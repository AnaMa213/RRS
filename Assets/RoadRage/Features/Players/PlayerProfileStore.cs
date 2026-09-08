using System;

namespace RoadRage.Features.Players
{
    /// <summary>
    /// Depot de session du profil joueur (Story 1.3). Objet C# pur porte par l'objet persistant du
    /// bootstrap : c'est ce qui rend le profil lisible par l'entree monde de la Story 1.5 apres un
    /// changement de scene, sans coupler la scene de menu a la scene de run.
    /// Duree de vie = celle de la session applicative : rien n'est ecrit sur disque.
    /// </summary>
    public sealed class PlayerProfileStore
    {
        public PlayerProfile Current { get; private set; }

        public bool HasProfile
        {
            get { return Current != null; }
        }

        public event Action<PlayerProfile> ProfileChanged;

        /// <summary>
        /// Remplace le profil courant. Seule la couche App appelle cette methode, apres validation :
        /// un nom invalide ne doit jamais atteindre le depot.
        /// </summary>
        public void Set(PlayerProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            Current = profile;

            var handler = ProfileChanged;
            if (handler != null)
            {
                handler(profile);
            }
        }

        // Volontairement aucune methode Clear ici : personne n'efface le profil de session, et une
        // remise a zero qui ne leverait pas ProfileChanged desynchroniserait silencieusement ses
        // abonnes (l'entree monde de la Story 1.5 en tete). A ajouter le jour ou un appelant existe,
        // en levant l'evenement comme Set.
    }
}
