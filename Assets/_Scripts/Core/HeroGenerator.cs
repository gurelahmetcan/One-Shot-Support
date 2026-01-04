using UnityEngine;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;
using OneShotSupport.Utils;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Pairs a hero's portrait sprite with their character card sprite
    /// </summary>
    [System.Serializable]
    public class HeroVisuals
    {
        [Tooltip("Hero portrait sprite")]
        public Sprite portrait;

        [Tooltip("Character card sprite (must match portrait)")]
        public Sprite characterCard;
    }

    /// <summary>
    /// Configuration for procedural hero generation
    /// </summary>
    [CreateAssetMenu(fileName = "HeroGenerator", menuName = "One-Shot Support/Hero Generator")]
    public class HeroGenerator : ScriptableObject
    {
        [Header("Hero Visuals")]
        [Tooltip("Visual pool for Noob tier heroes (randomly selected)")]
        public HeroVisuals[] noobVisuals;

        [Tooltip("Visual pool for Knight tier heroes (randomly selected)")]
        public HeroVisuals[] knightVisuals;

        [Tooltip("Visual pool for Legend tier heroes (randomly selected)")]
        public HeroVisuals[] legendVisuals;

        [Header("Hero Stats")]
        [Tooltip("Base chance range for Noob tier")]
        public Vector2Int noobBaseChanceRange = new Vector2Int(5, 15);

        [Tooltip("Base chance range for Knight tier")]
        public Vector2Int knightBaseChanceRange = new Vector2Int(15, 25);

        [Tooltip("Base chance range for Legend tier")]
        public Vector2Int legendBaseChanceRange = new Vector2Int(25, 35);

        [Header("Slot Distribution")]
        [Tooltip("Number of slots for each tier")]
        public int noobSlots = 1;
        public int knightSlots = 2;
        public int legendSlots = 3;

        [Header("Perk Probabilities")]
        [Range(0f, 1f)]
        [Tooltip("Chance for common perks")]
        public float commonPerkChance = 0.6f;

        [Range(0f, 1f)]
        [Tooltip("Chance for rare perks")]
        public float rarePerkChance = 0.3f;

        [Range(0f, 1f)]
        [Tooltip("Chance for legendary perks")]
        public float legendaryPerkChance = 0.1f;

        /// <summary>
        /// Generate a random hero
        /// </summary>
        public HeroData GenerateHero()
        {
            // Create runtime instance
            var hero = ScriptableObject.CreateInstance<HeroData>();

            // Random tier
            HeroTier tier = (HeroTier)Random.Range(0, 3);
            hero.tier = tier;

            // Set stats based on tier
            switch (tier)
            {
                case HeroTier.Noob:
                    hero.heroName = GenerateHeroName("Noob");
                    hero.baseChance = Random.Range(noobBaseChanceRange.x, noobBaseChanceRange.y + 1);
                    hero.slots = noobSlots;
                    AssignVisuals(hero, noobVisuals);
                    break;

                case HeroTier.Knight:
                    hero.heroName = GenerateHeroName("Knight");
                    hero.baseChance = Random.Range(knightBaseChanceRange.x, knightBaseChanceRange.y + 1);
                    hero.slots = knightSlots;
                    AssignVisuals(hero, knightVisuals);
                    break;

                case HeroTier.Legend:
                    hero.heroName = GenerateHeroName("Legend");
                    hero.baseChance = Random.Range(legendBaseChanceRange.x, legendBaseChanceRange.y + 1);
                    hero.slots = legendSlots;
                    AssignVisuals(hero, legendVisuals);
                    break;
            }

            // Generate random perk
            hero.perk = GenerateRandomPerk();

            // Description
            hero.description = $"A {tier} tier hero with {PerkModifier.GetDescription(hero.perk)}";

            return hero;
        }

        /// <summary>
        /// Generate a hero name
        /// </summary>
        private string GenerateHeroName(string prefix)
        {
            string[] names = { "Bob", "Alice", "Charlie", "Diana", "Eric", "Fiona", "Greg", "Hannah", "Ivan", "Julia" };
            return $"{names[Random.Range(0, names.Length)]} the {prefix}";
        }

        /// <summary>
        /// Generate a random perk based on rarity probabilities
        /// </summary>
        private Perk GenerateRandomPerk()
        {
            float roll = Random.value;

            if (roll < legendaryPerkChance)
            {
                // Legendary perk
                return Perk.Cursed;
            }
            else if (roll < legendaryPerkChance + rarePerkChance)
            {
                // Rare perk
                Perk[] rarePerks = { Perk.GlassCannon, Perk.SocialMediaStar, Perk.Lucky, Perk.Fearless, Perk.Inspiring };
                return rarePerks[Random.Range(0, rarePerks.Length)];
            }
            else
            {
                // Common perk (or None)
                Perk[] commonPerks = { Perk.None, Perk.Clumsy, Perk.Overconfident, Perk.Prepared, Perk.Honest };
                return commonPerks[Random.Range(0, commonPerks.Length)];
            }
        }

        /// <summary>
        /// Assign random visuals (portrait + character card) from the provided pool
        /// Ensures portrait and card sprites match
        /// </summary>
        private void AssignVisuals(HeroData hero, HeroVisuals[] visualPool)
        {
            if (visualPool == null || visualPool.Length == 0)
            {
                Debug.LogWarning("[HeroGenerator] Visual pool is empty or null!");
                return;
            }

            // Get random visual set from pool
            HeroVisuals visuals = visualPool[Random.Range(0, visualPool.Length)];

            // Assign both portrait and character card
            hero.portrait = visuals.portrait;
            hero.characterCard = visuals.characterCard;
        }
    }
}
