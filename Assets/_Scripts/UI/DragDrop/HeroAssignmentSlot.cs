using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.DragDrop
{
    /// <summary>
    /// Slot that can receive draggable heroes for mission assignment
    /// Used in the preparation phase to assign heroes to quests
    /// </summary>
    public class HeroAssignmentSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        public RectTransform heroContainer; // Where the hero visual will be parented
        public Image slotBackground;
        public Image placeholderImage; // Shows when empty

        [Header("Visual Feedback")]
        public Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        public Color highlightColor = new Color(0.3f, 0.6f, 0.3f, 0.7f);
        public Color filledColor = new Color(0.2f, 0.4f, 0.2f, 0.5f);

        private DraggableHero currentHero;
        private int slotIndex;

        // Events
        public System.Action<HeroAssignmentSlot, HeroData> OnHeroAssigned;
        public System.Action<HeroAssignmentSlot> OnHeroRemoved;

        private void Awake()
        {
            if (heroContainer == null)
                heroContainer = transform as RectTransform;

            UpdateVisuals();
        }

        /// <summary>
        /// Set the slot index (0, 1, 2 for up to 3 hero slots)
        /// </summary>
        public void SetSlotIndex(int index)
        {
            slotIndex = index;
        }

        public void OnDrop(PointerEventData eventData)
        {
            DraggableHero draggedHero = eventData.pointerDrag?.GetComponent<DraggableHero>();
            if (draggedHero != null && CanAcceptHero(draggedHero))
            {
                PlaceHero(draggedHero);
            }
        }

        /// <summary>
        /// Check if this slot can accept the given hero
        /// </summary>
        public bool CanAcceptHero(DraggableHero hero)
        {
            if (hero == null) return false;

            // Don't accept if already has a hero
            if (currentHero != null) return false;

            return true;
        }

        /// <summary>
        /// Place a hero in this slot
        /// </summary>
        public void PlaceHero(DraggableHero hero)
        {
            if (currentHero != null)
            {
                Debug.Log($"[HeroAssignmentSlot] Slot already occupied, rejecting placement");
                return;
            }

            // Remove hero from its current slot if any
            if (hero.CurrentSlot != null)
            {
                hero.CurrentSlot.RemoveHero();
            }

            // Place hero in this slot
            currentHero = hero;
            hero.transform.SetParent(heroContainer);
            hero.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            hero.SetSlot(this);

            // Hide placeholder
            if (placeholderImage != null)
                placeholderImage.gameObject.SetActive(false);

            UpdateVisuals();

            Debug.Log($"[HeroAssignmentSlot] {hero.heroData.heroName} assigned to slot {slotIndex}");
            OnHeroAssigned?.Invoke(this, hero.heroData);
        }

        /// <summary>
        /// Remove the current hero from this slot
        /// </summary>
        public void RemoveHero()
        {
            if (currentHero != null)
            {
                var removedHero = currentHero;
                currentHero.SetSlot(null);
                currentHero = null;

                // Show placeholder
                if (placeholderImage != null)
                    placeholderImage.gameObject.SetActive(true);

                UpdateVisuals();

                Debug.Log($"[HeroAssignmentSlot] Hero removed from slot {slotIndex}");
                OnHeroRemoved?.Invoke(this);
            }
        }

        /// <summary>
        /// Get the hero data in this slot
        /// </summary>
        public HeroData GetHeroData()
        {
            return currentHero?.heroData;
        }

        /// <summary>
        /// Check if slot is empty
        /// </summary>
        public bool IsEmpty()
        {
            return currentHero == null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Highlight when dragging a hero over
            if (eventData.pointerDrag != null)
            {
                var draggedHero = eventData.pointerDrag.GetComponent<DraggableHero>();
                if (draggedHero != null && CanAcceptHero(draggedHero))
                {
                    if (slotBackground != null)
                        slotBackground.color = highlightColor;
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UpdateVisuals();
        }

        /// <summary>
        /// Update visual state of the slot
        /// </summary>
        private void UpdateVisuals()
        {
            if (slotBackground != null)
            {
                slotBackground.color = currentHero != null ? filledColor : emptyColor;
            }
        }

        public DraggableHero CurrentHero => currentHero;
        public int SlotIndex => slotIndex;
    }
}
