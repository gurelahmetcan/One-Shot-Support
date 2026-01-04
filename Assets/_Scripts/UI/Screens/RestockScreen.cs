using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.Core;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Screen for purchasing item crates at the start of each day
    /// Players can buy up to 2 different crates
    /// </summary>
    public class RestockScreen : MonoBehaviour
    {
        [Header("Crate Buttons")]
        [SerializeField] private Button cheapCrateButton;
        [SerializeField] private Button mediumCrateButton;
        [SerializeField] private Button premiumCrateButton;

        [Header("Crate Info Texts")]
        [SerializeField] private TextMeshProUGUI cheapCrateText;
        [SerializeField] private TextMeshProUGUI mediumCrateText;
        [SerializeField] private TextMeshProUGUI premiumCrateText;

        [Header("Category Selection (Premium Crate)")]
        [SerializeField] private GameObject categorySelectionPanel;
        [SerializeField] private Button[] categoryButtons; // 4 buttons for Hygiene, Magic, Catering, Lighting

        [Header("Continue Button")]
        [SerializeField] private Button continueButton;

        [Header("References")]
        [SerializeField] private ItemDatabase itemDatabase;

        // Events
        public event Action<List<ItemData>> OnCratesPurchased;

        // State
        private GoldManager goldManager;
        private List<ItemData> purchasedItems = new List<ItemData>();
        private HashSet<CrateType> purchasedCrates = new HashSet<CrateType>();
        private ItemCategory[] mediumCrateCategories = new ItemCategory[2];
        private const int MAX_CRATES = 2;

        private void Awake()
        {
            // Setup button listeners
            cheapCrateButton?.onClick.AddListener(() => OnCrateButtonClicked(CrateType.Cheap));
            mediumCrateButton?.onClick.AddListener(() => OnCrateButtonClicked(CrateType.Medium));
            premiumCrateButton?.onClick.AddListener(() => OnCrateButtonClicked(CrateType.Premium));
            continueButton?.onClick.AddListener(OnContinueClicked);

            // Setup category selection buttons
            if (categoryButtons != null)
            {
                for (int i = 0; i < categoryButtons.Length && i < 4; i++)
                {
                    int categoryIndex = i;
                    categoryButtons[i]?.onClick.AddListener(() => OnCategorySelected((ItemCategory)categoryIndex));
                }
            }

            if (categorySelectionPanel != null)
                categorySelectionPanel.SetActive(false);
        }

        private void Start()
        {
            goldManager = FindObjectOfType<GoldManager>();
            if (goldManager == null)
                Debug.LogError("[RestockScreen] GoldManager not found!");
        }

        /// <summary>
        /// Setup the restock screen for a new day
        /// </summary>
        public void Setup()
        {
            purchasedItems.Clear();
            purchasedCrates.Clear();

            // Generate random categories for medium crate
            mediumCrateCategories[0] = (ItemCategory)UnityEngine.Random.Range(0, 4);
            do
            {
                mediumCrateCategories[1] = (ItemCategory)UnityEngine.Random.Range(0, 4);
            } while (mediumCrateCategories[1] == mediumCrateCategories[0]);

            UpdateUI();
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Update all UI elements
        /// </summary>
        private void UpdateUI()
        {
            if (goldManager == null) return;

            int cratesPurchased = purchasedCrates.Count;
            bool canBuyMore = cratesPurchased < MAX_CRATES;

            // Cheap crate
            UpdateCrateButton(cheapCrateButton, cheapCrateText, CrateType.Cheap,
                "Cheap Crate\n3 Random Items", canBuyMore);

            // Medium crate
            string mediumDesc = $"Medium Crate\n3 Items from:\n{mediumCrateCategories[0]} or {mediumCrateCategories[1]}";
            UpdateCrateButton(mediumCrateButton, mediumCrateText, CrateType.Medium, mediumDesc, canBuyMore);

            // Premium crate
            UpdateCrateButton(premiumCrateButton, premiumCrateText, CrateType.Premium,
                "Premium Crate\nSelect Category\n3 Items", canBuyMore);
            
            // Continue button
            if (continueButton != null)
                continueButton.interactable = cratesPurchased > 0;
        }

        /// <summary>
        /// Update individual crate button state
        /// </summary>
        private void UpdateCrateButton(Button button, TextMeshProUGUI text, CrateType crateType, string description, bool canBuyMore)
        {
            if (button == null || text == null) return;

            int cost = goldManager.GetCrateCost(crateType);
            bool alreadyPurchased = purchasedCrates.Contains(crateType);
            bool canAfford = goldManager.CanAffordCrate(crateType);

            // Button state
            button.interactable = !alreadyPurchased && canBuyMore && canAfford;

            // Text
            string statusText = "";
            if (alreadyPurchased)
                statusText = "\n[PURCHASED]";
            else if (!canBuyMore)
                statusText = "\n[MAX CRATES]";
            else if (!canAfford)
                statusText = "\n[NOT ENOUGH GOLD]";

            text.text = $"{description}\n\nCost: {cost}g{statusText}";
        }

        /// <summary>
        /// Handle crate button click
        /// </summary>
        private void OnCrateButtonClicked(CrateType crateType)
        {
            if (purchasedCrates.Count >= MAX_CRATES)
            {
                Debug.LogWarning("[RestockScreen] Already purchased maximum crates!");
                return;
            }

            if (purchasedCrates.Contains(crateType))
            {
                Debug.LogWarning($"[RestockScreen] Already purchased {crateType} crate!");
                return;
            }

            // Premium crate needs category selection
            if (crateType == CrateType.Premium)
            {
                ShowCategorySelection();
                return;
            }

            // Try to purchase
            PurchaseCrate(crateType, null);
        }

        /// <summary>
        /// Show category selection panel for premium crate
        /// </summary>
        private void ShowCategorySelection()
        {
            if (categorySelectionPanel != null)
                categorySelectionPanel.SetActive(true);

            // Update category button labels
            if (categoryButtons != null)
            {
                for (int i = 0; i < categoryButtons.Length && i < 4; i++)
                {
                    var buttonText = categoryButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        ItemCategory category = (ItemCategory)i;
                        buttonText.text = category.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Handle category selection for premium crate
        /// </summary>
        private void OnCategorySelected(ItemCategory category)
        {
            if (categorySelectionPanel != null)
                categorySelectionPanel.SetActive(false);

            PurchaseCrate(CrateType.Premium, category);
        }

        /// <summary>
        /// Purchase a crate and generate items
        /// </summary>
        private void PurchaseCrate(CrateType crateType, ItemCategory? selectedCategory)
        {
            if (goldManager == null || itemDatabase == null) return;

            // Try to spend gold
            int cost = goldManager.GetCrateCost(crateType);
            if (!goldManager.TrySpendGold(cost))
                return;

            // Mark as purchased
            purchasedCrates.Add(crateType);

            // Generate 3 items based on crate type
            List<ItemData> crateItems = GenerateCrateItems(crateType, selectedCategory);
            purchasedItems.AddRange(crateItems);

            Debug.Log($"[RestockScreen] Purchased {crateType} crate for {cost}g. Got {crateItems.Count} items.");

            // Update UI
            UpdateUI();
        }

        /// <summary>
        /// Generate items for a purchased crate
        /// </summary>
        private List<ItemData> GenerateCrateItems(CrateType crateType, ItemCategory? selectedCategory)
        {
            List<ItemData> items = new List<ItemData>();

            for (int i = 0; i < 3; i++)
            {
                ItemData item = null;

                switch (crateType)
                {
                    case CrateType.Cheap:
                        // Random item from any category
                        item = itemDatabase.GetRandomItem();
                        break;

                    case CrateType.Medium:
                        // Random item from one of the two shown categories
                        ItemCategory randomCategory = mediumCrateCategories[UnityEngine.Random.Range(0, 2)];
                        item = itemDatabase.GetRandomItemOfCategory(randomCategory);
                        break;

                    case CrateType.Premium:
                        // Item from selected category
                        if (selectedCategory.HasValue)
                            item = itemDatabase.GetRandomItemOfCategory(selectedCategory.Value);
                        break;
                }

                if (item != null)
                    items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// Continue to consultation phase
        /// </summary>
        private void OnContinueClicked()
        {
            if (purchasedItems.Count == 0)
            {
                Debug.LogWarning("[RestockScreen] No items purchased!");
                return;
            }

            OnCratesPurchased?.Invoke(purchasedItems);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Hide the screen
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
