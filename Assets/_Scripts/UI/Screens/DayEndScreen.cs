using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using OneShotSupport.Core;
using OneShotSupport.Data;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Day end screen showing all hero results
    /// </summary>
    public class DayEndScreen : MonoBehaviour
    {
        [Header("UI References")]
        public Transform resultsContainer; // Parent for result entries
        public GameObject resultEntryPrefab; // Prefab for each result
        public Button continueButton;
        public TextMeshProUGUI totalReputationText;
        public TextMeshProUGUI totalMoneyText;

        [Header("Animation Settings")]
        public float delayBetweenResults = 0.5f;
        public bool animateResults = true;

        private List<GameObject> resultEntries = new List<GameObject>();
        private bool isAnimating = false;

        private void Awake()
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
        }

        /// <summary>
        /// Display results for the day
        /// </summary>
        public void DisplayResults(List<HeroResult> results, int totalReputation)
        {
            // Clear previous results
            ClearResults();

            // Update total reputation
            if (totalReputationText != null)
                totalReputationText.text = $"{totalReputation}/100";

            if (totalMoneyText != null)
            {
                totalMoneyText.text = $"{GameManager.Instance.goldManager.CurrentGold}";
            }

            // Disable continue button during animation
            if (continueButton != null)
                continueButton.interactable = false;

            // Display results (animated or all at once)
            if (animateResults)
            {
                StartCoroutine(AnimateResults(results));
            }
            else
            {
                foreach (var result in results)
                {
                    CreateResultEntry(result);
                }

                if (continueButton != null)
                    continueButton.interactable = true;
            }
        }

        /// <summary>
        /// Animate results appearing one by one
        /// </summary>
        private System.Collections.IEnumerator AnimateResults(List<HeroResult> results)
        {
            isAnimating = true;

            foreach (var result in results)
            {
                CreateResultEntry(result);
                yield return new WaitForSeconds(delayBetweenResults);
            }

            isAnimating = false;

            // Enable continue button
            if (continueButton != null)
                continueButton.interactable = true;
        }

        /// <summary>
        /// Create a result entry for a hero
        /// </summary>
        private void CreateResultEntry(HeroResult result)
        {
            if (resultEntryPrefab == null || resultsContainer == null) return;

            GameObject entryObj = Instantiate(resultEntryPrefab, resultsContainer);
            entryObj.GetComponent<ResultEntryUI>().Initialize(result);
            resultEntries.Add(entryObj);
        }

        /// <summary>
        /// Clear all result entries
        /// </summary>
        private void ClearResults()
        {
            foreach (var entry in resultEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            resultEntries.Clear();
        }

        /// <summary>
        /// Continue to next day
        /// </summary>
        private void OnContinueClicked()
        {
            if (isAnimating) return;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNextDay();
            }
        }
    }
}
