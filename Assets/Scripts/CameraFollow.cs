using UnityEngine;

/// <summary>
/// Simple 2D camera follow with smoothing and optional bounds clamp.
/// Attach to Main Camera.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Follow")]
    [SerializeField] private bool smoothFollow = true;
    [SerializeField] private float smoothTime = 0.2f;

    [Header("Optional Bounds")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-100f, -100f);
    [SerializeField] private Vector2 maxBounds = new Vector2(100f, 100f);

    private Vector3 _velocity;

    private void Start()
    {
        if (target == null)
        {
            Inventory playerInventory = FindObjectOfType<Inventory>();
            if (playerInventory != null)
                target = playerInventory.transform;
        }

        if (target != null)
            SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desired = target.position + offset;
        desired = ApplyBounds(desired);

        if (smoothFollow)
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, Mathf.Max(0.01f, smoothTime));
        else
            transform.position = desired;
    }

    public void SetTarget(Transform newTarget, bool snapNow = true)
    {
        target = newTarget;
        if (snapNow && target != null)
            SnapToTarget();
    }

    private void SnapToTarget()
    {
        Vector3 snapped = target.position + offset;
        transform.position = ApplyBounds(snapped);
    }

    private Vector3 ApplyBounds(Vector3 position)
    {
        if (!useBounds)
            return position;

        position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
        position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
        return position;
    }
}

