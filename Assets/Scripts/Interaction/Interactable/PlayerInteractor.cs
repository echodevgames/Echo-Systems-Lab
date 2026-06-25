//-----PlayerInteractor.cs-----

using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InteractionPromptUI promptUI;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactMask;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private IInteractable currentInteractable;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        CheckForInteractable();

        if (currentInteractable != null && Input.GetKeyDown(interactKey))
            currentInteractable.Interact(this);
    }

    private void CheckForInteractable()
    {
        currentInteractable = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
        {
            currentInteractable = hit.collider.GetComponentInParent<IInteractable>();

            if (currentInteractable != null)
            {
                promptUI.Show(currentInteractable.GetPromptText());
                return;
            }
        }

        promptUI.Hide();
    }
}
//-----PlayerInteractor.cs END-----