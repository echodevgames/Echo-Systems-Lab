//-----WeaponData.cs START-----

using UnityEngine;

public enum WeaponFireMode
{
    Projectile,
    Hitscan
}

[CreateAssetMenu(
    fileName = "WeaponData_NewWeapon",
    menuName = "Echo Systems Lab/Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponId;

    [Header("HUD")]
    public string displayName;
    public string hudDisplayName;
    public Sprite weaponIcon;

    [TextArea(2, 4)]
    public string description;

    public string weaponType = "Pistol";


    [Header("Visuals")]
    public GameObject viewModelPrefab;
    public Vector3 viewLocalPosition;
    public Vector3 viewLocalEulerAngles;
    public Vector3 viewLocalScale = Vector3.one;

    [Header("Firing")]
    public WeaponFireMode fireMode = WeaponFireMode.Projectile;
    public AmmoData defaultAmmo;
    public float fireRate = 0.35f;
    public bool isAutomatic;

    [Header("Magazine")]
    public int clipSize = 6;
    public float reloadTime = 1.25f;
    public bool infiniteReserveAmmo = true;

    [Header("Projectile Pattern")]
    public int projectilesPerShot = 1;
    public float spreadAngle = 0f;

    [Header("Progression")]
    public int xpPerUse = 10;
}

//-----WeaponData.cs END-----