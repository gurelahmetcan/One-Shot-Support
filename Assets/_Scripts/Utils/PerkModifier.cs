using OneShotSupport.Data;

namespace OneShotSupport.Utils
{
    /// <summary>
    /// Comprehensive perk system handling all perk mechanics
    /// </summary>
    public static class PerkModifier
    {
        // === BASE CHANCE MODIFIERS ===

        /// <summary>
        /// Get the base chance modifier for a perk
        /// </summary>
        public static int GetBaseChanceModifier(Perk perk)
        {
            return perk switch
            {
                Perk.Clumsy => -10,
                Perk.Cursed => 40,  // High base, but items reduce it
                _ => 0
            };
        }

        // === SLOT MODIFIERS ===

        /// <summary>
        /// Get the slot count modifier for a perk
        /// </summary>
        public static int GetSlotModifier(Perk perk)
        {
            return perk switch
            {
                Perk.Overconfident => -1,
                Perk.Prepared => 1,
                _ => 0
            };
        }

        // === ITEM BOOST MODIFIERS ===

        /// <summary>
        /// Modify an item's base boost based on perk
        /// </summary>
        public static int ModifyBaseBoost(Perk perk, int originalBoost)
        {
            return perk switch
            {
                Perk.GlassCannon => originalBoost / 2,  // Halved
                _ => originalBoost
            };
        }

        /// <summary>
        /// Modify an item's match bonus based on perk
        /// </summary>
        public static int ModifyMatchBonus(Perk perk, int originalBonus)
        {
            return perk switch
            {
                Perk.GlassCannon => originalBonus * 2,  // Doubled
                _ => originalBonus
            };
        }

        // === MONSTER PENALTY MODIFIERS ===

        /// <summary>
        /// Check if perk ignores monster difficulty penalty
        /// </summary>
        public static bool IgnoresMonsterPenalty(Perk perk)
        {
            return perk == Perk.Fearless;
        }

        // === SUCCESS CHANCE MODIFIERS ===

        /// <summary>
        /// Apply minimum success chance floor if perk has one
        /// </summary>
        public static int ApplySuccessFloor(Perk perk, int calculatedChance)
        {
            if (perk == Perk.Lucky)
            {
                return calculatedChance < 25 ? 25 : calculatedChance;
            }
            return calculatedChance;
        }

        /// <summary>
        /// Get per-item penalty for perks like Cursed
        /// </summary>
        public static int GetPerItemPenalty(Perk perk)
        {
            return perk switch
            {
                Perk.Cursed => -10,  // -10% per equipped item
                _ => 0
            };
        }

        // === REPUTATION MODIFIERS ===

        /// <summary>
        /// Modify reputation change based on perk
        /// </summary>
        public static int ModifyReputationChange(Perk perk, int originalChange, bool isSuccess)
        {
            if (perk == Perk.SocialMediaStar)
            {
                if (isSuccess)
                    return originalChange + (originalChange / 2);  // +50%
                else
                    return originalChange - (originalChange / 2);  // -50% (less negative)
            }
            return originalChange;
        }

        // === CROSS-HERO EFFECTS ===

        /// <summary>
        /// Get bonus to apply to next hero if this hero succeeds
        /// </summary>
        public static int GetInspiringBonus(Perk perk, bool heroSucceeded)
        {
            if (perk == Perk.Inspiring && heroSucceeded)
                return 10;
            return 0;
        }

        // === UI FLAGS ===

        /// <summary>
        /// Check if confidence meter should be hidden
        /// </summary>
        public static bool HidesConfidenceMeter(Perk perk)
        {
            return perk == Perk.Honest;
        }

        // === PERK INFO ===

        /// <summary>
        /// Get perk rarity
        /// </summary>
        public static PerkRarity GetRarity(Perk perk)
        {
            return perk switch
            {
                Perk.Clumsy or Perk.Overconfident or Perk.Prepared or Perk.Honest => PerkRarity.Common,
                Perk.GlassCannon or Perk.SocialMediaStar or Perk.Lucky or Perk.Fearless or Perk.Inspiring => PerkRarity.Rare,
                Perk.Cursed => PerkRarity.Legendary,
                _ => PerkRarity.Common
            };
        }

        /// <summary>
        /// Get a description of what the perk does
        /// </summary>
        public static string GetDescription(Perk perk)
        {
            return perk switch
            {
                // Common
                Perk.Clumsy => "The Clumsy: -10% Base Success Chance",
                Perk.Overconfident => "The Overconfident: -1 Item Slot",
                Perk.Prepared => "The Prepared: +1 Item Slot",
                Perk.Honest => "The Honest: Confidence Meter is hidden",

                // Rare
                Perk.GlassCannon => "The Glass Cannon: Match bonuses doubled, base boosts halved",
                Perk.SocialMediaStar => "The Social Media Star: +50% reputation on success, -50% loss on failure",
                Perk.Lucky => "The Lucky: Success chance cannot drop below 25%",
                Perk.Fearless => "The Fearless: Ignores monster difficulty penalty",
                Perk.Inspiring => "The Inspiring: +10% base chance to next hero on success",

                // Legendary
                Perk.Cursed => "The Cursed: +40% base chance, but -10% per equipped item",

                Perk.None => "No special traits",
                _ => "Unknown perk"
            };
        }
    }
}
