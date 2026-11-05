using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using MurderMystery.Audio;

namespace MurderMystery.UI
{
    /// <summary>
    /// Person 4 Main Menu & Pause Menu System:
    /// Delivers an atmospheric, cohesive title screen and in-game pause menu:
    /// - Main Menu: "THE LAST SIGNAL" title, Start Investigation, Controls, Quit
    /// - Pause Menu: Resume, Restart Investigation, Controls, Return to Main Menu
    /// - Controls Modal: Clear keybinding layout
    /// </summary>
    public class GameMenuUI : MonoBehaviour
    {
        private static GameMenuUI instance;
        public static GameMenuUI Instance => instance;

        public enum MenuState
        {
            None,
            MainMenu,
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
        private readonly Color colorBackdropDim = new Color(0.04f, 0.05f, 0.08f, 0.94f);
        private readonly Color colorCardBg = new Color(0.07f, 0.09f, 0.12f, 0.98f);       // #12161E
        private readonly Color colorCardBorder = new Color(0.18f, 0.23f, 0.31f, 1.0f);     // #2E3B4F
        private readonly Color colorAmber = new Color(0.96f, 0.67f, 0.18f, 1.0f);          // #F5AB2E
        private readonly Color colorAmberHover = new Color(1.0f, 0.78f, 0.35f, 1.0f);
        private readonly Color colorRedMuted = new Color(0.72f, 0.20f, 0.20f, 1.0f);       // #B91C1C
        private readonly Color colorTextOffWhite = new Color(0.95f, 0.96f, 0.98f, 1.0f);
        private readonly Color colorTextMuted = new Color(0.65f, 0.72f, 0.82f, 1.0f);

        private Texture2D dimOverlayTex;
        private Texture2D cardBgTex;
        private Texture2D borderTex;
        private Texture2D buttonBgTex;
        private Texture2D amberTex;

        private GUIStyle titleMainStyle;
        private GUIStyle titleSubStyle;
        private GUIStyle buttonPrimaryStyle;
        private GUIStyle buttonSecondaryStyle;
        private GUIStyle bodyTextStyle;
        private GUIStyle headerStyle;
        private GUIStyle keyBadgeStyle;
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
            playerController = FindFirstObjectByType<FirstPersonController>();
            evidenceJournal = FindFirstObjectByType<EvidenceJournalUI>();

            if (showMainMenuOnStart)
            {
                OpenMainMenu();
            }
        }

        private void OnDestroy()
        {
            DestroyTex(ref dimOverlayTex);
            DestroyTex(ref cardBgTex);
            DestroyTex(ref borderTex);
            DestroyTex(ref buttonBgTex);
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
            if (currentState == MenuState.MainMenu)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Only toggle pause if journal or dialogue isn't consuming Escape
                if (currentState == MenuState.ControlsModal)
                {
                    currentState = returnStateFromControls;
                    AudioManager.Instance?.PlayButtonClick();
                }
                else if (currentState == MenuState.PauseMenu)
                {
                    ResumeGame();
                }
                else if (currentState == MenuState.None)
                {
                    // Check if journal or mystery dialogue is open
                    bool journalOpen = evidenceJournal != null && evidenceJournal.IsOpen;
                    bool mysteryModalOpen = false;
                    MysteryUI mysteryUI = FindFirstObjectByType<MysteryUI>();
                    // If no other modal is open, open pause menu
                    if (!journalOpen && !mysteryModalOpen)
                    {
                        OpenPauseMenu();
                    }
                }
            }
        }

        public void OpenMainMenu()
        {
            currentState = MenuState.MainMenu;
            SetPlayerLock(false);
        }

        public void OpenPauseMenu()
        {
            currentState = MenuState.PauseMenu;
            SetPlayerLock(false);
            AudioManager.Instance?.PlayJournalOpen();
        }

        public void ResumeGame()
        {
            currentState = MenuState.None;
            SetPlayerLock(true);
            AudioManager.Instance?.PlayJournalClose();
        }

        private void SetPlayerLock(bool locked)
        {
            if (locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (playerController != null) playerController.enabled = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (playerController != null) playerController.enabled = false;
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

            dimOverlayTex = MakeSolid(2, 2, colorBackdropDim);
            cardBgTex = MakeSolid(2, 2, colorCardBg);
            borderTex = MakeSolid(2, 2, colorCardBorder);
            buttonBgTex = MakeSolid(2, 2, new Color(0.12f, 0.16f, 0.22f, 1.0f));
            amberTex = MakeSolid(2, 2, colorAmber);

            titleMainStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
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
                fontSize = 12,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = colorTextOffWhite }
            };

            buttonPrimaryStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = colorAmber }
            };

            buttonSecondaryStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = colorTextOffWhite }
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

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimOverlayTex);

            switch (currentState)
            {
                case MenuState.MainMenu:
                    DrawMainMenu();
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
            float width = 480f;
            float height = 440f;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;

            Rect cardRect = new Rect(x, y, width, height);
            DrawBorderedBox(cardRect, cardBgTex, borderTex);

            // Red accent line at top
            GUI.DrawTexture(new Rect(x + 1f, y + 1f, width - 2f, 3f), MakeSolid(2, 2, colorRedMuted));

            // Title
            GUI.Label(new Rect(x, y + 28f, width, 38f), "THE LAST SIGNAL", titleMainStyle);
            GUI.Label(new Rect(x, y + 68f, width, 20f), "// UNIVERSITY FLOOR 11 INCIDENT //", titleSubStyle);

            // Synopsis
            Rect synRect = new Rect(x + 36f, y + 104f, width - 72f, 75f);
            string synopsis = "23:52. An alarm echoes through the 11th floor research facility. Senior researcher Dr. Ananya Mitra is dead. Four colleagues were inside. Review forensic evidence, interrogate suspects, and determine the truth.";
            GUI.Label(synRect, synopsis, bodyTextStyle);

            // Buttons
            float btnY = y + 200f;
            float btnW = width - 72f;
            float btnH = 42f;
            float btnX = x + 36f;

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "START INVESTIGATION", buttonPrimaryStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                ResumeGame();
            }

            if (GUI.Button(new Rect(btnX, btnY + 54f, btnW, btnH), "CONTROLS & DIRECTIVES", buttonSecondaryStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                returnStateFromControls = MenuState.MainMenu;
                currentState = MenuState.ControlsModal;
            }

            if (GUI.Button(new Rect(btnX, btnY + 108f, btnW, btnH), "EXIT GAME", buttonSecondaryStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        private void DrawPauseMenu()
        {
            float width = 420f;
            float height = 390f;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;

            Rect cardRect = new Rect(x, y, width, height);
            DrawBorderedBox(cardRect, cardBgTex, borderTex);

            GUI.DrawTexture(new Rect(x + 1f, y + 1f, width - 2f, 3f), MakeSolid(2, 2, colorAmber));

            GUI.Label(new Rect(x, y + 24f, width, 32f), "CASE SUSPENDED", titleMainStyle);
            GUI.Label(new Rect(x, y + 58f, width, 20f), "// INVESTIGATION PAUSED //", titleSubStyle);

            float btnY = y + 100f;
            float btnW = width - 64f;
            float btnH = 40f;
            float btnX = x + 32f;

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "RESUME INVESTIGATION", buttonPrimaryStyle))
            {
                ResumeGame();
            }

            if (GUI.Button(new Rect(btnX, btnY + 52f, btnW, btnH), "CONTROLS & DIRECTIVES", buttonSecondaryStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                returnStateFromControls = MenuState.PauseMenu;
                currentState = MenuState.ControlsModal;
            }

            if (GUI.Button(new Rect(btnX, btnY + 104f, btnW, btnH), "RESTART INVESTIGATION", buttonSecondaryStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (GUI.Button(new Rect(btnX, btnY + 156f, btnW, btnH), "MAIN MENU", buttonSecondaryStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                OpenMainMenu();
            }
        }

        private void DrawControlsModal()
        {
            float width = 560f;
            float height = 460f;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;

            Rect cardRect = new Rect(x, y, width, height);
            DrawBorderedBox(cardRect, cardBgTex, borderTex);

            GUI.Label(new Rect(x + 28f, y + 20f, width - 56f, 26f), "INVESTIGATION CONTROLS & DIRECTIVES", headerStyle);
            GUI.DrawTexture(new Rect(x + 28f, y + 48f, width - 56f, 1f), borderTex);

            string[] keybindings = {
                "W, A, S, D", "Locomotion / Movement across Floor 11",
                "MOUSE", "First-person camera observation",
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

                Rect keyBox = new Rect(x + 28f, rowY, 120f, 28f);
                DrawBorderedBox(keyBox, buttonBgTex, borderTex);
                GUI.Label(keyBox, key, keyBadgeStyle);

                Rect descRect = new Rect(x + 160f, rowY, width - 188f, 28f);
                GUI.Label(descRect, desc, bodyTextStyle);

                rowY += 36f;
            }

            // Tip note
            Rect tipRect = new Rect(x + 28f, rowY + 10f, width - 56f, 54f);
            string tip = "Note: Clues emit a subtle warm amber glow. Interview all 4 suspects and gather decisive evidence before presenting a formal accusation in the Foyer or Meeting Room.";
            GUI.Label(tipRect, tip, bodyTextStyle);

            // Back button
            Rect backBtnRect = new Rect(x + width - 160f, y + height - 52f, 132f, 34f);
            if (GUI.Button(backBtnRect, "BACK", buttonPrimaryStyle))
            {
                AudioManager.Instance?.PlayButtonClick();
                currentState = returnStateFromControls;
            }
        }
    }
}
