using System;
using System.Collections.Generic;
using UnityEngine;
using MurderMystery;

namespace MurderMystery.Tests
{
    public sealed class MysterySystemTests : MonoBehaviour
    {
        public struct TestResult
        {
            public string TestName;
            public bool Passed;
            public string Message;
        }

        private readonly List<TestResult> results = new List<TestResult>();
        private readonly List<EvidenceItem> createdEvidence =
            new List<EvidenceItem>();
        private EvidenceManager evidenceManager;
        private MysteryManager mysteryManager;
        private GameObject evidenceManagerObject;
        private GameObject mysteryManagerObject;

        public IReadOnlyList<TestResult> Results => results.AsReadOnly();

        public bool RunAllTests()
        {
            results.Clear();
            RunTest(
                "Accusation requires all interviews and four canonical clues",
                TestAccusationUnlockConditions);
            RunTest(
                "Unrecognized clues do not satisfy the evidence threshold",
                TestOnlyCanonicalCluesCount);
            RunTest(
                "Accusing Rohan causes permanent case failure",
                () => TestWrongSuspectCannotBeRetried(SuspectId.RohanKapoor));
            RunTest(
                "Accusing Priya causes permanent case failure",
                () => TestWrongSuspectCannotBeRetried(SuspectId.PriyaSharma));
            RunTest(
                "Accusing Karthik causes permanent case failure",
                () => TestWrongSuspectCannotBeRetried(SuspectId.KarthikNair));
            RunTest(
                "Accusation is restricted to the Foyer and Meeting Room",
                TestAccusationLocationGate);
            RunTest(
                "Every decisive clue must be submitted in the same accusation",
                TestDecisiveEvidenceIsMandatory);
            RunTest(
                "Evidence collection and presentation are separate",
                TestCollectionIsNotPresentation);
            RunTest(
                "Priya confrontation requires both relevant clues",
                TestPriyaConfrontationDependencies);
            RunTest(
                "Grant evidence establishes motive when discovered",
                TestGrantEvidenceEstablishesMotive);
            RunTest(
                "Vikram's decisive confrontation accepts any presentation order",
                TestVikramConfrontationOrder);
            RunTest(
                "Confrontations preserve evidence boundaries and do not repeat",
                TestRohanAndKarthikConfrontations);
            RunTest(
                "Room visits and evidence collection work out of order",
                TestOrderIndependentProgress);

            int failed = 0;
            foreach (TestResult result in results)
            {
                if (result.Passed)
                    Debug.Log($"[MysterySystemTests][PASS] {result.TestName}");
                else
                {
                    failed++;
                    Debug.LogError(
                        $"[MysterySystemTests][FAIL] {result.TestName}: " +
                        result.Message);
                }
            }

            Debug.Log(
                $"[MysterySystemTests] {results.Count - failed} passed, " +
                $"{failed} failed.");
            return failed == 0;
        }

        private void RunTest(string name, Action test)
        {
            SetUp();
            try
            {
                test();
                results.Add(new TestResult
                {
                    TestName = name,
                    Passed = true,
                    Message = string.Empty
                });
            }
            catch (Exception exception)
            {
                results.Add(new TestResult
                {
                    TestName = name,
                    Passed = false,
                    Message = exception.Message
                });
            }
            finally
            {
                TearDown();
            }
        }

        private void SetUp()
        {
            createdEvidence.Clear();
            evidenceManager = EvidenceManager.Instance;
            if (evidenceManager == null)
            {
                evidenceManagerObject =
                    new GameObject("MysterySystemTests_EvidenceManager");
                evidenceManager =
                    evidenceManagerObject.AddComponent<EvidenceManager>();
            }

            evidenceManager.ClearCollectedEvidence();
            mysteryManagerObject =
                new GameObject("MysterySystemTests_MysteryManager");
            mysteryManager =
                mysteryManagerObject.AddComponent<MysteryManager>();
            mysteryManager.Initialize(evidenceManager);
        }

        private void TearDown()
        {
            if (mysteryManagerObject != null)
                DestroyImmediate(mysteryManagerObject);
            if (evidenceManagerObject != null)
                DestroyImmediate(evidenceManagerObject);

            foreach (EvidenceItem item in createdEvidence)
            {
                if (item != null)
                    DestroyImmediate(item);
            }

            createdEvidence.Clear();
            evidenceManagerObject = null;
            mysteryManagerObject = null;
            evidenceManager = null;
            mysteryManager = null;
        }

        private void TestAccusationUnlockConditions()
        {
            Collect(
                MysteryManager.KeycardId,
                MysteryManager.LogbookId,
                MysteryManager.PhoneId,
                MysteryManager.CctvId);

            Interview(SuspectId.RohanKapoor);
            Interview(SuspectId.PriyaSharma);
            Interview(SuspectId.VikramDesai);
            Assert(!mysteryManager.CanAccuse,
                "Three initial interviews must not unlock an accusation.");

            Interview(SuspectId.KarthikNair);
            Assert(mysteryManager.CanAccuse,
                "Four initial interviews and four clues should unlock an accusation.");
        }

