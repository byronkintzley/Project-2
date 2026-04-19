using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project2
{
    /// <summary>
    /// Simple inventory for keys / generic collectible IDs.
    /// Pickup.cs adds items via AddKey(string).
    /// LockedDoor.cs queries via HasKey(string).
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        private readonly HashSet<string> keys = new HashSet<string>();

        public event Action<string> OnKeyAdded;

        public void AddKey(string keyId)
        {
            if (string.IsNullOrEmpty(keyId)) return;
            if (keys.Add(keyId))
                OnKeyAdded?.Invoke(keyId);
        }

        public bool HasKey(string keyId) => keys.Contains(keyId);

        public int KeyCount => keys.Count;
    }
}
