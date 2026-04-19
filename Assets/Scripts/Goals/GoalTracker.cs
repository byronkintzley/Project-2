using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project2
{
    /// <summary>
    /// Tracks in-level goals. Add ONE of these per level scene and configure
    /// the list of goals in the Inspector.
    ///
    /// Two built-in goal types are supported:
    ///
    ///   KillAllEnemies:
    ///     Automatically subscribes to every EnemyHealth found in the scene
    ///     when the level starts. The goal completes when all of them are dead.
    ///
    ///   CollectPickups:
    ///     Completes when RegisterPickup has been called with this goal's id
    ///     `requiredCount` times. Pickup.cs already calls this for you when
    ///     its type is set to Goal and its goalId matches.
    ///
    /// Listeners:
    ///   - LockedDoor can subscribe to OnGoalCompleted to auto-unlock.
    ///   - LevelGoal can check IsGoalComplete to only allow exit once cleared.
    /// </summary>
    public class GoalTracker : MonoBehaviour
    {
        public enum GoalType { KillAllEnemies, CollectPickups }

        [Serializable]
        public class Goal
        {
            public string id = "clearRoom";
            public GoalType type = GoalType.KillAllEnemies;
            public int requiredCount = 1;                 // only used for CollectPickups

            [HideInInspector] public int currentCount;
            [HideInInspector] public bool complete;

            // HashSet is not Unity-serializable. [NonSerialized] tells Unity to
            // skip it entirely so the field initializer below runs on construction.
            [System.NonSerialized] public HashSet<EnemyHealth> trackedEnemies = new HashSet<EnemyHealth>();
        }

        [SerializeField] private List<Goal> goals = new List<Goal>();

        /// <summary>Fires once when a goal's id transitions to complete.</summary>
        public event Action<string> OnGoalCompleted;

        private void Start()
        {
            foreach (var goal in goals)
            {
                if (goal.type == GoalType.KillAllEnemies)
                    BindEnemies(goal);
            }
        }

        private void BindEnemies(Goal goal)
        {
            EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                if (goal.trackedEnemies.Add(enemy))
                    enemy.OnDied += _ => HandleEnemyDied(goal);
            }
            goal.requiredCount = enemies.Length;

            if (enemies.Length == 0)
                MarkComplete(goal);
        }

        private void HandleEnemyDied(Goal goal)
        {
            goal.currentCount++;
            if (goal.currentCount >= goal.requiredCount)
                MarkComplete(goal);
        }

        /// <summary>Called by Pickup.cs when its type is Goal.</summary>
        public void RegisterPickup(string goalId)
        {
            Goal goal = goals.Find(g => g.id == goalId);
            if (goal == null || goal.complete) return;

            goal.currentCount++;
            if (goal.currentCount >= goal.requiredCount)
                MarkComplete(goal);
        }

        public bool IsGoalComplete(string goalId)
        {
            Goal goal = goals.Find(g => g.id == goalId);
            return goal != null && goal.complete;
        }

        private void MarkComplete(Goal goal)
        {
            if (goal.complete) return;
            goal.complete = true;
            Debug.Log($"[GoalTracker] Goal completed: {goal.id}");
            OnGoalCompleted?.Invoke(goal.id);
        }
    }
}
