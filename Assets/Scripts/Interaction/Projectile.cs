using UnityEngine;

namespace Project2
{
    /// <summary>
    /// Optional alternative to PlayerShoot's raycast. A Rigidbody projectile that
    /// flies forward, damages the first IDamageable it hits, and self-destructs.
    ///
    /// Set this up as a prefab with:
    ///   - Rigidbody (Use Gravity off for a straight shot, or on for arcs)
    ///   - Collider (Is Trigger = true, so OnTriggerEnter fires)
    ///
    /// A separate launcher script (or PlayerShoot extended) should:
    ///   var p = Instantiate(prefab, muzzle.position, muzzle.rotation);
    ///   p.GetComponent<Rigidbody>().linearVelocity = muzzle.forward * speed;
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float damage = 25f;
        [SerializeField] private float lifetime = 5f;
        [Tooltip("Objects with these tags will be ignored (e.g. 'Player' so it doesn't hit its owner).")]
        [SerializeField] private string[] ignoreTags = { "Player" };

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter(Collider other)
        {
            foreach (var t in ignoreTags)
                if (other.CompareTag(t)) return;

            IDamageable dmg = other.GetComponentInParent<IDamageable>();
            dmg?.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
