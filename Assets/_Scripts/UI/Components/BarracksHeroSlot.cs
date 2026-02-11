using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.ScriptableObjects;
using OneShotSupport.Data;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// UI component representing a recruited hero in the barracks
    /// Displays hero identity, stats, and contract status
    /// </summary>
    public class BarracksHeroSlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image heroPortrait;
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI lifecycleText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI contractText;

        [Header("Education Focus")]
        [SerializeField] private Button focusButton;
        [SerializeField] private Image focusIcon;
        [SerializeField] private TextMeshProUGUI focusText;
        [SerializeField] private EducationFocusPopup focusPopup;

        private HeroData hero;

        private void Awake()
        {
            // Setup focus button listener
            if (focusButton != null)
            {
                focusButton.onClick.AddListener(OnFocusButtonClicked);
            }
        }

        /// <summary>
        /// Setup the hero slot with hero data
        /// </summary>
        public void Setup(HeroData heroData)
        {
            hero = heroData;

            // Update portrait
            if (heroPortrait != null && hero.portrait != null)
            {
                heroPortrait.sprite = hero.portrait;
                heroPortrait.gameObject.SetActive(true);
            }
            else if (heroPortrait != null)
            {
                heroPortrait.gameObject.SetActive(false);
            }

            // Update name
            if (heroNameText != null)
                heroNameText.text = hero.heroName;

            // Update level
            if (levelText != null)
                levelText.text = $"Level {hero.level}";

            // Update lifecycle
            if (lifecycleText != null)
            {
                lifecycleText.text = $"{hero.lifeStage} (Age {hero.currentAge})";
                lifecycleText.color = GetLifecycleColor(hero.lifeStage);
            }

            // Update stats (5-stat system)
            if (statsText != null)
            {
                statsText.text = $"Might: {hero.might} | Charm: {hero.charm} | Wit: {hero.wit}\n" +
                                $"Agility: {hero.agility} | Fort: {hero.fortitude} | HP: {hero.currentHP}/{hero.MaxHP}";
            }

            // Update contract
            if (contractText != null)
            {
                int yearsRemaining = Mathf.CeilToInt(hero.turnsRemainingInContract / 4f);
                contractText.text = $"Contract: {hero.turnsRemainingInContract} turns ({yearsRemaining}yr)";
            }

            // Update education focus
            UpdateFocusDisplay();
        }

        /// <summary>
        /// Update the education focus display
        /// </summary>
        private void UpdateFocusDisplay()
        {
            if (hero == null) return;

            // Update focus text
            if (focusText != null)
            {
                focusText.text = EducationFocusPopup.GetFocusDisplayName(hero.preferredEducationFocus);
            }

            // Update focus icon (if popup is assigned and has icons)
            if (focusIcon != null && focusPopup != null)
            {
                Sprite icon = focusPopup.GetIconForFocus(hero.preferredEducationFocus);
                if (icon != null)
                {
                    focusIcon.sprite = icon;
                    focusIcon.gameObject.SetActive(true);
                }
                else
                {
                    focusIcon.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Handle focus button clicked - open popup
        /// </summary>
        private void OnFocusButtonClicked()
        {
            if (hero == null || focusPopup == null) return;

            focusPopup.Show(hero);

            // Subscribe to focus selection to refresh display
            focusPopup.OnFocusSelected -= OnFocusChanged;
            focusPopup.OnFocusSelected += OnFocusChanged;
        }

        /// <summary>
        /// Handle focus changed - refresh display
        /// </summary>
        private void OnFocusChanged(EducationFocus newFocus)
        {
            UpdateFocusDisplay();
        }

        /// <summary>
        /// Clear the slot
        /// </summary>
        public void Clear()
        {
            if (heroPortrait != null)
                heroPortrait.gameObject.SetActive(false);

            if (heroNameText != null)
                heroNameText.text = "";

            if (levelText != null)
                levelText.text = "";

            if (lifecycleText != null)
                lifecycleText.text = "";

            if (statsText != null)
                statsText.text = "";

            if (contractText != null)
                contractText.text = "";

            hero = null;
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
