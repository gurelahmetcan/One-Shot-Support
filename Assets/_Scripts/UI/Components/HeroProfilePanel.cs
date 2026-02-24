using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.ScriptableObjects;
using OneShotSupport.Data;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Hero Profile Panel - shown when clicking a hero in the Barracks.
    /// Displays detailed hero information: identity, level/XP, health, injury,
    /// perk (trait), contract details, skill pentagram, portrait, and bond level.
    ///
    /// UNITY SETUP:
    ///   - Create a full-screen Canvas overlay panel as a child of your Canvas.
    ///   - Assign all [SerializeField] references via the Inspector.
    ///   - The panel starts inactive; call Show(hero) to open it.
    ///   - Pentagon stat display uses the existing PentagonStatDisplay component.
    ///
    /// LAYOUT SUGGESTION (matches screenshot style):
    ///   Panel (full-screen semi-transparent background)
    ///   └─ ProfileCard (centered card ~600x700)
    ///      ├─ Row: Portrait (Image) | NameText | CloseButton
    ///      ├─ Row: LevelText | XpBar (Slider) | XpText
    ///      ├─ Row: AgeText
    ///      ├─ Row: HealthLabel | HealthBar (Slider) | HealthText
    ///      ├─ Row: InjuryText
    ///      ├─ Row: PerkText
    ///      ├─ Row: ContractText
    ///      ├─ PentagonStatDisplay (center)
    ///      └─ Row: BondText | BondStars[]
    /// </summary>
    public class HeroProfilePanel : MonoBehaviour
    {
        // ── Identity ──────────────────────────────────────────────────────────
        [Header("Identity")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image heroPortrait;

        // ── Level & XP ────────────────────────────────────────────────────────
        [Header("Level & XP")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image xpBar;
        [SerializeField] private TextMeshProUGUI xpText;

        // ── Vital Stats ───────────────────────────────────────────────────────
        [Header("Vital Stats")]
        [SerializeField] private TextMeshProUGUI ageText;
        [SerializeField] private TextMeshProUGUI healthText;

        // ── Status ────────────────────────────────────────────────────────────
        [Header("Status")]
        [SerializeField] private TextMeshProUGUI injuryText;
        [SerializeField] private TextMeshProUGUI perkText;

        // ── Contract ──────────────────────────────────────────────────────────
        [Header("Contract")]
        [SerializeField] private TextMeshProUGUI contractText;
        [SerializeField] private TextMeshProUGUI expiryText;

        // ── Education Focus ───────────────────────────────────────────────────
        [Header("Education Focus")]
        [Tooltip("Icon image that displays the current education focus sprite.")]
        [SerializeField] private Image educationFocusIcon;
        [Tooltip("Text label showing the focus name, e.g. 'Training: Might'.")]
        [SerializeField] private TextMeshProUGUI educationFocusText;
        [Tooltip("Reference to the scene's EducationFocusPopup — used to fetch focus icons.")]
        [SerializeField] private EducationFocusPopup educationFocusSource;

        // ── Bond Level ────────────────────────────────────────────────────────
        [Header("Bond Level")]
        [SerializeField] private TextMeshProUGUI bondLevelText;
        
        // ── Skill Pentagram ───────────────────────────────────────────────────
        [Header("Skill Pentagram")]
        [SerializeField] private PentagonStatDisplay pentagonStatDisplay;

        // ── Controls ──────────────────────────────────────────────────────────
        [Header("Controls")]
        [SerializeField] private Button closeButton;

        // Colors for health bar fill
        private static readonly Color HealthColorHigh   = new Color(0.20f, 0.78f, 0.20f); // green
        private static readonly Color HealthColorMedium = new Color(0.90f, 0.70f, 0.10f); // gold
        private static readonly Color HealthColorLow    = new Color(0.88f, 0.20f, 0.20f); // red
        
        private HeroData currentHero;

        /// <summary>Fired when the close button is pressed.</summary>
        public event Action OnClosed;

        // ── Unity lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Populate and show the profile panel for <paramref name="hero"/>.</summary>
        public void Show(HeroData hero)
        {
            currentHero = hero;
            PopulateAll();
            gameObject.SetActive(true);
        }

        /// <summary>Hide the panel.</summary>
        public void Close()
        {
            gameObject.SetActive(false);
            OnClosed?.Invoke();
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private void PopulateAll()
        {
            if (currentHero == null) return;

            PopulateIdentity();
            PopulateLevelXP();
            PopulateVitalStats();
            PopulateStatus();
            PopulateContract();
            PopulateEducationFocus();
            PopulateBond();
            PopulatePentagon();
        }

        // ── Identity ──────────────────────────────────────────────────────────

        private void PopulateIdentity()
        {
            if (nameText != null)
                nameText.text = currentHero.heroName;

            if (heroPortrait != null)
            {
                if (currentHero.portrait != null)
                {
                    heroPortrait.sprite = currentHero.cardSprite;
                    heroPortrait.color = Color.white;
                    heroPortrait.gameObject.SetActive(true);
                }
                else
                {
                    heroPortrait.gameObject.SetActive(false);
                }
            }
        }

        // ── Level & XP ────────────────────────────────────────────────────────

        private void PopulateLevelXP()
        {
            if (levelText != null)
                levelText.text = $"Level {currentHero.level}";

            int xpNeeded = currentHero.XPForNextLevel;
            float progress = xpNeeded > 0 ? Mathf.Clamp01((float)currentHero.currentXP / xpNeeded) : 0f;

            if (xpBar != null)
            {
                xpBar.fillAmount = progress;
            }

            if (xpText != null)
                xpText.text = $"{currentHero.currentXP}/{xpNeeded} XP";
        }

        // ── Vital Stats ───────────────────────────────────────────────────────

        private void PopulateVitalStats()
        {
            // Age line shows lifecycle stage for context
            if (ageText != null)
                ageText.text = $"Age: {currentHero.currentAge} ({currentHero.lifeStage})";

            int cur = currentHero.currentHP;
            int max = currentHero.MaxHP;

            if (healthText != null)
                healthText.text = $"HP: {cur}/{max}";
        }

        // ── Status ────────────────────────────────────────────────────────────

        private void PopulateStatus()
        {
            // Injury
            if (injuryText != null)
            {
                bool hasInjury = !string.IsNullOrEmpty(currentHero.currentInjury);
                injuryText.text = hasInjury ? currentHero.currentInjury : "Healthy";
                injuryText.color = hasInjury ? HealthColorLow : Color.green;
            }

            // Perk — uses the first trait on the hero
            if (perkText != null)
            {
                if (currentHero.traits != null && currentHero.traits.Count > 0 && currentHero.traits[0] != null)
                {
                    HeroTrait trait = currentHero.traits[0];
                    string desc = string.IsNullOrEmpty(trait.description) ? "" : $": {trait.description}";
                    perkText.text = $"Perk: {trait.traitName}{desc}";
                }
                else
                {
                    perkText.text = "—";
                }
            }
        }

        // ── Contract ──────────────────────────────────────────────────────────

        private void PopulateContract()
        {
            if (contractText == null) return;

            // dailySalary is paid per turn/season
            float salary = currentHero.GetEffectiveSalary();
            int seasonsLeft = currentHero.turnsRemainingInContract;
            string seasonLabel = seasonsLeft == 1 ? "Season" : "Seasons";

            contractText.text = $"{salary:0}g/Season";
            expiryText.text = $"{seasonsLeft} {seasonLabel} Left";
        }

        // ── Education Focus ───────────────────────────────────────────────────

        private void PopulateEducationFocus()
        {
            EducationFocus focus = currentHero.preferredEducationFocus;

            // Icon — sourced from the shared EducationFocusPopup
            if (educationFocusIcon != null)
            {
                Sprite icon = educationFocusSource != null
                    ? educationFocusSource.GetIconForFocus(focus)
                    : null;

                if (icon != null)
                {
                    educationFocusIcon.sprite = icon;
                    educationFocusIcon.color = Color.white;
                    educationFocusIcon.gameObject.SetActive(true);
                }
                else
                {
                    educationFocusIcon.gameObject.SetActive(false);
                }
            }

            // Label
            if (educationFocusText != null)
                educationFocusText.text = $"Focus: {EducationFocusPopup.GetFocusDisplayName(focus)}";
        }

        // ── Bond Level ────────────────────────────────────────────────────────

        private void PopulateBond()
        {
            int bond = currentHero.bondLevel;

            if (bondLevelText != null)
                bondLevelText.text = $"Bond  {bond}/10";
        }

        // ── Skill Pentagram ───────────────────────────────────────────────────

        private void PopulatePentagon()
        {
            if (pentagonStatDisplay == null) return;

            pentagonStatDisplay.SetStats(
                currentHero.might,
                currentHero.charm,
                currentHero.wit,
                currentHero.agility,
                currentHero.fortitude
            );
        }
    }
}
