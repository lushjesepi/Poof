using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ResourceNode : MonoBehaviour
{
    [Header("Collectible")]
    [SerializeField] private ResourceType resourceType = ResourceType.Wood;
    [SerializeField] private int amount = 1;

    /// <summary>
    /// Allows a spawner to configure the node at runtime.
    /// </summary>
    public void Initialize(ResourceType newResourceType, int newAmount)
    {
        resourceType = newResourceType;
        amount = Mathf.Max(0, newAmount);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryCollect(collision.collider);
    }

    private void TryCollect(Collider2D other)
    {
        if (other == null)
            return;

        if (amount <= 0)
            return;

        // Assumes the player object has an Inventory component somewhere in its hierarchy.
        Inventory inventory = other.GetComponentInParent<Inventory>();
        if (inventory == null)
            inventory = other.GetComponent<Inventory>();

        if (inventory == null)
            return;

        inventory.AddResource(resourceType, amount);
        Destroy(gameObject);
    }
}

