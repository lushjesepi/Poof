using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple main menu controller.
/// Uses OnGUI buttons so it doesn't require prebuilt Canvas UI to be present.
/// Optionally toggles a tutorial panel (Canvas panel) if assigned.
/// </summary>
[DisallowMultipleComponent]
public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Tutorial")]
    [Tooltip("Optional Canvas panel to show when Tutorial is pressed.")]
    [SerializeField] private GameObject tutorialPanel;

    [Header("OnGUI")]
    [SerializeField] private Vector2 menuPosition = new Vector2(0f, 0f);
    [SerializeField] private int fontSize = 24;
    [SerializeField] private int buttonFontSize = 18;
    [SerializeField] private int panelWidth = 420;
    [SerializeField] private int panelHeight = 260;

    private bool _isTutorialOpen;
    private GUIStyle _titleStyle;
    private GUIStyle _buttonStyle;

    private void Awake()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    private void EnsureStyles()
    {
        if (_titleStyle != null)
            return;

        _titleStyle = new GUIStyle(GUI.skin.label);
        _titleStyle.fontSize = fontSize;
        _titleStyle.alignment = TextAnchor.MiddleCenter;
        _titleStyle.normal.textColor = Color.white;

        _buttonStyle = new GUIStyle(GUI.skin.button);
        _buttonStyle.fontSize = buttonFontSize;
    }

    private void OnGUI()
    {
        EnsureStyles();

        float x = (Screen.width - panelWidth) * 0.5f + menuPosition.x;
        float y = (Screen.height - panelHeight) * 0.5f + menuPosition.y;
        Rect panelRect = new Rect(x, y, panelWidth, panelHeight);

        if (_isTutorialOpen)
        {
            // Tutorial panel is handled by your Canvas panel (if assigned).
            GUI.Label(new Rect(panelRect.x, panelRect.y, panelRect.width, 40), "Tutorial", _titleStyle);

            if (tutorialPanel == null)
            {
                GUI.Label(new Rect(panelRect.x, panelRect.y + 50, panelRect.width, 120),
                    "Assign a Tutorial Canvas Panel (tutorialPanel) to show your content here.",
                    GUI.skin.label);
            }

            if (GUI.Button(new Rect(panelRect.x + panelWidth * 0.5f - 100f, panelRect.y + panelHeight - 60f, 200f, 40),
                    "Back", _buttonStyle))
            {
                CloseTutorial();
            }

            return;
        }

        // Title
        GUI.Box(panelRect, string.Empty);
        GUI.Label(new Rect(panelRect.x, panelRect.y, panelRect.width, 60), "Main Menu", _titleStyle);

        float btnY = panelRect.y + 80f;
        float btnH = 46f;
        float btnW = panelWidth * 0.75f;
        float btnX = panelRect.x + (panelRect.width - btnW) * 0.5f;

        if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "Start Game", _buttonStyle))
            StartGame();

        if (GUI.Button(new Rect(btnX, btnY + 58f, btnW, btnH), "Tutorial", _buttonStyle))
            OpenTutorial();

        if (GUI.Button(new Rect(btnX, btnY + 116f, btnW, btnH), "Quit", _buttonStyle))
            QuitGame();
    }

    private void StartGame()
    {
        Time.timeScale = 1f;
        _isTutorialOpen = false;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        SceneManager.LoadScene(gameSceneName);
    }

    private void OpenTutorial()
    {
        _isTutorialOpen = true;
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    private void CloseTutorial()
    {
        _isTutorialOpen = false;
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

