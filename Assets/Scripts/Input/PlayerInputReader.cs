//-----PlayerInputReader.cs START-----

using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    private bool gameplayInputEnabled = true;

    public Vector2 MoveInput
    {
        get
        {
            if (!gameplayInputEnabled)
                return Vector2.zero;

            return inputActions.Player.Move.ReadValue<Vector2>();
        }
    }

    public Vector2 LookInput
    {
        get
        {
            if (!gameplayInputEnabled)
                return Vector2.zero;

            return inputActions.Player.Look.ReadValue<Vector2>();
        }
    }

    public bool InteractPressed =>
        gameplayInputEnabled &&
        inputActions.Player.Interact.WasPressedThisFrame();

    public bool FirePressed =>
        gameplayInputEnabled &&
        inputActions.Player.Attack.WasPressedThisFrame();

    public bool FireHeld =>
        gameplayInputEnabled &&
        inputActions.Player.Attack.IsPressed();

    public bool ReloadPressed =>
        gameplayInputEnabled &&
        inputActions.Player.Reload.WasPressedThisFrame();

    public bool CycleNextWeaponPressed =>
        gameplayInputEnabled &&
        inputActions.Player.Next.WasPressedThisFrame();

    public bool CyclePreviousWeaponPressed =>
        gameplayInputEnabled &&
        inputActions.Player.Previous.WasPressedThisFrame();

    public bool PausePressed =>
        inputActions.Player.Pause.WasPressedThisFrame();

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.UI.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.UI.Disable();
    }

    public void SetGameplayInputEnabled(bool enabled)
    {
        gameplayInputEnabled = enabled;
    }
}

//-----PlayerInputReader.cs END-----