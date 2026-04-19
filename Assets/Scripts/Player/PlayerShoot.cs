using UnityEngine;
using UnityEngine.InputSystem;

namespace Project2
{
    /// <summary>
    /// Raycast-based shooting. Put this on the Player (or a child of it that holds
    /// the first-person camera). Assign `cameraTransform` to the FPS camera so the
    /// ray originates at the camera and travels down its forward vector.
    ///
    /// On left-click: casts a ray up to `range`. If it hits something implementing
    /// IDamageable, deals `damage` to it. An optional muzzle-flash GameObject can
    /// be briefly enabled, and a debug line is drawn in the Scene view.
    ///
    /// Uses the new Input System (Mouse.current.leftButton). Requires the
    /// Input System package to be installed and enabled in Project Settings.
    /// </summary>
    public class PlayerShoot : MonoBehaviour
    {
        [Header("Raycast")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float range = 100f;
        [SerializeField] private float damage = 25f;
        [SerializeField] private LayerMask hitMask = ~0; // default: everything
        [SerializeField] private float fireRate = 0.25f; // seconds between shots

        [Header("Effects (optional)")]
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private float muzzleFlashTime = 0.05f;
        [SerializeField] private GameObject impactPrefab; // spawned at hit point
        [SerializeField] private float impactLifetime = 2f;

        private float nextFireTime;

        private void Awake()
        {
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            if (muzzleFlash != null) muzzleFlash.SetActive(false);
        }

        private void Update()
        {
            if (Time.time < nextFireTime) return;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Fire();
                nextFireTime = Time.time + fireRate;
            }
        }

        private void Fire()
        {
            if (cameraTransform == null)
            {
                Debug.LogError("[PlayerShoot] No camera transform assigned.");
                return;
            }

            if (muzzleFlash != null)
            {
                muzzleFlash.SetActive(true);
                Invoke(nameof(HideMuzzleFlash), muzzleFlashTime);
            }

            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 0.1f);

            if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                // Look up the chain - colliders are often on child objects.
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                damageable?.TakeDamage(damage);

                if (impactPrefab != null)
                {
                    GameObject fx = Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(fx, impactLifetime);
                }
            }
        }

        private void HideMuzzleFlash()
        {
            if (muzzleFlash != null) muzzleFlash.SetActive(false);
        }
    }
}
