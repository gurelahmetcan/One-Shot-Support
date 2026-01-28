using UnityEngine;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;
using System.Collections.Generic;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Pairs a hero's portrait sprite with their name and voiceline
    /// </summary>
    [System.Serializable]
    public class HeroVisuals
    {
        [Tooltip("Possible names for this hero (one will be randomly selected)")]
        public string[] heroNames;

        [Tooltip("Possible voicelines for this hero (one will be randomly selected)")]
        public AudioClip[] heroVoicelines;

        [Tooltip("Hero portrait sprite")]
        public Sprite portrait;
    }

    /// <summary>
    /// Configuration for procedural hero generation
    /// Generates heroes with random stats, aptitudes, and traits
    /// </summary>
    [CreateAssetMenu(fileName = "HeroGenerator", menuName = "One-Shot Support/Hero Generator")]
    public class HeroGenerator : ScriptableObject
    {
        [Header("Hero Visuals")]
        [Tooltip("Visual pool for heroes (randomly selected)")]
        public HeroVisuals[] visualPool;

        [Header("Trait Pools")]
        [Tooltip("Pool of possible traits heroes can spawn with (20% chance per trait)")]
        public List<HeroTrait> possibleTraits = new List<HeroTrait>();

        [Header("Age Configuration")]
        [Tooltip("Minimum starting age")]
        public int minAge = 20;

        [Tooltip("Maximum starting age")]
        public int maxAge = 30;

        [Header("Contract Configuration")]
        [Tooltip("Minimum contract length in years")]
        public int minContractYears = 1;

        [Tooltip("Maximum contract length in years")]
        public int maxContractYears = 3;

        [Header("Aptitude Configuration")]
        [Tooltip("Minimum aptitude multiplier (0.5 = slow learner)")]
        [Range(0.5f, 1.5f)]
        public float minAptitude = 0.6f;

        [Tooltip("Maximum aptitude multiplier (2.0 = fast learner)")]
        [Range(0.8f, 2.0f)]
        public float maxAptitude = 1.6f;

        /// <summary>
        /// Generate a random hero with random stats, aptitudes, and traits
        /// </summary>
        public HeroData GenerateHero()
        {
            // Create runtime instance
            var hero = ScriptableObject.CreateInstance<HeroData>();

            // Random age
            int age = Random.Range(minAge, maxAge + 1);

            // Random aptitudes (5-stat system + discipline)
            HeroAptitudes aptitudes = new HeroAptitudes(
                Random.Range(minAptitude, maxAptitude), // might
                Random.Range(minAptitude, maxAptitude), // charm
                Random.Range(minAptitude, maxAptitude), // wit
                Random.Range(minAptitude, maxAptitude), // agility
                Random.Range(minAptitude, maxAptitude), // fortitude
                Random.Range(minAptitude, maxAptitude)  // discipline
            );

            // Random contract length
            int contractYears = Random.Range(minContractYears, maxContractYears + 1);

            // Select random name from visual pool
            string heroName = GenerateHeroName();

            // Initialize hero with random stats
            hero.InitializeRandom(heroName, age, aptitudes, contractYears);

            // Assign visuals
            AssignVisuals(hero);

            // Randomly assign traits (20% chance per trait)
            AssignRandomTraits(hero);

            Debug.Log($"[HeroGenerator] Generated hero: {hero.heroName}, Age: {hero.currentAge}, Level: {hero.level}");

            return hero;
        }

        /// <summary>
        /// Generate a hero name from visual pool
        /// </summary>
        private string GenerateHeroName()
        {
            if (visualPool == null || visualPool.Length == 0)
            {
                Debug.LogWarning("[HeroGenerator] Visual pool is empty!");
                return "Unknown Hero";
            }

            // Get random visual set
            HeroVisuals visuals = visualPool[Random.Range(0, visualPool.Length)];

            if (visuals.heroNames != null && visuals.heroNames.Length > 0)
            {
                return visuals.heroNames[Random.Range(0, visuals.heroNames.Length)];
            }

            return "Unnamed Hero";
        }

        /// <summary>
        /// Assign random visuals (portrait + voiceline) from the visual pool
        /// </summary>
        private void AssignVisuals(HeroData hero)
        {
            if (visualPool == null || visualPool.Length == 0)
            {
                Debug.LogWarning("[HeroGenerator] Visual pool is empty or null!");
                return;
            }

            // Get random visual set from pool
            HeroVisuals visuals = visualPool[Random.Range(0, visualPool.Length)];

            // Assign portrait
            hero.portrait = visuals.portrait;

            // Randomly select one voiceline from the available voicelines for this visual
            if (visuals.heroVoicelines != null && visuals.heroVoicelines.Length > 0)
            {
                AudioClip randomVoiceline = visuals.heroVoicelines[Random.Range(0, visuals.heroVoicelines.Length)];
                if (randomVoiceline != null)
                {
                    hero.heroVoiceline = randomVoiceline;
                }
            }
        }

        /// <summary>
        /// Randomly assign exactly 1 trait to the hero
        /// </summary>
        private void AssignRandomTraits(HeroData hero)
        {
            if (possibleTraits == null || possibleTraits.Count == 0)
            {
                return;
            }

            // Pick exactly 1 random trait from the pool
            HeroTrait randomTrait = possibleTraits[Random.Range(0, possibleTraits.Count)];
            if (randomTrait != null)
            {
                hero.AddTrait(randomTrait);
            }
        }

        /// <summary>
        /// Generate a hero with specific parameters (for testing/debugging)
        /// </summary>
        public HeroData GenerateHero(string name, int age, int contractYears)
        {
            var hero = ScriptableObject.CreateInstance<HeroData>();

            // Random aptitudes
            HeroAptitudes aptitudes = new HeroAptitudes(
                Random.Range(minAptitude, maxAptitude),
                Random.Range(minAptitude, maxAptitude),
                Random.Range(minAptitude, maxAptitude),
                Random.Range(minAptitude, maxAptitude),
                Random.Range(minAptitude, maxAptitude),
                Random.Range(minAptitude, maxAptitude)
            );

            hero.InitializeRandom(name, age, aptitudes, contractYears);
            AssignVisuals(hero);
            AssignRandomTraits(hero);

            return hero;
        }
    }
}
