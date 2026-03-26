using UnityEngine;

/// <summary>
/// Displays the current ship repair stage requirements and player amounts.
/// Uses immediate-mode GUI (OnGUI) so no Canvas setup is required.
/// </summary>
public class ShipRepairRequirementsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Ship ship;
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private GameManager gameManager;

    [Header("Visibility")]
    [Tooltip("If true, requirements are shown only while this ship is active (player in range).")]
    [SerializeField] private bool showOnlyWhenShipActive = true;

    [Header("Layout")]
    [SerializeField] private Vector2 panelPosition = new Vector2(10f, 170f);
    [SerializeField] private int panelWidth = 360;
    [SerializeField] private int fontSize = 16;
    [SerializeField] private int padding = 10;

    [Header("Colors")]
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.45f);
    [SerializeField] private Color titleColor = Color.white;
    [SerializeField] private Color enoughColor = new Color(0.6f, 1f, 0.6f, 1f);
    [SerializeField] private Color missingColor = new Color(1f, 0.55f, 0.55f, 1f);

    private GUIStyle _titleStyle;
    private GUIStyle _rowStyle;
    private GUIStyle _hintStyle;

    private void Awake()
    {
        if (ship == null)
            ship = FindObjectOfType<Ship>();
        if (playerInventory == null)
            playerInventory = FindObjectOfType<Inventory>();
        if (gameManager == null)
            gameManager = GameManager.Instance != null ? GameManager.Instance : FindObjectOfType<GameManager>();
    }

    private void OnGUI()
    {
        if (ship == null || playerInventory == null)
            return;

        if (showOnlyWhenShipActive && gameManager != null && gameManager.ActiveShip != ship)
            return;

        EnsureStyles();

        if (ship.IsFullyRepaired)
        {
            DrawSimpleMessage("Ship fully repaired.");
            return;
        }

        Ship.RepairStage stage = ship.GetCurrentStage();
        if (stage == null)
            return;

        int rowCount = (stage.requiredResources != null ? stage.requiredResources.Count : 0) + 2;
        float lineHeight = Mathf.Max(fontSize + 2, _rowStyle.lineHeight);
        float panelHeight = padding * 2f + rowCount * lineHeight;

        Rect panelRect = new Rect(panelPosition.x, panelPosition.y, panelWidth, panelHeight);

        Color oldGuiColor = GUI.color;
        GUI.color = panelColor;
        GUI.Box(panelRect, string.Empty);
        GUI.color = oldGuiColor;

        float y = panelRect.y + padding;
        string stageTitle = $"Ship Repair {ship.CurrentStageIndex + 1}/{ship.TotalStages}";
        GUI.Label(new Rect(panelRect.x + padding, y, panelRect.width - padding * 2f, lineHeight), stageTitle, _titleStyle);
        y += lineHeight;

        string header = string.IsNullOrWhiteSpace(stage.stageName) ? "Requirements" : stage.stageName;
        GUI.Label(new Rect(panelRect.x + padding, y, panelRect.width - padding * 2f, lineHeight), header, _hintStyle);
        y += lineHeight;

        if (stage.requiredResources != null)
        {
            foreach (var req in stage.requiredResources)
            {
                if (req == null)
                    continue;

                int have = playerInventory.GetResourceAmount(req.type);
                bool enough = have >= req.amount;

                _rowStyle.normal.textColor = enough ? enoughColor : missingColor;
                string row = $"{ToDisplayName(req.type)}: {have}/{req.amount}";
                GUI.Label(new Rect(panelRect.x + padding, y, panelRect.width - padding * 2f, lineHeight), row, _rowStyle);
                y += lineHeight;
            }
        }
    }

    private void DrawSimpleMessage(string message)
    {
        EnsureStyles();

        float lineHeight = Mathf.Max(fontSize + 2, _rowStyle.lineHeight);
        float panelHeight = padding * 2f + lineHeight;
        Rect panelRect = new Rect(panelPosition.x, panelPosition.y, panelWidth, panelHeight);

        Color oldGuiColor = GUI.color;
        GUI.color = panelColor;
        GUI.Box(panelRect, string.Empty);
        GUI.color = oldGuiColor;

        _titleStyle.normal.textColor = titleColor;
        GUI.Label(new Rect(panelRect.x + padding, panelRect.y + padding, panelRect.width - padding * 2f, lineHeight), message, _titleStyle);
    }

    private void EnsureStyles()
    {
        if (_rowStyle != null)
            return;

        _titleStyle = new GUIStyle
        {
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperLeft
        };
        _titleStyle.normal.textColor = titleColor;

        _rowStyle = new GUIStyle
        {
            fontSize = fontSize,
            alignment = TextAnchor.UpperLeft
        };

        _hintStyle = new GUIStyle(_rowStyle)
        {
            fontStyle = FontStyle.Italic
        };
        _hintStyle.normal.textColor = titleColor;
    }

    private static string ToDisplayName(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.FallenStar:
                return "Fallen Stars";
            default:
                return type.ToString();
        }
    }
}

