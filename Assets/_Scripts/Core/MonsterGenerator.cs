using UnityEngine;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Configuration for procedural monster generation
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterGenerator", menuName = "One-Shot Support/Monster Generator")]
    public class MonsterGenerator : ScriptableObject
    {
        [Header("Monster Names")]
        [Tooltip("Pool of monster names")]
        public string[] monsterNames = {
            "Slime", "Goblin", "Dragon", "Troll", "Wraith",
            "Demon", "Hydra", "Basilisk", "Chimera", "Golem"
        };

        [Header("Monster Sprites")]
        [Tooltip("Pool of monster sprites (optional, can be null)")]
        public Sprite[] monsterSprites;

        [Header("Difficulty Settings")]
        [Tooltip("Easy difficulty penalty")]
        public int easyPenalty = 0;

        [Tooltip("Medium difficulty penalty")]
        public int mediumPenalty = 15;

        [Tooltip("Hard difficulty penalty")]
        public int hardPenalty = 30;

        [Header("Difficulty Distribution")]
        [Range(0f, 1f)]
        [Tooltip("Chance for easy monster")]
        public float easyChance = 0.3f;

        [Range(0f, 1f)]
        [Tooltip("Chance for medium monster")]
        public float mediumChance = 0.5f;

        [Range(0f, 1f)]
        [Tooltip("Chance for hard monster")]
        public float hardChance = 0.2f;

        /// <summary>
        /// Generate a random monster
        /// </summary>
        public MonsterData GenerateMonster()
        {
            // Create runtime instance
            var monster = ScriptableObject.CreateInstance<MonsterData>();

            // Random name
            monster.monsterName = monsterNames[Random.Range(0, monsterNames.Length)];

            // Random weakness
            monster.weakness = (ItemCategory)Random.Range(0, 4);

            // Random difficulty
            int difficulty = GetRandomDifficulty();
            monster.difficultyPenalty = difficulty switch
            {
                0 => easyPenalty,
                1 => mediumPenalty,
                2 => hardPenalty,
                _ => mediumPenalty
            };

            // Random sprite (if available)
            if (monsterSprites != null && monsterSprites.Length > 0)
            {
                monster.sprite = monsterSprites[Random.Range(0, monsterSprites.Length)];
            }

            // Description
            string difficultyName = difficulty switch
            {
                0 => "Easy",
                1 => "Medium",
                2 => "Hard",
                _ => "Medium"
            };
            monster.description = $"A {difficultyName} monster weak to {monster.weakness} attacks";

            return monster;
        }

        /// <summary>
        /// Get random difficulty based on probabilities
        /// </summary>
        private int GetRandomDifficulty()
        {
            float roll = Random.value;
            float normalizedTotal = easyChance + mediumChance + hardChance;

            float normalizedEasy = easyChance / normalizedTotal;
            float normalizedMedium = mediumChance / normalizedTotal;

            if (roll < normalizedEasy)
                return 0; // Easy
            else if (roll < normalizedEasy + normalizedMedium)
                return 1; // Medium
            else
                return 2; // Hard
        }
    }
}
