using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using OneShotSupport.Core;
using OneShotSupport.Data;
using OneShotSupport.UI.Screens;
using OneShotSupport.UI.Components;

namespace OneShotSupport.UI
{
    /// <summary>
    /// Main UI manager that coordinates all UI screens
    /// Integrates with GameManager to show/hide appropriate screens
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Screens")]
        public DayStartScreen dayStartScreen;
        public RestockScreen restockScreen;
        public ConsultationScreen consultationScreen;
        public DayEndScreen dayEndScreen;
        public GameOverScreen gameOverScreen;

        [Header("Persistent UI")]
        public ReputationBar reputationBar;
        public DayCounter dayCounter;
        public GoldDisplay goldDisplay;
        public Components.FameDisplay fameDisplay;
        public Components.TrustMeter trustMeter;
        public Button mainMenuButton;

        [Header("Settings")]
        public bool autoHideScreens = true;
        public string mainMenuSceneName = "MainMenuScene";

        private GameManager gameManager;

        private void Start()
        {
            // Get GameManager
            gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("UIManager: GameManager not found!");
                return;
            }

            // Subscribe to game events
            gameManager.OnStateChanged += HandleStateChanged;
            gameManager.OnSeasonStarted += HandleSeasonStarted;
            gameManager.OnSeasonChanged += HandleSeasonChanged;
            gameManager.OnYearChanged += HandleYearChanged;
            gameManager.OnHeroReady += HandleHeroReady;
            gameManager.OnDayEnded += HandleDayEnded;
            gameManager.OnGameOver += HandleGameOver;

            // Subscribe to reputation changes
            if (gameManager.Reputation != null)
            {
                gameManager.Reputation.OnReputationChanged += HandleReputationChanged;
            }

            // Subscribe to propaganda events (Fame & Trust)
            gameManager.OnFameChanged += HandleFameChanged;
            gameManager.OnTrustChanged += HandleTrustChanged;
            gameManager.OnFameMilestoneReached += HandleFameMilestoneReached;
            gameManager.OnTrustThresholdCrossed += HandleTrustThresholdCrossed;

            // Subscribe to screen events
            if (dayStartScreen != null)
            {
                dayStartScreen.OnContinueClicked += HandleDayStartContinue;
            }

            if (restockScreen != null)
            {
                restockScreen.OnCratesPurchased += HandleCratesPurchased;
            }

