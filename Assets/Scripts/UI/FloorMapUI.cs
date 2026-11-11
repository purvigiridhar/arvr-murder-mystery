using System;
using System.Collections.Generic;
using UnityEngine;
using MurderMystery;
using MurderMystery.Audio;

namespace MurderMystery.UI
{
    /// <summary>
    /// Person 4 Floor 11 Navigation & Minimap System:
    /// - Fullscreen Floor Plan Map toggled with [M] or HUD button.
    /// - Compact HUD Minimap / Compass Widget in top-right corner.
    /// - Real-time Player Position & Facing Arrow.
    /// - Accurate 2D representation of Floor 11 rooms and corridors.
    /// - Room identity colour pins matching the mandated palette.
    /// - Dynamic Objective Wayfinder integrated with Person 3's MysteryManager (OBJ_1 - OBJ_9).
    /// - Room Legend & Controls Guide.
    /// </summary>
    public class FloorMapUI : MonoBehaviour
    {
        private static FloorMapUI instance;
        public static FloorMapUI Instance => instance;

        [Header("Settings")]
        [SerializeField] private bool showMinimap = true;
        [SerializeField] private KeyCode toggleMapKey = KeyCode.M;

        private bool isFullscreenMapOpen = false;
        public bool IsFullscreenMapOpen => isFullscreenMapOpen;

        private Transform playerTransform;
        private Camera playerCamera;
        private FirstPersonController playerController;
        private MysteryManager mysteryManager;

        // Floor 11 Coordinate Mapping:
        // Real-world Unity space: X spans [-28, +28], Z spans [-22, +22]
        private const float WorldMinX = -28f;
        private const float WorldMaxX = 28f;
        private const float WorldMinZ = -22f;
        private const float WorldMaxZ = 22f;

        // Color Palette matching brief:
        private readonly Color colFoyer = new Color(0.031f, 0.498f, 0.549f, 1f);     // #087F8C Deep Teal
        private readonly Color colLab = new Color(0.153f, 0.392f, 0.847f, 1f);       // #2764D8 Electric Blue
        private readonly Color colMeeting = new Color(0.898f, 0.663f, 0.157f, 1f);   // #E5A928 Mustard
        private readonly Color colIoT = new Color(0.086f, 0.525f, 0.357f, 1f);       // #16865B Emerald
        private readonly Color colOffice = new Color(0.431f, 0.102f, 0.141f, 1f);    // #6E1A24 Deep Burgundy
        private readonly Color colSecurity = new Color(0.078f, 0.110f, 0.169f, 1f);  // #141C2B Dark Navy
        private readonly Color colCorridor = new Color(0.125f, 0.204f, 0.357f, 1f);  // #20345B Navy Corridor
        private readonly Color colAccentOrange = new Color(0.949f, 0.549f, 0.220f);  // #F28C38
        private readonly Color colPlayer = new Color(0.2f, 0.95f, 0.3f, 1f);         // Bright Lime Player Dot
        private readonly Color colObjective = new Color(1.0f, 0.2f, 0.2f, 1f);      // Red Objective Pulsing Dot

        // Textures
        private Texture2D texWhite;
        private Texture2D texDarkBg;
        private Texture2D texCardBg;
        private Texture2D texBorder;
        private Texture2D texMinimapBg;

        private GUIStyle headerStyle;
        private GUIStyle roomLabelStyle;
        private GUIStyle legendStyle;
        private GUIStyle objectiveBannerStyle;
        private GUIStyle buttonStyle;
        private bool stylesInitialized = false;

        private struct RoomZone
        {
            public string Name;
            public string Code;
            public Rect WorldBounds; // X, Z, Width, Height
            public Color RoomColor;
            public Vector3 Center;
        }

        private RoomZone[] rooms;

        private void Awake()
        {
            if (instance != null && instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            InitRooms();
        }

        private void Start()
        {
            FindReferences();
        }

        private void FindReferences()
        {
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<FirstPersonController>();
            }
            if (playerController != null)
            {
                playerTransform = playerController.transform;
                playerCamera = playerController.GetComponentInChildren<Camera>();
            }
            if (mysteryManager == null)
            {
                mysteryManager = FindFirstObjectByType<MysteryManager>();
            }
        }

