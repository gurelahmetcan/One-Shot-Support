using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;
using OneShotSupport.Utils;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Core calculation system for one-shot success percentage
    /// Now supports all perk mechanics from Perks.txt
    /// </summary>
    public static class OneShotCalculator
    {
        private const int MIN_CHANCE = 1;
        private const int MAX_CHANCE = 99;

        /// <summary>
        /// Calculate the final success percentage for a hero against a monster
        /// Implements the full formula with all perk mechanics
        /// </summary>
        /// <param name="hero">The hero data</param>
        /// <param name="monster">The monster data</param>
        /// <param name="equippedItems">List of items equipped to the hero</param>
        /// <param name="inspiringBonus">Bonus from previous hero's Inspiring perk (default 0)</param>
        /// <returns>Final success percentage (clamped 1-99)</returns>
        public static int CalculateSuccessChance(
            HeroData hero,
            MonsterData monster,
            List<ItemData> equippedItems,
            int inspiringBonus = 0)
        {
            var perk = hero.perk;
            var validItems = equippedItems.Where(item => item != null).ToList();

            // 1. BASE CHANCE (B)
            int baseChance = hero.baseChance;

            // 2. PERK MODIFIER (P_hero)
            int perkModifier = PerkModifier.GetBaseChanceModifier(perk);

            // 3. INSPIRING BONUS (from previous hero)
            int inspiring = inspiringBonus;

            // 4. ITEM BOOSTS (Σ(I_n)) - with perk modifications
            int itemBoosts = 0;
            foreach (var item in validItems)
            {
                bool isMatch = item.category == monster.weakness;
                int boost;

                if (isMatch)
                {
                    // Apply match bonus with perk modifications (GlassCannon doubles it)
                    boost = PerkModifier.ModifyMatchBonus(perk, item.matchBonus);
                }
                else
                {
                    // Apply base boost with perk modifications (GlassCannon halves it)
                    boost = PerkModifier.ModifyBaseBoost(perk, item.baseBoost);
                }

                itemBoosts += boost;
            }

            // 5. CURSED PENALTY (per-item penalty)
            int cursedPenalty = PerkModifier.GetPerItemPenalty(perk) * validItems.Count;

            // 6. MONSTER PENALTY (M_penalty) - unless Fearless
            int monsterPenalty = PerkModifier.IgnoresMonsterPenalty(perk)
                ? 0
                : monster.difficultyPenalty;

            // FORMULA: P_final = (B + P_hero + Inspiring) + Σ(I_n) + Cursed_penalty - M_penalty
            int finalChance = baseChance + perkModifier + inspiring + itemBoosts + cursedPenalty - monsterPenalty;

            // Apply perk-specific floors (Lucky: 25% minimum)
            finalChance = PerkModifier.ApplySuccessFloor(perk, finalChance);

            // Clamp between 1-99%
            return Mathf.Clamp(finalChance, MIN_CHANCE, MAX_CHANCE);
        }

        /// <summary>
        /// Determine the confidence level based on success percentage
        /// </summary>
        public static ConfidenceLevel GetConfidenceLevel(int successPercentage)
        {
            if (successPercentage >= 80) return ConfidenceLevel.High;
            if (successPercentage >= 40) return ConfidenceLevel.Medium;
            return ConfidenceLevel.Low;
        }

        /// <summary>
        /// Perform the one-shot dice roll
        /// </summary>
        /// <param name="successPercentage">The calculated success percentage</param>
        /// <returns>True if hero succeeds, false otherwise</returns>
        public static bool RollOneShot(int successPercentage)
        {
            int roll = Random.Range(0, 100);
            return roll < successPercentage;
        }

        /// <summary>
        /// Calculate review outcome based on confidence and result
        /// Now includes perk-based reputation modifiers
        /// </summary>
        /// <param name="confidence">The confidence level</param>
        /// <param name="success">Whether the hero succeeded</param>
        /// <param name="perk">The hero's perk (for SocialMediaStar modifier)</param>
        /// <returns>Tuple of (stars, reputationChange)</returns>
        public static (int stars, int reputationChange) CalculateReview(
            ConfidenceLevel confidence,
            bool success,
            Perk perk = Perk.None)
        {
            // Get base review values
            var (stars, baseRepChange) = (confidence, success) switch
            {
                // High confidence outcomes
                (ConfidenceLevel.High, true) => (5, 10),      // Met expectations
                (ConfidenceLevel.High, false) => (1, -25),    // "Scam" penalty

                // Medium confidence outcomes
                (ConfidenceLevel.Medium, true) => (4, 10),    // Good result
                (ConfidenceLevel.Medium, false) => (2, -10),  // Disappointing

                // Low confidence outcomes
                (ConfidenceLevel.Low, true) => (5, 25),       // "Miracle" bonus
                (ConfidenceLevel.Low, false) => (2, -5),      // Expected failure

                _ => (3, 0) // Fallback
            };

            // Apply perk-based reputation modifiers (SocialMediaStar)
            int finalRepChange = PerkModifier.ModifyReputationChange(perk, baseRepChange, success);

            return (stars, finalRepChange);
        }

        /// <summary>
        /// Get the inspiring bonus to pass to the next hero
        /// </summary>
        /// <param name="perk">Current hero's perk</param>
        /// <param name="succeeded">Whether the hero succeeded</param>
        /// <returns>Bonus to apply to next hero (0 if not inspiring or failed)</returns>
        public static int GetInspiringBonusForNextHero(Perk perk, bool succeeded)
        {
            return PerkModifier.GetInspiringBonus(perk, succeeded);
        }
    }
}
