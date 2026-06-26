//-----SaveData.cs START-----

using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string saveVersion = "0.1.0";

    public List<string> completedMissionIds = new List<string>();

    public List<WeaponTypeXpEntry> weaponTypeXpEntries = new List<WeaponTypeXpEntry>();

    public string lastSceneName = "Hub";

    public bool hasStartedGame;
}

[Serializable]
public class WeaponTypeXpEntry
{
    public string weaponType;
    public int xp;

    public WeaponTypeXpEntry(string weaponType, int xp)
    {
        this.weaponType = weaponType;
        this.xp = xp;
    }
}
//-----SaveData.cs END-----