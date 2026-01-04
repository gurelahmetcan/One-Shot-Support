using UnityEngine;
using TMPro;
using OneShotSupport.Core;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// UI component that displays the player's current gold
    /// Updates automatically when gold changes
    /// </summary>
    public class GoldDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("Display Format")]
        [SerializeField] private string prefix = "Gold: ";
        [SerializeField] private string suffix = "g";

        private GoldManager goldManager;

        private void Start()
        {
            // Find GoldManager
            goldManager = FindObjectOfType<GoldManager>();

            if (goldManager != null)
            {
                // Subscribe to gold changes
                goldManager.OnGoldChanged += UpdateDisplay;

                // Initial display
                UpdateDisplay(goldManager.CurrentGold);
            }
            else
            {
                Debug.LogWarning("[GoldDisplay] GoldManager not found!");
                UpdateDisplay(0);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (goldManager != null)
            {
                goldManager.OnGoldChanged -= UpdateDisplay;
            }
        }

        /// <summary>
        /// Update the gold display text
        /// </summary>
        private void UpdateDisplay(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"{prefix}{gold}{suffix}";
            }
        }

        /// <summary>
        /// Manually refresh the display (useful for debugging)
        /// </summary>
        public void Refresh()
        {
            if (goldManager != null)
            {
                UpdateDisplay(goldManager.CurrentGold);
            }
        }
    }
}
