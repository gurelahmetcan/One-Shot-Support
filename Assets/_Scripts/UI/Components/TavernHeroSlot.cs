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
    /// Displays hero stats, contract info, and recruitment cost
    /// </summary>
    public class TavernHeroSlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI ageText;
        [SerializeField] private TextMeshProUGUI lifecycleText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI contractText;
        [SerializeField] private TextMeshProUGUI traitsText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image heroPortrait;
        [SerializeField] private Button recruitButton;

        // Events
        public event Action OnRecruitClicked;

        private HeroData hero;

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
        public void Setup(HeroData heroData)
        {
            hero = heroData;

            // Update UI
            if (heroNameText != null)
                heroNameText.text = hero.heroName;

            if (levelText != null)
                levelText.text = $"Level {hero.level}";

            if (ageText != null)
                ageText.text = $"Age: {hero.currentAge}";

            if (lifecycleText != null)
            {
                lifecycleText.text = $"{hero.lifeStage}";
                lifecycleText.color = GetLifecycleColor(hero.lifeStage);
            }

            if (statsText != null)
            {
                // Display core stats
                statsText.text = $"Prowess: {hero.prowess} | Charisma: {hero.charisma}\n" +
                                $"Vitality: {hero.maxVitality} | Greed: {hero.greed}";
            }

            if (contractText != null)
            {
                // Display contract info (simplified - no loot cut)
                contractText.text = $"Contract: {hero.contractLengthInYears}yr | Salary: {hero.dailySalary}g/turn";
            }

            if (traitsText != null)
            {
                // Display traits
                if (hero.traits != null && hero.traits.Count > 0)
                {
                    string traitNames = "";
                    foreach (var trait in hero.traits)
                    {
                        if (trait != null)
                            traitNames += trait.traitName + ", ";
                    }
                    traitsText.text = $"Traits: {traitNames.TrimEnd(',', ' ')}";
                }
                else
                {
                    traitsText.text = "Traits: None";
                }
            }
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
