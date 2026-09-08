using System;
using RoadRage.Shared.Domain;

namespace RoadRage.Features.Online
{
    /// <summary>
    /// Service unique de synchronisation du roster de lobby : membres connectes, etat pret par joueur,
    /// et difficulte de partie partagee via les donnees de lobby Steam (Story 2.4). Objet C# pur, sans
    /// dependance Unity : suit LobbyRoomService et LobbyJoinService pour savoir quand un lobby est actif,
    /// puis interroge ISteamLobbyPlatform a chaque Tick() pour detecter les changements distants (membres,
    /// etats prets, difficulte), comme OnlineServicesBootstrapService le fait deja pour les callbacks
    /// Steamworks generaux. Ne mute jamais MatchSettings ni le moindre etat de gameplay partage.
    /// </summary>
    public sealed class LobbyRosterService
    {
        private readonly ISteamLobbyPlatform platform;

        private readonly LobbyRoomService lobbyRoom;

        private readonly LobbyJoinService lobbyJoin;

        private LobbyRosterSnapshot current = LobbyRosterSnapshot.Empty;

        public LobbyRosterService(ISteamLobbyPlatform platform, LobbyRoomService lobbyRoom, LobbyJoinService lobbyJoin)
        {
            if (platform == null)
            {
                throw new ArgumentNullException(nameof(platform));
            }

            if (lobbyRoom == null)
            {
                throw new ArgumentNullException(nameof(lobbyRoom));
            }

            if (lobbyJoin == null)
            {
                throw new ArgumentNullException(nameof(lobbyJoin));
            }

            this.platform = platform;
            this.lobbyRoom = lobbyRoom;
            this.lobbyJoin = lobbyJoin;

            lobbyRoom.StatusChanged += HandleRoomStatusChanged;
            lobbyJoin.StatusChanged += HandleJoinStatusChanged;
        }

        public LobbyRosterSnapshot Current
        {
            get { return current; }
        }

        public bool LocalReady { get; private set; }

        /// <summary>Vrai des que le lobby actif expose au moins un membre : condition de synchronisation du roster pour le gate de lancement (Story 2.4).</summary>
        public bool IsSynchronized
        {
            get { return current.HasLobby && current.Members.Length > 0; }
        }

        /// <summary>Vrai quand tous les membres connectes sont prets. Roster non synchronise compte comme non pret.</summary>
        public bool AllMembersReady
        {
            get
            {
                if (!IsSynchronized)
                {
                    return false;
                }

                foreach (var member in current.Members)
                {
                    if (!member.Ready)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public event Action<LobbyRosterSnapshot> RosterChanged;

        /// <summary>
        /// Interroge la plateforme pour un lobby actif et republie un evenement si le roster a change.
        /// A appeler depuis une boucle Update par le proprietaire persistant, comme
        /// OnlineServicesBootstrapService.Tick(). Sans effet hors lobby actif (ni hote, ni rejoint).
        /// </summary>
        public void Tick()
        {
            if (lobbyRoom.Status != LobbyRoomStatus.Open && lobbyJoin.Status != LobbyJoinStatus.Joined)
            {
                return;
            }

            var snapshot = platform.GetRosterSnapshot();
            if (!SnapshotsEqual(snapshot, current))
            {
                SetCurrent(snapshot);
            }
        }

        /// <summary>Publie l'etat pret local dans les donnees de membre du lobby. Sans effet hors lobby actif.</summary>
        public void SetLocalReady(bool ready)
        {
            LocalReady = ready;

            if (lobbyRoom.Status != LobbyRoomStatus.Open && lobbyJoin.Status != LobbyJoinStatus.Joined)
            {
                return;
            }

            platform.SetLocalMemberReady(ready);
        }

        /// <summary>Publie la difficulte choisie par l'hote dans les donnees de lobby. Sans effet hors room hote ouverte ; reserve a l'hote par l'appelant.</summary>
        public void PublishDifficulty(Difficulty difficulty)
        {
            if (lobbyRoom.Status != LobbyRoomStatus.Open)
            {
                return;
            }

            platform.SetLobbyDifficulty(difficulty);
        }

        /// <summary>
        /// Une room hote qui s'ouvre republie systematiquement l'etat pret local a "non pret" : un hote
        /// qui recree une room ne doit jamais heriter d'un etat pret perime d'une session precedente.
        /// </summary>
        private void HandleRoomStatusChanged(LobbyRoomStatus status)
        {
            if (status == LobbyRoomStatus.Open)
            {
                LocalReady = false;
                platform.SetLocalMemberReady(false);
            }
            else if (current.HasLobby)
            {
                SetCurrent(LobbyRosterSnapshot.Empty);
            }
        }

        /// <summary>
        /// Le leave explicite d'un lobby rejoint reste hors scope (Story 2.3) : seul le passage a Joined
        /// est pertinent ici, la remise a zero du roster est portee par HandleRoomStatusChanged cote hote.
        /// </summary>
        private void HandleJoinStatusChanged(LobbyJoinStatus status)
        {
            if (status == LobbyJoinStatus.Joined)
            {
                LocalReady = false;
                platform.SetLocalMemberReady(false);
            }
        }

        private void SetCurrent(LobbyRosterSnapshot snapshot)
        {
            current = snapshot;
            RaiseRosterChanged(snapshot);
        }

        private void RaiseRosterChanged(LobbyRosterSnapshot snapshot)
        {
            var handler = RosterChanged;
            if (handler != null)
            {
                handler(snapshot);
            }
        }

        private static bool SnapshotsEqual(LobbyRosterSnapshot a, LobbyRosterSnapshot b)
        {
            if (a.HasLobby != b.HasLobby || a.OwnerId != b.OwnerId || a.Difficulty != b.Difficulty)
            {
                return false;
            }

            if (a.Members.Length != b.Members.Length)
            {
                return false;
            }

            for (var i = 0; i < a.Members.Length; i++)
            {
                var left = a.Members[i];
                var right = b.Members[i];
                if (left.SteamId != right.SteamId || left.Ready != right.Ready || left.DisplayName != right.DisplayName)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
