using UnityEngine;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;
using System.Text;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Test and debug script for Contract Negotiation mechanics
    /// Add this to a GameObject to test value calculations, tension mechanics, and walk-away logic
    /// </summary>
    public class ContractNegotiationTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [Tooltip("Enable automatic testing on Start")]
        [SerializeField] private bool runTestOnStart = false;

        [Tooltip("Test hero for negotiation")]
        [SerializeField] private HeroData testHero;

        [Header("Test Offer Parameters")]
        [SerializeField] private float testSigningBonus = 50f;
        [SerializeField] private float testDailySalary = 15f;
        [SerializeField] private float testLootCutPercentage = 25f;
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
            Debug.Log("STARTING CONTRACT NEGOTIATION TEST");
            Debug.Log("========================================\n");

            // Test 1: Hero Information
            TestHeroInformation();

            // Test 2: Expected Value Calculation
            TestExpectedValue();

            // Test 3: Ideal Offer Calculation
            TestIdealOffer();

            // Test 4: Custom Offer Evaluation
            TestCustomOffer();

            // Test 5: Starting Tension
            TestStartingTension();

            // Test 6: Tension Delta Calculation
            TestTensionDelta();

            // Test 7: Walk-Away Simulation
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
            sb.AppendLine("\nCore Stats:");
            sb.AppendLine($"  Prowess: {testHero.prowess}");
            sb.AppendLine($"  Charisma: {testHero.charisma}");
            sb.AppendLine($"  Vitality: {testHero.maxVitality}");
            sb.AppendLine($"  Greed: {testHero.greed}");
            sb.AppendLine($"\nTrust Level: {testHero.trustLevel:F0}%");
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

            float vexp = negotiationManager.CalculateHeroExpectedValue(testHero);

            float coreStats = testHero.prowess + testHero.charisma + testHero.maxVitality;
            float baseValue = coreStats * 2f;

            sb.AppendLine($"Core Stats Sum: {coreStats:F1}");
            sb.AppendLine($"Base Value (Core × 2): {baseValue:F1}");
            sb.AppendLine($"Lifecycle Stage: {testHero.lifeStage}");
            sb.AppendLine($"Greed Premium: {testHero.greed}%");
            sb.AppendLine($"\n>>> TOTAL Vexp: {vexp:F1} gold <<<");

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Test 3: Calculate ideal offer where Voff = Vexp
        /// </summary>
        [ContextMenu("Test 3: Ideal Offer")]
        private void TestIdealOffer()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n--- TEST 3: IDEAL OFFER (Voff = Vexp) ---");

            ContractOffer idealOffer = negotiationManager.CalculateIdealOffer(testHero, testContractLength);
            float vexp = negotiationManager.CalculateHeroExpectedValue(testHero);
            float voff = negotiationManager.CalculateOfferValue(testHero, idealOffer);

            sb.AppendLine($"Hero Vexp: {vexp:F1} gold");
            sb.AppendLine("\nIdeal Offer Components:");
            sb.AppendLine($"  Signing Bonus: {idealOffer.signingBonus:F1} gold");
            sb.AppendLine($"  Daily Salary: {idealOffer.dailySalary:F1} gold/turn");
            sb.AppendLine($"  Loot Cut: {idealOffer.lootCutPercentage:F1}%");
            sb.AppendLine($"  Contract Length: {idealOffer.contractLengthYears} years");
            sb.AppendLine($"\n>>> TOTAL Voff: {voff:F1} gold <<<");
            sb.AppendLine($"Difference: {(voff - vexp):+0.0;-0.0} gold ({((voff / vexp - 1f) * 100f):+0.0;-0.0}%)");

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Test 4: Evaluate custom offer
        /// </summary>
        [ContextMenu("Test 4: Custom Offer")]
        private void TestCustomOffer()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n--- TEST 4: CUSTOM OFFER EVALUATION ---");

            ContractOffer customOffer = new ContractOffer(
                testSigningBonus,
                testDailySalary,
                testLootCutPercentage,
                testContractLength
            );

            float vexp = negotiationManager.CalculateHeroExpectedValue(testHero);
            float voff = negotiationManager.CalculateOfferValue(testHero, customOffer);

            sb.AppendLine($"Hero Vexp: {vexp:F1} gold");
            sb.AppendLine("\nCustom Offer Components:");
            sb.AppendLine($"  Signing Bonus: {customOffer.signingBonus:F1} gold");
            sb.AppendLine($"  Daily Salary: {customOffer.dailySalary:F1} gold/turn");
            sb.AppendLine($"  Loot Cut: {customOffer.lootCutPercentage:F1}%");
            sb.AppendLine($"  Contract Length: {customOffer.contractLengthYears} years");
            sb.AppendLine($"\n>>> TOTAL Voff: {voff:F1} gold <<<");

            float difference = voff - vexp;
            float percentageDiff = (difference / vexp) * 100f;

            sb.AppendLine($"\nValue Difference: {difference:+0.0;-0.0} gold ({percentageDiff:+0.0;-0.0}%)");

            if (difference < 0)
            {
                sb.AppendLine("Status: LOWBALL - Hero will be unhappy!");
            }
            else if (difference > vexp * 0.2f)
            {
                sb.AppendLine("Status: OVERPAYING - Player loses profit margin!");
            }
            else
            {
                sb.AppendLine("Status: FAIR - Good negotiation range");
            }

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Test 5: Calculate starting tension from trust
        /// </summary>
        [ContextMenu("Test 5: Starting Tension")]
        private void TestStartingTension()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n--- TEST 5: STARTING TENSION ---");

            sb.AppendLine("Testing different trust levels:\n");

            float[] trustLevels = { 100f, 75f, 50f, 25f, 0f };
            foreach (float trust in trustLevels)
            {
                float tension = negotiationManager.CalculateStartingTension(trust);
                sb.AppendLine($"Trust {trust:F0}% => Starting Tension: {tension:F1}%");
            }

            sb.AppendLine($"\nCurrent Hero ({testHero.heroName}):");
            sb.AppendLine($"Trust: {testHero.trustLevel:F0}%");
            float currentTension = negotiationManager.CalculateStartingTension(testHero.trustLevel);
            sb.AppendLine($"Starting Tension: {currentTension:F1}%");

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Test 6: Calculate tension delta from different offers
        /// </summary>
        [ContextMenu("Test 6: Tension Delta")]
        private void TestTensionDelta()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n--- TEST 6: TENSION DELTA ---");

            ContractOffer idealOffer = negotiationManager.CalculateIdealOffer(testHero, testContractLength);
            ContractOffer lowballOffer = new ContractOffer(
                idealOffer.signingBonus * 0.5f,
                idealOffer.dailySalary * 0.7f,
                idealOffer.lootCutPercentage * 0.7f,
                testContractLength
            );
            ContractOffer generousOffer = new ContractOffer(
                idealOffer.signingBonus * 1.5f,
                idealOffer.dailySalary * 1.2f,
                idealOffer.lootCutPercentage * 1.2f,
                testContractLength
            );

            float currentTension = 0f;

            sb.AppendLine("Offer Scenarios:\n");

            // Ideal offer
            float idealDelta = negotiationManager.CalculateTensionDelta(testHero, idealOffer, currentTension);
            sb.AppendLine($"1. IDEAL OFFER: Tension Delta = {idealDelta:+0.0;-0.0}%");

            // Lowball offer
            float lowballDelta = negotiationManager.CalculateTensionDelta(testHero, lowballOffer, currentTension);
            sb.AppendLine($"2. LOWBALL OFFER (70% value): Tension Delta = {lowballDelta:+0.0;-0.0}%");

            // Generous offer
            float generousDelta = negotiationManager.CalculateTensionDelta(testHero, generousOffer, currentTension);
            sb.AppendLine($"3. GENEROUS OFFER (120% value): Tension Delta = {generousDelta:+0.0;-0.0}%");

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
        /// Test 7: Simulate walk-away mechanics
        /// </summary>
        [ContextMenu("Test 7: Walk-Away Mechanics")]
        private void TestWalkAwayMechanics()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\n--- TEST 7: WALK-AWAY MECHANICS ---");

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
                sb.AppendLine($"  Turn {turn} (+ {turnsSince} turns): {(canRecruit ? "AVAILABLE" : "LOCKED")}");
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

            float vexp = negotiationManager.CalculateHeroExpectedValue(testHero);
            ContractOffer ideal = negotiationManager.CalculateIdealOffer(testHero, 2);

            sb.AppendLine($"\nExpected Value (Vexp): {vexp:F1} gold");
            sb.AppendLine($"Ideal Offer: {ideal.dailySalary:F1}g/turn, {ideal.lootCutPercentage:F1}% loot");
            sb.AppendLine($"Starting Tension: {negotiationManager.CalculateStartingTension(testHero.trustLevel):F1}%");

            Debug.Log(sb.ToString());
        }
    }
}
