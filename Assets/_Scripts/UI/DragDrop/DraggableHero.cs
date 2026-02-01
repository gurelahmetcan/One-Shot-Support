using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.DragDrop
{
    /// <summary>
    /// Draggable hero component for preparation phase
    /// Allows heroes to be dragged and dropped into mission slots
    /// </summary>
    public class DraggableHero : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        public HeroData heroData;
        public Image heroPortrait;
        public TMPro.TextMeshProUGUI heroNameText;

        [Header("Drag Settings")]
        public bool isDraggable = true;

        private Canvas canvas;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector2 originalPosition;
        private Transform originalParent;
        private HeroAssignmentSlot currentSlot;

        // Events
        public System.Action<DraggableHero> OnHeroSelected;
        public System.Action<DraggableHero> OnHoverEnter;
        public System.Action<DraggableHero> OnHoverExit;
        public System.Action<DraggableHero> OnDragStarted;
        public System.Action<DraggableHero> OnDragEnded;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            TryFindCanvas();
        }

        private void TryFindCanvas()
        {
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }
        }

        /// <summary>
        /// Initialize the draggable hero with data
        /// </summary>
        public void Initialize(HeroData data, HeroAssignmentSlot slot = null)
        {
            heroData = data;
            currentSlot = slot;

            if (heroPortrait != null && heroData != null && heroData.portrait != null)
            {
                heroPortrait.sprite = heroData.portrait;
            }

            if (heroNameText != null && heroData != null)
            {
                heroNameText.text = heroData.heroName;
            }

            TryFindCanvas();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isDraggable) return;

            TryFindCanvas();
            if (canvas == null)
            {
                Debug.LogError("DraggableHero: Canvas not found!");
                return;
            }

            // Store original position and parent
            originalPosition = rectTransform.anchoredPosition;
            originalParent = transform.parent;

            // Remove from current slot if in one
            if (currentSlot != null)
            {
                currentSlot.RemoveHero();
                currentSlot = null;
            }

            // Make semi-transparent while dragging
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;

            // Move to canvas root for proper rendering
            transform.SetParent(canvas.transform);

            OnDragStarted?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDraggable) return;

            if (canvas == null)
            {
                TryFindCanvas();
                if (canvas == null) return;
            }

            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDraggable) return;

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            // If placed in a slot, don't return to original
            if (currentSlot != null)
            {
                OnDragEnded?.Invoke(this);
                return;
            }

            // Item was not placed in any slot, return to original position
            ReturnToOriginal();
            OnDragEnded?.Invoke(this);
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
        /// Return hero to its original position
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
        /// Set the current slot this hero is in
        /// </summary>
        public void SetSlot(HeroAssignmentSlot slot)
        {
            currentSlot = slot;
        }

        /// <summary>
        /// Store original parent for returning later
        /// </summary>
        public void SetOriginalParent(Transform parent)
        {
            originalParent = parent;
            originalPosition = rectTransform.anchoredPosition;
        }

        public HeroAssignmentSlot CurrentSlot => currentSlot;
    }
}
