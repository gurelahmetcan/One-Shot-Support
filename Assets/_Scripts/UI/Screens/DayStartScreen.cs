using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using OneShotSupport.Tutorial;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Day start screen that shows day number and daily hint
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
        /// Setup and show the day start screen
        /// </summary>
        public void Setup(int dayNumber, string hintMessage)
        {
            // Update day number
            if (dayNumberText != null)
                dayNumberText.text = $"{dayNumber}";

            // Update hint message
            if (hintMessageText != null)
                hintMessageText.text = hintMessage;

            // Tutorial: Disable continue button during DayStartHint step
            // Player will auto-advance after reading the hint
            bool isTutorialDayStart = TutorialManager.Instance != null &&
                                      TutorialManager.Instance.IsTutorialActive() &&
                                      TutorialManager.Instance.GetCurrentStep() == TutorialStep.DayStartHint;

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(!isTutorialDayStart);
            }

            gameObject.SetActive(true);

            // Tutorial: Auto-advance after a short delay during tutorial
            if (isTutorialDayStart)
            {
                StartCoroutine(AutoAdvanceTutorial());
            }
        }

        /// <summary>
        /// Auto-advance tutorial after showing hint
        /// </summary>
        private System.Collections.IEnumerator AutoAdvanceTutorial()
        {
            // Wait for player to read the hint (3 seconds)
            yield return new WaitForSeconds(3f);

            // Auto-proceed
            HandleContinue();
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
