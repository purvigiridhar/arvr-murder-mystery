using System;
using System.Collections.Generic;
using UnityEngine;

namespace MurderMystery.Tests
{
    /// <summary>
    /// Automated test harness for the Evidence Collection & Inventory System.
    /// Can be executed in Unity Play Mode or via batchmode test runner.
    /// </summary>
    public class EvidenceSystemTests : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = true;

        public struct TestResult
        {
            public string TestName;
            public bool Passed;
            public string Message;
        }

        public readonly List<TestResult> Results = new List<TestResult>();

        private void Start()
        {
            if (runTestsOnStart)
            {
                RunAllTests();
            }
        }

        public bool RunAllTests()
        {
            Results.Clear();
            Debug.Log("==================================================");
            Debug.Log("[EvidenceSystemTests] STARTING VERIFICATION SUITE");
            Debug.Log("==================================================");

            Test_EvidenceItem_Creation();
            Test_EvidenceManager_InitialEmptyState();
            Test_EvidenceManager_CollectSingleClue();
            Test_EvidenceManager_DuplicatePrevention();
            Test_EvidenceManager_MultipleDistinctClues();
            Test_EvidenceManager_EventFiring();
            Test_EvidenceManager_NullAndInvalidInputResilience();
            Test_EvidenceCollectible_InteractableIntegration();
            Test_EvidenceCollectible_MissingItemHandling();
            Test_EvidenceCollectible_PreventRepeatedInteraction();
            Test_EvidenceJournalUI_CursorAndToggle();

            int passed = 0;
            int failed = 0;

            foreach (var r in Results)
            {
                if (r.Passed)
                {
                    passed++;
                    Debug.Log($"[PASS] {r.TestName}");
                }
                else
                {
                    failed++;
                    Debug.LogError($"[FAIL] {r.TestName} - {r.Message}");
                }
            }

            Debug.Log("==================================================");
            Debug.Log($"[EvidenceSystemTests] SUMMARY: {passed} PASSED, {failed} FAILED");
            Debug.Log("==================================================");

            return failed == 0;
        }

        private void Assert(bool condition, string testName, string failureMessage)
        {
            Results.Add(new TestResult
            {
                TestName = testName,
                Passed = condition,
                Message = condition ? "Success" : failureMessage
            });
        }

