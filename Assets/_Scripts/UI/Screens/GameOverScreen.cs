using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace OneShotSupport.UI.Screens
{
    /// <summary>
    /// Game over screen displayed when reputation reaches 0
    /// </summary>
    public class GameOverScreen : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI gameOverText;
        public TextMeshProUGUI daysSurvivedText;
        public TextMeshProUGUI finalReputationText;
        public Button restartButton;
        public Button quitButton;

        [Header("Text Settings")]
        public string gameOverMessage = "GAME OVER";
        public string daysSurvivedFormat = "You survived {0} days";

        private void Awake()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }

        /// <summary>
        /// Display game over screen with stats
        /// </summary>
        public void DisplayGameOver(int daysSurvived, int finalReputation)
        {
            if (gameOverText != null)
                gameOverText.text = gameOverMessage;

            if (daysSurvivedText != null)
                daysSurvivedText.text = string.Format(daysSurvivedFormat, daysSurvived);

            if (finalReputationText != null)
                finalReputationText.text = $"Final Reputation: {finalReputation}";
        }

        /// <summary>
        /// Restart the game
        /// </summary>
        private void OnRestartClicked()
        {
            // Reload current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Quit the game
        /// </summary>
        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
