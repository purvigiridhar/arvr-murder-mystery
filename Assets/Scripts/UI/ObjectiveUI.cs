using System;
using UnityEngine;
using MurderMystery.Audio;

namespace MurderMystery.UI
{
    /// <summary>
    /// Person 4 Objective Display:
    /// Compact, cinematic detective HUD displaying current directives, investigation status,
    /// and clue collection count. Automatically listens to EvidenceManager events to update
    /// progress counters. Allows Person 3 narrative system to update active objectives.
    /// </summary>
    public class ObjectiveUI : MonoBehaviour
    {
        private static ObjectiveUI instance;
        public static ObjectiveUI Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<ObjectiveUI>();
                return instance;
            }
        }

        [Header("Settings")]
        [SerializeField] private bool isVisible = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.O;

        [Header("Default Directive")]
        [SerializeField] private string currentObjectiveTitle = "Investigate 11th Floor Facility";
        [SerializeField] private string currentObjectiveDetails = "Search rooms, recover digital and physical evidence, and review findings in your Case Dossier [J].";

        // Visual Palette
        private readonly Color colorPanelBg = new Color(0.07f, 0.09f, 0.12f, 0.92f);     // #12161E
        private readonly Color colorBorder = new Color(0.18f, 0.23f, 0.31f, 1.0f);        // #2E3B4F
        private readonly Color colorAmber = new Color(0.96f, 0.67f, 0.18f, 1.0f);         // #F5AB2E
        private readonly Color colorTextOffWhite = new Color(0.95f, 0.96f, 0.98f, 1.0f);  // #F1F5F9
        private readonly Color colorTextMuted = new Color(0.65f, 0.72f, 0.82f, 1.0f);     // #A6B8D1

        // Cached textures & styles
        private Texture2D panelBgTex;
        private Texture2D borderTex;
        private Texture2D amberDotTex;
        private GUIStyle headerStyle;
        private GUIStyle titleStyle;
        private GUIStyle detailsStyle;
        private GUIStyle counterStyle;
        private bool stylesInitialized = false;

        public bool IsVisible => isVisible;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void Start()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnEvidenceCollected += OnClueCollected;
            }
        }

        private void OnDestroy()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnEvidenceCollected -= OnClueCollected;
            }

            DestroyTex(ref panelBgTex);
            DestroyTex(ref borderTex);
            DestroyTex(ref amberDotTex);
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
            if (Input.GetKeyDown(toggleKey))
            {
                isVisible = !isVisible;
                AudioManager.Instance?.PlayButtonClick();
            }
        }

        private void OnClueCollected(EvidenceItem item)
        {
            // Flash or refresh HUD
        }

        /// <summary>
        /// Public API for Person 3 / narrative progression to update the objective.
        /// </summary>
        public void SetObjective(string title, string details = "")
        {
            currentObjectiveTitle = title;
            currentObjectiveDetails = details;
        }

        public void SetVisible(bool visible)
        {
            isVisible = visible;
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
            amberDotTex = MakeSolid(2, 2, colorAmber);

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorAmber }
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorTextOffWhite }
            };

            detailsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                wordWrap = true,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorTextMuted }
            };

            counterStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = colorAmber }
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!isVisible) return;

            InitStyles();

            // Position at top-right
            float panelWidth = 320f;
            float panelHeight = string.IsNullOrEmpty(currentObjectiveDetails) ? 55f : 85f;
            float posX = Screen.width - panelWidth - 20f;
            float posY = 20f;

            Rect outerRect = new Rect(posX, posY, panelWidth, panelHeight);
            Rect innerRect = new Rect(posX + 1f, posY + 1f, panelWidth - 2f, panelHeight - 2f);

            // Background & Border
            GUI.DrawTexture(outerRect, borderTex);
            GUI.DrawTexture(innerRect, panelBgTex);

            // Amber Indicator Dot
            Rect dotRect = new Rect(posX + 12f, posY + 10f, 6f, 6f);
            GUI.DrawTexture(dotRect, amberDotTex);

            // Directive Header
            Rect headerRect = new Rect(posX + 24f, posY + 6f, 150f, 16f);
            GUI.Label(headerRect, "ACTIVE DIRECTIVE", headerStyle);

            // Clue Counter Badge
            int cluesCollected = EvidenceManager.Instance != null ? EvidenceManager.Instance.CollectedCount : 0;
            Rect counterRect = new Rect(posX + panelWidth - 140f, posY + 6f, 126f, 16f);
            GUI.Label(counterRect, $"CLUES: {cluesCollected} / 3", counterStyle);

            // Title
            Rect titleRect = new Rect(posX + 12f, posY + 22f, panelWidth - 24f, 20f);
            GUI.Label(titleRect, currentObjectiveTitle, titleStyle);

            // Optional Details
            if (!string.IsNullOrEmpty(currentObjectiveDetails))
            {
                Rect detailsRect = new Rect(posX + 12f, posY + 42f, panelWidth - 24f, 38f);
                GUI.Label(detailsRect, currentObjectiveDetails, detailsStyle);
            }
        }
    }
}