            // Setup main menu button
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }

            // Initialize UI
            InitializeUI();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (gameManager != null)
            {
                gameManager.OnStateChanged -= HandleStateChanged;
                gameManager.OnSeasonStarted -= HandleSeasonStarted;
                gameManager.OnSeasonChanged -= HandleSeasonChanged;
                gameManager.OnYearChanged -= HandleYearChanged;
                gameManager.OnHeroReady -= HandleHeroReady;
                gameManager.OnDayEnded -= HandleDayEnded;
                gameManager.OnGameOver -= HandleGameOver;

                if (gameManager.Reputation != null)
                {
                    gameManager.Reputation.OnReputationChanged -= HandleReputationChanged;
                }

                // Unsubscribe from propaganda events
                gameManager.OnFameChanged -= HandleFameChanged;
                gameManager.OnTrustChanged -= HandleTrustChanged;
                gameManager.OnFameMilestoneReached -= HandleFameMilestoneReached;
                gameManager.OnTrustThresholdCrossed -= HandleTrustThresholdCrossed;
            }

            // Unsubscribe from screen events
            if (dayStartScreen != null)
            {
                dayStartScreen.OnContinueClicked -= HandleDayStartContinue;
            }

            if (restockScreen != null)
            {
                restockScreen.OnCratesPurchased -= HandleCratesPurchased;
            }
        }

        /// <summary>
        /// Initialize UI state
        /// </summary>
        private void InitializeUI()
        {
            // Hide all screens initially
            if (autoHideScreens)
            {
                HideAllScreens();
            }

            // Update persistent UI
            if (reputationBar != null && gameManager.Reputation != null)
            {
                reputationBar.UpdateReputation(gameManager.Reputation.CurrentReputation);
            }

            if (dayCounter != null && gameManager.Calendar != null)
            {
                // Use new seasonal system
                dayCounter.UpdateSeason(gameManager.Calendar.CurrentSeason, gameManager.Calendar.CurrentYear);
            }

            if (fameDisplay != null && gameManager.Propaganda != null)
            {
                fameDisplay.UpdateFame(gameManager.Propaganda.CurrentFame);
            }

            if (trustMeter != null && gameManager.Propaganda != null)
            {
                trustMeter.UpdateTrust(gameManager.Propaganda.CurrentTrust);
            }
        }

        /// <summary>
        /// Handle game state changes
        /// </summary>
        private void HandleStateChanged(GameState newState)
        {
            Debug.Log($"[UIManager] State changed to: {newState}");

            if (!autoHideScreens) return;

            // Show/hide appropriate screens based on state
            switch (newState)
            {
                case GameState.DayStart:
                    // Day start hint is handled by OnDayHintGenerated event
                    break;

                case GameState.Restock:
                    ShowRestockScreen();
                    break;

                case GameState.Consultation:
                    ShowConsultationScreen();
                    break;

                case GameState.DayEnd:
                    ShowDayEndScreen();
                    break;

                case GameState.GameOver:
                    ShowGameOverScreen();
                    break;
            }

            // Update day counter with seasonal data
            if (dayCounter != null && gameManager.Calendar != null)
            {
                dayCounter.UpdateSeason(gameManager.Calendar.CurrentSeason, gameManager.Calendar.CurrentYear);
            }
        }

        /// <summary>
        /// Handle hero ready for consultation
        /// </summary>
        private void HandleHeroReady(HeroResult heroResult)
        {
            if (consultationScreen != null && gameManager.CurrentDay != null)
            {
                consultationScreen.SetupConsultation(
                    heroResult,
                    gameManager.CurrentDay.availableItems
                );
            }
        }

        /// <summary>
        /// Handle day end
        /// </summary>
        private void HandleDayEnded(List<HeroResult> results)
        {
            if (dayEndScreen != null && gameManager.Reputation != null)
            {
                dayEndScreen.DisplayResults(results, gameManager.Reputation.CurrentReputation);
            }
        }

        /// <summary>
        /// Handle game over
        /// </summary>
        private void HandleGameOver()
        {
            if (gameOverScreen != null && gameManager.Reputation != null && gameManager.Calendar != null)
            {
                int turnsSurvived = gameManager.Calendar.CurrentTurn - 1;
                gameOverScreen.DisplayGameOver(turnsSurvived, gameManager.Reputation.CurrentReputation);
            }
        }

        /// <summary>
        /// Handle reputation changes
        /// </summary>
        private void HandleReputationChanged(int newReputation)
        {
            if (reputationBar != null)
            {
                reputationBar.UpdateReputation(newReputation);
            }
        }

        /// <summary>
        /// Handle crates purchased from restock screen
        /// </summary>
        private void HandleCratesPurchased(List<ScriptableObjects.ItemData> purchasedItems)
        {
            // Notify GameManager about purchased items
            if (gameManager != null)
            {
                gameManager.CompleteCratePurchase(purchasedItems);
            }
        }

        /// <summary>
        /// Handle season started event - show the season start screen
        /// </summary>
        private void HandleSeasonStarted(Season season, int year)
        {
            ShowDayStartScreen(season, year);
        }

        /// <summary>
        /// Handle season changed
        /// </summary>
        private void HandleSeasonChanged(Season newSeason, int year)
        {
            Debug.Log($"[UIManager] Season changed to {newSeason}, Year {year}");

            // Update day counter to show season
            if (dayCounter != null && gameManager?.Calendar != null)
            {
                dayCounter.UpdateSeason(newSeason, year);
            }
        }

        /// <summary>
        /// Handle year changed
        /// </summary>
        private void HandleYearChanged(int newYear)
        {
            Debug.Log($"[UIManager] *** NEW YEAR: {newYear} ***");
            // Could trigger special UI effects, celebrations, etc.
        }

        /// <summary>
        /// Handle day start continue button clicked
        /// </summary>
        private void HandleDayStartContinue()
        {
            // Notify GameManager to transition to Restock state
            if (gameManager != null)
            {
                gameManager.StartRestock();
            }
        }

        /// <summary>
        /// Handle fame changed
        /// </summary>
        private void HandleFameChanged(int newFame)
        {
            if (fameDisplay != null)
            {
                fameDisplay.UpdateFame(newFame);
            }
        }

        /// <summary>
        /// Handle trust changed
        /// </summary>
        private void HandleTrustChanged(int newTrust)
        {
            if (trustMeter != null)
            {
                trustMeter.UpdateTrust(newTrust);
            }
        }

        /// <summary>
        /// Handle fame milestone reached
        /// </summary>
        private void HandleFameMilestoneReached(FameMilestone milestone)
        {
            Debug.Log($"[UIManager] Fame Milestone Reached: {milestone}");
            // TODO: Show celebration/notification UI when milestone is reached
        }

        /// <summary>
        /// Handle trust threshold crossed
        /// </summary>
        private void HandleTrustThresholdCrossed(TrustThreshold threshold)
        {
            Debug.Log($"[UIManager] Trust Threshold Crossed: {threshold}");
            // TODO: Show notification UI when trust threshold is crossed
        }

        // === Screen Management ===

        private void HideAllScreens()
        {
            if (dayStartScreen != null)
                dayStartScreen.gameObject.SetActive(false);

            if (restockScreen != null)
                restockScreen.gameObject.SetActive(false);

            if (consultationScreen != null)
                consultationScreen.gameObject.SetActive(false);

            if (dayEndScreen != null)
                dayEndScreen.gameObject.SetActive(false);

            if (gameOverScreen != null)
                gameOverScreen.gameObject.SetActive(false);
        }

        private void ShowDayStartScreen(Season season, int year)
        {
            HideAllScreens();

            if (dayStartScreen != null)
            {
                dayStartScreen.Setup(season, year);
            }
        }

        private void ShowRestockScreen()
        {
            HideAllScreens();

            if (restockScreen != null)
            {
                restockScreen.Setup();
                restockScreen.gameObject.SetActive(true);
            }
        }

        private void ShowConsultationScreen()
        {
            HideAllScreens();

            if (consultationScreen != null)
                consultationScreen.gameObject.SetActive(true);
        }

        private void ShowDayEndScreen()
        {
            HideAllScreens();

            if (dayEndScreen != null)
                dayEndScreen.gameObject.SetActive(true);
        }

        private void ShowGameOverScreen()
        {
            HideAllScreens();

            if (gameOverScreen != null)
                gameOverScreen.gameObject.SetActive(true);
        }

        /// <summary>
        /// Return to main menu
        /// </summary>
        private void OnMainMenuClicked()
        {
            Debug.Log("[UIManager] Returning to main menu...");
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
