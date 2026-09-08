namespace RoadRage.Features.Players
{
    /// <summary>
    /// Regle de validation du nom joueur (Story 1.3). Statique et purement C# : testable sans Unity,
    /// et reutilisable telle quelle par la validation cote hote de l'Epic 2.
    /// </summary>
    public static class PlayerNameValidator
    {
        public const int MinLength = 2;

        public const int MaxLength = 20;

        public const string EmptyError = "Nom requis : saisis un nom de joueur.";

        // Messages derives des bornes plutot que recopies : une borne modifiee sans son message
        // produirait un retour joueur faux qu'aucun test comparant aux constantes ne verrait.
        public static readonly string TooShortError = "Nom trop court : " + MinLength + " caracteres minimum.";

        public static readonly string TooLongError = "Nom trop long : " + MaxLength + " caracteres maximum.";

        public const string ForbiddenCharacterError = "Caracteres interdits : lettres, chiffres, espace, - et _ uniquement.";

        /// <summary>
        /// Normalise le texte brut saisi et dit s'il est acceptable. Le nom normalise est le texte
        /// debarrasse de ses espaces de bordure. Aucun echec silencieux : un refus renseigne toujours
        /// un message nommant la regle violee.
        /// </summary>
        public static bool TryNormalize(string raw, out string normalized, out string error)
        {
            normalized = string.Empty;

            if (string.IsNullOrWhiteSpace(raw))
            {
                error = EmptyError;
                return false;
            }

            var trimmed = raw.Trim();

            if (trimmed.Length < MinLength)
            {
                error = TooShortError;
                return false;
            }

            if (trimmed.Length > MaxLength)
            {
                error = TooLongError;
                return false;
            }

            for (var i = 0; i < trimmed.Length; i++)
            {
                if (!IsAllowed(trimmed[i]))
                {
                    error = ForbiddenCharacterError;
                    return false;
                }
            }

            normalized = trimmed;
            error = string.Empty;
            return true;
        }

        private static bool IsAllowed(char value)
        {
            if (char.IsLetterOrDigit(value))
            {
                return true;
            }

            return value == ' ' || value == '-' || value == '_';
        }
    }
}
