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
    public string displayName;
    public string weaponType = "Pistol";

    [TextArea(2, 4)]
    public string description;

    [Header("Visuals")]
    public GameObject viewModelPrefab;
    public Vector3 viewLocalPosition;
    public Vector3 viewLocalEulerAngles;
    public Vector3 viewLocalScale = Vector3.one;

    [Header("Firing")]
    public WeaponFireMode fireMode = WeaponFireMode.Projectile;
    public GameObject projectilePrefab;
    public int damage = 1;
    public float fireRate = 0.35f;
    public float projectileSpeed = 30f;
    public float projectileLifetime = 4f;

    [Header("Progression")]
    public int xpPerHit = 1;
    public int xpPerTargetDestroyed = 5;
}

//-----WeaponData.cs END-----