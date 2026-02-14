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
        public Screens.VillageHubScreen villageHubScreen;
        public Screens.MissionBoardScreen missionBoardScreen;
        public Screens.TavernScreen tavernScreen;
        public Screens.BarracksScreen barracksScreen;
        public Screens.EconomyScreen economyScreen;
        public Screens.PreparationPhaseScreen preparationPhaseScreen;
        public RestockScreen restockScreen;
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
            gameManager.OnMissionsGenerated += HandleMissionsGenerated;
            gameManager.OnMissionSelected += HandleMissionSelected;
            gameManager.OnTavernHeroesGenerated += HandleTavernHeroesGenerated;
            gameManager.OnHeroRecruited += HandleHeroRecruited;
            gameManager.OnBarracksOpened += HandleBarracksOpened;
            gameManager.OnPreparationPhaseStarted += HandlePreparationPhaseStarted;
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

            if (villageHubScreen != null)
            {
                villageHubScreen.OnTavernClicked += () => gameManager.OpenTavern();
                villageHubScreen.OnMissionBoardClicked += () => gameManager.OpenMissionBoard();
                villageHubScreen.OnBarracksClicked += () => gameManager.OpenBarracks();
                villageHubScreen.OnEconomyClicked += () => gameManager.OpenEconomy();
                villageHubScreen.OnPreparationClicked += () => gameManager.OpenPreparationPhase();
            }

            if (missionBoardScreen != null)
            {
                missionBoardScreen.OnMissionSelected += HandleMissionBoardSelection;
                missionBoardScreen.OnBackClicked += () => gameManager.LeaveMissionBoard();
            }

            if (tavernScreen != null)
            {
                tavernScreen.OnHeroRecruited += (hero, offer) => gameManager.RecruitHero(hero, offer);
                tavernScreen.OnHeroWalkedAway += (hero) => gameManager.HeroWalkedAway(hero);
                tavernScreen.OnBackClicked += () => gameManager.LeaveTavern();
            }

            if (barracksScreen != null)
            {
                barracksScreen.OnBackClicked += () => gameManager.LeaveBarracks();
            }

            if (economyScreen != null)
            {
                economyScreen.OnBackClicked += () => gameManager.LeaveEconomy();
            }

            if (preparationPhaseScreen != null)
            {
                preparationPhaseScreen.OnDispatchCompleted += HandlePreparationDispatchCompleted;
                preparationPhaseScreen.OnBackClicked += () => gameManager.LeavePreparationPhase();
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
                gameManager.OnMissionsGenerated -= HandleMissionsGenerated;
                gameManager.OnMissionSelected -= HandleMissionSelected;
                gameManager.OnTavernHeroesGenerated -= HandleTavernHeroesGenerated;
                gameManager.OnHeroRecruited -= HandleHeroRecruited;
                gameManager.OnBarracksOpened -= HandleBarracksOpened;
                gameManager.OnPreparationPhaseStarted -= HandlePreparationPhaseStarted;
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

            if (missionBoardScreen != null)
            {
                missionBoardScreen.OnMissionSelected -= HandleMissionBoardSelection;
                missionBoardScreen.OnBackClicked -= HandleMissionBoardBack;
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
                    // Day start is handled by OnSeasonStarted event
                    break;

                case GameState.VillageHub:
                    ShowVillageHubScreen();
                    break;

                case GameState.MissionBoard:
                    // Mission board will be shown when missions are generated
                    break;

                case GameState.Tavern:
                    // Tavern will be shown when heroes are generated
                    break;

                case GameState.Barracks:
                    // Barracks will be shown when OnBarracksOpened event fires
                    break;

                case GameState.Economy:
                    ShowEconomyScreen();
                    break;

                case GameState.PreparationPhase:
                    // Preparation phase will be shown when OnPreparationPhaseStarted event fires
                    break;

                case GameState.Restock:
                    ShowRestockScreen();
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
            // Notify GameManager to transition to Village Hub
            if (gameManager != null)
            {
                gameManager.StartVillageHub();
            }
        }

        /// <summary>
        /// Handle tavern heroes generated
        /// </summary>
        private void HandleTavernHeroesGenerated(List<ScriptableObjects.HeroData> heroes)
        {
            Debug.Log($"[UIManager] Tavern heroes generated: {heroes.Count}");
            ShowTavernScreen(heroes);
        }

        /// <summary>
        /// Handle hero recruited
        /// </summary>
        private void HandleHeroRecruited(ScriptableObjects.HeroData hero, Core.ContractOffer offer)
        {
            Debug.Log($"[UIManager] Hero recruited: {hero.heroName} (Contract: {offer.signingBonus}g signing + {offer.dailySalary}g/turn × {offer.contractLengthYears}yr)");
            // Refresh tavern screen to show updated hero list
            if (tavernScreen != null && tavernScreen.gameObject.activeSelf)
            {
                tavernScreen.Refresh();
            }

            // Refresh barracks screen if it's open
            if (barracksScreen != null && barracksScreen.gameObject.activeSelf)
            {
                barracksScreen.Refresh();
            }
        }

        /// <summary>
        /// Handle barracks opened
        /// </summary>
        private void HandleBarracksOpened(List<ScriptableObjects.HeroData> heroes, int maxCapacity)
        {
            Debug.Log($"[UIManager] Barracks opened: {heroes.Count}/{maxCapacity} heroes");
            ShowBarracksScreen(heroes, maxCapacity);
        }

        /// <summary>
        /// Handle preparation phase started
        /// </summary>
        private void HandlePreparationPhaseStarted(ScriptableObjects.MissionData mission, List<ScriptableObjects.HeroData> heroes)
        {
            Debug.Log($"[UIManager] Preparation phase started: {mission.missionName} with {heroes.Count} available heroes");
            ShowPreparationPhaseScreen(mission, heroes);
        }

        /// <summary>
        /// Handle dispatch completed with resolution result
        /// </summary>
        private void HandlePreparationDispatchCompleted(Core.MissionResolutionResult result)
        {
            if (preparationPhaseScreen != null && gameManager != null)
            {
                var assignedHeroes = preparationPhaseScreen.GetAssignedHeroes();
                gameManager.CompleteMissionDispatch(result, assignedHeroes);
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

        /// <summary>
        /// Handle missions generated
        /// </summary>
        private void HandleMissionsGenerated(List<ScriptableObjects.MissionData> missions)
        {
            Debug.Log($"[UIManager] Missions generated: {missions.Count}");
            ShowMissionBoardScreen(missions);
        }

        /// <summary>
        /// Handle mission selected from mission board
        /// </summary>
        private void HandleMissionBoardSelection(ScriptableObjects.MissionData mission)
        {
            Debug.Log($"[UIManager] Mission selected from UI: {mission.missionName}");
            // Notify game manager
            if (gameManager != null)
            {
                gameManager.SelectMission(mission);
            }
        }

        /// <summary>
        /// Handle mission board back button (currently unused, but could allow skipping mission selection)
        /// </summary>
        private void HandleMissionBoardBack()
        {
            Debug.Log($"[UIManager] Mission board back clicked");
            // For now, do nothing - mission selection is mandatory
        }

        /// <summary>
        /// Handle mission selected (from game manager)
        /// </summary>
        private void HandleMissionSelected(ScriptableObjects.MissionData mission)
        {
            Debug.Log($"[UIManager] Mission confirmed by GameManager: {mission.missionName}");
            // Mission selected, game will transition to Restock
        }

        // === Screen Management ===

        private void HideAllScreens()
        {
            if (dayStartScreen != null)
                dayStartScreen.gameObject.SetActive(false);

            if (villageHubScreen != null)
                villageHubScreen.gameObject.SetActive(false);

            if (missionBoardScreen != null)
                missionBoardScreen.gameObject.SetActive(false);

            if (tavernScreen != null)
                tavernScreen.gameObject.SetActive(false);

            if (barracksScreen != null)
                barracksScreen.gameObject.SetActive(false);

            if (economyScreen != null)
                economyScreen.gameObject.SetActive(false);

            if (preparationPhaseScreen != null)
                preparationPhaseScreen.gameObject.SetActive(false);

            if (restockScreen != null)
                restockScreen.gameObject.SetActive(false);

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

        private void ShowVillageHubScreen()
        {
            HideAllScreens();

            if (villageHubScreen != null)
            {
                villageHubScreen.Setup();
            }
        }

        private void ShowMissionBoardScreen(List<ScriptableObjects.MissionData> missions)
        {
            HideAllScreens();

            if (missionBoardScreen != null)
            {
                missionBoardScreen.Setup(missions);
            }
        }

        private void ShowTavernScreen(List<ScriptableObjects.HeroData> heroes)
        {
            HideAllScreens();

            if (tavernScreen != null)
            {
                tavernScreen.Setup(heroes);
            }
        }

        private void ShowBarracksScreen(List<ScriptableObjects.HeroData> heroes, int maxCapacity)
        {
            HideAllScreens();

            if (barracksScreen != null)
            {
                barracksScreen.Setup(heroes, maxCapacity);
            }
        }

        private void ShowEconomyScreen()
        {
            HideAllScreens();

            if (economyScreen != null && gameManager != null)
            {
                List<ScriptableObjects.HeroData> recruitedHeroes = gameManager.GetRecruitedHeroes();
                int currentGold = gameManager.Gold != null ? gameManager.Gold.CurrentGold : 0;
                economyScreen.Setup(recruitedHeroes, currentGold);
            }
        }

        private void ShowPreparationPhaseScreen(ScriptableObjects.MissionData mission, List<ScriptableObjects.HeroData> heroes)
        {
            HideAllScreens();

            if (preparationPhaseScreen != null)
            {
                preparationPhaseScreen.Setup(mission, heroes);
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
