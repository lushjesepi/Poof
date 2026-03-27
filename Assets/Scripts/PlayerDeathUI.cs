using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows a "You Died" overlay when PlayerController.IsDead is true.
/// Uses OnGUI so no Canvas setup is required.
/// </summary>
public class PlayerDeathUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Layout")]
    [SerializeField] private int panelWidth = 460;
    [SerializeField] private int panelHeight = 250;
    [SerializeField] private int titleFontSize = 32;
    [SerializeField] private int buttonFontSize = 18;

    [Header("Colors")]
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.8f);
    [SerializeField] private Color titleColor = Color.white;

    private GUIStyle _titleStyle;
    private GUIStyle _buttonStyle;

    private void Awake()
    {
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
    }

    private void OnGUI()
    {
        if (playerController == null || !playerController.IsDead)
            return;

        EnsureStyles();

        float x = (Screen.width - panelWidth) * 0.5f;
        float y = (Screen.height - panelHeight) * 0.5f;
        Rect panelRect = new Rect(x, y, panelWidth, panelHeight);

        Color oldColor = GUI.color;
        GUI.color = panelColor;
        GUI.Box(panelRect, string.Empty);
        GUI.color = oldColor;

        GUI.Label(new Rect(panelRect.x, panelRect.y + 12f, panelRect.width, 50f), "You Died", _titleStyle);

        float buttonWidth = panelWidth * 0.75f;
        float buttonHeight = 44f;
        float buttonX = panelRect.x + (panelRect.width - buttonWidth) * 0.5f;
        float firstButtonY = panelRect.y + 74f;
        float spacing = 52f;

        if (GUI.Button(new Rect(buttonX, firstButtonY, buttonWidth, buttonHeight), "Restart", _buttonStyle))
            RestartGame();

        if (GUI.Button(new Rect(buttonX, firstButtonY + spacing, buttonWidth, buttonHeight), "Main Menu", _buttonStyle))
            BackToMainMenu();

        if (GUI.Button(new Rect(buttonX, firstButtonY + spacing * 2f, buttonWidth, buttonHeight), "Quit", _buttonStyle))
            QuitGame();
    }

    private void EnsureStyles()
    {
        if (_titleStyle != null)
            return;

        _titleStyle = new GUIStyle
        {
            fontSize = titleFontSize,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _titleStyle.normal.textColor = titleColor;

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = buttonFontSize
        };
    }

    private static void RestartGame()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    private void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
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
