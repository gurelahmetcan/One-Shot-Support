using UnityEngine;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;
using OneShotSupport.Utils;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Configuration for procedural hero generation
    /// </summary>
    [CreateAssetMenu(fileName = "HeroGenerator", menuName = "One-Shot Support/Hero Generator")]
    public class HeroGenerator : ScriptableObject
    {
        [Header("Hero Visuals")]
        [Tooltip("Sprite pool for Noob tier heroes (randomly selected)")]
        public Sprite[] noobSprites;

        [Tooltip("Sprite pool for Knight tier heroes (randomly selected)")]
        public Sprite[] knightSprites;

        [Tooltip("Sprite pool for Legend tier heroes (randomly selected)")]
        public Sprite[] legendSprites;

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
                    hero.portrait = GetRandomSprite(noobSprites);
                    break;

                case HeroTier.Knight:
                    hero.heroName = GenerateHeroName("Knight");
                    hero.baseChance = Random.Range(knightBaseChanceRange.x, knightBaseChanceRange.y + 1);
                    hero.slots = knightSlots;
                    hero.portrait = GetRandomSprite(knightSprites);
                    break;

                case HeroTier.Legend:
                    hero.heroName = GenerateHeroName("Legend");
                    hero.baseChance = Random.Range(legendBaseChanceRange.x, legendBaseChanceRange.y + 1);
                    hero.slots = legendSlots;
                    hero.portrait = GetRandomSprite(legendSprites);
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
        /// Get a random sprite from the provided sprite pool
        /// </summary>
        private Sprite GetRandomSprite(Sprite[] spritePool)
        {
            if (spritePool == null || spritePool.Length == 0)
            {
                Debug.LogWarning("[HeroGenerator] Sprite pool is empty or null!");
                return null;
            }

            return spritePool[Random.Range(0, spritePool.Length)];
        }
    }
}
