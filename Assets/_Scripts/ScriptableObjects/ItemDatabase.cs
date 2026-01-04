using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OneShotSupport.Data;

namespace OneShotSupport.ScriptableObjects
{
    /// <summary>
    /// Database holding all available items in the game
    /// Referenced by GameManager for daily restocking
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "One-Shot Support/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [Header("Item Pool")]
        [Tooltip("All items that can appear in the daily restock")]
        public List<ItemData> allItems = new List<ItemData>();

        /// <summary>
        /// Get a random selection of items for daily restock
        /// </summary>
        /// <param name="count">Number of items to select</param>
        /// <returns>List of random items</returns>
        public List<ItemData> GetRandomItems(int count)
        {
            if (allItems == null || allItems.Count == 0)
            {
                Debug.LogWarning("ItemDatabase is empty!");
                return new List<ItemData>();
            }

            var selectedItems = new List<ItemData>();
            var availableItems = new List<ItemData>(allItems);

            // Select random items without replacement
            for (int i = 0; i < count && availableItems.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, availableItems.Count);
                selectedItems.Add(availableItems[randomIndex]);
                availableItems.RemoveAt(randomIndex);
            }

            return selectedItems;
        }

        /// <summary>
        /// Get a single random item from the database
        /// </summary>
        public ItemData GetRandomItem()
        {
            if (allItems == null || allItems.Count == 0)
            {
                Debug.LogWarning("ItemDatabase is empty!");
                return null;
            }

            return allItems[Random.Range(0, allItems.Count)];
        }

        /// <summary>
        /// Get a random item of a specific category
        /// </summary>
        public ItemData GetRandomItemOfCategory(ItemCategory category)
        {
            if (allItems == null || allItems.Count == 0)
            {
                Debug.LogWarning("ItemDatabase is empty!");
                return null;
            }

            // Filter items by category
            var categoryItems = allItems.Where(item => item.category == category).ToList();

            if (categoryItems.Count == 0)
            {
                Debug.LogWarning($"No items found for category {category}. Returning random item instead.");
                return GetRandomItem();
            }

            return categoryItems[Random.Range(0, categoryItems.Count)];
        }

        /// <summary>
        /// Validate the database
        /// </summary>
        private void OnValidate()
        {
            if (allItems.Count < 6)
            {
                Debug.LogWarning($"ItemDatabase should have at least 6 items for variety. Current count: {allItems.Count}");
            }
        }
    }
}
