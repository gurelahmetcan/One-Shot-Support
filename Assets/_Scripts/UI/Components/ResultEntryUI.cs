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

            if (resultText != null)
            {
                string resultString = result.succeeded ? "<color=green>SUCCESS!</color>" : "<color=red>FAILED</color>";
                resultText.text = resultString;
            }

            for (int i = 0; i < result.stars; i++)
            {
                starsList[i].gameObject.SetActive(true);
            }

            if (reputationChangeText != null)
            {
                string sign = result.reputationChange > 0 ? "+" : "";
                Color color = result.reputationChange > 0 ? Color.green : Color.red;
                reputationChangeText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{sign}{result.reputationChange}</color>";
            }

            if (successChanceText != null)
                successChanceText.text = $"Chance: {result.successChance}%";

        }
    }
}
