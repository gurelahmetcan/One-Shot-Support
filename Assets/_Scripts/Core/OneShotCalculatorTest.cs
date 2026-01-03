using UnityEngine;
using System.Collections.Generic;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;
using OneShotSupport.Utils;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Test script to verify OneShotCalculator logic with full perk support
    /// Attach to a GameObject and check Console for results
    /// </summary>
    public class OneShotCalculatorTest : MonoBehaviour
    {
        [Header("Test Data - Assign ScriptableObjects")]
        public HeroData testHero;
        public MonsterData testMonster;
        public List<ItemData> testItems = new List<ItemData>();

        [Header("Advanced Testing")]
        [Tooltip("Inspiring bonus from previous hero (for testing Inspiring perk)")]
        public int inspiringBonus = 0;

        [ContextMenu("Run Calculation Test")]
        public void RunTest()
        {
            if (testHero == null || testMonster == null)
            {
                Debug.LogWarning("Assign test hero and monster in inspector!");
                return;
            }

            Debug.Log("=== ONE-SHOT CALCULATION TEST ===");

            // Hero info
            Debug.Log($"\n<b>HERO:</b> {testHero.heroName}");
            Debug.Log($"  Base Chance: {testHero.baseChance}%");
            Debug.Log($"  Perk: {testHero.perk} ({PerkModifier.GetRarity(testHero.perk)})");
            Debug.Log($"  {PerkModifier.GetDescription(testHero.perk)}");
            Debug.Log($"  Slots: {testHero.slots} → Effective: {testHero.GetEffectiveSlots()}");

            // Monster info
            Debug.Log($"\n<b>MONSTER:</b> {testMonster.monsterName}");
            Debug.Log($"  Weakness: {testMonster.weakness}");
            Debug.Log($"  Difficulty Penalty: -{testMonster.difficultyPenalty}%");
            if (PerkModifier.IgnoresMonsterPenalty(testHero.perk))
                Debug.Log($"  <color=green>PENALTY IGNORED by {testHero.perk}!</color>");

            // Items info
            Debug.Log($"\n<b>EQUIPPED ITEMS:</b> ({testItems.Count})");
            foreach (var item in testItems)
            {
                if (item != null)
                {
                    bool isMatch = item.category == testMonster.weakness;
                    int boost = item.GetBoost(testMonster.weakness);

                    // Show perk modifications for GlassCannon
                    string modInfo = "";
                    if (testHero.perk == Perk.GlassCannon)
                    {
                        modInfo = isMatch ? " (x2 by GlassCannon!)" : " (÷2 by GlassCannon)";
                    }

                    Debug.Log($"  - {item.itemName} ({item.category}): +{boost}%{(isMatch ? " <color=yellow>MATCH!</color>" : "")}{modInfo}");
                }
            }

            // Calculate success chance
            int successChance = OneShotCalculator.CalculateSuccessChance(
                testHero,
                testMonster,
                testItems,
                inspiringBonus
            );

            var confidence = OneShotCalculator.GetConfidenceLevel(successChance);

            Debug.Log($"\n<b>CALCULATION:</b>");
            Debug.Log($"  Final Success Chance: <b>{successChance}%</b>");
            Debug.Log($"  Confidence Level: {confidence}");
            if (PerkModifier.HidesConfidenceMeter(testHero.perk))
                Debug.Log($"  <color=orange>Confidence meter hidden for this hero!</color>");

            // Simulate a roll
            bool success = OneShotCalculator.RollOneShot(successChance);
            var (stars, repChange) = OneShotCalculator.CalculateReview(confidence, success, testHero.perk);

            Debug.Log($"\n<b>SIMULATED RESULT:</b> {(success ? "<color=green>SUCCESS! ✓</color>" : "<color=red>FAILED ✗</color>")}");
            Debug.Log($"  Review: {stars} stars");
            Debug.Log($"  Reputation Change: {(repChange > 0 ? "<color=green>+" : "<color=red>")}{repChange}</color>");

            // Check for inspiring bonus
            int nextHeroBonus = OneShotCalculator.GetInspiringBonusForNextHero(testHero.perk, success);
            if (nextHeroBonus > 0)
                Debug.Log($"  <color=cyan>Next hero gets +{nextHeroBonus}% from Inspiring!</color>");

            Debug.Log("================================\n");
        }

        private void Start()
        {
            // Auto-run test if data is assigned
            if (testHero != null && testMonster != null)
            {
                RunTest();
            }
        }
    }
}
