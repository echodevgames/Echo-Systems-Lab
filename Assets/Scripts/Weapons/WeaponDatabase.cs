//-----WeaponDatabase.cs START-----

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponDatabase",
    menuName = "Echo Systems Lab/Weapons/Weapon Database")]
public class WeaponDatabase : ScriptableObject
{
    [SerializeField] private WeaponData[] weapons;

    public WeaponData GetWeaponById(string weaponId)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
            return null;

        foreach (WeaponData weapon in weapons)
        {
            if (weapon == null)
                continue;

            if (weapon.weaponId == weaponId)
                return weapon;
        }

        return null;
    }

    public List<WeaponData> GetOwnedWeaponsInDatabaseOrder()
    {
        List<WeaponData> ownedWeapons = new List<WeaponData>();

        foreach (WeaponData weapon in weapons)
        {
            if (weapon == null)
                continue;

            if (PlayerProgress.OwnsWeapon(weapon.weaponId))
                ownedWeapons.Add(weapon);
        }

        return ownedWeapons;
    }
}

//-----WeaponDatabase.cs END-----