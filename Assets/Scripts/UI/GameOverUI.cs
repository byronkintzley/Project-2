using UnityEngine;

namespace Project2
{
    /// <summary>
    /// Put this on the root GameObject of the Game Over panel (the whole panel,
    /// usually a child of the Canvas). The panel should start disabled.
    /// PlayerHealth enables it when the player dies.
    ///
    /// Wire the two buttons:
    ///   Restart     -> OnRestartPressed
    ///   Quit to Menu-> OnQuitToMenuPressed
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;

        private void Awake()
        {
            if (panelRoot == null) panelRoot = gameObject;
            panelRoot.SetActive(false);
        }

        /// <summary>Called by PlayerHealth when health <= 0.</summary>
        public void Show()
        {
            panelRoot.SetActive(true);
            Time.timeScale = 0f;       // freeze gameplay
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void OnRestartPressed()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.RestartCurrentLevel();
        }

        public void OnQuitToMenuPressed()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ReturnToMainMenu();
        }
    }
}
