using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MurderMystery;
using MurderMystery.Tests;

namespace MurderMystery.Editor
{
    /// <summary>
    /// Person 4 Complete Integrated Test Runner:
    /// Executes the full suite of verification tests across the entire project:
    /// 1. Person 2 Evidence Collection & Journal Tests
    /// 2. Person 3 Mystery Progression, Dialogue, and Accusation Tests
    /// 3. Person 4 UI/UX Polish, Audio Feedback, and Interaction Tests
    /// </summary>
    public static class CompleteGameTestRunner
    {
        [MenuItem("Murder Mystery/Run All Game Tests (Full Suite)")]
        public static void RunAllTests()
        {
            try
            {
                Floor11CompleteOverhaul.ExecuteOverhaul();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CompleteGameTestRunner] Failed to run complete environment overhaul: {ex}");
            }

            EvidenceManager existingManager = EvidenceManager.Instance;
            List<EvidenceItem> originalEvidence = existingManager == null
                ? null
                : new List<EvidenceItem>(existingManager.GetCollectedEvidence());

            bool allPassed = false;
            try
            {
                bool evidencePassed = RunEvidenceTests(existingManager);
                bool mysteryPassed = RunMysteryTests(existingManager);
                bool person4Passed = RunPerson4Tests();

                allPassed = evidencePassed && mysteryPassed && person4Passed;
            }
            finally
            {
                RestoreExistingEvidence(existingManager, originalEvidence);
            }

            Debug.Log(
                allPassed
                    ? "[CompleteGameTestRunner] >>> ALL GAME TESTS (P2, P3, P4) PASSED SUCCESSFULLY! <<<"
                    : "[CompleteGameTestRunner] >>> ONE OR MORE TEST SUITES FAILED! <<<");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(allPassed ? 0 : 1);
            }
        }

        private static bool RunEvidenceTests(EvidenceManager existingManager)
        {
            GameObject testObject = new GameObject("EvidenceSystem_AutomatedTestRunner");
            bool passed = false;
            try
            {
                passed = testObject.AddComponent<EvidenceSystemTests>().RunAllTests();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CompleteGameTestRunner] Evidence test exception: {ex}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(testObject);
                if (existingManager == null && EvidenceManager.Instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(EvidenceManager.Instance.gameObject);
                }
            }
            return passed;
        }

        private static bool RunMysteryTests(EvidenceManager existingManager)
        {
            GameObject testObject = new GameObject("MysterySystem_AutomatedTestRunner");
            bool passed = false;
            try
            {
                passed = testObject.AddComponent<MysterySystemTests>().RunAllTests();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CompleteGameTestRunner] Mystery test exception: {ex}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(testObject);
                if (existingManager == null && EvidenceManager.Instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(EvidenceManager.Instance.gameObject);
                }
            }
            return passed;
        }

        private static bool RunPerson4Tests()
        {
            GameObject testObject = new GameObject("Person4_AutomatedTestRunner");
            bool passed = false;
            try
            {
                passed = testObject.AddComponent<Person4ComprehensiveTests>().RunAllTests();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CompleteGameTestRunner] Person 4 test exception: {ex}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(testObject);
            }
            return passed;
        }

        private static void RestoreExistingEvidence(EvidenceManager manager, IReadOnlyList<EvidenceItem> evidence)
        {
            if (manager == null || evidence == null) return;
            manager.ClearCollectedEvidence();
            foreach (EvidenceItem item in evidence)
            {
                manager.TryCollectEvidence(item);
            }
        }
    }
}