        private void TestOnlyCanonicalCluesCount()
        {
            Collect(
                MysteryManager.KeycardId,
                MysteryManager.LogbookId,
                MysteryManager.PhoneId,
                "EV-UNRELATED-099");
            InterviewAllSuspects();
            Assert(!mysteryManager.CanAccuse,
                "An unrelated clue must not count toward the four-of-six threshold.");

            Collect(MysteryManager.CctvId);
            Assert(mysteryManager.CanAccuse,
                "The fourth canonical clue should satisfy the evidence threshold.");
        }

        private void TestWrongSuspectCannotBeRetried(SuspectId suspect)
        {
            UnlockAccusation(
                MysteryManager.KeycardId,
                MysteryManager.LogbookId,
                MysteryManager.PhoneId,
                MysteryManager.CctvId);
            AccusationResult first = mysteryManager.TryAccuse(
                suspect,
                Array.Empty<string>());
            AccusationResult retry = mysteryManager.TryAccuse(
                SuspectId.VikramDesai,
                Array.Empty<string>());

            Assert(first == AccusationResult.WrongSuspect &&
                   mysteryManager.CaseState == MysteryCaseState.Failed &&
                   retry == AccusationResult.AlreadyResolved,
                "A wrong accusation must permanently fail the case.");
        }

        private void TestAccusationLocationGate()
        {
            Collect(
                MysteryManager.KeycardId,
                MysteryManager.LogbookId,
                MysteryManager.PhoneId,
                MysteryManager.CctvId);
            InterviewAllSuspects();

            AccusationResult outsideLocation = mysteryManager.TryAccuse(
                SuspectId.VikramDesai,
                new[]
                {
                    MysteryManager.KeycardId,
                    MysteryManager.CctvId,
                    MysteryManager.GrantId
                });
            mysteryManager.EnterLocation(MysteryLocation.MeetingRoom);
            AccusationResult meetingRoom = mysteryManager.TryAccuse(
                SuspectId.VikramDesai,
                new[]
                {
                    MysteryManager.KeycardId,
                    MysteryManager.CctvId,
                    MysteryManager.GrantId
                });

            Assert(outsideLocation == AccusationResult.Locked &&
                   meetingRoom == AccusationResult.InvalidEvidence,
                "Accusation evaluation must require an approved location and " +
                "reject uncollected submitted evidence.");
        }

        private void TestDecisiveEvidenceIsMandatory()
        {
            UnlockAccusation(
                MysteryManager.KeycardId,
                MysteryManager.CctvId,
                MysteryManager.GrantId,
                MysteryManager.PowerId);

            string[] decisive =
            {
                MysteryManager.KeycardId,
                MysteryManager.CctvId,
                MysteryManager.GrantId
            };

            foreach (string missingEvidence in decisive)
            {
                var submitted = new List<string>();
                foreach (string evidenceId in decisive)
                {
                    if (evidenceId != missingEvidence)
                        submitted.Add(evidenceId);
                }

                submitted.Add(MysteryManager.PowerId);
                AccusationResult result = mysteryManager.TryAccuse(
                    SuspectId.VikramDesai,
                    submitted.AsReadOnly());
                Assert(result == AccusationResult.Inconclusive,
                    $"Omitting {missingEvidence} must not produce a victory.");
                Assert(mysteryManager.CaseState == MysteryCaseState.Inconclusive,
                    "Insufficient evidence must remain retryable.");
            }

            AccusationResult finalResult = mysteryManager.TryAccuse(
                SuspectId.VikramDesai,
                decisive);
            Assert(finalResult == AccusationResult.Solved &&
                   mysteryManager.CaseState == MysteryCaseState.Solved,
                "Submitting exactly the decisive trio should solve the case.");
        }

        private void TestCollectionIsNotPresentation()
        {
            UnlockAccusation(
                MysteryManager.KeycardId,
                MysteryManager.CctvId,
                MysteryManager.GrantId,
                MysteryManager.PowerId);

            Assert(!mysteryManager.IsEvidencePresented(MysteryManager.KeycardId) &&
                   !mysteryManager.IsEvidencePresented(MysteryManager.CctvId) &&
                   !mysteryManager.IsEvidencePresented(MysteryManager.GrantId),
                "Collecting the decisive trio must not count as presenting it.");
            AccusationResult result = mysteryManager.TryAccuse(
                SuspectId.VikramDesai,
                Array.Empty<string>());
            Assert(result == AccusationResult.Inconclusive &&
                   mysteryManager.CaseState == MysteryCaseState.Inconclusive,
                "Collecting decisive evidence without submitting it must not solve the case.");
        }

