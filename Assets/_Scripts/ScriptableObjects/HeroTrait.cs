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

        [Header("Stat Modifiers")]
        [Tooltip("Flat bonus/penalty to Prowess")]
        public int prowessModifier = 0;

        [Tooltip("Flat bonus/penalty to Charisma")]
        public int charismaModifier = 0;

        [Tooltip("Flat bonus/penalty to Max Vitality")]
        public int vitalityModifier = 0;

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
        /// Apply this trait's stat modifiers to the provided stats
        /// </summary>
        public void ApplyStatModifiers(ref int prowess, ref int charisma, ref int maxVitality)
        {
            prowess += prowessModifier;
            charisma += charismaModifier;
            maxVitality += vitalityModifier;
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
