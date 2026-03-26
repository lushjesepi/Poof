using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory UI")]
    [SerializeField] private bool showInventoryUI = true;
    [SerializeField] private Vector2 inventoryUiPosition = new Vector2(10f, 10f);
    [SerializeField] private int inventoryUiFontSize = 16;
    [SerializeField] private int inventoryUiWidth = 260;
    [SerializeField] private int inventoryUiPadding = 10;
    [SerializeField] private Color inventoryUiTextColor = Color.white;
    [SerializeField] private Color inventoryUiBoxColor = new Color(0f, 0f, 0f, 0.35f);

    [Serializable]
    private struct ResourceAmount
    {
        public ResourceType type;
        public int amount;
    }

    [Header("Optional Starting Resources (Inspector)")]
    [SerializeField] private List<ResourceAmount> startingResources = new List<ResourceAmount>();

    private readonly Dictionary<ResourceType, int> _resources = new Dictionary<ResourceType, int>();
    private ResourceType[] _resourceTypes;
    private GUIStyle _labelStyle;
    private GUIStyle _titleStyle;

    private void Awake()
    {
        RebuildDictionaryFromStartingResources();
        _resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));
    }

    private void RebuildDictionaryFromStartingResources()
    {
        _resources.Clear();

        foreach (var entry in startingResources)
        {
            if (entry.amount <= 0)
                continue;

            if (_resources.ContainsKey(entry.type))
                _resources[entry.type] += entry.amount;
            else
                _resources[entry.type] = entry.amount;
        }
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (amount <= 0)
            return;

        if (_resources.TryGetValue(type, out int current))
            _resources[type] = current + amount;
        else
            _resources[type] = amount;
    }

    /// <summary>
    /// Attempts to remove the given amount. Returns true if fully removed.
    /// </summary>
    public bool RemoveResource(ResourceType type, int amount)
    {
        if (amount <= 0)
            return true;

        if (!_resources.TryGetValue(type, out int current))
            return false;

        if (current < amount)
            return false;

        _resources[type] = current - amount;
        return true;
    }

    public bool HasResource(ResourceType type, int amount)
    {
        if (amount <= 0)
            return true;

        return _resources.TryGetValue(type, out int current) && current >= amount;
    }

    public int GetResourceAmount(ResourceType type)
    {
        return _resources.TryGetValue(type, out int current) ? current : 0;
    }

    private void OnGUI()
    {
        if (!showInventoryUI || _resourceTypes == null || _resourceTypes.Length == 0)
            return;

        // GUIStyle setup must happen during OnGUI to avoid Unity GUI exceptions.
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = inventoryUiFontSize;
            _labelStyle.normal.textColor = inventoryUiTextColor;
            _labelStyle.alignment = TextAnchor.UpperLeft;

            _titleStyle = new GUIStyle(_labelStyle);
            _titleStyle.fontStyle = FontStyle.Bold;
        }

        float lineHeight = Mathf.Max(inventoryUiFontSize + 2, _labelStyle.lineHeight);
        float boxHeight = inventoryUiPadding * 2f + lineHeight * (_resourceTypes.Length + 1);

        Rect boxRect = new Rect(inventoryUiPosition.x, inventoryUiPosition.y, inventoryUiWidth, boxHeight);

        // Box background
        Color oldColor = GUI.color;
        GUI.color = inventoryUiBoxColor;
        GUI.Box(boxRect, string.Empty);
        GUI.color = oldColor;

        // Title
        Rect titleRect = new Rect(
            boxRect.x + inventoryUiPadding,
            boxRect.y + inventoryUiPadding * 0.5f,
            boxRect.width - inventoryUiPadding * 2f,
            lineHeight
        );
        GUI.Label(titleRect, "Inventory", _titleStyle);

        // Resource rows
        float y = titleRect.y + lineHeight;
        for (int i = 0; i < _resourceTypes.Length; i++)
        {
            ResourceType type = _resourceTypes[i];
            int amount = GetResourceAmount(type);

            Rect labelRect = new Rect(
                boxRect.x + inventoryUiPadding,
                y + i * lineHeight,
                boxRect.width - inventoryUiPadding * 2f,
                lineHeight
            );

            GUI.Label(labelRect, $"{GetDisplayName(type)}: {amount}", _labelStyle);
        }
    }

    private static string GetDisplayName(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Wood: return "Wood";
            case ResourceType.Stone: return "Stone";
            case ResourceType.Iron: return "Iron";
            case ResourceType.FallenStar: return "Fallen Stars";
            default: return type.ToString();
        }
    }
}

