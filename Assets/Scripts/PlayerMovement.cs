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

    private Rigidbody2D _rb;
    private bool _isGrounded;
    private bool _jumpInputQueued;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Update grounded state in Update for more responsive jump input.
        _isGrounded = CheckGrounded();

        // Queue jump input on Space press.
        if (Input.GetKeyDown(KeyCode.Space))
            _jumpInputQueued = true;
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
}

