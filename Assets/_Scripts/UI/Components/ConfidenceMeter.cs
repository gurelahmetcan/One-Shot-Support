using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.Data;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Confidence meter that displays success chance percentage
    /// Updates in real-time as items are equipped
    /// Can be hidden for "Honest" perk heroes
    /// </summary>
    public class ConfidenceMeter : MonoBehaviour
    {
        [Header("UI References")]
        public Image confidenceFillImage;
        public TextMeshProUGUI percentageText;
        
        private bool isHidden = false;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        /// <summary>
        /// Update the confidence meter with new success chance
        /// </summary>
        public void UpdateConfidence(int successPercentage)
        {
            if (isHidden) return;

            // Clamp value
            successPercentage = Mathf.Clamp(successPercentage, 1, 99);

            // Update slider
            if (confidenceFillImage != null)
            {
                confidenceFillImage.fillAmount = successPercentage;
            }

            // Update percentage text
            if (percentageText != null)
            {
                percentageText.text = $"{successPercentage}%";
            }
        }

        /// <summary>
        /// Hide the confidence meter (for "Honest" perk)
        /// </summary>
        public void Hide()
        {
            isHidden = true;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
            }

            if (percentageText != null)
                percentageText.text = "???";
        }

        /// <summary>
        /// Show the confidence meter
        /// </summary>
        public void Show()
        {
            isHidden = false;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1;
            }
        }

        /// <summary>
        /// Get confidence level from percentage
        /// </summary>
        private ConfidenceLevel GetConfidenceLevel(int percentage)
        {
            if (percentage >= 80) return ConfidenceLevel.High;
            if (percentage >= 40) return ConfidenceLevel.Medium;
            return ConfidenceLevel.Low;
        }

        /// <summary>
        /// Reset the meter to 0
        /// </summary>
        public void Reset()
        {
            UpdateConfidence(0);
        }
    }
}
