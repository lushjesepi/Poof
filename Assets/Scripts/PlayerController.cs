using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private HumanFormData humanFormData;
    [SerializeField] private CatFormData catFormData;

    [Header("Switching")]
    [Tooltip("Key used to toggle between Human and Cat forms.")]
    [SerializeField] private KeyCode switchKey = KeyCode.C;

    private bool _isCat;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
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
        LogActiveForm();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(switchKey))
            return;

        _isCat = !_isCat;
        FormData nextForm = _isCat ? catFormData : humanFormData;

        if (nextForm == null)
        {
            Debug.LogWarning($"PlayerController: Missing FormData for {(_isCat ? "Cat" : "Human")}.");
            return;
        }

        playerMovement.SetFormData(nextForm);
        LogActiveForm();
    }

    private void LogActiveForm()
    {
        Debug.Log($"Active form: {(_isCat ? "Cat" : "Human")}");
    }
}

