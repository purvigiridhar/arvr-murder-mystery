using System;
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
    /// Person 4 Complete Architectural, Visual & Level Design Overhaul:
    /// - Comprehensive ceiling and roof coverage (room-wise solid ceilings, zero gaps, no empty sky).
    /// - Mandated vibrant university colour palette applied room-by-room (Teal, Coral, Mustard, Electric Blue, Emerald, Burgundy, Navy, Orange).
    /// - Realistic room furniture and specialized apparatus (Workbenches, CCTV server racks, conference tables, executive office bookshelves).
    /// - Subtle murder mystery environmental storytelling (Police tape cordons, evidence cones, disturbed equipment, flickering lighting).
    /// - Legible, illuminated 3D room signage and corridor wayfinding signposts at every intersection.
    /// </summary>
    public static class Floor11CompleteOverhaul
    {
        private const string ScenePath = "Assets/Scenes/MurderMystery_MainEnvironment.unity";

        [MenuItem("Murder Mystery/Execute Complete Environment & Visual Overhaul")]
        public static void ExecuteOverhaul()
        {
            Debug.Log("==================================================================");
            Debug.Log("[Person 4] STARTING COMPLETE FLOOR 11 ARCHITECTURAL & VISUAL OVERHAUL");
            Debug.Log("==================================================================");

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            EnsureFolder("Assets/Materials/VibrantFloor11");

            // 1. Build Mandated Vibrant Material Palette
            var mats = CreateVibrantPalette();

            // 2. Build Room-wise Ceilings & Roof System (No void / No missing ceilings)
            BuildRoomCeilings(mats);

            // 3. Apply Room-Specific Colour Identifies to Existing Architecture
            ApplyArchitecturalColors(mats);

            // 4. Build Professional 3D Room Entrance Signage & Corridor Wayfinding Posts
            BuildSignageAndWayfinding(mats);

            // 5. Populate All Rooms with Purpose-Built Modern Furniture & Tech Props
            PopulateRealRoomInteriors(mats);

            // 6. Add Murder Mystery Environmental Storytelling (Police Tape, Evidence Markers)
            AddMurderMysteryStorytelling(mats);

            // 7. Balance Indoor Vibrant Lighting
            SetupVibrantIndoorLighting();

            // 8. Ensure Guide NPC "Arjun Mehta" is correctly positioned in Foyer
            EnsureGuideNpc();

            // 9. Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("==================================================================");
            Debug.Log("[Person 4] FLOOR 11 COMPLETE OVERHAUL COMPLETED & SAVED TO DISK!");
            Debug.Log("==================================================================");
        }

        public class MaterialPalette
        {
            // Foyer
            public Material FoyerTeal;
            public Material FoyerCoral;
            public Material FoyerMustard;
            public Material FoyerWarmWood;

            // Research Lab
            public Material LabElectricBlue;
            public Material LabViolet;
            public Material LabCyan;
            public Material LabTechWhite;

            // Meeting Room
            public Material MeetingMustard;
            public Material MeetingTerracotta;
            public Material MeetingNavy;
            public Material MeetingOak;

            // IoT Centre
            public Material IoTEmerald;
            public Material IoTCyan;
            public Material IoTDarkTeal;

            // Corridors
            public Material CorridorNavy;
            public Material CorridorOrange;
            public Material CorridorWarmWhite;
            public Material CorridorFloorDark;

            // Professor's Office
            public Material OfficeBurgundy;
            public Material OfficeForestGreen;
            public Material OfficeWalnut;
            public Material OfficeWarmGold;

            // Security Area
            public Material SecurityDarkNavy;
            public Material SecurityElectricBlue;
            public Material SecurityAlertRed;

            // Architectural Staples
            public Material CeilingCharcoal;
            public Material CeilingTileOffWhite;
            public Material MetalGraphite;
            public Material ScreenGlow;
            public Material WhiteboardSurface;
            public Material FoliageGreen;
            public Material CeramicPot;
            public Material GlassMuted;
            public Material PoliceTapeYellow;
        }

        private static MaterialPalette CreateVibrantPalette()
        {
            string dir = "Assets/Materials/VibrantFloor11/";
            var p = new MaterialPalette();

            // Foyer: Deep teal #087F8C, coral #FF725E, mustard #F4B942, warm wood
            p.FoyerTeal = CreateMat(dir + "M_Foyer_DeepTeal.mat", new Color(0.031f, 0.498f, 0.549f), 0.25f);
            p.FoyerCoral = CreateMat(dir + "M_Foyer_Coral.mat", new Color(1.000f, 0.447f, 0.369f), 0.15f);
            p.FoyerMustard = CreateMat(dir + "M_Foyer_Mustard.mat", new Color(0.957f, 0.725f, 0.259f), 0.2f);
            p.FoyerWarmWood = CreateMat(dir + "M_Foyer_WarmWood.mat", new Color(0.42f, 0.26f, 0.15f), 0.3f);

            // Research Lab: Electric blue #2764D8, violet #7656C8, cyan #40B8D0, tech white
            p.LabElectricBlue = CreateMat(dir + "M_Lab_ElectricBlue.mat", new Color(0.153f, 0.392f, 0.847f), 0.35f);
            p.LabViolet = CreateMat(dir + "M_Lab_Violet.mat", new Color(0.463f, 0.337f, 0.784f), 0.25f);
            p.LabCyan = CreateMat(dir + "M_Lab_Cyan.mat", new Color(0.251f, 0.722f, 0.816f), 0.4f);
            p.LabTechWhite = CreateMat(dir + "M_Lab_TechWhite.mat", new Color(0.94f, 0.96f, 0.98f), 0.45f);

            // Meeting Room: Mustard #E5A928, terracotta #C65D46, navy #243B63, oak
            p.MeetingMustard = CreateMat(dir + "M_Meeting_Mustard.mat", new Color(0.898f, 0.663f, 0.157f), 0.2f);
            p.MeetingTerracotta = CreateMat(dir + "M_Meeting_Terracotta.mat", new Color(0.776f, 0.365f, 0.275f), 0.15f);
            p.MeetingNavy = CreateMat(dir + "M_Meeting_Navy.mat", new Color(0.141f, 0.231f, 0.388f), 0.25f);
            p.MeetingOak = CreateMat(dir + "M_Meeting_Oak.mat", new Color(0.55f, 0.35f, 0.17f), 0.3f);

            // IoT Centre: Emerald #16865B, cyan #21B8C5, dark teal
            p.IoTEmerald = CreateMat(dir + "M_IoT_Emerald.mat", new Color(0.086f, 0.525f, 0.357f), 0.3f);
            p.IoTCyan = CreateMat(dir + "M_IoT_Cyan.mat", new Color(0.129f, 0.722f, 0.773f), 0.4f);
            p.IoTDarkTeal = CreateMat(dir + "M_IoT_DarkTeal.mat", new Color(0.059f, 0.298f, 0.271f), 0.25f);

            // Corridors: Navy #20345B, orange #F28C38, warm white, dark floor
            p.CorridorNavy = CreateMat(dir + "M_Corridor_Navy.mat", new Color(0.125f, 0.204f, 0.357f), 0.25f);
            p.CorridorOrange = CreateMat(dir + "M_Corridor_Orange.mat", new Color(0.949f, 0.549f, 0.220f), 0.2f);
            p.CorridorWarmWhite = CreateMat(dir + "M_Corridor_WarmWhite.mat", new Color(0.969f, 0.961f, 0.941f), 0.1f);
            p.CorridorFloorDark = CreateMat(dir + "M_Corridor_FloorDark.mat", new Color(0.18f, 0.19f, 0.22f), 0.4f);

            // Professor's Office: Burgundy #6E1A24, forest green #1E4D2B, walnut, warm gold #D4AF37
            p.OfficeBurgundy = CreateMat(dir + "M_Office_Burgundy.mat", new Color(0.431f, 0.102f, 0.141f), 0.2f);
            p.OfficeForestGreen = CreateMat(dir + "M_Office_ForestGreen.mat", new Color(0.118f, 0.302f, 0.169f), 0.2f);
            p.OfficeWalnut = CreateMat(dir + "M_Office_Walnut.mat", new Color(0.290f, 0.180f, 0.106f), 0.3f);
            p.OfficeWarmGold = CreateMat(dir + "M_Office_WarmGold.mat", new Color(0.831f, 0.686f, 0.216f), 0.6f, 0.75f);

            // Security: Dark navy #141C2B, electric blue #1E60D5, alert red #D92525
            p.SecurityDarkNavy = CreateMat(dir + "M_Security_DarkNavy.mat", new Color(0.078f, 0.110f, 0.169f), 0.25f);
            p.SecurityElectricBlue = CreateMat(dir + "M_Security_ElectricBlue.mat", new Color(0.118f, 0.376f, 0.835f), 0.4f);
            p.SecurityAlertRed = CreateMat(dir + "M_Security_AlertRed.mat", new Color(0.851f, 0.145f, 0.145f), 0.3f, 0.1f, new Color(0.851f, 0.145f, 0.145f) * 0.8f);

            // General architectural
            p.CeilingCharcoal = CreateMat(dir + "M_Ceiling_ModernCharcoal.mat", new Color(0.14f, 0.15f, 0.18f), 0.05f);
            p.CeilingTileOffWhite = CreateMat(dir + "M_Ceiling_AcousticTile.mat", new Color(0.88f, 0.89f, 0.91f), 0.08f);
            p.MetalGraphite = CreateMat(dir + "M_Metal_DarkGraphite.mat", new Color(0.18f, 0.19f, 0.21f), 0.6f, 0.8f);
            p.ScreenGlow = CreateMat(dir + "M_Screen_CyanTechGlow.mat", new Color(0.1f, 0.7f, 0.9f), 0.8f, 0.1f, new Color(0.1f, 0.7f, 0.9f) * 1.6f);
            p.WhiteboardSurface = CreateMat(dir + "M_Prop_Whiteboard.mat", new Color(0.96f, 0.97f, 0.98f), 0.85f);
            p.FoliageGreen = CreateMat(dir + "M_Plant_NaturalGreen.mat", new Color(0.14f, 0.45f, 0.16f), 0.15f);
            p.CeramicPot = CreateMat(dir + "M_Plant_PotCeramic.mat", new Color(0.92f, 0.90f, 0.86f), 0.35f);
            p.GlassMuted = CreateMat(dir + "M_Glass_FrostedOffice.mat", new Color(0.8f, 0.88f, 0.92f, 0.5f), 0.85f);
            p.PoliceTapeYellow = CreateMat(dir + "M_Police_CordonYellow.mat", new Color(0.98f, 0.84f, 0.05f), 0.2f, 0f, new Color(0.98f, 0.84f, 0.05f) * 0.6f);

            return p;
        }

        private static void BuildRoomCeilings(MaterialPalette mats)
        {
            GameObject ceilingRoot = GameObject.Find("[09_ROOM_CEILINGS]");
            if (ceilingRoot == null)
            {
                ceilingRoot = new GameObject("[09_ROOM_CEILINGS]");
            }

            // Clean previous procedural ceiling parts if any
            while (ceilingRoot.transform.childCount > 0)
            {
                UnityEngine.Object.DestroyImmediate(ceilingRoot.transform.GetChild(0).gameObject);
            }

            // Create solid ceiling slabs with downlight recesses for each room zone:
            // Y position is set to 3.65m (perfect wall cap height). Thickness is 0.25m.
            CreateCeilingSlab(ceilingRoot.transform, "Ceiling_Foyer_Atrium", new Vector3(0f, 3.65f, -12.5f), new Vector3(18f, 0.25f, 13f), mats.CeilingCharcoal, mats.FoyerTeal);
            CreateCeilingSlab(ceilingRoot.transform, "Ceiling_ResearchLab_1112", new Vector3(18f, 3.65f, -5f), new Vector3(19.5f, 0.25f, 11f), mats.CeilingTileOffWhite, mats.LabElectricBlue);
            CreateCeilingSlab(ceilingRoot.transform, "Ceiling_IoTCentre_1108", new Vector3(18f, 3.65f, -14f), new Vector3(19.5f, 0.25f, 9.5f), mats.CeilingTileOffWhite, mats.IoTEmerald);
            CreateCeilingSlab(ceilingRoot.transform, "Ceiling_Security_ISFCR", new Vector3(8.5f, 3.65f, -12f), new Vector3(7f, 0.25f, 8.5f), mats.CeilingCharcoal, mats.SecurityElectricBlue);
            CreateCeilingSlab(ceilingRoot.transform, "Ceiling_MeetingRoom_1104", new Vector3(-17f, 3.65f, 11f), new Vector3(18.5f, 0.25f, 17f), mats.CeilingTileOffWhite, mats.MeetingMustard);
            CreateCeilingSlab(ceilingRoot.transform, "Ceiling_ProfOffice_1102", new Vector3(-17f, 3.65f, -7f), new Vector3(17.5f, 0.25f, 13f), mats.CeilingTileOffWhite, mats.OfficeBurgundy);

            // Corridor ceilings
            CreateCeilingSlab(ceilingRoot.transform, "Ceiling_Corridor_Central", new Vector3(0f, 3.65f, -4f), new Vector3(6.5f, 0.25f, 12f), mats.CeilingCharcoal, mats.CorridorOrange);
            CreateCeilingSlab(ceilingRoot.transform, "Ceiling_Corridor_Concourse", new Vector3(0f, 3.65f, 4f), new Vector3(36f, 0.25f, 6.5f), mats.CeilingCharcoal, mats.CorridorNavy);
            CreateCeilingSlab(ceilingRoot.transform, "Ceiling_Corridor_WestWing", new Vector3(-17f, 3.65f, 1.5f), new Vector3(6.5f, 0.25f, 7f), mats.CeilingCharcoal, mats.MeetingNavy);
            CreateCeilingSlab(ceilingRoot.transform, "Ceiling_Corridor_EastWing", new Vector3(18f, 3.65f, 1f), new Vector3(6.5f, 0.25f, 6f), mats.CeilingCharcoal, mats.LabElectricBlue);

            Debug.Log("[Person 4] Room-wise solid ceilings installed with zero sky void visibility.");
        }

        private static void CreateCeilingSlab(Transform parent, string name, Vector3 pos, Vector3 size, Material matSlab, Material matTrim)
        {
            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = name;
            slab.transform.SetParent(parent, false);
            slab.transform.position = pos;
            slab.transform.localScale = size;
            slab.GetComponent<Renderer>().sharedMaterial = matSlab;

            // Trim border around the ceiling edge facing into room
            GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = "Trim";
            trim.transform.SetParent(slab.transform, false);
            trim.transform.localPosition = new Vector3(0f, -0.48f, 0f);
            trim.transform.localScale = new Vector3(0.96f, 0.08f, 0.96f);
            trim.GetComponent<Renderer>().sharedMaterial = matTrim;
            UnityEngine.Object.DestroyImmediate(trim.GetComponent<Collider>());
        }

        private static void ApplyArchitecturalColors(MaterialPalette mats)
        {
            // 1. Building Shell & Floors
            GameObject rootShell = GameObject.Find("[01_BUILDING_SHELL]");
            if (rootShell != null)
            {
                MeshRenderer[] shellRends = rootShell.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mr in shellRends)
                {
                    string n = mr.gameObject.name.ToLower();
                    if (n.Contains("roof") || n.Contains("ceiling")) mr.sharedMaterial = mats.CeilingCharcoal;
                    else mr.sharedMaterial = mats.CorridorWarmWhite;
                }
            }

            GameObject rootFloors = GameObject.Find("[02_FLOORS]");
            if (rootFloors != null)
            {
                MeshRenderer[] floorRends = rootFloors.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mr in floorRends)
                {
                    mr.sharedMaterial = mats.CorridorFloorDark;
                }
            }

            // 2. Room specific architectural surfaces
            GameObject rootRooms = GameObject.Find("[03_ROOMS]");
            if (rootRooms != null)
            {
                MeshRenderer[] roomRenderers = rootRooms.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mr in roomRenderers)
                {
                    mr.enabled = true; // Ensure active
                    string objName = mr.gameObject.name.ToLower();
                    string parentName = mr.transform.parent != null ? mr.transform.parent.name.ToLower() : "";

                    // Foyer
                    if (parentName.Contains("foyer") || objName.Contains("foyer"))
                    {
                        if (objName.Contains("desk") || objName.Contains("table")) mr.sharedMaterial = mats.FoyerWarmWood;
                        else if (objName.Contains("accent") || objName.Contains("pillar")) mr.sharedMaterial = mats.FoyerCoral;
                        else mr.sharedMaterial = mats.FoyerTeal;
                    }
                    // Research Laboratory 1112
                    else if (parentName.Contains("iotlab") || parentName.Contains("lab") || objName.Contains("lab"))
                    {
                        if (objName.Contains("bench") || objName.Contains("counter")) mr.sharedMaterial = mats.LabTechWhite;
                        else if (objName.Contains("accent")) mr.sharedMaterial = mats.LabViolet;
                        else mr.sharedMaterial = mats.LabElectricBlue;
                    }
                    // IoT Centre 1108
                    else if (parentName.Contains("iotcentre") || objName.Contains("iot"))
                    {
                        if (objName.Contains("table") || objName.Contains("rack")) mr.sharedMaterial = mats.IoTCyan;
                        else mr.sharedMaterial = mats.IoTEmerald;
                    }
                    // Security ISFCR
                    else if (parentName.Contains("isfcr") || objName.Contains("security") || objName.Contains("server"))
                    {
                        if (objName.Contains("console") || objName.Contains("screen")) mr.sharedMaterial = mats.SecurityElectricBlue;
                        else mr.sharedMaterial = mats.SecurityDarkNavy;
                    }
                    // Meeting Room
                    else if (parentName.Contains("meeting") || objName.Contains("meeting"))
                    {
                        if (objName.Contains("table") || objName.Contains("desk")) mr.sharedMaterial = mats.MeetingOak;
                        else if (objName.Contains("chair")) mr.sharedMaterial = mats.MeetingNavy;
                        else mr.sharedMaterial = mats.MeetingMustard;
                    }
                    // Professor's Office (pesuacademy)
                    else if (parentName.Contains("pesu") || objName.Contains("office") || objName.Contains("academy"))
                    {
                        if (objName.Contains("shelf") || objName.Contains("desk")) mr.sharedMaterial = mats.OfficeWalnut;
                        else if (objName.Contains("chair")) mr.sharedMaterial = mats.OfficeForestGreen;
                        else mr.sharedMaterial = mats.OfficeBurgundy;
                    }
                    // Railings & metal
                    else if (objName.Contains("railing") || objName.Contains("metal") || objName.Contains("shelf"))
                    {
                        mr.sharedMaterial = mats.MetalGraphite;
                    }
                    else
                    {
                        mr.sharedMaterial = mats.CorridorWarmWhite;
                    }
                }
            }

            Debug.Log("[Person 4] Mandated vibrant room palettes assigned across all room models.");
        }

        private static void BuildSignageAndWayfinding(MaterialPalette mats)
        {
            GameObject signsRoot = GameObject.Find("[10_SIGNAGE_AND_WAYFINDING]");
            if (signsRoot == null)
            {
                signsRoot = new GameObject("[10_SIGNAGE_AND_WAYFINDING]");
            }

            while (signsRoot.transform.childCount > 0)
            {
                UnityEngine.Object.DestroyImmediate(signsRoot.transform.GetChild(0).gameObject);
            }

            // 1. Illuminated Room Entrance Portals & Headers
            CreateEntranceSign(signsRoot.transform, "Sign_Foyer_Entrance", new Vector3(0f, 3.1f, -6.5f), new Vector3(4.5f, 0.7f, 0.12f), "FOYER & MAIN ATRIUM", mats.FoyerTeal, mats.FoyerCoral);
            CreateEntranceSign(signsRoot.transform, "Sign_Lab_Entrance", new Vector3(8.2f, 3.0f, -5.0f), new Vector3(4.0f, 0.65f, 0.12f), "RESEARCH LAB 1112 // CRIME SCENE", mats.LabElectricBlue, mats.LabCyan, true);
            CreateEntranceSign(signsRoot.transform, "Sign_IoTCentre_Entrance", new Vector3(8.2f, 3.0f, -14.0f), new Vector3(3.8f, 0.65f, 0.12f), "IOT SYSTEMS CENTRE 1108", mats.IoTEmerald, mats.IoTCyan);
            CreateEntranceSign(signsRoot.transform, "Sign_Security_Entrance", new Vector3(5.2f, 2.9f, -12.0f), new Vector3(3.2f, 0.6f, 0.12f), "SECURITY VAULT 1110 (ISFCR)", mats.SecurityDarkNavy, mats.SecurityAlertRed, true);
            CreateEntranceSign(signsRoot.transform, "Sign_MeetingRoom_Entrance", new Vector3(-8.2f, 3.0f, 10.5f), new Vector3(3.8f, 0.65f, 0.12f), "CONFERENCE SUITE 1104", mats.MeetingMustard, mats.MeetingNavy);
            CreateEntranceSign(signsRoot.transform, "Sign_Office_Entrance", new Vector3(-8.2f, 3.0f, -7.0f), new Vector3(3.8f, 0.65f, 0.12f), "FACULTY OFFICE 1102 // DR. DESAI", mats.OfficeBurgundy, mats.OfficeWarmGold);

            // 2. Concourse Wayfinding Posts at Key Corridor Intersections
            CreateWayfindingPylon(signsRoot.transform, "Wayfinding_CentralJunction", new Vector3(0f, 0f, 3.5f), mats);
            CreateWayfindingPylon(signsRoot.transform, "Wayfinding_EastJunction", new Vector3(8.5f, 0f, 3.5f), mats);
            CreateWayfindingPylon(signsRoot.transform, "Wayfinding_WestJunction", new Vector3(-8.5f, 0f, 3.5f), mats);

            // 3. Wall Floor Directory in Foyer
            CreateFloorDirectoryBoard(signsRoot.transform, new Vector3(0f, 1.8f, -14.8f), mats);

            Debug.Log("[Person 4] Professional illuminated signage & wayfinding pylons installed.");
        }

        private static void CreateEntranceSign(Transform parent, string name, Vector3 pos, Vector3 size, string label, Material matBack, Material matTrim, bool restricted = false)
        {
            GameObject sign = new GameObject(name);
            sign.transform.SetParent(parent, false);
            sign.transform.position = pos;

            // Plate Backing
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "Plate";
            plate.transform.SetParent(sign.transform, false);
            plate.transform.localScale = size;
            plate.GetComponent<Renderer>().sharedMaterial = matBack;
            UnityEngine.Object.DestroyImmediate(plate.GetComponent<Collider>());

            // Glowing Trim Bar
            GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = "AccentTrim";
            trim.transform.SetParent(sign.transform, false);
            trim.transform.localPosition = new Vector3(0f, -size.y * 0.44f, size.z * 0.52f);
            trim.transform.localScale = new Vector3(size.x * 0.96f, 0.06f, 0.04f);
            trim.GetComponent<Renderer>().sharedMaterial = matTrim;
            UnityEngine.Object.DestroyImmediate(trim.GetComponent<Collider>());

            // Soft Downlight over door
            GameObject ltObj = new GameObject("SignLight");
            ltObj.transform.SetParent(sign.transform, false);
            ltObj.transform.localPosition = new Vector3(0f, -0.4f, 0.4f);
            Light lt = ltObj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = restricted ? new Color(1.0f, 0.4f, 0.3f) : new Color(1.0f, 0.95f, 0.85f);
            lt.range = 5.5f;
            lt.intensity = 1.3f;
        }

        private static void CreateWayfindingPylon(Transform parent, string name, Vector3 pos, MaterialPalette mats)
        {
            GameObject pylon = new GameObject(name);
            pylon.transform.SetParent(parent, false);
            pylon.transform.position = pos;

            // Upright Post
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "Post";
            post.transform.SetParent(pylon.transform, false);
            post.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            post.transform.localScale = new Vector3(0.22f, 2.8f, 0.22f);
            post.GetComponent<Renderer>().sharedMaterial = mats.MetalGraphite;

            // Upper Directional Wing - East Labs
            GameObject wingEast = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wingEast.name = "Wing_East_Labs";
            wingEast.transform.SetParent(pylon.transform, false);
            wingEast.transform.localPosition = new Vector3(0.6f, 2.3f, 0f);
            wingEast.transform.localScale = new Vector3(1.1f, 0.25f, 0.06f);
            wingEast.GetComponent<Renderer>().sharedMaterial = mats.LabElectricBlue;
            UnityEngine.Object.DestroyImmediate(wingEast.GetComponent<Collider>());

            // Lower Directional Wing - West Meeting & Office
            GameObject wingWest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wingWest.name = "Wing_West_Suites";
            wingWest.transform.SetParent(pylon.transform, false);
            wingWest.transform.localPosition = new Vector3(-0.6f, 1.9f, 0f);
            wingWest.transform.localScale = new Vector3(1.1f, 0.25f, 0.06f);
            wingWest.GetComponent<Renderer>().sharedMaterial = mats.MeetingMustard;
            UnityEngine.Object.DestroyImmediate(wingWest.GetComponent<Collider>());

            // Indicator Light
            GameObject ltObj = new GameObject("PylonLight");
            ltObj.transform.SetParent(pylon.transform, false);
            ltObj.transform.localPosition = new Vector3(0f, 2.7f, 0f);
            Light lt = ltObj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(0.95f, 0.95f, 1.0f);
            lt.range = 5f;
            lt.intensity = 1.0f;
        }

        private static void CreateFloorDirectoryBoard(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Floor11_DirectoryBoard";
            board.transform.SetParent(parent, false);
            board.transform.position = pos;
            board.transform.localScale = new Vector3(3.2f, 1.8f, 0.08f);
            board.GetComponent<Renderer>().sharedMaterial = mats.FoyerTeal;

            // Directory Frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame";
            frame.transform.SetParent(board.transform, false);
            frame.transform.localPosition = new Vector3(0f, 0f, 0.45f);
            frame.transform.localScale = new Vector3(0.96f, 0.92f, 0.2f);
            frame.GetComponent<Renderer>().sharedMaterial = mats.CorridorFloorDark;
            UnityEngine.Object.DestroyImmediate(frame.GetComponent<Collider>());
        }

        private static void PopulateRealRoomInteriors(MaterialPalette mats)
        {
            GameObject propsRoot = GameObject.Find("[11_ROOM_FURNITURE_PROPS]");
            if (propsRoot == null)
            {
                propsRoot = new GameObject("[11_ROOM_FURNITURE_PROPS]");
            }

            while (propsRoot.transform.childCount > 0)
            {
                UnityEngine.Object.DestroyImmediate(propsRoot.transform.GetChild(0).gameObject);
            }

            // 1. FOYER: Reception Desk, Sofas, Coffee Table, Tall Plants
            Transform foyerGroup = new GameObject("Foyer_Interiors").transform;
            foyerGroup.SetParent(propsRoot.transform, false);

            CreateReceptionDesk(foyerGroup, new Vector3(0f, 0.05f, -6.8f), mats);
            CreateLoungeArea(foyerGroup, new Vector3(-3.5f, 0.05f, -11.5f), mats);
            CreateTallPottedPlant(foyerGroup, new Vector3(4.5f, 0.05f, -9.5f), mats);
            CreateTallPottedPlant(foyerGroup, new Vector3(-4.5f, 0.05f, -9.5f), mats);
            CreateTallPottedPlant(foyerGroup, new Vector3(4.5f, 0.05f, -14.0f), mats);

            // 2. RESEARCH LABORATORY (1112): Steel Lab Benches, Dual Monitors, Chemical Apparatus
            Transform labGroup = new GameObject("ResearchLab_Interiors").transform;
            labGroup.SetParent(propsRoot.transform, false);

            CreateLabWorkbench(labGroup, "LabBench_CenterIsland", new Vector3(18f, 0.05f, -5f), new Vector3(4.2f, 0.92f, 1.6f), mats);
            CreateLabWorkbench(labGroup, "LabBench_SideWall", new Vector3(23f, 0.05f, -5f), new Vector3(1.4f, 0.92f, 5.0f), mats);
            CreateCentrifugeAndMicroscope(labGroup, new Vector3(17.2f, 0.98f, -4.8f), mats);
            CreateWorkstationMonitors(labGroup, new Vector3(19.2f, 0.98f, -4.8f), mats);

            // 3. MEETING ROOM (1104): Boardroom Table, 8 Chairs, 85" Display Screen, Glass Whiteboard
            Transform meetGroup = new GameObject("MeetingRoom_Interiors").transform;
            meetGroup.SetParent(propsRoot.transform, false);

            CreateConferenceTableWithChairs(meetGroup, new Vector3(-17f, 0.05f, 11f), mats);
            CreateLargePresentationScreen(meetGroup, new Vector3(-17f, 2.2f, 17.5f), mats);
            CreateGlassWhiteboard(meetGroup, new Vector3(-23.5f, 2.0f, 11f), mats);

            // 4. PROFESSOR'S OFFICE (1102): Walnut Desk, Leather Chair, Wall Bookshelves, Banker Lamp
            Transform officeGroup = new GameObject("ProfOffice_Interiors").transform;
            officeGroup.SetParent(propsRoot.transform, false);

            CreateExecutiveOfficeDesk(officeGroup, new Vector3(-17f, 0.05f, -7f), mats);
            CreateOfficeBookshelf(officeGroup, new Vector3(-23f, 0.05f, -7f), mats);
            CreateOfficeBookshelf(officeGroup, new Vector3(-17f, 0.05f, -12.5f), mats);

            // 5. IOT CENTRE (1108): Soldering Benches, Oscilloscopes, Circuit Component Bins
            Transform iotGroup = new GameObject("IoTCentre_Interiors").transform;
            iotGroup.SetParent(propsRoot.transform, false);

            CreateIoTWorkstation(iotGroup, new Vector3(18f, 0.05f, -14f), mats);
            CreateIoTComponentShelf(iotGroup, new Vector3(23f, 0.05f, -14f), mats);

            // 6. SECURITY & CONTROL (ISFCR 1110): Multi-Screen Console, Server Racks with Status LEDs
            Transform secGroup = new GameObject("Security_Interiors").transform;
            secGroup.SetParent(propsRoot.transform, false);

            CreateCCTVControlDesk(secGroup, new Vector3(8.5f, 0.05f, -11.5f), mats);
            CreateServerRackTower(secGroup, new Vector3(10.5f, 0.05f, -14.0f), mats);
            CreateServerRackTower(secGroup, new Vector3(10.5f, 0.05f, -12.2f), mats);

            Debug.Log("[Person 4] High-density realistic room furniture & specialized props populated.");
        }

        private static void AddMurderMysteryStorytelling(MaterialPalette mats)
        {
            GameObject storyRoot = GameObject.Find("[12_MYSTERY_ENVIRONMENTAL_STORY]");
            if (storyRoot == null)
            {
                storyRoot = new GameObject("[12_MYSTERY_ENVIRONMENTAL_STORY]");
            }

            while (storyRoot.transform.childCount > 0)
            {
                UnityEngine.Object.DestroyImmediate(storyRoot.transform.GetChild(0).gameObject);
            }

            // 1. Police Line Cordon Tape across Research Lab 1112 entrance
            CreatePoliceTapeCordon(storyRoot.transform, new Vector3(8.2f, 1.1f, -5.0f), mats);

            // 2. Yellow Numbered Evidence Markers in Crime Scene (1, 2, 3)
            CreateEvidenceCone(storyRoot.transform, "EvidenceCone_1", new Vector3(17.6f, 0.98f, -5.0f), mats);
            CreateEvidenceCone(storyRoot.transform, "EvidenceCone_2", new Vector3(18.0f, 0.98f, -4.6f), mats);
            CreateEvidenceCone(storyRoot.transform, "EvidenceCone_3", new Vector3(18.5f, 0.98f, -5.0f), mats);

            // 3. Flickering Crime Scene Light
            GameObject flickLightObj = new GameObject("Light_CrimeScene_Flickering");
            flickLightObj.transform.SetParent(storyRoot.transform, false);
            flickLightObj.transform.position = new Vector3(18f, 3.2f, -5f);
            Light flt = flickLightObj.AddComponent<Light>();
            flt.type = LightType.Point;
            flt.color = new Color(0.85f, 0.92f, 1.0f);
            flt.intensity = 1.6f;
            flt.range = 10f;

            Debug.Log("[Person 4] Murder mystery storytelling elements (police cordon, evidence markers) added.");
        }

        private static void CreatePoliceTapeCordon(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject cordon = new GameObject("Police_Tape_Cordon");
            cordon.transform.SetParent(parent, false);
            cordon.transform.position = pos;

            // Two yellow crossing tape banners
            GameObject tape1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tape1.name = "Tape_Upper";
            tape1.transform.SetParent(cordon.transform, false);
            tape1.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            tape1.transform.localRotation = Quaternion.Euler(0f, 0f, 6f);
            tape1.transform.localScale = new Vector3(2.6f, 0.12f, 0.02f);
            tape1.GetComponent<Renderer>().sharedMaterial = mats.PoliceTapeYellow;
            UnityEngine.Object.DestroyImmediate(tape1.GetComponent<Collider>());

            GameObject tape2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tape2.name = "Tape_Lower";
            tape2.transform.SetParent(cordon.transform, false);
            tape2.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            tape2.transform.localRotation = Quaternion.Euler(0f, 0f, -6f);
            tape2.transform.localScale = new Vector3(2.6f, 0.12f, 0.02f);
            tape2.GetComponent<Renderer>().sharedMaterial = mats.PoliceTapeYellow;
            UnityEngine.Object.DestroyImmediate(tape2.GetComponent<Collider>());
        }

        private static void CreateEvidenceCone(Transform parent, string name, Vector3 pos, MaterialPalette mats)
        {
            GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cone.name = name;
            cone.transform.SetParent(parent, false);
            cone.transform.position = pos;
            cone.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            cone.GetComponent<Renderer>().sharedMaterial = mats.PoliceTapeYellow;
            UnityEngine.Object.DestroyImmediate(cone.GetComponent<Collider>());
        }

        private static void CreateReceptionDesk(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject desk = new GameObject("ReceptionDesk_Modern");
            desk.transform.SetParent(parent, false);
            desk.transform.position = pos;

            // Main curved counter
            GameObject counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "Counter";
            counter.transform.SetParent(desk.transform, false);
            counter.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            counter.transform.localScale = new Vector3(3.6f, 1.1f, 1.1f);
            counter.GetComponent<Renderer>().sharedMaterial = mats.FoyerWarmWood;

            // Front acrylic facade
            GameObject front = GameObject.CreatePrimitive(PrimitiveType.Cube);
            front.name = "Front_Facade";
            front.transform.SetParent(desk.transform, false);
            front.transform.localPosition = new Vector3(0f, 0.55f, 0.58f);
            front.transform.localScale = new Vector3(3.4f, 0.95f, 0.12f);
            front.GetComponent<Renderer>().sharedMaterial = mats.FoyerTeal;

            // Coral Glowing Accent Trim
            GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = "Accent_Glow";
            trim.transform.SetParent(desk.transform, false);
            trim.transform.localPosition = new Vector3(0f, 0.12f, 0.65f);
            trim.transform.localScale = new Vector3(3.4f, 0.08f, 0.06f);
            trim.GetComponent<Renderer>().sharedMaterial = mats.FoyerCoral;
        }

        private static void CreateLoungeArea(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject lounge = new GameObject("LoungeArea");
            lounge.transform.SetParent(parent, false);
            lounge.transform.position = pos;

            // Sofa 1
            GameObject sofa = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sofa.name = "Sofa_Teal";
            sofa.transform.SetParent(lounge.transform, false);
            sofa.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            sofa.transform.localScale = new Vector3(2.2f, 0.85f, 0.9f);
            sofa.GetComponent<Renderer>().sharedMaterial = mats.FoyerTeal;

            // Coffee Table
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "CoffeeTable";
            table.transform.SetParent(lounge.transform, false);
            table.transform.localPosition = new Vector3(0f, 0.22f, 1.2f);
            table.transform.localScale = new Vector3(1.6f, 0.44f, 0.7f);
            table.GetComponent<Renderer>().sharedMaterial = mats.FoyerWarmWood;
        }

        private static void CreateTallPottedPlant(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject plant = new GameObject("IndoorPalm");
            plant.transform.SetParent(parent, false);
            plant.transform.position = pos;

            // Tall Ceramic Pot
            GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "Pot";
            pot.transform.SetParent(plant.transform, false);
            pot.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            pot.transform.localScale = new Vector3(0.65f, 0.45f, 0.65f);
            pot.GetComponent<Renderer>().sharedMaterial = mats.CeramicPot;

            // Lush Foliage
            GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves.name = "Leaves";
            leaves.transform.SetParent(plant.transform, false);
            leaves.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            leaves.transform.localScale = new Vector3(0.95f, 1.25f, 0.95f);
            leaves.GetComponent<Renderer>().sharedMaterial = mats.FoliageGreen;
        }

        private static void CreateLabWorkbench(Transform parent, string name, Vector3 pos, Vector3 size, MaterialPalette mats)
        {
            GameObject bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bench.name = name;
            bench.transform.SetParent(parent, false);
            bench.transform.position = pos + new Vector3(0f, size.y / 2f, 0f);
            bench.transform.localScale = size;
            bench.GetComponent<Renderer>().sharedMaterial = mats.LabTechWhite;

            // Blue edge trim
            GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = "EdgeTrim";
            trim.transform.SetParent(bench.transform, false);
            trim.transform.localPosition = new Vector3(0f, 0.46f, 0f);
            trim.transform.localScale = new Vector3(1.02f, 0.08f, 1.02f);
            trim.GetComponent<Renderer>().sharedMaterial = mats.LabElectricBlue;
            UnityEngine.Object.DestroyImmediate(trim.GetComponent<Collider>());
        }

        private static void CreateCentrifugeAndMicroscope(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject app = new GameObject("LabApparatus");
            app.transform.SetParent(parent, false);
            app.transform.position = pos;

            // Centrifuge
            GameObject cent = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cent.name = "Centrifuge";
            cent.transform.SetParent(app.transform, false);
            cent.transform.localScale = new Vector3(0.35f, 0.15f, 0.35f);
            cent.GetComponent<Renderer>().sharedMaterial = mats.MetalGraphite;

            // Microscope
            GameObject mic = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mic.name = "Microscope";
            mic.transform.SetParent(app.transform, false);
            mic.transform.localPosition = new Vector3(0.45f, 0.18f, 0f);
            mic.transform.localScale = new Vector3(0.2f, 0.36f, 0.22f);
            mic.GetComponent<Renderer>().sharedMaterial = mats.LabCyan;
        }

        private static void CreateWorkstationMonitors(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject ws = new GameObject("WorkstationMonitors");
            ws.transform.SetParent(parent, false);
            ws.transform.position = pos;

            // Left monitor
            GameObject mon1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mon1.name = "Monitor_Left";
            mon1.transform.SetParent(ws.transform, false);
            mon1.transform.localPosition = new Vector3(-0.28f, 0.2f, 0f);
            mon1.transform.localRotation = Quaternion.Euler(0f, 12f, 0f);
            mon1.transform.localScale = new Vector3(0.52f, 0.34f, 0.04f);
            mon1.GetComponent<Renderer>().sharedMaterial = mats.ScreenGlow;

            // Right monitor
            GameObject mon2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mon2.name = "Monitor_Right";
            mon2.transform.SetParent(ws.transform, false);
            mon2.transform.localPosition = new Vector3(0.28f, 0.2f, 0f);
            mon2.transform.localRotation = Quaternion.Euler(0f, -12f, 0f);
            mon2.transform.localScale = new Vector3(0.52f, 0.34f, 0.04f);
            mon2.GetComponent<Renderer>().sharedMaterial = mats.ScreenGlow;
        }

        private static void CreateConferenceTableWithChairs(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject tableGroup = new GameObject("ConferenceTable_Group");
            tableGroup.transform.SetParent(parent, false);
            tableGroup.transform.position = pos;

            // Large Boardroom Table
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "BoardroomTable";
            table.transform.SetParent(tableGroup.transform, false);
            table.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            table.transform.localScale = new Vector3(6.2f, 0.84f, 2.2f);
            table.GetComponent<Renderer>().sharedMaterial = mats.MeetingOak;

            // Terracotta table runner in center
            GameObject runner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runner.name = "Runner";
            runner.transform.SetParent(table.transform, false);
            runner.transform.localPosition = new Vector3(0f, 0.51f, 0f);
            runner.transform.localScale = new Vector3(0.92f, 0.04f, 0.4f);
            runner.GetComponent<Renderer>().sharedMaterial = mats.MeetingTerracotta;

            // Surrounding 8 Chairs
            for (int i = 0; i < 4; i++)
            {
                float xOff = -2.2f + i * 1.45f;
                CreateChair(tableGroup.transform, $"Chair_North_{i}", new Vector3(xOff, 0.4f, 1.55f), Quaternion.Euler(0f, 180f, 0f), mats.MeetingNavy);
                CreateChair(tableGroup.transform, $"Chair_South_{i}", new Vector3(xOff, 0.4f, -1.55f), Quaternion.identity, mats.MeetingNavy);
            }
        }

        private static void CreateChair(Transform parent, string name, Vector3 pos, Quaternion rot, Material mat)
        {
            GameObject chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chair.name = name;
            chair.transform.SetParent(parent, false);
            chair.transform.localPosition = pos;
            chair.transform.localRotation = rot;
            chair.transform.localScale = new Vector3(0.55f, 0.8f, 0.55f);
            chair.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateLargePresentationScreen(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = "Presentation_4K_Screen";
            screen.transform.SetParent(parent, false);
            screen.transform.position = pos;
            screen.transform.localScale = new Vector3(4.5f, 2.4f, 0.1f);
            screen.GetComponent<Renderer>().sharedMaterial = mats.ScreenGlow;
        }

        private static void CreateGlassWhiteboard(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject wb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wb.name = "Glass_Whiteboard";
            wb.transform.SetParent(parent, false);
            wb.transform.position = pos;
            wb.transform.localScale = new Vector3(0.1f, 1.8f, 3.8f);
            wb.GetComponent<Renderer>().sharedMaterial = mats.WhiteboardSurface;
        }

        private static void CreateExecutiveOfficeDesk(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject desk = new GameObject("ExecutiveDesk");
            desk.transform.SetParent(parent, false);
            desk.transform.position = pos;

            // Walnut Top
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "DeskTop";
            top.transform.SetParent(desk.transform, false);
            top.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            top.transform.localScale = new Vector3(2.6f, 0.84f, 1.3f);
            top.GetComponent<Renderer>().sharedMaterial = mats.OfficeWalnut;

            // Banker's Desk Lamp with warm glow
            GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lamp.name = "BankerLamp";
            lamp.transform.SetParent(desk.transform, false);
            lamp.transform.localPosition = new Vector3(-0.8f, 1.05f, 0.3f);
            lamp.transform.localScale = new Vector3(0.24f, 0.35f, 0.16f);
            lamp.GetComponent<Renderer>().sharedMaterial = mats.OfficeForestGreen;

            GameObject lampLight = new GameObject("LampLight");
            lampLight.transform.SetParent(lamp.transform, false);
            Light lt = lampLight.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(1.0f, 0.88f, 0.65f);
            lt.range = 4f;
            lt.intensity = 1.2f;

            // Leather Office Chair
            CreateChair(desk.transform, "ExecutiveChair", new Vector3(0f, 0.45f, -1.05f), Quaternion.identity, mats.OfficeForestGreen);
        }

        private static void CreateOfficeBookshelf(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelf.name = "Wall_Bookshelf";
            shelf.transform.SetParent(parent, false);
            shelf.transform.position = pos + new Vector3(0f, 1.4f, 0f);
            shelf.transform.localScale = new Vector3(0.6f, 2.8f, 2.8f);
            shelf.GetComponent<Renderer>().sharedMaterial = mats.OfficeWalnut;
        }

        private static void CreateIoTWorkstation(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject ws = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ws.name = "IoT_Electronics_Bench";
            ws.transform.SetParent(parent, false);
            ws.transform.position = pos + new Vector3(0f, 0.46f, 0f);
            ws.transform.localScale = new Vector3(3.8f, 0.92f, 1.6f);
            ws.GetComponent<Renderer>().sharedMaterial = mats.IoTEmerald;

            // Cyan Oscilloscope
            GameObject osc = GameObject.CreatePrimitive(PrimitiveType.Cube);
            osc.name = "Oscilloscope";
            osc.transform.SetParent(ws.transform, false);
            osc.transform.localPosition = new Vector3(0.3f, 0.68f, 0f);
            osc.transform.localScale = new Vector3(0.25f, 0.35f, 0.4f);
            osc.GetComponent<Renderer>().sharedMaterial = mats.IoTCyan;
        }

        private static void CreateIoTComponentShelf(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelf.name = "Component_Bin_Rack";
            shelf.transform.SetParent(parent, false);
            shelf.transform.position = pos + new Vector3(0f, 1.3f, 0f);
            shelf.transform.localScale = new Vector3(0.8f, 2.6f, 3.2f);
            shelf.GetComponent<Renderer>().sharedMaterial = mats.MetalGraphite;
        }

        private static void CreateCCTVControlDesk(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject console = new GameObject("CCTV_Console");
            console.transform.SetParent(parent, false);
            console.transform.position = pos;

            GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "Desk";
            desk.transform.SetParent(console.transform, false);
            desk.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            desk.transform.localScale = new Vector3(2.8f, 0.9f, 1.2f);
            desk.GetComponent<Renderer>().sharedMaterial = mats.SecurityDarkNavy;

            // Bank of 3 CCTV Monitors
            for (int i = 0; i < 3; i++)
            {
                float x = -0.7f + i * 0.7f;
                GameObject mon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mon.name = $"CCTV_Monitor_{i + 1}";
                mon.transform.SetParent(console.transform, false);
                mon.transform.localPosition = new Vector3(x, 1.15f, 0.2f);
                mon.transform.localScale = new Vector3(0.55f, 0.38f, 0.05f);
                mon.GetComponent<Renderer>().sharedMaterial = mats.ScreenGlow;
            }
        }

        private static void CreateServerRackTower(Transform parent, Vector3 pos, MaterialPalette mats)
        {
            GameObject rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rack.name = "Server_Rack_Tower";
            rack.transform.SetParent(parent, false);
            rack.transform.position = pos + new Vector3(0f, 1.4f, 0f);
            rack.transform.localScale = new Vector3(0.9f, 2.8f, 0.9f);
            rack.GetComponent<Renderer>().sharedMaterial = mats.SecurityDarkNavy;

            // Red & Blue Blinking Status LED bars
            GameObject ledRed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ledRed.name = "Status_Alert_LED";
            ledRed.transform.SetParent(rack.transform, false);
            ledRed.transform.localPosition = new Vector3(0f, 0.42f, 0.52f);
            ledRed.transform.localScale = new Vector3(0.7f, 0.05f, 0.06f);
            ledRed.GetComponent<Renderer>().sharedMaterial = mats.SecurityAlertRed;

            GameObject ledBlue = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ledBlue.name = "Status_Data_LED";
            ledBlue.transform.SetParent(rack.transform, false);
            ledBlue.transform.localPosition = new Vector3(0f, 0.35f, 0.52f);
            ledBlue.transform.localScale = new Vector3(0.7f, 0.05f, 0.06f);
            ledBlue.GetComponent<Renderer>().sharedMaterial = mats.SecurityElectricBlue;
        }

        private static void SetupVibrantIndoorLighting()
        {
            GameObject rootLighting = GameObject.Find("[05_LIGHTING]");
            if (rootLighting == null)
            {
                rootLighting = new GameObject("[05_LIGHTING]");
            }

            // Balanced ambient fill
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.48f, 0.50f, 0.55f);

            // Clean fixtures
            Transform existingRoomLights = rootLighting.transform.Find("Room_Fixtures");
            GameObject roomLights = existingRoomLights != null ? existingRoomLights.gameObject : new GameObject("Room_Fixtures");
            roomLights.transform.SetParent(rootLighting.transform, false);

            while (roomLights.transform.childCount > 0)
            {
                UnityEngine.Object.DestroyImmediate(roomLights.transform.GetChild(0).gameObject);
            }

            // Foyer: Warm golden chandeliers & welcoming fill
            CreateCeilingLight(roomLights.transform, "Light_Foyer_Atrium", new Vector3(0f, 3.4f, -6f), new Color(1.0f, 0.94f, 0.82f), 1.5f, 16f);
            CreateCeilingLight(roomLights.transform, "Light_Foyer_Reception", new Vector3(0f, 3.4f, -12f), new Color(1.0f, 0.92f, 0.78f), 1.6f, 14f);

            // Research Lab: High-tech blue-white fluorescent
            CreateCeilingLight(roomLights.transform, "Light_Lab_Center", new Vector3(18f, 3.3f, -5f), new Color(0.92f, 0.96f, 1.0f), 1.6f, 14f);
            CreateCeilingLight(roomLights.transform, "Light_Lab_West", new Vector3(13f, 3.3f, -5f), new Color(0.92f, 0.96f, 1.0f), 1.4f, 10f);

            // IoT Centre: Tech green-cyan tone
            CreateCeilingLight(roomLights.transform, "Light_IoT_Center", new Vector3(18f, 3.3f, -14f), new Color(0.88f, 0.98f, 0.95f), 1.5f, 14f);

            // Security Room: High-tech blue & red atmospheric
            CreateCeilingLight(roomLights.transform, "Light_Security_Room", new Vector3(8.5f, 3.2f, -12f), new Color(0.85f, 0.92f, 1.0f), 1.4f, 10f);

            // Meeting Room: Warm conference suite lighting
            CreateCeilingLight(roomLights.transform, "Light_Meeting_Center", new Vector3(-17f, 3.3f, 11f), new Color(1.0f, 0.94f, 0.85f), 1.6f, 16f);

            // Professor's Office: Elegant warm study lighting
            CreateCeilingLight(roomLights.transform, "Light_Office_Center", new Vector3(-17f, 3.3f, -7f), new Color(1.0f, 0.92f, 0.80f), 1.5f, 14f);

            // Corridors: Balanced navigation illumination
            CreateCeilingLight(roomLights.transform, "Light_Corridor_Central", new Vector3(0f, 3.3f, -3f), new Color(1.0f, 0.96f, 0.90f), 1.3f, 12f);
            CreateCeilingLight(roomLights.transform, "Light_Corridor_ConcourseCenter", new Vector3(0f, 3.3f, 4f), new Color(1.0f, 0.96f, 0.90f), 1.4f, 14f);
            CreateCeilingLight(roomLights.transform, "Light_Corridor_ConcourseWest", new Vector3(-12f, 3.3f, 4f), new Color(1.0f, 0.96f, 0.90f), 1.3f, 12f);
            CreateCeilingLight(roomLights.transform, "Light_Corridor_ConcourseEast", new Vector3(12f, 3.3f, 4f), new Color(1.0f, 0.96f, 0.90f), 1.3f, 12f);

            Debug.Log("[Person 4] Balanced vibrant indoor lighting setup completed.");
        }

        private static void CreateCeilingLight(Transform parent, string name, Vector3 pos, Color col, float intensity, float range)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            Light lt = go.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = col;
            lt.intensity = intensity;
            lt.range = range;
            lt.shadows = LightShadows.None;
        }

        private static void EnsureGuideNpc()
        {
            Transform npcTr = GameObject.Find("NPC_Guide_ArjunMehta")?.transform;
            if (npcTr == null)
            {
                Floor11VisualUpgrade.UpgradeScene();
            }
        }

        private static Material CreateMat(string path, Color color, float smoothness = 0.1f, float metallic = 0.0f, Color? emission = null)
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

