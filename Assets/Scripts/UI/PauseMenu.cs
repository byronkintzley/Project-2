using UnityEngine;
using UnityEngine.InputSystem;

namespace Project2
{
    /// <summary>
    /// Esc-to-pause menu. Put this on the Pause Menu panel's root GameObject
    /// (usually a full-screen Panel under the HUD Canvas). The panel hides
    /// itself on Awake.
    ///
    /// Wire buttons to:
    ///   Resume       -> PauseMenu.OnResumePressed
    ///   Restart      -> PauseMenu.OnRestartPressed   (optional)
    ///   Quit to Menu -> PauseMenu.OnQuitToMenuPressed
    ///
    /// Behaviour:
    ///   - Esc toggles the panel.
    ///   - Freezes gameplay by setting Time.timeScale = 0.
    ///   - Releases the cursor while paused, re-locks it on resume.
    ///   - Will NOT open if the player is already dead (GameOverUI has priority).
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Key toggleKey = Key.Escape;
        [Tooltip("Auto-found if left empty. Prevents pausing while dead.")]
        [SerializeField] private PlayerHealth playerHealth;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (panelRoot == null) panelRoot = gameObject;

            // If panelRoot is this GameObject, disabling it here would also stop
            // Update() from running, meaning Esc can't unpause. Warn and bail.
            if (panelRoot == gameObject)
            {
                Debug.LogError("[PauseMenu] `panelRoot` is set to this GameObject. " +
                    "Put PauseMenu on a parent (e.g. the Canvas) and assign the " +
                    "panel to `panelRoot`, otherwise the Esc toggle will not work.");
                return;
            }

            panelRoot.SetActive(false);
        }

        private void Start()
        {
            if (playerHealth == null)
                playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        private void OnDisable()
        {
            // Belt-and-suspenders: if this object gets disabled while paused
            // (e.g. scene unloading), make sure time isn't left frozen.
            if (IsPaused) Time.timeScale = 1f;
        }

        private void Update()
        {
            if (Keyboard.current == null) return;
            if (!Keyboard.current[toggleKey].wasPressedThisFrame) return;

            // Don't fight with the Game Over screen.
            if (playerHealth != null && playerHealth.IsDead) return;

            if (IsPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            IsPaused = true;
            panelRoot.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            IsPaused = false;
            panelRoot.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // ---- Button hooks ----

        public void OnResumePressed() => Resume();

        public void OnRestartPressed()
        {
            // Clear pause state before reloading so the next scene starts clean.
            IsPaused = false;
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
                GameManager.Instance.RestartCurrentLevel();
        }

        public void OnQuitToMenuPressed()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
                GameManager.Instance.ReturnToMainMenu();
        }
    }
}
