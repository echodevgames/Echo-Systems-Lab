//-----PlayerWeaponViewModelController.cs START-----

using UnityEngine;

public class PlayerWeaponViewModelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private CharacterController characterController;

    [Header("Fallback Handling")]
    [SerializeField] private Vector3 fallbackFirePositionKick = new Vector3(0f, -0.015f, -0.08f);
    [SerializeField] private Vector3 fallbackFireRotationKick = new Vector3(-4f, 0f, 0f);
    [SerializeField] private Vector3 fallbackRandomRotationKick = new Vector3(1f, 0.75f, 1.25f);

    [Header("Fallback Limits")]
    [SerializeField] private Vector3 fallbackMaxPositionOffset = new Vector3(0.08f, 0.08f, 0.18f);
    [SerializeField] private Vector3 fallbackMaxRotationOffset = new Vector3(12f, 6f, 8f);

    [Header("Fallback Recovery")]
    [SerializeField] private float fallbackPositionSnappiness = 24f;
    [SerializeField] private float fallbackRotationSnappiness = 24f;
    [SerializeField] private float fallbackPositionReturnSpeed = 12f;
    [SerializeField] private float fallbackRotationReturnSpeed = 12f;

    [Header("Fallback Look Sway")]
    [SerializeField] private bool fallbackUseLookSway = true;
    [SerializeField] private Vector3 fallbackLookSwayPositionAmount = new Vector3(0.015f, 0.015f, 0f);
    [SerializeField] private Vector3 fallbackLookSwayRotationAmount = new Vector3(1.5f, 2.5f, 1f);
    [SerializeField] private Vector3 fallbackMaxLookSwayPosition = new Vector3(0.06f, 0.06f, 0.02f);
    [SerializeField] private Vector3 fallbackMaxLookSwayRotation = new Vector3(6f, 8f, 5f);
    [SerializeField] private float fallbackLookSwaySnappiness = 14f;
    [SerializeField] private float fallbackLookSwayReturnSpeed = 10f;

    [Header("Fallback Movement Bob")]
    [SerializeField] private bool fallbackUseMovementBob = true;
    [SerializeField] private Vector3 fallbackMovementBobPositionAmount = new Vector3(0.025f, 0.018f, 0.01f);
    [SerializeField] private Vector3 fallbackMovementBobRotationAmount = new Vector3(1.2f, 0.8f, 1.6f);
    [SerializeField] private float fallbackMovementBobFrequency = 8f;
    [SerializeField] private float fallbackMovementBobSnappiness = 12f;
    [SerializeField] private float fallbackMovementBobReturnSpeed = 10f;
    [SerializeField] private float fallbackMovementInputThreshold = 0.1f;

    [Header("Fallback Idle Bob")]
    [SerializeField] private bool fallbackUseIdleBob = true;
    [SerializeField] private Vector3 fallbackIdleBobPositionAmount = new Vector3(0f, 0.006f, 0f);
    [SerializeField] private Vector3 fallbackIdleBobRotationAmount = new Vector3(0.25f, 0.15f, 0.15f);
    [SerializeField] private float fallbackIdleBobFrequency = 1.6f;
    [SerializeField] private float fallbackIdleBobSnappiness = 8f;

    [Header("Fallback Animator Triggers")]
    [SerializeField] private bool fallbackUseAnimatorTriggers = true;
    [SerializeField] private string fallbackFireTriggerName = "Fire";
    [SerializeField] private string fallbackReloadTriggerName = "Reload";
    [SerializeField] private string fallbackEquipTriggerName = "Equip";

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private Transform activeViewModel;
    private Transform activeMuzzlePoint;
    private Animator activeAnimator;
    private WeaponHandlingData activeHandlingData;

    private Vector3 baseLocalPosition;
    private Vector3 baseLocalEulerAngles;
    private Vector3 baseLocalScale;

    private Vector3 targetPositionOffset;
    private Vector3 targetRotationOffset;
    private Vector3 currentPositionOffset;
    private Vector3 currentRotationOffset;

    private Vector3 targetLookSwayPosition;
    private Vector3 targetLookSwayRotation;
    private Vector3 currentLookSwayPosition;
    private Vector3 currentLookSwayRotation;

    private Vector3 targetBobPosition;
    private Vector3 targetBobRotation;
    private Vector3 currentBobPosition;
    private Vector3 currentBobRotation;

    private float movementBobTimer;
    private float idleBobTimer;

    private void Awake()
    {
        if (inputReader == null)
            inputReader = GetComponent<PlayerInputReader>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (activeViewModel == null)
            return;

        float deltaTime = Time.deltaTime;

        UpdateFireKickback(deltaTime);
        UpdateLookSway(deltaTime);
        UpdateMovementBob(deltaTime);
        UpdateIdleBob(deltaTime);

        ApplyViewModelTransform();
    }

    public void SetActiveViewModel(Transform viewModelTransform, WeaponHandlingData handlingData)
    {
        activeViewModel = viewModelTransform;
        activeHandlingData = handlingData;

        activeAnimator = null;
        activeMuzzlePoint = null;

        ResetOffsets();

        if (activeViewModel == null)
            return;

        baseLocalPosition = activeViewModel.localPosition;
        baseLocalEulerAngles = activeViewModel.localEulerAngles;
        baseLocalScale = activeViewModel.localScale;

        activeAnimator = activeViewModel.GetComponentInChildren<Animator>(true);
        activeMuzzlePoint = FindMuzzlePoint();

        ApplyViewModelTransform();
        PlayEquipFeedback();

        if (debugLogs)
            Debug.Log($"View model controller linked to: {activeViewModel.name}");
    }

    public void ClearActiveViewModel()
    {
        activeViewModel = null;
        activeMuzzlePoint = null;
        activeAnimator = null;
        activeHandlingData = null;

        ResetOffsets();
    }

    public Vector3 PlayFireFeedback()
    {
        Vector3 appliedRotationKick = GetFireRotationKick() + GetRandomRotationKick();

        AddPositionKick(GetFirePositionKick());
        AddRotationKick(appliedRotationKick);

        TrySetAnimatorTrigger(GetFireTriggerName());
        SpawnMuzzleFlash();

        if (debugLogs)
            Debug.Log($"View model fire feedback played. Rotation kick: {appliedRotationKick}");

        return appliedRotationKick;
    }

    public void PlayReloadFeedback()
    {
        TrySetAnimatorTrigger(GetReloadTriggerName());

        if (debugLogs)
            Debug.Log("View model generic reload feedback played.");
    }

    public void PlayEquipFeedback()
    {
        TrySetAnimatorTrigger(GetEquipTriggerName());

        if (debugLogs)
            Debug.Log("View model equip feedback played.");
    }

    public void PlayDryFireFeedback()
    {
        TrySetAnimatorTrigger(GetDryFireTriggerName());

        if (debugLogs)
            Debug.Log("View model dry fire feedback played.");
    }

    public void PlayReloadStartFeedback()
    {
        TrySetAnimatorTrigger(GetReloadStartTriggerName());

        if (debugLogs)
            Debug.Log("View model reload start feedback played.");
    }

    public void PlayReloadInsertFeedback()
    {
        TrySetAnimatorTrigger(GetReloadInsertTriggerName());

        if (debugLogs)
            Debug.Log("View model reload insert feedback played.");
    }

    public void PlayReloadEndFeedback()
    {
        TrySetAnimatorTrigger(GetReloadEndTriggerName());

        if (debugLogs)
            Debug.Log("View model reload end feedback played.");
    }

    private void UpdateFireKickback(float deltaTime)
    {
        targetPositionOffset = Vector3.Lerp(
            targetPositionOffset,
            Vector3.zero,
            GetPositionReturnSpeed() * deltaTime);

        targetRotationOffset = Vector3.Lerp(
            targetRotationOffset,
            Vector3.zero,
            GetRotationReturnSpeed() * deltaTime);

        currentPositionOffset = Vector3.Lerp(
            currentPositionOffset,
            targetPositionOffset,
            GetPositionSnappiness() * deltaTime);

        currentRotationOffset = Vector3.Lerp(
            currentRotationOffset,
            targetRotationOffset,
            GetRotationSnappiness() * deltaTime);
    }

    private void UpdateLookSway(float deltaTime)
    {
        if (!ShouldUseLookSway() || inputReader == null)
        {
            targetLookSwayPosition = Vector3.Lerp(
                targetLookSwayPosition,
                Vector3.zero,
                GetLookSwayReturnSpeed() * deltaTime);

            targetLookSwayRotation = Vector3.Lerp(
                targetLookSwayRotation,
                Vector3.zero,
                GetLookSwayReturnSpeed() * deltaTime);
        }
        else
        {
            Vector2 lookInput = inputReader.LookInput;

            Vector3 positionAmount = GetLookSwayPositionAmount();
            Vector3 rotationAmount = GetLookSwayRotationAmount();

            targetLookSwayPosition = new Vector3(
                -lookInput.x * positionAmount.x,
                -lookInput.y * positionAmount.y,
                0f);

            targetLookSwayRotation = new Vector3(
                lookInput.y * rotationAmount.x,
                -lookInput.x * rotationAmount.y,
                lookInput.x * rotationAmount.z);

            targetLookSwayPosition = ClampVector(targetLookSwayPosition, GetMaxLookSwayPosition());
            targetLookSwayRotation = ClampVector(targetLookSwayRotation, GetMaxLookSwayRotation());
        }

        currentLookSwayPosition = Vector3.Lerp(
            currentLookSwayPosition,
            targetLookSwayPosition,
            GetLookSwaySnappiness() * deltaTime);

        currentLookSwayRotation = Vector3.Lerp(
            currentLookSwayRotation,
            targetLookSwayRotation,
            GetLookSwaySnappiness() * deltaTime);
    }

    private void UpdateMovementBob(float deltaTime)
    {
        if (!ShouldUseMovementBob())
        {
            ReturnBobToZero(deltaTime);
            return;
        }

        float movementAmount = GetMovementAmount();

        if (movementAmount <= GetMovementInputThreshold())
        {
            ReturnBobToZero(deltaTime);
            return;
        }

        movementBobTimer += deltaTime * GetMovementBobFrequency() * Mathf.Clamp01(movementAmount);

        float horizontalWave = Mathf.Sin(movementBobTimer);
        float verticalWave = Mathf.Abs(Mathf.Cos(movementBobTimer));

        Vector3 positionAmount = GetMovementBobPositionAmount();
        Vector3 rotationAmount = GetMovementBobRotationAmount();

        targetBobPosition = new Vector3(
            horizontalWave * positionAmount.x,
            verticalWave * positionAmount.y,
            horizontalWave * positionAmount.z);

        targetBobRotation = new Vector3(
            verticalWave * rotationAmount.x,
            horizontalWave * rotationAmount.y,
            horizontalWave * rotationAmount.z);

        currentBobPosition = Vector3.Lerp(
            currentBobPosition,
            targetBobPosition,
            GetMovementBobSnappiness() * deltaTime);

        currentBobRotation = Vector3.Lerp(
            currentBobRotation,
            targetBobRotation,
            GetMovementBobSnappiness() * deltaTime);
    }

    private void UpdateIdleBob(float deltaTime)
    {
        if (!ShouldUseIdleBob())
            return;

        if (GetMovementAmount() > GetMovementInputThreshold())
            return;

        idleBobTimer += deltaTime * GetIdleBobFrequency();

        float wave = Mathf.Sin(idleBobTimer);

        Vector3 idlePosition = GetIdleBobPositionAmount() * wave;
        Vector3 idleRotation = GetIdleBobRotationAmount() * wave;

        currentBobPosition = Vector3.Lerp(
            currentBobPosition,
            idlePosition,
            GetIdleBobSnappiness() * deltaTime);

        currentBobRotation = Vector3.Lerp(
            currentBobRotation,
            idleRotation,
            GetIdleBobSnappiness() * deltaTime);
    }

    private void ReturnBobToZero(float deltaTime)
    {
        targetBobPosition = Vector3.zero;
        targetBobRotation = Vector3.zero;

        currentBobPosition = Vector3.Lerp(
            currentBobPosition,
            Vector3.zero,
            GetMovementBobReturnSpeed() * deltaTime);

        currentBobRotation = Vector3.Lerp(
            currentBobRotation,
            Vector3.zero,
            GetMovementBobReturnSpeed() * deltaTime);
    }

    private void ApplyViewModelTransform()
    {
        if (activeViewModel == null)
            return;

        Vector3 finalPosition =
            baseLocalPosition +
            currentPositionOffset +
            currentLookSwayPosition +
            currentBobPosition;

        Vector3 finalRotation =
            baseLocalEulerAngles +
            currentRotationOffset +
            currentLookSwayRotation +
            currentBobRotation;

        activeViewModel.localPosition = finalPosition;
        activeViewModel.localRotation = Quaternion.Euler(finalRotation);
        activeViewModel.localScale = baseLocalScale;
    }

    private void AddPositionKick(Vector3 kick)
    {
        targetPositionOffset += kick;
        targetPositionOffset = ClampVector(targetPositionOffset, GetMaxPositionOffset());
    }

    private void AddRotationKick(Vector3 kick)
    {
        targetRotationOffset += kick;
        targetRotationOffset = ClampVector(targetRotationOffset, GetMaxRotationOffset());
    }

    private Vector3 ClampVector(Vector3 value, Vector3 maxAbs)
    {
        value.x = Mathf.Clamp(value.x, -Mathf.Abs(maxAbs.x), Mathf.Abs(maxAbs.x));
        value.y = Mathf.Clamp(value.y, -Mathf.Abs(maxAbs.y), Mathf.Abs(maxAbs.y));
        value.z = Mathf.Clamp(value.z, -Mathf.Abs(maxAbs.z), Mathf.Abs(maxAbs.z));

        return value;
    }

    private Vector3 GetRandomRotationKick()
    {
        Vector3 random = GetRandomRotationKickRange();

        return new Vector3(
            Random.Range(-random.x, random.x),
            Random.Range(-random.y, random.y),
            Random.Range(-random.z, random.z));
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

    private void ResetOffsets()
    {
        targetPositionOffset = Vector3.zero;
        targetRotationOffset = Vector3.zero;
        currentPositionOffset = Vector3.zero;
        currentRotationOffset = Vector3.zero;

        targetLookSwayPosition = Vector3.zero;
        targetLookSwayRotation = Vector3.zero;
        currentLookSwayPosition = Vector3.zero;
        currentLookSwayRotation = Vector3.zero;

        targetBobPosition = Vector3.zero;
        targetBobRotation = Vector3.zero;
        currentBobPosition = Vector3.zero;
        currentBobRotation = Vector3.zero;

        movementBobTimer = 0f;
        idleBobTimer = 0f;
    }

    private Transform FindMuzzlePoint()
    {
        if (activeViewModel == null)
            return null;

        string muzzleName = GetMuzzlePointName();

        if (string.IsNullOrWhiteSpace(muzzleName))
            return null;

        Transform[] children = activeViewModel.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == muzzleName)
                return child;
        }

        return null;
    }

    private void SpawnMuzzleFlash()
    {
        GameObject prefab = GetMuzzleFlashPrefab();

        if (prefab == null || activeMuzzlePoint == null)
            return;

        Vector3 localPositionOffset = GetMuzzleFlashLocalPositionOffset();
        Vector3 localEulerOffset = GetMuzzleFlashLocalEulerOffset();

        Vector3 spawnPosition =
            activeMuzzlePoint.position +
            activeMuzzlePoint.TransformDirection(localPositionOffset);

        Quaternion spawnRotation =
            activeMuzzlePoint.rotation *
            Quaternion.Euler(localEulerOffset);

        Transform parent = ShouldParentMuzzleFlashToMuzzle()
            ? activeMuzzlePoint
            : null;

        GameObject flash = Instantiate(prefab, spawnPosition, spawnRotation, parent);

        float lifetime = GetMuzzleFlashLifetime();

        if (lifetime > 0f)
            Destroy(flash, lifetime);
    }

    private void TrySetAnimatorTrigger(string triggerName)
    {
        if (!ShouldUseAnimatorTriggers())
            return;

        if (activeAnimator == null)
            return;

        if (string.IsNullOrWhiteSpace(triggerName))
            return;

        activeAnimator.SetTrigger(triggerName);
    }

    private Vector3 GetFirePositionKick()
    {
        return activeHandlingData != null
            ? activeHandlingData.firePositionKick
            : fallbackFirePositionKick;
    }

    private Vector3 GetFireRotationKick()
    {
        return activeHandlingData != null
            ? activeHandlingData.fireRotationKick
            : fallbackFireRotationKick;
    }

    private Vector3 GetRandomRotationKickRange()
    {
        return activeHandlingData != null
            ? activeHandlingData.randomRotationKick
            : fallbackRandomRotationKick;
    }

    private Vector3 GetMaxPositionOffset()
    {
        return activeHandlingData != null
            ? activeHandlingData.maxPositionOffset
            : fallbackMaxPositionOffset;
    }

    private Vector3 GetMaxRotationOffset()
    {
        return activeHandlingData != null
            ? activeHandlingData.maxRotationOffset
            : fallbackMaxRotationOffset;
    }

    private float GetPositionSnappiness()
    {
        return activeHandlingData != null
            ? Mathf.Max(0.01f, activeHandlingData.positionSnappiness)
            : Mathf.Max(0.01f, fallbackPositionSnappiness);
    }

    private float GetRotationSnappiness()
    {
        return activeHandlingData != null
            ? Mathf.Max(0.01f, activeHandlingData.rotationSnappiness)
            : Mathf.Max(0.01f, fallbackRotationSnappiness);
    }

    private float GetPositionReturnSpeed()
    {
        return activeHandlingData != null
            ? Mathf.Max(0.01f, activeHandlingData.positionReturnSpeed)
            : Mathf.Max(0.01f, fallbackPositionReturnSpeed);
    }

    private float GetRotationReturnSpeed()
    {
        return activeHandlingData != null
            ? Mathf.Max(0.01f, activeHandlingData.rotationReturnSpeed)
            : Mathf.Max(0.01f, fallbackRotationReturnSpeed);
    }

    private bool ShouldUseLookSway()
    {
        return activeHandlingData != null
            ? activeHandlingData.useLookSway
            : fallbackUseLookSway;
    }

    private Vector3 GetLookSwayPositionAmount()
    {
        return activeHandlingData != null
            ? activeHandlingData.lookSwayPositionAmount
            : fallbackLookSwayPositionAmount;
    }

    private Vector3 GetLookSwayRotationAmount()
    {
        return activeHandlingData != null
            ? activeHandlingData.lookSwayRotationAmount
            : fallbackLookSwayRotationAmount;
    }

    private Vector3 GetMaxLookSwayPosition()
    {
        return activeHandlingData != null
            ? activeHandlingData.maxLookSwayPosition
            : fallbackMaxLookSwayPosition;
    }

    private Vector3 GetMaxLookSwayRotation()
    {
        return activeHandlingData != null
            ? activeHandlingData.maxLookSwayRotation
            : fallbackMaxLookSwayRotation;
    }

    private float GetLookSwaySnappiness()
    {
        return activeHandlingData != null
            ? Mathf.Max(0.01f, activeHandlingData.lookSwaySnappiness)
            : Mathf.Max(0.01f, fallbackLookSwaySnappiness);
    }

    private float GetLookSwayReturnSpeed()
    {
        return activeHandlingData != null
            ? Mathf.Max(0.01f, activeHandlingData.lookSwayReturnSpeed)
            : Mathf.Max(0.01f, fallbackLookSwayReturnSpeed);
    }

    private bool ShouldUseMovementBob()
    {
        return activeHandlingData != null
            ? activeHandlingData.useMovementBob
            : fallbackUseMovementBob;
    }

    private Vector3 GetMovementBobPositionAmount()
    {
        return activeHandlingData != null
            ? activeHandlingData.movementBobPositionAmount
            : fallbackMovementBobPositionAmount;
    }

    private Vector3 GetMovementBobRotationAmount()
    {
        return activeHandlingData != null
            ? activeHandlingData.movementBobRotationAmount
            : fallbackMovementBobRotationAmount;
    }

    private float GetMovementBobFrequency()
    {
        return activeHandlingData != null
            ? Mathf.Max(0.01f, activeHandlingData.movementBobFrequency)
            : Mathf.Max(0.01f, fallbackMovementBobFrequency);
    }

    private float GetMovementBobSnappiness()
    {
        return activeHandlingData != null
            ? Mathf.Max(0.01f, activeHandlingData.movementBobSnappiness)
            : Mathf.Max(0.01f, fallbackMovementBobSnappiness);
    }

    private float GetMovementBobReturnSpeed()
    {
        return activeHandlingData != null
            ? Mathf.Max(0.01f, activeHandlingData.movementBobReturnSpeed)
            : Mathf.Max(0.01f, fallbackMovementBobReturnSpeed);
    }

    private float GetMovementInputThreshold()
    {
        return activeHandlingData != null
            ? Mathf.Max(0f, activeHandlingData.movementInputThreshold)
            : Mathf.Max(0f, fallbackMovementInputThreshold);
    }

    private bool ShouldUseIdleBob()
    {
        return activeHandlingData != null
            ? activeHandlingData.useIdleBob
            : fallbackUseIdleBob;
    }

    private Vector3 GetIdleBobPositionAmount()
    {
        return activeHandlingData != null
            ? activeHandlingData.idleBobPositionAmount
            : fallbackIdleBobPositionAmount;
    }

    private Vector3 GetIdleBobRotationAmount()
    {
        return activeHandlingData != null
            ? activeHandlingData.idleBobRotationAmount
            : fallbackIdleBobRotationAmount;
    }

    private float GetIdleBobFrequency()
    {
        return activeHandlingData != null
            ? Mathf.Max(0.01f, activeHandlingData.idleBobFrequency)
            : Mathf.Max(0.01f, fallbackIdleBobFrequency);
    }

    private float GetIdleBobSnappiness()
    {
        return activeHandlingData != null
            ? Mathf.Max(0.01f, activeHandlingData.idleBobSnappiness)
            : Mathf.Max(0.01f, fallbackIdleBobSnappiness);
    }

    private bool ShouldUseAnimatorTriggers()
    {
        return activeHandlingData != null
            ? activeHandlingData.useAnimatorTriggers
            : fallbackUseAnimatorTriggers;
    }

    private string GetFireTriggerName()
    {
        return activeHandlingData != null
            ? activeHandlingData.fireTriggerName
            : fallbackFireTriggerName;
    }

    private string GetReloadTriggerName()
    {
        return activeHandlingData != null
            ? activeHandlingData.reloadTriggerName
            : fallbackReloadTriggerName;
    }

    private string GetEquipTriggerName()
    {
        return activeHandlingData != null
            ? activeHandlingData.equipTriggerName
            : fallbackEquipTriggerName;
    }

    private string GetReloadStartTriggerName()
    {
        return activeHandlingData != null
            ? activeHandlingData.reloadStartTriggerName
            : "ReloadStart";
    }

    private string GetReloadInsertTriggerName()
    {
        return activeHandlingData != null
            ? activeHandlingData.reloadInsertTriggerName
            : "ReloadInsert";
    }

    private string GetReloadEndTriggerName()
    {
        return activeHandlingData != null
            ? activeHandlingData.reloadEndTriggerName
            : "ReloadEnd";
    }

    private string GetDryFireTriggerName()
    {
        return activeHandlingData != null
            ? activeHandlingData.dryFireTriggerName
            : "DryFire";
    }

    private GameObject GetMuzzleFlashPrefab()
    {
        return activeHandlingData != null
            ? activeHandlingData.muzzleFlashPrefab
            : null;
    }

    private string GetMuzzlePointName()
    {
        return activeHandlingData != null
            ? activeHandlingData.muzzlePointName
            : "MuzzlePoint";
    }

    private Vector3 GetMuzzleFlashLocalPositionOffset()
    {
        return activeHandlingData != null
            ? activeHandlingData.muzzleFlashLocalPositionOffset
            : Vector3.zero;
    }

    private Vector3 GetMuzzleFlashLocalEulerOffset()
    {
        return activeHandlingData != null
            ? activeHandlingData.muzzleFlashLocalEulerOffset
            : Vector3.zero;
    }

    private float GetMuzzleFlashLifetime()
    {
        return activeHandlingData != null
            ? Mathf.Max(0f, activeHandlingData.muzzleFlashLifetime)
            : 0.08f;
    }

    private bool ShouldParentMuzzleFlashToMuzzle()
    {
        return activeHandlingData == null || activeHandlingData.parentMuzzleFlashToMuzzle;
    }
}

//-----PlayerWeaponViewModelController.cs END-----