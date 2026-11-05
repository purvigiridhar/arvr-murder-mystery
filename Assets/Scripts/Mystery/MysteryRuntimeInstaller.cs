using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MurderMystery
{
    public static class MysteryRuntimeInstaller
    {
        private const string MainSceneName = "MurderMystery_MainEnvironment";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name != MainSceneName ||
                FindComponent<MysteryManager>(scene) != null)
            {
                return;
            }

            MysteryEvidenceCatalog catalog =
                Resources.Load<MysteryEvidenceCatalog>("MysteryEvidenceCatalog");
            if (catalog == null)
            {
                Debug.LogError(
                    "MysteryRuntimeInstaller: Resources/MysteryEvidenceCatalog.asset " +
                    "is missing. Person 3 gameplay was not installed.");
                return;
            }

            if (!catalog.HasAllCanonicalEvidence())
            {
                Debug.LogError(
                    "MysteryRuntimeInstaller: the evidence catalog must reference all " +
                    "six canonical evidence assets. Person 3 gameplay was not installed.");
                return;
            }

            GameObject systemObject = new GameObject("MysteryRuntimeSystems");
            EvidenceManager evidenceManager =
                FindComponent<EvidenceManager>(scene);
            if (evidenceManager == null)
                evidenceManager = systemObject.AddComponent<EvidenceManager>();

            MysteryManager mysteryManager =
                systemObject.AddComponent<MysteryManager>();
            mysteryManager.Initialize(evidenceManager);

            EvidenceJournalUI evidenceJournal =
                FindComponent<EvidenceJournalUI>(scene);
            if (evidenceJournal == null)
                evidenceJournal = systemObject.AddComponent<EvidenceJournalUI>();

            FirstPersonController playerController =
                FindComponent<FirstPersonController>(scene);
            Camera playerCamera = playerController != null
                ? playerController.GetComponentInChildren<Camera>()
                : null;
            if (playerController == null)
            {
                Debug.LogError(
                    "MysteryRuntimeInstaller: FirstPersonController was not found " +
                    "in the main scene.");
            }
            else if (playerCamera == null)
            {
                Debug.LogError(
                    "MysteryRuntimeInstaller: the player has no camera for suspect labels.");
            }

            MysteryUI mysteryUI = systemObject.AddComponent<MysteryUI>();
            mysteryUI.Initialize(
                mysteryManager,
                evidenceManager,
                evidenceJournal,
                playerController);

            // Person 4 Audio, Notification, and Menu Systems
            if (FindComponent<MurderMystery.Audio.AudioManager>(scene) == null)
            {
                systemObject.AddComponent<MurderMystery.Audio.AudioManager>();
            }

            if (FindComponent<MurderMystery.UI.GameNotificationUI>(scene) == null)
            {
                systemObject.AddComponent<MurderMystery.UI.GameNotificationUI>();
            }

            if (FindComponent<MurderMystery.UI.GameMenuUI>(scene) == null)
            {
                systemObject.AddComponent<MurderMystery.UI.GameMenuUI>();
            }

            if (playerController != null && playerController.GetComponent<PlayerFootsteps>() == null)
            {
                playerController.gameObject.AddComponent<PlayerFootsteps>();
            }

            GameObject contentRoot = new GameObject("MysteryRuntimeContent");
            InstallEvidence(scene, contentRoot.transform, catalog);
            InstallSuspects(scene, mysteryUI, playerCamera);
            InstallAreaTriggers(scene, mysteryManager);
        }

        private static void InstallEvidence(
            Scene scene,
            Transform contentRoot,
            MysteryEvidenceCatalog catalog)
        {
            Transform crimeScene = FindTransform(scene, "Clue_Spawn_IoTLab");
            PlaceEvidence(
                contentRoot,
                catalog,
                MysteryManager.KeycardId,
                crimeScene,
                new Vector3(-0.45f, 0.25f, 0f),
                new Vector3(0.28f, 0.035f, 0.18f),
                new Color(0.78f, 0.75f, 0.45f));
            PlaceEvidence(
                contentRoot,
                catalog,
                MysteryManager.LogbookId,
                crimeScene,
                new Vector3(0f, 0.25f, 0.35f),
                new Vector3(0.32f, 0.06f, 0.38f),
                new Color(0.42f, 0.24f, 0.16f));
            PlaceEvidence(
                contentRoot,
                catalog,
                MysteryManager.PhoneId,
                crimeScene,
                new Vector3(0.45f, 0.25f, 0f),
                new Vector3(0.16f, 0.035f, 0.28f),
                new Color(0.16f, 0.17f, 0.2f));

            PlaceEvidence(
                contentRoot,
                catalog,
                MysteryManager.CctvId,
                FindTransform(scene, "Clue_Spawn_ISFCR"),
                new Vector3(0f, 0.25f, 0f),
                new Vector3(0.32f, 0.055f, 0.4f),
                new Color(0.2f, 0.32f, 0.46f));

            PlaceEvidenceAtRoom(
                scene,
                contentRoot,
                catalog,
                MysteryManager.GrantId,
                "pesuacademy",
                new Vector3(0f, 0.8f, 0f),
                new Color(0.42f, 0.34f, 0.2f));
            PlaceEvidenceAtRoom(
                scene,
                contentRoot,
                catalog,
                MysteryManager.PowerId,
                "iotcentre1108",
                new Vector3(0f, 0.8f, 0f),
                new Color(0.24f, 0.42f, 0.34f));
        }

        private static void PlaceEvidenceAtRoom(
            Scene scene,
            Transform contentRoot,
            MysteryEvidenceCatalog catalog,
            string evidenceId,
            string roomName,
            Vector3 offset,
            Color color)
        {
            Transform room = FindTransform(scene, roomName);
            if (room == null)
            {
                Debug.LogError(
                    $"MysteryRuntimeInstaller: room '{roomName}' was not found; " +
                    $"'{evidenceId}' was not placed.");
                return;
            }

            Renderer[] renderers = room.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogError(
                    $"MysteryRuntimeInstaller: room '{room.name}' has no renderers " +
                    $"to determine a placement for '{evidenceId}'.");
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            Vector3 position = bounds.center;
            position.y = bounds.min.y;
            PlaceEvidence(
                contentRoot,
                catalog,
                evidenceId,
                room,
                room.InverseTransformPoint(position + offset),
                new Vector3(0.32f, 0.055f, 0.4f),
                color);
        }

        private static void PlaceEvidence(
            Transform contentRoot,
            MysteryEvidenceCatalog catalog,
            string evidenceId,
            Transform anchor,
            Vector3 localOffset,
            Vector3 scale,
            Color color)
        {
            if (anchor == null)
            {
                Debug.LogError(
                    $"MysteryRuntimeInstaller: no scene anchor was found for " +
                    $"'{evidenceId}'.");
                return;
            }

            EvidenceItem item = catalog.GetEvidence(evidenceId);
            if (item == null)
            {
                Debug.LogError(
                    $"MysteryRuntimeInstaller: '{evidenceId}' is missing from the " +
                    "evidence catalog.");
                return;
            }

            GameObject clue = GameObject.CreatePrimitive(PrimitiveType.Cube);
            clue.name = $"Collectible_{evidenceId}";
            clue.transform.SetParent(contentRoot, false);
            clue.transform.position = anchor.TransformPoint(localOffset);
            clue.transform.rotation = anchor.rotation;
            clue.transform.localScale = scale;

            Renderer renderer = clue.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = color;

            EvidenceCollectible collectible =
                clue.AddComponent<EvidenceCollectible>();
            collectible.SetEvidenceItem(item);

            // Person 4 Clue Visual Indicator (warm amber pulse)
            clue.AddComponent<ClueVisualCue>();
        }

        private static void InstallSuspects(
            Scene scene,
            MysteryUI mysteryUI,
            Camera playerCamera)
        {
            Transform spawn = FindTransform(scene, "NPC_Spawn_Foyer");
            if (spawn == null)
            {
                Debug.LogError(
                    "MysteryRuntimeInstaller: NPC_Spawn_Foyer was not found; " +
                    "suspect interactions were not created.");
                return;
            }

            SuspectId[] suspects =
            {
                SuspectId.RohanKapoor,
                SuspectId.PriyaSharma,
                SuspectId.VikramDesai,
                SuspectId.KarthikNair
            };
            Color[] colors =
            {
                new Color(0.22f, 0.45f, 0.72f),
                new Color(0.3f, 0.62f, 0.38f),
                new Color(0.62f, 0.2f, 0.2f),
                new Color(0.72f, 0.62f, 0.22f)
            };

            GameObject suspectRoot = new GameObject("MysterySuspects");
            suspectRoot.transform.SetParent(spawn, false);
            for (int i = 0; i < suspects.Length; i++)
            {
                GameObject suspectObject =
                    GameObject.CreatePrimitive(PrimitiveType.Capsule);
                suspectObject.name =
                    $"Suspect_{MysterySuspectInteractable.GetDisplayName(suspects[i])}";
                suspectObject.transform.SetParent(suspectRoot.transform, false);
                suspectObject.transform.localPosition =
                    new Vector3((i - 1.5f) * 1.05f, 1f, 0.5f);
                suspectObject.transform.localScale =
                    new Vector3(0.7f, 0.9f, 0.7f);

                Renderer renderer = suspectObject.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.color = colors[i];

                suspectObject
                    .AddComponent<MysterySuspectInteractable>()
                    .Initialize(suspects[i], mysteryUI, playerCamera);
            }
        }

        private static void InstallAreaTriggers(
            Scene scene,
            MysteryManager mysteryManager)
        {
            AddAreaTrigger(
                scene,
                "NPC_Spawn_Foyer",
                MysteryLocation.Foyer,
                4.5f,
                mysteryManager);
            AddAreaTrigger(
                scene,
                "Clue_Spawn_IoTLab",
                MysteryLocation.IoTLab1112,
                4f,
                mysteryManager);
            AddAreaTrigger(
                scene,
                "Clue_Spawn_MeetingRoom",
                MysteryLocation.MeetingRoom,
                4f,
                mysteryManager);
        }

        private static void AddAreaTrigger(
            Scene scene,
            string objectName,
            MysteryLocation location,
            float radius,
            MysteryManager mysteryManager)
        {
            Transform target = FindTransform(scene, objectName);
            if (target == null)
            {
                Debug.LogError(
                    $"MysteryRuntimeInstaller: area marker '{objectName}' was not found.");
                return;
            }

            SphereCollider trigger = target.gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = radius;
            target.gameObject
                .AddComponent<MysteryAreaTrigger>()
                .Initialize(mysteryManager, location);
        }

        private static T FindComponent<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null)
                    return component;
            }

            return null;
        }

        private static Transform FindTransform(Scene scene, string namePart)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform[] transforms =
                    root.GetComponentsInChildren<Transform>(true);
                foreach (Transform candidate in transforms)
                {
                    if (candidate.name.IndexOf(
                            namePart,
                            StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }
    }
}
