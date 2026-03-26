using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Inventory))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private HumanFormData humanFormData;
    [SerializeField] private CatFormData catFormData;

    private Inventory _inventory;

    private const ResourceType TransformationResource = ResourceType.FallenStar;
    private const int TransformationCost = 5;

    [Header("Switching")]
    [Tooltip("Key used to toggle between Human and Cat forms.")]
    [SerializeField] private KeyCode switchKey = KeyCode.C;

    private bool _isCat;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        _inventory = GetComponent<Inventory>();
        if (_inventory == null)
            _inventory = GetComponentInParent<Inventory>();
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
        LogActiveForm();
    }

    private void Update()
    {
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

        playerMovement.SetFormData(nextForm);
        _isCat = wantCat;
        ApplySprite(nextForm);
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
}

