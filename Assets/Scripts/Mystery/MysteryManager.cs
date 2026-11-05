using System;
using System.Collections.Generic;
using UnityEngine;

namespace MurderMystery
{
    public enum SuspectId
    {
        RohanKapoor,
        PriyaSharma,
        VikramDesai,
        KarthikNair
    }

    public enum MysteryLocation
    {
        Unknown,
        Foyer,
        IoTLab1112,
        IoTCentre1108,
        ISFCRSecurityLab,
        MeetingRoom,
        PESAcademyOffices
    }

    public enum MysteryCaseState
    {
        Open,
        Inconclusive,
        Failed,
        Solved
    }

    public enum AccusationResult
    {
        Locked,
        InvalidEvidence,
        AlreadyResolved,
        WrongSuspect,
        Inconclusive,
        Solved
    }

    public enum ObjectiveStatus
    {
        Active,
        Complete,
        Locked
    }

    public sealed class MysteryManager : MonoBehaviour
    {
        public const string KeycardId = "EV-KEYCARD-001";
        public const string LogbookId = "EV-LOGBOOK-002";
        public const string PhoneId = "EV-PHONE-003";
        public const string CctvId = "EV-CCTV-004";
        public const string GrantId = "EV-GRANT-005";
        public const string PowerId = "EV-POWER-006";

        private static readonly string[] ObjectiveIds =
        {
            "OBJ_1", "OBJ_2", "OBJ_3", "OBJ_4", "OBJ_5",
            "OBJ_6", "OBJ_7", "OBJ_8", "OBJ_9"
        };

        private static readonly string[] ObjectiveTexts =
        {
            "Secure and inspect IoT Lab 1112",
            "Collect physical evidence in Lab 1112",
            "Conduct initial suspect interviews",
            "Investigate security monitoring records",
            "Search departmental administrative records",
            "Analyze lab power grid telemetry",
            "Break suspect contradictions",
            "Uncover the primary motive",
            "Deliver formal accusation"
        };

        private static readonly IReadOnlyList<string> ReadOnlyObjectiveIds =
            Array.AsReadOnly(ObjectiveIds);

        private static readonly string[] AccusationEvidence =
        {
            KeycardId, CctvId, GrantId
        };

        private static readonly string[] AllEvidence =
        {
            KeycardId, LogbookId, PhoneId, CctvId, GrantId, PowerId
        };

        private readonly HashSet<SuspectId> interviewed = new HashSet<SuspectId>();
        private readonly HashSet<SuspectId> confronted = new HashSet<SuspectId>();
        private readonly HashSet<string> presentedEvidence =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> accusationSubmissions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<SuspectId, HashSet<string>> presentedToSuspect =
            new Dictionary<SuspectId, HashSet<string>>();
        private readonly Dictionary<string, string> presentationReplies =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<MysteryLocation> activeLocations =
            new List<MysteryLocation>();

        private EvidenceManager evidenceManager;
        private bool enteredCrimeScene;
        private bool motiveEstablished;

        public event Action OnProgressChanged;
        public event Action<MysteryCaseState, string> OnCaseStateChanged;

        public MysteryCaseState CaseState { get; private set; } = MysteryCaseState.Open;
        public string CaseMessage { get; private set; } = string.Empty;
        public MysteryLocation CurrentLocation { get; private set; } =
            MysteryLocation.Unknown;

        public bool IsInAccusationLocation =>
            CurrentLocation == MysteryLocation.Foyer ||
            CurrentLocation == MysteryLocation.MeetingRoom;

        public bool CanAccuse =>
            evidenceManager != null &&
            CaseState != MysteryCaseState.Failed &&
            CaseState != MysteryCaseState.Solved &&
            AllSuspectsInterviewed &&
            KnownEvidenceCount >= 4;

        public bool CanOpenAccusation => CanAccuse && IsInAccusationLocation;

        public int KnownEvidenceCount
        {
            get
            {
                if (evidenceManager == null)
                    return 0;

                int count = 0;
                foreach (string evidenceId in AllEvidence)
                {
                    if (evidenceManager.HasEvidence(evidenceId))
                        count++;
                }

                return count;
            }
        }

        public bool AllSuspectsInterviewed =>
            interviewed.Count == Enum.GetValues(typeof(SuspectId)).Length;

