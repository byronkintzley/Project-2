using UnityEngine;
using UnityEngine.InputSystem;

namespace Project2
{
    /// <summary>
    /// Put this on the gun viewmodel (the child of PlayerCamera). Adds:
    ///   - Sway: gun lags slightly behind mouse movement (feels weighty)
    ///   - Recoil: gun kicks backward briefly on fire
    ///
    /// Call Kick() from PlayerShoot when it fires, OR leave `autoDetectFire`
    /// on and this script watches for left-click itself.
    /// </summary>
    public class WeaponSway : MonoBehaviour
    {
        [Header("Sway")]
        [SerializeField] private float swayAmount = 0.02f;
        [SerializeField] private float maxSway = 0.06f;
        [SerializeField] private float swaySmooth = 6f;

        [Header("Recoil")]
        [SerializeField] private Vector3 recoilOffset = new Vector3(0f, 0.02f, -0.08f);
        [SerializeField] private float recoilRecoverySpeed = 8f;
        [SerializeField] private bool autoDetectFire = true;

        private Vector3 restPosition;
        private Vector3 currentRecoil;

        private void Start()
        {
            restPosition = transform.localPosition;
        }

        private void Update()
        {
            Vector2 lookDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;

            // Sway opposite to mouse direction, clamped so it doesn't fly off-screen.
            float swayX = Mathf.Clamp(-lookDelta.x * swayAmount * 0.01f, -maxSway, maxSway);
            float swayY = Mathf.Clamp(-lookDelta.y * swayAmount * 0.01f, -maxSway, maxSway);
            Vector3 swayTarget = new Vector3(swayX, swayY, 0f);

            if (autoDetectFire && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                Kick();

            // Recoil springs back to zero over time.
            currentRecoil = Vector3.Lerp(currentRecoil, Vector3.zero, recoilRecoverySpeed * Time.deltaTime);

            Vector3 target = restPosition + swayTarget + currentRecoil;
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, swaySmooth * Time.deltaTime);
        }

        /// <summary>Adds a recoil kick. Call on fire if autoDetectFire is off.</summary>
        public void Kick() => currentRecoil += recoilOffset;
    }
}
