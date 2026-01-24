using UnityEngine;

namespace OneShotSupport.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject representing a hero trait (permanent characteristic)
    /// Traits modify base stats, derived values, and mission outcomes
    /// Example traits: Brawny (+Prowess), Frugal (-Salary), Lucky (+Mission Success)
    /// </summary>
    [CreateAssetMenu(fileName = "HeroTrait", menuName = "One-Shot Support/Hero Trait")]
    public class HeroTrait : ScriptableObject
    {
        [Header("Trait Identity")]
        [Tooltip("Display name of the trait")]
        public string traitName;

        [Tooltip("Description of what the trait does")]
        [TextArea(2, 4)]
        public string description;

        [Header("Stat Modifiers (5-Stat System)")]
        [Tooltip("Flat bonus/penalty to Might")]
        public int mightModifier = 0;

        [Tooltip("Flat bonus/penalty to Charm")]
        public int charmModifier = 0;

        [Tooltip("Flat bonus/penalty to Wit")]
        public int witModifier = 0;

        [Tooltip("Flat bonus/penalty to Agility")]
        public int agilityModifier = 0;

        [Tooltip("Flat bonus/penalty to Fortitude")]
        public int fortitudeModifier = 0;

        [Header("Contract Modifiers")]
        [Tooltip("Percentage modifier to daily salary (e.g., -0.1 = 10% cheaper)")]
        [Range(-0.5f, 0.5f)]
        public float salaryModifier = 0f;

        [Tooltip("Percentage modifier to loot cut (e.g., -0.1 = takes 10% less loot)")]
        [Range(-0.5f, 0.5f)]
        public float lootCutModifier = 0f;

        [Header("Mission Modifiers (Future Use)")]
        [Tooltip("Percentage modifier to mission success chance")]
        [Range(-0.3f, 0.3f)]
        public float missionSuccessModifier = 0f;

        [Tooltip("Percentage modifier to damage taken")]
        [Range(-0.5f, 0.5f)]
        public float damageTakenModifier = 0f;

        [Tooltip("Percentage modifier to fame gained from missions")]
        [Range(-0.5f, 1.0f)]
        public float fameGainModifier = 0f;

        /// <summary>
        /// Apply this trait's stat modifiers to the provided stats (5-stat system)
        /// </summary>
        public void ApplyStatModifiers(ref int might, ref int charm, ref int wit, ref int agility, ref int fortitude)
        {
            might += mightModifier;
            charm += charmModifier;
            wit += witModifier;
            agility += agilityModifier;
            fortitude += fortitudeModifier;
        }

        /// <summary>
        /// Calculate modified salary based on trait
        /// </summary>
        public float ApplySalaryModifier(float baseSalary)
        {
            return baseSalary * (1f + salaryModifier);
        }

        /// <summary>
        /// Calculate modified loot cut based on trait
        /// </summary>
        public float ApplyLootCutModifier(float baseLootCut)
        {
            return baseLootCut * (1f + lootCutModifier);
        }

        /// <summary>
        /// Get total mission success modifier from this trait
        /// </summary>
        public float GetMissionSuccessModifier()
        {
            return missionSuccessModifier;
        }
    }
}
