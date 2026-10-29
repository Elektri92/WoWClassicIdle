namespace Models
{
    /// <summary>
    /// Rules for character names (based on WoW Classic): letters only, 2–12 characters,
    /// first letter uppercase, rest lowercase.
    /// </summary>
    public static class CharacterNames
    {
        public const int MinLength = 2;
        public const int MaxLength = 12;

        /// <summary>Normalizes a name to the spelling "Thrall" (trimmed, first letter uppercase).</summary>
        public static string Normalize(string? name)
        {
            var trimmed = (name ?? "").Trim();
            if (trimmed.Length == 0) return "";
            return char.ToUpperInvariant(trimmed[0]) + trimmed[1..].ToLowerInvariant();
        }

        /// <summary>Returns an error message (English, for the UI) or null if the name is valid.</summary>
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
