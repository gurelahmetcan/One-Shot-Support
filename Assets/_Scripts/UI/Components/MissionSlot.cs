using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// UI component representing a single mission slot on the mission board
    /// </summary>
    public class MissionSlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI missionNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI dangerText;
        [SerializeField] private TextMeshProUGUI goldRewardText;
        [SerializeField] private TextMeshProUGUI materialsRewardText;
        [SerializeField] private TextMeshProUGUI intelText;
        [SerializeField] private Image missionIcon;
        [SerializeField] private Button slotButton;
        [SerializeField] private Image selectionHighlight;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.yellow;

        // Events
        public event Action OnSlotClicked;

        private MissionData mission;

        private void Awake()
        {
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(HandleClick);
            }
        }

        /// <summary>
        /// Setup the mission slot with mission data
        /// </summary>
        public void Setup(MissionData missionData)
        {
            mission = missionData;

            // Update UI
            if (missionNameText != null)
                missionNameText.text = mission.missionName;

            if (descriptionText != null)
                descriptionText.text = mission.description;

            if (dangerText != null)
                dangerText.text = GetDangerStars(mission.dangerLevel);

            if (goldRewardText != null)
                goldRewardText.text = $"Gold: {mission.goldReward}";

            if (materialsRewardText != null)
                materialsRewardText.text = $"Materials: {mission.materialsReward}";

            if (intelText != null)
            {
                if (string.IsNullOrEmpty(mission.intelHint))
                {
                    intelText.text = "No intel available";
                    intelText.color = Color.gray;
                }
                else
                {
                    intelText.text = $"Intel: {mission.intelHint}";
                    intelText.color = Color.white;
                }
            }

            if (missionIcon != null && mission.missionSprite != null)
            {
                missionIcon.sprite = mission.missionSprite;
            }

            SetSelected(false);
        }

        /// <summary>
        /// Set the visual selection state
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (selectionHighlight != null)
            {
                selectionHighlight.gameObject.SetActive(selected);
                selectionHighlight.color = selected ? selectedColor : normalColor;
            }
        }

        /// <summary>
        /// Handle slot button click
        /// </summary>
        private void HandleClick()
        {
            OnSlotClicked?.Invoke();
        }

        /// <summary>
        /// Convert danger level to star display
        /// </summary>
        private string GetDangerStars(MissionDanger danger)
        {
            int stars = (int)danger;
            string starString = "";
            for (int i = 0; i < stars; i++)
            {
                starString += "★";
            }
            return $"Danger: {starString} ({stars} Star)";
        }
    }
}
