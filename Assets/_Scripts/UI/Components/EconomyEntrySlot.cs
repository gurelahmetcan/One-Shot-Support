using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// UI component representing a single economy entry (expense or income)
    /// Displays description and amount with color coding
    /// </summary>
    public class EconomyEntrySlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Image backgroundImage;

        [Header("Colors")]
        [SerializeField] private Color expenseColor = new Color(1f, 0.4f, 0.4f, 0.3f); // Light red
        [SerializeField] private Color incomeColor = new Color(0.4f, 1f, 0.4f, 0.3f);  // Light green

        /// <summary>
        /// Setup the entry slot with description and amount
        /// </summary>
        /// <param name="description">Description of the entry</param>
        /// <param name="amount">Amount (positive for income, negative for expense)</param>
        public void Setup(string description, int amount)
        {
            // Update description
            if (descriptionText != null)
                descriptionText.text = description;

            // Update amount with color coding
            if (amountText != null)
            {
                string sign = amount >= 0 ? "+" : "";
                amountText.text = $"{sign}{amount} Gold";
                amountText.color = amount >= 0 ? Color.green : Color.red;
            }

            // Update background color
            if (backgroundImage != null)
            {
                backgroundImage.color = amount >= 0 ? incomeColor : expenseColor;
            }
        }

        /// <summary>
        /// Clear the slot
        /// </summary>
        public void Clear()
        {
            if (descriptionText != null)
                descriptionText.text = "";

            if (amountText != null)
                amountText.text = "";

            if (backgroundImage != null)
                backgroundImage.color = Color.clear;
        }
    }
}
