using UnityEngine;

namespace MurderMystery
{
    public sealed class MysterySuspectInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private SuspectId suspect;

        private MysteryUI mysteryUI;
        private Camera playerCamera;

        public string InteractionPrompt =>
            $"Press E to interview {GetDisplayName(suspect)}";

        public SuspectId Suspect => suspect;

        public void Initialize(
            SuspectId suspectId,
            MysteryUI ui,
            Camera camera)
        {
            suspect = suspectId;
            mysteryUI = ui;
            playerCamera = camera;
        }

        public void Interact()
        {
            if (mysteryUI == null)
            {
                Debug.LogError(
                    $"MysterySuspectInteractable on '{name}' has no MysteryUI.",
                    this);
                return;
            }

            mysteryUI.OpenDialogue(suspect);
        }

        private void OnGUI()
        {
            if (playerCamera == null ||
                Vector3.Distance(
                    playerCamera.transform.position,
                    transform.position) > 12f)
            {
                return;
            }

            Vector3 screenPosition = playerCamera.WorldToScreenPoint(
                transform.position + Vector3.up * 1.15f);
            if (screenPosition.z <= 0f)
                return;

            Rect labelRect = new Rect(
                screenPosition.x - 75f,
                Screen.height - screenPosition.y - 14f,
                150f,
                26f);
            GUI.Box(labelRect, GetDisplayName(suspect));
        }

        public static string GetDisplayName(SuspectId suspectId)
        {
            switch (suspectId)
            {
                case SuspectId.RohanKapoor:
                    return "Rohan Kapoor";
                case SuspectId.PriyaSharma:
                    return "Priya Sharma";
                case SuspectId.VikramDesai:
                    return "Prof. Vikram Desai";
                case SuspectId.KarthikNair:
                    return "Karthik Nair";
                default:
                    return suspectId.ToString();
            }
        }
    }
}
