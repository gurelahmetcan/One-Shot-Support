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
    /// Based on the Guild Merchant Game Design Document:
    /// - Hero Expected Value (Vexp): Base stats × 2 × Lifecycle Multiplier + Greed Premium
    /// - Offer Value (Voff): Signing Bonus + (Salary × 4 × Length) + (Loot Cut × Expected Revenue)
    /// - Tension: Dynamic system based on offer vs expected value
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

        [Header("Expected Season Revenue by Lifecycle")]
        [Tooltip("Average gold per 1% loot cut for Rookie heroes per season (4 turns)")]
        [SerializeField] private float rookieLootCutValue = 50f;

        [Tooltip("Average gold per 1% loot cut for Prime heroes per season (4 turns)")]
        [SerializeField] private float primeLootCutValue = 125f;

        [Tooltip("Average gold per 1% loot cut for Veteran heroes per season (4 turns)")]
        [SerializeField] private float veteranLootCutValue = 200f;

        [Header("Tension Mechanics")]
        [Tooltip("Percentage reduction in tension per extra contract year (10% per year)")]
        [SerializeField] private float tensionReductionPerYear = 0.10f;

        [Tooltip("Trust threshold for zero starting tension (75-100%)")]
        [SerializeField] private float zeroTensionTrustThreshold = 75f;

        [Tooltip("Trust threshold for maximum starting tension (0-25%)")]
        [SerializeField] private float maxTensionTrustThreshold = 25f;

        [Tooltip("Maximum starting tension percentage for low trust heroes")]
        [SerializeField] private float maxStartingTension = 25f;

        [Header("Negotiation Settings")]
        [Tooltip("Walk-away tension threshold (hero refuses negotiation at 100%)")]
        [SerializeField] private float walkAwayThreshold = 100f;

        [Tooltip("Number of turns before walked-away heroes can be re-recruited (Annual Refresh)")]
        [SerializeField] private int reRecruitmentLockoutTurns = 4;

        // === EVENTS ===
        public event Action<HeroData, float> OnTensionChanged;
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
        /// Calculate Hero's Expected Value (Vexp)
        /// Formula: Vexp = ((Prowess + Charisma + Vitality) × 2) × LifecycleMultiplier
        /// The Greed stat acts as a "Premium" added on top of this base value
        /// </summary>
        public float CalculateHeroExpectedValue(HeroData hero)
        {
            if (hero == null)
            {
                Debug.LogError("[ContractNegotiation] Cannot calculate Vexp for null hero");
                return 0f;
            }

            // Get lifecycle multiplier
            float lifecycleMultiplier = GetLifecycleMultiplier(hero.lifeStage);

            // Base calculation: (Prowess + Charisma + Vitality) × 2
            float coreStatsSum = hero.prowess + hero.charisma + hero.maxVitality;
            float baseValue = coreStatsSum * 2f;

            // Apply lifecycle multiplier
            float lifecycleValue = baseValue * lifecycleMultiplier;

            // Greed acts as a premium percentage on top
            // Greed ranges from 30-70, treating it as 30%-70% premium
            float greedPremium = lifecycleValue * (hero.greed / 100f);

            // Apply trait modifiers for Vexp
            float traitModifier = GetTraitVexpModifier(hero);
            float totalValue = (lifecycleValue + greedPremium) * traitModifier;

            Debug.Log($"[ContractNegotiation] {hero.heroName} Vexp: Base={baseValue:F1}, " +
                      $"Lifecycle={lifecycleValue:F1}, Greed Premium={greedPremium:F1}, " +
                      $"Trait Modifier={traitModifier:F2}, Total={totalValue:F1}");

            return totalValue;
        }

        /// <summary>
        /// Calculate Offer Value (Voff)
        /// Formula: Voff = SigningBonus + (Salary × 4 × Length) + (LootCut% × ExpectedSeasonRevenue)
        /// </summary>
        public float CalculateOfferValue(HeroData hero, ContractOffer offer)
        {
            if (hero == null || offer == null)
            {
                Debug.LogError("[ContractNegotiation] Cannot calculate Voff with null parameters");
                return 0f;
            }

            // Signing bonus (one-time payment)
            float signingBonusValue = offer.signingBonus;

            // Salary over contract length (4 turns per year)
            float salaryValue = offer.dailySalary * 4f * offer.contractLengthYears;

            // Loot cut projected value
            float lootCutValue = offer.lootCutPercentage * GetExpectedSeasonRevenue(hero.lifeStage);

            float totalOfferValue = signingBonusValue + salaryValue + lootCutValue;

            Debug.Log($"[ContractNegotiation] {hero.heroName} Voff: Signing={signingBonusValue:F1}, " +
                      $"Salary={salaryValue:F1}, LootCut={lootCutValue:F1}, Total={totalOfferValue:F1}");

            return totalOfferValue;
        }

        /// <summary>
        /// Calculate the ideal contract values where Voff = Vexp
        /// This provides the "anchor" starting point for negotiations
        /// </summary>
        public ContractOffer CalculateIdealOffer(HeroData hero, int desiredContractLength = 2)
        {
            if (hero == null)
            {
                Debug.LogError("[ContractNegotiation] Cannot calculate ideal offer for null hero");
                return null;
            }

            float vexp = CalculateHeroExpectedValue(hero);

            // For simplicity, we'll distribute the value across the contract components
            // This is a starting distribution that players can adjust

            // Signing bonus: ~10% of total value
            float signingBonus = vexp * 0.1f;

            // Remaining value to distribute between salary and loot cut
            float remainingValue = vexp - signingBonus;

            // Split 60/40 between salary and loot cut
            float salaryValue = remainingValue * 0.6f;
            float lootCutValue = remainingValue * 0.4f;

            // Calculate actual salary per turn from total salary value
            float dailySalary = salaryValue / (4f * desiredContractLength);

            // Calculate loot cut percentage from loot cut value
            float expectedRevenue = GetExpectedSeasonRevenue(hero.lifeStage);
            float lootCutPercentage = expectedRevenue > 0 ? lootCutValue / expectedRevenue : 0f;

            ContractOffer idealOffer = new ContractOffer
            {
                signingBonus = signingBonus,
                dailySalary = dailySalary,
                lootCutPercentage = Mathf.Clamp(lootCutPercentage, 0f, 100f),
                contractLengthYears = desiredContractLength
            };

            Debug.Log($"[ContractNegotiation] {hero.heroName} Ideal Offer: " +
                      $"Signing={signingBonus:F1}, Salary={dailySalary:F1}/turn, " +
                      $"LootCut={lootCutPercentage:F1}%, Length={desiredContractLength} years");

            return idealOffer;
        }

        // ========== TENSION MECHANICS ==========

        /// <summary>
        /// Calculate starting tension based on trust level
        /// - 75-100% Trust: 0% Tension
        /// - 25-75% Trust: Linear interpolation (Lerp)
        /// - 0-25% Trust: 25% Tension
        /// </summary>
        public float CalculateStartingTension(float trustLevel)
        {
            trustLevel = Mathf.Clamp(trustLevel, 0f, 100f);

            if (trustLevel >= zeroTensionTrustThreshold)
            {
                // High trust: no tension
                return 0f;
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
                return Mathf.Clamp(tension, 0f, maxStartingTension);
            }
        }

        /// <summary>
        /// Calculate tension delta based on offer value vs expected value
        /// Positive delta = increased tension (bad offer)
        /// Negative delta = decreased tension (good offer)
        /// </summary>
        public float CalculateTensionDelta(HeroData hero, ContractOffer offer, float currentTension)
        {
            float vexp = CalculateHeroExpectedValue(hero);
            float voff = CalculateOfferValue(hero, offer);

            // Calculate value difference as percentage
            float valueDifference = vexp - voff;
            float percentageDifference = (valueDifference / vexp) * 100f;

            // Base tension delta (1:1 with percentage difference)
            float tensionDelta = percentageDifference;

            // Apply contract length mitigation (percentage-based reduction)
            float contractLengthMitigation = CalculateContractLengthMitigation(offer.contractLengthYears);
            tensionDelta *= (1f - contractLengthMitigation);

            // Apply trait modifiers for tension
            float traitTensionModifier = GetTraitTensionModifier(hero);
            tensionDelta *= traitTensionModifier;

            Debug.Log($"[ContractNegotiation] {hero.heroName} Tension Delta: " +
                      $"ValueDiff={valueDifference:F1} ({percentageDifference:F1}%), " +
                      $"LengthMitigation={contractLengthMitigation:F2}, " +
                      $"TraitMod={traitTensionModifier:F2}, Delta={tensionDelta:F1}");

            return tensionDelta;
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
        public bool ApplyTensionChange(HeroData hero, float tensionDelta, ref float currentTension)
        {
            currentTension += tensionDelta;
            currentTension = Mathf.Clamp(currentTension, 0f, 100f);

            OnTensionChanged?.Invoke(hero, currentTension);

            Debug.Log($"[ContractNegotiation] {hero.heroName} Tension: {currentTension:F1}% " +
                      $"(Delta: {tensionDelta:+0.0;-0.0}%)");

            // Check for walk-away
            if (currentTension >= walkAwayThreshold)
            {
                Debug.LogWarning($"[ContractNegotiation] {hero.heroName} walks away! Tension reached {currentTension:F1}%");
                OnHeroWalkAway?.Invoke(hero);
                return true;
            }

            return false;
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
        /// Get expected season revenue (value of 1% loot cut) by lifecycle stage
        /// </summary>
        private float GetExpectedSeasonRevenue(HeroLifecycleStage stage)
        {
            switch (stage)
            {
                case HeroLifecycleStage.Rookie:
                    return rookieLootCutValue;
                case HeroLifecycleStage.Prime:
                    return primeLootCutValue;
                case HeroLifecycleStage.Veteran:
                    return veteranLootCutValue;
                case HeroLifecycleStage.Retired:
                    return 0f;
                default:
                    Debug.LogWarning($"[ContractNegotiation] Unknown lifecycle stage: {stage}");
                    return rookieLootCutValue;
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
            hero.lootCutPercentage = offer.lootCutPercentage;
            hero.contractLengthInYears = offer.contractLengthYears;
            hero.turnsRemainingInContract = offer.contractLengthYears * 4;

            Debug.Log($"[ContractNegotiation] Contract finalized for {hero.heroName}: " +
                      $"Salary={offer.dailySalary:F1}/turn, LootCut={offer.lootCutPercentage:F1}%, " +
                      $"Length={offer.contractLengthYears} years");

            OnNegotiationComplete?.Invoke(hero, offer);
        }
    }

    /// <summary>
    /// Data structure representing a contract offer
    /// </summary>
    [Serializable]
    public class ContractOffer
    {
        [Tooltip("One-time signing bonus in gold")]
        public float signingBonus;

        [Tooltip("Daily salary per turn in gold")]
        public float dailySalary;

        [Tooltip("Percentage of mission loot (0-100)")]
        [Range(0f, 100f)]
        public float lootCutPercentage;

        [Tooltip("Contract length in years")]
        public int contractLengthYears;

        public ContractOffer()
        {
            signingBonus = 0f;
            dailySalary = 10f;
            lootCutPercentage = 20f;
            contractLengthYears = 2;
        }

        public ContractOffer(float bonus, float salary, float lootCut, int years)
        {
            signingBonus = bonus;
            dailySalary = salary;
            lootCutPercentage = lootCut;
            contractLengthYears = years;
        }
    }
}
