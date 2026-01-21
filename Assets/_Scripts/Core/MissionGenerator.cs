using System.Collections.Generic;
using UnityEngine;
using OneShotSupport.Data;
using OneShotSupport.ScriptableObjects;
using Random = UnityEngine.Random;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Pairs mission names with their icons
    /// </summary>
    [System.Serializable]
    public class MissionVisuals
    {
        [Tooltip("Possible names for this mission type (one will be randomly selected)")]
        public string[] missionNames;

        [Tooltip("Mission icon/sprite")]
        public Sprite sprite;
    }

    /// <summary>
    /// Configuration for procedural mission generation
    /// </summary>
    [CreateAssetMenu(fileName = "MissionGenerator", menuName = "One-Shot Support/Mission Generator")]
    public class MissionGenerator : ScriptableObject
    {
        [Header("Mission Visuals")]
        [Tooltip("Visual pool for missions (sprite + name pairs, randomly selected)")]
        public MissionVisuals[] missionVisuals;

        [Header("Danger Level Distribution")]
        [Range(0f, 1f)]
        [Tooltip("Chance for 1-star mission")]
        public float oneStarChance = 0.3f;

        [Range(0f, 1f)]
        [Tooltip("Chance for 2-star mission")]
        public float twoStarChance = 0.3f;

        [Range(0f, 1f)]
        [Tooltip("Chance for 3-star mission")]
        public float threeStarChance = 0.25f;

        [Range(0f, 1f)]
        [Tooltip("Chance for 4-star mission")]
        public float fourStarChance = 0.1f;

        [Range(0f, 1f)]
        [Tooltip("Chance for 5-star mission")]
        public float fiveStarChance = 0.05f;

        [Header("Reward Ranges per Danger Level")]
        [Tooltip("Gold reward range for 1-star missions")]
        public Vector2Int oneStarGoldRange = new Vector2Int(20, 50);

        [Tooltip("Gold reward range for 2-star missions")]
        public Vector2Int twoStarGoldRange = new Vector2Int(40, 80);

        [Tooltip("Gold reward range for 3-star missions")]
        public Vector2Int threeStarGoldRange = new Vector2Int(70, 120);

        [Tooltip("Gold reward range for 4-star missions")]
        public Vector2Int fourStarGoldRange = new Vector2Int(100, 180);

        [Tooltip("Gold reward range for 5-star missions")]
        public Vector2Int fiveStarGoldRange = new Vector2Int(150, 250);

        [Header("Threat Level per Danger")]
        [Tooltip("Threat level for 1-star missions")]
        public int oneStarThreat = 5;

        [Tooltip("Threat level for 2-star missions")]
        public int twoStarThreat = 15;

        [Tooltip("Threat level for 3-star missions")]
        public int threeStarThreat = 25;

        [Tooltip("Threat level for 4-star missions")]
        public int fourStarThreat = 35;

        [Tooltip("Threat level for 5-star missions")]
        public int fiveStarThreat = 45;

        [Header("Intel Settings")]
        [Range(0f, 1f)]
        [Tooltip("Chance for mission to have intel hint")]
        public float intelChance = 0.5f;

        [Tooltip("Intel hint templates")]
        public string[] intelHints = new string[]
        {
            "Sharp weapons recommended",
            "Bring magical items",
            "Catering supplies needed",
            "Lighting equipment required"
        };

        /// <summary>
        /// Generate a list of random missions
        /// </summary>
        public List<MissionData> GenerateMissions(int count)
        {
            var missions = new List<MissionData>();
            for (int i = 0; i < count; i++)
            {
                missions.Add(GenerateMission());
            }
            return missions;
        }

        /// <summary>
        /// Generate a single random mission
        /// </summary>
        public MissionData GenerateMission()
        {
            // Create runtime instance
            var mission = ScriptableObject.CreateInstance<MissionData>();

            // Assign danger level
            mission.dangerLevel = GetRandomDangerLevel();

            // Assign visuals (sprite + name)
            AssignVisuals(mission);

            // Assign description
            mission.description = GenerateDescription(mission.dangerLevel);

            // Assign rewards based on danger level
            AssignRewards(mission);

            // Assign threat level based on danger
            mission.threatLevel = GetThreatLevel(mission.dangerLevel);

            // Random intel hint
            if (Random.value < intelChance)
            {
                mission.recommendedCategory = (ItemCategory)Random.Range(0, 4);
                mission.intelHint = intelHints[(int)mission.recommendedCategory];
            }
            else
            {
                mission.intelHint = "";
            }

            return mission;
        }

        /// <summary>
        /// Get a random danger level based on distribution
        /// </summary>
        private MissionDanger GetRandomDangerLevel()
        {
            float roll = Random.value;
            float cumulative = 0f;

            cumulative += oneStarChance;
            if (roll < cumulative) return MissionDanger.OneStar;

            cumulative += twoStarChance;
            if (roll < cumulative) return MissionDanger.TwoStar;

            cumulative += threeStarChance;
            if (roll < cumulative) return MissionDanger.ThreeStar;

            cumulative += fourStarChance;
            if (roll < cumulative) return MissionDanger.FourStar;

            return MissionDanger.FiveStar;
        }

        /// <summary>
        /// Assign visuals (name + sprite) to mission
        /// </summary>
        private void AssignVisuals(MissionData mission)
        {
            if (missionVisuals == null || missionVisuals.Length == 0)
            {
                mission.missionName = "Unknown Mission";
                return;
            }

            // Pick random visual set
            var visualSet = missionVisuals[Random.Range(0, missionVisuals.Length)];

            // Pick random name from the set
            if (visualSet.missionNames != null && visualSet.missionNames.Length > 0)
            {
                mission.missionName = visualSet.missionNames[Random.Range(0, visualSet.missionNames.Length)];
            }
            else
            {
                mission.missionName = "Unknown Mission";
            }

            mission.missionSprite = visualSet.sprite;
        }

        /// <summary>
        /// Generate a description based on danger level
        /// </summary>
        private string GenerateDescription(MissionDanger danger)
        {
            return danger switch
            {
                MissionDanger.OneStar => "A simple task, suitable for rookies.",
                MissionDanger.TwoStar => "Moderate danger. Proceed with caution.",
                MissionDanger.ThreeStar => "Dangerous mission. Experienced heroes recommended.",
                MissionDanger.FourStar => "High risk mission. Only for skilled veterans.",
                MissionDanger.FiveStar => "Extremely dangerous. Survival not guaranteed.",
                _ => "Unknown danger level."
            };
        }

        /// <summary>
        /// Assign rewards based on danger level
        /// </summary>
        private void AssignRewards(MissionData mission)
        {
            Vector2Int goldRange = mission.dangerLevel switch
            {
                MissionDanger.OneStar => oneStarGoldRange,
                MissionDanger.TwoStar => twoStarGoldRange,
                MissionDanger.ThreeStar => threeStarGoldRange,
                MissionDanger.FourStar => fourStarGoldRange,
                MissionDanger.FiveStar => fiveStarGoldRange,
                _ => oneStarGoldRange
            };

            mission.goldReward = Random.Range(goldRange.x, goldRange.y + 1);
            mission.materialsReward = mission.goldReward / 2; // Materials = half of gold
        }

        /// <summary>
        /// Get threat level based on danger
        /// </summary>
        private int GetThreatLevel(MissionDanger danger)
        {
            return danger switch
            {
                MissionDanger.OneStar => oneStarThreat,
                MissionDanger.TwoStar => twoStarThreat,
                MissionDanger.ThreeStar => threeStarThreat,
                MissionDanger.FourStar => fourStarThreat,
                MissionDanger.FiveStar => fiveStarThreat,
                _ => oneStarThreat
            };
        }
    }
}
