//-----MissionData.cs START-----

using UnityEngine;

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

    [Header("Scene")]
    public string sceneName;

    [Header("Progression")]
    public bool unlockedByDefault;
    public string[] requiredCompletedMissionIds;
}
//-----MissionData.cs END-----