using UnityEngine;

/// <summary>
/// Top-right timer that displays remaining cat-form time.
/// Uses OnGUI so no Canvas setup is required.
/// </summary>
public class CatFormTimerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [Header("Layout")]
    [SerializeField] private float topPadding = 20f;
    [SerializeField] private float rightPadding = 20f;
    [SerializeField] private float panelWidth = 180f;
    [SerializeField] private float panelHeight = 48f;

    [Header("Text")]
    [SerializeField] private string timerPrefix = "Cat:";
    [SerializeField] private int fontSize = 20;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.6f);

    private GUIStyle _labelStyle;

    private void Awake()
    {
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
    }

    private void OnGUI()
    {
        if (playerController == null || !playerController.IsCatForm)
            return;

        EnsureStyle();

        float x = Screen.width - panelWidth - rightPadding;
        float y = topPadding;
        Rect panelRect = new Rect(x, y, panelWidth, panelHeight);

        Color oldColor = GUI.color;
        GUI.color = panelColor;
        GUI.Box(panelRect, string.Empty);
        GUI.color = oldColor;

        float secondsRemaining = playerController.CatFormTimeRemaining;
        string label = $"{timerPrefix} {secondsRemaining:0.0}s";
        GUI.Label(panelRect, label, _labelStyle);
    }

    private void EnsureStyle()
    {
        if (_labelStyle != null)
            return;

        _labelStyle = new GUIStyle
        {
            fontSize = fontSize,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        _labelStyle.normal.textColor = textColor;
    }
}
