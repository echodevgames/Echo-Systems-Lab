//-----SimpleFirstPersonController.cs START-----

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerInputReader inputReader;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private CharacterController characterController;
    private float verticalVelocity;
    private float cameraPitch;
    private bool inputEnabled = true;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (inputReader == null)
            inputReader = GetComponent<PlayerInputReader>();
    }

    private void Start()
    {
        SetCursorLocked(true);
    }

    private void Update()
    {
        if (!inputEnabled)
            return;

        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        if (inputReader == null || playerCamera == null)
            return;

        Vector2 lookInput = inputReader.LookInput;

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        if (inputReader == null)
            return;

        Vector2 moveInput = inputReader.MoveInput;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move = move.normalized * moveSpeed;

        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = move;
        finalMove.y = verticalVelocity;

        characterController.Move(finalMove * Time.deltaTime);
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    public void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}

//-----SimpleFirstPersonController.cs END-----