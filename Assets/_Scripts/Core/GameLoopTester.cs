using UnityEngine;
using System.Collections.Generic;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Debug script to test the game loop without UI
    /// Attach to the same GameObject as GameManager
    /// </summary>
    public class GameLoopTester : MonoBehaviour
    {
        [Header("Auto Test Settings")]
        [Tooltip("Automatically run the game loop on start")]
        public bool autoRun = true;

        [Tooltip("Auto-equip random items for each hero")]
        public bool autoEquip = true;

        [Tooltip("Delay between auto-actions (seconds)")]
        public float actionDelay = 2f;

        private GameManager gameManager;
        private float nextActionTime;
        private bool waitingForAction;

        private void Start()
        {
            gameManager = GetComponent<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("GameLoopTester requires GameManager on the same GameObject!");
                enabled = false;
                return;
            }

            // Subscribe to events
            gameManager.OnStateChanged += HandleStateChanged;
            gameManager.OnHeroReady += HandleHeroReady;
            gameManager.OnDayEnded += HandleDayEnded;
            gameManager.OnGameOver += HandleGameOver;

            Debug.Log("[GameLoopTester] Initialized and listening to game events");
        }

        private void Update()
        {
            if (!autoRun || !waitingForAction) return;

            // Auto-advance the game loop
            if (Time.time >= nextActionTime)
            {
                waitingForAction = false;
                PerformAutoAction();
            }
        }

        private void HandleStateChanged(GameState newState)
        {
            Debug.Log($"<color=cyan>[TEST] State → {newState}</color>");
        }

        private void HandleHeroReady(HeroResult heroResult)
        {
            Debug.Log($"<color=yellow>[TEST] Hero Ready: {heroResult.hero.heroName} vs {heroResult.monster.monsterName}</color>");

            if (autoEquip)
            {
                // Schedule auto-equip
                waitingForAction = true;
                nextActionTime = Time.time + actionDelay;
            }
        }

        private void HandleDayEnded(List<HeroResult> results)
        {
            Debug.Log("<color=green>[TEST] === DAY END RESULTS ===</color>");
            foreach (var result in results)
            {
                string resultText = result.succeeded ? "<color=green>SUCCESS</color>" : "<color=red>FAILED</color>";
                Debug.Log($"  {result.hero.heroName}: {result.successChance}% → {resultText} ({result.stars}★, {result.reputationChange:+0;-0} rep)");
            }
            Debug.Log($"<color=green>[TEST] Total Reputation: {gameManager.Reputation.CurrentReputation}/100</color>");

            if (autoRun)
            {
                // Schedule next day
                waitingForAction = true;
                nextActionTime = Time.time + actionDelay * 2; // Longer delay for day end
            }
        }

        private void HandleGameOver()
        {
            Debug.Log("<color=red>[TEST] === GAME OVER ===</color>");
            Debug.Log($"<color=red>[TEST] Survived {gameManager.CurrentDayNumber - 1} days</color>");
            autoRun = false; // Stop auto-running
        }

        private void PerformAutoAction()
        {
            switch (gameManager.CurrentState)
            {
                case GameState.Consultation:
                    AutoEquipHero();
                    break;

                case GameState.DayEnd:
                    gameManager.StartNextDay();
                    break;
            }
        }

        private void AutoEquipHero()
        {
            var currentHero = gameManager.CurrentDay.GetCurrentHero();
            if (currentHero == null) return;

            // Get effective slots
            int effectiveSlots = currentHero.hero.GetEffectiveSlots();

            // Randomly select items from available items
            var availableItems = gameManager.CurrentDay.availableItems;
            var equippedItems = new List<ItemData>();

            int itemsToEquip = Mathf.Min(effectiveSlots, availableItems.Count);
            for (int i = 0; i < itemsToEquip; i++)
            {
                if (availableItems.Count > 0)
                {
                    int randomIndex = Random.Range(0, availableItems.Count);
                    equippedItems.Add(availableItems[randomIndex]);
                    // Note: Not removing from available items for now (can equip same item multiple times)
                }
            }

            Debug.Log($"<color=yellow>[TEST] Auto-equipped {equippedItems.Count} items for {currentHero.hero.heroName}</color>");

            // Complete consultation
            gameManager.CompleteConsultation(equippedItems);
        }

        // Manual test buttons (called from context menu)

        [ContextMenu("Manual: Equip Random Items")]
        public void ManualEquipRandom()
        {
            if (gameManager.CurrentState == GameState.Consultation)
            {
                AutoEquipHero();
            }
            else
            {
                Debug.LogWarning("Not in Consultation state!");
            }
        }

        [ContextMenu("Manual: Start Next Day")]
        public void ManualNextDay()
        {
            if (gameManager.CurrentState == GameState.DayEnd)
            {
                gameManager.StartNextDay();
            }
            else
            {
                Debug.LogWarning("Not in DayEnd state!");
            }
        }

        [ContextMenu("Manual: Toggle Auto Run")]
        public void ToggleAutoRun()
        {
            autoRun = !autoRun;
            Debug.Log($"Auto Run: {autoRun}");
        }
    }
}
