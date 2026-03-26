using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Pause menu controller for the Game scene.
/// Press Escape to toggle pause and show a simple OnGUI panel:
/// - Restart
/// - Resume
/// - Back to Main Menu
/// </summary>
[DisallowMultipleComponent]
public class GamePauseController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Pause Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private bool showOnGUI = true;

    [Header("Optional Canvas Panel (recommended if you use Unity UI)")]
    [Tooltip("If assigned, this GameObject is toggled on pause/resume. When assigned, OnGUI buttons are hidden by default.")]
    [SerializeField] private GameObject pausePanel;

    [Header("Optional Unity Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button backToMainMenuButton;

    [Header("OnGUI Layout")]
    [SerializeField] private int panelWidth = 420;
    [SerializeField] private int panelHeight = 240;
    [SerializeField] private int fontSize = 26;
    [SerializeField] private int buttonFontSize = 16;

    private bool _isPaused;
    private GUIStyle _titleStyle;
    private GUIStyle _buttonStyle;

    private void Awake()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Wire up Unity UI buttons if they are assigned.
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.AddListener(BackToMainMenu);
    }

    private void EnsureStyles()
    {
        if (_titleStyle != null)
            return;

        _titleStyle = new GUIStyle(GUI.skin.label);
        _titleStyle.fontSize = fontSize;
        _titleStyle.fontStyle = FontStyle.Bold;
        _titleStyle.alignment = TextAnchor.MiddleCenter;
        _titleStyle.normal.textColor = Color.white;

        _buttonStyle = new GUIStyle(GUI.skin.button);
        _buttonStyle.fontSize = buttonFontSize;
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (_isPaused)
                Resume();
            else
                Pause();
        }
    }

    private void Pause()
    {
        _isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    private void Resume()
    {
        ResumeGame();
    }

    private void OnGUI()
    {
        bool shouldShowOnGUI = showOnGUI && (pausePanel == null);
        if (!_isPaused || !shouldShowOnGUI)
            return;

        EnsureStyles();

        float x = (Screen.width - panelWidth) * 0.5f;
        float y = (Screen.height - panelHeight) * 0.5f;
        Rect panelRect = new Rect(x, y, panelWidth, panelHeight);

        GUI.Box(panelRect, string.Empty);
        GUI.Label(new Rect(panelRect.x, panelRect.y + 10, panelRect.width, 40), "Paused", _titleStyle);

        float btnY = panelRect.y + 70f;
        float btnH = 44f;
        float btnW = panelWidth * 0.8f;
        float btnX = panelRect.x + (panelRect.width - btnW) * 0.5f;

        if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "Restart", _buttonStyle))
            RestartGame();

        if (GUI.Button(new Rect(btnX, btnY + 55f, btnW, btnH), "Resume", _buttonStyle))
            ResumeGame();

        if (GUI.Button(new Rect(btnX, btnY + 110f, btnW, btnH), "Back to Main Menu", _buttonStyle))
            BackToMainMenu();
    }

    public void RestartGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void BackToMainMenu()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}

