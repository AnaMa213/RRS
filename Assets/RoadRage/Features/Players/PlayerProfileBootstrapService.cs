using System;
using RoadRage.Shared.Definitions;

namespace RoadRage.Features.Players
{
    /// <summary>
    /// Politique de resolution du profil persistant (Story 4.5). Objet C# pur, sans dependance Unity
    /// ni etat de gameplay : il combine le fichier de profil et le catalogue de personnages pour dire
    /// quel profil le menu doit publier, et si son ecriture est requise.
    /// Il ne connait ni Steam ni la couche App : l'identite Steam lui arrive en valeurs simples, ce qui
    /// evite toute dependance entre le feature Players et le feature Online.
    /// Resolu une seule fois a l'ouverture du menu : les changements de selection suivants sont ecrits
    /// directement par la couche App, sans repasser par cette politique.
    /// </summary>
    public sealed class PlayerProfileBootstrapService
    {
        /// <summary>Personnage du profil cree automatiquement (catalogue Rookie).</summary>
        public const string DefaultCharacterId = "char_rookie";

        public const string SteamUnavailableMessage = "Steam indisponible : profil non enregistre, Rookie est utilise pour cette session.";

        public const string EmptyCatalogMessage = "Aucun personnage disponible : catalogue non assigne ou vide.";

        private readonly CharacterCatalog catalog;

        private readonly PlayerProfileFileStore fileStore;

        public PlayerProfileBootstrapService(CharacterCatalog catalog, PlayerProfileFileStore fileStore)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (fileStore == null)
            {
                throw new ArgumentNullException(nameof(fileStore));
            }

            this.catalog = catalog;
            this.fileStore = fileStore;
        }

        /// <summary>
        /// Resout le profil a publier. Sans Steam, aucune lecture ni ecriture de fichier : le menu
        /// utilise un profil en memoire non persiste (override humain du 2026-09-13). Avec Steam, un
        /// profil persistant valide et appartenant au compte courant est repris tel quel ; sinon il est
        /// cree depuis l'identite Steam avec Rookie par defaut, et le fichier est (re)ecrit — y compris
        /// quand le fichier appartient a un autre compte Steam, dont le choix n'est jamais herite.
        /// Ne leve jamais.
        /// </summary>
        public PlayerProfileResolution Resolve(bool steamAvailable, string steamDisplayName, string steamId)
        {
            if (!steamAvailable)
            {
                CharacterDef offlineCharacter;
                if (!TryGetUsableCharacter(new DefinitionId(DefaultCharacterId), out offlineCharacter))
                {
                    return new PlayerProfileResolution(null, false, EmptyCatalogMessage);
                }

                return new PlayerProfileResolution(
                    new PlayerProfile(offlineCharacter.DisplayName, offlineCharacter.Id),
                    false,
                    SteamUnavailableMessage);
            }

            PlayerProfile stored;
            if (fileStore.TryLoad(steamId, out stored))
            {
                CharacterDef storedCharacter;
                if (TryGetUsableCharacter(stored.CharacterId, out storedCharacter))
                {
                    string storedName;
                    string nameError;
                    if (PlayerNameValidator.TryNormalize(stored.DisplayName, out storedName, out nameError))
                    {
                        return new PlayerProfileResolution(new PlayerProfile(storedName, storedCharacter.Id), false, string.Empty);
                    }

                    // Nom persistant inutilisable : le personnage est conserve, le libelle retombe sur
                    // son nom d'affichage, et le fichier est corrige par l'ecriture qui suit.
                    return new PlayerProfileResolution(new PlayerProfile(storedCharacter.DisplayName, storedCharacter.Id), true, string.Empty);
                }
            }

            CharacterDef defaultCharacter;
            if (!TryGetUsableCharacter(new DefinitionId(DefaultCharacterId), out defaultCharacter))
            {
                return new PlayerProfileResolution(null, false, EmptyCatalogMessage);
            }

            var displayName = defaultCharacter.DisplayName;

            string steamName;
            string steamNameError;
            if (PlayerNameValidator.TryNormalize(steamDisplayName, out steamName, out steamNameError))
            {
                displayName = steamName;
            }

            return new PlayerProfileResolution(new PlayerProfile(displayName, defaultCharacter.Id), true, string.Empty);
        }

        /// <summary>
        /// Persiste le profil choisi pour ce compte Steam. Un profil non persistable (Steam
        /// indisponible) ou sans proprietaire n'est jamais ecrit.
        /// </summary>
        public bool TryPersist(PlayerProfile profile, string steamId, bool persistable)
        {
            return persistable && fileStore.TrySave(profile, steamId);
        }

        private bool TryGetUsableCharacter(DefinitionId id, out CharacterDef character)
        {
            character = null;

            if (id.IsEmpty)
            {
                return false;
            }

            CharacterDef candidate;
            if (!catalog.TryGetById(id, out candidate) || candidate == null)
            {
                return false;
            }

            character = candidate;
            return true;
        }

        /// <summary>
        /// Resultat de resolution : profil pret a publier, et si son ecriture sur disque est requise.
        /// Un profil non resolu porte toujours un message visible, jamais un echec silencieux.
        /// </summary>
        public sealed class PlayerProfileResolution
        {
            public PlayerProfileResolution(PlayerProfile profile, bool shouldPersist, string error)
            {
                Profile = profile;
                ShouldPersist = shouldPersist;
                Error = error ?? string.Empty;
            }

            public PlayerProfile Profile { get; }

            public bool ShouldPersist { get; }

            public string Error { get; }

            public bool IsResolved
            {
                get { return Profile != null; }
            }
        }
    }
}
