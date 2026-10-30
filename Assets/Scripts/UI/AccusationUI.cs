using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using MurderMystery.Audio;

namespace MurderMystery.UI
{
    /// <summary>
    /// Person 4 Accusation and Ending Screen UI:
    /// Displays visually polished case resolution dossier screens (verdict, suspect profile,
    /// and evidence summary) provided by Person 3's accusation and mystery conclusion system.
    /// Does not implement culprit selection rules or mystery logic.
    /// </summary>
    public class AccusationUI : MonoBehaviour
    {
        private static AccusationUI instance;
        public static AccusationUI Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<AccusationUI>();
                return instance;
            }
        }

        private bool isOpen = false;
        private bool isSuccessVerdict = false;
        private string accusedSuspect = "";
        private string verdictSummaryText = "";
        private Action onRestartCallback = null;

        // Visual Palette
        private readonly Color colorDimOverlay = new Color(0.03f, 0.04f, 0.06f, 0.94f);
        private readonly Color colorCardBg = new Color(0.07f, 0.09f, 0.12f, 0.98f);       // #12161E
        private readonly Color colorCardBorder = new Color(0.18f, 0.23f, 0.31f, 1.0f);     // #2E3B4F
        private readonly Color colorAmber = new Color(0.96f, 0.67f, 0.18f, 1.0f);          // #F5AB2E
        private readonly Color colorRedAccent = new Color(0.86f, 0.20f, 0.20f, 1.0f);      // #DC3545
        private readonly Color colorGreenAccent = new Color(0.20f, 0.75f, 0.45f, 1.0f);
        private readonly Color colorTextOffWhite = new Color(0.95f, 0.96f, 0.98f, 1.0f);
        private readonly Color colorTextMuted = new Color(0.65f, 0.72f, 0.82f, 1.0f);

        // Cached textures & styles
        private Texture2D dimOverlayTex;
        private Texture2D cardBgTex;
        private Texture2D borderTex;
        private Texture2D headerBannerTex;

        private GUIStyle bannerStyle;
        private GUIStyle suspectHeaderStyle;
        private GUIStyle summaryStyle;
        private GUIStyle metaStyle;
        private GUIStyle actionButtonStyle;
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
            DestroyTex(ref dimOverlayTex);
            DestroyTex(ref cardBgTex);
            DestroyTex(ref borderTex);
            DestroyTex(ref headerBannerTex);
        }

        private void DestroyTex(ref Texture2D tex)
        {
            if (tex != null)
            {
                Destroy(tex);
                tex = null;
            }
        }

        /// <summary>
        /// Public API called by Person 3's accusation system to present the investigation outcome.
        /// </summary>
        public void ShowResolution(bool isSuccess, string suspectName, string verdictSummary, Action onRestart = null)
        {
            isOpen = true;
            isSuccessVerdict = isSuccess;
            accusedSuspect = suspectName;
            verdictSummaryText = verdictSummary;
            onRestartCallback = onRestart;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            AudioManager.Instance?.PlayJournalOpen();
        }

        public void HideResolution()
        {
            isOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
            borderTex = MakeSolid(2, 2, colorCardBorder);
            headerBannerTex = MakeSolid(2, 2, isSuccessVerdict ? colorGreenAccent : colorRedAccent);

            bannerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            suspectHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorAmber }
            };

            metaStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorTextMuted }
            };

            summaryStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = colorTextOffWhite }
            };

            actionButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = colorTextOffWhite }
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!isOpen) return;

            InitStyles();

            // Fullscreen backdrop dim
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimOverlayTex);

            float cardWidth = Mathf.Min(680f, Screen.width - 40f);
            float cardHeight = Mathf.Min(480f, Screen.height - 40f);
            float posX = (Screen.width - cardWidth) / 2f;
            float posY = (Screen.height - cardHeight) / 2f;

            Rect outerCard = new Rect(posX, posY, cardWidth, cardHeight);
            Rect innerCard = new Rect(posX + 1f, posY + 1f, cardWidth - 2f, cardHeight - 2f);

            GUI.DrawTexture(outerCard, borderTex);
            GUI.DrawTexture(innerCard, cardBgTex);

            // Banner Header
            float bannerHeight = 44f;
            Rect bannerRect = new Rect(posX + 1f, posY + 1f, cardWidth - 2f, bannerHeight);
            Color bannerColor = isSuccessVerdict ? new Color(0.12f, 0.40f, 0.22f, 1.0f) : new Color(0.48f, 0.12f, 0.12f, 1.0f);
            GUI.DrawTexture(bannerRect, MakeSolid(2, 2, bannerColor));

            string bannerTitle = isSuccessVerdict
                ? "OPERATION COMPLETED // ACCUSATION VERIFIED"
                : "INVESTIGATION FAILED // ERRONEOUS ACCUSATION";
            GUI.Label(bannerRect, bannerTitle, bannerStyle);

            // Suspect and Clue Meta Information
            float contentY = posY + bannerHeight + 20f;
            float padX = posX + 28f;
            float contentW = cardWidth - 56f;

            string suspectText = string.IsNullOrEmpty(accusedSuspect) ? "UNKNOWN SUSPECT" : accusedSuspect.ToUpper();
            GUI.Label(new Rect(padX, contentY, contentW, 24f), $"ACCUSED TARGET: {suspectText}", suspectHeaderStyle);
            contentY += 26f;

            int cluesCount = EvidenceManager.Instance != null ? EvidenceManager.Instance.CollectedCount : 0;
            string evidenceMeta = $"FORENSIC DOSSIER RECORD: {cluesCount} CLUES SUBMITTED IN SUPPORT";
            GUI.Label(new Rect(padX, contentY, contentW, 20f), evidenceMeta, metaStyle);
            contentY += 26f;

            // Divider
            GUI.DrawTexture(new Rect(padX, contentY, contentW, 1f), borderTex);
            contentY += 16f;

            // Summary Debrief
            float summaryBoxHeight = cardHeight - (contentY - posY) - 75f;
            GUI.Label(new Rect(padX, contentY, contentW, summaryBoxHeight), verdictSummaryText, summaryStyle);

            // Action Buttons
            float btnY = posY + cardHeight - 50f;
            float btnW = 180f;

            Rect restartBtnRect = new Rect(posX + cardWidth - btnW - 28f, btnY, btnW, 34f);
            if (GUI.Button(restartBtnRect, "RETRY CASE", actionButtonStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                HideResolution();
                if (onRestartCallback != null)
                {
                    onRestartCallback.Invoke();
                }
                else
                {
                    // Reload active scene
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                return;
            }

            Rect dismissBtnRect = new Rect(posX + cardWidth - (btnW * 2f) - 44f, btnY, btnW, 34f);
            if (GUI.Button(dismissBtnRect, "REVIEW DOSSIER [J]", actionButtonStyle))
            {
                HideResolution();
                if (EvidenceJournalUI.FindFirstObjectByType<EvidenceJournalUI>() is EvidenceJournalUI journal)
                {
                    journal.OpenJournal();
                }
                return;
            }
        }
    }
}

