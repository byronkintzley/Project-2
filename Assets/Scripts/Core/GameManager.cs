using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project2
{
    /// <summary>
    /// Singleton that survives scene loads. Owns the level-progression flow:
    /// Main Menu -> Level 1 -> Level 2 -> Level 3 -> Main Menu.
    ///
    /// Scene names MUST match the strings in `levelScenes` and `mainMenuScene`.
    /// Make sure every scene is added to File -> Build Settings -> Scenes In Build.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scene Names (must match Build Settings)")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string[] levelScenes = { "Level1", "Level2", "Level3" };

        private int currentLevelIndex = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>Called by the Main Menu "Play" button. Loads the first level.</summary>
        public void StartGame()
        {
            currentLevelIndex = 0;
            SceneManager.LoadScene(levelScenes[0]);
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Called by a LevelGoal trigger when the player finishes a level.
        /// Loads the next level, or returns to the main menu after the last level.
        /// </summary>
        public void CompleteLevel()
        {
            currentLevelIndex++;
            if (currentLevelIndex >= levelScenes.Length)
            {
                ReturnToMainMenu();
            }
            else
            {
                SceneManager.LoadScene(levelScenes[currentLevelIndex]);
            }
            Time.timeScale = 1f;
        }

        /// <summary>Reload the active scene (used by the Game Over "Restart" button).</summary>
        public void RestartCurrentLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>Load the main menu scene (used by Game Over "Quit to Menu").</summary>
        public void ReturnToMainMenu()
        {
            currentLevelIndex = -1;
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuScene);
        }

        /// <summary>Quit the built application. No effect in the Editor except stopping Play mode.</summary>
        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
