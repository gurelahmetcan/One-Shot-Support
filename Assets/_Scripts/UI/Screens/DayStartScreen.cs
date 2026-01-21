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
        /// Setup and show the day start screen (backward compatibility)
        /// </summary>
        public void Setup(int dayNumber, string hintMessage)
        {
            // Update day number
            if (dayNumberText != null)
                dayNumberText.text = $"{dayNumber}";

            // Update hint message
            if (hintMessageText != null)
                hintMessageText.text = hintMessage;

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Setup and show the season start screen
        /// </summary>
        public void Setup(Season season, int year, string hintMessage)
        {
            // Update season and year
            if (dayNumberText != null)
                dayNumberText.text = $"{season}, Year {year}";

            // Update hint message
            if (hintMessageText != null)
                hintMessageText.text = hintMessage;

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Handle continue button click
        /// </summary>
        private void HandleContinue()
        {
            // Complete tutorial step if tutorial is active
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
            {
                if (TutorialManager.Instance.GetCurrentStep() == TutorialStep.DayStartHint)
                {
                    TutorialManager.Instance.CompleteCurrentStep();
                }
            }

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
