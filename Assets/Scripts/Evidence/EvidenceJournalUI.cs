using System;
using System.Collections.Generic;
using UnityEngine;
using MurderMystery.Audio;

namespace MurderMystery
{
    /// <summary>
    /// Person 4 Redesigned Evidence Journal:
    /// Cinematic detective case dossier featuring deep charcoal & navy backgrounds,
    /// warm amber highlights, category filter tabs, high-contrast typography,
    /// scrollable clue dossier, and integrated audio cues.
    /// Reuses Person 2's EvidenceJournalUI architecture without breaking existing tests or contracts.
    /// </summary>
    public class EvidenceJournalUI : MonoBehaviour
    {
        [Header("Keybindings")]
        [Tooltip("Key used to toggle the evidence journal open/closed.")]
        [SerializeField] private KeyCode toggleKey = KeyCode.J;

        [Header("Display Settings")]
        [Tooltip("Show a small HUD hint showing keybinding and evidence count when closed.")]
        [SerializeField] private bool showHudHint = true;

        private bool isOpen = false;
        private int selectedIndex = -1;
        private int selectedCategoryFilter = -1; // -1 = All, 0 = Physical, 1 = Document, etc.
        private Vector2 listScrollPosition = Vector2.zero;
        private Vector2 detailScrollPosition = Vector2.zero;

        // Cinematic Detective Color Palette
        private readonly Color colorBackdropDim = new Color(0.04f, 0.05f, 0.07f, 0.85f);
        private readonly Color colorWindowBg = new Color(0.07f, 0.09f, 0.12f, 0.98f);       // #12161E
        private readonly Color colorInnerPanel = new Color(0.10f, 0.13f, 0.18f, 1.0f);      // #1A212E
        private readonly Color colorPanelBorder = new Color(0.18f, 0.23f, 0.31f, 1.0f);     // #2E3B4F
        private readonly Color colorAmberAccent = new Color(0.96f, 0.67f, 0.18f, 1.0f);     // #F5AB2E
        private readonly Color colorAmberHover = new Color(1.0f, 0.78f, 0.35f, 1.0f);
        private readonly Color colorSelectedBg = new Color(0.16f, 0.22f, 0.32f, 1.0f);
        private readonly Color colorTextOffWhite = new Color(0.95f, 0.96f, 0.98f, 1.0f);    // #F1F5F9
        private readonly Color colorTextMuted = new Color(0.60f, 0.67f, 0.77f, 1.0f);       // #99ABC4
        private readonly Color colorTextDark = new Color(0.08f, 0.10f, 0.14f, 1.0f);

        // Cached GUI textures
        private Texture2D overlayTexture;
        private Texture2D windowBgTexture;
        private Texture2D innerPanelTexture;
        private Texture2D borderTexture;
        private Texture2D amberAccentTexture;
        private Texture2D selectedBgTexture;
        private Texture2D keyBadgeTexture;

        // Cached GUI styles
        private GUIStyle titleStyle;
        private GUIStyle counterBadgeStyle;
        private GUIStyle headerStyle;
        private GUIStyle tabButtonStyle;
        private GUIStyle activeTabButtonStyle;
        private GUIStyle clueButtonStyle;
        private GUIStyle selectedClueButtonStyle;
        private GUIStyle clueTitleStyle;
        private GUIStyle categoryBadgeStyle;
        private GUIStyle idStyle;
        private GUIStyle descriptionStyle;
        private GUIStyle emptyStateStyle;
        private GUIStyle closeButtonStyle;
        private GUIStyle hudHintKeyStyle;
        private GUIStyle hudHintTextStyle;

        private bool stylesInitialized = false;

        public bool IsOpen => isOpen;

