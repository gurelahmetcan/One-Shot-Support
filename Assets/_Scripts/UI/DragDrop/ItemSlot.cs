using UnityEngine;
using UnityEngine.EventSystems;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.DragDrop
{
    /// <summary>
    /// Item slot that can receive draggable items
    /// Used for both inventory grid and equipment slots
    /// </summary>
    public class ItemSlot : MonoBehaviour, IDropHandler
    {
        [Header("Slot Type")]
        public bool isEquipmentSlot = false; // true = equipment slot, false = inventory slot

        [Header("References")]
        public RectTransform itemContainer; // Where the item visual will be parented

        private DraggableItem currentItem;

        // Events
        public System.Action<ItemSlot, ItemData> OnItemPlaced;
        public System.Action<ItemSlot> OnItemRemoved;

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

            // Equipment slots: only accept if empty
            if (isEquipmentSlot)
            {
                return currentItem == null;
            }

            // Inventory slots: always accept (for returning items)
            return true;
        }

        /// <summary>
        /// Place an item in this slot
        /// </summary>
        public void PlaceItem(DraggableItem item)
        {
            // If slot already has an item, swap them
            if (currentItem != null && isEquipmentSlot)
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
    }
}
