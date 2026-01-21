using UnityEngine;
using OneShotSupport.Data;

namespace OneShotSupport.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject representing a mission/quest that heroes can undertake
    /// </summary>
    [CreateAssetMenu(fileName = "New Mission", menuName = "One-Shot Support/Mission")]
    public class MissionData : ScriptableObject
    {
        [Header("Mission Identity")]
        [Tooltip("Display name of the mission")]
        public string missionName;

        [Tooltip("Mission description for UI")]
        [TextArea(2, 4)]
        public string description;

        [Header("Difficulty")]
        [Tooltip("Mission danger level (1-5 stars)")]
        public MissionDanger dangerLevel = MissionDanger.OneStar;

        [Tooltip("Threat level for combat calculation (affects success chance)")]
        [Range(0, 50)]
        public int threatLevel = 15;

        [Header("Rewards")]
        [Tooltip("Gold reward for completing this mission")]
        [Range(10, 500)]
        public int goldReward = 50;

        [Tooltip("Materials reward for completing this mission")]
        [Range(5, 100)]
        public int materialsReward = 20;

        [Header("Intel (Optional)")]
        [Tooltip("Intel hint about this mission (e.g., 'Sharp weapons needed')")]
        public string intelHint = "";

        [Tooltip("Recommended item category for this mission")]
        public ItemCategory recommendedCategory;

        [Header("Visual")]
        [Tooltip("Mission icon/sprite for UI display")]
        public Sprite missionSprite;
    }
}
