using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Popup for selecting a hero's education focus
    /// Displays all available education focuses with icons
    /// </summary>
    public class EducationFocusPopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button closeButton;

        [Header("Focus Buttons")]
        [SerializeField] private Button mightButton;
        [SerializeField] private Button charmButton;
        [SerializeField] private Button witButton;
        [SerializeField] private Button agilityButton;
        [SerializeField] private Button fortitudeButton;
        [SerializeField] private Button disciplineButton;

        [Header("Focus Icons")]
        [SerializeField] private Image mightIcon;
        [SerializeField] private Image charmIcon;
        [SerializeField] private Image witIcon;
        [SerializeField] private Image agilityIcon;
        [SerializeField] private Image fortitudeIcon;
        [SerializeField] private Image disciplineIcon;

        [Header("Focus Descriptions")]
        [SerializeField] private TextMeshProUGUI mightDesc;
        [SerializeField] private TextMeshProUGUI charmDesc;
        [SerializeField] private TextMeshProUGUI witDesc;
        [SerializeField] private TextMeshProUGUI agilityDesc;
        [SerializeField] private TextMeshProUGUI fortitudeDesc;
        [SerializeField] private TextMeshProUGUI disciplineDesc;

        // Events
        public event Action<EducationFocus> OnFocusSelected;

        private HeroData currentHero;

        private void Awake()
        {
            // Setup button listeners
            if (mightButton != null)
                mightButton.onClick.AddListener(() => SelectFocus(EducationFocus.Might));
            if (charmButton != null)
                charmButton.onClick.AddListener(() => SelectFocus(EducationFocus.Charm));
            if (witButton != null)
                witButton.onClick.AddListener(() => SelectFocus(EducationFocus.Wit));
            if (agilityButton != null)
                agilityButton.onClick.AddListener(() => SelectFocus(EducationFocus.Agility));
            if (fortitudeButton != null)
                fortitudeButton.onClick.AddListener(() => SelectFocus(EducationFocus.Fortitude));
            if (disciplineButton != null)
                disciplineButton.onClick.AddListener(() => SelectFocus(EducationFocus.Discipline));

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            // Setup descriptions
            SetupDescriptions();

            // Hide by default
            Hide();
        }

        /// <summary>
        /// Setup the description texts for each focus
        /// </summary>
        private void SetupDescriptions()
        {
            if (mightDesc != null)
                mightDesc.text = "Combat & Strength\n+5 Might, +1 Fort";
            if (charmDesc != null)
                charmDesc.text = "Leadership & Persuasion\n+5 Charm, +1 Wit";
            if (witDesc != null)
                witDesc.text = "Tactics & Investigation\n+5 Wit, +1 Charm";
            if (agilityDesc != null)
                agilityDesc.text = "Stealth & Reflexes\n+5 Agility, +1 Wit";
            if (fortitudeDesc != null)
                fortitudeDesc.text = "Endurance & Survival\n+5 Fortitude, Heal HP";
            if (disciplineDesc != null)
                disciplineDesc.text = "Reduces Greed\n-5 Greed, +1 All";
        }

        /// <summary>
        /// Show the popup for a specific hero
        /// </summary>
        public void Show(HeroData hero)
        {
            currentHero = hero;

            if (titleText != null)
                titleText.text = $"Training Focus: {hero.heroName}";

            // Highlight current focus
            HighlightCurrentFocus(hero.preferredEducationFocus);

            if (popupPanel != null)
                popupPanel.SetActive(true);
        }

        /// <summary>
        /// Highlight the currently selected focus
        /// </summary>
        private void HighlightCurrentFocus(EducationFocus focus)
        {
            // Reset all buttons to normal color
            ResetButtonColors();

            // Highlight the current focus
            Button currentButton = GetButtonForFocus(focus);
            if (currentButton != null)
            {
                var colors = currentButton.colors;
                colors.normalColor = new Color(0.3f, 0.8f, 0.3f); // Green highlight
                currentButton.colors = colors;
            }
        }

        /// <summary>
        /// Reset all button colors to default
        /// </summary>
        private void ResetButtonColors()
        {
            Color defaultColor = Color.white;

            SetButtonColor(mightButton, defaultColor);
            SetButtonColor(charmButton, defaultColor);
            SetButtonColor(witButton, defaultColor);
            SetButtonColor(agilityButton, defaultColor);
            SetButtonColor(fortitudeButton, defaultColor);
            SetButtonColor(disciplineButton, defaultColor);
        }

        /// <summary>
        /// Set button normal color
        /// </summary>
        private void SetButtonColor(Button button, Color color)
        {
            if (button != null)
            {
                var colors = button.colors;
                colors.normalColor = color;
                button.colors = colors;
            }
        }

        /// <summary>
        /// Get the button for a specific focus
        /// </summary>
        private Button GetButtonForFocus(EducationFocus focus)
        {
            return focus switch
            {
                EducationFocus.Might => mightButton,
                EducationFocus.Charm => charmButton,
                EducationFocus.Wit => witButton,
                EducationFocus.Agility => agilityButton,
                EducationFocus.Fortitude => fortitudeButton,
                EducationFocus.Discipline => disciplineButton,
                _ => null
            };
        }

        /// <summary>
        /// Handle focus selection
        /// </summary>
        private void SelectFocus(EducationFocus focus)
        {
            if (currentHero == null) return;

            // Update hero's preferred focus
            currentHero.preferredEducationFocus = focus;

            Debug.Log($"[EducationFocus] {currentHero.heroName} training focus set to: {focus}");

            // Notify listeners
            OnFocusSelected?.Invoke(focus);

            // Close popup
            Hide();
        }

        /// <summary>
        /// Hide the popup
        /// </summary>
        public void Hide()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);

            currentHero = null;
        }

        /// <summary>
        /// Get icon sprite for a specific focus (for external use)
        /// </summary>
        public Sprite GetIconForFocus(EducationFocus focus)
        {
            Image icon = focus switch
            {
                EducationFocus.Might => mightIcon,
                EducationFocus.Charm => charmIcon,
                EducationFocus.Wit => witIcon,
                EducationFocus.Agility => agilityIcon,
                EducationFocus.Fortitude => fortitudeIcon,
                EducationFocus.Discipline => disciplineIcon,
                _ => null
            };

            return icon?.sprite;
        }

        /// <summary>
        /// Get display name for a focus
        /// </summary>
        public static string GetFocusDisplayName(EducationFocus focus)
        {
            return focus switch
            {
                EducationFocus.Might => "Might",
                EducationFocus.Charm => "Charm",
                EducationFocus.Wit => "Wit",
                EducationFocus.Agility => "Agility",
                EducationFocus.Fortitude => "Fortitude",
                EducationFocus.Discipline => "Discipline",
                _ => "Unknown"
            };
        }
    }
}
