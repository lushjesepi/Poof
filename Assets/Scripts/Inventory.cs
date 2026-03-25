using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Serializable]
    private struct ResourceAmount
    {
        public ResourceType type;
        public int amount;
    }

    [Header("Optional Starting Resources (Inspector)")]
    [SerializeField] private List<ResourceAmount> startingResources = new List<ResourceAmount>();

    private readonly Dictionary<ResourceType, int> _resources = new Dictionary<ResourceType, int>();

    private void Awake()
    {
        RebuildDictionaryFromStartingResources();
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
}

