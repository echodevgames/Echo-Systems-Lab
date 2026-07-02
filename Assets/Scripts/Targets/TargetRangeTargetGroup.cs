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

        activeTargets.Clear();
        HideAllTargets();

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
        if (missionTarget == null)
        {
            Debug.LogWarning($"{name} received destroyed target event with null missionTarget.");
            return;
        }

        Debug.Log($"{name} received target destroyed event from {missionTarget.name}.");

        if (activeTargets.Contains(missionTarget))
            activeTargets.Remove(missionTarget);

        TargetRangeMissionController controller = activeController;

        if (controller == null)
        {
            Debug.LogWarning($"{name} has no active TargetRangeMissionController while target was destroyed.");
            missionTarget.HideForMission();
            return;
        }

        if (!controller.IsMissionRunning)
        {
            Debug.LogWarning($"{name} received destroyed target event, but mission is not running.");
            missionTarget.HideForMission();
            return;
        }

        controller.RegisterMissionTargetDestroyed(targetHealth, damageInfo);

        missionTarget.HideForMission();

        if (!groupActive)
            return;

        Coroutine routine = StartCoroutine(RespawnAfterDelay());
        respawnRoutines.Add(routine);
    }

    private IEnumerator RespawnAfterDelay()
    {
        float delay = GetRespawnDelay();

        Debug.Log($"Respawning target from group {targetGroupId} in {delay:0.00} seconds.");

        yield return new WaitForSeconds(delay);

        if (!groupActive)
        {
            Debug.Log($"Respawn cancelled because group {targetGroupId} is inactive.");
            yield break;
        }

        if (activeController == null || !activeController.IsMissionRunning)
        {
            Debug.Log($"Respawn cancelled because mission is no longer running.");
            yield break;
        }

        if (activeTargets.Count >= GetMaxActiveTargets())
        {
            Debug.Log($"Respawn skipped because active target cap is already full.");
            yield break;
        }

        ActivateNextTarget();
    }

    private void PrepareTargetSlots()
    {
        allMissionTargets.Clear();

        if (targetSlots == null || targetSlots.Length == 0)
        {
            Debug.LogWarning($"{name} has no target slots assigned.");
            return;
        }

        foreach (TargetHealth target in targetSlots)
        {
            if (target == null)
                continue;

            GameObject targetObject = target.gameObject;
            targetObject.SetActive(true);

            TargetRangeMissionTarget missionTarget =
                targetObject.GetComponent<TargetRangeMissionTarget>();

            if (missionTarget == null)
                missionTarget = targetObject.AddComponent<TargetRangeMissionTarget>();

            missionTarget.Initialize(this, target);

            if (!allMissionTargets.Contains(missionTarget))
                allMissionTargets.Add(missionTarget);
        }

        Debug.Log($"{name} prepared {allMissionTargets.Count} target slot(s).");
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

        targetToActivate.ActivateForMission();

        if (!activeTargets.Contains(targetToActivate))
            activeTargets.Add(targetToActivate);

        Debug.Log($"Activated mission target slot: {targetToActivate.name}. Active count: {activeTargets.Count}/{GetMaxActiveTargets()}");
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

            if (missionTarget.IsActiveInMission)
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

            missionTarget.HideForMission();
        }
    }

    private void ApplyInactiveVisibility()
    {
        foreach (TargetRangeMissionTarget missionTarget in allMissionTargets)
        {
            if (missionTarget == null)
                continue;

            missionTarget.SetPreviewState(showTargetsBeforeMission, previewTargetsAreShootable);
        }
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