//-----WeaponHandlingData.cs START-----

using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponHandlingData_NewWeapon",
    menuName = "Echo Systems Lab/Weapons/Weapon Handling Data")]
public class WeaponHandlingData : ScriptableObject
{
    [Header("Fire Kickback")]
    public Vector3 firePositionKick = new Vector3(0f, -0.015f, -0.08f);
    public Vector3 fireRotationKick = new Vector3(-4f, 0f, 0f);

    [Header("Fire Randomness")]
    public Vector3 randomRotationKick = new Vector3(1f, 0.75f, 1.25f);

    [Header("Limits")]
    public Vector3 maxPositionOffset = new Vector3(0.08f, 0.08f, 0.18f);
    public Vector3 maxRotationOffset = new Vector3(12f, 6f, 8f);

    [Header("Recovery")]
    public float positionSnappiness = 24f;
    public float rotationSnappiness = 24f;
    public float positionReturnSpeed = 12f;
    public float rotationReturnSpeed = 12f;

    [Header("Muzzle Flash")]
    public GameObject muzzleFlashPrefab;
    public string muzzlePointName = "MuzzlePoint";
    public Vector3 muzzleFlashLocalPositionOffset;
    public Vector3 muzzleFlashLocalEulerOffset;
    public float muzzleFlashLifetime = 0.08f;
    public bool parentMuzzleFlashToMuzzle = true;

    [Header("Fire Audio")]
    public AudioClip[] fireAudioClips;
    [Range(0f, 1f)] public float fireAudioVolume = 1f;
    public Vector2 fireAudioPitchRange = new Vector2(0.96f, 1.04f);

    [Header("Reticle Recoil")]
    public bool useReticleRecoil = true;

    [Tooltip("Horizontal UI kick in pixels. Negative = left, positive = right.")]
    public Vector2 reticleKickXRange = new Vector2(-4f, 4f);

    [Tooltip("Vertical UI kick in pixels. Positive = up.")]
    public Vector2 reticleKickYRange = new Vector2(6f, 12f);

    [Tooltip("Maximum distance the reticle can be pushed away from center.")]
    public float maxReticleKickOffset = 22f;

    [Tooltip("How quickly the reticle catches up to the kick target.")]
    public float reticleKickSnappiness = 28f;

    [Tooltip("How quickly the reticle returns to center.")]
    public float reticleReturnSpeed = 16f;

    [Header("Reticle Pulse")]
    public bool useReticleScalePulse = true;
    public float reticleScaleKick = 0.12f;
    public float maxReticleScaleKick = 0.25f;
    public float reticleScaleSnappiness = 32f;
    public float reticleScaleReturnSpeed = 18f;

    [Header("Animator Triggers")]
    public bool useAnimatorTriggers = true;
    public string fireTriggerName = "Fire";
    public string reloadTriggerName = "Reload";
    public string equipTriggerName = "Equip";
}

//-----WeaponHandlingData.cs END-----