using UnityEngine;

namespace OneShotSupport.Data
{
    /// <summary>
    /// Aptitudes determine how effectively a hero learns/grows in each stat (5-stat system)
    /// Higher aptitude = faster stat growth when training that focus
    /// </summary>
    [System.Serializable]
    public class HeroAptitudes
    {
        [Tooltip("Multiplier for Might growth (0.5 to 2.0)")]
        [Range(0.5f, 2.0f)]
        public float mightAptitude = 1.0f;

        [Tooltip("Multiplier for Charm growth (0.5 to 2.0)")]
        [Range(0.5f, 2.0f)]
        public float charmAptitude = 1.0f;

        [Tooltip("Multiplier for Wit growth (0.5 to 2.0)")]
        [Range(0.5f, 2.0f)]
        public float witAptitude = 1.0f;

        [Tooltip("Multiplier for Agility growth (0.5 to 2.0)")]
        [Range(0.5f, 2.0f)]
        public float agilityAptitude = 1.0f;

        [Tooltip("Multiplier for Fortitude growth (0.5 to 2.0)")]
        [Range(0.5f, 2.0f)]
        public float fortitudeAptitude = 1.0f;

        [Tooltip("Multiplier for Discipline training effectiveness (greed reduction)")]
        [Range(0.5f, 2.0f)]
        public float disciplineAptitude = 1.0f;

        /// <summary>
        /// Constructor for random aptitudes (5-stat system)
        /// </summary>
        public HeroAptitudes(float might, float charm, float wit, float agility, float fortitude, float discipline)
        {
            mightAptitude = Mathf.Clamp(might, 0.5f, 2.0f);
            charmAptitude = Mathf.Clamp(charm, 0.5f, 2.0f);
            witAptitude = Mathf.Clamp(wit, 0.5f, 2.0f);
            agilityAptitude = Mathf.Clamp(agility, 0.5f, 2.0f);
            fortitudeAptitude = Mathf.Clamp(fortitude, 0.5f, 2.0f);
            disciplineAptitude = Mathf.Clamp(discipline, 0.5f, 2.0f);
        }

        /// <summary>
        /// Default constructor (average aptitudes)
        /// </summary>
        public HeroAptitudes()
        {
            mightAptitude = 1.0f;
            charmAptitude = 1.0f;
            witAptitude = 1.0f;
            agilityAptitude = 1.0f;
            fortitudeAptitude = 1.0f;
            disciplineAptitude = 1.0f;
        }

        /// <summary>
        /// Get aptitude for a specific focus type
        /// </summary>
        public float GetAptitude(EducationFocus focus)
        {
            return focus switch
            {
                EducationFocus.Might => mightAptitude,
                EducationFocus.Charm => charmAptitude,
                EducationFocus.Wit => witAptitude,
                EducationFocus.Agility => agilityAptitude,
                EducationFocus.Fortitude => fortitudeAptitude,
                EducationFocus.Discipline => disciplineAptitude,
                _ => 1.0f
            };
        }
    }
}
