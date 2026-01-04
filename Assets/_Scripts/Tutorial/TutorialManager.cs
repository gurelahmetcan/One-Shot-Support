using UnityEngine;
using System;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.Tutorial
{
    /// <summary>
    /// Tutorial step enumeration
    /// </summary>
    public enum TutorialStep
    {
        None,
        DayStartHint,           // Teach hint system
        ExamineMonster,         // Look at monster weakness
        CheckInventory,         // See available items
        DragItem,              // Drag matching item to slot
        CheckConfidence,        // Understand confidence meter
        UnderstandHero,         // Check hero stats
        SendHero,              // Send hero button
        Complete               // Tutorial finished
    }

    /// <summary>
    /// Manages tutorial flow and state
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("Tutorial Data")]
        [SerializeField] private TutorialData tutorialData;

        [Header("Tutorial UI")]
        [SerializeField] private TutorialUI tutorialUI;

        // Tutorial state
        private TutorialStep currentStep = TutorialStep.None;
        private bool isTutorialActive = false;

        // Events
        public event Action<TutorialStep> OnStepChanged;
        public event Action OnTutorialComplete;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// Check if tutorial should run (Day 1)
        /// </summary>
        public bool ShouldRunTutorial(int dayNumber)
        {
            return dayNumber == 1;
        }

        /// <summary>
        /// Start the tutorial
        /// </summary>
        public void StartTutorial()
        {
            if (tutorialData == null || !tutorialData.IsValid())
            {
                Debug.LogError("[TutorialManager] Tutorial data is invalid!");
                return;
            }

            isTutorialActive = true;
            currentStep = TutorialStep.None;

            Debug.Log("[TutorialManager] Tutorial started!");

            // Start with day start hint
            AdvanceToStep(TutorialStep.DayStartHint);
        }

        /// <summary>
        /// Advance to the next tutorial step
        /// </summary>
        public void AdvanceToStep(TutorialStep step)
        {
            currentStep = step;
            Debug.Log($"[TutorialManager] Advanced to step: {step}");

            OnStepChanged?.Invoke(step);

            // Show appropriate tutorial UI
            if (tutorialUI != null)
            {
                tutorialUI.ShowStepInstructions(step);
            }
        }

        /// <summary>
        /// Complete current step and move to next
        /// </summary>
        public void CompleteCurrentStep()
        {
            TutorialStep nextStep = GetNextStep(currentStep);

            if (nextStep == TutorialStep.Complete)
            {
                CompleteTutorial();
            }
            else
            {
                AdvanceToStep(nextStep);
            }
        }

        /// <summary>
        /// Get the next tutorial step
        /// </summary>
        private TutorialStep GetNextStep(TutorialStep current)
        {
            switch (current)
            {
                case TutorialStep.DayStartHint:
                    return TutorialStep.ExamineMonster;
                case TutorialStep.ExamineMonster:
                    return TutorialStep.CheckInventory;
                case TutorialStep.CheckInventory:
                    return TutorialStep.DragItem;
                case TutorialStep.DragItem:
                    return TutorialStep.CheckConfidence;
                case TutorialStep.CheckConfidence:
                    return TutorialStep.UnderstandHero;
                case TutorialStep.UnderstandHero:
                    return TutorialStep.SendHero;
                case TutorialStep.SendHero:
                    return TutorialStep.Complete;
                default:
                    return TutorialStep.Complete;
            }
        }

        /// <summary>
        /// Complete the tutorial
        /// </summary>
        private void CompleteTutorial()
        {
            isTutorialActive = false;
            currentStep = TutorialStep.Complete;

            Debug.Log("[TutorialManager] Tutorial complete!");

            if (tutorialUI != null)
            {
                tutorialUI.HideInstructions();
            }

            OnTutorialComplete?.Invoke();
        }

        /// <summary>
        /// Get tutorial data
        /// </summary>
        public TutorialData GetTutorialData()
        {
            return tutorialData;
        }

        /// <summary>
        /// Check if tutorial is currently active
        /// </summary>
        public bool IsTutorialActive()
        {
            return isTutorialActive;
        }

        /// <summary>
        /// Get current tutorial step
        /// </summary>
        public TutorialStep GetCurrentStep()
        {
            return currentStep;
        }

        /// <summary>
        /// Check if specific action is allowed in current tutorial step
        /// </summary>
        public bool IsActionAllowed(string actionName)
        {
            if (!isTutorialActive) return true;

            // Block actions based on current step
            switch (currentStep)
            {
                case TutorialStep.DayStartHint:
                    // Only allow continuing from day start
                    return actionName == "ContinueDayStart";

                case TutorialStep.ExamineMonster:
                case TutorialStep.CheckInventory:
                    // Block sending hero, allow dragging
                    return actionName != "SendHero";

                case TutorialStep.DragItem:
                    // Only allow dragging items
                    return actionName == "DragItem";

                case TutorialStep.CheckConfidence:
                case TutorialStep.UnderstandHero:
                    // Allow looking around but not sending
                    return actionName != "SendHero";

                case TutorialStep.SendHero:
                    // Only allow sending hero
                    return actionName == "SendHero";

                default:
                    return true;
            }
        }
    }
}
