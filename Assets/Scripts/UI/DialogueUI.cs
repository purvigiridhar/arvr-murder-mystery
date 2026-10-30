using System;
using UnityEngine;
using MurderMystery.Audio;

namespace MurderMystery.UI
{
    /// <summary>
    /// Person 4 Dialogue UI Presentation:
    /// Cinematic detective interview panel displaying speaker names, dialogue transcripts,
    /// and interactive dialogue response options for Person 3's narrative dialogue system.
    /// Operates without implementing story logic directly.
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        private static DialogueUI instance;
        public static DialogueUI Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<DialogueUI>();
                return instance;
            }
        }

        [Header("Display Settings")]
        [SerializeField] private KeyCode defaultAdvanceKey = KeyCode.Space;

        private bool isOpen = false;
        private string currentSpeaker = "";
        private string currentDialogue = "";
        private string[] currentOptions = null;
        private Action<int> onOptionSelectedCallback = null;

        // Visual Palette
        private readonly Color colorPanelBg = new Color(0.07f, 0.09f, 0.12f, 0.96f);     // #12161E
        private readonly Color colorBorder = new Color(0.18f, 0.23f, 0.31f, 1.0f);        // #2E3B4F
        private readonly Color colorSpeakerBadge = new Color(0.12f, 0.16f, 0.22f, 1.0f);  // #1F2937
        private readonly Color colorAmber = new Color(0.96f, 0.67f, 0.18f, 1.0f);         // #F5AB2E
        private readonly Color colorAmberHover = new Color(1.0f, 0.78f, 0.35f, 1.0f);
        private readonly Color colorTextOffWhite = new Color(0.95f, 0.96f, 0.98f, 1.0f);  // #F1F5F9
        private readonly Color colorTextMuted = new Color(0.65f, 0.72f, 0.82f, 1.0f);

        // Cached textures & styles
        private Texture2D panelBgTex;
        private Texture2D borderTex;
        private Texture2D speakerBadgeTex;
        private Texture2D optionBtnBgTex;
        private Texture2D amberAccentTex;

        private GUIStyle speakerStyle;
        private GUIStyle dialogueStyle;
        private GUIStyle optionButtonStyle;
        private GUIStyle advanceButtonStyle;
        private bool stylesInitialized = false;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void OnDestroy()
        {
            DestroyTex(ref panelBgTex);
            DestroyTex(ref borderTex);
            DestroyTex(ref speakerBadgeTex);
            DestroyTex(ref optionBtnBgTex);
            DestroyTex(ref amberAccentTex);
        }

        private void DestroyTex(ref Texture2D tex)
        {
            if (tex != null)
            {
                Destroy(tex);
                tex = null;
            }
        }

        private void Update()
        {
            if (!isOpen) return;

            // If simple continue prompt with no choices, Space or E advances
            if ((currentOptions == null || currentOptions.Length == 0) &&
                (Input.GetKeyDown(defaultAdvanceKey) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return)))
            {
                AdvanceOrSelect(-1);
            }
        }

        /// <summary>
        /// Public API for Person 3 narrative system: displays speaker and dialogue.
        /// </summary>
        public void ShowDialogue(string speakerName, string dialogueText, string[] options = null, Action<int> onSelect = null)
        {
            currentSpeaker = speakerName;
            currentDialogue = dialogueText;
            currentOptions = options;
            onOptionSelectedCallback = onSelect;
            isOpen = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            AudioManager.Instance?.PlayButtonClick();
        }

        public void HideDialogue()
        {
            isOpen = false;
            currentSpeaker = "";
            currentDialogue = "";
            currentOptions = null;
            onOptionSelectedCallback = null;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void AdvanceOrSelect(int optionIndex)
        {
            AudioManager.Instance?.PlayButtonClick();
            var callback = onOptionSelectedCallback;
            HideDialogue();
            callback?.Invoke(optionIndex);
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

            panelBgTex = MakeSolid(2, 2, colorPanelBg);
            borderTex = MakeSolid(2, 2, colorBorder);
            speakerBadgeTex = MakeSolid(2, 2, colorSpeakerBadge);
            optionBtnBgTex = MakeSolid(2, 2, new Color(0.11f, 0.15f, 0.20f, 1.0f));
            amberAccentTex = MakeSolid(2, 2, colorAmber);

            speakerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorAmber }
            };

            dialogueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = colorTextOffWhite }
            };

            optionButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(14, 10, 6, 6),
                normal = { textColor = colorTextOffWhite }
            };

            advanceButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = colorAmber }
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!isOpen) return;

            InitStyles();

            float totalWidth = Mathf.Min(840f, Screen.width - 60f);
            bool hasOptions = (currentOptions != null && currentOptions.Length > 0);
            float optionsHeight = hasOptions ? (currentOptions.Length * 34f) : 38f;
            float totalHeight = 110f + optionsHeight;

            float posX = (Screen.width - totalWidth) / 2f;
            float posY = Screen.height - totalHeight - 30f;

            Rect outerRect = new Rect(posX, posY, totalWidth, totalHeight);
            Rect innerRect = new Rect(posX + 1f, posY + 1f, totalWidth - 2f, totalHeight - 2f);

            // Container background
            GUI.DrawTexture(outerRect, borderTex);
            GUI.DrawTexture(innerRect, panelBgTex);

            // Speaker Tag Badge
            float badgeW = 240f;
            float badgeH = 28f;
            Rect badgeOuter = new Rect(posX + 16f, posY - 14f, badgeW, badgeH);
            Rect badgeInner = new Rect(badgeOuter.x + 1f, badgeOuter.y + 1f, badgeW - 2f, badgeH - 2f);

            GUI.DrawTexture(badgeOuter, borderTex);
            GUI.DrawTexture(badgeInner, speakerBadgeTex);
            GUI.Label(new Rect(badgeInner.x + 10f, badgeInner.y, badgeInner.width - 20f, badgeInner.height), currentSpeaker.ToUpper(), speakerStyle);

            // Dialogue Body Text
            Rect textRect = new Rect(posX + 20f, posY + 22f, totalWidth - 40f, 75f);
            GUI.Label(textRect, currentDialogue, dialogueStyle);

            // Dialogue Options or Continue Button
            float controlsY = posY + 95f;
            if (hasOptions)
            {
                for (int i = 0; i < currentOptions.Length; i++)
                {
                    Rect optRect = new Rect(posX + 20f, controlsY + (i * 32f), totalWidth - 40f, 28f);
                    string optLabel = $"  [ {i + 1} ]  {currentOptions[i]}";
                    if (GUI.Button(optRect, optLabel, optionButtonStyle))
                    {
                        AdvanceOrSelect(i);
                        return;
                    }
                }
            }
            else
            {
                Rect advRect = new Rect(posX + totalWidth - 160f, controlsY, 140f, 30f);
                if (GUI.Button(advRect, "CONTINUE [SPACE]", advanceButtonStyle))
                {
                    AdvanceOrSelect(-1);
                    return;
                }
            }
        }
    }
}

