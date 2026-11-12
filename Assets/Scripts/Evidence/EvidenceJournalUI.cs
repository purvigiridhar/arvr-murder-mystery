using System.Collections.Generic;
using UnityEngine;

namespace MurderMystery
{
    /// <summary>
    /// Immediate-mode GUI Evidence Journal allowing the player to inspect collected clues.
    /// Operates completely independently of InteractionPromptUI and requires zero external UI assets.
    /// Can be toggled with KeyCode.J and manages mouse cursor locking during inspection.
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
        private Vector2 listScrollPosition = Vector2.zero;
        private Vector2 detailScrollPosition = Vector2.zero;

        // Cached GUI textures and styles
        private Texture2D overlayTexture;
        private Texture2D panelBackgroundTexture;
        private Texture2D selectionHighlightTexture;
        private Texture2D dividerTexture;

        private GUIStyle titleStyle;
        private GUIStyle headerStyle;
        private GUIStyle clueButtonStyle;
        private GUIStyle selectedClueButtonStyle;
        private GUIStyle clueTitleStyle;
        private GUIStyle categoryBadgeStyle;
        private GUIStyle idStyle;
        private GUIStyle descriptionStyle;
        private GUIStyle emptyStateStyle;
        private GUIStyle closeButtonStyle;
        private GUIStyle hudHintStyle;

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

