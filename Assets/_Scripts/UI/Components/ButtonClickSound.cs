using UnityEngine;
using UnityEngine.UI;
using OneShotSupport.Core;

namespace OneShotSupport.UI.Components
{
    /// <summary>
    /// Component to play a sound when a button is clicked
    /// Attach to any Button GameObject to add click sound
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonClickSound : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                // Add click sound listener to the button
                button.onClick.AddListener(PlayClickSound);
            }
        }

        private void OnDestroy()
        {
            // Clean up listener
            if (button != null)
            {
                button.onClick.RemoveListener(PlayClickSound);
            }
        }

        /// <summary>
        /// Play button click sound
        /// </summary>
        private void PlayClickSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClickSound();
            }
        }
    }
}
