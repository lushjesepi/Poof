using UnityEngine;

/// <summary>
/// Detects the player within a ship's trigger range and repairs on key press.
/// </summary>
[RequireComponent(typeof(Ship))]
public class ShipRepairInteractor : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode repairKey = KeyCode.R;

    private Ship _ship;
    private GameManager _gameManager;

    private bool _playerInRange;
    private Inventory _playerInventory;

    private void Awake()
    {
        _ship = GetComponent<Ship>();
        _gameManager = GameManager.Instance != null ? GameManager.Instance : FindObjectOfType<GameManager>();
    }

    private void OnDrawGizmosSelected()
    {
        var trigger = GetComponent<Collider2D>();
        if (trigger == null)
            return;

        // Visualize the repair range in the editor.
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        if (trigger is BoxCollider2D box)
        {
            Vector3 center = box.offset;
            Gizmos.DrawCube((Vector3)box.transform.TransformPoint(center), (Vector3)box.size);
            return;
        }

        Gizmos.DrawWireCube(trigger.bounds.center, trigger.bounds.size);
    }

    private void Update()
    {
        if (!_playerInRange)
            return;

        if (!Input.GetKeyDown(repairKey))
            return;

        // Allow activation/repair to be mediated by the GameManager.
        if (_gameManager != null)
            _gameManager.TryActivateActiveShip(_playerInventory);
        else
            _ship.TryRepair(_playerInventory);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
            return;

        Inventory inv = other.GetComponentInParent<Inventory>();
        if (inv == null)
            inv = other.GetComponent<Inventory>();

        if (inv == null)
            return;

        _playerInRange = true;
        _playerInventory = inv;

        if (_gameManager != null)
            _gameManager.SetActiveShip(_ship);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null)
            return;

        Inventory inv = other.GetComponentInParent<Inventory>();
        if (inv == null)
            inv = other.GetComponent<Inventory>();

        if (inv == null)
            return;

        if (_playerInventory == inv)
        {
            _playerInRange = false;
            _playerInventory = null;

            if (_gameManager != null)
                _gameManager.ClearActiveShip(_ship);
        }
    }
}

