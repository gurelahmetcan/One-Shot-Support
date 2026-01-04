using UnityEngine;
using UnityEngine.EventSystems;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.DragDrop
{
    /// <summary>
    /// Draggable item component for inventory items
    /// Handles drag & drop functionality
    /// </summary>
    public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        public ItemData itemData;
        public UnityEngine.UI.Image itemIcon;

        [Header("Drag Settings")]
        public bool isDraggable = true;

        private Canvas canvas;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector2 originalPosition;
        private Transform originalParent;
        private ItemSlot currentSlot;

        // Events
        public System.Action<DraggableItem> OnHoverEnter;
        public System.Action<DraggableItem> OnHoverExit;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Try to find canvas, but don't fail if not found yet
            TryFindCanvas();
        }

        /// <summary>
        /// Try to find the parent canvas
        /// </summary>
        private void TryFindCanvas()
        {
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }
        }

        /// <summary>
        /// Initialize the draggable item with data
        /// </summary>
        public void Initialize(ItemData data, ItemSlot slot = null)
        {
            itemData = data;
            currentSlot = slot;

            if (itemIcon != null && itemData != null && itemData.icon != null)
            {
                itemIcon.sprite = itemData.icon;
            }

            // Ensure we have canvas reference after being placed in scene
            TryFindCanvas();
        }

        public void ChangeIconSize(bool isSmall)
        {
            RectTransform rt = itemIcon.GetComponent (typeof (RectTransform)) as RectTransform;
            if (rt != null) rt.sizeDelta = isSmall ? new Vector2(120, 120) : new Vector2(350, 350);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isDraggable) return;

            // Make sure we have canvas reference
            TryFindCanvas();
            if (canvas == null)
            {
                Debug.LogError("DraggableItem: Canvas not found! Make sure item is a child of a Canvas.");
                return;
            }

            // Store original position and parent
            originalPosition = rectTransform.anchoredPosition;
            originalParent = transform.parent;

            // Remove from current slot
            if (currentSlot != null)
            {
                currentSlot.RemoveItem();
                currentSlot = null;
            }

            // Make semi-transparent while dragging
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;

            // Move to canvas root for proper rendering
            transform.SetParent(canvas.transform);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDraggable) return;

            // Safety check
            if (canvas == null)
            {
                TryFindCanvas();
                if (canvas == null) return;
            }

            // Follow mouse
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDraggable) return;

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            // IMPORTANT: Check if item was already placed by ItemSlot.OnDrop()
            // If currentSlot is set, the item was successfully placed, don't return it
            if (currentSlot != null)
            {
                // Item was successfully placed in a slot by OnDrop
                return;
            }

            // Item was not placed in any slot, return to original position
            ReturnToOriginal();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoverEnter?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverExit?.Invoke(this);
        }

        /// <summary>
        /// Return item to its original position
        /// </summary>
        public void ReturnToOriginal()
        {
            if (originalParent != null)
            {
                transform.SetParent(originalParent);
                rectTransform.anchoredPosition = originalPosition;
            }
        }

        /// <summary>
        /// Set the current slot this item is in
        /// </summary>
        public void SetSlot(ItemSlot slot)
        {
            currentSlot = slot;
        }

        public ItemSlot CurrentSlot => currentSlot;
    }
}
