namespace Models
{
    /// <summary>
    /// Regeln für Charakternamen (angelehnt an WoW Classic): nur Buchstaben, 2–12 Zeichen,
    /// erster Buchstabe groß, Rest klein.
    /// </summary>
    public static class CharacterNames
    {
        public const int MinLength = 2;
        public const int MaxLength = 12;

        /// <summary>Bringt einen Namen in die Schreibweise „Thrall“ (getrimmt, erster Buchstabe groß).</summary>
        public static string Normalize(string? name)
        {
            var trimmed = (name ?? "").Trim();
            if (trimmed.Length == 0) return "";
            return char.ToUpperInvariant(trimmed[0]) + trimmed[1..].ToLowerInvariant();
        }

        /// <summary>Liefert eine Fehlermeldung (Englisch, für die UI) oder null, wenn der Name gültig ist.</summary>
        public static string? Validate(string? name)
        {
            var normalized = Normalize(name);
            if (normalized.Length == 0)
                return "Please enter a name for your character.";
            if (normalized.Length < MinLength || normalized.Length > MaxLength)
                return $"Names must be {MinLength}–{MaxLength} letters long.";
            if (!normalized.All(char.IsLetter))
                return "Names may only contain letters.";
            return null;
        }
    }
}
