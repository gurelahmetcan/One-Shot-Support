using UnityEngine;
using System;
using OneShotSupport.Data;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Manages hero aging and lifecycle stage transitions
    /// Configurable age ranges allow easy tuning during playtesting
    /// </summary>
    [CreateAssetMenu(fileName = "HeroLifecycleManager", menuName = "One-Shot Support/Hero Lifecycle Manager")]
    public class HeroLifecycleManager : ScriptableObject
    {
        [Header("Lifecycle Age Ranges (Configurable)")]
        [Tooltip("Minimum age for Rookie stage")]
        public int rookieMinAge = 20;

        [Tooltip("Maximum age for Rookie stage")]
        public int rookieMaxAge = 25;

        [Tooltip("Minimum age for Prime stage")]
        public int primeMinAge = 26;

        [Tooltip("Maximum age for Prime stage")]
        public int primeMaxAge = 35;

        [Tooltip("Minimum age for Veteran stage")]
        public int veteranMinAge = 36;

        [Tooltip("Maximum age for Veteran stage")]
        public int veteranMaxAge = 45;

        [Tooltip("Age at which heroes retire")]
        public int retirementAge = 46;

        [Header("Aging Configuration")]
        [Tooltip("How many years heroes age per turn (default: 0.25 = 1 year per 4 turns)")]
        [Range(0.1f, 1f)]
        public float yearsPerTurn = 0.25f;

        /// <summary>
        /// Determine lifecycle stage based on age
        /// </summary>
        public HeroLifecycleStage GetLifecycleStage(int age)
        {
            if (age >= retirementAge)
                return HeroLifecycleStage.Retired;
            else if (age >= veteranMinAge && age <= veteranMaxAge)
                return HeroLifecycleStage.Veteran;
            else if (age >= primeMinAge && age <= primeMaxAge)
                return HeroLifecycleStage.Prime;
            else
                return HeroLifecycleStage.Rookie;
        }

        /// <summary>
        /// Age a hero by one turn and return their new lifecycle stage
        /// </summary>
        public HeroLifecycleStage AgeHero(ref float currentAge)
        {
            currentAge += yearsPerTurn;
            return GetLifecycleStage(Mathf.FloorToInt(currentAge));
        }

        /// <summary>
        /// Get a random starting age for a new recruit in the Rookie stage
        /// </summary>
        public int GetRandomRookieAge()
        {
            return UnityEngine.Random.Range(rookieMinAge, rookieMaxAge + 1);
        }

        /// <summary>
        /// Get a random starting age for any stage (useful for testing)
        /// </summary>
        public int GetRandomAgeForStage(HeroLifecycleStage stage)
        {
            return stage switch
            {
                HeroLifecycleStage.Rookie => UnityEngine.Random.Range(rookieMinAge, rookieMaxAge + 1),
                HeroLifecycleStage.Prime => UnityEngine.Random.Range(primeMinAge, primeMaxAge + 1),
                HeroLifecycleStage.Veteran => UnityEngine.Random.Range(veteranMinAge, veteranMaxAge + 1),
                HeroLifecycleStage.Retired => retirementAge,
                _ => rookieMinAge
            };
        }

        /// <summary>
        /// Get display string for lifecycle stage
        /// </summary>
        public string GetStageDisplayName(HeroLifecycleStage stage)
        {
            return stage switch
            {
                HeroLifecycleStage.Rookie => "Rookie",
                HeroLifecycleStage.Prime => "Prime",
                HeroLifecycleStage.Veteran => "Veteran",
                HeroLifecycleStage.Retired => "Retired",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Get color for lifecycle stage (useful for UI)
        /// </summary>
        public Color GetStageColor(HeroLifecycleStage stage)
        {
            return stage switch
            {
                HeroLifecycleStage.Rookie => new Color(0.3f, 0.8f, 0.3f), // Green
                HeroLifecycleStage.Prime => new Color(0.9f, 0.7f, 0.2f),  // Gold
                HeroLifecycleStage.Veteran => new Color(0.7f, 0.3f, 0.9f), // Purple
                HeroLifecycleStage.Retired => new Color(0.5f, 0.5f, 0.5f), // Gray
                _ => Color.white
            };
        }

        /// <summary>
        /// Check if a hero is about to retire (within 1 year)
        /// </summary>
        public bool IsNearRetirement(int age)
        {
            return age >= retirementAge - 1;
        }

        /// <summary>
        /// Validate age range configuration (called in Unity Editor)
        /// </summary>
        private void OnValidate()
        {
            // Ensure age ranges don't overlap
            if (primeMinAge <= rookieMaxAge)
                primeMinAge = rookieMaxAge + 1;

            if (veteranMinAge <= primeMaxAge)
                veteranMinAge = primeMaxAge + 1;

            if (retirementAge <= veteranMaxAge)
                retirementAge = veteranMaxAge + 1;
        }
    }
}
