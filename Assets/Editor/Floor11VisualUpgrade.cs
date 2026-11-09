using System.Collections.Generic;
using System.IO;
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
    /// Person 4 Floor 11 Visual & Environmental Polish Pass:
    /// - Transforms the plain, ashen environment into a warm, colourful, modern university research lab.
    /// - Applies the approved architectural material palette (warm beige walls, lab blue-gray, polished gray floors, dark walnut doors, wood/metal furniture).
    /// - Adds realistic ceiling lights, chandelier fixtures, desk downlights, and warm ambient indoor lighting.
    /// - Adds indoor potted plants, wall notice boards, computer screens, and emergency detail markers.
    /// - Builds and positions the humanoid Guide NPC "Arjun Mehta" in the Foyer with idle animation and interaction.
    /// </summary>
    public static class Floor11VisualUpgrade
    {
        private const string ScenePath = "Assets/Scenes/MurderMystery_MainEnvironment.unity";

        [MenuItem("Murder Mystery/Upgrade Floor 11 Visuals & Guide NPC")]
        public static void UpgradeScene()
        {
            Debug.Log("==================================================");
            Debug.Log("[Person 4] STARTING FLOOR 11 VISUAL POLISH & GUIDE NPC");
            Debug.Log("==================================================");

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            EnsureFolder("Assets/Materials/Floor11");

            // 1. Create Modern Architectural Material Palette
            Material matMainWall = CreateMaterial("Assets/Materials/Floor11/M_Wall_WarmBeige.mat", new Color(0.91f, 0.89f, 0.85f), 0.08f);
            Material matLabWall = CreateMaterial("Assets/Materials/Floor11/M_Wall_LabLightBlueGray.mat", new Color(0.85f, 0.89f, 0.92f), 0.12f);
            Material matFloor = CreateMaterial("Assets/Materials/Floor11/M_Floor_PolishedGray.mat", new Color(0.32f, 0.33f, 0.35f), 0.35f);
            Material matCeiling = CreateMaterial("Assets/Materials/Floor11/M_Ceiling_ModernCharcoal.mat", new Color(0.12f, 0.13f, 0.15f), 0.05f);
            Material matWarmWood = CreateMaterial("Assets/Materials/Floor11/M_Wood_BoardroomOak.mat", new Color(0.42f, 0.26f, 0.14f), 0.32f);
            Material matDarkWalnut = CreateMaterial("Assets/Materials/Floor11/M_Door_DarkWalnut.mat", new Color(0.26f, 0.17f, 0.10f), 0.22f);
            Material matDarkMetal = CreateMaterial("Assets/Materials/Floor11/M_Metal_DarkGraphite.mat", new Color(0.16f, 0.17f, 0.19f), 0.55f, 0.75f);
            Material matLabWhite = CreateMaterial("Assets/Materials/Floor11/M_Lab_TechWhite.mat", new Color(0.88f, 0.90f, 0.92f), 0.45f);
            Material matScreenGlow = CreateMaterial("Assets/Materials/Floor11/M_Screen_TechGlow.mat", new Color(0.18f, 0.65f, 0.85f), 0.7f, 0.2f, new Color(0.12f, 0.45f, 0.65f) * 1.5f);
            Material matPlantGreen = CreateMaterial("Assets/Materials/Floor11/M_Plant_NaturalGreen.mat", new Color(0.18f, 0.42f, 0.16f), 0.15f);
            Material matPotCeramic = CreateMaterial("Assets/Materials/Floor11/M_Plant_PotCeramic.mat", new Color(0.82f, 0.80f, 0.78f), 0.4f);
            Material matEmergencyRed = CreateMaterial("Assets/Materials/Floor11/M_Emergency_MutedRed.mat", new Color(0.75f, 0.18f, 0.18f), 0.3f);
            Material matNoticeBoard = CreateMaterial("Assets/Materials/Floor11/M_Notice_CorkBoard.mat", new Color(0.65f, 0.52f, 0.38f), 0.05f);

            // 2. Apply Materials to Building Shell
            GameObject rootShell = GameObject.Find("[01_BUILDING_SHELL]");
            if (rootShell != null)
            {
                ApplyMaterialRecursively(rootShell, matMainWall);
            }

            // 3. Apply Materials to Floors
            GameObject rootFloors = GameObject.Find("[02_FLOORS]");
            if (rootFloors != null)
            {
                ApplyMaterialRecursively(rootFloors, matFloor);
            }

            // 4. Apply Materials across Individual Rooms & Architecture
            GameObject rootRooms = GameObject.Find("[03_ROOMS]");
            if (rootRooms != null)
            {
                MeshRenderer[] roomRenderers = rootRooms.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mr in roomRenderers)
                {
                    string objName = mr.gameObject.name.ToLower();
                    string parentName = mr.transform.parent != null ? mr.transform.parent.name.ToLower() : "";

                    // Re-enable in case any room walls were hidden
                    mr.enabled = true;

                    if (objName.Contains("door") || objName.Contains("frame"))
                    {
                        mr.sharedMaterial = matDarkWalnut;
                    }
                    else if (objName.Contains("railing") || objName.Contains("metal") || objName.Contains("shelf"))
                    {
                        mr.sharedMaterial = matDarkMetal;
                    }
                    else if (objName.Contains("chair") || objName.Contains("seat") || objName.Contains("stool"))
                    {
                        mr.sharedMaterial = matDarkMetal;
                    }
                    else if (objName.Contains("table") || objName.Contains("desk"))
                    {
                        mr.sharedMaterial = matWarmWood;
                    }
                    else if (objName.Contains("floor"))
                    {
                        mr.sharedMaterial = matFloor;
                    }
                    else if (objName.Contains("roof") || objName.Contains("ceiling"))
                    {
                        mr.sharedMaterial = matCeiling;
                    }
                    else if (parentName.Contains("iot") || parentName.Contains("isfcr") || objName.Contains("lab"))
                    {
                        mr.sharedMaterial = matLabWall;
                    }
                    else
                    {
                        mr.sharedMaterial = matMainWall;
                    }
                }
            }

            // 5. Apply Materials to Modular Props
            GameObject rootProps = GameObject.Find("[04_FURNITURE_PROPS]");
            if (rootProps != null)
            {
                MeshRenderer[] propRenderers = rootProps.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mr in propRenderers)
                {
                    string n = mr.gameObject.name.ToLower();
                    if (n.Contains("base") || n.Contains("metal") || n.Contains("frame"))
                        mr.sharedMaterial = matDarkMetal;
                    else if (n.Contains("table") || n.Contains("desk"))
                        mr.sharedMaterial = matWarmWood;
                    else
                        mr.sharedMaterial = matLabWhite;
                }
            }

            // 6. Balanced Indoor Atmospheric Lighting
            SetupLighting(matCeiling);

            // 7. Add Environmental Details (Plants, Notice Boards, Tech Displays)
            SetupEnvironmentalDetails(matPlantGreen, matPotCeramic, matNoticeBoard, matScreenGlow, matDarkMetal, matEmergencyRed);

            // 8. Construct Humanoid Guide NPC "Arjun Mehta" in Foyer
            SetupGuideNpc();

            // 9. Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("==================================================");
            Debug.Log("[Person 4] FLOOR 11 VISUAL POLISH & GUIDE NPC COMPLETE!");
            Debug.Log("==================================================");
        }

        private static void SetupLighting(Material matCeiling)
        {
            GameObject rootLighting = GameObject.Find("[05_LIGHTING]");
            if (rootLighting == null)
            {
                rootLighting = new GameObject("[05_LIGHTING]");
            }

            // Soft daylight sun fill
            GameObject dirLightGo = GameObject.Find("Directional_Light_Sun");
            if (dirLightGo != null)
            {
                Light dirLight = dirLightGo.GetComponent<Light>();
                if (dirLight != null)
                {
                    dirLight.intensity = 0.55f;
                    dirLight.color = new Color(1.0f, 0.94f, 0.88f);
                    dirLight.shadows = LightShadows.Soft;
                }
            }

            // Balanced ambient lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.38f, 0.39f, 0.42f);

            // Detailed Room Lights Group
            Transform existingRoomLights = rootLighting.transform.Find("Room_Fixtures");
            GameObject roomLights = existingRoomLights != null ? existingRoomLights.gameObject : new GameObject("Room_Fixtures");
            roomLights.transform.SetParent(rootLighting.transform, false);

            // Foyer Warm Overhead Chandelier
            CreatePointLight(roomLights.transform, "Light_Foyer_Atrium", new Vector3(0f, 3.8f, -6f), new Color(1.0f, 0.94f, 0.82f), 1.4f, 15f);
            CreatePointLight(roomLights.transform, "Light_Foyer_DeskSpot", new Vector3(0.5f, 2.5f, -6.8f), new Color(1.0f, 0.92f, 0.78f), 1.5f, 7f);
            CreatePointLight(roomLights.transform, "Light_Foyer_Entrance", new Vector3(0f, 3.2f, -12f), new Color(1.0f, 0.94f, 0.86f), 1.2f, 10f);

            // Meeting Room Warm Conference Lighting
            CreatePointLight(roomLights.transform, "Light_MeetingRoom_Center", new Vector3(-16f, 3.4f, 10f), new Color(1.0f, 0.93f, 0.84f), 1.4f, 13f);

            // IoT Lab Tech Blue-White Lighting
            CreatePointLight(roomLights.transform, "Light_IoTLab_Workbench", new Vector3(18f, 3.2f, -5f), new Color(0.92f, 0.96f, 1.0f), 1.35f, 12f);
            CreatePointLight(roomLights.transform, "Light_IoTLab_Center", new Vector3(18f, 3.2f, -12f), new Color(0.92f, 0.96f, 1.0f), 1.2f, 12f);

            // ISFCR Server Room Security Blue Tone
            CreatePointLight(roomLights.transform, "Light_ISFCR_Security", new Vector3(11f, 3.0f, -11f), new Color(0.88f, 0.94f, 1.0f), 1.2f, 10f);

            // North Corridor Lights
            CreatePointLight(roomLights.transform, "Light_NorthCorridor_West", new Vector3(-16f, 3.2f, 0f), new Color(1.0f, 0.95f, 0.90f), 1.1f, 10f);
            CreatePointLight(roomLights.transform, "Light_NorthCorridor_East", new Vector3(16f, 3.2f, 3f), new Color(1.0f, 0.95f, 0.90f), 1.1f, 10f);
        }

        private static void SetupEnvironmentalDetails(Material matPlant, Material matPot, Material matNotice, Material matScreen, Material matMetal, Material matRed)
        {
            GameObject detailsRoot = GameObject.Find("[08_ENVIRONMENTAL_DETAILS]");
            if (detailsRoot == null)
            {
                detailsRoot = new GameObject("[08_ENVIRONMENTAL_DETAILS]");
            }

            // 1. Indoor Potted Plants in Foyer & Entrance
            CreatePottedPlant(detailsRoot.transform, "Plant_Foyer_Right", new Vector3(3.8f, 0.05f, -10.5f), matPot, matPlant);
            CreatePottedPlant(detailsRoot.transform, "Plant_Foyer_Left", new Vector3(-3.8f, 0.05f, -10.5f), matPot, matPlant);
            CreatePottedPlant(detailsRoot.transform, "Plant_MeetingRoom_Corner", new Vector3(-20.5f, 0.05f, 7.5f), matPot, matPlant);
            CreatePottedPlant(detailsRoot.transform, "Plant_IoTLab_Corner", new Vector3(22.5f, 0.05f, -2.5f), matPot, matPlant);

            // 2. Wall Notice Boards & Floor Directory
            CreateWallSign(detailsRoot.transform, "Sign_FloorDirectory", new Vector3(0f, 2.2f, -14.8f), new Vector3(2.4f, 1.2f, 0.06f), matNotice);
            CreateWallSign(detailsRoot.transform, "Sign_LabNotice_IoT", new Vector3(15.2f, 2.0f, -0.2f), new Vector3(1.6f, 0.9f, 0.05f), matNotice);
            CreateWallSign(detailsRoot.transform, "Sign_MeetingRoom_Schedule", new Vector3(-13.5f, 2.0f, 5.2f), new Vector3(1.4f, 0.9f, 0.05f), matNotice);

            // 3. Computer Screens on Desks
            CreateMonitorDisplay(detailsRoot.transform, "Monitor_Foyer_Reception", new Vector3(-0.4f, 0.98f, -6.6f), matMetal, matScreen);
            CreateMonitorDisplay(detailsRoot.transform, "Monitor_IoTLab_1", new Vector3(18.8f, 0.92f, -4.6f), matMetal, matScreen);
            CreateMonitorDisplay(detailsRoot.transform, "Monitor_IoTLab_2", new Vector3(17.4f, 0.92f, -4.6f), matMetal, matScreen);

            // 4. Emergency Stations (Muted Red Markers)
            CreateWallSign(detailsRoot.transform, "Emergency_Station_Foyer", new Vector3(4.8f, 1.8f, -13.5f), new Vector3(0.35f, 0.5f, 0.08f), matRed);
            CreateWallSign(detailsRoot.transform, "Emergency_Station_North", new Vector3(0f, 1.8f, 4.5f), new Vector3(0.35f, 0.5f, 0.08f), matRed);
        }

        private static void SetupGuideNpc()
        {
            Transform existingNpc = GameObject.Find("NPC_Guide_ArjunMehta")?.transform;
            GameObject npcObj;

            if (existingNpc != null)
            {
                npcObj = existingNpc.gameObject;
            }
            else
            {
                npcObj = new GameObject("NPC_Guide_ArjunMehta");
            }

            // Position naturally in Foyer near entrance: x=2.4m, y=0.05m, z=-9.2m (welcoming, clear of corridors)
            npcObj.transform.position = new Vector3(2.4f, 0.05f, -9.2f);
            npcObj.transform.rotation = Quaternion.Euler(0f, -45f, 0f);

            // Materials for Arjun Mehta
            Material matCoat = CreateMaterial("Assets/Materials/Floor11/M_Arjun_LabCoat.mat", new Color(0.95f, 0.96f, 0.98f), 0.15f);
            Material matNavyShirt = CreateMaterial("Assets/Materials/Floor11/M_Arjun_NavyShirt.mat", new Color(0.12f, 0.18f, 0.28f), 0.1f);
            Material matPants = CreateMaterial("Assets/Materials/Floor11/M_Arjun_Trousers.mat", new Color(0.20f, 0.22f, 0.25f), 0.1f);
            Material matSkin = CreateMaterial("Assets/Materials/Floor11/M_Arjun_Skin.mat", new Color(0.85f, 0.68f, 0.55f), 0.1f);
            Material matHair = CreateMaterial("Assets/Materials/Floor11/M_Arjun_DarkHair.mat", new Color(0.12f, 0.10f, 0.08f), 0.05f);
            Material matShoes = CreateMaterial("Assets/Materials/Floor11/M_Arjun_Shoes.mat", new Color(0.10f, 0.10f, 0.10f), 0.3f);

            // Remove previous children if updating
            while (npcObj.transform.childCount > 0)
            {
                Object.DestroyImmediate(npcObj.transform.GetChild(0).gameObject);
            }

            // Build properly proportioned humanoid figure:
            // Torso (Lab Coat & Shirt)
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso.name = "Torso";
            torso.transform.SetParent(npcObj.transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            torso.transform.localScale = new Vector3(0.48f, 0.65f, 0.28f);
            torso.GetComponent<Renderer>().sharedMaterial = matCoat;

            // Inner Shirt Collar Accent
            GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            collar.name = "Shirt_Collar";
            collar.transform.SetParent(torso.transform, false);
            collar.transform.localPosition = new Vector3(0f, 0.35f, 0.08f);
            collar.transform.localScale = new Vector3(0.55f, 0.35f, 0.6f);
            collar.GetComponent<Renderer>().sharedMaterial = matNavyShirt;

            // Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(npcObj.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            head.transform.localScale = new Vector3(0.28f, 0.30f, 0.28f);
            head.GetComponent<Renderer>().sharedMaterial = matSkin;

            // Hair
            GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hair.name = "Hair";
            hair.transform.SetParent(head.transform, false);
            hair.transform.localPosition = new Vector3(0f, 0.18f, -0.04f);
            hair.transform.localScale = new Vector3(1.05f, 0.75f, 1.05f);
            hair.GetComponent<Renderer>().sharedMaterial = matHair;

            // Spectacles / Glasses frame
            GameObject glasses = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glasses.name = "Glasses_Frame";
            glasses.transform.SetParent(head.transform, false);
            glasses.transform.localPosition = new Vector3(0f, 0.04f, 0.46f);
            glasses.transform.localScale = new Vector3(0.85f, 0.18f, 0.12f);
            glasses.GetComponent<Renderer>().sharedMaterial = matHair;

            // Left Arm
            GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftArm.name = "Left_Arm";
            leftArm.transform.SetParent(npcObj.transform, false);
            leftArm.transform.localPosition = new Vector3(-0.32f, 1.12f, 0f);
            leftArm.transform.localScale = new Vector3(0.15f, 0.62f, 0.18f);
            leftArm.GetComponent<Renderer>().sharedMaterial = matCoat;

            // Right Arm
            GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightArm.name = "Right_Arm";
            rightArm.transform.SetParent(npcObj.transform, false);
            rightArm.transform.localPosition = new Vector3(0.32f, 1.12f, 0f);
            rightArm.transform.localScale = new Vector3(0.15f, 0.62f, 0.18f);
            rightArm.GetComponent<Renderer>().sharedMaterial = matCoat;

            // Left Leg
            GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftLeg.name = "Left_Leg";
            leftLeg.transform.SetParent(npcObj.transform, false);
            leftLeg.transform.localPosition = new Vector3(-0.14f, 0.45f, 0f);
            leftLeg.transform.localScale = new Vector3(0.18f, 0.75f, 0.22f);
            leftLeg.GetComponent<Renderer>().sharedMaterial = matPants;

            // Right Leg
            GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightLeg.name = "Right_Leg";
            rightLeg.transform.SetParent(npcObj.transform, false);
            rightLeg.transform.localPosition = new Vector3(0.14f, 0.45f, 0f);
            rightLeg.transform.localScale = new Vector3(0.18f, 0.75f, 0.22f);
            rightLeg.GetComponent<Renderer>().sharedMaterial = matPants;

            // Shoes
            GameObject leftShoe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftShoe.name = "Left_Shoe";
            leftShoe.transform.SetParent(npcObj.transform, false);
            leftShoe.transform.localPosition = new Vector3(-0.14f, 0.06f, 0.05f);
            leftShoe.transform.localScale = new Vector3(0.19f, 0.12f, 0.32f);
            leftShoe.GetComponent<Renderer>().sharedMaterial = matShoes;

            GameObject rightShoe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightShoe.name = "Right_Shoe";
            rightShoe.transform.SetParent(npcObj.transform, false);
            rightShoe.transform.localPosition = new Vector3(0.14f, 0.06f, 0.05f);
            rightShoe.transform.localScale = new Vector3(0.19f, 0.12f, 0.32f);
            rightShoe.GetComponent<Renderer>().sharedMaterial = matShoes;

            // Clean single BoxCollider on root for interaction raycasting
            BoxCollider col = npcObj.GetComponent<BoxCollider>();
            if (col == null) col = npcObj.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.95f, 0f);
            col.size = new Vector3(0.9f, 1.9f, 0.7f);

            // Remove colliders from child parts so raycaster hits the parent cleanly
            foreach (Collider c in npcObj.GetComponentsInChildren<Collider>())
            {
                if (c != col) Object.DestroyImmediate(c);
            }

            // Attach Interaction & Idle Animation components
            if (npcObj.GetComponent<GuideNpcInteractable>() == null)
            {
                npcObj.AddComponent<GuideNpcInteractable>();
            }

            if (npcObj.GetComponent<GuideNpcIdleAnimation>() == null)
            {
                npcObj.AddComponent<GuideNpcIdleAnimation>();
            }

            Debug.Log("[Person 4] Humanoid Guide NPC 'Arjun Mehta' configured at " + npcObj.transform.position);
        }

        private static void CreatePottedPlant(Transform parent, string name, Vector3 pos, Material matPot, Material matPlant)
        {
            GameObject plantObj = new GameObject(name);
            plantObj.transform.SetParent(parent, false);
            plantObj.transform.position = pos;

            // Ceramic Pot
            GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "Pot";
            pot.transform.SetParent(plantObj.transform, false);
            pot.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            pot.transform.localScale = new Vector3(0.55f, 0.35f, 0.55f);
            pot.GetComponent<Renderer>().sharedMaterial = matPot;

            // Plant Leaves / Foliage
            GameObject foliage1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage1.name = "Foliage_Center";
            foliage1.transform.SetParent(plantObj.transform, false);
            foliage1.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            foliage1.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            foliage1.GetComponent<Renderer>().sharedMaterial = matPlant;

            GameObject foliage2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage2.name = "Foliage_Top";
            foliage2.transform.SetParent(plantObj.transform, false);
            foliage2.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            foliage2.transform.localScale = new Vector3(0.55f, 0.65f, 0.55f);
            foliage2.GetComponent<Renderer>().sharedMaterial = matPlant;
        }

        private static void CreateWallSign(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = name;
            sign.transform.SetParent(parent, false);
            sign.transform.position = pos;
            sign.transform.localScale = scale;
            sign.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateMonitorDisplay(Transform parent, string name, Vector3 pos, Material matMetal, Material matScreen)
        {
            GameObject monitor = new GameObject(name);
            monitor.transform.SetParent(parent, false);
            monitor.transform.position = pos;

            // Stand
            GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stand.name = "Stand";
            stand.transform.SetParent(monitor.transform, false);
            stand.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            stand.transform.localScale = new Vector3(0.18f, 0.24f, 0.12f);
            stand.GetComponent<Renderer>().sharedMaterial = matMetal;

            // Screen Frame
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = "Screen";
            screen.transform.SetParent(monitor.transform, false);
            screen.transform.localPosition = new Vector3(0f, 0.32f, 0f);
            screen.transform.localScale = new Vector3(0.65f, 0.40f, 0.05f);
            screen.GetComponent<Renderer>().sharedMaterial = matScreen;
        }

        private static void CreatePointLight(Transform parent, string name, Vector3 pos, Color color, float intensity, float range)
        {
            Transform existing = parent.Find(name);
            GameObject lightObj = existing != null ? existing.gameObject : new GameObject(name);
            lightObj.transform.SetParent(parent, false);
            lightObj.transform.position = pos;

            Light lt = lightObj.GetComponent<Light>();
            if (lt == null) lt = lightObj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = color;
            lt.intensity = intensity;
            lt.range = range;
            lt.shadows = LightShadows.None; // Optimized for PC performance
        }

        private static Material CreateMaterial(string path, Color color, float smoothness = 0.1f, float metallic = 0.0f, Color? emission = null)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                mat.SetFloat("_Glossiness", smoothness);
                mat.SetFloat("_Metallic", metallic);

                if (emission.HasValue)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emission.Value);
                }

                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.color = color;
                mat.SetFloat("_Glossiness", smoothness);
                mat.SetFloat("_Metallic", metallic);
                if (emission.HasValue)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emission.Value);
                }
                EditorUtility.SetDirty(mat);
            }
            return mat;
        }

        private static void ApplyMaterialRecursively(GameObject root, Material mat)
        {
            if (root == null || mat == null) return;
            MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer mr in renderers)
            {
                mr.sharedMaterial = mat;
            }
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

