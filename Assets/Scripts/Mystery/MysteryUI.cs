using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MurderMystery.Audio;

namespace MurderMystery
{
    /// <summary>
    /// Person 4 Redesigned Mystery UI:
    /// High-contrast cinematic detective interface providing polished presentation for:
    /// - Investigation Objectives HUD (top-left docked directive panel)
    /// - Suspect Dialogue & Interrogation Panel (speaker badges, transcript, inquiry & evidence presentation)
    /// - Formal Accusation Modal (suspect dossier select, prerequisite indicator, evidence submission)
    /// - Case Outcome / Resolution Screen (Case Solved, Case Inconclusive, Case Failed)
    /// Completely preserves Person 3's underlying mystery logic, rules, and progression.
    /// </summary>
    public sealed class MysteryUI : MonoBehaviour
    {
        private MysteryManager mysteryManager;
        private EvidenceManager evidenceManager;
        private EvidenceJournalUI evidenceJournal;
        private FirstPersonController playerController;
        private SuspectId activeSuspect;
        private SuspectId selectedAccusationSuspect = SuspectId.VikramDesai;
        private readonly List<string> selectedEvidence = new List<string>();
        private string currentDialogue = string.Empty;
        private bool dialogueOpen;
        private bool accusationOpen;
        private bool previousCursorVisible;
        private CursorLockMode previousCursorLockState;
        private Vector2 dialogueScroll;
        private Vector2 accusationScroll;
        private Vector2 endingScroll;
        private bool previousJournalEnabled;
        private bool previousPlayerControllerEnabled;

        // Visual Palette (Cinematic Detective)
        private readonly Color colorBackdropDim = new Color(0.04f, 0.05f, 0.08f, 0.90f);
        private readonly Color colorWindowBg = new Color(0.07f, 0.09f, 0.12f, 0.98f);       // #12161E
        private readonly Color colorInnerPanel = new Color(0.10f, 0.13f, 0.18f, 1.0f);      // #1A212E
        private readonly Color colorPanelBorder = new Color(0.18f, 0.23f, 0.31f, 1.0f);     // #2E3B4F
        private readonly Color colorAmber = new Color(0.96f, 0.67f, 0.18f, 1.0f);          // #F5AB2E
        private readonly Color colorAmberHover = new Color(1.0f, 0.78f, 0.35f, 1.0f);
        private readonly Color colorGreenSuccess = new Color(0.20f, 0.75f, 0.42f, 1.0f);
        private readonly Color colorRedWarning = new Color(0.85f, 0.22f, 0.22f, 1.0f);      // #DC3545
        private readonly Color colorTextOffWhite = new Color(0.95f, 0.96f, 0.98f, 1.0f);    // #F1F5F9
        private readonly Color colorTextMuted = new Color(0.65f, 0.72f, 0.82f, 1.0f);       // #A6B8D1

        // Cached GUI Textures
        private Texture2D dimOverlayTex;
        private Texture2D windowBgTex;
        private Texture2D innerPanelTex;
        private Texture2D borderTex;
        private Texture2D amberTex;
        private Texture2D buttonBgTex;
        private Texture2D buttonSelectedTex;
        private Texture2D headerBannerTex;

        // Cached GUI Styles
        private GUIStyle windowTitleStyle;
        private GUIStyle headerBadgeStyle;
        private GUIStyle bodyTextStyle;
        private GUIStyle dialogueSpeakerStyle;
        private GUIStyle dialogueTranscriptStyle;
        private GUIStyle actionButtonStyle;
        private GUIStyle activeToggleButtonStyle;
        private GUIStyle normalToggleButtonStyle;
        private GUIStyle closeButtonStyle;
        private GUIStyle objectiveHeaderStyle;
        private GUIStyle objectiveItemStyle;
        private GUIStyle hintBadgeStyle;
        private bool stylesInitialized = false;

        public void Initialize(
            MysteryManager manager,
            EvidenceManager evidence,
            EvidenceJournalUI journal,
            FirstPersonController controller)
        {
            mysteryManager = manager;
            evidenceManager = evidence;
            evidenceJournal = journal;
            playerController = controller;
        }

