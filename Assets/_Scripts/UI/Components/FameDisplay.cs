using UnityEngine;
using TMPro;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Displays the current fame count
    /// Persistent across all screens
    /// </summary>
    public class FameDisplay : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI fameText;

        [Header("Text Format")]
        [Tooltip("Format for fame display (e.g., 'Fame: {0}')")]
        public string textFormat = "Fame: {0}";

        [Header("Milestone Colors")]
        [Tooltip("Color when no milestones reached")]
        public Color normalColor = Color.white;

        [Tooltip("Color when Market Influence reached (1000+)")]
        public Color marketInfluenceColor = new Color(0.8f, 0.8f, 0.8f); // Silver

        [Tooltip("Color when Prestigious Name reached (2500+)")]
        public Color prestigiousNameColor = new Color(1f, 0.84f, 0f); // Gold

        [Tooltip("Color when Chartered Guild reached (5000+)")]
        public Color charteredGuildColor = new Color(0.58f, 0f, 0.83f); // Purple

        /// <summary>
        /// Update the fame display
        /// </summary>
        public void UpdateFame(int fameAmount)
        {
            if (fameText != null)
            {
                fameText.text = string.Format(textFormat, fameAmount);

                // Update color based on milestones
                UpdateColorForFame(fameAmount);
            }
        }

        /// <summary>
        /// Update text color based on fame milestones
        /// </summary>
        private void UpdateColorForFame(int fame)
        {
            if (fameText == null) return;

            if (fame >= 5000)
                fameText.color = charteredGuildColor;
            else if (fame >= 2500)
                fameText.color = prestigiousNameColor;
            else if (fame >= 1000)
                fameText.color = marketInfluenceColor;
            else
                fameText.color = normalColor;
        }
    }
}
