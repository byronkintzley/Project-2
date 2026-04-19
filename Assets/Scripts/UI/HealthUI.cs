using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project2
{
    /// <summary>
    /// Drives two optional UI pieces:
    ///   - A TextMeshPro label showing "current/max"
    ///   - A Slider acting as a health bar
    ///
    /// Assign whichever you use and leave the other null. The script subscribes
    /// to PlayerHealth events so it never has to poll in Update().
    /// </summary>
    public class HealthUI : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private TMP_Text healthLabel;
        [SerializeField] private Slider healthSlider;

        private void Start()
        {
            if (playerHealth == null)
            {
                // Fall back to finding it in the scene (Unity 6 API).
                playerHealth = FindFirstObjectByType<PlayerHealth>();
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += HandleHealthChanged;
                // Prime the UI with the starting values.
                HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
            else
            {
                Debug.LogWarning("[HealthUI] No PlayerHealth found.");
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
                playerHealth.OnHealthChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (healthLabel != null)
                healthLabel.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";

            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }
        }
    }
}
