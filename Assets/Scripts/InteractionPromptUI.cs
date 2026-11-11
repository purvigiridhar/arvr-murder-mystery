using System;
using UnityEngine;

/// <summary>
/// Person 4 Redesigned Interaction Prompt UI:
/// Cinematic detective HUD prompt featuring smooth alpha fade transitions,
/// dedicated keybinding pill badge with warm amber highlight, dark navy container,
/// and readable off-white typography.
/// Fully preserves Person 2A's Show(string) and Hide() API.
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float fadeSpeed = 8.0f;

    [Header("Color Palette (Cinematic Detective Style)")]
    private readonly Color backgroundDarkNavy = new Color(0.07f, 0.09f, 0.12f, 0.92f); // #12161E
    private readonly Color borderSubtle = new Color(0.18f, 0.23f, 0.30f, 0.95f);       // #2E3B4D
    private readonly Color accentAmber = new Color(0.95f, 0.67f, 0.18f, 1.0f);          // #F5AB2E
    private readonly Color keyBadgeBg = new Color(0.14f, 0.18f, 0.25f, 1.0f);           // #242E40
    private readonly Color textOffWhite = new Color(0.95f, 0.96f, 0.98f, 1.0f);         // #F1F5F9
    private readonly Color textMuted = new Color(0.68f, 0.74f, 0.82f, 1.0f);            // #AEC0D1

    private string promptMessage = "";
    private bool isVisible = false;
    private float currentAlpha = 0f;

    // Procedural textures
    private Texture2D panelBgTex;
    private Texture2D borderTex;
    private Texture2D keyBadgeTex;

    // GUI styles
    private GUIStyle keyBadgeStyle;
    private GUIStyle actionTextStyle;
    private GUIStyle bracketStyle;
    private bool stylesInitialized = false;

    public void Show(string message)
    {
        promptMessage = message;
        isVisible = true;
    }

    public void Hide()
    {
        isVisible = false;
    }

    private void Update()
    {
        float targetAlpha = isVisible ? 1.0f : 0.0f;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
    }

    private void OnDestroy()
    {
        DestroyTex(ref panelBgTex);
        DestroyTex(ref borderTex);
        DestroyTex(ref keyBadgeTex);
    }

    private void DestroyTex(ref Texture2D tex)
    {
        if (tex != null)
        {
            Destroy(tex);
            tex = null;
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

        panelBgTex = MakeSolid(2, 2, backgroundDarkNavy);
        borderTex = MakeSolid(2, 2, borderSubtle);
        keyBadgeTex = MakeSolid(2, 2, keyBadgeBg);

        keyBadgeStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = accentAmber }
        };

        actionTextStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = textOffWhite }
        };

        bracketStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = textMuted }
        };

        stylesInitialized = true;
    }

    private void OnGUI()
    {
        if (currentAlpha <= 0.01f || string.IsNullOrEmpty(promptMessage))
            return;

        InitStyles();

        // Save original GUI color and apply smooth fade alpha
        Color origColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, currentAlpha);

        // Parse key prompt vs object text (e.g. "Press E to collect Security Keycard")
        string keyLabel = "E";
        string actionLabel = promptMessage;

        if (promptMessage.StartsWith("Press [", StringComparison.OrdinalIgnoreCase))
        {
            int closeBracket = promptMessage.IndexOf(']');
            if (closeBracket > 7)
            {
                keyLabel = promptMessage.Substring(7, closeBracket - 7);
                actionLabel = promptMessage.Substring(closeBracket + 1).Trim();
            }
        }
        else if (promptMessage.StartsWith("Press ", StringComparison.OrdinalIgnoreCase))
        {
            string[] parts = promptMessage.Split(new[] { ' ' }, 3);
            if (parts.Length >= 3)
            {
                keyLabel = parts[1];
                actionLabel = parts[2];
            }
        }

        // Layout measurements
        float keyBadgeWidth = Mathf.Max(32f, keyBadgeStyle.CalcSize(new GUIContent(keyLabel)).x + 14f);
        float actionWidth = actionTextStyle.CalcSize(new GUIContent(actionLabel)).x + 10f;
        float totalWidth = keyBadgeWidth + actionWidth + 36f;
        float totalHeight = 40f;

        float posX = (Screen.width - totalWidth) / 2f;
        float posY = Screen.height * 0.72f;

        Rect outerRect = new Rect(posX, posY, totalWidth, totalHeight);
        Rect innerRect = new Rect(posX + 1f, posY + 1f, totalWidth - 2f, totalHeight - 2f);

        // Draw Outer Border & Background
        GUI.DrawTexture(outerRect, borderTex);
        GUI.DrawTexture(innerRect, panelBgTex);

        // Draw Key Badge
        float badgeHeight = 24f;
        float badgeY = posY + (totalHeight - badgeHeight) / 2f;
        Rect badgeOuterRect = new Rect(posX + 10f, badgeY, keyBadgeWidth, badgeHeight);
        Rect badgeInnerRect = new Rect(badgeOuterRect.x + 1f, badgeOuterRect.y + 1f, badgeOuterRect.width - 2f, badgeOuterRect.height - 2f);

        GUI.DrawTexture(badgeOuterRect, borderTex);
        GUI.DrawTexture(badgeInnerRect, keyBadgeTex);
        GUI.Label(badgeInnerRect, keyLabel, keyBadgeStyle);

        // Draw Action Label
        Rect textRect = new Rect(posX + 18f + keyBadgeWidth, posY, actionWidth + 10f, totalHeight);
        GUI.Label(textRect, actionLabel, actionTextStyle);

        // Restore original GUI color
        GUI.color = origColor;
    }
}
