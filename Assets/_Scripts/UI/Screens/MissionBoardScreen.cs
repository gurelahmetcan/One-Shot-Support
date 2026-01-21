using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Mission Board screen that displays available missions
    /// Player can select one mission per season
    /// </summary>
    public class MissionBoardScreen : MonoBehaviour
    {
        [Header("Mission Slots")]
        [SerializeField] private MissionSlot[] missionSlots;

        [Header("UI References")]
        [SerializeField] private Button backButton;

        // Events
        public event Action<MissionData> OnMissionSelected;
        public event Action OnBackClicked;

        private List<MissionData> availableMissions;
        private MissionData selectedMission;

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackClicked);
            }

            // Setup mission slot callbacks
            if (missionSlots != null)
            {
                for (int i = 0; i < missionSlots.Length; i++)
                {
                    int index = i; // Capture index for closure
                    if (missionSlots[i] != null)
                    {
                        missionSlots[i].OnSlotClicked += () => HandleMissionClicked(index);
                    }
                }
            }
        }

        /// <summary>
        /// Setup and show the mission board with available missions
        /// </summary>
        public void Setup(List<MissionData> missions)
        {
            availableMissions = missions;
            selectedMission = null;

            // Display missions in slots
            for (int i = 0; i < missionSlots.Length; i++)
            {
                if (i < missions.Count)
                {
                    missionSlots[i].Setup(missions[i]);
                    missionSlots[i].SetSelected(false);
                    missionSlots[i].gameObject.SetActive(true);
                }
                else
                {
                    missionSlots[i].gameObject.SetActive(false);
                }
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Handle mission slot clicked
        /// </summary>
        private void HandleMissionClicked(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= availableMissions.Count) return;

            MissionData clickedMission = availableMissions[slotIndex];

            // If same mission clicked again, deselect
            if (selectedMission == clickedMission)
            {
                selectedMission = null;
                UpdateMissionSelection();
                return;
            }

            // Select this mission
            selectedMission = clickedMission;
            UpdateMissionSelection();

            // Notify listeners
            OnMissionSelected?.Invoke(selectedMission);

            Debug.Log($"[MissionBoard] Selected mission: {selectedMission.missionName}");
        }

        /// <summary>
        /// Update visual selection state of mission slots
        /// </summary>
        private void UpdateMissionSelection()
        {
            for (int i = 0; i < missionSlots.Length && i < availableMissions.Count; i++)
            {
                bool isSelected = (availableMissions[i] == selectedMission);
                missionSlots[i].SetSelected(isSelected);
            }
        }

        /// <summary>
        /// Handle back button clicked
        /// </summary>
        private void HandleBackClicked()
        {
            OnBackClicked?.Invoke();
            Hide();
        }

        /// <summary>
        /// Get the currently selected mission
        /// </summary>
        public MissionData GetSelectedMission()
        {
            return selectedMission;
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
