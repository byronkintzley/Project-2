using UnityEngine;

namespace Project2
{
    /// <summary>
    /// A door that starts locked and opens when an unlock condition is met:
    ///   - RequireKey:   unlocks when the player steps near and has `keyId`.
    ///   - RequireGoal:  stays locked until GoalTracker signals `goalId` complete.
    ///                   After that, the door auto-opens (or opens on approach).
    ///
    /// Opening is a simple slide upward - fine for a student project, and easy
    /// to replace with an Animator later. Put this script on the door root;
    /// the visible door mesh can be this object or a child.
    /// </summary>
    public class LockedDoor : MonoBehaviour
    {
        public enum UnlockMode { RequireKey, RequireGoal }

        [Header("Unlock Condition")]
        [SerializeField] private UnlockMode mode = UnlockMode.RequireKey;
        [SerializeField] private string keyId = "RedKey";
        [SerializeField] private string goalId = "clearRoom";

        [Header("Open Motion")]
        [Tooltip("Local offset from the closed position when fully open.")]
        [SerializeField] private Vector3 openOffset = new Vector3(0f, 4f, 0f);
        [SerializeField] private float openSpeed = 2f;

        [Header("Interaction")]
        [Tooltip("If true, the door opens automatically when the player is near and the condition is met.")]
        [SerializeField] private bool openOnProximity = true;
        [SerializeField] private float proximityRange = 3f;

        private Vector3 closedPos;
        private Vector3 openPos;
        private bool unlocked;
        private bool opening;
        private Transform player;

        private void Start()
        {
            closedPos = transform.position;
            openPos = closedPos + openOffset;

            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;

            if (mode == UnlockMode.RequireGoal)
            {
                GoalTracker tracker = FindFirstObjectByType<GoalTracker>();
                if (tracker != null)
                    tracker.OnGoalCompleted += HandleGoalCompleted;
            }
        }

        private void OnDestroy()
        {
            GoalTracker tracker = FindFirstObjectByType<GoalTracker>();
            if (tracker != null)
                tracker.OnGoalCompleted -= HandleGoalCompleted;
        }

        private void Update()
        {
            if (!opening && !unlocked && mode == UnlockMode.RequireKey && player != null && openOnProximity)
            {
                if (Vector3.Distance(transform.position, player.position) <= proximityRange)
                {
                    var inventory = player.GetComponent<PlayerInventory>();
                    if (inventory != null && inventory.HasKey(keyId))
                        Unlock();
                }
            }

            if (opening)
            {
                transform.position = Vector3.MoveTowards(transform.position, openPos, openSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, openPos) < 0.01f)
                    opening = false;
            }
        }

        private void HandleGoalCompleted(string id)
        {
            if (mode == UnlockMode.RequireGoal && id == goalId)
                Unlock();
        }

        public void Unlock()
        {
            if (unlocked) return;
            unlocked = true;
            opening = true;
        }
    }
}
