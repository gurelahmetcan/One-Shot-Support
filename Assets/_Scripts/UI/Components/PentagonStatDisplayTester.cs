using UnityEngine;
using TMPro;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Test script for PentagonStatDisplay
    /// Allows testing different stat configurations and overlays
    /// Add this to a GameObject with PentagonStatDisplay to test it
    /// </summary>
    public class PentagonStatDisplayTester : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The pentagon display to test")]
        public PentagonStatDisplay pentagonDisplay;

        [Header("Test Scenarios")]
        [Tooltip("Enable automatic scenario cycling")]
        public bool autoCycleScenarios = false;

        [Tooltip("Seconds between scenario changes")]
        public float cycleInterval = 3f;

        [Header("Test Stats - Main Pentagon")]
        [Range(0, 60)]
        public int testMight = 30;
        [Range(0, 60)]
        public int testCharm = 30;
        [Range(0, 60)]
        public int testWit = 30;
        [Range(0, 60)]
        public int testAgility = 30;
        [Range(0, 60)]
        public int testFortitude = 30;

        [Header("Test Stats - Overlay Pentagon")]
        public bool testOverlay = false;
        [Range(0, 60)]
        public int testOverlayMight = 20;
        [Range(0, 60)]
        public int testOverlayCharm = 20;
        [Range(0, 60)]
        public int testOverlayWit = 20;
        [Range(0, 60)]
        public int testOverlayAgility = 20;
        [Range(0, 60)]
        public int testOverlayFortitude = 20;

        [Header("UI Labels (Optional)")]
        [Tooltip("Text labels to show stat values")]
        public TextMeshProUGUI mightLabel;
        public TextMeshProUGUI charmLabel;
        public TextMeshProUGUI witLabel;
        public TextMeshProUGUI agilityLabel;
        public TextMeshProUGUI fortitudeLabel;

        private float cycleTimer = 0f;
        private int currentScenario = 0;

        private void Start()
        {
            if (pentagonDisplay == null)
            {
                pentagonDisplay = GetComponent<PentagonStatDisplay>();
            }

            UpdatePentagon();
        }

        private void Update()
        {
            if (autoCycleScenarios)
            {
                cycleTimer += Time.deltaTime;
                if (cycleTimer >= cycleInterval)
                {
                    cycleTimer = 0f;
                    CycleToNextScenario();
                }
            }
        }

        /// <summary>
        /// Apply current test values to pentagon
        /// </summary>
        [ContextMenu("Update Pentagon")]
        public void UpdatePentagon()
        {
            if (pentagonDisplay == null) return;

            pentagonDisplay.SetStats(testMight, testCharm, testWit, testAgility, testFortitude);

            if (testOverlay)
            {
                pentagonDisplay.SetOverlayStats(testOverlayMight, testOverlayCharm, testOverlayWit,
                                              testOverlayAgility, testOverlayFortitude);
            }
            else
            {
                pentagonDisplay.ClearOverlay();
            }

            UpdateLabels();
        }

        /// <summary>
        /// Update stat labels if they exist
        /// </summary>
        private void UpdateLabels()
        {
            if (mightLabel != null) mightLabel.text = $"Might: {testMight}";
            if (charmLabel != null) charmLabel.text = $"Charm: {testCharm}";
            if (witLabel != null) witLabel.text = $"Wit: {testWit}";
            if (agilityLabel != null) agilityLabel.text = $"Agility: {testAgility}";
            if (fortitudeLabel != null) fortitudeLabel.text = $"Fortitude: {testFortitude}";
        }

        /// <summary>
        /// Cycle through predefined test scenarios
        /// </summary>
        private void CycleToNextScenario()
        {
            currentScenario = (currentScenario + 1) % 6;

            switch (currentScenario)
            {
                case 0: // Balanced
                    LoadScenario("Balanced Hero", 30, 30, 30, 30, 30);
                    break;
                case 1: // Warrior (High Might/Fortitude)
                    LoadScenario("Warrior", 50, 15, 20, 25, 45);
                    break;
                case 2: // Rogue (High Agility/Wit)
                    LoadScenario("Rogue", 20, 25, 40, 50, 25);
                    break;
                case 3: // Diplomat (High Charm/Wit)
                    LoadScenario("Diplomat", 15, 50, 45, 20, 25);
                    break;
                case 4: // Tank (High Fortitude)
                    LoadScenario("Tank", 35, 10, 15, 20, 60);
                    break;
                case 5: // All-Rounder (Mixed)
                    LoadScenario("All-Rounder", 40, 35, 30, 25, 40);
                    break;
            }
        }

        /// <summary>
        /// Load a test scenario
        /// </summary>
        private void LoadScenario(string name, int m, int c, int w, int a, int f)
        {
            Debug.Log($"[PentagonTester] Loading scenario: {name}");
            testMight = m;
            testCharm = c;
            testWit = w;
            testAgility = a;
            testFortitude = f;
            UpdatePentagon();
        }

        // === QUICK TEST BUTTONS (callable from inspector) ===

        [ContextMenu("Test: Combat Mission (High Might/Fort)")]
        public void TestCombatMission()
        {
            testMight = 55;
            testCharm = 10;
            testWit = 15;
            testAgility = 20;
            testFortitude = 40;
            testOverlay = false;
            UpdatePentagon();
        }

        [ContextMenu("Test: Stealth Mission (High Agility/Wit)")]
        public void TestStealthMission()
        {
            testMight = 5;
            testCharm = 15;
            testWit = 30;
            testAgility = 40;
            testFortitude = 10;
            testOverlay = false;
            UpdatePentagon();
        }

        [ContextMenu("Test: Hero vs Mission Overlay")]
        public void TestHeroVsMissionOverlay()
        {
            // Mission requirements (base pentagon)
            testMight = 40;
            testCharm = 15;
            testWit = 20;
            testAgility = 15;
            testFortitude = 35;

            // Hero stats (overlay pentagon)
            testOverlay = true;
            testOverlayMight = 50; // Exceeds
            testOverlayCharm = 20; // Exceeds
            testOverlayWit = 15;   // Below
            testOverlayAgility = 30; // Exceeds
            testOverlayFortitude = 40; // Exceeds

            UpdatePentagon();
        }

        [ContextMenu("Test: Random Stats")]
        public void TestRandomStats()
        {
            testMight = Random.Range(10, 60);
            testCharm = Random.Range(10, 60);
            testWit = Random.Range(10, 60);
            testAgility = Random.Range(10, 60);
            testFortitude = Random.Range(10, 60);

            if (testOverlay)
            {
                testOverlayMight = Random.Range(10, 60);
                testOverlayCharm = Random.Range(10, 60);
                testOverlayWit = Random.Range(10, 60);
                testOverlayAgility = Random.Range(10, 60);
                testOverlayFortitude = Random.Range(10, 60);
            }

            UpdatePentagon();
        }

        [ContextMenu("Test: Clear Overlay")]
        public void TestClearOverlay()
        {
            testOverlay = false;
            UpdatePentagon();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-update when values change in inspector
            if (Application.isPlaying && pentagonDisplay != null)
            {
                UpdatePentagon();
            }
        }
#endif
    }
}
