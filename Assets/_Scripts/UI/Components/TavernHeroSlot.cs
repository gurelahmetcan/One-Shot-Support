using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// UI component representing a hero available for recruitment in the tavern
    /// </summary>
    public class TavernHeroSlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private TextMeshProUGUI tierText;
        [SerializeField] private TextMeshProUGUI ageText;
        [SerializeField] private TextMeshProUGUI lifecycleText;
        [SerializeField] private TextMeshProUGUI perkText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image heroPortrait;
        [SerializeField] private Button recruitButton;

        // Events
        public event Action OnRecruitClicked;

        private HeroData hero;
        private int cost;

        private void Awake()
        {
            if (recruitButton != null)
            {
                recruitButton.onClick.AddListener(HandleRecruitClick);
            }
        }

        /// <summary>
        /// Setup the hero slot with hero data
        /// </summary>
        public void Setup(HeroData heroData, int recruitmentCost)
        {
            hero = heroData;
            cost = recruitmentCost;

            // Update UI
            if (heroNameText != null)
                heroNameText.text = hero.heroName;

            if (tierText != null)
                tierText.text = $"Tier: {hero.tier}";

            if (ageText != null)
                ageText.text = $"Age: {Mathf.FloorToInt(hero.age)}";

            if (lifecycleText != null)
            {
                lifecycleText.text = $"Stage: {hero.lifecycleStage}";
                lifecycleText.color = GetLifecycleColor(hero.lifecycleStage);
            }

            if (perkText != null)
            {
                if (hero.perk == Perk.None)
                    perkText.text = "Perk: None";
                else
                    perkText.text = $"Perk: {hero.perk}";
            }

            if (costText != null)
                costText.text = $"Cost: {cost} Gold";

            if (heroPortrait != null && hero.portrait != null)
            {
                heroPortrait.sprite = hero.portrait;
            }
        }

        /// <summary>
        /// Handle recruit button click
        /// </summary>
        private void HandleRecruitClick()
        {
            OnRecruitClicked?.Invoke();
        }

        /// <summary>
        /// Get color for lifecycle stage
        /// </summary>
        private Color GetLifecycleColor(HeroLifecycleStage stage)
        {
            return stage switch
            {
                HeroLifecycleStage.Rookie => new Color(0.3f, 0.8f, 0.3f), // Green
                HeroLifecycleStage.Prime => new Color(0.9f, 0.7f, 0.2f),  // Gold
                HeroLifecycleStage.Veteran => new Color(0.7f, 0.3f, 0.9f), // Purple
                HeroLifecycleStage.Retired => new Color(0.5f, 0.5f, 0.5f), // Gray
                _ => Color.white
            };
        }
    }
}
