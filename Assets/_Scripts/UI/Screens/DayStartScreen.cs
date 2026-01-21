using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using OneShotSupport.Tutorial;
using OneShotSupport.Data;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Season start screen that shows season/year and seasonal hint
    /// Displayed before restock phase
    /// </summary>
    public class DayStartScreen : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI dayNumberText;
        [SerializeField] private TextMeshProUGUI hintMessageText;
        [SerializeField] private Button continueButton;

        // Events
        public event Action OnContinueClicked;

        private void Awake()
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(HandleContinue);
        }

        /// <summary>
        /// Setup and show the season start screen
        /// </summary>
        public void Setup(Season season, int year)
        {
            // Update season and year
            if (dayNumberText != null)
                dayNumberText.text = $"{season}, Year {year}";

            // Hide or remove hint message text since we're not using hints
            if (hintMessageText != null)
                hintMessageText.gameObject.SetActive(false);

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Handle continue button click
        /// </summary>
        private void HandleContinue()
        {
            OnContinueClicked?.Invoke();
            Hide();
        }

        /// <summary>
        /// Hide the screen
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
