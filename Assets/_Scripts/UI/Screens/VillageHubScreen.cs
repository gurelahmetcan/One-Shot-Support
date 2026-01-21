using System;
using UnityEngine;
using UnityEngine.UI;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Village Hub screen - main navigation hub (like Darkest Dungeon)
    /// Player can navigate to different locations from here
    /// </summary>
    public class VillageHubScreen : MonoBehaviour
    {
        [Header("Navigation Buttons")]
        [SerializeField] private Button tavernButton;
        [SerializeField] private Button missionBoardButton;
        [SerializeField] private Button barracksButton;
        [SerializeField] private Button forgeButton;

        // Events
        public event Action OnTavernClicked;
        public event Action OnMissionBoardClicked;
        public event Action OnBarracksClicked;
        public event Action OnForgeClicked;

        private void Awake()
        {
            // Setup button listeners
            if (tavernButton != null)
                tavernButton.onClick.AddListener(() => OnTavernClicked?.Invoke());

            if (missionBoardButton != null)
                missionBoardButton.onClick.AddListener(() => OnMissionBoardClicked?.Invoke());

            if (barracksButton != null)
                barracksButton.onClick.AddListener(() => OnBarracksClicked?.Invoke());

            if (forgeButton != null)
                forgeButton.onClick.AddListener(() => OnForgeClicked?.Invoke());
        }

        /// <summary>
        /// Setup and show the village hub
        /// </summary>
        public void Setup()
        {
            // Enable/disable buttons based on what's available
            // For now, all buttons are enabled
            UpdateButtonStates();

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Update button interactability based on game state
        /// </summary>
        private void UpdateButtonStates()
        {
            // For now, enable all buttons
            // Later we can disable buttons based on:
            // - Barracks if no heroes
            // - Mission Board if mission already selected
            // - etc.

            if (tavernButton != null)
                tavernButton.interactable = true;

            if (missionBoardButton != null)
                missionBoardButton.interactable = true;

            if (barracksButton != null)
                barracksButton.interactable = true;

            if (forgeButton != null)
                forgeButton.interactable = true;
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
