using UnityEngine;
using System;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Manages player's shop reputation
    /// Range: 0-100, starts at 30, game over at 0
    /// </summary>
    public class ReputationManager
    {
        private const int MIN_REPUTATION = 0;
        private const int MAX_REPUTATION = 100;
        private const int STARTING_REPUTATION = 30;

        private int currentReputation;

        // Events
        public event Action<int> OnReputationChanged;
        public event Action OnReputationDepleted;

        public int CurrentReputation => currentReputation;
        public bool IsGameOver => currentReputation <= MIN_REPUTATION;

        /// <summary>
        /// Initialize reputation system
        /// </summary>
        public void Initialize()
        {
            currentReputation = STARTING_REPUTATION;
            OnReputationChanged?.Invoke(currentReputation);
        }

        /// <summary>
        /// Add reputation (clamped to max)
        /// </summary>
        public void AddReputation(int amount)
        {
            int oldReputation = currentReputation;
            currentReputation = Mathf.Clamp(currentReputation + amount, MIN_REPUTATION, MAX_REPUTATION);

            if (currentReputation != oldReputation)
            {
                OnReputationChanged?.Invoke(currentReputation);
                Debug.Log($"Reputation: {oldReputation} → {currentReputation} ({(amount > 0 ? "+" : "")}{amount})");
            }

            // Check for game over
            if (currentReputation <= MIN_REPUTATION)
            {
                OnReputationDepleted?.Invoke();
            }
        }

        /// <summary>
        /// Remove reputation (can trigger game over)
        /// </summary>
        public void RemoveReputation(int amount)
        {
            AddReputation(-amount);
        }

        /// <summary>
        /// Get reputation as normalized value (0-1)
        /// </summary>
        public float GetNormalizedReputation()
        {
            return (float)currentReputation / MAX_REPUTATION;
        }

        /// <summary>
        /// Get reputation status text
        /// </summary>
        public string GetReputationStatus()
        {
            float normalized = GetNormalizedReputation();

            if (normalized >= 0.8f) return "Excellent";
            if (normalized >= 0.6f) return "Good";
            if (normalized >= 0.4f) return "Average";
            if (normalized >= 0.2f) return "Poor";
            return "Critical";
        }
    }
}
