using UnityEngine;

namespace OneShotSupport.Data
{
    /// <summary>
    /// Defines stat distribution percentages for mission archetypes
    /// Used to intelligently generate mission stat requirements
    /// </summary>
    [System.Serializable]
    public class MissionStatProfile
    {
        public float mightPercentage;
        public float charmPercentage;
        public float witPercentage;
        public float agilityPercentage;
        public float fortitudePercentage;

        public MissionStatProfile(float might, float charm, float wit, float agility, float fortitude)
        {
            mightPercentage = might;
            charmPercentage = charm;
            witPercentage = wit;
            agilityPercentage = agility;
            fortitudePercentage = fortitude;
        }

        /// <summary>
        /// Get stat profile for a specific archetype
        /// </summary>
        public static MissionStatProfile GetProfile(MissionArchetype archetype)
        {
            return archetype switch
            {
                MissionArchetype.Combat => new MissionStatProfile(
                    might: 0.40f,       // 40% - Primary
                    fortitude: 0.30f,   // 30% - Secondary
                    agility: 0.15f,     // 15% - Tertiary
                    wit: 0.10f,         // 10% - Minor
                    charm: 0.05f        // 5% - Minimal
                ),

                MissionArchetype.Stealth => new MissionStatProfile(
                    agility: 0.40f,     // 40% - Primary
                    wit: 0.30f,         // 30% - Secondary
                    charm: 0.15f,       // 15% - Tertiary
                    fortitude: 0.10f,   // 10% - Minor
                    might: 0.05f        // 5% - Minimal
                ),

                MissionArchetype.Diplomatic => new MissionStatProfile(
                    charm: 0.40f,       // 40% - Primary
                    wit: 0.30f,         // 30% - Secondary
                    might: 0.10f,       // 10% - Tertiary
                    fortitude: 0.10f,   // 10% - Minor
                    agility: 0.10f      // 10% - Minor
                ),

                MissionArchetype.Investigation => new MissionStatProfile(
                    wit: 0.40f,         // 40% - Primary
                    charm: 0.25f,       // 25% - Secondary
                    agility: 0.20f,     // 20% - Tertiary
                    fortitude: 0.10f,   // 10% - Minor
                    might: 0.05f        // 5% - Minimal
                ),

                MissionArchetype.Survival => new MissionStatProfile(
                    fortitude: 0.40f,   // 40% - Primary
                    agility: 0.25f,     // 25% - Secondary
                    wit: 0.20f,         // 20% - Tertiary
                    might: 0.10f,       // 10% - Minor
                    charm: 0.05f        // 5% - Minimal
                ),

                MissionArchetype.Balanced => new MissionStatProfile(
                    might: 0.20f,       // 20% - Equal
                    charm: 0.20f,       // 20% - Equal
                    wit: 0.20f,         // 20% - Equal
                    agility: 0.20f,     // 20% - Equal
                    fortitude: 0.20f    // 20% - Equal
                ),

                _ => new MissionStatProfile(0.20f, 0.20f, 0.20f, 0.20f, 0.20f)
            };
        }

        /// <summary>
        /// Calculate stat requirements based on total budget and round to multiples of 5
        /// </summary>
        public void CalculateRequirements(int totalBudget, out int might, out int charm, out int wit, out int agility, out int fortitude)
        {
            // Calculate raw values
            float mightRaw = totalBudget * mightPercentage;
            float charmRaw = totalBudget * charmPercentage;
            float witRaw = totalBudget * witPercentage;
            float agilityRaw = totalBudget * agilityPercentage;
            float fortitudeRaw = totalBudget * fortitudePercentage;

            // Round to nearest multiple of 5
            might = RoundToMultipleOf5(mightRaw);
            charm = RoundToMultipleOf5(charmRaw);
            wit = RoundToMultipleOf5(witRaw);
            agility = RoundToMultipleOf5(agilityRaw);
            fortitude = RoundToMultipleOf5(fortitudeRaw);

            // Clamp to valid range (0-60)
            might = Mathf.Clamp(might, 0, 60);
            charm = Mathf.Clamp(charm, 0, 60);
            wit = Mathf.Clamp(wit, 0, 60);
            agility = Mathf.Clamp(agility, 0, 60);
            fortitude = Mathf.Clamp(fortitude, 0, 60);
        }

        /// <summary>
        /// Round a value to the nearest multiple of 5
        /// </summary>
        private static int RoundToMultipleOf5(float value)
        {
            return Mathf.RoundToInt(value / 5f) * 5;
        }

        /// <summary>
        /// Get total stat requirement
        /// </summary>
        public static int GetTotalRequirement(int might, int charm, int wit, int agility, int fortitude)
        {
            return might + charm + wit + agility + fortitude;
        }
    }
}
