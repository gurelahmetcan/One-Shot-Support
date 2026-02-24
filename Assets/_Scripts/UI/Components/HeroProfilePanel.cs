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
        [SerializeField] private Slider xpBar;
        [SerializeField] private TextMeshProUGUI xpText;

        // ── Vital Stats ───────────────────────────────────────────────────────
        [Header("Vital Stats")]
        [SerializeField] private TextMeshProUGUI ageText;
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthText;

        // ── Status ────────────────────────────────────────────────────────────
        [Header("Status")]
        [SerializeField] private TextMeshProUGUI injuryText;
        [SerializeField] private TextMeshProUGUI perkText;

        // ── Contract ──────────────────────────────────────────────────────────
        [Header("Contract")]
        [SerializeField] private TextMeshProUGUI contractText;

        // ── Bond Level ────────────────────────────────────────────────────────
        [Header("Bond Level")]
        [SerializeField] private TextMeshProUGUI bondLevelText;
        /// <summary>
        /// Optional array of 10 star Images that light up gold to show bond level (0-10).
        /// Leave empty to rely on bondLevelText alone.
        /// </summary>
        [SerializeField] private Image[] bondStars;

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

        // Colors for bond stars
        private static readonly Color StarActive   = new Color(1.00f, 0.85f, 0.10f); // gold
        private static readonly Color StarInactive = new Color(0.25f, 0.25f, 0.25f); // dark grey

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
                    heroPortrait.sprite = currentHero.portrait;
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
                xpBar.minValue = 0f;
                xpBar.maxValue = 1f;
                xpBar.value = progress;
            }

            if (xpText != null)
                xpText.text = $"{currentHero.currentXP} / {xpNeeded} XP";
        }

        // ── Vital Stats ───────────────────────────────────────────────────────

        private void PopulateVitalStats()
        {
            // Age line shows lifecycle stage for context
            if (ageText != null)
                ageText.text = $"Age: {currentHero.currentAge}  ({currentHero.lifeStage})";

            int cur = currentHero.currentHP;
            int max = currentHero.MaxHP;
            float ratio = max > 0 ? Mathf.Clamp01((float)cur / max) : 0f;

            if (healthText != null)
                healthText.text = $"{cur} / {max}";

            if (healthBar != null)
            {
                healthBar.minValue = 0f;
                healthBar.maxValue = 1f;
                healthBar.value = ratio;

                // Tint fill to reflect health status
                Image fill = healthBar.fillRect != null
                    ? healthBar.fillRect.GetComponent<Image>()
                    : null;

                if (fill != null)
                {
                    fill.color = ratio > 0.5f ? HealthColorHigh
                               : ratio > 0.25f ? HealthColorMedium
                               : HealthColorLow;
                }
            }
        }

        // ── Status ────────────────────────────────────────────────────────────

        private void PopulateStatus()
        {
            // Injury
            if (injuryText != null)
            {
                bool hasInjury = !string.IsNullOrEmpty(currentHero.currentInjury);
                injuryText.text = hasInjury ? currentHero.currentInjury : "None";
                injuryText.color = hasInjury ? HealthColorLow : Color.white;
            }

            // Perk — uses the first trait on the hero
            if (perkText != null)
            {
                if (currentHero.traits != null && currentHero.traits.Count > 0 && currentHero.traits[0] != null)
                {
                    HeroTrait trait = currentHero.traits[0];
                    string desc = string.IsNullOrEmpty(trait.description) ? "" : $": {trait.description}";
                    perkText.text = $"{trait.traitName}{desc}";
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

            contractText.text = $"{salary:0}g / Season     •     {seasonsLeft} {seasonLabel} Left";
        }

        // ── Bond Level ────────────────────────────────────────────────────────

        private void PopulateBond()
        {
            int bond = currentHero.bondLevel;

            if (bondLevelText != null)
                bondLevelText.text = $"Bond  {bond} / 10";

            if (bondStars != null)
            {
                for (int i = 0; i < bondStars.Length; i++)
                {
                    if (bondStars[i] != null)
                        bondStars[i].color = i < bond ? StarActive : StarInactive;
                }
            }
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
