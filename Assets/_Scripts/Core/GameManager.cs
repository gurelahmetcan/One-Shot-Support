using UnityEngine;
using System;
using System.Collections.Generic;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Main game manager implementing the game loop state machine
    /// Controls the day/night cycle and overall game flow
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Configuration")]
        [Tooltip("Number of heroes per day")]
        [Range(1, 5)]
        public int heroesPerDay = 3;

        [Tooltip("Number of items to restock each day")]
        [Range(5, 10)]
        public int itemsPerDay = 6;

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

        [Header("UI References (Internal)")]
        [Tooltip("Consultation screen reference for item recycling")]
        [HideInInspector]
        public UI.Screens.ConsultationScreen consultationScreen;

        // State machine
        private GameState currentState;
        private ReputationManager reputationManager;
        private DayData currentDay;
        private int currentDayNumber = 1;

        // Events for UI
        public event Action<GameState> OnStateChanged;
        public event Action<DayData> OnDayStarted;
        public event Action<HeroResult> OnHeroReady;
        public event Action<HeroResult> OnHeroEquipped;
        public event Action<List<HeroResult>> OnDayEnded;
        public event Action OnGameOver;

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
            DontDestroyOnLoad(gameObject);

            // Initialize systems
            reputationManager = new ReputationManager();
        }

        private void Start()
        {
            // Start the game
            reputationManager.Initialize();
            reputationManager.OnReputationDepleted += HandleGameOver;

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
            Debug.Log($"=== DAY {currentDayNumber} START ===");
            currentDay = new DayData(currentDayNumber);
            OnDayStarted?.Invoke(currentDay);

            // Immediately transition to restock
            ChangeState(GameState.Restock);
        }

        private void UpdateDayStart()
        {
            // Nothing to update, transitions automatically
        }

        // === RESTOCK STATE ===

        private void EnterRestock()
        {
            Debug.Log("[Restock] Starting restock phase...");

            // Generate heroes for the day
            for (int i = 0; i < heroesPerDay; i++)
            {
                var heroResult = new HeroResult
                {
                    hero = heroGenerator != null ? heroGenerator.GenerateHero() : null,
                    monster = monsterGenerator != null ? monsterGenerator.GenerateMonster() : null
                };
                currentDay.heroResults.Add(heroResult);
            }

            Debug.Log($"[Restock] Generated {heroesPerDay} heroes");

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
                Debug.Log($"[Consultation] Hero {currentDay.currentHeroIndex + 1}/{heroesPerDay}: {currentHero.hero.heroName}");
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

            // Calculate review
            var (stars, repChange) = OneShotCalculator.CalculateReview(
                currentHero.confidence,
                currentHero.succeeded,
                currentHero.hero.perk
            );

            currentHero.stars = stars;
            currentHero.reputationChange = repChange;

            // Award gold for successful runs
            if (currentHero.succeeded && goldManager != null)
            {
                goldManager.AwardMonsterGold(currentHero.monster.rank);
            }

            Debug.Log($"[Consultation] {currentHero.hero.heroName}: {currentHero.successChance}% → {(currentHero.succeeded ? "SUCCESS" : "FAILED")} ({stars}★, {repChange:+0;-0} rep)");

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

            // Apply all reputation changes
            foreach (var result in currentDay.heroResults)
            {
                reputationManager.AddReputation(result.reputationChange);
            }

            Debug.Log($"[Day End] Reputation: {reputationManager.CurrentReputation}/100 ({reputationManager.GetReputationStatus()})");

            OnDayEnded?.Invoke(currentDay.heroResults);
        }

        private void UpdateDayEnd()
        {
            // Waiting for player to call StartNextDay()
        }

        /// <summary>
        /// Called by UI when player is ready for next day
        /// </summary>
        public void StartNextDay()
        {
            if (reputationManager.IsGameOver)
            {
                ChangeState(GameState.GameOver);
            }
            else
            {
                currentDayNumber++;
                ChangeState(GameState.DayStart);
            }
        }

        // === GAME OVER STATE ===

        private void EnterGameOver()
        {
            Debug.Log("=== GAME OVER ===");
            Debug.Log($"Survived {currentDayNumber - 1} days");
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

        // === PUBLIC ACCESSORS ===

        public GameState CurrentState => currentState;
        public ReputationManager Reputation => reputationManager;
        public DayData CurrentDay => currentDay;
        public int CurrentDayNumber => currentDayNumber;
    }
}
