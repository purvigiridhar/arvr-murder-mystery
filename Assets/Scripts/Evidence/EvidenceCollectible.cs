using UnityEngine;

namespace MurderMystery
{
    /// <summary>
    /// Component attached to physical evidence objects in the scene.
    /// Implements Person 2A's IInteractable interface to integrate directly with InteractionRaycaster.
    /// </summary>
    public class EvidenceCollectible : MonoBehaviour, IInteractable
    {
        [Header("Evidence Data")]
        [Tooltip("The ScriptableObject defining this clue's identity and description.")]
        [SerializeField] private EvidenceItem evidenceItem;

        [Header("Interaction Settings")]
        [Tooltip("Custom prompt text displayed by Person 2A's InteractionPromptUI.")]
        [SerializeField] private string customPrompt = "Press E to collect evidence";

        [Tooltip("If true, automatically formats the prompt as 'Press E to collect [DisplayName]'.")]
        [SerializeField] private bool includeItemNameInPrompt = true;

        [Header("Collection Feedback")]
        [Tooltip("If true, disables the GameObject upon successful collection.")]
        [SerializeField] private bool disableGameObjectOnCollect = true;

        [Tooltip("If true and GameObject is not disabled, disables colliders on this object so it cannot be raycast again.")]
        [SerializeField] private bool disableColliderOnCollect = true;

        [Tooltip("Optional audio clip played at the clue's position when collected.")]
        [SerializeField] private AudioClip collectSound = null;

        [Tooltip("Optional visual effect instantiated when collected.")]
        [SerializeField] private GameObject collectVfxPrefab = null;

        private bool isCollected = false;

        /// <summary>
        /// Gets the prompt string for Person 2A's InteractionPromptUI.
        /// </summary>
        public string InteractionPrompt
        {
            get
            {
                if (isCollected)
                    return string.Empty;

                if (evidenceItem == null)
                    return "Press E to inspect clue (unassigned)";

                if (includeItemNameInPrompt && !string.IsNullOrEmpty(evidenceItem.DisplayName))
                    return $"Press E to collect {evidenceItem.DisplayName}";

                return customPrompt;
            }
        }

        public EvidenceItem Item => evidenceItem;
        public bool IsCollected => isCollected;

        /// <summary>
        /// Sets the evidence item programmatically (useful for dynamic spawning and tests).
        /// </summary>
        public void SetEvidenceItem(EvidenceItem item)
        {
            evidenceItem = item;
        }

        /// <summary>
        /// Triggered when the player presses E while looking at this object via InteractionRaycaster.
        /// </summary>
        public void Interact()
        {
            if (isCollected)
            {
                Debug.Log($"[EvidenceCollectible] '{gameObject.name}' has already been collected.", this);
                return;
            }

            if (evidenceItem == null)
            {
                Debug.LogWarning($"[EvidenceCollectible] Cannot collect: no EvidenceItem is assigned to '{gameObject.name}'.", this);
                return;
            }

            if (EvidenceManager.Instance == null)
            {
                Debug.LogError($"[EvidenceCollectible] Cannot collect '{evidenceItem.DisplayName}': EvidenceManager.Instance was not found in the scene.", this);
                return;
            }

            // Request collection via EvidenceManager
            bool collected = EvidenceManager.Instance.TryCollectEvidence(evidenceItem);

            if (collected)
            {
                isCollected = true;

                // Audio feedback
                if (collectSound != null)
                {
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);
                }

                // Visual feedback
                if (collectVfxPrefab != null)
                {
                    Instantiate(collectVfxPrefab, transform.position, Quaternion.identity);
                }

                // Deactivation feedback
                if (disableGameObjectOnCollect)
                {
                    gameObject.SetActive(false);
                }
                else if (disableColliderOnCollect)
                {
                    Collider[] colliders = GetComponentsInChildren<Collider>();
                    foreach (var col in colliders)
                    {
                        col.enabled = false;
                    }
                }
            }
        }
    }
}
