using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MurderMystery;
using MurderMystery.Tests;

namespace MurderMystery.Editor
{
    public static class EvidenceTestSceneBuilder
    {
        [MenuItem("Murder Mystery/Build Evidence Test Scene")]
        public static void BuildScene()
        {
            Debug.Log("[EvidenceTestSceneBuilder] Building dedicated Evidence System test scene...");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 1. Remove initial default camera
            var defaultCam = GameObject.FindWithTag("MainCamera");
            if (defaultCam != null)
            {
                Object.DestroyImmediate(defaultCam);
            }

            // 2. Setup Ground Floor
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Floor";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(3f, 1f, 3f);

            // 3. Setup Player
            var playerObj = new GameObject("Player_FirstPersonController");
            playerObj.transform.position = new Vector3(0f, 0.1f, 0f);

            var charController = playerObj.AddComponent<CharacterController>();
            charController.height = 1.8f;
            charController.radius = 0.35f;
            charController.center = new Vector3(0f, 0.9f, 0f);

            var camObj = new GameObject("Main Camera");
            camObj.transform.SetParent(playerObj.transform);
            camObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            camObj.transform.localRotation = Quaternion.identity;

            var cam = camObj.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cam.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();

            var fpc = playerObj.AddComponent<FirstPersonController>();
            fpc.cameraTransform = camObj.transform;

            var promptUI = playerObj.AddComponent<InteractionPromptUI>();

            var raycaster = playerObj.AddComponent<InteractionRaycaster>();
            var raycasterSo = new SerializedObject(raycaster);
            raycasterSo.FindProperty("playerCamera").objectReferenceValue = cam;
            raycasterSo.FindProperty("promptUI").objectReferenceValue = promptUI;
            raycasterSo.FindProperty("interactionRange").floatValue = 3.5f;
            raycasterSo.ApplyModifiedProperties();

            // 4. Game Systems (EvidenceManager & JournalUI & Automated Self-Tests)
            var systemsObj = new GameObject("GameSystems");
            systemsObj.AddComponent<EvidenceManager>();
            systemsObj.AddComponent<EvidenceJournalUI>();
            systemsObj.AddComponent<EvidenceSystemTests>();

            // 5. Evidence Display Table
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "EvidenceInspectionTable";
            table.transform.position = new Vector3(0f, 0.45f, 2.2f);
            table.transform.localScale = new Vector3(3.6f, 0.9f, 1.2f);

            // 6. Evidence Collectible Clues
            // Item 1: Keycard
            var keycardAsset = AssetDatabase.LoadAssetAtPath<EvidenceItem>("Assets/Evidence/EV_Keycard_001.asset");
            var keycardObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            keycardObj.name = "Collectible_Keycard";
            keycardObj.transform.position = new Vector3(-1.0f, 1.0f, 2.2f);
            keycardObj.transform.localScale = new Vector3(0.25f, 0.05f, 0.4f);
            var ec1 = keycardObj.AddComponent<EvidenceCollectible>();
            var ec1So = new SerializedObject(ec1);
            ec1So.FindProperty("evidenceItem").objectReferenceValue = keycardAsset;
            ec1So.FindProperty("includeItemNameInPrompt").boolValue = true;
            ec1So.FindProperty("disableGameObjectOnCollect").boolValue = true;
            ec1So.ApplyModifiedProperties();

            // Item 2: Logbook
            var logbookAsset = AssetDatabase.LoadAssetAtPath<EvidenceItem>("Assets/Evidence/EV_Logbook_002.asset");
            var logbookObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            logbookObj.name = "Collectible_Logbook";
            logbookObj.transform.position = new Vector3(0f, 1.0f, 2.2f);
            logbookObj.transform.localScale = new Vector3(0.4f, 0.08f, 0.5f);
            var ec2 = logbookObj.AddComponent<EvidenceCollectible>();
            var ec2So = new SerializedObject(ec2);
            ec2So.FindProperty("evidenceItem").objectReferenceValue = logbookAsset;
            ec2So.FindProperty("includeItemNameInPrompt").boolValue = true;
            ec2So.FindProperty("disableGameObjectOnCollect").boolValue = true;
            ec2So.ApplyModifiedProperties();

            // Item 3: Phone
            var phoneAsset = AssetDatabase.LoadAssetAtPath<EvidenceItem>("Assets/Evidence/EV_Phone_003.asset");
            var phoneObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            phoneObj.name = "Collectible_Phone";
            phoneObj.transform.position = new Vector3(1.0f, 1.0f, 2.2f);
            phoneObj.transform.localScale = new Vector3(0.2f, 0.05f, 0.35f);
            var ec3 = phoneObj.AddComponent<EvidenceCollectible>();
            var ec3So = new SerializedObject(ec3);
            ec3So.FindProperty("evidenceItem").objectReferenceValue = phoneAsset;
            ec3So.FindProperty("includeItemNameInPrompt").boolValue = true;
            ec3So.FindProperty("disableGameObjectOnCollect").boolValue = true;
            ec3So.ApplyModifiedProperties();

            // 7. Save Scene
            string scenePath = "Assets/Scenes/EvidenceSystem_TestScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[EvidenceTestSceneBuilder] Successfully generated and saved test scene at: {scenePath}");

            EditorApplication.Exit(0);
        }
    }
}
