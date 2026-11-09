using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using MurderMystery.Audio;

namespace MurderMystery.UI
{
    /// <summary>
    /// Person 4 Main Menu, Tutorial, and Pause Menu System:
    /// Delivers an atmospheric, highly responsive, and reliable UI:
    /// - Main Menu: "THE LAST SIGNAL" title screen, Start Investigation, Controls, Quit
    /// - How-To-Play Tutorial: Displayed upon starting investigation with clear keybindings
    /// - Pause Menu: Toggled with [ESC] during gameplay (Resume, Controls, Restart, Main Menu)
    /// - Continuous cursor state management: guarantees unlocked visible cursor in menus,
    ///   and locked hidden cursor during gameplay.
    /// </summary>
    public class GameMenuUI : MonoBehaviour
    {
        private static GameMenuUI instance;
        public static GameMenuUI Instance => instance;

        public enum MenuState
        {
            None,
            MainMenu,
            TutorialModal,
            PauseMenu,
            ControlsModal
        }

        [Header("Menu State")]
        [SerializeField] private bool showMainMenuOnStart = true;

        private MenuState currentState = MenuState.None;
        private MenuState returnStateFromControls = MenuState.None;

        private FirstPersonController playerController;
        private EvidenceJournalUI evidenceJournal;

        // Visual Palette (Cinematic Detective)
        private readonly Color colorBackdropDim = new Color(0.05f, 0.07f, 0.10f, 0.94f);
        private readonly Color colorCardBg = new Color(0.08f, 0.10f, 0.14f, 0.98f);       // #141A24
        private readonly Color colorInnerPanel = new Color(0.12f, 0.15f, 0.20f, 1.0f);     // #1F2633
        private readonly Color colorCardBorder = new Color(0.22f, 0.28f, 0.38f, 1.0f);     // #384761
        private readonly Color colorAmber = new Color(0.96f, 0.67f, 0.18f, 1.0f);          // #F5AB2E
        private readonly Color colorAmberHover = new Color(1.0f, 0.82f, 0.42f, 1.0f);
        private readonly Color colorRedMuted = new Color(0.78f, 0.22f, 0.22f, 1.0f);       // #C73838
        private readonly Color colorCyanGuide = new Color(0.22f, 0.74f, 0.97f, 1.0f);      // #38BDF8
        private readonly Color colorTextOffWhite = new Color(0.95f, 0.96f, 0.98f, 1.0f);
        private readonly Color colorTextMuted = new Color(0.68f, 0.75f, 0.85f, 1.0f);

        private Texture2D dimOverlayTex;
        private Texture2D cardBgTex;
        private Texture2D innerPanelTex;
        private Texture2D borderTex;
        private Texture2D buttonBgTex;
        private Texture2D buttonHoverTex;
        private Texture2D amberTex;

        private GUIStyle titleMainStyle;
        private GUIStyle titleSubStyle;
        private GUIStyle buttonPrimaryStyle;
        private GUIStyle buttonSecondaryStyle;
        private GUIStyle bodyTextStyle;
        private GUIStyle headerStyle;
        private GUIStyle keyBadgeStyle;
        private GUIStyle guideTipStyle;
        private bool stylesInitialized = false;

        public bool IsMenuOpen => currentState != MenuState.None;

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
            FindReferences();

            if (showMainMenuOnStart)
            {
                OpenMainMenu();
            }
            else
            {
                ResumeGame();
            }
        }

        private void FindReferences()
        {
            if (playerController == null)
                playerController = FindFirstObjectByType<FirstPersonController>();
            if (evidenceJournal == null)
                evidenceJournal = FindFirstObjectByType<EvidenceJournalUI>();
        }

        private void OnDestroy()
        {
            DestroyTex(ref dimOverlayTex);
            DestroyTex(ref cardBgTex);
            DestroyTex(ref innerPanelTex);
            DestroyTex(ref borderTex);
            DestroyTex(ref buttonBgTex);
            DestroyTex(ref buttonHoverTex);
            DestroyTex(ref amberTex);
        }

        private void DestroyTex(ref Texture2D tex)
        {
            if (tex != null)
            {
                Destroy(tex);
                tex = null;
            }
        }

