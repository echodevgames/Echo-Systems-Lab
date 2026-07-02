//-----TargetRangeTargetGroup.cs START-----

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetRangeTargetGroup : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string targetGroupId = "pistol_targets";

    [Header("Target Slots")]
    [SerializeField] private TargetHealth[] targetSlots;

    [Header("Inactive Preview")]
    [SerializeField] private bool showTargetsBeforeMission = false;
    [SerializeField] private bool previewTargetsAreShootable = false;

    private readonly List<TargetRangeMissionTarget> allMissionTargets = new List<TargetRangeMissionTarget>();
    private readonly List<TargetRangeMissionTarget> activeTargets = new List<TargetRangeMissionTarget>();
    private readonly List<Coroutine> respawnRoutines = new List<Coroutine>();

    private TargetRangeMissionController activeController;
    private TargetRangeMissionData activeMission;

    private bool groupActive;

    public string TargetGroupId => targetGroupId;

    private void Awake()
    {
        PrepareTargetSlots();
        ApplyInactiveVisibility();
    }

    public void ActivateGroup(TargetRangeMissionController controller, TargetRangeMissionData mission)
    {
        activeController = controller;
        activeMission = mission;
        groupActive = true;

        PrepareTargetSlots();
        StopRespawnRoutines();
        HideAllTargets();

        activeTargets.Clear();

        int startingTargetCount = Mathf.Min(GetMaxActiveTargets(), allMissionTargets.Count);

        for (int i = 0; i < startingTargetCount; i++)
            ActivateNextTarget();

        Debug.Log($"Activated target group: {targetGroupId} with {startingTargetCount}/{allMissionTargets.Count} active targets.");
    }

    public void DeactivateGroup()
    {
        groupActive = false;

        StopRespawnRoutines();
        activeTargets.Clear();

        ApplyInactiveVisibility();

        activeController = null;
        activeMission = null;

        Debug.Log($"Deactivated target group: {targetGroupId}");
    }

    public void HandleTargetDestroyed(
        TargetRangeMissionTarget missionTarget,
        TargetHealth targetHealth,
        DamageInfo damageInfo)
    {
        if (missionTarget != null)
            activeTargets.Remove(missionTarget);

        TargetRangeMissionController controller = activeController;

        if (controller != null)
            controller.RegisterMissionTargetDestroyed(targetHealth, damageInfo);

        if (!groupActive)
            return;

        if (controller == null || !controller.IsMissionRunning)
            return;

        Coroutine routine = StartCoroutine(RespawnAfterDelay());
        respawnRoutines.Add(routine);
    }

    private IEnumerator RespawnAfterDelay()
    {
        float delay = GetRespawnDelay();

        yield return new WaitForSeconds(delay);

        if (!groupActive)
            yield break;

        if (activeController == null || !activeController.IsMissionRunning)
            yield break;

        if (activeTargets.Count < GetMaxActiveTargets())
            ActivateNextTarget();
    }

    private void PrepareTargetSlots()
    {
        allMissionTargets.Clear();

        if (targetSlots == null)
            return;

        foreach (TargetHealth target in targetSlots)
        {
            if (target == null)
                continue;

            TargetRangeMissionTarget missionTarget =
                target.GetComponent<TargetRangeMissionTarget>();

            if (missionTarget == null)
                missionTarget = target.gameObject.AddComponent<TargetRangeMissionTarget>();

            missionTarget.Initialize(this);

            if (!allMissionTargets.Contains(missionTarget))
                allMissionTargets.Add(missionTarget);
        }
    }

    private void ActivateNextTarget()
    {
        List<TargetRangeMissionTarget> availableTargets = GetAvailableTargets();

        if (availableTargets.Count <= 0)
        {
            Debug.LogWarning($"{name} has no available inactive targets to activate.");
            return;
        }

        int index = Random.Range(0, availableTargets.Count);
        TargetRangeMissionTarget targetToActivate = availableTargets[index];

        TargetHealth targetHealth = targetToActivate.GetComponent<TargetHealth>();

        if (targetHealth != null)
        {
            targetHealth.ResetTarget();
            SetTargetColliders(targetHealth, true);
        }
        else
        {
            targetToActivate.gameObject.SetActive(true);
        }

        activeTargets.Add(targetToActivate);

        Debug.Log($"Activated mission target slot: {targetToActivate.name}");
    }

    private List<TargetRangeMissionTarget> GetAvailableTargets()
    {
        List<TargetRangeMissionTarget> availableTargets = new List<TargetRangeMissionTarget>();

        foreach (TargetRangeMissionTarget missionTarget in allMissionTargets)
        {
            if (missionTarget == null)
                continue;

            if (activeTargets.Contains(missionTarget))
                continue;

            if (missionTarget.gameObject.activeSelf)
                continue;

            availableTargets.Add(missionTarget);
        }

        return availableTargets;
    }

    private void HideAllTargets()
    {
        foreach (TargetRangeMissionTarget missionTarget in allMissionTargets)
        {
            if (missionTarget == null)
                continue;

            missionTarget.gameObject.SetActive(false);
        }
    }

    private void ApplyInactiveVisibility()
    {
        foreach (TargetRangeMissionTarget missionTarget in allMissionTargets)
        {
            if (missionTarget == null)
                continue;

            TargetHealth targetHealth = missionTarget.GetComponent<TargetHealth>();

            if (targetHealth == null)
            {
                missionTarget.gameObject.SetActive(showTargetsBeforeMission);
                continue;
            }

            if (showTargetsBeforeMission)
            {
                targetHealth.ResetTarget();
                SetTargetColliders(targetHealth, previewTargetsAreShootable);
            }
            else
            {
                missionTarget.gameObject.SetActive(false);
            }
        }
    }

    private void SetTargetColliders(TargetHealth targetHealth, bool enabled)
    {
        if (targetHealth == null)
            return;

        Collider[] colliders = targetHealth.GetComponentsInChildren<Collider>(true);

        foreach (Collider collider in colliders)
            collider.enabled = enabled;
    }

    private int GetMaxActiveTargets()
    {
        if (activeMission == null)
            return 1;

        return Mathf.Max(1, activeMission.maxActiveTargets);
    }

    private float GetRespawnDelay()
    {
        if (activeMission == null)
            return 2f;

        return Mathf.Max(0f, activeMission.targetRespawnDelay);
    }

    private void StopRespawnRoutines()
    {
        foreach (Coroutine routine in respawnRoutines)
        {
            if (routine != null)
                StopCoroutine(routine);
        }

        respawnRoutines.Clear();
    }
}

//-----TargetRangeTargetGroup.cs END-----