        private EvidenceManager GetOrCreateManager()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.ClearCollectedEvidence();
                return EvidenceManager.Instance;
            }

            GameObject go = new GameObject("EvidenceManager_TestInstance");
            var mgr = go.AddComponent<EvidenceManager>();
            EvidenceManager.Instance = mgr;
            mgr.ClearCollectedEvidence();
            return mgr;
        }

        // Test 1: EvidenceItem Creation and Property Validation
        private void Test_EvidenceItem_Creation()
        {
            string id = "EV-TEST-001";
            string name = "Security Pass";
            string desc = "Access pass found near laboratory.";
            var item = EvidenceItem.CreateInstance(id, name, desc, EvidenceCategory.Digital);

            bool ok = item != null &&
                      item.EvidenceId == id &&
                      item.DisplayName == name &&
                      item.Description == desc &&
                      item.Category == EvidenceCategory.Digital;

            Assert(ok, "Test_EvidenceItem_Creation", "EvidenceItem properties did not match constructor values.");
        }

        // Test 2: Initial Empty State
        private void Test_EvidenceManager_InitialEmptyState()
        {
            var mgr = GetOrCreateManager();
            mgr.ClearCollectedEvidence();

            bool ok = mgr.CollectedCount == 0 &&
                      mgr.GetCollectedEvidence().Count == 0 &&
                      !mgr.HasEvidence("EV-NONEXISTENT") &&
                      mgr.GetEvidence("EV-NONEXISTENT") == null;

            Assert(ok, "Test_EvidenceManager_InitialEmptyState", "Initial manager state is not empty.");
        }

        // Test 3: Collect Single Clue
        private void Test_EvidenceManager_CollectSingleClue()
        {
            var mgr = GetOrCreateManager();
            mgr.ClearCollectedEvidence();

            var clue = EvidenceItem.CreateInstance("EV-TEST-002", "Fingerprint Lift", "Forensic lift from glass door.", EvidenceCategory.Physical);
            bool collected = mgr.TryCollectEvidence(clue);

            bool ok = collected &&
                      mgr.CollectedCount == 1 &&
                      mgr.HasEvidence("EV-TEST-002") &&
                      mgr.GetEvidence("EV-TEST-002") == clue;

            Assert(ok, "Test_EvidenceManager_CollectSingleClue", "Failed to collect single evidence clue.");
        }

        // Test 4: Duplicate Prevention
        private void Test_EvidenceManager_DuplicatePrevention()
        {
            var mgr = GetOrCreateManager();
            mgr.ClearCollectedEvidence();

            var clue = EvidenceItem.CreateInstance("EV-TEST-003", "Audio Tape", "Recording of corridor conversation.", EvidenceCategory.Digital);
            bool firstAttempt = mgr.TryCollectEvidence(clue);
            bool secondAttempt = mgr.TryCollectEvidence(clue);

            // Attempt with different instance but identical ID
            var duplicateIdClue = EvidenceItem.CreateInstance("EV-TEST-003", "Audio Tape Copy", "Duplicate ID item.", EvidenceCategory.Digital);
            bool thirdAttempt = mgr.TryCollectEvidence(duplicateIdClue);

            bool ok = firstAttempt &&
                      !secondAttempt &&
                      !thirdAttempt &&
                      mgr.CollectedCount == 1;

            Assert(ok, "Test_EvidenceManager_DuplicatePrevention", "Duplicate collection was not prevented.");
        }

        // Test 5: Multiple Distinct Clues
        private void Test_EvidenceManager_MultipleDistinctClues()
        {
            var mgr = GetOrCreateManager();
            mgr.ClearCollectedEvidence();

            var clueA = EvidenceItem.CreateInstance("EV-A", "Clue A", "Desc A", EvidenceCategory.Physical);
            var clueB = EvidenceItem.CreateInstance("EV-B", "Clue B", "Desc B", EvidenceCategory.Digital);
            var clueC = EvidenceItem.CreateInstance("EV-C", "Clue C", "Desc C", EvidenceCategory.Witness);

            mgr.TryCollectEvidence(clueA);
            mgr.TryCollectEvidence(clueB);
            mgr.TryCollectEvidence(clueC);

            var list = mgr.GetCollectedEvidence();
            bool ok = mgr.CollectedCount == 3 &&
                      list.Count == 3 &&
                      list[0].EvidenceId == "EV-A" &&
                      list[1].EvidenceId == "EV-B" &&
                      list[2].EvidenceId == "EV-C";

            Assert(ok, "Test_EvidenceManager_MultipleDistinctClues", "Multiple clues not stored independently or in order.");
        }

        // Test 6: Event Firing
        private void Test_EvidenceManager_EventFiring()
        {
            var mgr = GetOrCreateManager();
            mgr.ClearCollectedEvidence();

            int eventCallCount = 0;
            EvidenceItem receivedItem = null;

            Action<EvidenceItem> handler = (item) =>
            {
                eventCallCount++;
                receivedItem = item;
            };

            mgr.OnEvidenceCollected += handler;

            var clue = EvidenceItem.CreateInstance("EV-EVENT", "Fired Event Clue", "Test Event", EvidenceCategory.Document);
            mgr.TryCollectEvidence(clue);

            // Try duplicate collection - should NOT fire event
            mgr.TryCollectEvidence(clue);

            mgr.OnEvidenceCollected -= handler;

            bool ok = (eventCallCount == 1) && (receivedItem == clue);
            Assert(ok, "Test_EvidenceManager_EventFiring", $"OnEvidenceCollected expected 1 call, but got {eventCallCount}.");
        }

        // Test 7: Null and Invalid Input Handling
        private void Test_EvidenceManager_NullAndInvalidInputResilience()
        {
            var mgr = GetOrCreateManager();
            mgr.ClearCollectedEvidence();

            bool nullCollected = mgr.TryCollectEvidence(null);
            var emptyIdClue = EvidenceItem.CreateInstance("", "Blank ID", "Blank ID Desc");
            bool emptyIdCollected = mgr.TryCollectEvidence(emptyIdClue);
            var whitespaceIdClue = EvidenceItem.CreateInstance("   ", "Spaces ID", "Spaces ID Desc");
            bool whitespaceIdCollected = mgr.TryCollectEvidence(whitespaceIdClue);

            bool ok = !nullCollected &&
                      !emptyIdCollected &&
                      !whitespaceIdCollected &&
                      mgr.CollectedCount == 0 &&
                      !mgr.HasEvidence(null) &&
                      !mgr.HasEvidence("") &&
                      mgr.GetEvidence(null) == null;

            Assert(ok, "Test_EvidenceManager_NullAndInvalidInputResilience", "Manager crashed or accepted invalid/null IDs.");
        }

        // Test 8: EvidenceCollectible and IInteractable Integration
        private void Test_EvidenceCollectible_InteractableIntegration()
        {
            var mgr = GetOrCreateManager();
            mgr.ClearCollectedEvidence();

            GameObject obj = new GameObject("TestEvidenceObject");
            var col = obj.AddComponent<BoxCollider>();
            var collectible = obj.AddComponent<EvidenceCollectible>();

            var clue = EvidenceItem.CreateInstance("EV-COLLECTIBLE-01", "Lab Report", "Torn report page.", EvidenceCategory.Document);
            collectible.SetEvidenceItem(clue);

            // Verify IInteractable interface contract
            IInteractable interactable = collectible;
            string prompt = interactable.InteractionPrompt;
            bool promptValid = !string.IsNullOrEmpty(prompt) && prompt.Contains("Lab Report");

            // Execute Interact() as InteractionRaycaster does
            interactable.Interact();

            bool collectedInMgr = mgr.HasEvidence("EV-COLLECTIBLE-01");
            bool markedCollected = collectible.IsCollected;

            DestroyImmediate(obj);

            bool ok = promptValid && collectedInMgr && markedCollected;
            Assert(ok, "Test_EvidenceCollectible_InteractableIntegration", "EvidenceCollectible failed IInteractable integration.");
        }

        // Test 9: EvidenceCollectible Missing Item Handling
        private void Test_EvidenceCollectible_MissingItemHandling()
        {
            GameObject obj = new GameObject("TestEvidenceObject_MissingItem");
            var collectible = obj.AddComponent<EvidenceCollectible>();
            collectible.SetEvidenceItem(null);

            IInteractable interactable = collectible;
            string prompt = interactable.InteractionPrompt;

            // Should not throw exception
            bool didNotCrash = true;
            try
            {
                interactable.Interact();
            }
            catch (Exception)
            {
                didNotCrash = false;
            }

            DestroyImmediate(obj);

            bool ok = didNotCrash && !string.IsNullOrEmpty(prompt);
            Assert(ok, "Test_EvidenceCollectible_MissingItemHandling", "EvidenceCollectible threw exception on null item.");
        }

        // Test 10: Prevent Repeated Interaction
        private void Test_EvidenceCollectible_PreventRepeatedInteraction()
        {
            var mgr = GetOrCreateManager();
            mgr.ClearCollectedEvidence();

            GameObject obj = new GameObject("TestEvidenceObject_Repeat");
            var collectible = obj.AddComponent<EvidenceCollectible>();
            var clue = EvidenceItem.CreateInstance("EV-REPEAT-01", "Repeated Clue", "Test Repeat", EvidenceCategory.Physical);
            collectible.SetEvidenceItem(clue);

            IInteractable interactable = collectible;
            interactable.Interact();
            int countAfterFirst = mgr.CollectedCount;

            // Second interaction
            interactable.Interact();
            int countAfterSecond = mgr.CollectedCount;

            DestroyImmediate(obj);

            bool ok = (countAfterFirst == 1) && (countAfterSecond == 1);
            Assert(ok, "Test_EvidenceCollectible_PreventRepeatedInteraction", "Repeated interaction was not blocked.");
        }

        // Test 11: Journal UI Toggle and Cursor Management
        private void Test_EvidenceJournalUI_CursorAndToggle()
        {
            GameObject uiObj = new GameObject("TestJournalUI");
            var journal = uiObj.AddComponent<EvidenceJournalUI>();

            bool initiallyClosed = !journal.IsOpen;

            journal.OpenJournal();
            bool opened = journal.IsOpen;
            bool cursorUnlocked = (Cursor.lockState == CursorLockMode.None) && Cursor.visible;

            journal.CloseJournal();
            bool closed = !journal.IsOpen;
            bool cursorLocked = (Cursor.lockState == CursorLockMode.Locked) && !Cursor.visible;

            DestroyImmediate(uiObj);

            bool ok = initiallyClosed && opened && closed;
            if (!Application.isBatchMode)
            {
                ok = ok && cursorUnlocked && cursorLocked;
            }

            Assert(ok, "Test_EvidenceJournalUI_CursorAndToggle", "EvidenceJournalUI failed open/close toggle.");
        }
    }
}
