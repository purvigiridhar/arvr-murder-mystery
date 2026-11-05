using System;
using System.Collections.Generic;
using UnityEngine;
using MurderMystery.Audio;

namespace MurderMystery.UI
{
    /// <summary>
    /// Person 4 Evidence & HUD Notification System:
    /// Displays non-intrusive, cinematic notification banners when evidence is collected
    /// or major objectives update. Prevents duplicate notifications for the same clue.
    /// </summary>
    public class GameNotificationUI : MonoBehaviour
    {
        private static GameNotificationUI instance;

        public static GameNotificationUI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<GameNotificationUI>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("GameNotificationUI");
                        instance = go.AddComponent<GameNotificationUI>();
                    }
                }
                return instance;
            }
            internal set => instance = value;
        }

        private struct NotificationItem
        {
            public string Badge;
            public string Title;
            public string Subtitle;
            public float RemainingTime;
            public Color AccentColor;
        }

        private readonly Queue<NotificationItem> notificationQueue = new Queue<NotificationItem>();
        private NotificationItem currentNotification;
        private bool hasActiveNotification = false;
        private float currentAlpha = 0f;
        private readonly HashSet<string> notifiedEvidenceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Styling
        private readonly Color colorPanelBg = new Color(0.07f, 0.09f, 0.12f, 0.95f);     // #12161E
        private readonly Color colorBorder = new Color(0.18f, 0.23f, 0.31f, 1.0f);        // #2E3B4F
        private readonly Color colorAmber = new Color(0.96f, 0.67f, 0.18f, 1.0f);         // #F5AB2E
        private readonly Color colorTextOffWhite = new Color(0.95f, 0.96f, 0.98f, 1.0f);  // #F1F5F9
        private readonly Color colorTextMuted = new Color(0.65f, 0.72f, 0.82f, 1.0f);     // #A6B8D1

        private Texture2D panelBgTex;
        private Texture2D borderTex;
        private Texture2D accentTex;

        private GUIStyle badgeStyle;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private bool stylesInitialized = false;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void Start()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnEvidenceCollected += HandleEvidenceCollected;
            }
        }

        private void OnDestroy()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnEvidenceCollected -= HandleEvidenceCollected;
            }

            DestroyTex(ref panelBgTex);
            DestroyTex(ref borderTex);
            DestroyTex(ref accentTex);
        }

        private void DestroyTex(ref Texture2D tex)
        {
            if (tex != null)
            {
                Destroy(tex);
                tex = null;
            }
        }

        private void HandleEvidenceCollected(EvidenceItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.EvidenceId))
                return;

            // Avoid repeatedly displaying notifications for duplicate evidence
            if (notifiedEvidenceIds.Contains(item.EvidenceId))
                return;

            notifiedEvidenceIds.Add(item.EvidenceId);

            ShowNotification(
                "EVIDENCE RECOVERED",
                item.DisplayName,
                $"[{item.EvidenceId}]  Added to Case Dossier [J]",
                colorAmber);
        }

        public void ShowNotification(string badge, string title, string subtitle, Color accentColor, float duration = 3.6f)
        {
            var item = new NotificationItem
            {
                Badge = badge,
                Title = title,
                Subtitle = subtitle,
                RemainingTime = duration,
                AccentColor = accentColor
            };

            if (!hasActiveNotification)
            {
                currentNotification = item;
                hasActiveNotification = true;
            }
            else
            {
                notificationQueue.Enqueue(item);
            }
        }

        private void Update()
        {
            if (hasActiveNotification)
            {
                currentNotification.RemainingTime -= Time.deltaTime;
                currentAlpha = Mathf.MoveTowards(currentAlpha, 1.0f, Time.deltaTime * 6f);

                if (currentNotification.RemainingTime <= 0f)
                {
                    currentAlpha = Mathf.MoveTowards(currentAlpha, 0f, Time.deltaTime * 5f);
                    if (currentAlpha <= 0.01f)
                    {
                        if (notificationQueue.Count > 0)
                        {
                            currentNotification = notificationQueue.Dequeue();
                            currentAlpha = 0f;
                        }
                        else
                        {
                            hasActiveNotification = false;
                        }
                    }
                }
            }
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

            panelBgTex = MakeSolid(2, 2, colorPanelBg);
            borderTex = MakeSolid(2, 2, colorBorder);
            accentTex = MakeSolid(2, 2, colorAmber);

            badgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorAmber }
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorTextOffWhite }
            };

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorTextMuted }
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!hasActiveNotification || currentAlpha <= 0.01f)
                return;

            InitStyles();

            Color origColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, currentAlpha);

            float width = 420f;
            float height = 62f;
            float posX = (Screen.width - width) / 2f;
            float posY = 24f;

            Rect outerRect = new Rect(posX, posY, width, height);
            Rect innerRect = new Rect(posX + 1f, posY + 1f, width - 2f, height - 2f);

            GUI.DrawTexture(outerRect, borderTex);
            GUI.DrawTexture(innerRect, panelBgTex);

            // Left vertical accent bar
            Rect accentBar = new Rect(posX + 1f, posY + 1f, 4f, height - 2f);
            GUI.DrawTexture(accentBar, accentTex);

            // Badge text
            Rect badgeRect = new Rect(posX + 16f, posY + 8f, width - 32f, 16f);
            GUI.Label(badgeRect, $"// {currentNotification.Badge}", badgeStyle);

            // Title
            Rect titleRect = new Rect(posX + 16f, posY + 24f, width - 32f, 20f);
            GUI.Label(titleRect, currentNotification.Title, titleStyle);

            // Subtitle
            Rect subRect = new Rect(posX + 16f, posY + 42f, width - 32f, 16f);
            GUI.Label(subRect, currentNotification.Subtitle, subtitleStyle);

            GUI.color = origColor;
        }
    }
}
