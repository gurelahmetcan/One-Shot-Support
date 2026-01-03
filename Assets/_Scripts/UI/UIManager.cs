using UnityEngine;
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
        public ConsultationScreen consultationScreen;
        public DayEndScreen dayEndScreen;
        public GameOverScreen gameOverScreen;

        [Header("Persistent UI")]
        public ReputationBar reputationBar;
        public DayCounter dayCounter;

        [Header("Settings")]
        public bool autoHideScreens = true;

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
            gameManager.OnHeroReady += HandleHeroReady;
            gameManager.OnDayEnded += HandleDayEnded;
            gameManager.OnGameOver += HandleGameOver;

            // Subscribe to reputation changes
            if (gameManager.Reputation != null)
            {
                gameManager.Reputation.OnReputationChanged += HandleReputationChanged;
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
                gameManager.OnHeroReady -= HandleHeroReady;
                gameManager.OnDayEnded -= HandleDayEnded;
                gameManager.OnGameOver -= HandleGameOver;

                if (gameManager.Reputation != null)
                {
                    gameManager.Reputation.OnReputationChanged -= HandleReputationChanged;
                }
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

            if (dayCounter != null)
            {
                dayCounter.UpdateDay(gameManager.CurrentDayNumber);
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
                case GameState.Consultation:
                    ShowConsultationScreen();
                    break;

                case GameState.DayEnd:
                    ShowDayEndScreen();
                    break;

                case GameState.GameOver:
                    ShowGameOverScreen();
                    break;

                default:
                    // For DayStart and Restock, keep current screen
                    break;
            }

            // Update day counter
            if (dayCounter != null)
            {
                dayCounter.UpdateDay(gameManager.CurrentDayNumber);
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
            if (gameOverScreen != null && gameManager.Reputation != null)
            {
                int daysSurvived = gameManager.CurrentDayNumber - 1;
                gameOverScreen.DisplayGameOver(daysSurvived, gameManager.Reputation.CurrentReputation);
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

        // === Screen Management ===

        private void HideAllScreens()
        {
            if (consultationScreen != null)
                consultationScreen.gameObject.SetActive(false);

            if (dayEndScreen != null)
                dayEndScreen.gameObject.SetActive(false);

            if (gameOverScreen != null)
                gameOverScreen.gameObject.SetActive(false);
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
    }
}
