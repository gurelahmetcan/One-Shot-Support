using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Displays the current trust percentage as a bar and text
    /// Trust acts as a multiplier for Fame gains
    /// </summary>
    public class TrustMeter : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Fill image for the trust bar")]
        public Image fillImage;

        [Tooltip("Text display for trust percentage")]
        public TextMeshProUGUI trustText;

        [Header("Text Format")]
        [Tooltip("Format for trust percentage (e.g., 'Trust: {0}%')")]
        public string textFormat = "Trust: {0}%";

        [Header("Trust Colors")]
        [Tooltip("Color for Golden Reputation (80%+)")]
        public Color goldenReputationColor = new Color(1f, 0.84f, 0f); // Gold

        [Tooltip("Color for normal trust (20-79%)")]
        public Color normalColor = new Color(0.5f, 0.7f, 1f); // Light blue

        [Tooltip("Color for Notorious (<20%)")]
        public Color notoriousColor = new Color(1f, 0.3f, 0.3f); // Red

        /// <summary>
        /// Update the trust display
        /// </summary>
        public void UpdateTrust(int trustPercentage)
        {
            // Clamp to 0-100
            trustPercentage = Mathf.Clamp(trustPercentage, 0, 100);

            // Update fill amount
            if (fillImage != null)
            {
                fillImage.fillAmount = trustPercentage / 100f;
                fillImage.color = GetColorForTrust(trustPercentage);
            }

            // Update text
            if (trustText != null)
            {
                trustText.text = string.Format(textFormat, trustPercentage);
                trustText.color = GetColorForTrust(trustPercentage);
            }
        }

        /// <summary>
        /// Get color based on trust level
        /// </summary>
        private Color GetColorForTrust(int trust)
        {
            if (trust >= 80)
                return goldenReputationColor;
            else if (trust < 20)
                return notoriousColor;
            else
                return normalColor;
        }
    }
}
