using System.Collections.Generic;
using UnityEngine;

namespace MurderMystery
{
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

        private void OnGUI()
        {
            if (mysteryManager == null)
                return;

            DrawObjectivePanel();

            if (mysteryManager.CanAccuse &&
                mysteryManager.CaseState != MysteryCaseState.Failed &&
                mysteryManager.CaseState != MysteryCaseState.Solved)
            {
                string hint = mysteryManager.IsInAccusationLocation
                    ? "Press [T] to make a formal accusation."
                    : "Formal accusations can be made in the Foyer or Meeting Room.";
                GUI.Label(new Rect(16f, 290f, 440f, 28f), hint);
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
            Rect panelRect = new Rect(16f, 16f, 365f, 258f);
            GUI.Box(panelRect, "INVESTIGATION OBJECTIVES");

            IReadOnlyList<string> objectiveIds = mysteryManager.GetObjectiveIds();
            for (int i = 0; i < objectiveIds.Count; i++)
            {
                string objectiveId = objectiveIds[i];
                ObjectiveStatus status =
                    mysteryManager.GetObjectiveStatus(objectiveId);
                string marker = status == ObjectiveStatus.Complete
                    ? "[x]"
                    : status == ObjectiveStatus.Locked ? "[-]" : "[ ]";
                GUI.Label(
                    new Rect(
                        panelRect.x + 12f,
                        panelRect.y + 28f + i * 24f,
                        panelRect.width - 24f,
                        22f),
                    $"{marker} {objectiveId}  {mysteryManager.GetObjectiveText(objectiveId)}");
            }

            GUI.Label(
                new Rect(panelRect.x + 12f, panelRect.yMax + 6f, 350f, 22f),
                $"Evidence collected: {mysteryManager.KnownEvidenceCount}/6   |   [J] Journal");
        }

        private void DrawDialogue()
        {
            Rect rect = CenteredRect(700f, 540f);
            GUILayout.BeginArea(rect, GUI.skin.window);
            GUILayout.Label(
                MysterySuspectInteractable.GetDisplayName(activeSuspect),
                GUI.skin.box);
            dialogueScroll = GUILayout.BeginScrollView(
                dialogueScroll,
                GUILayout.Height(150f));
            GUILayout.Label(currentDialogue, GUI.skin.box);
            GUILayout.EndScrollView();

            GUILayout.Label("ASK A QUESTION");
            IReadOnlyList<string> questions =
                mysteryManager.GetQuestionPrompts(activeSuspect);
            for (int i = 0; i < questions.Count; i++)
            {
                int questionIndex = i;
                if (GUILayout.Button(questions[i]))
                {
                    currentDialogue =
                        mysteryManager.AskQuestion(activeSuspect, questionIndex);
                }
            }

            GUILayout.Label("PRESENT COLLECTED EVIDENCE");
            IReadOnlyList<EvidenceItem> presentable =
                mysteryManager.GetPresentableEvidence(activeSuspect);
            if (presentable.Count == 0)
            {
                GUILayout.Label("No relevant evidence has been collected yet.");
            }
            else
            {
                for (int i = 0; i < presentable.Count; i++)
                {
                    EvidenceItem item = presentable[i];
                    string status = mysteryManager.IsEvidencePresentedTo(
                        activeSuspect, item.EvidenceId) ? "Review" : "Present";
                    if (GUILayout.Button(
                            $"{status}: {item.DisplayName} [{item.EvidenceId}]"))
                    {
                        currentDialogue = mysteryManager.PresentEvidence(
                            activeSuspect, item.EvidenceId);
                    }
                }
            }

            if (GUILayout.Button("Close interview"))
                CloseModal();

            GUILayout.EndArea();
        }

        private void DrawAccusation()
        {
            Rect rect = CenteredRect(700f, 560f);
            GUILayout.BeginArea(rect, GUI.skin.window);
            GUILayout.Label("FORMAL ACCUSATION", GUI.skin.box);
            GUILayout.Label(
                "Choose a suspect and actively submit supporting evidence. " +
                "Collected evidence alone does not satisfy the decisive-evidence rule.");
            accusationScroll = GUILayout.BeginScrollView(
                accusationScroll,
                GUILayout.Height(115f));
            foreach (SuspectId suspect in GetSuspects())
            {
                bool selected = selectedAccusationSuspect == suspect;
                bool next = GUILayout.Toggle(
                    selected,
                    MysterySuspectInteractable.GetDisplayName(suspect),
                    GUI.skin.button);
                if (next)
                    selectedAccusationSuspect = suspect;
            }

            GUILayout.Space(8f);
            GUILayout.Label("SUBMIT EVIDENCE");
            if (evidenceManager != null)
            {
                foreach (EvidenceItem item in evidenceManager.GetCollectedEvidence())
                {
                    bool selected = selectedEvidence.Contains(item.EvidenceId);
                    bool next = GUILayout.Toggle(
                        selected,
                        $"{item.DisplayName} [{item.EvidenceId}]",
                        GUI.skin.button);
                    if (next && !selected)
                        selectedEvidence.Add(item.EvidenceId);
                    else if (!next && selected)
                        selectedEvidence.Remove(item.EvidenceId);
                }
            }

            GUILayout.EndScrollView();

            if (!string.IsNullOrEmpty(mysteryManager.CaseMessage) &&
                mysteryManager.CaseState == MysteryCaseState.Inconclusive)
            {
                GUILayout.Label(mysteryManager.CaseMessage, GUI.skin.box);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Submit accusation"))
                SubmitAccusation();
            if (GUILayout.Button("Close"))
                CloseModal();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawEnding()
        {
            Rect rect = CenteredRect(760f, 500f);
            GUILayout.BeginArea(rect, GUI.skin.window);
            GUILayout.Label(
                mysteryManager.CaseState == MysteryCaseState.Solved
                    ? "CASE SOLVED"
                    : "CASE FAILED",
                GUI.skin.box);
            endingScroll = GUILayout.BeginScrollView(
                endingScroll,
                GUILayout.Height(rect.height - 70f));
            GUILayout.Label(mysteryManager.CaseMessage, GUI.skin.box);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
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
