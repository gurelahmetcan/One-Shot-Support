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
        public Slider confidenceSlider;
        public TextMeshProUGUI percentageText;
        public TextMeshProUGUI confidenceLevelText;
        public Image fillImage;

        [Header("Color Settings")]
        public Color lowConfidenceColor = Color.red;
        public Color mediumConfidenceColor = Color.yellow;
        public Color highConfidenceColor = Color.green;

        private bool isHidden = false;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (confidenceSlider != null)
            {
                confidenceSlider.minValue = 0;
                confidenceSlider.maxValue = 100;
            }
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
            if (confidenceSlider != null)
            {
                confidenceSlider.value = successPercentage;
            }

            // Update percentage text
            if (percentageText != null)
            {
                percentageText.text = $"{successPercentage}%";
            }

            // Update confidence level text and color
            ConfidenceLevel level = GetConfidenceLevel(successPercentage);
            UpdateConfidenceLevel(level);

            // Update fill color
            UpdateFillColor(level);
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

            if (confidenceLevelText != null)
                confidenceLevelText.text = "Unknown";
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
        /// Update confidence level text
        /// </summary>
        private void UpdateConfidenceLevel(ConfidenceLevel level)
        {
            if (confidenceLevelText == null) return;

            string levelText = level switch
            {
                ConfidenceLevel.High => "High Confidence",
                ConfidenceLevel.Medium => "Medium Confidence",
                ConfidenceLevel.Low => "Low Confidence",
                _ => "Unknown"
            };

            confidenceLevelText.text = levelText;
        }

        /// <summary>
        /// Update fill color based on confidence level
        /// </summary>
        private void UpdateFillColor(ConfidenceLevel level)
        {
            if (fillImage == null) return;

            Color targetColor = level switch
            {
                ConfidenceLevel.High => highConfidenceColor,
                ConfidenceLevel.Medium => mediumConfidenceColor,
                ConfidenceLevel.Low => lowConfidenceColor,
                _ => Color.white
            };

            fillImage.color = targetColor;
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