        public void Initialize(EvidenceManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            if (evidenceManager != null)
            {
                evidenceManager.OnEvidenceCollected -= HandleEvidenceCollected;
                evidenceManager.OnEvidenceCleared -= HandleEvidenceCleared;
            }

            evidenceManager = manager;
            evidenceManager.OnEvidenceCollected += HandleEvidenceCollected;
            evidenceManager.OnEvidenceCleared += HandleEvidenceCleared;
            RefreshProgress();
        }

        private void OnDestroy()
        {
            if (evidenceManager == null)
                return;

            evidenceManager.OnEvidenceCollected -= HandleEvidenceCollected;
            evidenceManager.OnEvidenceCleared -= HandleEvidenceCleared;
        }

        public void EnterLocation(MysteryLocation location)
        {
            if (!Enum.IsDefined(typeof(MysteryLocation), location))
                throw new ArgumentOutOfRangeException(nameof(location));

            if (location == MysteryLocation.Unknown)
                return;

            activeLocations.Remove(location);
            activeLocations.Add(location);

            if (location == MysteryLocation.IoTLab1112)
                enteredCrimeScene = true;

            UpdateCurrentLocation();
            RefreshProgress();
        }

        public void ExitLocation(MysteryLocation location)
        {
            activeLocations.Remove(location);
            UpdateCurrentLocation();
            RefreshProgress();
        }

        public ObjectiveStatus GetObjectiveStatus(string objectiveId)
        {
            int index = Array.IndexOf(ObjectiveIds, objectiveId);
            if (index < 0)
                throw new ArgumentException("Unknown objective ID.", nameof(objectiveId));

            if (IsObjectiveComplete(index))
                return ObjectiveStatus.Complete;

            if (index == 8 && !CanAccuse)
                return ObjectiveStatus.Locked;

            return ObjectiveStatus.Active;
        }

        public IReadOnlyList<string> GetObjectiveIds()
        {
            return ReadOnlyObjectiveIds;
        }

        public string GetObjectiveText(string objectiveId)
        {
            int index = Array.IndexOf(ObjectiveIds, objectiveId);
            if (index < 0)
                throw new ArgumentException("Unknown objective ID.", nameof(objectiveId));

            return ObjectiveTexts[index];
        }

        public string BeginInterview(SuspectId suspect)
        {
            EnsureKnownSuspect(suspect);
            if (CaseState == MysteryCaseState.Failed ||
                CaseState == MysteryCaseState.Solved)
            {
                return CaseMessage;
            }

            bool firstInterview = interviewed.Add(suspect);
            RefreshProgress();
            return firstInterview
                ? GetInitialStatement(suspect)
                : GetReturnStatement(suspect);
        }

        public IReadOnlyList<string> GetQuestionPrompts(SuspectId suspect)
        {
            EnsureKnownSuspect(suspect);
            return Array.AsReadOnly(GetQuestions(suspect));
        }

        public string AskQuestion(SuspectId suspect, int questionIndex)
        {
            EnsureKnownSuspect(suspect);
            if (!interviewed.Contains(suspect))
                throw new InvalidOperationException(
                    "The suspect must be interviewed before asking questions.");

            string[] responses = GetQuestionResponses(suspect);
            if (questionIndex < 0 || questionIndex >= responses.Length)
                throw new ArgumentOutOfRangeException(nameof(questionIndex));

            return responses[questionIndex];
        }

        public IReadOnlyList<EvidenceItem> GetPresentableEvidence(SuspectId suspect)
        {
            EnsureKnownSuspect(suspect);
            EnsureInitialized();
            var result = new List<EvidenceItem>();
            foreach (EvidenceItem item in evidenceManager.GetCollectedEvidence())
            {
                if (item != null &&
                    IsRelevantEvidence(suspect, item.EvidenceId))
                    result.Add(item);
            }

            return result.AsReadOnly();
        }

        public bool IsEvidencePresented(string evidenceId)
        {
            return presentedEvidence.Contains(evidenceId) ||
                   accusationSubmissions.Contains(evidenceId);
        }

        public bool IsEvidencePresentedTo(SuspectId suspect, string evidenceId)
        {
            EnsureKnownSuspect(suspect);
            return presentedToSuspect.TryGetValue(suspect, out HashSet<string> items) &&
                   items.Contains(evidenceId);
        }

