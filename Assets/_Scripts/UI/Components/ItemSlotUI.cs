using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// UI component for displaying a single item slot in the gacha restock system
    /// Shows item icon, category, name, reroll button with cost, and lock state
    /// </summary>
    public class ItemSlotUI : MonoBehaviour
    {
        [Header("Item Display")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private Image categoryIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI categoryText;

        [Header("Reroll Button")]
        [SerializeField] private Button rerollButton;
        [SerializeField] private TextMeshProUGUI rerollCostText;

        [Header("Lock UI")]
        [SerializeField] private Button lockButton;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private Image cardBackground;

        [Header("Lock Visual Settings")]
        [SerializeField] private Color normalBorderColor = Color.white;
        [SerializeField] private Color lockedBorderColor = new Color(1f, 0.84f, 0f); // Gold color
        [SerializeField] private Image borderImage;

        [Header("Optional: Card Outline")]
        [SerializeField] private Outline cardOutline;

        // Events
        public event Action OnRerollClicked;
        public event Action OnLockToggled;

        private bool isLocked = false;

        private void Awake()
        {
            // Setup button listeners
            if (rerollButton != null)
            {
                rerollButton.onClick.AddListener(HandleRerollClick);
            }

            if (lockButton != null)
            {
                lockButton.onClick.AddListener(HandleLockClick);
            }

            // Initialize lock state
            UpdateLockVisual(false);
        }

        /// <summary>
        /// Update the slot display with new item data
        /// </summary>
        /// <param name="item">The item to display</param>
        /// <param name="locked">Whether this slot is locked</param>
        /// <param name="rerollCost">The cost to reroll this slot</param>
        /// <param name="canReroll">Whether the reroll button should be enabled</param>
        public void UpdateDisplay(ItemData item, bool locked, int rerollCost, bool canReroll)
        {
            isLocked = locked;

            // Update item display
            if (item != null)
            {
                if (itemIcon != null)
                {
                    itemIcon.sprite = item.icon;
                    itemIcon.enabled = true;
                }

                if (categoryIcon != null)
                {
                    categoryIcon.sprite = item.categoryIcon;
                    categoryIcon.enabled = item.categoryIcon != null;
                }

                if (itemNameText != null)
                {
                    itemNameText.text = item.itemName;
                }

                if (categoryText != null)
                {
                    categoryText.text = item.category.ToString();
                }
            }
            else
            {
                // No item - clear display
                if (itemIcon != null)
                {
                    itemIcon.sprite = null;
                    itemIcon.enabled = false;
                }

                if (categoryIcon != null)
                {
                    categoryIcon.sprite = null;
                    categoryIcon.enabled = false;
                }

                if (itemNameText != null)
                {
                    itemNameText.text = "Empty";
                }

                if (categoryText != null)
                {
                    categoryText.text = "";
                }
            }

            // Update reroll button
            if (rerollButton != null)
            {
                rerollButton.interactable = canReroll;
            }

            if (rerollCostText != null)
            {
                rerollCostText.text = $"{rerollCost}g";
                rerollCostText.color = canReroll ? Color.white : Color.red;
            }

            // Update lock visual
            UpdateLockVisual(locked);
        }

        /// <summary>
        /// Update the visual state for locked/unlocked
        /// </summary>
        private void UpdateLockVisual(bool locked)
        {
            // Show/hide lock icon
            if (lockIcon != null)
            {
                lockIcon.SetActive(locked);
            }

            // Update border color
            if (borderImage != null)
            {
                borderImage.color = locked ? lockedBorderColor : normalBorderColor;
            }

            // Update outline if present
            if (cardOutline != null)
            {
                cardOutline.effectColor = locked ? lockedBorderColor : normalBorderColor;
                cardOutline.enabled = locked;
            }

            // Dim reroll button visually when locked
            if (rerollButton != null)
            {
                rerollButton.interactable = !locked && rerollButton.interactable;
            }
        }

        /// <summary>
        /// Handle reroll button click
        /// </summary>
        private void HandleRerollClick()
        {
            if (!isLocked)
            {
                OnRerollClicked?.Invoke();
            }
        }

        /// <summary>
        /// Handle lock button click (clicking on the card)
        /// </summary>
        private void HandleLockClick()
        {
            OnLockToggled?.Invoke();
        }

        /// <summary>
        /// Set the locked state visually (called externally)
        /// </summary>
        public void SetLocked(bool locked)
        {
            isLocked = locked;
            UpdateLockVisual(locked);
        }

        /// <summary>
        /// Get the current locked state
        /// </summary>
        public bool IsLocked => isLocked;
    }
}
