using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using MurderMystery;
using MurderMystery.Audio;
using MurderMystery.UI;

namespace MurderMystery.Editor
{
    /// <summary>
    /// Person 4 Environment & UI Integrator:
    /// Seamlessly integrates the evidence system, interaction raycasting, HUD, and audio
    /// into Darshana's main Floor 11 environment (MurderMystery_MainEnvironment.unity).
    /// Idempotent and non-destructive: preserves all existing architecture, rooms, and colliders.
    /// </summary>
    public static class EnvironmentEvidenceIntegrator
    {
        private const string MainScenePath = "Assets/Scenes/MurderMystery_MainEnvironment.unity";

        [MenuItem("Murder Mystery/Integrate Floor 11 Environment & UI")]
        public static void IntegrateMainEnvironment()
        {
            Debug.Log("==================================================");
            Debug.Log("[Person 4] STARTING FLOOR 11 ENVIRONMENT INTEGRATION");
            Debug.Log("==================================================");

            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

            // 1. Setup Player Controller Interaction Raycaster & Prompt UI
            SetupPlayerController();

            // 2. Setup Central Game Systems (EvidenceManager, JournalUI, Audio, Objective, Dialogue, Accusation)
            SetupGameSystems();

            // 3. Integrate Evidence Clues into Floor 11
            SetupFloor11EvidenceClues();

            // 4. Save Scene Cleanly
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("==================================================");
            Debug.Log("[Person 4] FLOOR 11 INTEGRATION COMPLETED SUCCESSFULLY!");
            Debug.Log("==================================================");
        }

        private static void SetupPlayerController()
        {
            GameObject player = GameObject.Find("Player_FirstPersonController");
            if (player == null)
            {
                Debug.LogError("[Person 4] Could not find 'Player_FirstPersonController' in main scene!");
                return;
            }

            Camera playerCam = player.GetComponentInChildren<Camera>();
            if (playerCam == null)
            {
                Debug.LogError("[Person 4] Player camera not found!");
                return;
            }

            // Ensure InteractionPromptUI
            var promptUI = player.GetComponent<InteractionPromptUI>();
            if (promptUI == null)
            {
                promptUI = player.AddComponent<InteractionPromptUI>();
                Debug.Log("[Person 4] Attached InteractionPromptUI to Player.");
            }

            // Ensure InteractionRaycaster
            var raycaster = player.GetComponent<InteractionRaycaster>();
            if (raycaster == null)
            {
                raycaster = player.AddComponent<InteractionRaycaster>();
                Debug.Log("[Person 4] Attached InteractionRaycaster to Player.");
            }

            // Wire Serialized Properties
            var raycasterSo = new SerializedObject(raycaster);
            raycasterSo.FindProperty("playerCamera").objectReferenceValue = playerCam;
            raycasterSo.FindProperty("promptUI").objectReferenceValue = promptUI;
            raycasterSo.FindProperty("interactionRange").floatValue = 3.5f;
            raycasterSo.ApplyModifiedProperties();

            // Ensure FirstPersonController has cameraTransform assigned
            var fpc = player.GetComponent<FirstPersonController>();
            if (fpc != null)
            {
                var fpcSo = new SerializedObject(fpc);
                var camProp = fpcSo.FindProperty("cameraTransform");
                if (camProp != null && camProp.objectReferenceValue == null)
                {
                    camProp.objectReferenceValue = playerCam.transform;
                    fpcSo.ApplyModifiedProperties();
                }
            }
        }

        private static void SetupGameSystems()
        {
            GameObject gameSystems = GameObject.Find("GameSystems");
            if (gameSystems == null)
            {
                gameSystems = new GameObject("GameSystems");
                Debug.Log("[Person 4] Created 'GameSystems' container.");
            }

            // EvidenceManager
            if (gameSystems.GetComponent<EvidenceManager>() == null)
            {
                gameSystems.AddComponent<EvidenceManager>();
                Debug.Log("[Person 4] Attached EvidenceManager.");
            }

            // EvidenceJournalUI
            if (gameSystems.GetComponent<EvidenceJournalUI>() == null)
            {
                gameSystems.AddComponent<EvidenceJournalUI>();
                Debug.Log("[Person 4] Attached EvidenceJournalUI.");
            }

            // AudioManager
            if (gameSystems.GetComponent<AudioManager>() == null)
            {
                gameSystems.AddComponent<AudioManager>();
                Debug.Log("[Person 4] Attached AudioManager.");
            }

            // ObjectiveUI
            if (gameSystems.GetComponent<ObjectiveUI>() == null)
            {
                gameSystems.AddComponent<ObjectiveUI>();
                Debug.Log("[Person 4] Attached ObjectiveUI.");
            }

            // DialogueUI
            if (gameSystems.GetComponent<DialogueUI>() == null)
            {
                gameSystems.AddComponent<DialogueUI>();
                Debug.Log("[Person 4] Attached DialogueUI.");
            }

            // AccusationUI
            if (gameSystems.GetComponent<AccusationUI>() == null)
            {
                gameSystems.AddComponent<AccusationUI>();
                Debug.Log("[Person 4] Attached AccusationUI.");
            }
        }

