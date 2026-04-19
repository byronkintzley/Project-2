using System;
using UnityEngine;

namespace Project2
{
    /// <summary>
    /// Put on each enemy. Implements IDamageable so the raycast weapon can hit it.
    /// Raises OnDied so GoalTracker (or anything else) can react.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 50f;
        [Tooltip("Seconds to wait after dying before destroying the GameObject " +
                 "(gives a death animation or sound time to play).")]
        [SerializeField] private float destroyDelay = 0f;

        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<EnemyHealth> OnDied;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHealth -= amount;
            if (CurrentHealth <= 0f)
                Die();
        }

        private void Die()
        {
            IsDead = true;
            OnDied?.Invoke(this);
            Destroy(gameObject, destroyDelay);
        }
    }
}
