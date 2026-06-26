//-----PlayerProgress.cs START-----

using System.Collections.Generic;

public static class PlayerProgress
{
    private static readonly Dictionary<string, int> weaponTypeXp = new Dictionary<string, int>();
    private static readonly HashSet<string> ownedWeaponIds = new HashSet<string>();

    private static string activeWeaponId;

    public static string ActiveWeaponId => activeWeaponId;

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

    public static bool OwnsWeapon(string weaponId)
    {
        return !string.IsNullOrWhiteSpace(weaponId) &&
               ownedWeaponIds.Contains(weaponId);
    }

    public static void AddOwnedWeapon(string weaponId)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
            return;

        ownedWeaponIds.Add(weaponId);
    }

    public static void SetActiveWeapon(string weaponId)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
            return;

        AddOwnedWeapon(weaponId);
        activeWeaponId = weaponId;
    }

    public static List<string> GetOwnedWeaponIds()
    {
        return new List<string>(ownedWeaponIds);
    }

    public static void LoadFromSaveData(SaveData saveData)
    {
        weaponTypeXp.Clear();
        ownedWeaponIds.Clear();
        activeWeaponId = null;

        if (saveData == null)
            return;

        if (saveData.weaponTypeXpEntries != null)
        {
            foreach (WeaponTypeXpEntry entry in saveData.weaponTypeXpEntries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.weaponType))
                    continue;

                weaponTypeXp[entry.weaponType] = entry.xp;
            }
        }

        if (saveData.ownedWeaponIds != null)
        {
            foreach (string weaponId in saveData.ownedWeaponIds)
            {
                if (!string.IsNullOrWhiteSpace(weaponId))
                    ownedWeaponIds.Add(weaponId);
            }
        }

        activeWeaponId = saveData.activeWeaponId;
    }

    public static void WriteToSaveData(SaveData saveData)
    {
        if (saveData == null)
            return;

        saveData.weaponTypeXpEntries.Clear();

        foreach (KeyValuePair<string, int> pair in weaponTypeXp)
            saveData.weaponTypeXpEntries.Add(new WeaponTypeXpEntry(pair.Key, pair.Value));

        saveData.ownedWeaponIds = GetOwnedWeaponIds();
        saveData.activeWeaponId = activeWeaponId;
    }

    public static void ResetProgress()
    {
        weaponTypeXp.Clear();
        ownedWeaponIds.Clear();
        activeWeaponId = null;
    }
}
//-----PlayerProgress.cs END-----