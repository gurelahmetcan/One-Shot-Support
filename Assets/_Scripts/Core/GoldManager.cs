using System;
using UnityEngine;
using OneShotSupport.Data;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Manages the player's gold currency
    /// Handles earning gold from successful runs and spending on crates
    /// </summary>
    public class GoldManager : MonoBehaviour
    {
        [Header("Starting Gold")]
        [SerializeField] private int startingGold = 50;

        [Header("Crate Costs")]
        [SerializeField] private int cheapCrateCost = 10;
        [SerializeField] private int mediumCrateCost = 30;
        [SerializeField] private int premiumCrateCost = 50;

        [Header("Monster Rank Rewards")]
        [SerializeField] private int dRankReward = 15;
        [SerializeField] private int cRankReward = 30;
        [SerializeField] private int bRankReward = 45;
        [SerializeField] private int aRankReward = 60;
        [SerializeField] private int sRankReward = 100;

        [Header("Recycling")]
        [SerializeField] private int recycleValue = 10;

        private int currentGold;

        // Events
        public event Action<int> OnGoldChanged;
        public event Action OnNotEnoughGold;

        public int CurrentGold => currentGold;

        private void Awake()
        {
            currentGold = startingGold;
        }

        /// <summary>
        /// Reset gold to starting amount (for new game)
        /// </summary>
        public void ResetGold()
        {
            currentGold = startingGold;
            OnGoldChanged?.Invoke(currentGold);
        }

        /// <summary>
        /// Add gold to the player's total
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
            Debug.Log($"[GoldManager] Added {amount} gold. Total: {currentGold}");
        }

        /// <summary>
        /// Try to spend gold. Returns true if successful, false if not enough gold
        /// </summary>
        public bool TrySpendGold(int amount)
        {
            if (amount <= 0) return false;

            if (currentGold >= amount)
            {
                currentGold -= amount;
                OnGoldChanged?.Invoke(currentGold);
                Debug.Log($"[GoldManager] Spent {amount} gold. Remaining: {currentGold}");
                return true;
            }

            Debug.LogWarning($"[GoldManager] Not enough gold! Need {amount}, have {currentGold}");
            OnNotEnoughGold?.Invoke();
            return false;
        }
        
        /// <summary>
        /// Spend gold without checking.
        /// </summary>
        public void SpendGold(int amount)
        {
            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            Debug.Log($"[GoldManager] Spent {amount} gold. Remaining: {currentGold}");
        }

        /// <summary>
        /// Get the cost of a specific crate type
        /// </summary>
        public int GetCrateCost(CrateType crateType)
        {
            return crateType switch
            {
                CrateType.Cheap => cheapCrateCost,
                CrateType.Medium => mediumCrateCost,
                CrateType.Premium => premiumCrateCost,
                _ => 0
            };
        }

        /// <summary>
        /// Check if player can afford a crate
        /// </summary>
        public bool CanAffordCrate(CrateType crateType)
        {
            return currentGold >= GetCrateCost(crateType);
        }

        /// <summary>
        /// Get gold reward for defeating a monster of a given rank
        /// </summary>
        public int GetMonsterReward(MonsterRank rank)
        {
            return rank switch
            {
                MonsterRank.D => dRankReward,
                MonsterRank.C => cRankReward,
                MonsterRank.B => bRankReward,
                MonsterRank.A => aRankReward,
                MonsterRank.S => sRankReward,
                _ => 0
            };
        }

        /// <summary>
        /// Award gold for successful monster defeat
        /// </summary>
        public void AwardMonsterGold(MonsterRank rank)
        {
            int reward = GetMonsterReward(rank);
            AddGold(reward);
        }

        /// <summary>
        /// Get recycling value for leftover items
        /// </summary>
        public int GetRecycleValue()
        {
            return recycleValue;
        }

        /// <summary>
        /// Recycle leftover items for gold
        /// </summary>
        public void RecycleItems(int itemCount)
        {
            if (itemCount <= 0) return;

            int totalGold = itemCount * recycleValue;
            AddGold(totalGold);
            Debug.Log($"[GoldManager] Recycled {itemCount} items for {totalGold} gold");
        }
    }
}
