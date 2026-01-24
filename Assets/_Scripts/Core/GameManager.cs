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

        [Tooltip("Mission generator for procedural missions")]
        public MissionGenerator missionGenerator;

        [Header("Mission Configuration")]
        [Tooltip("Number of missions available per season")]
        [Range(1, 5)]
        public int missionsPerSeason = 3;

        [Header("Tavern Configuration")]
        [Tooltip("Number of heroes available in tavern")]
        [Range(2, 6)]
        public int heroesInTavern = 4;

        [Tooltip("Base cost to recruit a hero")]
        public int baseRecruitmentCost = 50;

        [Tooltip("Maximum barracks capacity (recruited hero roster size)")]
        [Range(4, 12)]
        public int maxBarracksCapacity = 6;

        [Header("Managers")]
        [Tooltip("Gold manager for currency system")]
        public GoldManager goldManager;

        [Tooltip("Hero lifecycle manager for aging system")]
        public HeroLifecycleManager heroLifecycleManager;
        
        // State machine
        private GameState currentState;
        private ReputationManager reputationManager;
        private PropagandaManager propagandaManager;
        private DayData currentDay; // Note: Still called "DayData" for now, represents current turn/season
        private SeasonalCalendar seasonalCalendar;
        private List<MissionData> availableMissions;
        private MissionData selectedMission;

        // Hero roster management
        private List<HeroData> recruitedHeroes = new List<HeroData>(); // Heroes in barracks
        private List<HeroData> tavernHeroes = new List<HeroData>(); // Heroes available for recruitment

        // Season refresh tracking
        private bool missionsGeneratedThisSeason = false;
        private bool tavernHeroesGeneratedThisSeason = false;

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
        public event Action<int> OnFameChanged; // Propagate fame changes to UI
        public event Action<int> OnTrustChanged; // Propagate trust changes to UI
        public event Action<FameMilestone> OnFameMilestoneReached; // Propagate milestone events
        public event Action<TrustThreshold> OnTrustThresholdCrossed; // Propagate trust threshold events
        public event Action<List<MissionData>> OnMissionsGenerated; // (missions) - when missions are available
        public event Action<MissionData> OnMissionSelected; // (mission) - when a mission is selected
        public event Action<List<HeroData>> OnTavernHeroesGenerated; // (heroes, cost) - when tavern heroes are available
        public event Action<HeroData, ContractOffer> OnHeroRecruited; // (hero, offer) - when a hero is recruited with contract
        public event Action<HeroData> OnHeroWalkedAway; // (hero) - when a hero walks away from negotiation
        public event Action<List<HeroData>, int> OnBarracksOpened; // (heroes, maxCapacity) - when barracks is opened

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
            propagandaManager = new PropagandaManager();
            seasonalCalendar = new SeasonalCalendar();
        }

        private void Start()
        {
            // Start the game
            reputationManager.Initialize();
            propagandaManager.Initialize();
            seasonalCalendar.Initialize();

            // Subscribe to calendar events
            seasonalCalendar.OnSeasonChanged += HandleSeasonChanged;
            seasonalCalendar.OnYearChanged += HandleYearChanged;

            // Subscribe to propaganda events
            propagandaManager.OnFameChanged += (fame) => OnFameChanged?.Invoke(fame);
            propagandaManager.OnTrustChanged += (trust) => OnTrustChanged?.Invoke(trust);
            propagandaManager.OnFameMilestoneReached += (milestone) => OnFameMilestoneReached?.Invoke(milestone);
            propagandaManager.OnTrustThresholdCrossed += (threshold) => OnTrustThresholdCrossed?.Invoke(threshold);

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

                case GameState.VillageHub:
                    EnterVillageHub();
                    break;

                case GameState.MissionBoard:
                    EnterMissionBoard();
                    break;

                case GameState.Tavern:
                    EnterTavern();
                    break;

                case GameState.Barracks:
                    EnterBarracks();
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

                case GameState.VillageHub:
                    UpdateVillageHub();
                    break;

                case GameState.MissionBoard:
                    UpdateMissionBoard();
                    break;

                case GameState.Tavern:
                    UpdateTavern();
                    break;

                case GameState.Barracks:
                    UpdateBarracks();
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

            // Transition to village hub will happen after DayStartScreen is dismissed via UIManager
        }

        private void UpdateDayStart()
        {
            // Waiting for player to acknowledge day start via UIManager
        }

        /// <summary>
        /// Called by UIManager when player dismisses day start screen
        /// </summary>
        public void StartVillageHub()
        {
            if (currentState == GameState.DayStart)
            {
                ChangeState(GameState.VillageHub);
            }
        }

        // === VILLAGE HUB STATE ===

        private void EnterVillageHub()
        {
            Debug.Log("[VillageHub] Entering village hub (main navigation)");

            // UI will show village hub via UIManager
        }

        private void UpdateVillageHub()
        {
            // Waiting for player to navigate to locations via UIManager
        }

        /// <summary>
        /// Called by UIManager when player clicks Tavern button
        /// </summary>
        public void OpenTavern()
        {
            if (currentState == GameState.VillageHub)
            {
                ChangeState(GameState.Tavern);
            }
        }

        /// <summary>
        /// Called by UIManager when player clicks Mission Board button
        /// </summary>
        public void OpenMissionBoard()
        {
            if (currentState == GameState.VillageHub)
            {
                ChangeState(GameState.MissionBoard);
            }
        }

        /// <summary>
        /// Called by UIManager when player dismisses day start screen (legacy, redirects to village hub)
        /// </summary>
        public void StartMissionBoard()
        {
            if (currentState == GameState.DayStart)
            {
                StartVillageHub();
            }
        }

        // === TAVERN STATE ===

        private void EnterTavern()
        {
            Debug.Log("[Tavern] Entering tavern for hero recruitment");

            // Only generate heroes if not already generated this season
            if (!tavernHeroesGeneratedThisSeason)
            {
                // Generate heroes available for recruitment
                if (heroGenerator != null)
                {
                    tavernHeroes.Clear();
                    for (int i = 0; i < heroesInTavern; i++)
                    {
                        var hero = heroGenerator.GenerateHero();
                        // Hero is fully initialized by HeroGenerator with random stats, age, and traits
                        tavernHeroes.Add(hero);
                    }

                    tavernHeroesGeneratedThisSeason = true;
                    Debug.Log($"[Tavern] Generated {tavernHeroes.Count} heroes for recruitment (new season)");
                }
                else
                {
                    Debug.LogError("[Tavern] HeroGenerator is null!");
                }
            }
            else
            {
                Debug.Log($"[Tavern] Using existing heroes from this season ({tavernHeroes.Count} available)");
            }

            // Always invoke event to show tavern UI
            OnTavernHeroesGenerated?.Invoke(tavernHeroes);

            // UI will show tavern via UIManager
        }

        private void UpdateTavern()
        {
            // Waiting for player to recruit heroes or leave tavern
        }

        /// <summary>
        /// Called by UIManager when player recruits a hero from tavern
        /// </summary>
        public void RecruitHero(HeroData hero, ContractOffer offer)
        {
            if (currentState != GameState.Tavern) return;

            // Check if player can afford signing bonus
            if (goldManager != null && goldManager.CurrentGold < offer.signingBonus)
            {
                Debug.LogWarning($"[Tavern] Cannot afford signing bonus! Need {offer.signingBonus}, have {goldManager.CurrentGold}");
                return;
            }

            // Check if barracks is full
            if (recruitedHeroes.Count >= maxBarracksCapacity)
            {
                Debug.LogWarning($"[Tavern] Barracks full! Cannot recruit more heroes (max {maxBarracksCapacity})");
                return;
            }

            // Deduct signing bonus
            if (goldManager != null)
            {
                goldManager.TrySpendGold(offer.signingBonus);
            }

            // Apply contract to hero
            hero.dailySalary = offer.dailySalary;
            hero.contractLengthInYears = offer.contractLengthYears;

            // Add to recruited heroes
            recruitedHeroes.Add(hero);

            // Remove from tavern heroes
            tavernHeroes.Remove(hero);

            Debug.Log($"[Tavern] Recruited {hero.heroName}! Contract: {offer.signingBonus}g signing + {offer.dailySalary}g/turn × {offer.contractLengthYears}yr. Barracks: {recruitedHeroes.Count}/{maxBarracksCapacity}");
            OnHeroRecruited?.Invoke(hero, offer);
        }

        /// <summary>
        /// Called when a hero walks away from negotiation
        /// </summary>
        public void HeroWalkedAway(HeroData hero)
        {
            if (currentState != GameState.Tavern) return;

            // Mark walk-away turn for re-recruitment lockout
            hero.walkAwayTurn = seasonalCalendar.CurrentTurn;

            Debug.LogWarning($"[Tavern] {hero.heroName} walked away! Locked until turn {hero.walkAwayTurn + 4}");
            OnHeroWalkedAway?.Invoke(hero);
        }

        /// <summary>
        /// Called by UIManager when player leaves tavern
        /// </summary>
        public void LeaveTavern()
        {
            if (currentState == GameState.Tavern)
            {
                ChangeState(GameState.VillageHub);
            }
        }

        // === BARRACKS STATE ===

        private void EnterBarracks()
        {
            Debug.Log("[Barracks] Entering barracks to view recruited heroes");

            // Show recruited heroes
            OnBarracksOpened?.Invoke(recruitedHeroes, maxBarracksCapacity);

            // UI will show barracks via UIManager
        }

        private void UpdateBarracks()
        {
            // Waiting for player to view heroes or leave barracks
        }

        /// <summary>
        /// Called by UIManager when player opens barracks from village hub
        /// </summary>
        public void OpenBarracks()
        {
            ChangeState(GameState.Barracks);
        }

        /// <summary>
        /// Called by UIManager when player leaves barracks
        /// </summary>
        public void LeaveBarracks()
        {
            if (currentState == GameState.Barracks)
            {
                ChangeState(GameState.VillageHub);
            }
        }

        // === MISSION BOARD STATE ===

        private void EnterMissionBoard()
        {
            Debug.Log("[MissionBoard] Entering mission board...");

            // Only generate missions if not already generated this season
            if (!missionsGeneratedThisSeason)
            {
                // Generate missions for this season
                if (missionGenerator != null)
                {
                    availableMissions = missionGenerator.GenerateMissions(missionsPerSeason);
                    missionsGeneratedThisSeason = true;
                    Debug.Log($"[MissionBoard] Generated {availableMissions.Count} missions (new season)");
                }
                else
                {
                    Debug.LogError("[MissionBoard] MissionGenerator is null!");
                    availableMissions = new List<MissionData>();
                }

                selectedMission = null;
            }
            else
            {
                Debug.Log($"[MissionBoard] Using existing missions from this season ({availableMissions.Count} available, selected: {(selectedMission != null ? selectedMission.missionName : "none")})");
            }

            // Always invoke event to show mission board UI
            OnMissionsGenerated?.Invoke(availableMissions);

            // UI will show mission board via UIManager
        }

        private void UpdateMissionBoard()
        {
            // Waiting for player to select a mission via UIManager
        }

        /// <summary>
        /// Called by UIManager when player selects a mission
        /// </summary>
        public void SelectMission(MissionData mission)
        {
            if (currentState != GameState.MissionBoard) return;

            selectedMission = mission;
            Debug.Log($"[MissionBoard] Mission selected: {mission.missionName}");
            OnMissionSelected?.Invoke(mission);

            // Transition back to village hub after mission selection
            // Player can continue preparing (recruit heroes, etc.) before starting the mission
            ChangeState(GameState.VillageHub);
        }

        /// <summary>
        /// Called by UIManager when player leaves mission board without selecting
        /// </summary>
        public void LeaveMissionBoard()
        {
            if (currentState == GameState.MissionBoard)
            {
                ChangeState(GameState.VillageHub);
            }
        }

        /// <summary>
        /// Called by UIManager when player dismisses day start screen (legacy, redirects to mission board)
        /// </summary>
        public void StartRestock()
        {
            if (currentState == GameState.DayStart)
            {
                StartMissionBoard();
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
        
        // === DAY END STATE ===

        private void EnterDayEnd()
        {
            Debug.Log("=== DAY END ===");
            
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

            AgeAllHeroes();

            // Check for game over AFTER applying reputation changes
            if (reputationManager.IsGameOver)
            {
                ChangeState(GameState.GameOver);
            }
            else
            {
                // Advance to next season
                seasonalCalendar.AdvanceSeason();

                // Reset season refresh flags for new content
                missionsGeneratedThisSeason = false;
                tavernHeroesGeneratedThisSeason = false;

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
            foreach (var hero in recruitedHeroes)
            {
                hero.TickTurn();
            }
        }

        // === PUBLIC ACCESSORS ===

        public GameState CurrentState => currentState;
        public ReputationManager Reputation => reputationManager;
        public PropagandaManager Propaganda => propagandaManager;
        public DayData CurrentDay => currentDay;
        public SeasonalCalendar Calendar => seasonalCalendar;
        public int CurrentDayNumber => seasonalCalendar?.CurrentTurn ?? 1; // Backward compatibility
        public List<MissionData> AvailableMissions => availableMissions;
        public MissionData SelectedMission => selectedMission;
        public List<HeroData> RecruitedHeroes => recruitedHeroes;
        public List<HeroData> TavernHeroes => tavernHeroes;
        public int BarracksCapacity => maxBarracksCapacity;
        public int RecruitmentCost => baseRecruitmentCost;
    }
}
