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
                totalReputationText.text = $"Total Reputation: {totalReputation}/100";

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
            resultEntries.Add(entryObj);

            // Get components (assuming prefab has these)
            var heroNameText = entryObj.transform.Find("HeroName")?.GetComponent<TextMeshProUGUI>();
            var resultText = entryObj.transform.Find("Result")?.GetComponent<TextMeshProUGUI>();
            var starsText = entryObj.transform.Find("Stars")?.GetComponent<TextMeshProUGUI>();
            var reputationText = entryObj.transform.Find("Reputation")?.GetComponent<TextMeshProUGUI>();
            var successChanceText = entryObj.transform.Find("SuccessChance")?.GetComponent<TextMeshProUGUI>();

            // Populate data
            if (heroNameText != null)
                heroNameText.text = result.hero.heroName;

            if (resultText != null)
            {
                string resultString = result.succeeded ? "<color=green>SUCCESS!</color>" : "<color=red>FAILED</color>";
                resultText.text = resultString;
            }

            if (starsText != null)
            {
                string stars = new string('★', result.stars) + new string('☆', 5 - result.stars);
                starsText.text = stars;
            }

            if (reputationText != null)
            {
                string sign = result.reputationChange > 0 ? "+" : "";
                Color color = result.reputationChange > 0 ? Color.green : Color.red;
                reputationText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{sign}{result.reputationChange}</color>";
            }

            if (successChanceText != null)
                successChanceText.text = $"Chance: {result.successChance}%";
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
