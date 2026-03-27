using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central game state manager.
/// - Tracks the main ship repair state
/// - Triggers a win state when that ship is fully repaired
/// - Tracks which ship the player is currently allowed to activate
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Ship Completion Win Condition")]
    [Tooltip("The single ship that must be fully repaired to win.")]
    [SerializeField] private Ship shipToRepair;

    [Header("Win State")]
    [SerializeField] private UnityEvent OnWin;

    private Ship _activeShip;
    private bool _isWin;

    public bool IsWin => _isWin;
    public Ship ActiveShip => _activeShip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (shipToRepair == null)
            shipToRepair = FindObjectOfType<Ship>(includeInactive: true);

        if (shipToRepair == null)
        {
            Debug.LogWarning("GameManager: No Ship found/assigned for win condition.");
            return;
        }

        shipToRepair.OnShipRepaired.AddListener(HandleShipFullyRepaired);

        if (shipToRepair.IsFullyRepaired)
            TriggerWin();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void HandleShipFullyRepaired()
    {
        TriggerWin();
    }

    private void TriggerWin()
    {
        if (_isWin)
            return;

        _isWin = true;
        Debug.Log("WIN! Ship is fully repaired.");
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayWin();
        OnWin?.Invoke();
    }

    /// <summary>
    /// Called by ship interaction scripts when the player enters range.
    /// </summary>
    public void SetActiveShip(Ship ship)
    {
        if (_isWin)
            return;

        _activeShip = ship;
    }

    /// <summary>
    /// Called by ship interaction scripts when the player exits range.
    /// </summary>
    public void ClearActiveShip(Ship ship)
    {
        if (_activeShip == ship)
            _activeShip = null;
    }

    /// <summary>
    /// Called when the player presses the repair/activate key.
    /// </summary>
    public bool TryActivateActiveShip(Inventory playerInventory)
    {
        if (_isWin)
            return false;

        if (_activeShip == null)
            return false;

        return _activeShip.TryRepair(playerInventory);
    }
}

