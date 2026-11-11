using System;
using UnityEngine;
using MurderMystery.Audio;

namespace MurderMystery
{
    /// <summary>
    /// Person 4 Guide NPC Component:
    /// Attached to Arjun Mehta (Junior Research Assistant) in the Foyer.
    /// Implements Person 2A's IInteractable to provide beginner-friendly orientation dialogue
    /// without interfering with the 4 canonical suspects or Person 3's accusation system.
    /// </summary>
    public class GuideNpcInteractable : MonoBehaviour, IInteractable
    {
        [Header("NPC Identity")]
        [SerializeField] private string npcName = "Arjun Mehta";
        [SerializeField] private string npcRole = "Junior Research Assistant (Investigation Guide)";

        private Camera playerCamera;
        private bool isDialogueOpen = false;
        private int dialoguePageIndex = 0;

        private readonly string[] dialoguePages = new string[]
        {
            "You're finally here. Something terrible happened on this floor last night.",
            "Dr. Ananya Mitra was found dead inside the research facility.",
            "The investigators have left several clues behind. You'll need to speak to the staff and piece together what happened.",
            "Use WASD to move around. Look around using your mouse. Press E whenever you see an interaction prompt.",
            "Press J to open your evidence journal. Collect clues, question the suspects and don't accuse anyone until you've examined the evidence.",
            "Start by speaking to the people working on this floor. Good luck."
        };

        // Styling
        private readonly Color colorCardBg = new Color(0.08f, 0.10f, 0.14f, 0.98f);
        private readonly Color colorInnerPanel = new Color(0.12f, 0.15f, 0.20f, 1.0f);
        private readonly Color colorBorder = new Color(0.22f, 0.28f, 0.38f, 1.0f);
        private readonly Color colorCyanGuide = new Color(0.22f, 0.74f, 0.97f, 1.0f);
        private readonly Color colorAmber = new Color(0.96f, 0.67f, 0.18f, 1.0f);
        private readonly Color colorTextOffWhite = new Color(0.95f, 0.96f, 0.98f, 1.0f);
        private readonly Color colorDimOverlay = new Color(0.04f, 0.05f, 0.08f, 0.82f);

        private Texture2D dimOverlayTex;
        private Texture2D cardBgTex;
        private Texture2D innerPanelTex;
        private Texture2D borderTex;
        private Texture2D cyanTex;
        private Texture2D buttonBgTex;

        private GUIStyle nameBadgeStyle;
        private GUIStyle roleBadgeStyle;
        private GUIStyle bodyTextStyle;
        private GUIStyle buttonStyle;
        private GUIStyle worldTagStyle;
        private bool stylesInitialized = false;

        public string InteractionPrompt => $"Press E to talk to {npcName} (Guide)";
        public bool IsDialogueOpen => isDialogueOpen;

        private void Start()
        {
            FirstPersonController fpc = FindFirstObjectByType<FirstPersonController>();
            if (fpc != null)
            {
                playerCamera = fpc.GetComponentInChildren<Camera>();
            }
        }

        private void OnDestroy()
        {
            DestroyTex(ref dimOverlayTex);
            DestroyTex(ref cardBgTex);
            DestroyTex(ref innerPanelTex);
            DestroyTex(ref borderTex);
            DestroyTex(ref cyanTex);
            DestroyTex(ref buttonBgTex);
        }

        private void DestroyTex(ref Texture2D tex)
        {
            if (tex != null)
            {
                Destroy(tex);
                tex = null;
            }
        }

        public void Interact()
        {
            if (isDialogueOpen)
                return;

            isDialogueOpen = true;
            dialoguePageIndex = 0;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            FirstPersonController fpc = FindFirstObjectByType<FirstPersonController>();
            if (fpc != null) fpc.enabled = false;

            AudioManager.Instance?.PlayDialogueTick();
        }

        private void Update()
        {
            if (!isDialogueOpen)
                return;

            // Space or Return advances dialogue, Escape closes
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
            {
                AdvanceDialogue();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseDialogue();
            }
        }

        private void AdvanceDialogue()
        {
            dialoguePageIndex++;
            if (dialoguePageIndex >= dialoguePages.Length)
            {
                CloseDialogue();
            }
            else
            {
                AudioManager.Instance?.PlayDialogueTick();
            }
        }

        public void CloseDialogue()
        {
            isDialogueOpen = false;
            dialoguePageIndex = 0;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            FirstPersonController fpc = FindFirstObjectByType<FirstPersonController>();
            if (fpc != null) fpc.enabled = true;

            AudioManager.Instance?.PlayButtonClick();
        }

