using System;

namespace RoadRage.Shared.Presentation
{
    /// <summary>
    /// Niveau de severite d'une notice utilisateur.
    /// </summary>
    public enum UserNoticeSeverity
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Notice visible destinee au joueur (info, avertissement ou erreur).
    /// Vocabulaire partage des retours visibles, reutilise par les epics suivantes.
    /// </summary>
    public readonly struct UserNotice : IEquatable<UserNotice>
    {
        public UserNotice(UserNoticeSeverity severity, string message)
        {
            Severity = severity;
            Message = message ?? string.Empty;
        }

        public UserNoticeSeverity Severity { get; }

        public string Message { get; }

        public bool Equals(UserNotice other)
        {
            return Severity == other.Severity && Message == other.Message;
        }

        public override bool Equals(object obj)
        {
            return obj is UserNotice other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Severity, Message);
        }
    }
}
