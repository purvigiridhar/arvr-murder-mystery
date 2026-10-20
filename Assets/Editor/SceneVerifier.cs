using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MurderMystery.Editor
{
    public static class SceneVerifier
    {
        public static void VerifyScene()
        {
            string scenePath = "Assets/Scenes/MurderMystery_MainEnvironment.unity";
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Debug.Log("=================================================");
            Debug.Log("VERIFYING ASSEMBLED UNITY ENVIRONMENT SCENE");
            Debug.Log("=================================================");

            int totalObjects = 0;
            int totalRenderers = 0;
            int disabledRenderers = 0;
            int totalColliders = 0;

            Bounds totalBounds = new Bounds(Vector3.zero, Vector3.zero);
            bool firstBound = true;

            Dictionary<string, int> rootChildCounts = new Dictionary<string, int>();

            foreach (GameObject rootGo in scene.GetRootGameObjects())
            {
                rootChildCounts[rootGo.name] = rootGo.transform.childCount;
                
                MeshRenderer[] renderers = rootGo.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mr in renderers)
                {
                    totalRenderers++;
                    if (!mr.enabled) disabledRenderers++;
                    else
                    {
                        if (firstBound)
                        {
                            totalBounds = mr.bounds;
                            firstBound = false;
                        }
                        else
                        {
                            totalBounds.Encapsulate(mr.bounds);
                        }
                    }
                }

                Collider[] colliders = rootGo.GetComponentsInChildren<Collider>(true);
                totalColliders += colliders.Length;

                totalObjects += rootGo.GetComponentsInChildren<Transform>(true).Length;
            }

            GameObject player = GameObject.Find("Player_FirstPersonController");
            GameObject spawn = GameObject.Find("PlayerSpawn");
            bool playerValid = (player != null && player.GetComponent<CharacterController>() != null);
            bool spawnValid = (spawn != null);

            Debug.Log($"Scene Name: {scene.name}");
            Debug.Log($"Total GameObjects in Scene: {totalObjects}");
            Debug.Log($"Total MeshRenderers: {totalRenderers} (Active: {totalRenderers - disabledRenderers}, Disabled/Deduplicated: {disabledRenderers})");
            Debug.Log($"Total Colliders: {totalColliders}");
            Debug.Log($"Building Bounds Center: {totalBounds.center}, Size: {totalBounds.size}");
            Debug.Log($"Player Controller Valid: {playerValid}, Player Spawn Valid: {spawnValid}");

            foreach (var kvp in rootChildCounts)
            {
                Debug.Log($"  Section '{kvp.Key}': {kvp.Value} child objects");
            }

            // Write verification JSON to Reports
            string reportJsonPath = @"c:\Users\DARSHANA\OneDrive\Documents\MurderMystery\Reports\scene_verification.json";
            string json = $@"{{
  ""scene_name"": ""{scene.name}"",
  ""scene_path"": ""{scenePath}"",
  ""total_scene_objects"": {totalObjects},
  ""total_mesh_renderers"": {totalRenderers},
  ""active_mesh_renderers"": {totalRenderers - disabledRenderers},
  ""disabled_duplicate_renderers"": {disabledRenderers},
  ""total_colliders"": {totalColliders},
  ""building_bounds"": {{
    ""center"": [{totalBounds.center.x}, {totalBounds.center.y}, {totalBounds.center.z}],
    ""size"": [{totalBounds.size.x}, {totalBounds.size.y}, {totalBounds.size.z}]
  }},
  ""player_controller_present"": {playerValid.ToString().ToLower()},
  ""player_spawn_present"": {spawnValid.ToString().ToLower()}
}}";

            File.WriteAllText(reportJsonPath, json);
            Debug.Log($"Verification output written to: {reportJsonPath}");
            Debug.Log("=================================================");
            Debug.Log("VERIFICATION TEST FINISHED SUCCESSFULLY!");
            Debug.Log("=================================================");
        }
    }
}
