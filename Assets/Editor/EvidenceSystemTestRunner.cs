using System;
using UnityEditor;
using UnityEngine;
using MurderMystery;
using MurderMystery.Tests;

namespace MurderMystery.Editor
{
    public static class EvidenceSystemTestRunner
    {
        [MenuItem("Murder Mystery/Run Evidence System Tests")]
        public static void RunTestsMenu()
        {
            RunTestsInternal(isCommandLine: false);
        }

        public static void RunTests()
        {
            RunTestsInternal(isCommandLine: true);
        }

        private static void RunTestsInternal(bool isCommandLine)
        {
            Debug.Log("[TestRunner] Starting Evidence System Verification Suite in Unity Editor...");

            GameObject testObj = new GameObject("EvidenceSystem_AutomatedTestRunner");
            var testHarness = testObj.AddComponent<EvidenceSystemTests>();

            bool allPassed = false;
            try
            {
                allPassed = testHarness.RunAllTests();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TestRunner] Unhandled exception during test execution: {ex}");
                allPassed = false;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(testObj);
                if (EvidenceManager.Instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(EvidenceManager.Instance.gameObject);
                }
            }

            if (allPassed)
            {
                Debug.Log("[TestRunner] >>> ALL EVIDENCE SYSTEM TESTS PASSED SUCCESSFULLY! <<<");
                if (isCommandLine)
                {
                    EditorApplication.Exit(0);
                }
            }
            else
            {
                Debug.LogError("[TestRunner] >>> SOME EVIDENCE SYSTEM TESTS FAILED! <<<");
                if (isCommandLine)
                {
                    EditorApplication.Exit(1);
                }
            }
        }
    }
}