        public string PresentEvidence(SuspectId suspect, string evidenceId)
        {
            EnsureKnownSuspect(suspect);
            EnsureInitialized();

            if (!interviewed.Contains(suspect))
                return "Interview this suspect before presenting evidence.";

            if (!evidenceManager.HasEvidence(evidenceId))
                return "That evidence has not been collected.";

            if (!IsRelevantEvidence(suspect, evidenceId))
                return "This suspect has nothing useful to say about that evidence.";

            string cacheKey = GetPresentationKey(suspect, evidenceId);
            if (presentationReplies.TryGetValue(cacheKey, out string previousReply))
                return previousReply;

            if (!presentedToSuspect.TryGetValue(
                    suspect, out HashSet<string> suspectEvidence))
            {
                suspectEvidence = new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);
                presentedToSuspect.Add(suspect, suspectEvidence);
            }

            suspectEvidence.Add(evidenceId);
            presentedEvidence.Add(evidenceId);

            string reply = suspect == SuspectId.VikramDesai &&
                           HasPresentedVikramEvidenceTrio()
                ? GetVikramConfrontation()
                : GetEvidenceReply(suspect, evidenceId);
            presentationReplies[cacheKey] = reply;
            RefreshProgress();
            return reply;
        }

        public AccusationResult TryAccuse(
            SuspectId suspect,
            IReadOnlyCollection<string> submittedEvidenceIds)
        {
            EnsureKnownSuspect(suspect);
            EnsureInitialized();

            if (CaseState == MysteryCaseState.Failed ||
                CaseState == MysteryCaseState.Solved)
            {
                return AccusationResult.AlreadyResolved;
            }

            if (!CanOpenAccusation)
                return AccusationResult.Locked;

            if (submittedEvidenceIds == null)
                return AccusationResult.InvalidEvidence;

            foreach (string evidenceId in submittedEvidenceIds)
            {
                if (!evidenceManager.HasEvidence(evidenceId))
                    return AccusationResult.InvalidEvidence;
            }

            foreach (string evidenceId in submittedEvidenceIds)
            {
                accusationSubmissions.Add(evidenceId);
                presentedEvidence.Add(evidenceId);
            }

            if (suspect != SuspectId.VikramDesai)
            {
                SetCaseState(
                    MysteryCaseState.Failed,
                    GetWrongAccusationMessage(suspect));
                return AccusationResult.WrongSuspect;
            }

            var submitted = new HashSet<string>(
                submittedEvidenceIds, StringComparer.OrdinalIgnoreCase);
            bool hasDecisiveEvidence = true;
            foreach (string evidenceId in AccusationEvidence)
            {
                if (!submitted.Contains(evidenceId))
                {
                    hasDecisiveEvidence = false;
                    break;
                }
            }

            if (!hasDecisiveEvidence)
            {
                SetCaseState(
                    MysteryCaseState.Inconclusive,
                    "Circumstantial suspicion and vague allegations do not constitute legal proof. " +
                    "Without verified proof of badge registration, corridor transit, and financial " +
                    "motive, my client has nothing more to say. Gather and actively present the " +
                    "keycard, security log, and grant summary before re-accusing Professor Desai.");
                return AccusationResult.Inconclusive;
            }

            SetCaseState(MysteryCaseState.Solved, GetVictoryMessage());
            return AccusationResult.Solved;
        }

