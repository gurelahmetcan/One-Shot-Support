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
    /// Main consultation screen where player equips heroes
    /// </summary>
    public class ConsultationScreen : MonoBehaviour
    {
        [Header("Hero Display")]
        public Image heroPortrait;
        public TextMeshProUGUI heroNameText;
        public TextMeshProUGUI heroTierText;
        public TextMeshProUGUI heroPerkText;
        public TextMeshProUGUI heroSlotsText;

        [Header("Monster Display")]
        public Image monsterSprite;
        public TextMeshProUGUI monsterNameText;
        public TextMeshProUGUI monsterWeaknessText;
        public TextMeshProUGUI monsterDifficultyText;

        [Header("Equipment")]
        public ItemSlot[] equipmentSlots; // Equipment slots for hero
        public Transform inventoryGrid; // Parent for inventory items
        public GameObject draggableItemPrefab; // Prefab for draggable items

        [Header("UI Components")]
        public ConfidenceMeter confidenceMeter;
        public ItemTooltip itemTooltip;
        public Button sendHeroButton;

        [Header("Inventory Visibility")]
        public GameObject inventoryPanel; // Panel containing inventory
        public Button toggleInventoryButton; // Button to show/hide inventory

        private HeroResult currentHeroResult;
        private List<DraggableItem> inventoryItems = new List<DraggableItem>();
        private bool inventoryVisible = false;

        private void Awake()
        {
            // Setup button listeners
            if (sendHeroButton != null)
                sendHeroButton.onClick.AddListener(OnSendHeroClicked);

            if (toggleInventoryButton != null)
                toggleInventoryButton.onClick.AddListener(ToggleInventory);

            // Setup equipment slots
            foreach (var slot in equipmentSlots)
            {
                slot.isEquipmentSlot = true;
                slot.OnItemPlaced += OnItemEquipped;
                slot.OnItemRemoved += OnItemUnequipped;
            }

            // Hide inventory by default
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);
        }

        /// <summary>
        /// Setup consultation for a new hero
        /// </summary>
        public void SetupConsultation(HeroResult heroResult, List<ItemData> availableItems)
        {
            currentHeroResult = heroResult;

            // Display hero info
            DisplayHero(heroResult.hero);

            // Display monster info
            DisplayMonster(heroResult.monster);

            // Clear previous equipment
            ClearEquipment();

            // Setup inventory
            SetupInventory(availableItems, heroResult.monster.weakness);

            // Setup equipment slots based on hero
            SetupEquipmentSlots(heroResult.hero);

            // Reset confidence meter
            if (confidenceMeter != null)
            {
                // Check if meter should be hidden (Honest perk)
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
        /// </summary>
        private void SetupInventory(List<ItemData> availableItems, ItemCategory monsterWeakness)
        {
            // Clear existing inventory
            foreach (var item in inventoryItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            inventoryItems.Clear();

            // Create draggable items
            foreach (var itemData in availableItems)
            {
                if (itemData == null) continue;

                GameObject itemObj = Instantiate(draggableItemPrefab, inventoryGrid);
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

                    inventoryItems.Add(draggableItem);
                }
            }
        }

        /// <summary>
        /// Setup equipment slots based on hero's effective slots
        /// </summary>
        private void SetupEquipmentSlots(HeroData hero)
        {
            int effectiveSlots = hero.GetEffectiveSlots();

            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                // Enable/disable slots based on hero's slot count
                equipmentSlots[i].gameObject.SetActive(i < effectiveSlots);
            }
        }

        /// <summary>
        /// Clear all equipped items
        /// </summary>
        private void ClearEquipment()
        {
            foreach (var slot in equipmentSlots)
            {
                slot.RemoveItem();
            }
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
        /// Toggle inventory visibility
        /// </summary>
        private void ToggleInventory()
        {
            inventoryVisible = !inventoryVisible;

            if (inventoryPanel != null)
                inventoryPanel.SetActive(inventoryVisible);
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

            // Hide this screen (will be handled by UIManager)
        }
    }
}
