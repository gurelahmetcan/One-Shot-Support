using UnityEngine;
using OneShotSupport.Core;

namespace OneShotSupport.Tutorial
{
    /// <summary>
    /// Diagnostic script to check tutorial setup
    /// Attach this to any GameObject and it will run checks on start
    /// </summary>
    public class TutorialDiagnostic : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("=== TUTORIAL DIAGNOSTIC START ===");

            // Check if TutorialManager exists in scene
            var tutorialManager = FindObjectOfType<TutorialManager>();
            if (tutorialManager == null)
            {
                Debug.LogError("❌ No TutorialManager found in scene! Add TutorialManager to GameManager GameObject.");
                return;
            }
            Debug.Log("✓ TutorialManager found in scene");

            // Check if TutorialManager has data
            var tutorialData = tutorialManager.GetTutorialData();
            if (tutorialData == null)
            {
                Debug.LogError("❌ TutorialManager has no TutorialData assigned! Assign it in the Inspector.");
                return;
            }
            Debug.Log("✓ TutorialData is assigned");

            // Check if data is valid
            if (!tutorialData.IsValid())
            {
                Debug.LogError("❌ TutorialData is invalid! Make sure hero, monster, and items are all assigned.");
                return;
            }
            Debug.Log("✓ TutorialData is valid (hero, monster, items assigned)");

            // Check if GameManager has reference to TutorialManager
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("❌ GameManager not found!");
                return;
            }
            Debug.Log("✓ GameManager found");

            // Use reflection to check if tutorialManager field is assigned
            var field = typeof(GameManager).GetField("tutorialManager");
            if (field != null)
            {
                var value = field.GetValue(gameManager);
                if (value == null)
                {
                    Debug.LogError("❌ GameManager.tutorialManager is NOT assigned! Drag TutorialManager to GameManager in Inspector.");
                    return;
                }
                Debug.Log("✓ GameManager.tutorialManager is assigned");
            }

            // Check current day
            int currentDay = gameManager.CurrentDayNumber;
            Debug.Log($"✓ Current Day: {currentDay}");

            if (currentDay == 1)
            {
                Debug.Log("✓ Day 1 - Tutorial should run!");

                if (tutorialManager.IsTutorialActive())
                {
                    Debug.Log($"✓ Tutorial is ACTIVE - Current step: {tutorialManager.GetCurrentStep()}");
                }
                else
                {
                    Debug.LogWarning("⚠ Tutorial is NOT active even though it's Day 1");
                }
            }
            else
            {
                Debug.Log($"⚠ Not Day 1 (Day {currentDay}) - Tutorial won't run. Start a NEW GAME to see tutorial.");
            }

            Debug.Log("=== TUTORIAL DIAGNOSTIC END ===");
        }
    }
}
