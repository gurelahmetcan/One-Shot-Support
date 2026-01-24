using System;
using UnityEngine;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Manages contract negotiation mechanics including value calculation,
    /// tension mechanics, and walk-away logic.
    ///
    /// SIMPLIFIED SYSTEM:
    /// - Hero Expected Value (Vexp): (Core Stats × 2) × Lifecycle Multiplier + Greed Premium
    /// - Offer Value (Voff): Signing Bonus + (Salary × 4 × Contract Length)
    /// - Only 3 variables: Signing Bonus, Salary, Contract Length
    /// - All values in INTEGER gold
    /// - Payment preferences based on hero traits
    /// </summary>
    public class ContractNegotiationManager : MonoBehaviour
    {
        // === SINGLETON ===
        public static ContractNegotiationManager Instance { get; private set; }

        [Header("Lifecycle Multipliers")]
        [Tooltip("Value multiplier for Rookie heroes (20-25 years)")]
        [SerializeField] private float rookieMultiplier = 0.8f;

        [Tooltip("Value multiplier for Prime heroes (26-35 years)")]
        [SerializeField] private float primeMultiplier = 1.2f;

        [Tooltip("Value multiplier for Veteran heroes (36-45 years)")]
        [SerializeField] private float veteranMultiplier = 1.5f;

        [Header("Tension Mechanics")]
        [Tooltip("Percentage reduction in tension per extra contract year (10% per year)")]
        [SerializeField] private float tensionReductionPerYear = 0.10f;

        [Tooltip("Trust threshold for zero starting tension (75-100%)")]
        [SerializeField] private int zeroTensionTrustThreshold = 75;

        [Tooltip("Trust threshold for maximum starting tension (0-25%)")]
        [SerializeField] private int maxTensionTrustThreshold = 25;

        [Tooltip("Maximum starting tension percentage for low trust heroes")]
        [SerializeField] private int maxStartingTension = 25;

        [Header("Negotiation Settings")]
        [Tooltip("Walk-away tension threshold (hero refuses negotiation at 100%)")]
        [SerializeField] private int walkAwayThreshold = 100;

        [Tooltip("Number of turns before walked-away heroes can be re-recruited (Annual Refresh)")]
        [SerializeField] private int reRecruitmentLockoutTurns = 4;

        [Header("Payment Preference Settings")]
        [Tooltip("Minimum percentage of total value as signing bonus for greedy/impulsive heroes (30%)")]
        [SerializeField] private float minSigningBonusPercentage = 0.3f;

        [Tooltip("Maximum percentage of total value as signing bonus before cautious heroes get upset (20%)")]
        [SerializeField] private float maxSigningBonusPercentage = 0.2f;

        [Tooltip("Additional tension added when payment preference is violated (10%)")]
        [SerializeField] private int paymentPreferencePenalty = 10;

        // === EVENTS ===
        public event Action<HeroData, int> OnTensionChanged;
        public event Action<HeroData> OnHeroWalkAway;
        public event Action<HeroData, ContractOffer> OnNegotiationComplete;

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // ========== VALUE CALCULATION ==========

        /// <summary>
        /// Calculate Hero's Expected Value (Vexp) in integer gold
        /// Formula: Vexp = ((Prowess + Charisma + Vitality) × 2) × LifecycleMultiplier + Greed Premium
        /// </summary>
        public int CalculateHeroExpectedValue(HeroData hero)
        {
            if (hero == null)
            {
                Debug.LogError("[ContractNegotiation] Cannot calculate Vexp for null hero");
                return 0;
            }

            // Get lifecycle multiplier
            float lifecycleMultiplier = GetLifecycleMultiplier(hero.lifeStage);

            // Base calculation: Average of all 5 core stats × 2 (5-stat system)
            int coreStatsSum = hero.might + hero.charm + hero.wit + hero.agility + hero.fortitude;
            float baseValue = (coreStatsSum / 5f) * 2f;

            // Apply lifecycle multiplier
            float lifecycleValue = baseValue * lifecycleMultiplier;

            // Greed acts as a premium percentage on top
            // Greed ranges from 30-70, treating it as 30%-70% premium
            float greedPremium = lifecycleValue * (hero.greed / 100f);

            // Apply trait modifiers for Vexp
            float traitModifier = GetTraitVexpModifier(hero);
            float totalValue = (lifecycleValue + greedPremium) * traitModifier;

            // Round to integer
            int vexp = Mathf.RoundToInt(totalValue);

            Debug.Log($"[ContractNegotiation] {hero.heroName} Vexp: Base={baseValue:F0}, " +
                      $"Lifecycle={lifecycleValue:F0}, Greed Premium={greedPremium:F0}, " +
                      $"Trait Modifier={traitModifier:F2}, Total={vexp}g");

            return vexp;
        }

        /// <summary>
        /// Calculate Offer Value (Voff) in integer gold
        /// SIMPLIFIED FORMULA: Voff = Signing Bonus + (Salary × 4 × Contract Length)
        /// </summary>
        public int CalculateOfferValue(ContractOffer offer)
        {
            if (offer == null)
            {
                Debug.LogError("[ContractNegotiation] Cannot calculate Voff with null offer");
                return 0;
            }

            // Signing bonus (one-time payment)
            int signingBonusValue = offer.signingBonus;

            // Salary over contract length (4 turns per year)
            int salaryValue = offer.dailySalary * 4 * offer.contractLengthYears;

            int totalOfferValue = signingBonusValue + salaryValue;

            Debug.Log($"[ContractNegotiation] Voff: Signing={signingBonusValue}g, " +
                      $"Salary={salaryValue}g ({offer.dailySalary}g × 4 × {offer.contractLengthYears}yr), " +
                      $"Total={totalOfferValue}g");

            return totalOfferValue;
        }

        /// <summary>
        /// Calculate the ideal contract values where Voff = Vexp
        /// This provides the "anchor" starting point for negotiations
        /// Default distribution: 20% signing, 80% salary spread over contract length
        /// </summary>
        public ContractOffer CalculateIdealOffer(HeroData hero, int desiredContractLength = 2)
        {
            if (hero == null)
            {
                Debug.LogError("[ContractNegotiation] Cannot calculate ideal offer for null hero");
                return null;
            }

            int vexp = CalculateHeroExpectedValue(hero);

            // Default distribution based on payment preferences
            PaymentPreference preference = GetPaymentPreference(hero);
            float signingPercentage = 0.2f; // Default 20%

            switch (preference)
            {
                case PaymentPreference.PrefersSigningBonus:
                    signingPercentage = 0.4f; // 40% upfront
                    break;
                case PaymentPreference.PrefersSalary:
                    signingPercentage = 0.1f; // 10% upfront
                    break;
                case PaymentPreference.Neutral:
                default:
                    signingPercentage = 0.2f; // 20% upfront
                    break;
            }

            // Calculate signing bonus
            int signingBonus = Mathf.RoundToInt(vexp * signingPercentage);

            // Remaining value goes to salary
            int remainingValue = vexp - signingBonus;

            // Calculate salary per turn
            int turnsInContract = 4 * desiredContractLength;
            int dailySalary = Mathf.RoundToInt((float)remainingValue / turnsInContract);

            ContractOffer idealOffer = new ContractOffer
            {
                signingBonus = signingBonus,
                dailySalary = dailySalary,
                contractLengthYears = desiredContractLength
            };

            Debug.Log($"[ContractNegotiation] {hero.heroName} Ideal Offer ({preference}): " +
                      $"Signing={signingBonus}g ({signingPercentage * 100:F0}%), " +
                      $"Salary={dailySalary}g/turn, Length={desiredContractLength} years, " +
                      $"Total={CalculateOfferValue(idealOffer)}g (Target: {vexp}g)");

            return idealOffer;
        }

        // ========== TENSION MECHANICS ==========

        /// <summary>
        /// Calculate starting tension based on trust level (integer)
        /// - 75-100% Trust: 0% Tension
        /// - 25-75% Trust: Linear interpolation
        /// - 0-25% Trust: 25% Tension
        /// </summary>
        public int CalculateStartingTension(int trustLevel)
        {
            trustLevel = Mathf.Clamp(trustLevel, 0, 100);

            if (trustLevel >= zeroTensionTrustThreshold)
            {
                // High trust: no tension
                return 0;
            }
            else if (trustLevel <= maxTensionTrustThreshold)
            {
                // Low trust: maximum tension
                return maxStartingTension;
            }
            else
            {
                // Middle range: linear interpolation
                // T_start = (75 - Trust) × 0.5
                float tension = (zeroTensionTrustThreshold - trustLevel) * 0.5f;
                return Mathf.RoundToInt(Mathf.Clamp(tension, 0f, maxStartingTension));
            }
        }

        /// <summary>
        /// Calculate tension delta based on offer value vs expected value
        /// Positive delta = increased tension (bad offer)
        /// Negative delta = decreased tension (good offer)
        /// Includes payment preference checking
        /// </summary>
        public int CalculateTensionDelta(HeroData hero, ContractOffer offer, int currentTension)
        {
            int vexp = CalculateHeroExpectedValue(hero);
            int voff = CalculateOfferValue(offer);

            // Calculate value difference as percentage
            int valueDifference = vexp - voff;
            float percentageDifference = ((float)valueDifference / vexp) * 100f;

            // Base tension delta (1:1 with percentage difference)
            float tensionDelta = percentageDifference;

            // Apply contract length mitigation (percentage-based reduction)
            float contractLengthMitigation = CalculateContractLengthMitigation(offer.contractLengthYears);
            tensionDelta *= (1f - contractLengthMitigation);

            // Apply trait modifiers for tension
            float traitTensionModifier = GetTraitTensionModifier(hero);
            tensionDelta *= traitTensionModifier;

            // Check payment preference violation
            int paymentPenalty = CheckPaymentPreferenceViolation(hero, offer, vexp);
            tensionDelta += paymentPenalty;

            int finalDelta = Mathf.RoundToInt(tensionDelta);

            Debug.Log($"[ContractNegotiation] {hero.heroName} Tension Delta: " +
                      $"ValueDiff={valueDifference}g ({percentageDifference:F1}%), " +
                      $"LengthMitigation={contractLengthMitigation:F2}, " +
                      $"TraitMod={traitTensionModifier:F2}, PaymentPenalty={paymentPenalty}%, " +
                      $"FinalDelta={finalDelta:+0;-0}%");

            return finalDelta;
        }

        /// <summary>
        /// Calculate contract length mitigation for tension
        /// Formula: T_reduction = (ContractLength - 1) × 0.10
        /// Longer contracts reduce tension generated
        /// </summary>
        public float CalculateContractLengthMitigation(int contractLengthYears)
        {
            float reduction = (contractLengthYears - 1) * tensionReductionPerYear;
            return Mathf.Clamp(reduction, 0f, 0.5f); // Cap at 50% reduction for very long contracts
        }

        /// <summary>
        /// Apply tension change and check for walk-away
        /// Returns true if hero walks away
        /// </summary>
        public bool ApplyTensionChange(HeroData hero, int tensionDelta, ref int currentTension)
        {
            currentTension += tensionDelta;
            currentTension = Mathf.Clamp(currentTension, 0, 100);

            OnTensionChanged?.Invoke(hero, currentTension);

            Debug.Log($"[ContractNegotiation] {hero.heroName} Tension: {currentTension}% " +
                      $"(Delta: {tensionDelta:+0;-0}%)");

            // Check for walk-away
            if (currentTension >= walkAwayThreshold)
            {
                Debug.LogWarning($"[ContractNegotiation] {hero.heroName} walks away! Tension reached {currentTension}%");
                OnHeroWalkAway?.Invoke(hero);
                return true;
            }

            return false;
        }

        // ========== PAYMENT PREFERENCES ==========

        /// <summary>
        /// Determine hero's payment preference based on traits
        /// </summary>
        public PaymentPreference GetPaymentPreference(HeroData hero)
        {
            if (hero == null) return PaymentPreference.Neutral;

            foreach (var trait in hero.traits)
            {
                if (trait == null) continue;
                string traitName = trait.traitName.ToLower();

                // Prefers high signing bonus
                if (traitName.Contains("greedy") || traitName.Contains("impulsive") || traitName.Contains("impatient"))
                {
                    return PaymentPreference.PrefersSigningBonus;
                }

                // Prefers steady salary
                if (traitName.Contains("cautious") || traitName.Contains("patient") || traitName.Contains("steady"))
                {
                    return PaymentPreference.PrefersSalary;
                }
            }

            return PaymentPreference.Neutral;
        }

        /// <summary>
        /// Check if the payment structure violates hero's preferences
        /// Returns additional tension penalty if violated
        /// </summary>
        public int CheckPaymentPreferenceViolation(HeroData hero, ContractOffer offer, int vexp)
        {
            PaymentPreference preference = GetPaymentPreference(hero);

            if (preference == PaymentPreference.Neutral)
                return 0; // No preference, no penalty

            int totalOffer = CalculateOfferValue(offer);
            if (totalOffer == 0) return 0;

            float signingPercentage = (float)offer.signingBonus / totalOffer;

            string violationReason = "";

            switch (preference)
            {
                case PaymentPreference.PrefersSigningBonus:
                    // Wants at least 30% upfront
                    if (signingPercentage < minSigningBonusPercentage)
                    {
                        violationReason = $"Wants ≥{minSigningBonusPercentage * 100:F0}% signing bonus, got {signingPercentage * 100:F0}%";
                        Debug.LogWarning($"[ContractNegotiation] {hero.heroName} payment preference violated! {violationReason}");
                        return paymentPreferencePenalty;
                    }
                    break;

                case PaymentPreference.PrefersSalary:
                    // Doesn't want more than 20% upfront
                    if (signingPercentage > maxSigningBonusPercentage)
                    {
                        violationReason = $"Wants ≤{maxSigningBonusPercentage * 100:F0}% signing bonus, got {signingPercentage * 100:F0}%";
                        Debug.LogWarning($"[ContractNegotiation] {hero.heroName} payment preference violated! {violationReason}");
                        return paymentPreferencePenalty;
                    }
                    break;
            }

            return 0; // No violation
        }

        // ========== HELPER METHODS ==========

        /// <summary>
        /// Get lifecycle multiplier for expected value calculation
        /// </summary>
        private float GetLifecycleMultiplier(HeroLifecycleStage stage)
        {
            switch (stage)
            {
                case HeroLifecycleStage.Rookie:
                    return rookieMultiplier;
                case HeroLifecycleStage.Prime:
                    return primeMultiplier;
                case HeroLifecycleStage.Veteran:
                    return veteranMultiplier;
                case HeroLifecycleStage.Retired:
                    return 0f; // Retired heroes shouldn't be negotiated with
                default:
                    Debug.LogWarning($"[ContractNegotiation] Unknown lifecycle stage: {stage}");
                    return 1f;
            }
        }

        /// <summary>
        /// Get trait modifier for Vexp (expected value)
        /// Traits like "Greedy" increase Vexp, "Frugal" decreases it
        /// </summary>
        private float GetTraitVexpModifier(HeroData hero)
        {
            float modifier = 1f;

            foreach (var trait in hero.traits)
            {
                if (trait == null) continue;

                // Check for specific negotiation-affecting traits
                string traitName = trait.traitName.ToLower();

                if (traitName.Contains("greedy"))
                {
                    modifier *= 1.3f; // Greedy heroes expect 30% more
                }
                else if (traitName.Contains("frugal") || traitName.Contains("humble"))
                {
                    modifier *= 0.7f; // Frugal heroes expect 30% less
                }
                else if (traitName.Contains("ambitious"))
                {
                    modifier *= 1.2f; // Ambitious heroes expect 20% more
                }
                else if (traitName.Contains("loyal"))
                {
                    modifier *= 0.85f; // Loyal heroes are more flexible
                }
            }

            return modifier;
        }

        /// <summary>
        /// Get trait modifier for tension generation
        /// Some traits make heroes more/less sensitive to poor offers
        /// </summary>
        private float GetTraitTensionModifier(HeroData hero)
        {
            float modifier = 1f;

            foreach (var trait in hero.traits)
            {
                if (trait == null) continue;

                string traitName = trait.traitName.ToLower();

                if (traitName.Contains("hotheaded") || traitName.Contains("impatient"))
                {
                    modifier *= 1.5f; // Tension builds 50% faster
                }
                else if (traitName.Contains("patient") || traitName.Contains("calm"))
                {
                    modifier *= 0.7f; // Tension builds 30% slower
                }
                else if (traitName.Contains("stubborn"))
                {
                    modifier *= 1.3f; // Tension builds 30% faster
                }
                else if (traitName.Contains("flexible"))
                {
                    modifier *= 0.8f; // Tension builds 20% slower
                }
            }

            return modifier;
        }

        // ========== NEGOTIATION OUTCOME ==========

        /// <summary>
        /// Finalize contract and apply to hero
        /// </summary>
        public void FinalizeContract(HeroData hero, ContractOffer offer)
        {
            if (hero == null || offer == null)
            {
                Debug.LogError("[ContractNegotiation] Cannot finalize contract with null parameters");
                return;
            }

            // Apply contract terms to hero
            hero.dailySalary = offer.dailySalary;
            hero.contractLengthInYears = offer.contractLengthYears;
            hero.turnsRemainingInContract = offer.contractLengthYears * 4;

            Debug.Log($"[ContractNegotiation] Contract finalized for {hero.heroName}: " +
                      $"Signing={offer.signingBonus}g, Salary={offer.dailySalary}g/turn, " +
                      $"Length={offer.contractLengthYears} years, Total={CalculateOfferValue(offer)}g");

            OnNegotiationComplete?.Invoke(hero, offer);
        }
    }

    /// <summary>
    /// Payment preference enum for hero negotiation styles
    /// </summary>
    public enum PaymentPreference
    {
        Neutral,                // No strong preference
        PrefersSigningBonus,    // Wants more upfront (Greedy, Impulsive)
        PrefersSalary           // Wants steady payments (Cautious, Patient)
    }

    /// <summary>
    /// Data structure representing a contract offer (SIMPLIFIED - no loot cut)
    /// </summary>
    [Serializable]
    public class ContractOffer
    {
        [Tooltip("One-time signing bonus in gold")]
        public int signingBonus;

        [Tooltip("Daily salary per turn in gold")]
        public int dailySalary;

        [Tooltip("Contract length in years")]
        public int contractLengthYears;

        public ContractOffer()
        {
            signingBonus = 0;
            dailySalary = 10;
            contractLengthYears = 2;
        }

        public ContractOffer(int bonus, int salary, int years)
        {
            signingBonus = bonus;
            dailySalary = salary;
            contractLengthYears = years;
        }
    }
}
