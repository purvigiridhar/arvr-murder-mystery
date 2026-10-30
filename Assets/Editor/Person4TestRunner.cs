using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MurderMystery.Tests;

namespace MurderMystery.Editor
{
    /// <summary>
    /// Person 4 Test Runner:
    /// Executes the full verification suite for Person 4 integration, UI, and audio systems,
    /// as well as regression tests for Person 2's evidence system.
    /// Can be executed from the Unity menu bar or CLI batchmode.
    /// </summary>
    public static class Person4TestRunner
    {
        [MenuItem("Murder Mystery/Run Person 4 Integration Tests")]
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
            Debug.Log("[Person4TestRunner] Initiating Person 4 Integration Verification...");

            // 1. Ensure Floor 11 scene is integrated
            EnvironmentEvidenceIntegrator.IntegrateMainEnvironment();

            // 2. Open Main Environment Scene
            EditorSceneManager.OpenScene("Assets/Scenes/MurderMystery_MainEnvironment.unity", OpenSceneMode.Single);

            // 3. Run Person 4 Tests
            GameObject p4TestObj = new GameObject("Person4_AutomatedTestRunner");
            var p4Harness = p4TestObj.AddComponent<Person4IntegrationTests>();

            bool p4Passed = false;
            try
            {
                p4Passed = p4Harness.RunAllTests();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Person4TestRunner] Exception during Person 4 tests: {ex}");
                p4Passed = false;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(p4TestObj);
            }

            // 4. Run Person 2 Regression Tests
            GameObject p2TestObj = new GameObject("Person2_RegressionTestRunner");
            var p2Harness = p2TestObj.AddComponent<EvidenceSystemTests>();

            bool p2Passed = false;
            try
            {
                p2Passed = p2Harness.RunAllTests();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Person4TestRunner] Exception during Person 2 regression tests: {ex}");
                p2Passed = false;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(p2TestObj);
            }

            bool allPassed = p4Passed && p2Passed;

            if (allPassed)
            {
                Debug.Log(">>> ALL PERSON 4 & PERSON 2 TESTS PASSED SUCCESSFULLY! <<<");
                if (isCommandLine) EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError(">>> ONE OR MORE VERIFICATION TESTS FAILED! <<<");
                if (isCommandLine) EditorApplication.Exit(1);
            }
        }
    }
}

