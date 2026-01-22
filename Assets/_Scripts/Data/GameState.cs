namespace OneShotSupport.Data
{
    /// <summary>
    /// Game state machine states
    /// </summary>
    public enum GameState
    {
        DayStart,       // Initialize new day/season
        VillageHub,     // Main navigation hub (like Darkest Dungeon village)
        MissionBoard,   // Select mission for the season
        Tavern,         // Recruit heroes
        Barracks,       // View recruited heroes
        Restock,        // Generate items for the day
        Consultation,   // Equipping a hero (one at a time)
        DayEnd,         // Show results, update reputation
        GameOver        // Reputation <= 0
    }
}
