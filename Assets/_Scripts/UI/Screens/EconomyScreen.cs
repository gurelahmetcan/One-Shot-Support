using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Economy screen for viewing seasonal expenses and incomes
    /// Displays hero salaries, income sources, and net balance
    /// </summary>
    public class EconomyScreen : MonoBehaviour
    {
        [Header("Entry Slots")]
        [SerializeField] private Components.EconomyEntrySlot entrySlotPrefab;
        [SerializeField] private Transform entryContainer;

        [Header("UI References")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI balanceText;

        // Events
        public event Action OnBackClicked;

        // Track instantiated slots
        private List<Components.EconomyEntrySlot> instantiatedSlots = new List<Components.EconomyEntrySlot>();

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackClicked);
            }
        }

        /// <summary>
        /// Setup and show the economy screen with current heroes and gold
        /// </summary>
        public void Setup(List<HeroData> recruitedHeroes, int currentGold)
        {
            // Calculate expenses and income
            List<EconomyEntry> entries = new List<EconomyEntry>();
            int totalExpenses = 0;
            int totalIncome = 0;

            // Add header
            if (titleText != null)
            {
                titleText.text = "Seasonal Economy";
            }

            // Calculate hero salaries (expenses)
            foreach (HeroData hero in recruitedHeroes)
            {
                int seasonalSalary = hero.dailySalary;
                entries.Add(new EconomyEntry
                {
                    description = $"{hero.heroName} - Salary (Season)",
                    amount = -seasonalSalary
                });
                totalExpenses += seasonalSalary;
            }

            // Add income note (missions will vary)
            // For now, show expected income range based on typical mission rewards
            entries.Add(new EconomyEntry
            {
                description = "Mission Rewards (Estimated)",
                amount = 60 // Average of D-S rank rewards
            });
            totalIncome += 60;

            // Calculate net balance
            int netBalance = totalIncome - totalExpenses;

            // Clear existing slots
            ClearEntrySlots();

            // Create entry slots dynamically
            if (entrySlotPrefab != null && entryContainer != null)
            {
                foreach (EconomyEntry entry in entries)
                {
                    Components.EconomyEntrySlot slot = Instantiate(entrySlotPrefab, entryContainer);
                    slot.Setup(entry.description, entry.amount);
                    instantiatedSlots.Add(slot);
                }
            }
            else
            {
                Debug.LogWarning("[EconomyScreen] Entry slot prefab or container is not assigned!");
            }

            // Update balance text
            if (balanceText != null)
            {
                string sign = netBalance >= 0 ? "+" : "";
                balanceText.text = $"Net Balance (Season): {sign}{netBalance} Gold";
                balanceText.color = netBalance >= 0 ? Color.green : Color.red;
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Handle back button clicked
        /// </summary>
        private void HandleBackClicked()
        {
            OnBackClicked?.Invoke();
            Hide();
        }

        /// <summary>
        /// Hide the screen
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Clear all instantiated entry slots
        /// </summary>
        private void ClearEntrySlots()
        {
            foreach (Components.EconomyEntrySlot slot in instantiatedSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            instantiatedSlots.Clear();
        }

        /// <summary>
        /// Simple struct to represent an economy entry
        /// </summary>
        private struct EconomyEntry
        {
            public string description;
            public int amount;
        }
    }
}
