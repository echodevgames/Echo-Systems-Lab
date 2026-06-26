//-----MissionProgress.cs START-----

using System.Collections.Generic;

public static class MissionProgress
{
    private static readonly HashSet<string> completedMissionIds = new HashSet<string>();

    public static bool IsCompleted(string missionId)
    {
        return !string.IsNullOrWhiteSpace(missionId) &&
               completedMissionIds.Contains(missionId);
    }

    public static void MarkCompleted(string missionId)
    {
        if (!string.IsNullOrWhiteSpace(missionId))
            completedMissionIds.Add(missionId);
    }

    public static bool IsUnlocked(MissionData mission)
    {
        if (mission == null)
            return false;

        if (mission.unlockedByDefault)
            return true;

        if (mission.requiredCompletedMissionIds == null || mission.requiredCompletedMissionIds.Length == 0)
            return false;

        foreach (string requiredId in mission.requiredCompletedMissionIds)
        {
            if (!completedMissionIds.Contains(requiredId))
                return false;
        }

        return true;
    }

    public static List<string> GetCompletedMissionIds()
    {
        return new List<string>(completedMissionIds);
    }

    public static void LoadFromSaveData(SaveData saveData)
    {
        completedMissionIds.Clear();

        if (saveData == null || saveData.completedMissionIds == null)
            return;

        foreach (string missionId in saveData.completedMissionIds)
        {
            if (!string.IsNullOrWhiteSpace(missionId))
                completedMissionIds.Add(missionId);
        }
    }

    public static void WriteToSaveData(SaveData saveData)
    {
        if (saveData == null)
            return;

        saveData.completedMissionIds = GetCompletedMissionIds();
    }

    public static void ResetProgress()
    {
        completedMissionIds.Clear();
    }
}
//-----MissionProgress.cs END-----