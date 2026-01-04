using UnityEngine;
using OneShotSupport.Data;

namespace OneShotSupport.Core
{
    /// <summary>
    /// Manages daily hints that suggest monster weaknesses to players
    /// 80% chance of hint, 20% chance of normal day
    /// </summary>
    [CreateAssetMenu(fileName = "HintSystem", menuName = "One-Shot Support/Hint System")]
    public class HintSystem : ScriptableObject
    {
        [Header("Hint Probability")]
        [Range(0f, 1f)]
        [Tooltip("Chance of getting a hint (0.8 = 80%)")]
        public float hintChance = 0.8f;

        [Header("Hygiene Hints")]
        [Tooltip("Hint messages for Hygiene weakness")]
        public string[] hygieneHints = new string[]
        {
            "Today the weather smells a bit bad...",
            "The air feels unclean today.",
            "A strange odor lingers in the atmosphere.",
            "Everything seems a bit grimy today.",
            "The streets are dirtier than usual."
        };

        [Header("Magic Hints")]
        [Tooltip("Hint messages for Magic weakness")]
        public string[] magicHints = new string[]
        {
            "Strange arcane energies fill the air...",
            "The stars align in mysterious ways today.",
            "Magical disturbances are detected.",
            "Ancient runes glow faintly in the distance.",
            "The fabric of reality feels thin today."
        };

        [Header("Catering Hints")]
        [Tooltip("Hint messages for Catering weakness")]
        public string[] cateringHints = new string[]
        {
            "Everyone seems extra hungry today...",
            "The scent of fresh bread fills the air.",
            "Stomachs are rumbling more than usual.",
            "Food supplies are running low in town.",
            "The tavern is busier than normal."
        };

        [Header("Lighting Hints")]
        [Tooltip("Hint messages for Lighting weakness")]
        public string[] lightingHints = new string[]
        {
            "The day seems darker than usual...",
            "Shadows are deeper today.",
            "Storm clouds gather on the horizon.",
            "The sun is obscured by dark clouds.",
            "Night seems to arrive early today."
        };

        [Header("Normal Day Message")]
        [Tooltip("Message when there's no hint")]
        public string normalDayMessage = "Today everything feels normal.";

        /// <summary>
        /// Generate a daily hint
        /// Returns the hinted weakness category, or null for normal day
        /// </summary>
        public DailyHint GenerateHint()
        {
            // 80% chance of hint, 20% normal day
            if (Random.value > hintChance)
            {
                return new DailyHint
                {
                    hasHint = false,
                    hintedWeakness = null,
                    hintMessage = normalDayMessage
                };
            }

            // Pick random category
            ItemCategory randomCategory = (ItemCategory)Random.Range(0, 4);

            // Get random hint message for that category
            string message = GetRandomHintMessage(randomCategory);

            return new DailyHint
            {
                hasHint = true,
                hintedWeakness = randomCategory,
                hintMessage = message
            };
        }

        /// <summary>
        /// Get random hint message for a category
        /// </summary>
        private string GetRandomHintMessage(ItemCategory category)
        {
            string[] hints = category switch
            {
                ItemCategory.Hygiene => hygieneHints,
                ItemCategory.Magic => magicHints,
                ItemCategory.Catering => cateringHints,
                ItemCategory.Lighting => lightingHints,
                _ => new string[] { normalDayMessage }
            };

            if (hints == null || hints.Length == 0)
            {
                Debug.LogWarning($"[HintSystem] No hints configured for {category}");
                return normalDayMessage;
            }

            return hints[Random.Range(0, hints.Length)];
        }
    }

    /// <summary>
    /// Data structure for daily hint information
    /// </summary>
    [System.Serializable]
    public class DailyHint
    {
        public bool hasHint;
        public ItemCategory? hintedWeakness;
        public string hintMessage;
    }
}
