//-----TargetRangeMissionTarget.cs START-----

using UnityEngine;

public class TargetRangeMissionTarget : MonoBehaviour
{
    private TargetRangeTargetGroup owningGroup;
    private TargetHealth targetHealth;
    private bool isActiveInMission;

    public TargetRangeTargetGroup OwningGroup => owningGroup;
    public TargetHealth TargetHealth => targetHealth;
    public bool IsActiveInMission => isActiveInMission;

    public void Initialize(TargetRangeTargetGroup group, TargetHealth health)
    {
        owningGroup = group;
        targetHealth = health;

        if (targetHealth == null)
            targetHealth = GetComponent<TargetHealth>();

        if (targetHealth == null)
            targetHealth = GetComponentInChildren<TargetHealth>(true);

        if (targetHealth != null)
            targetHealth.SetMissionTarget(this);
        else
            Debug.LogWarning($"{name} has no TargetHealth assigned or found.");
    }

    public void ActivateForMission()
    {
        gameObject.SetActive(true);

        isActiveInMission = true;

        if (targetHealth != null)
            targetHealth.ResetTarget();

        SetRenderersEnabled(true);
        SetCollidersEnabled(true);

        Debug.Log($"Mission target activated: {name}");
    }

    public void HideForMission(bool hideVisual, bool disableColliders)
    {
        isActiveInMission = false;

        if (disableColliders)
            SetCollidersEnabled(false);

        if (hideVisual)
            SetRenderersEnabled(false);

        Debug.Log($"Mission target hidden: {name}");
    }

    public void ResetToInactivePreview(bool visible, bool shootable)
    {
        gameObject.SetActive(true);

        isActiveInMission = false;

        if (targetHealth != null)
        {
            targetHealth.ClearSpawnedDestroyEffect();
            targetHealth.ResetTarget();
        }

        SetRenderersEnabled(visible);
        SetCollidersEnabled(visible && shootable);
    }

    public void PreserveDestroyedState(bool hideVisual, bool disableColliders)
    {
        isActiveInMission = false;

        if (disableColliders)
            SetCollidersEnabled(false);

        if (hideVisual)
            SetRenderersEnabled(false);

        Debug.Log($"Mission target preserved after completion: {name}");
    }

    public void NotifyDestroyed(TargetHealth destroyedTargetHealth, DamageInfo damageInfo)
    {
        Debug.Log($"Mission target notified destroyed: {name}");

        if (owningGroup == null)
        {
            Debug.LogWarning($"{name} was destroyed, but it has no owning TargetRangeTargetGroup.");
            return;
        }

        owningGroup.HandleTargetDestroyed(this, destroyedTargetHealth, damageInfo);
    }

    private void SetRenderersEnabled(bool enabled)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer targetRenderer in renderers)
            targetRenderer.enabled = enabled;
    }

    private void SetCollidersEnabled(bool enabled)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);

        foreach (Collider targetCollider in colliders)
            targetCollider.enabled = enabled;
    }
}

//-----TargetRangeMissionTarget.cs END-----