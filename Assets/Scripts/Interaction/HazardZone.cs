using System.Collections.Generic;
using UnityEngine;

namespace Project2
{
    /// <summary>
    /// A trigger collider that damages any IDamageable inside it every
    /// `damageInterval` seconds. Put this on a GameObject with a Collider
    /// whose `Is Trigger` box is checked. Examples: lava, spikes, poison pool.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HazardZone : MonoBehaviour
    {
        [SerializeField] private float damagePerTick = 10f;
        [SerializeField] private float damageInterval = 1f;

        private readonly Dictionary<IDamageable, float> nextTickTime = new Dictionary<IDamageable, float>();

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[Hazard] Something entered: {other.name}");
            IDamageable dmg = other.GetComponentInParent<IDamageable>();
            if (dmg == null) return;

            // First hit is immediate.
            dmg.TakeDamage(damagePerTick);
            nextTickTime[dmg] = Time.time + damageInterval;
        }

        private void OnTriggerStay(Collider other)
        {
            IDamageable dmg = other.GetComponentInParent<IDamageable>();
            if (dmg == null) return;

            if (!nextTickTime.TryGetValue(dmg, out float t) || Time.time >= t)
            {
                dmg.TakeDamage(damagePerTick);
                nextTickTime[dmg] = Time.time + damageInterval;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            IDamageable dmg = other.GetComponentInParent<IDamageable>();
            if (dmg != null) nextTickTime.Remove(dmg);
        }
    }
}
