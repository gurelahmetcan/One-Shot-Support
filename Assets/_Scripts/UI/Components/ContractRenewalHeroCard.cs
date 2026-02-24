using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// A single row card on the ContractRenewalScreen.
    /// Shows the hero's portrait, name, level, and two action buttons.
    ///
    /// UNITY SETUP:
    ///   Create a prefab with this component.  Suggested layout:
    ///   Card (HorizontalLayoutGroup)
    ///   ├─ Portrait  (Image)
    ///   ├─ InfoColumn (VerticalLayoutGroup)
    ///   │   ├─ NameText   (TMP)
    ///   │   └─ LevelText  (TMP)
    ///   ├─ RenewButton   (Button)
    ///   └─ ReleaseButton (Button)
    ///
    ///   Assign the prefab to ContractRenewalScreen.cardPrefab in the inspector.
    /// </summary>
    public class ContractRenewalHeroCard : MonoBehaviour
    {
        [Header("Hero Info")]
        [SerializeField] private Image heroPortrait;
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Action Buttons")]
        [SerializeField] private Button renewButton;
        [SerializeField] private Button releaseButton;

        /// <summary>
        /// Fired when the player clicks Renew on this card.
        /// Parameters: the hero and this card instance (so the screen can track which card to remove).
        /// </summary>
        public event Action<HeroData, ContractRenewalHeroCard> OnRenewClicked;

        /// <summary>
        /// Fired when the player clicks Release on this card.
        /// </summary>
        public event Action<HeroData, ContractRenewalHeroCard> OnReleaseClicked;

        private HeroData hero;

        public HeroData Hero => hero;

        private void Awake()
        {
            if (renewButton != null)
                renewButton.onClick.AddListener(() => OnRenewClicked?.Invoke(hero, this));

            if (releaseButton != null)
                releaseButton.onClick.AddListener(() => OnReleaseClicked?.Invoke(hero, this));
        }

        /// <summary>
        /// Populate the card with hero data.
        /// </summary>
        public void Setup(HeroData heroData)
        {
            hero = heroData;

            if (heroPortrait != null)
            {
                heroPortrait.sprite = hero.portrait;
                heroPortrait.gameObject.SetActive(hero.portrait != null);
            }

            if (heroNameText != null)
                heroNameText.text = hero.heroName;

            if (levelText != null)
                levelText.text = $"Level {hero.level}";
        }

        /// <summary>
        /// Lock both buttons while negotiation is in progress for this card.
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (renewButton != null)  renewButton.interactable  = interactable;
            if (releaseButton != null) releaseButton.interactable = interactable;
        }
    }
}
