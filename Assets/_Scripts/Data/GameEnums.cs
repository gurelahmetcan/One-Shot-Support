namespace OneShotSupport.Data
{
    /// <summary>
    /// Item categories that can match monster weaknesses
    /// </summary>
    public enum ItemCategory
    {
        Fire,
        Water,
        Electric,
        Magic
    }

    /// <summary>
    /// Hero tier levels affecting base chance
    /// </summary>
    public enum HeroTier
    {
        Noob,
        Knight,
        Legend
    }

    /// <summary>
    /// Hero perks that modify success chance and game mechanics
    /// Includes common, rare, and legendary perks
    /// </summary>
    public enum Perk
    {
        None,               // No modifier

        // Common Perks
        Clumsy,             // -10% Base Success Chance
        Overconfident,      // -1 Item Slot
        Prepared,           // +1 Item Slot
        Honest,             // Confidence Meter hidden (UI only)

        // Rare Perks
        GlassCannon,        // 2x match bonus, 0.5x base boost
        SocialMediaStar,    // +50% rep gain on success, -50% rep loss on failure
        Lucky,              // Success chance floor of 25%
        Fearless,           // Ignores monster difficulty penalty
        Inspiring,          // +10% base to next hero on success

        // Legendary Perks
        Cursed              // +40% base, -10% per equipped item
    }

    /// <summary>
    /// Perk rarity for UI display and generation
    /// </summary>
    public enum PerkRarity
    {
        Common,
        Rare,
        Legendary
    }

    /// <summary>
    /// Confidence levels for the review system
    /// </summary>
    public enum ConfidenceLevel
    {
        Low,      // < 40%
        Medium,   // 40-79%
        High      // >= 80%
    }
}
