//-----TargetRangeMissionController.cs START-----

using System;
using UnityEngine;

public class TargetRangeMissionController : MonoBehaviour
{
    public static TargetRangeMissionController Instance { get; private set; }

    public event Action OnMissionStateChanged;
    public event Action<TargetRangeMissionData> OnMissionStarted;
    public event Action<TargetRangeMissionData, bool> OnMissionEnded;

    [Header("Trial Completion")]
    [SerializeField] private string targetRangeTrialMissionId = "TargetRangeTrial";
    [SerializeField] private TargetRangeMissionData[] missionsRequiredToCompleteTrial;

    [Header("Runtime Debug")]
    [SerializeField] private TargetRangeMissionData activeMission;
    [SerializeField] private bool missionRunning;
    [SerializeField] private float timeRemaining;
    [SerializeField] private int destroyedTargetsThisMission;

    public TargetRangeMissionData ActiveMission => activeMission;
    public bool MissionRunning => missionRunning;
    public float TimeRemaining => timeRemaining;
    public int DestroyedTargetsThisMission => destroyedTargetsThisMission;

    public int RequiredDestroyedTargets
    {
        get
        {
            if (activeMission == null)
                return 0;

            return activeMission.requiredDestroyedTargets;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (!missionRunning)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndMission(false);
        }

        NotifyMissionStateChanged();
    }

    public void StartMission(TargetRangeMissionData mission)
    {
        if (mission == null)
        {
            Debug.LogWarning("Tried to start a null TargetRangeMissionData.");
            return;
        }

        activeMission = mission;
        missionRunning = true;
        timeRemaining = Mathf.Max(1f, activeMission.timeLimitSeconds);
        destroyedTargetsThisMission = 0;

        Debug.Log($"Started target range mission: {activeMission.displayName}");
        Debug.Log($"Goal: Destroy {activeMission.requiredDestroyedTargets} targets in {activeMission.timeLimitSeconds} seconds.");

        OnMissionStarted?.Invoke(activeMission);
        NotifyMissionStateChanged();
    }

    public void RegisterMissionTargetDestroyed(TargetHealth target, DamageInfo damageInfo)
    {
        if (!missionRunning)
            return;

        destroyedTargetsThisMission++;

        Debug.Log($"Mission target destroyed: {destroyedTargetsThisMission}/{activeMission.requiredDestroyedTargets}");

        if (destroyedTargetsThisMission >= activeMission.requiredDestroyedTargets)
            EndMission(true);

        NotifyMissionStateChanged();
    }

    private void EndMission(bool passed)
    {
        if (!missionRunning)
            return;

        missionRunning = false;

        if (passed)
        {
            MissionProgress.MarkCompleted(activeMission.missionId);

            Debug.Log($"Target range mission passed: {activeMission.displayName}");

            CheckTargetRangeTrialCompletion();
        }
        else
        {
            Debug.Log($"Target range mission failed: {activeMission.displayName}");
        }

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();

        OnMissionEnded?.Invoke(activeMission, passed);
        NotifyMissionStateChanged();
    }

    private void CheckTargetRangeTrialCompletion()
    {
        if (missionsRequiredToCompleteTrial == null || missionsRequiredToCompleteTrial.Length == 0)
            return;

        foreach (TargetRangeMissionData mission in missionsRequiredToCompleteTrial)
        {
            if (mission == null)
                continue;

            if (!mission.countsTowardTargetRangeTrialCompletion)
                continue;

            if (!MissionProgress.IsCompleted(mission.missionId))
                return;
        }

        MissionProgress.MarkCompleted(targetRangeTrialMissionId);

        if (TargetRangeTracker.Instance != null)
            TargetRangeTracker.Instance.CompleteTrialFromMissionSystem();

        Debug.Log($"All required target range missions complete. Marked {targetRangeTrialMissionId} complete.");
    }

    private void NotifyMissionStateChanged()
    {
        OnMissionStateChanged?.Invoke();
    }
}

//-----TargetRangeMissionController.cs END-----