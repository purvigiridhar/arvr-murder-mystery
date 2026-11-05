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

        private static Texture2D badgeBgTex;
        private static Texture2D badgeBorderTex;
        private static GUIStyle badgeStyle;

        private void OnGUI()
        {
            if (playerCamera == null ||
                Vector3.Distance(
                    playerCamera.transform.position,
                    transform.position) > 14f)
            {
                return;
            }

            Vector3 screenPosition = playerCamera.WorldToScreenPoint(
                transform.position + Vector3.up * 1.18f);
            if (screenPosition.z <= 0f)
                return;

            if (badgeBgTex == null)
            {
                badgeBgTex = new Texture2D(2, 2);
                badgeBgTex.SetPixels(new[] {
                    new Color(0.07f, 0.09f, 0.12f, 0.92f), new Color(0.07f, 0.09f, 0.12f, 0.92f),
                    new Color(0.07f, 0.09f, 0.12f, 0.92f), new Color(0.07f, 0.09f, 0.12f, 0.92f)
                });
                badgeBgTex.Apply();

                badgeBorderTex = new Texture2D(2, 2);
                badgeBorderTex.SetPixels(new[] {
                    new Color(0.18f, 0.23f, 0.31f, 1f), new Color(0.18f, 0.23f, 0.31f, 1f),
                    new Color(0.18f, 0.23f, 0.31f, 1f), new Color(0.18f, 0.23f, 0.31f, 1f)
                });
                badgeBorderTex.Apply();

                badgeStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.96f, 0.67f, 0.18f, 1f) }
                };
            }

            string name = GetDisplayName(suspect);
            float width = Mathf.Max(140f, badgeStyle.CalcSize(new GUIContent(name)).x + 20f);
            float height = 24f;

            Rect outerRect = new Rect(
                screenPosition.x - (width / 2f),
                Screen.height - screenPosition.y - 12f,
                width,
                height);
            Rect innerRect = new Rect(outerRect.x + 1f, outerRect.y + 1f, outerRect.width - 2f, outerRect.height - 2f);

            GUI.DrawTexture(outerRect, badgeBorderTex);
            GUI.DrawTexture(innerRect, badgeBgTex);
            GUI.Label(innerRect, name, badgeStyle);
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
