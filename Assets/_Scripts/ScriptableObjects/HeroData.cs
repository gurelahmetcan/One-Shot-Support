using UnityEngine;
using OneShotSupport.Data;
using OneShotSupport.Utils;

namespace OneShotSupport.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject representing hero "customers" who need support items
    /// </summary>
    [CreateAssetMenu(fileName = "New Hero", menuName = "One-Shot Support/Hero")]
    public class HeroData : ScriptableObject
    {
        [Header("Hero Identity")]
        [Tooltip("Display name or ID of the hero")]
        public string heroName;

        [Tooltip("Hero description for UI")]
        [TextArea(2, 4)]
        public string description;

        [Header("Stats")]
        [Tooltip("Hero tier (affects base chance)")]
        public HeroTier tier;

        [Tooltip("Base success percentage before modifiers")]
        [Range(0, 50)]
        public int baseChance = 10;

        [Tooltip("Number of item slots (1-3)")]
        [Range(1, 3)]
        public int slots = 2;

        [Tooltip("Hero's perk (personality trait)")]
        public Perk perk = Perk.None;

        [Header("Visual")]
        [Tooltip("Hero portrait for UI display")]
        public Sprite portrait;

        [Tooltip("Character card sprite for hero card UI")]
        public Sprite characterCard;

        [Header("Audio")]
        [Tooltip("Hero voiceline (plays when entering store)")]
        public AudioClip heroVoiceline;

        /// <summary>
        /// Get effective number of item slots after perk modifiers
        /// Overconfident: -1, Prepared: +1
        /// </summary>
        public int GetEffectiveSlots()
        {
            int effectiveSlots = slots + PerkModifier.GetSlotModifier(perk);
            return Mathf.Max(1, effectiveSlots); // Minimum 1 slot
        }
    }
}
