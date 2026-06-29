//-----PlayerInteractor.cs START-----

using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InteractionPromptUI promptUI;
    [SerializeField] private PlayerInputReader inputReader;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactMask;

    private IInteractable currentInteractable;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (inputReader == null)
            inputReader = GetComponent<PlayerInputReader>();
    }

    private void Update()
    {
        CheckForInteractable();

        if (currentInteractable != null && inputReader != null && inputReader.InteractPressed)
            currentInteractable.Interact(this);
    }

    private void CheckForInteractable()
    {
        currentInteractable = null;

        if (playerCamera == null)
        {
            HidePrompt();
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
        {
            currentInteractable = hit.collider.GetComponentInParent<IInteractable>();

            if (currentInteractable != null)
            {
                if (promptUI != null)
                    promptUI.Show(currentInteractable.GetPromptText());

                return;
            }
        }

        HidePrompt();
    }

    private void HidePrompt()
    {
        if (promptUI != null)
            promptUI.Hide();
    }

    public void SetInteractionEnabled(bool isEnabled)
    {
        enabled = isEnabled;
        currentInteractable = null;

        if (!isEnabled && promptUI != null)
            promptUI.Hide();
    }
}

//-----PlayerInteractor.cs END-----