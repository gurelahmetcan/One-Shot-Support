using UnityEngine;
using System.Collections.Generic;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Result of a mission resolution
    /// </summary>
    public class MissionResolutionResult
    {
        public bool isSuccess;
        public float coveragePercentage; // 0-100
        public Vector2 ballLandingPosition; // Normalized position where the ball "landed"
        public int landingSectorIndex; // Which stat sector (0-4: Might, Charm, Wit, Agility, Fortitude)
        public string landingSectorName;
        public float landingDistance; // How far from center (0-1 normalized)
        public float sectorRequirement; // What was required at that sector
        public float sectorHeroStat; // What heroes provided at that sector

        // Per-stat coverage for detailed feedback
        public float[] statCoverage = new float[5]; // 0-1 for each stat
    }

    /// <summary>
    /// Handles mission success/failure resolution using pentagon-based mechanics
    /// Similar to Dispatch game - ball bounces and lands in coverage area or danger zone
    /// </summary>
    public static class MissionResolver
    {
        private static readonly string[] StatNames = { "Might", "Charm", "Wit", "Agility", "Fortitude" };

        /// <summary>
        /// Resolve a mission with assigned heroes
        /// </summary>
        /// <param name="mission">The mission to resolve</param>
        /// <param name="assignedHeroes">Heroes assigned to this mission</param>
        /// <returns>Resolution result with success/failure and details</returns>
        public static MissionResolutionResult ResolveMission(MissionData mission, List<HeroData> assignedHeroes)
        {
            var result = new MissionResolutionResult();

            // Calculate combined hero stats
            int[] heroStats = CalculateCombinedStats(assignedHeroes);
            int[] missionReqs = GetMissionRequirements(mission);

            // Calculate per-stat coverage (0-1 for each)
            for (int i = 0; i < 5; i++)
            {
                if (missionReqs[i] <= 0)
                {
                    result.statCoverage[i] = 1f; // No requirement = fully covered
                }
                else
                {
                    result.statCoverage[i] = Mathf.Clamp01((float)heroStats[i] / missionReqs[i]);
                }
            }

            // Calculate overall coverage percentage
            result.coveragePercentage = CalculateOverallCoverage(result.statCoverage, missionReqs) * 100f;

            // Check for guaranteed success (100% coverage)
            if (result.coveragePercentage >= 100f)
            {
                result.isSuccess = true;
                result.coveragePercentage = 100f;

                // Ball lands safely anywhere - pick random safe spot
                result.landingSectorIndex = Random.Range(0, 5);
                result.landingSectorName = StatNames[result.landingSectorIndex];
                result.landingDistance = Random.Range(0f, 1f);
                result.sectorRequirement = missionReqs[result.landingSectorIndex];
                result.sectorHeroStat = heroStats[result.landingSectorIndex];
                result.ballLandingPosition = CalculatePentagonPosition(result.landingSectorIndex, result.landingDistance);

                Debug.Log($"[MissionResolver] GUARANTEED SUCCESS! Coverage: {result.coveragePercentage:F1}%");
            }
            else
            {
                // Simulate ball landing - pick random position weighted by mission requirements
                SimulateBallLanding(result, heroStats, missionReqs);
            }

            // Log detailed result
            LogResolutionDetails(result, heroStats, missionReqs);

            return result;
        }

        /// <summary>
        /// Calculate combined stats from all assigned heroes
        /// </summary>
        private static int[] CalculateCombinedStats(List<HeroData> heroes)
        {
            int[] combined = new int[5];

            foreach (var hero in heroes)
            {
                if (hero != null)
                {
                    combined[0] += hero.might;
                    combined[1] += hero.charm;
                    combined[2] += hero.wit;
                    combined[3] += hero.agility;
                    combined[4] += hero.fortitude;
                }
            }

            return combined;
        }

        /// <summary>
        /// Get mission requirements as array
        /// </summary>
        private static int[] GetMissionRequirements(MissionData mission)
        {
            return new int[]
            {
                mission.mightRequirement,
                mission.charmRequirement,
                mission.witRequirement,
                mission.agilityRequirement,
                mission.fortitudeRequirement
            };
        }

        /// <summary>
        /// Calculate overall coverage weighted by requirements
        /// </summary>
        private static float CalculateOverallCoverage(float[] statCoverage, int[] missionReqs)
        {
            float totalWeight = 0f;
            float weightedCoverage = 0f;

            for (int i = 0; i < 5; i++)
            {
                float weight = missionReqs[i];
                totalWeight += weight;
                weightedCoverage += statCoverage[i] * weight;
            }

            if (totalWeight <= 0) return 1f;

            return weightedCoverage / totalWeight;
        }

        /// <summary>
        /// Simulate the ball landing to determine success/failure
        /// The ball lands at a random position within the mission pentagon
        /// If that position is covered by hero stats, it's a success
        /// </summary>
        private static void SimulateBallLanding(MissionResolutionResult result, int[] heroStats, int[] missionReqs)
        {
            // Pick a random sector (weighted by mission requirements - higher req = larger sector)
            int totalReq = 0;
            for (int i = 0; i < 5; i++) totalReq += missionReqs[i];

            int randomValue = Random.Range(0, totalReq);
            int sectorIndex = 0;
            int accumulated = 0;

            for (int i = 0; i < 5; i++)
            {
                accumulated += missionReqs[i];
                if (randomValue < accumulated)
                {
                    sectorIndex = i;
                    break;
                }
            }

            result.landingSectorIndex = sectorIndex;
            result.landingSectorName = StatNames[sectorIndex];
            result.sectorRequirement = missionReqs[sectorIndex];
            result.sectorHeroStat = heroStats[sectorIndex];

            // Pick a random distance within that sector (0 to requirement)
            // The ball can land anywhere from center to the edge of the mission requirement
            float maxDistance = missionReqs[sectorIndex];
            if (maxDistance <= 0)
            {
                // No requirement in this sector - guaranteed success here
                result.isSuccess = true;
                result.landingDistance = 0f;
            }
            else
            {
                // Random distance from center to edge
                float landingDistance = Random.Range(0f, maxDistance);
                result.landingDistance = landingDistance / maxDistance; // Normalize to 0-1

                // Check if heroes cover this distance
                // Success if hero stat >= landing distance
                result.isSuccess = heroStats[sectorIndex] >= landingDistance;
            }

            result.ballLandingPosition = CalculatePentagonPosition(sectorIndex, result.landingDistance);
        }

        /// <summary>
        /// Calculate a position on the pentagon given sector and distance
        /// </summary>
        private static Vector2 CalculatePentagonPosition(int sectorIndex, float normalizedDistance)
        {
            // Pentagon angles (starting from top, going clockwise)
            // Might (top), Charm (top-right), Wit (bottom-right), Agility (bottom-left), Fortitude (top-left)
            float[] angles = { 90f, 18f, -54f, -126f, -198f };

            float angle = angles[sectorIndex] * Mathf.Deg2Rad;
            float distance = normalizedDistance;

            return new Vector2(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance
            );
        }

        /// <summary>
        /// Log detailed resolution information
        /// </summary>
        private static void LogResolutionDetails(MissionResolutionResult result, int[] heroStats, int[] missionReqs)
        {
            string successStr = result.isSuccess ? "SUCCESS" : "FAILURE";

            Debug.Log($"[MissionResolver] === MISSION {successStr} ===");
            Debug.Log($"[MissionResolver] Overall Coverage: {result.coveragePercentage:F1}%");
            Debug.Log($"[MissionResolver] Ball landed in {result.landingSectorName} sector at {result.landingDistance * 100:F0}% distance");
            Debug.Log($"[MissionResolver] Sector requirement: {result.sectorRequirement}, Heroes provided: {result.sectorHeroStat}");

            // Per-stat breakdown
            for (int i = 0; i < 5; i++)
            {
                string covered = heroStats[i] >= missionReqs[i] ? "✓" : "✗";
                Debug.Log($"[MissionResolver]   {StatNames[i]}: {heroStats[i]}/{missionReqs[i]} ({result.statCoverage[i] * 100:F0}%) {covered}");
            }
        }

        /// <summary>
        /// Calculate success probability without actually resolving
        /// Useful for showing the player their chances before dispatch
        /// </summary>
        public static float CalculateSuccessProbability(MissionData mission, List<HeroData> assignedHeroes)
        {
            int[] heroStats = CalculateCombinedStats(assignedHeroes);
            int[] missionReqs = GetMissionRequirements(mission);

            float[] statCoverage = new float[5];
            for (int i = 0; i < 5; i++)
            {
                if (missionReqs[i] <= 0)
                {
                    statCoverage[i] = 1f;
                }
                else
                {
                    statCoverage[i] = Mathf.Clamp01((float)heroStats[i] / missionReqs[i]);
                }
            }

            return CalculateOverallCoverage(statCoverage, missionReqs) * 100f;
        }
    }
}
