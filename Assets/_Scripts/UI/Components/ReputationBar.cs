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
        public Image fillImage;
        
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
        }
    }
}
