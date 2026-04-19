using UnityEngine;

namespace Project2
{
    /// <summary>
    /// Put this on a trigger volume at the end of a level (e.g. a glowing portal).
    /// When the player walks into it, the GameManager loads the next level
    /// (or the main menu if this was the final level).
    ///
    /// Optionally require a goal to be completed first - e.g. "all enemies dead" -
    /// by ticking `requireGoalCompleted` and matching `goalId` to a GoalTracker.
    /// If the goal is not complete, the trigger does nothing.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class LevelGoal : MonoBehaviour
    {
        [SerializeField] private bool requireGoalCompleted = false;
        [SerializeField] private string goalId = "clearRoom";

        private bool triggered;
        private GoalTracker tracker;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void Start()
        {
            if (requireGoalCompleted)
                tracker = FindFirstObjectByType<GoalTracker>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggered || !other.CompareTag("Player")) return;

            if (requireGoalCompleted)
            {
                if (tracker == null || !tracker.IsGoalComplete(goalId))
                    return;
            }

            triggered = true;

            if (GameManager.Instance != null)
                GameManager.Instance.CompleteLevel();
            else
                Debug.LogError("[LevelGoal] No GameManager.Instance - was one created in the Main Menu?");
        }
    }
}
