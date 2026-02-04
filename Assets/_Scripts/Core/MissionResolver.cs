using UnityEngine;
using System.Collections.Generic;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.Core
{
    public class MissionResolutionResult
    {
        public bool isSuccess;
        public float coveragePercentage;
        public float[] statCoverage = new float[5];
    }

    public static class MissionResolver
    {
        /// <summary>
        /// HELPER: Sums stats from all assigned heroes.
        /// Used by UI to draw the "Win Zone".
        /// </summary>
        public static int[] CalculateCombinedStats(List<HeroData> heroes)
        {
            int[] combined = new int[5]; // Might, Charm, Wit, Agility, Fortitude

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
        /// Packages the final data AFTER the animation finishes.
        /// </summary>
        public static MissionResolutionResult CreateResultFromPhysics(MissionData mission, List<HeroData> assignedHeroes, bool isSuccess)
        {
            var result = new MissionResolutionResult();
            result.isSuccess = isSuccess;

            // Calculate display stats
            int[] heroStats = CalculateCombinedStats(assignedHeroes);
            int[] missionReqs = { 
                mission.mightRequirement, mission.charmRequirement, 
                mission.witRequirement, mission.agilityRequirement, mission.fortitudeRequirement 
            };

            float totalReq = 0;
            float totalHave = 0;

            for (int i = 0; i < 5; i++)
            {
                if (missionReqs[i] <= 0)
                    result.statCoverage[i] = 1f;
                else
                    result.statCoverage[i] = Mathf.Clamp01((float)heroStats[i] / missionReqs[i]);
                
                totalReq += missionReqs[i];
                totalHave += heroStats[i];
            }

            if (totalReq > 0)
                result.coveragePercentage = (totalHave / totalReq) * 100f;
            else
                result.coveragePercentage = 100f;

            return result;
        }
    }
}