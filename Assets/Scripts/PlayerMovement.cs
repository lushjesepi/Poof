using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement/jump parameters for the current player form. Assign HumanFormData or CatFormData (or another FormData).")]
    [SerializeField] private FormData formData;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.08f;
    [SerializeField] private LayerMask groundLayer = ~0;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Enable to flip sprite when moving left/right.")]
    [SerializeField] private bool flipSpriteOnMove = true;
    [Tooltip("If true, the sprite's default art direction is facing right.")]
    [SerializeField] private bool spriteFacesRightByDefault = true;
    [Tooltip("Enable squash/stretch while moving.")]
    [SerializeField] private bool animateWalkSquash = true;

    [Header("Human Walk Squash (Up/Down)")]
    [SerializeField] private float humanSquashAmount = 0.06f;
    [SerializeField] private float humanSquashSpeed = 10f;

    [Header("Cat Walk Squash (In/Out Horizontal)")]
    [SerializeField] private float catSquashAmount = 0.06f;
    [SerializeField] private float catSquashSpeed = 12f;
    [SerializeField] private float returnToNeutralSpeed = 12f;

    private Rigidbody2D _rb;
    private bool _isGrounded;
    private bool _jumpInputQueued;
    private Vector3 _baseSpriteScale = Vector3.one;
    private float _squashTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            _baseSpriteScale = spriteRenderer.transform.localScale;
    }

    private void Update()
    {
        // Update grounded state in Update for more responsive jump input.
        _isGrounded = CheckGrounded();

        // Queue jump input on Space press.
        if (Input.GetKeyDown(KeyCode.Space))
            _jumpInputQueued = true;

        float moveInput = Input.GetAxisRaw("Horizontal");
        UpdateWalkSquash(moveInput);
    }

    private void FixedUpdate()
    {
        if (formData == null)
        {
            // Movement is driven by the active form. Without it, do nothing to avoid missing values.
            return;
        }

        float movementSpeed = formData.MovementSpeed;
        float jumpForce = formData.JumpForce;

        float moveInput = Input.GetAxisRaw("Horizontal");
        _rb.linearVelocity = new Vector2(moveInput * movementSpeed, _rb.linearVelocity.y);
        UpdateFacing(moveInput);

        if (_jumpInputQueued && _isGrounded)
        {
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            _jumpInputQueued = false;
        }
    }

    private bool CheckGrounded()
    {
        if (groundCheck == null)
            return false;

        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    /// <summary>
    /// Sets the active movement/jump parameters.
    /// Does not touch Rigidbody2D velocity (movement stays physics-driven).
    /// </summary>
    public void SetFormData(FormData newFormData)
    {
        formData = newFormData;
    }

    public FormData CurrentFormData => formData;

    public Transform GroundCheckTransform => groundCheck;

    private void UpdateFacing(float moveInput)
    {
        if (!flipSpriteOnMove || spriteRenderer == null)
            return;

        if (Mathf.Approximately(moveInput, 0f))
            return;

        if (spriteFacesRightByDefault)
            spriteRenderer.flipX = moveInput < 0f;
        else
            spriteRenderer.flipX = moveInput > 0f;
    }

    private void UpdateWalkSquash(float moveInput)
    {
        if (!animateWalkSquash || spriteRenderer == null)
            return;

        Transform spriteTransform = spriteRenderer.transform;
        bool isMovingHorizontally = !Mathf.Approximately(moveInput, 0f);
        if (!isMovingHorizontally)
        {
            spriteTransform.localScale = Vector3.Lerp(
                spriteTransform.localScale,
                _baseSpriteScale,
                returnToNeutralSpeed * Time.deltaTime);
            return;
        }

        bool isCatForm = formData is CatFormData;
        float amount = isCatForm ? catSquashAmount : humanSquashAmount;
        float speed = isCatForm ? catSquashSpeed : humanSquashSpeed;

        _squashTime += Time.deltaTime * speed;
        float wave = Mathf.Sin(_squashTime) * amount;

        float xScale = _baseSpriteScale.x;
        float yScale = _baseSpriteScale.y;
        if (isCatForm)
        {
            // Cat: subtle in/out horizontal pulse while moving.
            xScale = _baseSpriteScale.x * (1f + wave);
            yScale = _baseSpriteScale.y * (1f - wave);
        }
        else
        {
            // Human: subtle up/down squash while moving.
            yScale = _baseSpriteScale.y * (1f + wave);
            xScale = _baseSpriteScale.x * (1f - wave);
        }

        spriteTransform.localScale = new Vector3(xScale, yScale, _baseSpriteScale.z);
    }
}

