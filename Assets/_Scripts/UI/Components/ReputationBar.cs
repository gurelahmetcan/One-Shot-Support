using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Displays the player's reputation bar
    /// Persistent across all screens
    /// </summary>
    public class ReputationBar : MonoBehaviour
    {
        [Header("UI References")]
        public Slider reputationSlider;
        public TextMeshProUGUI reputationText;
        public TextMeshProUGUI statusText;
        public Image fillImage;

        [Header("Color Settings")]
        public Color criticalColor = Color.red;
        public Color poorColor = new Color(1f, 0.5f, 0f); // Orange
        public Color averageColor = Color.yellow;
        public Color goodColor = new Color(0.5f, 1f, 0.5f); // Light green
        public Color excellentColor = Color.green;

        private const int MIN_REP = 0;
        private const int MAX_REP = 100;

        private void Awake()
        {
            if (reputationSlider != null)
            {
                reputationSlider.minValue = MIN_REP;
                reputationSlider.maxValue = MAX_REP;
            }
        }

        /// <summary>
        /// Update reputation display
        /// </summary>
        public void UpdateReputation(int reputation)
        {
            reputation = Mathf.Clamp(reputation, MIN_REP, MAX_REP);

            // Update slider
            if (reputationSlider != null)
            {
                reputationSlider.value = reputation;
            }

            // Update text
            if (reputationText != null)
            {
                reputationText.text = $"{reputation}/{MAX_REP}";
            }

            // Update status
            UpdateStatus(reputation);

            // Update color
            UpdateColor(reputation);
        }

        /// <summary>
        /// Update status text based on reputation
        /// </summary>
        private void UpdateStatus(int reputation)
        {
            if (statusText == null) return;

            float normalized = (float)reputation / MAX_REP;

            string status = normalized switch
            {
                >= 0.8f => "Excellent",
                >= 0.6f => "Good",
                >= 0.4f => "Average",
                >= 0.2f => "Poor",
                _ => "Critical"
            };

            statusText.text = status;
        }

        /// <summary>
        /// Update fill color based on reputation
        /// </summary>
        private void UpdateColor(int reputation)
        {
            if (fillImage == null) return;

            float normalized = (float)reputation / MAX_REP;

            Color targetColor = normalized switch
            {
                >= 0.8f => excellentColor,
                >= 0.6f => goodColor,
                >= 0.4f => averageColor,
                >= 0.2f => poorColor,
                _ => criticalColor
            };

            fillImage.color = targetColor;
        }
    }
}
