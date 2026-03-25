using UnityEngine;

public class FormData : ScriptableObject
{
    // Human defaults (lower values).
    public const float HumanMovementSpeedDefault = 3f;
    public const float HumanJumpForceDefault = 12f;

    [SerializeField] protected float movementSpeed = HumanMovementSpeedDefault;
    [SerializeField] protected float jumpForce = HumanJumpForceDefault;

    public float MovementSpeed => movementSpeed;
    public float JumpForce => jumpForce;
}

