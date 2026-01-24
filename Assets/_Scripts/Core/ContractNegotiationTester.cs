using UnityEngine;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;
using System.Text;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Test and debug script for Contract Negotiation mechanics (SIMPLIFIED SYSTEM)
    /// Add this to a GameObject to test value calculations, tension mechanics, and walk-away logic
    /// Tests the simplified 3-variable system: Signing Bonus, Salary, Contract Length
    /// </summary>
    public class ContractNegotiationTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [Tooltip("Enable automatic testing on Start")]
        [SerializeField] private bool runTestOnStart = false;

        [Tooltip("Test hero for negotiation")]
        [SerializeField] private HeroData testHero;

        [Header("Test Offer Parameters (Integers Only)")]
        [SerializeField] private int testSigningBonus = 50;
        [SerializeField] private int testDailySalary = 15;
        [SerializeField] private int testContractLength = 2;

        private ContractNegotiationManager negotiationManager;

        private void Start()
        {
            negotiationManager = ContractNegotiationManager.Instance;

            if (negotiationManager == null)
            {
                Debug.LogError("[NegotiationTester] ContractNegotiationManager not found in scene!");
                return;
            }

            if (runTestOnStart && testHero != null)
            {
                RunCompleteTest();
            }
        }

        /// <summary>
        /// Run a complete negotiation test with the configured hero
        /// </summary>
        [ContextMenu("Run Complete Negotiation Test")]
        public void RunCompleteTest()
        {
            if (testHero == null)
            {
                Debug.LogWarning("[NegotiationTester] No test hero assigned!");
                return;
            }

            if (negotiationManager == null)
            {
                Debug.LogError("[NegotiationTester] ContractNegotiationManager not found!");
                return;
            }

            Debug.Log("========================================");
            Debug.Log("STARTING CONTRACT NEGOTIATION TEST (SIMPLIFIED SYSTEM)");
            Debug.Log("========================================\n");

            // Test 1: Hero Information
            TestHeroInformation();

            // Test 2: Expected Value Calculation
            TestExpectedValue();

            // Test 3: Payment Preference
            TestPaymentPreference();

            // Test 4: Ideal Offer Calculation
            TestIdealOffer();

            // Test 5: Custom Offer Evaluation
            TestCustomOffer();

            // Test 6: Starting Tension
            TestStartingTension();

            // Test 7: Tension Delta Calculation
            TestTensionDelta();

            // Test 8: Payment Preference Violation
            TestPaymentPreferenceViolation();

            // Test 9: Walk-Away Simulation
            TestWalkAwayMechanics();

            Debug.Log("\n========================================");
            Debug.Log("NEGOTIATION TEST COMPLETE");
            Debug.Log("========================================");
        }

        /// <summary>
        /// Test 1: Display hero information
        /// </summary>
        [ContextMenu("Test 1: Hero Information")]
        private void TestHeroInformation()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--- TEST 1: HERO INFORMATION ---");
            sb.AppendLine($"Name: {testHero.heroName}");
            sb.AppendLine($"Age: {testHero.currentAge} | Stage: {testHero.lifeStage}");
            sb.AppendLine($"Level: {testHero.level}");
            sb.AppendLine("\nCore Stats (5-Stat System):");
            sb.AppendLine($"  Might: {testHero.might}");
            sb.AppendLine($"  Charm: {testHero.charm}");
            sb.AppendLine($"  Wit: {testHero.wit}");
            sb.AppendLine($"  Agility: {testHero.agility}");
            sb.AppendLine($"  Fortitude: {testHero.fortitude}");
            sb.AppendLine($"  Greed: {testHero.greed}");
            sb.AppendLine($"\nTrust Level: {testHero.trustLevel}%");
            sb.AppendLine($"Bond Level: {testHero.bondLevel}");

            if (testHero.traits.Count > 0)
            {
                sb.AppendLine("\nTraits:");
                foreach (var trait in testHero.traits)
                {
                    if (trait != null)
                    {
                        sb.AppendLine($"  - {trait.traitName}: {trait.description}");
                    }
                }
            }

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Test 2: Calculate hero's expected value
        /// </summary>
        [ContextMenu("Test 2: Expected Value")]
        private void TestExpectedValue()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n--- TEST 2: EXPECTED VALUE (Vexp) ---");

            int vexp = negotiationManager.CalculateHeroExpectedValue(testHero);

            int coreStatsSum = testHero.might + testHero.charm + testHero.wit + testHero.agility + testHero.fortitude;
            float avgStats = coreStatsSum / 5f;
            int baseValue = Mathf.RoundToInt(avgStats * 2f);

            sb.AppendLine($"Core Stats Sum: {coreStatsSum} (Avg: {avgStats:F1})");
            sb.AppendLine($"Base Value (Avg × 2): {baseValue}g");
            sb.AppendLine($"Lifecycle Stage: {testHero.lifeStage}");
            sb.AppendLine($"Greed Premium: {testHero.greed}%");
            sb.AppendLine($"\n>>> TOTAL Vexp: {vexp}g <<<");

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Test 3: Determine payment preference
        /// </summary>
        [ContextMenu("Test 3: Payment Preference")]
        private void TestPaymentPreference()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n--- TEST 3: PAYMENT PREFERENCE ---");

            PaymentPreference preference = negotiationManager.GetPaymentPreference(testHero);

            sb.AppendLine($"Hero: {testHero.heroName}");
            sb.AppendLine($"Payment Preference: {preference}");

            switch (preference)
            {
                case PaymentPreference.PrefersSigningBonus:
                    sb.AppendLine("  → Wants HIGH signing bonus (≥30% upfront)");
                    sb.AppendLine("  → Traits: Greedy, Impulsive, or Impatient detected");
                    break;
                case PaymentPreference.PrefersSalary:
                    sb.AppendLine("  → Wants STEADY salary (≤20% upfront)");
                    sb.AppendLine("  → Traits: Cautious, Patient, or Steady detected");
                    break;
                case PaymentPreference.Neutral:
                    sb.AppendLine("  → No strong preference");
                    sb.AppendLine("  → ~20% signing, ~80% salary is fine");
                    break;
            }

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Test 4: Calculate ideal offer where Voff = Vexp
        /// </summary>
        [ContextMenu("Test 4: Ideal Offer")]
        private void TestIdealOffer()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n--- TEST 4: IDEAL OFFER (Voff = Vexp) ---");

            ContractOffer idealOffer = negotiationManager.CalculateIdealOffer(testHero, testContractLength);
            int vexp = negotiationManager.CalculateHeroExpectedValue(testHero);
            int voff = negotiationManager.CalculateOfferValue(idealOffer);

            sb.AppendLine($"Hero Vexp: {vexp}g");
            sb.AppendLine($"Payment Preference: {negotiationManager.GetPaymentPreference(testHero)}");
            sb.AppendLine("\nIdeal Offer Components:");
            sb.AppendLine($"  Signing Bonus: {idealOffer.signingBonus}g");
            sb.AppendLine($"  Daily Salary: {idealOffer.dailySalary}g/turn");
            sb.AppendLine($"  Contract Length: {idealOffer.contractLengthYears} years");
            sb.AppendLine($"\n  Calculation:");
            sb.AppendLine($"  Salary Total: {idealOffer.dailySalary}g × 4 turns × {idealOffer.contractLengthYears}yr = {idealOffer.dailySalary * 4 * idealOffer.contractLengthYears}g");
            sb.AppendLine($"\n>>> TOTAL Voff: {voff}g <<<");
            sb.AppendLine($"Difference: {(voff - vexp):+0;-0}g");

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Test 5: Evaluate custom offer
        /// </summary>
        [ContextMenu("Test 5: Custom Offer")]
        private void TestCustomOffer()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n--- TEST 5: CUSTOM OFFER EVALUATION ---");

            ContractOffer customOffer = new ContractOffer(
                testSigningBonus,
                testDailySalary,
                testContractLength
            );

            int vexp = negotiationManager.CalculateHeroExpectedValue(testHero);
            int voff = negotiationManager.CalculateOfferValue(customOffer);

            sb.AppendLine($"Hero Vexp: {vexp}g");
            sb.AppendLine("\nCustom Offer Components:");
            sb.AppendLine($"  Signing Bonus: {customOffer.signingBonus}g");
            sb.AppendLine($"  Daily Salary: {customOffer.dailySalary}g/turn");
            sb.AppendLine($"  Contract Length: {customOffer.contractLengthYears} years");
            sb.AppendLine($"\n  Calculation:");
            sb.AppendLine($"  {customOffer.signingBonus}g + ({customOffer.dailySalary}g × 4 × {customOffer.contractLengthYears}yr)");
            sb.AppendLine($"  = {customOffer.signingBonus}g + {customOffer.dailySalary * 4 * customOffer.contractLengthYears}g");
            sb.AppendLine($"\n>>> TOTAL Voff: {voff}g <<<");

            int difference = voff - vexp;
            float percentageDiff = ((float)difference / vexp) * 100f;

            sb.AppendLine($"\nValue Difference: {difference:+0;-0}g ({percentageDiff:+0.0;-0.0}%)");

            if (difference < 0)
            {
                sb.AppendLine("Status: ⚠️ LOWBALL - Hero will be unhappy!");
            }
            else if (difference > vexp * 0.2f)
            {
                sb.AppendLine("Status: 💰 OVERPAYING - Player loses profit margin!");
            }
            else
            {
                sb.AppendLine("Status: ✅ FAIR - Good negotiation range");
            }

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Test 6: Calculate starting tension from trust
        /// </summary>
        [ContextMenu("Test 6: Starting Tension")]
        private void TestStartingTension()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n--- TEST 6: STARTING TENSION ---");

            sb.AppendLine("Testing different trust levels:\n");

            int[] trustLevels = { 100, 75, 50, 25, 0 };
            foreach (int trust in trustLevels)
            {
                int tension = negotiationManager.CalculateStartingTension(trust);
                sb.AppendLine($"Trust {trust}% => Starting Tension: {tension}%");
            }

            sb.AppendLine($"\nCurrent Hero ({testHero.heroName}):");
            sb.AppendLine($"Trust: {testHero.trustLevel}%");
            int currentTension = negotiationManager.CalculateStartingTension(testHero.trustLevel);
            sb.AppendLine($"Starting Tension: {currentTension}%");

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Test 7: Calculate tension delta from different offers
        /// </summary>
        [ContextMenu("Test 7: Tension Delta")]
        private void TestTensionDelta()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n--- TEST 7: TENSION DELTA ---");

            ContractOffer idealOffer = negotiationManager.CalculateIdealOffer(testHero, testContractLength);
            ContractOffer lowballOffer = new ContractOffer(
                Mathf.RoundToInt(idealOffer.signingBonus * 0.5f),
                Mathf.RoundToInt(idealOffer.dailySalary * 0.7f),
                testContractLength
            );
            ContractOffer generousOffer = new ContractOffer(
                Mathf.RoundToInt(idealOffer.signingBonus * 1.5f),
                Mathf.RoundToInt(idealOffer.dailySalary * 1.2f),
                testContractLength
            );

            int currentTension = 0;

            sb.AppendLine("Offer Scenarios:\n");

            // Ideal offer
            int idealDelta = negotiationManager.CalculateTensionDelta(testHero, idealOffer, currentTension);
            sb.AppendLine($"1. IDEAL OFFER: Tension Delta = {idealDelta:+0;-0}%");

            // Lowball offer
            int lowballDelta = negotiationManager.CalculateTensionDelta(testHero, lowballOffer, currentTension);
            sb.AppendLine($"2. LOWBALL OFFER (~70% value): Tension Delta = {lowballDelta:+0;-0}%");

            // Generous offer
            int generousDelta = negotiationManager.CalculateTensionDelta(testHero, generousOffer, currentTension);
            sb.AppendLine($"3. GENEROUS OFFER (~120% value): Tension Delta = {generousDelta:+0;-0}%");

            // Contract length mitigation
            sb.AppendLine("\nContract Length Mitigation:");
            for (int years = 1; years <= 5; years++)
            {
                float mitigation = negotiationManager.CalculateContractLengthMitigation(years);
                sb.AppendLine($"  {years} year(s): {mitigation * 100f:F0}% tension reduction");
            }

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Test 8: Check payment preference violations
        /// </summary>
        [ContextMenu("Test 8: Payment Preference Violation")]
        private void TestPaymentPreferenceViolation()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n--- TEST 8: PAYMENT PREFERENCE VIOLATION ---");

            PaymentPreference preference = negotiationManager.GetPaymentPreference(testHero);
            int vexp = negotiationManager.CalculateHeroExpectedValue(testHero);

            sb.AppendLine($"Hero: {testHero.heroName}");
            sb.AppendLine($"Preference: {preference}");
            sb.AppendLine($"Vexp: {vexp}g\n");

            // Test different payment structures
            ContractOffer[] testOffers = new ContractOffer[]
            {
                new ContractOffer(0, 50, 2),              // 0% signing, 100% salary
                new ContractOffer(100, 37, 2),            // ~20% signing, 80% salary
                new ContractOffer(200, 25, 2),            // ~40% signing, 60% salary
                new ContractOffer(300, 12, 2),            // ~60% signing, 40% salary
            };

            foreach (var offer in testOffers)
            {
                int voff = negotiationManager.CalculateOfferValue(offer);
                float signingPercentage = voff > 0 ? ((float)offer.signingBonus / voff) * 100f : 0f;
                int penalty = negotiationManager.CheckPaymentPreferenceViolation(testHero, offer, vexp);

                sb.AppendLine($"Offer: {offer.signingBonus}g signing + {offer.dailySalary}g/turn × 2yr = {voff}g");
                sb.AppendLine($"  Signing: {signingPercentage:F0}% | Penalty: {penalty:+0;-0}%");
                sb.AppendLine();
            }

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Test 9: Simulate walk-away mechanics
        /// </summary>
        [ContextMenu("Test 9: Walk-Away Mechanics")]
        private void TestWalkAwayMechanics()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n--- TEST 9: WALK-AWAY MECHANICS ---");

            sb.AppendLine($"Initial State:");
            sb.AppendLine($"  Has Walked Away: {testHero.hasWalkedAway}");
            sb.AppendLine($"  Is Locked: {testHero.isLockedFromRecruitment}");

            // Simulate walk-away
            int currentTurn = 10;
            testHero.MarkAsWalkedAway(currentTurn);

            sb.AppendLine($"\nAfter Walking Away (Turn {currentTurn}):");
            sb.AppendLine($"  Has Walked Away: {testHero.hasWalkedAway}");
            sb.AppendLine($"  Walk Away Turn: {testHero.walkAwayTurn}");
            sb.AppendLine($"  Is Locked: {testHero.isLockedFromRecruitment}");

            // Test re-recruitment checks
            sb.AppendLine("\nRe-recruitment Availability:");
            for (int turn = currentTurn; turn <= currentTurn + 5; turn++)
            {
                bool canRecruit = testHero.CanBeReRecruited(turn, 4);
                int turnsSince = turn - currentTurn;
                sb.AppendLine($"  Turn {turn} (+ {turnsSince} turns): {(canRecruit ? "✅ AVAILABLE" : "🔒 LOCKED")}");
            }

            // Reset status
            testHero.ResetWalkAwayStatus();
            sb.AppendLine($"\nAfter Reset:");
            sb.AppendLine($"  Has Walked Away: {testHero.hasWalkedAway}");
            sb.AppendLine($"  Is Locked: {testHero.isLockedFromRecruitment}");

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Quick test: Print all key values for current configuration
        /// </summary>
        [ContextMenu("Quick Test: Current Configuration")]
        public void QuickTest()
        {
            if (testHero == null)
            {
                Debug.LogWarning("[NegotiationTester] No test hero assigned!");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine($"QUICK TEST: {testHero.heroName}");
            sb.AppendLine("========================================");

            int vexp = negotiationManager.CalculateHeroExpectedValue(testHero);
            ContractOffer ideal = negotiationManager.CalculateIdealOffer(testHero, 2);
            PaymentPreference pref = negotiationManager.GetPaymentPreference(testHero);

            sb.AppendLine($"\nExpected Value (Vexp): {vexp}g");
            sb.AppendLine($"Payment Preference: {pref}");
            sb.AppendLine($"Ideal Offer: {ideal.signingBonus}g signing + {ideal.dailySalary}g/turn × 2yr");
            sb.AppendLine($"Starting Tension: {negotiationManager.CalculateStartingTension(testHero.trustLevel)}%");

            Debug.Log(sb.ToString());
        }
    }
}
