namespace OneShotSupport.Data
{
    /// <summary>
    /// Item categories that can match monster weaknesses
    /// </summary>
    public enum ItemCategory
    {
        Sharp,
        Heavy,
        Precise,
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
    /// Hero perks that modify success chance
    /// Based on personality traits for customer support context
    /// </summary>
    public enum Perk
    {
        None,           // No modifier
        Clumsy,         // Negative modifier
        Prepared,       // Positive modifier
        Overconfident   // Special behavior (TBD based on design)
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