        private Texture2D MakeSolid(int w, int h, Color col)
        {
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
            Texture2D t = new Texture2D(w, h);
            t.SetPixels(pixels);
            t.Apply();
            return t;
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            dimOverlayTex = MakeSolid(2, 2, colorDimOverlay);
            cardBgTex = MakeSolid(2, 2, colorCardBg);
            innerPanelTex = MakeSolid(2, 2, colorInnerPanel);
            borderTex = MakeSolid(2, 2, colorBorder);
            cyanTex = MakeSolid(2, 2, colorCyanGuide);
            buttonBgTex = MakeSolid(2, 2, new Color(0.14f, 0.18f, 0.24f, 1.0f));

            nameBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorCyanGuide }
            };

            roleBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorAmber }
            };

            bodyTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = colorTextOffWhite }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { background = buttonBgTex, textColor = colorCyanGuide }
            };

            worldTagStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = colorCyanGuide }
            };

            stylesInitialized = true;
        }

        private void DrawBorderedBox(Rect r, Texture2D bg, Texture2D border)
        {
            GUI.DrawTexture(r, border);
            GUI.DrawTexture(new Rect(r.x + 1f, r.y + 1f, r.width - 2f, r.height - 2f), bg);
        }

        private void OnGUI()
        {
            InitStyles();

            // 1. Draw 3D World Name Tag above NPC Head
            DrawWorldNameTag();

            // 2. Draw Interactive Dialogue Panel
            if (isDialogueOpen)
            {
                DrawDialogueWindow();
            }
        }

        private void DrawWorldNameTag()
        {
            if (playerCamera == null)
            {
                FirstPersonController fpc = FindFirstObjectByType<FirstPersonController>();
                if (fpc != null) playerCamera = fpc.GetComponentInChildren<Camera>();
                if (playerCamera == null) return;
            }

            float dist = Vector3.Distance(playerCamera.transform.position, transform.position);
            if (dist > 15f)
                return;

            Vector3 screenPos = playerCamera.WorldToScreenPoint(transform.position + Vector3.up * 1.95f);
            if (screenPos.z <= 0f)
                return;

            float width = 230f;
            float height = 26f;
            float x = screenPos.x - (width / 2f);
            float y = Screen.height - screenPos.y - (height / 2f);

            Rect outerRect = new Rect(x, y, width, height);
            DrawBorderedBox(outerRect, cardBgTex, borderTex);

            // Left cyan indicator dot
            GUI.DrawTexture(new Rect(x + 6f, y + 9f, 8f, 8f), cyanTex);

            string tagText = $"{npcName.ToUpper()}  (GUIDE)";
            GUI.Label(new Rect(x + 18f, y, width - 24f, height), tagText, worldTagStyle);
        }

        private void DrawDialogueWindow()
        {
            GUI.depth = -95;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimOverlayTex);

            float width = 640f;
            float height = 260f;
            float x = (Screen.width - width) / 2f;
            float y = Screen.height - height - 40f;

            Rect cardRect = new Rect(x, y, width, height);
            DrawBorderedBox(cardRect, cardBgTex, borderTex);

            // Cyan top accent line
            GUI.DrawTexture(new Rect(x + 1f, y + 1f, width - 2f, 3f), cyanTex);

            float pad = 24f;

            // Header: Name & Role
            GUI.Label(new Rect(x + pad, y + 16f, width - (pad * 2f), 24f), npcName.ToUpper(), nameBadgeStyle);
            GUI.Label(new Rect(x + pad, y + 40f, width - (pad * 2f), 18f), $"// {npcRole.ToUpper()} //", roleBadgeStyle);

            GUI.DrawTexture(new Rect(x + pad, y + 64f, width - (pad * 2f), 1f), borderTex);

            // Dialogue Body Box
            Rect bodyBox = new Rect(x + pad, y + 76f, width - (pad * 2f), 105f);
            DrawBorderedBox(bodyBox, innerPanelTex, borderTex);

            Rect textRect = new Rect(bodyBox.x + 16f, bodyBox.y + 14f, bodyBox.width - 32f, bodyBox.height - 28f);
            string currentLine = dialoguePages[Mathf.Clamp(dialoguePageIndex, 0, dialoguePages.Length - 1)];
            GUI.Label(textRect, currentLine, bodyTextStyle);

            // Pagination indicator
            string pageIndicator = $"{dialoguePageIndex + 1} / {dialoguePages.Length}";
            GUI.Label(new Rect(x + pad, y + height - 44f, 100f, 28f), pageIndicator, roleBadgeStyle);

            // Next / Close Button
            bool isLastPage = (dialoguePageIndex >= dialoguePages.Length - 1);
            string btnLabel = isLastPage ? "FINISH [SPACE]" : "NEXT [SPACE]";
            float btnW = 160f;
            float btnH = 34f;
            Rect btnRect = new Rect(x + width - pad - btnW, y + height - 48f, btnW, btnH);

            if (GUI.Button(btnRect, btnLabel, buttonStyle))
            {
                AdvanceDialogue();
            }
        }
    }
}

