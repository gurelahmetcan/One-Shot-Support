using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.ScriptableObjects;
using OneShotSupport.Data;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Tooltip that displays item information on hover
    /// </summary>
    public class ItemTooltip : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI categoryText;
        public TextMeshProUGUI baseBoostText;
        public TextMeshProUGUI matchBonusText;
        public TextMeshProUGUI descriptionText;

        [Header("Settings")]
        public Vector2 offset = new Vector2(10, 10);

        private RectTransform rectTransform;
        private Canvas canvas;
        private bool isVisible = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            Hide();
        }

        private void Update()
        {
            if (isVisible)
            {
                // Follow mouse cursor
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    Input.mousePosition,
                    canvas.worldCamera,
                    out localPoint
                );

                rectTransform.anchoredPosition = localPoint + offset;
            }
        }

        /// <summary>
        /// Show tooltip with item data
        /// </summary>
        public void Show(ItemData itemData, ItemCategory? monsterWeakness = null)
        {
            if (itemData == null) return;

            gameObject.SetActive(true);
            isVisible = true;

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
        /// Hide the tooltip
        /// </summary>
        public void Hide()
        {
            isVisible = false;
            gameObject.SetActive(false);
        }
    }
}
