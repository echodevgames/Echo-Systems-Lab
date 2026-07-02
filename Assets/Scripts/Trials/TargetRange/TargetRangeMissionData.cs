//-----TargetRangeMissionData.cs START-----

using UnityEngine;

[CreateAssetMenu(
    fileName = "TargetRangeMissionData_NewChallenge",
    menuName = "Echo Systems Lab/Missions/Target Range Mission Data")]
public class TargetRangeMissionData : MissionData
{
    [Header("Target Range Mission")]
    public WeaponData weaponReward;

    [Tooltip("The target group this mission should activate. Example: pistol_targets")]
    public string targetGroupId = "pistol_targets";

    [Header("Challenge Rules")]
    public float timeLimitSeconds = 60f;
    public int requiredDestroyedTargets = 10;
    public float targetRespawnDelay = 3f;

    [Header("Trial Completion")]
    public bool countsTowardTargetRangeTrialCompletion = true;

    private void OnValidate()
    {
        executionMode = MissionExecutionMode.StartInCurrentScene;
        sceneName = "";
    }
}

//-----TargetRangeMissionData.cs END-----