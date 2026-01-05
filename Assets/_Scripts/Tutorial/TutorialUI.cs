using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace OneShotSupport.Tutorial
{
    /// <summary>
    /// Handles tutorial UI elements (instruction panel, hand animation, highlights)
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        [Header("Instruction Panel")]
        [SerializeField] private GameObject instructionPanel;
        [SerializeField] private Image paperBackground;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private Button continueButton;

        [Header("Hand Animation")]
        [SerializeField] private GameObject handSprite;
        [SerializeField] private RectTransform handTransform;
        [SerializeField] private float handAnimationDuration = 1f;

        [Header("Highlight Overlay")]
        [SerializeField] private GameObject highlightOverlay;

        [Header("UI Element References")]
        [SerializeField] private RectTransform openInventoryButtonTransform;
        [SerializeField] private RectTransform continueButtonTransform;
        [SerializeField] private RectTransform closeInventoryButtonTransform;
        [SerializeField] private RectTransform heroPanelButtonTransform;
        [SerializeField] private RectTransform sendHeroButtonTransform;

        // Tutorial step messages
        private readonly string[] stepMessages = new string[]
        {
            "", // None
            "Welcome to One-Shot Support!\n\nEach day starts with a HINT that tells you which items might be useful.\n\nPay attention to these hints - they help you prepare for the heroes!",
            "This is the MONSTER your hero will face.\n\nLook at the WEAKNESS - this tells you which item category is most effective!\n\nNow click the OPEN INVENTORY button to see your items.",
            "Here are your ITEMS.\n\nNotice the different CATEGORIES (Hygiene, Magic, Catering, Lighting).\n\nItems matching the monster's weakness are more effective!",
            "Now, DRAG items from inventory to the EQUIPMENT SLOTS.\n\nTry dragging an item that MATCHES the monster's weakness!",
            "See the CONFIDENCE METER?\n\nIt shows your hero's chance of success.\n\nMatching items increase confidence. More items = better chance!",
            "Check your HERO'S STATS:\n\n- BASE CHANCE: Starting success percentage\n- SLOTS: How many items they can carry\n- PERK: Special ability\n\nDifferent heroes have different strengths!",
            "Great job! Now click SEND HERO to send them on their quest!\n\nGood luck!",
            "Tutorial Complete!"
        };

        private void Awake()
        {
            // Setup continue button
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            // Hide everything initially
            HideInstructions();
            if (handSprite != null)
                handSprite.SetActive(false);
        }

        /// <summary>
        /// Show instructions for a specific tutorial step
        /// </summary>
        public void ShowStepInstructions(TutorialStep step)
        {
            if (instructionPanel != null)
                instructionPanel.SetActive(true);

            if (instructionText != null)
            {
                int stepIndex = (int)step;
                if (stepIndex >= 0 && stepIndex < stepMessages.Length)
                {
                    instructionText.text = stepMessages[stepIndex];
                }
            }

            // Show/hide continue button based on step
            // Only show continue button for CheckInventory step
            if (continueButton != null)
            {
                bool showContinue = step == TutorialStep.CheckInventory;
                continueButton.gameObject.SetActive(showContinue);
            }

            // Show hand animation for specific steps
            if (step == TutorialStep.ExamineMonster)
            {
                // Point to open inventory button
                ShowHandPointingAt(openInventoryButtonTransform);
            }
            else if (step == TutorialStep.CheckInventory)
            {
                // Point to continue button
                ShowHandPointingAt(continueButtonTransform);
            }
            else if (step == TutorialStep.DragItem)
            {
                // Animate dragging motion
                ShowHandDraggingAnimation();
            }
            else if (step == TutorialStep.CheckConfidence)
            {
                // Point to close inventory button
                ShowHandPointingAt(closeInventoryButtonTransform);
            }
            else if (step == TutorialStep.UnderstandHero)
            {
                // Point to hero panel button
                ShowHandPointingAt(heroPanelButtonTransform);
            }
            else if (step == TutorialStep.SendHero)
            {
                // Point to send hero button
                ShowHandPointingAt(sendHeroButtonTransform);
            }
            else
            {
                HideHandAnimation();
            }
        }

        /// <summary>
        /// Hide instruction panel
        /// </summary>
        public void HideInstructions()
        {
            if (instructionPanel != null)
                instructionPanel.SetActive(false);

            HideHandAnimation();
        }

        /// <summary>
        /// Show hand pointing at a specific UI element
        /// </summary>
        private void ShowHandPointingAt(RectTransform targetTransform)
        {
            if (handSprite != null && handTransform != null && targetTransform != null)
            {
                // Stop any previous animations
                StopAllCoroutines();

                handSprite.SetActive(true);
                // Position hand near target button
                handTransform.position = targetTransform.position;
                // Reset scale to normal in case it was pulsing
                handTransform.localScale = Vector3.one;
                // Start pulsing animation to draw attention
                StartCoroutine(PulseHand());
            }
        }

        /// <summary>
        /// Show hand animation for dragging items
        /// </summary>
        private void ShowHandDraggingAnimation()
        {
            if (handSprite != null)
            {
                // Stop any previous animations
                StopAllCoroutines();

                handSprite.SetActive(true);
                // Reset scale to normal in case it was pulsing
                handTransform.localScale = Vector3.one;
                StartCoroutine(AnimateHandDragging());
            }
        }

        /// <summary>
        /// Hide hand animation
        /// </summary>
        private void HideHandAnimation()
        {
            if (handSprite != null)
            {
                handSprite.SetActive(false);
                StopAllCoroutines();
            }
        }

        /// <summary>
        /// Pulse hand to draw attention
        /// </summary>
        private IEnumerator PulseHand()
        {
            if (handTransform == null) yield break;

            Vector3 originalScale = handTransform.localScale;

            while (true)
            {
                // Scale up
                float elapsed = 0f;
                float pulseDuration = 0.5f;
                while (elapsed < pulseDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / pulseDuration;
                    float scale = Mathf.Lerp(1f, 1.2f, Mathf.Sin(t * Mathf.PI));
                    handTransform.localScale = originalScale * scale;
                    yield return null;
                }

                handTransform.localScale = originalScale;
                yield return new WaitForSeconds(0.3f);
            }
        }

        /// <summary>
        /// Animate hand movement (inventory to equipment slot)
        /// </summary>
        private IEnumerator AnimateHandDragging()
        {
            if (handTransform == null) yield break;

            // TODO: Set actual positions based on inventory and equipment slot positions
            Vector2 startPos = new Vector2(0, -200);  // Inventory area
            Vector2 endPos = new Vector2(200, 100);   // Equipment area

            while (true)
            {
                // Move hand from inventory to equipment
                float elapsed = 0f;
                while (elapsed < handAnimationDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / handAnimationDuration;
                    handTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                    yield return null;
                }

                // Wait a bit
                yield return new WaitForSeconds(0.5f);

                // Reset position
                handTransform.anchoredPosition = startPos;
                yield return new WaitForSeconds(0.5f);
            }
        }

        /// <summary>
        /// Continue button clicked
        /// </summary>
        private void OnContinueClicked()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.CompleteCurrentStep();
            }
        }

        /// <summary>
        /// Show highlight overlay for specific UI element
        /// </summary>
        public void ShowHighlight(RectTransform target)
        {
            if (highlightOverlay != null)
            {
                highlightOverlay.SetActive(true);
                // TODO: Position highlight overlay around target
            }
        }

        /// <summary>
        /// Hide highlight overlay
        /// </summary>
        public void HideHighlight()
        {
            if (highlightOverlay != null)
            {
                highlightOverlay.SetActive(false);
            }
        }
    }
}
