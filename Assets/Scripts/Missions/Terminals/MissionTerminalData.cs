//-----MissionTerminalData.cs START-----

using UnityEngine;

[CreateAssetMenu(
    fileName = "MissionTerminalData_NewTerminal",
    menuName = "Echo Systems Lab/Missions/Mission Terminal Data")]
public class MissionTerminalData : ScriptableObject
{
    [Header("Identity")]
    public string terminalId;
    public string displayName = "Mission Terminal";

    [TextArea(2, 5)]
    public string description;

    [Header("Prompt")]
    public string promptText = "Press E to open Mission Terminal";

    [Header("Missions")]
    public MissionData[] missions;
}

//-----MissionTerminalData.cs END-----