        private void TestPriyaConfrontationDependencies()
        {
            Collect(MysteryManager.LogbookId, MysteryManager.PowerId);
            Interview(SuspectId.PriyaSharma);
            Assert(!mysteryManager.IsEvidencePresentedTo(
                       SuspectId.PriyaSharma, MysteryManager.LogbookId),
                "Collecting the logbook must not mark it presented.");

            string firstReply = mysteryManager.PresentEvidence(
                SuspectId.PriyaSharma,
                MysteryManager.LogbookId);
            Assert(firstReply.Contains("People argue in labs") &&
                   mysteryManager.GetObjectiveStatus("OBJ_7") ==
                   ObjectiveStatus.Active,
                "The logbook alone should not trigger Priya's confrontation.");

            string confrontation = mysteryManager.PresentEvidence(
                SuspectId.PriyaSharma,
                MysteryManager.PowerId);
            Assert(confrontation.Contains("22:50") &&
                   confrontation.Contains("my account") &&
                   !confrontation.Contains("telemetry proves you left"),
                "The combined confrontation must keep the telemetry and admission distinct.");
            Assert(mysteryManager.PresentEvidence(
                       SuspectId.PriyaSharma,
                       MysteryManager.PowerId) == confrontation,
                "Reviewing presented evidence must not repeat or alter the confrontation.");
        }

        private void TestGrantEvidenceEstablishesMotive()
        {
            Collect(MysteryManager.GrantId);
            Assert(mysteryManager.GetObjectiveStatus("OBJ_8") ==
                   ObjectiveStatus.Complete,
                "Discovering the grant filing should establish the motive objective.");
        }

        private void TestVikramConfrontationOrder()
        {
            Collect(
                MysteryManager.KeycardId,
                MysteryManager.CctvId,
                MysteryManager.GrantId);
            Interview(SuspectId.VikramDesai);

            mysteryManager.PresentEvidence(
                SuspectId.VikramDesai,
                MysteryManager.GrantId);
            mysteryManager.PresentEvidence(
                SuspectId.VikramDesai,
                MysteryManager.KeycardId);
            string confession = mysteryManager.PresentEvidence(
                SuspectId.VikramDesai,
                MysteryManager.CctvId);

            Assert(confession.Contains("I shoved her away") &&
                   confession.Contains("circuit breakers") &&
                   mysteryManager.GetObjectiveStatus("OBJ_8") ==
                   ObjectiveStatus.Complete,
                "Presenting all decisive evidence in any order should unlock Vikram's confession.");
        }

        private void TestRohanAndKarthikConfrontations()
        {
            Collect(MysteryManager.CctvId);
            Interview(SuspectId.RohanKapoor);
            Interview(SuspectId.KarthikNair);

            string rohanReply = mysteryManager.PresentEvidence(
                SuspectId.RohanKapoor,
                MysteryManager.CctvId);
            string repeatedRohanReply = mysteryManager.PresentEvidence(
                SuspectId.RohanKapoor,
                MysteryManager.CctvId);
            string karthikReply = mysteryManager.PresentEvidence(
                SuspectId.KarthikNair,
                MysteryManager.CctvId);

            Assert(rohanReply.Contains("no later exit scan") &&
                   !rohanReply.Contains("continuous presence") &&
                   rohanReply == repeatedRohanReply,
                "Rohan's admission must distinguish badge scans from continuous presence.");
            Assert(karthikReply.Contains("doesn't prove who bought it") &&
                   mysteryManager.GetObjectiveStatus("OBJ_7") ==
                   ObjectiveStatus.Complete,
                "Karthik's admission must distinguish the anonymous receipt from his location.");
        }

        private void TestOrderIndependentProgress()
        {
            Collect(
                MysteryManager.CctvId,
                MysteryManager.GrantId,
                MysteryManager.PowerId,
                MysteryManager.PhoneId);
            mysteryManager.EnterLocation(MysteryLocation.IoTLab1112);
            Interview(SuspectId.KarthikNair);
            Interview(SuspectId.VikramDesai);
            Interview(SuspectId.PriyaSharma);
            Interview(SuspectId.RohanKapoor);

            Assert(mysteryManager.GetObjectiveStatus("OBJ_1") ==
                   ObjectiveStatus.Complete &&
                   mysteryManager.GetObjectiveStatus("OBJ_3") ==
                   ObjectiveStatus.Complete &&
                   mysteryManager.CanAccuse,
                "Objectives and accusation unlocks must update regardless of visit order.");
        }

        private void UnlockAccusation(params string[] evidenceIds)
        {
            Collect(evidenceIds);
            InterviewAllSuspects();
            Assert(mysteryManager.CanAccuse,
                "Test setup did not meet accusation prerequisites.");
            mysteryManager.EnterLocation(MysteryLocation.Foyer);
        }

        private void Collect(params string[] evidenceIds)
        {
            foreach (string evidenceId in evidenceIds)
            {
                EvidenceItem item = EvidenceItem.CreateInstance(
                    evidenceId,
                    evidenceId,
                    "Test evidence",
                    EvidenceCategory.Document);
                createdEvidence.Add(item);
                evidenceManager.TryCollectEvidence(item);
            }
        }

        private void InterviewAllSuspects()
        {
            Interview(SuspectId.RohanKapoor);
            Interview(SuspectId.PriyaSharma);
            Interview(SuspectId.VikramDesai);
            Interview(SuspectId.KarthikNair);
        }

        private void Interview(SuspectId suspect)
        {
            mysteryManager.BeginInterview(suspect);
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
