using System;
using UnityEngine;

namespace Project2
{
    /// <summary>
    /// Player health. Implements IDamageable so hazards/enemies can call TakeDamage.
    /// Exposes OnHealthChanged for HealthUI, and triggers the Game Over screen at 0.
    ///
    /// Put this on the Player GameObject. Tag the Player as "Player" so hazards
    /// and enemies can find it easily.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private GameOverUI gameOverUI;

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        /// <summary>Fires whenever health changes. Args: (current, max).</summary>
        public event Action<float, float> OnHealthChanged;
        public event Action OnDied;

        private void Start()
        {
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (gameOverUI == null)
                gameOverUI = FindFirstObjectByType<GameOverUI>(FindObjectsInactive.Include);
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0f)
                Die();
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void Die()
        {
            IsDead = true;
            OnDied?.Invoke();

            if (gameOverUI != null)
                gameOverUI.Show();
            else
                Debug.LogWarning("[PlayerHealth] No GameOverUI assigned - player died with no screen.");
        }
    }
}