        private void InitRooms()
        {
            rooms = new RoomZone[]
            {
                new RoomZone
                {
                    Name = "FOYER & RECEPTION",
                    Code = "ZONE 11-A",
                    WorldBounds = new Rect(-9f, -18f, 18f, 10f),
                    RoomColor = colFoyer,
                    Center = new Vector3(0f, 0f, -13f)
                },
                new RoomZone
                {
                    Name = "RESEARCH LAB (CRIME SCENE)",
                    Code = "ROOM 1112",
                    WorldBounds = new Rect(8f, -9f, 19f, 10f),
                    RoomColor = colLab,
                    Center = new Vector3(18f, 0f, -5f)
                },
                new RoomZone
                {
                    Name = "IOT DEVELOPMENT CENTRE",
                    Code = "ROOM 1108",
                    WorldBounds = new Rect(8f, -19f, 19f, 9f),
                    RoomColor = colIoT,
                    Center = new Vector3(18f, 0f, -14f)
                },
                new RoomZone
                {
                    Name = "SECURITY & CONTROL (ISFCR)",
                    Code = "ROOM 1110",
                    WorldBounds = new Rect(4f, -16f, 6f, 8f),
                    RoomColor = colSecurity,
                    Center = new Vector3(8.5f, 0f, -12f)
                },
                new RoomZone
                {
                    Name = "CONFERENCE MEETING ROOM",
                    Code = "SUITE 1104",
                    WorldBounds = new Rect(-26f, 4f, 18f, 16f),
                    RoomColor = colMeeting,
                    Center = new Vector3(-17f, 0f, 11f)
                },
                new RoomZone
                {
                    Name = "PROFESSOR'S OFFICE",
                    Code = "OFFICE 1102",
                    WorldBounds = new Rect(-26f, -14f, 17f, 12f),
                    RoomColor = colOffice,
                    Center = new Vector3(-17f, 0f, -7f)
                },
                new RoomZone
                {
                    Name = "CENTRAL & EAST-WEST CORRIDORS",
                    Code = "CONCOURSE",
                    WorldBounds = new Rect(-8f, -7f, 16f, 24f),
                    RoomColor = colCorridor,
                    Center = new Vector3(0f, 0f, 5f)
                }
            };
        }

        private void Update()
        {
            if (playerTransform == null || mysteryManager == null)
            {
                FindReferences();
            }

            if (Input.GetKeyDown(toggleMapKey))
            {
                ToggleFullscreenMap();
            }
        }

        public void ToggleFullscreenMap()
        {
            isFullscreenMapOpen = !isFullscreenMapOpen;

            if (isFullscreenMapOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (playerController != null) playerController.enabled = false;
                AudioManager.Instance?.PlayJournalOpen();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (playerController != null) playerController.enabled = true;
                AudioManager.Instance?.PlayJournalClose();
            }
        }

        public void CloseFullscreenMap()
        {
            if (!isFullscreenMapOpen) return;
            isFullscreenMapOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (playerController != null) playerController.enabled = true;
            AudioManager.Instance?.PlayJournalClose();
        }

