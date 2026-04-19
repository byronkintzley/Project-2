using UnityEngine;

namespace Project2
{
    /// <summary>
    /// Very simple enemy behaviour - no NavMesh required:
    ///   1. Looks for a GameObject tagged "Player" on Start.
    ///   2. Moves toward it in a straight line whenever it's within `aggroRange`.
    ///   3. Deals `attackDamage` every `attackInterval` seconds while within `attackRange`.
    ///
    /// Works best with a Rigidbody (use `Is Kinematic` off, freeze rotation)
    /// OR a CharacterController. This script uses transform.position for simplicity
    /// so physics collisions with walls/floors work on a Rigidbody.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float aggroRange = 15f;
        [SerializeField] private float stopDistance = 1.2f;

        [Header("Attack")]
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackInterval = 1f;

        private Transform player;
        private PlayerHealth playerHealth;
        private Rigidbody rb;
        private float nextAttackTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
        }

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
            else
            {
                Debug.LogWarning("[EnemyAI] No GameObject tagged 'Player' found.");
            }
        }

        private void FixedUpdate()
        {
            if (player == null || playerHealth == null || playerHealth.IsDead) return;

            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;

            if (distance > aggroRange) return;

            // Face the player.
            if (toPlayer.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(toPlayer.normalized);

            // Move toward player until stopDistance.
            if (distance > stopDistance)
            {
                Vector3 step = toPlayer.normalized * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(rb.position + step);
            }

            // Attack if close enough.
            if (distance <= attackRange && Time.time >= nextAttackTime)
            {
                playerHealth.TakeDamage(attackDamage);
                nextAttackTime = Time.time + attackInterval;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aggroRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
