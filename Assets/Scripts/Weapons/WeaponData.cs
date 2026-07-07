//-----WeaponData.cs START-----

using UnityEngine;

public enum WeaponFireMode
{
    Projectile,
    Hitscan
}
public enum WeaponReloadMode
{
    FullClip,
    OneRoundAtATime
}
public enum EmptyFireBehavior
{
    DryFireOnly,
    ReloadOnly,
    DryFireThenReload
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

    [Header("Handling")]
    public WeaponHandlingData handlingData;

    [Header("Audio")]
    public WeaponAudioData audioData;

    [Header("Firing")]
    public WeaponFireMode fireMode = WeaponFireMode.Projectile;
    public AmmoData defaultAmmo;
    public float fireRate = 0.35f;
    public bool isAutomatic;

    [Header("Magazine")]
    public int clipSize = 6;
    public float reloadTime = 1.25f;
    public bool infiniteReserveAmmo = true;

    [Header("Reload Behavior")]
    public WeaponReloadMode reloadMode = WeaponReloadMode.FullClip;

    [Tooltip("If true, the player can fire before a reload has fully completed.")]
    public bool canFireDuringReload = false;

    [Tooltip("If true, firing during reload stops the reload process.")]
    public bool cancelReloadOnFire = true;

    [Tooltip("Only used for one-round-at-a-time reload weapons.")]
    public float reloadStartTime = 0.25f;

    [Tooltip("Only used for one-round-at-a-time reload weapons.")]
    public float reloadRoundInsertTime = 0.45f;

    [Tooltip("Only used for one-round-at-a-time reload weapons.")]
    public float reloadEndTime = 0.25f;

    [Header("Reload Feedback Stages")]
    [Tooltip("Plays reload start audio/trigger when reload begins.")]
    public bool playReloadStartFeedback = true;

    [Tooltip("Plays reload insert audio/trigger. For FullClip reloads, this plays once at Full Reload Insert Feedback Time. For OneRoundAtATime reloads, this plays once per inserted round.")]
    public bool playReloadInsertFeedback = false;

    [Tooltip("Plays reload end audio/trigger near the end of reload.")]
    public bool playReloadEndFeedback = false;

    [Tooltip("Only used for FullClip reloads. Time after reload starts before insert feedback plays.")]
    public float fullReloadInsertFeedbackTime = 0.5f;

    [Tooltip("Only used for FullClip reloads. How early before reload completion the end feedback should play.")]
    public float fullReloadEndFeedbackLeadTime = 0.05f;

    [Header("Dry Fire")]
    public bool canDryFire = true;
    public float dryFireCooldown = 0.2f;

    [Header("Empty Fire Behavior")]
    [Tooltip("What happens when the player presses fire while the weapon is empty.")]
    public EmptyFireBehavior emptyFireBehavior = EmptyFireBehavior.DryFireOnly;

    [Tooltip("Only used by DryFireThenReload. Adds a small delay after the dry fire click before reload starts.")]
    public float emptyFireReloadDelay = 0.12f;

    [Header("Projectile Pattern")]
    public int projectilesPerShot = 1;
    public float spreadAngle = 0f;

    [Header("Progression")]
    public int xpPerUse = 10;
}

//-----WeaponData.cs END-----