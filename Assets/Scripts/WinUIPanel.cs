using UnityEngine;

/// <summary>
/// Simple win overlay shown when GameManager.IsWin is true.
/// Uses OnGUI so no Canvas is required.
/// </summary>
public class WinUIPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("UI")]
    [SerializeField] private string title = "You Win!";
    [SerializeField] private string subtitle = "Ship fully repaired.";
    [SerializeField] private int panelWidth = 420;
    [SerializeField] private int panelHeight = 200;
    [SerializeField] private int titleFontSize = 28;
    [SerializeField] private int subtitleFontSize = 16;
    [SerializeField] private int buttonFontSize = 16;

    [Header("Colors")]
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.75f);
    [SerializeField] private Color textColor = Color.white;

    [Header("Optional Buttons")]
    [SerializeField] private bool showQuitButton = false;
    [SerializeField] private string quitButtonText = "Quit";

    private GUIStyle _titleStyle;
    private GUIStyle _subtitleStyle;
    private GUIStyle _buttonStyle;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = GameManager.Instance != null ? GameManager.Instance : FindObjectOfType<GameManager>();
    }

    private void OnGUI()
    {
        if (gameManager == null || !gameManager.IsWin)
            return;

        EnsureStyles();

        float x = (Screen.width - panelWidth) * 0.5f;
        float y = (Screen.height - panelHeight) * 0.5f;
        Rect panelRect = new Rect(x, y, panelWidth, panelHeight);

        Color oldGuiColor = GUI.color;
        GUI.color = panelColor;
        GUI.Box(panelRect, string.Empty);
        GUI.color = oldGuiColor;

        float padding = 18f;
        float cursorY = panelRect.y + padding;

        GUI.Label(
            new Rect(panelRect.x + padding, cursorY, panelRect.width - padding * 2f, 40f),
            title,
            _titleStyle
        );
        cursorY += 52f;

        GUI.Label(
            new Rect(panelRect.x + padding, cursorY, panelRect.width - padding * 2f, 40f),
            subtitle,
            _subtitleStyle
        );

        if (showQuitButton)
        {
            float buttonWidth = 160f;
            float buttonHeight = 36f;
            Rect buttonRect = new Rect(
                panelRect.x + (panelRect.width - buttonWidth) * 0.5f,
                panelRect.yMax - padding - buttonHeight,
                buttonWidth,
                buttonHeight
            );

            if (GUI.Button(buttonRect, quitButtonText, _buttonStyle))
                QuitGame();
        }
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        // In editor, Quit does nothing; stop play mode instead.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
        _titleStyle.normal.textColor = textColor;

        _subtitleStyle = new GUIStyle
        {
            fontSize = subtitleFontSize,
            alignment = TextAnchor.MiddleCenter
        };
        _subtitleStyle.normal.textColor = textColor;

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = buttonFontSize
        };
    }
}

