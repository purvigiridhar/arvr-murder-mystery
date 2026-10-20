using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MurderMystery.Editor
{
    public static class EnvironmentAssembler
    {
        [MenuItem("MurderMystery/Assemble Complete Environment")]
        public static void AssembleScene()
        {
            Debug.Log("=================================================");
            Debug.Log("STARTING FAST 11TH FLOOR ENVIRONMENT ASSEMBLY");
            Debug.Log("=================================================");

            // 1. Refresh Asset Database
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // 2. Create Destination Folders
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Materials");
            EnsureFolder("Assets/Prefabs");

            // 3. Create or Reset Scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            string scenePath = "Assets/Scenes/MurderMystery_MainEnvironment.unity";

            // 4. Create Material Palette
            Material matWall = CreateOrGetMaterial("Assets/Materials/M_Wall_Matte.mat", new Color(0.85f, 0.85f, 0.85f));
            Material matFloor = CreateOrGetMaterial("Assets/Materials/M_Floor_Base.mat", new Color(0.75f, 0.74f, 0.72f), 0.3f);
            Material matCeiling = CreateOrGetMaterial("Assets/Materials/M_Ceiling_Structure.mat", new Color(0.6f, 0.6f, 0.62f));
            Material matWood = CreateOrGetMaterial("Assets/Materials/M_Furniture_Wood.mat", new Color(0.45f, 0.28f, 0.15f), 0.2f);
            Material matMetal = CreateOrGetMaterial("Assets/Materials/M_Furniture_Metal.mat", new Color(0.3f, 0.32f, 0.35f), 0.6f);

            // 5. Create Scene Root Hierarchy
            GameObject rootShell = new GameObject("[01_BUILDING_SHELL]");
            GameObject rootFloors = new GameObject("[02_FLOORS]");
            GameObject rootRooms = new GameObject("[03_ROOMS]");
            GameObject rootProps = new GameObject("[04_FURNITURE_PROPS]");
            GameObject rootLighting = new GameObject("[05_LIGHTING]");
            GameObject rootGameplay = new GameObject("[06_GAMEPLAY_POINTS]");

            // 6. Instantiate Building Shell
            GameObject mainWalls = InstantiateModel("Assets/Environment/Architecture/MainWallsFile.fbx", rootShell.transform);
            GameObject roof = InstantiateModel("Assets/Environment/Architecture/roofblend.fbx", rootShell.transform);

            // 7. Instantiate Floors
            GameObject floorEdited = InstantiateModel("Assets/Environment/Architecture/editedfloor.fbx", rootFloors.transform);
            GameObject floorCutouts = InstantiateModel("Assets/Environment/Architecture/floorcutouts.fbx", rootFloors.transform);
            if (floorCutouts != null)
            {
                floorCutouts.SetActive(false); // Kept inactive to avoid Z-fighting, ready to toggle
                Debug.Log("Floor_Cutouts imported and set to INACTIVE by default (toggleable).");
            }

            // 8. Instantiate Major Rooms
            List<GameObject> roomList = new List<GameObject>();
            roomList.Add(InstantiateModel("Assets/Environment/Architecture/foyer.fbx", rootRooms.transform));
            roomList.Add(InstantiateModel("Assets/Environment/Architecture/iotcentre1108.fbx", rootRooms.transform));
            roomList.Add(InstantiateModel("Assets/Environment/Architecture/iotlab1112.fbx", rootRooms.transform));
            roomList.Add(InstantiateModel("Assets/Environment/Architecture/isfcr.fbx", rootRooms.transform));
            roomList.Add(InstantiateModel("Assets/Environment/Architecture/meetingroom.fbx", rootRooms.transform));
            roomList.Add(InstantiateModel("Assets/Environment/Architecture/pesuacademy.fbx", rootRooms.transform));
            roomList.Add(InstantiateModel("Assets/Environment/Architecture/railing.fbx", rootRooms.transform));
            roomList.Add(InstantiateModel("Assets/Environment/Architecture/reroshelf.fbx", rootRooms.transform));
            roomList.Add(InstantiateModel("Assets/Environment/Architecture/Chandelier.fbx", rootRooms.transform));

            // 9. Instantiate Modular Props Reference
            InstantiateModel("Assets/Environment/Props_Modular/ccbtable.fbx", rootProps.transform);
            InstantiateModel("Assets/Environment/Props_Modular/classnames.fbx", rootProps.transform);
            InstantiateModel("Assets/Environment/Props_Modular/iotTABLE.fbx", rootProps.transform);
            InstantiateModel("Assets/Environment/Props_Modular/IOTtablebase.fbx", rootProps.transform);

            // 10. Non-Destructive Wall Deduplication (Disable duplicate wall renderers in rooms)
            HashSet<string> duplicateWallNames = new HashSet<string> {
                "Cube.008", "Cube.009", "Cube.010", "Cube.011", "Cube.012", "Cube.013",
                "Cube.014", "Cube.015", "Cube.016", "Cube.017", "Cube.018", "Cube.019",
                "Cube.022", "Cube.023", "Cube.024", "Cube.025", "Cube.026", "Cube.027",
                "Cube.028", "Cube.029", "Cube.130", "Cube.131", "Cube.132", "Cube.133",
                "Cube.134", "Cube.161", "Cube.162", "Cube.181", "Cube.182", "Cube.183"
            };

            int disabledCount = 0;
            foreach (GameObject room in roomList)
            {
                if (room == null) continue;
                MeshRenderer[] renderers = room.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mr in renderers)
                {
                    if (duplicateWallNames.Contains(mr.gameObject.name))
                    {
                        mr.enabled = false;
                        disabledCount++;
                    }
                }
            }
            Debug.Log($"Disabled {disabledCount} duplicate wall mesh renderers across room models to prevent Z-fighting.");

            // 11. Add Simple Non-Destructive Physics Colliders
            // Ground floor walking collider
            GameObject floorCollider = new GameObject("Floor_Walkable_Collider");
            floorCollider.transform.SetParent(rootFloors.transform, false);
            BoxCollider bcFloor = floorCollider.AddComponent<BoxCollider>();
            bcFloor.center = new Vector3(0f, 0.05f, 0f);
            bcFloor.size = new Vector3(72f, 0.2f, 47f);

            // Perimeter barrier colliders
            GameObject perimeterColliders = new GameObject("Perimeter_Boundary_Colliders");
            perimeterColliders.transform.SetParent(rootShell.transform, false);

            AddBarrierBox(perimeterColliders, new Vector3(0f, 2f, 23.5f), new Vector3(72f, 4f, 1f));   // North
            AddBarrierBox(perimeterColliders, new Vector3(0f, 2f, -23.5f), new Vector3(72f, 4f, 1f));  // South
            AddBarrierBox(perimeterColliders, new Vector3(36f, 2f, 0f), new Vector3(1f, 4f, 48f));    // East
            AddBarrierBox(perimeterColliders, new Vector3(-36f, 2f, 0f), new Vector3(1f, 4f, 48f));   // West

            // Add MeshColliders to main solid walls in MainWallsFile so corridors and doors work
            if (mainWalls != null)
            {
                MeshFilter[] wallFilters = mainWalls.GetComponentsInChildren<MeshFilter>();
                int wallColliderCount = 0;
                foreach (MeshFilter mf in wallFilters)
                {
                    if (mf.sharedMesh != null && mf.gameObject.GetComponent<Collider>() == null)
                    {
                        // Add convex or standard mesh collider
                        MeshCollider mc = mf.gameObject.AddComponent<MeshCollider>();
                        mc.sharedMesh = mf.sharedMesh;
                        wallColliderCount++;
                    }
                }
                Debug.Log($"Added {wallColliderCount} wall mesh colliders for natural navigation.");
            }

            // 12. Setup Lighting
            GameObject dirLightGo = new GameObject("Directional_Light_Sun");
            dirLightGo.transform.SetParent(rootLighting.transform);
            Light dirLight = dirLightGo.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(1.0f, 0.96f, 0.90f);
            dirLight.intensity = 1.0f;
            dirLightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.48f);

            // 13. Setup PlayerSpawn & First Person Player
            GameObject spawnGo = new GameObject("PlayerSpawn");
            spawnGo.transform.SetParent(rootGameplay.transform);
            // Placed at Foyer entrance area: (0m, 0.5m, -11m)
            spawnGo.transform.position = new Vector3(0f, 0.5f, -11f);
            spawnGo.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            GameObject player = new GameObject("Player_FirstPersonController");
            player.transform.position = spawnGo.transform.position;
            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            GameObject camGo = new GameObject("MainCamera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(player.transform);
            camGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            camGo.transform.localRotation = Quaternion.identity;
            Camera cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 150f;
            camGo.AddComponent<AudioListener>();

            player.AddComponent<FirstPersonController>();

            // 14. Setup Gameplay Interaction Markers
            CreateMarker(rootGameplay.transform, "Clue_Spawn_MeetingRoom", new Vector3(-16f, 0.8f, 10f));
            CreateMarker(rootGameplay.transform, "Clue_Spawn_IoTLab", new Vector3(18f, 0.8f, -5f));
            CreateMarker(rootGameplay.transform, "Clue_Spawn_ISFCR", new Vector3(11f, 0.8f, -11f));
            CreateMarker(rootGameplay.transform, "NPC_Spawn_Foyer", new Vector3(0f, 0f, -15f));

            // 15. Save Scene
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"Scene successfully created and saved to: {scenePath}");

            // Set as active scene in Build Settings
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
            buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();

            Debug.Log("=================================================");
            Debug.Log("ASSEMBLY COMPLETED SUCCESSFULLY!");
            Debug.Log("=================================================");
        }

        private static GameObject InstantiateModel(string assetPath, Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                Debug.LogWarning($"Could not load asset at path: {assetPath}");
                return null;
            }
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go;
        }

        private static void AddBarrierBox(GameObject parent, Vector3 center, Vector3 size)
        {
            GameObject boxGo = new GameObject("Barrier_" + center.ToString());
            boxGo.transform.SetParent(parent.transform, false);
            BoxCollider bc = boxGo.AddComponent<BoxCollider>();
            bc.center = center;
            bc.size = size;
        }

        private static void CreateMarker(Transform parent, string name, Vector3 pos)
        {
            GameObject marker = new GameObject(name);
            marker.transform.SetParent(parent, false);
            marker.transform.position = pos;
        }

        private static Material CreateOrGetMaterial(string path, Color color, float smoothness = 0.1f)
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
