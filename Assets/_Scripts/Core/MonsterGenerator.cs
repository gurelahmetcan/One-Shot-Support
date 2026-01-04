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
        [Tooltip("Possible names for this monster (one will be randomly selected)")]
        public string[] monsterNames;

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

            // Assign category sprite (with proper validation)
            if (categorySprites != null && categorySprites.Length >= 4)
            {
                monster.categorySprite = GetCategorySprite(monster.weakness);
            }
            else
            {
                Debug.LogWarning("[MonsterGenerator] Cannot assign category sprite - array needs 4 sprites!");
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

            // Assign category sprite (with proper validation)
            if (categorySprites != null && categorySprites.Length >= 4)
            {
                monster.categorySprite = GetCategorySprite(monster.weakness);
            }
            else
            {
                Debug.LogWarning("[MonsterGenerator] Cannot assign category sprite - array needs 4 sprites!");
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
            // Ensure we have enough sprites in the array
            if (categorySprites == null || categorySprites.Length < 4)
            {
                Debug.LogWarning($"[MonsterGenerator] Category sprites array missing or incomplete! Need 4 sprites, got {categorySprites?.Length ?? 0}");
                return null;
            }

            Sprite returnSprite = null;
            int index = -1;

            switch (category)
            {
                case ItemCategory.Hygiene:
                    index = 0;
                    break;
                case ItemCategory.Magic:
                    index = 1;
                    break;
                case ItemCategory.Catering:
                    index = 2;
                    break;
                case ItemCategory.Lighting:
                    index = 3;
                    break;
                default:
                    index = 0;
                    break;
            }

            // Bounds check and null check
            if (index >= 0 && index < categorySprites.Length)
            {
                returnSprite = categorySprites[index];
                if (returnSprite == null)
                {
                    Debug.LogWarning($"[MonsterGenerator] Category sprite for {category} (index {index}) is null!");
                }
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

            // Assign sprite
            monster.sprite = visuals.sprite;

            // Randomly select one name from the available names for this sprite
            if (visuals.monsterNames != null && visuals.monsterNames.Length > 0)
            {
                string randomName = visuals.monsterNames[Random.Range(0, visuals.monsterNames.Length)];
                if (!string.IsNullOrEmpty(randomName))
                {
                    monster.monsterName = randomName;
                }
            }
            else
            {
                Debug.LogWarning("[MonsterGenerator] Monster visual has no names assigned!");
            }
        }
    }
}
