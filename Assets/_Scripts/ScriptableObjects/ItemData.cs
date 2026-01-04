using UnityEngine;
using OneShotSupport.Data;

namespace OneShotSupport.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject representing support items (companions, potions, weapons, etc.)
    /// that can be equipped to heroes
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "One-Shot Support/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Item Identity")]
        [Tooltip("Display name of the item")]
        public string itemName;

        [Tooltip("Item description for UI")]
        [TextArea(2, 4)]
        public string description;

        [Header("Stats")]
        [Tooltip("Item category (must match monster weakness for bonus)")]
        public ItemCategory category;

        [Tooltip("Base percentage boost when equipped")]
        [Range(0, 30)]
        public int baseBoost = 15;

        [Tooltip("Bonus percentage when category matches monster weakness")]
        [Range(0, 50)]
        public int matchBonus = 40;

        [Header("Visual")]
        [Tooltip("Icon for UI display")]
        public Sprite icon;

        [Tooltip("Icon for category")] 
        public Sprite categoryIcon;

        /// <summary>
        /// Get the boost value based on whether it matches the monster weakness
        /// </summary>
        public int GetBoost(ItemCategory monsterWeakness)
        {
            return category == monsterWeakness ? matchBonus : baseBoost;
        }
    }
}
