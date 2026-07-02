//-----WeaponPickupData.cs START-----
using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponPickupData_NewWeapon",
    menuName = "Echo Systems Lab/Pickups/Weapon Pickup Data")]
public class WeaponPickupData : ScriptableObject
{
    [Header("Weapon")]
    public WeaponData weaponData;

    [Header("Pickup Display")]
    public string displayName;
    public GameObject worldVisualPrefab;

    [Header("Pickup Behavior")]
    public float pickupImpulse = 2f;
    public bool destroyOnPickup = true;
    public bool saveOnPickup = true;

    public string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(displayName))
            return displayName;

        if (weaponData != null)
            return weaponData.name;

        return "Weapon";
    }
}
//-----WeaponPickupData.cs END-----