using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Presentation;
using UnityEngine;

namespace RoadRage.App.Players
{
    /// <summary>
    /// Seule couture entre l'ecran de setup de personnage, le catalogue de personnages et le depot de
    /// profil porte par le bootstrap (Story 1.3). Unique ecrivain du profil de session et de chaque
    /// libelle de l'ecran : l'ecran n'emet que des intentions et ne mute jamais l'etat de jeu.
    /// Aucune scene n'est chargee ici (Story 1.5), aucun etat reseau n'est touche (Epic 2).
    /// Aucun refus n'est silencieux : chaque chemin qui n'ecrit pas le profil affiche un libelle de
    /// validation nommant la cause.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerProfileFlowController : MonoBehaviour
    {
        public const string EmptyCatalogValidation = "Aucun personnage disponible : catalogue non assigne ou vide.";

        public const string MissingCharacterValidation = "Personnage indisponible : entree de catalogue manquante.";

        public const string EmptyCharacterIdValidation = "Personnage invalide : id de personnage vide dans le catalogue.";

        public const string ProfileStoreUnavailableValidation = "Profil non enregistre : le depot de session est indisponible.";

        [SerializeField]
        private LobbyShellScreen lobbyShellScreen;

        [SerializeField]
        private CharacterSetupScreen characterSetupScreen;

        [SerializeField]
        private CharacterCatalog catalog;

        private RoadRageBootstrap bootstrap;

        private int currentIndex;

        /// <summary>Index du personnage actuellement presente, avant toute confirmation.</summary>
        public int CurrentIndex
        {
            get { return currentIndex; }
        }

        private void Awake()
        {
            bootstrap = RoadRageBootstrap.EnsureInstance();

            if (catalog == null)
            {
                Debug.LogWarning("[App] PlayerProfileFlowController sans reference vers CharacterCatalog : le setup de personnage restera inerte.");
            }
            else
            {
                string catalogError;
                if (!catalog.TryValidate(out catalogError))
                {
                    Debug.LogWarning("[Players] Catalogue de personnages invalide : " + catalogError);
                }
            }

            if (lobbyShellScreen != null)
            {
                lobbyShellScreen.CharacterSetupRequested += HandleCharacterSetupRequested;
            }
            else
            {
                Debug.LogWarning("[App] PlayerProfileFlowController sans reference vers LobbyShellScreen.");
            }

            if (characterSetupScreen != null)
            {
                characterSetupScreen.NextCharacterRequested += HandleNextCharacterRequested;
                characterSetupScreen.ConfirmRequested += HandleConfirmRequested;
                characterSetupScreen.CloseRequested += HandleCloseRequested;
            }
            else
            {
                Debug.LogWarning("[App] PlayerProfileFlowController sans reference vers CharacterSetupScreen.");
            }
        }

        private void OnDestroy()
        {
            if (lobbyShellScreen != null)
            {
                lobbyShellScreen.CharacterSetupRequested -= HandleCharacterSetupRequested;
            }

            if (characterSetupScreen != null)
            {
                characterSetupScreen.NextCharacterRequested -= HandleNextCharacterRequested;
                characterSetupScreen.ConfirmRequested -= HandleConfirmRequested;
                characterSetupScreen.CloseRequested -= HandleCloseRequested;
            }
        }

        /// <summary>
        /// Ouvre l'ecran de setup. La couche App reecrit systematiquement chaque libelle a l'ouverture :
        /// le nom confirme s'il en existe un, la chaine vide sinon. Sans cette reecriture inconditionnelle,
        /// une saisie refusee puis abandonnee reapparaitrait a la reouverture sans son message d'erreur.
        /// </summary>
        private void HandleCharacterSetupRequested()
        {
            if (characterSetupScreen == null)
            {
                return;
            }

            Debug.Log("[App] Ouverture du setup de personnage.");

            var profile = CurrentProfile();
            characterSetupScreen.SetName(profile != null ? profile.DisplayName : string.Empty);

            if (profile != null && catalog != null)
            {
                var confirmedIndex = catalog.IndexOf(profile.CharacterId);
                if (confirmedIndex >= 0)
                {
                    currentIndex = confirmedIndex;
                }
            }

            CharacterDef character;
            string error;
            if (!TryGetUsableCharacter(out character, out error))
            {
                Debug.LogWarning("[Players] " + error);
                characterSetupScreen.ShowCharacter(string.Empty, Color.white);
                characterSetupScreen.ShowValidation(error);
                characterSetupScreen.Show();
                return;
            }

            characterSetupScreen.ClearValidation();
            characterSetupScreen.ShowCharacter(character.DisplayName, character.PreviewTint);
            characterSetupScreen.Show();
        }

        private void HandleNextCharacterRequested()
        {
            if (characterSetupScreen == null || catalog == null || catalog.Count == 0)
            {
                return;
            }

            currentIndex = (currentIndex + 1) % catalog.Count;

            CharacterDef character;
            string error;
            if (!TryGetUsableCharacter(out character, out error))
            {
                Debug.LogWarning("[Players] " + error);
                characterSetupScreen.ShowCharacter(string.Empty, Color.white);
                characterSetupScreen.ShowValidation(error);
                return;
            }

            characterSetupScreen.ClearValidation();
            characterSetupScreen.ShowCharacter(character.DisplayName, character.PreviewTint);
        }

        /// <summary>
        /// Valide le personnage courant puis le texte brut saisi, et seulement ensuite ecrit le profil
        /// de session. Tout refus laisse le depot strictement inchange, l'ecran ouvert, et affiche un
        /// message nommant la cause.
        /// </summary>
        private void HandleConfirmRequested(string rawName)
        {
            if (characterSetupScreen == null)
            {
                return;
            }

            CharacterDef character;
            string characterError;
            if (!TryGetUsableCharacter(out character, out characterError))
            {
                Debug.LogWarning("[Players] Confirmation refusee : " + characterError);
                characterSetupScreen.ShowValidation(characterError);
                return;
            }

            string normalized;
            string nameError;
            if (!PlayerNameValidator.TryNormalize(rawName, out normalized, out nameError))
            {
                Debug.Log("[Players] Nom de joueur refuse : " + nameError);
                characterSetupScreen.ShowValidation(nameError);
                return;
            }

            if (bootstrap == null || bootstrap.Profiles == null)
            {
                Debug.LogWarning("[App] PlayerProfileFlowController sans depot de profil disponible.");
                characterSetupScreen.ShowValidation(ProfileStoreUnavailableValidation);
                return;
            }

            bootstrap.Profiles.Set(new PlayerProfile(normalized, character.Id));
            Debug.Log("[Players] Profil confirme : " + normalized + " / " + character.Id);

            if (bootstrap.Notices != null)
            {
                bootstrap.Notices.Publish(new UserNotice(UserNoticeSeverity.Info, "Profil enregistre : " + normalized + " (" + character.DisplayName + ")"));
            }

            characterSetupScreen.ClearValidation();
            characterSetupScreen.Hide();
        }

        private void HandleCloseRequested()
        {
            if (characterSetupScreen == null)
            {
                return;
            }

            Debug.Log("[App] Fermeture du setup de personnage sans confirmation.");
            characterSetupScreen.Hide();
        }

        /// <summary>
        /// Garde unique du personnage courant, partagee par l'ouverture, le cycle et la confirmation.
        /// Un id vide est refuse ici : le profil doit toujours porter un DefinitionId non vide,
        /// exploitable tel quel par la synchro reseau de l'Epic 2.
        /// </summary>
        private bool TryGetUsableCharacter(out CharacterDef character, out string error)
        {
            character = null;

            if (catalog == null || catalog.Count == 0)
            {
                error = EmptyCatalogValidation;
                return false;
            }

            var candidate = catalog.GetAt(currentIndex);
            if (candidate == null)
            {
                error = MissingCharacterValidation;
                return false;
            }

            if (candidate.Id.IsEmpty)
            {
                error = EmptyCharacterIdValidation;
                return false;
            }

            character = candidate;
            error = string.Empty;
            return true;
        }

        private PlayerProfile CurrentProfile()
        {
            if (bootstrap == null || bootstrap.Profiles == null)
            {
                return null;
            }

            return bootstrap.Profiles.Current;
        }
    }
}
