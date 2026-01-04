using System.Collections.Generic;
using OneShotSupport.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneShotSupport.UI
{
    public class ResultEntryUI : MonoBehaviour
    {
        [SerializeField] private Image heroImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI reputationChangeText;
        [SerializeField] private TextMeshProUGUI successChanceText;

        [SerializeField] private List<GameObject> starsList;

        public void Initialize(HeroResult result)
        {
            // Populate data
            if (nameText != null)
                nameText.text = result.hero.heroName;

            if (heroImage != null)
                heroImage.sprite = result.hero.portrait;

            if (resultText != null)
            {
                string resultString = result.succeeded ? "<color=#789566>SUCCESS!</color>" : "<color=#E44949>FAILED</color>";
                resultText.text = resultString;
            }

            for (int i = 0; i < result.stars; i++)
            {
                starsList[i].gameObject.SetActive(true);
            }

            if (reputationChangeText != null)
            {
                string sign = result.reputationChange > 0 ? "+" : "";
                Color color = result.reputationChange > 0 ? new Color32(120, 149, 102, 255) : new Color32(228, 73, 73, 255);
                reputationChangeText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{sign}{result.reputationChange}</color>";
            }

            if (successChanceText != null)
                successChanceText.text = $"Chance: {result.successChance}%";

        }
    }
}
