using UnityEngine;

/// <summary>
/// Kills the player on collision, then destroys itself.
/// Also destroys itself when hitting non-trigger colliders.
/// </summary>
public class FallingEnemy : MonoBehaviour
{
    [Header("Layer Filtering")]
    [Tooltip("Name of the platform/ground layer to ignore collisions with (so enemies pass through). Leave blank to disable.")]
    [SerializeField] private string platformLayerNameToIgnore = "Ground";

    [Tooltip("Name of the death layer on your tilemap. Enemies destroy themselves when they collide with this layer.")]
    [SerializeField] private string deathLayerName = "Death";

    [Tooltip("Destroy this enemy after this many seconds as a cleanup fallback.")]
    [SerializeField] private float selfDestructAfterSeconds = 15f;

    private bool _hasCollided;
    private int _platformLayerIndex = -1;
    private int _deathLayerIndex = -1;

    private void Awake()
    {
        _platformLayerIndex = !string.IsNullOrWhiteSpace(platformLayerNameToIgnore)
            ? LayerMask.NameToLayer(platformLayerNameToIgnore)
            : -1;

        _deathLayerIndex = !string.IsNullOrWhiteSpace(deathLayerName)
            ? LayerMask.NameToLayer(deathLayerName)
            : -1;

        IgnorePlatformCollisionsForThisEnemy();
    }

    private void Start()
    {
        if (selfDestructAfterSeconds > 0f)
            Destroy(gameObject, selfDestructAfterSeconds);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other);
    }

    private void HandleCollision(Collider2D other)
    {
        if (_hasCollided || other == null)
            return;

        int otherLayer = other.gameObject.layer;

        // Never destroy itself when hitting ignored platform/ground colliders.
        if (_platformLayerIndex >= 0 && otherLayer == _platformLayerIndex)
            return;

        _hasCollided = true;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
            player = other.GetComponentInParent<PlayerController>();

        if (player != null)
            player.Die();

        // If it hit the death layer, destroy (this is the primary behavior you requested).
        if ((_deathLayerIndex >= 0 && otherLayer == _deathLayerIndex) || player != null)
        {
            Destroy(gameObject);
            return;
        }

        // Otherwise, keep the original behavior: destroy on collision.
        Destroy(gameObject);
    }

    private void IgnorePlatformCollisionsForThisEnemy()
    {
        if (_platformLayerIndex < 0)
            return;

        Collider2D[] myColliders = GetComponentsInChildren<Collider2D>();
        if (myColliders == null || myColliders.Length == 0)
            return;

        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();
        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider2D other = allColliders[i];
            if (other == null || other.gameObject.layer != _platformLayerIndex)
                continue;

            for (int j = 0; j < myColliders.Length; j++)
            {
                Collider2D mine = myColliders[j];
                if (mine == null)
                    continue;

                Physics2D.IgnoreCollision(mine, other, true);
            }
        }
    }
}
