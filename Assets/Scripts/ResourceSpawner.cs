using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Serializable]
    private class ResourceRoll
    {
        public ResourceType type = ResourceType.Wood;
        public int minAmount = 1;
        public int maxAmount = 1;
        [Min(0f)] public float weight = 1f;
    }

    [Header("Spawn Target")]
    [Tooltip("Prefab that contains ResourceNode + a Collider2D.")]
    [SerializeField] private ResourceNode resourceNodePrefab;

    [Header("Spawn Area")]
    [Tooltip("Optional collider to define the spawn bounds. If null, uses Spawn Min/Max values.")]
    [SerializeField] private Collider2D spawnBounds;
    [SerializeField] private Vector2 spawnMin = new Vector2(-10f, -5f);
    [SerializeField] private Vector2 spawnMax = new Vector2(10f, 5f);

    [SerializeField] private int spawnCount = 20;
    [SerializeField] private int maxSpawnAttempts = 250;

    [Header("Optional Player Exclusion")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float minDistanceFromPlayer = 1.5f;

    [Header("Random Resource Selection")]
    [SerializeField] private List<ResourceRoll> rolls = new List<ResourceRoll>
    {
        new ResourceRoll { type = ResourceType.Wood, minAmount = 1, maxAmount = 3, weight = 5f },
        new ResourceRoll { type = ResourceType.Stone, minAmount = 1, maxAmount = 3, weight = 3f },
        new ResourceRoll { type = ResourceType.Iron, minAmount = 1, maxAmount = 2, weight = 1.5f },
        new ResourceRoll { type = ResourceType.FallenStar, minAmount = 1, maxAmount = 1, weight = 0.2f },
    };

    private void Start()
    {
        EnsureRollsHaveAllTypes();
        SpawnRandom();
    }

    private void EnsureRollsHaveAllTypes()
    {
        if (rolls == null)
            rolls = new List<ResourceRoll>();

        var hasType = new HashSet<ResourceType>();
        for (int i = 0; i < rolls.Count; i++)
            hasType.Add(rolls[i].type);

        // If someone removed entries in the inspector, ensure we still spawn across all types.
        if (!hasType.Contains(ResourceType.Wood))
            rolls.Add(new ResourceRoll { type = ResourceType.Wood, minAmount = 1, maxAmount = 3, weight = 5f });
        if (!hasType.Contains(ResourceType.Stone))
            rolls.Add(new ResourceRoll { type = ResourceType.Stone, minAmount = 1, maxAmount = 3, weight = 3f });
        if (!hasType.Contains(ResourceType.Iron))
            rolls.Add(new ResourceRoll { type = ResourceType.Iron, minAmount = 1, maxAmount = 2, weight = 1.5f });
        if (!hasType.Contains(ResourceType.FallenStar))
            rolls.Add(new ResourceRoll { type = ResourceType.FallenStar, minAmount = 1, maxAmount = 1, weight = 0.2f });
    }

    private void SpawnRandom()
    {
        if (resourceNodePrefab == null)
        {
            Debug.LogError("ResourceSpawner: Missing ResourceNode prefab reference.");
            return;
        }

        if (rolls == null || rolls.Count == 0)
        {
            Debug.LogError("ResourceSpawner: No resource rolls configured.");
            return;
        }

        // Build a weighted selection list for resource types.
        float totalWeight = 0f;
        for (int i = 0; i < rolls.Count; i++)
            totalWeight += Mathf.Max(0f, rolls[i].weight);

        if (totalWeight <= 0f)
        {
            Debug.LogError("ResourceSpawner: Total roll weight must be > 0.");
            return;
        }

        Bounds? bounds = GetSpawnBoundsOrNull();

        Dictionary<ResourceType, int> spawnCounts = new Dictionary<ResourceType, int>();
        int spawned = 0;
        int attempts = 0;
        while (spawned < spawnCount && attempts < maxSpawnAttempts)
        {
            attempts++;

            Vector2 spawnPos2D = GetRandomPoint(bounds);

            if (playerTransform != null && minDistanceFromPlayer > 0f)
            {
                float d = Vector2.Distance(spawnPos2D, (Vector2)playerTransform.position);
                if (d < minDistanceFromPlayer)
                    continue;
            }

            ResourceRoll roll = PickRoll(totalWeight);
            int amount = UnityEngine.Random.Range(GetMinAmount(roll), GetMaxAmountInclusive(roll));

            if (!spawnCounts.TryGetValue(roll.type, out int current))
                spawnCounts[roll.type] = 0;
            spawnCounts[roll.type]++;

            ResourceNode node = Instantiate(resourceNodePrefab, new Vector3(spawnPos2D.x, spawnPos2D.y, 0f), Quaternion.identity, transform);
            node.Initialize(roll.type, amount);

            spawned++;
        }

        // One-time debug output so you can confirm the random selection is working.
        string summary = "ResourceSpawner spawn summary: spawned=" + spawned + " attempts=" + attempts + " | ";
        foreach (var kvp in spawnCounts)
            summary += kvp.Key + "=" + kvp.Value + " ";
        Debug.Log(summary);
    }

    private Bounds? GetSpawnBoundsOrNull()
    {
        if (spawnBounds != null)
            return spawnBounds.bounds;

        // Convert min/max into a 2D-aligned bounds.
        Vector2 center = (spawnMin + spawnMax) * 0.5f;
        Vector2 size = new Vector2(Mathf.Abs(spawnMax.x - spawnMin.x), Mathf.Abs(spawnMax.y - spawnMin.y));
        return new Bounds(new Vector3(center.x, center.y, 0f), new Vector3(size.x, size.y, 0f));
    }

    private Vector2 GetRandomPoint(Bounds? boundsOrNull)
    {
        Bounds b = boundsOrNull.Value;
        return new Vector2(
            UnityEngine.Random.Range(b.min.x, b.max.x),
            UnityEngine.Random.Range(b.min.y, b.max.y)
        );
    }

    private ResourceRoll PickRoll(float totalWeight)
    {
        float rollPoint = UnityEngine.Random.Range(0f, totalWeight);
        float running = 0f;

        for (int i = 0; i < rolls.Count; i++)
        {
            float w = Mathf.Max(0f, rolls[i].weight);
            if (w <= 0f)
                continue;

            running += w;
            if (rollPoint <= running)
                return rolls[i];
        }

        // Fallback (shouldn't happen).
        return rolls[rolls.Count - 1];
    }

    private int GetMinAmount(ResourceRoll roll)
    {
        if (roll.minAmount <= 0) return 0;
        return roll.minAmount;
    }

    private int GetMaxAmountInclusive(ResourceRoll roll)
    {
        int min = GetMinAmount(roll);
        int max = Mathf.Max(min, roll.maxAmount);

        // Random.Range(int,int) is max exclusive, so +1 to make it inclusive.
        return max + 1;
    }
}

