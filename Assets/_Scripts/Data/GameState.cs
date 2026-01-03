namespace OneShotSupport.Data
{
    /// <summary>
    /// Game state machine states
    /// </summary>
    public enum GameState
    {
        DayStart,       // Initialize new day
        Restock,        // Generate items for the day
        Consultation,   // Equipping a hero (one at a time)
        DayEnd,         // Show results, update reputation
        GameOver        // Reputation <= 0
    }
}
