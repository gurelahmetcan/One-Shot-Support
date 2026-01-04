using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.DragDrop
{
    /// <summary>
    /// Item slot that can receive draggable items
    /// Used for both inventory grid and equipment slots
    /// </summary>
    public class ItemSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Slot Type")]
        public bool isEquipmentSlot = false; // true = equipment slot, false = inventory slot

        [Header("References")]
        public RectTransform itemContainer; // Where the item visual will be parented
        public GameObject infoIcon; // Info icon shown at bottom right when slot has item

        private DraggableItem currentItem;
        private bool isInfoIconHovered = false;

        // Events
        public System.Action<ItemSlot, ItemData> OnItemPlaced;
        public System.Action<ItemSlot> OnItemRemoved;
        public System.Action<ItemSlot> OnInfoIconHover; // Triggered when info icon is hovered

        private void Awake()
        {
            if (itemContainer == null)
                itemContainer = transform as RectTransform;
        }

        public void OnDrop(PointerEventData eventData)
        {
            DraggableItem draggedItem = eventData.pointerDrag?.GetComponent<DraggableItem>();
            if (draggedItem != null && CanAcceptItem(draggedItem))
            {
                PlaceItem(draggedItem);
            }
        }

        /// <summary>
        /// Check if this slot can accept the given item
        /// </summary>
        public bool CanAcceptItem(DraggableItem item)
        {
            if (item == null) return false;

            // Always allow drops - will swap if occupied
            return true;
        }

        /// <summary>
        /// Place an item in this slot
        /// </summary>
        public void PlaceItem(DraggableItem item)
        {
            // BUG FIX: If slot already has an item, swap them (both equipment and inventory)
            if (currentItem != null)
            {
                SwapItems(item);
                return;
            }

            // Remove item from its current slot
            if (item.CurrentSlot != null)
            {
                item.CurrentSlot.RemoveItem();
            }

            // Place item in this slot
            currentItem = item;
            item.transform.SetParent(itemContainer);
            item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            item.SetSlot(this);

            // Resize item icon based on slot type
            // Equipment slots = small (120x120), Inventory slots = large (350x350)
            item.ChangeIconSize(isEquipmentSlot);

            // Show info icon when item is placed
            if (infoIcon != null)
                infoIcon.SetActive(true);

            OnItemPlaced?.Invoke(this, item.itemData);
        }

        /// <summary>
        /// Remove the current item from this slot
        /// </summary>
        public void RemoveItem()
        {
            if (currentItem != null)
            {
                currentItem.SetSlot(null);
                currentItem = null;

                // Hide info icon when item is removed
                if (infoIcon != null)
                    infoIcon.SetActive(false);

                OnItemRemoved?.Invoke(this);
            }
        }

        /// <summary>
        /// Swap items between this slot and the dragged item's slot
        /// </summary>
        private void SwapItems(DraggableItem draggedItem)
        {
            var itemToSwap = currentItem;
            var draggedItemSlot = draggedItem.CurrentSlot;

            // Remove both items
            RemoveItem();
            if (draggedItemSlot != null)
            {
                draggedItemSlot.RemoveItem();
            }

            // Place dragged item in this slot
            PlaceItem(draggedItem);

            // Place this slot's item in the dragged item's original slot
            if (draggedItemSlot != null && itemToSwap != null)
            {
                draggedItemSlot.PlaceItem(itemToSwap);
            }
        }

        /// <summary>
        /// Get the item data in this slot
        /// </summary>
        public ItemData GetItemData()
        {
            return currentItem?.itemData;
        }

        /// <summary>
        /// Check if slot is empty
        /// </summary>
        public bool IsEmpty()
        {
            return currentItem == null;
        }

        public DraggableItem CurrentItem => currentItem;

        /// <summary>
        /// Pointer enter handler for info icon hover
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Check if hovering over the info icon
            if (infoIcon != null && infoIcon.activeSelf && currentItem != null)
            {
                // Check if pointer is over info icon specifically
                if (RectTransformUtility.RectangleContainsScreenPoint(
                    infoIcon.GetComponent<RectTransform>(),
                    eventData.position,
                    eventData.pressEventCamera))
                {
                    OnInfoIconHover?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Pointer exit handler
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            // Optional: Could hide tooltip here if desired
        }
    }
}
