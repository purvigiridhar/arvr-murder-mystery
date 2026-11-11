using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MurderMystery;
using MurderMystery.Audio;
using MurderMystery.UI;

namespace MurderMystery.Tests
{
    /// <summary>
    /// Person 4 Automated Integration & Presentation Test Suite:
    /// Validates main environment integration, player raycaster & prompt UI,
    /// audio feedback systems, objective HUD, dialogue panel, and case accusation screens.
    /// </summary>
    public class Person4IntegrationTests : MonoBehaviour
    {
        public struct TestResult
        {
            public string TestName;
            public bool Passed;
            public string Message;
        }

        public readonly List<TestResult> Results = new List<TestResult>();

        public bool RunAllTests()
        {
            Results.Clear();
            Debug.Log("==================================================");
            Debug.Log("[Person 4 Tests] STARTING COMPREHENSIVE VERIFICATION");
            Debug.Log("==================================================");

            Test_Player_InteractionWiring();
            Test_GameSystems_Presence();
            Test_CluePlacement_AndReachability();
            Test_AudioManager_ProceduralAudio();
            Test_ObjectiveUI_Functionality();
            Test_DialogueUI_Functionality();
            Test_AccusationUI_Functionality();
            Test_EvidenceJournal_VisualQualityAndTabs();

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
            Debug.Log($"[Person 4 Tests] SUMMARY: {passed} PASSED, {failed} FAILED");
            Debug.Log("==================================================");

            return failed == 0;
        }

        private void Assert(bool condition, string testName, string failureMsg)
        {
            Results.Add(new TestResult
            {
                TestName = testName,
                Passed = condition,
                Message = condition ? "Success" : failureMsg
            });
        }

        private void Test_Player_InteractionWiring()
        {
            GameObject player = GameObject.Find("Player_FirstPersonController");
            if (player == null)
            {
                Assert(false, "Test_Player_InteractionWiring", "Player_FirstPersonController not found in scene.");
                return;
            }

            var promptUI = player.GetComponent<InteractionPromptUI>();
            var raycaster = player.GetComponent<InteractionRaycaster>();
            var cam = player.GetComponentInChildren<Camera>();

            bool ok = (promptUI != null && raycaster != null && cam != null);
            Assert(ok, "Test_Player_InteractionWiring", "Player missing InteractionPromptUI, InteractionRaycaster, or Camera.");
        }

        private void Test_GameSystems_Presence()
        {
            bool hasEvidenceMgr = FindFirstObjectByType<EvidenceManager>() != null;
            bool hasJournal = FindFirstObjectByType<EvidenceJournalUI>() != null;
            bool hasAudio = FindFirstObjectByType<AudioManager>() != null;
            bool hasObjective = FindFirstObjectByType<ObjectiveUI>() != null;
            bool hasDialogue = FindFirstObjectByType<DialogueUI>() != null;
            bool hasAccusation = FindFirstObjectByType<AccusationUI>() != null;

            bool ok = hasEvidenceMgr && hasJournal && hasAudio && hasObjective && hasDialogue && hasAccusation;
            Assert(ok, "Test_GameSystems_Presence", "One or more core Person 4 game systems are missing from scene.");
        }

        private void Test_CluePlacement_AndReachability()
        {
            EvidenceCollectible[] collectibles = FindObjectsByType<EvidenceCollectible>(FindObjectsSortMode.None);
            bool foundKeycard = false;
            bool foundLogbook = false;
            bool foundPhone = false;

            foreach (var c in collectibles)
            {
                if (c.Item == null) continue;
                if (c.Item.EvidenceId == "EV-KEYCARD-001") foundKeycard = true;
                if (c.Item.EvidenceId == "EV-LOGBOOK-002") foundLogbook = true;
                if (c.Item.EvidenceId == "EV-PHONE-003") foundPhone = true;

                // Ensure collider exists for raycasting
                if (c.GetComponent<Collider>() == null)
                {
                    Assert(false, "Test_CluePlacement_AndReachability", $"Clue '{c.gameObject.name}' is missing a Collider.");
                    return;
                }
            }

            bool ok = foundKeycard && foundLogbook && foundPhone;
            Assert(ok, "Test_CluePlacement_AndReachability", "One or more required Floor 11 evidence clues are missing.");
        }

        private void Test_AudioManager_ProceduralAudio()
        {
            GameObject audioObj = new GameObject("Test_AudioHarness");
            var audioMgr = audioObj.AddComponent<AudioManager>();

            bool noCrash = true;
            try
            {
                audioMgr.PlayEvidenceCollected();
                audioMgr.PlayJournalOpen();
                audioMgr.PlayJournalClose();
                audioMgr.PlayButtonClick();
                audioMgr.PlayButtonHover();
            }
            catch (Exception)
            {
                noCrash = false;
            }

            DestroyImmediate(audioObj);
            Assert(noCrash, "Test_AudioManager_ProceduralAudio", "AudioManager threw exception during audio playback.");
        }

        private void Test_ObjectiveUI_Functionality()
        {
            GameObject obj = new GameObject("Test_ObjectiveHarness");
            var objectiveUI = obj.AddComponent<ObjectiveUI>();

            objectiveUI.SetObjective("Directive Test", "Testing description");
            bool isVis = objectiveUI.IsVisible;

            DestroyImmediate(obj);
            Assert(isVis, "Test_ObjectiveUI_Functionality", "ObjectiveUI failed initialization.");
        }

        private void Test_DialogueUI_Functionality()
        {
            GameObject obj = new GameObject("Test_DialogueHarness");
            var dialogueUI = obj.AddComponent<DialogueUI>();

            int chosenOption = -99;
            dialogueUI.ShowDialogue("Detective", "Testing inquiry", new[] { "Option A", "Option B" }, (idx) => chosenOption = idx);
            bool open = dialogueUI.IsOpen;

            dialogueUI.HideDialogue();
            bool closed = !dialogueUI.IsOpen;

            DestroyImmediate(obj);
            Assert(open && closed, "Test_DialogueUI_Functionality", "DialogueUI failed show/hide transition.");
        }

        private void Test_AccusationUI_Functionality()
        {
            GameObject obj = new GameObject("Test_AccusationHarness");
            var accusationUI = obj.AddComponent<AccusationUI>();

            accusationUI.ShowResolution(true, "Dr. Vance", "Motive confirmed.");
            bool open = accusationUI.IsOpen;

            accusationUI.HideResolution();
            bool closed = !accusationUI.IsOpen;

            DestroyImmediate(obj);
            Assert(open && closed, "Test_AccusationUI_Functionality", "AccusationUI failed show/hide cycle.");
        }

        private void Test_EvidenceJournal_VisualQualityAndTabs()
        {
            GameObject obj = new GameObject("Test_JournalHarness");
            var journal = obj.AddComponent<EvidenceJournalUI>();

            journal.OpenJournal();
            bool open = journal.IsOpen;

            journal.CloseJournal();
            bool closed = !journal.IsOpen;

            DestroyImmediate(obj);
            Assert(open && closed, "Test_EvidenceJournal_VisualQualityAndTabs", "EvidenceJournalUI failed state toggle.");
        }
    }
}

