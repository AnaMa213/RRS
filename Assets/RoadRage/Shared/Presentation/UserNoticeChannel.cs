using System;

namespace RoadRage.Shared.Presentation
{
    /// <summary>
    /// Canal C# pur pour publier et effacer les notices visibles par le joueur.
    /// L'Epic 2 y branchera les echecs reels (join, reseau, service) sans redessiner l'ecran.
    /// </summary>
    public sealed class UserNoticeChannel
    {
        public event Action<UserNotice> NoticePublished;

        public UserNotice? LastNotice { get; private set; }

        public void Publish(UserNotice notice)
        {
            LastNotice = notice;
            NoticePublished?.Invoke(notice);
        }

        public void Clear()
        {
            LastNotice = null;
        }
    }
}
