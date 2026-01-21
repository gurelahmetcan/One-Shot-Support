using UnityEngine;
using System;
using System.Collections.Generic;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;
using OneShotSupport.Tutorial;
using Random = UnityEngine.Random;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Main game manager implementing the game loop state machine
    /// Controls the day/night cycle and overall game flow
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Configuration")]
        [Tooltip("Number of heroes per season/turn")]
        [Range(1, 5)]
        public int heroesPerTurn = 3;

        [Tooltip("Number of items to restock each season")]
        [Range(5, 10)]
        public int itemsPerTurn = 6;

        [Header("References")]
        [Tooltip("Item database for daily restocking")]
        public ItemDatabase itemDatabase;

        [Tooltip("Hero generator for procedural heroes")]
        public HeroGenerator heroGenerator;

        [Tooltip("Monster generator for procedural monsters")]
        public MonsterGenerator monsterGenerator;

        [Header("Managers")]
        [Tooltip("Gold manager for currency system")]
        public GoldManager goldManager;

        [Tooltip("Hero lifecycle manager for aging system")]
        public HeroLifecycleManager heroLifecycleManager;

        [Header("UI References (Internal)")]
        [Tooltip("Consultation screen reference for item recycling")]
        [HideInInspector]
        public UI.Screens.ConsultationScreen consultationScreen;

        // State machine
        private GameState currentState;
        private ReputationManager reputationManager;
        private DayData currentDay; // Note: Still called "DayData" for now, represents current turn/season
        private SeasonalCalendar seasonalCalendar;

        // Events for UI
        public event Action<GameState> OnStateChanged;
        public event Action<DayData> OnDayStarted; // Legacy support
        public event Action<Season, int> OnSeasonStarted; // (season, year) - replaces day start
        public event Action<HeroResult> OnHeroReady;
        public event Action<HeroResult> OnHeroEquipped;
        public event Action<List<HeroResult>> OnDayEnded; // Called OnSeasonEnded conceptually
        public event Action OnGameOver;
        public event Action<Season, int> OnSeasonChanged; // New event for season changes
        public event Action<int> OnYearChanged; // New event for year changes

        // Singleton for easy access (game jam pattern)
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            //DontDestroyOnLoad(gameObject);

            // Initialize systems
            reputationManager = new ReputationManager();
            seasonalCalendar = new SeasonalCalendar();
        }

        private void Start()
        {
            // Start the game
            reputationManager.Initialize();
            seasonalCalendar.Initialize();

            // Subscribe to calendar events
            seasonalCalendar.OnSeasonChanged += HandleSeasonChanged;
            seasonalCalendar.OnYearChanged += HandleYearChanged;

            // BUG FIX: Don't subscribe to immediate game over - check in StartNextDay() instead
            // reputationManager.OnReputationDepleted += HandleGameOver;

            ChangeState(GameState.DayStart);
        }

        private void Update()
        {
            // State machine update
            UpdateState();
        }

        /// <summary>
        /// Change game state
        /// </summary>
        private void ChangeState(GameState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
            Debug.Log($"[GameManager] State changed to: {currentState}");

            // Enter state
            EnterState(newState);
        }

        /// <summary>
        /// Enter a new state
        /// </summary>
        private void EnterState(GameState state)
        {
            switch (state)
            {
                case GameState.DayStart:
                    EnterDayStart();
                    break;

                case GameState.Restock:
                    EnterRestock();
                    break;

                case GameState.Consultation:
                    EnterConsultation();
                    break;

                case GameState.DayEnd:
                    EnterDayEnd();
                    break;

                case GameState.GameOver:
                    EnterGameOver();
                    break;
            }
        }

        /// <summary>
        /// Update current state
        /// </summary>
        private void UpdateState()
        {
            switch (currentState)
            {
                case GameState.DayStart:
                    UpdateDayStart();
                    break;

                case GameState.Restock:
                    UpdateRestock();
                    break;

                case GameState.Consultation:
                    UpdateConsultation();
                    break;

                case GameState.DayEnd:
                    UpdateDayEnd();
                    break;

                case GameState.GameOver:
                    UpdateGameOver();
                    break;
            }
        }

        // === DAY START STATE ===

        private void EnterDayStart()
        {
            Debug.Log($"=== {seasonalCalendar.GetDisplayString()} (Turn {seasonalCalendar.CurrentTurn}) START ===");
            currentDay = new DayData(seasonalCalendar.CurrentTurn);

            // Trigger season started event
            OnSeasonStarted?.Invoke(seasonalCalendar.CurrentSeason, seasonalCalendar.CurrentYear);
            OnDayStarted?.Invoke(currentDay);

            // Transition to restock will happen after DayStartScreen is dismissed via UIManager
        }

        private void UpdateDayStart()
        {
            // Waiting for player to acknowledge day start via UIManager
        }

        /// <summary>
        /// Called by UIManager when player dismisses day start screen
        /// </summary>
        public void StartRestock()
        {
            if (currentState == GameState.DayStart)
            {
                ChangeState(GameState.Restock);
            }
        }

        // === RESTOCK STATE ===

        private void EnterRestock()
        {
            Debug.Log("[Restock] Starting restock phase...");

            // Generate heroes for the season/turn
            for (int i = 0; i < heroesPerTurn; i++)
            {
                var heroResult = new HeroResult
                {
                    hero = heroGenerator != null ? heroGenerator.GenerateHero() : null,
                    monster = monsterGenerator != null ? monsterGenerator.GenerateMonster() : null
                };
                currentDay.heroResults.Add(heroResult);
            }

            Debug.Log($"[Restock] Generated {heroesPerTurn} heroes");

            // UI will show restock screen via UIManager
        }

        private void UpdateRestock()
        {
            // Waiting for player to purchase crates via UIManager
        }

        /// <summary>
        /// Called by UIManager when player finishes purchasing crates
        /// </summary>
        public void CompleteCratePurchase(List<ItemData> purchasedItems)
        {
            Debug.Log($"[Restock] Received {purchasedItems.Count} items from crates");
            currentDay.availableItems = purchasedItems;

            // Transition to consultation
            ChangeState(GameState.Consultation);
        }

        // === CONSULTATION STATE ===

        private void EnterConsultation()
        {
            var currentHero = currentDay.GetCurrentHero();
            if (currentHero != null)
            {
                Debug.Log($"[Consultation] Hero {currentDay.currentHeroIndex + 1}/{heroesPerTurn}: {currentHero.hero.heroName}");
                OnHeroReady?.Invoke(currentHero);
            }
            else
            {
                Debug.LogError("[Consultation] No hero available!");
            }
        }

        private void UpdateConsultation()
        {
            // Waiting for player to equip items and call CompleteConsultation()
        }

        /// <summary>
        /// Called by UI when player finishes equipping a hero
        /// </summary>
        public void CompleteConsultation(List<ItemData> equippedItems)
        {
            var currentHero = currentDay.GetCurrentHero();
            if (currentHero == null) return;

            // Store equipped items
            currentHero.equippedItems = equippedItems;

            // Calculate success chance with inspiring bonus
            currentHero.successChance = OneShotCalculator.CalculateSuccessChance(
                currentHero.hero,
                currentHero.monster,
                equippedItems,
                currentDay.inspiringBonus
            );

            currentHero.confidence = OneShotCalculator.GetConfidenceLevel(currentHero.successChance);

            // Roll for success
            currentHero.succeeded = OneShotCalculator.RollOneShot(currentHero.successChance);

            // Tutorial: Force success during tutorial (always win)
            if (tutorialManager != null && tutorialManager.IsTutorialActive())
            {
                currentHero.succeeded = true;
                Debug.Log("[Tutorial] Forced hero success - tutorial hero always wins!");
            }

            // Calculate review
            var (stars, repChange) = OneShotCalculator.CalculateReview(
                currentHero.confidence,
                currentHero.succeeded,
                currentHero.hero.perk
            );

            currentHero.stars = stars;
            currentHero.reputationChange = repChange;

            // BUG FIX: Calculate money change but DON'T apply it yet (apply in StartNextDay after animations)
            if (currentHero.succeeded && goldManager != null)
            {
                currentHero.moneyChange = goldManager.GetMonsterReward(currentHero.monster.rank);
            }
            else
            {
                currentHero.moneyChange = 0;
            }

            Debug.Log($"[Consultation] {currentHero.hero.heroName}: {currentHero.successChance}% → {(currentHero.succeeded ? "SUCCESS" : "FAILED")} ({stars}★, {repChange:+0;-0} rep, {currentHero.moneyChange} gold)");

            // Check for inspiring bonus for next hero
            int inspiringBonus = OneShotCalculator.GetInspiringBonusForNextHero(
                currentHero.hero.perk,
                currentHero.succeeded
            );
            currentDay.inspiringBonus = inspiringBonus;

            OnHeroEquipped?.Invoke(currentHero);

            // Move to next hero or end day
            if (currentDay.MoveToNextHero())
            {
                // Next hero
                ChangeState(GameState.Consultation);
            }
            else
            {
                // All heroes done, go to day end
                ChangeState(GameState.DayEnd);
            }
        }

        // === DAY END STATE ===

        private void EnterDayEnd()
        {
            Debug.Log("=== DAY END ===");

            // Recycle leftover items for gold
            if (goldManager != null && consultationScreen != null)
            {
                int leftoverItems = consultationScreen.GetLeftoverItemCount();
                if (leftoverItems > 0)
                {
                    goldManager.RecycleItems(leftoverItems);
                    Debug.Log($"[Day End] Recycled {leftoverItems} leftover items for gold");
                }
            }

            // BUG FIX: DON'T apply reputation changes here - wait for day end screen animations
            // Reputation will be applied in StartNextDay() after animations

            OnDayEnded?.Invoke(currentDay.heroResults);
        }

        private void UpdateDayEnd()
        {
            // Waiting for player to call StartNextDay()
        }

        /// <summary>
        /// Called by UI when player is ready for next turn/season (after day end animations)
        /// </summary>
        public void StartNextDay()
        {
            // BUG FIX: Apply reputation and money changes AFTER day end screen animations complete
            foreach (var result in currentDay.heroResults)
            {
                reputationManager.AddReputation(result.reputationChange);

                // Apply money changes
                if (result.moneyChange > 0 && goldManager != null)
                {
                    goldManager.AddGold(result.moneyChange);
                }
            }

            Debug.Log($"[Season End] Final Reputation: {reputationManager.CurrentReputation}/100 ({reputationManager.GetReputationStatus()})");
            Debug.Log($"[Season End] Final Gold: {goldManager?.CurrentGold ?? 0}");

            // Age heroes if lifecycle manager is available
            if (heroLifecycleManager != null)
            {
                AgeAllHeroes();
            }

            // Check for game over AFTER applying reputation changes
            if (reputationManager.IsGameOver)
            {
                ChangeState(GameState.GameOver);
            }
            else
            {
                // Advance to next season
                seasonalCalendar.AdvanceSeason();
                ChangeState(GameState.DayStart);
            }
        }

        // === GAME OVER STATE ===

        private void EnterGameOver()
        {
            Debug.Log("=== GAME OVER ===");
            Debug.Log($"Survived {seasonalCalendar.CurrentTurn - 1} turns ({seasonalCalendar.CurrentYear} years)");
            OnGameOver?.Invoke();
        }

        private void UpdateGameOver()
        {
            // Game over, waiting for restart
        }

        private void HandleGameOver()
        {
            if (currentState != GameState.GameOver)
            {
                ChangeState(GameState.GameOver);
            }
        }

        /// <summary>
        /// Handle season change event from SeasonalCalendar
        /// </summary>
        private void HandleSeasonChanged(Season newSeason, int year)
        {
            Debug.Log($"[Calendar] Season changed to {newSeason}, Year {year}");
            OnSeasonChanged?.Invoke(newSeason, year);
        }

        /// <summary>
        /// Handle year change event from SeasonalCalendar
        /// </summary>
        private void HandleYearChanged(int newYear)
        {
            Debug.Log($"[Calendar] *** NEW YEAR: {newYear} ***");
            OnYearChanged?.Invoke(newYear);
        }

        /// <summary>
        /// Age all heroes in the current roster by one turn
        /// </summary>
        private void AgeAllHeroes()
        {
            if (currentDay == null || currentDay.heroResults == null) return;

            foreach (var result in currentDay.heroResults)
            {
                if (result.hero != null && heroLifecycleManager != null)
                {
                    var oldStage = result.hero.lifecycleStage;
                    var newStage = heroLifecycleManager.AgeHero(ref result.hero.age);
                    result.hero.lifecycleStage = newStage;

                    if (oldStage != newStage)
                    {
                        Debug.Log($"[Lifecycle] {result.hero.heroName} aged to {heroLifecycleManager.GetStageDisplayName(newStage)} (age {Mathf.FloorToInt(result.hero.age)})");
                    }
                }
            }
        }

        // === PUBLIC ACCESSORS ===

        public GameState CurrentState => currentState;
        public ReputationManager Reputation => reputationManager;
        public DayData CurrentDay => currentDay;
        public SeasonalCalendar Calendar => seasonalCalendar;
        public int CurrentDayNumber => seasonalCalendar?.CurrentTurn ?? 1; // Backward compatibility
    }
}