        private void Update()
        {
            FindReferences();

            // CRITICAL CURSOR & CONTROLLER STATE ENFORCEMENT
            if (currentState != MenuState.None)
            {
                // Force cursor unlocked and visible whenever any menu/tutorial is active
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (playerController != null && playerController.enabled)
                {
                    playerController.enabled = false;
                }
            }

            // ESCAPE KEY HANDLING
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == MenuState.ControlsModal)
                {
                    currentState = returnStateFromControls;
                    AudioManager.Instance?.PlayButtonClick();
                }
                else if (currentState == MenuState.TutorialModal)
                {
                    DismissTutorialAndStart();
                }
                else if (currentState == MenuState.PauseMenu)
                {
                    ResumeGame();
                }
                else if (currentState == MenuState.None)
                {
                    // Check if journal or dialogue is active
                    bool journalOpen = evidenceJournal != null && evidenceJournal.IsOpen;
                    MysteryUI mysteryUI = FindFirstObjectByType<MysteryUI>();

                    if (!journalOpen)
                    {
                        OpenPauseMenu();
                    }
                }
            }
        }

        public void OpenMainMenu()
        {
            currentState = MenuState.MainMenu;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (playerController != null) playerController.enabled = false;
        }

        public void OpenPauseMenu()
        {
            currentState = MenuState.PauseMenu;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (playerController != null) playerController.enabled = false;
            AudioManager.Instance?.PlayJournalOpen();
        }

        public void ResumeGame()
        {
            currentState = MenuState.None;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (playerController != null) playerController.enabled = true;
            AudioManager.Instance?.PlayJournalClose();
        }

        private void DismissTutorialAndStart()
        {
            AudioManager.Instance?.PlayButtonClick();
            ResumeGame();
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

            dimOverlayTex = MakeSolid(2, 2, colorBackdropDim);
            cardBgTex = MakeSolid(2, 2, colorCardBg);
            innerPanelTex = MakeSolid(2, 2, colorInnerPanel);
            borderTex = MakeSolid(2, 2, colorCardBorder);
            buttonBgTex = MakeSolid(2, 2, new Color(0.14f, 0.18f, 0.24f, 1.0f));
            buttonHoverTex = MakeSolid(2, 2, new Color(0.20f, 0.27f, 0.38f, 1.0f));
            amberTex = MakeSolid(2, 2, colorAmber);

            titleMainStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = colorTextOffWhite }
            };

            titleSubStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = colorAmber }
            };

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = colorAmber }
            };

            bodyTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = colorTextOffWhite }
            };

            guideTipStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = colorCyanGuide }
            };

            buttonPrimaryStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(16, 16, 10, 10),
                normal = { background = buttonBgTex, textColor = colorAmber },
                hover = { background = buttonHoverTex, textColor = colorAmberHover },
                active = { background = buttonHoverTex, textColor = Color.white }
            };

            buttonSecondaryStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(14, 14, 8, 8),
                normal = { background = buttonBgTex, textColor = colorTextOffWhite },
                hover = { background = buttonHoverTex, textColor = colorAmberHover },
                active = { background = buttonHoverTex, textColor = Color.white }
            };

            keyBadgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = colorAmber }
            };

            stylesInitialized = true;
        }

        private void DrawBorderedBox(Rect r, Texture2D bg, Texture2D border)
        {
            GUI.DrawTexture(r, border);
            GUI.DrawTexture(new Rect(r.x + 1f, r.y + 1f, r.width - 2f, r.height - 2f), bg);
        }

        private void OnGUI()
        {
            if (currentState == MenuState.None)
                return;

            InitStyles();

            // Bring menu to top depth so it captures all clicks reliably
            GUI.depth = -100;

            // Fullscreen backdrop dim
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimOverlayTex);

            switch (currentState)
            {
                case MenuState.MainMenu:
                    DrawMainMenu();
                    break;
                case MenuState.TutorialModal:
                    DrawTutorialModal();
                    break;
                case MenuState.PauseMenu:
                    DrawPauseMenu();
                    break;
                case MenuState.ControlsModal:
                    DrawControlsModal();
                    break;
            }
        }

        private void DrawMainMenu()
        {
            float width = 520f;
            float height = 470f;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;

            Rect cardRect = new Rect(x, y, width, height);
            DrawBorderedBox(cardRect, cardBgTex, borderTex);

            // Muted red accent bar
            GUI.DrawTexture(new Rect(x + 1f, y + 1f, width - 2f, 4f), MakeSolid(2, 2, colorRedMuted));

            // Title Header
            GUI.Label(new Rect(x, y + 24f, width, 40f), "THE LAST SIGNAL", titleMainStyle);
            GUI.Label(new Rect(x, y + 68f, width, 20f), "// UNIVERSITY FLOOR 11 RESEARCH FACILITY //", titleSubStyle);

            // Synopsis Panel
            Rect synBoxRect = new Rect(x + 32f, y + 100f, width - 64f, 95f);
            DrawBorderedBox(synBoxRect, innerPanelTex, borderTex);

            Rect synTextRect = new Rect(synBoxRect.x + 14f, synBoxRect.y + 10f, synBoxRect.width - 28f, synBoxRect.height - 20f);
            string synopsis = "23:52. An emergency alarm sounds through the 11th floor research facility. Senior researcher Dr. Ananya Mitra is dead. Four colleagues remain inside. Collect forensic evidence, interview suspects, and uncover the truth.";
            GUI.Label(synTextRect, synopsis, bodyTextStyle);

            // Buttons
            float btnY = y + 215f;
            float btnW = width - 64f;
            float btnH = 46f;
            float btnX = x + 32f;

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "START INVESTIGATION", buttonPrimaryStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                currentState = MenuState.TutorialModal;
            }

            if (GUI.Button(new Rect(btnX, btnY + 58f, btnW, 40f), "CONTROLS & DIRECTIVES", buttonSecondaryStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                returnStateFromControls = MenuState.MainMenu;
                currentState = MenuState.ControlsModal;
            }

            if (GUI.Button(new Rect(btnX, btnY + 110f, btnW, 40f), "EXIT GAME", buttonSecondaryStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        private void DrawTutorialModal()
        {
            float width = 600f;
            float height = 500f;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;

            Rect cardRect = new Rect(x, y, width, height);
            DrawBorderedBox(cardRect, cardBgTex, borderTex);

            GUI.DrawTexture(new Rect(x + 1f, y + 1f, width - 2f, 4f), amberTex);

            GUI.Label(new Rect(x, y + 20f, width, 32f), "HOW TO PLAY", titleMainStyle);
            GUI.Label(new Rect(x, y + 54f, width, 20f), "// INVESTIGATOR ORIENTATION //", titleSubStyle);

            float rowY = y + 90f;
            float rowPadX = x + 36f;
            float rowW = width - 72f;

            string[] bindings = {
                "WASD", "Move your detective around Floor 11",
                "MOUSE", "Look around / Aim crosshair at objects and suspects",
                "SPACE", "Jump / Clear low obstacles",
                "E", "Interact with objects and people (Collect clue / Talk)",
                "J", "Open / Close Evidence Journal (Case Dossier)",
                "T", "Make Formal Accusation (when requirements are met)",
                "ESC", "Pause game / Review controls"
            };

            for (int i = 0; i < bindings.Length; i += 2)
            {
                Rect keyBox = new Rect(rowPadX, rowY, 110f, 26f);
                DrawBorderedBox(keyBox, innerPanelTex, borderTex);
                GUI.Label(keyBox, bindings[i], keyBadgeStyle);

                Rect textRect = new Rect(rowPadX + 125f, rowY + 3f, rowW - 125f, 24f);
                GUI.Label(textRect, bindings[i + 1], bodyTextStyle);

                rowY += 32f;
            }

            // Friendly Guide Tip Box
            float tipY = rowY + 10f;
            Rect tipBox = new Rect(rowPadX, tipY, rowW, 64f);
            DrawBorderedBox(tipBox, innerPanelTex, borderTex);

            Rect tipText = new Rect(tipBox.x + 12f, tipBox.y + 8f, tipBox.width - 24f, tipBox.height - 16f);
            string tip = "INVESTIGATION GUIDE: Arjun Mehta (Junior Research Assistant) is standing right in the Foyer entrance ahead. Approach him and press E to get briefed!";
            GUI.Label(tipText, tip, guideTipStyle);

            // Continue Button
            float btnW = 240f;
            float btnH = 44f;
            Rect btnRect = new Rect(x + (width - btnW) / 2f, y + height - 60f, btnW, btnH);
            if (GUI.Button(btnRect, "CONTINUE TO GAME", buttonPrimaryStyle))
            {
                DismissTutorialAndStart();
            }
        }

        private void DrawPauseMenu()
        {
            float width = 460f;
            float height = 410f;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;

            Rect cardRect = new Rect(x, y, width, height);
            DrawBorderedBox(cardRect, cardBgTex, borderTex);

            GUI.DrawTexture(new Rect(x + 1f, y + 1f, width - 2f, 4f), amberTex);

            GUI.Label(new Rect(x, y + 24f, width, 34f), "CASE SUSPENDED", titleMainStyle);
            GUI.Label(new Rect(x, y + 60f, width, 20f), "// INVESTIGATION PAUSED //", titleSubStyle);

            float btnY = y + 100f;
            float btnW = width - 64f;
            float btnH = 42f;
            float btnX = x + 32f;

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "RESUME INVESTIGATION", buttonPrimaryStyle))
            {
                ResumeGame();
            }

            if (GUI.Button(new Rect(btnX, btnY + 54f, btnW, btnH), "CONTROLS & DIRECTIVES", buttonSecondaryStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                returnStateFromControls = MenuState.PauseMenu;
                currentState = MenuState.ControlsModal;
            }

            if (GUI.Button(new Rect(btnX, btnY + 108f, btnW, btnH), "RESTART INVESTIGATION", buttonSecondaryStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (GUI.Button(new Rect(btnX, btnY + 162f, btnW, btnH), "MAIN MENU", buttonSecondaryStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                OpenMainMenu();
            }
        }

        private void DrawControlsModal()
        {
            float width = 580f;
            float height = 480f;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;

            Rect cardRect = new Rect(x, y, width, height);
            DrawBorderedBox(cardRect, cardBgTex, borderTex);

            GUI.Label(new Rect(x + 28f, y + 20f, width - 56f, 26f), "INVESTIGATION CONTROLS & DIRECTIVES", headerStyle);
            GUI.DrawTexture(new Rect(x + 28f, y + 48f, width - 56f, 1f), borderTex);

            string[] keybindings = {
                "WASD", "Move your detective around Floor 11",
                "MOUSE", "First-person camera observation",
                "SPACE", "Jump / Clear obstacles",
                "E", "Interact / Collect physical clue / Speak with suspect",
                "J", "Open / Close Case Dossier (Evidence Journal)",
                "T", "Open Formal Accusation (when prerequisites met)",
                "ESC", "Pause menu / Suspend investigation / Close modal"
            };

            float rowY = y + 62f;
            for (int i = 0; i < keybindings.Length; i += 2)
            {
                string key = keybindings[i];
                string desc = keybindings[i + 1];

                Rect keyBox = new Rect(x + 28f, rowY, 110f, 26f);
                DrawBorderedBox(keyBox, innerPanelTex, borderTex);
                GUI.Label(keyBox, key, keyBadgeStyle);

                Rect descRect = new Rect(x + 150f, rowY + 3f, width - 178f, 26f);
                GUI.Label(descRect, desc, bodyTextStyle);

                rowY += 34f;
            }

            Rect tipRect = new Rect(x + 28f, rowY + 8f, width - 56f, 54f);
            string tip = "Note: Clues emit a subtle warm amber glow. Interview all 4 suspects and gather decisive evidence before presenting a formal accusation in the Foyer or Meeting Room.";
            GUI.Label(tipRect, tip, bodyTextStyle);

            Rect backBtnRect = new Rect(x + width - 150f, y + height - 52f, 120f, 36f);
            if (GUI.Button(backBtnRect, "BACK", buttonPrimaryStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                currentState = returnStateFromControls;
            }
        }
    }
}
