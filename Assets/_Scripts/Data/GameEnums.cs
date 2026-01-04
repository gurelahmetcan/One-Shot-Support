namespace OneShotSupport.Data
{
    /// <summary>
    /// Item categories that can match monster weaknesses
    /// </summary>
    public enum ItemCategory
    {
        Hygiene,
        Magic,
        Catering,
        Lighting
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

    /// <summary>
    /// Monster difficulty ranks affecting gold rewards
    /// </summary>
    public enum MonsterRank
    {
        D,  // 15 gold
        C,  // 30 gold
        B,  // 45 gold
        A,  // 60 gold
        S   // 100 gold
    }

    /// <summary>
    /// Types of item crates available during restock phase
    /// </summary>
    public enum CrateType
    {
        Cheap,      // 10 gold - 3 random items from any category
        Medium,     // 30 gold - 3 items from 2 shown random categories
        Premium     // 50 gold - 3 items from 1 player-selected category
    }
}