            DestroyTexture(ref overlayTexture);
            DestroyTexture(ref panelBackgroundTexture);
            DestroyTexture(ref selectionHighlightTexture);
            DestroyTexture(ref dividerTexture);
        }

        private void DestroyTexture(ref Texture2D tex)
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
        }

        private void HandleEvidenceCollected(EvidenceItem item)
        {
            // If nothing was selected or only 1 item now exists, auto-select the latest
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

        private Texture2D MakeSolidTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            overlayTexture = MakeSolidTexture(2, 2, new Color(0.05f, 0.05f, 0.08f, 0.75f));
            panelBackgroundTexture = MakeSolidTexture(2, 2, new Color(0.12f, 0.14f, 0.18f, 0.95f));
            selectionHighlightTexture = MakeSolidTexture(2, 2, new Color(0.24f, 0.35f, 0.52f, 0.9f));
            dividerTexture = MakeSolidTexture(2, 2, new Color(0.3f, 0.35f, 0.42f, 0.8f));

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.95f, 0.85f, 0.45f) }
            };

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.75f, 0.8f, 0.9f) }
            };

            clueButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 8, 8),
                normal = { textColor = Color.white }
            };

            selectedClueButtonStyle = new GUIStyle(clueButtonStyle)
            {
                fontStyle = FontStyle.Bold,
                normal =
                {
                    background = selectionHighlightTexture,
                    textColor = new Color(1f, 0.95f, 0.6f)
                }
            };

            clueTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            categoryBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Italic,
                normal = { textColor = new Color(0.4f, 0.8f, 1f) }
            };

            idStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };

            descriptionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.92f, 0.95f) }
            };

            emptyStateStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.7f, 0.75f, 0.8f) }
            };

            closeButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            hudHintStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.9f, 0.9f, 0.9f) }
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitializeStyles();

            int clueCount = EvidenceManager.Instance != null ? EvidenceManager.Instance.CollectedCount : 0;

            // When closed: optionally draw non-intrusive HUD reminder
            if (!isOpen)
            {
                if (showHudHint)
                {
                    string hintText = $" [J] Evidence Journal ({clueCount})";
                    GUI.Box(new Rect(15f, 15f, 180f, 28f), hintText, hudHintStyle);
                }
                return;
            }

            // Dim background overlay
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture);

            // Journal Window Dimensions (Responsive)
            float windowWidth = Mathf.Min(820f, Screen.width - 40f);
            float windowHeight = Mathf.Min(520f, Screen.height - 40f);
            float startX = (Screen.width - windowWidth) / 2f;
            float startY = (Screen.height - windowHeight) / 2f;

            Rect windowRect = new Rect(startX, startY, windowWidth, windowHeight);
            GUI.DrawTexture(windowRect, panelBackgroundTexture);
            GUI.Box(windowRect, GUIContent.none);

            // Top Bar
            float padding = 16f;
            Rect titleRect = new Rect(startX + padding, startY + 12f, windowWidth - 160f, 32f);
            GUI.Label(titleRect, $"CASE DOSSIER // EVIDENCE JOURNAL  ({clueCount} Clues)", titleStyle);

            Rect closeBtnRect = new Rect(startX + windowWidth - 110f, startY + 12f, 95f, 28f);
            if (GUI.Button(closeBtnRect, "Close [J]", closeButtonStyle))
            {
                CloseJournal();
                return;
            }

            // Divider Line
            Rect topDividerRect = new Rect(startX + padding, startY + 48f, windowWidth - (padding * 2f), 2f);
            GUI.DrawTexture(topDividerRect, dividerTexture);

            // Column Layout
            float contentY = startY + 56f;
            float contentHeight = windowHeight - 72f;
            float leftColumnWidth = 280f;
            float rightColumnWidth = windowWidth - leftColumnWidth - (padding * 3f);
            float leftColumnX = startX + padding;
            float rightColumnX = leftColumnX + leftColumnWidth + padding;

            IReadOnlyList<EvidenceItem> clues = EvidenceManager.Instance != null
                ? EvidenceManager.Instance.GetCollectedEvidence()
                : new List<EvidenceItem>().AsReadOnly();

            // -------------------- LEFT COLUMN: CLUE LIST --------------------
            GUI.Label(new Rect(leftColumnX, contentY, leftColumnWidth, 22f), "COLLECTED EVIDENCE", headerStyle);

            Rect listContainerRect = new Rect(leftColumnX, contentY + 26f, leftColumnWidth, contentHeight - 26f);
            GUI.Box(listContainerRect, GUIContent.none);

            if (clues.Count == 0)
            {
                Rect emptyRect = new Rect(listContainerRect.x + 15f, listContainerRect.y + 40f, listContainerRect.width - 30f, 120f);
                GUI.Label(emptyRect, "No evidence collected yet.\n\nInvestigate the crime scene and press [E] to collect physical and digital clues.", emptyStateStyle);
            }
            else
            {
                float itemHeight = 44f;
                float viewHeight = clues.Count * (itemHeight + 4f);
                Rect scrollViewRect = new Rect(listContainerRect.x + 4f, listContainerRect.y + 4f, listContainerRect.width - 8f, listContainerRect.height - 8f);
                Rect viewRect = new Rect(0f, 0f, scrollViewRect.width - 18f, Mathf.Max(viewHeight, scrollViewRect.height));

                listScrollPosition = GUI.BeginScrollView(scrollViewRect, listScrollPosition, viewRect);

                for (int i = 0; i < clues.Count; i++)
                {
                    var clue = clues[i];
                    bool isSelected = (i == selectedIndex);
                    Rect itemBtnRect = new Rect(0f, i * (itemHeight + 4f), viewRect.width, itemHeight);

                    string itemLabel = $"[{clue.Category}] {clue.DisplayName}";
                    GUIStyle styleToUse = isSelected ? selectedClueButtonStyle : clueButtonStyle;

                    if (GUI.Button(itemBtnRect, itemLabel, styleToUse))
                    {
                        selectedIndex = i;
                    }
                }

                GUI.EndScrollView();
            }

            // -------------------- RIGHT COLUMN: CLUE DETAILS --------------------
            GUI.Label(new Rect(rightColumnX, contentY, rightColumnWidth, 22f), "CLUE DOSSIER & ANALYSIS", headerStyle);

            Rect detailContainerRect = new Rect(rightColumnX, contentY + 26f, rightColumnWidth, contentHeight - 26f);
            GUI.Box(detailContainerRect, GUIContent.none);

            if (selectedIndex >= 0 && selectedIndex < clues.Count)
            {
                var selectedClue = clues[selectedIndex];
                float innerPadding = 16f;
                float curY = detailContainerRect.y + innerPadding;
                float innerWidth = detailContainerRect.width - (innerPadding * 2f);

                // Display Name
                GUI.Label(new Rect(detailContainerRect.x + innerPadding, curY, innerWidth, 26f), selectedClue.DisplayName, clueTitleStyle);
                curY += 28f;

                // Category and Evidence ID
                string metaLine = $"Category: {selectedClue.Category} Evidence   |   ID: {selectedClue.EvidenceId}";
                GUI.Label(new Rect(detailContainerRect.x + innerPadding, curY, innerWidth, 20f), metaLine, categoryBadgeStyle);
                curY += 24f;

                // Inner divider
                GUI.DrawTexture(new Rect(detailContainerRect.x + innerPadding, curY, innerWidth, 1f), dividerTexture);
                curY += 10f;

                // Clue Icon Preview (if assigned)
                if (selectedClue.Icon != null && selectedClue.Icon.texture != null)
                {
                    float iconSize = 64f;
                    GUI.DrawTexture(new Rect(detailContainerRect.x + innerPadding, curY, iconSize, iconSize), selectedClue.Icon.texture, ScaleMode.ScaleToFit);
                    // Indent description area next to icon
                    Rect descWithIconRect = new Rect(detailContainerRect.x + innerPadding + iconSize + 12f, curY, innerWidth - iconSize - 12f, detailContainerRect.yMax - curY - innerPadding);
                    GUI.Label(descWithIconRect, selectedClue.Description, descriptionStyle);
                }
                else
                {
                    // Scrollable description for long forensic notes
                    float descHeight = detailContainerRect.yMax - curY - innerPadding;
                    Rect descOuterRect = new Rect(detailContainerRect.x + innerPadding, curY, innerWidth, descHeight);
                    Rect descViewRect = new Rect(0f, 0f, innerWidth - 18f, Mathf.Max(250f, descHeight));

                    detailScrollPosition = GUI.BeginScrollView(descOuterRect, detailScrollPosition, descViewRect);
                    GUI.Label(new Rect(0f, 0f, descViewRect.width, descViewRect.height), selectedClue.Description, descriptionStyle);
                    GUI.EndScrollView();
                }
            }
            else
            {
                Rect promptRect = new Rect(detailContainerRect.x + 20f, detailContainerRect.y + (detailContainerRect.height / 2f) - 30f, detailContainerRect.width - 40f, 60f);
                GUI.Label(promptRect, "Select a clue from the list on the left to examine forensic analysis and detailed evidence records.", emptyStateStyle);
            }
        }
    }
}
