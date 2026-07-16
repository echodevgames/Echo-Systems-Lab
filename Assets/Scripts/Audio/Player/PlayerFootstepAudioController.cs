//-----PlayerFootstepAudioController.cs START-----

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFootstepAudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private SimpleFirstPersonController firstPersonController;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private Transform footstepOrigin;

    [Header("Surface Detection")]
    [SerializeField] private FootstepSurfaceData defaultSurface;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float groundCheckDistance = 1.35f;

    [Header("Movement Detection")]
    [SerializeField] private float movementInputThreshold = 0.1f;
    [SerializeField] private float minimumHorizontalVelocity = 0.15f;
    [SerializeField] private bool requireVelocityForFootsteps = true;
    [SerializeField] private bool useRunFootstepsAboveSpeed = false;
    [SerializeField] private float runSpeedThreshold = 4.5f;

    [Header("Step Timing")]
    [SerializeField] private float defaultWalkStepInterval = 0.45f;
    [SerializeField] private float defaultRunStepInterval = 0.32f;

    [Header("Jump / Landing")]
    [SerializeField] private bool playJumpTakeoffAudio = true;
    [SerializeField] private bool playLandingAudio = true;
    [SerializeField] private float minimumAirTimeForLandingAudio = 0.15f;

    [Header("Playback")]
    [SerializeField] private bool playAttachedToPlayer = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private float stepTimer;
    private bool wasGrounded;
    private float airTime;

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (firstPersonController == null)
            firstPersonController = GetComponent<SimpleFirstPersonController>();

        if (inputReader == null)
            inputReader = GetComponent<PlayerInputReader>();

        if (footstepOrigin == null)
            footstepOrigin = transform;
    }

    private void OnEnable()
    {
        if (firstPersonController != null)
            firstPersonController.OnJumped += HandlePlayerJumped;
    }

    private void OnDisable()
    {
        if (firstPersonController != null)
            firstPersonController.OnJumped -= HandlePlayerJumped;
    }

    private void Update()
    {
        if (characterController == null)
            return;

        HandleGroundState();
        HandleFootsteps();
    }

    private void HandleGroundState()
    {
        bool isGrounded = characterController.isGrounded;

        if (!isGrounded)
        {
            airTime += Time.deltaTime;
            wasGrounded = false;
            return;
        }

        if (!wasGrounded)
        {
            if (playLandingAudio && airTime >= minimumAirTimeForLandingAudio)
                PlayLandingAudio();

            airTime = 0f;
            stepTimer = 0f;
        }

        wasGrounded = true;
    }

    private void HandleFootsteps()
    {
        if (!characterController.isGrounded)
            return;

        if (!IsMovingEnough())
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer > 0f)
            return;

        bool isRunning = IsRunning();
        FootstepSurfaceData surface = GetCurrentSurface();

        PlayFootstepAudio(surface, isRunning);

        float interval = GetStepInterval(surface, isRunning);
        stepTimer = interval;
    }

    private void HandlePlayerJumped()
    {
        if (!playJumpTakeoffAudio)
            return;

        PlayJumpTakeoffAudio();
    }

    private bool IsMovingEnough()
    {
        Vector2 moveInput = inputReader != null
            ? inputReader.MoveInput
            : Vector2.zero;

        Vector3 velocity = characterController.velocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

        bool hasInput = moveInput.magnitude >= movementInputThreshold;
        bool hasVelocity = horizontalVelocity.magnitude >= minimumHorizontalVelocity;

        if (requireVelocityForFootsteps)
            return hasInput && hasVelocity;

        return hasInput;
    }

    private bool IsRunning()
    {
        if (!useRunFootstepsAboveSpeed)
            return false;

        Vector3 velocity = characterController.velocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

        return horizontalVelocity.magnitude >= runSpeedThreshold;
    }

    private FootstepSurfaceData GetCurrentSurface()
    {
        Vector3 origin = footstepOrigin != null
            ? footstepOrigin.position + Vector3.up * 0.1f
            : transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(
                origin,
                Vector3.down,
                out RaycastHit hit,
                groundCheckDistance,
                groundMask,
                QueryTriggerInteraction.Ignore))
        {
            FootstepSurfaceTag surfaceTag = hit.collider.GetComponentInParent<FootstepSurfaceTag>();

            if (surfaceTag != null && surfaceTag.SurfaceData != null)
                return surfaceTag.SurfaceData;

            if (debugLogs)
                Debug.Log($"Footstep ray hit {hit.collider.name}, but it has no FootstepSurfaceTag. Using default surface.");
        }
        else
        {
            if (debugLogs)
                Debug.Log("Footstep ray did not hit ground. Using default surface.");
        }

        return defaultSurface;
    }

    private void PlayFootstepAudio(FootstepSurfaceData surface, bool isRunning)
    {
        if (surface == null)
        {
            if (debugLogs)
                Debug.LogWarning("No footstep surface found and no default surface assigned.");

            return;
        }

        AudioEventData audioEvent = surface.GetFootstepAudio(isRunning);

        if (audioEvent == null)
        {
            if (debugLogs)
                Debug.LogWarning($"Surface {surface.surfaceId} has no {(isRunning ? "run" : "walk")} footstep audio assigned.");

            return;
        }

        PlayAudioEvent(audioEvent);

        if (debugLogs)
            Debug.Log($"Footstep played on surface: {surface.surfaceId}");
    }

    private void PlayJumpTakeoffAudio()
    {
        FootstepSurfaceData surface = GetCurrentSurface();

        if (surface == null || surface.jumpTakeoffAudio == null)
            return;

        PlayAudioEvent(surface.jumpTakeoffAudio);

        if (debugLogs)
            Debug.Log($"Jump takeoff audio played on surface: {surface.surfaceId}");
    }

    private void PlayLandingAudio()
    {
        FootstepSurfaceData surface = GetCurrentSurface();

        if (surface == null || surface.landingAudio == null)
            return;

        PlayAudioEvent(surface.landingAudio);

        if (debugLogs)
            Debug.Log($"Landing audio played on surface: {surface.surfaceId}");
    }

    private void PlayAudioEvent(AudioEventData audioEvent)
    {
        if (audioEvent == null)
            return;

        if (GameAudioManager.Instance == null)
        {
            Debug.LogWarning("Tried to play footstep audio, but no GameAudioManager exists. Add SystemBootstrap with GameAudioManager to this scene.");
            return;
        }

        if (playAttachedToPlayer)
        {
            GameAudioManager.Instance.PlayOneShotAttached(audioEvent, transform);
            return;
        }

        Vector3 position = footstepOrigin != null
            ? footstepOrigin.position
            : transform.position;

        GameAudioManager.Instance.PlayOneShotAtPosition(audioEvent, position);
    }

    private float GetStepInterval(FootstepSurfaceData surface, bool isRunning)
    {
        if (surface != null)
            return surface.GetStepInterval(isRunning, defaultWalkStepInterval, defaultRunStepInterval);

        return isRunning
            ? Mathf.Max(0.05f, defaultRunStepInterval)
            : Mathf.Max(0.05f, defaultWalkStepInterval);
    }
}

//-----PlayerFootstepAudioController.cs END-----