        private static void SetupFloor11EvidenceClues()
        {
            GameObject cluesRoot = GameObject.Find("[07_EVIDENCE_CLUES]");
            if (cluesRoot == null)
            {
                cluesRoot = new GameObject("[07_EVIDENCE_CLUES]");
            }

            // Clue 1: Encrypted Access Card on Foyer Reception Counter
            // Located right in the Foyer lobby in front of player spawn (0f, 0.5f, -11f)
            CreateClueObject(
                cluesRoot.transform,
                "Evidence_Keycard_Foyer",
                "Assets/Evidence/EV_Keycard_001.asset",
                new Vector3(0.6f, 0.95f, -6.8f),
                new Vector3(0.24f, 0.04f, 0.36f),
                new Color(0.18f, 0.22f, 0.32f) // Deep Navy Card
            );

            // Clue 2: Torn Research Logbook in Meeting Room
            // Placed near Meeting Room marker (-16f, 0.8f, 10f)
            CreateClueObject(
                cluesRoot.transform,
                "Evidence_Logbook_MeetingRoom",
                "Assets/Evidence/EV_Logbook_002.asset",
                new Vector3(-16.0f, 0.85f, 10.0f),
                new Vector3(0.38f, 0.06f, 0.48f),
                new Color(0.42f, 0.32f, 0.20f) // Bound Paper/Leather
            );

            // Clue 3: Damaged Smartphone in IoT Lab
            // Placed near IoT Lab marker (18f, 0.8f, -5f)
            CreateClueObject(
                cluesRoot.transform,
                "Evidence_Phone_IoTLab",
                "Assets/Evidence/EV_Phone_003.asset",
                new Vector3(18.0f, 0.85f, -5.0f),
                new Vector3(0.20f, 0.04f, 0.36f),
                new Color(0.12f, 0.12f, 0.14f) // Dark Glass Phone
            );
        }

        private static void CreateClueObject(Transform parent, string name, string assetPath, Vector3 position, Vector3 scale, Color color)
        {
            Transform existing = parent.Find(name);
            GameObject clueObj;

            if (existing != null)
            {
                clueObj = existing.gameObject;
            }
            else
            {
                clueObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                clueObj.name = name;
                clueObj.transform.SetParent(parent, false);
            }

            clueObj.transform.position = position;
            clueObj.transform.localScale = scale;

            // Load EvidenceItem
            var evidenceAsset = AssetDatabase.LoadAssetAtPath<EvidenceItem>(assetPath);
            if (evidenceAsset == null)
            {
                Debug.LogWarning($"[Person 4] Could not load evidence item at: {assetPath}");
                return;
            }

            // Ensure material folder exists
            EnsureFolder("Assets/Materials/Clues");
            string matPath = $"Assets/Materials/Clues/M_{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                mat.SetFloat("_Glossiness", 0.6f);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            clueObj.GetComponent<MeshRenderer>().sharedMaterial = mat;

            // Box Collider
            var collider = clueObj.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = clueObj.AddComponent<BoxCollider>();
            }

            // EvidenceCollectible
            var collectible = clueObj.GetComponent<EvidenceCollectible>();
            if (collectible == null)
            {
                collectible = clueObj.AddComponent<EvidenceCollectible>();
            }

            var collectibleSo = new SerializedObject(collectible);
            collectibleSo.FindProperty("evidenceItem").objectReferenceValue = evidenceAsset;
            collectibleSo.FindProperty("includeItemNameInPrompt").boolValue = true;
            collectibleSo.FindProperty("disableGameObjectOnCollect").boolValue = true;
            collectibleSo.ApplyModifiedProperties();

            // ClueVisualCue (Ambient Warm Amber Glow Point Light)
            if (clueObj.GetComponent<ClueVisualCue>() == null)
            {
                clueObj.AddComponent<ClueVisualCue>();
            }

            Debug.Log($"[Person 4] Configured clue '{evidenceAsset.DisplayName}' at {position}.");
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            path = path.Replace("\\", "/");
            if (!AssetDatabase.IsValidFolder(path))
            {
                int lastSlash = path.LastIndexOf('/');
                if (lastSlash > 0)
                {
                    string parent = path.Substring(0, lastSlash);
                    string folder = path.Substring(lastSlash + 1);
                    EnsureFolder(parent);
                    if (!AssetDatabase.IsValidFolder(path))
                    {
                        AssetDatabase.CreateFolder(parent, folder);
                    }
                }
            }
        }
    }
}

