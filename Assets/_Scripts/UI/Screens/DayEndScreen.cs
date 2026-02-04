using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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
        public RectTransform reputationTarget; // Target position for flying reputation stars
        public RectTransform moneyTarget; // Target position for flying money stars

        [Header("Animation Settings")]
        public float delayBetweenResults = 0.5f;
        public bool animateResults = true;
        public Sprite starSprite; // Star sprite for flying animation
        public float starFlyDuration = 1f; // Duration of star flight
        public float numberCountDuration = 0.5f; // Duration of number counting animation
        public Sprite moneySprite; // Star sprite for flying animation

        private List<GameObject> resultEntries = new List<GameObject>();
        private bool isAnimating = false;
        private int currentDisplayedReputation = 0; // Current reputation shown during animation
        private int currentDisplayedMoney = 0; // Current money shown during animation

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

            // BUG FIX: totalReputation is now the STARTING reputation (before changes)
            // Reputation changes are applied AFTER animations in StartNextDay()
            currentDisplayedReputation = totalReputation;

            // Initialize money display
            if (GameManager.Instance != null && GameManager.Instance.goldManager != null)
            {
                currentDisplayedMoney = GameManager.Instance.goldManager.CurrentGold;
            }

            // Set initial reputation display
            if (totalReputationText != null)
                totalReputationText.text = $"{currentDisplayedReputation}/100";

            if (totalMoneyText != null)
            {
                totalMoneyText.text = $"{currentDisplayedMoney}";
            }

            // Disable continue button during animation
            if (continueButton != null)
                continueButton.interactable = false;

            // Display results (animated or all at once)
            if (animateResults)
            {
                StartCoroutine(AnimateResults(results, totalReputation));
            }
            else
            {
                foreach (var result in results)
                {
                    CreateResultEntry(result);
                }

                // Set final reputation
                currentDisplayedReputation = totalReputation;
                if (totalReputationText != null)
                    totalReputationText.text = $"{totalReputation}/100";

                if (continueButton != null)
                    continueButton.interactable = true;
            }
        }

        /// <summary>
        /// Animate results appearing one by one with star flying and reputation counting
        /// </summary>
        private IEnumerator AnimateResults(List<HeroResult> results, int finalReputation)
        {
            isAnimating = true;

            foreach (var result in results)
            {
                // Create and show the result entry
                GameObject entryObj = CreateResultEntry(result);

                // Wait a moment for result to appear
                yield return new WaitForSeconds(0.2f);

                // Animate REPUTATION star flying from result to reputation
                if (entryObj != null && starSprite != null && reputationTarget != null)
                {
                    yield return StartCoroutine(AnimateStarFlight(entryObj.GetComponent<RectTransform>(), reputationTarget));
                }

                // Animate reputation number counting
                int targetReputation = currentDisplayedReputation + result.reputationChange;
                yield return StartCoroutine(AnimateReputationCount(targetReputation));
                currentDisplayedReputation = targetReputation;

                // Animate MONEY star flying from result to money (if hero earned money)
                if (result.moneyChange > 0 && entryObj != null && starSprite != null && moneyTarget != null)
                {
                    yield return StartCoroutine(AnimateMoneyFlight(entryObj.GetComponent<RectTransform>(), moneyTarget));
                }

                // Animate money number counting
                int targetMoney = currentDisplayedMoney + result.moneyChange;
                yield return StartCoroutine(AnimateMoneyCount(targetMoney));
                currentDisplayedMoney = targetMoney;

                // Small delay before next result
                yield return new WaitForSeconds(delayBetweenResults);
            }

            isAnimating = false;

            // Enable continue button after all animations
            if (continueButton != null)
                continueButton.interactable = true;
        }

        /// <summary>
        /// Create a result entry for a hero
        /// </summary>
        private GameObject CreateResultEntry(HeroResult result)
        {
            if (resultEntryPrefab == null || resultsContainer == null) return null;

            GameObject entryObj = Instantiate(resultEntryPrefab, resultsContainer);
            entryObj.GetComponent<ResultEntryUI>().Initialize(result);
            resultEntries.Add(entryObj);
            return entryObj;
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
        /// Animate a star flying from result entry to target display
        /// </summary>
        private IEnumerator AnimateStarFlight(RectTransform resultTransform, RectTransform targetTransform)
        {
            // Create star object
            GameObject starObj = new GameObject("FlyingStar");
            starObj.transform.SetParent(transform, false);

            Image starImage = starObj.AddComponent<Image>();
            starImage.sprite = starSprite;
            starImage.SetNativeSize();

            RectTransform starRect = starObj.GetComponent<RectTransform>();

            // Set initial position (at result entry)
            Vector3 startPos = resultTransform.position;
            Vector3 endPos = targetTransform.position;

            starRect.position = startPos;

            // Animate movement
            float elapsed = 0f;
            while (elapsed < starFlyDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / starFlyDuration;

                // Ease out curve for smooth deceleration
                float smoothT = 1f - (1f - t) * (1f - t);

                starRect.position = Vector3.Lerp(startPos, endPos, smoothT);

                yield return null;
            }

            // Destroy star when it reaches target
            Destroy(starObj);
        }
        
        private IEnumerator AnimateMoneyFlight(RectTransform resultTransform, RectTransform targetTransform)
        {
            // Create star object
            GameObject moneyObj = new GameObject("FlyingMoney");
            moneyObj.transform.SetParent(transform, false);

            Image moneyImage = moneyObj.AddComponent<Image>();
            moneyImage.sprite = moneySprite;
            moneyImage.SetNativeSize();

            RectTransform moneyRect = moneyObj.GetComponent<RectTransform>();

            // Set initial position (at result entry)
            Vector3 startPos = resultTransform.position;
            Vector3 endPos = targetTransform.position;

            moneyRect.position = startPos;

            // Animate movement
            float elapsed = 0f;
            while (elapsed < starFlyDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / starFlyDuration;

                // Ease out curve for smooth deceleration
                float smoothT = 1f - (1f - t) * (1f - t);

                moneyRect.position = Vector3.Lerp(startPos, endPos, smoothT);

                yield return null;
            }

            // Destroy star when it reaches target
            Destroy(moneyObj);
        }


        /// <summary>
        /// Animate reputation number counting up/down
        /// </summary>
        private IEnumerator AnimateReputationCount(int targetReputation)
        {
            int startReputation = currentDisplayedReputation;

            if (startReputation == targetReputation)
                yield break;

            float elapsed = 0f;
            while (elapsed < numberCountDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / numberCountDuration);

                // Lerp between start and target
                int displayedValue = Mathf.RoundToInt(Mathf.Lerp(startReputation, targetReputation, t));

                // Update text
                if (totalReputationText != null)
                    totalReputationText.text = $"{displayedValue}/100";

                yield return null;
            }

            // Ensure final value is exact
            if (totalReputationText != null)
                totalReputationText.text = $"{targetReputation}/100";
        }

        /// <summary>
        /// Animate money number counting up/down
        /// </summary>
        private IEnumerator AnimateMoneyCount(int targetMoney)
        {
            int startMoney = currentDisplayedMoney;

            if (startMoney == targetMoney)
                yield break;

            float elapsed = 0f;
            while (elapsed < numberCountDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / numberCountDuration);

                // Lerp between start and target
                int displayedValue = Mathf.RoundToInt(Mathf.Lerp(startMoney, targetMoney, t));

                // Update text
                if (totalMoneyText != null)
                    totalMoneyText.text = $"{displayedValue}";

                yield return null;
            }

            // Ensure final value is exact
            if (totalMoneyText != null)
                totalMoneyText.text = $"{targetMoney}";
        }

        /// <summary>
        /// Continue to next day
        /// </summary>
        private void OnContinueClicked()
        {
            if (isAnimating) return;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNextSeason();
            }
        }
    }
}
