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

    [Header("Tutorial Content (OnGUI)")]
    [SerializeField] private string tutorialPart1 = "Movement uses WASD and arrow keys and use space bar to jump";
    [SerializeField] private string tutorialPart2 = "Collect resources to ugrade your broken ship. Once you have collected enough resources you can make your way to the spaceship, press the R key to repair the ship.";
    [SerializeField] private string tutorialPart3 = "Avoid falling stars and collect the fallen stars on the ground. To transform into the cat you need 5 fallen stars and then you can press C to transform. The cat gives you the ability to fit into smaller aresa and jump higher.";

    [SerializeField] private int tutorialPartsCount = 3;
    [SerializeField] private int tutorialTextFontSize = 18;
    [SerializeField] private int tutorialTitleFontSize = 26;
    [SerializeField] private int tutorialPadding = 12;

    [Header("OnGUI")]
    [SerializeField] private Vector2 menuPosition = new Vector2(0f, 0f);
    [SerializeField] private int fontSize = 24;
    [SerializeField] private int buttonFontSize = 18;
    [SerializeField] private int panelWidth = 420;
    [SerializeField] private int panelHeight = 260;

    private bool _isTutorialOpen;
    private int _tutorialStepIndex;
    private GUIStyle _titleStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _tutorialTitleStyle;
    private GUIStyle _tutorialTextStyle;

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

        _tutorialTitleStyle = new GUIStyle(GUI.skin.label);
        _tutorialTitleStyle.fontSize = tutorialTitleFontSize;
        _tutorialTitleStyle.fontStyle = FontStyle.Bold;
        _tutorialTitleStyle.alignment = TextAnchor.UpperCenter;
        _tutorialTitleStyle.normal.textColor = Color.white;

        _tutorialTextStyle = new GUIStyle(GUI.skin.label);
        _tutorialTextStyle.fontSize = tutorialTextFontSize;
        _tutorialTextStyle.alignment = TextAnchor.UpperLeft;
        _tutorialTextStyle.normal.textColor = Color.white;
        _tutorialTextStyle.wordWrap = true;
    }

    private void OnGUI()
    {
        EnsureStyles();

        float x = (Screen.width - panelWidth) * 0.5f + menuPosition.x;
        float y = (Screen.height - panelHeight) * 0.5f + menuPosition.y;
        Rect panelRect = new Rect(x, y, panelWidth, panelHeight);

        if (_isTutorialOpen)
        {
            string currentText = _tutorialStepIndex switch
            {
                0 => tutorialPart1,
                1 => tutorialPart2,
                2 => tutorialPart3,
                _ => tutorialPart1
            };

            string title = $"Tutorial ({_tutorialStepIndex + 1}/{tutorialPartsCount})";
            GUI.Box(panelRect, string.Empty);

            GUI.Label(
                new Rect(panelRect.x + tutorialPadding, panelRect.y + tutorialPadding, panelRect.width - tutorialPadding * 2f, 36f),
                title,
                _tutorialTitleStyle
            );

            float textY = panelRect.y + tutorialPadding + 46f;
            float textHeight = panelRect.height - (tutorialPadding * 2f + 96f);
            GUI.Label(
                new Rect(panelRect.x + tutorialPadding, textY, panelRect.width - tutorialPadding * 2f, textHeight),
                currentText,
                _tutorialTextStyle
            );

            string nextLabel = (_tutorialStepIndex >= 2) ? "Done" : "Next";
            if (GUI.Button(
                    new Rect(panelRect.x + panelWidth * 0.5f - 100f, panelRect.y + panelHeight - 60f, 200f, 40f),
                    nextLabel,
                    _buttonStyle))
            {
                if (_tutorialStepIndex < 2)
                    _tutorialStepIndex++;
                else
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
        _tutorialStepIndex = 0;
        // Use this OnGUI tutorial overlay instead of the optional Canvas panel.
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
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

