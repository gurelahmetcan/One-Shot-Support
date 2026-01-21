using UnityEngine;
using TMPro;
using OneShotSupport.Data;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Displays the current season and year (formerly day counter)
    /// Persistent across all screens
    /// </summary>
    public class DayCounter : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI dayText;

        [Header("Text Format")]
        [Tooltip("Format for day/turn number (backward compatibility)")]
        public string dayFormat = "Day {0}";

        [Tooltip("Format for season and year (e.g., 'Spring, Year 1')")]
        public string seasonFormat = "{0}, Year {1}";

        /// <summary>
        /// Update the day display (backward compatibility)
        /// </summary>
        public void UpdateDay(int dayNumber)
        {
            if (dayText != null)
            {
                dayText.text = string.Format(dayFormat, dayNumber);
            }
        }

        /// <summary>
        /// Update the season and year display
        /// </summary>
        public void UpdateSeason(Season season, int year)
        {
            if (dayText != null)
            {
                dayText.text = string.Format(seasonFormat, season.ToString(), year);
            }
        }

        /// <summary>
        /// Update with a custom formatted string
        /// </summary>
        public void UpdateDisplay(string displayText)
        {
            if (dayText != null)
            {
                dayText.text = displayText;
            }
        }
    }
}
