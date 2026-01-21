using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.Core;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;
using OneShotSupport.UI.Components;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Data structure for tracking each item slot in the gacha restock system
    /// </summary>
    [System.Serializable]
    public class ItemSlotRestock
    {
        public ItemData currentItem;
        public bool isLocked;
        public int timesRerolled;

        public ItemSlotRestock()
        {
            currentItem = null;
            isLocked = false;
            timesRerolled = 0;
        }

        /// <summary>
        /// Get the cost to reroll this slot
        /// Formula: 5 + (timesRerolled * 2)
        /// </summary>
        public int GetRerollCost()
        {
            return 5 + (timesRerolled * 2);
        }
    }

    /// <summary>
    /// Screen for the gacha-style item restock system
    /// Players get 6 random items and can reroll individual items for gold
    /// </summary>
    public class RestockScreen : MonoBehaviour
    {
        [Header("Item Slot UI References")]
        [SerializeField] private ItemSlotUI[] itemSlotUIs = new ItemSlotUI[6];

        [Header("Bottom Bar")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Button rerollAllButton;
        [SerializeField] private TextMeshProUGUI rerollAllCostText;
        [SerializeField] private Button continueButton;

        [Header("References")]
        [SerializeField] private ItemDatabase itemDatabase;

        [Header("Animation Settings")]
        [SerializeField] private float flipDuration = 0.2f;

        [Header("Costs")]
        [SerializeField] private int rerollAllCost = 20;

        // Events
        public event Action<List<ItemData>> OnCratesPurchased;

        // State
        private ItemSlotRestock[] itemSlots = new ItemSlotRestock[6];
        private ItemCategory? hintedCategory = null;
        private bool isAnimating = false;

        private void Awake()
        {
            // Initialize slot data
            for (int i = 0; i < 6; i++)
            {
                itemSlots[i] = new ItemSlotRestock();
            }

            // Setup button listeners
            continueButton?.onClick.AddListener(OnContinueClicked);
            rerollAllButton?.onClick.AddListener(OnRerollAllClicked);

            // Setup item slot callbacks
            for (int i = 0; i < itemSlotUIs.Length; i++)
            {
                int slotIndex = i; // Capture for closure
                if (itemSlotUIs[i] != null)
                {
                    itemSlotUIs[i].OnRerollClicked += () => OnSlotRerollClicked(slotIndex);
                    itemSlotUIs[i].OnLockToggled += () => OnSlotLockToggled(slotIndex);
                }
            }
        }

        /// <summary>
        /// Setup the restock screen for a new day
        /// </summary>
        public void Setup()
        {
            // Reset all slots
            for (int i = 0; i < 6; i++)
            {
                itemSlots[i] = new ItemSlotRestock();
            }

            // Generate initial items
            GenerateInitialItems();

            // Subscribe to gold changes
            if (GameManager.Instance?.goldManager != null)
            {
                GameManager.Instance.goldManager.OnGoldChanged -= OnGoldChanged;
                GameManager.Instance.goldManager.OnGoldChanged += OnGoldChanged;
            }

            // Update UI
            UpdateAllUI();
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Generate the initial 6 items
        /// If daily hint exists, guarantee 2 items from the hinted category
        /// </summary>
        private void GenerateInitialItems()
        {
            if (itemDatabase == null)
            {
                Debug.LogError("[RestockScreen] ItemDatabase is null!");
                return;
            }

            int hintedItemCount = 0;
            int maxHintedItems = 2;

            for (int i = 0; i < 6; i++)
            {
                ItemData item;

                // If we have a hint and haven't placed enough hinted items yet
                if (hintedCategory.HasValue && hintedItemCount < maxHintedItems)
                {
                    item = itemDatabase.GetRandomItemOfCategory(hintedCategory.Value);
                    hintedItemCount++;
                }
                else
                {
                    item = itemDatabase.GetRandomItem();
                }

                itemSlots[i].currentItem = item;
                itemSlots[i].isLocked = false;
                itemSlots[i].timesRerolled = 0;
            }

            Debug.Log($"[RestockScreen] Generated 6 items. Hinted category: {hintedCategory?.ToString() ?? "None"}");
        }

        /// <summary>
        /// Handle individual slot reroll click
        /// </summary>
        private void OnSlotRerollClicked(int slotIndex)
        {
            if (isAnimating) return;
            if (slotIndex < 0 || slotIndex >= 6) return;
            if (itemSlots[slotIndex].isLocked) return;

            int cost = itemSlots[slotIndex].GetRerollCost();

            // Try to spend gold
            if (!GameManager.Instance.goldManager.TrySpendGold(cost))
            {
                Debug.Log($"[RestockScreen] Not enough gold to reroll slot {slotIndex}. Need {cost}g");
                return;
            }

            // Play reroll sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClickSound();
            }

            // Start flip animation and generate new item
            StartCoroutine(FlipAndRerollSlot(slotIndex));
        }

        /// <summary>
        /// Flip animation coroutine for a single slot
        /// </summary>
        private IEnumerator FlipAndRerollSlot(int slotIndex)
        {
            isAnimating = true;

            var slotUI = itemSlotUIs[slotIndex];
            if (slotUI != null)
            {
                // Flip backward (scale X from 1 to 0)
                yield return StartCoroutine(AnimateFlip(slotUI.transform, 1f, 0f, flipDuration));

                // Generate new item
                itemSlots[slotIndex].currentItem = itemDatabase.GetRandomItem();
                itemSlots[slotIndex].timesRerolled++;

                // Update slot display (while flipped)
                UpdateSlotUI(slotIndex);

                // Flip forward (scale X from 0 to 1)
                yield return StartCoroutine(AnimateFlip(slotUI.transform, 0f, 1f, flipDuration));
            }

            // Update UI to show new cost
            UpdateAllUI();
            isAnimating = false;
        }

        /// <summary>
        /// Animate the flip by scaling X
        /// </summary>
        private IEnumerator AnimateFlip(Transform target, float fromScale, float toScale, float duration)
        {
            float elapsed = 0f;
            Vector3 scale = target.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Use ease-in-out for smoother animation
                t = t * t * (3f - 2f * t);
                scale.x = Mathf.Lerp(fromScale, toScale, t);
                target.localScale = scale;
                yield return null;
            }

            scale.x = toScale;
            target.localScale = scale;
        }

        /// <summary>
        /// Handle slot lock toggle
        /// </summary>
        private void OnSlotLockToggled(int slotIndex)
        {
            if (isAnimating) return;
            if (slotIndex < 0 || slotIndex >= 6) return;

            // Toggle lock
            itemSlots[slotIndex].isLocked = !itemSlots[slotIndex].isLocked;

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClickSound();
            }

            // Update UI
            UpdateSlotUI(slotIndex);
            UpdateRerollAllButton();
        }

        /// <summary>
        /// Handle Reroll All button click
        /// </summary>
        private void OnRerollAllClicked()
        {
            if (isAnimating) return;

            // Check if at least one item is unlocked
            int unlockedCount = 0;
            for (int i = 0; i < 6; i++)
            {
                if (!itemSlots[i].isLocked) unlockedCount++;
            }

            if (unlockedCount == 0)
            {
                Debug.Log("[RestockScreen] No unlocked items to reroll");
                return;
            }

            // Try to spend gold
            if (!GameManager.Instance.goldManager.TrySpendGold(rerollAllCost))
            {
                Debug.Log($"[RestockScreen] Not enough gold for Reroll All. Need {rerollAllCost}g");
                return;
            }

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCratePurchaseSound();
            }

            // Start reroll all animation
            StartCoroutine(RerollAllUnlocked());
        }

        /// <summary>
        /// Coroutine to reroll all unlocked items simultaneously
        /// </summary>
        private IEnumerator RerollAllUnlocked()
        {
            isAnimating = true;

            List<int> unlockedIndices = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                if (!itemSlots[i].isLocked)
                {
                    unlockedIndices.Add(i);
                }
            }

            // Start all flip-back animations
            List<Coroutine> flipCoroutines = new List<Coroutine>();
            foreach (int index in unlockedIndices)
            {
                if (itemSlotUIs[index] != null)
                {
                    flipCoroutines.Add(StartCoroutine(AnimateFlip(itemSlotUIs[index].transform, 1f, 0f, flipDuration)));
                }
            }

            // Wait for all to complete
            yield return new WaitForSeconds(flipDuration);

            // Generate new items for all unlocked slots
            foreach (int index in unlockedIndices)
            {
                itemSlots[index].currentItem = itemDatabase.GetRandomItem();
                itemSlots[index].timesRerolled++;
                UpdateSlotUI(index);
            }

            // Start all flip-forward animations
            foreach (int index in unlockedIndices)
            {
                if (itemSlotUIs[index] != null)
                {
                    StartCoroutine(AnimateFlip(itemSlotUIs[index].transform, 0f, 1f, flipDuration));
                }
            }

            // Wait for animations
            yield return new WaitForSeconds(flipDuration);

            UpdateAllUI();
            isAnimating = false;
        }

        /// <summary>
        /// Handle continue button click
        /// </summary>
        private void OnContinueClicked()
        {
            if (isAnimating) return;

            // Collect all current items
            List<ItemData> collectedItems = new List<ItemData>();
            for (int i = 0; i < 6; i++)
            {
                if (itemSlots[i].currentItem != null)
                {
                    collectedItems.Add(itemSlots[i].currentItem);
                }
            }

            Debug.Log($"[RestockScreen] Continuing with {collectedItems.Count} items");

            // Unsubscribe from gold changes
            if (GameManager.Instance?.goldManager != null)
            {
                GameManager.Instance.goldManager.OnGoldChanged -= OnGoldChanged;
            }

            // Fire event
            OnCratesPurchased?.Invoke(collectedItems);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Handle gold changes
        /// </summary>
        private void OnGoldChanged(int newGold)
        {
            UpdateGoldDisplay();
            UpdateAllSlotButtons();
            UpdateRerollAllButton();
        }

        /// <summary>
        /// Update all UI elements
        /// </summary>
        private void UpdateAllUI()
        {
            UpdateGoldDisplay();

            for (int i = 0; i < 6; i++)
            {
                UpdateSlotUI(i);
            }

            UpdateRerollAllButton();
        }

        /// <summary>
        /// Update gold display
        /// </summary>
        private void UpdateGoldDisplay()
        {
            if (goldText != null && GameManager.Instance?.goldManager != null)
            {
                goldText.text = $"{GameManager.Instance.goldManager.CurrentGold}g";
            }
        }

        /// <summary>
        /// Update a specific slot's UI
        /// </summary>
        private void UpdateSlotUI(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= itemSlotUIs.Length) return;
            if (itemSlotUIs[slotIndex] == null) return;

            var slot = itemSlots[slotIndex];
            var slotUI = itemSlotUIs[slotIndex];

            int currentGold = GameManager.Instance?.goldManager?.CurrentGold ?? 0;
            bool canAfford = currentGold >= slot.GetRerollCost();

            slotUI.UpdateDisplay(
                slot.currentItem,
                slot.isLocked,
                slot.GetRerollCost(),
                canAfford && !slot.isLocked
            );
        }

        /// <summary>
        /// Update all slot buttons based on current gold
        /// </summary>
        private void UpdateAllSlotButtons()
        {
            for (int i = 0; i < 6; i++)
            {
                UpdateSlotUI(i);
            }
        }

        /// <summary>
        /// Update the Reroll All button state
        /// </summary>
        private void UpdateRerollAllButton()
        {
            if (rerollAllButton == null) return;

            int currentGold = GameManager.Instance?.goldManager?.CurrentGold ?? 0;
            bool canAfford = currentGold >= rerollAllCost;

            // Count unlocked items
            int unlockedCount = 0;
            for (int i = 0; i < 6; i++)
            {
                if (!itemSlots[i].isLocked) unlockedCount++;
            }

            bool hasUnlocked = unlockedCount > 0;

            rerollAllButton.interactable = canAfford && hasUnlocked && !isAnimating;

            if (rerollAllCostText != null)
            {
                rerollAllCostText.text = $"Reroll All ({rerollAllCost}g)";
                rerollAllCostText.color = canAfford ? Color.white : Color.red;
            }
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
