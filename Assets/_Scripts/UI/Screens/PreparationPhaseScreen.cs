using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.ScriptableObjects;
using OneShotSupport.UI.Components;
using OneShotSupport.UI.DragDrop;
using OneShotSupport.Core;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Preparation Phase screen - assign heroes to quests before dispatching
    /// Shows the selected quest with hero assignment slots and available heroes
    /// </summary>
    public class PreparationPhaseScreen : MonoBehaviour
    {
        [Header("Quest Display")]
        [SerializeField] private TextMeshProUGUI questNameText;
        [SerializeField] private TextMeshProUGUI questDescriptionText;
        [SerializeField] private Image questImage;
        [SerializeField] private Transform difficultyStarsContainer;
        [SerializeField] private GameObject starPrefab;

        [Header("Pentagon Display")]
        [SerializeField] private PentagonStatDisplay pentagonDisplay;
        [SerializeField] private float pentagonRadius = 100f;

        [Header("Ball Resolution Animation")]
        [SerializeField] private BallResolutionAnimator ballAnimator;

        [Header("Rewards Display")]
        [SerializeField] private TextMeshProUGUI goldRewardText;
        [SerializeField] private TextMeshProUGUI materialsRewardText;

        [Header("Hero Assignment")]
        [SerializeField] private Transform heroSlotsContainer;
        [SerializeField] private GameObject heroSlotPrefab;
        [SerializeField] private List<HeroAssignmentSlot> heroSlots = new List<HeroAssignmentSlot>();

        [Header("Available Heroes")]
        [SerializeField] private Transform availableHeroesContainer;
        [SerializeField] private GameObject draggableHeroPrefab;

        [Header("Buttons")]
        [SerializeField] private Button dispatchButton;
        [SerializeField] private Button backButton;

        [Header("Requirements Display")]
        [SerializeField] private TextMeshProUGUI requirementsText;

        // Current state
        private MissionData currentMission;
        private List<HeroData> availableHeroes = new List<HeroData>();
        private List<DraggableHero> spawnedHeroCards = new List<DraggableHero>();
        private MissionResolutionResult pendingResult;
        private bool isResolving = false;

        // Events
        public event Action OnDispatchClicked;
        public event Action<MissionResolutionResult> OnDispatchCompleted;
        public event Action OnBackClicked;
        public event Action<List<HeroData>> OnHeroAssignmentChanged;

        private void Awake()
        {
            if (dispatchButton != null)
                dispatchButton.onClick.AddListener(HandleDispatchClicked);

            if (backButton != null)
                backButton.onClick.AddListener(() => OnBackClicked?.Invoke());

            if (ballAnimator != null)
                ballAnimator.OnAnimationComplete += HandleResolutionAnimationComplete;
        }

        private void OnDestroy()
        {
            if (ballAnimator != null)
                ballAnimator.OnAnimationComplete -= HandleResolutionAnimationComplete;
        }

        /// <summary>
        /// Setup the preparation phase with mission and available heroes
        /// </summary>
        public void Setup(MissionData mission, List<HeroData> heroes)
        {
            currentMission = mission;
            availableHeroes = heroes ?? new List<HeroData>();

            // Display quest info
            UpdateQuestDisplay();

            // Setup hero assignment slots based on mission capacity
            SetupHeroSlots();

            // Display available heroes
            DisplayAvailableHeroes();

            // Update pentagon to show requirements (full outer, empty inner for now)
            UpdatePentagonDisplay();

            // Update dispatch button state
            UpdateDispatchButtonState();

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Update the quest information display
        /// </summary>
        private void UpdateQuestDisplay()
        {
            if (currentMission == null) return;

            if (questNameText != null)
                questNameText.text = currentMission.missionName;

            if (questDescriptionText != null)
                questDescriptionText.text = currentMission.description;

            if (questImage != null && currentMission.missionSprite != null)
                questImage.sprite = currentMission.missionSprite;

            // Display difficulty stars
            UpdateDifficultyStars();

            // Display rewards
            if (goldRewardText != null)
                goldRewardText.text = $"{currentMission.goldReward} Gold";

            if (materialsRewardText != null)
                materialsRewardText.text = $"{currentMission.materialsReward} Materials";

            // Display requirements text
            if (requirementsText != null)
            {
                requirementsText.text = $"M:{currentMission.mightRequirement} C:{currentMission.charmRequirement} W:{currentMission.witRequirement} A:{currentMission.agilityRequirement} F:{currentMission.fortitudeRequirement}";
            }
        }

        /// <summary>
        /// Update difficulty star display
        /// </summary>
        private void UpdateDifficultyStars()
        {
            if (difficultyStarsContainer == null || starPrefab == null) return;

            // Clear existing stars
            foreach (Transform child in difficultyStarsContainer)
            {
                Destroy(child.gameObject);
            }

            // Add stars based on danger level
            int starCount = (int)currentMission.dangerLevel + 1;
            for (int i = 0; i < starCount; i++)
            {
                Instantiate(starPrefab, difficultyStarsContainer);
            }
        }

        /// <summary>
        /// Setup hero assignment slots based on mission hero capacity
        /// </summary>
        private void SetupHeroSlots()
        {
            // Clear existing slots
            foreach (var slot in heroSlots)
            {
                if (slot != null)
                {
                    slot.OnHeroAssigned -= HandleHeroAssigned;
                    slot.OnHeroRemoved -= HandleHeroRemoved;
                }
            }
            heroSlots.Clear();

            if (heroSlotsContainer != null)
            {
                foreach (Transform child in heroSlotsContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // Create slots based on mission capacity
            int slotCount = currentMission != null ? currentMission.maxHeroCount : 1;

            if (heroSlotPrefab != null && heroSlotsContainer != null)
            {
                for (int i = 0; i < slotCount; i++)
                {
                    var slotObj = Instantiate(heroSlotPrefab, heroSlotsContainer);
                    var slot = slotObj.GetComponent<HeroAssignmentSlot>();
                    if (slot != null)
                    {
                        slot.SetSlotIndex(i);
                        slot.OnHeroAssigned += HandleHeroAssigned;
                        slot.OnHeroRemoved += HandleHeroRemoved;
                        heroSlots.Add(slot);
                    }
                }
            }
        }

        /// <summary>
        /// Display available heroes for assignment
        /// </summary>
        private void DisplayAvailableHeroes()
        {
            // Clear existing hero cards
            foreach (var card in spawnedHeroCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            spawnedHeroCards.Clear();

            if (availableHeroesContainer == null || draggableHeroPrefab == null) return;

            // Spawn hero cards for each available hero
            foreach (var hero in availableHeroes)
            {
                var heroObj = Instantiate(draggableHeroPrefab, availableHeroesContainer);
                var draggableHero = heroObj.GetComponent<DraggableHero>();
                if (draggableHero != null)
                {
                    draggableHero.Initialize(hero);
                    draggableHero.SetOriginalParent(availableHeroesContainer);
                    draggableHero.OnDragEnded += HandleHeroDragEnded;
                    spawnedHeroCards.Add(draggableHero);
                }
            }
        }

        /// <summary>
        /// Update pentagon display with assigned heroes' combined stats
        /// </summary>
        private void UpdatePentagonDisplay()
        {
            if (pentagonDisplay == null || currentMission == null) return;

            // Set the base pentagon (outer) to full - represents the "max" or target
            // The main stats show the mission requirements
            pentagonDisplay.SetStats(
                currentMission.mightRequirement,
                currentMission.charmRequirement,
                currentMission.witRequirement,
                currentMission.agilityRequirement,
                currentMission.fortitudeRequirement
            );

            // Calculate combined hero stats
            int combinedMight = 0, combinedCharm = 0, combinedWit = 0, combinedAgility = 0, combinedFortitude = 0;

            foreach (var slot in heroSlots)
            {
                var heroData = slot.GetHeroData();
                if (heroData != null)
                {
                    combinedMight += heroData.might;
                    combinedCharm += heroData.charm;
                    combinedWit += heroData.wit;
                    combinedAgility += heroData.agility;
                    combinedFortitude += heroData.fortitude;
                }
            }

            // Set overlay to show combined hero stats
            if (combinedMight > 0 || combinedCharm > 0 || combinedWit > 0 || combinedAgility > 0 || combinedFortitude > 0)
            {
                pentagonDisplay.SetOverlayStats(combinedMight, combinedCharm, combinedWit, combinedAgility, combinedFortitude);
            }
            else
            {
                pentagonDisplay.ClearOverlay();
            }
        }

        /// <summary>
        /// Handle hero assigned to slot
        /// </summary>
        private void HandleHeroAssigned(HeroAssignmentSlot slot, HeroData hero)
        {
            UpdatePentagonDisplay();
            UpdateDispatchButtonState();
            NotifyAssignmentChanged();
        }

        /// <summary>
        /// Handle hero removed from slot
        /// </summary>
        private void HandleHeroRemoved(HeroAssignmentSlot slot)
        {
            UpdatePentagonDisplay();
            UpdateDispatchButtonState();
            NotifyAssignmentChanged();
        }

        /// <summary>
        /// Handle hero drag ended
        /// </summary>
        private void HandleHeroDragEnded(DraggableHero hero)
        {
            UpdatePentagonDisplay();
            UpdateDispatchButtonState();
        }

        /// <summary>
        /// Update dispatch button interactability
        /// </summary>
        private void UpdateDispatchButtonState()
        {
            if (dispatchButton == null) return;

            // Enable dispatch if at least one hero is assigned
            bool hasAssignedHero = false;
            foreach (var slot in heroSlots)
            {
                if (!slot.IsEmpty())
                {
                    hasAssignedHero = true;
                    break;
                }
            }

            dispatchButton.interactable = hasAssignedHero;
        }

        /// <summary>
        /// Handle dispatch button click
        /// </summary>
        private void HandleDispatchClicked()
        {
            if (isResolving) return;

            // Gather assigned heroes
            List<HeroData> assignedHeroes = GetAssignedHeroes();

            if (assignedHeroes.Count == 0)
            {
                Debug.LogWarning("[PreparationPhase] Cannot dispatch - no heroes assigned!");
                return;
            }

            Debug.Log($"[PreparationPhase] Dispatching {assignedHeroes.Count} heroes on mission: {currentMission.missionName}");

            // Calculate the result
            pendingResult = MissionResolver.ResolveMission(currentMission, assignedHeroes);

            // Start the ball animation if we have an animator
            if (ballAnimator != null)
            {
                isResolving = true;
                dispatchButton.interactable = false;
                backButton.interactable = false;

                // Start animation with mission requirements and target landing position
                ballAnimator.StartAnimation(
                    currentMission.mightRequirement,
                    currentMission.charmRequirement,
                    currentMission.witRequirement,
                    currentMission.agilityRequirement,
                    currentMission.fortitudeRequirement,
                    pendingResult.ballLandingPosition,
                    pendingResult.isSuccess,
                    pentagonRadius
                );
            }
            else
            {
                // No animator - complete immediately
                OnDispatchClicked?.Invoke();
                OnDispatchCompleted?.Invoke(pendingResult);
            }
        }

        /// <summary>
        /// Handle resolution animation complete
        /// </summary>
        private void HandleResolutionAnimationComplete(bool isSuccess)
        {
            isResolving = false;

            Debug.Log($"[PreparationPhase] Resolution animation complete - {(isSuccess ? "SUCCESS" : "FAILURE")}");

            // Re-enable buttons
            if (dispatchButton != null)
                dispatchButton.interactable = true;
            if (backButton != null)
                backButton.interactable = true;

            // Notify listeners
            OnDispatchClicked?.Invoke();
            OnDispatchCompleted?.Invoke(pendingResult);
        }

        /// <summary>
        /// Get list of assigned heroes
        /// </summary>
        public List<HeroData> GetAssignedHeroes()
        {
            List<HeroData> assigned = new List<HeroData>();
            foreach (var slot in heroSlots)
            {
                var heroData = slot.GetHeroData();
                if (heroData != null)
                {
                    assigned.Add(heroData);
                }
            }
            return assigned;
        }

        /// <summary>
        /// Notify listeners of assignment changes
        /// </summary>
        private void NotifyAssignmentChanged()
        {
            OnHeroAssignmentChanged?.Invoke(GetAssignedHeroes());
        }

        /// <summary>
        /// Hide the screen
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Get the current mission
        /// </summary>
        public MissionData CurrentMission => currentMission;
    }
}
