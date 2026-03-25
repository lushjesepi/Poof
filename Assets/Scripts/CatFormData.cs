using UnityEngine;

[CreateAssetMenu(menuName = "Forms/Cat")]
public class CatFormData : FormData
{
    // Cat defaults (higher values).
    private const float CatMovementSpeedDefault = 7f;
    private const float CatJumpForceDefault = 16f;

    private void OnValidate()
    {
        // If the asset is freshly created (still on human defaults), set cat defaults.
        if (Mathf.Approximately(movementSpeed, HumanMovementSpeedDefault) &&
            Mathf.Approximately(jumpForce, HumanJumpForceDefault))
        {
            movementSpeed = CatMovementSpeedDefault;
            jumpForce = CatJumpForceDefault;
        }
    }
}

