using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MurderMystery.Editor
{
    public static class EnvironmentRepairPass
    {
        [MenuItem("MurderMystery/Execute Environment Repair Pass")]
        public static void RepairEnvironment()
        {
            Debug.Log("=================================================");
            Debug.Log("STARTING ENVIRONMENT REPAIR PASS");
            Debug.Log("=================================================");

            string sourceScenePath = "Assets/Scenes/MurderMystery_MainEnvironment.unity";
            string fixedScenePath = "Assets/Scenes/MurderMystery_MainEnvironment_Fixed.unity";

            // 1. Open original scene as base (preserving it on disk)
            Scene scene = EditorSceneManager.OpenScene(sourceScenePath, OpenSceneMode.Single);

            // 2. Create Cohesive Neutral Material Palette
            EnsureFolder("Assets/Materials/Repaired");
            Material matWall = CreateMat("Assets/Materials/Repaired/M_Wall_CreamMatte.mat", new Color(0.82f, 0.80f, 0.76f), 0.05f);
            Material matFloor = CreateMat("Assets/Materials/Repaired/M_Floor_MediumGray.mat", new Color(0.35f, 0.35f, 0.37f), 0.15f);
            Material matCeiling = CreateMat("Assets/Materials/Repaired/M_Ceiling_DarkCharcoal.mat", new Color(0.18f, 0.18f, 0.20f), 0.02f);
            Material matWood = CreateMat("Assets/Materials/Repaired/M_Furniture_WarmWood.mat", new Color(0.36f, 0.24f, 0.15f), 0.20f);
            Material matMetal = CreateMat("Assets/Materials/Repaired/M_Furniture_DarkMetal.mat", new Color(0.20f, 0.21f, 0.23f), 0.45f);
            Material matFabric = CreateMat("Assets/Materials/Repaired/M_Furniture_Fabric.mat", new Color(0.22f, 0.25f, 0.28f), 0.05f);

            // 3. Restore Room Wall Renderers (Fix Disconnected Blocks / Gaps)
            GameObject rootRooms = GameObject.Find("[03_ROOMS]");
            int restoredCount = 0;
            if (rootRooms != null)
            {
                MeshRenderer[] allRoomRenderers = rootRooms.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mr in allRoomRenderers)
                {
                    // Re-enable all room renderers so interior partitions and door frames are visible
                    if (!mr.enabled)
                    {
                        mr.enabled = true;
                        restoredCount++;
                    }
                }
            }
            Debug.Log($"Restored {restoredCount} wall and fixture renderers across all room models.");

            // 4. Apply Consistent Architectural Materials to Prevent Blinding White Glare
            GameObject rootShell = GameObject.Find("[01_BUILDING_SHELL]");
            if (rootShell != null)
            {
                ApplyMaterialRecursively(rootShell, matWall);
            }

            GameObject rootFloors = GameObject.Find("[02_FLOORS]");
            if (rootFloors != null)
            {
                ApplyMaterialRecursively(rootFloors, matFloor);
            }

            // Assign materials across room objects based on name heuristics
            if (rootRooms != null)
            {
                MeshRenderer[] roomRenderers = rootRooms.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mr in roomRenderers)
                {
                    string n = mr.gameObject.name.ToLower();
                    if (n.Contains("chair") || n.Contains("seat") || n.Contains("stool"))
                    {
                        mr.sharedMaterial = matFabric;
                    }
                    else if (n.Contains("table") || n.Contains("desk") || n.Contains("shelf") || n.Contains("wood"))
                    {
                        mr.sharedMaterial = matWood;
                    }
                    else if (n.Contains("pc") || n.Contains("monitor") || n.Contains("line") || n.Contains("metal") || n.Contains("cable"))
                    {
                        mr.sharedMaterial = matMetal;
                    }
                    else if (n.Contains("floor"))
                    {
                        mr.sharedMaterial = matFloor;
                    }
                    else if (n.Contains("roof") || n.Contains("ceiling"))
                    {
                        mr.sharedMaterial = matCeiling;
                    }
                    else
                    {
                        // Walls, partitions, structural columns
                        mr.sharedMaterial = matWall;
                    }
                }
            }

            // Also format modular props
            GameObject rootProps = GameObject.Find("[04_FURNITURE_PROPS]");
            if (rootProps != null)
            {
                MeshRenderer[] propRenderers = rootProps.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mr in propRenderers)
                {
                    string n = mr.gameObject.name.ToLower();
                    if (n.Contains("base") || n.Contains("metal") || n.Contains("frame"))
                        mr.sharedMaterial = matMetal;
                    else
                        mr.sharedMaterial = matWood;
                }
            }

            // 5. Fix Roof / Ceiling Enclosure (Eliminate Open-Sky Feeling)
            // Add a proper continuous indoor ceiling slab at Y = 3.82m
            Transform shellTransform = rootShell != null ? rootShell.transform : null;
            GameObject ceilingRoot = new GameObject("Enclosed_Ceiling_Structure");
            if (shellTransform != null) ceilingRoot.transform.SetParent(shellTransform, false);

            // East Wing Ceiling (covering IoT Lab & ISFCR: X: 0 to 36m, Z: -23 to 0m)
            CreateCeilingSlab(ceilingRoot.transform, "Ceiling_EastWing", new Vector3(18f, 3.82f, -11f), new Vector3(36f, 0.15f, 24f), matCeiling);

            // West Wing Ceiling (covering IoT Centre, Meeting Room, PESU Academy: X: -36 to 0m, Z: -24 to 26m)
            CreateCeilingSlab(ceilingRoot.transform, "Ceiling_WestWing", new Vector3(-18f, 3.82f, 1f), new Vector3(36f, 0.15f, 50f), matCeiling);

            // North Corridor Ceiling (X: 0 to 36m, Z: 0 to 26m)
            CreateCeilingSlab(ceilingRoot.transform, "Ceiling_NorthEast", new Vector3(18f, 3.82f, 13f), new Vector3(36f, 0.15f, 26f), matCeiling);

            // South Foyer Entry Ceiling (X: -8 to 8m, Z: -22 to -10m)
            CreateCeilingSlab(ceilingRoot.transform, "Ceiling_SouthFoyer", new Vector3(0f, 3.82f, -16f), new Vector3(18f, 0.15f, 12f), matCeiling);

            // Note: Central Atrium (~0, 0) remains open vertically to the Chandelier fixture at Y = 4.77m as intended by architecture!

            // 6. Seal Outer Perimeter Wall Gaps
            // Ensure exterior walls form a continuous, solid building enclosure
            GameObject exteriorEnclosure = new GameObject("Exterior_Enclosure_Walls");
            if (shellTransform != null) exteriorEnclosure.transform.SetParent(shellTransform, false);

            CreateWallPanel(exteriorEnclosure.transform, "ExtWall_North", new Vector3(0f, 1.9f, 27.3f), new Vector3(81.9f, 3.8f, 0.4f), matWall);
            CreateWallPanel(exteriorEnclosure.transform, "ExtWall_South", new Vector3(0f, 1.9f, -27.3f), new Vector3(81.9f, 3.8f, 0.4f), matWall);
            CreateWallPanel(exteriorEnclosure.transform, "ExtWall_East", new Vector3(40.9f, 1.9f, 0f), new Vector3(0.4f, 3.8f, 54.7f), matWall);
            CreateWallPanel(exteriorEnclosure.transform, "ExtWall_West", new Vector3(-40.9f, 1.9f, 0f), new Vector3(0.4f, 3.8f, 54.7f), matWall);

            // 7. Adjust Lighting & Camera Atmosphere (Eliminate Blinding Overexposure)
            GameObject dirLightGo = GameObject.Find("Directional_Light_Sun");
            if (dirLightGo != null)
            {
                Light dirLight = dirLightGo.GetComponent<Light>();
                if (dirLight != null)
                {
                    dirLight.intensity = 0.45f; // Soft, warm daylight fill
                    dirLight.color = new Color(1.0f, 0.94f, 0.88f);
                    dirLight.shadows = LightShadows.Soft;
                }
            }

            // Set balanced ambient indoor lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.32f, 0.32f, 0.35f);

            // Camera background: dark indoor tone instead of skybox
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
            }

            // 8. Ensure Player Spawn is perfectly grounded and accessible
            GameObject player = GameObject.Find("Player_FirstPersonController");
            if (player != null)
            {
                player.transform.position = new Vector3(0f, 0.2f, -11f);
            }

            // 9. Save as NEW scene (preserving the original)
            EditorSceneManager.SaveScene(scene, fixedScenePath);
            Debug.Log($"Fixed environment successfully saved to: {fixedScenePath}");

            // 10. Register in Build Settings
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
            buildScenes.Add(new EditorBuildSettingsScene(fixedScenePath, true));
            buildScenes.Add(new EditorBuildSettingsScene(sourceScenePath, false));
            EditorBuildSettings.scenes = buildScenes.ToArray();

            Debug.Log("=================================================");
            Debug.Log("REPAIR PASS COMPLETED SUCCESSFULLY!");
            Debug.Log("=================================================");
        }

        private static void CreateCeilingSlab(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = name;
            slab.transform.SetParent(parent, false);
            slab.transform.position = pos;
            slab.transform.localScale = size;
            slab.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private static void CreateWallPanel(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, false);
            wall.transform.position = pos;
            wall.transform.localScale = size;
            wall.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private static void ApplyMaterialRecursively(GameObject root, Material mat)
        {
            MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer mr in renderers)
            {
                mr.sharedMaterial = mat;
            }
        }

        private static Material CreateMat(string path, Color color, float smoothness = 0.1f)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                mat.SetFloat("_Glossiness", smoothness);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
