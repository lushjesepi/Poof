using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Resource Prefabs (assign your prefabs)")]
    [SerializeField] private ResourceNode woodPrefab;
    [SerializeField] private ResourceNode stonePrefab;
    [SerializeField] private ResourceNode ironPrefab;
    [SerializeField] private ResourceNode fallenStarPrefab;

    [Header("Spawn Area")]
    [Tooltip("Optional collider used as the spawn bounds. If null, Spawn Min/Max are used.")]
    [SerializeField] private Collider2D spawnBounds;
    [SerializeField] private Vector2 spawnMin = new Vector2(-10f, -5f);
    [SerializeField] private Vector2 spawnMax = new Vector2(10f, 5f);

    [Header("Spawn Settings (per resource type)")]
    [SerializeField] private Vector2Int woodSpawnCount = new Vector2Int(5, 10);
    [SerializeField] private Vector2Int stoneSpawnCount = new Vector2Int(3, 8);
    [SerializeField] private Vector2Int ironSpawnCount = new Vector2Int(2, 6);
    [SerializeField] private Vector2Int fallenStarSpawnCount = new Vector2Int(1, 4);

    [Header("Manual Spawn Offsets (per resource type)")]
    [Tooltip("Extra position offset applied to spawned nodes of this type (e.g., raise Y so they appear above ground).")]
    [SerializeField] private Vector2 woodPositionOffset = Vector2.zero;
    [SerializeField] private Vector2 stonePositionOffset = Vector2.zero;
    [SerializeField] private Vector2 ironPositionOffset = Vector2.zero;
    [SerializeField] private Vector2 fallenStarPositionOffset = Vector2.zero;

    [SerializeField] private Vector2Int woodAmountRange = new Vector2Int(1, 3);
    [SerializeField] private Vector2Int stoneAmountRange = new Vector2Int(1, 3);
    [SerializeField] private Vector2Int ironAmountRange = new Vector2Int(1, 2);
    [SerializeField] private Vector2Int fallenStarAmountRange = new Vector2Int(1, 1);

    [Header("Optional Player Exclusion")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float minDistanceFromPlayer = 1.5f;
    [SerializeField] private int positionAttemptsPerNode = 50;

    private void Start()
    {
        SpawnAllTypes();
    }

    private void SpawnAllTypes()
    {
        Bounds bounds = GetSpawnBounds();

        SpawnType(woodPrefab, ResourceType.Wood, woodSpawnCount, woodAmountRange, bounds);
        SpawnType(stonePrefab, ResourceType.Stone, stoneSpawnCount, stoneAmountRange, bounds);
        SpawnType(ironPrefab, ResourceType.Iron, ironSpawnCount, ironAmountRange, bounds);
        SpawnType(fallenStarPrefab, ResourceType.FallenStar, fallenStarSpawnCount, fallenStarAmountRange, bounds);
    }

    private void SpawnType(
        ResourceNode prefab,
        ResourceType type,
        Vector2Int spawnCountRange,
        Vector2Int amountRange,
        Bounds bounds)
    {
        if (prefab == null)
            return;

        int spawnCount = RandomRangeInclusive(spawnCountRange);
        int amountMin = Mathf.Min(amountRange.x, amountRange.y);
        int amountMax = Mathf.Max(amountRange.x, amountRange.y);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 pos = GetRandomPosition(bounds) + GetPositionOffsetForType(type);
            int amount = Random.Range(amountMin, amountMax + 1);

            ResourceNode node = Instantiate(prefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity, transform);

            // Ensure the spawned node matches the type/amount we picked.
            // (This is safe even if your prefab is already configured.)
            node.Initialize(type, amount);
        }
    }

    private Vector2 GetPositionOffsetForType(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Wood: return woodPositionOffset;
            case ResourceType.Stone: return stonePositionOffset;
            case ResourceType.Iron: return ironPositionOffset;
            case ResourceType.FallenStar: return fallenStarPositionOffset;
            default: return Vector2.zero;
        }
    }

    private Bounds GetSpawnBounds()
    {
        if (spawnBounds != null)
            return spawnBounds.bounds;

        Vector2 center = (spawnMin + spawnMax) * 0.5f;
        Vector2 size = new Vector2(Mathf.Abs(spawnMax.x - spawnMin.x), Mathf.Abs(spawnMax.y - spawnMin.y));
        return new Bounds(new Vector3(center.x, center.y, 0f), new Vector3(size.x, size.y, 0f));
    }

    private Vector2 GetRandomPosition(Bounds bounds)
    {
        for (int attempt = 0; attempt < positionAttemptsPerNode; attempt++)
        {
            Vector2 pos = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y));

            if (playerTransform == null || minDistanceFromPlayer <= 0f)
                return pos;

            if (Vector2.Distance(pos, (Vector2)playerTransform.position) >= minDistanceFromPlayer)
                return pos;
        }

        // Fallback: return a position even if exclusion can’t be satisfied.
        return new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y));
    }

    private int RandomRangeInclusive(Vector2Int range)
    {
        int min = Mathf.Min(range.x, range.y);
        int max = Mathf.Max(range.x, range.y);
        return Random.Range(min, max + 1);
    }
}

