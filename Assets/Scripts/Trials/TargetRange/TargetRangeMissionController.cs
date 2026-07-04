//-----TargetRangeMissionController.cs START-----

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TargetRangeMissionRuntimeState
{
    None,
    Armed,
    Running,
    Completed,
    Failed
}

public class TargetRangeMissionController : MonoBehaviour
{
    public static TargetRangeMissionController Instance { get; private set; }

    public event Action OnMissionStateChanged;
    public event Action OnMissionCompleted;
    public event Action OnMissionFailed;
    public event Action OnTargetTrialCompleted;

    [Header("Trial Completion")]
    [SerializeField] private string targetRangeTrialMissionId = "target_range_trial";
    [SerializeField] private MissionData[] requiredMissionsForTrialCompletion;

    [Header("Scenes")]
    [SerializeField] private string hubSceneName = "Hub";

    [Header("Return To Hub")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private bool allowReloadInputReturnToHubAfterTrialComplete = true;

    [Header("Optional Player Score")]
    [SerializeField] private bool addMissionScoreToPlayerStats = true;
    [SerializeField] private PlayerStatsController playerStatsController;

    [Header("Runtime Debug")]
    [SerializeField] private TargetRangeMissionData activeMission;
    [SerializeField] private TargetRangeMissionRuntimeState missionState;
    [SerializeField] private bool targetRangeTrialCompleted;
    [SerializeField] private float remainingTime;
    [SerializeField] private int destroyedTargetCount;
    [SerializeField] private int missionScore;
    [SerializeField] private int shotsFired;
    [SerializeField] private int hitsLanded;
    [SerializeField] private string currentWeaponId;
    [SerializeField] private string currentWeaponType;

    private TargetRangeTargetGroup activeTargetGroup;
    private TargetRangeMissionWeaponPedestal armedWeaponPedestal;

    public TargetRangeMissionData ActiveMission => activeMission;
    public TargetRangeMissionRuntimeState MissionState => missionState;

    public float RemainingTime => remainingTime;
    public int DestroyedTargetCount => destroyedTargetCount;
    public int MissionScore => missionScore;
    public int ShotsFired => shotsFired;
    public int HitsLanded => hitsLanded;

    public string CurrentWeaponId => currentWeaponId;
    public string CurrentWeaponType => currentWeaponType;

    public bool IsMissionRunning => missionState == TargetRangeMissionRuntimeState.Running;
    public bool IsMissionArmed => missionState == TargetRangeMissionRuntimeState.Armed;
    public bool IsTargetRangeTrialCompleted => targetRangeTrialCompleted;

    public bool IsWeaponSelectionLocked => IsMissionArmed || IsMissionRunning;

    public int RequiredDestroyedTargets
    {
        get
        {
            if (activeMission == null)
                return 0;

            if (activeTargetGroup != null)
                return activeTargetGroup.GetGoalTargetCount(activeMission);

            return Mathf.Max(1, activeMission.requiredDestroyedTargets);
        }
    }
    public float TimeLimitSeconds
    {
        get
        {
            if (activeMission == null)
                return 0f;

            return activeMission.timeLimitSeconds;
        }
    }

    public float AccuracyPercent
    {
        get
        {
            if (shotsFired <= 0)
                return 0f;

            return (float)hitsLanded / shotsFired * 100f;
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

        if (playerStatsController == null)
            playerStatsController = FindFirstObjectByType<PlayerStatsController>();

        if (inputReader == null)
            inputReader = FindFirstObjectByType<PlayerInputReader>();

        targetRangeTrialCompleted = MissionProgress.IsCompleted(targetRangeTrialMissionId);
    }

    private void Update()
    {
        if (targetRangeTrialCompleted &&
            allowReloadInputReturnToHubAfterTrialComplete &&
            inputReader != null &&
            inputReader.ReloadPressed)
        {
            ReturnToHub();
            return;
        }

        if (!IsMissionRunning)
            return;

        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(0f, remainingTime);

        if (remainingTime <= 0f)
        {
            FailMission();
            return;
        }

        OnMissionStateChanged?.Invoke();
    }

    public void StartMission(TargetRangeMissionData mission)
    {
        ArmMission(mission);
    }

    public void ArmMission(TargetRangeMissionData mission)
    {
        if (mission == null)
        {
            Debug.LogWarning("Tried to arm null TargetRangeMissionData.");
            return;
        }

        StopActiveMission();

        activeMission = mission;
        missionState = TargetRangeMissionRuntimeState.Armed;

        remainingTime = Mathf.Max(1f, mission.timeLimitSeconds);
        destroyedTargetCount = 0;
        missionScore = 0;
        shotsFired = 0;
        hitsLanded = 0;
        currentWeaponId = "";
        currentWeaponType = "";

        EnableMissionWeaponPedestal(mission);

        Debug.Log($"Armed target range mission: {mission.displayName}");
        Debug.Log("Pick up the mission weapon to begin.");

        OnMissionStateChanged?.Invoke();
    }

    public void StartArmedMission()
    {
        if (!IsMissionArmed || activeMission == null)
        {
            Debug.LogWarning("Tried to start mission, but no mission is armed.");
            return;
        }

        missionState = TargetRangeMissionRuntimeState.Running;

        remainingTime = Mathf.Max(1f, activeMission.timeLimitSeconds);
        destroyedTargetCount = 0;
        missionScore = 0;
        shotsFired = 0;
        hitsLanded = 0;

        activeTargetGroup = FindTargetGroup(activeMission.targetGroupId);

        if (activeTargetGroup == null)
        {
            Debug.LogWarning($"No TargetRangeTargetGroup found with id: {activeMission.targetGroupId}");
            missionState = TargetRangeMissionRuntimeState.Failed;
            OnMissionStateChanged?.Invoke();
            return;
        }

        activeTargetGroup.ActivateGroup(this, activeMission);

        Debug.Log($"Started target range mission: {activeMission.displayName}");
        Debug.Log($"Goal: Destroy {activeMission.requiredDestroyedTargets} targets in {activeMission.timeLimitSeconds:0} seconds.");

        OnMissionStateChanged?.Invoke();
    }

    public bool IsWeaponAllowedForActiveMission(WeaponData weaponData)
    {
        if (!IsWeaponSelectionLocked)
            return true;

        if (weaponData == null)
            return false;

        if (activeMission == null || activeMission.weaponReward == null)
            return false;

        return weaponData.weaponId == activeMission.weaponReward.weaponId;
    }

    public void RegisterMissionWeaponEquipped(string weaponId, string weaponType)
    {
        currentWeaponId = weaponId;
        currentWeaponType = weaponType;

        OnMissionStateChanged?.Invoke();
    }

    public void RegisterMissionShot(string weaponId, string weaponType)
    {
        if (!IsMissionRunning)
            return;

        shotsFired++;
        currentWeaponId = weaponId;
        currentWeaponType = weaponType;

        OnMissionStateChanged?.Invoke();
    }

    public void RegisterMissionHit(string weaponId, string weaponType)
    {
        if (!IsMissionRunning)
            return;

        hitsLanded++;
        currentWeaponId = weaponId;
        currentWeaponType = weaponType;

        OnMissionStateChanged?.Invoke();
    }

    public void RegisterMissionTargetDestroyed(TargetHealth targetHealth, DamageInfo damageInfo)
    {
        if (!IsMissionRunning)
            return;

        destroyedTargetCount++;

        int scoreToAdd = targetHealth != null ? targetHealth.ScoreValue : 0;
        missionScore += scoreToAdd;

        if (addMissionScoreToPlayerStats && playerStatsController != null)
            playerStatsController.AddScore(scoreToAdd);

        Debug.Log($"Mission target destroyed: {destroyedTargetCount}/{RequiredDestroyedTargets}");
        Debug.Log($"Mission score: {missionScore}");

        if (destroyedTargetCount >= RequiredDestroyedTargets)
        {
            CompleteMission();
            return;
        }

        OnMissionStateChanged?.Invoke();
    }

    public void StopActiveMission()
    {
        if (activeTargetGroup != null)
            activeTargetGroup.DeactivateGroup(false);

        if (armedWeaponPedestal != null)
            armedWeaponPedestal.Disarm();

        activeTargetGroup = null;
        armedWeaponPedestal = null;
        activeMission = null;

        missionState = TargetRangeMissionRuntimeState.None;
        remainingTime = 0f;
        destroyedTargetCount = 0;
        missionScore = 0;
        shotsFired = 0;
        hitsLanded = 0;
        currentWeaponId = "";
        currentWeaponType = "";

        OnMissionStateChanged?.Invoke();
    }

    private void CompleteMission()
    {
        if (!IsMissionRunning)
            return;

        missionState = TargetRangeMissionRuntimeState.Completed;

        if (activeMission != null)
        {
            MissionProgress.MarkCompleted(activeMission.missionId);
            RewardMissionWeapon(activeMission);
        }

        CheckTargetRangeTrialCompletion();

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();

        Debug.Log($"Target range mission completed: {activeMission.displayName}");

        if (activeTargetGroup != null)
        {
            bool preserveDestroyedState =
                activeMission != null &&
                !activeMission.respawnTargetsAfterDestroyed;

            activeTargetGroup.DeactivateGroup(preserveDestroyedState);
        }

        OnMissionStateChanged?.Invoke();
        OnMissionCompleted?.Invoke();
    }

    private void FailMission()
    {
        if (!IsMissionRunning)
            return;

        missionState = TargetRangeMissionRuntimeState.Failed;

        Debug.Log($"Target range mission failed: {activeMission.displayName}");

        if (activeTargetGroup != null)
            activeTargetGroup.DeactivateGroup(false);

        OnMissionStateChanged?.Invoke();
        OnMissionFailed?.Invoke();
    }

    private void RewardMissionWeapon(TargetRangeMissionData mission)
    {
        if (mission == null || mission.weaponReward == null)
            return;

        PlayerProgress.AddOwnedWeapon(mission.weaponReward.weaponId);
        PlayerProgress.SetActiveWeapon(mission.weaponReward.weaponId);

        Debug.Log($"Mission reward unlocked: {mission.weaponReward.displayName}");
    }

    private TargetRangeTargetGroup FindTargetGroup(string targetGroupId)
    {
        TargetRangeTargetGroup[] groups = FindObjectsByType<TargetRangeTargetGroup>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (TargetRangeTargetGroup group in groups)
        {
            if (group == null)
                continue;

            if (group.TargetGroupId == targetGroupId)
                return group;
        }

        return null;
    }

    private void EnableMissionWeaponPedestal(TargetRangeMissionData mission)
    {
        armedWeaponPedestal = null;

        TargetRangeMissionWeaponPedestal[] pedestals =
            FindObjectsByType<TargetRangeMissionWeaponPedestal>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (TargetRangeMissionWeaponPedestal pedestal in pedestals)
        {
            if (pedestal == null)
                continue;

            pedestal.Disarm();

            if (mission.weaponReward != null &&
                pedestal.WeaponData != null &&
                pedestal.WeaponData.weaponId == mission.weaponReward.weaponId)
            {
                armedWeaponPedestal = pedestal;
                pedestal.Arm(mission);
            }
        }

        if (armedWeaponPedestal == null)
            Debug.LogWarning($"No pedestal found for mission weapon: {mission.weaponReward?.weaponId}");
    }

    private void CheckTargetRangeTrialCompletion()
    {
        if (targetRangeTrialCompleted)
            return;

        if (requiredMissionsForTrialCompletion == null ||
            requiredMissionsForTrialCompletion.Length == 0)
            return;

        foreach (MissionData mission in requiredMissionsForTrialCompletion)
        {
            if (mission == null)
                continue;

            if (!MissionProgress.IsCompleted(mission.missionId))
                return;
        }

        MissionProgress.MarkCompleted(targetRangeTrialMissionId);
        targetRangeTrialCompleted = true;

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();

        Debug.Log($"Completed parent trial mission: {targetRangeTrialMissionId}");

        OnTargetTrialCompleted?.Invoke();
        OnMissionStateChanged?.Invoke();
    }

    public void ReturnToHub()
    {
        Time.timeScale = 1f;

        if (GameSceneLoader.Instance != null)
        {
            GameSceneLoader.Instance.LoadHub();
            return;
        }

        SceneManager.LoadScene(hubSceneName);
    }
}

//-----TargetRangeMissionController.cs END-----