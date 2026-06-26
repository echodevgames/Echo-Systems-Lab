//-----PlayerProgress.cs START-----

using System.Collections.Generic;

public static class PlayerProgress
{
    private static readonly Dictionary<string, int> weaponTypeXp = new Dictionary<string, int>();

    public static int GetWeaponTypeXp(string weaponType)
    {
        if (string.IsNullOrWhiteSpace(weaponType))
            return 0;

        return weaponTypeXp.TryGetValue(weaponType, out int xp) ? xp : 0;
    }

    public static void AddWeaponTypeXp(string weaponType, int amount)
    {
        if (string.IsNullOrWhiteSpace(weaponType))
            return;

        if (amount <= 0)
            return;

        if (!weaponTypeXp.ContainsKey(weaponType))
            weaponTypeXp.Add(weaponType, 0);

        weaponTypeXp[weaponType] += amount;
    }

    public static void LoadFromSaveData(SaveData saveData)
    {
        weaponTypeXp.Clear();

        if (saveData == null || saveData.weaponTypeXpEntries == null)
            return;

        foreach (WeaponTypeXpEntry entry in saveData.weaponTypeXpEntries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.weaponType))
                continue;

            weaponTypeXp[entry.weaponType] = entry.xp;
        }
    }

    public static void WriteToSaveData(SaveData saveData)
    {
        if (saveData == null)
            return;

        saveData.weaponTypeXpEntries.Clear();

        foreach (KeyValuePair<string, int> pair in weaponTypeXp)
            saveData.weaponTypeXpEntries.Add(new WeaponTypeXpEntry(pair.Key, pair.Value));
    }

    public static void ResetProgress()
    {
        weaponTypeXp.Clear();
    }
}
//-----PlayerProgress.cs END-----