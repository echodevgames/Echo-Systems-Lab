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

    [Header("Look Sway")]
    public bool useLookSway = true;

    [Tooltip("Position sway from look input.")]
    public Vector3 lookSwayPositionAmount = new Vector3(0.015f, 0.015f, 0f);

    [Tooltip("Rotation sway from look input.")]
    public Vector3 lookSwayRotationAmount = new Vector3(1.5f, 2.5f, 1f);

    public Vector3 maxLookSwayPosition = new Vector3(0.06f, 0.06f, 0.02f);
    public Vector3 maxLookSwayRotation = new Vector3(6f, 8f, 5f);

    public float lookSwaySnappiness = 14f;
    public float lookSwayReturnSpeed = 10f;

    [Header("Movement Bob")]
    public bool useMovementBob = true;

    [Tooltip("Position bob while moving.")]
    public Vector3 movementBobPositionAmount = new Vector3(0.025f, 0.018f, 0.01f);

    [Tooltip("Rotation bob while moving.")]
    public Vector3 movementBobRotationAmount = new Vector3(1.2f, 0.8f, 1.6f);

    public float movementBobFrequency = 8f;
    public float movementBobSnappiness = 12f;
    public float movementBobReturnSpeed = 10f;
    public float movementInputThreshold = 0.1f;

    [Header("Idle Bob")]
    public bool useIdleBob = true;
    public Vector3 idleBobPositionAmount = new Vector3(0f, 0.006f, 0f);
    public Vector3 idleBobRotationAmount = new Vector3(0.25f, 0.15f, 0.15f);
    public float idleBobFrequency = 1.6f;
    public float idleBobSnappiness = 8f;

    [Header("Muzzle Flash")]
    public GameObject muzzleFlashPrefab;
    public string muzzlePointName = "MuzzlePoint";
    public Vector3 muzzleFlashLocalPositionOffset;
    public Vector3 muzzleFlashLocalEulerOffset;
    public float muzzleFlashLifetime = 0.08f;
    public bool parentMuzzleFlashToMuzzle = true;

    [Header("Reticle Recoil")]
    public bool useReticleRecoil = true;

    [Tooltip("If true, reticle kick is derived from the exact same rotation kick used by the weapon view model.")]
    public bool useWeaponKickForReticle = true;

    [Tooltip("Fallback random horizontal UI kick in pixels. Used only if shared weapon kick is unavailable or disabled.")]
    public Vector2 reticleKickXRange = new Vector2(-4f, 4f);

    [Tooltip("Fallback random vertical UI kick in pixels. Positive = up. Used only if shared weapon kick is unavailable or disabled.")]
    public Vector2 reticleKickYRange = new Vector2(6f, 12f);

    [Tooltip("How many reticle pixels each degree of weapon rotation should create.")]
    public Vector2 reticlePixelsPerWeaponRotationDegree = new Vector2(2.2f, 2.8f);

    [Tooltip("How much weapon roll affects reticle horizontal kick.")]
    public float reticleRollToHorizontalInfluence = 0.35f;

    [Tooltip("Maximum distance the reticle can be pushed away from center by firing.")]
    public float maxReticleKickOffset = 22f;

    [Tooltip("How quickly the reticle catches up to the fire kick target.")]
    public float reticleKickSnappiness = 28f;

    [Tooltip("How quickly the reticle fire kick returns to center.")]
    public float reticleReturnSpeed = 16f;

    [Header("Reticle Passive Motion")]
    public bool useReticlePassiveMotion = true;

    public bool useReticleLookSway = true;
    public Vector2 reticleLookSwayAmount = new Vector2(1.5f, 1.25f);

    public bool useReticleMovementBob = true;
    public Vector2 reticleMovementBobAmount = new Vector2(1.4f, 0.8f);
    public float reticleMovementBobFrequency = 8f;
    public float reticleMovementInputThreshold = 0.1f;

    public bool useReticleIdleBob = true;
    public Vector2 reticleIdleBobAmount = new Vector2(0f, 0.35f);
    public float reticleIdleBobFrequency = 1.6f;

    public float maxReticlePassiveOffset = 6f;
    public float reticlePassiveSnappiness = 18f;

    [Header("Reticle Pulse")]
    public bool useReticleScalePulse = true;
    public float reticleScaleKick = 0.12f;
    public float maxReticleScaleKick = 0.25f;
    public float reticleScaleSnappiness = 32f;
    public float reticleScaleReturnSpeed = 18f;

    [Header("Reload Animator Triggers")]
    public string reloadStartTriggerName = "ReloadStart";
    public string reloadInsertTriggerName = "ReloadInsert";
    public string reloadEndTriggerName = "ReloadEnd";
    public string dryFireTriggerName = "DryFire";

    [Header("Animator Triggers")]
    public bool useAnimatorTriggers = true;
    public string fireTriggerName = "Fire";
    public string reloadTriggerName = "Reload";
    public string equipTriggerName = "Equip";
}

//-----WeaponHandlingData.cs END-----