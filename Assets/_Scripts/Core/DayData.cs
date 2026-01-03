using System.Collections.Generic;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Holds all data for the current game day
    /// </summary>
    public class DayData
    {
        public int dayNumber;
        public List<ItemData> availableItems;
        public List<HeroResult> heroResults;
        public int currentHeroIndex;
        public int inspiringBonus; // Bonus from Inspiring perk to pass to next hero

        public DayData(int day)
        {
            dayNumber = day;
            availableItems = new List<ItemData>();
            heroResults = new List<HeroResult>();
            currentHeroIndex = 0;
            inspiringBonus = 0;
        }

        /// <summary>
        /// Get the current hero being consulted
        /// </summary>
        public HeroResult GetCurrentHero()
        {
            if (currentHeroIndex >= 0 && currentHeroIndex < heroResults.Count)
                return heroResults[currentHeroIndex];
            return null;
        }

        /// <summary>
        /// Move to next hero
        /// </summary>
        public bool MoveToNextHero()
        {
            currentHeroIndex++;
            return currentHeroIndex < heroResults.Count;
        }

        /// <summary>
        /// Check if all heroes have been consulted
        /// </summary>
        public bool AllHeroesConsulted()
        {
            return currentHeroIndex >= heroResults.Count;
        }
    }
}
