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
    /// Implements the formula: P_final = (B + P_hero) + Σ(I_n) - M_penalty
    /// </summary>
    public static class OneShotCalculator
    {
        private const int MIN_CHANCE = 1;
        private const int MAX_CHANCE = 99;

        /// <summary>
        /// Calculate the final success percentage for a hero against a monster
        /// </summary>
        /// <param name="hero">The hero data</param>
        /// <param name="monster">The monster data</param>
        /// <param name="equippedItems">List of items equipped to the hero</param>
        /// <returns>Final success percentage (clamped 1-99)</returns>
        public static int CalculateSuccessChance(
            HeroData hero,
            MonsterData monster,
            List<ItemData> equippedItems)
        {
            // B: Hero Base Chance
            int baseChance = hero.baseChance;

            // P_hero: Perk Modifier
            int perkModifier = PerkModifier.GetModifier(hero.perk);

            // Σ(I_n): Sum of equipped item boosts
            int itemBoosts = equippedItems
                .Where(item => item != null)
                .Sum(item => item.GetBoost(monster.weakness));

            // M_penalty: Monster Difficulty
            int monsterPenalty = monster.difficultyPenalty;

            // P_final = (B + P_hero) + Σ(I_n) - M_penalty
            int finalChance = baseChance + perkModifier + itemBoosts - monsterPenalty;

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
        /// </summary>
        public static (int stars, int reputationChange) CalculateReview(
            ConfidenceLevel confidence,
            bool success)
        {
            return (confidence, success) switch
            {
                // High confidence outcomes
                (ConfidenceLevel.High, true) => (5, 10),      // Met expectations
                (ConfidenceLevel.High, false) => (1, -25),    // "Scam" penalty

                // Medium confidence outcomes
                (ConfidenceLevel.Medium, true) => (4, 10),    // Good result
                (ConfidenceLevel.Medium, false) => (2, -10),  // Disappointing

                // Low confidence outcomes
                (ConfidenceLevel.Low, true) => (5, 25),       // "Miracle" bonus
                (ConfidenceLevel.Low, false) => (3, -2),      // Expected failure

                _ => (3, 0) // Fallback
            };
        }
    }
}
