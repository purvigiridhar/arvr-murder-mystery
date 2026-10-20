using UnityEngine;

namespace MurderMystery
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 4.5f;
        public float runSpeed = 7.0f;
        public float gravity = -18.0f;
        public float jumpHeight = 1.2f;

        [Header("Look Settings")]
        public float mouseSensitivity = 2.0f;
        public float minPitch = -85.0f;
        public float maxPitch = 85.0f;
        public Transform cameraTransform;

        private CharacterController controller;
        private Vector3 velocity;
        private float pitch = 0f;
        private bool isGrounded;

        void Start()
        {
            controller = GetComponent<CharacterController>();
            if (cameraTransform == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null) cameraTransform = cam.transform;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            // Toggle cursor lock with Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = (Cursor.lockState != CursorLockMode.Locked);
            }

            HandleLook();
            HandleMovement();
        }

        void HandleLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Yaw (horizontal) on character
            transform.Rotate(Vector3.up * mouseX);

            // Pitch (vertical) on camera
            if (cameraTransform != null)
            {
                pitch -= mouseY;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
            }
        }

        void HandleMovement()
        {
            isGrounded = controller.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Slight downward force to stay grounded
            }

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

            controller.Move(moveDirection * speed * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
