
using UnityEngine;

public class InteractionRaycaster : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InteractionPromptUI promptUI;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactionMask = ~0;

    private IInteractable currentInteractable;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
            Debug.LogError("InteractionRaycaster: Camera is missing.", this);

        if (promptUI == null)
            Debug.LogError("InteractionRaycaster: Prompt UI is missing.", this);
    }

    private void Update()
    {
        DetectInteractable();

        if (currentInteractable != null &&
            Cursor.lockState == CursorLockMode.Locked &&
            Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
        }
    }

    private void DetectInteractable()
    {
        IInteractable detected = null;

        if (playerCamera != null &&
            Physics.Raycast(
                playerCamera.transform.position,
                playerCamera.transform.forward,
                out RaycastHit hit,
                interactionRange,
                interactionMask,
                QueryTriggerInteraction.Ignore))
        {
            MonoBehaviour[] components =
                hit.collider.GetComponentsInParent<MonoBehaviour>();

            foreach (MonoBehaviour component in components)
            {
                if (component is IInteractable interactable)
                {
                    detected = interactable;
                    break;
                }
            }
        }

        if (detected != currentInteractable)
        {
            currentInteractable = detected;

            if (currentInteractable != null)
                promptUI?.Show(currentInteractable.InteractionPrompt);
            else
                promptUI?.Hide();
        }
    }

    private void OnDisable()
    {
        currentInteractable = null;
        promptUI?.Hide();
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(
            playerCamera.transform.position,
            playerCamera.transform.forward * interactionRange
        );
    }
}
