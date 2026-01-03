using UnityEngine;
using System.Collections.Generic;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Test script to verify OneShotCalculator logic
    /// Attach to a GameObject and check Console for results
    /// </summary>
    public class OneShotCalculatorTest : MonoBehaviour
    {
        [Header("Test Data - Assign ScriptableObjects")]
        public HeroData testHero;
        public MonsterData testMonster;
        public List<ItemData> testItems = new List<ItemData>();

        [Header("Manual Test")]
        [ContextMenu("Run Calculation Test")]
        public void RunTest()
        {
            if (testHero == null || testMonster == null)
            {
                Debug.LogWarning("Assign test hero and monster in inspector!");
                return;
            }

            // Calculate success chance
            int successChance = OneShotCalculator.CalculateSuccessChance(
                testHero,
                testMonster,
                testItems
            );

            var confidence = OneShotCalculator.GetConfidenceLevel(successChance);

            Debug.Log("=== ONE-SHOT CALCULATION TEST ===");
            Debug.Log($"Hero: {testHero.heroName} (Base: {testHero.baseChance}%, Perk: {testHero.perk})");
            Debug.Log($"Monster: {testMonster.monsterName} (Weakness: {testMonster.weakness}, Penalty: -{testMonster.difficultyPenalty}%)");
            Debug.Log($"Items Equipped: {testItems.Count}");

            foreach (var item in testItems)
            {
                if (item != null)
                {
                    int boost = item.GetBoost(testMonster.weakness);
                    bool isMatch = item.category == testMonster.weakness;
                    Debug.Log($"  - {item.itemName}: +{boost}% {(isMatch ? "(MATCH!)" : "")}");
                }
            }

            Debug.Log($"\nFINAL SUCCESS CHANCE: {successChance}%");
            Debug.Log($"Confidence Level: {confidence}");

            // Simulate a roll
            bool success = OneShotCalculator.RollOneShot(successChance);
            var (stars, repChange) = OneShotCalculator.CalculateReview(confidence, success);

            Debug.Log($"\nSIMULATED RESULT: {(success ? "SUCCESS! ✓" : "FAILED ✗")}");
            Debug.Log($"Review: {stars} stars, Reputation {(repChange > 0 ? "+" : "")}{repChange}");
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