        private string GetEvidenceReply(SuspectId suspect, string evidenceId)
        {
            switch (suspect)
            {
                case SuspectId.RohanKapoor:
                    if (evidenceId == PhoneId)
                    {
                        return "We found Ananya's phone. An unsent message says, \"They know " +
                               "what happened.\" Was she talking about you?\n\n" +
                               "I... I don't know! Ananya was suspicious of everyone lately. " +
                               "She thought people were undermining her work.";
                    }

                    return Confront(
                        suspect,
                        "Your badge scanned into 1108 at 23:30. The log records no later exit " +
                        "scan or entry into 1112. But you weren't preparing slides, were you? " +
                        "Tell me what you were doing at the terminal.",
                        "God... fine! Yes! Ananya caught me sharing preliminary telemetry with " +
                        "an outside firm. I needed money for my family's debts. I was purging the " +
                        "transfer logs so she wouldn't have proof for the syndicate. I was in 1108 " +
                        "when I ran the script. The badge log shows my entry, and there is no later " +
                        "exit scan or 1112 scan. I never went near Lab 1112!");

                case SuspectId.PriyaSharma:
                    if (evidenceId == LogbookId &&
                        HasPresented(suspect, PowerId))
                    {
                        return GetPriyaConfrontation();
                    }

                    if (evidenceId == PowerId &&
                        HasPresented(suspect, LogbookId))
                    {
                        return GetPriyaConfrontation();
                    }

                    if (evidenceId == LogbookId)
                    {
                        return "We found a page torn from the 1112 logbook recording a heated " +
                               "argument during the late shift.\n\n" +
                               "People argue in labs all the time. Doesn't prove who was there.";
                    }

                    return "The telemetry shows the calibration rig stopped drawing power at " +
                           "22:50. It does not identify who disconnected it or establish anyone's " +
                           "physical movements. Pair it with the torn logbook if you want to " +
                           "question my statement.";

                case SuspectId.VikramDesai:
                    if (evidenceId == KeycardId)
                    {
                        return "A Senior Researcher badge was scanned into 1112 at 23:45, " +
                               "minutes before Ananya died.\n\n" +
                               "Well, Dr. Mitra herself was a Senior Researcher. Or perhaps " +
                               "someone misplaced an old credential.";
                    }

                    if (evidenceId == CctvId)
                    {
                        return "Corridor cameras recorded someone in a tailored formal blazer " +
                               "walking to 1112 at 23:43 and leaving at 23:51. That matches your " +
                               "attire, Professor.\n\n" +
                               "The corridor camera is notoriously low-resolution and grainy. " +
                               "Many faculty members wear dark blazers. That is hardly proof of " +
                               "my presence.";
                    }

                    if (evidenceId == GrantId)
                    {
                        motiveEstablished = true;
                        if (HasPresented(suspect, KeycardId) &&
                            HasPresented(suspect, CctvId))
                        {
                            return GetVikramConfrontation();
                        }

                        return "The audit dossier links V.D. Advisory Services to you, Professor. " +
                               "What do you have to say about the ₹4,80,000 in grant funds?\n\n" +
                               "Those are allegations from Dr. Mitra's private research binder. " +
                               "An audited summary is not a verdict. I had no reason to harm her.";
                    }

                    return "Ananya's unsent draft says, \"They know what happened.\" Who do you " +
                           "think she meant?\n\nThat draft does not name a recipient or identify " +
                           "who Ananya meant by \"they.\" It cannot tell us which person she " +
                           "intended to contact.";

                case SuspectId.KarthikNair:
                    if (evidenceId == CctvId)
                    {
                        return Confront(
                            suspect,
                            "You claim you were watching the monitors all night, Karthik. But " +
                            "the terminal log shows an idle screen-lock from 23:35 to 23:50. " +
                            "Were you actually at your desk?",
                            "Look... please don't report this. I had a pounding headache. At " +
                            "23:35 I walked down to the foyer vending machine to grab a strong " +
                            "coffee and sat on the bench for ten minutes. There's a 23:40 receipt " +
                            "on the desk, but I know that doesn't prove who bought it. I was the " +
                            "one who went to the kiosk. I got back to ISFCR at 23:50 and barely " +
                            "unlocked my terminal before the sirens went crazy. If the administration " +
                            "finds out I left the desk, after Dr. Mitra cited me last month, I'm " +
                            "fired! But I swear, I never touched anyone. I was just drinking coffee " +
                            "in the foyer!");
                    }

                    return "Ananya's draft says, \"They know what happened. Don't speak to " +
                           "security until we talk.\" What do you make of that?\n\nThe phone draft " +
                           "does not identify its recipient or say who Ananya meant by \"they.\" " +
                           "I can't use it to identify a suspect.";

                default:
                    throw new ArgumentOutOfRangeException(nameof(suspect));
            }
        }

        private string GetPriyaConfrontation()
        {
            const string reply =
                "You said you never went near 1112, Priya. The power report shows the calibration " +
                "bench drawing power there from 22:15 to 22:50, and this torn logbook records a " +
                "late-shift argument. Explain that.\n\n" +
                "Alright! Look, Ananya put a black mark on my record over calibration errors. I " +
                "sneaked into 1112 at 22:15 to re-run the test on her bench and prove her sensors " +
                "were faulty. She came in at 22:30 and caught me. We had an ugly shouting match. " +
                "I ripped out my test sheet so she couldn't use it against me. The diagnostic cycle " +
                "finished at 22:50—the power report confirms the rig stopped drawing power then. I " +
                "disconnected my tools and left, and I was back in ISFCR by 22:55. That's my account, " +
                "and it was well before she died. I didn't kill her!";

            MarkConfronted(SuspectId.PriyaSharma);
            CacheReply(SuspectId.PriyaSharma, LogbookId, reply);
            CacheReply(SuspectId.PriyaSharma, PowerId, reply);
            return reply;
        }