        public void OpenDialogue(SuspectId suspect)
        {
            if (mysteryManager == null)
            {
                Debug.LogError("MysteryUI has no MysteryManager.", this);
                return;
            }

            if (mysteryManager.CaseState == MysteryCaseState.Failed ||
                mysteryManager.CaseState == MysteryCaseState.Solved)
            {
                return;
            }

            activeSuspect = suspect;
            currentDialogue = mysteryManager.BeginInterview(suspect);
            dialogueOpen = true;
            accusationOpen = false;
            BeginModal();
            AudioManager.Instance?.PlayDialogueTick();
        }

        public void OpenAccusation()
        {
            if (mysteryManager == null)
            {
                Debug.LogError("MysteryUI has no MysteryManager.", this);
                return;
            }

            if (!mysteryManager.CanOpenAccusation)
            {
                return;
            }

            selectedEvidence.Clear();
            accusationOpen = true;
            dialogueOpen = false;
            BeginModal();
            AudioManager.Instance?.PlayJournalOpen();
        }

        private void Update()
        {
            if (mysteryManager == null)
                return;

            if ((dialogueOpen || accusationOpen) &&
                Input.GetKeyDown(KeyCode.Escape))
            {
                CloseModal();
                return;
            }

            if (dialogueOpen || accusationOpen)
                return;

            if (Input.GetKeyDown(KeyCode.T) &&
                mysteryManager.CanOpenAccusation)
            {
                OpenAccusation();
            }
        }

        private void OnDestroy()
        {
            DestroyTex(ref dimOverlayTex);
            DestroyTex(ref windowBgTex);
            DestroyTex(ref innerPanelTex);
            DestroyTex(ref borderTex);
            DestroyTex(ref amberTex);
            DestroyTex(ref buttonBgTex);
            DestroyTex(ref buttonSelectedTex);
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

            dimOverlayTex = MakeSolid(2, 2, colorBackdropDim);
            windowBgTex = MakeSolid(2, 2, colorWindowBg);
            innerPanelTex = MakeSolid(2, 2, colorInnerPanel);
            borderTex = MakeSolid(2, 2, colorPanelBorder);
            amberTex = MakeSolid(2, 2, colorAmber);
            buttonBgTex = MakeSolid(2, 2, new Color(0.12f, 0.16f, 0.22f, 1.0f));
            buttonSelectedTex = MakeSolid(2, 2, new Color(0.22f, 0.32f, 0.46f, 1.0f));

            windowTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorTextOffWhite }
            };

            headerBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorAmber }
            };

            dialogueSpeakerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorAmber }
            };

            dialogueTranscriptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = colorTextOffWhite }
            };

            bodyTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                normal = { textColor = colorTextOffWhite }
            };

            actionButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(12, 10, 6, 6),
                normal = { textColor = colorTextOffWhite }
            };

            normalToggleButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(12, 10, 6, 6),
                normal = { textColor = colorTextOffWhite }
            };

            activeToggleButtonStyle = new GUIStyle(normalToggleButtonStyle)
            {
                fontStyle = FontStyle.Bold,
                normal =
                {
                    background = buttonSelectedTex,
                    textColor = colorAmberHover
                }
            };

            closeButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = colorTextOffWhite }
            };

            objectiveHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorAmber }
            };

            objectiveItemStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorTextOffWhite }
            };

            hintBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorAmber }
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
            if (mysteryManager == null)
                return;

            InitStyles();

            DrawObjectivePanel();

            // Accusation Location Hint
            if (mysteryManager.CanAccuse &&
                mysteryManager.CaseState != MysteryCaseState.Failed &&
                mysteryManager.CaseState != MysteryCaseState.Solved &&
                !dialogueOpen && !accusationOpen)
            {
                float hintW = 390f;
                float hintH = 34f;
                Rect hintRect = new Rect(16f, 310f, hintW, hintH);
                DrawBorderedBox(hintRect, innerPanelTex, borderTex);

                string hint = mysteryManager.IsInAccusationLocation
                    ? "  [T] MAKE FORMAL ACCUSATION"
                    : "  Accusations can be made in Foyer or Meeting Room";
                GUI.Label(hintRect, hint, hintBadgeStyle);
            }

            if (mysteryManager.CaseState == MysteryCaseState.Failed ||
                mysteryManager.CaseState == MysteryCaseState.Solved)
            {
                DrawEnding();
            }
            else if (dialogueOpen)
            {
                DrawDialogue();
            }
            else if (accusationOpen)
            {
                DrawAccusation();
            }
        }

        private void DrawObjectivePanel()
        {
            if (dialogueOpen || accusationOpen)
                return;

            float panelWidth = 375f;
            IReadOnlyList<string> objectiveIds = mysteryManager.GetObjectiveIds();
            float panelHeight = 44f + (objectiveIds.Count * 23f) + 36f;
            Rect panelRect = new Rect(16f, 16f, panelWidth, panelHeight);

            DrawBorderedBox(panelRect, windowBgTex, borderTex);

            // Left vertical accent bar
            GUI.DrawTexture(new Rect(panelRect.x + 1f, panelRect.y + 1f, 3f, panelHeight - 2f), amberTex);

            // Header
            GUI.Label(new Rect(panelRect.x + 14f, panelRect.y + 8f, panelWidth - 28f, 20f), "INVESTIGATION DIRECTIVES", objectiveHeaderStyle);
            GUI.DrawTexture(new Rect(panelRect.x + 14f, panelRect.y + 28f, panelWidth - 28f, 1f), borderTex);

            // Items
            float curY = panelRect.y + 34f;
            for (int i = 0; i < objectiveIds.Count; i++)
            {
                string objectiveId = objectiveIds[i];
                ObjectiveStatus status = mysteryManager.GetObjectiveStatus(objectiveId);

                string marker = status == ObjectiveStatus.Complete
                    ? "[✓]"
                    : status == ObjectiveStatus.Locked ? "[-]" : "[ ]";

                Color itemColor = status == ObjectiveStatus.Complete
                    ? colorAmber
                    : status == ObjectiveStatus.Locked ? colorTextMuted : colorTextOffWhite;

                objectiveItemStyle.normal.textColor = itemColor;
                GUI.Label(
                    new Rect(panelRect.x + 14f, curY, panelWidth - 28f, 20f),
                    $"{marker} {objectiveId}  {mysteryManager.GetObjectiveText(objectiveId)}",
                    objectiveItemStyle);

                curY += 23f;
            }

            // Footer
            GUI.DrawTexture(new Rect(panelRect.x + 14f, curY + 2f, panelWidth - 28f, 1f), borderTex);
            string footerText = $"Clues: {mysteryManager.KnownEvidenceCount}/6   |   [J] Dossier   |   [T] Accusation";
            GUI.Label(new Rect(panelRect.x + 14f, curY + 6f, panelWidth - 28f, 22f), footerText, headerBadgeStyle);
        }

        private void DrawDialogue()
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimOverlayTex);

            Rect rect = CenteredRect(740f, 580f);
            DrawBorderedBox(rect, windowBgTex, borderTex);

            float padding = 20f;
            float contentW = rect.width - (padding * 2f);

            // Speaker Tag Header
            Rect speakerRect = new Rect(rect.x + padding, rect.y + 14f, contentW - 100f, 26f);
            string speakerTitle = MysterySuspectInteractable.GetDisplayName(activeSuspect).ToUpper();
            GUI.Label(speakerRect, $"INTERROGATION // {speakerTitle}", dialogueSpeakerStyle);

            // Close Button
            Rect closeBtnRect = new Rect(rect.xMax - padding - 85f, rect.y + 14f, 85f, 26f);
            if (GUI.Button(closeBtnRect, "CLOSE", closeButtonStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                CloseModal();
                return;
            }

            GUI.DrawTexture(new Rect(rect.x + padding, rect.y + 44f, contentW, 1f), borderTex);

            // Transcript Box
            float transcriptY = rect.y + 52f;
            float transcriptH = 130f;
            Rect transcriptBoxRect = new Rect(rect.x + padding, transcriptY, contentW, transcriptH);
            DrawBorderedBox(transcriptBoxRect, innerPanelTex, borderTex);

            Rect scrollOuter = new Rect(transcriptBoxRect.x + 4f, transcriptBoxRect.y + 4f, transcriptBoxRect.width - 8f, transcriptBoxRect.height - 8f);
            Rect scrollView = new Rect(0f, 0f, scrollOuter.width - 16f, Mathf.Max(120f, dialogueTranscriptStyle.CalcHeight(new GUIContent(currentDialogue), scrollOuter.width - 20f)));
            dialogueScroll = GUI.BeginScrollView(scrollOuter, dialogueScroll, scrollView);
            GUI.Label(new Rect(8f, 4f, scrollView.width - 8f, scrollView.height), currentDialogue, dialogueTranscriptStyle);
            GUI.EndScrollView();

            // Questions Section
            float questionY = transcriptY + transcriptH + 12f;
            GUI.Label(new Rect(rect.x + padding, questionY, contentW, 20f), "INQUIRIES & QUESTIONS", headerBadgeStyle);

            IReadOnlyList<string> questions = mysteryManager.GetQuestionPrompts(activeSuspect);
            float qBtnY = questionY + 22f;
            for (int i = 0; i < questions.Count; i++)
            {
                int questionIndex = i;
                Rect qBtnRect = new Rect(rect.x + padding, qBtnY, contentW, 30f);
                if (GUI.Button(qBtnRect, $"  [ ? ]  {questions[i]}", actionButtonStyle))
                {
                    AudioManager.Instance?.PlayButtonClick();
                    currentDialogue = mysteryManager.AskQuestion(activeSuspect, questionIndex);
                }
                qBtnY += 33f;
            }

            // Present Evidence Section
            float evidenceY = qBtnY + 8f;
            GUI.Label(new Rect(rect.x + padding, evidenceY, contentW, 20f), "PRESENT COLLECTED EVIDENCE", headerBadgeStyle);

            IReadOnlyList<EvidenceItem> presentable = mysteryManager.GetPresentableEvidence(activeSuspect);
            float eBtnY = evidenceY + 22f;
            if (presentable.Count == 0)
            {
                GUI.Label(new Rect(rect.x + padding + 8f, eBtnY, contentW, 24f), "No relevant evidence for this suspect has been discovered yet.", bodyTextStyle);
            }
            else
            {
                for (int i = 0; i < presentable.Count; i++)
                {
                    EvidenceItem item = presentable[i];
                    bool alreadyPresented = mysteryManager.IsEvidencePresentedTo(activeSuspect, item.EvidenceId);
                    string status = alreadyPresented ? "REVIEW" : "CONFRONT";
                    Rect eBtnRect = new Rect(rect.x + padding, eBtnY, contentW, 30f);

                    if (GUI.Button(eBtnRect, $"  [ {status} ]  {item.DisplayName} [{item.EvidenceId}]", actionButtonStyle))
                    {
                        AudioManager.Instance?.PlayButtonClick();
                        currentDialogue = mysteryManager.PresentEvidence(activeSuspect, item.EvidenceId);
                    }
                    eBtnY += 33f;
                }
            }
        }

        private void DrawAccusation()
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimOverlayTex);

            Rect rect = CenteredRect(740f, 600f);
            DrawBorderedBox(rect, windowBgTex, borderTex);

            float padding = 20f;
            float contentW = rect.width - (padding * 2f);

            // Title
            GUI.Label(new Rect(rect.x + padding, rect.y + 14f, contentW - 100f, 26f), "FORMAL ACCUSATION DOSSIER", windowTitleStyle);

            Rect closeBtnRect = new Rect(rect.xMax - padding - 85f, rect.y + 14f, 85f, 26f);
            if (GUI.Button(closeBtnRect, "CANCEL", closeButtonStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                CloseModal();
                return;
            }

            GUI.DrawTexture(new Rect(rect.x + padding, rect.y + 44f, contentW, 1f), borderTex);

            // Suspect Selection
            float curY = rect.y + 52f;
            GUI.Label(new Rect(rect.x + padding, curY, contentW, 20f), "SELECT PRIMARY SUSPECT", headerBadgeStyle);
            curY += 22f;

            float suspectBtnW = (contentW - 9f) / 4f;
            SuspectId[] suspects = GetSuspects();
            for (int s = 0; s < suspects.Length; s++)
            {
                SuspectId sus = suspects[s];
                bool isSelected = (selectedAccusationSuspect == sus);
                Rect sBtnRect = new Rect(rect.x + padding + (s * (suspectBtnW + 3f)), curY, suspectBtnW, 32f);

                if (GUI.Button(sBtnRect, MysterySuspectInteractable.GetDisplayName(sus), isSelected ? activeToggleButtonStyle : normalToggleButtonStyle))
                {
                    selectedAccusationSuspect = sus;
                    AudioManager.Instance?.PlayButtonClick();
                }
            }
            curY += 40f;

            // Evidence Checklist Section
            GUI.Label(new Rect(rect.x + padding, curY, contentW, 20f), "SUBMIT DECISIVE FORENSIC EVIDENCE", headerBadgeStyle);
            curY += 22f;

            float listHeight = 160f;
            Rect listContainer = new Rect(rect.x + padding, curY, contentW, listHeight);
            DrawBorderedBox(listContainer, innerPanelTex, borderTex);

            if (evidenceManager != null)
            {
                var collected = evidenceManager.GetCollectedEvidence();
                float viewHeight = Mathf.Max(listHeight, collected.Count * 34f);
                Rect scrollOuter = new Rect(listContainer.x + 4f, listContainer.y + 4f, listContainer.width - 8f, listContainer.height - 8f);
                Rect scrollView = new Rect(0f, 0f, scrollOuter.width - 16f, viewHeight);

                accusationScroll = GUI.BeginScrollView(scrollOuter, accusationScroll, scrollView);

                for (int e = 0; e < collected.Count; e++)
                {
                    EvidenceItem item = collected[e];
                    bool isChecked = selectedEvidence.Contains(item.EvidenceId);
                    Rect eRect = new Rect(0f, e * 34f, scrollView.width, 30f);

                    string checkLabel = isChecked ? $"  [✓]  {item.DisplayName} [{item.EvidenceId}]" : $"  [ ]  {item.DisplayName} [{item.EvidenceId}]";
                    if (GUI.Button(eRect, checkLabel, isChecked ? activeToggleButtonStyle : normalToggleButtonStyle))
                    {
                        if (isChecked)
                            selectedEvidence.Remove(item.EvidenceId);
                        else
                            selectedEvidence.Add(item.EvidenceId);

                        AudioManager.Instance?.PlayButtonClick();
                    }
                }

                GUI.EndScrollView();
            }
            curY += listHeight + 10f;

            // Inconclusive feedback message if any
            if (!string.IsNullOrEmpty(mysteryManager.CaseMessage) &&
                mysteryManager.CaseState == MysteryCaseState.Inconclusive)
            {
                Rect msgRect = new Rect(rect.x + padding, curY, contentW, 48f);
                DrawBorderedBox(msgRect, innerPanelTex, borderTex);
                GUI.Label(new Rect(msgRect.x + 8f, msgRect.y + 4f, msgRect.width - 16f, msgRect.height - 8f), mysteryManager.CaseMessage, bodyTextStyle);
                curY += 54f;
            }

            // Bottom Buttons
            float btmBtnY = rect.yMax - padding - 36f;
            Rect submitBtnRect = new Rect(rect.xMax - padding - 220f, btmBtnY, 220f, 36f);
            if (GUI.Button(submitBtnRect, "SUBMIT ACCUSATION", actionButtonStyle))
            {
                SubmitAccusation();
            }
        }

        private void DrawEnding()
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimOverlayTex);

            Rect rect = CenteredRect(780f, 520f);
            DrawBorderedBox(rect, windowBgTex, borderTex);

            bool isSolved = (mysteryManager.CaseState == MysteryCaseState.Solved);
            Color bannerColor = isSolved ? new Color(0.12f, 0.42f, 0.22f, 1.0f) : new Color(0.48f, 0.14f, 0.14f, 1.0f);

            Rect bannerRect = new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, 44f);
            GUI.DrawTexture(bannerRect, MakeSolid(2, 2, bannerColor));

            string bannerText = isSolved
                ? "CASE SOLVED // INVESTIGATION SUCCESSFUL"
                : "CASE FAILED // INVESTIGATION CLOSED WITH UNRESOLVED INQUIRY";
            GUI.Label(bannerRect, bannerText, windowTitleStyle);

            float padding = 24f;
            float contentY = rect.y + 58f;
            float contentW = rect.width - (padding * 2f);
            float scrollH = rect.height - 130f;

            Rect scrollOuter = new Rect(rect.x + padding, contentY, contentW, scrollH);
            Rect scrollView = new Rect(0f, 0f, contentW - 16f, Mathf.Max(scrollH, dialogueTranscriptStyle.CalcHeight(new GUIContent(mysteryManager.CaseMessage), contentW - 20f)));

            endingScroll = GUI.BeginScrollView(scrollOuter, endingScroll, scrollView);
            GUI.Label(new Rect(0f, 0f, scrollView.width, scrollView.height), mysteryManager.CaseMessage, dialogueTranscriptStyle);
            GUI.EndScrollView();

            // Restart Button
            Rect restartBtnRect = new Rect(rect.xMax - padding - 180f, rect.yMax - padding - 32f, 180f, 32f);
            if (GUI.Button(restartBtnRect, "RESTART CASE", actionButtonStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void SubmitAccusation()
        {
            AccusationResult result = mysteryManager.TryAccuse(
                selectedAccusationSuspect,
                selectedEvidence.AsReadOnly());

            if (result == AccusationResult.Solved ||
                result == AccusationResult.WrongSuspect)
            {
                dialogueOpen = false;
                accusationOpen = false;
                EndModal();

                if (result == AccusationResult.Solved)
                    AudioManager.Instance?.PlayCaseSolved();
                else
                    AudioManager.Instance?.PlayCaseFailed();
            }
            else if (result == AccusationResult.Inconclusive)
            {
                AudioManager.Instance?.PlayCaseInconclusive();
            }
            else if (result == AccusationResult.InvalidEvidence)
            {
                Debug.LogWarning(
                    "Accusation rejected because submitted evidence was not collected.",
                    this);
            }
            else if (result == AccusationResult.Locked)
            {
                Debug.LogWarning(
                    "Accusation requires all initial interviews, four canonical " +
                    "evidence items, and the Foyer or Meeting Room.",
                    this);
            }
        }

        private void CloseModal()
        {
            dialogueOpen = false;
            accusationOpen = false;
            EndModal();
        }

        private void BeginModal()
        {
            previousCursorLockState = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            previousJournalEnabled =
                evidenceJournal != null && evidenceJournal.enabled;
            previousPlayerControllerEnabled =
                playerController != null && playerController.enabled;
            if (evidenceJournal != null)
                evidenceJournal.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (playerController != null)
                playerController.enabled = false;
        }

        private void EndModal()
        {
            Cursor.lockState = previousCursorLockState;
            Cursor.visible = previousCursorVisible;
            if (evidenceJournal != null)
                evidenceJournal.enabled = previousJournalEnabled;
            if (playerController != null)
                playerController.enabled = previousPlayerControllerEnabled;
        }

        private void OnDisable()
        {
            if (dialogueOpen || accusationOpen)
                EndModal();

            dialogueOpen = false;
            accusationOpen = false;
        }

        private static Rect CenteredRect(float width, float height)
        {
            width = Mathf.Min(width, Screen.width - 32f);
            height = Mathf.Min(height, Screen.height - 32f);
            return new Rect(
                (Screen.width - width) / 2f,
                (Screen.height - height) / 2f,
                width,
                height);
        }

        private static SuspectId[] GetSuspects()
        {
            return new[]
            {
                SuspectId.RohanKapoor,
                SuspectId.PriyaSharma,
                SuspectId.VikramDesai,
                SuspectId.KarthikNair
            };
        }
    }
}
