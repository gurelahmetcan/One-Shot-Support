using System;
using UnityEngine;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;
using Random = UnityEngine.Random;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Pairs a monster's sprite with their name
    /// </summary>
    [System.Serializable]
    public class MonsterVisuals
    {
        [Tooltip("Monster name (e.g., 'Slime King')")]
        public string monsterName;

        [Tooltip("Monster sprite")]
        public Sprite sprite;
    }

    /// <summary>
    /// Configuration for procedural monster generation
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterGenerator", menuName = "One-Shot Support/Monster Generator")]
    public class MonsterGenerator : ScriptableObject
    {
        [Header("Monster Visuals")]
        [Tooltip("Visual pool for monsters (sprite + name pairs, randomly selected)")]
        public MonsterVisuals[] monsterVisuals;

        [Header("Category Sprites")] 
        [Tooltip("Pool of category sprites")]
        public Sprite[] categorySprites;

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

            // Assign random visuals (sprite + name match)
            AssignVisuals(monster);

            // Random weakness
            monster.weakness = (ItemCategory)Random.Range(0, 4);

            // Random rank (D to S)
            monster.rank = (MonsterRank)Random.Range(0, 5);

            // Random difficulty
            int difficulty = GetRandomDifficulty();
            monster.difficultyPenalty = difficulty switch
            {
                0 => easyPenalty,
                1 => mediumPenalty,
                2 => hardPenalty,
                _ => mediumPenalty
            };

            if (categorySprites != null && categorySprites.Length > 0)
            {
                monster.categorySprite = GetCategorySprite(monster.weakness);
            }

            // Description
            string difficultyName = difficulty switch
            {
                0 => "Easy",
                1 => "Medium",
                2 => "Hard",
                _ => "Medium"
            };
            monster.description = $"Rank {monster.rank} - A {difficultyName} monster weak to {monster.weakness} attacks";

            return monster;
        }

        /// <summary>
        /// Generate a monster with a specific weakness
        /// Used for daily hints - guarantees one monster matches the hinted weakness
        /// </summary>
        public MonsterData GenerateMonsterWithWeakness(ItemCategory specificWeakness)
        {
            // Create runtime instance
            var monster = ScriptableObject.CreateInstance<MonsterData>();

            // Assign random visuals (sprite + name match)
            AssignVisuals(monster);

            // Specific weakness (from hint)
            monster.weakness = specificWeakness;

            // Random rank (D to S)
            monster.rank = (MonsterRank)Random.Range(0, 5);

            // Random difficulty
            int difficulty = GetRandomDifficulty();
            monster.difficultyPenalty = difficulty switch
            {
                0 => easyPenalty,
                1 => mediumPenalty,
                2 => hardPenalty,
                _ => mediumPenalty
            };

            if (categorySprites != null && categorySprites.Length > 0)
            {
                monster.categorySprite = GetCategorySprite(monster.weakness);
            }

            // Description
            string difficultyName = difficulty switch
            {
                0 => "Easy",
                1 => "Medium",
                2 => "Hard",
                _ => "Medium"
            };
            monster.description = $"Rank {monster.rank} - A {difficultyName} monster weak to {monster.weakness} attacks";

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

        private Sprite GetCategorySprite(ItemCategory category)
        {
            Sprite returnSprite;
            switch (category)
            {
                case ItemCategory.Hygiene:
                    returnSprite = categorySprites[0];
                    break;
                case ItemCategory.Magic:
                    returnSprite = categorySprites[1];
                    break;
                case ItemCategory.Catering:
                    returnSprite = categorySprites[2];
                    break;
                case ItemCategory.Lighting:
                    returnSprite = categorySprites[3];
                    break;
                default:
                    returnSprite = categorySprites[0];
                    break;
            }
            return returnSprite;
        }

        /// <summary>
        /// Assign random visuals (sprite + name) from the pool
        /// Ensures sprite and name always match
        /// </summary>
        private void AssignVisuals(MonsterData monster)
        {
            if (monsterVisuals == null || monsterVisuals.Length == 0)
            {
                Debug.LogWarning("[MonsterGenerator] Visual pool is empty or null!");
                return;
            }

            // Get random visual set from pool
            MonsterVisuals visuals = monsterVisuals[Random.Range(0, monsterVisuals.Length)];

            // Assign name and sprite (both match the same monster)
            if (!string.IsNullOrEmpty(visuals.monsterName))
            {
                monster.monsterName = visuals.monsterName;
            }
            monster.sprite = visuals.sprite;
        }
    }
}
