using UnityEngine;

namespace Project2
{
    /// <summary>
    /// A trigger pickup. Supports three modes:
    ///   - Key:    adds `keyId` to the player's PlayerInventory.
    ///   - Health: heals the player by `amount`.
    ///   - Goal:   just notifies a GoalTracker with `goalId` (see GoalTracker).
    ///
    /// Put on a GameObject with a trigger collider. The pickup destroys itself
    /// when collected.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Pickup : MonoBehaviour
    {
        public enum PickupType { Key, Health, Goal }

        [SerializeField] private PickupType type = PickupType.Key;
        [SerializeField] private string keyId = "RedKey";
        [SerializeField] private float amount = 25f;
        [SerializeField] private string goalId = "collectible";
        [SerializeField] private GameObject collectVfxPrefab;

        private bool collected;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected) return;
            if (!other.CompareTag("Player")) return;

            switch (type)
            {
                case PickupType.Key:
                    var inventory = other.GetComponentInParent<PlayerInventory>();
                    if (inventory != null) inventory.AddKey(keyId);
                    break;

                case PickupType.Health:
                    var health = other.GetComponentInParent<PlayerHealth>();
                    if (health != null) health.Heal(amount);
                    break;

                case PickupType.Goal:
                    GoalTracker tracker = FindFirstObjectByType<GoalTracker>();
                    if (tracker != null) tracker.RegisterPickup(goalId);
                    break;
            }

            collected = true;
            if (collectVfxPrefab != null)
                Instantiate(collectVfxPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
