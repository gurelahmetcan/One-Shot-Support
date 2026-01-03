using UnityEngine;
using TMPro;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Displays the current day number
    /// Persistent across all screens
    /// </summary>
    public class DayCounter : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI dayText;

        [Header("Text Format")]
        public string textFormat = "Day {0}";

        /// <summary>
        /// Update the day display
        /// </summary>
        public void UpdateDay(int dayNumber)
        {
            if (dayText != null)
            {
                dayText.text = string.Format(textFormat, dayNumber);
            }
        }
    }
}
