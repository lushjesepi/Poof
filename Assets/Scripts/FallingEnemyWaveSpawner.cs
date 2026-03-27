using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns falling enemies in 3 waves.
/// Default counts: 5, 10, 15 with configurable delay between waves.
/// </summary>
public class FallingEnemyWaveSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FallingEnemy enemyPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameManager gameManager;

    [Header("Waves")]
    [SerializeField] private int wave1Count = 5;
    [SerializeField] private int wave2Count = 10;
    [SerializeField] private int wave3Count = 15;
    [Tooltip("Seconds between each wave.")]
    [SerializeField] private float secondsBetweenWaves = 20f;
    [Tooltip("Seconds between wave 3 completing and wave 1 starting again.")]
    [SerializeField] private float secondsBetweenCycles = 0f;
    [Tooltip("If true, repeats waves until GameManager.IsWin is true.")]
    [SerializeField] private bool loopUntilWin = true;

    [Header("Spawn Area")]
    [Tooltip("Optional collider used for spawn X bounds.")]
    [SerializeField] private Collider2D spawnBounds;
    [SerializeField] private float spawnXMin = -12f;
    [SerializeField] private float spawnXMax = 12f;
    [SerializeField] private float spawnY = 9f;
    [SerializeField] private float minDistanceFromPlayerX = 1f;

    private void Awake()
    {
        if (playerTransform == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
                playerTransform = player.transform;
        }

        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        if (gameManager == null)
            gameManager = GameManager.Instance != null ? GameManager.Instance : FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        StartCoroutine(SpawnWavesRoutine());
    }

    private IEnumerator SpawnWavesRoutine()
    {
        int[] waveCounts = { wave1Count, wave2Count, wave3Count };

        while (true)
        {
            if (playerController != null && playerController.IsDead)
                yield break;

            if (loopUntilWin && gameManager != null && gameManager.IsWin)
                yield break;

            for (int i = 0; i < waveCounts.Length; i++)
            {
                if (playerController != null && playerController.IsDead)
                    yield break;

                if (loopUntilWin && gameManager != null && gameManager.IsWin)
                    yield break;

                SpawnWave(waveCounts[i]);

                if (i < waveCounts.Length - 1 && secondsBetweenWaves > 0f)
                    yield return new WaitForSeconds(secondsBetweenWaves);
            }

            if (secondsBetweenCycles > 0f)
                yield return new WaitForSeconds(secondsBetweenCycles);
        }
    }

    private void SpawnWave(int count)
    {
        if (enemyPrefab == null || count <= 0)
            return;

        for (int i = 0; i < count; i++)
        {
            float spawnX = GetSpawnX();
            Vector3 spawnPosition = new Vector3(spawnX, GetSpawnY(), 0f);
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, transform);
        }
    }

    private float GetSpawnX()
    {
        float minX = spawnXMin;
        float maxX = spawnXMax;

        if (spawnBounds != null)
        {
            minX = spawnBounds.bounds.min.x;
            maxX = spawnBounds.bounds.max.x;
        }

        for (int attempt = 0; attempt < 20; attempt++)
        {
            float x = Random.Range(minX, maxX);
            if (playerTransform == null || minDistanceFromPlayerX <= 0f)
                return x;

            if (Mathf.Abs(x - playerTransform.position.x) >= minDistanceFromPlayerX)
                return x;
        }

        return Random.Range(minX, maxX);
    }

    private float GetSpawnY()
    {
        if (spawnBounds != null)
            return spawnBounds.bounds.max.y;

        return spawnY;
    }
}
