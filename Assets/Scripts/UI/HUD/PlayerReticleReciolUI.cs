//-----PlayerReticleRecoilUI.cs START-----

using UnityEngine;

public class PlayerReticleRecoilUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform reticleRoot;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private CharacterController characterController;

    [Header("Fallback Reticle Recoil")]
    [SerializeField] private bool fallbackUseReticleRecoil = true;
    [SerializeField] private bool fallbackUseWeaponKickForReticle = true;
    [SerializeField] private Vector2 fallbackKickXRange = new Vector2(-4f, 4f);
    [SerializeField] private Vector2 fallbackKickYRange = new Vector2(6f, 12f);
    [SerializeField] private Vector2 fallbackPixelsPerWeaponRotationDegree = new Vector2(2.2f, 2.8f);
    [SerializeField] private float fallbackRollToHorizontalInfluence = 0.35f;
    [SerializeField] private float fallbackMaxKickOffset = 22f;
    [SerializeField] private float fallbackKickSnappiness = 28f;
    [SerializeField] private float fallbackReturnSpeed = 16f;

    [Header("Fallback Reticle Passive Motion")]
    [SerializeField] private bool fallbackUsePassiveMotion = true;
    [SerializeField] private bool fallbackUseLookSway = true;
    [SerializeField] private Vector2 fallbackLookSwayAmount = new Vector2(1.5f, 1.25f);

    [SerializeField] private bool fallbackUseMovementBob = true;
    [SerializeField] private Vector2 fallbackMovementBobAmount = new Vector2(1.4f, 0.8f);
    [SerializeField] private float fallbackMovementBobFrequency = 8f;
    [SerializeField] private float fallbackMovementInputThreshold = 0.1f;

    [SerializeField] private bool fallbackUseIdleBob = true;
    [SerializeField] private Vector2 fallbackIdleBobAmount = new Vector2(0f, 0.35f);
    [SerializeField] private float fallbackIdleBobFrequency = 1.6f;

    [SerializeField] private float fallbackMaxPassiveOffset = 6f;
    [SerializeField] private float fallbackPassiveSnappiness = 18f;

    [Header("Fallback Reticle Pulse")]
    [SerializeField] private bool fallbackUseScalePulse = true;
    [SerializeField] private float fallbackScaleKick = 0.12f;
    [SerializeField] private float fallbackMaxScaleKick = 0.25f;
    [SerializeField] private float fallbackScaleSnappiness = 32f;
    [SerializeField] private float fallbackScaleReturnSpeed = 18f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private WeaponHandlingData activeHandlingData;

    private Vector2 baseAnchoredPosition;
    private Vector3 baseLocalScale;

    private Vector2 targetFireOffset;
    private Vector2 currentFireOffset;

    private Vector2 targetPassiveOffset;
    private Vector2 currentPassiveOffset;

    private float targetScaleOffset;
    private float currentScaleOffset;

    private float activeKickSnappiness;
    private float activeReturnSpeed;
    private float activePassiveSnappiness;
    private float activeScaleSnappiness;
    private float activeScaleReturnSpeed;

    private float movementBobTimer;
    private float idleBobTimer;

    private void Awake()
    {
        if (reticleRoot == null)
            reticleRoot = GetComponent<RectTransform>();

        if (inputReader == null)
            inputReader = FindFirstObjectByType<PlayerInputReader>();

        if (characterController == null)
            characterController = FindFirstObjectByType<CharacterController>();

        CacheBaseValues();
        ResetActiveTuning();
        ValidateSetup();
    }

    private void OnEnable()
    {
        CacheBaseValues();
        ResetReticle();
    }

    private void Update()
    {
        if (reticleRoot == null)
            return;

        RefreshActiveTuning(activeHandlingData);

        float deltaTime = Time.unscaledDeltaTime;

        UpdateFireRecoil(deltaTime);
        UpdatePassiveMotion(deltaTime);
        UpdateScalePulse(deltaTime);

        ApplyReticleTransform();
    }

    public void SetActiveHandlingData(WeaponHandlingData handlingData)
    {
        activeHandlingData = handlingData;
        RefreshActiveTuning(activeHandlingData);
        ResetReticle();
    }

    public void ClearActiveHandlingData()
    {
        activeHandlingData = null;
        ResetActiveTuning();
        ResetReticle();
    }

    public void PlayFireFeedback(WeaponHandlingData handlingData)
    {
        PlayFireFeedback(handlingData, Vector3.zero, false);
    }

    public void PlayFireFeedback(
        WeaponHandlingData handlingData,
        Vector3 sharedWeaponRotationKick,
        bool hasSharedWeaponRotationKick)
    {
        if (reticleRoot == null)
            return;

        activeHandlingData = handlingData;
        RefreshActiveTuning(activeHandlingData);

        bool useReticleRecoil = handlingData != null
            ? handlingData.useReticleRecoil
            : fallbackUseReticleRecoil;

        if (useReticleRecoil)
            AddReticleKick(handlingData, sharedWeaponRotationKick, hasSharedWeaponRotationKick);

        bool useScalePulse = handlingData != null
            ? handlingData.useReticleScalePulse
            : fallbackUseScalePulse;

        if (useScalePulse)
            AddScaleKick(handlingData);

        if (debugLogs)
        {
            Debug.Log(
                $"Reticle fire feedback played. Fire Offset: {targetFireOffset}, Passive Offset: {targetPassiveOffset}, Scale: {targetScaleOffset}");
        }
    }

    public void ResetReticle()
    {
        targetFireOffset = Vector2.zero;
        currentFireOffset = Vector2.zero;

        targetPassiveOffset = Vector2.zero;
        currentPassiveOffset = Vector2.zero;

        targetScaleOffset = 0f;
        currentScaleOffset = 0f;

        movementBobTimer = 0f;
        idleBobTimer = 0f;

        ApplyReticleTransform();
    }

    [ContextMenu("Test Fire Feedback")]
    private void TestFireFeedback()
    {
        PlayFireFeedback(activeHandlingData, new Vector3(-4f, 1f, 0.5f), true);
    }

    private void UpdateFireRecoil(float deltaTime)
    {
        targetFireOffset = Vector2.Lerp(
            targetFireOffset,
            Vector2.zero,
            activeReturnSpeed * deltaTime);

        currentFireOffset = Vector2.Lerp(
            currentFireOffset,
            targetFireOffset,
            activeKickSnappiness * deltaTime);
    }

    private void UpdatePassiveMotion(float deltaTime)
    {
        if (!ShouldUsePassiveMotion())
        {
            targetPassiveOffset = Vector2.zero;
            currentPassiveOffset = Vector2.Lerp(
                currentPassiveOffset,
                Vector2.zero,
                activePassiveSnappiness * deltaTime);

            return;
        }

        Vector2 passiveOffset = Vector2.zero;

        if (ShouldUseLookSway() && inputReader != null)
        {
            Vector2 lookInput = inputReader.LookInput;
            Vector2 lookAmount = GetLookSwayAmount();

            passiveOffset += new Vector2(
                -lookInput.x * lookAmount.x,
                -lookInput.y * lookAmount.y);
        }

        float movementAmount = GetMovementAmount();

        if (movementAmount > GetMovementInputThreshold())
        {
            if (ShouldUseMovementBob())
            {
                movementBobTimer += deltaTime *
                                    GetMovementBobFrequency() *
                                    Mathf.Clamp01(movementAmount);

                Vector2 bobAmount = GetMovementBobAmount();

                passiveOffset += new Vector2(
                    Mathf.Sin(movementBobTimer) * bobAmount.x,
                    Mathf.Abs(Mathf.Cos(movementBobTimer)) * bobAmount.y);
            }
        }
        else
        {
            if (ShouldUseIdleBob())
            {
                idleBobTimer += deltaTime * GetIdleBobFrequency();

                Vector2 idleAmount = GetIdleBobAmount();

                passiveOffset += new Vector2(
                    Mathf.Sin(idleBobTimer) * idleAmount.x,
                    Mathf.Sin(idleBobTimer) * idleAmount.y);
            }
        }

        passiveOffset = Vector2.ClampMagnitude(
            passiveOffset,
            Mathf.Max(0f, GetMaxPassiveOffset()));

        targetPassiveOffset = passiveOffset;

        currentPassiveOffset = Vector2.Lerp(
            currentPassiveOffset,
            targetPassiveOffset,
            activePassiveSnappiness * deltaTime);
    }

    private void UpdateScalePulse(float deltaTime)
    {
        targetScaleOffset = Mathf.Lerp(
            targetScaleOffset,
            0f,
            activeScaleReturnSpeed * deltaTime);

        currentScaleOffset = Mathf.Lerp(
            currentScaleOffset,
            targetScaleOffset,
            activeScaleSnappiness * deltaTime);
    }

    private void AddReticleKick(
        WeaponHandlingData handlingData,
        Vector3 sharedWeaponRotationKick,
        bool hasSharedWeaponRotationKick)
    {
        bool useWeaponKick = handlingData != null
            ? handlingData.useWeaponKickForReticle
            : fallbackUseWeaponKickForReticle;

        Vector2 kick;

        if (useWeaponKick && hasSharedWeaponRotationKick)
            kick = ConvertWeaponKickToReticleKick(handlingData, sharedWeaponRotationKick);
        else
            kick = GetRandomReticleKick(handlingData);

        float maxKickOffset = handlingData != null
            ? handlingData.maxReticleKickOffset
            : fallbackMaxKickOffset;

        targetFireOffset += kick;
        targetFireOffset = Vector2.ClampMagnitude(targetFireOffset, Mathf.Max(0f, maxKickOffset));
    }

    private Vector2 ConvertWeaponKickToReticleKick(
        WeaponHandlingData handlingData,
        Vector3 weaponRotationKick)
    {
        Vector2 pixelsPerDegree = handlingData != null
            ? handlingData.reticlePixelsPerWeaponRotationDegree
            : fallbackPixelsPerWeaponRotationDegree;

        float rollInfluence = handlingData != null
            ? handlingData.reticleRollToHorizontalInfluence
            : fallbackRollToHorizontalInfluence;

        float x = (weaponRotationKick.y + weaponRotationKick.z * rollInfluence) * pixelsPerDegree.x;

        // Weapon pitch kick is usually negative for upward recoil.
        // Reticle UI positive Y moves upward in anchoredPosition.
        float y = -weaponRotationKick.x * pixelsPerDegree.y;

        return new Vector2(x, y);
    }

    private Vector2 GetRandomReticleKick(WeaponHandlingData handlingData)
    {
        Vector2 xRange = handlingData != null
            ? handlingData.reticleKickXRange
            : fallbackKickXRange;

        Vector2 yRange = handlingData != null
            ? handlingData.reticleKickYRange
            : fallbackKickYRange;

        float minX = Mathf.Min(xRange.x, xRange.y);
        float maxX = Mathf.Max(xRange.x, xRange.y);

        float minY = Mathf.Min(yRange.x, yRange.y);
        float maxY = Mathf.Max(yRange.x, yRange.y);

        return new Vector2(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY));
    }

    private void AddScaleKick(WeaponHandlingData handlingData)
    {
        float scaleKick = handlingData != null
            ? handlingData.reticleScaleKick
            : fallbackScaleKick;

        float maxScaleKick = handlingData != null
            ? handlingData.maxReticleScaleKick
            : fallbackMaxScaleKick;

        targetScaleOffset += scaleKick;
        targetScaleOffset = Mathf.Clamp(targetScaleOffset, 0f, Mathf.Max(0f, maxScaleKick));
    }

    private float GetMovementAmount()
    {
        if (characterController != null)
        {
            Vector3 velocity = characterController.velocity;
            velocity.y = 0f;
            return Mathf.Clamp01(velocity.magnitude);
        }

        if (inputReader != null)
            return Mathf.Clamp01(inputReader.MoveInput.magnitude);

        return 0f;
    }

    private void ApplyReticleTransform()
    {
        if (reticleRoot == null)
            return;

        Vector2 finalOffset = currentFireOffset + currentPassiveOffset;

        reticleRoot.anchoredPosition = baseAnchoredPosition + finalOffset;

        float scaleMultiplier = 1f + currentScaleOffset;
        reticleRoot.localScale = baseLocalScale * scaleMultiplier;
    }

    private void CacheBaseValues()
    {
        if (reticleRoot == null)
            return;

        baseAnchoredPosition = reticleRoot.anchoredPosition;
        baseLocalScale = reticleRoot.localScale;
    }

    private void ResetActiveTuning()
    {
        activeKickSnappiness = Mathf.Max(0.01f, fallbackKickSnappiness);
        activeReturnSpeed = Mathf.Max(0.01f, fallbackReturnSpeed);
        activePassiveSnappiness = Mathf.Max(0.01f, fallbackPassiveSnappiness);
        activeScaleSnappiness = Mathf.Max(0.01f, fallbackScaleSnappiness);
        activeScaleReturnSpeed = Mathf.Max(0.01f, fallbackScaleReturnSpeed);
    }

    private void RefreshActiveTuning(WeaponHandlingData handlingData)
    {
        activeKickSnappiness = handlingData != null
            ? Mathf.Max(0.01f, handlingData.reticleKickSnappiness)
            : Mathf.Max(0.01f, fallbackKickSnappiness);

        activeReturnSpeed = handlingData != null
            ? Mathf.Max(0.01f, handlingData.reticleReturnSpeed)
            : Mathf.Max(0.01f, fallbackReturnSpeed);

        activePassiveSnappiness = handlingData != null
            ? Mathf.Max(0.01f, handlingData.reticlePassiveSnappiness)
            : Mathf.Max(0.01f, fallbackPassiveSnappiness);

        activeScaleSnappiness = handlingData != null
            ? Mathf.Max(0.01f, handlingData.reticleScaleSnappiness)
            : Mathf.Max(0.01f, fallbackScaleSnappiness);

        activeScaleReturnSpeed = handlingData != null
            ? Mathf.Max(0.01f, handlingData.reticleScaleReturnSpeed)
            : Mathf.Max(0.01f, fallbackScaleReturnSpeed);
    }

    private bool ShouldUsePassiveMotion()
    {
        return activeHandlingData != null
            ? activeHandlingData.useReticlePassiveMotion
            : fallbackUsePassiveMotion;
    }

    private bool ShouldUseLookSway()
    {
        return activeHandlingData != null
            ? activeHandlingData.useReticleLookSway
            : fallbackUseLookSway;
    }

    private Vector2 GetLookSwayAmount()
    {
        return activeHandlingData != null
            ? activeHandlingData.reticleLookSwayAmount
            : fallbackLookSwayAmount;
    }

    private bool ShouldUseMovementBob()
    {
        return activeHandlingData != null
            ? activeHandlingData.useReticleMovementBob
            : fallbackUseMovementBob;
    }

    private Vector2 GetMovementBobAmount()
    {
        return activeHandlingData != null
            ? activeHandlingData.reticleMovementBobAmount
            : fallbackMovementBobAmount;
    }

    private float GetMovementBobFrequency()
    {
        return activeHandlingData != null
            ? Mathf.Max(0.01f, activeHandlingData.reticleMovementBobFrequency)
            : Mathf.Max(0.01f, fallbackMovementBobFrequency);
    }

    private float GetMovementInputThreshold()
    {
        return activeHandlingData != null
            ? Mathf.Max(0f, activeHandlingData.reticleMovementInputThreshold)
            : Mathf.Max(0f, fallbackMovementInputThreshold);
    }

    private bool ShouldUseIdleBob()
    {
        return activeHandlingData != null
            ? activeHandlingData.useReticleIdleBob
            : fallbackUseIdleBob;
    }

    private Vector2 GetIdleBobAmount()
    {
        return activeHandlingData != null
            ? activeHandlingData.reticleIdleBobAmount
            : fallbackIdleBobAmount;
    }

    private float GetIdleBobFrequency()
    {
        return activeHandlingData != null
            ? Mathf.Max(0.01f, activeHandlingData.reticleIdleBobFrequency)
            : Mathf.Max(0.01f, fallbackIdleBobFrequency);
    }

    private float GetMaxPassiveOffset()
    {
        return activeHandlingData != null
            ? Mathf.Max(0f, activeHandlingData.maxReticlePassiveOffset)
            : Mathf.Max(0f, fallbackMaxPassiveOffset);
    }

    private void ValidateSetup()
    {
        if (reticleRoot == null)
            return;

        if (reticleRoot.GetComponent<Canvas>() != null)
        {
            Debug.LogWarning(
                "PlayerReticleRecoilUI is assigned to a Canvas. Assign the small ReticleRoot/Image RectTransform instead.");
        }
    }
}

//-----PlayerReticleRecoilUI.cs END-----