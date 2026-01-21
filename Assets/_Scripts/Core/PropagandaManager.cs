using System;
using UnityEngine;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Manages the Fame vs Trust system (The Propaganda Engine)
    /// Fame = Quantity (additive, unlocks milestones)
    /// Trust = Quality (0-100%, acts as multiplier)
    /// </summary>
    [System.Serializable]
    public class PropagandaManager
    {
        [Header("Fame System")]
        [Tooltip("Current fame points (additive)")]
        [SerializeField] private int currentFame = 0;

        [Header("Trust System")]
        [Tooltip("Current trust percentage (0-100)")]
        [SerializeField] private int currentTrust = 50;

        [Header("Starting Values")]
        [Tooltip("Starting fame points")]
        public int startingFame = 0;

        [Tooltip("Starting trust percentage")]
        public int startingTrust = 50;

        // Fame Milestones (from GDD)
        public const int MILESTONE_MARKET_INFLUENCE = 1000;    // 10% cheaper materials
        public const int MILESTONE_PRESTIGIOUS_NAME = 2500;    // 20% lower signing bonuses
        public const int MILESTONE_CHARTERED_GUILD = 5000;     // Tax immunity

        // Trust Thresholds (from GDD)
        public const int TRUST_GOLDEN_REPUTATION = 80;         // 2x Fame gains, +25% bargaining
        public const int TRUST_NOTORIOUS = 20;                 // 0.5x Fame gains, 2x hazard pay

        // Events
        public event Action<int> OnFameChanged;                // (newFame)
        public event Action<int> OnTrustChanged;               // (newTrust)
        public event Action<FameMilestone> OnFameMilestoneReached; // (milestone)
        public event Action<TrustThreshold> OnTrustThresholdCrossed; // (threshold)

        // Properties
        public int CurrentFame => currentFame;
        public int CurrentTrust => currentTrust;

        /// <summary>
        /// Initialize the propaganda system
        /// </summary>
        public void Initialize()
        {
            currentFame = startingFame;
            currentTrust = startingTrust;
            Debug.Log($"[Propaganda] Initialized - Fame: {currentFame}, Trust: {currentTrust}%");
        }

        /// <summary>
        /// Add fame points (will be multiplied by trust)
        /// </summary>
        /// <param name="baseFame">Base fame to add (before trust multiplier)</param>
        /// <param name="applyTrustMultiplier">Whether to apply trust multiplier</param>
        public void AddFame(int baseFame, bool applyTrustMultiplier = true)
        {
            if (baseFame == 0) return;

            int oldFame = currentFame;
            int fameToAdd = baseFame;

            // Apply trust multiplier if enabled
            if (applyTrustMultiplier)
            {
                float trustMultiplier = GetTrustMultiplier();
                fameToAdd = Mathf.RoundToInt(baseFame * trustMultiplier);
            }

            currentFame += fameToAdd;
            currentFame = Mathf.Max(0, currentFame); // Can't go below 0

            Debug.Log($"[Propaganda] Fame changed: {oldFame} -> {currentFame} (added {fameToAdd}, base {baseFame})");

            OnFameChanged?.Invoke(currentFame);

            // Check for milestone reached
            CheckFameMilestones(oldFame, currentFame);
        }

        /// <summary>
        /// Set fame directly (for testing or special events)
        /// </summary>
        public void SetFame(int newFame)
        {
            int oldFame = currentFame;
            currentFame = Mathf.Max(0, newFame);
            OnFameChanged?.Invoke(currentFame);
            CheckFameMilestones(oldFame, currentFame);
        }

        /// <summary>
        /// Add or remove trust percentage
        /// </summary>
        public void AddTrust(int trustDelta)
        {
            if (trustDelta == 0) return;

            int oldTrust = currentTrust;
            currentTrust += trustDelta;
            currentTrust = Mathf.Clamp(currentTrust, 0, 100);

            Debug.Log($"[Propaganda] Trust changed: {oldTrust}% -> {currentTrust}% (delta {trustDelta})");

            OnTrustChanged?.Invoke(currentTrust);

            // Check for threshold crossing
            CheckTrustThresholds(oldTrust, currentTrust);
        }

        /// <summary>
        /// Set trust directly
        /// </summary>
        public void SetTrust(int newTrust)
        {
            int oldTrust = currentTrust;
            currentTrust = Mathf.Clamp(newTrust, 0, 100);
            OnTrustChanged?.Invoke(currentTrust);
            CheckTrustThresholds(oldTrust, currentTrust);
        }

        /// <summary>
        /// Get the trust multiplier for fame gains
        /// Golden Reputation (80%+): 2x
        /// Notorious (<20%): 0.5x
        /// Normal: 1x
        /// </summary>
        public float GetTrustMultiplier()
        {
            if (currentTrust >= TRUST_GOLDEN_REPUTATION)
                return 2.0f;
            else if (currentTrust < TRUST_NOTORIOUS)
                return 0.5f;
            else
                return 1.0f;
        }

        /// <summary>
        /// Get bargaining success rate modifier based on trust
        /// Golden Reputation: +25%
        /// Notorious: 0% (no bonus)
        /// </summary>
        public float GetBargainingBonus()
        {
            if (currentTrust >= TRUST_GOLDEN_REPUTATION)
                return 0.25f; // +25%
            else
                return 0f;
        }

        /// <summary>
        /// Check if hazard pay is required (low trust)
        /// </summary>
        public bool RequiresHazardPay()
        {
            return currentTrust < TRUST_NOTORIOUS;
        }

        /// <summary>
        /// Get hazard pay multiplier (2x salary if notorious)
        /// </summary>
        public float GetHazardPayMultiplier()
        {
            return RequiresHazardPay() ? 2.0f : 1.0f;
        }

        /// <summary>
        /// Check if a fame milestone has been unlocked
        /// </summary>
        public bool HasReachedMilestone(FameMilestone milestone)
        {
            return milestone switch
            {
                FameMilestone.MarketInfluence => currentFame >= MILESTONE_MARKET_INFLUENCE,
                FameMilestone.PrestigiousName => currentFame >= MILESTONE_PRESTIGIOUS_NAME,
                FameMilestone.CharteredGuild => currentFame >= MILESTONE_CHARTERED_GUILD,
                _ => false
            };
        }

        /// <summary>
        /// Get the material cost discount from Market Influence milestone
        /// </summary>
        public float GetMaterialCostDiscount()
        {
            return HasReachedMilestone(FameMilestone.MarketInfluence) ? 0.10f : 0f; // 10% discount
        }

        /// <summary>
        /// Get the signing bonus discount from Prestigious Name milestone
        /// </summary>
        public float GetSigningBonusDiscount()
        {
            return HasReachedMilestone(FameMilestone.PrestigiousName) ? 0.20f : 0f; // 20% discount
        }

        /// <summary>
        /// Check if tax immunity is unlocked (Chartered Guild)
        /// </summary>
        public bool HasTaxImmunity()
        {
            return HasReachedMilestone(FameMilestone.CharteredGuild);
        }

        /// <summary>
        /// Get current trust status as a string
        /// </summary>
        public string GetTrustStatus()
        {
            if (currentTrust >= TRUST_GOLDEN_REPUTATION)
                return "Golden Reputation";
            else if (currentTrust < TRUST_NOTORIOUS)
                return "Notorious";
            else
                return "Neutral";
        }

        /// <summary>
        /// Check for fame milestone crossing
        /// </summary>
        private void CheckFameMilestones(int oldFame, int newFame)
        {
            // Check Market Influence (1000)
            if (oldFame < MILESTONE_MARKET_INFLUENCE && newFame >= MILESTONE_MARKET_INFLUENCE)
            {
                Debug.Log($"[Propaganda] *** MILESTONE REACHED: Market Influence (10% cheaper materials) ***");
                OnFameMilestoneReached?.Invoke(FameMilestone.MarketInfluence);
            }

            // Check Prestigious Name (2500)
            if (oldFame < MILESTONE_PRESTIGIOUS_NAME && newFame >= MILESTONE_PRESTIGIOUS_NAME)
            {
                Debug.Log($"[Propaganda] *** MILESTONE REACHED: Prestigious Name (20% lower signing bonuses) ***");
                OnFameMilestoneReached?.Invoke(FameMilestone.PrestigiousName);
            }

            // Check Chartered Guild (5000)
            if (oldFame < MILESTONE_CHARTERED_GUILD && newFame >= MILESTONE_CHARTERED_GUILD)
            {
                Debug.Log($"[Propaganda] *** MILESTONE REACHED: Chartered Guild (Tax Immunity) ***");
                OnFameMilestoneReached?.Invoke(FameMilestone.CharteredGuild);
            }
        }

        /// <summary>
        /// Check for trust threshold crossing
        /// </summary>
        private void CheckTrustThresholds(int oldTrust, int newTrust)
        {
            // Check Golden Reputation (80+)
            if (oldTrust < TRUST_GOLDEN_REPUTATION && newTrust >= TRUST_GOLDEN_REPUTATION)
            {
                Debug.Log($"[Propaganda] *** Trust Status: Golden Reputation! (2x Fame gains, +25% bargaining) ***");
                OnTrustThresholdCrossed?.Invoke(TrustThreshold.GoldenReputation);
            }
            else if (oldTrust >= TRUST_GOLDEN_REPUTATION && newTrust < TRUST_GOLDEN_REPUTATION)
            {
                Debug.Log($"[Propaganda] Lost Golden Reputation status");
                OnTrustThresholdCrossed?.Invoke(TrustThreshold.LostGoldenReputation);
            }

            // Check Notorious (<20)
            if (oldTrust >= TRUST_NOTORIOUS && newTrust < TRUST_NOTORIOUS)
            {
                Debug.Log($"[Propaganda] *** Trust Status: Notorious! (0.5x Fame gains, 2x hazard pay required) ***");
                OnTrustThresholdCrossed?.Invoke(TrustThreshold.Notorious);
            }
            else if (oldTrust < TRUST_NOTORIOUS && newTrust >= TRUST_NOTORIOUS)
            {
                Debug.Log($"[Propaganda] No longer Notorious");
                OnTrustThresholdCrossed?.Invoke(TrustThreshold.RecoveredFromNotorious);
            }
        }
    }

    /// <summary>
    /// Fame milestones that unlock permanent benefits
    /// </summary>
    public enum FameMilestone
    {
        MarketInfluence,    // 1,000 Fame - 10% cheaper materials
        PrestigiousName,    // 2,500 Fame - 20% lower signing bonuses
        CharteredGuild      // 5,000 Fame - Tax immunity
    }

    /// <summary>
    /// Trust thresholds that affect multipliers
    /// </summary>
    public enum TrustThreshold
    {
        GoldenReputation,           // Reached 80%+
        LostGoldenReputation,       // Fell below 80%
        Notorious,                  // Fell below 20%
        RecoveredFromNotorious      // Recovered to 20%+
    }
}
