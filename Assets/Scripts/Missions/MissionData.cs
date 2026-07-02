//-----MissionData.cs START-----

using UnityEngine;

public enum MissionExecutionMode
{
    LoadScene,
    StartInCurrentScene
}

[CreateAssetMenu(
    fileName = "MissionData_NewMission",
    menuName = "Echo Systems Lab/Missions/Mission Data")]
public class MissionData : ScriptableObject
{
    [Header("Identity")]
    public string missionId;
    public string displayName;

    [TextArea(3, 6)]
    public string description;

    [Header("Execution")]
    public MissionExecutionMode executionMode = MissionExecutionMode.LoadScene;

    [Header("Scene")]
    public string sceneName;

    [Header("Progression")]
    public bool unlockedByDefault;
    public string[] requiredCompletedMissionIds;
}

//-----MissionData.cs END-----