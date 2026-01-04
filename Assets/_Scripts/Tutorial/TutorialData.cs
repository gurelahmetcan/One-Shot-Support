using System.Collections.Generic;
using UnityEngine;
using OneShotSupport.ScriptableObjects;

namespace OneShotSupport.Tutorial
{
    /// <summary>
    /// ScriptableObject containing scripted tutorial data
    /// Defines the tutorial hero, monster, and starting items
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialData", menuName = "One-Shot Support/Tutorial Data")]
    public class TutorialData : ScriptableObject
    {
        [Header("Tutorial Hero")]
        [Tooltip("Pre-configured tutorial hero")]
        public HeroData tutorialHero;

        [Header("Tutorial Monster")]
        [Tooltip("Pre-configured tutorial monster")]
        public MonsterData tutorialMonster;

        [Header("Tutorial Items")]
        [Tooltip("6 starting items (3 matching weakness + 3 others)")]
        public ItemData[] tutorialItems = new ItemData[6];

        [Header("Tutorial Hint")]
        [Tooltip("Tutorial hint message for day start")]
        public string tutorialHint = "Today, heroes seeking magical assistance might visit your shop!";

        /// <summary>
        /// Validate tutorial data is properly configured
        /// </summary>
        public bool IsValid()
        {
            if (tutorialHero == null)
            {
                Debug.LogError("[TutorialData] Tutorial hero is not assigned!");
                return false;
            }

            if (tutorialMonster == null)
            {
                Debug.LogError("[TutorialData] Tutorial monster is not assigned!");
                return false;
            }

            if (tutorialItems == null || tutorialItems.Length != 6)
            {
                Debug.LogError("[TutorialData] Tutorial items array must have exactly 6 items!");
                return false;
            }

            for (int i = 0; i < tutorialItems.Length; i++)
            {
                if (tutorialItems[i] == null)
                {
                    Debug.LogError($"[TutorialData] Tutorial item at index {i} is null!");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get tutorial items as a list
        /// </summary>
        public List<ItemData> GetTutorialItems()
        {
            return new List<ItemData>(tutorialItems);
        }
    }
}
