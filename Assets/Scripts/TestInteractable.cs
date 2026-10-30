
using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "Press E to inspect";
    [SerializeField] private string objectName = "Test Object";

    public string InteractionPrompt => prompt;

    public void Interact()
    {
        Debug.Log($"Interacted with: {objectName}", this);
    }
}
