using OneShotSupport.Data;

namespace OneShotSupport.Utils
{
    /// <summary>
    /// Utility class to get perk modifiers
    /// Centralized location for perk balance values
    /// </summary>
    public static class PerkModifier
    {
        /// <summary>
        /// Get the percentage modifier for a given perk
        /// </summary>
        public static int GetModifier(Perk perk)
        {
            return perk switch
            {
                Perk.Clumsy => -10,        // Customer is clumsy, reduces success chance
                Perk.Prepared => 10,       // Customer came prepared, increases success chance
                Perk.Overconfident => 5,   // Slight bonus but risky (could affect reviews differently)
                Perk.None => 0,            // No modifier
                _ => 0
            };
        }

        /// <summary>
        /// Get a description of what the perk does
        /// </summary>
        public static string GetDescription(Perk perk)
        {
            return perk switch
            {
                Perk.Clumsy => "This hero is clumsy (-10% success chance)",
                Perk.Prepared => "This hero came prepared (+10% success chance)",
                Perk.Overconfident => "This hero is overconfident (+5% success chance)",
                Perk.None => "No special traits",
                _ => "Unknown perk"
            };
        }
    }
}
