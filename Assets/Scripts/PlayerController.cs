using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Inventory))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private HumanFormData humanFormData;
    [SerializeField] private CatFormData catFormData;

    [Header("Collider References (for form switching)")]
    [SerializeField] private CapsuleCollider2D capsuleCollider2D;
    [SerializeField] private BoxCollider2D boxCollider2D;

    [Tooltip("If true, the collider will be resized when switching forms.")]
    [SerializeField] private bool adjustCollidersOnFormSwitch = true;

    [Header("Cat Collider Sizes")]
    [Tooltip("Cat capsule height (CapsuleCollider2D.size.y). Radius (size.x) is kept as-is.")]
    [SerializeField] private float catCapsuleHeight = 1f;

    [Tooltip("Cat box collider height (BoxCollider2D.size.y). X size is kept as-is.")]
    [SerializeField] private float catBoxHeight = 1f;

    [Tooltip("If a BoxCollider2D exists, enables it for the cat form (square collider) and disables the capsule during cat.")]
    [SerializeField] private bool useBoxColliderForCat = true;

    [Tooltip("If true, uses explicit cat offsets for the colliders instead of keeping the human offsets.")]
    [SerializeField] private bool useCustomCatColliderOffsets = false;

    [Tooltip("Cat capsule offset.y (only used if Use Custom Cat Collider Offsets is true).")]
    [SerializeField] private float catCapsuleOffsetY = 0f;

    [Tooltip("Cat box offset.y (only used if Use Custom Cat Collider Offsets is true).")]
    [SerializeField] private float catBoxOffsetY = 0f;

    // Stored "human" values so we can return to them.
    private bool _hasHumanColliderCache;
    private Vector2 _humanCapsuleSize;
    private Vector2 _humanCapsuleOffset;
    private Vector2 _humanBoxSize;
    private Vector2 _humanBoxOffset;
    private Vector3 _humanGroundCheckLocalPosition;
    private float _groundCheckRelativeToCapsuleBottomY;
    private float _groundCheckRelativeToBoxBottomY;
    private Inventory _inventory;

    private const ResourceType TransformationResource = ResourceType.FallenStar;
    private const int TransformationCost = 5;

    [Header("Switching")]
    [Tooltip("Key used to toggle between Human and Cat forms.")]
    [SerializeField] private KeyCode switchKey = KeyCode.C;
    [Tooltip("How long cat form lasts before automatically returning to human form.")]
    [SerializeField] private float catFormDurationSeconds = 45f;

    private bool _isCat;
    private float _catFormTimeRemaining;
    private bool _isDead;

    [Header("Death")]
    [SerializeField] private UnityEvent onPlayerDied;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        _inventory = GetComponent<Inventory>();
        if (_inventory == null)
            _inventory = GetComponentInParent<Inventory>();

        if (capsuleCollider2D == null)
            capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        if (boxCollider2D == null)
            boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        // If the designer already assigned a FormData in the inspector, keep it.
        // Otherwise default to Human.
        FormData current = playerMovement.CurrentFormData;
        _isCat = current is CatFormData;

        if (current == null && humanFormData != null)
            playerMovement.SetFormData(humanFormData);

        // If current was null but humanFormData is also missing, _isCat stays false.
        ApplySprite(playerMovement.CurrentFormData);
        _catFormTimeRemaining = _isCat ? catFormDurationSeconds : 0f;
        CacheHumanColliderValuesIfNeeded();
        ApplyColliderForCurrentForm();
        LogActiveForm();
    }

    private void Update()
    {
        if (_isDead)
            return;

        HandleCatFormTimer();

        if (!Input.GetKeyDown(switchKey))
            return;

        // Decide based on what is currently active, not a separate toggled flag.
        bool currentlyCat = playerMovement.CurrentFormData is CatFormData;
        bool wantCat = !currentlyCat;
        FormData nextForm = wantCat ? catFormData : humanFormData;

        if (nextForm == null)
        {
            Debug.LogWarning($"PlayerController: Missing FormData for {(wantCat ? "Cat" : "Human")}.");
            return;
        }

        if (_inventory == null)
        {
            Debug.LogWarning("PlayerController: Missing Inventory reference.");
            return;
        }

        // Pay the resource cost before allowing the transformation.
        if (!_inventory.HasResource(TransformationResource, TransformationCost))
            return;

        if (!_inventory.RemoveResource(TransformationResource, TransformationCost))
            return;

        ApplyForm(nextForm, wantCat);
    }

    private void HandleCatFormTimer()
    {
        if (!_isCat || humanFormData == null)
            return;

        _catFormTimeRemaining -= Time.deltaTime;
        if (_catFormTimeRemaining > 0f)
            return;

        // Auto-revert does not consume resources.
        ApplyForm(humanFormData, false);
    }

    private void ApplyForm(FormData nextForm, bool isCat)
    {
        if (_isDead || nextForm == null)
            return;

        playerMovement.SetFormData(nextForm);
        _isCat = isCat;
        _catFormTimeRemaining = _isCat ? catFormDurationSeconds : 0f;
        ApplySprite(nextForm);
        ApplyColliderForCurrentForm();
        LogActiveForm();
    }

    private void ApplySprite(FormData form)
    {
        if (spriteRenderer == null || form == null)
            return;

        if (form.FormSprite != null)
            spriteRenderer.sprite = form.FormSprite;
    }

    private void LogActiveForm()
    {
        Debug.Log($"Active form: {(_isCat ? "Cat" : "Human")}");
    }

    private void CacheHumanColliderValuesIfNeeded()
    {
        if (_hasHumanColliderCache)
            return;

        // Cache human values from the current collider state (assumed to be human defaults).
        if (capsuleCollider2D != null)
        {
            _humanCapsuleSize = capsuleCollider2D.size;
            _humanCapsuleOffset = capsuleCollider2D.offset;
        }

        if (boxCollider2D != null)
        {
            _humanBoxSize = boxCollider2D.size;
            _humanBoxOffset = boxCollider2D.offset;
        }

        if (playerMovement != null && playerMovement.GroundCheckTransform != null)
            _humanGroundCheckLocalPosition = playerMovement.GroundCheckTransform.localPosition;

        // Cache how far the ground check is from the bottom of the colliders in local space.
        // This lets us move the ground check appropriately when collider height/offset changes.
        if (capsuleCollider2D != null && playerMovement != null && playerMovement.GroundCheckTransform != null)
        {
            float humanCapsuleHeight = _humanCapsuleSize.y;
            float humanCapsuleBottomY = _humanCapsuleOffset.y - humanCapsuleHeight * 0.5f;
            _groundCheckRelativeToCapsuleBottomY = _humanGroundCheckLocalPosition.y - humanCapsuleBottomY;
        }
        else
        {
            _groundCheckRelativeToCapsuleBottomY = 0f;
        }

        if (boxCollider2D != null && playerMovement != null && playerMovement.GroundCheckTransform != null)
        {
            float humanBoxHeight = _humanBoxSize.y;
            float humanBoxBottomY = _humanBoxOffset.y - humanBoxHeight * 0.5f;
            _groundCheckRelativeToBoxBottomY = _humanGroundCheckLocalPosition.y - humanBoxBottomY;
        }
        else
        {
            _groundCheckRelativeToBoxBottomY = 0f;
        }

        _hasHumanColliderCache = true;
    }

    private void ApplyColliderForCurrentForm()
    {
        if (!adjustCollidersOnFormSwitch)
            return;

        if (!_hasHumanColliderCache)
            CacheHumanColliderValuesIfNeeded();

        // If you added a BoxCollider2D and want cat to use it, enable/disable the correct collider.
        bool useBox = useBoxColliderForCat && boxCollider2D != null;
        if (capsuleCollider2D != null)
            capsuleCollider2D.enabled = !(_isCat && useBox);
        if (boxCollider2D != null)
            boxCollider2D.enabled = _isCat && useBox;

        if (capsuleCollider2D != null)
        {
            Vector2 newSize = capsuleCollider2D.size;
            float humanHeight = _humanCapsuleSize.y;
            float newHeight = _isCat ? catCapsuleHeight : humanHeight;
            newSize.y = newHeight;
            capsuleCollider2D.size = newSize;

            // Keep the collider bottom aligned while changing height:
            // bottomY = offset.y - height/2
            // => offset.y = bottomY + height/2
            float autoAlignedOffsetY = _humanCapsuleOffset.y + (newHeight - humanHeight) * 0.5f;
            float newOffsetY = autoAlignedOffsetY;
            if (_isCat && useCustomCatColliderOffsets)
                newOffsetY = catCapsuleOffsetY;

            capsuleCollider2D.offset = new Vector2(_humanCapsuleOffset.x, newOffsetY);
        }

        if (boxCollider2D != null)
        {
            Vector2 newSize = boxCollider2D.size;
            float newHeight = _isCat ? catBoxHeight : _humanBoxSize.y;
            newSize.y = newHeight;
            boxCollider2D.size = newSize;

            float newOffsetY = _humanBoxOffset.y;
            if (_isCat && useCustomCatColliderOffsets)
                newOffsetY = catBoxOffsetY;

            boxCollider2D.offset = new Vector2(_humanBoxOffset.x, newOffsetY);
        }

        // Move ground check so it stays positioned relative to whichever collider exists.
        if (playerMovement != null && playerMovement.GroundCheckTransform != null)
        {
            Vector3 localPos = playerMovement.GroundCheckTransform.localPosition;

            bool capsuleActive = capsuleCollider2D != null && capsuleCollider2D.enabled;
            bool boxActive = boxCollider2D != null && boxCollider2D.enabled;

            // Prefer whichever collider is currently enabled.
            if (capsuleActive)
            {
                float height = capsuleCollider2D.size.y;
                float bottomY = capsuleCollider2D.offset.y - height * 0.5f;
                localPos.y = bottomY + _groundCheckRelativeToCapsuleBottomY;
            }
            else if (boxActive)
            {
                float height = boxCollider2D.size.y;
                float bottomY = boxCollider2D.offset.y - height * 0.5f;
                localPos.y = bottomY + _groundCheckRelativeToBoxBottomY;
            }

            playerMovement.GroundCheckTransform.localPosition = localPos;
        }
    }

    public bool IsCatForm => _isCat;
    public float CatFormTimeRemaining => Mathf.Max(0f, _catFormTimeRemaining);
    public float CatFormDurationSeconds => catFormDurationSeconds;
    public bool IsDead => _isDead;

    public void Die()
    {
        if (_isDead)
            return;

        // If the player already won by repairing the ship, don't allow enemies to kill them.
        if (GameManager.Instance != null && GameManager.Instance.IsWin)
            return;

        _isDead = true;
        _isCat = false;
        _catFormTimeRemaining = 0f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPlayerDeath();

        if (playerMovement != null)
            playerMovement.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        onPlayerDied?.Invoke();
        Debug.Log("Player died.");
    }
}