        private string GetVikramConfrontation()
        {
            const string reply =
                "It's over, Professor. Badge #SR-01 was swiped at 23:45. The security registry " +
                "proves only two Senior Researcher badges exist: Dr. Mitra's, which remained on " +
                "her body, and yours. You just told me your card never left your pocket. Supporting " +
                "camera footage tracked your tailored blazer leaving at 23:51. And we found Dr. " +
                "Mitra's audit dossier: complete corporate filings proving you are the sole " +
                "registered director of 'V.D. Advisory Services,' where ₹4,80,000 in grant funds " +
                "were embezzled. You went there to silence her.\n\n" +
                "…She was going to destroy everything. Twenty-five years of academic standing, " +
                "over an administrative reallocation! I went in to reason with her, to beg her to " +
                "delay the report. She wouldn't listen. She called me a thief and reached for her " +
                "phone. I lost control—I shoved her away to stop her. She fell back... the bench " +
                "corner... the sound of her skull... I panicked! I threw the circuit breakers so no " +
                "one could see in through the glass, and I ran. It was an accident! I didn't plan it!";

            MarkConfronted(SuspectId.VikramDesai);
            motiveEstablished = true;
            CacheReply(SuspectId.VikramDesai, KeycardId, reply);
            CacheReply(SuspectId.VikramDesai, CctvId, reply);
            CacheReply(SuspectId.VikramDesai, GrantId, reply);
            RefreshProgress();
            return reply;
        }

        private string Confront(
            SuspectId suspect,
            string playerLine,
            string suspectLine)
        {
            if (confronted.Contains(suspect))
                return GetReturnStatement(suspect);

            confronted.Add(suspect);
            RefreshProgress();
            return playerLine + "\n\n" + suspectLine;
        }

        private void MarkConfronted(SuspectId suspect)
        {
            if (confronted.Add(suspect))
                RefreshProgress();
        }

        private void CacheReply(
            SuspectId suspect,
            string evidenceId,
            string reply)
        {
            presentationReplies[GetPresentationKey(suspect, evidenceId)] = reply;
        }

        private bool HasPresented(SuspectId suspect, string evidenceId)
        {
            return presentedToSuspect.TryGetValue(suspect, out HashSet<string> items) &&
                   items.Contains(evidenceId);
        }

        private bool HasPresentedVikramEvidenceTrio()
        {
            return HasPresented(SuspectId.VikramDesai, KeycardId) &&
                   HasPresented(SuspectId.VikramDesai, CctvId) &&
                   HasPresented(SuspectId.VikramDesai, GrantId);
        }

        private static bool IsRelevantEvidence(
            SuspectId suspect,
            string evidenceId)
        {
            switch (suspect)
            {
                case SuspectId.RohanKapoor:
                    return Matches(evidenceId, PhoneId) ||
                           Matches(evidenceId, CctvId);
                case SuspectId.PriyaSharma:
                    return Matches(evidenceId, LogbookId) ||
                           Matches(evidenceId, PowerId);
                case SuspectId.VikramDesai:
                    return Matches(evidenceId, KeycardId) ||
                           Matches(evidenceId, CctvId) ||
                           Matches(evidenceId, GrantId) ||
                           Matches(evidenceId, PhoneId);
                case SuspectId.KarthikNair:
                    return Matches(evidenceId, CctvId) ||
                           Matches(evidenceId, PhoneId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(suspect));
            }
        }

        private static bool Matches(string evidenceId, string canonicalId)
        {
            return string.Equals(
                evidenceId,
                canonicalId,
                StringComparison.OrdinalIgnoreCase);
        }

