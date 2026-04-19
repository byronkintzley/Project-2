using UnityEngine;

namespace Project2
{
    /// <summary>
    /// Wire these methods to the Main Menu scene's buttons:
    ///   Play button  -> OnPlayPressed
    ///   Quit button  -> OnQuitPressed
    ///
    /// Also ensures a GameManager exists. Drop a GameManager prefab into the
    /// Main Menu scene, or assign `gameManagerPrefab` below so one is spawned
    /// automatically the first time the menu loads.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Tooltip("Optional: instantiated if no GameManager exists yet (first boot).")]
        [SerializeField] private GameManager gameManagerPrefab;

        private void Awake()
        {
            if (GameManager.Instance == null && gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
            }

            // Menus generally want the cursor visible and unlocked.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;
        }

        public void OnPlayPressed()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StartGame();
            else
                Debug.LogError("[MainMenuController] No GameManager in scene. Assign a prefab or add one manually.");
        }

        public void OnQuitPressed()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.QuitApplication();
            else
                Application.Quit();
        }
    }
}
