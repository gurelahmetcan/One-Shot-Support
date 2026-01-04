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

        [Header("Item Preview Slots")]
        [SerializeField] private Image[] itemPreviewSlots = new Image[6]; // Bottom slots to show purchased items

        [Header("Continue Button")]
        [SerializeField] private Button continueButton;

        [Header("References")]
        [SerializeField] private ItemDatabase itemDatabase;

        // Events
        public event Action<List<ItemData>> OnCratesPurchased;

        // State
        private List<ItemData> purchasedItems = new List<ItemData>();
        private HashSet<CrateType> purchasedCrates = new HashSet<CrateType>();
        private ItemCategory mediumCrateCategory; // Single random category for medium crate
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

        /// <summary>
        /// Setup the restock screen for a new day
        /// </summary>
        public void Setup()
        {
            purchasedItems.Clear();
            purchasedCrates.Clear();

            // Generate ONE random category for medium crate
            mediumCrateCategory = (ItemCategory)UnityEngine.Random.Range(0, 4);

            // Clear item preview slots
            ClearItemPreview();

            UpdateUI();
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Update all UI elements
        /// </summary>
        private void UpdateUI()
        {
            int cratesPurchased = purchasedCrates.Count;
            bool canBuyMore = cratesPurchased < MAX_CRATES;

            // Cheap crate
            UpdateCrateButton(cheapCrateButton, cheapCrateText, CrateType.Cheap,
                "3 Random Items\n (Random Category)", canBuyMore);

            // Medium crate (shows single random category)
            string mediumDesc = $"3 Random Items\n ({mediumCrateCategory} Category)";
            UpdateCrateButton(mediumCrateButton, mediumCrateText, CrateType.Medium, mediumDesc, canBuyMore);

            // Premium crate
            UpdateCrateButton(premiumCrateButton, premiumCrateText, CrateType.Premium,
                "3 Random Items\n(Select Category)", canBuyMore);
            
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

            int cost = GameManager.Instance.goldManager.GetCrateCost(crateType);
            bool alreadyPurchased = purchasedCrates.Contains(crateType);
            bool canAfford = GameManager.Instance.goldManager.CanAffordCrate(crateType);

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

            text.text = $"{description} {statusText}";
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
            if (itemDatabase == null) return;

            // Try to spend gold
            int cost = GameManager.Instance.goldManager.GetCrateCost(crateType);
            if (!GameManager.Instance.goldManager.TrySpendGold(cost))
                return;

            // Mark as purchased
            purchasedCrates.Add(crateType);

            // Generate 3 items based on crate type
            List<ItemData> crateItems = GenerateCrateItems(crateType, selectedCategory);
            purchasedItems.AddRange(crateItems);

            // Play crate purchase sound
            if (Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlayCratePurchaseSound();
            }

            Debug.Log($"[RestockScreen] Purchased {crateType} crate for {cost}g. Got {crateItems.Count} items.");

            // Update item preview display
            UpdateItemPreview();

            // Update UI
            UpdateUI();
        }

        /// <summary>
        /// Generate items for a purchased crate
        /// </summary>
        private List<ItemData> GenerateCrateItems(CrateType crateType, ItemCategory? selectedCategory)
        {
            List<ItemData> items = new List<ItemData>();
            HashSet<ItemCategory> usedCategories = new HashSet<ItemCategory>(); // Track used categories for cheap crate

            for (int i = 0; i < 3; i++)
            {
                ItemData item = null;

                switch (crateType)
                {
                    case CrateType.Cheap:
                        // Get items from DIFFERENT categories
                        // Try to get an item from a category we haven't used yet
                        int attempts = 0;
                        const int maxAttempts = 20; // Prevent infinite loop

                        do
                        {
                            item = itemDatabase.GetRandomItem();
                            attempts++;
                        }
                        while (item != null && usedCategories.Contains(item.category) && attempts < maxAttempts);

                        if (item != null)
                        {
                            usedCategories.Add(item.category);
                        }
                        break;

                    case CrateType.Medium:
                        // Item from the single shown category
                        item = itemDatabase.GetRandomItemOfCategory(mediumCrateCategory);
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

        /// <summary>
        /// Clear all item preview slots
        /// </summary>
        private void ClearItemPreview()
        {
            if (itemPreviewSlots == null) return;

            for (int i = 0; i < itemPreviewSlots.Length; i++)
            {
                if (itemPreviewSlots[i] != null)
                {
                    itemPreviewSlots[i].sprite = null;
                    itemPreviewSlots[i].enabled = false;
                }
            }
        }

        /// <summary>
        /// Update item preview slots to show purchased items
        /// </summary>
        private void UpdateItemPreview()
        {
            if (itemPreviewSlots == null) return;

            // Display all purchased items in the preview slots (max 6)
            for (int i = 0; i < itemPreviewSlots.Length; i++)
            {
                if (itemPreviewSlots[i] == null) continue;

                if (i < purchasedItems.Count && purchasedItems[i] != null)
                {
                    // Show item icon
                    itemPreviewSlots[i].sprite = purchasedItems[i].icon;
                    itemPreviewSlots[i].enabled = true;
                }
                else
                {
                    // Empty slot
                    itemPreviewSlots[i].sprite = null;
                    itemPreviewSlots[i].enabled = false;
                }
            }
        }
    }
}
