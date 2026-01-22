using UnityEngine;

namespace OneShotSupport.Data
{
    /// <summary>
    /// Aptitudes determine how effectively a hero learns/grows in each stat
    /// Higher aptitude = faster stat growth when training that focus
    /// </summary>
    [System.Serializable]
    public class HeroAptitudes
    {
        [Tooltip("Multiplier for Prowess growth (0.5 to 2.0)")]
        [Range(0.5f, 2.0f)]
        public float prowessAptitude = 1.0f;

        [Tooltip("Multiplier for Charisma growth (0.5 to 2.0)")]
        [Range(0.5f, 2.0f)]
        public float charismaAptitude = 1.0f;

        [Tooltip("Multiplier for Vitality growth (0.5 to 2.0)")]
        [Range(0.5f, 2.0f)]
        public float vitalityAptitude = 1.0f;

        [Tooltip("Multiplier for Discipline growth (0.5 to 2.0)")]
        [Range(0.5f, 2.0f)]
        public float disciplineAptitude = 1.0f;

        /// <summary>
        /// Constructor for random aptitudes
        /// </summary>
        public HeroAptitudes(float prowess, float charisma, float vitality, float discipline)
        {
            prowessAptitude = Mathf.Clamp(prowess, 0.5f, 2.0f);
            charismaAptitude = Mathf.Clamp(charisma, 0.5f, 2.0f);
            vitalityAptitude = Mathf.Clamp(vitality, 0.5f, 2.0f);
            disciplineAptitude = Mathf.Clamp(discipline, 0.5f, 2.0f);
        }

        /// <summary>
        /// Default constructor (average aptitudes)
        /// </summary>
        public HeroAptitudes()
        {
            prowessAptitude = 1.0f;
            charismaAptitude = 1.0f;
            vitalityAptitude = 1.0f;
            disciplineAptitude = 1.0f;
        }

        /// <summary>
        /// Get aptitude for a specific focus type
        /// </summary>
        public float GetAptitude(EducationFocus focus)
        {
            return focus switch
            {
                EducationFocus.Prowess => prowessAptitude,
                EducationFocus.Charisma => charismaAptitude,
                EducationFocus.Vitality => vitalityAptitude,
                EducationFocus.Discipline => disciplineAptitude,
                _ => 1.0f
            };
        }
    }
}
