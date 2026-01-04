using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.ScriptableObjects;
using OneShotSupport.Data;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Persistent tooltip panel that displays item information at bottom left
    /// Shows selected item's icon and details
    /// </summary>
    public class ItemTooltip : MonoBehaviour
    {
        [Header("UI References")]
        public Image itemIconImage;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI categoryText;
        public TextMeshProUGUI baseBoostText;
        public TextMeshProUGUI matchBonusText;
        public TextMeshProUGUI descriptionText;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            // Get or add CanvasGroup
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Panel doesn't block raycasts
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            // Start hidden
            Hide();
        }

        /// <summary>
        /// Show tooltip with item data
        /// </summary>
        public void Show(ItemData itemData, ItemCategory? monsterWeakness = null)
        {
            if (itemData == null)
            {
                Hide();
                return;
            }

            gameObject.SetActive(true);

            // Item icon
            if (itemIconImage != null && itemData.icon != null)
                itemIconImage.sprite = itemData.icon;

            // Item name
            if (itemNameText != null)
                itemNameText.text = itemData.itemName;

            // Category
            if (categoryText != null)
                categoryText.text = $"Category: {itemData.category}";

            // Base boost
            if (baseBoostText != null)
                baseBoostText.text = $"Base Boost: +{itemData.baseBoost}%";

            // Match bonus
            if (matchBonusText != null)
            {
                string matchText = $"Match Bonus: +{itemData.matchBonus}%";

                // Highlight if this matches current monster weakness
                if (monsterWeakness.HasValue && itemData.category == monsterWeakness.Value)
                {
                    matchText = $"<color=yellow>{matchText} (MATCH!)</color>";
                }

                matchBonusText.text = matchText;
            }

            // Description
            if (descriptionText != null && !string.IsNullOrEmpty(itemData.description))
                descriptionText.text = itemData.description;
        }

        /// <summary>
        /// Hide the tooltip panel
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
