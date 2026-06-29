//-----AmmoData.cs START-----

using UnityEngine;

[CreateAssetMenu(
    fileName = "AmmoData_NewAmmo",
    menuName = "Echo Systems Lab/Weapons/Ammo Data")]
public class AmmoData : ScriptableObject
{
    [Header("Identity")]
    public string ammoId;

    [Header("HUD")]
    public string displayName;
    public Sprite ammoIcon;
    public Color hudColor = Color.white;//I might not use this on the end... (mostly dark BGs)
    public string caliberLabel; // I'm thinking about using an icon here too of the profile of each ammo...

    [TextArea(2, 4)]
    public string description;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public int damage = 10;
    public float projectileSpeed = 30f;
    public float projectileLifetime = 10f;

    [Header("Progression")]
    public int xpPerUse = 10;
}

//-----AmmoData.cs END-----