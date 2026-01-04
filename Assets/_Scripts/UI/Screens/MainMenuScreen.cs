using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Main menu screen with game start, how to play, and quit options
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        [Header("UI Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private Button quitButton;

        [Header("Scene Settings")]
        [SerializeField] private string gameSceneName = "GameScene";

        private void Awake()
        {
            // Setup button listeners
            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGameClicked);

            if (howToPlayButton != null)
                howToPlayButton.onClick.AddListener(OnHowToPlayClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        private void OnNewGameClicked()
        {
            Debug.Log("[MainMenuScreen] Starting new game...");
            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>
        /// Show how to play screen
        /// </summary>
        private void OnHowToPlayClicked()
        {
            // TODO: Implement how to play screen
            Debug.Log("[MainMenuScreen] How to Play button clicked - not yet implemented");
        }

        /// <summary>
        /// Quit the game
        /// </summary>
        private void OnQuitClicked()
        {
            Debug.Log("[MainMenuScreen] Quitting game...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
