using System.Collections.Generic;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.Data
{
    /// <summary>
    /// Result data for a single hero's consultation and battle
    /// </summary>
    [System.Serializable]
    public class HeroResult
    {
        public HeroData hero;
        public MonsterData monster;
        public List<ItemData> equippedItems;
        public int successChance;
        public ConfidenceLevel confidence;
        public bool succeeded;
        public int stars;
        public int reputationChange;

        public HeroResult()
        {
            equippedItems = new List<ItemData>();
        }
    }
}
