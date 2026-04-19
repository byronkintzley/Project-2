using UnityEngine;
using UnityEngine.InputSystem;

namespace Project2
{
    /// <summary>
    /// Minimal first-person controller using the new Input System directly
    /// (no InputActionAsset needed - reads Keyboard.current and Mouse.current).
    ///
    /// Setup:
    ///   - Create an empty GameObject called "Player".
    ///   - Tag it "Player".
    ///   - Add a CharacterController component (RequireComponent handles this).
    ///     Set: Center Y = 1, Radius = 0.5, Height = 2.
    ///   - Add a Camera as a CHILD of Player. Position it at head height (Y = 1.6).
    ///   - Add this FirstPersonController and assign the child camera to
    ///     `Camera Transform`.
    ///   - Add PlayerHealth, PlayerShoot (same child camera), PlayerInventory.
    ///
    /// Controls:
    ///   WASD  - move
    ///   Mouse - look
    ///   Space - jump
    ///   Shift - sprint
    ///
    /// REQUIRES the Input System package and "Active Input Handling" set to
    /// "Input System Package (New)" or "Both" in Project Settings > Player.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private Key sprintKey = Key.LeftShift;

        [Header("Look")]
        [Tooltip("Assign the child Camera's Transform. If left empty, Camera.main is used.")]
        [SerializeField] private Transform cameraTransform;
        [Tooltip("Mouse delta from the new Input System is in pixels/frame - much larger " +
                 "than the old Input.GetAxis values. 0.1 - 0.2 is usually a good range.")]
        [SerializeField] private float mouseSensitivity = 0.15f;
        [Range(0f, 89f)]
        [SerializeField] private float verticalLookLimit = 85f;

        [Header("Cursor")]
        [SerializeField] private bool lockCursorOnStart = true;

        private CharacterController controller;
        private Vector3 velocity;
        private float cameraPitch;

        /// <summary>Set to false to disable input (e.g. cutscenes).</summary>
        public bool InputEnabled { get; set; } = true;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void Start()
        {
            if (lockCursorOnStart)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            // Skip input while paused, dead, or explicitly disabled.
            // Time.timeScale == 0 covers PauseMenu and GameOverUI automatically.
            if (!InputEnabled || Time.timeScale <= 0f) return;

            HandleLook();
            HandleMovement();
        }

        // ---------------------------------------------------------------
        // Input helpers (new Input System, direct device API)
        // ---------------------------------------------------------------

        private Vector2 ReadMoveInput()
        {
            if (Keyboard.current == null) return Vector2.zero;

            Vector2 move = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) move.y += 1f;
            if (Keyboard.current.sKey.isPressed) move.y -= 1f;
            if (Keyboard.current.aKey.isPressed) move.x -= 1f;
            if (Keyboard.current.dKey.isPressed) move.x += 1f;
            return move;
        }

        private Vector2 ReadLookDelta() =>
            Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;

        private bool IsSprinting() =>
            Keyboard.current != null && Keyboard.current[sprintKey].isPressed;

        private bool JumpPressedThisFrame() =>
            Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        // ---------------------------------------------------------------

        private void HandleLook()
        {
            Vector2 delta = ReadLookDelta();
            float mouseX = delta.x * mouseSensitivity;
            float mouseY = delta.y * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -verticalLookLimit, verticalLookLimit);
            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            if (controller.isGrounded && velocity.y < 0f)
                velocity.y = -2f;

            Vector2 moveInput = ReadMoveInput();
            Vector3 input = transform.right * moveInput.x + transform.forward * moveInput.y;
            if (input.sqrMagnitude > 1f) input.Normalize();

            float speed = IsSprinting() ? sprintSpeed : walkSpeed;

            if (JumpPressedThisFrame() && controller.isGrounded)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            velocity.y += gravity * Time.deltaTime;

            Vector3 frameMove = new Vector3(input.x * speed, velocity.y, input.z * speed);
            controller.Move(frameMove * Time.deltaTime);
        }
    }
}
