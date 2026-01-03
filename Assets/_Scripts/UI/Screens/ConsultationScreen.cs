using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using OneShotSupport.Core;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;
using OneShotSupport.UI.Components;
using OneShotSupport.UI.DragDrop;
using OneShotSupport.Utils;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Main consultation screen with two separate panels:
    /// 1. Main View: Shows hero vs monster info
    /// 2. Inventory View: Shows inventory slots + equipment slots
    ///
    /// IMPORTANT: Inventory persists across all heroes in a day, only resets on new day
    /// </summary>
    public class ConsultationScreen : MonoBehaviour
    {
        [Header("Panel Management")]
        [Tooltip("Main view panel (hero/monster display)")]
        public GameObject mainViewPanel;

        [Tooltip("Inventory view panel (inventory slots + equipment slots)")]
        public GameObject inventoryViewPanel;

        [Header("Main View - Hero Display")]
        public Image heroPortrait;
        public TextMeshProUGUI heroNameText;
        public TextMeshProUGUI heroTierText;
        public TextMeshProUGUI heroPerkText;
        public TextMeshProUGUI heroSlotsText;

        [Header("Main View - Monster Display")]
        public Image monsterSprite;
        public TextMeshProUGUI monsterNameText;
        public TextMeshProUGUI monsterWeaknessText;
        public TextMeshProUGUI monsterDifficultyText;

        [Header("Main View - Buttons")]
        public Button openInventoryButton;
        public Button sendHeroButton;

        [Header("Inventory View - Slots")]
        [Tooltip("Inventory slots (manually created in scene, e.g., 6 slots)")]
        public ItemSlot[] inventorySlots;

        [Tooltip("Equipment slots for hero")]
        public ItemSlot[] equipmentSlots;

        [Header("Inventory View - Prefabs")]
        public GameObject draggableItemPrefab;
        public Button closeInventoryButton;

        [Header("UI Components")]
        public ConfidenceMeter confidenceMeter;
        public ItemTooltip itemTooltip;

        private HeroResult currentHeroResult;
        private List<DraggableItem> currentItems = new List<DraggableItem>();
        private bool isInventorySetupForDay = false; // Track if inventory is setup for current day
        private int currentDay = -1; // Track current day number

        private void Awake()
        {
            // Setup button listeners
            if (sendHeroButton != null)
                sendHeroButton.onClick.AddListener(OnSendHeroClicked);

            if (openInventoryButton != null)
                openInventoryButton.onClick.AddListener(ShowInventoryView);

            if (closeInventoryButton != null)
                closeInventoryButton.onClick.AddListener(ShowMainView);

            // Setup equipment slots
            foreach (var slot in equipmentSlots)
            {
                slot.isEquipmentSlot = true;
                slot.OnItemPlaced += OnItemEquipped;
                slot.OnItemRemoved += OnItemUnequipped;
            }

            // Setup inventory slots
            foreach (var slot in inventorySlots)
            {
                slot.isEquipmentSlot = false;
                slot.OnItemRemoved += OnItemUnequipped;
            }

            // Show main view by default
            ShowMainView();
        }

        /// <summary>
        /// Setup consultation for a new hero
        /// IMPORTANT: Inventory persists across heroes, only equipment is cleared
        /// </summary>
        public void SetupConsultation(HeroResult heroResult, List<ItemData> availableItems)
        {
            currentHeroResult = heroResult;

            // Check if this is a new day (reset inventory)
            int dayNumber = GameManager.Instance?.CurrentDayNumber ?? 0;
            bool isNewDay = dayNumber != currentDay;

            if (isNewDay)
            {
                currentDay = dayNumber;
                isInventorySetupForDay = false;
            }

            // Display hero info
            DisplayHero(heroResult.hero);

            // Display monster info
            DisplayMonster(heroResult.monster);

            // Clear previous equipment (always clear between heroes)
            ClearEquipment();

            // Setup inventory ONLY on first hero of the day
            if (!isInventorySetupForDay)
            {
                ClearInventory();
                SetupInventory(availableItems, heroResult.monster.weakness);
                isInventorySetupForDay = true;
            }

            // Setup equipment slots based on hero
            SetupEquipmentSlots(heroResult.hero);

            // Reset confidence meter
            if (confidenceMeter != null)
            {
                if (PerkModifier.HidesConfidenceMeter(heroResult.hero.perk))
                {
                    confidenceMeter.Hide();
                }
                else
                {
                    confidenceMeter.Show();
                    UpdateConfidence();
                }
            }

            // Start in main view
            ShowMainView();
        }

        /// <summary>
        /// Display hero information
        /// </summary>
        private void DisplayHero(HeroData hero)
        {
            if (heroPortrait != null)
                heroPortrait.sprite = hero.portrait;

            if (heroNameText != null)
                heroNameText.text = hero.heroName;

            if (heroTierText != null)
                heroTierText.text = $"Tier: {hero.tier}";

            if (heroPerkText != null)
                heroPerkText.text = PerkModifier.GetDescription(hero.perk);

            if (heroSlotsText != null)
                heroSlotsText.text = $"Equipment Slots: {hero.GetEffectiveSlots()}";
        }

        /// <summary>
        /// Display monster information
        /// </summary>
        private void DisplayMonster(MonsterData monster)
        {
            if (monsterSprite != null)
                monsterSprite.sprite = monster.sprite;

            if (monsterNameText != null)
                monsterNameText.text = monster.monsterName;

            if (monsterWeaknessText != null)
                monsterWeaknessText.text = $"Weakness: {monster.weakness}";

            if (monsterDifficultyText != null)
                monsterDifficultyText.text = $"Difficulty: -{monster.difficultyPenalty}%";
        }

        /// <summary>
        /// Setup inventory with available items
        /// ONLY called once per day (first hero)
        /// </summary>
        private void SetupInventory(List<ItemData> availableItems, ItemCategory monsterWeakness)
        {
            // Clear existing items
            ClearInventory();

            // Create items and place them in inventory slots
            for (int i = 0; i < availableItems.Count && i < inventorySlots.Length; i++)
            {
                var itemData = availableItems[i];
                if (itemData == null) continue;

                // Create draggable item
                GameObject itemObj = Instantiate(draggableItemPrefab);
                DraggableItem draggableItem = itemObj.GetComponent<DraggableItem>();

                if (draggableItem != null)
                {
                    draggableItem.Initialize(itemData);

                    // Setup tooltip events
                    draggableItem.OnHoverEnter += (item) => {
                        if (itemTooltip != null)
                            itemTooltip.Show(item.itemData, monsterWeakness);
                    };

                    draggableItem.OnHoverExit += (item) => {
                        if (itemTooltip != null)
                            itemTooltip.Hide();
                    };

                    // Place item in inventory slot
                    inventorySlots[i].PlaceItem(draggableItem);

                    currentItems.Add(draggableItem);
                }
            }

            Debug.Log($"[ConsultationScreen] Inventory setup with {currentItems.Count} items for Day {currentDay}");
        }

        /// <summary>
        /// Setup equipment slots based on hero's effective slots
        /// </summary>
        private void SetupEquipmentSlots(HeroData hero)
        {
            int effectiveSlots = hero.GetEffectiveSlots();

            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                equipmentSlots[i].gameObject.SetActive(i < effectiveSlots);
            }
        }

        /// <summary>
        /// Clear all equipped items
        /// Called between heroes - returns items to inventory
        /// </summary>
        private void ClearEquipment()
        {
            foreach (var slot in equipmentSlots)
            {
                if (!slot.IsEmpty())
                {
                    // Return item to first empty inventory slot
                    ReturnItemToInventory(slot.CurrentItem);
                    slot.RemoveItem();
                }
            }
        }

        /// <summary>
        /// Clear all inventory items
        /// ONLY called when starting a new day
        /// </summary>
        private void ClearInventory()
        {
            // Destroy all current items
            foreach (var item in currentItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            currentItems.Clear();

            // Clear all inventory slots
            foreach (var slot in inventorySlots)
            {
                slot.RemoveItem();
            }

            Debug.Log("[ConsultationScreen] Inventory cleared");
        }

        /// <summary>
        /// Return an item to the first empty inventory slot
        /// </summary>
        private void ReturnItemToInventory(DraggableItem item)
        {
            if (item == null) return;

            // Find first empty inventory slot
            foreach (var slot in inventorySlots)
            {
                if (slot.IsEmpty())
                {
                    slot.PlaceItem(item);
                    return;
                }
            }

            // If no empty slot found, destroy the item (shouldn't happen)
            Debug.LogWarning("No empty inventory slot found to return item!");
            Destroy(item.gameObject);
        }

        /// <summary>
        /// Called when an item is equipped
        /// </summary>
        private void OnItemEquipped(ItemSlot slot, ItemData item)
        {
            UpdateConfidence();
        }

        /// <summary>
        /// Called when an item is unequipped
        /// </summary>
        private void OnItemUnequipped(ItemSlot slot)
        {
            UpdateConfidence();
        }

        /// <summary>
        /// Update confidence meter based on current equipment
        /// </summary>
        private void UpdateConfidence()
        {
            if (currentHeroResult == null || confidenceMeter == null) return;

            // Get equipped items
            List<ItemData> equippedItems = equipmentSlots
                .Where(slot => !slot.IsEmpty())
                .Select(slot => slot.GetItemData())
                .ToList();

            // Calculate success chance
            int successChance = OneShotCalculator.CalculateSuccessChance(
                currentHeroResult.hero,
                currentHeroResult.monster,
                equippedItems,
                GameManager.Instance?.CurrentDay?.inspiringBonus ?? 0
            );

            // Update meter
            confidenceMeter.UpdateConfidence(successChance);
        }

        /// <summary>
        /// Show the main view panel (hero vs monster)
        /// </summary>
        private void ShowMainView()
        {
            if (mainViewPanel != null)
                mainViewPanel.SetActive(true);

            if (inventoryViewPanel != null)
                inventoryViewPanel.SetActive(false);

            if (itemTooltip != null)
                itemTooltip.Hide();
        }

        /// <summary>
        /// Show the inventory view panel (inventory + equipment)
        /// </summary>
        private void ShowInventoryView()
        {
            if (mainViewPanel != null)
                mainViewPanel.SetActive(false);

            if (inventoryViewPanel != null)
                inventoryViewPanel.SetActive(true);

            UpdateConfidence();
        }

        /// <summary>
        /// Called when Send Hero button is clicked
        /// </summary>
        private void OnSendHeroClicked()
        {
            // Get equipped items
            List<ItemData> equippedItems = equipmentSlots
                .Where(slot => !slot.IsEmpty())
                .Select(slot => slot.GetItemData())
                .ToList();

            // Send to game manager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteConsultation(equippedItems);
            }

            if (itemTooltip != null)
                itemTooltip.Hide();
        }
    }
}
