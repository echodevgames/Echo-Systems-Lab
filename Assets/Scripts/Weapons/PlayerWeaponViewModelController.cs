//-----PlayerWeaponViewModelController.cs START-----

using UnityEngine;

public class PlayerWeaponViewModelController : MonoBehaviour
{
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

    [Header("Fallback Animator Triggers")]
    [SerializeField] private bool fallbackUseAnimatorTriggers = true;
    [SerializeField] private string fallbackFireTriggerName = "Fire";
    [SerializeField] private string fallbackReloadTriggerName = "Reload";
    [SerializeField] private string fallbackEquipTriggerName = "Equip";

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private Transform activeViewModel;
    private Animator activeAnimator;
    private WeaponHandlingData activeHandlingData;

    private Vector3 baseLocalPosition;
    private Vector3 baseLocalEulerAngles;
    private Vector3 baseLocalScale;

    private Vector3 targetPositionOffset;
    private Vector3 targetRotationOffset;

    private Vector3 currentPositionOffset;
    private Vector3 currentRotationOffset;

    private void Update()
    {
        if (activeViewModel == null)
            return;

        float deltaTime = Time.deltaTime;

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

        ApplyViewModelTransform();
    }

    public void SetActiveViewModel(Transform viewModelTransform, WeaponHandlingData handlingData)
    {
        activeViewModel = viewModelTransform;
        activeHandlingData = handlingData;

        activeAnimator = null;

        targetPositionOffset = Vector3.zero;
        targetRotationOffset = Vector3.zero;
        currentPositionOffset = Vector3.zero;
        currentRotationOffset = Vector3.zero;

        if (activeViewModel == null)
            return;

        baseLocalPosition = activeViewModel.localPosition;
        baseLocalEulerAngles = activeViewModel.localEulerAngles;
        baseLocalScale = activeViewModel.localScale;

        activeAnimator = activeViewModel.GetComponentInChildren<Animator>(true);

        ApplyViewModelTransform();
        PlayEquipFeedback();

        if (debugLogs)
            Debug.Log($"View model controller linked to: {activeViewModel.name}");
    }

    public void ClearActiveViewModel()
    {
        activeViewModel = null;
        activeAnimator = null;
        activeHandlingData = null;

        targetPositionOffset = Vector3.zero;
        targetRotationOffset = Vector3.zero;
        currentPositionOffset = Vector3.zero;
        currentRotationOffset = Vector3.zero;
    }

    public void PlayFireFeedback()
    {
        AddPositionKick(GetFirePositionKick());
        AddRotationKick(GetFireRotationKick() + GetRandomRotationKick());

        TrySetAnimatorTrigger(GetFireTriggerName());

        if (debugLogs)
            Debug.Log("View model fire feedback played.");
    }

    public void PlayReloadFeedback()
    {
        TrySetAnimatorTrigger(GetReloadTriggerName());

        if (debugLogs)
            Debug.Log("View model reload feedback played.");
    }

    public void PlayEquipFeedback()
    {
        TrySetAnimatorTrigger(GetEquipTriggerName());

        if (debugLogs)
            Debug.Log("View model equip feedback played.");
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

    private void ApplyViewModelTransform()
    {
        if (activeViewModel == null)
            return;

        activeViewModel.localPosition = baseLocalPosition + currentPositionOffset;
        activeViewModel.localRotation = Quaternion.Euler(baseLocalEulerAngles + currentRotationOffset);
        activeViewModel.localScale = baseLocalScale;
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
}

//-----PlayerWeaponViewModelController.cs END-----