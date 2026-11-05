using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MurderMystery;
using MurderMystery.Tests;

namespace MurderMystery.Editor
{
    public static class MysterySystemTestRunner
    {
        [MenuItem("Murder Mystery/Run Evidence and Mystery Tests")]
        public static void RunTests()
        {
            EvidenceManager existingManager = EvidenceManager.Instance;
            List<EvidenceItem> originalEvidence = existingManager == null
                ? null
                : new List<EvidenceItem>(existingManager.GetCollectedEvidence());
            bool allPassed = false;
            try
            {
                bool evidencePassed = RunEvidenceTests(existingManager);
                bool mysteryPassed = RunMysteryTests(existingManager);
                allPassed = evidencePassed && mysteryPassed;
            }
            finally
            {
                RestoreExistingEvidence(existingManager, originalEvidence);
            }

            Debug.Log(
                allPassed
                    ? "[MysterySystemTestRunner] All evidence and mystery tests passed."
                    : "[MysterySystemTestRunner] One or more test suites failed.");

            if (Application.isBatchMode)
                EditorApplication.Exit(allPassed ? 0 : 1);
        }

        private static bool RunEvidenceTests(
            EvidenceManager existingManager)
        {
            GameObject testObject =
                new GameObject("EvidenceSystem_AutomatedTestRunner");
            bool passed = false;
            try
            {
                passed = testObject.AddComponent<EvidenceSystemTests>()
                    .RunAllTests();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[MysterySystemTestRunner] Evidence test exception: {exception}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(testObject);
                if (existingManager == null &&
                    EvidenceManager.Instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        EvidenceManager.Instance.gameObject);
                }
            }

            return passed;
        }

        private static bool RunMysteryTests(
            EvidenceManager existingManager)
        {
            GameObject testObject =
                new GameObject("MysterySystem_AutomatedTestRunner");
            bool passed = false;
            try
            {
                passed = testObject.AddComponent<MysterySystemTests>()
                    .RunAllTests();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[MysterySystemTestRunner] Mystery test exception: {exception}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(testObject);
                if (existingManager == null &&
                    EvidenceManager.Instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        EvidenceManager.Instance.gameObject);
                }
            }

            return passed;
        }

        private static void RestoreExistingEvidence(
            EvidenceManager manager,
            IReadOnlyList<EvidenceItem> evidence)
        {
            if (manager == null || evidence == null)
                return;

            manager.ClearCollectedEvidence();
            foreach (EvidenceItem item in evidence)
                manager.TryCollectEvidence(item);
        }
    }
}
