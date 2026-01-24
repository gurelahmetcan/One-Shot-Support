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

        [Header("Stat Budget per Danger Level")]
        [Tooltip("Total stat budget for 1-star missions")]
        public Vector2Int oneStarBudget = new Vector2Int(60, 80);

        [Tooltip("Total stat budget for 2-star missions")]
        public Vector2Int twoStarBudget = new Vector2Int(80, 120);

        [Tooltip("Total stat budget for 3-star missions")]
        public Vector2Int threeStarBudget = new Vector2Int(120, 160);

        [Tooltip("Total stat budget for 4-star missions")]
        public Vector2Int fourStarBudget = new Vector2Int(160, 200);

        [Tooltip("Total stat budget for 5-star missions")]
        public Vector2Int fiveStarBudget = new Vector2Int(200, 250);

        // Mission name pools per archetype (with stat hints)
        private readonly Dictionary<MissionArchetype, string[]> missionNamePools = new Dictionary<MissionArchetype, string[]>()
        {
            { MissionArchetype.Combat, new string[] {
                "Slay the Dragon (Might)",
                "Defend the Village (Might)",
                "Clear the Dungeon (Might)",
                "Defeat the Warlord (Might)",
                "Hunt the Beast (Might)"
            }},
            { MissionArchetype.Stealth, new string[] {
                "Infiltrate the Castle (Agility)",
                "Steal the Relic (Agility)",
                "Shadow Assassination (Agility)",
                "Bypass the Guards (Agility)",
                "Secret Passage (Agility)"
            }},
            { MissionArchetype.Diplomatic, new string[] {
                "Negotiate Peace (Charm)",
                "Convince the Council (Charm)",
                "Royal Audience (Charm)",
                "Merchant Bargain (Charm)",
                "Alliance Proposal (Charm)"
            }},
            { MissionArchetype.Investigation, new string[] {
                "Solve the Mystery (Wit)",
                "Find the Culprit (Wit)",
                "Decode the Message (Wit)",
                "Track the Clues (Wit)",
                "Uncover the Plot (Wit)"
            }},
            { MissionArchetype.Survival, new string[] {
                "Cross the Desert (Fortitude)",
                "Survive the Blizzard (Fortitude)",
                "Endure the Trial (Fortitude)",
                "Mountain Expedition (Fortitude)",
                "Wasteland Trek (Fortitude)"
            }},
            { MissionArchetype.Balanced, new string[] {
                "General Quest",
                "Mixed Objectives",
                "Standard Mission",
                "Varied Challenges",
                "All-Purpose Task"
            }}
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
        /// Generate a single random mission with stat requirements (5-stat system)
        /// </summary>
        public MissionData GenerateMission()
        {
            // Create runtime instance
            var mission = ScriptableObject.CreateInstance<MissionData>();

            // Assign danger level
            mission.dangerLevel = GetRandomDangerLevel();

            // Assign random archetype
            mission.archetype = GetRandomArchetype();

            // Assign mission name based on archetype (with stat hint)
            mission.missionName = GetMissionName(mission.archetype);

            // Assign description
            mission.description = GenerateDescription(mission.dangerLevel);

            // Assign stat requirements based on archetype and danger
            AssignStatRequirements(mission);

            // Assign max hero count based on danger
            mission.maxHeroCount = GetMaxHeroCount(mission.dangerLevel);

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

            Debug.Log($"[MissionGen] {mission.missionName} ({mission.dangerLevel}, {mission.archetype}) | " +
                      $"M:{mission.mightRequirement} C:{mission.charmRequirement} W:{mission.witRequirement} " +
                      $"A:{mission.agilityRequirement} F:{mission.fortitudeRequirement} | Heroes: {mission.maxHeroCount}");

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

        /// <summary>
        /// Get a random archetype
        /// </summary>
        private MissionArchetype GetRandomArchetype()
        {
            return (MissionArchetype)Random.Range(0, 6); // 0-5 = 6 archetype types
        }

        /// <summary>
        /// Get mission name from archetype pool
        /// </summary>
        private string GetMissionName(MissionArchetype archetype)
        {
            if (missionNamePools.TryGetValue(archetype, out string[] names))
            {
                return names[Random.Range(0, names.Length)];
            }
            return "Unknown Mission";
        }

        /// <summary>
        /// Assign stat requirements based on archetype and danger level
        /// All requirements are multiples of 5
        /// </summary>
        private void AssignStatRequirements(MissionData mission)
        {
            // Get stat budget for this danger level
            Vector2Int budgetRange = mission.dangerLevel switch
            {
                MissionDanger.OneStar => oneStarBudget,
                MissionDanger.TwoStar => twoStarBudget,
                MissionDanger.ThreeStar => threeStarBudget,
                MissionDanger.FourStar => fourStarBudget,
                MissionDanger.FiveStar => fiveStarBudget,
                _ => oneStarBudget
            };

            int totalBudget = Random.Range(budgetRange.x, budgetRange.y + 1);

            // Get stat profile for archetype
            MissionStatProfile profile = MissionStatProfile.GetProfile(mission.archetype);

            // Calculate requirements (automatically rounds to multiples of 5)
            profile.CalculateRequirements(
                totalBudget,
                out int might,
                out int charm,
                out int wit,
                out int agility,
                out int fortitude
            );

            // Assign to mission
            mission.mightRequirement = might;
            mission.charmRequirement = charm;
            mission.witRequirement = wit;
            mission.agilityRequirement = agility;
            mission.fortitudeRequirement = fortitude;
        }

        /// <summary>
        /// Get max hero count based on danger level
        /// Higher danger = more heroes allowed
        /// </summary>
        private int GetMaxHeroCount(MissionDanger danger)
        {
            return danger switch
            {
                MissionDanger.OneStar => 1,      // Solo mission
                MissionDanger.TwoStar => Random.Range(1, 3) == 1 ? 1 : 2,  // 1-2 heroes
                MissionDanger.ThreeStar => 2,    // Duo mission
                MissionDanger.FourStar => Random.Range(2, 4) == 2 ? 2 : 3, // 2-3 heroes
                MissionDanger.FiveStar => 3,     // Full party
                _ => 1
            };
        }
    }
}
