using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Barracks screen for viewing recruited heroes.
    /// Displays all heroes currently in the player's roster.
    /// Clicking a hero slot opens the HeroProfilePanel overlay.
    /// </summary>
    public class BarracksScreen : MonoBehaviour
    {
        [Header("Hero Slots")]
        [SerializeField] private Components.BarracksHeroSlot[] heroSlots;

        [Header("UI References")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI capacityText;

        [Header("Hero Profile")]
        [Tooltip("Full-screen overlay panel that shows detailed hero information.")]
        [SerializeField] private Components.HeroProfilePanel heroProfilePanel;

        // Events
        public event Action OnBackClicked;

        private List<HeroData> recruitedHeroes;
        private int maxCapacity;

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackClicked);
            }
        }

        /// <summary>
        /// Setup and show the barracks with recruited heroes
        /// </summary>
        public void Setup(List<HeroData> heroes, int maxBarracksCapacity)
        {
            recruitedHeroes = heroes;
            maxCapacity = maxBarracksCapacity;

            // Update capacity text
            if (capacityText != null)
            {
                capacityText.text = $"Heroes: {heroes.Count}/{maxCapacity}";
            }

            // Display heroes in slots and subscribe to profile events
            for (int i = 0; i < heroSlots.Length; i++)
            {
                // Unsubscribe first to avoid duplicate listeners on re-open
                heroSlots[i].OnHeroSelected -= OpenHeroProfile;

                if (i < heroes.Count)
                {
                    heroSlots[i].Setup(heroes[i]);
                    heroSlots[i].OnHeroSelected += OpenHeroProfile;
                    heroSlots[i].gameObject.SetActive(true);
                }
                else
                {
                    heroSlots[i].Clear();
                    heroSlots[i].gameObject.SetActive(false);
                }
            }

            // Make sure the profile panel starts closed
            if (heroProfilePanel != null)
                heroProfilePanel.gameObject.SetActive(false);

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Open the Hero Profile Panel for the selected hero.
        /// </summary>
        private void OpenHeroProfile(HeroData hero)
        {
            if (heroProfilePanel != null)
                heroProfilePanel.Show(hero);
        }

        /// <summary>
        /// Refresh the display (e.g., after recruiting a new hero)
        /// </summary>
        public void Refresh()
        {
            if (recruitedHeroes != null)
            {
                // Update capacity text
                if (capacityText != null)
                {
                    capacityText.text = $"Heroes: {recruitedHeroes.Count}/{maxCapacity}";
                }

                // Refresh hero slots
                for (int i = 0; i < heroSlots.Length; i++)
                {
                    if (i < recruitedHeroes.Count)
                    {
                        heroSlots[i].Setup(recruitedHeroes[i]);
                        heroSlots[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        heroSlots[i].Clear();
                        heroSlots[i].gameObject.SetActive(false);
                    }
                }
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
        /// Hide the screen
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
