using UnityEngine;
using MurderMystery.Audio;

namespace MurderMystery
{
    /// <summary>
    /// Person 4 Player Footsteps:
    /// Tracks player locomotion via CharacterController and plays paced footsteps
    /// matching the player's movement speed. Operates cleanly alongside Person 2A's FirstPersonController.
    /// </summary>
    public class PlayerFootsteps : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float runStepInterval = 0.35f;
        [SerializeField] private float minVelocityThreshold = 0.4f;

        private CharacterController characterController;
        private float stepTimer = 0f;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (characterController == null || !characterController.isGrounded)
            {
                return;
            }

            Vector3 horizontalVelocity = new Vector3(
                characterController.velocity.x,
                0f,
                characterController.velocity.z);

            float speed = horizontalVelocity.magnitude;
            if (speed < minVelocityThreshold)
            {
                stepTimer = 0f;
                return;
            }

            bool isRunning = speed > 5.0f;
            float currentInterval = isRunning ? runStepInterval : walkStepInterval;

            stepTimer += Time.deltaTime;
            if (stepTimer >= currentInterval)
            {
                stepTimer = 0f;
                AudioManager.Instance?.PlayFootstep();
            }
        }
    }
}

