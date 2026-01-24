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
        [SerializeField] private Components.NegotiationPanel negotiationPanel;

        // Events
        public event Action<HeroData, Core.ContractOffer> OnHeroRecruited;
        public event Action<HeroData> OnHeroWalkedAway;
        public event Action OnBackClicked;

        private List<HeroData> availableHeroes;
        private int recruitmentCost = 50; // Base cost to recruit a hero

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackClicked);
            }

            // Setup hero slot callbacks (now opens negotiation instead of direct recruit)
            if (heroSlots != null)
            {
                for (int i = 0; i < heroSlots.Length; i++)
                {
                    int index = i; // Capture index for closure
                    if (heroSlots[i] != null)
                    {
                        heroSlots[i].OnRecruitClicked += () => HandleNegotiateClicked(index);
                    }
                }
            }

            // Setup negotiation panel callbacks
            if (negotiationPanel != null)
            {
                negotiationPanel.OnNegotiationAccepted += HandleNegotiationAccepted;
                negotiationPanel.OnHeroWalkedAway += HandleHeroWalkedAway;
                negotiationPanel.OnNegotiationCancelled += HandleNegotiationCancelled;
                negotiationPanel.gameObject.SetActive(false); // Start hidden
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
        /// Handle hero negotiate button clicked
        /// Opens negotiation panel instead of directly recruiting
        /// </summary>
        private void HandleNegotiateClicked(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= availableHeroes.Count) return;

            HeroData hero = availableHeroes[slotIndex];

            // Don't open if panel is already open
            if (negotiationPanel != null && negotiationPanel.gameObject.activeSelf)
            {
                Debug.LogWarning("[Tavern] Negotiation panel already open!");
                return;
            }

            // Get player's current gold
            int playerGold = Core.GoldManager.Instance != null ? Core.GoldManager.Instance.CurrentGold : 0;

            // Open negotiation panel
            if (negotiationPanel != null)
            {
                negotiationPanel.Setup(hero, playerGold);
                Debug.Log($"[Tavern] Opened negotiation with: {hero.heroName}");
            }
            else
            {
                Debug.LogError("[Tavern] NegotiationPanel reference is missing!");
            }
        }

        /// <summary>
        /// Handle successful negotiation - hero accepts offer
        /// </summary>
        private void HandleNegotiationAccepted(HeroData hero, Core.ContractOffer offer)
        {
            Debug.Log($"[Tavern] Negotiation successful! Recruiting {hero.heroName}");

            // Notify listeners (GameManager will handle gold deduction and hero recruitment)
            OnHeroRecruited?.Invoke(hero, offer);

            // Refresh display
            Refresh();
        }

        /// <summary>
        /// Handle hero walking away from negotiation
        /// </summary>
        private void HandleHeroWalkedAway(HeroData hero)
        {
            Debug.LogWarning($"[Tavern] {hero.heroName} walked away from negotiations!");

            // Notify listeners (GameManager will mark hero as walked away)
            OnHeroWalkedAway?.Invoke(hero);

            // Refresh display (hero should now show as grayed out/locked)
            Refresh();
        }

        /// <summary>
        /// Handle negotiation cancelled by player
        /// </summary>
        private void HandleNegotiationCancelled()
        {
            Debug.Log("[Tavern] Negotiation cancelled");
            // Nothing special to do - just back to tavern view
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
