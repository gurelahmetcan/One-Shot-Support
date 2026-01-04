using UnityEngine;
using OneShotSupport.Data;

namespace OneShotSupport.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject representing monsters/bosses that heroes fight
    /// </summary>
    [CreateAssetMenu(fileName = "New Monster", menuName = "One-Shot Support/Monster")]
    public class MonsterData : ScriptableObject
    {
        [Header("Monster Identity")]
        [Tooltip("Display name of the monster")]
        public string monsterName;

        [Tooltip("Monster description for UI")]
        [TextArea(2, 4)]
        public string description;

        [Header("Stats")]
        [Tooltip("Monster's weakness - items matching this get bonus")]
        public ItemCategory weakness;

        [Tooltip("Monster rank determining difficulty and gold reward")]
        public MonsterRank rank = MonsterRank.D;

        [Tooltip("Difficulty penalty subtracted from hero's success chance")]
        [Range(0, 30)]
        public int difficultyPenalty = 15;

        [Header("Visual")]
        [Tooltip("Monster sprite for UI display")]
        public Sprite sprite;
        [Tooltip("Category sprite for UI display")]
        public Sprite categorySprite;
    }
}