        private bool IsObjectiveComplete(int index)
        {
            switch (index)
            {
                case 0:
                    return enteredCrimeScene;
                case 1:
                    return evidenceManager != null &&
                           evidenceManager.HasEvidence(KeycardId) &&
                           evidenceManager.HasEvidence(LogbookId) &&
                           evidenceManager.HasEvidence(PhoneId);
                case 2:
                    return AllSuspectsInterviewed;
                case 3:
                    return HasEvidence(CctvId);
                case 4:
                    return HasEvidence(GrantId);
                case 5:
                    return HasEvidence(PowerId);
                case 6:
                    return CountConfrontations() >= 2;
                case 7:
                    return motiveEstablished;
                case 8:
                    return CaseState == MysteryCaseState.Solved ||
                           CaseState == MysteryCaseState.Failed;
                default:
                    return false;
            }
        }

        private bool HasEvidence(string evidenceId)
        {
            return evidenceManager != null && evidenceManager.HasEvidence(evidenceId);
        }

        private int CountConfrontations()
        {
            int count = 0;
            if (confronted.Contains(SuspectId.RohanKapoor)) count++;
            if (confronted.Contains(SuspectId.PriyaSharma)) count++;
            if (confronted.Contains(SuspectId.KarthikNair)) count++;
            return count;
        }

        private void HandleEvidenceCollected(EvidenceItem item)
        {
            if (item != null &&
                string.Equals(
                    item.EvidenceId,
                    GrantId,
                    StringComparison.OrdinalIgnoreCase))
            {
                motiveEstablished = true;
            }

            RefreshProgress();
        }

        private void HandleEvidenceCleared()
        {
            interviewed.Clear();
            confronted.Clear();
            presentedEvidence.Clear();
            accusationSubmissions.Clear();
            presentedToSuspect.Clear();
            presentationReplies.Clear();
            enteredCrimeScene = false;
            motiveEstablished = false;
            CaseState = MysteryCaseState.Open;
            CaseMessage = string.Empty;
            RefreshProgress();
        }

        private void UpdateCurrentLocation()
        {
            CurrentLocation = activeLocations.Count == 0
                ? MysteryLocation.Unknown
                : activeLocations[activeLocations.Count - 1];
        }

        private void RefreshProgress()
        {
            OnProgressChanged?.Invoke();
        }

        private void SetCaseState(MysteryCaseState state, string message)
        {
            CaseState = state;
            CaseMessage = message;
            OnCaseStateChanged?.Invoke(state, message);
            RefreshProgress();
        }

        private string GetWrongAccusationMessage(SuspectId suspect)
        {
            switch (suspect)
            {
                case SuspectId.RohanKapoor:
                    return "CASE FAILED — ROHAN KAPOOR\n\n" +
                           "Rohan admits to deleting unauthorized data-transfer records. " +
                           "The access log records his 23:30 badge entry into 1108 and no later " +
                           "exit or 1112 scan, but does not prove he stayed there continuously. " +
                           "You arrested a data thief while the killer walked free.";
                case SuspectId.PriyaSharma:
                    return "CASE FAILED — PRIYA SHARMA\n\n" +
                           "The power telemetry establishes that the calibration rig stopped " +
                           "drawing power at 22:50; it does not track Priya's movements. Her " +
                           "admission places her return to ISFCR by 22:55, well before the murder. " +
                           "Her misconduct was insubordination, not homicide.";
                case SuspectId.KarthikNair:
                    return "CASE FAILED — KARTHIK NAIR\n\n" +
                           "The console was idle from 23:35 to 23:50. The 23:40 kiosk receipt " +
                           "proves only that an anonymous coffee transaction occurred; Karthik's " +
                           "admission places him in the foyer. His security badge was not a Senior " +
                           "Researcher credential. The real killer remains unidentified.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(suspect));
            }
        }

        private static string GetVictoryMessage()
        {
            return "CASE CLOSED — THE LAST SIGNAL\n\n" +
                   "You formally present the evidence to campus security and the university " +
                   "syndicate:\n\n" +
                   "1. PHYSICAL PRESENCE (EV-KEYCARD-001 + EV-CCTV-004): At 23:45, badge " +
                   "#SR-01 entered Lab 1112. The registry identifies #SR-01 as Prof. Desai's; " +
                   "his statement that the card remained in his pocket and supporting corridor " +
                   "footage provide converging circumstantial evidence, not facial recognition.\n\n" +
                   "2. MOTIVE (EV-GRANT-005): Corporate filings name Vikram Desai as sole " +
                   "director of V.D. Advisory Services, linked to ₹4,80,000 in diverted grant " +
                   "funds. Ananya planned to submit the audit dossier to the syndicate.\n\n" +
                   "3. CONFESSION: Confronted with the decisive evidence, Vikram admits that " +
                   "he intentionally pushed Ananya at 23:48 during their argument, then manually " +
                   "cut power at 23:49 to obscure the scene. The evidence forms a strong converging " +
                   "circumstantial deduction, substantiated by his confession.";
        }

