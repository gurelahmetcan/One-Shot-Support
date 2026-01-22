using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Tavern screen for recruiting heroes
    /// Shows available heroes and allows recruitment
    /// </summary>
    public class TavernScreen : MonoBehaviour
    {
        [Header("Hero Slots")]
        [SerializeField] private Components.TavernHeroSlot[] heroSlots;

        [Header("UI References")]
        [SerializeField] private Button backButton;

        // Events
        public event Action<HeroData> OnHeroRecruited;
        public event Action OnBackClicked;

        private List<HeroData> availableHeroes;
        private int recruitmentCost = 50; // Base cost to recruit a hero

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackClicked);
            }

            // Setup hero slot callbacks
            if (heroSlots != null)
            {
                for (int i = 0; i < heroSlots.Length; i++)
                {
                    int index = i; // Capture index for closure
                    if (heroSlots[i] != null)
                    {
                        heroSlots[i].OnRecruitClicked += () => HandleHeroRecruit(index);
                    }
                }
            }
        }

        /// <summary>
        /// Setup and show the tavern with available heroes
        /// </summary>
        public void Setup(List<HeroData> heroes, int costPerHero)
        {
            availableHeroes = heroes;
            recruitmentCost = costPerHero;

            // Display heroes in slots
            for (int i = 0; i < heroSlots.Length; i++)
            {
                if (i < heroes.Count)
                {
                    heroSlots[i].Setup(heroes[i], recruitmentCost);
                    heroSlots[i].gameObject.SetActive(true);
                }
                else
                {
                    heroSlots[i].gameObject.SetActive(false);
                }
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Handle hero recruit button clicked
        /// </summary>
        private void HandleHeroRecruit(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= availableHeroes.Count) return;

            HeroData hero = availableHeroes[slotIndex];

            // Notify listeners (GameManager will handle removal from the list)
            OnHeroRecruited?.Invoke(hero);

            Debug.Log($"[Tavern] Recruited hero: {hero.heroName}");

            // NOTE: Don't modify availableHeroes here - it's a reference to GameManager's tavernHeroes
            // GameManager will remove the hero, then we refresh via the public Refresh method
        }

        /// <summary>
        /// Refresh hero slot displays after recruitment
        /// Called by UIManager after GameManager removes a hero
        /// </summary>
        public void Refresh()
        {
            for (int i = 0; i < heroSlots.Length; i++)
            {
                if (i < availableHeroes.Count)
                {
                    heroSlots[i].Setup(availableHeroes[i], recruitmentCost);
                    heroSlots[i].gameObject.SetActive(true);
                }
                else
                {
                    heroSlots[i].gameObject.SetActive(false);
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
