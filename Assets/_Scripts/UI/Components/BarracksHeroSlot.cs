using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// UI component representing a recruited hero in the barracks
    /// </summary>
    public class BarracksHeroSlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image heroPortrait;
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private TextMeshProUGUI tierText;

        private HeroData hero;

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

            // Update tier/rank
            if (tierText != null)
                tierText.text = $"Tier {hero.tier}";
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

            if (tierText != null)
                tierText.text = "";

            hero = null;
        }
    }
}