        private static string GetInitialStatement(SuspectId suspect)
        {
            switch (suspect)
            {
                case SuspectId.RohanKapoor:
                    return "I can't believe this is happening. I was in Ananya's office in 1108 " +
                           "working on slides for our project review. I hadn't seen her since she " +
                           "went down to the lab around 22:00. When the siren went off, I ran " +
                           "straight to the foyer.";
                case SuspectId.PriyaSharma:
                    return "I've been in the ISFCR lab all evening running diagnostics. I never " +
                           "set foot near Lab 1112 tonight. Ananya and I had our professional " +
                           "differences, but I haven't spoken to her today.";
                case SuspectId.VikramDesai:
                    return "A tragic, unspeakable loss. Dr. Mitra was our finest researcher. I was " +
                           "down the hall in the Meeting Room preparing an urgent grant submission " +
                           "until the siren sounded. I have not spoken to Dr. Mitra since yesterday " +
                           "afternoon's general meeting.";
                case SuspectId.KarthikNair:
                    return "I remained at my terminal in the ISFCR lab observing all camera feeds " +
                           "continuously throughout the night. No unauthorized persons entered the " +
                           "corridor. The alert at 23:52 was purely a technical power fault.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(suspect));
            }
        }

        private static string GetReturnStatement(SuspectId suspect)
        {
            switch (suspect)
            {
                case SuspectId.RohanKapoor:
                    return "What else do you need to ask me?";
                case SuspectId.PriyaSharma:
                    return "I've told you what I know. Do you have something else to show me?";
                case SuspectId.VikramDesai:
                    return "I have answered your questions. Is there anything else?";
                case SuspectId.KarthikNair:
                    return "I have nothing more to add unless you have evidence.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(suspect));
            }
        }

        private static string[] GetQuestions(SuspectId suspect)
        {
            switch (suspect)
            {
                case SuspectId.RohanKapoor:
                    return new[]
                    {
                        "What specific slides were you working on?",
                        "Did you hear anything in the hallway?"
                    };
                case SuspectId.PriyaSharma:
                    return new[]
                    {
                        "What diagnostics were you running?",
                        "Was Karthik in ISFCR with you?"
                    };
                case SuspectId.VikramDesai:
                    return new[]
                    {
                        "Did you notice anyone in the corridor?",
                        "Where was your keycard during the evening?"
                    };
                case SuspectId.KarthikNair:
                    return new[]
                    {
                        "What did the corridor feeds show around 23:45?",
                        "Did Ananya have any visitors?"
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(suspect));
            }
        }

        private static string[] GetQuestionResponses(SuspectId suspect)
        {
            switch (suspect)
            {
                case SuspectId.RohanKapoor:
                    return new[]
                    {
                        "Latency charts, deployment readiness, and the usual project-review " +
                        "material. Nothing unusual.",
                        "Nothing until the main alarm blared."
                    };
                case SuspectId.PriyaSharma:
                    return new[]
                    {
                        "Sensor parity checks on the bench hardware.",
                        "He was at the security desk when I came in. I keep my head down and do " +
                        "my work."
                    };
                case SuspectId.VikramDesai:
                    return new[]
                    {
                        "I was completely immersed in budget sheets.",
                        "Right here in my breast pocket. I never take it out unless swiping."
                    };
                case SuspectId.KarthikNair:
                    return new[]
                    {
                        "All quiet. Nothing flagged on my watch.",
                        "Not that I logged."
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(suspect));
            }
        }

        private static string GetPresentationKey(
            SuspectId suspect,
            string evidenceId)
        {
            return suspect + ":" + evidenceId;
        }

        private static void EnsureKnownSuspect(SuspectId suspect)
        {
            if (!Enum.IsDefined(typeof(SuspectId), suspect))
                throw new ArgumentOutOfRangeException(nameof(suspect));
        }

        private void EnsureInitialized()
        {
            if (evidenceManager == null)
                throw new InvalidOperationException(
                    "MysteryManager.Initialize must be called before use.");
        }
    }
}