        private Texture2D MakeSolid(int w, int h, Color col)
        {
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
            Texture2D t = new Texture2D(w, h);
            t.SetPixels(pixels);
            t.Apply();
            return t;
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            texWhite = MakeSolid(2, 2, Color.white);
            texDarkBg = MakeSolid(2, 2, new Color(0.04f, 0.06f, 0.09f, 0.94f));
            texCardBg = MakeSolid(2, 2, new Color(0.08f, 0.12f, 0.18f, 0.98f));
            texBorder = MakeSolid(2, 2, new Color(0.24f, 0.35f, 0.50f, 1.0f));
            texMinimapBg = MakeSolid(2, 2, new Color(0.05f, 0.08f, 0.12f, 0.88f));

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.95f, 0.96f, 0.98f) }
            };

            roomLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            legendStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.85f, 0.88f, 0.92f) }
            };

            objectiveBannerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colAccentOrange }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            if (isFullscreenMapOpen)
            {
                DrawFullscreenMap();
            }
            else if (showMinimap)
            {
                DrawHUDMinimap();
            }
        }

        private Vector2 WorldToMapLocal(Vector3 worldPos, Rect mapRect)
        {
            float normX = Mathf.InverseLerp(WorldMinX, WorldMaxX, worldPos.x);
            // Unity Z corresponds to Map Y (inverted since Screen Y is top-to-bottom)
            float normY = Mathf.InverseLerp(WorldMaxZ, WorldMinZ, worldPos.z);

            return new Vector2(
                mapRect.x + normX * mapRect.width,
                mapRect.y + normY * mapRect.height
            );
        }

        private void DrawHUDMinimap()
        {
            float miniW = 190f;
            float miniH = 150f;
            float miniX = Screen.width - miniW - 20f;
            float miniY = 20f;

            Rect outerRect = new Rect(miniX, miniY, miniW, miniH);
            DrawBorder(outerRect, texMinimapBg, texBorder, 2f);

            // Title Bar
            GUI.DrawTexture(new Rect(miniX, miniY, miniW, 22f), texCardBg);
            GUI.Label(new Rect(miniX + 8f, miniY + 2f, miniW - 40f, 18f), "FLOOR 11 [M]", legendStyle);

            // Inner Map Area
            Rect innerMap = new Rect(miniX + 6f, miniY + 24f, miniW - 12f, miniH - 30f);
            GUI.DrawTexture(innerMap, texDarkBg);

            // Draw miniature room boxes
            foreach (var r in rooms)
            {
                Vector2 min = WorldToMapLocal(new Vector3(r.WorldBounds.xMin, 0f, r.WorldBounds.yMax), innerMap);
                Vector2 max = WorldToMapLocal(new Vector3(r.WorldBounds.xMax, 0f, r.WorldBounds.yMin), innerMap);
                Rect rRect = Rect.MinMaxRect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y), Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));

                Color c = r.RoomColor;
                c.a = 0.45f;
                DrawSolidColor(rRect, c);
            }

            // Draw Player Marker
            if (playerTransform != null)
            {
                Vector2 pPos = WorldToMapLocal(playerTransform.position, innerMap);
                if (innerMap.Contains(pPos))
                {
                    DrawPlayerDot(pPos, playerTransform.eulerAngles.y, 6f);
                }
            }

            // Draw Target Objective Marker
            Vector3 targetPos = GetCurrentObjectiveTargetPosition();
            if (targetPos != Vector3.zero)
            {
                Vector2 oPos = WorldToMapLocal(targetPos, innerMap);
                if (innerMap.Contains(oPos))
                {
                    DrawObjectiveMarker(oPos, 8f);
                }
            }
        }

        private void DrawFullscreenMap()
        {
            GUI.depth = -110;
            // Fullscreen backdrop
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texDarkBg);

            float winW = Mathf.Min(960f, Screen.width - 40f);
            float winH = Mathf.Min(660f, Screen.height - 40f);
            float winX = (Screen.width - winW) / 2f;
            float winY = (Screen.height - winH) / 2f;

            Rect mainWin = new Rect(winX, winY, winW, winH);
            DrawBorder(mainWin, texCardBg, texBorder, 2f);

            // Top Header Line
            GUI.DrawTexture(new Rect(winX + 2f, winY + 2f, winW - 4f, 4f), texWhite);

            // Header Title
            GUI.Label(new Rect(winX + 20f, winY + 16f, 400f, 26f), "FLOOR 11 // ARCHITECTURAL DIRECTORY & NAVIGATION", headerStyle);

            // Close Button
            if (GUI.Button(new Rect(winX + winW - 110f, winY + 14f, 90f, 30f), "CLOSE [M]", buttonStyle))
            {
                CloseFullscreenMap();
            }

            // Current Objective Banner
            string objText = GetCurrentObjectiveBanner();
            Rect objBannerRect = new Rect(winX + 20f, winY + 48f, winW - 40f, 28f);
            GUI.DrawTexture(objBannerRect, texDarkBg);
            GUI.Label(new Rect(objBannerRect.x + 10f, objBannerRect.y + 4f, objBannerRect.width - 20f, 20f), objText, objectiveBannerStyle);

            // Main Floor Plan Map Panel
            float mapPanelW = winW - 270f;
            float mapPanelH = winH - 100f;
            Rect mapRect = new Rect(winX + 20f, winY + 84f, mapPanelW, mapPanelH);
            DrawBorder(mapRect, texDarkBg, texBorder, 1.5f);

            // Draw Rooms on Floor Plan
            foreach (var r in rooms)
            {
                Vector2 min = WorldToMapLocal(new Vector3(r.WorldBounds.xMin, 0f, r.WorldBounds.yMax), mapRect);
                Vector2 max = WorldToMapLocal(new Vector3(r.WorldBounds.xMax, 0f, r.WorldBounds.yMin), mapRect);
                Rect rRect = Rect.MinMaxRect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y), Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));

                Color c = r.RoomColor;
                c.a = 0.55f;
                DrawSolidColor(rRect, c);
                DrawBorder(rRect, Color.clear, r.RoomColor, 1.5f);

                // Room Title inside Zone
                GUI.Label(new Rect(rRect.x, rRect.y + 6f, rRect.width, 16f), r.Name, roomLabelStyle);
                GUI.Label(new Rect(rRect.x, rRect.y + 22f, rRect.width, 14f), $"[{r.Code}]", legendStyle);
            }

            // Draw Objective Marker & Direction Line
            Vector3 targetPos = GetCurrentObjectiveTargetPosition();
            if (targetPos != Vector3.zero)
            {
                Vector2 oPos = WorldToMapLocal(targetPos, mapRect);
                DrawObjectiveMarker(oPos, 14f);
            }

            // Draw Player Position & Direction Arrow
            if (playerTransform != null)
            {
                Vector2 pPos = WorldToMapLocal(playerTransform.position, mapRect);
                DrawPlayerDot(pPos, playerTransform.eulerAngles.y, 10f);

                // Live Coordinates text
                string coordStr = $"YOUR POSITION: ({playerTransform.position.x:F1}m, {playerTransform.position.z:F1}m)";
                GUI.Label(new Rect(mapRect.x + 10f, mapRect.y + mapRect.height - 24f, 300f, 20f), coordStr, legendStyle);
            }

            // Right Sidebar: Legend & Room Guide
            float sideX = winX + mapPanelW + 35f;
            float sideY = winY + 84f;
            float sideW = winW - mapPanelW - 55f;

            GUI.Label(new Rect(sideX, sideY, sideW, 22f), "ZONE DIRECTORY", headerStyle);
            sideY += 28f;

            foreach (var r in rooms)
            {
                // Color swatch
                DrawSolidColor(new Rect(sideX, sideY + 2f, 14f, 14f), r.RoomColor);
                GUI.Label(new Rect(sideX + 22f, sideY, sideW - 22f, 18f), r.Name, legendStyle);
                sideY += 22f;
            }

            sideY += 10f;
            GUI.Label(new Rect(sideX, sideY, sideW, 20f), "INVESTIGATION ICONS", headerStyle);
            sideY += 26f;

            // Player Icon legend
            DrawSolidColor(new Rect(sideX, sideY + 3f, 12f, 12f), colPlayer);
            GUI.Label(new Rect(sideX + 20f, sideY, sideW - 20f, 18f), "Player (Live Location)", legendStyle);
            sideY += 22f;

            // Objective Icon legend
            DrawSolidColor(new Rect(sideX, sideY + 3f, 12f, 12f), colObjective);
            GUI.Label(new Rect(sideX + 20f, sideY, sideW - 20f, 18f), "Current Investigation Target", legendStyle);
            sideY += 22f;

            // Clue Marker legend
            DrawSolidColor(new Rect(sideX, sideY + 3f, 12f, 12f), colAccentOrange);
            GUI.Label(new Rect(sideX + 20f, sideY, sideW - 20f, 18f), "Evidence & Suspects", legendStyle);
            sideY += 30f;

            GUI.Label(new Rect(sideX, sideY, sideW, 40f), "CONTROLS:\n[WASD] Move   [E] Interact\n[J] Journal     [M] Close Map", legendStyle);
        }

        private void DrawSolidColor(Rect r, Color c)
        {
            Color old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, texWhite);
            GUI.color = old;
        }

        private void DrawBorder(Rect r, Texture2D bg, Texture2D border, float thickness)
        {
            GUI.DrawTexture(r, border);
            GUI.DrawTexture(new Rect(r.x + thickness, r.y + thickness, r.width - (thickness * 2f), r.height - (thickness * 2f)), bg);
        }

        private void DrawBorder(Rect r, Color bg, Color border, float thickness)
        {
            DrawSolidColor(r, border);
            DrawSolidColor(new Rect(r.x + thickness, r.y + thickness, r.width - (thickness * 2f), r.height - (thickness * 2f)), bg);
        }

        private void DrawPlayerDot(Vector2 pos, float angleY, float size)
        {
            float half = size / 2f;
            Rect dotRect = new Rect(pos.x - half, pos.y - half, size, size);
            DrawSolidColor(dotRect, colPlayer);

            // Direction arrow line
            float rad = angleY * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Sin(rad), -Mathf.Cos(rad)) * (size * 1.5f);
            DrawLine(pos, pos + dir, colPlayer, 2f);
        }

        private void DrawObjectiveMarker(Vector2 pos, float size)
        {
            float pulse = Mathf.PingPong(Time.unscaledTime * 3f, 1f);
            float actualSize = size + pulse * 4f;
            float half = actualSize / 2f;

            Color c = colObjective;
            c.a = 0.8f + pulse * 0.2f;

            Rect r = new Rect(pos.x - half, pos.y - half, actualSize, actualSize);
            DrawSolidColor(r, c);
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color, float width)
        {
            Color old = GUI.color;
            GUI.color = color;
            float angle = Vector2.SignedAngle(Vector2.right, end - start);
            float length = Vector2.Distance(start, end);

            Matrix4x4 matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, start);
            GUI.DrawTexture(new Rect(start.x, start.y - width / 2f, length, width), texWhite);
            GUI.matrix = matrix;
            GUI.color = old;
        }

        private string GetActiveObjectiveId()
        {
            if (mysteryManager == null) return "OBJ_1";

            IReadOnlyList<string> ids = mysteryManager.GetObjectiveIds();
            foreach (string id in ids)
            {
                if (mysteryManager.GetObjectiveStatus(id) == ObjectiveStatus.Active)
                {
                    return id;
                }
            }
            return "OBJ_1";
        }

        private Vector3 GetCurrentObjectiveTargetPosition()
        {
            if (mysteryManager == null) return Vector3.zero;

            string activeId = GetActiveObjectiveId();
            switch (activeId)
            {
                case "OBJ_1":
                case "OBJ_2":
                    return new Vector3(18f, 0.8f, -5f); // Research Lab 1112
                case "OBJ_3":
                case "OBJ_7":
                case "OBJ_8":
                case "OBJ_9":
                    return new Vector3(0f, 0f, -15f); // Suspects at Foyer
                case "OBJ_4":
                    return new Vector3(11f, 0.8f, -11f); // Security ISFCR
                case "OBJ_5":
                    return new Vector3(-16f, 0.8f, -5f); // Professor's office
                case "OBJ_6":
                    return new Vector3(18f, 0.8f, -14f); // IoT Centre
                default:
                    return new Vector3(0f, 0f, -13f);
            }
        }

        private string GetCurrentObjectiveBanner()
        {
            if (mysteryManager == null)
            {
                return "ACTIVE DIRECTIVE: Explore Floor 11 and talk to Arjun Mehta (Guide)";
            }

            string activeId = GetActiveObjectiveId();
            string desc = mysteryManager.GetObjectiveText(activeId);
            string locHint = GetObjectiveLocationHint(activeId);
            return $"ACTIVE DIRECTIVE // [{activeId}] {desc}  -->  [TARGET: {locHint.ToUpper()}]";
        }

        private string GetObjectiveLocationHint(string objId)
        {
            switch (objId)
            {
                case "OBJ_1":
                case "OBJ_2":
                    return "Research Laboratory (Room 1112 - East Wing)";
                case "OBJ_3":
                    return "Foyer / Reception Area (Question Suspects)";
                case "OBJ_4":
                    return "Security & Server Control (ISFCR Room 1110)";
                case "OBJ_5":
                    return "Professor's Office (Suite 1102 - West Wing)";
                case "OBJ_6":
                    return "IoT Systems Centre (Room 1108 - East Wing)";
                case "OBJ_7":
                case "OBJ_8":
                    return "Foyer / Suspect Assembly (Examine Motive & Contradictions)";
                case "OBJ_9":
                    return "Foyer Assembly (Deliver Formal Accusation)";
                default:
                    return "Floor 11";
            }
        }
    }
}

