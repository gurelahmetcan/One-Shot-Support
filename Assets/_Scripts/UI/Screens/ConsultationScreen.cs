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
using OneShotSupport.Tutorial;

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
        [SerializeField] private GameObject heroPanel;
        [SerializeField] private Button heroPanelButton;
        [SerializeField] private Sprite heroPanelActiveSprite;
        [SerializeField] private Sprite heroPanelInactiveSprite;
        [SerializeField] private Image heroCardImage;

        [Header("Main View - Monster Display")]
        public Image monsterSprite;
        public TextMeshProUGUI monsterNameText;
        public TextMeshProUGUI monsterWeaknessText;
        public TextMeshProUGUI monsterDifficultyText;
        public TextMeshProUGUI monsterBountyText;
        [SerializeField] private GameObject monsterPanel;
        [SerializeField] private Button monsterPanelButton;
        [SerializeField] private Sprite monsterPanelActiveSprite;
        [SerializeField] private Sprite monsterPanelInactiveSprite;
        [SerializeField] private Image monsterWeaknessImage;

        [Header("Main View - Buttons")]
        public Button openInventoryButton;
        public Button sendHeroButton;

        [Header("Inventory View - Slots")]
        [Tooltip("Inventory slots (manually created in scene, e.g., 6 slots)")]
        public ItemSlot[] inventorySlots;

        [Tooltip("Equipment slots for hero")]
        public ItemSlot[] equipmentSlots;

        [Header("Equipment Background")]
        [Tooltip("Background image for equipment slots area")]
        public Image equipmentBackgroundImage;

        [Tooltip("Background sprites for different slot counts [1-slot, 2-slot, 3-slot, 4-slot]")]
        public Sprite[] equipmentBackgroundSprites = new Sprite[4];

        [Header("Inventory View - Prefabs")]
        public GameObject draggableItemPrefab;
        public Button closeInventoryButton;

        [Header("UI Components")]
        public ConfidenceMeter confidenceMeter;
        public ConfidenceMeter confidenceMeterMain;
        public ItemTooltip itemTooltip;
        public EquipmentDisplay equipmentDisplay;

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

            if (heroPanelButton != null)
            {
                heroPanelButton.onClick.AddListener(OnHeroPanelClicked);
            }
            
            if (monsterPanelButton != null)
            {
                monsterPanelButton.onClick.AddListener(OnMonsterPanelClicked);
            }

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

            // Play hero voiceline when they enter the store
            if (heroResult.hero.heroVoiceline != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHeroVoiceline(heroResult.hero.heroVoiceline);
            }

            // Check if this is a new day (reset inventory)
            int dayNumber = GameManager.Instance?.CurrentDayNumber ?? 0;
            bool isNewDay = dayNumber != currentDay;

            if (isNewDay)
            {
                currentDay = dayNumber;
                isInventorySetupForDay = false;
            }

            OnMonsterPanelClicked();

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
                    confidenceMeterMain.Hide();
                }
                else
                {
                    confidenceMeter.Show();
                    confidenceMeterMain.Show();
                    UpdateConfidence();
                }
            }

            // Start in main view
            ShowMainView();

            // Tutorial: Advance to ExamineMonster step when consultation screen opens
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
            {
                var currentStep = TutorialManager.Instance.GetCurrentStep();
                if (currentStep == TutorialStep.DayStartHint || currentStep == TutorialStep.None)
                {
                    TutorialManager.Instance.AdvanceToStep(TutorialStep.ExamineMonster);
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

            if (heroCardImage != null)
                heroCardImage.sprite = hero.characterCard;

            if (heroNameText != null)
                heroNameText.text = hero.heroName;

            if (heroTierText != null)
                heroTierText.text = $"{hero.tier}";

            if (heroPerkText != null)
                heroPerkText.text = hero.perk.ToString();
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

            if (monsterWeaknessImage != null)
                monsterWeaknessImage.sprite = monster.categorySprite;

            if (monsterDifficultyText != null)
                monsterDifficultyText.text = $"Difficulty: -{monster.difficultyPenalty}%";

            if (monsterBountyText != null)
                monsterBountyText.text = $"{GameManager.Instance.goldManager.GetMonsterReward(monster.rank)}";
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

            // Show/hide equipment slots based on hero's effective slot count
            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                equipmentSlots[i].gameObject.SetActive(i < effectiveSlots);
            }

            // Change equipment background sprite based on slot count
            if (equipmentBackgroundImage != null && equipmentBackgroundSprites != null)
            {
                // Clamp to valid range (1-4 slots)
                int slotIndex = Mathf.Clamp(effectiveSlots, 1, 4) - 1; // Convert to 0-based index

                if (slotIndex >= 0 && slotIndex < equipmentBackgroundSprites.Length)
                {
                    Sprite backgroundSprite = equipmentBackgroundSprites[slotIndex];
                    if (backgroundSprite != null)
                    {
                        equipmentBackgroundImage.sprite = backgroundSprite;
                    }
                }
            }

            // Update equipment display on main panel
            UpdateEquipmentDisplay();
        }

        /// <summary>
        /// Clear all equipped items
        /// Called between heroes - DESTROYS equipped items (they are consumed)
        /// </summary>
        private void ClearEquipment()
        {
            foreach (var slot in equipmentSlots)
            {
                if (!slot.IsEmpty())
                {
                    // Destroy the equipped item (it's been used by the hero)
                    var item = slot.CurrentItem;
                    slot.RemoveItem();

                    if (item != null)
                    {
                        // Remove from tracking list
                        currentItems.Remove(item);
                        Destroy(item.gameObject);
                    }
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
            UpdateEquipmentDisplay();

            // Tutorial: Complete DragItem step when player equips an item
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
            {
                if (TutorialManager.Instance.GetCurrentStep() == TutorialStep.DragItem)
                {
                    TutorialManager.Instance.CompleteCurrentStep(); // Advances to CheckConfidence
                }
            }
        }

        /// <summary>
        /// Called when an item is unequipped
        /// </summary>
        private void OnItemUnequipped(ItemSlot slot)
        {
            UpdateConfidence();
            UpdateEquipmentDisplay();
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
            confidenceMeterMain.UpdateConfidence(successChance);
        }

        /// <summary>
        /// Update equipment display on main panel with currently equipped items
        /// </summary>
        private void UpdateEquipmentDisplay()
        {
            if (equipmentDisplay == null || currentHeroResult == null) return;

            // Get currently equipped items (or null for empty slots)
            ItemData[] equippedItems = new ItemData[4];
            for (int i = 0; i < equipmentSlots.Length && i < equippedItems.Length; i++)
            {
                equippedItems[i] = equipmentSlots[i].IsEmpty() ? null : equipmentSlots[i].GetItemData();
            }

            // Get active slot count for current hero
            int activeSlots = currentHeroResult.hero.GetEffectiveSlots();

            // Update the display
            equipmentDisplay.UpdateDisplay(equippedItems, activeSlots);
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

            // Tutorial: Advance to CheckInventory and then DragItem when opening inventory
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
            {
                var currentStep = TutorialManager.Instance.GetCurrentStep();
                if (currentStep == TutorialStep.ExamineMonster)
                {
                    // Advance through CheckInventory to DragItem
                    TutorialManager.Instance.AdvanceToStep(TutorialStep.CheckInventory);
                }
                else if (currentStep == TutorialStep.CheckInventory)
                {
                    // Auto-advance to DragItem step
                    TutorialManager.Instance.CompleteCurrentStep();
                }
            }
        }

        /// <summary>
        /// Called when Send Hero button is clicked
        /// </summary>
        private void OnSendHeroClicked()
        {
            // Tutorial: Check if SendHero action is allowed
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
            {
                if (!TutorialManager.Instance.IsActionAllowed("SendHero"))
                {
                    Debug.Log("[Tutorial] SendHero action blocked - complete tutorial steps first");
                    return;
                }

                // Complete tutorial when player sends hero
                if (TutorialManager.Instance.GetCurrentStep() == TutorialStep.SendHero)
                {
                    TutorialManager.Instance.CompleteCurrentStep(); // Completes tutorial
                }
            }

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

        /// <summary>
        /// Called when Hero Panel button is clicked
        /// </summary>
        private void OnHeroPanelClicked()
        {
            if (heroPanel != null)
            {
                heroPanel.SetActive(true);
                heroPanelButton.image.sprite = heroPanelActiveSprite;
            }

            if (monsterPanel != null)
            {
                monsterPanel.SetActive(false);
                monsterPanelButton.image.sprite = monsterPanelInactiveSprite;
            }

            // Tutorial: Complete UnderstandHero step when player clicks hero panel
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
            {
                if (TutorialManager.Instance.GetCurrentStep() == TutorialStep.UnderstandHero)
                {
                    TutorialManager.Instance.CompleteCurrentStep(); // Advances to SendHero
                }
            }
        }
        
        /// <summary>
        /// Called when Hero Panel button is clicked
        /// </summary>
        private void OnMonsterPanelClicked()
        {
            if (heroPanel != null)
            {
                heroPanel.SetActive(false);
                heroPanelButton.image.sprite = heroPanelInactiveSprite;
            }

            if (monsterPanel != null)
            {
                monsterPanel.SetActive(true);
                monsterPanelButton.image.sprite = monsterPanelActiveSprite;
            }
        }

        /// <summary>
        /// Get the number of leftover items in inventory at day end
        /// </summary>
        public int GetLeftoverItemCount()
        {
            int count = 0;
            foreach (var slot in inventorySlots)
            {
                if (!slot.IsEmpty())
                    count++;
            }
            return count;
        }
    }
}
