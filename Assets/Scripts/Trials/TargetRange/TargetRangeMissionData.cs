//-----TargetRangeMissionData.cs START-----

using UnityEngine;

[CreateAssetMenu(
    fileName = "TargetRangeMissionData_NewChallenge",
    menuName = "Echo Systems Lab/Missions/Target Range Mission Data")]
public class TargetRangeMissionData : MissionData
{
    [Header("Target Range Mission")]
    public WeaponData weaponReward;
    public bool equipWeaponRewardOnStart = true;

    [Tooltip("The target group this mission should activate. Example: pistol_targets")]
    public string targetGroupId = "pistol_targets";

    [Header("Challenge Rules")]
    public float timeLimitSeconds = 60f;

    [Tooltip("Used unless Use Target Slot Count As Goal is enabled.")]
    public int requiredDestroyedTargets = 10;

    [Tooltip("How many targets from this group should be active at the same time.")]
    public int maxActiveTargets = 2;

    [Header("Target Respawn Rules")]
    [Tooltip("ON for arcade drills. OFF for destroy-them-all missions.")]
    public bool respawnTargetsAfterDestroyed = true;

    [Tooltip("If true, all target slots activate when the mission starts.")]
    public bool activateAllTargetsAtStart = false;

    [Tooltip("If true, the mission goal becomes the amount of target slots in the target group.")]
    public bool useTargetSlotCountAsGoal = false;

    [Tooltip("Only used when respawnTargetsAfterDestroyed is true.")]
    public float targetRespawnDelay = 3f;

    [Header("Destroyed Target Visuals")]
    [Tooltip("If true, the target disappears after being destroyed.")]
    public bool hideDestroyedTargetVisual = true;

    [Tooltip("If true, destroyed targets stop blocking shots after they are destroyed.")]
    public bool disableDestroyedTargetColliders = true;

    [Header("Trial Completion")]
    public bool countsTowardTargetRangeTrialCompletion = true;

    private void OnValidate()
    {
        executionMode = MissionExecutionMode.StartInCurrentScene;
        sceneName = "";

        timeLimitSeconds = Mathf.Max(1f, timeLimitSeconds);
        requiredDestroyedTargets = Mathf.Max(1, requiredDestroyedTargets);
        maxActiveTargets = Mathf.Max(1, maxActiveTargets);
        targetRespawnDelay = Mathf.Max(0f, targetRespawnDelay);

        // Important:
        // Do NOT force hideDestroyedTargetVisual here.
        // Some destroy-all missions should leave broken targets visible,
        // while others should make targets disappear permanently.
        if (!respawnTargetsAfterDestroyed)
        {
            activateAllTargetsAtStart = true;
            useTargetSlotCountAsGoal = true;
            disableDestroyedTargetColliders = true;
        }
    }

    [ContextMenu("Configure As Respawning Drill")]
    private void ConfigureAsRespawningDrill()
    {
        respawnTargetsAfterDestroyed = true;
        activateAllTargetsAtStart = false;
        useTargetSlotCountAsGoal = false;
        hideDestroyedTargetVisual = true;
        disableDestroyedTargetColliders = true;
    }

    [ContextMenu("Configure As Destroy All - Hide Destroyed")]
    private void ConfigureAsDestroyAllHideDestroyed()
    {
        respawnTargetsAfterDestroyed = false;
        activateAllTargetsAtStart = true;
        useTargetSlotCountAsGoal = true;
        hideDestroyedTargetVisual = true;
        disableDestroyedTargetColliders = true;
    }

    [ContextMenu("Configure As Destroy All - Leave Broken")]
    private void ConfigureAsDestroyAllLeaveBroken()
    {
        respawnTargetsAfterDestroyed = false;
        activateAllTargetsAtStart = true;
        useTargetSlotCountAsGoal = true;
        hideDestroyedTargetVisual = false;
        disableDestroyedTargetColliders = true;
    }
}

//-----TargetRangeMissionData.cs END-----