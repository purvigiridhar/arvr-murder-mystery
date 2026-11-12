
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    private string promptMessage = "";
    private bool isVisible = false;

    public void Show(string message)
    {
        promptMessage = message;
        isVisible = true;
    }

    public void Hide()
    {
        isVisible = false;
    }

    private void OnGUI()
    {
        if (!isVisible || string.IsNullOrEmpty(promptMessage))
            return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 20;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;

        float width = 300f;
        float height = 45f;
        float x = (Screen.width - width) / 2f;
        float y = Screen.height * 0.75f;

        GUI.Box(new Rect(x, y, width, height), promptMessage, style);
    }
}