        private void Start()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnEvidenceCollected += HandleEvidenceCollected;
                EvidenceManager.Instance.OnEvidenceCleared += HandleEvidenceCleared;
            }
        }

        private void OnDestroy()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnEvidenceCollected -= HandleEvidenceCollected;
                EvidenceManager.Instance.OnEvidenceCleared -= HandleEvidenceCleared;
            }

            DestroyTex(ref overlayTexture);
            DestroyTex(ref windowBgTexture);
            DestroyTex(ref innerPanelTexture);
            DestroyTex(ref borderTexture);
            DestroyTex(ref amberAccentTexture);
            DestroyTex(ref selectedBgTexture);
            DestroyTex(ref keyBadgeTexture);
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
                ToggleJournal();
            }
            else if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseJournal();
            }
        }

        public void ToggleJournal()
        {
            if (isOpen)
                CloseJournal();
            else
                OpenJournal();
        }

        public void OpenJournal()
        {
            isOpen = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            AudioManager.Instance?.PlayJournalOpen();

            // Auto-select first clue if available and nothing is selected
            if (EvidenceManager.Instance != null)
            {
                var clues = EvidenceManager.Instance.GetCollectedEvidence();
                if (clues.Count > 0 && (selectedIndex < 0 || selectedIndex >= clues.Count))
                {
                    selectedIndex = 0;
                }
            }
        }

        public void CloseJournal()
        {
            isOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            AudioManager.Instance?.PlayJournalClose();
        }

        private void HandleEvidenceCollected(EvidenceItem item)
        {
            if (selectedIndex < 0 && EvidenceManager.Instance != null)
            {
                selectedIndex = EvidenceManager.Instance.CollectedCount - 1;
            }
        }

        private void HandleEvidenceCleared()
        {
            selectedIndex = -1;
            listScrollPosition = Vector2.zero;
            detailScrollPosition = Vector2.zero;
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

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            overlayTexture = MakeSolid(2, 2, colorBackdropDim);
            windowBgTexture = MakeSolid(2, 2, colorWindowBg);
            innerPanelTexture = MakeSolid(2, 2, colorInnerPanel);
            borderTexture = MakeSolid(2, 2, colorPanelBorder);
            amberAccentTexture = MakeSolid(2, 2, colorAmberAccent);
            selectedBgTexture = MakeSolid(2, 2, colorSelectedBg);
            keyBadgeTexture = MakeSolid(2, 2, new Color(0.13f, 0.17f, 0.23f, 1.0f));

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorTextOffWhite }
            };

            counterBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = colorAmberAccent }
            };

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorAmberAccent }
            };

            tabButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = colorTextMuted }
            };

            activeTabButtonStyle = new GUIStyle(tabButtonStyle)
            {
                normal =
                {
                    background = selectedBgTexture,
                    textColor = colorAmberAccent
                }
            };

            clueButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(12, 10, 8, 8),
                wordWrap = false,
                normal = { textColor = colorTextOffWhite }
            };

            selectedClueButtonStyle = new GUIStyle(clueButtonStyle)
            {
                fontStyle = FontStyle.Bold,
                normal =
                {
                    background = selectedBgTexture,
                    textColor = colorAmberHover
                }
            };

            clueTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = colorTextOffWhite }
            };

            categoryBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = colorAmberAccent }
            };

            idStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = colorTextMuted }
            };

            descriptionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                normal = { textColor = colorTextOffWhite }
            };

            emptyStateStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = colorTextMuted }
            };

            closeButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = colorTextOffWhite }
            };

            hudHintKeyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = colorAmberAccent }
            };

            hudHintTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorTextOffWhite }
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
            InitializeStyles();

            int clueCount = EvidenceManager.Instance != null ? EvidenceManager.Instance.CollectedCount : 0;

            // -------------------- CLOSED HUD HINT --------------------
            if (!isOpen)
            {
                if (showHudHint)
                {
                    float hintW = 200f;
                    float hintH = 34f;
                    Rect hintRect = new Rect(20f, 20f, hintW, hintH);
                    DrawBorderedBox(hintRect, windowBgTexture, borderTexture);

                    // Key badge [J]
                    Rect keyRect = new Rect(26f, 25f, 24f, 24f);
                    DrawBorderedBox(keyRect, keyBadgeTexture, borderTexture);
                    GUI.Label(keyRect, "J", hudHintKeyStyle);

                    // Text
                    Rect textRect = new Rect(58f, 20f, hintW - 65f, hintH);
                    string label = clueCount > 0 ? $"CASE DOSSIER ({clueCount})" : "CASE DOSSIER [J]";
                    GUI.Label(textRect, label, hudHintTextStyle);
                }
                return;
            }

            // -------------------- FULLSCREEN DIM --------------------
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture);

            // Responsive Window Sizing
            float windowWidth = Mathf.Min(880f, Screen.width - 40f);
            float windowHeight = Mathf.Min(560f, Screen.height - 40f);
            float startX = (Screen.width - windowWidth) / 2f;
            float startY = (Screen.height - windowHeight) / 2f;

            Rect windowRect = new Rect(startX, startY, windowWidth, windowHeight);
            DrawBorderedBox(windowRect, windowBgTexture, borderTexture);

            // -------------------- TOP BAR --------------------
            float padding = 18f;

            // Title & Detective Header
            Rect titleRect = new Rect(startX + padding, startY + 12f, windowWidth - 280f, 32f);
            GUI.Label(titleRect, "CASE DOSSIER // FORENSIC EVIDENCE LOG", titleStyle);

            // Clue Count Pill
            Rect countPillRect = new Rect(startX + windowWidth - 240f, startY + 14f, 120f, 26f);
            DrawBorderedBox(countPillRect, innerPanelTexture, borderTexture);
            string countText = clueCount == 1 ? "1 CLUE" : $"{clueCount} CLUES";
            GUI.Label(countPillRect, countText, counterBadgeStyle);

            // Close Button
            Rect closeBtnRect = new Rect(startX + windowWidth - 105f, startY + 14f, 90f, 26f);
            if (GUI.Button(closeBtnRect, "CLOSE [J]", closeButtonStyle))
            {
                CloseJournal();
                return;
            }

            // Divider Line with subtle Amber Accent Dot
            Rect topDividerRect = new Rect(startX + padding, startY + 48f, windowWidth - (padding * 2f), 1f);
            GUI.DrawTexture(topDividerRect, borderTexture);

            // -------------------- COLUMN LAYOUT --------------------
            float contentY = startY + 56f;
            float contentHeight = windowHeight - 74f;
            float leftColumnWidth = 320f;
            float rightColumnWidth = windowWidth - leftColumnWidth - (padding * 3f);
            float leftColumnX = startX + padding;
            float rightColumnX = leftColumnX + leftColumnWidth + padding;

            IReadOnlyList<EvidenceItem> allClues = EvidenceManager.Instance != null
                ? EvidenceManager.Instance.GetCollectedEvidence()
                : new List<EvidenceItem>().AsReadOnly();

            // Filter clues based on category tab
            List<int> visibleIndices = new List<int>();
            for (int i = 0; i < allClues.Count; i++)
            {
                if (selectedCategoryFilter < 0 || (int)allClues[i].Category == selectedCategoryFilter)
                {
                    visibleIndices.Add(i);
                }
            }

            // -------------------- LEFT COLUMN: CATEGORIES & LIST --------------------
            GUI.Label(new Rect(leftColumnX, contentY, leftColumnWidth, 20f), "RECOVERED EVIDENCE", headerStyle);

            // Category Filter Tabs
            float tabY = contentY + 22f;
            string[] tabLabels = { "ALL", "PHYSICAL", "DOC", "DIGITAL" };
            int[] tabValues = { -1, 0, 2, 1 }; // -1=All, 0=Physical, 2=Document, 1=Digital
            float tabWidth = (leftColumnWidth - 9f) / 4f;

            for (int t = 0; t < tabLabels.Length; t++)
            {
                Rect tabRect = new Rect(leftColumnX + (t * (tabWidth + 3f)), tabY, tabWidth, 24f);
                bool isActiveTab = (selectedCategoryFilter == tabValues[t]);
                if (GUI.Button(tabRect, tabLabels[t], isActiveTab ? activeTabButtonStyle : tabButtonStyle))
                {
                    selectedCategoryFilter = tabValues[t];
                    AudioManager.Instance?.PlayButtonClick();
                }
            }

            // Clue List Box
            float listY = tabY + 30f;
            float listHeight = contentHeight - 52f;
            Rect listContainerRect = new Rect(leftColumnX, listY, leftColumnWidth, listHeight);
            DrawBorderedBox(listContainerRect, innerPanelTexture, borderTexture);

            if (allClues.Count == 0)
            {
                Rect emptyRect = new Rect(listContainerRect.x + 16f, listContainerRect.y + 40f, listContainerRect.width - 32f, 130f);
                GUI.Label(emptyRect, "No evidence collected.\n\nExplore Floor 11. Clues emit a subtle amber glow. Approach and press [E] to collect.", emptyStateStyle);
            }
            else if (visibleIndices.Count == 0)
            {
                Rect emptyRect = new Rect(listContainerRect.x + 16f, listContainerRect.y + 40f, listContainerRect.width - 32f, 100f);
                GUI.Label(emptyRect, "No evidence found under this category filter.", emptyStateStyle);
            }
            else
            {
                float itemHeight = 44f;
                float viewHeight = visibleIndices.Count * (itemHeight + 4f);
                Rect scrollViewRect = new Rect(listContainerRect.x + 4f, listContainerRect.y + 4f, listContainerRect.width - 8f, listContainerRect.height - 8f);
                Rect viewRect = new Rect(0f, 0f, scrollViewRect.width - 16f, Mathf.Max(viewHeight, scrollViewRect.height));

                listScrollPosition = GUI.BeginScrollView(scrollViewRect, listScrollPosition, viewRect);

                for (int v = 0; v < visibleIndices.Count; v++)
                {
                    int originalIndex = visibleIndices[v];
                    var clue = allClues[originalIndex];
                    bool isSelected = (originalIndex == selectedIndex);

                    Rect itemBtnRect = new Rect(0f, v * (itemHeight + 4f), viewRect.width, itemHeight);

                    // Accent bar on selected clue
                    if (isSelected)
                    {
                        Rect accentBar = new Rect(itemBtnRect.x, itemBtnRect.y, 4f, itemHeight);
                        GUI.DrawTexture(accentBar, amberAccentTexture);
                    }

                    string itemLabel = $"  [{clue.Category.ToString().ToUpper()}]  {clue.DisplayName}";
                    GUIStyle styleToUse = isSelected ? selectedClueButtonStyle : clueButtonStyle;

                    if (GUI.Button(itemBtnRect, itemLabel, styleToUse))
                    {
                        if (selectedIndex != originalIndex)
                        {
                            selectedIndex = originalIndex;
                            AudioManager.Instance?.PlayButtonClick();
                        }
                    }
                }

                GUI.EndScrollView();
            }

            // -------------------- RIGHT COLUMN: FORENSIC DETAILS --------------------
            GUI.Label(new Rect(rightColumnX, contentY, rightColumnWidth, 20f), "FORENSIC DOSSIER & ANALYSIS", headerStyle);

            Rect detailContainerRect = new Rect(rightColumnX, contentY + 22f, rightColumnWidth, contentHeight - 22f);
            DrawBorderedBox(detailContainerRect, innerPanelTexture, borderTexture);

            if (selectedIndex >= 0 && selectedIndex < allClues.Count)
            {
                var selectedClue = allClues[selectedIndex];
                float innerPad = 18f;
                float curY = detailContainerRect.y + innerPad;
                float innerWidth = detailContainerRect.width - (innerPad * 2f);

                // ID Stamp
                string idStamp = $"EVIDENCE RECORD // ID: {selectedClue.EvidenceId.ToUpper()}";
                GUI.Label(new Rect(detailContainerRect.x + innerPad, curY, innerWidth, 18f), idStamp, idStyle);
                curY += 20f;

                // Title
                GUI.Label(new Rect(detailContainerRect.x + innerPad, curY, innerWidth, 26f), selectedClue.DisplayName, clueTitleStyle);
                curY += 28f;

                // Category Badge
                string metaLine = $"CLASSIFICATION: {selectedClue.Category.ToString().ToUpper()} EVIDENCE";
                GUI.Label(new Rect(detailContainerRect.x + innerPad, curY, innerWidth, 20f), metaLine, categoryBadgeStyle);
                curY += 22f;

                // Divider
                GUI.DrawTexture(new Rect(detailContainerRect.x + innerPad, curY, innerWidth, 1f), borderTexture);
                curY += 14f;

                // Description Box (Scrollable for extensive forensic notes)
                float descHeight = detailContainerRect.yMax - curY - innerPad;
                Rect descOuterRect = new Rect(detailContainerRect.x + innerPad, curY, innerWidth, descHeight);
                Rect descViewRect = new Rect(0f, 0f, innerWidth - 18f, Mathf.Max(260f, descHeight));

                detailScrollPosition = GUI.BeginScrollView(descOuterRect, detailScrollPosition, descViewRect);

                if (selectedClue.Icon != null && selectedClue.Icon.texture != null)
                {
                    float iconSize = 64f;
                    GUI.DrawTexture(new Rect(0f, 0f, iconSize, iconSize), selectedClue.Icon.texture, ScaleMode.ScaleToFit);
                    Rect descWithIconRect = new Rect(iconSize + 12f, 0f, descViewRect.width - iconSize - 12f, descViewRect.height);
                    GUI.Label(descWithIconRect, selectedClue.Description, descriptionStyle);
                }
                else
                {
                    GUI.Label(new Rect(0f, 0f, descViewRect.width, descViewRect.height), selectedClue.Description, descriptionStyle);
                }

                GUI.EndScrollView();
            }
            else
            {
                Rect promptRect = new Rect(detailContainerRect.x + 20f, detailContainerRect.y + (detailContainerRect.height / 2f) - 30f, detailContainerRect.width - 40f, 60f);
                GUI.Label(promptRect, "Select a clue from the list on the left to examine detailed forensic analysis and recovered metadata.", emptyStateStyle);
            }
        }
    }
}
