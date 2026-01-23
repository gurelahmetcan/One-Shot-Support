using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.ScriptableObjects;
using OneShotSupport.Core;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// UI panel for negotiating contracts with heroes
    /// Allows player to adjust signing bonus, salary, and contract length
    /// Shows tension meter and payment preferences
    /// </summary>
    public class NegotiationPanel : MonoBehaviour
    {
        [Header("Hero Display")]
        [SerializeField] private Image heroPortrait;
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private TextMeshProUGUI heroExpectedValueText;
        [SerializeField] private TextMeshProUGUI currentOfferValueText;
        [SerializeField] private TextMeshProUGUI paymentPreferenceText;

        [Header("Tension Display")]
        [SerializeField] private ConfidenceMeter tensionMeter;
        [SerializeField] private TextMeshProUGUI tensionLabelText;

        [Header("Contract Sliders")]
        [SerializeField] private Slider signingBonusSlider;
        [SerializeField] private TextMeshProUGUI signingBonusValueText;
        [SerializeField] private Slider salarySlider;
        [SerializeField] private TextMeshProUGUI salaryValueText;

        [Header("Contract Length Buttons")]
        [SerializeField] private Button oneYearButton;
        [SerializeField] private Button twoYearButton;
        [SerializeField] private Button threeYearButton;
        [SerializeField] private Button fourYearButton;
        [SerializeField] private Button fiveYearButton;

        [Header("Action Buttons")]
        [SerializeField] private Button offerButton;
        [SerializeField] private TextMeshProUGUI offerButtonText;
        [SerializeField] private Button cancelButton;

        [Header("Button Visual States")]
        [SerializeField] private Color selectedYearColor = new Color(0.3f, 0.7f, 1f);
        [SerializeField] private Color normalYearColor = Color.white;

        // Events
        public event Action<HeroData, ContractOffer> OnNegotiationAccepted;
        public event Action<HeroData> OnHeroWalkedAway;
        public event Action OnNegotiationCancelled;

        // State
        private HeroData currentHero;
        private int heroExpectedValue;
        private int selectedContractLength = 2; // Default 2 years
        private int playerCurrentGold;
        private ContractNegotiationManager negotiationManager;

        private void Awake()
        {
            negotiationManager = ContractNegotiationManager.Instance;

            // Setup slider listeners (update display values only, not tension)
            if (signingBonusSlider != null)
            {
                signingBonusSlider.minValue = 0;
                signingBonusSlider.maxValue = 200;
                signingBonusSlider.wholeNumbers = true;
                signingBonusSlider.onValueChanged.AddListener(OnSigningBonusChanged);
            }

            if (salarySlider != null)
            {
                salarySlider.minValue = 0;
                salarySlider.maxValue = 200;
                salarySlider.wholeNumbers = true;
                salarySlider.onValueChanged.AddListener(OnSalaryChanged);
            }

            // Setup contract length buttons
            if (oneYearButton != null)
                oneYearButton.onClick.AddListener(() => SelectContractLength(1));
            if (twoYearButton != null)
                twoYearButton.onClick.AddListener(() => SelectContractLength(2));
            if (threeYearButton != null)
                threeYearButton.onClick.AddListener(() => SelectContractLength(3));
            if (fourYearButton != null)
                fourYearButton.onClick.AddListener(() => SelectContractLength(4));
            if (fiveYearButton != null)
                fiveYearButton.onClick.AddListener(() => SelectContractLength(5));

            // Setup action buttons
            if (offerButton != null)
                offerButton.onClick.AddListener(HandleOfferClicked);
            if (cancelButton != null)
                cancelButton.onClick.AddListener(HandleCancelClicked);
        }

        /// <summary>
        /// Open negotiation panel for a hero
        /// </summary>
        public void Setup(HeroData hero, int currentGold)
        {
            if (negotiationManager == null)
            {
                Debug.LogError("[NegotiationPanel] ContractNegotiationManager not found!");
                return;
            }

            currentHero = hero;
            playerCurrentGold = currentGold;

            // Calculate hero's expected value
            heroExpectedValue = negotiationManager.CalculateHeroExpectedValue(hero);

            // Initialize hero's negotiation state
            hero.InitializeNegotiation();

            // Display hero info
            if (heroNameText != null)
                heroNameText.text = hero.heroName;

            if (heroPortrait != null && hero.portrait != null)
                heroPortrait.sprite = hero.portrait;

            if (heroExpectedValueText != null)
                heroExpectedValueText.text = $"Hero Expects: {heroExpectedValue}g";

            // Display payment preference
            PaymentPreference preference = negotiationManager.GetPaymentPreference(hero);
            if (paymentPreferenceText != null)
            {
                switch (preference)
                {
                    case PaymentPreference.PrefersSigningBonus:
                        paymentPreferenceText.text = "💰 Prefers Signing Bonus (≥30%)";
                        break;
                    case PaymentPreference.PrefersSalary:
                        paymentPreferenceText.text = "📊 Prefers Steady Salary (≤20%)";
                        break;
                    case PaymentPreference.Neutral:
                        paymentPreferenceText.text = "Neutral Payment Preference";
                        break;
                }
            }

            // Set ideal offer as starting point
            ContractOffer idealOffer = negotiationManager.CalculateIdealOffer(hero, 2);
            if (signingBonusSlider != null)
                signingBonusSlider.value = idealOffer.signingBonus;
            if (salarySlider != null)
                salarySlider.value = idealOffer.dailySalary;

            // Select 2 years by default
            SelectContractLength(2);

            // Update initial tension display
            UpdateTensionDisplay(hero.currentTension);

            // Update offer value display
            UpdateOfferDisplay();

            // Update offer button state
            UpdateOfferButtonState();

            // Show panel
            gameObject.SetActive(true);

            Debug.Log($"[NegotiationPanel] Opened negotiation with {hero.heroName} (Vexp: {heroExpectedValue}g)");
        }

        /// <summary>
        /// Handle signing bonus slider changed
        /// </summary>
        private void OnSigningBonusChanged(float value)
        {
            int intValue = Mathf.RoundToInt(value);
            if (signingBonusValueText != null)
                signingBonusValueText.text = $"{intValue}g";

            UpdateOfferDisplay();
            UpdateOfferButtonState();
        }

        /// <summary>
        /// Handle salary slider changed
        /// </summary>
        private void OnSalaryChanged(float value)
        {
            int intValue = Mathf.RoundToInt(value);
            if (salaryValueText != null)
                salaryValueText.text = $"{intValue}g/turn";

            UpdateOfferDisplay();
        }

        /// <summary>
        /// Select contract length and update button visuals
        /// </summary>
        private void SelectContractLength(int years)
        {
            selectedContractLength = years;

            // Update button visuals
            UpdateContractLengthButtons();

            // Update offer display
            UpdateOfferDisplay();

            Debug.Log($"[NegotiationPanel] Selected contract length: {years} years");
        }

        /// <summary>
        /// Update contract length button visuals
        /// </summary>
        private void UpdateContractLengthButtons()
        {
            SetButtonColor(oneYearButton, selectedContractLength == 1);
            SetButtonColor(twoYearButton, selectedContractLength == 2);
            SetButtonColor(threeYearButton, selectedContractLength == 3);
            SetButtonColor(fourYearButton, selectedContractLength == 4);
            SetButtonColor(fiveYearButton, selectedContractLength == 5);
        }

        /// <summary>
        /// Set button color based on selected state
        /// </summary>
        private void SetButtonColor(Button button, bool isSelected)
        {
            if (button == null) return;

            ColorBlock colors = button.colors;
            colors.normalColor = isSelected ? selectedYearColor : normalYearColor;
            colors.highlightedColor = isSelected ? selectedYearColor : normalYearColor;
            button.colors = colors;
        }

        /// <summary>
        /// Update the current offer value display
        /// </summary>
        private void UpdateOfferDisplay()
        {
            int signingBonus = Mathf.RoundToInt(signingBonusSlider.value);
            int salary = Mathf.RoundToInt(salarySlider.value);

            ContractOffer currentOffer = new ContractOffer(signingBonus, salary, selectedContractLength);
            int offerValue = negotiationManager.CalculateOfferValue(currentOffer);

            if (currentOfferValueText != null)
            {
                int difference = offerValue - heroExpectedValue;
                string diffText = difference >= 0 ? $"+{difference}" : $"{difference}";
                currentOfferValueText.text = $"Your Offer: {offerValue}g ({diffText}g)";
            }
        }

        /// <summary>
        /// Update tension display using confidence meter style
        /// </summary>
        private void UpdateTensionDisplay(int tensionValue)
        {
            // Update tension meter (uses confidence meter component with fill and handle)
            if (tensionMeter != null)
                tensionMeter.UpdateConfidence(tensionValue);

            // Update tension label (separate from the meter's percentage text)
            if (tensionLabelText != null)
                tensionLabelText.text = "Tension";
        }

        /// <summary>
        /// Update offer button state (enable/disable based on gold)
        /// </summary>
        private void UpdateOfferButtonState()
        {
            if (offerButton == null) return;

            int signingBonus = Mathf.RoundToInt(signingBonusSlider.value);
            bool canAfford = playerCurrentGold >= signingBonus;

            offerButton.interactable = canAfford;

            if (offerButtonText != null)
            {
                if (!canAfford)
                    offerButtonText.text = "Insufficient Gold";
                else
                    offerButtonText.text = "Make Offer";
            }
        }

        /// <summary>
        /// Handle offer button clicked
        /// </summary>
        private void HandleOfferClicked()
        {
            if (currentHero == null || negotiationManager == null)
            {
                Debug.LogError("[NegotiationPanel] Cannot make offer - missing hero or manager");
                return;
            }

            // Create offer from current slider values
            int signingBonus = Mathf.RoundToInt(signingBonusSlider.value);
            int salary = Mathf.RoundToInt(salarySlider.value);
            ContractOffer offer = new ContractOffer(signingBonus, salary, selectedContractLength);

            // Calculate tension delta
            int tensionDelta = negotiationManager.CalculateTensionDelta(
                currentHero,
                offer,
                currentHero.currentTension
            );

            // Apply tension change
            int newTension = currentHero.currentTension;
            bool walkedAway = negotiationManager.ApplyTensionChange(
                currentHero,
                tensionDelta,
                ref newTension
            );

            currentHero.currentTension = newTension;

            // Update display
            UpdateTensionDisplay(newTension);

            Debug.Log($"[NegotiationPanel] Offer made! Tension: {currentHero.currentTension}% (Delta: {tensionDelta:+0;-0}%)");

            // Check result
            if (walkedAway)
            {
                // Hero walked away!
                HandleHeroWalkAway();
            }
            else
            {
                // Hero accepts!
                HandleNegotiationSuccess(offer);
            }
        }

        /// <summary>
        /// Handle successful negotiation
        /// </summary>
        private void HandleNegotiationSuccess(ContractOffer offer)
        {
            Debug.Log($"[NegotiationPanel] {currentHero.heroName} accepted the offer!");

            // Finalize contract
            negotiationManager.FinalizeContract(currentHero, offer);

            // Notify listeners (will handle gold deduction and hero recruitment)
            OnNegotiationAccepted?.Invoke(currentHero, offer);

            // Close panel
            Hide();
        }

        /// <summary>
        /// Handle hero walking away
        /// </summary>
        private void HandleHeroWalkAway()
        {
            Debug.LogWarning($"[NegotiationPanel] {currentHero.heroName} walked away from negotiations!");

            // Mark hero as walked away (turn number will be set by GameManager)
            // For now just mark the flag
            currentHero.hasWalkedAway = true;
            currentHero.isLockedFromRecruitment = true;

            // Notify listeners
            OnHeroWalkedAway?.Invoke(currentHero);

            // Close panel
            Hide();
        }

        /// <summary>
        /// Handle cancel button clicked
        /// </summary>
        private void HandleCancelClicked()
        {
            Debug.Log("[NegotiationPanel] Negotiation cancelled");

            OnNegotiationCancelled?.Invoke();
            Hide();
        }

        /// <summary>
        /// Hide the panel
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            currentHero = null;
        }

        /// <summary>
        /// Update player gold (called from outside when gold changes)
        /// </summary>
        public void UpdatePlayerGold(int newGold)
        {
            playerCurrentGold = newGold;
            UpdateOfferButtonState();
        }
    }
}
