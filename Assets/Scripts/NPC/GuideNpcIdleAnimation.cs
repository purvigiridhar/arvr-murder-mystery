using UnityEngine;

namespace MurderMystery
{
    /// <summary>
    /// Person 4 Guide NPC Idle Animation:
    /// Provides subtle, natural humanoid breathing and idle posture sway
    /// for Arjun Mehta in the Foyer so he looks like a living, breathing guide.
    /// </summary>
    public class GuideNpcIdleAnimation : MonoBehaviour
    {
        [Header("Animation Parameters")]
        [SerializeField] private float breatheSpeed = 1.4f;
        [SerializeField] private float breatheAmount = 0.015f;
        [SerializeField] private float swaySpeed = 0.8f;
        [SerializeField] private float swayAngle = 1.2f;

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void Start()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        private void Update()
        {
            float time = Time.time;

            // Subtle vertical breathing oscillation
            float verticalOffset = Mathf.Sin(time * breatheSpeed) * breatheAmount;
            transform.position = initialPosition + new Vector3(0f, verticalOffset, 0f);

            // Subtle lateral shoulder/torso sway
            float angleOffset = Mathf.Sin(time * swaySpeed) * swayAngle;
            transform.rotation = initialRotation * Quaternion.Euler(0f, angleOffset, 0f);
        }
    }
}

