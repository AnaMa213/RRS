using System;
using UnityEngine;

namespace RoadRage.Features.Players
{
    /// <summary>
    /// Depot de session du profil joueur (Story 1.3). Objet C# pur porte par l'objet persistant du
    /// bootstrap : c'est ce qui rend le profil lisible par l'entree monde de la Story 1.5 apres un
    /// changement de scene, sans coupler la scene de menu a la scene de run.
    /// Duree de vie = celle de la session applicative : rien n'est ecrit sur disque.
    /// Depuis la Story 4.6 il est explicitement gelable : a l'entree du lobby, Freeze() capture un
    /// instantane de session et Set() refuse toute mutation jusqu'a Unfreeze(). Le gel reste une
    /// immuabilite de session, jamais un second depot : Current demeure le profil du menu, et
    /// SessionSelection est la seule lecture de la selection en aval du menu.
    /// </summary>
    public sealed class PlayerProfileStore
    {
        private PlayerProfile sessionSelection;

        public PlayerProfile Current { get; private set; }

        public bool HasProfile
        {
            get { return Current != null; }
        }

        /// <summary>Vrai entre Freeze() et Unfreeze() : plus aucune mutation n'est acceptee.</summary>
        public bool IsFrozen { get; private set; }

        /// <summary>
        /// Selection a consommer en aval du menu (payload de session, spawn solo) : l'instantane pris a
        /// l'entree du lobby une fois gele, sinon le profil courant. Passer par ici evite toute lecture
        /// live du profil pendant une session.
        /// </summary>
        public PlayerProfile SessionSelection
        {
            get { return IsFrozen ? sessionSelection : Current; }
        }

        public event Action<PlayerProfile> ProfileChanged;

        /// <summary>
        /// Gele la selection de personnage : capture l'instantane de session et refuse toute mutation
        /// jusqu'a Unfreeze(). Idempotent : un second appel (Play en solo puis room Open en hote)
        /// conserve l'instantane deja pris, jamais un etat derive d'une ecriture intermediaire.
        /// Ne leve aucun evenement : le gel n'est pas un changement de selection.
        /// </summary>
        public void Freeze()
        {
            if (IsFrozen)
            {
                return;
            }

            sessionSelection = Current;
            IsFrozen = true;
        }

        /// <summary>
        /// Leve le gel et oublie l'instantane de session. Appartient au menu principal, seule surface de
        /// selection : sans ce degel a sa (re)ouverture, le menu resterait en lecture seule pour toujours.
        /// </summary>
        public void Unfreeze()
        {
            IsFrozen = false;
            sessionSelection = null;
        }

        /// <summary>
        /// Remplace le profil courant. Seule la couche App appelle cette methode, apres validation :
        /// un nom invalide ne doit jamais atteindre le depot.
        /// Retourne faux, sans rien muter ni lever ProfileChanged, quand le depot est gele : le refus
        /// remonte a l'appelant, qui doit le rendre visible au joueur et ne rien ecrire sur disque.
        /// </summary>
        public bool Set(PlayerProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (IsFrozen)
            {
                Debug.LogWarning("[Players] Mutation de profil refusee : la selection est gelee pour la session courante.");
                return false;
            }

            Current = profile;

            var handler = ProfileChanged;
            if (handler != null)
            {
                handler(profile);
            }

            return true;
        }

        // Volontairement aucune methode Clear ici : personne n'efface le profil de session, et une
        // remise a zero qui ne leverait pas ProfileChanged desynchroniserait silencieusement ses
        // abonnes (l'entree monde de la Story 1.5 en tete). A ajouter le jour ou un appelant existe,
        // en levant l'evenement comme Set.
    }
}
