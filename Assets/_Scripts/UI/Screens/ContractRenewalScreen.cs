using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.Core;
using OneShotSupport.ScriptableObjects;
using OneShotSupport.UI.Components;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Screen shown at the end of a season when one or more heroes have expired contracts.
    /// The player must Renew (opens full NegotiationPanel) or Release each hero before
    /// the Continue button becomes active and the next season begins.
    ///
    /// UNITY SETUP:
    ///   1. Create a full-screen Panel as a child of the main Canvas.
    ///   2. Add a ScrollRect whose Content transform is assigned to cardContainer.
    ///   3. Create a ContractRenewalHeroCard prefab and assign it to cardPrefab.
    ///   4. Assign the scene's existing NegotiationPanel to negotiationPanel
    ///      (can be shared with TavernScreen — they are never open simultaneously).
    ///   5. Add a header TMP text and a Continue Button; wire them up in the inspector.
    ///   6. Assign this screen to UIManager.contractRenewalScreen.
    ///
    /// FLOW:
    ///   Setup(heroes)  ─►  cards spawned
    ///   [Renew]        ─►  NegotiationPanel opens for that hero
    ///                        ├─ accepted   → CompleteHeroRenewal + RemoveCard
    ///                        ├─ walked-away → ReleaseHero + RemoveCard
    ///                        └─ cancelled  → back to this screen, card stays
    ///   [Release]      ─►  ReleaseHero + RemoveCard immediately
    ///   [all cards gone] → Continue button becomes active
    ///   [Continue]     ─►  OnContinueClicked → UIManager → GameManager.CompleteContractRenewal()
    /// </summary>
    public class ContractRenewalScreen : MonoBehaviour
    {
        // ── Card list ─────────────────────────────────────────────────────────
        [Header("Card List")]
        [Tooltip("Content transform of the ScrollRect where cards are instantiated.")]
        [SerializeField] private Transform cardContainer;

        [Tooltip("Prefab with ContractRenewalHeroCard component.")]
        [SerializeField] private ContractRenewalHeroCard cardPrefab;

        // ── Negotiation ───────────────────────────────────────────────────────
        [Header("Negotiation")]
        [Tooltip("The shared NegotiationPanel used to bargain renewal terms.")]
        [SerializeField] private NegotiationPanel negotiationPanel;

        // ── UI ────────────────────────────────────────────────────────────────
        [Header("UI")]
        [Tooltip("Header text showing how many contracts remain.")]
        [SerializeField] private TextMeshProUGUI headerText;

        [Tooltip("Becomes interactable only when all cards are resolved.")]
        [SerializeField] private Button continueButton;

        // ── Events ────────────────────────────────────────────────────────────
        /// <summary>Fired when all heroes are resolved and the player presses Continue.</summary>
        public event Action OnContinueClicked;

        // ── State ─────────────────────────────────────────────────────────────
        private readonly List<ContractRenewalHeroCard> activeCards = new List<ContractRenewalHeroCard>();
        private ContractRenewalHeroCard pendingCard; // card whose hero is currently in negotiation

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(HandleContinueClicked);

            if (negotiationPanel != null)
            {
                negotiationPanel.OnNegotiationAccepted += HandleNegotiationAccepted;
                negotiationPanel.OnHeroWalkedAway      += HandleNegotiationWalkAway;
                negotiationPanel.OnNegotiationCancelled += HandleNegotiationCancelled;
                negotiationPanel.gameObject.SetActive(false);
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Populate the screen with the list of heroes whose contracts just expired.
        /// </summary>
        public void Setup(List<HeroData> expiredHeroes)
        {
            // Destroy any cards left over from a previous call
            foreach (var old in activeCards)
                if (old != null) Destroy(old.gameObject);
            activeCards.Clear();
            pendingCard = null;

            // Spawn one card per hero
            foreach (var hero in expiredHeroes)
            {
                if (cardPrefab == null || cardContainer == null) break;

                var card = Instantiate(cardPrefab, cardContainer);
                card.Setup(hero);
                card.OnRenewClicked   += HandleRenewClicked;
                card.OnReleaseClicked += HandleReleaseClicked;
                activeCards.Add(card);
            }

            RefreshHeader();
            RefreshContinueButton();
            gameObject.SetActive(true);
        }

        /// <summary>Hide the screen.</summary>
        public void Hide()
        {
            if (negotiationPanel != null)
                negotiationPanel.gameObject.SetActive(false);

            gameObject.SetActive(false);
        }

        // ── Card button handlers ──────────────────────────────────────────────

        private void HandleRenewClicked(HeroData hero, ContractRenewalHeroCard card)
        {
            if (negotiationPanel == null)
            {
                Debug.LogError("[ContractRenewalScreen] NegotiationPanel reference is missing!");
                return;
            }

            if (negotiationPanel.gameObject.activeSelf)
            {
                Debug.LogWarning("[ContractRenewalScreen] Negotiation already in progress.");
                return;
            }

            // Lock all cards while negotiation is open so the player can't click multiple
            SetAllCardsInteractable(false);
            pendingCard = card;

            int gold = GameManager.Instance?.goldManager != null
                ? GameManager.Instance.goldManager.CurrentGold
                : 0;

            negotiationPanel.Setup(hero, gold);
        }

        private void HandleReleaseClicked(HeroData hero, ContractRenewalHeroCard card)
        {
            GameManager.Instance?.ReleaseHero(hero);
            RemoveCard(card);
        }

        // ── Negotiation result handlers ───────────────────────────────────────

        private void HandleNegotiationAccepted(HeroData hero, ContractOffer offer)
        {
            // NegotiationPanel already called FinalizeContract (sets new terms on hero).
            // Tell GameManager to deduct signing bonus.
            GameManager.Instance?.CompleteHeroRenewal(hero, offer);
            FinishNegotiation(removeCard: true);
        }

        private void HandleNegotiationWalkAway(HeroData hero)
        {
            // Walk-away during renewal = immediate release
            GameManager.Instance?.ReleaseHero(hero);
            FinishNegotiation(removeCard: true);
        }

        private void HandleNegotiationCancelled()
        {
            // Player backed out — card stays, all cards re-enabled
            FinishNegotiation(removeCard: false);
        }

        private void FinishNegotiation(bool removeCard)
        {
            SetAllCardsInteractable(true);

            if (removeCard && pendingCard != null)
                RemoveCard(pendingCard);

            pendingCard = null;
        }

        // ── Continue handler ──────────────────────────────────────────────────

        private void HandleContinueClicked()
        {
            OnContinueClicked?.Invoke();
            Hide();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void RemoveCard(ContractRenewalHeroCard card)
        {
            if (card == null) return;

            activeCards.Remove(card);
            card.OnRenewClicked   -= HandleRenewClicked;
            card.OnReleaseClicked -= HandleReleaseClicked;
            Destroy(card.gameObject);

            RefreshHeader();
            RefreshContinueButton();
        }

        private void SetAllCardsInteractable(bool interactable)
        {
            foreach (var c in activeCards)
                if (c != null) c.SetInteractable(interactable);
        }

        private void RefreshHeader()
        {
            if (headerText == null) return;

            int remaining = activeCards.Count;
            headerText.text = remaining > 0
                ? $"Expired Contracts  —  {remaining} Remaining"
                : "All Contracts Resolved";
        }

        private void RefreshContinueButton()
        {
            if (continueButton != null)
                continueButton.interactable = activeCards.Count == 0;
        }
    }
}
