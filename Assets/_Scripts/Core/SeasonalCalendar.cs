using System;
using OneShotSupport.Data;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Manages the seasonal calendar system where 4 turns = 1 year
    /// Tracks current season, year, and total turn count
    /// </summary>
    [System.Serializable]
    public class SeasonalCalendar
    {
        private int currentTurn;
        private int currentYear;
        private Season currentSeason;

        // Events
        public event Action<Season, int> OnSeasonChanged; // (season, year)
        public event Action<int> OnYearChanged; // (newYear)

        /// <summary>
        /// Initialize the calendar to turn 1 (Spring, Year 1)
        /// </summary>
        public void Initialize()
        {
            currentTurn = 1;
            currentYear = 1;
            currentSeason = Season.Spring;
        }

        /// <summary>
        /// Advance to the next season/turn
        /// </summary>
        public void AdvanceSeason()
        {
            currentTurn++;

            // Calculate season (0-3 repeating)
            int seasonIndex = (currentTurn - 1) % 4;
            currentSeason = (Season)seasonIndex;

            // Calculate year (every 4 turns = 1 year)
            int newYear = ((currentTurn - 1) / 4) + 1;

            // Check if year changed
            if (newYear > currentYear)
            {
                currentYear = newYear;
                OnYearChanged?.Invoke(currentYear);
            }

            OnSeasonChanged?.Invoke(currentSeason, currentYear);
        }

        /// <summary>
        /// Get the current season
        /// </summary>
        public Season CurrentSeason => currentSeason;

        /// <summary>
        /// Get the current year
        /// </summary>
        public int CurrentYear => currentYear;

        /// <summary>
        /// Get the current turn number (absolute count since game start)
        /// </summary>
        public int CurrentTurn => currentTurn;

        /// <summary>
        /// Get the turn within the current year (1-4)
        /// </summary>
        public int TurnInYear => ((currentTurn - 1) % 4) + 1;

        /// <summary>
        /// Get a formatted display string for UI (e.g., "Spring, Year 1")
        /// </summary>
        public string GetDisplayString()
        {
            return $"{currentSeason}, Year {currentYear}";
        }

        /// <summary>
        /// Get a short display string for UI (e.g., "S1", "W3")
        /// </summary>
        public string GetShortDisplayString()
        {
            string seasonLetter = currentSeason switch
            {
                Season.Spring => "Sp",
                Season.Summer => "Su",
                Season.Autumn => "Au",
                Season.Winter => "Wi",
                _ => "?"
            };
            return $"{seasonLetter} Y{currentYear}";
        }

        /// <summary>
        /// Check if this is the start of a new year (Spring)
        /// </summary>
        public bool IsNewYear()
        {
            return currentSeason == Season.Spring;
        }
    }
}
