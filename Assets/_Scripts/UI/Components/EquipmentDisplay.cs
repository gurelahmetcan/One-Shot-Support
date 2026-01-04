using UnityEngine;
using UnityEngine.UI;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Read-only display of equipped items on the main panel
    /// Shows the same items that are equipped in the inventory view
    /// </summary>
    public class EquipmentDisplay : MonoBehaviour
    {
        [Header("Display Slots")]
        [Tooltip("Image components for displaying equipped item icons (max 4)")]
        public Image[] displaySlots = new Image[4];

        [Tooltip("GameObjects for each slot (to show/hide based on active slots)")]
        public GameObject[] slotContainers = new GameObject[4];

        [Tooltip("Background image for equipment display area")]
        public Image backgroundImage;

        [Tooltip("Background sprites for different slot counts [1-slot, 2-slot, 3-slot, 4-slot]")]
        public Sprite[] backgroundSprites = new Sprite[4];

        /// <summary>
        /// Update the display to show currently equipped items
        /// </summary>
        /// <param name="equippedItems">Array of equipped ItemData (null if slot is empty)</param>
        /// <param name="activeSlotCount">Number of active equipment slots for current hero</param>
        public void UpdateDisplay(ItemData[] equippedItems, int activeSlotCount)
        {
            // Clamp active slots to valid range (1-4)
            activeSlotCount = Mathf.Clamp(activeSlotCount, 1, 4);

            // Update background sprite based on slot count
            if (backgroundImage != null && backgroundSprites != null)
            {
                int spriteIndex = activeSlotCount - 1; // Convert to 0-based index
                if (spriteIndex >= 0 && spriteIndex < backgroundSprites.Length)
                {
                    Sprite bgSprite = backgroundSprites[spriteIndex];
                    if (bgSprite != null)
                    {
                        backgroundImage.sprite = bgSprite;
                    }
                }
            }

            // Update each slot
            for (int i = 0; i < displaySlots.Length; i++)
            {
                bool isActive = i < activeSlotCount;

                // Show/hide slot container
                if (slotContainers[i] != null)
                {
                    slotContainers[i].SetActive(isActive);
                }

                // Update slot image if active
                if (isActive && displaySlots[i] != null)
                {
                    if (equippedItems != null && i < equippedItems.Length && equippedItems[i] != null)
                    {
                        // Show equipped item icon
                        displaySlots[i].sprite = equippedItems[i].icon;
                        displaySlots[i].enabled = true;
                    }
                    else
                    {
                        // Empty slot - hide icon
                        displaySlots[i].sprite = null;
                        displaySlots[i].enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Clear the display (show empty slots)
        /// </summary>
        public void Clear()
        {
            UpdateDisplay(null, 1);
        }
    }
}
