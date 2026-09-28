using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoadRage.Features.Players;
using RoadRage.Shared.Definitions;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Story 4.6 : gel du profil, payload de session et spawn du personnage selectionne.
    /// Couvre le contrat du depot (gel, refus de mutation, idempotence, degel), la lecture unique de la
    /// selection gelee en aval du menu, les gardes qui empechent une mutation refusee d'atteindre le
    /// disque, et la purete de l'etat de session (qui n'ecrit jamais le profil persistant).
    /// </summary>
    [Category("Core")]
    public sealed class Story46ProfileFreezeSessionPayloadAndSelectedCharacterSpawnTests
    {
        private const string MenuFlowPath = "Assets/RoadRage/App/MainMenu/MainMenuFlowController.cs";
        private const string MenuProfileFlowPath = "Assets/RoadRage/App/MainMenu/MainMenuProfileFlowController.cs";
        private const string LobbyFlowPath = "Assets/RoadRage/App/Lobby/LobbyFlowController.cs";
        private const string RunFlowPath = "Assets/RoadRage/App/Run/RunFlowController.cs";
        private const string SpawnServicePath = "Assets/RoadRage/App/Run/NetworkedPlayerSpawnService.cs";
        private const string PresentationPath = "Assets/RoadRage/Features/Players/NetworkedPlayerPresentation.cs";
        private const string PlayerStatePath = "Assets/RoadRage/Features/Players/NetworkedPlayerState.cs";

        private const string RookieId = "char_rookie";
        private const string VeteranId = "char_veteran";

        // ---------------------------------------------------------------- Contrat du depot gele

        [Test]
        public void FrozenSelectionIsTheSnapshotTakenAtLobbyEntry()
        {
            var store = new PlayerProfileStore();
            var rookie = new PlayerProfile("Kenan", new DefinitionId(RookieId));
            store.Set(rookie);

            store.Freeze();

            Assert.That(store.IsFrozen, Is.True);
            Assert.That(store.SessionSelection, Is.SameAs(rookie), "l'instantane est la selection affichee a l'entree");
            Assert.That(store.Current, Is.SameAs(rookie), "le profil du menu n'est pas efface par le gel");
            Assert.That(store.HasProfile, Is.True);
        }

        [Test]
        public void MutationUnderFreezeIsRefusedWithoutTouchingTheStoreOrRaisingProfileChanged()
        {
            var store = new PlayerProfileStore();
            var rookie = new PlayerProfile("Kenan", new DefinitionId(RookieId));
            store.Set(rookie);
            store.Freeze();

            var changed = 0;
            store.ProfileChanged += profile => changed++;

            var accepted = store.Set(new PlayerProfile("Kenan", new DefinitionId(VeteranId)));

            Assert.That(accepted, Is.False, "un depot gele doit refuser la mutation");
            Assert.That(changed, Is.Zero, "un refus ne doit lever aucun ProfileChanged");
            Assert.That(store.Current, Is.SameAs(rookie), "aucune mutation ne doit atteindre le depot gele");
            Assert.That(store.SessionSelection, Is.SameAs(rookie), "la selection gelee reste celle capturee");
        }

        /// <summary>
        /// Play en solo puis room Open en hote : le second gel ne recapture rien, sans quoi une ecriture
        /// intermediaire pourrait redefinir la selection d'une session deja commencee.
        /// </summary>
        [Test]
        public void FreezeIsIdempotentAndKeepsTheFirstSnapshot()
        {
            var store = new PlayerProfileStore();
            var rookie = new PlayerProfile("Kenan", new DefinitionId(RookieId));
            store.Set(rookie);

            store.Freeze();
            var firstSnapshot = store.SessionSelection;
            store.Freeze();

            Assert.That(store.IsFrozen, Is.True);
            Assert.That(store.SessionSelection, Is.SameAs(firstSnapshot));
            Assert.That(store.SessionSelection.CharacterId, Is.EqualTo(rookie.CharacterId));
        }

        [Test]
        public void UnfreezeRestoresMutabilityAndDropsTheSessionSnapshot()
        {
            var store = new PlayerProfileStore();
            store.Set(new PlayerProfile("Kenan", new DefinitionId(RookieId)));
            store.Freeze();

            store.Unfreeze();

            var veteran = new PlayerProfile("Kenan", new DefinitionId(VeteranId));
            Assert.That(store.IsFrozen, Is.False);
            Assert.That(store.Set(veteran), Is.True, "le menu rouvert doit redevenir modifiable");
            Assert.That(store.Current, Is.SameAs(veteran));
            Assert.That(store.SessionSelection, Is.SameAs(veteran), "hors gel, la selection de session suit le profil courant");
        }

        [Test]
        public void SelectionReadsTheLiveProfileUntilTheFreeze()
        {
            var store = new PlayerProfileStore();
            Assert.That(store.SessionSelection, Is.Null);
            Assert.That(store.HasProfile, Is.False);

            var changed = 0;
            store.ProfileChanged += profile => changed++;

            var rookie = new PlayerProfile("Kenan", new DefinitionId(RookieId));
            Assert.That(store.Set(rookie), Is.True);
            Assert.That(changed, Is.EqualTo(1), "hors gel, un Set accepte leve ProfileChanged");

            var veteran = new PlayerProfile("Kenan", new DefinitionId(VeteranId));
            store.Set(veteran);
            Assert.That(store.SessionSelection, Is.SameAs(veteran), "hors gel, SessionSelection reflete le profil courant, sans conteneur parallele");
        }

        [Test]
        public void SetStillRejectsANullProfileUnderFreeze()
        {
            var store = new PlayerProfileStore();
            store.Freeze();

            Assert.Throws<ArgumentNullException>(() => store.Set(null));
        }

        // ---------------------------------------------------------------- Gel, degel et persistance

        /// <summary>
        /// Le bootstrap survit aux scenes : un depot gele par une session precedente doit etre degèle a
        /// la (re)ouverture du menu, avant tout Set, sinon le menu resterait en lecture seule.
        /// </summary>
        [Test]
        public void InheritedFrozenStoreIsReleasedWhenTheMenuReopens()
        {
            var store = new PlayerProfileStore();
            store.Set(new PlayerProfile("Kenan", new DefinitionId(RookieId)));
            store.Freeze();

            Assert.That(store.Set(new PlayerProfile("Kenan", new DefinitionId(VeteranId))), Is.False,
                "un depot herite gele refuse encore les mutations");

            store.Unfreeze();

            Assert.That(store.IsFrozen, Is.False);
            Assert.That(store.Set(new PlayerProfile("Kenan", new DefinitionId(VeteranId))), Is.True,
                "le menu rouvert doit retrouver une selection modifiable");

            var menuProfileSource = File.ReadAllText(MenuProfileFlowPath);
            var resolveIndex = menuProfileSource.IndexOf("private void ResolveProfileAtMenuOpen()", StringComparison.Ordinal);
            var unfreezeIndex = menuProfileSource.IndexOf("bootstrap.Profiles.Unfreeze()", StringComparison.Ordinal);
            var setIndex = menuProfileSource.IndexOf("bootstrap.Profiles.Set(", StringComparison.Ordinal);

            Assert.That(resolveIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(unfreezeIndex, Is.GreaterThan(resolveIndex), "le degel doit appartenir a la resolution du menu");
            Assert.That(setIndex, Is.GreaterThan(unfreezeIndex), "le degel doit preceder tout Set de la resolution");
        }

        [Test]
        public void MainMenuFreezesOnLobbyEntryAndUnfreezesOnReturn()
        {
            var source = File.ReadAllText(MenuFlowPath);

            Assert.That(source, Does.Contain("screen.PlayRequested += EnterLobbyShell;"),
                "l'entree de la coquille lobby doit passer par le gel, pas directement par ShowSetupPlaceholder");
            Assert.That(source, Does.Contain("bootstrap.Profiles.Freeze()"), "Play en solo doit geler la selection");
            Assert.That(source, Does.Contain("bootstrap.Profiles.Unfreeze()"), "ReturnToMenu doit lever le gel en solo");
        }

        /// <summary>
        /// Le refus de mutation ne protege pas le disque a lui seul : MainMenuProfileFlowController
        /// enchaine Set puis TryPersist, donc les deux appels doivent etre gardes, dans cet ordre.
        /// </summary>
        [Test]
        public void MenuProfileFlowGuardsBothWritesAgainstTheSessionFreeze()
        {
            var source = File.ReadAllText(MenuProfileFlowPath);

            Assert.That(CountOccurrences(source, "bootstrap.Profiles.Set("), Is.EqualTo(2),
                "les deux ecritures du menu (resolution initiale et clic d'emplacement) doivent passer par Set");
            Assert.That(CountOccurrences(source, "if (!bootstrap.Profiles.Set("), Is.EqualTo(2),
                "chaque Set du menu doit refuser explicitement le gel, sinon une mutation refusee atteindrait le disque");
            Assert.That(CountOccurrences(source, "RefuseFrozenSelectionChange();"), Is.EqualTo(2),
                "chaque refus de gel doit publier une notice visible, jamais seulement un log");
            Assert.That(CountOccurrences(source, "profileBootstrap.TryPersist("), Is.EqualTo(2));

            var firstGuardedSet = source.IndexOf("if (!bootstrap.Profiles.Set(", StringComparison.Ordinal);
            var secondGuardedSet = source.IndexOf("if (!bootstrap.Profiles.Set(", firstGuardedSet + 1, StringComparison.Ordinal);
            var firstPersist = source.IndexOf("profileBootstrap.TryPersist(", StringComparison.Ordinal);
            var secondPersist = source.IndexOf("profileBootstrap.TryPersist(", firstPersist + 1, StringComparison.Ordinal);

            Assert.That(firstPersist, Is.GreaterThan(firstGuardedSet),
                "aucune ecriture disque ne doit preceder la garde de gel de la resolution initiale");
            Assert.That(secondPersist, Is.GreaterThan(secondGuardedSet),
                "aucune ecriture disque ne doit preceder la garde de gel du clic d'emplacement");
        }

        // ---------------------------------------------------------------- Payload et lecture unique

        [Test]
        public void SessionPayloadCarriesTheFrozenSelectionNotTheLiveProfile()
        {
            var store = new PlayerProfileStore();
            store.Set(new PlayerProfile("Kenan", new DefinitionId(RookieId)));
            store.Freeze();

            Assert.That(store.Set(new PlayerProfile("Kenan", new DefinitionId(VeteranId))), Is.False,
                "le payload ne doit jamais pouvoir suivre une mutation refusee");

            var selection = store.SessionSelection;
            Assert.That(selection, Is.Not.Null);

            var payload = NetworkPlayerConnectionPayload.Encode(selection.DisplayName, selection.CharacterId.Value);
            Assert.That(NetworkPlayerConnectionPayload.TryDecode(payload, out var displayName, out var characterId), Is.True);
            Assert.That(displayName, Is.EqualTo("Kenan"));
            Assert.That(characterId, Is.EqualTo(RookieId), "le payload transporte la selection gelee, jamais une lecture live");
        }

        [Test]
        public void LobbyFlowFreezesOnBothEntriesAndReadsOnlyTheSessionSelection()
        {
            var source = File.ReadAllText(LobbyFlowPath);

            Assert.That(CountOccurrences(source, "FreezeProfileSelection();"), Is.EqualTo(2),
                "les deux entrees du lobby multi (room Open, join Joined) doivent geler la selection");
            Assert.That(source, Does.Contain("bootstrap.Profiles.SessionSelection"),
                "le payload de session et la publication roster lisent la selection gelee");
            Assert.That(source, Does.Not.Contain("bootstrap.Profiles.Current"),
                "plus aucune lecture live du profil en aval du menu");

            var openCaseIndex = source.IndexOf("case LobbyRoomStatus.Open:", StringComparison.Ordinal);
            var joinedCaseIndex = source.IndexOf("case LobbyJoinStatus.Joined:", StringComparison.Ordinal);
            var firstFreeze = source.IndexOf("FreezeProfileSelection();", StringComparison.Ordinal);
            var secondFreeze = source.IndexOf("FreezeProfileSelection();", firstFreeze + 1, StringComparison.Ordinal);

            Assert.That(openCaseIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(joinedCaseIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(firstFreeze, Is.GreaterThan(openCaseIndex));
            Assert.That(firstFreeze, Is.LessThan(source.IndexOf("PublishLocalProfile();", openCaseIndex, StringComparison.Ordinal)),
                "le gel hote doit preceder la publication au roster");
            Assert.That(secondFreeze, Is.GreaterThan(joinedCaseIndex));
            Assert.That(secondFreeze, Is.LessThan(source.IndexOf("PublishLocalProfile();", joinedCaseIndex, StringComparison.Ordinal)),
                "le gel invite doit preceder la publication au roster");
        }

        [Test]
        public void RunFlowSpawnsFromTheSessionSelection()
        {
            var source = File.ReadAllText(RunFlowPath);

            Assert.That(source, Does.Contain("bootstrap.Profiles.SessionSelection.CharacterId"),
                "le spawn solo doit resoudre le personnage depuis la selection gelee");
            Assert.That(source, Does.Not.Contain("bootstrap.Profiles.Current"),
                "le solo ne doit plus lire le profil courant");
        }

        // ---------------------------------------------------------------- Purete de l'etat de session

        /// <summary>
        /// Aucun chemin de session (run, spawn reseau, presentation, etat reseau) ne doit ecrire le
        /// profil, ni contourner le depot pour atteindre le fichier.
        /// </summary>
        [Test]
        public void SessionStateNeverWritesThePlayerProfile()
        {
            foreach (var path in new[] { RunFlowPath, SpawnServicePath, PresentationPath, PlayerStatePath })
            {
                var source = File.ReadAllText(path);

                foreach (var forbidden in new[]
                         {
                             "Profiles.Set(",
                             "Profiles.Freeze(",
                             "Profiles.Unfreeze(",
                             "ProfileFiles",
                             "PlayerProfileFileStore",
                             "TryPersist",
                             "TrySave"
                         })
                {
                    Assert.That(source, Does.Not.Contain(forbidden),
                        Path.GetFileName(path) + " ne doit jamais ecrire le profil joueur : " + forbidden);
                }
            }
        }

        [Test]
        public void PersistentProfileRecordKeepsOnlyIdentityAndCosmeticChoice()
        {
            var fields = typeof(PersistentPlayerProfileRecord)
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Select(field => field.Name)
                .ToArray();

            Assert.That(fields, Is.EquivalentTo(new[] { "displayName", "characterId", "steamId" }),
                "aucun etat de session (siege, vie, monnaie, lobby, run) ne doit entrer dans le profil persistant");
        }

        // ---------------------------------------------------------------- Helpers

        private static int CountOccurrences(string source, string text)
        {
            var count = 0;
            var index = source.IndexOf(text, StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index = source.IndexOf(text, index + text.Length, StringComparison.Ordinal);
            }

            return count;
        }
    }
}
