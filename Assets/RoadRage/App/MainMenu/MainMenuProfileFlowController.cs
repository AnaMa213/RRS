using System.Globalization;
using RoadRage.Features.Online;
using RoadRage.Features.Players;
using RoadRage.Features.UI;
using RoadRage.Shared.Presentation;
using UnityEngine;

namespace RoadRage.App.MainMenu
{
    /// <summary>
    /// Seule couture entre le menu principal, le catalogue de personnages, l'identite Steam et le
    /// depot de profil porte par le bootstrap (Story 4.5).
    /// Unique ecrivain du profil : l'ecran n'emet que des intentions et ne mute jamais l'etat de jeu.
    /// Le profil est resolu une seule fois a l'ouverture du menu, puis seuls les changements de
    /// selection le remplacent : une seconde resolution ecraserait le profil injecte par les tests et
    /// les parcours directs.
    /// Aucun chargement de scene, aucun etat reseau, et aucune ecriture quand Steam est indisponible.
    /// Aucun refus n'est silencieux : chaque echec publie une notice visible dans le menu.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MainMenuProfileFlowController : MonoBehaviour
    {
        public const string MissingCatalogMessage = PlayerProfileBootstrapService.EmptyCatalogMessage;

        public const string ProfileStoreUnavailableMessage = "Profil non enregistre : le depot de session est indisponible.";

        public const string ProfilePersistenceFailedMessage = "Profil non enregistre : ecriture sur disque impossible.";

        public const string MissingCharacterMessage = "Personnage indisponible : entree de catalogue manquante.";

        public const string SelectionFrozenMessage = "Selection de personnage gelee : elle ne peut plus changer pendant la session.";

        [SerializeField]
        private MainMenuScreen screen;

        [SerializeField]
        private CharacterCatalog catalog;

        private RoadRageBootstrap bootstrap;

        private PlayerProfileBootstrapService profileBootstrap;

        /// <summary>
        /// Autorisation d'ecrire sur disque, derivee de la disponibilite de l'identite Steam.
        /// A ne pas confondre avec <c>PlayerProfileResolution.ShouldPersist</c>, qui dit seulement si
        /// la resolution courante exige une ecriture corrective : s'en servir comme autorisation
        /// perdait silencieusement tout changement de personnage apres une relance normale (fichier
        /// deja valide, donc ShouldPersist faux).
        /// </summary>
        private bool canPersistProfile;

        /// <summary>
        /// Id du compte Steam proprietaire du profil persistant, capture a la resolution. Vide quand
        /// Steam est indisponible, auquel cas rien ne doit etre ecrit.
        /// </summary>
        private string profileOwnerSteamId = string.Empty;

        private int currentIndex;

        /// <summary>Index du personnage actuellement selectionne dans le catalogue.</summary>
        public int CurrentIndex
        {
            get { return currentIndex; }
        }

        private void Awake()
        {
            bootstrap = RoadRageBootstrap.EnsureInstance();

            if (catalog == null)
            {
                Debug.LogWarning("[App] MainMenuProfileFlowController sans reference vers CharacterCatalog : la selection de personnage restera inerte.");
            }

            if (bootstrap != null && bootstrap.ProfileFiles != null)
            {
                profileBootstrap = new PlayerProfileBootstrapService(catalog, bootstrap.ProfileFiles);
            }
            else
            {
                Debug.LogWarning("[App] MainMenuProfileFlowController sans depot de fichier de profil disponible.");
            }

            if (screen != null)
            {
                screen.CharacterOptionRequested += HandleCharacterOptionRequested;
            }
            else
            {
                Debug.LogWarning("[App] MainMenuProfileFlowController sans reference vers MainMenuScreen.");
            }
        }

        /// <summary>
        /// Resolution unique, apres tous les Awake de la scene : une resolution en Awake serait effacee
        /// par le ShowMenu() de l'ecran et la notice d'erreur Steam pourrait ne jamais s'afficher.
        /// </summary>
        private void Start()
        {
            ResolveProfileAtMenuOpen();
        }

        private void OnDestroy()
        {
            if (screen != null)
            {
                screen.CharacterOptionRequested -= HandleCharacterOptionRequested;
            }
        }

        /// <summary>
        /// Tente l'initialisation Steam (idempotente avec le flux lobby), lit l'identite locale, puis
        /// publie le profil persistant ou recree. Chaque refus est visible.
        /// </summary>
        private void ResolveProfileAtMenuOpen()
        {
            if (bootstrap == null || bootstrap.Profiles == null)
            {
                PublishNotice(ProfileStoreUnavailableMessage, UserNoticeSeverity.Warning);
                return;
            }

            // Le bootstrap est DontDestroyOnLoad : une session precedente peut avoir laisse le depot
            // gele. Le menu est la seule surface de selection, donc sa (re)ouverture leve le gel avant
            // toute lecture et tout Set (Story 4.6), sinon le menu resterait en lecture seule.
            bootstrap.Profiles.Unfreeze();

            if (profileBootstrap == null || catalog == null)
            {
                PublishNotice(MissingCatalogMessage, UserNoticeSeverity.Warning);
                return;
            }

            if (bootstrap.OnlineServices != null)
            {
                bootstrap.OnlineServices.TryInitialize();
            }

            var steamName = string.Empty;
            var localSteamId = 0UL;
            var steamAvailable = bootstrap.OnlineServices != null
                && bootstrap.OnlineServices.Status == OnlineServicesStatus.Online
                && bootstrap.SteamIdentity != null
                && bootstrap.SteamIdentity.TryGetLocalIdentity(out steamName, out localSteamId);

            // L'id du compte est jete quand Steam est indisponible : un profil sans proprietaire ne doit
            // jamais etre ecrit, sinon le compte Steam suivant adopterait le choix du precedent.
            profileOwnerSteamId = steamAvailable ? localSteamId.ToString(CultureInfo.InvariantCulture) : string.Empty;

            var resolution = profileBootstrap.Resolve(steamAvailable, steamName, profileOwnerSteamId);
            if (!resolution.IsResolved)
            {
                PublishNotice(resolution.Error, UserNoticeSeverity.Warning);
                return;
            }

            canPersistProfile = steamAvailable;

            // Un depot gele refuserait la mutation : ne rien ecrire et rendre le refus visible plutot
            // que de laisser la session croire que la selection a change (Story 4.6).
            if (!bootstrap.Profiles.Set(resolution.Profile))
            {
                RefuseFrozenSelectionChange();
                return;
            }

            if (canPersistProfile && resolution.ShouldPersist)
            {
                if (!profileBootstrap.TryPersist(resolution.Profile, profileOwnerSteamId, true))
                {
                    PublishNotice(ProfilePersistenceFailedMessage, UserNoticeSeverity.Warning);
                }
            }

            if (!string.IsNullOrEmpty(resolution.Error))
            {
                PublishNotice(resolution.Error, UserNoticeSeverity.Warning);
            }

            Debug.Log("[Players] Profil du menu : " + resolution.Profile.DisplayName + " / " + resolution.Profile.CharacterId.Value + (canPersistProfile ? " (persistable)" : " (session)"));
            ShowProfile(resolution.Profile);
        }

        /// <summary>
        /// Remplace le personnage du profil courant et le persiste si la session le permet. Le nom
        /// affiche reste celui deja resolu : le menu ne propose aucune saisie de nom.
        /// </summary>
        private void HandleCharacterOptionRequested(MainMenuScreen.CharacterOption option)
        {
            if (bootstrap == null || bootstrap.Profiles == null || profileBootstrap == null || catalog == null || catalog.Count == 0)
            {
                return;
            }

            var character = catalog.GetAt((int)option);
            if (character == null || character.Id.IsEmpty)
            {
                Debug.LogWarning("[Players] " + MissingCharacterMessage);
                PublishNotice(MissingCharacterMessage, UserNoticeSeverity.Warning);
                return;
            }

            var previous = bootstrap.Profiles.Current;
            var displayName = previous != null ? previous.DisplayName : character.DisplayName;
            var profile = new PlayerProfile(displayName, character.Id);

            // Meme garde que la resolution initiale : sous gel, ni mutation, ni ProfileChanged, ni
            // ecriture disque, et le refus reste visible dans le menu (Story 4.6).
            if (!bootstrap.Profiles.Set(profile))
            {
                RefuseFrozenSelectionChange();
                return;
            }

            if (canPersistProfile && !profileBootstrap.TryPersist(profile, profileOwnerSteamId, true))
            {
                PublishNotice(ProfilePersistenceFailedMessage, UserNoticeSeverity.Warning);
            }
            Debug.Log("[Players] Personnage selectionne : " + character.RawId);

            ShowProfile(profile);
        }

        /// <summary>
        /// Refus visible d'un changement de personnage demande sous gel (Story 4.6) : l'ecran ne doit
        /// jamais laisser croire qu'une selection gelee a ete remplacee, et le refus n'est jamais
        /// seulement journalise.
        /// </summary>
        private void RefuseFrozenSelectionChange()
        {
            Debug.LogWarning("[Players] " + SelectionFrozenMessage);
            PublishNotice(SelectionFrozenMessage, UserNoticeSeverity.Warning);
        }

        private void ShowProfile(PlayerProfile profile)
        {
            if (catalog != null)
            {
                var index = catalog.IndexOf(profile.CharacterId);
                currentIndex = index >= 0 ? index : 0;
            }

            var character = catalog != null ? catalog.GetAt(currentIndex) : null;
            if (character == null || character.Id.IsEmpty)
            {
                PublishNotice(MissingCharacterMessage, UserNoticeSeverity.Warning);
                return;
            }

            if (screen == null)
            {
                return;
            }

            // Le libelle presente le personnage selectionne, pas le nom du joueur : c'est le choix
            // courant qui doit rester lisible sous l'apercu, et il doit rester du texte, jamais la
            // seule couleur du modele.
            screen.ShowCharacter(character.DisplayName, character.PreviewPrefab, character.PreviewTint, (MainMenuScreen.CharacterOption)currentIndex);
        }

        private void PublishNotice(string message, UserNoticeSeverity severity)
        {
            if (string.IsNullOrEmpty(message) || bootstrap == null || bootstrap.Notices == null)
            {
                return;
            }

            bootstrap.Notices.Publish(new UserNotice(severity, message));
        }
    }
}
