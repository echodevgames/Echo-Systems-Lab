//-----AmmoPickupData.cs START-----

using UnityEngine;

[CreateAssetMenu(
    fileName = "AmmoPickupData_NewPickup",
    menuName = "Echo Systems Lab/Weapons/Ammo Pickup Data")]
public class AmmoPickupData : ScriptableObject
{
    [Header("Identity")]
    public string pickupId;
    public string displayName;

    [TextArea(2, 4)]
    public string description;

    [Header("Ammo")]
    public AmmoData ammoData;
    public int amount = 30;

    [Header("Pickup Behavior")]
    public bool destroyOnPickup = true;
}

//-----AmmoPickupData.cs END-----