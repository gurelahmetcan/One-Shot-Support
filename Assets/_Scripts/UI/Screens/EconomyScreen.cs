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
        [SerializeField] private Components.EconomyEntrySlot[] entrySlots;

        [Header("UI References")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI balanceText;

        // Events
        public event Action OnBackClicked;

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
                int seasonalSalary = hero.dailySalary * 4; // 4 turns per season
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

            // Display entries in slots
            for (int i = 0; i < entrySlots.Length; i++)
            {
                if (i < entries.Count)
                {
                    entrySlots[i].Setup(entries[i].description, entries[i].amount);
                    entrySlots[i].gameObject.SetActive(true);
                }
                else
                {
                    entrySlots[i].Clear();
                    entrySlots[i].gameObject.SetActive(false);
                }
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
        /// Simple struct to represent an economy entry
        /// </summary>
        private struct EconomyEntry
        {
            public string description;
            public int amount;
        }
    }
}
